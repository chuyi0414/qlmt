using System;
using System.Collections.Generic;
using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;

public sealed partial class BgScrollComponent
{
    /// <summary>
    /// Chunk 配置表名称。
    /// </summary>
    private const string ChunkDataTableName = "BgScroll/BgChunkConfig";

    /// <summary>
    /// ThemeSegment 配置表名称。
    /// </summary>
    private const string ThemeSegmentDataTableName = "BgScroll/BgThemeSegmentConfig";

    /// <summary>
    /// LevelTheme 配置表名称。
    /// </summary>
    private const string LevelThemeDataTableName = "BgScroll/BgLevelThemeConfig";

    /// <summary>
    /// 背景块配置。
    /// 用于描述某一类背景块资源、权重、主题与连接规则。
    /// </summary>
    private sealed class BgChunkDefinition
    {
        private readonly string _entityRelativePath;
        private readonly string _themeTag;
        private readonly float _weight;
        private readonly string[] _canFollowThemes;

        public BgChunkDefinition(string entityRelativePath, string themeTag, float weight, string[] canFollowThemes)
        {
            _entityRelativePath = entityRelativePath;
            _themeTag = themeTag;
            _weight = weight;
            _canFollowThemes = canFollowThemes;
        }

        public string EntityRelativePath
        {
            get { return _entityRelativePath; }
        }

        public string EntityAssetName
        {
            get { return GameAssetPath.GetEntity(_entityRelativePath); }
        }

        public string ThemeTag
        {
            get { return _themeTag; }
        }

        public float Weight
        {
            get { return _weight; }
        }

        public string[] CanFollowThemes
        {
            get { return _canFollowThemes; }
        }
    }

    /// <summary>
    /// 主题段配置。
    /// 用于定义某个主题持续的背景块数量。
    /// </summary>
    private sealed class BgThemeSegment
    {
        private readonly string _themeTag;
        private readonly int _chunkCount;

        public BgThemeSegment(string themeTag, int chunkCount)
        {
            _themeTag = themeTag;
            _chunkCount = chunkCount;
        }

        public string ThemeTag
        {
            get { return _themeTag; }
        }

        public int ChunkCount
        {
            get { return _chunkCount; }
        }
    }

    /// <summary>
    /// 背景块配置列表。
    /// 启动时由 DataTable 构建。
    /// </summary>
    private BgChunkDefinition[] _chunkDefinitions = new BgChunkDefinition[0];

    /// <summary>
    /// 主题段配置列表。
    /// 启动时由 DataTable 构建；为空时表示不启用主题流程。
    /// </summary>
    private BgThemeSegment[] _themeSegments = new BgThemeSegment[0];

    /// <summary>
    /// 从 DataTable 构建背景块与主题段配置。
    /// </summary>
    /// <returns>是否构建成功。</returns>
    private bool BuildDefinitionsFromDataTable(int levelId)
    {
        if (levelId <= 0)
        {
            Log.Error("背景滚动配置构建失败：无效关卡 Id，LevelId={0}。", levelId);
            return false;
        }

        if (GameEntry.DataTable == null)
        {
            Log.Error("背景滚动配置构建失败：DataTable 组件不可用。");
            return false;
        }

        if (!GameEntry.DataTable.HasDataTable<DRBgChunkConfig>(ChunkDataTableName))
        {
            Log.Error("背景滚动配置构建失败：未找到数据表 {0}。", ChunkDataTableName);
            return false;
        }

        if (!GameEntry.DataTable.HasDataTable<DRBgThemeSegmentConfig>(ThemeSegmentDataTableName))
        {
            Log.Error("背景滚动配置构建失败：未找到数据表 {0}。", ThemeSegmentDataTableName);
            return false;
        }

        if (!GameEntry.DataTable.HasDataTable<DRBgLevelThemeConfig>(LevelThemeDataTableName))
        {
            Log.Error("背景滚动配置构建失败：未找到数据表 {0}。", LevelThemeDataTableName);
            return false;
        }

        IDataTable<DRBgChunkConfig> chunkDataTable = GameEntry.DataTable.GetDataTable<DRBgChunkConfig>(ChunkDataTableName);
        IDataTable<DRBgThemeSegmentConfig> themeSegmentDataTable = GameEntry.DataTable.GetDataTable<DRBgThemeSegmentConfig>(ThemeSegmentDataTableName);
        IDataTable<DRBgLevelThemeConfig> levelThemeDataTable = GameEntry.DataTable.GetDataTable<DRBgLevelThemeConfig>(LevelThemeDataTableName);

        if (chunkDataTable == null || themeSegmentDataTable == null || levelThemeDataTable == null)
        {
            Log.Error("背景滚动配置构建失败：存在空数据表实例。");
            return false;
        }

        DRBgChunkConfig[] chunkRows = chunkDataTable.GetAllDataRows();
        DRBgThemeSegmentConfig[] themeSegmentRows = themeSegmentDataTable.GetAllDataRows();
        DRBgLevelThemeConfig[] levelThemeRows = levelThemeDataTable.GetAllDataRows();

        if (chunkRows == null || chunkRows.Length == 0)
        {
            Log.Error("背景滚动配置构建失败：Chunk 数据表为空。");
            return false;
        }

        if (themeSegmentRows == null || themeSegmentRows.Length == 0)
        {
            Log.Error("背景滚动配置构建失败：ThemeSegment 数据表为空。");
            return false;
        }

        if (levelThemeRows == null || levelThemeRows.Length == 0)
        {
            Log.Error("背景滚动配置构建失败：LevelTheme 数据表为空。");
            return false;
        }

        DRBgLevelThemeConfig levelThemeRow;
        if (!TryGetLevelThemeRow(levelThemeRows, levelId, out levelThemeRow))
        {
            return false;
        }

        List<BgChunkDefinition> chunkDefinitions = new List<BgChunkDefinition>(chunkRows.Length);
        for (int i = 0; i < chunkRows.Length; i++)
        {
            DRBgChunkConfig row = chunkRows[i];
            if (row == null)
            {
                continue;
            }

            chunkDefinitions.Add(new BgChunkDefinition(
                row.EntityRelativePath,
                row.ThemeTag,
                row.Weight,
                ParseCanFollowThemes(row.CanFollowThemes)));
        }

        int[] themeGroupIds = levelThemeRow.ThemeGroupIds;
        List<BgThemeSegment> themeSegments = new List<BgThemeSegment>(themeSegmentRows.Length * themeGroupIds.Length);
        for (int i = 0; i < themeGroupIds.Length; i++)
        {
            int themeGroupId = themeGroupIds[i];
            List<DRBgThemeSegmentConfig> groupThemeRows = GetThemeRowsByGroup(themeSegmentRows, themeGroupId);
            if (groupThemeRows.Count == 0)
            {
                Log.Error("背景滚动配置构建失败：关卡 {0} 引用的主题组 {1} 无有效 ThemeSegment 配置。", levelId, themeGroupId);
                return false;
            }

            for (int j = 0; j < groupThemeRows.Count; j++)
            {
                DRBgThemeSegmentConfig row = groupThemeRows[j];
                DRBgThemeSegmentConfig.ThemeChunkSegment[] segments = row.Segments;
                if (segments == null || segments.Length == 0)
                {
                    Log.Error("背景滚动配置构建失败：关卡 {0} 的主题组 {1} 存在空片段配置，RowId={2}。", levelId, themeGroupId, row.Id);
                    return false;
                }

                for (int k = 0; k < segments.Length; k++)
                {
                    DRBgThemeSegmentConfig.ThemeChunkSegment segment = segments[k];
                    if (segment == null || string.IsNullOrWhiteSpace(segment.ThemeTag) || segment.ChunkCount <= 0)
                    {
                        Log.Error("背景滚动配置构建失败：关卡 {0} 的主题组 {1} 存在非法片段配置，RowId={2}，SegmentIndex={3}。", levelId, themeGroupId, row.Id, k);
                        return false;
                    }

                    themeSegments.Add(new BgThemeSegment(segment.ThemeTag, segment.ChunkCount));
                }
            }
        }

        _chunkDefinitions = chunkDefinitions.ToArray();
        _themeSegments = themeSegments.ToArray();
        _loopThemeSegments = levelThemeRow.LoopThemeSegments;
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
    /// 初始化主题状态。
    /// </summary>
    private void InitThemeState()
    {
        _isThemeSequenceCompleted = false;
        _pendingStopAtTopSpawnLine = false;
        _finalChunkEntityId = -1;
        _currentThemeSegmentIndex = 0;

        if (_themeSegments == null || _themeSegments.Length == 0)
        {
            _currentThemeSegmentRemainChunkCount = int.MaxValue;
            return;
        }

        _currentThemeSegmentRemainChunkCount = Mathf.Max(1, _themeSegments[0].ChunkCount);
    }

    /// <summary>
    /// 推进主题剩余块数。
    /// 当剩余块数耗尽时，切换到下一主题段。
    /// </summary>
    /// <param name="count">需要消耗的背景块数。</param>
    private void ConsumeThemeChunkCount(int count)
    {
        if (count <= 0)
        {
            return;
        }

        if (_themeSegments == null || _themeSegments.Length == 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            _currentThemeSegmentRemainChunkCount--;
            if (_currentThemeSegmentRemainChunkCount <= 0)
            {
                MoveToNextThemeSegment();
            }
        }
    }

    /// <summary>
    /// 主题序列是否允许继续生成背景块。
    /// </summary>
    /// <returns>可继续生成返回 true。</returns>
    private bool CanSpawnMoreChunksByThemeSequence()
    {
        if (_themeSegments == null || _themeSegments.Length == 0)
        {
            return true;
        }

        return !_isThemeSequenceCompleted || _loopThemeSegments;
    }

    /// <summary>
    /// 切换到下一主题段。
    /// </summary>
    private void MoveToNextThemeSegment()
    {
        if (_themeSegments == null || _themeSegments.Length == 0)
        {
            _currentThemeSegmentRemainChunkCount = int.MaxValue;
            _isThemeSequenceCompleted = false;
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
                _currentThemeSegmentIndex = nextIndex;
                _currentThemeSegmentRemainChunkCount = 0;
                _isThemeSequenceCompleted = true;
                return;
            }
        }

        _currentThemeSegmentIndex = Mathf.Clamp(nextIndex, 0, _themeSegments.Length - 1);
        _currentThemeSegmentRemainChunkCount = Mathf.Max(1, _themeSegments[_currentThemeSegmentIndex].ChunkCount);
        _isThemeSequenceCompleted = false;
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

    private bool TryGetLevelThemeRow(
        DRBgLevelThemeConfig[] levelThemeRows,
        int levelId,
        out DRBgLevelThemeConfig levelThemeRow)
    {
        levelThemeRow = null;
        int duplicateRowId = -1;

        for (int i = 0; i < levelThemeRows.Length; i++)
        {
            DRBgLevelThemeConfig row = levelThemeRows[i];
            if (row == null || row.Id != levelId)
            {
                continue;
            }

            if (levelThemeRow != null)
            {
                duplicateRowId = row.Id;
                Log.Error("背景滚动配置构建失败：关卡 {0} 存在重复配置，至少包含 Id={1} 与 Id={2}。", levelId, levelThemeRow.Id, duplicateRowId);
                return false;
            }

            levelThemeRow = row;
        }

        if (levelThemeRow == null)
        {
            Log.Error("背景滚动配置构建失败：关卡 {0} 未配置主题组序列。", levelId);
            return false;
        }

        if (levelThemeRow.ThemeGroupIds == null || levelThemeRow.ThemeGroupIds.Length == 0)
        {
            Log.Error("背景滚动配置构建失败：关卡 {0} 的 ThemeGroupIds 为空。", levelId);
            return false;
        }

        return true;
    }

    private static List<DRBgThemeSegmentConfig> GetThemeRowsByGroup(DRBgThemeSegmentConfig[] themeSegmentRows, int themeGroupId)
    {
        List<DRBgThemeSegmentConfig> result = new List<DRBgThemeSegmentConfig>(themeSegmentRows.Length);
        for (int i = 0; i < themeSegmentRows.Length; i++)
        {
            DRBgThemeSegmentConfig row = themeSegmentRows[i];
            if (row == null || row.Id != themeGroupId)
            {
                continue;
            }

            result.Add(row);
        }

        return result;
    }

    private static string[] ParseCanFollowThemes(string canFollowThemes)
    {
        if (string.IsNullOrWhiteSpace(canFollowThemes))
        {
            return null;
        }

        string[] source = canFollowThemes.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        if (source == null || source.Length == 0)
        {
            return null;
        }

        List<string> result = new List<string>(source.Length);
        for (int i = 0; i < source.Length; i++)
        {
            string value = source[i].Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            result.Add(value);
        }

        return result.Count == 0 ? null : result.ToArray();
    }

}
