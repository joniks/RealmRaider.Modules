using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.SylvanFirstExpansionPresentation.Tests
{
    public sealed class SylvanFirstExpansionPresentationCatalogueTests
    {
        private static readonly ExpectedRecipe[] Expected =
        {
            new ExpectedRecipe("realmraiders.sylvan-layout.ancient-crossroads", "ancient.west-bough",
                "realmraiders.sylvan-motif.wolf-den-fallen-bough", "realmraiders.palette.sylvan-moss",
                "Wolf den beneath a fallen bough", new[]
                {
                    new ExpectedAnchor("ancient.bough.north", "realmraiders.prop-family.fallen-bough", "realmraiders.prop.fallen-bough.a", -2.5f, 1.4f, 1.1f, 25f),
                    new ExpectedAnchor("ancient.bough.south", "realmraiders.prop-family.fallen-bough", "realmraiders.prop.fallen-bough.b", 2.6f, 1.2f, 0.9f, 205f),
                    new ExpectedAnchor("ancient.den.stone", "realmraiders.prop-family.root-stone", "realmraiders.prop.root-stone.a", 2.2f, -1.4f, 0.75f, 110f)
                }),
            new ExpectedRecipe("realmraiders.sylvan-layout.forked-canopy", "canopy.high-bough",
                "realmraiders.sylvan-motif.ent-watch-root-pillar", "realmraiders.palette.sylvan-canopy",
                "Ent watch among root pillars", new[]
                {
                    new ExpectedAnchor("canopy.pillar.west", "realmraiders.prop-family.root-pillar", "realmraiders.prop.root-pillar.a", -2.6f, 1.3f, 1.25f, 15f),
                    new ExpectedAnchor("canopy.pillar.east", "realmraiders.prop-family.root-pillar", "realmraiders.prop.root-pillar.b", 2.6f, 1.3f, 1.2f, 345f),
                    new ExpectedAnchor("canopy.watch.stone", "realmraiders.prop-family.root-stone", "realmraiders.prop.root-stone.b", -2.1f, -1.5f, 0.8f, 140f)
                }),
            new ExpectedRecipe("realmraiders.sylvan-layout.serpent-roots", "serpent.east-burrow",
                "realmraiders.sylvan-motif.burrow-curved-root-stone", "realmraiders.palette.sylvan-roots",
                "Burrow framed by curved roots and stone", new[]
                {
                    new ExpectedAnchor("serpent.root.west", "realmraiders.prop-family.curved-root", "realmraiders.prop.curved-root.a", -2.6f, 1.2f, 1.05f, 35f),
                    new ExpectedAnchor("serpent.root.east", "realmraiders.prop-family.curved-root", "realmraiders.prop.curved-root.b", 2.6f, 1.2f, 1.05f, 325f),
                    new ExpectedAnchor("serpent.burrow.stone", "realmraiders.prop-family.root-stone", "realmraiders.prop.root-stone.c", 2.2f, -1.4f, 0.85f, 150f)
                })
        };

        [Test]
        public void FindExact_ReturnsThreeExactOrderedRecipesWithinMobileBounds()
        {
            for (var index = 0; index < Expected.Length; index++)
                AssertRecipe(index, Expected[index]);
        }

        [Test]
        public void FindExact_FailsClosedForInvalidUnknownAndNonFirstSockets()
        {
            AssertRejected(StarterSylvanFirstExpansionPresentation.FindExact(null, "ancient.west-bough"), SylvanFirstExpansionPresentationLookupStatus.LayoutIdInvalid);
            AssertRejected(StarterSylvanFirstExpansionPresentation.FindExact(StarterSylvanRealmLayouts.AncientCrossroadsId, "Invalid Socket"), SylvanFirstExpansionPresentationLookupStatus.SocketIdInvalid);
            AssertRejected(StarterSylvanFirstExpansionPresentation.FindExact("realmraiders.sylvan-layout.unknown", "socket.one"), SylvanFirstExpansionPresentationLookupStatus.LayoutUnknown);
            AssertRejected(StarterSylvanFirstExpansionPresentation.FindExact(StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.east-bough"), SylvanFirstExpansionPresentationLookupStatus.SocketNotFirstAuthored);
            AssertRejected(StarterSylvanFirstExpansionPresentation.FindExact(StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.unknown"), SylvanFirstExpansionPresentationLookupStatus.SocketUnknown);
        }

        [Test]
        public void Validate_FailsClosedForDuplicateAndWrongFirstSocket()
        {
            var recipes = new List<SylvanFirstExpansionPresentationRecipe>
            {
                StarterSylvanFirstExpansionPresentation.AncientCrossroads,
                new SylvanFirstExpansionPresentationRecipe(StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.east-bough", "realmraiders.sylvan-motif.wolf-den-fallen-bough", "realmraiders.palette.sylvan-moss", "Wolf den beneath a fallen bough", StarterSylvanFirstExpansionPresentation.AncientCrossroads.Anchors),
                StarterSylvanFirstExpansionPresentation.SerpentRoots
            };
            var validation = StarterSylvanFirstExpansionPresentation.Validate(recipes);
            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.LayoutIdDuplicate));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.FirstSocketMismatch));
        }

        [Test]
        public void Validate_FailsClosedForUnsafeAnchorBoundsAndTooManyPropFamilies()
        {
            var unsafeAnchors = new List<SylvanFirstExpansionDecorativeAnchor>
            {
                new SylvanFirstExpansionDecorativeAnchor("unsafe.center", "realmraiders.prop-family.one", "realmraiders.prop.one", 0f, 0f, 0.2f, 361f),
                new SylvanFirstExpansionDecorativeAnchor("unsafe.corridor", "realmraiders.prop-family.two", "realmraiders.prop.two", 0f, -2.8f, 1f, 0f),
                new SylvanFirstExpansionDecorativeAnchor("unsafe.rim", "realmraiders.prop-family.three", "Invalid Prop", 3.1f, 0f, 1f, 0f),
                new SylvanFirstExpansionDecorativeAnchor("unsafe.four", "realmraiders.prop-family.four", "realmraiders.prop.four", -2.5f, 1.4f, 1f, 0f),
                new SylvanFirstExpansionDecorativeAnchor("unsafe.five", "realmraiders.prop-family.five", "realmraiders.prop.five", 2.5f, 1.4f, 1f, 0f),
                new SylvanFirstExpansionDecorativeAnchor("unsafe.six", "realmraiders.prop-family.six", "realmraiders.prop.six", -2.2f, -1.4f, 1f, 0f),
                new SylvanFirstExpansionDecorativeAnchor("unsafe.seven", "realmraiders.prop-family.seven", "realmraiders.prop.seven", 2.2f, -1.4f, 1f, 0f)
            };
            var changed = new List<SylvanFirstExpansionPresentationRecipe>
            {
                new SylvanFirstExpansionPresentationRecipe(StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.west-bough", "realmraiders.sylvan-motif.wolf-den-fallen-bough", "realmraiders.palette.sylvan-moss", "Wolf den beneath a fallen bough", unsafeAnchors),
                StarterSylvanFirstExpansionPresentation.ForkedCanopy,
                StarterSylvanFirstExpansionPresentation.SerpentRoots
            };
            var validation = StarterSylvanFirstExpansionPresentation.Validate(changed);
            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.AnchorIntrudesCentralGameplayDisc));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.AnchorIntrudesIncomingPathCorridor));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.AnchorOutsideDecorativeRim));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.AnchorScaleInvalid));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.AnchorYawInvalid));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.AnchorCardinalityInvalid));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.PropFamilyCardinalityInvalid));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.PropIdInvalid));
        }

        [Test]
        public void Validate_FailsClosedForChangedFactsAndAnchorOrder()
        {
            var anchors = StarterSylvanFirstExpansionPresentation.AncientCrossroads.Anchors;
            var reordered = new[] { anchors[1], anchors[0], anchors[2] };
            var changed = new List<SylvanFirstExpansionPresentationRecipe>
            {
                new SylvanFirstExpansionPresentationRecipe(StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.west-bough", "realmraiders.sylvan-motif.changed", "realmraiders.palette.sylvan-moss", "Changed label", reordered),
                StarterSylvanFirstExpansionPresentation.ForkedCanopy,
                StarterSylvanFirstExpansionPresentation.SerpentRoots
            };
            var validation = StarterSylvanFirstExpansionPresentation.Validate(changed);
            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.ExactAuthoredFactsMismatch));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionPresentationValidationIssue.AnchorOrderMismatch));
        }

        [Test]
        public void FindExact_IsDeterministicAndDoesNotMutateInputOrCatalogue()
        {
            var first = StarterSylvanFirstExpansionPresentation.FindExact(StarterSylvanRealmLayouts.ForkedCanopyId, "canopy.high-bough");
            var second = StarterSylvanFirstExpansionPresentation.FindExact(StarterSylvanRealmLayouts.ForkedCanopyId, "canopy.high-bough");
            var input = new List<SylvanFirstExpansionPresentationRecipe>(StarterSylvanFirstExpansionPresentation.All);
            var validation = StarterSylvanFirstExpansionPresentation.Validate(input);
            Assert.That(second.Recipe, Is.SameAs(first.Recipe));
            Assert.That(input.Count, Is.EqualTo(3));
            Assert.That(input[0], Is.SameAs(StarterSylvanFirstExpansionPresentation.AncientCrossroads));
            Assert.That(validation.IsValid, Is.True);
        }

        [Test]
        public void Recipe_SnapshotsCallerOwnedAnchorList()
        {
            var source = new List<SylvanFirstExpansionDecorativeAnchor>
            {
                new SylvanFirstExpansionDecorativeAnchor("caller.anchor", "realmraiders.prop-family.caller", "realmraiders.prop.caller", 2.5f, 1.4f, 1f, 45f)
            };
            var recipe = new SylvanFirstExpansionPresentationRecipe(
                "realmraiders.sylvan-layout.ancient-crossroads", "ancient.west-bough",
                "realmraiders.sylvan-motif.caller", "realmraiders.palette.caller", "Caller label", source);

            source[0] = new SylvanFirstExpansionDecorativeAnchor("caller.changed", "realmraiders.prop-family.changed", "realmraiders.prop.changed", 2.5f, 1.4f, 1f, 45f);
            source.Add(new SylvanFirstExpansionDecorativeAnchor("caller.added", "realmraiders.prop-family.added", "realmraiders.prop.added", 2.5f, 1.4f, 1f, 45f));

            Assert.That(recipe.Anchors.Count, Is.EqualTo(1));
            Assert.That(recipe.Anchors[0].AnchorId, Is.EqualTo("caller.anchor"));
        }

        private static void AssertRecipe(int index, ExpectedRecipe expected)
        {
            var recipe = StarterSylvanFirstExpansionPresentation.All[index];
            var result = StarterSylvanFirstExpansionPresentation.FindExact(expected.LayoutId, expected.SocketId);
            Assert.That(recipe.LayoutId, Is.EqualTo(expected.LayoutId));
            Assert.That(recipe.SocketId, Is.EqualTo(expected.SocketId));
            Assert.That(recipe.MotifId, Is.EqualTo(expected.MotifId));
            Assert.That(recipe.PaletteKey, Is.EqualTo(expected.PaletteKey));
            Assert.That(recipe.AccessibleLabel, Is.EqualTo(expected.AccessibleLabel));
            Assert.That(recipe.Anchors.Count, Is.EqualTo(expected.Anchors.Length));
            Assert.That(new HashSet<string>(AnchorFamilies(recipe)).Count, Is.LessThanOrEqualTo(2));
            Assert.That(result.Recipe, Is.SameAs(recipe));
            for (var anchorIndex = 0; anchorIndex < expected.Anchors.Length; anchorIndex++)
            {
                var actual = recipe.Anchors[anchorIndex];
                var expectedAnchor = expected.Anchors[anchorIndex];
                Assert.That(actual.AnchorId, Is.EqualTo(expectedAnchor.AnchorId));
                Assert.That(actual.PropFamilyId, Is.EqualTo(expectedAnchor.PropFamilyId));
                Assert.That(actual.PropId, Is.EqualTo(expectedAnchor.PropId));
                Assert.That(actual.LocalX, Is.EqualTo(expectedAnchor.LocalX));
                Assert.That(actual.LocalZ, Is.EqualTo(expectedAnchor.LocalZ));
                Assert.That(actual.Scale, Is.EqualTo(expectedAnchor.Scale));
                Assert.That(actual.YawDegrees, Is.EqualTo(expectedAnchor.YawDegrees));
            }
        }

        private static IEnumerable<string> AnchorFamilies(SylvanFirstExpansionPresentationRecipe recipe)
        { foreach (var anchor in recipe.Anchors) yield return anchor.PropFamilyId; }
        private static void AssertRejected(SylvanFirstExpansionPresentationLookupResult result, SylvanFirstExpansionPresentationLookupStatus status)
        { Assert.That(result.Status, Is.EqualTo(status)); Assert.That(result.Found, Is.False); Assert.That(result.Recipe, Is.Null); }

        private sealed class ExpectedRecipe
        {
            public ExpectedRecipe(string layoutId, string socketId, string motifId, string paletteKey,
                string accessibleLabel, ExpectedAnchor[] anchors)
            {
                LayoutId = layoutId; SocketId = socketId; MotifId = motifId; PaletteKey = paletteKey;
                AccessibleLabel = accessibleLabel; Anchors = anchors;
            }
            public string LayoutId { get; }
            public string SocketId { get; }
            public string MotifId { get; }
            public string PaletteKey { get; }
            public string AccessibleLabel { get; }
            public ExpectedAnchor[] Anchors { get; }
        }

        private sealed class ExpectedAnchor
        {
            public ExpectedAnchor(string anchorId, string propFamilyId, string propId,
                float localX, float localZ, float scale, float yawDegrees)
            {
                AnchorId = anchorId; PropFamilyId = propFamilyId; PropId = propId;
                LocalX = localX; LocalZ = localZ; Scale = scale; YawDegrees = yawDegrees;
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
