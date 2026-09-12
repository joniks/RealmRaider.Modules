using System;
using System.Collections.Generic;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.SylvanEncounterPacing
{
    public enum SylvanEncounterPacingBeatKind
    {
        Unknown,
        Encounter,
        HazardRoute,
        RecoveryOpportunity,
        Objective
    }

    public enum SylvanEncounterPacingRequirement
    {
        Unknown,
        Required,
        Optional
    }

    public enum SylvanEncounterPacingChoiceKind
    {
        Unknown,
        OptionalBranch,
        RouteChoice
    }

    public enum SylvanEncounterPacingLookupStatus
    {
        Found,
        InvalidLayoutId,
        NotFound
    }

    public enum SylvanEncounterPacingValidationIssue
    {
        RecipeMissing,
        LayoutIdInvalid,
        LayoutNotFound,
        LayoutInvalid,
        TacticalSummaryInvalid,
        BeatCardinalityInvalid,
        BeatMissing,
        BeatIdInvalid,
        BeatIdDuplicate,
        BeatRoleInvalid,
        BeatRoleDuplicate,
        BeatKindInvalid,
        BeatRequirementInvalid,
        PacingRoleCoverageInvalid,
        HeartSequenceInvalid,
        ChoiceMissing,
        ChoiceIdInvalid,
        ChoiceIdDuplicate,
        ChoiceKindInvalid,
        ChoiceRouteInvalid,
        ChoiceRoleInvalid,
        ChoiceEdgeInvalid
    }

    /// <summary>
    /// One immutable player-visible pacing fact. Core owns encounter activation,
    /// combat, spawning, rewards, and all runtime state.
    /// </summary>
    public sealed class SylvanEncounterPacingBeat
    {
        public SylvanEncounterPacingBeat(
            string beatId,
            SylvanRealmNodeMaterializationRole role,
            SylvanEncounterPacingBeatKind kind,
            SylvanEncounterPacingRequirement requirement)
        {
            BeatId = beatId;
            Role = role;
            Kind = kind;
            Requirement = requirement;
        }

        public string BeatId { get; }

        public SylvanRealmNodeMaterializationRole Role { get; }

        public SylvanEncounterPacingBeatKind Kind { get; }

        public SylvanEncounterPacingRequirement Requirement { get; }
    }

    /// <summary>
    /// One factual optional branch or two-route choice over existing layout roles.
    /// Route roles are not actions, spawns, or gates.
    /// </summary>
    public sealed class SylvanEncounterPacingChoice
    {
        public SylvanEncounterPacingChoice(
            string choiceId,
            SylvanEncounterPacingChoiceKind kind,
            IReadOnlyList<SylvanRealmNodeMaterializationRole> firstRoute,
            IReadOnlyList<SylvanRealmNodeMaterializationRole> secondRoute)
        {
            ChoiceId = choiceId;
            Kind = kind;
            FirstRoute = Snapshot(firstRoute);
            SecondRoute = Snapshot(secondRoute);
        }

        public string ChoiceId { get; }

        public SylvanEncounterPacingChoiceKind Kind { get; }

        public IReadOnlyList<SylvanRealmNodeMaterializationRole> FirstRoute { get; }

        public IReadOnlyList<SylvanRealmNodeMaterializationRole> SecondRoute { get; }

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

    /// <summary>One immutable, exact-layout pacing recipe with no gameplay authority.</summary>
    public sealed class SylvanEncounterPacingRecipe
    {
        public SylvanEncounterPacingRecipe(
            string layoutId,
            string tacticalSummary,
            IReadOnlyList<SylvanEncounterPacingBeat> beats,
            IReadOnlyList<SylvanEncounterPacingChoice> choices)
        {
            LayoutId = layoutId;
            TacticalSummary = tacticalSummary;
            Beats = Snapshot(beats);
            Choices = Snapshot(choices);
        }

        public string LayoutId { get; }

        public string TacticalSummary { get; }

        public IReadOnlyList<SylvanEncounterPacingBeat> Beats { get; }

        public IReadOnlyList<SylvanEncounterPacingChoice> Choices { get; }

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

    public sealed class SylvanEncounterPacingLookupResult
    {
        internal SylvanEncounterPacingLookupResult(
            SylvanEncounterPacingLookupStatus status,
            SylvanEncounterPacingRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public SylvanEncounterPacingLookupStatus Status { get; }

        public SylvanEncounterPacingRecipe Recipe { get; }

        public bool Found
        {
            get
            {
                return Status == SylvanEncounterPacingLookupStatus.Found && Recipe != null;
            }
        }
    }

    public sealed class SylvanEncounterPacingValidationResult
    {
        internal SylvanEncounterPacingValidationResult(
            IReadOnlyList<SylvanEncounterPacingValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<SylvanEncounterPacingValidationIssue> Issues { get; }

        public bool IsValid
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        private static IReadOnlyList<SylvanEncounterPacingValidationIssue> Snapshot(
            IReadOnlyList<SylvanEncounterPacingValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<SylvanEncounterPacingValidationIssue>());
            }

            var copy = new SylvanEncounterPacingValidationIssue[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    /// <summary>
    /// Exact lookup and fail-closed validation for caller-selected pacing facts.
    /// This type neither chooses a layout nor controls its gameplay realization.
    /// </summary>
    public static class SylvanEncounterPacingEvidence
    {
        public static SylvanEncounterPacingLookupResult FindByLayoutId(string layoutId)
        {
            if (!HasStableId(layoutId))
            {
                return new SylvanEncounterPacingLookupResult(
                    SylvanEncounterPacingLookupStatus.InvalidLayoutId,
                    null);
            }

            foreach (var recipe in StarterSylvanEncounterPacing.All)
            {
                if (string.Equals(recipe.LayoutId, layoutId, StringComparison.Ordinal))
                {
                    return new SylvanEncounterPacingLookupResult(
                        SylvanEncounterPacingLookupStatus.Found,
                        recipe);
                }
            }

            return new SylvanEncounterPacingLookupResult(
                SylvanEncounterPacingLookupStatus.NotFound,
                null);
        }

        public static SylvanEncounterPacingValidationResult Validate(
            SylvanEncounterPacingRecipe recipe)
        {
            var issues = new List<SylvanEncounterPacingValidationIssue>();
            if (recipe == null)
            {
                AddIssue(issues, SylvanEncounterPacingValidationIssue.RecipeMissing);
                return new SylvanEncounterPacingValidationResult(issues);
            }

            var layout = ResolveLayout(recipe.LayoutId, issues);
            if (string.IsNullOrWhiteSpace(recipe.TacticalSummary)
                || !string.Equals(recipe.TacticalSummary, recipe.TacticalSummary.ToUpperInvariant(), StringComparison.Ordinal))
            {
                AddIssue(issues, SylvanEncounterPacingValidationIssue.TacticalSummaryInvalid);
            }

            var beatRoles = ValidateBeats(recipe.Beats, layout, issues);
            ValidatePacingRoleCoverage(recipe.Beats, layout, issues);
            ValidateRequiredHeartSequence(recipe.Beats, layout, issues);
            ValidateChoices(recipe.Choices, layout, beatRoles, issues);

            return new SylvanEncounterPacingValidationResult(issues);
        }

        private static RealmLayoutRecipe ResolveLayout(
            string layoutId,
            ICollection<SylvanEncounterPacingValidationIssue> issues)
        {
            if (!HasStableId(layoutId))
            {
                AddIssue(issues, SylvanEncounterPacingValidationIssue.LayoutIdInvalid);
                return null;
            }

            var resolved = StarterSylvanRealmLayoutResolver.ResolveExact(layoutId);
            if (resolved.Status != RealmLayoutResolveStatus.Resolved || !resolved.HasRecipe)
            {
                AddIssue(issues, SylvanEncounterPacingValidationIssue.LayoutNotFound);
                return null;
            }

            if (!RealmLayoutRecipeValidator.ValidateStarterRecipe(resolved.Recipe).IsValid)
            {
                AddIssue(issues, SylvanEncounterPacingValidationIssue.LayoutInvalid);
                return null;
            }

            return resolved.Recipe;
        }

        private static ISet<SylvanRealmNodeMaterializationRole> ValidateBeats(
            IReadOnlyList<SylvanEncounterPacingBeat> beats,
            RealmLayoutRecipe layout,
            ICollection<SylvanEncounterPacingValidationIssue> issues)
        {
            var roles = new HashSet<SylvanRealmNodeMaterializationRole>();
            if (beats == null || beats.Count == 0)
            {
                AddIssue(issues, SylvanEncounterPacingValidationIssue.BeatCardinalityInvalid);
                AddIssue(issues, SylvanEncounterPacingValidationIssue.HeartSequenceInvalid);
                return roles;
            }

            var beatIds = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < beats.Count; index++)
            {
                var beat = beats[index];
                if (beat == null)
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.BeatMissing);
                    continue;
                }

                if (!HasStableId(beat.BeatId))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.BeatIdInvalid);
                }
                else if (!beatIds.Add(beat.BeatId))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.BeatIdDuplicate);
                }

                if (!ContainsRole(layout, beat.Role) || !IsPacingRole(beat.Role))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.BeatRoleInvalid);
                }
                else if (!roles.Add(beat.Role))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.BeatRoleDuplicate);
                }

                if (!IsExpectedBeatKind(beat.Role, beat.Kind))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.BeatKindInvalid);
                }

                if (beat.Requirement <= SylvanEncounterPacingRequirement.Unknown
                    || beat.Requirement > SylvanEncounterPacingRequirement.Optional)
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.BeatRequirementInvalid);
                }
            }

            if (beats.Count == 0
                || beats[beats.Count - 1] == null
                || beats[beats.Count - 1].Role != SylvanRealmNodeMaterializationRole.HeartTreeObjective
                || beats[beats.Count - 1].Kind != SylvanEncounterPacingBeatKind.Objective
                || beats[beats.Count - 1].Requirement != SylvanEncounterPacingRequirement.Required)
            {
                AddIssue(issues, SylvanEncounterPacingValidationIssue.HeartSequenceInvalid);
            }

            return roles;
        }

        private static void ValidatePacingRoleCoverage(
            IReadOnlyList<SylvanEncounterPacingBeat> beats,
            RealmLayoutRecipe layout,
            ICollection<SylvanEncounterPacingValidationIssue> issues)
        {
            if (layout == null)
            {
                return;
            }

            var expectedRoleCounts = new Dictionary<SylvanRealmNodeMaterializationRole, int>();
            foreach (var node in layout.Nodes)
            {
                if (node == null || !IsPacingRole(node.MaterializationRole))
                {
                    continue;
                }

                if (!expectedRoleCounts.ContainsKey(node.MaterializationRole))
                {
                    expectedRoleCounts.Add(node.MaterializationRole, 0);
                }
            }

            if (beats != null)
            {
                foreach (var beat in beats)
                {
                    if (beat != null && expectedRoleCounts.ContainsKey(beat.Role))
                    {
                        expectedRoleCounts[beat.Role]++;
                    }
                }
            }

            foreach (var pair in expectedRoleCounts)
            {
                if (pair.Value != 1)
                {
                    AddIssue(
                        issues,
                        SylvanEncounterPacingValidationIssue.PacingRoleCoverageInvalid);
                    return;
                }
            }
        }

        private static void ValidateRequiredHeartSequence(
            IReadOnlyList<SylvanEncounterPacingBeat> beats,
            RealmLayoutRecipe layout,
            ICollection<SylvanEncounterPacingValidationIssue> issues)
        {
            if (beats == null || layout == null)
            {
                return;
            }

            SylvanRealmNodeMaterializationRole? previousRole = null;
            foreach (var beat in beats)
            {
                if (beat == null
                    || beat.Requirement != SylvanEncounterPacingRequirement.Required)
                {
                    continue;
                }

                if (previousRole.HasValue && !HasEdge(layout, previousRole.Value, beat.Role))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.HeartSequenceInvalid);
                    return;
                }

                previousRole = beat.Role;
            }
        }

        private static void ValidateChoices(
            IReadOnlyList<SylvanEncounterPacingChoice> choices,
            RealmLayoutRecipe layout,
            ISet<SylvanRealmNodeMaterializationRole> beatRoles,
            ICollection<SylvanEncounterPacingValidationIssue> issues)
        {
            if (choices == null)
            {
                AddIssue(issues, SylvanEncounterPacingValidationIssue.ChoiceMissing);
                return;
            }

            var choiceIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var choice in choices)
            {
                if (choice == null)
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.ChoiceMissing);
                    continue;
                }

                if (!HasStableId(choice.ChoiceId))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.ChoiceIdInvalid);
                }
                else if (!choiceIds.Add(choice.ChoiceId))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.ChoiceIdDuplicate);
                }

                var hasFirstRoute = choice.FirstRoute.Count >= 2;
                var hasSecondRoute = choice.SecondRoute.Count >= 2;
                if (choice.Kind == SylvanEncounterPacingChoiceKind.OptionalBranch)
                {
                    if (!hasFirstRoute || choice.SecondRoute.Count != 0)
                    {
                        AddIssue(issues, SylvanEncounterPacingValidationIssue.ChoiceRouteInvalid);
                    }
                }
                else if (choice.Kind == SylvanEncounterPacingChoiceKind.RouteChoice)
                {
                    if (!hasFirstRoute || !hasSecondRoute)
                    {
                        AddIssue(issues, SylvanEncounterPacingValidationIssue.ChoiceRouteInvalid);
                    }
                }
                else
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.ChoiceKindInvalid);
                }

                ValidateRoute(choice.FirstRoute, layout, beatRoles, issues);
                ValidateRoute(choice.SecondRoute, layout, beatRoles, issues);
            }
        }

        private static void ValidateRoute(
            IReadOnlyList<SylvanRealmNodeMaterializationRole> route,
            RealmLayoutRecipe layout,
            ISet<SylvanRealmNodeMaterializationRole> beatRoles,
            ICollection<SylvanEncounterPacingValidationIssue> issues)
        {
            if (route == null || route.Count == 0)
            {
                return;
            }

            for (var index = 0; index < route.Count; index++)
            {
                var role = route[index];
                if (!ContainsRole(layout, role)
                    || (role != SylvanRealmNodeMaterializationRole.LandmarkJunction
                        && !beatRoles.Contains(role)))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.ChoiceRoleInvalid);
                }

                if (index > 0 && !HasEdge(layout, route[index - 1], role))
                {
                    AddIssue(issues, SylvanEncounterPacingValidationIssue.ChoiceEdgeInvalid);
                }
            }
        }

        private static bool ContainsRole(
            RealmLayoutRecipe layout,
            SylvanRealmNodeMaterializationRole role)
        {
            if (layout == null)
            {
                return false;
            }

            foreach (var node in layout.Nodes)
            {
                if (node != null && node.MaterializationRole == role)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasEdge(
            RealmLayoutRecipe layout,
            SylvanRealmNodeMaterializationRole firstRole,
            SylvanRealmNodeMaterializationRole secondRole)
        {
            if (layout == null)
            {
                return false;
            }

            foreach (var edge in layout.Edges)
            {
                if (edge == null)
                {
                    continue;
                }

                var fromRole = RoleForNodeId(layout, edge.FromNodeId);
                var toRole = RoleForNodeId(layout, edge.ToNodeId);
                if ((fromRole == firstRole && toRole == secondRole)
                    || (fromRole == secondRole && toRole == firstRole))
                {
                    return true;
                }
            }

            return false;
        }

        private static SylvanRealmNodeMaterializationRole RoleForNodeId(
            RealmLayoutRecipe layout,
            string nodeId)
        {
            foreach (var node in layout.Nodes)
            {
                if (node != null && string.Equals(node.NodeId, nodeId, StringComparison.Ordinal))
                {
                    return node.MaterializationRole;
                }
            }

            return SylvanRealmNodeMaterializationRole.Unknown;
        }

        private static bool IsPacingRole(SylvanRealmNodeMaterializationRole role)
        {
            return role == SylvanRealmNodeMaterializationRole.WolfGroveEncounter
                || role == SylvanRealmNodeMaterializationRole.RootPathHazard
                || role == SylvanRealmNodeMaterializationRole.EntGroveEncounter
                || role == SylvanRealmNodeMaterializationRole.MoonwellRecovery
                || role == SylvanRealmNodeMaterializationRole.HeartTreeObjective;
        }

        private static bool IsExpectedBeatKind(
            SylvanRealmNodeMaterializationRole role,
            SylvanEncounterPacingBeatKind kind)
        {
            if (role == SylvanRealmNodeMaterializationRole.WolfGroveEncounter
                || role == SylvanRealmNodeMaterializationRole.EntGroveEncounter)
            {
                return kind == SylvanEncounterPacingBeatKind.Encounter;
            }

            if (role == SylvanRealmNodeMaterializationRole.RootPathHazard)
            {
                return kind == SylvanEncounterPacingBeatKind.HazardRoute;
            }

            if (role == SylvanRealmNodeMaterializationRole.MoonwellRecovery)
            {
                return kind == SylvanEncounterPacingBeatKind.RecoveryOpportunity;
            }

            return role == SylvanRealmNodeMaterializationRole.HeartTreeObjective
                && kind == SylvanEncounterPacingBeatKind.Objective;
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
            ICollection<SylvanEncounterPacingValidationIssue> issues,
            SylvanEncounterPacingValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }
    }

    /// <summary>
    /// Three fixed pacing recipes for the three exact cached starter layouts.
    /// Consumers deliberately resolve one recipe after resolving a layout.
    /// </summary>
    public static class StarterSylvanEncounterPacing
    {
        public static SylvanEncounterPacingRecipe AncientCrossroads { get; } =
            new SylvanEncounterPacingRecipe(
                StarterSylvanRealmLayouts.AncientCrossroadsId,
                "OPTIONAL WOLF BRANCH • ROOT OR MOONWELL BEFORE ENT",
                new SylvanEncounterPacingBeat[]
                {
                    Beat("ancient.wolf", SylvanRealmNodeMaterializationRole.WolfGroveEncounter, SylvanEncounterPacingBeatKind.Encounter, SylvanEncounterPacingRequirement.Optional),
                    Beat("ancient.root", SylvanRealmNodeMaterializationRole.RootPathHazard, SylvanEncounterPacingBeatKind.HazardRoute, SylvanEncounterPacingRequirement.Optional),
                    Beat("ancient.moonwell", SylvanRealmNodeMaterializationRole.MoonwellRecovery, SylvanEncounterPacingBeatKind.RecoveryOpportunity, SylvanEncounterPacingRequirement.Optional),
                    Beat("ancient.ent", SylvanRealmNodeMaterializationRole.EntGroveEncounter, SylvanEncounterPacingBeatKind.Encounter, SylvanEncounterPacingRequirement.Required),
                    Beat("ancient.heart", SylvanRealmNodeMaterializationRole.HeartTreeObjective, SylvanEncounterPacingBeatKind.Objective, SylvanEncounterPacingRequirement.Required)
                },
                new SylvanEncounterPacingChoice[]
                {
                    OptionalBranch("ancient.wolf-branch", SylvanRealmNodeMaterializationRole.LandmarkJunction, SylvanRealmNodeMaterializationRole.WolfGroveEncounter),
                    RouteChoice("ancient.ent-approach", new[] { SylvanRealmNodeMaterializationRole.LandmarkJunction, SylvanRealmNodeMaterializationRole.RootPathHazard, SylvanRealmNodeMaterializationRole.EntGroveEncounter }, new[] { SylvanRealmNodeMaterializationRole.LandmarkJunction, SylvanRealmNodeMaterializationRole.MoonwellRecovery, SylvanRealmNodeMaterializationRole.EntGroveEncounter })
                });

        public static SylvanEncounterPacingRecipe ForkedCanopy { get; } =
            new SylvanEncounterPacingRecipe(
                StarterSylvanRealmLayouts.ForkedCanopyId,
                "HIGH WOLF AND ROOT RISK • OR MOONWELL TO ENT",
                new SylvanEncounterPacingBeat[]
                {
                    Beat("canopy.wolf", SylvanRealmNodeMaterializationRole.WolfGroveEncounter, SylvanEncounterPacingBeatKind.Encounter, SylvanEncounterPacingRequirement.Optional),
                    Beat("canopy.root", SylvanRealmNodeMaterializationRole.RootPathHazard, SylvanEncounterPacingBeatKind.HazardRoute, SylvanEncounterPacingRequirement.Optional),
                    Beat("canopy.moonwell", SylvanRealmNodeMaterializationRole.MoonwellRecovery, SylvanEncounterPacingBeatKind.RecoveryOpportunity, SylvanEncounterPacingRequirement.Optional),
                    Beat("canopy.ent", SylvanRealmNodeMaterializationRole.EntGroveEncounter, SylvanEncounterPacingBeatKind.Encounter, SylvanEncounterPacingRequirement.Required),
                    Beat("canopy.heart", SylvanRealmNodeMaterializationRole.HeartTreeObjective, SylvanEncounterPacingBeatKind.Objective, SylvanEncounterPacingRequirement.Required)
                },
                new SylvanEncounterPacingChoice[]
                {
                    RouteChoice("canopy.ent-approach", new[] { SylvanRealmNodeMaterializationRole.LandmarkJunction, SylvanRealmNodeMaterializationRole.WolfGroveEncounter, SylvanRealmNodeMaterializationRole.RootPathHazard, SylvanRealmNodeMaterializationRole.EntGroveEncounter }, new[] { SylvanRealmNodeMaterializationRole.LandmarkJunction, SylvanRealmNodeMaterializationRole.MoonwellRecovery, SylvanRealmNodeMaterializationRole.EntGroveEncounter })
                });

        public static SylvanEncounterPacingRecipe SerpentRoots { get; } =
            new SylvanEncounterPacingRecipe(
                StarterSylvanRealmLayouts.SerpentRootsId,
                "WOLF • ROOT • ENT • MOONWELL • HEART",
                new SylvanEncounterPacingBeat[]
                {
                    Beat("serpent.wolf", SylvanRealmNodeMaterializationRole.WolfGroveEncounter, SylvanEncounterPacingBeatKind.Encounter, SylvanEncounterPacingRequirement.Required),
                    Beat("serpent.root", SylvanRealmNodeMaterializationRole.RootPathHazard, SylvanEncounterPacingBeatKind.HazardRoute, SylvanEncounterPacingRequirement.Required),
                    Beat("serpent.ent", SylvanRealmNodeMaterializationRole.EntGroveEncounter, SylvanEncounterPacingBeatKind.Encounter, SylvanEncounterPacingRequirement.Required),
                    Beat("serpent.moonwell", SylvanRealmNodeMaterializationRole.MoonwellRecovery, SylvanEncounterPacingBeatKind.RecoveryOpportunity, SylvanEncounterPacingRequirement.Required),
                    Beat("serpent.heart", SylvanRealmNodeMaterializationRole.HeartTreeObjective, SylvanEncounterPacingBeatKind.Objective, SylvanEncounterPacingRequirement.Required)
                },
                Array.Empty<SylvanEncounterPacingChoice>());

        public static IReadOnlyList<SylvanEncounterPacingRecipe> All { get; } =
            Array.AsReadOnly(new[]
            {
                AncientCrossroads,
                ForkedCanopy,
                SerpentRoots
            });

        private static SylvanEncounterPacingBeat Beat(
            string beatId,
            SylvanRealmNodeMaterializationRole role,
            SylvanEncounterPacingBeatKind kind,
            SylvanEncounterPacingRequirement requirement)
        {
            return new SylvanEncounterPacingBeat(beatId, role, kind, requirement);
        }

        private static SylvanEncounterPacingChoice OptionalBranch(
            string choiceId,
            SylvanRealmNodeMaterializationRole fromRole,
            SylvanRealmNodeMaterializationRole branchRole)
        {
            return new SylvanEncounterPacingChoice(
                choiceId,
                SylvanEncounterPacingChoiceKind.OptionalBranch,
                new[]
                {
                    fromRole,
                    branchRole
                },
                Array.Empty<SylvanRealmNodeMaterializationRole>());
        }

        private static SylvanEncounterPacingChoice RouteChoice(
            string choiceId,
            IReadOnlyList<SylvanRealmNodeMaterializationRole> firstRoute,
            IReadOnlyList<SylvanRealmNodeMaterializationRole> secondRoute)
        {
            return new SylvanEncounterPacingChoice(
                choiceId,
                SylvanEncounterPacingChoiceKind.RouteChoice,
                firstRoute,
                secondRoute);
        }
    }
}
