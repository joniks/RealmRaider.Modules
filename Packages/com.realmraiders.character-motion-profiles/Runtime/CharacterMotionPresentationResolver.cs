namespace RealmRaiders.Modules.CharacterMotionProfiles
{
    public enum MotionPresentationReaction
    {
        None = 0,
        Hit = 1,
        Death = 2
    }

    public enum MotionPresentationAttack
    {
        None = 0,
        Primary = 1,
        Ability = 2
    }

    public enum MotionPresentationJumpPhase
    {
        None = 0,
        Takeoff = 1,
        Falling = 2,
        Landing = 3
    }

    /// <summary>
    /// Zero-allocation factual presentation input supplied by an owning adapter.
    /// It has no timing, movement, root-motion, asset, or gameplay authority.
    /// </summary>
    public readonly struct CharacterMotionPresentationInput
    {
        public CharacterMotionPresentationInput(
            MotionPresentationReaction reaction,
            MotionPresentationAttack attack,
            MotionPresentationJumpPhase jumpPhase,
            bool isLocomoting)
        {
            Reaction = reaction;
            Attack = attack;
            JumpPhase = jumpPhase;
            IsLocomoting = isLocomoting;
        }

        public MotionPresentationReaction Reaction { get; }
        public MotionPresentationAttack Attack { get; }
        public MotionPresentationJumpPhase JumpPhase { get; }
        public bool IsLocomoting { get; }
    }

    /// <summary>
    /// Selects a semantic visual key from caller-supplied facts only.
    /// It neither reads nor changes clips, timing, movement, root motion, or gameplay state.
    /// </summary>
    public static class CharacterMotionPresentationResolver
    {
        public static MotionClipKey Resolve(CharacterMotionPresentationInput input)
        {
            switch (input.Reaction)
            {
                case MotionPresentationReaction.Death:
                    return MotionClipKey.Death;
                case MotionPresentationReaction.Hit:
                    return MotionClipKey.Hit;
            }

            switch (input.Attack)
            {
                case MotionPresentationAttack.Primary:
                    return MotionClipKey.AttackPrimary;
                case MotionPresentationAttack.Ability:
                    return MotionClipKey.AttackAbility;
            }

            switch (input.JumpPhase)
            {
                case MotionPresentationJumpPhase.Takeoff:
                    return MotionClipKey.JumpTakeoff;
                case MotionPresentationJumpPhase.Falling:
                    return MotionClipKey.JumpFall;
                case MotionPresentationJumpPhase.Landing:
                    return MotionClipKey.JumpLand;
            }

            return input.IsLocomoting ? MotionClipKey.Locomotion : MotionClipKey.Idle;
        }
    }
}
