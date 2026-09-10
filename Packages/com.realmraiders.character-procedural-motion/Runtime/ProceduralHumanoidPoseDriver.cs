using UnityEngine;
using RealmRaiders.Modules.CharacterMotionProfiles;

namespace RealmRaiders.Modules.CharacterProceduralMotion
{
    /// <summary>
    /// Caller-driven, bounded, additive local-bone presentation only. This type has no
    /// MonoBehaviour lifecycle, scene search, Animator, root-motion, physics, or gameplay authority.
    /// </summary>
    public sealed class ProceduralHumanoidPoseDriver
    {
        public const float MaxAdditiveAngleDegrees = 30f;
        public const float MaxPerPoseAngleDegrees = 90f;

        private Transform visualRoot;
        private Transform leftUpperArm;
        private Transform rightUpperArm;
        private Transform leftThigh;
        private Transform rightThigh;
        private Transform leftCalf;
        private Transform rightCalf;
        private Quaternion leftUpperArmBaseline;
        private Quaternion rightUpperArmBaseline;
        private Quaternion leftThighBaseline;
        private Quaternion rightThighBaseline;
        private Quaternion leftCalfBaseline;
        private Quaternion rightCalfBaseline;
        private Vector3 leftUpperArmPositionBaseline;
        private Vector3 rightUpperArmPositionBaseline;
        private Vector3 leftThighPositionBaseline;
        private Vector3 rightThighPositionBaseline;
        private Vector3 leftCalfPositionBaseline;
        private Vector3 rightCalfPositionBaseline;
        private Vector3 leftUpperArmScaleBaseline;
        private Vector3 rightUpperArmScaleBaseline;
        private Vector3 leftThighScaleBaseline;
        private Vector3 rightThighScaleBaseline;
        private Vector3 leftCalfScaleBaseline;
        private Vector3 rightCalfScaleBaseline;
        private readonly ProceduralHumanoidMotionTuning tuning;

        public bool IsBound => visualRoot != null;
        public ProceduralHumanoidMotionTuning Tuning => tuning;

        public ProceduralHumanoidPoseDriver(ProceduralHumanoidMotionTuning tuning = null)
        {
            this.tuning = tuning ?? ProceduralHumanoidMotionTuning.CompatibilityDefault;
        }

        /// <summary>Restores any prior binding before attempting a new exact local bind.</summary>
        public bool Bind(Transform suppliedVisualRoot, HumanoidBoneNameMap names)
        {
            Clear();
            if (suppliedVisualRoot == null || names == null || !names.IsValid())
                return false;

            var descendants = suppliedVisualRoot.GetComponentsInChildren<Transform>(true);
            Transform foundLeftUpperArm = null;
            Transform foundRightUpperArm = null;
            Transform foundLeftThigh = null;
            Transform foundRightThigh = null;
            Transform foundLeftCalf = null;
            Transform foundRightCalf = null;
            for (var index = 0; index < descendants.Length; index++)
            {
                var candidate = descendants[index];
                if (candidate == suppliedVisualRoot)
                    continue;
                if (!TryAssign(candidate, names.LeftUpperArm, ref foundLeftUpperArm) ||
                    !TryAssign(candidate, names.RightUpperArm, ref foundRightUpperArm) ||
                    !TryAssign(candidate, names.LeftThigh, ref foundLeftThigh) ||
                    !TryAssign(candidate, names.RightThigh, ref foundRightThigh) ||
                    !TryAssign(candidate, names.LeftCalf, ref foundLeftCalf) ||
                    !TryAssign(candidate, names.RightCalf, ref foundRightCalf))
                    return false;
            }

            if (foundLeftUpperArm == null || foundRightUpperArm == null ||
                foundLeftThigh == null || foundRightThigh == null ||
                foundLeftCalf == null || foundRightCalf == null)
                return false;

            visualRoot = suppliedVisualRoot;
            leftUpperArm = foundLeftUpperArm;
            rightUpperArm = foundRightUpperArm;
            leftThigh = foundLeftThigh;
            rightThigh = foundRightThigh;
            leftCalf = foundLeftCalf;
            rightCalf = foundRightCalf;
            CaptureBaselines();
            return true;
        }

        /// <summary>Samples caller-supplied presentation facts without allocating or moving any root.</summary>
        public void Sample(
            CharacterMotionPresentationInput input,
            float normalizedLocomotionSpeed,
            float presentationClock,
            float deltaTime)
        {
            SampleInternal(input, normalizedLocomotionSpeed, presentationClock, deltaTime, 1f, true);
        }

        /// <summary>Samples caller-owned normalized jump progress; this driver has no time authority.</summary>
        public void Sample(
            CharacterMotionPresentationInput input,
            float normalizedLocomotionSpeed,
            float presentationClock,
            float deltaTime,
            float normalizedJumpPresentationProgress)
        {
            SampleInternal(input, normalizedLocomotionSpeed, presentationClock, deltaTime, normalizedJumpPresentationProgress, false);
        }

        private void SampleInternal(
            CharacterMotionPresentationInput input,
            float normalizedLocomotionSpeed,
            float presentationClock,
            float deltaTime,
            float normalizedJumpPresentationProgress,
            bool useLegacyStaticJumpPose)
        {
            if (!IsBound)
                return;

            RestoreBaselines();
            var speed = IsFinite(normalizedLocomotionSpeed) ? Mathf.Clamp01(normalizedLocomotionSpeed) : 0f;
            var clock = IsFinite(presentationClock) ? presentationClock : 0f;
            var safeDelta = IsFinite(deltaTime) ? Mathf.Clamp(deltaTime, 0f, 0.1f) : 0f;
            var jumpProgress = IsFinite(normalizedJumpPresentationProgress) ? Mathf.Clamp01(normalizedJumpPresentationProgress) : 0f;
            var swing = Mathf.Sin((clock + safeDelta) * tuning.SwingCadenceRadiansPerSecond);

            switch (CharacterMotionPresentationResolver.Resolve(input))
            {
                case MotionClipKey.Idle:
                    ApplyIdle(swing);
                    break;
                case MotionClipKey.Locomotion:
                    ApplyLocomotion(swing, speed);
                    break;
                case MotionClipKey.JumpTakeoff:
                    ApplyJumpTakeoff(jumpProgress);
                    break;
                case MotionClipKey.JumpFall:
                    if (useLegacyStaticJumpPose) ApplyPose(tuning.JumpFall); else ApplyJumpFall(jumpProgress);
                    break;
                case MotionClipKey.JumpLand:
                    if (useLegacyStaticJumpPose) ApplyPose(tuning.JumpLand); else ApplyJumpLand(jumpProgress);
                    break;
                case MotionClipKey.AttackPrimary:
                    ApplyPrimaryAttack();
                    break;
                case MotionClipKey.AttackAbility:
                    ApplyAbilityAttack();
                    break;
                case MotionClipKey.Hit:
                    ApplyHit();
                    break;
                case MotionClipKey.Death:
                    ApplyDeathSettle();
                    break;
            }
        }

        /// <summary>Exactly restores cached local rotations and drops every cached transform.</summary>
        public void Clear()
        {
            if (IsBound)
                RestoreBaselines();
            visualRoot = null;
            leftUpperArm = null;
            rightUpperArm = null;
            leftThigh = null;
            rightThigh = null;
            leftCalf = null;
            rightCalf = null;
        }

        private static bool TryAssign(Transform candidate, string requiredName, ref Transform assigned)
        {
            if (!string.Equals(candidate.name, requiredName, System.StringComparison.Ordinal))
                return true;
            if (assigned != null)
                return false;
            assigned = candidate;
            return true;
        }

        private void CaptureBaselines()
        {
            leftUpperArmBaseline = leftUpperArm.localRotation;
            rightUpperArmBaseline = rightUpperArm.localRotation;
            leftThighBaseline = leftThigh.localRotation;
            rightThighBaseline = rightThigh.localRotation;
            leftCalfBaseline = leftCalf.localRotation;
            rightCalfBaseline = rightCalf.localRotation;
            leftUpperArmPositionBaseline = leftUpperArm.localPosition;
            rightUpperArmPositionBaseline = rightUpperArm.localPosition;
            leftThighPositionBaseline = leftThigh.localPosition;
            rightThighPositionBaseline = rightThigh.localPosition;
            leftCalfPositionBaseline = leftCalf.localPosition;
            rightCalfPositionBaseline = rightCalf.localPosition;
            leftUpperArmScaleBaseline = leftUpperArm.localScale;
            rightUpperArmScaleBaseline = rightUpperArm.localScale;
            leftThighScaleBaseline = leftThigh.localScale;
            rightThighScaleBaseline = rightThigh.localScale;
            leftCalfScaleBaseline = leftCalf.localScale;
            rightCalfScaleBaseline = rightCalf.localScale;
        }

        private void RestoreBaselines()
        {
            leftUpperArm.localRotation = leftUpperArmBaseline;
            rightUpperArm.localRotation = rightUpperArmBaseline;
            leftThigh.localRotation = leftThighBaseline;
            rightThigh.localRotation = rightThighBaseline;
            leftCalf.localRotation = leftCalfBaseline;
            rightCalf.localRotation = rightCalfBaseline;
            leftUpperArm.localPosition = leftUpperArmPositionBaseline;
            rightUpperArm.localPosition = rightUpperArmPositionBaseline;
            leftThigh.localPosition = leftThighPositionBaseline;
            rightThigh.localPosition = rightThighPositionBaseline;
            leftCalf.localPosition = leftCalfPositionBaseline;
            rightCalf.localPosition = rightCalfPositionBaseline;
            leftUpperArm.localScale = leftUpperArmScaleBaseline;
            rightUpperArm.localScale = rightUpperArmScaleBaseline;
            leftThigh.localScale = leftThighScaleBaseline;
            rightThigh.localScale = rightThighScaleBaseline;
            leftCalf.localScale = leftCalfScaleBaseline;
            rightCalf.localScale = rightCalfScaleBaseline;
        }

        private void ApplyIdle(float swing)
        {
            AddRotation(leftUpperArm, leftUpperArmBaseline, Vector3.forward, tuning.IdleArmDegrees * swing, MaxAdditiveAngleDegrees);
            AddRotation(rightUpperArm, rightUpperArmBaseline, Vector3.forward, -tuning.IdleArmDegrees * swing, MaxAdditiveAngleDegrees);
        }

        private void ApplyLocomotion(float swing, float speed)
        {
            var arm = tuning.Locomotion.UpperArmDegrees * speed * swing;
            var thigh = tuning.Locomotion.ThighDegrees * speed * swing;
            var calf = tuning.Locomotion.CalfDegrees * speed * swing;
            AddRotation(leftUpperArm, leftUpperArmBaseline, Axis(tuning.Locomotion.UpperArmAxis), arm, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(rightUpperArm, rightUpperArmBaseline, Axis(tuning.Locomotion.UpperArmAxis), -arm, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(leftThigh, leftThighBaseline, Axis(tuning.Locomotion.ThighAxis), -thigh, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(rightThigh, rightThighBaseline, Axis(tuning.Locomotion.ThighAxis), thigh, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(leftCalf, leftCalfBaseline, Axis(tuning.Locomotion.CalfAxis), calf, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(rightCalf, rightCalfBaseline, Axis(tuning.Locomotion.CalfAxis), -calf, tuning.Locomotion.MaxAdditiveAngleDegrees);
        }

        private void ApplyJumpTakeoff(float progress)
        {
            var crouch = tuning.DeepCrouch;
            var push = tuning.AsymmetricJumpTakeoff;
            ApplyTakeoff(progress, crouch, push);
        }

        private void ApplyJumpFall(float progress)
        {
            ApplyTakeoffToPose(progress, tuning.AsymmetricJumpTakeoff, tuning.JumpFall);
        }

        private void ApplyJumpLand(float progress)
        {
            if (progress <= 0.5f)
                ApplyPoseTransition(tuning.JumpFall, tuning.JumpLand, progress * 2f);
            else
                ApplyPose(tuning.JumpLand, 2f - progress * 2f);
        }

        private void ApplyPrimaryAttack()
        {
            AddRotation(rightUpperArm, rightUpperArmBaseline, Vector3.right, tuning.PrimaryWeaponArmDegrees, MaxAdditiveAngleDegrees);
            AddRotation(leftUpperArm, leftUpperArmBaseline, Vector3.right, tuning.PrimarySupportArmDegrees, MaxAdditiveAngleDegrees);
        }

        private void ApplyAbilityAttack()
        {
            ApplyPose(tuning.AbilityAttack);
        }

        private void ApplyHit()
        {
            ApplyPose(tuning.Hit);
        }

        private void ApplyDeathSettle()
        {
            ApplyPose(tuning.Death);
        }

        private void ApplyTakeoff(float progress, ProceduralHumanoidTakeoffPose crouch, ProceduralHumanoidTakeoffPose push)
        {
            var maximum = Mathf.Lerp(crouch.MaxAdditiveAngleDegrees, push.MaxAdditiveAngleDegrees, progress);
            var usePushAxis = progress >= 0.5f;
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, Mathf.Lerp(crouch.UpperArmDegrees, push.UpperArmDegrees, progress), usePushAxis ? push.UpperArmAxis : crouch.UpperArmAxis, maximum);
            AddRotation(leftThigh, leftThighBaseline, Axis(usePushAxis ? push.LeftThighAxis : crouch.LeftThighAxis), Mathf.Lerp(crouch.LeftThighDegrees, push.LeftThighDegrees, progress), maximum);
            AddRotation(rightThigh, rightThighBaseline, Axis(usePushAxis ? push.RightThighAxis : crouch.RightThighAxis), Mathf.Lerp(crouch.RightThighDegrees, push.RightThighDegrees, progress), maximum);
            AddRotation(leftCalf, leftCalfBaseline, Axis(usePushAxis ? push.LeftCalfAxis : crouch.LeftCalfAxis), Mathf.Lerp(crouch.LeftCalfDegrees, push.LeftCalfDegrees, progress), maximum);
            AddRotation(rightCalf, rightCalfBaseline, Axis(usePushAxis ? push.RightCalfAxis : crouch.RightCalfAxis), Mathf.Lerp(crouch.RightCalfDegrees, push.RightCalfDegrees, progress), maximum);
        }

        private void ApplyTakeoffToPose(float progress, ProceduralHumanoidTakeoffPose start, ProceduralHumanoidLimbPose end)
        {
            var maximum = Mathf.Lerp(start.MaxAdditiveAngleDegrees, end.MaxAdditiveAngleDegrees, progress);
            var useEndAxis = progress >= 0.5f;
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, Mathf.Lerp(start.UpperArmDegrees, end.UpperArmDegrees, progress), useEndAxis ? end.UpperArmAxis : start.UpperArmAxis, maximum);
            AddRotation(leftThigh, leftThighBaseline, Axis(useEndAxis ? end.ThighAxis : start.LeftThighAxis), Mathf.Lerp(start.LeftThighDegrees, end.ThighDegrees, progress), maximum);
            AddRotation(rightThigh, rightThighBaseline, Axis(useEndAxis ? end.ThighAxis : start.RightThighAxis), Mathf.Lerp(start.RightThighDegrees, end.ThighDegrees, progress), maximum);
            AddRotation(leftCalf, leftCalfBaseline, Axis(useEndAxis ? end.CalfAxis : start.LeftCalfAxis), Mathf.Lerp(start.LeftCalfDegrees, end.CalfDegrees, progress), maximum);
            AddRotation(rightCalf, rightCalfBaseline, Axis(useEndAxis ? end.CalfAxis : start.RightCalfAxis), Mathf.Lerp(start.RightCalfDegrees, end.CalfDegrees, progress), maximum);
        }

        private void ApplyPoseTransition(ProceduralHumanoidLimbPose start, ProceduralHumanoidLimbPose end, float progress)
        {
            var maximum = Mathf.Lerp(start.MaxAdditiveAngleDegrees, end.MaxAdditiveAngleDegrees, progress);
            var useEndAxis = progress >= 0.5f;
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, Mathf.Lerp(start.UpperArmDegrees, end.UpperArmDegrees, progress), useEndAxis ? end.UpperArmAxis : start.UpperArmAxis, maximum);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, Mathf.Lerp(start.ThighDegrees, end.ThighDegrees, progress), useEndAxis ? end.ThighAxis : start.ThighAxis, maximum);
            ApplyPair(leftCalf, leftCalfBaseline, rightCalf, rightCalfBaseline, Mathf.Lerp(start.CalfDegrees, end.CalfDegrees, progress), useEndAxis ? end.CalfAxis : start.CalfAxis, maximum);
        }

        private void ApplyPose(ProceduralHumanoidLimbPose pose, float weight = 1f)
        {
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, pose.UpperArmDegrees * weight, pose.UpperArmAxis, pose.MaxAdditiveAngleDegrees);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, pose.ThighDegrees * weight, pose.ThighAxis, pose.MaxAdditiveAngleDegrees);
            ApplyPair(leftCalf, leftCalfBaseline, rightCalf, rightCalfBaseline, pose.CalfDegrees * weight, pose.CalfAxis, pose.MaxAdditiveAngleDegrees);
        }

        private static void ApplyPair(
            Transform left,
            Quaternion leftBaseline,
            Transform right,
            Quaternion rightBaseline,
            float degrees,
            ProceduralHumanoidLocalAxis axis,
            float maximum)
        {
            var vector = Axis(axis);
            AddRotation(left, leftBaseline, vector, degrees, maximum);
            AddRotation(right, rightBaseline, vector, degrees, maximum);
        }

        private static Vector3 Axis(ProceduralHumanoidLocalAxis axis)
        {
            switch (axis)
            {
                case ProceduralHumanoidLocalAxis.Up:
                    return Vector3.up;
                case ProceduralHumanoidLocalAxis.Forward:
                    return Vector3.forward;
                default:
                    return Vector3.right;
            }
        }

        private static void AddRotation(Transform target, Quaternion baseline, Vector3 axis, float degrees, float maximum)
        {
            target.localRotation = baseline * Quaternion.AngleAxis(
                Mathf.Clamp(degrees, -maximum, maximum), axis);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
