using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterArtManifests.Tests
{
    public sealed class CharacterArtIntakeComplianceEvaluatorTests
    {
        [Test]
        public void ExactLimitsAndReorderedCollections_AreCompliant()
        {
            var manifest = ValidManifest();
            var measurement = ValidMeasurement(
                lodTriangleCounts: RequiredLodMeasurements().Reverse(),
                importedMotionClipIds: RequiredClipIds().Reverse());

            var result = CharacterArtIntakeComplianceEvaluator.Evaluate(manifest, measurement);

            Assert.That(result.IsCompliant, Is.True);
            Assert.That(result.Issues, Is.Empty);
        }

        [Test]
        public void EveryOverBudgetMeasurement_FailsAtItsExactPath()
        {
            var measurement = ValidMeasurement(
                lodTriangleCounts: new[]
                {
                    Lod(CharacterArtLodLevel.Lod0, 4001),
                    Lod(CharacterArtLodLevel.Lod1, 2001),
                    Lod(CharacterArtLodLevel.Lod2, 801)
                },
                rendererCount: 4,
                materialCount: 2,
                textureCount: 2,
                maxTextureEdgePixels: 1025);

            var result = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                measurement);

            Assert.That(result.IsCompliant, Is.False);
            Assert.That(Codes(result).Count(code =>
                code == CharacterArtIntakeComplianceIssueCode.TriangleBudgetExceeded), Is.EqualTo(3));
            Assert.That(Codes(result), Does.Contain(CharacterArtIntakeComplianceIssueCode.RendererBudgetExceeded));
            Assert.That(Codes(result), Does.Contain(CharacterArtIntakeComplianceIssueCode.MaterialBudgetExceeded));
            Assert.That(Codes(result), Does.Contain(CharacterArtIntakeComplianceIssueCode.TextureBudgetExceeded));
            Assert.That(Codes(result), Does.Contain(CharacterArtIntakeComplianceIssueCode.TextureEdgeBudgetExceeded));
        }

        [Test]
        public void IdentityComparison_IsExactOrdinalAndRejectsMalformedIds()
        {
            var caseMismatch = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                ValidMeasurement(sourceId: "test.source.SENTINEL.v1"));
            var invalidCharacter = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                ValidMeasurement(characterId: "Test/Character"));
            var missingSource = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                ValidMeasurement(sourceId: " "));

            Assert.That(Codes(caseMismatch), Does.Contain(CharacterArtIntakeComplianceIssueCode.InvalidMeasurementId));
            Assert.That(Codes(caseMismatch), Does.Contain(CharacterArtIntakeComplianceIssueCode.SourceIdMismatch));
            Assert.That(Codes(invalidCharacter), Does.Contain(CharacterArtIntakeComplianceIssueCode.InvalidMeasurementId));
            Assert.That(Codes(invalidCharacter), Does.Contain(CharacterArtIntakeComplianceIssueCode.CharacterIdMismatch));
            Assert.That(Codes(missingSource), Does.Contain(CharacterArtIntakeComplianceIssueCode.MissingMeasurementId));
            Assert.That(Codes(missingSource), Does.Contain(CharacterArtIntakeComplianceIssueCode.SourceIdMismatch));
        }

        [Test]
        public void ObservedColliderRootMotionAndAnimationEvents_AreRejected()
        {
            var result = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                ValidMeasurement(
                    hasSourceColliders: true,
                    usesRootMotion: true,
                    hasAnimationEvents: true));

            Assert.That(Codes(result), Does.Contain(CharacterArtIntakeComplianceIssueCode.SourceCollidersPresent));
            Assert.That(Codes(result), Does.Contain(CharacterArtIntakeComplianceIssueCode.RootMotionPresent));
            Assert.That(Codes(result), Does.Contain(CharacterArtIntakeComplianceIssueCode.AnimationEventsPresent));
        }

        [Test]
        public void AnimationAndClipCoverage_FollowsManifestFlagExactly()
        {
            var required = RequiredClipIds().ToArray();
            var incomplete = required
                .Where(clipId => clipId != ClipId(CharacterArtMotionClipKey.Death))
                .Concat(new[]
                {
                    required[0],
                    "test.clip.large-creature.extra.v1"
                });
            var importedResult = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                ValidMeasurement(importedMotionClipIds: incomplete));

            Assert.That(Codes(importedResult), Does.Contain(CharacterArtIntakeComplianceIssueCode.DuplicateMotionClipId));
            Assert.That(Codes(importedResult), Does.Contain(CharacterArtIntakeComplianceIssueCode.MissingMotionClip));
            Assert.That(Codes(importedResult), Does.Contain(CharacterArtIntakeComplianceIssueCode.UnexpectedMotionClip));

            var missingAnimationResult = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                ValidMeasurement(animationsImported: false));
            Assert.That(Codes(missingAnimationResult), Does.Contain(CharacterArtIntakeComplianceIssueCode.RequiredAnimationsMissing));

            var noAnimationManifest = ValidManifest(importAnimations: false);
            var noAnimationResult = CharacterArtIntakeComplianceEvaluator.Evaluate(
                noAnimationManifest,
                ValidMeasurement(
                    animationsImported: false,
                    importedMotionClipIds: Array.Empty<string>()));
            Assert.That(noAnimationResult.IsCompliant, Is.True);

            var unexpectedResult = CharacterArtIntakeComplianceEvaluator.Evaluate(
                noAnimationManifest,
                ValidMeasurement(
                    animationsImported: true,
                    importedMotionClipIds: new[] { required[0] }));
            Assert.That(Codes(unexpectedResult), Does.Contain(CharacterArtIntakeComplianceIssueCode.UnexpectedAnimations));
            Assert.That(Codes(unexpectedResult), Does.Contain(CharacterArtIntakeComplianceIssueCode.UnexpectedMotionClip));
        }

        [Test]
        public void NullInvalidAndUnreadableInputs_FailClosedWithoutThrowing()
        {
            Assert.That(
                Codes(CharacterArtIntakeComplianceEvaluator.Evaluate(null, ValidMeasurement())),
                Does.Contain(CharacterArtIntakeComplianceIssueCode.MissingManifest));
            Assert.That(
                Codes(CharacterArtIntakeComplianceEvaluator.Evaluate(ValidManifest(), null)),
                Does.Contain(CharacterArtIntakeComplianceIssueCode.MissingMeasurement));

            var invalidManifestResult = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(maxRendererCount: 0),
                ValidMeasurement());
            Assert.That(invalidManifestResult.Issues.Any(issue =>
                issue.Code == CharacterArtIntakeComplianceIssueCode.InvalidManifest &&
                issue.ManifestIssueCode == CharacterArtManifestIssueCode.InvalidRendererBudget), Is.True);

            var nullCollections = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                Measurement(null, null));
            Assert.That(Codes(nullCollections), Does.Contain(CharacterArtIntakeComplianceIssueCode.NullLodTriangleCounts));
            Assert.That(Codes(nullCollections), Does.Contain(CharacterArtIntakeComplianceIssueCode.NullImportedMotionClipIds));

            CharacterArtMeasurementSnapshot unreadable = null;
            Assert.DoesNotThrow(() => unreadable = Measurement(
                new ThrowingEnumerable<CharacterArtLodTriangleMeasurement>(),
                new ThrowingEnumerable<string>()));
            CharacterArtIntakeComplianceResult unreadableResult = null;
            Assert.DoesNotThrow(() => unreadableResult =
                CharacterArtIntakeComplianceEvaluator.Evaluate(ValidManifest(), unreadable));
            Assert.That(Codes(unreadableResult), Does.Contain(CharacterArtIntakeComplianceIssueCode.UnreadableLodTriangleCounts));
            Assert.That(Codes(unreadableResult), Does.Contain(CharacterArtIntakeComplianceIssueCode.UnreadableImportedMotionClipIds));
        }

        [Test]
        public void SnapshotAndResult_AreImmutableAndIsolatedFromCallerMutation()
        {
            var mutableLods = RequiredLodMeasurements().ToList();
            var mutableClips = RequiredClipIds().ToList();
            var measurement = ValidMeasurement(
                lodTriangleCounts: mutableLods,
                importedMotionClipIds: mutableClips);
            mutableLods.Clear();
            mutableClips.Clear();

            var result = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                measurement);

            Assert.That(measurement.LodTriangleCounts.Count, Is.EqualTo(3));
            Assert.That(measurement.ImportedMotionClipIds.Count, Is.EqualTo(6));
            Assert.That(result.IsCompliant, Is.True);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterArtLodTriangleMeasurement>)measurement.LodTriangleCounts).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<string>)measurement.ImportedMotionClipIds).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterArtIntakeComplianceIssue>)result.Issues).Clear());
            Assert.That(typeof(CharacterArtMeasurementSnapshot)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtLodTriangleMeasurement)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtIntakeComplianceIssue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtIntakeComplianceResult)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
        }

        [Test]
        public void InvalidCollectionOrder_ProducesTheSameSortedIssuesAndNoUnityDependency()
        {
            var lods = new CharacterArtLodTriangleMeasurement[]
            {
                null,
                Lod((CharacterArtLodLevel)99, 4),
                Lod(CharacterArtLodLevel.Lod0, 4000),
                Lod(CharacterArtLodLevel.Lod0, 4500),
                Lod(CharacterArtLodLevel.Lod1, -1)
            };
            var clips = new[]
            {
                null,
                "Bad/clip",
                "test.clip.extra.v1",
                "test.clip.extra.v1"
            };

            var first = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                ValidMeasurement(
                    lodTriangleCounts: lods,
                    rendererCount: -1,
                    materialCount: -1,
                    textureCount: -1,
                    maxTextureEdgePixels: -1,
                    importedMotionClipIds: clips));
            var second = CharacterArtIntakeComplianceEvaluator.Evaluate(
                ValidManifest(),
                ValidMeasurement(
                    lodTriangleCounts: lods.Reverse(),
                    rendererCount: -1,
                    materialCount: -1,
                    textureCount: -1,
                    maxTextureEdgePixels: -1,
                    importedMotionClipIds: clips.Reverse()));

            Assert.That(Signatures(first), Is.EqualTo(Signatures(second)));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.NullLodTriangleMeasurement));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.UnknownLodLevel));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.DuplicateLodLevel));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.MissingLodLevel));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.InvalidTriangleCount));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.InvalidRendererCount));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.InvalidMaterialCount));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.InvalidTextureCount));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.InvalidTextureEdge));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.NullMotionClipId));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.InvalidMotionClipId));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.DuplicateMotionClipId));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.MissingMotionClip));
            Assert.That(Codes(first), Does.Contain(CharacterArtIntakeComplianceIssueCode.UnexpectedMotionClip));
            Assert.That(first.Issues, Is.EqualTo(first.Issues
                .OrderBy(issue => issue.Path, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code)
                .ThenBy(issue => issue.ManifestIssueCode)
                .ToArray()));

            var dependencies = typeof(CharacterArtMeasurementSnapshot).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();
            Assert.That(dependencies.Any(name =>
                name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static CharacterArtIntakeComplianceIssueCode[] Codes(
            CharacterArtIntakeComplianceResult result)
        {
            return result.Issues.Select(issue => issue.Code).ToArray();
        }

        private static string[] Signatures(CharacterArtIntakeComplianceResult result)
        {
            return result.Issues
                .Select(issue => issue.Path + "|" + issue.Code + "|" + issue.ManifestIssueCode)
                .ToArray();
        }

        private static CharacterArtMeasurementSnapshot ValidMeasurement(
            string characterId = "test.character.sentinel",
            string sourceId = "test.source.sentinel.v1",
            IEnumerable<CharacterArtLodTriangleMeasurement> lodTriangleCounts = null,
            int rendererCount = 3,
            int materialCount = 1,
            int textureCount = 1,
            int maxTextureEdgePixels = 1024,
            bool hasSourceColliders = false,
            bool usesRootMotion = false,
            bool hasAnimationEvents = false,
            bool animationsImported = true,
            IEnumerable<string> importedMotionClipIds = null)
        {
            return Measurement(
                lodTriangleCounts ?? RequiredLodMeasurements(),
                importedMotionClipIds ?? RequiredClipIds(),
                characterId,
                sourceId,
                rendererCount,
                materialCount,
                textureCount,
                maxTextureEdgePixels,
                hasSourceColliders,
                usesRootMotion,
                hasAnimationEvents,
                animationsImported);
        }

        private static CharacterArtMeasurementSnapshot Measurement(
            IEnumerable<CharacterArtLodTriangleMeasurement> lodTriangleCounts,
            IEnumerable<string> importedMotionClipIds,
            string characterId = "test.character.sentinel",
            string sourceId = "test.source.sentinel.v1",
            int rendererCount = 3,
            int materialCount = 1,
            int textureCount = 1,
            int maxTextureEdgePixels = 1024,
            bool hasSourceColliders = false,
            bool usesRootMotion = false,
            bool hasAnimationEvents = false,
            bool animationsImported = true)
        {
            return new CharacterArtMeasurementSnapshot(
                characterId,
                sourceId,
                lodTriangleCounts,
                rendererCount,
                materialCount,
                textureCount,
                maxTextureEdgePixels,
                hasSourceColliders,
                usesRootMotion,
                hasAnimationEvents,
                animationsImported,
                importedMotionClipIds);
        }

        private static IEnumerable<CharacterArtLodTriangleMeasurement> RequiredLodMeasurements()
        {
            yield return Lod(CharacterArtLodLevel.Lod0, 4000);
            yield return Lod(CharacterArtLodLevel.Lod1, 2000);
            yield return Lod(CharacterArtLodLevel.Lod2, 800);
        }

        private static CharacterArtLodTriangleMeasurement Lod(
            CharacterArtLodLevel level,
            int triangleCount)
        {
            return new CharacterArtLodTriangleMeasurement(level, triangleCount);
        }

        private static IEnumerable<string> RequiredClipIds()
        {
            yield return ClipId(CharacterArtMotionClipKey.Idle);
            yield return ClipId(CharacterArtMotionClipKey.Locomotion);
            yield return ClipId(CharacterArtMotionClipKey.AttackPrimary);
            yield return ClipId(CharacterArtMotionClipKey.AttackAbility);
            yield return ClipId(CharacterArtMotionClipKey.Hit);
            yield return ClipId(CharacterArtMotionClipKey.Death);
        }

        private static CharacterArtIntakeManifest ValidManifest(
            bool importAnimations = true,
            int maxRendererCount = 3)
        {
            return new CharacterArtIntakeManifest(
                CharacterArtIntakeManifestValidator.SupportedSchemaVersion,
                "test.character.sentinel",
                "test.source.sentinel.v1",
                CharacterBodyFamily.LargeCreature,
                "Test Sentinel Source",
                "Test Creator",
                "https://example.com/source/sentinel-v1.zip",
                "Test Permissive License 1.0",
                "https://example.com/licenses/test-1.0",
                "Test Sentinel Source by Test Creator.",
                "Retopologized for the test fixture.",
                "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                "Source/Test Sentinel/sentinel.fbx",
                "test.rig.large-creature.v1",
                RequiredMotionClips(),
                new[]
                {
                    new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod0, 4000),
                    new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod1, 2000),
                    new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod2, 800)
                },
                maxRendererCount,
                1,
                1,
                1024,
                false,
                false,
                importAnimations,
                false,
                false,
                false);
        }

        private static IEnumerable<CharacterArtMotionClip> RequiredMotionClips()
        {
            yield return Clip(CharacterArtMotionClipKey.Idle);
            yield return Clip(CharacterArtMotionClipKey.Locomotion);
            yield return Clip(CharacterArtMotionClipKey.AttackPrimary);
            yield return Clip(CharacterArtMotionClipKey.AttackAbility);
            yield return Clip(CharacterArtMotionClipKey.Hit);
            yield return Clip(CharacterArtMotionClipKey.Death);
        }

        private static CharacterArtMotionClip Clip(CharacterArtMotionClipKey key)
        {
            return new CharacterArtMotionClip(key, ClipId(key));
        }

        private static string ClipId(CharacterArtMotionClipKey key)
        {
            var name = key == CharacterArtMotionClipKey.AttackPrimary
                ? "attack-primary"
                : key == CharacterArtMotionClipKey.AttackAbility
                    ? "attack-ability"
                    : key.ToString().ToLowerInvariant();
            return "test.clip.large-creature." + name + ".v1";
        }

        private sealed class ThrowingEnumerable<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                throw new InvalidOperationException("Synthetic unreadable input.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
