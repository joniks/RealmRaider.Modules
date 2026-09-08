using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterArtManifests.Tests
{
    public sealed class CharacterArtIntakeManifestCatalogueTests
    {
        [Test]
        public void Build_SnapshotsSortsAndProvidesExactOrdinalLookups()
        {
            var alpha = Manifest("alpha");
            var middle = Manifest("middle");
            var zeta = Manifest("zeta");
            var mutableManifests = new List<CharacterArtIntakeManifest> { zeta, alpha };
            var mutableProviders = new List<ICharacterArtIntakeManifestProvider>
            {
                new TestProvider("test.provider.zeta", mutableManifests),
                new TestProvider("test.provider.middle", middle)
            };

            var first = CharacterArtIntakeManifestCatalogue.Build(mutableProviders);
            var reordered = CharacterArtIntakeManifestCatalogue.Build(new ICharacterArtIntakeManifestProvider[]
            {
                new TestProvider("test.provider.middle", middle),
                new TestProvider("test.provider.zeta", alpha, zeta)
            });

            Assert.That(first.Succeeded, Is.True);
            Assert.That(first.Issues, Is.Empty);
            Assert.That(first.Catalogue.Manifests.Select(manifest => manifest.SourceId), Is.EqualTo(new[]
            {
                "test.source.alpha.v1",
                "test.source.middle.v1",
                "test.source.zeta.v1"
            }));
            Assert.That(CanonicalSignatures(reordered.Catalogue), Is.EqualTo(CanonicalSignatures(first.Catalogue)));
            Assert.That(first.Catalogue.TryGetBySourceId("test.source.middle.v1", out var bySource), Is.True);
            Assert.That(first.Catalogue.TryGetByCharacterId("test.character.middle", out var byCharacter), Is.True);
            Assert.That(bySource, Is.SameAs(middle));
            Assert.That(byCharacter, Is.SameAs(middle));
            Assert.That(first.Catalogue.TryGetBySourceId("TEST.SOURCE.MIDDLE.V1", out _), Is.False);
            Assert.That(first.Catalogue.TryGetByCharacterId("TEST.CHARACTER.MIDDLE", out _), Is.False);
            Assert.That(first.Catalogue.TryGetBySourceId(null, out _), Is.False);
            Assert.That(first.Catalogue.TryGetByCharacterId(null, out _), Is.False);

            mutableManifests.Clear();
            mutableManifests.Add(Manifest("late"));
            mutableProviders.Clear();
            Assert.That(first.Catalogue.Manifests.Count, Is.EqualTo(3));
            Assert.That(first.Catalogue.TryGetBySourceId(alpha.SourceId, out var snapshotted), Is.True);
            Assert.That(snapshotted, Is.SameAs(alpha));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterArtIntakeManifest>)first.Catalogue.Manifests).Clear());
        }

        [Test]
        public void Build_DuplicateModuleSourceAndCharacterIdsFailClosedDeterministically()
        {
            var providers = new ICharacterArtIntakeManifestProvider[]
            {
                new TestProvider(
                    "test.provider.duplicate",
                    Manifest("alpha", sourceId: "test.source.shared.v1")),
                new TestProvider(
                    "test.provider.duplicate",
                    Manifest("beta", characterId: "test.character.shared")),
                new TestProvider(
                    "test.provider.unique",
                    Manifest("gamma", sourceId: "test.source.shared.v1", characterId: "test.character.shared"))
            };

            var first = CharacterArtIntakeManifestCatalogue.Build(providers);
            var reordered = CharacterArtIntakeManifestCatalogue.Build(providers.Reverse());
            var codes = first.Issues.Select(issue => issue.Code).ToArray();

            Assert.That(first.Succeeded, Is.False);
            Assert.That(first.Catalogue, Is.Null);
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.DuplicateProviderModuleId));
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.DuplicateSourceId));
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.AmbiguousCharacterId));
            Assert.That(IssueSignatures(reordered), Is.EqualTo(IssueSignatures(first)));
        }

        [Test]
        public void Build_NullThrowingAndInvalidInputsReturnSortedStructuredIssues()
        {
            var providers = new ICharacterArtIntakeManifestProvider[]
            {
                null,
                new TestProvider("Bad/Provider", Manifest("valid")),
                new ThrowingModuleProvider(),
                new ThrowingManifestsProvider(),
                new TestProvider(
                    "test.provider.null-collection",
                    (IReadOnlyList<CharacterArtIntakeManifest>)null),
                new TestProvider(
                    "test.provider.throwing-collection",
                    new ThrowingCountManifestList()),
                new TestProvider(
                    "test.provider.throwing-item",
                    new ThrowingItemManifestList()),
                new TestProvider(
                    "test.provider.null-item",
                    new CharacterArtIntakeManifest[] { null }),
                new TestProvider(
                    "test.provider.invalid-item",
                    Manifest("invalid", archiveSha256: new string('A', 64)))
            };

            var first = CharacterArtIntakeManifestCatalogue.Build(providers);
            var reordered = CharacterArtIntakeManifestCatalogue.Build(providers.Reverse());
            var codes = first.Issues.Select(issue => issue.Code).ToArray();

            Assert.That(first.Succeeded, Is.False);
            Assert.That(first.Catalogue, Is.Null);
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.NullProvider));
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.UnreadableProvider));
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.InvalidModuleId));
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.NullManifestCollection));
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.UnreadableManifestCollection));
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.UnreadableManifest));
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.NullManifest));
            Assert.That(codes, Has.Member(CharacterArtIntakeManifestCatalogueIssueCode.InvalidManifest));
            Assert.That(first.Issues.Single(issue =>
                    issue.Code == CharacterArtIntakeManifestCatalogueIssueCode.InvalidManifest)
                .ManifestIssueCode, Is.EqualTo(CharacterArtManifestIssueCode.InvalidArchiveSha256));
            Assert.That(IssueSignatures(reordered), Is.EqualTo(IssueSignatures(first)));
            Assert.That(first.Issues, Is.EqualTo(first.Issues
                .OrderBy(issue => issue.Path, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code)
                .ThenBy(issue => issue.ManifestIssueCode)
                .ToArray()));

            var missing = CharacterArtIntakeManifestCatalogue.Build(null);
            Assert.That(missing.Issues.Single().Code,
                Is.EqualTo(CharacterArtIntakeManifestCatalogueIssueCode.NullProviderCollection));

            var unreadable = CharacterArtIntakeManifestCatalogue.Build(new ThrowingProviderCollection());
            Assert.That(unreadable.Issues.Single().Code,
                Is.EqualTo(CharacterArtIntakeManifestCatalogueIssueCode.UnreadableProviderCollection));
        }

        [Test]
        public void EmptyExplicitProviderSetBuildsAnImmutableEmptyCatalogue()
        {
            var result = CharacterArtIntakeManifestCatalogue.Build(
                Array.Empty<ICharacterArtIntakeManifestProvider>());

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Issues, Is.Empty);
            Assert.That(result.Catalogue.Manifests, Is.Empty);
            Assert.That(result.Catalogue.TryGetBySourceId("test.source.missing", out _), Is.False);
            Assert.That(result.Catalogue.TryGetByCharacterId("test.character.missing", out _), Is.False);
        }

        [Test]
        public void CatalogueContractsAndCollections_AreImmutableWithoutDiscoveryUnityOrGameRuntime()
        {
            var success = CharacterArtIntakeManifestCatalogue.Build(new[]
            {
                new TestProvider("test.provider.alpha", Manifest("alpha"))
            });
            var failure = CharacterArtIntakeManifestCatalogue.Build(
                new ICharacterArtIntakeManifestProvider[] { null });

            Assert.That(typeof(ICharacterArtIntakeManifestProvider).GetProperty("ModuleId").CanWrite, Is.False);
            Assert.That(typeof(ICharacterArtIntakeManifestProvider).GetProperty("Manifests").CanWrite, Is.False);
            Assert.That(typeof(CharacterArtIntakeManifestCatalogue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtIntakeManifestCatalogueBuildResult)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtIntakeManifestCatalogueIssue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterArtIntakeManifestCatalogueIssue>)failure.Issues).Clear());

            var dictionaryFields = typeof(CharacterArtIntakeManifestCatalogue)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Count(field => field.FieldType.IsGenericType &&
                    field.FieldType.GetGenericTypeDefinition() == typeof(System.Collections.ObjectModel.ReadOnlyDictionary<,>));
            Assert.That(dictionaryFields, Is.EqualTo(2));

            var dependencies = typeof(CharacterArtIntakeManifestCatalogue).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();
            Assert.That(dependencies, Has.Member("RealmRaiders.ModuleContracts"));
            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static string[] CanonicalSignatures(CharacterArtIntakeManifestCatalogue catalogue)
        {
            return catalogue.Manifests
                .Select(manifest => manifest.SourceId + "|" + manifest.CharacterId + "|" +
                    CharacterArtIntakeManifestCanonicalizer.ContentHash(manifest))
                .ToArray();
        }

        private static string[] IssueSignatures(CharacterArtIntakeManifestCatalogueBuildResult result)
        {
            return result.Issues
                .Select(issue => issue.Code + "|" + issue.Path + "|" + issue.ManifestIssueCode)
                .ToArray();
        }

        private static CharacterArtIntakeManifest Manifest(
            string suffix,
            string sourceId = null,
            string characterId = null,
            string archiveSha256 = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")
        {
            return new CharacterArtIntakeManifest(
                CharacterArtIntakeManifestValidator.SupportedSchemaVersion,
                characterId ?? "test.character." + suffix,
                sourceId ?? "test.source." + suffix + ".v1",
                CharacterBodyFamily.LargeCreature,
                "Test " + suffix + " source",
                "Test Creator",
                "https://example.com/source/" + suffix + ".zip",
                "Test Permissive License 1.0",
                "https://example.com/licenses/test-1.0",
                "Test " + suffix + " source by Test Creator.",
                "Retopologized for the test fixture.",
                archiveSha256,
                "Source/Test/" + suffix + ".fbx",
                "test.rig.large-creature.v1",
                RequiredMotionClips(suffix),
                RequiredLodBudgets(),
                3,
                1,
                1,
                1024,
                false,
                false,
                true,
                false,
                false,
                false);
        }

        private static IEnumerable<CharacterArtMotionClip> RequiredMotionClips(string suffix)
        {
            yield return Clip(CharacterArtMotionClipKey.Idle, suffix);
            yield return Clip(CharacterArtMotionClipKey.Locomotion, suffix);
            yield return Clip(CharacterArtMotionClipKey.AttackPrimary, suffix);
            yield return Clip(CharacterArtMotionClipKey.AttackAbility, suffix);
            yield return Clip(CharacterArtMotionClipKey.Hit, suffix);
            yield return Clip(CharacterArtMotionClipKey.Death, suffix);
        }

        private static CharacterArtMotionClip Clip(CharacterArtMotionClipKey key, string suffix)
        {
            return new CharacterArtMotionClip(
                key,
                "test.clip.large-creature." + suffix + "." + ClipName(key) + ".v1");
        }

        private static string ClipName(CharacterArtMotionClipKey key)
        {
            switch (key)
            {
                case CharacterArtMotionClipKey.AttackPrimary: return "attack-primary";
                case CharacterArtMotionClipKey.AttackAbility: return "attack-ability";
                default: return key.ToString().ToLowerInvariant();
            }
        }

        private static IEnumerable<CharacterArtLodTriangleBudget> RequiredLodBudgets()
        {
            yield return new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod0, 4000);
            yield return new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod1, 2000);
            yield return new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod2, 800);
        }

        private sealed class TestProvider : ICharacterArtIntakeManifestProvider
        {
            public TestProvider(string moduleId, params CharacterArtIntakeManifest[] manifests)
                : this(moduleId, (IReadOnlyList<CharacterArtIntakeManifest>)manifests)
            {
            }

            public TestProvider(
                string moduleId,
                IReadOnlyList<CharacterArtIntakeManifest> manifests)
            {
                ModuleId = moduleId;
                Manifests = manifests;
            }

            public string ModuleId { get; }
            public IReadOnlyList<CharacterArtIntakeManifest> Manifests { get; }
        }

        private sealed class ThrowingModuleProvider : ICharacterArtIntakeManifestProvider
        {
            public string ModuleId => throw new InvalidOperationException("Unreadable module fixture.");
            public IReadOnlyList<CharacterArtIntakeManifest> Manifests =>
                Array.Empty<CharacterArtIntakeManifest>();
        }

        private sealed class ThrowingManifestsProvider : ICharacterArtIntakeManifestProvider
        {
            public string ModuleId => "test.provider.throwing-property";
            public IReadOnlyList<CharacterArtIntakeManifest> Manifests =>
                throw new InvalidOperationException("Unreadable manifest property fixture.");
        }

        private sealed class ThrowingCountManifestList : IReadOnlyList<CharacterArtIntakeManifest>
        {
            public int Count => throw new InvalidOperationException("Unreadable collection fixture.");
            public CharacterArtIntakeManifest this[int index] => Manifest("unreachable");
            public IEnumerator<CharacterArtIntakeManifest> GetEnumerator() =>
                Array.Empty<CharacterArtIntakeManifest>().AsEnumerable().GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class ThrowingItemManifestList : IReadOnlyList<CharacterArtIntakeManifest>
        {
            public int Count => 1;
            public CharacterArtIntakeManifest this[int index] =>
                throw new InvalidOperationException("Unreadable item fixture.");
            public IEnumerator<CharacterArtIntakeManifest> GetEnumerator() =>
                Array.Empty<CharacterArtIntakeManifest>().AsEnumerable().GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class ThrowingProviderCollection : IEnumerable<ICharacterArtIntakeManifestProvider>
        {
            public IEnumerator<ICharacterArtIntakeManifestProvider> GetEnumerator()
            {
                throw new InvalidOperationException("Unreadable provider collection fixture.");
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
