using UnityGameFramework.Runtime;

/// <summary>
/// 背景块配置行。
/// </summary>
public class DRBgBlock : DataRowBase
{
    /// <summary>
    /// 背景块 Id。
    /// </summary>
    private int _id;
    /// <summary>
    /// 背景块名称。
    /// </summary>
    private string _name;
    /// <summary>
    /// 背景实体路径（相对 EntityRoot）。
    /// </summary>
    private string _entityPath;

    /// <summary>
    /// 行 Id。
    /// </summary>
    public override int Id => _id;
    /// <summary>
    /// 背景块名称。
    /// </summary>
    public string Name => _name;
    /// <summary>
    /// 背景实体路径。
    /// </summary>
    public string EntityPath => _entityPath;

    /// <summary>
    /// 解析文本行。
    /// </summary>
    public override bool ParseDataRow(string dataRowString, object userData)
    {
        if (string.IsNullOrEmpty(dataRowString))
        {
            Log.Warning("DRBgBlock 解析失败，数据行为空。");
            return false;
        }

        string[] columns = dataRowString.Split('\t');
        if (columns.Length < 3)
        {
            Log.Warning("DRBgBlock 解析失败，列数量不足：{0}", dataRowString);
            return false;
        }

        if (!int.TryParse(columns[0].Trim(), out _id))
        {
            Log.Warning("DRBgBlock 解析失败，Id 非法：{0}", dataRowString);
            return false;
        }

        _name = columns[1].Trim();
        _entityPath = columns[2].Trim();
        if (string.IsNullOrEmpty(_entityPath))
        {
            Log.Warning("DRBgBlock 解析失败，实体路径为空：{0}", dataRowString);
            return false;
        }

        return true;
    }
}
