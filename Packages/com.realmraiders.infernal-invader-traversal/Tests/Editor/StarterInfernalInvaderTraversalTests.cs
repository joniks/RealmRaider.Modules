using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace RealmRaiders.Modules.InfernalInvaderTraversal.Tests
{
    public sealed class StarterInfernalInvaderTraversalTests
    {
        [Test]
        public void Catalogue_HasExactFieldForFieldAuthoredRoutes()
        {
            AssertRoute(
                StarterInfernalInvaderTraversals.AshenSpur,
                "realmraiders.infernal-defense.ashen-spur",
                new[] { "ashen.entry", "ashen.junction", "ashen.flame", "ashen.brute", "ashen.heart" },
                new[] { "ashen.entry-junction", "ashen.junction-flame", "ashen.flame-brute", "ashen.brute-heart" },
                new[] { InfernalTraversalClass.Safe, InfernalTraversalClass.Safe, InfernalTraversalClass.Safe, InfernalTraversalClass.Safe },
                "ashen.flame-brute");
            AssertRoute(
                StarterInfernalInvaderTraversals.CinderFork,
                "realmraiders.infernal-defense.cinder-fork",
                new[] { "cinder.entry", "cinder.junction", "cinder.hellhound", "cinder.junction", "cinder.flame", "cinder.brute", "cinder.heart" },
                new[] { "cinder.entry-junction", "cinder.junction-hellhound", "cinder.junction-hellhound", "cinder.junction-flame", "cinder.flame-brute", "cinder.brute-heart" },
                new[] { InfernalTraversalClass.Safe, InfernalTraversalClass.Safe, InfernalTraversalClass.Safe, InfernalTraversalClass.DeclaredRisk, InfernalTraversalClass.DeclaredRisk, InfernalTraversalClass.Safe },
                "cinder.flame-brute");
            AssertRoute(
                StarterInfernalInvaderTraversals.EmberCircuit,
                "realmraiders.infernal-defense.ember-circuit",
                new[] { "ember.entry", "ember.junction", "ember.hellhound", "ember.flame", "ember.brute", "ember.heart" },
                new[] { "ember.entry-junction", "ember.junction-hellhound", "ember.hellhound-flame", "ember.flame-brute", "ember.brute-heart" },
                new[] { InfernalTraversalClass.Safe, InfernalTraversalClass.Safe, InfernalTraversalClass.DeclaredRisk, InfernalTraversalClass.Safe, InfernalTraversalClass.Safe },
                "ember.flame-brute");
        }

        [Test]
        public void Catalogue_IsCachedImmutableAndValid()
        {
            Assert.That(StarterInfernalInvaderTraversals.All.Count, Is.EqualTo(3));
            Assert.That(StarterInfernalInvaderTraversals.All[0], Is.SameAs(StarterInfernalInvaderTraversals.AshenSpur));
            Assert.That(StarterInfernalInvaderTraversals.All[1], Is.SameAs(StarterInfernalInvaderTraversals.CinderFork));
            Assert.That(StarterInfernalInvaderTraversals.All[2], Is.SameAs(StarterInfernalInvaderTraversals.EmberCircuit));
            Assert.That(
                InfernalInvaderTraversalValidator.ValidateCatalogue(StarterInfernalInvaderTraversals.All).IsValid,
                Is.True);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<InfernalInvaderTraversalRecipe>)StarterInfernalInvaderTraversals.All).Add(
                    StarterInfernalInvaderTraversals.AshenSpur));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<string>)StarterInfernalInvaderTraversals.AshenSpur.NodeIds)[0] = "mutated");
        }

        [Test]
        public void Recipe_SnapshotsCallerCollections()
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var nodes = Copy(source.NodeIds);
            var steps = Copy(source.EdgeSteps);
            var recipe = Copy(source, nodes: nodes, steps: steps);

            nodes[0] = "mutated";
            steps[0] = new InfernalTraversalEdgeStep("mutated", InfernalTraversalClass.DeclaredRisk);

            Assert.That(recipe.NodeIds[0], Is.EqualTo("ashen.entry"));
            Assert.That(recipe.EdgeSteps[0].EdgeId, Is.EqualTo("ashen.entry-junction"));
            Assert.That(InfernalInvaderTraversalValidator.Validate(recipe).IsValid, Is.True);
        }

        [Test]
        public void EveryRoute_ValidatesAgainstExactCachedLayoutAndPacing()
        {
            foreach (var recipe in StarterInfernalInvaderTraversals.All)
            {
                var result = InfernalInvaderTraversalValidator.Validate(recipe);
                Assert.That(result.IsValid, Is.True, recipe.LayoutId);
                Assert.That(result.Issues, Is.Empty);
                Assert.That(recipe.PacingLayoutId, Is.EqualTo(recipe.LayoutId));
            }
        }

        [Test]
        public void ResolveExact_IsOrdinalAndReturnsCachedInstances()
        {
            foreach (var expected in StarterInfernalInvaderTraversals.All)
            {
                var result = StarterInfernalInvaderTraversalResolver.ResolveExact(expected.LayoutId);
                Assert.That(result.Status, Is.EqualTo(InfernalInvaderTraversalLookupStatus.Found));
                Assert.That(result.Found, Is.True);
                Assert.That(result.Recipe, Is.SameAs(expected));
            }

            AssertLookup(null, InfernalInvaderTraversalLookupStatus.LayoutIdInvalid);
            AssertLookup(" cinder", InfernalInvaderTraversalLookupStatus.LayoutIdInvalid);
            AssertLookup("CINDER", InfernalInvaderTraversalLookupStatus.NotFound);
            AssertLookup("realmraiders.infernal-defense.unknown", InfernalInvaderTraversalLookupStatus.NotFound);
        }

        [Test]
        public void SemanticSignatures_AreStableAndPairwiseDistinct()
        {
            var signatures = new HashSet<string>(StringComparer.Ordinal);
            foreach (var route in StarterInfernalInvaderTraversals.All)
            {
                var first = InfernalInvaderTraversalValidator.CreateSemanticSignature(route);
                var second = InfernalInvaderTraversalValidator.CreateSemanticSignature(route);
                Assert.That(first, Is.Not.Null.And.Not.Empty);
                Assert.That(second, Is.EqualTo(first));
                Assert.That(signatures.Add(first), Is.True, route.LayoutId);
            }

            Assert.That(InfernalInvaderTraversalValidator.CreateSemanticSignature(null), Is.Null);
        }

        [Test]
        public void Validate_FailsClosedForMissingRecipeAndCollectionsWithStableEvidenceOrder()
        {
            CollectionAssert.AreEqual(
                new[] { InfernalInvaderTraversalValidationIssue.RecipeMissing },
                InfernalInvaderTraversalValidator.Validate(null).Issues);

            var result = InfernalInvaderTraversalValidator.Validate(
                new InfernalInvaderTraversalRecipe(null, null, null, null, null));
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalInvaderTraversalValidationIssue.LayoutIdInvalid,
                    InfernalInvaderTraversalValidationIssue.PacingLayoutIdInvalid,
                    InfernalInvaderTraversalValidationIssue.NodeCollectionMissing,
                    InfernalInvaderTraversalValidationIssue.EdgeStepCollectionMissing,
                    InfernalInvaderTraversalValidationIssue.StepCardinalityInvalid,
                    InfernalInvaderTraversalValidationIssue.FlameRoleMissing,
                    InfernalInvaderTraversalValidationIssue.BruteRoleMissing,
                    InfernalInvaderTraversalValidationIssue.HeartRoleMissing,
                    InfernalInvaderTraversalValidationIssue.FlameBruteHeartOrderInvalid,
                    InfernalInvaderTraversalValidationIssue.LavaGateMissing
                },
                result.Issues);
        }

        [Test]
        public void Validate_FailsClosedForNullElements()
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var nodes = Copy(source.NodeIds);
            nodes[1] = null;
            var steps = Copy(source.EdgeSteps);
            steps[1] = null;
            var result = InfernalInvaderTraversalValidator.Validate(
                Copy(source, nodes: nodes, steps: steps));

            Assert.That(result.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.NodeMissing));
            Assert.That(result.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.EdgeStepMissing));
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void Validate_FailsClosedForNullAndMalformedEdgeIds()
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var nullId = Copy(source.EdgeSteps);
            nullId[0] = new InfernalTraversalEdgeStep(null, InfernalTraversalClass.Safe);
            var malformedId = Copy(source.EdgeSteps);
            malformedId[0] = new InfernalTraversalEdgeStep(" bad", InfernalTraversalClass.Safe);

            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, steps: nullId)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.EdgeIdInvalid));
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, steps: malformedId)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.EdgeIdInvalid));
        }

        [TestCase(null, InfernalInvaderTraversalValidationIssue.LayoutIdInvalid)]
        [TestCase(" bad", InfernalInvaderTraversalValidationIssue.LayoutIdInvalid)]
        [TestCase("realmraiders.infernal-defense.missing", InfernalInvaderTraversalValidationIssue.LayoutNotFound)]
        [TestCase("REALMRAIDERS.INFERNAL-DEFENSE.ASHEN-SPUR", InfernalInvaderTraversalValidationIssue.LayoutNotFound)]
        public void Validate_FailsClosedForMalformedWrongOrUnknownLayout(
            string layoutId,
            InfernalInvaderTraversalValidationIssue expected)
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var result = InfernalInvaderTraversalValidator.Validate(
                new InfernalInvaderTraversalRecipe(
                    layoutId,
                    source.PacingLayoutId,
                    source.NodeIds,
                    source.EdgeSteps,
                    source.LavaGate));
            Assert.That(result.Issues, Does.Contain(expected));
        }

        [Test]
        public void Validate_FailsClosedForMalformedWrongOrUnknownPacing()
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var malformed = InfernalInvaderTraversalValidator.Validate(
                Copy(source, pacingLayoutId: " bad"));
            var unknown = InfernalInvaderTraversalValidator.Validate(
                Copy(source, pacingLayoutId: "realmraiders.infernal-defense.missing"));
            var wrong = InfernalInvaderTraversalValidator.Validate(
                Copy(source, pacingLayoutId: StarterInfernalInvaderTraversals.CinderFork.LayoutId));

            Assert.That(malformed.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.PacingLayoutIdInvalid));
            Assert.That(unknown.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.PacingNotFound));
            Assert.That(wrong.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.PacingLayoutMismatch));
            Assert.That(malformed.IsValid || unknown.IsValid || wrong.IsValid, Is.False);
        }

        [Test]
        public void Validate_RejectsKnownButMismatchedLayoutAndPacingIdentity()
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var result = InfernalInvaderTraversalValidator.Validate(
                new InfernalInvaderTraversalRecipe(
                    StarterInfernalInvaderTraversals.CinderFork.LayoutId,
                    source.PacingLayoutId,
                    source.NodeIds,
                    source.EdgeSteps,
                    source.LavaGate));

            Assert.That(result.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.PacingLayoutMismatch));
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void Validate_RejectsCardinalityStartEndAndInvalidNodes()
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var shortSteps = new[] { source.EdgeSteps[0] };
            var cardinality = InfernalInvaderTraversalValidator.Validate(Copy(source, steps: shortSteps));
            var reversedNodes = Copy(source.NodeIds);
            Array.Reverse(reversedNodes);
            var reverse = InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: reversedNodes));
            var malformedNodes = Copy(source.NodeIds);
            malformedNodes[1] = " invalid";
            var malformed = InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: malformedNodes));
            var phantomNodes = Copy(source.NodeIds);
            phantomNodes[1] = "ashen.phantom";
            var phantom = InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: phantomNodes));

            Assert.That(cardinality.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.StepCardinalityInvalid));
            Assert.That(reverse.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.StartOrEndInvalid));
            Assert.That(malformed.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.NodeIdInvalid));
            Assert.That(phantom.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.NodeInvalid));
        }

        [Test]
        public void Validate_RejectsPhantomDisconnectedAndReversedRoutes()
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var phantomSteps = Copy(source.EdgeSteps);
            phantomSteps[1] = new InfernalTraversalEdgeStep("ashen.phantom", InfernalTraversalClass.Safe);
            var disconnectedSteps = Copy(source.EdgeSteps);
            disconnectedSteps[1] = source.EdgeSteps[3];
            var reversedNodes = Copy(source.NodeIds);
            Array.Reverse(reversedNodes);
            var reversedSteps = Copy(source.EdgeSteps);
            Array.Reverse(reversedSteps);

            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, steps: phantomSteps)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.EdgeInvalid));
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, steps: disconnectedSteps)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.EdgeDisconnected));
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: reversedNodes, steps: reversedSteps)).IsValid,
                Is.False);
        }

        [Test]
        public void Validate_RequiresEveryTraversalClassToMirrorLayoutSafety()
        {
            foreach (var source in StarterInfernalInvaderTraversals.All)
            {
                for (var index = 0; index < source.EdgeSteps.Count; index++)
                {
                    var steps = Copy(source.EdgeSteps);
                    var opposite = steps[index].TraversalClass == InfernalTraversalClass.Safe
                        ? InfernalTraversalClass.DeclaredRisk
                        : InfernalTraversalClass.Safe;
                    steps[index] = new InfernalTraversalEdgeStep(steps[index].EdgeId, opposite);
                    var result = InfernalInvaderTraversalValidator.Validate(Copy(source, steps: steps));
                    Assert.That(
                        result.Issues,
                        Does.Contain(InfernalInvaderTraversalValidationIssue.EdgeTraversalClassInvalid),
                        source.LayoutId + " edge " + index);
                }
            }

            var malformed = Copy(StarterInfernalInvaderTraversals.AshenSpur.EdgeSteps);
            malformed[0] = new InfernalTraversalEdgeStep(
                malformed[0].EdgeId,
                (InfernalTraversalClass)99);
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(
                    Copy(StarterInfernalInvaderTraversals.AshenSpur, steps: malformed)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.EdgeTraversalClassInvalid));
        }

        [Test]
        public void Validate_AllowsOnlyExactContiguousHellhoundJunctionOutAndBack()
        {
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(StarterInfernalInvaderTraversals.CinderFork).IsValid,
                Is.True);

            var source = StarterInfernalInvaderTraversals.CinderFork;
            var wrongReturn = Copy(source.EdgeSteps);
            wrongReturn[2] = source.EdgeSteps[0];
            var wrongExcursionNodes = Copy(source.NodeIds);
            wrongExcursionNodes[2] = "cinder.flame";
            var repeatedHeartNodes = Copy(source.NodeIds);
            repeatedHeartNodes[1] = "cinder.heart";

            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, steps: wrongReturn)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.IllegalEdgeRetrace));
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: wrongExcursionNodes)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.IllegalNodeRetrace));
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: repeatedHeartNodes)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.IllegalNodeRetrace));
        }

        [Test]
        public void Validate_RejectsMissingAndMisorderedRequiredBeats()
        {
            var source = StarterInfernalInvaderTraversals.CinderFork;
            var missingHound = Copy(source.NodeIds);
            missingHound[2] = "cinder.junction";
            var misordered = Copy(source.NodeIds);
            var flame = misordered[4];
            misordered[4] = misordered[5];
            misordered[5] = flame;
            var requiredMisordered = Copy(source.NodeIds);
            var hound = requiredMisordered[2];
            requiredMisordered[2] = requiredMisordered[5];
            requiredMisordered[5] = hound;

            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: missingHound)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.RequiredRoleMissing));
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: misordered)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.FlameBruteHeartOrderInvalid));
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: requiredMisordered)).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.RequiredRoleOrderInvalid));
        }

        [Test]
        public void Validate_RequiresFlameBruteAndHeartEvenWhenFlamePacingIsOptional()
        {
            var source = StarterInfernalInvaderTraversals.EmberCircuit;
            AssertMissingRole(source, 3, InfernalInvaderTraversalValidationIssue.FlameRoleMissing);
            AssertMissingRole(source, 4, InfernalInvaderTraversalValidationIssue.BruteRoleMissing);
            AssertMissingRole(source, 5, InfernalInvaderTraversalValidationIssue.HeartRoleMissing);

            Assert.That(source.NodeIds, Does.Contain("ember.flame"));
            Assert.That(StarterInfernalInvaderTraversals.CinderFork.NodeIds, Does.Contain("cinder.flame"));
        }

        [Test]
        public void Validate_RequiresExactIncomingBruteLavaGateAtHalfEdge()
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var missing = new InfernalInvaderTraversalRecipe(
                source.LayoutId,
                source.PacingLayoutId,
                source.NodeIds,
                source.EdgeSteps,
                null);
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(missing).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.LavaGateMissing));
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, lavaGate: new InfernalLavaGatePlacement(" bad", .5f))).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.LavaGateEdgeIdInvalid));
            Assert.That(
                InfernalInvaderTraversalValidator.Validate(Copy(source, lavaGate: new InfernalLavaGatePlacement("ashen.phantom", .5f))).Issues,
                Does.Contain(InfernalInvaderTraversalValidationIssue.LavaGateEdgeInvalid));

            var notInRoute = InfernalInvaderTraversalValidator.Validate(
                Copy(source, lavaGate: new InfernalLavaGatePlacement("ashen.junction-hellhound", .5f)));
            Assert.That(notInRoute.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.LavaGateNotInRoute));
            Assert.That(notInRoute.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.LavaGateNotIncomingBrute));

            var wrongRouteEdge = InfernalInvaderTraversalValidator.Validate(
                Copy(source, lavaGate: new InfernalLavaGatePlacement("ashen.brute-heart", .5f)));
            CollectionAssert.DoesNotContain(
                wrongRouteEdge.Issues,
                InfernalInvaderTraversalValidationIssue.LavaGateNotInRoute);
            Assert.That(wrongRouteEdge.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.LavaGateNotIncomingBrute));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        [TestCase(-0.1f)]
        [TestCase(0f)]
        [TestCase(0.25f)]
        [TestCase(0.75f)]
        [TestCase(1f)]
        [TestCase(1.1f)]
        public void Validate_RejectsEveryNonExactLavaGateFraction(float fraction)
        {
            var source = StarterInfernalInvaderTraversals.AshenSpur;
            var result = InfernalInvaderTraversalValidator.Validate(
                Copy(source, lavaGate: new InfernalLavaGatePlacement(source.LavaGate.EdgeId, fraction)));
            Assert.That(result.Issues, Does.Contain(InfernalInvaderTraversalValidationIssue.LavaGateFractionInvalid));
        }

        [Test]
        public void ValidateCatalogue_FailsClosedWithStableEvidence()
        {
            CollectionAssert.AreEqual(
                new[] { InfernalInvaderTraversalCatalogueValidationIssue.CatalogueMissing },
                InfernalInvaderTraversalValidator.ValidateCatalogue(null).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalInvaderTraversalCatalogueValidationIssue.RecipeCardinalityInvalid,
                    InfernalInvaderTraversalCatalogueValidationIssue.RecipeMissing,
                    InfernalInvaderTraversalCatalogueValidationIssue.LayoutCoverageInvalid
                },
                InfernalInvaderTraversalValidator.ValidateCatalogue(
                    new InfernalInvaderTraversalRecipe[] { null }).Issues);

            var duplicates = new[]
            {
                StarterInfernalInvaderTraversals.AshenSpur,
                StarterInfernalInvaderTraversals.AshenSpur,
                StarterInfernalInvaderTraversals.AshenSpur
            };
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalInvaderTraversalCatalogueValidationIssue.LayoutIdDuplicate,
                    InfernalInvaderTraversalCatalogueValidationIssue.SemanticSignatureDuplicate,
                    InfernalInvaderTraversalCatalogueValidationIssue.LayoutCoverageInvalid
                },
                InfernalInvaderTraversalValidator.ValidateCatalogue(duplicates).Issues);
        }

        [Test]
        public void ValidateCatalogue_RejectsCardinalityCoverageAndInvalidRecipes()
        {
            var shortCatalogue = new[]
            {
                StarterInfernalInvaderTraversals.AshenSpur,
                StarterInfernalInvaderTraversals.CinderFork
            };
            var malformedEmber = Copy(
                StarterInfernalInvaderTraversals.EmberCircuit,
                pacingLayoutId: "unknown.pacing");
            var invalidCatalogue = new[]
            {
                StarterInfernalInvaderTraversals.AshenSpur,
                StarterInfernalInvaderTraversals.CinderFork,
                malformedEmber
            };

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalInvaderTraversalCatalogueValidationIssue.RecipeCardinalityInvalid,
                    InfernalInvaderTraversalCatalogueValidationIssue.LayoutCoverageInvalid
                },
                InfernalInvaderTraversalValidator.ValidateCatalogue(shortCatalogue).Issues);
            CollectionAssert.AreEqual(
                new[] { InfernalInvaderTraversalCatalogueValidationIssue.RecipeInvalid },
                InfernalInvaderTraversalValidator.ValidateCatalogue(invalidCatalogue).Issues);
        }

        [Test]
        public void ValidationResults_AreImmutableSnapshots()
        {
            var validation = InfernalInvaderTraversalValidator.Validate(null);
            var catalogue = InfernalInvaderTraversalValidator.ValidateCatalogue(null);

            Assert.Throws<NotSupportedException>(() =>
                ((IList<InfernalInvaderTraversalValidationIssue>)validation.Issues).Add(
                    InfernalInvaderTraversalValidationIssue.LayoutInvalid));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<InfernalInvaderTraversalCatalogueValidationIssue>)catalogue.Issues).Add(
                    InfernalInvaderTraversalCatalogueValidationIssue.RecipeInvalid));
        }

        private static void AssertMissingRole(
            InfernalInvaderTraversalRecipe source,
            int index,
            InfernalInvaderTraversalValidationIssue expected)
        {
            var nodes = Copy(source.NodeIds);
            nodes[index] = nodes[1];
            var result = InfernalInvaderTraversalValidator.Validate(Copy(source, nodes: nodes));
            Assert.That(result.Issues, Does.Contain(expected));
        }

        private static void AssertLookup(
            string layoutId,
            InfernalInvaderTraversalLookupStatus expected)
        {
            var result = StarterInfernalInvaderTraversalResolver.ResolveExact(layoutId);
            Assert.That(result.Status, Is.EqualTo(expected));
            Assert.That(result.Found, Is.False);
            Assert.That(result.Recipe, Is.Null);
        }

        private static void AssertRoute(
            InfernalInvaderTraversalRecipe route,
            string layoutId,
            IReadOnlyList<string> nodes,
            IReadOnlyList<string> edgeIds,
            IReadOnlyList<InfernalTraversalClass> classes,
            string lavaEdgeId)
        {
            Assert.That(route.LayoutId, Is.EqualTo(layoutId));
            Assert.That(route.PacingLayoutId, Is.EqualTo(layoutId));
            CollectionAssert.AreEqual(nodes, route.NodeIds);
            CollectionAssert.AreEqual(edgeIds, EdgeIds(route.EdgeSteps));
            CollectionAssert.AreEqual(classes, Classes(route.EdgeSteps));
            Assert.That(route.LavaGate.EdgeId, Is.EqualTo(lavaEdgeId));
            Assert.That(route.LavaGate.NormalizedFraction, Is.EqualTo(.5f));
        }

        private static InfernalInvaderTraversalRecipe Copy(
            InfernalInvaderTraversalRecipe source,
            string layoutId = null,
            string pacingLayoutId = null,
            IReadOnlyList<string> nodes = null,
            IReadOnlyList<InfernalTraversalEdgeStep> steps = null,
            InfernalLavaGatePlacement lavaGate = null)
        {
            return new InfernalInvaderTraversalRecipe(
                layoutId ?? source.LayoutId,
                pacingLayoutId ?? source.PacingLayoutId,
                nodes ?? source.NodeIds,
                steps ?? source.EdgeSteps,
                lavaGate ?? source.LavaGate);
        }

        private static string[] Copy(IReadOnlyList<string> source)
        {
            var copy = new string[source.Count];
            for (var index = 0; index < source.Count; index++) copy[index] = source[index];
            return copy;
        }

        private static InfernalTraversalEdgeStep[] Copy(
            IReadOnlyList<InfernalTraversalEdgeStep> source)
        {
            var copy = new InfernalTraversalEdgeStep[source.Count];
            for (var index = 0; index < source.Count; index++) copy[index] = source[index];
            return copy;
        }

        private static string[] EdgeIds(IReadOnlyList<InfernalTraversalEdgeStep> steps)
        {
            var values = new string[steps.Count];
            for (var index = 0; index < steps.Count; index++) values[index] = steps[index].EdgeId;
            return values;
        }

        private static InfernalTraversalClass[] Classes(
            IReadOnlyList<InfernalTraversalEdgeStep> steps)
        {
            var values = new InfernalTraversalClass[steps.Count];
            for (var index = 0; index < steps.Count; index++) values[index] = steps[index].TraversalClass;
            return values;
        }
    }
}
