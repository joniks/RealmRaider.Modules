using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.CharacterVisualTuning
{
    /// <summary>
    /// Stable report-level evidence. Row-level policy evidence remains in each compatibility result.
    /// Existing values must not be renumbered because later adapters may persist or display them.
    /// </summary>
    public enum CharacterVisualBudgetBatchReportIssueCode
    {
        MissingCatalogue = 0
    }

    public sealed class CharacterVisualBudgetBatchReportIssue
    {
        internal CharacterVisualBudgetBatchReportIssue(
            CharacterVisualBudgetBatchReportIssueCode code,
            string message)
        {
            Code = code;
            Message = message;
        }

        public CharacterVisualBudgetBatchReportIssueCode Code { get; }
        public string Message { get; }
    }

    /// <summary>
    /// Immutable evidence for exactly one ProfileId in a caller-supplied catalogue.
    /// </summary>
    public sealed class CharacterVisualBudgetBatchRow
    {
        internal CharacterVisualBudgetBatchRow(
            string profileId,
            MobileVisualBudget budget,
            MobileVisualBudgetCompatibilityResult compatibility)
        {
            ProfileId = profileId;
            Budget = CopyBudget(budget);
            Compatibility = compatibility;
        }

        public string ProfileId { get; }
        public MobileVisualBudget Budget { get; }
        public MobileVisualBudgetCompatibilityResult Compatibility { get; }

        private static MobileVisualBudget CopyBudget(MobileVisualBudget budget)
        {
            return budget == null
                ? null
                : new MobileVisualBudget(
                    budget.MaterialCount,
                    budget.TextureCount,
                    budget.MaxTextureEdgePixels,
                    budget.TriangleCount);
        }
    }

    /// <summary>
    /// ProfileId-ordered immutable snapshot of one explicit catalogue under one explicit policy.
    /// It does not discover providers, inspect assets, or choose any policy defaults.
    /// </summary>
    public sealed class CharacterVisualBudgetBatchReport
    {
        private readonly ReadOnlyCollection<CharacterVisualBudgetBatchRow> rows;
        private readonly ReadOnlyCollection<CharacterVisualBudgetBatchReportIssue> issues;

        private CharacterVisualBudgetBatchReport(
            MobileVisualBudgetPolicy policy,
            IList<CharacterVisualBudgetBatchRow> rows,
            IList<CharacterVisualBudgetBatchReportIssue> issues)
        {
            Policy = CopyPolicy(policy);
            this.rows = new ReadOnlyCollection<CharacterVisualBudgetBatchRow>(
                new List<CharacterVisualBudgetBatchRow>(rows));
            this.issues = new ReadOnlyCollection<CharacterVisualBudgetBatchReportIssue>(
                new List<CharacterVisualBudgetBatchReportIssue>(issues));

            TotalProfileCount = this.rows.Count;
            for (var index = 0; index < this.rows.Count; index++)
            {
                if (this.rows[index].Compatibility.IsCompatible)
                    CompatibleProfileCount++;
            }
            IncompatibleProfileCount = TotalProfileCount - CompatibleProfileCount;
        }

        public MobileVisualBudgetPolicy Policy { get; }
        public IReadOnlyList<CharacterVisualBudgetBatchRow> Rows => rows;
        public IReadOnlyList<CharacterVisualBudgetBatchReportIssue> Issues => issues;
        public int TotalProfileCount { get; }
        public int CompatibleProfileCount { get; }
        public int IncompatibleProfileCount { get; }
        public bool HasReportIssues => Issues.Count != 0;

        public static CharacterVisualBudgetBatchReport Create(
            CharacterVisualTuningCatalogue catalogue,
            MobileVisualBudgetPolicy policy)
        {
            var reportIssues = new List<CharacterVisualBudgetBatchReportIssue>();
            if (catalogue == null)
            {
                reportIssues.Add(new CharacterVisualBudgetBatchReportIssue(
                    CharacterVisualBudgetBatchReportIssueCode.MissingCatalogue,
                    "Character visual tuning catalogue is required."));
                return new CharacterVisualBudgetBatchReport(
                    policy,
                    new List<CharacterVisualBudgetBatchRow>(),
                    reportIssues);
            }

            var rows = new List<CharacterVisualBudgetBatchRow>(catalogue.Profiles.Count);
            for (var index = 0; index < catalogue.Profiles.Count; index++)
            {
                var profile = catalogue.Profiles[index];
                var compatibility = MobileVisualBudgetCompatibilityGate.Evaluate(profile.Budget, policy);
                rows.Add(new CharacterVisualBudgetBatchRow(
                    profile.ProfileId,
                    profile.Budget,
                    compatibility));
            }
            rows.Sort((left, right) =>
                StringComparer.Ordinal.Compare(left.ProfileId, right.ProfileId));

            return new CharacterVisualBudgetBatchReport(policy, rows, reportIssues);
        }

        private static MobileVisualBudgetPolicy CopyPolicy(MobileVisualBudgetPolicy policy)
        {
            return policy == null
                ? null
                : new MobileVisualBudgetPolicy(
                    policy.MaximumMaterialCount,
                    policy.MaximumTextureCount,
                    policy.MaximumTextureEdgePixels,
                    policy.MaximumTriangleCount);
        }
    }
}
