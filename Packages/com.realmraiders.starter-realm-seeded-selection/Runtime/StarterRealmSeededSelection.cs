using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.RealmLayoutContracts;
using RealmRaiders.Modules.SeededLayoutSelection;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.StarterRealmSeededSelection
{
    public enum StarterRealmSeededSelectionStatus
    {
        Selected,
        SelectionResultMissing,
        SelectionRejected,
        RealmIdMismatch,
        LayoutIdUnknown,
        SelectionEvidenceMismatch,
        CatalogueMismatch,
        FamilyCatalogueInvalid
    }

    public static class StarterRealmSeededCatalogues
    {
        public const string SylvanRealmId = "realmraiders.realm.sylvan";

        public const string InfernalRealmId = "realmraiders.realm.infernal";

        public static IReadOnlyList<string> SylvanLayoutIds { get; } =
            SnapshotSylvanIds();

        public static IReadOnlyList<string> InfernalLayoutIds { get; } =
            SnapshotInfernalIds();

        private static IReadOnlyList<string> SnapshotSylvanIds()
        {
            var ids = new string[StarterSylvanRealmLayouts.All.Count];
            for (var index = 0; index < ids.Length; index++)
            {
                ids[index] = StarterSylvanRealmLayouts.All[index].LayoutId;
            }

            return Array.AsReadOnly(ids);
        }

        private static IReadOnlyList<string> SnapshotInfernalIds()
        {
            var ids = new string[StarterInfernalDefenseLayouts.All.Count];
            for (var index = 0; index < ids.Length; index++)
            {
                ids[index] = StarterInfernalDefenseLayouts.All[index].LayoutId;
            }

            return Array.AsReadOnly(ids);
        }
    }

    public sealed class SylvanStarterSeededLayoutResult
    {
        internal SylvanStarterSeededLayoutResult(
            StarterRealmSeededSelectionStatus status,
            SeededLayoutSelectionResult selectionResult,
            RealmLayoutRecipe recipe)
        {
            Status = status;
            SelectionResult = selectionResult;
            Recipe = recipe;
        }

        public StarterRealmSeededSelectionStatus Status { get; }

        public SeededLayoutSelectionResult SelectionResult { get; }

        public RealmLayoutRecipe Recipe { get; }

        public bool HasRecipe =>
            Status == StarterRealmSeededSelectionStatus.Selected
            && SelectionResult != null
            && SelectionResult.HasSelection
            && Recipe != null;
    }

    public sealed class InfernalStarterSeededLayoutResult
    {
        internal InfernalStarterSeededLayoutResult(
            StarterRealmSeededSelectionStatus status,
            SeededLayoutSelectionResult selectionResult,
            RealmLayoutGraph layout)
        {
            Status = status;
            SelectionResult = selectionResult;
            Layout = layout;
        }

        public StarterRealmSeededSelectionStatus Status { get; }

        public SeededLayoutSelectionResult SelectionResult { get; }

        public RealmLayoutGraph Layout { get; }

        public bool HasLayout =>
            Status == StarterRealmSeededSelectionStatus.Selected
            && SelectionResult != null
            && SelectionResult.HasSelection
            && Layout != null;
    }

    public static class SylvanStarterSeededLayoutSelector
    {
        public static SylvanStarterSeededLayoutResult SelectExact(
            string realmId,
            int seed)
        {
            return ResolveExactSelection(
                SeededLayoutSelector.SelectExact(
                    realmId,
                    seed,
                    StarterRealmSeededCatalogues.SylvanLayoutIds));
        }

        public static SylvanStarterSeededLayoutResult ResolveExactSelection(
            SeededLayoutSelectionResult selectionResult)
        {
            var status = ValidateSelection(
                selectionResult,
                StarterRealmSeededCatalogues.SylvanRealmId,
                StarterRealmSeededCatalogues.SylvanLayoutIds);
            if (status != StarterRealmSeededSelectionStatus.Selected)
            {
                return new SylvanStarterSeededLayoutResult(
                    status,
                    selectionResult,
                    null);
            }

            var resolved = StarterSylvanRealmLayoutResolver.ResolveExact(
                selectionResult.Selection.LayoutId);
            if (resolved.Status == RealmLayoutResolveStatus.CatalogueInvalid)
            {
                return new SylvanStarterSeededLayoutResult(
                    StarterRealmSeededSelectionStatus.FamilyCatalogueInvalid,
                    selectionResult,
                    null);
            }

            if (!resolved.HasRecipe)
            {
                return new SylvanStarterSeededLayoutResult(
                    StarterRealmSeededSelectionStatus.LayoutIdUnknown,
                    selectionResult,
                    null);
            }

            return new SylvanStarterSeededLayoutResult(
                StarterRealmSeededSelectionStatus.Selected,
                selectionResult,
                resolved.Recipe);
        }

        private static StarterRealmSeededSelectionStatus ValidateSelection(
            SeededLayoutSelectionResult selectionResult,
            string expectedRealmId,
            IReadOnlyList<string> expectedLayoutIds)
        {
            return StarterRealmSeededSelectionEvidence.Validate(
                selectionResult,
                expectedRealmId,
                expectedLayoutIds);
        }
    }

    public static class InfernalStarterSeededLayoutSelector
    {
        public static InfernalStarterSeededLayoutResult SelectExact(
            string realmId,
            int seed)
        {
            return ResolveExactSelection(
                SeededLayoutSelector.SelectExact(
                    realmId,
                    seed,
                    StarterRealmSeededCatalogues.InfernalLayoutIds));
        }

        public static InfernalStarterSeededLayoutResult ResolveExactSelection(
            SeededLayoutSelectionResult selectionResult)
        {
            var status = StarterRealmSeededSelectionEvidence.Validate(
                selectionResult,
                StarterRealmSeededCatalogues.InfernalRealmId,
                StarterRealmSeededCatalogues.InfernalLayoutIds);
            if (status != StarterRealmSeededSelectionStatus.Selected)
            {
                return new InfernalStarterSeededLayoutResult(
                    status,
                    selectionResult,
                    null);
            }

            var resolved = StarterInfernalDefenseLayoutResolver.ResolveExact(
                selectionResult.Selection.LayoutId);
            if (resolved.Status == InfernalDefenseLayoutResolveStatus.CatalogueInvalid)
            {
                return new InfernalStarterSeededLayoutResult(
                    StarterRealmSeededSelectionStatus.FamilyCatalogueInvalid,
                    selectionResult,
                    null);
            }

            if (!resolved.HasLayout)
            {
                return new InfernalStarterSeededLayoutResult(
                    StarterRealmSeededSelectionStatus.LayoutIdUnknown,
                    selectionResult,
                    null);
            }

            return new InfernalStarterSeededLayoutResult(
                StarterRealmSeededSelectionStatus.Selected,
                selectionResult,
                resolved.Layout);
        }
    }

    internal static class StarterRealmSeededSelectionEvidence
    {
        public static StarterRealmSeededSelectionStatus Validate(
            SeededLayoutSelectionResult selectionResult,
            string expectedRealmId,
            IReadOnlyList<string> expectedLayoutIds)
        {
            if (selectionResult == null)
            {
                return StarterRealmSeededSelectionStatus.SelectionResultMissing;
            }

            if (!selectionResult.HasSelection)
            {
                return StarterRealmSeededSelectionStatus.SelectionRejected;
            }

            var selection = selectionResult.Selection;
            if (!string.Equals(selection.RealmId, expectedRealmId, StringComparison.Ordinal))
            {
                return StarterRealmSeededSelectionStatus.RealmIdMismatch;
            }

            var expectedIndex = IndexOf(expectedLayoutIds, selection.LayoutId);
            if (expectedIndex < 0)
            {
                return StarterRealmSeededSelectionStatus.LayoutIdUnknown;
            }

            if (selection.CatalogueIndex != expectedIndex
                || selection.PreviousLayoutId != null
                || selection.PreviousLayoutAvoided
                || selection.UsedSingleLayoutFallback)
            {
                return StarterRealmSeededSelectionStatus.SelectionEvidenceMismatch;
            }

            if (!HasExactOrder(selection.CanonicalLayoutIds, expectedLayoutIds))
            {
                return StarterRealmSeededSelectionStatus.CatalogueMismatch;
            }

            return StarterRealmSeededSelectionStatus.Selected;
        }

        private static int IndexOf(IReadOnlyList<string> values, string expected)
        {
            for (var index = 0; index < values.Count; index++)
            {
                if (string.Equals(values[index], expected, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool HasExactOrder(
            IReadOnlyList<string> actual,
            IReadOnlyList<string> expected)
        {
            if (actual == null || actual.Count != expected.Count)
            {
                return false;
            }

            for (var index = 0; index < expected.Count; index++)
            {
                if (!string.Equals(actual[index], expected[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
