using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RealmRaiders.Modules.InfernalEncounters.Tests
{
    public sealed class StarterInfernalRaidPresentationCatalogueTests
    {
        [Test]
        public void CachedFactsPreservePacingOrderCanonicalNamesAndTacticalSummaries()
        {
            var presentations = StarterInfernalRaidPresentationCatalogue.All;
            var pacingCompositions = StarterInfernalRaidPacingCatalogue.All;

            Assert.That(presentations.Select(presentation => presentation.CompositionId), Is.EqualTo(
                pacingCompositions.Select(composition => composition.CompositionId)));
            Assert.That(presentations.Select(presentation => presentation.DisplayName), Is.EqualTo(
                pacingCompositions.Select(composition => composition.DisplayName)));
            Assert.That(presentations.Select(presentation => presentation.TacticalSummary), Is.EqualTo(
                new[]
                {
                    "1 HELLHOUND \u2022 ~35 SEC",
                    "2 HELLHOUNDS \u2022 OPTIONAL FLAME BYPASS \u2022 ~55 SEC",
                    "2 HELLHOUNDS + BRUTE \u2022 OPTIONAL FLAME BYPASS \u2022 ~80 SEC"
                }));

            foreach (var presentation in presentations)
            {
                Assert.That(presentation.DisplayName, Is.SameAs(
                    FindPacingComposition(presentation.CompositionId).DisplayName));
                Assert.That(presentation.TacticalSummary, Is.EqualTo(
                    presentation.TacticalSummary.ToUpperInvariant()));
                Assert.That(presentation.TacticalSummary.Length, Is.LessThanOrEqualTo(64));
            }
        }

        [Test]
        public void FindByCompositionIdReturnsTheExactCachedPresentationUsingOrdinalIds()
        {
            foreach (var expectedPresentation in StarterInfernalRaidPresentationCatalogue.All)
            {
                var result = StarterInfernalRaidPresentationCatalogue.FindByCompositionId(
                    expectedPresentation.CompositionId);

                Assert.That(result.Status, Is.EqualTo(
                    InfernalRaidPresentationLookupStatus.Found));
                Assert.That(result.Found, Is.True);
                Assert.That(result.Presentation, Is.SameAs(expectedPresentation));
            }

            var caseMismatch = StarterInfernalRaidPresentationCatalogue.FindByCompositionId(
                StarterInfernalRaidPacingCatalogue.EntryTrial.CompositionId.ToUpperInvariant());

            Assert.That(caseMismatch.Status, Is.EqualTo(
                InfernalRaidPresentationLookupStatus.NotFound));
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
                "realmraiders.infernal-raid.unknown"
            })
            {
                var result = StarterInfernalRaidPresentationCatalogue.FindByCompositionId(
                    compositionId);

                Assert.That(result.Found, Is.False);
                Assert.That(result.Presentation, Is.Null);
                Assert.That(result.Status, Is.EqualTo(string.IsNullOrWhiteSpace(compositionId)
                    ? InfernalRaidPresentationLookupStatus.InvalidCompositionId
                    : InfernalRaidPresentationLookupStatus.NotFound));
            }
        }

        [Test]
        public void ValidateAcceptsEveryCachedFactWithStableEmptyEvidence()
        {
            foreach (var presentation in StarterInfernalRaidPresentationCatalogue.All)
            {
                var first = InfernalRaidPresentationEvidence.Validate(presentation);
                var second = InfernalRaidPresentationEvidence.Validate(presentation);

                Assert.That(first.IsValid, Is.True);
                Assert.That(first.Issues, Is.Empty);
                Assert.That(second.IsValid, Is.EqualTo(first.IsValid));
                Assert.That(second.Issues, Is.EqualTo(first.Issues));
            }
        }

        [Test]
        public void ValidateFailsClosedForMissingEmptyAndUnknownFacts()
        {
            var missing = InfernalRaidPresentationEvidence.Validate(null);
            var emptyId = InfernalRaidPresentationEvidence.Validate(
                new InfernalRaidPresentationFact(
                    string.Empty,
                    "Entry Trial",
                    "1 HELLHOUND \u2022 ~35 SEC"));
            var unknown = InfernalRaidPresentationEvidence.Validate(
                new InfernalRaidPresentationFact(
                    "realmraiders.infernal-raid.unknown",
                    "Unknown",
                    "UNKNOWN"));

            Assert.That(missing.Issues, Is.EqualTo(new[]
            {
                InfernalRaidPresentationValidationIssue.PresentationMissing
            }));
            Assert.That(emptyId.Issues, Is.EqualTo(new[]
            {
                InfernalRaidPresentationValidationIssue.CompositionIdInvalid
            }));
            Assert.That(unknown.Issues, Is.EqualTo(new[]
            {
                InfernalRaidPresentationValidationIssue.PacingCompositionNotFound
            }));
        }

        [Test]
        public void ValidateReportsDisplayAndSummaryMismatchesInStableOrder()
        {
            var presentation = new InfernalRaidPresentationFact(
                StarterInfernalRaidPacingCatalogue.RiskRoute.CompositionId,
                "Risky Route",
                "2 HELLHOUNDS \u2022 ~55 SEC");

            var result = InfernalRaidPresentationEvidence.Validate(presentation);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues, Is.EqualTo(new[]
            {
                InfernalRaidPresentationValidationIssue.DisplayNameMismatch,
                InfernalRaidPresentationValidationIssue.TacticalSummaryMismatch
            }));
        }

        [Test]
        public void CatalogueAndValidationResultAreImmutableCachedSnapshots()
        {
            Assert.That(StarterInfernalRaidPresentationCatalogue.All,
                Is.SameAs(StarterInfernalRaidPresentationCatalogue.All));
            Assert.That(StarterInfernalRaidPresentationCatalogue.EntryTrial,
                Is.SameAs(StarterInfernalRaidPresentationCatalogue.EntryTrial));
            Assert.That(StarterInfernalRaidPresentationCatalogue.RiskRoute,
                Is.SameAs(StarterInfernalRaidPresentationCatalogue.RiskRoute));
            Assert.That(StarterInfernalRaidPresentationCatalogue.BruteFinale,
                Is.SameAs(StarterInfernalRaidPresentationCatalogue.BruteFinale));

            var presentations = (IList<InfernalRaidPresentationFact>)
                StarterInfernalRaidPresentationCatalogue.All;
            var validation = InfernalRaidPresentationEvidence.Validate(
                StarterInfernalRaidPresentationCatalogue.EntryTrial);
            var issues = (IList<InfernalRaidPresentationValidationIssue>)validation.Issues;

            Assert.That(presentations.IsReadOnly, Is.True);
            Assert.That(issues.IsReadOnly, Is.True);
            Assert.That(typeof(InfernalRaidPresentationFact).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(InfernalRaidPresentationLookupResult).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.That(typeof(InfernalRaidPresentationValidationResult).GetProperties(
                BindingFlags.Instance | BindingFlags.Public).All(property => !property.CanWrite),
                Is.True);
            Assert.Throws<NotSupportedException>(() => presentations[0] = null);
            Assert.Throws<NotSupportedException>(() => issues.Add(
                InfernalRaidPresentationValidationIssue.PresentationMissing));
        }

        [Test]
        public void RuntimeAssemblyHasNoUnityOrGameRuntimeReference()
        {
            var dependencies = typeof(StarterInfernalRaidPresentationCatalogue).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine")), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static InfernalRaidPacingComposition FindPacingComposition(string compositionId)
        {
            foreach (var composition in StarterInfernalRaidPacingCatalogue.All)
            {
                if (string.Equals(
                        composition.CompositionId,
                        compositionId,
                        StringComparison.Ordinal))
                {
                    return composition;
                }
            }

            Assert.Fail("Expected a known pacing composition.");
            return null;
        }
    }
}
