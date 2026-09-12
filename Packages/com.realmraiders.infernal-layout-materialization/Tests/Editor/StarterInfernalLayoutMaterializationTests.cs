using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.InfernalLayoutMaterialization.Tests
{
    public sealed class StarterInfernalLayoutMaterializationTests
    {
        [TestCaseSource(nameof(CachedSnapshots))]
        public void CachedRecipe_PreservesExactThemeAndAllRoleMappings(
            RecipeSnapshot expected)
        {
            var result = StarterInfernalLayoutMaterializationResolver.ResolveExact(
                expected.LayoutId);
            var recipe = result.Recipe;

            Assert.That(result.Status,
                Is.EqualTo(InfernalLayoutMaterializationLookupStatus.Found));
            Assert.That(recipe.LayoutId, Is.EqualTo(expected.LayoutId));
            Assert.That(recipe.Theme.ThemeId, Is.EqualTo(expected.ThemeId));
            CollectionAssert.AreEqual(expected.ThemeTokens, recipe.Theme.PresentationTokens);
            Assert.That(recipe.Mappings.Count, Is.EqualTo(expected.Mappings.Count));
            for (var index = 0; index < expected.Mappings.Count; index++)
            {
                var actual = recipe.Mappings[index];
                var snapshot = expected.Mappings[index];

                Assert.That(actual.RoleId, Is.EqualTo(snapshot.RoleId));
                Assert.That(actual.RoleKind, Is.EqualTo(snapshot.RoleKind));
                Assert.That(actual.FloorTreatmentId, Is.EqualTo(snapshot.FloorTreatmentId));
                Assert.That(actual.BoundarySilhouetteId,
                    Is.EqualTo(snapshot.BoundarySilhouetteId));
                Assert.That(actual.LandmarkPresentationId,
                    Is.EqualTo(snapshot.LandmarkPresentationId));
                Assert.That(actual.NodeFootprintRadius,
                    Is.EqualTo(RealmLayoutGraphValidator.NodeFootprintRadius));
                CollectionAssert.AreEqual(
                    snapshot.RouteWidthSourceEdgeIds,
                    actual.RouteWidthSourceEdgeIds);
            }
        }

        [Test]
        public void Catalogue_UsesCachedIdentityAndDistinctSemanticPresentationSignatures()
        {
            var validation = InfernalLayoutMaterializationValidator.ValidateCatalogue(
                StarterInfernalLayoutMaterializations.All);
            var ashen = InfernalLayoutMaterializationValidator.CreateSemanticPresentationSignature(
                StarterInfernalLayoutMaterializations.AshenSpur);
            var cinder = InfernalLayoutMaterializationValidator.CreateSemanticPresentationSignature(
                StarterInfernalLayoutMaterializations.CinderFork);
            var ember = InfernalLayoutMaterializationValidator.CreateSemanticPresentationSignature(
                StarterInfernalLayoutMaterializations.EmberCircuit);

            Assert.That(validation.IsValid, Is.True);
            Assert.That(StarterInfernalLayoutMaterializations.All[0],
                Is.SameAs(StarterInfernalLayoutMaterializations.AshenSpur));
            Assert.That(StarterInfernalLayoutMaterializations.All[1],
                Is.SameAs(StarterInfernalLayoutMaterializations.CinderFork));
            Assert.That(StarterInfernalLayoutMaterializations.All[2],
                Is.SameAs(StarterInfernalLayoutMaterializations.EmberCircuit));
            Assert.That(ashen, Is.Not.EqualTo(cinder));
            Assert.That(ashen, Is.Not.EqualTo(ember));
            Assert.That(cinder, Is.Not.EqualTo(ember));
        }

        [Test]
        public void ResolveExact_IsStrictAndCachedAndSnapshotsAreImmutable()
        {
            var found = StarterInfernalLayoutMaterializationResolver.ResolveExact(
                StarterInfernalDefenseLayouts.CinderForkId);
            var unknown = StarterInfernalLayoutMaterializationResolver.ResolveExact(
                "realmraiders.infernal-defense.unknown");
            var changedCase = StarterInfernalLayoutMaterializationResolver.ResolveExact(
                "Realmraiders.infernal-defense.cinder-fork");
            var padded = StarterInfernalLayoutMaterializationResolver.ResolveExact(
                StarterInfernalDefenseLayouts.CinderForkId + " ");

            Assert.That(found.Recipe,
                Is.SameAs(StarterInfernalLayoutMaterializations.CinderFork));
            Assert.That(unknown.Status,
                Is.EqualTo(InfernalLayoutMaterializationLookupStatus.NotFound));
            Assert.That(changedCase.Status,
                Is.EqualTo(InfernalLayoutMaterializationLookupStatus.LayoutIdInvalid));
            Assert.That(padded.Status,
                Is.EqualTo(InfernalLayoutMaterializationLookupStatus.LayoutIdInvalid));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterInfernalLayoutMaterializations.All).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterInfernalLayoutMaterializations.AshenSpur.Mappings).RemoveAt(0));
            Assert.Throws<NotSupportedException>(() =>
                ((IList)StarterInfernalLayoutMaterializations.AshenSpur.Theme.PresentationTokens)
                    .RemoveAt(0));
        }

        [Test]
        public void Validate_FailsClosedForNullMissingExtraDuplicateWrongAndUnknownMappings()
        {
            var source = StarterInfernalLayoutMaterializations.AshenSpur;
            var missing = CopyWithMappings(source, RemoveMapping(source.Mappings, 5));
            var extra = CopyWithMappings(source, AppendMapping(source.Mappings, source.Mappings[0]));
            var nullMapping = CopyWithMappings(
                source,
                ReplaceMapping(source.Mappings, 2, null));
            var unknown = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    2,
                    CopyMapping(source.Mappings[2], roleId: "realmraiders.infernal.unknown")));
            var wrongLayout = new InfernalLayoutMaterializationRecipe(
                "realmraiders.infernal-defense.unknown",
                source.Theme,
                source.Mappings);

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.RecipeMissing
                },
                InfernalLayoutMaterializationValidator.Validate(null).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.MappingCardinalityInvalid,
                    InfernalLayoutMaterializationValidationIssue.MappingRoleCoverageInvalid
                },
                InfernalLayoutMaterializationValidator.Validate(missing).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.MappingCardinalityInvalid,
                    InfernalLayoutMaterializationValidationIssue.RoleDuplicate
                },
                InfernalLayoutMaterializationValidator.Validate(extra).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.MappingMissing,
                    InfernalLayoutMaterializationValidationIssue.MappingRoleCoverageInvalid
                },
                InfernalLayoutMaterializationValidator.Validate(nullMapping).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.RoleIdInvalid,
                    InfernalLayoutMaterializationValidationIssue.RoleKindInvalid,
                    InfernalLayoutMaterializationValidationIssue.FloorTreatmentIdNotAllowed,
                    InfernalLayoutMaterializationValidationIssue.RouteWidthSourceInvalid,
                    InfernalLayoutMaterializationValidationIssue.MappingRoleCoverageInvalid
                },
                InfernalLayoutMaterializationValidator.Validate(unknown).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.LayoutNotFound
                },
                InfernalLayoutMaterializationValidator.Validate(wrongLayout).Issues);
        }

        [Test]
        public void Validate_FailsClosedForInvalidEnumFootprintAndLandmarkBinding()
        {
            var source = StarterInfernalLayoutMaterializations.CinderFork;
            var invalidEnum = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    1,
                    CopyMapping(
                        source.Mappings[1],
                        roleKind: InfernalLayoutMaterializationRoleKind.Unknown)));
            var nonfinite = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    0,
                    CopyMapping(source.Mappings[0], nodeFootprintRadius: float.NaN)));
            var rangeMismatch = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    0,
                    CopyMapping(source.Mappings[0], nodeFootprintRadius: 1f)));
            var landmarkMismatch = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    1,
                    CopyMapping(
                        source.Mappings[1],
                        landmarkPresentationId:
                            StarterInfernalLayoutMaterializations.NonePresentationId)));
            var routeMismatch = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    0,
                    CopyMapping(
                        source.Mappings[0],
                        routeWidthSourceEdgeIds: new[]
                        {
                            "cinder.junction-hellhound"
                        })));

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.RoleKindInvalid
                },
                InfernalLayoutMaterializationValidator.Validate(invalidEnum).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.NodeFootprintInvalid
                },
                InfernalLayoutMaterializationValidator.Validate(nonfinite).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.NodeFootprintRangeMismatch
                },
                InfernalLayoutMaterializationValidator.Validate(rangeMismatch).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.LandmarkBindingInvalid
                },
                InfernalLayoutMaterializationValidator.Validate(landmarkMismatch).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.RouteWidthSourceInvalid,
                    InfernalLayoutMaterializationValidationIssue.RouteWidthSourceCoverageInvalid
                },
                InfernalLayoutMaterializationValidator.Validate(routeMismatch).Issues);
        }

        [Test]
        public void Validate_RequiresClosedThemeFloorBoundaryAndLandmarkFacts()
        {
            var source = StarterInfernalLayoutMaterializations.AshenSpur;
            var wrongThemeId = CopyWithTheme(
                source,
                new InfernalLayoutPresentationTheme(
                    "realmraiders.infernal-presentation.theme.cinder-fork",
                    source.Theme.PresentationTokens));
            var wrongTokens = CopyWithTheme(
                source,
                new InfernalLayoutPresentationTheme(
                    source.Theme.ThemeId,
                    new[]
                    {
                        "realmraiders.infernal-presentation.token.cinder",
                        "realmraiders.infernal-presentation.token.basalt"
                    }));
            var wrongFloor = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    0,
                    CopyMapping(
                        source.Mappings[0],
                        floorTreatmentId:
                            "realmraiders.infernal-presentation.floor.route-junction")));
            var wrongBoundary = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    0,
                    CopyMapping(
                        source.Mappings[0],
                        boundarySilhouetteId:
                            "realmraiders.infernal-presentation.boundary.cinder-fork")));
            var wrongLandmark = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    1,
                    CopyMapping(
                        source.Mappings[1],
                        landmarkPresentationId:
                            "realmraiders.infernal-defense.cinder-fork")));
            var unknownLandmark = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    1,
                    CopyMapping(
                        source.Mappings[1],
                        landmarkPresentationId:
                            "realmraiders.infernal-defense.unknown-landmark")));

            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.ThemeIdNotAllowed
                },
                InfernalLayoutMaterializationValidator.Validate(wrongThemeId).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.ThemeTokenSetMismatch
                },
                InfernalLayoutMaterializationValidator.Validate(wrongTokens).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.FloorTreatmentIdNotAllowed
                },
                InfernalLayoutMaterializationValidator.Validate(wrongFloor).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.BoundarySilhouetteIdNotAllowed
                },
                InfernalLayoutMaterializationValidator.Validate(wrongBoundary).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.LandmarkBindingInvalid
                },
                InfernalLayoutMaterializationValidator.Validate(wrongLandmark).Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationValidationIssue.LandmarkPresentationIdNotAllowed,
                    InfernalLayoutMaterializationValidationIssue.LandmarkBindingInvalid
                },
                InfernalLayoutMaterializationValidator.Validate(unknownLandmark).Issues);
        }

        [Test]
        public void ValidateCatalogue_IgnoresRouteSourceProvenanceInPresentationSignature()
        {
            var source = StarterInfernalLayoutMaterializations.AshenSpur;
            var reorderedProvenance = CopyWithMappings(
                source,
                ReplaceMapping(
                    source.Mappings,
                    1,
                    CopyMapping(
                        source.Mappings[1],
                        routeWidthSourceEdgeIds: new[]
                        {
                            "ashen.junction-flame",
                            "ashen.junction-hellhound",
                            "ashen.entry-junction"
                        })));
            var catalogue = InfernalLayoutMaterializationValidator.ValidateCatalogue(
                new InfernalLayoutMaterializationRecipe[]
                {
                    source,
                    reorderedProvenance,
                    StarterInfernalLayoutMaterializations.CinderFork,
                    StarterInfernalLayoutMaterializations.EmberCircuit
                });

            Assert.That(InfernalLayoutMaterializationValidator.Validate(reorderedProvenance).IsValid,
                Is.True);
            Assert.That(
                InfernalLayoutMaterializationValidator.CreateSemanticPresentationSignature(source),
                Is.EqualTo(
                    InfernalLayoutMaterializationValidator.CreateSemanticPresentationSignature(
                        reorderedProvenance)));
            CollectionAssert.AreEqual(
                new[]
                {
                    InfernalLayoutMaterializationCatalogueValidationIssue.RecipeCardinalityInvalid,
                    InfernalLayoutMaterializationCatalogueValidationIssue.LayoutIdDuplicate,
                    InfernalLayoutMaterializationCatalogueValidationIssue.SemanticSignatureDuplicate
                },
                catalogue.Issues);
        }

        private static IEnumerable CachedSnapshots
        {
            get
            {
                yield return new TestCaseData(Snapshot(
                    StarterInfernalDefenseLayouts.AshenSpurId,
                    "realmraiders.infernal-presentation.theme.ashen-spur",
                    new[]
                    {
                        "realmraiders.infernal-presentation.token.ash",
                        "realmraiders.infernal-presentation.token.basalt"
                    },
                    "realmraiders.infernal-presentation.boundary.ashen-spur",
                    "realmraiders.infernal-defense.ash-crucible",
                    "ashen")).SetName("AshenSpur exact mappings");
                yield return new TestCaseData(Snapshot(
                    StarterInfernalDefenseLayouts.CinderForkId,
                    "realmraiders.infernal-presentation.theme.cinder-fork",
                    new[]
                    {
                        "realmraiders.infernal-presentation.token.cinder",
                        "realmraiders.infernal-presentation.token.basalt"
                    },
                    "realmraiders.infernal-presentation.boundary.cinder-fork",
                    "realmraiders.infernal-defense.cinder-fork",
                    "cinder")).SetName("CinderFork exact mappings");
                yield return new TestCaseData(Snapshot(
                    StarterInfernalDefenseLayouts.EmberCircuitId,
                    "realmraiders.infernal-presentation.theme.ember-circuit",
                    new[]
                    {
                        "realmraiders.infernal-presentation.token.ember",
                        "realmraiders.infernal-presentation.token.obsidian"
                    },
                    "realmraiders.infernal-presentation.boundary.ember-circuit",
                    "realmraiders.infernal-defense.circuit-brazier",
                    "ember")).SetName("EmberCircuit exact mappings");
            }
        }

        private static RecipeSnapshot Snapshot(
            string layoutId,
            string themeId,
            IReadOnlyList<string> themeTokens,
            string boundaryId,
            string junctionPresentationId,
            string prefix)
        {
            var junctionEdges = new[]
            {
                prefix + ".entry-junction",
                prefix + ".junction-hellhound",
                prefix + ".junction-flame"
            };
            var hellhoundEdges = new[]
            {
                prefix + ".junction-hellhound",
                prefix + ".hellhound-brute"
            };
            var flameEdges = new[]
            {
                prefix + ".junction-flame",
                prefix + ".flame-brute"
            };
            var bruteEdges = new[]
            {
                prefix + ".hellhound-brute",
                prefix + ".flame-brute",
                prefix + ".brute-heart"
            };
            if (string.Equals(prefix, "ashen", StringComparison.Ordinal))
            {
                hellhoundEdges = new[]
                {
                    "ashen.junction-hellhound"
                };
                bruteEdges = new[]
                {
                    "ashen.flame-brute",
                    "ashen.brute-heart"
                };
            }
            else if (string.Equals(prefix, "ember", StringComparison.Ordinal))
            {
                junctionEdges = new[]
                {
                    "ember.entry-junction",
                    "ember.junction-hellhound",
                    "ember.junction-brute"
                };
                hellhoundEdges = new[]
                {
                    "ember.junction-hellhound",
                    "ember.hellhound-flame"
                };
                flameEdges = new[]
                {
                    "ember.hellhound-flame",
                    "ember.flame-brute"
                };
                bruteEdges = new[]
                {
                    "ember.flame-brute",
                    "ember.junction-brute",
                    "ember.brute-heart"
                };
            }

            return new RecipeSnapshot(
                layoutId,
                themeId,
                themeTokens,
                new[]
                {
                    Mapping(StarterInfernalDefenseLayouts.InvaderEntryRoleId, InfernalLayoutMaterializationRoleKind.EntryGate, "floor.entry-gate", boundaryId, StarterInfernalLayoutMaterializations.NonePresentationId, prefix + ".entry-junction"),
                    Mapping(StarterInfernalDefenseLayouts.RouteJunctionRoleId, InfernalLayoutMaterializationRoleKind.RouteJunction, "floor.route-junction", boundaryId, junctionPresentationId, junctionEdges),
                    Mapping(StarterInfernalDefenseLayouts.HellhoundEncounterRoleId, InfernalLayoutMaterializationRoleKind.HellhoundGround, "floor.hellhound-ground", boundaryId, StarterInfernalLayoutMaterializations.NonePresentationId, hellhoundEdges),
                    Mapping(StarterInfernalDefenseLayouts.FlameTrapHazardRoleId, InfernalLayoutMaterializationRoleKind.FlameTrapGround, "floor.flame-trap-ground", boundaryId, StarterInfernalLayoutMaterializations.NonePresentationId, flameEdges),
                    Mapping(StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId, InfernalLayoutMaterializationRoleKind.BruteGround, "floor.brute-ground", boundaryId, StarterInfernalLayoutMaterializations.NonePresentationId, bruteEdges),
                    Mapping(StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId, InfernalLayoutMaterializationRoleKind.InfernalHeart, "floor.infernal-heart", boundaryId, "realmraiders.infernal-defense.heart-altar", prefix + ".brute-heart")
                });
        }

        private static MappingSnapshot Mapping(
            string roleId,
            InfernalLayoutMaterializationRoleKind roleKind,
            string floorSuffix,
            string boundarySilhouetteId,
            string landmarkPresentationId,
            params string[] routeWidthSourceEdgeIds)
        {
            return new MappingSnapshot(
                roleId,
                roleKind,
                "realmraiders.infernal-presentation." + floorSuffix,
                boundarySilhouetteId,
                landmarkPresentationId,
                routeWidthSourceEdgeIds);
        }

        private static InfernalLayoutMaterializationRecipe CopyWithMappings(
            InfernalLayoutMaterializationRecipe source,
            IReadOnlyList<InfernalLayoutMaterializationFact> mappings)
        {
            return new InfernalLayoutMaterializationRecipe(
                source.LayoutId,
                source.Theme,
                mappings);
        }

        private static InfernalLayoutMaterializationRecipe CopyWithTheme(
            InfernalLayoutMaterializationRecipe source,
            InfernalLayoutPresentationTheme theme)
        {
            return new InfernalLayoutMaterializationRecipe(
                source.LayoutId,
                theme,
                source.Mappings);
        }

        private static InfernalLayoutMaterializationFact CopyMapping(
            InfernalLayoutMaterializationFact source,
            string roleId = null,
            InfernalLayoutMaterializationRoleKind? roleKind = null,
            string floorTreatmentId = null,
            string boundarySilhouetteId = null,
            string landmarkPresentationId = null,
            float? nodeFootprintRadius = null,
            IReadOnlyList<string> routeWidthSourceEdgeIds = null)
        {
            return new InfernalLayoutMaterializationFact(
                roleId ?? source.RoleId,
                roleKind ?? source.RoleKind,
                floorTreatmentId ?? source.FloorTreatmentId,
                boundarySilhouetteId ?? source.BoundarySilhouetteId,
                landmarkPresentationId ?? source.LandmarkPresentationId,
                nodeFootprintRadius ?? source.NodeFootprintRadius,
                routeWidthSourceEdgeIds ?? source.RouteWidthSourceEdgeIds);
        }

        private static IReadOnlyList<InfernalLayoutMaterializationFact> ReplaceMapping(
            IReadOnlyList<InfernalLayoutMaterializationFact> source,
            int index,
            InfernalLayoutMaterializationFact replacement)
        {
            var copy = new InfernalLayoutMaterializationFact[source.Count];
            for (var sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
            {
                copy[sourceIndex] = sourceIndex == index ? replacement : source[sourceIndex];
            }

            return copy;
        }

        private static IReadOnlyList<InfernalLayoutMaterializationFact> RemoveMapping(
            IReadOnlyList<InfernalLayoutMaterializationFact> source,
            int index)
        {
            var copy = new InfernalLayoutMaterializationFact[source.Count - 1];
            for (var sourceIndex = 0; sourceIndex < copy.Length; sourceIndex++)
            {
                copy[sourceIndex] = sourceIndex < index
                    ? source[sourceIndex]
                    : source[sourceIndex + 1];
            }

            return copy;
        }

        private static IReadOnlyList<InfernalLayoutMaterializationFact> AppendMapping(
            IReadOnlyList<InfernalLayoutMaterializationFact> source,
            InfernalLayoutMaterializationFact appended)
        {
            var copy = new InfernalLayoutMaterializationFact[source.Count + 1];
            for (var sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
            {
                copy[sourceIndex] = source[sourceIndex];
            }

            copy[source.Count] = appended;
            return copy;
        }

        public sealed class RecipeSnapshot
        {
            public RecipeSnapshot(
                string layoutId,
                string themeId,
                IReadOnlyList<string> themeTokens,
                IReadOnlyList<MappingSnapshot> mappings)
            {
                LayoutId = layoutId;
                ThemeId = themeId;
                ThemeTokens = themeTokens;
                Mappings = mappings;
            }

            public string LayoutId { get; }

            public string ThemeId { get; }

            public IReadOnlyList<string> ThemeTokens { get; }

            public IReadOnlyList<MappingSnapshot> Mappings { get; }
        }

        public sealed class MappingSnapshot
        {
            public MappingSnapshot(
                string roleId,
                InfernalLayoutMaterializationRoleKind roleKind,
                string floorTreatmentId,
                string boundarySilhouetteId,
                string landmarkPresentationId,
                IReadOnlyList<string> routeWidthSourceEdgeIds)
            {
                RoleId = roleId;
                RoleKind = roleKind;
                FloorTreatmentId = floorTreatmentId;
                BoundarySilhouetteId = boundarySilhouetteId;
                LandmarkPresentationId = landmarkPresentationId;
                RouteWidthSourceEdgeIds = routeWidthSourceEdgeIds;
            }

            public string RoleId { get; }

            public InfernalLayoutMaterializationRoleKind RoleKind { get; }

            public string FloorTreatmentId { get; }

            public string BoundarySilhouetteId { get; }

            public string LandmarkPresentationId { get; }

            public IReadOnlyList<string> RouteWidthSourceEdgeIds { get; }
        }
    }
}
