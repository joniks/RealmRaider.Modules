using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.InfernalEncounters.Tests
{
    public sealed class InfernalEntTrialSpatialRecipeTests
    {
        [Test]
        public void BruteFinaleMapsEveryPacingBeatExactlyOnceInEncounterOrder()
        {
            var recipe = StarterInfernalEntTrialSpatialRecipes.BruteFinale;
            var orderedPoints = recipe.EncounterPoints
                .Select(point => new OrderedBeat(point.Sequence, point.BeatId, point.ContentId))
                .Concat(new[]
                {
                    new OrderedBeat(
                        recipe.FlameTrap.Sequence,
                        recipe.FlameTrap.BeatId,
                        recipe.FlameTrap.ContentId)
                })
                .OrderBy(point => point.Sequence)
                .ToArray();

            Assert.That(recipe.CompositionId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.BruteFinale.CompositionId));
            Assert.That(orderedPoints.Select(point => point.Sequence), Is.EqualTo(
                new[]
                {
                    1,
                    2,
                    3,
                    4,
                    5
                }));
            Assert.That(orderedPoints.Select(point => point.BeatId), Is.EqualTo(
                new[]
                {
                    StarterInfernalRaidPacingCatalogue.BruteFinaleHellhoundABeatId,
                    StarterInfernalRaidPacingCatalogue.BruteFinaleHellhoundBBeatId,
                    StarterInfernalRaidPacingCatalogue.BruteFinaleFlameChoiceBeatId,
                    StarterInfernalRaidPacingCatalogue.BruteFinaleInfernalBruteBeatId,
                    StarterInfernalRaidPacingCatalogue.BruteFinaleInfernalHeartBeatId
                }));
            Assert.That(orderedPoints.Select(point => point.ContentId), Is.EqualTo(
                new[]
                {
                    StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                    StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                    StarterInfernalRaidPacingCatalogue.FlameTrapContentId,
                    StarterInfernalRaidPacingCatalogue.InfernalBruteArchetypeId,
                    StarterInfernalRaidPacingCatalogue.InfernalHeartContentId
                }));
        }

        [Test]
        public void BruteFinaleUsesTheExactEntEnemyHazardAndHeartCoordinates()
        {
            var recipe = StarterInfernalEntTrialSpatialRecipes.BruteFinale;

            Assert.That(recipe.LaneHalfWidth, Is.EqualTo(7f));
            Assert.That(recipe.Hero.ArchetypeId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.GuardianEntArchetypeId));
            Assert.That(recipe.Hero.X, Is.EqualTo(0f));
            Assert.That(recipe.Hero.Z, Is.EqualTo(-30f));
            Assert.That(recipe.Hero.Scale, Is.EqualTo(1.45f));
            Assert.That(recipe.Hero.SafeCenterHalfWidth, Is.EqualTo(5.64f));

            AssertPoint(recipe.EncounterPoints[0], -3.6f, -13f);
            AssertPoint(recipe.EncounterPoints[1], 3.6f, -7f);
            AssertPoint(recipe.EncounterPoints[2], 0f, 16f);
            AssertPoint(recipe.EncounterPoints[3], 0f, 30f);
            Assert.That(recipe.FlameTrap.X, Is.EqualTo(0f));
            Assert.That(recipe.FlameTrap.Z, Is.EqualTo(2f));
            Assert.That(recipe.FlameTrap.TriggerRadius, Is.EqualTo(2f));
            Assert.That(recipe.FlameTrap.AutomaticAfterInitialize, Is.True);
            Assert.That(recipe.FlameTrap.RequiresNonBlockingPresentation, Is.True);
            Assert.That(recipe.FlameTrap.LeftBypassX, Is.EqualTo(-3.5f));
            Assert.That(recipe.FlameTrap.RightBypassX, Is.EqualTo(3.5f));
        }

        [Test]
        public void BruteFinaleHasExactlyOneHazardAndBothBypassCorridorsAreSafe()
        {
            var recipe = StarterInfernalEntTrialSpatialRecipes.BruteFinale;
            var hazards = new[]
            {
                recipe.FlameTrap
            };

            Assert.That(hazards.Length, Is.EqualTo(1));
            Assert.That(hazards[0].ContentId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.FlameTrapContentId));
            Assert.That(hazards[0].RequiresNonBlockingPresentation, Is.True);

            foreach (var bypassX in new[]
            {
                recipe.FlameTrap.LeftBypassX,
                recipe.FlameTrap.RightBypassX
            })
            {
                Assert.That(Math.Abs(bypassX - recipe.FlameTrap.X), Is.GreaterThan(
                    recipe.FlameTrap.TriggerRadius));
                Assert.That(Math.Abs(bypassX), Is.LessThanOrEqualTo(
                    recipe.Hero.SafeCenterHalfWidth));
            }
        }

        [Test]
        public void EveryRequiredPlacementStaysInsideTheSafeLane()
        {
            var recipe = StarterInfernalEntTrialSpatialRecipes.BruteFinale;

            Assert.That(Math.Abs(recipe.Hero.X), Is.LessThanOrEqualTo(
                recipe.Hero.SafeCenterHalfWidth));
            Assert.That(recipe.Hero.SafeCenterHalfWidth, Is.LessThanOrEqualTo(
                recipe.LaneHalfWidth));

            foreach (var point in recipe.EncounterPoints)
            {
                Assert.That(Math.Abs(point.X), Is.LessThanOrEqualTo(recipe.LaneHalfWidth));
                Assert.That(float.IsNaN(point.X), Is.False);
                Assert.That(float.IsInfinity(point.X), Is.False);
                Assert.That(float.IsNaN(point.Z), Is.False);
                Assert.That(float.IsInfinity(point.Z), Is.False);
            }

            Assert.That(Math.Abs(recipe.FlameTrap.X), Is.LessThanOrEqualTo(
                recipe.LaneHalfWidth));
            Assert.That(Math.Abs(recipe.FlameTrap.LeftBypassX), Is.LessThanOrEqualTo(
                recipe.Hero.SafeCenterHalfWidth));
            Assert.That(Math.Abs(recipe.FlameTrap.RightBypassX), Is.LessThanOrEqualTo(
                recipe.Hero.SafeCenterHalfWidth));
        }

        [Test]
        public void SpatialRecipeAndEncounterSnapshotAreReadOnlyAndStable()
        {
            Assert.That(StarterInfernalEntTrialSpatialRecipes.BruteFinale,
                Is.SameAs(StarterInfernalEntTrialSpatialRecipes.BruteFinale));
            Assert.That(StarterInfernalEntTrialSpatialRecipes.BruteFinale.EncounterPoints,
                Is.SameAs(StarterInfernalEntTrialSpatialRecipes.BruteFinale.EncounterPoints));

            var points = (IList<InfernalEntTrialEncounterPoint>)
                StarterInfernalEntTrialSpatialRecipes.BruteFinale.EncounterPoints;
            Assert.That(points.IsReadOnly, Is.True);
            Assert.That(typeof(InfernalEntTrialHeroPlacement).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(InfernalEntTrialEncounterPoint).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(InfernalEntTrialHazardPoint).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(InfernalEntTrialSpatialRecipe).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.Throws<NotSupportedException>(() => points[0] = null);
        }

        [Test]
        public void SpatialRecipeSnapshotsCallerProvidedEncounterPoints()
        {
            var original = new List<InfernalEntTrialEncounterPoint>
            {
                new InfernalEntTrialEncounterPoint(
                    1,
                    "test.beat",
                    StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                    0f,
                    0f)
            };
            var recipe = new InfernalEntTrialSpatialRecipe(
                "test.composition",
                7f,
                new InfernalEntTrialHeroPlacement(
                    StarterInfernalRaidPacingCatalogue.GuardianEntArchetypeId,
                    0f,
                    -30f,
                    1.45f,
                    5.64f),
                original,
                new InfernalEntTrialHazardPoint(
                    2,
                    "test.hazard",
                    StarterInfernalRaidPacingCatalogue.FlameTrapContentId,
                    0f,
                    2f,
                    2f,
                    true,
                    true,
                    -3.5f,
                    3.5f));

            original[0] = new InfernalEntTrialEncounterPoint(
                1,
                "test.replacement",
                StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                1f,
                1f);

            Assert.That(recipe.EncounterPoints[0].BeatId, Is.EqualTo("test.beat"));
            Assert.That(recipe.EncounterPoints[0].X, Is.EqualTo(0f));
        }

        [Test]
        public void RuntimeAssemblyHasNoUnityOrGameRuntimeReference()
        {
            var dependencies = typeof(InfernalEntTrialSpatialRecipe).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static void AssertPoint(
            InfernalEntTrialEncounterPoint point,
            float expectedX,
            float expectedZ)
        {
            Assert.That(point.X, Is.EqualTo(expectedX));
            Assert.That(point.Z, Is.EqualTo(expectedZ));
        }

        private sealed class OrderedBeat
        {
            public OrderedBeat(
                int sequence,
                string beatId,
                string contentId)
            {
                Sequence = sequence;
                BeatId = beatId;
                ContentId = contentId;
            }

            public int Sequence { get; }

            public string BeatId { get; }

            public string ContentId { get; }
        }
    }
}
