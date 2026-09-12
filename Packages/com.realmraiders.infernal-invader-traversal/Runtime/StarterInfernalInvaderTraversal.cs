using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.InfernalDefensePacing;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.InfernalInvaderTraversal
{
    public enum InfernalTraversalClass
    {
        Safe,
        DeclaredRisk
    }

    public enum InfernalInvaderTraversalValidationIssue
    {
        RecipeMissing,
        LayoutIdInvalid,
        LayoutNotFound,
        LayoutInvalid,
        PacingLayoutIdInvalid,
        PacingLayoutMismatch,
        PacingNotFound,
        PacingInvalid,
        NodeCollectionMissing,
        EdgeStepCollectionMissing,
        StepCardinalityInvalid,
        StartOrEndInvalid,
        NodeMissing,
        NodeIdInvalid,
        NodeInvalid,
        EdgeStepMissing,
        EdgeIdInvalid,
        EdgeInvalid,
        EdgeDisconnected,
        EdgeTraversalClassInvalid,
        IllegalNodeRetrace,
        IllegalEdgeRetrace,
        RequiredRoleMissing,
        RequiredRoleOrderInvalid,
        FlameRoleMissing,
        BruteRoleMissing,
        HeartRoleMissing,
        FlameBruteHeartOrderInvalid,
        LavaGateMissing,
        LavaGateEdgeIdInvalid,
        LavaGateEdgeInvalid,
        LavaGateNotInRoute,
        LavaGateNotIncomingBrute,
        LavaGateFractionInvalid
    }

    public enum InfernalInvaderTraversalCatalogueValidationIssue
    {
        CatalogueMissing,
        RecipeCardinalityInvalid,
        RecipeMissing,
        LayoutIdDuplicate,
        RecipeInvalid,
        LayoutCoverageInvalid,
        SemanticSignatureDuplicate
    }

    public enum InfernalInvaderTraversalLookupStatus
    {
        Found,
        LayoutIdInvalid,
        NotFound,
        CatalogueInvalid
    }

    public sealed class InfernalTraversalEdgeStep
    {
        public InfernalTraversalEdgeStep(
            string edgeId,
            InfernalTraversalClass traversalClass)
        {
            EdgeId = edgeId;
            TraversalClass = traversalClass;
        }

        public string EdgeId { get; }

        public InfernalTraversalClass TraversalClass { get; }
    }

    public sealed class InfernalLavaGatePlacement
    {
        public InfernalLavaGatePlacement(string edgeId, float normalizedFraction)
        {
            EdgeId = edgeId;
            NormalizedFraction = normalizedFraction;
        }

        public string EdgeId { get; }

        public float NormalizedFraction { get; }
    }

    /// <summary>
    /// One exact-layout, immutable, authored invader route. Core remains the owner
    /// of materialization, movement, combat, hazards, selection and persistence.
    /// </summary>
    public sealed class InfernalInvaderTraversalRecipe
    {
        private readonly bool nodeCollectionWasMissing;
        private readonly bool edgeStepCollectionWasMissing;

        public InfernalInvaderTraversalRecipe(
            string layoutId,
            string pacingLayoutId,
            IReadOnlyList<string> nodeIds,
            IReadOnlyList<InfernalTraversalEdgeStep> edgeSteps,
            InfernalLavaGatePlacement lavaGate)
        {
            LayoutId = layoutId;
            PacingLayoutId = pacingLayoutId;
            nodeCollectionWasMissing = nodeIds == null;
            edgeStepCollectionWasMissing = edgeSteps == null;
            NodeIds = Snapshot(nodeIds);
            EdgeSteps = Snapshot(edgeSteps);
            LavaGate = lavaGate;
        }

        public string LayoutId { get; }

        public string PacingLayoutId { get; }

        public IReadOnlyList<string> NodeIds { get; }

        public IReadOnlyList<InfernalTraversalEdgeStep> EdgeSteps { get; }

        public InfernalLavaGatePlacement LavaGate { get; }

        internal bool NodeCollectionWasMissing => nodeCollectionWasMissing;

        internal bool EdgeStepCollectionWasMissing => edgeStepCollectionWasMissing;

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<T>());
            }

            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalInvaderTraversalValidationResult
    {
        internal InfernalInvaderTraversalValidationResult(
            IReadOnlyList<InfernalInvaderTraversalValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalInvaderTraversalValidationIssue> Issues { get; }

        public bool IsValid => Issues.Count == 0;

        private static IReadOnlyList<InfernalInvaderTraversalValidationIssue> Snapshot(
            IReadOnlyList<InfernalInvaderTraversalValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(
                    Array.Empty<InfernalInvaderTraversalValidationIssue>());
            }

            var copy = new InfernalInvaderTraversalValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalInvaderTraversalCatalogueValidationResult
    {
        internal InfernalInvaderTraversalCatalogueValidationResult(
            IReadOnlyList<InfernalInvaderTraversalCatalogueValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalInvaderTraversalCatalogueValidationIssue> Issues { get; }

        public bool IsValid => Issues.Count == 0;

        private static IReadOnlyList<InfernalInvaderTraversalCatalogueValidationIssue> Snapshot(
            IReadOnlyList<InfernalInvaderTraversalCatalogueValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(
                    Array.Empty<InfernalInvaderTraversalCatalogueValidationIssue>());
            }

            var copy = new InfernalInvaderTraversalCatalogueValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalInvaderTraversalLookupResult
    {
        internal InfernalInvaderTraversalLookupResult(
            InfernalInvaderTraversalLookupStatus status,
            InfernalInvaderTraversalRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public InfernalInvaderTraversalLookupStatus Status { get; }

        public InfernalInvaderTraversalRecipe Recipe { get; }

        public bool Found => Status == InfernalInvaderTraversalLookupStatus.Found
            && Recipe != null;
    }

    /// <summary>
    /// Fail-closed evidence against the exact cached layout and pacing catalogues.
    /// It never pathfinds or changes runtime behavior.
    /// </summary>
    public static class InfernalInvaderTraversalValidator
    {
        public static InfernalInvaderTraversalValidationResult Validate(
            InfernalInvaderTraversalRecipe recipe)
        {
            var issues = new List<InfernalInvaderTraversalValidationIssue>();
            if (recipe == null)
            {
                AddIssue(issues, InfernalInvaderTraversalValidationIssue.RecipeMissing);
                return new InfernalInvaderTraversalValidationResult(issues);
            }

            var layout = ResolveLayout(recipe.LayoutId, issues);
            var pacing = ResolvePacing(recipe.PacingLayoutId, issues);
            if (HasStableId(recipe.LayoutId)
                && HasStableId(recipe.PacingLayoutId)
                && !string.Equals(
                    recipe.LayoutId,
                    recipe.PacingLayoutId,
                    StringComparison.Ordinal))
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.PacingLayoutMismatch);
            }

            if (recipe.NodeCollectionWasMissing)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.NodeCollectionMissing);
            }

            if (recipe.EdgeStepCollectionWasMissing)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.EdgeStepCollectionMissing);
            }

            if (recipe.NodeIds.Count < 2
                || recipe.NodeIds.Count != recipe.EdgeSteps.Count + 1)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.StepCardinalityInvalid);
            }

            var nodesById = CreateNodesById(layout);
            var edgesById = CreateEdgesById(layout);
            ValidateStartAndEnd(recipe, layout, issues);
            ValidateNodes(recipe, nodesById, issues);
            ValidateEdges(recipe, edgesById, issues);
            ValidateRetraces(recipe, nodesById, layout, issues);
            ValidateRoleOrder(recipe, nodesById, pacing, issues);
            ValidateLavaGate(recipe, layout, edgesById, issues);

            return new InfernalInvaderTraversalValidationResult(issues);
        }

        public static InfernalInvaderTraversalCatalogueValidationResult ValidateCatalogue(
            IReadOnlyList<InfernalInvaderTraversalRecipe> recipes)
        {
            var issues = new List<InfernalInvaderTraversalCatalogueValidationIssue>();
            if (recipes == null)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalCatalogueValidationIssue.CatalogueMissing);
                return new InfernalInvaderTraversalCatalogueValidationResult(issues);
            }

            if (recipes.Count != StarterInfernalDefenseLayouts.All.Count)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalCatalogueValidationIssue.RecipeCardinalityInvalid);
            }

            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            var signatures = new HashSet<string>(StringComparer.Ordinal);
            foreach (var recipe in recipes)
            {
                if (recipe == null)
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalCatalogueValidationIssue.RecipeMissing);
                    continue;
                }

                if (!layoutIds.Add(recipe.LayoutId))
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalCatalogueValidationIssue.LayoutIdDuplicate);
                }

                if (!Validate(recipe).IsValid)
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalCatalogueValidationIssue.RecipeInvalid);
                    continue;
                }

                var signature = CreateSemanticSignature(recipe);
                if (signature != null && !signatures.Add(signature))
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalCatalogueValidationIssue.SemanticSignatureDuplicate);
                }
            }

            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                if (!layoutIds.Contains(layout.LayoutId))
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalCatalogueValidationIssue.LayoutCoverageInvalid);
                }
            }

            return new InfernalInvaderTraversalCatalogueValidationResult(issues);
        }

        public static string CreateSemanticSignature(
            InfernalInvaderTraversalRecipe recipe)
        {
            if (recipe == null || !Validate(recipe).IsValid)
            {
                return null;
            }

            var layoutResult = StarterInfernalDefenseLayoutResolver.ResolveExact(
                recipe.LayoutId);
            if (!layoutResult.HasLayout)
            {
                return null;
            }

            var rolesByNode = CreateRolesByNode(layoutResult.Layout);
            var parts = new List<string>();
            for (var index = 0; index < recipe.NodeIds.Count; index++)
            {
                parts.Add(rolesByNode[recipe.NodeIds[index]]);
                if (index < recipe.EdgeSteps.Count)
                {
                    parts.Add(((int)recipe.EdgeSteps[index].TraversalClass).ToString());
                }
            }

            var lavaIndex = IndexOfEdge(recipe.EdgeSteps, recipe.LavaGate.EdgeId);
            parts.Add("lava@" + lavaIndex.ToString());
            parts.Add(recipe.LavaGate.NormalizedFraction.ToString(
                "R",
                System.Globalization.CultureInfo.InvariantCulture));
            return string.Join("|", parts.ToArray());
        }

        private static RealmLayoutGraph ResolveLayout(
            string layoutId,
            ICollection<InfernalInvaderTraversalValidationIssue> issues)
        {
            if (!HasStableId(layoutId))
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.LayoutIdInvalid);
                return null;
            }

            var resolved = StarterInfernalDefenseLayoutResolver.ResolveExact(layoutId);
            if (!resolved.HasLayout)
            {
                AddIssue(issues,
                    resolved.Status == InfernalDefenseLayoutResolveStatus.CatalogueInvalid
                        ? InfernalInvaderTraversalValidationIssue.LayoutInvalid
                        : InfernalInvaderTraversalValidationIssue.LayoutNotFound);
                return null;
            }

            if (!InfernalDefenseLayoutValidator.Validate(resolved.Layout).IsValid)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.LayoutInvalid);
                return null;
            }

            return resolved.Layout;
        }

        private static InfernalDefensePacingRecipe ResolvePacing(
            string pacingLayoutId,
            ICollection<InfernalInvaderTraversalValidationIssue> issues)
        {
            if (!HasStableId(pacingLayoutId))
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.PacingLayoutIdInvalid);
                return null;
            }

            var resolved = StarterInfernalDefensePacingResolver.ResolveExact(pacingLayoutId);
            if (!resolved.Found)
            {
                AddIssue(issues,
                    resolved.Status == InfernalDefensePacingLookupStatus.CatalogueInvalid
                        ? InfernalInvaderTraversalValidationIssue.PacingInvalid
                        : InfernalInvaderTraversalValidationIssue.PacingNotFound);
                return null;
            }

            if (!InfernalDefensePacingValidator.Validate(resolved.Recipe).IsValid)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.PacingInvalid);
                return null;
            }

            return resolved.Recipe;
        }

        private static void ValidateStartAndEnd(
            InfernalInvaderTraversalRecipe recipe,
            RealmLayoutGraph layout,
            ICollection<InfernalInvaderTraversalValidationIssue> issues)
        {
            if (layout == null || recipe.NodeIds.Count == 0)
            {
                return;
            }

            var entryNodeId = NodeForRole(
                layout,
                StarterInfernalDefenseLayouts.InvaderEntryRoleId);
            var heartNodeId = NodeForRole(
                layout,
                StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId);
            if (!string.Equals(recipe.NodeIds[0], entryNodeId, StringComparison.Ordinal)
                || !string.Equals(
                    recipe.NodeIds[recipe.NodeIds.Count - 1],
                    heartNodeId,
                    StringComparison.Ordinal))
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.StartOrEndInvalid);
            }
        }

        private static void ValidateNodes(
            InfernalInvaderTraversalRecipe recipe,
            IReadOnlyDictionary<string, RealmLayoutGraphNode> nodesById,
            ICollection<InfernalInvaderTraversalValidationIssue> issues)
        {
            foreach (var nodeId in recipe.NodeIds)
            {
                if (nodeId == null)
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.NodeMissing);
                }
                else if (!HasStableId(nodeId))
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.NodeIdInvalid);
                }
                else if (!nodesById.ContainsKey(nodeId))
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.NodeInvalid);
                }
            }
        }

        private static void ValidateEdges(
            InfernalInvaderTraversalRecipe recipe,
            IReadOnlyDictionary<string, RealmLayoutGraphEdge> edgesById,
            ICollection<InfernalInvaderTraversalValidationIssue> issues)
        {
            for (var index = 0; index < recipe.EdgeSteps.Count; index++)
            {
                var step = recipe.EdgeSteps[index];
                if (step == null)
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.EdgeStepMissing);
                    continue;
                }

                if (!HasStableId(step.EdgeId))
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.EdgeIdInvalid);
                    continue;
                }

                if (step.TraversalClass != InfernalTraversalClass.Safe
                    && step.TraversalClass != InfernalTraversalClass.DeclaredRisk)
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.EdgeTraversalClassInvalid);
                }

                if (!edgesById.TryGetValue(step.EdgeId, out var edge))
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.EdgeInvalid);
                    continue;
                }

                if (index + 1 >= recipe.NodeIds.Count
                    || !Connects(
                        edge,
                        recipe.NodeIds[index],
                        recipe.NodeIds[index + 1]))
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.EdgeDisconnected);
                }

                var expectedClass = edge.IsActivePathSafe
                    ? InfernalTraversalClass.Safe
                    : InfernalTraversalClass.DeclaredRisk;
                if (step.TraversalClass != expectedClass)
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.EdgeTraversalClassInvalid);
                }
            }
        }

        private static void ValidateRetraces(
            InfernalInvaderTraversalRecipe recipe,
            IReadOnlyDictionary<string, RealmLayoutGraphNode> nodesById,
            RealmLayoutGraph layout,
            ICollection<InfernalInvaderTraversalValidationIssue> issues)
        {
            var nodeIndexes = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            for (var index = 0; index < recipe.NodeIds.Count; index++)
            {
                var nodeId = recipe.NodeIds[index];
                if (nodeId == null || !nodesById.ContainsKey(nodeId))
                {
                    continue;
                }

                if (!nodeIndexes.TryGetValue(nodeId, out var indexes))
                {
                    indexes = new List<int>();
                    nodeIndexes.Add(nodeId, indexes);
                }

                indexes.Add(index);
            }

            var junctionId = NodeForRole(
                layout,
                StarterInfernalDefenseLayouts.RouteJunctionRoleId);
            var allowedOutAndBackStart = -1;
            foreach (var pair in nodeIndexes)
            {
                if (pair.Value.Count == 1)
                {
                    continue;
                }

                if (string.Equals(pair.Key, junctionId, StringComparison.Ordinal)
                    && pair.Value.Count == 2
                    && pair.Value[1] == pair.Value[0] + 2
                    && IsExactHellhoundOutAndBack(recipe, nodesById, pair.Value[0]))
                {
                    allowedOutAndBackStart = pair.Value[0];
                    continue;
                }

                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.IllegalNodeRetrace);
            }

            var edgeIndexes = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            for (var index = 0; index < recipe.EdgeSteps.Count; index++)
            {
                var step = recipe.EdgeSteps[index];
                if (step == null || !HasStableId(step.EdgeId))
                {
                    continue;
                }

                if (!edgeIndexes.TryGetValue(step.EdgeId, out var indexes))
                {
                    indexes = new List<int>();
                    edgeIndexes.Add(step.EdgeId, indexes);
                }

                indexes.Add(index);
            }

            foreach (var pair in edgeIndexes)
            {
                if (pair.Value.Count == 1)
                {
                    continue;
                }

                if (pair.Value.Count == 2
                    && pair.Value[0] == allowedOutAndBackStart
                    && pair.Value[1] == allowedOutAndBackStart + 1)
                {
                    continue;
                }

                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.IllegalEdgeRetrace);
            }
        }

        private static bool IsExactHellhoundOutAndBack(
            InfernalInvaderTraversalRecipe recipe,
            IReadOnlyDictionary<string, RealmLayoutGraphNode> nodesById,
            int junctionIndex)
        {
            if (junctionIndex < 0
                || junctionIndex + 2 >= recipe.NodeIds.Count
                || junctionIndex + 1 >= recipe.EdgeSteps.Count)
            {
                return false;
            }

            var first = recipe.EdgeSteps[junctionIndex];
            var second = recipe.EdgeSteps[junctionIndex + 1];
            if (first == null
                || second == null
                || !string.Equals(first.EdgeId, second.EdgeId, StringComparison.Ordinal)
                || !nodesById.TryGetValue(
                    recipe.NodeIds[junctionIndex + 1],
                    out var excursionNode))
            {
                return false;
            }

            return string.Equals(
                excursionNode.GameplayRoleId,
                StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                StringComparison.Ordinal);
        }

        private static void ValidateRoleOrder(
            InfernalInvaderTraversalRecipe recipe,
            IReadOnlyDictionary<string, RealmLayoutGraphNode> nodesById,
            InfernalDefensePacingRecipe pacing,
            ICollection<InfernalInvaderTraversalValidationIssue> issues)
        {
            var roles = new List<string>();
            foreach (var nodeId in recipe.NodeIds)
            {
                if (nodeId != null && nodesById.TryGetValue(nodeId, out var node))
                {
                    roles.Add(node.GameplayRoleId);
                }
                else
                {
                    roles.Add(null);
                }
            }

            if (pacing != null)
            {
                var cursor = 0;
                foreach (var beat in pacing.Beats)
                {
                    if (beat == null
                        || beat.Requirement != InfernalDefensePacingRequirement.Required)
                    {
                        continue;
                    }

                    var found = IndexOf(roles, beat.RoleId, cursor);
                    if (found < 0)
                    {
                        AddIssue(issues,
                            InfernalInvaderTraversalValidationIssue.RequiredRoleMissing);
                        continue;
                    }

                    cursor = found + 1;
                }

                if (!RequiredRolesAppearInOrder(roles, pacing))
                {
                    AddIssue(issues,
                        InfernalInvaderTraversalValidationIssue.RequiredRoleOrderInvalid);
                }
            }

            var flame = IndexOf(
                roles,
                StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                0);
            var brute = IndexOf(
                roles,
                StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                0);
            var heart = IndexOf(
                roles,
                StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                0);
            if (flame < 0)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.FlameRoleMissing);
            }

            if (brute < 0)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.BruteRoleMissing);
            }

            if (heart < 0)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.HeartRoleMissing);
            }

            if (flame < 0 || brute < 0 || heart < 0 || flame >= brute || brute >= heart)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.FlameBruteHeartOrderInvalid);
            }
        }

        private static bool RequiredRolesAppearInOrder(
            IReadOnlyList<string> roles,
            InfernalDefensePacingRecipe pacing)
        {
            var cursor = 0;
            foreach (var beat in pacing.Beats)
            {
                if (beat == null
                    || beat.Requirement != InfernalDefensePacingRequirement.Required)
                {
                    continue;
                }

                var found = IndexOf(roles, beat.RoleId, cursor);
                if (found < 0)
                {
                    return false;
                }

                cursor = found + 1;
            }

            return true;
        }

        private static void ValidateLavaGate(
            InfernalInvaderTraversalRecipe recipe,
            RealmLayoutGraph layout,
            IReadOnlyDictionary<string, RealmLayoutGraphEdge> edgesById,
            ICollection<InfernalInvaderTraversalValidationIssue> issues)
        {
            if (recipe.LavaGate == null)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.LavaGateMissing);
                return;
            }

            if (float.IsNaN(recipe.LavaGate.NormalizedFraction)
                || float.IsInfinity(recipe.LavaGate.NormalizedFraction)
                || recipe.LavaGate.NormalizedFraction != .5f)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.LavaGateFractionInvalid);
            }

            if (!HasStableId(recipe.LavaGate.EdgeId))
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.LavaGateEdgeIdInvalid);
                return;
            }

            if (!edgesById.ContainsKey(recipe.LavaGate.EdgeId))
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.LavaGateEdgeInvalid);
                return;
            }

            var inRoute = false;
            var entersBrute = false;
            var bruteNodeId = NodeForRole(
                layout,
                StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId);
            for (var index = 0; index < recipe.EdgeSteps.Count; index++)
            {
                var step = recipe.EdgeSteps[index];
                if (step == null
                    || !string.Equals(
                        step.EdgeId,
                        recipe.LavaGate.EdgeId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                inRoute = true;
                if (index + 1 < recipe.NodeIds.Count
                    && string.Equals(
                        recipe.NodeIds[index + 1],
                        bruteNodeId,
                        StringComparison.Ordinal))
                {
                    entersBrute = true;
                }
            }

            if (!inRoute)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.LavaGateNotInRoute);
            }

            if (!entersBrute)
            {
                AddIssue(issues,
                    InfernalInvaderTraversalValidationIssue.LavaGateNotIncomingBrute);
            }
        }

        private static IReadOnlyDictionary<string, RealmLayoutGraphNode> CreateNodesById(
            RealmLayoutGraph layout)
        {
            var nodes = new Dictionary<string, RealmLayoutGraphNode>(StringComparer.Ordinal);
            if (layout != null)
            {
                foreach (var node in layout.Nodes)
                {
                    if (node != null && !nodes.ContainsKey(node.NodeId))
                    {
                        nodes.Add(node.NodeId, node);
                    }
                }
            }

            return nodes;
        }

        private static IReadOnlyDictionary<string, RealmLayoutGraphEdge> CreateEdgesById(
            RealmLayoutGraph layout)
        {
            var edges = new Dictionary<string, RealmLayoutGraphEdge>(StringComparer.Ordinal);
            if (layout != null)
            {
                foreach (var edge in layout.Edges)
                {
                    if (edge != null && !edges.ContainsKey(edge.EdgeId))
                    {
                        edges.Add(edge.EdgeId, edge);
                    }
                }
            }

            return edges;
        }

        private static IReadOnlyDictionary<string, string> CreateRolesByNode(
            RealmLayoutGraph layout)
        {
            var roles = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var node in layout.Nodes)
            {
                roles.Add(node.NodeId, node.GameplayRoleId);
            }

            return roles;
        }

        private static string NodeForRole(RealmLayoutGraph layout, string roleId)
        {
            if (layout != null)
            {
                foreach (var node in layout.Nodes)
                {
                    if (node != null
                        && string.Equals(
                            node.GameplayRoleId,
                            roleId,
                            StringComparison.Ordinal))
                    {
                        return node.NodeId;
                    }
                }
            }

            return null;
        }

        private static bool Connects(
            RealmLayoutGraphEdge edge,
            string firstNodeId,
            string secondNodeId)
        {
            return string.Equals(edge.FromNodeId, firstNodeId, StringComparison.Ordinal)
                    && string.Equals(edge.ToNodeId, secondNodeId, StringComparison.Ordinal)
                || string.Equals(edge.FromNodeId, secondNodeId, StringComparison.Ordinal)
                    && string.Equals(edge.ToNodeId, firstNodeId, StringComparison.Ordinal);
        }

        private static int IndexOf(
            IReadOnlyList<string> values,
            string value,
            int startIndex)
        {
            for (var index = Math.Max(0, startIndex); index < values.Count; index++)
            {
                if (string.Equals(values[index], value, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static int IndexOfEdge(
            IReadOnlyList<InfernalTraversalEdgeStep> steps,
            string edgeId)
        {
            for (var index = 0; index < steps.Count; index++)
            {
                if (steps[index] != null
                    && string.Equals(steps[index].EdgeId, edgeId, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || !IsAsciiAlphaNumeric(value[0])
                || !IsAsciiAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            for (var index = 1; index < value.Length - 1; index++)
            {
                var character = value[index];
                if (!IsAsciiAlphaNumeric(character)
                    && character != '.'
                    && character != '-'
                    && character != '_')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiAlphaNumeric(char value)
        {
            return value >= 'a' && value <= 'z'
                || value >= 'A' && value <= 'Z'
                || value >= '0' && value <= '9';
        }

        private static void AddIssue(
            ICollection<InfernalInvaderTraversalValidationIssue> issues,
            InfernalInvaderTraversalValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private static void AddIssue(
            ICollection<InfernalInvaderTraversalCatalogueValidationIssue> issues,
            InfernalInvaderTraversalCatalogueValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }

    /// <summary>
    /// Three cached routes. Optional Flame pacing means activation is optional;
    /// every route still visits Flame so Flame Rush remains available.
    /// </summary>
    public static class StarterInfernalInvaderTraversals
    {
        public static InfernalInvaderTraversalRecipe AshenSpur { get; } =
            Route(
                StarterInfernalDefenseLayouts.AshenSpurId,
                new[] { "ashen.entry", "ashen.junction", "ashen.flame", "ashen.brute", "ashen.heart" },
                new[]
                {
                    Step("ashen.entry-junction", InfernalTraversalClass.Safe),
                    Step("ashen.junction-flame", InfernalTraversalClass.Safe),
                    Step("ashen.flame-brute", InfernalTraversalClass.Safe),
                    Step("ashen.brute-heart", InfernalTraversalClass.Safe)
                },
                "ashen.flame-brute");

        public static InfernalInvaderTraversalRecipe CinderFork { get; } =
            Route(
                StarterInfernalDefenseLayouts.CinderForkId,
                new[] { "cinder.entry", "cinder.junction", "cinder.hellhound", "cinder.junction", "cinder.flame", "cinder.brute", "cinder.heart" },
                new[]
                {
                    Step("cinder.entry-junction", InfernalTraversalClass.Safe),
                    Step("cinder.junction-hellhound", InfernalTraversalClass.Safe),
                    Step("cinder.junction-hellhound", InfernalTraversalClass.Safe),
                    Step("cinder.junction-flame", InfernalTraversalClass.DeclaredRisk),
                    Step("cinder.flame-brute", InfernalTraversalClass.DeclaredRisk),
                    Step("cinder.brute-heart", InfernalTraversalClass.Safe)
                },
                "cinder.flame-brute");

        public static InfernalInvaderTraversalRecipe EmberCircuit { get; } =
            Route(
                StarterInfernalDefenseLayouts.EmberCircuitId,
                new[] { "ember.entry", "ember.junction", "ember.hellhound", "ember.flame", "ember.brute", "ember.heart" },
                new[]
                {
                    Step("ember.entry-junction", InfernalTraversalClass.Safe),
                    Step("ember.junction-hellhound", InfernalTraversalClass.Safe),
                    Step("ember.hellhound-flame", InfernalTraversalClass.DeclaredRisk),
                    Step("ember.flame-brute", InfernalTraversalClass.Safe),
                    Step("ember.brute-heart", InfernalTraversalClass.Safe)
                },
                "ember.flame-brute");

        public static IReadOnlyList<InfernalInvaderTraversalRecipe> All { get; } =
            Array.AsReadOnly(new[] { AshenSpur, CinderFork, EmberCircuit });

        private static InfernalInvaderTraversalRecipe Route(
            string layoutId,
            IReadOnlyList<string> nodes,
            IReadOnlyList<InfernalTraversalEdgeStep> edges,
            string lavaGateEdgeId)
        {
            return new InfernalInvaderTraversalRecipe(
                layoutId,
                layoutId,
                nodes,
                edges,
                new InfernalLavaGatePlacement(lavaGateEdgeId, .5f));
        }

        private static InfernalTraversalEdgeStep Step(
            string edgeId,
            InfernalTraversalClass traversalClass)
        {
            return new InfernalTraversalEdgeStep(edgeId, traversalClass);
        }
    }

    public static class StarterInfernalInvaderTraversalResolver
    {
        public static InfernalInvaderTraversalLookupResult ResolveExact(string layoutId)
        {
            if (!HasStableId(layoutId))
            {
                return new InfernalInvaderTraversalLookupResult(
                    InfernalInvaderTraversalLookupStatus.LayoutIdInvalid,
                    null);
            }

            if (!InfernalInvaderTraversalValidator.ValidateCatalogue(
                    StarterInfernalInvaderTraversals.All).IsValid)
            {
                return new InfernalInvaderTraversalLookupResult(
                    InfernalInvaderTraversalLookupStatus.CatalogueInvalid,
                    null);
            }

            foreach (var recipe in StarterInfernalInvaderTraversals.All)
            {
                if (string.Equals(recipe.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return new InfernalInvaderTraversalLookupResult(
                        InfernalInvaderTraversalLookupStatus.Found,
                        recipe);
                }
            }

            return new InfernalInvaderTraversalLookupResult(
                InfernalInvaderTraversalLookupStatus.NotFound,
                null);
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || !IsAsciiAlphaNumeric(value[0])
                || !IsAsciiAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            for (var index = 1; index < value.Length - 1; index++)
            {
                var character = value[index];
                if (!IsAsciiAlphaNumeric(character)
                    && character != '.'
                    && character != '-'
                    && character != '_')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiAlphaNumeric(char value)
        {
            return value >= 'a' && value <= 'z'
                || value >= 'A' && value <= 'Z'
                || value >= '0' && value <= '9';
        }
    }
}
