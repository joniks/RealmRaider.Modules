namespace RealmRaiders.Modules.CharacterProceduralMotion
{
    /// <summary>Explicit starter tuning assignments. Consumers must opt in and resolve them themselves.</summary>
    public static class StarterProceduralHumanoidTuningAssignments
    {
        public const string BloodKnightCharacterId = "realmraiders.blood-knight";

        public static ProceduralHumanoidTuningAssignment BloodKnight { get; } =
            new ProceduralHumanoidTuningAssignment(
                BloodKnightCharacterId,
                StarterProceduralHumanoidTuningProvider.BloodKnightDeviceReadableProfileId,
                StarterProceduralHumanoidTuningProvider.CompatibilityProfileId);
    }
}
