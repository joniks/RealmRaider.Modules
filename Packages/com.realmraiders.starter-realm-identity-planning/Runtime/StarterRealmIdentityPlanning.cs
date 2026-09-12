using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.RealmLayoutContracts;
using RealmRaiders.Modules.SeededLayoutSelection;
using RealmRaiders.Modules.StarterRealmIdentityRecord;
using RealmRaiders.Modules.StarterRealmLayouts;
using RealmRaiders.Modules.StarterRealmSeededSelection;
using IdentityRecord = RealmRaiders.Modules.StarterRealmIdentityRecord.StarterRealmIdentityRecord;
using SeededSelection = RealmRaiders.Modules.SeededLayoutSelection.SeededLayoutSelection;

namespace RealmRaiders.Modules.StarterRealmIdentityPlanning
{
    public enum StarterRealmIdentityPlanningStatus
    {
        Planned,
        SelectionResultMissing,
        SelectionRejected,
        RealmIdMismatch,
        SelectionCatalogueMismatch,
        SelectionEvidenceMismatch,
        RecordResultMissing,
        RecordRejected,
        RecordFieldMismatch,
        CanonicalBytesMissing,
        CanonicalBytesRejected,
        CanonicalBytesMismatch
    }

    public sealed class SylvanStarterRealmIdentityPlan
    {
        internal SylvanStarterRealmIdentityPlan(
            StarterRealmIdentityPlanningStatus status,
            SylvanStarterSeededLayoutResult selectionResult,
            StarterRealmIdentityResult identityResult,
            RealmLayoutRecipe recipe,
            IReadOnlyList<byte> canonicalBytes)
        {
            Status = status;
            SelectionResult = selectionResult;
            IdentityResult = identityResult;
            Recipe = recipe;
            CanonicalBytes = Snapshot(canonicalBytes);
        }

        public StarterRealmIdentityPlanningStatus Status { get; }

        public SylvanStarterSeededLayoutResult SelectionResult { get; }

        public StarterRealmIdentityResult IdentityResult { get; }

        public RealmLayoutRecipe Recipe { get; }

        public IReadOnlyList<byte> CanonicalBytes { get; }

        public bool HasPlan =>
            Status == StarterRealmIdentityPlanningStatus.Planned
            && SelectionResult != null
            && IdentityResult != null
            && IdentityResult.HasRecord
            && Recipe != null
            && CanonicalBytes.Count > 0;

        private static IReadOnlyList<byte> Snapshot(IReadOnlyList<byte> source)
        {
            return StarterRealmIdentityPlanningValidation.SnapshotBytes(source);
        }
    }

    public sealed class InfernalStarterRealmIdentityPlan
    {
        internal InfernalStarterRealmIdentityPlan(
            StarterRealmIdentityPlanningStatus status,
            InfernalStarterSeededLayoutResult selectionResult,
            StarterRealmIdentityResult identityResult,
            RealmLayoutGraph layout,
            IReadOnlyList<byte> canonicalBytes)
        {
            Status = status;
            SelectionResult = selectionResult;
            IdentityResult = identityResult;
            Layout = layout;
            CanonicalBytes = Snapshot(canonicalBytes);
        }

        public StarterRealmIdentityPlanningStatus Status { get; }

        public InfernalStarterSeededLayoutResult SelectionResult { get; }

        public StarterRealmIdentityResult IdentityResult { get; }

        public RealmLayoutGraph Layout { get; }

        public IReadOnlyList<byte> CanonicalBytes { get; }

        public bool HasPlan =>
            Status == StarterRealmIdentityPlanningStatus.Planned
            && SelectionResult != null
            && IdentityResult != null
            && IdentityResult.HasRecord
            && Layout != null
            && CanonicalBytes.Count > 0;

        private static IReadOnlyList<byte> Snapshot(IReadOnlyList<byte> source)
        {
            return StarterRealmIdentityPlanningValidation.SnapshotBytes(source);
        }
    }

    public static class SylvanStarterRealmIdentityPlanner
    {
        public static SylvanStarterRealmIdentityPlan PlanExact(
            string realmId,
            int seed)
        {
            var selection = SylvanStarterSeededLayoutSelector.SelectExact(realmId, seed);
            if (!selection.HasRecipe)
            {
                return Reject(
                    StarterRealmIdentityPlanningValidation.MapSelectionStatus(
                        selection.Status),
                    selection,
                    null);
            }

            var identity = StarterRealmIdentityCodec.Create(
                StarterRealmIdentityRecordContract.CurrentVersion,
                realmId,
                seed,
                selection.Recipe.LayoutId);
            var bytes = identity.HasRecord
                ? StarterRealmIdentityCodec.SerializeCanonicalUtf8(identity.Record)
                : null;
            return ComposeExact(selection, identity, bytes);
        }

        public static SylvanStarterRealmIdentityPlan ComposeExact(
            SylvanStarterSeededLayoutResult selectionResult,
            StarterRealmIdentityResult identityResult,
            IReadOnlyList<byte> canonicalBytes)
        {
            var selectionStatus = ValidateSelection(selectionResult);
            if (selectionStatus != StarterRealmIdentityPlanningStatus.Planned)
            {
                return Reject(selectionStatus, selectionResult, identityResult);
            }

            var status = StarterRealmIdentityPlanningValidation.ValidateRecordAndBytes(
                selectionResult.SelectionResult.Selection,
                identityResult,
                canonicalBytes,
                out var bytesSnapshot);
            if (status != StarterRealmIdentityPlanningStatus.Planned)
            {
                return Reject(status, selectionResult, identityResult);
            }

            return new SylvanStarterRealmIdentityPlan(
                StarterRealmIdentityPlanningStatus.Planned,
                selectionResult,
                identityResult,
                selectionResult.Recipe,
                bytesSnapshot);
        }

        private static StarterRealmIdentityPlanningStatus ValidateSelection(
            SylvanStarterSeededLayoutResult selectionResult)
        {
            if (selectionResult == null)
            {
                return StarterRealmIdentityPlanningStatus.SelectionResultMissing;
            }

            if (!selectionResult.HasRecipe)
            {
                return StarterRealmIdentityPlanningValidation.MapSelectionStatus(
                    selectionResult.Status);
            }

            var revalidated = SylvanStarterSeededLayoutSelector.ResolveExactSelection(
                selectionResult.SelectionResult);
            if (!revalidated.HasRecipe)
            {
                return StarterRealmIdentityPlanningValidation.MapSelectionStatus(
                    revalidated.Status);
            }

            return ReferenceEquals(revalidated.Recipe, selectionResult.Recipe)
                ? StarterRealmIdentityPlanningStatus.Planned
                : StarterRealmIdentityPlanningStatus.SelectionEvidenceMismatch;
        }

        private static SylvanStarterRealmIdentityPlan Reject(
            StarterRealmIdentityPlanningStatus status,
            SylvanStarterSeededLayoutResult selectionResult,
            StarterRealmIdentityResult identityResult)
        {
            return new SylvanStarterRealmIdentityPlan(
                status,
                selectionResult,
                identityResult,
                null,
                null);
        }
    }

    public static class InfernalStarterRealmIdentityPlanner
    {
        public static InfernalStarterRealmIdentityPlan PlanExact(
            string realmId,
            int seed)
        {
            var selection = InfernalStarterSeededLayoutSelector.SelectExact(realmId, seed);
            if (!selection.HasLayout)
            {
                return Reject(
                    StarterRealmIdentityPlanningValidation.MapSelectionStatus(
                        selection.Status),
                    selection,
                    null);
            }

            var identity = StarterRealmIdentityCodec.Create(
                StarterRealmIdentityRecordContract.CurrentVersion,
                realmId,
                seed,
                selection.Layout.LayoutId);
            var bytes = identity.HasRecord
                ? StarterRealmIdentityCodec.SerializeCanonicalUtf8(identity.Record)
                : null;
            return ComposeExact(selection, identity, bytes);
        }

        public static InfernalStarterRealmIdentityPlan ComposeExact(
            InfernalStarterSeededLayoutResult selectionResult,
            StarterRealmIdentityResult identityResult,
            IReadOnlyList<byte> canonicalBytes)
        {
            var selectionStatus = ValidateSelection(selectionResult);
            if (selectionStatus != StarterRealmIdentityPlanningStatus.Planned)
            {
                return Reject(selectionStatus, selectionResult, identityResult);
            }

            var status = StarterRealmIdentityPlanningValidation.ValidateRecordAndBytes(
                selectionResult.SelectionResult.Selection,
                identityResult,
                canonicalBytes,
                out var bytesSnapshot);
            if (status != StarterRealmIdentityPlanningStatus.Planned)
            {
                return Reject(status, selectionResult, identityResult);
            }

            return new InfernalStarterRealmIdentityPlan(
                StarterRealmIdentityPlanningStatus.Planned,
                selectionResult,
                identityResult,
                selectionResult.Layout,
                bytesSnapshot);
        }

        private static StarterRealmIdentityPlanningStatus ValidateSelection(
            InfernalStarterSeededLayoutResult selectionResult)
        {
            if (selectionResult == null)
            {
                return StarterRealmIdentityPlanningStatus.SelectionResultMissing;
            }

            if (!selectionResult.HasLayout)
            {
                return StarterRealmIdentityPlanningValidation.MapSelectionStatus(
                    selectionResult.Status);
            }

            var revalidated = InfernalStarterSeededLayoutSelector.ResolveExactSelection(
                selectionResult.SelectionResult);
            if (!revalidated.HasLayout)
            {
                return StarterRealmIdentityPlanningValidation.MapSelectionStatus(
                    revalidated.Status);
            }

            return ReferenceEquals(revalidated.Layout, selectionResult.Layout)
                ? StarterRealmIdentityPlanningStatus.Planned
                : StarterRealmIdentityPlanningStatus.SelectionEvidenceMismatch;
        }

        private static InfernalStarterRealmIdentityPlan Reject(
            StarterRealmIdentityPlanningStatus status,
            InfernalStarterSeededLayoutResult selectionResult,
            StarterRealmIdentityResult identityResult)
        {
            return new InfernalStarterRealmIdentityPlan(
                status,
                selectionResult,
                identityResult,
                null,
                null);
        }
    }

    internal static class StarterRealmIdentityPlanningValidation
    {
        public static StarterRealmIdentityPlanningStatus MapSelectionStatus(
            StarterRealmSeededSelectionStatus status)
        {
            switch (status)
            {
                case StarterRealmSeededSelectionStatus.SelectionResultMissing:
                    return StarterRealmIdentityPlanningStatus.SelectionResultMissing;
                case StarterRealmSeededSelectionStatus.RealmIdMismatch:
                    return StarterRealmIdentityPlanningStatus.RealmIdMismatch;
                case StarterRealmSeededSelectionStatus.SelectionEvidenceMismatch:
                    return StarterRealmIdentityPlanningStatus.SelectionEvidenceMismatch;
                case StarterRealmSeededSelectionStatus.LayoutIdUnknown:
                case StarterRealmSeededSelectionStatus.CatalogueMismatch:
                case StarterRealmSeededSelectionStatus.FamilyCatalogueInvalid:
                    return StarterRealmIdentityPlanningStatus.SelectionCatalogueMismatch;
                default:
                    return StarterRealmIdentityPlanningStatus.SelectionRejected;
            }
        }

        public static StarterRealmIdentityPlanningStatus ValidateRecordAndBytes(
            SeededSelection selection,
            StarterRealmIdentityResult identityResult,
            IReadOnlyList<byte> canonicalBytes,
            out IReadOnlyList<byte> bytesSnapshot)
        {
            bytesSnapshot = SnapshotBytes(canonicalBytes);
            if (identityResult == null)
            {
                return StarterRealmIdentityPlanningStatus.RecordResultMissing;
            }

            if (!identityResult.HasRecord)
            {
                return StarterRealmIdentityPlanningStatus.RecordRejected;
            }

            var record = identityResult.Record;
            var revalidated = StarterRealmIdentityCodec.Create(
                record.Version,
                record.RealmId,
                record.Seed,
                record.LayoutId);
            if (!revalidated.HasRecord)
            {
                return StarterRealmIdentityPlanningStatus.RecordRejected;
            }

            if (selection == null
                || record.Version != StarterRealmIdentityRecordContract.CurrentVersion
                || !string.Equals(record.RealmId, selection.RealmId,
                    StringComparison.Ordinal)
                || record.Seed != selection.Seed
                || !string.Equals(record.LayoutId, selection.LayoutId,
                    StringComparison.Ordinal))
            {
                return StarterRealmIdentityPlanningStatus.RecordFieldMismatch;
            }

            if (canonicalBytes == null)
            {
                return StarterRealmIdentityPlanningStatus.CanonicalBytesMissing;
            }

            var byteArray = ToArray(bytesSnapshot);
            var parsed = StarterRealmIdentityCodec.ParseCanonicalUtf8(byteArray);
            if (!parsed.HasRecord)
            {
                return StarterRealmIdentityPlanningStatus.CanonicalBytesRejected;
            }

            if (!FieldsMatch(parsed.Record, record))
            {
                return StarterRealmIdentityPlanningStatus.CanonicalBytesMismatch;
            }

            var expectedBytes = StarterRealmIdentityCodec.SerializeCanonicalUtf8(record);
            return BytesEqual(expectedBytes, bytesSnapshot)
                ? StarterRealmIdentityPlanningStatus.Planned
                : StarterRealmIdentityPlanningStatus.CanonicalBytesMismatch;
        }

        public static IReadOnlyList<byte> SnapshotBytes(IReadOnlyList<byte> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<byte>());
            }

            var copy = new byte[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }

        private static byte[] ToArray(IReadOnlyList<byte> source)
        {
            var result = new byte[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }

        private static bool FieldsMatch(
            IdentityRecord first,
            IdentityRecord second)
        {
            return first.Version == second.Version
                && string.Equals(first.RealmId, second.RealmId, StringComparison.Ordinal)
                && first.Seed == second.Seed
                && string.Equals(first.LayoutId, second.LayoutId, StringComparison.Ordinal);
        }

        private static bool BytesEqual(
            IReadOnlyList<byte> first,
            IReadOnlyList<byte> second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }

            for (var index = 0; index < first.Count; index++)
            {
                if (first[index] != second[index])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
