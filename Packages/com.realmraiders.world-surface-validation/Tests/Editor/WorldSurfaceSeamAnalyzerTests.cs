using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace RealmRaiders.Modules.WorldSurfaceValidation.Tests
{
    public sealed class WorldSurfaceSeamAnalyzerTests
    {
        [Test]
        public void PerfectlyTilingSurface_HasZeroDeltasAndPasses()
        {
            var surface = new WorldSurfacePixels(2, 2, new[]
            {
                Pixel(1), Pixel(1),
                Pixel(2), Pixel(2)
            });

            var result = WorldSurfaceSeamAnalyzer.Analyze(surface, 0d);

            Assert.That(result.LeftToRight.SampleCount, Is.EqualTo(2));
            Assert.That(result.TopToBottom.SampleCount, Is.EqualTo(2));
            Assert.That(result.LeftToRight.MeanDelta, Is.Zero);
            Assert.That(result.LeftToRight.WorstDelta, Is.Zero);
            Assert.That(result.TopToBottom.MeanDelta, Is.Zero);
            Assert.That(result.TopToBottom.WorstDelta, Is.Zero);
            Assert.That(result.PassesThreshold, Is.True);
        }

        [Test]
        public void MismatchedOpposingEdge_FailsTheSuppliedThreshold()
        {
            var surface = new WorldSurfacePixels(2, 2, new[]
            {
                Pixel(0), Pixel(255),
                Pixel(0), Pixel(255)
            });

            var result = WorldSurfaceSeamAnalyzer.Analyze(surface, 0.2d);

            Assert.That(result.LeftToRight.MeanDelta, Is.EqualTo(0.25d));
            Assert.That(result.LeftToRight.WorstDelta, Is.EqualTo(0.25d));
            Assert.That(result.LeftToRight.PassesThreshold, Is.False);
            Assert.That(result.TopToBottom.PassesThreshold, Is.True);
            Assert.That(result.PassesThreshold, Is.False);
        }

        [Test]
        public void ThresholdBoundary_UsesWorstEdgeSampleAndTreatsEqualityAsPass()
        {
            var surface = new WorldSurfacePixels(2, 3, new[]
            {
                RedPixel(0), RedPixel(0),
                RedPixel(0), RedPixel(255),
                RedPixel(0), RedPixel(0)
            });

            var boundaryResult = WorldSurfaceSeamAnalyzer.Analyze(surface, 0.25d);
            var belowBoundaryResult = WorldSurfaceSeamAnalyzer.Analyze(surface, 0.249d);

            Assert.That(boundaryResult.LeftToRight.MeanDelta, Is.EqualTo(1d / 12d));
            Assert.That(boundaryResult.LeftToRight.WorstDelta, Is.EqualTo(0.25d));
            Assert.That(boundaryResult.LeftToRight.PassesThreshold, Is.True);
            Assert.That(boundaryResult.PassesThreshold, Is.True);
            Assert.That(belowBoundaryResult.LeftToRight.PassesThreshold, Is.False);
            Assert.That(belowBoundaryResult.PassesThreshold, Is.False);
        }

        [Test]
        public void FullRgbaEdgeDifference_IsNormalizedToOne()
        {
            var surface = new WorldSurfacePixels(2, 2, new[]
            {
                RgbaPixel(0, 0, 0, 0), RgbaPixel(255, 255, 255, 255),
                RgbaPixel(0, 0, 0, 0), RgbaPixel(255, 255, 255, 255)
            });

            var passingResult = WorldSurfaceSeamAnalyzer.Analyze(surface, 1d);
            var failingResult = WorldSurfaceSeamAnalyzer.Analyze(surface, 0.999d);

            Assert.That(passingResult.LeftToRight.MeanDelta, Is.EqualTo(1d));
            Assert.That(passingResult.LeftToRight.WorstDelta, Is.EqualTo(1d));
            Assert.That(passingResult.LeftToRight.PassesThreshold, Is.True);
            Assert.That(failingResult.LeftToRight.PassesThreshold, Is.False);
        }

        [Test]
        public void MismatchedTopToBottomEdge_FailsWithoutAffectingLeftToRightVerdict()
        {
            var surface = new WorldSurfacePixels(2, 2, new[]
            {
                Pixel(0), Pixel(0),
                Pixel(255), Pixel(255)
            });

            var result = WorldSurfaceSeamAnalyzer.Analyze(surface, 0.2d);

            Assert.That(result.LeftToRight.MeanDelta, Is.Zero);
            Assert.That(result.LeftToRight.PassesThreshold, Is.True);
            Assert.That(result.TopToBottom.SampleCount, Is.EqualTo(2));
            Assert.That(result.TopToBottom.MeanDelta, Is.EqualTo(0.25d));
            Assert.That(result.TopToBottom.WorstDelta, Is.EqualTo(0.25d));
            Assert.That(result.TopToBottom.PassesThreshold, Is.False);
            Assert.That(result.PassesThreshold, Is.False);
        }

        [Test]
        public void PixelBuffer_SnapshotsCallerPixelsIntoAnImmutableRowMajorInput()
        {
            var supplied = new List<Rgba32>
            {
                Pixel(1), Pixel(1),
                Pixel(2), Pixel(2)
            };
            var surface = new WorldSurfacePixels(2, 2, supplied);
            supplied[0] = Pixel(255);
            supplied.Clear();

            var result = WorldSurfaceSeamAnalyzer.Analyze(surface, 0d);

            Assert.That(result.PassesThreshold, Is.True);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<Rgba32>)surface.Pixels).Clear());
        }

        [Test]
        public void MalformedInputAndThresholds_AreRejectedExplicitly()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorldSurfacePixels(0, 1, Array.Empty<Rgba32>()));
            Assert.Throws<ArgumentNullException>(() => new WorldSurfacePixels(1, 1, null));
            Assert.Throws<ArgumentException>(() => new WorldSurfacePixels(2, 2, new[] { Pixel(1) }));

            var surface = new WorldSurfacePixels(1, 1, new[] { Pixel(1) });
            Assert.Throws<ArgumentOutOfRangeException>(() => WorldSurfaceSeamAnalyzer.Analyze(surface, -0.01d));
            Assert.Throws<ArgumentOutOfRangeException>(() => WorldSurfaceSeamAnalyzer.Analyze(surface, 1.01d));
            Assert.Throws<ArgumentOutOfRangeException>(() => WorldSurfaceSeamAnalyzer.Analyze(surface, double.NaN));
        }

        private static Rgba32 Pixel(byte red)
        {
            return RedPixel(red);
        }

        private static Rgba32 RedPixel(byte red)
        {
            return RgbaPixel(red, 0, 0, 255);
        }

        private static Rgba32 RgbaPixel(byte red, byte green, byte blue, byte alpha)
        {
            return new Rgba32(red, green, blue, alpha);
        }
    }
}
