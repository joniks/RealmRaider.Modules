namespace RealmRaiders.Modules.CharacterProceduralMotion
{
    /// <summary>Closed set of safe local axes for additive limb presentation.</summary>
    public enum ProceduralHumanoidLocalAxis
    {
        Right = 0,
        Up = 1,
        Forward = 2,
    }

    /// <summary>Immutable additive limb strengths for one presentation pose.</summary>
    public readonly struct ProceduralHumanoidLimbPose
    {
        public ProceduralHumanoidLimbPose(float upperArmDegrees, float thighDegrees, float calfDegrees)
            : this(
                upperArmDegrees, thighDegrees, calfDegrees,
                ProceduralHumanoidLocalAxis.Right, ProceduralHumanoidLocalAxis.Right, ProceduralHumanoidLocalAxis.Right)
        {
        }

        public ProceduralHumanoidLimbPose(
            float upperArmDegrees,
            float thighDegrees,
            float calfDegrees,
            ProceduralHumanoidLocalAxis upperArmAxis,
            ProceduralHumanoidLocalAxis thighAxis,
            ProceduralHumanoidLocalAxis calfAxis)
            : this(upperArmDegrees, thighDegrees, calfDegrees, upperArmAxis, thighAxis, calfAxis, ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees)
        {
        }

        public ProceduralHumanoidLimbPose(float upperArmDegrees, float thighDegrees, float calfDegrees, ProceduralHumanoidLocalAxis upperArmAxis, ProceduralHumanoidLocalAxis thighAxis, ProceduralHumanoidLocalAxis calfAxis, float maximum)
        {
            MaxAdditiveAngleDegrees = ClampMaximum(maximum);
            UpperArmDegrees = ClampDegrees(upperArmDegrees, MaxAdditiveAngleDegrees);
            ThighDegrees = ClampDegrees(thighDegrees, MaxAdditiveAngleDegrees);
            CalfDegrees = ClampDegrees(calfDegrees, MaxAdditiveAngleDegrees);
            UpperArmAxis = ClampAxis(upperArmAxis);
            ThighAxis = ClampAxis(thighAxis);
            CalfAxis = ClampAxis(calfAxis);
        }

        public float UpperArmDegrees { get; }
        public float ThighDegrees { get; }
        public float CalfDegrees { get; }
        public ProceduralHumanoidLocalAxis UpperArmAxis { get; }
        public ProceduralHumanoidLocalAxis ThighAxis { get; }
        public ProceduralHumanoidLocalAxis CalfAxis { get; }
        public float MaxAdditiveAngleDegrees { get; }

        internal static float ClampDegrees(float value)
        {
            return ClampDegrees(value, ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees);
        }

        internal static float ClampDegrees(float value, float maximum)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 0f;
            return value < -maximum ? -maximum : value > maximum ? maximum
                    : value;
        }

        internal static float ClampMaximum(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees;
            return value < ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees ? ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees : value > ProceduralHumanoidPoseDriver.MaxPerPoseAngleDegrees ? ProceduralHumanoidPoseDriver.MaxPerPoseAngleDegrees : value;
        }

        internal static ProceduralHumanoidLocalAxis ClampAxis(ProceduralHumanoidLocalAxis value)
        {
            return value == ProceduralHumanoidLocalAxis.Up || value == ProceduralHumanoidLocalAxis.Forward
                ? value
                : ProceduralHumanoidLocalAxis.Right;
        }
    }

    /// <summary>Immutable takeoff silhouette with independently posed planted and free legs.</summary>
    public readonly struct ProceduralHumanoidTakeoffPose
    {
        public ProceduralHumanoidTakeoffPose(
            float upperArmDegrees,
            float leftThighDegrees,
            float rightThighDegrees,
            float leftCalfDegrees,
            float rightCalfDegrees,
            ProceduralHumanoidLocalAxis upperArmAxis,
            ProceduralHumanoidLocalAxis leftThighAxis,
            ProceduralHumanoidLocalAxis rightThighAxis,
            ProceduralHumanoidLocalAxis leftCalfAxis,
            ProceduralHumanoidLocalAxis rightCalfAxis)
            : this(upperArmDegrees, leftThighDegrees, rightThighDegrees, leftCalfDegrees, rightCalfDegrees, upperArmAxis, leftThighAxis, rightThighAxis, leftCalfAxis, rightCalfAxis, ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees)
        {
        }

        public ProceduralHumanoidTakeoffPose(float upperArmDegrees, float leftThighDegrees, float rightThighDegrees, float leftCalfDegrees, float rightCalfDegrees, ProceduralHumanoidLocalAxis upperArmAxis, ProceduralHumanoidLocalAxis leftThighAxis, ProceduralHumanoidLocalAxis rightThighAxis, ProceduralHumanoidLocalAxis leftCalfAxis, ProceduralHumanoidLocalAxis rightCalfAxis, float maximum)
        {
            MaxAdditiveAngleDegrees = ProceduralHumanoidLimbPose.ClampMaximum(maximum);
            UpperArmDegrees = ProceduralHumanoidLimbPose.ClampDegrees(upperArmDegrees, MaxAdditiveAngleDegrees);
            LeftThighDegrees = ProceduralHumanoidLimbPose.ClampDegrees(leftThighDegrees, MaxAdditiveAngleDegrees);
            RightThighDegrees = ProceduralHumanoidLimbPose.ClampDegrees(rightThighDegrees, MaxAdditiveAngleDegrees);
            LeftCalfDegrees = ProceduralHumanoidLimbPose.ClampDegrees(leftCalfDegrees, MaxAdditiveAngleDegrees);
            RightCalfDegrees = ProceduralHumanoidLimbPose.ClampDegrees(rightCalfDegrees, MaxAdditiveAngleDegrees);
            UpperArmAxis = ProceduralHumanoidLimbPose.ClampAxis(upperArmAxis);
            LeftThighAxis = ProceduralHumanoidLimbPose.ClampAxis(leftThighAxis);
            RightThighAxis = ProceduralHumanoidLimbPose.ClampAxis(rightThighAxis);
            LeftCalfAxis = ProceduralHumanoidLimbPose.ClampAxis(leftCalfAxis);
            RightCalfAxis = ProceduralHumanoidLimbPose.ClampAxis(rightCalfAxis);
        }

        public float UpperArmDegrees { get; }
        public float LeftThighDegrees { get; }
        public float RightThighDegrees { get; }
        public float LeftCalfDegrees { get; }
        public float RightCalfDegrees { get; }
        public ProceduralHumanoidLocalAxis UpperArmAxis { get; }
        public ProceduralHumanoidLocalAxis LeftThighAxis { get; }
        public ProceduralHumanoidLocalAxis RightThighAxis { get; }
        public ProceduralHumanoidLocalAxis LeftCalfAxis { get; }
        public ProceduralHumanoidLocalAxis RightCalfAxis { get; }
        public float MaxAdditiveAngleDegrees { get; }

        internal static ProceduralHumanoidTakeoffPose FromSymmetric(ProceduralHumanoidLimbPose pose)
        {
            return new ProceduralHumanoidTakeoffPose(
                pose.UpperArmDegrees, pose.ThighDegrees, pose.ThighDegrees, pose.CalfDegrees, pose.CalfDegrees,
                pose.UpperArmAxis, pose.ThighAxis, pose.ThighAxis, pose.CalfAxis, pose.CalfAxis);
        }
    }

    /// <summary>
    /// Immutable, caller-selected presentation tuning. Values are clamped on construction so a
    /// profile can never request an unsafe additive rotation or non-finite cadence.
    /// </summary>
    public sealed class ProceduralHumanoidMotionTuning
    {
        public const float MaxSwingCadenceRadiansPerSecond = 16f;
        public const float BloodKnightLocomotionMaxAdditiveAngleDegrees = 60f;
        public const float BloodKnightCrouchMaxAdditiveAngleDegrees = 90f;

        public static readonly ProceduralHumanoidMotionTuning CompatibilityDefault =
            new ProceduralHumanoidMotionTuning(
                6f, 2f,
                new ProceduralHumanoidLimbPose(22f, 18f, 6.3f),
                new ProceduralHumanoidLimbPose(-12f, -10f, 8f),
                new ProceduralHumanoidLimbPose(16f, 8f, 0f),
                new ProceduralHumanoidLimbPose(-6f, -16f, 18f),
                -28f, 8f,
                new ProceduralHumanoidLimbPose(-22f, 8f, 0f),
                new ProceduralHumanoidLimbPose(10f, -6f, 0f),
                new ProceduralHumanoidLimbPose(12f, -8f, 0f));

        /// <summary>Stronger device-readable silhouette for Blood Knight's named visual recipe.</summary>
        public static readonly ProceduralHumanoidMotionTuning BloodKnightDeviceReadable =
            new ProceduralHumanoidMotionTuning(
                7.5f, 3.5f,
                new ProceduralHumanoidLimbPose(
                    56f, 50f, 26f,
                    ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, BloodKnightLocomotionMaxAdditiveAngleDegrees),
                new ProceduralHumanoidLimbPose(-18f, -16f, 12f),
                new ProceduralHumanoidLimbPose(22f, 12f, 0f, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward),
                new ProceduralHumanoidLimbPose(-10f, -22f, 24f, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward),
                -30f, 12f,
                new ProceduralHumanoidLimbPose(-28f, 12f, 0f),
                new ProceduralHumanoidLimbPose(14f, -9f, 0f),
                new ProceduralHumanoidLimbPose(16f, -12f, 0f),
                new ProceduralHumanoidTakeoffPose(
                    -18f, -22f, 12f, 20f, -10f,
                    ProceduralHumanoidLocalAxis.Forward,
                    ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward,
                    ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward),
                new ProceduralHumanoidTakeoffPose(-30f, -88f, -88f, 80f, 80f, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, BloodKnightCrouchMaxAdditiveAngleDegrees),
                4f, 3f);

        public ProceduralHumanoidMotionTuning(
            float swingCadenceRadiansPerSecond,
            float idleArmDegrees,
            ProceduralHumanoidLimbPose locomotion,
            ProceduralHumanoidLimbPose jumpTakeoff,
            ProceduralHumanoidLimbPose jumpFall,
            ProceduralHumanoidLimbPose jumpLand,
            float primaryWeaponArmDegrees,
            float primarySupportArmDegrees,
            ProceduralHumanoidLimbPose abilityAttack,
            ProceduralHumanoidLimbPose hit,
            ProceduralHumanoidLimbPose death)
            : this(
                swingCadenceRadiansPerSecond, idleArmDegrees, locomotion, jumpTakeoff, jumpFall, jumpLand,
                primaryWeaponArmDegrees, primarySupportArmDegrees, abilityAttack, hit, death,
                ProceduralHumanoidTakeoffPose.FromSymmetric(jumpTakeoff))
        {
        }

        public ProceduralHumanoidMotionTuning(
            float swingCadenceRadiansPerSecond,
            float idleArmDegrees,
            ProceduralHumanoidLimbPose locomotion,
            ProceduralHumanoidLimbPose jumpTakeoff,
            ProceduralHumanoidLimbPose jumpFall,
            ProceduralHumanoidLimbPose jumpLand,
            float primaryWeaponArmDegrees,
            float primarySupportArmDegrees,
            ProceduralHumanoidLimbPose abilityAttack,
            ProceduralHumanoidLimbPose hit,
            ProceduralHumanoidLimbPose death,
            ProceduralHumanoidTakeoffPose asymmetricJumpTakeoff)
            : this(swingCadenceRadiansPerSecond, idleArmDegrees, locomotion, jumpTakeoff, jumpFall, jumpLand, primaryWeaponArmDegrees, primarySupportArmDegrees, abilityAttack, hit, death, asymmetricJumpTakeoff, ProceduralHumanoidTakeoffPose.FromSymmetric(jumpTakeoff), 1f, 1f)
        {
        }

        public ProceduralHumanoidMotionTuning(float swingCadenceRadiansPerSecond, float idleArmDegrees, ProceduralHumanoidLimbPose locomotion, ProceduralHumanoidLimbPose jumpTakeoff, ProceduralHumanoidLimbPose jumpFall, ProceduralHumanoidLimbPose jumpLand, float primaryWeaponArmDegrees, float primarySupportArmDegrees, ProceduralHumanoidLimbPose abilityAttack, ProceduralHumanoidLimbPose hit, ProceduralHumanoidLimbPose death, ProceduralHumanoidTakeoffPose asymmetricJumpTakeoff, ProceduralHumanoidTakeoffPose deepCrouch, float jumpPresentationDurationMultiplier, float takeoffStraightenDurationMultiplier)
        {
            SwingCadenceRadiansPerSecond = ClampCadence(swingCadenceRadiansPerSecond);
            IdleArmDegrees = ProceduralHumanoidLimbPose.ClampDegrees(idleArmDegrees);
            Locomotion = locomotion;
            JumpTakeoff = jumpTakeoff;
            AsymmetricJumpTakeoff = asymmetricJumpTakeoff;
            DeepCrouch = deepCrouch;
            JumpFall = jumpFall;
            JumpLand = jumpLand;
            PrimaryWeaponArmDegrees = ProceduralHumanoidLimbPose.ClampDegrees(primaryWeaponArmDegrees);
            PrimarySupportArmDegrees = ProceduralHumanoidLimbPose.ClampDegrees(primarySupportArmDegrees);
            AbilityAttack = abilityAttack;
            Hit = hit;
            Death = death;
            JumpPresentationDurationMultiplier = ClampDurationMultiplier(jumpPresentationDurationMultiplier);
            TakeoffStraightenDurationMultiplier = ClampDurationMultiplier(takeoffStraightenDurationMultiplier);
        }

        public float SwingCadenceRadiansPerSecond { get; }
        public float IdleArmDegrees { get; }
        public ProceduralHumanoidLimbPose Locomotion { get; }
        public ProceduralHumanoidLimbPose JumpTakeoff { get; }
        public ProceduralHumanoidTakeoffPose AsymmetricJumpTakeoff { get; }
        public ProceduralHumanoidTakeoffPose DeepCrouch { get; }
        public ProceduralHumanoidLimbPose JumpFall { get; }
        public ProceduralHumanoidLimbPose JumpLand { get; }
        public float PrimaryWeaponArmDegrees { get; }
        public float PrimarySupportArmDegrees { get; }
        public ProceduralHumanoidLimbPose AbilityAttack { get; }
        public ProceduralHumanoidLimbPose Hit { get; }
        public ProceduralHumanoidLimbPose Death { get; }
        /// <summary>Core-owned duration multiplier for the entire factual jump presentation timeline.</summary>
        public float JumpPresentationDurationMultiplier { get; }
        /// <summary>Core-owned duration multiplier for the normalized crouch-to-push segment.</summary>
        public float TakeoffStraightenDurationMultiplier { get; }

        private static float ClampCadence(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                return 0f;
            return value > MaxSwingCadenceRadiansPerSecond ? MaxSwingCadenceRadiansPerSecond : value;
        }

        private static float ClampDurationMultiplier(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 1f) return 1f;
            return value > 4f ? 4f : value;
        }
    }
}
