using System;
using System.Security.Cryptography;
using System.Text;

namespace RealmRaiders.Modules.CharacterRecipes
{
    public static class ModularCharacterRecipeCanonicalizer
    {
        public static byte[] SerializeUtf8(ModularCharacterRecipe recipe)
        {
            var issues = ModularCharacterRecipeValidator.Validate(recipe);
            if (issues.Count != 0)
                throw new ArgumentException("Recipe must pass deterministic validation before canonical serialization.", nameof(recipe));

            var modules = ModularCharacterRecipeValidator.CanonicalModules(recipe.Modules);
            var json = new StringBuilder(768);
            json.Append("{\"schemaVersion\":").Append(recipe.SchemaVersion);
            AppendString(json, "recipeId", recipe.RecipeId);
            AppendString(json, "characterId", recipe.CharacterId);
            AppendString(json, "visualProfileId", recipe.VisualProfileId);
            AppendString(json, "family", ModularCharacterRecipeValidator.FamilyName(recipe.Family));
            AppendString(json, "rigProfileId", recipe.RigProfileId);
            AppendString(json, "animationProfileId", recipe.AnimationProfileId);
            json.Append(",\"modules\":[");
            for (var index = 0; index < modules.Count; index++)
            {
                if (index > 0) json.Append(',');
                var module = modules[index];
                json.Append("{\"slot\":\"").Append(ModularCharacterRecipeValidator.SlotName(module.AssignedSlot));
                json.Append("\",\"moduleId\":\"").Append(module.ModuleId);
                json.Append("\",\"family\":\"").Append(ModularCharacterRecipeValidator.FamilyName(module.ModuleFamily));
                json.Append("\",\"declaredSlot\":\"").Append(ModularCharacterRecipeValidator.SlotName(module.DeclaredSlot));
                json.Append("\"}");
            }
            json.Append(']');
            AppendString(json, "paletteId", recipe.PaletteId);
            AppendString(json, "lodBudgetId", recipe.LodBudgetId);
            json.Append(",\"sourceIds\":[");
            for (var index = 0; index < recipe.SourceIds.Count; index++)
            {
                if (index > 0) json.Append(',');
                json.Append('\"').Append(recipe.SourceIds[index]).Append('\"');
            }
            json.Append("]}");
            return new UTF8Encoding(false, true).GetBytes(json.ToString());
        }

        public static string ContentHash(ModularCharacterRecipe recipe)
        {
            var bytes = SerializeUtf8(recipe);
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(bytes);
                var text = new StringBuilder(hash.Length * 2);
                for (var index = 0; index < hash.Length; index++)
                    text.Append(hash[index].ToString("x2"));
                return text.ToString();
            }
        }

        private static void AppendString(StringBuilder json, string field, string value)
        {
            json.Append(",\"").Append(field).Append("\":\"").Append(value).Append('\"');
        }
    }
}
