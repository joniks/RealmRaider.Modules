using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.SeededLayoutSelection
{
    public enum SeededLayoutSelectionStatus
    {
        Selected,
        Rejected
    }

    public enum SeededLayoutSelectionIssue
    {
        RealmIdMissing,
        RealmIdMalformed,
        RealmIdNonCanonical,
        CatalogueMissing,
        CatalogueEmpty,
        LayoutIdMissing,
        LayoutIdMalformed,
        LayoutIdNonCanonical,
        LayoutIdDuplicate,
        PreviousLayoutIdMissing,
        PreviousLayoutIdMalformed,
        PreviousLayoutIdNonCanonical,
        PreviousLayoutIdUnknown
    }

    public static class CanonicalRealmLayoutIdBounds
    {
        public const int MinimumCharacters = 3;
        public const int MaximumCharacters = 96;
    }

    public sealed class SeededLayoutSelectionIssueEvidence
    {
        internal SeededLayoutSelectionIssueEvidence(
            SeededLayoutSelectionIssue issue,
            int catalogueIndex,
            string suppliedValue)
        {
            Issue = issue;
            CatalogueIndex = catalogueIndex;
            SuppliedValue = suppliedValue;
        }

        public SeededLayoutSelectionIssue Issue { get; }

        /// <summary>-1 identifies a realm, catalogue, or avoidance-level issue.</summary>
        public int CatalogueIndex { get; }

        public string SuppliedValue { get; }
    }

    public sealed class SeededLayoutSelection
    {
        internal SeededLayoutSelection(
            string realmId,
            int seed,
            string layoutId,
            int catalogueIndex,
            IReadOnlyList<string> canonicalLayoutIds,
            string previousLayoutId,
            bool previousLayoutAvoided,
            bool usedSingleLayoutFallback)
        {
            RealmId = realmId;
            Seed = seed;
            LayoutId = layoutId;
            CatalogueIndex = catalogueIndex;
            CanonicalLayoutIds = Snapshot(canonicalLayoutIds);
            PreviousLayoutId = previousLayoutId;
            PreviousLayoutAvoided = previousLayoutAvoided;
            UsedSingleLayoutFallback = usedSingleLayoutFallback;
        }

        public string RealmId { get; }

        public int Seed { get; }

        public string LayoutId { get; }

        public int CatalogueIndex { get; }

        public IReadOnlyList<string> CanonicalLayoutIds { get; }

        public string PreviousLayoutId { get; }

        public bool PreviousLayoutAvoided { get; }

        public bool UsedSingleLayoutFallback { get; }

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

    public sealed class SeededLayoutSelectionResult
    {
        internal SeededLayoutSelectionResult(
            SeededLayoutSelectionStatus status,
            SeededLayoutSelection selection,
            IReadOnlyList<SeededLayoutSelectionIssueEvidence> evidence)
        {
            Status = status;
            Selection = selection;
            Evidence = Snapshot(evidence);
            Issues = SnapshotIssues(Evidence);
        }

        public SeededLayoutSelectionStatus Status { get; }

        public SeededLayoutSelection Selection { get; }

        public IReadOnlyList<SeededLayoutSelectionIssueEvidence> Evidence { get; }

        public IReadOnlyList<SeededLayoutSelectionIssue> Issues { get; }

        public bool HasSelection =>
            Status == SeededLayoutSelectionStatus.Selected
            && Selection != null
            && Evidence.Count == 0;

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

        private static IReadOnlyList<SeededLayoutSelectionIssue> SnapshotIssues(
            IReadOnlyList<SeededLayoutSelectionIssueEvidence> evidence)
        {
            var issues = new SeededLayoutSelectionIssue[evidence.Count];
            for (var index = 0; index < evidence.Count; index++)
            {
                issues[index] = evidence[index].Issue;
            }

            return Array.AsReadOnly(issues);
        }
    }
}
