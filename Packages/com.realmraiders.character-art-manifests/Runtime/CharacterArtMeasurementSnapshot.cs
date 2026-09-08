using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.CharacterArtManifests
{
    public sealed class CharacterArtLodTriangleMeasurement
    {
        public CharacterArtLodTriangleMeasurement(
            CharacterArtLodLevel level,
            int triangleCount)
        {
            Level = level;
            TriangleCount = triangleCount;
        }

        public CharacterArtLodLevel Level { get; }
        public int TriangleCount { get; }
    }

    internal enum CharacterArtMeasurementCollectionState
    {
        Readable,
        Null,
        Unreadable
    }

    /// <summary>
    /// Immutable adapter-neutral measurements for one imported character-art source.
    /// Construction snapshots caller-owned collections and records unreadable inputs
    /// for fail-closed evaluation instead of allowing their exceptions to escape.
    /// </summary>
    public sealed class CharacterArtMeasurementSnapshot
    {
        private readonly ReadOnlyCollection<CharacterArtLodTriangleMeasurement> lodTriangleCounts;
        private readonly ReadOnlyCollection<string> importedMotionClipIds;

        public CharacterArtMeasurementSnapshot(
            string characterId,
            string sourceId,
            IEnumerable<CharacterArtLodTriangleMeasurement> lodTriangleCounts,
            int rendererCount,
            int materialCount,
            int textureCount,
            int maxTextureEdgePixels,
            bool hasSourceColliders,
            bool usesRootMotion,
            bool hasAnimationEvents,
            bool animationsImported,
            IEnumerable<string> importedMotionClipIds)
        {
            CharacterId = characterId;
            SourceId = sourceId;
            this.lodTriangleCounts = Snapshot(
                lodTriangleCounts,
                out var lodTriangleCountsState);
            LodTriangleCountsState = lodTriangleCountsState;
            RendererCount = rendererCount;
            MaterialCount = materialCount;
            TextureCount = textureCount;
            MaxTextureEdgePixels = maxTextureEdgePixels;
            HasSourceColliders = hasSourceColliders;
            UsesRootMotion = usesRootMotion;
            HasAnimationEvents = hasAnimationEvents;
            AnimationsImported = animationsImported;
            this.importedMotionClipIds = Snapshot(
                importedMotionClipIds,
                out var importedMotionClipIdsState);
            ImportedMotionClipIdsState = importedMotionClipIdsState;
        }

        public string CharacterId { get; }
        public string SourceId { get; }
        public IReadOnlyList<CharacterArtLodTriangleMeasurement> LodTriangleCounts => lodTriangleCounts;
        public int RendererCount { get; }
        public int MaterialCount { get; }
        public int TextureCount { get; }
        public int MaxTextureEdgePixels { get; }
        public bool HasSourceColliders { get; }
        public bool UsesRootMotion { get; }
        public bool HasAnimationEvents { get; }
        public bool AnimationsImported { get; }
        public IReadOnlyList<string> ImportedMotionClipIds => importedMotionClipIds;

        internal CharacterArtMeasurementCollectionState LodTriangleCountsState { get; }
        internal CharacterArtMeasurementCollectionState ImportedMotionClipIdsState { get; }

        private static ReadOnlyCollection<T> Snapshot<T>(
            IEnumerable<T> source,
            out CharacterArtMeasurementCollectionState state)
        {
            if (source == null)
            {
                state = CharacterArtMeasurementCollectionState.Null;
                return new ReadOnlyCollection<T>(new List<T>());
            }

            try
            {
                var snapshot = new List<T>();
                foreach (var item in source)
                    snapshot.Add(item);
                state = CharacterArtMeasurementCollectionState.Readable;
                return new ReadOnlyCollection<T>(snapshot);
            }
            catch (Exception)
            {
                state = CharacterArtMeasurementCollectionState.Unreadable;
                return new ReadOnlyCollection<T>(new List<T>());
            }
        }
    }
}
