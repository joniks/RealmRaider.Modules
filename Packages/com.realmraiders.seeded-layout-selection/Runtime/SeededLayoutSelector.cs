using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules.SeededLayoutSelection
{
    /// <summary>
    /// Pure platform-stable selection. The realm hash rotates caller order; the
    /// signed seed maps directly to one ordinal without system RNG or rerolls.
    /// </summary>
    public static class SeededLayoutSelector
    {
        public static SeededLayoutSelectionResult SelectExact(
            string realmId,
            int seed,
            IReadOnlyList<string> canonicalLayoutIds)
        {
            return SelectInternal(
                realmId,
                seed,
                canonicalLayoutIds,
                null,
                false);
        }

        public static SeededLayoutSelectionResult SelectExactAvoidingPrevious(
            string realmId,
            int seed,
            IReadOnlyList<string> canonicalLayoutIds,
            string previousLayoutId)
        {
            return SelectInternal(
                realmId,
                seed,
                canonicalLayoutIds,
                previousLayoutId,
                true);
        }

        private static SeededLayoutSelectionResult SelectInternal(
            string realmId,
            int seed,
            IReadOnlyList<string> canonicalLayoutIds,
            string previousLayoutId,
            bool avoidPrevious)
        {
            var evidence = new List<SeededLayoutSelectionIssueEvidence>();
            ValidateRealmId(realmId, evidence);
            ValidateCatalogue(canonicalLayoutIds, evidence);
            if (avoidPrevious)
            {
                ValidatePreviousLayoutId(
                    previousLayoutId,
                    canonicalLayoutIds,
                    evidence);
            }

            if (evidence.Count > 0)
            {
                return new SeededLayoutSelectionResult(
                    SeededLayoutSelectionStatus.Rejected,
                    null,
                    evidence);
            }

            var selectionOrdinal = CreateSelectionOrdinal(realmId, seed);
            var catalogueIndex = 0;
            var previousAvoided = false;
            var singleLayoutFallback = false;
            if (!avoidPrevious)
            {
                catalogueIndex = (int)(selectionOrdinal % canonicalLayoutIds.Count);
            }
            else if (canonicalLayoutIds.Count == 1)
            {
                catalogueIndex = 0;
                singleLayoutFallback = true;
            }
            else
            {
                var alternativeOrdinal = (int)(
                    selectionOrdinal % (canonicalLayoutIds.Count - 1));
                catalogueIndex = FindAlternativeIndex(
                    canonicalLayoutIds,
                    previousLayoutId,
                    alternativeOrdinal);
                previousAvoided = true;
            }

            return new SeededLayoutSelectionResult(
                SeededLayoutSelectionStatus.Selected,
                new SeededLayoutSelection(
                    realmId,
                    seed,
                    canonicalLayoutIds[catalogueIndex],
                    catalogueIndex,
                    canonicalLayoutIds,
                    avoidPrevious ? previousLayoutId : null,
                    previousAvoided,
                    singleLayoutFallback),
                Array.AsReadOnly(Array.Empty<SeededLayoutSelectionIssueEvidence>()));
        }

        private static void ValidateRealmId(
            string realmId,
            ICollection<SeededLayoutSelectionIssueEvidence> evidence)
        {
            var classification = ClassifyId(realmId);
            if (classification == IdClassification.Missing)
            {
                AddEvidence(evidence, SeededLayoutSelectionIssue.RealmIdMissing, -1, realmId);
            }
            else if (classification == IdClassification.Malformed)
            {
                AddEvidence(
                    evidence,
                    SeededLayoutSelectionIssue.RealmIdMalformed,
                    -1,
                    realmId);
            }
            else if (classification == IdClassification.NonCanonical)
            {
                AddEvidence(
                    evidence,
                    SeededLayoutSelectionIssue.RealmIdNonCanonical,
                    -1,
                    realmId);
            }
        }

        private static void ValidateCatalogue(
            IReadOnlyList<string> canonicalLayoutIds,
            ICollection<SeededLayoutSelectionIssueEvidence> evidence)
        {
            if (canonicalLayoutIds == null)
            {
                AddEvidence(
                    evidence,
                    SeededLayoutSelectionIssue.CatalogueMissing,
                    -1,
                    null);
                return;
            }

            if (canonicalLayoutIds.Count == 0)
            {
                AddEvidence(
                    evidence,
                    SeededLayoutSelectionIssue.CatalogueEmpty,
                    -1,
                    null);
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < canonicalLayoutIds.Count; index++)
            {
                var layoutId = canonicalLayoutIds[index];
                var classification = ClassifyId(layoutId);
                if (classification == IdClassification.Missing)
                {
                    AddEvidence(
                        evidence,
                        SeededLayoutSelectionIssue.LayoutIdMissing,
                        index,
                        layoutId);
                }
                else if (classification == IdClassification.Malformed)
                {
                    AddEvidence(
                        evidence,
                        SeededLayoutSelectionIssue.LayoutIdMalformed,
                        index,
                        layoutId);
                }
                else if (classification == IdClassification.NonCanonical)
                {
                    AddEvidence(
                        evidence,
                        SeededLayoutSelectionIssue.LayoutIdNonCanonical,
                        index,
                        layoutId);
                }
                else if (!seen.Add(layoutId))
                {
                    AddEvidence(
                        evidence,
                        SeededLayoutSelectionIssue.LayoutIdDuplicate,
                        index,
                        layoutId);
                }
            }
        }

        private static void ValidatePreviousLayoutId(
            string previousLayoutId,
            IReadOnlyList<string> canonicalLayoutIds,
            ICollection<SeededLayoutSelectionIssueEvidence> evidence)
        {
            var classification = ClassifyId(previousLayoutId);
            if (classification == IdClassification.Missing)
            {
                AddEvidence(
                    evidence,
                    SeededLayoutSelectionIssue.PreviousLayoutIdMissing,
                    -1,
                    previousLayoutId);
                return;
            }

            if (classification == IdClassification.Malformed)
            {
                AddEvidence(
                    evidence,
                    SeededLayoutSelectionIssue.PreviousLayoutIdMalformed,
                    -1,
                    previousLayoutId);
                return;
            }

            if (classification == IdClassification.NonCanonical)
            {
                AddEvidence(
                    evidence,
                    SeededLayoutSelectionIssue.PreviousLayoutIdNonCanonical,
                    -1,
                    previousLayoutId);
                return;
            }

            if (canonicalLayoutIds == null || !Contains(canonicalLayoutIds, previousLayoutId))
            {
                AddEvidence(
                    evidence,
                    SeededLayoutSelectionIssue.PreviousLayoutIdUnknown,
                    -1,
                    previousLayoutId);
            }
        }

        private static long CreateSelectionOrdinal(string realmId, int seed)
        {
            var signedSeedOrdinal = (long)seed - int.MinValue;
            return signedSeedOrdinal + StableRealmHash(realmId);
        }

        private static uint StableRealmHash(string realmId)
        {
            const uint offsetBasis = 2166136261u;
            const uint prime = 16777619u;
            var hash = offsetBasis;
            unchecked
            {
                foreach (var symbol in realmId)
                {
                    hash ^= (byte)symbol;
                    hash *= prime;
                }
            }

            return hash;
        }

        private static int FindAlternativeIndex(
            IReadOnlyList<string> catalogue,
            string previousLayoutId,
            int alternativeOrdinal)
        {
            var currentAlternative = 0;
            for (var index = 0; index < catalogue.Count; index++)
            {
                if (string.Equals(catalogue[index], previousLayoutId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (currentAlternative == alternativeOrdinal)
                {
                    return index;
                }

                currentAlternative++;
            }

            throw new InvalidOperationException("Validated alternative ordinal was not found.");
        }

        private static IdClassification ClassifyId(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return IdClassification.Missing;
            }

            if (IsCanonicalId(value))
            {
                return IdClassification.Valid;
            }

            var normalized = NormalizeAsciiCandidate(value);
            return IsCanonicalId(normalized)
                ? IdClassification.NonCanonical
                : IdClassification.Malformed;
        }

        private static bool IsCanonicalId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || value.Length < CanonicalRealmLayoutIdBounds.MinimumCharacters
                || value.Length > CanonicalRealmLayoutIdBounds.MaximumCharacters
                || !IsAsciiLowerAlphaNumeric(value[0])
                || !IsAsciiLowerAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            var containsNamespaceSeparator = false;
            var previousWasSeparator = false;
            for (var index = 1; index < value.Length; index++)
            {
                var symbol = value[index];
                if (IsAsciiLowerAlphaNumeric(symbol))
                {
                    previousWasSeparator = false;
                    continue;
                }

                if (symbol != '.' && symbol != '-' || previousWasSeparator)
                {
                    return false;
                }

                containsNamespaceSeparator = containsNamespaceSeparator || symbol == '.';
                previousWasSeparator = true;
            }

            return containsNamespaceSeparator && !previousWasSeparator;
        }

        private static string NormalizeAsciiCandidate(string value)
        {
            var start = 0;
            while (start < value.Length && value[start] == ' ')
            {
                start++;
            }

            var end = value.Length;
            while (end > start && value[end - 1] == ' ')
            {
                end--;
            }

            var normalized = new char[end - start];
            for (var index = start; index < end; index++)
            {
                var symbol = value[index];
                normalized[index - start] = symbol >= 'A' && symbol <= 'Z'
                    ? (char)(symbol + ('a' - 'A'))
                    : symbol;
            }

            return new string(normalized);
        }

        private static bool Contains(IReadOnlyList<string> values, string expected)
        {
            foreach (var value in values)
            {
                if (string.Equals(value, expected, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAsciiLowerAlphaNumeric(char value)
        {
            return value >= 'a' && value <= 'z'
                || value >= '0' && value <= '9';
        }

        private static void AddEvidence(
            ICollection<SeededLayoutSelectionIssueEvidence> evidence,
            SeededLayoutSelectionIssue issue,
            int catalogueIndex,
            string suppliedValue)
        {
            evidence.Add(new SeededLayoutSelectionIssueEvidence(
                issue,
                catalogueIndex,
                suppliedValue));
        }

        private enum IdClassification
        {
            Valid,
            Missing,
            Malformed,
            NonCanonical
        }
    }
}
