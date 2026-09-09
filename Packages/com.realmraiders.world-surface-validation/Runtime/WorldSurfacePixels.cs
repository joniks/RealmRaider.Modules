using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.WorldSurfaceValidation
{
    /// <summary>
    /// Immutable row-major raw pixel input. This type never reads image files.
    /// </summary>
    public sealed class WorldSurfacePixels
    {
        private readonly ReadOnlyCollection<Rgba32> pixels;

        public WorldSurfacePixels(int width, int height, IReadOnlyList<Rgba32> pixels)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));

            var expectedCount = (long)width * height;
            if (pixels.Count != expectedCount)
            {
                throw new ArgumentException(
                    "Pixel count must equal width multiplied by height.",
                    nameof(pixels));
            }

            var snapshot = new Rgba32[pixels.Count];
            try
            {
                for (var index = 0; index < snapshot.Length; index++)
                    snapshot[index] = pixels[index];
            }
            catch (Exception exception) when (!(exception is ArgumentException))
            {
                throw new ArgumentException("Pixels must be readable by index.", nameof(pixels), exception);
            }

            Width = width;
            Height = height;
            this.pixels = new ReadOnlyCollection<Rgba32>(snapshot);
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<Rgba32> Pixels => pixels;

        internal Rgba32 At(int x, int y)
        {
            return pixels[(y * Width) + x];
        }
    }
}
