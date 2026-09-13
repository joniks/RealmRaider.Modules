using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalFirstExpansionContent;

namespace RealmRaiders.Modules.InfernalFirstExpansionPresentation.Tests
{
    public sealed class InfernalFirstExpansionPresentationTests
    {
        private static readonly ExpectedRecipe[] Expected =
        {
            new ExpectedRecipe(
                "realmraiders.infernal-defense.ashen-spur",
                "ashen.west-vent",
                "ashen.hellhound",
                "realmraiders.infernal-expansion.ashen-pack-vent",
                "ashen-pack-vent-basalt-rim",
                "ashen+basalt",
                new[]
                {
                    new ExpectedAnchor("ashen.left", "realmraiders.primitive.basalt-rim", "realmraiders.primitive.basalt-rim.low-block", -2.55f, 1.35f, 0.85f, 25f),
                    new ExpectedAnchor("ashen.right", "realmraiders.primitive.basalt-rim", "realmraiders.primitive.basalt-rim.low-block", 2.6f, 1.2f, 0.8f, 205f)
                }),
            new ExpectedRecipe(
                "realmraiders.infernal-defense.cinder-fork",
                "cinder.north-vent",
                "cinder.hellhound",
                "realmraiders.infernal-expansion.cinder-snare-vent",
                "cinder-snare-vent-open-flanks",
                "cinder+basalt",
                new[]
                {
                    new ExpectedAnchor("cinder.left", "realmraiders.primitive.basalt-rim", "realmraiders.primitive.basalt-rim.low-cinder-marked-block", -2.6f, 1.25f, 0.75f, 20f),
                    new ExpectedAnchor("cinder.right", "realmraiders.primitive.basalt-rim", "realmraiders.primitive.basalt-rim.low-cinder-marked-block", 2.6f, 1.25f, 0.75f, 340f)
                }),
            new ExpectedRecipe(
                "realmraiders.infernal-defense.ember-circuit",
                "ember.west-vent",
                "ember.hellhound",
                "realmraiders.infernal-expansion.brute-kiln-vent",
                "brute-kiln-vent-obsidian-rim",
                "ember+obsidian",
                new[]
                {
                    new ExpectedAnchor("ember.left", "realmraiders.primitive.obsidian-rim", "realmraiders.primitive.obsidian-rim.low-block", -2.55f, 1.3f, 0.95f, 35f),
                    new ExpectedAnchor("ember.right", "realmraiders.primitive.obsidian-rim", "realmraiders.primitive.obsidian-rim.low-block", 2.55f, 1.3f, 0.95f, 325f),
                    new ExpectedAnchor("ember.back", "realmraiders.primitive.obsidian-rim", "realmraiders.primitive.obsidian-rim.low-block", 2.15f, -1.4f, 0.8f, 145f)
                })
        };

        [Test]
        public void Catalogue_PinsEveryExactLiteralAndAcceptedOrder()
        {
            Assert.That(StarterInfernalFirstExpansionPresentation.All.Count, Is.EqualTo(3));
            for (var index = 0; index < Expected.Length; index++)
                AssertRecipe(StarterInfernalFirstExpansionPresentation.All[index], Expected[index]);
        }

        [Test]
        public void FindExact_ReturnsCachedRecipesBoundToMgc31Evidence()
        {
            for (var index = 0; index < Expected.Length; index++)
            {
                var expected = Expected[index];
                var first = StarterInfernalFirstExpansionPresentation.FindExact(
                    expected.LayoutId, expected.SocketId);
                var second = StarterInfernalFirstExpansionPresentation.FindExact(
                    expected.LayoutId, expected.SocketId);

                Assert.That(first.Status, Is.EqualTo(InfernalFirstExpansionPresentationLookupStatus.Found));
                Assert.That(first.Found, Is.True);
                Assert.That(first.Recipe, Is.SameAs(StarterInfernalFirstExpansionPresentation.All[index]));
                Assert.That(second.Recipe, Is.SameAs(first.Recipe));
                Assert.That(first.Recipe.SourceNodeId, Is.EqualTo(expected.SourceNodeId));
                Assert.That(first.Recipe.SiteId, Is.EqualTo(expected.SiteId));
            }
        }

        [Test]
        public void FindExact_DistinguishesInvalidUnknownAndKnownLaterEvidence()
        {
            AssertRejected(
                StarterInfernalFirstExpansionPresentation.FindExact(null, "ashen.west-vent"),
                InfernalFirstExpansionPresentationLookupStatus.LayoutIdInvalid);
            AssertRejected(
                StarterInfernalFirstExpansionPresentation.FindExact(
                    "realmraiders.infernal-defense.ashen-spur", "Invalid Socket"),
                InfernalFirstExpansionPresentationLookupStatus.SocketIdInvalid);
            AssertRejected(
                StarterInfernalFirstExpansionPresentation.FindExact(
                    "realmraiders.infernal-defense.unknown", "socket.one"),
                InfernalFirstExpansionPresentationLookupStatus.LayoutUnknown);
            AssertRejected(
                StarterInfernalFirstExpansionPresentation.FindExact(
                    "realmraiders.infernal-defense.ashen-spur", "ashen.unknown"),
                InfernalFirstExpansionPresentationLookupStatus.SocketUnknown);
            AssertRejected(
                StarterInfernalFirstExpansionPresentation.FindExact(
                    "realmraiders.infernal-defense.ashen-spur", "ashen.east-vent"),
                InfernalFirstExpansionPresentationLookupStatus.SocketNotFirstAuthored);
        }

        [Test]
        public void Mgc31FailureMapping_DistinguishesInvalidCatalogueEvidence()
        {
            Assert.That(
                StarterInfernalFirstExpansionPresentation.MapContentLookupStatus(
                    InfernalFirstExpansionContentLookupStatus.CatalogueInvalid),
                Is.EqualTo(InfernalFirstExpansionPresentationLookupStatus.CatalogueInvalid));
            Assert.That(
                StarterInfernalFirstExpansionPresentation.MapContentValidationIssue(
                    InfernalFirstExpansionContentLookupStatus.CatalogueInvalid),
                Is.EqualTo(InfernalFirstExpansionPresentationValidationIssue.ContentCatalogueInvalid));
        }

        [Test]
        public void Recipe_SnapshotsCallerListAndTreatsNullAsEmpty()
        {
            var original = Anchor("caller.anchor", "realmraiders.primitive.caller",
                "realmraiders.primitive.caller.low-block", 2.5f, 1f, 1f, 45f);
            var source = new List<InfernalFirstExpansionDecorativeAnchor> { original };
            var recipe = RecipeFrom(StarterInfernalFirstExpansionPresentation.AshenPackVent, source);

            source[0] = Anchor("caller.changed", "realmraiders.primitive.changed",
                "realmraiders.primitive.changed.low-block", 2.5f, 1f, 1f, 45f);
            source.Clear();

            Assert.That(recipe.Anchors.Count, Is.EqualTo(1));
            Assert.That(recipe.Anchors[0], Is.SameAs(original));
            Assert.That(RecipeFrom(StarterInfernalFirstExpansionPresentation.AshenPackVent, null)
                .Anchors.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_AcceptsCanonicalCatalogueWithoutMutatingCallerList()
        {
            var caller = new List<InfernalFirstExpansionPresentationRecipe>(
                StarterInfernalFirstExpansionPresentation.All);
            var validation = StarterInfernalFirstExpansionPresentation.Validate(caller);

            Assert.That(validation.IsValid, Is.True);
            Assert.That(validation.Issues.Count, Is.EqualTo(0));
            Assert.That(caller.Count, Is.EqualTo(3));
            Assert.That(caller[0], Is.SameAs(StarterInfernalFirstExpansionPresentation.AshenPackVent));
        }

        [Test]
        public void Validate_RejectsNullMissingCardinalityOrderAndDuplicateCatalogueIds()
        {
            AssertIssue(StarterInfernalFirstExpansionPresentation.Validate(null),
                InfernalFirstExpansionPresentationValidationIssue.CatalogueCardinalityInvalid);

            var missing = new List<InfernalFirstExpansionPresentationRecipe>
            {
                null,
                StarterInfernalFirstExpansionPresentation.CinderSnareVent,
                StarterInfernalFirstExpansionPresentation.BruteKilnVent
            };
            var missingValidation = StarterInfernalFirstExpansionPresentation.Validate(missing);
            AssertIssue(missingValidation, InfernalFirstExpansionPresentationValidationIssue.RecipeMissing);

            var reordered = new List<InfernalFirstExpansionPresentationRecipe>
            {
                StarterInfernalFirstExpansionPresentation.CinderSnareVent,
                StarterInfernalFirstExpansionPresentation.AshenPackVent,
                StarterInfernalFirstExpansionPresentation.BruteKilnVent
            };
            AssertIssue(StarterInfernalFirstExpansionPresentation.Validate(reordered),
                InfernalFirstExpansionPresentationValidationIssue.CatalogueOrderMismatch);

            var duplicate = new List<InfernalFirstExpansionPresentationRecipe>
            {
                StarterInfernalFirstExpansionPresentation.AshenPackVent,
                StarterInfernalFirstExpansionPresentation.AshenPackVent,
                StarterInfernalFirstExpansionPresentation.AshenPackVent
            };
            var duplicateValidation = StarterInfernalFirstExpansionPresentation.Validate(duplicate);
            AssertIssue(duplicateValidation, InfernalFirstExpansionPresentationValidationIssue.LayoutIdDuplicate);
            AssertIssue(duplicateValidation, InfernalFirstExpansionPresentationValidationIssue.SocketIdDuplicate);
            AssertIssue(duplicateValidation, InfernalFirstExpansionPresentationValidationIssue.SiteIdDuplicate);
        }

        [Test]
        public void Validate_RejectsInvalidStableRecipeIdentifiers()
        {
            var canonical = StarterInfernalFirstExpansionPresentation.AshenPackVent;
            var invalid = new InfernalFirstExpansionPresentationRecipe(
                "Invalid Layout", "Invalid Socket", "Invalid Source", "Invalid Site",
                "Invalid Motif", "ashen++basalt", canonical.Anchors);
            var validation = ValidateFirst(invalid);

            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.LayoutIdInvalid);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.SocketIdInvalid);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.SourceNodeIdInvalid);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.SiteIdInvalid);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.MotifIdInvalid);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.PaletteKeyInvalid);
        }

        [Test]
        public void Validate_DistinguishesUnknownLaterAndMismatchedMgc31Bindings()
        {
            var canonical = StarterInfernalFirstExpansionPresentation.AshenPackVent;
            var unknownLayout = Copy(canonical, layoutId: "realmraiders.infernal-defense.unknown");
            var unknownSocket = Copy(canonical, socketId: "ashen.unknown");
            var laterSocket = Copy(canonical, socketId: "ashen.east-vent");
            var wrongSource = Copy(canonical, sourceNodeId: "ashen.brute");
            var wrongSite = Copy(canonical, siteId: "realmraiders.infernal-expansion.wrong-site");

            AssertIssue(ValidateFirst(unknownLayout),
                InfernalFirstExpansionPresentationValidationIssue.LayoutUnknown);
            AssertIssue(ValidateFirst(unknownSocket),
                InfernalFirstExpansionPresentationValidationIssue.SocketUnknown);
            AssertIssue(ValidateFirst(laterSocket),
                InfernalFirstExpansionPresentationValidationIssue.SocketNotFirstAuthored);
            AssertIssue(ValidateFirst(wrongSource),
                InfernalFirstExpansionPresentationValidationIssue.SourceNodeMismatch);
            AssertIssue(ValidateFirst(wrongSite),
                InfernalFirstExpansionPresentationValidationIssue.SiteIdMismatch);
        }

        [Test]
        public void Validate_RejectsMissingInvalidAndDuplicateAnchorIdentifiers()
        {
            var invalid = new List<InfernalFirstExpansionDecorativeAnchor>
            {
                null,
                Anchor("Invalid Anchor", "Invalid Family", "Invalid Prop", -2.5f, 1f, 1f, 0f),
                Anchor("duplicate.anchor", "realmraiders.primitive.one", "realmraiders.primitive.one.low", 2.5f, 1f, 1f, 0f),
                Anchor("duplicate.anchor", "realmraiders.primitive.two", "realmraiders.primitive.two.low", -2.5f, 1f, 1f, 0f)
            };
            var validation = ValidateFirst(RecipeFrom(
                StarterInfernalFirstExpansionPresentation.AshenPackVent, invalid));

            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.AnchorMissing);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.AnchorIdInvalid);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.AnchorIdDuplicate);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.PropFamilyIdInvalid);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.PropIdInvalid);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.PropFamilyCardinalityInvalid);
        }

        [Test]
        public void Validate_EnforcesOneToThreeAnchorRendererAndOneFamilyBudgets()
        {
            var empty = ValidateFirst(RecipeFrom(
                StarterInfernalFirstExpansionPresentation.AshenPackVent,
                Array.Empty<InfernalFirstExpansionDecorativeAnchor>()));
            AssertIssue(empty, InfernalFirstExpansionPresentationValidationIssue.AnchorCardinalityInvalid);
            AssertIssue(empty, InfernalFirstExpansionPresentationValidationIssue.PropFamilyCardinalityInvalid);

            var four = new[]
            {
                Anchor("budget.one", "realmraiders.primitive.one", "realmraiders.primitive.one.low", -2.5f, 1f, 1f, 0f),
                Anchor("budget.two", "realmraiders.primitive.one", "realmraiders.primitive.one.low", 2.5f, 1f, 1f, 0f),
                Anchor("budget.three", "realmraiders.primitive.one", "realmraiders.primitive.one.low", -2.5f, 1.1f, 1f, 0f),
                Anchor("budget.four", "realmraiders.primitive.one", "realmraiders.primitive.one.low", 2.5f, 1.1f, 1f, 0f)
            };
            AssertIssue(ValidateFirst(RecipeFrom(
                StarterInfernalFirstExpansionPresentation.AshenPackVent, four)),
                InfernalFirstExpansionPresentationValidationIssue.RendererBudgetExceeded);

            var twoFamilies = new[]
            {
                Anchor("family.one", "realmraiders.primitive.one", "realmraiders.primitive.one.low", -2.5f, 1f, 1f, 0f),
                Anchor("family.two", "realmraiders.primitive.two", "realmraiders.primitive.two.low", 2.5f, 1f, 1f, 0f)
            };
            AssertIssue(ValidateFirst(RecipeFrom(
                StarterInfernalFirstExpansionPresentation.AshenPackVent, twoFamilies)),
                InfernalFirstExpansionPresentationValidationIssue.PropFamilyCardinalityInvalid);
        }

        [Test]
        public void Validate_RejectsNonFiniteAndUnsafeAnchorGeometryScaleAndYaw()
        {
            var unsafeAnchors = new[]
            {
                Anchor("unsafe.nan-x", "realmraiders.primitive.test", "realmraiders.primitive.test.low", float.NaN, 2.5f, 1f, 0f),
                Anchor("unsafe.infinite-z", "realmraiders.primitive.test", "realmraiders.primitive.test.low", 2.5f, float.NegativeInfinity, float.NaN, float.PositiveInfinity),
                Anchor("unsafe.rim", "realmraiders.primitive.test", "realmraiders.primitive.test.low", 3.01f, 0f, 1.51f, 360f),
                Anchor("unsafe.center", "realmraiders.primitive.test", "realmraiders.primitive.test.low", 1f, 1f, 1f, 0f)
            };
            var validation = ValidateFirst(RecipeFrom(
                StarterInfernalFirstExpansionPresentation.AshenPackVent, unsafeAnchors));

            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.AnchorOffsetNotFinite);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.AnchorOutsideDecorativeRim);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.AnchorIntrudesCentralGameplayDisc);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.AnchorScaleInvalid);
            AssertIssue(validation, InfernalFirstExpansionPresentationValidationIssue.AnchorYawInvalid);
        }

        [Test]
        public void Validate_RejectsIncomingCorridorAndKeepsCinderFlameTrapClear()
        {
            var atFlameTrap = new[]
            {
                Anchor("unsafe.flame", "realmraiders.primitive.test", "realmraiders.primitive.test.low", 0f, -0.4f, 1f, 0f)
            };
            AssertIssue(ValidateFirst(RecipeFrom(
                StarterInfernalFirstExpansionPresentation.CinderSnareVent, atFlameTrap)),
                InfernalFirstExpansionPresentationValidationIssue.AnchorIntrudesIncomingPathCorridor);

            foreach (var anchor in StarterInfernalFirstExpansionPresentation.CinderSnareVent.Anchors)
            {
                var occupiesFlameTrap = anchor.LocalX == 0f && anchor.LocalZ == -0.4f;
                Assert.That(occupiesFlameTrap, Is.False,
                    "The exact MGC31 Flame Trap at local (0, -0.4) must remain clear.");
                Assert.That(Math.Abs(anchor.LocalX) < 1.25f && anchor.LocalZ <= 0.5f, Is.False);
            }
        }

        [Test]
        public void Validate_SeparatesExactFactChangesFromAnchorReordering()
        {
            var canonical = StarterInfernalFirstExpansionPresentation.AshenPackVent;
            var changed = Copy(canonical, motifId: "ashen-pack-vent-changed");
            var changedValidation = ValidateFirst(changed);
            AssertIssue(changedValidation,
                InfernalFirstExpansionPresentationValidationIssue.ExactAuthoredFactsMismatch);
            AssertNoIssue(changedValidation,
                InfernalFirstExpansionPresentationValidationIssue.AnchorOrderMismatch);

            var reorderedAnchors = new[] { canonical.Anchors[1], canonical.Anchors[0] };
            var reorderedValidation = ValidateFirst(RecipeFrom(canonical, reorderedAnchors));
            AssertIssue(reorderedValidation,
                InfernalFirstExpansionPresentationValidationIssue.ExactAuthoredFactsMismatch);
            AssertIssue(reorderedValidation,
                InfernalFirstExpansionPresentationValidationIssue.AnchorOrderMismatch);
        }

        [Test]
        public void ValidationAndLookup_AreCultureInvariantWithoutParsing()
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

                var validation = StarterInfernalFirstExpansionPresentation.Validate(
                    StarterInfernalFirstExpansionPresentation.All);
                var found = StarterInfernalFirstExpansionPresentation.FindExact(
                    "realmraiders.infernal-defense.cinder-fork", "cinder.north-vent");

                Assert.That(validation.IsValid, Is.True);
                Assert.That(found.Found, Is.True);
                Assert.That(found.Recipe.Anchors[0].LocalX, Is.EqualTo(-2.6f));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }

        private static InfernalFirstExpansionPresentationValidationResult ValidateFirst(
            InfernalFirstExpansionPresentationRecipe first)
        {
            return StarterInfernalFirstExpansionPresentation.Validate(
                new[]
                {
                    first,
                    StarterInfernalFirstExpansionPresentation.CinderSnareVent,
                    StarterInfernalFirstExpansionPresentation.BruteKilnVent
                });
        }

        private static InfernalFirstExpansionPresentationRecipe Copy(
            InfernalFirstExpansionPresentationRecipe source,
            string layoutId = null,
            string socketId = null,
            string sourceNodeId = null,
            string siteId = null,
            string motifId = null,
            string paletteKey = null)
        {
            return new InfernalFirstExpansionPresentationRecipe(
                layoutId ?? source.LayoutId,
                socketId ?? source.SocketId,
                sourceNodeId ?? source.SourceNodeId,
                siteId ?? source.SiteId,
                motifId ?? source.MotifId,
                paletteKey ?? source.PaletteKey,
                source.Anchors);
        }

        private static InfernalFirstExpansionPresentationRecipe RecipeFrom(
            InfernalFirstExpansionPresentationRecipe source,
            IReadOnlyList<InfernalFirstExpansionDecorativeAnchor> anchors)
        {
            return new InfernalFirstExpansionPresentationRecipe(
                source.LayoutId, source.SocketId, source.SourceNodeId, source.SiteId,
                source.MotifId, source.PaletteKey, anchors);
        }

        private static InfernalFirstExpansionDecorativeAnchor Anchor(
            string anchorId,
            string propFamilyId,
            string propId,
            float localX,
            float localZ,
            float scale,
            float yawDegrees)
        {
            return new InfernalFirstExpansionDecorativeAnchor(
                anchorId, propFamilyId, propId, localX, localZ, scale, yawDegrees);
        }

        private static void AssertRecipe(
            InfernalFirstExpansionPresentationRecipe actual,
            ExpectedRecipe expected)
        {
            Assert.That(actual.LayoutId, Is.EqualTo(expected.LayoutId));
            Assert.That(actual.SocketId, Is.EqualTo(expected.SocketId));
            Assert.That(actual.SourceNodeId, Is.EqualTo(expected.SourceNodeId));
            Assert.That(actual.SiteId, Is.EqualTo(expected.SiteId));
            Assert.That(actual.MotifId, Is.EqualTo(expected.MotifId));
            Assert.That(actual.PaletteKey, Is.EqualTo(expected.PaletteKey));
            Assert.That(actual.Anchors.Count, Is.EqualTo(expected.Anchors.Length));

            var families = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < expected.Anchors.Length; index++)
            {
                var anchor = actual.Anchors[index];
                var expectedAnchor = expected.Anchors[index];
                Assert.That(anchor.AnchorId, Is.EqualTo(expectedAnchor.AnchorId));
                Assert.That(anchor.PropFamilyId, Is.EqualTo(expectedAnchor.PropFamilyId));
                Assert.That(anchor.PropId, Is.EqualTo(expectedAnchor.PropId));
                Assert.That(anchor.LocalX, Is.EqualTo(expectedAnchor.LocalX));
                Assert.That(anchor.LocalZ, Is.EqualTo(expectedAnchor.LocalZ));
                Assert.That(anchor.Scale, Is.EqualTo(expectedAnchor.Scale));
                Assert.That(anchor.YawDegrees, Is.EqualTo(expectedAnchor.YawDegrees));

                var radius = Math.Sqrt(anchor.LocalX * anchor.LocalX + anchor.LocalZ * anchor.LocalZ);
                Assert.That(radius, Is.GreaterThanOrEqualTo(2.35d));
                Assert.That(radius, Is.LessThanOrEqualTo(3d));
                Assert.That(radius, Is.GreaterThanOrEqualTo(1.5d));
                Assert.That(Math.Abs(anchor.LocalX) < 1.25f && anchor.LocalZ <= 0.5f, Is.False);
                families.Add(anchor.PropFamilyId);
            }

            Assert.That(families.Count, Is.EqualTo(1));
            Assert.That(actual.Anchors.Count, Is.InRange(1, 3));
        }

        private static void AssertRejected(
            InfernalFirstExpansionPresentationLookupResult result,
            InfernalFirstExpansionPresentationLookupStatus expectedStatus)
        {
            Assert.That(result.Status, Is.EqualTo(expectedStatus));
            Assert.That(result.Found, Is.False);
            Assert.That(result.Recipe, Is.Null);
        }

        private static void AssertIssue(
            InfernalFirstExpansionPresentationValidationResult validation,
            InfernalFirstExpansionPresentationValidationIssue expected)
        {
            Assert.That(HasIssue(validation, expected), Is.True,
                "Expected validation issue: " + expected);
        }

        private static void AssertNoIssue(
            InfernalFirstExpansionPresentationValidationResult validation,
            InfernalFirstExpansionPresentationValidationIssue unexpected)
        {
            Assert.That(HasIssue(validation, unexpected), Is.False,
                "Unexpected validation issue: " + unexpected);
        }

        private static bool HasIssue(
            InfernalFirstExpansionPresentationValidationResult validation,
            InfernalFirstExpansionPresentationValidationIssue issue)
        {
            foreach (var actual in validation.Issues)
                if (actual == issue) return true;
            return false;
        }

        private sealed class ExpectedRecipe
        {
            public ExpectedRecipe(
                string layoutId,
                string socketId,
                string sourceNodeId,
                string siteId,
                string motifId,
                string paletteKey,
                ExpectedAnchor[] anchors)
            {
                LayoutId = layoutId;
                SocketId = socketId;
                SourceNodeId = sourceNodeId;
                SiteId = siteId;
                MotifId = motifId;
                PaletteKey = paletteKey;
                Anchors = anchors;
            }

            public string LayoutId { get; }
            public string SocketId { get; }
            public string SourceNodeId { get; }
            public string SiteId { get; }
            public string MotifId { get; }
            public string PaletteKey { get; }
            public ExpectedAnchor[] Anchors { get; }
        }

        private sealed class ExpectedAnchor
        {
            public ExpectedAnchor(
                string anchorId,
                string propFamilyId,
                string propId,
                float localX,
                float localZ,
                float scale,
                float yawDegrees)
            {
                AnchorId = anchorId;
                PropFamilyId = propFamilyId;
                PropId = propId;
                LocalX = localX;
                LocalZ = localZ;
                Scale = scale;
                YawDegrees = yawDegrees;
            }

            public string AnchorId { get; }
            public string PropFamilyId { get; }
            public string PropId { get; }
            public float LocalX { get; }
            public float LocalZ { get; }
            public float Scale { get; }
            public float YawDegrees { get; }
        }
    }
}
