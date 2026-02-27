using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

/// <summary>
/// 加载UI
/// </summary>
public class LoadUIForm : UIFormLogic
{
    /// <summary>
    /// 进入游戏按钮
    /// </summary>
    [SerializeField]
    private Button _enterGameBtn;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        _enterGameBtn.onClick.AddListener(OnEnterGameBtnClick);
    }

    /// <summary>
    /// 进入main流程
    /// </summary>
    private void OnEnterGameBtnClick()
    {
        GameFramework.Procedure.ProcedureBase currentProcedure = GameEntry.Procedure.CurrentProcedure;
        currentProcedure.ChangeState<MainProcedure>(currentProcedure.procedureOwner);
    }
}
