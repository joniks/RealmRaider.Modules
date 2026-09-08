using System;
using System.Security.Cryptography;
using System.Text;

namespace RealmRaiders.Modules.CharacterArtManifests
{
    public static class CharacterArtIntakeManifestCanonicalizer
    {
        public static byte[] SerializeUtf8(CharacterArtIntakeManifest manifest)
        {
            var issues = CharacterArtIntakeManifestValidator.Validate(manifest);
            if (issues.Count != 0)
            {
                throw new ArgumentException(
                    "Character-art manifest must pass deterministic validation before canonical serialization.",
                    nameof(manifest));
            }

            var clips = CharacterArtIntakeManifestValidator.CanonicalMotionClips(manifest.MotionClips);
            var json = new StringBuilder(1536);
            json.Append("{\"schemaVersion\":").Append(manifest.SchemaVersion);
            AppendString(json, "characterId", manifest.CharacterId);
            AppendString(json, "sourceId", manifest.SourceId);
            AppendString(json, "family", CharacterArtIntakeManifestValidator.FamilyName(manifest.Family));
            AppendString(json, "title", manifest.Title);
            AppendString(json, "creator", manifest.Creator);
            AppendString(json, "directSourceUrl", manifest.DirectSourceUrl);
            AppendString(json, "licenseName", manifest.LicenseName);
            AppendString(json, "licenseLegalUrl", manifest.LicenseLegalUrl);
            AppendString(json, "attribution", manifest.Attribution);
            AppendString(json, "changeNote", manifest.ChangeNote);
            AppendString(json, "archiveSha256", manifest.ArchiveSha256);
            AppendString(json, "selectedSourceFile", manifest.SelectedSourceFile);
            AppendString(json, "rigProfileId", manifest.RigProfileId);
            json.Append(",\"motionClips\":[");
            for (var index = 0; index < clips.Count; index++)
            {
                if (index > 0) json.Append(',');
                var clip = clips[index];
                json.Append("{\"key\":");
                AppendQuoted(json, CharacterArtIntakeManifestValidator.MotionClipKeyName(clip.Key));
                json.Append(",\"clipId\":");
                AppendQuoted(json, clip.ClipId);
                json.Append('}');
            }
            json.Append("],\"lodTriangleBudgets\":[");
            for (var index = 0; index < manifest.LodTriangleBudgets.Count; index++)
            {
                if (index > 0) json.Append(',');
                var budget = manifest.LodTriangleBudgets[index];
                json.Append("{\"level\":");
                AppendQuoted(json, CharacterArtIntakeManifestValidator.LodLevelName(budget.Level));
                json.Append(",\"maxTriangles\":").Append(budget.MaxTriangles).Append('}');
            }
            json.Append(']');
            AppendNumber(json, "maxRendererCount", manifest.MaxRendererCount);
            AppendNumber(json, "maxMaterialCount", manifest.MaxMaterialCount);
            AppendNumber(json, "maxTextureCount", manifest.MaxTextureCount);
            AppendNumber(json, "maxTextureDimensionPixels", manifest.MaxTextureDimensionPixels);
            AppendBoolean(json, "importSourceColliders", manifest.ImportSourceColliders);
            AppendBoolean(json, "applyRootMotion", manifest.ApplyRootMotion);
            AppendBoolean(json, "importAnimations", manifest.ImportAnimations);
            AppendBoolean(json, "importAnimationEvents", manifest.ImportAnimationEvents);
            AppendBoolean(json, "importEmbeddedMaterials", manifest.ImportEmbeddedMaterials);
            AppendBoolean(json, "importEmbeddedTextures", manifest.ImportEmbeddedTextures);
            json.Append('}');
            return new UTF8Encoding(false, true).GetBytes(json.ToString());
        }

        public static string ContentHash(CharacterArtIntakeManifest manifest)
        {
            var bytes = SerializeUtf8(manifest);
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
            json.Append(",\"").Append(field).Append("\":");
            AppendQuoted(json, value);
        }

        private static void AppendNumber(StringBuilder json, string field, int value)
        {
            json.Append(",\"").Append(field).Append("\":").Append(value);
        }

        private static void AppendBoolean(StringBuilder json, string field, bool value)
        {
            json.Append(",\"").Append(field).Append("\":").Append(value ? "true" : "false");
        }

        private static void AppendQuoted(StringBuilder json, string value)
        {
            json.Append('"');
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                switch (character)
                {
                    case '"': json.Append("\\\""); break;
                    case '\\': json.Append("\\\\"); break;
                    case '\b': json.Append("\\b"); break;
                    case '\f': json.Append("\\f"); break;
                    case '\n': json.Append("\\n"); break;
                    case '\r': json.Append("\\r"); break;
                    case '\t': json.Append("\\t"); break;
                    default:
                        if (character < ' ')
                            json.Append("\\u").Append(((int)character).ToString("x4"));
                        else
                            json.Append(character);
                        break;
                }
            }
            json.Append('"');
        }
    }
}
