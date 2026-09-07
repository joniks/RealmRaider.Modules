using System.Linq;
using NUnit.Framework;

namespace RealmRaiders.CharacterVisualTuning.Tests
{
    public sealed class StarterCreatureVisualProfilesTests
    {
        private static readonly string[] ExpectedProfileIds =
        {
            "realmraiders.guardian-ent.prototype",
            "realmraiders.sylvan-wolf.prototype",
            "realmraiders.infernal-brute.prototype",
            "realmraiders.hellhound.prototype"
        };

        [Test]
        public void CreateAll_ReturnsExactOrderedStarterCreatureSet()
        {
            var profiles = StarterCreatureVisualProfiles.CreateAll();

            Assert.That(profiles.Count, Is.EqualTo(4));
            Assert.That(profiles.Select(profile => profile.ProfileId).ToArray(), Is.EqualTo(ExpectedProfileIds));
        }

        [Test]
        public void Profiles_AreValidNoOpIntentsWithUniquePaletteRolesAndPositiveBudgets()
        {
            var profiles = StarterCreatureVisualProfiles.CreateAll();

            foreach (var profile in profiles)
            {
                Assert.That(VisualTuningValidator.Validate(profile), Is.Empty, profile.ProfileId);
                Assert.That(profile.PresentationTransform.IsNoOp, Is.True, profile.ProfileId);
                Assert.That(
                    profile.Palette.Select(intent => intent.Role).Distinct().Count(),
                    Is.EqualTo(profile.Palette.Count),
                    profile.ProfileId);
                Assert.That(profile.Budget.MaterialCount, Is.GreaterThan(0), profile.ProfileId);
                Assert.That(profile.Budget.TextureCount, Is.GreaterThan(0), profile.ProfileId);
                Assert.That(profile.Budget.MaxTextureEdgePixels, Is.GreaterThan(0), profile.ProfileId);
                Assert.That(profile.Budget.TriangleCount, Is.GreaterThan(0), profile.ProfileId);
            }
        }
    }
}
