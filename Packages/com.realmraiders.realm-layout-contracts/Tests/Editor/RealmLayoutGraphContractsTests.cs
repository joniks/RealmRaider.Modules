using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace RealmRaiders.Modules.RealmLayoutContracts.Tests
{
    public sealed class RealmLayoutGraphContractsTests
    {
        [Test]
        public void Validate_AcceptsFactionNeutralStableRoleAndPresentationIds()
        {
            var graph = ValidGraph();
            var result = RealmLayoutGraphValidator.Validate(graph);

            Assert.That(result.IsValid, Is.True);
            Assert.That(graph.Nodes[0].GameplayRoleId, Is.EqualTo("faction.entry"));
            Assert.That(graph.Landmarks[0].PresentationRoleId,
                Is.EqualTo("faction.junction-marker"));
        }

        [Test]
        public void Validate_UsesFootprintBasedNodeSpacingAtAndInsideTheExactBoundary()
        {
            var exactBoundary = TwoNodeGraph(RealmLayoutGraphValidator.MinimumNodeSpacing);
            var justInside = TwoNodeGraph(
                RealmLayoutGraphValidator.MinimumNodeSpacing - 0.001f);

            Assert.That(RealmLayoutGraphValidator.MinimumNodeSpacing, Is.EqualTo(6.75f));
            Assert.That(RealmLayoutGraphValidator.Validate(exactBoundary).IsValid, Is.True);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.NodeSpacingInvalid
                },
                RealmLayoutGraphValidator.Validate(justInside).Issues);
        }

        [Test]
        public void Validate_FailsClosedForMalformedDuplicateAndNonfiniteFactsInStableOrder()
        {
            var malformed = Copy(
                ValidGraph(),
                "Faction.layout",
                null,
                null,
                null,
                null);
            var duplicate = Copy(
                ValidGraph(),
                null,
                null,
                new RealmLayoutGraphEdge[]
                {
                    new RealmLayoutGraphEdge("edge.entry-junction", "node.entry", "node.junction", true, 7f),
                    new RealmLayoutGraphEdge("edge.reverse", "node.junction", "node.entry", true, 7f),
                    new RealmLayoutGraphEdge("edge.junction-encounter", "node.junction", "node.encounter", true, 7f),
                    new RealmLayoutGraphEdge("edge.encounter-objective", "node.encounter", "node.objective", true, 7f)
                },
                null,
                null);
            var nonfinite = Copy(
                ValidGraph(),
                null,
                new RealmLayoutGraphNode[]
                {
                    new RealmLayoutGraphNode("node.entry", "faction.entry", RealmLayoutGraphNodeKind.Entry, 0f, 0f),
                    new RealmLayoutGraphNode("node.junction", "faction.junction", RealmLayoutGraphNodeKind.Landmark, 0f, 15f),
                    new RealmLayoutGraphNode("node.encounter", "faction.encounter", RealmLayoutGraphNodeKind.Encounter, 0f, 30f),
                    new RealmLayoutGraphNode("node.objective", "faction.objective", RealmLayoutGraphNodeKind.Objective, 0f, float.NaN)
                },
                null,
                null,
                null);

            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.LayoutIdInvalid
                },
                RealmLayoutGraphValidator.Validate(malformed).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.EdgeDuplicate
                },
                RealmLayoutGraphValidator.Validate(duplicate).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.NodeCoordinateInvalid
                },
                RealmLayoutGraphValidator.Validate(nonfinite).Issues);
        }

        [Test]
        public void Validate_FailsClosedForDisconnectedUnsafeAndCrossingGeometry()
        {
            var disconnected = Copy(
                ValidGraph(),
                null,
                null,
                new RealmLayoutGraphEdge[]
                {
                    new RealmLayoutGraphEdge("edge.entry-junction", "node.entry", "node.junction", true, 7f),
                    new RealmLayoutGraphEdge("edge.junction-encounter", "node.junction", "node.encounter", true, 7f)
                },
                null,
                null);
            var unsafePath = Copy(
                ValidGraph(),
                null,
                null,
                new RealmLayoutGraphEdge[]
                {
                    new RealmLayoutGraphEdge("edge.entry-junction", "node.entry", "node.junction", true, 7f),
                    new RealmLayoutGraphEdge("edge.junction-encounter", "node.junction", "node.encounter", true, 7f),
                    new RealmLayoutGraphEdge("edge.encounter-objective", "node.encounter", "node.objective", false, 7f)
                },
                null,
                null);
            var crossing = CrossingGraph();

            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.LayoutDisconnected,
                    RealmLayoutGraphValidationIssue.ActivePathUnsafe
                },
                RealmLayoutGraphValidator.Validate(disconnected).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.ActivePathUnsafe
                },
                RealmLayoutGraphValidator.Validate(unsafePath).Issues);
            CollectionAssert.Contains(
                RealmLayoutGraphValidator.Validate(crossing).Issues,
                RealmLayoutGraphValidationIssue.CorridorClearanceInvalid);
        }

        [Test]
        public void GraphAndValidationSnapshotsRemainImmutableAndDeterministic()
        {
            var nodes = new[]
            {
                new RealmLayoutGraphNode("node.entry", "faction.entry", RealmLayoutGraphNodeKind.Entry, 0f, 0f),
                new RealmLayoutGraphNode("node.objective", "faction.objective", RealmLayoutGraphNodeKind.Objective, 0f, 15f)
            };
            var graph = new RealmLayoutGraph(
                "faction.snapshot",
                "Snapshot",
                nodes,
                new RealmLayoutGraphEdge[]
                {
                    new RealmLayoutGraphEdge("edge.path", "node.entry", "node.objective", true, 7f)
                },
                new RealmLayoutGraphLandmark[]
                {
                    new RealmLayoutGraphLandmark("landmark.entry", "faction.marker", "node.entry")
                },
                Array.Empty<RealmLayoutGraphExpansionSocket>());
            var first = RealmLayoutGraphValidator.Validate(graph);
            var second = RealmLayoutGraphValidator.Validate(graph);
            var invalid = RealmLayoutGraphValidator.Validate(null);

            nodes[0] = null;

            Assert.That(first.IsValid, Is.True);
            Assert.That(graph.Nodes[0], Is.Not.Null);
            CollectionAssert.AreEqual(first.Issues, second.Issues);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutGraphNode>)graph.Nodes)[0] = null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutGraphValidationIssue>)invalid.Issues)[0] =
                    RealmLayoutGraphValidationIssue.GraphMissing);
        }

        private static RealmLayoutGraph ValidGraph()
        {
            return new RealmLayoutGraph(
                "faction.valid-layout",
                "Faction Valid Layout",
                new RealmLayoutGraphNode[]
                {
                    new RealmLayoutGraphNode("node.entry", "faction.entry", RealmLayoutGraphNodeKind.Entry, 0f, 0f),
                    new RealmLayoutGraphNode("node.junction", "faction.junction", RealmLayoutGraphNodeKind.Landmark, 0f, 15f),
                    new RealmLayoutGraphNode("node.encounter", "faction.encounter", RealmLayoutGraphNodeKind.Encounter, 0f, 30f),
                    new RealmLayoutGraphNode("node.objective", "faction.objective", RealmLayoutGraphNodeKind.Objective, 0f, 45f)
                },
                new RealmLayoutGraphEdge[]
                {
                    new RealmLayoutGraphEdge("edge.entry-junction", "node.entry", "node.junction", true, 7f),
                    new RealmLayoutGraphEdge("edge.junction-encounter", "node.junction", "node.encounter", true, 7f),
                    new RealmLayoutGraphEdge("edge.encounter-objective", "node.encounter", "node.objective", true, 7f)
                },
                new RealmLayoutGraphLandmark[]
                {
                    new RealmLayoutGraphLandmark("landmark.junction", "faction.junction-marker", "node.junction")
                },
                new RealmLayoutGraphExpansionSocket[]
                {
                    new RealmLayoutGraphExpansionSocket("socket.encounter", "node.encounter", 10f, 30f)
                });
        }

        private static RealmLayoutGraph CrossingGraph()
        {
            return new RealmLayoutGraph(
                "faction.crossing-layout",
                "Crossing Layout",
                new RealmLayoutGraphNode[]
                {
                    new RealmLayoutGraphNode("node.entry", "faction.entry", RealmLayoutGraphNodeKind.Entry, -25f, -25f),
                    new RealmLayoutGraphNode("node.junction", "faction.junction", RealmLayoutGraphNodeKind.Landmark, -25f, 25f),
                    new RealmLayoutGraphNode("node.encounter", "faction.encounter", RealmLayoutGraphNodeKind.Encounter, 25f, -25f),
                    new RealmLayoutGraphNode("node.objective", "faction.objective", RealmLayoutGraphNodeKind.Objective, 25f, 25f)
                },
                new RealmLayoutGraphEdge[]
                {
                    new RealmLayoutGraphEdge("edge.entry-objective", "node.entry", "node.objective", true, 7f),
                    new RealmLayoutGraphEdge("edge.junction-encounter", "node.junction", "node.encounter", true, 7f),
                    new RealmLayoutGraphEdge("edge.entry-junction", "node.entry", "node.junction", true, 7f),
                    new RealmLayoutGraphEdge("edge.encounter-objective", "node.encounter", "node.objective", true, 7f)
                },
                new RealmLayoutGraphLandmark[]
                {
                    new RealmLayoutGraphLandmark("landmark.junction", "faction.junction-marker", "node.junction")
                },
                Array.Empty<RealmLayoutGraphExpansionSocket>());
        }

        private static RealmLayoutGraph TwoNodeGraph(float objectiveZ)
        {
            return new RealmLayoutGraph(
                "faction.spacing-layout",
                "Spacing Layout",
                new RealmLayoutGraphNode[]
                {
                    new RealmLayoutGraphNode(
                        "node.entry",
                        "faction.entry",
                        RealmLayoutGraphNodeKind.Entry,
                        0f,
                        0f),
                    new RealmLayoutGraphNode(
                        "node.objective",
                        "faction.objective",
                        RealmLayoutGraphNodeKind.Objective,
                        0f,
                        objectiveZ)
                },
                new RealmLayoutGraphEdge[]
                {
                    new RealmLayoutGraphEdge(
                        "edge.path",
                        "node.entry",
                        "node.objective",
                        true,
                        7f)
                },
                Array.Empty<RealmLayoutGraphLandmark>(),
                Array.Empty<RealmLayoutGraphExpansionSocket>());
        }

        private static RealmLayoutGraph Copy(
            RealmLayoutGraph source,
            string layoutId,
            IReadOnlyList<RealmLayoutGraphNode> nodes,
            IReadOnlyList<RealmLayoutGraphEdge> edges,
            IReadOnlyList<RealmLayoutGraphLandmark> landmarks,
            IReadOnlyList<RealmLayoutGraphExpansionSocket> expansionSockets)
        {
            return new RealmLayoutGraph(
                layoutId ?? source.LayoutId,
                source.DisplayName,
                nodes ?? source.Nodes,
                edges ?? source.Edges,
                landmarks ?? source.Landmarks,
                expansionSockets ?? source.ExpansionSockets);
        }
    }
}
