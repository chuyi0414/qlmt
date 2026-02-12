using System;
using System.Collections.Generic;
using System.Globalization;
using GameFramework;
using UnityGameFramework.Runtime;

/// <summary>
/// 关卡主题映射配置数据行。
/// </summary>
public sealed class DRBgLevelThemeConfig : DataRowBase
{
    /// <summary>
    /// 配置表列数。
    /// </summary>
    private const int ColumnCount = 3;

    /// <summary>
    /// 关卡 Id（同时作为数据行主键）。
    /// </summary>
    public int ConfigId { get; set; }

    /// <summary>
    /// 关卡主题组序列，按配置顺序执行。
    /// </summary>
    public int[] ThemeGroupIds { get; set; }

    /// <summary>
    /// 是否循环主题段。
    /// </summary>
    public bool LoopThemeSegments { get; set; }

    public override int Id
    {
        get { return ConfigId; }
    }

    public override bool ParseDataRow(string dataRowString, object userData)
    {
        if (string.IsNullOrWhiteSpace(dataRowString))
        {
            Log.Warning("关卡主题映射解析失败：空数据行。");
            return false;
        }

        string[] columns = dataRowString.Split(new[] { '\t' }, StringSplitOptions.None);
        if (columns.Length != ColumnCount)
        {
            Log.Warning("关卡主题映射解析失败：列数错误，Expected={0}，Actual={1}，Raw={2}。", ColumnCount, columns.Length, dataRowString);
            return false;
        }

        int id;
        if (!int.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
        {
            Log.Warning("关卡主题映射解析失败：Id 非法，Raw={0}。", dataRowString);
            return false;
        }

        if (id <= 0)
        {
            Log.Warning("关卡主题映射解析失败：Id 必须大于 0，Value={0}。", id);
            return false;
        }

        int[] themeGroupIds;
        if (!TryParseThemeGroupIds(columns[1], out themeGroupIds))
        {
            Log.Warning("关卡主题映射解析失败：ThemeGroupIds 非法，Id={0}，Value={1}。", id, columns[1]);
            return false;
        }

        int loopThemeSegmentsRaw;
        if (!int.TryParse(columns[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out loopThemeSegmentsRaw))
        {
            Log.Warning("关卡主题映射解析失败：LoopThemeSegments 非法，Id={0}，Value={1}。", id, columns[2]);
            return false;
        }

        if (loopThemeSegmentsRaw != 0 && loopThemeSegmentsRaw != 1)
        {
            Log.Warning("关卡主题映射解析失败：LoopThemeSegments 仅支持 0/1，Id={0}，Value={1}。", id, loopThemeSegmentsRaw);
            return false;
        }

        ConfigId = id;
        ThemeGroupIds = themeGroupIds;
        LoopThemeSegments = loopThemeSegmentsRaw == 1;
        return true;
    }

    public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
    {
        return ParseDataRow(Utility.Converter.GetString(dataRowBytes, startIndex, length), userData);
    }

    private static bool TryParseThemeGroupIds(string value, out int[] themeGroupIds)
    {
        themeGroupIds = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] source = value.Split(new[] { '|' }, StringSplitOptions.None);
        if (source == null || source.Length == 0)
        {
            return false;
        }

        List<int> parsed = new List<int>(source.Length);
        for (int i = 0; i < source.Length; i++)
        {
            string item = source[i].Trim();
            if (string.IsNullOrWhiteSpace(item))
            {
                return false;
            }

            int themeGroupId;
            if (!int.TryParse(item, NumberStyles.Integer, CultureInfo.InvariantCulture, out themeGroupId))
            {
                return false;
            }

            if (themeGroupId <= 0)
            {
                return false;
            }

            parsed.Add(themeGroupId);
        }

        if (parsed.Count <= 0)
        {
            return false;
        }

        themeGroupIds = parsed.ToArray();
        return true;
    }
}
