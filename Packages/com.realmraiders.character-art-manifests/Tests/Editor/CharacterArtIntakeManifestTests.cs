using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterArtManifests.Tests
{
    public sealed class CharacterArtIntakeManifestTests
    {
        [Test]
        public void ValidManifest_ProducesCanonicalBomlessUtf8AndStableHash()
        {
            var manifest = ValidManifest(
                title: "Test \"Sentinel\"",
                changeNote: "Retopology\nNo gameplay changes.");

            var issues = CharacterArtIntakeManifestValidator.Validate(manifest);
            var bytes = CharacterArtIntakeManifestCanonicalizer.SerializeUtf8(manifest);
            var json = Encoding.UTF8.GetString(bytes);

            Assert.That(issues, Is.Empty);
            Assert.That(bytes[0], Is.EqualTo((byte)'{'));
            Assert.That(json, Does.StartWith("{\"schemaVersion\":1,\"characterId\":\"test.character.sentinel\""));
            Assert.That(json, Does.Contain("\"title\":\"Test \\\"Sentinel\\\"\""));
            Assert.That(json, Does.Contain("\"changeNote\":\"Retopology\\nNo gameplay changes.\""));
            Assert.That(CharacterArtIntakeManifestCanonicalizer.ContentHash(manifest), Does.Match("^[0-9a-f]{64}$"));
        }

        [Test]
        public void Validator_RejectsDuplicateMotionAndLodKeys()
        {
            var duplicateClips = RequiredMotionClips()
                .Concat(new[] { Clip(CharacterArtMotionClipKey.Idle, "test.clip.duplicate.idle") });
            var duplicateLods = new[]
            {
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod0, 4000),
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod1, 2000),
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod1, 800)
            };

            var codes = Codes(ValidManifest(motionClips: duplicateClips, lodBudgets: duplicateLods));

            Assert.That(codes, Does.Contain(CharacterArtManifestIssueCode.DuplicateMotionClipKey));
            Assert.That(codes, Does.Contain(CharacterArtManifestIssueCode.DuplicateLodLevel));
            Assert.That(codes, Does.Contain(CharacterArtManifestIssueCode.MissingLodBudget));
        }

        [Test]
        public void Validator_RejectsMalformedIdsUrlsHashesAndUnsafeSourcePaths()
        {
            var invalid = ValidManifest(
                characterId: "Test/Character",
                sourceId: "test..source",
                family: (CharacterBodyFamily)99,
                directSourceUrl: "ftp://example.com/source.zip",
                licenseLegalUrl: "relative/license",
                archiveSha256: new string('A', 64),
                selectedSourceFile: "Source/../character.fbx",
                rigProfileId: "test_rig");

            var codes = Codes(invalid);

            Assert.That(codes.Count(code => code == CharacterArtManifestIssueCode.InvalidId), Is.EqualTo(3));
            Assert.That(codes, Does.Contain(CharacterArtManifestIssueCode.UnknownFamily));
            Assert.That(codes.Count(code => code == CharacterArtManifestIssueCode.InvalidUrl), Is.EqualTo(2));
            Assert.That(codes, Does.Contain(CharacterArtManifestIssueCode.InvalidArchiveSha256));
            Assert.That(codes, Does.Contain(CharacterArtManifestIssueCode.InvalidSelectedSourceFile));
            Assert.Throws<ArgumentException>(() => CharacterArtIntakeManifestCanonicalizer.ContentHash(invalid));

            Assert.That(
                Codes(ValidManifest(selectedSourceFile: "/Source/character.fbx")),
                Does.Contain(CharacterArtManifestIssueCode.InvalidSelectedSourceFile));
            Assert.That(
                Codes(ValidManifest(selectedSourceFile: "C:\\Source\\character.fbx")),
                Does.Contain(CharacterArtManifestIssueCode.InvalidSelectedSourceFile));
        }

        [Test]
        public void Validator_RequiresLicenseAttributionChangeAndSourceFacts()
        {
            var manifest = ValidManifest(
                title: " ",
                creator: null,
                licenseName: string.Empty,
                attribution: "",
                changeNote: null);

            var issues = CharacterArtIntakeManifestValidator.Validate(manifest);

            Assert.That(issues.Any(issue => issue.Code == CharacterArtManifestIssueCode.MissingText && issue.Path == "title"), Is.True);
            Assert.That(issues.Any(issue => issue.Code == CharacterArtManifestIssueCode.MissingText && issue.Path == "creator"), Is.True);
            Assert.That(issues.Any(issue => issue.Code == CharacterArtManifestIssueCode.MissingText && issue.Path == "licenseName"), Is.True);
            Assert.That(issues.Any(issue => issue.Code == CharacterArtManifestIssueCode.MissingText && issue.Path == "attribution"), Is.True);
            Assert.That(issues.Any(issue => issue.Code == CharacterArtManifestIssueCode.MissingText && issue.Path == "changeNote"), Is.True);
        }

        [Test]
        public void Validator_RequiresAllSixMotionKeys()
        {
            var missing = RequiredMotionClips()
                .Where(clip => clip.Key != CharacterArtMotionClipKey.Death);
            var unknown = RequiredMotionClips()
                .Concat(new[] { Clip((CharacterArtMotionClipKey)99, "test.clip.unknown") });

            var missingIssues = CharacterArtIntakeManifestValidator.Validate(
                ValidManifest(motionClips: missing));
            var unknownCodes = Codes(ValidManifest(motionClips: unknown));

            Assert.That(missingIssues.Any(issue =>
                issue.Code == CharacterArtManifestIssueCode.MissingMotionClip &&
                issue.Path == "motionClips.death"), Is.True);
            Assert.That(unknownCodes, Does.Contain(CharacterArtManifestIssueCode.UnknownMotionClipKey));
        }

        [Test]
        public void Validator_RequiresOrderedDescendingLodsAndPositiveBudgets()
        {
            var unordered = new[]
            {
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod1, 2000),
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod0, 4000),
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod2, 800)
            };
            var nonDescending = new[]
            {
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod0, 4000),
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod1, 4000),
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod2, 800)
            };
            var invalidTriangles = new[]
            {
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod0, 4000),
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod1, 2000),
                new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod2, 0)
            };

            Assert.That(
                Codes(ValidManifest(lodBudgets: unordered)),
                Does.Contain(CharacterArtManifestIssueCode.UnorderedLodLevels));
            Assert.That(
                Codes(ValidManifest(lodBudgets: nonDescending)),
                Does.Contain(CharacterArtManifestIssueCode.NonDescendingTriangleBudgets));
            Assert.That(
                Codes(ValidManifest(lodBudgets: invalidTriangles)),
                Does.Contain(CharacterArtManifestIssueCode.InvalidTriangleBudget));

            var budgetCodes = Codes(ValidManifest(
                maxRendererCount: 0,
                maxMaterialCount: -1,
                maxTextureCount: 0,
                maxTextureDimensionPixels: -512));
            Assert.That(budgetCodes, Does.Contain(CharacterArtManifestIssueCode.InvalidRendererBudget));
            Assert.That(budgetCodes, Does.Contain(CharacterArtManifestIssueCode.InvalidMaterialBudget));
            Assert.That(budgetCodes, Does.Contain(CharacterArtManifestIssueCode.InvalidTextureBudget));
            Assert.That(budgetCodes, Does.Contain(CharacterArtManifestIssueCode.InvalidTextureDimensionBudget));
        }

        [Test]
        public void Manifest_SnapshotsCollectionsAndCanonicalizesMotionInputOrder()
        {
            var mutableClips = RequiredMotionClips().Reverse().ToList();
            var mutableLods = RequiredLodBudgets().ToList();
            var first = ValidManifest(motionClips: mutableClips, lodBudgets: mutableLods);
            var second = ValidManifest(motionClips: RequiredMotionClips());

            mutableClips.Clear();
            mutableLods.Clear();

            Assert.That(first.MotionClips.Count, Is.EqualTo(6));
            Assert.That(first.LodTriangleBudgets.Count, Is.EqualTo(3));
            Assert.That(
                CharacterArtIntakeManifestCanonicalizer.SerializeUtf8(first),
                Is.EqualTo(CharacterArtIntakeManifestCanonicalizer.SerializeUtf8(second)));
            Assert.That(
                CharacterArtIntakeManifestCanonicalizer.ContentHash(first),
                Is.EqualTo(CharacterArtIntakeManifestCanonicalizer.ContentHash(second)));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterArtMotionClip>)first.MotionClips).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterArtLodTriangleBudget>)first.LodTriangleBudgets).Clear());
        }

        [Test]
        public void Validator_RejectsColliderRootMotionAndAnimationEventAuthority()
        {
            var codes = Codes(ValidManifest(
                importSourceColliders: true,
                applyRootMotion: true,
                importAnimationEvents: true));

            Assert.That(codes, Does.Contain(CharacterArtManifestIssueCode.SourceCollidersNotAllowed));
            Assert.That(codes, Does.Contain(CharacterArtManifestIssueCode.RootMotionNotAllowed));
            Assert.That(codes, Does.Contain(CharacterArtManifestIssueCode.AnimationEventsNotAllowed));
        }

        [Test]
        public void Validator_IsFailClosedAndReturnsDeterministicallySortedIssues()
        {
            var missing = CharacterArtIntakeManifestValidator.Validate(null);
            Assert.That(missing.Single().Code, Is.EqualTo(CharacterArtManifestIssueCode.MissingManifest));
            Assert.Throws<ArgumentException>(() =>
                CharacterArtIntakeManifestCanonicalizer.SerializeUtf8(null));

            var issues = CharacterArtIntakeManifestValidator.Validate(ValidManifest(
                characterId: "Bad/Character",
                title: "",
                directSourceUrl: "not-a-url",
                maxRendererCount: 0));

            Assert.That(issues, Is.EqualTo(issues
                .OrderBy(issue => issue.Path, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code)
                .ToArray()));
        }

        [Test]
        public void Contracts_AreImmutableAndHaveNoUnityOrGameRuntimeDependency()
        {
            Assert.That(typeof(CharacterArtIntakeManifest)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtMotionClip)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtLodTriangleBudget)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtManifestIssue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);

            var dependencies = typeof(CharacterArtIntakeManifest).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();
            Assert.That(dependencies, Has.Member("RealmRaiders.ModuleContracts"));
            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        [Test]
        public void ClosedRootPolicy_RejectsMissingDuplicateAndUnknownFields()
        {
            var fields = CanonicalRootFields()
                .Where(field => field != "archiveSha256")
                .Concat(new[] { "sourceId", "downloadTimestamp" });

            var issues = CharacterArtIntakeManifestValidator.ValidateRootFields(fields);

            Assert.That(issues.Any(issue => issue.Code == CharacterArtManifestIssueCode.MissingField && issue.Path == "archiveSha256"), Is.True);
            Assert.That(issues.Any(issue => issue.Code == CharacterArtManifestIssueCode.DuplicateField && issue.Path == "sourceId"), Is.True);
            Assert.That(issues.Any(issue => issue.Code == CharacterArtManifestIssueCode.UnknownField && issue.Path == "downloadTimestamp"), Is.True);
        }

        private static CharacterArtManifestIssueCode[] Codes(CharacterArtIntakeManifest manifest)
        {
            return CharacterArtIntakeManifestValidator.Validate(manifest)
                .Select(issue => issue.Code)
                .ToArray();
        }

        private static CharacterArtIntakeManifest ValidManifest(
            string characterId = "test.character.sentinel",
            string sourceId = "test.source.sentinel.v1",
            CharacterBodyFamily family = CharacterBodyFamily.LargeCreature,
            string title = "Test Sentinel Source",
            string creator = "Test Creator",
            string directSourceUrl = "https://example.com/source/sentinel-v1.zip",
            string licenseName = "Test Permissive License 1.0",
            string licenseLegalUrl = "https://example.com/licenses/test-1.0",
            string attribution = "Test Sentinel Source by Test Creator.",
            string changeNote = "Retopologized for the test fixture.",
            string archiveSha256 = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
            string selectedSourceFile = "Source/Test Sentinel/sentinel.fbx",
            string rigProfileId = "test.rig.large-creature.v1",
            IEnumerable<CharacterArtMotionClip> motionClips = null,
            IEnumerable<CharacterArtLodTriangleBudget> lodBudgets = null,
            int maxRendererCount = 3,
            int maxMaterialCount = 1,
            int maxTextureCount = 1,
            int maxTextureDimensionPixels = 1024,
            bool importSourceColliders = false,
            bool applyRootMotion = false,
            bool importAnimations = true,
            bool importAnimationEvents = false,
            bool importEmbeddedMaterials = false,
            bool importEmbeddedTextures = false)
        {
            return new CharacterArtIntakeManifest(
                CharacterArtIntakeManifestValidator.SupportedSchemaVersion,
                characterId,
                sourceId,
                family,
                title,
                creator,
                directSourceUrl,
                licenseName,
                licenseLegalUrl,
                attribution,
                changeNote,
                archiveSha256,
                selectedSourceFile,
                rigProfileId,
                motionClips ?? RequiredMotionClips(),
                lodBudgets ?? RequiredLodBudgets(),
                maxRendererCount,
                maxMaterialCount,
                maxTextureCount,
                maxTextureDimensionPixels,
                importSourceColliders,
                applyRootMotion,
                importAnimations,
                importAnimationEvents,
                importEmbeddedMaterials,
                importEmbeddedTextures);
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

        private static CharacterArtMotionClip Clip(
            CharacterArtMotionClipKey key,
            string clipId = null)
        {
            return new CharacterArtMotionClip(
                key,
                clipId ?? "test.clip.large-creature." + ClipName(key) + ".v1");
        }

        private static string ClipName(CharacterArtMotionClipKey key)
        {
            switch (key)
            {
                case CharacterArtMotionClipKey.AttackPrimary: return "attack-primary";
                case CharacterArtMotionClipKey.AttackAbility: return "attack-ability";
                default: return key.ToString().ToLowerInvariant();
            }
        }

        private static IEnumerable<CharacterArtLodTriangleBudget> RequiredLodBudgets()
        {
            yield return new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod0, 4000);
            yield return new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod1, 2000);
            yield return new CharacterArtLodTriangleBudget(CharacterArtLodLevel.Lod2, 800);
        }

        private static IEnumerable<string> CanonicalRootFields()
        {
            return new[]
            {
                "schemaVersion", "characterId", "sourceId", "family", "title", "creator",
                "directSourceUrl", "licenseName", "licenseLegalUrl", "attribution", "changeNote",
                "archiveSha256", "selectedSourceFile", "rigProfileId", "motionClips",
                "lodTriangleBudgets", "maxRendererCount", "maxMaterialCount", "maxTextureCount",
                "maxTextureDimensionPixels", "importSourceColliders", "applyRootMotion",
                "importAnimations", "importAnimationEvents", "importEmbeddedMaterials",
                "importEmbeddedTextures"
            };
        }
    }
}
