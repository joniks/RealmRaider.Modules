using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.RealmExpansionPlanning;
using RealmRaiders.Modules.RealmGrowthContracts;
using RealmRaiders.Modules.RealmLayoutContracts;
using RealmRaiders.Modules.StarterRealmIdentityPlanning;
using RealmRaiders.Modules.StarterRealmIdentityRecord;
using RealmRaiders.Modules.StarterRealmLayouts;
using RealmRaiders.Modules.StarterRealmSeededSelection;

namespace RealmRaiders.Modules.StarterRealmGrowthIdentityPlanning.Tests
{
    public sealed class StarterRealmGrowthIdentityPlanningTests
    {
        [Test]
        public void PlanExact_SylvanPreservesIdentityBytesAndZeroToFullAuthoredOrder()
        {
            var identity = SylvanStarterRealmIdentityPlanner.PlanExact(
                "realmraiders.realm.sylvan", 17);
            for (var capacity = 0; capacity <= identity.Recipe.ExpansionSockets.Count; capacity++)
            {
                var plan = SylvanStarterRealmGrowthIdentityPlanner.PlanExact(
                    "realmraiders.realm.sylvan", 17, Tier(capacity));
                Assert.That(plan.HasPlan, Is.True, "capacity " + capacity);
                Assert.That(plan.IdentityPlan, Is.Not.Null);
                Assert.That(plan.CanonicalBytes, Is.EqualTo(plan.IdentityPlan.CanonicalBytes));
                Assert.That(plan.ExpansionSockets.Count, Is.EqualTo(capacity));
                for (var index = 0; index < capacity; index++)
                {
                    Assert.That(plan.ExpansionSockets[index], Is.SameAs(identity.Recipe.ExpansionSockets[index]));
                    Assert.That(plan.ExpansionResult.Plan.ExpansionSockets[index].SocketId,
                        Is.EqualTo(identity.Recipe.ExpansionSockets[index].SocketId));
                    Assert.That(plan.ExpansionResult.Plan.ExpansionSockets[index].NodeId,
                        Is.EqualTo(identity.Recipe.ExpansionSockets[index].NodeId));
                }
            }
        }

        [Test]
        public void PlanExact_InfernalPreservesExactCachedSocketReferencesAtFullCapacity()
        {
            var identity = InfernalStarterRealmIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", -91);
            var plan = InfernalStarterRealmGrowthIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", -91, Tier(identity.Layout.ExpansionSockets.Count));

            Assert.That(plan.HasPlan, Is.True);
            Assert.That(plan.IdentityPlan.Layout, Is.SameAs(identity.Layout));
            for (var index = 0; index < plan.ExpansionSockets.Count; index++)
                Assert.That(plan.ExpansionSockets[index], Is.SameAs(identity.Layout.ExpansionSockets[index]));
        }

        [Test]
        public void ComposeExact_InfernalZeroCapacityPreservesSuppliedEvidenceReferences()
        {
            var identity = InfernalStarterRealmIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", 23);
            var tier = Tier(0);
            var expansion = RealmExpansionPlanEvaluator.Evaluate(identity.Layout, tier);
            var plan = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(identity, tier, expansion);

            Assert.That(plan.HasPlan, Is.True);
            Assert.That(plan.IdentityPlan, Is.SameAs(identity));
            Assert.That(plan.Tier, Is.SameAs(tier));
            Assert.That(plan.ExpansionResult, Is.SameAs(expansion));
            Assert.That(plan.Layout, Is.SameAs(identity.Layout));
            Assert.That(plan.ExpansionSockets, Is.Empty);
        }

        [Test]
        public void ComposeExact_FailsClosedForMissingRejectedAndMismatchedExpansionEvidence()
        {
            var tier = Tier(0);
            var missing = SylvanStarterRealmGrowthIdentityPlanner.ComposeExact(null, tier, null);
            Assert.That(missing.Status, Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.IdentityPlanMissing));

            var validIdentity = SylvanStarterRealmIdentityPlanner.PlanExact(
                "realmraiders.realm.sylvan", 1);
            var missingExpansion = SylvanStarterRealmGrowthIdentityPlanner.ComposeExact(
                validIdentity, tier, null);
            Assert.That(missingExpansion.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.ExpansionResultMissing));

            var rejectedIdentity = SylvanStarterRealmIdentityPlanner.PlanExact("wrong.realm", 1);
            var rejectedIdentityPlan = SylvanStarterRealmGrowthIdentityPlanner.ComposeExact(rejectedIdentity, tier, null);
            Assert.That(rejectedIdentityPlan.Status, Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.IdentityPlanRejected));

            var infernalIdentity = InfernalStarterRealmIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", 1);
            var otherLayout = StarterInfernalDefenseLayouts.All[0];
            for (var index = 1; ReferenceEquals(otherLayout, infernalIdentity.Layout)
                 && index < StarterInfernalDefenseLayouts.All.Count; index++)
                otherLayout = StarterInfernalDefenseLayouts.All[index];
            var wrongExpansion = RealmExpansionPlanEvaluator.Evaluate(otherLayout, Tier(0));
            var mismatched = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(
                infernalIdentity, tier, wrongExpansion);
            Assert.That(mismatched.Status, Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.ExpansionEvidenceMismatch));

            var rejectedExpansion = RealmExpansionPlanEvaluator.Evaluate(infernalIdentity.Layout, null);
            var rejectedExpansionPlan = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(
                infernalIdentity, tier, rejectedExpansion);
            Assert.That(rejectedExpansionPlan.Status, Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.ExpansionResultRejected));

            var oneCapacityTier = Tier(1);
            var exactLayoutWrongCapacity = RealmExpansionPlanEvaluator.Evaluate(
                infernalIdentity.Layout, oneCapacityTier);
            var capacityMismatch = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(
                infernalIdentity, tier, exactLayoutWrongCapacity);
            Assert.That(capacityMismatch.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.ExpansionEvidenceMismatch));
        }

        [Test]
        public void ComposeExact_SnapshotsOutputAndRepeatsDeterministically()
        {
            var identity = InfernalStarterRealmIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", int.MinValue);
            var tier = Tier(Math.Min(1, identity.Layout.ExpansionSockets.Count));
            var expansion = RealmExpansionPlanEvaluator.Evaluate(identity.Layout, tier);
            var first = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(identity, tier, expansion);
            var second = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(identity, tier, expansion);

            Assert.That(first.HasPlan, Is.True);
            Assert.That(first.CanonicalBytes, Is.EqualTo(second.CanonicalBytes));
            Assert.That(first.ExpansionSockets.Count, Is.EqualTo(second.ExpansionSockets.Count));
            Assert.Throws<NotSupportedException>(() => ((IList)first.CanonicalBytes).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() => ((IList)first.ExpansionSockets).RemoveAt(0));
        }

        [Test]
        public void ComposeExact_RejectsCopiedSylvanGraphAndSameIdDifferentTierFacts()
        {
            var sylvanIdentity = SylvanStarterRealmIdentityPlanner.PlanExact(
                "realmraiders.realm.sylvan", 19);
            var tier = Tier(0);
            var copiedGraphExpansion = RealmExpansionPlanEvaluator.Evaluate(
                StarterSylvanRealmLayoutGraphAdapter.Adapt(sylvanIdentity.Recipe), tier);
            var copiedSource = SylvanStarterRealmGrowthIdentityPlanner.ComposeExact(
                sylvanIdentity, tier, copiedGraphExpansion);
            Assert.That(copiedSource.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.ExpansionEvidenceMismatch));

            var infernalIdentity = InfernalStarterRealmIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", 19);
            var sameIdDifferentFacts = new RealmGrowthTier("tier.valid", 7, 9, 11, 0);
            var foreignTierExpansion = RealmExpansionPlanEvaluator.Evaluate(
                infernalIdentity.Layout, sameIdDifferentFacts);
            var tierMismatch = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(
                infernalIdentity, tier, foreignTierExpansion);
            Assert.That(tierMismatch.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.ExpansionEvidenceMismatch));
        }

        [Test]
        public void ComposeExact_RejectsCopiedOrReorderedInfernalSourceGraphs()
        {
            var identity = InfernalStarterRealmIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", 29);
            var tier = Tier(Math.Min(2, identity.Layout.ExpansionSockets.Count));
            var copied = CopyGraph(identity.Layout, identity.Layout.ExpansionSockets);
            var copiedPlan = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(
                identity, tier, RealmExpansionPlanEvaluator.Evaluate(copied, tier));
            Assert.That(copiedPlan.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.ExpansionEvidenceMismatch));

            var reorderedSockets = new RealmLayoutGraphExpansionSocket[identity.Layout.ExpansionSockets.Count];
            for (var index = 0; index < reorderedSockets.Length; index++)
                reorderedSockets[index] = identity.Layout.ExpansionSockets[reorderedSockets.Length - index - 1];
            var reordered = CopyGraph(identity.Layout, reorderedSockets);
            var reorderedPlan = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(
                identity, tier, RealmExpansionPlanEvaluator.Evaluate(reordered, tier));
            Assert.That(reorderedPlan.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.ExpansionEvidenceMismatch));
        }

        [Test]
        public void ComposeExact_UsesMgc26CanonicalSnapshotAndRejectsCorruptCanonicalInput()
        {
            var selection = SylvanStarterSeededLayoutSelector.SelectExact(
                "realmraiders.realm.sylvan", 31);
            var record = StarterRealmIdentityCodec.Create(
                StarterRealmIdentityRecordContract.CurrentVersion,
                "realmraiders.realm.sylvan", 31, selection.Recipe.LayoutId);
            var bytes = StarterRealmIdentityCodec.SerializeCanonicalUtf8(record.Record);
            var identity = SylvanStarterRealmIdentityPlanner.ComposeExact(selection, record, bytes);
            bytes[0] = (byte)'x';

            var tier = Tier(0);
            var expansion = RealmExpansionPlanEvaluator.Evaluate(
                StarterSylvanRealmLayoutGraphAdapter.AdaptExactCached(identity.Recipe), tier);
            var plan = SylvanStarterRealmGrowthIdentityPlanner.ComposeExact(identity, tier, expansion);
            Assert.That(plan.HasPlan, Is.True);
            Assert.That(plan.CanonicalBytes[0], Is.Not.EqualTo((byte)'x'));

            var corruptBytes = StarterRealmIdentityCodec.SerializeCanonicalUtf8(record.Record);
            corruptBytes[0] = (byte)'x';
            var corruptIdentity = SylvanStarterRealmIdentityPlanner.ComposeExact(
                selection, record, corruptBytes);
            Assert.That(corruptIdentity.HasPlan, Is.False);
            Assert.That(SylvanStarterRealmGrowthIdentityPlanner.ComposeExact(
                corruptIdentity, tier, expansion).Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.IdentityPlanRejected));
        }

        [Test]
        public void ComposePersistedExact_PreservesBothFactionCachedReferencesWithoutReselection()
        {
            const string realmInstanceId = "0123456789abcdef0123456789abcdef";
            var tier = Tier(0);
            var sylvanRecipe = StarterSylvanRealmLayouts.All[0];
            var sylvanIdentity = StarterRealmIdentityCodec.Create(
                StarterRealmIdentityRecordContract.CurrentVersion,
                realmInstanceId, -17, sylvanRecipe.LayoutId);
            var sylvanBytes = StarterRealmIdentityCodec.SerializeCanonicalUtf8(sylvanIdentity.Record);
            var sylvanExpansion = RealmExpansionPlanEvaluator.Evaluate(
                StarterSylvanRealmLayoutGraphAdapter.AdaptExactCached(sylvanRecipe), tier);
            var sylvan = SylvanStarterRealmGrowthIdentityPlanner.ComposePersistedExact(
                sylvanIdentity, sylvanBytes, sylvanRecipe, tier, sylvanExpansion);
            var sylvanRepeat = SylvanStarterRealmGrowthIdentityPlanner.ComposePersistedExact(
                sylvanIdentity, sylvanBytes, sylvanRecipe, tier, sylvanExpansion);

            Assert.That(sylvan.HasPlan, Is.True);
            Assert.That(sylvan.IdentityPlan, Is.Null);
            Assert.That(sylvan.IdentityResult, Is.SameAs(sylvanIdentity));
            Assert.That(sylvan.Recipe, Is.SameAs(sylvanRecipe));
            Assert.That(sylvan.Tier, Is.SameAs(tier));
            Assert.That(sylvan.ExpansionResult, Is.SameAs(sylvanExpansion));
            Assert.That(sylvan.ExpansionSockets, Is.Empty);
            Assert.That(sylvan.CanonicalBytes, Is.EqualTo(sylvanBytes));
            Assert.That(sylvanRepeat.CanonicalBytes, Is.EqualTo(sylvan.CanonicalBytes));

            var infernalLayout = StarterInfernalDefenseLayouts.All[0];
            var infernalIdentity = StarterRealmIdentityCodec.Create(
                StarterRealmIdentityRecordContract.CurrentVersion,
                realmInstanceId, -17, infernalLayout.LayoutId);
            var infernalBytes = StarterRealmIdentityCodec.SerializeCanonicalUtf8(infernalIdentity.Record);
            var infernalExpansion = RealmExpansionPlanEvaluator.Evaluate(infernalLayout, tier);
            var infernal = InfernalStarterRealmGrowthIdentityPlanner.ComposePersistedExact(
                infernalIdentity, infernalBytes, infernalLayout, tier, infernalExpansion);

            Assert.That(infernal.HasPlan, Is.True);
            Assert.That(infernal.IdentityPlan, Is.Null);
            Assert.That(infernal.IdentityResult, Is.SameAs(infernalIdentity));
            Assert.That(infernal.Layout, Is.SameAs(infernalLayout));
            Assert.That(infernal.Tier, Is.SameAs(tier));
            Assert.That(infernal.ExpansionResult, Is.SameAs(infernalExpansion));
            Assert.That(infernal.ExpansionSockets, Is.Empty);
            Assert.That(infernal.CanonicalBytes, Is.EqualTo(infernalBytes));
            sylvanBytes[0] = (byte)'x';
            infernalBytes[0] = (byte)'x';
            Assert.That(sylvan.CanonicalBytes[0], Is.Not.EqualTo((byte)'x'));
            Assert.That(infernal.CanonicalBytes[0], Is.Not.EqualTo((byte)'x'));
        }

        [Test]
        public void ComposePersistedExact_FailsClosedForCopiedLayoutAndCorruptBytes()
        {
            const string realmInstanceId = "0123456789abcdef0123456789abcdef";
            var tier = Tier(0);
            var recipe = StarterSylvanRealmLayouts.All[0];
            var identity = StarterRealmIdentityCodec.Create(1, realmInstanceId, 5, recipe.LayoutId);
            var bytes = StarterRealmIdentityCodec.SerializeCanonicalUtf8(identity.Record);
            var expansion = RealmExpansionPlanEvaluator.Evaluate(
                StarterSylvanRealmLayoutGraphAdapter.AdaptExactCached(recipe), tier);
            var copiedRecipe = new RealmLayoutRecipe(
                recipe.LayoutId, recipe.DisplayName, recipe.Nodes, recipe.Edges,
                recipe.Landmarks, recipe.ExpansionSockets);
            var copied = SylvanStarterRealmGrowthIdentityPlanner.ComposePersistedExact(
                identity, bytes, copiedRecipe, tier, expansion);
            Assert.That(copied.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.CachedLayoutMismatch));

            var corruptBytes = (byte[])bytes.Clone();
            corruptBytes[0] = (byte)'x';
            var corrupt = SylvanStarterRealmGrowthIdentityPlanner.ComposePersistedExact(
                identity, corruptBytes, recipe, tier, expansion);
            Assert.That(corrupt.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.CanonicalBytesRejected));

            var infernalLayout = StarterInfernalDefenseLayouts.All[0];
            var infernalIdentity = StarterRealmIdentityCodec.Create(
                1, realmInstanceId, 5, infernalLayout.LayoutId);
            var infernalBytes = StarterRealmIdentityCodec.SerializeCanonicalUtf8(infernalIdentity.Record);
            var infernalExpansion = RealmExpansionPlanEvaluator.Evaluate(infernalLayout, tier);
            var copiedInfernal = InfernalStarterRealmGrowthIdentityPlanner.ComposePersistedExact(
                infernalIdentity, infernalBytes,
                CopyGraph(infernalLayout, infernalLayout.ExpansionSockets), tier, infernalExpansion);
            Assert.That(copiedInfernal.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.CachedLayoutMismatch));

            var missing = SylvanStarterRealmGrowthIdentityPlanner.ComposePersistedExact(
                null, bytes, recipe, tier, expansion);
            Assert.That(missing.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.PersistedIdentityResultMissing));
            var rejectedIdentity = StarterRealmIdentityCodec.Create(2, realmInstanceId, 5, recipe.LayoutId);
            var rejected = SylvanStarterRealmGrowthIdentityPlanner.ComposePersistedExact(
                rejectedIdentity, bytes, recipe, tier, expansion);
            Assert.That(rejected.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.PersistedIdentityRejected));
            var mismatchedIdentity = StarterRealmIdentityCodec.Create(
                1, realmInstanceId, 5, "realmraiders.sylvan-layout.forked-canopy");
            var mismatchBytes = StarterRealmIdentityCodec.SerializeCanonicalUtf8(mismatchedIdentity.Record);
            var mismatch = SylvanStarterRealmGrowthIdentityPlanner.ComposePersistedExact(
                mismatchedIdentity, mismatchBytes, recipe, tier, expansion);
            Assert.That(mismatch.Status,
                Is.EqualTo(StarterRealmGrowthIdentityPlanningStatus.PersistedRecordLayoutMismatch));
        }

        private static RealmGrowthTier Tier(int capacity)
        {
            return new RealmGrowthTier("tier.valid", 0, 1, 1, capacity);
        }

        private static RealmLayoutGraph CopyGraph(
            RealmLayoutGraph source,
            IReadOnlyList<RealmLayoutGraphExpansionSocket> sockets)
        {
            return new RealmLayoutGraph(
                source.LayoutId,
                source.DisplayName,
                source.Nodes,
                source.Edges,
                source.Landmarks,
                sockets);
        }
    }
}
