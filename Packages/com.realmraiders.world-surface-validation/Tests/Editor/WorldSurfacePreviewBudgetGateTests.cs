using System;
using NUnit.Framework;

namespace RealmRaiders.Modules.WorldSurfaceValidation.Tests
{
    public sealed class WorldSurfacePreviewBudgetGateTests
    {
        [Test]
        public void PreviewProfile1024To512_WithSupportedMobileFacts_IsAccepted()
        {
            var metadata = new WorldSurfacePreviewMetadata(
                "sylvan-moss-preview",
                1024,
                1024,
                512,
                false,
                true,
                WorldSurfacePreviewBudgetGate.Astc6x6CompressionLabel);

            var result = WorldSurfacePreviewBudgetGate.Evaluate(metadata);

            Assert.That(result.IsAccepted, Is.True);
            Assert.That(result.Issues, Is.Empty);
        }

        [Test]
        public void AndroidDimensionBoundary_IsAcceptedAt512AndRejectedAboveIt()
        {
            var atBoundary = WorldSurfacePreviewBudgetGate.Evaluate(CreateAcceptedMetadata(512));
            var aboveBoundary = WorldSurfacePreviewBudgetGate.Evaluate(CreateAcceptedMetadata(513));

            Assert.That(atBoundary.IsAccepted, Is.True);
            Assert.That(aboveBoundary.IsAccepted, Is.False);
            Assert.That(aboveBoundary.Issues, Has.Count.EqualTo(1));
            Assert.That(
                aboveBoundary.Issues[0].Code,
                Is.EqualTo(WorldSurfacePreviewBudgetIssueCode.AndroidMaximumDimensionExceedsPreviewBudget));
            Assert.That(
                aboveBoundary.Issues[0].Message,
                Is.EqualTo("Android maximum dimension exceeds the 512 preview budget."));
        }

        [Test]
        public void SquareButNonPowerOfTwoSource_IsRejectedByItsOwnStableIssue()
        {
            var metadata = new WorldSurfacePreviewMetadata(
                "rough-stone-preview",
                768,
                768,
                512,
                false,
                true,
                WorldSurfacePreviewBudgetGate.Astc6x6CompressionLabel);

            var result = WorldSurfacePreviewBudgetGate.Evaluate(metadata);

            Assert.That(result.IsAccepted, Is.False);
            Assert.That(result.Issues, Has.Count.EqualTo(1));
            AssertIssue(
                result,
                0,
                WorldSurfacePreviewBudgetIssueCode.SourceDimensionsMustBePowerOfTwo,
                "Source dimensions must both be powers of two.");
        }

        [Test]
        public void InvalidMetadata_ReturnsStableOrdinalIssuesWithoutChangingInput()
        {
            var metadata = new WorldSurfacePreviewMetadata(
                " ",
                1024,
                512,
                1024,
                true,
                false,
                "astc_6x6");

            var result = WorldSurfacePreviewBudgetGate.Evaluate(metadata);

            Assert.That(result.IsAccepted, Is.False);
            Assert.That(result.Issues, Has.Count.EqualTo(6));
            AssertIssue(result, 0, WorldSurfacePreviewBudgetIssueCode.MissingSemanticId, "Semantic identifier is required.");
            AssertIssue(result, 1, WorldSurfacePreviewBudgetIssueCode.SourceDimensionsMustBeSquare, "Source dimensions must be square.");
            AssertIssue(result, 2, WorldSurfacePreviewBudgetIssueCode.AndroidMaximumDimensionExceedsPreviewBudget, "Android maximum dimension exceeds the 512 preview budget.");
            AssertIssue(result, 3, WorldSurfacePreviewBudgetIssueCode.ReadWriteMustBeDisabled, "Read/Write must be disabled.");
            AssertIssue(result, 4, WorldSurfacePreviewBudgetIssueCode.MipmapsMustBeEnabled, "Mipmaps must be enabled.");
            AssertIssue(result, 5, WorldSurfacePreviewBudgetIssueCode.UnsupportedMobileCompressionLabel, "Mobile compression label must be ASTC_6x6 or ETC2_RGBA8.");
            Assert.That(metadata.MobileCompressionLabel, Is.EqualTo("astc_6x6"));
        }

        [Test]
        public void NonPositiveDimensionsAndAndroidMaximum_AreReportedExplicitly()
        {
            var metadata = new WorldSurfacePreviewMetadata(
                null,
                0,
                -1,
                0,
                false,
                true,
                WorldSurfacePreviewBudgetGate.Etc2Rgba8CompressionLabel);

            var result = WorldSurfacePreviewBudgetGate.Evaluate(metadata);

            Assert.That(result.Issues, Has.Count.EqualTo(3));
            AssertIssue(result, 0, WorldSurfacePreviewBudgetIssueCode.MissingSemanticId, "Semantic identifier is required.");
            AssertIssue(result, 1, WorldSurfacePreviewBudgetIssueCode.SourceDimensionsMustBePositive, "Source dimensions must both be positive.");
            AssertIssue(result, 2, WorldSurfacePreviewBudgetIssueCode.AndroidMaximumDimensionMustBePositive, "Android maximum dimension must be positive.");
        }

        [Test]
        public void NullMetadata_IsRejectedExplicitly()
        {
            Assert.Throws<ArgumentNullException>(() => WorldSurfacePreviewBudgetGate.Evaluate(null));
        }

        private static WorldSurfacePreviewMetadata CreateAcceptedMetadata(int androidMaximumDimension)
        {
            return new WorldSurfacePreviewMetadata(
                "infernal-ash-preview",
                1024,
                1024,
                androidMaximumDimension,
                false,
                true,
                WorldSurfacePreviewBudgetGate.Etc2Rgba8CompressionLabel);
        }

        private static void AssertIssue(
            WorldSurfacePreviewBudgetResult result,
            int index,
            WorldSurfacePreviewBudgetIssueCode expectedCode,
            string expectedMessage)
        {
            Assert.That(result.Issues[index].Code, Is.EqualTo(expectedCode));
            Assert.That(result.Issues[index].Message, Is.EqualTo(expectedMessage));
        }
    }
}
