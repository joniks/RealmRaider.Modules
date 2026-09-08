using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.CharacterVisualTuning
{
    /// <summary>A passive, explicitly supplied source of visual-tuning profiles.</summary>
    public interface ICharacterVisualTuningProfileProvider
    {
        string ModuleId { get; }
        IReadOnlyList<CharacterVisualTuningProfile> Profiles { get; }
    }

    public enum CharacterVisualTuningCatalogueIssueCode
    {
        NullProviderCollection,
        UnreadableProviderCollection,
        NullProvider,
        UnreadableProvider,
        InvalidProviderModuleId,
        DuplicateProviderModuleId,
        NullProfileCollection,
        UnreadableProfileCollection,
        NullProfile,
        UnreadableProfile,
        InvalidProfileId,
        InvalidSourceId,
        InvalidProfile,
        DuplicateProfileId,
        DuplicateSourceId,
        UnreadableInput
    }

    public sealed class CharacterVisualTuningCatalogueIssue
    {
        public CharacterVisualTuningCatalogueIssue(
            CharacterVisualTuningCatalogueIssueCode code,
            string path,
            VisualTuningIssueCode? profileIssueCode = null)
        {
            Code = code;
            Path = path;
            ProfileIssueCode = profileIssueCode;
        }

        public CharacterVisualTuningCatalogueIssueCode Code { get; }
        public string Path { get; }
        public VisualTuningIssueCode? ProfileIssueCode { get; }
    }

    public sealed class CharacterVisualTuningCatalogueBuildResult
    {
        private readonly ReadOnlyCollection<CharacterVisualTuningCatalogueIssue> issues;

        internal CharacterVisualTuningCatalogueBuildResult(
            CharacterVisualTuningCatalogue catalogue,
            IList<CharacterVisualTuningCatalogueIssue> issues)
        {
            Catalogue = catalogue;
            this.issues = new ReadOnlyCollection<CharacterVisualTuningCatalogueIssue>(
                new List<CharacterVisualTuningCatalogueIssue>(issues));
        }

        public bool Succeeded => Catalogue != null;
        public CharacterVisualTuningCatalogue Catalogue { get; }
        public IReadOnlyList<CharacterVisualTuningCatalogueIssue> Issues => issues;
    }

    /// <summary>
    /// Immutable ProfileId-sorted exact-lookup snapshot built from explicit providers only.
    /// </summary>
    public sealed class CharacterVisualTuningCatalogue
    {
        private readonly ReadOnlyCollection<CharacterVisualTuningProfile> profiles;
        private readonly ReadOnlyDictionary<string, CharacterVisualTuningProfile> profilesById;
        private readonly ReadOnlyDictionary<string, CharacterVisualTuningProfile> profilesBySourceId;

        private CharacterVisualTuningCatalogue(IList<CharacterVisualTuningProfile> profiles)
        {
            var profileCopy = new List<CharacterVisualTuningProfile>(profiles);
            this.profiles = new ReadOnlyCollection<CharacterVisualTuningProfile>(profileCopy);

            var profileIds = new Dictionary<string, CharacterVisualTuningProfile>(StringComparer.Ordinal);
            var sourceIds = new Dictionary<string, CharacterVisualTuningProfile>(StringComparer.Ordinal);
            for (var index = 0; index < profileCopy.Count; index++)
            {
                var profile = profileCopy[index];
                profileIds.Add(profile.ProfileId, profile);
                sourceIds.Add(profile.SourceId, profile);
            }
            profilesById = new ReadOnlyDictionary<string, CharacterVisualTuningProfile>(profileIds);
            profilesBySourceId = new ReadOnlyDictionary<string, CharacterVisualTuningProfile>(sourceIds);
        }

        public IReadOnlyList<CharacterVisualTuningProfile> Profiles => profiles;

        public bool TryGetByProfileId(
            string profileId,
            out CharacterVisualTuningProfile profile)
        {
            if (profileId == null)
            {
                profile = null;
                return false;
            }
            return profilesById.TryGetValue(profileId, out profile);
        }

        public bool TryGetBySourceId(
            string sourceId,
            out CharacterVisualTuningProfile profile)
        {
            if (sourceId == null)
            {
                profile = null;
                return false;
            }
            return profilesBySourceId.TryGetValue(sourceId, out profile);
        }

        public static CharacterVisualTuningCatalogueBuildResult Build(
            IEnumerable<ICharacterVisualTuningProfileProvider> providers)
        {
            try
            {
                return BuildCore(providers);
            }
            catch (Exception)
            {
                return Failure(
                    CharacterVisualTuningCatalogueIssueCode.UnreadableInput,
                    "catalogue");
            }
        }

        private static CharacterVisualTuningCatalogueBuildResult BuildCore(
            IEnumerable<ICharacterVisualTuningProfileProvider> providers)
        {
            var issues = new List<CharacterVisualTuningCatalogueIssue>();
            var providerSnapshots = SnapshotProviders(providers, issues);
            AddDuplicateProviderIssues(providerSnapshots, issues);
            var profileSnapshots = SnapshotProfiles(providerSnapshots, issues);
            ValidateProfiles(profileSnapshots, issues);
            AddDuplicateProfileIssues(profileSnapshots, issues);
            SortIssues(issues);

            if (issues.Count != 0)
                return new CharacterVisualTuningCatalogueBuildResult(null, issues);

            var validProfiles = new List<CharacterVisualTuningProfile>(profileSnapshots.Count);
            for (var index = 0; index < profileSnapshots.Count; index++)
                validProfiles.Add(profileSnapshots[index].Profile);
            validProfiles.Sort((left, right) =>
                StringComparer.Ordinal.Compare(left.ProfileId, right.ProfileId));
            return new CharacterVisualTuningCatalogueBuildResult(
                new CharacterVisualTuningCatalogue(validProfiles),
                issues);
        }

        private static List<ProviderSnapshot> SnapshotProviders(
            IEnumerable<ICharacterVisualTuningProfileProvider> providers,
            ICollection<CharacterVisualTuningCatalogueIssue> issues)
        {
            var source = new List<ICharacterVisualTuningProfileProvider>();
            if (providers == null)
            {
                issues.Add(new CharacterVisualTuningCatalogueIssue(
                    CharacterVisualTuningCatalogueIssueCode.NullProviderCollection,
                    "providers"));
                return new List<ProviderSnapshot>();
            }

            try
            {
                foreach (var provider in providers)
                    source.Add(provider);
            }
            catch (Exception)
            {
                issues.Add(new CharacterVisualTuningCatalogueIssue(
                    CharacterVisualTuningCatalogueIssueCode.UnreadableProviderCollection,
                    "providers"));
                return new List<ProviderSnapshot>();
            }

            var snapshots = new List<ProviderSnapshot>(source.Count);
            for (var index = 0; index < source.Count; index++)
            {
                var provider = source[index];
                var indexedPath = "providers[" + index + "]";
                if (provider == null)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.NullProvider,
                        indexedPath));
                    continue;
                }

                string moduleId;
                try
                {
                    moduleId = provider.ModuleId;
                }
                catch (Exception)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.UnreadableProvider,
                        indexedPath + ".moduleId"));
                    continue;
                }

                var providerPath = IsStableId(moduleId)
                    ? "providers[" + moduleId + "]"
                    : indexedPath;
                if (!IsStableId(moduleId))
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.InvalidProviderModuleId,
                        providerPath + ".moduleId"));
                }

                IReadOnlyList<CharacterVisualTuningProfile> profiles;
                try
                {
                    profiles = provider.Profiles;
                }
                catch (Exception)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.UnreadableProvider,
                        providerPath + ".profiles"));
                    continue;
                }
                snapshots.Add(new ProviderSnapshot(moduleId, profiles, providerPath));
            }
            snapshots.Sort(CompareProviders);
            return snapshots;
        }

        private static void AddDuplicateProviderIssues(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<CharacterVisualTuningCatalogueIssue> issues)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < providers.Count; index++)
            {
                var moduleId = providers[index].ModuleId;
                if (!IsStableId(moduleId))
                    continue;
                counts[moduleId] = counts.TryGetValue(moduleId, out var count)
                    ? count + 1
                    : 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.DuplicateProviderModuleId,
                        "providers[" + pair.Key + "].moduleId"));
                }
            }
        }

        private static List<ProfileSnapshot> SnapshotProfiles(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<CharacterVisualTuningCatalogueIssue> issues)
        {
            var snapshots = new List<ProfileSnapshot>();
            for (var providerIndex = 0; providerIndex < providers.Count; providerIndex++)
            {
                var provider = providers[providerIndex];
                if (provider.Profiles == null)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.NullProfileCollection,
                        provider.Path + ".profiles"));
                    continue;
                }

                int count;
                try
                {
                    count = provider.Profiles.Count;
                }
                catch (Exception)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.UnreadableProfileCollection,
                        provider.Path + ".profiles"));
                    continue;
                }
                if (count < 0)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.UnreadableProfileCollection,
                        provider.Path + ".profiles"));
                    continue;
                }

                for (var profileIndex = 0; profileIndex < count; profileIndex++)
                {
                    var indexedPath = provider.Path + ".profiles[" + profileIndex + "]";
                    CharacterVisualTuningProfile profile;
                    try
                    {
                        profile = provider.Profiles[profileIndex];
                    }
                    catch (Exception)
                    {
                        issues.Add(new CharacterVisualTuningCatalogueIssue(
                            CharacterVisualTuningCatalogueIssueCode.UnreadableProfile,
                            indexedPath));
                        continue;
                    }
                    if (profile == null)
                    {
                        issues.Add(new CharacterVisualTuningCatalogueIssue(
                            CharacterVisualTuningCatalogueIssueCode.NullProfile,
                            indexedPath));
                        continue;
                    }

                    var path = IsStableId(profile.ProfileId)
                        ? provider.Path + ".profiles[" + profile.ProfileId + "]"
                        : indexedPath;
                    snapshots.Add(new ProfileSnapshot(provider.ModuleId, profile, path));
                }
            }
            snapshots.Sort(CompareProfiles);
            return snapshots;
        }

        private static void ValidateProfiles(
            IReadOnlyList<ProfileSnapshot> profiles,
            ICollection<CharacterVisualTuningCatalogueIssue> issues)
        {
            for (var index = 0; index < profiles.Count; index++)
            {
                var snapshot = profiles[index];
                IReadOnlyList<VisualTuningIssue> profileIssues;
                try
                {
                    profileIssues = VisualTuningValidator.Validate(snapshot.Profile);
                }
                catch (Exception)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.UnreadableProfile,
                        snapshot.Path));
                    continue;
                }

                var profileIdIssue = false;
                var sourceIdIssue = false;
                for (var issueIndex = 0; issueIndex < profileIssues.Count; issueIndex++)
                {
                    var issue = profileIssues[issueIndex];
                    var code = MapProfileIssue(issue.Code);
                    if (code == CharacterVisualTuningCatalogueIssueCode.InvalidProfileId)
                        profileIdIssue = true;
                    if (code == CharacterVisualTuningCatalogueIssueCode.InvalidSourceId)
                        sourceIdIssue = true;
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        code,
                        snapshot.Path + "." + issue.Path,
                        issue.Code));
                }

                if (!profileIdIssue && !IsStableId(snapshot.Profile.ProfileId))
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.InvalidProfileId,
                        snapshot.Path + ".profileId"));
                }
                if (!sourceIdIssue && !IsStableId(snapshot.Profile.SourceId))
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.InvalidSourceId,
                        snapshot.Path + ".sourceId"));
                }
            }
        }

        private static CharacterVisualTuningCatalogueIssueCode MapProfileIssue(
            VisualTuningIssueCode issueCode)
        {
            if (issueCode == VisualTuningIssueCode.MissingProfileId ||
                issueCode == VisualTuningIssueCode.InvalidProfileId)
            {
                return CharacterVisualTuningCatalogueIssueCode.InvalidProfileId;
            }
            if (issueCode == VisualTuningIssueCode.MissingSourceId ||
                issueCode == VisualTuningIssueCode.InvalidSourceId)
            {
                return CharacterVisualTuningCatalogueIssueCode.InvalidSourceId;
            }
            return CharacterVisualTuningCatalogueIssueCode.InvalidProfile;
        }

        private static void AddDuplicateProfileIssues(
            IReadOnlyList<ProfileSnapshot> profiles,
            ICollection<CharacterVisualTuningCatalogueIssue> issues)
        {
            var profileIds = new Dictionary<string, int>(StringComparer.Ordinal);
            var sourceIds = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < profiles.Count; index++)
            {
                var profile = profiles[index].Profile;
                if (IsStableId(profile.ProfileId))
                    AddCount(profileIds, profile.ProfileId);
                if (IsStableId(profile.SourceId))
                    AddCount(sourceIds, profile.SourceId);
            }

            foreach (var pair in profileIds)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.DuplicateProfileId,
                        "profiles[" + pair.Key + "].profileId"));
                }
            }
            foreach (var pair in sourceIds)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterVisualTuningCatalogueIssue(
                        CharacterVisualTuningCatalogueIssueCode.DuplicateSourceId,
                        "sources[" + pair.Key + "].sourceId"));
                }
            }
        }

        private static void AddCount(IDictionary<string, int> counts, string value)
        {
            counts[value] = counts.TryGetValue(value, out var count)
                ? count + 1
                : 1;
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                !IsAlphaNumeric(value[0]) ||
                !IsAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

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
            return character >= 'a' && character <= 'z' ||
                character >= '0' && character <= '9';
        }

        private static int CompareProviders(ProviderSnapshot left, ProviderSnapshot right)
        {
            var moduleId = StringComparer.Ordinal.Compare(left.ModuleId, right.ModuleId);
            return moduleId != 0
                ? moduleId
                : StringComparer.Ordinal.Compare(left.Path, right.Path);
        }

        private static int CompareProfiles(ProfileSnapshot left, ProfileSnapshot right)
        {
            var profileId = StringComparer.Ordinal.Compare(
                left.Profile.ProfileId,
                right.Profile.ProfileId);
            if (profileId != 0)
                return profileId;
            var sourceId = StringComparer.Ordinal.Compare(
                left.Profile.SourceId,
                right.Profile.SourceId);
            if (sourceId != 0)
                return sourceId;
            return StringComparer.Ordinal.Compare(left.ModuleId, right.ModuleId);
        }

        private static void SortIssues(List<CharacterVisualTuningCatalogueIssue> issues)
        {
            issues.Sort((left, right) =>
            {
                var path = StringComparer.Ordinal.Compare(left.Path, right.Path);
                if (path != 0)
                    return path;
                var code = left.Code.CompareTo(right.Code);
                return code != 0
                    ? code
                    : Nullable.Compare(left.ProfileIssueCode, right.ProfileIssueCode);
            });
        }

        private static CharacterVisualTuningCatalogueBuildResult Failure(
            CharacterVisualTuningCatalogueIssueCode code,
            string path)
        {
            return new CharacterVisualTuningCatalogueBuildResult(
                null,
                new List<CharacterVisualTuningCatalogueIssue>
                {
                    new CharacterVisualTuningCatalogueIssue(code, path)
                });
        }

        private sealed class ProviderSnapshot
        {
            public ProviderSnapshot(
                string moduleId,
                IReadOnlyList<CharacterVisualTuningProfile> profiles,
                string path)
            {
                ModuleId = moduleId;
                Profiles = profiles;
                Path = path;
            }

            public string ModuleId { get; }
            public IReadOnlyList<CharacterVisualTuningProfile> Profiles { get; }
            public string Path { get; }
        }

        private sealed class ProfileSnapshot
        {
            public ProfileSnapshot(
                string moduleId,
                CharacterVisualTuningProfile profile,
                string path)
            {
                ModuleId = moduleId;
                Profile = profile;
                Path = path;
            }

            public string ModuleId { get; }
            public CharacterVisualTuningProfile Profile { get; }
            public string Path { get; }
        }
    }
}
