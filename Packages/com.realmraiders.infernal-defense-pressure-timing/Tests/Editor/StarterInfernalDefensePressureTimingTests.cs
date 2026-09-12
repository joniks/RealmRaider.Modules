using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefensePacing;

namespace RealmRaiders.Modules.InfernalDefensePressureTiming.Tests
{
    public sealed class StarterInfernalDefensePressureTimingTests
    {
        [Test]
        public void Catalogue_IsCachedValidCompleteAndSemanticallyDistinct()
        {
            var result = InfernalDefensePressureTimingValidator.ValidateCatalogue(
                StarterInfernalDefensePressureTiming.All);
            var signatures = new HashSet<string>(StringComparer.Ordinal);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Issues, Is.Empty);
            Assert.That(StarterInfernalDefensePressureTiming.All.Count, Is.EqualTo(3));
            Assert.That(StarterInfernalDefensePressureTiming.All[0],
                Is.SameAs(StarterInfernalDefensePressureTiming.AshenSpur));
            Assert.That(StarterInfernalDefensePressureTiming.All[1],
                Is.SameAs(StarterInfernalDefensePressureTiming.CinderFork));
            Assert.That(StarterInfernalDefensePressureTiming.All[2],
                Is.SameAs(StarterInfernalDefensePressureTiming.EmberCircuit));
            foreach (var profile in StarterInfernalDefensePressureTiming.All)
            {
                Assert.That(signatures.Add(
                    InfernalDefensePressureTimingValidator.CreateSemanticSignature(profile)),
                    Is.True,
                    profile.LayoutId);
            }
        }

        [Test]
        public void CachedProfiles_PreserveExactFieldForFieldSnapshots()
        {
            AssertSnapshot(
                StarterInfernalDefensePressureTiming.AshenSpur,
                StarterInfernalDefensePacing.AshenSpur,
                34d,
                Expected("ashen-spur.entry", 1d, 4d, 2d),
                Expected("ashen-spur.hellhound-spur", 6d, 12d, 3d),
                Expected("ashen-spur.flame-route", 8d, 5d, 2.5d),
                Expected("ashen-spur.brute-window", 16d, 8d, 4d),
                Expected("ashen-spur.heart-objective", 27d, 7d, 3d));
            AssertSnapshot(
                StarterInfernalDefensePressureTiming.CinderFork,
                StarterInfernalDefensePacing.CinderFork,
                35d,
                Expected("cinder-fork.entry", 1d, 3d, 1.5d),
                Expected("cinder-fork.hellhound-pressure", 5d, 10d, 2d),
                Expected("cinder-fork.flame-choice", 7d, 4d, 2d),
                Expected("cinder-fork.brute-window", 18d, 7d, 3.5d),
                Expected("cinder-fork.heart-objective", 28d, 7d, 2.5d));
            AssertSnapshot(
                StarterInfernalDefensePressureTiming.EmberCircuit,
                StarterInfernalDefensePacing.EmberCircuit,
                43d,
                Expected("ember-circuit.entry", 1d, 4d, 2d),
                Expected("ember-circuit.hellhound-pressure", 6d, 7d, 2.5d),
                Expected("ember-circuit.flame-choice", 15d, 5d, 3d),
                Expected("ember-circuit.brute-window", 23d, 9d, 4.5d),
                Expected("ember-circuit.heart-objective", 35d, 8d, 3.5d));
        }

        [Test]
        public void ResolveExact_ReturnsOnlyExactCachedProfilesDeterministically()
        {
            foreach (var profile in StarterInfernalDefensePressureTiming.All)
            {
                var first = StarterInfernalDefensePressureTimingResolver.ResolveExact(
                    profile.LayoutId);
                var second = StarterInfernalDefensePressureTimingResolver.ResolveExact(
                    profile.LayoutId);

                Assert.That(first.Status,
                    Is.EqualTo(InfernalDefensePressureTimingLookupStatus.Found));
                Assert.That(first.Profile, Is.SameAs(profile));
                Assert.That(second.Profile, Is.SameAs(first.Profile));
            }

            Assert.That(
                StarterInfernalDefensePressureTimingResolver.ResolveExact(null).Status,
                Is.EqualTo(InfernalDefensePressureTimingLookupStatus.LayoutIdInvalid));
            Assert.That(
                StarterInfernalDefensePressureTimingResolver.ResolveExact(
                    "Realmraiders.Infernal").Status,
                Is.EqualTo(InfernalDefensePressureTimingLookupStatus.LayoutIdInvalid));
            Assert.That(
                StarterInfernalDefensePressureTimingResolver.ResolveExact(
                    "realmraiders.infernal-defense.unknown").Status,
                Is.EqualTo(InfernalDefensePressureTimingLookupStatus.NotFound));
        }

        [Test]
        public void EveryProfile_HasDependencyCompatibleAuthoredOrderAndBoundedTotal()
        {
            foreach (var profile in StarterInfernalDefensePressureTiming.All)
            {
                var recipe = ResolveRecipe(profile.LayoutId);
                var timingByBeatId = TimingIndex(profile.Beats);
                var maximumEnd = 0d;
                for (var index = 0; index < recipe.Beats.Count; index++)
                {
                    var beat = recipe.Beats[index];
                    var timing = profile.Beats[index];
                    Assert.That(timing.BeatId, Is.EqualTo(beat.BeatId));
                    foreach (var dependencyId in beat.DependsOnBeatIds)
                    {
                        Assert.That(
                            timing.StartOffsetSeconds,
                            Is.GreaterThanOrEqualTo(
                                timingByBeatId[dependencyId].EndOffsetSeconds));
                    }

                    maximumEnd = Math.Max(maximumEnd, timing.EndOffsetSeconds);
                }

                Assert.That(profile.TargetDurationSeconds, Is.EqualTo(maximumEnd));
                Assert.That(
                    profile.TargetDurationSeconds,
                    Is.InRange(
                        InfernalDefensePressureTimingBounds.MinimumTargetDurationSeconds,
                        InfernalDefensePressureTimingBounds.MaximumTargetDurationSeconds));
            }
        }

        [Test]
        public void FlameToBruteOpportunity_IsHumanSizedWithoutBecomingARequiredDependency()
        {
            foreach (var profile in StarterInfernalDefensePressureTiming.All)
            {
                var recipe = ResolveRecipe(profile.LayoutId);
                var flame = FindTiming(
                    profile,
                    InfernalDefensePacingBeatKind.FlameTrapOpportunity);
                var brute = FindTiming(
                    profile,
                    InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow);
                var flameBeat = FindBeat(
                    recipe,
                    InfernalDefensePacingBeatKind.FlameTrapOpportunity);
                var bruteBeat = FindBeat(
                    recipe,
                    InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow);
                var gap = brute.StartOffsetSeconds - flame.EndOffsetSeconds;

                Assert.That(
                    gap,
                    Is.InRange(
                        InfernalDefensePressureTimingBounds
                            .MinimumFlameToBruteOpportunitySeconds,
                        InfernalDefensePressureTimingBounds
                            .MaximumFlameToBruteOpportunitySeconds));
                Assert.That(
                    brute.ResponseWindowSeconds,
                    Is.GreaterThanOrEqualTo(
                        InfernalDefensePressureTimingBounds
                            .MinimumFlameToBruteOpportunitySeconds));
                if (flameBeat.Requirement == InfernalDefensePacingRequirement.Optional)
                {
                    CollectionAssert.DoesNotContain(
                        bruteBeat.DependsOnBeatIds,
                        flameBeat.BeatId,
                        profile.LayoutId);
                }
            }
        }

        [Test]
        public void SnapshotsAndValidationEvidence_AreImmutable()
        {
            var profile = StarterInfernalDefensePressureTiming.AshenSpur;
            var validation = InfernalDefensePressureTimingValidator.Validate(profile);
            var invalid = InfernalDefensePressureTimingValidator.Validate(null);

            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterInfernalDefensePressureTiming.All).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)profile.Beats).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)validation.Issues).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList)invalid.Issues).Clear());
        }

        [Test]
        public void Validate_FailsClosedForMissingInvalidAndUnknownLayouts()
        {
            AssertIssues(
                InfernalDefensePressureTimingValidator.Validate(null),
                InfernalDefensePressureTimingValidationIssue.ProfileMissing);

            var source = StarterInfernalDefensePressureTiming.AshenSpur;
            var invalid = Copy(source, layoutId: "Invalid Layout");
            var unknown = Copy(
                source,
                layoutId: "realmraiders.infernal-defense.unknown");

            Assert.That(
                InfernalDefensePressureTimingValidator.Validate(invalid).Issues,
                Does.Contain(InfernalDefensePressureTimingValidationIssue.LayoutIdInvalid));
            Assert.That(
                InfernalDefensePressureTimingValidator.Validate(unknown).Issues,
                Does.Contain(InfernalDefensePressureTimingValidationIssue.LayoutNotFound));
        }

        [Test]
        public void Validate_FailsClosedForBeatCoverageIdentityAndOrderDefects()
        {
            var source = StarterInfernalDefensePressureTiming.CinderFork;
            var missing = Copy(source, beats: RemoveAt(source.Beats, 4));
            var nullBeat = Copy(source, beats: Replace(source.Beats, 1, null));
            var malformed = Copy(
                source,
                beats: Replace(
                    source.Beats,
                    1,
                    CopyBeat(source.Beats[1], beatId: "Bad beat")));
            var duplicate = Copy(
                source,
                beats: Replace(
                    source.Beats,
                    1,
                    CopyBeat(source.Beats[1], beatId: source.Beats[0].BeatId)));
            var unknown = Copy(
                source,
                beats: Replace(
                    source.Beats,
                    1,
                    CopyBeat(source.Beats[1], beatId: "unknown.beat")));
            var reordered = Copy(source, beats: Swap(source.Beats, 1, 2));

            AssertContains(missing, InfernalDefensePressureTimingValidationIssue.BeatCardinalityInvalid);
            AssertContains(nullBeat, InfernalDefensePressureTimingValidationIssue.BeatMissing);
            AssertContains(malformed, InfernalDefensePressureTimingValidationIssue.BeatIdInvalid);
            AssertContains(duplicate, InfernalDefensePressureTimingValidationIssue.BeatIdDuplicate);
            AssertContains(unknown, InfernalDefensePressureTimingValidationIssue.BeatUnknown);
            AssertContains(reordered, InfernalDefensePressureTimingValidationIssue.BeatOrderMismatch);
        }

        [Test]
        public void Validate_FailsClosedForBeatSemanticMismatches()
        {
            var source = StarterInfernalDefensePressureTiming.EmberCircuit;
            var role = Copy(
                source,
                beats: Replace(
                    source.Beats,
                    1,
                    CopyBeat(source.Beats[1], roleId: "unknown.role")));
            var kind = Copy(
                source,
                beats: Replace(
                    source.Beats,
                    1,
                    CopyBeat(
                        source.Beats[1],
                        kind: InfernalDefensePacingBeatKind.Unknown)));
            var requirement = Copy(
                source,
                beats: Replace(
                    source.Beats,
                    2,
                    CopyBeat(
                        source.Beats[2],
                        requirement: InfernalDefensePacingRequirement.Required)));

            AssertContains(role, InfernalDefensePressureTimingValidationIssue.BeatRoleMismatch);
            AssertContains(kind, InfernalDefensePressureTimingValidationIssue.BeatKindMismatch);
            AssertContains(
                requirement,
                InfernalDefensePressureTimingValidationIssue.BeatRequirementMismatch);
        }

        [Test]
        public void Validate_FailsClosedForNonfiniteNonpositiveAndUnboundedTimings()
        {
            var source = StarterInfernalDefensePressureTiming.AshenSpur;
            AssertContains(
                WithBeatTiming(source, 0, start: double.NaN),
                InfernalDefensePressureTimingValidationIssue.TimingNonFinite);
            AssertContains(
                WithBeatTiming(source, 0, duration: double.PositiveInfinity),
                InfernalDefensePressureTimingValidationIssue.TimingNonFinite);
            AssertContains(
                WithBeatTiming(source, 0, response: 0d),
                InfernalDefensePressureTimingValidationIssue.TimingNotPositive,
                InfernalDefensePressureTimingValidationIssue.TimingOutOfBounds);
            AssertContains(
                WithBeatTiming(source, 1, duration: 21d),
                InfernalDefensePressureTimingValidationIssue.TimingOutOfBounds);
            AssertContains(
                WithBeatTiming(source, 0, response: 5d),
                InfernalDefensePressureTimingValidationIssue.ResponseWindowInvalid);
        }

        [Test]
        public void Validate_FailsClosedForTargetDependencyAndOpportunityDefects()
        {
            var source = StarterInfernalDefensePressureTiming.CinderFork;
            var badDependency = WithBeatTiming(source, 1, start: 2d);
            var badOpportunity = WithBeatTiming(source, 3, start: 11.5d);

            AssertContains(
                Copy(source, targetDuration: double.NaN),
                InfernalDefensePressureTimingValidationIssue.TargetDurationNonFinite);
            AssertContains(
                Copy(source, targetDuration: 0d),
                InfernalDefensePressureTimingValidationIssue.TargetDurationNotPositive,
                InfernalDefensePressureTimingValidationIssue.TargetDurationOutOfBounds);
            AssertContains(
                Copy(source, targetDuration: 61d),
                InfernalDefensePressureTimingValidationIssue.TargetDurationOutOfBounds,
                InfernalDefensePressureTimingValidationIssue.TargetDurationMismatch);
            AssertContains(
                badDependency,
                InfernalDefensePressureTimingValidationIssue.DependencyOrderingInvalid);
            AssertContains(
                badOpportunity,
                InfernalDefensePressureTimingValidationIssue.DependencyOrderingInvalid,
                InfernalDefensePressureTimingValidationIssue.FlameBruteOpportunityInvalid);
        }

        [Test]
        public void Validate_HasStableCombinedIssueOrdering()
        {
            var source = StarterInfernalDefensePressureTiming.AshenSpur;
            var badBeat = CopyBeat(
                source.Beats[0],
                beatId: "Bad beat",
                start: double.NaN,
                duration: double.PositiveInfinity,
                response: double.NegativeInfinity);
            var profile = Copy(
                source,
                targetDuration: 0d,
                beats: Replace(source.Beats, 0, badBeat));

            AssertIssues(
                InfernalDefensePressureTimingValidator.Validate(profile),
                InfernalDefensePressureTimingValidationIssue.TargetDurationNotPositive,
                InfernalDefensePressureTimingValidationIssue.TargetDurationOutOfBounds,
                InfernalDefensePressureTimingValidationIssue.BeatIdInvalid,
                InfernalDefensePressureTimingValidationIssue.TimingNonFinite,
                InfernalDefensePressureTimingValidationIssue.TargetDurationMismatch,
                InfernalDefensePressureTimingValidationIssue.DependencyOrderingInvalid);
        }

        [Test]
        public void ValidateCatalogue_FailsClosedForCoverageDuplicateAndEquivalentTimingShapes()
        {
            var commonAshen = WithCommonShape(StarterInfernalDefensePacing.AshenSpur);
            var commonCinder = WithCommonShape(StarterInfernalDefensePacing.CinderFork);
            var commonEmber = WithCommonShape(StarterInfernalDefensePacing.EmberCircuit);
            var duplicateShape = InfernalDefensePressureTimingValidator.ValidateCatalogue(
                new[] { commonAshen, commonCinder, commonEmber });
            var duplicateLayout = InfernalDefensePressureTimingValidator.ValidateCatalogue(
                new[]
                {
                    StarterInfernalDefensePressureTiming.AshenSpur,
                    StarterInfernalDefensePressureTiming.AshenSpur,
                    StarterInfernalDefensePressureTiming.EmberCircuit
                });

            Assert.That(commonAshen, Is.Not.Null);
            Assert.That(
                InfernalDefensePressureTimingValidator.Validate(commonCinder).IsValid,
                Is.True);
            Assert.That(
                InfernalDefensePressureTimingValidator.Validate(commonEmber).IsValid,
                Is.True);
            Assert.That(
                duplicateShape.Issues,
                Does.Contain(
                    InfernalDefensePressureTimingCatalogueIssue.SemanticSignatureDuplicate));
            Assert.That(
                duplicateLayout.Issues,
                Does.Contain(InfernalDefensePressureTimingCatalogueIssue.LayoutIdDuplicate));
            Assert.That(
                duplicateLayout.Issues,
                Does.Contain(InfernalDefensePressureTimingCatalogueIssue.LayoutCoverageInvalid));
            AssertIssues(
                InfernalDefensePressureTimingValidator.ValidateCatalogue(null),
                InfernalDefensePressureTimingCatalogueIssue.CatalogueMissing);
        }

        private static void AssertSnapshot(
            InfernalDefensePressureTimingProfile profile,
            InfernalDefensePacingRecipe recipe,
            double targetDuration,
            params ExpectedTiming[] expected)
        {
            Assert.That(
                InfernalDefensePressureTimingValidator.Validate(profile).IsValid,
                Is.True,
                profile.LayoutId);
            Assert.That(profile.LayoutId, Is.EqualTo(recipe.LayoutId));
            Assert.That(profile.TargetDurationSeconds, Is.EqualTo(targetDuration));
            Assert.That(profile.Beats.Count, Is.EqualTo(expected.Length));
            for (var index = 0; index < expected.Length; index++)
            {
                var actual = profile.Beats[index];
                var expectedTiming = expected[index];
                var recipeBeat = recipe.Beats[index];
                Assert.That(actual.BeatId, Is.EqualTo(expectedTiming.BeatId));
                Assert.That(actual.BeatId, Is.EqualTo(recipeBeat.BeatId));
                Assert.That(actual.RoleId, Is.EqualTo(recipeBeat.RoleId));
                Assert.That(actual.Kind, Is.EqualTo(recipeBeat.Kind));
                Assert.That(actual.Requirement, Is.EqualTo(recipeBeat.Requirement));
                Assert.That(actual.StartOffsetSeconds,
                    Is.EqualTo(expectedTiming.Start));
                Assert.That(actual.PressureDurationSeconds,
                    Is.EqualTo(expectedTiming.Duration));
                Assert.That(actual.ResponseWindowSeconds,
                    Is.EqualTo(expectedTiming.Response));
                Assert.That(actual.EndOffsetSeconds,
                    Is.EqualTo(expectedTiming.Start + expectedTiming.Duration));
            }
        }

        private static InfernalDefensePressureTimingProfile WithCommonShape(
            InfernalDefensePacingRecipe recipe)
        {
            return new InfernalDefensePressureTimingProfile(
                recipe.LayoutId,
                33d,
                new[]
                {
                    FromRecipe(recipe, 0, 1d, 3d, 1.5d),
                    FromRecipe(recipe, 1, 5d, 5d, 2d),
                    FromRecipe(recipe, 2, 11d, 4d, 2d),
                    FromRecipe(recipe, 3, 18d, 6d, 3d),
                    FromRecipe(recipe, 4, 27d, 6d, 3d)
                });
        }

        private static InfernalDefensePressureBeatTiming FromRecipe(
            InfernalDefensePacingRecipe recipe,
            int index,
            double start,
            double duration,
            double response)
        {
            var beat = recipe.Beats[index];
            return new InfernalDefensePressureBeatTiming(
                beat.BeatId,
                beat.RoleId,
                beat.Kind,
                beat.Requirement,
                start,
                duration,
                response);
        }

        private static InfernalDefensePressureTimingProfile WithBeatTiming(
            InfernalDefensePressureTimingProfile source,
            int index,
            double? start = null,
            double? duration = null,
            double? response = null)
        {
            var replacement = CopyBeat(
                source.Beats[index],
                start: start,
                duration: duration,
                response: response);
            return Copy(source, beats: Replace(source.Beats, index, replacement));
        }

        private static InfernalDefensePressureTimingProfile Copy(
            InfernalDefensePressureTimingProfile source,
            string layoutId = null,
            double? targetDuration = null,
            IReadOnlyList<InfernalDefensePressureBeatTiming> beats = null)
        {
            return new InfernalDefensePressureTimingProfile(
                layoutId ?? source.LayoutId,
                targetDuration ?? source.TargetDurationSeconds,
                beats ?? source.Beats);
        }

        private static InfernalDefensePressureBeatTiming CopyBeat(
            InfernalDefensePressureBeatTiming source,
            string beatId = null,
            string roleId = null,
            InfernalDefensePacingBeatKind? kind = null,
            InfernalDefensePacingRequirement? requirement = null,
            double? start = null,
            double? duration = null,
            double? response = null)
        {
            return new InfernalDefensePressureBeatTiming(
                beatId ?? source.BeatId,
                roleId ?? source.RoleId,
                kind ?? source.Kind,
                requirement ?? source.Requirement,
                start ?? source.StartOffsetSeconds,
                duration ?? source.PressureDurationSeconds,
                response ?? source.ResponseWindowSeconds);
        }

        private static IReadOnlyList<InfernalDefensePressureBeatTiming> RemoveAt(
            IReadOnlyList<InfernalDefensePressureBeatTiming> source,
            int removeIndex)
        {
            var result = new List<InfernalDefensePressureBeatTiming>();
            for (var index = 0; index < source.Count; index++)
            {
                if (index != removeIndex)
                {
                    result.Add(source[index]);
                }
            }

            return result;
        }

        private static IReadOnlyList<InfernalDefensePressureBeatTiming> Replace(
            IReadOnlyList<InfernalDefensePressureBeatTiming> source,
            int replaceIndex,
            InfernalDefensePressureBeatTiming replacement)
        {
            var result = new InfernalDefensePressureBeatTiming[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                result[index] = index == replaceIndex ? replacement : source[index];
            }

            return result;
        }

        private static IReadOnlyList<InfernalDefensePressureBeatTiming> Swap(
            IReadOnlyList<InfernalDefensePressureBeatTiming> source,
            int first,
            int second)
        {
            var result = new InfernalDefensePressureBeatTiming[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            var temporary = result[first];
            result[first] = result[second];
            result[second] = temporary;
            return result;
        }

        private static InfernalDefensePacingRecipe ResolveRecipe(string layoutId)
        {
            var result = StarterInfernalDefensePacingResolver.ResolveExact(layoutId);
            Assert.That(result.Found, Is.True, layoutId);
            return result.Recipe;
        }

        private static Dictionary<string, InfernalDefensePressureBeatTiming> TimingIndex(
            IReadOnlyList<InfernalDefensePressureBeatTiming> timings)
        {
            var result = new Dictionary<string, InfernalDefensePressureBeatTiming>(
                StringComparer.Ordinal);
            foreach (var timing in timings)
            {
                result.Add(timing.BeatId, timing);
            }

            return result;
        }

        private static InfernalDefensePacingBeat FindBeat(
            InfernalDefensePacingRecipe recipe,
            InfernalDefensePacingBeatKind kind)
        {
            foreach (var beat in recipe.Beats)
            {
                if (beat.Kind == kind)
                {
                    return beat;
                }
            }

            Assert.Fail("Expected authored beat kind: " + kind);
            return null;
        }

        private static InfernalDefensePressureBeatTiming FindTiming(
            InfernalDefensePressureTimingProfile profile,
            InfernalDefensePacingBeatKind kind)
        {
            foreach (var timing in profile.Beats)
            {
                if (timing.Kind == kind)
                {
                    return timing;
                }
            }

            Assert.Fail("Expected authored timing kind: " + kind);
            return null;
        }

        private static void AssertContains(
            InfernalDefensePressureTimingProfile profile,
            params InfernalDefensePressureTimingValidationIssue[] expectedIssues)
        {
            var validation = InfernalDefensePressureTimingValidator.Validate(profile);
            Assert.That(validation.IsValid, Is.False);
            foreach (var issue in expectedIssues)
            {
                Assert.That(validation.Issues, Does.Contain(issue));
            }
        }

        private static void AssertIssues(
            InfernalDefensePressureTimingValidationResult result,
            params InfernalDefensePressureTimingValidationIssue[] expected)
        {
            CollectionAssert.AreEqual(expected, result.Issues);
        }

        private static void AssertIssues(
            InfernalDefensePressureTimingCatalogueValidationResult result,
            params InfernalDefensePressureTimingCatalogueIssue[] expected)
        {
            CollectionAssert.AreEqual(expected, result.Issues);
        }

        private static ExpectedTiming Expected(
            string beatId,
            double start,
            double duration,
            double response)
        {
            return new ExpectedTiming(beatId, start, duration, response);
        }

        private sealed class ExpectedTiming
        {
            public ExpectedTiming(
                string beatId,
                double start,
                double duration,
                double response)
            {
                BeatId = beatId;
                Start = start;
                Duration = duration;
                Response = response;
            }

            public string BeatId { get; }
            public double Start { get; }
            public double Duration { get; }
            public double Response { get; }
        }
    }
}
