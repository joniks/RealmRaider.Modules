using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterRecipes.Tests
{
    public sealed class ModularCharacterRecipeTests
    {
        [Test]
        public void ValidRecipe_SerializesCanonicalUtf8AndHashesDeterministically()
        {
            var first = ValidRecipe(new[]
            {
                Module(CharacterRecipeSlot.Head, "realmraiders.module.beast.head.mossback.v1"),
                Module(CharacterRecipeSlot.BaseBody, "realmraiders.module.beast.base-body.wolf.v1")
            });
            var reordered = ValidRecipe(new[]
            {
                Module(CharacterRecipeSlot.BaseBody, "realmraiders.module.beast.base-body.wolf.v1"),
                Module(CharacterRecipeSlot.Head, "realmraiders.module.beast.head.mossback.v1")
            });

            Assert.That(ModularCharacterRecipeValidator.Validate(first), Is.Empty);
            Assert.That(ModularCharacterRecipeCanonicalizer.SerializeUtf8(first), Is.EqualTo(ModularCharacterRecipeCanonicalizer.SerializeUtf8(reordered)));
            Assert.That(ModularCharacterRecipeCanonicalizer.ContentHash(first), Is.EqualTo(ModularCharacterRecipeCanonicalizer.ContentHash(reordered)));
            Assert.That(ModularCharacterRecipeCanonicalizer.ContentHash(first), Does.Match("^[0-9a-f]{64}$"));
            Assert.That(Encoding.UTF8.GetString(ModularCharacterRecipeCanonicalizer.SerializeUtf8(first)), Does.StartWith("{\"schemaVersion\":1,\"recipeId\":"));
        }

        [Test]
        public void Validator_RejectsMissingInvalidAndUnsupportedRootValues()
        {
            var recipe = new ModularCharacterRecipe(
                2,
                "Bad/Path",
                string.Empty,
                "realmraiders.wolf..visual",
                (CharacterBodyFamily)99,
                "realmraiders.rig.beast.v1",
                "realmraiders.anim.beast.v1",
                new[] { Module(CharacterRecipeSlot.BaseBody, "realmraiders.module.beast.base-body.wolf.v1") },
                "realmraiders.palette.sylvan.moss.v1",
                "realmraiders.budget.beast.v1",
                new[] { "realmraiders.source.wolf" });

            var codes = ModularCharacterRecipeValidator.Validate(recipe).Select(issue => issue.Code).ToArray();

            Assert.That(codes, Does.Contain(RecipeValidationIssueCode.UnsupportedSchemaVersion));
            Assert.That(codes, Does.Contain(RecipeValidationIssueCode.InvalidId));
            Assert.That(codes, Does.Contain(RecipeValidationIssueCode.MissingId));
            Assert.That(codes, Does.Contain(RecipeValidationIssueCode.UnknownFamily));
            Assert.Throws<ArgumentException>(() => ModularCharacterRecipeCanonicalizer.SerializeUtf8(recipe));
        }

        [Test]
        public void Validator_RejectsUnknownDuplicateMissingAndMultipleBaseSlots()
        {
            var unknown = ValidRecipe(new[] { Module((CharacterRecipeSlot)99, "realmraiders.module.beast.unknown.wolf.v1") });
            var missing = ValidRecipe(new[] { Module(CharacterRecipeSlot.Head, "realmraiders.module.beast.head.wolf.v1") });
            var multiple = ValidRecipe(new[]
            {
                Module(CharacterRecipeSlot.BaseBody, "realmraiders.module.beast.base-body.wolf-a.v1"),
                Module(CharacterRecipeSlot.BaseBody, "realmraiders.module.beast.base-body.wolf-b.v1")
            });

            Assert.That(Codes(unknown), Does.Contain(RecipeValidationIssueCode.UnknownSlot).And.Contain(RecipeValidationIssueCode.MissingBaseBody));
            Assert.That(Codes(missing), Does.Contain(RecipeValidationIssueCode.MissingBaseBody));
            Assert.That(Codes(multiple), Does.Contain(RecipeValidationIssueCode.DuplicateSlot).And.Contain(RecipeValidationIssueCode.MultipleBaseBodies));
        }

        [Test]
        public void Validator_RejectsModuleFamilyAndSlotMismatch()
        {
            var mismatched = ValidRecipe(new[]
            {
                new CharacterRecipeModule(
                    CharacterRecipeSlot.BaseBody,
                    "realmraiders.module.large-creature.head.ent.v1",
                    CharacterBodyFamily.LargeCreature,
                    CharacterRecipeSlot.Head)
            });

            Assert.That(Codes(mismatched), Does.Contain(RecipeValidationIssueCode.FamilyMismatch).And.Contain(RecipeValidationIssueCode.SlotMismatch));
        }

        [Test]
        public void Validator_RejectsUnorderedAndDuplicateSources()
        {
            var missing = ValidRecipe(sourceIds: new string[0]);
            var unordered = ValidRecipe(sourceIds: new[] { "realmraiders.source.zeta", "realmraiders.source.alpha" });
            var duplicate = ValidRecipe(sourceIds: new[] { "realmraiders.source.alpha", "realmraiders.source.alpha" });
            var nonAdjacentDuplicate = ValidRecipe(sourceIds: new[]
            {
                "realmraiders.source.alpha",
                "realmraiders.source.beta",
                "realmraiders.source.alpha"
            });

            Assert.That(Codes(missing), Does.Contain(RecipeValidationIssueCode.MissingSourceId));
            Assert.That(Codes(unordered), Does.Contain(RecipeValidationIssueCode.UnorderedSourceIds));
            Assert.That(Codes(duplicate), Does.Contain(RecipeValidationIssueCode.DuplicateSourceId));
            Assert.That(Codes(nonAdjacentDuplicate), Does.Contain(RecipeValidationIssueCode.DuplicateSourceId).And.Contain(RecipeValidationIssueCode.UnorderedSourceIds));
            Assert.Throws<ArgumentException>(() => ModularCharacterRecipeCanonicalizer.ContentHash(unordered));
        }

        [Test]
        public void RootFieldPolicy_RejectsMutableTimestampPathRandomAndUnknownFields()
        {
            var fields = CanonicalRootFields().Concat(new[] { "displayName", "timestamp", "absolutePath", "randomSeed" });
            var issues = ModularCharacterRecipeValidator.ValidateRootFields(fields).ToArray();

            Assert.That(issues.Count(issue => issue.Code == RecipeValidationIssueCode.UnknownField), Is.EqualTo(4));
            Assert.That(issues.Select(issue => issue.Path), Does.Contain("displayName").And.Contain("timestamp").And.Contain("absolutePath").And.Contain("randomSeed"));
        }

        [Test]
        public void Contracts_AreImmutableAndRuntimeHasNoUnityOrGameRuntimeDependency()
        {
            Assert.That(typeof(ModularCharacterRecipe).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterRecipeModule).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);

            var recipe = ValidRecipe();
            Assert.Throws<NotSupportedException>(() => ((IList<string>)recipe.SourceIds).Add("realmraiders.source.other"));
            Assert.Throws<NotSupportedException>(() => ((IList<CharacterRecipeModule>)recipe.Modules).Clear());

            var dependencies = typeof(ModularCharacterRecipe).Assembly.GetReferencedAssemblies().Select(assembly => assembly.Name).ToArray();
            Assert.That(dependencies, Has.Member("RealmRaiders.ModuleContracts"));
            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static ModularCharacterRecipe ValidRecipe(
            IEnumerable<CharacterRecipeModule> modules = null,
            IEnumerable<string> sourceIds = null)
        {
            return new ModularCharacterRecipe(
                ModularCharacterRecipeValidator.SupportedSchemaVersion,
                "realmraiders.recipe.sylvan-wolf.mossback.v1",
                "realmraiders.sylvan-wolf",
                "realmraiders.sylvan-wolf.prototype",
                CharacterBodyFamily.Beast,
                "realmraiders.rig.beast.v1",
                "realmraiders.anim.beast.v1",
                modules ?? new[] { Module(CharacterRecipeSlot.BaseBody, "realmraiders.module.beast.base-body.wolf.v1") },
                "realmraiders.palette.sylvan.moss.v1",
                "realmraiders.budget.beast.v1",
                sourceIds ?? new[] { "realmraiders.source.wolf", "realmraiders.source.wolf-texture" });
        }

        private static CharacterRecipeModule Module(CharacterRecipeSlot slot, string id)
        {
            return new CharacterRecipeModule(slot, id, CharacterBodyFamily.Beast, slot);
        }

        private static RecipeValidationIssueCode[] Codes(ModularCharacterRecipe recipe)
        {
            return ModularCharacterRecipeValidator.Validate(recipe).Select(issue => issue.Code).ToArray();
        }

        private static IEnumerable<string> CanonicalRootFields()
        {
            return new[]
            {
                "schemaVersion", "recipeId", "characterId", "visualProfileId", "family",
                "rigProfileId", "animationProfileId", "modules", "paletteId", "lodBudgetId", "sourceIds"
            };
        }
    }
}
