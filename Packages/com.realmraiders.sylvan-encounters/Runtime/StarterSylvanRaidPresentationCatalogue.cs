using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.SylvanEncounters
{
    public enum SylvanRaidPresentationLookupStatus
    {
        Found,
        InvalidCompositionId,
        NotFound
    }

    public enum SylvanRaidPresentationValidationIssue
    {
        PresentationMissing,
        CompositionIdInvalid,
        CompositionNotFound,
        DisplayNameMismatch,
        TacticalSummaryMismatch
    }

    /// <summary>Immutable compact-Hub facts for one already-authored Sylvan raid.</summary>
    public sealed class SylvanRaidPresentationFact
    {
        public SylvanRaidPresentationFact(
            string compositionId,
            string displayName,
            string tacticalSummary)
        {
            CompositionId = compositionId;
            DisplayName = displayName;
            TacticalSummary = tacticalSummary;
        }

        public string CompositionId { get; }

        public string DisplayName { get; }

        public string TacticalSummary { get; }
    }

    /// <summary>Fail-closed result of an exact cached presentation lookup.</summary>
    public sealed class SylvanRaidPresentationLookupResult
    {
        public SylvanRaidPresentationLookupResult(
            SylvanRaidPresentationLookupStatus status,
            SylvanRaidPresentationFact presentation)
        {
            Status = status;
            Presentation = presentation;
        }

        public SylvanRaidPresentationLookupStatus Status { get; }

        public SylvanRaidPresentationFact Presentation { get; }

        public bool Found
        {
            get
            {
                return Status == SylvanRaidPresentationLookupStatus.Found;
            }
        }
    }

    /// <summary>Immutable, ordered evidence for a supplied presentation fact.</summary>
    public sealed class SylvanRaidPresentationValidationResult
    {
        public SylvanRaidPresentationValidationResult(
            IReadOnlyList<SylvanRaidPresentationValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<SylvanRaidPresentationValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<SylvanRaidPresentationValidationIssue> Snapshot(
            IReadOnlyList<SylvanRaidPresentationValidationIssue> issues)
        {
            if (issues == null)
            {
                return Array.AsReadOnly(Array.Empty<SylvanRaidPresentationValidationIssue>());
            }

            var copy = new SylvanRaidPresentationValidationIssue[issues.Count];
            for (var index = 0; index < issues.Count; index++)
            {
                copy[index] = issues[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// Cached presentation facts in the composition catalogue's order. Core decides
    /// whether and where to show a returned fact; this class has no UI authority.
    /// </summary>
    public static class StarterSylvanRaidPresentationCatalogue
    {
        public static SylvanRaidPresentationFact Baseline { get; } =
            CreatePresentation(StarterSylvanRaidCompositions.Baseline);

        public static SylvanRaidPresentationFact WolfPressure { get; } =
            CreatePresentation(StarterSylvanRaidCompositions.WolfPressure);

        public static SylvanRaidPresentationFact SentinelEscort { get; } =
            CreatePresentation(StarterSylvanRaidCompositions.SentinelEscort);

        public static IReadOnlyList<SylvanRaidPresentationFact> All { get; } =
            Array.AsReadOnly(new[]
            {
                Baseline,
                WolfPressure,
                SentinelEscort
            });

        public static SylvanRaidPresentationLookupResult FindByCompositionId(
            string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId))
            {
                return new SylvanRaidPresentationLookupResult(
                    SylvanRaidPresentationLookupStatus.InvalidCompositionId,
                    null);
            }

            foreach (var presentation in All)
            {
                if (string.Equals(
                        presentation.CompositionId,
                        compositionId,
                        StringComparison.Ordinal))
                {
                    return new SylvanRaidPresentationLookupResult(
                        SylvanRaidPresentationLookupStatus.Found,
                        presentation);
                }
            }

            return new SylvanRaidPresentationLookupResult(
                SylvanRaidPresentationLookupStatus.NotFound,
                null);
        }

        internal static string CreateTacticalSummary(SylvanRaidComposition composition)
        {
            if (composition == null)
            {
                return string.Empty;
            }

            var wolfCount = 0;
            var entCount = 0;
            var hasMoonwellWolf = false;
            var hasEntGroveWolf = false;
            var hasWolfGroveWolf = false;

            foreach (var spawn in composition.Spawns)
            {
                if (spawn == null)
                {
                    return string.Empty;
                }

                if (string.Equals(
                        spawn.ArchetypeId,
                        StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                        StringComparison.Ordinal))
                {
                    wolfCount++;
                    if (string.Equals(
                            spawn.NodeId,
                            StarterSylvanRaidCompositions.MoonwellNodeId,
                            StringComparison.Ordinal))
                    {
                        hasMoonwellWolf = true;
                        continue;
                    }

                    if (string.Equals(
                            spawn.NodeId,
                            StarterSylvanRaidCompositions.EntGroveNodeId,
                            StringComparison.Ordinal))
                    {
                        hasEntGroveWolf = true;
                        continue;
                    }

                    if (string.Equals(
                            spawn.NodeId,
                            StarterSylvanRaidCompositions.WolfGroveNodeId,
                            StringComparison.Ordinal))
                    {
                        hasWolfGroveWolf = true;
                        continue;
                    }

                    return string.Empty;
                }

                if (string.Equals(
                        spawn.ArchetypeId,
                        StarterSylvanRaidCompositions.GuardianEntArchetypeId,
                        StringComparison.Ordinal))
                {
                    entCount++;
                    continue;
                }

                return string.Empty;
            }

            if (wolfCount <= 0 || entCount != 1)
            {
                return string.Empty;
            }

            var pressureNode = hasMoonwellWolf
                ? "MOONWELL"
                : hasEntGroveWolf
                    ? "ENT GROVE"
                    : hasWolfGroveWolf
                        ? "WOLF GROVE"
                        : string.Empty;
            if (string.IsNullOrEmpty(pressureNode))
            {
                return string.Empty;
            }

            var wolves = wolfCount == 1 ? "1 WOLF" : wolfCount + " WOLVES";
            return wolves + " \u2022 " + pressureNode + " PRESSURE \u2022 1 ENT";
        }

        private static SylvanRaidPresentationFact CreatePresentation(
            SylvanRaidComposition composition)
        {
            return new SylvanRaidPresentationFact(
                composition.CompositionId,
                composition.DisplayName,
                CreateTacticalSummary(composition));
        }
    }

    /// <summary>
    /// Deterministic evidence that compact presentation text remains tied to current
    /// composition facts. It does not choose a raid or control a Hub.
    /// </summary>
    public static class SylvanRaidPresentationEvidence
    {
        public static SylvanRaidPresentationValidationResult Validate(
            SylvanRaidPresentationFact presentation)
        {
            var issues = new List<SylvanRaidPresentationValidationIssue>();

            if (presentation == null)
            {
                AddIssue(issues, SylvanRaidPresentationValidationIssue.PresentationMissing);
                return new SylvanRaidPresentationValidationResult(issues);
            }

            if (string.IsNullOrWhiteSpace(presentation.CompositionId))
            {
                AddIssue(issues, SylvanRaidPresentationValidationIssue.CompositionIdInvalid);
                return new SylvanRaidPresentationValidationResult(issues);
            }

            var composition = FindComposition(presentation.CompositionId);
            if (composition == null)
            {
                AddIssue(issues, SylvanRaidPresentationValidationIssue.CompositionNotFound);
                return new SylvanRaidPresentationValidationResult(issues);
            }

            if (!string.Equals(
                    presentation.DisplayName,
                    composition.DisplayName,
                    StringComparison.Ordinal))
            {
                AddIssue(issues, SylvanRaidPresentationValidationIssue.DisplayNameMismatch);
            }

            var expectedSummary = StarterSylvanRaidPresentationCatalogue.CreateTacticalSummary(
                composition);
            if (string.IsNullOrEmpty(expectedSummary)
                || !string.Equals(
                    presentation.TacticalSummary,
                    expectedSummary,
                    StringComparison.Ordinal))
            {
                AddIssue(issues, SylvanRaidPresentationValidationIssue.TacticalSummaryMismatch);
            }

            return new SylvanRaidPresentationValidationResult(issues);
        }

        private static SylvanRaidComposition FindComposition(string compositionId)
        {
            foreach (var composition in StarterSylvanRaidCompositions.All)
            {
                if (string.Equals(
                        composition.CompositionId,
                        compositionId,
                        StringComparison.Ordinal))
                {
                    return composition;
                }
            }

            return null;
        }

        private static void AddIssue(
            ICollection<SylvanRaidPresentationValidationIssue> issues,
            SylvanRaidPresentationValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }
}
