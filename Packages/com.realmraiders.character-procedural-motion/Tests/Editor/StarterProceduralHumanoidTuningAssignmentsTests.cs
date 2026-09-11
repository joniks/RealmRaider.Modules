using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.CharacterProceduralMotion.Tests
{
    public sealed class StarterProceduralHumanoidTuningAssignmentsTests
    {
        [Test]
        public void BloodKnightAssignmentUsesExactFactualAndStarterProfileIds()
        {
            var assignment = StarterProceduralHumanoidTuningAssignments.BloodKnight;

            Assert.That(assignment.CharacterOrRecipeId, Is.EqualTo(
                "realmraiders.blood-knight"));
            Assert.That(assignment.PreferredProfileId, Is.EqualTo(
                StarterProceduralHumanoidTuningProvider.BloodKnightDeviceReadableProfileId));
            Assert.That(assignment.FallbackProfileId, Is.EqualTo(
                StarterProceduralHumanoidTuningProvider.CompatibilityProfileId));
        }

        [Test]
        public void BloodKnightAssignmentIsStableAndImmutable()
        {
            var first = StarterProceduralHumanoidTuningAssignments.BloodKnight;
            var second = StarterProceduralHumanoidTuningAssignments.BloodKnight;

            Assert.That(first, Is.SameAs(second));
            Assert.That(typeof(ProceduralHumanoidTuningAssignment)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(StarterProceduralHumanoidTuningAssignments)
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Select(property => property.Name), Is.EqualTo(new[] { "BloodKnight" }));
        }

        [Test]
        public void BloodKnightAssignmentResolvesPreferredAgainstTheStarterProvider()
        {
            var catalogue = Catalogue(new StarterProceduralHumanoidTuningProvider());
            var result = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                catalogue,
                StarterProceduralHumanoidTuningAssignments.BloodKnight);

            Assert.That(result.Resolution, Is.EqualTo(
                ProceduralHumanoidTuningResolution.Preferred));
            Assert.That(result.RejectionReason, Is.EqualTo(
                ProceduralHumanoidTuningRejectionReason.None));
            Assert.That(result.Profile.ProfileId, Is.EqualTo(
                StarterProceduralHumanoidTuningProvider.BloodKnightDeviceReadableProfileId));
            Assert.That(result.Tuning, Is.SameAs(
                ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable));
        }

        [Test]
        public void BloodKnightAssignmentUsesFallbackOnlyWhenCompatibilityIsExplicitlySupplied()
        {
            var catalogue = Catalogue(new CompatibilityOnlyProvider());
            var result = ProceduralHumanoidTuningAssignmentResolver.Resolve(
                catalogue,
                StarterProceduralHumanoidTuningAssignments.BloodKnight);

            Assert.That(result.Resolution, Is.EqualTo(
                ProceduralHumanoidTuningResolution.Fallback));
            Assert.That(result.Profile.ProfileId, Is.EqualTo(
                StarterProceduralHumanoidTuningProvider.CompatibilityProfileId));
            Assert.That(result.Tuning, Is.SameAs(
                ProceduralHumanoidMotionTuning.CompatibilityDefault));
        }

        private static ProceduralHumanoidTuningCatalogue Catalogue(
            IProceduralHumanoidTuningProvider provider)
        {
            var build = ProceduralHumanoidTuningCatalogue.Build(
                new IProceduralHumanoidTuningProvider[]
                {
                    provider
                });
            Assert.That(build.Succeeded, Is.True);
            return build.Catalogue;
        }

        private sealed class CompatibilityOnlyProvider : IProceduralHumanoidTuningProvider
        {
            private static readonly IReadOnlyList<ProceduralHumanoidTuningProfile> profiles =
                new ProceduralHumanoidTuningProfile[]
                {
                    new ProceduralHumanoidTuningProfile(
                        StarterProceduralHumanoidTuningProvider.CompatibilityProfileId,
                        ProceduralHumanoidMotionTuning.CompatibilityDefault)
                };

            public string ProviderId => "test.procedural-humanoid.compatibility-only.v1";

            public IReadOnlyList<ProceduralHumanoidTuningProfile> Profiles => profiles;
        }
    }
}
