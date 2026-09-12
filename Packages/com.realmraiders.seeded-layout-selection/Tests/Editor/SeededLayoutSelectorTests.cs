using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace RealmRaiders.Modules.SeededLayoutSelection.Tests
{
    public sealed class SeededLayoutSelectorTests
    {
        private static readonly string[] FiveLayouts =
        {
            "layout.alpha",
            "layout.beta",
            "layout.gamma",
            "layout.delta",
            "layout.epsilon"
        };

        [Test]
        public void PlatformStableVectors_CoverSignedInt32EdgesAndKnownSeeds()
        {
            AssertSelection(int.MinValue, 4, "layout.epsilon");
            AssertSelection(-1, 1, "layout.beta");
            AssertSelection(0, 2, "layout.gamma");
            AssertSelection(1, 3, "layout.delta");
            AssertSelection(42, 4, "layout.epsilon");
            AssertSelection(int.MaxValue, 4, "layout.epsilon");
        }

        [Test]
        public void Selection_IsDeterministicForFullIntEdgeCases()
        {
            foreach (var seed in new[]
            {
                int.MinValue,
                int.MinValue + 1,
                -1024,
                -1,
                0,
                1,
                1024,
                int.MaxValue - 1,
                int.MaxValue
            })
            {
                var first = SeededLayoutSelector.SelectExact(
                    "realm.sylvan",
                    seed,
                    FiveLayouts);
                var second = SeededLayoutSelector.SelectExact(
                    "realm.sylvan",
                    seed,
                    FiveLayouts);
                Assert.That(first.HasSelection, Is.True);
                Assert.That(second.Selection.LayoutId,
                    Is.EqualTo(first.Selection.LayoutId));
                Assert.That(second.Selection.CatalogueIndex,
                    Is.EqualTo(first.Selection.CatalogueIndex));
                Assert.That(second.Selection.Seed, Is.EqualTo(seed));
            }
        }

        [Test]
        public void BoundedRepresentativeSeedMatrix_ReachesEveryEntryForSizesOneToSixtyFour()
        {
            for (var count = 1; count <= 64; count++)
            {
                var catalogue = Catalogue(count);
                var reached = new HashSet<string>(StringComparer.Ordinal);
                for (var seed = 0; seed < count; seed++)
                {
                    var result = SeededLayoutSelector.SelectExact(
                        "realm.infernal",
                        seed,
                        catalogue);
                    Assert.That(result.HasSelection, Is.True);
                    reached.Add(result.Selection.LayoutId);
                }

                Assert.That(reached.Count, Is.EqualTo(count), "catalogue size " + count);
            }
        }

        [Test]
        public void CallerOrder_IsPreservedAndDeterminesTheSelectedExactEntry()
        {
            var original = new[]
            {
                "layout.alpha",
                "layout.beta",
                "layout.gamma",
                "layout.delta"
            };
            var reordered = new[]
            {
                "layout.delta",
                "layout.gamma",
                "layout.beta",
                "layout.alpha"
            };
            var first = SeededLayoutSelector.SelectExact("realm.sylvan", 19, original);
            var second = SeededLayoutSelector.SelectExact("realm.sylvan", 19, reordered);

            Assert.That(second.Selection.CatalogueIndex,
                Is.EqualTo(first.Selection.CatalogueIndex));
            Assert.That(first.Selection.LayoutId,
                Is.EqualTo(original[first.Selection.CatalogueIndex]));
            Assert.That(second.Selection.LayoutId,
                Is.EqualTo(reordered[second.Selection.CatalogueIndex]));
            CollectionAssert.AreEqual(original, first.Selection.CanonicalLayoutIds);
            CollectionAssert.AreEqual(reordered, second.Selection.CanonicalLayoutIds);
        }

        [Test]
        public void RealmId_ProvidesAStableFactionNeutralOrderRotation()
        {
            var selections = new HashSet<int>();
            foreach (var realmId in new[]
            {
                "realm.sylvan",
                "realm.infernal",
                "realm.frost",
                "realm.arcane"
            })
            {
                var first = SeededLayoutSelector.SelectExact(realmId, 7, FiveLayouts);
                var second = SeededLayoutSelector.SelectExact(realmId, 7, FiveLayouts);
                Assert.That(first.Selection.CatalogueIndex,
                    Is.EqualTo(second.Selection.CatalogueIndex));
                Assert.That(first.Selection.RealmId, Is.EqualTo(realmId));
                selections.Add(first.Selection.CatalogueIndex);
            }

            Assert.That(selections.Count, Is.GreaterThan(1));
        }

        [Test]
        public void Avoidance_IsDeterministicAndNeverSelectsPreviousWhenAlternativesExist()
        {
            foreach (var previousLayoutId in FiveLayouts)
            {
                foreach (var seed in new[]
                {
                    int.MinValue,
                    -1,
                    0,
                    1,
                    37,
                    int.MaxValue
                })
                {
                    var first = SeededLayoutSelector.SelectExactAvoidingPrevious(
                        "realm.sylvan",
                        seed,
                        FiveLayouts,
                        previousLayoutId);
                    var second = SeededLayoutSelector.SelectExactAvoidingPrevious(
                        "realm.sylvan",
                        seed,
                        FiveLayouts,
                        previousLayoutId);

                    Assert.That(first.HasSelection, Is.True);
                    Assert.That(first.Selection.LayoutId, Is.Not.EqualTo(previousLayoutId));
                    Assert.That(first.Selection.PreviousLayoutId,
                        Is.EqualTo(previousLayoutId));
                    Assert.That(first.Selection.PreviousLayoutAvoided, Is.True);
                    Assert.That(first.Selection.UsedSingleLayoutFallback, Is.False);
                    Assert.That(second.Selection.LayoutId,
                        Is.EqualTo(first.Selection.LayoutId));
                    Assert.That(second.Selection.CatalogueIndex,
                        Is.EqualTo(first.Selection.CatalogueIndex));
                }
            }
        }

        [Test]
        public void AvoidanceBoundedSeedMatrix_ReachesEveryAlternativeWithoutRerolling()
        {
            for (var count = 2; count <= 64; count++)
            {
                var catalogue = Catalogue(count);
                var previous = catalogue[count / 2];
                var reached = new HashSet<string>(StringComparer.Ordinal);
                for (var seed = 0; seed < count - 1; seed++)
                {
                    var result = SeededLayoutSelector.SelectExactAvoidingPrevious(
                        "realm.infernal",
                        seed,
                        catalogue,
                        previous);
                    Assert.That(result.Selection.LayoutId, Is.Not.EqualTo(previous));
                    reached.Add(result.Selection.LayoutId);
                }

                Assert.That(reached.Count, Is.EqualTo(count - 1),
                    "catalogue size " + count);
            }
        }

        [Test]
        public void OneLayoutAvoidance_HasExplicitDeterministicFallback()
        {
            var catalogue = new[] { "layout.only" };
            foreach (var seed in new[] { int.MinValue, -1, 0, 1, int.MaxValue })
            {
                var result = SeededLayoutSelector.SelectExactAvoidingPrevious(
                    "realm.sylvan",
                    seed,
                    catalogue,
                    "layout.only");

                Assert.That(result.HasSelection, Is.True);
                Assert.That(result.Selection.LayoutId, Is.EqualTo("layout.only"));
                Assert.That(result.Selection.CatalogueIndex, Is.EqualTo(0));
                Assert.That(result.Selection.PreviousLayoutAvoided, Is.False);
                Assert.That(result.Selection.UsedSingleLayoutFallback, Is.True);
            }
        }

        [Test]
        public void SelectionSnapshot_IsDetachedImmutableAndRetainsExactSeed()
        {
            var catalogue = new List<string>
            {
                "layout.alpha",
                "layout.beta",
                "layout.gamma"
            };
            var result = SeededLayoutSelector.SelectExact(
                "realm.sylvan",
                int.MinValue,
                catalogue);
            catalogue.Clear();

            Assert.That(result.Selection.Seed, Is.EqualTo(int.MinValue));
            Assert.That(result.Selection.CanonicalLayoutIds.Count, Is.EqualTo(3));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)result.Selection.CanonicalLayoutIds).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() => ((IList)result.Evidence).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList)result.Issues).Clear());
        }

        [Test]
        public void NullAndEmptyCatalogue_FailClosedWithExactEvidence()
        {
            AssertEvidence(
                SeededLayoutSelector.SelectExact("realm.sylvan", 0, null),
                Expected(SeededLayoutSelectionIssue.CatalogueMissing, -1, null));
            AssertEvidence(
                SeededLayoutSelector.SelectExact(
                    "realm.sylvan",
                    0,
                    Array.Empty<string>()),
                Expected(SeededLayoutSelectionIssue.CatalogueEmpty, -1, null));
        }

        [Test]
        public void InvalidRealmIds_FailClosedAsMissingMalformedOrNonCanonical()
        {
            AssertSingleIssue(null, SeededLayoutSelectionIssue.RealmIdMissing);
            AssertSingleIssue(string.Empty, SeededLayoutSelectionIssue.RealmIdMissing);
            AssertSingleIssue("realm..sylvan", SeededLayoutSelectionIssue.RealmIdMalformed);
            AssertSingleIssue("realm_sylvan", SeededLayoutSelectionIssue.RealmIdMalformed);
            AssertSingleIssue("Realm.Sylvan", SeededLayoutSelectionIssue.RealmIdNonCanonical);
            AssertSingleIssue(" realm.sylvan ", SeededLayoutSelectionIssue.RealmIdNonCanonical);
        }

        [Test]
        public void InvalidCatalogueEvidence_PreservesOccurrenceOrderAndExactIndexes()
        {
            var catalogue = new string[]
            {
                null,
                " Layout.One ",
                "layout..broken",
                "layout.alpha",
                "layout.alpha",
                "Layout.Beta"
            };
            var result = SeededLayoutSelector.SelectExact("realm.sylvan", 0, catalogue);

            AssertEvidence(
                result,
                Expected(SeededLayoutSelectionIssue.LayoutIdMissing, 0, null),
                Expected(SeededLayoutSelectionIssue.LayoutIdNonCanonical,
                    1, " Layout.One "),
                Expected(SeededLayoutSelectionIssue.LayoutIdMalformed,
                    2, "layout..broken"),
                Expected(SeededLayoutSelectionIssue.LayoutIdDuplicate,
                    4, "layout.alpha"),
                Expected(SeededLayoutSelectionIssue.LayoutIdNonCanonical,
                    5, "Layout.Beta"));
        }

        [Test]
        public void MalformedLayoutBoundariesAndCharacters_FailClosed()
        {
            var tooLong = "layout." + new string('a', 100);
            foreach (var malformed in new[]
            {
                "layout",
                ".layout",
                "layout.",
                "layout.-alpha",
                "layout.alpha-",
                "layout.alpha/beta",
                "layout_alpha",
                tooLong
            })
            {
                var result = SeededLayoutSelector.SelectExact(
                    "realm.sylvan",
                    0,
                    new[] { malformed });
                Assert.That(result.Issues,
                    Is.EqualTo(new[] { SeededLayoutSelectionIssue.LayoutIdMalformed }),
                    malformed);
            }
        }

        [Test]
        public void AvoidanceRejectsMissingMalformedNonCanonicalAndUnknownPreviousIds()
        {
            AssertPreviousIssue(null, SeededLayoutSelectionIssue.PreviousLayoutIdMissing);
            AssertPreviousIssue("layout..alpha",
                SeededLayoutSelectionIssue.PreviousLayoutIdMalformed);
            AssertPreviousIssue("Layout.Alpha",
                SeededLayoutSelectionIssue.PreviousLayoutIdNonCanonical);
            AssertPreviousIssue("layout.unknown",
                SeededLayoutSelectionIssue.PreviousLayoutIdUnknown);
        }

        [Test]
        public void RejectedEvidenceAndIssueViews_AreImmutableAndOrdered()
        {
            var result = SeededLayoutSelector.SelectExact(
                null,
                0,
                new string[] { null, "Layout.Alpha" });

            CollectionAssert.AreEqual(
                new[]
                {
                    SeededLayoutSelectionIssue.RealmIdMissing,
                    SeededLayoutSelectionIssue.LayoutIdMissing,
                    SeededLayoutSelectionIssue.LayoutIdNonCanonical
                },
                result.Issues);
            Assert.Throws<NotSupportedException>(() => ((IList)result.Evidence).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList)result.Issues).Clear());
            Assert.That(result.Selection, Is.Null);
            Assert.That(result.HasSelection, Is.False);
        }

        private static void AssertSelection(int seed, int index, string layoutId)
        {
            var result = SeededLayoutSelector.SelectExact(
                "realm.sylvan",
                seed,
                FiveLayouts);
            Assert.That(result.Status, Is.EqualTo(SeededLayoutSelectionStatus.Selected));
            Assert.That(result.Selection.CatalogueIndex, Is.EqualTo(index));
            Assert.That(result.Selection.LayoutId, Is.EqualTo(layoutId));
        }

        private static void AssertSingleIssue(
            string realmId,
            SeededLayoutSelectionIssue issue)
        {
            var result = SeededLayoutSelector.SelectExact(
                realmId,
                0,
                new[] { "layout.alpha" });
            Assert.That(result.Issues, Is.EqualTo(new[] { issue }));
        }

        private static void AssertPreviousIssue(
            string previousLayoutId,
            SeededLayoutSelectionIssue issue)
        {
            var result = SeededLayoutSelector.SelectExactAvoidingPrevious(
                "realm.sylvan",
                0,
                FiveLayouts,
                previousLayoutId);
            Assert.That(result.Issues, Is.EqualTo(new[] { issue }));
        }

        private static IReadOnlyList<string> Catalogue(int count)
        {
            var result = new string[count];
            for (var index = 0; index < count; index++)
            {
                result[index] = "layout.option-" + index;
            }

            return result;
        }

        private static void AssertEvidence(
            SeededLayoutSelectionResult result,
            params ExpectedEvidence[] expected)
        {
            Assert.That(result.Status, Is.EqualTo(SeededLayoutSelectionStatus.Rejected));
            Assert.That(result.HasSelection, Is.False);
            Assert.That(result.Selection, Is.Null);
            Assert.That(result.Evidence.Count, Is.EqualTo(expected.Length));
            for (var index = 0; index < expected.Length; index++)
            {
                Assert.That(result.Evidence[index].Issue, Is.EqualTo(expected[index].Issue));
                Assert.That(result.Evidence[index].CatalogueIndex,
                    Is.EqualTo(expected[index].CatalogueIndex));
                Assert.That(result.Evidence[index].SuppliedValue,
                    Is.EqualTo(expected[index].SuppliedValue));
                Assert.That(result.Issues[index], Is.EqualTo(expected[index].Issue));
            }
        }

        private static ExpectedEvidence Expected(
            SeededLayoutSelectionIssue issue,
            int catalogueIndex,
            string suppliedValue)
        {
            return new ExpectedEvidence(issue, catalogueIndex, suppliedValue);
        }

        private sealed class ExpectedEvidence
        {
            public ExpectedEvidence(
                SeededLayoutSelectionIssue issue,
                int catalogueIndex,
                string suppliedValue)
            {
                Issue = issue;
                CatalogueIndex = catalogueIndex;
                SuppliedValue = suppliedValue;
            }

            public SeededLayoutSelectionIssue Issue { get; }
            public int CatalogueIndex { get; }
            public string SuppliedValue { get; }
        }
    }
}
