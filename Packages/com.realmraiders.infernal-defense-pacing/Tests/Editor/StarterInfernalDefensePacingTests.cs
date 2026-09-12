using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.InfernalDefensePacing.Tests
{
    public sealed class StarterInfernalDefensePacingTests
    {
        [Test]
        public void Catalogue_UsesCachedIdentityExactLayoutsAndDistinctPacingSignatures()
        {
            var catalogue = InfernalDefensePacingValidator.ValidateCatalogue(
                StarterInfernalDefensePacing.All);
            var signatures = new[]
            {
                InfernalDefensePacingValidator.CreatePacingSignature(
                    StarterInfernalDefensePacing.AshenSpur),
                InfernalDefensePacingValidator.CreatePacingSignature(
                    StarterInfernalDefensePacing.CinderFork),
                InfernalDefensePacingValidator.CreatePacingSignature(
                    StarterInfernalDefensePacing.EmberCircuit)
            };

            Assert.That(catalogue.IsValid, Is.True);
            Assert.That(StarterInfernalDefensePacing.All[0],
                Is.SameAs(StarterInfernalDefensePacing.AshenSpur));
            Assert.That(StarterInfernalDefensePacing.All[1],
                Is.SameAs(StarterInfernalDefensePacing.CinderFork));
            Assert.That(StarterInfernalDefensePacing.All[2],
                Is.SameAs(StarterInfernalDefensePacing.EmberCircuit));
            Assert.That(signatures[0], Is.Not.EqualTo(signatures[1]));
            Assert.That(signatures[0], Is.Not.EqualTo(signatures[2]));
            Assert.That(signatures[1], Is.Not.EqualTo(signatures[2]));
        }

        [TestCaseSource(nameof(CachedRecipeSnapshots))]
        public void CachedRecipe_PreservesExactImmutableBeatAndIntentSnapshot(
            PacingRecipeSnapshot expected)
        {
            var result = StarterInfernalDefensePacingResolver.ResolveExact(expected.LayoutId);
            var recipe = result.Recipe;

            Assert.That(result.Status, Is.EqualTo(InfernalDefensePacingLookupStatus.Found));
            Assert.That(recipe.LayoutId, Is.EqualTo(expected.LayoutId));
            Assert.That(recipe.TacticalIntent, Is.EqualTo(expected.TacticalIntent));
            Assert.That(recipe.Beats.Count, Is.EqualTo(expected.Beats.Count));
            for (var index = 0; index < expected.Beats.Count; index++)
            {
                var actualBeat = recipe.Beats[index];
                var expectedBeat = expected.Beats[index];

                Assert.That(actualBeat.BeatId, Is.EqualTo(expectedBeat.BeatId));
                Assert.That(actualBeat.RoleId, Is.EqualTo(expectedBeat.RoleId));
                Assert.That(actualBeat.Kind, Is.EqualTo(expectedBeat.Kind));
                Assert.That(actualBeat.Requirement, Is.EqualTo(expectedBeat.Requirement));
                CollectionAssert.AreEqual(
                    expectedBeat.DependsOnBeatIds,
                    actualBeat.DependsOnBeatIds);
            }
        }

        [Test]
        public void ResolveExact_ReturnsOnlyExactCachedRecipes()
        {
            var ashen = StarterInfernalDefensePacingResolver.ResolveExact(
                StarterInfernalDefenseLayouts.AshenSpurId);
            var cinder = StarterInfernalDefensePacingResolver.ResolveExact(
                StarterInfernalDefenseLayouts.CinderForkId);
            var ember = StarterInfernalDefensePacingResolver.ResolveExact(
                StarterInfernalDefenseLayouts.EmberCircuitId);
            var unknown = StarterInfernalDefensePacingResolver.ResolveExact(
                "realmraiders.infernal-defense.unknown");
            var changedCase = StarterInfernalDefensePacingResolver.ResolveExact(
                "Realmraiders.infernal-defense.ashen-spur");
            var padded = StarterInfernalDefensePacingResolver.ResolveExact(
                StarterInfernalDefenseLayouts.AshenSpurId + " ");

            Assert.That(ashen.Status, Is.EqualTo(InfernalDefensePacingLookupStatus.Found));
            Assert.That(ashen.Recipe, Is.SameAs(StarterInfernalDefensePacing.AshenSpur));
            Assert.That(cinder.Recipe, Is.SameAs(StarterInfernalDefensePacing.CinderFork));
            Assert.That(ember.Recipe, Is.SameAs(StarterInfernalDefensePacing.EmberCircuit));
            Assert.That(unknown.Status, Is.EqualTo(InfernalDefensePacingLookupStatus.NotFound));
            Assert.That(changedCase.Status,
                Is.EqualTo(InfernalDefensePacingLookupStatus.LayoutIdInvalid));
            Assert.That(padded.Status,
                Is.EqualTo(InfernalDefensePacingLookupStatus.LayoutIdInvalid));
        }

        [TestCaseSource(nameof(CachedRecipes))]
        public void CachedRecipe_UsesOnlyFactualLayoutRolesAndSafeRequiredBeats(
            InfernalDefensePacingRecipe recipe)
        {
            var layout = ResolveLayout(recipe.LayoutId);
            var layoutRoleIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var node in layout.Nodes)
            {
                layoutRoleIds.Add(node.GameplayRoleId);
            }

            Assert.That(InfernalDefensePacingValidator.Validate(recipe).IsValid, Is.True);
            Assert.That(recipe.Beats.Count, Is.EqualTo(5));
            Assert.That(recipe.Beats[recipe.Beats.Count - 1].RoleId,
                Is.EqualTo(StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId));
            foreach (var beat in recipe.Beats)
            {
                Assert.That(layoutRoleIds.Contains(beat.RoleId), Is.True);
            }
        }

        [Test]
        public void CachedSnapshots_AreImmutable()
        {
            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterInfernalDefensePacing.All).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterInfernalDefensePacing.AshenSpur.Beats).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterInfernalDefensePacing.AshenSpur.Beats[1].DependsOnBeatIds)
                    .RemoveAt(0));
        }

        [Test]
        public void Validate_FailsClosedForNullMalformedDuplicateUnknownAndWrongLayout()
        {
            var source = StarterInfernalDefensePacing.AshenSpur;
            var malformed = CopyWithBeats(
                source,
                ReplaceBeat(
                    source.Beats,
                    1,
                    CopyBeat(
                        source.Beats[1],
                        kind: InfernalDefensePacingBeatKind.Unknown)));
            var duplicate = CopyWithBeats(
                source,
                ReplaceBeat(
                    source.Beats,
                    1,
                    CopyBeat(source.Beats[1], beatId: source.Beats[0].BeatId)));
            var unknown = CopyWithBeats(
                source,
                ReplaceBeat(
                    source.Beats,
                    1,
                    CopyBeat(
                        source.Beats[1],
                        roleId: "realmraiders.infernal-defense.unknown-role")));
            var wrongLayout = new InfernalDefensePacingRecipe(
                "realmraiders.infernal-defense.unknown",
                source.TacticalIntent,
                source.Beats);

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.RecipeMissing
                },
                InfernalDefensePacingValidator.Validate(null).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.BeatKindInvalid
                },
                InfernalDefensePacingValidator.Validate(malformed).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.BeatIdDuplicate
                },
                InfernalDefensePacingValidator.Validate(duplicate).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.BeatRoleInvalid,
                    InfernalDefensePacingValidationIssue.BeatKindInvalid,
                    InfernalDefensePacingValidationIssue.PacingRoleCoverageInvalid
                },
                InfernalDefensePacingValidator.Validate(unknown).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.LayoutNotFound
                },
                InfernalDefensePacingValidator.Validate(wrongLayout).Issues);
        }

        [Test]
        public void Validate_FailsClosedForCycleAndNonfinalHeartWithStableIssues()
        {
            var source = StarterInfernalDefensePacing.AshenSpur;
            var cycle = CopyWithBeats(
                source,
                ReplaceBeat(
                    source.Beats,
                    0,
                    CopyBeat(
                        source.Beats[0],
                        dependsOnBeatIds: new[]
                        {
                            source.Beats[4].BeatId
                        })));
            var nonfinalHeart = CopyWithBeats(
                source,
                SwapBeats(source.Beats, 3, 4));

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.BeatDependencyInvalid,
                    InfernalDefensePacingValidationIssue.BeatDependencyCycle
                },
                InfernalDefensePacingValidator.Validate(cycle).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.BeatDependencyInvalid,
                    InfernalDefensePacingValidationIssue.HeartFinalInvalid
                },
                InfernalDefensePacingValidator.Validate(nonfinalHeart).Issues);
        }

        [Test]
        public void Validate_FailsClosedWhenAnUnsafeRoleIsMarkedRequired()
        {
            var source = StarterInfernalDefensePacing.AshenSpur;
            var unreachableRequiredBeat = CopyWithBeats(
                source,
                ReplaceBeat(
                    source.Beats,
                    1,
                    CopyBeat(
                        source.Beats[1],
                        requirement: InfernalDefensePacingRequirement.Required)));

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.RequiredBeatNotHeartPrerequisite,
                    InfernalDefensePacingValidationIssue.RequiredBeatUnreachable
                },
                InfernalDefensePacingValidator.Validate(unreachableRequiredBeat).Issues);
        }

        [Test]
        public void Validate_FailsClosedForOptionalEntryOrFinalHeart()
        {
            var source = StarterInfernalDefensePacing.CinderFork;
            var optionalEntry = CopyWithBeats(
                source,
                ReplaceBeat(
                    source.Beats,
                    0,
                    CopyBeat(
                        source.Beats[0],
                        requirement: InfernalDefensePacingRequirement.Optional)));
            var optionalHeart = CopyWithBeats(
                source,
                ReplaceBeat(
                    source.Beats,
                    4,
                    CopyBeat(
                        source.Beats[4],
                        requirement: InfernalDefensePacingRequirement.Optional)));

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.EntryRequirementInvalid,
                    InfernalDefensePacingValidationIssue.RequiredBeatDependsOnOptional
                },
                InfernalDefensePacingValidator.Validate(optionalEntry).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.HeartRequirementInvalid
                },
                InfernalDefensePacingValidator.Validate(optionalHeart).Issues);
        }

        [Test]
        public void Validate_FailsClosedForRequiredDirectAndTransitiveOptionalPrerequisites()
        {
            var source = StarterInfernalDefensePacing.AshenSpur;
            var directAndTransitiveOptionalDependency = CopyWithBeats(
                source,
                ReplaceBeat(
                    source.Beats,
                    2,
                    CopyBeat(
                        source.Beats[2],
                        dependsOnBeatIds: new[]
                        {
                            source.Beats[1].BeatId
                        })));

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.RequiredBeatDependsOnOptional
                },
                InfernalDefensePacingValidator.Validate(
                    directAndTransitiveOptionalDependency).Issues);
        }

        [Test]
        public void Validate_FailsClosedWhenARequiredBeatIsNotAHeartPrerequisite()
        {
            var source = StarterInfernalDefensePacing.EmberCircuit;
            var requiredOrphan = CopyWithBeats(
                source,
                ReplaceBeat(
                    source.Beats,
                    2,
                    CopyBeat(
                        source.Beats[2],
                        requirement: InfernalDefensePacingRequirement.Required)));

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingValidationIssue.RequiredBeatNotHeartPrerequisite
                },
                InfernalDefensePacingValidator.Validate(requiredOrphan).Issues);
        }

        [Test]
        public void ValidateCatalogue_RejectsDuplicatePacingSignatures()
        {
            var emberWithCinderRhythm = CopyWithLayoutId(
                StarterInfernalDefensePacing.CinderFork,
                StarterInfernalDefenseLayouts.EmberCircuitId);
            var result = InfernalDefensePacingValidator.ValidateCatalogue(
                new InfernalDefensePacingRecipe[]
                {
                    StarterInfernalDefensePacing.AshenSpur,
                    StarterInfernalDefensePacing.CinderFork,
                    emberWithCinderRhythm
                });

            Assert.That(InfernalDefensePacingValidator.Validate(emberWithCinderRhythm).IsValid,
                Is.True);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalDefensePacingCatalogueValidationIssue.PacingSignatureDuplicate
                },
                result.Issues);
        }

        private static IEnumerable CachedRecipes
        {
            get
            {
                foreach (var recipe in StarterInfernalDefensePacing.All)
                {
                    yield return new TestCaseData(recipe).SetName(recipe.LayoutId);
                }
            }
        }

        private static IEnumerable CachedRecipeSnapshots
        {
            get
            {
                yield return new TestCaseData(
                    Snapshot(
                        StarterInfernalDefenseLayouts.AshenSpurId,
                        "OPTIONAL HELLHOUND SPUR; COMMIT THROUGH FLAME TO THE BRUTE.",
                        BeatSnapshot(
                            "ashen-spur.entry",
                            StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                            InfernalDefensePacingBeatKind.InvaderEntry,
                            InfernalDefensePacingRequirement.Required),
                        BeatSnapshot(
                            "ashen-spur.hellhound-spur",
                            StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                            InfernalDefensePacingBeatKind.HellhoundPressure,
                            InfernalDefensePacingRequirement.Optional,
                            "ashen-spur.entry"),
                        BeatSnapshot(
                            "ashen-spur.flame-route",
                            StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                            InfernalDefensePacingBeatKind.FlameTrapOpportunity,
                            InfernalDefensePacingRequirement.Required,
                            "ashen-spur.entry"),
                        BeatSnapshot(
                            "ashen-spur.brute-window",
                            StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                            InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow,
                            InfernalDefensePacingRequirement.Required,
                            "ashen-spur.flame-route"),
                        BeatSnapshot(
                            "ashen-spur.heart-objective",
                            StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                            InfernalDefensePacingBeatKind.InfernalHeartObjective,
                            InfernalDefensePacingRequirement.Required,
                            "ashen-spur.brute-window"))).SetName("AshenSpur exact snapshot");
                yield return new TestCaseData(
                    Snapshot(
                        StarterInfernalDefenseLayouts.CinderForkId,
                        "HELLHOUND ROUTE REQUIRED; FLAME IS OPTIONAL BEFORE THE BRUTE.",
                        BeatSnapshot(
                            "cinder-fork.entry",
                            StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                            InfernalDefensePacingBeatKind.InvaderEntry,
                            InfernalDefensePacingRequirement.Required),
                        BeatSnapshot(
                            "cinder-fork.hellhound-pressure",
                            StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                            InfernalDefensePacingBeatKind.HellhoundPressure,
                            InfernalDefensePacingRequirement.Required,
                            "cinder-fork.entry"),
                        BeatSnapshot(
                            "cinder-fork.flame-choice",
                            StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                            InfernalDefensePacingBeatKind.FlameTrapOpportunity,
                            InfernalDefensePacingRequirement.Optional,
                            "cinder-fork.entry"),
                        BeatSnapshot(
                            "cinder-fork.brute-window",
                            StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                            InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow,
                            InfernalDefensePacingRequirement.Required,
                            "cinder-fork.hellhound-pressure"),
                        BeatSnapshot(
                            "cinder-fork.heart-objective",
                            StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                            InfernalDefensePacingBeatKind.InfernalHeartObjective,
                            InfernalDefensePacingRequirement.Required,
                            "cinder-fork.brute-window"))).SetName("CinderFork exact snapshot");
                yield return new TestCaseData(
                    Snapshot(
                        StarterInfernalDefenseLayouts.EmberCircuitId,
                        "CLEAR THE HELLHOUND; CHOOSE FLAME OR THE DIRECT BRUTE ROUTE.",
                        BeatSnapshot(
                            "ember-circuit.entry",
                            StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                            InfernalDefensePacingBeatKind.InvaderEntry,
                            InfernalDefensePacingRequirement.Required),
                        BeatSnapshot(
                            "ember-circuit.hellhound-pressure",
                            StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                            InfernalDefensePacingBeatKind.HellhoundPressure,
                            InfernalDefensePacingRequirement.Required,
                            "ember-circuit.entry"),
                        BeatSnapshot(
                            "ember-circuit.flame-choice",
                            StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                            InfernalDefensePacingBeatKind.FlameTrapOpportunity,
                            InfernalDefensePacingRequirement.Optional,
                            "ember-circuit.hellhound-pressure"),
                        BeatSnapshot(
                            "ember-circuit.brute-window",
                            StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                            InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow,
                            InfernalDefensePacingRequirement.Required,
                            "ember-circuit.hellhound-pressure"),
                        BeatSnapshot(
                            "ember-circuit.heart-objective",
                            StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                            InfernalDefensePacingBeatKind.InfernalHeartObjective,
                            InfernalDefensePacingRequirement.Required,
                            "ember-circuit.brute-window"))).SetName("EmberCircuit exact snapshot");
            }
        }

        private static RealmLayoutGraph ResolveLayout(string layoutId)
        {
            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                if (string.Equals(layout.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return layout;
                }
            }

            Assert.Fail("Expected the exact cached Infernal defense layout.");
            return null;
        }

        private static InfernalDefensePacingRecipe CopyWithBeats(
            InfernalDefensePacingRecipe source,
            IReadOnlyList<InfernalDefensePacingBeat> beats)
        {
            return new InfernalDefensePacingRecipe(
                source.LayoutId,
                source.TacticalIntent,
                beats);
        }

        private static PacingRecipeSnapshot Snapshot(
            string layoutId,
            string tacticalIntent,
            params PacingBeatSnapshot[] beats)
        {
            return new PacingRecipeSnapshot(layoutId, tacticalIntent, beats);
        }

        private static PacingBeatSnapshot BeatSnapshot(
            string beatId,
            string roleId,
            InfernalDefensePacingBeatKind kind,
            InfernalDefensePacingRequirement requirement,
            params string[] dependsOnBeatIds)
        {
            return new PacingBeatSnapshot(
                beatId,
                roleId,
                kind,
                requirement,
                dependsOnBeatIds);
        }

        private static InfernalDefensePacingRecipe CopyWithLayoutId(
            InfernalDefensePacingRecipe source,
            string layoutId)
        {
            return new InfernalDefensePacingRecipe(
                layoutId,
                source.TacticalIntent,
                source.Beats);
        }

        private static InfernalDefensePacingBeat CopyBeat(
            InfernalDefensePacingBeat source,
            string beatId = null,
            string roleId = null,
            InfernalDefensePacingBeatKind? kind = null,
            InfernalDefensePacingRequirement? requirement = null,
            IReadOnlyList<string> dependsOnBeatIds = null)
        {
            return new InfernalDefensePacingBeat(
                beatId ?? source.BeatId,
                roleId ?? source.RoleId,
                kind ?? source.Kind,
                requirement ?? source.Requirement,
                dependsOnBeatIds ?? source.DependsOnBeatIds);
        }

        private static IReadOnlyList<InfernalDefensePacingBeat> ReplaceBeat(
            IReadOnlyList<InfernalDefensePacingBeat> source,
            int index,
            InfernalDefensePacingBeat replacement)
        {
            var copy = new InfernalDefensePacingBeat[source.Count];
            for (var sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
            {
                copy[sourceIndex] = sourceIndex == index ? replacement : source[sourceIndex];
            }

            return copy;
        }

        private static IReadOnlyList<InfernalDefensePacingBeat> SwapBeats(
            IReadOnlyList<InfernalDefensePacingBeat> source,
            int firstIndex,
            int secondIndex)
        {
            var copy = new InfernalDefensePacingBeat[source.Count];
            for (var sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
            {
                copy[sourceIndex] = source[sourceIndex];
            }

            var first = copy[firstIndex];
            copy[firstIndex] = copy[secondIndex];
            copy[secondIndex] = first;
            return copy;
        }

        private sealed class PacingRecipeSnapshot
        {
            public PacingRecipeSnapshot(
                string layoutId,
                string tacticalIntent,
                IReadOnlyList<PacingBeatSnapshot> beats)
            {
                LayoutId = layoutId;
                TacticalIntent = tacticalIntent;
                Beats = beats;
            }

            public string LayoutId { get; }

            public string TacticalIntent { get; }

            public IReadOnlyList<PacingBeatSnapshot> Beats { get; }
        }

        private sealed class PacingBeatSnapshot
        {
            public PacingBeatSnapshot(
                string beatId,
                string roleId,
                InfernalDefensePacingBeatKind kind,
                InfernalDefensePacingRequirement requirement,
                IReadOnlyList<string> dependsOnBeatIds)
            {
                BeatId = beatId;
                RoleId = roleId;
                Kind = kind;
                Requirement = requirement;
                DependsOnBeatIds = dependsOnBeatIds;
            }

            public string BeatId { get; }

            public string RoleId { get; }

            public InfernalDefensePacingBeatKind Kind { get; }

            public InfernalDefensePacingRequirement Requirement { get; }

            public IReadOnlyList<string> DependsOnBeatIds { get; }
        }
    }
}
