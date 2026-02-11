using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

/// <summary>
/// º”‘ÿUI
/// </summary>
public class LoadUIForm : UIFormLogic
{
    /// <summary>
    /// º”‘ÿ∞¥≈•
    /// </summary>
    [SerializeField]private Button _ButtonLoad;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        _ButtonLoad.onClick.AddListener(OnButtonLoadClick);
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        base.OnClose(isShutdown, userData);
        _ButtonLoad.onClick.RemoveListener(OnButtonLoadClick);
    }

    private void OnButtonLoadClick()
    {
        GameFramework.Procedure.ProcedureBase currentProcedure = GameEntry.Procedure.CurrentProcedure;
        currentProcedure.ChangeState<MainProcedure>(currentProcedure.procedureOwner);
    }
}
