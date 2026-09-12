using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefenseBeatReadability;
using RealmRaiders.Modules.InfernalDefensePacing;
using RealmRaiders.Modules.InfernalDefensePacingProgression;
using RealmRaiders.Modules.InfernalDefensePressureTiming;

namespace RealmRaiders.Modules.InfernalDefenseBeatBundle.Tests
{
    public sealed class InfernalDefenseBeatBundleComposerTests
    {
        [Test]
        public void EveryEligibleBeatAcrossEveryValidAuthoredState_ComposesExactSources()
        {
            var stateCount = 0;
            var bundleCount = 0;
            foreach (var recipe in StarterInfernalDefensePacing.All)
            {
                var timingProfile = ResolveTiming(recipe.LayoutId);
                var readabilityProfile = ResolveReadability(recipe.LayoutId);
                foreach (var progression in ValidProgressions(recipe))
                {
                    stateCount++;
                    foreach (var eligible in progression.Progression.Eligible)
                    {
                        var beatIndex = FindBeatIndex(recipe, eligible.BeatId);
                        var result = InfernalDefenseBeatBundleComposer.ComposeExactEligible(
                            recipe.LayoutId,
                            eligible.BeatId,
                            progression,
                            timingProfile.Beats[beatIndex],
                            readabilityProfile.Facts[beatIndex]);

                        Assert.That(result.HasBundle, Is.True, eligible.BeatId);
                        Assert.That(result.Status,
                            Is.EqualTo(InfernalDefenseBeatBundleStatus.Composed));
                        Assert.That(result.Issues, Is.Empty);
                        Assert.That(result.Bundle.LayoutId, Is.EqualTo(recipe.LayoutId));
                        Assert.That(result.Bundle.AuthoredBeatIndex, Is.EqualTo(beatIndex));
                        Assert.That(result.Bundle.BeatId, Is.EqualTo(recipe.Beats[beatIndex].BeatId));
                        Assert.That(result.Bundle.RoleId, Is.EqualTo(recipe.Beats[beatIndex].RoleId));
                        Assert.That(result.Bundle.Kind, Is.EqualTo(recipe.Beats[beatIndex].Kind));
                        Assert.That(
                            result.Bundle.Requirement,
                            Is.EqualTo(recipe.Beats[beatIndex].Requirement));
                        Assert.That(result.Bundle.ProgressionResult, Is.SameAs(progression));
                        Assert.That(result.Bundle.EligibilityEvidence, Is.SameAs(eligible));
                        Assert.That(
                            result.Bundle.TimingFact,
                            Is.SameAs(timingProfile.Beats[beatIndex]));
                        Assert.That(
                            result.Bundle.ReadabilityFact,
                            Is.SameAs(readabilityProfile.Facts[beatIndex]));
                        bundleCount++;
                    }
                }
            }

            Assert.That(stateCount, Is.EqualTo(41));
            Assert.That(bundleCount, Is.EqualTo(43));
        }

        [Test]
        public void ProgressionClassification_PreservesAuthoredOrderAndDependencyEvidence()
        {
            foreach (var recipe in StarterInfernalDefensePacing.All)
            {
                foreach (var progression in ValidProgressions(recipe))
                {
                    AssertAuthoredOrder(recipe, progression.Progression.Completed);
                    AssertAuthoredOrder(recipe, progression.Progression.SkippedOptional);
                    AssertAuthoredOrder(recipe, progression.Progression.Eligible);
                    AssertAuthoredOrder(recipe, progression.Progression.Blocked);

                    foreach (var evidence in progression.Progression.Eligible)
                    {
                        Assert.That(evidence.BlockingBeatIds, Is.Empty);
                    }

                    foreach (var evidence in progression.Progression.Blocked)
                    {
                        Assert.That(evidence.BlockingBeatIds, Is.Not.Empty);
                    }
                }
            }
        }

        [Test]
        public void MissingMalformedAndUnknownLayoutOrBeat_FailClosed()
        {
            var source = ValidStart(StarterInfernalDefensePacing.AshenSpur);
            var timing = StarterInfernalDefensePressureTiming.AshenSpur.Beats[0];
            var readability = StarterInfernalDefenseBeatReadability.AshenSpur.Facts[0];

            AssertContains(
                Compose(null, "ashen-spur.entry", source, timing, readability),
                InfernalDefenseBeatBundleIssue.LayoutIdMissing);
            AssertContains(
                Compose("Bad Layout", "ashen-spur.entry", source, timing, readability),
                InfernalDefenseBeatBundleIssue.LayoutIdInvalid);
            AssertContains(
                Compose(
                    "realmraiders.infernal-defense.unknown",
                    "ashen-spur.entry",
                    source,
                    timing,
                    readability),
                InfernalDefenseBeatBundleIssue.LayoutNotFound);
            AssertContains(
                Compose(StarterInfernalDefensePacing.AshenSpur.LayoutId, null,
                    source, timing, readability),
                InfernalDefenseBeatBundleIssue.BeatIdMissing);
            AssertContains(
                Compose(StarterInfernalDefensePacing.AshenSpur.LayoutId, "Bad Beat",
                    source, timing, readability),
                InfernalDefenseBeatBundleIssue.BeatIdInvalid);
            AssertContains(
                Compose(StarterInfernalDefensePacing.AshenSpur.LayoutId, "unknown.beat",
                    source, timing, readability),
                InfernalDefenseBeatBundleIssue.BeatUnknown);
        }

        [Test]
        public void MissingRejectedAndCrossLayoutProgression_FailClosed()
        {
            var ashen = StarterInfernalDefensePacing.AshenSpur;
            var timing = StarterInfernalDefensePressureTiming.AshenSpur.Beats[0];
            var readability = StarterInfernalDefenseBeatReadability.AshenSpur.Facts[0];
            var rejected = InfernalDefensePacingProgressionEvaluator.Evaluate(
                ashen,
                new[] { ashen.Beats[3].BeatId },
                Array.Empty<string>());
            var cinder = ValidStart(StarterInfernalDefensePacing.CinderFork);

            AssertContains(
                Compose(ashen.LayoutId, ashen.Beats[0].BeatId, null, timing, readability),
                InfernalDefenseBeatBundleIssue.ProgressionMissing);
            AssertContains(
                Compose(ashen.LayoutId, ashen.Beats[0].BeatId,
                    rejected, timing, readability),
                InfernalDefenseBeatBundleIssue.ProgressionInvalid);
            AssertContains(
                Compose(ashen.LayoutId, ashen.Beats[0].BeatId,
                    cinder, timing, readability),
                InfernalDefenseBeatBundleIssue.ProgressionLayoutMismatch);
        }

        [Test]
        public void CompletedSkippedAndBlockedBeats_AreNeverEligibleBundles()
        {
            var recipe = StarterInfernalDefensePacing.CinderFork;
            var afterEntry = InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                new[] { recipe.Beats[0].BeatId },
                Array.Empty<string>());
            var skippedFlame = InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                new[] { recipe.Beats[0].BeatId },
                new[] { recipe.Beats[2].BeatId });
            var timing = StarterInfernalDefensePressureTiming.CinderFork;
            var readability = StarterInfernalDefenseBeatReadability.CinderFork;

            AssertIneligible(recipe, 0, afterEntry, timing, readability);
            AssertIneligible(recipe, 2, skippedFlame, timing, readability);
            AssertIneligible(recipe, 4, afterEntry, timing, readability);
        }

        [Test]
        public void CrossLayoutAndCrossBeatTimingFacts_FailClosed()
        {
            var recipe = StarterInfernalDefensePacing.AshenSpur;
            var progression = ValidStart(recipe);
            var readability = StarterInfernalDefenseBeatReadability.AshenSpur.Facts[0];

            AssertContains(
                Compose(
                    recipe.LayoutId,
                    recipe.Beats[0].BeatId,
                    progression,
                    StarterInfernalDefensePressureTiming.CinderFork.Beats[0],
                    readability),
                InfernalDefenseBeatBundleIssue.TimingFactInvalid,
                InfernalDefenseBeatBundleIssue.TimingFactMismatch,
                InfernalDefenseBeatBundleIssue.TimingFactIdentityMismatch);
            AssertContains(
                Compose(
                    recipe.LayoutId,
                    recipe.Beats[0].BeatId,
                    progression,
                    StarterInfernalDefensePressureTiming.AshenSpur.Beats[1],
                    readability),
                InfernalDefenseBeatBundleIssue.TimingFactInvalid,
                InfernalDefenseBeatBundleIssue.TimingFactMismatch,
                InfernalDefenseBeatBundleIssue.TimingFactIdentityMismatch);
        }

        [Test]
        public void CrossLayoutAndCrossBeatReadabilityFacts_FailClosed()
        {
            var recipe = StarterInfernalDefensePacing.AshenSpur;
            var progression = ValidStart(recipe);
            var timing = StarterInfernalDefensePressureTiming.AshenSpur.Beats[0];

            AssertContains(
                Compose(
                    recipe.LayoutId,
                    recipe.Beats[0].BeatId,
                    progression,
                    timing,
                    StarterInfernalDefenseBeatReadability.CinderFork.Facts[0]),
                InfernalDefenseBeatBundleIssue.ReadabilityFactInvalid,
                InfernalDefenseBeatBundleIssue.ReadabilityFactMismatch,
                InfernalDefenseBeatBundleIssue.ReadabilityFactIdentityMismatch);
            AssertContains(
                Compose(
                    recipe.LayoutId,
                    recipe.Beats[0].BeatId,
                    progression,
                    timing,
                    StarterInfernalDefenseBeatReadability.AshenSpur.Facts[1]),
                InfernalDefenseBeatBundleIssue.ReadabilityFactInvalid,
                InfernalDefenseBeatBundleIssue.ReadabilityFactMismatch,
                InfernalDefenseBeatBundleIssue.ReadabilityFactIdentityMismatch);
        }

        [Test]
        public void EqualClonesFailIdentityEvenWhenEveryPublicFieldMatches()
        {
            var recipe = StarterInfernalDefensePacing.EmberCircuit;
            var progression = ValidStart(recipe);
            var timingSource = StarterInfernalDefensePressureTiming.EmberCircuit.Beats[0];
            var readabilitySource = StarterInfernalDefenseBeatReadability.EmberCircuit.Facts[0];
            var timingClone = CloneTiming(timingSource);
            var readabilityClone = CloneReadability(readabilitySource);

            AssertIssues(
                Compose(recipe.LayoutId, recipe.Beats[0].BeatId,
                    progression, timingClone, readabilitySource),
                InfernalDefenseBeatBundleIssue.TimingFactIdentityMismatch);
            AssertIssues(
                Compose(recipe.LayoutId, recipe.Beats[0].BeatId,
                    progression, timingSource, readabilityClone),
                InfernalDefenseBeatBundleIssue.ReadabilityFactIdentityMismatch);
        }

        [Test]
        public void InvalidTimingAndReadabilityFacts_FailClosedWithoutPartialBundle()
        {
            var recipe = StarterInfernalDefensePacing.CinderFork;
            var progression = ValidStart(recipe);
            var timingSource = StarterInfernalDefensePressureTiming.CinderFork.Beats[0];
            var readabilitySource = StarterInfernalDefenseBeatReadability.CinderFork.Facts[0];
            var invalidTiming = new InfernalDefensePressureBeatTiming(
                timingSource.BeatId,
                timingSource.RoleId,
                timingSource.Kind,
                timingSource.Requirement,
                double.NaN,
                timingSource.PressureDurationSeconds,
                timingSource.ResponseWindowSeconds);
            var invalidReadability = new InfernalDefenseBeatReadabilityFact(
                readabilitySource.BeatId,
                readabilitySource.RoleId,
                readabilitySource.Kind,
                readabilitySource.Requirement,
                "ENTRY AUTOMATIC REWARD",
                readabilitySource.TacticalHint,
                readabilitySource.Emphasis);

            AssertContains(
                Compose(recipe.LayoutId, recipe.Beats[0].BeatId,
                    progression, invalidTiming, readabilitySource),
                InfernalDefenseBeatBundleIssue.TimingFactInvalid,
                InfernalDefenseBeatBundleIssue.TimingFactMismatch,
                InfernalDefenseBeatBundleIssue.TimingFactIdentityMismatch);
            var readabilityResult = Compose(
                recipe.LayoutId,
                recipe.Beats[0].BeatId,
                progression,
                timingSource,
                invalidReadability);
            AssertContains(
                readabilityResult,
                InfernalDefenseBeatBundleIssue.ReadabilityFactInvalid,
                InfernalDefenseBeatBundleIssue.ReadabilityFactMismatch,
                InfernalDefenseBeatBundleIssue.ReadabilityFactIdentityMismatch);
            Assert.That(readabilityResult.Bundle, Is.Null);
            Assert.That(readabilityResult.HasBundle, Is.False);
        }

        [Test]
        public void MissingTimingAndReadabilityFacts_ReportStableEvidenceOrder()
        {
            var recipe = StarterInfernalDefensePacing.AshenSpur;
            var result = Compose(
                recipe.LayoutId,
                recipe.Beats[0].BeatId,
                null,
                null,
                null);

            AssertIssues(
                result,
                InfernalDefenseBeatBundleIssue.ProgressionMissing,
                InfernalDefenseBeatBundleIssue.TimingFactMissing,
                InfernalDefenseBeatBundleIssue.ReadabilityFactMissing);
            Assert.That(result.Status, Is.EqualTo(InfernalDefenseBeatBundleStatus.Rejected));
            Assert.That(result.Bundle, Is.Null);
        }

        [Test]
        public void ResultIssueSnapshot_IsImmutable()
        {
            var recipe = StarterInfernalDefensePacing.AshenSpur;
            var rejected = Compose(recipe.LayoutId, recipe.Beats[0].BeatId,
                null, null, null);
            var valid = Compose(
                recipe.LayoutId,
                recipe.Beats[0].BeatId,
                ValidStart(recipe),
                StarterInfernalDefensePressureTiming.AshenSpur.Beats[0],
                StarterInfernalDefenseBeatReadability.AshenSpur.Facts[0]);

            Assert.Throws<NotSupportedException>(() => ((IList)rejected.Issues).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList)valid.Issues).Clear());
        }

        private static IEnumerable<InfernalDefensePacingProgressionResult> ValidProgressions(
            InfernalDefensePacingRecipe recipe)
        {
            var required = new List<string>();
            InfernalDefensePacingBeat optional = null;
            foreach (var beat in recipe.Beats)
            {
                if (beat.Requirement == InfernalDefensePacingRequirement.Required)
                {
                    required.Add(beat.BeatId);
                }
                else
                {
                    optional = beat;
                }
            }

            for (var prefixLength = 0; prefixLength <= required.Count; prefixLength++)
            {
                var completed = Prefix(required, prefixLength);
                yield return Evaluate(recipe, completed, Array.Empty<string>());
                yield return Evaluate(recipe, completed, new[] { optional.BeatId });
                if (ContainsAll(completed, optional.DependsOnBeatIds))
                {
                    yield return Evaluate(
                        recipe,
                        Append(completed, optional.BeatId),
                        Array.Empty<string>());
                }
            }
        }

        private static InfernalDefensePacingProgressionResult Evaluate(
            InfernalDefensePacingRecipe recipe,
            IReadOnlyList<string> completed,
            IReadOnlyList<string> skipped)
        {
            var result = InfernalDefensePacingProgressionEvaluator.Evaluate(
                recipe,
                completed,
                skipped);
            Assert.That(result.HasProgression, Is.True, recipe.LayoutId);
            return result;
        }

        private static InfernalDefensePacingProgressionResult ValidStart(
            InfernalDefensePacingRecipe recipe)
        {
            return Evaluate(recipe, Array.Empty<string>(), Array.Empty<string>());
        }

        private static InfernalDefenseBeatBundleResult Compose(
            string layoutId,
            string beatId,
            InfernalDefensePacingProgressionResult progression,
            InfernalDefensePressureBeatTiming timing,
            InfernalDefenseBeatReadabilityFact readability)
        {
            return InfernalDefenseBeatBundleComposer.ComposeExactEligible(
                layoutId,
                beatId,
                progression,
                timing,
                readability);
        }

        private static void AssertIneligible(
            InfernalDefensePacingRecipe recipe,
            int beatIndex,
            InfernalDefensePacingProgressionResult progression,
            InfernalDefensePressureTimingProfile timing,
            InfernalDefenseBeatReadabilityProfile readability)
        {
            AssertIssues(
                Compose(
                    recipe.LayoutId,
                    recipe.Beats[beatIndex].BeatId,
                    progression,
                    timing.Beats[beatIndex],
                    readability.Facts[beatIndex]),
                InfernalDefenseBeatBundleIssue.BeatIneligible);
        }

        private static void AssertAuthoredOrder(
            InfernalDefensePacingRecipe recipe,
            IReadOnlyList<InfernalDefensePacingBeatEvidence> evidence)
        {
            var previous = -1;
            foreach (var item in evidence)
            {
                var index = FindBeatIndex(recipe, item.BeatId);
                Assert.That(index, Is.GreaterThan(previous));
                previous = index;
            }
        }

        private static InfernalDefensePressureTimingProfile ResolveTiming(string layoutId)
        {
            var result = StarterInfernalDefensePressureTimingResolver.ResolveExact(layoutId);
            Assert.That(result.Found, Is.True, layoutId);
            return result.Profile;
        }

        private static InfernalDefenseBeatReadabilityProfile ResolveReadability(
            string layoutId)
        {
            var result = StarterInfernalDefenseBeatReadabilityResolver.ResolveExact(layoutId);
            Assert.That(result.Found, Is.True, layoutId);
            return result.Profile;
        }

        private static int FindBeatIndex(
            InfernalDefensePacingRecipe recipe,
            string beatId)
        {
            for (var index = 0; index < recipe.Beats.Count; index++)
            {
                if (string.Equals(recipe.Beats[index].BeatId, beatId, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            Assert.Fail("Expected exact beat: " + beatId);
            return -1;
        }

        private static IReadOnlyList<string> Prefix(IReadOnlyList<string> source, int count)
        {
            var result = new string[count];
            for (var index = 0; index < count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }

        private static IReadOnlyList<string> Append(
            IReadOnlyList<string> source,
            string value)
        {
            var result = new string[source.Count + 1];
            for (var index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            result[result.Length - 1] = value;
            return result;
        }

        private static bool ContainsAll(
            IReadOnlyList<string> source,
            IReadOnlyList<string> expected)
        {
            var values = new HashSet<string>(source, StringComparer.Ordinal);
            foreach (var value in expected)
            {
                if (!values.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }

        private static InfernalDefensePressureBeatTiming CloneTiming(
            InfernalDefensePressureBeatTiming source)
        {
            return new InfernalDefensePressureBeatTiming(
                source.BeatId,
                source.RoleId,
                source.Kind,
                source.Requirement,
                source.StartOffsetSeconds,
                source.PressureDurationSeconds,
                source.ResponseWindowSeconds);
        }

        private static InfernalDefenseBeatReadabilityFact CloneReadability(
            InfernalDefenseBeatReadabilityFact source)
        {
            return new InfernalDefenseBeatReadabilityFact(
                source.BeatId,
                source.RoleId,
                source.Kind,
                source.Requirement,
                source.PrimaryCue,
                source.TacticalHint,
                source.Emphasis);
        }

        private static void AssertContains(
            InfernalDefenseBeatBundleResult result,
            params InfernalDefenseBeatBundleIssue[] expectedIssues)
        {
            Assert.That(result.HasBundle, Is.False);
            Assert.That(result.Bundle, Is.Null);
            foreach (var issue in expectedIssues)
            {
                Assert.That(result.Issues, Does.Contain(issue));
            }
        }

        private static void AssertIssues(
            InfernalDefenseBeatBundleResult result,
            params InfernalDefenseBeatBundleIssue[] expectedIssues)
        {
            Assert.That(result.HasBundle, Is.False);
            CollectionAssert.AreEqual(expectedIssues, result.Issues);
        }
    }
}
