using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.CharacterMotionProfiles.Tests
{
    public sealed class LargeCreatureMotionReadinessTests
    {
        [Test]
        public void NullInvalidFactsAndTierFailClosed()
        {
            var missing = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                null);
            var invalidFacts = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(skinnedRenderer: (LargeCreatureMotionFact)99));
            var invalidTier = LargeCreatureMotionReadiness.Evaluate(
                (LargeCreatureMotionReadinessTier)99,
                Facts());

            Assert.That(missing.IsProductionReady, Is.False);
            Assert.That(missing.Issues, Is.EqualTo(new[]
            {
                LargeCreatureMotionReadinessIssue.MissingFacts
            }));
            Assert.That(invalidFacts.IsProductionReady, Is.False);
            Assert.That(invalidFacts.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.InvalidFacts));
            Assert.That(invalidTier.IsProductionReady, Is.False);
            Assert.That(invalidTier.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.InvalidTier));
        }

        [Test]
        public void Tree01CurrentStaticFactsReportTheKnownMotionDeficit()
        {
            var result = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.Temporary,
                Facts(
                    skinnedRenderer: LargeCreatureMotionFact.Contradicted,
                    presentationBoneOwnership: LargeCreatureMotionFact.Unverified,
                    idleClip: LargeCreatureMotionFact.Contradicted,
                    runClip: LargeCreatureMotionFact.Contradicted,
                    attackClip: LargeCreatureMotionFact.Contradicted,
                    deathClip: LargeCreatureMotionFact.Contradicted,
                    sourceHitClip: LargeCreatureMotionFact.Contradicted,
                    inPlaceMotionReviewed: LargeCreatureMotionFact.Unverified,
                    idleLoopReviewed: LargeCreatureMotionFact.Unverified,
                    runLoopReviewed: LargeCreatureMotionFact.Unverified,
                    deathHeldPoseReviewed: LargeCreatureMotionFact.Unverified));

            Assert.That(result.HasReadinessEvidence, Is.False);
            Assert.That(result.IsProductionReady, Is.False);
            Assert.That(result.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.SkinnedRendererNotVerified));
            Assert.That(result.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.PresentationBoneOwnershipNotVerified));
            Assert.That(result.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.MissingIdleClip));
            Assert.That(result.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.MissingRunClip));
            Assert.That(result.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.MissingAttackClip));
            Assert.That(result.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.MissingDeathClip));
            Assert.That(result.Issues, Does.Not.Contain(
                LargeCreatureMotionReadinessIssue.BoundedHitFallbackNotAvailable));
        }

        [Test]
        public void CompleteCandidateIsProductionReady()
        {
            var result = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts());

            Assert.That(result.HasReadinessEvidence, Is.True);
            Assert.That(result.IsProductionReady, Is.True);
            Assert.That(result.Issues, Is.Empty);
        }

        [Test]
        public void ProbeAndTemporaryCanReportEvidenceWithoutProductionReadiness()
        {
            var probe = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.Probe,
                Facts());
            var temporary = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.Temporary,
                Facts());

            Assert.That(probe.HasReadinessEvidence, Is.True);
            Assert.That(probe.IsProductionReady, Is.False);
            Assert.That(temporary.HasReadinessEvidence, Is.True);
            Assert.That(temporary.IsProductionReady, Is.False);
        }

        [Test]
        public void MissingRequiredClipFailsWithStableIssue()
        {
            var result = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(runClip: LargeCreatureMotionFact.Contradicted));

            Assert.That(result.IsProductionReady, Is.False);
            Assert.That(result.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.MissingRunClip));
        }

        [Test]
        public void RootMotionSafetyMustBeConfirmed()
        {
            var result = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(rootMotionDisabled: LargeCreatureMotionFact.Contradicted));

            Assert.That(result.IsProductionReady, Is.False);
            Assert.That(result.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.RootMotionNotDisabled));
        }

        [Test]
        public void BoundedExistingHitFallbackIsRequiredEvenWhenHitClipExists()
        {
            var missingHit = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(
                    sourceHitClip: LargeCreatureMotionFact.Contradicted,
                    boundedHitFallbackAvailable: LargeCreatureMotionFact.Contradicted));
            var presentHit = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(
                    sourceHitClip: LargeCreatureMotionFact.Confirmed,
                    boundedHitFallbackAvailable: LargeCreatureMotionFact.Contradicted));

            Assert.That(missingHit.IsProductionReady, Is.False);
            Assert.That(missingHit.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.BoundedHitFallbackNotAvailable));
            Assert.That(presentHit.IsProductionReady, Is.False);
            Assert.That(presentHit.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.BoundedHitFallbackNotAvailable));
        }

        [Test]
        public void LoopDeathAndStaticFallbackFactsMustBeConfirmed()
        {
            var idleLoop = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(idleLoopReviewed: LargeCreatureMotionFact.Contradicted));
            var runLoop = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(runLoopReviewed: LargeCreatureMotionFact.Contradicted));
            var deathHold = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(deathHeldPoseReviewed: LargeCreatureMotionFact.Contradicted));
            var fallback = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(staticFallbackAvailable: LargeCreatureMotionFact.Contradicted));

            Assert.That(idleLoop.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.IdleLoopNotReviewed));
            Assert.That(runLoop.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.RunLoopNotReviewed));
            Assert.That(deathHold.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.DeathHeldPoseNotReviewed));
            Assert.That(fallback.Issues, Does.Contain(
                LargeCreatureMotionReadinessIssue.StaticFallbackNotAvailable));
        }

        [Test]
        public void FactsAndResultAreImmutableAndIssuesKeepTheirFixedOrder()
        {
            var facts = Facts();
            var result = LargeCreatureMotionReadiness.Evaluate(
                LargeCreatureMotionReadinessTier.ProductionReady,
                Facts(
                    skinnedRenderer: LargeCreatureMotionFact.Contradicted,
                    idleClip: LargeCreatureMotionFact.Contradicted,
                    rootMotionDisabled: LargeCreatureMotionFact.Contradicted));

            Assert.That(typeof(LargeCreatureMotionFacts)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(LargeCreatureMotionReadinessResult)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<LargeCreatureMotionReadinessIssue>)result.Issues).Clear());
            Assert.That(result.Issues, Is.EqualTo(new[]
            {
                LargeCreatureMotionReadinessIssue.SkinnedRendererNotVerified,
                LargeCreatureMotionReadinessIssue.MissingIdleClip,
                LargeCreatureMotionReadinessIssue.RootMotionNotDisabled
            }));
            Assert.That(facts.StaticFallbackAvailable, Is.EqualTo(
                LargeCreatureMotionFact.Confirmed));
        }

        private static LargeCreatureMotionFacts Facts(
            LargeCreatureMotionFact skinnedRenderer = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact presentationBoneOwnership = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact idleClip = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact runClip = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact attackClip = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact deathClip = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact sourceHitClip = LargeCreatureMotionFact.Contradicted,
            LargeCreatureMotionFact rootMotionDisabled = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact inPlaceMotionReviewed = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact idleLoopReviewed = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact runLoopReviewed = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact deathHeldPoseReviewed = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact staticFallbackAvailable = LargeCreatureMotionFact.Confirmed,
            LargeCreatureMotionFact boundedHitFallbackAvailable = LargeCreatureMotionFact.Confirmed)
        {
            return new LargeCreatureMotionFacts(
                skinnedRenderer,
                presentationBoneOwnership,
                idleClip,
                runClip,
                attackClip,
                deathClip,
                sourceHitClip,
                rootMotionDisabled,
                inPlaceMotionReviewed,
                idleLoopReviewed,
                runLoopReviewed,
                deathHeldPoseReviewed,
                staticFallbackAvailable,
                boundedHitFallbackAvailable);
        }
    }
}
