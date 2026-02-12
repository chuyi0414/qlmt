using GameFramework.DataTable;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

/// <summary>
/// 加载流程。
/// </summary>
public class LoadProcedure : ProcedureBase
{
    /// <summary>
    /// BgChunk 配置表名称。
    /// </summary>
    private const string BgChunkDataTableName = "BgScroll/BgChunkConfig";

    /// <summary>
    /// BgThemeSegment 配置表名称。
    /// </summary>
    private const string BgThemeSegmentDataTableName = "BgScroll/BgThemeSegmentConfig";

    /// <summary>
    /// BgLevelTheme 配置表名称。
    /// </summary>
    private const string BgLevelThemeDataTableName = "BgScroll/BgLevelThemeConfig";

    /// <summary>
    /// Unit 配置表名称。
    /// </summary>
    private const string UnitDataTableName = "Entity/UnitConfig";

    /// <summary>
    /// 本流程需要加载的 DataTable 总数。
    /// </summary>
    private const int DataTableLoadCount = 4;

    private int _loadUIId;
    private int _loadedDataTableCount;
    private bool _hasLoadFailure;
    private bool _isLoadReady;
    private bool _isLoadButtonShown;

    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);

        _loadedDataTableCount = 0;
        _hasLoadFailure = false;
        _isLoadReady = false;
        _isLoadButtonShown = false;

        _loadUIId = GameEntry.UI.OpenUIForm(GameAssetPath.GetUI("Load/LoadUI"), "Main");

        GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
        GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);

        ReadAllDataTables();
    }

    protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        TryShowLoadButton();
    }

    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        base.OnLeave(procedureOwner, isShutdown);

        GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
        GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
        GameEntry.UI.CloseUIForm(_loadUIId);
    }

    /// <summary>
    /// 加载完成后显示加载按钮。
    /// </summary>
    private void TryShowLoadButton()
    {
        if (!_isLoadReady || _isLoadButtonShown)
        {
            return;
        }

        UIForm loadUI = GameEntry.UI.GetUIForm(_loadUIId);
        if (loadUI == null)
        {
            return;
        }

        LoadUIForm loadUIForm = loadUI.Logic as LoadUIForm;
        if (loadUIForm == null || !loadUIForm.Available)
        {
            return;
        }

        loadUIForm.SetLoadButtonVisible(true);
        _isLoadButtonShown = true;
    }

    /// <summary>
    /// 读取本流程所需的全部 DataTable。
    /// </summary>
    private void ReadAllDataTables()
    {
        if (GameEntry.DataTable == null)
        {
            _hasLoadFailure = true;
            Log.Error("加载流程中断：DataTable 组件不可用。");
            return;
        }

        ReadDataTable<DRBgChunkConfig>(BgChunkDataTableName);
        ReadDataTable<DRBgThemeSegmentConfig>(BgThemeSegmentDataTableName);
        ReadDataTable<DRBgLevelThemeConfig>(BgLevelThemeDataTableName);
        ReadDataTable<DRUnitConfig>(UnitDataTableName);
    }

    /// <summary>
    /// 创建并读取指定 DataTable。
    /// </summary>
    private void ReadDataTable<T>(string dataTableName) where T : class, IDataRow, new()
    {
        IDataTable<T> dataTable = GameEntry.DataTable.GetDataTable<T>(dataTableName) ?? GameEntry.DataTable.CreateDataTable<T>(dataTableName);
        DataTableBase dataTableBase = (DataTableBase)dataTable;
        dataTableBase.RemoveAllDataRows();
        dataTableBase.ReadData(
            GameAssetPath.GetDataTable(dataTableName),
            new object[]
            {
                this,
                dataTableName
            });
    }

    /// <summary>
    /// DataTable 读取成功回调。
    /// </summary>
    private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
    {
        LoadDataTableSuccessEventArgs eventArgs = e as LoadDataTableSuccessEventArgs;
        object[] userData = eventArgs != null ? eventArgs.UserData as object[] : null;
        if (userData == null || userData.Length < 2 || userData[0] != this || _hasLoadFailure)
        {
            return;
        }

        _loadedDataTableCount++;
        if (_loadedDataTableCount >= DataTableLoadCount)
        {
            _isLoadReady = true;
        }
    }

    /// <summary>
    /// DataTable 读取失败回调。
    /// </summary>
    private void OnLoadDataTableFailure(object sender, GameEventArgs e)
    {
        LoadDataTableFailureEventArgs eventArgs = e as LoadDataTableFailureEventArgs;
        object[] userData = eventArgs != null ? eventArgs.UserData as object[] : null;
        if (userData == null || userData.Length < 2 || userData[0] != this)
        {
            return;
        }

        _hasLoadFailure = true;
        _isLoadReady = false;
        Log.Error("加载流程中断：DataTable 配置加载失败，Table={0}，Error={1}。", userData[1], eventArgs.ErrorMessage);
    }
}
