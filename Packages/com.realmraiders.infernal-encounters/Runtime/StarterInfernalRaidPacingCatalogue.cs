using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.InfernalEncounters
{
    public enum InfernalRaidBeatKind
    {
        Enemy,
        Hazard,
        Objective
    }

    /// <summary>One ordered, factual pacing beat. Core owns all later gameplay behavior.</summary>
    public sealed class InfernalRaidPacingBeat
    {
        public InfernalRaidPacingBeat(
            string beatId,
            string displayName,
            InfernalRaidBeatKind kind,
            string contentId,
            bool isOptionalRisk,
            bool isBypassable,
            string prerequisiteGateId,
            int approximateStartSeconds,
            int approximateEndSeconds)
        {
            BeatId = beatId;
            DisplayName = displayName;
            Kind = kind;
            ContentId = contentId;
            IsOptionalRisk = isOptionalRisk;
            IsBypassable = isBypassable;
            PrerequisiteGateId = prerequisiteGateId;
            ApproximateStartSeconds = approximateStartSeconds;
            ApproximateEndSeconds = approximateEndSeconds;
        }

        public string BeatId { get; }

        public string DisplayName { get; }

        public InfernalRaidBeatKind Kind { get; }

        public string ContentId { get; }

        public bool IsOptionalRisk { get; }

        public bool IsBypassable { get; }

        public string PrerequisiteGateId { get; }

        public int ApproximateStartSeconds { get; }

        public int ApproximateEndSeconds { get; }
    }

    /// <summary>Immutable, caller-selected pacing facts for one complete Infernal raid.</summary>
    public sealed class InfernalRaidPacingComposition
    {
        public InfernalRaidPacingComposition(
            string compositionId,
            string displayName,
            string heroArchetypeId,
            string entryPrerequisiteGateId,
            string completionPrerequisiteGateId,
            int approximateDurationSeconds,
            IReadOnlyList<InfernalRaidPacingBeat> beats)
        {
            CompositionId = compositionId;
            DisplayName = displayName;
            HeroArchetypeId = heroArchetypeId;
            EntryPrerequisiteGateId = entryPrerequisiteGateId;
            CompletionPrerequisiteGateId = completionPrerequisiteGateId;
            ApproximateDurationSeconds = approximateDurationSeconds;
            Beats = Snapshot(beats);
        }

        public string CompositionId { get; }

        public string DisplayName { get; }

        public string HeroArchetypeId { get; }

        public string EntryPrerequisiteGateId { get; }

        public string CompletionPrerequisiteGateId { get; }

        public int ApproximateDurationSeconds { get; }

        public IReadOnlyList<InfernalRaidPacingBeat> Beats { get; }

        private static IReadOnlyList<InfernalRaidPacingBeat> Snapshot(
            IReadOnlyList<InfernalRaidPacingBeat> beats)
        {
            if (beats == null)
            {
                return Array.AsReadOnly(Array.Empty<InfernalRaidPacingBeat>());
            }

            var copy = new InfernalRaidPacingBeat[beats.Count];
            for (var index = 0; index < beats.Count; index++)
            {
                copy[index] = beats[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// Three authored Infernal raid pacing choices. Consumers must deliberately
    /// select one exact property; this class neither discovers nor resolves content.
    /// </summary>
    public static class StarterInfernalRaidPacingCatalogue
    {
        public const string GuardianEntArchetypeId = "realmraiders.guardian-ent";

        public const string HellhoundArchetypeId = "realmraiders.hellhound";

        public const string InfernalBruteArchetypeId = "realmraiders.infernal-brute";

        public const string FlameTrapContentId = "realmraiders.infernal.flame-trap";

        public const string InfernalHeartContentId = "realmraiders.infernal-heart";

        public const string EntDirectControlGateId = "realmraiders.infernal.ent-direct-control-ready";

        public const string AllHostilesClearedGateId = "realmraiders.infernal.all-hostiles-cleared";

        public const string InfernalBruteDefeatedGateId = "realmraiders.infernal.infernal-brute-defeated";

        public const string EntryTrialHellhoundABeatId = "entry-trial.hellhound-a";

        public const string EntryTrialInfernalHeartBeatId = "entry-trial.infernal-heart";

        public const string RiskRouteHellhoundABeatId = "risk-route.hellhound-a";

        public const string RiskRouteHellhoundBBeatId = "risk-route.hellhound-b";

        public const string RiskRouteFlameChoiceBeatId = "risk-route.flame-choice";

        public const string RiskRouteInfernalHeartBeatId = "risk-route.infernal-heart";

        public const string BruteFinaleHellhoundABeatId = "brute-finale.hellhound-a";

        public const string BruteFinaleHellhoundBBeatId = "brute-finale.hellhound-b";

        public const string BruteFinaleFlameChoiceBeatId = "brute-finale.flame-choice";

        public const string BruteFinaleInfernalBruteBeatId = "brute-finale.infernal-brute";

        public const string BruteFinaleInfernalHeartBeatId = "brute-finale.infernal-heart";

        public static InfernalRaidPacingComposition EntryTrial { get; } =
            new InfernalRaidPacingComposition(
                "realmraiders.infernal-raid.entry-trial",
                "Entry Trial",
                GuardianEntArchetypeId,
                EntDirectControlGateId,
                AllHostilesClearedGateId,
                35,
                new InfernalRaidPacingBeat[]
                {
                    new InfernalRaidPacingBeat(
                        EntryTrialHellhoundABeatId,
                        "Hellhound Trial",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        2,
                        20),
                    new InfernalRaidPacingBeat(
                        EntryTrialInfernalHeartBeatId,
                        "Claim the Infernal Heart",
                        InfernalRaidBeatKind.Objective,
                        InfernalHeartContentId,
                        false,
                        false,
                        AllHostilesClearedGateId,
                        20,
                        35)
                });

        public static InfernalRaidPacingComposition RiskRoute { get; } =
            new InfernalRaidPacingComposition(
                "realmraiders.infernal-raid.risk-route",
                "Risk Route",
                GuardianEntArchetypeId,
                EntDirectControlGateId,
                AllHostilesClearedGateId,
                55,
                new InfernalRaidPacingBeat[]
                {
                    new InfernalRaidPacingBeat(
                        RiskRouteHellhoundABeatId,
                        "First Hellhound",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        2,
                        18),
                    new InfernalRaidPacingBeat(
                        RiskRouteHellhoundBBeatId,
                        "Second Hellhound",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        18,
                        32),
                    new InfernalRaidPacingBeat(
                        RiskRouteFlameChoiceBeatId,
                        "Bypass the Flame",
                        InfernalRaidBeatKind.Hazard,
                        FlameTrapContentId,
                        true,
                        true,
                        EntDirectControlGateId,
                        32,
                        40),
                    new InfernalRaidPacingBeat(
                        RiskRouteInfernalHeartBeatId,
                        "Claim the Infernal Heart",
                        InfernalRaidBeatKind.Objective,
                        InfernalHeartContentId,
                        false,
                        false,
                        AllHostilesClearedGateId,
                        40,
                        55)
                });

        public static InfernalRaidPacingComposition BruteFinale { get; } =
            new InfernalRaidPacingComposition(
                "realmraiders.infernal-raid.brute-finale",
                "Brute Finale",
                GuardianEntArchetypeId,
                EntDirectControlGateId,
                InfernalBruteDefeatedGateId,
                80,
                new InfernalRaidPacingBeat[]
                {
                    new InfernalRaidPacingBeat(
                        BruteFinaleHellhoundABeatId,
                        "First Hellhound",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        2,
                        18),
                    new InfernalRaidPacingBeat(
                        BruteFinaleHellhoundBBeatId,
                        "Second Hellhound",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        18,
                        32),
                    new InfernalRaidPacingBeat(
                        BruteFinaleFlameChoiceBeatId,
                        "Bypass the Flame",
                        InfernalRaidBeatKind.Hazard,
                        FlameTrapContentId,
                        true,
                        true,
                        EntDirectControlGateId,
                        32,
                        42),
                    new InfernalRaidPacingBeat(
                        BruteFinaleInfernalBruteBeatId,
                        "Infernal Brute",
                        InfernalRaidBeatKind.Enemy,
                        InfernalBruteArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        42,
                        70),
                    new InfernalRaidPacingBeat(
                        BruteFinaleInfernalHeartBeatId,
                        "Claim the Infernal Heart",
                        InfernalRaidBeatKind.Objective,
                        InfernalHeartContentId,
                        false,
                        false,
                        InfernalBruteDefeatedGateId,
                        70,
                        80)
                });

        public static IReadOnlyList<InfernalRaidPacingComposition> All { get; } =
            Array.AsReadOnly(new[]
            {
                EntryTrial,
                RiskRoute,
                BruteFinale
            });
    }
}
