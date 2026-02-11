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
    /// ¿ªÊ¼ÓÎÏ·
    /// </summary>
    [SerializeField]private Button _buttonStartGame;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        _buttonStartGame.onClick.AddListener(OnButtonStartGameClick);
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        base.OnClose(isShutdown, userData);
        _buttonStartGame.onClick.RemoveListener(OnButtonStartGameClick);
    }

    private void OnButtonStartGameClick()
    {
        GameFramework.Procedure.ProcedureBase currentProcedure = GameEntry.Procedure.CurrentProcedure;
        currentProcedure.ChangeState<CombatProcedure>(currentProcedure.procedureOwner);
    }
}
