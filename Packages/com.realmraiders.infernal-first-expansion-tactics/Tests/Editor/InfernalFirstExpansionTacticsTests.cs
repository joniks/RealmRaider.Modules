using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace RealmRaiders.Modules.InfernalFirstExpansionTactics.Tests
{
    public sealed class InfernalFirstExpansionTacticsTests
    {
        [Test]
        public void Catalogue_PinsExactPackRecipeAndBothOrderedActors()
        {
            AssertRecipe(
                StarterInfernalFirstExpansionTactics.AshenPackVent,
                "realmraiders.infernal-defense.ashen-spur",
                "ashen.west-vent",
                "ashen.hellhound",
                "realmraiders.infernal-expansion.ashen-pack-vent",
                "pack-pincer",
                TacticTrigger.SceneStart,
                "PACK INTERCEPT",
                null,
                0f,
                null,
                2);
            AssertActor(
                StarterInfernalFirstExpansionTactics.AshenPackVent.Actors[0],
                0,
                "realmraiders.hellhound",
                TacticRole.LeftFlank,
                0f);
            AssertActor(
                StarterInfernalFirstExpansionTactics.AshenPackVent.Actors[1],
                1,
                "realmraiders.hellhound",
                TacticRole.RightFlank,
                0.25f);
        }

        [Test]
        public void Catalogue_PinsExactSnareRecipeActorAndFlameTrapDependency()
        {
            var recipe = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            AssertRecipe(
                recipe,
                "realmraiders.infernal-defense.cinder-fork",
                "cinder.north-vent",
                "cinder.hellhound",
                "realmraiders.infernal-expansion.cinder-snare-vent",
                "snare-counterattack",
                TacticTrigger.FlameActivated,
                "SNARE COUNTER",
                null,
                0f,
                "realmraiders.infernal.flame-trap",
                1);
            AssertActor(
                recipe.Actors[0],
                0,
                "realmraiders.hellhound",
                TacticRole.TrapGuard,
                0.2f);
            Assert.That(recipe.Hazard.Automatic, Is.True);
            Assert.That(recipe.Hazard.NonBlocking, Is.True);
            Assert.That(recipe.Hazard.Bypassable, Is.True);
        }

        [Test]
        public void Catalogue_PinsExactBruteRecipeActorRadiusAndAbility()
        {
            var recipe = StarterInfernalFirstExpansionTactics.BruteKilnVent;
            AssertRecipe(
                recipe,
                "realmraiders.infernal-defense.ember-circuit",
                "ember.west-vent",
                "ember.hellhound",
                "realmraiders.infernal-expansion.brute-kiln-vent",
                "brute-intercept",
                TacticTrigger.InvaderInRange,
                "BRUTE INTERCEPT",
                "Charge",
                7f,
                null,
                1);
            AssertActor(
                recipe.Actors[0],
                0,
                "realmraiders.infernal-brute",
                TacticRole.HeavyInterceptor,
                0f);
        }

        [Test]
        public void Catalogue_PinsThreeRecipeOrderAndExactTwoOneOneActorCounts()
        {
            var all = StarterInfernalFirstExpansionTactics.All;
            Assert.That(all.Count, Is.EqualTo(3));
            Assert.That(all[0], Is.SameAs(StarterInfernalFirstExpansionTactics.AshenPackVent));
            Assert.That(all[1], Is.SameAs(StarterInfernalFirstExpansionTactics.CinderSnareVent));
            Assert.That(all[2], Is.SameAs(StarterInfernalFirstExpansionTactics.BruteKilnVent));
            Assert.That(all[0].Actors.Count, Is.EqualTo(2));
            Assert.That(all[1].Actors.Count, Is.EqualTo(1));
            Assert.That(all[2].Actors.Count, Is.EqualTo(1));
            Assert.That(StarterInfernalFirstExpansionTactics.Validate(all).IsValid, Is.True);
        }

        [Test]
        public void Lookup_ReturnsCachedCanonicalIdentityForEveryAcceptedRecipe()
        {
            foreach (var recipe in StarterInfernalFirstExpansionTactics.All)
            {
                var first = StarterInfernalFirstExpansionTactics.FindExact(
                    recipe.LayoutId,
                    recipe.SocketId);
                var second = StarterInfernalFirstExpansionTactics.FindExact(
                    recipe.LayoutId,
                    recipe.SocketId);
                Assert.That(first.Status, Is.EqualTo(TacticLookupStatus.Found));
                Assert.That(first.Recipe, Is.SameAs(recipe));
                Assert.That(second.Recipe, Is.SameAs(first.Recipe));
            }
        }

        [Test]
        public void Recipe_SnapshotsCallerActorListAndExposesReadOnlyOrder()
        {
            var source = new List<TacticActor>
            {
                Actor(0, "realmraiders.hellhound", TacticRole.TrapGuard, 0.2f)
            };
            var recipe = CopyWithActors(
                StarterInfernalFirstExpansionTactics.CinderSnareVent,
                source);
            source.Clear();

            Assert.That(recipe.Actors.Count, Is.EqualTo(1));
            Assert.Throws<NotSupportedException>(
                () => ((IList<TacticActor>)recipe.Actors).Add(
                    Actor(1, "realmraiders.hellhound", TacticRole.TrapGuard, 0.2f)));
        }

        [Test]
        public void ValidationResult_SnapshotsIssuesAndCannotBeMutatedByCaller()
        {
            var caller = new List<TacticRecipe>
            {
                StarterInfernalFirstExpansionTactics.AshenPackVent
            };
            var result = StarterInfernalFirstExpansionTactics.Validate(caller);
            caller.Add(StarterInfernalFirstExpansionTactics.CinderSnareVent);
            caller.Add(StarterInfernalFirstExpansionTactics.BruteKilnVent);

            Assert.That(result.Issues, Is.EquivalentTo(
                new[] { TacticValidationIssue.CatalogueCardinalityInvalid }));
            Assert.Throws<NotSupportedException>(
                () => ((IList<TacticValidationIssue>)result.Issues).Add(
                    TacticValidationIssue.RecipeMissing));
        }

        [Test]
        public void Lookup_MapsInvalidLayoutAndSocketIdentifiersPrecisely()
        {
            AssertRejected(
                StarterInfernalFirstExpansionTactics.FindExact(null, "ashen.west-vent"),
                TacticLookupStatus.LayoutIdInvalid);
            AssertRejected(
                StarterInfernalFirstExpansionTactics.FindExact(
                    "realmraiders.infernal-defense.ashen-spur",
                    "Invalid Socket"),
                TacticLookupStatus.SocketIdInvalid);
        }

        [Test]
        public void Lookup_MapsUnknownForeignAndLaterSocketEvidencePrecisely()
        {
            AssertRejected(
                StarterInfernalFirstExpansionTactics.FindExact(
                    "realmraiders.infernal-defense.unknown",
                    "unknown.socket"),
                TacticLookupStatus.LayoutUnknown);
            AssertRejected(
                StarterInfernalFirstExpansionTactics.FindExact(
                    "realmraiders.infernal-defense.ashen-spur",
                    "cinder.north-vent"),
                TacticLookupStatus.SocketUnknown);
            AssertRejected(
                StarterInfernalFirstExpansionTactics.FindExact(
                    "realmraiders.infernal-defense.ashen-spur",
                    "ashen.east-vent"),
                TacticLookupStatus.SocketNotFirstAuthored);
        }

        [Test]
        public void Validate_IsNullSafeAndRejectsWrongCatalogueCardinality()
        {
            var nullCatalogue = StarterInfernalFirstExpansionTactics.Validate(null);
            AssertIssue(nullCatalogue, TacticValidationIssue.CatalogueCardinalityInvalid);
            AssertNoOrderIssues(nullCatalogue);

            var missingSet = StarterInfernalFirstExpansionTactics.Validate(
                new[] { StarterInfernalFirstExpansionTactics.AshenPackVent });
            AssertIssue(missingSet, TacticValidationIssue.CatalogueCardinalityInvalid);
            AssertNoOrderIssues(missingSet);
        }

        [Test]
        public void Validate_RejectsNullRecipeWithoutThrowing()
        {
            var recipes = CanonicalList();
            recipes[1] = null;
            var result = StarterInfernalFirstExpansionTactics.Validate(recipes);
            AssertIssue(result, TacticValidationIssue.RecipeMissing);
            AssertNoOrderIssues(result);
        }

        [Test]
        public void Validate_RejectsCatalogueReorderOnlyAsCatalogueOrderMismatch()
        {
            var recipes = new List<TacticRecipe>
            {
                StarterInfernalFirstExpansionTactics.CinderSnareVent,
                StarterInfernalFirstExpansionTactics.AshenPackVent,
                StarterInfernalFirstExpansionTactics.BruteKilnVent
            };
            var result = StarterInfernalFirstExpansionTactics.Validate(recipes);
            AssertIssue(result, TacticValidationIssue.CatalogueOrderMismatch);
            AssertNoIssue(result, TacticValidationIssue.ActorOrderMismatch);
        }

        [Test]
        public void Validate_RejectsDuplicateStableLayoutSocketSiteAndTacticIds()
        {
            var recipes = CanonicalList();
            recipes[1] = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var result = StarterInfernalFirstExpansionTactics.Validate(recipes);
            AssertIssue(result, TacticValidationIssue.LayoutIdDuplicate);
            AssertIssue(result, TacticValidationIssue.SocketIdDuplicate);
            AssertIssue(result, TacticValidationIssue.SiteIdDuplicate);
            AssertIssue(result, TacticValidationIssue.TacticIdDuplicate);
            AssertNoIssue(result, TacticValidationIssue.CatalogueOrderMismatch);
        }

        [Test]
        public void Validate_RejectsUnstableIdentifiersWithSpecificIssues()
        {
            var source = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var changed = new TacticRecipe(
                "Invalid Layout",
                "Invalid Socket",
                "Invalid Source",
                "Invalid Site",
                "Invalid Tactic",
                source.Trigger,
                source.Cue,
                source.Actors,
                source.PreferredAbilityDisplayName,
                source.TriggerRadiusMetres,
                source.Hazard);
            var result = ValidateReplacement(0, changed);
            AssertIssue(result, TacticValidationIssue.LayoutIdInvalid);
            AssertIssue(result, TacticValidationIssue.SocketIdInvalid);
            AssertIssue(result, TacticValidationIssue.SourceNodeIdInvalid);
            AssertIssue(result, TacticValidationIssue.SiteIdInvalid);
            AssertIssue(result, TacticValidationIssue.TacticIdInvalid);
        }

        [Test]
        public void Validate_RejectsUnknownLayoutAndLaterOrUnknownSocketsSpecifically()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            AssertIssue(
                ValidateReplacement(0, CopyWithIdentity(
                    pack,
                    "realmraiders.infernal-defense.unknown",
                    "unknown.socket")),
                TacticValidationIssue.LayoutUnknown);
            AssertIssue(
                ValidateReplacement(0, CopyWithIdentity(
                    pack,
                    pack.LayoutId,
                    "ashen.east-vent")),
                TacticValidationIssue.FirstSocketMismatch);
            AssertIssue(
                ValidateReplacement(0, CopyWithIdentity(
                    pack,
                    pack.LayoutId,
                    "cinder.north-vent")),
                TacticValidationIssue.SocketUnknown);
        }

        [Test]
        public void Validate_RejectsSourceNodeMutationWithoutFalseOrderIssue()
        {
            var result = ValidateReplacement(
                0,
                CopyWithBinding(
                    StarterInfernalFirstExpansionTactics.AshenPackVent,
                    "ashen.changed",
                    "realmraiders.infernal-expansion.ashen-pack-vent"));
            AssertIssue(result, TacticValidationIssue.SourceNodeMismatch);
            AssertNoOrderIssues(result);
        }

        [Test]
        public void Validate_RejectsSiteMutationWithoutFalseOrderIssue()
        {
            var result = ValidateReplacement(
                1,
                CopyWithBinding(
                    StarterInfernalFirstExpansionTactics.CinderSnareVent,
                    "cinder.hellhound",
                    "realmraiders.infernal-expansion.changed"));
            AssertIssue(result, TacticValidationIssue.SiteIdMismatch);
            AssertNoOrderIssues(result);
        }

        [Test]
        public void Validate_RejectsActorContentMutationWithoutFalseOrderIssue()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var changed = CopyWithActors(
                pack,
                new[]
                {
                    Actor(0, "realmraiders.infernal-brute", TacticRole.LeftFlank, 0f),
                    pack.Actors[1]
                });
            var result = ValidateReplacement(0, changed);
            AssertIssue(result, TacticValidationIssue.ActorContentMismatch);
            AssertNoOrderIssues(result);
        }

        [Test]
        public void Validate_RejectsActorNullAndExactActorCardinalityMutations()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var missing = CopyWithActors(pack, new TacticActor[] { null, pack.Actors[1] });
            var nullActor = ValidateReplacement(0, missing);
            AssertIssue(nullActor, TacticValidationIssue.ActorMissing);
            AssertNoOrderIssues(nullActor);

            var missingActor = ValidateReplacement(
                0,
                CopyWithActors(pack, new[] { pack.Actors[0] }));
            AssertIssue(missingActor, TacticValidationIssue.ActorCardinalityInvalid);
            AssertNoOrderIssues(missingActor);
        }

        [Test]
        public void Validate_RejectsDuplicateActorIndex()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var changed = CopyWithActors(
                pack,
                new[]
                {
                    pack.Actors[0],
                    Actor(0, "realmraiders.hellhound", TacticRole.RightFlank, 0.25f)
                });
            var result = ValidateReplacement(0, changed);
            AssertIssue(result, TacticValidationIssue.ActorIndexDuplicate);
            AssertNoIssue(result, TacticValidationIssue.ActorOrderMismatch);
        }

        [Test]
        public void Validate_RejectsNegativeAndPastEndActorIndices()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            AssertIssue(
                ValidateReplacement(0, CopyWithActors(
                    pack,
                    new[]
                    {
                        Actor(-1, "realmraiders.hellhound", TacticRole.LeftFlank, 0f),
                        pack.Actors[1]
                    })),
                TacticValidationIssue.ActorIndexOutOfRange);
            AssertIssue(
                ValidateReplacement(0, CopyWithActors(
                    pack,
                    new[]
                    {
                        pack.Actors[0],
                        Actor(2, "realmraiders.hellhound", TacticRole.RightFlank, 0.25f)
                    })),
                TacticValidationIssue.ActorIndexOutOfRange);
        }

        [Test]
        public void Validate_ChangedActorIndicesWithoutExactMultisetAreNotReorder()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var changed = CopyWithActors(
                pack,
                new[]
                {
                    Actor(1, "realmraiders.hellhound", TacticRole.RightFlank, 0.25f),
                    Actor(2, "realmraiders.hellhound", TacticRole.LeftFlank, 0f)
                });
            var result = ValidateReplacement(0, changed);
            AssertIssue(result, TacticValidationIssue.ActorIndexOutOfRange);
            AssertNoIssue(result, TacticValidationIssue.ActorOrderMismatch);
        }

        [Test]
        public void Validate_RejectsActorReorderWithSpecificActorOrderIssue()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var result = ValidateReplacement(
                0,
                CopyWithActors(pack, new[] { pack.Actors[1], pack.Actors[0] }));
            AssertIssue(result, TacticValidationIssue.ActorOrderMismatch);
            AssertNoIssue(result, TacticValidationIssue.CatalogueOrderMismatch);
        }

        [Test]
        public void Validate_RejectsChangedAndInvalidActorRoles()
        {
            var snare = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            AssertIssue(
                ValidateReplacement(1, CopyWithActors(
                    snare,
                    new[] { Actor(0, "realmraiders.hellhound", TacticRole.LeftFlank, 0.2f) })),
                TacticValidationIssue.ActorRoleMismatch);
            AssertIssue(
                ValidateReplacement(1, CopyWithActors(
                    snare,
                    new[] { Actor(0, "realmraiders.hellhound", (TacticRole)99, 0.2f) })),
                TacticValidationIssue.ActorRoleInvalid);
        }

        [Test]
        public void Validate_RejectsEveryChangedAuthoredResponseDelay()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var changedPack = CopyWithActors(
                pack,
                new[]
                {
                    Actor(0, "realmraiders.hellhound", TacticRole.LeftFlank, 0.1f),
                    Actor(1, "realmraiders.hellhound", TacticRole.RightFlank, 0.2f)
                });
            AssertIssue(
                ValidateReplacement(0, changedPack),
                TacticValidationIssue.ActorDelayMismatch);

            var snare = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            AssertIssue(
                ValidateReplacement(1, CopyWithActors(
                    snare,
                    new[] { Actor(0, "realmraiders.hellhound", TacticRole.TrapGuard, 0.25f) })),
                TacticValidationIssue.ActorDelayMismatch);

            var brute = StarterInfernalFirstExpansionTactics.BruteKilnVent;
            AssertIssue(
                ValidateReplacement(2, CopyWithActors(
                    brute,
                    new[]
                    {
                        Actor(
                            0,
                            "realmraiders.infernal-brute",
                            TacticRole.HeavyInterceptor,
                            0.1f)
                    })),
                TacticValidationIssue.ActorDelayMismatch);
        }

        [Test]
        public void Validate_RejectsNaNAndInfiniteResponseDelays()
        {
            var snare = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            AssertIssue(
                ValidateReplacement(1, CopyWithActors(
                    snare,
                    new[] { Actor(0, "realmraiders.hellhound", TacticRole.TrapGuard, float.NaN) })),
                TacticValidationIssue.ActorDelayInvalid);
            AssertIssue(
                ValidateReplacement(1, CopyWithActors(
                    snare,
                    new[] { Actor(0, "realmraiders.hellhound", TacticRole.TrapGuard, float.PositiveInfinity) })),
                TacticValidationIssue.ActorDelayInvalid);
            AssertIssue(
                ValidateReplacement(1, CopyWithActors(
                    snare,
                    new[] { Actor(0, "realmraiders.hellhound", TacticRole.TrapGuard, float.NegativeInfinity) })),
                TacticValidationIssue.ActorDelayInvalid);
        }

        [Test]
        public void Validate_RejectsResponseDelayOutsideInclusiveZeroToOneRange()
        {
            var brute = StarterInfernalFirstExpansionTactics.BruteKilnVent;
            AssertIssue(
                ValidateReplacement(2, CopyWithActors(
                    brute,
                    new[] { Actor(0, "realmraiders.infernal-brute", TacticRole.HeavyInterceptor, -0.01f) })),
                TacticValidationIssue.ActorDelayInvalid);
            AssertIssue(
                ValidateReplacement(2, CopyWithActors(
                    brute,
                    new[] { Actor(0, "realmraiders.infernal-brute", TacticRole.HeavyInterceptor, 1.01f) })),
                TacticValidationIssue.ActorDelayInvalid);
        }

        [Test]
        public void Validate_RejectsChangedAndInvalidTriggerEnums()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            AssertIssue(
                ValidateReplacement(0, CopyWithTacticalFacts(
                    pack,
                    pack.TacticId,
                    TacticTrigger.FlameActivated,
                    pack.Cue,
                    null,
                    0f)),
                TacticValidationIssue.TriggerMismatch);
            AssertIssue(
                ValidateReplacement(0, CopyWithTacticalFacts(
                    pack,
                    pack.TacticId,
                    (TacticTrigger)99,
                    pack.Cue,
                    null,
                    0f)),
                TacticValidationIssue.TriggerInvalid);
        }

        [Test]
        public void Validate_RejectsChangedTacticIdAndCueIncludingAbsentCue()
        {
            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var changed = ValidateReplacement(0, CopyWithTacticalFacts(
                pack,
                "pack-changed",
                pack.Trigger,
                "PACK CHANGED",
                null,
                0f));
            AssertIssue(changed, TacticValidationIssue.TacticIdMismatch);
            AssertIssue(changed, TacticValidationIssue.CueMismatch);

            var absent = ValidateReplacement(0, CopyWithTacticalFacts(
                pack,
                pack.TacticId,
                pack.Trigger,
                null,
                null,
                0f));
            AssertIssue(absent, TacticValidationIssue.CueInvalid);
            AssertIssue(absent, TacticValidationIssue.CueMismatch);
        }

        [Test]
        public void Validate_RejectsChangedRadiusAndRequiredRadiusAbsence()
        {
            var brute = StarterInfernalFirstExpansionTactics.BruteKilnVent;
            AssertIssue(
                ValidateReplacement(2, CopyWithTacticalFacts(
                    brute,
                    brute.TacticId,
                    brute.Trigger,
                    brute.Cue,
                    "Charge",
                    6.5f)),
                TacticValidationIssue.TriggerRadiusMismatch);

            var pack = StarterInfernalFirstExpansionTactics.AshenPackVent;
            AssertIssue(
                ValidateReplacement(0, CopyWithTacticalFacts(
                    pack,
                    pack.TacticId,
                    pack.Trigger,
                    pack.Cue,
                    null,
                    1f)),
                TacticValidationIssue.TriggerRadiusMismatch);
        }

        [Test]
        public void Validate_RejectsNaNInfiniteAndNegativeTriggerRadii()
        {
            var brute = StarterInfernalFirstExpansionTactics.BruteKilnVent;
            foreach (var radius in new[]
            {
                float.NaN,
                float.PositiveInfinity,
                float.NegativeInfinity,
                -0.01f
            })
            {
                AssertIssue(
                    ValidateReplacement(2, CopyWithTacticalFacts(
                        brute,
                        brute.TacticId,
                        brute.Trigger,
                        brute.Cue,
                        "Charge",
                        radius)),
                    TacticValidationIssue.TriggerRadiusInvalid);
            }
        }

        [Test]
        public void Validate_RejectsChangedRequiredAbilityAndUnexpectedAbilities()
        {
            var brute = StarterInfernalFirstExpansionTactics.BruteKilnVent;
            AssertIssue(
                ValidateReplacement(2, CopyWithTacticalFacts(
                    brute,
                    brute.TacticId,
                    brute.Trigger,
                    brute.Cue,
                    null,
                    7f)),
                TacticValidationIssue.AbilityMismatch);

            var snare = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            AssertIssue(
                ValidateReplacement(1, CopyWithTacticalFacts(
                    snare,
                    snare.TacticId,
                    snare.Trigger,
                    snare.Cue,
                    "Charge",
                    0f)),
                TacticValidationIssue.AbilityMismatch);
        }

        [Test]
        public void Validate_RejectsMissingSnareHazardAndExtraPackOrBruteHazards()
        {
            var snare = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            AssertIssue(
                ValidateReplacement(1, CopyWithHazard(snare, null)),
                TacticValidationIssue.HazardMissing);

            var extra = new TacticHazardDependency(
                "realmraiders.infernal.flame-trap",
                true,
                true,
                true);
            AssertIssue(
                ValidateReplacement(0, CopyWithHazard(
                    StarterInfernalFirstExpansionTactics.AshenPackVent,
                    extra)),
                TacticValidationIssue.HazardUnexpected);
            AssertIssue(
                ValidateReplacement(2, CopyWithHazard(
                    StarterInfernalFirstExpansionTactics.BruteKilnVent,
                    extra)),
                TacticValidationIssue.HazardUnexpected);
        }

        [Test]
        public void Validate_RejectsWrongSnareFlameTrapContentDependency()
        {
            var snare = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            var changed = new TacticHazardDependency(
                "realmraiders.infernal-brute",
                true,
                true,
                true);
            AssertIssue(
                ValidateReplacement(1, CopyWithHazard(snare, changed)),
                TacticValidationIssue.HazardContentMismatch);
        }

        [Test]
        public void Validate_RejectsEachSnareHazardSemanticMutationIndependently()
        {
            var snare = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            AssertIssue(
                ValidateReplacement(1, CopyWithHazard(
                    snare,
                    new TacticHazardDependency(
                        "realmraiders.infernal.flame-trap",
                        false,
                        true,
                        true))),
                TacticValidationIssue.HazardAutomaticMismatch);
            AssertIssue(
                ValidateReplacement(1, CopyWithHazard(
                    snare,
                    new TacticHazardDependency(
                        "realmraiders.infernal.flame-trap",
                        true,
                        false,
                        true))),
                TacticValidationIssue.HazardNonBlockingMismatch);
            AssertIssue(
                ValidateReplacement(1, CopyWithHazard(
                    snare,
                    new TacticHazardDependency(
                        "realmraiders.infernal.flame-trap",
                        true,
                        true,
                        false))),
                TacticValidationIssue.HazardBypassableMismatch);
        }

        private static TacticValidationResult ValidateReplacement(
            int index,
            TacticRecipe replacement)
        {
            var recipes = CanonicalList();
            recipes[index] = replacement;
            return StarterInfernalFirstExpansionTactics.Validate(recipes);
        }

        private static List<TacticRecipe> CanonicalList()
        {
            return new List<TacticRecipe>
            {
                StarterInfernalFirstExpansionTactics.AshenPackVent,
                StarterInfernalFirstExpansionTactics.CinderSnareVent,
                StarterInfernalFirstExpansionTactics.BruteKilnVent
            };
        }

        private static TacticRecipe CopyWithIdentity(
            TacticRecipe source,
            string layoutId,
            string socketId)
        {
            return new TacticRecipe(
                layoutId,
                socketId,
                source.SourceNodeId,
                source.SiteId,
                source.TacticId,
                source.Trigger,
                source.Cue,
                source.Actors,
                source.PreferredAbilityDisplayName,
                source.TriggerRadiusMetres,
                source.Hazard);
        }

        private static TacticRecipe CopyWithBinding(
            TacticRecipe source,
            string sourceNodeId,
            string siteId)
        {
            return new TacticRecipe(
                source.LayoutId,
                source.SocketId,
                sourceNodeId,
                siteId,
                source.TacticId,
                source.Trigger,
                source.Cue,
                source.Actors,
                source.PreferredAbilityDisplayName,
                source.TriggerRadiusMetres,
                source.Hazard);
        }

        private static TacticRecipe CopyWithActors(
            TacticRecipe source,
            IReadOnlyList<TacticActor> actors)
        {
            return new TacticRecipe(
                source.LayoutId,
                source.SocketId,
                source.SourceNodeId,
                source.SiteId,
                source.TacticId,
                source.Trigger,
                source.Cue,
                actors,
                source.PreferredAbilityDisplayName,
                source.TriggerRadiusMetres,
                source.Hazard);
        }

        private static TacticRecipe CopyWithTacticalFacts(
            TacticRecipe source,
            string tacticId,
            TacticTrigger trigger,
            string cue,
            string ability,
            float radius)
        {
            return new TacticRecipe(
                source.LayoutId,
                source.SocketId,
                source.SourceNodeId,
                source.SiteId,
                tacticId,
                trigger,
                cue,
                source.Actors,
                ability,
                radius,
                source.Hazard);
        }

        private static TacticRecipe CopyWithHazard(
            TacticRecipe source,
            TacticHazardDependency hazard)
        {
            return new TacticRecipe(
                source.LayoutId,
                source.SocketId,
                source.SourceNodeId,
                source.SiteId,
                source.TacticId,
                source.Trigger,
                source.Cue,
                source.Actors,
                source.PreferredAbilityDisplayName,
                source.TriggerRadiusMetres,
                hazard);
        }

        private static TacticActor Actor(
            int index,
            string contentId,
            TacticRole role,
            float delay)
        {
            return new TacticActor(index, contentId, role, delay);
        }

        private static void AssertRecipe(
            TacticRecipe recipe,
            string layoutId,
            string socketId,
            string sourceNodeId,
            string siteId,
            string tacticId,
            TacticTrigger trigger,
            string cue,
            string ability,
            float radius,
            string hazardContentId,
            int actorCount)
        {
            Assert.That(recipe.LayoutId, Is.EqualTo(layoutId));
            Assert.That(recipe.SocketId, Is.EqualTo(socketId));
            Assert.That(recipe.SourceNodeId, Is.EqualTo(sourceNodeId));
            Assert.That(recipe.SiteId, Is.EqualTo(siteId));
            Assert.That(recipe.TacticId, Is.EqualTo(tacticId));
            Assert.That(recipe.Trigger, Is.EqualTo(trigger));
            Assert.That(recipe.Cue, Is.EqualTo(cue));
            Assert.That(recipe.PreferredAbilityDisplayName, Is.EqualTo(ability));
            Assert.That(recipe.TriggerRadiusMetres, Is.EqualTo(radius));
            Assert.That(recipe.Actors.Count, Is.EqualTo(actorCount));
            if (hazardContentId == null)
            {
                Assert.That(recipe.Hazard, Is.Null);
            }
            else
            {
                Assert.That(recipe.Hazard, Is.Not.Null);
                Assert.That(recipe.Hazard.ContentId, Is.EqualTo(hazardContentId));
            }
        }

        private static void AssertActor(
            TacticActor actor,
            int index,
            string contentId,
            TacticRole role,
            float delay)
        {
            Assert.That(actor.ActorIndex, Is.EqualTo(index));
            Assert.That(actor.ContentId, Is.EqualTo(contentId));
            Assert.That(actor.Role, Is.EqualTo(role));
            Assert.That(actor.ResponseDelaySeconds, Is.EqualTo(delay));
        }

        private static void AssertRejected(
            TacticLookupResult result,
            TacticLookupStatus status)
        {
            Assert.That(result.Status, Is.EqualTo(status));
            Assert.That(result.Found, Is.False);
            Assert.That(result.Recipe, Is.Null);
        }

        private static void AssertIssue(
            TacticValidationResult result,
            TacticValidationIssue issue)
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues, Does.Contain(issue));
        }

        private static void AssertNoIssue(
            TacticValidationResult result,
            TacticValidationIssue issue)
        {
            var found = false;
            foreach (var candidate in result.Issues)
            {
                if (candidate == issue)
                {
                    found = true;
                    break;
                }
            }

            Assert.That(found, Is.False);
        }

        private static void AssertNoOrderIssues(TacticValidationResult result)
        {
            AssertNoIssue(result, TacticValidationIssue.CatalogueOrderMismatch);
            AssertNoIssue(result, TacticValidationIssue.ActorOrderMismatch);
        }
    }
}
