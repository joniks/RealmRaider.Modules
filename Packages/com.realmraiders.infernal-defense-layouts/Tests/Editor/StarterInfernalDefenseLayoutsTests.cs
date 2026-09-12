using System;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.InfernalDefenseLayouts.Tests
{
    public sealed class StarterInfernalDefenseLayoutsTests
    {
        [TestCaseSource(nameof(CachedSnapshots))]
        public void CachedLayouts_PreserveExactImmutableFieldSnapshots(
            RealmLayoutGraph layout,
            LayoutSnapshot expected)
        {
            var genericValidation = RealmLayoutGraphValidator.Validate(layout);
            var infernalValidation = InfernalDefenseLayoutValidator.Validate(layout);

            Assert.That(layout.LayoutId, Is.EqualTo(expected.LayoutId));
            Assert.That(layout.DisplayName, Is.EqualTo(expected.DisplayName));
            Assert.That(genericValidation.IsValid, Is.True);
            Assert.That(infernalValidation.IsValid, Is.True);
            AssertNodes(layout.Nodes, expected.Nodes);
            AssertEdges(layout.Edges, expected.Edges);
            AssertLandmarks(layout.Landmarks, expected.Landmarks);
            AssertSockets(layout.ExpansionSockets, expected.Sockets);
        }

        [Test]
        public void Catalogue_UsesCachedIdentityExactRolesAndDistinctTopologyAndSafePathSignatures()
        {
            var catalogue = InfernalDefenseLayoutValidator.ValidateCatalogue(
                StarterInfernalDefenseLayouts.All);
            var topologySignatures = new[]
            {
                InfernalDefenseLayoutValidator.CreateTopologySignature(
                    StarterInfernalDefenseLayouts.AshenSpur),
                InfernalDefenseLayoutValidator.CreateTopologySignature(
                    StarterInfernalDefenseLayouts.CinderFork),
                InfernalDefenseLayoutValidator.CreateTopologySignature(
                    StarterInfernalDefenseLayouts.EmberCircuit)
            };
            var safePathSignatures = new[]
            {
                InfernalDefenseLayoutValidator.CreateSafePathSignature(
                    StarterInfernalDefenseLayouts.AshenSpur),
                InfernalDefenseLayoutValidator.CreateSafePathSignature(
                    StarterInfernalDefenseLayouts.CinderFork),
                InfernalDefenseLayoutValidator.CreateSafePathSignature(
                    StarterInfernalDefenseLayouts.EmberCircuit)
            };

            Assert.That(catalogue.IsValid, Is.True);
            Assert.That(StarterInfernalDefenseLayouts.All[0],
                Is.SameAs(StarterInfernalDefenseLayouts.AshenSpur));
            Assert.That(StarterInfernalDefenseLayouts.All[1],
                Is.SameAs(StarterInfernalDefenseLayouts.CinderFork));
            Assert.That(StarterInfernalDefenseLayouts.All[2],
                Is.SameAs(StarterInfernalDefenseLayouts.EmberCircuit));
            AssertDistinctPairwise(topologySignatures);
            AssertDistinctPairwise(safePathSignatures);

            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                        StarterInfernalDefenseLayouts.RouteJunctionRoleId,
                        StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                        StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                        StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                        StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId
                    },
                    GameplayRoleIds(layout));
                Assert.That(layout.ExpansionSockets.Count, Is.InRange(1, 3));
            }
        }

        [Test]
        public void Select_IsDeterministicAndAvoidsExactPreviousLayout()
        {
            for (var seed = -3; seed <= 3; seed++)
            {
                var first = StarterInfernalDefenseLayoutSelector.Select(seed, null);
                var second = StarterInfernalDefenseLayoutSelector.Select(seed, null);
                var avoided = StarterInfernalDefenseLayoutSelector.Select(seed, first.Layout.LayoutId);

                Assert.That(first.HasLayout, Is.True);
                Assert.That(first.Layout, Is.SameAs(second.Layout));
                Assert.That(avoided.HasLayout, Is.True);
                Assert.That(avoided.Layout.LayoutId, Is.Not.EqualTo(first.Layout.LayoutId));
            }

            var invalidPrevious = StarterInfernalDefenseLayoutSelector.Select(
                0,
                "Infernal.invalid");

            Assert.That(invalidPrevious.Status,
                Is.EqualTo(InfernalDefenseLayoutSelectionStatus.PreviousLayoutIdInvalid));
            Assert.That(invalidPrevious.Layout, Is.Null);
        }

        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        public void Select_ExtremeSeedReturnsCachedIdentityAndAvoidsExactPreviousLayout(int seed)
        {
            var first = StarterInfernalDefenseLayoutSelector.Select(seed, null);
            var second = StarterInfernalDefenseLayoutSelector.Select(seed, null);
            var avoided = StarterInfernalDefenseLayoutSelector.Select(seed, first.Layout.LayoutId);

            Assert.That(first.HasLayout, Is.True);
            Assert.That(first.Layout, Is.SameAs(second.Layout));
            Assert.That(avoided.HasLayout, Is.True);
            Assert.That(avoided.Layout, Is.Not.SameAs(first.Layout));
            Assert.That(avoided.Layout.LayoutId, Is.Not.EqualTo(first.Layout.LayoutId));
        }

        [Test]
        public void ValidateCatalogue_RejectsIdenticalTopologyWhenSafePathFlagsDiffer()
        {
            var source = StarterInfernalDefenseLayouts.AshenSpur;
            var differentSafePaths = CopyWithLayoutIdAndEdges(
                source,
                "realmraiders.infernal-defense.ashen-spur-safe-variant",
                ReplaceEdge(
                    source.Edges,
                    1,
                    new RealmLayoutGraphEdge(
                        source.Edges[1].EdgeId,
                        source.Edges[1].FromNodeId,
                        source.Edges[1].ToNodeId,
                        true,
                        source.Edges[1].FloorPathWidth)));
            var result = InfernalDefenseLayoutValidator.ValidateCatalogue(
                new RealmLayoutGraph[]
                {
                    source,
                    differentSafePaths,
                    StarterInfernalDefenseLayouts.CinderFork
                });

            Assert.That(InfernalDefenseLayoutValidator.Validate(differentSafePaths).IsValid,
                Is.True);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefenseLayoutCatalogueValidationIssue.TopologySignatureDuplicate
                },
                result.Issues);
        }

        [Test]
        public void ValidateCatalogue_RejectsIdenticalSafePathSubgraphWhenRiskTopologyDiffers()
        {
            var source = StarterInfernalDefenseLayouts.AshenSpur;
            var differentRiskTopology = CopyWithLayoutIdAndEdges(
                source,
                "realmraiders.infernal-defense.ashen-spur-risk-variant",
                AppendEdge(
                    source.Edges,
                    new RealmLayoutGraphEdge(
                        "ashen.hellhound-flame-risk",
                        "ashen.hellhound",
                        "ashen.flame",
                        false,
                        6f)));
            var result = InfernalDefenseLayoutValidator.ValidateCatalogue(
                new RealmLayoutGraph[]
                {
                    source,
                    differentRiskTopology,
                    StarterInfernalDefenseLayouts.CinderFork
                });

            Assert.That(InfernalDefenseLayoutValidator.Validate(differentRiskTopology).IsValid,
                Is.True);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefenseLayoutCatalogueValidationIssue.SafePathSignatureDuplicate
                },
                result.Issues);
        }

        [Test]
        public void ResolveExact_ReturnsOnlyExactCachedLayouts()
        {
            var found = StarterInfernalDefenseLayoutResolver.ResolveExact(
                StarterInfernalDefenseLayouts.CinderForkId);
            var unknown = StarterInfernalDefenseLayoutResolver.ResolveExact(
                "realmraiders.infernal-defense.unknown");
            var missing = StarterInfernalDefenseLayoutResolver.ResolveExact(null);
            var changedCase = StarterInfernalDefenseLayoutResolver.ResolveExact(
                "Realmraiders.infernal-defense.cinder-fork");
            var padded = StarterInfernalDefenseLayoutResolver.ResolveExact(
                StarterInfernalDefenseLayouts.CinderForkId + " ");

            Assert.That(found.Status, Is.EqualTo(InfernalDefenseLayoutResolveStatus.Resolved));
            Assert.That(found.Layout, Is.SameAs(StarterInfernalDefenseLayouts.CinderFork));
            Assert.That(unknown.Status,
                Is.EqualTo(InfernalDefenseLayoutResolveStatus.LayoutIdInvalid));
            Assert.That(missing.Status,
                Is.EqualTo(InfernalDefenseLayoutResolveStatus.LayoutIdInvalid));
            Assert.That(changedCase.Status,
                Is.EqualTo(InfernalDefenseLayoutResolveStatus.LayoutIdInvalid));
            Assert.That(padded.Status,
                Is.EqualTo(InfernalDefenseLayoutResolveStatus.LayoutIdInvalid));
        }

        [Test]
        public void Validate_FailsClosedForNullMalformedAndFactionRoleCardinalityInStableOrder()
        {
            var source = StarterInfernalDefenseLayouts.AshenSpur;
            var malformed = CopyWithNodes(
                source,
                ReplaceNode(
                    source.Nodes,
                    0,
                    new RealmLayoutGraphNode(
                        source.Nodes[0].NodeId,
                        source.Nodes[0].GameplayRoleId,
                        source.Nodes[0].Kind,
                        float.NaN,
                        source.Nodes[0].Z)));
            var missingHellhound = CopyWithNodes(
                source,
                ReplaceNode(
                    source.Nodes,
                    2,
                    new RealmLayoutGraphNode(
                        source.Nodes[2].NodeId,
                        "realmraiders.infernal-defense.unknown-role",
                        source.Nodes[2].Kind,
                        source.Nodes[2].X,
                        source.Nodes[2].Z)));
            var unsafePath = CopyWithEdges(
                source,
                ReplaceEdge(
                    source.Edges,
                    2,
                    new RealmLayoutGraphEdge(
                        source.Edges[2].EdgeId,
                        source.Edges[2].FromNodeId,
                        source.Edges[2].ToNodeId,
                        false,
                        source.Edges[2].FloorPathWidth)));
            var nullResult = InfernalDefenseLayoutValidator.Validate(null);
            var malformedResult = InfernalDefenseLayoutValidator.Validate(malformed);
            var missingHellhoundResult = InfernalDefenseLayoutValidator.Validate(missingHellhound);
            var unsafePathResult = InfernalDefenseLayoutValidator.Validate(unsafePath);

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefenseLayoutValidationIssue.LayoutMissing
                },
                nullResult.Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefenseLayoutValidationIssue.GenericGraphInvalid
                },
                malformedResult.Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefenseLayoutValidationIssue.HellhoundRoleCardinalityInvalid,
                    InfernalDefenseLayoutValidationIssue.UnexpectedGameplayRole
                },
                missingHellhoundResult.Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefenseLayoutValidationIssue.GenericGraphInvalid
                },
                unsafePathResult.Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.NodeCoordinateInvalid
                },
                malformedResult.GraphValidation.Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.ActivePathUnsafe
                },
                unsafePathResult.GraphValidation.Issues);
        }

        [Test]
        public void CatalogueAndResultsSnapshotDataAndRemainImmutable()
        {
            var layout = StarterInfernalDefenseLayouts.EmberCircuit;
            var first = InfernalDefenseLayoutValidator.Validate(layout);
            var second = InfernalDefenseLayoutValidator.Validate(layout);
            var invalid = InfernalDefenseLayoutValidator.Validate(null);

            CollectionAssert.AreEqual(first.Issues, second.Issues);
            Assert.That(layout.Nodes[0], Is.Not.Null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutGraph>)StarterInfernalDefenseLayouts.All)[0] = null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<InfernalDefenseLayoutValidationIssue>)invalid.Issues)[0] =
                    InfernalDefenseLayoutValidationIssue.GenericGraphInvalid);
        }

        private static IEnumerable<TestCaseData> CachedSnapshots()
        {
            yield return new TestCaseData(
                    StarterInfernalDefenseLayouts.AshenSpur,
                    new LayoutSnapshot(
                        StarterInfernalDefenseLayouts.AshenSpurId,
                        "Ashen Spur",
                        new[]
                        {
                            Node("ashen.entry", StarterInfernalDefenseLayouts.InvaderEntryRoleId, RealmLayoutGraphNodeKind.Entry, 0f, -36f),
                            Node("ashen.junction", StarterInfernalDefenseLayouts.RouteJunctionRoleId, RealmLayoutGraphNodeKind.Landmark, 0f, -20f),
                            Node("ashen.hellhound", StarterInfernalDefenseLayouts.HellhoundEncounterRoleId, RealmLayoutGraphNodeKind.Encounter, -18f, -4f),
                            Node("ashen.flame", StarterInfernalDefenseLayouts.FlameTrapHazardRoleId, RealmLayoutGraphNodeKind.Encounter, 0f, 8f),
                            Node("ashen.brute", StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId, RealmLayoutGraphNodeKind.Encounter, 16f, 24f),
                            Node("ashen.heart", StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId, RealmLayoutGraphNodeKind.Objective, 16f, 42f)
                        },
                        new[]
                        {
                            Edge("ashen.entry-junction", "ashen.entry", "ashen.junction", true, 7f),
                            Edge("ashen.junction-hellhound", "ashen.junction", "ashen.hellhound", false, 6f),
                            Edge("ashen.junction-flame", "ashen.junction", "ashen.flame", true, 7f),
                            Edge("ashen.flame-brute", "ashen.flame", "ashen.brute", true, 7f),
                            Edge("ashen.brute-heart", "ashen.brute", "ashen.heart", true, 7f)
                        },
                        new[]
                        {
                            Landmark("ashen.junction.crucible", "realmraiders.infernal-defense.ash-crucible", "ashen.junction"),
                            Landmark("ashen.heart.altar", "realmraiders.infernal-defense.heart-altar", "ashen.heart")
                        },
                        new[]
                        {
                            Socket("ashen.west-vent", "ashen.hellhound", -30f, 6f),
                            Socket("ashen.east-vent", "ashen.brute", 30f, 24f)
                        }))
                .SetName("ashen-spur-snapshot");
            yield return new TestCaseData(
                    StarterInfernalDefenseLayouts.CinderFork,
                    new LayoutSnapshot(
                        StarterInfernalDefenseLayouts.CinderForkId,
                        "Cinder Fork",
                        new[]
                        {
                            Node("cinder.entry", StarterInfernalDefenseLayouts.InvaderEntryRoleId, RealmLayoutGraphNodeKind.Entry, -28f, -36f),
                            Node("cinder.junction", StarterInfernalDefenseLayouts.RouteJunctionRoleId, RealmLayoutGraphNodeKind.Landmark, -12f, -20f),
                            Node("cinder.hellhound", StarterInfernalDefenseLayouts.HellhoundEncounterRoleId, RealmLayoutGraphNodeKind.Encounter, -28f, 0f),
                            Node("cinder.flame", StarterInfernalDefenseLayouts.FlameTrapHazardRoleId, RealmLayoutGraphNodeKind.Encounter, 4f, -2f),
                            Node("cinder.brute", StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId, RealmLayoutGraphNodeKind.Encounter, 18f, 18f),
                            Node("cinder.heart", StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId, RealmLayoutGraphNodeKind.Objective, 18f, 38f)
                        },
                        new[]
                        {
                            Edge("cinder.entry-junction", "cinder.entry", "cinder.junction", true, 7f),
                            Edge("cinder.junction-hellhound", "cinder.junction", "cinder.hellhound", true, 7f),
                            Edge("cinder.junction-flame", "cinder.junction", "cinder.flame", false, 6f),
                            Edge("cinder.hellhound-brute", "cinder.hellhound", "cinder.brute", true, 7f),
                            Edge("cinder.flame-brute", "cinder.flame", "cinder.brute", false, 6f),
                            Edge("cinder.brute-heart", "cinder.brute", "cinder.heart", true, 7f)
                        },
                        new[]
                        {
                            Landmark("cinder.junction.fork", "realmraiders.infernal-defense.cinder-fork", "cinder.junction"),
                            Landmark("cinder.heart.altar", "realmraiders.infernal-defense.heart-altar", "cinder.heart")
                        },
                        new[]
                        {
                            Socket("cinder.north-vent", "cinder.hellhound", -30f, 14f),
                            Socket("cinder.east-vent", "cinder.brute", 32f, 18f)
                        }))
                .SetName("cinder-fork-snapshot");
            yield return new TestCaseData(
                    StarterInfernalDefenseLayouts.EmberCircuit,
                    new LayoutSnapshot(
                        StarterInfernalDefenseLayouts.EmberCircuitId,
                        "Ember Circuit",
                        new[]
                        {
                            Node("ember.entry", StarterInfernalDefenseLayouts.InvaderEntryRoleId, RealmLayoutGraphNodeKind.Entry, 0f, -40f),
                            Node("ember.junction", StarterInfernalDefenseLayouts.RouteJunctionRoleId, RealmLayoutGraphNodeKind.Landmark, 0f, -22f),
                            Node("ember.hellhound", StarterInfernalDefenseLayouts.HellhoundEncounterRoleId, RealmLayoutGraphNodeKind.Encounter, -22f, -4f),
                            Node("ember.flame", StarterInfernalDefenseLayouts.FlameTrapHazardRoleId, RealmLayoutGraphNodeKind.Encounter, 0f, 12f),
                            Node("ember.brute", StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId, RealmLayoutGraphNodeKind.Encounter, 22f, 28f),
                            Node("ember.heart", StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId, RealmLayoutGraphNodeKind.Objective, 22f, 48f)
                        },
                        new[]
                        {
                            Edge("ember.entry-junction", "ember.entry", "ember.junction", true, 7f),
                            Edge("ember.junction-hellhound", "ember.junction", "ember.hellhound", true, 7f),
                            Edge("ember.hellhound-flame", "ember.hellhound", "ember.flame", false, 6f),
                            Edge("ember.flame-brute", "ember.flame", "ember.brute", true, 7f),
                            Edge("ember.junction-brute", "ember.junction", "ember.brute", true, 7f),
                            Edge("ember.brute-heart", "ember.brute", "ember.heart", true, 7f)
                        },
                        new[]
                        {
                            Landmark("ember.junction.brazier", "realmraiders.infernal-defense.circuit-brazier", "ember.junction"),
                            Landmark("ember.heart.altar", "realmraiders.infernal-defense.heart-altar", "ember.heart")
                        },
                        new[]
                        {
                            Socket("ember.west-vent", "ember.hellhound", -34f, 8f),
                            Socket("ember.south-vent", "ember.junction", -12f, -34f),
                            Socket("ember.east-vent", "ember.brute", 36f, 28f)
                        }))
                .SetName("ember-circuit-snapshot");
        }

        private static void AssertNodes(
            IReadOnlyList<RealmLayoutGraphNode> actual,
            IReadOnlyList<NodeSnapshot> expected)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            for (var index = 0; index < expected.Count; index++)
            {
                Assert.That(actual[index].NodeId, Is.EqualTo(expected[index].NodeId));
                Assert.That(actual[index].GameplayRoleId, Is.EqualTo(expected[index].GameplayRoleId));
                Assert.That(actual[index].Kind, Is.EqualTo(expected[index].Kind));
                Assert.That(actual[index].X, Is.EqualTo(expected[index].X));
                Assert.That(actual[index].Z, Is.EqualTo(expected[index].Z));
            }
        }

        private static void AssertEdges(
            IReadOnlyList<RealmLayoutGraphEdge> actual,
            IReadOnlyList<EdgeSnapshot> expected)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            for (var index = 0; index < expected.Count; index++)
            {
                Assert.That(actual[index].EdgeId, Is.EqualTo(expected[index].EdgeId));
                Assert.That(actual[index].FromNodeId, Is.EqualTo(expected[index].FromNodeId));
                Assert.That(actual[index].ToNodeId, Is.EqualTo(expected[index].ToNodeId));
                Assert.That(actual[index].IsActivePathSafe, Is.EqualTo(expected[index].IsActivePathSafe));
                Assert.That(actual[index].FloorPathWidth, Is.EqualTo(expected[index].FloorPathWidth));
            }
        }

        private static void AssertLandmarks(
            IReadOnlyList<RealmLayoutGraphLandmark> actual,
            IReadOnlyList<LandmarkSnapshot> expected)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            for (var index = 0; index < expected.Count; index++)
            {
                Assert.That(actual[index].LandmarkId, Is.EqualTo(expected[index].LandmarkId));
                Assert.That(actual[index].PresentationRoleId,
                    Is.EqualTo(expected[index].PresentationRoleId));
                Assert.That(actual[index].NodeId, Is.EqualTo(expected[index].NodeId));
            }
        }

        private static void AssertSockets(
            IReadOnlyList<RealmLayoutGraphExpansionSocket> actual,
            IReadOnlyList<SocketSnapshot> expected)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            for (var index = 0; index < expected.Count; index++)
            {
                Assert.That(actual[index].SocketId, Is.EqualTo(expected[index].SocketId));
                Assert.That(actual[index].NodeId, Is.EqualTo(expected[index].NodeId));
                Assert.That(actual[index].X, Is.EqualTo(expected[index].X));
                Assert.That(actual[index].Z, Is.EqualTo(expected[index].Z));
            }
        }

        private static IReadOnlyList<string> GameplayRoleIds(RealmLayoutGraph layout)
        {
            var roleIds = new string[layout.Nodes.Count];
            for (var index = 0; index < layout.Nodes.Count; index++)
            {
                roleIds[index] = layout.Nodes[index].GameplayRoleId;
            }

            return roleIds;
        }

        private static RealmLayoutGraph CopyWithNodes(
            RealmLayoutGraph source,
            IReadOnlyList<RealmLayoutGraphNode> nodes)
        {
            return new RealmLayoutGraph(
                source.LayoutId,
                source.DisplayName,
                nodes,
                source.Edges,
                source.Landmarks,
                source.ExpansionSockets);
        }

        private static RealmLayoutGraph CopyWithEdges(
            RealmLayoutGraph source,
            IReadOnlyList<RealmLayoutGraphEdge> edges)
        {
            return CopyWithLayoutIdAndEdges(source, source.LayoutId, edges);
        }

        private static RealmLayoutGraph CopyWithLayoutIdAndEdges(
            RealmLayoutGraph source,
            string layoutId,
            IReadOnlyList<RealmLayoutGraphEdge> edges)
        {
            return new RealmLayoutGraph(
                layoutId,
                source.DisplayName,
                source.Nodes,
                edges,
                source.Landmarks,
                source.ExpansionSockets);
        }

        private static IReadOnlyList<RealmLayoutGraphNode> ReplaceNode(
            IReadOnlyList<RealmLayoutGraphNode> source,
            int index,
            RealmLayoutGraphNode replacement)
        {
            var copy = new RealmLayoutGraphNode[source.Count];
            for (var sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
            {
                copy[sourceIndex] = sourceIndex == index ? replacement : source[sourceIndex];
            }

            return copy;
        }

        private static IReadOnlyList<RealmLayoutGraphEdge> ReplaceEdge(
            IReadOnlyList<RealmLayoutGraphEdge> source,
            int index,
            RealmLayoutGraphEdge replacement)
        {
            var copy = new RealmLayoutGraphEdge[source.Count];
            for (var sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
            {
                copy[sourceIndex] = sourceIndex == index ? replacement : source[sourceIndex];
            }

            return copy;
        }

        private static IReadOnlyList<RealmLayoutGraphEdge> AppendEdge(
            IReadOnlyList<RealmLayoutGraphEdge> source,
            RealmLayoutGraphEdge appended)
        {
            var copy = new RealmLayoutGraphEdge[source.Count + 1];
            for (var sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
            {
                copy[sourceIndex] = source[sourceIndex];
            }

            copy[source.Count] = appended;
            return copy;
        }

        private static void AssertDistinctPairwise(IReadOnlyList<string> signatures)
        {
            Assert.That(signatures[0], Is.Not.EqualTo(signatures[1]));
            Assert.That(signatures[0], Is.Not.EqualTo(signatures[2]));
            Assert.That(signatures[1], Is.Not.EqualTo(signatures[2]));
        }

        private static NodeSnapshot Node(
            string nodeId,
            string gameplayRoleId,
            RealmLayoutGraphNodeKind kind,
            float x,
            float z)
        {
            return new NodeSnapshot(nodeId, gameplayRoleId, kind, x, z);
        }

        private static EdgeSnapshot Edge(
            string edgeId,
            string fromNodeId,
            string toNodeId,
            bool isActivePathSafe,
            float floorPathWidth)
        {
            return new EdgeSnapshot(
                edgeId,
                fromNodeId,
                toNodeId,
                isActivePathSafe,
                floorPathWidth);
        }

        private static LandmarkSnapshot Landmark(
            string landmarkId,
            string presentationRoleId,
            string nodeId)
        {
            return new LandmarkSnapshot(landmarkId, presentationRoleId, nodeId);
        }

        private static SocketSnapshot Socket(
            string socketId,
            string nodeId,
            float x,
            float z)
        {
            return new SocketSnapshot(socketId, nodeId, x, z);
        }

        public sealed class LayoutSnapshot
        {
            public LayoutSnapshot(
                string layoutId,
                string displayName,
                IReadOnlyList<NodeSnapshot> nodes,
                IReadOnlyList<EdgeSnapshot> edges,
                IReadOnlyList<LandmarkSnapshot> landmarks,
                IReadOnlyList<SocketSnapshot> sockets)
            {
                LayoutId = layoutId;
                DisplayName = displayName;
                Nodes = nodes;
                Edges = edges;
                Landmarks = landmarks;
                Sockets = sockets;
            }

            public string LayoutId { get; }

            public string DisplayName { get; }

            public IReadOnlyList<NodeSnapshot> Nodes { get; }

            public IReadOnlyList<EdgeSnapshot> Edges { get; }

            public IReadOnlyList<LandmarkSnapshot> Landmarks { get; }

            public IReadOnlyList<SocketSnapshot> Sockets { get; }
        }

        public sealed class NodeSnapshot
        {
            public NodeSnapshot(
                string nodeId,
                string gameplayRoleId,
                RealmLayoutGraphNodeKind kind,
                float x,
                float z)
            {
                NodeId = nodeId;
                GameplayRoleId = gameplayRoleId;
                Kind = kind;
                X = x;
                Z = z;
            }

            public string NodeId { get; }

            public string GameplayRoleId { get; }

            public RealmLayoutGraphNodeKind Kind { get; }

            public float X { get; }

            public float Z { get; }
        }

        public sealed class EdgeSnapshot
        {
            public EdgeSnapshot(
                string edgeId,
                string fromNodeId,
                string toNodeId,
                bool isActivePathSafe,
                float floorPathWidth)
            {
                EdgeId = edgeId;
                FromNodeId = fromNodeId;
                ToNodeId = toNodeId;
                IsActivePathSafe = isActivePathSafe;
                FloorPathWidth = floorPathWidth;
            }

            public string EdgeId { get; }

            public string FromNodeId { get; }

            public string ToNodeId { get; }

            public bool IsActivePathSafe { get; }

            public float FloorPathWidth { get; }
        }

        public sealed class LandmarkSnapshot
        {
            public LandmarkSnapshot(
                string landmarkId,
                string presentationRoleId,
                string nodeId)
            {
                LandmarkId = landmarkId;
                PresentationRoleId = presentationRoleId;
                NodeId = nodeId;
            }

            public string LandmarkId { get; }

            public string PresentationRoleId { get; }

            public string NodeId { get; }
        }

        public sealed class SocketSnapshot
        {
            public SocketSnapshot(
                string socketId,
                string nodeId,
                float x,
                float z)
            {
                SocketId = socketId;
                NodeId = nodeId;
                X = x;
                Z = z;
            }

            public string SocketId { get; }

            public string NodeId { get; }

            public float X { get; }

            public float Z { get; }
        }
    }
}
