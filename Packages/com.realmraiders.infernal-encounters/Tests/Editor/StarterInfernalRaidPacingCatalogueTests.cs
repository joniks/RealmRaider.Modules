using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.InfernalEncounters.Tests
{
    public sealed class StarterInfernalRaidPacingCatalogueTests
    {
        [Test]
        public void CatalogueExposesTheExactOrderedInfernalRaidChoices()
        {
            Assert.That(StarterInfernalRaidPacingCatalogue.All.Select(
                composition => composition.CompositionId), Is.EqualTo(
                new[]
                {
                    "realmraiders.infernal-raid.entry-trial",
                    "realmraiders.infernal-raid.risk-route",
                    "realmraiders.infernal-raid.brute-finale"
                }));
            Assert.That(StarterInfernalRaidPacingCatalogue.All.Select(
                composition => composition.DisplayName), Is.EqualTo(
                new[]
                {
                    "Entry Trial",
                    "Risk Route",
                    "Brute Finale"
                }));
        }

        [Test]
        public void EntryTrialIsASingleHellhoundIntroductionBeforeTheGatedHeart()
        {
            AssertComposition(
                StarterInfernalRaidPacingCatalogue.EntryTrial,
                35,
                new ExpectedBeat(
                    "entry-trial.hellhound-a",
                    InfernalRaidBeatKind.Enemy,
                    StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                    false,
                    false,
                    StarterInfernalRaidPacingCatalogue.EntDirectControlGateId,
                    2,
                    20),
                new ExpectedBeat(
                    "entry-trial.infernal-heart",
                    InfernalRaidBeatKind.Objective,
                    StarterInfernalRaidPacingCatalogue.InfernalHeartContentId,
                    false,
                    false,
                    StarterInfernalRaidPacingCatalogue.AllHostilesClearedGateId,
                    20,
                    35));
        }

        [Test]
        public void RiskRouteAddsTwoHoundsAndOneOptionalBypassableFlameChoice()
        {
            var composition = StarterInfernalRaidPacingCatalogue.RiskRoute;

            Assert.That(composition.ApproximateDurationSeconds, Is.EqualTo(55));
            Assert.That(composition.Beats.Count, Is.EqualTo(4));
            Assert.That(composition.Beats.Count(beat => beat.Kind == InfernalRaidBeatKind.Enemy),
                Is.EqualTo(2));
            Assert.That(composition.Beats[2].ContentId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.FlameTrapContentId));
            Assert.That(composition.Beats[2].IsOptionalRisk, Is.True);
            Assert.That(composition.Beats[2].IsBypassable, Is.True);
            Assert.That(composition.Beats[3].PrerequisiteGateId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.AllHostilesClearedGateId));
        }

        [Test]
        public void BruteFinaleContainsTheFullSixtyToNinetySecondEnemyAndRiskSequence()
        {
            AssertComposition(
                StarterInfernalRaidPacingCatalogue.BruteFinale,
                80,
                new ExpectedBeat(
                    "brute-finale.hellhound-a",
                    InfernalRaidBeatKind.Enemy,
                    StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                    false,
                    false,
                    StarterInfernalRaidPacingCatalogue.EntDirectControlGateId,
                    2,
                    18),
                new ExpectedBeat(
                    "brute-finale.hellhound-b",
                    InfernalRaidBeatKind.Enemy,
                    StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                    false,
                    false,
                    StarterInfernalRaidPacingCatalogue.EntDirectControlGateId,
                    18,
                    32),
                new ExpectedBeat(
                    "brute-finale.flame-choice",
                    InfernalRaidBeatKind.Hazard,
                    StarterInfernalRaidPacingCatalogue.FlameTrapContentId,
                    true,
                    true,
                    StarterInfernalRaidPacingCatalogue.EntDirectControlGateId,
                    32,
                    42),
                new ExpectedBeat(
                    "brute-finale.infernal-brute",
                    InfernalRaidBeatKind.Enemy,
                    StarterInfernalRaidPacingCatalogue.InfernalBruteArchetypeId,
                    false,
                    false,
                    StarterInfernalRaidPacingCatalogue.EntDirectControlGateId,
                    42,
                    70),
                new ExpectedBeat(
                    "brute-finale.infernal-heart",
                    InfernalRaidBeatKind.Objective,
                    StarterInfernalRaidPacingCatalogue.InfernalHeartContentId,
                    false,
                    false,
                    StarterInfernalRaidPacingCatalogue.InfernalBruteDefeatedGateId,
                    70,
                    80));

            var beats = StarterInfernalRaidPacingCatalogue.BruteFinale.Beats;
            Assert.That(beats.Count(beat => beat.Kind == InfernalRaidBeatKind.Hazard),
                Is.EqualTo(1));
            Assert.That(beats.Count(beat => beat.Kind == InfernalRaidBeatKind.Enemy),
                Is.EqualTo(3));
            Assert.That(beats.Count(beat => beat.ContentId
                == StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId), Is.EqualTo(2));
            Assert.That(beats.Count(beat => beat.ContentId
                == StarterInfernalRaidPacingCatalogue.InfernalBruteArchetypeId), Is.EqualTo(1));
            Assert.That(StarterInfernalRaidPacingCatalogue.BruteFinale.CompletionPrerequisiteGateId,
                Is.EqualTo(StarterInfernalRaidPacingCatalogue.InfernalBruteDefeatedGateId));
            Assert.That(beats.Last().PrerequisiteGateId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.InfernalBruteDefeatedGateId));
        }

        [Test]
        public void EveryPresetUsesTheEntEntryGateAndASequentialGatedHeartObjective()
        {
            foreach (var composition in StarterInfernalRaidPacingCatalogue.All)
            {
                Assert.That(composition.HeroArchetypeId, Is.EqualTo(
                    StarterInfernalRaidPacingCatalogue.GuardianEntArchetypeId));
                Assert.That(composition.EntryPrerequisiteGateId, Is.EqualTo(
                    StarterInfernalRaidPacingCatalogue.EntDirectControlGateId));
                Assert.That(composition.CompletionPrerequisiteGateId, Is.EqualTo(
                    StarterInfernalRaidPacingCatalogue.AllHostilesClearedGateId));
                Assert.That(composition.Beats.Count, Is.GreaterThan(0));
                Assert.That(composition.Beats.Last().Kind, Is.EqualTo(
                    InfernalRaidBeatKind.Objective));
                Assert.That(composition.Beats.Last().ContentId, Is.EqualTo(
                    StarterInfernalRaidPacingCatalogue.InfernalHeartContentId));
                Assert.That(composition.Beats.Last().PrerequisiteGateId, Is.EqualTo(
                    composition.CompletionPrerequisiteGateId));
                Assert.That(composition.Beats.Where(beat => beat.Kind
                    == InfernalRaidBeatKind.Hazard).All(beat => beat.IsOptionalRisk
                    && beat.IsBypassable), Is.True);

                var previousEnd = 0;
                foreach (var beat in composition.Beats)
                {
                    Assert.That(beat.ApproximateStartSeconds, Is.GreaterThanOrEqualTo(
                        previousEnd));
                    Assert.That(beat.ApproximateEndSeconds, Is.GreaterThan(
                        beat.ApproximateStartSeconds));
                    Assert.That(beat.ApproximateEndSeconds, Is.LessThanOrEqualTo(
                        composition.ApproximateDurationSeconds));
                    previousEnd = beat.ApproximateEndSeconds;
                }
            }
        }

        [Test]
        public void ContentAndBeatSnapshotsAreReadOnlyAndStable()
        {
            Assert.That(StarterInfernalRaidPacingCatalogue.BruteFinale,
                Is.SameAs(StarterInfernalRaidPacingCatalogue.BruteFinale));
            Assert.That(StarterInfernalRaidPacingCatalogue.All,
                Is.SameAs(StarterInfernalRaidPacingCatalogue.All));
            Assert.That(StarterInfernalRaidPacingCatalogue.BruteFinale.Beats,
                Is.SameAs(StarterInfernalRaidPacingCatalogue.BruteFinale.Beats));

            var compositions = (IList<InfernalRaidPacingComposition>)
                StarterInfernalRaidPacingCatalogue.All;
            var beats = (IList<InfernalRaidPacingBeat>)
                StarterInfernalRaidPacingCatalogue.BruteFinale.Beats;
            Assert.That(compositions.IsReadOnly, Is.True);
            Assert.That(beats.IsReadOnly, Is.True);
            Assert.That(typeof(InfernalRaidPacingBeat).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(InfernalRaidPacingComposition).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.Throws<NotSupportedException>(() => compositions[0] = null);
            Assert.Throws<NotSupportedException>(() => beats[0] = null);
        }

        [Test]
        public void CompositionSnapshotsCallerProvidedBeatLists()
        {
            var original = new List<InfernalRaidPacingBeat>
            {
                new InfernalRaidPacingBeat(
                    "test.enemy",
                    "Test Enemy",
                    InfernalRaidBeatKind.Enemy,
                    StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                    false,
                    false,
                    StarterInfernalRaidPacingCatalogue.EntDirectControlGateId,
                    1,
                    2)
            };
            var composition = new InfernalRaidPacingComposition(
                "test.composition",
                "Test Composition",
                StarterInfernalRaidPacingCatalogue.GuardianEntArchetypeId,
                StarterInfernalRaidPacingCatalogue.EntDirectControlGateId,
                StarterInfernalRaidPacingCatalogue.AllHostilesClearedGateId,
                2,
                original);

            original[0] = new InfernalRaidPacingBeat(
                "test.replacement",
                "Replacement Enemy",
                InfernalRaidBeatKind.Enemy,
                StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                false,
                false,
                StarterInfernalRaidPacingCatalogue.EntDirectControlGateId,
                1,
                2);

            Assert.That(composition.Beats[0].BeatId, Is.EqualTo("test.enemy"));
            Assert.That(composition.Beats[0].DisplayName, Is.EqualTo("Test Enemy"));
        }

        [Test]
        public void RuntimeAssemblyHasNoUnityOrGameRuntimeReference()
        {
            var dependencies = typeof(StarterInfernalRaidPacingCatalogue).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static void AssertComposition(
            InfernalRaidPacingComposition composition,
            int approximateDurationSeconds,
            params ExpectedBeat[] expectedBeats)
        {
            Assert.That(composition.HeroArchetypeId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.GuardianEntArchetypeId));
            Assert.That(composition.ApproximateDurationSeconds, Is.EqualTo(
                approximateDurationSeconds));
            Assert.That(composition.Beats.Count, Is.EqualTo(expectedBeats.Length));

            for (var index = 0; index < expectedBeats.Length; index++)
            {
                var actual = composition.Beats[index];
                var expected = expectedBeats[index];
                Assert.That(actual.BeatId, Is.EqualTo(expected.BeatId));
                Assert.That(actual.Kind, Is.EqualTo(expected.Kind));
                Assert.That(actual.ContentId, Is.EqualTo(expected.ContentId));
                Assert.That(actual.IsOptionalRisk, Is.EqualTo(expected.IsOptionalRisk));
                Assert.That(actual.IsBypassable, Is.EqualTo(expected.IsBypassable));
                Assert.That(actual.PrerequisiteGateId, Is.EqualTo(expected.PrerequisiteGateId));
                Assert.That(actual.ApproximateStartSeconds, Is.EqualTo(
                    expected.ApproximateStartSeconds));
                Assert.That(actual.ApproximateEndSeconds, Is.EqualTo(
                    expected.ApproximateEndSeconds));
            }
        }

        private sealed class ExpectedBeat
        {
            public ExpectedBeat(
                string beatId,
                InfernalRaidBeatKind kind,
                string contentId,
                bool isOptionalRisk,
                bool isBypassable,
                string prerequisiteGateId,
                int approximateStartSeconds,
                int approximateEndSeconds)
            {
                BeatId = beatId;
                Kind = kind;
                ContentId = contentId;
                IsOptionalRisk = isOptionalRisk;
                IsBypassable = isBypassable;
                PrerequisiteGateId = prerequisiteGateId;
                ApproximateStartSeconds = approximateStartSeconds;
                ApproximateEndSeconds = approximateEndSeconds;
            }

            public string BeatId { get; }

            public InfernalRaidBeatKind Kind { get; }

            public string ContentId { get; }

            public bool IsOptionalRisk { get; }

            public bool IsBypassable { get; }

            public string PrerequisiteGateId { get; }

            public int ApproximateStartSeconds { get; }

            public int ApproximateEndSeconds { get; }
        }
    }
}
