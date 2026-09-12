using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.SylvanEncounters.Tests
{
    public sealed class StarterSylvanRaidPresentationCatalogueTests
    {
        [Test]
        public void CachedFactsPreserveCompositionOrderCanonicalNamesAndTacticalSummaries()
        {
            var presentations = StarterSylvanRaidPresentationCatalogue.All;
            var compositions = StarterSylvanRaidCompositions.All;

            Assert.That(presentations.Select(presentation => presentation.CompositionId), Is.EqualTo(
                compositions.Select(composition => composition.CompositionId)));
            Assert.That(presentations.Select(presentation => presentation.DisplayName), Is.EqualTo(
                compositions.Select(composition => composition.DisplayName)));
            Assert.That(presentations.Select(presentation => presentation.TacticalSummary), Is.EqualTo(
                new[]
                {
                    "2 WOLVES \u2022 WOLF GROVE PRESSURE \u2022 1 ENT",
                    "3 WOLVES \u2022 MOONWELL PRESSURE \u2022 1 ENT",
                    "2 WOLVES \u2022 ENT GROVE PRESSURE \u2022 1 ENT"
                }));

            foreach (var presentation in presentations)
            {
                Assert.That(presentation.DisplayName, Is.SameAs(
                    FindComposition(presentation.CompositionId).DisplayName));
                Assert.That(presentation.TacticalSummary, Is.EqualTo(
                    presentation.TacticalSummary.ToUpperInvariant()));
                Assert.That(presentation.TacticalSummary.Length, Is.LessThanOrEqualTo(64));
            }
        }

        [Test]
        public void OnlyWolfPressureStatesMoonwellPressure()
        {
            Assert.That(StarterSylvanRaidPresentationCatalogue.Baseline.TacticalSummary,
                Does.Not.Contain("MOONWELL"));
            Assert.That(StarterSylvanRaidPresentationCatalogue.WolfPressure.TacticalSummary,
                Does.Contain("MOONWELL PRESSURE"));
            Assert.That(StarterSylvanRaidPresentationCatalogue.SentinelEscort.TacticalSummary,
                Does.Not.Contain("MOONWELL"));
        }

        [Test]
        public void FindByCompositionIdReturnsTheExactCachedPresentationUsingOrdinalIds()
        {
            foreach (var expectedPresentation in StarterSylvanRaidPresentationCatalogue.All)
            {
                var result = StarterSylvanRaidPresentationCatalogue.FindByCompositionId(
                    expectedPresentation.CompositionId);

                Assert.That(result.Status, Is.EqualTo(SylvanRaidPresentationLookupStatus.Found));
                Assert.That(result.Found, Is.True);
                Assert.That(result.Presentation, Is.SameAs(expectedPresentation));
            }

            var caseMismatch = StarterSylvanRaidPresentationCatalogue.FindByCompositionId(
                StarterSylvanRaidCompositions.Baseline.CompositionId.ToUpperInvariant());

            Assert.That(caseMismatch.Status, Is.EqualTo(
                SylvanRaidPresentationLookupStatus.NotFound));
            Assert.That(caseMismatch.Found, Is.False);
            Assert.That(caseMismatch.Presentation, Is.Null);
        }

        [Test]
        public void FindByCompositionIdFailsClosedForEmptyAndUnknownIds()
        {
            foreach (var compositionId in new[]
            {
                null,
                string.Empty,
                " ",
                "realmraiders.sylvan-raid.unknown"
            })
            {
                var result = StarterSylvanRaidPresentationCatalogue.FindByCompositionId(
                    compositionId);

                Assert.That(result.Found, Is.False);
                Assert.That(result.Presentation, Is.Null);
                Assert.That(result.Status, Is.EqualTo(string.IsNullOrWhiteSpace(compositionId)
                    ? SylvanRaidPresentationLookupStatus.InvalidCompositionId
                    : SylvanRaidPresentationLookupStatus.NotFound));
            }
        }

        [Test]
        public void ValidateAcceptsEveryCachedFactWithStableEmptyEvidence()
        {
            foreach (var presentation in StarterSylvanRaidPresentationCatalogue.All)
            {
                var first = SylvanRaidPresentationEvidence.Validate(presentation);
                var second = SylvanRaidPresentationEvidence.Validate(presentation);

                Assert.That(first.IsValid, Is.True);
                Assert.That(first.Issues, Is.Empty);
                Assert.That(second.IsValid, Is.EqualTo(first.IsValid));
                Assert.That(second.Issues, Is.EqualTo(first.Issues));
            }
        }

        [Test]
        public void ValidateFailsClosedForMissingEmptyAndUnknownFacts()
        {
            var missing = SylvanRaidPresentationEvidence.Validate(null);
            var emptyId = SylvanRaidPresentationEvidence.Validate(
                new SylvanRaidPresentationFact(
                    string.Empty,
                    "Baseline",
                    "2 WOLVES \u2022 WOLF GROVE PRESSURE \u2022 1 ENT"));
            var unknown = SylvanRaidPresentationEvidence.Validate(
                new SylvanRaidPresentationFact(
                    "realmraiders.sylvan-raid.unknown",
                    "Unknown",
                    "UNKNOWN"));

            Assert.That(missing.Issues, Is.EqualTo(new[]
            {
                SylvanRaidPresentationValidationIssue.PresentationMissing
            }));
            Assert.That(emptyId.Issues, Is.EqualTo(new[]
            {
                SylvanRaidPresentationValidationIssue.CompositionIdInvalid
            }));
            Assert.That(unknown.Issues, Is.EqualTo(new[]
            {
                SylvanRaidPresentationValidationIssue.CompositionNotFound
            }));
        }

        [Test]
        public void ValidateReportsDisplayAndSummaryMismatchesInStableOrder()
        {
            var presentation = new SylvanRaidPresentationFact(
                StarterSylvanRaidCompositions.WolfPressure.CompositionId,
                "Wolf Rush",
                "3 WOLVES \u2022 1 ENT");

            var result = SylvanRaidPresentationEvidence.Validate(presentation);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues, Is.EqualTo(new[]
            {
                SylvanRaidPresentationValidationIssue.DisplayNameMismatch,
                SylvanRaidPresentationValidationIssue.TacticalSummaryMismatch
            }));
        }

        [Test]
        public void CatalogueAndValidationResultAreImmutableCachedSnapshots()
        {
            Assert.That(StarterSylvanRaidPresentationCatalogue.All,
                Is.SameAs(StarterSylvanRaidPresentationCatalogue.All));
            Assert.That(StarterSylvanRaidPresentationCatalogue.Baseline,
                Is.SameAs(StarterSylvanRaidPresentationCatalogue.Baseline));
            Assert.That(StarterSylvanRaidPresentationCatalogue.WolfPressure,
                Is.SameAs(StarterSylvanRaidPresentationCatalogue.WolfPressure));
            Assert.That(StarterSylvanRaidPresentationCatalogue.SentinelEscort,
                Is.SameAs(StarterSylvanRaidPresentationCatalogue.SentinelEscort));

            var presentations = (IList<SylvanRaidPresentationFact>)
                StarterSylvanRaidPresentationCatalogue.All;
            var validation = SylvanRaidPresentationEvidence.Validate(
                StarterSylvanRaidPresentationCatalogue.Baseline);
            var issues = (IList<SylvanRaidPresentationValidationIssue>)validation.Issues;

            Assert.That(presentations.IsReadOnly, Is.True);
            Assert.That(issues.IsReadOnly, Is.True);
            Assert.That(typeof(SylvanRaidPresentationFact).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(SylvanRaidPresentationLookupResult).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(SylvanRaidPresentationValidationResult).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.Throws<NotSupportedException>(() => presentations[0] = null);
            Assert.Throws<NotSupportedException>(() => issues.Add(
                SylvanRaidPresentationValidationIssue.PresentationMissing));
        }

        [Test]
        public void RuntimeAssemblyHasNoUnityOrGameRuntimeReference()
        {
            var dependencies = typeof(StarterSylvanRaidPresentationCatalogue).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static SylvanRaidComposition FindComposition(string compositionId)
        {
            foreach (var composition in StarterSylvanRaidCompositions.All)
            {
                if (string.Equals(
                        composition.CompositionId,
                        compositionId,
                        StringComparison.Ordinal))
                {
                    return composition;
                }
            }

            Assert.Fail("Expected a known Sylvan composition.");
            return null;
        }
    }
}
