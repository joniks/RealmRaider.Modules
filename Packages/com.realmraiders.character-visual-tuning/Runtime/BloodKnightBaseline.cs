namespace RealmRaiders.CharacterVisualTuning
{
    public static class BloodKnightBaseline
    {
        public const string ProfileId = "realmraiders.blood-knight.3drt-baseline";
        public const string SourceId = "3drt.fantasy-warrior";

        public static CharacterVisualTuningProfile Create()
        {
            return new CharacterVisualTuningProfile(
                ProfileId,
                SourceId,
                "Blood Knight — 3DRT baseline",
                new PresentationTransformIntent(),
                new[]
                {
                    new MaterialPaletteIntent(MaterialRole.PrimaryArmor, "Aged metal; low-gloss, battle-worn dark fantasy."),
                    new MaterialPaletteIntent(MaterialRole.SecondaryArmor, "Charcoal; preserve silhouette separation."),
                    new MaterialPaletteIntent(MaterialRole.Cloth, "Restrained crimson; subordinate to the silhouette."),
                    new MaterialPaletteIntent(MaterialRole.Accent, "Sparse muted crimson accents; avoid bright saturation.")
                },
                new MobileVisualBudget(
                    materialCount: 1,
                    textureCount: 1,
                    maxTextureEdgePixels: 1024,
                    triangleCount: 2500));
        }
    }
}
