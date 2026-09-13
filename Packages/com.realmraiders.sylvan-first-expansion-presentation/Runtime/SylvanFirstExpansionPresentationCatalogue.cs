using System;
using System.Collections.Generic;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.SylvanFirstExpansionPresentation
{
    public enum SylvanFirstExpansionPresentationLookupStatus
    {
        Found, LayoutIdInvalid, SocketIdInvalid, LayoutUnknown, SocketUnknown,
        SocketNotFirstAuthored, CatalogueInvalid
    }

    public enum SylvanFirstExpansionPresentationValidationIssue
    {
        RecipeMissing, CatalogueCardinalityInvalid, LayoutIdInvalid, SocketIdInvalid,
        LayoutIdDuplicate, LayoutUnknown, FirstSocketMismatch, MotifIdInvalid,
        PaletteKeyInvalid, LabelInvalid, AnchorCardinalityInvalid, AnchorMissing,
        AnchorIdInvalid, AnchorIdDuplicate, PropFamilyIdInvalid, PropIdInvalid,
        PropFamilyCardinalityInvalid, AnchorOutsideDecorativeRim,
        AnchorIntrudesCentralGameplayDisc, AnchorIntrudesIncomingPathCorridor,
        AnchorScaleInvalid, AnchorYawInvalid,
        ExactAuthoredFactsMismatch, AnchorOrderMismatch
    }

    /// <summary>One immutable non-colliding decorative fact; it has no Unity authority.</summary>
    public sealed class SylvanFirstExpansionDecorativeAnchor
    {
        public SylvanFirstExpansionDecorativeAnchor(string anchorId, string propFamilyId,
            string propId, float localX, float localZ, float scale, float yawDegrees)
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

    /// <summary>Immutable visual-only facts for one exact first expansion grove.</summary>
    public sealed class SylvanFirstExpansionPresentationRecipe
    {
        public SylvanFirstExpansionPresentationRecipe(string layoutId, string socketId,
            string motifId, string paletteKey, string accessibleLabel,
            IReadOnlyList<SylvanFirstExpansionDecorativeAnchor> anchors)
        {
            LayoutId = layoutId;
            SocketId = socketId;
            MotifId = motifId;
            PaletteKey = paletteKey;
            AccessibleLabel = accessibleLabel;
            Anchors = Snapshot(anchors);
        }

        public string LayoutId { get; }
        public string SocketId { get; }
        public string MotifId { get; }
        public string PaletteKey { get; }
        public string AccessibleLabel { get; }
        public IReadOnlyList<SylvanFirstExpansionDecorativeAnchor> Anchors { get; }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null) return Array.AsReadOnly(Array.Empty<T>());
            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++) copy[index] = source[index];
            return Array.AsReadOnly(copy);
        }
    }

    public sealed class SylvanFirstExpansionPresentationLookupResult
    {
        internal SylvanFirstExpansionPresentationLookupResult(
            SylvanFirstExpansionPresentationLookupStatus status,
            SylvanFirstExpansionPresentationRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public SylvanFirstExpansionPresentationLookupStatus Status { get; }
        public SylvanFirstExpansionPresentationRecipe Recipe { get; }
        public bool Found => Status == SylvanFirstExpansionPresentationLookupStatus.Found && Recipe != null;
    }

    public sealed class SylvanFirstExpansionPresentationValidationResult
    {
        internal SylvanFirstExpansionPresentationValidationResult(
            IReadOnlyList<SylvanFirstExpansionPresentationValidationIssue> issues)
        {
            var copy = issues == null ? Array.Empty<SylvanFirstExpansionPresentationValidationIssue>()
                : Copy(issues);
            Issues = Array.AsReadOnly(copy);
        }

        public IReadOnlyList<SylvanFirstExpansionPresentationValidationIssue> Issues { get; }
        public bool IsValid => Issues.Count == 0;

        private static SylvanFirstExpansionPresentationValidationIssue[] Copy(
            IReadOnlyList<SylvanFirstExpansionPresentationValidationIssue> source)
        {
            var copy = new SylvanFirstExpansionPresentationValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++) copy[index] = source[index];
            return copy;
        }
    }

    /// <summary>Explicit visual facts only; this catalogue does not create or place objects.</summary>
    public static class StarterSylvanFirstExpansionPresentation
    {
        private const float MinimumAnchorRadius = 2.35f;
        private const float MaximumAnchorRadius = 3f;
        private const float CentralGameplayRadius = 1.5f;
        private const float IncomingPathHalfWidth = 1.25f;
        private const float IncomingPathMaximumZ = 0.5f;
        private const int MaximumAnchorCount = 6;
        private const int MaximumPropFamilies = 2;

        public static SylvanFirstExpansionPresentationRecipe AncientCrossroads { get; } = Recipe(
            StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.west-bough",
            "realmraiders.sylvan-motif.wolf-den-fallen-bough", "realmraiders.palette.sylvan-moss",
            "Wolf den beneath a fallen bough", new[]
            {
                Anchor("ancient.bough.north", "realmraiders.prop-family.fallen-bough", "realmraiders.prop.fallen-bough.a", -2.5f, 1.4f, 1.1f, 25f),
                Anchor("ancient.bough.south", "realmraiders.prop-family.fallen-bough", "realmraiders.prop.fallen-bough.b", 2.6f, 1.2f, 0.9f, 205f),
                Anchor("ancient.den.stone", "realmraiders.prop-family.root-stone", "realmraiders.prop.root-stone.a", 2.2f, -1.4f, 0.75f, 110f)
            });

        public static SylvanFirstExpansionPresentationRecipe ForkedCanopy { get; } = Recipe(
            StarterSylvanRealmLayouts.ForkedCanopyId, "canopy.high-bough",
            "realmraiders.sylvan-motif.ent-watch-root-pillar", "realmraiders.palette.sylvan-canopy",
            "Ent watch among root pillars", new[]
            {
                Anchor("canopy.pillar.west", "realmraiders.prop-family.root-pillar", "realmraiders.prop.root-pillar.a", -2.6f, 1.3f, 1.25f, 15f),
                Anchor("canopy.pillar.east", "realmraiders.prop-family.root-pillar", "realmraiders.prop.root-pillar.b", 2.6f, 1.3f, 1.2f, 345f),
                Anchor("canopy.watch.stone", "realmraiders.prop-family.root-stone", "realmraiders.prop.root-stone.b", -2.1f, -1.5f, 0.8f, 140f)
            });

        public static SylvanFirstExpansionPresentationRecipe SerpentRoots { get; } = Recipe(
            StarterSylvanRealmLayouts.SerpentRootsId, "serpent.east-burrow",
            "realmraiders.sylvan-motif.burrow-curved-root-stone", "realmraiders.palette.sylvan-roots",
            "Burrow framed by curved roots and stone", new[]
            {
                Anchor("serpent.root.west", "realmraiders.prop-family.curved-root", "realmraiders.prop.curved-root.a", -2.6f, 1.2f, 1.05f, 35f),
                Anchor("serpent.root.east", "realmraiders.prop-family.curved-root", "realmraiders.prop.curved-root.b", 2.6f, 1.2f, 1.05f, 325f),
                Anchor("serpent.burrow.stone", "realmraiders.prop-family.root-stone", "realmraiders.prop.root-stone.c", 2.2f, -1.4f, 0.85f, 150f)
            });

        public static IReadOnlyList<SylvanFirstExpansionPresentationRecipe> All { get; } =
            Array.AsReadOnly(new[] { AncientCrossroads, ForkedCanopy, SerpentRoots });

        public static SylvanFirstExpansionPresentationLookupResult FindExact(string layoutId, string socketId)
        {
            if (!HasStableId(layoutId)) return Reject(SylvanFirstExpansionPresentationLookupStatus.LayoutIdInvalid);
            if (!HasStableId(socketId)) return Reject(SylvanFirstExpansionPresentationLookupStatus.SocketIdInvalid);
            if (!Validate(All).IsValid) return Reject(SylvanFirstExpansionPresentationLookupStatus.CatalogueInvalid);
            var layout = StarterSylvanRealmLayoutResolver.ResolveExact(layoutId);
            if (!layout.HasRecipe) return Reject(SylvanFirstExpansionPresentationLookupStatus.LayoutUnknown);
            var first = layout.Recipe.ExpansionSockets.Count == 0 ? null : layout.Recipe.ExpansionSockets[0];
            if (first == null) return Reject(SylvanFirstExpansionPresentationLookupStatus.CatalogueInvalid);
            if (!string.Equals(first.SocketId, socketId, StringComparison.Ordinal))
                return Reject(ContainsSocket(layout.Recipe, socketId)
                    ? SylvanFirstExpansionPresentationLookupStatus.SocketNotFirstAuthored
                    : SylvanFirstExpansionPresentationLookupStatus.SocketUnknown);
            foreach (var recipe in All)
                if (string.Equals(recipe.LayoutId, layoutId, StringComparison.Ordinal)
                    && string.Equals(recipe.SocketId, socketId, StringComparison.Ordinal))
                    return new SylvanFirstExpansionPresentationLookupResult(
                        SylvanFirstExpansionPresentationLookupStatus.Found, recipe);
            return Reject(SylvanFirstExpansionPresentationLookupStatus.CatalogueInvalid);
        }

        public static SylvanFirstExpansionPresentationValidationResult Validate(
            IReadOnlyList<SylvanFirstExpansionPresentationRecipe> recipes)
        {
            var issues = new List<SylvanFirstExpansionPresentationValidationIssue>();
            if (recipes == null || recipes.Count != 3) Add(issues, SylvanFirstExpansionPresentationValidationIssue.CatalogueCardinalityInvalid);
            var layouts = new HashSet<string>(StringComparer.Ordinal);
            if (recipes != null) foreach (var recipe in recipes) ValidateRecipe(recipe, layouts, issues);
            return new SylvanFirstExpansionPresentationValidationResult(issues);
        }

        private static void ValidateRecipe(SylvanFirstExpansionPresentationRecipe recipe, ISet<string> layouts,
            ICollection<SylvanFirstExpansionPresentationValidationIssue> issues)
        {
            if (recipe == null) { Add(issues, SylvanFirstExpansionPresentationValidationIssue.RecipeMissing); return; }
            if (!HasStableId(recipe.LayoutId)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.LayoutIdInvalid);
            else if (!layouts.Add(recipe.LayoutId)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.LayoutIdDuplicate);
            if (!HasStableId(recipe.SocketId)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.SocketIdInvalid);
            var layout = StarterSylvanRealmLayoutResolver.ResolveExact(recipe.LayoutId);
            if (!layout.HasRecipe) Add(issues, SylvanFirstExpansionPresentationValidationIssue.LayoutUnknown);
            else if (layout.Recipe.ExpansionSockets.Count == 0 || layout.Recipe.ExpansionSockets[0] == null
                || !string.Equals(layout.Recipe.ExpansionSockets[0].SocketId, recipe.SocketId, StringComparison.Ordinal))
                Add(issues, SylvanFirstExpansionPresentationValidationIssue.FirstSocketMismatch);
            if (!HasStableId(recipe.MotifId)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.MotifIdInvalid);
            if (!HasStableId(recipe.PaletteKey)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.PaletteKeyInvalid);
            if (string.IsNullOrWhiteSpace(recipe.AccessibleLabel)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.LabelInvalid);
            ValidateAnchors(recipe.Anchors, issues);
            ValidateExactFacts(recipe, issues);
        }

        private static void ValidateAnchors(IReadOnlyList<SylvanFirstExpansionDecorativeAnchor> anchors,
            ICollection<SylvanFirstExpansionPresentationValidationIssue> issues)
        {
            if (anchors == null || anchors.Count == 0 || anchors.Count > MaximumAnchorCount)
                Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorCardinalityInvalid);
            var anchorIds = new HashSet<string>(StringComparer.Ordinal);
            var families = new HashSet<string>(StringComparer.Ordinal);
            if (anchors == null) return;
            foreach (var anchor in anchors)
            {
                if (anchor == null) { Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorMissing); continue; }
                if (!HasStableId(anchor.AnchorId)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorIdInvalid);
                else if (!anchorIds.Add(anchor.AnchorId)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorIdDuplicate);
                if (!HasStableId(anchor.PropFamilyId)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.PropFamilyIdInvalid);
                else families.Add(anchor.PropFamilyId);
                if (!HasStableId(anchor.PropId)) Add(issues, SylvanFirstExpansionPresentationValidationIssue.PropIdInvalid);
                var radiusSquared = anchor.LocalX * anchor.LocalX + anchor.LocalZ * anchor.LocalZ;
                if (!IsFinite(anchor.LocalX) || !IsFinite(anchor.LocalZ) || radiusSquared < MinimumAnchorRadius * MinimumAnchorRadius || radiusSquared > MaximumAnchorRadius * MaximumAnchorRadius)
                    Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorOutsideDecorativeRim);
                if (radiusSquared < CentralGameplayRadius * CentralGameplayRadius)
                    Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorIntrudesCentralGameplayDisc);
                if (Math.Abs(anchor.LocalX) < IncomingPathHalfWidth && anchor.LocalZ <= IncomingPathMaximumZ)
                    Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorIntrudesIncomingPathCorridor);
                if (!IsFinite(anchor.Scale) || anchor.Scale < 0.5f || anchor.Scale > 1.5f)
                    Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorScaleInvalid);
                if (!IsFinite(anchor.YawDegrees) || anchor.YawDegrees < 0f || anchor.YawDegrees >= 360f)
                    Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorYawInvalid);
            }
            if (families.Count > MaximumPropFamilies) Add(issues, SylvanFirstExpansionPresentationValidationIssue.PropFamilyCardinalityInvalid);
        }

        private static void ValidateExactFacts(SylvanFirstExpansionPresentationRecipe recipe,
            ICollection<SylvanFirstExpansionPresentationValidationIssue> issues)
        {
            var expected = Expected(recipe.LayoutId, recipe.SocketId);
            if (expected == null) return;
            if (!string.Equals(recipe.MotifId, expected.MotifId, StringComparison.Ordinal)
                || !string.Equals(recipe.PaletteKey, expected.PaletteKey, StringComparison.Ordinal)
                || !string.Equals(recipe.AccessibleLabel, expected.AccessibleLabel, StringComparison.Ordinal)
                || !AnchorsMatch(recipe.Anchors, expected.Anchors, false))
                Add(issues, SylvanFirstExpansionPresentationValidationIssue.ExactAuthoredFactsMismatch);
            if (!AnchorsMatch(recipe.Anchors, expected.Anchors, true))
                Add(issues, SylvanFirstExpansionPresentationValidationIssue.AnchorOrderMismatch);
        }

        private static bool AnchorsMatch(IReadOnlyList<SylvanFirstExpansionDecorativeAnchor> actual,
            IReadOnlyList<SylvanFirstExpansionDecorativeAnchor> expected, bool orderOnly)
        {
            if (actual == null || expected == null || actual.Count != expected.Count) return false;
            for (var index = 0; index < actual.Count; index++)
            {
                var first = actual[index]; var second = expected[index];
                if (first == null || second == null || !string.Equals(first.AnchorId, second.AnchorId, StringComparison.Ordinal)) return false;
                if (!orderOnly && (!string.Equals(first.PropFamilyId, second.PropFamilyId, StringComparison.Ordinal)
                    || !string.Equals(first.PropId, second.PropId, StringComparison.Ordinal)
                    || first.LocalX != second.LocalX || first.LocalZ != second.LocalZ
                    || first.Scale != second.Scale || first.YawDegrees != second.YawDegrees)) return false;
            }
            return true;
        }

        private static SylvanFirstExpansionPresentationRecipe Expected(string layoutId, string socketId)
        {
            if (Matches(AncientCrossroads, layoutId, socketId)) return AncientCrossroads;
            if (Matches(ForkedCanopy, layoutId, socketId)) return ForkedCanopy;
            if (Matches(SerpentRoots, layoutId, socketId)) return SerpentRoots;
            return null;
        }

        private static bool Matches(SylvanFirstExpansionPresentationRecipe recipe, string layoutId, string socketId)
        { return string.Equals(recipe.LayoutId, layoutId, StringComparison.Ordinal) && string.Equals(recipe.SocketId, socketId, StringComparison.Ordinal); }
        private static bool ContainsSocket(RealmLayoutRecipe recipe, string socketId)
        { foreach (var socket in recipe.ExpansionSockets) if (socket != null && string.Equals(socket.SocketId, socketId, StringComparison.Ordinal)) return true; return false; }
        private static SylvanFirstExpansionPresentationLookupResult Reject(SylvanFirstExpansionPresentationLookupStatus status)
        { return new SylvanFirstExpansionPresentationLookupResult(status, null); }
        private static SylvanFirstExpansionPresentationRecipe Recipe(string layoutId, string socketId, string motifId, string paletteKey, string label, IReadOnlyList<SylvanFirstExpansionDecorativeAnchor> anchors)
        { return new SylvanFirstExpansionPresentationRecipe(layoutId, socketId, motifId, paletteKey, label, anchors); }
        private static SylvanFirstExpansionDecorativeAnchor Anchor(string id, string family, string prop, float x, float z, float scale, float yaw)
        { return new SylvanFirstExpansionDecorativeAnchor(id, family, prop, x, z, scale, yaw); }
        private static bool IsFinite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || !AlphaNumeric(value[0]) || !AlphaNumeric(value[value.Length - 1])) return false;
            for (var index = 1; index < value.Length - 1; index++) if (!AlphaNumeric(value[index]) && value[index] != '.' && value[index] != '_' && value[index] != '-') return false;
            return true;
        }
        private static bool AlphaNumeric(char value) { return value >= 'a' && value <= 'z' || value >= '0' && value <= '9'; }
        private static void Add(ICollection<SylvanFirstExpansionPresentationValidationIssue> issues, SylvanFirstExpansionPresentationValidationIssue issue)
        { if (!issues.Contains(issue)) issues.Add(issue); }
    }
}
