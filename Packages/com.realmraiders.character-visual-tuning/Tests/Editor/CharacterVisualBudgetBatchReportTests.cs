using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace RealmRaiders.CharacterVisualTuning.Tests
{
    public sealed class CharacterVisualBudgetBatchReportTests
    {
        [Test]
        public void Create_OrdersRowsByProfileIdRegardlessOfSourceInsertionOrder()
        {
            var firstCatalogue = CreateCatalogue(
                CreateProfile("zeta", 1, 1, 512, 100),
                CreateProfile("alpha", 1, 1, 512, 100),
                CreateProfile("middle", 1, 1, 512, 100));
            var secondCatalogue = CreateCatalogue(
                CreateProfile("middle", 1, 1, 512, 100),
                CreateProfile("zeta", 1, 1, 512, 100),
                CreateProfile("alpha", 1, 1, 512, 100));
            var policy = new MobileVisualBudgetPolicy(1, 1, 512, 100);

            var first = CharacterVisualBudgetBatchReport.Create(firstCatalogue, policy);
            var second = CharacterVisualBudgetBatchReport.Create(secondCatalogue, policy);

            Assert.That(first.Rows.Select(row => row.ProfileId), Is.EqualTo(new[]
            {
                "test.profile.alpha",
                "test.profile.middle",
                "test.profile.zeta"
            }));
            Assert.That(
                first.Rows.Select(row => row.ProfileId),
                Is.EqualTo(second.Rows.Select(row => row.ProfileId)));
        }

        [Test]
        public void Create_ReturnsMixedRowsWithExactAggregateCounts()
        {
            var accepted = CreateProfile("accepted", 1, 1, 512, 100);
            var rejected = CreateProfile("rejected", 2, 1, 512, 100);
            var report = CharacterVisualBudgetBatchReport.Create(
                CreateCatalogue(rejected, accepted),
                new MobileVisualBudgetPolicy(1, 1, 512, 100));

            Assert.That(report.Issues, Is.Empty);
            Assert.That(report.TotalProfileCount, Is.EqualTo(2));
            Assert.That(report.CompatibleProfileCount, Is.EqualTo(1));
            Assert.That(report.IncompatibleProfileCount, Is.EqualTo(1));
            Assert.That(report.Rows[0].ProfileId, Is.EqualTo("test.profile.accepted"));
            Assert.That(report.Rows[0].Compatibility.IsCompatible, Is.True);
            Assert.That(report.Rows[1].ProfileId, Is.EqualTo("test.profile.rejected"));
            Assert.That(report.Rows[1].Compatibility.IsCompatible, Is.False);
            Assert.That(
                report.Rows[1].Compatibility.Issues[0].Code,
                Is.EqualTo(MobileVisualBudgetCompatibilityIssueCode.MaterialCountExceedsPolicy));
        }

        [Test]
        public void MissingCatalogue_ReturnsStableReportEvidence()
        {
            var policy = new MobileVisualBudgetPolicy(1, 1, 512, 100);

            var report = CharacterVisualBudgetBatchReport.Create(null, policy);

            Assert.That(report.HasReportIssues, Is.True);
            Assert.That(report.Rows, Is.Empty);
            Assert.That(report.TotalProfileCount, Is.Zero);
            Assert.That(report.CompatibleProfileCount, Is.Zero);
            Assert.That(report.IncompatibleProfileCount, Is.Zero);
            Assert.That(report.Issues, Has.Count.EqualTo(1));
            Assert.That(report.Issues[0].Code, Is.EqualTo(CharacterVisualBudgetBatchReportIssueCode.MissingCatalogue));
            Assert.That(report.Issues[0].Message, Is.EqualTo("Character visual tuning catalogue is required."));
        }

        [Test]
        public void MissingPolicy_IsReportedThroughEveryCatalogueRow()
        {
            var report = CharacterVisualBudgetBatchReport.Create(
                CreateCatalogue(
                    CreateProfile("one", 1, 1, 512, 100),
                    CreateProfile("two", 1, 1, 512, 100)),
                null);

            Assert.That(report.Issues, Is.Empty);
            Assert.That(report.Policy, Is.Null);
            Assert.That(report.TotalProfileCount, Is.EqualTo(2));
            Assert.That(report.CompatibleProfileCount, Is.Zero);
            Assert.That(report.IncompatibleProfileCount, Is.EqualTo(2));
            Assert.That(
                report.Rows.Select(row => row.Compatibility.Issues[0].Code),
                Is.EqualTo(new[]
                {
                    MobileVisualBudgetCompatibilityIssueCode.MissingPolicy,
                    MobileVisualBudgetCompatibilityIssueCode.MissingPolicy
                }));
        }

        [Test]
        public void InvalidPolicy_IsReportedThroughEveryCatalogueRow()
        {
            var report = CharacterVisualBudgetBatchReport.Create(
                CreateCatalogue(
                    CreateProfile("one", 1, 1, 512, 100),
                    CreateProfile("two", 1, 1, 512, 100)),
                new MobileVisualBudgetPolicy(0, 1, 512, 100));

            Assert.That(report.Issues, Is.Empty);
            Assert.That(report.CompatibleProfileCount, Is.Zero);
            Assert.That(report.IncompatibleProfileCount, Is.EqualTo(2));
            Assert.That(
                report.Rows.Select(row => row.Compatibility.Issues[0].Code),
                Is.EqualTo(new[]
                {
                    MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumMaterialCountMustBePositive,
                    MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumMaterialCountMustBePositive
                }));
        }

        [Test]
        public void ReportAndRows_SnapshotImmutableInputAndOutput()
        {
            var profile = CreateProfile("snapshot", 1, 1, 512, 100);
            var mutableProfiles = new List<CharacterVisualTuningProfile> { profile };
            var catalogue = CreateCatalogue(mutableProfiles);
            var policy = new MobileVisualBudgetPolicy(1, 1, 512, 100);

            var report = CharacterVisualBudgetBatchReport.Create(catalogue, policy);
            mutableProfiles.Clear();

            Assert.That(report.Rows, Has.Count.EqualTo(1));
            Assert.That(report.Rows[0].Budget, Is.Not.SameAs(profile.Budget));
            Assert.That(report.Rows[0].Budget.MaterialCount, Is.EqualTo(profile.Budget.MaterialCount));
            Assert.That(report.Policy, Is.Not.SameAs(policy));
            Assert.That(report.Policy.MaximumTriangleCount, Is.EqualTo(policy.MaximumTriangleCount));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterVisualBudgetBatchRow>)report.Rows).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterVisualBudgetBatchReportIssue>)report.Issues).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<MobileVisualBudgetCompatibilityIssue>)report.Rows[0].Compatibility.Issues).Clear());
        }

        private static CharacterVisualTuningCatalogue CreateCatalogue(
            params CharacterVisualTuningProfile[] profiles)
        {
            return CreateCatalogue((IReadOnlyList<CharacterVisualTuningProfile>)profiles);
        }

        private static CharacterVisualTuningCatalogue CreateCatalogue(
            IReadOnlyList<CharacterVisualTuningProfile> profiles)
        {
            var result = CharacterVisualTuningCatalogue.Build(new ICharacterVisualTuningProfileProvider[]
            {
                new TestProvider("test.module.batch-report", profiles)
            });
            Assert.That(result.Succeeded, Is.True);
            return result.Catalogue;
        }

        private static CharacterVisualTuningProfile CreateProfile(
            string suffix,
            int materialCount,
            int textureCount,
            int maxTextureEdgePixels,
            int triangleCount)
        {
            return new CharacterVisualTuningProfile(
                "test.profile." + suffix,
                "test.source." + suffix,
                "Test " + suffix,
                new PresentationTransformIntent(),
                new[]
                {
                    new MaterialPaletteIntent(MaterialRole.PrimaryArmor, "Test direction")
                },
                new MobileVisualBudget(
                    materialCount,
                    textureCount,
                    maxTextureEdgePixels,
                    triangleCount));
        }

        private sealed class TestProvider : ICharacterVisualTuningProfileProvider
        {
            public TestProvider(
                string moduleId,
                IReadOnlyList<CharacterVisualTuningProfile> profiles)
            {
                ModuleId = moduleId;
                Profiles = profiles;
            }

            public string ModuleId { get; }
            public IReadOnlyList<CharacterVisualTuningProfile> Profiles { get; }
        }
    }
}
