using System;

namespace RealmRaiders.Modules.CharacterProceduralMotion
{
    /// <summary>Immutable exact transform names for the six pose-driver bones.</summary>
    public sealed class HumanoidBoneNameMap
    {
        public HumanoidBoneNameMap(
            string leftUpperArm,
            string rightUpperArm,
            string leftThigh,
            string rightThigh,
            string leftCalf,
            string rightCalf)
            : this(leftUpperArm, rightUpperArm, leftThigh, rightThigh, leftCalf, rightCalf, null)
        {
        }

        public HumanoidBoneNameMap(
            string leftUpperArm,
            string rightUpperArm,
            string leftThigh,
            string rightThigh,
            string leftCalf,
            string rightCalf,
            string optionalUpperTorso)
        {
            LeftUpperArm = leftUpperArm;
            RightUpperArm = rightUpperArm;
            LeftThigh = leftThigh;
            RightThigh = rightThigh;
            LeftCalf = leftCalf;
            RightCalf = rightCalf;
            OptionalUpperTorso = optionalUpperTorso;
        }

        public string LeftUpperArm { get; }
        public string RightUpperArm { get; }
        public string LeftThigh { get; }
        public string RightThigh { get; }
        public string LeftCalf { get; }
        public string RightCalf { get; }
        public string OptionalUpperTorso { get; }

        internal bool HasUsableOptionalUpperTorsoName()
        {
            if (string.IsNullOrWhiteSpace(OptionalUpperTorso))
                return false;
            return !string.Equals(OptionalUpperTorso, LeftUpperArm, StringComparison.Ordinal) &&
                   !string.Equals(OptionalUpperTorso, RightUpperArm, StringComparison.Ordinal) &&
                   !string.Equals(OptionalUpperTorso, LeftThigh, StringComparison.Ordinal) &&
                   !string.Equals(OptionalUpperTorso, RightThigh, StringComparison.Ordinal) &&
                   !string.Equals(OptionalUpperTorso, LeftCalf, StringComparison.Ordinal) &&
                   !string.Equals(OptionalUpperTorso, RightCalf, StringComparison.Ordinal);
        }

        internal bool IsValid()
        {
            var names = new[]
            {
                LeftUpperArm, RightUpperArm, LeftThigh,
                RightThigh, LeftCalf, RightCalf
            };
            for (var index = 0; index < names.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(names[index]))
                    return false;
                for (var other = index + 1; other < names.Length; other++)
                {
                    if (string.Equals(names[index], names[other], StringComparison.Ordinal))
                        return false;
                }
            }
            return true;
        }
    }
}
