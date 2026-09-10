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
        {
            UpperArmDegrees = ClampDegrees(upperArmDegrees);
            ThighDegrees = ClampDegrees(thighDegrees);
            CalfDegrees = ClampDegrees(calfDegrees);
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

        internal static float ClampDegrees(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 0f;
            return value < -ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees
                ? -ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees
                : value > ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees
                    ? ProceduralHumanoidPoseDriver.MaxAdditiveAngleDegrees
                    : value;
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
        {
            UpperArmDegrees = ProceduralHumanoidLimbPose.ClampDegrees(upperArmDegrees);
            LeftThighDegrees = ProceduralHumanoidLimbPose.ClampDegrees(leftThighDegrees);
            RightThighDegrees = ProceduralHumanoidLimbPose.ClampDegrees(rightThighDegrees);
            LeftCalfDegrees = ProceduralHumanoidLimbPose.ClampDegrees(leftCalfDegrees);
            RightCalfDegrees = ProceduralHumanoidLimbPose.ClampDegrees(rightCalfDegrees);
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
                    28f, 25f, 13f,
                    ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward),
                new ProceduralHumanoidLimbPose(-18f, -16f, 12f),
                new ProceduralHumanoidLimbPose(22f, 12f, 0f),
                new ProceduralHumanoidLimbPose(-10f, -22f, 24f),
                -30f, 12f,
                new ProceduralHumanoidLimbPose(-28f, 12f, 0f),
                new ProceduralHumanoidLimbPose(14f, -9f, 0f),
                new ProceduralHumanoidLimbPose(16f, -12f, 0f),
                new ProceduralHumanoidTakeoffPose(
                    -18f, -22f, 12f, 20f, -10f,
                    ProceduralHumanoidLocalAxis.Forward,
                    ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward,
                    ProceduralHumanoidLocalAxis.Forward, ProceduralHumanoidLocalAxis.Forward));

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
        {
            SwingCadenceRadiansPerSecond = ClampCadence(swingCadenceRadiansPerSecond);
            IdleArmDegrees = ProceduralHumanoidLimbPose.ClampDegrees(idleArmDegrees);
            Locomotion = locomotion;
            JumpTakeoff = jumpTakeoff;
            AsymmetricJumpTakeoff = asymmetricJumpTakeoff;
            JumpFall = jumpFall;
            JumpLand = jumpLand;
            PrimaryWeaponArmDegrees = ProceduralHumanoidLimbPose.ClampDegrees(primaryWeaponArmDegrees);
            PrimarySupportArmDegrees = ProceduralHumanoidLimbPose.ClampDegrees(primarySupportArmDegrees);
            AbilityAttack = abilityAttack;
            Hit = hit;
            Death = death;
        }

        public float SwingCadenceRadiansPerSecond { get; }
        public float IdleArmDegrees { get; }
        public ProceduralHumanoidLimbPose Locomotion { get; }
        public ProceduralHumanoidLimbPose JumpTakeoff { get; }
        public ProceduralHumanoidTakeoffPose AsymmetricJumpTakeoff { get; }
        public ProceduralHumanoidLimbPose JumpFall { get; }
        public ProceduralHumanoidLimbPose JumpLand { get; }
        public float PrimaryWeaponArmDegrees { get; }
        public float PrimarySupportArmDegrees { get; }
        public ProceduralHumanoidLimbPose AbilityAttack { get; }
        public ProceduralHumanoidLimbPose Hit { get; }
        public ProceduralHumanoidLimbPose Death { get; }

        private static float ClampCadence(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                return 0f;
            return value > MaxSwingCadenceRadiansPerSecond ? MaxSwingCadenceRadiansPerSecond : value;
        }
    }
}
