using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace RealmRaiders.Modules.CharacterMotionProfiles
{
    public enum CharacterMotionCompatibilityIssueCode
    {
        MissingProfile,
        UnreadableProfile,
        InvalidProfile,
        MissingTarget,
        UnknownTargetFamily,
        InvalidTargetRigProfileId,
        NullRequiredClipKeys,
        UnreadableRequiredClipKeys,
        UnknownRequiredClipKey,
        DuplicateRequiredClipKey,
        MissingRequiredClipKey,
        MissingProfileClip,
        DuplicateProfileClip,
        WrongProfileClipKey,
        FamilyMismatch,
        RigProfileMismatch,
        DisallowedFallback,
        UnreadableInput
    }

    public sealed class CharacterMotionCompatibilityIssue
    {
        public CharacterMotionCompatibilityIssue(
            CharacterMotionCompatibilityIssueCode code,
            string path,
            MotionProfileIssueCode? profileIssueCode = null)
        {
            Code = code;
            Path = path;
            ProfileIssueCode = profileIssueCode;
        }

        public CharacterMotionCompatibilityIssueCode Code { get; }
        public string Path { get; }
        public MotionProfileIssueCode? ProfileIssueCode { get; }
        public string Signature =>
            Path + "|" + Code + "|" +
            (ProfileIssueCode.HasValue ? ProfileIssueCode.Value.ToString() : "-");
    }

    public sealed class CharacterMotionCompatibilityResult
    {
        private readonly ReadOnlyCollection<CharacterMotionCompatibilityIssue> issues;

        internal CharacterMotionCompatibilityResult(
            IList<CharacterMotionCompatibilityIssue> issues)
        {
            this.issues = new ReadOnlyCollection<CharacterMotionCompatibilityIssue>(
                new List<CharacterMotionCompatibilityIssue>(issues));
        }

        public bool IsCompatible => issues.Count == 0;
        public IReadOnlyList<CharacterMotionCompatibilityIssue> Issues => issues;
    }

    /// <summary>
    /// Deterministically compares an explicit motion profile with explicit target requirements.
    /// It does not resolve clips, inspect assets, or control animation or gameplay.
    /// </summary>
    public static class CharacterMotionCompatibilityEvaluator
    {
        private static readonly MotionClipKey[] ExactRequiredClipKeys =
        {
            MotionClipKey.Idle,
            MotionClipKey.Locomotion,
            MotionClipKey.AttackPrimary,
            MotionClipKey.AttackAbility,
            MotionClipKey.Hit,
            MotionClipKey.Death
        };

        public static CharacterMotionCompatibilityResult Evaluate(
            CharacterMotionProfile profile,
            CharacterMotionTargetRequirements target)
        {
            try
            {
                return EvaluateCore(profile, target);
            }
            catch (Exception)
            {
                return Result(new List<CharacterMotionCompatibilityIssue>
                {
                    new CharacterMotionCompatibilityIssue(
                        CharacterMotionCompatibilityIssueCode.UnreadableInput,
                        "compatibility")
                });
            }
        }

        private static CharacterMotionCompatibilityResult EvaluateCore(
            CharacterMotionProfile profile,
            CharacterMotionTargetRequirements target)
        {
            var issues = new List<CharacterMotionCompatibilityIssue>();
            if (profile == null)
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.MissingProfile,
                    "profile"));
            }
            else
            {
                ValidateProfile(profile, issues);
            }

            if (target == null)
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.MissingTarget,
                    "target"));
                return Result(issues);
            }

            var targetFamilyValid = CharacterMotionProfileValidator.FamilyName(target.Family) != null;
            if (!targetFamilyValid)
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.UnknownTargetFamily,
                    "target.family"));
            }

            var targetRigValid = CharacterMotionProfileValidator.IsStableId(target.RigProfileId);
            if (!targetRigValid)
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.InvalidTargetRigProfileId,
                    "target.rigProfileId"));
            }

            ValidateTargetClipKeys(target, issues);
            if (profile == null)
                return Result(issues);

            if (targetFamilyValid &&
                CharacterMotionProfileValidator.FamilyName(profile.Family) != null &&
                profile.Family != target.Family)
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.FamilyMismatch,
                    "profile.family"));
            }
            if (targetRigValid &&
                CharacterMotionProfileValidator.IsStableId(profile.RigProfileId) &&
                !string.Equals(profile.RigProfileId, target.RigProfileId, StringComparison.Ordinal))
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.RigProfileMismatch,
                    "profile.rigProfileId"));
            }
            if (!target.FallbackAllowed && !string.IsNullOrWhiteSpace(profile.FallbackProfileId))
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.DisallowedFallback,
                    "profile.fallbackProfileId"));
            }

            return Result(issues);
        }

        private static void ValidateProfile(
            CharacterMotionProfile profile,
            ICollection<CharacterMotionCompatibilityIssue> issues)
        {
            IReadOnlyList<MotionProfileIssue> profileIssues;
            try
            {
                profileIssues = CharacterMotionProfileValidator.Validate(profile);
            }
            catch (Exception)
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.UnreadableProfile,
                    "profile"));
                return;
            }

            for (var index = 0; index < profileIssues.Count; index++)
            {
                var issue = profileIssues[index];
                issues.Add(new CharacterMotionCompatibilityIssue(
                    MapProfileIssue(issue.Code),
                    "profile." + issue.Path,
                    issue.Code));
            }
        }

        private static CharacterMotionCompatibilityIssueCode MapProfileIssue(
            MotionProfileIssueCode issueCode)
        {
            switch (issueCode)
            {
                case MotionProfileIssueCode.MissingClip:
                    return CharacterMotionCompatibilityIssueCode.MissingProfileClip;
                case MotionProfileIssueCode.DuplicateClip:
                    return CharacterMotionCompatibilityIssueCode.DuplicateProfileClip;
                case MotionProfileIssueCode.UnknownClip:
                case MotionProfileIssueCode.ClipKeyMismatch:
                    return CharacterMotionCompatibilityIssueCode.WrongProfileClipKey;
                default:
                    return CharacterMotionCompatibilityIssueCode.InvalidProfile;
            }
        }

        private static void ValidateTargetClipKeys(
            CharacterMotionTargetRequirements target,
            ICollection<CharacterMotionCompatibilityIssue> issues)
        {
            if (target.RequiredClipKeysState == MotionTargetClipCollectionState.Null)
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.NullRequiredClipKeys,
                    "target.requiredClipKeys"));
                return;
            }
            if (target.RequiredClipKeysState == MotionTargetClipCollectionState.Unreadable)
            {
                issues.Add(new CharacterMotionCompatibilityIssue(
                    CharacterMotionCompatibilityIssueCode.UnreadableRequiredClipKeys,
                    "target.requiredClipKeys"));
                return;
            }

            var counts = new Dictionary<MotionClipKey, int>();
            for (var index = 0; index < target.RequiredClipKeys.Count; index++)
            {
                var key = target.RequiredClipKeys[index];
                var keyName = CharacterMotionProfileValidator.ClipKeyName(key);
                if (keyName == null)
                {
                    issues.Add(new CharacterMotionCompatibilityIssue(
                        CharacterMotionCompatibilityIssueCode.UnknownRequiredClipKey,
                        "target.requiredClipKeys[" +
                        Convert.ToInt32(key).ToString(CultureInfo.InvariantCulture) + "]"));
                    continue;
                }
                counts[key] = counts.TryGetValue(key, out var count)
                    ? count + 1
                    : 1;
            }

            for (var index = 0; index < ExactRequiredClipKeys.Length; index++)
            {
                var required = ExactRequiredClipKeys[index];
                var path = "target.requiredClipKeys." +
                    CharacterMotionProfileValidator.ClipKeyName(required);
                if (!counts.TryGetValue(required, out var count))
                {
                    issues.Add(new CharacterMotionCompatibilityIssue(
                        CharacterMotionCompatibilityIssueCode.MissingRequiredClipKey,
                        path));
                }
                else if (count > 1)
                {
                    issues.Add(new CharacterMotionCompatibilityIssue(
                        CharacterMotionCompatibilityIssueCode.DuplicateRequiredClipKey,
                        path));
                }
            }
        }

        private static CharacterMotionCompatibilityResult Result(
            List<CharacterMotionCompatibilityIssue> issues)
        {
            issues.Sort(CompareIssues);
            return new CharacterMotionCompatibilityResult(issues);
        }

        private static int CompareIssues(
            CharacterMotionCompatibilityIssue left,
            CharacterMotionCompatibilityIssue right)
        {
            var path = StringComparer.Ordinal.Compare(left.Path, right.Path);
            if (path != 0)
                return path;
            var code = left.Code.CompareTo(right.Code);
            return code != 0
                ? code
                : Nullable.Compare(left.ProfileIssueCode, right.ProfileIssueCode);
        }
    }
}
