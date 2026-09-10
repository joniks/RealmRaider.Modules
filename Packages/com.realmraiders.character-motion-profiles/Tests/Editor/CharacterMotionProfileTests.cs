using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterMotionProfiles.Tests
{
    public sealed class CharacterMotionProfileTests
    {
        [Test]
        public void ValidProfile_NormalizesClipOrderAndProducesStableBomlessUtf8Hash()
        {
            var first = ValidProfile(RequiredClips().Reverse());
            var second = ValidProfile(RequiredClips());

            var firstBytes = CharacterMotionProfileCanonicalizer.SerializeUtf8(first);
            var secondBytes = CharacterMotionProfileCanonicalizer.SerializeUtf8(second);

            Assert.That(CharacterMotionProfileValidator.Validate(first), Is.Empty);
            Assert.That(firstBytes, Is.EqualTo(secondBytes));
            Assert.That(firstBytes[0], Is.EqualTo((byte)'{'));
            Assert.That(Encoding.UTF8.GetString(firstBytes), Does.StartWith("{\"schemaVersion\":1,\"motionProfileId\":"));
            Assert.That(CharacterMotionProfileCanonicalizer.ContentHash(first), Is.EqualTo(CharacterMotionProfileCanonicalizer.ContentHash(second)));
            Assert.That(CharacterMotionProfileCanonicalizer.ContentHash(first), Does.Match("^[0-9a-f]{64}$"));
        }

        [Test]
        public void MotionClipKeys_PreserveExistingOrdinalsAndAppendJumpKeys()
        {
            Assert.That((int)MotionClipKey.Idle, Is.EqualTo(0));
            Assert.That((int)MotionClipKey.Locomotion, Is.EqualTo(1));
            Assert.That((int)MotionClipKey.AttackPrimary, Is.EqualTo(2));
            Assert.That((int)MotionClipKey.AttackAbility, Is.EqualTo(3));
            Assert.That((int)MotionClipKey.Hit, Is.EqualTo(4));
            Assert.That((int)MotionClipKey.Death, Is.EqualTo(5));
            Assert.That((int)MotionClipKey.JumpTakeoff, Is.EqualTo(6));
            Assert.That((int)MotionClipKey.JumpFall, Is.EqualTo(7));
            Assert.That((int)MotionClipKey.JumpLand, Is.EqualTo(8));
        }

        [Test]
        public void Validator_RejectsUnsupportedSchemaInvalidIdsFamilyAndRhythm()
        {
            var profile = new CharacterMotionProfile(
                2,
                "Bad/Path",
                (CharacterBodyFamily)99,
                string.Empty,
                "realmraiders.anim..beast",
                RequiredClips(),
                (MotionRhythm)99,
                "scene/fallback",
                new[] { "realmraiders.source.motion" });

            var codes = Codes(profile);

            Assert.That(codes, Does.Contain(MotionProfileIssueCode.UnsupportedSchemaVersion));
            Assert.That(codes, Does.Contain(MotionProfileIssueCode.InvalidId));
            Assert.That(codes, Does.Contain(MotionProfileIssueCode.MissingId));
            Assert.That(codes, Does.Contain(MotionProfileIssueCode.UnknownFamily));
            Assert.That(codes, Does.Contain(MotionProfileIssueCode.UnknownRhythm));
            Assert.Throws<ArgumentException>(() => CharacterMotionProfileCanonicalizer.SerializeUtf8(profile));
        }

        [Test]
        public void Validator_RequiresExactlyNineKnownUniqueClipKeys()
        {
            var missing = ValidProfile(RequiredClips().Where(clip => clip.AssignedKey != MotionClipKey.Death));
            var duplicate = ValidProfile(RequiredClips().Concat(new[] { Clip(MotionClipKey.Idle) }));
            var unknown = ValidProfile(RequiredClips().Concat(new[]
            {
                new MotionClipBinding((MotionClipKey)99, "realmraiders.clip.beast.unknown.v1", CharacterBodyFamily.Beast, RigId, (MotionClipKey)99)
            }));

            Assert.That(Codes(missing), Does.Contain(MotionProfileIssueCode.MissingClip));
            Assert.That(Codes(duplicate), Does.Contain(MotionProfileIssueCode.DuplicateClip));
            Assert.That(Codes(unknown), Does.Contain(MotionProfileIssueCode.UnknownClip));
        }

        [Test]
        public void Validator_RejectsDeclaredFamilyRigAndClipKeyMismatch()
        {
            var clips = RequiredClips().ToArray();
            clips[0] = new MotionClipBinding(
                MotionClipKey.Idle,
                "realmraiders.clip.large-creature.locomotion.v1",
                CharacterBodyFamily.LargeCreature,
                "realmraiders.rig.large-creature.v1",
                MotionClipKey.Locomotion);

            var codes = Codes(ValidProfile(clips));

            Assert.That(codes, Does.Contain(MotionProfileIssueCode.FamilyMismatch));
            Assert.That(codes, Does.Contain(MotionProfileIssueCode.RigMismatch));
            Assert.That(codes, Does.Contain(MotionProfileIssueCode.ClipKeyMismatch));
        }

        [Test]
        public void Validator_RejectsMissingUnorderedAndNonAdjacentDuplicateSources()
        {
            var missing = ValidProfile(sourceIds: new string[0]);
            var unordered = ValidProfile(sourceIds: new[] { "realmraiders.source.zeta", "realmraiders.source.alpha" });
            var repeated = ValidProfile(sourceIds: new[]
            {
                "realmraiders.source.alpha",
                "realmraiders.source.beta",
                "realmraiders.source.alpha"
            });

            Assert.That(Codes(missing), Does.Contain(MotionProfileIssueCode.MissingSourceId));
            Assert.That(Codes(unordered), Does.Contain(MotionProfileIssueCode.UnorderedSourceIds));
            Assert.That(Codes(repeated), Does.Contain(MotionProfileIssueCode.DuplicateSourceId).And.Contain(MotionProfileIssueCode.UnorderedSourceIds));
            Assert.Throws<ArgumentException>(() => CharacterMotionProfileCanonicalizer.ContentHash(repeated));
        }

        [Test]
        public void ClosedRootPolicy_RejectsMutablePathSceneTimestampRandomAndUnknownFields()
        {
            var fields = CanonicalRootFields().Concat(new[] { "displayName", "assetPath", "sceneName", "timestamp", "randomSeed" });
            var issues = CharacterMotionProfileValidator.ValidateRootFields(fields).ToArray();

            Assert.That(issues.Count(issue => issue.Code == MotionProfileIssueCode.UnknownField), Is.EqualTo(5));
            Assert.That(issues.Select(issue => issue.Path), Does.Contain("displayName").And.Contain("assetPath").And.Contain("sceneName").And.Contain("timestamp").And.Contain("randomSeed"));
        }

        [Test]
        public void ClosedRootPolicy_RejectsMissingAndDuplicateFields()
        {
            var fields = CanonicalRootFields()
                .Where(field => field != "fallbackProfileId")
                .Concat(new[] { "sourceIds" });
            var issues = CharacterMotionProfileValidator.ValidateRootFields(fields).ToArray();

            Assert.That(issues.Any(issue => issue.Code == MotionProfileIssueCode.MissingField && issue.Path == "fallbackProfileId"), Is.True);
            Assert.That(issues.Any(issue => issue.Code == MotionProfileIssueCode.DuplicateField && issue.Path == "sourceIds"), Is.True);
        }

        [Test]
        public void Contracts_AreImmutableAndRuntimeHasNoUnityOrGameRuntimeDependency()
        {
            Assert.That(typeof(CharacterMotionProfile).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(MotionClipBinding).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);

            var profile = ValidProfile();
            Assert.Throws<NotSupportedException>(() => ((IList<string>)profile.SourceIds).Add("realmraiders.source.other"));
            Assert.Throws<NotSupportedException>(() => ((IList<MotionClipBinding>)profile.Clips).Clear());

            var dependencies = typeof(CharacterMotionProfile).Assembly.GetReferencedAssemblies().Select(assembly => assembly.Name).ToArray();
            Assert.That(dependencies, Has.Member("RealmRaiders.ModuleContracts"));
            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private const string RigId = "realmraiders.rig.beast.v1";

        private static CharacterMotionProfile ValidProfile(
            IEnumerable<MotionClipBinding> clips = null,
            IEnumerable<string> sourceIds = null)
        {
            return new CharacterMotionProfile(
                CharacterMotionProfileValidator.SupportedSchemaVersion,
                "realmraiders.motion.beast.sylvan.v1",
                CharacterBodyFamily.Beast,
                RigId,
                "realmraiders.anim.beast.v1",
                clips ?? RequiredClips(),
                MotionRhythm.Sylvan,
                "realmraiders.motion.beast.neutral.v1",
                sourceIds ?? new[] { "realmraiders.source.beast-motion", "realmraiders.source.beast-rig" });
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

        private static MotionClipBinding Clip(MotionClipKey key)
        {
            return new MotionClipBinding(
                key,
                "realmraiders.clip.beast." + ClipName(key) + ".v1",
                CharacterBodyFamily.Beast,
                RigId,
                key);
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

        private static MotionProfileIssueCode[] Codes(CharacterMotionProfile profile)
        {
            return CharacterMotionProfileValidator.Validate(profile).Select(issue => issue.Code).ToArray();
        }

        private static IEnumerable<string> CanonicalRootFields()
        {
            return new[]
            {
                "schemaVersion", "motionProfileId", "family", "rigProfileId", "animatorProfileId",
                "clips", "rhythmProfile", "fallbackProfileId", "sourceIds"
            };
        }
    }
}
