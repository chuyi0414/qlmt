using System;
using System.Globalization;
using GameFramework;
using UnityGameFramework.Runtime;

/// <summary>
/// 背景块配置数据行。
/// </summary>
public sealed class DRBgChunkConfig : DataRowBase
{
    /// <summary>
    /// 配置表列数。
    /// </summary>
    private const int ColumnCount = 5;

    /// <summary>
    /// 数据行主键。
    /// </summary>
    public int ConfigId { get; set; }

    /// <summary>
    /// 背景块实体相对路径（相对 EntityRoot）。
    /// </summary>
    public string EntityRelativePath { get; set; }

    /// <summary>
    /// 背景块主题标签。
    /// </summary>
    public string ThemeTag { get; set; }

    /// <summary>
    /// 背景块权重。
    /// </summary>
    public float Weight { get; set; }

    /// <summary>
    /// 可跟随主题原始配置串（| 分隔）。
    /// </summary>
    public string CanFollowThemes { get; set; }

    public override int Id
    {
        get { return ConfigId; }
    }

    public override bool ParseDataRow(string dataRowString, object userData)
    {
        if (string.IsNullOrWhiteSpace(dataRowString))
        {
            Log.Warning("背景块配置解析失败：空数据行。");
            return false;
        }

        string[] columns = dataRowString.Split(new[] { '\t' }, StringSplitOptions.None);
        if (columns.Length != ColumnCount)
        {
            Log.Warning("背景块配置解析失败：列数错误，Expected={0}，Actual={1}，Raw={2}。", ColumnCount, columns.Length, dataRowString);
            return false;
        }

        int id;
        if (!int.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
        {
            Log.Warning("背景块配置解析失败：Id 非法，Raw={0}。", dataRowString);
            return false;
        }

        string entityRelativePath = columns[1].Trim();
        if (string.IsNullOrWhiteSpace(entityRelativePath))
        {
            Log.Warning("背景块配置解析失败：EntityRelativePath 为空，Id={0}。", id);
            return false;
        }

        float weight;
        if (!float.TryParse(columns[3], NumberStyles.Float, CultureInfo.InvariantCulture, out weight))
        {
            Log.Warning("背景块配置解析失败：Weight 非法，Id={0}，Value={1}。", id, columns[3]);
            return false;
        }

        if (weight < 0f)
        {
            Log.Warning("背景块配置解析失败：Weight 不能小于 0，Id={0}，Value={1}。", id, weight);
            return false;
        }

        ConfigId = id;
        EntityRelativePath = entityRelativePath;
        ThemeTag = columns[2].Trim();
        Weight = weight;
        CanFollowThemes = columns[4].Trim();
        return true;
    }

    public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
    {
        return ParseDataRow(Utility.Converter.GetString(dataRowBytes, startIndex, length), userData);
    }
}
