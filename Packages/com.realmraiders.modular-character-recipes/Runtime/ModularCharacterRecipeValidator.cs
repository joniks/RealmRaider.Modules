using System;
using System.Collections.Generic;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterRecipes
{
    public enum RecipeValidationIssueCode
    {
        MissingRecipe,
        UnsupportedSchemaVersion,
        MissingId,
        InvalidId,
        UnknownFamily,
        UnknownSlot,
        DuplicateSlot,
        MissingBaseBody,
        MultipleBaseBodies,
        FamilyMismatch,
        SlotMismatch,
        MissingSourceId,
        UnorderedSourceIds,
        DuplicateSourceId,
        MissingField,
        DuplicateField,
        UnknownField
    }

    public sealed class RecipeValidationIssue
    {
        public RecipeValidationIssue(RecipeValidationIssueCode code, string path)
        {
            Code = code;
            Path = path;
        }

        public RecipeValidationIssueCode Code { get; }
        public string Path { get; }
    }

    public static class ModularCharacterRecipeValidator
    {
        public const int SupportedSchemaVersion = 1;

        private static readonly string[] RequiredRootFields =
        {
            "schemaVersion",
            "recipeId",
            "characterId",
            "visualProfileId",
            "family",
            "rigProfileId",
            "animationProfileId",
            "modules",
            "paletteId",
            "lodBudgetId",
            "sourceIds"
        };

        public static IReadOnlyList<RecipeValidationIssue> Validate(ModularCharacterRecipe recipe)
        {
            var issues = new List<RecipeValidationIssue>();
            if (recipe == null)
            {
                issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.MissingRecipe, "recipe"));
                return issues.AsReadOnly();
            }

            if (recipe.SchemaVersion != SupportedSchemaVersion)
                issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.UnsupportedSchemaVersion, "schemaVersion"));

            ValidateId(recipe.RecipeId, "recipeId", issues);
            ValidateId(recipe.CharacterId, "characterId", issues);
            ValidateId(recipe.VisualProfileId, "visualProfileId", issues);
            ValidateFamily(recipe.Family, "family", issues);
            ValidateId(recipe.RigProfileId, "rigProfileId", issues);
            ValidateId(recipe.AnimationProfileId, "animationProfileId", issues);
            ValidateModules(recipe, issues);
            ValidateId(recipe.PaletteId, "paletteId", issues);
            ValidateId(recipe.LodBudgetId, "lodBudgetId", issues);
            ValidateSources(recipe.SourceIds, issues);
            return issues.AsReadOnly();
        }

        public static IReadOnlyList<RecipeValidationIssue> ValidateRootFields(IEnumerable<string> fieldNames)
        {
            var issues = new List<RecipeValidationIssue>();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            if (fieldNames != null)
            {
                foreach (var fieldName in fieldNames)
                {
                    var normalized = fieldName ?? string.Empty;
                    counts[normalized] = counts.TryGetValue(normalized, out var count) ? count + 1 : 1;
                }
            }

            for (var index = 0; index < RequiredRootFields.Length; index++)
            {
                var required = RequiredRootFields[index];
                if (!counts.TryGetValue(required, out var count))
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.MissingField, required));
                else if (count > 1)
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.DuplicateField, required));
                counts.Remove(required);
            }

            var unknown = new List<string>(counts.Keys);
            unknown.Sort(StringComparer.Ordinal);
            for (var index = 0; index < unknown.Count; index++)
                issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.UnknownField, unknown[index]));

            return issues.AsReadOnly();
        }

        internal static List<CharacterRecipeModule> CanonicalModules(IReadOnlyList<CharacterRecipeModule> modules)
        {
            var canonical = new List<CharacterRecipeModule>();
            for (var index = 0; index < modules.Count; index++)
                canonical.Add(modules[index]);
            canonical.Sort(CompareModules);
            return canonical;
        }

        internal static string SlotName(CharacterRecipeSlot slot)
        {
            switch (slot)
            {
                case CharacterRecipeSlot.BaseBody: return "base_body";
                case CharacterRecipeSlot.Head: return "head";
                case CharacterRecipeSlot.Back: return "back";
                case CharacterRecipeSlot.Arms: return "arms";
                case CharacterRecipeSlot.Accent: return "accent";
                default: return null;
            }
        }

        internal static string FamilyName(CharacterBodyFamily family)
        {
            switch (family)
            {
                case CharacterBodyFamily.Humanoid: return "humanoid";
                case CharacterBodyFamily.LargeCreature: return "large-creature";
                case CharacterBodyFamily.Beast: return "beast";
                default: return null;
            }
        }

        private static void ValidateModules(ModularCharacterRecipe recipe, ICollection<RecipeValidationIssue> issues)
        {
            var modules = CanonicalModules(recipe.Modules);
            var seenSlots = new HashSet<CharacterRecipeSlot>();
            var baseBodyCount = 0;

            for (var index = 0; index < modules.Count; index++)
            {
                var module = modules[index];
                var path = "modules[" + index + "]";
                if (module == null)
                {
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.MissingId, path + ".moduleId"));
                    continue;
                }

                var assignedSlotName = SlotName(module.AssignedSlot);
                var declaredSlotName = SlotName(module.DeclaredSlot);
                if (assignedSlotName == null)
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.UnknownSlot, path + ".slot"));
                if (declaredSlotName == null)
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.UnknownSlot, path + ".declaredSlot"));

                if (assignedSlotName != null)
                {
                    if (!seenSlots.Add(module.AssignedSlot))
                        issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.DuplicateSlot, path + ".slot"));
                    if (module.AssignedSlot == CharacterRecipeSlot.BaseBody)
                        baseBodyCount++;
                }

                ValidateId(module.ModuleId, path + ".moduleId", issues);
                ValidateFamily(module.ModuleFamily, path + ".family", issues);
                if (FamilyName(module.ModuleFamily) != null && module.ModuleFamily != recipe.Family)
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.FamilyMismatch, path + ".family"));
                if (assignedSlotName != null && declaredSlotName != null && module.AssignedSlot != module.DeclaredSlot)
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.SlotMismatch, path + ".declaredSlot"));
            }

            if (baseBodyCount == 0)
                issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.MissingBaseBody, "modules.base_body"));
            else if (baseBodyCount > 1)
                issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.MultipleBaseBodies, "modules.base_body"));
        }

        private static void ValidateSources(IReadOnlyList<string> sourceIds, ICollection<RecipeValidationIssue> issues)
        {
            if (sourceIds.Count == 0)
            {
                issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.MissingSourceId, "sourceIds"));
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            string previous = null;
            for (var index = 0; index < sourceIds.Count; index++)
            {
                var source = sourceIds[index];
                var path = "sourceIds[" + index + "]";
                if (string.IsNullOrWhiteSpace(source))
                {
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.MissingSourceId, path));
                    continue;
                }

                ValidateId(source, path, issues);
                if (!seen.Add(source))
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.DuplicateSourceId, path));
                if (previous != null)
                {
                    var comparison = string.CompareOrdinal(previous, source);
                    if (comparison > 0)
                        issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.UnorderedSourceIds, path));
                }
                previous = source;
            }
        }

        private static void ValidateFamily(CharacterBodyFamily family, string path, ICollection<RecipeValidationIssue> issues)
        {
            if (FamilyName(family) == null)
                issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.UnknownFamily, path));
        }

        private static void ValidateId(string value, string path, ICollection<RecipeValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.MissingId, path));
                return;
            }

            if (!IsAlphaNumeric(value[0]) || !IsAlphaNumeric(value[value.Length - 1]))
            {
                issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.InvalidId, path));
                return;
            }

            var previousSeparator = false;
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                var separator = character == '.' || character == '-';
                if (!IsAlphaNumeric(character) && !separator || separator && previousSeparator)
                {
                    issues.Add(new RecipeValidationIssue(RecipeValidationIssueCode.InvalidId, path));
                    return;
                }
                previousSeparator = separator;
            }
        }

        private static bool IsAlphaNumeric(char character)
        {
            return character >= 'a' && character <= 'z' || character >= '0' && character <= '9';
        }

        private static int CompareModules(CharacterRecipeModule left, CharacterRecipeModule right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return -1;
            if (right == null) return 1;
            var slot = ((int)left.AssignedSlot).CompareTo((int)right.AssignedSlot);
            if (slot != 0) return slot;
            return string.CompareOrdinal(left.ModuleId, right.ModuleId);
        }
    }
}
