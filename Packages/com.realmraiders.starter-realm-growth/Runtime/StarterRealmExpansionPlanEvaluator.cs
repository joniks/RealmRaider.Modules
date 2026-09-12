using System;
using System.Collections.Generic;
using RealmRaiders.Modules.RealmGrowthContracts;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.StarterRealmGrowth
{
    public enum StarterRealmExpansionPlanStatus
    {
        Planned,
        Rejected
    }

    public enum StarterRealmExpansionPlanIssue
    {
        LayoutMissing,
        LayoutIdInvalid,
        LayoutIdentityMismatch,
        LayoutInvalid,
        TierMissing,
        TierIdInvalid,
        TierFactsInvalid,
        ExpansionSocketFactInvalid,
        ExpansionAnchorCapacityExceedsAvailableSockets
    }

    /// <summary>
    /// Immutable expansion references selected from a caller-resolved, cached starter
    /// layout. These are authored socket facts only; they do not materialize a realm.
    /// </summary>
    public sealed class StarterRealmExpansionPlan
    {
        internal StarterRealmExpansionPlan(
            string layoutId,
            string tierId,
            IReadOnlyList<RealmLayoutExpansionSocket> expansionSockets)
        {
            LayoutId = layoutId;
            TierId = tierId;
            ExpansionSockets = Snapshot(expansionSockets);
        }

        public string LayoutId { get; }

        public string TierId { get; }

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

    /// <summary>Immutable plan result or deterministic, ordered rejection evidence.</summary>
    public sealed class StarterRealmExpansionPlanResult
    {
        internal StarterRealmExpansionPlanResult(
            StarterRealmExpansionPlanStatus status,
            StarterRealmExpansionPlan plan,
            IReadOnlyList<StarterRealmExpansionPlanIssue> issues)
        {
            Status = status;
            Plan = plan;
            Issues = Snapshot(issues);
        }

        public StarterRealmExpansionPlanStatus Status { get; }

        public StarterRealmExpansionPlan Plan { get; }

        public IReadOnlyList<StarterRealmExpansionPlanIssue> Issues { get; }

        public bool HasPlan
        {
            get
            {
                return Status == StarterRealmExpansionPlanStatus.Planned
                    && Plan != null
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

    /// <summary>
    /// Stateless, deterministic selection of the first authored expansion sockets.
    /// Core resolves the layout and tier beforehand and remains responsible for levels,
    /// persistence, geometry, and all materialization.
    /// </summary>
    public static class StarterRealmExpansionPlanEvaluator
    {
        public static StarterRealmExpansionPlanResult Evaluate(
            RealmLayoutRecipe layout,
            RealmGrowthTier tier)
        {
            var issues = new List<StarterRealmExpansionPlanIssue>();

            ValidateLayout(layout, issues);
            ValidateTier(tier, issues);
            ValidateExpansionSockets(layout, issues);

            if (layout != null
                && tier != null
                && tier.ExpansionAnchorCapacity >= 0
                && layout.ExpansionSockets != null
                && tier.ExpansionAnchorCapacity > layout.ExpansionSockets.Count)
            {
                AddIssue(
                    issues,
                    StarterRealmExpansionPlanIssue.ExpansionAnchorCapacityExceedsAvailableSockets);
            }

            if (issues.Count > 0)
            {
                return new StarterRealmExpansionPlanResult(
                    StarterRealmExpansionPlanStatus.Rejected,
                    null,
                    issues);
            }

            var selectedSockets = new RealmLayoutExpansionSocket[tier.ExpansionAnchorCapacity];
            for (var index = 0; index < selectedSockets.Length; index++)
            {
                selectedSockets[index] = layout.ExpansionSockets[index];
            }

            return new StarterRealmExpansionPlanResult(
                StarterRealmExpansionPlanStatus.Planned,
                new StarterRealmExpansionPlan(layout.LayoutId, tier.TierId, selectedSockets),
                Array.AsReadOnly(Array.Empty<StarterRealmExpansionPlanIssue>()));
        }

        private static void ValidateLayout(
            RealmLayoutRecipe layout,
            ICollection<StarterRealmExpansionPlanIssue> issues)
        {
            if (layout == null)
            {
                AddIssue(issues, StarterRealmExpansionPlanIssue.LayoutMissing);
                return;
            }

            var resolved = StarterSylvanRealmLayoutResolver.ResolveExact(layout.LayoutId);
            if (!resolved.HasRecipe)
            {
                AddIssue(issues, StarterRealmExpansionPlanIssue.LayoutIdInvalid);
            }
            else if (!ReferenceEquals(layout, resolved.Recipe))
            {
                AddIssue(issues, StarterRealmExpansionPlanIssue.LayoutIdentityMismatch);
            }

            if (!RealmLayoutRecipeValidator.ValidateStarterRecipe(layout).IsValid)
            {
                AddIssue(issues, StarterRealmExpansionPlanIssue.LayoutInvalid);
            }
        }

        private static void ValidateTier(
            RealmGrowthTier tier,
            ICollection<StarterRealmExpansionPlanIssue> issues)
        {
            if (tier == null)
            {
                AddIssue(issues, StarterRealmExpansionPlanIssue.TierMissing);
                return;
            }

            if (!HasStableId(tier.TierId))
            {
                AddIssue(issues, StarterRealmExpansionPlanIssue.TierIdInvalid);
            }

            if (tier.MinimumLevel < 0
                || tier.MaximumFootprintBudget <= 0
                || tier.MaximumNodeBudget <= 0
                || tier.ExpansionAnchorCapacity < 0)
            {
                AddIssue(issues, StarterRealmExpansionPlanIssue.TierFactsInvalid);
            }
        }

        private static void ValidateExpansionSockets(
            RealmLayoutRecipe layout,
            ICollection<StarterRealmExpansionPlanIssue> issues)
        {
            if (layout == null || layout.ExpansionSockets == null)
            {
                return;
            }

            foreach (var socket in layout.ExpansionSockets)
            {
                if (socket == null
                    || !HasStableId(socket.SocketId)
                    || !HasStableId(socket.NodeId)
                    || !IsFinite(socket.X)
                    || !IsFinite(socket.Z))
                {
                    AddIssue(issues, StarterRealmExpansionPlanIssue.ExpansionSocketFactInvalid);
                }
            }
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

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void AddIssue(
            ICollection<StarterRealmExpansionPlanIssue> issues,
            StarterRealmExpansionPlanIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }
}
