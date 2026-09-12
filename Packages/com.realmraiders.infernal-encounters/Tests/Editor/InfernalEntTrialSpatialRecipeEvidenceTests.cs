using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.InfernalEncounters.Tests
{
    public sealed class InfernalEntTrialSpatialRecipeEvidenceTests
    {
        [Test]
        public void FindByCompositionIdReturnsTheExactCachedRecipeUsingOrdinalIds()
        {
            foreach (var expectedRecipe in StarterInfernalEntTrialSpatialRecipes.All)
            {
                var result = InfernalEntTrialSpatialRecipeEvidence.FindByCompositionId(
                    expectedRecipe.CompositionId);

                Assert.That(result.Status, Is.EqualTo(
                    InfernalEntTrialSpatialRecipeLookupStatus.Found));
                Assert.That(result.Found, Is.True);
                Assert.That(result.Recipe, Is.SameAs(expectedRecipe));
            }

            var caseMismatch = InfernalEntTrialSpatialRecipeEvidence.FindByCompositionId(
                StarterInfernalRaidPacingCatalogue.EntryTrial.CompositionId.ToUpperInvariant());

            Assert.That(caseMismatch.Status, Is.EqualTo(
                InfernalEntTrialSpatialRecipeLookupStatus.NotFound));
            Assert.That(caseMismatch.Found, Is.False);
            Assert.That(caseMismatch.Recipe, Is.Null);
        }

        [Test]
        public void FindByCompositionIdFailsClosedForEmptyAndUnknownIds()
        {
            foreach (var compositionId in new[]
            {
                null,
                string.Empty,
                " ",
                "realmraiders.infernal-raid.unknown"
            })
            {
                var result = InfernalEntTrialSpatialRecipeEvidence.FindByCompositionId(compositionId);

                Assert.That(result.Found, Is.False);
                Assert.That(result.Recipe, Is.Null);
                Assert.That(result.Status, Is.EqualTo(string.IsNullOrWhiteSpace(compositionId)
                    ? InfernalEntTrialSpatialRecipeLookupStatus.InvalidCompositionId
                    : InfernalEntTrialSpatialRecipeLookupStatus.NotFound));
            }
        }

        [Test]
        public void ValidateAcceptsEveryCachedRecipeWithStableEmptyEvidence()
        {
            foreach (var recipe in StarterInfernalEntTrialSpatialRecipes.All)
            {
                var first = InfernalEntTrialSpatialRecipeEvidence.Validate(recipe);
                var second = InfernalEntTrialSpatialRecipeEvidence.Validate(recipe);

                Assert.That(first.IsValid, Is.True);
                Assert.That(first.Issues, Is.Empty);
                Assert.That(second.IsValid, Is.EqualTo(first.IsValid));
                Assert.That(second.Issues, Is.EqualTo(first.Issues));
            }
        }

        [Test]
        public void ValidateFailsClosedForMissingRecipeAndUnknownComposition()
        {
            var missingRecipe = InfernalEntTrialSpatialRecipeEvidence.Validate(null);
            var emptyComposition = InfernalEntTrialSpatialRecipeEvidence.Validate(
                CreateEntryTrialRecipe(string.Empty));
            var unknownComposition = InfernalEntTrialSpatialRecipeEvidence.Validate(
                CreateEntryTrialRecipe("realmraiders.infernal-raid.unknown"));

            Assert.That(missingRecipe.IsValid, Is.False);
            Assert.That(missingRecipe.Issues, Is.EqualTo(new[]
            {
                InfernalEntTrialSpatialRecipeValidationIssue.RecipeMissing
            }));
            Assert.That(emptyComposition.IsValid, Is.False);
            Assert.That(emptyComposition.Issues, Is.EqualTo(new[]
            {
                InfernalEntTrialSpatialRecipeValidationIssue.CompositionIdInvalid
            }));
            Assert.That(unknownComposition.IsValid, Is.False);
            Assert.That(unknownComposition.Issues, Is.EqualTo(new[]
            {
                InfernalEntTrialSpatialRecipeValidationIssue.PacingCompositionNotFound
            }));
        }

        [Test]
        public void ValidateReportsMalformedPlacementsAndMappingInStableOrder()
        {
            var recipe = new InfernalEntTrialSpatialRecipe(
                StarterInfernalRaidPacingCatalogue.EntryTrial.CompositionId,
                float.NaN,
                null,
                new InfernalEntTrialEncounterPoint[]
                {
                    null
                },
                new InfernalEntTrialHazardPoint(
                    2,
                    StarterInfernalRaidPacingCatalogue.EntryTrialInfernalHeartBeatId,
                    StarterInfernalRaidPacingCatalogue.InfernalHeartContentId,
                    0f,
                    2f,
                    2f,
                    true,
                    true,
                    -3.5f,
                    3.5f));

            var result = InfernalEntTrialSpatialRecipeEvidence.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues, Is.EqualTo(new[]
            {
                InfernalEntTrialSpatialRecipeValidationIssue.LaneHalfWidthInvalid,
                InfernalEntTrialSpatialRecipeValidationIssue.HeroMissing,
                InfernalEntTrialSpatialRecipeValidationIssue.EncounterPointPlacementInvalid,
                InfernalEntTrialSpatialRecipeValidationIssue.BeatMappingInvalid,
                InfernalEntTrialSpatialRecipeValidationIssue.HazardCardinalityInvalid
            }));
        }

        [Test]
        public void ValidateRejectsNonGuardianHeroAndNonFiniteOrOutOfLaneEncounterPoints()
        {
            var invalidHero = new InfernalEntTrialHeroPlacement(
                StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                0f,
                -30f,
                1.45f,
                5.64f);
            var invalidPoint = new InfernalEntTrialEncounterPoint(
                1,
                StarterInfernalRaidPacingCatalogue.EntryTrialHellhoundABeatId,
                StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                float.PositiveInfinity,
                -13f);
            var recipe = new InfernalEntTrialSpatialRecipe(
                StarterInfernalRaidPacingCatalogue.EntryTrial.CompositionId,
                7f,
                invalidHero,
                new[]
                {
                    invalidPoint,
                    StarterInfernalEntTrialSpatialRecipes.EntryTrial.EncounterPoints[1]
                },
                null);

            var result = InfernalEntTrialSpatialRecipeEvidence.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues, Is.EqualTo(new[]
            {
                InfernalEntTrialSpatialRecipeValidationIssue.HeroArchetypeInvalid,
                InfernalEntTrialSpatialRecipeValidationIssue.EncounterPointPlacementInvalid
            }));
        }

        [Test]
        public void ValidateRejectsMissingRiskRouteHazardAndSameSideBypassFacts()
        {
            var missingHazard = new InfernalEntTrialSpatialRecipe(
                StarterInfernalRaidPacingCatalogue.RiskRoute.CompositionId,
                7f,
                StarterInfernalEntTrialSpatialRecipes.RiskRoute.Hero,
                StarterInfernalEntTrialSpatialRecipes.RiskRoute.EncounterPoints,
                null);
            var sameSideBypass = new InfernalEntTrialSpatialRecipe(
                StarterInfernalRaidPacingCatalogue.RiskRoute.CompositionId,
                7f,
                StarterInfernalEntTrialSpatialRecipes.RiskRoute.Hero,
                StarterInfernalEntTrialSpatialRecipes.RiskRoute.EncounterPoints,
                new InfernalEntTrialHazardPoint(
                    3,
                    StarterInfernalRaidPacingCatalogue.RiskRouteFlameChoiceBeatId,
                    StarterInfernalRaidPacingCatalogue.FlameTrapContentId,
                    0f,
                    2f,
                    2f,
                    true,
                    true,
                    3.5f,
                    4.5f));

            var missingHazardResult = InfernalEntTrialSpatialRecipeEvidence.Validate(missingHazard);
            var sameSideBypassResult = InfernalEntTrialSpatialRecipeEvidence.Validate(
                sameSideBypass);

            Assert.That(missingHazardResult.IsValid, Is.False);
            Assert.That(missingHazardResult.Issues, Is.EqualTo(new[]
            {
                InfernalEntTrialSpatialRecipeValidationIssue.BeatMappingInvalid,
                InfernalEntTrialSpatialRecipeValidationIssue.HazardCardinalityInvalid
            }));
            Assert.That(sameSideBypassResult.IsValid, Is.False);
            Assert.That(sameSideBypassResult.Issues, Is.EqualTo(new[]
            {
                InfernalEntTrialSpatialRecipeValidationIssue.HazardBypassInvalid
            }));
        }

        [Test]
        public void ValidateRejectsSwappedHazardAndEnemyRepresentations()
        {
            var recipe = new InfernalEntTrialSpatialRecipe(
                StarterInfernalRaidPacingCatalogue.RiskRoute.CompositionId,
                7f,
                StarterInfernalEntTrialSpatialRecipes.RiskRoute.Hero,
                new InfernalEntTrialEncounterPoint[]
                {
                    StarterInfernalEntTrialSpatialRecipes.RiskRoute.EncounterPoints[0],
                    new InfernalEntTrialEncounterPoint(
                        3,
                        StarterInfernalRaidPacingCatalogue.RiskRouteFlameChoiceBeatId,
                        StarterInfernalRaidPacingCatalogue.FlameTrapContentId,
                        0f,
                        2f),
                    StarterInfernalEntTrialSpatialRecipes.RiskRoute.EncounterPoints[2]
                },
                new InfernalEntTrialHazardPoint(
                    2,
                    StarterInfernalRaidPacingCatalogue.RiskRouteHellhoundBBeatId,
                    StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId,
                    0f,
                    2f,
                    2f,
                    true,
                    true,
                    -3.5f,
                    3.5f));

            var result = InfernalEntTrialSpatialRecipeEvidence.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues, Is.EqualTo(new[]
            {
                InfernalEntTrialSpatialRecipeValidationIssue.BeatMappingInvalid,
                InfernalEntTrialSpatialRecipeValidationIssue.HazardRepresentationInvalid
            }));
        }

        [Test]
        public void LookupAndValidationResultsAreImmutableSnapshots()
        {
            var lookup = InfernalEntTrialSpatialRecipeEvidence.FindByCompositionId(
                StarterInfernalRaidPacingCatalogue.EntryTrial.CompositionId);
            var result = InfernalEntTrialSpatialRecipeEvidence.Validate(
                StarterInfernalEntTrialSpatialRecipes.EntryTrial);
            var issues = (IList<InfernalEntTrialSpatialRecipeValidationIssue>)result.Issues;

            Assert.That(issues.IsReadOnly, Is.True);
            Assert.That(typeof(InfernalEntTrialSpatialRecipeLookupResult).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(InfernalEntTrialSpatialRecipeValidationResult).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(lookup.Recipe, Is.SameAs(
                StarterInfernalEntTrialSpatialRecipes.EntryTrial));
            Assert.Throws<NotSupportedException>(() => issues.Add(
                InfernalEntTrialSpatialRecipeValidationIssue.RecipeMissing));
        }

        [Test]
        public void RuntimeAssemblyHasNoUnityOrGameRuntimeReference()
        {
            var dependencies = typeof(InfernalEntTrialSpatialRecipeEvidence).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static InfernalEntTrialSpatialRecipe CreateEntryTrialRecipe(string compositionId)
        {
            return new InfernalEntTrialSpatialRecipe(
                compositionId,
                StarterInfernalEntTrialSpatialRecipes.EntryTrial.LaneHalfWidth,
                StarterInfernalEntTrialSpatialRecipes.EntryTrial.Hero,
                StarterInfernalEntTrialSpatialRecipes.EntryTrial.EncounterPoints,
                null);
        }
    }
}
