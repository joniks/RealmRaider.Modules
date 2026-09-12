using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.SylvanDefenseTradeoffs
{
    public enum SylvanDefenseTradeoffEvaluationIssue
    {
        RosterSummaryMissing,
        WolfCountNegative,
        GuardianEntCountNegative,
        RootTrapCountNegative,
        OpenCreatureSlotCountNegative,
        GuardianEntCardinalityInvalid,
        RootTrapCardinalityInvalid,
        CreatureSlotCardinalityInvalid,
        UnsupportedRosterSummary
    }

    /// <summary>
    /// Immutable, caller-supplied counts after Core has validated its authoritative
    /// DefenseLayout. This package does not know slot positions, costs, or saves.
    /// </summary>
    public sealed class SylvanDefenseRosterSummary
    {
        public SylvanDefenseRosterSummary(
            int wolves,
            int guardianEnts,
            int rootTraps,
            int openCreatureSlots)
        {
            Wolves = wolves;
            GuardianEnts = guardianEnts;
            RootTraps = rootTraps;
            OpenCreatureSlots = openCreatureSlots;
        }

        public int Wolves { get; }

        public int GuardianEnts { get; }

        public int RootTraps { get; }

        public int OpenCreatureSlots { get; }
    }

    /// <summary>Immutable player-facing tradeoff facts with no gameplay authority.</summary>
    public sealed class SylvanDefenseTradeoffFact
    {
        public SylvanDefenseTradeoffFact(
            string tradeoffId,
            string displayName,
            string tacticalSummary,
            int wolves,
            int guardianEnts,
            int rootTraps,
            int openCreatureSlots,
            int possessionEnergyMaximumSeconds)
        {
            TradeoffId = tradeoffId;
            DisplayName = displayName;
            TacticalSummary = tacticalSummary;
            Wolves = wolves;
            GuardianEnts = guardianEnts;
            RootTraps = rootTraps;
            OpenCreatureSlots = openCreatureSlots;
            PossessionEnergyMaximumSeconds = possessionEnergyMaximumSeconds;
        }

        public string TradeoffId { get; }

        public string DisplayName { get; }

        public string TacticalSummary { get; }

        public int Wolves { get; }

        public int GuardianEnts { get; }

        public int RootTraps { get; }

        public int OpenCreatureSlots { get; }

        public int PossessionEnergyMaximumSeconds { get; }
    }

    /// <summary>Immutable success fact or ordered fail-closed evaluation evidence.</summary>
    public sealed class SylvanDefenseTradeoffEvaluationResult
    {
        internal SylvanDefenseTradeoffEvaluationResult(
            SylvanDefenseTradeoffFact tradeoff,
            IReadOnlyList<SylvanDefenseTradeoffEvaluationIssue> issues)
        {
            Tradeoff = tradeoff;
            Issues = Snapshot(issues);
        }

        public SylvanDefenseTradeoffFact Tradeoff { get; }

        public IReadOnlyList<SylvanDefenseTradeoffEvaluationIssue> Issues { get; }

        public bool HasTradeoff
        {
            get
            {
                return Tradeoff != null && Issues.Count == 0;
            }
        }

        private static IReadOnlyList<SylvanDefenseTradeoffEvaluationIssue> Snapshot(
            IReadOnlyList<SylvanDefenseTradeoffEvaluationIssue> issues)
        {
            if (issues == null)
            {
                return Array.AsReadOnly(Array.Empty<SylvanDefenseTradeoffEvaluationIssue>());
            }

            var copy = new SylvanDefenseTradeoffEvaluationIssue[issues.Count];
            for (var index = 0; index < issues.Count; index++)
            {
                copy[index] = issues[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// Two explicit, cached tradeoff facts. Core deliberately applies its own
    /// authoritative layout, trap, and possession behavior after evaluation.
    /// </summary>
    public static class StarterSylvanDefenseTradeoffs
    {
        public const string PackPressureId = "PACK_PRESSURE";

        public const string KeeperReserveId = "KEEPER_RESERVE";

        public static SylvanDefenseTradeoffFact PackPressure { get; } =
            new SylvanDefenseTradeoffFact(
                PackPressureId,
                "Pack Pressure",
                "2 WOLVES \u2022 30 SEC CONTROL",
                2,
                1,
                1,
                0,
                30);

        public static SylvanDefenseTradeoffFact KeeperReserve { get; } =
            new SylvanDefenseTradeoffFact(
                KeeperReserveId,
                "Keeper Reserve",
                "1 WOLF SACRIFICED \u2022 45 SEC CONTROL",
                1,
                1,
                1,
                1,
                45);

        public static IReadOnlyList<SylvanDefenseTradeoffFact> All { get; } =
            Array.AsReadOnly(new[]
            {
                PackPressure,
                KeeperReserve
            });
    }

    /// <summary>Pure, deterministic mapping from Core-supplied roster counts to a cached fact.</summary>
    public static class SylvanDefenseTradeoffEvaluator
    {
        public static SylvanDefenseTradeoffEvaluationResult Evaluate(
            SylvanDefenseRosterSummary rosterSummary)
        {
            var issues = new List<SylvanDefenseTradeoffEvaluationIssue>();

            if (rosterSummary == null)
            {
                AddIssue(issues, SylvanDefenseTradeoffEvaluationIssue.RosterSummaryMissing);
                return new SylvanDefenseTradeoffEvaluationResult(null, issues);
            }

            AddNegativeCountIssues(rosterSummary, issues);
            if (issues.Count > 0)
            {
                return new SylvanDefenseTradeoffEvaluationResult(null, issues);
            }

            if (rosterSummary.GuardianEnts != 1)
            {
                AddIssue(issues, SylvanDefenseTradeoffEvaluationIssue.GuardianEntCardinalityInvalid);
            }

            if (rosterSummary.RootTraps != 1)
            {
                AddIssue(issues, SylvanDefenseTradeoffEvaluationIssue.RootTrapCardinalityInvalid);
            }

            if (rosterSummary.Wolves + rosterSummary.GuardianEnts
                + rosterSummary.OpenCreatureSlots != 3)
            {
                AddIssue(issues, SylvanDefenseTradeoffEvaluationIssue.CreatureSlotCardinalityInvalid);
            }

            if (issues.Count > 0)
            {
                return new SylvanDefenseTradeoffEvaluationResult(null, issues);
            }

            foreach (var tradeoff in StarterSylvanDefenseTradeoffs.All)
            {
                if (Matches(rosterSummary, tradeoff))
                {
                    return new SylvanDefenseTradeoffEvaluationResult(
                        tradeoff,
                        Array.AsReadOnly(Array.Empty<SylvanDefenseTradeoffEvaluationIssue>()));
                }
            }

            AddIssue(issues, SylvanDefenseTradeoffEvaluationIssue.UnsupportedRosterSummary);
            return new SylvanDefenseTradeoffEvaluationResult(null, issues);
        }

        private static void AddNegativeCountIssues(
            SylvanDefenseRosterSummary rosterSummary,
            ICollection<SylvanDefenseTradeoffEvaluationIssue> issues)
        {
            if (rosterSummary.Wolves < 0)
            {
                AddIssue(issues, SylvanDefenseTradeoffEvaluationIssue.WolfCountNegative);
            }

            if (rosterSummary.GuardianEnts < 0)
            {
                AddIssue(issues, SylvanDefenseTradeoffEvaluationIssue.GuardianEntCountNegative);
            }

            if (rosterSummary.RootTraps < 0)
            {
                AddIssue(issues, SylvanDefenseTradeoffEvaluationIssue.RootTrapCountNegative);
            }

            if (rosterSummary.OpenCreatureSlots < 0)
            {
                AddIssue(issues, SylvanDefenseTradeoffEvaluationIssue.OpenCreatureSlotCountNegative);
            }
        }

        private static bool Matches(
            SylvanDefenseRosterSummary rosterSummary,
            SylvanDefenseTradeoffFact tradeoff)
        {
            return rosterSummary.Wolves == tradeoff.Wolves
                && rosterSummary.GuardianEnts == tradeoff.GuardianEnts
                && rosterSummary.RootTraps == tradeoff.RootTraps
                && rosterSummary.OpenCreatureSlots == tradeoff.OpenCreatureSlots;
        }

        private static void AddIssue(
            ICollection<SylvanDefenseTradeoffEvaluationIssue> issues,
            SylvanDefenseTradeoffEvaluationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }
}
