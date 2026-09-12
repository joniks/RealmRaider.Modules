using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace RealmRaiders.Modules.StarterRealmLayouts.Tests
{
    public sealed class StarterSylvanRealmLayoutTests
    {
        [Test]
        public void AllRecipes_AreValidConnectedAndDeclareExpansionSockets()
        {
            Assert.That(StarterSylvanRealmLayouts.All, Has.Count.EqualTo(3));
            CollectionAssert.AreEqual(
                new[]
                {
                    StarterSylvanRealmLayouts.AncientCrossroadsId,
                    StarterSylvanRealmLayouts.ForkedCanopyId,
                    StarterSylvanRealmLayouts.SerpentRootsId
                },
                LayoutIds(StarterSylvanRealmLayouts.All));
            Assert.That(
                new HashSet<string>(LayoutIds(StarterSylvanRealmLayouts.All), StringComparer.Ordinal).Count,
                Is.EqualTo(StarterSylvanRealmLayouts.All.Count));

            foreach (var recipe in StarterSylvanRealmLayouts.All)
            {
                var result = RealmLayoutRecipeValidator.ValidateStarterRecipe(recipe);

                Assert.That(result.IsValid, Is.True, recipe.LayoutId);
                Assert.That(recipe.Nodes, Has.Count.EqualTo(7), recipe.LayoutId);
                Assert.That(recipe.ExpansionSockets, Is.Not.Empty, recipe.LayoutId);
                Assert.That(recipe.Landmarks, Is.Not.Empty, recipe.LayoutId);
                Assert.That(HasKind(recipe.Nodes, RealmLayoutNodeKind.Start), Is.True);
                Assert.That(HasKind(recipe.Nodes, RealmLayoutNodeKind.Core), Is.True);
                Assert.That(HasUniqueNodeIds(recipe.Nodes), Is.True, recipe.LayoutId);
                Assert.That(HasSafeRouteToHeartTree(recipe), Is.True, recipe.LayoutId);

                foreach (var node in recipe.Nodes)
                {
                    Assert.That(
                        node.MaterializationRole,
                        Is.Not.EqualTo(SylvanRealmNodeMaterializationRole.Unknown));
                }

                Assert.That(
                    CountRole(recipe.Nodes, SylvanRealmNodeMaterializationRole.PortalStart),
                    Is.EqualTo(1));
                Assert.That(
                    CountRole(recipe.Nodes, SylvanRealmNodeMaterializationRole.LandmarkJunction),
                    Is.EqualTo(1));
                Assert.That(
                    CountRole(recipe.Nodes, SylvanRealmNodeMaterializationRole.WolfGroveEncounter),
                    Is.EqualTo(1));
                Assert.That(
                    CountRole(recipe.Nodes, SylvanRealmNodeMaterializationRole.RootPathHazard),
                    Is.EqualTo(1));
                Assert.That(
                    CountRole(recipe.Nodes, SylvanRealmNodeMaterializationRole.EntGroveEncounter),
                    Is.EqualTo(1));
                Assert.That(
                    CountRole(recipe.Nodes, SylvanRealmNodeMaterializationRole.MoonwellRecovery),
                    Is.EqualTo(1));
                Assert.That(
                    CountRole(recipe.Nodes, SylvanRealmNodeMaterializationRole.HeartTreeObjective),
                    Is.EqualTo(1));

                foreach (var edge in recipe.Edges)
                {
                    Assert.That(
                        edge.FloorPathWidth,
                        Is.InRange(
                            RealmLayoutRecipeValidator.MinimumFloorPathWidth,
                            RealmLayoutRecipeValidator.MaximumFloorPathWidth));
                }

                foreach (var landmark in recipe.Landmarks)
                {
                    Assert.That(
                        landmark.VisualRole,
                        Is.Not.EqualTo(SylvanLandmarkVisualRole.Unknown));
                }
            }
        }

        [Test]
        public void ValidateStarterRecipe_MissingRequiredRoleFailsClosedInStableOrder()
        {
            var nodes = CopyNodes(StarterSylvanRealmLayouts.AncientCrossroads.Nodes);
            nodes[3] = new RealmLayoutNode(
                nodes[3].NodeId,
                nodes[3].ContentId,
                nodes[3].Kind,
                SylvanRealmNodeMaterializationRole.Unknown,
                nodes[3].X,
                nodes[3].Z);
            var result = RealmLayoutRecipeValidator.ValidateStarterRecipe(
                CopyRecipeWithNodes(StarterSylvanRealmLayouts.AncientCrossroads, nodes));

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.NodeMaterializationRoleInvalid,
                    RealmLayoutValidationIssue.RootPathHazardRoleCardinalityInvalid
                },
                result.Issues);
        }

        [Test]
        public void ValidateStarterRecipe_DuplicateRequiredRoleFailsClosedInStableOrder()
        {
            var nodes = CopyNodes(StarterSylvanRealmLayouts.AncientCrossroads.Nodes);
            nodes[5] = new RealmLayoutNode(
                nodes[5].NodeId,
                nodes[5].ContentId,
                nodes[5].Kind,
                SylvanRealmNodeMaterializationRole.RootPathHazard,
                nodes[5].X,
                nodes[5].Z);
            var result = RealmLayoutRecipeValidator.ValidateStarterRecipe(
                CopyRecipeWithNodes(StarterSylvanRealmLayouts.AncientCrossroads, nodes));

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.RootPathHazardRoleCardinalityInvalid,
                    RealmLayoutValidationIssue.MoonwellRecoveryRoleCardinalityInvalid
                },
                result.Issues);
        }

        [Test]
        public void CachedRecipes_HavePairwiseDistinctTopologyAndSafeRouteSignatures()
        {
            var ancientSignature = RoleTopologySignature(StarterSylvanRealmLayouts.AncientCrossroads);
            var forkedSignature = RoleTopologySignature(StarterSylvanRealmLayouts.ForkedCanopy);
            var serpentSignature = RoleTopologySignature(StarterSylvanRealmLayouts.SerpentRoots);

            Assert.That(ancientSignature, Is.Not.EqualTo(forkedSignature));
            Assert.That(ancientSignature, Is.Not.EqualTo(serpentSignature));
            Assert.That(forkedSignature, Is.Not.EqualTo(serpentSignature));
        }

        [Test]
        public void Validate_NonAdjacentNodeAndCorridorClearanceFailClosed()
        {
            var nodeClearanceRecipe = ValidRecipe(
                new RealmLayoutNode[]
                {
                    new RealmLayoutNode(
                        "start",
                        "realmraiders.node.start",
                        RealmLayoutNodeKind.Start,
                        SylvanRealmNodeMaterializationRole.PortalStart,
                        0f,
                        0f),
                    new RealmLayoutNode(
                        "middle",
                        "realmraiders.node.middle",
                        RealmLayoutNodeKind.Encounter,
                        SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                        0f,
                        20f),
                    new RealmLayoutNode(
                        "core",
                        "realmraiders.node.core",
                        RealmLayoutNodeKind.Core,
                        SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                        6f,
                        10f)
                },
                new RealmLayoutEdge[]
                {
                    new RealmLayoutEdge("start-middle", "start", "middle", true, 6f),
                    new RealmLayoutEdge("middle-core", "middle", "core", true, 6f)
                });
            var corridorClearanceRecipe = ValidRecipe(
                new RealmLayoutNode[]
                {
                    new RealmLayoutNode(
                        "start",
                        "realmraiders.node.start",
                        RealmLayoutNodeKind.Start,
                        SylvanRealmNodeMaterializationRole.PortalStart,
                        0f,
                        0f),
                    new RealmLayoutNode(
                        "middle",
                        "realmraiders.node.middle",
                        RealmLayoutNodeKind.Encounter,
                        SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                        0f,
                        20f),
                    new RealmLayoutNode(
                        "core",
                        "realmraiders.node.core",
                        RealmLayoutNodeKind.Core,
                        SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                        20f,
                        20f),
                    new RealmLayoutNode(
                        "other",
                        "realmraiders.node.other",
                        RealmLayoutNodeKind.Landmark,
                        SylvanRealmNodeMaterializationRole.LandmarkJunction,
                        20f,
                        0f)
                },
                new RealmLayoutEdge[]
                {
                    new RealmLayoutEdge("start-core", "start", "core", true, 6f),
                    new RealmLayoutEdge("middle-other", "middle", "other", false, 6f)
                });

            var nodeClearance = RealmLayoutRecipeValidator.Validate(nodeClearanceRecipe);
            var corridorClearance = RealmLayoutRecipeValidator.Validate(corridorClearanceRecipe);

            CollectionAssert.Contains(
                nodeClearance.Issues,
                RealmLayoutValidationIssue.NodeCorridorClearanceInvalid);
            CollectionAssert.Contains(
                corridorClearance.Issues,
                RealmLayoutValidationIssue.CorridorClearanceInvalid);
        }

        [Test]
        public void ResolveExact_ReturnsOnlyTheCachedExactStarterRecipe()
        {
            var resolved = StarterSylvanRealmLayoutResolver.ResolveExact(
                StarterSylvanRealmLayouts.ForkedCanopyId);

            Assert.That(resolved.Status, Is.EqualTo(RealmLayoutResolveStatus.Resolved));
            Assert.That(resolved.Recipe, Is.SameAs(StarterSylvanRealmLayouts.ForkedCanopy));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("unknown-layout")]
        [TestCase(" realmraiders.sylvan-layout.ancient-crossroads")]
        [TestCase("realmraiders.sylvan-layout.ancient-crossroads ")]
        [TestCase("REALMRAIDERS.SYLVAN-LAYOUT.ANCIENT-CROSSROADS")]
        public void ResolveExact_RejectsNullUnknownAndNonExactIds(string layoutId)
        {
            var result = StarterSylvanRealmLayoutResolver.ResolveExact(layoutId);

            Assert.That(result.Status, Is.EqualTo(RealmLayoutResolveStatus.LayoutIdInvalid));
            Assert.That(result.Recipe, Is.Null);
        }

        [Test]
        public void Validate_MissingOrUnsupportedMaterializationRolesFailClosed()
        {
            var nodes = ValidNodes();
            nodes[1] = new RealmLayoutNode(
                "middle",
                "realmraiders.node.middle",
                RealmLayoutNodeKind.Encounter,
                SylvanRealmNodeMaterializationRole.Unknown,
                0f,
                8f);
            var missingNodeRole = RealmLayoutRecipeValidator.Validate(ValidRecipe(nodes));

            Assert.That(missingNodeRole.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.NodeMaterializationRoleInvalid
                },
                missingNodeRole.Issues);

            var unsupportedLandmarkRole = RealmLayoutRecipeValidator.Validate(
                new RealmLayoutRecipe(
                    "test.recipe",
                    "Test Recipe",
                    ValidNodes(),
                    ValidEdges(),
                    new RealmLayoutLandmark[]
                    {
                        new RealmLayoutLandmark(
                            "landmark",
                            "realmraiders.landmark.test",
                            "middle",
                            (SylvanLandmarkVisualRole)99)
                    },
                    ValidExpansionSockets()));

            Assert.That(unsupportedLandmarkRole.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.LandmarkVisualRoleInvalid
                },
                unsupportedLandmarkRole.Issues);
        }

        [Test]
        public void Validate_MissingOrNonPhysicalFloorPathWidthFailsClosed()
        {
            var recipe = ValidRecipe(
                ValidNodes(),
                new RealmLayoutEdge[]
                {
                    new RealmLayoutEdge("start-middle", "start", "middle", true, 0f),
                    new RealmLayoutEdge("middle-core", "middle", "core", true, float.NaN)
                });

            var result = RealmLayoutRecipeValidator.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.EdgeFloorPathWidthInvalid
                },
                result.Issues);
        }

        [Test]
        public void Select_SameSeedReturnsTheSameCachedRecipe()
        {
            var first = StarterSylvanRealmLayoutSelector.Select(40, null);
            var second = StarterSylvanRealmLayoutSelector.Select(40, null);

            Assert.That(first.Status, Is.EqualTo(RealmLayoutSelectionStatus.Selected));
            Assert.That(second.Status, Is.EqualTo(RealmLayoutSelectionStatus.Selected));
            Assert.That(second.Recipe, Is.SameAs(first.Recipe));
            Assert.That(first.Recipe, Is.SameAs(StarterSylvanRealmLayouts.ForkedCanopy));
        }

        [Test]
        public void Validate_OutOfBoundsCoordinatesFailClosed()
        {
            var recipe = ValidRecipe(
                new RealmLayoutNode[]
                {
                    new RealmLayoutNode(
                        "start",
                        "realmraiders.node.start",
                        RealmLayoutNodeKind.Start,
                        SylvanRealmNodeMaterializationRole.PortalStart,
                        0f,
                        0f),
                    new RealmLayoutNode(
                        "middle",
                        "realmraiders.node.middle",
                        RealmLayoutNodeKind.Encounter,
                        SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                        RealmLayoutRecipeValidator.MaximumAbsoluteCoordinate + 0.01f,
                        8f),
                    new RealmLayoutNode(
                        "core",
                        "realmraiders.node.core",
                        RealmLayoutNodeKind.Core,
                        SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                        0f,
                        16f)
                });

            var result = RealmLayoutRecipeValidator.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.Contains(result.Issues, RealmLayoutValidationIssue.NodeCoordinateInvalid);
        }

        [Test]
        public void Validate_ReverseEdgesAreDuplicates()
        {
            var recipe = ValidRecipe(
                ValidNodes(),
                new RealmLayoutEdge[]
                {
                    new RealmLayoutEdge("start-middle", "start", "middle", true, 6f),
                    new RealmLayoutEdge("middle-start", "middle", "start", true, 6f),
                    new RealmLayoutEdge("middle-core", "middle", "core", true, 6f)
                });

            var result = RealmLayoutRecipeValidator.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.EdgeDuplicate
                },
                result.Issues);
        }

        [Test]
        public void Validate_EdgeOrderAndDirectionDoNotChangeUndirectedConnectivity()
        {
            var recipe = ValidRecipe(
                ValidNodes(),
                new RealmLayoutEdge[]
                {
                    new RealmLayoutEdge("core-middle", "core", "middle", true, 6f),
                    new RealmLayoutEdge("middle-start", "middle", "start", true, 6f)
                });

            var result = RealmLayoutRecipeValidator.Validate(recipe);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void Validate_TooCloseNodesFailClosed()
        {
            var recipe = ValidRecipe(
                new RealmLayoutNode[]
                {
                    new RealmLayoutNode(
                        "start",
                        "realmraiders.node.start",
                        RealmLayoutNodeKind.Start,
                        SylvanRealmNodeMaterializationRole.PortalStart,
                        0f,
                        0f),
                    new RealmLayoutNode(
                        "middle",
                        "realmraiders.node.middle",
                        RealmLayoutNodeKind.Encounter,
                        SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                        0f,
                        RealmLayoutRecipeValidator.MinimumNodeSpacing - 0.01f),
                    new RealmLayoutNode(
                        "core",
                        "realmraiders.node.core",
                        RealmLayoutNodeKind.Core,
                        SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                        0f,
                        16f)
                });

            var result = RealmLayoutRecipeValidator.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.NodeSpacingInvalid
                },
                result.Issues);
        }

        [Test]
        public void Validate_EmptyDisplayNameAndNodeContentIdReportExactIssues()
        {
            var nodes = ValidNodes();
            nodes[0] = new RealmLayoutNode(
                "start",
                string.Empty,
                RealmLayoutNodeKind.Start,
                SylvanRealmNodeMaterializationRole.PortalStart,
                0f,
                0f);
            var recipe = new RealmLayoutRecipe(
                "test.recipe",
                string.Empty,
                nodes,
                new RealmLayoutEdge[]
                {
                    new RealmLayoutEdge("start-middle", "start", "middle", true, 6f),
                    new RealmLayoutEdge("middle-core", "middle", "core", true, 6f)
                },
                new RealmLayoutLandmark[]
                {
                    new RealmLayoutLandmark(
                        "landmark",
                        "realmraiders.landmark.test",
                        "middle",
                        SylvanLandmarkVisualRole.NodeCanopy)
                },
                new RealmLayoutExpansionSocket[]
                {
                    new RealmLayoutExpansionSocket("socket", "core", 4f, 18f)
                });

            var result = RealmLayoutRecipeValidator.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.DisplayNameInvalid,
                    RealmLayoutValidationIssue.NodeContentIdInvalid
                },
                result.Issues);
        }

        [Test]
        public void Select_SeedDistributionReachesEveryStarterRecipe()
        {
            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            for (var seed = 0; seed < StarterSylvanRealmLayouts.All.Count; seed++)
            {
                var result = StarterSylvanRealmLayoutSelector.Select(seed, null);

                Assert.That(result.HasRecipe, Is.True);
                layoutIds.Add(result.Recipe.LayoutId);
            }

            CollectionAssert.AreEquivalent(
                LayoutIds(StarterSylvanRealmLayouts.All),
                layoutIds);
        }

        [Test]
        public void Select_PreviousExactRecipeIsAvoidedWhenAlternativesExist()
        {
            for (var seed = 0; seed < StarterSylvanRealmLayouts.All.Count; seed++)
            {
                var initial = StarterSylvanRealmLayoutSelector.Select(seed, null);
                var avoided = StarterSylvanRealmLayoutSelector.Select(
                    seed,
                    initial.Recipe.LayoutId);

                Assert.That(avoided.HasRecipe, Is.True);
                Assert.That(avoided.Recipe.LayoutId, Is.Not.EqualTo(initial.Recipe.LayoutId));
            }
        }

        [Test]
        public void Select_UnknownOrCaseChangedPreviousIdFailsClosed()
        {
            var unknown = StarterSylvanRealmLayoutSelector.Select(0, "unknown-layout");
            var caseChanged = StarterSylvanRealmLayoutSelector.Select(
                0,
                StarterSylvanRealmLayouts.AncientCrossroadsId.ToUpperInvariant());

            Assert.That(
                unknown.Status,
                Is.EqualTo(RealmLayoutSelectionStatus.PreviousLayoutIdInvalid));
            Assert.That(unknown.Recipe, Is.Null);
            Assert.That(
                caseChanged.Status,
                Is.EqualTo(RealmLayoutSelectionStatus.PreviousLayoutIdInvalid));
            Assert.That(caseChanged.Recipe, Is.Null);
        }

        [TestCase(" padded")]
        [TestCase("padded ")]
        [TestCase("control\u0001id")]
        [TestCase("Uppercase")]
        public void Validate_StableIdsRejectPaddedControlAndUppercaseSymbols(
            string layoutId)
        {
            var result = RealmLayoutRecipeValidator.Validate(
                ValidRecipe(ValidNodes(), null, layoutId));

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.LayoutIdInvalid
                },
                result.Issues);
        }

        [Test]
        public void RecipeSnapshots_SourceChangesAndCollectionWritesCannotMutateFacts()
        {
            var nodes = new[]
            {
                new RealmLayoutNode(
                    "start",
                    "realmraiders.node.start",
                    RealmLayoutNodeKind.Start,
                    SylvanRealmNodeMaterializationRole.PortalStart,
                    0f,
                    0f),
                new RealmLayoutNode(
                    "middle",
                    "realmraiders.node.middle",
                    RealmLayoutNodeKind.Encounter,
                    SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                    0f,
                    8f),
                new RealmLayoutNode(
                    "core",
                    "realmraiders.node.core",
                    RealmLayoutNodeKind.Core,
                    SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                    0f,
                    16f)
            };
            var recipe = ValidRecipe(nodes);

            nodes[0] = null;

            Assert.That(recipe.Nodes[0], Is.Not.Null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutNode>)recipe.Nodes)[0] = null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutEdge>)recipe.Edges)[0] = null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutLandmark>)recipe.Landmarks)[0] = null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutExpansionSocket>)recipe.ExpansionSockets)[0] = null);
        }

        [Test]
        public void Validate_InvalidFactsFailClosedWithStableOrderedIssues()
        {
            var recipe = new RealmLayoutRecipe(
                "invalid",
                "Invalid",
                new RealmLayoutNode[]
                {
                    new RealmLayoutNode(
                        "start",
                        "realmraiders.node.start",
                        RealmLayoutNodeKind.Start,
                        SylvanRealmNodeMaterializationRole.PortalStart,
                        float.NaN,
                        0f),
                    new RealmLayoutNode(
                        "start",
                        "realmraiders.node.core",
                        RealmLayoutNodeKind.Core,
                        SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                        0f,
                        16f),
                    null
                },
                new RealmLayoutEdge[]
                {
                    new RealmLayoutEdge("edge", "start", "missing", false, 6f),
                    new RealmLayoutEdge("edge", "start", "start", false, 6f)
                },
                new RealmLayoutLandmark[]
                {
                    new RealmLayoutLandmark(
                        "landmark",
                        "",
                        "missing",
                        SylvanLandmarkVisualRole.NodeCanopy)
                },
                new RealmLayoutExpansionSocket[]
                {
                    new RealmLayoutExpansionSocket("socket", "missing", float.PositiveInfinity, 0f)
                });

            var result = RealmLayoutRecipeValidator.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    RealmLayoutValidationIssue.NodeCoordinateInvalid,
                    RealmLayoutValidationIssue.NodeIdDuplicate,
                    RealmLayoutValidationIssue.NodeMissing,
                    RealmLayoutValidationIssue.EdgeEndpointInvalid,
                    RealmLayoutValidationIssue.EdgeIdDuplicate,
                    RealmLayoutValidationIssue.EdgeSelfReferenceInvalid,
                    RealmLayoutValidationIssue.LandmarkTypeInvalid,
                    RealmLayoutValidationIssue.LandmarkNodeInvalid,
                    RealmLayoutValidationIssue.ExpansionSocketNodeInvalid,
                    RealmLayoutValidationIssue.ExpansionSocketCoordinateInvalid
                },
                result.Issues);
        }

        [Test]
        public void Validate_UnsafeOrDisconnectedRouteFailsClosed()
        {
            var recipe = ValidRecipe(
                new RealmLayoutNode[]
                {
                    new RealmLayoutNode(
                        "start",
                        "realmraiders.node.start",
                        RealmLayoutNodeKind.Start,
                        SylvanRealmNodeMaterializationRole.PortalStart,
                        0f,
                        0f),
                    new RealmLayoutNode(
                        "middle",
                        "realmraiders.node.middle",
                        RealmLayoutNodeKind.Encounter,
                        SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                        0f,
                        8f),
                    new RealmLayoutNode(
                        "core",
                        "realmraiders.node.core",
                        RealmLayoutNodeKind.Core,
                        SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                        0f,
                        16f)
                },
                new RealmLayoutEdge[]
                {
                    new RealmLayoutEdge("start-middle", "start", "middle", false, 6f)
                });

            var result = RealmLayoutRecipeValidator.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            CollectionAssert.Contains(result.Issues, RealmLayoutValidationIssue.LayoutDisconnected);
            CollectionAssert.Contains(result.Issues, RealmLayoutValidationIssue.ActivePathUnsafe);
        }

        private static string[] LayoutIds(IReadOnlyList<RealmLayoutRecipe> recipes)
        {
            var layoutIds = new string[recipes.Count];
            for (var index = 0; index < recipes.Count; index++)
            {
                layoutIds[index] = recipes[index].LayoutId;
            }

            return layoutIds;
        }

        private static RealmLayoutNode[] CopyNodes(IReadOnlyList<RealmLayoutNode> nodes)
        {
            var copy = new RealmLayoutNode[nodes.Count];
            for (var index = 0; index < nodes.Count; index++)
            {
                copy[index] = nodes[index];
            }

            return copy;
        }

        private static RealmLayoutRecipe CopyRecipeWithNodes(
            RealmLayoutRecipe source,
            IReadOnlyList<RealmLayoutNode> nodes)
        {
            return new RealmLayoutRecipe(
                source.LayoutId,
                source.DisplayName,
                nodes,
                source.Edges,
                source.Landmarks,
                source.ExpansionSockets);
        }

        private static int CountRole(
            IReadOnlyList<RealmLayoutNode> nodes,
            SylvanRealmNodeMaterializationRole role)
        {
            var count = 0;
            foreach (var node in nodes)
            {
                if (node != null && node.MaterializationRole == role)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasUniqueNodeIds(IReadOnlyList<RealmLayoutNode> nodes)
        {
            var nodeIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var node in nodes)
            {
                if (node == null || !nodeIds.Add(node.NodeId))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasSafeRouteToHeartTree(RealmLayoutRecipe recipe)
        {
            var startNodeId = string.Empty;
            var heartTreeNodeId = string.Empty;
            foreach (var node in recipe.Nodes)
            {
                if (node.MaterializationRole == SylvanRealmNodeMaterializationRole.PortalStart)
                {
                    startNodeId = node.NodeId;
                }

                if (node.MaterializationRole == SylvanRealmNodeMaterializationRole.HeartTreeObjective)
                {
                    heartTreeNodeId = node.NodeId;
                }
            }

            var visited = new HashSet<string>(StringComparer.Ordinal)
            {
                startNodeId
            };
            var pending = new Queue<string>();
            pending.Enqueue(startNodeId);
            while (pending.Count > 0)
            {
                var currentNodeId = pending.Dequeue();
                foreach (var edge in recipe.Edges)
                {
                    if (!edge.IsActivePathSafe)
                    {
                        continue;
                    }

                    var adjacentNodeId = string.Equals(
                        edge.FromNodeId,
                        currentNodeId,
                        StringComparison.Ordinal)
                        ? edge.ToNodeId
                        : string.Equals(edge.ToNodeId, currentNodeId, StringComparison.Ordinal)
                            ? edge.FromNodeId
                            : null;
                    if (adjacentNodeId != null && visited.Add(adjacentNodeId))
                    {
                        pending.Enqueue(adjacentNodeId);
                    }
                }
            }

            return visited.Contains(heartTreeNodeId);
        }

        private static string RoleTopologySignature(RealmLayoutRecipe recipe)
        {
            var rolesByNodeId = new Dictionary<string, SylvanRealmNodeMaterializationRole>(
                StringComparer.Ordinal);
            foreach (var node in recipe.Nodes)
            {
                rolesByNodeId.Add(node.NodeId, node.MaterializationRole);
            }

            var edgeSignatures = new List<string>();
            foreach (var edge in recipe.Edges)
            {
                var firstRole = rolesByNodeId[edge.FromNodeId].ToString();
                var secondRole = rolesByNodeId[edge.ToNodeId].ToString();
                if (string.CompareOrdinal(firstRole, secondRole) > 0)
                {
                    var temporary = firstRole;
                    firstRole = secondRole;
                    secondRole = temporary;
                }

                edgeSignatures.Add(
                    firstRole
                    + "-"
                    + secondRole
                    + ":"
                    + (edge.IsActivePathSafe ? "safe" : "unsafe"));
            }

            edgeSignatures.Sort(StringComparer.Ordinal);
            return string.Join("|", edgeSignatures);
        }

        private static bool HasKind(
            IReadOnlyList<RealmLayoutNode> nodes,
            RealmLayoutNodeKind kind)
        {
            foreach (var node in nodes)
            {
                if (node != null && node.Kind == kind)
                {
                    return true;
                }
            }

            return false;
        }

        private static RealmLayoutRecipe ValidRecipe(
            IReadOnlyList<RealmLayoutNode> nodes,
            IReadOnlyList<RealmLayoutEdge> edges = null,
            string layoutId = "test.recipe")
        {
            return new RealmLayoutRecipe(
                layoutId,
                "Test Recipe",
                nodes,
                edges ?? ValidEdges(),
                new RealmLayoutLandmark[]
                {
                    new RealmLayoutLandmark(
                        "landmark",
                        "realmraiders.landmark.test",
                        "middle",
                        SylvanLandmarkVisualRole.NodeCanopy)
                },
                new RealmLayoutExpansionSocket[]
                {
                    new RealmLayoutExpansionSocket("socket", "core", 4f, 18f)
                });
        }

        private static RealmLayoutEdge[] ValidEdges()
        {
            return new RealmLayoutEdge[]
            {
                new RealmLayoutEdge("start-middle", "start", "middle", true, 6f),
                new RealmLayoutEdge("middle-core", "middle", "core", true, 6f)
            };
        }

        private static RealmLayoutExpansionSocket[] ValidExpansionSockets()
        {
            return new RealmLayoutExpansionSocket[]
            {
                new RealmLayoutExpansionSocket("socket", "core", 4f, 18f)
            };
        }

        private static RealmLayoutNode[] ValidNodes()
        {
            return new RealmLayoutNode[]
            {
                new RealmLayoutNode(
                    "start",
                    "realmraiders.node.start",
                    RealmLayoutNodeKind.Start,
                    SylvanRealmNodeMaterializationRole.PortalStart,
                    0f,
                    0f),
                new RealmLayoutNode(
                    "middle",
                    "realmraiders.node.middle",
                    RealmLayoutNodeKind.Encounter,
                    SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                    0f,
                    8f),
                new RealmLayoutNode(
                    "core",
                    "realmraiders.node.core",
                    RealmLayoutNodeKind.Core,
                    SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                    0f,
                    16f)
            };
        }
    }
}
