using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 背景滚动组件。
/// 该组件负责驱动背景块沿 Y 轴负方向滚动，并根据相机视野执行补块与回收。
/// </summary>
public sealed class BgScrollComponent : GameFrameworkComponent
{
    /// <summary>
    /// 背景块配置。
    /// 用于描述某一类背景块资源、权重、主题与连接规则。
    /// </summary>
    [Serializable]
    private sealed class BgChunkDefinition
    {
        /// <summary>
        /// 背景块实体相对路径（相对于 Prefabs/Entity）。
        /// 示例：Bg/Chunk_Forest_01。
        /// </summary>
        [SerializeField]
        private string _entityRelativePath;

        /// <summary>
        /// 该背景块所属主题标签。
        /// 为空表示通用背景块，可在任意主题下出现。
        /// </summary>
        [SerializeField]
        private string _themeTag;

        /// <summary>
        /// 该背景块的随机权重。
        /// 数值越大，被选中的概率越高。
        /// </summary>
        [SerializeField]
        private float _weight = 1f;

        /// <summary>
        /// 该背景块的逻辑高度（世界单位）。
        /// 用于预热、补块与回收判定，建议与预制体实际高度保持一致。
        /// </summary>
        [SerializeField]
        private float _chunkLength = 10f;

        /// <summary>
        /// 允许跟随的前一主题列表。
        /// 为空表示不限制；支持使用 * 表示允许跟随任意主题。
        /// </summary>
        [SerializeField]
        private string[] _canFollowThemes;

        /// <summary>
        /// 获取实体相对路径。
        /// </summary>
        public string EntityRelativePath
        {
            get { return _entityRelativePath; }
        }

        /// <summary>
        /// 获取实体完整资源路径。
        /// </summary>
        public string EntityAssetName
        {
            get { return GameAssetPath.GetEntity(_entityRelativePath); }
        }

        /// <summary>
        /// 获取主题标签。
        /// </summary>
        public string ThemeTag
        {
            get { return _themeTag; }
        }

        /// <summary>
        /// 获取随机权重。
        /// </summary>
        public float Weight
        {
            get { return _weight; }
        }

        /// <summary>
        /// 获取背景块逻辑高度。
        /// </summary>
        public float ChunkLength
        {
            get { return _chunkLength; }
        }

        /// <summary>
        /// 获取允许跟随的前一主题列表。
        /// </summary>
        public string[] CanFollowThemes
        {
            get { return _canFollowThemes; }
        }
    }

    /// <summary>
    /// 主题段配置。
    /// 用于定义某个主题持续的行进距离。
    /// </summary>
    [Serializable]
    private sealed class BgThemeSegment
    {
        /// <summary>
        /// 主题标签。
        /// </summary>
        [SerializeField]
        private string _themeTag;

        /// <summary>
        /// 该主题段持续距离（世界单位）。
        /// </summary>
        [SerializeField]
        private float _distance = 200f;

        /// <summary>
        /// 获取主题标签。
        /// </summary>
        public string ThemeTag
        {
            get { return _themeTag; }
        }

        /// <summary>
        /// 获取持续距离。
        /// </summary>
        public float Distance
        {
            get { return _distance; }
        }
    }

    /// <summary>
    /// 背景块运行时槽位。
    /// 用于记录某个背景块的逻辑区间与实体 Id。
    /// </summary>
    private sealed class ChunkRuntimeSlot
    {
        /// <summary>
        /// 实体 Id。
        /// </summary>
        public int EntityId;

        /// <summary>
        /// 该块逻辑底边 Y。
        /// </summary>
        public float PlannedBottomY;

        /// <summary>
        /// 该块逻辑顶边 Y。
        /// </summary>
        public float PlannedTopY;

        /// <summary>
        /// 该块关联的静态配置。
        /// </summary>
        public BgChunkDefinition Definition;
    }

    /// <summary>
    /// 主相机引用。
    /// 用于计算当前视野范围。
    /// </summary>
    [Header("基础引用")]
    [SerializeField]
    private Camera _mainCamera;

    /// <summary>
    /// 背景实体组名称。
    /// 所有背景块实体都将显示在该实体组中。
    /// </summary>
    [Header("实体配置")]
    [SerializeField]
    private string _entityGroupName = "Background";

    /// <summary>
    /// 背景块配置列表。
    /// 至少需要配置一项有效背景块。
    /// </summary>
    [SerializeField]
    private BgChunkDefinition[] _chunkDefinitions;

    /// <summary>
    /// 主题段配置列表。
    /// 为空时表示不启用主题流程，仅按通用规则随机。
    /// </summary>
    [Header("主题配置")]
    [SerializeField]
    private BgThemeSegment[] _themeSegments;

    /// <summary>
    /// 是否循环主题段配置。
    /// 启用后，最后一段结束后会回到第一段继续循环。
    /// </summary>
    [SerializeField]
    private bool _loopThemeSegments = true;

    /// <summary>
    /// 背景滚动速度（世界单位/秒）。
    /// </summary>
    [Header("滚动参数")]
    [SerializeField]
    private float _scrollSpeed = 1f;

    /// <summary>
    /// 在相机顶部额外提前生成的缓冲距离。
    /// 用于避免滚动过程中出现顶部空白。
    /// </summary>
    private float _spawnAhead = 4f;

    /// <summary>
    /// 相机前方至少保留的背景块数量。
    /// 该数量按“背景块底边位于相机顶部之上”来统计，用于减少前方加载突兀感。
    /// </summary>
    [SerializeField]
    private int _minAheadChunkCount = 1;

    /// <summary>
    /// 在相机底部额外延迟回收的缓冲距离。
    /// 用于避免刚离屏就回收导致的视觉抖动。
    /// </summary>
    private float _recycleBehind = 4f;

    /// <summary>
    /// 同一背景块最大连续出现次数。
    /// 该限制用于降低重复感。
    /// </summary>
    [SerializeField]
    private int _maxConsecutiveSameAsset = 2;

    /// <summary>
    /// 当前活跃背景块槽位列表（顺序：下 -> 上）。
    /// </summary>
    private readonly List<ChunkRuntimeSlot> _activeSlots = new List<ChunkRuntimeSlot>();

    /// <summary>
    /// 实体 Id 到槽位的映射。
    /// 用于快速定位实体对应的逻辑区间。
    /// </summary>
    private readonly Dictionary<int, ChunkRuntimeSlot> _slotByEntityId = new Dictionary<int, ChunkRuntimeSlot>();

    /// <summary>
    /// 已加载背景块实体逻辑缓存。
    /// </summary>
    private readonly Dictionary<int, BgChunkEntity> _loadedChunkByEntityId = new Dictionary<int, BgChunkEntity>();

    /// <summary>
    /// 需要在显示成功后立即隐藏的实体 Id 集合。
    /// 用于处理“尚未显示完成就进入回收流程”的情况。
    /// </summary>
    private readonly HashSet<int> _hideAfterShowEntityIds = new HashSet<int>();

    /// <summary>
    /// 临时候选列表。
    /// 用于选块时复用容器，减少运行时 GC。
    /// </summary>
    private readonly List<BgChunkDefinition> _candidateBuffer = new List<BgChunkDefinition>();

    /// <summary>
    /// 下一个待生成背景块的逻辑底边 Y。
    /// </summary>
    private float _nextSpawnBottomY;

    /// <summary>
    /// 当前主题段索引。
    /// </summary>
    private int _currentThemeSegmentIndex;

    /// <summary>
    /// 当前主题段剩余距离。
    /// </summary>
    private float _currentThemeSegmentRemainDistance;

    /// <summary>
    /// 上一次生成的背景块定义。
    /// </summary>
    private BgChunkDefinition _lastSpawnDefinition;

    /// <summary>
    /// 当前连续出现同一背景块的次数。
    /// </summary>
    private int _currentSameAssetCount;

    /// <summary>
    /// 组件是否已完成初始化。
    /// 初始化完成后，组件具备响应手动预热与手动启动的能力。
    /// </summary>
    private bool _isInitialized;

    /// <summary>
    /// 组件是否已完成预热。
    /// 该标记用于避免重复预热。
    /// </summary>
    private bool _isPrewarmed;

    /// <summary>
    /// 组件是否处于运行状态。
    /// 仅当该标记为 true 时，Update 才会驱动背景滚动与补块回收。
    /// </summary>
    private bool _isReady;

    /// <summary>
    /// 组件启动入口。
    /// 该入口不执行任何自动初始化逻辑。
    /// 组件会在外部首次调用 ManualPrewarm 或 ManualStartScroll 时完成初始化。
    /// </summary>
    private void Start()
    {
        // 按设计不在 Start 自动初始化，避免与外部流程时序冲突。
    }

    /// <summary>
    /// 组件销毁。
    /// 负责反订阅事件，防止事件泄漏。
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    /// <summary>
    /// 帧更新。
    /// 负责驱动背景下移、推进主题段并维护补块/回收。
    /// </summary>
    private void Update()
    {
        if (!_isReady)
        {
            return;
        }

        float moveDistance = Mathf.Max(0f, _scrollSpeed) * Time.deltaTime;
        if (moveDistance > 0f)
        {
            MoveLoadedChunksDown(moveDistance);
            ShiftPlannedSlotsDown(moveDistance);
            ConsumeThemeDistance(moveDistance);
        }

        MaintainSpawnAndRecycle();
    }

    /// <summary>
    /// 设置滚动速度。
    /// 可用于与追击值等系统联动。
    /// </summary>
    /// <param name="speed">目标速度（小于 0 时将被钳制为 0）。</param>
    public void SetScrollSpeed(float speed)
    {
        _scrollSpeed = Mathf.Max(0f, speed);
    }

    /// <summary>
    /// 手动执行背景预热。
    /// 预热会按当前相机视野与缓冲距离生成初始背景块，但不会自动进入滚动状态。
    /// </summary>
    /// <returns>预热是否成功。</returns>
    public bool ManualPrewarm()
    {
        if (!EnsureInitialized())
        {
            return false;
        }

        if (_isPrewarmed)
        {
            return true;
        }

        try
        {
            PrewarmBackgroundChunks();
            _isPrewarmed = true;
            return true;
        }
        catch (GameFrameworkException exception)
        {
            Log.Error("背景滚动组件手动预热失败：{0}", exception.Message);
            return false;
        }
    }

    /// <summary>
    /// 手动启动背景滚动。
    /// 可选择在启动前自动执行一次预热。
    /// </summary>
    /// <param name="prewarmBeforeStart">是否在启动前执行预热。</param>
    /// <returns>启动是否成功。</returns>
    public bool ManualStartScroll(bool prewarmBeforeStart = true)
    {
        if (!EnsureInitialized())
        {
            return false;
        }

        if (prewarmBeforeStart && !_isPrewarmed)
        {
            if (!ManualPrewarm())
            {
                return false;
            }
        }

        _isReady = true;
        return true;
    }

    /// <summary>
    /// 手动停止背景滚动。
    /// 停止后不会自动清理已存在的背景块，仅暂停移动与补块回收逻辑。
    /// </summary>
    public void ManualStopScroll()
    {
        _isReady = false;
    }

    /// <summary>
    /// 确保组件完成初始化。
    /// 初始化仅执行一次，重复调用将直接返回。
    /// </summary>
    /// <returns>初始化是否成功。</returns>
    private bool EnsureInitialized()
    {
        if (_isInitialized)
        {
            return true;
        }

        if (!TryInitReferences())
        {
            Log.Error("背景滚动组件初始化失败，缺少必要依赖。请检查相机、Entity、Event、EntityIdPool 配置。");
            return false;
        }

        if (!HasValidChunkDefinition())
        {
            Log.Error("背景滚动组件初始化失败，未配置可用背景块。请检查 Chunk Definitions。");
            return false;
        }

        SubscribeEvents();
        InitThemeState();
        _isInitialized = true;
        return true;
    }

    /// <summary>
    /// 尝试初始化依赖引用。
    /// </summary>
    /// <returns>初始化是否成功。</returns>
    private bool TryInitReferences()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        return true;
    }

    /// <summary>
    /// 检查是否存在有效背景块配置。
    /// </summary>
    /// <returns>是否存在有效配置。</returns>
    private bool HasValidChunkDefinition()
    {
        if (_chunkDefinitions == null || _chunkDefinitions.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < _chunkDefinitions.Length; i++)
        {
            BgChunkDefinition definition = _chunkDefinitions[i];
            if (definition == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(definition.EntityRelativePath))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 订阅实体相关事件。
    /// </summary>
    private void SubscribeEvents()
    {
        GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
        GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
    }

    /// <summary>
    /// 取消订阅实体相关事件。
    /// </summary>
    private void UnsubscribeEvents()
    {
        if (GameEntry.Event == null || !Application.isPlaying)
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
    }

    /// <summary>
    /// 初始化主题状态。
    /// </summary>
    private void InitThemeState()
    {
        _currentThemeSegmentIndex = 0;

        if (_themeSegments == null || _themeSegments.Length == 0)
        {
            _currentThemeSegmentRemainDistance = float.MaxValue;
            return;
        }

        _currentThemeSegmentRemainDistance = Mathf.Max(0.01f, _themeSegments[0].Distance);
    }

    /// <summary>
    /// 执行开局预热。
    /// 在启动时一次性铺满视野与缓冲区，避免首帧出现空白。
    /// </summary>
    private void PrewarmBackgroundChunks()
    {
        float initialBottomY = GetCameraBottomY() - _recycleBehind;
        float targetTopY = GetCameraTopY() + _spawnAhead;
        float cameraTopY = GetCameraTopY();

        _nextSpawnBottomY = initialBottomY;

        while (_nextSpawnBottomY < targetTopY)
        {
            if (!RequestSpawnAt(_nextSpawnBottomY))
            {
                break;
            }
        }

        // 预热阶段额外保证相机前方至少存在指定数量的背景块。
        EnsureAheadChunkCount(cameraTopY);
    }

    /// <summary>
    /// 推进主题剩余距离。
    /// 当剩余距离耗尽时，切换到下一主题段。
    /// </summary>
    /// <param name="distance">本帧行进距离。</param>
    private void ConsumeThemeDistance(float distance)
    {
        if (distance <= 0f)
        {
            return;
        }

        if (_themeSegments == null || _themeSegments.Length == 0)
        {
            return;
        }

        _currentThemeSegmentRemainDistance -= distance;
        while (_currentThemeSegmentRemainDistance <= 0f)
        {
            float overflowDistance = -_currentThemeSegmentRemainDistance;
            MoveToNextThemeSegment();
            _currentThemeSegmentRemainDistance -= overflowDistance;
        }
    }

    /// <summary>
    /// 切换到下一主题段。
    /// </summary>
    private void MoveToNextThemeSegment()
    {
        if (_themeSegments == null || _themeSegments.Length == 0)
        {
            _currentThemeSegmentRemainDistance = float.MaxValue;
            return;
        }

        int nextIndex = _currentThemeSegmentIndex + 1;
        if (nextIndex >= _themeSegments.Length)
        {
            if (_loopThemeSegments)
            {
                nextIndex = 0;
            }
            else
            {
                nextIndex = _themeSegments.Length - 1;
            }
        }

        _currentThemeSegmentIndex = Mathf.Clamp(nextIndex, 0, _themeSegments.Length - 1);
        _currentThemeSegmentRemainDistance = Mathf.Max(0.01f, _themeSegments[_currentThemeSegmentIndex].Distance);
    }

    /// <summary>
    /// 驱动所有已加载背景块向下移动。
    /// </summary>
    /// <param name="distance">移动距离。</param>
    private void MoveLoadedChunksDown(float distance)
    {
        for (int i = 0; i < _activeSlots.Count; i++)
        {
            ChunkRuntimeSlot slot = _activeSlots[i];
            BgChunkEntity chunk;
            if (_loadedChunkByEntityId.TryGetValue(slot.EntityId, out chunk))
            {
                chunk.MoveDown(distance);
            }
        }
    }

    /// <summary>
    /// 将所有逻辑槽位整体下移。
    /// 该步骤确保“逻辑区间”与“视觉移动”保持一致。
    /// </summary>
    /// <param name="distance">移动距离。</param>
    private void ShiftPlannedSlotsDown(float distance)
    {
        for (int i = 0; i < _activeSlots.Count; i++)
        {
            ChunkRuntimeSlot slot = _activeSlots[i];
            slot.PlannedBottomY -= distance;
            slot.PlannedTopY -= distance;
        }

        _nextSpawnBottomY -= distance;
    }

    /// <summary>
    /// 维护背景块补充与回收。
    /// </summary>
    private void MaintainSpawnAndRecycle()
    {
        float cameraTopY = GetCameraTopY();
        float spawnLineY = cameraTopY + _spawnAhead;

        // 采用“距离 + 数量”双条件补块：
        // 1) 传统的顶部缓冲距离；
        // 2) 相机前方最少背景块数量。
        while (_nextSpawnBottomY < spawnLineY || GetAheadChunkCount(cameraTopY) < Mathf.Max(0, _minAheadChunkCount))
        {
            if (!RequestSpawnAt(_nextSpawnBottomY))
            {
                break;
            }
        }

        float recycleLineY = GetCameraBottomY() - _recycleBehind;
        while (_activeSlots.Count > 0)
        {
            ChunkRuntimeSlot bottomSlot = _activeSlots[0];
            if (bottomSlot.PlannedTopY >= recycleLineY)
            {
                break;
            }

            RecycleBottomSlot();
        }
    }

    /// <summary>
    /// 统计当前相机前方的背景块数量。
    /// 统计规则：背景块底边 Y 大于等于相机顶部 Y，视为“在前方”。
    /// </summary>
    /// <param name="cameraTopY">相机顶部世界坐标 Y。</param>
    /// <returns>前方背景块数量。</returns>
    private int GetAheadChunkCount(float cameraTopY)
    {
        int aheadChunkCount = 0;

        for (int i = _activeSlots.Count - 1; i >= 0; i--)
        {
            ChunkRuntimeSlot slot = _activeSlots[i];
            if (slot.PlannedBottomY >= cameraTopY)
            {
                aheadChunkCount++;
                continue;
            }

            break;
        }

        return aheadChunkCount;
    }

    /// <summary>
    /// 确保相机前方至少存在指定数量的背景块。
    /// 若当前数量不足，则持续补块直至满足条件或生成失败。
    /// </summary>
    /// <param name="cameraTopY">相机顶部世界坐标 Y。</param>
    private void EnsureAheadChunkCount(float cameraTopY)
    {
        int minAheadCount = Mathf.Max(0, _minAheadChunkCount);
        while (GetAheadChunkCount(cameraTopY) < minAheadCount)
        {
            if (!RequestSpawnAt(_nextSpawnBottomY))
            {
                break;
            }
        }
    }

    /// <summary>
    /// 在指定逻辑底边位置生成背景块。
    /// </summary>
    /// <param name="bottomY">目标逻辑底边 Y。</param>
    /// <returns>请求是否成功发起。</returns>
    private bool RequestSpawnAt(float bottomY)
    {
        BgChunkDefinition definition = SelectNextChunkDefinition();
        if (definition == null)
        {
            Log.Error("背景块生成失败：未找到可用配置。请检查主题或连接规则。");
            return false;
        }

        int entityId = GameEntry.EntityIdPool.Acquire();
        if (entityId <= 0)
        {
            Log.Error("背景块生成失败：无法从 EntityIdPool 申请实体 Id。");
            return false;
        }

        float chunkLength = Mathf.Max(0.01f, definition.ChunkLength);

        ChunkRuntimeSlot slot = new ChunkRuntimeSlot();
        slot.EntityId = entityId;
        slot.PlannedBottomY = bottomY;
        slot.PlannedTopY = bottomY + chunkLength;
        slot.Definition = definition;

        _activeSlots.Add(slot);
        _slotByEntityId[entityId] = slot;
        _nextSpawnBottomY = slot.PlannedTopY;

        UpdateRepeatState(definition);

        GameEntry.Entity.ShowEntity<BgChunkEntity>(entityId, definition.EntityAssetName, _entityGroupName);
        return true;
    }

    /// <summary>
    /// 回收当前最底部背景块。
    /// </summary>
    private void RecycleBottomSlot()
    {
        ChunkRuntimeSlot slot = _activeSlots[0];
        _activeSlots.RemoveAt(0);
        _slotByEntityId.Remove(slot.EntityId);

        BgChunkEntity chunk;
        if (_loadedChunkByEntityId.TryGetValue(slot.EntityId, out chunk))
        {
            _loadedChunkByEntityId.Remove(slot.EntityId);
            if (GameEntry.Entity.HasEntity(slot.EntityId))
            {
                GameEntry.Entity.HideEntity(slot.EntityId);
            }
        }
        else
        {
            _hideAfterShowEntityIds.Add(slot.EntityId);
        }

        RefreshNextSpawnBottomFromSlots();
    }

    /// <summary>
    /// 根据当前槽位刷新下一个生成位置。
    /// </summary>
    private void RefreshNextSpawnBottomFromSlots()
    {
        if (_activeSlots.Count == 0)
        {
            _nextSpawnBottomY = GetCameraBottomY() - _recycleBehind;
            return;
        }

        ChunkRuntimeSlot topSlot = _activeSlots[_activeSlots.Count - 1];
        _nextSpawnBottomY = topSlot.PlannedTopY;
    }

    /// <summary>
    /// 选择下一块背景配置。
    /// 选择顺序：主题 + 连接规则 + 防重复；若无结果则逐级放宽条件。
    /// </summary>
    /// <returns>选中的背景块配置。</returns>
    private BgChunkDefinition SelectNextChunkDefinition()
    {
        string currentTheme = GetCurrentThemeTag();
        string previousTheme = _lastSpawnDefinition != null ? _lastSpawnDefinition.ThemeTag : string.Empty;

        BgChunkDefinition strictCandidate = PickWeightedDefinition(currentTheme, previousTheme, true, true);
        if (strictCandidate != null)
        {
            return strictCandidate;
        }

        BgChunkDefinition ignoreRepeatCandidate = PickWeightedDefinition(currentTheme, previousTheme, true, false);
        if (ignoreRepeatCandidate != null)
        {
            return ignoreRepeatCandidate;
        }

        BgChunkDefinition ignoreFollowCandidate = PickWeightedDefinition(currentTheme, previousTheme, false, false);
        if (ignoreFollowCandidate != null)
        {
            return ignoreFollowCandidate;
        }

        return PickWeightedDefinition(string.Empty, string.Empty, false, false);
    }

    /// <summary>
    /// 在给定约束下执行一次加权随机选块。
    /// </summary>
    /// <param name="currentTheme">当前主题标签。</param>
    /// <param name="previousTheme">前一块主题标签。</param>
    /// <param name="enforceFollowRule">是否强制连接规则。</param>
    /// <param name="enforceRepeatRule">是否强制防重复规则。</param>
    /// <returns>选中的背景块配置；若无候选则返回 null。</returns>
    private BgChunkDefinition PickWeightedDefinition(string currentTheme, string previousTheme, bool enforceFollowRule, bool enforceRepeatRule)
    {
        _candidateBuffer.Clear();

        for (int i = 0; i < _chunkDefinitions.Length; i++)
        {
            BgChunkDefinition definition = _chunkDefinitions[i];
            if (definition == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(definition.EntityRelativePath))
            {
                continue;
            }

            if (!IsThemeCompatible(definition, currentTheme))
            {
                continue;
            }

            if (enforceFollowRule && !IsFollowRuleMatched(definition, previousTheme))
            {
                continue;
            }

            if (enforceRepeatRule && IsRepeatRuleBlocked(definition))
            {
                continue;
            }

            _candidateBuffer.Add(definition);
        }

        if (_candidateBuffer.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        for (int i = 0; i < _candidateBuffer.Count; i++)
        {
            totalWeight += Mathf.Max(0f, _candidateBuffer[i].Weight);
        }

        if (totalWeight <= 0f)
        {
            return _candidateBuffer[UnityEngine.Random.Range(0, _candidateBuffer.Count)];
        }

        float roll = UnityEngine.Random.value * totalWeight;
        for (int i = 0; i < _candidateBuffer.Count; i++)
        {
            BgChunkDefinition definition = _candidateBuffer[i];
            roll -= Mathf.Max(0f, definition.Weight);
            if (roll <= 0f)
            {
                return definition;
            }
        }

        return _candidateBuffer[_candidateBuffer.Count - 1];
    }

    /// <summary>
    /// 判断背景块是否与当前主题兼容。
    /// </summary>
    /// <param name="definition">背景块定义。</param>
    /// <param name="currentTheme">当前主题。</param>
    /// <returns>是否兼容。</returns>
    private bool IsThemeCompatible(BgChunkDefinition definition, string currentTheme)
    {
        if (string.IsNullOrEmpty(currentTheme))
        {
            return true;
        }

        if (string.IsNullOrEmpty(definition.ThemeTag))
        {
            return true;
        }

        return string.Equals(definition.ThemeTag, currentTheme, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 判断背景块是否满足“可跟随前一主题”规则。
    /// </summary>
    /// <param name="definition">背景块定义。</param>
    /// <param name="previousTheme">前一主题。</param>
    /// <returns>是否满足连接规则。</returns>
    private bool IsFollowRuleMatched(BgChunkDefinition definition, string previousTheme)
    {
        string[] canFollowThemes = definition.CanFollowThemes;
        if (canFollowThemes == null || canFollowThemes.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < canFollowThemes.Length; i++)
        {
            string allowTheme = canFollowThemes[i];
            if (string.IsNullOrWhiteSpace(allowTheme))
            {
                continue;
            }

            if (allowTheme == "*")
            {
                return true;
            }

            if (string.Equals(allowTheme, previousTheme, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断防重复规则是否阻止该背景块。
    /// </summary>
    /// <param name="definition">背景块定义。</param>
    /// <returns>若被阻止返回 true。</returns>
    private bool IsRepeatRuleBlocked(BgChunkDefinition definition)
    {
        if (_maxConsecutiveSameAsset <= 0)
        {
            return false;
        }

        if (_lastSpawnDefinition == null)
        {
            return false;
        }

        if (!string.Equals(_lastSpawnDefinition.EntityRelativePath, definition.EntityRelativePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return _currentSameAssetCount >= _maxConsecutiveSameAsset;
    }

    /// <summary>
    /// 更新连续重复状态。
    /// </summary>
    /// <param name="definition">本次选中的背景块定义。</param>
    private void UpdateRepeatState(BgChunkDefinition definition)
    {
        if (_lastSpawnDefinition == null)
        {
            _lastSpawnDefinition = definition;
            _currentSameAssetCount = 1;
            return;
        }

        if (string.Equals(_lastSpawnDefinition.EntityRelativePath, definition.EntityRelativePath, StringComparison.OrdinalIgnoreCase))
        {
            _currentSameAssetCount++;
        }
        else
        {
            _currentSameAssetCount = 1;
        }

        _lastSpawnDefinition = definition;
    }

    /// <summary>
    /// 获取当前主题标签。
    /// </summary>
    /// <returns>当前主题标签，若无主题配置则返回空字符串。</returns>
    private string GetCurrentThemeTag()
    {
        if (_themeSegments == null || _themeSegments.Length == 0)
        {
            return string.Empty;
        }

        int index = Mathf.Clamp(_currentThemeSegmentIndex, 0, _themeSegments.Length - 1);
        return _themeSegments[index] != null ? _themeSegments[index].ThemeTag : string.Empty;
    }

    /// <summary>
    /// 处理实体显示成功事件。
    /// 成功后将实体与逻辑槽位绑定，并执行位置对齐。
    /// </summary>
    /// <param name="sender">事件发送者。</param>
    /// <param name="e">事件参数。</param>
    private void OnShowEntitySuccess(object sender, GameEventArgs e)
    {
        ShowEntitySuccessEventArgs ne = e as ShowEntitySuccessEventArgs;
        if (ne == null || ne.EntityLogicType != typeof(BgChunkEntity))
        {
            return;
        }

        int entityId = ne.Entity.Id;
        ChunkRuntimeSlot slot;
        if (!_slotByEntityId.TryGetValue(entityId, out slot))
        {
            if (GameEntry.Entity.HasEntity(entityId))
            {
                GameEntry.Entity.HideEntity(entityId);
            }

            return;
        }

        BgChunkEntity chunk = ne.Entity.Logic as BgChunkEntity;
        if (chunk == null)
        {
            Log.Warning("背景块显示成功但逻辑类型转换失败，EntityId={0}。", entityId);
            return;
        }

        _loadedChunkByEntityId[entityId] = chunk;
        chunk.SnapBottomTo(slot.PlannedBottomY);

        float configuredLength = Mathf.Max(0.01f, slot.PlannedTopY - slot.PlannedBottomY);
        float actualLength = chunk.Length;
        if (Mathf.Abs(configuredLength - actualLength) > 0.05f)
        {
            Log.Warning("背景块长度与配置存在偏差，EntityId={0}，ConfigLength={1}，ActualLength={2}。",
                entityId, configuredLength, actualLength);
        }

        if (_hideAfterShowEntityIds.Remove(entityId))
        {
            _loadedChunkByEntityId.Remove(entityId);
            if (GameEntry.Entity.HasEntity(entityId))
            {
                GameEntry.Entity.HideEntity(entityId);
            }
        }
    }

    /// <summary>
    /// 处理实体显示失败事件。
    /// 失败后清理运行时状态并主动回收实体 Id。
    /// </summary>
    /// <param name="sender">事件发送者。</param>
    /// <param name="e">事件参数。</param>
    private void OnShowEntityFailure(object sender, GameEventArgs e)
    {
        ShowEntityFailureEventArgs ne = e as ShowEntityFailureEventArgs;
        if (ne == null || ne.EntityLogicType != typeof(BgChunkEntity))
        {
            return;
        }

        int entityId = ne.EntityId;

        RemoveSlotByEntityId(entityId);
        _loadedChunkByEntityId.Remove(entityId);
        _hideAfterShowEntityIds.Remove(entityId);
        GameEntry.EntityIdPool.Release(entityId);
        RefreshNextSpawnBottomFromSlots();

        Log.Warning("背景块显示失败，EntityId={0}，Asset={1}，Error={2}。",
            ne.EntityId, ne.EntityAssetName, ne.ErrorMessage);
    }

    /// <summary>
    /// 按实体 Id 移除槽位。
    /// </summary>
    /// <param name="entityId">目标实体 Id。</param>
    private void RemoveSlotByEntityId(int entityId)
    {
        _slotByEntityId.Remove(entityId);

        for (int i = 0; i < _activeSlots.Count; i++)
        {
            if (_activeSlots[i].EntityId == entityId)
            {
                _activeSlots.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// 获取相机顶部世界坐标 Y。
    /// </summary>
    /// <returns>相机顶部 Y。</returns>
    private float GetCameraTopY()
    {
        return _mainCamera.transform.position.y + _mainCamera.orthographicSize;
    }

    /// <summary>
    /// 获取相机底部世界坐标 Y。
    /// </summary>
    /// <returns>相机底部 Y。</returns>
    private float GetCameraBottomY()
    {
        return _mainCamera.transform.position.y - _mainCamera.orthographicSize;
    }
}
