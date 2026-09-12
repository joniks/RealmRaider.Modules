using System;
using System.Collections;
using System.Text;
using NUnit.Framework;

namespace RealmRaiders.Modules.StarterRealmIdentityRecord.Tests
{
    public sealed class StarterRealmIdentityCodecTests
    {
        private const string RealmId = "realmraiders.realm.sylvan";
        private const string LayoutId = "realmraiders.sylvan-layout.ancient-crossroads";
        private const string Canonical =
            "version=1\nrealmId=realmraiders.realm.sylvan\nseed=-17\n"
            + "layoutId=realmraiders.sylvan-layout.ancient-crossroads";

        [Test]
        public void Create_ProducesImmutableExactIdentityFacts()
        {
            var result = StarterRealmIdentityCodec.Create(1, RealmId, -17, LayoutId);

            Assert.That(result.HasRecord, Is.True);
            Assert.That(result.Record.Version, Is.EqualTo(1));
            Assert.That(result.Record.RealmId, Is.EqualTo(RealmId));
            Assert.That(result.Record.Seed, Is.EqualTo(-17));
            Assert.That(result.Record.LayoutId, Is.EqualTo(LayoutId));
            Assert.That(result.Evidence, Is.Empty);
            Assert.That(result.Issues, Is.Empty);
        }

        [Test]
        public void Serialization_HasExactCanonicalTextAndUtf8BytesWithoutBom()
        {
            var record = ValidRecord(-17);

            Assert.That(
                StarterRealmIdentityCodec.SerializeCanonicalText(record),
                Is.EqualTo(Canonical));
            CollectionAssert.AreEqual(
                new UTF8Encoding(false, true).GetBytes(Canonical),
                StarterRealmIdentityCodec.SerializeCanonicalUtf8(record));
        }

        [Test]
        public void RoundTripUtf8_IsByteExactForFullSignedSeedEdges()
        {
            foreach (var seed in new[]
            {
                int.MinValue,
                int.MinValue + 1,
                -1,
                0,
                1,
                int.MaxValue - 1,
                int.MaxValue
            })
            {
                var original = StarterRealmIdentityCodec.SerializeCanonicalUtf8(
                    ValidRecord(seed));
                var parsed = StarterRealmIdentityCodec.ParseCanonicalUtf8(original);
                var roundTrip = StarterRealmIdentityCodec.SerializeCanonicalUtf8(
                    parsed.Record);

                Assert.That(parsed.HasRecord, Is.True);
                Assert.That(parsed.Record.Seed, Is.EqualTo(seed));
                CollectionAssert.AreEqual(original, roundTrip, "seed " + seed);
            }
        }

        [Test]
        public void Serialization_IsDeterministicAcrossRepeatedCalls()
        {
            var record = ValidRecord(74931);
            var first = StarterRealmIdentityCodec.SerializeCanonicalUtf8(record);
            for (var repeat = 0; repeat < 16; repeat++)
            {
                CollectionAssert.AreEqual(
                    first,
                    StarterRealmIdentityCodec.SerializeCanonicalUtf8(record));
            }
        }

        [Test]
        public void Parse_RejectsNullEmptyAndInvalidUtf8()
        {
            AssertIssues(
                StarterRealmIdentityCodec.ParseCanonicalText(null),
                StarterRealmIdentityIssue.InputMissing);
            AssertIssues(
                StarterRealmIdentityCodec.ParseCanonicalText(string.Empty),
                StarterRealmIdentityIssue.InputEmpty);
            AssertIssues(
                StarterRealmIdentityCodec.ParseCanonicalUtf8(null),
                StarterRealmIdentityIssue.InputMissing);
            AssertIssues(
                StarterRealmIdentityCodec.ParseCanonicalUtf8(
                    new byte[] { 0x76, 0x65, 0x72, 0xFF }),
                StarterRealmIdentityIssue.EncodingInvalid);
        }

        [Test]
        public void Parse_RejectsEveryMissingFieldInCanonicalEvidenceOrder()
        {
            AssertMissing("realmId=" + RealmId + "\nseed=1\nlayoutId=" + LayoutId,
                "version");
            AssertMissing("version=1\nseed=1\nlayoutId=" + LayoutId,
                "realmId");
            AssertMissing("version=1\nrealmId=" + RealmId + "\nlayoutId=" + LayoutId,
                "seed");
            AssertMissing("version=1\nrealmId=" + RealmId + "\nseed=1",
                "layoutId");
        }

        [Test]
        public void Parse_RejectsDuplicateUnknownAndExtraFieldsWithStableEvidence()
        {
            var duplicate = StarterRealmIdentityCodec.ParseCanonicalText(
                Canonical + "\nseed=18");
            AssertEvidence(
                duplicate,
                Evidence(StarterRealmIdentityIssue.FieldDuplicate, 4, "seed", "18"),
                Evidence(StarterRealmIdentityIssue.FieldExtra, 4, "seed", "18"));

            var unknown = StarterRealmIdentityCodec.ParseCanonicalText(
                Canonical + "\nnote=starter");
            AssertEvidence(
                unknown,
                Evidence(StarterRealmIdentityIssue.FieldUnknown, 4, "note", "starter"),
                Evidence(StarterRealmIdentityIssue.FieldExtra, 4, "note", "starter"));
        }

        [Test]
        public void Parse_RejectsWrongFieldOrder()
        {
            var result = StarterRealmIdentityCodec.ParseCanonicalText(
                "realmId=" + RealmId + "\nversion=1\nseed=-17\nlayoutId=" + LayoutId);

            AssertEvidence(
                result,
                Evidence(StarterRealmIdentityIssue.FieldOrderInvalid, 0, null, null));
        }

        [Test]
        public void Parse_RejectsMalformedSyntaxEscapesCarriageReturnsAndTrailingData()
        {
            AssertIssues(
                StarterRealmIdentityCodec.ParseCanonicalText(
                    "version:1\nrealmId=" + RealmId + "\nseed=-17\nlayoutId=" + LayoutId),
                StarterRealmIdentityIssue.FieldSyntaxInvalid,
                StarterRealmIdentityIssue.FieldMissing);
            AssertIssues(
                StarterRealmIdentityCodec.ParseCanonicalText(
                    Canonical.Replace("realm.sylvan", "realm\\.sylvan")),
                StarterRealmIdentityIssue.RealmIdMalformed);
            AssertIssues(
                StarterRealmIdentityCodec.ParseCanonicalText(Canonical.Replace("\n", "\r\n")),
                StarterRealmIdentityIssue.VersionMalformed,
                StarterRealmIdentityIssue.RealmIdMalformed,
                StarterRealmIdentityIssue.SeedMalformed);
            AssertIssues(
                StarterRealmIdentityCodec.ParseCanonicalText(Canonical + "\n"),
                StarterRealmIdentityIssue.TrailingData);
        }

        [Test]
        public void Parse_RejectsMalformedAndUnsupportedVersions()
        {
            AssertIssues(ParseWith("version", "01"), StarterRealmIdentityIssue.VersionMalformed);
            AssertIssues(ParseWith("version", "\"1\""), StarterRealmIdentityIssue.VersionMalformed);
            AssertIssues(ParseWith("version", "2"), StarterRealmIdentityIssue.VersionUnsupported);
            AssertIssues(ParseWith("version", "-1"), StarterRealmIdentityIssue.VersionMalformed);
        }

        [Test]
        public void Parse_RejectsMalformedAndOverflowingSeeds()
        {
            foreach (var malformed in new[] { string.Empty, "+1", "01", "-0", "1.0", "\"1\"" })
            {
                AssertIssues(
                    ParseWith("seed", malformed),
                    StarterRealmIdentityIssue.SeedMalformed);
            }

            AssertIssues(
                ParseWith("seed", "2147483648"),
                StarterRealmIdentityIssue.SeedOverflow);
            AssertIssues(
                ParseWith("seed", "-2147483649"),
                StarterRealmIdentityIssue.SeedOverflow);
        }

        [Test]
        public void Parse_RejectsMissingMalformedAndNonCanonicalRealmIds()
        {
            AssertIssues(ParseWith("realmId", string.Empty),
                StarterRealmIdentityIssue.RealmIdMissing);
            AssertIssues(ParseWith("realmId", "realm..sylvan"),
                StarterRealmIdentityIssue.RealmIdMalformed);
            AssertIssues(ParseWith("realmId", "realm_sylvan"),
                StarterRealmIdentityIssue.RealmIdMalformed);
            AssertIssues(ParseWith("realmId", "Realm.Sylvan"),
                StarterRealmIdentityIssue.RealmIdNonCanonical);
            AssertIssues(ParseWith("realmId", " realm.sylvan "),
                StarterRealmIdentityIssue.RealmIdNonCanonical);
        }

        [Test]
        public void Parse_RejectsMissingMalformedAndNonCanonicalLayoutIds()
        {
            AssertIssues(ParseWith("layoutId", string.Empty),
                StarterRealmIdentityIssue.LayoutIdMissing);
            AssertIssues(ParseWith("layoutId", "layout..alpha"),
                StarterRealmIdentityIssue.LayoutIdMalformed);
            AssertIssues(ParseWith("layoutId", "layout/alpha"),
                StarterRealmIdentityIssue.LayoutIdMalformed);
            AssertIssues(ParseWith("layoutId", "Layout.Alpha"),
                StarterRealmIdentityIssue.LayoutIdNonCanonical);
            AssertIssues(ParseWith("layoutId", " layout.alpha "),
                StarterRealmIdentityIssue.LayoutIdNonCanonical);
        }

        [Test]
        public void Create_FailsClosedForUnsupportedVersionAndInvalidIdsInStableOrder()
        {
            var result = StarterRealmIdentityCodec.Create(
                2,
                "Realm.Sylvan",
                0,
                "layout..alpha");

            AssertIssues(
                result,
                StarterRealmIdentityIssue.VersionUnsupported,
                StarterRealmIdentityIssue.RealmIdNonCanonical,
                StarterRealmIdentityIssue.LayoutIdMalformed);
        }

        [Test]
        public void RejectedEvidenceAndIssueViews_AreImmutableSnapshots()
        {
            var result = StarterRealmIdentityCodec.ParseCanonicalText(
                "version=2\nrealmId=Realm.Sylvan\nseed=01\nlayoutId=layout..alpha");

            AssertIssues(
                result,
                StarterRealmIdentityIssue.VersionUnsupported,
                StarterRealmIdentityIssue.RealmIdNonCanonical,
                StarterRealmIdentityIssue.SeedMalformed,
                StarterRealmIdentityIssue.LayoutIdMalformed);
            Assert.Throws<NotSupportedException>(() => ((IList)result.Evidence).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList)result.Issues).Clear());
        }

        [Test]
        public void Serialize_RejectsMissingRecordRatherThanEmittingAmbiguousText()
        {
            Assert.Throws<ArgumentNullException>(() =>
                StarterRealmIdentityCodec.SerializeCanonicalText(null));
            Assert.Throws<ArgumentNullException>(() =>
                StarterRealmIdentityCodec.SerializeCanonicalUtf8(null));
        }

        private static StarterRealmIdentityRecord ValidRecord(int seed)
        {
            return StarterRealmIdentityCodec.Create(1, RealmId, seed, LayoutId).Record;
        }

        private static StarterRealmIdentityResult ParseWith(string fieldName, string value)
        {
            var source = Canonical;
            var start = source.IndexOf(fieldName + "=", StringComparison.Ordinal);
            var end = source.IndexOf('\n', start);
            if (end < 0)
            {
                end = source.Length;
            }

            return StarterRealmIdentityCodec.ParseCanonicalText(
                source.Substring(0, start)
                + fieldName + "=" + value
                + source.Substring(end));
        }

        private static void AssertMissing(string text, string fieldName)
        {
            AssertEvidence(
                StarterRealmIdentityCodec.ParseCanonicalText(text),
                Evidence(StarterRealmIdentityIssue.FieldMissing, -1, fieldName, null));
        }

        private static void AssertIssues(
            StarterRealmIdentityResult result,
            params StarterRealmIdentityIssue[] issues)
        {
            Assert.That(result.HasRecord, Is.False);
            Assert.That(result.Record, Is.Null);
            CollectionAssert.AreEqual(issues, result.Issues);
        }

        private static void AssertEvidence(
            StarterRealmIdentityResult result,
            params ExpectedEvidence[] expected)
        {
            Assert.That(result.HasRecord, Is.False);
            Assert.That(result.Record, Is.Null);
            Assert.That(result.Evidence.Count, Is.EqualTo(expected.Length));
            for (var index = 0; index < expected.Length; index++)
            {
                Assert.That(result.Evidence[index].Issue, Is.EqualTo(expected[index].Issue));
                Assert.That(result.Evidence[index].FieldIndex,
                    Is.EqualTo(expected[index].FieldIndex));
                Assert.That(result.Evidence[index].FieldName,
                    Is.EqualTo(expected[index].FieldName));
                Assert.That(result.Evidence[index].SuppliedValue,
                    Is.EqualTo(expected[index].Value));
            }
        }

        private static ExpectedEvidence Evidence(
            StarterRealmIdentityIssue issue,
            int fieldIndex,
            string fieldName,
            string value)
        {
            return new ExpectedEvidence(issue, fieldIndex, fieldName, value);
        }

        private sealed class ExpectedEvidence
        {
            public ExpectedEvidence(
                StarterRealmIdentityIssue issue,
                int fieldIndex,
                string fieldName,
                string value)
            {
                Issue = issue;
                FieldIndex = fieldIndex;
                FieldName = fieldName;
                Value = value;
            }

            public StarterRealmIdentityIssue Issue { get; }
            public int FieldIndex { get; }
            public string FieldName { get; }
            public string Value { get; }
        }
    }
}
