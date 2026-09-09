using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.WorldSurfaceValidation
{
    /// <summary>
    /// Stable ordinal reasons why a declared preview does not meet the mobile budget.
    /// Do not renumber existing values; callers may persist or display them as evidence.
    /// </summary>
    public enum WorldSurfacePreviewBudgetIssueCode
    {
        MissingSemanticId = 0,
        SourceDimensionsMustBePositive = 1,
        SourceDimensionsMustBeSquare = 2,
        SourceDimensionsMustBePowerOfTwo = 3,
        AndroidMaximumDimensionMustBePositive = 4,
        AndroidMaximumDimensionExceedsPreviewBudget = 5,
        ReadWriteMustBeDisabled = 6,
        MipmapsMustBeEnabled = 7,
        UnsupportedMobileCompressionLabel = 8
    }

    public sealed class WorldSurfacePreviewBudgetIssue
    {
        internal WorldSurfacePreviewBudgetIssue(WorldSurfacePreviewBudgetIssueCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public WorldSurfacePreviewBudgetIssueCode Code { get; }
        public string Message { get; }
    }

    public sealed class WorldSurfacePreviewBudgetResult
    {
        internal WorldSurfacePreviewBudgetResult(IList<WorldSurfacePreviewBudgetIssue> issues)
        {
            Issues = new ReadOnlyCollection<WorldSurfacePreviewBudgetIssue>(
                new List<WorldSurfacePreviewBudgetIssue>(issues));
        }

        public IReadOnlyList<WorldSurfacePreviewBudgetIssue> Issues { get; }
        public bool IsAccepted => Issues.Count == 0;
    }

    /// <summary>
    /// Pure validation of declared preview metadata. It never loads, reads or changes assets.
    /// </summary>
    public static class WorldSurfacePreviewBudgetGate
    {
        public const int MaximumAndroidPreviewDimension = 512;
        public const string Astc6x6CompressionLabel = "ASTC_6x6";
        public const string Etc2Rgba8CompressionLabel = "ETC2_RGBA8";

        public static WorldSurfacePreviewBudgetResult Evaluate(WorldSurfacePreviewMetadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            var issues = new List<WorldSurfacePreviewBudgetIssue>();
            if (string.IsNullOrWhiteSpace(metadata.SemanticId))
                Add(issues, WorldSurfacePreviewBudgetIssueCode.MissingSemanticId);

            if (metadata.SourceWidth <= 0 || metadata.SourceHeight <= 0)
            {
                Add(issues, WorldSurfacePreviewBudgetIssueCode.SourceDimensionsMustBePositive);
            }
            else
            {
                if (metadata.SourceWidth != metadata.SourceHeight)
                    Add(issues, WorldSurfacePreviewBudgetIssueCode.SourceDimensionsMustBeSquare);
                if (!IsPowerOfTwo(metadata.SourceWidth) || !IsPowerOfTwo(metadata.SourceHeight))
                    Add(issues, WorldSurfacePreviewBudgetIssueCode.SourceDimensionsMustBePowerOfTwo);
            }

            if (metadata.AndroidMaximumDimension <= 0)
            {
                Add(issues, WorldSurfacePreviewBudgetIssueCode.AndroidMaximumDimensionMustBePositive);
            }
            else if (metadata.AndroidMaximumDimension > MaximumAndroidPreviewDimension)
            {
                Add(issues, WorldSurfacePreviewBudgetIssueCode.AndroidMaximumDimensionExceedsPreviewBudget);
            }

            if (metadata.ReadWriteEnabled)
                Add(issues, WorldSurfacePreviewBudgetIssueCode.ReadWriteMustBeDisabled);
            if (!metadata.MipmapsEnabled)
                Add(issues, WorldSurfacePreviewBudgetIssueCode.MipmapsMustBeEnabled);
            if (!IsSupportedMobileCompressionLabel(metadata.MobileCompressionLabel))
                Add(issues, WorldSurfacePreviewBudgetIssueCode.UnsupportedMobileCompressionLabel);

            return new WorldSurfacePreviewBudgetResult(issues);
        }

        public static bool IsSupportedMobileCompressionLabel(string mobileCompressionLabel)
        {
            return string.Equals(mobileCompressionLabel, Astc6x6CompressionLabel, StringComparison.Ordinal)
                || string.Equals(mobileCompressionLabel, Etc2Rgba8CompressionLabel, StringComparison.Ordinal);
        }

        private static bool IsPowerOfTwo(int value)
        {
            return (value & (value - 1)) == 0;
        }

        private static void Add(ICollection<WorldSurfacePreviewBudgetIssue> issues, WorldSurfacePreviewBudgetIssueCode code)
        {
            issues.Add(new WorldSurfacePreviewBudgetIssue(code, GetMessage(code)));
        }

        private static string GetMessage(WorldSurfacePreviewBudgetIssueCode code)
        {
            switch (code)
            {
                case WorldSurfacePreviewBudgetIssueCode.MissingSemanticId:
                    return "Semantic identifier is required.";
                case WorldSurfacePreviewBudgetIssueCode.SourceDimensionsMustBePositive:
                    return "Source dimensions must both be positive.";
                case WorldSurfacePreviewBudgetIssueCode.SourceDimensionsMustBeSquare:
                    return "Source dimensions must be square.";
                case WorldSurfacePreviewBudgetIssueCode.SourceDimensionsMustBePowerOfTwo:
                    return "Source dimensions must both be powers of two.";
                case WorldSurfacePreviewBudgetIssueCode.AndroidMaximumDimensionMustBePositive:
                    return "Android maximum dimension must be positive.";
                case WorldSurfacePreviewBudgetIssueCode.AndroidMaximumDimensionExceedsPreviewBudget:
                    return "Android maximum dimension exceeds the 512 preview budget.";
                case WorldSurfacePreviewBudgetIssueCode.ReadWriteMustBeDisabled:
                    return "Read/Write must be disabled.";
                case WorldSurfacePreviewBudgetIssueCode.MipmapsMustBeEnabled:
                    return "Mipmaps must be enabled.";
                case WorldSurfacePreviewBudgetIssueCode.UnsupportedMobileCompressionLabel:
                    return "Mobile compression label must be ASTC_6x6 or ETC2_RGBA8.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown preview-budget issue code.");
            }
        }
    }
}
