using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

/// <summary>
/// 加载流程。
/// </summary>
public class LoadProcedure : ProcedureBase
{
    private int _loadUIId;
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner); 
        _loadUIId = GameEntry.UI.OpenUIForm(GameAssetPath.GetUI("Load/LoadUI"),"Main");
        if (GameEntry.DataTableManager == null)
        {
            Log.Error("加载全部数据表失败，DataTableManagerComponent 未挂载。");
            return;
        }

        if (!GameEntry.DataTableManager.LoadAllDataTables())
        {
            Log.Error("加载全部数据表失败。");
        }
    }

    protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        
    }

    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        base.OnLeave(procedureOwner, isShutdown);
        GameEntry.UI.CloseUIForm(_loadUIId);
    }

}
