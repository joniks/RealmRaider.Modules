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
        public void CatalogueCoversEachPacingCompositionAndMapsEveryBeatExactlyOnceInOrder()
        {
            var recipes = StarterInfernalEntTrialSpatialRecipes.All;
            var pacingCompositions = StarterInfernalRaidPacingCatalogue.All;

            Assert.That(recipes.Select(recipe => recipe.CompositionId), Is.EqualTo(
                pacingCompositions.Select(composition => composition.CompositionId)));

            for (var index = 0; index < recipes.Count; index++)
            {
                AssertEveryPacingBeatMapsExactlyOnce(recipes[index], pacingCompositions[index]);
            }
        }

        [Test]
        public void EntryTrialUsesNoFlameTrapAndMapsItsHellhoundAndHeart()
        {
            var recipe = StarterInfernalEntTrialSpatialRecipes.EntryTrial;

            Assert.That(recipe.FlameTrap, Is.Null);
            Assert.That(recipe.EncounterPoints.Count, Is.EqualTo(2));
            Assert.That(recipe.EncounterPoints[0].BeatId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.EntryTrialHellhoundABeatId));
            Assert.That(recipe.EncounterPoints[0].ContentId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId));
            AssertPoint(recipe.EncounterPoints[0], -3.6f, -13f);
            Assert.That(recipe.EncounterPoints[1].BeatId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.EntryTrialInfernalHeartBeatId));
            Assert.That(recipe.EncounterPoints[1].ContentId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.InfernalHeartContentId));
            AssertPoint(recipe.EncounterPoints[1], 0f, 16f);
        }

        [Test]
        public void RiskRouteMapsTwoHellhoundsOneBypassableFlameTrapAndHeart()
        {
            var recipe = StarterInfernalEntTrialSpatialRecipes.RiskRoute;

            Assert.That(recipe.EncounterPoints.Count, Is.EqualTo(3));
            Assert.That(recipe.EncounterPoints[0].BeatId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.RiskRouteHellhoundABeatId));
            AssertPoint(recipe.EncounterPoints[0], -3.6f, -13f);
            Assert.That(recipe.EncounterPoints[1].BeatId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.RiskRouteHellhoundBBeatId));
            AssertPoint(recipe.EncounterPoints[1], 3.6f, -7f);
            Assert.That(recipe.EncounterPoints[2].BeatId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.RiskRouteInfernalHeartBeatId));
            AssertPoint(recipe.EncounterPoints[2], 0f, 22f);
            Assert.That(recipe.FlameTrap, Is.Not.Null);
            Assert.That(recipe.FlameTrap.BeatId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.RiskRouteFlameChoiceBeatId));
            Assert.That(recipe.FlameTrap.ContentId, Is.EqualTo(
                StarterInfernalRaidPacingCatalogue.FlameTrapContentId));
            Assert.That(recipe.FlameTrap.Sequence, Is.EqualTo(3));
            Assert.That(recipe.FlameTrap.X, Is.EqualTo(0f));
            Assert.That(recipe.FlameTrap.Z, Is.EqualTo(2f));
            Assert.That(recipe.FlameTrap.TriggerRadius, Is.EqualTo(2f));
            Assert.That(recipe.FlameTrap.AutomaticAfterInitialize, Is.True);
            Assert.That(recipe.FlameTrap.RequiresNonBlockingPresentation, Is.True);
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
        public void EveryRequiredPlacementStaysInsideTheSafeLaneAndHazardsDoNotOverlapSpawns()
        {
            foreach (var recipe in StarterInfernalEntTrialSpatialRecipes.All)
            {
                Assert.That(recipe.Hero.ArchetypeId, Is.EqualTo(
                    StarterInfernalRaidPacingCatalogue.GuardianEntArchetypeId));
                Assert.That(recipe.Hero.X, Is.EqualTo(0f));
                Assert.That(recipe.Hero.Z, Is.EqualTo(-30f));
                Assert.That(recipe.Hero.Scale, Is.EqualTo(1.45f));
                Assert.That(Math.Abs(recipe.Hero.X), Is.LessThanOrEqualTo(
                    recipe.Hero.SafeCenterHalfWidth));
                Assert.That(recipe.Hero.SafeCenterHalfWidth, Is.LessThanOrEqualTo(
                    recipe.LaneHalfWidth));

                foreach (var point in recipe.EncounterPoints)
                {
                    Assert.That(Math.Abs(point.X), Is.LessThanOrEqualTo(recipe.LaneHalfWidth));
                    AssertFinite(point.X);
                    AssertFinite(point.Z);
                }

                if (recipe.FlameTrap != null)
                {
                    AssertHazardIsBypassableAndDoesNotOverlapSpawns(recipe);
                }
            }
        }

        [Test]
        public void SpatialRecipeAndEncounterSnapshotAreReadOnlyAndStable()
        {
            Assert.That(StarterInfernalEntTrialSpatialRecipes.All,
                Is.SameAs(StarterInfernalEntTrialSpatialRecipes.All));
            Assert.That(StarterInfernalEntTrialSpatialRecipes.EntryTrial,
                Is.SameAs(StarterInfernalEntTrialSpatialRecipes.EntryTrial));
            Assert.That(StarterInfernalEntTrialSpatialRecipes.RiskRoute,
                Is.SameAs(StarterInfernalEntTrialSpatialRecipes.RiskRoute));
            Assert.That(StarterInfernalEntTrialSpatialRecipes.BruteFinale,
                Is.SameAs(StarterInfernalEntTrialSpatialRecipes.BruteFinale));
            Assert.That(StarterInfernalEntTrialSpatialRecipes.BruteFinale.EncounterPoints,
                Is.SameAs(StarterInfernalEntTrialSpatialRecipes.BruteFinale.EncounterPoints));

            var points = (IList<InfernalEntTrialEncounterPoint>)
                StarterInfernalEntTrialSpatialRecipes.BruteFinale.EncounterPoints;
            var recipes = (IList<InfernalEntTrialSpatialRecipe>)
                StarterInfernalEntTrialSpatialRecipes.All;
            Assert.That(points.IsReadOnly, Is.True);
            Assert.That(recipes.IsReadOnly, Is.True);
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
            Assert.Throws<NotSupportedException>(() => recipes[0] = null);
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

        private static void AssertEveryPacingBeatMapsExactlyOnce(
            InfernalEntTrialSpatialRecipe recipe,
            InfernalRaidPacingComposition pacingComposition)
        {
            var orderedSpatialBeats = GetOrderedSpatialBeats(recipe);

            Assert.That(orderedSpatialBeats.Count, Is.EqualTo(pacingComposition.Beats.Count));
            Assert.That(orderedSpatialBeats.Select(beat => beat.Sequence), Is.EqualTo(
                Enumerable.Range(1, pacingComposition.Beats.Count)));
            Assert.That(orderedSpatialBeats.Select(beat => beat.BeatId), Is.EqualTo(
                pacingComposition.Beats.Select(beat => beat.BeatId)));
            Assert.That(orderedSpatialBeats.Select(beat => beat.ContentId), Is.EqualTo(
                pacingComposition.Beats.Select(beat => beat.ContentId)));
        }

        private static IReadOnlyList<OrderedBeat> GetOrderedSpatialBeats(
            InfernalEntTrialSpatialRecipe recipe)
        {
            var beats = new List<OrderedBeat>();

            foreach (var point in recipe.EncounterPoints)
            {
                beats.Add(new OrderedBeat(point.Sequence, point.BeatId, point.ContentId));
            }

            if (recipe.FlameTrap != null)
            {
                beats.Add(new OrderedBeat(
                    recipe.FlameTrap.Sequence,
                    recipe.FlameTrap.BeatId,
                    recipe.FlameTrap.ContentId));
            }

            return beats.OrderBy(beat => beat.Sequence).ToArray();
        }

        private static void AssertHazardIsBypassableAndDoesNotOverlapSpawns(
            InfernalEntTrialSpatialRecipe recipe)
        {
            var hazard = recipe.FlameTrap;

            Assert.That(Math.Abs(hazard.X), Is.LessThanOrEqualTo(recipe.LaneHalfWidth));
            AssertFinite(hazard.X);
            AssertFinite(hazard.Z);
            Assert.That(hazard.TriggerRadius, Is.GreaterThan(0f));

            foreach (var bypassX in new[]
            {
                hazard.LeftBypassX,
                hazard.RightBypassX
            })
            {
                Assert.That(Math.Abs(bypassX - hazard.X), Is.GreaterThan(
                    hazard.TriggerRadius));
                Assert.That(Math.Abs(bypassX), Is.LessThanOrEqualTo(
                    recipe.Hero.SafeCenterHalfWidth));
            }

            foreach (var point in recipe.EncounterPoints)
            {
                var deltaX = point.X - hazard.X;
                var deltaZ = point.Z - hazard.Z;
                var squaredDistance = (deltaX * deltaX) + (deltaZ * deltaZ);

                Assert.That(squaredDistance, Is.GreaterThan(
                    hazard.TriggerRadius * hazard.TriggerRadius));
            }
        }

        private static void AssertFinite(float value)
        {
            Assert.That(float.IsNaN(value), Is.False);
            Assert.That(float.IsInfinity(value), Is.False);
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
