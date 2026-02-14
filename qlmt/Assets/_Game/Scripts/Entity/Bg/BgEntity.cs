using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 背景实体逻辑。
/// </summary>
public class BgEntity : EntityLogic
{
    /// <summary>
    /// 顶部锚点。
    /// </summary>
    [SerializeField] private Transform _topAnchor;
    /// <summary>
    /// 底部锚点。
    /// </summary>
    [SerializeField] private Transform _bottomAnchor;

    /// <summary>
    /// 顶部锚点世界坐标。
    /// </summary>
    public Vector3 TopAnchorPosition => _topAnchor != null ? _topAnchor.position : CachedTransform.position;
    /// <summary>
    /// 底部锚点世界坐标。
    /// </summary>
    public Vector3 BottomAnchorPosition => _bottomAnchor != null ? _bottomAnchor.position : CachedTransform.position;

    /// <summary>
    /// 初始化回调。
    /// </summary>
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        EnsureAnchors();
    }

    /// <summary>
    /// 显示回调。
    /// </summary>
    protected override void OnShow(object userData)
    {
        EnsureAnchors();

        // 在实体可见前先定位到底锚点，避免先闪到 (0,0,0) 再挪动。
        if (userData is Vector3 bottomAnchorWorldPosition)
        {
            SetBottomAnchorWorldPosition(bottomAnchorWorldPosition);
        }

        base.OnShow(userData);
    }

    /// <summary>
    /// 确保锚点可用。
    /// </summary>
    private void EnsureAnchors()
    {
        if (_topAnchor == null)
        {
            Transform topTransform = CachedTransform.Find("_topAnchor");
            if (topTransform != null)
            {
                _topAnchor = topTransform;
            }
        }

        if (_bottomAnchor == null)
        {
            Transform bottomTransform = CachedTransform.Find("_bottomAnchor");
            if (bottomTransform != null)
            {
                _bottomAnchor = bottomTransform;
            }
        }

        if (_topAnchor == null || _bottomAnchor == null)
        {
            Log.Warning("BgEntity 锚点缺失：{0}", Name);
        }
    }

    /// <summary>
    /// 将底部锚点对齐到目标世界坐标。
    /// </summary>
    /// <param name="bottomAnchorWorldPosition">目标底锚点世界坐标。</param>
    public void SetBottomAnchorWorldPosition(Vector3 bottomAnchorWorldPosition)
    {
        EnsureAnchors();

        Vector3 currentBottom = BottomAnchorPosition;
        Vector3 delta = bottomAnchorWorldPosition - currentBottom;
        CachedTransform.position += delta;
    }
}
