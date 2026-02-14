using UnityEngine;

/// <summary>
/// 战斗组件（背景拼接与边界对齐）。
/// </summary>
public partial class CombatManager
{
    /// <summary>
    /// 获取第一个有效背景实体。
    /// </summary>
    private BgEntity GetFirstValidBackgroundEntity()
    {
        for (int i = 0; i < _activeBackgrounds.Count; i++)
        {
            BgEntity entity = _activeBackgrounds[i].Entity;
            if (entity != null)
            {
                return entity;
            }
        }

        return null;
    }

    /// <summary>
    /// 对所有背景执行同向平移。
    /// </summary>
    /// <param name="offset">平移偏移。</param>
    private void ShiftAllBackgrounds(Vector3 offset)
    {
        for (int i = 0; i < _activeBackgrounds.Count; i++)
        {
            BgEntity entity = _activeBackgrounds[i].Entity;
            if (entity != null)
            {
                entity.CachedTransform.position += offset;
            }
        }
    }

    /// <summary>
    /// 确保底部覆盖相机下边界，避免回收后底部缝隙。
    /// </summary>
    private void EnsureBottomCoverage()
    {
        BgEntity firstEntity = GetFirstValidBackgroundEntity();
        if (firstEntity == null)
        {
            return;
        }

        float bottomGap = firstEntity.BottomAnchorPosition.y - _cameraBottomY;
        if (bottomGap > 0f)
        {
            ShiftAllBackgrounds(Vector3.down * bottomGap);
        }
    }

    /// <summary>
    /// 停止滚动前吸附顶部，避免顶部缝隙。
    /// </summary>
    private void TryStopWhenLastTopReachCameraTop()
    {
        BgEntity lastEntity = GetLastValidBackgroundEntity();
        if (lastEntity == null)
        {
            StopBackgroundScroll();
            return;
        }

        if (lastEntity.TopAnchorPosition.y > _cameraTopY)
        {
            return;
        }

        float topGap = _cameraTopY - lastEntity.TopAnchorPosition.y;
        if (topGap > 0f)
        {
            ShiftAllBackgrounds(Vector3.up * topGap);
            EnsureBottomCoverage();
        }

        StopBackgroundScroll();
    }
}
