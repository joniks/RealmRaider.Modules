using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.CharacterVisualTuning
{
    /// <summary>
    /// Immutable caller-supplied maximums for one mobile character-visual acceptance decision.
    /// This type has no defaults and does not inspect models, assets or Unity settings.
    /// </summary>
    public sealed class MobileVisualBudgetPolicy
    {
        public MobileVisualBudgetPolicy(
            int maximumMaterialCount,
            int maximumTextureCount,
            int maximumTextureEdgePixels,
            int maximumTriangleCount)
        {
            MaximumMaterialCount = maximumMaterialCount;
            MaximumTextureCount = maximumTextureCount;
            MaximumTextureEdgePixels = maximumTextureEdgePixels;
            MaximumTriangleCount = maximumTriangleCount;
        }

        public int MaximumMaterialCount { get; }
        public int MaximumTextureCount { get; }
        public int MaximumTextureEdgePixels { get; }
        public int MaximumTriangleCount { get; }
    }

    /// <summary>
    /// Stable ordinal reasons why a declared budget cannot be accepted by a supplied policy.
    /// Existing values must not be renumbered because later adapters may persist or display them.
    /// </summary>
    public enum MobileVisualBudgetCompatibilityIssueCode
    {
        MissingBudget = 0,
        MissingPolicy = 1,
        PolicyMaximumMaterialCountMustBePositive = 2,
        PolicyMaximumTextureCountMustBePositive = 3,
        PolicyMaximumTextureEdgePixelsMustBePositive = 4,
        PolicyMaximumTriangleCountMustBePositive = 5,
        BudgetMaterialCountMustBePositive = 6,
        BudgetTextureCountMustBePositive = 7,
        BudgetTextureEdgePixelsMustBePositive = 8,
        BudgetTriangleCountMustBePositive = 9,
        MaterialCountExceedsPolicy = 10,
        TextureCountExceedsPolicy = 11,
        TextureEdgePixelsExceedsPolicy = 12,
        TriangleCountExceedsPolicy = 13
    }

    public sealed class MobileVisualBudgetCompatibilityIssue
    {
        internal MobileVisualBudgetCompatibilityIssue(
            MobileVisualBudgetCompatibilityIssueCode code,
            string message)
        {
            Code = code;
            Message = message;
        }

        public MobileVisualBudgetCompatibilityIssueCode Code { get; }
        public string Message { get; }
    }

    public sealed class MobileVisualBudgetCompatibilityResult
    {
        internal MobileVisualBudgetCompatibilityResult(IList<MobileVisualBudgetCompatibilityIssue> issues)
        {
            Issues = new ReadOnlyCollection<MobileVisualBudgetCompatibilityIssue>(
                new List<MobileVisualBudgetCompatibilityIssue>(issues));
        }

        public IReadOnlyList<MobileVisualBudgetCompatibilityIssue> Issues { get; }
        public bool IsCompatible => Issues.Count == 0;
    }

    /// <summary>
    /// Compares immutable declared counts against immutable caller-supplied limits only.
    /// </summary>
    public static class MobileVisualBudgetCompatibilityGate
    {
        public static MobileVisualBudgetCompatibilityResult Evaluate(
            MobileVisualBudget budget,
            MobileVisualBudgetPolicy policy)
        {
            var issues = new List<MobileVisualBudgetCompatibilityIssue>();
            if (budget == null)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.MissingBudget);
            if (policy == null)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.MissingPolicy);

            if (policy != null)
                ValidatePolicy(policy, issues);
            if (budget != null)
                ValidateBudget(budget, issues);
            if (budget != null && policy != null)
                ValidateCompatibility(budget, policy, issues);

            return new MobileVisualBudgetCompatibilityResult(issues);
        }

        private static void ValidatePolicy(
            MobileVisualBudgetPolicy policy,
            ICollection<MobileVisualBudgetCompatibilityIssue> issues)
        {
            if (policy.MaximumMaterialCount <= 0)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumMaterialCountMustBePositive);
            if (policy.MaximumTextureCount <= 0)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumTextureCountMustBePositive);
            if (policy.MaximumTextureEdgePixels <= 0)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumTextureEdgePixelsMustBePositive);
            if (policy.MaximumTriangleCount <= 0)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumTriangleCountMustBePositive);
        }

        private static void ValidateBudget(
            MobileVisualBudget budget,
            ICollection<MobileVisualBudgetCompatibilityIssue> issues)
        {
            if (budget.MaterialCount <= 0)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.BudgetMaterialCountMustBePositive);
            if (budget.TextureCount <= 0)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.BudgetTextureCountMustBePositive);
            if (budget.MaxTextureEdgePixels <= 0)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.BudgetTextureEdgePixelsMustBePositive);
            if (budget.TriangleCount <= 0)
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.BudgetTriangleCountMustBePositive);
        }

        private static void ValidateCompatibility(
            MobileVisualBudget budget,
            MobileVisualBudgetPolicy policy,
            ICollection<MobileVisualBudgetCompatibilityIssue> issues)
        {
            if (budget.MaterialCount > 0 && policy.MaximumMaterialCount > 0 &&
                budget.MaterialCount > policy.MaximumMaterialCount)
            {
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.MaterialCountExceedsPolicy);
            }
            if (budget.TextureCount > 0 && policy.MaximumTextureCount > 0 &&
                budget.TextureCount > policy.MaximumTextureCount)
            {
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.TextureCountExceedsPolicy);
            }
            if (budget.MaxTextureEdgePixels > 0 && policy.MaximumTextureEdgePixels > 0 &&
                budget.MaxTextureEdgePixels > policy.MaximumTextureEdgePixels)
            {
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.TextureEdgePixelsExceedsPolicy);
            }
            if (budget.TriangleCount > 0 && policy.MaximumTriangleCount > 0 &&
                budget.TriangleCount > policy.MaximumTriangleCount)
            {
                Add(issues, MobileVisualBudgetCompatibilityIssueCode.TriangleCountExceedsPolicy);
            }
        }

        private static void Add(
            ICollection<MobileVisualBudgetCompatibilityIssue> issues,
            MobileVisualBudgetCompatibilityIssueCode code)
        {
            issues.Add(new MobileVisualBudgetCompatibilityIssue(code, GetMessage(code)));
        }

        private static string GetMessage(MobileVisualBudgetCompatibilityIssueCode code)
        {
            switch (code)
            {
                case MobileVisualBudgetCompatibilityIssueCode.MissingBudget:
                    return "Mobile visual budget is required.";
                case MobileVisualBudgetCompatibilityIssueCode.MissingPolicy:
                    return "Mobile visual budget policy is required.";
                case MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumMaterialCountMustBePositive:
                    return "Policy maximum material count must be positive.";
                case MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumTextureCountMustBePositive:
                    return "Policy maximum texture count must be positive.";
                case MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumTextureEdgePixelsMustBePositive:
                    return "Policy maximum texture edge pixels must be positive.";
                case MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumTriangleCountMustBePositive:
                    return "Policy maximum triangle count must be positive.";
                case MobileVisualBudgetCompatibilityIssueCode.BudgetMaterialCountMustBePositive:
                    return "Budget material count must be positive.";
                case MobileVisualBudgetCompatibilityIssueCode.BudgetTextureCountMustBePositive:
                    return "Budget texture count must be positive.";
                case MobileVisualBudgetCompatibilityIssueCode.BudgetTextureEdgePixelsMustBePositive:
                    return "Budget texture edge pixels must be positive.";
                case MobileVisualBudgetCompatibilityIssueCode.BudgetTriangleCountMustBePositive:
                    return "Budget triangle count must be positive.";
                case MobileVisualBudgetCompatibilityIssueCode.MaterialCountExceedsPolicy:
                    return "Budget material count exceeds the supplied policy.";
                case MobileVisualBudgetCompatibilityIssueCode.TextureCountExceedsPolicy:
                    return "Budget texture count exceeds the supplied policy.";
                case MobileVisualBudgetCompatibilityIssueCode.TextureEdgePixelsExceedsPolicy:
                    return "Budget texture edge pixels exceed the supplied policy.";
                case MobileVisualBudgetCompatibilityIssueCode.TriangleCountExceedsPolicy:
                    return "Budget triangle count exceeds the supplied policy.";
                default:
                    return "Unknown mobile visual budget compatibility issue.";
            }
        }
    }
}
