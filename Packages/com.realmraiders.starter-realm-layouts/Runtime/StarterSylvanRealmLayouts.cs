using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.StarterRealmLayouts
{
    public enum RealmLayoutNodeKind
    {
        Start,
        Encounter,
        Landmark,
        Core
    }

    /// <summary>
    /// Closed Core adapter vocabulary for existing Sylvan node concepts. These are
    /// materialization facts only; they do not create encounters or grant rewards.
    /// </summary>
    public enum SylvanRealmNodeMaterializationRole
    {
        Unknown,
        PortalStart,
        WolfGroveEncounter,
        EntGroveEncounter,
        MoonwellRecovery,
        LandmarkJunction,
        HeartTreeObjective
    }

    /// <summary>Closed Core adapter vocabulary for existing Sylvan landmark presentation.</summary>
    public enum SylvanLandmarkVisualRole
    {
        Unknown,
        NodeCanopy,
        SylvanHeartTree
    }

    public enum RealmLayoutValidationIssue
    {
        RecipeMissing,
        LayoutIdInvalid,
        DisplayNameInvalid,
        NodeCardinalityInvalid,
        NodeMissing,
        NodeIdInvalid,
        NodeIdDuplicate,
        NodeContentIdInvalid,
        NodeKindInvalid,
        NodeMaterializationRoleInvalid,
        NodeCoordinateInvalid,
        NodeSpacingInvalid,
        StartNodeCardinalityInvalid,
        CoreNodeCardinalityInvalid,
        EdgeCardinalityInvalid,
        EdgeMissing,
        EdgeIdInvalid,
        EdgeIdDuplicate,
        EdgeEndpointInvalid,
        EdgeSelfReferenceInvalid,
        EdgeDuplicate,
        EdgeFloorPathWidthInvalid,
        LandmarkCardinalityInvalid,
        LandmarkMissing,
        LandmarkIdInvalid,
        LandmarkIdDuplicate,
        LandmarkTypeInvalid,
        LandmarkVisualRoleInvalid,
        LandmarkNodeInvalid,
        ExpansionSocketCardinalityInvalid,
        ExpansionSocketMissing,
        ExpansionSocketIdInvalid,
        ExpansionSocketIdDuplicate,
        ExpansionSocketNodeInvalid,
        ExpansionSocketCoordinateInvalid,
        LayoutDisconnected,
        ActivePathUnsafe
    }

    public enum RealmLayoutSelectionStatus
    {
        Selected,
        PreviousLayoutIdInvalid,
        CatalogueInvalid
    }

    /// <summary>Immutable planar fact with no world or scene authority.</summary>
    public sealed class RealmLayoutNode
    {
        public RealmLayoutNode(
            string nodeId,
            string contentId,
            RealmLayoutNodeKind kind,
            SylvanRealmNodeMaterializationRole materializationRole,
            float x,
            float z)
        {
            NodeId = nodeId;
            ContentId = contentId;
            Kind = kind;
            MaterializationRole = materializationRole;
            X = x;
            Z = z;
        }

        public string NodeId { get; }

        public string ContentId { get; }

        public RealmLayoutNodeKind Kind { get; }

        public SylvanRealmNodeMaterializationRole MaterializationRole { get; }

        public float X { get; }

        public float Z { get; }
    }

    /// <summary>
    /// An authored undirected connection; Core materializes each edge once, while
    /// safe edges form the active route fact.
    /// </summary>
    public sealed class RealmLayoutEdge
    {
        public RealmLayoutEdge(
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

    /// <summary>A landmark is attached to one stable recipe node rather than a scene object.</summary>
    public sealed class RealmLayoutLandmark
    {
        public RealmLayoutLandmark(
            string landmarkId,
            string landmarkTypeId,
            string nodeId,
            SylvanLandmarkVisualRole visualRole)
        {
            LandmarkId = landmarkId;
            LandmarkTypeId = landmarkTypeId;
            NodeId = nodeId;
            VisualRole = visualRole;
        }

        public string LandmarkId { get; }

        public string LandmarkTypeId { get; }

        public string NodeId { get; }

        public SylvanLandmarkVisualRole VisualRole { get; }
    }

    /// <summary>An authored future branch anchor, with no automatic expansion behavior.</summary>
    public sealed class RealmLayoutExpansionSocket
    {
        public RealmLayoutExpansionSocket(
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

    /// <summary>One immutable, caller-materialized starter realm topology recipe.</summary>
    public sealed class RealmLayoutRecipe
    {
        public RealmLayoutRecipe(
            string layoutId,
            string displayName,
            IReadOnlyList<RealmLayoutNode> nodes,
            IReadOnlyList<RealmLayoutEdge> edges,
            IReadOnlyList<RealmLayoutLandmark> landmarks,
            IReadOnlyList<RealmLayoutExpansionSocket> expansionSockets)
        {
            LayoutId = layoutId;
            DisplayName = displayName;
            Nodes = Snapshot(nodes);
            Edges = Snapshot(edges);
            Landmarks = Snapshot(landmarks);
            ExpansionSockets = Snapshot(expansionSockets);
        }

        public string LayoutId { get; }

        public string DisplayName { get; }

        public IReadOnlyList<RealmLayoutNode> Nodes { get; }

        public IReadOnlyList<RealmLayoutEdge> Edges { get; }

        public IReadOnlyList<RealmLayoutLandmark> Landmarks { get; }

        public IReadOnlyList<RealmLayoutExpansionSocket> ExpansionSockets { get; }

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

    /// <summary>Ordered fail-closed validator evidence for one layout recipe.</summary>
    public sealed class RealmLayoutValidationResult
    {
        internal RealmLayoutValidationResult(
            IReadOnlyList<RealmLayoutValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<RealmLayoutValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<RealmLayoutValidationIssue> Snapshot(
            IReadOnlyList<RealmLayoutValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<RealmLayoutValidationIssue>());
            }

            var copy = new RealmLayoutValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>Fail-closed selection result containing one cached recipe only on success.</summary>
    public sealed class RealmLayoutSelectionResult
    {
        internal RealmLayoutSelectionResult(
            RealmLayoutSelectionStatus status,
            RealmLayoutRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public RealmLayoutSelectionStatus Status { get; }

        public RealmLayoutRecipe Recipe { get; }

        public bool HasRecipe
        {
            get
            {
                return Status == RealmLayoutSelectionStatus.Selected && Recipe != null;
            }
        }
    }

    /// <summary>
    /// Pure validation of immutable facts. It does not select, persist, materialize,
    /// spawn, reward, or otherwise take gameplay authority.
    /// </summary>
    public static class RealmLayoutRecipeValidator
    {
        public const float MaximumAbsoluteCoordinate = 64f;

        public const float MinimumNodeSpacing = 4f;

        public const float MinimumFloorPathWidth = 6f;

        public const float MaximumFloorPathWidth = 8f;

        public static RealmLayoutValidationResult Validate(RealmLayoutRecipe recipe)
        {
            var issues = new List<RealmLayoutValidationIssue>();
            if (recipe == null)
            {
                AddIssue(issues, RealmLayoutValidationIssue.RecipeMissing);
                return new RealmLayoutValidationResult(issues);
            }

            if (!HasStableId(recipe.LayoutId))
            {
                AddIssue(issues, RealmLayoutValidationIssue.LayoutIdInvalid);
            }

            if (string.IsNullOrWhiteSpace(recipe.DisplayName))
            {
                AddIssue(issues, RealmLayoutValidationIssue.DisplayNameInvalid);
            }

            var nodeIds = ValidateNodes(recipe.Nodes, issues, out var startNodeId, out var coreNodeId);
            ValidateEdges(recipe.Edges, nodeIds, issues);
            ValidateLandmarks(recipe.Landmarks, nodeIds, issues);
            ValidateExpansionSockets(recipe.ExpansionSockets, nodeIds, issues);
            ValidateReachability(recipe, nodeIds, startNodeId, coreNodeId, issues);

            return new RealmLayoutValidationResult(issues);
        }

        private static HashSet<string> ValidateNodes(
            IReadOnlyList<RealmLayoutNode> nodes,
            ICollection<RealmLayoutValidationIssue> issues,
            out string startNodeId,
            out string coreNodeId)
        {
            startNodeId = null;
            coreNodeId = null;
            var nodeIds = new HashSet<string>(StringComparer.Ordinal);
            if (nodes == null || nodes.Count < 3)
            {
                AddIssue(issues, RealmLayoutValidationIssue.NodeCardinalityInvalid);
            }

            var startCount = 0;
            var coreCount = 0;
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node == null)
                    {
                        AddIssue(issues, RealmLayoutValidationIssue.NodeMissing);
                        continue;
                    }

                    if (!HasStableId(node.NodeId))
                    {
                        AddIssue(issues, RealmLayoutValidationIssue.NodeIdInvalid);
                    }
                    else if (!nodeIds.Add(node.NodeId))
                    {
                        AddIssue(issues, RealmLayoutValidationIssue.NodeIdDuplicate);
                    }

                    if (!HasStableId(node.ContentId))
                    {
                        AddIssue(issues, RealmLayoutValidationIssue.NodeContentIdInvalid);
                    }

                    if (node.Kind < RealmLayoutNodeKind.Start || node.Kind > RealmLayoutNodeKind.Core)
                    {
                        AddIssue(issues, RealmLayoutValidationIssue.NodeKindInvalid);
                    }

                    if (!IsMaterializationRoleValid(node.Kind, node.MaterializationRole))
                    {
                        AddIssue(issues, RealmLayoutValidationIssue.NodeMaterializationRoleInvalid);
                    }

                    if (!IsCoordinateValid(node.X) || !IsCoordinateValid(node.Z))
                    {
                        AddIssue(issues, RealmLayoutValidationIssue.NodeCoordinateInvalid);
                    }

                    if (node.Kind == RealmLayoutNodeKind.Start)
                    {
                        startCount++;
                        startNodeId = node.NodeId;
                    }

                    if (node.Kind == RealmLayoutNodeKind.Core)
                    {
                        coreCount++;
                        coreNodeId = node.NodeId;
                    }
                }
            }

            ValidateNodeSpacing(nodes, issues);

            if (startCount != 1)
            {
                AddIssue(issues, RealmLayoutValidationIssue.StartNodeCardinalityInvalid);
            }

            if (coreCount != 1)
            {
                AddIssue(issues, RealmLayoutValidationIssue.CoreNodeCardinalityInvalid);
            }

            return nodeIds;
        }

        private static void ValidateNodeSpacing(
            IReadOnlyList<RealmLayoutNode> nodes,
            ICollection<RealmLayoutValidationIssue> issues)
        {
            if (nodes == null)
            {
                return;
            }

            var minimumDistanceSquared = MinimumNodeSpacing * MinimumNodeSpacing;
            for (var first = 0; first < nodes.Count; first++)
            {
                var firstNode = nodes[first];
                if (firstNode == null
                    || !IsCoordinateValid(firstNode.X)
                    || !IsCoordinateValid(firstNode.Z))
                {
                    continue;
                }

                for (var second = first + 1; second < nodes.Count; second++)
                {
                    var secondNode = nodes[second];
                    if (secondNode == null
                        || !IsCoordinateValid(secondNode.X)
                        || !IsCoordinateValid(secondNode.Z))
                    {
                        continue;
                    }

                    var deltaX = firstNode.X - secondNode.X;
                    var deltaZ = firstNode.Z - secondNode.Z;
                    if (deltaX * deltaX + deltaZ * deltaZ < minimumDistanceSquared)
                    {
                        AddIssue(issues, RealmLayoutValidationIssue.NodeSpacingInvalid);
                        return;
                    }
                }
            }
        }

        private static void ValidateEdges(
            IReadOnlyList<RealmLayoutEdge> edges,
            ISet<string> nodeIds,
            ICollection<RealmLayoutValidationIssue> issues)
        {
            if (edges == null || edges.Count == 0)
            {
                AddIssue(issues, RealmLayoutValidationIssue.EdgeCardinalityInvalid);
                return;
            }

            var edgeIds = new HashSet<string>(StringComparer.Ordinal);
            var endpoints = new HashSet<string>(StringComparer.Ordinal);
            foreach (var edge in edges)
            {
                if (edge == null)
                {
                    AddIssue(issues, RealmLayoutValidationIssue.EdgeMissing);
                    continue;
                }

                if (!HasStableId(edge.EdgeId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.EdgeIdInvalid);
                }
                else if (!edgeIds.Add(edge.EdgeId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.EdgeIdDuplicate);
                }

                if (!HasStableId(edge.FromNodeId)
                    || !HasStableId(edge.ToNodeId)
                    || !nodeIds.Contains(edge.FromNodeId)
                    || !nodeIds.Contains(edge.ToNodeId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.EdgeEndpointInvalid);
                    continue;
                }

                if (string.Equals(edge.FromNodeId, edge.ToNodeId, StringComparison.Ordinal))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.EdgeSelfReferenceInvalid);
                    continue;
                }

                if (!IsFloorPathWidthValid(edge.FloorPathWidth))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.EdgeFloorPathWidthInvalid);
                }

                if (!endpoints.Add(UndirectedEdgeKey(edge.FromNodeId, edge.ToNodeId)))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.EdgeDuplicate);
                }
            }
        }

        private static void ValidateLandmarks(
            IReadOnlyList<RealmLayoutLandmark> landmarks,
            ISet<string> nodeIds,
            ICollection<RealmLayoutValidationIssue> issues)
        {
            if (landmarks == null || landmarks.Count == 0)
            {
                AddIssue(issues, RealmLayoutValidationIssue.LandmarkCardinalityInvalid);
                return;
            }

            var landmarkIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var landmark in landmarks)
            {
                if (landmark == null)
                {
                    AddIssue(issues, RealmLayoutValidationIssue.LandmarkMissing);
                    continue;
                }

                if (!HasStableId(landmark.LandmarkId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.LandmarkIdInvalid);
                }
                else if (!landmarkIds.Add(landmark.LandmarkId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.LandmarkIdDuplicate);
                }

                if (!HasStableId(landmark.LandmarkTypeId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.LandmarkTypeInvalid);
                }

                if (!IsLandmarkVisualRoleValid(landmark.VisualRole))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.LandmarkVisualRoleInvalid);
                }

                if (!HasStableId(landmark.NodeId) || !nodeIds.Contains(landmark.NodeId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.LandmarkNodeInvalid);
                }
            }
        }

        private static void ValidateExpansionSockets(
            IReadOnlyList<RealmLayoutExpansionSocket> expansionSockets,
            ISet<string> nodeIds,
            ICollection<RealmLayoutValidationIssue> issues)
        {
            if (expansionSockets == null || expansionSockets.Count == 0)
            {
                AddIssue(issues, RealmLayoutValidationIssue.ExpansionSocketCardinalityInvalid);
                return;
            }

            var socketIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var socket in expansionSockets)
            {
                if (socket == null)
                {
                    AddIssue(issues, RealmLayoutValidationIssue.ExpansionSocketMissing);
                    continue;
                }

                if (!HasStableId(socket.SocketId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.ExpansionSocketIdInvalid);
                }
                else if (!socketIds.Add(socket.SocketId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.ExpansionSocketIdDuplicate);
                }

                if (!HasStableId(socket.NodeId) || !nodeIds.Contains(socket.NodeId))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.ExpansionSocketNodeInvalid);
                }

                if (!IsCoordinateValid(socket.X) || !IsCoordinateValid(socket.Z))
                {
                    AddIssue(issues, RealmLayoutValidationIssue.ExpansionSocketCoordinateInvalid);
                }
            }
        }

        private static void ValidateReachability(
            RealmLayoutRecipe recipe,
            ISet<string> nodeIds,
            string startNodeId,
            string coreNodeId,
            ICollection<RealmLayoutValidationIssue> issues)
        {
            if (nodeIds.Count == 0 || !HasStableId(startNodeId) || !HasStableId(coreNodeId))
            {
                AddIssue(issues, RealmLayoutValidationIssue.LayoutDisconnected);
                AddIssue(issues, RealmLayoutValidationIssue.ActivePathUnsafe);
                return;
            }

            var reachable = ReachableNodes(recipe.Edges, startNodeId, false);
            if (reachable.Count != nodeIds.Count)
            {
                AddIssue(issues, RealmLayoutValidationIssue.LayoutDisconnected);
            }

            var safeReachable = ReachableNodes(recipe.Edges, startNodeId, true);
            if (!safeReachable.Contains(coreNodeId))
            {
                AddIssue(issues, RealmLayoutValidationIssue.ActivePathUnsafe);
            }
        }

        private static ISet<string> ReachableNodes(
            IReadOnlyList<RealmLayoutEdge> edges,
            string startNodeId,
            bool safeOnly)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal);
            if (edges == null || !HasStableId(startNodeId))
            {
                return visited;
            }

            var pending = new Queue<string>();
            visited.Add(startNodeId);
            pending.Enqueue(startNodeId);
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                foreach (var edge in edges)
                {
                    if (edge == null || safeOnly && !edge.IsActivePathSafe)
                    {
                        continue;
                    }

                    var next = string.Equals(edge.FromNodeId, current, StringComparison.Ordinal)
                        ? edge.ToNodeId
                        : string.Equals(edge.ToNodeId, current, StringComparison.Ordinal)
                            ? edge.FromNodeId
                            : null;
                    if (HasStableId(next) && visited.Add(next))
                    {
                        pending.Enqueue(next);
                    }
                }
            }

            return visited;
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || !IsAsciiAlphaNumeric(value[0])
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

        private static bool IsCoordinateValid(float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value)
                && Math.Abs(value) <= MaximumAbsoluteCoordinate;
        }

        private static bool IsMaterializationRoleValid(
            RealmLayoutNodeKind kind,
            SylvanRealmNodeMaterializationRole role)
        {
            if (role <= SylvanRealmNodeMaterializationRole.Unknown
                || role > SylvanRealmNodeMaterializationRole.HeartTreeObjective)
            {
                return false;
            }

            if (kind == RealmLayoutNodeKind.Start)
            {
                return role == SylvanRealmNodeMaterializationRole.PortalStart;
            }

            if (kind == RealmLayoutNodeKind.Core)
            {
                return role == SylvanRealmNodeMaterializationRole.HeartTreeObjective;
            }

            if (kind == RealmLayoutNodeKind.Landmark)
            {
                return role == SylvanRealmNodeMaterializationRole.LandmarkJunction;
            }

            return kind == RealmLayoutNodeKind.Encounter
                && (role == SylvanRealmNodeMaterializationRole.WolfGroveEncounter
                    || role == SylvanRealmNodeMaterializationRole.EntGroveEncounter
                    || role == SylvanRealmNodeMaterializationRole.MoonwellRecovery);
        }

        private static bool IsFloorPathWidthValid(float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value)
                && value >= MinimumFloorPathWidth
                && value <= MaximumFloorPathWidth;
        }

        private static bool IsLandmarkVisualRoleValid(SylvanLandmarkVisualRole role)
        {
            return role == SylvanLandmarkVisualRole.NodeCanopy
                || role == SylvanLandmarkVisualRole.SylvanHeartTree;
        }

        private static string UndirectedEdgeKey(string firstNodeId, string secondNodeId)
        {
            return string.CompareOrdinal(firstNodeId, secondNodeId) < 0
                ? firstNodeId + "\u001f" + secondNodeId
                : secondNodeId + "\u001f" + firstNodeId;
        }

        private static void AddIssue(
            ICollection<RealmLayoutValidationIssue> issues,
            RealmLayoutValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }

    /// <summary>Three authored, cached Sylvan layouts. This catalogue never selects one.</summary>
    public static class StarterSylvanRealmLayouts
    {
        public const string AncientCrossroadsId = "realmraiders.sylvan-layout.ancient-crossroads";

        public const string ForkedCanopyId = "realmraiders.sylvan-layout.forked-canopy";

        public const string SerpentRootsId = "realmraiders.sylvan-layout.serpent-roots";

        public static RealmLayoutRecipe AncientCrossroads { get; } = CreateAncientCrossroads();

        public static RealmLayoutRecipe ForkedCanopy { get; } = CreateForkedCanopy();

        public static RealmLayoutRecipe SerpentRoots { get; } = CreateSerpentRoots();

        public static IReadOnlyList<RealmLayoutRecipe> All { get; } =
            Array.AsReadOnly(new[]
            {
                AncientCrossroads,
                ForkedCanopy,
                SerpentRoots
            });

        private static RealmLayoutRecipe CreateAncientCrossroads()
        {
            return new RealmLayoutRecipe(
                AncientCrossroadsId,
                "Ancient Crossroads",
                new RealmLayoutNode[]
                {
                    Node("ancient.start", "realmraiders.node.start", RealmLayoutNodeKind.Start, SylvanRealmNodeMaterializationRole.PortalStart, 0f, -24f),
                    Node("ancient.crossroads", "realmraiders.node.crossroads", RealmLayoutNodeKind.Landmark, SylvanRealmNodeMaterializationRole.LandmarkJunction, 0f, -8f),
                    Node("ancient.wolf-grove", "realmraiders.node.wolf-grove", RealmLayoutNodeKind.Encounter, SylvanRealmNodeMaterializationRole.WolfGroveEncounter, -14f, 4f),
                    Node("ancient.moonwell", "realmraiders.node.moonwell", RealmLayoutNodeKind.Encounter, SylvanRealmNodeMaterializationRole.MoonwellRecovery, 14f, 4f),
                    Node("ancient.core", "realmraiders.node.heart-tree", RealmLayoutNodeKind.Core, SylvanRealmNodeMaterializationRole.HeartTreeObjective, 0f, 20f)
                },
                new RealmLayoutEdge[]
                {
                    Edge("ancient.start-crossroads", "ancient.start", "ancient.crossroads", true, 7f),
                    Edge("ancient.crossroads-wolf", "ancient.crossroads", "ancient.wolf-grove", false, 6f),
                    Edge("ancient.crossroads-moonwell", "ancient.crossroads", "ancient.moonwell", false, 6f),
                    Edge("ancient.crossroads-core", "ancient.crossroads", "ancient.core", true, 7f)
                },
                new RealmLayoutLandmark[]
                {
                    new RealmLayoutLandmark("ancient.crossroads.great-oak", "realmraiders.landmark.great-oak", "ancient.crossroads", SylvanLandmarkVisualRole.NodeCanopy),
                    new RealmLayoutLandmark("ancient.core.heart-tree", "realmraiders.landmark.heart-tree", "ancient.core", SylvanLandmarkVisualRole.SylvanHeartTree)
                },
                new RealmLayoutExpansionSocket[]
                {
                    new RealmLayoutExpansionSocket("ancient.west-bough", "ancient.wolf-grove", -24f, 8f),
                    new RealmLayoutExpansionSocket("ancient.east-bough", "ancient.moonwell", 24f, 8f)
                });
        }

        private static RealmLayoutRecipe CreateForkedCanopy()
        {
            return new RealmLayoutRecipe(
                ForkedCanopyId,
                "Forked Canopy",
                new RealmLayoutNode[]
                {
                    Node("canopy.start", "realmraiders.node.start", RealmLayoutNodeKind.Start, SylvanRealmNodeMaterializationRole.PortalStart, -16f, -24f),
                    Node("canopy.fork", "realmraiders.node.canopy-fork", RealmLayoutNodeKind.Landmark, SylvanRealmNodeMaterializationRole.LandmarkJunction, -8f, -8f),
                    Node("canopy.high-path", "realmraiders.node.high-canopy", RealmLayoutNodeKind.Encounter, SylvanRealmNodeMaterializationRole.WolfGroveEncounter, -18f, 8f),
                    Node("canopy.low-path", "realmraiders.node.root-basin", RealmLayoutNodeKind.Encounter, SylvanRealmNodeMaterializationRole.EntGroveEncounter, 8f, 4f),
                    Node("canopy.core", "realmraiders.node.heart-tree", RealmLayoutNodeKind.Core, SylvanRealmNodeMaterializationRole.HeartTreeObjective, 12f, 22f)
                },
                new RealmLayoutEdge[]
                {
                    Edge("canopy.start-fork", "canopy.start", "canopy.fork", true, 7f),
                    Edge("canopy.fork-high", "canopy.fork", "canopy.high-path", false, 6f),
                    Edge("canopy.fork-low", "canopy.fork", "canopy.low-path", true, 7f),
                    Edge("canopy.high-core", "canopy.high-path", "canopy.core", false, 6f),
                    Edge("canopy.low-core", "canopy.low-path", "canopy.core", true, 7f)
                },
                new RealmLayoutLandmark[]
                {
                    new RealmLayoutLandmark("canopy.fork.watchtree", "realmraiders.landmark.watchtree", "canopy.fork", SylvanLandmarkVisualRole.NodeCanopy),
                    new RealmLayoutLandmark("canopy.core.heart-tree", "realmraiders.landmark.heart-tree", "canopy.core", SylvanLandmarkVisualRole.SylvanHeartTree)
                },
                new RealmLayoutExpansionSocket[]
                {
                    new RealmLayoutExpansionSocket("canopy.high-bough", "canopy.high-path", -30f, 16f),
                    new RealmLayoutExpansionSocket("canopy.root-run", "canopy.low-path", 20f, 6f)
                });
        }

        private static RealmLayoutRecipe CreateSerpentRoots()
        {
            return new RealmLayoutRecipe(
                SerpentRootsId,
                "Serpent Roots",
                new RealmLayoutNode[]
                {
                    Node("serpent.start", "realmraiders.node.start", RealmLayoutNodeKind.Start, SylvanRealmNodeMaterializationRole.PortalStart, -18f, -24f),
                    Node("serpent.east-turn", "realmraiders.node.root-turn", RealmLayoutNodeKind.Encounter, SylvanRealmNodeMaterializationRole.WolfGroveEncounter, 14f, -12f),
                    Node("serpent.west-turn", "realmraiders.node.root-turn", RealmLayoutNodeKind.Landmark, SylvanRealmNodeMaterializationRole.LandmarkJunction, -14f, 0f),
                    Node("serpent.guardian", "realmraiders.node.ent-grove", RealmLayoutNodeKind.Encounter, SylvanRealmNodeMaterializationRole.EntGroveEncounter, 14f, 12f),
                    Node("serpent.core", "realmraiders.node.heart-tree", RealmLayoutNodeKind.Core, SylvanRealmNodeMaterializationRole.HeartTreeObjective, -10f, 24f)
                },
                new RealmLayoutEdge[]
                {
                    Edge("serpent.start-east", "serpent.start", "serpent.east-turn", true, 7f),
                    Edge("serpent.east-west", "serpent.east-turn", "serpent.west-turn", true, 7f),
                    Edge("serpent.west-guardian", "serpent.west-turn", "serpent.guardian", true, 7f),
                    Edge("serpent.guardian-core", "serpent.guardian", "serpent.core", true, 7f)
                },
                new RealmLayoutLandmark[]
                {
                    new RealmLayoutLandmark("serpent.west-turn.root-arch", "realmraiders.landmark.root-arch", "serpent.west-turn", SylvanLandmarkVisualRole.NodeCanopy),
                    new RealmLayoutLandmark("serpent.core.heart-tree", "realmraiders.landmark.heart-tree", "serpent.core", SylvanLandmarkVisualRole.SylvanHeartTree)
                },
                new RealmLayoutExpansionSocket[]
                {
                    new RealmLayoutExpansionSocket("serpent.east-burrow", "serpent.east-turn", 28f, -8f),
                    new RealmLayoutExpansionSocket("serpent.north-root", "serpent.core", -24f, 30f)
                });
        }

        private static RealmLayoutNode Node(
            string nodeId,
            string contentId,
            RealmLayoutNodeKind kind,
            SylvanRealmNodeMaterializationRole materializationRole,
            float x,
            float z)
        {
            return new RealmLayoutNode(nodeId, contentId, kind, materializationRole, x, z);
        }

        private static RealmLayoutEdge Edge(
            string edgeId,
            string fromNodeId,
            string toNodeId,
            bool isActivePathSafe,
            float floorPathWidth)
        {
            return new RealmLayoutEdge(edgeId, fromNodeId, toNodeId, isActivePathSafe, floorPathWidth);
        }
    }

    /// <summary>
    /// Deterministic selector over the fixed catalogue. The supplied seed is a value,
    /// not runtime RNG state; Core owns where it came from and whether to persist it.
    /// </summary>
    public static class StarterSylvanRealmLayoutSelector
    {
        public static RealmLayoutSelectionResult Select(
            int seed,
            string previousLayoutId)
        {
            if (!string.IsNullOrEmpty(previousLayoutId) && !ContainsLayoutId(previousLayoutId))
            {
                return new RealmLayoutSelectionResult(
                    RealmLayoutSelectionStatus.PreviousLayoutIdInvalid,
                    null);
            }

            if (!CatalogueIsValid())
            {
                return new RealmLayoutSelectionResult(
                    RealmLayoutSelectionStatus.CatalogueInvalid,
                    null);
            }

            var index = (int)((uint)seed % (uint)StarterSylvanRealmLayouts.All.Count);
            var selected = StarterSylvanRealmLayouts.All[index];
            if (StarterSylvanRealmLayouts.All.Count > 1
                && string.Equals(selected.LayoutId, previousLayoutId, StringComparison.Ordinal))
            {
                selected = StarterSylvanRealmLayouts.All[
                    (index + 1) % StarterSylvanRealmLayouts.All.Count];
            }

            return new RealmLayoutSelectionResult(
                RealmLayoutSelectionStatus.Selected,
                selected);
        }

        private static bool ContainsLayoutId(string layoutId)
        {
            foreach (var recipe in StarterSylvanRealmLayouts.All)
            {
                if (string.Equals(recipe.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CatalogueIsValid()
        {
            if (StarterSylvanRealmLayouts.All.Count == 0)
            {
                return false;
            }

            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var recipe in StarterSylvanRealmLayouts.All)
            {
                if (recipe == null
                    || !RealmLayoutRecipeValidator.Validate(recipe).IsValid
                    || !layoutIds.Add(recipe.LayoutId))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
