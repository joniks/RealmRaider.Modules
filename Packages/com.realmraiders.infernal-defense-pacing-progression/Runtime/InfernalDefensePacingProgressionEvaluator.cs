using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefensePacing;

namespace RealmRaiders.Modules.InfernalDefensePacingProgression
{
    public enum InfernalDefensePacingProgressionStatus
    {
        Evaluated,
        Rejected
    }

    public enum InfernalDefensePacingProgressionIssue
    {
        RecipeMissing,
        LayoutIdInvalid,
        LayoutNotFound,
        RecipeIdentityMismatch,
        RecipeInvalid,
        CompletedBeatIdsMissing,
        SkippedOptionalBeatIdsMissing,
        CompletedBeatIdMissing,
        CompletedBeatIdInvalid,
        CompletedBeatIdUnknown,
        CompletedBeatIdDuplicate,
        SkippedOptionalBeatIdMissing,
        SkippedOptionalBeatIdInvalid,
        SkippedOptionalBeatIdUnknown,
        SkippedOptionalBeatIdDuplicate,
        BeatStateConflict,
        RequiredBeatSkipped,
        RequiredCompletionPrefixInvalid,
        CompletedBeatDependencyUnsatisfied
    }

    /// <summary>
    /// Immutable authored beat evidence. BlockingBeatIds contains only exact direct
    /// prerequisites not satisfied by the supplied completed/skipped state.
    /// </summary>
    public sealed class InfernalDefensePacingBeatEvidence
    {
        internal InfernalDefensePacingBeatEvidence(
            InfernalDefensePacingBeat beat,
            IReadOnlyList<string> blockingBeatIds)
        {
            BeatId = beat.BeatId;
            RoleId = beat.RoleId;
            Kind = beat.Kind;
            Requirement = beat.Requirement;
            DependsOnBeatIds = Snapshot(beat.DependsOnBeatIds);
            BlockingBeatIds = Snapshot(blockingBeatIds);
        }

        public string BeatId { get; }

        public string RoleId { get; }

        public InfernalDefensePacingBeatKind Kind { get; }

        public InfernalDefensePacingRequirement Requirement { get; }

        public IReadOnlyList<string> DependsOnBeatIds { get; }

        public IReadOnlyList<string> BlockingBeatIds { get; }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<T>());
            }

            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalDefensePacingProgression
    {
        internal InfernalDefensePacingProgression(
            string layoutId,
            IReadOnlyList<InfernalDefensePacingBeatEvidence> completed,
            IReadOnlyList<InfernalDefensePacingBeatEvidence> skippedOptional,
            IReadOnlyList<InfernalDefensePacingBeatEvidence> eligible,
            IReadOnlyList<InfernalDefensePacingBeatEvidence> blocked)
        {
            LayoutId = layoutId;
            Completed = Snapshot(completed);
            SkippedOptional = Snapshot(skippedOptional);
            Eligible = Snapshot(eligible);
            Blocked = Snapshot(blocked);
        }

        public string LayoutId { get; }

        public IReadOnlyList<InfernalDefensePacingBeatEvidence> Completed { get; }

        public IReadOnlyList<InfernalDefensePacingBeatEvidence> SkippedOptional { get; }

        public IReadOnlyList<InfernalDefensePacingBeatEvidence> Eligible { get; }

        public IReadOnlyList<InfernalDefensePacingBeatEvidence> Blocked { get; }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<T>());
            }

            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalDefensePacingProgressionResult
    {
        internal InfernalDefensePacingProgressionResult(
            InfernalDefensePacingProgressionStatus status,
            InfernalDefensePacingProgression progression,
            IReadOnlyList<InfernalDefensePacingProgressionIssue> issues,
            InfernalDefensePacingValidationResult recipeValidation)
        {
            Status = status;
            Progression = progression;
            Issues = Snapshot(issues);
            RecipeValidation = recipeValidation;
        }

        public InfernalDefensePacingProgressionStatus Status { get; }

        public InfernalDefensePacingProgression Progression { get; }

        public IReadOnlyList<InfernalDefensePacingProgressionIssue> Issues { get; }

        public InfernalDefensePacingValidationResult RecipeValidation { get; }

        public bool HasProgression =>
            Status == InfernalDefensePacingProgressionStatus.Evaluated
            && Progression != null
            && Issues.Count == 0
            && RecipeValidation != null
            && RecipeValidation.IsValid;

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<T>());
            }

            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// Pure state classification over one exact cached MGC15 recipe. The caller
    /// supplies all state and retains every progression/runtime decision.
    /// </summary>
    public static class InfernalDefensePacingProgressionEvaluator
    {
        public static InfernalDefensePacingProgressionResult Evaluate(
            InfernalDefensePacingRecipe recipe,
            IReadOnlyList<string> completedBeatIds,
            IReadOnlyList<string> skippedOptionalBeatIds)
        {
            var issues = new List<InfernalDefensePacingProgressionIssue>();
            var validation = InfernalDefensePacingValidator.Validate(recipe);
            var exactRecipe = ValidateRecipe(recipe, validation, issues);

            if (completedBeatIds == null)
            {
                AddIssue(
                    issues,
                    InfernalDefensePacingProgressionIssue.CompletedBeatIdsMissing);
            }

            if (skippedOptionalBeatIds == null)
            {
                AddIssue(
                    issues,
                    InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdsMissing);
            }

            var beatsById = CreateBeatsById(exactRecipe);
            var completed = ValidateIds(
                completedBeatIds,
                beatsById,
                true,
                issues);
            var skipped = ValidateIds(
                skippedOptionalBeatIds,
                beatsById,
                false,
                issues);

            ValidateState(exactRecipe, completed, skipped, issues);
            if (issues.Count > 0)
            {
                return new InfernalDefensePacingProgressionResult(
                    InfernalDefensePacingProgressionStatus.Rejected,
                    null,
                    issues,
                    validation);
            }

            return new InfernalDefensePacingProgressionResult(
                InfernalDefensePacingProgressionStatus.Evaluated,
                CreateProgression(exactRecipe, completed, skipped),
                Array.AsReadOnly(Array.Empty<InfernalDefensePacingProgressionIssue>()),
                validation);
        }

        private static InfernalDefensePacingRecipe ValidateRecipe(
            InfernalDefensePacingRecipe recipe,
            InfernalDefensePacingValidationResult validation,
            ICollection<InfernalDefensePacingProgressionIssue> issues)
        {
            if (recipe == null)
            {
                AddIssue(issues, InfernalDefensePacingProgressionIssue.RecipeMissing);
                return null;
            }

            if (!HasStableId(recipe.LayoutId))
            {
                AddIssue(issues, InfernalDefensePacingProgressionIssue.LayoutIdInvalid);
            }

            var resolved = StarterInfernalDefensePacingResolver.ResolveExact(recipe.LayoutId);
            if (!resolved.Found)
            {
                AddIssue(
                    issues,
                    resolved.Status == InfernalDefensePacingLookupStatus.LayoutIdInvalid
                        ? InfernalDefensePacingProgressionIssue.LayoutIdInvalid
                        : InfernalDefensePacingProgressionIssue.LayoutNotFound);
            }
            else if (!ReferenceEquals(recipe, resolved.Recipe))
            {
                AddIssue(
                    issues,
                    InfernalDefensePacingProgressionIssue.RecipeIdentityMismatch);
            }

            if (validation == null || !validation.IsValid)
            {
                AddIssue(issues, InfernalDefensePacingProgressionIssue.RecipeInvalid);
            }

            return resolved.Found && ReferenceEquals(recipe, resolved.Recipe)
                ? resolved.Recipe
                : null;
        }

        private static IReadOnlyDictionary<string, InfernalDefensePacingBeat> CreateBeatsById(
            InfernalDefensePacingRecipe recipe)
        {
            var beats = new Dictionary<string, InfernalDefensePacingBeat>(StringComparer.Ordinal);
            if (recipe != null)
            {
                foreach (var beat in recipe.Beats)
                {
                    if (beat != null && !beats.ContainsKey(beat.BeatId))
                    {
                        beats.Add(beat.BeatId, beat);
                    }
                }
            }

            return beats;
        }

        private static ISet<string> ValidateIds(
            IReadOnlyList<string> ids,
            IReadOnlyDictionary<string, InfernalDefensePacingBeat> beatsById,
            bool completed,
            ICollection<InfernalDefensePacingProgressionIssue> issues)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            if (ids == null)
            {
                return result;
            }

            var missing = false;
            var invalid = false;
            var unknown = false;
            var duplicate = false;
            foreach (var id in ids)
            {
                if (id == null)
                {
                    missing = true;
                    continue;
                }

                if (!HasStableId(id))
                {
                    invalid = true;
                    continue;
                }

                if (!result.Add(id))
                {
                    duplicate = true;
                }

                if (!beatsById.ContainsKey(id))
                {
                    unknown = true;
                }
            }

            AddIdIssues(issues, completed, missing, invalid, unknown, duplicate);
            return result;
        }

        private static void AddIdIssues(
            ICollection<InfernalDefensePacingProgressionIssue> issues,
            bool completed,
            bool missing,
            bool invalid,
            bool unknown,
            bool duplicate)
        {
            if (missing)
            {
                AddIssue(
                    issues,
                    completed
                        ? InfernalDefensePacingProgressionIssue.CompletedBeatIdMissing
                        : InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdMissing);
            }

            if (invalid)
            {
                AddIssue(
                    issues,
                    completed
                        ? InfernalDefensePacingProgressionIssue.CompletedBeatIdInvalid
                        : InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdInvalid);
            }

            if (unknown)
            {
                AddIssue(
                    issues,
                    completed
                        ? InfernalDefensePacingProgressionIssue.CompletedBeatIdUnknown
                        : InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdUnknown);
            }

            if (duplicate)
            {
                AddIssue(
                    issues,
                    completed
                        ? InfernalDefensePacingProgressionIssue.CompletedBeatIdDuplicate
                        : InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdDuplicate);
            }
        }

        private static void ValidateState(
            InfernalDefensePacingRecipe recipe,
            ISet<string> completed,
            ISet<string> skipped,
            ICollection<InfernalDefensePacingProgressionIssue> issues)
        {
            foreach (var beatId in completed)
            {
                if (skipped.Contains(beatId))
                {
                    AddIssue(
                        issues,
                        InfernalDefensePacingProgressionIssue.BeatStateConflict);
                }
            }

            if (recipe == null)
            {
                return;
            }

            var requiredGapSeen = false;
            foreach (var beat in recipe.Beats)
            {
                if (beat.Requirement == InfernalDefensePacingRequirement.Required)
                {
                    if (skipped.Contains(beat.BeatId))
                    {
                        AddIssue(
                            issues,
                            InfernalDefensePacingProgressionIssue.RequiredBeatSkipped);
                    }

                    if (!completed.Contains(beat.BeatId))
                    {
                        requiredGapSeen = true;
                    }
                    else if (requiredGapSeen)
                    {
                        AddIssue(
                            issues,
                            InfernalDefensePacingProgressionIssue
                                .RequiredCompletionPrefixInvalid);
                    }
                }

                if (!completed.Contains(beat.BeatId))
                {
                    continue;
                }

                foreach (var dependencyId in beat.DependsOnBeatIds)
                {
                    if (!completed.Contains(dependencyId))
                    {
                        AddIssue(
                            issues,
                            InfernalDefensePacingProgressionIssue
                                .CompletedBeatDependencyUnsatisfied);
                    }
                }
            }
        }

        private static InfernalDefensePacingProgression CreateProgression(
            InfernalDefensePacingRecipe recipe,
            ISet<string> completedIds,
            ISet<string> skippedIds)
        {
            var completed = new List<InfernalDefensePacingBeatEvidence>();
            var skipped = new List<InfernalDefensePacingBeatEvidence>();
            var eligible = new List<InfernalDefensePacingBeatEvidence>();
            var blocked = new List<InfernalDefensePacingBeatEvidence>();

            foreach (var beat in recipe.Beats)
            {
                if (completedIds.Contains(beat.BeatId))
                {
                    completed.Add(Evidence(beat, Array.Empty<string>()));
                    continue;
                }

                if (skippedIds.Contains(beat.BeatId))
                {
                    skipped.Add(Evidence(beat, Array.Empty<string>()));
                    continue;
                }

                var blockers = new List<string>();
                foreach (var dependencyId in beat.DependsOnBeatIds)
                {
                    if (!completedIds.Contains(dependencyId)
                        && !skippedIds.Contains(dependencyId))
                    {
                        blockers.Add(dependencyId);
                    }
                }

                var evidence = Evidence(beat, blockers);
                if (blockers.Count == 0)
                {
                    eligible.Add(evidence);
                }
                else
                {
                    blocked.Add(evidence);
                }
            }

            return new InfernalDefensePacingProgression(
                recipe.LayoutId,
                completed,
                skipped,
                eligible,
                blocked);
        }

        private static InfernalDefensePacingBeatEvidence Evidence(
            InfernalDefensePacingBeat beat,
            IReadOnlyList<string> blockers)
        {
            return new InfernalDefensePacingBeatEvidence(beat, blockers);
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || !IsAsciiLowerAlphaNumeric(value[0])
                || !IsAsciiLowerAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            for (var index = 1; index < value.Length - 1; index++)
            {
                var symbol = value[index];
                if (!IsAsciiLowerAlphaNumeric(symbol)
                    && symbol != '.'
                    && symbol != '_'
                    && symbol != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiLowerAlphaNumeric(char value)
        {
            return value >= 'a' && value <= 'z'
                || value >= '0' && value <= '9';
        }

        private static void AddIssue(
            ICollection<InfernalDefensePacingProgressionIssue> issues,
            InfernalDefensePacingProgressionIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }
}
