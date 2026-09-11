using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.CharacterProceduralMotion.Tests
{
    public sealed class ProceduralHumanoidTuningCatalogueTests
    {
        [Test]
        public void Build_SnapshotsSortsAndUsesExactProfileLookup()
        {
            var alpha = Profile("test.procedural-humanoid.alpha.v1");
            var middle = Profile("test.procedural-humanoid.middle.v1");
            var zeta = Profile("test.procedural-humanoid.zeta.v1");
            var mutableProfiles = new List<ProceduralHumanoidTuningProfile>
            {
                zeta,
                alpha
            };
            var mutableProviders = new List<IProceduralHumanoidTuningProvider>
            {
                new TestProvider("test.provider.zeta", mutableProfiles),
                new TestProvider("test.provider.middle", middle)
            };

            var first = ProceduralHumanoidTuningCatalogue.Build(mutableProviders);
            var reordered = ProceduralHumanoidTuningCatalogue.Build(
                new IProceduralHumanoidTuningProvider[]
                {
                    new TestProvider("test.provider.middle", middle),
                    new TestProvider("test.provider.zeta", alpha, zeta)
                });

            Assert.That(first.Succeeded, Is.True);
            Assert.That(first.Issues, Is.Empty);
            Assert.That(first.Catalogue.Profiles.Select(profile => profile.ProfileId), Is.EqualTo(
                new[]
                {
                    "test.procedural-humanoid.alpha.v1",
                    "test.procedural-humanoid.middle.v1",
                    "test.procedural-humanoid.zeta.v1"
                }));
            Assert.That(ProfileSignatures(reordered.Catalogue), Is.EqualTo(
                ProfileSignatures(first.Catalogue)));
            Assert.That(first.Catalogue.TryGetByProfileId(
                "test.procedural-humanoid.middle.v1",
                out var found), Is.True);
            Assert.That(found, Is.SameAs(middle));
            Assert.That(first.Catalogue.TryGetByProfileId(
                "TEST.PROCEDURAL-HUMANOID.MIDDLE.V1",
                out _), Is.False);
            Assert.That(first.Catalogue.TryGetByProfileId(null, out _), Is.False);

            mutableProfiles.Clear();
            mutableProfiles.Add(Profile("test.procedural-humanoid.late.v1"));
            mutableProviders.Clear();
            Assert.That(first.Catalogue.Profiles.Count, Is.EqualTo(3));
            Assert.That(first.Catalogue.TryGetByProfileId(
                "test.procedural-humanoid.alpha.v1",
                out var snapshotted), Is.True);
            Assert.That(snapshotted, Is.SameAs(alpha));
        }

        [Test]
        public void Build_DuplicateProviderAndProfileIdsFailClosedDeterministically()
        {
            var providers = new IProceduralHumanoidTuningProvider[]
            {
                new TestProvider(
                    "test.provider.duplicate",
                    Profile("test.procedural-humanoid.shared.v1")),
                new TestProvider(
                    "test.provider.duplicate",
                    Profile("test.procedural-humanoid.beta.v1")),
                new TestProvider(
                    "test.provider.unique",
                    Profile("test.procedural-humanoid.shared.v1"))
            };

            var first = ProceduralHumanoidTuningCatalogue.Build(providers);
            var reordered = ProceduralHumanoidTuningCatalogue.Build(providers.Reverse());

            Assert.That(first.Succeeded, Is.False);
            Assert.That(first.Catalogue, Is.Null);
            Assert.That(first.Issues.Select(issue => issue.Code), Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.DuplicateProviderId));
            Assert.That(first.Issues.Select(issue => issue.Code), Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.DuplicateProfileId));
            Assert.That(IssueSignatures(reordered), Is.EqualTo(IssueSignatures(first)));
        }

        [Test]
        public void Build_NullUnreadableAndInvalidInputsReturnSortedStructuredIssues()
        {
            var providers = new IProceduralHumanoidTuningProvider[]
            {
                null,
                new TestProvider("Bad/Provider", Profile("test.procedural-humanoid.valid.v1")),
                new TestProvider(
                    "test.provider.null-profiles",
                    (IReadOnlyList<ProceduralHumanoidTuningProfile>)null),
                new TestProvider("test.provider.unreadable-profiles", new UnreadableProfileList()),
                new TestProvider(
                    "test.provider.invalid-profiles",
                    new ProceduralHumanoidTuningProfile[]
                    {
                        null,
                        Profile("Bad/Profile"),
                        new ProceduralHumanoidTuningProfile(
                            "test.procedural-humanoid.null-tuning.v1",
                            null)
                    }),
                new UnreadableProvider()
            };

            var first = ProceduralHumanoidTuningCatalogue.Build(providers);
            var reordered = ProceduralHumanoidTuningCatalogue.Build(providers.Reverse());
            var codes = first.Issues.Select(issue => issue.Code).ToArray();

            Assert.That(first.Succeeded, Is.False);
            Assert.That(first.Catalogue, Is.Null);
            Assert.That(codes, Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.NullProvider));
            Assert.That(codes, Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.UnreadableProvider));
            Assert.That(codes, Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.InvalidProviderId));
            Assert.That(codes, Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.NullProfileCollection));
            Assert.That(codes, Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.UnreadableProfileCollection));
            Assert.That(codes, Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.NullProfile));
            Assert.That(codes, Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.InvalidProfileId));
            Assert.That(codes, Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.NullTuning));
            Assert.That(IssueSignatures(reordered), Is.EqualTo(IssueSignatures(first)));
            Assert.That(first.Issues, Is.EqualTo(first.Issues
                .OrderBy(issue => issue.Path, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code)
                .ToArray()));

            var missing = ProceduralHumanoidTuningCatalogue.Build(null);
            Assert.That(missing.Issues.Single().Code, Is.EqualTo(
                ProceduralHumanoidTuningCatalogueIssueCode.NullProviderCollection));

            var unreadable = ProceduralHumanoidTuningCatalogue.Build(
                new UnreadableProviderCollection());
            Assert.That(unreadable.Issues.Single().Code, Is.EqualTo(
                ProceduralHumanoidTuningCatalogueIssueCode.UnreadableProviderCollection));
        }

        [Test]
        public void StarterProviderPreservesExistingTuningIdentityAndValues()
        {
            var provider = new StarterProceduralHumanoidTuningProvider();
            var result = ProceduralHumanoidTuningCatalogue.Build(
                new IProceduralHumanoidTuningProvider[]
                {
                    provider
                });

            Assert.That(provider.ProviderId, Is.EqualTo(
                StarterProceduralHumanoidTuningProvider.StarterProviderId));
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Catalogue.TryGetByProfileId(
                StarterProceduralHumanoidTuningProvider.CompatibilityProfileId,
                out var compatibility), Is.True);
            Assert.That(result.Catalogue.TryGetByProfileId(
                StarterProceduralHumanoidTuningProvider.BloodKnightDeviceReadableProfileId,
                out var bloodKnight), Is.True);
            Assert.That(compatibility.Tuning, Is.SameAs(
                ProceduralHumanoidMotionTuning.CompatibilityDefault));
            Assert.That(bloodKnight.Tuning, Is.SameAs(
                ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable));

            Assert.That(compatibility.Tuning.SwingCadenceRadiansPerSecond, Is.EqualTo(6f));
            Assert.That(compatibility.Tuning.Locomotion.UpperArmDegrees, Is.EqualTo(22f));
            Assert.That(compatibility.Tuning.JumpTakeoff.ThighDegrees, Is.EqualTo(-10f));
            Assert.That(compatibility.Tuning.AbilityAttack.UpperArmDegrees, Is.EqualTo(-22f));
            Assert.That(compatibility.Tuning.Hit.UpperArmDegrees, Is.EqualTo(10f));

            Assert.That(bloodKnight.Tuning.SwingCadenceRadiansPerSecond, Is.EqualTo(7.5f));
            Assert.That(bloodKnight.Tuning.Locomotion.UpperArmDegrees, Is.EqualTo(56f));
            Assert.That(bloodKnight.Tuning.AsymmetricJumpTakeoff.LeftThighDegrees, Is.EqualTo(-22f));
            Assert.That(bloodKnight.Tuning.AbilityAttack.UpperArmDegrees, Is.EqualTo(-28f));
            Assert.That(bloodKnight.Tuning.Hit.UpperArmDegrees, Is.EqualTo(14f));
        }

        [Test]
        public void PublicContractsAreImmutableAndRequireNoAutomaticApplication()
        {
            var success = ProceduralHumanoidTuningCatalogue.Build(
                new IProceduralHumanoidTuningProvider[]
                {
                    new TestProvider(
                        "test.provider.alpha",
                        Profile("test.procedural-humanoid.alpha.v1"))
                });
            var failure = ProceduralHumanoidTuningCatalogue.Build(
                new IProceduralHumanoidTuningProvider[]
                {
                    null
                });

            Assert.That(typeof(IProceduralHumanoidTuningProvider)
                .GetProperty("ProviderId").CanWrite, Is.False);
            Assert.That(typeof(IProceduralHumanoidTuningProvider)
                .GetProperty("Profiles").CanWrite, Is.False);
            Assert.That(typeof(ProceduralHumanoidTuningProfile)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ProceduralHumanoidTuningCatalogue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ProceduralHumanoidTuningCatalogueBuildResult)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ProceduralHumanoidTuningCatalogueIssue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<ProceduralHumanoidTuningProfile>)success.Catalogue.Profiles).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<ProceduralHumanoidTuningCatalogueIssue>)failure.Issues).Clear());
        }

        private static string[] ProfileSignatures(
            ProceduralHumanoidTuningCatalogue catalogue)
        {
            return catalogue.Profiles.Select(profile => profile.ProfileId).ToArray();
        }

        private static string[] IssueSignatures(
            ProceduralHumanoidTuningCatalogueBuildResult result)
        {
            return result.Issues.Select(issue => issue.Signature).ToArray();
        }

        private static ProceduralHumanoidTuningProfile Profile(string profileId)
        {
            return new ProceduralHumanoidTuningProfile(
                profileId,
                ProceduralHumanoidMotionTuning.CompatibilityDefault);
        }

        private sealed class TestProvider : IProceduralHumanoidTuningProvider
        {
            public TestProvider(
                string providerId,
                params ProceduralHumanoidTuningProfile[] profiles)
                : this(providerId, (IReadOnlyList<ProceduralHumanoidTuningProfile>)profiles)
            {
            }

            public TestProvider(
                string providerId,
                IReadOnlyList<ProceduralHumanoidTuningProfile> profiles)
            {
                ProviderId = providerId;
                Profiles = profiles;
            }

            public string ProviderId { get; }

            public IReadOnlyList<ProceduralHumanoidTuningProfile> Profiles { get; }
        }

        private sealed class UnreadableProvider : IProceduralHumanoidTuningProvider
        {
            public string ProviderId => throw new InvalidOperationException(
                "Unreadable provider fixture.");

            public IReadOnlyList<ProceduralHumanoidTuningProfile> Profiles =>
                new ProceduralHumanoidTuningProfile[0];
        }

        private sealed class UnreadableProviderCollection :
            IEnumerable<IProceduralHumanoidTuningProvider>
        {
            public IEnumerator<IProceduralHumanoidTuningProvider> GetEnumerator()
            {
                throw new InvalidOperationException("Unreadable provider collection fixture.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private sealed class UnreadableProfileList :
            IReadOnlyList<ProceduralHumanoidTuningProfile>
        {
            public int Count => 1;

            public ProceduralHumanoidTuningProfile this[int index] =>
                throw new InvalidOperationException("Unreadable profile fixture.");

            public IEnumerator<ProceduralHumanoidTuningProfile> GetEnumerator()
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
