using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.RealmGrowthContracts
{
    public enum RealmGrowthTierValidationIssue
    {
        CatalogueMissing,
        TierCardinalityInvalid,
        TierMissing,
        TierIdInvalid,
        TierIdDuplicate,
        MinimumLevelNegative,
        MinimumLevelNotStrictlyIncreasing,
        FootprintBudgetNonPositive,
        FootprintBudgetDecreases,
        NodeBudgetNonPositive,
        NodeBudgetDecreases,
        ExpansionAnchorCapacityNegative
    }

    public enum RealmGrowthTierLookupStatus
    {
        Found,
        InvalidTierId,
        NotFound,
        CatalogueInvalid
    }

    public enum RealmGrowthTierResolutionStatus
    {
        Resolved,
        LevelNegative,
        LevelBelowFirstTier,
        CatalogueInvalid
    }

    /// <summary>
    /// Immutable caller-supplied capacity facts. Values are budgets only: this type
    /// does not assign locations, currencies, content, or gameplay behavior.
    /// </summary>
    public sealed class RealmGrowthTier
    {
        public RealmGrowthTier(
            string tierId,
            int minimumLevel,
            int maximumFootprintBudget,
            int maximumNodeBudget,
            int expansionAnchorCapacity)
        {
            TierId = tierId;
            MinimumLevel = minimumLevel;
            MaximumFootprintBudget = maximumFootprintBudget;
            MaximumNodeBudget = maximumNodeBudget;
            ExpansionAnchorCapacity = expansionAnchorCapacity;
        }

        public string TierId { get; }

        public int MinimumLevel { get; }

        public int MaximumFootprintBudget { get; }

        public int MaximumNodeBudget { get; }

        public int ExpansionAnchorCapacity { get; }
    }

    /// <summary>
    /// Immutable caller-owned tier snapshot. The caller defines all tier facts;
    /// this package supplies no default thresholds or named game content.
    /// </summary>
    public sealed class RealmGrowthTierCatalogue
    {
        public RealmGrowthTierCatalogue(IReadOnlyList<RealmGrowthTier> tiers)
        {
            Tiers = Snapshot(tiers);
        }

        public IReadOnlyList<RealmGrowthTier> Tiers { get; }

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

    public sealed class RealmGrowthTierValidationResult
    {
        internal RealmGrowthTierValidationResult(
            IReadOnlyList<RealmGrowthTierValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<RealmGrowthTierValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<RealmGrowthTierValidationIssue> Snapshot(
            IReadOnlyList<RealmGrowthTierValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<RealmGrowthTierValidationIssue>());
            }

            var copy = new RealmGrowthTierValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class RealmGrowthTierLookupResult
    {
        internal RealmGrowthTierLookupResult(
            RealmGrowthTierLookupStatus status,
            RealmGrowthTier tier,
            RealmGrowthTierValidationResult validation)
        {
            Status = status;
            Tier = tier;
            Validation = validation;
        }

        public RealmGrowthTierLookupStatus Status { get; }

        public RealmGrowthTier Tier { get; }

        public RealmGrowthTierValidationResult Validation { get; }

        public bool Found
        {
            get
            {
                return Status == RealmGrowthTierLookupStatus.Found && Tier != null;
            }
        }
    }

    public sealed class RealmGrowthTierResolutionResult
    {
        internal RealmGrowthTierResolutionResult(
            RealmGrowthTierResolutionStatus status,
            RealmGrowthTier tier,
            RealmGrowthTierValidationResult validation)
        {
            Status = status;
            Tier = tier;
            Validation = validation;
        }

        public RealmGrowthTierResolutionStatus Status { get; }

        public RealmGrowthTier Tier { get; }

        public RealmGrowthTierValidationResult Validation { get; }

        public bool Resolved
        {
            get
            {
                return Status == RealmGrowthTierResolutionStatus.Resolved && Tier != null;
            }
        }
    }

    /// <summary>
    /// Deterministic validation, exact ordinal lookup, and level resolution over
    /// one explicit caller-supplied catalogue. It owns no progression state.
    /// </summary>
    public static class RealmGrowthTierContracts
    {
        public static RealmGrowthTierValidationResult Validate(
            RealmGrowthTierCatalogue catalogue)
        {
            var issues = new List<RealmGrowthTierValidationIssue>();
            if (catalogue == null)
            {
                AddIssue(issues, RealmGrowthTierValidationIssue.CatalogueMissing);
                return new RealmGrowthTierValidationResult(issues);
            }

            if (catalogue.Tiers == null || catalogue.Tiers.Count == 0)
            {
                AddIssue(issues, RealmGrowthTierValidationIssue.TierCardinalityInvalid);
                return new RealmGrowthTierValidationResult(issues);
            }

            var tierIds = new HashSet<string>(StringComparer.Ordinal);
            RealmGrowthTier previous = null;
            foreach (var tier in catalogue.Tiers)
            {
                if (tier == null)
                {
                    AddIssue(issues, RealmGrowthTierValidationIssue.TierMissing);
                    continue;
                }

                if (!HasStableId(tier.TierId))
                {
                    AddIssue(issues, RealmGrowthTierValidationIssue.TierIdInvalid);
                }
                else if (!tierIds.Add(tier.TierId))
                {
                    AddIssue(issues, RealmGrowthTierValidationIssue.TierIdDuplicate);
                }

                if (tier.MinimumLevel < 0)
                {
                    AddIssue(issues, RealmGrowthTierValidationIssue.MinimumLevelNegative);
                }

                if (tier.MaximumFootprintBudget <= 0)
                {
                    AddIssue(issues, RealmGrowthTierValidationIssue.FootprintBudgetNonPositive);
                }

                if (tier.MaximumNodeBudget <= 0)
                {
                    AddIssue(issues, RealmGrowthTierValidationIssue.NodeBudgetNonPositive);
                }

                if (tier.ExpansionAnchorCapacity < 0)
                {
                    AddIssue(issues, RealmGrowthTierValidationIssue.ExpansionAnchorCapacityNegative);
                }

                if (previous != null)
                {
                    if (tier.MinimumLevel <= previous.MinimumLevel)
                    {
                        AddIssue(issues, RealmGrowthTierValidationIssue.MinimumLevelNotStrictlyIncreasing);
                    }

                    if (tier.MaximumFootprintBudget < previous.MaximumFootprintBudget)
                    {
                        AddIssue(issues, RealmGrowthTierValidationIssue.FootprintBudgetDecreases);
                    }

                    if (tier.MaximumNodeBudget < previous.MaximumNodeBudget)
                    {
                        AddIssue(issues, RealmGrowthTierValidationIssue.NodeBudgetDecreases);
                    }
                }

                previous = tier;
            }

            return new RealmGrowthTierValidationResult(issues);
        }

        public static RealmGrowthTierLookupResult FindByTierId(
            RealmGrowthTierCatalogue catalogue,
            string tierId)
        {
            var validation = Validate(catalogue);
            if (!validation.IsValid)
            {
                return new RealmGrowthTierLookupResult(
                    RealmGrowthTierLookupStatus.CatalogueInvalid,
                    null,
                    validation);
            }

            if (!HasStableId(tierId))
            {
                return new RealmGrowthTierLookupResult(
                    RealmGrowthTierLookupStatus.InvalidTierId,
                    null,
                    validation);
            }

            foreach (var tier in catalogue.Tiers)
            {
                if (string.Equals(tier.TierId, tierId, StringComparison.Ordinal))
                {
                    return new RealmGrowthTierLookupResult(
                        RealmGrowthTierLookupStatus.Found,
                        tier,
                        validation);
                }
            }

            return new RealmGrowthTierLookupResult(
                RealmGrowthTierLookupStatus.NotFound,
                null,
                validation);
        }

        public static RealmGrowthTierResolutionResult ResolveLevel(
            RealmGrowthTierCatalogue catalogue,
            int level)
        {
            var validation = Validate(catalogue);
            if (!validation.IsValid)
            {
                return new RealmGrowthTierResolutionResult(
                    RealmGrowthTierResolutionStatus.CatalogueInvalid,
                    null,
                    validation);
            }

            if (level < 0)
            {
                return new RealmGrowthTierResolutionResult(
                    RealmGrowthTierResolutionStatus.LevelNegative,
                    null,
                    validation);
            }

            RealmGrowthTier resolved = null;
            foreach (var tier in catalogue.Tiers)
            {
                if (tier.MinimumLevel > level)
                {
                    break;
                }

                resolved = tier;
            }

            if (resolved == null)
            {
                return new RealmGrowthTierResolutionResult(
                    RealmGrowthTierResolutionStatus.LevelBelowFirstTier,
                    null,
                    validation);
            }

            return new RealmGrowthTierResolutionResult(
                RealmGrowthTierResolutionStatus.Resolved,
                resolved,
                validation);
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
            ICollection<RealmGrowthTierValidationIssue> issues,
            RealmGrowthTierValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }
}
