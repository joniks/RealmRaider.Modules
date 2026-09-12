using System;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules.RealmGrowthContracts;
using RealmRaiders.Modules.StarterRealmLayouts;

namespace RealmRaiders.Modules.StarterRealmGrowth.Tests
{
    public sealed class StarterRealmExpansionPlanEvaluatorTests
    {
        [Test]
        public void Evaluate_UsesEveryCachedLayoutWithZeroOneAndTwoAuthoredSocketsInOrder()
        {
            AssertPlan(StarterSylvanRealmLayouts.AncientCrossroads, 0);
            AssertPlan(StarterSylvanRealmLayouts.AncientCrossroads, 1);
            AssertPlan(StarterSylvanRealmLayouts.AncientCrossroads, 2);
            AssertPlan(StarterSylvanRealmLayouts.ForkedCanopy, 0);
            AssertPlan(StarterSylvanRealmLayouts.ForkedCanopy, 1);
            AssertPlan(StarterSylvanRealmLayouts.ForkedCanopy, 2);
            AssertPlan(StarterSylvanRealmLayouts.SerpentRoots, 0);
            AssertPlan(StarterSylvanRealmLayouts.SerpentRoots, 1);
            AssertPlan(StarterSylvanRealmLayouts.SerpentRoots, 2);
        }

        [Test]
        public void Evaluate_FailsClosedWhenCapacityExceedsAvailableAuthoredSockets()
        {
            var layout = StarterSylvanRealmLayouts.AncientCrossroads;
            var result = StarterRealmExpansionPlanEvaluator.Evaluate(
                layout,
                ValidTier(3));

            Assert.That(result.HasPlan, Is.False);
            Assert.That(result.Status,
                Is.EqualTo(StarterRealmExpansionPlanStatus.Rejected));
            Assert.That(result.Plan, Is.Null);
            CollectionAssert.AreEqual(
                new[]
                {
                    StarterRealmExpansionPlanIssue.ExpansionAnchorCapacityExceedsAvailableSockets
                },
                result.Issues);
        }

        [Test]
        public void Evaluate_FailsClosedForMissingInvalidAndMismatchedInputsInStableOrder()
        {
            var missing = StarterRealmExpansionPlanEvaluator.Evaluate(null, null);
            var invalidTier = StarterRealmExpansionPlanEvaluator.Evaluate(
                StarterSylvanRealmLayouts.ForkedCanopy,
                new RealmGrowthTier("Tier.invalid", -1, 0, 0, -1));
            var invalidLayoutId = StarterRealmExpansionPlanEvaluator.Evaluate(
                CopyWithLayoutId(
                    StarterSylvanRealmLayouts.AncientCrossroads,
                    "realmraiders.sylvan-layout.unknown"),
                ValidTier(1));
            var mismatchedLayout = StarterRealmExpansionPlanEvaluator.Evaluate(
                CopyWithSockets(
                    StarterSylvanRealmLayouts.SerpentRoots,
                    StarterSylvanRealmLayouts.SerpentRoots.ExpansionSockets),
                ValidTier(1));

            CollectionAssert.AreEqual(
                new[]
                {
                    StarterRealmExpansionPlanIssue.LayoutMissing,
                    StarterRealmExpansionPlanIssue.TierMissing
                },
                missing.Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    StarterRealmExpansionPlanIssue.TierIdInvalid,
                    StarterRealmExpansionPlanIssue.TierFactsInvalid
                },
                invalidTier.Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    StarterRealmExpansionPlanIssue.LayoutIdInvalid
                },
                invalidLayoutId.Issues);
            CollectionAssert.AreEqual(
                new[]
                {
                    StarterRealmExpansionPlanIssue.LayoutIdentityMismatch
                },
                mismatchedLayout.Issues);
        }

        [Test]
        public void Evaluate_FailsClosedForNonFiniteAuthoredSocketFacts()
        {
            var layout = StarterSylvanRealmLayouts.AncientCrossroads;
            var alteredSockets = new RealmLayoutExpansionSocket[]
            {
                new RealmLayoutExpansionSocket(
                    layout.ExpansionSockets[0].SocketId,
                    layout.ExpansionSockets[0].NodeId,
                    float.NaN,
                    layout.ExpansionSockets[0].Z),
                layout.ExpansionSockets[1]
            };
            var result = StarterRealmExpansionPlanEvaluator.Evaluate(
                CopyWithSockets(layout, alteredSockets),
                ValidTier(1));

            Assert.That(result.HasPlan, Is.False);
            CollectionAssert.AreEqual(
                new[]
                {
                    StarterRealmExpansionPlanIssue.LayoutIdentityMismatch,
                    StarterRealmExpansionPlanIssue.LayoutInvalid,
                    StarterRealmExpansionPlanIssue.ExpansionSocketFactInvalid
                },
                result.Issues);
        }

        [TestCaseSource(nameof(InvalidLayoutAndSocketFixtures))]
        public void Evaluate_FailsClosedForInvalidLayoutAndSocketFactsInStableOrder(
            RealmLayoutRecipe layout,
            StarterRealmExpansionPlanIssue[] expectedEvaluationIssues,
            RealmLayoutValidationIssue[] expectedLayoutIssues)
        {
            var evaluation = StarterRealmExpansionPlanEvaluator.Evaluate(
                layout,
                ValidTier(1));
            var validation = RealmLayoutRecipeValidator.ValidateStarterRecipe(layout);

            Assert.That(evaluation.HasPlan, Is.False);
            CollectionAssert.AreEqual(expectedEvaluationIssues, evaluation.Issues);
            CollectionAssert.AreEqual(expectedLayoutIssues, validation.Issues);
        }

        [Test]
        public void Evaluate_IsDeterministicAndSnapshotsSelectedSockets()
        {
            var layout = StarterSylvanRealmLayouts.ForkedCanopy;
            var first = StarterRealmExpansionPlanEvaluator.Evaluate(layout, ValidTier(2));
            var second = StarterRealmExpansionPlanEvaluator.Evaluate(layout, ValidTier(2));

            Assert.That(first.HasPlan, Is.True);
            Assert.That(second.HasPlan, Is.True);
            Assert.That(first.Plan.LayoutId, Is.EqualTo(layout.LayoutId));
            Assert.That(first.Plan.TierId, Is.EqualTo("tier.valid"));
            CollectionAssert.AreEqual(first.Plan.ExpansionSockets, second.Plan.ExpansionSockets);
            Assert.That(first.Plan.ExpansionSockets[0],
                Is.SameAs(layout.ExpansionSockets[0]));
            Assert.Throws<NotSupportedException>(() =>
                ((IList<RealmLayoutExpansionSocket>)first.Plan.ExpansionSockets)[0] = null);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<StarterRealmExpansionPlanIssue>)
                    StarterRealmExpansionPlanEvaluator.Evaluate(null, null).Issues)[0] =
                    StarterRealmExpansionPlanIssue.LayoutInvalid);
        }

        private static void AssertPlan(
            RealmLayoutRecipe layout,
            int capacity)
        {
            var result = StarterRealmExpansionPlanEvaluator.Evaluate(
                layout,
                ValidTier(capacity));

            Assert.That(result.HasPlan, Is.True);
            Assert.That(result.Status,
                Is.EqualTo(StarterRealmExpansionPlanStatus.Planned));
            Assert.That(result.Plan.LayoutId, Is.EqualTo(layout.LayoutId));
            Assert.That(result.Plan.TierId, Is.EqualTo("tier.valid"));
            Assert.That(result.Plan.ExpansionSockets.Count, Is.EqualTo(capacity));
            for (var index = 0; index < capacity; index++)
            {
                Assert.That(result.Plan.ExpansionSockets[index],
                    Is.SameAs(layout.ExpansionSockets[index]));
            }
        }

        private static RealmGrowthTier ValidTier(int capacity)
        {
            return new RealmGrowthTier("tier.valid", 0, 1, 1, capacity);
        }

        private static IEnumerable<TestCaseData> InvalidLayoutAndSocketFixtures()
        {
            var layout = StarterSylvanRealmLayouts.AncientCrossroads;
            yield return Fixture(
                "noncanonical-layout-id",
                CopyWithLayoutId(layout, "Realmraiders.sylvan-layout.ancient-crossroads"),
                new[]
                {
                    StarterRealmExpansionPlanIssue.LayoutIdInvalid,
                    StarterRealmExpansionPlanIssue.LayoutInvalid
                },
                new[]
                {
                    RealmLayoutValidationIssue.LayoutIdInvalid
                });
            yield return Fixture(
                "null-socket",
                CopyWithSockets(
                    layout,
                    new RealmLayoutExpansionSocket[]
                    {
                        null,
                        layout.ExpansionSockets[1]
                    }),
                new[]
                {
                    StarterRealmExpansionPlanIssue.LayoutIdentityMismatch,
                    StarterRealmExpansionPlanIssue.LayoutInvalid,
                    StarterRealmExpansionPlanIssue.ExpansionSocketFactInvalid
                },
                new[]
                {
                    RealmLayoutValidationIssue.ExpansionSocketMissing
                });
            yield return Fixture(
                "noncanonical-socket-id",
                CopyWithSockets(
                    layout,
                    new RealmLayoutExpansionSocket[]
                    {
                        new RealmLayoutExpansionSocket(
                            "Ancient.west-bough",
                            layout.ExpansionSockets[0].NodeId,
                            layout.ExpansionSockets[0].X,
                            layout.ExpansionSockets[0].Z),
                        layout.ExpansionSockets[1]
                    }),
                InvalidSocketEvaluationIssues(),
                new[]
                {
                    RealmLayoutValidationIssue.ExpansionSocketIdInvalid
                });
            yield return Fixture(
                "noncanonical-socket-node-id",
                CopyWithSockets(
                    layout,
                    new RealmLayoutExpansionSocket[]
                    {
                        new RealmLayoutExpansionSocket(
                            layout.ExpansionSockets[0].SocketId,
                            "Ancient.wolf-grove",
                            layout.ExpansionSockets[0].X,
                            layout.ExpansionSockets[0].Z),
                        layout.ExpansionSockets[1]
                    }),
                InvalidSocketEvaluationIssues(),
                new[]
                {
                    RealmLayoutValidationIssue.ExpansionSocketNodeInvalid
                });
            yield return Fixture(
                "duplicate-socket-id",
                CopyWithSockets(
                    layout,
                    new RealmLayoutExpansionSocket[]
                    {
                        layout.ExpansionSockets[0],
                        new RealmLayoutExpansionSocket(
                            layout.ExpansionSockets[0].SocketId,
                            layout.ExpansionSockets[1].NodeId,
                            layout.ExpansionSockets[1].X,
                            layout.ExpansionSockets[1].Z)
                    }),
                new[]
                {
                    StarterRealmExpansionPlanIssue.LayoutIdentityMismatch,
                    StarterRealmExpansionPlanIssue.LayoutInvalid
                },
                new[]
                {
                    RealmLayoutValidationIssue.ExpansionSocketIdDuplicate
                });
            yield return Fixture(
                "socket-node-not-in-layout",
                CopyWithSockets(
                    layout,
                    new RealmLayoutExpansionSocket[]
                    {
                        new RealmLayoutExpansionSocket(
                            layout.ExpansionSockets[0].SocketId,
                            "ancient.unknown-node",
                            layout.ExpansionSockets[0].X,
                            layout.ExpansionSockets[0].Z),
                        layout.ExpansionSockets[1]
                    }),
                new[]
                {
                    StarterRealmExpansionPlanIssue.LayoutIdentityMismatch,
                    StarterRealmExpansionPlanIssue.LayoutInvalid
                },
                new[]
                {
                    RealmLayoutValidationIssue.ExpansionSocketNodeInvalid
                });
        }

        private static TestCaseData Fixture(
            string name,
            RealmLayoutRecipe layout,
            StarterRealmExpansionPlanIssue[] expectedEvaluationIssues,
            RealmLayoutValidationIssue[] expectedLayoutIssues)
        {
            return new TestCaseData(
                    layout,
                    expectedEvaluationIssues,
                    expectedLayoutIssues)
                .SetName(name);
        }

        private static StarterRealmExpansionPlanIssue[] InvalidSocketEvaluationIssues()
        {
            return new[]
            {
                StarterRealmExpansionPlanIssue.LayoutIdentityMismatch,
                StarterRealmExpansionPlanIssue.LayoutInvalid,
                StarterRealmExpansionPlanIssue.ExpansionSocketFactInvalid
            };
        }

        private static RealmLayoutRecipe CopyWithSockets(
            RealmLayoutRecipe layout,
            IReadOnlyList<RealmLayoutExpansionSocket> expansionSockets)
        {
            return new RealmLayoutRecipe(
                layout.LayoutId,
                layout.DisplayName,
                layout.Nodes,
                layout.Edges,
                layout.Landmarks,
                expansionSockets);
        }

        private static RealmLayoutRecipe CopyWithLayoutId(
            RealmLayoutRecipe layout,
            string layoutId)
        {
            return new RealmLayoutRecipe(
                layoutId,
                layout.DisplayName,
                layout.Nodes,
                layout.Edges,
                layout.Landmarks,
                layout.ExpansionSockets);
        }
    }
}
