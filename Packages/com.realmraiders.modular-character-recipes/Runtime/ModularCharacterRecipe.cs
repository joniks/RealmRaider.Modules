using System.Collections.Generic;
using System.Collections.ObjectModel;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterRecipes
{
    public enum CharacterRecipeSlot
    {
        BaseBody,
        Head,
        Back,
        Arms,
        Accent
    }

    public sealed class CharacterRecipeModule
    {
        public CharacterRecipeModule(
            CharacterRecipeSlot assignedSlot,
            string moduleId,
            CharacterBodyFamily moduleFamily,
            CharacterRecipeSlot declaredSlot)
        {
            AssignedSlot = assignedSlot;
            ModuleId = moduleId;
            ModuleFamily = moduleFamily;
            DeclaredSlot = declaredSlot;
        }

        public CharacterRecipeSlot AssignedSlot { get; }
        public string ModuleId { get; }
        public CharacterBodyFamily ModuleFamily { get; }
        public CharacterRecipeSlot DeclaredSlot { get; }
    }

    public sealed class ModularCharacterRecipe
    {
        private readonly ReadOnlyCollection<CharacterRecipeModule> modules;
        private readonly ReadOnlyCollection<string> sourceIds;

        public ModularCharacterRecipe(
            int schemaVersion,
            string recipeId,
            string characterId,
            string visualProfileId,
            CharacterBodyFamily family,
            string rigProfileId,
            string animationProfileId,
            IEnumerable<CharacterRecipeModule> modules,
            string paletteId,
            string lodBudgetId,
            IEnumerable<string> sourceIds)
        {
            SchemaVersion = schemaVersion;
            RecipeId = recipeId;
            CharacterId = characterId;
            VisualProfileId = visualProfileId;
            Family = family;
            RigProfileId = rigProfileId;
            AnimationProfileId = animationProfileId;
            this.modules = new ReadOnlyCollection<CharacterRecipeModule>(
                modules == null ? new List<CharacterRecipeModule>() : new List<CharacterRecipeModule>(modules));
            PaletteId = paletteId;
            LodBudgetId = lodBudgetId;
            this.sourceIds = new ReadOnlyCollection<string>(
                sourceIds == null ? new List<string>() : new List<string>(sourceIds));
        }

        public int SchemaVersion { get; }
        public string RecipeId { get; }
        public string CharacterId { get; }
        public string VisualProfileId { get; }
        public CharacterBodyFamily Family { get; }
        public string RigProfileId { get; }
        public string AnimationProfileId { get; }
        public IReadOnlyList<CharacterRecipeModule> Modules => modules;
        public string PaletteId { get; }
        public string LodBudgetId { get; }
        public IReadOnlyList<string> SourceIds => sourceIds;
    }
}
