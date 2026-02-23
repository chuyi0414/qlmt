using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 战斗组件（背景侧边物资点重叠约束）。
/// </summary>
public partial class CombatManager
{
    /// <summary>
    /// 占位 Prefab 无法读取尺寸时的兜底半径。
    /// </summary>
    [SerializeField] private float _procFallbackMarkerAvoidRadius = 0.6f;
    /// <summary>
    /// 小物资点占位半径缓存。
    /// </summary>
    private float _procSmallMarkerAvoidRadius;
    /// <summary>
    /// 大物资点占位半径缓存。
    /// </summary>
    private float _procBigMarkerAvoidRadius;

    /// <summary>
    /// 重置重叠约束运行时。
    /// </summary>
    private void ResetMarkerOverlapRuntime()
    {
        _procSmallMarkerAvoidRadius = 0f;
        _procBigMarkerAvoidRadius = 0f;
    }

    /// <summary>
    /// 刷新占位半径缓存。
    /// </summary>
    private void RefreshMarkerOverlapConfig()
    {
        _procSmallMarkerAvoidRadius = CalculateMarkerPrefabAvoidRadius(_smallMarkerPrefab, "小物资点");
        _procBigMarkerAvoidRadius = CalculateMarkerPrefabAvoidRadius(_bigMarkerPrefab, "大物资点");
    }

    /// <summary>
    /// 获取过程化点位的避免重叠半径。
    /// </summary>
    private float GetProcPointAvoidRadius(ProcPointKind pointKind)
    {
        switch (pointKind)
        {
            case ProcPointKind.Big:
                return Mathf.Max(0.05f, _procBigMarkerAvoidRadius);
            case ProcPointKind.Small:
                return Mathf.Max(0.05f, _procSmallMarkerAvoidRadius);
            default:
                return Mathf.Max(0.05f, _procFallbackMarkerAvoidRadius);
        }
    }

    /// <summary>
    /// 获取已生成占位点的避免重叠半径。
    /// </summary>
    private float GetMarkerTypeAvoidRadius(ResourcePointType markerType)
    {
        switch (markerType)
        {
            case ResourcePointType.Big:
                return Mathf.Max(0.05f, _procBigMarkerAvoidRadius);
            case ResourcePointType.Small:
                return Mathf.Max(0.05f, _procSmallMarkerAvoidRadius);
            default:
                return Mathf.Max(0.05f, _procFallbackMarkerAvoidRadius);
        }
    }

    /// <summary>
    /// 计算占位 Prefab 的平面半径（X/Y 最大外接半径）。
    /// </summary>
    private float CalculateMarkerPrefabAvoidRadius(GameObject prefab, string markerName)
    {
        float fallbackRadius = Mathf.Max(0.05f, _procFallbackMarkerAvoidRadius);
        if (prefab == null)
        {
            return fallbackRadius;
        }

        float radius = 0f;
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            Bounds bounds = renderers[i].bounds;
            float rendererRadius = Mathf.Max(bounds.extents.x, bounds.extents.y);
            if (rendererRadius > radius)
            {
                radius = rendererRadius;
            }
        }

        Collider[] colliders = prefab.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
            {
                continue;
            }

            Bounds bounds = colliders[i].bounds;
            float colliderRadius = Mathf.Max(bounds.extents.x, bounds.extents.y);
            if (colliderRadius > radius)
            {
                radius = colliderRadius;
            }
        }

        if (radius <= 0f)
        {
            Log.Warning("{0}占位 Prefab 半径读取失败，使用兜底值：{1}", markerName, fallbackRadius);
            return fallbackRadius;
        }

        return Mathf.Max(0.05f, radius);
    }

    /// <summary>
    /// 检查候选点是否与同块计划点和已生成点都不重叠。
    /// </summary>
    private bool IsCandidateNonOverlapping(int ownerEntityId, Vector2 candidatePoint, float candidateRadius, List<ProcPointPlan> existingPlans)
    {
        float normalizedCandidateRadius = Mathf.Max(0.05f, candidateRadius);
        if (!IsCandidateFarEnoughFromPlans(candidatePoint, normalizedCandidateRadius, existingPlans))
        {
            return false;
        }

        return IsCandidateFarEnoughFromActiveMarkers(ownerEntityId, candidatePoint, normalizedCandidateRadius);
    }

    /// <summary>
    /// 检查候选点与当前背景块计划点的距离。
    /// </summary>
    private bool IsCandidateFarEnoughFromPlans(Vector2 candidatePoint, float candidateRadius, List<ProcPointPlan> existingPlans)
    {
        if (existingPlans == null || existingPlans.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < existingPlans.Count; i++)
        {
            ProcPointPlan plan = existingPlans[i];
            Vector2 existingPoint = new Vector2(plan.Position.x, plan.Position.y);
            float existingRadius = Mathf.Max(0.05f, plan.AvoidRadius);
            float requiredDistance = candidateRadius + existingRadius + _procMinInBlockPointDistance;
            if ((candidatePoint - existingPoint).sqrMagnitude < requiredDistance * requiredDistance)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 检查候选点与已生成占位点（跨背景块）的距离。
    /// </summary>
    private bool IsCandidateFarEnoughFromActiveMarkers(int ownerEntityId, Vector2 candidatePoint, float candidateRadius)
    {
        foreach (KeyValuePair<int, List<GameObject>> pair in _markerInstancesByEntityId)
        {
            if (pair.Key == ownerEntityId || pair.Value == null)
            {
                continue;
            }

            List<GameObject> markerList = pair.Value;
            for (int i = 0; i < markerList.Count; i++)
            {
                GameObject markerObject = markerList[i];
                if (markerObject == null)
                {
                    continue;
                }

                ResourcePointMarker marker = markerObject.GetComponent<ResourcePointMarker>();
                if (marker == null)
                {
                    continue;
                }

                float existingRadius = GetMarkerTypeAvoidRadius(marker.PointType);
                float requiredDistance = candidateRadius + existingRadius + _procMinInBlockPointDistance;
                Vector3 markerPosition = markerObject.transform.position;
                Vector2 existingPoint = new Vector2(markerPosition.x, markerPosition.y);
                if ((candidatePoint - existingPoint).sqrMagnitude < requiredDistance * requiredDistance)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
