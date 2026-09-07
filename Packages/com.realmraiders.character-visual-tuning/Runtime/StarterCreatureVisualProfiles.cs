using System.Collections.Generic;

namespace RealmRaiders.CharacterVisualTuning
{
    public static class GuardianEntPrototypeProfile
    {
        public const string ProfileId = "realmraiders.guardian-ent.prototype";
        public const string SourceId = "realmraiders.source.guardian-ent-prototype";

        public static CharacterVisualTuningProfile Create()
        {
            return new CharacterVisualTuningProfile(
                ProfileId,
                SourceId,
                "Guardian Ent — Living Grove",
                new PresentationTransformIntent(),
                new[]
                {
                    new MaterialPaletteIntent(MaterialRole.PrimaryArmor, "Living bark in layered warm and deep browns."),
                    new MaterialPaletteIntent(MaterialRole.SecondaryArmor, "Sparse forest moss that preserves the trunk silhouette."),
                    new MaterialPaletteIntent(MaterialRole.Accent, "Restrained amber eye glow as the focal point.")
                },
                new MobileVisualBudget(1, 1, 1024, 4000));
        }
    }

    public static class SylvanWolfPrototypeProfile
    {
        public const string ProfileId = "realmraiders.sylvan-wolf.prototype";
        public const string SourceId = "realmraiders.source.sylvan-wolf-prototype";

        public static CharacterVisualTuningProfile Create()
        {
            return new CharacterVisualTuningProfile(
                ProfileId,
                SourceId,
                "Sylvan Wolf — Mossback Hunter",
                new PresentationTransformIntent(),
                new[]
                {
                    new MaterialPaletteIntent(MaterialRole.PrimaryArmor, "Forest grey-brown coat with readable flank planes."),
                    new MaterialPaletteIntent(MaterialRole.SecondaryArmor, "Muted moss along the back; keep the agile outline clear."),
                    new MaterialPaletteIntent(MaterialRole.Accent, "Low-saturation woodland highlights around face and paws.")
                },
                new MobileVisualBudget(1, 1, 512, 1800));
        }
    }

    public static class InfernalBrutePrototypeProfile
    {
        public const string ProfileId = "realmraiders.infernal-brute.prototype";
        public const string SourceId = "realmraiders.source.infernal-brute-prototype";

        public static CharacterVisualTuningProfile Create()
        {
            return new CharacterVisualTuningProfile(
                ProfileId,
                SourceId,
                "Infernal Brute — Obsidian Mauler",
                new PresentationTransformIntent(),
                new[]
                {
                    new MaterialPaletteIntent(MaterialRole.PrimaryArmor, "Heavy obsidian masses with broad, readable breaks."),
                    new MaterialPaletteIntent(MaterialRole.SecondaryArmor, "Charcoal-black surfaces that retain edge definition."),
                    new MaterialPaletteIntent(MaterialRole.Accent, "Sparse ash-ember glow in protected cracks and eyes.")
                },
                new MobileVisualBudget(1, 1, 1024, 4000));
        }
    }

    public static class HellhoundPrototypeProfile
    {
        public const string ProfileId = "realmraiders.hellhound.prototype";
        public const string SourceId = "realmraiders.source.hellhound-prototype";

        public static CharacterVisualTuningProfile Create()
        {
            return new CharacterVisualTuningProfile(
                ProfileId,
                SourceId,
                "Hellhound — Cooled-Lava Stalker",
                new PresentationTransformIntent(),
                new[]
                {
                    new MaterialPaletteIntent(MaterialRole.PrimaryArmor, "Dark ash hide with a lean, uninterrupted silhouette."),
                    new MaterialPaletteIntent(MaterialRole.SecondaryArmor, "Cooled-lava plates in near-black volcanic tones."),
                    new MaterialPaletteIntent(MaterialRole.Accent, "Restrained dormant heat at eyes and narrow fissures.")
                },
                new MobileVisualBudget(1, 1, 512, 1800));
        }
    }

    public static class StarterCreatureVisualProfiles
    {
        public static IReadOnlyList<CharacterVisualTuningProfile> CreateAll()
        {
            return new[]
            {
                GuardianEntPrototypeProfile.Create(),
                SylvanWolfPrototypeProfile.Create(),
                InfernalBrutePrototypeProfile.Create(),
                HellhoundPrototypeProfile.Create()
            };
        }
    }
}
