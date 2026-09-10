namespace RealmRaiders.Modules.CharacterProceduralMotion
{
    /// <summary>Immutable additive limb strengths for one presentation pose.</summary>
    public readonly struct ProceduralHumanoidLimbPose
    {
        public ProceduralHumanoidLimbPose(float upperArmDegrees, float thighDegrees, float calfDegrees)
        {
            UpperArmDegrees = ClampDegrees(upperArmDegrees);
            ThighDegrees = ClampDegrees(thighDegrees);
            CalfDegrees = ClampDegrees(calfDegrees);
        }

        public float UpperArmDegrees { get; }
        public float ThighDegrees { get; }
        public float CalfDegrees { get; }

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
                new ProceduralHumanoidLimbPose(28f, 25f, 13f),
                new ProceduralHumanoidLimbPose(-18f, -16f, 12f),
                new ProceduralHumanoidLimbPose(22f, 12f, 0f),
                new ProceduralHumanoidLimbPose(-10f, -22f, 24f),
                -30f, 12f,
                new ProceduralHumanoidLimbPose(-28f, 12f, 0f),
                new ProceduralHumanoidLimbPose(14f, -9f, 0f),
                new ProceduralHumanoidLimbPose(16f, -12f, 0f));

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
        {
            SwingCadenceRadiansPerSecond = ClampCadence(swingCadenceRadiansPerSecond);
            IdleArmDegrees = ProceduralHumanoidLimbPose.ClampDegrees(idleArmDegrees);
            Locomotion = locomotion;
            JumpTakeoff = jumpTakeoff;
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
