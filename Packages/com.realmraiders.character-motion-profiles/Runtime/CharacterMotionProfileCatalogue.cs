using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.CharacterMotionProfiles
{
    /// <summary>A passive, explicitly supplied source of character motion profiles.</summary>
    public interface ICharacterMotionProfileProvider
    {
        string ModuleId { get; }
        IReadOnlyList<CharacterMotionProfile> Profiles { get; }
    }

    public enum CharacterMotionProfileCatalogueIssueCode
    {
        NullProviderCollection,
        UnreadableProviderCollection,
        NullProvider,
        UnreadableProvider,
        InvalidModuleId,
        DuplicateProviderModuleId,
        NullProfileCollection,
        UnreadableProfileCollection,
        NullProfile,
        UnreadableProfile,
        InvalidProfile,
        DuplicateMotionProfileId
    }

    public sealed class CharacterMotionProfileCatalogueIssue
    {
        public CharacterMotionProfileCatalogueIssue(
            CharacterMotionProfileCatalogueIssueCode code,
            string path,
            MotionProfileIssueCode? profileIssueCode = null)
        {
            Code = code;
            Path = path;
            ProfileIssueCode = profileIssueCode;
        }

        public CharacterMotionProfileCatalogueIssueCode Code { get; }
        public string Path { get; }
        public MotionProfileIssueCode? ProfileIssueCode { get; }
    }

    public sealed class CharacterMotionProfileCatalogueBuildResult
    {
        private readonly ReadOnlyCollection<CharacterMotionProfileCatalogueIssue> issues;

        internal CharacterMotionProfileCatalogueBuildResult(
            CharacterMotionProfileCatalogue catalogue,
            IList<CharacterMotionProfileCatalogueIssue> issues)
        {
            Catalogue = catalogue;
            this.issues = new ReadOnlyCollection<CharacterMotionProfileCatalogueIssue>(
                new List<CharacterMotionProfileCatalogueIssue>(issues));
        }

        public bool Succeeded => Catalogue != null;
        public CharacterMotionProfileCatalogue Catalogue { get; }
        public IReadOnlyList<CharacterMotionProfileCatalogueIssue> Issues => issues;
    }

    /// <summary>An immutable exact-lookup snapshot built only from explicitly supplied providers.</summary>
    public sealed class CharacterMotionProfileCatalogue
    {
        private readonly ReadOnlyCollection<CharacterMotionProfile> profiles;
        private readonly ReadOnlyDictionary<string, CharacterMotionProfile> profilesById;

        private CharacterMotionProfileCatalogue(IList<CharacterMotionProfile> profiles)
        {
            var profileCopy = new List<CharacterMotionProfile>(profiles);
            this.profiles = new ReadOnlyCollection<CharacterMotionProfile>(profileCopy);

            var profileIds = new Dictionary<string, CharacterMotionProfile>(StringComparer.Ordinal);
            for (var index = 0; index < profileCopy.Count; index++)
            {
                var profile = profileCopy[index];
                profileIds.Add(profile.MotionProfileId, profile);
            }
            profilesById = new ReadOnlyDictionary<string, CharacterMotionProfile>(profileIds);
        }

        public IReadOnlyList<CharacterMotionProfile> Profiles => profiles;

        public bool TryGetByMotionProfileId(string motionProfileId, out CharacterMotionProfile profile)
        {
            if (motionProfileId == null)
            {
                profile = null;
                return false;
            }

            return profilesById.TryGetValue(motionProfileId, out profile);
        }

        public static CharacterMotionProfileCatalogueBuildResult Build(
            IEnumerable<ICharacterMotionProfileProvider> providers)
        {
            var issues = new List<CharacterMotionProfileCatalogueIssue>();
            var providerSnapshots = SnapshotProviders(providers, issues);

            AddDuplicateProviderIssues(providerSnapshots, issues);
            var profileSnapshots = SnapshotProfiles(providerSnapshots, issues);
            ValidateProfiles(profileSnapshots, issues);
            AddDuplicateProfileIssues(profileSnapshots, issues);
            SortIssues(issues);

            if (issues.Count != 0)
                return new CharacterMotionProfileCatalogueBuildResult(null, issues);

            var profiles = new List<CharacterMotionProfile>(profileSnapshots.Count);
            for (var index = 0; index < profileSnapshots.Count; index++)
                profiles.Add(profileSnapshots[index].Profile);
            profiles.Sort((left, right) => string.CompareOrdinal(left.MotionProfileId, right.MotionProfileId));

            return new CharacterMotionProfileCatalogueBuildResult(
                new CharacterMotionProfileCatalogue(profiles),
                issues);
        }

        private static List<ProviderSnapshot> SnapshotProviders(
            IEnumerable<ICharacterMotionProfileProvider> providers,
            ICollection<CharacterMotionProfileCatalogueIssue> issues)
        {
            var snapshots = new List<ProviderSnapshot>();
            if (providers == null)
            {
                issues.Add(new CharacterMotionProfileCatalogueIssue(
                    CharacterMotionProfileCatalogueIssueCode.NullProviderCollection,
                    "providers"));
                return snapshots;
            }

            var suppliedProviders = new List<ICharacterMotionProfileProvider>();
            try
            {
                suppliedProviders.AddRange(providers);
            }
            catch (Exception)
            {
                issues.Add(new CharacterMotionProfileCatalogueIssue(
                    CharacterMotionProfileCatalogueIssueCode.UnreadableProviderCollection,
                    "providers"));
                return snapshots;
            }

            for (var providerIndex = 0; providerIndex < suppliedProviders.Count; providerIndex++)
            {
                var provider = suppliedProviders[providerIndex];
                if (provider == null)
                {
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.NullProvider,
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
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.UnreadableProvider,
                        "providers.moduleId"));
                    continue;
                }

                var providerPath = ProviderPath(moduleId);
                if (!CharacterMotionProfileValidator.IsStableId(moduleId))
                {
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.InvalidModuleId,
                        providerPath + ".moduleId"));
                }

                IReadOnlyList<CharacterMotionProfile> suppliedProfiles;
                try
                {
                    suppliedProfiles = provider.Profiles;
                }
                catch (Exception)
                {
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.UnreadableProvider,
                        providerPath + ".profiles"));
                    continue;
                }

                if (suppliedProfiles == null)
                {
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.NullProfileCollection,
                        providerPath + ".profiles"));
                    continue;
                }

                List<CharacterMotionProfile> profileCopy;
                try
                {
                    profileCopy = new List<CharacterMotionProfile>(suppliedProfiles);
                }
                catch (Exception)
                {
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.UnreadableProfileCollection,
                        providerPath + ".profiles"));
                    continue;
                }

                snapshots.Add(new ProviderSnapshot(moduleId, providerPath, profileCopy));
            }

            return snapshots;
        }

        private static void AddDuplicateProviderIssues(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<CharacterMotionProfileCatalogueIssue> issues)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < providers.Count; index++)
            {
                var moduleId = providers[index].ModuleId;
                if (!CharacterMotionProfileValidator.IsStableId(moduleId))
                    continue;
                counts[moduleId] = counts.TryGetValue(moduleId, out var count) ? count + 1 : 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.DuplicateProviderModuleId,
                        ProviderPath(pair.Key) + ".moduleId"));
                }
            }
        }

        private static List<ProfileSnapshot> SnapshotProfiles(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<CharacterMotionProfileCatalogueIssue> issues)
        {
            var profiles = new List<ProfileSnapshot>();
            for (var providerIndex = 0; providerIndex < providers.Count; providerIndex++)
            {
                var provider = providers[providerIndex];
                for (var profileIndex = 0; profileIndex < provider.Profiles.Count; profileIndex++)
                {
                    var profile = provider.Profiles[profileIndex];
                    if (profile == null)
                    {
                        issues.Add(new CharacterMotionProfileCatalogueIssue(
                            CharacterMotionProfileCatalogueIssueCode.NullProfile,
                            provider.Path + ".profiles"));
                        continue;
                    }

                    profiles.Add(new ProfileSnapshot(
                        profile,
                        ProfilePath(provider.Path, profile.MotionProfileId)));
                }
            }
            return profiles;
        }

        private static void ValidateProfiles(
            IReadOnlyList<ProfileSnapshot> profiles,
            ICollection<CharacterMotionProfileCatalogueIssue> issues)
        {
            for (var index = 0; index < profiles.Count; index++)
            {
                var snapshot = profiles[index];
                IReadOnlyList<MotionProfileIssue> profileIssues;
                try
                {
                    profileIssues = CharacterMotionProfileValidator.Validate(snapshot.Profile);
                }
                catch (Exception)
                {
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.UnreadableProfile,
                        snapshot.Path));
                    continue;
                }
                for (var issueIndex = 0; issueIndex < profileIssues.Count; issueIndex++)
                {
                    var profileIssue = profileIssues[issueIndex];
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.InvalidProfile,
                        snapshot.Path + "." + profileIssue.Path,
                        profileIssue.Code));
                }
            }
        }

        private static void AddDuplicateProfileIssues(
            IReadOnlyList<ProfileSnapshot> profiles,
            ICollection<CharacterMotionProfileCatalogueIssue> issues)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < profiles.Count; index++)
            {
                var profileId = profiles[index].Profile.MotionProfileId;
                if (!CharacterMotionProfileValidator.IsStableId(profileId))
                    continue;
                counts[profileId] = counts.TryGetValue(profileId, out var count) ? count + 1 : 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterMotionProfileCatalogueIssue(
                        CharacterMotionProfileCatalogueIssueCode.DuplicateMotionProfileId,
                        "profiles[\"" + EscapePathSegment(pair.Key) + "\"].motionProfileId"));
                }
            }
        }

        private static void SortIssues(List<CharacterMotionProfileCatalogueIssue> issues)
        {
            issues.Sort((left, right) =>
            {
                var path = string.CompareOrdinal(left.Path, right.Path);
                if (path != 0)
                    return path;
                var code = left.Code.CompareTo(right.Code);
                if (code != 0)
                    return code;
                return Nullable.Compare(left.ProfileIssueCode, right.ProfileIssueCode);
            });
        }

        private static string ProviderPath(string moduleId)
        {
            return string.IsNullOrEmpty(moduleId)
                ? "providers"
                : "providers[\"" + EscapePathSegment(moduleId) + "\"]";
        }

        private static string ProfilePath(string providerPath, string profileId)
        {
            return string.IsNullOrEmpty(profileId)
                ? providerPath + ".profiles"
                : providerPath + ".profiles[\"" + EscapePathSegment(profileId) + "\"]";
        }

        private static string EscapePathSegment(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private sealed class ProviderSnapshot
        {
            public ProviderSnapshot(string moduleId, string path, List<CharacterMotionProfile> profiles)
            {
                ModuleId = moduleId;
                Path = path;
                Profiles = profiles;
            }

            public string ModuleId { get; }
            public string Path { get; }
            public List<CharacterMotionProfile> Profiles { get; }
        }

        private sealed class ProfileSnapshot
        {
            public ProfileSnapshot(CharacterMotionProfile profile, string path)
            {
                Profile = profile;
                Path = path;
            }

            public CharacterMotionProfile Profile { get; }
            public string Path { get; }
        }
    }
}
