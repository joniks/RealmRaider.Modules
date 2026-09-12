using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.SylvanEncounters.Tests
{
    public sealed class StarterSylvanRaidCompositionsTests
    {
        private const float NodeRadius = 3.25f;
        private const float WolfBaseCapsuleRadius = 0.5f;
        private const float EntBaseCapsuleRadius = 0.8f;
        private const float CapsuleClearance = 0.2f;

        [Test]
        public void CompositionsExposeTheExactOrderedFullRaidChoices()
        {
            var compositions = StarterSylvanRaidCompositions.All;

            Assert.That(compositions.Select(composition => composition.CompositionId), Is.EqualTo(
                new[]
                {
                    "realmraiders.sylvan-raid.baseline",
                    "realmraiders.sylvan-raid.wolf-pressure",
                    "realmraiders.sylvan-raid.sentinel-escort"
                }));
            Assert.That(compositions.Select(composition => composition.DisplayName), Is.EqualTo(
                new[]
                {
                    "Baseline",
                    "Wolf Pressure",
                    "Sentinel Escort"
                }));
        }

        [Test]
        public void BaselinePreservesTheCurrentEncounterFactsWithASafeScoutCorrection()
        {
            AssertComposition(
                StarterSylvanRaidCompositions.Baseline,
                "realmraiders.sylvan-raid.baseline",
                new ExpectedSpawn(
                    "baseline.wolf-grove.alpha",
                    "Wolf Alpha",
                    StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                    StarterSylvanRaidCompositions.WolfGroveNodeId,
                    1f,
                    -1f,
                    0.75f),
                new ExpectedSpawn(
                    "baseline.wolf-grove.scout",
                    "Wolf Scout",
                    StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                    StarterSylvanRaidCompositions.WolfGroveNodeId,
                    -1.5f,
                    1.7f,
                    0.68f),
                new ExpectedSpawn(
                    "baseline.ent-grove.guardian",
                    "Sylvan Ent",
                    StarterSylvanRaidCompositions.GuardianEntArchetypeId,
                    StarterSylvanRaidCompositions.EntGroveNodeId,
                    0f,
                    0f,
                    1.45f));
        }

        [Test]
        public void WolfPressureUsesTwoWolfGroveWolvesAnEntAndOneMoonwellWolf()
        {
            AssertComposition(
                StarterSylvanRaidCompositions.WolfPressure,
                "realmraiders.sylvan-raid.wolf-pressure",
                new ExpectedSpawn(
                    "wolf-pressure.wolf-grove.alpha",
                    "Wolf Alpha",
                    StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                    StarterSylvanRaidCompositions.WolfGroveNodeId,
                    1f,
                    -1f,
                    0.75f),
                new ExpectedSpawn(
                    "wolf-pressure.wolf-grove.hunter",
                    "Wolf Hunter",
                    StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                    StarterSylvanRaidCompositions.WolfGroveNodeId,
                    -2f,
                    1.5f,
                    0.68f),
                new ExpectedSpawn(
                    "wolf-pressure.ent-grove.guardian",
                    "Sylvan Ent",
                    StarterSylvanRaidCompositions.GuardianEntArchetypeId,
                    StarterSylvanRaidCompositions.EntGroveNodeId,
                    0f,
                    0f,
                    1.45f),
                new ExpectedSpawn(
                    "wolf-pressure.moonwell.stalker",
                    "Moonwell Wolf",
                    StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                    StarterSylvanRaidCompositions.MoonwellNodeId,
                    -1.8f,
                    1.5f,
                    0.68f));
        }

        [Test]
        public void SentinelEscortUsesOneWolfGroveWolfAndAnEntGrovePair()
        {
            AssertComposition(
                StarterSylvanRaidCompositions.SentinelEscort,
                "realmraiders.sylvan-raid.sentinel-escort",
                new ExpectedSpawn(
                    "sentinel-escort.wolf-grove.scout",
                    "Wolf Scout",
                    StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                    StarterSylvanRaidCompositions.WolfGroveNodeId,
                    1f,
                    -1f,
                    0.75f),
                new ExpectedSpawn(
                    "sentinel-escort.ent-grove.sentinel",
                    "Ent Sentinel",
                    StarterSylvanRaidCompositions.GuardianEntArchetypeId,
                    StarterSylvanRaidCompositions.EntGroveNodeId,
                    -1.1f,
                    0f,
                    1.45f),
                new ExpectedSpawn(
                    "sentinel-escort.ent-grove.escort",
                    "Ent Grove Wolf",
                    StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                    StarterSylvanRaidCompositions.EntGroveNodeId,
                    1.8f,
                    1.4f,
                    0.68f));
        }

        [Test]
        public void EveryCompositionIsACompactSafeRaidOfApprovedArchetypesAndNodes()
        {
            var allowedArchetypes = new[]
            {
                StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                StarterSylvanRaidCompositions.GuardianEntArchetypeId
            };
            var allowedNodes = new[]
            {
                StarterSylvanRaidCompositions.WolfGroveNodeId,
                StarterSylvanRaidCompositions.EntGroveNodeId,
                StarterSylvanRaidCompositions.MoonwellNodeId
            };

            foreach (var composition in StarterSylvanRaidCompositions.All)
            {
                Assert.That(composition.Spawns.Count, Is.GreaterThan(0));
                Assert.That(composition.Spawns.Count, Is.LessThanOrEqualTo(4));
                Assert.That(composition.Spawns.Count(spawn => spawn.ArchetypeId ==
                    StarterSylvanRaidCompositions.GuardianEntArchetypeId), Is.EqualTo(1));
                Assert.That(composition.Spawns.Select(spawn => spawn.SpawnId).Distinct().Count(),
                    Is.EqualTo(composition.Spawns.Count));
                Assert.That(composition.Spawns.Select(spawn => spawn.DisplayName).Distinct().Count(),
                    Is.EqualTo(composition.Spawns.Count));

                foreach (var spawn in composition.Spawns)
                {
                    Assert.That(allowedArchetypes, Has.Member(spawn.ArchetypeId));
                    Assert.That(allowedNodes, Has.Member(spawn.NodeId));
                    Assert.That(float.IsNaN(spawn.LocalOffsetX), Is.False);
                    Assert.That(float.IsInfinity(spawn.LocalOffsetX), Is.False);
                    Assert.That(float.IsNaN(spawn.LocalOffsetZ), Is.False);
                    Assert.That(float.IsInfinity(spawn.LocalOffsetZ), Is.False);
                    Assert.That(float.IsNaN(spawn.Scale), Is.False);
                    Assert.That(float.IsInfinity(spawn.Scale), Is.False);
                    Assert.That(spawn.Scale, Is.GreaterThan(0f));
                    Assert.That(Math.Sqrt(spawn.LocalOffsetX * spawn.LocalOffsetX
                        + spawn.LocalOffsetZ * spawn.LocalOffsetZ)
                        + ScaledCapsuleRadius(spawn) + CapsuleClearance,
                        Is.LessThanOrEqualTo(NodeRadius));
                }

                foreach (var nodeGroup in composition.Spawns.GroupBy(spawn => spawn.NodeId))
                {
                    var nodeSpawns = nodeGroup.ToArray();
                    Assert.That(nodeSpawns.Length, Is.LessThanOrEqualTo(2));

                    for (var first = 0; first < nodeSpawns.Length; first++)
                    {
                        for (var second = first + 1; second < nodeSpawns.Length; second++)
                        {
                            var xDelta = nodeSpawns[first].LocalOffsetX
                                - nodeSpawns[second].LocalOffsetX;
                            var zDelta = nodeSpawns[first].LocalOffsetZ
                                - nodeSpawns[second].LocalOffsetZ;
                            var requiredSeparation = RequiredSpawnSeparation(
                                nodeSpawns[first],
                                nodeSpawns[second]);
                            Assert.That(xDelta * xDelta + zDelta * zDelta,
                                Is.GreaterThanOrEqualTo(
                                    requiredSeparation * requiredSeparation));
                        }
                    }
                }
            }
        }

        [Test]
        public void StaticContentAndSpawnSnapshotsAreReadOnlyAndStable()
        {
            Assert.That(StarterSylvanRaidCompositions.Baseline,
                Is.SameAs(StarterSylvanRaidCompositions.Baseline));
            Assert.That(StarterSylvanRaidCompositions.All,
                Is.SameAs(StarterSylvanRaidCompositions.All));
            Assert.That(StarterSylvanRaidCompositions.Baseline.Spawns,
                Is.SameAs(StarterSylvanRaidCompositions.Baseline.Spawns));

            var all = (IList<SylvanRaidComposition>)StarterSylvanRaidCompositions.All;
            var spawns = (IList<SylvanRaidSpawn>)StarterSylvanRaidCompositions.Baseline.Spawns;
            Assert.That(all.IsReadOnly, Is.True);
            Assert.That(spawns.IsReadOnly, Is.True);
            Assert.That(typeof(SylvanRaidSpawn).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(SylvanRaidComposition).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.Throws<NotSupportedException>(() => all[0] = null);
            Assert.Throws<NotSupportedException>(() => spawns[0] = null);
            Assert.That(StarterSylvanRaidCompositions.Baseline.Spawns[0].SpawnId,
                Is.EqualTo("baseline.wolf-grove.alpha"));
        }

        [Test]
        public void CompositionSnapshotsTheCallerProvidedSpawnList()
        {
            var original = new List<SylvanRaidSpawn>
            {
                new SylvanRaidSpawn(
                    "test.wolf",
                    "Test Wolf",
                    StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                    StarterSylvanRaidCompositions.WolfGroveNodeId,
                    0f,
                    0f,
                    0.68f)
            };
            var composition = new SylvanRaidComposition(
                "test.composition",
                "Test Composition",
                original);

            original[0] = new SylvanRaidSpawn(
                "test.replacement",
                "Replacement Wolf",
                StarterSylvanRaidCompositions.SylvanWolfArchetypeId,
                StarterSylvanRaidCompositions.WolfGroveNodeId,
                0f,
                0f,
                0.75f);

            Assert.That(composition.Spawns[0].SpawnId, Is.EqualTo("test.wolf"));
            Assert.That(composition.Spawns[0].DisplayName, Is.EqualTo("Test Wolf"));
        }

        private static void AssertComposition(
            SylvanRaidComposition composition,
            string compositionId,
            params ExpectedSpawn[] expectedSpawns)
        {
            Assert.That(composition.CompositionId, Is.EqualTo(compositionId));
            Assert.That(composition.Spawns.Count, Is.EqualTo(expectedSpawns.Length));

            for (var index = 0; index < expectedSpawns.Length; index++)
            {
                var actual = composition.Spawns[index];
                var expected = expectedSpawns[index];
                Assert.That(actual.SpawnId, Is.EqualTo(expected.SpawnId));
                Assert.That(actual.DisplayName, Is.EqualTo(expected.DisplayName));
                Assert.That(actual.ArchetypeId, Is.EqualTo(expected.ArchetypeId));
                Assert.That(actual.NodeId, Is.EqualTo(expected.NodeId));
                Assert.That(actual.LocalOffsetX, Is.EqualTo(expected.LocalOffsetX));
                Assert.That(actual.LocalOffsetZ, Is.EqualTo(expected.LocalOffsetZ));
                Assert.That(actual.Scale, Is.EqualTo(expected.Scale));
            }
        }

        private static float ScaledCapsuleRadius(SylvanRaidSpawn spawn)
        {
            return spawn.ArchetypeId == StarterSylvanRaidCompositions.SylvanWolfArchetypeId
                ? WolfBaseCapsuleRadius * spawn.Scale
                : EntBaseCapsuleRadius * spawn.Scale;
        }

        private static float RequiredSpawnSeparation(
            SylvanRaidSpawn first,
            SylvanRaidSpawn second)
        {
            return ScaledCapsuleRadius(first)
                + ScaledCapsuleRadius(second)
                + CapsuleClearance;
        }

        private sealed class ExpectedSpawn
        {
            public ExpectedSpawn(
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
    }
}
