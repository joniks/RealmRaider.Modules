using System;
using NUnit.Framework;
using RealmRaiders.Modules.RealmExpansionPlanning;
using RealmRaiders.Modules.RealmGrowthContracts;
using RealmRaiders.Modules.RealmLayoutContracts;
using RealmRaiders.Modules.StarterRealmExpansionChoiceRequest;
using RealmRaiders.Modules.StarterRealmGrowthIdentityPlanning;
using RealmRaiders.Modules.StarterRealmIdentityPlanning;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.StarterRealmExpansionChoiceRequest.Tests
{
    public sealed class StarterRealmExpansionChoiceRequestTests
    {
        [Test]
        public void Evaluate_SylvanReturnsExactFirstAndLastUnlockedReferencesInOrder()
        {
            var identity = SylvanStarterRealmIdentityPlanner.PlanExact("realmraiders.realm.sylvan", 37);
            var plan = SylvanStarterRealmGrowthIdentityPlanner.PlanExact(
                "realmraiders.realm.sylvan", 37, Tier(identity.Recipe.ExpansionSockets.Count));
            var first = SylvanStarterRealmExpansionChoiceEvaluator.Evaluate(
                plan, plan.ExpansionSockets[0].SocketId);
            var lastIndex = plan.ExpansionSockets.Count - 1;
            var last = SylvanStarterRealmExpansionChoiceEvaluator.Evaluate(
                plan, plan.ExpansionSockets[lastIndex].SocketId);

            Assert.That(first.IsEligible, Is.True);
            Assert.That(first.GrowthPlan, Is.SameAs(plan));
            Assert.That(first.Socket, Is.SameAs(plan.ExpansionSockets[0]));
            Assert.That(first.AuthoredSocketIndex, Is.EqualTo(0));
            Assert.That(last.Socket, Is.SameAs(plan.ExpansionSockets[lastIndex]));
            Assert.That(last.AuthoredSocketIndex, Is.EqualTo(lastIndex));
        }

        [Test]
        public void Evaluate_InfernalReturnsExactFirstAndLastUnlockedReferencesInOrder()
        {
            var identity = InfernalStarterRealmIdentityPlanner.PlanExact("realmraiders.realm.infernal", 41);
            var plan = InfernalStarterRealmGrowthIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", 41, Tier(identity.Layout.ExpansionSockets.Count));
            var first = InfernalStarterRealmExpansionChoiceEvaluator.Evaluate(
                plan, plan.ExpansionSockets[0].SocketId);
            var lastIndex = plan.ExpansionSockets.Count - 1;
            var last = InfernalStarterRealmExpansionChoiceEvaluator.Evaluate(
                plan, plan.ExpansionSockets[lastIndex].SocketId);

            Assert.That(first.IsEligible, Is.True);
            Assert.That(first.GrowthPlan, Is.SameAs(plan));
            Assert.That(first.Socket, Is.SameAs(plan.Layout.ExpansionSockets[0]));
            Assert.That(last.Socket, Is.SameAs(plan.Layout.ExpansionSockets[lastIndex]));
            Assert.That(last.AuthoredSocketIndex, Is.EqualTo(lastIndex));
        }

        [Test]
        public void Evaluate_RejectsZeroCapacityForBothFactions()
        {
            var sylvan = SylvanStarterRealmGrowthIdentityPlanner.PlanExact(
                "realmraiders.realm.sylvan", 1, Tier(0));
            var infernal = InfernalStarterRealmGrowthIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", 1, Tier(0));

            AssertRejected(StarterRealmExpansionChoiceStatus.ExpansionCapacityZero,
                SylvanStarterRealmExpansionChoiceEvaluator.Evaluate(sylvan, "socket.one"));
            AssertRejected(StarterRealmExpansionChoiceStatus.ExpansionCapacityZero,
                InfernalStarterRealmExpansionChoiceEvaluator.Evaluate(infernal, "socket.one"));
        }

        [Test]
        public void Evaluate_RejectsMissingAndRejectedGrowthPlans()
        {
            AssertRejected(StarterRealmExpansionChoiceStatus.GrowthPlanMissing,
                SylvanStarterRealmExpansionChoiceEvaluator.Evaluate(null, "socket.one"));
            var rejected = InfernalStarterRealmGrowthIdentityPlanner.PlanExact("wrong.realm", 1, Tier(0));
            AssertRejected(StarterRealmExpansionChoiceStatus.GrowthPlanRejected,
                InfernalStarterRealmExpansionChoiceEvaluator.Evaluate(rejected, "socket.one"));
        }

        [Test]
        public void Evaluate_RejectsMissingInvalidAndUnknownRequestedIds()
        {
            var plan = SylvanFullPlan();
            AssertRejected(StarterRealmExpansionChoiceStatus.RequestedSocketIdMissing,
                SylvanStarterRealmExpansionChoiceEvaluator.Evaluate(plan, null));
            AssertRejected(StarterRealmExpansionChoiceStatus.RequestedSocketIdInvalid,
                SylvanStarterRealmExpansionChoiceEvaluator.Evaluate(plan, "Socket.Invalid"));
            AssertRejected(StarterRealmExpansionChoiceStatus.RequestedSocketUnknown,
                SylvanStarterRealmExpansionChoiceEvaluator.Evaluate(plan, "realmraiders.socket.unknown"));
        }

        [Test]
        public void Evaluate_RejectsKnownSocketOutsideUnlockedPrefix()
        {
            var identity = SylvanStarterRealmIdentityPlanner.PlanExact("realmraiders.realm.sylvan", 7);
            var plan = SylvanStarterRealmGrowthIdentityPlanner.PlanExact(
                "realmraiders.realm.sylvan", 7, Tier(1));
            Assert.That(plan.ExpansionSockets.Count, Is.EqualTo(1));
            var later = identity.Recipe.ExpansionSockets[1];
            AssertRejected(StarterRealmExpansionChoiceStatus.RequestedSocketNotUnlocked,
                SylvanStarterRealmExpansionChoiceEvaluator.Evaluate(plan, later.SocketId));
        }

        [Test]
        public void Evaluate_InfernalRejectsKnownLayoutSocketOutsideUnlockedPrefix()
        {
            var identity = InfernalStarterRealmIdentityPlanner.PlanExact("realmraiders.realm.infernal", 8);
            var plan = InfernalStarterRealmGrowthIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", 8, Tier(1));
            Assert.That(plan.ExpansionSockets.Count, Is.EqualTo(1));
            var later = identity.Layout.ExpansionSockets[1];
            var result = InfernalStarterRealmExpansionChoiceEvaluator.Evaluate(plan, later.SocketId);

            AssertRejected(StarterRealmExpansionChoiceStatus.RequestedSocketNotUnlocked, result);
        }

        [Test]
        public void Evaluate_IsDeterministicAndDoesNotMutatePlanEvidence()
        {
            var plan = InfernalFullPlan();
            var id = plan.ExpansionSockets[0].SocketId;
            var first = InfernalStarterRealmExpansionChoiceEvaluator.Evaluate(plan, id);
            var second = InfernalStarterRealmExpansionChoiceEvaluator.Evaluate(plan, id);

            Assert.That(second.Status, Is.EqualTo(first.Status));
            Assert.That(second.Socket, Is.SameAs(first.Socket));
            Assert.That(plan.ExpansionSockets[0].SocketId, Is.EqualTo(id));
            Assert.That(plan.ExpansionSockets.Count, Is.EqualTo(plan.ExpansionResult.Plan.ExpansionSockets.Count));
        }

        [Test]
        public void Evaluate_UsesOnlyExactGrowthPlanEvidence()
        {
            var identity = InfernalStarterRealmIdentityPlanner.PlanExact("realmraiders.realm.infernal", 3);
            var tier = Tier(1);
            var copiedLayout = new RealmLayoutGraph(
                identity.Layout.LayoutId, identity.Layout.DisplayName, identity.Layout.Nodes,
                identity.Layout.Edges, identity.Layout.Landmarks, identity.Layout.ExpansionSockets);
            var copiedExpansion = RealmExpansionPlanEvaluator.Evaluate(copiedLayout, tier);
            var rejected = InfernalStarterRealmGrowthIdentityPlanner.ComposeExact(
                identity, tier, copiedExpansion);

            Assert.That(rejected.HasPlan, Is.False);
            AssertRejected(StarterRealmExpansionChoiceStatus.GrowthPlanRejected,
                InfernalStarterRealmExpansionChoiceEvaluator.Evaluate(rejected, "socket.one"));
        }

        private static SylvanStarterRealmGrowthIdentityPlan SylvanFullPlan()
        {
            var identity = SylvanStarterRealmIdentityPlanner.PlanExact("realmraiders.realm.sylvan", 11);
            return SylvanStarterRealmGrowthIdentityPlanner.PlanExact(
                "realmraiders.realm.sylvan", 11, Tier(identity.Recipe.ExpansionSockets.Count));
        }

        private static InfernalStarterRealmGrowthIdentityPlan InfernalFullPlan()
        {
            var identity = InfernalStarterRealmIdentityPlanner.PlanExact("realmraiders.realm.infernal", 13);
            return InfernalStarterRealmGrowthIdentityPlanner.PlanExact(
                "realmraiders.realm.infernal", 13, Tier(identity.Layout.ExpansionSockets.Count));
        }

        private static RealmGrowthTier Tier(int capacity)
        {
            return new RealmGrowthTier("tier.valid", 0, 1, 1, capacity);
        }

        private static void AssertRejected(
            StarterRealmExpansionChoiceStatus expectedStatus,
            SylvanStarterRealmExpansionChoiceResult result)
        {
            Assert.That(result.Status, Is.EqualTo(expectedStatus));
            Assert.That(result.IsEligible, Is.False);
            Assert.That(result.Socket, Is.Null);
            Assert.That(result.AuthoredSocketIndex, Is.EqualTo(-1));
        }

        private static void AssertRejected(
            StarterRealmExpansionChoiceStatus expectedStatus,
            InfernalStarterRealmExpansionChoiceResult result)
        {
            Assert.That(result.Status, Is.EqualTo(expectedStatus));
            Assert.That(result.IsEligible, Is.False);
            Assert.That(result.Socket, Is.Null);
            Assert.That(result.AuthoredSocketIndex, Is.EqualTo(-1));
        }
    }
}
