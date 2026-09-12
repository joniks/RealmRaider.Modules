using System;
using System.Collections.Generic;
using RealmRaiders.Modules.RealmGrowthContracts;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.RealmExpansionPlanning
{
    public enum RealmExpansionPlanStatus
    {
        Planned,
        Rejected
    }

    public enum RealmExpansionPlanIssue
    {
        LayoutMissing,
        LayoutIdInvalid,
        LayoutInvalid,
        TierMissing,
        TierIdInvalid,
        TierFactsInvalid,
        ExpansionSocketMissing,
        ExpansionSocketIdInvalid,
        ExpansionSocketIdDuplicate,
        ExpansionSocketNodeInvalid,
        ExpansionSocketCoordinateInvalid,
        ExpansionAnchorCapacityExceedsAvailableSockets
    }

    /// <summary>
    /// Immutable references to the first authored sockets in the caller's graph.
    /// The plan never materializes those sockets or chooses content for them.
    /// </summary>
    public sealed class RealmExpansionPlan
    {
        internal RealmExpansionPlan(
            string layoutId,
            string tierId,
            IReadOnlyList<RealmLayoutGraphExpansionSocket> expansionSockets)
        {
            LayoutId = layoutId;
            TierId = tierId;
            ExpansionSockets = Snapshot(expansionSockets);
        }

        public string LayoutId { get; }

        public string TierId { get; }

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

    /// <summary>Immutable plan or deterministic ordered rejection evidence.</summary>
    public sealed class RealmExpansionPlanResult
    {
        internal RealmExpansionPlanResult(
            RealmExpansionPlanStatus status,
            RealmExpansionPlan plan,
            IReadOnlyList<RealmExpansionPlanIssue> issues,
            RealmLayoutGraphValidationResult layoutValidation,
            RealmGrowthTierValidationResult tierValidation)
        {
            Status = status;
            Plan = plan;
            Issues = Snapshot(issues);
            LayoutValidation = layoutValidation;
            TierValidation = tierValidation;
        }

        public RealmExpansionPlanStatus Status { get; }

        public RealmExpansionPlan Plan { get; }

        public IReadOnlyList<RealmExpansionPlanIssue> Issues { get; }

        public RealmLayoutGraphValidationResult LayoutValidation { get; }

        public RealmGrowthTierValidationResult TierValidation { get; }

        public bool HasPlan => Status == RealmExpansionPlanStatus.Planned
            && Plan != null
            && Issues.Count == 0
            && LayoutValidation != null
            && LayoutValidation.IsValid
            && TierValidation != null
            && TierValidation.IsValid;

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
    /// Stateless first-N selection only. The caller owns tier resolution, levels,
    /// thresholds, persistence, geometry and every runtime effect.
    /// </summary>
    public static class RealmExpansionPlanEvaluator
    {
        public static RealmExpansionPlanResult Evaluate(
            RealmLayoutGraph layout,
            RealmGrowthTier tier)
        {
            var issues = new List<RealmExpansionPlanIssue>();
            var layoutValidation = ValidateLayout(layout, issues);
            var tierValidation = ValidateTier(tier, issues);

            if (layout != null
                && tier != null
                && tier.ExpansionAnchorCapacity >= 0
                && tier.ExpansionAnchorCapacity > layout.ExpansionSockets.Count)
            {
                AddIssue(
                    issues,
                    RealmExpansionPlanIssue.ExpansionAnchorCapacityExceedsAvailableSockets);
            }

            if (issues.Count > 0)
            {
                return new RealmExpansionPlanResult(
                    RealmExpansionPlanStatus.Rejected,
                    null,
                    issues,
                    layoutValidation,
                    tierValidation);
            }

            var selected = new RealmLayoutGraphExpansionSocket[
                tier.ExpansionAnchorCapacity];
            for (var index = 0; index < selected.Length; index++)
            {
                selected[index] = layout.ExpansionSockets[index];
            }

            return new RealmExpansionPlanResult(
                RealmExpansionPlanStatus.Planned,
                new RealmExpansionPlan(layout.LayoutId, tier.TierId, selected),
                Array.AsReadOnly(Array.Empty<RealmExpansionPlanIssue>()),
                layoutValidation,
                tierValidation);
        }

        private static RealmLayoutGraphValidationResult ValidateLayout(
            RealmLayoutGraph layout,
            ICollection<RealmExpansionPlanIssue> issues)
        {
            var validation = RealmLayoutGraphValidator.Validate(layout);
            if (layout == null)
            {
                AddIssue(issues, RealmExpansionPlanIssue.LayoutMissing);
                return validation;
            }

            if (!HasStableId(layout.LayoutId))
            {
                AddIssue(issues, RealmExpansionPlanIssue.LayoutIdInvalid);
            }

            if (!validation.IsValid)
            {
                AddIssue(issues, RealmExpansionPlanIssue.LayoutInvalid);
            }

            AddMappedSocketIssues(validation.Issues, issues);
            return validation;
        }

        private static RealmGrowthTierValidationResult ValidateTier(
            RealmGrowthTier tier,
            ICollection<RealmExpansionPlanIssue> issues)
        {
            if (tier == null)
            {
                AddIssue(issues, RealmExpansionPlanIssue.TierMissing);
                return RealmGrowthTierContracts.Validate(null);
            }

            var validation = RealmGrowthTierContracts.Validate(
                new RealmGrowthTierCatalogue(new[] { tier }));
            if (Contains(
                    validation.Issues,
                    RealmGrowthTierValidationIssue.TierIdInvalid))
            {
                AddIssue(issues, RealmExpansionPlanIssue.TierIdInvalid);
            }

            if (Contains(
                    validation.Issues,
                    RealmGrowthTierValidationIssue.MinimumLevelNegative)
                || Contains(
                    validation.Issues,
                    RealmGrowthTierValidationIssue.FootprintBudgetNonPositive)
                || Contains(
                    validation.Issues,
                    RealmGrowthTierValidationIssue.NodeBudgetNonPositive)
                || Contains(
                    validation.Issues,
                    RealmGrowthTierValidationIssue.ExpansionAnchorCapacityNegative))
            {
                AddIssue(issues, RealmExpansionPlanIssue.TierFactsInvalid);
            }

            return validation;
        }

        private static void AddMappedSocketIssues(
            IReadOnlyList<RealmLayoutGraphValidationIssue> graphIssues,
            ICollection<RealmExpansionPlanIssue> issues)
        {
            if (Contains(graphIssues, RealmLayoutGraphValidationIssue.ExpansionSocketMissing))
            {
                AddIssue(issues, RealmExpansionPlanIssue.ExpansionSocketMissing);
            }

            if (Contains(graphIssues, RealmLayoutGraphValidationIssue.ExpansionSocketIdInvalid))
            {
                AddIssue(issues, RealmExpansionPlanIssue.ExpansionSocketIdInvalid);
            }

            if (Contains(graphIssues, RealmLayoutGraphValidationIssue.ExpansionSocketIdDuplicate))
            {
                AddIssue(issues, RealmExpansionPlanIssue.ExpansionSocketIdDuplicate);
            }

            if (Contains(graphIssues, RealmLayoutGraphValidationIssue.ExpansionSocketNodeInvalid))
            {
                AddIssue(issues, RealmExpansionPlanIssue.ExpansionSocketNodeInvalid);
            }

            if (Contains(
                    graphIssues,
                    RealmLayoutGraphValidationIssue.ExpansionSocketCoordinateInvalid))
            {
                AddIssue(issues, RealmExpansionPlanIssue.ExpansionSocketCoordinateInvalid);
            }
        }

        private static bool Contains(
            IReadOnlyList<RealmLayoutGraphValidationIssue> issues,
            RealmLayoutGraphValidationIssue expected)
        {
            foreach (var issue in issues)
            {
                if (issue == expected)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Contains(
            IReadOnlyList<RealmGrowthTierValidationIssue> issues,
            RealmGrowthTierValidationIssue expected)
        {
            foreach (var issue in issues)
            {
                if (issue == expected)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || !IsAsciiLowerAlphaNumeric(value[0])
                || !IsAsciiLowerAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            for (var index = 1; index < value.Length - 1; index++)
            {
                var symbol = value[index];
                if (!IsAsciiLowerAlphaNumeric(symbol)
                    && symbol != '.'
                    && symbol != '_'
                    && symbol != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiLowerAlphaNumeric(char symbol)
        {
            return symbol >= 'a' && symbol <= 'z'
                || symbol >= '0' && symbol <= '9';
        }

        private static void AddIssue(
            ICollection<RealmExpansionPlanIssue> issues,
            RealmExpansionPlanIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }
}
