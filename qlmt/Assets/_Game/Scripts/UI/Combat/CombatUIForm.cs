using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

/// <summary>
/// 战斗UI
/// </summary>
public class CombatUIForm : UIFormLogic
{
    /// <summary>
    /// 继续逃亡
    /// </summary>
    [SerializeField]private Button _buttonContinue;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        _buttonContinue.onClick.AddListener(OnContinueButtonClick);
        _buttonContinue.gameObject.SetActive(true);
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        base.OnClose(isShutdown, userData);
        _buttonContinue.onClick.RemoveListener(OnContinueButtonClick);
    }

    private void OnContinueButtonClick()
    {
        GameEntry.CombatManager.StartBackgroundScroll();
        _buttonContinue.gameObject.SetActive(false);
    }
}
