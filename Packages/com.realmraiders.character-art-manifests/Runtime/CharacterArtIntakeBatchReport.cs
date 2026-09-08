using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.CharacterArtManifests
{
    public sealed class CharacterArtIntakeBatchItem
    {
        public CharacterArtIntakeBatchItem(
            CharacterArtIntakeManifest manifest,
            CharacterArtMeasurementSnapshot measurement)
        {
            Manifest = manifest;
            Measurement = measurement;
        }

        public CharacterArtIntakeManifest Manifest { get; }
        public CharacterArtMeasurementSnapshot Measurement { get; }
    }

    /// <summary>A passive, explicitly supplied source of art-intake batch items.</summary>
    public interface ICharacterArtIntakeBatchProvider
    {
        string ModuleId { get; }
        IReadOnlyList<CharacterArtIntakeBatchItem> Items { get; }
    }

    public enum CharacterArtIntakeBatchBuildIssueCode
    {
        NullProviderCollection,
        UnreadableProviderCollection,
        NullProvider,
        UnreadableProvider,
        InvalidModuleId,
        DuplicateProviderModuleId,
        NullItemCollection,
        UnreadableItemCollection,
        NullItem,
        UnreadableItem,
        MissingItemManifest,
        InvalidItemSourceId,
        InvalidItemCharacterId,
        DuplicateSourceId,
        DuplicateCharacterId,
        UnreadableInput
    }

    public sealed class CharacterArtIntakeBatchBuildIssue
    {
        public CharacterArtIntakeBatchBuildIssue(
            CharacterArtIntakeBatchBuildIssueCode code,
            string path)
        {
            Code = code;
            Path = path;
        }

        public CharacterArtIntakeBatchBuildIssueCode Code { get; }
        public string Path { get; }
    }

    public sealed class CharacterArtIntakeBatchBuildResult
    {
        private readonly ReadOnlyCollection<CharacterArtIntakeBatchBuildIssue> issues;

        internal CharacterArtIntakeBatchBuildResult(
            CharacterArtIntakeBatchReport report,
            IList<CharacterArtIntakeBatchBuildIssue> issues)
        {
            Report = report;
            this.issues = new ReadOnlyCollection<CharacterArtIntakeBatchBuildIssue>(
                new List<CharacterArtIntakeBatchBuildIssue>(issues));
        }

        public bool Succeeded => Report != null;
        public CharacterArtIntakeBatchReport Report { get; }
        public IReadOnlyList<CharacterArtIntakeBatchBuildIssue> Issues => issues;
    }

    public sealed class CharacterArtIntakeBatchItemIssue
    {
        internal CharacterArtIntakeBatchItemIssue(
            CharacterArtIntakeComplianceIssue issue,
            string path)
        {
            Code = issue.Code;
            Path = path;
            ManifestIssueCode = issue.ManifestIssueCode;
        }

        public CharacterArtIntakeComplianceIssueCode Code { get; }
        public string Path { get; }
        public CharacterArtManifestIssueCode? ManifestIssueCode { get; }
    }

    public sealed class CharacterArtIntakeBatchReportItem
    {
        private readonly ReadOnlyCollection<CharacterArtIntakeBatchItemIssue> issues;

        internal CharacterArtIntakeBatchReportItem(
            string moduleId,
            CharacterArtIntakeBatchItem input,
            CharacterArtIntakeComplianceResult result)
        {
            ModuleId = moduleId;
            Manifest = input.Manifest;
            Measurement = input.Measurement;

            var itemIssues = new List<CharacterArtIntakeBatchItemIssue>(result.Issues.Count);
            var itemPath = "items[" + Manifest.SourceId + "].";
            for (var index = 0; index < result.Issues.Count; index++)
            {
                var issue = result.Issues[index];
                itemIssues.Add(new CharacterArtIntakeBatchItemIssue(
                    issue,
                    itemPath + issue.Path));
            }
            itemIssues.Sort(CompareItemIssues);
            issues = new ReadOnlyCollection<CharacterArtIntakeBatchItemIssue>(itemIssues);
        }

        public string ModuleId { get; }
        public string SourceId => Manifest.SourceId;
        public string CharacterId => Manifest.CharacterId;
        public CharacterArtIntakeManifest Manifest { get; }
        public CharacterArtMeasurementSnapshot Measurement { get; }
        public bool IsCompliant => issues.Count == 0;
        public IReadOnlyList<CharacterArtIntakeBatchItemIssue> Issues => issues;

        private static int CompareItemIssues(
            CharacterArtIntakeBatchItemIssue left,
            CharacterArtIntakeBatchItemIssue right)
        {
            var path = StringComparer.Ordinal.Compare(left.Path, right.Path);
            if (path != 0)
                return path;
            var code = left.Code.CompareTo(right.Code);
            if (code != 0)
                return code;
            return Nullable.Compare(left.ManifestIssueCode, right.ManifestIssueCode);
        }
    }

    /// <summary>
    /// Immutable SourceId-sorted report built only from explicitly supplied providers.
    /// </summary>
    public sealed class CharacterArtIntakeBatchReport
    {
        private readonly ReadOnlyCollection<CharacterArtIntakeBatchReportItem> items;
        private readonly ReadOnlyDictionary<string, CharacterArtIntakeBatchReportItem> itemsBySourceId;
        private readonly ReadOnlyDictionary<string, CharacterArtIntakeBatchReportItem> itemsByCharacterId;

        private CharacterArtIntakeBatchReport(IList<CharacterArtIntakeBatchReportItem> items)
        {
            var itemCopy = new List<CharacterArtIntakeBatchReportItem>(items);
            this.items = new ReadOnlyCollection<CharacterArtIntakeBatchReportItem>(itemCopy);

            var sourceIds = new Dictionary<string, CharacterArtIntakeBatchReportItem>(
                StringComparer.Ordinal);
            var characterIds = new Dictionary<string, CharacterArtIntakeBatchReportItem>(
                StringComparer.Ordinal);
            var compliantCount = 0;
            for (var index = 0; index < itemCopy.Count; index++)
            {
                var item = itemCopy[index];
                sourceIds.Add(item.SourceId, item);
                characterIds.Add(item.CharacterId, item);
                if (item.IsCompliant)
                    compliantCount++;
            }

            itemsBySourceId = new ReadOnlyDictionary<string, CharacterArtIntakeBatchReportItem>(sourceIds);
            itemsByCharacterId = new ReadOnlyDictionary<string, CharacterArtIntakeBatchReportItem>(characterIds);
            CompliantCount = compliantCount;
        }

        public IReadOnlyList<CharacterArtIntakeBatchReportItem> Items => items;
        public int TotalCount => items.Count;
        public int CompliantCount { get; }
        public int NoncompliantCount => items.Count - CompliantCount;

        public bool TryGetBySourceId(
            string sourceId,
            out CharacterArtIntakeBatchReportItem item)
        {
            if (sourceId == null)
            {
                item = null;
                return false;
            }
            return itemsBySourceId.TryGetValue(sourceId, out item);
        }

        public bool TryGetByCharacterId(
            string characterId,
            out CharacterArtIntakeBatchReportItem item)
        {
            if (characterId == null)
            {
                item = null;
                return false;
            }
            return itemsByCharacterId.TryGetValue(characterId, out item);
        }

        public static CharacterArtIntakeBatchBuildResult Build(
            IEnumerable<ICharacterArtIntakeBatchProvider> providers)
        {
            try
            {
                return BuildCore(providers);
            }
            catch (Exception)
            {
                return Failure(
                    CharacterArtIntakeBatchBuildIssueCode.UnreadableInput,
                    "batch");
            }
        }

        private static CharacterArtIntakeBatchBuildResult BuildCore(
            IEnumerable<ICharacterArtIntakeBatchProvider> providers)
        {
            var issues = new List<CharacterArtIntakeBatchBuildIssue>();
            var providerSnapshots = SnapshotProviders(providers, issues);
            AddDuplicateProviderIssues(providerSnapshots, issues);
            var itemSnapshots = SnapshotItems(providerSnapshots, issues);
            AddDuplicateItemIssues(itemSnapshots, issues);
            SortBuildIssues(issues);

            if (issues.Count != 0)
                return new CharacterArtIntakeBatchBuildResult(null, issues);

            var reportItems = new List<CharacterArtIntakeBatchReportItem>(itemSnapshots.Count);
            for (var index = 0; index < itemSnapshots.Count; index++)
            {
                var snapshot = itemSnapshots[index];
                var result = CharacterArtIntakeComplianceEvaluator.Evaluate(
                    snapshot.Item.Manifest,
                    snapshot.Item.Measurement);
                reportItems.Add(new CharacterArtIntakeBatchReportItem(
                    snapshot.ModuleId,
                    snapshot.Item,
                    result));
            }
            reportItems.Sort((left, right) =>
                StringComparer.Ordinal.Compare(left.SourceId, right.SourceId));
            return new CharacterArtIntakeBatchBuildResult(
                new CharacterArtIntakeBatchReport(reportItems),
                issues);
        }

        private static List<ProviderSnapshot> SnapshotProviders(
            IEnumerable<ICharacterArtIntakeBatchProvider> providers,
            ICollection<CharacterArtIntakeBatchBuildIssue> issues)
        {
            var source = new List<ICharacterArtIntakeBatchProvider>();
            if (providers == null)
            {
                issues.Add(new CharacterArtIntakeBatchBuildIssue(
                    CharacterArtIntakeBatchBuildIssueCode.NullProviderCollection,
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
                issues.Add(new CharacterArtIntakeBatchBuildIssue(
                    CharacterArtIntakeBatchBuildIssueCode.UnreadableProviderCollection,
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
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.NullProvider,
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
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.UnreadableProvider,
                        indexedPath + ".moduleId"));
                    continue;
                }

                var providerPath = IsStableId(moduleId)
                    ? "providers[" + moduleId + "]"
                    : indexedPath;
                if (!IsStableId(moduleId))
                {
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.InvalidModuleId,
                        providerPath + ".moduleId"));
                }

                IReadOnlyList<CharacterArtIntakeBatchItem> items;
                try
                {
                    items = provider.Items;
                }
                catch (Exception)
                {
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.UnreadableProvider,
                        providerPath + ".items"));
                    continue;
                }
                snapshots.Add(new ProviderSnapshot(moduleId, items, providerPath));
            }
            snapshots.Sort(CompareProviders);
            return snapshots;
        }

        private static void AddDuplicateProviderIssues(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<CharacterArtIntakeBatchBuildIssue> issues)
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
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.DuplicateProviderModuleId,
                        "providers[" + pair.Key + "].moduleId"));
                }
            }
        }

        private static List<ItemSnapshot> SnapshotItems(
            IReadOnlyList<ProviderSnapshot> providers,
            ICollection<CharacterArtIntakeBatchBuildIssue> issues)
        {
            var snapshots = new List<ItemSnapshot>();
            for (var providerIndex = 0; providerIndex < providers.Count; providerIndex++)
            {
                var provider = providers[providerIndex];
                if (provider.Items == null)
                {
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.NullItemCollection,
                        provider.Path + ".items"));
                    continue;
                }

                int count;
                try
                {
                    count = provider.Items.Count;
                }
                catch (Exception)
                {
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.UnreadableItemCollection,
                        provider.Path + ".items"));
                    continue;
                }
                if (count < 0)
                {
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.UnreadableItemCollection,
                        provider.Path + ".items"));
                    continue;
                }

                for (var itemIndex = 0; itemIndex < count; itemIndex++)
                {
                    var indexedPath = provider.Path + ".items[" + itemIndex + "]";
                    CharacterArtIntakeBatchItem item;
                    try
                    {
                        item = provider.Items[itemIndex];
                    }
                    catch (Exception)
                    {
                        issues.Add(new CharacterArtIntakeBatchBuildIssue(
                            CharacterArtIntakeBatchBuildIssueCode.UnreadableItem,
                            indexedPath));
                        continue;
                    }
                    if (item == null)
                    {
                        issues.Add(new CharacterArtIntakeBatchBuildIssue(
                            CharacterArtIntakeBatchBuildIssueCode.NullItem,
                            indexedPath));
                        continue;
                    }
                    if (item.Manifest == null)
                    {
                        issues.Add(new CharacterArtIntakeBatchBuildIssue(
                            CharacterArtIntakeBatchBuildIssueCode.MissingItemManifest,
                            indexedPath + ".manifest"));
                        continue;
                    }

                    var sourceId = item.Manifest.SourceId;
                    var characterId = item.Manifest.CharacterId;
                    var itemPath = IsStableId(sourceId)
                        ? provider.Path + ".items[" + sourceId + "]"
                        : indexedPath;
                    if (!IsStableId(sourceId))
                    {
                        issues.Add(new CharacterArtIntakeBatchBuildIssue(
                            CharacterArtIntakeBatchBuildIssueCode.InvalidItemSourceId,
                            itemPath + ".manifest.sourceId"));
                    }
                    if (!IsStableId(characterId))
                    {
                        issues.Add(new CharacterArtIntakeBatchBuildIssue(
                            CharacterArtIntakeBatchBuildIssueCode.InvalidItemCharacterId,
                            itemPath + ".manifest.characterId"));
                    }
                    if (IsStableId(sourceId) && IsStableId(characterId))
                    {
                        snapshots.Add(new ItemSnapshot(
                            provider.ModuleId,
                            item,
                            itemPath));
                    }
                }
            }
            snapshots.Sort(CompareItems);
            return snapshots;
        }

        private static void AddDuplicateItemIssues(
            IReadOnlyList<ItemSnapshot> items,
            ICollection<CharacterArtIntakeBatchBuildIssue> issues)
        {
            var sourceIds = new Dictionary<string, int>(StringComparer.Ordinal);
            var characterIds = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < items.Count; index++)
            {
                AddCount(sourceIds, items[index].Item.Manifest.SourceId);
                AddCount(characterIds, items[index].Item.Manifest.CharacterId);
            }

            foreach (var pair in sourceIds)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.DuplicateSourceId,
                        "items[" + pair.Key + "].manifest.sourceId"));
                }
            }
            foreach (var pair in characterIds)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterArtIntakeBatchBuildIssue(
                        CharacterArtIntakeBatchBuildIssueCode.DuplicateCharacterId,
                        "characters[" + pair.Key + "].manifest.characterId"));
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

        private static int CompareItems(ItemSnapshot left, ItemSnapshot right)
        {
            var sourceId = StringComparer.Ordinal.Compare(
                left.Item.Manifest.SourceId,
                right.Item.Manifest.SourceId);
            if (sourceId != 0)
                return sourceId;
            var characterId = StringComparer.Ordinal.Compare(
                left.Item.Manifest.CharacterId,
                right.Item.Manifest.CharacterId);
            if (characterId != 0)
                return characterId;
            return StringComparer.Ordinal.Compare(left.ModuleId, right.ModuleId);
        }

        private static void SortBuildIssues(List<CharacterArtIntakeBatchBuildIssue> issues)
        {
            issues.Sort((left, right) =>
            {
                var path = StringComparer.Ordinal.Compare(left.Path, right.Path);
                return path != 0 ? path : left.Code.CompareTo(right.Code);
            });
        }

        private static CharacterArtIntakeBatchBuildResult Failure(
            CharacterArtIntakeBatchBuildIssueCode code,
            string path)
        {
            return new CharacterArtIntakeBatchBuildResult(
                null,
                new List<CharacterArtIntakeBatchBuildIssue>
                {
                    new CharacterArtIntakeBatchBuildIssue(code, path)
                });
        }

        private sealed class ProviderSnapshot
        {
            public ProviderSnapshot(
                string moduleId,
                IReadOnlyList<CharacterArtIntakeBatchItem> items,
                string path)
            {
                ModuleId = moduleId;
                Items = items;
                Path = path;
            }

            public string ModuleId { get; }
            public IReadOnlyList<CharacterArtIntakeBatchItem> Items { get; }
            public string Path { get; }
        }

        private sealed class ItemSnapshot
        {
            public ItemSnapshot(
                string moduleId,
                CharacterArtIntakeBatchItem item,
                string path)
            {
                ModuleId = moduleId;
                Item = item;
                Path = path;
            }

            public string ModuleId { get; }
            public CharacterArtIntakeBatchItem Item { get; }
            public string Path { get; }
        }
    }
}
