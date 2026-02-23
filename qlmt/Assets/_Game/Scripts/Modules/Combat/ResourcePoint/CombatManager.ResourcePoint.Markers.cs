using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗组件（背景侧边物资点占位实例生命周期）。
/// </summary>
public partial class CombatManager
{
    /// <summary>
    /// 实例化点位计划。
    /// </summary>
    private void SpawnMarkerByPlan(int entityId, BgEntity bgEntity, ProcPointPlan pointPlan)
    {
        GameObject markerPrefab = pointPlan.Kind == ProcPointKind.Big ? _bigMarkerPrefab : _smallMarkerPrefab;
        if (markerPrefab == null || bgEntity == null)
        {
            return;
        }

        Vector3 spawnPosition = new Vector3(pointPlan.Position.x, pointPlan.Position.y, bgEntity.CachedTransform.position.z);
        GameObject markerObject = Object.Instantiate(markerPrefab, spawnPosition, Quaternion.identity, bgEntity.CachedTransform);
        ResourcePointMarker marker = markerObject.GetComponent<ResourcePointMarker>();
        if (marker == null)
        {
            marker = markerObject.AddComponent<ResourcePointMarker>();
        }

        ResourcePointType markerType = pointPlan.Kind == ProcPointKind.Big ? ResourcePointType.Big : ResourcePointType.Small;
        marker.SetMarkerData(markerType, pointPlan.Side, pointPlan.SegmentIndex);
        RegisterMarkerInstance(entityId, markerObject);
    }

    /// <summary>
    /// 记录背景块对应占位实例。
    /// </summary>
    private void RegisterMarkerInstance(int entityId, GameObject markerObject)
    {
        if (!_markerInstancesByEntityId.TryGetValue(entityId, out List<GameObject> markerList))
        {
            markerList = new List<GameObject>();
            _markerInstancesByEntityId.Add(entityId, markerList);
        }

        markerList.Add(markerObject);
    }

    /// <summary>
    /// 清理背景块占位实例。
    /// </summary>
    private void ClearMarkerInstances(int entityId, BgEntity bgEntity)
    {
        if (_markerInstancesByEntityId.TryGetValue(entityId, out List<GameObject> markerList))
        {
            for (int i = 0; i < markerList.Count; i++)
            {
                if (markerList[i] != null)
                {
                    Object.Destroy(markerList[i]);
                }
            }

            markerList.Clear();
            _markerInstancesByEntityId.Remove(entityId);
        }

        if (bgEntity == null)
        {
            return;
        }

        Transform root = bgEntity.CachedTransform;
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            if (child != null && child.GetComponent<ResourcePointMarker>() != null)
            {
                Object.Destroy(child.gameObject);
            }
        }
    }
}
