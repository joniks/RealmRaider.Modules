using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalEncounters;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.InfernalDefenseLayouts
{
    public enum InfernalDefenseLayoutValidationIssue
    {
        LayoutMissing,
        GenericGraphInvalid,
        EntryRoleCardinalityInvalid,
        RouteJunctionRoleCardinalityInvalid,
        HellhoundRoleCardinalityInvalid,
        FlameTrapRoleCardinalityInvalid,
        InfernalBruteRoleCardinalityInvalid,
        InfernalHeartRoleCardinalityInvalid,
        UnexpectedGameplayRole,
        RoleNodeKindInvalid
    }

    public enum InfernalDefenseLayoutCatalogueValidationIssue
    {
        CatalogueMissing,
        LayoutCardinalityInvalid,
        LayoutMissing,
        LayoutIdDuplicate,
        LayoutInvalid,
        TopologySignatureDuplicate,
        SafePathSignatureDuplicate
    }

    public enum InfernalDefenseLayoutSelectionStatus
    {
        Selected,
        PreviousLayoutIdInvalid,
        CatalogueInvalid
    }

    public enum InfernalDefenseLayoutResolveStatus
    {
        Resolved,
        LayoutIdInvalid,
        CatalogueInvalid
    }

    /// <summary>Immutable Infernal-specific evidence layered over generic graph validation.</summary>
    public sealed class InfernalDefenseLayoutValidationResult
    {
        internal InfernalDefenseLayoutValidationResult(
            RealmLayoutGraphValidationResult graphValidation,
            IReadOnlyList<InfernalDefenseLayoutValidationIssue> issues)
        {
            GraphValidation = graphValidation;
            Issues = Snapshot(issues);
        }

        public RealmLayoutGraphValidationResult GraphValidation { get; }

        public IReadOnlyList<InfernalDefenseLayoutValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return GraphValidation != null
                    && GraphValidation.IsValid
                    && Issues.Count == 0;
            }
        }

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

    public sealed class InfernalDefenseLayoutCatalogueValidationResult
    {
        internal InfernalDefenseLayoutCatalogueValidationResult(
            IReadOnlyList<InfernalDefenseLayoutCatalogueValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalDefenseLayoutCatalogueValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

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

    public sealed class InfernalDefenseLayoutSelectionResult
    {
        internal InfernalDefenseLayoutSelectionResult(
            InfernalDefenseLayoutSelectionStatus status,
            RealmLayoutGraph layout)
        {
            Status = status;
            Layout = layout;
        }

        public InfernalDefenseLayoutSelectionStatus Status { get; }

        public RealmLayoutGraph Layout { get; }

        public bool HasLayout
        {
            get
            {
                return Status == InfernalDefenseLayoutSelectionStatus.Selected
                    && Layout != null;
            }
        }
    }

    public sealed class InfernalDefenseLayoutResolveResult
    {
        internal InfernalDefenseLayoutResolveResult(
            InfernalDefenseLayoutResolveStatus status,
            RealmLayoutGraph layout)
        {
            Status = status;
            Layout = layout;
        }

        public InfernalDefenseLayoutResolveStatus Status { get; }

        public RealmLayoutGraph Layout { get; }

        public bool HasLayout
        {
            get
            {
                return Status == InfernalDefenseLayoutResolveStatus.Resolved
                    && Layout != null;
            }
        }
    }

    /// <summary>
    /// Exact-role validation layered over the shared physical graph contract. It
    /// contains no scene, spawning, combat, possession, save, or UI authority.
    /// </summary>
    public static class InfernalDefenseLayoutValidator
    {
        public static InfernalDefenseLayoutValidationResult Validate(RealmLayoutGraph layout)
        {
            var issues = new List<InfernalDefenseLayoutValidationIssue>();
            if (layout == null)
            {
                AddIssue(issues, InfernalDefenseLayoutValidationIssue.LayoutMissing);
                return new InfernalDefenseLayoutValidationResult(
                    RealmLayoutGraphValidator.Validate(null),
                    issues);
            }

            var graphValidation = RealmLayoutGraphValidator.Validate(layout);
            if (!graphValidation.IsValid)
            {
                AddIssue(issues, InfernalDefenseLayoutValidationIssue.GenericGraphInvalid);
            }

            var roleCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var hasUnexpectedRole = false;
            var hasRoleNodeKindMismatch = false;
            foreach (var node in layout.Nodes)
            {
                if (node == null)
                {
                    continue;
                }

                if (!IsExpectedRole(node.GameplayRoleId))
                {
                    hasUnexpectedRole = true;
                    continue;
                }

                if (!roleCounts.ContainsKey(node.GameplayRoleId))
                {
                    roleCounts.Add(node.GameplayRoleId, 0);
                }

                roleCounts[node.GameplayRoleId]++;
                if (!IsExpectedKind(node.GameplayRoleId, node.Kind))
                {
                    hasRoleNodeKindMismatch = true;
                }
            }

            AddCardinalityIssue(
                roleCounts,
                StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                InfernalDefenseLayoutValidationIssue.EntryRoleCardinalityInvalid,
                issues);
            AddCardinalityIssue(
                roleCounts,
                StarterInfernalDefenseLayouts.RouteJunctionRoleId,
                InfernalDefenseLayoutValidationIssue.RouteJunctionRoleCardinalityInvalid,
                issues);
            AddCardinalityIssue(
                roleCounts,
                StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                InfernalDefenseLayoutValidationIssue.HellhoundRoleCardinalityInvalid,
                issues);
            AddCardinalityIssue(
                roleCounts,
                StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                InfernalDefenseLayoutValidationIssue.FlameTrapRoleCardinalityInvalid,
                issues);
            AddCardinalityIssue(
                roleCounts,
                StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                InfernalDefenseLayoutValidationIssue.InfernalBruteRoleCardinalityInvalid,
                issues);
            AddCardinalityIssue(
                roleCounts,
                StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                InfernalDefenseLayoutValidationIssue.InfernalHeartRoleCardinalityInvalid,
                issues);

            if (hasUnexpectedRole)
            {
                AddIssue(issues, InfernalDefenseLayoutValidationIssue.UnexpectedGameplayRole);
            }

            if (hasRoleNodeKindMismatch)
            {
                AddIssue(issues, InfernalDefenseLayoutValidationIssue.RoleNodeKindInvalid);
            }

            return new InfernalDefenseLayoutValidationResult(graphValidation, issues);
        }

        public static InfernalDefenseLayoutCatalogueValidationResult ValidateCatalogue(
            IReadOnlyList<RealmLayoutGraph> layouts)
        {
            var issues = new List<InfernalDefenseLayoutCatalogueValidationIssue>();
            if (layouts == null)
            {
                AddIssue(issues, InfernalDefenseLayoutCatalogueValidationIssue.CatalogueMissing);
                return new InfernalDefenseLayoutCatalogueValidationResult(issues);
            }

            if (layouts.Count != 3)
            {
                AddIssue(issues, InfernalDefenseLayoutCatalogueValidationIssue.LayoutCardinalityInvalid);
            }

            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            var topologySignatures = new HashSet<string>(StringComparer.Ordinal);
            var safePathSignatures = new HashSet<string>(StringComparer.Ordinal);
            foreach (var layout in layouts)
            {
                if (layout == null)
                {
                    AddIssue(issues, InfernalDefenseLayoutCatalogueValidationIssue.LayoutMissing);
                    continue;
                }

                if (!layoutIds.Add(layout.LayoutId))
                {
                    AddIssue(issues, InfernalDefenseLayoutCatalogueValidationIssue.LayoutIdDuplicate);
                }

                if (!Validate(layout).IsValid)
                {
                    AddIssue(issues, InfernalDefenseLayoutCatalogueValidationIssue.LayoutInvalid);
                }

                var topologySignature = CreateTopologySignature(layout);
                if (topologySignature != null && !topologySignatures.Add(topologySignature))
                {
                    AddIssue(issues,
                        InfernalDefenseLayoutCatalogueValidationIssue.TopologySignatureDuplicate);
                }

                var safePathSignature = CreateSafePathSignature(layout);
                if (safePathSignature != null && !safePathSignatures.Add(safePathSignature))
                {
                    AddIssue(issues,
                        InfernalDefenseLayoutCatalogueValidationIssue.SafePathSignatureDuplicate);
                }
            }

            return new InfernalDefenseLayoutCatalogueValidationResult(issues);
        }

        /// <summary>
        /// Canonical undirected role-pair topology for authoring review. It never
        /// includes safe-route flags, selects a layout, or directs runtime behavior.
        /// </summary>
        public static string CreateTopologySignature(RealmLayoutGraph layout)
        {
            return CreateRolePairSignature(layout, false);
        }

        /// <summary>
        /// Canonical undirected role pairs reachable through only explicitly safe
        /// edges. Risk edges never contribute to this independent signature.
        /// </summary>
        public static string CreateSafePathSignature(RealmLayoutGraph layout)
        {
            return CreateRolePairSignature(layout, true);
        }

        private static string CreateRolePairSignature(
            RealmLayoutGraph layout,
            bool safeOnly)
        {
            if (layout == null || !Validate(layout).IsValid)
            {
                return null;
            }

            var rolesByNodeId = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var node in layout.Nodes)
            {
                rolesByNodeId.Add(node.NodeId, node.GameplayRoleId);
            }

            var rolePairs = new List<string>();
            foreach (var edge in layout.Edges)
            {
                if (safeOnly && !edge.IsActivePathSafe)
                {
                    continue;
                }

                if (!rolesByNodeId.TryGetValue(edge.FromNodeId, out var fromRole)
                    || !rolesByNodeId.TryGetValue(edge.ToNodeId, out var toRole))
                {
                    return null;
                }

                rolePairs.Add(UndirectedRolePair(fromRole, toRole));
            }

            rolePairs.Sort(StringComparer.Ordinal);
            return string.Join("|", rolePairs.ToArray());
        }

        private static string UndirectedRolePair(string firstRole, string secondRole)
        {
            return string.CompareOrdinal(firstRole, secondRole) < 0
                ? firstRole + ">" + secondRole
                : secondRole + ">" + firstRole;
        }

        private static bool IsExpectedRole(string roleId)
        {
            return string.Equals(roleId, StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                       StringComparison.Ordinal)
                || string.Equals(roleId, StarterInfernalDefenseLayouts.RouteJunctionRoleId,
                    StringComparison.Ordinal)
                || string.Equals(roleId, StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                    StringComparison.Ordinal)
                || string.Equals(roleId, StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                    StringComparison.Ordinal)
                || string.Equals(roleId, StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                    StringComparison.Ordinal)
                || string.Equals(roleId, StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                    StringComparison.Ordinal);
        }

        private static bool IsExpectedKind(
            string roleId,
            RealmLayoutGraphNodeKind kind)
        {
            if (string.Equals(roleId, StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                    StringComparison.Ordinal))
            {
                return kind == RealmLayoutGraphNodeKind.Entry;
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.RouteJunctionRoleId,
                    StringComparison.Ordinal))
            {
                return kind == RealmLayoutGraphNodeKind.Landmark;
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                    StringComparison.Ordinal))
            {
                return kind == RealmLayoutGraphNodeKind.Objective;
            }

            return kind == RealmLayoutGraphNodeKind.Encounter;
        }

        private static void AddCardinalityIssue(
            IReadOnlyDictionary<string, int> roleCounts,
            string roleId,
            InfernalDefenseLayoutValidationIssue issue,
            ICollection<InfernalDefenseLayoutValidationIssue> issues)
        {
            if (!roleCounts.TryGetValue(roleId, out var count) || count != 1)
            {
                AddIssue(issues, issue);
            }
        }

        private static void AddIssue(
            ICollection<InfernalDefenseLayoutValidationIssue> issues,
            InfernalDefenseLayoutValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private static void AddIssue(
            ICollection<InfernalDefenseLayoutCatalogueValidationIssue> issues,
            InfernalDefenseLayoutCatalogueValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }

    /// <summary>
    /// Three cached authored Infernal defense layouts. These facts never spawn or
    /// configure enemies, traps, possession, objectives, or presentation objects.
    /// </summary>
    public static class StarterInfernalDefenseLayouts
    {
        public const string AshenSpurId = "realmraiders.infernal-defense.ashen-spur";

        public const string CinderForkId = "realmraiders.infernal-defense.cinder-fork";

        public const string EmberCircuitId = "realmraiders.infernal-defense.ember-circuit";

        public const string InvaderEntryRoleId = "realmraiders.infernal-defense.invader-entry";

        public const string RouteJunctionRoleId = "realmraiders.infernal-defense.route-junction";

        public const string HellhoundEncounterRoleId = StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId;

        public const string FlameTrapHazardRoleId = StarterInfernalRaidPacingCatalogue.FlameTrapContentId;

        public const string InfernalBrutePossessionRoleId = StarterInfernalRaidPacingCatalogue.InfernalBruteArchetypeId;

        public const string InfernalHeartObjectiveRoleId = StarterInfernalRaidPacingCatalogue.InfernalHeartContentId;

        public static RealmLayoutGraph AshenSpur { get; } = CreateAshenSpur();

        public static RealmLayoutGraph CinderFork { get; } = CreateCinderFork();

        public static RealmLayoutGraph EmberCircuit { get; } = CreateEmberCircuit();

        public static IReadOnlyList<RealmLayoutGraph> All { get; } =
            Array.AsReadOnly(new[]
            {
                AshenSpur,
                CinderFork,
                EmberCircuit
            });

        private static RealmLayoutGraph CreateAshenSpur()
        {
            return new RealmLayoutGraph(
                AshenSpurId,
                "Ashen Spur",
                new RealmLayoutGraphNode[]
                {
                    Node("ashen.entry", InvaderEntryRoleId, RealmLayoutGraphNodeKind.Entry, 0f, -36f),
                    Node("ashen.junction", RouteJunctionRoleId, RealmLayoutGraphNodeKind.Landmark, 0f, -20f),
                    Node("ashen.hellhound", HellhoundEncounterRoleId, RealmLayoutGraphNodeKind.Encounter, -18f, -4f),
                    Node("ashen.flame", FlameTrapHazardRoleId, RealmLayoutGraphNodeKind.Encounter, 0f, 8f),
                    Node("ashen.brute", InfernalBrutePossessionRoleId, RealmLayoutGraphNodeKind.Encounter, 16f, 24f),
                    Node("ashen.heart", InfernalHeartObjectiveRoleId, RealmLayoutGraphNodeKind.Objective, 16f, 42f)
                },
                new RealmLayoutGraphEdge[]
                {
                    Edge("ashen.entry-junction", "ashen.entry", "ashen.junction", true, 7f),
                    Edge("ashen.junction-hellhound", "ashen.junction", "ashen.hellhound", false, 6f),
                    Edge("ashen.junction-flame", "ashen.junction", "ashen.flame", true, 7f),
                    Edge("ashen.flame-brute", "ashen.flame", "ashen.brute", true, 7f),
                    Edge("ashen.brute-heart", "ashen.brute", "ashen.heart", true, 7f)
                },
                new RealmLayoutGraphLandmark[]
                {
                    Landmark("ashen.junction.crucible", "realmraiders.infernal-defense.ash-crucible", "ashen.junction"),
                    Landmark("ashen.heart.altar", "realmraiders.infernal-defense.heart-altar", "ashen.heart")
                },
                new RealmLayoutGraphExpansionSocket[]
                {
                    Socket("ashen.west-vent", "ashen.hellhound", -30f, 6f),
                    Socket("ashen.east-vent", "ashen.brute", 30f, 24f)
                });
        }

        private static RealmLayoutGraph CreateCinderFork()
        {
            return new RealmLayoutGraph(
                CinderForkId,
                "Cinder Fork",
                new RealmLayoutGraphNode[]
                {
                    Node("cinder.entry", InvaderEntryRoleId, RealmLayoutGraphNodeKind.Entry, -28f, -36f),
                    Node("cinder.junction", RouteJunctionRoleId, RealmLayoutGraphNodeKind.Landmark, -12f, -20f),
                    Node("cinder.hellhound", HellhoundEncounterRoleId, RealmLayoutGraphNodeKind.Encounter, -28f, 0f),
                    Node("cinder.flame", FlameTrapHazardRoleId, RealmLayoutGraphNodeKind.Encounter, 4f, -2f),
                    Node("cinder.brute", InfernalBrutePossessionRoleId, RealmLayoutGraphNodeKind.Encounter, 18f, 18f),
                    Node("cinder.heart", InfernalHeartObjectiveRoleId, RealmLayoutGraphNodeKind.Objective, 18f, 38f)
                },
                new RealmLayoutGraphEdge[]
                {
                    Edge("cinder.entry-junction", "cinder.entry", "cinder.junction", true, 7f),
                    Edge("cinder.junction-hellhound", "cinder.junction", "cinder.hellhound", true, 7f),
                    Edge("cinder.junction-flame", "cinder.junction", "cinder.flame", false, 6f),
                    Edge("cinder.hellhound-brute", "cinder.hellhound", "cinder.brute", true, 7f),
                    Edge("cinder.flame-brute", "cinder.flame", "cinder.brute", false, 6f),
                    Edge("cinder.brute-heart", "cinder.brute", "cinder.heart", true, 7f)
                },
                new RealmLayoutGraphLandmark[]
                {
                    Landmark("cinder.junction.fork", "realmraiders.infernal-defense.cinder-fork", "cinder.junction"),
                    Landmark("cinder.heart.altar", "realmraiders.infernal-defense.heart-altar", "cinder.heart")
                },
                new RealmLayoutGraphExpansionSocket[]
                {
                    Socket("cinder.north-vent", "cinder.hellhound", -30f, 14f),
                    Socket("cinder.east-vent", "cinder.brute", 32f, 18f)
                });
        }

        private static RealmLayoutGraph CreateEmberCircuit()
        {
            return new RealmLayoutGraph(
                EmberCircuitId,
                "Ember Circuit",
                new RealmLayoutGraphNode[]
                {
                    Node("ember.entry", InvaderEntryRoleId, RealmLayoutGraphNodeKind.Entry, 0f, -40f),
                    Node("ember.junction", RouteJunctionRoleId, RealmLayoutGraphNodeKind.Landmark, 0f, -22f),
                    Node("ember.hellhound", HellhoundEncounterRoleId, RealmLayoutGraphNodeKind.Encounter, -22f, -4f),
                    Node("ember.flame", FlameTrapHazardRoleId, RealmLayoutGraphNodeKind.Encounter, 0f, 12f),
                    Node("ember.brute", InfernalBrutePossessionRoleId, RealmLayoutGraphNodeKind.Encounter, 22f, 28f),
                    Node("ember.heart", InfernalHeartObjectiveRoleId, RealmLayoutGraphNodeKind.Objective, 22f, 48f)
                },
                new RealmLayoutGraphEdge[]
                {
                    Edge("ember.entry-junction", "ember.entry", "ember.junction", true, 7f),
                    Edge("ember.junction-hellhound", "ember.junction", "ember.hellhound", true, 7f),
                    Edge("ember.hellhound-flame", "ember.hellhound", "ember.flame", false, 6f),
                    Edge("ember.flame-brute", "ember.flame", "ember.brute", true, 7f),
                    Edge("ember.junction-brute", "ember.junction", "ember.brute", true, 7f),
                    Edge("ember.brute-heart", "ember.brute", "ember.heart", true, 7f)
                },
                new RealmLayoutGraphLandmark[]
                {
                    Landmark("ember.junction.brazier", "realmraiders.infernal-defense.circuit-brazier", "ember.junction"),
                    Landmark("ember.heart.altar", "realmraiders.infernal-defense.heart-altar", "ember.heart")
                },
                new RealmLayoutGraphExpansionSocket[]
                {
                    Socket("ember.west-vent", "ember.hellhound", -34f, 8f),
                    Socket("ember.south-vent", "ember.junction", -12f, -34f),
                    Socket("ember.east-vent", "ember.brute", 36f, 28f)
                });
        }

        private static RealmLayoutGraphNode Node(
            string nodeId,
            string gameplayRoleId,
            RealmLayoutGraphNodeKind kind,
            float x,
            float z)
        {
            return new RealmLayoutGraphNode(nodeId, gameplayRoleId, kind, x, z);
        }

        private static RealmLayoutGraphEdge Edge(
            string edgeId,
            string fromNodeId,
            string toNodeId,
            bool isActivePathSafe,
            float floorPathWidth)
        {
            return new RealmLayoutGraphEdge(
                edgeId,
                fromNodeId,
                toNodeId,
                isActivePathSafe,
                floorPathWidth);
        }

        private static RealmLayoutGraphLandmark Landmark(
            string landmarkId,
            string presentationRoleId,
            string nodeId)
        {
            return new RealmLayoutGraphLandmark(landmarkId, presentationRoleId, nodeId);
        }

        private static RealmLayoutGraphExpansionSocket Socket(
            string socketId,
            string nodeId,
            float x,
            float z)
        {
            return new RealmLayoutGraphExpansionSocket(socketId, nodeId, x, z);
        }
    }

    /// <summary>Deterministic selection only; Core owns seeds and any persistence.</summary>
    public static class StarterInfernalDefenseLayoutSelector
    {
        public static InfernalDefenseLayoutSelectionResult Select(
            int seed,
            string previousLayoutId)
        {
            if (!string.IsNullOrEmpty(previousLayoutId)
                && !ContainsLayoutId(previousLayoutId))
            {
                return new InfernalDefenseLayoutSelectionResult(
                    InfernalDefenseLayoutSelectionStatus.PreviousLayoutIdInvalid,
                    null);
            }

            if (!CatalogueIsValid())
            {
                return new InfernalDefenseLayoutSelectionResult(
                    InfernalDefenseLayoutSelectionStatus.CatalogueInvalid,
                    null);
            }

            var index = (int)((uint)seed % (uint)StarterInfernalDefenseLayouts.All.Count);
            var selected = StarterInfernalDefenseLayouts.All[index];
            if (StarterInfernalDefenseLayouts.All.Count > 1
                && string.Equals(selected.LayoutId, previousLayoutId, StringComparison.Ordinal))
            {
                selected = StarterInfernalDefenseLayouts.All[
                    (index + 1) % StarterInfernalDefenseLayouts.All.Count];
            }

            return new InfernalDefenseLayoutSelectionResult(
                InfernalDefenseLayoutSelectionStatus.Selected,
                selected);
        }

        internal static bool CatalogueIsValid()
        {
            return InfernalDefenseLayoutValidator.ValidateCatalogue(
                StarterInfernalDefenseLayouts.All).IsValid;
        }

        private static bool ContainsLayoutId(string layoutId)
        {
            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                if (string.Equals(layout.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>Exact cached-ID lookup without selection, persistence, or reseeding.</summary>
    public static class StarterInfernalDefenseLayoutResolver
    {
        public static InfernalDefenseLayoutResolveResult ResolveExact(string layoutId)
        {
            if (!HasStableId(layoutId))
            {
                return new InfernalDefenseLayoutResolveResult(
                    InfernalDefenseLayoutResolveStatus.LayoutIdInvalid,
                    null);
            }

            if (!StarterInfernalDefenseLayoutSelector.CatalogueIsValid())
            {
                return new InfernalDefenseLayoutResolveResult(
                    InfernalDefenseLayoutResolveStatus.CatalogueInvalid,
                    null);
            }

            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                if (string.Equals(layout.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return new InfernalDefenseLayoutResolveResult(
                        InfernalDefenseLayoutResolveStatus.Resolved,
                        layout);
                }
            }

            return new InfernalDefenseLayoutResolveResult(
                InfernalDefenseLayoutResolveStatus.LayoutIdInvalid,
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
