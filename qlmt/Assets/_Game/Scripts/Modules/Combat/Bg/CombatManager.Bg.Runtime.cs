using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 战斗组件（背景滚动运行时逻辑）。
/// </summary>
public partial class CombatManager
{
    private void EnsureMainCamera()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
    }

    private void UpdateCameraBounds()
    {
        float halfHeight = _mainCamera.orthographicSize;
        float cameraY = _mainCamera.transform.position.y;
        _cameraTopY = cameraY + halfHeight;
        _cameraBottomY = cameraY - halfHeight;
    }

    private void EnsureBackgroundEntityGroup()
    {
        if (GameEntry.Entity.HasEntityGroup(_bgEntityGroupName))
        {
            return;
        }

        GameEntry.Entity.AddEntityGroup(_bgEntityGroupName, 10f, 32, 60f, -20);
    }

    private bool TryGetNextSpawnPosition(out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;
        EnsureMainCamera();
        if (_mainCamera == null)
        {
            return false;
        }

        BgEntity lastEntity = GetLastValidBackgroundEntity();
        if (lastEntity == null)
        {
            spawnPosition = new Vector3(_mainCamera.transform.position.x, _cameraBottomY, 0f);
        }
        else
        {
            spawnPosition = lastEntity.TopAnchorPosition;
            spawnPosition.z = 0f;
        }

        return true;
    }

    /// <summary>
    /// 获取最后一个有效背景实体。
    /// </summary>
    private BgEntity GetLastValidBackgroundEntity()
    {
        for (int i = _activeBackgrounds.Count - 1; i >= 0; i--)
        {
            BgEntity entity = _activeBackgrounds[i].Entity;
            if (entity != null)
            {
                return entity;
            }

            OnBackgroundWillRecycleForProcGen(_activeBackgrounds[i].EntityId, null);
            _activeBackgrounds.RemoveAt(i);
        }

        _currentBackgroundCount = _activeBackgrounds.Count;
        return null;
    }

    /// <summary>
    /// 背景整体向下移动。
    /// </summary>
    private void MoveBackgrounds(float elapseSeconds)
    {
        Vector3 offset = Vector3.down * (_scrollSpeed * elapseSeconds);
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
    /// 回收屏幕下方背景。
    /// </summary>
    private void RecycleFrontBackgrounds()
    {
        while (_activeBackgrounds.Count > 0)
        {
            BackgroundRuntime first = _activeBackgrounds[0];
            if (first.Entity == null)
            {
                OnBackgroundWillRecycleForProcGen(first.EntityId, null);
                _activeBackgrounds.RemoveAt(0);
                continue;
            }

            if (first.Entity.TopAnchorPosition.y > _cameraBottomY)
            {
                break;
            }

            OnBackgroundWillRecycleForProcGen(first.EntityId, first.Entity);
            if (GameEntry.Entity.HasEntity(first.EntityId))
            {
                GameEntry.Entity.HideEntity(first.EntityId);
            }

            _activeBackgrounds.RemoveAt(0);
            _currentBackgroundCount = _activeBackgrounds.Count;
        }
    }

    /// <summary>
    /// 将新背景块底锚点对齐到目标位置，避免拼接缝隙。
    /// </summary>
    private void AlignSpawnedBackground(BgEntity bgEntity)
    {
        EnsureMainCamera();
        if (_mainCamera == null || bgEntity == null)
        {
            return;
        }

        UpdateCameraBounds();
        BgEntity lastEntity = GetLastValidBackgroundEntity();
        Vector3 targetBottomPosition;
        if (lastEntity == null)
        {
            targetBottomPosition = new Vector3(_mainCamera.transform.position.x, _cameraBottomY, 0f);
        }
        else
        {
            targetBottomPosition = lastEntity.TopAnchorPosition;
            targetBottomPosition.z = 0f;
        }

        bgEntity.SetBottomAnchorWorldPosition(targetBottomPosition);
    }

    /// <summary>
    /// 背景显示成功回调。
    /// </summary>
    private void OnShowEntitySuccess(object sender, GameEventArgs e)
    {
        ShowEntitySuccessEventArgs ne = e as ShowEntitySuccessEventArgs;
        if (ne == null)
        {
            return;
        }

        int entityId = ne.Entity.Id;
        if (_abortedRequestIds.Remove(entityId))
        {
            if (GameEntry.Entity.HasEntity(entityId))
            {
                GameEntry.Entity.HideEntity(entityId);
            }
            return;
        }

        if (!_pendingRequests.TryGetValue(entityId, out SpawnRequest request))
        {
            return;
        }

        _pendingRequests.Remove(entityId);
        BgEntity bgEntity = ne.Entity.Logic as BgEntity;
        if (bgEntity == null)
        {
            Log.Error("背景实体逻辑类型错误，EntityId={0}", entityId);
            GameEntry.Entity.HideEntity(entityId);
            return;
        }

        AlignSpawnedBackground(bgEntity);
        _activeBackgrounds.Add(new BackgroundRuntime
        {
            EntityId = entityId,
            BgBlockId = request.BgBlockId,
            SegmentIndex = request.SegmentIndex,
            Entity = bgEntity
        });
        _currentBackgroundCount = _activeBackgrounds.Count;
        OnBackgroundShownForProcGen(entityId, request.SegmentIndex, bgEntity);

        if (_needStopWhenTopReachCamera)
        {
            TryStopWhenLastTopReachCameraTop();
        }
        else
        {
            FillBackgroundUntilMinCount();
        }
    }

    /// <summary>
    /// 背景显示失败回调。
    /// </summary>
    private void OnShowEntityFailure(object sender, GameEventArgs e)
    {
        ShowEntityFailureEventArgs ne = e as ShowEntityFailureEventArgs;
        if (ne == null)
        {
            return;
        }

        if (_abortedRequestIds.Remove(ne.EntityId))
        {
            GameEntry.EntityIdPool.Release(ne.EntityId);
            return;
        }

        if (!_pendingRequests.Remove(ne.EntityId))
        {
            return;
        }

        Log.Error("背景创建失败：EntityId={0}, Asset={1}, Error={2}", ne.EntityId, ne.EntityAssetName, ne.ErrorMessage);
        GameEntry.EntityIdPool.Release(ne.EntityId);

        if (!_needStopWhenTopReachCamera)
        {
            FillBackgroundUntilMinCount();
        }
    }
}
