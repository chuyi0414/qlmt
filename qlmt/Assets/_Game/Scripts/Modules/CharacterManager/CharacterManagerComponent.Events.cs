using GameFramework.Event;
using UnityGameFramework.Runtime;

/// <summary>
/// 角色管理器组件（实体事件订阅与回调）。
/// </summary>
public sealed partial class CharacterManagerComponent
{
    /// <summary>
    /// 订阅角色实体显示事件。
    /// </summary>
    private void EnsureEntityEventSubscription()
    {
        if (_isSubscribedEntityEvents || GameEntry.Event == null)
        {
            return;
        }

        GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
        GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
        _isSubscribedEntityEvents = true;
    }

    /// <summary>
    /// 反订阅角色实体显示事件。
    /// </summary>
    private void UnsubscribeEntityEvents()
    {
        if (!_isSubscribedEntityEvents || GameEntry.Event == null)
        {
            return;
        }

        if (GameEntry.Event.Check(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess))
        {
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
        }

        if (GameEntry.Event.Check(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure))
        {
            GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
        }

        _isSubscribedEntityEvents = false;
    }

    /// <summary>
    /// 角色显示成功回调。
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
            else
            {
                GameEntry.EntityIdPool.Release(entityId);
            }

            return;
        }

        if (!_pendingRequests.TryGetValue(entityId, out CharacterSpawnRequest request))
        {
            return;
        }
        _pendingRequests.Remove(entityId);

        CharacterEntity characterEntity = ne.Entity.Logic as CharacterEntity;
        if (characterEntity == null)
        {
            Log.Error("角色实体逻辑类型错误，EntityId={0}", entityId);
            if (GameEntry.Entity.HasEntity(entityId))
            {
                GameEntry.Entity.HideEntity(entityId);
            }
            else
            {
                GameEntry.EntityIdPool.Release(entityId);
            }

            return;
        }

        _characterRuntimes[entityId] = new CharacterRuntime
        {
            EntityId = entityId,
            CharacterConfigId = request.CharacterConfigId,
            Entity = characterEntity
        };
    }

    /// <summary>
    /// 角色显示失败回调。
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

        Log.Error("角色创建失败：EntityId={0}, Asset={1}, Error={2}", ne.EntityId, ne.EntityAssetName, ne.ErrorMessage);
        GameEntry.EntityIdPool.Release(ne.EntityId);
    }
}
