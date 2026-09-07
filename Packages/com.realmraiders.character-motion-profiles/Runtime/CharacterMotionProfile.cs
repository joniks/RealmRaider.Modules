using System.Collections.Generic;
using System.Collections.ObjectModel;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterMotionProfiles
{
    public enum MotionClipKey
    {
        Idle,
        Locomotion,
        AttackPrimary,
        AttackAbility,
        Hit,
        Death
    }

    public enum MotionRhythm
    {
        Neutral,
        Sylvan,
        Infernal
    }

    public sealed class MotionClipBinding
    {
        public MotionClipBinding(
            MotionClipKey assignedKey,
            string clipId,
            CharacterBodyFamily declaredFamily,
            string declaredRigProfileId,
            MotionClipKey declaredKey)
        {
            AssignedKey = assignedKey;
            ClipId = clipId;
            DeclaredFamily = declaredFamily;
            DeclaredRigProfileId = declaredRigProfileId;
            DeclaredKey = declaredKey;
        }

        public MotionClipKey AssignedKey { get; }
        public string ClipId { get; }
        public CharacterBodyFamily DeclaredFamily { get; }
        public string DeclaredRigProfileId { get; }
        public MotionClipKey DeclaredKey { get; }
    }

    public sealed class CharacterMotionProfile
    {
        private readonly ReadOnlyCollection<MotionClipBinding> clips;
        private readonly ReadOnlyCollection<string> sourceIds;

        public CharacterMotionProfile(
            int schemaVersion,
            string motionProfileId,
            CharacterBodyFamily family,
            string rigProfileId,
            string animatorProfileId,
            IEnumerable<MotionClipBinding> clips,
            MotionRhythm rhythmProfile,
            string fallbackProfileId,
            IEnumerable<string> sourceIds)
        {
            SchemaVersion = schemaVersion;
            MotionProfileId = motionProfileId;
            Family = family;
            RigProfileId = rigProfileId;
            AnimatorProfileId = animatorProfileId;
            this.clips = new ReadOnlyCollection<MotionClipBinding>(
                clips == null ? new List<MotionClipBinding>() : new List<MotionClipBinding>(clips));
            RhythmProfile = rhythmProfile;
            FallbackProfileId = fallbackProfileId;
            this.sourceIds = new ReadOnlyCollection<string>(
                sourceIds == null ? new List<string>() : new List<string>(sourceIds));
        }

        public int SchemaVersion { get; }
        public string MotionProfileId { get; }
        public CharacterBodyFamily Family { get; }
        public string RigProfileId { get; }
        public string AnimatorProfileId { get; }
        public IReadOnlyList<MotionClipBinding> Clips => clips;
        public MotionRhythm RhythmProfile { get; }
        public string FallbackProfileId { get; }
        public IReadOnlyList<string> SourceIds => sourceIds;
    }
}
