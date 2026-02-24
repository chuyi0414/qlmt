using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 战斗组件（背景滚动逻辑）。
/// </summary>
public partial class CombatManager : GameFrameworkComponent
{
    /// <summary>
    /// 运行时背景实例信息。
    /// </summary>
    private struct BackgroundRuntime
    {
        public int EntityId;
        public int BgBlockId;
        public BgEntity Entity;
    }

    /// <summary>
    /// 背景异步创建请求信息。
    /// </summary>
    private struct SpawnRequest
    {
        public int BgBlockId;
        public Vector3 SpawnPosition;
    }

    /// <summary>
    /// 最少背景数量。
    /// </summary>
    [SerializeField] private int _minBackgroundCount = 3;
    /// <summary>
    /// 背景滚动速度。
    /// </summary>
    [SerializeField] private float _scrollSpeed = 1f;
    /// <summary>
    /// 背景实体组名称。
    /// </summary>
    [SerializeField] private string _bgEntityGroupName = "Environment";
    /// <summary>
    /// 当前背景数量。
    /// </summary>
    private int _currentBackgroundCount;

    /// <summary>
    /// 活动背景列表。
    /// </summary>
    private readonly List<BackgroundRuntime> _activeBackgrounds = new List<BackgroundRuntime>();
    /// <summary>
    /// 待完成的背景请求。
    /// </summary>
    private readonly Dictionary<int, SpawnRequest> _pendingRequests = new Dictionary<int, SpawnRequest>();
    /// <summary>
    /// 已中断但还可能回调的请求 Id。
    /// </summary>
    private readonly HashSet<int> _abortedRequestIds = new HashSet<int>();
    /// <summary>
    /// 主题展开后的背景块顺序。
    /// </summary>
    private readonly List<int> _themeSequence = new List<int>();

    /// <summary>
    /// 主相机缓存。
    /// </summary>
    private Camera _mainCamera;
    /// <summary>
    /// 主题游标。
    /// </summary>
    private int _themeCursor;
    /// <summary>
    /// 是否循环主题。
    /// </summary>
    private bool _isThemeLoop;
    /// <summary>
    /// 是否已准备好背景系统。
    /// </summary>
    private bool _isPrepared;
    /// <summary>
    /// 是否正在滚动。
    /// </summary>
    private bool _isScrolling;
    /// <summary>
    /// 是否等待最后块顶部到达相机顶部后停止。
    /// </summary>
    private bool _needStopWhenTopReachCamera;
    /// <summary>
    /// 是否已订阅实体回调。
    /// </summary>
    private bool _isSubscribedEntityEvents;
    /// <summary>
    /// 相机顶部 Y。
    /// </summary>
    private float _cameraTopY;
    /// <summary>
    /// 相机底部 Y。
    /// </summary>
    private float _cameraBottomY;

    /// <summary>
    /// 预加载关卡背景。
    /// </summary>
    public void PrepareLevelBackground(int levelId)
    {
        ClearBackgroundRuntime();
        EnsureMainCamera();
        if (_mainCamera == null)
        {
            Log.Error("准备背景失败，Main Camera 不存在。");
            return;
        }

        EnsureBackgroundEntityGroup();
        EnsureEntityEventSubscription();
        UpdateCameraBounds();

        if (!EnsureBackgroundDataTablesReady() || !BuildThemeSequence(levelId))
        {
            return;
        }

        _isPrepared = true;
        _isScrolling = false;
        FillBackgroundUntilMinCount();
    }

    /// <summary>
    /// 开始背景滚动。
    /// </summary>
    public void StartBackgroundScroll()
    {
        if (!_isPrepared)
        {
            Log.Warning("背景尚未准备完成，忽略开始滚动请求。");
            return;
        }

        _isScrolling = true;
    }

    /// <summary>
    /// 停止背景滚动。
    /// </summary>
    public void StopBackgroundScroll() => _isScrolling = false;

    /// <summary>
    /// 轮询背景滚动。
    /// </summary>
    public void UpdateBackgroundScroll(float elapseSeconds)
    {
        if (!_isPrepared || !_isScrolling || elapseSeconds <= 0f)
        {
            return;
        }

        EnsureMainCamera();
        if (_mainCamera == null)
        {
            return;
        }

        UpdateCameraBounds();
        MoveBackgrounds(elapseSeconds);
        RecycleFrontBackgrounds();
        EnsureBottomCoverage();

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
    /// 清理背景运行时状态。
    /// </summary>
    public void ClearBackgroundRuntime()
    {
        _isPrepared = false;
        _isScrolling = false;
        _needStopWhenTopReachCamera = false;
        _isThemeLoop = false;
        _themeCursor = 0;
        _themeSequence.Clear();

        for (int i = 0; i < _activeBackgrounds.Count; i++)
        {
            BackgroundRuntime runtime = _activeBackgrounds[i];
            int entityId = runtime.EntityId;
            if (entityId > 0 && GameEntry.Entity.HasEntity(entityId))
            {
                GameEntry.Entity.HideEntity(entityId);
            }
        }
        _activeBackgrounds.Clear();

        foreach (KeyValuePair<int, SpawnRequest> pair in _pendingRequests)
        {
            _abortedRequestIds.Add(pair.Key);
        }
        _pendingRequests.Clear();
        _currentBackgroundCount = 0;
    }

    /// <summary>
    /// 组件销毁时清理订阅与状态。
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeEntityEvents();
        ClearBackgroundRuntime();
    }
}
