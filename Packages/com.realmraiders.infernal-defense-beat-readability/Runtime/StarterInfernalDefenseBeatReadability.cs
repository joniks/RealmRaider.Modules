using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefensePacing;

namespace RealmRaiders.Modules.InfernalDefenseBeatReadability
{
    public static class InfernalDefenseBeatReadabilityValidator
    {
        private static readonly IReadOnlyList<string> ForbiddenPromiseTokens =
            Array.AsReadOnly(new[]
            {
                "ACTIVATE",
                "ACTIVATED",
                "ACTIVE",
                "AUTO",
                "AUTOMATIC",
                "AUTOMATICALLY",
                "AVAILABLE",
                "COMPLETE",
                "COMPLETED",
                "EARNED",
                "EXPOSE",
                "EXPOSED",
                "GRANTED",
                "GUARANTEED",
                "OPEN",
                "OPENED",
                "POSSESSED",
                "REWARD",
                "REWARDS",
                "SUCCEEDED",
                "SUCCESS",
                "UNLOCK",
                "UNLOCKED"
            });

        public static InfernalDefenseBeatReadabilityValidationResult Validate(
            InfernalDefenseBeatReadabilityProfile profile)
        {
            var issues = new List<InfernalDefenseBeatReadabilityValidationIssue>();
            if (profile == null)
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityValidationIssue.ProfileMissing);
                return new InfernalDefenseBeatReadabilityValidationResult(issues);
            }

            var recipe = ResolveRecipe(profile.LayoutId, issues);
            if (recipe != null && profile.Facts.Count != recipe.Beats.Count)
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityValidationIssue.FactCardinalityInvalid);
            }

            var recipeBeats = CreateRecipeBeatIndex(recipe);
            var seenBeatIds = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < profile.Facts.Count; index++)
            {
                var fact = profile.Facts[index];
                if (fact == null)
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue.FactMissing);
                    continue;
                }

                ValidateCopy(fact, issues);
                ValidateEmphasis(fact, issues);

                var beatIdIsValid = HasStableId(fact.BeatId);
                if (!beatIdIsValid)
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue.BeatIdInvalid);
                }
                else if (!seenBeatIds.Add(fact.BeatId))
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue.BeatIdDuplicate);
                }

                if (recipe == null || !beatIdIsValid)
                {
                    continue;
                }

                if (!recipeBeats.TryGetValue(fact.BeatId, out var expectedBeat))
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue.BeatUnknown);
                    continue;
                }

                if (index >= recipe.Beats.Count
                    || !string.Equals(
                        recipe.Beats[index].BeatId,
                        fact.BeatId,
                        StringComparison.Ordinal))
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue.BeatOrderMismatch);
                }

                if (!string.Equals(fact.RoleId, expectedBeat.RoleId, StringComparison.Ordinal))
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue.BeatRoleMismatch);
                }

                if (fact.Kind != expectedBeat.Kind)
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue.BeatKindMismatch);
                }

                if (fact.Requirement != expectedBeat.Requirement)
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue
                            .BeatRequirementMismatch);
                }

                ValidateSemanticCopy(fact, expectedBeat, issues);
            }

            return new InfernalDefenseBeatReadabilityValidationResult(issues);
        }

        public static InfernalDefenseBeatReadabilityCatalogueValidationResult
            ValidateCatalogue(IReadOnlyList<InfernalDefenseBeatReadabilityProfile> profiles)
        {
            var issues = new List<InfernalDefenseBeatReadabilityCatalogueIssue>();
            if (profiles == null)
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityCatalogueIssue.CatalogueMissing);
                return new InfernalDefenseBeatReadabilityCatalogueValidationResult(issues);
            }

            if (profiles.Count != StarterInfernalDefensePacing.All.Count)
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityCatalogueIssue.ProfileCardinalityInvalid);
            }

            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            var signatures = new HashSet<string>(StringComparer.Ordinal);
            foreach (var profile in profiles)
            {
                if (profile == null)
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityCatalogueIssue.ProfileMissing);
                    continue;
                }

                if (!layoutIds.Add(profile.LayoutId))
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityCatalogueIssue.LayoutIdDuplicate);
                }

                var validation = Validate(profile);
                if (!validation.IsValid)
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityCatalogueIssue.ProfileInvalid);
                    continue;
                }

                var signature = CreateSemanticSignature(profile);
                if (!signatures.Add(signature))
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityCatalogueIssue
                            .SemanticSignatureDuplicate);
                }
            }

            foreach (var recipe in StarterInfernalDefensePacing.All)
            {
                if (!layoutIds.Contains(recipe.LayoutId))
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityCatalogueIssue.LayoutCoverageInvalid);
                }
            }

            return new InfernalDefenseBeatReadabilityCatalogueValidationResult(issues);
        }

        /// <summary>
        /// Copy and closed semantics only. Layout, beat and role IDs are excluded,
        /// so renamed duplicate readability cannot masquerade as distinct.
        /// </summary>
        public static string CreateSemanticSignature(
            InfernalDefenseBeatReadabilityProfile profile)
        {
            if (profile == null || !Validate(profile).IsValid)
            {
                return null;
            }

            var parts = new string[profile.Facts.Count];
            for (var index = 0; index < profile.Facts.Count; index++)
            {
                var fact = profile.Facts[index];
                parts[index] = ((int)fact.Kind).ToString()
                    + ":"
                    + ((int)fact.Requirement).ToString()
                    + ":"
                    + ((int)fact.Emphasis).ToString()
                    + ":"
                    + fact.PrimaryCue
                    + ":"
                    + (fact.TacticalHint ?? string.Empty);
            }

            return string.Join("|", parts);
        }

        private static InfernalDefensePacingRecipe ResolveRecipe(
            string layoutId,
            ICollection<InfernalDefenseBeatReadabilityValidationIssue> issues)
        {
            if (!HasStableId(layoutId))
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityValidationIssue.LayoutIdInvalid);
                return null;
            }

            var lookup = StarterInfernalDefensePacingResolver.ResolveExact(layoutId);
            if (!lookup.Found)
            {
                AddIssue(
                    issues,
                    lookup.Status == InfernalDefensePacingLookupStatus.LayoutIdInvalid
                        ? InfernalDefenseBeatReadabilityValidationIssue.LayoutIdInvalid
                        : InfernalDefenseBeatReadabilityValidationIssue.LayoutNotFound);
                return null;
            }

            if (!InfernalDefensePacingValidator.Validate(lookup.Recipe).IsValid)
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityValidationIssue.RecipeInvalid);
                return null;
            }

            return lookup.Recipe;
        }

        private static IReadOnlyDictionary<string, InfernalDefensePacingBeat>
            CreateRecipeBeatIndex(InfernalDefensePacingRecipe recipe)
        {
            var result = new Dictionary<string, InfernalDefensePacingBeat>(
                StringComparer.Ordinal);
            if (recipe != null)
            {
                foreach (var beat in recipe.Beats)
                {
                    result.Add(beat.BeatId, beat);
                }
            }

            return result;
        }

        private static void ValidateCopy(
            InfernalDefenseBeatReadabilityFact fact,
            ICollection<InfernalDefenseBeatReadabilityValidationIssue> issues)
        {
            if (string.IsNullOrEmpty(fact.PrimaryCue))
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityValidationIssue.PrimaryCueMissing);
            }
            else
            {
                if (fact.PrimaryCue.Length
                        < InfernalDefenseBeatReadabilityBounds.MinimumPrimaryCueCharacters
                    || fact.PrimaryCue.Length
                        > InfernalDefenseBeatReadabilityBounds.MaximumPrimaryCueCharacters)
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue
                            .PrimaryCueLengthInvalid);
                }

                ValidateMobileCopy(fact.PrimaryCue, issues);
            }

            if (!string.IsNullOrEmpty(fact.TacticalHint))
            {
                if (fact.TacticalHint.Length
                        < InfernalDefenseBeatReadabilityBounds.MinimumTacticalHintCharacters
                    || fact.TacticalHint.Length
                        > InfernalDefenseBeatReadabilityBounds.MaximumTacticalHintCharacters)
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue
                            .TacticalHintLengthInvalid);
                }

                ValidateMobileCopy(fact.TacticalHint, issues);
            }

            var combined = (fact.PrimaryCue ?? string.Empty)
                + " "
                + (fact.TacticalHint ?? string.Empty);
            foreach (var token in ForbiddenPromiseTokens)
            {
                if (ContainsWord(combined, token))
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue.PromiseCopyInvalid);
                }
            }
        }

        private static void ValidateMobileCopy(
            string value,
            ICollection<InfernalDefenseBeatReadabilityValidationIssue> issues)
        {
            if (value[0] == ' '
                || value[value.Length - 1] == ' '
                || value.Contains("  "))
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityValidationIssue.CopySpacingInvalid);
            }

            foreach (var symbol in value)
            {
                if (!(symbol >= 'A' && symbol <= 'Z')
                    && !(symbol >= '0' && symbol <= '9')
                    && symbol != ' '
                    && symbol != '-'
                    && symbol != '\'')
                {
                    AddIssue(
                        issues,
                        InfernalDefenseBeatReadabilityValidationIssue.CopyCharacterInvalid);
                    return;
                }
            }
        }

        private static void ValidateEmphasis(
            InfernalDefenseBeatReadabilityFact fact,
            ICollection<InfernalDefenseBeatReadabilityValidationIssue> issues)
        {
            if (!IsClosedEmphasis(fact.Emphasis))
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityValidationIssue.EmphasisInvalid);
                return;
            }

            if (fact.Emphasis != ExpectedEmphasis(fact.Kind))
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityValidationIssue.EmphasisRoleMismatch);
            }
        }

        private static void ValidateSemanticCopy(
            InfernalDefenseBeatReadabilityFact fact,
            InfernalDefensePacingBeat expectedBeat,
            ICollection<InfernalDefenseBeatReadabilityValidationIssue> issues)
        {
            if (!IsClosedPrimaryCue(
                    fact.PrimaryCue,
                    expectedBeat.Kind,
                    expectedBeat.Requirement)
                || fact.HasTacticalHint
                && !IsClosedTacticalHint(
                    fact.TacticalHint,
                    expectedBeat.Kind,
                    expectedBeat.Requirement))
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatReadabilityValidationIssue.CopySemanticMismatch);
            }
        }

        private static bool IsClosedPrimaryCue(
            string primaryCue,
            InfernalDefensePacingBeatKind kind,
            InfernalDefensePacingRequirement requirement)
        {
            switch (kind)
            {
                case InfernalDefensePacingBeatKind.InvaderEntry:
                    return requirement == InfernalDefensePacingRequirement.Required
                        && IsOneOf(
                            primaryCue,
                            "INVADERS AT ASHEN ENTRY",
                            "INVADERS AT CINDER ENTRY",
                            "INVADERS AT EMBER ENTRY");
                case InfernalDefensePacingBeatKind.HellhoundPressure:
                    return requirement == InfernalDefensePacingRequirement.Optional
                            && string.Equals(
                                primaryCue,
                                "HELLHOUND SPUR PRESSURE",
                                StringComparison.Ordinal)
                        || requirement == InfernalDefensePacingRequirement.Required
                            && IsOneOf(
                                primaryCue,
                                "HELLHOUND ROUTE PRESSURE",
                                "HELLHOUND PRESSURE");
                case InfernalDefensePacingBeatKind.FlameTrapOpportunity:
                    return requirement == InfernalDefensePacingRequirement.Optional
                            && IsOneOf(
                                primaryCue,
                                "OPTIONAL FLAME ROUTE",
                                "OPTIONAL FLAME DETOUR")
                        || requirement == InfernalDefensePacingRequirement.Required
                            && string.Equals(
                                primaryCue,
                                "FLAME ROUTE PRESSURE",
                                StringComparison.Ordinal);
                case InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow:
                    return requirement == InfernalDefensePacingRequirement.Required
                        && string.Equals(
                            primaryCue,
                            "BRUTE POSSESSION WINDOW",
                            StringComparison.Ordinal);
                case InfernalDefensePacingBeatKind.InfernalHeartObjective:
                    return requirement == InfernalDefensePacingRequirement.Required
                        && string.Equals(
                            primaryCue,
                            "INFERNAL HEART OBJECTIVE",
                            StringComparison.Ordinal);
                default:
                    return false;
            }
        }

        private static bool IsClosedTacticalHint(
            string tacticalHint,
            InfernalDefensePacingBeatKind kind,
            InfernalDefensePacingRequirement requirement)
        {
            switch (kind)
            {
                case InfernalDefensePacingBeatKind.InvaderEntry:
                    return requirement == InfernalDefensePacingRequirement.Required
                        && IsOneOf(
                            tacticalHint,
                            "MAIN ROUTE PRESSURE BEGINS",
                            "PRESSURE SPLITS AFTER ENTRY",
                            "PRESSURE BUILDS IN SEQUENCE");
                case InfernalDefensePacingBeatKind.HellhoundPressure:
                    return requirement == InfernalDefensePacingRequirement.Optional
                            && string.Equals(
                                tacticalHint,
                                "OPTIONAL SPUR - NOT REQUIRED FOR PROGRESS",
                                StringComparison.Ordinal)
                        || requirement == InfernalDefensePacingRequirement.Required
                            && IsOneOf(
                                tacticalHint,
                                "REQUIRED ROUTE BEAT BEFORE THE BRUTE",
                                "REQUIRED BEFORE BOTH FORWARD ROUTES");
                case InfernalDefensePacingBeatKind.FlameTrapOpportunity:
                    return requirement == InfernalDefensePacingRequirement.Optional
                            && string.Equals(
                                tacticalHint,
                                "SKIP WITHOUT BLOCKING THE BRUTE",
                                StringComparison.Ordinal)
                        || requirement == InfernalDefensePacingRequirement.Required
                            && string.Equals(
                                tacticalHint,
                                "REQUIRED ROUTE BEAT BEFORE THE BRUTE",
                                StringComparison.Ordinal);
                case InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow:
                    return requirement == InfernalDefensePacingRequirement.Required
                        && IsOneOf(
                            tacticalHint,
                            "CHOOSE POSSESS OR KEEP DEFENDING",
                            "BRUTE WINDOW FOLLOWS HELLHOUND PRESSURE",
                            "CHOOSE AFTER THE HELLHOUND ROUTE");
                case InfernalDefensePacingBeatKind.InfernalHeartObjective:
                    return requirement == InfernalDefensePacingRequirement.Required
                        && string.Equals(
                            tacticalHint,
                            "FINAL OBJECTIVE PRESSURE",
                            StringComparison.Ordinal);
                default:
                    return false;
            }
        }

        private static bool IsOneOf(string value, params string[] allowedValues)
        {
            foreach (var allowedValue in allowedValues)
            {
                if (string.Equals(value, allowedValue, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static InfernalDefenseBeatEmphasis ExpectedEmphasis(
            InfernalDefensePacingBeatKind kind)
        {
            switch (kind)
            {
                case InfernalDefensePacingBeatKind.InvaderEntry:
                    return InfernalDefenseBeatEmphasis.Arrival;
                case InfernalDefensePacingBeatKind.HellhoundPressure:
                    return InfernalDefenseBeatEmphasis.Pressure;
                case InfernalDefensePacingBeatKind.FlameTrapOpportunity:
                    return InfernalDefenseBeatEmphasis.Hazard;
                case InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow:
                    return InfernalDefenseBeatEmphasis.Possession;
                case InfernalDefensePacingBeatKind.InfernalHeartObjective:
                    return InfernalDefenseBeatEmphasis.Objective;
                default:
                    return (InfernalDefenseBeatEmphasis)(-1);
            }
        }

        private static bool IsClosedEmphasis(InfernalDefenseBeatEmphasis emphasis)
        {
            return emphasis == InfernalDefenseBeatEmphasis.Arrival
                || emphasis == InfernalDefenseBeatEmphasis.Pressure
                || emphasis == InfernalDefenseBeatEmphasis.Hazard
                || emphasis == InfernalDefenseBeatEmphasis.Possession
                || emphasis == InfernalDefenseBeatEmphasis.Objective;
        }

        private static bool ContainsWord(string value, string word)
        {
            var searchStart = 0;
            while (searchStart <= value.Length - word.Length)
            {
                var index = value.IndexOf(word, searchStart, StringComparison.Ordinal);
                if (index < 0)
                {
                    return false;
                }

                var beginsAtBoundary = index == 0 || !IsAsciiUpperAlphaNumeric(value[index - 1]);
                var end = index + word.Length;
                var endsAtBoundary = end == value.Length
                    || !IsAsciiUpperAlphaNumeric(value[end]);
                if (beginsAtBoundary && endsAtBoundary)
                {
                    return true;
                }

                searchStart = index + 1;
            }

            return false;
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || !IsAsciiLowerAlphaNumeric(value[0])
                || !IsAsciiLowerAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            for (var index = 1; index < value.Length - 1; index++)
            {
                var symbol = value[index];
                if (!IsAsciiLowerAlphaNumeric(symbol)
                    && symbol != '.'
                    && symbol != '_'
                    && symbol != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiLowerAlphaNumeric(char value)
        {
            return value >= 'a' && value <= 'z'
                || value >= '0' && value <= '9';
        }

        private static bool IsAsciiUpperAlphaNumeric(char value)
        {
            return value >= 'A' && value <= 'Z'
                || value >= '0' && value <= '9';
        }

        private static void AddIssue(
            ICollection<InfernalDefenseBeatReadabilityValidationIssue> issues,
            InfernalDefenseBeatReadabilityValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private static void AddIssue(
            ICollection<InfernalDefenseBeatReadabilityCatalogueIssue> issues,
            InfernalDefenseBeatReadabilityCatalogueIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }

    public static class StarterInfernalDefenseBeatReadability
    {
        public static InfernalDefenseBeatReadabilityProfile AshenSpur { get; } =
            Create(
                StarterInfernalDefensePacing.AshenSpur,
                Copy("INVADERS AT ASHEN ENTRY", "MAIN ROUTE PRESSURE BEGINS"),
                Copy("HELLHOUND SPUR PRESSURE",
                    "OPTIONAL SPUR - NOT REQUIRED FOR PROGRESS"),
                Copy("FLAME ROUTE PRESSURE",
                    "REQUIRED ROUTE BEAT BEFORE THE BRUTE"),
                Copy("BRUTE POSSESSION WINDOW",
                    "CHOOSE POSSESS OR KEEP DEFENDING"),
                Copy("INFERNAL HEART OBJECTIVE", "FINAL OBJECTIVE PRESSURE"));

        public static InfernalDefenseBeatReadabilityProfile CinderFork { get; } =
            Create(
                StarterInfernalDefensePacing.CinderFork,
                Copy("INVADERS AT CINDER ENTRY", "PRESSURE SPLITS AFTER ENTRY"),
                Copy("HELLHOUND ROUTE PRESSURE",
                    "REQUIRED ROUTE BEAT BEFORE THE BRUTE"),
                Copy("OPTIONAL FLAME ROUTE", "SKIP WITHOUT BLOCKING THE BRUTE"),
                Copy("BRUTE POSSESSION WINDOW",
                    "BRUTE WINDOW FOLLOWS HELLHOUND PRESSURE"),
                Copy("INFERNAL HEART OBJECTIVE", "FINAL OBJECTIVE PRESSURE"));

        public static InfernalDefenseBeatReadabilityProfile EmberCircuit { get; } =
            Create(
                StarterInfernalDefensePacing.EmberCircuit,
                Copy("INVADERS AT EMBER ENTRY", "PRESSURE BUILDS IN SEQUENCE"),
                Copy("HELLHOUND PRESSURE", "REQUIRED BEFORE BOTH FORWARD ROUTES"),
                Copy("OPTIONAL FLAME DETOUR", "SKIP WITHOUT BLOCKING THE BRUTE"),
                Copy("BRUTE POSSESSION WINDOW", "CHOOSE AFTER THE HELLHOUND ROUTE"),
                Copy("INFERNAL HEART OBJECTIVE", "FINAL OBJECTIVE PRESSURE"));

        public static IReadOnlyList<InfernalDefenseBeatReadabilityProfile> All { get; } =
            Array.AsReadOnly(new[]
            {
                AshenSpur,
                CinderFork,
                EmberCircuit
            });

        private static InfernalDefenseBeatReadabilityProfile Create(
            InfernalDefensePacingRecipe recipe,
            params CopyDefinition[] copy)
        {
            var facts = new InfernalDefenseBeatReadabilityFact[recipe.Beats.Count];
            for (var index = 0; index < recipe.Beats.Count; index++)
            {
                var beat = recipe.Beats[index];
                facts[index] = new InfernalDefenseBeatReadabilityFact(
                    beat.BeatId,
                    beat.RoleId,
                    beat.Kind,
                    beat.Requirement,
                    copy[index].PrimaryCue,
                    copy[index].TacticalHint,
                    InfernalDefenseBeatReadabilityValidatorExpectedEmphasis.For(beat.Kind));
            }

            return new InfernalDefenseBeatReadabilityProfile(recipe.LayoutId, facts);
        }

        private static CopyDefinition Copy(string primaryCue, string tacticalHint)
        {
            return new CopyDefinition(primaryCue, tacticalHint);
        }

        private sealed class CopyDefinition
        {
            public CopyDefinition(string primaryCue, string tacticalHint)
            {
                PrimaryCue = primaryCue;
                TacticalHint = tacticalHint;
            }

            public string PrimaryCue { get; }
            public string TacticalHint { get; }
        }
    }

    internal static class InfernalDefenseBeatReadabilityValidatorExpectedEmphasis
    {
        public static InfernalDefenseBeatEmphasis For(InfernalDefensePacingBeatKind kind)
        {
            switch (kind)
            {
                case InfernalDefensePacingBeatKind.InvaderEntry:
                    return InfernalDefenseBeatEmphasis.Arrival;
                case InfernalDefensePacingBeatKind.HellhoundPressure:
                    return InfernalDefenseBeatEmphasis.Pressure;
                case InfernalDefensePacingBeatKind.FlameTrapOpportunity:
                    return InfernalDefenseBeatEmphasis.Hazard;
                case InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow:
                    return InfernalDefenseBeatEmphasis.Possession;
                case InfernalDefensePacingBeatKind.InfernalHeartObjective:
                    return InfernalDefenseBeatEmphasis.Objective;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }

    public static class StarterInfernalDefenseBeatReadabilityResolver
    {
        public static InfernalDefenseBeatReadabilityLookupResult ResolveExact(
            string layoutId)
        {
            if (!HasStableId(layoutId))
            {
                return new InfernalDefenseBeatReadabilityLookupResult(
                    InfernalDefenseBeatReadabilityLookupStatus.LayoutIdInvalid,
                    null);
            }

            if (!InfernalDefenseBeatReadabilityValidator.ValidateCatalogue(
                StarterInfernalDefenseBeatReadability.All).IsValid)
            {
                return new InfernalDefenseBeatReadabilityLookupResult(
                    InfernalDefenseBeatReadabilityLookupStatus.CatalogueInvalid,
                    null);
            }

            foreach (var profile in StarterInfernalDefenseBeatReadability.All)
            {
                if (string.Equals(profile.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return new InfernalDefenseBeatReadabilityLookupResult(
                        InfernalDefenseBeatReadabilityLookupStatus.Found,
                        profile);
                }
            }

            return new InfernalDefenseBeatReadabilityLookupResult(
                InfernalDefenseBeatReadabilityLookupStatus.NotFound,
                null);
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || !IsAsciiLowerAlphaNumeric(value[0])
                || !IsAsciiLowerAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            for (var index = 1; index < value.Length - 1; index++)
            {
                var symbol = value[index];
                if (!IsAsciiLowerAlphaNumeric(symbol)
                    && symbol != '.'
                    && symbol != '_'
                    && symbol != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiLowerAlphaNumeric(char value)
        {
            return value >= 'a' && value <= 'z'
                || value >= '0' && value <= '9';
        }
    }
}
