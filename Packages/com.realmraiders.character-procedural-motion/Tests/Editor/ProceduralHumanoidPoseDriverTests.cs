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
                var rootPose = BonePose.Of(root.transform);
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver();

                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.True);
                Assert.That(driver.IsBound, Is.True);
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0.8f, 1.25f, 1f / 60f);

                Assert.That(BonePose.Of(root.transform), Is.EqualTo(rootPose));
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
        public void Tuning_CompatibilityDefaultPreservesLegacyStrengthsAndBloodKnightIsMoreReadable()
        {
            var compatibility = ProceduralHumanoidMotionTuning.CompatibilityDefault;
            var bloodKnight = ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable;

            Assert.That(compatibility.SwingCadenceRadiansPerSecond, Is.EqualTo(6f));
            Assert.That(compatibility.IdleArmDegrees, Is.EqualTo(2f));
            Assert.That(compatibility.Locomotion.UpperArmDegrees, Is.EqualTo(22f));
            Assert.That(compatibility.Locomotion.ThighDegrees, Is.EqualTo(18f));
            Assert.That(compatibility.Locomotion.CalfDegrees, Is.EqualTo(6.3f));
            Assert.That(compatibility.Locomotion.UpperArmAxis, Is.EqualTo(ProceduralHumanoidLocalAxis.Right));
            Assert.That(compatibility.AsymmetricJumpTakeoff.LeftThighDegrees, Is.EqualTo(-10f));
            Assert.That(compatibility.AsymmetricJumpTakeoff.RightThighDegrees, Is.EqualTo(-10f));
            Assert.That(bloodKnight.Locomotion.UpperArmDegrees, Is.EqualTo(56f));
            Assert.That(bloodKnight.Locomotion.ThighDegrees, Is.EqualTo(50f));
            Assert.That(bloodKnight.Locomotion.MaxAdditiveAngleDegrees, Is.EqualTo(60f));
            Assert.That(bloodKnight.DeepCrouch.LeftThighDegrees, Is.EqualTo(-88f));
            Assert.That(bloodKnight.DeepCrouch.RightThighDegrees, Is.EqualTo(-88f));
            Assert.That(bloodKnight.JumpPresentationDurationMultiplier, Is.EqualTo(4f));
            Assert.That(bloodKnight.TakeoffStraightenDurationMultiplier, Is.EqualTo(3f));
            Assert.That(bloodKnight.Locomotion.UpperArmAxis, Is.EqualTo(ProceduralHumanoidLocalAxis.Forward));
            Assert.That(bloodKnight.Locomotion.ThighAxis, Is.EqualTo(ProceduralHumanoidLocalAxis.Forward));
            Assert.That(bloodKnight.AsymmetricJumpTakeoff.LeftThighDegrees, Is.EqualTo(-22f));
            Assert.That(bloodKnight.AsymmetricJumpTakeoff.RightThighDegrees, Is.EqualTo(12f));
            Assert.That(Math.Abs(bloodKnight.PrimaryWeaponArmDegrees), Is.GreaterThan(Math.Abs(compatibility.PrimaryWeaponArmDegrees)));
            Assert.That(Math.Abs(bloodKnight.JumpLand.ThighDegrees), Is.GreaterThan(Math.Abs(compatibility.JumpLand.ThighDegrees)));
        }

        [Test]
        public void Tuning_NonFiniteNegativeAndUnsafeValuesAreDeterministicallyClamped()
        {
            var unsafePose = new ProceduralHumanoidLimbPose(float.PositiveInfinity, float.NegativeInfinity, 99f);
            var tuning = new ProceduralHumanoidMotionTuning(
                float.NaN, float.PositiveInfinity, unsafePose, unsafePose, unsafePose, unsafePose,
                float.NegativeInfinity, 99f, unsafePose, unsafePose, unsafePose);
            var fast = new ProceduralHumanoidMotionTuning(
                99f, -99f, unsafePose, unsafePose, unsafePose, unsafePose,
                -99f, 99f, unsafePose, unsafePose, unsafePose);

            Assert.That(tuning.SwingCadenceRadiansPerSecond, Is.EqualTo(0f));
            Assert.That(tuning.IdleArmDegrees, Is.EqualTo(0f));
            Assert.That(tuning.PrimaryWeaponArmDegrees, Is.EqualTo(0f));
            Assert.That(tuning.PrimarySupportArmDegrees, Is.EqualTo(ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees));
            Assert.That(tuning.Locomotion.UpperArmDegrees, Is.EqualTo(0f));
            Assert.That(tuning.Locomotion.ThighDegrees, Is.EqualTo(0f));
            Assert.That(tuning.Locomotion.CalfDegrees, Is.EqualTo(ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees));
            Assert.That(fast.SwingCadenceRadiansPerSecond, Is.EqualTo(ProceduralHumanoidMotionTuning.MaxSwingCadenceRadiansPerSecond));
            Assert.That(fast.IdleArmDegrees, Is.EqualTo(-ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees));
            Assert.That(fast.PrimaryWeaponArmDegrees, Is.EqualTo(-ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees));
            var invalidAxisPose = new ProceduralHumanoidLimbPose(1f, 2f, 3f, (ProceduralHumanoidLocalAxis)99, (ProceduralHumanoidLocalAxis)(-1), ProceduralHumanoidLocalAxis.Up);
            Assert.That(invalidAxisPose.UpperArmAxis, Is.EqualTo(ProceduralHumanoidLocalAxis.Right));
            Assert.That(invalidAxisPose.ThighAxis, Is.EqualTo(ProceduralHumanoidLocalAxis.Right));
            Assert.That(invalidAxisPose.CalfAxis, Is.EqualTo(ProceduralHumanoidLocalAxis.Up));
        }

        [Test]
        public void CompatibilityDefault_LocomotionRetainsLegacyRightAxisAngles()
        {
            var root = CreateRig(out var bones);
            try
            {
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver();
                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.True);
                driver.Sample(Input(isLocomoting: true), 1f, 0.2f, 1f / 60f);

                var swing = Mathf.Sin((0.2f + 1f / 60f) * 6f);
                AssertLocalRotation(bones[0], baseline[0], Vector3.right, 22f * swing);
                AssertLocalRotation(bones[1], baseline[1], Vector3.right, -22f * swing);
                AssertLocalRotation(bones[2], baseline[2], Vector3.right, -18f * swing);
                AssertLocalRotation(bones[3], baseline[3], Vector3.right, 18f * swing);
                AssertLocalRotation(bones[4], baseline[4], Vector3.right, 6.3f * swing);
                AssertLocalRotation(bones[5], baseline[5], Vector3.right, -6.3f * swing);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void BloodKnight_LocomotionUsesForwardCounterSwingAndAsymmetricForwardTakeoff()
        {
            var root = CreateRig(out var bones);
            try
            {
                var rootPose = BonePose.Of(root.transform);
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.True);

                driver.Sample(Input(isLocomoting: true), 1f, 0.2f, 1f / 60f);
                var swing = Mathf.Sin((0.2f + 1f / 60f) * 7.5f);
                AssertLocalRotation(bones[0], baseline[0], Vector3.forward, 56f * swing);
                AssertLocalRotation(bones[1], baseline[1], Vector3.forward, -56f * swing);
                AssertLocalRotation(bones[2], baseline[2], Vector3.forward, -50f * swing);
                AssertLocalRotation(bones[3], baseline[3], Vector3.forward, 50f * swing);
                AssertLocalRotation(bones[4], baseline[4], Vector3.forward, 26f * swing);
                AssertLocalRotation(bones[5], baseline[5], Vector3.forward, -26f * swing);

                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Takeoff), 0f, 0f, 0f, float.NaN);
                AssertLocalRotation(bones[2], baseline[2], Vector3.forward, -88f);
                AssertLocalRotation(bones[3], baseline[3], Vector3.forward, -88f);
                AssertLocalRotation(bones[4], baseline[4], Vector3.forward, 80f);
                AssertLocalRotation(bones[5], baseline[5], Vector3.forward, 80f);
                Assert.That(AllBoundsRespected(bones, baseline, 90f), Is.True);
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Takeoff), 0f, 0f, 0f, 0.5f);
                AssertLocalRotation(bones[2], baseline[2], Vector3.forward, -55f);
                AssertLocalRotation(bones[3], baseline[3], Vector3.forward, -38f);
                AssertLocalRotation(bones[4], baseline[4], Vector3.forward, 50f);
                AssertLocalRotation(bones[5], baseline[5], Vector3.forward, 35f);
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Takeoff), 0f, 0f, 0f, 2f);
                AssertLocalRotation(bones[0], baseline[0], Vector3.forward, -18f);
                AssertLocalRotation(bones[1], baseline[1], Vector3.forward, -18f);
                AssertLocalRotation(bones[2], baseline[2], Vector3.forward, -22f);
                AssertLocalRotation(bones[3], baseline[3], Vector3.forward, 12f);
                AssertLocalRotation(bones[4], baseline[4], Vector3.forward, 20f);
                AssertLocalRotation(bones[5], baseline[5], Vector3.forward, -10f);
                var pushBoundary = Snapshot(bones);
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Falling), 0f, 0f, 0f, float.NaN);
                Assert.That(Snapshot(bones), Is.EqualTo(pushBoundary));
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Falling), 0f, 0f, 0f, 1f);
                AssertLocalRotation(bones[0], baseline[0], Vector3.forward, 22f);
                AssertLocalRotation(bones[2], baseline[2], Vector3.forward, 12f);
                var fallBoundary = Snapshot(bones);
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Landing), 0f, 0f, 0f, -1f);
                Assert.That(Snapshot(bones), Is.EqualTo(fallBoundary));
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Landing), 0f, 0f, 0f, 0.5f);
                AssertLocalRotation(bones[0], baseline[0], Vector3.forward, -10f);
                AssertLocalRotation(bones[2], baseline[2], Vector3.forward, -22f);
                AssertLocalRotation(bones[4], baseline[4], Vector3.forward, 24f);
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Landing), 0f, 0f, 0f, 2f);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
                Assert.That(BonePose.Of(root.transform), Is.EqualTo(rootPose));
                Assert.That(AllBoundsRespected(bones, baseline, 60f), Is.True);

                driver.Clear();
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void BloodKnightPreset_SamplesDeterministicallyWithMeaningfulPoseDeltaAndNoTransformDrift()
        {
            var defaultRoot = CreateRig(out var defaultBones);
            var bloodKnightRoot = CreateRig(out var bloodKnightBones);
            try
            {
                var defaultRootPose = BonePose.Of(defaultRoot.transform);
                var bloodKnightRootPose = BonePose.Of(bloodKnightRoot.transform);
                var defaultBaseline = Snapshot(defaultBones);
                var bloodKnightBaseline = Snapshot(bloodKnightBones);
                var defaultDriver = new ProceduralHumanoidPoseDriver();
                var bloodKnightDriver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                var locomotion = Input(isLocomoting: true);

                Assert.That(defaultDriver.Bind(defaultRoot.transform, Bip01Map()), Is.True);
                Assert.That(bloodKnightDriver.Bind(bloodKnightRoot.transform, Bip01Map()), Is.True);
                defaultDriver.Sample(locomotion, 1f, 0.2f, 1f / 60f);
                bloodKnightDriver.Sample(locomotion, 1f, 0.2f, 1f / 60f);
                var firstBloodKnightSample = Snapshot(bloodKnightBones);
                bloodKnightDriver.Sample(locomotion, 1f, 0.2f, 1f / 60f);

                Assert.That(Snapshot(bloodKnightBones), Is.EqualTo(firstBloodKnightSample));
                Assert.That(BonePose.Of(defaultRoot.transform), Is.EqualTo(defaultRootPose));
                Assert.That(BonePose.Of(bloodKnightRoot.transform), Is.EqualTo(bloodKnightRootPose));
                Assert.That(AllBoundsRespected(defaultBones, defaultBaseline), Is.True);
                Assert.That(AllBoundsRespected(bloodKnightBones, bloodKnightBaseline, 60f), Is.True);
                Assert.That(RotationMagnitude(bloodKnightBones, bloodKnightBaseline),
                    Is.GreaterThan(RotationMagnitude(defaultBones, defaultBaseline) + 10f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(defaultRoot);
                UnityEngine.Object.DestroyImmediate(bloodKnightRoot);
            }
        }

        [Test]
        public void Contracts_AreImmutableAndDoNotReferenceGameplayAssembly()
        {
            Assert.That(typeof(HumanoidBoneNameMap).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ProceduralHumanoidLimbPose).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ProceduralHumanoidTakeoffPose).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ProceduralHumanoidMotionTuning).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
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

        private static bool AllBoundsRespected(Transform[] bones, BonePose[] baseline, float maximum = ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees)
        {
            for (var index = 0; index < bones.Length; index++)
            {
                if (Quaternion.Angle(bones[index].localRotation, baseline[index].Rotation) >
                    maximum + 0.001f)
                    return false;
                if (bones[index].localPosition != baseline[index].Position || bones[index].localScale != baseline[index].Scale)
                    return false;
            }
            return true;
        }

        private static float RotationMagnitude(Transform[] bones, BonePose[] baseline)
        {
            var total = 0f;
            for (var index = 0; index < bones.Length; index++)
                total += Quaternion.Angle(bones[index].localRotation, baseline[index].Rotation);
            return total;
        }

        private static void AssertLocalRotation(Transform bone, BonePose baseline, Vector3 axis, float degrees)
        {
            var expected = baseline.Rotation * Quaternion.AngleAxis(degrees, axis);
            Assert.That(Quaternion.Angle(bone.localRotation, expected), Is.LessThan(0.001f));
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
