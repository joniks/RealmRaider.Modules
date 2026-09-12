using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.LargeCreatureCombatRhythm.Tests
{
    public sealed class GuardianEntCombatRhythmRecipeTests
    {
        [Test]
        public void GuardianEntUsesTheExactAiEligibleSlotOrder()
        {
            var recipe = GuardianEntCombatRhythmRecipes.GuardianEnt;

            Assert.That(recipe.RecipeId, Is.EqualTo("realmraiders.guardian-ent.attack-rhythm"));
            Assert.That(recipe.ArchetypeId, Is.EqualTo(
                GuardianEntCombatRhythmRecipes.GuardianEntArchetypeId));
            Assert.That(recipe.AiEligibleSlots, Is.EqualTo(new[]
            {
                LargeCreatureAbilitySlot.Basic,
                LargeCreatureAbilitySlot.Area
            }));
            Assert.That(recipe.AiEligibleSlots, Has.No.Member(
                LargeCreatureAbilitySlot.Charge));
            Assert.That((int)LargeCreatureAbilitySlot.Basic, Is.EqualTo(0));
            Assert.That((int)LargeCreatureAbilitySlot.Charge, Is.EqualTo(1));
            Assert.That((int)LargeCreatureAbilitySlot.Area, Is.EqualTo(2));
        }

        [Test]
        public void AreaOpportunityUsesExactTwoPathThresholdsAndExistingSourceSemantics()
        {
            var opportunity = GuardianEntCombatRhythmRecipes.GuardianEnt.HeavyAreaOpportunity;

            Assert.That(opportunity.ImmediateAreaMinimumEligibleTargets, Is.EqualTo(2));
            Assert.That(opportunity.SingleTargetAreaAfterConsecutiveBasicCount, Is.EqualTo(2));
            Assert.That(opportunity.EligibleTargetSource, Is.EqualTo(
                LargeCreatureEligibleTargetSource.ExistingExplicitBrainTargets));
            Assert.That(opportunity.EligibilityDistanceSource, Is.EqualTo(
                LargeCreatureEligibilityDistanceSource.ExistingAreaAbilityRadius));
            Assert.That(opportunity.PunishWindowSource, Is.EqualTo(
                LargeCreaturePunishWindowSource.ExistingAreaAbilityRecovery));
        }

        [Test]
        public void CoreOwnedDecisionNeverOffersAreaForZeroTargets()
        {
            var opportunity = GuardianEntCombatRhythmRecipes.GuardianEnt.HeavyAreaOpportunity;

            Assert.That(CoreMayOfferArea(opportunity, 0, 0), Is.False);
            Assert.That(CoreMayOfferArea(opportunity, 0, 2), Is.False);
        }

        [Test]
        public void CoreOwnedDecisionOffersAreaForOneTargetOnlyAfterTwoBasics()
        {
            var opportunity = GuardianEntCombatRhythmRecipes.GuardianEnt.HeavyAreaOpportunity;

            Assert.That(CoreMayOfferArea(opportunity, 1, 0), Is.False);
            Assert.That(CoreMayOfferArea(opportunity, 1, 1), Is.False);
            Assert.That(CoreMayOfferArea(opportunity, 1, 2), Is.True);
        }

        [Test]
        public void CoreOwnedDecisionOffersAreaImmediatelyForTwoTargets()
        {
            var opportunity = GuardianEntCombatRhythmRecipes.GuardianEnt.HeavyAreaOpportunity;

            Assert.That(CoreMayOfferArea(opportunity, 2, 0), Is.True);
        }

        [Test]
        public void CoreOwnedBasicCountCanResetWithoutChangingImmutableRecipe()
        {
            var opportunity = GuardianEntCombatRhythmRecipes.GuardianEnt.HeavyAreaOpportunity;
            var coreOwnedConsecutiveBasicCount = 2;

            Assert.That(CoreMayOfferArea(opportunity, 1, coreOwnedConsecutiveBasicCount), Is.True);

            coreOwnedConsecutiveBasicCount = 0;

            Assert.That(CoreMayOfferArea(opportunity, 1, coreOwnedConsecutiveBasicCount), Is.False);
            Assert.That(typeof(HeavyAreaOpportunity).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName), Is.Empty);
        }

        [Test]
        public void RecipeAndAiEligibleSlotSnapshotAreReadOnlyAndStable()
        {
            var recipe = GuardianEntCombatRhythmRecipes.GuardianEnt;

            Assert.That(recipe, Is.SameAs(GuardianEntCombatRhythmRecipes.GuardianEnt));
            Assert.That(recipe.AiEligibleSlots, Is.SameAs(
                GuardianEntCombatRhythmRecipes.GuardianEnt.AiEligibleSlots));

            var slots = (IList<LargeCreatureAbilitySlot>)recipe.AiEligibleSlots;
            Assert.That(slots.IsReadOnly, Is.True);
            Assert.That(typeof(HeavyAreaOpportunity).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(LargeCreatureCombatRhythmRecipe).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.Throws<NotSupportedException>(() => slots[0] = LargeCreatureAbilitySlot.Area);
        }

        [Test]
        public void RecipeSnapshotsCallerProvidedAiEligibleSlots()
        {
            var original = new List<LargeCreatureAbilitySlot>
            {
                LargeCreatureAbilitySlot.Basic,
                LargeCreatureAbilitySlot.Area
            };
            var recipe = new LargeCreatureCombatRhythmRecipe(
                "test.large-creature",
                GuardianEntCombatRhythmRecipes.GuardianEntArchetypeId,
                original,
                GuardianEntCombatRhythmRecipes.GuardianEnt.HeavyAreaOpportunity);

            original[0] = LargeCreatureAbilitySlot.Area;

            Assert.That(recipe.AiEligibleSlots[0], Is.EqualTo(LargeCreatureAbilitySlot.Basic));
        }

        [Test]
        public void RuntimeAssemblyHasNoUnityOrGameRuntimeReference()
        {
            var dependencies = typeof(GuardianEntCombatRhythmRecipes).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static bool CoreMayOfferArea(
            HeavyAreaOpportunity opportunity,
            int coreSuppliedEligibleTargetCount,
            int coreOwnedConsecutiveBasicCount)
        {
            return coreSuppliedEligibleTargetCount >= opportunity.ImmediateAreaMinimumEligibleTargets ||
                coreSuppliedEligibleTargetCount == 1 &&
                coreOwnedConsecutiveBasicCount >= opportunity.SingleTargetAreaAfterConsecutiveBasicCount;
        }
    }
}
