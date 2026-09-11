using System;

namespace RealmRaiders.Modules.CharacterProceduralMotion
{
    /// <summary>Immutable explicit tuning preference for one stable character or recipe identifier.</summary>
    public sealed class ProceduralHumanoidTuningAssignment
    {
        public ProceduralHumanoidTuningAssignment(
            string characterOrRecipeId,
            string preferredProfileId,
            string fallbackProfileId)
        {
            CharacterOrRecipeId = characterOrRecipeId;
            PreferredProfileId = preferredProfileId;
            FallbackProfileId = fallbackProfileId;
        }

        public string CharacterOrRecipeId { get; }

        public string PreferredProfileId { get; }

        public string FallbackProfileId { get; }
    }

    public enum ProceduralHumanoidTuningResolution
    {
        Preferred,
        Fallback,
        Rejected
    }

    public enum ProceduralHumanoidTuningRejectionReason
    {
        None,
        NullCatalogue,
        NullAssignment,
        InvalidCharacterOrRecipeId,
        InvalidPreferredProfileId,
        InvalidFallbackProfileId,
        PreferredProfileHasNullTuning,
        FallbackProfileHasNullTuning,
        NoAssignedProfileAvailable
    }

    /// <summary>Immutable passive resolution output. The caller remains responsible for any later application.</summary>
    public sealed class ProceduralHumanoidTuningAssignmentResult
    {
        internal ProceduralHumanoidTuningAssignmentResult(
            ProceduralHumanoidTuningResolution resolution,
            ProceduralHumanoidTuningRejectionReason rejectionReason,
            ProceduralHumanoidTuningProfile profile)
        {
            Resolution = resolution;
            RejectionReason = rejectionReason;
            Profile = profile;
        }

        public ProceduralHumanoidTuningResolution Resolution { get; }

        public ProceduralHumanoidTuningRejectionReason RejectionReason { get; }

        public ProceduralHumanoidTuningProfile Profile { get; }

        public ProceduralHumanoidMotionTuning Tuning => Profile == null
            ? null
            : Profile.Tuning;

        public string Signature => Resolution + "|" + RejectionReason + "|"
            + (Profile == null ? "-" : Profile.ProfileId);
    }

    /// <summary>
    /// Resolves one caller-selected assignment against one already-built catalogue. It performs
    /// neither provider discovery nor automatic runtime tuning application.
    /// </summary>
    public static class ProceduralHumanoidTuningAssignmentResolver
    {
        public static ProceduralHumanoidTuningAssignmentResult Resolve(
            ProceduralHumanoidTuningCatalogue catalogue,
            ProceduralHumanoidTuningAssignment assignment)
        {
            if (catalogue == null)
            {
                return Rejected(
                    ProceduralHumanoidTuningRejectionReason.NullCatalogue);
            }

            if (assignment == null)
            {
                return Rejected(
                    ProceduralHumanoidTuningRejectionReason.NullAssignment);
            }

            var invalidReason = InvalidAssignmentReason(assignment);
            if (invalidReason != ProceduralHumanoidTuningRejectionReason.None)
            {
                return Rejected(invalidReason);
            }

            if (catalogue.TryGetByProfileId(
                assignment.PreferredProfileId,
                out var preferred))
            {
                return preferred == null || preferred.Tuning == null
                    ? Rejected(
                        ProceduralHumanoidTuningRejectionReason.PreferredProfileHasNullTuning)
                    : Resolved(
                        ProceduralHumanoidTuningResolution.Preferred,
                        preferred);
            }

            if (catalogue.TryGetByProfileId(
                assignment.FallbackProfileId,
                out var fallback))
            {
                return fallback == null || fallback.Tuning == null
                    ? Rejected(
                        ProceduralHumanoidTuningRejectionReason.FallbackProfileHasNullTuning)
                    : Resolved(
                        ProceduralHumanoidTuningResolution.Fallback,
                        fallback);
            }

            return Rejected(
                ProceduralHumanoidTuningRejectionReason.NoAssignedProfileAvailable);
        }

        private static ProceduralHumanoidTuningAssignmentResult Resolved(
            ProceduralHumanoidTuningResolution resolution,
            ProceduralHumanoidTuningProfile profile)
        {
            return new ProceduralHumanoidTuningAssignmentResult(
                resolution,
                ProceduralHumanoidTuningRejectionReason.None,
                profile);
        }

        private static ProceduralHumanoidTuningAssignmentResult Rejected(
            ProceduralHumanoidTuningRejectionReason reason)
        {
            return new ProceduralHumanoidTuningAssignmentResult(
                ProceduralHumanoidTuningResolution.Rejected,
                reason,
                null);
        }

        private static ProceduralHumanoidTuningRejectionReason InvalidAssignmentReason(
            ProceduralHumanoidTuningAssignment assignment)
        {
            if (!IsStableId(assignment.CharacterOrRecipeId))
            {
                return ProceduralHumanoidTuningRejectionReason.InvalidCharacterOrRecipeId;
            }

            if (!IsStableId(assignment.PreferredProfileId))
            {
                return ProceduralHumanoidTuningRejectionReason.InvalidPreferredProfileId;
            }

            if (!IsStableId(assignment.FallbackProfileId))
            {
                return ProceduralHumanoidTuningRejectionReason.InvalidFallbackProfileId;
            }

            return ProceduralHumanoidTuningRejectionReason.None;
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!IsAlphaNumeric(value[0]) || !IsAlphaNumeric(value[value.Length - 1]))
            {
                return false;
            }

            var previousSeparator = false;
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                var separator = character == '.' || character == '-';
                if ((!IsAlphaNumeric(character) && !separator)
                    || (separator && previousSeparator))
                {
                    return false;
                }

                previousSeparator = separator;
            }

            return true;
        }

        private static bool IsAlphaNumeric(char character)
        {
            return (character >= 'a' && character <= 'z')
                || (character >= '0' && character <= '9');
        }
    }
}
