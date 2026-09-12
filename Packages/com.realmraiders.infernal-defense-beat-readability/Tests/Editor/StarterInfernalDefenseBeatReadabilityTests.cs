using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefensePacing;

namespace RealmRaiders.Modules.InfernalDefenseBeatReadability.Tests
{
    public sealed class StarterInfernalDefenseBeatReadabilityTests
    {
        [Test]
        public void Catalogue_IsCachedCompleteValidAndPairwiseDistinctWithoutIds()
        {
            var catalogue = InfernalDefenseBeatReadabilityValidator.ValidateCatalogue(
                StarterInfernalDefenseBeatReadability.All);
            var signatures = new HashSet<string>(StringComparer.Ordinal);

            Assert.That(catalogue.IsValid, Is.True);
            Assert.That(catalogue.Issues, Is.Empty);
            Assert.That(StarterInfernalDefenseBeatReadability.All.Count, Is.EqualTo(3));
            Assert.That(StarterInfernalDefenseBeatReadability.All[0],
                Is.SameAs(StarterInfernalDefenseBeatReadability.AshenSpur));
            Assert.That(StarterInfernalDefenseBeatReadability.All[1],
                Is.SameAs(StarterInfernalDefenseBeatReadability.CinderFork));
            Assert.That(StarterInfernalDefenseBeatReadability.All[2],
                Is.SameAs(StarterInfernalDefenseBeatReadability.EmberCircuit));

            foreach (var profile in StarterInfernalDefenseBeatReadability.All)
            {
                var signature =
                    InfernalDefenseBeatReadabilityValidator.CreateSemanticSignature(profile);
                Assert.That(signature, Does.Not.Contain(profile.LayoutId));
                foreach (var fact in profile.Facts)
                {
                    Assert.That(signature, Does.Not.Contain(fact.BeatId));
                    Assert.That(signature, Does.Not.Contain(fact.RoleId));
                }

                Assert.That(signatures.Add(signature), Is.True, profile.LayoutId);
            }
        }

        [Test]
        public void AshenSpur_PreservesExactAuthoredReadabilitySnapshot()
        {
            AssertSnapshot(
                StarterInfernalDefenseBeatReadability.AshenSpur,
                StarterInfernalDefensePacing.AshenSpur,
                Expected("ashen-spur.entry", "INVADERS AT ASHEN ENTRY",
                    "MAIN ROUTE PRESSURE BEGINS", InfernalDefenseBeatEmphasis.Arrival),
                Expected("ashen-spur.hellhound-spur", "HELLHOUND SPUR PRESSURE",
                    "OPTIONAL SPUR - NOT REQUIRED FOR PROGRESS",
                    InfernalDefenseBeatEmphasis.Pressure),
                Expected("ashen-spur.flame-route", "FLAME ROUTE PRESSURE",
                    "REQUIRED ROUTE BEAT BEFORE THE BRUTE",
                    InfernalDefenseBeatEmphasis.Hazard),
                Expected("ashen-spur.brute-window", "BRUTE POSSESSION WINDOW",
                    "CHOOSE POSSESS OR KEEP DEFENDING",
                    InfernalDefenseBeatEmphasis.Possession),
                Expected("ashen-spur.heart-objective", "INFERNAL HEART OBJECTIVE",
                    "FINAL OBJECTIVE PRESSURE", InfernalDefenseBeatEmphasis.Objective));
        }

        [Test]
        public void CinderFork_PreservesExactAuthoredReadabilitySnapshot()
        {
            AssertSnapshot(
                StarterInfernalDefenseBeatReadability.CinderFork,
                StarterInfernalDefensePacing.CinderFork,
                Expected("cinder-fork.entry", "INVADERS AT CINDER ENTRY",
                    "PRESSURE SPLITS AFTER ENTRY", InfernalDefenseBeatEmphasis.Arrival),
                Expected("cinder-fork.hellhound-pressure", "HELLHOUND ROUTE PRESSURE",
                    "REQUIRED ROUTE BEAT BEFORE THE BRUTE",
                    InfernalDefenseBeatEmphasis.Pressure),
                Expected("cinder-fork.flame-choice", "OPTIONAL FLAME ROUTE",
                    "SKIP WITHOUT BLOCKING THE BRUTE", InfernalDefenseBeatEmphasis.Hazard),
                Expected("cinder-fork.brute-window", "BRUTE POSSESSION WINDOW",
                    "BRUTE WINDOW FOLLOWS HELLHOUND PRESSURE",
                    InfernalDefenseBeatEmphasis.Possession),
                Expected("cinder-fork.heart-objective", "INFERNAL HEART OBJECTIVE",
                    "FINAL OBJECTIVE PRESSURE", InfernalDefenseBeatEmphasis.Objective));
        }

        [Test]
        public void EmberCircuit_PreservesExactAuthoredReadabilitySnapshot()
        {
            AssertSnapshot(
                StarterInfernalDefenseBeatReadability.EmberCircuit,
                StarterInfernalDefensePacing.EmberCircuit,
                Expected("ember-circuit.entry", "INVADERS AT EMBER ENTRY",
                    "PRESSURE BUILDS IN SEQUENCE", InfernalDefenseBeatEmphasis.Arrival),
                Expected("ember-circuit.hellhound-pressure", "HELLHOUND PRESSURE",
                    "REQUIRED BEFORE BOTH FORWARD ROUTES",
                    InfernalDefenseBeatEmphasis.Pressure),
                Expected("ember-circuit.flame-choice", "OPTIONAL FLAME DETOUR",
                    "SKIP WITHOUT BLOCKING THE BRUTE", InfernalDefenseBeatEmphasis.Hazard),
                Expected("ember-circuit.brute-window", "BRUTE POSSESSION WINDOW",
                    "CHOOSE AFTER THE HELLHOUND ROUTE",
                    InfernalDefenseBeatEmphasis.Possession),
                Expected("ember-circuit.heart-objective", "INFERNAL HEART OBJECTIVE",
                    "FINAL OBJECTIVE PRESSURE", InfernalDefenseBeatEmphasis.Objective));
        }

        [Test]
        public void ResolveExact_IsOrdinalDeterministicAndReturnsCachedProfilesOnly()
        {
            foreach (var profile in StarterInfernalDefenseBeatReadability.All)
            {
                var first = StarterInfernalDefenseBeatReadabilityResolver.ResolveExact(
                    profile.LayoutId);
                var second = StarterInfernalDefenseBeatReadabilityResolver.ResolveExact(
                    profile.LayoutId);
                Assert.That(first.Status,
                    Is.EqualTo(InfernalDefenseBeatReadabilityLookupStatus.Found));
                Assert.That(first.Profile, Is.SameAs(profile));
                Assert.That(second.Profile, Is.SameAs(first.Profile));
            }

            Assert.That(
                StarterInfernalDefenseBeatReadabilityResolver.ResolveExact(null).Status,
                Is.EqualTo(InfernalDefenseBeatReadabilityLookupStatus.LayoutIdInvalid));
            Assert.That(
                StarterInfernalDefenseBeatReadabilityResolver.ResolveExact(
                    "Realmraiders.Invalid").Status,
                Is.EqualTo(InfernalDefenseBeatReadabilityLookupStatus.LayoutIdInvalid));
            Assert.That(
                StarterInfernalDefenseBeatReadabilityResolver.ResolveExact(
                    "realmraiders.infernal-defense.unknown").Status,
                Is.EqualTo(InfernalDefenseBeatReadabilityLookupStatus.NotFound));
        }

        [Test]
        public void EveryFact_IsMobileBoundedAndContainsNoPromiseLanguage()
        {
            var forbidden = new[]
            {
                "ACTIVATED",
                "AUTOMATIC",
                "EXPOSED",
                "GUARANTEED",
                "OPENED",
                "REWARD"
            };

            foreach (var profile in StarterInfernalDefenseBeatReadability.All)
            {
                foreach (var fact in profile.Facts)
                {
                    Assert.That(
                        fact.PrimaryCue.Length,
                        Is.InRange(
                            InfernalDefenseBeatReadabilityBounds
                                .MinimumPrimaryCueCharacters,
                            InfernalDefenseBeatReadabilityBounds
                                .MaximumPrimaryCueCharacters));
                    if (fact.HasTacticalHint)
                    {
                        Assert.That(
                            fact.TacticalHint.Length,
                            Is.InRange(
                                InfernalDefenseBeatReadabilityBounds
                                    .MinimumTacticalHintCharacters,
                                InfernalDefenseBeatReadabilityBounds
                                    .MaximumTacticalHintCharacters));
                    }

                    var combined = fact.PrimaryCue + " " + fact.TacticalHint;
                    foreach (var token in forbidden)
                    {
                        Assert.That(combined, Does.Not.Contain(token));
                    }
                }
            }
        }

        [Test]
        public void OptionalSemantics_AreExplicitAndNeverCopiedOntoRequiredBeats()
        {
            foreach (var profile in StarterInfernalDefenseBeatReadability.All)
            {
                var recipe = ResolveRecipe(profile.LayoutId);
                for (var index = 0; index < recipe.Beats.Count; index++)
                {
                    var fact = profile.Facts[index];
                    var combined = fact.PrimaryCue + " " + fact.TacticalHint;
                    if (recipe.Beats[index].Requirement
                        == InfernalDefensePacingRequirement.Optional)
                    {
                        Assert.That(combined, Does.Contain("OPTIONAL"));
                    }
                    else
                    {
                        Assert.That(combined, Does.Not.Contain("OPTIONAL"));
                    }
                }
            }

            Assert.That(
                StarterInfernalDefenseBeatReadability.CinderFork.Facts[2].TacticalHint,
                Does.Contain("WITHOUT BLOCKING THE BRUTE"));
            Assert.That(
                StarterInfernalDefenseBeatReadability.EmberCircuit.Facts[2].TacticalHint,
                Does.Contain("WITHOUT BLOCKING THE BRUTE"));
        }

        [Test]
        public void OptionalTacticalHint_MayBeAbsentWithoutChangingExactBeatMeaning()
        {
            var source = StarterInfernalDefenseBeatReadability.CinderFork;
            var noEntryHint = CopyFact(source.Facts[0], tacticalHint: string.Empty);
            var profile = Copy(source, facts: Replace(source.Facts, 0, noEntryHint));
            var validation = InfernalDefenseBeatReadabilityValidator.Validate(profile);

            Assert.That(noEntryHint.HasTacticalHint, Is.False);
            Assert.That(validation.IsValid, Is.True);
        }

        [Test]
        public void SnapshotsAndValidationEvidence_AreImmutableAndDetached()
        {
            var sourceFacts = new List<InfernalDefenseBeatReadabilityFact>(
                StarterInfernalDefenseBeatReadability.AshenSpur.Facts);
            var copy = new InfernalDefenseBeatReadabilityProfile(
                StarterInfernalDefensePacing.AshenSpur.LayoutId,
                sourceFacts);
            sourceFacts.Clear();

            Assert.That(copy.Facts.Count, Is.EqualTo(5));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)copy.Facts).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterInfernalDefenseBeatReadability.All).RemoveAt(0));
            var issues = InfernalDefenseBeatReadabilityValidator.Validate(null).Issues;
            Assert.Throws<NotSupportedException>(() => ((IList)issues).Clear());
        }

        [Test]
        public void Validate_FailsClosedForLayoutCoverageAndBeatIdentityDefects()
        {
            var source = StarterInfernalDefenseBeatReadability.CinderFork;
            AssertIssues(
                InfernalDefenseBeatReadabilityValidator.Validate(null),
                InfernalDefenseBeatReadabilityValidationIssue.ProfileMissing);
            AssertContains(
                Copy(source, layoutId: "Invalid Layout"),
                InfernalDefenseBeatReadabilityValidationIssue.LayoutIdInvalid);
            AssertContains(
                Copy(source, layoutId: "realmraiders.infernal-defense.unknown"),
                InfernalDefenseBeatReadabilityValidationIssue.LayoutNotFound);
            AssertContains(
                Copy(source, facts: RemoveAt(source.Facts, 4)),
                InfernalDefenseBeatReadabilityValidationIssue.FactCardinalityInvalid);
            AssertContains(
                Copy(source, facts: Replace(source.Facts, 1, null)),
                InfernalDefenseBeatReadabilityValidationIssue.FactMissing);
            AssertContains(
                Copy(source, facts: Replace(
                    source.Facts,
                    1,
                    CopyFact(source.Facts[1], beatId: "Bad beat"))),
                InfernalDefenseBeatReadabilityValidationIssue.BeatIdInvalid);
            AssertContains(
                Copy(source, facts: Replace(
                    source.Facts,
                    1,
                    CopyFact(source.Facts[1], beatId: source.Facts[0].BeatId))),
                InfernalDefenseBeatReadabilityValidationIssue.BeatIdDuplicate);
            AssertContains(
                Copy(source, facts: Replace(
                    source.Facts,
                    1,
                    CopyFact(source.Facts[1], beatId: "unknown.beat"))),
                InfernalDefenseBeatReadabilityValidationIssue.BeatUnknown);
            AssertContains(
                Copy(source, facts: Swap(source.Facts, 1, 2)),
                InfernalDefenseBeatReadabilityValidationIssue.BeatOrderMismatch);
        }

        [Test]
        public void Validate_FailsClosedForBeatSemanticsAndClosedEmphasisDefects()
        {
            var source = StarterInfernalDefenseBeatReadability.EmberCircuit;
            AssertContains(
                WithFact(source, 1, CopyFact(source.Facts[1], roleId: "unknown.role")),
                InfernalDefenseBeatReadabilityValidationIssue.BeatRoleMismatch);
            AssertContains(
                WithFact(source, 1, CopyFact(
                    source.Facts[1],
                    kind: InfernalDefensePacingBeatKind.Unknown)),
                InfernalDefenseBeatReadabilityValidationIssue.BeatKindMismatch,
                InfernalDefenseBeatReadabilityValidationIssue.EmphasisRoleMismatch);
            AssertContains(
                WithFact(source, 2, CopyFact(
                    source.Facts[2],
                    requirement: InfernalDefensePacingRequirement.Required)),
                InfernalDefenseBeatReadabilityValidationIssue.BeatRequirementMismatch);
            AssertContains(
                WithFact(source, 1, CopyFact(
                    source.Facts[1],
                    emphasis: (InfernalDefenseBeatEmphasis)99)),
                InfernalDefenseBeatReadabilityValidationIssue.EmphasisInvalid);
            AssertContains(
                WithFact(source, 1, CopyFact(
                    source.Facts[1],
                    emphasis: InfernalDefenseBeatEmphasis.Hazard)),
                InfernalDefenseBeatReadabilityValidationIssue.EmphasisRoleMismatch);
        }

        [Test]
        public void Validate_FailsClosedForUnreadableMisleadingAndPromiseCopy()
        {
            var source = StarterInfernalDefenseBeatReadability.AshenSpur;
            AssertContains(
                WithFact(source, 0, CopyFact(source.Facts[0], primaryCue: string.Empty)),
                InfernalDefenseBeatReadabilityValidationIssue.PrimaryCueMissing);
            AssertContains(
                WithFact(source, 0, CopyFact(source.Facts[0], primaryCue: "ABC")),
                InfernalDefenseBeatReadabilityValidationIssue.PrimaryCueLengthInvalid,
                InfernalDefenseBeatReadabilityValidationIssue.CopySemanticMismatch);
            AssertContains(
                WithFact(source, 0, CopyFact(source.Facts[0], tacticalHint: "TOO")),
                InfernalDefenseBeatReadabilityValidationIssue.TacticalHintLengthInvalid);
            AssertContains(
                WithFact(source, 0, CopyFact(
                    source.Facts[0],
                    primaryCue: "INVADERS\nAT ENTRY")),
                InfernalDefenseBeatReadabilityValidationIssue.CopyCharacterInvalid);
            AssertContains(
                WithFact(source, 0, CopyFact(
                    source.Facts[0],
                    primaryCue: " INVADERS AT ENTRY")),
                InfernalDefenseBeatReadabilityValidationIssue.CopySpacingInvalid);
            AssertContains(
                WithFact(source, 1, CopyFact(
                    source.Facts[1],
                    tacticalHint: "HELLHOUND SPUR PRESSURE")),
                InfernalDefenseBeatReadabilityValidationIssue.CopySemanticMismatch);
            AssertContains(
                WithFact(source, 2, CopyFact(
                    source.Facts[2],
                    tacticalHint: "FLAME ACTIVATED AUTOMATIC REWARD")),
                InfernalDefenseBeatReadabilityValidationIssue.PromiseCopyInvalid);
            AssertContains(
                WithFact(source, 4, CopyFact(
                    source.Facts[4],
                    tacticalHint: "FINAL HEART EXPOSED")),
                InfernalDefenseBeatReadabilityValidationIssue.PromiseCopyInvalid);
        }

        [Test]
        public void Validate_ClosedIntentVocabularyRejectsContradictionsAndPrematureClaims()
        {
            var ashen = StarterInfernalDefenseBeatReadability.AshenSpur;
            var cinder = StarterInfernalDefenseBeatReadability.CinderFork;

            AssertRejectedPrimaryVariants(
                ashen,
                2,
                "FLAME MAY BE SKIPPED",
                "FLAME CAN BE SKIPPED",
                "FLAME NOT REQUIRED");
            AssertRejectedPrimaryVariants(
                cinder,
                2,
                "OPTIONAL FLAME REQUIRED",
                "OPTIONAL FLAME MANDATORY");
            AssertRejectedPrimaryVariants(
                cinder,
                3,
                "BRUTE POSSESSION COMPLETE",
                "BRUTE POSSESSION SUCCEEDED",
                "BRUTE NOW POSSESSED");
            AssertRejectedPrimaryVariants(
                cinder,
                4,
                "FINAL HEART UNLOCKED",
                "FINAL HEART OPEN",
                "FINAL HEART AVAILABLE",
                "FINAL HEART EXPOSED",
                "FINAL HEART REWARD GRANTED");

            var automaticFlame = WithFact(
                cinder,
                2,
                CopyFact(
                    cinder.Facts[2],
                    primaryCue: "FLAME ACTIVATES AUTOMATICALLY"));
            AssertContains(
                automaticFlame,
                InfernalDefenseBeatReadabilityValidationIssue.PromiseCopyInvalid,
                InfernalDefenseBeatReadabilityValidationIssue.CopySemanticMismatch);
        }

        [Test]
        public void ValidateCatalogue_RejectsDuplicateCoverageAndIdFreeSemanticCopies()
        {
            var cinder = StarterInfernalDefenseBeatReadability.CinderFork;
            var ember = StarterInfernalDefenseBeatReadability.EmberCircuit;
            var copiedEmber = CopyWithOtherCopy(ember, cinder);
            Assert.That(
                InfernalDefenseBeatReadabilityValidator.Validate(copiedEmber).IsValid,
                Is.True);

            var duplicateSignature = InfernalDefenseBeatReadabilityValidator
                .ValidateCatalogue(new[]
                {
                    StarterInfernalDefenseBeatReadability.AshenSpur,
                    cinder,
                    copiedEmber
                });
            Assert.That(
                duplicateSignature.Issues,
                Does.Contain(
                    InfernalDefenseBeatReadabilityCatalogueIssue
                        .SemanticSignatureDuplicate));

            var duplicateLayout = InfernalDefenseBeatReadabilityValidator
                .ValidateCatalogue(new[]
                {
                    StarterInfernalDefenseBeatReadability.AshenSpur,
                    cinder,
                    cinder
                });
            Assert.That(
                duplicateLayout.Issues,
                Does.Contain(InfernalDefenseBeatReadabilityCatalogueIssue.LayoutIdDuplicate));
            Assert.That(
                duplicateLayout.Issues,
                Does.Contain(
                    InfernalDefenseBeatReadabilityCatalogueIssue.LayoutCoverageInvalid));
            AssertIssues(
                InfernalDefenseBeatReadabilityValidator.ValidateCatalogue(null),
                InfernalDefenseBeatReadabilityCatalogueIssue.CatalogueMissing);
        }

        private static void AssertSnapshot(
            InfernalDefenseBeatReadabilityProfile profile,
            InfernalDefensePacingRecipe recipe,
            params ExpectedFact[] expected)
        {
            Assert.That(
                InfernalDefenseBeatReadabilityValidator.Validate(profile).IsValid,
                Is.True,
                profile.LayoutId);
            Assert.That(profile.LayoutId, Is.EqualTo(recipe.LayoutId));
            Assert.That(profile.Facts.Count, Is.EqualTo(expected.Length));
            for (var index = 0; index < expected.Length; index++)
            {
                var fact = profile.Facts[index];
                var recipeBeat = recipe.Beats[index];
                var expectedFact = expected[index];
                Assert.That(fact.BeatId, Is.EqualTo(expectedFact.BeatId));
                Assert.That(fact.BeatId, Is.EqualTo(recipeBeat.BeatId));
                Assert.That(fact.RoleId, Is.EqualTo(recipeBeat.RoleId));
                Assert.That(fact.Kind, Is.EqualTo(recipeBeat.Kind));
                Assert.That(fact.Requirement, Is.EqualTo(recipeBeat.Requirement));
                Assert.That(fact.PrimaryCue, Is.EqualTo(expectedFact.PrimaryCue));
                Assert.That(fact.TacticalHint, Is.EqualTo(expectedFact.TacticalHint));
                Assert.That(fact.Emphasis, Is.EqualTo(expectedFact.Emphasis));
            }
        }

        private static InfernalDefenseBeatReadabilityProfile CopyWithOtherCopy(
            InfernalDefenseBeatReadabilityProfile identitySource,
            InfernalDefenseBeatReadabilityProfile copySource)
        {
            var facts = new InfernalDefenseBeatReadabilityFact[identitySource.Facts.Count];
            for (var index = 0; index < facts.Length; index++)
            {
                facts[index] = CopyFact(
                    identitySource.Facts[index],
                    primaryCue: copySource.Facts[index].PrimaryCue,
                    tacticalHint: copySource.Facts[index].TacticalHint,
                    emphasis: copySource.Facts[index].Emphasis);
            }

            return Copy(identitySource, facts: facts);
        }

        private static void AssertRejectedPrimaryVariants(
            InfernalDefenseBeatReadabilityProfile source,
            int factIndex,
            params string[] primaryCues)
        {
            foreach (var primaryCue in primaryCues)
            {
                var profile = WithFact(
                    source,
                    factIndex,
                    CopyFact(source.Facts[factIndex], primaryCue: primaryCue));
                AssertContains(
                    profile,
                    InfernalDefenseBeatReadabilityValidationIssue.CopySemanticMismatch);
            }
        }

        private static InfernalDefenseBeatReadabilityProfile WithFact(
            InfernalDefenseBeatReadabilityProfile source,
            int index,
            InfernalDefenseBeatReadabilityFact fact)
        {
            return Copy(source, facts: Replace(source.Facts, index, fact));
        }

        private static InfernalDefenseBeatReadabilityProfile Copy(
            InfernalDefenseBeatReadabilityProfile source,
            string layoutId = null,
            IReadOnlyList<InfernalDefenseBeatReadabilityFact> facts = null)
        {
            return new InfernalDefenseBeatReadabilityProfile(
                layoutId ?? source.LayoutId,
                facts ?? source.Facts);
        }

        private static InfernalDefenseBeatReadabilityFact CopyFact(
            InfernalDefenseBeatReadabilityFact source,
            string beatId = null,
            string roleId = null,
            InfernalDefensePacingBeatKind? kind = null,
            InfernalDefensePacingRequirement? requirement = null,
            string primaryCue = null,
            string tacticalHint = null,
            InfernalDefenseBeatEmphasis? emphasis = null)
        {
            return new InfernalDefenseBeatReadabilityFact(
                beatId ?? source.BeatId,
                roleId ?? source.RoleId,
                kind ?? source.Kind,
                requirement ?? source.Requirement,
                primaryCue ?? source.PrimaryCue,
                tacticalHint ?? source.TacticalHint,
                emphasis ?? source.Emphasis);
        }

        private static IReadOnlyList<InfernalDefenseBeatReadabilityFact> RemoveAt(
            IReadOnlyList<InfernalDefenseBeatReadabilityFact> source,
            int removeIndex)
        {
            var result = new List<InfernalDefenseBeatReadabilityFact>();
            for (var index = 0; index < source.Count; index++)
            {
                if (index != removeIndex)
                {
                    result.Add(source[index]);
                }
            }

            return result;
        }

        private static IReadOnlyList<InfernalDefenseBeatReadabilityFact> Replace(
            IReadOnlyList<InfernalDefenseBeatReadabilityFact> source,
            int replaceIndex,
            InfernalDefenseBeatReadabilityFact replacement)
        {
            var result = new InfernalDefenseBeatReadabilityFact[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                result[index] = index == replaceIndex ? replacement : source[index];
            }

            return result;
        }

        private static IReadOnlyList<InfernalDefenseBeatReadabilityFact> Swap(
            IReadOnlyList<InfernalDefenseBeatReadabilityFact> source,
            int first,
            int second)
        {
            var result = new InfernalDefenseBeatReadabilityFact[source.Count];
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
            var lookup = StarterInfernalDefensePacingResolver.ResolveExact(layoutId);
            Assert.That(lookup.Found, Is.True, layoutId);
            return lookup.Recipe;
        }

        private static void AssertContains(
            InfernalDefenseBeatReadabilityProfile profile,
            params InfernalDefenseBeatReadabilityValidationIssue[] expectedIssues)
        {
            var validation = InfernalDefenseBeatReadabilityValidator.Validate(profile);
            Assert.That(validation.IsValid, Is.False);
            foreach (var issue in expectedIssues)
            {
                Assert.That(validation.Issues, Does.Contain(issue));
            }
        }

        private static void AssertIssues(
            InfernalDefenseBeatReadabilityValidationResult result,
            params InfernalDefenseBeatReadabilityValidationIssue[] expectedIssues)
        {
            CollectionAssert.AreEqual(expectedIssues, result.Issues);
        }

        private static void AssertIssues(
            InfernalDefenseBeatReadabilityCatalogueValidationResult result,
            params InfernalDefenseBeatReadabilityCatalogueIssue[] expectedIssues)
        {
            CollectionAssert.AreEqual(expectedIssues, result.Issues);
        }

        private static ExpectedFact Expected(
            string beatId,
            string primaryCue,
            string tacticalHint,
            InfernalDefenseBeatEmphasis emphasis)
        {
            return new ExpectedFact(beatId, primaryCue, tacticalHint, emphasis);
        }

        private sealed class ExpectedFact
        {
            public ExpectedFact(
                string beatId,
                string primaryCue,
                string tacticalHint,
                InfernalDefenseBeatEmphasis emphasis)
            {
                BeatId = beatId;
                PrimaryCue = primaryCue;
                TacticalHint = tacticalHint;
                Emphasis = emphasis;
            }

            public string BeatId { get; }
            public string PrimaryCue { get; }
            public string TacticalHint { get; }
            public InfernalDefenseBeatEmphasis Emphasis { get; }
        }
    }
}
