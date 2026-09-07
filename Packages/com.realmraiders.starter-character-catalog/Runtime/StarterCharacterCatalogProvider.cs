using System.Collections.Generic;
using System.Collections.ObjectModel;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.StarterCharacterCatalog
{
    public sealed class StarterCharacterCatalogProvider : ICharacterCatalogProvider
    {
        public const string StableModuleId = "realmraiders.starter-character-catalog";

        private static readonly IReadOnlyList<CharacterCatalogueEntry> Entries =
            new ReadOnlyCollection<CharacterCatalogueEntry>(new[]
            {
                new CharacterCatalogueEntry(
                    "realmraiders.blood-knight",
                    "Blood Knight",
                    CharacterBodyFamily.Humanoid,
                    "realmraiders.blood-knight.3drt-baseline"),
                new CharacterCatalogueEntry(
                    "realmraiders.guardian-ent",
                    "Guardian Ent",
                    CharacterBodyFamily.LargeCreature,
                    "realmraiders.guardian-ent.prototype"),
                new CharacterCatalogueEntry(
                    "realmraiders.sylvan-wolf",
                    "Sylvan Wolf",
                    CharacterBodyFamily.Beast,
                    "realmraiders.sylvan-wolf.prototype"),
                new CharacterCatalogueEntry(
                    "realmraiders.infernal-brute",
                    "Infernal Brute",
                    CharacterBodyFamily.LargeCreature,
                    "realmraiders.infernal-brute.prototype"),
                new CharacterCatalogueEntry(
                    "realmraiders.hellhound",
                    "Hellhound",
                    CharacterBodyFamily.Beast,
                    "realmraiders.hellhound.prototype")
            });

        public string ModuleId => StableModuleId;

        public IReadOnlyList<CharacterCatalogueEntry> GetCatalogueEntries()
        {
            return Entries;
        }
    }
}
