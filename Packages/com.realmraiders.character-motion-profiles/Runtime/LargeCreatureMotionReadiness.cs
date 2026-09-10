using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.CharacterMotionProfiles
{
    public enum LargeCreatureMotionReadinessTier
    {
        Probe,
        Temporary,
        ProductionReady
    }

    public enum LargeCreatureMotionFact
    {
        Unverified,
        Confirmed,
        Contradicted
    }

    public enum LargeCreatureMotionReadinessIssue
    {
        InvalidTier,
        MissingFacts,
        InvalidFacts,
        SkinnedRendererNotVerified,
        PresentationBoneOwnershipNotVerified,
        MissingIdleClip,
        MissingRunClip,
        MissingAttackClip,
        MissingDeathClip,
        RootMotionNotDisabled,
        InPlaceMotionNotReviewed,
        IdleLoopNotReviewed,
        RunLoopNotReviewed,
        DeathHeldPoseNotReviewed,
        StaticFallbackNotAvailable,
        HitCoverageUnverified,
        BoundedHitFallbackNotAvailable
    }

    /// <summary>
    /// Immutable caller-supplied evidence for presentation-only LargeCreature motion.
    /// Confirmed facts identify a safe visual path; this contract has no asset, timing,
    /// root-transform, physics, or gameplay authority.
    /// </summary>
    public sealed class LargeCreatureMotionFacts
    {
        public LargeCreatureMotionFacts(
            LargeCreatureMotionFact skinnedRenderer,
            LargeCreatureMotionFact presentationBoneOwnership,
            LargeCreatureMotionFact idleClip,
            LargeCreatureMotionFact runClip,
            LargeCreatureMotionFact attackClip,
            LargeCreatureMotionFact deathClip,
            LargeCreatureMotionFact sourceHitClip,
            LargeCreatureMotionFact rootMotionDisabled,
            LargeCreatureMotionFact inPlaceMotionReviewed,
            LargeCreatureMotionFact idleLoopReviewed,
            LargeCreatureMotionFact runLoopReviewed,
            LargeCreatureMotionFact deathHeldPoseReviewed,
            LargeCreatureMotionFact staticFallbackAvailable,
            LargeCreatureMotionFact boundedHitFallbackAvailable)
        {
            SkinnedRenderer = skinnedRenderer;
            PresentationBoneOwnership = presentationBoneOwnership;
            IdleClip = idleClip;
            RunClip = runClip;
            AttackClip = attackClip;
            DeathClip = deathClip;
            SourceHitClip = sourceHitClip;
            RootMotionDisabled = rootMotionDisabled;
            InPlaceMotionReviewed = inPlaceMotionReviewed;
            IdleLoopReviewed = idleLoopReviewed;
            RunLoopReviewed = runLoopReviewed;
            DeathHeldPoseReviewed = deathHeldPoseReviewed;
            StaticFallbackAvailable = staticFallbackAvailable;
            BoundedHitFallbackAvailable = boundedHitFallbackAvailable;
        }

        public LargeCreatureMotionFact SkinnedRenderer { get; }

        public LargeCreatureMotionFact PresentationBoneOwnership { get; }

        public LargeCreatureMotionFact IdleClip { get; }

        public LargeCreatureMotionFact RunClip { get; }

        public LargeCreatureMotionFact AttackClip { get; }

        public LargeCreatureMotionFact DeathClip { get; }

        public LargeCreatureMotionFact SourceHitClip { get; }

        public LargeCreatureMotionFact RootMotionDisabled { get; }

        public LargeCreatureMotionFact InPlaceMotionReviewed { get; }

        public LargeCreatureMotionFact IdleLoopReviewed { get; }

        public LargeCreatureMotionFact RunLoopReviewed { get; }

        public LargeCreatureMotionFact DeathHeldPoseReviewed { get; }

        public LargeCreatureMotionFact StaticFallbackAvailable { get; }

        public LargeCreatureMotionFact BoundedHitFallbackAvailable { get; }
    }

    public sealed class LargeCreatureMotionReadinessResult
    {
        private readonly ReadOnlyCollection<LargeCreatureMotionReadinessIssue> issues;

        internal LargeCreatureMotionReadinessResult(
            LargeCreatureMotionReadinessTier tier,
            bool tierIsValid,
            List<LargeCreatureMotionReadinessIssue> issues)
        {
            Tier = tier;
            this.issues = new ReadOnlyCollection<LargeCreatureMotionReadinessIssue>(
                new List<LargeCreatureMotionReadinessIssue>(issues));
            HasReadinessEvidence = tierIsValid && this.issues.Count == 0;
            IsProductionReady = tier == LargeCreatureMotionReadinessTier.ProductionReady
                && HasReadinessEvidence;
        }

        public LargeCreatureMotionReadinessTier Tier { get; }

        public bool HasReadinessEvidence { get; }

        public bool IsProductionReady { get; }

        public IReadOnlyList<LargeCreatureMotionReadinessIssue> Issues => issues;
    }

    /// <summary>
    /// Validates factual LargeCreature motion evidence in a fixed issue order.
    /// Probe and Temporary results can expose complete evidence, but only the explicit
    /// ProductionReady tier can report production readiness.
    /// </summary>
    public static class LargeCreatureMotionReadiness
    {
        public static LargeCreatureMotionReadinessResult Evaluate(
            LargeCreatureMotionReadinessTier tier,
            LargeCreatureMotionFacts facts)
        {
            var issues = new List<LargeCreatureMotionReadinessIssue>();
            var tierIsValid = IsValidTier(tier);

            if (!tierIsValid)
            {
                issues.Add(LargeCreatureMotionReadinessIssue.InvalidTier);
            }

            if (facts == null)
            {
                issues.Add(LargeCreatureMotionReadinessIssue.MissingFacts);
            }
            else
            {
                AddFactIssues(facts, issues);
            }

            return new LargeCreatureMotionReadinessResult(
                tier,
                tierIsValid,
                issues);
        }

        private static void AddFactIssues(
            LargeCreatureMotionFacts facts,
            List<LargeCreatureMotionReadinessIssue> issues)
        {
            if (!HasValidFacts(facts))
            {
                issues.Add(LargeCreatureMotionReadinessIssue.InvalidFacts);
            }

            AddUnverifiedIssue(
                facts.SkinnedRenderer,
                LargeCreatureMotionReadinessIssue.SkinnedRendererNotVerified,
                issues);
            AddUnverifiedIssue(
                facts.PresentationBoneOwnership,
                LargeCreatureMotionReadinessIssue.PresentationBoneOwnershipNotVerified,
                issues);
            AddUnverifiedIssue(
                facts.IdleClip,
                LargeCreatureMotionReadinessIssue.MissingIdleClip,
                issues);
            AddUnverifiedIssue(
                facts.RunClip,
                LargeCreatureMotionReadinessIssue.MissingRunClip,
                issues);
            AddUnverifiedIssue(
                facts.AttackClip,
                LargeCreatureMotionReadinessIssue.MissingAttackClip,
                issues);
            AddUnverifiedIssue(
                facts.DeathClip,
                LargeCreatureMotionReadinessIssue.MissingDeathClip,
                issues);
            AddUnverifiedIssue(
                facts.RootMotionDisabled,
                LargeCreatureMotionReadinessIssue.RootMotionNotDisabled,
                issues);
            AddUnverifiedIssue(
                facts.InPlaceMotionReviewed,
                LargeCreatureMotionReadinessIssue.InPlaceMotionNotReviewed,
                issues);
            AddUnverifiedIssue(
                facts.IdleLoopReviewed,
                LargeCreatureMotionReadinessIssue.IdleLoopNotReviewed,
                issues);
            AddUnverifiedIssue(
                facts.RunLoopReviewed,
                LargeCreatureMotionReadinessIssue.RunLoopNotReviewed,
                issues);
            AddUnverifiedIssue(
                facts.DeathHeldPoseReviewed,
                LargeCreatureMotionReadinessIssue.DeathHeldPoseNotReviewed,
                issues);
            AddUnverifiedIssue(
                facts.StaticFallbackAvailable,
                LargeCreatureMotionReadinessIssue.StaticFallbackNotAvailable,
                issues);
            AddHitCoverageIssue(facts, issues);
        }

        private static void AddHitCoverageIssue(
            LargeCreatureMotionFacts facts,
            List<LargeCreatureMotionReadinessIssue> issues)
        {
            if (facts.SourceHitClip == LargeCreatureMotionFact.Unverified)
            {
                issues.Add(LargeCreatureMotionReadinessIssue.HitCoverageUnverified);
            }

            AddUnverifiedIssue(
                facts.BoundedHitFallbackAvailable,
                LargeCreatureMotionReadinessIssue.BoundedHitFallbackNotAvailable,
                issues);
        }

        private static void AddUnverifiedIssue(
            LargeCreatureMotionFact fact,
            LargeCreatureMotionReadinessIssue issue,
            List<LargeCreatureMotionReadinessIssue> issues)
        {
            if (fact != LargeCreatureMotionFact.Confirmed)
            {
                issues.Add(issue);
            }
        }

        private static bool HasValidFacts(LargeCreatureMotionFacts facts)
        {
            return IsValidFact(facts.SkinnedRenderer)
                && IsValidFact(facts.PresentationBoneOwnership)
                && IsValidFact(facts.IdleClip)
                && IsValidFact(facts.RunClip)
                && IsValidFact(facts.AttackClip)
                && IsValidFact(facts.DeathClip)
                && IsValidFact(facts.SourceHitClip)
                && IsValidFact(facts.RootMotionDisabled)
                && IsValidFact(facts.InPlaceMotionReviewed)
                && IsValidFact(facts.IdleLoopReviewed)
                && IsValidFact(facts.RunLoopReviewed)
                && IsValidFact(facts.DeathHeldPoseReviewed)
                && IsValidFact(facts.StaticFallbackAvailable)
                && IsValidFact(facts.BoundedHitFallbackAvailable);
        }

        private static bool IsValidTier(LargeCreatureMotionReadinessTier tier)
        {
            return tier == LargeCreatureMotionReadinessTier.Probe
                || tier == LargeCreatureMotionReadinessTier.Temporary
                || tier == LargeCreatureMotionReadinessTier.ProductionReady;
        }

        private static bool IsValidFact(LargeCreatureMotionFact fact)
        {
            return fact == LargeCreatureMotionFact.Unverified
                || fact == LargeCreatureMotionFact.Confirmed
                || fact == LargeCreatureMotionFact.Contradicted;
        }
    }
}
