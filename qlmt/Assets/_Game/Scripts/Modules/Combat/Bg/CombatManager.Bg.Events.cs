using UnityGameFramework.Runtime;

/// <summary>
/// 战斗组件（背景实体事件订阅）。
/// </summary>
public partial class CombatManager
{
    /// <summary>
    /// 订阅实体显示事件。
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
    /// 反订阅实体显示事件。
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
}
