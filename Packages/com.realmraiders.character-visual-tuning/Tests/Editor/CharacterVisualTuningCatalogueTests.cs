using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.CharacterVisualTuning.Tests
{
    public sealed class CharacterVisualTuningCatalogueTests
    {
        private static readonly string[] ExpectedStarterProfileIds =
        {
            BloodKnightBaseline.ProfileId,
            GuardianEntPrototypeProfile.ProfileId,
            SylvanWolfPrototypeProfile.ProfileId,
            InfernalBrutePrototypeProfile.ProfileId,
            HellhoundPrototypeProfile.ProfileId
        };

        private static readonly string[] ExpectedStarterSourceIds =
        {
            BloodKnightBaseline.SourceId,
            GuardianEntPrototypeProfile.SourceId,
            SylvanWolfPrototypeProfile.SourceId,
            InfernalBrutePrototypeProfile.SourceId,
            HellhoundPrototypeProfile.SourceId
        };

        [Test]
        public void StarterProvider_ReturnsExactExistingFiveWithoutChangingPayloads()
        {
            var provider = new StarterCharacterVisualTuningProvider();
            var existingFactories = new[]
            {
                BloodKnightBaseline.Create(),
                GuardianEntPrototypeProfile.Create(),
                SylvanWolfPrototypeProfile.Create(),
                InfernalBrutePrototypeProfile.Create(),
                HellhoundPrototypeProfile.Create()
            };

            Assert.That(provider.ModuleId, Is.EqualTo("realmraiders.starter-character-visual-tuning"));
            Assert.That(provider.Profiles.Count, Is.EqualTo(5));
            Assert.That(
                provider.Profiles.Select(profile => profile.ProfileId),
                Is.EqualTo(ExpectedStarterProfileIds));
            Assert.That(
                provider.Profiles.Select(profile => profile.SourceId),
                Is.EqualTo(ExpectedStarterSourceIds));
            Assert.That(provider.Profiles[2].ProfileId,
                Is.EqualTo("realmraiders.sylvan-wolf.prototype"));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterVisualTuningProfile>)provider.Profiles).Clear());

            for (var index = 0; index < provider.Profiles.Count; index++)
            {
                AssertSamePayload(existingFactories[index], provider.Profiles[index]);
                Assert.That(VisualTuningValidator.Validate(provider.Profiles[index]),
                    Is.Empty,
                    provider.Profiles[index].ProfileId);
            }
        }

        [Test]
        public void Catalogue_SortsTheExactStarterSetByOrdinalProfileId()
        {
            var result = CharacterVisualTuningCatalogue.Build(new[]
            {
                new StarterCharacterVisualTuningProvider()
            });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Issues, Is.Empty);
            Assert.That(result.Catalogue.Profiles.Select(profile => profile.ProfileId),
                Is.EqualTo(ExpectedStarterProfileIds.OrderBy(id => id, StringComparer.Ordinal)));
            Assert.That(result.Catalogue.Profiles.Select(profile => profile.SourceId).Distinct().Count(),
                Is.EqualTo(5));
        }

        [Test]
        public void ProfileAndSourceLookup_IsExactCaseSensitiveAndHasNoFallback()
        {
            var catalogue = CharacterVisualTuningCatalogue.Build(new[]
            {
                new StarterCharacterVisualTuningProvider()
            }).Catalogue;

            Assert.That(catalogue.TryGetByProfileId(
                "realmraiders.sylvan-wolf.prototype",
                out var wolf), Is.True);
            Assert.That(wolf.SourceId,
                Is.EqualTo("realmraiders.source.sylvan-wolf-prototype"));
            Assert.That(catalogue.TryGetBySourceId(
                "3drt.fantasy-warrior",
                out var bloodKnight), Is.True);
            Assert.That(bloodKnight.ProfileId, Is.EqualTo(BloodKnightBaseline.ProfileId));
            Assert.That(catalogue.TryGetByProfileId(
                "realmraiders.Sylvan-wolf.prototype",
                out _), Is.False);
            Assert.That(catalogue.TryGetByProfileId(
                "realmraiders.sylvanwolf.prototype",
                out _), Is.False);
            Assert.That(catalogue.TryGetBySourceId("3DRT.fantasy-warrior", out _), Is.False);
            Assert.That(catalogue.TryGetByProfileId(null, out _), Is.False);
            Assert.That(catalogue.TryGetBySourceId(null, out _), Is.False);
        }

        [Test]
        public void ProviderAndProfileReordering_ProducesTheSameCatalogue()
        {
            var firstProfiles = new List<CharacterVisualTuningProfile>
            {
                ValidProfile("zeta"),
                ValidProfile("alpha")
            };
            var secondProfiles = new List<CharacterVisualTuningProfile>
            {
                ValidProfile("middle")
            };
            var first = CharacterVisualTuningCatalogue.Build(new ICharacterVisualTuningProfileProvider[]
            {
                new TestProvider("test.module.zeta", firstProfiles),
                new TestProvider("test.module.alpha", secondProfiles)
            });
            var second = CharacterVisualTuningCatalogue.Build(new ICharacterVisualTuningProfileProvider[]
            {
                new TestProvider("test.module.alpha", secondProfiles.AsEnumerable().Reverse().ToList()),
                new TestProvider("test.module.zeta", firstProfiles.AsEnumerable().Reverse().ToList())
            });

            Assert.That(first.Succeeded, Is.True);
            Assert.That(second.Succeeded, Is.True);
            Assert.That(CatalogueSignatures(first.Catalogue),
                Is.EqualTo(CatalogueSignatures(second.Catalogue)));
            Assert.That(first.Catalogue.Profiles.Select(profile => profile.ProfileId),
                Is.EqualTo(new[]
                {
                    "test.profile.alpha",
                    "test.profile.middle",
                    "test.profile.zeta"
                }));
        }

        [Test]
        public void DuplicateModuleProfileAndSourceIds_FailClosedDeterministically()
        {
            var providers = new ICharacterVisualTuningProfileProvider[]
            {
                new TestProvider("test.module.duplicate", new[] { ValidProfile("one") }),
                new TestProvider("test.module.duplicate", new[] { ValidProfile("two") }),
                new TestProvider("test.module.profile-a", new[]
                {
                    ValidProfile("profile-a", profileId: "test.profile.shared")
                }),
                new TestProvider("test.module.profile-b", new[]
                {
                    ValidProfile("profile-b", profileId: "test.profile.shared")
                }),
                new TestProvider("test.module.source-a", new[]
                {
                    ValidProfile("source-a", sourceId: "test.source.shared")
                }),
                new TestProvider("test.module.source-b", new[]
                {
                    ValidProfile("source-b", sourceId: "test.source.shared")
                })
            };

            var first = CharacterVisualTuningCatalogue.Build(providers);
            var second = CharacterVisualTuningCatalogue.Build(providers.Reverse());

            Assert.That(first.Succeeded, Is.False);
            Assert.That(first.Catalogue, Is.Null);
            Assert.That(Codes(first), Does.Contain(
                CharacterVisualTuningCatalogueIssueCode.DuplicateProviderModuleId));
            Assert.That(Codes(first), Does.Contain(
                CharacterVisualTuningCatalogueIssueCode.DuplicateProfileId));
            Assert.That(Codes(first), Does.Contain(
                CharacterVisualTuningCatalogueIssueCode.DuplicateSourceId));
            Assert.That(IssueSignatures(first), Is.EqualTo(IssueSignatures(second)));
        }

        [Test]
        public void NullMalformedAndInvalidInputs_ReturnSortedStructuredIssues()
        {
            Assert.That(
                Codes(CharacterVisualTuningCatalogue.Build(null)),
                Does.Contain(CharacterVisualTuningCatalogueIssueCode.NullProviderCollection));

            var invalidProfile = new CharacterVisualTuningProfile(
                "Bad/profile",
                "bad..source",
                " ",
                null,
                null,
                null);
            var result = CharacterVisualTuningCatalogue.Build(
                new ICharacterVisualTuningProfileProvider[]
                {
                    null,
                    new TestProvider("Bad/module", new[] { ValidProfile("valid") }),
                    new TestProvider("test.module.null-profiles", null),
                    new TestProvider("test.module.invalid-profiles", new CharacterVisualTuningProfile[]
                    {
                        null,
                        invalidProfile
                    })
                });

            Assert.That(result.Succeeded, Is.False);
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.NullProvider));
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.InvalidProviderModuleId));
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.NullProfileCollection));
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.NullProfile));
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.InvalidProfileId));
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.InvalidSourceId));
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.InvalidProfile));
            Assert.That(result.Issues.Any(issue =>
                issue.ProfileIssueCode == VisualTuningIssueCode.MissingBudget), Is.True);
            Assert.That(result.Issues, Is.EqualTo(result.Issues
                .OrderBy(issue => issue.Path, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code)
                .ThenBy(issue => issue.ProfileIssueCode)
                .ToArray()));
        }

        [Test]
        public void ThrowingProviderCollectionsAndProfiles_FailWithoutExceptionLeak()
        {
            CharacterVisualTuningCatalogueBuildResult unreadableProviders = null;
            Assert.DoesNotThrow(() => unreadableProviders = CharacterVisualTuningCatalogue.Build(
                new ThrowingEnumerable<ICharacterVisualTuningProfileProvider>()));
            Assert.That(Codes(unreadableProviders), Does.Contain(
                CharacterVisualTuningCatalogueIssueCode.UnreadableProviderCollection));

            var providers = new ICharacterVisualTuningProfileProvider[]
            {
                new ThrowingProvider(throwModuleId: true),
                new ThrowingProvider(throwProfiles: true),
                new TestProvider(
                    "test.module.unreadable-collection",
                    new ThrowingReadOnlyList<CharacterVisualTuningProfile>(throwCount: true)),
                new TestProvider(
                    "test.module.unreadable-profile",
                    new ThrowingReadOnlyList<CharacterVisualTuningProfile>(throwCount: false))
            };

            CharacterVisualTuningCatalogueBuildResult result = null;
            Assert.DoesNotThrow(() => result = CharacterVisualTuningCatalogue.Build(providers));
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.UnreadableProvider));
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.UnreadableProfileCollection));
            Assert.That(Codes(result), Does.Contain(CharacterVisualTuningCatalogueIssueCode.UnreadableProfile));
        }

        [Test]
        public void CatalogueSnapshotsAreImmutableAndHaveNoUnityOrGameDependency()
        {
            var mutableProfiles = new List<CharacterVisualTuningProfile>
            {
                ValidProfile("isolated")
            };
            var mutableProviders = new List<ICharacterVisualTuningProfileProvider>
            {
                new TestProvider("test.module.isolated", mutableProfiles)
            };

            var result = CharacterVisualTuningCatalogue.Build(mutableProviders);
            mutableProfiles.Clear();
            mutableProviders.Clear();

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Catalogue.Profiles.Count, Is.EqualTo(1));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterVisualTuningProfile>)result.Catalogue.Profiles).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterVisualTuningCatalogueIssue>)result.Issues).Clear());
            Assert.That(typeof(ICharacterVisualTuningProfileProvider)
                .GetProperties().All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterVisualTuningCatalogue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterVisualTuningCatalogueIssue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);

            var dependencies = typeof(CharacterVisualTuningCatalogue).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();
            Assert.That(dependencies.Any(name =>
                name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static CharacterVisualTuningCatalogueIssueCode[] Codes(
            CharacterVisualTuningCatalogueBuildResult result)
        {
            return result.Issues.Select(issue => issue.Code).ToArray();
        }

        private static string[] IssueSignatures(
            CharacterVisualTuningCatalogueBuildResult result)
        {
            return result.Issues.Select(issue =>
                issue.Path + "|" + issue.Code + "|" + issue.ProfileIssueCode).ToArray();
        }

        private static string[] CatalogueSignatures(CharacterVisualTuningCatalogue catalogue)
        {
            return catalogue.Profiles.Select(profile =>
                profile.ProfileId + "|" + profile.SourceId + "|" + profile.DisplayName).ToArray();
        }

        private static CharacterVisualTuningProfile ValidProfile(
            string suffix,
            string profileId = null,
            string sourceId = null)
        {
            return new CharacterVisualTuningProfile(
                profileId ?? "test.profile." + suffix,
                sourceId ?? "test.source." + suffix,
                "Test " + suffix,
                new PresentationTransformIntent(),
                new[]
                {
                    new MaterialPaletteIntent(MaterialRole.PrimaryArmor, "Test direction " + suffix)
                },
                new MobileVisualBudget(1, 1, 256, 100));
        }

        private static void AssertSamePayload(
            CharacterVisualTuningProfile expected,
            CharacterVisualTuningProfile actual)
        {
            Assert.That(actual.ProfileId, Is.EqualTo(expected.ProfileId));
            Assert.That(actual.SourceId, Is.EqualTo(expected.SourceId));
            Assert.That(actual.DisplayName, Is.EqualTo(expected.DisplayName));
            Assert.That(actual.PresentationTransform.LocalPosition,
                Is.EqualTo(expected.PresentationTransform.LocalPosition));
            Assert.That(actual.PresentationTransform.LocalEulerAngles,
                Is.EqualTo(expected.PresentationTransform.LocalEulerAngles));
            Assert.That(actual.PresentationTransform.LocalScale,
                Is.EqualTo(expected.PresentationTransform.LocalScale));
            Assert.That(actual.Palette.Select(intent => intent.Role),
                Is.EqualTo(expected.Palette.Select(intent => intent.Role)));
            Assert.That(actual.Palette.Select(intent => intent.Direction),
                Is.EqualTo(expected.Palette.Select(intent => intent.Direction)));
            Assert.That(actual.Budget.MaterialCount, Is.EqualTo(expected.Budget.MaterialCount));
            Assert.That(actual.Budget.TextureCount, Is.EqualTo(expected.Budget.TextureCount));
            Assert.That(actual.Budget.MaxTextureEdgePixels,
                Is.EqualTo(expected.Budget.MaxTextureEdgePixels));
            Assert.That(actual.Budget.TriangleCount, Is.EqualTo(expected.Budget.TriangleCount));
        }

        private sealed class TestProvider : ICharacterVisualTuningProfileProvider
        {
            public TestProvider(
                string moduleId,
                IReadOnlyList<CharacterVisualTuningProfile> profiles)
            {
                ModuleId = moduleId;
                Profiles = profiles;
            }

            public string ModuleId { get; }
            public IReadOnlyList<CharacterVisualTuningProfile> Profiles { get; }
        }

        private sealed class ThrowingProvider : ICharacterVisualTuningProfileProvider
        {
            private readonly bool throwModuleId;
            private readonly bool throwProfiles;

            public ThrowingProvider(bool throwModuleId = false, bool throwProfiles = false)
            {
                this.throwModuleId = throwModuleId;
                this.throwProfiles = throwProfiles;
            }

            public string ModuleId => throwModuleId
                ? throw new InvalidOperationException("Synthetic unreadable module ID.")
                : "test.module.throwing";

            public IReadOnlyList<CharacterVisualTuningProfile> Profiles => throwProfiles
                ? throw new InvalidOperationException("Synthetic unreadable profiles.")
                : Array.Empty<CharacterVisualTuningProfile>();
        }

        private sealed class ThrowingEnumerable<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                throw new InvalidOperationException("Synthetic unreadable enumerable.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private sealed class ThrowingReadOnlyList<T> : IReadOnlyList<T>
        {
            private readonly bool throwCount;

            public ThrowingReadOnlyList(bool throwCount)
            {
                this.throwCount = throwCount;
            }

            public int Count => throwCount
                ? throw new InvalidOperationException("Synthetic unreadable count.")
                : 1;

            public T this[int index] =>
                throw new InvalidOperationException("Synthetic unreadable item.");

            public IEnumerator<T> GetEnumerator()
            {
                throw new InvalidOperationException("Enumeration is not supported.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
