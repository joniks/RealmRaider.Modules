using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RealmRaiders.Modules.StarterRealmIdentityRecord
{
    public static class StarterRealmIdentityCodec
    {
        private const string VersionField = "version";
        private const string RealmIdField = "realmId";
        private const string SeedField = "seed";
        private const string LayoutIdField = "layoutId";

        private static readonly string[] CanonicalFieldOrder =
        {
            VersionField,
            RealmIdField,
            SeedField,
            LayoutIdField
        };

        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public static StarterRealmIdentityResult Create(
            int version,
            string realmId,
            int seed,
            string layoutId)
        {
            var evidence = new List<StarterRealmIdentityIssueEvidence>();
            ValidateVersion(version, VersionField, -1, evidence);
            ValidateId(realmId, true, RealmIdField, -1, evidence);
            ValidateId(layoutId, false, LayoutIdField, -1, evidence);
            return Complete(version, realmId, seed, layoutId, evidence);
        }

        public static string SerializeCanonicalText(StarterRealmIdentityRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return VersionField + "="
                + record.Version.ToString(CultureInfo.InvariantCulture)
                + "\n" + RealmIdField + "=" + record.RealmId
                + "\n" + SeedField + "="
                + record.Seed.ToString(CultureInfo.InvariantCulture)
                + "\n" + LayoutIdField + "=" + record.LayoutId;
        }

        public static byte[] SerializeCanonicalUtf8(StarterRealmIdentityRecord record)
        {
            return StrictUtf8.GetBytes(SerializeCanonicalText(record));
        }

        public static StarterRealmIdentityResult ParseCanonicalUtf8(byte[] bytes)
        {
            if (bytes == null)
            {
                return Reject(
                    StarterRealmIdentityIssue.InputMissing,
                    -1,
                    null,
                    null);
            }

            try
            {
                return ParseCanonicalText(StrictUtf8.GetString(bytes));
            }
            catch (DecoderFallbackException)
            {
                return Reject(
                    StarterRealmIdentityIssue.EncodingInvalid,
                    -1,
                    null,
                    null);
            }
        }

        public static StarterRealmIdentityResult ParseCanonicalText(string text)
        {
            if (text == null)
            {
                return Reject(
                    StarterRealmIdentityIssue.InputMissing,
                    -1,
                    null,
                    null);
            }

            if (text.Length == 0)
            {
                return Reject(
                    StarterRealmIdentityIssue.InputEmpty,
                    -1,
                    null,
                    text);
            }

            var evidence = new List<StarterRealmIdentityIssueEvidence>();
            var lines = text.Split(new[] { '\n' }, StringSplitOptions.None);
            if (lines[lines.Length - 1].Length == 0)
            {
                AddEvidence(
                    evidence,
                    StarterRealmIdentityIssue.TrailingData,
                    lines.Length - 1,
                    null,
                    string.Empty);
            }

            var fields = new Dictionary<string, ParsedField>(StringComparer.Ordinal);
            var parsedNames = new List<string>();
            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];
                if (line.Length == 0)
                {
                    continue;
                }

                var separator = line.IndexOf('=');
                if (separator <= 0
                    || separator != line.LastIndexOf('='))
                {
                    AddEvidence(
                        evidence,
                        StarterRealmIdentityIssue.FieldSyntaxInvalid,
                        index,
                        null,
                        line);
                    continue;
                }

                var name = line.Substring(0, separator);
                var value = line.Substring(separator + 1);
                parsedNames.Add(name);
                if (!IsKnownField(name))
                {
                    AddEvidence(
                        evidence,
                        StarterRealmIdentityIssue.FieldUnknown,
                        index,
                        name,
                        value);
                }
                else if (fields.ContainsKey(name))
                {
                    AddEvidence(
                        evidence,
                        StarterRealmIdentityIssue.FieldDuplicate,
                        index,
                        name,
                        value);
                }
                else
                {
                    fields.Add(name, new ParsedField(index, value));
                }

                if (index >= CanonicalFieldOrder.Length)
                {
                    AddEvidence(
                        evidence,
                        StarterRealmIdentityIssue.FieldExtra,
                        index,
                        name,
                        value);
                }
            }

            foreach (var fieldName in CanonicalFieldOrder)
            {
                if (!fields.ContainsKey(fieldName))
                {
                    AddEvidence(
                        evidence,
                        StarterRealmIdentityIssue.FieldMissing,
                        -1,
                        fieldName,
                        null);
                }
            }

            if (fields.Count == CanonicalFieldOrder.Length
                && parsedNames.Count == CanonicalFieldOrder.Length
                && !HasCanonicalOrder(parsedNames))
            {
                AddEvidence(
                    evidence,
                    StarterRealmIdentityIssue.FieldOrderInvalid,
                    FirstOrderMismatch(parsedNames),
                    null,
                    null);
            }

            var version = 0;
            var seed = 0;
            var realmId = ValueOf(fields, RealmIdField);
            var layoutId = ValueOf(fields, LayoutIdField);
            if (fields.TryGetValue(VersionField, out var versionField))
            {
                if (!TryParseCanonicalUnsigned(versionField.Value, out version))
                {
                    AddEvidence(
                        evidence,
                        StarterRealmIdentityIssue.VersionMalformed,
                        versionField.Index,
                        VersionField,
                        versionField.Value);
                }
                else
                {
                    ValidateVersion(version, VersionField, versionField.Index, evidence);
                }
            }

            if (fields.TryGetValue(RealmIdField, out var realmField))
            {
                ValidateId(realmField.Value, true, RealmIdField, realmField.Index, evidence);
            }

            if (fields.TryGetValue(SeedField, out var seedField))
            {
                if (!HasCanonicalSignedIntegerSyntax(seedField.Value))
                {
                    AddEvidence(
                        evidence,
                        StarterRealmIdentityIssue.SeedMalformed,
                        seedField.Index,
                        SeedField,
                        seedField.Value);
                }
                else if (!int.TryParse(
                             seedField.Value,
                             NumberStyles.AllowLeadingSign,
                             CultureInfo.InvariantCulture,
                             out seed))
                {
                    AddEvidence(
                        evidence,
                        StarterRealmIdentityIssue.SeedOverflow,
                        seedField.Index,
                        SeedField,
                        seedField.Value);
                }
            }

            if (fields.TryGetValue(LayoutIdField, out var layoutField))
            {
                ValidateId(layoutField.Value, false, LayoutIdField, layoutField.Index, evidence);
            }

            return Complete(version, realmId, seed, layoutId, evidence);
        }

        private static StarterRealmIdentityResult Complete(
            int version,
            string realmId,
            int seed,
            string layoutId,
            IReadOnlyList<StarterRealmIdentityIssueEvidence> evidence)
        {
            return evidence.Count == 0
                ? new StarterRealmIdentityResult(
                    new StarterRealmIdentityRecord(version, realmId, seed, layoutId),
                    evidence)
                : new StarterRealmIdentityResult(null, evidence);
        }

        private static StarterRealmIdentityResult Reject(
            StarterRealmIdentityIssue issue,
            int fieldIndex,
            string fieldName,
            string suppliedValue)
        {
            return new StarterRealmIdentityResult(
                null,
                Array.AsReadOnly(new[]
                {
                    new StarterRealmIdentityIssueEvidence(
                        issue,
                        fieldIndex,
                        fieldName,
                        suppliedValue)
                }));
        }

        private static void ValidateVersion(
            int version,
            string fieldName,
            int fieldIndex,
            ICollection<StarterRealmIdentityIssueEvidence> evidence)
        {
            if (version != StarterRealmIdentityRecordContract.CurrentVersion)
            {
                AddEvidence(
                    evidence,
                    StarterRealmIdentityIssue.VersionUnsupported,
                    fieldIndex,
                    fieldName,
                    version.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void ValidateId(
            string value,
            bool isRealmId,
            string fieldName,
            int fieldIndex,
            ICollection<StarterRealmIdentityIssueEvidence> evidence)
        {
            var classification = ClassifyId(value, isRealmId);
            if (classification == IdClassification.Valid)
            {
                return;
            }

            var issue = classification == IdClassification.Missing
                ? isRealmId
                    ? StarterRealmIdentityIssue.RealmIdMissing
                    : StarterRealmIdentityIssue.LayoutIdMissing
                : classification == IdClassification.NonCanonical
                    ? isRealmId
                        ? StarterRealmIdentityIssue.RealmIdNonCanonical
                        : StarterRealmIdentityIssue.LayoutIdNonCanonical
                    : isRealmId
                        ? StarterRealmIdentityIssue.RealmIdMalformed
                        : StarterRealmIdentityIssue.LayoutIdMalformed;
            AddEvidence(evidence, issue, fieldIndex, fieldName, value);
        }

        private static IdClassification ClassifyId(string value, bool isRealmId)
        {
            if (string.IsNullOrEmpty(value))
            {
                return IdClassification.Missing;
            }

            if (IsCanonicalId(value, isRealmId))
            {
                return IdClassification.Valid;
            }

            return IsCanonicalId(NormalizeAsciiCandidate(value), isRealmId)
                ? IdClassification.NonCanonical
                : IdClassification.Malformed;
        }

        private static bool IsCanonicalId(string value, bool isRealmId)
        {
            if (isRealmId && IsLowerHexRealmInstanceId(value))
            {
                return true;
            }

            if (string.IsNullOrEmpty(value)
                || value.Length < StarterRealmIdentityRecordContract.MinimumIdCharacters
                || value.Length > StarterRealmIdentityRecordContract.MaximumIdCharacters
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

                if ((symbol != '.' && symbol != '-') || previousWasSeparator)
                {
                    return false;
                }

                containsNamespaceSeparator = containsNamespaceSeparator || symbol == '.';
                previousWasSeparator = true;
            }

            return containsNamespaceSeparator && !previousWasSeparator;
        }

        private static bool IsLowerHexRealmInstanceId(string value)
        {
            if (value == null || value.Length != 32)
            {
                return false;
            }

            for (var index = 0; index < value.Length; index++)
            {
                var symbol = value[index];
                if (!((symbol >= '0' && symbol <= '9')
                    || (symbol >= 'a' && symbol <= 'f')))
                {
                    return false;
                }
            }

            return true;
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

        private static bool HasCanonicalSignedIntegerSyntax(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value == "0")
            {
                return true;
            }

            var index = value[0] == '-' ? 1 : 0;
            if (index == value.Length || value[index] < '1' || value[index] > '9')
            {
                return false;
            }

            for (index++; index < value.Length; index++)
            {
                if (value[index] < '0' || value[index] > '9')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseCanonicalUnsigned(string value, out int parsed)
        {
            parsed = 0;
            if (string.IsNullOrEmpty(value)
                || value.Length > 1 && value[0] == '0')
            {
                return false;
            }

            for (var index = 0; index < value.Length; index++)
            {
                if (value[index] < '0' || value[index] > '9')
                {
                    return false;
                }
            }

            return int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out parsed);
        }

        private static string ValueOf(
            IReadOnlyDictionary<string, ParsedField> fields,
            string fieldName)
        {
            return fields.TryGetValue(fieldName, out var field) ? field.Value : null;
        }

        private static bool IsKnownField(string name)
        {
            foreach (var expected in CanonicalFieldOrder)
            {
                if (string.Equals(name, expected, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCanonicalOrder(IReadOnlyList<string> names)
        {
            return FirstOrderMismatch(names) < 0;
        }

        private static int FirstOrderMismatch(IReadOnlyList<string> names)
        {
            for (var index = 0; index < CanonicalFieldOrder.Length; index++)
            {
                if (!string.Equals(names[index], CanonicalFieldOrder[index],
                        StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool IsAsciiLowerAlphaNumeric(char value)
        {
            return value >= 'a' && value <= 'z'
                || value >= '0' && value <= '9';
        }

        private static void AddEvidence(
            ICollection<StarterRealmIdentityIssueEvidence> evidence,
            StarterRealmIdentityIssue issue,
            int fieldIndex,
            string fieldName,
            string suppliedValue)
        {
            evidence.Add(new StarterRealmIdentityIssueEvidence(
                issue,
                fieldIndex,
                fieldName,
                suppliedValue));
        }

        private sealed class ParsedField
        {
            public ParsedField(int index, string value)
            {
                Index = index;
                Value = value;
            }

            public int Index { get; }

            public string Value { get; }
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
