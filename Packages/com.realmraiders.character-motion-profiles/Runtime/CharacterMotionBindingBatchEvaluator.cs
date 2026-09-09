using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.CharacterMotionProfiles
{
    /// <summary>An explicit, adapter-neutral request to validate one character motion binding.</summary>
    public sealed class CharacterMotionBindingRequest
    {
        public CharacterMotionBindingRequest(
            string characterId,
            string motionProfileId,
            CharacterMotionTargetRequirements targetRequirements)
        {
            CharacterId = characterId;
            MotionProfileId = motionProfileId;
            TargetRequirements = targetRequirements;
        }

        public string CharacterId { get; }
        public string MotionProfileId { get; }
        public CharacterMotionTargetRequirements TargetRequirements { get; }
    }

    public enum CharacterMotionBindingBatchIssueCode
    {
        NullCatalogue,
        NullRequestCollection,
        UnreadableRequestCollection,
        NullRequest,
        InvalidCharacterId,
        InvalidMotionProfileId,
        DuplicateCharacterBinding,
        MissingProfile,
        IncompatibleProfile
    }

    public sealed class CharacterMotionBindingBatchIssue
    {
        public CharacterMotionBindingBatchIssue(
            CharacterMotionBindingBatchIssueCode code,
            string path,
            CharacterMotionCompatibilityIssueCode? compatibilityIssueCode = null,
            MotionProfileIssueCode? profileIssueCode = null)
        {
            Code = code;
            Path = path;
            CompatibilityIssueCode = compatibilityIssueCode;
            ProfileIssueCode = profileIssueCode;
        }

        public CharacterMotionBindingBatchIssueCode Code { get; }
        public string Path { get; }
        public CharacterMotionCompatibilityIssueCode? CompatibilityIssueCode { get; }
        public MotionProfileIssueCode? ProfileIssueCode { get; }
    }

    public sealed class CharacterMotionBindingRecord
    {
        internal CharacterMotionBindingRecord(
            string characterId,
            string motionProfileId,
            CharacterMotionProfile profile)
        {
            CharacterId = characterId;
            MotionProfileId = motionProfileId;
            Profile = profile;
        }

        public string CharacterId { get; }
        public string MotionProfileId { get; }
        public CharacterMotionProfile Profile { get; }
    }

    public sealed class CharacterMotionBindingBatchResult
    {
        private readonly ReadOnlyCollection<CharacterMotionBindingRecord> records;
        private readonly ReadOnlyCollection<CharacterMotionBindingBatchIssue> issues;

        internal CharacterMotionBindingBatchResult(
            IList<CharacterMotionBindingRecord> records,
            IList<CharacterMotionBindingBatchIssue> issues)
        {
            this.records = new ReadOnlyCollection<CharacterMotionBindingRecord>(
                new List<CharacterMotionBindingRecord>(records));
            this.issues = new ReadOnlyCollection<CharacterMotionBindingBatchIssue>(
                new List<CharacterMotionBindingBatchIssue>(issues));
        }

        public bool Succeeded => issues.Count == 0;
        public IReadOnlyList<CharacterMotionBindingRecord> Records => records;
        public IReadOnlyList<CharacterMotionBindingBatchIssue> Issues => issues;
    }

    /// <summary>
    /// Evaluates only explicit binding requests against an explicit profile catalogue.
    /// It neither discovers profiles nor resolves assets, fallbacks, animation, or gameplay authority.
    /// </summary>
    public static class CharacterMotionBindingBatchEvaluator
    {
        public static CharacterMotionBindingBatchResult Evaluate(
            CharacterMotionProfileCatalogue catalogue,
            IEnumerable<CharacterMotionBindingRequest> requests)
        {
            var issues = new List<CharacterMotionBindingBatchIssue>();
            var requestSnapshots = SnapshotRequests(requests, issues);
            if (catalogue == null)
                issues.Add(new CharacterMotionBindingBatchIssue(
                    CharacterMotionBindingBatchIssueCode.NullCatalogue,
                    "catalogue"));

            AddDuplicateCharacterIssues(requestSnapshots, issues);
            var records = catalogue == null
                ? new List<CharacterMotionBindingRecord>()
                : EvaluateRequests(catalogue, requestSnapshots, issues);

            SortIssues(issues);
            if (issues.Count != 0)
                records.Clear();
            else
                records.Sort((left, right) => string.CompareOrdinal(left.CharacterId, right.CharacterId));

            return new CharacterMotionBindingBatchResult(records, issues);
        }

        private static List<CharacterMotionBindingRequest> SnapshotRequests(
            IEnumerable<CharacterMotionBindingRequest> requests,
            ICollection<CharacterMotionBindingBatchIssue> issues)
        {
            var snapshots = new List<CharacterMotionBindingRequest>();
            if (requests == null)
            {
                issues.Add(new CharacterMotionBindingBatchIssue(
                    CharacterMotionBindingBatchIssueCode.NullRequestCollection,
                    "bindings"));
                return snapshots;
            }

            try
            {
                foreach (var request in requests)
                    snapshots.Add(request);
            }
            catch (Exception)
            {
                snapshots.Clear();
                issues.Add(new CharacterMotionBindingBatchIssue(
                    CharacterMotionBindingBatchIssueCode.UnreadableRequestCollection,
                    "bindings"));
            }
            return snapshots;
        }

        private static void AddDuplicateCharacterIssues(
            IReadOnlyList<CharacterMotionBindingRequest> requests,
            ICollection<CharacterMotionBindingBatchIssue> issues)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < requests.Count; index++)
            {
                var request = requests[index];
                if (request == null || !CharacterMotionProfileValidator.IsStableId(request.CharacterId))
                    continue;
                counts[request.CharacterId] = counts.TryGetValue(request.CharacterId, out var count)
                    ? count + 1
                    : 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterMotionBindingBatchIssue(
                        CharacterMotionBindingBatchIssueCode.DuplicateCharacterBinding,
                        RequestPath(pair.Key) + ".characterId"));
                }
            }
        }

        private static List<CharacterMotionBindingRecord> EvaluateRequests(
            CharacterMotionProfileCatalogue catalogue,
            IReadOnlyList<CharacterMotionBindingRequest> requests,
            ICollection<CharacterMotionBindingBatchIssue> issues)
        {
            var records = new List<CharacterMotionBindingRecord>();
            for (var index = 0; index < requests.Count; index++)
            {
                var request = requests[index];
                if (request == null)
                {
                    issues.Add(new CharacterMotionBindingBatchIssue(
                        CharacterMotionBindingBatchIssueCode.NullRequest,
                        "bindings"));
                    continue;
                }

                var requestPath = RequestPath(request.CharacterId);
                var characterIdValid = CharacterMotionProfileValidator.IsStableId(request.CharacterId);
                if (!characterIdValid)
                {
                    issues.Add(new CharacterMotionBindingBatchIssue(
                        CharacterMotionBindingBatchIssueCode.InvalidCharacterId,
                        requestPath + ".characterId"));
                }

                var motionProfileIdValid = CharacterMotionProfileValidator.IsStableId(request.MotionProfileId);
                if (!motionProfileIdValid)
                {
                    issues.Add(new CharacterMotionBindingBatchIssue(
                        CharacterMotionBindingBatchIssueCode.InvalidMotionProfileId,
                        requestPath + ".motionProfileId"));
                }

                if (!characterIdValid || !motionProfileIdValid)
                    continue;

                if (!catalogue.TryGetByMotionProfileId(request.MotionProfileId, out var profile))
                {
                    issues.Add(new CharacterMotionBindingBatchIssue(
                        CharacterMotionBindingBatchIssueCode.MissingProfile,
                        requestPath + ".motionProfileId"));
                    continue;
                }

                var compatibility = CharacterMotionCompatibilityEvaluator.Evaluate(
                    profile,
                    request.TargetRequirements);
                if (!compatibility.IsCompatible)
                {
                    AddCompatibilityIssues(requestPath, compatibility.Issues, issues);
                    continue;
                }

                records.Add(new CharacterMotionBindingRecord(
                    request.CharacterId,
                    request.MotionProfileId,
                    profile));
            }
            return records;
        }

        private static void AddCompatibilityIssues(
            string requestPath,
            IReadOnlyList<CharacterMotionCompatibilityIssue> compatibilityIssues,
            ICollection<CharacterMotionBindingBatchIssue> issues)
        {
            for (var index = 0; index < compatibilityIssues.Count; index++)
            {
                var issue = compatibilityIssues[index];
                issues.Add(new CharacterMotionBindingBatchIssue(
                    CharacterMotionBindingBatchIssueCode.IncompatibleProfile,
                    requestPath + "." + issue.Path,
                    issue.Code,
                    issue.ProfileIssueCode));
            }
        }

        private static void SortIssues(List<CharacterMotionBindingBatchIssue> issues)
        {
            issues.Sort((left, right) =>
            {
                var path = string.CompareOrdinal(left.Path, right.Path);
                if (path != 0)
                    return path;
                var code = left.Code.CompareTo(right.Code);
                if (code != 0)
                    return code;
                var compatibility = Nullable.Compare(
                    left.CompatibilityIssueCode,
                    right.CompatibilityIssueCode);
                return compatibility != 0
                    ? compatibility
                    : Nullable.Compare(left.ProfileIssueCode, right.ProfileIssueCode);
            });
        }

        private static string RequestPath(string characterId)
        {
            return string.IsNullOrEmpty(characterId)
                ? "bindings"
                : "bindings[\"" + EscapePathSegment(characterId) + "\"]";
        }

        private static string EscapePathSegment(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
