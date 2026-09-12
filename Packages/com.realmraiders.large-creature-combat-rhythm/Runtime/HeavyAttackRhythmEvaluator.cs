namespace RealmRaiders.Modules.LargeCreatureCombatRhythm
{
    public enum HeavyAttackRhythmAction
    {
        NoAction = -1,
        Basic = 0,
        Area = 2
    }

    public enum HeavyAttackRhythmDecisionReason
    {
        InvalidInput = 0,
        NoEligibleTargets = 1,
        BasicFallback = 2,
        SingleTargetAreaAfterConsecutiveBasics = 3,
        ImmediateAreaForEligibleTargets = 4
    }

    /// <summary>
    /// Immutable Core-supplied facts for one rhythm decision. The evaluator does
    /// not own target discovery or the consecutive Basic counter lifecycle.
    /// </summary>
    public sealed class HeavyAttackRhythmInput
    {
        public HeavyAttackRhythmInput(
            LargeCreatureCombatRhythmRecipe recipe,
            int eligibleTargetCount,
            int consecutiveBasicCount)
        {
            Recipe = recipe;
            EligibleTargetCount = eligibleTargetCount;
            ConsecutiveBasicCount = consecutiveBasicCount;
        }

        public LargeCreatureCombatRhythmRecipe Recipe { get; }

        public int EligibleTargetCount { get; }

        public int ConsecutiveBasicCount { get; }
    }

    /// <summary>Immutable action recommendation with no gameplay execution authority.</summary>
    public sealed class HeavyAttackRhythmResult
    {
        internal HeavyAttackRhythmResult(
            HeavyAttackRhythmAction action,
            HeavyAttackRhythmDecisionReason reason)
        {
            Action = action;
            Reason = reason;
        }

        public HeavyAttackRhythmAction Action { get; }

        public HeavyAttackRhythmDecisionReason Reason { get; }
    }

    /// <summary>Pure, fail-closed decision helper for heavy-creature Basic and Area rhythm.</summary>
    public static class HeavyAttackRhythmEvaluator
    {
        static readonly HeavyAttackRhythmResult InvalidInputResult = new(
            HeavyAttackRhythmAction.NoAction,
            HeavyAttackRhythmDecisionReason.InvalidInput);

        static readonly HeavyAttackRhythmResult NoEligibleTargetsResult = new(
            HeavyAttackRhythmAction.NoAction,
            HeavyAttackRhythmDecisionReason.NoEligibleTargets);

        static readonly HeavyAttackRhythmResult BasicFallbackResult = new(
            HeavyAttackRhythmAction.Basic,
            HeavyAttackRhythmDecisionReason.BasicFallback);

        static readonly HeavyAttackRhythmResult SingleTargetAreaResult = new(
            HeavyAttackRhythmAction.Area,
            HeavyAttackRhythmDecisionReason.SingleTargetAreaAfterConsecutiveBasics);

        static readonly HeavyAttackRhythmResult ImmediateAreaResult = new(
            HeavyAttackRhythmAction.Area,
            HeavyAttackRhythmDecisionReason.ImmediateAreaForEligibleTargets);

        public static HeavyAttackRhythmResult Evaluate(HeavyAttackRhythmInput input)
        {
            if (!IsValid(input))
            {
                return InvalidInputResult;
            }

            if (input.EligibleTargetCount == 0)
            {
                return NoEligibleTargetsResult;
            }

            var opportunity = input.Recipe.HeavyAreaOpportunity;
            if (input.EligibleTargetCount >= opportunity.ImmediateAreaMinimumEligibleTargets)
            {
                return ImmediateAreaResult;
            }

            if (input.EligibleTargetCount == 1 &&
                input.ConsecutiveBasicCount >= opportunity.SingleTargetAreaAfterConsecutiveBasicCount)
            {
                return SingleTargetAreaResult;
            }

            return BasicFallbackResult;
        }

        static bool IsValid(HeavyAttackRhythmInput input)
        {
            if (input == null || input.Recipe == null ||
                input.EligibleTargetCount < 0 || input.ConsecutiveBasicCount < 0)
            {
                return false;
            }

            var recipe = input.Recipe;
            var opportunity = recipe.HeavyAreaOpportunity;
            if (string.IsNullOrWhiteSpace(recipe.RecipeId) ||
                string.IsNullOrWhiteSpace(recipe.ArchetypeId) ||
                opportunity == null ||
                opportunity.ImmediateAreaMinimumEligibleTargets < 2 ||
                opportunity.SingleTargetAreaAfterConsecutiveBasicCount < 1 ||
                opportunity.EligibleTargetSource != LargeCreatureEligibleTargetSource.ExistingExplicitBrainTargets ||
                opportunity.EligibilityDistanceSource != LargeCreatureEligibilityDistanceSource.ExistingAreaAbilityRadius ||
                opportunity.PunishWindowSource != LargeCreaturePunishWindowSource.ExistingAreaAbilityRecovery)
            {
                return false;
            }

            var slots = recipe.AiEligibleSlots;
            return slots != null &&
                slots.Count == 2 &&
                slots[0] == LargeCreatureAbilitySlot.Basic &&
                slots[1] == LargeCreatureAbilitySlot.Area;
        }
    }
}
