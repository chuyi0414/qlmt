using System;
using System.Globalization;
using GameFramework;
using UnityGameFramework.Runtime;

/// <summary>
/// 单位基础配置数据行。
/// </summary>
public sealed class DRUnitConfig : DataRowBase
{
    /// <summary>
    /// 配置表列数。
    /// </summary>
    private const int ColumnCount = 4;

    /// <summary>
    /// 主键 Id 的内部存储，对应数据表的 Id 列。
    /// </summary>
    private int m_Id;
    /// <summary>
    /// 数据行唯一 Id。
    /// </summary>
    public override int Id => m_Id;

    /// <summary>
    /// 单位血量。
    /// </summary>
    public float Health { get; set; }

    /// <summary>
    /// 单位攻击力。
    /// </summary>
    public float Attack { get; set; }

    /// <summary>
    /// 单位速度。
    /// </summary>
    public float Speed { get; set; }

    public override bool ParseDataRow(string dataRowString, object userData)
    {
        if (string.IsNullOrWhiteSpace(dataRowString))
        {
            Log.Warning("单位配置解析失败：空数据行。");
            return false;
        }

        string[] columns = dataRowString.Split(new[] { '\t' }, StringSplitOptions.None);
        if (columns.Length != ColumnCount)
        {
            Log.Warning("单位配置解析失败：列数错误，Expected={0}，Actual={1}，Raw={2}。", ColumnCount, columns.Length, dataRowString);
            return false;
        }

        int id;
        float health;
        float attack;
        float speed;
        if (!TryParseId(columns[0], dataRowString, out id)
            || !TryParsePositiveFloat(columns[1], id, "Health", out health)
            || !TryParseNonNegativeFloat(columns[2], id, "Attack", out attack)
            || !TryParseNonNegativeFloat(columns[3], id, "Speed", out speed))
        {
            return false;
        }

        m_Id = id;
        Health = health;
        Attack = attack;
        Speed = speed;
        return true;
    }

    public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
    {
        return ParseDataRow(Utility.Converter.GetString(dataRowBytes, startIndex, length), userData);
    }

    /// <summary>
    /// 解析并校验 Id。
    /// </summary>
    /// <param name="rawId">原始 Id 字符串。</param>
    /// <param name="rawRow">原始数据行。</param>
    /// <param name="id">解析后的 Id。</param>
    /// <returns>解析成功返回 true。</returns>
    private static bool TryParseId(string rawId, string rawRow, out int id)
    {
        if (!int.TryParse(rawId, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
        {
            Log.Warning("单位配置解析失败：Id 非法，Raw={0}。", rawRow);
            return false;
        }

        if (id > 0)
        {
            return true;
        }

        Log.Warning("单位配置解析失败：Id 必须大于 0，Value={0}。", id);
        return false;
    }

    /// <summary>
    /// 解析并校验必须大于 0 的浮点字段。
    /// </summary>
    /// <param name="rawValue">原始字段值。</param>
    /// <param name="id">数据行 Id。</param>
    /// <param name="fieldName">字段名。</param>
    /// <param name="value">解析后的值。</param>
    /// <returns>解析成功返回 true。</returns>
    private static bool TryParsePositiveFloat(string rawValue, int id, string fieldName, out float value)
    {
        if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            Log.Warning("单位配置解析失败：{0} 非法，Id={1}，Value={2}。", fieldName, id, rawValue);
            return false;
        }

        if (value > 0f)
        {
            return true;
        }

        Log.Warning("单位配置解析失败：{0} 必须大于 0，Id={1}，Value={2}。", fieldName, id, value);
        return false;
    }

    /// <summary>
    /// 解析并校验必须大于等于 0 的浮点字段。
    /// </summary>
    /// <param name="rawValue">原始字段值。</param>
    /// <param name="id">数据行 Id。</param>
    /// <param name="fieldName">字段名。</param>
    /// <param name="value">解析后的值。</param>
    /// <returns>解析成功返回 true。</returns>
    private static bool TryParseNonNegativeFloat(string rawValue, int id, string fieldName, out float value)
    {
        if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            Log.Warning("单位配置解析失败：{0} 非法，Id={1}，Value={2}。", fieldName, id, rawValue);
            return false;
        }

        if (value >= 0f)
        {
            return true;
        }

        Log.Warning("单位配置解析失败：{0} 不能小于 0，Id={1}，Value={2}。", fieldName, id, value);
        return false;
    }
}
