using System;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.SylvanEncounterPacing.Tests
{
    public sealed class StarterSylvanEncounterPacingTests
    {
        [Test]
        public void All_UsesTheExactStarterLayoutOrderAndValidatesEveryRecipe()
        {
            CollectionAssert.AreEqual(
                new[]
                {
                    StarterSylvanRealmLayouts.AncientCrossroadsId,
                    StarterSylvanRealmLayouts.ForkedCanopyId,
                    StarterSylvanRealmLayouts.SerpentRootsId
                },
                LayoutIds(StarterSylvanEncounterPacing.All));
            CollectionAssert.AreEqual(
                new[]
                {
                    "OPTIONAL WOLF BRANCH • ROOT OR MOONWELL BEFORE ENT",
                    "HIGH WOLF AND ROOT RISK • OR MOONWELL TO ENT",
                    "WOLF • ROOT • ENT • MOONWELL • HEART"
                },
                TacticalSummaries(StarterSylvanEncounterPacing.All));

            foreach (var recipe in StarterSylvanEncounterPacing.All)
            {
                var validation = SylvanEncounterPacingEvidence.Validate(recipe);

                Assert.That(validation.IsValid, Is.True, recipe.LayoutId);
                Assert.That(recipe.TacticalSummary, Is.EqualTo(recipe.TacticalSummary.ToUpperInvariant()));
                Assert.That(recipe.Beats[recipe.Beats.Count - 1].Role,
                    Is.EqualTo(SylvanRealmNodeMaterializationRole.HeartTreeObjective));
            }
        }

        [Test]
        public void AncientCrossroads_RecordsTheWolfDeadEndAndRootMoonwellChoice()
        {
            var recipe = StarterSylvanEncounterPacing.AncientCrossroads;

            CollectionAssert.AreEqual(
                new[]
                {
                    SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                    SylvanRealmNodeMaterializationRole.RootPathHazard,
                    SylvanRealmNodeMaterializationRole.MoonwellRecovery,
                    SylvanRealmNodeMaterializationRole.EntGroveEncounter,
                    SylvanRealmNodeMaterializationRole.HeartTreeObjective
                },
                BeatRoles(recipe));
            Assert.That(recipe.Choices, Has.Count.EqualTo(2));
            Assert.That(recipe.Beats[0].Requirement,
                Is.EqualTo(SylvanEncounterPacingRequirement.Optional));
            Assert.That(recipe.Beats[1].Requirement,
                Is.EqualTo(SylvanEncounterPacingRequirement.Optional));
            Assert.That(recipe.Beats[2].Requirement,
                Is.EqualTo(SylvanEncounterPacingRequirement.Optional));
            Assert.That(recipe.Beats[3].Requirement,
                Is.EqualTo(SylvanEncounterPacingRequirement.Required));
            Assert.That(recipe.Choices[0].Kind,
                Is.EqualTo(SylvanEncounterPacingChoiceKind.OptionalBranch));
            CollectionAssert.AreEqual(
                new[]
                {
                    SylvanRealmNodeMaterializationRole.LandmarkJunction,
                    SylvanRealmNodeMaterializationRole.WolfGroveEncounter
                },
                recipe.Choices[0].FirstRoute);
            Assert.That(recipe.Choices[1].Kind,
                Is.EqualTo(SylvanEncounterPacingChoiceKind.RouteChoice));
            CollectionAssert.AreEqual(
                new[]
                {
                    SylvanRealmNodeMaterializationRole.LandmarkJunction,
                    SylvanRealmNodeMaterializationRole.RootPathHazard,
                    SylvanRealmNodeMaterializationRole.EntGroveEncounter
                },
                recipe.Choices[1].FirstRoute);
            CollectionAssert.AreEqual(
                new[]
                {
                    SylvanRealmNodeMaterializationRole.LandmarkJunction,
                    SylvanRealmNodeMaterializationRole.MoonwellRecovery,
                    SylvanRealmNodeMaterializationRole.EntGroveEncounter
                },
                recipe.Choices[1].SecondRoute);
        }

        [Test]
        public void ForkedCanopy_RecordsHighRiskAndMoonwellRoutesThatConvergeAtEnt()
        {
            var choice = StarterSylvanEncounterPacing.ForkedCanopy.Choices[0];

            Assert.That(choice.Kind, Is.EqualTo(SylvanEncounterPacingChoiceKind.RouteChoice));
            CollectionAssert.AreEqual(
                new[]
                {
                    SylvanRealmNodeMaterializationRole.LandmarkJunction,
                    SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                    SylvanRealmNodeMaterializationRole.RootPathHazard,
                    SylvanRealmNodeMaterializationRole.EntGroveEncounter
                },
                choice.FirstRoute);
            CollectionAssert.AreEqual(
                new[]
                {
                    SylvanRealmNodeMaterializationRole.LandmarkJunction,
                    SylvanRealmNodeMaterializationRole.MoonwellRecovery,
                    SylvanRealmNodeMaterializationRole.EntGroveEncounter
                },
                choice.SecondRoute);
        }

        [Test]
        public void SerpentRoots_RecordsTheExactLinearEscalation()
        {
            var recipe = StarterSylvanEncounterPacing.SerpentRoots;

            Assert.That(recipe.Choices, Is.Empty);
            CollectionAssert.AreEqual(
                new[]
                {
                    SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                    SylvanRealmNodeMaterializationRole.RootPathHazard,
                    SylvanRealmNodeMaterializationRole.EntGroveEncounter,
                    SylvanRealmNodeMaterializationRole.MoonwellRecovery,
                    SylvanRealmNodeMaterializationRole.HeartTreeObjective
                },
                BeatRoles(recipe));
            foreach (var beat in recipe.Beats)
            {
                Assert.That(beat.Requirement,
                    Is.EqualTo(SylvanEncounterPacingRequirement.Required));
            }
        }

        [Test]
        public void FindByLayoutId_IsExactOrdinalAndReturnsTheCachedRecipe()
        {
            var result = SylvanEncounterPacingEvidence.FindByLayoutId(
                StarterSylvanRealmLayouts.ForkedCanopyId);

            Assert.That(result.Status, Is.EqualTo(SylvanEncounterPacingLookupStatus.Found));
            Assert.That(result.Recipe, Is.SameAs(StarterSylvanEncounterPacing.ForkedCanopy));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" realmraiders.sylvan-layout.ancient-crossroads")]
        [TestCase("realmraiders.sylvan-layout.ancient-crossroads ")]
        [TestCase("REALMRAIDERS.SYLVAN-LAYOUT.ANCIENT-CROSSROADS")]
        public void FindByLayoutId_RejectsNullAndNonCanonicalIds(string layoutId)
        {
            var result = SylvanEncounterPacingEvidence.FindByLayoutId(layoutId);

            Assert.That(result.Status, Is.EqualTo(SylvanEncounterPacingLookupStatus.InvalidLayoutId));
            Assert.That(result.Recipe, Is.Null);
        }

        [Test]
        public void FindByLayoutId_RejectsUnknownStableId()
        {
            var result = SylvanEncounterPacingEvidence.FindByLayoutId("realmraiders.sylvan-layout.unknown");

            Assert.That(result.Status, Is.EqualTo(SylvanEncounterPacingLookupStatus.NotFound));
            Assert.That(result.Recipe, Is.Null);
        }

        [Test]
        public void Snapshots_AreStableReadOnlyAndDeterministic()
        {
            var beats = new[]
            {
                new SylvanEncounterPacingBeat(
                    "test.wolf",
                    SylvanRealmNodeMaterializationRole.WolfGroveEncounter,
                    SylvanEncounterPacingBeatKind.Encounter,
                    SylvanEncounterPacingRequirement.Required),
                new SylvanEncounterPacingBeat(
                    "test.heart",
                    SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                    SylvanEncounterPacingBeatKind.Objective,
                    SylvanEncounterPacingRequirement.Required)
            };
            var choices = new[]
            {
                new SylvanEncounterPacingChoice(
                    "test.branch",
                    SylvanEncounterPacingChoiceKind.OptionalBranch,
                    new[]
                    {
                        SylvanRealmNodeMaterializationRole.LandmarkJunction,
                        SylvanRealmNodeMaterializationRole.WolfGroveEncounter
                    },
                    Array.Empty<SylvanRealmNodeMaterializationRole>())
            };
            var recipe = new SylvanEncounterPacingRecipe(
                StarterSylvanRealmLayouts.AncientCrossroadsId,
                "TEST",
                beats,
                choices);

            beats[0] = null;
            choices[0] = null;

            Assert.That(recipe.Beats[0], Is.Not.Null);
            Assert.That(recipe.Choices[0], Is.Not.Null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<SylvanEncounterPacingBeat>)recipe.Beats)[0] = null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<SylvanEncounterPacingChoice>)recipe.Choices)[0] = null);
            Assert.That(StarterSylvanEncounterPacing.All,
                Is.SameAs(StarterSylvanEncounterPacing.All));
        }

        [Test]
        public void Validate_HostileRoleAndEdgeFixturesFailClosed()
        {
            var invalidRole = new SylvanEncounterPacingRecipe(
                StarterSylvanRealmLayouts.AncientCrossroadsId,
                "TEST",
                new SylvanEncounterPacingBeat[]
                {
                    new SylvanEncounterPacingBeat(
                        "test.portal",
                        SylvanRealmNodeMaterializationRole.PortalStart,
                        SylvanEncounterPacingBeatKind.Encounter,
                        SylvanEncounterPacingRequirement.Required),
                    new SylvanEncounterPacingBeat(
                        "test.heart",
                        SylvanRealmNodeMaterializationRole.HeartTreeObjective,
                        SylvanEncounterPacingBeatKind.Objective,
                        SylvanEncounterPacingRequirement.Required)
                },
                Array.Empty<SylvanEncounterPacingChoice>());
            var invalidEdge = new SylvanEncounterPacingRecipe(
                StarterSylvanRealmLayouts.SerpentRootsId,
                "TEST",
                StarterSylvanEncounterPacing.SerpentRoots.Beats,
                new SylvanEncounterPacingChoice[]
                {
                    new SylvanEncounterPacingChoice(
                        "test.non-edge",
                        SylvanEncounterPacingChoiceKind.RouteChoice,
                        new[]
                        {
                            SylvanRealmNodeMaterializationRole.LandmarkJunction,
                            SylvanRealmNodeMaterializationRole.EntGroveEncounter
                        },
                        new[]
                        {
                            SylvanRealmNodeMaterializationRole.LandmarkJunction,
                            SylvanRealmNodeMaterializationRole.WolfGroveEncounter
                        })
                });

            var roleResult = SylvanEncounterPacingEvidence.Validate(invalidRole);
            var edgeResult = SylvanEncounterPacingEvidence.Validate(invalidEdge);

            CollectionAssert.Contains(
                roleResult.Issues,
                SylvanEncounterPacingValidationIssue.BeatRoleInvalid);
            CollectionAssert.Contains(
                roleResult.Issues,
                SylvanEncounterPacingValidationIssue.BeatKindInvalid);
            CollectionAssert.Contains(
                edgeResult.Issues,
                SylvanEncounterPacingValidationIssue.ChoiceEdgeInvalid);
        }

        [Test]
        public void Validate_UnknownLayoutFailsClosed()
        {
            var recipe = new SylvanEncounterPacingRecipe(
                "realmraiders.sylvan-layout.unknown",
                "TEST",
                Array.Empty<SylvanEncounterPacingBeat>(),
                Array.Empty<SylvanEncounterPacingChoice>());

            var result = SylvanEncounterPacingEvidence.Validate(recipe);

            CollectionAssert.AreEqual(
                new[]
                {
                    SylvanEncounterPacingValidationIssue.LayoutNotFound,
                    SylvanEncounterPacingValidationIssue.BeatCardinalityInvalid,
                    SylvanEncounterPacingValidationIssue.HeartSequenceInvalid
                },
                result.Issues);
        }

        private static string[] LayoutIds(IReadOnlyList<SylvanEncounterPacingRecipe> recipes)
        {
            var layoutIds = new string[recipes.Count];
            for (var index = 0; index < recipes.Count; index++)
            {
                layoutIds[index] = recipes[index].LayoutId;
            }

            return layoutIds;
        }

        private static SylvanRealmNodeMaterializationRole[] BeatRoles(
            SylvanEncounterPacingRecipe recipe)
        {
            var roles = new SylvanRealmNodeMaterializationRole[recipe.Beats.Count];
            for (var index = 0; index < recipe.Beats.Count; index++)
            {
                roles[index] = recipe.Beats[index].Role;
            }

            return roles;
        }

        private static string[] TacticalSummaries(
            IReadOnlyList<SylvanEncounterPacingRecipe> recipes)
        {
            var summaries = new string[recipes.Count];
            for (var index = 0; index < recipes.Count; index++)
            {
                summaries[index] = recipes[index].TacticalSummary;
            }

            return summaries;
        }
    }
}
