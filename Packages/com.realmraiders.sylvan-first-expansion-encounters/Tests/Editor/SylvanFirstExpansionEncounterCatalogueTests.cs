using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.SylvanFirstExpansionEncounters.Tests
{
    public sealed class SylvanFirstExpansionEncounterCatalogueTests
    {
        [Test]
        public void FindExact_ReturnsExactOrderedFactsForEachAcceptedLayoutFirstSocket()
        {
            AssertExact(0, StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.west-bough",
                StarterSylvanFirstExpansionEncounters.SylvanWolfArchetypeId, 2,
                "WOLVES HOLD THE WEST BOUGH. MOONWELL RECOVERY AWAITS.");
            AssertExact(1, StarterSylvanRealmLayouts.ForkedCanopyId, "canopy.high-bough",
                StarterSylvanFirstExpansionEncounters.GuardianEntArchetypeId, 1,
                "AN ENT WATCHES THE HIGH BOUGH. MOONWELL RECOVERY AWAITS.");
            AssertExact(2, StarterSylvanRealmLayouts.SerpentRootsId, "serpent.east-burrow",
                StarterSylvanFirstExpansionEncounters.SylvanWolfArchetypeId, 1,
                "A WOLF GUARDS THE EAST BURROW. MOONWELL RECOVERY AWAITS.");
        }

        [Test]
        public void FindExact_FailsClosedForInvalidUnknownAndNonFirstSockets()
        {
            AssertRejected(StarterSylvanFirstExpansionEncounters.FindExact(null, "ancient.west-bough"),
                SylvanFirstExpansionEncounterLookupStatus.LayoutIdInvalid);
            AssertRejected(StarterSylvanFirstExpansionEncounters.FindExact(
                StarterSylvanRealmLayouts.AncientCrossroadsId, "Invalid Socket"),
                SylvanFirstExpansionEncounterLookupStatus.SocketIdInvalid);
            AssertRejected(StarterSylvanFirstExpansionEncounters.FindExact(
                "realmraiders.sylvan-layout.unknown", "socket.one"),
                SylvanFirstExpansionEncounterLookupStatus.LayoutUnknown);
            AssertRejected(StarterSylvanFirstExpansionEncounters.FindExact(
                StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.east-bough"),
                SylvanFirstExpansionEncounterLookupStatus.SocketNotFirstAuthored);
            AssertRejected(StarterSylvanFirstExpansionEncounters.FindExact(
                StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.unknown"),
                SylvanFirstExpansionEncounterLookupStatus.SocketUnknown);
        }

        [Test]
        public void Validate_FailsClosedForDuplicateAndMismatchedEvidence()
        {
            var duplicate = new List<SylvanFirstExpansionEncounterRecipe>
            {
                StarterSylvanFirstExpansionEncounters.AncientCrossroads,
                new SylvanFirstExpansionEncounterRecipe(
                    StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.east-bough",
                    StarterSylvanFirstExpansionEncounters.SylvanWolfArchetypeId, 1,
                    StarterSylvanFirstExpansionEncounters.MoonwellExpansionNodeRecoveryContentId, "CUE"),
                StarterSylvanFirstExpansionEncounters.SerpentRoots
            };

            var validation = StarterSylvanFirstExpansionEncounters.Validate(duplicate);

            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionEncounterValidationIssue.LayoutIdDuplicate));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionEncounterValidationIssue.FirstSocketMismatch));
        }

        [Test]
        public void Validate_RejectsChangedExactArchetypeCountRecoveryAndCueFacts()
        {
            var changed = new List<SylvanFirstExpansionEncounterRecipe>
            {
                new SylvanFirstExpansionEncounterRecipe(
                    StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.west-bough",
                    StarterSylvanFirstExpansionEncounters.GuardianEntArchetypeId, 2,
                    "realmraiders.node.other", "WOLVES HOLD THE WEST BOUGH. MOONWELL RECOVERY AWAITS."),
                new SylvanFirstExpansionEncounterRecipe(
                    StarterSylvanRealmLayouts.ForkedCanopyId, "canopy.high-bough",
                    StarterSylvanFirstExpansionEncounters.GuardianEntArchetypeId, 2,
                    StarterSylvanFirstExpansionEncounters.MoonwellExpansionNodeRecoveryContentId,
                    "AN ENT WATCHES THE HIGH BOUGH. MOONWELL RECOVERY AWAITS."),
                new SylvanFirstExpansionEncounterRecipe(
                    StarterSylvanRealmLayouts.SerpentRootsId, "serpent.east-burrow",
                    StarterSylvanFirstExpansionEncounters.SylvanWolfArchetypeId, 1,
                    StarterSylvanFirstExpansionEncounters.MoonwellExpansionNodeRecoveryContentId, "CHANGED CUE")
            };

            var validation = StarterSylvanFirstExpansionEncounters.Validate(changed);

            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionEncounterValidationIssue.EncounterArchetypeMismatch));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionEncounterValidationIssue.EncounterCountMismatch));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionEncounterValidationIssue.ExpansionNodeRecoveryContentIdInvalid));
            Assert.That(validation.Issues, Does.Contain(SylvanFirstExpansionEncounterValidationIssue.CueMismatch));

            changed[0] = new SylvanFirstExpansionEncounterRecipe(
                StarterSylvanRealmLayouts.AncientCrossroadsId, "ancient.west-bough",
                StarterSylvanFirstExpansionEncounters.SylvanWolfArchetypeId, 2, null,
                "WOLVES HOLD THE WEST BOUGH. MOONWELL RECOVERY AWAITS.");
            var missingRecovery = StarterSylvanFirstExpansionEncounters.Validate(changed);

            Assert.That(missingRecovery.IsValid, Is.False);
            Assert.That(missingRecovery.Issues, Does.Contain(
                SylvanFirstExpansionEncounterValidationIssue.ExpansionNodeRecoveryContentIdInvalid));
        }

        [Test]
        public void FindExact_IsDeterministicAndDoesNotMutateCatalogue()
        {
            var first = StarterSylvanFirstExpansionEncounters.FindExact(
                StarterSylvanRealmLayouts.ForkedCanopyId, "canopy.high-bough");
            var second = StarterSylvanFirstExpansionEncounters.FindExact(
                StarterSylvanRealmLayouts.ForkedCanopyId, "canopy.high-bough");

            Assert.That(second.Status, Is.EqualTo(first.Status));
            Assert.That(second.Recipe, Is.SameAs(first.Recipe));
            Assert.That(StarterSylvanFirstExpansionEncounters.All.Count, Is.EqualTo(3));
            Assert.That(StarterSylvanFirstExpansionEncounters.Validate(
                StarterSylvanFirstExpansionEncounters.All).IsValid, Is.True);
        }

        private static void AssertRejected(SylvanFirstExpansionEncounterLookupResult result,
            SylvanFirstExpansionEncounterLookupStatus expectedStatus)
        {
            Assert.That(result.Status, Is.EqualTo(expectedStatus));
            Assert.That(result.Found, Is.False);
            Assert.That(result.Recipe, Is.Null);
        }

        private static void AssertExact(int expectedIndex, string layoutId, string socketId,
            string archetypeId, int encounterCount, string cue)
        {
            var layout = StarterSylvanRealmLayouts.All[expectedIndex];
            var result = StarterSylvanFirstExpansionEncounters.FindExact(layoutId, socketId);

            Assert.That(layout.LayoutId, Is.EqualTo(layoutId));
            Assert.That(layout.ExpansionSockets[0].SocketId, Is.EqualTo(socketId));
            Assert.That(result.Found, Is.True);
            Assert.That(result.Recipe, Is.SameAs(StarterSylvanFirstExpansionEncounters.All[expectedIndex]));
            Assert.That(result.Recipe.EncounterArchetypeId, Is.EqualTo(archetypeId));
            Assert.That(result.Recipe.EncounterCount, Is.EqualTo(encounterCount));
            Assert.That(result.Recipe.ExpansionNodeRecoveryContentId,
                Is.EqualTo(StarterSylvanFirstExpansionEncounters.MoonwellExpansionNodeRecoveryContentId));
            Assert.That(result.Recipe.Cue, Is.EqualTo(cue));
        }
    }
}
