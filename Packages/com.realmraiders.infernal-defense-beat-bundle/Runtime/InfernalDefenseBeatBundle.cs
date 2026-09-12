using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefenseBeatReadability;
using RealmRaiders.Modules.InfernalDefensePacing;
using RealmRaiders.Modules.InfernalDefensePacingProgression;
using RealmRaiders.Modules.InfernalDefensePressureTiming;

namespace RealmRaiders.Modules.InfernalDefenseBeatBundle
{
    public enum InfernalDefenseBeatBundleStatus
    {
        Composed,
        Rejected
    }

    public enum InfernalDefenseBeatBundleIssue
    {
        LayoutIdMissing,
        LayoutIdInvalid,
        LayoutNotFound,
        BeatIdMissing,
        BeatIdInvalid,
        BeatUnknown,
        ProgressionMissing,
        ProgressionInvalid,
        ProgressionLayoutMismatch,
        ProgressionEvidenceInvalid,
        BeatIneligible,
        TimingFactMissing,
        TimingSourceInvalid,
        TimingFactInvalid,
        TimingFactMismatch,
        TimingFactIdentityMismatch,
        ReadabilityFactMissing,
        ReadabilitySourceInvalid,
        ReadabilityFactInvalid,
        ReadabilityFactMismatch,
        ReadabilityFactIdentityMismatch,
        BundleIdentityMismatch
    }

    /// <summary>
    /// Immutable exact source composition. Each object reference is the validated
    /// MGC19, MGC20 or MGC21 source supplied or selected during composition.
    /// </summary>
    public sealed class InfernalDefenseBeatBundle
    {
        internal InfernalDefenseBeatBundle(
            string layoutId,
            int authoredBeatIndex,
            InfernalDefensePacingProgressionResult progressionResult,
            InfernalDefensePacingBeatEvidence eligibilityEvidence,
            InfernalDefensePressureBeatTiming timingFact,
            InfernalDefenseBeatReadabilityFact readabilityFact)
        {
            LayoutId = layoutId;
            AuthoredBeatIndex = authoredBeatIndex;
            BeatId = eligibilityEvidence.BeatId;
            RoleId = eligibilityEvidence.RoleId;
            Kind = eligibilityEvidence.Kind;
            Requirement = eligibilityEvidence.Requirement;
            ProgressionResult = progressionResult;
            EligibilityEvidence = eligibilityEvidence;
            TimingFact = timingFact;
            ReadabilityFact = readabilityFact;
        }

        public string LayoutId { get; }

        public int AuthoredBeatIndex { get; }

        public string BeatId { get; }

        public string RoleId { get; }

        public InfernalDefensePacingBeatKind Kind { get; }

        public InfernalDefensePacingRequirement Requirement { get; }

        public InfernalDefensePacingProgressionResult ProgressionResult { get; }

        public InfernalDefensePacingBeatEvidence EligibilityEvidence { get; }

        public InfernalDefensePressureBeatTiming TimingFact { get; }

        public InfernalDefenseBeatReadabilityFact ReadabilityFact { get; }
    }

    public sealed class InfernalDefenseBeatBundleResult
    {
        internal InfernalDefenseBeatBundleResult(
            InfernalDefenseBeatBundleStatus status,
            InfernalDefenseBeatBundle bundle,
            IReadOnlyList<InfernalDefenseBeatBundleIssue> issues)
        {
            Status = status;
            Bundle = bundle;
            Issues = Snapshot(issues);
        }

        public InfernalDefenseBeatBundleStatus Status { get; }

        public InfernalDefenseBeatBundle Bundle { get; }

        public IReadOnlyList<InfernalDefenseBeatBundleIssue> Issues { get; }

        public bool HasBundle =>
            Status == InfernalDefenseBeatBundleStatus.Composed
            && Bundle != null
            && Issues.Count == 0;

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
}
