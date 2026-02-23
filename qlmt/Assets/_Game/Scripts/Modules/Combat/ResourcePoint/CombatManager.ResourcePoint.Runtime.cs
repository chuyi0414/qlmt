using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗组件（背景侧边物资点过程化运行时）。
/// </summary>
public partial class CombatManager
{
    /// <summary>
    /// 过程化点位计划。
    /// </summary>
    private struct ProcPointPlan
    {
        /// <summary>
        /// 世界坐标。
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// 点位类型。
        /// </summary>
        public ProcPointKind Kind;
        /// <summary>
        /// 所在侧边。
        /// </summary>
        public ResourcePointSide Side;
        /// <summary>
        /// 所属分段索引。
        /// </summary>
        public int SegmentIndex;
        /// <summary>
        /// 类型采样值（0~1）。
        /// </summary>
        public float TypeSample;
        /// <summary>
        /// 点位避免重叠半径。
        /// </summary>
        public float AvoidRadius;
    }

    /// <summary>
    /// 背景块创建成功后生成侧边物资点占位。
    /// </summary>
    private void OnBackgroundShownForProcGen(int entityId, int segmentIndex, BgEntity bgEntity)
    {
        if (!_isNoiseSidePointEnabled || bgEntity == null || segmentIndex < 0)
        {
            return;
        }

        ClearMarkerInstances(entityId, bgEntity);
        List<ProcPointPlan> pointPlans = new List<ProcPointPlan>();
        BuildPointPlansForBackground(entityId, segmentIndex, bgEntity.BottomAnchorPosition.y, bgEntity.TopAnchorPosition.y, pointPlans);
        for (int i = 0; i < pointPlans.Count; i++)
        {
            SpawnMarkerByPlan(entityId, bgEntity, pointPlans[i]);
        }
    }

    /// <summary>
    /// 背景块回收前清理占位实例。
    /// </summary>
    private void OnBackgroundWillRecycleForProcGen(int entityId, BgEntity bgEntity)
    {
        ClearMarkerInstances(entityId, bgEntity);
    }

    /// <summary>
    /// 按背景块范围构建连续点流计划。
    /// </summary>
    private void BuildPointPlansForBackground(int ownerEntityId, int segmentIndex, float rawBottomY, float rawTopY, List<ProcPointPlan> pointPlans)
    {
        if (pointPlans == null)
        {
            return;
        }

        pointPlans.Clear();
        // 首个背景块不生成物资点，避免开场遮挡/干扰。
        if (segmentIndex == 0)
        {
            return;
        }

        if (!_isNoiseSidePointEnabled || _procMaxPointsPerBg <= 0)
        {
            return;
        }

        if (!TryGetCameraHorizontalBounds(out float cameraLeftX, out float cameraRightX))
        {
            return;
        }

        float bottomY = Mathf.Min(rawBottomY, rawTopY);
        float topY = Mathf.Max(rawBottomY, rawTopY);
        if (topY <= bottomY)
        {
            return;
        }

        EnsureMainCamera();
        float roadCenterX = (_mainCamera != null ? _mainCamera.transform.position.x : 0f) + _procRoadCenterOffsetX;
        float leftMinX = cameraLeftX + _procSpawnEdgePaddingX;
        float leftMaxX = roadCenterX - _procRoadHalfWidth;
        float rightMinX = roadCenterX + _procRoadHalfWidth;
        float rightMaxX = cameraRightX - _procSpawnEdgePaddingX;

        float leftLength = Mathf.Max(0f, leftMaxX - leftMinX);
        float rightLength = Mathf.Max(0f, rightMaxX - rightMinX);
        float totalLength = leftLength + rightLength;
        if (totalLength <= 0f)
        {
            return;
        }

        System.Random random = CreateSegmentRandom(segmentIndex);
        float firstOffsetSample = BlendWithPerlin01(NextFloat01(random), bottomY, 17);
        float yCursor = bottomY + firstOffsetSample * _procMaxSpawnGapY;
        int acceptedCount = 0;
        int guard = 0;
        int maxGuard = Mathf.Max(64, _procMaxPointsPerBg * 24);
        while (acceptedCount < _procMaxPointsPerBg && guard < maxGuard)
        {
            guard++;

            float gapSample = BlendWithPerlin01(NextFloat01(random), yCursor, 31);
            float gap = Mathf.Lerp(_procMinSpawnGapY, _procMaxSpawnGapY, gapSample);
            yCursor += gap;
            if (yCursor > topY)
            {
                break;
            }

            float typeSample = BlendWithPerlin01(NextFloat01(random), yCursor, 53);
            ProcPointKind pointKind = EvaluateProcPointKindBySample(typeSample);
            if (pointKind == ProcPointKind.None)
            {
                continue;
            }

            if (!TryBuildPointPlan(ownerEntityId, segmentIndex, yCursor, pointKind, typeSample, leftMinX, leftMaxX, rightMinX, rightMaxX, totalLength, pointPlans, random, roadCenterX, out ProcPointPlan pointPlan))
            {
                continue;
            }

            pointPlans.Add(pointPlan);
            acceptedCount++;
        }
    }

    /// <summary>
    /// 构建单个点位计划。
    /// </summary>
    private bool TryBuildPointPlan(
        int ownerEntityId,
        int segmentIndex,
        float yCursor,
        ProcPointKind pointKind,
        float typeSample,
        float leftMinX,
        float leftMaxX,
        float rightMinX,
        float rightMaxX,
        float totalLength,
        List<ProcPointPlan> existingPlans,
        System.Random random,
        float roadCenterX,
        out ProcPointPlan pointPlan)
    {
        pointPlan = default(ProcPointPlan);
        float leftLength = Mathf.Max(0f, leftMaxX - leftMinX);
        float rightLength = Mathf.Max(0f, rightMaxX - rightMinX);
        if (totalLength <= 0f || (leftLength <= 0f && rightLength <= 0f))
        {
            return false;
        }

        for (int retry = 0; retry < 6; retry++)
        {
            float sidePick = BlendWithPerlin01(NextFloat01(random), yCursor + retry * 0.11f, 79 + retry, 0.18f);
            float intervalPick = sidePick * totalLength;
            bool chooseLeft = intervalPick < leftLength;
            float x;
            if (chooseLeft && leftLength > 0f)
            {
                float t = BlendWithPerlin01(NextFloat01(random), yCursor + retry * 0.23f, 101 + retry, 0.12f);
                x = Mathf.Lerp(leftMinX, leftMaxX, t);
            }
            else if (rightLength > 0f)
            {
                float t = BlendWithPerlin01(NextFloat01(random), yCursor + retry * 0.29f, 131 + retry, 0.12f);
                x = Mathf.Lerp(rightMinX, rightMaxX, t);
            }
            else
            {
                continue;
            }

            Vector2 candidatePoint = new Vector2(x, yCursor);
            float candidateRadius = GetProcPointAvoidRadius(pointKind);
            if (!IsCandidateNonOverlapping(ownerEntityId, candidatePoint, candidateRadius, existingPlans))
            {
                continue;
            }

            pointPlan.Position = new Vector3(x, yCursor, 0f);
            pointPlan.Kind = pointKind;
            pointPlan.Side = x < roadCenterX ? ResourcePointSide.Left : ResourcePointSide.Right;
            pointPlan.SegmentIndex = segmentIndex;
            pointPlan.TypeSample = typeSample;
            pointPlan.AvoidRadius = candidateRadius;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 按权重采样点位类型。
    /// </summary>
    private ProcPointKind EvaluateProcPointKindBySample(float sample)
    {
        if (!TryGetNormalizedWeightThresholds(out float noneThreshold, out float smallThreshold))
        {
            return ProcPointKind.None;
        }

        float normalizedSample = Mathf.Clamp01(sample);
        if (normalizedSample < noneThreshold)
        {
            return ProcPointKind.None;
        }

        if (normalizedSample < smallThreshold)
        {
            return ProcPointKind.Small;
        }

        return _procBigWeight > 0f ? ProcPointKind.Big : ProcPointKind.None;
    }

    /// <summary>
    /// 将随机采样与 Perlin 采样混合，增强空间连续性。
    /// </summary>
    private float BlendWithPerlin01(float randomSample, float worldY, int salt, float perlinWeight = 0.5f)
    {
        float perlinSample = SamplePerlin01ByWorld(worldY, salt);
        float weight = Mathf.Clamp01(perlinWeight);
        return Mathf.Clamp01(randomSample * (1f - weight) + perlinSample * weight);
    }

}
