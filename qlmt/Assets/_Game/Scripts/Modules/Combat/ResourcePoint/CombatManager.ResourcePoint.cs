using GameFramework.DataTable;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 战斗组件（背景侧边物资点过程化配置）。
/// </summary>
public partial class CombatManager
{
    /// <summary>
    /// 过程化抽样结果。
    /// </summary>
    private enum ProcPointKind
    {
        None = 0,
        Small = 1,
        Big = 2
    }

    /// <summary>
    /// 是否启用噪点侧边物资点。
    /// </summary>
    private bool _isNoiseSidePointEnabled;
    /// <summary>
    /// 下一个背景块分段索引。
    /// </summary>
    private int _nextSegmentIndex;
    /// <summary>
    /// 固定随机种子。
    /// </summary>
    private int _procSeed;
    /// <summary>
    /// Perlin 频率。
    /// </summary>
    private float _procPerlinFrequency;
    /// <summary>
    /// 道路半宽。
    /// </summary>
    private float _procRoadHalfWidth;
    /// <summary>
    /// 生成区域边缘留白。
    /// </summary>
    private float _procSpawnEdgePaddingX;
    /// <summary>
    /// 不生成权重。
    /// </summary>
    private float _procNoneWeight;
    /// <summary>
    /// 小物资点权重。
    /// </summary>
    private float _procSmallWeight;
    /// <summary>
    /// 大物资点权重。
    /// </summary>
    private float _procBigWeight;
    /// <summary>
    /// 最小纵向间距。
    /// </summary>
    private float _procMinSpawnGapY;
    /// <summary>
    /// 最大纵向间距。
    /// </summary>
    private float _procMaxSpawnGapY;
    /// <summary>
    /// 单个背景块最大点位数。
    /// </summary>
    private int _procMaxPointsPerBg;
    /// <summary>
    /// 同块最小点间距。
    /// </summary>
    private float _procMinInBlockPointDistance;
    /// <summary>
    /// 道路中心 X 偏移。
    /// </summary>
    private float _procRoadCenterOffsetX;

    /// <summary>
    /// 小物资点占位 Prefab。
    /// </summary>
    private GameObject _smallMarkerPrefab;
    /// <summary>
    /// 大物资点占位 Prefab。
    /// </summary>
    private GameObject _bigMarkerPrefab;

    /// <summary>
    /// 背景实体对应的占位实例列表。
    /// </summary>
    private readonly Dictionary<int, List<GameObject>> _markerInstancesByEntityId = new Dictionary<int, List<GameObject>>();

    /// <summary>
    /// 重置噪点侧边物资点运行时。
    /// </summary>
    private void ResetRoadProcGenRuntime()
    {
        _isNoiseSidePointEnabled = false;
        _nextSegmentIndex = 0;
        _procSeed = 0;
        _procPerlinFrequency = 0f;
        _procRoadHalfWidth = 0f;
        _procSpawnEdgePaddingX = 0f;
        _procNoneWeight = 0f;
        _procSmallWeight = 0f;
        _procBigWeight = 0f;
        _procMinSpawnGapY = 0f;
        _procMaxSpawnGapY = 0f;
        _procMaxPointsPerBg = 0;
        _procMinInBlockPointDistance = 0f;
        _procRoadCenterOffsetX = 0f;
        _smallMarkerPrefab = null;
        _bigMarkerPrefab = null;
        _markerInstancesByEntityId.Clear();
        ResetMarkerOverlapRuntime();
    }

    /// <summary>
    /// 根据关卡行配置噪点侧边物资点参数。
    /// </summary>
    private void ConfigureRoadProcGenByLevel(DRLevel levelRow)
    {
        ResetRoadProcGenRuntime();
        if (levelRow == null || !levelRow.UseNoiseSidePoints)
        {
            return;
        }

        IDataTable<DRBgRoadGenProfile> profileTable = GameEntry.DataTable.GetDataTable<DRBgRoadGenProfile>();
        DRBgRoadGenProfile profileRow = profileTable != null ? profileTable.GetDataRow(levelRow.BgRoadGenProfileId) : null;
        if (profileRow == null)
        {
            Log.Error("背景道路生成配置不存在：{0}", levelRow.BgRoadGenProfileId);
            return;
        }

        _procSeed = profileRow.Seed;
        _procPerlinFrequency = profileRow.PerlinFrequency;
        _procRoadHalfWidth = profileRow.RoadHalfWidth;
        _procSpawnEdgePaddingX = profileRow.SpawnEdgePaddingX;
        _procNoneWeight = profileRow.NoneWeight;
        _procSmallWeight = profileRow.SmallWeight;
        _procBigWeight = profileRow.BigWeight;
        _procMinSpawnGapY = profileRow.MinSpawnGapY;
        _procMaxSpawnGapY = profileRow.MaxSpawnGapY;
        _procMaxPointsPerBg = profileRow.MaxPointsPerBg;
        _procMinInBlockPointDistance = profileRow.MinInBlockPointDistance;
        _procRoadCenterOffsetX = profileRow.RoadCenterOffsetX;
        _smallMarkerPrefab = LoadMarkerPrefab(profileRow.SmallMarkerPath, "小物资点");
        _bigMarkerPrefab = LoadMarkerPrefab(profileRow.BigMarkerPath, "大物资点");
        RefreshMarkerOverlapConfig();
        _isNoiseSidePointEnabled = true;
    }

    /// <summary>
    /// 获取并递增分段索引。
    /// </summary>
    private int TakeNextSegmentIndex()
    {
        int segmentIndex = _nextSegmentIndex;
        _nextSegmentIndex++;
        return segmentIndex;
    }

    /// <summary>
    /// 加载占位 Prefab。
    /// </summary>
    private GameObject LoadMarkerPrefab(string markerPath, string markerName)
    {
        if (string.IsNullOrEmpty(markerPath))
        {
            Log.Error("{0}占位路径为空。", markerName);
            return null;
        }

        string assetPath = GameAssetPath.GetEntity(markerPath);
        GameObject prefab = Resources.Load<GameObject>(assetPath);
        if (prefab == null)
        {
            Log.Error("{0}占位 Prefab 加载失败：{1}", markerName, assetPath);
        }

        return prefab;
    }

    /// <summary>
    /// 获取权重阈值（归一化到 0~1）。
    /// </summary>
    /// <param name="noneThreshold">不生成阈值上界。</param>
    /// <param name="smallThreshold">小物资点阈值上界。</param>
    /// <returns>权重可用返回 true。</returns>
    private bool TryGetNormalizedWeightThresholds(out float noneThreshold, out float smallThreshold)
    {
        noneThreshold = 1f;
        smallThreshold = 1f;

        float totalWeight = _procNoneWeight + _procSmallWeight + _procBigWeight;
        if (totalWeight <= 0f)
        {
            return false;
        }

        noneThreshold = _procNoneWeight / totalWeight;
        smallThreshold = (_procNoneWeight + _procSmallWeight) / totalWeight;
        return true;
    }

    /// <summary>
    /// 计算主相机水平边界。
    /// </summary>
    private bool TryGetCameraHorizontalBounds(out float leftX, out float rightX)
    {
        leftX = 0f;
        rightX = 0f;
        EnsureMainCamera();
        if (_mainCamera == null)
        {
            return false;
        }

        float halfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
        float cameraX = _mainCamera.transform.position.x;
        leftX = cameraX - halfWidth;
        rightX = cameraX + halfWidth;
        return true;
    }

}
