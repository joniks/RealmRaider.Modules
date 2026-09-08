using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterMotionProfiles
{
    internal enum MotionTargetClipCollectionState
    {
        Readable,
        Null,
        Unreadable
    }

    /// <summary>
    /// Immutable adapter-neutral requirements for one motion-profile compatibility check.
    /// </summary>
    public sealed class CharacterMotionTargetRequirements
    {
        private readonly ReadOnlyCollection<MotionClipKey> requiredClipKeys;

        public CharacterMotionTargetRequirements(
            CharacterBodyFamily family,
            string rigProfileId,
            IEnumerable<MotionClipKey> requiredClipKeys,
            bool fallbackAllowed)
        {
            Family = family;
            RigProfileId = rigProfileId;
            this.requiredClipKeys = Snapshot(
                requiredClipKeys,
                out var requiredClipKeysState);
            RequiredClipKeysState = requiredClipKeysState;
            FallbackAllowed = fallbackAllowed;
        }

        public CharacterBodyFamily Family { get; }
        public string RigProfileId { get; }
        public IReadOnlyList<MotionClipKey> RequiredClipKeys => requiredClipKeys;
        public bool FallbackAllowed { get; }

        internal MotionTargetClipCollectionState RequiredClipKeysState { get; }

        private static ReadOnlyCollection<MotionClipKey> Snapshot(
            IEnumerable<MotionClipKey> source,
            out MotionTargetClipCollectionState state)
        {
            if (source == null)
            {
                state = MotionTargetClipCollectionState.Null;
                return new ReadOnlyCollection<MotionClipKey>(new List<MotionClipKey>());
            }

            try
            {
                var snapshot = new List<MotionClipKey>();
                foreach (var key in source)
                    snapshot.Add(key);
                state = MotionTargetClipCollectionState.Readable;
                return new ReadOnlyCollection<MotionClipKey>(snapshot);
            }
            catch (Exception)
            {
                state = MotionTargetClipCollectionState.Unreadable;
                return new ReadOnlyCollection<MotionClipKey>(new List<MotionClipKey>());
            }
        }
    }
}
