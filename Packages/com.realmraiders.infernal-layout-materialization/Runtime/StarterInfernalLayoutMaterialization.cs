using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.InfernalLayoutMaterialization
{
    public enum InfernalLayoutMaterializationRoleKind
    {
        Unknown,
        EntryGate,
        RouteJunction,
        HellhoundGround,
        FlameTrapGround,
        BruteGround,
        InfernalHeart
    }

    public enum InfernalLayoutMaterializationLookupStatus
    {
        Found,
        LayoutIdInvalid,
        NotFound,
        CatalogueInvalid
    }

    public enum InfernalLayoutMaterializationValidationIssue
    {
        RecipeMissing,
        LayoutIdInvalid,
        LayoutNotFound,
        LayoutInvalid,
        ThemeMissing,
        ThemeIdInvalid,
        ThemeTokenCardinalityInvalid,
        ThemeTokenInvalid,
        ThemeTokenDuplicate,
        ThemeIdNotAllowed,
        ThemeTokenSetMismatch,
        MappingCardinalityInvalid,
        MappingMissing,
        RoleIdInvalid,
        RoleDuplicate,
        RoleKindInvalid,
        FloorTreatmentIdInvalid,
        FloorTreatmentIdNotAllowed,
        BoundarySilhouetteIdInvalid,
        BoundarySilhouetteIdNotAllowed,
        LandmarkPresentationIdInvalid,
        LandmarkPresentationIdNotAllowed,
        NodeFootprintInvalid,
        NodeFootprintRangeMismatch,
        RouteWidthSourceMissing,
        RouteWidthSourceDuplicate,
        RouteWidthSourceInvalid,
        RouteWidthSourceCoverageInvalid,
        LandmarkBindingInvalid,
        MappingRoleCoverageInvalid
    }

    public enum InfernalLayoutMaterializationCatalogueValidationIssue
    {
        CatalogueMissing,
        RecipeCardinalityInvalid,
        RecipeMissing,
        LayoutIdDuplicate,
        RecipeInvalid,
        LayoutCoverageInvalid,
        SemanticSignatureDuplicate
    }

    public sealed class InfernalLayoutPresentationTheme
    {
        public InfernalLayoutPresentationTheme(
            string themeId,
            IReadOnlyList<string> presentationTokens)
        {
            ThemeId = themeId;
            PresentationTokens = Snapshot(presentationTokens);
        }

        public string ThemeId { get; }

        public IReadOnlyList<string> PresentationTokens { get; }

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

    /// <summary>
    /// One immutable role-to-presentation seam. Route widths remain source edge
    /// IDs, so consumers read the existing layout facts rather than copied values.
    /// </summary>
    public sealed class InfernalLayoutMaterializationFact
    {
        public InfernalLayoutMaterializationFact(
            string roleId,
            InfernalLayoutMaterializationRoleKind roleKind,
            string floorTreatmentId,
            string boundarySilhouetteId,
            string landmarkPresentationId,
            float nodeFootprintRadius,
            IReadOnlyList<string> routeWidthSourceEdgeIds)
        {
            RoleId = roleId;
            RoleKind = roleKind;
            FloorTreatmentId = floorTreatmentId;
            BoundarySilhouetteId = boundarySilhouetteId;
            LandmarkPresentationId = landmarkPresentationId;
            NodeFootprintRadius = nodeFootprintRadius;
            RouteWidthSourceEdgeIds = Snapshot(routeWidthSourceEdgeIds);
        }

        public string RoleId { get; }

        public InfernalLayoutMaterializationRoleKind RoleKind { get; }

        public string FloorTreatmentId { get; }

        public string BoundarySilhouetteId { get; }

        public string LandmarkPresentationId { get; }

        public float NodeFootprintRadius { get; }

        public IReadOnlyList<string> RouteWidthSourceEdgeIds { get; }

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

    public sealed class InfernalLayoutMaterializationRecipe
    {
        public InfernalLayoutMaterializationRecipe(
            string layoutId,
            InfernalLayoutPresentationTheme theme,
            IReadOnlyList<InfernalLayoutMaterializationFact> mappings)
        {
            LayoutId = layoutId;
            Theme = theme;
            Mappings = Snapshot(mappings);
        }

        public string LayoutId { get; }

        public InfernalLayoutPresentationTheme Theme { get; }

        public IReadOnlyList<InfernalLayoutMaterializationFact> Mappings { get; }

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

    public sealed class InfernalLayoutMaterializationValidationResult
    {
        internal InfernalLayoutMaterializationValidationResult(
            IReadOnlyList<InfernalLayoutMaterializationValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalLayoutMaterializationValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<InfernalLayoutMaterializationValidationIssue> Snapshot(
            IReadOnlyList<InfernalLayoutMaterializationValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(
                    Array.Empty<InfernalLayoutMaterializationValidationIssue>());
            }

            var copy = new InfernalLayoutMaterializationValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalLayoutMaterializationCatalogueValidationResult
    {
        internal InfernalLayoutMaterializationCatalogueValidationResult(
            IReadOnlyList<InfernalLayoutMaterializationCatalogueValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalLayoutMaterializationCatalogueValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<InfernalLayoutMaterializationCatalogueValidationIssue> Snapshot(
            IReadOnlyList<InfernalLayoutMaterializationCatalogueValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(
                    Array.Empty<InfernalLayoutMaterializationCatalogueValidationIssue>());
            }

            var copy = new InfernalLayoutMaterializationCatalogueValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalLayoutMaterializationLookupResult
    {
        internal InfernalLayoutMaterializationLookupResult(
            InfernalLayoutMaterializationLookupStatus status,
            InfernalLayoutMaterializationRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public InfernalLayoutMaterializationLookupStatus Status { get; }

        public InfernalLayoutMaterializationRecipe Recipe { get; }

        public bool Found
        {
            get
            {
                return Status == InfernalLayoutMaterializationLookupStatus.Found
                    && Recipe != null;
            }
        }
    }

    public static class InfernalLayoutMaterializationValidator
    {
        public static InfernalLayoutMaterializationValidationResult Validate(
            InfernalLayoutMaterializationRecipe recipe)
        {
            var issues = new List<InfernalLayoutMaterializationValidationIssue>();
            if (recipe == null)
            {
                AddIssue(issues, InfernalLayoutMaterializationValidationIssue.RecipeMissing);
                return new InfernalLayoutMaterializationValidationResult(issues);
            }

            if (!HasStableId(recipe.LayoutId))
            {
                AddIssue(issues, InfernalLayoutMaterializationValidationIssue.LayoutIdInvalid);
                return new InfernalLayoutMaterializationValidationResult(issues);
            }

            var layout = FindLayout(recipe.LayoutId);
            if (layout == null)
            {
                AddIssue(issues, InfernalLayoutMaterializationValidationIssue.LayoutNotFound);
                return new InfernalLayoutMaterializationValidationResult(issues);
            }

            if (!InfernalDefenseLayoutValidator.Validate(layout).IsValid)
            {
                AddIssue(issues, InfernalLayoutMaterializationValidationIssue.LayoutInvalid);
                return new InfernalLayoutMaterializationValidationResult(issues);
            }

            ValidateTheme(recipe.Theme, recipe.LayoutId, issues);
            if (recipe.Mappings.Count != ExpectedRoleIds.Count)
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.MappingCardinalityInvalid);
            }

            var mappingsByRoleId = new Dictionary<string, InfernalLayoutMaterializationFact>(
                StringComparer.Ordinal);
            foreach (var mapping in recipe.Mappings)
            {
                if (mapping == null)
                {
                    AddIssue(issues, InfernalLayoutMaterializationValidationIssue.MappingMissing);
                    continue;
                }

                if (!IsExpectedRoleId(mapping.RoleId))
                {
                    AddIssue(issues, InfernalLayoutMaterializationValidationIssue.RoleIdInvalid);
                }
                else if (mappingsByRoleId.ContainsKey(mapping.RoleId))
                {
                    AddIssue(issues, InfernalLayoutMaterializationValidationIssue.RoleDuplicate);
                }
                else
                {
                    mappingsByRoleId.Add(mapping.RoleId, mapping);
                }

                ValidateMapping(mapping, layout, issues);
            }

            foreach (var roleId in ExpectedRoleIds)
            {
                if (!mappingsByRoleId.ContainsKey(roleId))
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationValidationIssue.MappingRoleCoverageInvalid);
                }
            }

            return new InfernalLayoutMaterializationValidationResult(issues);
        }

        public static InfernalLayoutMaterializationCatalogueValidationResult ValidateCatalogue(
            IReadOnlyList<InfernalLayoutMaterializationRecipe> recipes)
        {
            var issues = new List<InfernalLayoutMaterializationCatalogueValidationIssue>();
            if (recipes == null)
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationCatalogueValidationIssue.CatalogueMissing);
                return new InfernalLayoutMaterializationCatalogueValidationResult(issues);
            }

            if (recipes.Count != StarterInfernalDefenseLayouts.All.Count)
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationCatalogueValidationIssue.RecipeCardinalityInvalid);
            }

            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            var signatures = new HashSet<string>(StringComparer.Ordinal);
            foreach (var recipe in recipes)
            {
                if (recipe == null)
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationCatalogueValidationIssue.RecipeMissing);
                    continue;
                }

                if (!layoutIds.Add(recipe.LayoutId))
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationCatalogueValidationIssue.LayoutIdDuplicate);
                }

                if (!Validate(recipe).IsValid)
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationCatalogueValidationIssue.RecipeInvalid);
                }

                var signature = CreateSemanticPresentationSignature(recipe);
                if (signature != null && !signatures.Add(signature))
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationCatalogueValidationIssue.SemanticSignatureDuplicate);
                }
            }

            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                if (!layoutIds.Contains(layout.LayoutId))
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationCatalogueValidationIssue.LayoutCoverageInvalid);
                }
            }

            return new InfernalLayoutMaterializationCatalogueValidationResult(issues);
        }

        public static string CreateSemanticPresentationSignature(
            InfernalLayoutMaterializationRecipe recipe)
        {
            if (recipe == null || !Validate(recipe).IsValid)
            {
                return null;
            }

            var parts = new List<string>();
            var tokens = CanonicalTokens(recipe.Theme.PresentationTokens);
            parts.Add(string.Join(",", tokens));
            foreach (var mapping in recipe.Mappings)
            {
                parts.Add(mapping.RoleId
                    + ":"
                    + ((int)mapping.RoleKind).ToString()
                    + ":"
                    + mapping.FloorTreatmentId
                    + ":"
                    + mapping.BoundarySilhouetteId
                    + ":"
                    + mapping.LandmarkPresentationId);
            }

            parts.Sort(StringComparer.Ordinal);
            return string.Join("|", parts.ToArray());
        }

        private static readonly IReadOnlyList<string> ExpectedRoleIds =
            Array.AsReadOnly(new[]
            {
                StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                StarterInfernalDefenseLayouts.RouteJunctionRoleId,
                StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId
            });

        private static void ValidateTheme(
            InfernalLayoutPresentationTheme theme,
            string layoutId,
            ICollection<InfernalLayoutMaterializationValidationIssue> issues)
        {
            if (theme == null)
            {
                AddIssue(issues, InfernalLayoutMaterializationValidationIssue.ThemeMissing);
                return;
            }

            if (!HasStableId(theme.ThemeId))
            {
                AddIssue(issues, InfernalLayoutMaterializationValidationIssue.ThemeIdInvalid);
            }

            if (theme.PresentationTokens.Count < 2 || theme.PresentationTokens.Count > 3)
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.ThemeTokenCardinalityInvalid);
            }

            var tokenIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var token in theme.PresentationTokens)
            {
                if (!HasStableId(token))
                {
                    AddIssue(issues, InfernalLayoutMaterializationValidationIssue.ThemeTokenInvalid);
                }
                else if (!tokenIds.Add(token))
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationValidationIssue.ThemeTokenDuplicate);
                }
            }

            var expectedProfile = FindPresentationProfile(layoutId);
            if (expectedProfile != null)
            {
                if (!string.Equals(theme.ThemeId, expectedProfile.ThemeId,
                        StringComparison.Ordinal))
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationValidationIssue.ThemeIdNotAllowed);
                }

                if (!HasExactTokenSet(theme.PresentationTokens, expectedProfile.Tokens))
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationValidationIssue.ThemeTokenSetMismatch);
                }
            }
        }

        private static void ValidateMapping(
            InfernalLayoutMaterializationFact mapping,
            RealmLayoutGraph layout,
            ICollection<InfernalLayoutMaterializationValidationIssue> issues)
        {
            if (!HasExpectedKind(mapping.RoleId, mapping.RoleKind))
            {
                AddIssue(issues, InfernalLayoutMaterializationValidationIssue.RoleKindInvalid);
            }

            if (!HasStableId(mapping.FloorTreatmentId))
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.FloorTreatmentIdInvalid);
            }
            else if (!string.Equals(mapping.FloorTreatmentId,
                         ExpectedFloorTreatmentId(mapping.RoleId),
                         StringComparison.Ordinal))
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.FloorTreatmentIdNotAllowed);
            }

            if (!HasStableId(mapping.BoundarySilhouetteId))
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.BoundarySilhouetteIdInvalid);
            }
            else
            {
                var profile = FindPresentationProfile(layout.LayoutId);
                if (profile == null
                    || !string.Equals(mapping.BoundarySilhouetteId,
                        profile.BoundarySilhouetteId,
                        StringComparison.Ordinal))
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationValidationIssue.BoundarySilhouetteIdNotAllowed);
                }
            }

            if (!HasStableId(mapping.LandmarkPresentationId))
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.LandmarkPresentationIdInvalid);
            }
            else if (!IsAllowedLandmarkPresentationId(mapping.LandmarkPresentationId))
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.LandmarkPresentationIdNotAllowed);
            }

            if (float.IsNaN(mapping.NodeFootprintRadius)
                || float.IsInfinity(mapping.NodeFootprintRadius))
            {
                AddIssue(issues, InfernalLayoutMaterializationValidationIssue.NodeFootprintInvalid);
            }
            else if (mapping.NodeFootprintRadius != RealmLayoutGraphValidator.NodeFootprintRadius)
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.NodeFootprintRangeMismatch);
            }

            var node = FindNodeByRoleId(layout, mapping.RoleId);
            ValidateRouteWidthSources(mapping, node, layout, issues);
            ValidateLandmarkBinding(mapping, node, layout, issues);
        }

        private static void ValidateRouteWidthSources(
            InfernalLayoutMaterializationFact mapping,
            RealmLayoutGraphNode node,
            RealmLayoutGraph layout,
            ICollection<InfernalLayoutMaterializationValidationIssue> issues)
        {
            if (mapping.RouteWidthSourceEdgeIds.Count == 0)
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.RouteWidthSourceMissing);
            }

            var sourceEdgeIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var edgeId in mapping.RouteWidthSourceEdgeIds)
            {
                if (!HasStableId(edgeId)
                    || !sourceEdgeIds.Add(edgeId))
                {
                    AddIssue(issues,
                        !HasStableId(edgeId)
                            ? InfernalLayoutMaterializationValidationIssue.RouteWidthSourceInvalid
                            : InfernalLayoutMaterializationValidationIssue.RouteWidthSourceDuplicate);
                    continue;
                }

                var edge = FindEdge(layout, edgeId);
                if (node == null
                    || edge == null
                    || !EdgeTouchesNode(edge, node.NodeId)
                    || edge.FloorPathWidth < RealmLayoutGraphValidator.MinimumFloorPathWidth
                    || edge.FloorPathWidth > RealmLayoutGraphValidator.MaximumFloorPathWidth)
                {
                    AddIssue(issues,
                        InfernalLayoutMaterializationValidationIssue.RouteWidthSourceInvalid);
                }
            }

            if (node != null && !HasExactIncidentEdgeCoverage(sourceEdgeIds, node.NodeId, layout))
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.RouteWidthSourceCoverageInvalid);
            }
        }

        private static void ValidateLandmarkBinding(
            InfernalLayoutMaterializationFact mapping,
            RealmLayoutGraphNode node,
            RealmLayoutGraph layout,
            ICollection<InfernalLayoutMaterializationValidationIssue> issues)
        {
            var expectedPresentationId = StarterInfernalLayoutMaterializations.NonePresentationId;
            if (node != null)
            {
                foreach (var landmark in layout.Landmarks)
                {
                    if (string.Equals(landmark.NodeId, node.NodeId, StringComparison.Ordinal))
                    {
                        expectedPresentationId = landmark.PresentationRoleId;
                        break;
                    }
                }
            }

            if (!string.Equals(mapping.LandmarkPresentationId, expectedPresentationId,
                    StringComparison.Ordinal))
            {
                AddIssue(issues,
                    InfernalLayoutMaterializationValidationIssue.LandmarkBindingInvalid);
            }
        }

        private static bool HasExactIncidentEdgeCoverage(
            ISet<string> sourceEdgeIds,
            string nodeId,
            RealmLayoutGraph layout)
        {
            var incidentEdgeIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var edge in layout.Edges)
            {
                if (EdgeTouchesNode(edge, nodeId))
                {
                    incidentEdgeIds.Add(edge.EdgeId);
                }
            }

            return incidentEdgeIds.SetEquals(sourceEdgeIds);
        }

        private static readonly IReadOnlyList<PresentationProfile> PresentationProfiles =
            Array.AsReadOnly(new[]
            {
                new PresentationProfile(
                    StarterInfernalDefenseLayouts.AshenSpurId,
                    "realmraiders.infernal-presentation.theme.ashen-spur",
                    "realmraiders.infernal-presentation.boundary.ashen-spur",
                    new[]
                    {
                        "realmraiders.infernal-presentation.token.ash",
                        "realmraiders.infernal-presentation.token.basalt"
                    }),
                new PresentationProfile(
                    StarterInfernalDefenseLayouts.CinderForkId,
                    "realmraiders.infernal-presentation.theme.cinder-fork",
                    "realmraiders.infernal-presentation.boundary.cinder-fork",
                    new[]
                    {
                        "realmraiders.infernal-presentation.token.cinder",
                        "realmraiders.infernal-presentation.token.basalt"
                    }),
                new PresentationProfile(
                    StarterInfernalDefenseLayouts.EmberCircuitId,
                    "realmraiders.infernal-presentation.theme.ember-circuit",
                    "realmraiders.infernal-presentation.boundary.ember-circuit",
                    new[]
                    {
                        "realmraiders.infernal-presentation.token.ember",
                        "realmraiders.infernal-presentation.token.obsidian"
                    })
            });

        private static PresentationProfile FindPresentationProfile(string layoutId)
        {
            foreach (var profile in PresentationProfiles)
            {
                if (string.Equals(profile.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return profile;
                }
            }

            return null;
        }

        private static string ExpectedFloorTreatmentId(string roleId)
        {
            if (string.Equals(roleId, StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                    StringComparison.Ordinal))
            {
                return "realmraiders.infernal-presentation.floor.entry-gate";
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.RouteJunctionRoleId,
                    StringComparison.Ordinal))
            {
                return "realmraiders.infernal-presentation.floor.route-junction";
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                    StringComparison.Ordinal))
            {
                return "realmraiders.infernal-presentation.floor.hellhound-ground";
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                    StringComparison.Ordinal))
            {
                return "realmraiders.infernal-presentation.floor.flame-trap-ground";
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                    StringComparison.Ordinal))
            {
                return "realmraiders.infernal-presentation.floor.brute-ground";
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                    StringComparison.Ordinal))
            {
                return "realmraiders.infernal-presentation.floor.infernal-heart";
            }

            return null;
        }

        private static bool IsAllowedLandmarkPresentationId(string presentationId)
        {
            return string.Equals(presentationId,
                       StarterInfernalLayoutMaterializations.NonePresentationId,
                       StringComparison.Ordinal)
                || string.Equals(presentationId, "realmraiders.infernal-defense.ash-crucible",
                    StringComparison.Ordinal)
                || string.Equals(presentationId, "realmraiders.infernal-defense.cinder-fork",
                    StringComparison.Ordinal)
                || string.Equals(presentationId, "realmraiders.infernal-defense.circuit-brazier",
                    StringComparison.Ordinal)
                || string.Equals(presentationId, "realmraiders.infernal-defense.heart-altar",
                    StringComparison.Ordinal);
        }

        private static bool HasExactTokenSet(
            IReadOnlyList<string> actualTokens,
            IReadOnlyList<string> expectedTokens)
        {
            return new HashSet<string>(actualTokens, StringComparer.Ordinal)
                .SetEquals(expectedTokens)
                && actualTokens.Count == expectedTokens.Count;
        }

        private static string[] CanonicalTokens(IReadOnlyList<string> tokens)
        {
            var copy = new string[tokens.Count];
            for (var index = 0; index < tokens.Count; index++)
            {
                copy[index] = tokens[index];
            }

            Array.Sort(copy, StringComparer.Ordinal);
            return copy;
        }

        private sealed class PresentationProfile
        {
            public PresentationProfile(
                string layoutId,
                string themeId,
                string boundarySilhouetteId,
                IReadOnlyList<string> tokens)
            {
                LayoutId = layoutId;
                ThemeId = themeId;
                BoundarySilhouetteId = boundarySilhouetteId;
                Tokens = tokens;
            }

            public string LayoutId { get; }

            public string ThemeId { get; }

            public string BoundarySilhouetteId { get; }

            public IReadOnlyList<string> Tokens { get; }
        }

        private static RealmLayoutGraph FindLayout(string layoutId)
        {
            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                if (string.Equals(layout.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return layout;
                }
            }

            return null;
        }

        private static RealmLayoutGraphNode FindNodeByRoleId(
            RealmLayoutGraph layout,
            string roleId)
        {
            foreach (var node in layout.Nodes)
            {
                if (string.Equals(node.GameplayRoleId, roleId, StringComparison.Ordinal))
                {
                    return node;
                }
            }

            return null;
        }

        private static RealmLayoutGraphEdge FindEdge(RealmLayoutGraph layout, string edgeId)
        {
            foreach (var edge in layout.Edges)
            {
                if (string.Equals(edge.EdgeId, edgeId, StringComparison.Ordinal))
                {
                    return edge;
                }
            }

            return null;
        }

        private static bool EdgeTouchesNode(RealmLayoutGraphEdge edge, string nodeId)
        {
            return edge != null
                && (string.Equals(edge.FromNodeId, nodeId, StringComparison.Ordinal)
                    || string.Equals(edge.ToNodeId, nodeId, StringComparison.Ordinal));
        }

        private static bool IsExpectedRoleId(string roleId)
        {
            foreach (var expectedRoleId in ExpectedRoleIds)
            {
                if (string.Equals(roleId, expectedRoleId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasExpectedKind(
            string roleId,
            InfernalLayoutMaterializationRoleKind roleKind)
        {
            if (string.Equals(roleId, StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                    StringComparison.Ordinal))
            {
                return roleKind == InfernalLayoutMaterializationRoleKind.EntryGate;
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.RouteJunctionRoleId,
                    StringComparison.Ordinal))
            {
                return roleKind == InfernalLayoutMaterializationRoleKind.RouteJunction;
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                    StringComparison.Ordinal))
            {
                return roleKind == InfernalLayoutMaterializationRoleKind.HellhoundGround;
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                    StringComparison.Ordinal))
            {
                return roleKind == InfernalLayoutMaterializationRoleKind.FlameTrapGround;
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                    StringComparison.Ordinal))
            {
                return roleKind == InfernalLayoutMaterializationRoleKind.BruteGround;
            }

            return string.Equals(roleId, StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                    StringComparison.Ordinal)
                && roleKind == InfernalLayoutMaterializationRoleKind.InfernalHeart;
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
                var symbol = value[index];
                if (!IsAsciiAlphaNumeric(symbol)
                    && symbol != '.'
                    && symbol != '_'
                    && symbol != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiAlphaNumeric(char symbol)
        {
            return symbol >= 'a' && symbol <= 'z'
                || symbol >= '0' && symbol <= '9';
        }

        private static void AddIssue(
            ICollection<InfernalLayoutMaterializationValidationIssue> issues,
            InfernalLayoutMaterializationValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private static void AddIssue(
            ICollection<InfernalLayoutMaterializationCatalogueValidationIssue> issues,
            InfernalLayoutMaterializationCatalogueValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }

    public static class StarterInfernalLayoutMaterializations
    {
        public const string NonePresentationId = "realmraiders.infernal-presentation.none";

        public static InfernalLayoutMaterializationRecipe AshenSpur { get; } =
            CreateAshenSpur();

        public static InfernalLayoutMaterializationRecipe CinderFork { get; } =
            CreateCinderFork();

        public static InfernalLayoutMaterializationRecipe EmberCircuit { get; } =
            CreateEmberCircuit();

        public static IReadOnlyList<InfernalLayoutMaterializationRecipe> All { get; } =
            Array.AsReadOnly(new[]
            {
                AshenSpur,
                CinderFork,
                EmberCircuit
            });

        private static InfernalLayoutMaterializationRecipe CreateAshenSpur()
        {
            return Recipe(
                StarterInfernalDefenseLayouts.AshenSpurId,
                "realmraiders.infernal-presentation.theme.ashen-spur",
                new[]
                {
                    "realmraiders.infernal-presentation.token.ash",
                    "realmraiders.infernal-presentation.token.basalt"
                },
                "realmraiders.infernal-presentation.boundary.ashen-spur",
                "realmraiders.infernal-defense.ash-crucible",
                new[]
                {
                    Fact(StarterInfernalDefenseLayouts.InvaderEntryRoleId, InfernalLayoutMaterializationRoleKind.EntryGate, "floor.entry-gate", NonePresentationId, "ashen.entry-junction"),
                    Fact(StarterInfernalDefenseLayouts.RouteJunctionRoleId, InfernalLayoutMaterializationRoleKind.RouteJunction, "floor.route-junction", "realmraiders.infernal-defense.ash-crucible", "ashen.entry-junction", "ashen.junction-hellhound", "ashen.junction-flame"),
                    Fact(StarterInfernalDefenseLayouts.HellhoundEncounterRoleId, InfernalLayoutMaterializationRoleKind.HellhoundGround, "floor.hellhound-ground", NonePresentationId, "ashen.junction-hellhound"),
                    Fact(StarterInfernalDefenseLayouts.FlameTrapHazardRoleId, InfernalLayoutMaterializationRoleKind.FlameTrapGround, "floor.flame-trap-ground", NonePresentationId, "ashen.junction-flame", "ashen.flame-brute"),
                    Fact(StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId, InfernalLayoutMaterializationRoleKind.BruteGround, "floor.brute-ground", NonePresentationId, "ashen.flame-brute", "ashen.brute-heart"),
                    Fact(StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId, InfernalLayoutMaterializationRoleKind.InfernalHeart, "floor.infernal-heart", "realmraiders.infernal-defense.heart-altar", "ashen.brute-heart")
                });
        }

        private static InfernalLayoutMaterializationRecipe CreateCinderFork()
        {
            return Recipe(
                StarterInfernalDefenseLayouts.CinderForkId,
                "realmraiders.infernal-presentation.theme.cinder-fork",
                new[]
                {
                    "realmraiders.infernal-presentation.token.cinder",
                    "realmraiders.infernal-presentation.token.basalt"
                },
                "realmraiders.infernal-presentation.boundary.cinder-fork",
                "realmraiders.infernal-defense.cinder-fork",
                new[]
                {
                    Fact(StarterInfernalDefenseLayouts.InvaderEntryRoleId, InfernalLayoutMaterializationRoleKind.EntryGate, "floor.entry-gate", NonePresentationId, "cinder.entry-junction"),
                    Fact(StarterInfernalDefenseLayouts.RouteJunctionRoleId, InfernalLayoutMaterializationRoleKind.RouteJunction, "floor.route-junction", "realmraiders.infernal-defense.cinder-fork", "cinder.entry-junction", "cinder.junction-hellhound", "cinder.junction-flame"),
                    Fact(StarterInfernalDefenseLayouts.HellhoundEncounterRoleId, InfernalLayoutMaterializationRoleKind.HellhoundGround, "floor.hellhound-ground", NonePresentationId, "cinder.junction-hellhound", "cinder.hellhound-brute"),
                    Fact(StarterInfernalDefenseLayouts.FlameTrapHazardRoleId, InfernalLayoutMaterializationRoleKind.FlameTrapGround, "floor.flame-trap-ground", NonePresentationId, "cinder.junction-flame", "cinder.flame-brute"),
                    Fact(StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId, InfernalLayoutMaterializationRoleKind.BruteGround, "floor.brute-ground", NonePresentationId, "cinder.hellhound-brute", "cinder.flame-brute", "cinder.brute-heart"),
                    Fact(StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId, InfernalLayoutMaterializationRoleKind.InfernalHeart, "floor.infernal-heart", "realmraiders.infernal-defense.heart-altar", "cinder.brute-heart")
                });
        }

        private static InfernalLayoutMaterializationRecipe CreateEmberCircuit()
        {
            return Recipe(
                StarterInfernalDefenseLayouts.EmberCircuitId,
                "realmraiders.infernal-presentation.theme.ember-circuit",
                new[]
                {
                    "realmraiders.infernal-presentation.token.ember",
                    "realmraiders.infernal-presentation.token.obsidian"
                },
                "realmraiders.infernal-presentation.boundary.ember-circuit",
                "realmraiders.infernal-defense.circuit-brazier",
                new[]
                {
                    Fact(StarterInfernalDefenseLayouts.InvaderEntryRoleId, InfernalLayoutMaterializationRoleKind.EntryGate, "floor.entry-gate", NonePresentationId, "ember.entry-junction"),
                    Fact(StarterInfernalDefenseLayouts.RouteJunctionRoleId, InfernalLayoutMaterializationRoleKind.RouteJunction, "floor.route-junction", "realmraiders.infernal-defense.circuit-brazier", "ember.entry-junction", "ember.junction-hellhound", "ember.junction-brute"),
                    Fact(StarterInfernalDefenseLayouts.HellhoundEncounterRoleId, InfernalLayoutMaterializationRoleKind.HellhoundGround, "floor.hellhound-ground", NonePresentationId, "ember.junction-hellhound", "ember.hellhound-flame"),
                    Fact(StarterInfernalDefenseLayouts.FlameTrapHazardRoleId, InfernalLayoutMaterializationRoleKind.FlameTrapGround, "floor.flame-trap-ground", NonePresentationId, "ember.hellhound-flame", "ember.flame-brute"),
                    Fact(StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId, InfernalLayoutMaterializationRoleKind.BruteGround, "floor.brute-ground", NonePresentationId, "ember.flame-brute", "ember.junction-brute", "ember.brute-heart"),
                    Fact(StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId, InfernalLayoutMaterializationRoleKind.InfernalHeart, "floor.infernal-heart", "realmraiders.infernal-defense.heart-altar", "ember.brute-heart")
                });
        }

        private static InfernalLayoutMaterializationRecipe Recipe(
            string layoutId,
            string themeId,
            IReadOnlyList<string> tokens,
            string boundarySilhouetteId,
            string junctionPresentationId,
            IReadOnlyList<InfernalLayoutMaterializationFact> mappings)
        {
            return new InfernalLayoutMaterializationRecipe(
                layoutId,
                new InfernalLayoutPresentationTheme(themeId, tokens),
                ApplyBoundaryAndJunctionPresentation(
                    mappings,
                    boundarySilhouetteId,
                    junctionPresentationId));
        }

        private static IReadOnlyList<InfernalLayoutMaterializationFact> ApplyBoundaryAndJunctionPresentation(
            IReadOnlyList<InfernalLayoutMaterializationFact> mappings,
            string boundarySilhouetteId,
            string junctionPresentationId)
        {
            var copy = new InfernalLayoutMaterializationFact[mappings.Count];
            for (var index = 0; index < mappings.Count; index++)
            {
                var mapping = mappings[index];
                var landmarkPresentationId = string.Equals(mapping.RoleId,
                    StarterInfernalDefenseLayouts.RouteJunctionRoleId,
                    StringComparison.Ordinal)
                    ? junctionPresentationId
                    : mapping.LandmarkPresentationId;
                copy[index] = new InfernalLayoutMaterializationFact(
                    mapping.RoleId,
                    mapping.RoleKind,
                    mapping.FloorTreatmentId,
                    boundarySilhouetteId,
                    landmarkPresentationId,
                    mapping.NodeFootprintRadius,
                    mapping.RouteWidthSourceEdgeIds);
            }

            return Array.AsReadOnly(copy);
        }

        private static InfernalLayoutMaterializationFact Fact(
            string roleId,
            InfernalLayoutMaterializationRoleKind roleKind,
            string floorSuffix,
            string landmarkPresentationId,
            params string[] routeWidthSourceEdgeIds)
        {
            return new InfernalLayoutMaterializationFact(
                roleId,
                roleKind,
                "realmraiders.infernal-presentation." + floorSuffix,
                NonePresentationId,
                landmarkPresentationId,
                RealmLayoutGraphValidator.NodeFootprintRadius,
                routeWidthSourceEdgeIds);
        }
    }

    public static class StarterInfernalLayoutMaterializationResolver
    {
        public static InfernalLayoutMaterializationLookupResult ResolveExact(string layoutId)
        {
            if (!HasStableId(layoutId))
            {
                return new InfernalLayoutMaterializationLookupResult(
                    InfernalLayoutMaterializationLookupStatus.LayoutIdInvalid,
                    null);
            }

            if (!InfernalLayoutMaterializationValidator.ValidateCatalogue(
                    StarterInfernalLayoutMaterializations.All).IsValid)
            {
                return new InfernalLayoutMaterializationLookupResult(
                    InfernalLayoutMaterializationLookupStatus.CatalogueInvalid,
                    null);
            }

            foreach (var recipe in StarterInfernalLayoutMaterializations.All)
            {
                if (string.Equals(recipe.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return new InfernalLayoutMaterializationLookupResult(
                        InfernalLayoutMaterializationLookupStatus.Found,
                        recipe);
                }
            }

            return new InfernalLayoutMaterializationLookupResult(
                InfernalLayoutMaterializationLookupStatus.NotFound,
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
                var symbol = value[index];
                if (!IsAsciiAlphaNumeric(symbol)
                    && symbol != '.'
                    && symbol != '_'
                    && symbol != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiAlphaNumeric(char symbol)
        {
            return symbol >= 'a' && symbol <= 'z'
                || symbol >= '0' && symbol <= '9';
        }
    }
}
