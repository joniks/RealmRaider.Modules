using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.CharacterProceduralMotion.Tests
{
    public sealed class ProceduralHumanoidTuningAssignmentTests
    {
        const string CharacterId = "realmraiders.blood-knight.prototype";
        const string PreferredId = "realmraiders.procedural-humanoid.blood-knight-device-readable.v1";
        const string FallbackId = "realmraiders.procedural-humanoid.compatibility.v1";

        [Test]
        public void Resolve_UsesPreferredThenFallbackThenRejectedInOrder()
        {
            var preferred = Profile(
                PreferredId,
                ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
            var fallback = Profile(
                FallbackId,
                ProceduralHumanoidMotionTuning.CompatibilityDefault);
            var assignment = Assignment();

            var preferredResult = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                Catalogue(preferred, fallback),
                assignment);
            var fallbackResult = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                Catalogue(fallback),
                assignment);
            var rejectedResult = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                Catalogue(Profile(
                    "realmraiders.procedural-humanoid.other.v1",
                    ProceduralHumanoidMotionTuning.CompatibilityDefault)),
                assignment);

            Assert.That(preferredResult.Resolution, Is.EqualTo(
                ProceduralHumanoidTuningResolution.Preferred));
            Assert.That(preferredResult.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.None));
            Assert.That(preferredResult.Profile, Is.SameAs(preferred));
            Assert.That(preferredResult.Tuning, Is.SameAs(
                ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable));

            Assert.That(fallbackResult.Resolution, Is.EqualTo(
                ProceduralHumanoidTuningResolution.Fallback));
            Assert.That(fallbackResult.Profile, Is.SameAs(fallback));
            Assert.That(fallbackResult.Tuning, Is.SameAs(
                ProceduralHumanoidMotionTuning.CompatibilityDefault));

            Assert.That(rejectedResult.Resolution, Is.EqualTo(
                ProceduralHumanoidTuningResolution.Rejected));
            Assert.That(rejectedResult.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.NoAssignedProfileAvailable));
            Assert.That(rejectedResult.Profile, Is.Null);
            Assert.That(rejectedResult.Tuning, Is.Null);
        }

        [Test]
        public void Resolve_IsExactCaseSensitiveAndDeterministic()
        {
            var catalogue = Catalogue(
                Profile(
                    PreferredId,
                    ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable));
            var exact = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                catalogue,
                Assignment(fallbackId: PreferredId));
            var wrongCase = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                catalogue,
                Assignment(
                    preferredId: PreferredId.ToUpperInvariant(),
                    fallbackId: FallbackId.ToUpperInvariant()));
            var repeated = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                catalogue,
                Assignment(
                    preferredId: PreferredId.ToUpperInvariant(),
                    fallbackId: FallbackId.ToUpperInvariant()));

            Assert.That(exact.Resolution, Is.EqualTo(
                ProceduralHumanoidTuningResolution.Preferred));
            Assert.That(wrongCase.Resolution, Is.EqualTo(
                ProceduralHumanoidTuningResolution.Rejected));
            Assert.That(wrongCase.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.InvalidPreferredProfileId));
            Assert.That(repeated.Signature, Is.EqualTo(wrongCase.Signature));
        }

        [Test]
        public void Resolve_NullInvalidAndNullTuningInputsFailClosed()
        {
            var catalogue = Catalogue(Profile(
                PreferredId,
                ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable));
            var nullCatalogue = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                null,
                Assignment());
            var nullAssignment = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                catalogue,
                null);
            var invalidCharacter = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                catalogue,
                Assignment(characterId: "Bad/Character"));
            var invalidPreferred = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                catalogue,
                Assignment(preferredId: "bad..preferred"));
            var invalidFallback = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                catalogue,
                Assignment(fallbackId: "bad/fallback"));
            var invalidBuild = ProceduralHumanoidTuningCatalogue.Build(
                new IProceduralHumanoidTuningProvider[]
                {
                    new TestProvider(Profile(PreferredId, null))
                });
            var nullTuning = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                invalidBuild.Catalogue,
                Assignment());

            Assert.That(nullCatalogue.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.NullCatalogue));
            Assert.That(nullAssignment.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.NullAssignment));
            Assert.That(invalidCharacter.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.InvalidCharacterOrRecipeId));
            Assert.That(invalidPreferred.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.InvalidPreferredProfileId));
            Assert.That(invalidFallback.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.InvalidFallbackProfileId));
            Assert.That(invalidBuild.Succeeded, Is.False);
            Assert.That(invalidBuild.Issues.Select(issue => issue.Code), Does.Contain(
                ProceduralHumanoidTuningCatalogueIssueCode.NullTuning));
            Assert.That(nullTuning.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.NullCatalogue));
        }

        [Test]
        public void AssignmentAndResultAreImmutableAndNeverApplyTuning()
        {
            var assignment = Assignment();
            var result = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                Catalogue(Profile(
                    PreferredId,
                    ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable)),
                assignment);

            Assert.That(typeof(ProceduralHumanoidTuningAssignment)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ProceduralHumanoidTuningAssignmentResult)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(ProceduralHumanoidTuningAssignmentResolver)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Select(method => method.Name), Is.EqualTo(new[] { "Resolve" }));
            Assert.That(result.Tuning, Is.SameAs(
                ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable));
        }

        private static ProceduralHumanoidTuningAssignment Assignment(
            string characterId = CharacterId,
            string preferredId = PreferredId,
            string fallbackId = FallbackId)
        {
            return new ProceduralHumanoidTuningAssignment(
                characterId,
                preferredId,
                fallbackId);
        }

        private static ProceduralHumanoidTuningProfile Profile(
            string profileId,
            ProceduralHumanoidMotionTuning tuning)
        {
            return new ProceduralHumanoidTuningProfile(
                profileId,
                tuning);
        }

        private static ProceduralHumanoidTuningCatalogue Catalogue(
            params ProceduralHumanoidTuningProfile[] profiles)
        {
            var build = ProceduralHumanoidTuningCatalogue.Build(
                new IProceduralHumanoidTuningProvider[]
                {
                    new TestProvider(profiles)
                });
            Assert.That(build.Succeeded, Is.True);
            return build.Catalogue;
        }

        private sealed class TestProvider : IProceduralHumanoidTuningProvider
        {
            public TestProvider(
                params ProceduralHumanoidTuningProfile[] profiles)
            {
                Profiles = profiles;
            }

            public string ProviderId => "test.procedural-humanoid.assignment.v1";

            public IReadOnlyList<ProceduralHumanoidTuningProfile> Profiles { get; }
        }
    }
}
