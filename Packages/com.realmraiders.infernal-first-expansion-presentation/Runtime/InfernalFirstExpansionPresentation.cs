using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RealmRaiders.Modules.InfernalFirstExpansionContent;

[assembly: InternalsVisibleTo("RealmRaiders.InfernalFirstExpansionPresentation.EditorTests")]

namespace RealmRaiders.Modules.InfernalFirstExpansionPresentation
{
    public enum InfernalFirstExpansionPresentationLookupStatus
    {
        Found,
        LayoutIdInvalid,
        SocketIdInvalid,
        LayoutUnknown,
        SocketUnknown,
        SocketNotFirstAuthored,
        CatalogueInvalid
    }

    public enum InfernalFirstExpansionPresentationValidationIssue
    {
        RecipeMissing,
        CatalogueCardinalityInvalid,
        CatalogueOrderMismatch,
        LayoutIdInvalid,
        SocketIdInvalid,
        SourceNodeIdInvalid,
        SiteIdInvalid,
        LayoutIdDuplicate,
        SocketIdDuplicate,
        SiteIdDuplicate,
        LayoutUnknown,
        SocketUnknown,
        SocketNotFirstAuthored,
        ContentCatalogueInvalid,
        SourceNodeMismatch,
        SiteIdMismatch,
        MotifIdInvalid,
        PaletteKeyInvalid,
        AnchorCardinalityInvalid,
        RendererBudgetExceeded,
        AnchorMissing,
        AnchorIdInvalid,
        AnchorIdDuplicate,
        PropFamilyIdInvalid,
        PropIdInvalid,
        PropFamilyCardinalityInvalid,
        AnchorOffsetNotFinite,
        AnchorOutsideDecorativeRim,
        AnchorIntrudesCentralGameplayDisc,
        AnchorIntrudesIncomingPathCorridor,
        AnchorScaleInvalid,
        AnchorYawInvalid,
        ExactAuthoredFactsMismatch,
        AnchorOrderMismatch
    }

    /// <summary>An immutable, visual-only anchor. It grants no collider or gameplay authority.</summary>
    public sealed class InfernalFirstExpansionDecorativeAnchor
    {
        public InfernalFirstExpansionDecorativeAnchor(
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

    /// <summary>Immutable decorative facts bound to one exact MGC31 expansion site.</summary>
    public sealed class InfernalFirstExpansionPresentationRecipe
    {
        public InfernalFirstExpansionPresentationRecipe(
            string layoutId,
            string socketId,
            string sourceNodeId,
            string siteId,
            string motifId,
            string paletteKey,
            IReadOnlyList<InfernalFirstExpansionDecorativeAnchor> anchors)
        {
            LayoutId = layoutId;
            SocketId = socketId;
            SourceNodeId = sourceNodeId;
            SiteId = siteId;
            MotifId = motifId;
            PaletteKey = paletteKey;
            Anchors = Snapshot(anchors);
        }

        public string LayoutId { get; }
        public string SocketId { get; }
        public string SourceNodeId { get; }
        public string SiteId { get; }
        public string MotifId { get; }
        public string PaletteKey { get; }
        public IReadOnlyList<InfernalFirstExpansionDecorativeAnchor> Anchors { get; }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null) return Array.AsReadOnly(Array.Empty<T>());
            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++) copy[index] = source[index];
            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalFirstExpansionPresentationLookupResult
    {
        internal InfernalFirstExpansionPresentationLookupResult(
            InfernalFirstExpansionPresentationLookupStatus status,
            InfernalFirstExpansionPresentationRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public InfernalFirstExpansionPresentationLookupStatus Status { get; }
        public InfernalFirstExpansionPresentationRecipe Recipe { get; }
        public bool Found => Status == InfernalFirstExpansionPresentationLookupStatus.Found
            && Recipe != null;
    }

    public sealed class InfernalFirstExpansionPresentationValidationResult
    {
        internal InfernalFirstExpansionPresentationValidationResult(
            IReadOnlyList<InfernalFirstExpansionPresentationValidationIssue> issues)
        {
            var copy = issues == null
                ? Array.Empty<InfernalFirstExpansionPresentationValidationIssue>()
                : Copy(issues);
            Issues = Array.AsReadOnly(copy);
        }

        public IReadOnlyList<InfernalFirstExpansionPresentationValidationIssue> Issues { get; }
        public bool IsValid => Issues.Count == 0;

        private static InfernalFirstExpansionPresentationValidationIssue[] Copy(
            IReadOnlyList<InfernalFirstExpansionPresentationValidationIssue> source)
        {
            var copy = new InfernalFirstExpansionPresentationValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++) copy[index] = source[index];
            return copy;
        }
    }

    /// <summary>
    /// Exact no-engine presentation lookup. It validates MGC31 content evidence but never creates renderers.
    /// </summary>
    public static class StarterInfernalFirstExpansionPresentation
    {
        private const float MinimumAnchorRadius = 2.35f;
        private const float MaximumAnchorRadius = 3f;
        private const float CentralGameplayRadius = 1.5f;
        private const float IncomingPathHalfWidth = 1.25f;
        private const float IncomingPathMaximumZ = 0.5f;
        private const float MinimumScale = 0.5f;
        private const float MaximumScale = 1.5f;
        private const int MaximumRendererCount = 3;
        private const int RequiredPropFamilyCount = 1;

        public static InfernalFirstExpansionPresentationRecipe AshenPackVent { get; } = Recipe(
            "realmraiders.infernal-defense.ashen-spur",
            "ashen.west-vent",
            "ashen.hellhound",
            "realmraiders.infernal-expansion.ashen-pack-vent",
            "ashen-pack-vent-basalt-rim",
            "ashen+basalt",
            new[]
            {
                Anchor("ashen.left", "realmraiders.primitive.basalt-rim", "realmraiders.primitive.basalt-rim.low-block", -2.55f, 1.35f, 0.85f, 25f),
                Anchor("ashen.right", "realmraiders.primitive.basalt-rim", "realmraiders.primitive.basalt-rim.low-block", 2.6f, 1.2f, 0.8f, 205f)
            });

        public static InfernalFirstExpansionPresentationRecipe CinderSnareVent { get; } = Recipe(
            "realmraiders.infernal-defense.cinder-fork",
            "cinder.north-vent",
            "cinder.hellhound",
            "realmraiders.infernal-expansion.cinder-snare-vent",
            "cinder-snare-vent-open-flanks",
            "cinder+basalt",
            new[]
            {
                Anchor("cinder.left", "realmraiders.primitive.basalt-rim", "realmraiders.primitive.basalt-rim.low-cinder-marked-block", -2.6f, 1.25f, 0.75f, 20f),
                Anchor("cinder.right", "realmraiders.primitive.basalt-rim", "realmraiders.primitive.basalt-rim.low-cinder-marked-block", 2.6f, 1.25f, 0.75f, 340f)
            });

        public static InfernalFirstExpansionPresentationRecipe BruteKilnVent { get; } = Recipe(
            "realmraiders.infernal-defense.ember-circuit",
            "ember.west-vent",
            "ember.hellhound",
            "realmraiders.infernal-expansion.brute-kiln-vent",
            "brute-kiln-vent-obsidian-rim",
            "ember+obsidian",
            new[]
            {
                Anchor("ember.left", "realmraiders.primitive.obsidian-rim", "realmraiders.primitive.obsidian-rim.low-block", -2.55f, 1.3f, 0.95f, 35f),
                Anchor("ember.right", "realmraiders.primitive.obsidian-rim", "realmraiders.primitive.obsidian-rim.low-block", 2.55f, 1.3f, 0.95f, 325f),
                Anchor("ember.back", "realmraiders.primitive.obsidian-rim", "realmraiders.primitive.obsidian-rim.low-block", 2.15f, -1.4f, 0.8f, 145f)
            });

        public static IReadOnlyList<InfernalFirstExpansionPresentationRecipe> All { get; } =
            Array.AsReadOnly(new[] { AshenPackVent, CinderSnareVent, BruteKilnVent });

        public static InfernalFirstExpansionPresentationLookupResult FindExact(
            string layoutId,
            string socketId)
        {
            if (!HasStableId(layoutId))
                return Reject(InfernalFirstExpansionPresentationLookupStatus.LayoutIdInvalid);
            if (!HasStableId(socketId))
                return Reject(InfernalFirstExpansionPresentationLookupStatus.SocketIdInvalid);
            if (!Validate(All).IsValid)
                return Reject(InfernalFirstExpansionPresentationLookupStatus.CatalogueInvalid);

            var content = StarterInfernalFirstExpansionContent.FindExact(layoutId, socketId);
            if (!content.Found) return Reject(MapContentLookupStatus(content.Status));

            foreach (var recipe in All)
            {
                if (Matches(recipe, layoutId, socketId)
                    && string.Equals(recipe.SourceNodeId, content.Recipe.SourceNodeId, StringComparison.Ordinal)
                    && string.Equals(recipe.SiteId, content.Recipe.SiteId, StringComparison.Ordinal))
                    return new InfernalFirstExpansionPresentationLookupResult(
                        InfernalFirstExpansionPresentationLookupStatus.Found, recipe);
            }

            return Reject(InfernalFirstExpansionPresentationLookupStatus.CatalogueInvalid);
        }

        public static InfernalFirstExpansionPresentationValidationResult Validate(
            IReadOnlyList<InfernalFirstExpansionPresentationRecipe> recipes)
        {
            var issues = new List<InfernalFirstExpansionPresentationValidationIssue>();
            if (recipes == null || recipes.Count != All.Count)
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.CatalogueCardinalityInvalid);

            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            var socketIds = new HashSet<string>(StringComparer.Ordinal);
            var siteIds = new HashSet<string>(StringComparer.Ordinal);
            if (recipes != null)
            {
                foreach (var recipe in recipes)
                    ValidateRecipe(recipe, layoutIds, socketIds, siteIds, issues);

                if (recipes.Count == All.Count)
                {
                    for (var index = 0; index < All.Count; index++)
                    {
                        if (recipes[index] == null
                            || !Matches(recipes[index], All[index].LayoutId, All[index].SocketId))
                        {
                            Add(issues, InfernalFirstExpansionPresentationValidationIssue.CatalogueOrderMismatch);
                            break;
                        }
                    }
                }
            }

            return new InfernalFirstExpansionPresentationValidationResult(issues);
        }

        private static void ValidateRecipe(
            InfernalFirstExpansionPresentationRecipe recipe,
            ISet<string> layoutIds,
            ISet<string> socketIds,
            ISet<string> siteIds,
            ICollection<InfernalFirstExpansionPresentationValidationIssue> issues)
        {
            if (recipe == null)
            {
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.RecipeMissing);
                return;
            }

            ValidateStableUniqueId(recipe.LayoutId, layoutIds,
                InfernalFirstExpansionPresentationValidationIssue.LayoutIdInvalid,
                InfernalFirstExpansionPresentationValidationIssue.LayoutIdDuplicate, issues);
            ValidateStableUniqueId(recipe.SocketId, socketIds,
                InfernalFirstExpansionPresentationValidationIssue.SocketIdInvalid,
                InfernalFirstExpansionPresentationValidationIssue.SocketIdDuplicate, issues);
            if (!HasStableId(recipe.SourceNodeId))
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.SourceNodeIdInvalid);
            ValidateStableUniqueId(recipe.SiteId, siteIds,
                InfernalFirstExpansionPresentationValidationIssue.SiteIdInvalid,
                InfernalFirstExpansionPresentationValidationIssue.SiteIdDuplicate, issues);

            BindContent(recipe, issues);

            if (!HasStableId(recipe.MotifId))
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.MotifIdInvalid);
            if (!HasStablePaletteKey(recipe.PaletteKey))
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.PaletteKeyInvalid);

            ValidateAnchors(recipe.Anchors, issues);
            ValidateExactFacts(recipe, issues);
        }

        private static void BindContent(
            InfernalFirstExpansionPresentationRecipe recipe,
            ICollection<InfernalFirstExpansionPresentationValidationIssue> issues)
        {
            var content = StarterInfernalFirstExpansionContent.FindExact(recipe.LayoutId, recipe.SocketId);
            switch (content.Status)
            {
                case InfernalFirstExpansionContentLookupStatus.Found:
                    if (!string.Equals(recipe.SourceNodeId, content.Recipe.SourceNodeId, StringComparison.Ordinal))
                        Add(issues, InfernalFirstExpansionPresentationValidationIssue.SourceNodeMismatch);
                    if (!string.Equals(recipe.SiteId, content.Recipe.SiteId, StringComparison.Ordinal))
                        Add(issues, InfernalFirstExpansionPresentationValidationIssue.SiteIdMismatch);
                    break;
                default:
                    var issue = MapContentValidationIssue(content.Status);
                    if (issue.HasValue) Add(issues, issue.Value);
                    break;
            }
        }

        private static void ValidateAnchors(
            IReadOnlyList<InfernalFirstExpansionDecorativeAnchor> anchors,
            ICollection<InfernalFirstExpansionPresentationValidationIssue> issues)
        {
            if (anchors == null || anchors.Count == 0)
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorCardinalityInvalid);
            if (anchors != null && anchors.Count > MaximumRendererCount)
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.RendererBudgetExceeded);

            var anchorIds = new HashSet<string>(StringComparer.Ordinal);
            var familyIds = new HashSet<string>(StringComparer.Ordinal);
            if (anchors == null) return;

            foreach (var anchor in anchors)
            {
                if (anchor == null)
                {
                    Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorMissing);
                    continue;
                }

                if (!HasStableId(anchor.AnchorId))
                    Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorIdInvalid);
                else if (!anchorIds.Add(anchor.AnchorId))
                    Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorIdDuplicate);

                if (!HasStableId(anchor.PropFamilyId))
                    Add(issues, InfernalFirstExpansionPresentationValidationIssue.PropFamilyIdInvalid);
                else
                    familyIds.Add(anchor.PropFamilyId);
                if (!HasStableId(anchor.PropId))
                    Add(issues, InfernalFirstExpansionPresentationValidationIssue.PropIdInvalid);

                ValidateAnchorGeometry(anchor, issues);
            }

            if (familyIds.Count != RequiredPropFamilyCount)
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.PropFamilyCardinalityInvalid);
        }

        private static void ValidateAnchorGeometry(
            InfernalFirstExpansionDecorativeAnchor anchor,
            ICollection<InfernalFirstExpansionPresentationValidationIssue> issues)
        {
            if (!IsFinite(anchor.LocalX) || !IsFinite(anchor.LocalZ))
            {
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorOffsetNotFinite);
            }
            else
            {
                var radiusSquared = anchor.LocalX * anchor.LocalX + anchor.LocalZ * anchor.LocalZ;
                if (radiusSquared < MinimumAnchorRadius * MinimumAnchorRadius
                    || radiusSquared > MaximumAnchorRadius * MaximumAnchorRadius)
                    Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorOutsideDecorativeRim);
                if (radiusSquared < CentralGameplayRadius * CentralGameplayRadius)
                    Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorIntrudesCentralGameplayDisc);
                if (Math.Abs(anchor.LocalX) < IncomingPathHalfWidth
                    && anchor.LocalZ <= IncomingPathMaximumZ)
                    Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorIntrudesIncomingPathCorridor);
            }

            if (!IsFinite(anchor.Scale) || anchor.Scale < MinimumScale || anchor.Scale > MaximumScale)
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorScaleInvalid);
            if (!IsFinite(anchor.YawDegrees) || anchor.YawDegrees < 0f || anchor.YawDegrees >= 360f)
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorYawInvalid);
        }

        private static void ValidateExactFacts(
            InfernalFirstExpansionPresentationRecipe recipe,
            ICollection<InfernalFirstExpansionPresentationValidationIssue> issues)
        {
            var expected = Expected(recipe.LayoutId, recipe.SocketId);
            if (expected == null) return;

            if (!string.Equals(recipe.SourceNodeId, expected.SourceNodeId, StringComparison.Ordinal)
                || !string.Equals(recipe.SiteId, expected.SiteId, StringComparison.Ordinal)
                || !string.Equals(recipe.MotifId, expected.MotifId, StringComparison.Ordinal)
                || !string.Equals(recipe.PaletteKey, expected.PaletteKey, StringComparison.Ordinal)
                || !AnchorsMatch(recipe.Anchors, expected.Anchors))
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.ExactAuthoredFactsMismatch);

            if (!AnchorsMatch(recipe.Anchors, expected.Anchors)
                && SameAnchorSet(recipe.Anchors, expected.Anchors))
                Add(issues, InfernalFirstExpansionPresentationValidationIssue.AnchorOrderMismatch);
        }

        private static bool AnchorsMatch(
            IReadOnlyList<InfernalFirstExpansionDecorativeAnchor> actual,
            IReadOnlyList<InfernalFirstExpansionDecorativeAnchor> expected)
        {
            if (actual == null || expected == null || actual.Count != expected.Count) return false;
            for (var index = 0; index < actual.Count; index++)
                if (!AnchorMatches(actual[index], expected[index])) return false;
            return true;
        }

        private static bool SameAnchorSet(
            IReadOnlyList<InfernalFirstExpansionDecorativeAnchor> actual,
            IReadOnlyList<InfernalFirstExpansionDecorativeAnchor> expected)
        {
            if (actual == null || expected == null || actual.Count != expected.Count) return false;
            var matched = new bool[expected.Count];
            foreach (var anchor in actual)
            {
                var found = -1;
                for (var index = 0; index < expected.Count; index++)
                {
                    if (!matched[index] && AnchorMatches(anchor, expected[index]))
                    {
                        found = index;
                        break;
                    }
                }

                if (found < 0) return false;
                matched[found] = true;
            }

            return true;
        }

        private static bool AnchorMatches(
            InfernalFirstExpansionDecorativeAnchor first,
            InfernalFirstExpansionDecorativeAnchor second)
        {
            return first != null && second != null
                && string.Equals(first.AnchorId, second.AnchorId, StringComparison.Ordinal)
                && string.Equals(first.PropFamilyId, second.PropFamilyId, StringComparison.Ordinal)
                && string.Equals(first.PropId, second.PropId, StringComparison.Ordinal)
                && first.LocalX == second.LocalX
                && first.LocalZ == second.LocalZ
                && first.Scale == second.Scale
                && first.YawDegrees == second.YawDegrees;
        }

        private static InfernalFirstExpansionPresentationRecipe Expected(string layoutId, string socketId)
        {
            foreach (var recipe in All)
                if (Matches(recipe, layoutId, socketId)) return recipe;
            return null;
        }

        internal static InfernalFirstExpansionPresentationLookupStatus MapContentLookupStatus(
            InfernalFirstExpansionContentLookupStatus status)
        {
            switch (status)
            {
                case InfernalFirstExpansionContentLookupStatus.LayoutIdInvalid:
                    return InfernalFirstExpansionPresentationLookupStatus.LayoutIdInvalid;
                case InfernalFirstExpansionContentLookupStatus.SocketIdInvalid:
                    return InfernalFirstExpansionPresentationLookupStatus.SocketIdInvalid;
                case InfernalFirstExpansionContentLookupStatus.LayoutUnknown:
                    return InfernalFirstExpansionPresentationLookupStatus.LayoutUnknown;
                case InfernalFirstExpansionContentLookupStatus.SocketUnknown:
                    return InfernalFirstExpansionPresentationLookupStatus.SocketUnknown;
                case InfernalFirstExpansionContentLookupStatus.SocketNotFirstAuthored:
                    return InfernalFirstExpansionPresentationLookupStatus.SocketNotFirstAuthored;
                default:
                    return InfernalFirstExpansionPresentationLookupStatus.CatalogueInvalid;
            }
        }

        internal static InfernalFirstExpansionPresentationValidationIssue? MapContentValidationIssue(
            InfernalFirstExpansionContentLookupStatus status)
        {
            switch (status)
            {
                case InfernalFirstExpansionContentLookupStatus.LayoutUnknown:
                    return InfernalFirstExpansionPresentationValidationIssue.LayoutUnknown;
                case InfernalFirstExpansionContentLookupStatus.SocketUnknown:
                    return InfernalFirstExpansionPresentationValidationIssue.SocketUnknown;
                case InfernalFirstExpansionContentLookupStatus.SocketNotFirstAuthored:
                    return InfernalFirstExpansionPresentationValidationIssue.SocketNotFirstAuthored;
                case InfernalFirstExpansionContentLookupStatus.CatalogueInvalid:
                    return InfernalFirstExpansionPresentationValidationIssue.ContentCatalogueInvalid;
                default:
                    return null;
            }
        }

        private static void ValidateStableUniqueId(
            string value,
            ISet<string> values,
            InfernalFirstExpansionPresentationValidationIssue invalidIssue,
            InfernalFirstExpansionPresentationValidationIssue duplicateIssue,
            ICollection<InfernalFirstExpansionPresentationValidationIssue> issues)
        {
            if (!HasStableId(value)) Add(issues, invalidIssue);
            else if (!values.Add(value)) Add(issues, duplicateIssue);
        }

        private static bool Matches(
            InfernalFirstExpansionPresentationRecipe recipe,
            string layoutId,
            string socketId)
        {
            return string.Equals(recipe.LayoutId, layoutId, StringComparison.Ordinal)
                && string.Equals(recipe.SocketId, socketId, StringComparison.Ordinal);
        }

        private static InfernalFirstExpansionPresentationRecipe Recipe(
            string layoutId,
            string socketId,
            string sourceNodeId,
            string siteId,
            string motifId,
            string paletteKey,
            IReadOnlyList<InfernalFirstExpansionDecorativeAnchor> anchors)
        {
            return new InfernalFirstExpansionPresentationRecipe(
                layoutId, socketId, sourceNodeId, siteId, motifId, paletteKey, anchors);
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

        private static bool HasStablePaletteKey(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var separator = value.IndexOf('+');
            return separator > 0
                && separator < value.Length - 1
                && value.IndexOf('+', separator + 1) < 0
                && HasStableId(value.Substring(0, separator))
                && HasStableId(value.Substring(separator + 1));
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || !AlphaNumeric(value[0])
                || !AlphaNumeric(value[value.Length - 1])) return false;
            for (var index = 1; index < value.Length - 1; index++)
            {
                var character = value[index];
                if (!AlphaNumeric(character) && character != '.' && character != '_'
                    && character != '-') return false;
            }
            return true;
        }

        private static bool AlphaNumeric(char value)
        {
            return value >= 'a' && value <= 'z' || value >= '0' && value <= '9';
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void Add(
            ICollection<InfernalFirstExpansionPresentationValidationIssue> issues,
            InfernalFirstExpansionPresentationValidationIssue issue)
        {
            if (!issues.Contains(issue)) issues.Add(issue);
        }

        private static InfernalFirstExpansionPresentationLookupResult Reject(
            InfernalFirstExpansionPresentationLookupStatus status)
        {
            return new InfernalFirstExpansionPresentationLookupResult(status, null);
        }
    }
}
