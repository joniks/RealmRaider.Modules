using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.SeededLayoutSelection;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.StarterRealmSeededSelection.Tests
{
    public sealed class StarterRealmSeededSelectionTests
    {
        [Test]
        public void Catalogues_PreserveExactAuthoredFamilyOrderAndAreImmutable()
        {
            AssertExactIds(
                StarterRealmSeededCatalogues.SylvanLayoutIds,
                StarterSylvanRealmLayouts.AncientCrossroadsId,
                StarterSylvanRealmLayouts.ForkedCanopyId,
                StarterSylvanRealmLayouts.SerpentRootsId);
            AssertExactIds(
                StarterRealmSeededCatalogues.InfernalLayoutIds,
                StarterInfernalDefenseLayouts.AshenSpurId,
                StarterInfernalDefenseLayouts.CinderForkId,
                StarterInfernalDefenseLayouts.EmberCircuitId);
            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterRealmSeededCatalogues.SylvanLayoutIds).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterRealmSeededCatalogues.InfernalLayoutIds).RemoveAt(0));
        }

        [Test]
        public void Sylvan_AllLayoutsResolveToExactCachedRecipeIdentity()
        {
            foreach (var expected in StarterSylvanRealmLayouts.All)
            {
                var result = FindSylvan(expected.LayoutId);

                Assert.That(result.Status,
                    Is.EqualTo(StarterRealmSeededSelectionStatus.Selected));
                Assert.That(result.HasRecipe, Is.True);
                Assert.That(result.Recipe, Is.SameAs(expected));
                Assert.That(result.SelectionResult.Selection.LayoutId,
                    Is.EqualTo(expected.LayoutId));
            }
        }

        [Test]
        public void Infernal_AllLayoutsResolveToExactCachedRecipeIdentity()
        {
            foreach (var expected in StarterInfernalDefenseLayouts.All)
            {
                var result = FindInfernal(expected.LayoutId);

                Assert.That(result.Status,
                    Is.EqualTo(StarterRealmSeededSelectionStatus.Selected));
                Assert.That(result.HasLayout, Is.True);
                Assert.That(result.Layout, Is.SameAs(expected));
                Assert.That(result.SelectionResult.Selection.LayoutId,
                    Is.EqualTo(expected.LayoutId));
            }
        }

        [Test]
        public void SignedSeedEdges_AreDeterministicAndPreserveSelectionEvidenceIdentity()
        {
            foreach (var seed in new[]
            {
                int.MinValue,
                int.MinValue + 1,
                -1,
                0,
                1,
                int.MaxValue - 1,
                int.MaxValue
            })
            {
                var sylvanFirst = SylvanStarterSeededLayoutSelector.SelectExact(
                    StarterRealmSeededCatalogues.SylvanRealmId,
                    seed);
                var sylvanSecond = SylvanStarterSeededLayoutSelector.SelectExact(
                    StarterRealmSeededCatalogues.SylvanRealmId,
                    seed);
                var infernalFirst = InfernalStarterSeededLayoutSelector.SelectExact(
                    StarterRealmSeededCatalogues.InfernalRealmId,
                    seed);
                var infernalSecond = InfernalStarterSeededLayoutSelector.SelectExact(
                    StarterRealmSeededCatalogues.InfernalRealmId,
                    seed);

                Assert.That(sylvanSecond.Recipe, Is.SameAs(sylvanFirst.Recipe));
                Assert.That(infernalSecond.Layout, Is.SameAs(infernalFirst.Layout));
                Assert.That(sylvanFirst.SelectionResult.Selection.Seed, Is.EqualTo(seed));
                Assert.That(infernalFirst.SelectionResult.Selection.Seed, Is.EqualTo(seed));
                Assert.That(sylvanFirst.SelectionResult.Selection.CanonicalLayoutIds,
                    Is.Not.SameAs(StarterRealmSeededCatalogues.SylvanLayoutIds));
                Assert.That(infernalFirst.SelectionResult.Selection.CanonicalLayoutIds,
                    Is.Not.SameAs(StarterRealmSeededCatalogues.InfernalLayoutIds));
            }
        }

        [Test]
        public void CrossRealmSelections_AreRejectedWithoutRecipe()
        {
            var infernalEvidence = SeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.InfernalRealmId,
                0,
                StarterRealmSeededCatalogues.SylvanLayoutIds);
            var sylvanEvidence = SeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.SylvanRealmId,
                0,
                StarterRealmSeededCatalogues.InfernalLayoutIds);

            AssertSylvanRejected(
                SylvanStarterSeededLayoutSelector.ResolveExactSelection(infernalEvidence),
                StarterRealmSeededSelectionStatus.RealmIdMismatch);
            AssertInfernalRejected(
                InfernalStarterSeededLayoutSelector.ResolveExactSelection(sylvanEvidence),
                StarterRealmSeededSelectionStatus.RealmIdMismatch);
        }

        [Test]
        public void MissingAndRejectedSelectionResults_FailClosed()
        {
            AssertSylvanRejected(
                SylvanStarterSeededLayoutSelector.ResolveExactSelection(null),
                StarterRealmSeededSelectionStatus.SelectionResultMissing);
            AssertInfernalRejected(
                InfernalStarterSeededLayoutSelector.ResolveExactSelection(null),
                StarterRealmSeededSelectionStatus.SelectionResultMissing);

            var rejected = SeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.SylvanRealmId,
                0,
                Array.Empty<string>());
            var result = SylvanStarterSeededLayoutSelector.ResolveExactSelection(rejected);
            AssertSylvanRejected(result, StarterRealmSeededSelectionStatus.SelectionRejected);
            Assert.That(result.SelectionResult, Is.SameAs(rejected));
        }

        [Test]
        public void UnknownLayoutIds_AreRejectedBeforeCatalogueMismatch()
        {
            var sylvanUnknown = SeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.SylvanRealmId,
                0,
                new[] { "realmraiders.sylvan-layout.unknown" });
            var infernalUnknown = SeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.InfernalRealmId,
                0,
                new[] { "realmraiders.infernal-defense.unknown" });

            AssertSylvanRejected(
                SylvanStarterSeededLayoutSelector.ResolveExactSelection(sylvanUnknown),
                StarterRealmSeededSelectionStatus.LayoutIdUnknown);
            AssertInfernalRejected(
                InfernalStarterSeededLayoutSelector.ResolveExactSelection(infernalUnknown),
                StarterRealmSeededSelectionStatus.LayoutIdUnknown);
        }

        [Test]
        public void ReorderedCatalogueEvidence_IsRejectedWhenSelectedIndexChangesMeaning()
        {
            var reordered = new[]
            {
                StarterSylvanRealmLayouts.ForkedCanopyId,
                StarterSylvanRealmLayouts.AncientCrossroadsId,
                StarterSylvanRealmLayouts.SerpentRootsId
            };
            var evidence = SelectForIndex(
                StarterRealmSeededCatalogues.SylvanRealmId,
                reordered,
                0);

            AssertSylvanRejected(
                SylvanStarterSeededLayoutSelector.ResolveExactSelection(evidence),
                StarterRealmSeededSelectionStatus.SelectionEvidenceMismatch);
        }

        [Test]
        public void CatalogueMismatch_IsRejectedEvenWhenSelectedEntryAndIndexMatch()
        {
            var altered = new[]
            {
                StarterSylvanRealmLayouts.AncientCrossroadsId,
                "realmraiders.sylvan-layout.unknown",
                StarterSylvanRealmLayouts.SerpentRootsId
            };
            var evidence = SelectForIndex(
                StarterRealmSeededCatalogues.SylvanRealmId,
                altered,
                0);

            AssertSylvanRejected(
                SylvanStarterSeededLayoutSelector.ResolveExactSelection(evidence),
                StarterRealmSeededSelectionStatus.CatalogueMismatch);
        }

        [Test]
        public void PreviousAvoidanceEvidence_IsNotAcceptedAsExactSelectionEvidence()
        {
            var evidence = SeededLayoutSelector.SelectExactAvoidingPrevious(
                StarterRealmSeededCatalogues.InfernalRealmId,
                0,
                StarterRealmSeededCatalogues.InfernalLayoutIds,
                StarterInfernalDefenseLayouts.AshenSpurId);

            AssertInfernalRejected(
                InfernalStarterSeededLayoutSelector.ResolveExactSelection(evidence),
                StarterRealmSeededSelectionStatus.SelectionEvidenceMismatch);
        }

        [Test]
        public void PublicSelectors_RejectWrongAndMalformedRealmIdsThroughExactEvidence()
        {
            AssertSylvanRejected(
                SylvanStarterSeededLayoutSelector.SelectExact(
                    StarterRealmSeededCatalogues.InfernalRealmId,
                    7),
                StarterRealmSeededSelectionStatus.RealmIdMismatch);
            AssertInfernalRejected(
                InfernalStarterSeededLayoutSelector.SelectExact(
                    StarterRealmSeededCatalogues.SylvanRealmId,
                    7),
                StarterRealmSeededSelectionStatus.RealmIdMismatch);
            AssertSylvanRejected(
                SylvanStarterSeededLayoutSelector.SelectExact("Realm.Sylvan", 7),
                StarterRealmSeededSelectionStatus.SelectionRejected);
        }

        private static SylvanStarterSeededLayoutResult FindSylvan(string layoutId)
        {
            for (var seed = 0; seed < 16; seed++)
            {
                var result = SylvanStarterSeededLayoutSelector.SelectExact(
                    StarterRealmSeededCatalogues.SylvanRealmId,
                    seed);
                if (string.Equals(result.Recipe.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return result;
                }
            }

            Assert.Fail("No bounded Sylvan seed selected " + layoutId);
            return null;
        }

        private static InfernalStarterSeededLayoutResult FindInfernal(string layoutId)
        {
            for (var seed = 0; seed < 16; seed++)
            {
                var result = InfernalStarterSeededLayoutSelector.SelectExact(
                    StarterRealmSeededCatalogues.InfernalRealmId,
                    seed);
                if (string.Equals(result.Layout.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return result;
                }
            }

            Assert.Fail("No bounded Infernal seed selected " + layoutId);
            return null;
        }

        private static SeededLayoutSelectionResult SelectForIndex(
            string realmId,
            IReadOnlyList<string> catalogue,
            int expectedIndex)
        {
            for (var seed = 0; seed < 16; seed++)
            {
                var result = SeededLayoutSelector.SelectExact(realmId, seed, catalogue);
                if (result.Selection.CatalogueIndex == expectedIndex)
                {
                    return result;
                }
            }

            Assert.Fail("No bounded seed selected index " + expectedIndex);
            return null;
        }

        private static void AssertSylvanRejected(
            SylvanStarterSeededLayoutResult result,
            StarterRealmSeededSelectionStatus status)
        {
            Assert.That(result.Status, Is.EqualTo(status));
            Assert.That(result.HasRecipe, Is.False);
            Assert.That(result.Recipe, Is.Null);
        }

        private static void AssertInfernalRejected(
            InfernalStarterSeededLayoutResult result,
            StarterRealmSeededSelectionStatus status)
        {
            Assert.That(result.Status, Is.EqualTo(status));
            Assert.That(result.HasLayout, Is.False);
            Assert.That(result.Layout, Is.Null);
        }

        private static void AssertExactIds(
            IReadOnlyList<string> actual,
            params string[] expected)
        {
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
