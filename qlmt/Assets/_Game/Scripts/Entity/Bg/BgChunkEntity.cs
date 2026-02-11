using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 背景块实体逻辑。
/// 该实体负责提供上下边界锚点与基础位移能力，供背景滚动系统进行无缝拼接与移动。
/// </summary>
public sealed class BgChunkEntity : EntityLogic
{
    /// <summary>
    /// 背景块顶部锚点。
    /// 用于计算该块当前顶部世界坐标。
    /// </summary>
    [SerializeField]
    private Transform _topAnchor;

    /// <summary>
    /// 背景块底部锚点。
    /// 用于计算该块当前底部世界坐标。
    /// </summary>
    [SerializeField]
    private Transform _bottomAnchor;

    /// <summary>
    /// 是否已经尝试过自动修复锚点引用。
    /// 该标记用于避免每帧重复执行查找逻辑。
    /// </summary>
    private bool _isAnchorAutoFixTried;

    /// <summary>
    /// 自动查找锚点时使用的顶部锚点名称。
    /// </summary>
    private const string TopAnchorName = "_topAnchor";

    /// <summary>
    /// 自动查找锚点时使用的底部锚点名称。
    /// </summary>
    private const string BottomAnchorName = "_bottomAnchor";

    /// <summary>
    /// 获取背景块顶部世界坐标 Y。
    /// </summary>
    public float TopY
    {
        get
        {
            TryAutoResolveAnchorsIfNeeded();

            if (_topAnchor != null)
            {
                return _topAnchor.position.y;
            }

            return CachedTransform.position.y + Length * 0.5f;
        }
    }

    /// <summary>
    /// 获取背景块底部世界坐标 Y。
    /// </summary>
    public float BottomY
    {
        get
        {
            TryAutoResolveAnchorsIfNeeded();

            if (_bottomAnchor != null)
            {
                return _bottomAnchor.position.y;
            }

            return CachedTransform.position.y - Length * 0.5f;
        }
    }

    /// <summary>
    /// 获取背景块高度（世界单位）。
    /// </summary>
    public float Length
    {
        get
        {
            TryAutoResolveAnchorsIfNeeded();
            return _topAnchor.position.y - _bottomAnchor.position.y;
        }
    }

    /// <summary>
    /// 将背景块底边对齐到指定世界坐标 Y。
    /// </summary>
    /// <param name="targetBottomY">目标底边世界坐标 Y。</param>
    public void SnapBottomTo(float targetBottomY)
    {
        float offsetY = targetBottomY - BottomY;
        CachedTransform.position += Vector3.up * offsetY;
    }

    /// <summary>
    /// 让背景块沿 Y 轴负方向移动。
    /// </summary>
    /// <param name="distance">移动距离（世界单位，要求非负）。</param>
    public void MoveDown(float distance)
    {
        if (distance <= 0f)
        {
            return;
        }

        CachedTransform.position += Vector3.down * distance;
    }

    /// <summary>
    /// 在锚点引用缺失或错误时，自动尝试从子节点修复锚点引用。
    /// 修复策略：
    /// 1) 优先按名称查找 _topAnchor 与 _bottomAnchor；
    /// 2) 若仍缺失，则按子节点局部 Y 最大/最小进行推断。
    /// </summary>
    private void TryAutoResolveAnchorsIfNeeded()
    {
        if (_isAnchorAutoFixTried)
        {
            return;
        }

        _isAnchorAutoFixTried = true;

        bool needFix = _topAnchor == null || _bottomAnchor == null || _topAnchor == _bottomAnchor;
        if (!needFix)
        {
            return;
        }

        Transform topByName = CachedTransform.Find(TopAnchorName);
        Transform bottomByName = CachedTransform.Find(BottomAnchorName);

        if (topByName != null)
        {
            _topAnchor = topByName;
        }

        if (bottomByName != null)
        {
            _bottomAnchor = bottomByName;
        }

        if (_topAnchor != null && _bottomAnchor != null && _topAnchor != _bottomAnchor)
        {
            return;
        }

        if (CachedTransform.childCount == 0)
        {
            return;
        }

        Transform highestChild = null;
        Transform lowestChild = null;
        float highestLocalY = float.MinValue;
        float lowestLocalY = float.MaxValue;

        for (int i = 0; i < CachedTransform.childCount; i++)
        {
            Transform child = CachedTransform.GetChild(i);
            float localY = child.localPosition.y;

            if (localY > highestLocalY)
            {
                highestLocalY = localY;
                highestChild = child;
            }

            if (localY < lowestLocalY)
            {
                lowestLocalY = localY;
                lowestChild = child;
            }
        }

        if (highestChild != null)
        {
            _topAnchor = highestChild;
        }

        if (lowestChild != null)
        {
            _bottomAnchor = lowestChild;
        }
    }
}
