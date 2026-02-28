using Game.DataTable;
using GameFramework.DataTable;
using GameFramework.Event;
using System;
using UnityGameFramework.Runtime;

/// <summary>
/// 加载流程 - 数据表加载部分。
/// </summary>
public partial class LoadProcedure
{
    /// <summary>
    /// 待加载数据表总数。
    /// </summary>
    private int _totalDataTableCount = 0;

    /// <summary>
    /// 已完成回调（成功或失败）数量。
    /// </summary>
    private int _completedDataTableCount = 0;

    /// <summary>
    /// 加载失败数量。
    /// </summary>
    private int _failedDataTableCount = 0;

    /// <summary>
    /// 是否已订阅数据表加载事件。
    /// </summary>
    private bool _isDataTableEventSubscribed = false;

    /// <summary>
    /// 订阅数据表加载事件。
    /// </summary>
    private void SubscribeDataTableEvents()
    {
        if (_isDataTableEventSubscribed)
        {
            return;
        }

        if (GameEntry.Event == null)
        {
            Log.Error("订阅数据表事件失败，Event 组件为空。");
            return;
        }

        GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
        GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
        _isDataTableEventSubscribed = true;
    }

    /// <summary>
    /// 取消订阅数据表加载事件。
    /// </summary>
    private void UnsubscribeDataTableEvents()
    {
        if (!_isDataTableEventSubscribed || GameEntry.Event == null)
        {
            return;
        }

        if (GameEntry.Event.Check(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess))
        {
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
        }

        if (GameEntry.Event.Check(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure))
        {
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
        }

        _isDataTableEventSubscribed = false;
    }

    /// <summary>
    /// 加载所有数据表
    /// </summary>
    private void LoadDataTables()
    {
        Log.Info("开始加载数据表...");
        _totalDataTableCount = 0;
        _completedDataTableCount = 0;
        _failedDataTableCount = 0;
        _dataTableLoadComplete = false;

        // 加载角色基础属性表
        QueueLoadDataTable<DRCharacterBaseStats>("Character/DRCharacterBaseStats");

        // 加载角色配置表
        QueueLoadDataTable<DRCharacter>("Character/DRCharacter");

        // TODO: 加载其他数据表
        // QueueLoadDataTable<DRAbility>("Character/DRAbility");
        // QueueLoadDataTable<DRPersonality>("Character/DRPersonality");

        if (_totalDataTableCount == 0)
        {
            _dataTableLoadComplete = true;
            Log.Warning("未配置任何需要加载的数据表。");
        }
    }

    /// <summary>
    /// 记录一张待加载数据表并发起加载请求。
    /// </summary>
    /// <typeparam name="T">数据行类型。</typeparam>
    /// <param name="relativePath">DataTables 目录下相对路径（可省略扩展名）。</param>
    private void QueueLoadDataTable<T>(string relativePath) where T : DataRowBase, new()
    {
        _totalDataTableCount++;
        if (ReadDataTable<T>(relativePath))
        {
            return;
        }

        _completedDataTableCount++;
        _failedDataTableCount++;
        CheckDataTableLoadProgress();
    }

    /// <summary>
    /// 通过 GF 资源系统读取一张数据表。
    /// </summary>
    /// <typeparam name="T">数据行类型。</typeparam>
    /// <param name="relativePath">DataTables 目录下相对路径（可省略扩展名）。</param>
    /// <returns>是否成功发起加载请求。</returns>
    private bool ReadDataTable<T>(string relativePath) where T : DataRowBase, new()
    {
        string dataTableAssetName = GetDataTableAssetName(relativePath);
        if (string.IsNullOrEmpty(dataTableAssetName))
        {
            Log.Error("加载数据表失败，路径为空。");
            return false;
        }

        IDataTable<T> dataTable = GameEntry.DataTable.HasDataTable<T>()
            ? GameEntry.DataTable.GetDataTable<T>()
            : GameEntry.DataTable.CreateDataTable<T>();

        dataTable.RemoveAllDataRows();

        DataTableBase dataTableBase = dataTable as DataTableBase;
        if (dataTableBase == null)
        {
            Log.Error("加载数据表失败，DataTableBase 转换失败：{0}", dataTableAssetName);
            return false;
        }

        try
        {
            dataTableBase.ReadData(dataTableAssetName, this);
            return true;
        }
        catch (Exception exception)
        {
            Log.Error("读取数据表失败：{0}，{1}", dataTableAssetName, exception.Message);
            return false;
        }
    }

    /// <summary>
    /// 将相对路径转换为 GF 可读取的资源名。
    /// </summary>
    /// <param name="relativePath">DataTables 目录下相对路径。</param>
    /// <returns>GF 资源名。</returns>
    private string GetDataTableAssetName(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        return $"{GameAssetPath.GetDataTable(relativePath)}";
    }

    /// <summary>
    /// 数据表读取成功回调。
    /// </summary>
    private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
    {
        LoadDataTableSuccessEventArgs ne = e as LoadDataTableSuccessEventArgs;
        if (ne == null || !ReferenceEquals(ne.UserData, this))
        {
            return;
        }

        _completedDataTableCount++;
        Log.Info("数据表加载成功：{0} ({1}/{2})", ne.DataTableAssetName, _completedDataTableCount, _totalDataTableCount);
        CheckDataTableLoadProgress();
    }

    /// <summary>
    /// 数据表读取失败回调。
    /// </summary>
    private void OnLoadDataTableFailure(object sender, GameEventArgs e)
    {
        LoadDataTableFailureEventArgs ne = e as LoadDataTableFailureEventArgs;
        if (ne == null || !ReferenceEquals(ne.UserData, this))
        {
            return;
        }

        _completedDataTableCount++;
        _failedDataTableCount++;
        Log.Error("数据表加载失败：{0}，{1}", ne.DataTableAssetName, ne.ErrorMessage);
        CheckDataTableLoadProgress();
    }

    /// <summary>
    /// 检查所有数据表是否已加载完成。
    /// </summary>
    private void CheckDataTableLoadProgress()
    {
        if (_completedDataTableCount < _totalDataTableCount)
        {
            return;
        }

        if (_failedDataTableCount > 0)
        {
            _dataTableLoadComplete = false;
            Log.Error("数据表加载结束，失败 {0}/{1} 张。", _failedDataTableCount, _totalDataTableCount);
            return;
        }

        _dataTableLoadComplete = true;
        Log.Info("所有数据表加载完成，共 {0} 张。", _totalDataTableCount);
    }
}
