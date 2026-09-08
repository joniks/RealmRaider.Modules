using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.CharacterRecipes
{
    /// <summary>A passive, explicitly supplied source of immutable modular character recipes.</summary>
    public interface IModularCharacterRecipeProvider
    {
        string ModuleId { get; }
        IReadOnlyList<ModularCharacterRecipe> Recipes { get; }
    }

    public enum ModularCharacterRecipeCatalogueIssueCode
    {
        NullProviderCollection,
        NullProvider,
        InvalidProvider,
        InvalidModuleId,
        NullRecipeCollection,
        NullRecipe,
        InvalidRecipe,
        DuplicateProviderModuleId,
        DuplicateRecipeId,
        AmbiguousCharacterId
    }

    public sealed class ModularCharacterRecipeCatalogueIssue
    {
        public ModularCharacterRecipeCatalogueIssue(
            ModularCharacterRecipeCatalogueIssueCode code,
            string path,
            RecipeValidationIssueCode? recipeIssueCode = null)
        {
            Code = code;
            Path = path;
            RecipeIssueCode = recipeIssueCode;
        }

        public ModularCharacterRecipeCatalogueIssueCode Code { get; }
        public string Path { get; }
        public RecipeValidationIssueCode? RecipeIssueCode { get; }
    }

    public sealed class ModularCharacterRecipeCatalogueBuildResult
    {
        private readonly ReadOnlyCollection<ModularCharacterRecipeCatalogueIssue> issues;

        internal ModularCharacterRecipeCatalogueBuildResult(
            ModularCharacterRecipeCatalogue catalogue,
            IList<ModularCharacterRecipeCatalogueIssue> issues)
        {
            Catalogue = catalogue;
            this.issues = new ReadOnlyCollection<ModularCharacterRecipeCatalogueIssue>(
                new List<ModularCharacterRecipeCatalogueIssue>(issues));
        }

        public bool Succeeded => Catalogue != null;
        public ModularCharacterRecipeCatalogue Catalogue { get; }
        public IReadOnlyList<ModularCharacterRecipeCatalogueIssue> Issues => issues;
    }

    /// <summary>
    /// Immutable recipe catalogue assembled only from providers supplied to <see cref="Build"/>.
    /// </summary>
    public sealed class ModularCharacterRecipeCatalogue
    {
        private readonly ReadOnlyCollection<ModularCharacterRecipe> recipes;
        private readonly ReadOnlyDictionary<string, ModularCharacterRecipe> recipesByRecipeId;
        private readonly ReadOnlyDictionary<string, ModularCharacterRecipe> recipesByCharacterId;

        private ModularCharacterRecipeCatalogue(IList<ModularCharacterRecipe> recipes)
        {
            var recipeCopy = new List<ModularCharacterRecipe>(recipes);
            this.recipes = new ReadOnlyCollection<ModularCharacterRecipe>(recipeCopy);

            var recipeIds = new Dictionary<string, ModularCharacterRecipe>(StringComparer.Ordinal);
            var characterIds = new Dictionary<string, ModularCharacterRecipe>(StringComparer.Ordinal);
            for (var index = 0; index < recipeCopy.Count; index++)
            {
                var recipe = recipeCopy[index];
                recipeIds.Add(recipe.RecipeId, recipe);
                characterIds.Add(recipe.CharacterId, recipe);
            }

            recipesByRecipeId = new ReadOnlyDictionary<string, ModularCharacterRecipe>(recipeIds);
            recipesByCharacterId = new ReadOnlyDictionary<string, ModularCharacterRecipe>(characterIds);
        }

        public IReadOnlyList<ModularCharacterRecipe> Recipes => recipes;

        public bool TryGetByRecipeId(string recipeId, out ModularCharacterRecipe recipe)
        {
            if (recipeId == null)
            {
                recipe = null;
                return false;
            }

            return recipesByRecipeId.TryGetValue(recipeId, out recipe);
        }

        public bool TryGetByCharacterId(string characterId, out ModularCharacterRecipe recipe)
        {
            if (characterId == null)
            {
                recipe = null;
                return false;
            }

            return recipesByCharacterId.TryGetValue(characterId, out recipe);
        }

        public static ModularCharacterRecipeCatalogueBuildResult Build(
            IEnumerable<IModularCharacterRecipeProvider> providers)
        {
            var issues = new List<ModularCharacterRecipeCatalogueIssue>();
            var providerSnapshots = SnapshotProviders(providers, issues);

            AddDuplicateProviderIssues(providerSnapshots, issues);
            var recipeSnapshots = SnapshotRecipes(providerSnapshots, issues);
            ValidateRecipes(recipeSnapshots, issues);
            AddDuplicateRecipeIssues(recipeSnapshots, issues);
            SortIssues(issues);

            if (issues.Count != 0)
                return new ModularCharacterRecipeCatalogueBuildResult(null, issues);

            var recipes = new List<ModularCharacterRecipe>(recipeSnapshots.Count);
            for (var index = 0; index < recipeSnapshots.Count; index++)
                recipes.Add(recipeSnapshots[index].Recipe);
            recipes.Sort((left, right) => string.CompareOrdinal(left.RecipeId, right.RecipeId));

            return new ModularCharacterRecipeCatalogueBuildResult(
                new ModularCharacterRecipeCatalogue(recipes),
                issues);
        }

        private static List<ProviderSnapshot> SnapshotProviders(
            IEnumerable<IModularCharacterRecipeProvider> providers,
            ICollection<ModularCharacterRecipeCatalogueIssue> issues)
        {
            var snapshots = new List<ProviderSnapshot>();
            if (providers == null)
            {
                issues.Add(new ModularCharacterRecipeCatalogueIssue(
                    ModularCharacterRecipeCatalogueIssueCode.NullProviderCollection,
                    "providers"));
                return snapshots;
            }

            var suppliedProviders = new List<IModularCharacterRecipeProvider>();
            try
            {
                suppliedProviders.AddRange(providers);
            }
            catch (Exception)
            {
                issues.Add(new ModularCharacterRecipeCatalogueIssue(
                    ModularCharacterRecipeCatalogueIssueCode.InvalidProvider,
                    "providers"));
                return snapshots;
            }

            for (var providerIndex = 0; providerIndex < suppliedProviders.Count; providerIndex++)
            {
                var provider = suppliedProviders[providerIndex];
                if (provider == null)
                {
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.NullProvider,
                        "providers"));
                    continue;
                }

                string moduleId;
                try
                {
                    moduleId = provider.ModuleId;
                }
                catch (Exception)
                {
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.InvalidProvider,
                        "providers.moduleId"));
                    continue;
                }

                var providerPath = ProviderPath(moduleId);
                if (!ModularCharacterRecipeValidator.IsStableId(moduleId))
                {
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.InvalidModuleId,
                        providerPath + ".moduleId"));
                }

                IReadOnlyList<ModularCharacterRecipe> suppliedRecipes;
                try
                {
                    suppliedRecipes = provider.Recipes;
                }
                catch (Exception)
                {
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.InvalidProvider,
                        providerPath + ".recipes"));
                    continue;
                }

                if (suppliedRecipes == null)
                {
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.NullRecipeCollection,
                        providerPath + ".recipes"));
                    continue;
                }

                List<ModularCharacterRecipe> recipeCopy;
                try
                {
                    recipeCopy = new List<ModularCharacterRecipe>(suppliedRecipes);
                }
                catch (Exception)
                {
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.InvalidProvider,
                        providerPath + ".recipes"));
                    continue;
                }

                snapshots.Add(new ProviderSnapshot(moduleId, providerPath, recipeCopy));
            }

            return snapshots;
        }

        private static void AddDuplicateProviderIssues(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<ModularCharacterRecipeCatalogueIssue> issues)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < providers.Count; index++)
            {
                var moduleId = providers[index].ModuleId;
                if (!ModularCharacterRecipeValidator.IsStableId(moduleId))
                    continue;
                counts[moduleId] = counts.TryGetValue(moduleId, out var count) ? count + 1 : 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.DuplicateProviderModuleId,
                        ProviderPath(pair.Key) + ".moduleId"));
                }
            }
        }

        private static List<RecipeSnapshot> SnapshotRecipes(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<ModularCharacterRecipeCatalogueIssue> issues)
        {
            var recipes = new List<RecipeSnapshot>();
            for (var providerIndex = 0; providerIndex < providers.Count; providerIndex++)
            {
                var provider = providers[providerIndex];
                for (var recipeIndex = 0; recipeIndex < provider.Recipes.Count; recipeIndex++)
                {
                    var recipe = provider.Recipes[recipeIndex];
                    if (recipe == null)
                    {
                        issues.Add(new ModularCharacterRecipeCatalogueIssue(
                            ModularCharacterRecipeCatalogueIssueCode.NullRecipe,
                            provider.Path + ".recipes"));
                        continue;
                    }

                    recipes.Add(new RecipeSnapshot(
                        recipe,
                        RecipePath(provider.Path, recipe.RecipeId)));
                }
            }
            return recipes;
        }

        private static void ValidateRecipes(
            IReadOnlyList<RecipeSnapshot> recipes,
            ICollection<ModularCharacterRecipeCatalogueIssue> issues)
        {
            for (var index = 0; index < recipes.Count; index++)
            {
                var snapshot = recipes[index];
                var recipeIssues = ModularCharacterRecipeValidator.Validate(snapshot.Recipe);
                for (var issueIndex = 0; issueIndex < recipeIssues.Count; issueIndex++)
                {
                    var recipeIssue = recipeIssues[issueIndex];
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.InvalidRecipe,
                        snapshot.Path + "." + recipeIssue.Path,
                        recipeIssue.Code));
                }
            }
        }

        private static void AddDuplicateRecipeIssues(
            IReadOnlyList<RecipeSnapshot> recipes,
            ICollection<ModularCharacterRecipeCatalogueIssue> issues)
        {
            var recipeIds = new Dictionary<string, int>(StringComparer.Ordinal);
            var characterIds = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < recipes.Count; index++)
            {
                var recipe = recipes[index].Recipe;
                AddCount(recipeIds, recipe.RecipeId);
                AddCount(characterIds, recipe.CharacterId);
            }

            foreach (var pair in recipeIds)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.DuplicateRecipeId,
                        "recipes[\"" + EscapePathSegment(pair.Key) + "\"].recipeId"));
                }
            }

            foreach (var pair in characterIds)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new ModularCharacterRecipeCatalogueIssue(
                        ModularCharacterRecipeCatalogueIssueCode.AmbiguousCharacterId,
                        "characters[\"" + EscapePathSegment(pair.Key) + "\"].characterId"));
                }
            }
        }

        private static void AddCount(IDictionary<string, int> counts, string id)
        {
            if (!ModularCharacterRecipeValidator.IsStableId(id))
                return;
            counts[id] = counts.TryGetValue(id, out var count) ? count + 1 : 1;
        }

        private static void SortIssues(List<ModularCharacterRecipeCatalogueIssue> issues)
        {
            issues.Sort((left, right) =>
            {
                var path = string.CompareOrdinal(left.Path, right.Path);
                if (path != 0)
                    return path;
                var code = left.Code.CompareTo(right.Code);
                if (code != 0)
                    return code;
                return Nullable.Compare(left.RecipeIssueCode, right.RecipeIssueCode);
            });
        }

        private static string ProviderPath(string moduleId)
        {
            return string.IsNullOrEmpty(moduleId)
                ? "providers"
                : "providers[\"" + EscapePathSegment(moduleId) + "\"]";
        }

        private static string RecipePath(string providerPath, string recipeId)
        {
            return string.IsNullOrEmpty(recipeId)
                ? providerPath + ".recipes"
                : providerPath + ".recipes[\"" + EscapePathSegment(recipeId) + "\"]";
        }

        private static string EscapePathSegment(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private sealed class ProviderSnapshot
        {
            public ProviderSnapshot(string moduleId, string path, List<ModularCharacterRecipe> recipes)
            {
                ModuleId = moduleId;
                Path = path;
                Recipes = recipes;
            }

            public string ModuleId { get; }
            public string Path { get; }
            public List<ModularCharacterRecipe> Recipes { get; }
        }

        private sealed class RecipeSnapshot
        {
            public RecipeSnapshot(ModularCharacterRecipe recipe, string path)
            {
                Recipe = recipe;
                Path = path;
            }

            public ModularCharacterRecipe Recipe { get; }
            public string Path { get; }
        }
    }
}
