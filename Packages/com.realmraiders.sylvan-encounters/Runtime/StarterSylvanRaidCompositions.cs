using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.SylvanEncounters
{
    /// <summary>
    /// Immutable spawn facts for one Core-authored raid run. Core retains authority
    /// over selection, placement, entities, rewards, and all combat behavior.
    /// </summary>
    public sealed class SylvanRaidSpawn
    {
        public SylvanRaidSpawn(
            string spawnId,
            string displayName,
            string archetypeId,
            string nodeId,
            float localOffsetX,
            float localOffsetZ,
            float scale)
        {
            SpawnId = spawnId;
            DisplayName = displayName;
            ArchetypeId = archetypeId;
            NodeId = nodeId;
            LocalOffsetX = localOffsetX;
            LocalOffsetZ = localOffsetZ;
            Scale = scale;
        }

        public string SpawnId { get; }

        public string DisplayName { get; }

        public string ArchetypeId { get; }

        public string NodeId { get; }

        public float LocalOffsetX { get; }

        public float LocalOffsetZ { get; }

        public float Scale { get; }
    }

    /// <summary>One fixed full-raid composition with an immutable spawn snapshot.</summary>
    public sealed class SylvanRaidComposition
    {
        public SylvanRaidComposition(
            string compositionId,
            string displayName,
            IReadOnlyList<SylvanRaidSpawn> spawns)
        {
            CompositionId = compositionId;
            DisplayName = displayName;
            Spawns = Snapshot(spawns);
        }

        public string CompositionId { get; }

        public string DisplayName { get; }

        public IReadOnlyList<SylvanRaidSpawn> Spawns { get; }

        private static IReadOnlyList<SylvanRaidSpawn> Snapshot(
            IReadOnlyList<SylvanRaidSpawn> spawns)
        {
            if (spawns == null)
            {
                return Array.AsReadOnly(Array.Empty<SylvanRaidSpawn>());
            }

            var copy = new SylvanRaidSpawn[spawns.Count];
            for (var index = 0; index < spawns.Count; index++)
            {
                copy[index] = spawns[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// The three explicitly authored Sylvan runs. Consumers must choose one of
    /// these exact properties; this package does not discover or select content.
    /// </summary>
    public static class StarterSylvanRaidCompositions
    {
        public const string SylvanWolfArchetypeId = "realmraiders.sylvan-wolf";

        public const string GuardianEntArchetypeId = "realmraiders.guardian-ent";

        public const string WolfGroveNodeId = "Wolf Grove";

        public const string EntGroveNodeId = "Ent Grove";

        public const string MoonwellNodeId = "Moonwell";

        public static SylvanRaidComposition Baseline { get; } = new SylvanRaidComposition(
            "realmraiders.sylvan-raid.baseline",
            "Baseline",
            new SylvanRaidSpawn[]
            {
                new SylvanRaidSpawn(
                    "baseline.wolf-grove.alpha",
                    "Wolf Alpha",
                    SylvanWolfArchetypeId,
                    WolfGroveNodeId,
                    1f,
                    -1f,
                    0.75f),
                new SylvanRaidSpawn(
                    "baseline.wolf-grove.scout",
                    "Wolf Scout",
                    SylvanWolfArchetypeId,
                    WolfGroveNodeId,
                    -1.5f,
                    1.7f,
                    0.68f),
                new SylvanRaidSpawn(
                    "baseline.ent-grove.guardian",
                    "Sylvan Ent",
                    GuardianEntArchetypeId,
                    EntGroveNodeId,
                    0f,
                    0f,
                    1.45f)
            });

        public static SylvanRaidComposition WolfPressure { get; } = new SylvanRaidComposition(
            "realmraiders.sylvan-raid.wolf-pressure",
            "Wolf Pressure",
            new SylvanRaidSpawn[]
            {
                new SylvanRaidSpawn(
                    "wolf-pressure.wolf-grove.alpha",
                    "Wolf Alpha",
                    SylvanWolfArchetypeId,
                    WolfGroveNodeId,
                    1f,
                    -1f,
                    0.75f),
                new SylvanRaidSpawn(
                    "wolf-pressure.wolf-grove.hunter",
                    "Wolf Hunter",
                    SylvanWolfArchetypeId,
                    WolfGroveNodeId,
                    -2f,
                    1.5f,
                    0.68f),
                new SylvanRaidSpawn(
                    "wolf-pressure.ent-grove.guardian",
                    "Sylvan Ent",
                    GuardianEntArchetypeId,
                    EntGroveNodeId,
                    0f,
                    0f,
                    1.45f),
                new SylvanRaidSpawn(
                    "wolf-pressure.moonwell.stalker",
                    "Moonwell Wolf",
                    SylvanWolfArchetypeId,
                    MoonwellNodeId,
                    -1.8f,
                    1.5f,
                    0.68f)
            });

        public static SylvanRaidComposition SentinelEscort { get; } = new SylvanRaidComposition(
            "realmraiders.sylvan-raid.sentinel-escort",
            "Sentinel Escort",
            new SylvanRaidSpawn[]
            {
                new SylvanRaidSpawn(
                    "sentinel-escort.wolf-grove.scout",
                    "Wolf Scout",
                    SylvanWolfArchetypeId,
                    WolfGroveNodeId,
                    1f,
                    -1f,
                    0.75f),
                new SylvanRaidSpawn(
                    "sentinel-escort.ent-grove.sentinel",
                    "Ent Sentinel",
                    GuardianEntArchetypeId,
                    EntGroveNodeId,
                    -1.1f,
                    0f,
                    1.45f),
                new SylvanRaidSpawn(
                    "sentinel-escort.ent-grove.escort",
                    "Ent Grove Wolf",
                    SylvanWolfArchetypeId,
                    EntGroveNodeId,
                    1.8f,
                    1.4f,
                    0.68f)
            });

        public static IReadOnlyList<SylvanRaidComposition> All { get; } =
            Array.AsReadOnly(new[]
            {
                Baseline,
                WolfPressure,
                SentinelEscort
            });
    }
}
