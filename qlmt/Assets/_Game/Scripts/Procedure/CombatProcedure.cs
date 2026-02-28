using Game.Character;
using Game.DataTable;
using Game.Entity;
using GameFramework.Fsm;
using GameFramework.Procedure;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 战斗流程
/// </summary>
public class CombatProcedure : ProcedureBase
{
    protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnInit(procedureOwner);

    }

    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);

        // 1. 获取角色配置表
        GameFramework.DataTable.IDataTable<DRCharacter> characterTable = 
            GameEntry.DataTable.GetDataTable<DRCharacter>();
        
        if (characterTable == null)
        {
            Debug.LogError("角色配置表未加载，无法创建主角");
            return;
        }

        // 2. 读取主角配置（ID=1001）
        DRCharacter playerConfig = characterTable.GetDataRow(1001);
        if (playerConfig == null)
        {
            Debug.LogError("找不到主角配置 ID=1001");
            return;
        }
        
        // 3. 构造角色显示数据
        CharacterData playerData = new CharacterData
        {
            CharacterId = playerConfig.Id,
            Name = playerConfig.Name,
            BaseStatsId = playerConfig.BaseStatsId,
            Identity = playerConfig.GetIdentity(),
            AbilityIds = new List<int>(playerConfig.AbilityIds),
            PersonalityIds = new List<int>(playerConfig.PersonalityIds),
            Position = new Vector3(0, 0, 0),  // 主角初始位置
            Rotation = Quaternion.identity
        };

        // 4. 显示主角实体
        int entityId = GameEntry.EntityIdPool.Acquire();
        string assetPath = GameAssetPath.GetEntity(playerConfig.AssetName);
        GameEntry.Entity.ShowEntity<CharacterEntity>(
            entityId,           // 实体ID
            assetPath,          // 资源路径
            "Character",          // 实体组名
            playerData          // 角色数据
        );

        
    }

    protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

    }

    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        base.OnLeave(procedureOwner, isShutdown);

    }

    protected override void OnDestroy(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnDestroy(procedureOwner);

    }
}
