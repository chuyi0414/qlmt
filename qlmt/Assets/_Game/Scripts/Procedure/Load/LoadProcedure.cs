using Game.DataTable;
using GameFramework.DataTable;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 加载流程。
/// </summary>
public partial class LoadProcedure : ProcedureBase
{
    private int _loadUIId;
    private bool _dataTableLoadComplete = false;

    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        _loadUIId = GameEntry.UI.OpenUIForm(GameAssetPath.GetUI("Load/LoadUIForm"), "Main");

        // 重置加载状态
        _dataTableLoadComplete = false;
        SubscribeDataTableEvents();

        // 开始加载数据表
        LoadDataTables();
    }

    protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

        // 等待数据表加载完成后切换到下一个流程
        if (_dataTableLoadComplete)
        {
            // TODO: 切换到下一个流程（例如主菜单或游戏主流程）
            // ChangeState<MainProcedure>(procedureOwner);
        }
    }

    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        base.OnLeave(procedureOwner, isShutdown);
        UnsubscribeDataTableEvents();
        GameEntry.UI.CloseUIForm(_loadUIId);
    }


}
