using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.CharacterVisualTuning
{
    /// <summary>
    /// Explicit provider for the five existing starter visual-tuning profiles.
    /// It does not discover profiles or alter any profile value.
    /// </summary>
    public sealed class StarterCharacterVisualTuningProvider : ICharacterVisualTuningProfileProvider
    {
        public const string StableModuleId = "realmraiders.starter-character-visual-tuning";

        private readonly ReadOnlyCollection<CharacterVisualTuningProfile> profiles;

        public StarterCharacterVisualTuningProvider()
        {
            profiles = new ReadOnlyCollection<CharacterVisualTuningProfile>(
                new List<CharacterVisualTuningProfile>
                {
                    BloodKnightBaseline.Create(),
                    GuardianEntPrototypeProfile.Create(),
                    SylvanWolfPrototypeProfile.Create(),
                    InfernalBrutePrototypeProfile.Create(),
                    HellhoundPrototypeProfile.Create()
                });
        }

        public string ModuleId => StableModuleId;
        public IReadOnlyList<CharacterVisualTuningProfile> Profiles => profiles;
    }
}
