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
        private Transform upperTorso;
        private Quaternion leftUpperArmBaseline;
        private Quaternion rightUpperArmBaseline;
        private Quaternion leftThighBaseline;
        private Quaternion rightThighBaseline;
        private Quaternion leftCalfBaseline;
        private Quaternion rightCalfBaseline;
        private Quaternion upperTorsoBaseline;
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
        private Vector3 leftUpperArmSagittalAxis;
        private Vector3 rightUpperArmSagittalAxis;
        private Vector3 leftThighSagittalAxis;
        private Vector3 rightThighSagittalAxis;
        private Vector3 leftCalfSagittalAxis;
        private Vector3 rightCalfSagittalAxis;
        private Vector3 upperTorsoSagittalAxis;
        private bool usesCharacterSagittalPlane;
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
            return BindInternal(suppliedVisualRoot, null, names, ProceduralHumanoidAxisPolicy.LocalBoneAxes);
        }

        /// <summary>Binds an explicit character-oriented reference for opt-in sagittal presentation.</summary>
        public bool Bind(
            Transform suppliedVisualRoot,
            Transform characterOrientationReference,
            HumanoidBoneNameMap names,
            ProceduralHumanoidAxisPolicy axisPolicy)
        {
            if (characterOrientationReference == null || !HasSupportedScale(characterOrientationReference))
            {
                Clear();
                return false;
            }
            return BindInternal(suppliedVisualRoot, characterOrientationReference, names, axisPolicy);
        }

        private bool BindInternal(
            Transform suppliedVisualRoot,
            Transform characterOrientationReference,
            HumanoidBoneNameMap names,
            ProceduralHumanoidAxisPolicy axisPolicy)
        {
            Clear();
            if (suppliedVisualRoot == null || names == null || !names.IsValid() ||
                (axisPolicy != ProceduralHumanoidAxisPolicy.LocalBoneAxes &&
                 axisPolicy != ProceduralHumanoidAxisPolicy.CharacterSagittalPlane))
                return false;
            if (axisPolicy == ProceduralHumanoidAxisPolicy.CharacterSagittalPlane &&
                (characterOrientationReference == null ||
                 !HasSupportedScale(suppliedVisualRoot) ||
                 !HasSupportedScale(characterOrientationReference)))
                return false;

            var descendants = suppliedVisualRoot.GetComponentsInChildren<Transform>(true);
            Transform foundLeftUpperArm = null;
            Transform foundRightUpperArm = null;
            Transform foundLeftThigh = null;
            Transform foundRightThigh = null;
            Transform foundLeftCalf = null;
            Transform foundRightCalf = null;
            Transform foundUpperTorso = null;
            var duplicateUpperTorso = false;
            var wantsUpperTorso = axisPolicy == ProceduralHumanoidAxisPolicy.CharacterSagittalPlane &&
                                  IsBloodKnightTuning() && names.HasUsableOptionalUpperTorsoName();
            for (var index = 0; index < descendants.Length; index++)
            {
                var candidate = descendants[index];
                if (candidate == suppliedVisualRoot)
                    continue;
                if (wantsUpperTorso && string.Equals(candidate.name, names.OptionalUpperTorso, System.StringComparison.Ordinal))
                {
                    if (foundUpperTorso == null)
                        foundUpperTorso = candidate;
                    else
                        duplicateUpperTorso = true;
                }
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

            if (axisPolicy == ProceduralHumanoidAxisPolicy.CharacterSagittalPlane &&
                (!HasSupportedScale(foundLeftUpperArm) || !HasSupportedScale(foundRightUpperArm) ||
                 !HasSupportedScale(foundLeftThigh) || !HasSupportedScale(foundRightThigh) ||
                 !HasSupportedScale(foundLeftCalf) || !HasSupportedScale(foundRightCalf)))
                return false;

            visualRoot = suppliedVisualRoot;
            leftUpperArm = foundLeftUpperArm;
            rightUpperArm = foundRightUpperArm;
            leftThigh = foundLeftThigh;
            rightThigh = foundRightThigh;
            leftCalf = foundLeftCalf;
            rightCalf = foundRightCalf;
            CaptureBaselines();
            if (axisPolicy == ProceduralHumanoidAxisPolicy.CharacterSagittalPlane &&
                !TryCacheSagittalAxes(characterOrientationReference))
            {
                Clear();
                return false;
            }
            usesCharacterSagittalPlane = axisPolicy == ProceduralHumanoidAxisPolicy.CharacterSagittalPlane;
            TryBindOptionalUpperTorso(foundUpperTorso, duplicateUpperTorso, characterOrientationReference);
            return true;
        }

        /// <summary>Samples caller-supplied presentation facts without allocating or moving any root.</summary>
        public void Sample(
            CharacterMotionPresentationInput input,
            float normalizedLocomotionSpeed,
            float presentationClock,
            float deltaTime)
        {
            SampleInternal(input, normalizedLocomotionSpeed, presentationClock, deltaTime, 1f, true, true);
        }

        /// <summary>Samples caller-owned normalized jump progress; this driver has no time authority.</summary>
        public void Sample(
            CharacterMotionPresentationInput input,
            float normalizedLocomotionSpeed,
            float presentationClock,
            float deltaTime,
            float normalizedJumpPresentationProgress)
        {
            SampleInternal(input, normalizedLocomotionSpeed, presentationClock, deltaTime, normalizedJumpPresentationProgress, false, true, default(ProceduralHumanoidCombatPoseSample));
        }

        /// <summary>Samples explicit combat facts without changing resolver priority or owning timing.</summary>
        public void Sample(CharacterMotionPresentationInput input, float normalizedLocomotionSpeed, float presentationClock, float deltaTime, float normalizedJumpPresentationProgress, ProceduralHumanoidCombatPoseSample combat)
        {
            SampleInternal(input, normalizedLocomotionSpeed, presentationClock, deltaTime, normalizedJumpPresentationProgress, false, false, combat);
        }

        private void SampleInternal(
            CharacterMotionPresentationInput input,
            float normalizedLocomotionSpeed,
            float presentationClock,
            float deltaTime,
            float normalizedJumpPresentationProgress,
            bool useLegacyStaticJumpPose,
            bool useLegacyCombatPose,
            ProceduralHumanoidCombatPoseSample combat = default(ProceduralHumanoidCombatPoseSample))
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
                    if (useLegacyStaticJumpPose) ApplyPose(tuning.JumpFall, 1f, UsesBloodKnightSagittalPlane); else ApplyJumpFall(jumpProgress);
                    break;
                case MotionClipKey.JumpLand:
                    if (useLegacyStaticJumpPose) ApplyPose(tuning.JumpLand, 1f, UsesBloodKnightSagittalPlane); else ApplyJumpLand(jumpProgress);
                    break;
                case MotionClipKey.AttackPrimary:
                    if (useLegacyCombatPose)
                        ApplyPrimaryAttack();
                    else
                    {
                        ApplyLiveBase(speed, swing);
                        ApplyCombatAttack(combat);
                        ApplyUpperTorsoAttack(speed, swing, combat);
                    }
                    break;
                case MotionClipKey.AttackAbility:
                    if (useLegacyCombatPose)
                        ApplyAbilityAttack();
                    else
                    {
                        ApplyLiveBase(speed, swing);
                        ApplyCombatAttack(combat);
                        ApplyUpperTorsoAttack(speed, swing, combat);
                    }
                    break;
                case MotionClipKey.Hit:
                    if (useLegacyCombatPose)
                        ApplyHit();
                    else
                    {
                        ApplyLiveBase(speed, swing);
                        ApplyCombatHit(combat);
                        ApplyUpperTorsoHit(speed, swing, combat);
                    }
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
            upperTorso = null;
            leftUpperArmSagittalAxis = Vector3.zero;
            rightUpperArmSagittalAxis = Vector3.zero;
            leftThighSagittalAxis = Vector3.zero;
            rightThighSagittalAxis = Vector3.zero;
            leftCalfSagittalAxis = Vector3.zero;
            rightCalfSagittalAxis = Vector3.zero;
            upperTorsoSagittalAxis = Vector3.zero;
            upperTorsoBaseline = Quaternion.identity;
            usesCharacterSagittalPlane = false;
        }

        private void TryBindOptionalUpperTorso(
            Transform candidate,
            bool isDuplicate,
            Transform characterOrientationReference)
        {
            if (!usesCharacterSagittalPlane || !IsBloodKnightTuning() || candidate == null ||
                isDuplicate || !HasSupportedScale(candidate))
                return;

            var referenceRight = characterOrientationReference.rotation * Vector3.right;
            if (!IsFinite(referenceRight) || referenceRight.sqrMagnitude < 0.999f)
                return;
            referenceRight.Normalize();
            if (!TryCalculateLocalAxis(candidate, referenceRight, out var localAxis))
                return;

            upperTorso = candidate;
            upperTorsoBaseline = candidate.localRotation;
            upperTorsoSagittalAxis = localAxis;
        }

        private bool TryCacheSagittalAxes(Transform characterOrientationReference)
        {
            var referenceRight = characterOrientationReference.rotation * Vector3.right;
            if (!IsFinite(referenceRight) || referenceRight.sqrMagnitude < 0.999f)
                return false;
            referenceRight.Normalize();

            return TryCalculateLocalAxis(leftUpperArm, referenceRight, out leftUpperArmSagittalAxis) &&
                   TryCalculateLocalAxis(rightUpperArm, referenceRight, out rightUpperArmSagittalAxis) &&
                   TryCalculateLocalAxis(leftThigh, referenceRight, out leftThighSagittalAxis) &&
                   TryCalculateLocalAxis(rightThigh, referenceRight, out rightThighSagittalAxis) &&
                   TryCalculateLocalAxis(leftCalf, referenceRight, out leftCalfSagittalAxis) &&
                   TryCalculateLocalAxis(rightCalf, referenceRight, out rightCalfSagittalAxis);
        }

        private static bool TryCalculateLocalAxis(Transform bone, Vector3 referenceRight, out Vector3 localAxis)
        {
            var baselineWorldRotation = bone.rotation;
            localAxis = Quaternion.Inverse(baselineWorldRotation) * referenceRight;
            if (!IsFinite(baselineWorldRotation) || !IsFinite(localAxis) || localAxis.sqrMagnitude < 0.999f)
            {
                localAxis = Vector3.zero;
                return false;
            }

            localAxis.Normalize();
            return true;
        }

        private static bool HasSupportedScale(Transform transform)
        {
            var scale = transform.lossyScale;
            if (!IsFinite(scale) || Mathf.Abs(scale.x) < 0.0001f || Mathf.Abs(scale.y) < 0.0001f || Mathf.Abs(scale.z) < 0.0001f)
                return false;
            var determinant = transform.localToWorldMatrix.determinant;
            return IsFinite(determinant) && determinant > 0.000001f;
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
            if (upperTorso != null)
                upperTorso.localRotation = upperTorsoBaseline;
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
            var useSemanticAxis = UsesBloodKnightSagittalPlane;
            AddRotation(leftUpperArm, leftUpperArmBaseline, EffectiveAxis(leftUpperArm, tuning.Locomotion.UpperArmAxis, useSemanticAxis), arm, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(rightUpperArm, rightUpperArmBaseline, EffectiveAxis(rightUpperArm, tuning.Locomotion.UpperArmAxis, useSemanticAxis), -arm, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(leftThigh, leftThighBaseline, EffectiveAxis(leftThigh, tuning.Locomotion.ThighAxis, useSemanticAxis), -thigh, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(rightThigh, rightThighBaseline, EffectiveAxis(rightThigh, tuning.Locomotion.ThighAxis, useSemanticAxis), thigh, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(leftCalf, leftCalfBaseline, EffectiveAxis(leftCalf, tuning.Locomotion.CalfAxis, useSemanticAxis), calf, tuning.Locomotion.MaxAdditiveAngleDegrees);
            AddRotation(rightCalf, rightCalfBaseline, EffectiveAxis(rightCalf, tuning.Locomotion.CalfAxis, useSemanticAxis), -calf, tuning.Locomotion.MaxAdditiveAngleDegrees);
            ApplyUpperTorso(WalkUpperTorsoDegrees(speed, swing), ProceduralHumanoidMotionTuning.BloodKnightUpperTorsoWalkDegrees);
        }

        private void ApplyLiveBase(float speed, float swing)
        {
            if (speed > 0f) ApplyLocomotion(swing, speed); else ApplyIdle(swing);
        }

        private void ApplyJumpTakeoff(float progress)
        {
            var crouch = tuning.DeepCrouch;
            var push = tuning.AsymmetricJumpTakeoff;
            ApplyTakeoff(progress, crouch, push, UsesBloodKnightSagittalPlane);
        }

        private void ApplyJumpFall(float progress)
        {
            ApplyTakeoffToPose(progress, tuning.AsymmetricJumpTakeoff, tuning.JumpFall, UsesBloodKnightSagittalPlane);
        }

        private void ApplyJumpLand(float progress)
        {
            if (progress <= 0.5f)
                ApplyPoseTransition(tuning.JumpFall, tuning.JumpLand, progress * 2f, UsesBloodKnightSagittalPlane);
            else
                ApplyPose(tuning.JumpLand, 2f - progress * 2f, UsesBloodKnightSagittalPlane);
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

        private void ApplyCombatAttack(ProceduralHumanoidCombatPoseSample combat)
        {
            var progress = Finite01(combat.AttackProgress);
            var blend = Finite01(combat.AttackBlend);
            var direction = Signed(combat.SignedAttackDirection);
            var windup = IsBloodKnightTuning() ? ProceduralHumanoidMotionTuning.BloodKnightAttackWindup : tuning.AbilityAttack;
            var impact = IsBloodKnightTuning() ? ProceduralHumanoidMotionTuning.BloodKnightAttackImpact : tuning.AbilityAttack;
            var recovery = IsBloodKnightTuning() ? ProceduralHumanoidMotionTuning.BloodKnightAttackRecovery : tuning.AbilityAttack;
            var stage = ClampAttackStage(combat.AttackStage);
            var useSemanticAxis = UsesBloodKnightSagittalPlane;
            if (stage == ProceduralHumanoidAttackStage.Windup)
                ApplyDirectionalPose(windup, direction, progress * blend, useSemanticAxis);
            else if (stage == ProceduralHumanoidAttackStage.Impact)
                ApplyDirectionalTransition(windup, impact, direction, progress, blend, useSemanticAxis);
            else if (progress <= 0.5f)
                ApplyDirectionalTransition(impact, recovery, direction, progress * 2f, blend, useSemanticAxis);
            else
                ApplyDirectionalPose(recovery, direction, (2f - progress * 2f) * blend, useSemanticAxis);
        }

        private void ApplyCombatHit(ProceduralHumanoidCombatPoseSample combat)
        {
            var progress = Finite01(combat.HitProgress);
            var envelope = progress <= 0.5f ? progress * 2f : 2f - progress * 2f;
            ApplyDirectionalPose(
                IsBloodKnightTuning() ? ProceduralHumanoidMotionTuning.BloodKnightDirectionalHit : tuning.Hit,
                Signed(combat.SignedRecoilDirection),
                envelope * Finite01(combat.HitWeight),
                UsesBloodKnightSagittalPlane);
        }

        private void ApplyUpperTorsoAttack(
            float speed,
            float swing,
            ProceduralHumanoidCombatPoseSample combat)
        {
            if (upperTorso == null)
                return;

            var progress = Finite01(combat.AttackProgress);
            var stage = ClampAttackStage(combat.AttackStage);
            float attackDegrees;
            if (stage == ProceduralHumanoidAttackStage.Windup)
                attackDegrees = Mathf.Lerp(0f, -4f, progress);
            else if (stage == ProceduralHumanoidAttackStage.Impact)
                attackDegrees = Mathf.Lerp(-4f, ProceduralHumanoidMotionTuning.BloodKnightUpperTorsoAttackDegrees, progress);
            else if (progress <= 0.5f)
                attackDegrees = Mathf.Lerp(ProceduralHumanoidMotionTuning.BloodKnightUpperTorsoAttackDegrees, 3f, progress * 2f);
            else
                attackDegrees = Mathf.Lerp(3f, 0f, progress * 2f - 1f);

            var combined = WalkUpperTorsoDegrees(speed, swing) + attackDegrees * Finite01(combat.AttackBlend);
            ApplyUpperTorso(combined, ProceduralHumanoidMotionTuning.BloodKnightUpperTorsoAttackDegrees);
        }

        private void ApplyUpperTorsoHit(
            float speed,
            float swing,
            ProceduralHumanoidCombatPoseSample combat)
        {
            if (upperTorso == null)
                return;

            var progress = Finite01(combat.HitProgress);
            var envelope = progress <= 0.5f ? progress * 2f : 2f - progress * 2f;
            var hitDegrees = -ProceduralHumanoidMotionTuning.BloodKnightUpperTorsoHitDegrees * envelope * Finite01(combat.HitWeight);
            ApplyUpperTorso(
                WalkUpperTorsoDegrees(speed, swing) + hitDegrees,
                ProceduralHumanoidMotionTuning.BloodKnightUpperTorsoHitDegrees);
        }

        private static float WalkUpperTorsoDegrees(float speed, float swing)
        {
            return -ProceduralHumanoidMotionTuning.BloodKnightUpperTorsoWalkDegrees * speed * swing;
        }

        private void ApplyUpperTorso(float degrees, float maximum)
        {
            if (upperTorso == null)
                return;
            AddRotation(upperTorso, upperTorsoBaseline, upperTorsoSagittalAxis, degrees, maximum);
        }

        private void ApplyDirectionalPose(ProceduralHumanoidLimbPose pose, float direction, float weight, bool useSemanticAxis)
        {
            var leftLead = 1f + 0.35f * direction;
            var rightLead = 1f - 0.35f * direction;
            AddOverlayRotation(leftUpperArm, leftUpperArmBaseline, pose.UpperArmAxis, pose.UpperArmDegrees * weight * leftLead, pose.MaxAdditiveAngleDegrees, useSemanticAxis);
            AddOverlayRotation(rightUpperArm, rightUpperArmBaseline, pose.UpperArmAxis, pose.UpperArmDegrees * weight * rightLead, pose.MaxAdditiveAngleDegrees, useSemanticAxis);
            AddOverlayRotation(leftThigh, leftThighBaseline, pose.ThighAxis, pose.ThighDegrees * weight * leftLead, pose.MaxAdditiveAngleDegrees, useSemanticAxis);
            AddOverlayRotation(rightThigh, rightThighBaseline, pose.ThighAxis, pose.ThighDegrees * weight * rightLead, pose.MaxAdditiveAngleDegrees, useSemanticAxis);
            AddOverlayRotation(leftCalf, leftCalfBaseline, pose.CalfAxis, pose.CalfDegrees * weight * leftLead, pose.MaxAdditiveAngleDegrees, useSemanticAxis);
            AddOverlayRotation(rightCalf, rightCalfBaseline, pose.CalfAxis, pose.CalfDegrees * weight * rightLead, pose.MaxAdditiveAngleDegrees, useSemanticAxis);
        }

        private void ApplyDirectionalTransition(ProceduralHumanoidLimbPose start, ProceduralHumanoidLimbPose end, float direction, float progress, float weight, bool useSemanticAxis)
        {
            ApplyDirectionalPose(
                new ProceduralHumanoidLimbPose(
                    Mathf.Lerp(start.UpperArmDegrees, end.UpperArmDegrees, progress),
                    Mathf.Lerp(start.ThighDegrees, end.ThighDegrees, progress),
                    Mathf.Lerp(start.CalfDegrees, end.CalfDegrees, progress),
                    start.UpperArmAxis,
                    start.ThighAxis,
                    start.CalfAxis),
                direction,
                weight,
                useSemanticAxis);
        }

        private void ApplyDeathSettle()
        {
            ApplyPose(tuning.Death);
        }

        private void ApplyTakeoff(float progress, ProceduralHumanoidTakeoffPose crouch, ProceduralHumanoidTakeoffPose push, bool useSemanticAxis)
        {
            var maximum = Mathf.Lerp(crouch.MaxAdditiveAngleDegrees, push.MaxAdditiveAngleDegrees, progress);
            var usePushAxis = progress >= 0.5f;
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, Mathf.Lerp(crouch.UpperArmDegrees, push.UpperArmDegrees, progress), usePushAxis ? push.UpperArmAxis : crouch.UpperArmAxis, maximum, useSemanticAxis);
            AddRotation(leftThigh, leftThighBaseline, EffectiveAxis(leftThigh, usePushAxis ? push.LeftThighAxis : crouch.LeftThighAxis, useSemanticAxis), Mathf.Lerp(crouch.LeftThighDegrees, push.LeftThighDegrees, progress), maximum);
            AddRotation(rightThigh, rightThighBaseline, EffectiveAxis(rightThigh, usePushAxis ? push.RightThighAxis : crouch.RightThighAxis, useSemanticAxis), Mathf.Lerp(crouch.RightThighDegrees, push.RightThighDegrees, progress), maximum);
            AddRotation(leftCalf, leftCalfBaseline, EffectiveAxis(leftCalf, usePushAxis ? push.LeftCalfAxis : crouch.LeftCalfAxis, useSemanticAxis), Mathf.Lerp(crouch.LeftCalfDegrees, push.LeftCalfDegrees, progress), maximum);
            AddRotation(rightCalf, rightCalfBaseline, EffectiveAxis(rightCalf, usePushAxis ? push.RightCalfAxis : crouch.RightCalfAxis, useSemanticAxis), Mathf.Lerp(crouch.RightCalfDegrees, push.RightCalfDegrees, progress), maximum);
        }

        private void ApplyTakeoffToPose(float progress, ProceduralHumanoidTakeoffPose start, ProceduralHumanoidLimbPose end, bool useSemanticAxis)
        {
            var maximum = Mathf.Lerp(start.MaxAdditiveAngleDegrees, end.MaxAdditiveAngleDegrees, progress);
            var useEndAxis = progress >= 0.5f;
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, Mathf.Lerp(start.UpperArmDegrees, end.UpperArmDegrees, progress), useEndAxis ? end.UpperArmAxis : start.UpperArmAxis, maximum, useSemanticAxis);
            AddRotation(leftThigh, leftThighBaseline, EffectiveAxis(leftThigh, useEndAxis ? end.ThighAxis : start.LeftThighAxis, useSemanticAxis), Mathf.Lerp(start.LeftThighDegrees, end.ThighDegrees, progress), maximum);
            AddRotation(rightThigh, rightThighBaseline, EffectiveAxis(rightThigh, useEndAxis ? end.ThighAxis : start.RightThighAxis, useSemanticAxis), Mathf.Lerp(start.RightThighDegrees, end.ThighDegrees, progress), maximum);
            AddRotation(leftCalf, leftCalfBaseline, EffectiveAxis(leftCalf, useEndAxis ? end.CalfAxis : start.LeftCalfAxis, useSemanticAxis), Mathf.Lerp(start.LeftCalfDegrees, end.CalfDegrees, progress), maximum);
            AddRotation(rightCalf, rightCalfBaseline, EffectiveAxis(rightCalf, useEndAxis ? end.CalfAxis : start.RightCalfAxis, useSemanticAxis), Mathf.Lerp(start.RightCalfDegrees, end.CalfDegrees, progress), maximum);
        }

        private void ApplyPoseTransition(ProceduralHumanoidLimbPose start, ProceduralHumanoidLimbPose end, float progress, bool useSemanticAxis)
        {
            var maximum = Mathf.Lerp(start.MaxAdditiveAngleDegrees, end.MaxAdditiveAngleDegrees, progress);
            var useEndAxis = progress >= 0.5f;
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, Mathf.Lerp(start.UpperArmDegrees, end.UpperArmDegrees, progress), useEndAxis ? end.UpperArmAxis : start.UpperArmAxis, maximum, useSemanticAxis);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, Mathf.Lerp(start.ThighDegrees, end.ThighDegrees, progress), useEndAxis ? end.ThighAxis : start.ThighAxis, maximum, useSemanticAxis);
            ApplyPair(leftCalf, leftCalfBaseline, rightCalf, rightCalfBaseline, Mathf.Lerp(start.CalfDegrees, end.CalfDegrees, progress), useEndAxis ? end.CalfAxis : start.CalfAxis, maximum, useSemanticAxis);
        }

        private void ApplyPose(ProceduralHumanoidLimbPose pose, float weight = 1f, bool useSemanticAxis = false)
        {
            ApplyPair(leftUpperArm, leftUpperArmBaseline, rightUpperArm, rightUpperArmBaseline, pose.UpperArmDegrees * weight, pose.UpperArmAxis, pose.MaxAdditiveAngleDegrees, useSemanticAxis);
            ApplyPair(leftThigh, leftThighBaseline, rightThigh, rightThighBaseline, pose.ThighDegrees * weight, pose.ThighAxis, pose.MaxAdditiveAngleDegrees, useSemanticAxis);
            ApplyPair(leftCalf, leftCalfBaseline, rightCalf, rightCalfBaseline, pose.CalfDegrees * weight, pose.CalfAxis, pose.MaxAdditiveAngleDegrees, useSemanticAxis);
        }

        private void ApplyPair(
            Transform left,
            Quaternion leftBaseline,
            Transform right,
            Quaternion rightBaseline,
            float degrees,
            ProceduralHumanoidLocalAxis axis,
            float maximum,
            bool useSemanticAxis)
        {
            AddRotation(left, leftBaseline, EffectiveAxis(left, axis, useSemanticAxis), degrees, maximum);
            AddRotation(right, rightBaseline, EffectiveAxis(right, axis, useSemanticAxis), degrees, maximum);
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

        private Vector3 EffectiveAxis(Transform target, ProceduralHumanoidLocalAxis localAxis, bool useSemanticAxis)
        {
            if (!useSemanticAxis)
                return Axis(localAxis);
            if (target == leftUpperArm)
                return leftUpperArmSagittalAxis;
            if (target == rightUpperArm)
                return rightUpperArmSagittalAxis;
            if (target == leftThigh)
                return leftThighSagittalAxis;
            if (target == rightThigh)
                return rightThighSagittalAxis;
            if (target == leftCalf)
                return leftCalfSagittalAxis;
            return rightCalfSagittalAxis;
        }

        private static void AddRotation(Transform target, Quaternion baseline, Vector3 axis, float degrees, float maximum)
        {
            target.localRotation = baseline * Quaternion.AngleAxis(
                Mathf.Clamp(degrees, -maximum, maximum), axis);
        }
        private void AddOverlayRotation(
            Transform target,
            Quaternion baseline,
            ProceduralHumanoidLocalAxis localAxis,
            float degrees,
            float maximum,
            bool useSemanticAxis)
        {
            var clampedDegrees = Mathf.Clamp(degrees, -maximum, maximum);
            var axis = EffectiveAxis(target, localAxis, useSemanticAxis);
            if (!useSemanticAxis)
            {
                target.localRotation = target.localRotation * Quaternion.AngleAxis(clampedDegrees, axis);
                return;
            }

            var liveBaseDelta = Quaternion.Inverse(baseline) * target.localRotation;
            target.localRotation = baseline * Quaternion.AngleAxis(clampedDegrees, axis) * liveBaseDelta;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(Quaternion value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z) && IsFinite(value.w);
        }

        private bool IsBloodKnightTuning() { return object.ReferenceEquals(tuning, ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable); }
        private bool UsesBloodKnightSagittalPlane => usesCharacterSagittalPlane && IsBloodKnightTuning();

        private static float Finite01(float value) { return IsFinite(value) ? Mathf.Clamp01(value) : 0f; }
        private static float Signed(float value) { return !IsFinite(value) || value == 0f ? 0f : value < 0f ? -1f : 1f; }
        private static ProceduralHumanoidAttackStage ClampAttackStage(ProceduralHumanoidAttackStage value) { return value == ProceduralHumanoidAttackStage.Impact || value == ProceduralHumanoidAttackStage.Recovery ? value : ProceduralHumanoidAttackStage.Windup; }
    }
}
