using System.Linq;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.StarterCharacterCatalog.Tests
{
    public sealed class StarterCharacterCatalogProviderTests
    {
        private static readonly ExpectedEntry[] ExpectedEntries =
        {
            new ExpectedEntry("realmraiders.blood-knight", "Blood Knight", CharacterBodyFamily.Humanoid, "realmraiders.blood-knight.3drt-baseline"),
            new ExpectedEntry("realmraiders.guardian-ent", "Guardian Ent", CharacterBodyFamily.LargeCreature, "realmraiders.guardian-ent.prototype"),
            new ExpectedEntry("realmraiders.sylvan-wolf", "Sylvan Wolf", CharacterBodyFamily.Beast, "realmraiders.sylvan-wolf.prototype"),
            new ExpectedEntry("realmraiders.infernal-brute", "Infernal Brute", CharacterBodyFamily.LargeCreature, "realmraiders.infernal-brute.prototype"),
            new ExpectedEntry("realmraiders.hellhound", "Hellhound", CharacterBodyFamily.Beast, "realmraiders.hellhound.prototype")
        };

        [Test]
        public void Provider_ExposesExactOrderedStarterRoster()
        {
            ICharacterCatalogProvider provider = new StarterCharacterCatalogProvider();
            var entries = provider.GetCatalogueEntries();

            Assert.That(provider.ModuleId, Is.EqualTo("realmraiders.starter-character-catalog"));
            Assert.That(entries.Count, Is.EqualTo(ExpectedEntries.Length));

            for (var index = 0; index < ExpectedEntries.Length; index++)
            {
                Assert.That(entries[index].StableId, Is.EqualTo(ExpectedEntries[index].StableId), "stable ID at index " + index);
                Assert.That(entries[index].DisplayName, Is.EqualTo(ExpectedEntries[index].DisplayName), "display name at index " + index);
                Assert.That(entries[index].BodyFamily, Is.EqualTo(ExpectedEntries[index].BodyFamily), "body family at index " + index);
                Assert.That(entries[index].VisualProfileKey, Is.EqualTo(ExpectedEntries[index].VisualProfileKey), "visual profile key at index " + index);
            }
        }

        [Test]
        public void Provider_UsesUniqueStableIdsWithoutSceneAliases()
        {
            var entries = new StarterCharacterCatalogProvider().GetCatalogueEntries();

            Assert.That(entries.Select(entry => entry.StableId).Distinct().Count(), Is.EqualTo(entries.Count));
            Assert.That(entries.All(entry => entry.StableId.StartsWith("realmraiders.")), Is.True);
            Assert.That(entries.All(entry => entry.StableId == entry.StableId.ToLowerInvariant()), Is.True);
            Assert.That(entries.Any(entry => entry.DisplayName == "Wolf Alpha" || entry.DisplayName == "Hellhound A"), Is.False);
        }

        [Test]
        public void RuntimeAssembly_HasNoUnityEngineOrGameRuntimeReference()
        {
            var dependencies = typeof(StarterCharacterCatalogProvider).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(dependencies, Has.Member("RealmRaiders.ModuleContracts"));
            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private sealed class ExpectedEntry
        {
            public ExpectedEntry(string stableId, string displayName, CharacterBodyFamily bodyFamily, string visualProfileKey)
            {
                StableId = stableId;
                DisplayName = displayName;
                BodyFamily = bodyFamily;
                VisualProfileKey = visualProfileKey;
            }

            public string StableId { get; }
            public string DisplayName { get; }
            public CharacterBodyFamily BodyFamily { get; }
            public string VisualProfileKey { get; }
        }
    }
}
