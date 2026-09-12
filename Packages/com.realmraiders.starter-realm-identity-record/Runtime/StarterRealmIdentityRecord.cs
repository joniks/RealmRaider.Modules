using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.StarterRealmIdentityRecord
{
    public static class StarterRealmIdentityRecordContract
    {
        public const int CurrentVersion = 1;

        public const int MinimumIdCharacters = 3;

        public const int MaximumIdCharacters = 96;
    }

    public enum StarterRealmIdentityIssue
    {
        InputMissing,
        InputEmpty,
        EncodingInvalid,
        FieldSyntaxInvalid,
        FieldUnknown,
        FieldDuplicate,
        FieldExtra,
        FieldMissing,
        FieldOrderInvalid,
        TrailingData,
        VersionMalformed,
        VersionUnsupported,
        RealmIdMissing,
        RealmIdMalformed,
        RealmIdNonCanonical,
        SeedMalformed,
        SeedOverflow,
        LayoutIdMissing,
        LayoutIdMalformed,
        LayoutIdNonCanonical
    }

    public sealed class StarterRealmIdentityIssueEvidence
    {
        internal StarterRealmIdentityIssueEvidence(
            StarterRealmIdentityIssue issue,
            int fieldIndex,
            string fieldName,
            string suppliedValue)
        {
            Issue = issue;
            FieldIndex = fieldIndex;
            FieldName = fieldName;
            SuppliedValue = suppliedValue;
        }

        public StarterRealmIdentityIssue Issue { get; }

        /// <summary>Zero-based source field index, or -1 for record-level evidence.</summary>
        public int FieldIndex { get; }

        public string FieldName { get; }

        public string SuppliedValue { get; }
    }

    public sealed class StarterRealmIdentityRecord
    {
        internal StarterRealmIdentityRecord(
            int version,
            string realmId,
            int seed,
            string layoutId)
        {
            Version = version;
            RealmId = realmId;
            Seed = seed;
            LayoutId = layoutId;
        }

        public int Version { get; }

        public string RealmId { get; }

        public int Seed { get; }

        public string LayoutId { get; }
    }

    public sealed class StarterRealmIdentityResult
    {
        internal StarterRealmIdentityResult(
            StarterRealmIdentityRecord record,
            IReadOnlyList<StarterRealmIdentityIssueEvidence> evidence)
        {
            Record = record;
            Evidence = Snapshot(evidence);
            Issues = SnapshotIssues(Evidence);
        }

        public StarterRealmIdentityRecord Record { get; }

        public IReadOnlyList<StarterRealmIdentityIssueEvidence> Evidence { get; }

        public IReadOnlyList<StarterRealmIdentityIssue> Issues { get; }

        public bool HasRecord => Record != null && Evidence.Count == 0;

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

        private static IReadOnlyList<StarterRealmIdentityIssue> SnapshotIssues(
            IReadOnlyList<StarterRealmIdentityIssueEvidence> evidence)
        {
            var issues = new StarterRealmIdentityIssue[evidence.Count];
            for (var index = 0; index < evidence.Count; index++)
            {
                issues[index] = evidence[index].Issue;
            }

            return Array.AsReadOnly(issues);
        }
    }
}
