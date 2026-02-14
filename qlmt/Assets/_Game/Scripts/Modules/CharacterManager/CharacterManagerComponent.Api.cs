using GameFramework.DataTable;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 角色管理器组件（对外接口与基础运行时）。
/// </summary>
public sealed partial class CharacterManagerComponent
{
    /// <summary>
    /// 尝试创建角色实体。
    /// </summary>
    /// <param name="characterConfigId">角色配置 Id。</param>
    /// <param name="spawnPosition">出生世界坐标。</param>
    /// <param name="entityId">返回创建请求的实体 Id。</param>
    /// <returns>提交创建请求成功返回 true。</returns>
    public bool TrySpawnCharacter(int characterConfigId, Vector3 spawnPosition, out int entityId)
    {
        entityId = 0;
        if (!EnsureCharacterDataTableReady())
        {
            return false;
        }

        IDataTable<DRCharacter> characterTable = GameEntry.DataTable.GetDataTable<DRCharacter>();
        DRCharacter characterRow = characterTable != null ? characterTable.GetDataRow(characterConfigId) : null;
        if (characterRow == null)
        {
            Log.Error("创建角色失败，角色配置不存在：{0}", characterConfigId);
            return false;
        }

        if (string.IsNullOrEmpty(characterRow.EntityPath))
        {
            Log.Error("创建角色失败，实体路径为空：{0}", characterConfigId);
            return false;
        }

        EnsureCharacterEntityGroup();
        EnsureEntityEventSubscription();

        entityId = GameEntry.EntityIdPool.Acquire();
        if (entityId <= 0)
        {
            Log.Error("创建角色失败，无法分配有效实体 Id。");
            return false;
        }

        _pendingRequests[entityId] = new CharacterSpawnRequest
        {
            CharacterConfigId = characterConfigId
        };

        CharacterEntity.CharacterShowData showData = new CharacterEntity.CharacterShowData
        {
            SpawnPosition = spawnPosition,
            CampType = characterRow.CampType,
            MaxHp = characterRow.Hp,
            CurrentHp = characterRow.Hp,
            Attack = characterRow.Attack,
            MoveSpeed = characterRow.MoveSpeed
        };

        GameEntry.Entity.ShowEntity<CharacterEntity>(
            entityId,
            GameAssetPath.GetEntity(characterRow.EntityPath),
            _characterEntityGroupName,
            showData);
        return true;
    }

    /// <summary>
    /// 尝试获取角色快照。
    /// </summary>
    /// <param name="entityId">实体 Id。</param>
    /// <param name="snapshot">角色快照。</param>
    /// <returns>获取成功返回 true。</returns>
    public bool TryGetCharacterSnapshot(int entityId, out CharacterSnapshot snapshot)
    {
        snapshot = default(CharacterSnapshot);
        if (!_characterRuntimes.TryGetValue(entityId, out CharacterRuntime runtime) || runtime.Entity == null)
        {
            return false;
        }

        snapshot.EntityId = runtime.EntityId;
        snapshot.CharacterConfigId = runtime.CharacterConfigId;
        snapshot.CampType = runtime.Entity.CampType;
        snapshot.MaxHp = runtime.Entity.MaxHp;
        snapshot.CurrentHp = runtime.Entity.CurrentHp;
        snapshot.Attack = runtime.Entity.Attack;
        snapshot.MoveSpeed = runtime.Entity.MoveSpeed;
        return true;
    }

    /// <summary>
    /// 尝试隐藏角色实体。
    /// </summary>
    /// <param name="entityId">实体 Id。</param>
    /// <returns>发起成功返回 true。</returns>
    public bool TryHideCharacter(int entityId)
    {
        if (entityId <= 0)
        {
            Log.Warning("隐藏角色失败，实体 Id 非法：{0}", entityId);
            return false;
        }

        if (_pendingRequests.Remove(entityId))
        {
            _abortedRequestIds.Add(entityId);
            if (GameEntry.Entity.HasEntity(entityId))
            {
                GameEntry.Entity.HideEntity(entityId);
            }

            return true;
        }

        if (!_characterRuntimes.Remove(entityId))
        {
            return false;
        }

        if (GameEntry.Entity.HasEntity(entityId))
        {
            GameEntry.Entity.HideEntity(entityId);
        }
        else
        {
            GameEntry.EntityIdPool.Release(entityId);
        }

        return true;
    }

    /// <summary>
    /// 清理全部角色运行时状态。
    /// </summary>
    public void ClearAllCharacters()
    {
        foreach (KeyValuePair<int, CharacterRuntime> pair in _characterRuntimes)
        {
            int entityId = pair.Key;
            if (entityId <= 0)
            {
                continue;
            }

            if (GameEntry.Entity.HasEntity(entityId))
            {
                GameEntry.Entity.HideEntity(entityId);
            }
            else
            {
                GameEntry.EntityIdPool.Release(entityId);
            }
        }
        _characterRuntimes.Clear();

        foreach (KeyValuePair<int, CharacterSpawnRequest> pair in _pendingRequests)
        {
            _abortedRequestIds.Add(pair.Key);
        }
        _pendingRequests.Clear();
    }

    /// <summary>
    /// 组件销毁时清理状态与事件订阅。
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeEntityEvents();
        ClearAllCharacters();
    }

    /// <summary>
    /// 确保角色数据表已在加载流程中完成加载。
    /// </summary>
    private bool EnsureCharacterDataTableReady()
    {
        if (GameEntry.DataTableManager == null)
        {
            Log.Error("角色数据表未就绪，DataTableManagerComponent 未挂载。");
            return false;
        }

        if (!GameEntry.DataTableManager.HasCharacterDataTable())
        {
            Log.Error("角色数据表未就绪，请先在 LoadProcedure 中加载。");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 确保角色实体组存在。
    /// </summary>
    private void EnsureCharacterEntityGroup()
    {
        if (GameEntry.Entity.HasEntityGroup(_characterEntityGroupName))
        {
            return;
        }

        GameEntry.Entity.AddEntityGroup(_characterEntityGroupName, 10f, 32, 60f, -10);
    }
}
