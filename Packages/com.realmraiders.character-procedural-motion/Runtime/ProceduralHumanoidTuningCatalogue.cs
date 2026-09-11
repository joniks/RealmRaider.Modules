using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.CharacterProceduralMotion
{
    /// <summary>An immutable explicit mapping from a stable profile identifier to procedural tuning.</summary>
    public sealed class ProceduralHumanoidTuningProfile
    {
        public ProceduralHumanoidTuningProfile(
            string profileId,
            ProceduralHumanoidMotionTuning tuning)
        {
            ProfileId = profileId;
            Tuning = tuning;
        }

        public string ProfileId { get; }

        public ProceduralHumanoidMotionTuning Tuning { get; }
    }

    /// <summary>A passive explicit source of tuning profiles. It performs no discovery or application.</summary>
    public interface IProceduralHumanoidTuningProvider
    {
        string ProviderId { get; }

        IReadOnlyList<ProceduralHumanoidTuningProfile> Profiles { get; }
    }

    public enum ProceduralHumanoidTuningCatalogueIssueCode
    {
        NullProviderCollection,
        UnreadableProviderCollection,
        NullProvider,
        UnreadableProvider,
        InvalidProviderId,
        DuplicateProviderId,
        NullProfileCollection,
        UnreadableProfileCollection,
        NullProfile,
        InvalidProfileId,
        NullTuning,
        DuplicateProfileId
    }

    public sealed class ProceduralHumanoidTuningCatalogueIssue
    {
        public ProceduralHumanoidTuningCatalogueIssue(
            ProceduralHumanoidTuningCatalogueIssueCode code,
            string path)
        {
            Code = code;
            Path = path;
        }

        public ProceduralHumanoidTuningCatalogueIssueCode Code { get; }

        public string Path { get; }

        public string Signature => Code + "|" + Path;
    }

    public sealed class ProceduralHumanoidTuningCatalogueBuildResult
    {
        private readonly ReadOnlyCollection<ProceduralHumanoidTuningCatalogueIssue> issues;

        internal ProceduralHumanoidTuningCatalogueBuildResult(
            ProceduralHumanoidTuningCatalogue catalogue,
            IList<ProceduralHumanoidTuningCatalogueIssue> issues)
        {
            Catalogue = catalogue;
            this.issues = new ReadOnlyCollection<ProceduralHumanoidTuningCatalogueIssue>(
                new List<ProceduralHumanoidTuningCatalogueIssue>(issues));
        }

        public bool Succeeded => Catalogue != null;

        public ProceduralHumanoidTuningCatalogue Catalogue { get; }

        public IReadOnlyList<ProceduralHumanoidTuningCatalogueIssue> Issues => issues;
    }

    /// <summary>
    /// Immutable exact-lookup snapshot of caller-supplied procedural humanoid tuning profiles.
    /// It does not inspect Unity, choose a profile automatically, or apply tuning to a runtime driver.
    /// </summary>
    public sealed class ProceduralHumanoidTuningCatalogue
    {
        private readonly ReadOnlyCollection<ProceduralHumanoidTuningProfile> profiles;
        private readonly ReadOnlyDictionary<string, ProceduralHumanoidTuningProfile> profilesById;

        private ProceduralHumanoidTuningCatalogue(
            IList<ProceduralHumanoidTuningProfile> profiles)
        {
            var profileCopy = new List<ProceduralHumanoidTuningProfile>(profiles);
            this.profiles = new ReadOnlyCollection<ProceduralHumanoidTuningProfile>(profileCopy);

            var lookup = new Dictionary<string, ProceduralHumanoidTuningProfile>(
                StringComparer.Ordinal);
            for (var index = 0; index < profileCopy.Count; index++)
            {
                var profile = profileCopy[index];
                lookup.Add(profile.ProfileId, profile);
            }

            profilesById = new ReadOnlyDictionary<string, ProceduralHumanoidTuningProfile>(
                lookup);
        }

        public IReadOnlyList<ProceduralHumanoidTuningProfile> Profiles => profiles;

        public bool TryGetByProfileId(
            string profileId,
            out ProceduralHumanoidTuningProfile profile)
        {
            if (profileId == null)
            {
                profile = null;
                return false;
            }

            return profilesById.TryGetValue(profileId, out profile);
        }

        public static ProceduralHumanoidTuningCatalogueBuildResult Build(
            IEnumerable<IProceduralHumanoidTuningProvider> providers)
        {
            var issues = new List<ProceduralHumanoidTuningCatalogueIssue>();
            var providerSnapshots = SnapshotProviders(providers, issues);

            AddDuplicateProviderIssues(providerSnapshots, issues);
            var profileSnapshots = SnapshotProfiles(providerSnapshots, issues);
            ValidateProfiles(profileSnapshots, issues);
            AddDuplicateProfileIssues(profileSnapshots, issues);
            SortIssues(issues);

            if (issues.Count != 0)
            {
                return new ProceduralHumanoidTuningCatalogueBuildResult(
                    null,
                    issues);
            }

            var profiles = new List<ProceduralHumanoidTuningProfile>(
                profileSnapshots.Count);
            for (var index = 0; index < profileSnapshots.Count; index++)
            {
                profiles.Add(profileSnapshots[index].Profile);
            }

            profiles.Sort((left, right) => string.CompareOrdinal(
                left.ProfileId,
                right.ProfileId));
            return new ProceduralHumanoidTuningCatalogueBuildResult(
                new ProceduralHumanoidTuningCatalogue(profiles),
                issues);
        }

        private static List<ProviderSnapshot> SnapshotProviders(
            IEnumerable<IProceduralHumanoidTuningProvider> providers,
            ICollection<ProceduralHumanoidTuningCatalogueIssue> issues)
        {
            var snapshots = new List<ProviderSnapshot>();
            if (providers == null)
            {
                issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                    ProceduralHumanoidTuningCatalogueIssueCode.NullProviderCollection,
                    "providers"));
                return snapshots;
            }

            var suppliedProviders = new List<IProceduralHumanoidTuningProvider>();
            try
            {
                suppliedProviders.AddRange(providers);
            }
            catch (Exception)
            {
                issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                    ProceduralHumanoidTuningCatalogueIssueCode.UnreadableProviderCollection,
                    "providers"));
                return snapshots;
            }

            for (var providerIndex = 0; providerIndex < suppliedProviders.Count; providerIndex++)
            {
                var provider = suppliedProviders[providerIndex];
                if (provider == null)
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.NullProvider,
                        "providers"));
                    continue;
                }

                string providerId;
                try
                {
                    providerId = provider.ProviderId;
                }
                catch (Exception)
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.UnreadableProvider,
                        "providers.providerId"));
                    continue;
                }

                var providerPath = ProviderPath(providerId);
                if (!IsStableId(providerId))
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.InvalidProviderId,
                        providerPath + ".providerId"));
                }

                IReadOnlyList<ProceduralHumanoidTuningProfile> suppliedProfiles;
                try
                {
                    suppliedProfiles = provider.Profiles;
                }
                catch (Exception)
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.UnreadableProvider,
                        providerPath + ".profiles"));
                    continue;
                }

                if (suppliedProfiles == null)
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.NullProfileCollection,
                        providerPath + ".profiles"));
                    continue;
                }

                List<ProceduralHumanoidTuningProfile> profileCopy;
                try
                {
                    profileCopy = new List<ProceduralHumanoidTuningProfile>(
                        suppliedProfiles);
                }
                catch (Exception)
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.UnreadableProfileCollection,
                        providerPath + ".profiles"));
                    continue;
                }

                snapshots.Add(new ProviderSnapshot(
                    providerId,
                    providerPath,
                    profileCopy));
            }

            return snapshots;
        }

        private static void AddDuplicateProviderIssues(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<ProceduralHumanoidTuningCatalogueIssue> issues)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < providers.Count; index++)
            {
                var providerId = providers[index].ProviderId;
                if (!IsStableId(providerId))
                {
                    continue;
                }

                counts[providerId] = counts.TryGetValue(providerId, out var count)
                    ? count + 1
                    : 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.DuplicateProviderId,
                        ProviderPath(pair.Key) + ".providerId"));
                }
            }
        }

        private static List<ProfileSnapshot> SnapshotProfiles(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<ProceduralHumanoidTuningCatalogueIssue> issues)
        {
            var snapshots = new List<ProfileSnapshot>();
            for (var providerIndex = 0; providerIndex < providers.Count; providerIndex++)
            {
                var provider = providers[providerIndex];
                for (var profileIndex = 0; profileIndex < provider.Profiles.Count; profileIndex++)
                {
                    var profile = provider.Profiles[profileIndex];
                    if (profile == null)
                    {
                        issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                            ProceduralHumanoidTuningCatalogueIssueCode.NullProfile,
                            provider.Path + ".profiles"));
                        continue;
                    }

                    snapshots.Add(new ProfileSnapshot(
                        profile,
                        ProfilePath(provider.Path, profile.ProfileId)));
                }
            }

            return snapshots;
        }

        private static void ValidateProfiles(
            IReadOnlyList<ProfileSnapshot> profiles,
            ICollection<ProceduralHumanoidTuningCatalogueIssue> issues)
        {
            for (var index = 0; index < profiles.Count; index++)
            {
                var snapshot = profiles[index];
                if (!IsStableId(snapshot.Profile.ProfileId))
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.InvalidProfileId,
                        snapshot.Path + ".profileId"));
                }

                if (snapshot.Profile.Tuning == null)
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.NullTuning,
                        snapshot.Path + ".tuning"));
                }
            }
        }

        private static void AddDuplicateProfileIssues(
            IReadOnlyList<ProfileSnapshot> profiles,
            ICollection<ProceduralHumanoidTuningCatalogueIssue> issues)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < profiles.Count; index++)
            {
                var profileId = profiles[index].Profile.ProfileId;
                if (!IsStableId(profileId))
                {
                    continue;
                }

                counts[profileId] = counts.TryGetValue(profileId, out var count)
                    ? count + 1
                    : 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new ProceduralHumanoidTuningCatalogueIssue(
                        ProceduralHumanoidTuningCatalogueIssueCode.DuplicateProfileId,
                        "profiles[\"" + EscapePathSegment(pair.Key) + "\"].profileId"));
                }
            }
        }

        private static void SortIssues(
            List<ProceduralHumanoidTuningCatalogueIssue> issues)
        {
            issues.Sort((left, right) =>
            {
                var path = string.CompareOrdinal(left.Path, right.Path);
                if (path != 0)
                {
                    return path;
                }

                return left.Code.CompareTo(right.Code);
            });
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!IsAlphaNumeric(value[0]) || !IsAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            var previousSeparator = false;
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                var separator = character == '.' || character == '-';
                if ((!IsAlphaNumeric(character) && !separator)
                    || (separator && previousSeparator))
                {
                    return false;
                }

                previousSeparator = separator;
            }

            return true;
        }

        private static bool IsAlphaNumeric(char character)
        {
            return (character >= 'a' && character <= 'z')
                || (character >= '0' && character <= '9');
        }

        private static string ProviderPath(string providerId)
        {
            return string.IsNullOrEmpty(providerId)
                ? "providers"
                : "providers[\"" + EscapePathSegment(providerId) + "\"]";
        }

        private static string ProfilePath(
            string providerPath,
            string profileId)
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
            public ProviderSnapshot(
                string providerId,
                string path,
                List<ProceduralHumanoidTuningProfile> profiles)
            {
                ProviderId = providerId;
                Path = path;
                Profiles = profiles;
            }

            public string ProviderId { get; }

            public string Path { get; }

            public List<ProceduralHumanoidTuningProfile> Profiles { get; }
        }

        private sealed class ProfileSnapshot
        {
            public ProfileSnapshot(
                ProceduralHumanoidTuningProfile profile,
                string path)
            {
                Profile = profile;
                Path = path;
            }

            public ProceduralHumanoidTuningProfile Profile { get; }

            public string Path { get; }
        }
    }

    /// <summary>Explicit starter entries; consumers choose a profile by its stable identifier.</summary>
    public sealed class StarterProceduralHumanoidTuningProvider : IProceduralHumanoidTuningProvider
    {
        public const string StarterProviderId = "realmraiders.procedural-humanoid.starter.v1";
        public const string CompatibilityProfileId = "realmraiders.procedural-humanoid.compatibility.v1";
        public const string BloodKnightDeviceReadableProfileId = "realmraiders.procedural-humanoid.blood-knight-device-readable.v1";

        private static readonly ReadOnlyCollection<ProceduralHumanoidTuningProfile> profiles =
            new ReadOnlyCollection<ProceduralHumanoidTuningProfile>(
                new List<ProceduralHumanoidTuningProfile>
                {
                    new ProceduralHumanoidTuningProfile(
                        CompatibilityProfileId,
                        ProceduralHumanoidMotionTuning.CompatibilityDefault),
                    new ProceduralHumanoidTuningProfile(
                        BloodKnightDeviceReadableProfileId,
                        ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable)
                });

        public string ProviderId => StarterProviderId;

        public IReadOnlyList<ProceduralHumanoidTuningProfile> Profiles => profiles;
    }
}
