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

        private static readonly string[] SagittalAxisFieldNames =
        {
            "leftUpperArmSagittalAxis", "rightUpperArmSagittalAxis", "leftThighSagittalAxis",
            "rightThighSagittalAxis", "leftCalfSagittalAxis", "rightCalfSagittalAxis"
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
        public void CombatSample_DirectionalInputsPriorityAndCleanupAreDeterministic()
        {
            var root = CreateRig(out var bones);
            try
            {
                var rootPose = BonePose.Of(root.transform);
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.True);
                var attack = Input(attack: MotionPresentationAttack.Primary);
                var neutralAttack = Combat(ProceduralHumanoidAttackStage.Impact, 1f, 1f);
                driver.Sample(attack, 0f, 0f, 0f, 1f, neutralAttack);
                var neutralPose = Snapshot(bones);
                Assert.That(Quaternion.Angle(baseline[0].Rotation, neutralPose[0].Rotation), Is.GreaterThan(0f));
                var left = Combat(ProceduralHumanoidAttackStage.Impact, 1f, 1f, -1f);
                var right = Combat(ProceduralHumanoidAttackStage.Impact, 1f, 1f, 1f);
                driver.Sample(attack, 0f, 0f, 0f, 1f, left);
                var leftPose = Snapshot(bones);
                driver.Sample(attack, 0f, 0f, 0f, 1f, right);
                var rightPose = Snapshot(bones);
                Assert.That(Quaternion.Angle(baseline[0].Rotation, leftPose[0].Rotation), Is.LessThan(Quaternion.Angle(baseline[1].Rotation, leftPose[1].Rotation)));
                Assert.That(Quaternion.Angle(baseline[0].Rotation, rightPose[0].Rotation), Is.GreaterThan(Quaternion.Angle(baseline[1].Rotation, rightPose[1].Rotation)));
                driver.Sample(attack, 0f, 0f, 0f, 1f, right);
                Assert.That(Snapshot(bones), Is.EqualTo(rightPose), "Repeated attack samples must be exactly idempotent.");

                var hitInput = Input(reaction: MotionPresentationReaction.Hit);
                var hit = Combat(hitProgress: 0.5f, hitWeight: 1f, recoilDirection: -1f);
                driver.Sample(hitInput, 0f, 0f, 0f, 1f, hit);
                var hitPose = Snapshot(bones);
                driver.Sample(hitInput, 0f, 0f, 0f, 1f, hit);
                Assert.That(Snapshot(bones), Is.EqualTo(hitPose), "Repeated hit samples must be exactly idempotent.");

                driver.Sample(attack, 0f, 0f, 0f, 1f, new ProceduralHumanoidCombatPoseSample(ProceduralHumanoidAttackStage.Impact, float.NaN, float.PositiveInfinity, float.NaN, 0f, 0f, 0f));
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));

                var deathOverAttack = Input(reaction: MotionPresentationReaction.Death, attack: MotionPresentationAttack.Primary);
                driver.Sample(deathOverAttack, 0f, 0f, 0f);
                var legacyDeath = Snapshot(bones);
                driver.Sample(deathOverAttack, 0f, 0f, 0f, 1f, neutralAttack);
                Assert.That(Snapshot(bones), Is.EqualTo(legacyDeath), "Death must remain the exact legacy pose above progressive combat.");

                var hitOverAttack = Input(reaction: MotionPresentationReaction.Hit, attack: MotionPresentationAttack.Primary);
                driver.Sample(hitOverAttack, 0f, 0f, 0f, 1f, hit);
                var priorityHit = Snapshot(bones);
                driver.Sample(hitInput, 0f, 0f, 0f, 1f, hit);
                Assert.That(Snapshot(bones), Is.EqualTo(priorityHit), "Hit must resolve identically with or without a simultaneous attack.");
                Assert.That(priorityHit, Is.Not.EqualTo(neutralPose), "Hit must remain above Attack in the resolver priority.");

                Assert.That(AllBoundsRespected(bones, baseline), Is.True);
                Assert.That(BonePose.Of(root.transform), Is.EqualTo(rootPose));
                driver.Clear();
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void CombatSample_StagesAndHitJoinSameClockLocomotionWithoutIntermediateDiscontinuities()
        {
            var root = CreateRig(out var bones);
            try
            {
                var rootPose = BonePose.Of(root.transform);
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.True);
                var locomotion = Input(isLocomoting: true);
                var attack = Input(attack: MotionPresentationAttack.Primary);
                var hit = Input(reaction: MotionPresentationReaction.Hit);
                const float speed = 0.7f;
                const float clock = 0.31f;
                const float delta = 1f / 60f;
                driver.Sample(locomotion, speed, clock, delta);
                var liveBase = Snapshot(bones);
                foreach (var stage in new[] { ProceduralHumanoidAttackStage.Windup, ProceduralHumanoidAttackStage.Impact, ProceduralHumanoidAttackStage.Recovery })
                {
                    driver.Sample(attack, speed, clock, delta, 1f, Combat(stage, 0.5f, 0f));
                    Assert.That(Snapshot(bones), Is.EqualTo(liveBase), $"{stage} with zero blend must be the live locomotion base.");
                }

                driver.Sample(attack, speed, clock, delta, 1f, Combat(ProceduralHumanoidAttackStage.Windup, 0f));
                var windupStart = Snapshot(bones);
                driver.Sample(attack, speed, clock, delta, 1f, Combat(ProceduralHumanoidAttackStage.Windup, 0.5f));
                var windupMiddle = Snapshot(bones);
                driver.Sample(attack, speed, clock, delta, 1f, Combat(ProceduralHumanoidAttackStage.Windup, 1f));
                var windupEnd = Snapshot(bones);
                AssertIntermediate(windupStart, windupMiddle, windupEnd, "Windup");

                driver.Sample(attack, speed, clock, delta, 1f, Combat((ProceduralHumanoidAttackStage)99, 1f));
                Assert.That(Snapshot(bones), Is.EqualTo(windupEnd));

                driver.Sample(attack, speed, clock, delta, 1f, Combat(ProceduralHumanoidAttackStage.Impact, 0f));
                var impactStart = Snapshot(bones);
                Assert.That(Snapshot(bones), Is.EqualTo(windupEnd));
                driver.Sample(attack, speed, clock, delta, 1f, Combat(ProceduralHumanoidAttackStage.Impact, 0.5f));
                var impactMiddle = Snapshot(bones);
                driver.Sample(attack, speed, clock, delta, 1f, Combat(ProceduralHumanoidAttackStage.Impact, 1f));
                var impactEnd = Snapshot(bones);
                AssertIntermediate(impactStart, impactMiddle, impactEnd, "Impact");

                driver.Sample(attack, speed, clock, delta, 1f, Combat(ProceduralHumanoidAttackStage.Recovery, 0f));
                var recoveryStart = Snapshot(bones);
                Assert.That(Snapshot(bones), Is.EqualTo(impactEnd));
                driver.Sample(attack, speed, clock, delta, 1f, Combat(ProceduralHumanoidAttackStage.Recovery, 0.5f));
                var recoveryMiddle = Snapshot(bones);
                driver.Sample(attack, speed, clock, delta, 1f, Combat(ProceduralHumanoidAttackStage.Recovery, 1f));
                var recoveryEnd = Snapshot(bones);
                AssertIntermediate(recoveryStart, recoveryMiddle, recoveryEnd, "Recovery");
                Assert.That(recoveryEnd, Is.EqualTo(liveBase));

                driver.Sample(hit, speed, clock, delta, 1f, Combat(hitProgress: 0f, hitWeight: 1f));
                var hitStart = Snapshot(bones);
                driver.Sample(hit, speed, clock, delta, 1f, Combat(hitProgress: 0.5f, hitWeight: 1f, recoilDirection: 1f));
                var hitMiddle = Snapshot(bones);
                driver.Sample(hit, speed, clock, delta, 1f, Combat(hitProgress: 1f, hitWeight: 1f, recoilDirection: 1f));
                var hitEnd = Snapshot(bones);
                AssertIntermediate(hitStart, hitMiddle, hitEnd, "Hit");
                Assert.That(hitStart, Is.EqualTo(liveBase));
                Assert.That(hitEnd, Is.EqualTo(liveBase));

                AssertPoseWithinBounds(windupMiddle, baseline, ProceduralHumanoidPoseDriver.MaxPerPoseAngleDegrees, "Windup");
                AssertPoseWithinBounds(impactMiddle, baseline, ProceduralHumanoidPoseDriver.MaxPerPoseAngleDegrees, "Impact");
                AssertPoseWithinBounds(recoveryMiddle, baseline, ProceduralHumanoidPoseDriver.MaxPerPoseAngleDegrees, "Recovery");
                AssertPoseWithinBounds(hitMiddle, baseline, ProceduralHumanoidPoseDriver.MaxPerPoseAngleDegrees, "Hit");
                Assert.That(BonePose.Of(root.transform), Is.EqualTo(rootPose));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void CombatSample_CompatibilityAndCustomTuningsUseTheirOwnLegacyPoses()
        {
            var custom = new ProceduralHumanoidMotionTuning(
                5f,
                0f,
                new ProceduralHumanoidLimbPose(4f, 5f, 6f),
                new ProceduralHumanoidLimbPose(7f, 8f, 9f),
                new ProceduralHumanoidLimbPose(10f, 11f, 12f),
                new ProceduralHumanoidLimbPose(13f, 14f, 15f),
                16f,
                -17f,
                new ProceduralHumanoidLimbPose(9f, -7f, 5f, ProceduralHumanoidLocalAxis.Up, ProceduralHumanoidLocalAxis.Up, ProceduralHumanoidLocalAxis.Up),
                new ProceduralHumanoidLimbPose(-8f, 6f, -4f, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward),
                new ProceduralHumanoidLimbPose(3f, -2f, 1f));

            AssertProgressiveCombatMatchesLegacy(ProceduralHumanoidMotionTuning.CompatibilityDefault, out var compatibilityAttack, out var compatibilityHit);
            AssertProgressiveCombatMatchesLegacy(custom, out var customAttack, out var customHit);
            AssertProgressiveCombatMatchesLegacy(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable, out var bloodAttack, out var bloodHit, expectLegacyMatch: false);

            Assert.That(customAttack, Is.Not.EqualTo(compatibilityAttack));
            Assert.That(customHit, Is.Not.EqualTo(compatibilityHit));
            Assert.That(customAttack, Is.Not.EqualTo(bloodAttack), "Custom attack must never inherit Blood Knight globals.");
            Assert.That(customHit, Is.Not.EqualTo(bloodHit), "Custom hit must never inherit Blood Knight globals.");
        }

        [Test]
        public void CharacterSagittalBind_CachesSixDistinctAxesAlignedWithExplicitReference()
        {
            var presentation = CreateSemanticRig(out var bodyRoot, out var bones, out _);
            try
            {
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                var baselineWorldRotations = bones.Select(bone => bone.rotation).ToArray();
                var referenceRight = presentation.transform.right.normalized;

                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                for (var index = 0; index < bones.Length; index++)
                {
                    var cachedAxis = CachedSagittalAxis(driver, index);
                    Assert.That(cachedAxis.sqrMagnitude, Is.EqualTo(1f).Within(0.0001f));
                    var effectiveWorldHinge = (baselineWorldRotations[index] * cachedAxis).normalized;
                    Assert.That(Vector3.Dot(effectiveWorldHinge, referenceRight), Is.GreaterThan(0.9999f), $"Bone {index} must align with +reference.right, not its absolute or reversed axis.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(presentation);
            }
        }

        [Test]
        public void CharacterSagittalLocomotion_StaysInPlaneOpposesSidesAndRestoresAtCycleZero()
        {
            var presentation = CreateSemanticRig(out var bodyRoot, out var bones, out var endpoints);
            try
            {
                var presentationPose = BonePose.Of(presentation.transform);
                var bodyPose = BonePose.Of(bodyRoot);
                var baseline = Snapshot(bones);
                var endpointBaseline = endpoints.Select(endpoint => presentation.transform.InverseTransformPoint(endpoint.position)).ToArray();
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                var locomotion = Input(isLocomoting: true);
                var positiveClock = Mathf.PI * 0.5f / driver.Tuning.SwingCadenceRadiansPerSecond;
                var negativeClock = Mathf.PI * 1.5f / driver.Tuning.SwingCadenceRadiansPerSecond;

                driver.Sample(locomotion, 1f, positiveClock, 0f);
                var positiveEndpoints = endpoints.Select(endpoint => presentation.transform.InverseTransformPoint(endpoint.position)).ToArray();
                AssertSagittalLateralUnchanged(endpointBaseline, positiveEndpoints);
                Assert.That(positiveEndpoints[0].z - endpointBaseline[0].z, Is.LessThan(-0.01f), "Positive left-arm swing must move forward/back, not sideways.");
                Assert.That(positiveEndpoints[1].z - endpointBaseline[1].z, Is.GreaterThan(0.01f));
                Assert.That(positiveEndpoints[2].z - endpointBaseline[2].z, Is.GreaterThan(0.01f), "Left thigh keeps the required opposite sign.");
                Assert.That(positiveEndpoints[3].z - endpointBaseline[3].z, Is.LessThan(-0.01f));

                driver.Sample(locomotion, 1f, negativeClock, 0f);
                var negativeEndpoints = endpoints.Select(endpoint => presentation.transform.InverseTransformPoint(endpoint.position)).ToArray();
                AssertSagittalLateralUnchanged(endpointBaseline, negativeEndpoints);
                Assert.That(negativeEndpoints[0].z - endpointBaseline[0].z, Is.GreaterThan(0.01f));
                Assert.That(negativeEndpoints[1].z - endpointBaseline[1].z, Is.LessThan(-0.01f));
                Assert.That(negativeEndpoints[2].z - endpointBaseline[2].z, Is.LessThan(-0.01f));
                Assert.That(negativeEndpoints[3].z - endpointBaseline[3].z, Is.GreaterThan(0.01f));

                var fullCycleClock = Mathf.PI * 2f / driver.Tuning.SwingCadenceRadiansPerSecond;
                driver.Sample(locomotion, 1f, fullCycleClock, 0f);
                AssertPoseApproximatelyBaseline(Snapshot(bones), baseline, "A complete gait cycle");
                driver.Sample(locomotion, 1f, 0f, 0f);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline), "Returning to cycle zero must exactly restore all six bind baselines.");
                Assert.That(BonePose.Of(presentation.transform), Is.EqualTo(presentationPose));
                Assert.That(BonePose.Of(bodyRoot), Is.EqualTo(bodyPose));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(presentation);
            }
        }

        [Test]
        public void CharacterSagittalJumpAndCombat_UseTheSameSixHingesWithoutTransformDrift()
        {
            var presentation = CreateSemanticRig(out var bodyRoot, out var bones, out _);
            try
            {
                var presentationPose = BonePose.Of(presentation.transform);
                var bodyPose = BonePose.Of(bodyRoot);
                var baseline = Snapshot(bones);
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                var axes = Enumerable.Range(0, bones.Length).Select(index => CachedSagittalAxis(driver, index)).ToArray();

                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Takeoff), 0f, 0f, 0f, 0f);
                AssertSemanticRotations(
                    bones,
                    baseline,
                    axes,
                    new[] { -30f, -30f, -88f, -88f, 80f, 80f },
                    "Deep crouch must keep bilateral jump signs on the shared sagittal hinges.");

                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Takeoff), 0f, 0f, 0f, 1f);
                AssertSemanticRotations(bones, baseline, axes, new[] { -18f, -18f, -22f, 12f, 20f, -10f }, "Asymmetric push");
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Falling), 0f, 0f, 0f, 1f);
                AssertSymmetricSemanticPose(bones, baseline, axes, driver.Tuning.JumpFall, "Fall");
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Landing), 0f, 0f, 0f, 0.5f);
                AssertSymmetricSemanticPose(bones, baseline, axes, driver.Tuning.JumpLand, "Landing");

                var attack = Combat(ProceduralHumanoidAttackStage.Impact, 1f, 1f);
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0f, 0f, 0f, 1f, attack);
                AssertSymmetricSemanticPose(bones, baseline, axes, ProceduralHumanoidMotionTuning.BloodKnightAttackImpact, "Impact");
                driver.Sample(Input(reaction: MotionPresentationReaction.Hit), 0f, 0f, 0f, 1f, Combat(hitProgress: 0.5f, hitWeight: 1f));
                AssertSymmetricSemanticPose(bones, baseline, axes, ProceduralHumanoidMotionTuning.BloodKnightDirectionalHit, "Hit");

                Assert.That(BonePose.Of(presentation.transform), Is.EqualTo(presentationPose));
                Assert.That(BonePose.Of(bodyRoot), Is.EqualTo(bodyPose));
                AssertPoseWithinBounds(Snapshot(bones), baseline, ProceduralHumanoidPoseDriver.MaxPerPoseAngleDegrees, "Semantic combat");
                driver.Clear();
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(presentation);
            }
        }

        [Test]
        public void CharacterSagittalBind_FailsClosedAndSemanticRebindRestoresBothRigs()
        {
            var firstPresentation = CreateSemanticRig(out var firstBody, out var firstBones, out _);
            var secondPresentation = CreateSemanticRig(out var secondBody, out var secondBones, out _);
            try
            {
                var firstBaseline = Snapshot(firstBones);
                var secondBaseline = Snapshot(secondBones);
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);

                Assert.That(driver.Bind(firstBody, null, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.False);
                Assert.That(driver.Bind(firstBody, null, Bip01Map(), ProceduralHumanoidAxisPolicy.LocalBoneAxes), Is.False);
                Assert.That(driver.Bind(firstBody, firstPresentation.transform, Bip01Map(), (ProceduralHumanoidAxisPolicy)99), Is.False);
                firstPresentation.transform.localScale = new Vector3(1f, 0f, 1f);
                Assert.That(driver.Bind(firstBody, firstPresentation.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.False);
                firstPresentation.transform.localScale = Vector3.one;
                firstBody.localScale = new Vector3(-1f, 1f, 1f);
                Assert.That(driver.Bind(firstBody, firstPresentation.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.False);
                firstBody.localScale = Vector3.one;
                firstBones[0].localScale = new Vector3(0f, 1f, 1f);
                Assert.That(driver.Bind(firstBody, firstPresentation.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.False);
                firstBones[0].localScale = firstBaseline[0].Scale;

                Assert.That(driver.Bind(firstBody, firstPresentation.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                driver.Sample(Input(isLocomoting: true), 1f, Mathf.PI * 0.5f / driver.Tuning.SwingCadenceRadiansPerSecond, 0f);
                Assert.That(Snapshot(firstBones), Is.Not.EqualTo(firstBaseline));
                Assert.That(driver.Bind(secondBody, secondPresentation.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                Assert.That(Snapshot(firstBones), Is.EqualTo(firstBaseline));
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Takeoff), 0f, 0f, 0f, 0f);
                driver.Clear();
                Assert.That(Snapshot(secondBones), Is.EqualTo(secondBaseline));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstPresentation);
                UnityEngine.Object.DestroyImmediate(secondPresentation);
            }
        }

        [Test]
        public void AxisPolicy_LegacyBindAndNonBloodProfilesRemainExact()
        {
            var legacyRoot = CreateRig(out var legacyBones);
            var explicitRoot = CreateRig(out var explicitBones);
            var reference = new GameObject("Character Orientation Reference");
            try
            {
                reference.transform.rotation = Quaternion.Euler(7f, 123f, -4f);
                var legacy = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.CompatibilityDefault);
                var explicitLocal = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.CompatibilityDefault);
                Assert.That(legacy.Bind(legacyRoot.transform, Bip01Map()), Is.True);
                Assert.That(explicitLocal.Bind(explicitRoot.transform, reference.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.LocalBoneAxes), Is.True);
                var input = Input(isLocomoting: true);
                legacy.Sample(input, 0.8f, 0.37f, 1f / 60f);
                explicitLocal.Sample(input, 0.8f, 0.37f, 1f / 60f);
                Assert.That(Snapshot(explicitBones), Is.EqualTo(Snapshot(legacyBones)), "Explicit local-axis policy must preserve the existing Bind output exactly.");
                explicitLocal.Clear();

                var semanticCompatibility = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.CompatibilityDefault);
                Assert.That(semanticCompatibility.Bind(explicitRoot.transform, reference.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                semanticCompatibility.Sample(input, 0.8f, 0.37f, 1f / 60f);
                Assert.That(Snapshot(explicitBones), Is.EqualTo(Snapshot(legacyBones)), "Only BloodKnightDeviceReadable opts into semantic presentation fields.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(legacyRoot);
                UnityEngine.Object.DestroyImmediate(explicitRoot);
                UnityEngine.Object.DestroyImmediate(reference);
            }
        }

        [Test]
        public void OptionalUpperTorso_BindsExactlyAndEveryOptionalFailureKeepsSixBoneMotion()
        {
            var presentation = CreateSemanticRig(out var bodyRoot, out var bones, out _, out var upperTorso);
            try
            {
                var limbBaseline = Snapshot(bones);
                var torsoBaseline = BonePose.Of(upperTorso);
                var walkClock = Mathf.PI * 0.5f / ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable.SwingCadenceRadiansPerSecond;
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);

                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01MapWithUpperTorso(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                driver.Sample(Input(isLocomoting: true), 1f, walkClock, 0f);
                Assert.That(Quaternion.Angle(upperTorso.localRotation, torsoBaseline.Rotation), Is.GreaterThan(0.1f));
                driver.Clear();
                Assert.That(BonePose.Of(upperTorso), Is.EqualTo(torsoBaseline));

                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01MapWithUpperTorso("Missing Spine"), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                driver.Sample(Input(isLocomoting: true), 1f, walkClock, 0f);
                Assert.That(BonePose.Of(upperTorso), Is.EqualTo(torsoBaseline));
                Assert.That(Snapshot(bones), Is.Not.EqualTo(limbBaseline), "Missing optional torso must preserve the accepted six-bone motion.");
                driver.Clear();

                var duplicate = CreateChild(bodyRoot, "Bip01 Spine1", Vector3.zero, Quaternion.Euler(11f, 13f, 17f));
                var duplicateBaseline = BonePose.Of(duplicate);
                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01MapWithUpperTorso(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                driver.Sample(Input(isLocomoting: true), 1f, walkClock, 0f);
                Assert.That(BonePose.Of(upperTorso), Is.EqualTo(torsoBaseline));
                Assert.That(BonePose.Of(duplicate), Is.EqualTo(duplicateBaseline));
                Assert.That(Snapshot(bones), Is.Not.EqualTo(limbBaseline));
                driver.Clear();
                UnityEngine.Object.DestroyImmediate(duplicate.gameObject);

                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01MapWithUpperTorso(RequiredNames[0]), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                driver.Sample(Input(isLocomoting: true), 1f, walkClock, 0f);
                Assert.That(BonePose.Of(upperTorso), Is.EqualTo(torsoBaseline), "An optional name colliding with a required limb must disable only the torso extension.");
                driver.Clear();

                upperTorso.localScale = new Vector3(-1f, 1f, 1f);
                var reflectedTorso = BonePose.Of(upperTorso);
                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01MapWithUpperTorso(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                driver.Sample(Input(isLocomoting: true), 1f, walkClock, 0f);
                Assert.That(BonePose.Of(upperTorso), Is.EqualTo(reflectedTorso));
                Assert.That(Snapshot(bones), Is.Not.EqualTo(limbBaseline));
                driver.Clear();
                upperTorso.localScale = torsoBaseline.Scale;

                upperTorso.localScale = new Vector3(1f, 0f, 1f);
                var degenerateTorso = BonePose.Of(upperTorso);
                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01MapWithUpperTorso(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                driver.Sample(Input(isLocomoting: true), 1f, walkClock, 0f);
                Assert.That(BonePose.Of(upperTorso), Is.EqualTo(degenerateTorso));
                Assert.That(Snapshot(bones), Is.Not.EqualTo(limbBaseline));
                driver.Clear();
                upperTorso.localScale = torsoBaseline.Scale;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(presentation);
            }
        }

        [Test]
        public void OptionalUpperTorso_UsesReferenceYawAndBoundedWalkAttackHitSigns()
        {
            var presentation = CreateSemanticRig(out var bodyRoot, out var bones, out _, out var upperTorso);
            try
            {
                var presentationPose = BonePose.Of(presentation.transform);
                var bodyPose = BonePose.Of(bodyRoot);
                var limbBaseline = Snapshot(bones);
                var torsoBaseline = BonePose.Of(upperTorso);
                var torsoWorldBaseline = upperTorso.rotation;
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                Assert.That(driver.Bind(bodyRoot, presentation.transform, Bip01MapWithUpperTorso(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                var torsoAxis = CachedUpperTorsoAxis(driver);
                Assert.That(Vector3.Dot((torsoWorldBaseline * torsoAxis).normalized, presentation.transform.right.normalized), Is.GreaterThan(0.9999f));

                var positiveClock = Mathf.PI * 0.5f / driver.Tuning.SwingCadenceRadiansPerSecond;
                var negativeClock = Mathf.PI * 1.5f / driver.Tuning.SwingCadenceRadiansPerSecond;
                driver.Sample(Input(isLocomoting: true), 1f, positiveClock, 0f);
                AssertUpperTorsoRotation(upperTorso, torsoBaseline, torsoAxis, -2f, 2f, "Positive gait counterweight");
                driver.Sample(Input(isLocomoting: true), 1f, negativeClock, 0f);
                AssertUpperTorsoRotation(upperTorso, torsoBaseline, torsoAxis, 2f, 2f, "Negative gait counterweight");

                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0f, 0f, 0f, 1f, Combat(ProceduralHumanoidAttackStage.Windup, 1f));
                AssertUpperTorsoRotation(upperTorso, torsoBaseline, torsoAxis, -4f, 8f, "Windup");
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0f, 0f, 0f, 1f, Combat(ProceduralHumanoidAttackStage.Impact, 1f));
                AssertUpperTorsoRotation(upperTorso, torsoBaseline, torsoAxis, 8f, 8f, "Impact");
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0f, 0f, 0f, 1f, Combat(ProceduralHumanoidAttackStage.Recovery, 1f));
                Assert.That(BonePose.Of(upperTorso), Is.EqualTo(torsoBaseline));

                driver.Sample(Input(reaction: MotionPresentationReaction.Hit), 0f, 0f, 0f, 1f, Combat(hitProgress: 0.5f, hitWeight: 1f));
                AssertUpperTorsoRotation(upperTorso, torsoBaseline, torsoAxis, -5f, 5f, "Hit recoil");
                Assert.That(BonePose.Of(presentation.transform), Is.EqualTo(presentationPose));
                Assert.That(BonePose.Of(bodyRoot), Is.EqualTo(bodyPose));
                AssertPoseWithinBounds(Snapshot(bones), limbBaseline, ProceduralHumanoidPoseDriver.MaxPerPoseAngleDegrees, "Torso extension limbs");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(presentation);
            }
        }

        [Test]
        public void OptionalUpperTorso_PriorityIdempotenceDenseSparseAndLifecycleAreExact()
        {
            var firstPresentation = CreateSemanticRig(out var firstBody, out _, out _, out var firstTorso);
            var secondPresentation = CreateSemanticRig(out var secondBody, out _, out _, out var secondTorso);
            try
            {
                var firstBaseline = BonePose.Of(firstTorso);
                var secondBaseline = BonePose.Of(secondTorso);
                var driver = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                Assert.That(driver.Bind(firstBody, firstPresentation.transform, Bip01MapWithUpperTorso(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                var axis = CachedUpperTorsoAxis(driver);
                var finalAttack = Combat(ProceduralHumanoidAttackStage.Impact, 0.65f, 1f);

                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0.6f, 0.21f, 0f, 1f, finalAttack);
                var sparse = BonePose.Of(firstTorso);
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0.6f, 0.21f, 0f, 1f, Combat(ProceduralHumanoidAttackStage.Windup, 0.25f, 1f));
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0.6f, 0.21f, 0f, 1f, Combat(ProceduralHumanoidAttackStage.Impact, 0.3f, 1f));
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0.6f, 0.21f, 0f, 1f, finalAttack);
                Assert.That(BonePose.Of(firstTorso), Is.EqualTo(sparse), "Dense and sparse factual sampling must end at the same exact torso pose.");
                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0.6f, 0.21f, 0f, 1f, finalAttack);
                Assert.That(BonePose.Of(firstTorso), Is.EqualTo(sparse), "Repeated attack sampling must be idempotent.");

                var hit = Combat(hitProgress: 0.5f, hitWeight: 1f);
                driver.Sample(Input(reaction: MotionPresentationReaction.Hit, attack: MotionPresentationAttack.Primary), 0f, 0f, 0f, 1f, hit);
                var hitPriority = BonePose.Of(firstTorso);
                AssertUpperTorsoRotation(firstTorso, firstBaseline, axis, -5f, 5f, "Hit priority");
                driver.Sample(Input(reaction: MotionPresentationReaction.Hit, attack: MotionPresentationAttack.Primary), 0f, 0f, 0f, 1f, hit);
                Assert.That(BonePose.Of(firstTorso), Is.EqualTo(hitPriority));
                driver.Sample(Input(reaction: MotionPresentationReaction.Death, attack: MotionPresentationAttack.Primary), 0f, 0f, 0f, 1f, hit);
                Assert.That(BonePose.Of(firstTorso), Is.EqualTo(firstBaseline), "Death has no torso accent and remains above Hit and Attack.");
                driver.Sample(Input(jumpPhase: MotionPresentationJumpPhase.Takeoff), 0f, 0f, 0f, 0f);
                Assert.That(BonePose.Of(firstTorso), Is.EqualTo(firstBaseline), "Jump has no torso accent.");

                driver.Sample(Input(attack: MotionPresentationAttack.Primary), 0f, 0f, 0f, 1f, Combat(ProceduralHumanoidAttackStage.Impact, 1f));
                Assert.That(driver.Bind(secondBody, secondPresentation.transform, Bip01MapWithUpperTorso(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                Assert.That(BonePose.Of(firstTorso), Is.EqualTo(firstBaseline));
                driver.Sample(Input(isLocomoting: true), 1f, Mathf.PI * 0.5f / driver.Tuning.SwingCadenceRadiansPerSecond, 0f);
                driver.Clear();
                Assert.That(BonePose.Of(secondTorso), Is.EqualTo(secondBaseline));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstPresentation);
                UnityEngine.Object.DestroyImmediate(secondPresentation);
            }
        }

        [Test]
        public void OptionalUpperTorso_NeverChangesAcceptedSixBoneOrLegacyOutputs()
        {
            var withoutPresentation = CreateSemanticRig(out var withoutBody, out var withoutBones, out _, out var withoutTorso);
            var withPresentation = CreateSemanticRig(out var withBody, out var withBones, out _, out var withTorso);
            var legacyRoot = CreateRig(out var legacyBones);
            try
            {
                var withoutTorsoBaseline = BonePose.Of(withoutTorso);
                var legacyBaseline = Snapshot(legacyBones);
                var without = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                var with = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                var legacy = new ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
                Assert.That(without.Bind(withoutBody, withoutPresentation.transform, Bip01Map(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                Assert.That(with.Bind(withBody, withPresentation.transform, Bip01MapWithUpperTorso(), ProceduralHumanoidAxisPolicy.CharacterSagittalPlane), Is.True);
                Assert.That(legacy.Bind(legacyRoot.transform, Bip01Map()), Is.True);

                var walkClock = Mathf.PI * 0.5f / with.Tuning.SwingCadenceRadiansPerSecond;
                without.Sample(Input(isLocomoting: true), 0.8f, walkClock, 0f);
                with.Sample(Input(isLocomoting: true), 0.8f, walkClock, 0f);
                Assert.That(Snapshot(withBones), Is.EqualTo(Snapshot(withoutBones)));
                Assert.That(BonePose.Of(withoutTorso), Is.EqualTo(withoutTorsoBaseline));

                var attack = Combat(ProceduralHumanoidAttackStage.Impact, 0.7f, 1f, 1f);
                without.Sample(Input(attack: MotionPresentationAttack.Primary), 0.8f, walkClock, 0f, 1f, attack);
                with.Sample(Input(attack: MotionPresentationAttack.Primary), 0.8f, walkClock, 0f, 1f, attack);
                Assert.That(Snapshot(withBones), Is.EqualTo(Snapshot(withoutBones)));
                Assert.That(Quaternion.Angle(withTorso.localRotation, withoutTorso.localRotation), Is.GreaterThan(0.1f));

                legacy.Sample(Input(isLocomoting: true), 1f, 0.2f, 1f / 60f);
                var firstLegacy = Snapshot(legacyBones);
                legacy.Sample(Input(isLocomoting: true), 1f, 0.2f, 1f / 60f);
                Assert.That(Snapshot(legacyBones), Is.EqualTo(firstLegacy));
                Assert.That(AllBoundsRespected(legacyBones, legacyBaseline, 60f), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(withoutPresentation);
                UnityEngine.Object.DestroyImmediate(withPresentation);
                UnityEngine.Object.DestroyImmediate(legacyRoot);
            }
        }

        [Test]
        public void Contracts_AreImmutableAndDoNotReferenceGameplayAssembly()
        {
            Assert.That(typeof(HumanoidBoneNameMap).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ProceduralHumanoidCombatPoseSample).GetProperties(BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite), Is.True);
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

        private static ProceduralHumanoidCombatPoseSample Combat(
            ProceduralHumanoidAttackStage stage = ProceduralHumanoidAttackStage.Windup,
            float attackProgress = 0f,
            float attackBlend = 1f,
            float attackDirection = 0f,
            float hitProgress = 0f,
            float hitWeight = 0f,
            float recoilDirection = 0f)
        {
            return new ProceduralHumanoidCombatPoseSample(
                stage,
                attackProgress,
                attackBlend,
                attackDirection,
                hitProgress,
                hitWeight,
                recoilDirection);
        }

        private static void AssertIntermediate(BonePose[] start, BonePose[] middle, BonePose[] end, string label)
        {
            Assert.That(middle, Is.Not.EqualTo(start), $"{label} midpoint must differ from its start.");
            Assert.That(middle, Is.Not.EqualTo(end), $"{label} midpoint must differ from its end.");
        }

        private static void AssertPoseWithinBounds(BonePose[] pose, BonePose[] baseline, float maximum, string label)
        {
            for (var index = 0; index < pose.Length; index++)
            {
                Assert.That(Quaternion.Angle(pose[index].Rotation, baseline[index].Rotation), Is.LessThanOrEqualTo(maximum + 0.001f), $"{label} bone {index} exceeded its additive bound.");
                Assert.That(pose[index].Position, Is.EqualTo(baseline[index].Position), $"{label} moved bone {index}.");
                Assert.That(pose[index].Scale, Is.EqualTo(baseline[index].Scale), $"{label} scaled bone {index}.");
            }
        }

        private static void AssertPoseApproximatelyBaseline(BonePose[] pose, BonePose[] baseline, string label)
        {
            for (var index = 0; index < pose.Length; index++)
            {
                Assert.That(Quaternion.Angle(pose[index].Rotation, baseline[index].Rotation), Is.LessThan(0.001f), $"{label} did not restore bone {index} rotation.");
                Assert.That(pose[index].Position, Is.EqualTo(baseline[index].Position));
                Assert.That(pose[index].Scale, Is.EqualTo(baseline[index].Scale));
            }
        }

        private static void AssertProgressiveCombatMatchesLegacy(
            ProceduralHumanoidMotionTuning tuning,
            out BonePose[] progressiveAttack,
            out BonePose[] progressiveHit,
            bool expectLegacyMatch = true)
        {
            var root = CreateRig(out var bones);
            try
            {
                var rootPose = BonePose.Of(root.transform);
                var driver = new ProceduralHumanoidPoseDriver(tuning);
                Assert.That(driver.Bind(root.transform, Bip01Map()), Is.True);

                var attack = Input(attack: MotionPresentationAttack.Ability);
                driver.Sample(attack, 0f, 0f, 0f);
                var legacyAttack = Snapshot(bones);
                driver.Sample(attack, 0f, 0f, 0f, 1f, Combat(ProceduralHumanoidAttackStage.Impact, 1f));
                progressiveAttack = Snapshot(bones);

                var hit = Input(reaction: MotionPresentationReaction.Hit);
                driver.Sample(hit, 0f, 0f, 0f);
                var legacyHit = Snapshot(bones);
                driver.Sample(hit, 0f, 0f, 0f, 1f, Combat(hitProgress: 0.5f, hitWeight: 1f));
                progressiveHit = Snapshot(bones);

                if (expectLegacyMatch)
                {
                    Assert.That(progressiveAttack, Is.EqualTo(legacyAttack), "Non-Blood progressive attack must use the selected tuning's own AbilityAttack pose.");
                    Assert.That(progressiveHit, Is.EqualTo(legacyHit), "Non-Blood progressive hit must use the selected tuning's own Hit pose.");
                }
                else
                {
                    Assert.That(progressiveAttack, Is.Not.EqualTo(legacyAttack));
                    Assert.That(progressiveHit, Is.Not.EqualTo(legacyHit));
                }

                Assert.That(BonePose.Of(root.transform), Is.EqualTo(rootPose));
                driver.Clear();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Vector3 CachedSagittalAxis(ProceduralHumanoidPoseDriver driver, int boneIndex)
        {
            var field = typeof(ProceduralHumanoidPoseDriver).GetField(SagittalAxisFieldNames[boneIndex], BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (Vector3)field.GetValue(driver);
        }

        private static Vector3 CachedUpperTorsoAxis(ProceduralHumanoidPoseDriver driver)
        {
            var field = typeof(ProceduralHumanoidPoseDriver).GetField("upperTorsoSagittalAxis", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (Vector3)field.GetValue(driver);
        }

        private static void AssertUpperTorsoRotation(
            Transform upperTorso,
            BonePose baseline,
            Vector3 axis,
            float degrees,
            float maximum,
            string label)
        {
            Assert.That(Mathf.Abs(degrees), Is.LessThanOrEqualTo(maximum + 0.001f), $"{label} exceeded its torso bound.");
            var expected = baseline.Rotation * Quaternion.AngleAxis(degrees, axis);
            Assert.That(Quaternion.Angle(upperTorso.localRotation, expected), Is.LessThan(0.001f), $"{label} used the wrong bind-time axis or sign.");
            Assert.That(upperTorso.localPosition, Is.EqualTo(baseline.Position), $"{label} moved the torso.");
            Assert.That(upperTorso.localScale, Is.EqualTo(baseline.Scale), $"{label} scaled the torso.");
        }

        private static void AssertSagittalLateralUnchanged(Vector3[] baseline, Vector3[] sampled)
        {
            for (var index = 0; index < baseline.Length; index++)
                Assert.That(sampled[index].x, Is.EqualTo(baseline[index].x).Within(0.0001f), $"Endpoint {index} left the character sagittal plane.");
        }

        private static void AssertSemanticRotations(
            Transform[] bones,
            BonePose[] baseline,
            Vector3[] axes,
            float[] degrees,
            string label)
        {
            for (var index = 0; index < bones.Length; index++)
            {
                var expected = baseline[index].Rotation * Quaternion.AngleAxis(degrees[index], axes[index]);
                Assert.That(Quaternion.Angle(bones[index].localRotation, expected), Is.LessThan(0.001f), $"{label} bone {index} used the wrong semantic hinge or sign.");
                Assert.That(bones[index].localPosition, Is.EqualTo(baseline[index].Position));
                Assert.That(bones[index].localScale, Is.EqualTo(baseline[index].Scale));
            }
        }

        private static void AssertSymmetricSemanticPose(
            Transform[] bones,
            BonePose[] baseline,
            Vector3[] axes,
            ProceduralHumanoidLimbPose pose,
            string label)
        {
            AssertSemanticRotations(
                bones,
                baseline,
                axes,
                new[]
                {
                    pose.UpperArmDegrees, pose.UpperArmDegrees,
                    pose.ThighDegrees, pose.ThighDegrees,
                    pose.CalfDegrees, pose.CalfDegrees
                },
                label);
        }

        private static HumanoidBoneNameMap Bip01Map()
        {
            return new HumanoidBoneNameMap(
                RequiredNames[0], RequiredNames[1], RequiredNames[2],
                RequiredNames[3], RequiredNames[4], RequiredNames[5]);
        }

        private static HumanoidBoneNameMap Bip01MapWithUpperTorso(string optionalUpperTorso = "Bip01 Spine1")
        {
            return new HumanoidBoneNameMap(
                RequiredNames[0], RequiredNames[1], RequiredNames[2],
                RequiredNames[3], RequiredNames[4], RequiredNames[5], optionalUpperTorso);
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

        private static GameObject CreateSemanticRig(out Transform bodyRoot, out Transform[] bones, out Transform[] endpoints)
        {
            return CreateSemanticRig(out bodyRoot, out bones, out endpoints, out _);
        }

        private static GameObject CreateSemanticRig(out Transform bodyRoot, out Transform[] bones, out Transform[] endpoints, out Transform upperTorso)
        {
            var presentation = new GameObject("Presentation Pivot");
            presentation.transform.position = new Vector3(3f, 2f, -5f);
            presentation.transform.rotation = Quaternion.Euler(7f, 37f, -4f);

            bodyRoot = new GameObject("Base Body").transform;
            bodyRoot.SetParent(presentation.transform, false);
            bodyRoot.localPosition = new Vector3(0.4f, 0.2f, -0.3f);
            bodyRoot.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var leftShoulder = CreateChild(bodyRoot, "Left Shoulder Parent", new Vector3(-0.35f, 1.35f, 0f), Quaternion.Euler(3f, -5f, 2f));
            var rightShoulder = CreateChild(bodyRoot, "Right Shoulder Parent", new Vector3(0.35f, 1.35f, 0f), Quaternion.Euler(-2f, 6f, -3f));
            var pelvis = CreateChild(bodyRoot, "Rotated Pelvis Parent", new Vector3(0f, 0.75f, 0f), Quaternion.Euler(2f, 4f, -1f));
            var spine = CreateChild(pelvis, "Bip01 Spine", new Vector3(0f, 0.2f, 0f), Quaternion.Euler(3f, -6f, 2f));
            upperTorso = CreateChild(spine, "Bip01 Spine1", new Vector3(0f, 0.45f, 0f), Quaternion.Euler(-4f, 7f, -3f));
            CreateChild(upperTorso, "Bip01 Neck", new Vector3(0f, 0.35f, 0f), Quaternion.Euler(2f, -2f, 1f));

            bones = new Transform[RequiredNames.Length];
            bones[0] = CreateChild(leftShoulder, RequiredNames[0], Vector3.zero, Quaternion.Euler(4f, -7f, 5f));
            bones[1] = CreateChild(rightShoulder, RequiredNames[1], Vector3.zero, Quaternion.Euler(-5f, 8f, -4f));
            bones[2] = CreateChild(pelvis, RequiredNames[2], new Vector3(-0.2f, 0f, 0f), Quaternion.Euler(3f, -4f, 2f));
            bones[3] = CreateChild(pelvis, RequiredNames[3], new Vector3(0.2f, 0f, 0f), Quaternion.Euler(-4f, 5f, -2f));
            bones[4] = CreateChild(bones[2], RequiredNames[4], new Vector3(0f, -0.8f, 0f), Quaternion.Euler(5f, 3f, -2f));
            bones[5] = CreateChild(bones[3], RequiredNames[5], new Vector3(0f, -0.8f, 0f), Quaternion.Euler(-6f, -3f, 2f));

            var leftArmTip = CreateChild(bones[0], "Left Arm Tip", new Vector3(0f, -0.7f, 0f), Quaternion.identity);
            var rightArmTip = CreateChild(bones[1], "Right Arm Tip", new Vector3(0f, -0.7f, 0f), Quaternion.identity);
            var leftFootTip = CreateChild(bones[4], "Left Foot Tip", new Vector3(0f, -0.65f, 0f), Quaternion.identity);
            var rightFootTip = CreateChild(bones[5], "Right Foot Tip", new Vector3(0f, -0.65f, 0f), Quaternion.identity);
            endpoints = new[] { leftArmTip, rightArmTip, bones[4], bones[5], leftFootTip, rightFootTip };
            return presentation;
        }

        private static Transform CreateChild(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            child.localRotation = localRotation;
            return child;
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
