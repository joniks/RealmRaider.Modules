using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefensePacing;

namespace RealmRaiders.Modules.InfernalDefenseBeatReadability
{
    public enum InfernalDefenseBeatEmphasis
    {
        Arrival,
        Pressure,
        Hazard,
        Possession,
        Objective
    }

    public enum InfernalDefenseBeatReadabilityLookupStatus
    {
        Found,
        LayoutIdInvalid,
        NotFound,
        CatalogueInvalid
    }

    public enum InfernalDefenseBeatReadabilityValidationIssue
    {
        ProfileMissing,
        LayoutIdInvalid,
        LayoutNotFound,
        RecipeInvalid,
        FactCardinalityInvalid,
        FactMissing,
        BeatIdInvalid,
        BeatIdDuplicate,
        BeatUnknown,
        BeatOrderMismatch,
        BeatRoleMismatch,
        BeatKindMismatch,
        BeatRequirementMismatch,
        PrimaryCueMissing,
        PrimaryCueLengthInvalid,
        TacticalHintLengthInvalid,
        CopyCharacterInvalid,
        CopySpacingInvalid,
        EmphasisInvalid,
        EmphasisRoleMismatch,
        CopySemanticMismatch,
        PromiseCopyInvalid
    }

    public enum InfernalDefenseBeatReadabilityCatalogueIssue
    {
        CatalogueMissing,
        ProfileCardinalityInvalid,
        ProfileMissing,
        LayoutIdDuplicate,
        ProfileInvalid,
        LayoutCoverageInvalid,
        SemanticSignatureDuplicate
    }

    public static class InfernalDefenseBeatReadabilityBounds
    {
        public const int MinimumPrimaryCueCharacters = 4;
        public const int MaximumPrimaryCueCharacters = 32;
        public const int MinimumTacticalHintCharacters = 8;
        public const int MaximumTacticalHintCharacters = 64;
    }

    /// <summary>
    /// One immutable copy fact for one exact MGC15 beat. It carries no display
    /// eligibility and cannot make the represented beat factual.
    /// </summary>
    public sealed class InfernalDefenseBeatReadabilityFact
    {
        public InfernalDefenseBeatReadabilityFact(
            string beatId,
            string roleId,
            InfernalDefensePacingBeatKind kind,
            InfernalDefensePacingRequirement requirement,
            string primaryCue,
            string tacticalHint,
            InfernalDefenseBeatEmphasis emphasis)
        {
            BeatId = beatId;
            RoleId = roleId;
            Kind = kind;
            Requirement = requirement;
            PrimaryCue = primaryCue;
            TacticalHint = tacticalHint;
            Emphasis = emphasis;
        }

        public string BeatId { get; }

        public string RoleId { get; }

        public InfernalDefensePacingBeatKind Kind { get; }

        public InfernalDefensePacingRequirement Requirement { get; }

        public string PrimaryCue { get; }

        public string TacticalHint { get; }

        public bool HasTacticalHint => !string.IsNullOrEmpty(TacticalHint);

        public InfernalDefenseBeatEmphasis Emphasis { get; }
    }

    public sealed class InfernalDefenseBeatReadabilityProfile
    {
        public InfernalDefenseBeatReadabilityProfile(
            string layoutId,
            IReadOnlyList<InfernalDefenseBeatReadabilityFact> facts)
        {
            LayoutId = layoutId;
            Facts = Snapshot(facts);
        }

        public string LayoutId { get; }

        public IReadOnlyList<InfernalDefenseBeatReadabilityFact> Facts { get; }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<T>());
            }

            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalDefenseBeatReadabilityValidationResult
    {
        internal InfernalDefenseBeatReadabilityValidationResult(
            IReadOnlyList<InfernalDefenseBeatReadabilityValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalDefenseBeatReadabilityValidationIssue> Issues { get; }

        public bool IsValid => Issues.Count == 0;

        private static IReadOnlyList<InfernalDefenseBeatReadabilityValidationIssue> Snapshot(
            IReadOnlyList<InfernalDefenseBeatReadabilityValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(
                    Array.Empty<InfernalDefenseBeatReadabilityValidationIssue>());
            }

            var copy = new InfernalDefenseBeatReadabilityValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalDefenseBeatReadabilityCatalogueValidationResult
    {
        internal InfernalDefenseBeatReadabilityCatalogueValidationResult(
            IReadOnlyList<InfernalDefenseBeatReadabilityCatalogueIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalDefenseBeatReadabilityCatalogueIssue> Issues { get; }

        public bool IsValid => Issues.Count == 0;

        private static IReadOnlyList<InfernalDefenseBeatReadabilityCatalogueIssue> Snapshot(
            IReadOnlyList<InfernalDefenseBeatReadabilityCatalogueIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(
                    Array.Empty<InfernalDefenseBeatReadabilityCatalogueIssue>());
            }

            var copy = new InfernalDefenseBeatReadabilityCatalogueIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalDefenseBeatReadabilityLookupResult
    {
        internal InfernalDefenseBeatReadabilityLookupResult(
            InfernalDefenseBeatReadabilityLookupStatus status,
            InfernalDefenseBeatReadabilityProfile profile)
        {
            Status = status;
            Profile = profile;
        }

        public InfernalDefenseBeatReadabilityLookupStatus Status { get; }

        public InfernalDefenseBeatReadabilityProfile Profile { get; }

        public bool Found =>
            Status == InfernalDefenseBeatReadabilityLookupStatus.Found && Profile != null;
    }
}
