using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefensePacing;

namespace RealmRaiders.Modules.InfernalDefensePressureTiming
{
    /// <summary>
    /// Fail-closed structural and semantic validation. It observes authored
    /// facts only and never schedules, advances, or applies a beat.
    /// </summary>
    public static class InfernalDefensePressureTimingValidator
    {
        private const double EqualityTolerance = 0.000001d;

        public static InfernalDefensePressureTimingValidationResult Validate(
            InfernalDefensePressureTimingProfile profile)
        {
            var issues = new List<InfernalDefensePressureTimingValidationIssue>();
            if (profile == null)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.ProfileMissing);
                return new InfernalDefensePressureTimingValidationResult(issues);
            }

            var recipe = ResolveRecipe(profile.LayoutId, issues);
            if (!IsFinite(profile.TargetDurationSeconds))
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.TargetDurationNonFinite);
            }
            else
            {
                if (profile.TargetDurationSeconds <= 0d)
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue
                            .TargetDurationNotPositive);
                }

                if (profile.TargetDurationSeconds
                        < InfernalDefensePressureTimingBounds.MinimumTargetDurationSeconds
                    || profile.TargetDurationSeconds
                        > InfernalDefensePressureTimingBounds.MaximumTargetDurationSeconds)
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue
                            .TargetDurationOutOfBounds);
                }
            }

            if (recipe != null && profile.Beats.Count != recipe.Beats.Count)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.BeatCardinalityInvalid);
            }

            var timingByBeatId = new Dictionary<string, InfernalDefensePressureBeatTiming>(
                StringComparer.Ordinal);
            var recipeBeatById = CreateRecipeBeatIndex(recipe);
            var maximumEnd = 0d;
            for (var index = 0; index < profile.Beats.Count; index++)
            {
                var timing = profile.Beats[index];
                if (timing == null)
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue.BeatMissing);
                    continue;
                }

                var beatIdIsValid = HasStableId(timing.BeatId);
                if (!beatIdIsValid)
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue.BeatIdInvalid);
                }
                else if (!timingByBeatId.TryAdd(timing.BeatId, timing))
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue.BeatIdDuplicate);
                }

                ValidateTimingValues(timing, issues);
                if (IsFinite(timing.EndOffsetSeconds)
                    && timing.EndOffsetSeconds > maximumEnd)
                {
                    maximumEnd = timing.EndOffsetSeconds;
                }

                if (recipe == null || !beatIdIsValid)
                {
                    continue;
                }

                if (!recipeBeatById.TryGetValue(timing.BeatId, out var expectedBeat))
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue.BeatUnknown);
                    continue;
                }

                if (index >= recipe.Beats.Count
                    || !string.Equals(
                        recipe.Beats[index].BeatId,
                        timing.BeatId,
                        StringComparison.Ordinal))
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue.BeatOrderMismatch);
                }

                if (!string.Equals(
                    timing.RoleId,
                    expectedBeat.RoleId,
                    StringComparison.Ordinal))
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue.BeatRoleMismatch);
                }

                if (timing.Kind != expectedBeat.Kind)
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue.BeatKindMismatch);
                }

                if (timing.Requirement != expectedBeat.Requirement)
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingValidationIssue.BeatRequirementMismatch);
                }
            }

            if (IsFinite(profile.TargetDurationSeconds)
                && IsFinite(maximumEnd)
                && Absolute(profile.TargetDurationSeconds - maximumEnd) > EqualityTolerance)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.TargetDurationMismatch);
            }

            if (recipe != null)
            {
                ValidateDependencyOrdering(recipe, timingByBeatId, issues);
                ValidateFlameBruteOpportunity(recipe, timingByBeatId, issues);
            }

            return new InfernalDefensePressureTimingValidationResult(issues);
        }

        public static InfernalDefensePressureTimingCatalogueValidationResult ValidateCatalogue(
            IReadOnlyList<InfernalDefensePressureTimingProfile> profiles)
        {
            var issues = new List<InfernalDefensePressureTimingCatalogueIssue>();
            if (profiles == null)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingCatalogueIssue.CatalogueMissing);
                return new InfernalDefensePressureTimingCatalogueValidationResult(issues);
            }

            if (profiles.Count != StarterInfernalDefensePacing.All.Count)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingCatalogueIssue.ProfileCardinalityInvalid);
            }

            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            var semanticSignatures = new HashSet<string>(StringComparer.Ordinal);
            foreach (var profile in profiles)
            {
                if (profile == null)
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingCatalogueIssue.ProfileMissing);
                    continue;
                }

                if (!layoutIds.Add(profile.LayoutId))
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingCatalogueIssue.LayoutIdDuplicate);
                }

                var validation = Validate(profile);
                if (!validation.IsValid)
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingCatalogueIssue.ProfileInvalid);
                    continue;
                }

                var signature = CreateSemanticSignature(profile);
                if (!semanticSignatures.Add(signature))
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingCatalogueIssue.SemanticSignatureDuplicate);
                }
            }

            foreach (var recipe in StarterInfernalDefensePacing.All)
            {
                if (!layoutIds.Contains(recipe.LayoutId))
                {
                    AddIssue(
                        issues,
                        InfernalDefensePressureTimingCatalogueIssue.LayoutCoverageInvalid);
                }
            }

            return new InfernalDefensePressureTimingCatalogueValidationResult(issues);
        }

        /// <summary>
        /// Coarse timing-shape signature. It excludes layout and beat IDs so two
        /// layouts cannot pass merely by renaming otherwise equivalent pacing.
        /// </summary>
        public static string CreateSemanticSignature(
            InfernalDefensePressureTimingProfile profile)
        {
            if (profile == null || !Validate(profile).IsValid)
            {
                return null;
            }

            var parts = new string[profile.Beats.Count];
            for (var index = 0; index < profile.Beats.Count; index++)
            {
                var beat = profile.Beats[index];
                parts[index] = ((int)beat.Kind).ToString()
                    + ":"
                    + ((int)beat.Requirement).ToString()
                    + ":"
                    + TimingBand(beat.StartOffsetSeconds, 6d, 16d, 28d)
                    + ":"
                    + TimingBand(beat.PressureDurationSeconds, 5d, 8d, 11d)
                    + ":"
                    + TimingBand(beat.ResponseWindowSeconds, 2d, 3.5d, 5d);
            }

            return string.Join("|", parts);
        }

        private static InfernalDefensePacingRecipe ResolveRecipe(
            string layoutId,
            ICollection<InfernalDefensePressureTimingValidationIssue> issues)
        {
            if (!HasStableId(layoutId))
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.LayoutIdInvalid);
                return null;
            }

            var lookup = StarterInfernalDefensePacingResolver.ResolveExact(layoutId);
            if (!lookup.Found)
            {
                AddIssue(
                    issues,
                    lookup.Status == InfernalDefensePacingLookupStatus.LayoutIdInvalid
                        ? InfernalDefensePressureTimingValidationIssue.LayoutIdInvalid
                        : InfernalDefensePressureTimingValidationIssue.LayoutNotFound);
                return null;
            }

            if (!InfernalDefensePacingValidator.Validate(lookup.Recipe).IsValid)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.RecipeInvalid);
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

        private static void ValidateTimingValues(
            InfernalDefensePressureBeatTiming timing,
            ICollection<InfernalDefensePressureTimingValidationIssue> issues)
        {
            if (!IsFinite(timing.StartOffsetSeconds)
                || !IsFinite(timing.PressureDurationSeconds)
                || !IsFinite(timing.ResponseWindowSeconds)
                || !IsFinite(timing.EndOffsetSeconds))
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.TimingNonFinite);
                return;
            }

            if (timing.StartOffsetSeconds <= 0d
                || timing.PressureDurationSeconds <= 0d
                || timing.ResponseWindowSeconds <= 0d)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.TimingNotPositive);
            }

            if (timing.StartOffsetSeconds
                    < InfernalDefensePressureTimingBounds.MinimumStartSeconds
                || timing.StartOffsetSeconds
                    > InfernalDefensePressureTimingBounds.MaximumStartSeconds
                || timing.PressureDurationSeconds
                    < InfernalDefensePressureTimingBounds.MinimumPressureDurationSeconds
                || timing.PressureDurationSeconds
                    > InfernalDefensePressureTimingBounds.MaximumPressureDurationSeconds
                || timing.ResponseWindowSeconds
                    < InfernalDefensePressureTimingBounds.MinimumResponseWindowSeconds
                || timing.ResponseWindowSeconds
                    > InfernalDefensePressureTimingBounds.MaximumResponseWindowSeconds)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.TimingOutOfBounds);
            }

            if (timing.ResponseWindowSeconds > timing.PressureDurationSeconds)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue.ResponseWindowInvalid);
            }
        }

        private static void ValidateDependencyOrdering(
            InfernalDefensePacingRecipe recipe,
            IReadOnlyDictionary<string, InfernalDefensePressureBeatTiming> timingByBeatId,
            ICollection<InfernalDefensePressureTimingValidationIssue> issues)
        {
            foreach (var beat in recipe.Beats)
            {
                if (!timingByBeatId.TryGetValue(beat.BeatId, out var timing))
                {
                    continue;
                }

                foreach (var dependencyId in beat.DependsOnBeatIds)
                {
                    if (!timingByBeatId.TryGetValue(dependencyId, out var dependencyTiming)
                        || !IsFinite(timing.StartOffsetSeconds)
                        || !IsFinite(dependencyTiming.EndOffsetSeconds)
                        || timing.StartOffsetSeconds < dependencyTiming.EndOffsetSeconds)
                    {
                        AddIssue(
                            issues,
                            InfernalDefensePressureTimingValidationIssue
                                .DependencyOrderingInvalid);
                    }
                }
            }
        }

        private static void ValidateFlameBruteOpportunity(
            InfernalDefensePacingRecipe recipe,
            IReadOnlyDictionary<string, InfernalDefensePressureBeatTiming> timingByBeatId,
            ICollection<InfernalDefensePressureTimingValidationIssue> issues)
        {
            var flameBeat = FindBeat(
                recipe,
                InfernalDefensePacingBeatKind.FlameTrapOpportunity);
            var bruteBeat = FindBeat(
                recipe,
                InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow);
            if (flameBeat == null
                || bruteBeat == null
                || !timingByBeatId.TryGetValue(flameBeat.BeatId, out var flameTiming)
                || !timingByBeatId.TryGetValue(bruteBeat.BeatId, out var bruteTiming))
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue
                        .FlameBruteOpportunityInvalid);
                return;
            }

            var opportunitySeconds = bruteTiming.StartOffsetSeconds
                - flameTiming.EndOffsetSeconds;
            if (!IsFinite(opportunitySeconds)
                || opportunitySeconds
                    < InfernalDefensePressureTimingBounds
                        .MinimumFlameToBruteOpportunitySeconds
                || opportunitySeconds
                    > InfernalDefensePressureTimingBounds
                        .MaximumFlameToBruteOpportunitySeconds
                || bruteTiming.ResponseWindowSeconds
                    < InfernalDefensePressureTimingBounds
                        .MinimumFlameToBruteOpportunitySeconds)
            {
                AddIssue(
                    issues,
                    InfernalDefensePressureTimingValidationIssue
                        .FlameBruteOpportunityInvalid);
            }
        }

        private static InfernalDefensePacingBeat FindBeat(
            InfernalDefensePacingRecipe recipe,
            InfernalDefensePacingBeatKind kind)
        {
            foreach (var beat in recipe.Beats)
            {
                if (beat.Kind == kind)
                {
                    return beat;
                }
            }

            return null;
        }

        private static string TimingBand(
            double value,
            double firstBoundary,
            double secondBoundary,
            double thirdBoundary)
        {
            if (value < firstBoundary)
            {
                return "a";
            }

            if (value < secondBoundary)
            {
                return "b";
            }

            if (value < thirdBoundary)
            {
                return "c";
            }

            return "d";
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

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static double Absolute(double value)
        {
            return value < 0d ? -value : value;
        }

        private static void AddIssue(
            ICollection<InfernalDefensePressureTimingValidationIssue> issues,
            InfernalDefensePressureTimingValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private static void AddIssue(
            ICollection<InfernalDefensePressureTimingCatalogueIssue> issues,
            InfernalDefensePressureTimingCatalogueIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }

    /// <summary>Cached authored timing profiles for all exact MGC15 recipes.</summary>
    public static class StarterInfernalDefensePressureTiming
    {
        public static InfernalDefensePressureTimingProfile AshenSpur { get; } =
            Create(
                StarterInfernalDefensePacing.AshenSpur,
                34d,
                Timing(StarterInfernalDefensePacing.AshenSpur, 0, 1d, 4d, 2d),
                Timing(StarterInfernalDefensePacing.AshenSpur, 1, 6d, 12d, 3d),
                Timing(StarterInfernalDefensePacing.AshenSpur, 2, 8d, 5d, 2.5d),
                Timing(StarterInfernalDefensePacing.AshenSpur, 3, 16d, 8d, 4d),
                Timing(StarterInfernalDefensePacing.AshenSpur, 4, 27d, 7d, 3d));

        public static InfernalDefensePressureTimingProfile CinderFork { get; } =
            Create(
                StarterInfernalDefensePacing.CinderFork,
                35d,
                Timing(StarterInfernalDefensePacing.CinderFork, 0, 1d, 3d, 1.5d),
                Timing(StarterInfernalDefensePacing.CinderFork, 1, 5d, 10d, 2d),
                Timing(StarterInfernalDefensePacing.CinderFork, 2, 7d, 4d, 2d),
                Timing(StarterInfernalDefensePacing.CinderFork, 3, 18d, 7d, 3.5d),
                Timing(StarterInfernalDefensePacing.CinderFork, 4, 28d, 7d, 2.5d));

        public static InfernalDefensePressureTimingProfile EmberCircuit { get; } =
            Create(
                StarterInfernalDefensePacing.EmberCircuit,
                43d,
                Timing(StarterInfernalDefensePacing.EmberCircuit, 0, 1d, 4d, 2d),
                Timing(StarterInfernalDefensePacing.EmberCircuit, 1, 6d, 7d, 2.5d),
                Timing(StarterInfernalDefensePacing.EmberCircuit, 2, 15d, 5d, 3d),
                Timing(StarterInfernalDefensePacing.EmberCircuit, 3, 23d, 9d, 4.5d),
                Timing(StarterInfernalDefensePacing.EmberCircuit, 4, 35d, 8d, 3.5d));

        public static IReadOnlyList<InfernalDefensePressureTimingProfile> All { get; } =
            Array.AsReadOnly(new[]
            {
                AshenSpur,
                CinderFork,
                EmberCircuit
            });

        private static InfernalDefensePressureTimingProfile Create(
            InfernalDefensePacingRecipe recipe,
            double targetDurationSeconds,
            params InfernalDefensePressureBeatTiming[] beats)
        {
            return new InfernalDefensePressureTimingProfile(
                recipe.LayoutId,
                targetDurationSeconds,
                beats);
        }

        private static InfernalDefensePressureBeatTiming Timing(
            InfernalDefensePacingRecipe recipe,
            int beatIndex,
            double startOffsetSeconds,
            double pressureDurationSeconds,
            double responseWindowSeconds)
        {
            var beat = recipe.Beats[beatIndex];
            return new InfernalDefensePressureBeatTiming(
                beat.BeatId,
                beat.RoleId,
                beat.Kind,
                beat.Requirement,
                startOffsetSeconds,
                pressureDurationSeconds,
                responseWindowSeconds);
        }
    }

    public static class StarterInfernalDefensePressureTimingResolver
    {
        public static InfernalDefensePressureTimingLookupResult ResolveExact(
            string layoutId)
        {
            if (!HasStableId(layoutId))
            {
                return new InfernalDefensePressureTimingLookupResult(
                    InfernalDefensePressureTimingLookupStatus.LayoutIdInvalid,
                    null);
            }

            if (!InfernalDefensePressureTimingValidator.ValidateCatalogue(
                StarterInfernalDefensePressureTiming.All).IsValid)
            {
                return new InfernalDefensePressureTimingLookupResult(
                    InfernalDefensePressureTimingLookupStatus.CatalogueInvalid,
                    null);
            }

            foreach (var profile in StarterInfernalDefensePressureTiming.All)
            {
                if (string.Equals(profile.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return new InfernalDefensePressureTimingLookupResult(
                        InfernalDefensePressureTimingLookupStatus.Found,
                        profile);
                }
            }

            return new InfernalDefensePressureTimingLookupResult(
                InfernalDefensePressureTimingLookupStatus.NotFound,
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
