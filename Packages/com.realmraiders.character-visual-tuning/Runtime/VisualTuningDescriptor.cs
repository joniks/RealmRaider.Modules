using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.CharacterVisualTuning
{
    public enum MaterialRole
    {
        PrimaryArmor,
        SecondaryArmor,
        Cloth,
        Accent
    }

    public struct AxisValues
    {
        public AxisValues(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
    }

    public sealed class PresentationTransformIntent
    {
        public PresentationTransformIntent(
            AxisValues? localPosition = null,
            AxisValues? localEulerAngles = null,
            AxisValues? localScale = null)
        {
            LocalPosition = localPosition;
            LocalEulerAngles = localEulerAngles;
            LocalScale = localScale;
        }

        public AxisValues? LocalPosition { get; }
        public AxisValues? LocalEulerAngles { get; }
        public AxisValues? LocalScale { get; }
        public bool IsNoOp => !LocalPosition.HasValue && !LocalEulerAngles.HasValue && !LocalScale.HasValue;
    }

    public sealed class MaterialPaletteIntent
    {
        public MaterialPaletteIntent(MaterialRole role, string direction)
        {
            Role = role;
            Direction = direction;
        }

        public MaterialRole Role { get; }
        public string Direction { get; }
    }

    public sealed class MobileVisualBudget
    {
        public MobileVisualBudget(int materialCount, int textureCount, int maxTextureEdgePixels, int triangleCount)
        {
            MaterialCount = materialCount;
            TextureCount = textureCount;
            MaxTextureEdgePixels = maxTextureEdgePixels;
            TriangleCount = triangleCount;
        }

        public int MaterialCount { get; }
        public int TextureCount { get; }
        public int MaxTextureEdgePixels { get; }
        public int TriangleCount { get; }
    }

    public sealed class CharacterVisualTuningProfile
    {
        private readonly ReadOnlyCollection<MaterialPaletteIntent> palette;

        public CharacterVisualTuningProfile(
            string profileId,
            string sourceId,
            string displayName,
            PresentationTransformIntent presentationTransform,
            IEnumerable<MaterialPaletteIntent> palette,
            MobileVisualBudget budget)
        {
            ProfileId = profileId;
            SourceId = sourceId;
            DisplayName = displayName;
            PresentationTransform = presentationTransform;
            this.palette = new ReadOnlyCollection<MaterialPaletteIntent>(
                palette == null ? new List<MaterialPaletteIntent>() : new List<MaterialPaletteIntent>(palette));
            Budget = budget;
        }

        public string ProfileId { get; }
        public string SourceId { get; }
        public string DisplayName { get; }
        public PresentationTransformIntent PresentationTransform { get; }
        public IReadOnlyList<MaterialPaletteIntent> Palette => palette;
        public MobileVisualBudget Budget { get; }
    }

    public enum VisualTuningIssueCode
    {
        MissingProfileId,
        InvalidProfileId,
        MissingSourceId,
        InvalidSourceId,
        MissingDisplayName,
        MissingPresentationTransform,
        TransformOverrideRequiresUnityReview,
        InvalidTransformValue,
        MissingPalette,
        MissingPaletteDirection,
        DuplicateMaterialRole,
        MissingBudget,
        InvalidMaterialCount,
        InvalidTextureCount,
        InvalidTextureEdge,
        InvalidTriangleCount
    }

    public sealed class VisualTuningIssue
    {
        public VisualTuningIssue(VisualTuningIssueCode code, string path)
        {
            Code = code;
            Path = path;
        }

        public VisualTuningIssueCode Code { get; }
        public string Path { get; }
    }

    public static class VisualTuningValidator
    {
        public static IReadOnlyList<VisualTuningIssue> Validate(CharacterVisualTuningProfile profile)
        {
            var issues = new List<VisualTuningIssue>();
            if (profile == null)
            {
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.MissingProfileId, "profile"));
                return issues.AsReadOnly();
            }

            ValidateId(profile.ProfileId, "profileId", VisualTuningIssueCode.MissingProfileId, VisualTuningIssueCode.InvalidProfileId, issues);
            ValidateId(profile.SourceId, "sourceId", VisualTuningIssueCode.MissingSourceId, VisualTuningIssueCode.InvalidSourceId, issues);

            if (string.IsNullOrWhiteSpace(profile.DisplayName))
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.MissingDisplayName, "displayName"));

            if (profile.PresentationTransform == null)
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.MissingPresentationTransform, "presentationTransform"));
            else if (!profile.PresentationTransform.IsNoOp)
            {
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.TransformOverrideRequiresUnityReview, "presentationTransform"));
                ValidateTransform(profile.PresentationTransform, issues);
            }

            var roles = new HashSet<MaterialRole>();
            if (profile.Palette.Count == 0)
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.MissingPalette, "palette"));
            for (var index = 0; index < profile.Palette.Count; index++)
            {
                var intent = profile.Palette[index];
                var path = "palette[" + index + "]";
                if (intent == null || string.IsNullOrWhiteSpace(intent.Direction))
                    issues.Add(new VisualTuningIssue(VisualTuningIssueCode.MissingPaletteDirection, path + ".direction"));
                if (intent != null && !roles.Add(intent.Role))
                    issues.Add(new VisualTuningIssue(VisualTuningIssueCode.DuplicateMaterialRole, path + ".role"));
            }

            ValidateBudget(profile.Budget, issues);
            return issues.AsReadOnly();
        }

        private static void ValidateTransform(PresentationTransformIntent transform, ICollection<VisualTuningIssue> issues)
        {
            ValidateAxis(transform.LocalPosition, "presentationTransform.localPosition", false, issues);
            ValidateAxis(transform.LocalEulerAngles, "presentationTransform.localEulerAngles", false, issues);
            ValidateAxis(transform.LocalScale, "presentationTransform.localScale", true, issues);
        }

        private static void ValidateAxis(AxisValues? axis, string path, bool requirePositive, ICollection<VisualTuningIssue> issues)
        {
            if (!axis.HasValue)
                return;

            var value = axis.Value;
            if (!IsFinite(value.X) || !IsFinite(value.Y) || !IsFinite(value.Z) ||
                (requirePositive && (value.X <= 0f || value.Y <= 0f || value.Z <= 0f)))
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.InvalidTransformValue, path));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void ValidateId(string value, string path, VisualTuningIssueCode missing, VisualTuningIssueCode invalid, ICollection<VisualTuningIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                issues.Add(new VisualTuningIssue(missing, path));
                return;
            }

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if ((character < 'a' || character > 'z') && (character < '0' || character > '9') && character != '.' && character != '-')
                {
                    issues.Add(new VisualTuningIssue(invalid, path));
                    return;
                }
            }
        }

        private static void ValidateBudget(MobileVisualBudget budget, ICollection<VisualTuningIssue> issues)
        {
            if (budget == null)
            {
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.MissingBudget, "budget"));
                return;
            }

            if (budget.MaterialCount < 1)
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.InvalidMaterialCount, "budget.materialCount"));
            if (budget.TextureCount < 1)
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.InvalidTextureCount, "budget.textureCount"));
            if (budget.MaxTextureEdgePixels < 1)
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.InvalidTextureEdge, "budget.maxTextureEdgePixels"));
            if (budget.TriangleCount < 1)
                issues.Add(new VisualTuningIssue(VisualTuningIssueCode.InvalidTriangleCount, "budget.triangleCount"));
        }
    }
}
