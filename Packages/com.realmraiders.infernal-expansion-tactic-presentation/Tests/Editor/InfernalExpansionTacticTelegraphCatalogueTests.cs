using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalFirstExpansionTactics;

namespace RealmRaiders.Modules.InfernalExpansionTacticPresentation.Tests
{
    public sealed class InfernalExpansionTacticTelegraphCatalogueTests
    {
        [Test]
        public void PackRecipe_PinsExactLiteralsAndOrderedActorChevrons()
        {
            var recipe = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            AssertRecipe(
                recipe,
                StarterInfernalFirstExpansionTactics.AshenPackVent,
                "pack-pincer",
                "ember-flank-chevrons",
                "infernal-amber",
                0.65f,
                TacticTelegraphDirectionBinding.None,
                2);
            AssertAnchor(
                recipe.Anchors[0],
                TacticTelegraphAnchorBindingKind.ActorGroundChevron,
                0,
                0.8f,
                0.6f,
                1.2f,
                0.85f);
            AssertAnchor(
                recipe.Anchors[1],
                TacticTelegraphAnchorBindingKind.ActorGroundChevron,
                1,
                0.8f,
                0.6f,
                1.2f,
                0.85f);
            Assert.That(recipe.TacticRecipe.Actors[0].ResponseDelaySeconds, Is.EqualTo(0f));
            Assert.That(recipe.TacticRecipe.Actors[1].ResponseDelaySeconds, Is.EqualTo(0.25f));
        }

        [Test]
        public void SnareRecipe_PinsExactLiteralsAndExactHazardRingBinding()
        {
            var recipe = StarterInfernalExpansionTacticTelegraphs.SnareCounterattack;
            var tactic = StarterInfernalFirstExpansionTactics.CinderSnareVent;
            AssertRecipe(
                recipe,
                tactic,
                "snare-counterattack",
                "cinder-trap-ring",
                "cinder-orange",
                0.75f,
                TacticTelegraphDirectionBinding.None,
                1);
            AssertAnchor(
                recipe.Anchors[0],
                TacticTelegraphAnchorBindingKind.HazardGroundRing,
                0,
                1f,
                1.6f,
                1.6f,
                0.9f);
            Assert.That(recipe.TacticRecipe.Hazard, Is.SameAs(tactic.Hazard));
            Assert.That(recipe.TacticRecipe.Hazard.ContentId, Is.EqualTo(
                "realmraiders.infernal.flame-trap"));
            Assert.That(recipe.TacticRecipe.Cue, Is.EqualTo("SNARE COUNTER"));
        }

        [Test]
        public void BruteRecipe_PinsExactLaneAndCurrentInvaderDirectionOnly()
        {
            var recipe = StarterInfernalExpansionTacticTelegraphs.BruteIntercept;
            AssertRecipe(
                recipe,
                StarterInfernalFirstExpansionTactics.BruteKilnVent,
                "brute-intercept",
                "brute-charge-lane",
                "obsidian-red",
                0.60f,
                TacticTelegraphDirectionBinding.CurrentInvaderDirection,
                1);
            AssertAnchor(
                recipe.Anchors[0],
                TacticTelegraphAnchorBindingKind.ActorTargetLane,
                0,
                1f,
                0.75f,
                2.5f,
                1f);
            Assert.That(recipe.TacticRecipe.PreferredAbilityDisplayName, Is.EqualTo("Charge"));
            Assert.That(recipe.TacticRecipe.TriggerRadiusMetres, Is.EqualTo(7f));
        }

        [Test]
        public void Catalogue_PinsExactOrderCachedIdentitiesAndSafetyBudgets()
        {
            var all = StarterInfernalExpansionTacticTelegraphs.All;
            Assert.That(all.Count, Is.EqualTo(3));
            Assert.That(all[0], Is.SameAs(StarterInfernalExpansionTacticTelegraphs.PackPincer));
            Assert.That(all[1], Is.SameAs(
                StarterInfernalExpansionTacticTelegraphs.SnareCounterattack));
            Assert.That(all[2], Is.SameAs(
                StarterInfernalExpansionTacticTelegraphs.BruteIntercept));

            for (var index = 0; index < all.Count; index++)
            {
                Assert.That(
                    all[index].TacticRecipe,
                    Is.SameAs(StarterInfernalFirstExpansionTactics.All[index]));
                AssertSafety(all[index]);
            }

            Assert.That(
                StarterInfernalExpansionTacticTelegraphs.Validate(all).IsValid,
                Is.True);
        }

        [Test]
        public void Lookup_ReturnsSameCachedPresentationForEveryExactPair()
        {
            foreach (var recipe in StarterInfernalExpansionTacticTelegraphs.All)
            {
                var first = StarterInfernalExpansionTacticTelegraphs.FindExact(
                    recipe.TacticRecipe,
                    recipe.TacticId);
                var second = StarterInfernalExpansionTacticTelegraphs.FindExact(
                    recipe.TacticRecipe,
                    recipe.TacticId);
                Assert.That(first.Status, Is.EqualTo(TacticTelegraphLookupStatus.Found));
                Assert.That(first.Recipe, Is.SameAs(recipe));
                Assert.That(second.Recipe, Is.SameAs(first.Recipe));
            }
        }

        [Test]
        public void Lookup_RejectsNullRecipeAndInvalidTacticId()
        {
            AssertRejected(
                StarterInfernalExpansionTacticTelegraphs.FindExact(null, "pack-pincer"),
                TacticTelegraphLookupStatus.RecipeMissing);
            AssertRejected(
                StarterInfernalExpansionTacticTelegraphs.FindExact(
                    StarterInfernalFirstExpansionTactics.AshenPackVent,
                    "Invalid Tactic"),
                TacticTelegraphLookupStatus.TacticIdInvalid);
        }

        [Test]
        public void Lookup_RejectsCopiedExactRecipeReference()
        {
            var source = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var copy = CopyTactic(
                source,
                source.LayoutId,
                source.SocketId,
                source.TacticId,
                source.Trigger,
                source.Actors);

            AssertRejected(
                StarterInfernalExpansionTacticTelegraphs.FindExact(copy, source.TacticId),
                TacticTelegraphLookupStatus.RecipeNotCachedExact);
        }

        [Test]
        public void Lookup_RejectsForeignCachedRecipeAndMismatchedTacticId()
        {
            AssertRejected(
                StarterInfernalExpansionTacticTelegraphs.FindExact(
                    StarterInfernalFirstExpansionTactics.CinderSnareVent,
                    "pack-pincer"),
                TacticTelegraphLookupStatus.TacticIdMismatch);
        }

        [Test]
        public void Lookup_RejectsLaterSocketRecipeReference()
        {
            var source = StarterInfernalFirstExpansionTactics.AshenPackVent;
            var later = CopyTactic(
                source,
                source.LayoutId,
                "ashen.east-vent",
                source.TacticId,
                source.Trigger,
                source.Actors);

            AssertRejected(
                StarterInfernalExpansionTacticTelegraphs.FindExact(
                    later,
                    source.TacticId),
                TacticTelegraphLookupStatus.RecipeNotCachedExact);
        }

        [Test]
        public void Validate_RejectsNullAndWrongCatalogueCardinality()
        {
            AssertIssue(
                StarterInfernalExpansionTacticTelegraphs.Validate(null),
                TacticTelegraphValidationIssue.CatalogueCardinalityInvalid);
            AssertIssue(
                StarterInfernalExpansionTacticTelegraphs.Validate(
                    new[] { StarterInfernalExpansionTacticTelegraphs.PackPincer }),
                TacticTelegraphValidationIssue.CatalogueCardinalityInvalid);
        }

        [Test]
        public void Validate_RejectsNullRecipe()
        {
            var recipes = CanonicalList();
            recipes[1] = null;
            var result = StarterInfernalExpansionTacticTelegraphs.Validate(recipes);

            AssertIssue(result, TacticTelegraphValidationIssue.RecipeMissing);
        }

        [Test]
        public void Validate_RejectsExactCatalogueReorderSpecifically()
        {
            var result = StarterInfernalExpansionTacticTelegraphs.Validate(
                new[]
                {
                    StarterInfernalExpansionTacticTelegraphs.SnareCounterattack,
                    StarterInfernalExpansionTacticTelegraphs.PackPincer,
                    StarterInfernalExpansionTacticTelegraphs.BruteIntercept
                });

            AssertIssue(result, TacticTelegraphValidationIssue.CatalogueOrderMismatch);
        }

        [Test]
        public void Validate_RejectsMissingTacticRecipeReference()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            var changed = CopyRecipe(source, null, source.TacticId);
            var result = ValidateReplacement(0, changed);

            AssertIssue(result, TacticTelegraphValidationIssue.TacticRecipeMissing);
            AssertIssue(result, TacticTelegraphValidationIssue.AnchorBindingIndexInvalid);
        }

        [Test]
        public void Validate_RejectsCopiedTacticRecipeReference()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            var tactic = source.TacticRecipe;
            var copiedTactic = CopyTactic(
                tactic,
                tactic.LayoutId,
                tactic.SocketId,
                tactic.TacticId,
                tactic.Trigger,
                tactic.Actors);
            var result = ValidateReplacement(
                0,
                CopyRecipe(source, copiedTactic, source.TacticId));

            AssertIssue(
                result,
                TacticTelegraphValidationIssue.TacticRecipeReferenceMismatch);
        }

        [Test]
        public void Validate_RejectsInvalidMismatchedAndDuplicateTacticIds()
        {
            var pack = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            AssertIssue(
                ValidateReplacement(0, CopyRecipe(pack, pack.TacticRecipe, "Invalid Tactic")),
                TacticTelegraphValidationIssue.TacticIdInvalid);
            AssertIssue(
                ValidateReplacement(0, CopyRecipe(pack, pack.TacticRecipe, "brute-intercept")),
                TacticTelegraphValidationIssue.TacticIdMismatch);

            var snare = StarterInfernalExpansionTacticTelegraphs.SnareCounterattack;
            AssertIssue(
                ValidateReplacement(1, CopyRecipe(snare, snare.TacticRecipe, "pack-pincer")),
                TacticTelegraphValidationIssue.TacticIdDuplicate);
        }

        [Test]
        public void Validate_RejectsInvalidAndMismatchedMotif()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            AssertIssue(
                ValidateReplacement(0, CopyPresentation(
                    source,
                    "Invalid Motif",
                    source.PaletteId,
                    source.DurationSeconds,
                    source.DirectionBinding)),
                TacticTelegraphValidationIssue.MotifInvalid);
            AssertIssue(
                ValidateReplacement(0, CopyPresentation(
                    source,
                    "changed-motif",
                    source.PaletteId,
                    source.DurationSeconds,
                    source.DirectionBinding)),
                TacticTelegraphValidationIssue.MotifMismatch);
        }

        [Test]
        public void Validate_RejectsInvalidAndMismatchedPalette()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            AssertIssue(
                ValidateReplacement(0, CopyPresentation(
                    source,
                    source.MotifId,
                    "Invalid Palette",
                    source.DurationSeconds,
                    source.DirectionBinding)),
                TacticTelegraphValidationIssue.PaletteInvalid);
            AssertIssue(
                ValidateReplacement(0, CopyPresentation(
                    source,
                    source.MotifId,
                    "changed-palette",
                    source.DurationSeconds,
                    source.DirectionBinding)),
                TacticTelegraphValidationIssue.PaletteMismatch);
        }

        [Test]
        public void Validate_RejectsEveryInvalidDurationClassAndExactMismatch()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            var invalid = new[] { 0f, -0.1f, float.NaN, float.PositiveInfinity, 1.01f };
            for (var index = 0; index < invalid.Length; index++)
            {
                AssertIssue(
                    ValidateReplacement(0, CopyPresentation(
                        source,
                        source.MotifId,
                        source.PaletteId,
                        invalid[index],
                        source.DirectionBinding)),
                    TacticTelegraphValidationIssue.DurationInvalid);
            }

            AssertIssue(
                ValidateReplacement(0, CopyPresentation(
                    source,
                    source.MotifId,
                    source.PaletteId,
                    0.7f,
                    source.DirectionBinding)),
                TacticTelegraphValidationIssue.DurationMismatch);
        }

        [Test]
        public void Validate_RejectsInvalidAndMismatchedDirectionBinding()
        {
            var brute = StarterInfernalExpansionTacticTelegraphs.BruteIntercept;
            AssertIssue(
                ValidateReplacement(2, CopyPresentation(
                    brute,
                    brute.MotifId,
                    brute.PaletteId,
                    brute.DurationSeconds,
                    (TacticTelegraphDirectionBinding)99)),
                TacticTelegraphValidationIssue.DirectionBindingInvalid);
            AssertIssue(
                ValidateReplacement(2, CopyPresentation(
                    brute,
                    brute.MotifId,
                    brute.PaletteId,
                    brute.DurationSeconds,
                    TacticTelegraphDirectionBinding.None)),
                TacticTelegraphValidationIssue.DirectionBindingMismatch);
        }

        [Test]
        public void Validate_RejectsMissingAndWrongAnchorCardinality()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            AssertIssue(
                ValidateReplacement(0, CopyAnchors(
                    source,
                    new TacticTelegraphAnchor[] { null, source.Anchors[1] })),
                TacticTelegraphValidationIssue.AnchorMissing);
            AssertIssue(
                ValidateReplacement(0, CopyAnchors(
                    source,
                    new[] { source.Anchors[0] })),
                TacticTelegraphValidationIssue.AnchorCardinalityInvalid);
        }

        [Test]
        public void Validate_RejectsInvalidAndMismatchedAnchorBindingKind()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            var anchor = source.Anchors[0];
            AssertIssue(
                ValidateReplacement(0, ReplaceAnchor(
                    source,
                    0,
                    CopyAnchor(anchor, (TacticTelegraphAnchorBindingKind)99, 0))),
                TacticTelegraphValidationIssue.AnchorBindingKindInvalid);
            AssertIssue(
                ValidateReplacement(0, ReplaceAnchor(
                    source,
                    0,
                    CopyAnchor(
                        anchor,
                        TacticTelegraphAnchorBindingKind.ActorTargetLane,
                        0))),
                TacticTelegraphValidationIssue.AnchorBindingKindMismatch);
        }

        [Test]
        public void Validate_RejectsInvalidAndMismatchedAnchorBindingIndex()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            var anchor = source.Anchors[0];
            AssertIssue(
                ValidateReplacement(0, ReplaceAnchor(
                    source,
                    0,
                    CopyAnchor(anchor, anchor.BindingKind, -1))),
                TacticTelegraphValidationIssue.AnchorBindingIndexInvalid);
            AssertIssue(
                ValidateReplacement(0, ReplaceAnchor(
                    source,
                    0,
                    CopyAnchor(anchor, anchor.BindingKind, 1))),
                TacticTelegraphValidationIssue.AnchorBindingIndexMismatch);
        }

        [Test]
        public void Validate_RejectsExactAnchorReorderSpecifically()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            var result = ValidateReplacement(
                0,
                CopyAnchors(source, new[] { source.Anchors[1], source.Anchors[0] }));

            AssertIssue(result, TacticTelegraphValidationIssue.AnchorOrderMismatch);
        }

        [Test]
        public void Validate_RejectsNonfiniteNonpositiveAndOverBoundAnchorFacts()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.BruteIntercept;
            var anchor = source.Anchors[0];
            var changed = CopyAnchors(
                source,
                new[]
                {
                    new TacticTelegraphAnchor(
                        anchor.BindingKind,
                        anchor.BindingIndex,
                        float.NaN,
                        0f,
                        StarterInfernalExpansionTacticTelegraphs
                            .MaximumNormalizedDimension + 0.1f,
                        float.PositiveInfinity)
                });
            var result = ValidateReplacement(2, changed);

            AssertIssue(result, TacticTelegraphValidationIssue.AnchorScaleInvalid);
            AssertIssue(result, TacticTelegraphValidationIssue.AnchorWidthInvalid);
            AssertIssue(result, TacticTelegraphValidationIssue.AnchorLengthInvalid);
            AssertIssue(result, TacticTelegraphValidationIssue.AnchorIntensityInvalid);
        }

        [Test]
        public void Validate_RejectsEveryValidButNonexactAnchorMeasurement()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.BruteIntercept;
            var anchor = source.Anchors[0];
            var changed = CopyAnchors(
                source,
                new[]
                {
                    new TacticTelegraphAnchor(
                        anchor.BindingKind,
                        anchor.BindingIndex,
                        1.1f,
                        0.8f,
                        2.4f,
                        0.95f)
                });
            var result = ValidateReplacement(2, changed);

            AssertIssue(result, TacticTelegraphValidationIssue.AnchorScaleMismatch);
            AssertIssue(result, TacticTelegraphValidationIssue.AnchorWidthMismatch);
            AssertIssue(result, TacticTelegraphValidationIssue.AnchorLengthMismatch);
            AssertIssue(result, TacticTelegraphValidationIssue.AnchorIntensityMismatch);
        }

        [Test]
        public void Validate_RejectsEverySafetyAuthorityViolation()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            var result = ValidateReplacement(
                0,
                CopySafety(source, false, false, true, 3, 1, true));

            AssertIssue(result, TacticTelegraphValidationIssue.NonRaycastRequired);
            AssertIssue(result, TacticTelegraphValidationIssue.ColliderFreeRequired);
            AssertIssue(result, TacticTelegraphValidationIssue.CameraAuthorityForbidden);
            AssertIssue(
                result,
                TacticTelegraphValidationIssue.PerFrameCatalogueWorkForbidden);
        }

        [Test]
        public void Validate_RejectsInvalidAndNonexactRendererBudgets()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            AssertIssue(
                ValidateReplacement(0, CopySafety(source, true, true, false, 4, 1, false)),
                TacticTelegraphValidationIssue.RendererBudgetInvalid);
            AssertIssue(
                ValidateReplacement(0, CopySafety(source, true, true, false, 2, 1, false)),
                TacticTelegraphValidationIssue.RendererBudgetMismatch);
        }

        [Test]
        public void Validate_RejectsInvalidAndNonexactMaterialBudgets()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.PackPincer;
            AssertIssue(
                ValidateReplacement(0, CopySafety(source, true, true, false, 3, 2, false)),
                TacticTelegraphValidationIssue.SharedMaterialBudgetInvalid);
            AssertIssue(
                ValidateReplacement(0, CopySafety(source, true, true, false, 3, 0, false)),
                TacticTelegraphValidationIssue.SharedMaterialBudgetMismatch);
        }

        [Test]
        public void RecipeAndValidationResult_SnapshotCallerCollectionsReadOnly()
        {
            var source = StarterInfernalExpansionTacticTelegraphs.SnareCounterattack;
            var callerAnchors = new List<TacticTelegraphAnchor>(source.Anchors);
            var copied = CopyAnchors(source, callerAnchors);
            callerAnchors.Clear();

            Assert.That(copied.Anchors.Count, Is.EqualTo(1));
            Assert.Throws<NotSupportedException>(
                () => ((IList<TacticTelegraphAnchor>)copied.Anchors).Add(
                    source.Anchors[0]));
            Assert.Throws<NotSupportedException>(
                () => ((IList<TacticTelegraphRecipe>)
                    StarterInfernalExpansionTacticTelegraphs.All).Add(source));

            var result = ValidateReplacement(1, copied);
            Assert.Throws<NotSupportedException>(
                () => ((IList<TacticTelegraphValidationIssue>)result.Issues).Add(
                    TacticTelegraphValidationIssue.RecipeMissing));
        }

        [Test]
        public void CatalogueAndLookup_AreDeterministicAcrossCultures()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                var recipe = StarterInfernalFirstExpansionTactics.BruteKilnVent;
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");
                var turkish = StarterInfernalExpansionTacticTelegraphs.FindExact(
                    recipe,
                    "brute-intercept");
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");
                var french = StarterInfernalExpansionTacticTelegraphs.FindExact(
                    recipe,
                    "brute-intercept");

                Assert.That(french.Status, Is.EqualTo(turkish.Status));
                Assert.That(french.Recipe, Is.SameAs(turkish.Recipe));
                Assert.That(french.Recipe.Anchors[0], Is.SameAs(turkish.Recipe.Anchors[0]));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Test]
        public void PublicDataTypes_AreSealedAndExposeOnlyReadOnlyProperties()
        {
            Assert.That(typeof(TacticTelegraphAnchor).IsSealed, Is.True);
            Assert.That(typeof(TacticTelegraphRecipe).IsSealed, Is.True);
            Assert.That(typeof(TacticTelegraphLookupResult).IsSealed, Is.True);
            Assert.That(typeof(TacticTelegraphValidationResult).IsSealed, Is.True);
            AssertReadOnlyProperties(typeof(TacticTelegraphAnchor));
            AssertReadOnlyProperties(typeof(TacticTelegraphRecipe));
            AssertReadOnlyProperties(typeof(TacticTelegraphLookupResult));
            AssertReadOnlyProperties(typeof(TacticTelegraphValidationResult));

            var properties = typeof(TacticTelegraphAnchor).GetProperties(
                BindingFlags.Instance | BindingFlags.Public);
            var names = new string[properties.Length];
            for (var index = 0; index < properties.Length; index++)
            {
                names[index] = properties[index].Name;
            }

            Array.Sort(names, StringComparer.Ordinal);
            Assert.That(names, Is.EqualTo(new[]
            {
                "BindingIndex",
                "BindingKind",
                "NormalizedLength",
                "NormalizedLocalScale",
                "NormalizedWidth",
                "PaletteIntensity"
            }));
        }

        private static void AssertRecipe(
            TacticTelegraphRecipe recipe,
            TacticRecipe tacticRecipe,
            string tacticId,
            string motifId,
            string paletteId,
            float durationSeconds,
            TacticTelegraphDirectionBinding directionBinding,
            int anchorCount)
        {
            Assert.That(recipe.TacticRecipe, Is.SameAs(tacticRecipe));
            Assert.That(recipe.TacticId, Is.EqualTo(tacticId));
            Assert.That(recipe.MotifId, Is.EqualTo(motifId));
            Assert.That(recipe.PaletteId, Is.EqualTo(paletteId));
            Assert.That(recipe.DurationSeconds, Is.EqualTo(durationSeconds));
            Assert.That(recipe.DirectionBinding, Is.EqualTo(directionBinding));
            Assert.That(recipe.Anchors.Count, Is.EqualTo(anchorCount));
            AssertSafety(recipe);
        }

        private static void AssertSafety(TacticTelegraphRecipe recipe)
        {
            Assert.That(recipe.NonRaycast, Is.True);
            Assert.That(recipe.ColliderFree, Is.True);
            Assert.That(recipe.CameraAuthority, Is.False);
            Assert.That(recipe.RendererBudget, Is.EqualTo(3));
            Assert.That(recipe.SharedMaterialBudget, Is.EqualTo(1));
            Assert.That(recipe.HasPerFrameCatalogueWork, Is.False);
        }

        private static void AssertAnchor(
            TacticTelegraphAnchor anchor,
            TacticTelegraphAnchorBindingKind bindingKind,
            int bindingIndex,
            float scale,
            float width,
            float length,
            float intensity)
        {
            Assert.That(anchor.BindingKind, Is.EqualTo(bindingKind));
            Assert.That(anchor.BindingIndex, Is.EqualTo(bindingIndex));
            Assert.That(anchor.NormalizedLocalScale, Is.EqualTo(scale));
            Assert.That(anchor.NormalizedWidth, Is.EqualTo(width));
            Assert.That(anchor.NormalizedLength, Is.EqualTo(length));
            Assert.That(anchor.PaletteIntensity, Is.EqualTo(intensity));
        }

        private static void AssertRejected(
            TacticTelegraphLookupResult result,
            TacticTelegraphLookupStatus status)
        {
            Assert.That(result.Status, Is.EqualTo(status));
            Assert.That(result.Found, Is.False);
            Assert.That(result.Recipe, Is.Null);
        }

        private static void AssertIssue(
            TacticTelegraphValidationResult result,
            TacticTelegraphValidationIssue issue)
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues, Does.Contain(issue));
        }

        private static List<TacticTelegraphRecipe> CanonicalList()
        {
            return new List<TacticTelegraphRecipe>(
                StarterInfernalExpansionTacticTelegraphs.All);
        }

        private static TacticTelegraphValidationResult ValidateReplacement(
            int index,
            TacticTelegraphRecipe replacement)
        {
            var recipes = CanonicalList();
            recipes[index] = replacement;
            return StarterInfernalExpansionTacticTelegraphs.Validate(recipes);
        }

        private static TacticTelegraphRecipe CopyRecipe(
            TacticTelegraphRecipe source,
            TacticRecipe tacticRecipe,
            string tacticId)
        {
            return NewRecipe(
                source,
                tacticRecipe,
                tacticId,
                source.MotifId,
                source.PaletteId,
                source.DurationSeconds,
                source.DirectionBinding,
                source.Anchors,
                source.NonRaycast,
                source.ColliderFree,
                source.CameraAuthority,
                source.RendererBudget,
                source.SharedMaterialBudget,
                source.HasPerFrameCatalogueWork);
        }

        private static TacticTelegraphRecipe CopyPresentation(
            TacticTelegraphRecipe source,
            string motifId,
            string paletteId,
            float durationSeconds,
            TacticTelegraphDirectionBinding directionBinding)
        {
            return NewRecipe(
                source,
                source.TacticRecipe,
                source.TacticId,
                motifId,
                paletteId,
                durationSeconds,
                directionBinding,
                source.Anchors,
                source.NonRaycast,
                source.ColliderFree,
                source.CameraAuthority,
                source.RendererBudget,
                source.SharedMaterialBudget,
                source.HasPerFrameCatalogueWork);
        }

        private static TacticTelegraphRecipe CopyAnchors(
            TacticTelegraphRecipe source,
            IReadOnlyList<TacticTelegraphAnchor> anchors)
        {
            return NewRecipe(
                source,
                source.TacticRecipe,
                source.TacticId,
                source.MotifId,
                source.PaletteId,
                source.DurationSeconds,
                source.DirectionBinding,
                anchors,
                source.NonRaycast,
                source.ColliderFree,
                source.CameraAuthority,
                source.RendererBudget,
                source.SharedMaterialBudget,
                source.HasPerFrameCatalogueWork);
        }

        private static TacticTelegraphRecipe ReplaceAnchor(
            TacticTelegraphRecipe source,
            int index,
            TacticTelegraphAnchor replacement)
        {
            var anchors = new List<TacticTelegraphAnchor>(source.Anchors);
            anchors[index] = replacement;
            return CopyAnchors(source, anchors);
        }

        private static TacticTelegraphRecipe CopySafety(
            TacticTelegraphRecipe source,
            bool nonRaycast,
            bool colliderFree,
            bool cameraAuthority,
            int rendererBudget,
            int sharedMaterialBudget,
            bool hasPerFrameCatalogueWork)
        {
            return NewRecipe(
                source,
                source.TacticRecipe,
                source.TacticId,
                source.MotifId,
                source.PaletteId,
                source.DurationSeconds,
                source.DirectionBinding,
                source.Anchors,
                nonRaycast,
                colliderFree,
                cameraAuthority,
                rendererBudget,
                sharedMaterialBudget,
                hasPerFrameCatalogueWork);
        }

        private static TacticTelegraphRecipe NewRecipe(
            TacticTelegraphRecipe source,
            TacticRecipe tacticRecipe,
            string tacticId,
            string motifId,
            string paletteId,
            float durationSeconds,
            TacticTelegraphDirectionBinding directionBinding,
            IReadOnlyList<TacticTelegraphAnchor> anchors,
            bool nonRaycast,
            bool colliderFree,
            bool cameraAuthority,
            int rendererBudget,
            int sharedMaterialBudget,
            bool hasPerFrameCatalogueWork)
        {
            return new TacticTelegraphRecipe(
                tacticRecipe,
                tacticId,
                motifId,
                paletteId,
                durationSeconds,
                directionBinding,
                anchors,
                nonRaycast,
                colliderFree,
                cameraAuthority,
                rendererBudget,
                sharedMaterialBudget,
                hasPerFrameCatalogueWork);
        }

        private static TacticTelegraphAnchor CopyAnchor(
            TacticTelegraphAnchor source,
            TacticTelegraphAnchorBindingKind bindingKind,
            int bindingIndex)
        {
            return new TacticTelegraphAnchor(
                bindingKind,
                bindingIndex,
                source.NormalizedLocalScale,
                source.NormalizedWidth,
                source.NormalizedLength,
                source.PaletteIntensity);
        }

        private static TacticRecipe CopyTactic(
            TacticRecipe source,
            string layoutId,
            string socketId,
            string tacticId,
            TacticTrigger trigger,
            IReadOnlyList<TacticActor> actors)
        {
            return new TacticRecipe(
                layoutId,
                socketId,
                source.SourceNodeId,
                source.SiteId,
                tacticId,
                trigger,
                source.Cue,
                actors,
                source.PreferredAbilityDisplayName,
                source.TriggerRadiusMetres,
                source.Hazard);
        }

        private static void AssertReadOnlyProperties(Type type)
        {
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                Assert.That(property.CanWrite, Is.False, type.Name + "." + property.Name);
            }
        }
    }
}
