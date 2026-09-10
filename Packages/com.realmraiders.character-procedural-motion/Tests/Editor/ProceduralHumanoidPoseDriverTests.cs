using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules.CharacterMotionProfiles;
using UnityEngine;

namespace RealmRaiders.Modules.CharacterProceduralMotion.Tests
{
    public sealed class ProceduralHumanoidPoseDriverTests
    {
        private static readonly string[] RequiredNames =
        {
            "Bip01 L UpperArm", "Bip01 R UpperArm", "Bip01 L Thigh",
            "Bip01 R Thigh", "Bip01 L Calf", "Bip01 R Calf"
        };

        [Test]
        public void Bind_CachesExactLocalBaselinesAndSampleKeepsRootInvariant()
        {
            var root = CreateRig(out var bones);
            try
            {
                var rootPose = Pose.Of(root.transform);
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver();

                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.True);
                Assert.That(driver.IsBound, Is.True);
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0.8f, 1.25f, 1f / 60f);

                Assert.That(Pose.Of(root.transform), Is.EqualTo(rootPose));
                Assert.That(AnyRotationChanged(bones, baseline), Is.True);
                Assert.That(AllBoundsRespected(bones, baseline), Is.True);

                driver.Clear();
                Assert.That(driver.IsBound, Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [TestCase(MotionPresentationReaction.None, MotionPresentationAttack.None, MotionPresentationJumpPhase.None, MotionClipKey.Idle)]
        [TestCase(MotionPresentationReaction.None, MotionPresentationAttack.None, MotionPresentationJumpPhase.Takeoff, MotionClipKey.JumpTakeoff)]
        [TestCase(MotionPresentationReaction.None, MotionPresentationAttack.None, MotionPresentationJumpPhase.Falling, MotionClipKey.JumpFall)]
        [TestCase(MotionPresentationReaction.None, MotionPresentationAttack.None, MotionPresentationJumpPhase.Landing, MotionClipKey.JumpLand)]
        [TestCase(MotionPresentationReaction.None, MotionPresentationAttack.Primary, MotionPresentationJumpPhase.None, MotionClipKey.AttackPrimary)]
        [TestCase(MotionPresentationReaction.None, MotionPresentationAttack.Ability, MotionPresentationJumpPhase.None, MotionClipKey.AttackAbility)]
        [TestCase(MotionPresentationReaction.Hit, MotionPresentationAttack.None, MotionPresentationJumpPhase.None, MotionClipKey.Hit)]
        [TestCase(MotionPresentationReaction.Death, MotionPresentationAttack.None, MotionPresentationJumpPhase.None, MotionClipKey.Death)]
        public void Sample_AppliesEachSemanticStateWithinAdditiveBounds(
            MotionPresentationReaction reaction,
            MotionPresentationAttack attack,
            MotionPresentationJumpPhase jumpPhase,
            MotionClipKey expectedKey)
        {
            var root = CreateRig(out var bones);
            try
            {
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver();
                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.True);
                var input = Input(reaction, attack, jumpPhase, expectedKey == MotionClipKey.Locomotion);

                driver.Sample(input, expectedKey == MotionClipKey.Locomotion ? 1f : 0.8f, 0.75f, 1f / 60f);

                Assert.That(CharacterMotionPresentationResolver.Resolve(input), Is.EqualTo(expectedKey));
                Assert.That(AllBoundsRespected(bones, baseline), Is.True);
                if (expectedKey != MotionClipKey.Idle)
                    Assert.That(AnyRotationChanged(bones, baseline), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Sample_LocomotionUsesOpposedArmsAndThighsWithoutAccumulating()
        {
            var root = CreateRig(out var bones);
            try
            {
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver();
                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.True);
                var input = Input(isLocomoting: true);

                driver.Sample(input, 1f, 0.2f, 1f / 60f);
                var first = Snapshot(bones);
                driver.Sample(input, 1f, 0.2f, 1f / 60f);
                var second = Snapshot(bones);

                Assert.That(first, Is.EqualTo(second));
                Assert.That(Quaternion.Angle(baseline[0].Rotation, first[0].Rotation), Is.GreaterThan(0f));
                Assert.That(Quaternion.Angle(baseline[1].Rotation, first[1].Rotation), Is.GreaterThan(0f));
                Assert.That(Quaternion.Angle(baseline[2].Rotation, first[2].Rotation), Is.GreaterThan(0f));
                Assert.That(Quaternion.Angle(baseline[3].Rotation, first[3].Rotation), Is.GreaterThan(0f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Bind_InvalidOrDuplicateMapFailsClosedAndLeavesHierarchyUnchanged()
        {
            var root = CreateRig(out var bones);
            try
            {
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver();
                var invalid = new HumanoidBoneNameMap(
                    "Bip01 L UpperArm", "Bip01 L UpperArm", "Bip01 L Thigh",
                    "Bip01 R Thigh", "Bip01 L Calf", "Bip01 R Calf");

                Assert.That(driver.Bind(root.transform, invalid), Is.False);
                driver.Sample(Input(attack: MotionPresentationAttack.Ability), 1f, 1f, 1f / 60f);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));

                var missing = new HumanoidBoneNameMap(
                    "Bip01 L UpperArm", "Bip01 R UpperArm", "Bip01 L Thigh",
                    "Bip01 R Thigh", "Bip01 L Calf", "Bip01 Missing Calf");
                Assert.That(driver.Bind(root.transform, missing), Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));

                new GameObject("Bip01 L UpperArm").transform.SetParent(root.transform, false);
                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Rebind_RestoresOldHierarchyBeforeCapturingNewBaseline()
        {
            var firstRoot = CreateRig(out var firstBones);
            var secondRoot = CreateRig(out var secondBones);
            try
            {
                var firstBaseline = Snapshot(firstBones);
                var secondBaseline = Snapshot(secondBones);
                var driver = new ProceduralHumanoidPoseDriver();
                Assert.That(driver.Bind(firstRoot.transform, Bip01Map()), Is.True);
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 1f, 0.4f, 1f / 60f);

                Assert.That(driver.Bind(secondRoot.transform, Bip01Map()), Is.True);
                Assert.That(Snapshot(firstBones), Is.EqualTo(firstBaseline));
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Landing), 1f, 0.4f, 1f / 60f);
                Assert.That(AnyRotationChanged(secondBones, secondBaseline), Is.True);
                driver.Clear();
                Assert.That(Snapshot(secondBones), Is.EqualTo(secondBaseline));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstRoot);
                UnityEngine.Object.DestroyImmediate(secondRoot);
            }
        }

        [Test]
        public void Contracts_AreImmutableAndDoNotReferenceGameplayAssembly()
        {
            Assert.That(typeof(HumanoidBoneNameMap).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
            var dependencies = typeof(ProceduralHumanoidPoseDriver).Assembly.GetReferencedAssemblies().Select(assembly => assembly.Name).ToArray();
            Assert.That(dependencies, Has.Member("RealmRaiders.CharacterMotionProfiles"));
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static CharacterMotionPresentationInput Input(
            MotionPresentationReaction reaction = MotionPresentationReaction.None,
            MotionPresentationAttack attack = MotionPresentationAttack.None,
            MotionPresentationJumpPhase jumpPhase = MotionPresentationJumpPhase.None,
            bool isLocomoting = false)
        {
            return new CharacterMotionPresentationInput(reaction, attack, jumpPhase, isLocomoting);
        }

        private static HumanoidBoneNameMap Bip01Map()
        {
            return new HumanoidBoneNameMap(
                RequiredNames[0], RequiredNames[1], RequiredNames[2],
                RequiredNames[3], RequiredNames[4], RequiredNames[5]);
        }

        private static GameObject CreateRig(out Transform[] bones)
        {
            var root = new GameObject("Visual Root");
            bones = new Transform[RequiredNames.Length];
            for (var index = 0; index < RequiredNames.Length; index++)
            {
                var child = new GameObject(RequiredNames[index]).transform;
                child.SetParent(root.transform, false);
                child.localPosition = new Vector3(index * 0.1f, index * 0.05f, 0f);
                child.localRotation = Quaternion.Euler(index * 2f, index * 3f, index * 4f);
                child.localScale = new Vector3(1f + index * 0.01f, 1f, 1f);
                bones[index] = child;
            }
            return root;
        }

        private static BonePose[] Snapshot(Transform[] bones)
        {
            var result = new BonePose[bones.Length];
            for (var index = 0; index < bones.Length; index++)
                result[index] = BonePose.Of(bones[index]);
            return result;
        }

        private static bool AnyRotationChanged(Transform[] bones, BonePose[] baseline)
        {
            for (var index = 0; index < bones.Length; index++)
            {
                if (Quaternion.Angle(bones[index].localRotation, baseline[index].Rotation) > 0.001f)
                    return true;
            }
            return false;
        }

        private static bool AllBoundsRespected(Transform[] bones, BonePose[] baseline)
        {
            for (var index = 0; index < bones.Length; index++)
            {
                if (Quaternion.Angle(bones[index].localRotation, baseline[index].Rotation) >
                    ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees + 0.001f)
                    return false;
                if (bones[index].localPosition != baseline[index].Position || bones[index].localScale != baseline[index].Scale)
                    return false;
            }
            return true;
        }

        private readonly struct BonePose : IEquatable<BonePose>
        {
            public BonePose(Vector3 position, Quaternion rotation, Vector3 scale)
            {
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }

            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
            public Vector3 Scale { get; }

            public static BonePose Of(Transform transform)
            {
                return new BonePose(transform.localPosition, transform.localRotation, transform.localScale);
            }

            public bool Equals(BonePose other)
            {
                return Position == other.Position && Rotation == other.Rotation && Scale == other.Scale;
            }

            public override bool Equals(object value)
            {
                return value is BonePose other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Position.GetHashCode() ^ Rotation.GetHashCode() ^ Scale.GetHashCode();
            }
        }
    }
}
