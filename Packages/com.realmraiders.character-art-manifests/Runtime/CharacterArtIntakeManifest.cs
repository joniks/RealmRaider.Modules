using System.Collections.Generic;
using System.Collections.ObjectModel;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterArtManifests
{
    public enum CharacterArtMotionClipKey
    {
        Idle,
        Locomotion,
        AttackPrimary,
        AttackAbility,
        Hit,
        Death
    }

    public sealed class CharacterArtMotionClip
    {
        public CharacterArtMotionClip(CharacterArtMotionClipKey key, string clipId)
        {
            Key = key;
            ClipId = clipId;
        }

        public CharacterArtMotionClipKey Key { get; }
        public string ClipId { get; }
    }

    public enum CharacterArtLodLevel
    {
        Lod0,
        Lod1,
        Lod2
    }

    public sealed class CharacterArtLodTriangleBudget
    {
        public CharacterArtLodTriangleBudget(CharacterArtLodLevel level, int maxTriangles)
        {
            Level = level;
            MaxTriangles = maxTriangles;
        }

        public CharacterArtLodLevel Level { get; }
        public int MaxTriangles { get; }
    }

    /// <summary>
    /// Immutable evidence and import-policy snapshot for one reviewed character-art source.
    /// It does not approve, locate, import, or apply the described content.
    /// </summary>
    public sealed class CharacterArtIntakeManifest
    {
        private readonly ReadOnlyCollection<CharacterArtMotionClip> motionClips;
        private readonly ReadOnlyCollection<CharacterArtLodTriangleBudget> lodTriangleBudgets;

        public CharacterArtIntakeManifest(
            int schemaVersion,
            string characterId,
            string sourceId,
            CharacterBodyFamily family,
            string title,
            string creator,
            string directSourceUrl,
            string licenseName,
            string licenseLegalUrl,
            string attribution,
            string changeNote,
            string archiveSha256,
            string selectedSourceFile,
            string rigProfileId,
            IEnumerable<CharacterArtMotionClip> motionClips,
            IEnumerable<CharacterArtLodTriangleBudget> lodTriangleBudgets,
            int maxRendererCount,
            int maxMaterialCount,
            int maxTextureCount,
            int maxTextureDimensionPixels,
            bool importSourceColliders,
            bool applyRootMotion,
            bool importAnimations,
            bool importAnimationEvents,
            bool importEmbeddedMaterials,
            bool importEmbeddedTextures)
        {
            SchemaVersion = schemaVersion;
            CharacterId = characterId;
            SourceId = sourceId;
            Family = family;
            Title = title;
            Creator = creator;
            DirectSourceUrl = directSourceUrl;
            LicenseName = licenseName;
            LicenseLegalUrl = licenseLegalUrl;
            Attribution = attribution;
            ChangeNote = changeNote;
            ArchiveSha256 = archiveSha256;
            SelectedSourceFile = selectedSourceFile;
            RigProfileId = rigProfileId;
            this.motionClips = new ReadOnlyCollection<CharacterArtMotionClip>(
                motionClips == null
                    ? new List<CharacterArtMotionClip>()
                    : new List<CharacterArtMotionClip>(motionClips));
            this.lodTriangleBudgets = new ReadOnlyCollection<CharacterArtLodTriangleBudget>(
                lodTriangleBudgets == null
                    ? new List<CharacterArtLodTriangleBudget>()
                    : new List<CharacterArtLodTriangleBudget>(lodTriangleBudgets));
            MaxRendererCount = maxRendererCount;
            MaxMaterialCount = maxMaterialCount;
            MaxTextureCount = maxTextureCount;
            MaxTextureDimensionPixels = maxTextureDimensionPixels;
            ImportSourceColliders = importSourceColliders;
            ApplyRootMotion = applyRootMotion;
            ImportAnimations = importAnimations;
            ImportAnimationEvents = importAnimationEvents;
            ImportEmbeddedMaterials = importEmbeddedMaterials;
            ImportEmbeddedTextures = importEmbeddedTextures;
        }

        public int SchemaVersion { get; }
        public string CharacterId { get; }
        public string SourceId { get; }
        public CharacterBodyFamily Family { get; }
        public string Title { get; }
        public string Creator { get; }
        public string DirectSourceUrl { get; }
        public string LicenseName { get; }
        public string LicenseLegalUrl { get; }
        public string Attribution { get; }
        public string ChangeNote { get; }
        public string ArchiveSha256 { get; }
        public string SelectedSourceFile { get; }
        public string RigProfileId { get; }
        public IReadOnlyList<CharacterArtMotionClip> MotionClips => motionClips;
        public IReadOnlyList<CharacterArtLodTriangleBudget> LodTriangleBudgets => lodTriangleBudgets;
        public int MaxRendererCount { get; }
        public int MaxMaterialCount { get; }
        public int MaxTextureCount { get; }
        public int MaxTextureDimensionPixels { get; }
        public bool ImportSourceColliders { get; }
        public bool ApplyRootMotion { get; }
        public bool ImportAnimations { get; }
        public bool ImportAnimationEvents { get; }
        public bool ImportEmbeddedMaterials { get; }
        public bool ImportEmbeddedTextures { get; }
    }
}
