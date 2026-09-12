using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.RealmLayoutContracts;

namespace RealmRaiders.Modules.InfernalDefensePacing
{
    public enum InfernalDefensePacingBeatKind
    {
        Unknown,
        InvaderEntry,
        HellhoundPressure,
        FlameTrapOpportunity,
        InfernalBrutePossessionWindow,
        InfernalHeartObjective
    }

    public enum InfernalDefensePacingRequirement
    {
        Unknown,
        Required,
        Optional
    }

    public enum InfernalDefensePacingLookupStatus
    {
        Found,
        LayoutIdInvalid,
        NotFound,
        CatalogueInvalid
    }

    public enum InfernalDefensePacingValidationIssue
    {
        RecipeMissing,
        LayoutIdInvalid,
        LayoutNotFound,
        LayoutInvalid,
        TacticalIntentInvalid,
        BeatCardinalityInvalid,
        BeatMissing,
        BeatIdInvalid,
        BeatIdDuplicate,
        BeatRoleInvalid,
        BeatRoleDuplicate,
        BeatKindInvalid,
        BeatRequirementInvalid,
        BeatDependencyInvalid,
        BeatDependencyCycle,
        PacingRoleCoverageInvalid,
        HeartFinalInvalid,
        EntryRequirementInvalid,
        HeartRequirementInvalid,
        RequiredBeatDependsOnOptional,
        RequiredBeatNotHeartPrerequisite,
        RequiredBeatUnreachable
    }

    public enum InfernalDefensePacingCatalogueValidationIssue
    {
        CatalogueMissing,
        RecipeCardinalityInvalid,
        RecipeMissing,
        LayoutIdDuplicate,
        RecipeInvalid,
        LayoutCoverageInvalid,
        PacingSignatureDuplicate
    }

    /// <summary>
    /// One immutable authored pacing fact. It does not trigger gameplay, create
    /// entities, select targets, or retain runtime progression state.
    /// </summary>
    public sealed class InfernalDefensePacingBeat
    {
        public InfernalDefensePacingBeat(
            string beatId,
            string roleId,
            InfernalDefensePacingBeatKind kind,
            InfernalDefensePacingRequirement requirement,
            IReadOnlyList<string> dependsOnBeatIds)
        {
            BeatId = beatId;
            RoleId = roleId;
            Kind = kind;
            Requirement = requirement;
            DependsOnBeatIds = Snapshot(dependsOnBeatIds);
        }

        public string BeatId { get; }

        public string RoleId { get; }

        public InfernalDefensePacingBeatKind Kind { get; }

        public InfernalDefensePacingRequirement Requirement { get; }

        public IReadOnlyList<string> DependsOnBeatIds { get; }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<T>());
            }

            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// One exact-layout, immutable defense pacing recipe. Core owns every later
    /// action, encounter activation, combat result, and player-facing UI.
    /// </summary>
    public sealed class InfernalDefensePacingRecipe
    {
        public InfernalDefensePacingRecipe(
            string layoutId,
            string tacticalIntent,
            IReadOnlyList<InfernalDefensePacingBeat> beats)
        {
            LayoutId = layoutId;
            TacticalIntent = tacticalIntent;
            Beats = Snapshot(beats);
        }

        public string LayoutId { get; }

        public string TacticalIntent { get; }

        public IReadOnlyList<InfernalDefensePacingBeat> Beats { get; }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<T>());
            }

            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalDefensePacingLookupResult
    {
        internal InfernalDefensePacingLookupResult(
            InfernalDefensePacingLookupStatus status,
            InfernalDefensePacingRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public InfernalDefensePacingLookupStatus Status { get; }

        public InfernalDefensePacingRecipe Recipe { get; }

        public bool Found
        {
            get
            {
                return Status == InfernalDefensePacingLookupStatus.Found && Recipe != null;
            }
        }
    }

    public sealed class InfernalDefensePacingValidationResult
    {
        internal InfernalDefensePacingValidationResult(
            IReadOnlyList<InfernalDefensePacingValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalDefensePacingValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<InfernalDefensePacingValidationIssue> Snapshot(
            IReadOnlyList<InfernalDefensePacingValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<InfernalDefensePacingValidationIssue>());
            }

            var copy = new InfernalDefensePacingValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class InfernalDefensePacingCatalogueValidationResult
    {
        internal InfernalDefensePacingCatalogueValidationResult(
            IReadOnlyList<InfernalDefensePacingCatalogueValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<InfernalDefensePacingCatalogueValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<InfernalDefensePacingCatalogueValidationIssue> Snapshot(
            IReadOnlyList<InfernalDefensePacingCatalogueValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(
                    Array.Empty<InfernalDefensePacingCatalogueValidationIssue>());
            }

            var copy = new InfernalDefensePacingCatalogueValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// Fail-closed structural validation only. It resolves existing cached
    /// layouts but never changes their graph, selects a route, or executes a beat.
    /// </summary>
    public static class InfernalDefensePacingValidator
    {
        public static InfernalDefensePacingValidationResult Validate(
            InfernalDefensePacingRecipe recipe)
        {
            var issues = new List<InfernalDefensePacingValidationIssue>();
            if (recipe == null)
            {
                AddIssue(issues, InfernalDefensePacingValidationIssue.RecipeMissing);
                return new InfernalDefensePacingValidationResult(issues);
            }

            RealmLayoutGraph layout = null;
            if (!HasStableId(recipe.LayoutId))
            {
                AddIssue(issues, InfernalDefensePacingValidationIssue.LayoutIdInvalid);
            }
            else
            {
                layout = FindLayout(recipe.LayoutId);
                if (layout == null)
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.LayoutNotFound);
                }
                else if (!InfernalDefenseLayoutValidator.Validate(layout).IsValid)
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.LayoutInvalid);
                }
            }

            if (layout == null)
            {
                return new InfernalDefensePacingValidationResult(issues);
            }

            if (string.IsNullOrEmpty(recipe.TacticalIntent)
                || recipe.TacticalIntent.Length > 96)
            {
                AddIssue(issues, InfernalDefensePacingValidationIssue.TacticalIntentInvalid);
            }

            if (recipe.Beats.Count != ExpectedRoleIds.Count)
            {
                AddIssue(issues, InfernalDefensePacingValidationIssue.BeatCardinalityInvalid);
            }

            var layoutRoleIds = CreateLayoutRoleIds(layout);
            var beatIds = new HashSet<string>(StringComparer.Ordinal);
            var beatIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
            var roleCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var beatsById = new Dictionary<string, InfernalDefensePacingBeat>(
                StringComparer.Ordinal);
            for (var index = 0; index < recipe.Beats.Count; index++)
            {
                var beat = recipe.Beats[index];
                if (beat == null)
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.BeatMissing);
                    continue;
                }

                if (!HasStableId(beat.BeatId))
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.BeatIdInvalid);
                }
                else if (!beatIds.Add(beat.BeatId))
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.BeatIdDuplicate);
                }
                else
                {
                    beatIndexes.Add(beat.BeatId, index);
                    beatsById.Add(beat.BeatId, beat);
                }

                if (!IsExpectedRoleId(beat.RoleId)
                    || !layoutRoleIds.Contains(beat.RoleId))
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.BeatRoleInvalid);
                }
                else
                {
                    Increment(roleCounts, beat.RoleId);
                }

                if (!HasExpectedKind(beat.RoleId, beat.Kind))
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.BeatKindInvalid);
                }

                if (beat.Requirement != InfernalDefensePacingRequirement.Required
                    && beat.Requirement != InfernalDefensePacingRequirement.Optional)
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.BeatRequirementInvalid);
                }
            }

            foreach (var expectedRoleId in ExpectedRoleIds)
            {
                if (!roleCounts.TryGetValue(expectedRoleId, out var count) || count != 1)
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.PacingRoleCoverageInvalid);
                }
            }

            foreach (var pair in roleCounts)
            {
                if (pair.Value > 1)
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.BeatRoleDuplicate);
                }
            }

            ValidateDependencies(recipe.Beats, beatIndexes, issues);
            if (HasDependencyCycle(beatsById))
            {
                AddIssue(issues, InfernalDefensePacingValidationIssue.BeatDependencyCycle);
            }

            var finalHeartObjective = GetFinalHeartObjective(recipe.Beats);
            if (finalHeartObjective == null)
            {
                AddIssue(issues, InfernalDefensePacingValidationIssue.HeartFinalInvalid);
            }
            else
            {
                ValidateRequirementDagSemantics(
                    beatsById,
                    finalHeartObjective,
                    issues);
            }

            if (layout != null)
            {
                ValidateRequiredBeatReachability(recipe.Beats, layout, issues);
            }

            return new InfernalDefensePacingValidationResult(issues);
        }

        public static InfernalDefensePacingCatalogueValidationResult ValidateCatalogue(
            IReadOnlyList<InfernalDefensePacingRecipe> recipes)
        {
            var issues = new List<InfernalDefensePacingCatalogueValidationIssue>();
            if (recipes == null)
            {
                AddIssue(issues, InfernalDefensePacingCatalogueValidationIssue.CatalogueMissing);
                return new InfernalDefensePacingCatalogueValidationResult(issues);
            }

            if (recipes.Count != StarterInfernalDefenseLayouts.All.Count)
            {
                AddIssue(issues,
                    InfernalDefensePacingCatalogueValidationIssue.RecipeCardinalityInvalid);
            }

            var layoutIds = new HashSet<string>(StringComparer.Ordinal);
            var signatures = new HashSet<string>(StringComparer.Ordinal);
            foreach (var recipe in recipes)
            {
                if (recipe == null)
                {
                    AddIssue(issues, InfernalDefensePacingCatalogueValidationIssue.RecipeMissing);
                    continue;
                }

                if (!layoutIds.Add(recipe.LayoutId))
                {
                    AddIssue(issues,
                        InfernalDefensePacingCatalogueValidationIssue.LayoutIdDuplicate);
                }

                if (!Validate(recipe).IsValid)
                {
                    AddIssue(issues, InfernalDefensePacingCatalogueValidationIssue.RecipeInvalid);
                }

                var signature = CreatePacingSignature(recipe);
                if (signature != null && !signatures.Add(signature))
                {
                    AddIssue(issues,
                        InfernalDefensePacingCatalogueValidationIssue.PacingSignatureDuplicate);
                }
            }

            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                if (!layoutIds.Contains(layout.LayoutId))
                {
                    AddIssue(issues,
                        InfernalDefensePacingCatalogueValidationIssue.LayoutCoverageInvalid);
                }
            }

            return new InfernalDefensePacingCatalogueValidationResult(issues);
        }

        /// <summary>
        /// Canonical authored beat order, requirement, role, and prerequisite roles.
        /// It is validation evidence only and never selects a player route.
        /// </summary>
        public static string CreatePacingSignature(InfernalDefensePacingRecipe recipe)
        {
            if (recipe == null || !Validate(recipe).IsValid)
            {
                return null;
            }

            var roleIdsByBeatId = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var beat in recipe.Beats)
            {
                roleIdsByBeatId.Add(beat.BeatId, beat.RoleId);
            }

            var parts = new string[recipe.Beats.Count];
            for (var index = 0; index < recipe.Beats.Count; index++)
            {
                var beat = recipe.Beats[index];
                var dependencies = new string[beat.DependsOnBeatIds.Count];
                for (var dependencyIndex = 0;
                     dependencyIndex < beat.DependsOnBeatIds.Count;
                     dependencyIndex++)
                {
                    dependencies[dependencyIndex] = roleIdsByBeatId[
                        beat.DependsOnBeatIds[dependencyIndex]];
                }

                Array.Sort(dependencies, StringComparer.Ordinal);
                parts[index] = beat.RoleId
                    + ":"
                    + ((int)beat.Kind).ToString()
                    + ":"
                    + ((int)beat.Requirement).ToString()
                    + ":"
                    + string.Join(",", dependencies);
            }

            return string.Join("|", parts);
        }

        private static readonly IReadOnlyList<string> ExpectedRoleIds =
            Array.AsReadOnly(new[]
            {
                StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId
            });

        private static HashSet<string> CreateLayoutRoleIds(RealmLayoutGraph layout)
        {
            var roleIds = new HashSet<string>(StringComparer.Ordinal);
            if (layout == null)
            {
                return roleIds;
            }

            foreach (var node in layout.Nodes)
            {
                if (node != null)
                {
                    roleIds.Add(node.GameplayRoleId);
                }
            }

            return roleIds;
        }

        private static RealmLayoutGraph FindLayout(string layoutId)
        {
            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                if (string.Equals(layout.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return layout;
                }
            }

            return null;
        }

        private static bool HasExpectedKind(
            string roleId,
            InfernalDefensePacingBeatKind kind)
        {
            if (string.Equals(roleId, StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                    StringComparison.Ordinal))
            {
                return kind == InfernalDefensePacingBeatKind.InvaderEntry;
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                    StringComparison.Ordinal))
            {
                return kind == InfernalDefensePacingBeatKind.HellhoundPressure;
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                    StringComparison.Ordinal))
            {
                return kind == InfernalDefensePacingBeatKind.FlameTrapOpportunity;
            }

            if (string.Equals(roleId, StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                    StringComparison.Ordinal))
            {
                return kind == InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow;
            }

            return string.Equals(roleId, StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                    StringComparison.Ordinal)
                && kind == InfernalDefensePacingBeatKind.InfernalHeartObjective;
        }

        private static bool IsExpectedRoleId(string roleId)
        {
            foreach (var expectedRoleId in ExpectedRoleIds)
            {
                if (string.Equals(roleId, expectedRoleId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateDependencies(
            IReadOnlyList<InfernalDefensePacingBeat> beats,
            IReadOnlyDictionary<string, int> beatIndexes,
            ICollection<InfernalDefensePacingValidationIssue> issues)
        {
            for (var index = 0; index < beats.Count; index++)
            {
                var beat = beats[index];
                if (beat == null)
                {
                    continue;
                }

                var requiresNoDependencies = beat.Kind
                    == InfernalDefensePacingBeatKind.InvaderEntry;
                if (requiresNoDependencies && beat.DependsOnBeatIds.Count != 0
                    || !requiresNoDependencies && beat.DependsOnBeatIds.Count != 1)
                {
                    AddIssue(issues, InfernalDefensePacingValidationIssue.BeatDependencyInvalid);
                }

                var dependencyIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var dependencyId in beat.DependsOnBeatIds)
                {
                    if (!HasStableId(dependencyId)
                        || !dependencyIds.Add(dependencyId)
                        || !beatIndexes.TryGetValue(dependencyId, out var dependencyIndex)
                        || dependencyIndex >= index)
                    {
                        AddIssue(issues, InfernalDefensePacingValidationIssue.BeatDependencyInvalid);
                    }
                }
            }
        }

        private static bool HasDependencyCycle(
            IReadOnlyDictionary<string, InfernalDefensePacingBeat> beatsById)
        {
            var visiting = new HashSet<string>(StringComparer.Ordinal);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            foreach (var beatId in beatsById.Keys)
            {
                if (ContainsCycle(beatId, beatsById, visiting, visited))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsCycle(
            string beatId,
            IReadOnlyDictionary<string, InfernalDefensePacingBeat> beatsById,
            ISet<string> visiting,
            ISet<string> visited)
        {
            if (visited.Contains(beatId))
            {
                return false;
            }

            if (!visiting.Add(beatId))
            {
                return true;
            }

            var beat = beatsById[beatId];
            foreach (var dependencyId in beat.DependsOnBeatIds)
            {
                if (beatsById.ContainsKey(dependencyId)
                    && ContainsCycle(dependencyId, beatsById, visiting, visited))
                {
                    return true;
                }
            }

            visiting.Remove(beatId);
            visited.Add(beatId);
            return false;
        }

        private static InfernalDefensePacingBeat GetFinalHeartObjective(
            IReadOnlyList<InfernalDefensePacingBeat> beats)
        {
            if (beats.Count == 0)
            {
                return null;
            }

            var finalBeat = beats[beats.Count - 1];
            return finalBeat != null
                && string.Equals(finalBeat.RoleId,
                    StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                    StringComparison.Ordinal)
                && finalBeat.Kind == InfernalDefensePacingBeatKind.InfernalHeartObjective
                ? finalBeat
                : null;
        }

        private static void ValidateRequirementDagSemantics(
            IReadOnlyDictionary<string, InfernalDefensePacingBeat> beatsById,
            InfernalDefensePacingBeat finalHeartObjective,
            ICollection<InfernalDefensePacingValidationIssue> issues)
        {
            var entryBeat = FindBeatByRole(
                beatsById,
                StarterInfernalDefenseLayouts.InvaderEntryRoleId);
            if (entryBeat != null
                && entryBeat.Requirement != InfernalDefensePacingRequirement.Required)
            {
                AddIssue(issues,
                    InfernalDefensePacingValidationIssue.EntryRequirementInvalid);
            }

            if (finalHeartObjective.Requirement != InfernalDefensePacingRequirement.Required)
            {
                AddIssue(issues,
                    InfernalDefensePacingValidationIssue.HeartRequirementInvalid);
            }

            var heartPrerequisiteIds = CreatePrerequisiteClosure(
                finalHeartObjective.BeatId,
                beatsById);
            foreach (var pair in beatsById)
            {
                if (pair.Value.Requirement != InfernalDefensePacingRequirement.Required)
                {
                    continue;
                }

                if (HasOptionalPrerequisite(pair.Key, beatsById))
                {
                    AddIssue(issues,
                        InfernalDefensePacingValidationIssue.RequiredBeatDependsOnOptional);
                }

                if (!heartPrerequisiteIds.Contains(pair.Key))
                {
                    AddIssue(issues,
                        InfernalDefensePacingValidationIssue.RequiredBeatNotHeartPrerequisite);
                }
            }
        }

        private static InfernalDefensePacingBeat FindBeatByRole(
            IReadOnlyDictionary<string, InfernalDefensePacingBeat> beatsById,
            string roleId)
        {
            foreach (var pair in beatsById)
            {
                if (string.Equals(pair.Value.RoleId, roleId, StringComparison.Ordinal))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        private static ISet<string> CreatePrerequisiteClosure(
            string beatId,
            IReadOnlyDictionary<string, InfernalDefensePacingBeat> beatsById)
        {
            var prerequisiteIds = new HashSet<string>(StringComparer.Ordinal);
            if (!beatsById.ContainsKey(beatId))
            {
                return prerequisiteIds;
            }

            var pending = new Queue<string>();
            prerequisiteIds.Add(beatId);
            pending.Enqueue(beatId);
            while (pending.Count > 0)
            {
                var currentBeatId = pending.Dequeue();
                foreach (var dependencyId in beatsById[currentBeatId].DependsOnBeatIds)
                {
                    if (beatsById.ContainsKey(dependencyId)
                        && prerequisiteIds.Add(dependencyId))
                    {
                        pending.Enqueue(dependencyId);
                    }
                }
            }

            return prerequisiteIds;
        }

        private static bool HasOptionalPrerequisite(
            string beatId,
            IReadOnlyDictionary<string, InfernalDefensePacingBeat> beatsById)
        {
            if (!beatsById.ContainsKey(beatId))
            {
                return false;
            }

            var visitedBeatIds = new HashSet<string>(StringComparer.Ordinal);
            var pending = new Queue<string>();
            foreach (var dependencyId in beatsById[beatId].DependsOnBeatIds)
            {
                if (beatsById.ContainsKey(dependencyId) && visitedBeatIds.Add(dependencyId))
                {
                    pending.Enqueue(dependencyId);
                }
            }

            while (pending.Count > 0)
            {
                var prerequisiteId = pending.Dequeue();
                var prerequisite = beatsById[prerequisiteId];
                if (prerequisite.Requirement == InfernalDefensePacingRequirement.Optional)
                {
                    return true;
                }

                foreach (var dependencyId in prerequisite.DependsOnBeatIds)
                {
                    if (beatsById.ContainsKey(dependencyId) && visitedBeatIds.Add(dependencyId))
                    {
                        pending.Enqueue(dependencyId);
                    }
                }
            }

            return false;
        }

        private static void ValidateRequiredBeatReachability(
            IReadOnlyList<InfernalDefensePacingBeat> beats,
            RealmLayoutGraph layout,
            ICollection<InfernalDefensePacingValidationIssue> issues)
        {
            var reachableNodeIds = ReachableSafeNodeIds(layout);
            var nodesByRoleId = new Dictionary<string, RealmLayoutGraphNode>(
                StringComparer.Ordinal);
            foreach (var node in layout.Nodes)
            {
                if (node != null && !nodesByRoleId.ContainsKey(node.GameplayRoleId))
                {
                    nodesByRoleId.Add(node.GameplayRoleId, node);
                }
            }

            foreach (var beat in beats)
            {
                if (beat == null || beat.Requirement != InfernalDefensePacingRequirement.Required)
                {
                    continue;
                }

                if (!nodesByRoleId.TryGetValue(beat.RoleId, out var node)
                    || !reachableNodeIds.Contains(node.NodeId))
                {
                    AddIssue(issues,
                        InfernalDefensePacingValidationIssue.RequiredBeatUnreachable);
                }
            }
        }

        private static ISet<string> ReachableSafeNodeIds(RealmLayoutGraph layout)
        {
            var reachable = new HashSet<string>(StringComparer.Ordinal);
            var entryNodeId = string.Empty;
            foreach (var node in layout.Nodes)
            {
                if (node != null && string.Equals(node.GameplayRoleId,
                        StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                        StringComparison.Ordinal))
                {
                    entryNodeId = node.NodeId;
                    break;
                }
            }

            if (string.IsNullOrEmpty(entryNodeId))
            {
                return reachable;
            }

            var pending = new Queue<string>();
            reachable.Add(entryNodeId);
            pending.Enqueue(entryNodeId);
            while (pending.Count > 0)
            {
                var currentNodeId = pending.Dequeue();
                foreach (var edge in layout.Edges)
                {
                    if (edge == null || !edge.IsActivePathSafe)
                    {
                        continue;
                    }

                    var nextNodeId = string.Equals(edge.FromNodeId, currentNodeId,
                        StringComparison.Ordinal)
                        ? edge.ToNodeId
                        : string.Equals(edge.ToNodeId, currentNodeId, StringComparison.Ordinal)
                            ? edge.FromNodeId
                            : null;
                    if (nextNodeId != null && reachable.Add(nextNodeId))
                    {
                        pending.Enqueue(nextNodeId);
                    }
                }
            }

            return reachable;
        }

        private static void Increment(IDictionary<string, int> counts, string roleId)
        {
            if (!counts.TryGetValue(roleId, out var count))
            {
                counts.Add(roleId, 1);
                return;
            }

            counts[roleId] = count + 1;
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || !IsAsciiAlphaNumeric(value[0])
                || !IsAsciiAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            for (var index = 1; index < value.Length - 1; index++)
            {
                var symbol = value[index];
                if (!IsAsciiAlphaNumeric(symbol)
                    && symbol != '.'
                    && symbol != '_'
                    && symbol != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiAlphaNumeric(char symbol)
        {
            return symbol >= 'a' && symbol <= 'z'
                || symbol >= '0' && symbol <= '9';
        }

        private static void AddIssue(
            ICollection<InfernalDefensePacingValidationIssue> issues,
            InfernalDefensePacingValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private static void AddIssue(
            ICollection<InfernalDefensePacingCatalogueValidationIssue> issues,
            InfernalDefensePacingCatalogueValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }

    /// <summary>
    /// Three cached, authored pacing facts for the exact existing Infernal layouts.
    /// They describe player-visible order only; they do not make runtime decisions.
    /// </summary>
    public static class StarterInfernalDefensePacing
    {
        public static InfernalDefensePacingRecipe AshenSpur { get; } =
            CreateAshenSpur();

        public static InfernalDefensePacingRecipe CinderFork { get; } =
            CreateCinderFork();

        public static InfernalDefensePacingRecipe EmberCircuit { get; } =
            CreateEmberCircuit();

        public static IReadOnlyList<InfernalDefensePacingRecipe> All { get; } =
            Array.AsReadOnly(new[]
            {
                AshenSpur,
                CinderFork,
                EmberCircuit
            });

        private static InfernalDefensePacingRecipe CreateAshenSpur()
        {
            return new InfernalDefensePacingRecipe(
                StarterInfernalDefenseLayouts.AshenSpurId,
                "OPTIONAL HELLHOUND SPUR; COMMIT THROUGH FLAME TO THE BRUTE.",
                new InfernalDefensePacingBeat[]
                {
                    Beat(
                        "ashen-spur.entry",
                        StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                        InfernalDefensePacingBeatKind.InvaderEntry,
                        InfernalDefensePacingRequirement.Required),
                    Beat(
                        "ashen-spur.hellhound-spur",
                        StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                        InfernalDefensePacingBeatKind.HellhoundPressure,
                        InfernalDefensePacingRequirement.Optional,
                        "ashen-spur.entry"),
                    Beat(
                        "ashen-spur.flame-route",
                        StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                        InfernalDefensePacingBeatKind.FlameTrapOpportunity,
                        InfernalDefensePacingRequirement.Required,
                        "ashen-spur.entry"),
                    Beat(
                        "ashen-spur.brute-window",
                        StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                        InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow,
                        InfernalDefensePacingRequirement.Required,
                        "ashen-spur.flame-route"),
                    Beat(
                        "ashen-spur.heart-objective",
                        StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                        InfernalDefensePacingBeatKind.InfernalHeartObjective,
                        InfernalDefensePacingRequirement.Required,
                        "ashen-spur.brute-window")
                });
        }

        private static InfernalDefensePacingRecipe CreateCinderFork()
        {
            return new InfernalDefensePacingRecipe(
                StarterInfernalDefenseLayouts.CinderForkId,
                "HELLHOUND ROUTE REQUIRED; FLAME IS OPTIONAL BEFORE THE BRUTE.",
                new InfernalDefensePacingBeat[]
                {
                    Beat(
                        "cinder-fork.entry",
                        StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                        InfernalDefensePacingBeatKind.InvaderEntry,
                        InfernalDefensePacingRequirement.Required),
                    Beat(
                        "cinder-fork.hellhound-pressure",
                        StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                        InfernalDefensePacingBeatKind.HellhoundPressure,
                        InfernalDefensePacingRequirement.Required,
                        "cinder-fork.entry"),
                    Beat(
                        "cinder-fork.flame-choice",
                        StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                        InfernalDefensePacingBeatKind.FlameTrapOpportunity,
                        InfernalDefensePacingRequirement.Optional,
                        "cinder-fork.entry"),
                    Beat(
                        "cinder-fork.brute-window",
                        StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                        InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow,
                        InfernalDefensePacingRequirement.Required,
                        "cinder-fork.hellhound-pressure"),
                    Beat(
                        "cinder-fork.heart-objective",
                        StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                        InfernalDefensePacingBeatKind.InfernalHeartObjective,
                        InfernalDefensePacingRequirement.Required,
                        "cinder-fork.brute-window")
                });
        }

        private static InfernalDefensePacingRecipe CreateEmberCircuit()
        {
            return new InfernalDefensePacingRecipe(
                StarterInfernalDefenseLayouts.EmberCircuitId,
                "CLEAR THE HELLHOUND; CHOOSE FLAME OR THE DIRECT BRUTE ROUTE.",
                new InfernalDefensePacingBeat[]
                {
                    Beat(
                        "ember-circuit.entry",
                        StarterInfernalDefenseLayouts.InvaderEntryRoleId,
                        InfernalDefensePacingBeatKind.InvaderEntry,
                        InfernalDefensePacingRequirement.Required),
                    Beat(
                        "ember-circuit.hellhound-pressure",
                        StarterInfernalDefenseLayouts.HellhoundEncounterRoleId,
                        InfernalDefensePacingBeatKind.HellhoundPressure,
                        InfernalDefensePacingRequirement.Required,
                        "ember-circuit.entry"),
                    Beat(
                        "ember-circuit.flame-choice",
                        StarterInfernalDefenseLayouts.FlameTrapHazardRoleId,
                        InfernalDefensePacingBeatKind.FlameTrapOpportunity,
                        InfernalDefensePacingRequirement.Optional,
                        "ember-circuit.hellhound-pressure"),
                    Beat(
                        "ember-circuit.brute-window",
                        StarterInfernalDefenseLayouts.InfernalBrutePossessionRoleId,
                        InfernalDefensePacingBeatKind.InfernalBrutePossessionWindow,
                        InfernalDefensePacingRequirement.Required,
                        "ember-circuit.hellhound-pressure"),
                    Beat(
                        "ember-circuit.heart-objective",
                        StarterInfernalDefenseLayouts.InfernalHeartObjectiveRoleId,
                        InfernalDefensePacingBeatKind.InfernalHeartObjective,
                        InfernalDefensePacingRequirement.Required,
                        "ember-circuit.brute-window")
                });
        }

        private static InfernalDefensePacingBeat Beat(
            string beatId,
            string roleId,
            InfernalDefensePacingBeatKind kind,
            InfernalDefensePacingRequirement requirement,
            params string[] dependsOnBeatIds)
        {
            return new InfernalDefensePacingBeat(
                beatId,
                roleId,
                kind,
                requirement,
                dependsOnBeatIds);
        }
    }

    /// <summary>Exact ordinal cached lookup with no selection or runtime authority.</summary>
    public static class StarterInfernalDefensePacingResolver
    {
        public static InfernalDefensePacingLookupResult ResolveExact(string layoutId)
        {
            if (!HasStableId(layoutId))
            {
                return new InfernalDefensePacingLookupResult(
                    InfernalDefensePacingLookupStatus.LayoutIdInvalid,
                    null);
            }

            if (!InfernalDefensePacingValidator.ValidateCatalogue(
                    StarterInfernalDefensePacing.All).IsValid)
            {
                return new InfernalDefensePacingLookupResult(
                    InfernalDefensePacingLookupStatus.CatalogueInvalid,
                    null);
            }

            foreach (var recipe in StarterInfernalDefensePacing.All)
            {
                if (string.Equals(recipe.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return new InfernalDefensePacingLookupResult(
                        InfernalDefensePacingLookupStatus.Found,
                        recipe);
                }
            }

            return new InfernalDefensePacingLookupResult(
                InfernalDefensePacingLookupStatus.NotFound,
                null);
        }

        private static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value)
                || !IsAsciiAlphaNumeric(value[0])
                || !IsAsciiAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            for (var index = 1; index < value.Length - 1; index++)
            {
                var symbol = value[index];
                if (!IsAsciiAlphaNumeric(symbol)
                    && symbol != '.'
                    && symbol != '_'
                    && symbol != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAsciiAlphaNumeric(char symbol)
        {
            return symbol >= 'a' && symbol <= 'z'
                || symbol >= '0' && symbol <= '9';
        }
    }
}
