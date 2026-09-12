using System;
using System.Collections.Generic;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.StarterRealmLayouts
{
    /// <summary>
    /// Additive conversion of existing cached Sylvan facts to the faction-neutral
    /// graph contract. It neither replaces the Sylvan API nor selects or materializes
    /// a layout; the Sylvan validator retains its exact seven-role rule.
    /// </summary>
    public static class StarterSylvanRealmLayoutGraphAdapter
    {
        public static RealmLayoutGraph Adapt(RealmLayoutRecipe recipe)
        {
            if (recipe == null)
            {
                return null;
            }

            return new RealmLayoutGraph(
                recipe.LayoutId,
                recipe.DisplayName,
                AdaptNodes(recipe.Nodes),
                AdaptEdges(recipe.Edges),
                AdaptLandmarks(recipe.Landmarks),
                AdaptExpansionSockets(recipe.ExpansionSockets));
        }

        private static IReadOnlyList<RealmLayoutGraphNode> AdaptNodes(
            IReadOnlyList<RealmLayoutNode> nodes)
        {
            if (nodes == null)
            {
                return null;
            }

            var adapted = new RealmLayoutGraphNode[nodes.Count];
            for (var index = 0; index < nodes.Count; index++)
            {
                var node = nodes[index];
                adapted[index] = node == null
                    ? null
                    : new RealmLayoutGraphNode(
                        node.NodeId,
                        MapGameplayRoleId(node.MaterializationRole),
                        MapNodeKind(node.Kind),
                        node.X,
                        node.Z);
            }

            return adapted;
        }

        private static IReadOnlyList<RealmLayoutGraphEdge> AdaptEdges(
            IReadOnlyList<RealmLayoutEdge> edges)
        {
            if (edges == null)
            {
                return null;
            }

            var adapted = new RealmLayoutGraphEdge[edges.Count];
            for (var index = 0; index < edges.Count; index++)
            {
                var edge = edges[index];
                adapted[index] = edge == null
                    ? null
                    : new RealmLayoutGraphEdge(
                        edge.EdgeId,
                        edge.FromNodeId,
                        edge.ToNodeId,
                        edge.IsActivePathSafe,
                        edge.FloorPathWidth);
            }

            return adapted;
        }

        private static IReadOnlyList<RealmLayoutGraphLandmark> AdaptLandmarks(
            IReadOnlyList<RealmLayoutLandmark> landmarks)
        {
            if (landmarks == null)
            {
                return null;
            }

            var adapted = new RealmLayoutGraphLandmark[landmarks.Count];
            for (var index = 0; index < landmarks.Count; index++)
            {
                var landmark = landmarks[index];
                adapted[index] = landmark == null
                    ? null
                    : new RealmLayoutGraphLandmark(
                        landmark.LandmarkId,
                        MapPresentationRoleId(landmark.VisualRole),
                        landmark.NodeId);
            }

            return adapted;
        }

        private static IReadOnlyList<RealmLayoutGraphExpansionSocket> AdaptExpansionSockets(
            IReadOnlyList<RealmLayoutExpansionSocket> expansionSockets)
        {
            if (expansionSockets == null)
            {
                return null;
            }

            var adapted = new RealmLayoutGraphExpansionSocket[expansionSockets.Count];
            for (var index = 0; index < expansionSockets.Count; index++)
            {
                var socket = expansionSockets[index];
                adapted[index] = socket == null
                    ? null
                    : new RealmLayoutGraphExpansionSocket(
                        socket.SocketId,
                        socket.NodeId,
                        socket.X,
                        socket.Z);
            }

            return adapted;
        }

        private static RealmLayoutGraphNodeKind MapNodeKind(RealmLayoutNodeKind kind)
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
                    return (RealmLayoutGraphNodeKind)(-1);
            }
        }

        private static string MapGameplayRoleId(SylvanRealmNodeMaterializationRole role)
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
                    return null;
            }
        }

        private static string MapPresentationRoleId(SylvanLandmarkVisualRole role)
        {
            switch (role)
            {
                case SylvanLandmarkVisualRole.NodeCanopy:
                    return "realmraiders.sylvan.node-canopy";
                case SylvanLandmarkVisualRole.SylvanHeartTree:
                    return "realmraiders.sylvan.heart-tree";
                default:
                    return null;
            }
        }
    }
}
