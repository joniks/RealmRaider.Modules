using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalFirstExpansionTactics;

namespace RealmRaiders.Modules.InfernalExpansionTacticPresentation
{
    public enum TacticTelegraphAnchorBindingKind
    {
        ActorGroundChevron,
        HazardGroundRing,
        ActorTargetLane
    }

    public enum TacticTelegraphDirectionBinding
    {
        None,
        CurrentInvaderDirection
    }

    public enum TacticTelegraphLookupStatus
    {
        Found,
        RecipeMissing,
        TacticIdInvalid,
        RecipeNotCachedExact,
        TacticIdMismatch,
        CatalogueInvalid
    }

    public enum TacticTelegraphValidationIssue
    {
        CatalogueCardinalityInvalid,
        CatalogueOrderMismatch,
        RecipeMissing,
        TacticRecipeMissing,
        TacticRecipeReferenceMismatch,
        TacticIdInvalid,
        TacticIdMismatch,
        TacticIdDuplicate,
        MotifInvalid,
        MotifMismatch,
        PaletteInvalid,
        PaletteMismatch,
        DurationInvalid,
        DurationMismatch,
        DirectionBindingInvalid,
        DirectionBindingMismatch,
        AnchorCardinalityInvalid,
        AnchorMissing,
        AnchorBindingKindInvalid,
        AnchorBindingKindMismatch,
        AnchorBindingIndexInvalid,
        AnchorBindingIndexMismatch,
        AnchorOrderMismatch,
        AnchorScaleInvalid,
        AnchorScaleMismatch,
        AnchorWidthInvalid,
        AnchorWidthMismatch,
        AnchorLengthInvalid,
        AnchorLengthMismatch,
        AnchorIntensityInvalid,
        AnchorIntensityMismatch,
        NonRaycastRequired,
        ColliderFreeRequired,
        CameraAuthorityForbidden,
        RendererBudgetInvalid,
        RendererBudgetMismatch,
        SharedMaterialBudgetInvalid,
        SharedMaterialBudgetMismatch,
        PerFrameCatalogueWorkForbidden
    }

    public sealed class TacticTelegraphAnchor
    {
        public TacticTelegraphAnchor(
            TacticTelegraphAnchorBindingKind bindingKind,
            int bindingIndex,
            float normalizedLocalScale,
            float normalizedWidth,
            float normalizedLength,
            float paletteIntensity)
        {
            BindingKind = bindingKind;
            BindingIndex = bindingIndex;
            NormalizedLocalScale = normalizedLocalScale;
            NormalizedWidth = normalizedWidth;
            NormalizedLength = normalizedLength;
            PaletteIntensity = paletteIntensity;
        }

        public TacticTelegraphAnchorBindingKind BindingKind { get; }
        public int BindingIndex { get; }
        public float NormalizedLocalScale { get; }
        public float NormalizedWidth { get; }
        public float NormalizedLength { get; }
        public float PaletteIntensity { get; }
    }

    public sealed class TacticTelegraphRecipe
    {
        public TacticTelegraphRecipe(
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
            TacticRecipe = tacticRecipe;
            TacticId = tacticId;
            MotifId = motifId;
            PaletteId = paletteId;
            DurationSeconds = durationSeconds;
            DirectionBinding = directionBinding;
            Anchors = Snapshot(anchors);
            NonRaycast = nonRaycast;
            ColliderFree = colliderFree;
            CameraAuthority = cameraAuthority;
            RendererBudget = rendererBudget;
            SharedMaterialBudget = sharedMaterialBudget;
            HasPerFrameCatalogueWork = hasPerFrameCatalogueWork;
        }

        public TacticRecipe TacticRecipe { get; }
        public string TacticId { get; }
        public string MotifId { get; }
        public string PaletteId { get; }
        public float DurationSeconds { get; }
        public TacticTelegraphDirectionBinding DirectionBinding { get; }
        public IReadOnlyList<TacticTelegraphAnchor> Anchors { get; }
        public bool NonRaycast { get; }
        public bool ColliderFree { get; }
        public bool CameraAuthority { get; }
        public int RendererBudget { get; }
        public int SharedMaterialBudget { get; }
        public bool HasPerFrameCatalogueWork { get; }

        private static IReadOnlyList<TacticTelegraphAnchor> Snapshot(
            IReadOnlyList<TacticTelegraphAnchor> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<TacticTelegraphAnchor>());
            }

            var copy = new TacticTelegraphAnchor[source.Count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class TacticTelegraphLookupResult
    {
        internal TacticTelegraphLookupResult(
            TacticTelegraphLookupStatus status,
            TacticTelegraphRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public TacticTelegraphLookupStatus Status { get; }
        public TacticTelegraphRecipe Recipe { get; }
        public bool Found => Status == TacticTelegraphLookupStatus.Found
            && Recipe != null;
    }

    public sealed class TacticTelegraphValidationResult
    {
        internal TacticTelegraphValidationResult(
            IReadOnlyList<TacticTelegraphValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<TacticTelegraphValidationIssue> Issues { get; }
        public bool IsValid => Issues.Count == 0;

        private static IReadOnlyList<TacticTelegraphValidationIssue> Snapshot(
            IReadOnlyList<TacticTelegraphValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<TacticTelegraphValidationIssue>());
            }

            var copy = new TacticTelegraphValidationIssue[source.Count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public static class StarterInfernalExpansionTacticTelegraphs
    {
        public const int MaximumRendererBudget = 3;
        public const int ExactSharedMaterialBudget = 1;
        public const float MaximumDurationSeconds = 1f;
        public const float MaximumNormalizedLocalScale = 2f;
        public const float MaximumNormalizedDimension = 3f;
        public const float MaximumPaletteIntensity = 1f;

        private const int AuthoredRecipeCount = 3;

        private static readonly AuthoredDefinition[] Definitions =
        {
            new AuthoredDefinition(
                StarterInfernalFirstExpansionTactics.AshenPackVent,
                "pack-pincer",
                "ember-flank-chevrons",
                "infernal-amber",
                0.65f,
                TacticTelegraphDirectionBinding.None,
                new AuthoredAnchor(
                    TacticTelegraphAnchorBindingKind.ActorGroundChevron,
                    0,
                    0.8f,
                    0.6f,
                    1.2f,
                    0.85f),
                new AuthoredAnchor(
                    TacticTelegraphAnchorBindingKind.ActorGroundChevron,
                    1,
                    0.8f,
                    0.6f,
                    1.2f,
                    0.85f)),
            new AuthoredDefinition(
                StarterInfernalFirstExpansionTactics.CinderSnareVent,
                "snare-counterattack",
                "cinder-trap-ring",
                "cinder-orange",
                0.75f,
                TacticTelegraphDirectionBinding.None,
                new AuthoredAnchor(
                    TacticTelegraphAnchorBindingKind.HazardGroundRing,
                    0,
                    1f,
                    1.6f,
                    1.6f,
                    0.9f)),
            new AuthoredDefinition(
                StarterInfernalFirstExpansionTactics.BruteKilnVent,
                "brute-intercept",
                "brute-charge-lane",
                "obsidian-red",
                0.60f,
                TacticTelegraphDirectionBinding.CurrentInvaderDirection,
                new AuthoredAnchor(
                    TacticTelegraphAnchorBindingKind.ActorTargetLane,
                    0,
                    1f,
                    0.75f,
                    2.5f,
                    1f))
        };

        static StarterInfernalExpansionTacticTelegraphs()
        {
            PackPincer = Create(Definitions[0]);
            SnareCounterattack = Create(Definitions[1]);
            BruteIntercept = Create(Definitions[2]);
            All = Array.AsReadOnly(
                new[] { PackPincer, SnareCounterattack, BruteIntercept });
        }

        public static TacticTelegraphRecipe PackPincer { get; }
        public static TacticTelegraphRecipe SnareCounterattack { get; }
        public static TacticTelegraphRecipe BruteIntercept { get; }
        public static IReadOnlyList<TacticTelegraphRecipe> All { get; }

        public static TacticTelegraphLookupResult FindExact(
            TacticRecipe tacticRecipe,
            string tacticId)
        {
            if (tacticRecipe == null)
            {
                return Reject(TacticTelegraphLookupStatus.RecipeMissing);
            }

            if (!IsStableId(tacticId))
            {
                return Reject(TacticTelegraphLookupStatus.TacticIdInvalid);
            }

            if (!Validate(All).IsValid)
            {
                return Reject(TacticTelegraphLookupStatus.CatalogueInvalid);
            }

            if (CachedTacticIndex(tacticRecipe) < 0)
            {
                return Reject(TacticTelegraphLookupStatus.RecipeNotCachedExact);
            }

            if (!Same(tacticRecipe.TacticId, tacticId))
            {
                return Reject(TacticTelegraphLookupStatus.TacticIdMismatch);
            }

            for (var index = 0; index < All.Count; index++)
            {
                var candidate = All[index];
                if (ReferenceEquals(candidate.TacticRecipe, tacticRecipe)
                    && Same(candidate.TacticId, tacticId))
                {
                    return new TacticTelegraphLookupResult(
                        TacticTelegraphLookupStatus.Found,
                        candidate);
                }
            }

            return Reject(TacticTelegraphLookupStatus.CatalogueInvalid);
        }

        public static TacticTelegraphValidationResult Validate(
            IReadOnlyList<TacticTelegraphRecipe> recipes)
        {
            var issues = new List<TacticTelegraphValidationIssue>();
            if (recipes == null || recipes.Count != AuthoredRecipeCount)
            {
                Add(issues, TacticTelegraphValidationIssue.CatalogueCardinalityInvalid);
            }

            if (recipes == null)
            {
                return new TacticTelegraphValidationResult(issues);
            }

            var tacticIds = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < recipes.Count; index++)
            {
                ValidateRecipe(recipes[index], tacticIds, issues);
            }

            if (HasExactAuthoredMultiset(recipes) && !HasAcceptedOrder(recipes))
            {
                Add(issues, TacticTelegraphValidationIssue.CatalogueOrderMismatch);
            }

            return new TacticTelegraphValidationResult(issues);
        }

        private static void ValidateRecipe(
            TacticTelegraphRecipe recipe,
            ISet<string> tacticIds,
            ICollection<TacticTelegraphValidationIssue> issues)
        {
            if (recipe == null)
            {
                Add(issues, TacticTelegraphValidationIssue.RecipeMissing);
                return;
            }

            if (!IsStableId(recipe.TacticId))
            {
                Add(issues, TacticTelegraphValidationIssue.TacticIdInvalid);
            }
            else if (!tacticIds.Add(recipe.TacticId))
            {
                Add(issues, TacticTelegraphValidationIssue.TacticIdDuplicate);
            }

            if (recipe.TacticRecipe == null)
            {
                Add(issues, TacticTelegraphValidationIssue.TacticRecipeMissing);
            }
            else
            {
                if (CachedTacticIndex(recipe.TacticRecipe) < 0)
                {
                    Add(
                        issues,
                        TacticTelegraphValidationIssue.TacticRecipeReferenceMismatch);
                }

                if (!Same(recipe.TacticId, recipe.TacticRecipe.TacticId))
                {
                    Add(issues, TacticTelegraphValidationIssue.TacticIdMismatch);
                }
            }

            var definition = Definition(recipe.TacticId);
            if (definition != null
                && !ReferenceEquals(recipe.TacticRecipe, definition.TacticRecipe))
            {
                Add(
                    issues,
                    TacticTelegraphValidationIssue.TacticRecipeReferenceMismatch);
            }

            ValidatePresentationFacts(recipe, definition, issues);
            ValidateAnchors(recipe, definition, issues);
            ValidateSafetyAndBudget(recipe, issues);
        }

        private static void ValidatePresentationFacts(
            TacticTelegraphRecipe recipe,
            AuthoredDefinition definition,
            ICollection<TacticTelegraphValidationIssue> issues)
        {
            ValidateStableId(
                recipe.MotifId,
                TacticTelegraphValidationIssue.MotifInvalid,
                issues);
            ValidateStableId(
                recipe.PaletteId,
                TacticTelegraphValidationIssue.PaletteInvalid,
                issues);

            if (!IsFinitePositiveBounded(
                recipe.DurationSeconds,
                MaximumDurationSeconds))
            {
                Add(issues, TacticTelegraphValidationIssue.DurationInvalid);
            }

            if (!Enum.IsDefined(
                typeof(TacticTelegraphDirectionBinding),
                recipe.DirectionBinding))
            {
                Add(issues, TacticTelegraphValidationIssue.DirectionBindingInvalid);
            }

            if (definition == null)
            {
                return;
            }

            if (!Same(recipe.MotifId, definition.MotifId))
            {
                Add(issues, TacticTelegraphValidationIssue.MotifMismatch);
            }

            if (!Same(recipe.PaletteId, definition.PaletteId))
            {
                Add(issues, TacticTelegraphValidationIssue.PaletteMismatch);
            }

            if (IsFinitePositiveBounded(
                recipe.DurationSeconds,
                MaximumDurationSeconds)
                && recipe.DurationSeconds != definition.DurationSeconds)
            {
                Add(issues, TacticTelegraphValidationIssue.DurationMismatch);
            }

            if (Enum.IsDefined(
                typeof(TacticTelegraphDirectionBinding),
                recipe.DirectionBinding)
                && recipe.DirectionBinding != definition.DirectionBinding)
            {
                Add(issues, TacticTelegraphValidationIssue.DirectionBindingMismatch);
            }
        }

        private static void ValidateAnchors(
            TacticTelegraphRecipe recipe,
            AuthoredDefinition definition,
            ICollection<TacticTelegraphValidationIssue> issues)
        {
            var expectedCount = definition == null ? -1 : definition.Anchors.Length;
            if (recipe.Anchors == null
                || expectedCount < 0
                || recipe.Anchors.Count != expectedCount)
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorCardinalityInvalid);
            }

            if (recipe.Anchors == null)
            {
                return;
            }

            var exactMultiset = definition != null
                && HasExactAnchorMultiset(recipe.Anchors, definition);
            for (var index = 0; index < recipe.Anchors.Count; index++)
            {
                var anchor = recipe.Anchors[index];
                if (anchor == null)
                {
                    Add(issues, TacticTelegraphValidationIssue.AnchorMissing);
                    continue;
                }

                ValidateAnchorBounds(anchor, recipe.TacticRecipe, issues);
                if (definition != null && index < definition.Anchors.Length)
                {
                    var expected = exactMultiset
                        ? FindAnchor(definition, anchor.BindingKind, anchor.BindingIndex)
                        : definition.Anchors[index];
                    ValidateExactAnchor(anchor, expected, issues);
                }
            }

            if (exactMultiset && !HasAcceptedAnchorOrder(recipe.Anchors, definition))
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorOrderMismatch);
            }
        }

        private static void ValidateAnchorBounds(
            TacticTelegraphAnchor anchor,
            TacticRecipe tacticRecipe,
            ICollection<TacticTelegraphValidationIssue> issues)
        {
            if (!Enum.IsDefined(
                typeof(TacticTelegraphAnchorBindingKind),
                anchor.BindingKind))
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorBindingKindInvalid);
            }

            var bindingIndexValid = false;
            if (tacticRecipe != null)
            {
                switch (anchor.BindingKind)
                {
                    case TacticTelegraphAnchorBindingKind.ActorGroundChevron:
                    case TacticTelegraphAnchorBindingKind.ActorTargetLane:
                        bindingIndexValid = anchor.BindingIndex >= 0
                            && anchor.BindingIndex < tacticRecipe.Actors.Count;
                        break;
                    case TacticTelegraphAnchorBindingKind.HazardGroundRing:
                        bindingIndexValid = anchor.BindingIndex == 0
                            && tacticRecipe.Hazard != null;
                        break;
                }
            }

            if (!bindingIndexValid)
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorBindingIndexInvalid);
            }

            if (!IsFinitePositiveBounded(
                anchor.NormalizedLocalScale,
                MaximumNormalizedLocalScale))
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorScaleInvalid);
            }

            if (!IsFinitePositiveBounded(
                anchor.NormalizedWidth,
                MaximumNormalizedDimension))
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorWidthInvalid);
            }

            if (!IsFinitePositiveBounded(
                anchor.NormalizedLength,
                MaximumNormalizedDimension))
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorLengthInvalid);
            }

            if (!IsFinitePositiveBounded(
                anchor.PaletteIntensity,
                MaximumPaletteIntensity))
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorIntensityInvalid);
            }
        }

        private static void ValidateExactAnchor(
            TacticTelegraphAnchor anchor,
            AuthoredAnchor expected,
            ICollection<TacticTelegraphValidationIssue> issues)
        {
            if (expected == null)
            {
                return;
            }

            if (Enum.IsDefined(
                typeof(TacticTelegraphAnchorBindingKind),
                anchor.BindingKind)
                && anchor.BindingKind != expected.BindingKind)
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorBindingKindMismatch);
            }

            if (anchor.BindingIndex >= 0
                && anchor.BindingIndex != expected.BindingIndex)
            {
                Add(issues, TacticTelegraphValidationIssue.AnchorBindingIndexMismatch);
            }

            AddFloatMismatch(
                anchor.NormalizedLocalScale,
                expected.NormalizedLocalScale,
                MaximumNormalizedLocalScale,
                TacticTelegraphValidationIssue.AnchorScaleMismatch,
                issues);
            AddFloatMismatch(
                anchor.NormalizedWidth,
                expected.NormalizedWidth,
                MaximumNormalizedDimension,
                TacticTelegraphValidationIssue.AnchorWidthMismatch,
                issues);
            AddFloatMismatch(
                anchor.NormalizedLength,
                expected.NormalizedLength,
                MaximumNormalizedDimension,
                TacticTelegraphValidationIssue.AnchorLengthMismatch,
                issues);
            AddFloatMismatch(
                anchor.PaletteIntensity,
                expected.PaletteIntensity,
                MaximumPaletteIntensity,
                TacticTelegraphValidationIssue.AnchorIntensityMismatch,
                issues);
        }

        private static void ValidateSafetyAndBudget(
            TacticTelegraphRecipe recipe,
            ICollection<TacticTelegraphValidationIssue> issues)
        {
            if (!recipe.NonRaycast)
            {
                Add(issues, TacticTelegraphValidationIssue.NonRaycastRequired);
            }

            if (!recipe.ColliderFree)
            {
                Add(issues, TacticTelegraphValidationIssue.ColliderFreeRequired);
            }

            if (recipe.CameraAuthority)
            {
                Add(issues, TacticTelegraphValidationIssue.CameraAuthorityForbidden);
            }

            if (recipe.RendererBudget < 0
                || recipe.RendererBudget > MaximumRendererBudget
                || recipe.RendererBudget < recipe.Anchors.Count)
            {
                Add(issues, TacticTelegraphValidationIssue.RendererBudgetInvalid);
            }
            else if (recipe.RendererBudget != MaximumRendererBudget)
            {
                Add(issues, TacticTelegraphValidationIssue.RendererBudgetMismatch);
            }

            if (recipe.SharedMaterialBudget < 0
                || recipe.SharedMaterialBudget > ExactSharedMaterialBudget)
            {
                Add(issues, TacticTelegraphValidationIssue.SharedMaterialBudgetInvalid);
            }
            else if (recipe.SharedMaterialBudget != ExactSharedMaterialBudget)
            {
                Add(issues, TacticTelegraphValidationIssue.SharedMaterialBudgetMismatch);
            }

            if (recipe.HasPerFrameCatalogueWork)
            {
                Add(
                    issues,
                    TacticTelegraphValidationIssue.PerFrameCatalogueWorkForbidden);
            }
        }

        private static TacticTelegraphRecipe Create(AuthoredDefinition definition)
        {
            var anchors = new TacticTelegraphAnchor[definition.Anchors.Length];
            for (var index = 0; index < anchors.Length; index++)
            {
                var source = definition.Anchors[index];
                anchors[index] = new TacticTelegraphAnchor(
                    source.BindingKind,
                    source.BindingIndex,
                    source.NormalizedLocalScale,
                    source.NormalizedWidth,
                    source.NormalizedLength,
                    source.PaletteIntensity);
            }

            return new TacticTelegraphRecipe(
                definition.TacticRecipe,
                definition.TacticId,
                definition.MotifId,
                definition.PaletteId,
                definition.DurationSeconds,
                definition.DirectionBinding,
                anchors,
                true,
                true,
                false,
                MaximumRendererBudget,
                ExactSharedMaterialBudget,
                false);
        }

        private static bool HasExactAuthoredMultiset(
            IReadOnlyList<TacticTelegraphRecipe> recipes)
        {
            if (recipes == null || recipes.Count != Definitions.Length)
            {
                return false;
            }

            var seen = new bool[Definitions.Length];
            for (var index = 0; index < recipes.Count; index++)
            {
                var definitionIndex = DefinitionIndex(recipes[index]);
                if (definitionIndex < 0
                    || seen[definitionIndex]
                    || !MatchesExact(recipes[index], Definitions[definitionIndex]))
                {
                    return false;
                }

                seen[definitionIndex] = true;
            }

            return true;
        }

        private static bool MatchesExact(
            TacticTelegraphRecipe recipe,
            AuthoredDefinition definition)
        {
            if (recipe == null
                || !ReferenceEquals(recipe.TacticRecipe, definition.TacticRecipe)
                || !Same(recipe.TacticId, definition.TacticId)
                || !Same(recipe.MotifId, definition.MotifId)
                || !Same(recipe.PaletteId, definition.PaletteId)
                || recipe.DurationSeconds != definition.DurationSeconds
                || recipe.DirectionBinding != definition.DirectionBinding
                || !recipe.NonRaycast
                || !recipe.ColliderFree
                || recipe.CameraAuthority
                || recipe.RendererBudget != MaximumRendererBudget
                || recipe.SharedMaterialBudget != ExactSharedMaterialBudget
                || recipe.HasPerFrameCatalogueWork
                || recipe.Anchors.Count != definition.Anchors.Length)
            {
                return false;
            }

            for (var index = 0; index < recipe.Anchors.Count; index++)
            {
                if (!MatchesExact(recipe.Anchors[index], definition.Anchors[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchesExact(
            TacticTelegraphAnchor anchor,
            AuthoredAnchor definition)
        {
            return anchor != null
                && anchor.BindingKind == definition.BindingKind
                && anchor.BindingIndex == definition.BindingIndex
                && anchor.NormalizedLocalScale == definition.NormalizedLocalScale
                && anchor.NormalizedWidth == definition.NormalizedWidth
                && anchor.NormalizedLength == definition.NormalizedLength
                && anchor.PaletteIntensity == definition.PaletteIntensity;
        }

        private static bool HasExactAnchorMultiset(
            IReadOnlyList<TacticTelegraphAnchor> anchors,
            AuthoredDefinition definition)
        {
            if (anchors == null || anchors.Count != definition.Anchors.Length)
            {
                return false;
            }

            var seen = new bool[definition.Anchors.Length];
            for (var index = 0; index < anchors.Count; index++)
            {
                var definitionIndex = AnchorIndex(definition, anchors[index]);
                if (definitionIndex < 0 || seen[definitionIndex])
                {
                    return false;
                }

                seen[definitionIndex] = true;
            }

            return true;
        }

        private static bool HasAcceptedAnchorOrder(
            IReadOnlyList<TacticTelegraphAnchor> anchors,
            AuthoredDefinition definition)
        {
            for (var index = 0; index < anchors.Count; index++)
            {
                if (!MatchesExact(anchors[index], definition.Anchors[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static AuthoredAnchor FindAnchor(
            AuthoredDefinition definition,
            TacticTelegraphAnchorBindingKind kind,
            int bindingIndex)
        {
            for (var index = 0; index < definition.Anchors.Length; index++)
            {
                var candidate = definition.Anchors[index];
                if (candidate.BindingKind == kind
                    && candidate.BindingIndex == bindingIndex)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static int AnchorIndex(
            AuthoredDefinition definition,
            TacticTelegraphAnchor anchor)
        {
            if (anchor == null)
            {
                return -1;
            }

            for (var index = 0; index < definition.Anchors.Length; index++)
            {
                if (MatchesExact(anchor, definition.Anchors[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool HasAcceptedOrder(
            IReadOnlyList<TacticTelegraphRecipe> recipes)
        {
            for (var index = 0; index < recipes.Count; index++)
            {
                if (DefinitionIndex(recipes[index]) != index)
                {
                    return false;
                }
            }

            return true;
        }

        private static int DefinitionIndex(TacticTelegraphRecipe recipe)
        {
            if (recipe == null)
            {
                return -1;
            }

            for (var index = 0; index < Definitions.Length; index++)
            {
                if (ReferenceEquals(recipe.TacticRecipe, Definitions[index].TacticRecipe)
                    && Same(recipe.TacticId, Definitions[index].TacticId))
                {
                    return index;
                }
            }

            return -1;
        }

        private static AuthoredDefinition Definition(string tacticId)
        {
            for (var index = 0; index < Definitions.Length; index++)
            {
                if (Same(tacticId, Definitions[index].TacticId))
                {
                    return Definitions[index];
                }
            }

            return null;
        }

        private static int CachedTacticIndex(TacticRecipe recipe)
        {
            for (var index = 0; index < StarterInfernalFirstExpansionTactics.All.Count; index++)
            {
                if (ReferenceEquals(
                    recipe,
                    StarterInfernalFirstExpansionTactics.All[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        private static void ValidateStableId(
            string value,
            TacticTelegraphValidationIssue issue,
            ICollection<TacticTelegraphValidationIssue> issues)
        {
            if (!IsStableId(value))
            {
                Add(issues, issue);
            }
        }

        private static void AddFloatMismatch(
            float actual,
            float expected,
            float maximum,
            TacticTelegraphValidationIssue issue,
            ICollection<TacticTelegraphValidationIssue> issues)
        {
            if (IsFinitePositiveBounded(actual, maximum) && actual != expected)
            {
                Add(issues, issue);
            }
        }

        private static bool IsFinitePositiveBounded(float value, float maximum)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value)
                && value > 0f
                && value <= maximum;
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || !IsLowerAlphaNumeric(value[0])
                || !IsLowerAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            for (var index = 1; index < value.Length - 1; index++)
            {
                var character = value[index];
                if (!IsLowerAlphaNumeric(character)
                    && character != '.'
                    && character != '-'
                    && character != '_')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsLowerAlphaNumeric(char character)
        {
            return character >= 'a' && character <= 'z'
                || character >= '0' && character <= '9';
        }

        private static bool Same(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        private static void Add(
            ICollection<TacticTelegraphValidationIssue> issues,
            TacticTelegraphValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private static TacticTelegraphLookupResult Reject(
            TacticTelegraphLookupStatus status)
        {
            return new TacticTelegraphLookupResult(status, null);
        }

        private sealed class AuthoredDefinition
        {
            public AuthoredDefinition(
                TacticRecipe tacticRecipe,
                string tacticId,
                string motifId,
                string paletteId,
                float durationSeconds,
                TacticTelegraphDirectionBinding directionBinding,
                params AuthoredAnchor[] anchors)
            {
                TacticRecipe = tacticRecipe;
                TacticId = tacticId;
                MotifId = motifId;
                PaletteId = paletteId;
                DurationSeconds = durationSeconds;
                DirectionBinding = directionBinding;
                Anchors = anchors;
            }

            public TacticRecipe TacticRecipe { get; }
            public string TacticId { get; }
            public string MotifId { get; }
            public string PaletteId { get; }
            public float DurationSeconds { get; }
            public TacticTelegraphDirectionBinding DirectionBinding { get; }
            public AuthoredAnchor[] Anchors { get; }
        }

        private sealed class AuthoredAnchor
        {
            public AuthoredAnchor(
                TacticTelegraphAnchorBindingKind bindingKind,
                int bindingIndex,
                float normalizedLocalScale,
                float normalizedWidth,
                float normalizedLength,
                float paletteIntensity)
            {
                BindingKind = bindingKind;
                BindingIndex = bindingIndex;
                NormalizedLocalScale = normalizedLocalScale;
                NormalizedWidth = normalizedWidth;
                NormalizedLength = normalizedLength;
                PaletteIntensity = paletteIntensity;
            }

            public TacticTelegraphAnchorBindingKind BindingKind { get; }
            public int BindingIndex { get; }
            public float NormalizedLocalScale { get; }
            public float NormalizedWidth { get; }
            public float NormalizedLength { get; }
            public float PaletteIntensity { get; }
        }
    }
}
