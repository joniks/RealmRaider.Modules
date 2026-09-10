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

        public bool IsBound => visualRoot != null;

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
            if (!IsBound)
                return;

            RestoreBaselines();
            var speed = IsFinite(normalizedLocomotionSpeed) ? Mathf.Clamp01(normalizedLocomotionSpeed) : 0f;
            var clock = IsFinite(presentationClock) ? presentationClock : 0f;
            var safeDelta = IsFinite(deltaTime) ? Mathf.Clamp(deltaTime, 0f, 0.1f) : 0f;
            var swing = Mathf.Sin((clock + safeDelta) * 6f);

            switch (CharacterMotionPresentationResolver.Resolve(input))
            {
                case MotionClipKey.Idle:
                    ApplyIdle(swing);
                    break;
                case MotionClipKey.Locomotion:
                    ApplyLocomotion(swing, speed);
                    break;
                case MotionClipKey.JumpTakeoff:
                    ApplyJumpTakeoff();
                    break;
                case MotionClipKey.JumpFall:
                    ApplyJumpFall();
                    break;
                case MotionClipKey.JumpLand:
                    ApplyJumpLand();
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
            AddRotation(leftUpperArm, leftUpperArmBaseline, Vector3.forward, 2f * swing);
            AddRotation(rightUpperArm, rightUpperArmBaseline, Vector3.forward, -2f * swing);
        }

        private void ApplyLocomotion(float swing, float speed)
        {
            var arm = 22f * speed * swing;
            var thigh = 18f * speed * swing;
            AddRotation(leftUpperArm, leftUpperArmBaseline, Vector3.right, arm);
            AddRotation(rightUpperArm, rightUpperArmBaseline, Vector3.right, -arm);
            AddRotation(leftThigh, leftThighBaseline, Vector3.right, -thigh);
            AddRotation(rightThigh, rightThighBaseline, Vector3.right, thigh);
            AddRotation(leftCalf, leftCalfBaseline, Vector3.right, 0.35f * thigh);
            AddRotation(rightCalf, rightCalfBaseline, Vector3.right, -0.35f * thigh);
        }

        private void ApplyJumpTakeoff()
        {
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, -12f);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, -10f);
            ApplyPair(leftCalf, leftCalfBaseline, rightCalf, rightCalfBaseline, 8f);
        }

        private void ApplyJumpFall()
        {
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, 16f);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, 8f);
        }

        private void ApplyJumpLand()
        {
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, -6f);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, -16f);
            ApplyPair(leftCalf, leftCalfBaseline, rightCalf, rightCalfBaseline, 18f);
        }

        private void ApplyPrimaryAttack()
        {
            AddRotation(rightUpperArm, rightUpperArmBaseline, Vector3.right, -28f);
            AddRotation(leftUpperArm, leftUpperArmBaseline, Vector3.right, 8f);
        }

        private void ApplyAbilityAttack()
        {
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, -22f);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, 8f);
        }

        private void ApplyHit()
        {
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, 10f);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, -6f);
        }

        private void ApplyDeathSettle()
        {
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, 12f);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, -8f);
        }

        private static void ApplyPair(
            Transform left, Quaternion leftBaseline, Transform right, Quaternion rightBaseline, float degrees)
        {
            AddRotation(left, leftBaseline, Vector3.right, degrees);
            AddRotation(right, rightBaseline, Vector3.right, degrees);
        }

        private static void AddRotation(Transform target, Quaternion baseline, Vector3 axis, float degrees)
        {
            target.localRotation = baseline * Quaternion.AngleAxis(
                Mathf.Clamp(degrees, -MaxAdditiveAngleDegrees, MaxAdditiveAngleDegrees), axis);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
