using System;
using System.Security.Cryptography;
using System.Text;

namespace RealmRaiders.Modules.CharacterMotionProfiles
{
    public static class CharacterMotionProfileCanonicalizer
    {
        public static byte[] SerializeUtf8(CharacterMotionProfile profile)
        {
            var issues = CharacterMotionProfileValidator.Validate(profile);
            if (issues.Count != 0)
                throw new ArgumentException("Motion profile must pass deterministic validation before canonical serialization.", nameof(profile));

            var clips = CharacterMotionProfileValidator.CanonicalClips(profile.Clips);
            var json = new StringBuilder(1024);
            json.Append("{\"schemaVersion\":").Append(profile.SchemaVersion);
            AppendString(json, "motionProfileId", profile.MotionProfileId);
            AppendString(json, "family", CharacterMotionProfileValidator.FamilyName(profile.Family));
            AppendString(json, "rigProfileId", profile.RigProfileId);
            AppendString(json, "animatorProfileId", profile.AnimatorProfileId);
            json.Append(",\"clips\":[");
            for (var index = 0; index < clips.Count; index++)
            {
                if (index > 0) json.Append(',');
                var clip = clips[index];
                json.Append("{\"key\":\"").Append(CharacterMotionProfileValidator.ClipKeyName(clip.AssignedKey));
                json.Append("\",\"clipId\":\"").Append(clip.ClipId);
                json.Append("\",\"declaredFamily\":\"").Append(CharacterMotionProfileValidator.FamilyName(clip.DeclaredFamily));
                json.Append("\",\"declaredRigProfileId\":\"").Append(clip.DeclaredRigProfileId);
                json.Append("\",\"declaredKey\":\"").Append(CharacterMotionProfileValidator.ClipKeyName(clip.DeclaredKey));
                json.Append("\"}");
            }
            json.Append(']');
            AppendString(json, "rhythmProfile", CharacterMotionProfileValidator.RhythmName(profile.RhythmProfile));
            AppendString(json, "fallbackProfileId", profile.FallbackProfileId);
            json.Append(",\"sourceIds\":[");
            for (var index = 0; index < profile.SourceIds.Count; index++)
            {
                if (index > 0) json.Append(',');
                json.Append('\"').Append(profile.SourceIds[index]).Append('\"');
            }
            json.Append("]}");
            return new UTF8Encoding(false, true).GetBytes(json.ToString());
        }

        public static string ContentHash(CharacterMotionProfile profile)
        {
            var bytes = SerializeUtf8(profile);
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(bytes);
                var text = new StringBuilder(hash.Length * 2);
                for (var index = 0; index < hash.Length; index++)
                    text.Append(hash[index].ToString("x2"));
                return text.ToString();
            }
        }

        private static void AppendString(StringBuilder json, string field, string value)
        {
            json.Append(",\"").Append(field).Append("\":\"").Append(value).Append('\"');
        }
    }
}
