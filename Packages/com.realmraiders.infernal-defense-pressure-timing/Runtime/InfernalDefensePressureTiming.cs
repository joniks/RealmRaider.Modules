using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefensePacing;

namespace RealmRaiders.Modules.InfernalDefensePressureTiming
{
    public enum InfernalDefensePressureTimingLookupStatus
    {
        Found,
        LayoutIdInvalid,
        NotFound,
        CatalogueInvalid
    }

    public enum InfernalDefensePressureTimingValidationIssue
    {
        ProfileMissing,
        LayoutIdInvalid,
        LayoutNotFound,
        RecipeInvalid,
        BeatCardinalityInvalid,
        BeatMissing,
        BeatIdInvalid,
        BeatIdDuplicate,
        BeatUnknown,
        BeatOrderMismatch,
        BeatRoleMismatch,
        BeatKindMismatch,
        BeatRequirementMismatch,
        TimingNonFinite,
        TimingNotPositive,
        TimingOutOfBounds,
        ResponseWindowInvalid,
        DependencyOrderingInvalid,
        TargetDurationNonFinite,
        TargetDurationNotPositive,
        TargetDurationOutOfBounds,
        TargetDurationMismatch,
        FlameBruteOpportunityInvalid
    }

    public enum InfernalDefensePressureTimingCatalogueIssue
    {
        CatalogueMissing,
        ProfileCardinalityInvalid,
        ProfileMissing,
        LayoutIdDuplicate,
        ProfileInvalid,
        LayoutCoverageInvalid,
        SemanticSignatureDuplicate
    }

    /// <summary>Shared validation bounds; these constants do not run a clock.</summary>
    public static class InfernalDefensePressureTimingBounds
    {
        public const double MinimumStartSeconds = 0.25d;
        public const double MaximumStartSeconds = 60d;
        public const double MinimumPressureDurationSeconds = 0.5d;
        public const double MaximumPressureDurationSeconds = 20d;
        public const double MinimumResponseWindowSeconds = 0.5d;
        public const double MaximumResponseWindowSeconds = 10d;
        public const double MinimumTargetDurationSeconds = 20d;
        public const double MaximumTargetDurationSeconds = 60d;
        public const double MinimumFlameToBruteOpportunitySeconds = 2d;
        public const double MaximumFlameToBruteOpportunitySeconds = 12d;
    }

    /// <summary>
    /// One immutable timing fact for one exact MGC15 beat. StartOffsetSeconds is
    /// relative authored evidence, not a scheduling instruction.
    /// </summary>
    public sealed class InfernalDefensePressureBeatTiming
    {
        public InfernalDefensePressureBeatTiming(
            string beatId,
            string roleId,
            InfernalDefensePacingBeatKind kind,
            InfernalDefensePacingRequirement requirement,
            double startOffsetSeconds,
            double pressureDurationSeconds,
            double responseWindowSeconds)
        {
            BeatId = beatId;
            RoleId = roleId;
            Kind = kind;
            Requirement = requirement;
            StartOffsetSeconds = startOffsetSeconds;
            PressureDurationSeconds = pressureDurationSeconds;
            ResponseWindowSeconds = responseWindowSeconds;
        }

        public string BeatId { get; }

        public string RoleId { get; }

        public InfernalDefensePacingBeatKind Kind { get; }

        public InfernalDefensePacingRequirement Requirement { get; }

        public double StartOffsetSeconds { get; }

        public double PressureDurationSeconds { get; }

        public double ResponseWindowSeconds { get; }

        public double EndOffsetSeconds => StartOffsetSeconds + PressureDurationSeconds;
    }

    public sealed class InfernalDefensePressureTimingProfile
    {
        public InfernalDefensePressureTimingProfile(
            string layoutId,
            double targetDurationSeconds,
            IReadOnlyList<InfernalDefensePressureBeatTiming> beats)
        {
            LayoutId = layoutId;
            TargetDurationSeconds = targetDurationSeconds;
            Beats = Snapshot(beats);
        }

        public string LayoutId { get; }

        public double TargetDurationSeconds { get; }

        public IReadOnlyList<InfernalDefensePressureBeatTiming> Beats { get; }

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

    public sealed class InfernalDefensePressureTimingValidationResult
    {
        internal InfernalDefensePressureTimingValidationResult(
            IReadOnlyList<InfernalDefensePressureTimingValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalDefensePressureTimingValidationIssue> Issues { get; }

        public bool IsValid => Issues.Count == 0;

        private static IReadOnlyList<InfernalDefensePressureTimingValidationIssue> Snapshot(
            IReadOnlyList<InfernalDefensePressureTimingValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(
                    Array.Empty<InfernalDefensePressureTimingValidationIssue>());
            }

            var copy = new InfernalDefensePressureTimingValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalDefensePressureTimingCatalogueValidationResult
    {
        internal InfernalDefensePressureTimingCatalogueValidationResult(
            IReadOnlyList<InfernalDefensePressureTimingCatalogueIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalDefensePressureTimingCatalogueIssue> Issues { get; }

        public bool IsValid => Issues.Count == 0;

        private static IReadOnlyList<InfernalDefensePressureTimingCatalogueIssue> Snapshot(
            IReadOnlyList<InfernalDefensePressureTimingCatalogueIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(
                    Array.Empty<InfernalDefensePressureTimingCatalogueIssue>());
            }

            var copy = new InfernalDefensePressureTimingCatalogueIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalDefensePressureTimingLookupResult
    {
        internal InfernalDefensePressureTimingLookupResult(
            InfernalDefensePressureTimingLookupStatus status,
            InfernalDefensePressureTimingProfile profile)
        {
            Status = status;
            Profile = profile;
        }

        public InfernalDefensePressureTimingLookupStatus Status { get; }

        public InfernalDefensePressureTimingProfile Profile { get; }

        public bool Found =>
            Status == InfernalDefensePressureTimingLookupStatus.Found && Profile != null;
    }
}
