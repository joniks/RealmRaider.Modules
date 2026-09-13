using System;
using System.Collections.Generic;
using RealmRaiders.Modules.RealmLayoutContracts;
using RealmRaiders.Modules.StarterRealmGrowthIdentityPlanning;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.StarterRealmExpansionChoiceRequest
{
    public enum StarterRealmExpansionChoiceStatus
    {
        Eligible,
        GrowthPlanMissing,
        GrowthPlanRejected,
        ExpansionCapacityZero,
        RequestedSocketIdMissing,
        RequestedSocketIdInvalid,
        RequestedSocketUnknown,
        RequestedSocketNotUnlocked,
        PlanEvidenceMismatch
    }

    public sealed class SylvanStarterRealmExpansionChoiceResult
    {
        internal SylvanStarterRealmExpansionChoiceResult(
            StarterRealmExpansionChoiceStatus status,
            SylvanStarterRealmGrowthIdentityPlan growthPlan,
            RealmLayoutExpansionSocket socket,
            int authoredSocketIndex)
        {
            Status = status;
            GrowthPlan = growthPlan;
            Socket = socket;
            AuthoredSocketIndex = authoredSocketIndex;
        }

        public StarterRealmExpansionChoiceStatus Status { get; }
        public SylvanStarterRealmGrowthIdentityPlan GrowthPlan { get; }
        public RealmLayoutExpansionSocket Socket { get; }
        public int AuthoredSocketIndex { get; }
        public bool IsEligible => Status == StarterRealmExpansionChoiceStatus.Eligible
            && GrowthPlan != null && Socket != null && AuthoredSocketIndex >= 0;
    }

    public sealed class InfernalStarterRealmExpansionChoiceResult
    {
        internal InfernalStarterRealmExpansionChoiceResult(
            StarterRealmExpansionChoiceStatus status,
            InfernalStarterRealmGrowthIdentityPlan growthPlan,
            RealmLayoutGraphExpansionSocket socket,
            int authoredSocketIndex)
        {
            Status = status;
            GrowthPlan = growthPlan;
            Socket = socket;
            AuthoredSocketIndex = authoredSocketIndex;
        }

        public StarterRealmExpansionChoiceStatus Status { get; }
        public InfernalStarterRealmGrowthIdentityPlan GrowthPlan { get; }
        public RealmLayoutGraphExpansionSocket Socket { get; }
        public int AuthoredSocketIndex { get; }
        public bool IsEligible => Status == StarterRealmExpansionChoiceStatus.Eligible
            && GrowthPlan != null && Socket != null && AuthoredSocketIndex >= 0;
    }

    /// <summary>
    /// Stateless request validation only. The caller owns entitlement, persistence,
    /// occupancy, geometry and every effect after an eligible result.
    /// </summary>
    public static class SylvanStarterRealmExpansionChoiceEvaluator
    {
        public static SylvanStarterRealmExpansionChoiceResult Evaluate(
            SylvanStarterRealmGrowthIdentityPlan growthPlan,
            string requestedSocketId)
        {
            var status = StarterRealmExpansionChoiceValidation.ValidateRequest(requestedSocketId);
            if (growthPlan == null)
                return Reject(StarterRealmExpansionChoiceStatus.GrowthPlanMissing, growthPlan);
            if (!growthPlan.HasPlan)
                return Reject(StarterRealmExpansionChoiceStatus.GrowthPlanRejected, growthPlan);
            if (!StarterRealmExpansionChoiceValidation.HasValidSylvanEvidence(growthPlan))
                return Reject(StarterRealmExpansionChoiceStatus.PlanEvidenceMismatch, growthPlan);
            if (growthPlan.ExpansionSockets.Count == 0)
                return Reject(StarterRealmExpansionChoiceStatus.ExpansionCapacityZero, growthPlan);
            if (status != StarterRealmExpansionChoiceStatus.Eligible)
                return Reject(status, growthPlan);

            var sourceIndex = StarterRealmExpansionChoiceValidation.FindSocket(
                growthPlan.Recipe.ExpansionSockets, requestedSocketId);
            if (sourceIndex < 0)
                return Reject(StarterRealmExpansionChoiceStatus.RequestedSocketUnknown, growthPlan);
            if (sourceIndex >= growthPlan.ExpansionSockets.Count)
                return Reject(StarterRealmExpansionChoiceStatus.RequestedSocketNotUnlocked, growthPlan);
            return new SylvanStarterRealmExpansionChoiceResult(
                StarterRealmExpansionChoiceStatus.Eligible,
                growthPlan,
                growthPlan.ExpansionSockets[sourceIndex],
                sourceIndex);
        }

        private static SylvanStarterRealmExpansionChoiceResult Reject(
            StarterRealmExpansionChoiceStatus status,
            SylvanStarterRealmGrowthIdentityPlan growthPlan)
        {
            return new SylvanStarterRealmExpansionChoiceResult(status, growthPlan, null, -1);
        }
    }

    public static class InfernalStarterRealmExpansionChoiceEvaluator
    {
        public static InfernalStarterRealmExpansionChoiceResult Evaluate(
            InfernalStarterRealmGrowthIdentityPlan growthPlan,
            string requestedSocketId)
        {
            var status = StarterRealmExpansionChoiceValidation.ValidateRequest(requestedSocketId);
            if (growthPlan == null)
                return Reject(StarterRealmExpansionChoiceStatus.GrowthPlanMissing, growthPlan);
            if (!growthPlan.HasPlan)
                return Reject(StarterRealmExpansionChoiceStatus.GrowthPlanRejected, growthPlan);
            if (!StarterRealmExpansionChoiceValidation.HasValidInfernalEvidence(growthPlan))
                return Reject(StarterRealmExpansionChoiceStatus.PlanEvidenceMismatch, growthPlan);
            if (growthPlan.ExpansionSockets.Count == 0)
                return Reject(StarterRealmExpansionChoiceStatus.ExpansionCapacityZero, growthPlan);
            if (status != StarterRealmExpansionChoiceStatus.Eligible)
                return Reject(status, growthPlan);

            var sourceIndex = StarterRealmExpansionChoiceValidation.FindSocket(
                growthPlan.Layout.ExpansionSockets, requestedSocketId);
            if (sourceIndex < 0)
                return Reject(StarterRealmExpansionChoiceStatus.RequestedSocketUnknown, growthPlan);
            if (sourceIndex >= growthPlan.ExpansionSockets.Count)
                return Reject(StarterRealmExpansionChoiceStatus.RequestedSocketNotUnlocked, growthPlan);
            return new InfernalStarterRealmExpansionChoiceResult(
                StarterRealmExpansionChoiceStatus.Eligible,
                growthPlan,
                growthPlan.ExpansionSockets[sourceIndex],
                sourceIndex);
        }

        private static InfernalStarterRealmExpansionChoiceResult Reject(
            StarterRealmExpansionChoiceStatus status,
            InfernalStarterRealmGrowthIdentityPlan growthPlan)
        {
            return new InfernalStarterRealmExpansionChoiceResult(status, growthPlan, null, -1);
        }
    }

    internal static class StarterRealmExpansionChoiceValidation
    {
        public static StarterRealmExpansionChoiceStatus ValidateRequest(string socketId)
        {
            if (string.IsNullOrEmpty(socketId))
                return StarterRealmExpansionChoiceStatus.RequestedSocketIdMissing;
            return HasStableId(socketId)
                ? StarterRealmExpansionChoiceStatus.Eligible
                : StarterRealmExpansionChoiceStatus.RequestedSocketIdInvalid;
        }

        public static bool HasValidSylvanEvidence(SylvanStarterRealmGrowthIdentityPlan plan)
        {
            if (plan.Recipe == null || plan.ExpansionResult == null || !plan.ExpansionResult.HasPlan
                || plan.ExpansionSockets.Count != plan.ExpansionResult.Plan.ExpansionSockets.Count)
                return false;
            for (var index = 0; index < plan.ExpansionSockets.Count; index++)
            {
                if (!ReferenceEquals(plan.Recipe.ExpansionSockets[index], plan.ExpansionSockets[index])
                    || !SocketFactsMatch(plan.Recipe.ExpansionSockets[index], plan.ExpansionSockets[index])
                    || !SocketFactsMatch(plan.ExpansionResult.Plan.ExpansionSockets[index], plan.ExpansionSockets[index]))
                    return false;
            }
            return true;
        }

        public static bool HasValidInfernalEvidence(InfernalStarterRealmGrowthIdentityPlan plan)
        {
            if (plan.Layout == null || plan.ExpansionResult == null || !plan.ExpansionResult.HasPlan
                || plan.ExpansionSockets.Count != plan.ExpansionResult.Plan.ExpansionSockets.Count)
                return false;
            for (var index = 0; index < plan.ExpansionSockets.Count; index++)
            {
                if (!ReferenceEquals(plan.Layout.ExpansionSockets[index], plan.ExpansionSockets[index])
                    || !ReferenceEquals(plan.ExpansionResult.Plan.ExpansionSockets[index], plan.ExpansionSockets[index]))
                    return false;
            }
            return true;
        }

        public static int FindSocket<T>(IReadOnlyList<T> sockets, string socketId)
            where T : class
        {
            if (sockets == null) return -1;
            for (var index = 0; index < sockets.Count; index++)
            {
                var socket = sockets[index];
                var id = socket is RealmLayoutExpansionSocket sylvan ? sylvan.SocketId
                    : socket is RealmLayoutGraphExpansionSocket infernal ? infernal.SocketId
                    : null;
                if (string.Equals(id, socketId, StringComparison.Ordinal)) return index;
            }
            return -1;
        }

        private static bool SocketFactsMatch(
            RealmLayoutExpansionSocket source,
            RealmLayoutExpansionSocket actual)
        {
            return source != null && actual != null
                && string.Equals(source.SocketId, actual.SocketId, StringComparison.Ordinal)
                && string.Equals(source.NodeId, actual.NodeId, StringComparison.Ordinal)
                && source.X.Equals(actual.X) && source.Z.Equals(actual.Z);
        }

        private static bool SocketFactsMatch(
            RealmLayoutGraphExpansionSocket source,
            RealmLayoutExpansionSocket actual)
        {
            return source != null && actual != null
                && string.Equals(source.SocketId, actual.SocketId, StringComparison.Ordinal)
                && string.Equals(source.NodeId, actual.NodeId, StringComparison.Ordinal)
                && source.X.Equals(actual.X) && source.Z.Equals(actual.Z);
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || !IsAsciiLowerAlphaNumeric(value[0])
                || !IsAsciiLowerAlphaNumeric(value[value.Length - 1])) return false;
            for (var index = 1; index < value.Length - 1; index++)
            {
                var symbol = value[index];
                if (!IsAsciiLowerAlphaNumeric(symbol) && symbol != '.' && symbol != '_' && symbol != '-')
                    return false;
            }
            return true;
        }

        private static bool IsAsciiLowerAlphaNumeric(char symbol)
        {
            return symbol >= 'a' && symbol <= 'z' || symbol >= '0' && symbol <= '9';
        }
    }
}
