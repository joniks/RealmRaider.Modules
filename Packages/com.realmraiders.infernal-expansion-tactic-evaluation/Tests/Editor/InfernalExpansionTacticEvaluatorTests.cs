using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalFirstExpansionTactics;

namespace RealmRaiders.Modules.InfernalExpansionTacticEvaluation.Tests
{
    public sealed class InfernalExpansionTacticEvaluatorTests
    {
        [Test]
        public void PackTrigger_TruthTableUsesRunSceneAndIssuedFactsExactly()
        {
            var recipe = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var cases = new[]
            {
                Case(false, false, false, InfernalExpansionTacticEvaluationStatus.Inactive),
                Case(false, false, true, InfernalExpansionTacticEvaluationStatus.Inactive),
                Case(false, true, false, InfernalExpansionTacticEvaluationStatus.Inactive),
                Case(false, true, true, InfernalExpansionTacticEvaluationStatus.Inactive),
                Case(true, true, false, InfernalExpansionTacticEvaluationStatus.AlreadyIssued),
                Case(true, true, true, InfernalExpansionTacticEvaluationStatus.AlreadyIssued),
                Case(true, false, false, InfernalExpansionTacticEvaluationStatus.Waiting),
                Case(true, false, true, InfernalExpansionTacticEvaluationStatus.Eligible)
            };

            for (var index = 0; index < cases.Length; index++)
            {
                var item = cases[index];
                var result = Evaluate(
                    recipe,
                    item.RunActive,
                    item.AlreadyIssued,
                    item.SceneStarted,
                    0,
                    0,
                    999f);
                Assert.That(result.Status, Is.EqualTo(item.Status), "case " + index);
            }
        }

        [Test]
        public void SnareTrigger_TruthTableUsesStrictPositiveUnconsumedRevision()
        {
            var recipe = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            AssertStatus(
                Evaluate(recipe, false, false, false, 1, 0, 999f),
                InfernalExpansionTacticEvaluationStatus.Inactive);
            AssertStatus(
                Evaluate(recipe, true, true, false, 1, 0, 999f),
                InfernalExpansionTacticEvaluationStatus.AlreadyIssued);
            AssertStatus(
                Evaluate(recipe, true, false, true, 0, 0, 999f),
                InfernalExpansionTacticEvaluationStatus.Waiting);
            AssertStatus(
                Evaluate(recipe, true, false, false, 1, 1, 999f),
                InfernalExpansionTacticEvaluationStatus.Waiting);
            AssertStatus(
                Evaluate(recipe, true, false, false, 1, 0, 999f),
                InfernalExpansionTacticEvaluationStatus.Eligible);
        }

        [Test]
        public void BruteTrigger_TruthTableUsesOnlyAcceptedRangeAfterLifecyclePrecedence()
        {
            var recipe = StarterInfernalFirstExpansionTactics.BruteKilnVent;
            AssertStatus(
                Evaluate(recipe, false, true, true, 0, 0, 0f),
                InfernalExpansionTacticEvaluationStatus.Inactive);
            AssertStatus(
                Evaluate(recipe, true, true, true, 0, 0, 0f),
                InfernalExpansionTacticEvaluationStatus.AlreadyIssued);
            AssertStatus(
                Evaluate(recipe, true, false, false, 0, 0, 6f),
                InfernalExpansionTacticEvaluationStatus.Eligible);
            AssertStatus(
                Evaluate(recipe, true, false, true, 0, 0, 8f),
                InfernalExpansionTacticEvaluationStatus.Waiting);
        }

        [Test]
        public void BruteBoundary_ExactlySevenMetresIsEligible()
        {
            var result = Evaluate(
                StarterInfernalFirstExpansionTactics.BruteKilnVent,
                true,
                false,
                false,
                0,
                0,
                7f);

            AssertStatus(result, InfernalExpansionTacticEvaluationStatus.Eligible);
        }

        [Test]
        public void BruteBoundary_JustAboveSevenMetresIsWaiting()
        {
            var result = Evaluate(
                StarterInfernalFirstExpansionTactics.BruteKilnVent,
                true,
                false,
                false,
                0,
                0,
                7.000001f);

            AssertStatus(result, InfernalExpansionTacticEvaluationStatus.Waiting);
        }

        [Test]
        public void DistanceEvidence_NaNInfinityAndNegativeFailClosedForEveryTrigger()
        {
            var invalidDistances = new[]
            {
                float.NaN,
                float.PositiveInfinity,
                float.NegativeInfinity,
                -0.001f
            };

            foreach (var recipe in StarterInfernalFirstExpansionTactics.All)
            {
                for (var index = 0; index < invalidDistances.Length; index++)
                {
                    var result = Evaluate(
                        recipe,
                        true,
                        false,
                        true,
                        1,
                        0,
                        invalidDistances[index]);
                    AssertStatus(result, InfernalExpansionTacticEvaluationStatus.Invalid);
                    Assert.That(
                        result.Issues,
                        Does.Contain(float.IsNaN(invalidDistances[index])
                            || float.IsInfinity(invalidDistances[index])
                                ? InfernalExpansionTacticEvaluationIssue
                                    .InvaderDistanceNotFinite
                                : InfernalExpansionTacticEvaluationIssue
                                    .InvaderDistanceNegative));
                }
            }
        }

        [Test]
        public void SnareRevision_EqualityWaits()
        {
            var result = Evaluate(
                StarterInfernalFirstExpansionTactics.CinderSnareVent,
                true,
                false,
                false,
                8,
                8,
                0f);

            AssertStatus(result, InfernalExpansionTacticEvaluationStatus.Waiting);
        }

        [Test]
        public void SnareRevision_StrictIncreaseIsEligible()
        {
            var result = Evaluate(
                StarterInfernalFirstExpansionTactics.CinderSnareVent,
                true,
                false,
                false,
                9,
                8,
                0f);

            AssertStatus(result, InfernalExpansionTacticEvaluationStatus.Eligible);
        }

        [Test]
        public void NegativeRevisions_FailClosedEvenForPackAndBrute()
        {
            var pack = Evaluate(
                StarterInfernalFirstExpansionTactics.AshenPackVent,
                true,
                false,
                true,
                -1,
                0,
                0f);
            var brute = Evaluate(
                StarterInfernalFirstExpansionTactics.BruteKilnVent,
                true,
                false,
                true,
                0,
                -1,
                0f);

            AssertIssue(
                pack,
                InfernalExpansionTacticEvaluationIssue.FlameActivationRevisionNegative);
            AssertIssue(
                brute,
                InfernalExpansionTacticEvaluationIssue.LastHandledFlameRevisionNegative);
        }

        [Test]
        public void HandledRevisionAboveActivation_FailsClosed()
        {
            var result = Evaluate(
                StarterInfernalFirstExpansionTactics.CinderSnareVent,
                true,
                false,
                false,
                3,
                4,
                0f);

            AssertIssue(
                result,
                InfernalExpansionTacticEvaluationIssue.HandledRevisionExceedsActivation);
        }

        [Test]
        public void Inactive_PrecedesAlreadyIssuedAndReadyTrigger()
        {
            var result = Evaluate(
                StarterInfernalFirstExpansionTactics.AshenPackVent,
                false,
                true,
                true,
                0,
                0,
                0f);

            AssertStatus(result, InfernalExpansionTacticEvaluationStatus.Inactive);
        }

        [Test]
        public void AlreadyIssued_PrecedesTriggerReadiness()
        {
            var result = Evaluate(
                StarterInfernalFirstExpansionTactics.BruteKilnVent,
                true,
                true,
                false,
                0,
                0,
                999f);

            AssertStatus(result, InfernalExpansionTacticEvaluationStatus.AlreadyIssued);
        }

        [Test]
        public void InvalidEvidence_PrecedesInactiveAndAlreadyIssued()
        {
            var result = Evaluate(
                StarterInfernalFirstExpansionTactics.AshenPackVent,
                false,
                true,
                true,
                0,
                0,
                float.NaN);

            AssertIssue(
                result,
                InfernalExpansionTacticEvaluationIssue.InvaderDistanceNotFinite);
        }

        [Test]
        public void NullRequestAndNullRecipe_FailClosedWithoutThrowing()
        {
            var missingRequest = InfernalExpansionTacticEvaluator.Evaluate(null);
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var missingRecipe = InfernalExpansionTacticEvaluator.Evaluate(
                new InfernalExpansionTacticEvaluationRequest(
                    pack.LayoutId,
                    pack.SocketId,
                    null,
                    true,
                    false,
                    true,
                    0,
                    0,
                    0f));

            AssertIssue(
                missingRequest,
                InfernalExpansionTacticEvaluationIssue.RequestMissing);
            AssertIssue(
                missingRecipe,
                InfernalExpansionTacticEvaluationIssue.RecipeMissing);
        }

        [Test]
        public void CopiedExactRecipe_FailsCachedReferenceIdentity()
        {
            var source = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var copy = Copy(source, source.Trigger, source.Actors);
            var result = Evaluate(copy, true, false, true, 0, 0, 0f);

            AssertIssue(
                result,
                InfernalExpansionTacticEvaluationIssue.RecipeReferenceMismatch);
            Assert.That(result.TacticValidationIssues, Is.Empty);
        }

        [Test]
        public void ForeignCachedRecipe_FailsRequestedBindingAndReference()
        {
            var requested = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var foreign = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            var result = InfernalExpansionTacticEvaluator.Evaluate(
                new InfernalExpansionTacticEvaluationRequest(
                    requested.LayoutId,
                    requested.SocketId,
                    foreign,
                    true,
                    false,
                    true,
                    1,
                    0,
                    0f));

            AssertIssue(
                result,
                InfernalExpansionTacticEvaluationIssue.RecipeBindingMismatch);
            Assert.That(
                result.Issues,
                Does.Contain(
                    InfernalExpansionTacticEvaluationIssue.RecipeReferenceMismatch));
        }

        [Test]
        public void LaterSocket_FailsClosedBeforeLifecycleState()
        {
            var recipe = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var result = InfernalExpansionTacticEvaluator.Evaluate(
                new InfernalExpansionTacticEvaluationRequest(
                    recipe.LayoutId,
                    "ashen.east-vent",
                    recipe,
                    false,
                    true,
                    true,
                    0,
                    0,
                    0f));

            AssertIssue(
                result,
                InfernalExpansionTacticEvaluationIssue.SocketNotFirstAuthored);
            Assert.That(
                result.Issues,
                Does.Contain(
                    InfernalExpansionTacticEvaluationIssue.RecipeBindingMismatch));
        }

        [Test]
        public void InvalidTriggerEnum_PreservesMgc33ValidationEvidence()
        {
            var source = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var changed = Copy(source, (TacticTrigger)99, source.Actors);
            var result = Evaluate(changed, true, false, true, 0, 0, 0f);

            AssertIssue(
                result,
                InfernalExpansionTacticEvaluationIssue.RecipeValidationInvalid);
            Assert.That(
                result.TacticValidationIssues,
                Does.Contain(TacticValidationIssue.TriggerInvalid));
        }

        [Test]
        public void InvalidActorContent_PreservesMgc33ValidationEvidence()
        {
            var source = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var actors = new[]
            {
                new TacticActor(
                    0,
                    "realmraiders.infernal-brute",
                    TacticRole.LeftFlank,
                    0f),
                source.Actors[1]
            };
            var changed = Copy(source, source.Trigger, actors);
            var result = Evaluate(changed, true, false, true, 0, 0, 0f);

            AssertIssue(
                result,
                InfernalExpansionTacticEvaluationIssue.RecipeValidationInvalid);
            Assert.That(
                result.TacticValidationIssues,
                Does.Contain(TacticValidationIssue.ActorContentMismatch));
        }

        [Test]
        public void EligibleOutput_ReturnsExactRecipeAndActorReferenceSnapshotInOrder()
        {
            var recipe = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var result = Evaluate(recipe, true, false, true, 0, 0, 500f);

            Assert.That(result.Recipe, Is.SameAs(recipe));
            Assert.That(result.Actors, Is.Not.SameAs(recipe.Actors));
            Assert.That(result.Actors.Count, Is.EqualTo(2));
            Assert.That(result.Actors[0], Is.SameAs(recipe.Actors[0]));
            Assert.That(result.Actors[1], Is.SameAs(recipe.Actors[1]));
            Assert.That(result.Actors[0].ContentId, Is.EqualTo("realmraiders.hellhound"));
            Assert.That(result.Actors[0].Role, Is.EqualTo(TacticRole.LeftFlank));
            Assert.That(result.Actors[1].Role, Is.EqualTo(TacticRole.RightFlank));
            Assert.Throws<NotSupportedException>(
                () => ((IList<TacticActor>)result.Actors).Add(recipe.Actors[0]));
        }

        [Test]
        public void NonEligibleOutputs_NeverExposeRecipeOrActors()
        {
            var recipe = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var statuses = new[]
            {
                Evaluate(recipe, false, false, true, 0, 0, 0f),
                Evaluate(recipe, true, false, false, 0, 0, 0f),
                Evaluate(recipe, true, true, true, 0, 0, 0f),
                Evaluate(recipe, true, false, true, 0, 0, float.NaN)
            };

            for (var index = 0; index < statuses.Length; index++)
            {
                Assert.That(statuses[index].Recipe, Is.Null);
                Assert.That(statuses[index].Actors, Is.Empty);
            }
        }

        [Test]
        public void Evaluation_IsDeterministicAndDoesNotChangeCallerEvidence()
        {
            var recipe = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            var request = Request(recipe, true, false, false, 12, 11, 321f);
            var first = InfernalExpansionTacticEvaluator.Evaluate(request);
            var second = InfernalExpansionTacticEvaluator.Evaluate(request);

            Assert.That(second.Status, Is.EqualTo(first.Status));
            Assert.That(second.Recipe, Is.SameAs(first.Recipe));
            Assert.That(second.Issues, Is.EqualTo(first.Issues));
            Assert.That(
                second.TacticValidationIssues,
                Is.EqualTo(first.TacticValidationIssues));
            Assert.That(request.FlameActivationRevision, Is.EqualTo(12));
            Assert.That(request.LastHandledFlameRevision, Is.EqualTo(11));
            Assert.That(request.AlreadyIssued, Is.False);
        }

        [Test]
        public void Evaluation_IsCultureInvariant()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                var recipe = StarterInfernalFirstExpansionTactics.BruteKilnVent;
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");
                var turkish = Evaluate(recipe, true, false, false, 0, 0, 7f);
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");
                var french = Evaluate(recipe, true, false, false, 0, 0, 7f);

                Assert.That(french.Status, Is.EqualTo(turkish.Status));
                Assert.That(french.Recipe, Is.SameAs(turkish.Recipe));
                Assert.That(french.Actors[0], Is.SameAs(turkish.Actors[0]));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Test]
        public void PublicEvidenceTypes_AreImmutableAndStatusEnumIsExact()
        {
            Assert.That(
                Enum.GetNames(typeof(InfernalExpansionTacticEvaluationStatus)),
                Is.EqualTo(new[]
                {
                    "Invalid",
                    "Inactive",
                    "Waiting",
                    "AlreadyIssued",
                    "Eligible"
                }));
            Assert.That(
                typeof(InfernalExpansionTacticEvaluationRequest).IsSealed,
                Is.True);
            Assert.That(
                typeof(InfernalExpansionTacticEvaluationResult).IsSealed,
                Is.True);
            Assert.That(
                typeof(InfernalExpansionTacticEvaluator).IsAbstract
                    && typeof(InfernalExpansionTacticEvaluator).IsSealed,
                Is.True);

            AssertReadOnlyProperties(typeof(InfernalExpansionTacticEvaluationRequest));
            AssertReadOnlyProperties(typeof(InfernalExpansionTacticEvaluationResult));
        }

        private static InfernalExpansionTacticEvaluationRequest Request(
            TacticRecipe recipe,
            bool runActive,
            bool alreadyIssued,
            bool sceneStarted,
            int flameActivationRevision,
            int lastHandledFlameRevision,
            float invaderDistanceMetres)
        {
            return new InfernalExpansionTacticEvaluationRequest(
                recipe.LayoutId,
                recipe.SocketId,
                recipe,
                runActive,
                alreadyIssued,
                sceneStarted,
                flameActivationRevision,
                lastHandledFlameRevision,
                invaderDistanceMetres);
        }

        private static InfernalExpansionTacticEvaluationResult Evaluate(
            TacticRecipe recipe,
            bool runActive,
            bool alreadyIssued,
            bool sceneStarted,
            int flameActivationRevision,
            int lastHandledFlameRevision,
            float invaderDistanceMetres)
        {
            return InfernalExpansionTacticEvaluator.Evaluate(
                Request(
                    recipe,
                    runActive,
                    alreadyIssued,
                    sceneStarted,
                    flameActivationRevision,
                    lastHandledFlameRevision,
                    invaderDistanceMetres));
        }

        private static TacticRecipe Copy(
            TacticRecipe source,
            TacticTrigger trigger,
            IReadOnlyList<TacticActor> actors)
        {
            return new TacticRecipe(
                source.LayoutId,
                source.SocketId,
                source.SourceNodeId,
                source.SiteId,
                source.TacticId,
                trigger,
                source.Cue,
                actors,
                source.PreferredAbilityDisplayName,
                source.TriggerRadiusMetres,
                source.Hazard);
        }

        private static ExpectedCase Case(
            bool runActive,
            bool alreadyIssued,
            bool sceneStarted,
            InfernalExpansionTacticEvaluationStatus status)
        {
            return new ExpectedCase(runActive, alreadyIssued, sceneStarted, status);
        }

        private static void AssertIssue(
            InfernalExpansionTacticEvaluationResult result,
            InfernalExpansionTacticEvaluationIssue issue)
        {
            AssertStatus(result, InfernalExpansionTacticEvaluationStatus.Invalid);
            Assert.That(result.Issues, Does.Contain(issue));
            Assert.That(result.Recipe, Is.Null);
            Assert.That(result.Actors, Is.Empty);
        }

        private static void AssertStatus(
            InfernalExpansionTacticEvaluationResult result,
            InfernalExpansionTacticEvaluationStatus status)
        {
            Assert.That(result.Status, Is.EqualTo(status));
            Assert.That(result.IsEligible, Is.EqualTo(
                status == InfernalExpansionTacticEvaluationStatus.Eligible));
        }

        private static void AssertReadOnlyProperties(Type type)
        {
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                Assert.That(property.CanWrite, Is.False, type.Name + "." + property.Name);
            }
        }

        private sealed class ExpectedCase
        {
            public ExpectedCase(
                bool runActive,
                bool alreadyIssued,
                bool sceneStarted,
                InfernalExpansionTacticEvaluationStatus status)
            {
                RunActive = runActive;
                AlreadyIssued = alreadyIssued;
                SceneStarted = sceneStarted;
                Status = status;
            }

            public bool RunActive { get; }
            public bool AlreadyIssued { get; }
            public bool SceneStarted { get; }
            public InfernalExpansionTacticEvaluationStatus Status { get; }
        }
    }
}
