using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 战斗组件（背景滚动数据与生成逻辑）。
/// </summary>
public partial class CombatManager
{
    /// <summary>
    /// 检查背景系统所需数据表是否已在加载流程准备完成。
    /// </summary>
    private bool EnsureBackgroundDataTablesReady()
    {
        if (GameEntry.DataTableManager == null)
        {
            Log.Error("背景数据表未就绪，DataTableManagerComponent 未挂载。");
            return false;
        }

        if (!GameEntry.DataTableManager.HasCombatBackgroundDataTables())
        {
            Log.Error("背景数据表未就绪，请先在 LoadProcedure 中加载。");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 根据关卡构建主题顺序。
    /// </summary>
    private bool BuildThemeSequence(int levelId)
    {
        IDataTable<DRLevel> levelTable = GameEntry.DataTable.GetDataTable<DRLevel>();
        DRLevel levelRow = levelTable != null ? levelTable.GetDataRow(levelId) : null;
        if (levelRow == null)
        {
            Log.Error("关卡不存在：{0}", levelId);
            return false;
        }

        IDataTable<DRBgTheme> themeTable = GameEntry.DataTable.GetDataTable<DRBgTheme>();
        DRBgTheme themeRow = themeTable != null ? themeTable.GetDataRow(levelRow.ThemeId) : null;
        if (themeRow == null)
        {
            Log.Error("关卡主题不存在：{0}", levelRow.ThemeId);
            return false;
        }

        _themeSequence.Clear();
        for (int i = 0; i < themeRow.BgPairs.Count; i++)
        {
            DRBgTheme.BgPair pair = themeRow.BgPairs[i];
            for (int j = 0; j < pair.Count; j++)
            {
                _themeSequence.Add(pair.BgBlockId);
            }
        }

        if (_themeSequence.Count == 0)
        {
            Log.Error("关卡主题未配置可用背景：{0}", levelRow.ThemeId);
            return false;
        }

        _themeCursor = 0;
        _isThemeLoop = levelRow.IsLoopTheme;
        _needStopWhenTopReachCamera = false;
        return true;
    }

    /// <summary>
    /// 尝试补齐最小背景数量（单次触发一个异步创建请求）。
    /// </summary>
    private void FillBackgroundUntilMinCount()
    {
        if (!_isPrepared || _pendingRequests.Count > 0 || _activeBackgrounds.Count >= _minBackgroundCount)
        {
            return;
        }

        TrySpawnNextBackground();
    }

    /// <summary>
    /// 尝试创建下一个背景块。
    /// </summary>
    private void TrySpawnNextBackground()
    {
        if (!TryGetNextBgBlockId(out int bgBlockId))
        {
            _needStopWhenTopReachCamera = !_isThemeLoop;
            return;
        }

        IDataTable<DRBgBlock> blockTable = GameEntry.DataTable.GetDataTable<DRBgBlock>();
        DRBgBlock blockRow = blockTable != null ? blockTable.GetDataRow(bgBlockId) : null;
        if (blockRow == null || string.IsNullOrEmpty(blockRow.EntityPath))
        {
            Log.Error("背景块配置不存在或实体路径为空：{0}", bgBlockId);
            return;
        }

        if (!TryGetNextSpawnPosition(out Vector3 spawnPosition))
        {
            return;
        }

        int entityId = GameEntry.EntityIdPool.Acquire();
        if (entityId <= 0)
        {
            Log.Error("背景创建失败，无法分配有效实体 Id。");
            return;
        }

        _pendingRequests[entityId] = new SpawnRequest { BgBlockId = bgBlockId, SpawnPosition = spawnPosition };
        GameEntry.Entity.ShowEntity<BgEntity>(
            entityId,
            GameAssetPath.GetEntity(blockRow.EntityPath),
            _bgEntityGroupName,
            spawnPosition);
    }

    /// <summary>
    /// 获取下一个背景块 Id。
    /// </summary>
    private bool TryGetNextBgBlockId(out int bgBlockId)
    {
        bgBlockId = 0;
        if (_themeSequence.Count == 0)
        {
            return false;
        }

        if (_themeCursor >= _themeSequence.Count)
        {
            if (_isThemeLoop)
            {
                _themeCursor = 0;
            }
            else
            {
                return false;
            }
        }

        bgBlockId = _themeSequence[_themeCursor++];
        return true;
    }
}
