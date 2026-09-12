using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.RealmLayoutContracts
{
    public enum RealmLayoutGraphNodeKind
    {
        Entry,
        Encounter,
        Landmark,
        Objective
    }

    public enum RealmLayoutGraphValidationIssue
    {
        GraphMissing,
        LayoutIdInvalid,
        DisplayNameInvalid,
        NodeCardinalityInvalid,
        NodeMissing,
        NodeIdInvalid,
        NodeIdDuplicate,
        NodeGameplayRoleIdInvalid,
        NodeKindInvalid,
        NodeCoordinateInvalid,
        NodeSpacingInvalid,
        EntryNodeCardinalityInvalid,
        ObjectiveNodeCardinalityInvalid,
        EdgeCardinalityInvalid,
        EdgeMissing,
        EdgeIdInvalid,
        EdgeIdDuplicate,
        EdgeEndpointInvalid,
        EdgeSelfReferenceInvalid,
        EdgeDuplicate,
        EdgeFloorPathWidthInvalid,
        NodeCorridorClearanceInvalid,
        CorridorClearanceInvalid,
        LandmarkMissing,
        LandmarkIdInvalid,
        LandmarkIdDuplicate,
        LandmarkPresentationRoleIdInvalid,
        LandmarkNodeInvalid,
        ExpansionSocketMissing,
        ExpansionSocketIdInvalid,
        ExpansionSocketIdDuplicate,
        ExpansionSocketNodeInvalid,
        ExpansionSocketCoordinateInvalid,
        LayoutDisconnected,
        ActivePathUnsafe
    }

    /// <summary>Immutable faction-neutral planar node facts with stable gameplay-role IDs.</summary>
    public sealed class RealmLayoutGraphNode
    {
        public RealmLayoutGraphNode(
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

    /// <summary>One authored undirected floor connection. Core materializes it once.</summary>
    public sealed class RealmLayoutGraphEdge
    {
        public RealmLayoutGraphEdge(
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

    /// <summary>Presentation role attached to a stable graph node, never a scene object.</summary>
    public sealed class RealmLayoutGraphLandmark
    {
        public RealmLayoutGraphLandmark(
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

    /// <summary>An authored expansion anchor with no automatic expansion behavior.</summary>
    public sealed class RealmLayoutGraphExpansionSocket
    {
        public RealmLayoutGraphExpansionSocket(
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

    /// <summary>Immutable graph facts. Role IDs are exact ordinal strings, not faction enums.</summary>
    public sealed class RealmLayoutGraph
    {
        public RealmLayoutGraph(
            string layoutId,
            string displayName,
            IReadOnlyList<RealmLayoutGraphNode> nodes,
            IReadOnlyList<RealmLayoutGraphEdge> edges,
            IReadOnlyList<RealmLayoutGraphLandmark> landmarks,
            IReadOnlyList<RealmLayoutGraphExpansionSocket> expansionSockets)
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

        public IReadOnlyList<RealmLayoutGraphNode> Nodes { get; }

        public IReadOnlyList<RealmLayoutGraphEdge> Edges { get; }

        public IReadOnlyList<RealmLayoutGraphLandmark> Landmarks { get; }

        public IReadOnlyList<RealmLayoutGraphExpansionSocket> ExpansionSockets { get; }

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

    public sealed class RealmLayoutGraphValidationResult
    {
        internal RealmLayoutGraphValidationResult(
            IReadOnlyList<RealmLayoutGraphValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<RealmLayoutGraphValidationIssue> Issues { get; }

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

    /// <summary>
    /// Structural, faction-neutral graph validation. It owns neither authored role
    /// vocabularies nor gameplay materialization, encounter, reward, or save rules.
    /// </summary>
    public static class RealmLayoutGraphValidator
    {
        public const float MaximumAbsoluteCoordinate = 64f;

        public const float NodeFootprintRadius = 3.25f;

        public const float MinimumRouteClearance = 0.25f;

        public const float MinimumNodeSpacing = NodeFootprintRadius * 2f
            + MinimumRouteClearance;

        public const float MinimumFloorPathWidth = 6f;

        public const float MaximumFloorPathWidth = 8f;

        public static RealmLayoutGraphValidationResult Validate(RealmLayoutGraph graph)
        {
            var issues = new List<RealmLayoutGraphValidationIssue>();
            if (graph == null)
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.GraphMissing);
                return new RealmLayoutGraphValidationResult(issues);
            }

            if (!HasStableId(graph.LayoutId))
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.LayoutIdInvalid);
            }

            if (string.IsNullOrWhiteSpace(graph.DisplayName))
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.DisplayNameInvalid);
            }

            var nodeIds = ValidateNodes(graph.Nodes, issues, out var entryNodeId, out var objectiveNodeId);
            ValidateEdges(graph.Edges, nodeIds, issues);
            ValidateRouteClearance(graph.Nodes, graph.Edges, issues);
            ValidateLandmarks(graph.Landmarks, nodeIds, issues);
            ValidateExpansionSockets(graph.ExpansionSockets, nodeIds, issues);
            ValidateReachability(graph.Edges, nodeIds, entryNodeId, objectiveNodeId, issues);

            return new RealmLayoutGraphValidationResult(issues);
        }

        private static HashSet<string> ValidateNodes(
            IReadOnlyList<RealmLayoutGraphNode> nodes,
            ICollection<RealmLayoutGraphValidationIssue> issues,
            out string entryNodeId,
            out string objectiveNodeId)
        {
            entryNodeId = null;
            objectiveNodeId = null;
            var nodeIds = new HashSet<string>(StringComparer.Ordinal);
            if (nodes == null || nodes.Count < 2)
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.NodeCardinalityInvalid);
            }

            var entryCount = 0;
            var objectiveCount = 0;
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node == null)
                    {
                        AddIssue(issues, RealmLayoutGraphValidationIssue.NodeMissing);
                        continue;
                    }

                    if (!HasStableId(node.NodeId))
                    {
                        AddIssue(issues, RealmLayoutGraphValidationIssue.NodeIdInvalid);
                    }
                    else if (!nodeIds.Add(node.NodeId))
                    {
                        AddIssue(issues, RealmLayoutGraphValidationIssue.NodeIdDuplicate);
                    }

                    if (!HasStableId(node.GameplayRoleId))
                    {
                        AddIssue(issues, RealmLayoutGraphValidationIssue.NodeGameplayRoleIdInvalid);
                    }

                    if (node.Kind < RealmLayoutGraphNodeKind.Entry
                        || node.Kind > RealmLayoutGraphNodeKind.Objective)
                    {
                        AddIssue(issues, RealmLayoutGraphValidationIssue.NodeKindInvalid);
                    }

                    if (!IsCoordinateValid(node.X) || !IsCoordinateValid(node.Z))
                    {
                        AddIssue(issues, RealmLayoutGraphValidationIssue.NodeCoordinateInvalid);
                    }

                    if (node.Kind == RealmLayoutGraphNodeKind.Entry)
                    {
                        entryCount++;
                        entryNodeId = node.NodeId;
                    }

                    if (node.Kind == RealmLayoutGraphNodeKind.Objective)
                    {
                        objectiveCount++;
                        objectiveNodeId = node.NodeId;
                    }
                }
            }

            ValidateNodeSpacing(nodes, issues);
            if (entryCount != 1)
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.EntryNodeCardinalityInvalid);
            }

            if (objectiveCount != 1)
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.ObjectiveNodeCardinalityInvalid);
            }

            return nodeIds;
        }

        private static void ValidateNodeSpacing(
            IReadOnlyList<RealmLayoutGraphNode> nodes,
            ICollection<RealmLayoutGraphValidationIssue> issues)
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

                    if (DistanceSquared(firstNode, secondNode) < minimumDistanceSquared)
                    {
                        AddIssue(issues, RealmLayoutGraphValidationIssue.NodeSpacingInvalid);
                        return;
                    }
                }
            }
        }

        private static void ValidateEdges(
            IReadOnlyList<RealmLayoutGraphEdge> edges,
            ISet<string> nodeIds,
            ICollection<RealmLayoutGraphValidationIssue> issues)
        {
            if (edges == null || edges.Count == 0)
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.EdgeCardinalityInvalid);
                return;
            }

            var edgeIds = new HashSet<string>(StringComparer.Ordinal);
            var endpoints = new HashSet<string>(StringComparer.Ordinal);
            foreach (var edge in edges)
            {
                if (edge == null)
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.EdgeMissing);
                    continue;
                }

                if (!HasStableId(edge.EdgeId))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.EdgeIdInvalid);
                }
                else if (!edgeIds.Add(edge.EdgeId))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.EdgeIdDuplicate);
                }

                if (!HasStableId(edge.FromNodeId)
                    || !HasStableId(edge.ToNodeId)
                    || !nodeIds.Contains(edge.FromNodeId)
                    || !nodeIds.Contains(edge.ToNodeId))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.EdgeEndpointInvalid);
                    continue;
                }

                if (string.Equals(edge.FromNodeId, edge.ToNodeId, StringComparison.Ordinal))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.EdgeSelfReferenceInvalid);
                    continue;
                }

                if (!IsFloorPathWidthValid(edge.FloorPathWidth))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.EdgeFloorPathWidthInvalid);
                }

                if (!endpoints.Add(UndirectedEdgeKey(edge.FromNodeId, edge.ToNodeId)))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.EdgeDuplicate);
                }
            }
        }

        private static void ValidateLandmarks(
            IReadOnlyList<RealmLayoutGraphLandmark> landmarks,
            ISet<string> nodeIds,
            ICollection<RealmLayoutGraphValidationIssue> issues)
        {
            if (landmarks == null || landmarks.Count == 0)
            {
                return;
            }

            var landmarkIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var landmark in landmarks)
            {
                if (landmark == null)
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.LandmarkMissing);
                    continue;
                }

                if (!HasStableId(landmark.LandmarkId))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.LandmarkIdInvalid);
                }
                else if (!landmarkIds.Add(landmark.LandmarkId))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.LandmarkIdDuplicate);
                }

                if (!HasStableId(landmark.PresentationRoleId))
                {
                    AddIssue(issues,
                        RealmLayoutGraphValidationIssue.LandmarkPresentationRoleIdInvalid);
                }

                if (!HasStableId(landmark.NodeId) || !nodeIds.Contains(landmark.NodeId))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.LandmarkNodeInvalid);
                }
            }
        }

        private static void ValidateExpansionSockets(
            IReadOnlyList<RealmLayoutGraphExpansionSocket> expansionSockets,
            ISet<string> nodeIds,
            ICollection<RealmLayoutGraphValidationIssue> issues)
        {
            if (expansionSockets == null)
            {
                return;
            }

            var socketIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var socket in expansionSockets)
            {
                if (socket == null)
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.ExpansionSocketMissing);
                    continue;
                }

                if (!HasStableId(socket.SocketId))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.ExpansionSocketIdInvalid);
                }
                else if (!socketIds.Add(socket.SocketId))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.ExpansionSocketIdDuplicate);
                }

                if (!HasStableId(socket.NodeId) || !nodeIds.Contains(socket.NodeId))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.ExpansionSocketNodeInvalid);
                }

                if (!IsCoordinateValid(socket.X) || !IsCoordinateValid(socket.Z))
                {
                    AddIssue(issues, RealmLayoutGraphValidationIssue.ExpansionSocketCoordinateInvalid);
                }
            }
        }

        private static void ValidateReachability(
            IReadOnlyList<RealmLayoutGraphEdge> edges,
            ISet<string> nodeIds,
            string entryNodeId,
            string objectiveNodeId,
            ICollection<RealmLayoutGraphValidationIssue> issues)
        {
            if (nodeIds.Count == 0
                || !HasStableId(entryNodeId)
                || !HasStableId(objectiveNodeId))
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.LayoutDisconnected);
                AddIssue(issues, RealmLayoutGraphValidationIssue.ActivePathUnsafe);
                return;
            }

            var reachable = ReachableNodes(edges, entryNodeId, false);
            if (reachable.Count != nodeIds.Count)
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.LayoutDisconnected);
            }

            var safeReachable = ReachableNodes(edges, entryNodeId, true);
            if (!safeReachable.Contains(objectiveNodeId))
            {
                AddIssue(issues, RealmLayoutGraphValidationIssue.ActivePathUnsafe);
            }
        }

        private static void ValidateRouteClearance(
            IReadOnlyList<RealmLayoutGraphNode> nodes,
            IReadOnlyList<RealmLayoutGraphEdge> edges,
            ICollection<RealmLayoutGraphValidationIssue> issues)
        {
            if (nodes == null || edges == null)
            {
                return;
            }

            var nodesById = new Dictionary<string, RealmLayoutGraphNode>(StringComparer.Ordinal);
            foreach (var node in nodes)
            {
                if (node != null
                    && HasStableId(node.NodeId)
                    && IsCoordinateValid(node.X)
                    && IsCoordinateValid(node.Z)
                    && !nodesById.ContainsKey(node.NodeId))
                {
                    nodesById.Add(node.NodeId, node);
                }
            }

            if (!HasPhysicalNodeSpacing(nodes))
            {
                return;
            }

            for (var edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
            {
                var edge = edges[edgeIndex];
                if (!TryGetPhysicalEdge(edge, nodesById, out var fromNode, out var toNode))
                {
                    continue;
                }

                var nodeClearance = NodeFootprintRadius
                    + edge.FloorPathWidth / 2f
                    + MinimumRouteClearance;
                foreach (var node in nodesById.Values)
                {
                    if (string.Equals(node.NodeId, edge.FromNodeId, StringComparison.Ordinal)
                        || string.Equals(node.NodeId, edge.ToNodeId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (PointToSegmentDistanceSquared(node, fromNode, toNode)
                        < nodeClearance * nodeClearance)
                    {
                        AddIssue(issues,
                            RealmLayoutGraphValidationIssue.NodeCorridorClearanceInvalid);
                    }
                }

                for (var comparisonIndex = edgeIndex + 1;
                     comparisonIndex < edges.Count;
                     comparisonIndex++)
                {
                    var comparison = edges[comparisonIndex];
                    if (SharesEndpoint(edge, comparison)
                        || !TryGetPhysicalEdge(
                            comparison,
                            nodesById,
                            out var comparisonFromNode,
                            out var comparisonToNode))
                    {
                        continue;
                    }

                    var corridorClearance = (edge.FloorPathWidth + comparison.FloorPathWidth) / 2f
                        + MinimumRouteClearance;
                    if (SegmentDistanceSquared(
                            fromNode,
                            toNode,
                            comparisonFromNode,
                            comparisonToNode)
                        < corridorClearance * corridorClearance)
                    {
                        AddIssue(issues,
                            RealmLayoutGraphValidationIssue.CorridorClearanceInvalid);
                    }
                }
            }
        }

        private static bool HasPhysicalNodeSpacing(IReadOnlyList<RealmLayoutGraphNode> nodes)
        {
            var minimumDistanceSquared = MinimumNodeSpacing * MinimumNodeSpacing;
            for (var first = 0; first < nodes.Count; first++)
            {
                var firstNode = nodes[first];
                if (firstNode == null
                    || !IsCoordinateValid(firstNode.X)
                    || !IsCoordinateValid(firstNode.Z))
                {
                    return false;
                }

                for (var second = first + 1; second < nodes.Count; second++)
                {
                    var secondNode = nodes[second];
                    if (secondNode == null
                        || !IsCoordinateValid(secondNode.X)
                        || !IsCoordinateValid(secondNode.Z)
                        || DistanceSquared(firstNode, secondNode) < minimumDistanceSquared)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static ISet<string> ReachableNodes(
            IReadOnlyList<RealmLayoutGraphEdge> edges,
            string entryNodeId,
            bool safeOnly)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal);
            if (edges == null || !HasStableId(entryNodeId))
            {
                return visited;
            }

            var pending = new Queue<string>();
            visited.Add(entryNodeId);
            pending.Enqueue(entryNodeId);
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

        private static bool TryGetPhysicalEdge(
            RealmLayoutGraphEdge edge,
            IReadOnlyDictionary<string, RealmLayoutGraphNode> nodesById,
            out RealmLayoutGraphNode fromNode,
            out RealmLayoutGraphNode toNode)
        {
            fromNode = null;
            toNode = null;
            return edge != null
                && IsFloorPathWidthValid(edge.FloorPathWidth)
                && HasStableId(edge.FromNodeId)
                && HasStableId(edge.ToNodeId)
                && nodesById.TryGetValue(edge.FromNodeId, out fromNode)
                && nodesById.TryGetValue(edge.ToNodeId, out toNode)
                && !string.Equals(edge.FromNodeId, edge.ToNodeId, StringComparison.Ordinal);
        }

        private static bool SharesEndpoint(RealmLayoutGraphEdge first, RealmLayoutGraphEdge second)
        {
            return first == null
                || second == null
                || string.Equals(first.FromNodeId, second.FromNodeId, StringComparison.Ordinal)
                || string.Equals(first.FromNodeId, second.ToNodeId, StringComparison.Ordinal)
                || string.Equals(first.ToNodeId, second.FromNodeId, StringComparison.Ordinal)
                || string.Equals(first.ToNodeId, second.ToNodeId, StringComparison.Ordinal);
        }

        private static float SegmentDistanceSquared(
            RealmLayoutGraphNode firstStart,
            RealmLayoutGraphNode firstEnd,
            RealmLayoutGraphNode secondStart,
            RealmLayoutGraphNode secondEnd)
        {
            if (SegmentsIntersect(firstStart, firstEnd, secondStart, secondEnd))
            {
                return 0f;
            }

            return Math.Min(
                Math.Min(
                    PointToSegmentDistanceSquared(firstStart, secondStart, secondEnd),
                    PointToSegmentDistanceSquared(firstEnd, secondStart, secondEnd)),
                Math.Min(
                    PointToSegmentDistanceSquared(secondStart, firstStart, firstEnd),
                    PointToSegmentDistanceSquared(secondEnd, firstStart, firstEnd)));
        }

        private static float PointToSegmentDistanceSquared(
            RealmLayoutGraphNode point,
            RealmLayoutGraphNode segmentStart,
            RealmLayoutGraphNode segmentEnd)
        {
            var deltaX = segmentEnd.X - segmentStart.X;
            var deltaZ = segmentEnd.Z - segmentStart.Z;
            var lengthSquared = deltaX * deltaX + deltaZ * deltaZ;
            if (lengthSquared <= 0f)
            {
                return DistanceSquared(point, segmentStart);
            }

            var progress = ((point.X - segmentStart.X) * deltaX
                + (point.Z - segmentStart.Z) * deltaZ) / lengthSquared;
            progress = Math.Max(0f, Math.Min(1f, progress));
            var nearestX = segmentStart.X + progress * deltaX;
            var nearestZ = segmentStart.Z + progress * deltaZ;
            var pointDeltaX = point.X - nearestX;
            var pointDeltaZ = point.Z - nearestZ;
            return pointDeltaX * pointDeltaX + pointDeltaZ * pointDeltaZ;
        }

        private static bool SegmentsIntersect(
            RealmLayoutGraphNode firstStart,
            RealmLayoutGraphNode firstEnd,
            RealmLayoutGraphNode secondStart,
            RealmLayoutGraphNode secondEnd)
        {
            var firstSecondStart = Cross(firstStart, firstEnd, secondStart);
            var firstSecondEnd = Cross(firstStart, firstEnd, secondEnd);
            var secondFirstStart = Cross(secondStart, secondEnd, firstStart);
            var secondFirstEnd = Cross(secondStart, secondEnd, firstEnd);
            if (((firstSecondStart > 0f && firstSecondEnd < 0f)
                    || (firstSecondStart < 0f && firstSecondEnd > 0f))
                && ((secondFirstStart > 0f && secondFirstEnd < 0f)
                    || (secondFirstStart < 0f && secondFirstEnd > 0f)))
            {
                return true;
            }

            return firstSecondStart == 0f && IsOnSegment(secondStart, firstStart, firstEnd)
                || firstSecondEnd == 0f && IsOnSegment(secondEnd, firstStart, firstEnd)
                || secondFirstStart == 0f && IsOnSegment(firstStart, secondStart, secondEnd)
                || secondFirstEnd == 0f && IsOnSegment(firstEnd, secondStart, secondEnd);
        }

        private static float Cross(
            RealmLayoutGraphNode segmentStart,
            RealmLayoutGraphNode segmentEnd,
            RealmLayoutGraphNode point)
        {
            return (segmentEnd.X - segmentStart.X) * (point.Z - segmentStart.Z)
                - (segmentEnd.Z - segmentStart.Z) * (point.X - segmentStart.X);
        }

        private static bool IsOnSegment(
            RealmLayoutGraphNode point,
            RealmLayoutGraphNode segmentStart,
            RealmLayoutGraphNode segmentEnd)
        {
            return point.X >= Math.Min(segmentStart.X, segmentEnd.X)
                && point.X <= Math.Max(segmentStart.X, segmentEnd.X)
                && point.Z >= Math.Min(segmentStart.Z, segmentEnd.Z)
                && point.Z <= Math.Max(segmentStart.Z, segmentEnd.Z);
        }

        private static float DistanceSquared(
            RealmLayoutGraphNode first,
            RealmLayoutGraphNode second)
        {
            var deltaX = first.X - second.X;
            var deltaZ = first.Z - second.Z;
            return deltaX * deltaX + deltaZ * deltaZ;
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

        private static bool IsCoordinateValid(float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value)
                && Math.Abs(value) <= MaximumAbsoluteCoordinate;
        }

        private static bool IsFloorPathWidthValid(float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value)
                && value >= MinimumFloorPathWidth
                && value <= MaximumFloorPathWidth;
        }

        private static string UndirectedEdgeKey(string firstNodeId, string secondNodeId)
        {
            return string.CompareOrdinal(firstNodeId, secondNodeId) < 0
                ? firstNodeId + "\u001f" + secondNodeId
                : secondNodeId + "\u001f" + firstNodeId;
        }

        private static void AddIssue(
            ICollection<RealmLayoutGraphValidationIssue> issues,
            RealmLayoutGraphValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }
}
