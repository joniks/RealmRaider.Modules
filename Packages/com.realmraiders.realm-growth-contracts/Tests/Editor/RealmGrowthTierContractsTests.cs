using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace RealmRaiders.Modules.RealmGrowthContracts.Tests
{
    public sealed class RealmGrowthTierContractsTests
    {
        [Test]
        public void ResolveLevel_UsesExactGreatestSatisfiedMinimumLevel()
        {
            var catalogue = ValidCatalogue();

            AssertResolved(catalogue, 0, "tier.one");
            AssertResolved(catalogue, 4, "tier.one");
            AssertResolved(catalogue, 5, "tier.two");
            AssertResolved(catalogue, 8, "tier.two");
            AssertResolved(catalogue, 9, "tier.three");
            AssertResolved(catalogue, 100, "tier.three");
        }

        [Test]
        public void ResolveLevel_FailsClosedForNegativeAndBelowFirstLevel()
        {
            var catalogue = new RealmGrowthTierCatalogue(
                new RealmGrowthTier[]
                {
                    new RealmGrowthTier("tier.one", 3, 8, 3, 0)
                });

            var negative = RealmGrowthTierContracts.ResolveLevel(catalogue, -1);
            var belowFirst = RealmGrowthTierContracts.ResolveLevel(catalogue, 2);

            Assert.That(negative.Status, Is.EqualTo(RealmGrowthTierResolutionStatus.LevelNegative));
            Assert.That(negative.Tier, Is.Null);
            Assert.That(belowFirst.Status,
                Is.EqualTo(RealmGrowthTierResolutionStatus.LevelBelowFirstTier));
            Assert.That(belowFirst.Tier, Is.Null);
        }

        [Test]
        public void FindByTierId_UsesExactOrdinalLookup()
        {
            var catalogue = ValidCatalogue();
            var found = RealmGrowthTierContracts.FindByTierId(catalogue, "tier.two");
            var unknown = RealmGrowthTierContracts.FindByTierId(catalogue, "tier.unknown");
            var caseChanged = RealmGrowthTierContracts.FindByTierId(catalogue, "TIER.TWO");
            var padded = RealmGrowthTierContracts.FindByTierId(catalogue, "tier.two ");

            Assert.That(found.Status, Is.EqualTo(RealmGrowthTierLookupStatus.Found));
            Assert.That(found.Tier, Is.SameAs(catalogue.Tiers[1]));
            Assert.That(unknown.Status, Is.EqualTo(RealmGrowthTierLookupStatus.NotFound));
            Assert.That(caseChanged.Status, Is.EqualTo(RealmGrowthTierLookupStatus.InvalidTierId));
            Assert.That(padded.Status, Is.EqualTo(RealmGrowthTierLookupStatus.InvalidTierId));
        }

        [Test]
        public void Validate_RejectsDuplicateUnorderedAndNonPhysicalTierFactsInStableOrder()
        {
            var catalogue = new RealmGrowthTierCatalogue(
                new RealmGrowthTier[]
                {
                    new RealmGrowthTier("tier.one", 5, 8, 3, 1),
                    new RealmGrowthTier("tier.one", 5, 7, 2, -1),
                    new RealmGrowthTier("tier.three", -1, 0, 0, 0)
                });

            var result = RealmGrowthTierContracts.Validate(catalogue);

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmGrowthTierValidationIssue.TierIdDuplicate,
                    RealmGrowthTierValidationIssue.MinimumLevelNotStrictlyIncreasing,
                    RealmGrowthTierValidationIssue.FootprintBudgetDecreases,
                    RealmGrowthTierValidationIssue.NodeBudgetDecreases,
                    RealmGrowthTierValidationIssue.ExpansionAnchorCapacityNegative,
                    RealmGrowthTierValidationIssue.MinimumLevelNegative,
                    RealmGrowthTierValidationIssue.FootprintBudgetNonPositive,
                    RealmGrowthTierValidationIssue.NodeBudgetNonPositive
                },
                result.Issues);
        }

        [Test]
        public void Validate_RejectsMissingCatalogueAndTier()
        {
            var missingCatalogue = RealmGrowthTierContracts.Validate(null);
            var missingTier = RealmGrowthTierContracts.Validate(
                new RealmGrowthTierCatalogue(new RealmGrowthTier[] { null }));

            CollectionAssert.AreEqual(
                new[]
                {
                    RealmGrowthTierValidationIssue.CatalogueMissing
                },
                missingCatalogue.Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmGrowthTierValidationIssue.TierMissing
                },
                missingTier.Issues);
        }

        [Test]
        public void ResolveAndLookup_RejectInvalidCatalogueWithOrderedEvidence()
        {
            var invalidCatalogue = new RealmGrowthTierCatalogue(
                new RealmGrowthTier[]
                {
                    new RealmGrowthTier("tier.one", 0, 0, 1, 0)
                });

            var resolution = RealmGrowthTierContracts.ResolveLevel(invalidCatalogue, 0);
            var lookup = RealmGrowthTierContracts.FindByTierId(invalidCatalogue, "tier.one");

            Assert.That(resolution.Status,
                Is.EqualTo(RealmGrowthTierResolutionStatus.CatalogueInvalid));
            Assert.That(lookup.Status, Is.EqualTo(RealmGrowthTierLookupStatus.CatalogueInvalid));
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmGrowthTierValidationIssue.FootprintBudgetNonPositive
                },
                resolution.Validation.Issues);
            CollectionAssert.AreEqual(resolution.Validation.Issues, lookup.Validation.Issues);
        }

        [Test]
        public void CatalogueAndResultsSnapshotCallerDataAndRemainImmutable()
        {
            var tiers = new[]
            {
                new RealmGrowthTier("tier.one", 0, 8, 3, 0),
                new RealmGrowthTier("tier.two", 5, 8, 5, 2)
            };
            var catalogue = new RealmGrowthTierCatalogue(tiers);
            var first = RealmGrowthTierContracts.ResolveLevel(catalogue, 5);
            var second = RealmGrowthTierContracts.ResolveLevel(catalogue, 5);
            var invalid = RealmGrowthTierContracts.ResolveLevel(
                new RealmGrowthTierCatalogue(
                    new RealmGrowthTier[]
                    {
                        new RealmGrowthTier("tier.invalid", 0, 0, 1, 0)
                    }),
                0);

            tiers[1] = null;

            Assert.That(catalogue.Tiers[1], Is.Not.Null);
            Assert.That(first.Tier, Is.SameAs(second.Tier));
            CollectionAssert.AreEqual(first.Validation.Issues, second.Validation.Issues);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmGrowthTier>)catalogue.Tiers)[0] = null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmGrowthTierValidationIssue>)invalid.Validation.Issues)[0] =
                    RealmGrowthTierValidationIssue.CatalogueMissing);
        }

        private static RealmGrowthTierCatalogue ValidCatalogue()
        {
            return new RealmGrowthTierCatalogue(
                new RealmGrowthTier[]
                {
                    new RealmGrowthTier("tier.one", 0, 8, 3, 0),
                    new RealmGrowthTier("tier.two", 5, 8, 5, 2),
                    new RealmGrowthTier("tier.three", 9, 12, 5, 2)
                });
        }

        private static void AssertResolved(
            RealmGrowthTierCatalogue catalogue,
            int level,
            string expectedTierId)
        {
            var result = RealmGrowthTierContracts.ResolveLevel(catalogue, level);

            Assert.That(result.Resolved, Is.True);
            Assert.That(result.Status, Is.EqualTo(RealmGrowthTierResolutionStatus.Resolved));
            Assert.That(result.Tier.TierId, Is.EqualTo(expectedTierId));
            Assert.That(result.Validation.IsValid, Is.True);
        }
    }
}
