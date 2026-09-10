using System;
using System.Collections.Generic;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterMotionProfiles
{
    public enum MotionProfileIssueCode
    {
        MissingProfile,
        UnsupportedSchemaVersion,
        MissingId,
        InvalidId,
        UnknownFamily,
        UnknownRhythm,
        MissingClip,
        DuplicateClip,
        UnknownClip,
        FamilyMismatch,
        RigMismatch,
        ClipKeyMismatch,
        MissingSourceId,
        UnorderedSourceIds,
        DuplicateSourceId,
        MissingField,
        DuplicateField,
        UnknownField
    }

    public sealed class MotionProfileIssue
    {
        public MotionProfileIssue(MotionProfileIssueCode code, string path)
        {
            Code = code;
            Path = path;
        }

        public MotionProfileIssueCode Code { get; }
        public string Path { get; }
    }

    public static class CharacterMotionProfileValidator
    {
        public const int SupportedSchemaVersion = 1;

        private static readonly MotionClipKey[] RequiredClipKeys =
        {
            MotionClipKey.Idle,
            MotionClipKey.Locomotion,
            MotionClipKey.AttackPrimary,
            MotionClipKey.AttackAbility,
            MotionClipKey.Hit,
            MotionClipKey.Death,
            MotionClipKey.JumpTakeoff,
            MotionClipKey.JumpFall,
            MotionClipKey.JumpLand
        };

        private static readonly string[] RequiredRootFields =
        {
            "schemaVersion",
            "motionProfileId",
            "family",
            "rigProfileId",
            "animatorProfileId",
            "clips",
            "rhythmProfile",
            "fallbackProfileId",
            "sourceIds"
        };

        public static IReadOnlyList<MotionProfileIssue> Validate(CharacterMotionProfile profile)
        {
            var issues = new List<MotionProfileIssue>();
            if (profile == null)
            {
                issues.Add(new MotionProfileIssue(MotionProfileIssueCode.MissingProfile, "profile"));
                return issues.AsReadOnly();
            }

            if (profile.SchemaVersion != SupportedSchemaVersion)
                issues.Add(new MotionProfileIssue(MotionProfileIssueCode.UnsupportedSchemaVersion, "schemaVersion"));
            ValidateId(profile.MotionProfileId, "motionProfileId", issues);
            ValidateFamily(profile.Family, "family", issues);
            ValidateId(profile.RigProfileId, "rigProfileId", issues);
            ValidateId(profile.AnimatorProfileId, "animatorProfileId", issues);
            ValidateClips(profile, issues);
            if (RhythmName(profile.RhythmProfile) == null)
                issues.Add(new MotionProfileIssue(MotionProfileIssueCode.UnknownRhythm, "rhythmProfile"));
            ValidateId(profile.FallbackProfileId, "fallbackProfileId", issues);
            ValidateSources(profile.SourceIds, issues);
            return issues.AsReadOnly();
        }

        public static IReadOnlyList<MotionProfileIssue> ValidateRootFields(IEnumerable<string> fieldNames)
        {
            var issues = new List<MotionProfileIssue>();
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
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.MissingField, required));
                else if (count > 1)
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.DuplicateField, required));
                counts.Remove(required);
            }

            var unknown = new List<string>(counts.Keys);
            unknown.Sort(StringComparer.Ordinal);
            for (var index = 0; index < unknown.Count; index++)
                issues.Add(new MotionProfileIssue(MotionProfileIssueCode.UnknownField, unknown[index]));
            return issues.AsReadOnly();
        }

        internal static List<MotionClipBinding> CanonicalClips(IReadOnlyList<MotionClipBinding> clips)
        {
            var canonical = new List<MotionClipBinding>();
            for (var index = 0; index < clips.Count; index++)
                canonical.Add(clips[index]);
            canonical.Sort(CompareClips);
            return canonical;
        }

        internal static string ClipKeyName(MotionClipKey key)
        {
            switch (key)
            {
                case MotionClipKey.Idle: return "idle";
                case MotionClipKey.Locomotion: return "locomotion";
                case MotionClipKey.JumpTakeoff: return "jump_takeoff";
                case MotionClipKey.JumpFall: return "jump_fall";
                case MotionClipKey.JumpLand: return "jump_land";
                case MotionClipKey.AttackPrimary: return "attack_primary";
                case MotionClipKey.AttackAbility: return "attack_ability";
                case MotionClipKey.Hit: return "hit";
                case MotionClipKey.Death: return "death";
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

        internal static string RhythmName(MotionRhythm rhythm)
        {
            switch (rhythm)
            {
                case MotionRhythm.Neutral: return "neutral";
                case MotionRhythm.Sylvan: return "sylvan";
                case MotionRhythm.Infernal: return "infernal";
                default: return null;
            }
        }

        private static void ValidateClips(CharacterMotionProfile profile, ICollection<MotionProfileIssue> issues)
        {
            var clips = CanonicalClips(profile.Clips);
            var seen = new HashSet<MotionClipKey>();
            for (var index = 0; index < clips.Count; index++)
            {
                var clip = clips[index];
                var path = "clips[" + index + "]";
                if (clip == null)
                {
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.MissingClip, path));
                    continue;
                }

                var assignedKey = ClipKeyName(clip.AssignedKey);
                var declaredKey = ClipKeyName(clip.DeclaredKey);
                if (assignedKey == null)
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.UnknownClip, path + ".key"));
                else if (!seen.Add(clip.AssignedKey))
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.DuplicateClip, path + ".key"));
                if (declaredKey == null)
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.UnknownClip, path + ".declaredKey"));

                ValidateId(clip.ClipId, path + ".clipId", issues);
                ValidateFamily(clip.DeclaredFamily, path + ".declaredFamily", issues);
                ValidateId(clip.DeclaredRigProfileId, path + ".declaredRigProfileId", issues);
                if (FamilyName(clip.DeclaredFamily) != null && clip.DeclaredFamily != profile.Family)
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.FamilyMismatch, path + ".declaredFamily"));
                if (!string.Equals(clip.DeclaredRigProfileId, profile.RigProfileId, StringComparison.Ordinal))
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.RigMismatch, path + ".declaredRigProfileId"));
                if (assignedKey != null && declaredKey != null && clip.AssignedKey != clip.DeclaredKey)
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.ClipKeyMismatch, path + ".declaredKey"));
            }

            for (var index = 0; index < RequiredClipKeys.Length; index++)
            {
                var required = RequiredClipKeys[index];
                if (!seen.Contains(required))
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.MissingClip, "clips." + ClipKeyName(required)));
            }
        }

        private static void ValidateSources(IReadOnlyList<string> sourceIds, ICollection<MotionProfileIssue> issues)
        {
            if (sourceIds.Count == 0)
            {
                issues.Add(new MotionProfileIssue(MotionProfileIssueCode.MissingSourceId, "sourceIds"));
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
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.MissingSourceId, path));
                    continue;
                }

                ValidateId(source, path, issues);
                if (!seen.Add(source))
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.DuplicateSourceId, path));
                if (previous != null && string.CompareOrdinal(previous, source) > 0)
                    issues.Add(new MotionProfileIssue(MotionProfileIssueCode.UnorderedSourceIds, path));
                previous = source;
            }
        }

        private static void ValidateFamily(CharacterBodyFamily family, string path, ICollection<MotionProfileIssue> issues)
        {
            if (FamilyName(family) == null)
                issues.Add(new MotionProfileIssue(MotionProfileIssueCode.UnknownFamily, path));
        }

        private static void ValidateId(string value, string path, ICollection<MotionProfileIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                issues.Add(new MotionProfileIssue(MotionProfileIssueCode.MissingId, path));
                return;
            }

            if (!IsStableId(value))
            {
                issues.Add(new MotionProfileIssue(MotionProfileIssueCode.InvalidId, path));
                return;
            }
        }

        internal static bool IsStableId(string value)
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

        private static int CompareClips(MotionClipBinding left, MotionClipBinding right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return -1;
            if (right == null) return 1;
            var key = ((int)left.AssignedKey).CompareTo((int)right.AssignedKey);
            if (key != 0) return key;
            return string.CompareOrdinal(left.ClipId, right.ClipId);
        }
    }
}
