namespace RealmRaiders.Modules.WorldSurfaceValidation
{
    /// <summary>
    /// Immutable, caller-declared facts for a world-surface preview candidate.
    /// This type does not inspect an asset or Unity import settings.
    /// </summary>
    public sealed class WorldSurfacePreviewMetadata
    {
        public WorldSurfacePreviewMetadata(
            string semanticId,
            int sourceWidth,
            int sourceHeight,
            int androidMaximumDimension,
            bool readWriteEnabled,
            bool mipmapsEnabled,
            string mobileCompressionLabel)
        {
            SemanticId = semanticId;
            SourceWidth = sourceWidth;
            SourceHeight = sourceHeight;
            AndroidMaximumDimension = androidMaximumDimension;
            ReadWriteEnabled = readWriteEnabled;
            MipmapsEnabled = mipmapsEnabled;
            MobileCompressionLabel = mobileCompressionLabel;
        }

        public string SemanticId { get; }
        public int SourceWidth { get; }
        public int SourceHeight { get; }
        public int AndroidMaximumDimension { get; }
        public bool ReadWriteEnabled { get; }
        public bool MipmapsEnabled { get; }
        public string MobileCompressionLabel { get; }
    }
}
