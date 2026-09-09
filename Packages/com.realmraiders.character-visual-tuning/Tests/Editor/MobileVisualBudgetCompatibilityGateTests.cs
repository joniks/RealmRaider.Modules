using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace RealmRaiders.CharacterVisualTuning.Tests
{
    public sealed class MobileVisualBudgetCompatibilityGateTests
    {
        [Test]
        public void CurrentBloodKnightStarterBudget_IsCompatibleWithExplicitPolicy()
        {
            var budget = BloodKnightBaseline.Create().Budget;
            var policy = new MobileVisualBudgetPolicy(1, 1, 1024, 2500);

            var result = MobileVisualBudgetCompatibilityGate.Evaluate(budget, policy);

            Assert.That(result.IsCompatible, Is.True);
            Assert.That(result.Issues, Is.Empty);
            Assert.That(budget.MaterialCount, Is.EqualTo(1));
            Assert.That(budget.TextureCount, Is.EqualTo(1));
            Assert.That(budget.MaxTextureEdgePixels, Is.EqualTo(1024));
            Assert.That(budget.TriangleCount, Is.EqualTo(2500));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<MobileVisualBudgetCompatibilityIssue>)result.Issues).Clear());
        }

        [TestCase(2, 1, 1024, 2500, MobileVisualBudgetCompatibilityIssueCode.MaterialCountExceedsPolicy)]
        [TestCase(1, 2, 1024, 2500, MobileVisualBudgetCompatibilityIssueCode.TextureCountExceedsPolicy)]
        [TestCase(1, 1, 2048, 2500, MobileVisualBudgetCompatibilityIssueCode.TextureEdgePixelsExceedsPolicy)]
        [TestCase(1, 1, 1024, 2501, MobileVisualBudgetCompatibilityIssueCode.TriangleCountExceedsPolicy)]
        public void EachOverBudgetDimension_ReturnsItsStableIssue(
            int materialCount,
            int textureCount,
            int maxTextureEdgePixels,
            int triangleCount,
            MobileVisualBudgetCompatibilityIssueCode expectedCode)
        {
            var result = MobileVisualBudgetCompatibilityGate.Evaluate(
                new MobileVisualBudget(materialCount, textureCount, maxTextureEdgePixels, triangleCount),
                new MobileVisualBudgetPolicy(1, 1, 1024, 2500));

            Assert.That(result.IsCompatible, Is.False);
            Assert.That(result.Issues, Has.Count.EqualTo(1));
            Assert.That(result.Issues[0].Code, Is.EqualTo(expectedCode));
        }

        [Test]
        public void MultipleExceededDimensions_AreReturnedInStableOrdinalOrder()
        {
            var budget = new MobileVisualBudget(2, 3, 2048, 5000);
            var policy = new MobileVisualBudgetPolicy(1, 1, 1024, 2500);

            var result = MobileVisualBudgetCompatibilityGate.Evaluate(budget, policy);

            Assert.That(result.IsCompatible, Is.False);
            Assert.That(result.Issues, Has.Count.EqualTo(4));
            AssertIssue(result, 0, MobileVisualBudgetCompatibilityIssueCode.MaterialCountExceedsPolicy, "Budget material count exceeds the supplied policy.");
            AssertIssue(result, 1, MobileVisualBudgetCompatibilityIssueCode.TextureCountExceedsPolicy, "Budget texture count exceeds the supplied policy.");
            AssertIssue(result, 2, MobileVisualBudgetCompatibilityIssueCode.TextureEdgePixelsExceedsPolicy, "Budget texture edge pixels exceed the supplied policy.");
            AssertIssue(result, 3, MobileVisualBudgetCompatibilityIssueCode.TriangleCountExceedsPolicy, "Budget triangle count exceeds the supplied policy.");
        }

        [Test]
        public void InvalidAndNullInput_ReturnsStableEvidenceWithoutMutatingProvidedBudget()
        {
            var invalidBudget = new MobileVisualBudget(0, -1, 0, -2);
            var invalidPolicy = new MobileVisualBudgetPolicy(0, -1, 0, -2);

            var invalidResult = MobileVisualBudgetCompatibilityGate.Evaluate(invalidBudget, invalidPolicy);
            var nullResult = MobileVisualBudgetCompatibilityGate.Evaluate(null, null);

            Assert.That(invalidResult.Issues, Has.Count.EqualTo(8));
            AssertIssue(invalidResult, 0, MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumMaterialCountMustBePositive, "Policy maximum material count must be positive.");
            AssertIssue(invalidResult, 1, MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumTextureCountMustBePositive, "Policy maximum texture count must be positive.");
            AssertIssue(invalidResult, 2, MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumTextureEdgePixelsMustBePositive, "Policy maximum texture edge pixels must be positive.");
            AssertIssue(invalidResult, 3, MobileVisualBudgetCompatibilityIssueCode.PolicyMaximumTriangleCountMustBePositive, "Policy maximum triangle count must be positive.");
            AssertIssue(invalidResult, 4, MobileVisualBudgetCompatibilityIssueCode.BudgetMaterialCountMustBePositive, "Budget material count must be positive.");
            AssertIssue(invalidResult, 5, MobileVisualBudgetCompatibilityIssueCode.BudgetTextureCountMustBePositive, "Budget texture count must be positive.");
            AssertIssue(invalidResult, 6, MobileVisualBudgetCompatibilityIssueCode.BudgetTextureEdgePixelsMustBePositive, "Budget texture edge pixels must be positive.");
            AssertIssue(invalidResult, 7, MobileVisualBudgetCompatibilityIssueCode.BudgetTriangleCountMustBePositive, "Budget triangle count must be positive.");
            Assert.That(invalidBudget.MaterialCount, Is.EqualTo(0));
            Assert.That(invalidBudget.TextureCount, Is.EqualTo(-1));
            Assert.That(invalidBudget.MaxTextureEdgePixels, Is.EqualTo(0));
            Assert.That(invalidBudget.TriangleCount, Is.EqualTo(-2));
            Assert.That(nullResult.Issues, Has.Count.EqualTo(2));
            AssertIssue(nullResult, 0, MobileVisualBudgetCompatibilityIssueCode.MissingBudget, "Mobile visual budget is required.");
            AssertIssue(nullResult, 1, MobileVisualBudgetCompatibilityIssueCode.MissingPolicy, "Mobile visual budget policy is required.");
        }

        private static void AssertIssue(
            MobileVisualBudgetCompatibilityResult result,
            int index,
            MobileVisualBudgetCompatibilityIssueCode expectedCode,
            string expectedMessage)
        {
            Assert.That(result.Issues[index].Code, Is.EqualTo(expectedCode));
            Assert.That(result.Issues[index].Message, Is.EqualTo(expectedMessage));
        }
    }
}
