using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

/// <summary>
/// 加载界面。
/// </summary>
public class LoadUIForm : UIFormLogic
{
    [SerializeField]
    private Button _ButtonLoad;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        _ButtonLoad.onClick.AddListener(OnButtonLoadClick);
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
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

    /// <summary>
    /// 设置加载按钮显隐。
    /// </summary>
    /// <param name="visible">是否显示按钮。</param>
    public void SetLoadButtonVisible(bool visible)
    {
        _ButtonLoad.gameObject.SetActive(visible);
    }
}
