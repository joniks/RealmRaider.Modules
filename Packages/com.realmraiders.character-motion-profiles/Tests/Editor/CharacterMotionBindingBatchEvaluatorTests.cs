using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Modules.CharacterMotionProfiles.Tests
{
    public sealed class CharacterMotionBindingBatchEvaluatorTests
    {
        [Test]
        public void Evaluate_ValidExplicitBindingsAreOrdinalSortedAndUseCatalogueProfiles()
        {
            var alpha = Profile("alpha");
            var zeta = Profile("zeta");
            var catalogue = Catalogue(alpha, zeta);

            var result = CharacterMotionBindingBatchEvaluator.Evaluate(catalogue, new[]
            {
                Request("test.character.zeta", zeta.MotionProfileId),
                Request("test.character.alpha", alpha.MotionProfileId)
            });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Issues, Is.Empty);
            Assert.That(result.Records.Select(record => record.CharacterId), Is.EqualTo(new[]
            {
                "test.character.alpha",
                "test.character.zeta"
            }));
            Assert.That(result.Records[0].Profile, Is.SameAs(alpha));
            Assert.That(result.Records[1].Profile, Is.SameAs(zeta));
        }

        [Test]
        public void Evaluate_SnapshotsCallerRequestCollection()
        {
            var profile = Profile("alpha");
            var requests = new List<CharacterMotionBindingRequest>
            {
                Request("test.character.alpha", profile.MotionProfileId)
            };

            var result = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), requests);
            requests.Clear();
            requests.Add(Request("test.character.late", profile.MotionProfileId));

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Records.Count, Is.EqualTo(1));
            Assert.That(result.Records[0].CharacterId, Is.EqualTo("test.character.alpha"));
        }

        [Test]
        public void Evaluate_DuplicateCharacterBindingsFailClosedAndAreOrderIndependent()
        {
            var profile = Profile("alpha");
            var requests = new[]
            {
                Request("test.character.shared", profile.MotionProfileId),
                Request("test.character.shared", profile.MotionProfileId)
            };

            var first = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), requests);
            var reordered = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), requests.Reverse());

            Assert.That(first.Succeeded, Is.False);
            Assert.That(first.Records, Is.Empty);
            Assert.That(Codes(first), Has.Member(CharacterMotionBindingBatchIssueCode.DuplicateCharacterBinding));
            Assert.That(IssueSignatures(reordered), Is.EqualTo(IssueSignatures(first)));
        }

        [Test]
        public void Evaluate_NullAndUnreadableInputsFailClosed()
        {
            var profile = Profile("alpha");
            var validRequest = new[] { Request("test.character.alpha", profile.MotionProfileId) };

            var nullCatalogue = CharacterMotionBindingBatchEvaluator.Evaluate(null, validRequest);
            var nullRequests = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), null);
            var unreadableRequests = CharacterMotionBindingBatchEvaluator.Evaluate(
                Catalogue(profile),
                new ThrowingEnumerable<CharacterMotionBindingRequest>());
            var partiallyUnreadableRequests = CharacterMotionBindingBatchEvaluator.Evaluate(
                Catalogue(profile),
                new ThrowAfterOneEnumerable<CharacterMotionBindingRequest>(
                    Request("test.character.alpha", profile.MotionProfileId)));

            Assert.That(Codes(nullCatalogue), Has.Member(CharacterMotionBindingBatchIssueCode.NullCatalogue));
            Assert.That(Codes(nullRequests), Has.Member(CharacterMotionBindingBatchIssueCode.NullRequestCollection));
            Assert.That(Codes(unreadableRequests), Has.Member(CharacterMotionBindingBatchIssueCode.UnreadableRequestCollection));
            Assert.That(partiallyUnreadableRequests.Issues, Has.Count.EqualTo(1));
            Assert.That(
                partiallyUnreadableRequests.Issues[0].Code,
                Is.EqualTo(CharacterMotionBindingBatchIssueCode.UnreadableRequestCollection));
            Assert.That(nullCatalogue.Records, Is.Empty);
            Assert.That(nullRequests.Records, Is.Empty);
            Assert.That(unreadableRequests.Records, Is.Empty);
            Assert.That(partiallyUnreadableRequests.Records, Is.Empty);
        }

        [Test]
        public void Evaluate_NullRequestAndMalformedStableIdsFailClosed()
        {
            var profile = Profile("alpha");
            var result = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), new CharacterMotionBindingRequest[]
            {
                null,
                Request("Bad/Character", profile.MotionProfileId),
                Request("test.character.invalid-profile", "Bad/Profile")
            });

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Records, Is.Empty);
            Assert.That(Codes(result), Has.Member(CharacterMotionBindingBatchIssueCode.NullRequest));
            Assert.That(Codes(result), Has.Member(CharacterMotionBindingBatchIssueCode.InvalidCharacterId));
            Assert.That(Codes(result), Has.Member(CharacterMotionBindingBatchIssueCode.InvalidMotionProfileId));
            Assert.That(result.Issues, Is.EqualTo(result.Issues
                .OrderBy(issue => issue.Path, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code)
                .ThenBy(issue => issue.CompatibilityIssueCode)
                .ThenBy(issue => issue.ProfileIssueCode)
                .ToArray()));
        }

        [Test]
        public void Evaluate_MissingProfileUsesExactOrdinalLookupWithoutFallback()
        {
            var profile = Profile("alpha");
            var result = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), new[]
            {
                Request("test.character.alpha", "test.motion.alpha.v2")
            });

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Records, Is.Empty);
            Assert.That(result.Issues.Single().Code, Is.EqualTo(CharacterMotionBindingBatchIssueCode.MissingProfile));
            Assert.That(result.Issues.Single().Path,
                Is.EqualTo("bindings[\"test.character.alpha\"].motionProfileId"));
        }

        [Test]
        public void Evaluate_IncompatibleProfilePreservesCompatibilityIssueDetails()
        {
            var profile = Profile("alpha");
            var target = new CharacterMotionTargetRequirements(
                CharacterBodyFamily.LargeCreature,
                "test.rig.large-creature.v1",
                RequiredKeys(),
                false);
            var result = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), new[]
            {
                new CharacterMotionBindingRequest(
                    "test.character.alpha",
                    profile.MotionProfileId,
                    target)
            });

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Records, Is.Empty);
            Assert.That(Codes(result), Has.Member(CharacterMotionBindingBatchIssueCode.IncompatibleProfile));
            Assert.That(result.Issues.Any(issue =>
                issue.CompatibilityIssueCode == CharacterMotionCompatibilityIssueCode.FamilyMismatch), Is.True);
            Assert.That(result.Issues.Any(issue =>
                issue.CompatibilityIssueCode == CharacterMotionCompatibilityIssueCode.RigProfileMismatch), Is.True);
            Assert.That(result.Issues.Any(issue =>
                issue.CompatibilityIssueCode == CharacterMotionCompatibilityIssueCode.DisallowedFallback), Is.True);
        }

        [Test]
        public void Evaluate_MissingTargetRequirementsPreservesCompatibilityIssueDetails()
        {
            var profile = Profile("alpha");
            var result = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), new[]
            {
                new CharacterMotionBindingRequest(
                    "test.character.alpha",
                    profile.MotionProfileId,
                    null)
            });

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Records, Is.Empty);
            Assert.That(result.Issues.Single().Code,
                Is.EqualTo(CharacterMotionBindingBatchIssueCode.IncompatibleProfile));
            Assert.That(result.Issues.Single().CompatibilityIssueCode,
                Is.EqualTo(CharacterMotionCompatibilityIssueCode.MissingTarget));
            Assert.That(result.Issues.Single().Path,
                Is.EqualTo("bindings[\"test.character.alpha\"].target"));
        }

        [Test]
        public void BatchContractsAndCollections_AreImmutableWithoutUnityOrRuntimeDependency()
        {
            var profile = Profile("alpha");
            var success = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), new[]
            {
                Request("test.character.alpha", profile.MotionProfileId)
            });
            var failure = CharacterMotionBindingBatchEvaluator.Evaluate(Catalogue(profile), null);

            Assert.That(typeof(CharacterMotionBindingRequest).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterMotionBindingRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterMotionBindingBatchIssue).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.That(typeof(CharacterMotionBindingBatchResult).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite), Is.True);
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterMotionBindingRecord>)success.Records).Clear());
            Assert.Throws<NotSupportedException>(() =>
                ((IList<CharacterMotionBindingBatchIssue>)failure.Issues).Clear());

            var dependencies = typeof(CharacterMotionBindingBatchEvaluator).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();
            Assert.That(dependencies, Has.Member("RealmRaiders.ModuleContracts"));
            Assert.That(dependencies.Any(name => name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(dependencies.Any(name => name == "RealmRaiders.Runtime"), Is.False);
        }

        private static CharacterMotionBindingBatchIssueCode[] Codes(CharacterMotionBindingBatchResult result)
        {
            return result.Issues.Select(issue => issue.Code).ToArray();
        }

        private static string[] IssueSignatures(CharacterMotionBindingBatchResult result)
        {
            return result.Issues.Select(issue =>
                issue.Code + "|" + issue.Path + "|" + issue.CompatibilityIssueCode + "|" + issue.ProfileIssueCode).ToArray();
        }

        private static CharacterMotionBindingRequest Request(string characterId, string motionProfileId)
        {
            return new CharacterMotionBindingRequest(characterId, motionProfileId, ValidTarget());
        }

        private static CharacterMotionTargetRequirements ValidTarget()
        {
            return new CharacterMotionTargetRequirements(
                CharacterBodyFamily.Beast,
                "test.rig.beast.v1",
                RequiredKeys(),
                true);
        }

        private static CharacterMotionProfileCatalogue Catalogue(params CharacterMotionProfile[] profiles)
        {
            var build = CharacterMotionProfileCatalogue.Build(new ICharacterMotionProfileProvider[]
            {
                new TestProvider("test.provider.batch", profiles)
            });
            Assert.That(build.Succeeded, Is.True);
            return build.Catalogue;
        }

        private static CharacterMotionProfile Profile(string suffix)
        {
            return new CharacterMotionProfile(
                CharacterMotionProfileValidator.SupportedSchemaVersion,
                "test.motion." + suffix + ".v1",
                CharacterBodyFamily.Beast,
                "test.rig.beast.v1",
                "test.animator.beast.v1",
                RequiredClips(suffix),
                MotionRhythm.Neutral,
                "test.motion.fallback.v1",
                new[] { "test.source." + suffix + ".v1" });
        }

        private static IEnumerable<MotionClipKey> RequiredKeys()
        {
            yield return MotionClipKey.Idle;
            yield return MotionClipKey.Locomotion;
            yield return MotionClipKey.JumpTakeoff;
            yield return MotionClipKey.JumpFall;
            yield return MotionClipKey.JumpLand;
            yield return MotionClipKey.AttackPrimary;
            yield return MotionClipKey.AttackAbility;
            yield return MotionClipKey.Hit;
            yield return MotionClipKey.Death;
        }

        private static IEnumerable<MotionClipBinding> RequiredClips(string suffix)
        {
            foreach (var key in RequiredKeys())
            {
                yield return new MotionClipBinding(
                    key,
                    "test.clip." + suffix + "." + ClipName(key) + ".v1",
                    CharacterBodyFamily.Beast,
                    "test.rig.beast.v1",
                    key);
            }
        }

        private static string ClipName(MotionClipKey key)
        {
            switch (key)
            {
                case MotionClipKey.AttackPrimary: return "attack-primary";
                case MotionClipKey.AttackAbility: return "attack-ability";
                default: return key.ToString().ToLowerInvariant();
            }
        }

        private sealed class TestProvider : ICharacterMotionProfileProvider
        {
            public TestProvider(string moduleId, params CharacterMotionProfile[] profiles)
            {
                ModuleId = moduleId;
                Profiles = profiles;
            }

            public string ModuleId { get; }
            public IReadOnlyList<CharacterMotionProfile> Profiles { get; }
        }

        private sealed class ThrowingEnumerable<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                throw new InvalidOperationException("Synthetic unreadable input.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private sealed class ThrowAfterOneEnumerable<T> : IEnumerable<T>
        {
            private readonly T first;

            public ThrowAfterOneEnumerable(T first)
            {
                this.first = first;
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield return first;
                throw new InvalidOperationException("Synthetic partially unreadable input.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
