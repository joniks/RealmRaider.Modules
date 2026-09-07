using System.Linq;
using NUnit.Framework;

namespace RealmRaiders.CharacterVisualTuning.Tests
{
    public sealed class VisualTuningDescriptorTests
    {
        [Test]
        public void BloodKnightBaseline_IsValidAndPreservesNoOpTransform()
        {
            var profile = BloodKnightBaseline.Create();

            Assert.That(profile.ProfileId, Is.EqualTo("realmraiders.blood-knight.3drt-baseline"));
            Assert.That(profile.SourceId, Is.EqualTo("3drt.fantasy-warrior"));
            Assert.That(profile.PresentationTransform.IsNoOp, Is.True);
            Assert.That(profile.Budget.MaterialCount, Is.EqualTo(1));
            Assert.That(profile.Budget.TextureCount, Is.EqualTo(1));
            Assert.That(profile.Budget.MaxTextureEdgePixels, Is.EqualTo(1024));
            Assert.That(profile.Budget.TriangleCount, Is.EqualTo(2500));
            Assert.That(VisualTuningValidator.Validate(profile), Is.Empty);
        }

        [Test]
        public void Validate_ReturnsIssuesInStableDescriptorOrder()
        {
            var profile = new CharacterVisualTuningProfile(
                "Bad ID",
                string.Empty,
                " ",
                new PresentationTransformIntent(localScale: new AxisValues(1f, 1f, 1f)),
                new[]
                {
                    new MaterialPaletteIntent(MaterialRole.Cloth, string.Empty),
                    new MaterialPaletteIntent(MaterialRole.Cloth, "duplicate")
                },
                new MobileVisualBudget(0, 0, 0, 0));

            var actual = VisualTuningValidator.Validate(profile).Select(issue => issue.Code).ToArray();

            Assert.That(actual, Is.EqualTo(new[]
            {
                VisualTuningIssueCode.InvalidProfileId,
                VisualTuningIssueCode.MissingSourceId,
                VisualTuningIssueCode.MissingDisplayName,
                VisualTuningIssueCode.TransformOverrideRequiresUnityReview,
                VisualTuningIssueCode.MissingPaletteDirection,
                VisualTuningIssueCode.DuplicateMaterialRole,
                VisualTuningIssueCode.InvalidMaterialCount,
                VisualTuningIssueCode.InvalidTextureCount,
                VisualTuningIssueCode.InvalidTextureEdge,
                VisualTuningIssueCode.InvalidTriangleCount
            }));
        }
    }
}
