using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.RealmExpansionPlanning;
using RealmRaiders.Modules.RealmGrowthContracts;
using RealmRaiders.Modules.RealmLayoutContracts;
using RealmRaiders.Modules.StarterRealmIdentityPlanning;
using RealmRaiders.Modules.StarterRealmIdentityRecord;
using RealmRaiders.Modules.StarterRealmLayouts;
using IdentityRecord = RealmRaiders.Modules.StarterRealmIdentityRecord.StarterRealmIdentityRecord;

namespace RealmRaiders.Modules.StarterRealmGrowthIdentityPlanning
{
    public enum StarterRealmGrowthIdentityPlanningStatus
    {
        Planned,
        IdentityPlanMissing,
        IdentityPlanRejected,
        PersistedIdentityResultMissing,
        PersistedIdentityRejected,
        PersistedRecordLayoutMismatch,
        CachedLayoutMismatch,
        CanonicalBytesRejected,
        ExpansionResultMissing,
        ExpansionResultRejected,
        ExpansionEvidenceMismatch
    }

    /// <summary>
    /// Immutable evidence only. It neither resolves a player tier nor applies an expansion.
    /// Socket instances retain the exact authored order from the supplied MGC18 plan.
    /// </summary>
    public sealed class SylvanStarterRealmGrowthIdentityPlan
    {
        internal SylvanStarterRealmGrowthIdentityPlan(
            StarterRealmGrowthIdentityPlanningStatus status,
            SylvanStarterRealmIdentityPlan identityPlan,
            StarterRealmIdentityResult identityResult,
            RealmLayoutRecipe recipe,
            RealmGrowthTier tier,
            RealmExpansionPlanResult expansionResult,
            IReadOnlyList<byte> canonicalBytes,
            IReadOnlyList<RealmLayoutExpansionSocket> expansionSockets)
        {
            Status = status;
            IdentityPlan = identityPlan;
            IdentityResult = identityResult;
            Recipe = recipe;
            Tier = tier;
            ExpansionResult = expansionResult;
            CanonicalBytes = Snapshot(canonicalBytes);
            ExpansionSockets = Snapshot(expansionSockets);
        }

        public StarterRealmGrowthIdentityPlanningStatus Status { get; }
        public SylvanStarterRealmIdentityPlan IdentityPlan { get; }
        public StarterRealmIdentityResult IdentityResult { get; }
        public RealmGrowthTier Tier { get; }
        public RealmExpansionPlanResult ExpansionResult { get; }
        public RealmLayoutRecipe Recipe { get; }
        public IReadOnlyList<byte> CanonicalBytes { get; }
        public IReadOnlyList<RealmLayoutExpansionSocket> ExpansionSockets { get; }
        public bool HasPlan => Status == StarterRealmGrowthIdentityPlanningStatus.Planned
            && IdentityResult != null && IdentityResult.HasRecord
            && Recipe != null
            && ExpansionResult != null && ExpansionResult.HasPlan;

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null) return Array.AsReadOnly(Array.Empty<T>());
            var copy = new T[source.Count];
            for (var index = 0; index < copy.Length; index++) copy[index] = source[index];
            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalStarterRealmGrowthIdentityPlan
    {
        internal InfernalStarterRealmGrowthIdentityPlan(
            StarterRealmGrowthIdentityPlanningStatus status,
            InfernalStarterRealmIdentityPlan identityPlan,
            StarterRealmIdentityResult identityResult,
            RealmLayoutGraph layout,
            RealmGrowthTier tier,
            RealmExpansionPlanResult expansionResult,
            IReadOnlyList<byte> canonicalBytes,
            IReadOnlyList<RealmLayoutGraphExpansionSocket> expansionSockets)
        {
            Status = status;
            IdentityPlan = identityPlan;
            IdentityResult = identityResult;
            Layout = layout;
            Tier = tier;
            ExpansionResult = expansionResult;
            CanonicalBytes = Snapshot(canonicalBytes);
            ExpansionSockets = Snapshot(expansionSockets);
        }

        public StarterRealmGrowthIdentityPlanningStatus Status { get; }
        public InfernalStarterRealmIdentityPlan IdentityPlan { get; }
        public StarterRealmIdentityResult IdentityResult { get; }
        public RealmGrowthTier Tier { get; }
        public RealmExpansionPlanResult ExpansionResult { get; }
        public RealmLayoutGraph Layout { get; }
        public IReadOnlyList<byte> CanonicalBytes { get; }
        public IReadOnlyList<RealmLayoutGraphExpansionSocket> ExpansionSockets { get; }
        public bool HasPlan => Status == StarterRealmGrowthIdentityPlanningStatus.Planned
            && IdentityResult != null && IdentityResult.HasRecord
            && Layout != null
            && ExpansionResult != null && ExpansionResult.HasPlan;

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null) return Array.AsReadOnly(Array.Empty<T>());
            var copy = new T[source.Count];
            for (var index = 0; index < copy.Length; index++) copy[index] = source[index];
            return Array.AsReadOnly(copy);
        }
    }

    public static class SylvanStarterRealmGrowthIdentityPlanner
    {
        public static SylvanStarterRealmGrowthIdentityPlan PlanExact(string realmId, int seed, RealmGrowthTier tier)
        {
            var identity = SylvanStarterRealmIdentityPlanner.PlanExact(realmId, seed);
            var graph = identity.HasPlan ? StarterSylvanRealmLayoutGraphAdapter.AdaptExactCached(identity.Recipe) : null;
            return ComposeExact(identity, tier, RealmExpansionPlanEvaluator.Evaluate(graph, tier));
        }

        public static SylvanStarterRealmGrowthIdentityPlan ComposeExact(
            SylvanStarterRealmIdentityPlan identityPlan,
            RealmGrowthTier tier,
            RealmExpansionPlanResult expansionResult)
        {
            var status = StarterRealmGrowthIdentityPlanningValidation.ValidateSylvanIdentity(identityPlan);
            if (status == StarterRealmGrowthIdentityPlanningStatus.Planned)
            {
                var expected = RealmExpansionPlanEvaluator.Evaluate(
                    StarterSylvanRealmLayoutGraphAdapter.AdaptExactCached(identityPlan.Recipe), tier);
                status = StarterRealmGrowthIdentityPlanningValidation.ValidateExpansion(expansionResult, expected);
            }

            return new SylvanStarterRealmGrowthIdentityPlan(
                status, identityPlan, identityPlan == null ? null : identityPlan.IdentityResult,
                identityPlan == null ? null : identityPlan.Recipe, tier, expansionResult,
                status == StarterRealmGrowthIdentityPlanningStatus.Planned ? identityPlan.CanonicalBytes : null,
                status == StarterRealmGrowthIdentityPlanningStatus.Planned
                    ? FirstAuthoredSockets(identityPlan.Recipe, expansionResult.Plan.ExpansionSockets.Count)
                    : null);
        }

        public static SylvanStarterRealmGrowthIdentityPlan ComposePersistedExact(
            StarterRealmIdentityResult identityResult,
            IReadOnlyList<byte> canonicalBytes,
            RealmLayoutRecipe recipe,
            RealmGrowthTier tier,
            RealmExpansionPlanResult expansionResult)
        {
            var status = StarterRealmGrowthIdentityPlanningValidation.ValidatePersistedIdentity(
                identityResult, canonicalBytes, recipe == null ? null : recipe.LayoutId);
            if (status == StarterRealmGrowthIdentityPlanningStatus.Planned
                && !StarterRealmGrowthIdentityPlanningValidation.IsExactSylvanRecipe(recipe))
            {
                status = StarterRealmGrowthIdentityPlanningStatus.CachedLayoutMismatch;
            }

            if (status == StarterRealmGrowthIdentityPlanningStatus.Planned)
            {
                var expected = RealmExpansionPlanEvaluator.Evaluate(
                    StarterSylvanRealmLayoutGraphAdapter.AdaptExactCached(recipe), tier);
                status = StarterRealmGrowthIdentityPlanningValidation.ValidateExpansion(expansionResult, expected);
            }

            return new SylvanStarterRealmGrowthIdentityPlan(
                status, null, identityResult, recipe, tier, expansionResult,
                status == StarterRealmGrowthIdentityPlanningStatus.Planned ? canonicalBytes : null,
                status == StarterRealmGrowthIdentityPlanningStatus.Planned
                    ? FirstAuthoredSockets(recipe, expansionResult.Plan.ExpansionSockets.Count)
                    : null);
        }

        private static IReadOnlyList<RealmLayoutExpansionSocket> FirstAuthoredSockets(
            RealmLayoutRecipe recipe, int count)
        {
            var selected = new RealmLayoutExpansionSocket[count];
            for (var index = 0; index < count; index++) selected[index] = recipe.ExpansionSockets[index];
            return selected;
        }
    }

    public static class InfernalStarterRealmGrowthIdentityPlanner
    {
        public static InfernalStarterRealmGrowthIdentityPlan PlanExact(string realmId, int seed, RealmGrowthTier tier)
        {
            var identity = InfernalStarterRealmIdentityPlanner.PlanExact(realmId, seed);
            return ComposeExact(identity, tier, RealmExpansionPlanEvaluator.Evaluate(identity.Layout, tier));
        }

        public static InfernalStarterRealmGrowthIdentityPlan ComposeExact(
            InfernalStarterRealmIdentityPlan identityPlan,
            RealmGrowthTier tier,
            RealmExpansionPlanResult expansionResult)
        {
            var status = StarterRealmGrowthIdentityPlanningValidation.ValidateInfernalIdentity(identityPlan);
            if (status == StarterRealmGrowthIdentityPlanningStatus.Planned)
            {
                var expected = RealmExpansionPlanEvaluator.Evaluate(identityPlan.Layout, tier);
                status = StarterRealmGrowthIdentityPlanningValidation.ValidateExpansion(expansionResult, expected);
            }

            return new InfernalStarterRealmGrowthIdentityPlan(
                status, identityPlan, identityPlan == null ? null : identityPlan.IdentityResult,
                identityPlan == null ? null : identityPlan.Layout, tier, expansionResult,
                status == StarterRealmGrowthIdentityPlanningStatus.Planned ? identityPlan.CanonicalBytes : null,
                status == StarterRealmGrowthIdentityPlanningStatus.Planned ? expansionResult.Plan.ExpansionSockets : null);
        }

        public static InfernalStarterRealmGrowthIdentityPlan ComposePersistedExact(
            StarterRealmIdentityResult identityResult,
            IReadOnlyList<byte> canonicalBytes,
            RealmLayoutGraph layout,
            RealmGrowthTier tier,
            RealmExpansionPlanResult expansionResult)
        {
            var status = StarterRealmGrowthIdentityPlanningValidation.ValidatePersistedIdentity(
                identityResult, canonicalBytes, layout == null ? null : layout.LayoutId);
            if (status == StarterRealmGrowthIdentityPlanningStatus.Planned
                && !StarterRealmGrowthIdentityPlanningValidation.IsExactInfernalLayout(layout))
            {
                status = StarterRealmGrowthIdentityPlanningStatus.CachedLayoutMismatch;
            }

            if (status == StarterRealmGrowthIdentityPlanningStatus.Planned)
            {
                var expected = RealmExpansionPlanEvaluator.Evaluate(layout, tier);
                status = StarterRealmGrowthIdentityPlanningValidation.ValidateExpansion(expansionResult, expected);
            }

            return new InfernalStarterRealmGrowthIdentityPlan(
                status, null, identityResult, layout, tier, expansionResult,
                status == StarterRealmGrowthIdentityPlanningStatus.Planned ? canonicalBytes : null,
                status == StarterRealmGrowthIdentityPlanningStatus.Planned ? expansionResult.Plan.ExpansionSockets : null);
        }
    }

    internal static class StarterRealmGrowthIdentityPlanningValidation
    {
        public static bool IsExactSylvanRecipe(RealmLayoutRecipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            var resolved = StarterSylvanRealmLayoutResolver.ResolveExact(recipe.LayoutId);
            return resolved.HasRecipe && ReferenceEquals(resolved.Recipe, recipe);
        }

        public static bool IsExactInfernalLayout(RealmLayoutGraph layout)
        {
            if (layout == null)
            {
                return false;
            }

            var resolved = StarterInfernalDefenseLayoutResolver.ResolveExact(layout.LayoutId);
            return resolved.HasLayout && ReferenceEquals(resolved.Layout, layout);
        }

        public static StarterRealmGrowthIdentityPlanningStatus ValidatePersistedIdentity(
            StarterRealmIdentityResult identityResult,
            IReadOnlyList<byte> canonicalBytes,
            string layoutId)
        {
            if (identityResult == null)
            {
                return StarterRealmGrowthIdentityPlanningStatus.PersistedIdentityResultMissing;
            }

            if (!identityResult.HasRecord)
            {
                return StarterRealmGrowthIdentityPlanningStatus.PersistedIdentityRejected;
            }

            if (!string.Equals(identityResult.Record.LayoutId, layoutId, StringComparison.Ordinal))
            {
                return StarterRealmGrowthIdentityPlanningStatus.PersistedRecordLayoutMismatch;
            }

            return ValidateCanonicalBytes(identityResult.Record, canonicalBytes);
        }
        public static StarterRealmGrowthIdentityPlanningStatus ValidateSylvanIdentity(SylvanStarterRealmIdentityPlan plan)
        {
            if (plan == null) return StarterRealmGrowthIdentityPlanningStatus.IdentityPlanMissing;
            if (!plan.HasPlan) return StarterRealmGrowthIdentityPlanningStatus.IdentityPlanRejected;
            return !IsExactSylvanRecipe(plan.Recipe)
                ? StarterRealmGrowthIdentityPlanningStatus.CachedLayoutMismatch
                : ValidateCanonicalBytes(plan.IdentityResult.Record, plan.CanonicalBytes);
        }

        public static StarterRealmGrowthIdentityPlanningStatus ValidateInfernalIdentity(InfernalStarterRealmIdentityPlan plan)
        {
            if (plan == null) return StarterRealmGrowthIdentityPlanningStatus.IdentityPlanMissing;
            if (!plan.HasPlan) return StarterRealmGrowthIdentityPlanningStatus.IdentityPlanRejected;
            return !IsExactInfernalLayout(plan.Layout)
                ? StarterRealmGrowthIdentityPlanningStatus.CachedLayoutMismatch
                : ValidateCanonicalBytes(plan.IdentityResult.Record, plan.CanonicalBytes);
        }

        public static StarterRealmGrowthIdentityPlanningStatus ValidateExpansion(
            RealmExpansionPlanResult actual,
            RealmExpansionPlanResult expected)
        {
            if (actual == null) return StarterRealmGrowthIdentityPlanningStatus.ExpansionResultMissing;
            if (!actual.HasPlan || expected == null || !expected.HasPlan)
                return StarterRealmGrowthIdentityPlanningStatus.ExpansionResultRejected;
            if (!ReferenceEquals(actual.SourceLayout, expected.SourceLayout)
                || !ReferenceEquals(actual.SourceTier, expected.SourceTier)
                || !string.Equals(actual.Plan.LayoutId, expected.Plan.LayoutId, StringComparison.Ordinal)
                || !string.Equals(actual.Plan.TierId, expected.Plan.TierId, StringComparison.Ordinal)
                || actual.Plan.ExpansionSockets.Count != expected.Plan.ExpansionSockets.Count)
                return StarterRealmGrowthIdentityPlanningStatus.ExpansionEvidenceMismatch;

            for (var index = 0; index < actual.Plan.ExpansionSockets.Count; index++)
            {
                var actualSocket = actual.Plan.ExpansionSockets[index];
                var expectedSocket = expected.Plan.ExpansionSockets[index];
                if (actualSocket == null || expectedSocket == null
                    || !ReferenceEquals(actualSocket, expectedSocket)
                    || !SocketFactsMatch(actualSocket, expectedSocket))
                    return StarterRealmGrowthIdentityPlanningStatus.ExpansionEvidenceMismatch;
            }

            return StarterRealmGrowthIdentityPlanningStatus.Planned;
        }

        private static StarterRealmGrowthIdentityPlanningStatus ValidateCanonicalBytes(
            IdentityRecord record, IReadOnlyList<byte> bytes)
        {
            if (record == null || bytes == null || bytes.Count == 0)
                return StarterRealmGrowthIdentityPlanningStatus.CanonicalBytesRejected;
            var copy = new byte[bytes.Count];
            for (var index = 0; index < copy.Length; index++) copy[index] = bytes[index];
            var parsed = StarterRealmIdentityCodec.ParseCanonicalUtf8(copy);
            if (!parsed.HasRecord || !RecordsMatch(parsed.Record, record))
                return StarterRealmGrowthIdentityPlanningStatus.CanonicalBytesRejected;
            var expected = StarterRealmIdentityCodec.SerializeCanonicalUtf8(record);
            if (expected.Length != bytes.Count) return StarterRealmGrowthIdentityPlanningStatus.CanonicalBytesRejected;
            for (var index = 0; index < expected.Length; index++)
                if (expected[index] != bytes[index]) return StarterRealmGrowthIdentityPlanningStatus.CanonicalBytesRejected;
            return StarterRealmGrowthIdentityPlanningStatus.Planned;
        }

        private static bool RecordsMatch(IdentityRecord first, IdentityRecord second)
        {
            return first.Version == second.Version && first.Seed == second.Seed
                && string.Equals(first.RealmId, second.RealmId, StringComparison.Ordinal)
                && string.Equals(first.LayoutId, second.LayoutId, StringComparison.Ordinal);
        }

        private static bool SocketFactsMatch(RealmLayoutGraphExpansionSocket first, RealmLayoutGraphExpansionSocket second)
        {
            return string.Equals(first.SocketId, second.SocketId, StringComparison.Ordinal)
                && string.Equals(first.NodeId, second.NodeId, StringComparison.Ordinal)
                && first.X.Equals(second.X) && first.Z.Equals(second.Z);
        }
    }
}
