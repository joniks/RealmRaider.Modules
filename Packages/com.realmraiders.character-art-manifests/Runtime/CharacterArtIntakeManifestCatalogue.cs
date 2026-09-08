using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.CharacterArtManifests
{
    /// <summary>A passive, explicitly supplied source of immutable art-intake manifests.</summary>
    public interface ICharacterArtIntakeManifestProvider
    {
        string ModuleId { get; }
        IReadOnlyList<CharacterArtIntakeManifest> Manifests { get; }
    }

    public enum CharacterArtIntakeManifestCatalogueIssueCode
    {
        NullProviderCollection,
        UnreadableProviderCollection,
        NullProvider,
        UnreadableProvider,
        InvalidModuleId,
        DuplicateProviderModuleId,
        NullManifestCollection,
        UnreadableManifestCollection,
        NullManifest,
        UnreadableManifest,
        InvalidManifest,
        DuplicateSourceId,
        AmbiguousCharacterId
    }

    public sealed class CharacterArtIntakeManifestCatalogueIssue
    {
        public CharacterArtIntakeManifestCatalogueIssue(
            CharacterArtIntakeManifestCatalogueIssueCode code,
            string path,
            CharacterArtManifestIssueCode? manifestIssueCode = null)
        {
            Code = code;
            Path = path;
            ManifestIssueCode = manifestIssueCode;
        }

        public CharacterArtIntakeManifestCatalogueIssueCode Code { get; }
        public string Path { get; }
        public CharacterArtManifestIssueCode? ManifestIssueCode { get; }
    }

    public sealed class CharacterArtIntakeManifestCatalogueBuildResult
    {
        private readonly ReadOnlyCollection<CharacterArtIntakeManifestCatalogueIssue> issues;

        internal CharacterArtIntakeManifestCatalogueBuildResult(
            CharacterArtIntakeManifestCatalogue catalogue,
            IList<CharacterArtIntakeManifestCatalogueIssue> issues)
        {
            Catalogue = catalogue;
            this.issues = new ReadOnlyCollection<CharacterArtIntakeManifestCatalogueIssue>(
                new List<CharacterArtIntakeManifestCatalogueIssue>(issues));
        }

        public bool Succeeded => Catalogue != null;
        public CharacterArtIntakeManifestCatalogue Catalogue { get; }
        public IReadOnlyList<CharacterArtIntakeManifestCatalogueIssue> Issues => issues;
    }

    /// <summary>An immutable exact-lookup snapshot built only from explicitly supplied providers.</summary>
    public sealed class CharacterArtIntakeManifestCatalogue
    {
        private readonly ReadOnlyCollection<CharacterArtIntakeManifest> manifests;
        private readonly ReadOnlyDictionary<string, CharacterArtIntakeManifest> manifestsBySourceId;
        private readonly ReadOnlyDictionary<string, CharacterArtIntakeManifest> manifestsByCharacterId;

        private CharacterArtIntakeManifestCatalogue(IList<CharacterArtIntakeManifest> manifests)
        {
            var manifestCopy = new List<CharacterArtIntakeManifest>(manifests);
            this.manifests = new ReadOnlyCollection<CharacterArtIntakeManifest>(manifestCopy);

            var sourceIds = new Dictionary<string, CharacterArtIntakeManifest>(StringComparer.Ordinal);
            var characterIds = new Dictionary<string, CharacterArtIntakeManifest>(StringComparer.Ordinal);
            for (var index = 0; index < manifestCopy.Count; index++)
            {
                var manifest = manifestCopy[index];
                sourceIds.Add(manifest.SourceId, manifest);
                characterIds.Add(manifest.CharacterId, manifest);
            }

            manifestsBySourceId = new ReadOnlyDictionary<string, CharacterArtIntakeManifest>(sourceIds);
            manifestsByCharacterId = new ReadOnlyDictionary<string, CharacterArtIntakeManifest>(characterIds);
        }

        public IReadOnlyList<CharacterArtIntakeManifest> Manifests => manifests;

        public bool TryGetBySourceId(string sourceId, out CharacterArtIntakeManifest manifest)
        {
            if (sourceId == null)
            {
                manifest = null;
                return false;
            }

            return manifestsBySourceId.TryGetValue(sourceId, out manifest);
        }

        public bool TryGetByCharacterId(string characterId, out CharacterArtIntakeManifest manifest)
        {
            if (characterId == null)
            {
                manifest = null;
                return false;
            }

            return manifestsByCharacterId.TryGetValue(characterId, out manifest);
        }

        public static CharacterArtIntakeManifestCatalogueBuildResult Build(
            IEnumerable<ICharacterArtIntakeManifestProvider> providers)
        {
            var issues = new List<CharacterArtIntakeManifestCatalogueIssue>();
            var providerSnapshots = SnapshotProviders(providers, issues);

            AddDuplicateProviderIssues(providerSnapshots, issues);
            var manifestSnapshots = SnapshotManifests(providerSnapshots);
            ValidateManifests(manifestSnapshots, issues);
            AddDuplicateManifestIssues(manifestSnapshots, issues);
            SortIssues(issues);

            if (issues.Count != 0)
                return new CharacterArtIntakeManifestCatalogueBuildResult(null, issues);

            var manifests = new List<CharacterArtIntakeManifest>(manifestSnapshots.Count);
            for (var index = 0; index < manifestSnapshots.Count; index++)
                manifests.Add(manifestSnapshots[index].Manifest);
            manifests.Sort((left, right) => string.CompareOrdinal(left.SourceId, right.SourceId));

            return new CharacterArtIntakeManifestCatalogueBuildResult(
                new CharacterArtIntakeManifestCatalogue(manifests),
                issues);
        }

        private static List<ProviderSnapshot> SnapshotProviders(
            IEnumerable<ICharacterArtIntakeManifestProvider> providers,
            ICollection<CharacterArtIntakeManifestCatalogueIssue> issues)
        {
            var snapshots = new List<ProviderSnapshot>();
            if (providers == null)
            {
                issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                    CharacterArtIntakeManifestCatalogueIssueCode.NullProviderCollection,
                    "providers"));
                return snapshots;
            }

            var suppliedProviders = new List<ICharacterArtIntakeManifestProvider>();
            try
            {
                suppliedProviders.AddRange(providers);
            }
            catch (Exception)
            {
                issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                    CharacterArtIntakeManifestCatalogueIssueCode.UnreadableProviderCollection,
                    "providers"));
                return snapshots;
            }

            for (var providerIndex = 0; providerIndex < suppliedProviders.Count; providerIndex++)
            {
                var provider = suppliedProviders[providerIndex];
                if (provider == null)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.NullProvider,
                        "providers"));
                    continue;
                }

                string moduleId;
                try
                {
                    moduleId = provider.ModuleId;
                }
                catch (Exception)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.UnreadableProvider,
                        "providers.moduleId"));
                    continue;
                }

                var providerPath = ProviderPath(moduleId);
                if (!IsStableId(moduleId))
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.InvalidModuleId,
                        providerPath + ".moduleId"));
                }

                IReadOnlyList<CharacterArtIntakeManifest> suppliedManifests;
                try
                {
                    suppliedManifests = provider.Manifests;
                }
                catch (Exception)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.UnreadableProvider,
                        providerPath + ".manifests"));
                    continue;
                }

                if (suppliedManifests == null)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.NullManifestCollection,
                        providerPath + ".manifests"));
                    continue;
                }

                int manifestCount;
                try
                {
                    manifestCount = suppliedManifests.Count;
                }
                catch (Exception)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.UnreadableManifestCollection,
                        providerPath + ".manifests"));
                    continue;
                }

                if (manifestCount < 0)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.UnreadableManifestCollection,
                        providerPath + ".manifests"));
                    continue;
                }

                var manifestCopy = new List<CharacterArtIntakeManifest>();
                for (var manifestIndex = 0; manifestIndex < manifestCount; manifestIndex++)
                {
                    CharacterArtIntakeManifest manifest;
                    try
                    {
                        manifest = suppliedManifests[manifestIndex];
                    }
                    catch (Exception)
                    {
                        issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                            CharacterArtIntakeManifestCatalogueIssueCode.UnreadableManifest,
                            providerPath + ".manifests[" + manifestIndex + "]"));
                        continue;
                    }

                    if (manifest == null)
                    {
                        issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                            CharacterArtIntakeManifestCatalogueIssueCode.NullManifest,
                            providerPath + ".manifests[" + manifestIndex + "]"));
                        continue;
                    }

                    manifestCopy.Add(manifest);
                }

                snapshots.Add(new ProviderSnapshot(moduleId, providerPath, manifestCopy));
            }

            return snapshots;
        }

        private static void AddDuplicateProviderIssues(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<CharacterArtIntakeManifestCatalogueIssue> issues)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < providers.Count; index++)
            {
                var moduleId = providers[index].ModuleId;
                if (!IsStableId(moduleId))
                    continue;
                counts[moduleId] = counts.TryGetValue(moduleId, out var count) ? count + 1 : 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.DuplicateProviderModuleId,
                        ProviderPath(pair.Key) + ".moduleId"));
                }
            }
        }

        private static List<ManifestSnapshot> SnapshotManifests(
            IReadOnlyList<ProviderSnapshot> providers)
        {
            var manifests = new List<ManifestSnapshot>();
            for (var providerIndex = 0; providerIndex < providers.Count; providerIndex++)
            {
                var provider = providers[providerIndex];
                for (var manifestIndex = 0; manifestIndex < provider.Manifests.Count; manifestIndex++)
                {
                    var manifest = provider.Manifests[manifestIndex];
                    manifests.Add(new ManifestSnapshot(
                        manifest,
                        ManifestPath(provider.Path, manifest.SourceId)));
                }
            }
            return manifests;
        }

        private static void ValidateManifests(
            IReadOnlyList<ManifestSnapshot> manifests,
            ICollection<CharacterArtIntakeManifestCatalogueIssue> issues)
        {
            for (var index = 0; index < manifests.Count; index++)
            {
                var snapshot = manifests[index];
                IReadOnlyList<CharacterArtManifestIssue> manifestIssues;
                try
                {
                    manifestIssues = CharacterArtIntakeManifestValidator.Validate(snapshot.Manifest);
                }
                catch (Exception)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.UnreadableManifest,
                        snapshot.Path));
                    continue;
                }

                for (var issueIndex = 0; issueIndex < manifestIssues.Count; issueIndex++)
                {
                    var manifestIssue = manifestIssues[issueIndex];
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.InvalidManifest,
                        snapshot.Path + "." + manifestIssue.Path,
                        manifestIssue.Code));
                }
            }
        }

        private static void AddDuplicateManifestIssues(
            IReadOnlyList<ManifestSnapshot> manifests,
            ICollection<CharacterArtIntakeManifestCatalogueIssue> issues)
        {
            var sourceIds = new Dictionary<string, int>(StringComparer.Ordinal);
            var characterIds = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < manifests.Count; index++)
            {
                var manifest = manifests[index].Manifest;
                AddCount(sourceIds, manifest.SourceId);
                AddCount(characterIds, manifest.CharacterId);
            }

            foreach (var pair in sourceIds)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.DuplicateSourceId,
                        "manifests[\"" + EscapePathSegment(pair.Key) + "\"].sourceId"));
                }
            }

            foreach (var pair in characterIds)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterArtIntakeManifestCatalogueIssue(
                        CharacterArtIntakeManifestCatalogueIssueCode.AmbiguousCharacterId,
                        "characters[\"" + EscapePathSegment(pair.Key) + "\"].characterId"));
                }
            }
        }

        private static void AddCount(IDictionary<string, int> counts, string id)
        {
            if (!IsStableId(id))
                return;
            counts[id] = counts.TryGetValue(id, out var count) ? count + 1 : 1;
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (!IsAlphaNumeric(value[0]) || !IsAlphaNumeric(value[value.Length - 1]))
                return false;

            var previousSeparator = false;
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                var separator = character == '.' || character == '-';
                if (!IsAlphaNumeric(character) && !separator || separator && previousSeparator)
                    return false;
                previousSeparator = separator;
            }
            return true;
        }

        private static bool IsAlphaNumeric(char character)
        {
            return character >= 'a' && character <= 'z' || character >= '0' && character <= '9';
        }

        private static void SortIssues(List<CharacterArtIntakeManifestCatalogueIssue> issues)
        {
            issues.Sort((left, right) =>
            {
                var path = string.CompareOrdinal(left.Path, right.Path);
                if (path != 0)
                    return path;
                var code = left.Code.CompareTo(right.Code);
                if (code != 0)
                    return code;
                return Nullable.Compare(left.ManifestIssueCode, right.ManifestIssueCode);
            });
        }

        private static string ProviderPath(string moduleId)
        {
            return string.IsNullOrEmpty(moduleId)
                ? "providers"
                : "providers[\"" + EscapePathSegment(moduleId) + "\"]";
        }

        private static string ManifestPath(string providerPath, string sourceId)
        {
            return string.IsNullOrEmpty(sourceId)
                ? providerPath + ".manifests"
                : providerPath + ".manifests[\"" + EscapePathSegment(sourceId) + "\"]";
        }

        private static string EscapePathSegment(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private sealed class ProviderSnapshot
        {
            public ProviderSnapshot(
                string moduleId,
                string path,
                List<CharacterArtIntakeManifest> manifests)
            {
                ModuleId = moduleId;
                Path = path;
                Manifests = manifests;
            }

            public string ModuleId { get; }
            public string Path { get; }
            public List<CharacterArtIntakeManifest> Manifests { get; }
        }

        private sealed class ManifestSnapshot
        {
            public ManifestSnapshot(CharacterArtIntakeManifest manifest, string path)
            {
                Manifest = manifest;
                Path = path;
            }

            public CharacterArtIntakeManifest Manifest { get; }
            public string Path { get; }
        }
    }
}
