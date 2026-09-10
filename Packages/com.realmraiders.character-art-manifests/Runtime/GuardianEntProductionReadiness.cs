using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RealmRaiders.Modules.CharacterArtManifests
{
    public enum GuardianEntReadinessTier
    {
        TemporaryVisualPilot,
        ProductionReady
    }

    public enum GuardianEntReadinessIssue
    {
        SourceHashMismatch,
        InvalidMeasurement,
        MissingLod0,
        TriangleBudgetExceeded,
        MissingLod1,
        MissingLod2,
        Lod1BudgetExceeded,
        Lod2BudgetExceeded,
        BoneBudgetExceeded,
        InfluenceBudgetExceeded,
        MaterialBudgetExceeded,
        SampledBaseColorBudgetExceeded,
        PilotExceptionRequired,
        PilotMustBeAlbedoOnly,
        PilotMustDisableAnimationPhysicsAndColliders
    }

    public sealed class GuardianEntSourceMeasurement
    {
        public GuardianEntSourceMeasurement(
            string fbxHash,
            string albedoHash,
            string normalHash,
            string maskHash,
            int triangles,
            int lod1Triangles,
            int lod2Triangles,
            int controlPoints,
            int deformBones,
            int maxInfluences,
            int pointsOverFourInfluences,
            int materialCount,
            int sourceTextureEdge,
            int sampledBaseColorMaxSize,
            bool animationEnabled,
            bool physicsEnabled,
            bool collidersEnabled,
            bool albedoOnly,
            bool explicitPilotException)
        {
            FbxHash = fbxHash;
            AlbedoHash = albedoHash;
            NormalHash = normalHash;
            MaskHash = maskHash;
            Triangles = triangles;
            Lod1Triangles = lod1Triangles;
            Lod2Triangles = lod2Triangles;
            ControlPoints = controlPoints;
            DeformBones = deformBones;
            MaxInfluences = maxInfluences;
            PointsOverFourInfluences = pointsOverFourInfluences;
            MaterialCount = materialCount;
            SourceTextureEdge = sourceTextureEdge;
            SampledBaseColorMaxSize = sampledBaseColorMaxSize;
            AnimationEnabled = animationEnabled;
            PhysicsEnabled = physicsEnabled;
            CollidersEnabled = collidersEnabled;
            AlbedoOnly = albedoOnly;
            ExplicitPilotException = explicitPilotException;
        }

        public string FbxHash { get; }

        public string AlbedoHash { get; }

        public string NormalHash { get; }

        public string MaskHash { get; }

        public int Triangles { get; }

        public int Lod1Triangles { get; }

        public int Lod2Triangles { get; }

        public int ControlPoints { get; }

        public int DeformBones { get; }

        public int MaxInfluences { get; }

        public int PointsOverFourInfluences { get; }

        public int MaterialCount { get; }

        public int SourceTextureEdge { get; }

        public int SampledBaseColorMaxSize { get; }

        public bool AnimationEnabled { get; }

        public bool PhysicsEnabled { get; }

        public bool CollidersEnabled { get; }

        public bool AlbedoOnly { get; }

        public bool ExplicitPilotException { get; }
    }

    public sealed class GuardianEntReadinessResult
    {
        internal GuardianEntReadinessResult(List<GuardianEntReadinessIssue> issues)
        {
            Issues = new ReadOnlyCollection<GuardianEntReadinessIssue>(issues);
        }

        public bool IsReady => Issues.Count == 0;

        public IReadOnlyList<GuardianEntReadinessIssue> Issues { get; }
    }

    public static class GuardianEntProductionReadiness
    {
        public const string Tree01FbxHash = "bd90b4f8dd823334cbb226229ef731c1280ae601ab942d16193b96f5f674a02e";
        public const string Tree01AlbedoHash = "cd50e1a9179f1242a862f6fb7f8448cff5cbbd56d599e687d916dc8583625573";
        public const string Tree01NormalHash = "1b0372d5b2797d520c33cfa2839b46f4536f2ce15104e23f5c50cb4a1b503d2f";
        public const string Tree01MaskHash = "72b6fe38c9ca4f1b6a7f6f457a73b4900a7897eb3129a4079472eec5c48e093c";

        public static GuardianEntReadinessResult Evaluate(
            GuardianEntReadinessTier tier,
            GuardianEntSourceMeasurement source)
        {
            var issues = new List<GuardianEntReadinessIssue>();

            if (source == null || !HasExpectedSourceHashes(source))
            {
                issues.Add(GuardianEntReadinessIssue.SourceHashMismatch);
            }

            if (source != null && HasInvalidMeasurement(source))
            {
                issues.Add(GuardianEntReadinessIssue.InvalidMeasurement);
            }

            if (tier == GuardianEntReadinessTier.TemporaryVisualPilot)
            {
                AddPilotIssues(source, issues);
            }
            else if (tier == GuardianEntReadinessTier.ProductionReady)
            {
                AddProductionIssues(source, issues);
            }
            else
            {
                issues.Add(GuardianEntReadinessIssue.InvalidMeasurement);
            }

            return new GuardianEntReadinessResult(issues);
        }

        private static void AddPilotIssues(
            GuardianEntSourceMeasurement source,
            List<GuardianEntReadinessIssue> issues)
        {
            if (source == null || !source.ExplicitPilotException)
            {
                issues.Add(GuardianEntReadinessIssue.PilotExceptionRequired);
            }

            if (source == null || !source.AlbedoOnly)
            {
                issues.Add(GuardianEntReadinessIssue.PilotMustBeAlbedoOnly);
            }

            if (source == null || source.AnimationEnabled || source.PhysicsEnabled || source.CollidersEnabled)
            {
                issues.Add(GuardianEntReadinessIssue.PilotMustDisableAnimationPhysicsAndColliders);
            }
        }

        private static void AddProductionIssues(
            GuardianEntSourceMeasurement source,
            List<GuardianEntReadinessIssue> issues)
        {
            if (source == null)
            {
                issues.Add(GuardianEntReadinessIssue.MissingLod0);
                return;
            }

            AddLod0Issue(source.Triangles, issues);
            AddLod1Issue(source.Lod1Triangles, issues);
            AddLod2Issue(source.Lod2Triangles, issues);

            if (source.DeformBones > 48)
            {
                issues.Add(GuardianEntReadinessIssue.BoneBudgetExceeded);
            }

            if (source.MaxInfluences > 4 || source.PointsOverFourInfluences > 0)
            {
                issues.Add(GuardianEntReadinessIssue.InfluenceBudgetExceeded);
            }

            if (source.MaterialCount != 1)
            {
                issues.Add(GuardianEntReadinessIssue.MaterialBudgetExceeded);
            }

            if (source.SampledBaseColorMaxSize > 1024)
            {
                issues.Add(GuardianEntReadinessIssue.SampledBaseColorBudgetExceeded);
            }
        }

        private static void AddLod0Issue(
            int triangles,
            List<GuardianEntReadinessIssue> issues)
        {
            if (triangles == -1)
            {
                issues.Add(GuardianEntReadinessIssue.MissingLod0);
            }
            else if (triangles > 4000)
            {
                issues.Add(GuardianEntReadinessIssue.TriangleBudgetExceeded);
            }
        }

        private static void AddLod1Issue(
            int triangles,
            List<GuardianEntReadinessIssue> issues)
        {
            if (triangles == -1)
            {
                issues.Add(GuardianEntReadinessIssue.MissingLod1);
            }
            else if (triangles > 2000)
            {
                issues.Add(GuardianEntReadinessIssue.Lod1BudgetExceeded);
            }
        }

        private static void AddLod2Issue(
            int triangles,
            List<GuardianEntReadinessIssue> issues)
        {
            if (triangles == -1)
            {
                issues.Add(GuardianEntReadinessIssue.MissingLod2);
            }
            else if (triangles > 800)
            {
                issues.Add(GuardianEntReadinessIssue.Lod2BudgetExceeded);
            }
        }

        private static bool HasExpectedSourceHashes(GuardianEntSourceMeasurement source)
        {
            return Exact(source.FbxHash, Tree01FbxHash)
                && Exact(source.AlbedoHash, Tree01AlbedoHash)
                && Exact(source.NormalHash, Tree01NormalHash)
                && Exact(source.MaskHash, Tree01MaskHash);
        }

        private static bool HasInvalidMeasurement(GuardianEntSourceMeasurement source)
        {
            return source.Triangles < -1
                || source.Triangles == 0
                || IsInvalidOptionalLod(source.Lod1Triangles)
                || IsInvalidOptionalLod(source.Lod2Triangles)
                || source.ControlPoints <= 0
                || source.DeformBones <= 0
                || source.MaxInfluences <= 0
                || source.PointsOverFourInfluences < 0
                || source.PointsOverFourInfluences > source.ControlPoints
                || source.MaterialCount <= 0
                || source.SourceTextureEdge <= 0
                || source.SampledBaseColorMaxSize <= 0;
        }

        private static bool IsInvalidOptionalLod(int triangles)
        {
            return triangles < -1 || triangles == 0;
        }

        private static bool Exact(string actual, string expected)
        {
            return string.Equals(actual, expected, StringComparison.Ordinal);
        }
    }
}
