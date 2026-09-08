using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace RealmRaiders.Modules.CharacterArtManifests
{
    public enum CharacterArtIntakeComplianceIssueCode
    {
        MissingManifest,
        InvalidManifest,
        MissingMeasurement,
        UnreadableInput,
        MissingMeasurementId,
        InvalidMeasurementId,
        SourceIdMismatch,
        CharacterIdMismatch,
        NullLodTriangleCounts,
        UnreadableLodTriangleCounts,
        NullLodTriangleMeasurement,
        UnknownLodLevel,
        DuplicateLodLevel,
        MissingLodLevel,
        InvalidTriangleCount,
        TriangleBudgetExceeded,
        InvalidRendererCount,
        RendererBudgetExceeded,
        InvalidMaterialCount,
        MaterialBudgetExceeded,
        InvalidTextureCount,
        TextureBudgetExceeded,
        InvalidTextureEdge,
        TextureEdgeBudgetExceeded,
        SourceCollidersPresent,
        RootMotionPresent,
        AnimationEventsPresent,
        RequiredAnimationsMissing,
        UnexpectedAnimations,
        NullImportedMotionClipIds,
        UnreadableImportedMotionClipIds,
        NullMotionClipId,
        InvalidMotionClipId,
        DuplicateMotionClipId,
        MissingMotionClip,
        UnexpectedMotionClip
    }

    public sealed class CharacterArtIntakeComplianceIssue
    {
        public CharacterArtIntakeComplianceIssue(
            CharacterArtIntakeComplianceIssueCode code,
            string path,
            CharacterArtManifestIssueCode? manifestIssueCode = null)
        {
            Code = code;
            Path = path;
            ManifestIssueCode = manifestIssueCode;
        }

        public CharacterArtIntakeComplianceIssueCode Code { get; }
        public string Path { get; }
        public CharacterArtManifestIssueCode? ManifestIssueCode { get; }
    }

    public sealed class CharacterArtIntakeComplianceResult
    {
        private readonly ReadOnlyCollection<CharacterArtIntakeComplianceIssue> issues;

        internal CharacterArtIntakeComplianceResult(
            IList<CharacterArtIntakeComplianceIssue> issues)
        {
            this.issues = new ReadOnlyCollection<CharacterArtIntakeComplianceIssue>(
                new List<CharacterArtIntakeComplianceIssue>(issues));
        }

        public bool IsCompliant => issues.Count == 0;
        public IReadOnlyList<CharacterArtIntakeComplianceIssue> Issues => issues;
    }

    /// <summary>
    /// Compares explicit adapter-neutral measurements with one intake manifest.
    /// It does not locate, inspect, import, or modify art or Unity objects.
    /// </summary>
    public static class CharacterArtIntakeComplianceEvaluator
    {
        public static CharacterArtIntakeComplianceResult Evaluate(
            CharacterArtIntakeManifest manifest,
            CharacterArtMeasurementSnapshot measurement)
        {
            try
            {
                return EvaluateCore(manifest, measurement);
            }
            catch (Exception)
            {
                return Failure(
                    CharacterArtIntakeComplianceIssueCode.UnreadableInput,
                    "evaluation");
            }
        }

        private static CharacterArtIntakeComplianceResult EvaluateCore(
            CharacterArtIntakeManifest manifest,
            CharacterArtMeasurementSnapshot measurement)
        {
            var issues = new List<CharacterArtIntakeComplianceIssue>();
            if (manifest == null)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.MissingManifest,
                    "manifest"));
                return Result(issues);
            }

            var manifestIssues = CharacterArtIntakeManifestValidator.Validate(manifest);
            for (var index = 0; index < manifestIssues.Count; index++)
            {
                var issue = manifestIssues[index];
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.InvalidManifest,
                    "manifest." + issue.Path,
                    issue.Code));
            }
            if (issues.Count != 0)
                return Result(issues);

            if (measurement == null)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.MissingMeasurement,
                    "measurement"));
                return Result(issues);
            }

            ValidateIdentity(
                measurement.SourceId,
                manifest.SourceId,
                "measurement.sourceId",
                CharacterArtIntakeComplianceIssueCode.SourceIdMismatch,
                issues);
            ValidateIdentity(
                measurement.CharacterId,
                manifest.CharacterId,
                "measurement.characterId",
                CharacterArtIntakeComplianceIssueCode.CharacterIdMismatch,
                issues);
            ValidateLodTriangleCounts(manifest, measurement, issues);
            ValidateCount(
                measurement.RendererCount,
                manifest.MaxRendererCount,
                "measurement.rendererCount",
                CharacterArtIntakeComplianceIssueCode.InvalidRendererCount,
                CharacterArtIntakeComplianceIssueCode.RendererBudgetExceeded,
                issues);
            ValidateCount(
                measurement.MaterialCount,
                manifest.MaxMaterialCount,
                "measurement.materialCount",
                CharacterArtIntakeComplianceIssueCode.InvalidMaterialCount,
                CharacterArtIntakeComplianceIssueCode.MaterialBudgetExceeded,
                issues);
            ValidateCount(
                measurement.TextureCount,
                manifest.MaxTextureCount,
                "measurement.textureCount",
                CharacterArtIntakeComplianceIssueCode.InvalidTextureCount,
                CharacterArtIntakeComplianceIssueCode.TextureBudgetExceeded,
                issues);
            ValidateCount(
                measurement.MaxTextureEdgePixels,
                manifest.MaxTextureDimensionPixels,
                "measurement.maxTextureEdgePixels",
                CharacterArtIntakeComplianceIssueCode.InvalidTextureEdge,
                CharacterArtIntakeComplianceIssueCode.TextureEdgeBudgetExceeded,
                issues);
            ValidateProhibitedImportState(measurement, issues);
            ValidateAnimationCoverage(manifest, measurement, issues);
            return Result(issues);
        }

        private static void ValidateIdentity(
            string actual,
            string expected,
            string path,
            CharacterArtIntakeComplianceIssueCode mismatchCode,
            ICollection<CharacterArtIntakeComplianceIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(actual))
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.MissingMeasurementId,
                    path));
            }
            else if (!IsStableId(actual))
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.InvalidMeasurementId,
                    path));
            }

            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    mismatchCode,
                    path));
            }
        }

        private static void ValidateLodTriangleCounts(
            CharacterArtIntakeManifest manifest,
            CharacterArtMeasurementSnapshot measurement,
            ICollection<CharacterArtIntakeComplianceIssue> issues)
        {
            if (measurement.LodTriangleCountsState == CharacterArtMeasurementCollectionState.Null)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.NullLodTriangleCounts,
                    "measurement.lodTriangleCounts"));
                return;
            }
            if (measurement.LodTriangleCountsState == CharacterArtMeasurementCollectionState.Unreadable)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.UnreadableLodTriangleCounts,
                    "measurement.lodTriangleCounts"));
                return;
            }

            var countsByLevel = new Dictionary<CharacterArtLodLevel, List<int>>();
            for (var index = 0; index < measurement.LodTriangleCounts.Count; index++)
            {
                var value = measurement.LodTriangleCounts[index];
                if (value == null)
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.NullLodTriangleMeasurement,
                        "measurement.lodTriangleCounts"));
                    continue;
                }

                var levelName = CharacterArtIntakeManifestValidator.LodLevelName(value.Level);
                if (levelName == null)
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.UnknownLodLevel,
                        "measurement.lodTriangleCounts[" +
                        Convert.ToInt32(value.Level).ToString(CultureInfo.InvariantCulture) + "].level"));
                    continue;
                }

                if (!countsByLevel.TryGetValue(value.Level, out var values))
                {
                    values = new List<int>();
                    countsByLevel.Add(value.Level, values);
                }
                values.Add(value.TriangleCount);
            }

            for (var index = 0; index < manifest.LodTriangleBudgets.Count; index++)
            {
                var budget = manifest.LodTriangleBudgets[index];
                var path = "measurement.lodTriangleCounts." +
                    CharacterArtIntakeManifestValidator.LodLevelName(budget.Level);
                if (!countsByLevel.TryGetValue(budget.Level, out var values))
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.MissingLodLevel,
                        path));
                    continue;
                }
                if (values.Count > 1)
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.DuplicateLodLevel,
                        path));
                    continue;
                }

                var triangleCount = values[0];
                if (triangleCount <= 0)
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.InvalidTriangleCount,
                        path + ".triangleCount"));
                }
                else if (triangleCount > budget.MaxTriangles)
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.TriangleBudgetExceeded,
                        path + ".triangleCount"));
                }
            }
        }

        private static void ValidateCount(
            int actual,
            int maximum,
            string path,
            CharacterArtIntakeComplianceIssueCode invalidCode,
            CharacterArtIntakeComplianceIssueCode exceededCode,
            ICollection<CharacterArtIntakeComplianceIssue> issues)
        {
            if (actual < 0)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(invalidCode, path));
            }
            else if (actual > maximum)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(exceededCode, path));
            }
        }

        private static void ValidateProhibitedImportState(
            CharacterArtMeasurementSnapshot measurement,
            ICollection<CharacterArtIntakeComplianceIssue> issues)
        {
            if (measurement.HasSourceColliders)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.SourceCollidersPresent,
                    "measurement.hasSourceColliders"));
            }
            if (measurement.UsesRootMotion)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.RootMotionPresent,
                    "measurement.usesRootMotion"));
            }
            if (measurement.HasAnimationEvents)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.AnimationEventsPresent,
                    "measurement.hasAnimationEvents"));
            }
        }

        private static void ValidateAnimationCoverage(
            CharacterArtIntakeManifest manifest,
            CharacterArtMeasurementSnapshot measurement,
            ICollection<CharacterArtIntakeComplianceIssue> issues)
        {
            if (manifest.ImportAnimations && !measurement.AnimationsImported)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.RequiredAnimationsMissing,
                    "measurement.animationsImported"));
            }
            else if (!manifest.ImportAnimations && measurement.AnimationsImported)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.UnexpectedAnimations,
                    "measurement.animationsImported"));
            }

            if (measurement.ImportedMotionClipIdsState == CharacterArtMeasurementCollectionState.Null)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.NullImportedMotionClipIds,
                    "measurement.importedMotionClipIds"));
                return;
            }
            if (measurement.ImportedMotionClipIdsState == CharacterArtMeasurementCollectionState.Unreadable)
            {
                issues.Add(new CharacterArtIntakeComplianceIssue(
                    CharacterArtIntakeComplianceIssueCode.UnreadableImportedMotionClipIds,
                    "measurement.importedMotionClipIds"));
                return;
            }

            var clipCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < measurement.ImportedMotionClipIds.Count; index++)
            {
                var clipId = measurement.ImportedMotionClipIds[index];
                if (clipId == null)
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.NullMotionClipId,
                        "measurement.importedMotionClipIds"));
                    continue;
                }

                if (!IsStableId(clipId))
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.InvalidMotionClipId,
                        "measurement.importedMotionClipIds[" + clipId + "]"));
                    continue;
                }

                clipCounts[clipId] = clipCounts.TryGetValue(clipId, out var count)
                    ? count + 1
                    : 1;
            }

            var requiredClipIds = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < manifest.MotionClips.Count; index++)
                requiredClipIds.Add(manifest.MotionClips[index].ClipId);

            foreach (var pair in clipCounts)
            {
                var path = "measurement.importedMotionClipIds[" + pair.Key + "]";
                if (pair.Value > 1)
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.DuplicateMotionClipId,
                        path));
                }
                if (!manifest.ImportAnimations || !requiredClipIds.Contains(pair.Key))
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.UnexpectedMotionClip,
                        path));
                }
            }

            if (!manifest.ImportAnimations)
                return;

            foreach (var requiredClipId in requiredClipIds)
            {
                if (!clipCounts.ContainsKey(requiredClipId))
                {
                    issues.Add(new CharacterArtIntakeComplianceIssue(
                        CharacterArtIntakeComplianceIssueCode.MissingMotionClip,
                        "measurement.importedMotionClipIds[" + requiredClipId + "]"));
                }
            }
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

        private static CharacterArtIntakeComplianceResult Failure(
            CharacterArtIntakeComplianceIssueCode code,
            string path)
        {
            return Result(new List<CharacterArtIntakeComplianceIssue>
            {
                new CharacterArtIntakeComplianceIssue(code, path)
            });
        }

        private static CharacterArtIntakeComplianceResult Result(
            List<CharacterArtIntakeComplianceIssue> issues)
        {
            issues.Sort(CompareIssues);
            return new CharacterArtIntakeComplianceResult(issues);
        }

        private static int CompareIssues(
            CharacterArtIntakeComplianceIssue left,
            CharacterArtIntakeComplianceIssue right)
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
}
