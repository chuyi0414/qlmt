using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

/// <summary>
/// MainUI
/// </summary>
public class MainUIForm : UIFormLogic
{
    /// <summary>
    /// 开始游戏
    /// </summary>
    [SerializeField]
    private Button _startGameBtn;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        _startGameBtn.onClick.AddListener(OnStartGameBtnClick);

    }

    /// <summary>
    /// 开始游戏方法
    /// </summary>
    private void OnStartGameBtnClick()
    {
        GameFramework.Procedure.ProcedureBase currentProcedure = GameEntry.Procedure.CurrentProcedure;
        currentProcedure.ChangeState<CombatProcedure>(currentProcedure.procedureOwner);
    }
}
