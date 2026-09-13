using System;
using System.Collections.Generic;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.SylvanFirstExpansionEncounters
{
    public enum SylvanFirstExpansionEncounterLookupStatus
    {
        Found,
        LayoutIdInvalid,
        SocketIdInvalid,
        LayoutUnknown,
        SocketUnknown,
        SocketNotFirstAuthored,
        CatalogueInvalid
    }

    public enum SylvanFirstExpansionEncounterValidationIssue
    {
        RecipeMissing,
        LayoutIdInvalid,
        SocketIdInvalid,
        LayoutIdDuplicate,
        EncounterArchetypeIdInvalid,
        EncounterArchetypeMismatch,
        EncounterCountInvalid,
        EncounterCountMismatch,
        ExpansionNodeRecoveryContentIdInvalid,
        CueInvalid,
        CueMismatch,
        LayoutUnknown,
        FirstSocketMismatch,
        CatalogueCardinalityInvalid
    }

    /// <summary>
    /// Immutable descriptive facts only. Core owns all encounter activation,
    /// spawning, recovery, room rewards and gameplay authority.
    /// </summary>
    public sealed class SylvanFirstExpansionEncounterRecipe
    {
        public SylvanFirstExpansionEncounterRecipe(
            string layoutId,
            string socketId,
            string encounterArchetypeId,
            int encounterCount,
            string expansionNodeRecoveryContentId,
            string cue)
        {
            LayoutId = layoutId;
            SocketId = socketId;
            EncounterArchetypeId = encounterArchetypeId;
            EncounterCount = encounterCount;
            ExpansionNodeRecoveryContentId = expansionNodeRecoveryContentId;
            Cue = cue;
        }

        public string LayoutId { get; }
        public string SocketId { get; }
        public string EncounterArchetypeId { get; }
        public int EncounterCount { get; }
        /// <summary>Recovery content Core may materialize inside the new expansion node only.</summary>
        public string ExpansionNodeRecoveryContentId { get; }
        public string Cue { get; }
    }

    public sealed class SylvanFirstExpansionEncounterLookupResult
    {
        internal SylvanFirstExpansionEncounterLookupResult(
            SylvanFirstExpansionEncounterLookupStatus status,
            SylvanFirstExpansionEncounterRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public SylvanFirstExpansionEncounterLookupStatus Status { get; }
        public SylvanFirstExpansionEncounterRecipe Recipe { get; }
        public bool Found => Status == SylvanFirstExpansionEncounterLookupStatus.Found && Recipe != null;
    }

    public sealed class SylvanFirstExpansionEncounterValidationResult
    {
        internal SylvanFirstExpansionEncounterValidationResult(
            IReadOnlyList<SylvanFirstExpansionEncounterValidationIssue> issues)
        {
            var copy = issues == null
                ? Array.Empty<SylvanFirstExpansionEncounterValidationIssue>()
                : Copy(issues);
            Issues = Array.AsReadOnly(copy);
        }

        public IReadOnlyList<SylvanFirstExpansionEncounterValidationIssue> Issues { get; }
        public bool IsValid => Issues.Count == 0;

        private static SylvanFirstExpansionEncounterValidationIssue[] Copy(
            IReadOnlyList<SylvanFirstExpansionEncounterValidationIssue> source)
        {
            var copy = new SylvanFirstExpansionEncounterValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++) copy[index] = source[index];
            return copy;
        }
    }

    /// <summary>Explicit three-layout catalogue; it neither selects nor discovers content.</summary>
    public static class StarterSylvanFirstExpansionEncounters
    {
        public const string SylvanWolfArchetypeId = "realmraiders.sylvan-wolf";
        public const string GuardianEntArchetypeId = "realmraiders.guardian-ent";
        public const string MoonwellExpansionNodeRecoveryContentId = "realmraiders.node.moonwell";

        public static SylvanFirstExpansionEncounterRecipe AncientCrossroads { get; } =
            new SylvanFirstExpansionEncounterRecipe(
                StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.west-bough",
                SylvanWolfArchetypeId, 2, MoonwellExpansionNodeRecoveryContentId,
                "WOLVES HOLD THE WEST BOUGH. MOONWELL RECOVERY AWAITS.");

        public static SylvanFirstExpansionEncounterRecipe ForkedCanopy { get; } =
            new SylvanFirstExpansionEncounterRecipe(
                StarterSylvanRealmLayouts.ForkedCanopyId, "canopy.high-bough",
                GuardianEntArchetypeId, 1, MoonwellExpansionNodeRecoveryContentId,
                "AN ENT WATCHES THE HIGH BOUGH. MOONWELL RECOVERY AWAITS.");

        public static SylvanFirstExpansionEncounterRecipe SerpentRoots { get; } =
            new SylvanFirstExpansionEncounterRecipe(
                StarterSylvanRealmLayouts.SerpentRootsId, "serpent.east-burrow",
                SylvanWolfArchetypeId, 1, MoonwellExpansionNodeRecoveryContentId,
                "A WOLF GUARDS THE EAST BURROW. MOONWELL RECOVERY AWAITS.");

        public static IReadOnlyList<SylvanFirstExpansionEncounterRecipe> All { get; } =
            Array.AsReadOnly(new[] { AncientCrossroads, ForkedCanopy, SerpentRoots });

        public static SylvanFirstExpansionEncounterLookupResult FindExact(string layoutId, string socketId)
        {
            if (!HasStableId(layoutId)) return Reject(SylvanFirstExpansionEncounterLookupStatus.LayoutIdInvalid);
            if (!HasStableId(socketId)) return Reject(SylvanFirstExpansionEncounterLookupStatus.SocketIdInvalid);
            if (!Validate(All).IsValid) return Reject(SylvanFirstExpansionEncounterLookupStatus.CatalogueInvalid);

            var layout = StarterSylvanRealmLayoutResolver.ResolveExact(layoutId);
            if (!layout.HasRecipe) return Reject(SylvanFirstExpansionEncounterLookupStatus.LayoutUnknown);
            var firstSocket = layout.Recipe.ExpansionSockets.Count == 0 ? null : layout.Recipe.ExpansionSockets[0];
            if (firstSocket == null) return Reject(SylvanFirstExpansionEncounterLookupStatus.CatalogueInvalid);
            if (!string.Equals(firstSocket.SocketId, socketId, StringComparison.Ordinal))
            {
                return Reject(ContainsSocket(layout.Recipe, socketId)
                    ? SylvanFirstExpansionEncounterLookupStatus.SocketNotFirstAuthored
                    : SylvanFirstExpansionEncounterLookupStatus.SocketUnknown);
            }

            foreach (var recipe in All)
            {
                if (string.Equals(recipe.LayoutId, layoutId, StringComparison.Ordinal)
                    && string.Equals(recipe.SocketId, socketId, StringComparison.Ordinal))
                {
                    return new SylvanFirstExpansionEncounterLookupResult(
                        SylvanFirstExpansionEncounterLookupStatus.Found, recipe);
                }
            }
            return Reject(SylvanFirstExpansionEncounterLookupStatus.CatalogueInvalid);
        }

        public static SylvanFirstExpansionEncounterValidationResult Validate(
            IReadOnlyList<SylvanFirstExpansionEncounterRecipe> recipes)
        {
            var issues = new List<SylvanFirstExpansionEncounterValidationIssue>();
            if (recipes == null || recipes.Count != 3)
                AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.CatalogueCardinalityInvalid);

            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            if (recipes != null)
            {
                foreach (var recipe in recipes)
                {
                    if (recipe == null)
                    {
                        AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.RecipeMissing);
                        continue;
                    }
                    if (!HasStableId(recipe.LayoutId)) AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.LayoutIdInvalid);
                    else if (!layoutIds.Add(recipe.LayoutId)) AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.LayoutIdDuplicate);
                    if (!HasStableId(recipe.SocketId)) AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.SocketIdInvalid);
                    if (!IsArchetypeId(recipe.EncounterArchetypeId)) AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.EncounterArchetypeIdInvalid);
                    if (recipe.EncounterCount <= 0) AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.EncounterCountInvalid);
                    if (!string.Equals(recipe.ExpansionNodeRecoveryContentId, MoonwellExpansionNodeRecoveryContentId, StringComparison.Ordinal)) AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.ExpansionNodeRecoveryContentIdInvalid);
                    if (string.IsNullOrWhiteSpace(recipe.Cue)) AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.CueInvalid);
                    ValidateFirstSocket(recipe, issues);
                    ValidateExactFacts(recipe, issues);
                }
            }
            return new SylvanFirstExpansionEncounterValidationResult(issues);
        }

        private static void ValidateFirstSocket(SylvanFirstExpansionEncounterRecipe recipe,
            ICollection<SylvanFirstExpansionEncounterValidationIssue> issues)
        {
            var layout = StarterSylvanRealmLayoutResolver.ResolveExact(recipe.LayoutId);
            if (!layout.HasRecipe)
            {
                AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.LayoutUnknown);
                return;
            }
            if (layout.Recipe.ExpansionSockets.Count == 0 || layout.Recipe.ExpansionSockets[0] == null
                || !string.Equals(layout.Recipe.ExpansionSockets[0].SocketId, recipe.SocketId, StringComparison.Ordinal))
                AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.FirstSocketMismatch);
        }

        private static void ValidateExactFacts(SylvanFirstExpansionEncounterRecipe recipe,
            ICollection<SylvanFirstExpansionEncounterValidationIssue> issues)
        {
            var expected = ExpectedRecipeFor(recipe.LayoutId, recipe.SocketId);
            if (expected == null) return;
            if (!string.Equals(recipe.EncounterArchetypeId, expected.EncounterArchetypeId, StringComparison.Ordinal))
                AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.EncounterArchetypeMismatch);
            if (recipe.EncounterCount != expected.EncounterCount)
                AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.EncounterCountMismatch);
            if (!string.Equals(recipe.Cue, expected.Cue, StringComparison.Ordinal))
                AddIssue(issues, SylvanFirstExpansionEncounterValidationIssue.CueMismatch);
        }

        private static SylvanFirstExpansionEncounterRecipe ExpectedRecipeFor(string layoutId, string socketId)
        {
            if (string.Equals(layoutId, AncientCrossroads.LayoutId, StringComparison.Ordinal)
                && string.Equals(socketId, AncientCrossroads.SocketId, StringComparison.Ordinal)) return AncientCrossroads;
            if (string.Equals(layoutId, ForkedCanopy.LayoutId, StringComparison.Ordinal)
                && string.Equals(socketId, ForkedCanopy.SocketId, StringComparison.Ordinal)) return ForkedCanopy;
            if (string.Equals(layoutId, SerpentRoots.LayoutId, StringComparison.Ordinal)
                && string.Equals(socketId, SerpentRoots.SocketId, StringComparison.Ordinal)) return SerpentRoots;
            return null;
        }

        private static SylvanFirstExpansionEncounterLookupResult Reject(SylvanFirstExpansionEncounterLookupStatus status)
        {
            return new SylvanFirstExpansionEncounterLookupResult(status, null);
        }

        private static bool ContainsSocket(RealmLayoutRecipe layout, string socketId)
        {
            foreach (var socket in layout.ExpansionSockets)
                if (socket != null && string.Equals(socket.SocketId, socketId, StringComparison.Ordinal)) return true;
            return false;
        }

        private static bool IsArchetypeId(string value)
        {
            return string.Equals(value, SylvanWolfArchetypeId, StringComparison.Ordinal)
                || string.Equals(value, GuardianEntArchetypeId, StringComparison.Ordinal);
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || !IsAlphaNumeric(value[0]) || !IsAlphaNumeric(value[value.Length - 1])) return false;
            for (var index = 1; index < value.Length - 1; index++)
                if (!IsAlphaNumeric(value[index]) && value[index] != '.' && value[index] != '_' && value[index] != '-') return false;
            return true;
        }

        private static bool IsAlphaNumeric(char value)
        {
            return value >= 'a' && value <= 'z' || value >= '0' && value <= '9';
        }

        private static void AddIssue(ICollection<SylvanFirstExpansionEncounterValidationIssue> issues,
            SylvanFirstExpansionEncounterValidationIssue issue)
        {
            if (!issues.Contains(issue)) issues.Add(issue);
        }
    }
}
