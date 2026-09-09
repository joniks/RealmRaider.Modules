using System;

namespace RealmRaiders.Modules.WorldSurfaceValidation
{
    public sealed class WorldSurfaceSeamAxisResult
    {
        internal WorldSurfaceSeamAxisResult(int sampleCount, double meanDelta, double worstDelta, double threshold)
        {
            SampleCount = sampleCount;
            MeanDelta = meanDelta;
            WorstDelta = worstDelta;
            Threshold = threshold;
        }

        public int SampleCount { get; }
        public double MeanDelta { get; }
        public double WorstDelta { get; }
        public double Threshold { get; }
        public bool PassesThreshold => WorstDelta <= Threshold;
    }

    public sealed class WorldSurfaceSeamAnalysisResult
    {
        internal WorldSurfaceSeamAnalysisResult(
            WorldSurfaceSeamAxisResult leftToRight,
            WorldSurfaceSeamAxisResult topToBottom)
        {
            LeftToRight = leftToRight;
            TopToBottom = topToBottom;
        }

        public WorldSurfaceSeamAxisResult LeftToRight { get; }
        public WorldSurfaceSeamAxisResult TopToBottom { get; }
        public bool PassesThreshold => LeftToRight.PassesThreshold && TopToBottom.PassesThreshold;
    }

    /// <summary>
    /// Evaluates opposing texture edges from caller-supplied pixels only.
    /// Deltas are the mean absolute difference across unpremultiplied RGBA channels,
    /// normalized to the inclusive range [0, 1].
    /// </summary>
    public static class WorldSurfaceSeamAnalyzer
    {
        public static WorldSurfaceSeamAnalysisResult Analyze(WorldSurfacePixels surface, double threshold)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));
            if (double.IsNaN(threshold) || double.IsInfinity(threshold) || threshold < 0d || threshold > 1d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(threshold),
                    "Threshold must be a finite value from 0 through 1.");
            }

            return new WorldSurfaceSeamAnalysisResult(
                AnalyzeLeftToRight(surface, threshold),
                AnalyzeTopToBottom(surface, threshold));
        }

        private static WorldSurfaceSeamAxisResult AnalyzeLeftToRight(WorldSurfacePixels surface, double threshold)
        {
            var total = 0d;
            var worst = 0d;
            for (var y = 0; y < surface.Height; y++)
            {
                var delta = surface.At(0, y).NormalizedAbsoluteDelta(surface.At(surface.Width - 1, y));
                total += delta;
                if (delta > worst)
                    worst = delta;
            }

            return new WorldSurfaceSeamAxisResult(surface.Height, total / surface.Height, worst, threshold);
        }

        private static WorldSurfaceSeamAxisResult AnalyzeTopToBottom(WorldSurfacePixels surface, double threshold)
        {
            var total = 0d;
            var worst = 0d;
            for (var x = 0; x < surface.Width; x++)
            {
                var delta = surface.At(x, 0).NormalizedAbsoluteDelta(surface.At(x, surface.Height - 1));
                total += delta;
                if (delta > worst)
                    worst = delta;
            }

            return new WorldSurfaceSeamAxisResult(surface.Width, total / surface.Width, worst, threshold);
        }
    }
}
