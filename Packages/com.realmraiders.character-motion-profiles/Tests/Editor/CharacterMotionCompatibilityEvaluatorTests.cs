using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterMotionProfiles.Tests
{
    public sealed class CharacterMotionCompatibilityEvaluatorTests
    {
        [Test]
        public void MatchingProfileAndReorderedNineKeyTarget_AreCompatible()
        {
            var profile = ValidProfile(clips: RequiredClips().Reverse());
            var target = ValidTarget(requiredClipKeys: RequiredKeys().Reverse());

            var result = CharacterMotionCompatibilityEvaluator.Evaluate(profile, target);

            Assert.That(result.IsCompatible, Is.True);
            Assert.That(result.Issues, Is.Empty);
        }

        [Test]
        public void TargetFamilyMismatch_FailsAtStableProfilePath()
        {
            var result = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                ValidTarget(family: CharacterBodyFamily.LargeCreature));

            Assert.That(result.IsCompatible, Is.False);
            Assert.That(result.Issues.Single().Code,
                Is.EqualTo(CharacterMotionCompatibilityIssueCode.FamilyMismatch));
            Assert.That(result.Issues.Single().Path, Is.EqualTo("profile.family"));
            Assert.That(result.Issues.Single().Signature,
                Is.EqualTo("profile.family|FamilyMismatch|-"));
        }

        [Test]
        public void RigMatching_IsExactOrdinalAndTargetRigMustBeStable()
        {
            var mismatch = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                ValidTarget(rigProfileId: "test.rig.other.v1"));
            var malformedCase = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                ValidTarget(rigProfileId: "test.rig.Beast.v1"));

            Assert.That(Codes(mismatch), Does.Contain(
                CharacterMotionCompatibilityIssueCode.RigProfileMismatch));
            Assert.That(mismatch.Issues.Single(issue =>
                issue.Code == CharacterMotionCompatibilityIssueCode.RigProfileMismatch).Path,
                Is.EqualTo("profile.rigProfileId"));
            Assert.That(Codes(malformedCase), Does.Contain(
                CharacterMotionCompatibilityIssueCode.InvalidTargetRigProfileId));
            Assert.That(malformedCase.IsCompatible, Is.False);
        }

        [Test]
        public void MissingDuplicateAndWrongProfileClipKeys_PreserveValidatorIssues()
        {
            var clips = new[]
            {
                Clip(MotionClipKey.Idle),
                Clip(MotionClipKey.Idle, clipSuffix: "idle-duplicate"),
                Clip(MotionClipKey.Locomotion),
                Clip(MotionClipKey.JumpTakeoff),
                Clip(MotionClipKey.JumpFall),
                Clip(MotionClipKey.JumpLand),
                Clip(MotionClipKey.AttackPrimary, declaredKey: MotionClipKey.Hit),
                Clip(MotionClipKey.AttackAbility),
                Clip(MotionClipKey.Hit)
            };
            var first = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(clips: clips),
                ValidTarget());
            var second = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(clips: clips.Reverse()),
                ValidTarget());

            Assert.That(Codes(first), Does.Contain(
                CharacterMotionCompatibilityIssueCode.MissingProfileClip));
            Assert.That(Codes(first), Does.Contain(
                CharacterMotionCompatibilityIssueCode.DuplicateProfileClip));
            Assert.That(Codes(first), Does.Contain(
                CharacterMotionCompatibilityIssueCode.WrongProfileClipKey));
            Assert.That(first.Issues.Any(issue =>
                issue.ProfileIssueCode == MotionProfileIssueCode.MissingClip), Is.True);
            Assert.That(first.Issues.Any(issue =>
                issue.ProfileIssueCode == MotionProfileIssueCode.DuplicateClip), Is.True);
            Assert.That(first.Issues.Any(issue =>
                issue.ProfileIssueCode == MotionProfileIssueCode.ClipKeyMismatch), Is.True);
            Assert.That(Signatures(first), Is.EqualTo(Signatures(second)));
        }

        [Test]
        public void FallbackPolicy_AllowsOrRejectsTheDeclaredFallbackExactly()
        {
            var allowed = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                ValidTarget(fallbackAllowed: true));
            var disallowed = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                ValidTarget(fallbackAllowed: false));

            Assert.That(allowed.IsCompatible, Is.True);
            Assert.That(disallowed.IsCompatible, Is.False);
            Assert.That(disallowed.Issues.Single().Code,
                Is.EqualTo(CharacterMotionCompatibilityIssueCode.DisallowedFallback));
            Assert.That(disallowed.Issues.Single().Path,
                Is.EqualTo("profile.fallbackProfileId"));
        }

        [Test]
        public void NullInvalidAndUnreadableInputs_FailClosedWithoutExceptionLeak()
        {
            Assert.That(Codes(CharacterMotionCompatibilityEvaluator.Evaluate(
                null,
                ValidTarget())), Does.Contain(CharacterMotionCompatibilityIssueCode.MissingProfile));
            Assert.That(Codes(CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                null)), Does.Contain(CharacterMotionCompatibilityIssueCode.MissingTarget));

            var invalidProfile = ValidProfile(
                schemaVersion: 99,
                motionProfileId: "Bad/Profile");
            var invalidResult = CharacterMotionCompatibilityEvaluator.Evaluate(
                invalidProfile,
                ValidTarget());
            Assert.That(invalidResult.Issues.Any(issue =>
                issue.Code == CharacterMotionCompatibilityIssueCode.InvalidProfile &&
                issue.ProfileIssueCode == MotionProfileIssueCode.UnsupportedSchemaVersion), Is.True);
            Assert.That(invalidResult.Issues.Any(issue =>
                issue.Code == CharacterMotionCompatibilityIssueCode.InvalidProfile &&
                issue.ProfileIssueCode == MotionProfileIssueCode.InvalidId), Is.True);

            var invalidTarget = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                ValidTarget(
                    family: (CharacterBodyFamily)99,
                    rigProfileId: "bad..rig"));
            Assert.That(Codes(invalidTarget), Does.Contain(
                CharacterMotionCompatibilityIssueCode.UnknownTargetFamily));
            Assert.That(Codes(invalidTarget), Does.Contain(
                CharacterMotionCompatibilityIssueCode.InvalidTargetRigProfileId));

            var nullKeys = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                Target(null));
            Assert.That(Codes(nullKeys), Does.Contain(
                CharacterMotionCompatibilityIssueCode.NullRequiredClipKeys));

            CharacterMotionTargetRequirements unreadableTarget = null;
            Assert.DoesNotThrow(() => unreadableTarget = Target(
                new ThrowingEnumerable<MotionClipKey>()));
            CharacterMotionCompatibilityResult unreadableResult = null;
            Assert.DoesNotThrow(() => unreadableResult =
                CharacterMotionCompatibilityEvaluator.Evaluate(
                    ValidProfile(),
                    unreadableTarget));
            Assert.That(Codes(unreadableResult), Does.Contain(
                CharacterMotionCompatibilityIssueCode.UnreadableRequiredClipKeys));
        }

        [Test]
        public void InvalidRequiredKeySets_AreOrderIndependentAndStructured()
        {
            var keys = new[]
            {
                MotionClipKey.Idle,
                MotionClipKey.Idle,
                MotionClipKey.Locomotion,
                MotionClipKey.JumpTakeoff,
                MotionClipKey.JumpFall,
                MotionClipKey.JumpLand,
                MotionClipKey.AttackPrimary,
                MotionClipKey.AttackAbility,
                MotionClipKey.Hit,
                (MotionClipKey)99
            };
            var first = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                ValidTarget(requiredClipKeys: keys));
            var second = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                ValidTarget(requiredClipKeys: keys.Reverse()));

            Assert.That(Codes(first), Does.Contain(
                CharacterMotionCompatibilityIssueCode.DuplicateRequiredClipKey));
            Assert.That(Codes(first), Does.Contain(
                CharacterMotionCompatibilityIssueCode.MissingRequiredClipKey));
            Assert.That(Codes(first), Does.Contain(
                CharacterMotionCompatibilityIssueCode.UnknownRequiredClipKey));
            Assert.That(Signatures(first), Is.EqualTo(Signatures(second)));
            Assert.That(first.Issues, Is.EqualTo(first.Issues
                .OrderBy(issue => issue.Path, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code)
                .ThenBy(issue => issue.ProfileIssueCode)
                .ToArray()));
        }

        [Test]
        public void TargetAndResult_AreImmutableWithoutUnityOrGameDependency()
        {
            var mutableKeys = RequiredKeys().ToList();
            var target = ValidTarget(requiredClipKeys: mutableKeys);
            mutableKeys.Clear();
            var result = CharacterMotionCompatibilityEvaluator.Evaluate(
                ValidProfile(),
                target);

            Assert.That(target.RequiredClipKeys.Count, Is.EqualTo(9));
            Assert.That(result.IsCompatible, Is.True);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<MotionClipKey>)target.RequiredClipKeys).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterMotionCompatibilityIssue>)result.Issues).Clear());
            Assert.That(typeof(CharacterMotionTargetRequirements)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterMotionCompatibilityIssue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterMotionCompatibilityResult)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);

            var dependencies = typeof(CharacterMotionCompatibilityEvaluator).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();
            Assert.That(dependencies, Has.Member("RealmRaiders.ModuleContracts"));
            Assert.That(dependencies.Any(name =>
                name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static CharacterMotionCompatibilityIssueCode[] Codes(
            CharacterMotionCompatibilityResult result)
        {
            return result.Issues.Select(issue => issue.Code).ToArray();
        }

        private static string[] Signatures(CharacterMotionCompatibilityResult result)
        {
            return result.Issues.Select(issue => issue.Signature).ToArray();
        }

        private static CharacterMotionTargetRequirements ValidTarget(
            CharacterBodyFamily family = CharacterBodyFamily.Beast,
            string rigProfileId = "test.rig.beast.v1",
            IEnumerable<MotionClipKey> requiredClipKeys = null,
            bool fallbackAllowed = true)
        {
            return Target(
                requiredClipKeys ?? RequiredKeys(),
                family,
                rigProfileId,
                fallbackAllowed);
        }

        private static CharacterMotionTargetRequirements Target(
            IEnumerable<MotionClipKey> requiredClipKeys,
            CharacterBodyFamily family = CharacterBodyFamily.Beast,
            string rigProfileId = "test.rig.beast.v1",
            bool fallbackAllowed = true)
        {
            return new CharacterMotionTargetRequirements(
                family,
                rigProfileId,
                requiredClipKeys,
                fallbackAllowed);
        }

        private static CharacterMotionProfile ValidProfile(
            int schemaVersion = CharacterMotionProfileValidator.SupportedSchemaVersion,
            string motionProfileId = "test.motion.beast.v1",
            CharacterBodyFamily family = CharacterBodyFamily.Beast,
            string rigProfileId = "test.rig.beast.v1",
            IEnumerable<MotionClipBinding> clips = null,
            string fallbackProfileId = "test.motion.fallback.v1")
        {
            return new CharacterMotionProfile(
                schemaVersion,
                motionProfileId,
                family,
                rigProfileId,
                "test.animator.beast.v1",
                clips ?? RequiredClips(),
                MotionRhythm.Neutral,
                fallbackProfileId,
                new[] { "test.source.beast.v1" });
        }

        private static IEnumerable<MotionClipKey> RequiredKeys()
        {
            yield return MotionClipKey.Idle;
            yield return MotionClipKey.Locomotion;
            yield return MotionClipKey.JumpTakeoff;
            yield return MotionClipKey.JumpFall;
            yield return MotionClipKey.JumpLand;
            yield return MotionClipKey.AttackPrimary;
            yield return MotionClipKey.AttackAbility;
            yield return MotionClipKey.Hit;
            yield return MotionClipKey.Death;
        }

        private static IEnumerable<MotionClipBinding> RequiredClips()
        {
            yield return Clip(MotionClipKey.Idle);
            yield return Clip(MotionClipKey.Locomotion);
            yield return Clip(MotionClipKey.JumpTakeoff);
            yield return Clip(MotionClipKey.JumpFall);
            yield return Clip(MotionClipKey.JumpLand);
            yield return Clip(MotionClipKey.AttackPrimary);
            yield return Clip(MotionClipKey.AttackAbility);
            yield return Clip(MotionClipKey.Hit);
            yield return Clip(MotionClipKey.Death);
        }

        private static MotionClipBinding Clip(
            MotionClipKey assignedKey,
            MotionClipKey? declaredKey = null,
            string clipSuffix = null)
        {
            var suffix = clipSuffix ?? ClipName(assignedKey);
            return new MotionClipBinding(
                assignedKey,
                "test.clip.beast." + suffix + ".v1",
                CharacterBodyFamily.Beast,
                "test.rig.beast.v1",
                declaredKey ?? assignedKey);
        }

        private static string ClipName(MotionClipKey key)
        {
            switch (key)
            {
                case MotionClipKey.AttackPrimary: return "attack-primary";
                case MotionClipKey.AttackAbility: return "attack-ability";
                default: return key.ToString().ToLowerInvariant();
            }
        }

        private sealed class ThrowingEnumerable<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                throw new InvalidOperationException("Synthetic unreadable input.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
