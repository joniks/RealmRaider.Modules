using NUnit.Framework;
using RealmRaiders.Modules.InfernalDefenseLayouts;
using RealmRaiders.Modules.RealmGrowthContracts;

namespace RealmRaiders.Modules.RealmExpansionPlanning.InfernalCompatibility.Tests
{
    public sealed class InfernalRealmExpansionCompatibilityTests
    {
        [Test]
        public void Evaluate_MapsEveryInfernalLayoutCapacityInExactAuthoredOrder()
        {
            Assert.That(StarterInfernalDefenseLayouts.All.Count, Is.EqualTo(3));
            foreach (var layout in StarterInfernalDefenseLayouts.All)
            {
                for (var capacity = 0;
                     capacity <= layout.ExpansionSockets.Count;
                     capacity++)
                {
                    var result = RealmExpansionPlanEvaluator.Evaluate(
                        layout,
                        new RealmGrowthTier("tier.valid", 0, 1, 1, capacity));

                    Assert.That(result.HasPlan, Is.True, layout.LayoutId + " @ " + capacity);
                    Assert.That(result.Plan.LayoutId, Is.EqualTo(layout.LayoutId));
                    Assert.That(result.Plan.TierId, Is.EqualTo("tier.valid"));
                    Assert.That(result.Plan.ExpansionSockets.Count, Is.EqualTo(capacity));
                    for (var index = 0; index < capacity; index++)
                    {
                        var expected = layout.ExpansionSockets[index];
                        var actual = result.Plan.ExpansionSockets[index];
                        Assert.That(actual, Is.SameAs(expected));
                        Assert.That(actual.SocketId, Is.EqualTo(expected.SocketId));
                        Assert.That(actual.NodeId, Is.EqualTo(expected.NodeId));
                        Assert.That(actual.X, Is.EqualTo(expected.X));
                        Assert.That(actual.Z, Is.EqualTo(expected.Z));
                    }
                }
            }
        }
    }
}
