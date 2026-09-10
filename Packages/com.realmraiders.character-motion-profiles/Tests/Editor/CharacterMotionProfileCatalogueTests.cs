using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterMotionProfiles.Tests
{
    public sealed class CharacterMotionProfileCatalogueTests
    {
        [Test]
        public void Build_SnapshotsAndSortsProfilesWithExactLookupAndCanonicalOrder()
        {
            var alpha = Profile("alpha");
            var middle = Profile("middle");
            var zeta = Profile("zeta");
            var mutableProfiles = new List<CharacterMotionProfile> { zeta, alpha };
            var mutableProviders = new List<ICharacterMotionProfileProvider>
            {
                new TestProvider("test.provider.zeta", mutableProfiles),
                new TestProvider("test.provider.middle", middle)
            };
            var first = CharacterMotionProfileCatalogue.Build(mutableProviders);
            var reordered = CharacterMotionProfileCatalogue.Build(new ICharacterMotionProfileProvider[]
            {
                new TestProvider("test.provider.middle", middle),
                new TestProvider("test.provider.zeta", alpha, zeta)
            });

            Assert.That(first.Succeeded, Is.True);
            Assert.That(first.Issues, Is.Empty);
            Assert.That(first.Catalogue.Profiles.Select(profile => profile.MotionProfileId), Is.EqualTo(new[]
            {
                "test.motion.alpha.v1",
                "test.motion.middle.v1",
                "test.motion.zeta.v1"
            }));
            Assert.That(CanonicalSignatures(reordered.Catalogue), Is.EqualTo(CanonicalSignatures(first.Catalogue)));
            Assert.That(first.Catalogue.TryGetByMotionProfileId("test.motion.middle.v1", out var found), Is.True);
            Assert.That(found, Is.SameAs(middle));
            Assert.That(first.Catalogue.TryGetByMotionProfileId("TEST.MOTION.MIDDLE.V1", out _), Is.False);
            Assert.That(first.Catalogue.TryGetByMotionProfileId(null, out _), Is.False);

            mutableProfiles.Clear();
            mutableProfiles.Add(Profile("late"));
            mutableProviders.Clear();
            Assert.That(first.Catalogue.Profiles.Count, Is.EqualTo(3));
            Assert.That(first.Catalogue.TryGetByMotionProfileId("test.motion.alpha.v1", out var snapshotted), Is.True);
            Assert.That(snapshotted, Is.SameAs(alpha));

            var lookupFields = typeof(CharacterMotionProfileCatalogue)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Count(field => field.FieldType.IsGenericType &&
                    field.FieldType.GetGenericTypeDefinition() == typeof(System.Collections.ObjectModel.ReadOnlyDictionary<,>));
            Assert.That(lookupFields, Is.EqualTo(1));
        }

        [Test]
        public void Build_DuplicateModuleAndMotionProfileIdsFailClosedDeterministically()
        {
            var providers = new ICharacterMotionProfileProvider[]
            {
                new TestProvider("test.provider.duplicate", Profile("alpha", "test.motion.shared.v1")),
                new TestProvider("test.provider.duplicate", Profile("beta")),
                new TestProvider("test.provider.unique", Profile("gamma", "test.motion.shared.v1"))
            };

            var first = CharacterMotionProfileCatalogue.Build(providers);
            var reordered = CharacterMotionProfileCatalogue.Build(providers.Reverse());

            Assert.That(first.Succeeded, Is.False);
            Assert.That(first.Catalogue, Is.Null);
            var codes = first.Issues.Select(issue => issue.Code).ToArray();
            Assert.That(codes, Has.Member(CharacterMotionProfileCatalogueIssueCode.DuplicateProviderModuleId));
            Assert.That(codes, Has.Member(CharacterMotionProfileCatalogueIssueCode.DuplicateMotionProfileId));
            Assert.That(IssueSignatures(reordered), Is.EqualTo(IssueSignatures(first)));
        }

        [Test]
        public void Build_NullUnreadableAndInvalidInputsReturnSortedStructuredIssues()
        {
            var providers = new ICharacterMotionProfileProvider[]
            {
                null,
                new TestProvider("Bad/Provider", Profile("valid")),
                new TestProvider("test.provider.null-profiles", (IReadOnlyList<CharacterMotionProfile>)null),
                new TestProvider("test.provider.unreadable-profiles", new UnreadableProfileList()),
                new TestProvider("test.provider.invalid-profiles", new CharacterMotionProfile[]
                {
                    null,
                    Profile("invalid", "Bad/Profile")
                }),
                new UnreadableProvider()
            };

            var first = CharacterMotionProfileCatalogue.Build(providers);
            var reordered = CharacterMotionProfileCatalogue.Build(providers.Reverse());
            var codes = first.Issues.Select(issue => issue.Code).ToArray();

            Assert.That(first.Succeeded, Is.False);
            Assert.That(first.Catalogue, Is.Null);
            Assert.That(codes, Has.Member(CharacterMotionProfileCatalogueIssueCode.NullProvider));
            Assert.That(codes, Has.Member(CharacterMotionProfileCatalogueIssueCode.UnreadableProvider));
            Assert.That(codes, Has.Member(CharacterMotionProfileCatalogueIssueCode.InvalidModuleId));
            Assert.That(codes, Has.Member(CharacterMotionProfileCatalogueIssueCode.NullProfileCollection));
            Assert.That(codes, Has.Member(CharacterMotionProfileCatalogueIssueCode.UnreadableProfileCollection));
            Assert.That(codes, Has.Member(CharacterMotionProfileCatalogueIssueCode.NullProfile));
            Assert.That(codes, Has.Member(CharacterMotionProfileCatalogueIssueCode.InvalidProfile));
            Assert.That(first.Issues.Single(issue => issue.Code == CharacterMotionProfileCatalogueIssueCode.InvalidProfile).ProfileIssueCode,
                Is.EqualTo(MotionProfileIssueCode.InvalidId));
            Assert.That(IssueSignatures(reordered), Is.EqualTo(IssueSignatures(first)));
            Assert.That(first.Issues, Is.EqualTo(first.Issues
                .OrderBy(issue => issue.Path, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code)
                .ThenBy(issue => issue.ProfileIssueCode)
                .ToArray()));

            var missing = CharacterMotionProfileCatalogue.Build(null);
            Assert.That(missing.Issues.Single().Code,
                Is.EqualTo(CharacterMotionProfileCatalogueIssueCode.NullProviderCollection));

            var unreadable = CharacterMotionProfileCatalogue.Build(new UnreadableProviderCollection());
            Assert.That(unreadable.Issues.Single().Code,
                Is.EqualTo(CharacterMotionProfileCatalogueIssueCode.UnreadableProviderCollection));
        }

        [Test]
        public void CatalogueContractsAndCollections_AreImmutableWithoutUnityOrGameRuntimeDependency()
        {
            var success = CharacterMotionProfileCatalogue.Build(new[]
            {
                new TestProvider("test.provider.alpha", Profile("alpha"))
            });
            var failure = CharacterMotionProfileCatalogue.Build(new ICharacterMotionProfileProvider[] { null });

            Assert.That(typeof(ICharacterMotionProfileProvider).GetProperty("ModuleId").CanWrite, Is.False);
            Assert.That(typeof(ICharacterMotionProfileProvider).GetProperty("Profiles").CanWrite, Is.False);
            Assert.That(typeof(CharacterMotionProfileCatalogue).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterMotionProfileCatalogueBuildResult).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterMotionProfileCatalogueIssue).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.Throws<NotSupportedException>(() => ((IList<CharacterMotionProfile>)success.Catalogue.Profiles).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<CharacterMotionProfileCatalogueIssue>)failure.Issues).Clear());

            var dependencies = typeof(CharacterMotionProfileCatalogue).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();
            Assert.That(dependencies, Has.Member("RealmRaiders.ModuleContracts"));
            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static string[] CanonicalSignatures(CharacterMotionProfileCatalogue catalogue)
        {
            return catalogue.Profiles
                .Select(profile => profile.MotionProfileId + "|" + CharacterMotionProfileCanonicalizer.ContentHash(profile))
                .ToArray();
        }

        private static string[] IssueSignatures(CharacterMotionProfileCatalogueBuildResult result)
        {
            return result.Issues
                .Select(issue => issue.Code + "|" + issue.Path + "|" + issue.ProfileIssueCode)
                .ToArray();
        }

        private static CharacterMotionProfile Profile(string suffix, string profileId = null)
        {
            return new CharacterMotionProfile(
                CharacterMotionProfileValidator.SupportedSchemaVersion,
                profileId ?? "test.motion." + suffix + ".v1",
                CharacterBodyFamily.Beast,
                "test.rig.beast.v1",
                "test.animator.beast.v1",
                RequiredClips(suffix),
                MotionRhythm.Neutral,
                "test.motion.fallback.v1",
                new[] { "test.source." + suffix + ".v1" });
        }

        private static IEnumerable<MotionClipBinding> RequiredClips(string suffix)
        {
            yield return Clip(MotionClipKey.Idle, suffix);
            yield return Clip(MotionClipKey.Locomotion, suffix);
            yield return Clip(MotionClipKey.JumpTakeoff, suffix);
            yield return Clip(MotionClipKey.JumpFall, suffix);
            yield return Clip(MotionClipKey.JumpLand, suffix);
            yield return Clip(MotionClipKey.AttackPrimary, suffix);
            yield return Clip(MotionClipKey.AttackAbility, suffix);
            yield return Clip(MotionClipKey.Hit, suffix);
            yield return Clip(MotionClipKey.Death, suffix);
        }

        private static MotionClipBinding Clip(MotionClipKey key, string suffix)
        {
            return new MotionClipBinding(
                key,
                "test.clip." + suffix + "." + ClipName(key) + ".v1",
                CharacterBodyFamily.Beast,
                "test.rig.beast.v1",
                key);
        }

        private static string ClipName(MotionClipKey key)
        {
            switch (key)
            {
                case MotionClipKey.AttackPrimary: return "attack-primary";
                case MotionClipKey.AttackAbility: return "attack-ability";
                default: return key.ToString().ToLowerInvariant();
            }
        }

        private sealed class TestProvider : ICharacterMotionProfileProvider
        {
            public TestProvider(string moduleId, params CharacterMotionProfile[] profiles)
                : this(moduleId, (IReadOnlyList<CharacterMotionProfile>)profiles)
            {
            }

            public TestProvider(string moduleId, IReadOnlyList<CharacterMotionProfile> profiles)
            {
                ModuleId = moduleId;
                Profiles = profiles;
            }

            public string ModuleId { get; }
            public IReadOnlyList<CharacterMotionProfile> Profiles { get; }
        }

        private sealed class UnreadableProvider : ICharacterMotionProfileProvider
        {
            public string ModuleId => throw new InvalidOperationException("Unreadable provider fixture.");
            public IReadOnlyList<CharacterMotionProfile> Profiles => new CharacterMotionProfile[0];
        }

        private sealed class UnreadableProviderCollection : IEnumerable<ICharacterMotionProfileProvider>
        {
            public IEnumerator<ICharacterMotionProfileProvider> GetEnumerator()
            {
                throw new InvalidOperationException("Unreadable provider collection fixture.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private sealed class UnreadableProfileList : IReadOnlyList<CharacterMotionProfile>
        {
            public int Count => 1;
            public CharacterMotionProfile this[int index] => throw new InvalidOperationException("Unreadable profile fixture.");

            public IEnumerator<CharacterMotionProfile> GetEnumerator()
            {
                throw new InvalidOperationException("Unreadable profile collection fixture.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
