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
        _buttonContinue.onClick.AddListener(OnContinueButtonClick);
    }

    private void OnContinueButtonClick()
    {
        _buttonContinue.gameObject.SetActive(false);
    }
}
