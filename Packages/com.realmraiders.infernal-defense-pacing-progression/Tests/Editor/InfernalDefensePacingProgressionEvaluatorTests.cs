using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefensePacing;

namespace RealmRaiders.Modules.InfernalDefensePacingProgression.Tests
{
    public sealed class InfernalDefensePacingProgressionEvaluatorTests
    {
        [Test]
        public void CachedRecipes_EvaluateEveryRequiredPrefixWithOptionalPendingOrSkipped()
        {
            var evaluatedStates = 0;
            foreach (var recipe in StarterInfernalDefensePacing.All)
            {
                var requiredBeatIds = BeatIds(recipe, InfernalDefensePacingRequirement.Required);
                var optionalBeatIds = BeatIds(recipe, InfernalDefensePacingRequirement.Optional);
                Assert.That(optionalBeatIds.Count, Is.EqualTo(1), recipe.LayoutId);

                for (var prefixLength = 0;
                     prefixLength <= requiredBeatIds.Count;
                     prefixLength++)
                {
                    var completed = Prefix(requiredBeatIds, prefixLength);
                    AssertExactProgression(
                        recipe,
                        completed,
                        Array.Empty<string>());
                    AssertExactProgression(
                        recipe,
                        completed,
                        optionalBeatIds);
                    evaluatedStates += 2;
                }
            }

            Assert.That(evaluatedStates, Is.EqualTo(30));
        }

        [Test]
        public void CachedRecipes_EvaluateEveryValidOptionalCompletionWithEveryRequiredPrefix()
        {
            var evaluatedStates = 0;
            foreach (var recipe in StarterInfernalDefensePacing.All)
            {
                var requiredBeatIds = BeatIds(recipe, InfernalDefensePacingRequirement.Required);
                var optionalBeat = FindBeat(
                    recipe,
                    InfernalDefensePacingRequirement.Optional);

                for (var prefixLength = 0;
                     prefixLength <= requiredBeatIds.Count;
                     prefixLength++)
                {
                    var completed = Prefix(requiredBeatIds, prefixLength);
                    if (!AllContained(optionalBeat.DependsOnBeatIds, completed))
                    {
                        continue;
                    }

                    var completedWithOptional = Append(completed, optionalBeat.BeatId);
                    AssertExactProgression(
                        recipe,
                        completedWithOptional,
                        Array.Empty<string>());
                    evaluatedStates++;
                }
            }

            Assert.That(evaluatedStates, Is.EqualTo(11));
        }

        [Test]
        public void OptionalFlameSkip_NeverBlocksBruteOrHeart()
        {
            AssertOptionalFlameSkip(
                StarterInfernalDefensePacing.CinderFork,
                "cinder-fork.entry",
                "cinder-fork.hellhound-pressure",
                "cinder-fork.flame-choice",
                "cinder-fork.brute-window",
                "cinder-fork.heart-objective");
            AssertOptionalFlameSkip(
                StarterInfernalDefensePacing.EmberCircuit,
                "ember-circuit.entry",
                "ember-circuit.hellhound-pressure",
                "ember-circuit.flame-choice",
                "ember-circuit.brute-window",
                "ember-circuit.heart-objective");
        }

        [Test]
        public void ResultAndEvidence_AreFieldForFieldSnapshotsInAuthoredOrder()
        {
            var recipe = StarterInfernalDefensePacing.CinderFork;
            var result = InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                new[]
                {
                    "cinder-fork.hellhound-pressure",
                    "cinder-fork.entry"
                },
                new[]
                {
                    "cinder-fork.flame-choice"
                });

            Assert.That(result.HasProgression, Is.True);
            Assert.That(result.Status,
                Is.EqualTo(InfernalDefensePacingProgressionStatus.Evaluated));
            Assert.That(result.Progression.LayoutId, Is.EqualTo(recipe.LayoutId));
            AssertEvidenceIds(
                result.Progression.Completed,
                "cinder-fork.entry",
                "cinder-fork.hellhound-pressure");
            AssertEvidenceIds(
                result.Progression.SkippedOptional,
                "cinder-fork.flame-choice");
            AssertEvidenceIds(
                result.Progression.Eligible,
                "cinder-fork.brute-window");
            AssertEvidenceIds(
                result.Progression.Blocked,
                "cinder-fork.heart-objective");

            foreach (var category in Categories(result.Progression))
            {
                foreach (var evidence in category)
                {
                    var beat = FindBeat(recipe, evidence.BeatId);
                    Assert.That(evidence.RoleId, Is.EqualTo(beat.RoleId));
                    Assert.That(evidence.Kind, Is.EqualTo(beat.Kind));
                    Assert.That(evidence.Requirement, Is.EqualTo(beat.Requirement));
                    CollectionAssert.AreEqual(
                        beat.DependsOnBeatIds,
                        evidence.DependsOnBeatIds);
                }
            }

            CollectionAssert.AreEqual(
                new[] { "cinder-fork.brute-window" },
                result.Progression.Blocked[0].BlockingBeatIds);
        }

        [Test]
        public void ExactCachedRecipeIdentity_IsRequiredEvenForValidCopies()
        {
            foreach (var recipe in StarterInfernalDefensePacing.All)
            {
                var copy = Copy(recipe, recipe.LayoutId, recipe.Beats);
                var result = InfernalDefensePacingProgressionEvaluator.Evaluate(
                    copy,
                    Array.Empty<string>(),
                    Array.Empty<string>());

                AssertRejected(
                    result,
                    InfernalDefensePacingProgressionIssue.RecipeIdentityMismatch);
                Assert.That(result.RecipeValidation.IsValid, Is.True);
            }
        }

        [Test]
        public void RecipeValidation_FailsClosedForMissingMalformedUnknownAndWrongIdentity()
        {
            var source = StarterInfernalDefensePacing.AshenSpur;
            var malformedId = Copy(source, "Ashen Spur", source.Beats);
            var unknownId = Copy(source, "realmraiders.infernal-defense.unknown", source.Beats);
            var malformedDependency = Copy(
                source,
                source.LayoutId,
                ReplaceBeat(
                    source.Beats,
                    2,
                    CopyBeat(
                        source.Beats[2],
                        new[] { "missing.dependency" })));
            var validWrongLayoutRhythm = Copy(
                StarterInfernalDefensePacing.CinderFork,
                StarterInfernalDefensePacing.EmberCircuit.LayoutId,
                StarterInfernalDefensePacing.CinderFork.Beats);

            AssertRejected(
                InfernalDefensePacingProgressionEvaluator.Evaluate(
                    null,
                    Array.Empty<string>(),
                    Array.Empty<string>()),
                InfernalDefensePacingProgressionIssue.RecipeMissing);
            AssertRejected(
                InfernalDefensePacingProgressionEvaluator.Evaluate(
                    malformedId,
                    Array.Empty<string>(),
                    Array.Empty<string>()),
                InfernalDefensePacingProgressionIssue.LayoutIdInvalid,
                InfernalDefensePacingProgressionIssue.RecipeInvalid);
            AssertRejected(
                InfernalDefensePacingProgressionEvaluator.Evaluate(
                    unknownId,
                    Array.Empty<string>(),
                    Array.Empty<string>()),
                InfernalDefensePacingProgressionIssue.LayoutNotFound,
                InfernalDefensePacingProgressionIssue.RecipeInvalid);
            AssertRejected(
                InfernalDefensePacingProgressionEvaluator.Evaluate(
                    malformedDependency,
                    Array.Empty<string>(),
                    Array.Empty<string>()),
                InfernalDefensePacingProgressionIssue.RecipeIdentityMismatch,
                InfernalDefensePacingProgressionIssue.RecipeInvalid);
            AssertRejected(
                InfernalDefensePacingProgressionEvaluator.Evaluate(
                    validWrongLayoutRhythm,
                    Array.Empty<string>(),
                    Array.Empty<string>()),
                InfernalDefensePacingProgressionIssue.RecipeIdentityMismatch);
            Assert.That(
                InfernalDefensePacingValidator.Validate(validWrongLayoutRhythm).IsValid,
                Is.True,
                "This fixture must prove identity, not merely structural validation.");
        }

        [Test]
        public void CallerCollections_AreRequired()
        {
            var recipe = StarterInfernalDefensePacing.AshenSpur;

            AssertRejected(
                InfernalDefensePacingProgressionEvaluator.Evaluate(
                    recipe,
                    null,
                    Array.Empty<string>()),
                InfernalDefensePacingProgressionIssue.CompletedBeatIdsMissing);
            AssertRejected(
                InfernalDefensePacingProgressionEvaluator.Evaluate(
                    recipe,
                    Array.Empty<string>(),
                    null),
                InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdsMissing);
        }

        [Test]
        public void CompletedIds_FailClosedForNullMalformedUnknownAndDuplicateEntries()
        {
            var recipe = StarterInfernalDefensePacing.AshenSpur;

            AssertRejected(
                EvaluateCompleted(recipe, new string[] { null }),
                InfernalDefensePacingProgressionIssue.CompletedBeatIdMissing);
            AssertRejected(
                EvaluateCompleted(recipe, new[] { "Bad id" }),
                InfernalDefensePacingProgressionIssue.CompletedBeatIdInvalid);
            AssertRejected(
                EvaluateCompleted(recipe, new[] { "unknown.beat" }),
                InfernalDefensePacingProgressionIssue.CompletedBeatIdUnknown);
            AssertRejected(
                EvaluateCompleted(
                    recipe,
                    new[] { "ashen-spur.entry", "ashen-spur.entry" }),
                InfernalDefensePacingProgressionIssue.CompletedBeatIdDuplicate);
        }

        [Test]
        public void SkippedOptionalIds_FailClosedForNullMalformedUnknownAndDuplicateEntries()
        {
            var recipe = StarterInfernalDefensePacing.AshenSpur;

            AssertRejected(
                EvaluateSkipped(recipe, new string[] { null }),
                InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdMissing);
            AssertRejected(
                EvaluateSkipped(recipe, new[] { "Bad id" }),
                InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdInvalid);
            AssertRejected(
                EvaluateSkipped(recipe, new[] { "unknown.beat" }),
                InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdUnknown);
            AssertRejected(
                EvaluateSkipped(
                    recipe,
                    new[]
                    {
                        "ashen-spur.hellhound-spur",
                        "ashen-spur.hellhound-spur"
                    }),
                InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdDuplicate);
        }

        [Test]
        public void State_FailsClosedForConflictRequiredSkipAndImpossibleCompletions()
        {
            var recipe = StarterInfernalDefensePacing.CinderFork;

            AssertRejected(
                InfernalDefensePacingProgressionEvaluator.Evaluate(
                    recipe,
                    new[]
                    {
                        "cinder-fork.entry",
                        "cinder-fork.flame-choice"
                    },
                    new[] { "cinder-fork.flame-choice" }),
                InfernalDefensePacingProgressionIssue.BeatStateConflict);
            AssertRejected(
                EvaluateSkipped(recipe, new[] { "cinder-fork.entry" }),
                InfernalDefensePacingProgressionIssue.RequiredBeatSkipped);
            AssertRejected(
                EvaluateCompleted(recipe, new[] { "cinder-fork.brute-window" }),
                InfernalDefensePacingProgressionIssue.RequiredCompletionPrefixInvalid,
                InfernalDefensePacingProgressionIssue.CompletedBeatDependencyUnsatisfied);
            AssertRejected(
                EvaluateCompleted(recipe, new[] { "cinder-fork.flame-choice" }),
                InfernalDefensePacingProgressionIssue.CompletedBeatDependencyUnsatisfied);
        }

        [Test]
        public void IssueEvidence_HasStableOrderAndNoPartialProgression()
        {
            var recipe = StarterInfernalDefensePacing.CinderFork;
            var result = InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                new string[]
                {
                    null,
                    "Bad id",
                    "unknown.beat",
                    "cinder-fork.entry",
                    "cinder-fork.entry"
                },
                new string[]
                {
                    null,
                    "Bad id",
                    "unknown.beat",
                    "cinder-fork.flame-choice",
                    "cinder-fork.flame-choice"
                });

            AssertRejected(
                result,
                InfernalDefensePacingProgressionIssue.CompletedBeatIdMissing,
                InfernalDefensePacingProgressionIssue.CompletedBeatIdInvalid,
                InfernalDefensePacingProgressionIssue.CompletedBeatIdUnknown,
                InfernalDefensePacingProgressionIssue.CompletedBeatIdDuplicate,
                InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdMissing,
                InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdInvalid,
                InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdUnknown,
                InfernalDefensePacingProgressionIssue.SkippedOptionalBeatIdDuplicate,
                InfernalDefensePacingProgressionIssue.BeatStateConflict);
            Assert.That(result.Progression, Is.Null);
            Assert.That(result.HasProgression, Is.False);
        }

        [Test]
        public void ResultSnapshots_AreImmutableAndDetachedFromCallerCollections()
        {
            var recipe = StarterInfernalDefensePacing.EmberCircuit;
            var completed = new List<string> { "ember-circuit.entry" };
            var skipped = new List<string> { "ember-circuit.flame-choice" };
            var result = InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                completed,
                skipped);

            completed.Clear();
            skipped.Clear();

            AssertEvidenceIds(result.Progression.Completed, "ember-circuit.entry");
            AssertEvidenceIds(
                result.Progression.SkippedOptional,
                "ember-circuit.flame-choice");
            Assert.Throws<NotSupportedException>(() =>
                ((IList)result.Progression.Completed).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)result.Progression.Eligible).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)result.Progression.Blocked).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)result.Progression.Completed[0].DependsOnBeatIds).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList)result.Progression.Blocked[0].BlockingBeatIds).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList)result.Issues).Clear());
        }

        [Test]
        public void Evaluation_IsDeterministicAndDoesNotMutateCachedRecipes()
        {
            foreach (var recipe in StarterInfernalDefensePacing.All)
            {
                var beatIdsBefore = AllBeatIds(recipe);
                var first = InfernalDefensePacingProgressionEvaluator.Evaluate(
                    recipe,
                    new[] { recipe.Beats[0].BeatId },
                    Array.Empty<string>());
                var second = InfernalDefensePacingProgressionEvaluator.Evaluate(
                    recipe,
                    new[] { recipe.Beats[0].BeatId },
                    Array.Empty<string>());

                Assert.That(first.HasProgression, Is.True);
                Assert.That(second.HasProgression, Is.True);
                AssertEvidenceIds(
                    first.Progression.Completed,
                    EvidenceIds(second.Progression.Completed));
                AssertEvidenceIds(
                    first.Progression.Eligible,
                    EvidenceIds(second.Progression.Eligible));
                AssertEvidenceIds(
                    first.Progression.Blocked,
                    EvidenceIds(second.Progression.Blocked));
                CollectionAssert.AreEqual(beatIdsBefore, AllBeatIds(recipe));
                Assert.That(
                    StarterInfernalDefensePacingResolver.ResolveExact(recipe.LayoutId).Recipe,
                    Is.SameAs(recipe));
            }
        }

        private static void AssertOptionalFlameSkip(
            InfernalDefensePacingRecipe recipe,
            string entryId,
            string hellhoundId,
            string flameId,
            string bruteId,
            string heartId)
        {
            var atBrute = InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                new[] { entryId, hellhoundId },
                new[] { flameId });
            var atHeart = InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                new[] { entryId, hellhoundId, bruteId },
                new[] { flameId });

            Assert.That(atBrute.HasProgression, Is.True);
            AssertEvidenceIds(atBrute.Progression.Eligible, bruteId);
            Assert.That(atBrute.Progression.Eligible[0].BlockingBeatIds, Is.Empty);
            Assert.That(atHeart.HasProgression, Is.True);
            AssertEvidenceIds(atHeart.Progression.Eligible, heartId);
            Assert.That(atHeart.Progression.Eligible[0].BlockingBeatIds, Is.Empty);
        }

        private static void AssertExactProgression(
            InfernalDefensePacingRecipe recipe,
            IReadOnlyList<string> completed,
            IReadOnlyList<string> skipped)
        {
            var result = InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                completed,
                skipped);
            Assert.That(result.HasProgression, Is.True, recipe.LayoutId);
            Assert.That(result.Issues, Is.Empty, recipe.LayoutId);
            Assert.That(result.RecipeValidation.IsValid, Is.True, recipe.LayoutId);

            var completedSet = new HashSet<string>(completed, StringComparer.Ordinal);
            var skippedSet = new HashSet<string>(skipped, StringComparer.Ordinal);
            var expectedCompleted = new List<string>();
            var expectedSkipped = new List<string>();
            var expectedEligible = new List<string>();
            var expectedBlocked = new List<string>();
            foreach (var beat in recipe.Beats)
            {
                if (completedSet.Contains(beat.BeatId))
                {
                    expectedCompleted.Add(beat.BeatId);
                }
                else if (skippedSet.Contains(beat.BeatId))
                {
                    expectedSkipped.Add(beat.BeatId);
                }
                else if (AllSatisfied(beat.DependsOnBeatIds, completedSet, skippedSet))
                {
                    expectedEligible.Add(beat.BeatId);
                }
                else
                {
                    expectedBlocked.Add(beat.BeatId);
                }
            }

            AssertEvidenceIds(result.Progression.Completed, expectedCompleted.ToArray());
            AssertEvidenceIds(
                result.Progression.SkippedOptional,
                expectedSkipped.ToArray());
            AssertEvidenceIds(result.Progression.Eligible, expectedEligible.ToArray());
            AssertEvidenceIds(result.Progression.Blocked, expectedBlocked.ToArray());

            foreach (var evidence in result.Progression.Blocked)
            {
                var beat = FindBeat(recipe, evidence.BeatId);
                var expectedBlockers = new List<string>();
                foreach (var dependencyId in beat.DependsOnBeatIds)
                {
                    if (!completedSet.Contains(dependencyId)
                        && !skippedSet.Contains(dependencyId))
                    {
                        expectedBlockers.Add(dependencyId);
                    }
                }

                CollectionAssert.AreEqual(expectedBlockers, evidence.BlockingBeatIds);
            }
        }

        private static bool AllSatisfied(
            IReadOnlyList<string> dependencyIds,
            ISet<string> completed,
            ISet<string> skipped)
        {
            foreach (var dependencyId in dependencyIds)
            {
                if (!completed.Contains(dependencyId) && !skipped.Contains(dependencyId))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AllContained(
            IReadOnlyList<string> expected,
            IReadOnlyList<string> actual)
        {
            var actualSet = new HashSet<string>(actual, StringComparer.Ordinal);
            foreach (var value in expected)
            {
                if (!actualSet.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }

        private static InfernalDefensePacingProgressionResult EvaluateCompleted(
            InfernalDefensePacingRecipe recipe,
            IReadOnlyList<string> completed)
        {
            return InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                completed,
                Array.Empty<string>());
        }

        private static InfernalDefensePacingProgressionResult EvaluateSkipped(
            InfernalDefensePacingRecipe recipe,
            IReadOnlyList<string> skipped)
        {
            return InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                Array.Empty<string>(),
                skipped);
        }

        private static void AssertRejected(
            InfernalDefensePacingProgressionResult result,
            params InfernalDefensePacingProgressionIssue[] expectedIssues)
        {
            Assert.That(result.Status,
                Is.EqualTo(InfernalDefensePacingProgressionStatus.Rejected));
            Assert.That(result.HasProgression, Is.False);
            Assert.That(result.Progression, Is.Null);
            CollectionAssert.AreEqual(expectedIssues, result.Issues);
        }

        private static IReadOnlyList<string> BeatIds(
            InfernalDefensePacingRecipe recipe,
            InfernalDefensePacingRequirement requirement)
        {
            var result = new List<string>();
            foreach (var beat in recipe.Beats)
            {
                if (beat.Requirement == requirement)
                {
                    result.Add(beat.BeatId);
                }
            }

            return result;
        }

        private static IReadOnlyList<string> Prefix(
            IReadOnlyList<string> values,
            int count)
        {
            var result = new string[count];
            for (var index = 0; index < count; index++)
            {
                result[index] = values[index];
            }

            return result;
        }

        private static IReadOnlyList<string> Append(
            IReadOnlyList<string> values,
            string value)
        {
            var result = new string[values.Count + 1];
            for (var index = 0; index < values.Count; index++)
            {
                result[index] = values[index];
            }

            result[result.Length - 1] = value;
            return result;
        }

        private static InfernalDefensePacingBeat FindBeat(
            InfernalDefensePacingRecipe recipe,
            InfernalDefensePacingRequirement requirement)
        {
            foreach (var beat in recipe.Beats)
            {
                if (beat.Requirement == requirement)
                {
                    return beat;
                }
            }

            Assert.Fail("Expected one beat with the requested requirement.");
            return null;
        }

        private static InfernalDefensePacingBeat FindBeat(
            InfernalDefensePacingRecipe recipe,
            string beatId)
        {
            foreach (var beat in recipe.Beats)
            {
                if (string.Equals(beat.BeatId, beatId, StringComparison.Ordinal))
                {
                    return beat;
                }
            }

            Assert.Fail("Expected exact authored beat: " + beatId);
            return null;
        }

        private static IEnumerable<IReadOnlyList<InfernalDefensePacingBeatEvidence>> Categories(
            InfernalDefensePacingProgression progression)
        {
            yield return progression.Completed;
            yield return progression.SkippedOptional;
            yield return progression.Eligible;
            yield return progression.Blocked;
        }

        private static void AssertEvidenceIds(
            IReadOnlyList<InfernalDefensePacingBeatEvidence> evidence,
            params string[] expectedBeatIds)
        {
            CollectionAssert.AreEqual(expectedBeatIds, EvidenceIds(evidence));
        }

        private static string[] EvidenceIds(
            IReadOnlyList<InfernalDefensePacingBeatEvidence> evidence)
        {
            var result = new string[evidence.Count];
            for (var index = 0; index < evidence.Count; index++)
            {
                result[index] = evidence[index].BeatId;
            }

            return result;
        }

        private static string[] AllBeatIds(InfernalDefensePacingRecipe recipe)
        {
            var result = new string[recipe.Beats.Count];
            for (var index = 0; index < recipe.Beats.Count; index++)
            {
                result[index] = recipe.Beats[index].BeatId;
            }

            return result;
        }

        private static InfernalDefensePacingRecipe Copy(
            InfernalDefensePacingRecipe source,
            string layoutId,
            IReadOnlyList<InfernalDefensePacingBeat> beats)
        {
            return new InfernalDefensePacingRecipe(
                layoutId,
                source.TacticalIntent,
                beats);
        }

        private static InfernalDefensePacingBeat CopyBeat(
            InfernalDefensePacingBeat source,
            IReadOnlyList<string> dependencyIds)
        {
            return new InfernalDefensePacingBeat(
                source.BeatId,
                source.RoleId,
                source.Kind,
                source.Requirement,
                dependencyIds);
        }

        private static IReadOnlyList<InfernalDefensePacingBeat> ReplaceBeat(
            IReadOnlyList<InfernalDefensePacingBeat> source,
            int index,
            InfernalDefensePacingBeat replacement)
        {
            var result = new InfernalDefensePacingBeat[source.Count];
            for (var current = 0; current < source.Count; current++)
            {
                result[current] = current == index ? replacement : source[current];
            }

            return result;
        }
    }
}
