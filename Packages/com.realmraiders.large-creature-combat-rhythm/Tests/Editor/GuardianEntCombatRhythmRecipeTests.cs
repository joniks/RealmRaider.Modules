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
        public void CoreSuppliedEligibleTargetCountBlocksOneAndAllowsTwoOrMore()
        {
            var opportunity = GuardianEntCombatRhythmRecipes.GuardianEnt.HeavyAreaOpportunity;
            var coreSuppliedOneEligibleTarget = 1;
            var coreSuppliedTwoEligibleTargets = 2;

            Assert.That(opportunity.MinimumEligibleNearbyTargetCount, Is.EqualTo(2));
            Assert.That(coreSuppliedOneEligibleTarget, Is.LessThan(
                opportunity.MinimumEligibleNearbyTargetCount));
            Assert.That(coreSuppliedTwoEligibleTargets, Is.GreaterThanOrEqualTo(
                opportunity.MinimumEligibleNearbyTargetCount));
        }

        [Test]
        public void AreaOpportunityUsesExistingAbilityRangeAndRecoverySemantics()
        {
            var opportunity = GuardianEntCombatRhythmRecipes.GuardianEnt.HeavyAreaOpportunity;

            Assert.That(opportunity.EligibilityDistanceSource, Is.EqualTo(
                LargeCreatureEligibilityDistanceSource.ExistingAreaAbilityRadius));
            Assert.That(opportunity.PunishWindowSource, Is.EqualTo(
                LargeCreaturePunishWindowSource.ExistingAreaAbilityRecovery));
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
    }
}
