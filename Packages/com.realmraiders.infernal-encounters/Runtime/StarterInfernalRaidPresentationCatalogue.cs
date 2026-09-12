using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.InfernalEncounters
{
    public enum InfernalRaidPresentationLookupStatus
    {
        Found,
        InvalidCompositionId,
        NotFound
    }

    public enum InfernalRaidPresentationValidationIssue
    {
        PresentationMissing,
        CompositionIdInvalid,
        PacingCompositionNotFound,
        DisplayNameMismatch,
        TacticalSummaryMismatch
    }

    /// <summary>Immutable compact-Hub facts for one already-authored raid composition.</summary>
    public sealed class InfernalRaidPresentationFact
    {
        public InfernalRaidPresentationFact(
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
    public sealed class InfernalRaidPresentationLookupResult
    {
        public InfernalRaidPresentationLookupResult(
            InfernalRaidPresentationLookupStatus status,
            InfernalRaidPresentationFact presentation)
        {
            Status = status;
            Presentation = presentation;
        }

        public InfernalRaidPresentationLookupStatus Status { get; }

        public InfernalRaidPresentationFact Presentation { get; }

        public bool Found
        {
            get
            {
                return Status == InfernalRaidPresentationLookupStatus.Found;
            }
        }
    }

    /// <summary>Immutable, ordered evidence for a supplied presentation fact.</summary>
    public sealed class InfernalRaidPresentationValidationResult
    {
        public InfernalRaidPresentationValidationResult(
            IReadOnlyList<InfernalRaidPresentationValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalRaidPresentationValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<InfernalRaidPresentationValidationIssue> Snapshot(
            IReadOnlyList<InfernalRaidPresentationValidationIssue> issues)
        {
            if (issues == null)
            {
                return Array.AsReadOnly(Array.Empty<InfernalRaidPresentationValidationIssue>());
            }

            var copy = new InfernalRaidPresentationValidationIssue[issues.Count];
            for (var index = 0; index < issues.Count; index++)
            {
                copy[index] = issues[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// Cached presentation facts in the pacing catalogue's order. Core must choose
    /// whether and where to show a returned fact; this class has no UI authority.
    /// </summary>
    public static class StarterInfernalRaidPresentationCatalogue
    {
        public static InfernalRaidPresentationFact EntryTrial { get; } =
            CreatePresentation(StarterInfernalRaidPacingCatalogue.EntryTrial);

        public static InfernalRaidPresentationFact RiskRoute { get; } =
            CreatePresentation(StarterInfernalRaidPacingCatalogue.RiskRoute);

        public static InfernalRaidPresentationFact BruteFinale { get; } =
            CreatePresentation(StarterInfernalRaidPacingCatalogue.BruteFinale);

        public static IReadOnlyList<InfernalRaidPresentationFact> All { get; } =
            Array.AsReadOnly(new[]
            {
                EntryTrial,
                RiskRoute,
                BruteFinale
            });

        public static InfernalRaidPresentationLookupResult FindByCompositionId(
            string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId))
            {
                return new InfernalRaidPresentationLookupResult(
                    InfernalRaidPresentationLookupStatus.InvalidCompositionId,
                    null);
            }

            foreach (var presentation in All)
            {
                if (string.Equals(
                        presentation.CompositionId,
                        compositionId,
                        StringComparison.Ordinal))
                {
                    return new InfernalRaidPresentationLookupResult(
                        InfernalRaidPresentationLookupStatus.Found,
                        presentation);
                }
            }

            return new InfernalRaidPresentationLookupResult(
                InfernalRaidPresentationLookupStatus.NotFound,
                null);
        }

        internal static string CreateTacticalSummary(InfernalRaidPacingComposition composition)
        {
            if (composition == null || composition.ApproximateDurationSeconds <= 0)
            {
                return string.Empty;
            }

            var hellhoundCount = 0;
            var hasBrute = false;
            var hasOptionalFlameBypass = false;

            foreach (var beat in composition.Beats)
            {
                if (beat.Kind == InfernalRaidBeatKind.Enemy)
                {
                    if (string.Equals(
                            beat.ContentId,
                            StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                            StringComparison.Ordinal))
                    {
                        hellhoundCount++;
                        continue;
                    }

                    if (string.Equals(
                            beat.ContentId,
                            StarterInfernalRaidPacingCatalogue.InfernalBruteArchetypeId,
                            StringComparison.Ordinal))
                    {
                        hasBrute = true;
                        continue;
                    }

                    return string.Empty;
                }

                if (beat.Kind == InfernalRaidBeatKind.Hazard)
                {
                    if (!string.Equals(
                            beat.ContentId,
                            StarterInfernalRaidPacingCatalogue.FlameTrapContentId,
                            StringComparison.Ordinal)
                        || !beat.IsOptionalRisk
                        || !beat.IsBypassable)
                    {
                        return string.Empty;
                    }

                    hasOptionalFlameBypass = true;
                }
            }

            if (hellhoundCount <= 0)
            {
                return string.Empty;
            }

            var summary = hellhoundCount == 1
                ? "1 HELLHOUND"
                : hellhoundCount + " HELLHOUNDS";

            if (hasBrute)
            {
                summary += " + BRUTE";
            }

            if (hasOptionalFlameBypass)
            {
                summary += " \u2022 OPTIONAL FLAME BYPASS";
            }

            return summary + " \u2022 ~" + composition.ApproximateDurationSeconds + " SEC";
        }

        private static InfernalRaidPresentationFact CreatePresentation(
            InfernalRaidPacingComposition composition)
        {
            return new InfernalRaidPresentationFact(
                composition.CompositionId,
                composition.DisplayName,
                CreateTacticalSummary(composition));
        }
    }

    /// <summary>
    /// Deterministic evidence that presentation text remains tied to existing pacing
    /// facts. It does not choose a presentation or control a Hub.
    /// </summary>
    public static class InfernalRaidPresentationEvidence
    {
        public static InfernalRaidPresentationValidationResult Validate(
            InfernalRaidPresentationFact presentation)
        {
            var issues = new List<InfernalRaidPresentationValidationIssue>();

            if (presentation == null)
            {
                AddIssue(issues, InfernalRaidPresentationValidationIssue.PresentationMissing);
                return new InfernalRaidPresentationValidationResult(issues);
            }

            if (string.IsNullOrWhiteSpace(presentation.CompositionId))
            {
                AddIssue(issues, InfernalRaidPresentationValidationIssue.CompositionIdInvalid);
                return new InfernalRaidPresentationValidationResult(issues);
            }

            var composition = FindPacingComposition(presentation.CompositionId);
            if (composition == null)
            {
                AddIssue(issues, InfernalRaidPresentationValidationIssue.PacingCompositionNotFound);
                return new InfernalRaidPresentationValidationResult(issues);
            }

            if (!string.Equals(
                    presentation.DisplayName,
                    composition.DisplayName,
                    StringComparison.Ordinal))
            {
                AddIssue(issues, InfernalRaidPresentationValidationIssue.DisplayNameMismatch);
            }

            var expectedSummary = StarterInfernalRaidPresentationCatalogue.CreateTacticalSummary(
                composition);
            if (string.IsNullOrEmpty(expectedSummary)
                || !string.Equals(
                    presentation.TacticalSummary,
                    expectedSummary,
                    StringComparison.Ordinal))
            {
                AddIssue(issues, InfernalRaidPresentationValidationIssue.TacticalSummaryMismatch);
            }

            return new InfernalRaidPresentationValidationResult(issues);
        }

        private static InfernalRaidPacingComposition FindPacingComposition(string compositionId)
        {
            foreach (var composition in StarterInfernalRaidPacingCatalogue.All)
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
            ICollection<InfernalRaidPresentationValidationIssue> issues,
            InfernalRaidPresentationValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }
}
