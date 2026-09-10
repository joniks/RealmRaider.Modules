using NUnit.Framework;

namespace RealmRaiders.Modules.CharacterArtManifests.Tests
{
    public sealed class GuardianEntProductionReadinessTests
    {
        [Test]
        public void Tree01_PilotPassesOnlyExplicitSafeExceptionAndProductionListsDeficits()
        {
            var source = Source(
                exception: true,
                albedoOnly: true,
                animation: false,
                physics: false,
                colliders: false);

            var pilot = GuardianEntProductionReadiness.Evaluate(
                GuardianEntReadinessTier.TemporaryVisualPilot,
                source);
            var production = GuardianEntProductionReadiness.Evaluate(
                GuardianEntReadinessTier.ProductionReady,
                source);

            Assert.That(pilot.IsReady, Is.True);
            Assert.That(production.Issues, Does.Contain(GuardianEntReadinessIssue.TriangleBudgetExceeded));
            Assert.That(production.Issues, Does.Contain(GuardianEntReadinessIssue.MissingLod1));
            Assert.That(production.Issues, Does.Contain(GuardianEntReadinessIssue.MissingLod2));
            Assert.That(production.Issues, Does.Contain(GuardianEntReadinessIssue.InfluenceBudgetExceeded));
            Assert.That(production.Issues, Does.Not.Contain(GuardianEntReadinessIssue.SampledBaseColorBudgetExceeded));
        }

        [Test]
        public void HashMismatchAndUnsafePilotFailClosed()
        {
            var source = new GuardianEntSourceMeasurement(
                "wrong",
                GuardianEntProductionReadiness.Tree01AlbedoHash,
                GuardianEntProductionReadiness.Tree01NormalHash,
                GuardianEntProductionReadiness.Tree01MaskHash,
                5438,
                -1,
                -1,
                2721,
                31,
                8,
                373,
                1,
                2048,
                1024,
                true,
                false,
                false,
                false,
                false);

            var result = GuardianEntProductionReadiness.Evaluate(
                GuardianEntReadinessTier.TemporaryVisualPilot,
                source);

            Assert.That(result.Issues, Does.Contain(GuardianEntReadinessIssue.SourceHashMismatch));
            Assert.That(result.Issues, Does.Contain(GuardianEntReadinessIssue.PilotExceptionRequired));
            Assert.That(result.Issues, Does.Contain(GuardianEntReadinessIssue.PilotMustBeAlbedoOnly));
            Assert.That(result.Issues, Does.Contain(GuardianEntReadinessIssue.PilotMustDisableAnimationPhysicsAndColliders));
        }

        [Test]
        public void DerivedProduction_RejectsPresentOverBudgetMeasurements()
        {
            var source = new GuardianEntSourceMeasurement(
                GuardianEntProductionReadiness.Tree01FbxHash,
                GuardianEntProductionReadiness.Tree01AlbedoHash,
                GuardianEntProductionReadiness.Tree01NormalHash,
                GuardianEntProductionReadiness.Tree01MaskHash,
                4001,
                2001,
                801,
                2721,
                49,
                5,
                1,
                2,
                2048,
                1025,
                false,
                false,
                false,
                true,
                true);

            var issues = GuardianEntProductionReadiness.Evaluate(
                GuardianEntReadinessTier.ProductionReady,
                source).Issues;

            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.TriangleBudgetExceeded));
            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.Lod1BudgetExceeded));
            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.Lod2BudgetExceeded));
            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.BoneBudgetExceeded));
            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.InfluenceBudgetExceeded));
            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.MaterialBudgetExceeded));
            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.SampledBaseColorBudgetExceeded));
        }

        [Test]
        public void DerivedProduction_PassesWithCompliantLodsAndSampledAlbedo()
        {
            var source = new GuardianEntSourceMeasurement(
                GuardianEntProductionReadiness.Tree01FbxHash,
                GuardianEntProductionReadiness.Tree01AlbedoHash,
                GuardianEntProductionReadiness.Tree01NormalHash,
                GuardianEntProductionReadiness.Tree01MaskHash,
                4000,
                2000,
                800,
                2721,
                48,
                4,
                0,
                1,
                2048,
                1024,
                false,
                false,
                false,
                true,
                true);

            var result = GuardianEntProductionReadiness.Evaluate(
                GuardianEntReadinessTier.ProductionReady,
                source);

            Assert.That(result.IsReady, Is.True);
        }

        [Test]
        public void DerivedProduction_ReportsEachAbsentLodWithoutTreatingSentinelAsInvalid()
        {
            var source = new GuardianEntSourceMeasurement(
                GuardianEntProductionReadiness.Tree01FbxHash,
                GuardianEntProductionReadiness.Tree01AlbedoHash,
                GuardianEntProductionReadiness.Tree01NormalHash,
                GuardianEntProductionReadiness.Tree01MaskHash,
                -1,
                -1,
                -1,
                2721,
                48,
                4,
                0,
                1,
                2048,
                1024,
                false,
                false,
                false,
                true,
                true);

            var issues = GuardianEntProductionReadiness.Evaluate(
                GuardianEntReadinessTier.ProductionReady,
                source).Issues;

            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.MissingLod0));
            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.MissingLod1));
            Assert.That(issues, Does.Contain(GuardianEntReadinessIssue.MissingLod2));
            Assert.That(issues, Does.Not.Contain(GuardianEntReadinessIssue.InvalidMeasurement));
        }

        [Test]
        public void InvalidMeasurementsFailClosedExceptAbsentLodSentinel()
        {
            var source = new GuardianEntSourceMeasurement(
                GuardianEntProductionReadiness.Tree01FbxHash,
                GuardianEntProductionReadiness.Tree01AlbedoHash,
                GuardianEntProductionReadiness.Tree01NormalHash,
                GuardianEntProductionReadiness.Tree01MaskHash,
                -2,
                -1,
                0,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                -1,
                false,
                false,
                false,
                true,
                true);

            var pilot = GuardianEntProductionReadiness.Evaluate(
                GuardianEntReadinessTier.TemporaryVisualPilot,
                source);
            var production = GuardianEntProductionReadiness.Evaluate(
                GuardianEntReadinessTier.ProductionReady,
                source);

            Assert.That(pilot.Issues, Does.Contain(GuardianEntReadinessIssue.InvalidMeasurement));
            Assert.That(production.Issues, Does.Contain(GuardianEntReadinessIssue.InvalidMeasurement));
            Assert.That(production.Issues, Does.Contain(GuardianEntReadinessIssue.MissingLod1));
        }

        [Test]
        public void PilotRejectsZeroMaterialMeasurement()
        {
            var source = new GuardianEntSourceMeasurement(
                GuardianEntProductionReadiness.Tree01FbxHash,
                GuardianEntProductionReadiness.Tree01AlbedoHash,
                GuardianEntProductionReadiness.Tree01NormalHash,
                GuardianEntProductionReadiness.Tree01MaskHash,
                5438,
                -1,
                -1,
                2721,
                31,
                8,
                373,
                0,
                2048,
                1024,
                false,
                false,
                false,
                true,
                true);

            var result = GuardianEntProductionReadiness.Evaluate(
                GuardianEntReadinessTier.TemporaryVisualPilot,
                source);

            Assert.That(result.Issues, Does.Contain(GuardianEntReadinessIssue.InvalidMeasurement));
            Assert.That(result.IsReady, Is.False);
        }

        private static GuardianEntSourceMeasurement Source(
            bool exception,
            bool albedoOnly,
            bool animation,
            bool physics,
            bool colliders)
        {
            return new GuardianEntSourceMeasurement(
                GuardianEntProductionReadiness.Tree01FbxHash,
                GuardianEntProductionReadiness.Tree01AlbedoHash,
                GuardianEntProductionReadiness.Tree01NormalHash,
                GuardianEntProductionReadiness.Tree01MaskHash,
                5438,
                -1,
                -1,
                2721,
                31,
                8,
                373,
                1,
                2048,
                1024,
                animation,
                physics,
                colliders,
                albedoOnly,
                exception);
        }
    }
}
