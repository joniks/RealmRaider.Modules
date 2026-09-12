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
                        "entry-trial.hellhound-a",
                        "Hellhound Trial",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        2,
                        20),
                    new InfernalRaidPacingBeat(
                        "entry-trial.infernal-heart",
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
                        "risk-route.hellhound-a",
                        "First Hellhound",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        2,
                        18),
                    new InfernalRaidPacingBeat(
                        "risk-route.hellhound-b",
                        "Second Hellhound",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        18,
                        32),
                    new InfernalRaidPacingBeat(
                        "risk-route.flame-choice",
                        "Bypass the Flame",
                        InfernalRaidBeatKind.Hazard,
                        FlameTrapContentId,
                        true,
                        true,
                        EntDirectControlGateId,
                        32,
                        40),
                    new InfernalRaidPacingBeat(
                        "risk-route.infernal-heart",
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
                        "brute-finale.hellhound-a",
                        "First Hellhound",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        2,
                        18),
                    new InfernalRaidPacingBeat(
                        "brute-finale.hellhound-b",
                        "Second Hellhound",
                        InfernalRaidBeatKind.Enemy,
                        HellhoundArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        18,
                        32),
                    new InfernalRaidPacingBeat(
                        "brute-finale.flame-choice",
                        "Bypass the Flame",
                        InfernalRaidBeatKind.Hazard,
                        FlameTrapContentId,
                        true,
                        true,
                        EntDirectControlGateId,
                        32,
                        42),
                    new InfernalRaidPacingBeat(
                        "brute-finale.infernal-brute",
                        "Infernal Brute",
                        InfernalRaidBeatKind.Enemy,
                        InfernalBruteArchetypeId,
                        false,
                        false,
                        EntDirectControlGateId,
                        42,
                        70),
                    new InfernalRaidPacingBeat(
                        "brute-finale.infernal-heart",
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
