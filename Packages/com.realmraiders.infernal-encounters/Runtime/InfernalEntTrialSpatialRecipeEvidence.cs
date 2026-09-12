using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.InfernalEncounters
{
    public enum InfernalEntTrialSpatialRecipeLookupStatus
    {
        Found,
        InvalidCompositionId,
        NotFound
    }

    public enum InfernalEntTrialSpatialRecipeValidationIssue
    {
        RecipeMissing,
        CompositionIdInvalid,
        PacingCompositionNotFound,
        LaneHalfWidthInvalid,
        HeroMissing,
        HeroArchetypeInvalid,
        HeroPlacementInvalid,
        EncounterPointPlacementInvalid,
        BeatMappingInvalid,
        HazardCardinalityInvalid,
        HazardPacingContractInvalid,
        HazardRepresentationInvalid,
        HazardPlacementInvalid,
        HazardBypassInvalid,
        HazardPresentationInvalid
    }

    /// <summary>Fail-closed result of an exact cached spatial-recipe lookup.</summary>
    public sealed class InfernalEntTrialSpatialRecipeLookupResult
    {
        public InfernalEntTrialSpatialRecipeLookupResult(
            InfernalEntTrialSpatialRecipeLookupStatus status,
            InfernalEntTrialSpatialRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public InfernalEntTrialSpatialRecipeLookupStatus Status { get; }

        public InfernalEntTrialSpatialRecipe Recipe { get; }

        public bool Found
        {
            get
            {
                return Status == InfernalEntTrialSpatialRecipeLookupStatus.Found;
            }
        }
    }

    /// <summary>Stable, structured evidence that a supplied recipe matches its pacing facts.</summary>
    public sealed class InfernalEntTrialSpatialRecipeValidationResult
    {
        public InfernalEntTrialSpatialRecipeValidationResult(
            IReadOnlyList<InfernalEntTrialSpatialRecipeValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalEntTrialSpatialRecipeValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<InfernalEntTrialSpatialRecipeValidationIssue> Snapshot(
            IReadOnlyList<InfernalEntTrialSpatialRecipeValidationIssue> issues)
        {
            if (issues == null)
            {
                return Array.AsReadOnly(Array.Empty<InfernalEntTrialSpatialRecipeValidationIssue>());
            }

            var copy = new InfernalEntTrialSpatialRecipeValidationIssue[issues.Count];
            for (var index = 0; index < issues.Count; index++)
            {
                copy[index] = issues[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// Exact lookup and deterministic validation evidence for adapter-selected Infernal
    /// spatial facts. It does not select a variant or perform any runtime work.
    /// </summary>
    public static class InfernalEntTrialSpatialRecipeEvidence
    {
        public static InfernalEntTrialSpatialRecipeLookupResult FindByCompositionId(
            string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId))
            {
                return new InfernalEntTrialSpatialRecipeLookupResult(
                    InfernalEntTrialSpatialRecipeLookupStatus.InvalidCompositionId,
                    null);
            }

            foreach (var recipe in StarterInfernalEntTrialSpatialRecipes.All)
            {
                if (string.Equals(recipe.CompositionId, compositionId, StringComparison.Ordinal))
                {
                    return new InfernalEntTrialSpatialRecipeLookupResult(
                        InfernalEntTrialSpatialRecipeLookupStatus.Found,
                        recipe);
                }
            }

            return new InfernalEntTrialSpatialRecipeLookupResult(
                InfernalEntTrialSpatialRecipeLookupStatus.NotFound,
                null);
        }

        public static InfernalEntTrialSpatialRecipeValidationResult Validate(
            InfernalEntTrialSpatialRecipe recipe)
        {
            var issues = new List<InfernalEntTrialSpatialRecipeValidationIssue>();

            if (recipe == null)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.RecipeMissing);
                return new InfernalEntTrialSpatialRecipeValidationResult(issues);
            }

            if (string.IsNullOrWhiteSpace(recipe.CompositionId))
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.CompositionIdInvalid);
                return new InfernalEntTrialSpatialRecipeValidationResult(issues);
            }

            var pacingComposition = FindPacingComposition(recipe.CompositionId);
            if (pacingComposition == null)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.PacingCompositionNotFound);
                return new InfernalEntTrialSpatialRecipeValidationResult(issues);
            }

            ValidateLane(recipe, issues);
            ValidateHero(recipe, issues);
            ValidateEncounterPoints(recipe, issues);
            ValidateBeatMapping(recipe, pacingComposition, issues);
            ValidateHazard(recipe, pacingComposition, issues);

            return new InfernalEntTrialSpatialRecipeValidationResult(issues);
        }

        private static InfernalRaidPacingComposition FindPacingComposition(string compositionId)
        {
            foreach (var composition in StarterInfernalRaidPacingCatalogue.All)
            {
                if (string.Equals(composition.CompositionId, compositionId, StringComparison.Ordinal))
                {
                    return composition;
                }
            }

            return null;
        }

        private static void ValidateLane(
            InfernalEntTrialSpatialRecipe recipe,
            ICollection<InfernalEntTrialSpatialRecipeValidationIssue> issues)
        {
            if (!IsFinitePositive(recipe.LaneHalfWidth))
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.LaneHalfWidthInvalid);
            }
        }

        private static void ValidateHero(
            InfernalEntTrialSpatialRecipe recipe,
            ICollection<InfernalEntTrialSpatialRecipeValidationIssue> issues)
        {
            if (recipe.Hero == null)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.HeroMissing);
                return;
            }

            if (!string.Equals(
                    recipe.Hero.ArchetypeId,
                    StarterInfernalRaidPacingCatalogue.GuardianEntArchetypeId,
                    StringComparison.Ordinal))
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.HeroArchetypeInvalid);
            }

            if (!IsFinitePositive(recipe.Hero.Scale)
                || !IsFinitePositive(recipe.Hero.SafeCenterHalfWidth)
                || !IsFinite(recipe.Hero.X)
                || !IsFinite(recipe.Hero.Z)
                || Math.Abs(recipe.Hero.X) > recipe.Hero.SafeCenterHalfWidth
                || !IsFinitePositive(recipe.LaneHalfWidth)
                || recipe.Hero.SafeCenterHalfWidth > recipe.LaneHalfWidth)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.HeroPlacementInvalid);
            }
        }

        private static void ValidateEncounterPoints(
            InfernalEntTrialSpatialRecipe recipe,
            ICollection<InfernalEntTrialSpatialRecipeValidationIssue> issues)
        {
            foreach (var point in recipe.EncounterPoints)
            {
                if (point == null
                    || !IsFinite(point.X)
                    || !IsFinite(point.Z)
                    || !IsFinitePositive(recipe.LaneHalfWidth)
                    || Math.Abs(point.X) > recipe.LaneHalfWidth)
                {
                    AddIssue(
                        issues,
                        InfernalEntTrialSpatialRecipeValidationIssue.EncounterPointPlacementInvalid);
                    return;
                }
            }
        }

        private static void ValidateBeatMapping(
            InfernalEntTrialSpatialRecipe recipe,
            InfernalRaidPacingComposition pacingComposition,
            ICollection<InfernalEntTrialSpatialRecipeValidationIssue> issues)
        {
            var actualBeatCount = recipe.EncounterPoints.Count;
            if (recipe.FlameTrap != null)
            {
                actualBeatCount++;
            }

            if (actualBeatCount != pacingComposition.Beats.Count)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.BeatMappingInvalid);
                return;
            }

            for (var index = 0; index < pacingComposition.Beats.Count; index++)
            {
                var expectedBeat = pacingComposition.Beats[index];
                var sequence = index + 1;
                var encounterSequenceCount = 0;
                var encounterMatchesPacingBeat = false;
                var flameTrapMatchesPacingBeat = false;

                foreach (var point in recipe.EncounterPoints)
                {
                    if (point != null && point.Sequence == sequence)
                    {
                        encounterSequenceCount++;
                        var beatIdMatches = string.Equals(
                            point.BeatId,
                            expectedBeat.BeatId,
                            StringComparison.Ordinal);
                        var contentIdMatches = string.Equals(
                            point.ContentId,
                            expectedBeat.ContentId,
                            StringComparison.Ordinal);
                        encounterMatchesPacingBeat = beatIdMatches && contentIdMatches;
                    }
                }

                if (recipe.FlameTrap != null && recipe.FlameTrap.Sequence == sequence)
                {
                    var beatIdMatches = string.Equals(
                        recipe.FlameTrap.BeatId,
                        expectedBeat.BeatId,
                        StringComparison.Ordinal);
                    var contentIdMatches = string.Equals(
                        recipe.FlameTrap.ContentId,
                        expectedBeat.ContentId,
                        StringComparison.Ordinal);
                    flameTrapMatchesPacingBeat = beatIdMatches && contentIdMatches;
                }

                if (expectedBeat.Kind == InfernalRaidBeatKind.Hazard)
                {
                    if (encounterSequenceCount != 0 || !flameTrapMatchesPacingBeat)
                    {
                        AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.BeatMappingInvalid);
                        return;
                    }

                    continue;
                }

                if (encounterSequenceCount != 1 || !encounterMatchesPacingBeat
                    || (recipe.FlameTrap != null && recipe.FlameTrap.Sequence == sequence))
                {
                    AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.BeatMappingInvalid);
                    return;
                }
            }
        }

        private static void ValidateHazard(
            InfernalEntTrialSpatialRecipe recipe,
            InfernalRaidPacingComposition pacingComposition,
            ICollection<InfernalEntTrialSpatialRecipeValidationIssue> issues)
        {
            InfernalRaidPacingBeat expectedHazardBeat = null;
            var expectedHazardCount = 0;
            var expectedHazardSequence = 0;

            for (var index = 0; index < pacingComposition.Beats.Count; index++)
            {
                var pacingBeat = pacingComposition.Beats[index];
                if (pacingBeat.Kind == InfernalRaidBeatKind.Hazard)
                {
                    expectedHazardBeat = pacingBeat;
                    expectedHazardCount++;
                    expectedHazardSequence = index + 1;
                }
            }

            var actualHazardCount = recipe.FlameTrap == null ? 0 : 1;
            if (actualHazardCount != expectedHazardCount)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.HazardCardinalityInvalid);
                return;
            }

            if (expectedHazardCount == 0)
            {
                return;
            }

            if (expectedHazardCount != 1
                || !expectedHazardBeat.IsOptionalRisk
                || !expectedHazardBeat.IsBypassable)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.HazardPacingContractInvalid);
                return;
            }

            var hazard = recipe.FlameTrap;
            if (hazard.Sequence != expectedHazardSequence
                || !string.Equals(hazard.BeatId, expectedHazardBeat.BeatId, StringComparison.Ordinal)
                || !string.Equals(
                    hazard.ContentId,
                    expectedHazardBeat.ContentId,
                    StringComparison.Ordinal))
            {
                AddIssue(
                    issues,
                    InfernalEntTrialSpatialRecipeValidationIssue.HazardRepresentationInvalid);
            }

            if (!IsFinite(hazard.X)
                || !IsFinite(hazard.Z)
                || !IsFinitePositive(hazard.TriggerRadius)
                || !IsFinitePositive(recipe.LaneHalfWidth)
                || Math.Abs(hazard.X) > recipe.LaneHalfWidth)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.HazardPlacementInvalid);
            }

            if (!hazard.AutomaticAfterInitialize || !hazard.RequiresNonBlockingPresentation)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.HazardPresentationInvalid);
            }

            if (recipe.Hero == null
                || !IsFinite(hazard.LeftBypassX)
                || !IsFinite(hazard.RightBypassX)
                || hazard.LeftBypassX >= hazard.RightBypassX
                || hazard.LeftBypassX >= hazard.X
                || hazard.RightBypassX <= hazard.X
                || Math.Abs(hazard.LeftBypassX - hazard.X) <= hazard.TriggerRadius
                || Math.Abs(hazard.RightBypassX - hazard.X) <= hazard.TriggerRadius
                || Math.Abs(hazard.LeftBypassX) > recipe.Hero.SafeCenterHalfWidth
                || Math.Abs(hazard.RightBypassX) > recipe.Hero.SafeCenterHalfWidth)
            {
                AddIssue(issues, InfernalEntTrialSpatialRecipeValidationIssue.HazardBypassInvalid);
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinitePositive(float value)
        {
            return IsFinite(value) && value > 0f;
        }

        private static void AddIssue(
            ICollection<InfernalEntTrialSpatialRecipeValidationIssue> issues,
            InfernalEntTrialSpatialRecipeValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }
}
