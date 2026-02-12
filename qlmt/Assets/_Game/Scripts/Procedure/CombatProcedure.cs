using GameFramework.Fsm;
using GameFramework.Procedure;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗流程
/// </summary>
public class CombatProcedure : ProcedureBase
{
    private const int CurrentLevelId = 1;
    private int _combatUIId;
    protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnInit(procedureOwner);

    }

    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        GameEntry.Entity.ShowEntity<CharacterEntity>(
            GameEntry.EntityIdPool.Acquire(), 
            GameAssetPath.GetEntity("Unit/Character/CharacterEntity"),
            "Character",
            userData : 1);
        _combatUIId = GameEntry.UI.OpenUIForm(GameAssetPath.GetUI("Combat/CombatUI"), "Main");
        GameEntry.BgScroll.ManualStartScroll(CurrentLevelId, true);

    }

    protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);


    }

    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        base.OnLeave(procedureOwner, isShutdown);
        GameEntry.UI.CloseUIForm(_combatUIId);
    }
}
