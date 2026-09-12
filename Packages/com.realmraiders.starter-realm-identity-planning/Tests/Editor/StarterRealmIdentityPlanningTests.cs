using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.SeededLayoutSelection;
using RealmRaiders.Modules.StarterRealmIdentityRecord;
using RealmRaiders.Modules.StarterRealmLayouts;
using RealmRaiders.Modules.StarterRealmSeededSelection;
using IdentityRecord = RealmRaiders.Modules.StarterRealmIdentityRecord.StarterRealmIdentityRecord;
using SeededSelection = RealmRaiders.Modules.SeededLayoutSelection.SeededLayoutSelection;

namespace RealmRaiders.Modules.StarterRealmIdentityPlanning.Tests
{
    public sealed class StarterRealmIdentityPlanningTests
    {
        [Test]
        public void Sylvan_AllAuthoredLayoutsProduceExactCachedRecipesAndCanonicalRecords()
        {
            foreach (var expected in StarterSylvanRealmLayouts.All)
            {
                var plan = FindSylvan(expected.LayoutId);

                Assert.That(plan.HasPlan, Is.True);
                Assert.That(plan.Recipe, Is.SameAs(expected));
                AssertRecordMatchesSelection(
                    plan.SelectionResult.SelectionResult.Selection,
                    plan.IdentityResult.Record,
                    plan.CanonicalBytes);
            }
        }

        [Test]
        public void Infernal_AllAuthoredLayoutsProduceExactCachedRecipesAndCanonicalRecords()
        {
            foreach (var expected in StarterInfernalDefenseLayouts.All)
            {
                var plan = FindInfernal(expected.LayoutId);

                Assert.That(plan.HasPlan, Is.True);
                Assert.That(plan.Layout, Is.SameAs(expected));
                AssertRecordMatchesSelection(
                    plan.SelectionResult.SelectionResult.Selection,
                    plan.IdentityResult.Record,
                    plan.CanonicalBytes);
            }
        }

        [Test]
        public void SignedSeedEdges_AreDeterministicForBothRealms()
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
                var sylvanFirst = SylvanStarterRealmIdentityPlanner.PlanExact(
                    StarterRealmSeededCatalogues.SylvanRealmId,
                    seed);
                var sylvanSecond = SylvanStarterRealmIdentityPlanner.PlanExact(
                    StarterRealmSeededCatalogues.SylvanRealmId,
                    seed);
                var infernalFirst = InfernalStarterRealmIdentityPlanner.PlanExact(
                    StarterRealmSeededCatalogues.InfernalRealmId,
                    seed);
                var infernalSecond = InfernalStarterRealmIdentityPlanner.PlanExact(
                    StarterRealmSeededCatalogues.InfernalRealmId,
                    seed);

                Assert.That(sylvanFirst.Recipe, Is.SameAs(sylvanSecond.Recipe));
                Assert.That(infernalFirst.Layout, Is.SameAs(infernalSecond.Layout));
                CollectionAssert.AreEqual(
                    sylvanFirst.CanonicalBytes,
                    sylvanSecond.CanonicalBytes);
                CollectionAssert.AreEqual(
                    infernalFirst.CanonicalBytes,
                    infernalSecond.CanonicalBytes);
                Assert.That(sylvanFirst.IdentityResult.Record.Seed, Is.EqualTo(seed));
                Assert.That(infernalFirst.IdentityResult.Record.Seed, Is.EqualTo(seed));
            }
        }

        [Test]
        public void PlansSnapshotCanonicalBytesAsImmutableEvidence()
        {
            var selection = SylvanStarterSeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.SylvanRealmId,
                17);
            var identity = IdentityFor(selection.SelectionResult.Selection);
            var bytes = StarterRealmIdentityCodec.SerializeCanonicalUtf8(identity.Record);
            var plan = SylvanStarterRealmIdentityPlanner.ComposeExact(
                selection,
                identity,
                bytes);
            var firstByte = plan.CanonicalBytes[0];
            bytes[0] = 0;

            Assert.That(plan.SelectionResult, Is.SameAs(selection));
            Assert.That(plan.IdentityResult, Is.SameAs(identity));
            Assert.That(plan.Recipe, Is.SameAs(selection.Recipe));
            Assert.That(plan.CanonicalBytes[0], Is.EqualTo(firstByte));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)plan.CanonicalBytes).RemoveAt(0));
        }

        [Test]
        public void PublicPlanners_FailClosedForCrossRealmAndMalformedRealmIds()
        {
            AssertSylvanRejected(
                SylvanStarterRealmIdentityPlanner.PlanExact(
                    StarterRealmSeededCatalogues.InfernalRealmId,
                    0),
                StarterRealmIdentityPlanningStatus.RealmIdMismatch);
            AssertInfernalRejected(
                InfernalStarterRealmIdentityPlanner.PlanExact(
                    StarterRealmSeededCatalogues.SylvanRealmId,
                    0),
                StarterRealmIdentityPlanningStatus.RealmIdMismatch);
            AssertSylvanRejected(
                SylvanStarterRealmIdentityPlanner.PlanExact("Realm.Sylvan", 0),
                StarterRealmIdentityPlanningStatus.SelectionRejected);
        }

        [Test]
        public void Compose_RejectsMissingAndRejectedSelectionResults()
        {
            AssertSylvanRejected(
                SylvanStarterRealmIdentityPlanner.ComposeExact(null, null, null),
                StarterRealmIdentityPlanningStatus.SelectionResultMissing);

            var hostile = SeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.SylvanRealmId,
                0,
                new[] { "realmraiders.sylvan-layout.unknown" });
            var rejected = SylvanStarterSeededLayoutSelector.ResolveExactSelection(hostile);
            AssertSylvanRejected(
                SylvanStarterRealmIdentityPlanner.ComposeExact(rejected, null, null),
                StarterRealmIdentityPlanningStatus.SelectionCatalogueMismatch);
        }

        [Test]
        public void Compose_RejectsCrossRealmAndAlteredCatalogueSelectionEvidence()
        {
            var crossRealmEvidence = SeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.InfernalRealmId,
                0,
                StarterRealmSeededCatalogues.SylvanLayoutIds);
            var crossRealm = SylvanStarterSeededLayoutSelector.ResolveExactSelection(
                crossRealmEvidence);
            AssertSylvanRejected(
                SylvanStarterRealmIdentityPlanner.ComposeExact(crossRealm, null, null),
                StarterRealmIdentityPlanningStatus.RealmIdMismatch);

            var alteredEvidence = SeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.InfernalRealmId,
                0,
                new[]
                {
                    StarterInfernalDefenseLayouts.AshenSpurId,
                    "realmraiders.infernal-defense.unknown",
                    StarterInfernalDefenseLayouts.EmberCircuitId
                });
            var altered = InfernalStarterSeededLayoutSelector.ResolveExactSelection(
                alteredEvidence);
            AssertInfernalRejected(
                InfernalStarterRealmIdentityPlanner.ComposeExact(altered, null, null),
                StarterRealmIdentityPlanningStatus.SelectionCatalogueMismatch);
        }

        [Test]
        public void Compose_RejectsMissingAndInvalidRecordResults()
        {
            var selection = ValidSylvanSelection(3);
            AssertSylvanRejected(
                SylvanStarterRealmIdentityPlanner.ComposeExact(selection, null, null),
                StarterRealmIdentityPlanningStatus.RecordResultMissing);

            var rejected = StarterRealmIdentityCodec.Create(
                2,
                StarterRealmSeededCatalogues.SylvanRealmId,
                3,
                selection.Recipe.LayoutId);
            AssertSylvanRejected(
                SylvanStarterRealmIdentityPlanner.ComposeExact(
                    selection,
                    rejected,
                    Array.Empty<byte>()),
                StarterRealmIdentityPlanningStatus.RecordRejected);
        }

        [Test]
        public void Compose_RejectsEveryRecordFieldMismatch()
        {
            var selection = ValidSylvanSelection(9);
            var selected = selection.SelectionResult.Selection;
            AssertRecordMismatch(selection, StarterRealmIdentityCodec.Create(
                1,
                StarterRealmSeededCatalogues.InfernalRealmId,
                selected.Seed,
                selected.LayoutId));
            AssertRecordMismatch(selection, StarterRealmIdentityCodec.Create(
                1,
                selected.RealmId,
                selected.Seed + 1,
                selected.LayoutId));
            AssertRecordMismatch(selection, StarterRealmIdentityCodec.Create(
                1,
                selected.RealmId,
                selected.Seed,
                StarterSylvanRealmLayouts.All[
                    (selected.CatalogueIndex + 1) % StarterSylvanRealmLayouts.All.Count].LayoutId));
        }

        [Test]
        public void Compose_RejectsMissingMalformedAndNoncanonicalBytes()
        {
            var selection = ValidInfernalSelection(21);
            var identity = IdentityFor(selection.SelectionResult.Selection);
            AssertInfernalRejected(
                InfernalStarterRealmIdentityPlanner.ComposeExact(
                    selection,
                    identity,
                    null),
                StarterRealmIdentityPlanningStatus.CanonicalBytesMissing);
            AssertInfernalRejected(
                InfernalStarterRealmIdentityPlanner.ComposeExact(
                    selection,
                    identity,
                    new byte[] { 0xFF }),
                StarterRealmIdentityPlanningStatus.CanonicalBytesRejected);

            var noncanonical = StarterRealmIdentityCodec.SerializeCanonicalText(
                identity.Record) + "\n";
            AssertInfernalRejected(
                InfernalStarterRealmIdentityPlanner.ComposeExact(
                    selection,
                    identity,
                    System.Text.Encoding.UTF8.GetBytes(noncanonical)),
                StarterRealmIdentityPlanningStatus.CanonicalBytesRejected);
        }

        [Test]
        public void Compose_RejectsCanonicalBytesForAnotherValidRecord()
        {
            var selection = ValidInfernalSelection(4);
            var identity = IdentityFor(selection.SelectionResult.Selection);
            var other = StarterRealmIdentityCodec.Create(
                1,
                identity.Record.RealmId,
                identity.Record.Seed + 1,
                identity.Record.LayoutId);

            AssertInfernalRejected(
                InfernalStarterRealmIdentityPlanner.ComposeExact(
                    selection,
                    identity,
                    StarterRealmIdentityCodec.SerializeCanonicalUtf8(other.Record)),
                StarterRealmIdentityPlanningStatus.CanonicalBytesMismatch);
        }

        private static SylvanStarterRealmIdentityPlan FindSylvan(string layoutId)
        {
            for (var seed = 0; seed < 16; seed++)
            {
                var plan = SylvanStarterRealmIdentityPlanner.PlanExact(
                    StarterRealmSeededCatalogues.SylvanRealmId,
                    seed);
                if (string.Equals(plan.Recipe.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return plan;
                }
            }

            Assert.Fail("No bounded Sylvan seed selected " + layoutId);
            return null;
        }

        private static InfernalStarterRealmIdentityPlan FindInfernal(string layoutId)
        {
            for (var seed = 0; seed < 16; seed++)
            {
                var plan = InfernalStarterRealmIdentityPlanner.PlanExact(
                    StarterRealmSeededCatalogues.InfernalRealmId,
                    seed);
                if (string.Equals(plan.Layout.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return plan;
                }
            }

            Assert.Fail("No bounded Infernal seed selected " + layoutId);
            return null;
        }

        private static SylvanStarterSeededLayoutResult ValidSylvanSelection(int seed)
        {
            return SylvanStarterSeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.SylvanRealmId,
                seed);
        }

        private static InfernalStarterSeededLayoutResult ValidInfernalSelection(int seed)
        {
            return InfernalStarterSeededLayoutSelector.SelectExact(
                StarterRealmSeededCatalogues.InfernalRealmId,
                seed);
        }

        private static StarterRealmIdentityResult IdentityFor(
            SeededSelection selection)
        {
            return StarterRealmIdentityCodec.Create(
                1,
                selection.RealmId,
                selection.Seed,
                selection.LayoutId);
        }

        private static void AssertRecordMismatch(
            SylvanStarterSeededLayoutResult selection,
            StarterRealmIdentityResult identity)
        {
            AssertSylvanRejected(
                SylvanStarterRealmIdentityPlanner.ComposeExact(
                    selection,
                    identity,
                    StarterRealmIdentityCodec.SerializeCanonicalUtf8(identity.Record)),
                StarterRealmIdentityPlanningStatus.RecordFieldMismatch);
        }

        private static void AssertRecordMatchesSelection(
            SeededSelection selection,
            IdentityRecord record,
            IReadOnlyList<byte> bytes)
        {
            Assert.That(record.Version,
                Is.EqualTo(StarterRealmIdentityRecordContract.CurrentVersion));
            Assert.That(record.RealmId, Is.EqualTo(selection.RealmId));
            Assert.That(record.Seed, Is.EqualTo(selection.Seed));
            Assert.That(record.LayoutId, Is.EqualTo(selection.LayoutId));
            var parsed = StarterRealmIdentityCodec.ParseCanonicalUtf8(ToArray(bytes));
            Assert.That(parsed.HasRecord, Is.True);
            Assert.That(parsed.Record.LayoutId, Is.EqualTo(record.LayoutId));
            CollectionAssert.AreEqual(
                StarterRealmIdentityCodec.SerializeCanonicalUtf8(record),
                bytes);
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

        private static void AssertSylvanRejected(
            SylvanStarterRealmIdentityPlan plan,
            StarterRealmIdentityPlanningStatus status)
        {
            Assert.That(plan.Status, Is.EqualTo(status));
            Assert.That(plan.HasPlan, Is.False);
            Assert.That(plan.Recipe, Is.Null);
            Assert.That(plan.CanonicalBytes, Is.Empty);
        }

        private static void AssertInfernalRejected(
            InfernalStarterRealmIdentityPlan plan,
            StarterRealmIdentityPlanningStatus status)
        {
            Assert.That(plan.Status, Is.EqualTo(status));
            Assert.That(plan.HasPlan, Is.False);
            Assert.That(plan.Layout, Is.Null);
            Assert.That(plan.CanonicalBytes, Is.Empty);
        }
    }
}
