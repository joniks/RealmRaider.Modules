using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.InfernalEncounters
{
    /// <summary>Immutable direct-control hero facts for the Infernal Ent trial lane.</summary>
    public sealed class InfernalEntTrialHeroPlacement
    {
        public InfernalEntTrialHeroPlacement(
            string archetypeId,
            float x,
            float z,
            float scale,
            float safeCenterHalfWidth)
        {
            ArchetypeId = archetypeId;
            X = x;
            Z = z;
            Scale = scale;
            SafeCenterHalfWidth = safeCenterHalfWidth;
        }

        public string ArchetypeId { get; }

        public float X { get; }

        public float Z { get; }

        public float Scale { get; }

        public float SafeCenterHalfWidth { get; }
    }

    /// <summary>One ordered enemy or objective point, tied to the pacing catalogue's stable IDs.</summary>
    public sealed class InfernalEntTrialEncounterPoint
    {
        public InfernalEntTrialEncounterPoint(
            int sequence,
            string beatId,
            string contentId,
            float x,
            float z)
        {
            Sequence = sequence;
            BeatId = beatId;
            ContentId = contentId;
            X = x;
            Z = z;
        }

        public int Sequence { get; }

        public string BeatId { get; }

        public string ContentId { get; }

        public float X { get; }

        public float Z { get; }
    }

    /// <summary>One optional Flame Trap point and its concrete bypass/presentation facts.</summary>
    public sealed class InfernalEntTrialHazardPoint
    {
        public InfernalEntTrialHazardPoint(
            int sequence,
            string beatId,
            string contentId,
            float x,
            float z,
            float triggerRadius,
            bool automaticAfterInitialize,
            bool requiresNonBlockingPresentation,
            float leftBypassX,
            float rightBypassX)
        {
            Sequence = sequence;
            BeatId = beatId;
            ContentId = contentId;
            X = x;
            Z = z;
            TriggerRadius = triggerRadius;
            AutomaticAfterInitialize = automaticAfterInitialize;
            RequiresNonBlockingPresentation = requiresNonBlockingPresentation;
            LeftBypassX = leftBypassX;
            RightBypassX = rightBypassX;
        }

        public int Sequence { get; }

        public string BeatId { get; }

        public string ContentId { get; }

        public float X { get; }

        public float Z { get; }

        public float TriggerRadius { get; }

        public bool AutomaticAfterInitialize { get; }

        public bool RequiresNonBlockingPresentation { get; }

        public float LeftBypassX { get; }

        public float RightBypassX { get; }
    }

    /// <summary>
    /// Immutable spatial facts for the Brute Finale trial. It has no authority to
    /// build a lane, instantiate an entity, configure AI, or activate a hazard.
    /// </summary>
    public sealed class InfernalEntTrialSpatialRecipe
    {
        public InfernalEntTrialSpatialRecipe(
            string compositionId,
            float laneHalfWidth,
            InfernalEntTrialHeroPlacement hero,
            IReadOnlyList<InfernalEntTrialEncounterPoint> encounterPoints,
            InfernalEntTrialHazardPoint flameTrap)
        {
            CompositionId = compositionId;
            LaneHalfWidth = laneHalfWidth;
            Hero = hero;
            EncounterPoints = Snapshot(encounterPoints);
            FlameTrap = flameTrap;
        }

        public string CompositionId { get; }

        public float LaneHalfWidth { get; }

        public InfernalEntTrialHeroPlacement Hero { get; }

        public IReadOnlyList<InfernalEntTrialEncounterPoint> EncounterPoints { get; }

        public InfernalEntTrialHazardPoint FlameTrap { get; }

        private static IReadOnlyList<InfernalEntTrialEncounterPoint> Snapshot(
            IReadOnlyList<InfernalEntTrialEncounterPoint> encounterPoints)
        {
            if (encounterPoints == null)
            {
                return Array.AsReadOnly(Array.Empty<InfernalEntTrialEncounterPoint>());
            }

            var copy = new InfernalEntTrialEncounterPoint[encounterPoints.Count];
            for (var index = 0; index < encounterPoints.Count; index++)
            {
                copy[index] = encounterPoints[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>One explicit spatial recipe. Core must deliberately choose and materialize it.</summary>
    public static class StarterInfernalEntTrialSpatialRecipes
    {
        public static InfernalEntTrialSpatialRecipe BruteFinale { get; } =
            new InfernalEntTrialSpatialRecipe(
                StarterInfernalRaidPacingCatalogue.BruteFinale.CompositionId,
                7f,
                new InfernalEntTrialHeroPlacement(
                    StarterInfernalRaidPacingCatalogue.GuardianEntArchetypeId,
                    0f,
                    -30f,
                    1.45f,
                    5.64f),
                new InfernalEntTrialEncounterPoint[]
                {
                    new InfernalEntTrialEncounterPoint(
                        1,
                        StarterInfernalRaidPacingCatalogue.BruteFinaleHellhoundABeatId,
                        StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                        -3.6f,
                        -13f),
                    new InfernalEntTrialEncounterPoint(
                        2,
                        StarterInfernalRaidPacingCatalogue.BruteFinaleHellhoundBBeatId,
                        StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                        3.6f,
                        -7f),
                    new InfernalEntTrialEncounterPoint(
                        4,
                        StarterInfernalRaidPacingCatalogue.BruteFinaleInfernalBruteBeatId,
                        StarterInfernalRaidPacingCatalogue.InfernalBruteArchetypeId,
                        0f,
                        16f),
                    new InfernalEntTrialEncounterPoint(
                        5,
                        StarterInfernalRaidPacingCatalogue.BruteFinaleInfernalHeartBeatId,
                        StarterInfernalRaidPacingCatalogue.InfernalHeartContentId,
                        0f,
                        30f)
                },
                new InfernalEntTrialHazardPoint(
                    3,
                    StarterInfernalRaidPacingCatalogue.BruteFinaleFlameChoiceBeatId,
                    StarterInfernalRaidPacingCatalogue.FlameTrapContentId,
                    0f,
                    2f,
                    2f,
                    true,
                    true,
                    -3.5f,
                    3.5f));
    }
}
