using System;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.StarterRealmLayouts.Tests
{
    public sealed class StarterSylvanRealmLayoutGraphAdapterTests
    {
        [TestCaseSource(nameof(CachedRecipes))]
        public void Adapt_PreservesCachedSylvanStructuralSnapshots(
            RealmLayoutRecipe recipe)
        {
            var graph = StarterSylvanRealmLayoutGraphAdapter.Adapt(recipe);
            var sylvanValidation = RealmLayoutRecipeValidator.ValidateStarterRecipe(recipe);
            var genericValidation = RealmLayoutGraphValidator.Validate(graph);

            Assert.That(sylvanValidation.IsValid, Is.True);
            Assert.That(genericValidation.IsValid, Is.True);
            Assert.That(graph.LayoutId, Is.EqualTo(recipe.LayoutId));
            Assert.That(graph.DisplayName, Is.EqualTo(recipe.DisplayName));
            Assert.That(graph.Nodes.Count, Is.EqualTo(recipe.Nodes.Count));
            Assert.That(graph.Edges.Count, Is.EqualTo(recipe.Edges.Count));
            Assert.That(graph.Landmarks.Count, Is.EqualTo(recipe.Landmarks.Count));
            Assert.That(graph.ExpansionSockets.Count, Is.EqualTo(recipe.ExpansionSockets.Count));

            for (var index = 0; index < recipe.Nodes.Count; index++)
            {
                Assert.That(graph.Nodes[index].NodeId, Is.EqualTo(recipe.Nodes[index].NodeId));
                Assert.That(graph.Nodes[index].Kind,
                    Is.EqualTo(ExpectedKind(recipe.Nodes[index].Kind)));
                Assert.That(graph.Nodes[index].GameplayRoleId,
                    Is.EqualTo(ExpectedGameplayRoleId(recipe.Nodes[index].MaterializationRole)));
                Assert.That(graph.Nodes[index].X, Is.EqualTo(recipe.Nodes[index].X));
                Assert.That(graph.Nodes[index].Z, Is.EqualTo(recipe.Nodes[index].Z));
            }

            for (var index = 0; index < recipe.Edges.Count; index++)
            {
                Assert.That(graph.Edges[index].EdgeId, Is.EqualTo(recipe.Edges[index].EdgeId));
                Assert.That(graph.Edges[index].FromNodeId,
                    Is.EqualTo(recipe.Edges[index].FromNodeId));
                Assert.That(graph.Edges[index].ToNodeId,
                    Is.EqualTo(recipe.Edges[index].ToNodeId));
                Assert.That(graph.Edges[index].IsActivePathSafe,
                    Is.EqualTo(recipe.Edges[index].IsActivePathSafe));
                Assert.That(graph.Edges[index].FloorPathWidth,
                    Is.EqualTo(recipe.Edges[index].FloorPathWidth));
            }

            for (var index = 0; index < recipe.Landmarks.Count; index++)
            {
                Assert.That(graph.Landmarks[index].LandmarkId,
                    Is.EqualTo(recipe.Landmarks[index].LandmarkId));
                Assert.That(graph.Landmarks[index].NodeId,
                    Is.EqualTo(recipe.Landmarks[index].NodeId));
                Assert.That(graph.Landmarks[index].PresentationRoleId,
                    Is.EqualTo(ExpectedPresentationRoleId(recipe.Landmarks[index].VisualRole)));
            }

            for (var index = 0; index < recipe.ExpansionSockets.Count; index++)
            {
                Assert.That(graph.ExpansionSockets[index].SocketId,
                    Is.EqualTo(recipe.ExpansionSockets[index].SocketId));
                Assert.That(graph.ExpansionSockets[index].NodeId,
                    Is.EqualTo(recipe.ExpansionSockets[index].NodeId));
                Assert.That(graph.ExpansionSockets[index].X,
                    Is.EqualTo(recipe.ExpansionSockets[index].X));
                Assert.That(graph.ExpansionSockets[index].Z,
                    Is.EqualTo(recipe.ExpansionSockets[index].Z));
            }
        }

        [Test]
        public void Adapt_MapsSylvanRolesToStableFactionNeutralIdsWithoutChangingSource()
        {
            var source = StarterSylvanRealmLayouts.AncientCrossroads;
            var graph = StarterSylvanRealmLayoutGraphAdapter.Adapt(source);

            CollectionAssert.AreEqual(
                new[]
                {
                    "realmraiders.sylvan.portal-start",
                    "realmraiders.sylvan.landmark-junction",
                    "realmraiders.sylvan.wolf-grove-encounter",
                    "realmraiders.sylvan.root-path-hazard",
                    "realmraiders.sylvan.ent-grove-encounter",
                    "realmraiders.sylvan.moonwell-recovery",
                    "realmraiders.sylvan.heart-tree-objective"
                },
                GameplayRoleIds(graph));
            CollectionAssert.AreEqual(
                new[]
                {
                    "realmraiders.sylvan.node-canopy",
                    "realmraiders.sylvan.heart-tree"
                },
                PresentationRoleIds(graph));
            Assert.That(source.Nodes[0].MaterializationRole,
                Is.EqualTo(SylvanRealmNodeMaterializationRole.PortalStart));
            Assert.That(source.Landmarks[0].VisualRole,
                Is.EqualTo(SylvanLandmarkVisualRole.NodeCanopy));
        }

        [Test]
        public void Adapt_ReturnsImmutableDeterministicSnapshots()
        {
            var first = StarterSylvanRealmLayoutGraphAdapter.Adapt(
                StarterSylvanRealmLayouts.ForkedCanopy);
            var second = StarterSylvanRealmLayoutGraphAdapter.Adapt(
                StarterSylvanRealmLayouts.ForkedCanopy);

            Assert.That(first.LayoutId, Is.EqualTo(second.LayoutId));
            Assert.That(first.Nodes[0].NodeId, Is.EqualTo(second.Nodes[0].NodeId));
            Assert.That(first.Nodes[0], Is.Not.SameAs(second.Nodes[0]));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutGraphNode>)first.Nodes)[0] = null);
        }

        [Test]
        public void AdaptExactCached_ReturnsOneTokenOnlyForTheExactCachedRecipe()
        {
            var source = StarterSylvanRealmLayouts.ForkedCanopy;
            var first = StarterSylvanRealmLayoutGraphAdapter.AdaptExactCached(source);
            var second = StarterSylvanRealmLayoutGraphAdapter.AdaptExactCached(source);
            var copy = CopyRecipe(source, source.Nodes, source.Landmarks);

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
            Assert.That(StarterSylvanRealmLayoutGraphAdapter.AdaptExactCached(copy), Is.Null);
        }

        [Test]
        public void Adapt_FailsClosedForInvalidEnumAndNullElements()
        {
            var source = StarterSylvanRealmLayouts.AncientCrossroads;
            var invalidRoleNodes = new RealmLayoutNode[]
            {
                new RealmLayoutNode(
                    source.Nodes[0].NodeId,
                    source.Nodes[0].ContentId,
                    source.Nodes[0].Kind,
                    (SylvanRealmNodeMaterializationRole)(-1),
                    source.Nodes[0].X,
                    source.Nodes[0].Z),
                source.Nodes[1],
                source.Nodes[2],
                source.Nodes[3],
                source.Nodes[4],
                source.Nodes[5],
                source.Nodes[6]
            };
            var nullLandmarks = new RealmLayoutLandmark[]
            {
                null,
                source.Landmarks[1]
            };
            var invalidRoleGraph = StarterSylvanRealmLayoutGraphAdapter.Adapt(
                CopyRecipe(source, invalidRoleNodes, source.Landmarks));
            var nullLandmarkGraph = StarterSylvanRealmLayoutGraphAdapter.Adapt(
                CopyRecipe(source, source.Nodes, nullLandmarks));

            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.NodeGameplayRoleIdInvalid
                },
                RealmLayoutGraphValidator.Validate(invalidRoleGraph).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutGraphValidationIssue.LandmarkMissing
                },
                RealmLayoutGraphValidator.Validate(nullLandmarkGraph).Issues);
        }

        private static IEnumerable<TestCaseData> CachedRecipes()
        {
            yield return new TestCaseData(StarterSylvanRealmLayouts.AncientCrossroads)
                .SetName("ancient-crossroads");
            yield return new TestCaseData(StarterSylvanRealmLayouts.ForkedCanopy)
                .SetName("forked-canopy");
            yield return new TestCaseData(StarterSylvanRealmLayouts.SerpentRoots)
                .SetName("serpent-roots");
        }

        private static IReadOnlyList<string> GameplayRoleIds(RealmLayoutGraph graph)
        {
            var roleIds = new string[graph.Nodes.Count];
            for (var index = 0; index < graph.Nodes.Count; index++)
            {
                roleIds[index] = graph.Nodes[index].GameplayRoleId;
            }

            return roleIds;
        }

        private static IReadOnlyList<string> PresentationRoleIds(RealmLayoutGraph graph)
        {
            var roleIds = new string[graph.Landmarks.Count];
            for (var index = 0; index < graph.Landmarks.Count; index++)
            {
                roleIds[index] = graph.Landmarks[index].PresentationRoleId;
            }

            return roleIds;
        }

        private static RealmLayoutRecipe CopyRecipe(
            RealmLayoutRecipe source,
            IReadOnlyList<RealmLayoutNode> nodes,
            IReadOnlyList<RealmLayoutLandmark> landmarks)
        {
            return new RealmLayoutRecipe(
                source.LayoutId,
                source.DisplayName,
                nodes,
                source.Edges,
                landmarks,
                source.ExpansionSockets);
        }

        private static RealmLayoutGraphNodeKind ExpectedKind(RealmLayoutNodeKind kind)
        {
            switch (kind)
            {
                case RealmLayoutNodeKind.Start:
                    return RealmLayoutGraphNodeKind.Entry;
                case RealmLayoutNodeKind.Encounter:
                    return RealmLayoutGraphNodeKind.Encounter;
                case RealmLayoutNodeKind.Landmark:
                    return RealmLayoutGraphNodeKind.Landmark;
                case RealmLayoutNodeKind.Core:
                    return RealmLayoutGraphNodeKind.Objective;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static string ExpectedGameplayRoleId(SylvanRealmNodeMaterializationRole role)
        {
            switch (role)
            {
                case SylvanRealmNodeMaterializationRole.PortalStart:
                    return "realmraiders.sylvan.portal-start";
                case SylvanRealmNodeMaterializationRole.WolfGroveEncounter:
                    return "realmraiders.sylvan.wolf-grove-encounter";
                case SylvanRealmNodeMaterializationRole.RootPathHazard:
                    return "realmraiders.sylvan.root-path-hazard";
                case SylvanRealmNodeMaterializationRole.EntGroveEncounter:
                    return "realmraiders.sylvan.ent-grove-encounter";
                case SylvanRealmNodeMaterializationRole.MoonwellRecovery:
                    return "realmraiders.sylvan.moonwell-recovery";
                case SylvanRealmNodeMaterializationRole.LandmarkJunction:
                    return "realmraiders.sylvan.landmark-junction";
                case SylvanRealmNodeMaterializationRole.HeartTreeObjective:
                    return "realmraiders.sylvan.heart-tree-objective";
                default:
                    throw new ArgumentOutOfRangeException(nameof(role));
            }
        }

        private static string ExpectedPresentationRoleId(SylvanLandmarkVisualRole role)
        {
            switch (role)
            {
                case SylvanLandmarkVisualRole.NodeCanopy:
                    return "realmraiders.sylvan.node-canopy";
                case SylvanLandmarkVisualRole.SylvanHeartTree:
                    return "realmraiders.sylvan.heart-tree";
                default:
                    throw new ArgumentOutOfRangeException(nameof(role));
            }
        }
    }
}
