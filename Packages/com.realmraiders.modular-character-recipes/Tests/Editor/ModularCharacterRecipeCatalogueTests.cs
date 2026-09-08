using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterRecipes.Tests
{
    public sealed class ModularCharacterRecipeCatalogueTests
    {
        [Test]
        public void Build_SortsRecipesAndPrebuildsExactOrdinalLookupsRegardlessOfInputOrder()
        {
            var alpha = Recipe("alpha");
            var middle = Recipe("middle");
            var zeta = Recipe("zeta");
            var first = ModularCharacterRecipeCatalogue.Build(new IModularCharacterRecipeProvider[]
            {
                new TestProvider("test.provider.zeta", zeta, alpha),
                new TestProvider("test.provider.middle", middle)
            });
            var reordered = ModularCharacterRecipeCatalogue.Build(new IModularCharacterRecipeProvider[]
            {
                new TestProvider("test.provider.middle", middle),
                new TestProvider("test.provider.zeta", alpha, zeta)
            });

            Assert.That(first.Succeeded, Is.True);
            Assert.That(first.Issues, Is.Empty);
            Assert.That(first.Catalogue.Recipes.Select(recipe => recipe.RecipeId), Is.EqualTo(new[]
            {
                "test.recipe.alpha.v1",
                "test.recipe.middle.v1",
                "test.recipe.zeta.v1"
            }));
            Assert.That(
                reordered.Catalogue.Recipes.Select(recipe => recipe.RecipeId),
                Is.EqualTo(first.Catalogue.Recipes.Select(recipe => recipe.RecipeId)));

            Assert.That(first.Catalogue.TryGetByRecipeId("test.recipe.middle.v1", out var byRecipeId), Is.True);
            Assert.That(byRecipeId, Is.SameAs(middle));
            Assert.That(first.Catalogue.TryGetByCharacterId("test.character.middle", out var byCharacterId), Is.True);
            Assert.That(byCharacterId, Is.SameAs(middle));
            Assert.That(first.Catalogue.TryGetByRecipeId("TEST.RECIPE.MIDDLE.V1", out _), Is.False);
            Assert.That(first.Catalogue.TryGetByCharacterId(null, out _), Is.False);

            var lookupFields = typeof(ModularCharacterRecipeCatalogue)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Count(field => field.FieldType.IsGenericType &&
                    field.FieldType.GetGenericTypeDefinition() == typeof(System.Collections.ObjectModel.ReadOnlyDictionary<,>));
            Assert.That(lookupFields, Is.EqualTo(2));
        }

        [Test]
        public void Build_DuplicateProviderRecipeAndCharacterIdsFailClosed()
        {
            var result = ModularCharacterRecipeCatalogue.Build(new IModularCharacterRecipeProvider[]
            {
                new TestProvider("test.provider.duplicate", Recipe("alpha", recipeId: "test.recipe.shared.v1")),
                new TestProvider("test.provider.duplicate", Recipe("beta", characterId: "test.character.shared")),
                new TestProvider("test.provider.unique", Recipe(
                    "gamma",
                    recipeId: "test.recipe.shared.v1",
                    characterId: "test.character.shared"))
            });

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Catalogue, Is.Null);
            var codes = result.Issues.Select(issue => issue.Code).ToArray();
            Assert.That(codes, Has.Member(ModularCharacterRecipeCatalogueIssueCode.DuplicateProviderModuleId));
            Assert.That(codes, Has.Member(ModularCharacterRecipeCatalogueIssueCode.DuplicateRecipeId));
            Assert.That(codes, Has.Member(ModularCharacterRecipeCatalogueIssueCode.AmbiguousCharacterId));
            Assert.That(result.Issues.All(issue => !string.IsNullOrEmpty(issue.Path)), Is.True);
        }

        [Test]
        public void Build_NullAndInvalidProvidersAndRecipesFailClosedWithStableStructuredIssues()
        {
            var providers = new IModularCharacterRecipeProvider[]
            {
                null,
                new TestProvider("Bad/Provider", Recipe("valid")),
                new TestProvider("test.provider.null-recipes", null),
                new TestProvider("test.provider.invalid-recipes", new ModularCharacterRecipe[]
                {
                    null,
                    Recipe("invalid", recipeId: "Bad/Recipe")
                }),
                new ThrowingProvider()
            };

            var first = ModularCharacterRecipeCatalogue.Build(providers);
            var reordered = ModularCharacterRecipeCatalogue.Build(providers.Reverse());

            Assert.That(first.Succeeded, Is.False);
            Assert.That(first.Catalogue, Is.Null);
            var codes = first.Issues.Select(issue => issue.Code).ToArray();
            Assert.That(codes, Has.Member(ModularCharacterRecipeCatalogueIssueCode.NullProvider));
            Assert.That(codes, Has.Member(ModularCharacterRecipeCatalogueIssueCode.InvalidProvider));
            Assert.That(codes, Has.Member(ModularCharacterRecipeCatalogueIssueCode.InvalidModuleId));
            Assert.That(codes, Has.Member(ModularCharacterRecipeCatalogueIssueCode.NullRecipeCollection));
            Assert.That(codes, Has.Member(ModularCharacterRecipeCatalogueIssueCode.NullRecipe));
            Assert.That(codes, Has.Member(ModularCharacterRecipeCatalogueIssueCode.InvalidRecipe));
            Assert.That(first.Issues.Single(issue => issue.Code == ModularCharacterRecipeCatalogueIssueCode.InvalidRecipe).RecipeIssueCode,
                Is.EqualTo(RecipeValidationIssueCode.InvalidId));
            Assert.That(IssueSignatures(reordered), Is.EqualTo(IssueSignatures(first)));

            var missingCollection = ModularCharacterRecipeCatalogue.Build(null);
            Assert.That(missingCollection.Succeeded, Is.False);
            Assert.That(missingCollection.Issues.Single().Code,
                Is.EqualTo(ModularCharacterRecipeCatalogueIssueCode.NullProviderCollection));
        }

        [Test]
        public void CatalogueContractsAndCollections_AreImmutableAndKeepPassiveDependencyBoundary()
        {
            var success = ModularCharacterRecipeCatalogue.Build(new[]
            {
                new TestProvider("test.provider.alpha", Recipe("alpha"))
            });
            var failure = ModularCharacterRecipeCatalogue.Build(new IModularCharacterRecipeProvider[] { null });

            Assert.That(typeof(IModularCharacterRecipeProvider).GetProperty("ModuleId").CanWrite, Is.False);
            Assert.That(typeof(IModularCharacterRecipeProvider).GetProperty("Recipes").CanWrite, Is.False);
            Assert.That(typeof(ModularCharacterRecipeCatalogue).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ModularCharacterRecipeCatalogueBuildResult).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ModularCharacterRecipeCatalogueIssue).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.Throws<NotSupportedException>(() => ((IList<ModularCharacterRecipe>)success.Catalogue.Recipes).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<ModularCharacterRecipeCatalogueIssue>)failure.Issues).Clear());

            var dependencies = typeof(ModularCharacterRecipeCatalogue).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();
            Assert.That(dependencies, Has.Member("RealmRaiders.ModuleContracts"));
            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static string[] IssueSignatures(ModularCharacterRecipeCatalogueBuildResult result)
        {
            return result.Issues
                .Select(issue => issue.Code + "|" + issue.Path + "|" + issue.RecipeIssueCode)
                .ToArray();
        }

        private static ModularCharacterRecipe Recipe(
            string suffix,
            string recipeId = null,
            string characterId = null)
        {
            return new ModularCharacterRecipe(
                ModularCharacterRecipeValidator.SupportedSchemaVersion,
                recipeId ?? "test.recipe." + suffix + ".v1",
                characterId ?? "test.character." + suffix,
                "test.visual." + suffix + ".v1",
                CharacterBodyFamily.Beast,
                "test.rig.beast.v1",
                "test.animation.beast.v1",
                new[]
                {
                    new CharacterRecipeModule(
                        CharacterRecipeSlot.BaseBody,
                        "test.module.base-body." + suffix + ".v1",
                        CharacterBodyFamily.Beast,
                        CharacterRecipeSlot.BaseBody)
                },
                "test.palette." + suffix + ".v1",
                "test.budget.beast.v1",
                new[] { "test.source." + suffix + ".v1" });
        }

        private sealed class TestProvider : IModularCharacterRecipeProvider
        {
            public TestProvider(string moduleId, params ModularCharacterRecipe[] recipes)
            {
                ModuleId = moduleId;
                Recipes = recipes;
            }

            public string ModuleId { get; }
            public IReadOnlyList<ModularCharacterRecipe> Recipes { get; }
        }

        private sealed class ThrowingProvider : IModularCharacterRecipeProvider
        {
            public string ModuleId => throw new InvalidOperationException("Invalid provider fixture.");
            public IReadOnlyList<ModularCharacterRecipe> Recipes => new ModularCharacterRecipe[0];
        }
    }
}
