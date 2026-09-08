using System;
using System.Collections.Generic;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterArtManifests
{
    public enum CharacterArtManifestIssueCode
    {
        MissingManifest,
        UnsupportedSchemaVersion,
        MissingId,
        InvalidId,
        UnknownFamily,
        MissingText,
        InvalidText,
        InvalidUrl,
        InvalidArchiveSha256,
        InvalidSelectedSourceFile,
        MissingMotionClip,
        NullMotionClip,
        UnknownMotionClipKey,
        DuplicateMotionClipKey,
        MissingLodBudget,
        NullLodBudget,
        UnknownLodLevel,
        DuplicateLodLevel,
        UnorderedLodLevels,
        InvalidTriangleBudget,
        NonDescendingTriangleBudgets,
        InvalidRendererBudget,
        InvalidMaterialBudget,
        InvalidTextureBudget,
        InvalidTextureDimensionBudget,
        SourceCollidersNotAllowed,
        RootMotionNotAllowed,
        AnimationEventsNotAllowed,
        MissingField,
        DuplicateField,
        UnknownField
    }

    public sealed class CharacterArtManifestIssue
    {
        public CharacterArtManifestIssue(CharacterArtManifestIssueCode code, string path)
        {
            Code = code;
            Path = path;
        }

        public CharacterArtManifestIssueCode Code { get; }
        public string Path { get; }
    }

    public static class CharacterArtIntakeManifestValidator
    {
        public const int SupportedSchemaVersion = 1;

        private static readonly CharacterArtMotionClipKey[] RequiredMotionClipKeys =
        {
            CharacterArtMotionClipKey.Idle,
            CharacterArtMotionClipKey.Locomotion,
            CharacterArtMotionClipKey.AttackPrimary,
            CharacterArtMotionClipKey.AttackAbility,
            CharacterArtMotionClipKey.Hit,
            CharacterArtMotionClipKey.Death
        };

        private static readonly CharacterArtLodLevel[] RequiredLodLevels =
        {
            CharacterArtLodLevel.Lod0,
            CharacterArtLodLevel.Lod1,
            CharacterArtLodLevel.Lod2
        };

        private static readonly string[] RequiredRootFields =
        {
            "schemaVersion",
            "characterId",
            "sourceId",
            "family",
            "title",
            "creator",
            "directSourceUrl",
            "licenseName",
            "licenseLegalUrl",
            "attribution",
            "changeNote",
            "archiveSha256",
            "selectedSourceFile",
            "rigProfileId",
            "motionClips",
            "lodTriangleBudgets",
            "maxRendererCount",
            "maxMaterialCount",
            "maxTextureCount",
            "maxTextureDimensionPixels",
            "importSourceColliders",
            "applyRootMotion",
            "importAnimations",
            "importAnimationEvents",
            "importEmbeddedMaterials",
            "importEmbeddedTextures"
        };

        public static IReadOnlyList<CharacterArtManifestIssue> Validate(
            CharacterArtIntakeManifest manifest)
        {
            var issues = new List<CharacterArtManifestIssue>();
            if (manifest == null)
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.MissingManifest,
                    "manifest"));
                return issues.AsReadOnly();
            }

            if (manifest.SchemaVersion != SupportedSchemaVersion)
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.UnsupportedSchemaVersion,
                    "schemaVersion"));
            }

            ValidateId(manifest.CharacterId, "characterId", issues);
            ValidateId(manifest.SourceId, "sourceId", issues);
            ValidateFamily(manifest.Family, "family", issues);
            ValidateRequiredText(manifest.Title, "title", issues);
            ValidateRequiredText(manifest.Creator, "creator", issues);
            ValidateUrl(manifest.DirectSourceUrl, "directSourceUrl", issues);
            ValidateRequiredText(manifest.LicenseName, "licenseName", issues);
            ValidateUrl(manifest.LicenseLegalUrl, "licenseLegalUrl", issues);
            ValidateRequiredText(manifest.Attribution, "attribution", issues);
            ValidateRequiredText(manifest.ChangeNote, "changeNote", issues);
            ValidateArchiveSha256(manifest.ArchiveSha256, issues);
            ValidateSelectedSourceFile(manifest.SelectedSourceFile, issues);
            ValidateId(manifest.RigProfileId, "rigProfileId", issues);
            ValidateMotionClips(manifest.MotionClips, issues);
            ValidateLodTriangleBudgets(manifest.LodTriangleBudgets, issues);
            ValidatePositiveBudget(
                manifest.MaxRendererCount,
                CharacterArtManifestIssueCode.InvalidRendererBudget,
                "maxRendererCount",
                issues);
            ValidatePositiveBudget(
                manifest.MaxMaterialCount,
                CharacterArtManifestIssueCode.InvalidMaterialBudget,
                "maxMaterialCount",
                issues);
            ValidatePositiveBudget(
                manifest.MaxTextureCount,
                CharacterArtManifestIssueCode.InvalidTextureBudget,
                "maxTextureCount",
                issues);
            ValidatePositiveBudget(
                manifest.MaxTextureDimensionPixels,
                CharacterArtManifestIssueCode.InvalidTextureDimensionBudget,
                "maxTextureDimensionPixels",
                issues);

            if (manifest.ImportSourceColliders)
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.SourceCollidersNotAllowed,
                    "importSourceColliders"));
            }
            if (manifest.ApplyRootMotion)
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.RootMotionNotAllowed,
                    "applyRootMotion"));
            }
            if (manifest.ImportAnimationEvents)
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.AnimationEventsNotAllowed,
                    "importAnimationEvents"));
            }

            SortIssues(issues);
            return issues.AsReadOnly();
        }

        public static IReadOnlyList<CharacterArtManifestIssue> ValidateRootFields(
            IEnumerable<string> fieldNames)
        {
            var issues = new List<CharacterArtManifestIssue>();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            if (fieldNames != null)
            {
                foreach (var fieldName in fieldNames)
                {
                    var normalized = fieldName ?? string.Empty;
                    counts[normalized] = counts.TryGetValue(normalized, out var count)
                        ? count + 1
                        : 1;
                }
            }

            for (var index = 0; index < RequiredRootFields.Length; index++)
            {
                var required = RequiredRootFields[index];
                if (!counts.TryGetValue(required, out var count))
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.MissingField,
                        required));
                }
                else if (count > 1)
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.DuplicateField,
                        required));
                }
                counts.Remove(required);
            }

            var unknownFields = new List<string>(counts.Keys);
            unknownFields.Sort(StringComparer.Ordinal);
            for (var index = 0; index < unknownFields.Count; index++)
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.UnknownField,
                    unknownFields[index]));
            }

            SortIssues(issues);
            return issues.AsReadOnly();
        }

        internal static List<CharacterArtMotionClip> CanonicalMotionClips(
            IReadOnlyList<CharacterArtMotionClip> motionClips)
        {
            var canonical = new List<CharacterArtMotionClip>();
            for (var index = 0; index < motionClips.Count; index++)
                canonical.Add(motionClips[index]);
            canonical.Sort(CompareMotionClips);
            return canonical;
        }

        internal static string MotionClipKeyName(CharacterArtMotionClipKey key)
        {
            switch (key)
            {
                case CharacterArtMotionClipKey.Idle: return "idle";
                case CharacterArtMotionClipKey.Locomotion: return "locomotion";
                case CharacterArtMotionClipKey.AttackPrimary: return "attack_primary";
                case CharacterArtMotionClipKey.AttackAbility: return "attack_ability";
                case CharacterArtMotionClipKey.Hit: return "hit";
                case CharacterArtMotionClipKey.Death: return "death";
                default: return null;
            }
        }

        internal static string LodLevelName(CharacterArtLodLevel level)
        {
            switch (level)
            {
                case CharacterArtLodLevel.Lod0: return "lod0";
                case CharacterArtLodLevel.Lod1: return "lod1";
                case CharacterArtLodLevel.Lod2: return "lod2";
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

        private static void ValidateMotionClips(
            IReadOnlyList<CharacterArtMotionClip> motionClips,
            ICollection<CharacterArtManifestIssue> issues)
        {
            var canonical = CanonicalMotionClips(motionClips);
            var seen = new HashSet<CharacterArtMotionClipKey>();
            for (var index = 0; index < canonical.Count; index++)
            {
                var clip = canonical[index];
                if (clip == null)
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.NullMotionClip,
                        "motionClips"));
                    continue;
                }

                var keyName = MotionClipKeyName(clip.Key);
                var path = keyName == null
                    ? "motionClips[" + index + "]"
                    : "motionClips." + keyName;
                if (keyName == null)
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.UnknownMotionClipKey,
                        path + ".key"));
                }
                else if (!seen.Add(clip.Key))
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.DuplicateMotionClipKey,
                        path + ".key"));
                }

                ValidateId(clip.ClipId, path + ".clipId", issues);
            }

            for (var index = 0; index < RequiredMotionClipKeys.Length; index++)
            {
                var required = RequiredMotionClipKeys[index];
                if (!seen.Contains(required))
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.MissingMotionClip,
                        "motionClips." + MotionClipKeyName(required)));
                }
            }
        }

        private static void ValidateLodTriangleBudgets(
            IReadOnlyList<CharacterArtLodTriangleBudget> lodBudgets,
            ICollection<CharacterArtManifestIssue> issues)
        {
            var seen = new HashSet<CharacterArtLodLevel>();
            var completeOrderedSet = lodBudgets.Count == RequiredLodLevels.Length;
            for (var index = 0; index < lodBudgets.Count; index++)
            {
                var budget = lodBudgets[index];
                var indexedPath = "lodTriangleBudgets[" + index + "]";
                if (budget == null)
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.NullLodBudget,
                        indexedPath));
                    completeOrderedSet = false;
                    continue;
                }

                var levelName = LodLevelName(budget.Level);
                var path = levelName == null
                    ? indexedPath
                    : "lodTriangleBudgets." + levelName;
                if (levelName == null)
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.UnknownLodLevel,
                        path + ".level"));
                    completeOrderedSet = false;
                }
                else
                {
                    if (!seen.Add(budget.Level))
                    {
                        issues.Add(new CharacterArtManifestIssue(
                            CharacterArtManifestIssueCode.DuplicateLodLevel,
                            path + ".level"));
                        completeOrderedSet = false;
                    }
                    if (index >= RequiredLodLevels.Length || budget.Level != RequiredLodLevels[index])
                    {
                        issues.Add(new CharacterArtManifestIssue(
                            CharacterArtManifestIssueCode.UnorderedLodLevels,
                            indexedPath + ".level"));
                        completeOrderedSet = false;
                    }
                }

                if (budget.MaxTriangles <= 0)
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.InvalidTriangleBudget,
                        path + ".maxTriangles"));
                    completeOrderedSet = false;
                }
            }

            for (var index = 0; index < RequiredLodLevels.Length; index++)
            {
                var required = RequiredLodLevels[index];
                if (!seen.Contains(required))
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.MissingLodBudget,
                        "lodTriangleBudgets." + LodLevelName(required)));
                }
            }

            if (!completeOrderedSet)
                return;

            for (var index = 1; index < lodBudgets.Count; index++)
            {
                if (lodBudgets[index - 1].MaxTriangles <= lodBudgets[index].MaxTriangles)
                {
                    issues.Add(new CharacterArtManifestIssue(
                        CharacterArtManifestIssueCode.NonDescendingTriangleBudgets,
                        "lodTriangleBudgets." + LodLevelName(lodBudgets[index].Level) + ".maxTriangles"));
                }
            }
        }

        private static void ValidateFamily(
            CharacterBodyFamily family,
            string path,
            ICollection<CharacterArtManifestIssue> issues)
        {
            if (FamilyName(family) == null)
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.UnknownFamily,
                    path));
            }
        }

        private static void ValidateId(
            string value,
            string path,
            ICollection<CharacterArtManifestIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.MissingId,
                    path));
                return;
            }

            if (!IsStableId(value))
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.InvalidId,
                    path));
            }
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (!IsAlphaNumeric(value[0]) || !IsAlphaNumeric(value[value.Length - 1]))
                return false;

            var previousSeparator = false;
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                var separator = character == '.' || character == '-';
                if (!IsAlphaNumeric(character) && !separator || separator && previousSeparator)
                    return false;
                previousSeparator = separator;
            }
            return true;
        }

        private static bool IsAlphaNumeric(char character)
        {
            return character >= 'a' && character <= 'z' || character >= '0' && character <= '9';
        }

        private static void ValidateRequiredText(
            string value,
            string path,
            ICollection<CharacterArtManifestIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.MissingText,
                    path));
                return;
            }

            if (!IsWellFormedUnicode(value))
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.InvalidText,
                    path));
            }
        }

        private static void ValidateUrl(
            string value,
            string path,
            ICollection<CharacterArtManifestIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.MissingText,
                    path));
                return;
            }

            if (!IsWellFormedUnicode(value) || ContainsWhitespace(value) ||
                !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                string.IsNullOrEmpty(uri.Host) ||
                !string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.InvalidUrl,
                    path));
            }
        }

        private static bool ContainsWhitespace(string value)
        {
            for (var index = 0; index < value.Length; index++)
            {
                if (char.IsWhiteSpace(value[index]))
                    return true;
            }
            return false;
        }

        private static void ValidateArchiveSha256(
            string value,
            ICollection<CharacterArtManifestIssue> issues)
        {
            var valid = value != null && value.Length == 64;
            if (valid)
            {
                for (var index = 0; index < value.Length; index++)
                {
                    var character = value[index];
                    if (!(character >= '0' && character <= '9') &&
                        !(character >= 'a' && character <= 'f'))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (!valid)
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.InvalidArchiveSha256,
                    "archiveSha256"));
            }
        }

        private static void ValidateSelectedSourceFile(
            string value,
            ICollection<CharacterArtManifestIssue> issues)
        {
            if (!IsRepositoryRelativePath(value))
            {
                issues.Add(new CharacterArtManifestIssue(
                    CharacterArtManifestIssueCode.InvalidSelectedSourceFile,
                    "selectedSourceFile"));
            }
        }

        private static bool IsRepositoryRelativePath(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !IsWellFormedUnicode(value) ||
                value[0] == '/' || value[value.Length - 1] == '/' ||
                value.IndexOf('\\') >= 0 || value.IndexOf(':') >= 0)
            {
                return false;
            }

            var segments = value.Split('/');
            for (var index = 0; index < segments.Length; index++)
            {
                var segment = segments[index];
                if (segment.Length == 0 || segment == "." || segment == ".." ||
                    char.IsWhiteSpace(segment[0]) || char.IsWhiteSpace(segment[segment.Length - 1]))
                {
                    return false;
                }
                for (var characterIndex = 0; characterIndex < segment.Length; characterIndex++)
                {
                    if (char.IsControl(segment[characterIndex]))
                        return false;
                }
            }
            return true;
        }

        private static bool IsWellFormedUnicode(string value)
        {
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (char.IsHighSurrogate(character))
                {
                    if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                        return false;
                    index++;
                }
                else if (char.IsLowSurrogate(character))
                {
                    return false;
                }
            }
            return true;
        }

        private static void ValidatePositiveBudget(
            int value,
            CharacterArtManifestIssueCode code,
            string path,
            ICollection<CharacterArtManifestIssue> issues)
        {
            if (value <= 0)
                issues.Add(new CharacterArtManifestIssue(code, path));
        }

        private static int CompareMotionClips(CharacterArtMotionClip left, CharacterArtMotionClip right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return -1;
            if (right == null) return 1;
            var key = ((int)left.Key).CompareTo((int)right.Key);
            if (key != 0) return key;
            return string.CompareOrdinal(left.ClipId, right.ClipId);
        }

        private static void SortIssues(List<CharacterArtManifestIssue> issues)
        {
            issues.Sort((left, right) =>
            {
                var path = string.CompareOrdinal(left.Path, right.Path);
                return path != 0 ? path : left.Code.CompareTo(right.Code);
            });
        }
    }
}
