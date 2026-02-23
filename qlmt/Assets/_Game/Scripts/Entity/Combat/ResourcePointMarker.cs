using UnityEngine;

/// <summary>
/// 物资点占位类型。
/// </summary>
public enum ResourcePointType
{
    Small = 1,
    Big = 2
}

/// <summary>
/// 物资点所在道路侧边。
/// </summary>
public enum ResourcePointSide
{
    Left = 0,
    Right = 1
}

/// <summary>
/// 背景侧边物资点占位标记。
/// </summary>
public class ResourcePointMarker : MonoBehaviour
{
    /// <summary>
    /// 物资点类型。
    /// </summary>
    [SerializeField] private ResourcePointType _pointType = ResourcePointType.Small;
    /// <summary>
    /// 物资点侧边。
    /// </summary>
    [SerializeField] private ResourcePointSide _side = ResourcePointSide.Left;
    /// <summary>
    /// 所属分段索引。
    /// </summary>
    [SerializeField] private int _segmentIndex = -1;

    /// <summary>
    /// 物资点类型。
    /// </summary>
    public ResourcePointType PointType => _pointType;
    /// <summary>
    /// 物资点侧边。
    /// </summary>
    public ResourcePointSide Side => _side;
    /// <summary>
    /// 所属分段索引。
    /// </summary>
    public int SegmentIndex => _segmentIndex;

    /// <summary>
    /// 初始化占位标记数据。
    /// </summary>
    public void SetMarkerData(ResourcePointType pointType, ResourcePointSide side, int segmentIndex)
    {
        _pointType = pointType;
        _side = side;
        _segmentIndex = segmentIndex;
    }
}
