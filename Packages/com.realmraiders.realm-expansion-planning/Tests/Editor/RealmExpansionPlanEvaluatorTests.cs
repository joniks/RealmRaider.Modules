using System;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.RealmGrowthContracts;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.RealmExpansionPlanning.Tests
{
    public sealed class RealmExpansionPlanEvaluatorTests
    {
        [Test]
        public void Evaluate_MapsEveryCapacityInExactAuthoredOrder()
        {
            var layout = ValidGraph();
            for (var capacity = 0;
                 capacity <= layout.ExpansionSockets.Count;
                 capacity++)
            {
                var result = RealmExpansionPlanEvaluator.Evaluate(
                    layout,
                    ValidTier(capacity));

                Assert.That(result.HasPlan, Is.True, layout.LayoutId + " @ " + capacity);
                Assert.That(result.Status, Is.EqualTo(RealmExpansionPlanStatus.Planned));
                Assert.That(result.Plan.LayoutId, Is.EqualTo(layout.LayoutId));
                Assert.That(result.Plan.TierId, Is.EqualTo("tier.valid"));
                Assert.That(result.Plan.ExpansionSockets.Count, Is.EqualTo(capacity));
                Assert.That(result.LayoutValidation.IsValid, Is.True);
                Assert.That(result.TierValidation.IsValid, Is.True);
                for (var index = 0; index < capacity; index++)
                {
                    Assert.That(
                        result.Plan.ExpansionSockets[index],
                        Is.SameAs(layout.ExpansionSockets[index]));
                    AssertSocketEqual(
                        layout.ExpansionSockets[index],
                        result.Plan.ExpansionSockets[index]);
                }
            }
        }

        [Test]
        public void Evaluate_ZeroCapacityIsAValidEmptyPlan()
        {
            var layout = ValidGraph();
            var result = RealmExpansionPlanEvaluator.Evaluate(layout, ValidTier(0));
            var emptyLayout = Copy(
                layout,
                sockets: Array.Empty<RealmLayoutGraphExpansionSocket>());
            var emptyResult = RealmExpansionPlanEvaluator.Evaluate(
                emptyLayout,
                ValidTier(0));

            Assert.That(result.HasPlan, Is.True);
            Assert.That(result.Plan.ExpansionSockets, Is.Empty);
            Assert.That(result.Issues, Is.Empty);
            Assert.That(emptyResult.HasPlan, Is.True);
            Assert.That(emptyResult.Plan.ExpansionSockets, Is.Empty);
        }

        [Test]
        public void Evaluate_FailsClosedWhenCapacityExceedsAvailableSockets()
        {
            var layout = ValidGraph();
            var result = RealmExpansionPlanEvaluator.Evaluate(
                layout,
                ValidTier(layout.ExpansionSockets.Count + 1));

            Assert.That(result.HasPlan, Is.False);
            Assert.That(result.Status, Is.EqualTo(RealmExpansionPlanStatus.Rejected));
            Assert.That(result.Plan, Is.Null);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmExpansionPlanIssue.ExpansionAnchorCapacityExceedsAvailableSockets
                },
                result.Issues);
        }

        [Test]
        public void Evaluate_FailsClosedForMissingInputsInStableOrder()
        {
            var result = RealmExpansionPlanEvaluator.Evaluate(null, null);

            CollectionAssert.AreEqual(
                new[]
                {
                    RealmExpansionPlanIssue.LayoutMissing,
                    RealmExpansionPlanIssue.TierMissing
                },
                result.Issues);
            Assert.That(result.LayoutValidation, Is.Not.Null);
            Assert.That(result.TierValidation, Is.Not.Null);
            CollectionAssert.AreEqual(
                new[] { RealmLayoutGraphValidationIssue.GraphMissing },
                result.LayoutValidation.Issues);
        }

        [Test]
        public void Evaluate_FailsClosedForInvalidLayoutAndTierIdentities()
        {
            var layout = ValidGraph();
            var invalidLayout = Copy(layout, layoutId: "Layout.invalid");
            var invalidTier = new RealmGrowthTier("Tier.invalid", 0, 1, 1, 0);

            CollectionAssert.AreEqual(
                new[]
                {
                    RealmExpansionPlanIssue.LayoutIdInvalid,
                    RealmExpansionPlanIssue.LayoutInvalid
                },
                RealmExpansionPlanEvaluator.Evaluate(invalidLayout, ValidTier(0)).Issues);
            CollectionAssert.AreEqual(
                new[] { RealmExpansionPlanIssue.TierIdInvalid },
                RealmExpansionPlanEvaluator.Evaluate(layout, invalidTier).Issues);
        }

        [Test]
        public void Evaluate_FailsClosedForEveryInvalidTierFact()
        {
            var layout = ValidGraph();
            var tiers = new[]
            {
                new RealmGrowthTier("tier.invalid", -1, 1, 1, 0),
                new RealmGrowthTier("tier.invalid", 0, 0, 1, 0),
                new RealmGrowthTier("tier.invalid", 0, 1, 0, 0),
                new RealmGrowthTier("tier.invalid", 0, 1, 1, -1)
            };

            foreach (var tier in tiers)
            {
                CollectionAssert.AreEqual(
                    new[] { RealmExpansionPlanIssue.TierFactsInvalid },
                    RealmExpansionPlanEvaluator.Evaluate(layout, tier).Issues);
            }
        }

        [Test]
        public void Evaluate_FailsClosedForMalformedDuplicateAndUnknownSocketFacts()
        {
            var source = ValidGraph();
            var first = source.ExpansionSockets[0];
            var second = source.ExpansionSockets[1];

            AssertSocketIssue(
                source,
                new RealmLayoutGraphExpansionSocket[] { null, second },
                RealmExpansionPlanIssue.ExpansionSocketMissing);
            AssertSocketIssue(
                source,
                new[]
                {
                    new RealmLayoutGraphExpansionSocket(" Bad", first.NodeId, first.X, first.Z),
                    second
                },
                RealmExpansionPlanIssue.ExpansionSocketIdInvalid);
            AssertSocketIssue(
                source,
                new[]
                {
                    first,
                    new RealmLayoutGraphExpansionSocket(first.SocketId, second.NodeId, second.X, second.Z)
                },
                RealmExpansionPlanIssue.ExpansionSocketIdDuplicate);
            AssertSocketIssue(
                source,
                new[]
                {
                    new RealmLayoutGraphExpansionSocket(first.SocketId, "test.unknown", first.X, first.Z),
                    second
                },
                RealmExpansionPlanIssue.ExpansionSocketNodeInvalid);
        }

        [Test]
        public void Evaluate_FailsClosedForNonFiniteAndOutOfRangeSocketCoordinates()
        {
            var source = ValidGraph();
            var first = source.ExpansionSockets[0];
            var values = new[]
            {
                float.NaN,
                float.PositiveInfinity,
                float.NegativeInfinity,
                RealmLayoutGraphValidator.MaximumAbsoluteCoordinate + 1f
            };

            foreach (var value in values)
            {
                AssertSocketIssue(
                    source,
                    new[]
                    {
                        new RealmLayoutGraphExpansionSocket(
                            first.SocketId,
                            first.NodeId,
                            value,
                            first.Z),
                        source.ExpansionSockets[1]
                    },
                    RealmExpansionPlanIssue.ExpansionSocketCoordinateInvalid);
            }
        }

        [Test]
        public void Evaluate_ReportsCombinedSocketFaultsInStableEvidenceOrder()
        {
            var source = ValidGraph();
            var first = source.ExpansionSockets[0];
            var sockets = new[]
            {
                new RealmLayoutGraphExpansionSocket(
                    first.SocketId,
                    "test.unknown",
                    float.NaN,
                    first.Z),
                new RealmLayoutGraphExpansionSocket(
                    first.SocketId,
                    source.ExpansionSockets[1].NodeId,
                    float.PositiveInfinity,
                    source.ExpansionSockets[1].Z)
            };

            CollectionAssert.AreEqual(
                new[]
                {
                    RealmExpansionPlanIssue.LayoutInvalid,
                    RealmExpansionPlanIssue.ExpansionSocketIdDuplicate,
                    RealmExpansionPlanIssue.ExpansionSocketNodeInvalid,
                    RealmExpansionPlanIssue.ExpansionSocketCoordinateInvalid
                },
                RealmExpansionPlanEvaluator.Evaluate(
                    Copy(source, sockets: sockets),
                    ValidTier(1)).Issues);
        }

        [Test]
        public void Evaluate_RejectsInvalidNonSocketGraphFacts()
        {
            var source = ValidGraph();
            var nodes = Copy(source.Nodes);
            nodes[0] = new RealmLayoutGraphNode(
                nodes[0].NodeId,
                null,
                nodes[0].Kind,
                nodes[0].X,
                nodes[0].Z);
            var result = RealmExpansionPlanEvaluator.Evaluate(
                Copy(source, nodes: nodes),
                ValidTier(0));

            CollectionAssert.AreEqual(
                new[] { RealmExpansionPlanIssue.LayoutInvalid },
                result.Issues);
            Assert.That(
                result.LayoutValidation.Issues,
                Does.Contain(RealmLayoutGraphValidationIssue.NodeGameplayRoleIdInvalid));
        }

        [Test]
        public void Evaluate_IsDeterministicAndPlanCollectionsAreImmutable()
        {
            var layout = ValidGraph();
            var first = RealmExpansionPlanEvaluator.Evaluate(layout, ValidTier(3));
            var second = RealmExpansionPlanEvaluator.Evaluate(layout, ValidTier(3));

            CollectionAssert.AreEqual(first.Plan.ExpansionSockets, second.Plan.ExpansionSockets);
            for (var index = 0; index < first.Plan.ExpansionSockets.Count; index++)
            {
                Assert.That(
                    first.Plan.ExpansionSockets[index],
                    Is.SameAs(second.Plan.ExpansionSockets[index]));
            }

            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutGraphExpansionSocket>)first.Plan.ExpansionSockets)[0] = null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmExpansionPlanIssue>)
                    RealmExpansionPlanEvaluator.Evaluate(null, null).Issues)[0] =
                    RealmExpansionPlanIssue.LayoutInvalid);
        }

        private static void AssertSocketIssue(
            RealmLayoutGraph source,
            IReadOnlyList<RealmLayoutGraphExpansionSocket> sockets,
            RealmExpansionPlanIssue expected)
        {
            var result = RealmExpansionPlanEvaluator.Evaluate(
                Copy(source, sockets: sockets),
                ValidTier(1));
            Assert.That(result.HasPlan, Is.False);
            Assert.That(result.Issues[0], Is.EqualTo(RealmExpansionPlanIssue.LayoutInvalid));
            Assert.That(result.Issues, Does.Contain(expected));
        }

        private static RealmGrowthTier ValidTier(int capacity)
        {
            return new RealmGrowthTier("tier.valid", 0, 1, 1, capacity);
        }

        private static RealmLayoutGraph ValidGraph()
        {
            return new RealmLayoutGraph(
                "realmraiders.test.expansion",
                "Expansion Test",
                new[]
                {
                    new RealmLayoutGraphNode(
                        "test.entry",
                        "test.role.entry",
                        RealmLayoutGraphNodeKind.Entry,
                        -20f,
                        0f),
                    new RealmLayoutGraphNode(
                        "test.mid",
                        "test.role.mid",
                        RealmLayoutGraphNodeKind.Landmark,
                        0f,
                        0f),
                    new RealmLayoutGraphNode(
                        "test.objective",
                        "test.role.objective",
                        RealmLayoutGraphNodeKind.Objective,
                        20f,
                        0f)
                },
                new[]
                {
                    new RealmLayoutGraphEdge(
                        "test.entry-mid",
                        "test.entry",
                        "test.mid",
                        true,
                        7f),
                    new RealmLayoutGraphEdge(
                        "test.mid-objective",
                        "test.mid",
                        "test.objective",
                        true,
                        7f)
                },
                Array.Empty<RealmLayoutGraphLandmark>(),
                new[]
                {
                    new RealmLayoutGraphExpansionSocket(
                        "test.socket-a",
                        "test.mid",
                        0f,
                        10f),
                    new RealmLayoutGraphExpansionSocket(
                        "test.socket-b",
                        "test.entry",
                        -20f,
                        -10f),
                    new RealmLayoutGraphExpansionSocket(
                        "test.socket-c",
                        "test.objective",
                        20f,
                        10f)
                });
        }

        private static RealmLayoutGraph Copy(
            RealmLayoutGraph source,
            string layoutId = null,
            IReadOnlyList<RealmLayoutGraphNode> nodes = null,
            IReadOnlyList<RealmLayoutGraphExpansionSocket> sockets = null)
        {
            return new RealmLayoutGraph(
                layoutId ?? source.LayoutId,
                source.DisplayName,
                nodes ?? source.Nodes,
                source.Edges,
                source.Landmarks,
                sockets ?? source.ExpansionSockets);
        }

        private static RealmLayoutGraphNode[] Copy(IReadOnlyList<RealmLayoutGraphNode> source)
        {
            var copy = new RealmLayoutGraphNode[source.Count];
            for (var index = 0; index < source.Count; index++) copy[index] = source[index];
            return copy;
        }

        private static void AssertSocketEqual(
            RealmLayoutGraphExpansionSocket expected,
            RealmLayoutGraphExpansionSocket actual)
        {
            Assert.That(actual.SocketId, Is.EqualTo(expected.SocketId));
            Assert.That(actual.NodeId, Is.EqualTo(expected.NodeId));
            Assert.That(actual.X, Is.EqualTo(expected.X));
            Assert.That(actual.Z, Is.EqualTo(expected.Z));
        }
    }
}
