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
public sealed partial class BgScrollComponent : GameFrameworkComponent
{

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

    [Header("主题配置")]

    /// <summary>
    /// 是否循环主题段配置。
    /// 启用后，最后一段结束后会回到第一段继续循环。
    /// </summary>
    private bool _loopThemeSegments = false;

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
    /// 当前主题段剩余背景块数。
    /// </summary>
    private int _currentThemeSegmentRemainChunkCount;

    /// <summary>
    /// 主题序列是否已耗尽。
    /// 仅在非循环模式下使用。
    /// </summary>
    private bool _isThemeSequenceCompleted;

    /// <summary>
    /// 是否等待“最后一块到达顶部生成线”后停止滚动。
    /// </summary>
    private bool _pendingStopAtTopSpawnLine;

    /// <summary>
    /// 触发主题序列耗尽的最后一块实体 Id。
    /// </summary>
    private int _finalChunkEntityId = -1;

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
    /// 当前关卡 Id。
    /// 由外部在启动滚动前显式指定。
    /// </summary>
    private int _currentLevelId = -1;

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
    public void Tick()
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
        }

        TryStopScrollWhenFinalChunkReachedCameraTop();
        if (!_isReady)
        {
            return;
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
        if (_currentLevelId <= 0)
        {
            Log.Error("背景滚动启动失败：未设置有效关卡 Id。请调用 ManualStartScroll(levelId, prewarmBeforeStart)。");
            return false;
        }

        return ManualStartScroll(_currentLevelId, prewarmBeforeStart);
    }

    /// <summary>
    /// 手动启动背景滚动（指定关卡）。
    /// </summary>
    /// <param name="levelId">关卡 Id（必须大于 0）。</param>
    /// <param name="prewarmBeforeStart">是否在启动前执行预热。</param>
    /// <returns>启动是否成功。</returns>
    public bool ManualStartScroll(int levelId, bool prewarmBeforeStart = true)
    {
        if (levelId <= 0)
        {
            Log.Error("背景滚动启动失败：无效关卡 Id，LevelId={0}。", levelId);
            return false;
        }

        if (_currentLevelId != levelId)
        {
            _currentLevelId = levelId;
            _isInitialized = false;
            _isPrewarmed = false;
            _isReady = false;
            _isThemeSequenceCompleted = false;
            _pendingStopAtTopSpawnLine = false;
            _finalChunkEntityId = -1;
        }

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

        if (!BuildDefinitionsFromDataTable(_currentLevelId))
        {
            Log.Error("背景滚动组件初始化失败，背景滚动配置表读取失败。");
            return false;
        }

        if (!HasValidChunkDefinition())
        {
            Log.Error("背景滚动组件初始化失败，未配置可用背景块。请检查 BgChunkConfig DataTable。");
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
    /// 执行开局预热。
    /// 在启动时一次性铺满视野与缓冲区，避免首帧出现空白。
    /// </summary>
    private void PrewarmBackgroundChunks()
    {
        float initialBottomY = GetCameraBottomY();
        _nextSpawnBottomY = initialBottomY;
        TryFillChunksByLoadedTopAnchor();
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
        TryFillChunksByLoadedTopAnchor();

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
    /// 基于上一块实体 _topAnchor 世界坐标持续补块。
    /// </summary>
    private void TryFillChunksByLoadedTopAnchor()
    {
        float cameraTopY = GetCameraTopY();
        float spawnLineY = cameraTopY + _spawnAhead;

        while (CanSpawnMoreChunksByThemeSequence()
            && (_nextSpawnBottomY < spawnLineY || GetAheadChunkCount(cameraTopY) < Mathf.Max(0, _minAheadChunkCount)))
        {
            if (!RequestSpawnAt(_nextSpawnBottomY))
            {
                break;
            }
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
    /// 检查最后一块是否以“顶边”到达相机可视顶部，满足条件后停止滚动。
    /// </summary>
    private void TryStopScrollWhenFinalChunkReachedCameraTop()
    {
        if (!_pendingStopAtTopSpawnLine || _finalChunkEntityId <= 0)
        {
            return;
        }

        if (!_slotByEntityId.ContainsKey(_finalChunkEntityId))
        {
            return;
        }

        BgChunkEntity finalChunk;
        if (!_loadedChunkByEntityId.TryGetValue(_finalChunkEntityId, out finalChunk) || finalChunk == null)
        {
            return;
        }

        float finalChunkTopY = finalChunk.TopY;
        float cameraTopY = GetCameraTopY();
        if (finalChunkTopY > cameraTopY)
        {
            return;
        }

        float alignOffsetY = cameraTopY - finalChunkTopY;
        AlignActiveChunksByOffsetY(alignOffsetY);

        _isReady = false;
        _pendingStopAtTopSpawnLine = false;
        _finalChunkEntityId = -1;
    }

    /// <summary>
    /// 将当前已激活背景块与逻辑槽位整体沿 Y 轴偏移。
    /// 用于停滚动前做一次精确贴线，消除帧步进造成的像素级误差。
    /// </summary>
    /// <param name="offsetY">Y 轴偏移量。</param>
    private void AlignActiveChunksByOffsetY(float offsetY)
    {
        if (Mathf.Abs(offsetY) <= 0.0001f)
        {
            return;
        }

        for (int i = 0; i < _activeSlots.Count; i++)
        {
            ChunkRuntimeSlot slot = _activeSlots[i];
            slot.PlannedBottomY += offsetY;
            slot.PlannedTopY += offsetY;

            BgChunkEntity chunk;
            if (_loadedChunkByEntityId.TryGetValue(slot.EntityId, out chunk) && chunk != null)
            {
                chunk.SnapBottomTo(slot.PlannedBottomY);
            }
        }

        _nextSpawnBottomY += offsetY;
    }

    /// <summary>
    /// 在指定逻辑底边位置生成背景块。
    /// </summary>
    /// <param name="bottomY">目标逻辑底边 Y。</param>
    /// <returns>请求是否成功发起。</returns>
    private bool RequestSpawnAt(float bottomY)
    {
        if (!CanSpawnMoreChunksByThemeSequence())
        {
            return false;
        }

        if (_activeSlots.Count > 0)
        {
            ChunkRuntimeSlot topSlot = _activeSlots[_activeSlots.Count - 1];
            BgChunkEntity topChunk;
            if (!_loadedChunkByEntityId.TryGetValue(topSlot.EntityId, out topChunk) || topChunk == null)
            {
                // 上一块未显示完成前，不生成下一块，避免重叠。
                return false;
            }

            topSlot.PlannedTopY = topChunk.TopY;
            bottomY = topSlot.PlannedTopY;
        }

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

        ChunkRuntimeSlot slot = new ChunkRuntimeSlot();
        slot.EntityId = entityId;
        slot.PlannedBottomY = bottomY;
        slot.PlannedTopY = bottomY;
        slot.Definition = definition;

        _activeSlots.Add(slot);
        _slotByEntityId[entityId] = slot;
        _nextSpawnBottomY = bottomY;

        bool wasThemeSequenceCompleted = _isThemeSequenceCompleted;
        UpdateRepeatState(definition);
        ConsumeThemeChunkCount(1);
        if (!wasThemeSequenceCompleted && _isThemeSequenceCompleted && !_loopThemeSegments)
        {
            _finalChunkEntityId = entityId;
            _pendingStopAtTopSpawnLine = true;
        }

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
            _nextSpawnBottomY = GetCameraBottomY();
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
        slot.PlannedTopY = chunk.TopY;
        RefreshNextSpawnBottomFromSlots();
        TryFillChunksByLoadedTopAnchor();

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
        if (_pendingStopAtTopSpawnLine && entityId == _finalChunkEntityId)
        {
            _isReady = false;
            _pendingStopAtTopSpawnLine = false;
            _finalChunkEntityId = -1;
            Log.Warning("背景滚动最终块显示失败，已提前停止滚动，EntityId={0}。", entityId);
        }

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
