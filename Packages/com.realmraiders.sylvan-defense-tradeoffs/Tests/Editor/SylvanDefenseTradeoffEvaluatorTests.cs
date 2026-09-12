using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.SylvanDefenseTradeoffs.Tests
{
    public sealed class SylvanDefenseTradeoffEvaluatorTests
    {
        [Test]
        public void PackPressureReturnsTheExactCachedFactForTwoWolves()
        {
            var result = SylvanDefenseTradeoffEvaluator.Evaluate(
                new SylvanDefenseRosterSummary(2, 1, 1, 0));

            Assert.That(result.HasTradeoff, Is.True);
            Assert.That(result.Tradeoff, Is.SameAs(StarterSylvanDefenseTradeoffs.PackPressure));
            Assert.That(result.Issues, Is.Empty);
            Assert.That(StarterSylvanDefenseTradeoffs.PackPressureId, Is.EqualTo("PACK_PRESSURE"));
            Assert.That(result.Tradeoff.TradeoffId, Is.EqualTo(
                StarterSylvanDefenseTradeoffs.PackPressureId));
            Assert.That(result.Tradeoff.DisplayName, Is.EqualTo("Pack Pressure"));
            Assert.That(result.Tradeoff.TacticalSummary, Is.EqualTo(
                "2 WOLVES \u2022 30 SEC CONTROL"));
            Assert.That(result.Tradeoff.PossessionEnergyMaximumSeconds, Is.EqualTo(30));
        }

        [Test]
        public void KeeperReserveReturnsTheExactCachedFactForOneWolfAndOneOpenSlot()
        {
            var result = SylvanDefenseTradeoffEvaluator.Evaluate(
                new SylvanDefenseRosterSummary(1, 1, 1, 1));

            Assert.That(result.HasTradeoff, Is.True);
            Assert.That(result.Tradeoff, Is.SameAs(StarterSylvanDefenseTradeoffs.KeeperReserve));
            Assert.That(result.Issues, Is.Empty);
            Assert.That(StarterSylvanDefenseTradeoffs.KeeperReserveId, Is.EqualTo("KEEPER_RESERVE"));
            Assert.That(result.Tradeoff.TradeoffId, Is.EqualTo(
                StarterSylvanDefenseTradeoffs.KeeperReserveId));
            Assert.That(result.Tradeoff.DisplayName, Is.EqualTo("Keeper Reserve"));
            Assert.That(result.Tradeoff.TacticalSummary, Is.EqualTo(
                "1 WOLF SACRIFICED \u2022 45 SEC CONTROL"));
            Assert.That(result.Tradeoff.PossessionEnergyMaximumSeconds, Is.EqualTo(45));
        }

        [Test]
        public void CachedFactsExposeExactRosterCountsInStableOrder()
        {
            var facts = StarterSylvanDefenseTradeoffs.All;

            Assert.That(facts, Is.EqualTo(new[]
            {
                StarterSylvanDefenseTradeoffs.PackPressure,
                StarterSylvanDefenseTradeoffs.KeeperReserve
            }));
            Assert.That(facts.Select(fact => fact.Wolves), Is.EqualTo(new[]
            {
                2,
                1
            }));
            Assert.That(facts.Select(fact => fact.GuardianEnts), Is.EqualTo(new[]
            {
                1,
                1
            }));
            Assert.That(facts.Select(fact => fact.RootTraps), Is.EqualTo(new[]
            {
                1,
                1
            }));
            Assert.That(facts.Select(fact => fact.OpenCreatureSlots), Is.EqualTo(new[]
            {
                0,
                1
            }));
        }

        [Test]
        public void EvaluatorFailsClosedForMissingAndNegativeRosterSummaries()
        {
            var missing = SylvanDefenseTradeoffEvaluator.Evaluate(null);
            var negative = SylvanDefenseTradeoffEvaluator.Evaluate(
                new SylvanDefenseRosterSummary(-1, -1, -1, -1));

            AssertNoTradeoff(missing, new[]
            {
                SylvanDefenseTradeoffEvaluationIssue.RosterSummaryMissing
            });
            AssertNoTradeoff(negative, new[]
            {
                SylvanDefenseTradeoffEvaluationIssue.WolfCountNegative,
                SylvanDefenseTradeoffEvaluationIssue.GuardianEntCountNegative,
                SylvanDefenseTradeoffEvaluationIssue.RootTrapCountNegative,
                SylvanDefenseTradeoffEvaluationIssue.OpenCreatureSlotCountNegative
            });
        }

        [Test]
        public void EvaluatorFailsClosedForInvalidCardinalityAndUnsupportedRosterShapes()
        {
            var invalidCardinality = SylvanDefenseTradeoffEvaluator.Evaluate(
                new SylvanDefenseRosterSummary(2, 2, 0, 0));
            var unsupportedShape = SylvanDefenseTradeoffEvaluator.Evaluate(
                new SylvanDefenseRosterSummary(0, 1, 1, 2));

            AssertNoTradeoff(invalidCardinality, new[]
            {
                SylvanDefenseTradeoffEvaluationIssue.GuardianEntCardinalityInvalid,
                SylvanDefenseTradeoffEvaluationIssue.RootTrapCardinalityInvalid,
                SylvanDefenseTradeoffEvaluationIssue.CreatureSlotCardinalityInvalid
            });
            AssertNoTradeoff(unsupportedShape, new[]
            {
                SylvanDefenseTradeoffEvaluationIssue.UnsupportedRosterSummary
            });
        }

        [Test]
        public void InputsFactsAndResultsAreImmutableAndEvaluationIsDeterministic()
        {
            var input = new SylvanDefenseRosterSummary(1, 1, 1, 1);
            var first = SylvanDefenseTradeoffEvaluator.Evaluate(input);
            var second = SylvanDefenseTradeoffEvaluator.Evaluate(input);
            var facts = (IList<SylvanDefenseTradeoffFact>)StarterSylvanDefenseTradeoffs.All;
            var issues = (IList<SylvanDefenseTradeoffEvaluationIssue>)first.Issues;

            Assert.That(first.Tradeoff, Is.SameAs(second.Tradeoff));
            Assert.That(StarterSylvanDefenseTradeoffs.All,
                Is.SameAs(StarterSylvanDefenseTradeoffs.All));
            Assert.That(facts.IsReadOnly, Is.True);
            Assert.That(issues.IsReadOnly, Is.True);
            Assert.That(typeof(SylvanDefenseRosterSummary).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(SylvanDefenseTradeoffFact).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(SylvanDefenseTradeoffEvaluationResult).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.Throws<NotSupportedException>(() => facts[0] = null);
            Assert.Throws<NotSupportedException>(() => issues.Add(
                SylvanDefenseTradeoffEvaluationIssue.RosterSummaryMissing));
        }

        [Test]
        public void RuntimeAssemblyHasNoUnityOrGameRuntimeReference()
        {
            var dependencies = typeof(SylvanDefenseTradeoffEvaluator).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static void AssertNoTradeoff(
            SylvanDefenseTradeoffEvaluationResult result,
            IReadOnlyList<SylvanDefenseTradeoffEvaluationIssue> expectedIssues)
        {
            Assert.That(result.HasTradeoff, Is.False);
            Assert.That(result.Tradeoff, Is.Null);
            Assert.That(result.Issues, Is.EqualTo(expectedIssues));
        }
    }
}
