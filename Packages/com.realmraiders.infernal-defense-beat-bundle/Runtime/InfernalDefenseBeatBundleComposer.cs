using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefenseBeatReadability;
using RealmRaiders.Modules.InfernalDefensePacing;
using RealmRaiders.Modules.InfernalDefensePacingProgression;
using RealmRaiders.Modules.InfernalDefensePressureTiming;
using ProgressionSnapshot =
    RealmRaiders.Modules.InfernalDefensePacingProgression.InfernalDefensePacingProgression;

namespace RealmRaiders.Modules.InfernalDefenseBeatBundle
{
    /// <summary>
    /// Pure exact-source composition. It never chooses a beat or interprets an
    /// event; the requested beat must already be classified as eligible.
    /// </summary>
    public static class InfernalDefenseBeatBundleComposer
    {
        public static InfernalDefenseBeatBundleResult ComposeExactEligible(
            string layoutId,
            string beatId,
            InfernalDefensePacingProgressionResult progressionResult,
            InfernalDefensePressureBeatTiming timingFact,
            InfernalDefenseBeatReadabilityFact readabilityFact)
        {
            var issues = new List<InfernalDefenseBeatBundleIssue>();
            var recipe = ResolveRecipe(layoutId, issues);
            var authoredBeatIndex = ResolveBeatIndex(recipe, beatId, issues);
            var expectedBeat = authoredBeatIndex >= 0
                ? recipe.Beats[authoredBeatIndex]
                : null;

            var eligibilityEvidence = ValidateProgression(
                recipe,
                expectedBeat,
                layoutId,
                progressionResult,
                issues);
            var exactTiming = ValidateTiming(
                recipe,
                authoredBeatIndex,
                layoutId,
                timingFact,
                issues);
            var exactReadability = ValidateReadability(
                recipe,
                authoredBeatIndex,
                layoutId,
                readabilityFact,
                issues);

            if (expectedBeat != null
                && eligibilityEvidence != null
                && exactTiming != null
                && exactReadability != null
                && (!IdentityMatches(expectedBeat, eligibilityEvidence)
                    || !IdentityMatches(expectedBeat, exactTiming)
                    || !IdentityMatches(expectedBeat, exactReadability)))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.BundleIdentityMismatch);
            }

            if (issues.Count > 0)
            {
                return new InfernalDefenseBeatBundleResult(
                    InfernalDefenseBeatBundleStatus.Rejected,
                    null,
                    issues);
            }

            return new InfernalDefenseBeatBundleResult(
                InfernalDefenseBeatBundleStatus.Composed,
                new InfernalDefenseBeatBundle(
                    layoutId,
                    authoredBeatIndex,
                    progressionResult,
                    eligibilityEvidence,
                    exactTiming,
                    exactReadability),
                Array.AsReadOnly(Array.Empty<InfernalDefenseBeatBundleIssue>()));
        }

        private static InfernalDefensePacingRecipe ResolveRecipe(
            string layoutId,
            ICollection<InfernalDefenseBeatBundleIssue> issues)
        {
            if (string.IsNullOrEmpty(layoutId))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.LayoutIdMissing);
                return null;
            }

            if (!HasStableId(layoutId))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.LayoutIdInvalid);
                return null;
            }

            var lookup = StarterInfernalDefensePacingResolver.ResolveExact(layoutId);
            if (!lookup.Found)
            {
                AddIssue(
                    issues,
                    lookup.Status == InfernalDefensePacingLookupStatus.LayoutIdInvalid
                        ? InfernalDefenseBeatBundleIssue.LayoutIdInvalid
                        : InfernalDefenseBeatBundleIssue.LayoutNotFound);
                return null;
            }

            return lookup.Recipe;
        }

        private static int ResolveBeatIndex(
            InfernalDefensePacingRecipe recipe,
            string beatId,
            ICollection<InfernalDefenseBeatBundleIssue> issues)
        {
            if (string.IsNullOrEmpty(beatId))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.BeatIdMissing);
                return -1;
            }

            if (!HasStableId(beatId))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.BeatIdInvalid);
                return -1;
            }

            if (recipe == null)
            {
                return -1;
            }

            for (var index = 0; index < recipe.Beats.Count; index++)
            {
                if (string.Equals(recipe.Beats[index].BeatId, beatId, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            AddIssue(issues, InfernalDefenseBeatBundleIssue.BeatUnknown);
            return -1;
        }

        private static InfernalDefensePacingBeatEvidence ValidateProgression(
            InfernalDefensePacingRecipe recipe,
            InfernalDefensePacingBeat expectedBeat,
            string layoutId,
            InfernalDefensePacingProgressionResult result,
            ICollection<InfernalDefenseBeatBundleIssue> issues)
        {
            if (result == null)
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.ProgressionMissing);
                return null;
            }

            if (!result.HasProgression
                || result.Status != InfernalDefensePacingProgressionStatus.Evaluated
                || result.Progression == null
                || result.Issues.Count != 0
                || result.RecipeValidation == null
                || !result.RecipeValidation.IsValid)
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.ProgressionInvalid);
                return null;
            }

            if (!string.Equals(
                result.Progression.LayoutId,
                layoutId,
                StringComparison.Ordinal))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.ProgressionLayoutMismatch);
                return null;
            }

            if (recipe == null || !HasValidProgressionEvidence(recipe, result.Progression))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.ProgressionEvidenceInvalid);
                return null;
            }

            if (expectedBeat == null)
            {
                return null;
            }

            foreach (var evidence in result.Progression.Eligible)
            {
                if (string.Equals(evidence.BeatId, expectedBeat.BeatId, StringComparison.Ordinal))
                {
                    return evidence;
                }
            }

            AddIssue(issues, InfernalDefenseBeatBundleIssue.BeatIneligible);
            return null;
        }

        private static bool HasValidProgressionEvidence(
            InfernalDefensePacingRecipe recipe,
            ProgressionSnapshot progression)
        {
            var categoryByBeatId = new Dictionary<string, EvidenceCategory>(
                StringComparer.Ordinal);
            var evidenceByBeatId = new Dictionary<string, InfernalDefensePacingBeatEvidence>(
                StringComparer.Ordinal);
            if (!AddEvidence(
                    recipe,
                    progression.Completed,
                    EvidenceCategory.Completed,
                    categoryByBeatId,
                    evidenceByBeatId)
                || !AddEvidence(
                    recipe,
                    progression.SkippedOptional,
                    EvidenceCategory.SkippedOptional,
                    categoryByBeatId,
                    evidenceByBeatId)
                || !AddEvidence(
                    recipe,
                    progression.Eligible,
                    EvidenceCategory.Eligible,
                    categoryByBeatId,
                    evidenceByBeatId)
                || !AddEvidence(
                    recipe,
                    progression.Blocked,
                    EvidenceCategory.Blocked,
                    categoryByBeatId,
                    evidenceByBeatId)
                || categoryByBeatId.Count != recipe.Beats.Count)
            {
                return false;
            }

            var requiredGapSeen = false;
            foreach (var beat in recipe.Beats)
            {
                var category = categoryByBeatId[beat.BeatId];
                var evidence = evidenceByBeatId[beat.BeatId];
                if (category == EvidenceCategory.SkippedOptional
                    && beat.Requirement != InfernalDefensePacingRequirement.Optional)
                {
                    return false;
                }

                if (beat.Requirement == InfernalDefensePacingRequirement.Required)
                {
                    if (category != EvidenceCategory.Completed)
                    {
                        requiredGapSeen = true;
                    }
                    else if (requiredGapSeen)
                    {
                        return false;
                    }
                }

                if (category == EvidenceCategory.Completed)
                {
                    foreach (var dependencyId in beat.DependsOnBeatIds)
                    {
                        if (categoryByBeatId[dependencyId] != EvidenceCategory.Completed)
                        {
                            return false;
                        }
                    }
                }

                var expectedBlockers = new List<string>();
                foreach (var dependencyId in beat.DependsOnBeatIds)
                {
                    var dependencyCategory = categoryByBeatId[dependencyId];
                    if (dependencyCategory != EvidenceCategory.Completed
                        && dependencyCategory != EvidenceCategory.SkippedOptional)
                    {
                        expectedBlockers.Add(dependencyId);
                    }
                }

                if (category == EvidenceCategory.Eligible && expectedBlockers.Count != 0
                    || category == EvidenceCategory.Blocked && expectedBlockers.Count == 0
                    || category != EvidenceCategory.Blocked
                        && evidence.BlockingBeatIds.Count != 0
                    || category == EvidenceCategory.Blocked
                        && !SequenceEqual(expectedBlockers, evidence.BlockingBeatIds))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AddEvidence(
            InfernalDefensePacingRecipe recipe,
            IReadOnlyList<InfernalDefensePacingBeatEvidence> evidenceList,
            EvidenceCategory category,
            IDictionary<string, EvidenceCategory> categoryByBeatId,
            IDictionary<string, InfernalDefensePacingBeatEvidence> evidenceByBeatId)
        {
            if (evidenceList == null)
            {
                return false;
            }

            var previousIndex = -1;
            foreach (var evidence in evidenceList)
            {
                if (evidence == null)
                {
                    return false;
                }

                var index = FindBeatIndex(recipe, evidence.BeatId);
                if (index < 0
                    || index <= previousIndex
                    || categoryByBeatId.ContainsKey(evidence.BeatId)
                    || !IdentityMatches(recipe.Beats[index], evidence)
                    || !SequenceEqual(
                        recipe.Beats[index].DependsOnBeatIds,
                        evidence.DependsOnBeatIds))
                {
                    return false;
                }

                previousIndex = index;
                categoryByBeatId.Add(evidence.BeatId, category);
                evidenceByBeatId.Add(evidence.BeatId, evidence);
            }

            return true;
        }

        private static InfernalDefensePressureBeatTiming ValidateTiming(
            InfernalDefensePacingRecipe recipe,
            int beatIndex,
            string layoutId,
            InfernalDefensePressureBeatTiming timingFact,
            ICollection<InfernalDefenseBeatBundleIssue> issues)
        {
            if (timingFact == null)
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.TimingFactMissing);
                return null;
            }

            if (recipe == null || beatIndex < 0)
            {
                return null;
            }

            var lookup = StarterInfernalDefensePressureTimingResolver.ResolveExact(layoutId);
            if (!lookup.Found
                || !InfernalDefensePressureTimingValidator.Validate(lookup.Profile).IsValid)
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.TimingSourceInvalid);
                return null;
            }

            var expected = lookup.Profile.Beats[beatIndex];
            var candidate = ReplaceTiming(lookup.Profile, beatIndex, timingFact);
            if (!InfernalDefensePressureTimingValidator.Validate(candidate).IsValid)
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.TimingFactInvalid);
            }

            if (!TimingFieldsEqual(expected, timingFact))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.TimingFactMismatch);
            }

            if (!ReferenceEquals(expected, timingFact))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.TimingFactIdentityMismatch);
            }

            return ReferenceEquals(expected, timingFact) ? timingFact : null;
        }

        private static InfernalDefenseBeatReadabilityFact ValidateReadability(
            InfernalDefensePacingRecipe recipe,
            int beatIndex,
            string layoutId,
            InfernalDefenseBeatReadabilityFact readabilityFact,
            ICollection<InfernalDefenseBeatBundleIssue> issues)
        {
            if (readabilityFact == null)
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.ReadabilityFactMissing);
                return null;
            }

            if (recipe == null || beatIndex < 0)
            {
                return null;
            }

            var lookup = StarterInfernalDefenseBeatReadabilityResolver.ResolveExact(layoutId);
            if (!lookup.Found
                || !InfernalDefenseBeatReadabilityValidator.Validate(lookup.Profile).IsValid)
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.ReadabilitySourceInvalid);
                return null;
            }

            var expected = lookup.Profile.Facts[beatIndex];
            var candidate = ReplaceReadability(lookup.Profile, beatIndex, readabilityFact);
            if (!InfernalDefenseBeatReadabilityValidator.Validate(candidate).IsValid)
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.ReadabilityFactInvalid);
            }

            if (!ReadabilityFieldsEqual(expected, readabilityFact))
            {
                AddIssue(issues, InfernalDefenseBeatBundleIssue.ReadabilityFactMismatch);
            }

            if (!ReferenceEquals(expected, readabilityFact))
            {
                AddIssue(
                    issues,
                    InfernalDefenseBeatBundleIssue.ReadabilityFactIdentityMismatch);
            }

            return ReferenceEquals(expected, readabilityFact) ? readabilityFact : null;
        }

        private static InfernalDefensePressureTimingProfile ReplaceTiming(
            InfernalDefensePressureTimingProfile source,
            int beatIndex,
            InfernalDefensePressureBeatTiming replacement)
        {
            var beats = new InfernalDefensePressureBeatTiming[source.Beats.Count];
            for (var index = 0; index < beats.Length; index++)
            {
                beats[index] = index == beatIndex ? replacement : source.Beats[index];
            }

            return new InfernalDefensePressureTimingProfile(
                source.LayoutId,
                source.TargetDurationSeconds,
                beats);
        }

        private static InfernalDefenseBeatReadabilityProfile ReplaceReadability(
            InfernalDefenseBeatReadabilityProfile source,
            int beatIndex,
            InfernalDefenseBeatReadabilityFact replacement)
        {
            var facts = new InfernalDefenseBeatReadabilityFact[source.Facts.Count];
            for (var index = 0; index < facts.Length; index++)
            {
                facts[index] = index == beatIndex ? replacement : source.Facts[index];
            }

            return new InfernalDefenseBeatReadabilityProfile(source.LayoutId, facts);
        }

        private static bool TimingFieldsEqual(
            InfernalDefensePressureBeatTiming expected,
            InfernalDefensePressureBeatTiming actual)
        {
            return actual != null
                && string.Equals(expected.BeatId, actual.BeatId, StringComparison.Ordinal)
                && string.Equals(expected.RoleId, actual.RoleId, StringComparison.Ordinal)
                && expected.Kind == actual.Kind
                && expected.Requirement == actual.Requirement
                && expected.StartOffsetSeconds.Equals(actual.StartOffsetSeconds)
                && expected.PressureDurationSeconds.Equals(actual.PressureDurationSeconds)
                && expected.ResponseWindowSeconds.Equals(actual.ResponseWindowSeconds);
        }

        private static bool ReadabilityFieldsEqual(
            InfernalDefenseBeatReadabilityFact expected,
            InfernalDefenseBeatReadabilityFact actual)
        {
            return actual != null
                && string.Equals(expected.BeatId, actual.BeatId, StringComparison.Ordinal)
                && string.Equals(expected.RoleId, actual.RoleId, StringComparison.Ordinal)
                && expected.Kind == actual.Kind
                && expected.Requirement == actual.Requirement
                && string.Equals(expected.PrimaryCue, actual.PrimaryCue, StringComparison.Ordinal)
                && string.Equals(
                    expected.TacticalHint,
                    actual.TacticalHint,
                    StringComparison.Ordinal)
                && expected.Emphasis == actual.Emphasis;
        }

        private static bool IdentityMatches(
            InfernalDefensePacingBeat expected,
            InfernalDefensePacingBeatEvidence actual)
        {
            return actual != null
                && string.Equals(expected.BeatId, actual.BeatId, StringComparison.Ordinal)
                && string.Equals(expected.RoleId, actual.RoleId, StringComparison.Ordinal)
                && expected.Kind == actual.Kind
                && expected.Requirement == actual.Requirement;
        }

        private static bool IdentityMatches(
            InfernalDefensePacingBeat expected,
            InfernalDefensePressureBeatTiming actual)
        {
            return actual != null
                && string.Equals(expected.BeatId, actual.BeatId, StringComparison.Ordinal)
                && string.Equals(expected.RoleId, actual.RoleId, StringComparison.Ordinal)
                && expected.Kind == actual.Kind
                && expected.Requirement == actual.Requirement;
        }

        private static bool IdentityMatches(
            InfernalDefensePacingBeat expected,
            InfernalDefenseBeatReadabilityFact actual)
        {
            return actual != null
                && string.Equals(expected.BeatId, actual.BeatId, StringComparison.Ordinal)
                && string.Equals(expected.RoleId, actual.RoleId, StringComparison.Ordinal)
                && expected.Kind == actual.Kind
                && expected.Requirement == actual.Requirement;
        }

        private static int FindBeatIndex(
            InfernalDefensePacingRecipe recipe,
            string beatId)
        {
            for (var index = 0; index < recipe.Beats.Count; index++)
            {
                if (string.Equals(recipe.Beats[index].BeatId, beatId, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool SequenceEqual(
            IReadOnlyList<string> first,
            IReadOnlyList<string> second)
        {
            if (first == null || second == null || first.Count != second.Count)
            {
                return false;
            }

            for (var index = 0; index < first.Count; index++)
            {
                if (!string.Equals(first[index], second[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
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

        private static void AddIssue(
            ICollection<InfernalDefenseBeatBundleIssue> issues,
            InfernalDefenseBeatBundleIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private enum EvidenceCategory
        {
            Completed,
            SkippedOptional,
            Eligible,
            Blocked
        }
    }
}
