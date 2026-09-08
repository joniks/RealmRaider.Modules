using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterArtManifests.Tests
{
    public sealed class CharacterArtIntakeBatchReportTests
    {
        [Test]
        public void ProviderAndItemOrder_ProduceTheSameSourceSortedReport()
        {
            var firstItems = new List<CharacterArtIntakeBatchItem>
            {
                Pair("zeta"),
                Pair("alpha", rendererCount: 4)
            };
            var secondItems = new List<CharacterArtIntakeBatchItem>
            {
                Pair("middle")
            };
            var firstProviders = new ICharacterArtIntakeBatchProvider[]
            {
                new TestProvider("test.module.zeta", firstItems),
                new TestProvider("test.module.alpha", secondItems)
            };
            var secondProviders = new ICharacterArtIntakeBatchProvider[]
            {
                new TestProvider("test.module.alpha", secondItems.AsEnumerable().Reverse().ToList()),
                new TestProvider("test.module.zeta", firstItems.AsEnumerable().Reverse().ToList())
            };

            var first = CharacterArtIntakeBatchReport.Build(firstProviders);
            var second = CharacterArtIntakeBatchReport.Build(secondProviders);

            Assert.That(first.Succeeded, Is.True);
            Assert.That(second.Succeeded, Is.True);
            Assert.That(first.Report.Items.Select(item => item.SourceId), Is.EqualTo(new[]
            {
                "test.source.alpha.v1",
                "test.source.middle.v1",
                "test.source.zeta.v1"
            }));
            Assert.That(ReportSignatures(first.Report), Is.EqualTo(ReportSignatures(second.Report)));
        }

        [Test]
        public void DuplicateModuleSourceAndCharacterIds_FailClosed()
        {
            var providers = new ICharacterArtIntakeBatchProvider[]
            {
                new TestProvider("test.module.duplicate", new[] { Pair("one") }),
                new TestProvider("test.module.duplicate", new[] { Pair("two") }),
                new TestProvider("test.module.source-a", new[]
                {
                    Pair("source-a", sourceId: "test.source.shared.v1")
                }),
                new TestProvider("test.module.source-b", new[]
                {
                    Pair("source-b", sourceId: "test.source.shared.v1")
                }),
                new TestProvider("test.module.character-a", new[]
                {
                    Pair("character-a", characterId: "test.character.shared")
                }),
                new TestProvider("test.module.character-b", new[]
                {
                    Pair("character-b", characterId: "test.character.shared")
                })
            };

            var result = CharacterArtIntakeBatchReport.Build(providers);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Report, Is.Null);
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.DuplicateProviderModuleId));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.DuplicateSourceId));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.DuplicateCharacterId));
        }

        [Test]
        public void NullAndInvalidProviderItems_ReturnSortedStructuralIssues()
        {
            var invalidIdentity = Pair(
                "invalid",
                characterId: "Bad/Character",
                sourceId: "Bad/Source");
            var providers = new ICharacterArtIntakeBatchProvider[]
            {
                new TestProvider("Bad/module", new[] { Pair("valid") }),
                new TestProvider("test.module.null-collection", null),
                new TestProvider("test.module.invalid-items", new CharacterArtIntakeBatchItem[]
                {
                    null,
                    new CharacterArtIntakeBatchItem(null, null),
                    invalidIdentity
                })
            };

            var result = CharacterArtIntakeBatchReport.Build(providers);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.InvalidModuleId));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.NullItemCollection));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.NullItem));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.MissingItemManifest));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.InvalidItemSourceId));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.InvalidItemCharacterId));
            Assert.That(result.Issues, Is.EqualTo(result.Issues
                .OrderBy(issue => issue.Path, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code)
                .ToArray()));
        }

        [Test]
        public void NullAndThrowingInputs_FailClosedWithoutLeakingExceptions()
        {
            Assert.That(
                BuildCodes(CharacterArtIntakeBatchReport.Build(null)),
                Does.Contain(CharacterArtIntakeBatchBuildIssueCode.NullProviderCollection));

            CharacterArtIntakeBatchBuildResult throwingEnumerableResult = null;
            Assert.DoesNotThrow(() => throwingEnumerableResult = CharacterArtIntakeBatchReport.Build(
                new ThrowingEnumerable<ICharacterArtIntakeBatchProvider>()));
            Assert.That(
                BuildCodes(throwingEnumerableResult),
                Does.Contain(CharacterArtIntakeBatchBuildIssueCode.UnreadableProviderCollection));

            var providers = new ICharacterArtIntakeBatchProvider[]
            {
                null,
                new ThrowingProvider(throwModuleId: true),
                new ThrowingProvider(throwItems: true),
                new TestProvider(
                    "test.module.unreadable-collection",
                    new ThrowingReadOnlyList<CharacterArtIntakeBatchItem>(throwCount: true)),
                new TestProvider(
                    "test.module.unreadable-item",
                    new ThrowingReadOnlyList<CharacterArtIntakeBatchItem>(throwCount: false))
            };

            CharacterArtIntakeBatchBuildResult result = null;
            Assert.DoesNotThrow(() => result = CharacterArtIntakeBatchReport.Build(providers));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.NullProvider));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.UnreadableProvider));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.UnreadableItemCollection));
            Assert.That(BuildCodes(result), Does.Contain(CharacterArtIntakeBatchBuildIssueCode.UnreadableItem));
        }

        [Test]
        public void PerItemComplianceIssues_ArePreservedWithStableItemPaths()
        {
            var invalidManifest = Pair("invalid-manifest", maxRendererCount: 0);
            var overBudget = Pair("over-budget", rendererCount: 4);

            var result = CharacterArtIntakeBatchReport.Build(new[]
            {
                new TestProvider("test.module.batch", new[] { overBudget, invalidManifest })
            });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Report.NoncompliantCount, Is.EqualTo(2));
            Assert.That(result.Report.TryGetBySourceId(
                "test.source.invalid-manifest.v1",
                out var invalidItem), Is.True);
            Assert.That(invalidItem.Issues.Any(issue =>
                issue.Code == CharacterArtIntakeComplianceIssueCode.InvalidManifest &&
                issue.ManifestIssueCode == CharacterArtManifestIssueCode.InvalidRendererBudget &&
                issue.Path == "items[test.source.invalid-manifest.v1].manifest.maxRendererCount"), Is.True);
            Assert.That(result.Report.TryGetBySourceId(
                "test.source.over-budget.v1",
                out var overBudgetItem), Is.True);
            Assert.That(overBudgetItem.Issues.Any(issue =>
                issue.Code == CharacterArtIntakeComplianceIssueCode.RendererBudgetExceeded &&
                issue.Path == "items[test.source.over-budget.v1].measurement.rendererCount"), Is.True);
        }

        [Test]
        public void BuiltReport_IsImmutableAndIsolatedFromCallerMutation()
        {
            var mutableItems = new List<CharacterArtIntakeBatchItem>
            {
                Pair("isolated", rendererCount: 4)
            };
            var mutableProviders = new List<ICharacterArtIntakeBatchProvider>
            {
                new TestProvider("test.module.isolated", mutableItems)
            };

            var result = CharacterArtIntakeBatchReport.Build(mutableProviders);
            mutableItems.Clear();
            mutableProviders.Clear();

            Assert.That(result.Report.TotalCount, Is.EqualTo(1));
            Assert.That(result.Report.NoncompliantCount, Is.EqualTo(1));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterArtIntakeBatchReportItem>)result.Report.Items).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterArtIntakeBatchItemIssue>)result.Report.Items[0].Issues).Clear());
            Assert.That(typeof(CharacterArtIntakeBatchItem)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtIntakeBatchReportItem)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtIntakeBatchItemIssue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterArtIntakeBatchReport)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
        }

        [Test]
        public void ExactLookupsAndComplianceTotals_AreTruthful()
        {
            var result = CharacterArtIntakeBatchReport.Build(new[]
            {
                new TestProvider("test.module.counts", new[]
                {
                    Pair("one"),
                    Pair("two", rendererCount: 4),
                    Pair("three")
                })
            });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Report.TotalCount, Is.EqualTo(3));
            Assert.That(result.Report.CompliantCount, Is.EqualTo(2));
            Assert.That(result.Report.NoncompliantCount, Is.EqualTo(1));
            Assert.That(result.Report.TryGetBySourceId(
                "test.source.two.v1",
                out var bySource), Is.True);
            Assert.That(bySource.IsCompliant, Is.False);
            Assert.That(result.Report.TryGetByCharacterId(
                "test.character.three",
                out var byCharacter), Is.True);
            Assert.That(byCharacter.SourceId, Is.EqualTo("test.source.three.v1"));
            Assert.That(result.Report.TryGetBySourceId("test.source.TWO.v1", out _), Is.False);
            Assert.That(result.Report.TryGetByCharacterId("test.character.Three", out _), Is.False);
            Assert.That(result.Report.TryGetBySourceId(null, out _), Is.False);
            Assert.That(result.Report.TryGetByCharacterId(null, out _), Is.False);
        }

        [Test]
        public void EmptyExplicitProviderSet_HasNoUnityOrGameRuntimeDependency()
        {
            var result = CharacterArtIntakeBatchReport.Build(
                Array.Empty<ICharacterArtIntakeBatchProvider>());

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Report.TotalCount, Is.Zero);
            Assert.That(result.Report.CompliantCount, Is.Zero);
            Assert.That(result.Report.NoncompliantCount, Is.Zero);

            var dependencies = typeof(CharacterArtIntakeBatchReport).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();
            Assert.That(dependencies.Any(name =>
                name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static CharacterArtIntakeBatchBuildIssueCode[] BuildCodes(
            CharacterArtIntakeBatchBuildResult result)
        {
            return result.Issues.Select(issue => issue.Code).ToArray();
        }

        private static string[] ReportSignatures(CharacterArtIntakeBatchReport report)
        {
            return report.Items.Select(item =>
                item.ModuleId + "|" + item.SourceId + "|" + item.CharacterId + "|" +
                item.IsCompliant + "|" + string.Join(",", item.Issues.Select(issue =>
                    issue.Path + ":" + issue.Code + ":" + issue.ManifestIssueCode)))
                .ToArray();
        }

        private static CharacterArtIntakeBatchItem Pair(
            string suffix,
            string characterId = null,
            string sourceId = null,
            int rendererCount = 3,
            int maxRendererCount = 3)
        {
            characterId = characterId ?? "test.character." + suffix;
            sourceId = sourceId ?? "test.source." + suffix + ".v1";
            var manifest = ValidManifest(
                suffix,
                characterId,
                sourceId,
                maxRendererCount);
            return new CharacterArtIntakeBatchItem(
                manifest,
                ValidMeasurement(manifest, rendererCount));
        }

        private static CharacterArtIntakeManifest ValidManifest(
            string suffix,
            string characterId,
            string sourceId,
            int maxRendererCount)
        {
            return new CharacterArtIntakeManifest(
                CharacterArtIntakeManifestValidator.SupportedSchemaVersion,
                characterId,
                sourceId,
                CharacterBodyFamily.LargeCreature,
                "Test " + suffix + " Source",
                "Test Creator",
                "https://example.com/source/" + suffix + ".zip",
                "Test Permissive License 1.0",
                "https://example.com/licenses/test-1.0",
                "Test source by Test Creator.",
                "Prepared for the test fixture.",
                "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                "Source/Test/character.fbx",
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
                true,
                false,
                false,
                false);
        }

        private static CharacterArtMeasurementSnapshot ValidMeasurement(
            CharacterArtIntakeManifest manifest,
            int rendererCount)
        {
            return new CharacterArtMeasurementSnapshot(
                manifest.CharacterId,
                manifest.SourceId,
                new[]
                {
                    new CharacterArtLodTriangleMeasurement(CharacterArtLodLevel.Lod0, 4000),
                    new CharacterArtLodTriangleMeasurement(CharacterArtLodLevel.Lod1, 2000),
                    new CharacterArtLodTriangleMeasurement(CharacterArtLodLevel.Lod2, 800)
                },
                rendererCount,
                1,
                1,
                1024,
                false,
                false,
                false,
                true,
                RequiredClipIds());
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

        private static IEnumerable<string> RequiredClipIds()
        {
            yield return ClipId(CharacterArtMotionClipKey.Idle);
            yield return ClipId(CharacterArtMotionClipKey.Locomotion);
            yield return ClipId(CharacterArtMotionClipKey.AttackPrimary);
            yield return ClipId(CharacterArtMotionClipKey.AttackAbility);
            yield return ClipId(CharacterArtMotionClipKey.Hit);
            yield return ClipId(CharacterArtMotionClipKey.Death);
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

        private sealed class TestProvider : ICharacterArtIntakeBatchProvider
        {
            public TestProvider(
                string moduleId,
                IReadOnlyList<CharacterArtIntakeBatchItem> items)
            {
                ModuleId = moduleId;
                Items = items;
            }

            public string ModuleId { get; }
            public IReadOnlyList<CharacterArtIntakeBatchItem> Items { get; }
        }

        private sealed class ThrowingProvider : ICharacterArtIntakeBatchProvider
        {
            private readonly bool throwModuleId;
            private readonly bool throwItems;

            public ThrowingProvider(bool throwModuleId = false, bool throwItems = false)
            {
                this.throwModuleId = throwModuleId;
                this.throwItems = throwItems;
            }

            public string ModuleId => throwModuleId
                ? throw new InvalidOperationException("Synthetic unreadable module ID.")
                : "test.module.throwing";

            public IReadOnlyList<CharacterArtIntakeBatchItem> Items => throwItems
                ? throw new InvalidOperationException("Synthetic unreadable items.")
                : Array.Empty<CharacterArtIntakeBatchItem>();
        }

        private sealed class ThrowingEnumerable<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                throw new InvalidOperationException("Synthetic unreadable enumerable.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private sealed class ThrowingReadOnlyList<T> : IReadOnlyList<T>
        {
            private readonly bool throwCount;

            public ThrowingReadOnlyList(bool throwCount)
            {
                this.throwCount = throwCount;
            }

            public int Count => throwCount
                ? throw new InvalidOperationException("Synthetic unreadable count.")
                : 1;

            public T this[int index] =>
                throw new InvalidOperationException("Synthetic unreadable item.");

            public IEnumerator<T> GetEnumerator()
            {
                throw new InvalidOperationException("Enumeration is not supported.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
