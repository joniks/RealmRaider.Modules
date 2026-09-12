using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.LargeCreatureCombatRhythm.Tests
{
    public sealed class HeavyAttackRhythmEvaluatorTests
    {
        [Test]
        public void GuardianRecipeReturnsNoActionForZeroEligibleTargets()
        {
            var result = HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(
                GuardianEntCombatRhythmRecipes.GuardianEnt,
                0,
                2));

            Assert.That(result.Action, Is.EqualTo(HeavyAttackRhythmAction.NoAction));
            Assert.That(result.Reason, Is.EqualTo(
                HeavyAttackRhythmDecisionReason.NoEligibleTargets));
        }

        [Test]
        public void GuardianRecipeUsesBasicThenAreaAtTheExactSingleTargetThreshold()
        {
            var beforeThreshold = HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(
                GuardianEntCombatRhythmRecipes.GuardianEnt,
                1,
                1));
            var atThreshold = HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(
                GuardianEntCombatRhythmRecipes.GuardianEnt,
                1,
                2));

            Assert.That(beforeThreshold.Action, Is.EqualTo(HeavyAttackRhythmAction.Basic));
            Assert.That(beforeThreshold.Reason, Is.EqualTo(
                HeavyAttackRhythmDecisionReason.BasicFallback));
            Assert.That(atThreshold.Action, Is.EqualTo(HeavyAttackRhythmAction.Area));
            Assert.That(atThreshold.Reason, Is.EqualTo(
                HeavyAttackRhythmDecisionReason.SingleTargetAreaAfterConsecutiveBasics));
        }

        [Test]
        public void GuardianRecipeReturnsAreaImmediatelyForTwoEligibleTargets()
        {
            var result = HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(
                GuardianEntCombatRhythmRecipes.GuardianEnt,
                2,
                0));

            Assert.That(result.Action, Is.EqualTo(HeavyAttackRhythmAction.Area));
            Assert.That(result.Reason, Is.EqualTo(
                HeavyAttackRhythmDecisionReason.ImmediateAreaForEligibleTargets));
            Assert.That((int)HeavyAttackRhythmAction.Basic, Is.EqualTo(
                (int)LargeCreatureAbilitySlot.Basic));
            Assert.That((int)HeavyAttackRhythmAction.Area, Is.EqualTo(
                (int)LargeCreatureAbilitySlot.Area));
            Assert.That((int)HeavyAttackRhythmAction.NoAction, Is.EqualTo(-1));
        }

        [Test]
        public void EvaluatorRespectsExactConfiguredThresholdBoundaries()
        {
            var recipe = CreateRecipe(3, 4);

            Assert.That(HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(
                recipe,
                2,
                9)).Action, Is.EqualTo(HeavyAttackRhythmAction.Basic));
            Assert.That(HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(
                recipe,
                3,
                0)).Action, Is.EqualTo(HeavyAttackRhythmAction.Area));
            Assert.That(HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(
                recipe,
                1,
                3)).Action, Is.EqualTo(HeavyAttackRhythmAction.Basic));
            Assert.That(HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(
                recipe,
                1,
                4)).Action, Is.EqualTo(HeavyAttackRhythmAction.Area));
        }

        [Test]
        public void CoreCanResetItsBasicCountAfterAreaWithoutEvaluatorState()
        {
            var recipe = GuardianEntCombatRhythmRecipes.GuardianEnt;
            var area = HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(recipe, 1, 2));
            var reset = HeavyAttackRhythmEvaluator.Evaluate(new HeavyAttackRhythmInput(recipe, 1, 0));

            Assert.That(area.Action, Is.EqualTo(HeavyAttackRhythmAction.Area));
            Assert.That(reset.Action, Is.EqualTo(HeavyAttackRhythmAction.Basic));
        }

        [Test]
        public void EvaluatorFailsClosedForNullNegativeAndMalformedInputs()
        {
            var missingOpportunityRecipe = new LargeCreatureCombatRhythmRecipe(
                "test.no-opportunity",
                GuardianEntCombatRhythmRecipes.GuardianEntArchetypeId,
                new[]
                {
                    LargeCreatureAbilitySlot.Basic,
                    LargeCreatureAbilitySlot.Area
                },
                null);
            var malformedRecipe = new LargeCreatureCombatRhythmRecipe(
                "test.malformed",
                GuardianEntCombatRhythmRecipes.GuardianEntArchetypeId,
                new[]
                {
                    LargeCreatureAbilitySlot.Basic,
                    LargeCreatureAbilitySlot.Charge
                },
                new HeavyAreaOpportunity(
                    2,
                    2,
                    LargeCreatureEligibleTargetSource.ExistingExplicitBrainTargets,
                    LargeCreatureEligibilityDistanceSource.ExistingAreaAbilityRadius,
                    LargeCreaturePunishWindowSource.ExistingAreaAbilityRecovery));

            AssertInvalid(null);
            AssertInvalid(new HeavyAttackRhythmInput(null, 1, 0));
            AssertInvalid(new HeavyAttackRhythmInput(
                GuardianEntCombatRhythmRecipes.GuardianEnt,
                -1,
                0));
            AssertInvalid(new HeavyAttackRhythmInput(
                GuardianEntCombatRhythmRecipes.GuardianEnt,
                1,
                -1));
            AssertInvalid(new HeavyAttackRhythmInput(missingOpportunityRecipe, 1, 0));
            AssertInvalid(new HeavyAttackRhythmInput(malformedRecipe, 1, 0));
        }

        [Test]
        public void InputAndResultAreImmutableAndEvaluationIsDeterministic()
        {
            var input = new HeavyAttackRhythmInput(
                GuardianEntCombatRhythmRecipes.GuardianEnt,
                1,
                2);
            var first = HeavyAttackRhythmEvaluator.Evaluate(input);
            var second = HeavyAttackRhythmEvaluator.Evaluate(input);

            Assert.That(first, Is.SameAs(second));
            Assert.That(first.Action, Is.EqualTo(HeavyAttackRhythmAction.Area));
            Assert.That(typeof(HeavyAttackRhythmInput).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(HeavyAttackRhythmResult).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
        }

        private static LargeCreatureCombatRhythmRecipe CreateRecipe(
            int immediateAreaMinimumEligibleTargets,
            int singleTargetAreaAfterConsecutiveBasicCount)
        {
            return new LargeCreatureCombatRhythmRecipe(
                "test.heavy-rhythm",
                "test.heavy-creature",
                new[]
                {
                    LargeCreatureAbilitySlot.Basic,
                    LargeCreatureAbilitySlot.Area
                },
                new HeavyAreaOpportunity(
                    immediateAreaMinimumEligibleTargets,
                    singleTargetAreaAfterConsecutiveBasicCount,
                    LargeCreatureEligibleTargetSource.ExistingExplicitBrainTargets,
                    LargeCreatureEligibilityDistanceSource.ExistingAreaAbilityRadius,
                    LargeCreaturePunishWindowSource.ExistingAreaAbilityRecovery));
        }

        private static void AssertInvalid(HeavyAttackRhythmInput input)
        {
            var result = HeavyAttackRhythmEvaluator.Evaluate(input);

            Assert.That(result.Action, Is.EqualTo(HeavyAttackRhythmAction.NoAction));
            Assert.That(result.Reason, Is.EqualTo(
                HeavyAttackRhythmDecisionReason.InvalidInput));
        }
    }
}
