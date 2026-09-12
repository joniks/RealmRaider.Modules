using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.LargeCreatureCombatRhythm
{
    public enum LargeCreatureAbilitySlot
    {
        Basic = 0,
        Charge = 1,
        Area = 2
    }

    public enum LargeCreatureEligibilityDistanceSource
    {
        ExistingAreaAbilityRadius
    }

    public enum LargeCreaturePunishWindowSource
    {
        ExistingAreaAbilityRecovery
    }

    /// <summary>Immutable facts that make a heavy Area opportunity readable and punishable.</summary>
    public sealed class HeavyAreaOpportunity
    {
        public HeavyAreaOpportunity(
            int minimumEligibleNearbyTargetCount,
            LargeCreatureEligibilityDistanceSource eligibilityDistanceSource,
            LargeCreaturePunishWindowSource punishWindowSource)
        {
            MinimumEligibleNearbyTargetCount = minimumEligibleNearbyTargetCount;
            EligibilityDistanceSource = eligibilityDistanceSource;
            PunishWindowSource = punishWindowSource;
        }

        public int MinimumEligibleNearbyTargetCount { get; }

        public LargeCreatureEligibilityDistanceSource EligibilityDistanceSource { get; }

        public LargeCreaturePunishWindowSource PunishWindowSource { get; }
    }

    /// <summary>
    /// Immutable, adapter-neutral rhythm data. It has no authority to select targets
    /// or execute an ability; Core remains responsible for those gameplay decisions.
    /// </summary>
    public sealed class LargeCreatureCombatRhythmRecipe
    {
        public LargeCreatureCombatRhythmRecipe(
            string recipeId,
            string archetypeId,
            IReadOnlyList<LargeCreatureAbilitySlot> aiEligibleSlots,
            HeavyAreaOpportunity heavyAreaOpportunity)
        {
            RecipeId = recipeId;
            ArchetypeId = archetypeId;
            AiEligibleSlots = Snapshot(aiEligibleSlots);
            HeavyAreaOpportunity = heavyAreaOpportunity;
        }

        public string RecipeId { get; }

        public string ArchetypeId { get; }

        public IReadOnlyList<LargeCreatureAbilitySlot> AiEligibleSlots { get; }

        public HeavyAreaOpportunity HeavyAreaOpportunity { get; }

        private static IReadOnlyList<LargeCreatureAbilitySlot> Snapshot(
            IReadOnlyList<LargeCreatureAbilitySlot> aiEligibleSlots)
        {
            if (aiEligibleSlots == null)
            {
                return Array.AsReadOnly(Array.Empty<LargeCreatureAbilitySlot>());
            }

            var copy = new LargeCreatureAbilitySlot[aiEligibleSlots.Count];
            for (var index = 0; index < aiEligibleSlots.Count; index++)
            {
                copy[index] = aiEligibleSlots[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>One explicit Guardian Ent recipe. Consumers must deliberately opt in.</summary>
    public static class GuardianEntCombatRhythmRecipes
    {
        public const string GuardianEntArchetypeId = "realmraiders.guardian-ent";

        public static LargeCreatureCombatRhythmRecipe GuardianEnt { get; } =
            new LargeCreatureCombatRhythmRecipe(
                "realmraiders.guardian-ent.attack-rhythm",
                GuardianEntArchetypeId,
                new[]
                {
                    LargeCreatureAbilitySlot.Basic,
                    LargeCreatureAbilitySlot.Area
                },
                new HeavyAreaOpportunity(
                    2,
                    LargeCreatureEligibilityDistanceSource.ExistingAreaAbilityRadius,
                    LargeCreaturePunishWindowSource.ExistingAreaAbilityRecovery));
    }
}
