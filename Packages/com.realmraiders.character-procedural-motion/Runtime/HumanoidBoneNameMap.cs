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
        {
            LeftUpperArm = leftUpperArm;
            RightUpperArm = rightUpperArm;
            LeftThigh = leftThigh;
            RightThigh = rightThigh;
            LeftCalf = leftCalf;
            RightCalf = rightCalf;
        }

        public string LeftUpperArm { get; }
        public string RightUpperArm { get; }
        public string LeftThigh { get; }
        public string RightThigh { get; }
        public string LeftCalf { get; }
        public string RightCalf { get; }

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
