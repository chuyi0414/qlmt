using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 数据表管理器（统一数据表加载入口）。
/// </summary>
public sealed class DataTableManagerComponent : GameFrameworkComponent
{
    /// <summary>
    /// 背景块数据表相对路径。
    /// </summary>
    private const string BgBlockDataTableRelativePath = "BgScroll/BgBlock";
    /// <summary>
    /// 背景主题数据表相对路径。
    /// </summary>
    private const string BgThemeDataTableRelativePath = "BgScroll/BgTheme";
    /// <summary>
    /// 关卡数据表相对路径。
    /// </summary>
    private const string LevelDataTableRelativePath = "Level/Level";
    /// <summary>
    /// 角色数据表相对路径。
    /// </summary>
    private const string CharacterDataTableRelativePath = "Entity/Character";

    /// <summary>
    /// 加载游戏内全部数据表。
    /// </summary>
    /// <returns>全部加载成功返回 true。</returns>
    public bool LoadAllDataTables()
    {
        bool success = true;
        success &= LoadCombatBackgroundDataTables();
        success &= LoadCharacterDataTable();
        return success;
    }

    /// <summary>
    /// 检查战斗背景数据表是否已创建。
    /// </summary>
    /// <returns>全部已创建返回 true。</returns>
    public bool HasCombatBackgroundDataTables()
    {
        return GameEntry.DataTable.HasDataTable<DRBgBlock>() &&
               GameEntry.DataTable.HasDataTable<DRBgTheme>() &&
               GameEntry.DataTable.HasDataTable<DRLevel>();
    }

    /// <summary>
    /// 检查角色数据表是否已创建。
    /// </summary>
    /// <returns>已创建返回 true。</returns>
    public bool HasCharacterDataTable()
    {
        return GameEntry.DataTable.HasDataTable<DRCharacter>();
    }

    /// <summary>
    /// 加载战斗背景系统所需的数据表。
    /// </summary>
    /// <returns>全部加载成功返回 true。</returns>
    public bool LoadCombatBackgroundDataTables()
    {
        return LoadDataTable<DRBgBlock>(BgBlockDataTableRelativePath) &&
               LoadDataTable<DRBgTheme>(BgThemeDataTableRelativePath) &&
               LoadDataTable<DRLevel>(LevelDataTableRelativePath);
    }

    /// <summary>
    /// 加载角色系统所需数据表。
    /// </summary>
    private bool LoadCharacterDataTable()
    {
        return LoadDataTable<DRCharacter>(CharacterDataTableRelativePath);
    }

    /// <summary>
    /// 同步加载一张文本数据表并解析。
    /// </summary>
    /// <typeparam name="T">数据行类型。</typeparam>
    /// <param name="relativePath">DataTables 根目录下相对路径（不含扩展名）。</param>
    /// <returns>加载并解析成功返回 true。</returns>
    public bool LoadDataTable<T>(string relativePath) where T : DataRowBase, new()
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            Log.Error("加载数据表失败，路径为空。");
            return false;
        }

        string dataTablePath = GameAssetPath.GetDataTable(relativePath);
        TextAsset textAsset = Resources.Load<TextAsset>(dataTablePath);
        if (textAsset == null)
        {
            Log.Error("加载数据表失败，资源不存在：{0}", dataTablePath);
            return false;
        }

        IDataTable<T> dataTable = GameEntry.DataTable.HasDataTable<T>()
            ? GameEntry.DataTable.GetDataTable<T>()
            : GameEntry.DataTable.CreateDataTable<T>();
        dataTable.RemoveAllDataRows();

        DataTableBase dataTableBase = dataTable as DataTableBase;
        if (dataTableBase == null || !dataTableBase.ParseData(textAsset.text))
        {
            Log.Error("解析数据表失败：{0}", dataTablePath);
            return false;
        }

        return true;
    }
}
