using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalFirstExpansionTactics;

namespace RealmRaiders.Modules.InfernalExpansionTacticEvaluation
{
    public enum InfernalExpansionTacticEvaluationStatus
    {
        Invalid,
        Inactive,
        Waiting,
        AlreadyIssued,
        Eligible
    }

    public enum InfernalExpansionTacticEvaluationIssue
    {
        RequestMissing,
        LayoutIdInvalid,
        SocketIdInvalid,
        LayoutUnknown,
        SocketUnknown,
        SocketNotFirstAuthored,
        TacticCatalogueInvalid,
        RecipeMissing,
        RecipeBindingMismatch,
        RecipeReferenceMismatch,
        RecipeValidationInvalid,
        FlameActivationRevisionNegative,
        LastHandledFlameRevisionNegative,
        HandledRevisionExceedsActivation,
        InvaderDistanceNotFinite,
        InvaderDistanceNegative,
        TriggerUnsupported
    }

    public sealed class InfernalExpansionTacticEvaluationRequest
    {
        public InfernalExpansionTacticEvaluationRequest(
            string layoutId,
            string socketId,
            TacticRecipe recipe,
            bool runActive,
            bool alreadyIssued,
            bool sceneStarted,
            int flameActivationRevision,
            int lastHandledFlameRevision,
            float invaderDistanceMetres)
        {
            LayoutId = layoutId;
            SocketId = socketId;
            Recipe = recipe;
            RunActive = runActive;
            AlreadyIssued = alreadyIssued;
            SceneStarted = sceneStarted;
            FlameActivationRevision = flameActivationRevision;
            LastHandledFlameRevision = lastHandledFlameRevision;
            InvaderDistanceMetres = invaderDistanceMetres;
        }

        public string LayoutId { get; }
        public string SocketId { get; }
        public TacticRecipe Recipe { get; }
        public bool RunActive { get; }
        public bool AlreadyIssued { get; }
        public bool SceneStarted { get; }
        public int FlameActivationRevision { get; }
        public int LastHandledFlameRevision { get; }
        public float InvaderDistanceMetres { get; }
    }

    public sealed class InfernalExpansionTacticEvaluationResult
    {
        internal InfernalExpansionTacticEvaluationResult(
            InfernalExpansionTacticEvaluationStatus status,
            IReadOnlyList<InfernalExpansionTacticEvaluationIssue> issues,
            IReadOnlyList<TacticValidationIssue> tacticValidationIssues,
            TacticRecipe recipe,
            IReadOnlyList<TacticActor> actors)
        {
            Status = status;
            Issues = Snapshot(issues);
            TacticValidationIssues = Snapshot(tacticValidationIssues);
            Recipe = status == InfernalExpansionTacticEvaluationStatus.Eligible
                ? recipe
                : null;
            Actors = status == InfernalExpansionTacticEvaluationStatus.Eligible
                ? Snapshot(actors)
                : Array.AsReadOnly(Array.Empty<TacticActor>());
        }

        public InfernalExpansionTacticEvaluationStatus Status { get; }
        public IReadOnlyList<InfernalExpansionTacticEvaluationIssue> Issues { get; }
        public IReadOnlyList<TacticValidationIssue> TacticValidationIssues { get; }
        public TacticRecipe Recipe { get; }
        public IReadOnlyList<TacticActor> Actors { get; }
        public bool IsEligible =>
            Status == InfernalExpansionTacticEvaluationStatus.Eligible;

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.AsReadOnly(Array.Empty<T>());
            }

            var copy = new T[source.Count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public static class InfernalExpansionTacticEvaluator
    {
        public static InfernalExpansionTacticEvaluationResult Evaluate(
            InfernalExpansionTacticEvaluationRequest request)
        {
            var issues = new List<InfernalExpansionTacticEvaluationIssue>();
            var tacticValidationIssues = new List<TacticValidationIssue>();
            if (request == null)
            {
                Add(issues, InfernalExpansionTacticEvaluationIssue.RequestMissing);
                return Result(
                    InfernalExpansionTacticEvaluationStatus.Invalid,
                    issues,
                    tacticValidationIssues,
                    null);
            }

            var catalogueValidation = StarterInfernalFirstExpansionTactics.Validate(
                StarterInfernalFirstExpansionTactics.All);
            if (!catalogueValidation.IsValid)
            {
                Add(issues, InfernalExpansionTacticEvaluationIssue.TacticCatalogueInvalid);
                CopyIssues(catalogueValidation.Issues, tacticValidationIssues);
            }

            var lookup = StarterInfernalFirstExpansionTactics.FindExact(
                request.LayoutId,
                request.SocketId);
            AddLookupIssue(lookup.Status, issues);

            if (request.Recipe == null)
            {
                Add(issues, InfernalExpansionTacticEvaluationIssue.RecipeMissing);
            }
            else
            {
                if (!Same(request.Recipe.LayoutId, request.LayoutId)
                    || !Same(request.Recipe.SocketId, request.SocketId))
                {
                    Add(issues, InfernalExpansionTacticEvaluationIssue.RecipeBindingMismatch);
                }

                var recipeValidation = ValidateCandidateRecipe(request.Recipe);
                if (!recipeValidation.IsValid)
                {
                    Add(issues, InfernalExpansionTacticEvaluationIssue.RecipeValidationInvalid);
                    CopyIssues(recipeValidation.Issues, tacticValidationIssues);
                }

                if (lookup.Found && !ReferenceEquals(request.Recipe, lookup.Recipe))
                {
                    Add(issues, InfernalExpansionTacticEvaluationIssue.RecipeReferenceMismatch);
                }
            }

            ValidateObservations(request, issues);
            if (issues.Count != 0)
            {
                return Result(
                    InfernalExpansionTacticEvaluationStatus.Invalid,
                    issues,
                    tacticValidationIssues,
                    null);
            }

            if (!request.RunActive)
            {
                return Result(
                    InfernalExpansionTacticEvaluationStatus.Inactive,
                    issues,
                    tacticValidationIssues,
                    null);
            }

            if (request.AlreadyIssued)
            {
                return Result(
                    InfernalExpansionTacticEvaluationStatus.AlreadyIssued,
                    issues,
                    tacticValidationIssues,
                    null);
            }

            switch (request.Recipe.Trigger)
            {
                case TacticTrigger.SceneStart:
                    return Result(
                        request.SceneStarted
                            ? InfernalExpansionTacticEvaluationStatus.Eligible
                            : InfernalExpansionTacticEvaluationStatus.Waiting,
                        issues,
                        tacticValidationIssues,
                        request.Recipe);
                case TacticTrigger.FlameActivated:
                    return Result(
                        request.FlameActivationRevision > 0
                            && request.FlameActivationRevision
                                > request.LastHandledFlameRevision
                            ? InfernalExpansionTacticEvaluationStatus.Eligible
                            : InfernalExpansionTacticEvaluationStatus.Waiting,
                        issues,
                        tacticValidationIssues,
                        request.Recipe);
                case TacticTrigger.InvaderInRange:
                    return Result(
                        request.InvaderDistanceMetres <= request.Recipe.TriggerRadiusMetres
                            ? InfernalExpansionTacticEvaluationStatus.Eligible
                            : InfernalExpansionTacticEvaluationStatus.Waiting,
                        issues,
                        tacticValidationIssues,
                        request.Recipe);
                default:
                    Add(issues, InfernalExpansionTacticEvaluationIssue.TriggerUnsupported);
                    return Result(
                        InfernalExpansionTacticEvaluationStatus.Invalid,
                        issues,
                        tacticValidationIssues,
                        null);
            }
        }

        private static void ValidateObservations(
            InfernalExpansionTacticEvaluationRequest request,
            ICollection<InfernalExpansionTacticEvaluationIssue> issues)
        {
            if (request.FlameActivationRevision < 0)
            {
                Add(
                    issues,
                    InfernalExpansionTacticEvaluationIssue
                        .FlameActivationRevisionNegative);
            }

            if (request.LastHandledFlameRevision < 0)
            {
                Add(
                    issues,
                    InfernalExpansionTacticEvaluationIssue
                        .LastHandledFlameRevisionNegative);
            }

            if (request.FlameActivationRevision >= 0
                && request.LastHandledFlameRevision >= 0
                && request.LastHandledFlameRevision > request.FlameActivationRevision)
            {
                Add(
                    issues,
                    InfernalExpansionTacticEvaluationIssue
                        .HandledRevisionExceedsActivation);
            }

            if (!IsFinite(request.InvaderDistanceMetres))
            {
                Add(
                    issues,
                    InfernalExpansionTacticEvaluationIssue.InvaderDistanceNotFinite);
            }
            else if (request.InvaderDistanceMetres < 0f)
            {
                Add(
                    issues,
                    InfernalExpansionTacticEvaluationIssue.InvaderDistanceNegative);
            }
        }

        private static TacticValidationResult ValidateCandidateRecipe(TacticRecipe recipe)
        {
            var recipes = new TacticRecipe[
                StarterInfernalFirstExpansionTactics.All.Count];
            for (var index = 0; index < recipes.Length; index++)
            {
                recipes[index] = StarterInfernalFirstExpansionTactics.All[index];
            }

            recipes[ReplacementIndex(recipe)] = recipe;
            return StarterInfernalFirstExpansionTactics.Validate(recipes);
        }

        private static int ReplacementIndex(TacticRecipe recipe)
        {
            for (var index = 0; index < StarterInfernalFirstExpansionTactics.All.Count; index++)
            {
                var canonical = StarterInfernalFirstExpansionTactics.All[index];
                if (Same(recipe.LayoutId, canonical.LayoutId)
                    && Same(recipe.SocketId, canonical.SocketId))
                {
                    return index;
                }
            }

            for (var index = 0; index < StarterInfernalFirstExpansionTactics.All.Count; index++)
            {
                if (Same(
                    recipe.LayoutId,
                    StarterInfernalFirstExpansionTactics.All[index].LayoutId))
                {
                    return index;
                }
            }

            for (var index = 0; index < StarterInfernalFirstExpansionTactics.All.Count; index++)
            {
                if (Same(
                    recipe.SocketId,
                    StarterInfernalFirstExpansionTactics.All[index].SocketId))
                {
                    return index;
                }
            }

            return 0;
        }

        private static void AddLookupIssue(
            TacticLookupStatus status,
            ICollection<InfernalExpansionTacticEvaluationIssue> issues)
        {
            switch (status)
            {
                case TacticLookupStatus.Found:
                    return;
                case TacticLookupStatus.LayoutIdInvalid:
                    Add(issues, InfernalExpansionTacticEvaluationIssue.LayoutIdInvalid);
                    return;
                case TacticLookupStatus.SocketIdInvalid:
                    Add(issues, InfernalExpansionTacticEvaluationIssue.SocketIdInvalid);
                    return;
                case TacticLookupStatus.LayoutUnknown:
                    Add(issues, InfernalExpansionTacticEvaluationIssue.LayoutUnknown);
                    return;
                case TacticLookupStatus.SocketUnknown:
                    Add(issues, InfernalExpansionTacticEvaluationIssue.SocketUnknown);
                    return;
                case TacticLookupStatus.SocketNotFirstAuthored:
                    Add(issues, InfernalExpansionTacticEvaluationIssue.SocketNotFirstAuthored);
                    return;
                default:
                    Add(issues, InfernalExpansionTacticEvaluationIssue.TacticCatalogueInvalid);
                    return;
            }
        }

        private static InfernalExpansionTacticEvaluationResult Result(
            InfernalExpansionTacticEvaluationStatus status,
            IReadOnlyList<InfernalExpansionTacticEvaluationIssue> issues,
            IReadOnlyList<TacticValidationIssue> tacticValidationIssues,
            TacticRecipe recipe)
        {
            return new InfernalExpansionTacticEvaluationResult(
                status,
                issues,
                tacticValidationIssues,
                status == InfernalExpansionTacticEvaluationStatus.Eligible
                    ? recipe
                    : null,
                status == InfernalExpansionTacticEvaluationStatus.Eligible
                    ? recipe.Actors
                    : null);
        }

        private static void CopyIssues(
            IReadOnlyList<TacticValidationIssue> source,
            ICollection<TacticValidationIssue> target)
        {
            for (var index = 0; index < source.Count; index++)
            {
                if (!target.Contains(source[index]))
                {
                    target.Add(source[index]);
                }
            }
        }

        private static void Add(
            ICollection<InfernalExpansionTacticEvaluationIssue> issues,
            InfernalExpansionTacticEvaluationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private static bool Same(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
