using System;
using System.Collections.Generic;
using RealmRaiders.Modules.InfernalFirstExpansionContent;

namespace RealmRaiders.Modules.InfernalFirstExpansionTactics
{
    public enum TacticTrigger
    {
        SceneStart,
        FlameActivated,
        InvaderInRange
    }

    public enum TacticRole
    {
        LeftFlank,
        RightFlank,
        TrapGuard,
        HeavyInterceptor
    }

    public enum TacticLookupStatus
    {
        Found,
        LayoutIdInvalid,
        SocketIdInvalid,
        LayoutUnknown,
        SocketUnknown,
        SocketNotFirstAuthored,
        CatalogueInvalid
    }

    public enum TacticValidationIssue
    {
        RecipeMissing,
        CatalogueCardinalityInvalid,
        CatalogueOrderMismatch,
        LayoutIdInvalid,
        SocketIdInvalid,
        SourceNodeIdInvalid,
        SiteIdInvalid,
        TacticIdInvalid,
        LayoutIdDuplicate,
        SocketIdDuplicate,
        SiteIdDuplicate,
        TacticIdDuplicate,
        LayoutUnknown,
        SocketUnknown,
        FirstSocketMismatch,
        ContentCatalogueInvalid,
        SourceNodeMismatch,
        SiteIdMismatch,
        TacticIdMismatch,
        TriggerInvalid,
        TriggerMismatch,
        CueInvalid,
        CueMismatch,
        ActorCardinalityInvalid,
        ActorMissing,
        ActorIndexDuplicate,
        ActorIndexOutOfRange,
        ActorOrderMismatch,
        ActorContentMismatch,
        ActorRoleInvalid,
        ActorRoleMismatch,
        ActorDelayInvalid,
        ActorDelayMismatch,
        TriggerRadiusInvalid,
        TriggerRadiusMismatch,
        AbilityMismatch,
        HazardMissing,
        HazardUnexpected,
        HazardContentMismatch,
        HazardAutomaticMismatch,
        HazardNonBlockingMismatch,
        HazardBypassableMismatch
    }

    public sealed class TacticActor
    {
        public TacticActor(
            int actorIndex,
            string contentId,
            TacticRole role,
            float responseDelaySeconds)
        {
            ActorIndex = actorIndex;
            ContentId = contentId;
            Role = role;
            ResponseDelaySeconds = responseDelaySeconds;
        }

        public int ActorIndex { get; }
        public string ContentId { get; }
        public TacticRole Role { get; }
        public float ResponseDelaySeconds { get; }
    }

    public sealed class TacticHazardDependency
    {
        public TacticHazardDependency(
            string contentId,
            bool automatic,
            bool nonBlocking,
            bool bypassable)
        {
            ContentId = contentId;
            Automatic = automatic;
            NonBlocking = nonBlocking;
            Bypassable = bypassable;
        }

        public string ContentId { get; }
        public bool Automatic { get; }
        public bool NonBlocking { get; }
        public bool Bypassable { get; }
    }

    public sealed class TacticRecipe
    {
        public TacticRecipe(
            string layoutId,
            string socketId,
            string sourceNodeId,
            string siteId,
            string tacticId,
            TacticTrigger trigger,
            string cue,
            IReadOnlyList<TacticActor> actors,
            string preferredAbilityDisplayName,
            float triggerRadiusMetres,
            TacticHazardDependency hazard)
        {
            LayoutId = layoutId;
            SocketId = socketId;
            SourceNodeId = sourceNodeId;
            SiteId = siteId;
            TacticId = tacticId;
            Trigger = trigger;
            Cue = cue;
            Actors = Snapshot(actors);
            PreferredAbilityDisplayName = preferredAbilityDisplayName;
            TriggerRadiusMetres = triggerRadiusMetres;
            Hazard = hazard;
        }

        public string LayoutId { get; }
        public string SocketId { get; }
        public string SourceNodeId { get; }
        public string SiteId { get; }
        public string TacticId { get; }
        public TacticTrigger Trigger { get; }
        public string Cue { get; }
        public IReadOnlyList<TacticActor> Actors { get; }
        public string PreferredAbilityDisplayName { get; }
        public float TriggerRadiusMetres { get; }
        public TacticHazardDependency Hazard { get; }

        private static IReadOnlyList<TacticActor> Snapshot(IReadOnlyList<TacticActor> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<TacticActor>());
            }

            var copy = new TacticActor[source.Count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class TacticValidationResult
    {
        internal TacticValidationResult(IReadOnlyList<TacticValidationIssue> issues)
        {
            Issues = Snapshot(issues);
        }

        public IReadOnlyList<TacticValidationIssue> Issues { get; }
        public bool IsValid => Issues.Count == 0;

        private static IReadOnlyList<TacticValidationIssue> Snapshot(
            IReadOnlyList<TacticValidationIssue> source)
        {
            if (source == null)
            {
                return Array.AsReadOnly(Array.Empty<TacticValidationIssue>());
            }

            var copy = new TacticValidationIssue[source.Count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = source[index];
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class TacticLookupResult
    {
        internal TacticLookupResult(TacticLookupStatus status, TacticRecipe recipe)
        {
            Status = status;
            Recipe = recipe;
        }

        public TacticLookupStatus Status { get; }
        public TacticRecipe Recipe { get; }
        public bool Found => Status == TacticLookupStatus.Found && Recipe != null;
    }

    public static class StarterInfernalFirstExpansionTactics
    {
        private const int AuthoredRecipeCount = 3;
        private const string AshenLayoutId = "realmraiders.infernal-defense.ashen-spur";
        private const string CinderLayoutId = "realmraiders.infernal-defense.cinder-fork";
        private const string EmberLayoutId = "realmraiders.infernal-defense.ember-circuit";
        private const string HellhoundContentId = "realmraiders.hellhound";
        private const string BruteContentId = "realmraiders.infernal-brute";
        private const string FlameTrapContentId = "realmraiders.infernal.flame-trap";

        private static readonly AuthoredDefinition[] Definitions =
        {
            new AuthoredDefinition(
                AshenLayoutId,
                "ashen.west-vent",
                "ashen.hellhound",
                "realmraiders.infernal-expansion.ashen-pack-vent",
                "pack-pincer",
                TacticTrigger.SceneStart,
                "PACK INTERCEPT",
                null,
                0f,
                null,
                new AuthoredActor(0, HellhoundContentId, TacticRole.LeftFlank, 0f),
                new AuthoredActor(1, HellhoundContentId, TacticRole.RightFlank, 0.25f)),
            new AuthoredDefinition(
                CinderLayoutId,
                "cinder.north-vent",
                "cinder.hellhound",
                "realmraiders.infernal-expansion.cinder-snare-vent",
                "snare-counterattack",
                TacticTrigger.FlameActivated,
                "SNARE COUNTER",
                null,
                0f,
                new AuthoredHazard(FlameTrapContentId, true, true, true),
                new AuthoredActor(0, HellhoundContentId, TacticRole.TrapGuard, 0.2f)),
            new AuthoredDefinition(
                EmberLayoutId,
                "ember.west-vent",
                "ember.hellhound",
                "realmraiders.infernal-expansion.brute-kiln-vent",
                "brute-intercept",
                TacticTrigger.InvaderInRange,
                "BRUTE INTERCEPT",
                "Charge",
                7f,
                null,
                new AuthoredActor(0, BruteContentId, TacticRole.HeavyInterceptor, 0f))
        };

        static StarterInfernalFirstExpansionTactics()
        {
            AshenPackVent = Create(Definitions[0]);
            CinderSnareVent = Create(Definitions[1]);
            BruteKilnVent = Create(Definitions[2]);
            All = Array.AsReadOnly(
                new[] { AshenPackVent, CinderSnareVent, BruteKilnVent });
        }

        public static TacticRecipe AshenPackVent { get; }
        public static TacticRecipe CinderSnareVent { get; }
        public static TacticRecipe BruteKilnVent { get; }
        public static IReadOnlyList<TacticRecipe> All { get; }

        public static TacticLookupResult FindExact(string layoutId, string socketId)
        {
            if (!IsStableId(layoutId))
            {
                return Reject(TacticLookupStatus.LayoutIdInvalid);
            }

            if (!IsStableId(socketId))
            {
                return Reject(TacticLookupStatus.SocketIdInvalid);
            }

            if (!Validate(All).IsValid)
            {
                return Reject(TacticLookupStatus.CatalogueInvalid);
            }

            var content = StarterInfernalFirstExpansionContent.FindExact(layoutId, socketId);
            if (!content.Found)
            {
                return Reject(MapLookupStatus(content.Status));
            }

            for (var index = 0; index < All.Count; index++)
            {
                var candidate = All[index];
                if (SameIdentity(candidate, layoutId, socketId))
                {
                    return new TacticLookupResult(TacticLookupStatus.Found, candidate);
                }
            }

            return Reject(TacticLookupStatus.CatalogueInvalid);
        }

        public static TacticValidationResult Validate(IReadOnlyList<TacticRecipe> recipes)
        {
            var issues = new List<TacticValidationIssue>();
            if (recipes == null || recipes.Count != AuthoredRecipeCount)
            {
                Add(issues, TacticValidationIssue.CatalogueCardinalityInvalid);
            }

            if (recipes == null)
            {
                return new TacticValidationResult(issues);
            }

            var layouts = new HashSet<string>(StringComparer.Ordinal);
            var sockets = new HashSet<string>(StringComparer.Ordinal);
            var sites = new HashSet<string>(StringComparer.Ordinal);
            var tactics = new HashSet<string>(StringComparer.Ordinal);
            var hasExactAuthoredMultiset = HasExactAuthoredRecipeMultiset(recipes);

            for (var index = 0; index < recipes.Count; index++)
            {
                var recipe = recipes[index];
                ValidateRecipe(recipe, layouts, sockets, sites, tactics, issues);
            }

            if (hasExactAuthoredMultiset && !HasAcceptedCatalogueOrder(recipes))
            {
                Add(issues, TacticValidationIssue.CatalogueOrderMismatch);
            }

            return new TacticValidationResult(issues);
        }

        private static void ValidateRecipe(
            TacticRecipe recipe,
            ISet<string> layouts,
            ISet<string> sockets,
            ISet<string> sites,
            ISet<string> tactics,
            ICollection<TacticValidationIssue> issues)
        {
            if (recipe == null)
            {
                Add(issues, TacticValidationIssue.RecipeMissing);
                return;
            }

            ValidateStableId(recipe.LayoutId, TacticValidationIssue.LayoutIdInvalid, issues);
            ValidateStableId(recipe.SocketId, TacticValidationIssue.SocketIdInvalid, issues);
            ValidateStableId(recipe.SourceNodeId, TacticValidationIssue.SourceNodeIdInvalid, issues);
            ValidateStableId(recipe.SiteId, TacticValidationIssue.SiteIdInvalid, issues);
            ValidateStableId(recipe.TacticId, TacticValidationIssue.TacticIdInvalid, issues);
            AddDuplicate(recipe.LayoutId, layouts, TacticValidationIssue.LayoutIdDuplicate, issues);
            AddDuplicate(recipe.SocketId, sockets, TacticValidationIssue.SocketIdDuplicate, issues);
            AddDuplicate(recipe.SiteId, sites, TacticValidationIssue.SiteIdDuplicate, issues);
            AddDuplicate(recipe.TacticId, tactics, TacticValidationIssue.TacticIdDuplicate, issues);

            InfernalFirstExpansionContentRecipe contentRecipe = null;
            if (IsStableId(recipe.LayoutId) && IsStableId(recipe.SocketId))
            {
                var content = StarterInfernalFirstExpansionContent.FindExact(
                    recipe.LayoutId,
                    recipe.SocketId);
                if (content.Found)
                {
                    contentRecipe = content.Recipe;
                }
                else
                {
                    AddContentLookupIssue(content.Status, issues);
                }
            }

            var definition = Definition(recipe.LayoutId, recipe.SocketId);
            if (contentRecipe != null)
            {
                if (!Same(recipe.SourceNodeId, contentRecipe.SourceNodeId))
                {
                    Add(issues, TacticValidationIssue.SourceNodeMismatch);
                }

                if (!Same(recipe.SiteId, contentRecipe.SiteId))
                {
                    Add(issues, TacticValidationIssue.SiteIdMismatch);
                }
            }

            ValidateExactTacticalFacts(recipe, definition, issues);
            ValidateActors(recipe.Actors, definition, contentRecipe, issues);
            ValidateHazard(recipe.Hazard, definition, contentRecipe, issues);
        }

        private static void ValidateExactTacticalFacts(
            TacticRecipe recipe,
            AuthoredDefinition definition,
            ICollection<TacticValidationIssue> issues)
        {
            if (!Enum.IsDefined(typeof(TacticTrigger), recipe.Trigger))
            {
                Add(issues, TacticValidationIssue.TriggerInvalid);
            }

            if (string.IsNullOrWhiteSpace(recipe.Cue))
            {
                Add(issues, TacticValidationIssue.CueInvalid);
            }

            if (!IsFinite(recipe.TriggerRadiusMetres) || recipe.TriggerRadiusMetres < 0f)
            {
                Add(issues, TacticValidationIssue.TriggerRadiusInvalid);
            }

            if (definition == null)
            {
                return;
            }

            if (!Same(recipe.SourceNodeId, definition.SourceNodeId))
            {
                Add(issues, TacticValidationIssue.SourceNodeMismatch);
            }

            if (!Same(recipe.SiteId, definition.SiteId))
            {
                Add(issues, TacticValidationIssue.SiteIdMismatch);
            }

            if (!Same(recipe.TacticId, definition.TacticId))
            {
                Add(issues, TacticValidationIssue.TacticIdMismatch);
            }

            if (Enum.IsDefined(typeof(TacticTrigger), recipe.Trigger)
                && recipe.Trigger != definition.Trigger)
            {
                Add(issues, TacticValidationIssue.TriggerMismatch);
            }

            if (!Same(recipe.Cue, definition.Cue))
            {
                Add(issues, TacticValidationIssue.CueMismatch);
            }

            if (IsFinite(recipe.TriggerRadiusMetres)
                && recipe.TriggerRadiusMetres >= 0f
                && recipe.TriggerRadiusMetres != definition.TriggerRadiusMetres)
            {
                Add(issues, TacticValidationIssue.TriggerRadiusMismatch);
            }

            if (!Same(recipe.PreferredAbilityDisplayName, definition.Ability))
            {
                Add(issues, TacticValidationIssue.AbilityMismatch);
            }
        }

        private static void ValidateActors(
            IReadOnlyList<TacticActor> actors,
            AuthoredDefinition definition,
            InfernalFirstExpansionContentRecipe content,
            ICollection<TacticValidationIssue> issues)
        {
            var expectedCount = definition == null ? -1 : definition.Actors.Length;
            if (actors == null || expectedCount < 0 || actors.Count != expectedCount)
            {
                Add(issues, TacticValidationIssue.ActorCardinalityInvalid);
            }

            if (actors == null)
            {
                return;
            }

            var indices = new HashSet<int>();
            for (var position = 0; position < actors.Count; position++)
            {
                var actor = actors[position];
                if (actor == null)
                {
                    Add(issues, TacticValidationIssue.ActorMissing);
                    continue;
                }

                if (!indices.Add(actor.ActorIndex))
                {
                    Add(issues, TacticValidationIssue.ActorIndexDuplicate);
                }

                var indexInDefinition = definition != null
                    && actor.ActorIndex >= 0
                    && actor.ActorIndex < definition.Actors.Length;
                var indexInContent = content != null
                    && actor.ActorIndex >= 0
                    && actor.ActorIndex < content.Hostiles.Count;
                if (!indexInDefinition || !indexInContent)
                {
                    Add(issues, TacticValidationIssue.ActorIndexOutOfRange);
                }
                else
                {
                    var expected = definition.Actors[actor.ActorIndex];
                    if (!Same(actor.ContentId, expected.ContentId)
                        || !Same(actor.ContentId, content.Hostiles[actor.ActorIndex].ContentId))
                    {
                        Add(issues, TacticValidationIssue.ActorContentMismatch);
                    }

                    if (Enum.IsDefined(typeof(TacticRole), actor.Role)
                        && actor.Role != expected.Role)
                    {
                        Add(issues, TacticValidationIssue.ActorRoleMismatch);
                    }

                    if (IsFinite(actor.ResponseDelaySeconds)
                        && actor.ResponseDelaySeconds >= 0f
                        && actor.ResponseDelaySeconds <= 1f
                        && actor.ResponseDelaySeconds != expected.ResponseDelaySeconds)
                    {
                        Add(issues, TacticValidationIssue.ActorDelayMismatch);
                    }
                }

                if (!Enum.IsDefined(typeof(TacticRole), actor.Role))
                {
                    Add(issues, TacticValidationIssue.ActorRoleInvalid);
                }

                if (!IsFinite(actor.ResponseDelaySeconds)
                    || actor.ResponseDelaySeconds < 0f
                    || actor.ResponseDelaySeconds > 1f)
                {
                    Add(issues, TacticValidationIssue.ActorDelayInvalid);
                }
            }

            if (definition != null
                && content != null
                && HasExactAuthoredActorMultiset(actors, definition, content)
                && !HasAcceptedActorOrder(actors))
            {
                Add(issues, TacticValidationIssue.ActorOrderMismatch);
            }
        }

        private static void ValidateHazard(
            TacticHazardDependency hazard,
            AuthoredDefinition definition,
            InfernalFirstExpansionContentRecipe content,
            ICollection<TacticValidationIssue> issues)
        {
            var expected = definition == null ? null : definition.Hazard;
            var contentHazard = content == null ? null : content.Hazard;

            if (expected == null)
            {
                if (hazard != null)
                {
                    Add(issues, TacticValidationIssue.HazardUnexpected);
                }

                return;
            }

            if (hazard == null)
            {
                Add(issues, TacticValidationIssue.HazardMissing);
                return;
            }

            if (contentHazard == null
                || !Same(hazard.ContentId, expected.ContentId)
                || !Same(hazard.ContentId, contentHazard.ContentId))
            {
                Add(issues, TacticValidationIssue.HazardContentMismatch);
            }

            if (hazard.Automatic != expected.Automatic)
            {
                Add(issues, TacticValidationIssue.HazardAutomaticMismatch);
            }

            if (contentHazard == null
                || hazard.NonBlocking != expected.NonBlocking
                || hazard.NonBlocking != contentHazard.IsNonBlocking)
            {
                Add(issues, TacticValidationIssue.HazardNonBlockingMismatch);
            }

            if (contentHazard == null
                || hazard.Bypassable != expected.Bypassable
                || hazard.Bypassable != contentHazard.IsBypassable)
            {
                Add(issues, TacticValidationIssue.HazardBypassableMismatch);
            }
        }

        private static TacticRecipe Create(AuthoredDefinition definition)
        {
            var actors = new TacticActor[definition.Actors.Length];
            for (var index = 0; index < actors.Length; index++)
            {
                var actor = definition.Actors[index];
                actors[index] = new TacticActor(
                    actor.ActorIndex,
                    actor.ContentId,
                    actor.Role,
                    actor.ResponseDelaySeconds);
            }

            TacticHazardDependency hazard = null;
            if (definition.Hazard != null)
            {
                hazard = new TacticHazardDependency(
                    definition.Hazard.ContentId,
                    definition.Hazard.Automatic,
                    definition.Hazard.NonBlocking,
                    definition.Hazard.Bypassable);
            }

            return new TacticRecipe(
                definition.LayoutId,
                definition.SocketId,
                definition.SourceNodeId,
                definition.SiteId,
                definition.TacticId,
                definition.Trigger,
                definition.Cue,
                actors,
                definition.Ability,
                definition.TriggerRadiusMetres,
                hazard);
        }

        private static bool HasExactAuthoredRecipeMultiset(
            IReadOnlyList<TacticRecipe> recipes)
        {
            if (recipes == null || recipes.Count != Definitions.Length)
            {
                return false;
            }

            var seen = new bool[Definitions.Length];
            for (var index = 0; index < recipes.Count; index++)
            {
                var recipe = recipes[index];
                var definitionIndex = DefinitionIndex(recipe);
                if (definitionIndex < 0
                    || seen[definitionIndex]
                    || !MatchesExactAuthoredRecipe(recipe, Definitions[definitionIndex]))
                {
                    return false;
                }

                seen[definitionIndex] = true;
            }

            return true;
        }

        private static bool MatchesExactAuthoredRecipe(
            TacticRecipe recipe,
            AuthoredDefinition definition)
        {
            if (recipe == null
                || !Same(recipe.LayoutId, definition.LayoutId)
                || !Same(recipe.SocketId, definition.SocketId)
                || !Same(recipe.SourceNodeId, definition.SourceNodeId)
                || !Same(recipe.SiteId, definition.SiteId)
                || !Same(recipe.TacticId, definition.TacticId)
                || recipe.Trigger != definition.Trigger
                || !Same(recipe.Cue, definition.Cue)
                || !Same(recipe.PreferredAbilityDisplayName, definition.Ability)
                || recipe.TriggerRadiusMetres != definition.TriggerRadiusMetres)
            {
                return false;
            }

            var content = StarterInfernalFirstExpansionContent.FindExact(
                recipe.LayoutId,
                recipe.SocketId);
            if (!content.Found
                || !Same(recipe.SourceNodeId, content.Recipe.SourceNodeId)
                || !Same(recipe.SiteId, content.Recipe.SiteId)
                || !HasExactAuthoredActorOrder(recipe.Actors, definition, content.Recipe))
            {
                return false;
            }

            return MatchesExactHazard(recipe.Hazard, definition.Hazard, content.Recipe.Hazard);
        }

        private static bool HasExactAuthoredActorOrder(
            IReadOnlyList<TacticActor> actors,
            AuthoredDefinition definition,
            InfernalFirstExpansionContentRecipe content)
        {
            if (actors == null || actors.Count != definition.Actors.Length)
            {
                return false;
            }

            for (var index = 0; index < actors.Count; index++)
            {
                if (!MatchesExactActor(actors[index], definition.Actors[index], content))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasExactAuthoredActorMultiset(
            IReadOnlyList<TacticActor> actors,
            AuthoredDefinition definition,
            InfernalFirstExpansionContentRecipe content)
        {
            if (actors == null || actors.Count != definition.Actors.Length)
            {
                return false;
            }

            var seen = new bool[definition.Actors.Length];
            for (var position = 0; position < actors.Count; position++)
            {
                var actor = actors[position];
                if (actor == null
                    || actor.ActorIndex < 0
                    || actor.ActorIndex >= definition.Actors.Length
                    || seen[actor.ActorIndex]
                    || !MatchesExactActor(
                        actor,
                        definition.Actors[actor.ActorIndex],
                        content))
                {
                    return false;
                }

                seen[actor.ActorIndex] = true;
            }

            return true;
        }

        private static bool MatchesExactActor(
            TacticActor actor,
            AuthoredActor expected,
            InfernalFirstExpansionContentRecipe content)
        {
            return actor != null
                && actor.ActorIndex == expected.ActorIndex
                && actor.ActorIndex >= 0
                && actor.ActorIndex < content.Hostiles.Count
                && Same(actor.ContentId, expected.ContentId)
                && Same(actor.ContentId, content.Hostiles[actor.ActorIndex].ContentId)
                && actor.Role == expected.Role
                && actor.ResponseDelaySeconds == expected.ResponseDelaySeconds;
        }

        private static bool MatchesExactHazard(
            TacticHazardDependency hazard,
            AuthoredHazard expected,
            InfernalExpansionHazardPlacement content)
        {
            if (expected == null || content == null)
            {
                return hazard == null && expected == null && content == null;
            }

            return hazard != null
                && Same(hazard.ContentId, expected.ContentId)
                && Same(hazard.ContentId, content.ContentId)
                && hazard.Automatic == expected.Automatic
                && hazard.NonBlocking == expected.NonBlocking
                && hazard.NonBlocking == content.IsNonBlocking
                && hazard.Bypassable == expected.Bypassable
                && hazard.Bypassable == content.IsBypassable;
        }

        private static bool HasAcceptedCatalogueOrder(IReadOnlyList<TacticRecipe> recipes)
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

        private static bool HasAcceptedActorOrder(IReadOnlyList<TacticActor> actors)
        {
            for (var position = 0; position < actors.Count; position++)
            {
                if (actors[position].ActorIndex != position)
                {
                    return false;
                }
            }

            return true;
        }

        private static AuthoredDefinition Definition(string layoutId, string socketId)
        {
            var index = DefinitionIndex(layoutId, socketId);
            return index < 0 ? null : Definitions[index];
        }

        private static int DefinitionIndex(TacticRecipe recipe)
        {
            return recipe == null ? -1 : DefinitionIndex(recipe.LayoutId, recipe.SocketId);
        }

        private static int DefinitionIndex(string layoutId, string socketId)
        {
            for (var index = 0; index < Definitions.Length; index++)
            {
                var candidate = Definitions[index];
                if (Same(candidate.LayoutId, layoutId) && Same(candidate.SocketId, socketId))
                {
                    return index;
                }
            }

            return -1;
        }

        private static TacticLookupStatus MapLookupStatus(
            InfernalFirstExpansionContentLookupStatus status)
        {
            switch (status)
            {
                case InfernalFirstExpansionContentLookupStatus.LayoutIdInvalid:
                    return TacticLookupStatus.LayoutIdInvalid;
                case InfernalFirstExpansionContentLookupStatus.SocketIdInvalid:
                    return TacticLookupStatus.SocketIdInvalid;
                case InfernalFirstExpansionContentLookupStatus.LayoutUnknown:
                    return TacticLookupStatus.LayoutUnknown;
                case InfernalFirstExpansionContentLookupStatus.SocketUnknown:
                    return TacticLookupStatus.SocketUnknown;
                case InfernalFirstExpansionContentLookupStatus.SocketNotFirstAuthored:
                    return TacticLookupStatus.SocketNotFirstAuthored;
                default:
                    return TacticLookupStatus.CatalogueInvalid;
            }
        }

        private static void AddContentLookupIssue(
            InfernalFirstExpansionContentLookupStatus status,
            ICollection<TacticValidationIssue> issues)
        {
            switch (status)
            {
                case InfernalFirstExpansionContentLookupStatus.LayoutUnknown:
                    Add(issues, TacticValidationIssue.LayoutUnknown);
                    break;
                case InfernalFirstExpansionContentLookupStatus.SocketUnknown:
                    Add(issues, TacticValidationIssue.SocketUnknown);
                    break;
                case InfernalFirstExpansionContentLookupStatus.SocketNotFirstAuthored:
                    Add(issues, TacticValidationIssue.FirstSocketMismatch);
                    break;
                case InfernalFirstExpansionContentLookupStatus.LayoutIdInvalid:
                    Add(issues, TacticValidationIssue.LayoutIdInvalid);
                    break;
                case InfernalFirstExpansionContentLookupStatus.SocketIdInvalid:
                    Add(issues, TacticValidationIssue.SocketIdInvalid);
                    break;
                default:
                    Add(issues, TacticValidationIssue.ContentCatalogueInvalid);
                    break;
            }
        }

        private static void ValidateStableId(
            string value,
            TacticValidationIssue issue,
            ICollection<TacticValidationIssue> issues)
        {
            if (!IsStableId(value))
            {
                Add(issues, issue);
            }
        }

        private static void AddDuplicate(
            string value,
            ISet<string> values,
            TacticValidationIssue issue,
            ICollection<TacticValidationIssue> issues)
        {
            if (IsStableId(value) && !values.Add(value))
            {
                Add(issues, issue);
            }
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

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool SameIdentity(
            TacticRecipe recipe,
            string layoutId,
            string socketId)
        {
            return recipe != null
                && Same(recipe.LayoutId, layoutId)
                && Same(recipe.SocketId, socketId);
        }

        private static bool Same(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        private static void Add(
            ICollection<TacticValidationIssue> issues,
            TacticValidationIssue issue)
        {
            if (!issues.Contains(issue))
            {
                issues.Add(issue);
            }
        }

        private static TacticLookupResult Reject(TacticLookupStatus status)
        {
            return new TacticLookupResult(status, null);
        }

        private sealed class AuthoredDefinition
        {
            public AuthoredDefinition(
                string layoutId,
                string socketId,
                string sourceNodeId,
                string siteId,
                string tacticId,
                TacticTrigger trigger,
                string cue,
                string ability,
                float triggerRadiusMetres,
                AuthoredHazard hazard,
                params AuthoredActor[] actors)
            {
                LayoutId = layoutId;
                SocketId = socketId;
                SourceNodeId = sourceNodeId;
                SiteId = siteId;
                TacticId = tacticId;
                Trigger = trigger;
                Cue = cue;
                Ability = ability;
                TriggerRadiusMetres = triggerRadiusMetres;
                Hazard = hazard;
                Actors = actors;
            }

            public string LayoutId { get; }
            public string SocketId { get; }
            public string SourceNodeId { get; }
            public string SiteId { get; }
            public string TacticId { get; }
            public TacticTrigger Trigger { get; }
            public string Cue { get; }
            public string Ability { get; }
            public float TriggerRadiusMetres { get; }
            public AuthoredHazard Hazard { get; }
            public AuthoredActor[] Actors { get; }
        }

        private sealed class AuthoredActor
        {
            public AuthoredActor(
                int actorIndex,
                string contentId,
                TacticRole role,
                float responseDelaySeconds)
            {
                ActorIndex = actorIndex;
                ContentId = contentId;
                Role = role;
                ResponseDelaySeconds = responseDelaySeconds;
            }

            public int ActorIndex { get; }
            public string ContentId { get; }
            public TacticRole Role { get; }
            public float ResponseDelaySeconds { get; }
        }

        private sealed class AuthoredHazard
        {
            public AuthoredHazard(
                string contentId,
                bool automatic,
                bool nonBlocking,
                bool bypassable)
            {
                ContentId = contentId;
                Automatic = automatic;
                NonBlocking = nonBlocking;
                Bypassable = bypassable;
            }

            public string ContentId { get; }
            public bool Automatic { get; }
            public bool NonBlocking { get; }
            public bool Bypassable { get; }
        }
    }
}
