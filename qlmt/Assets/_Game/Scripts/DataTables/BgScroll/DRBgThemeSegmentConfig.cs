using System;
using System.Collections.Generic;
using System.Globalization;
using GameFramework;
using UnityGameFramework.Runtime;

/// <summary>
/// 主题段配置数据行。
/// </summary>
public sealed class DRBgThemeSegmentConfig : DataRowBase
{
    /// <summary>
    /// 配置表列数。
    /// </summary>
    private const int ColumnCount = 2;

    /// <summary>
    /// 主题片段定义（ThemeTag + ChunkCount）。
    /// </summary>
    public sealed class ThemeChunkSegment
    {
        public ThemeChunkSegment(string themeTag, int chunkCount)
        {
            ThemeTag = themeTag;
            ChunkCount = chunkCount;
        }

        public string ThemeTag { get; set; }

        public int ChunkCount { get; set; }
    }

        /// <summary>
    /// 主键 Id 的内部存储，对应数据表的 Id 列。
    /// </summary>
    private int m_Id;
    /// <summary>
    /// 数据行唯一 Id。
    /// </summary>
    public override int Id => m_Id;

    /// <summary>
    /// 主题片段序列（按配置顺序执行）。
    /// </summary>
    public ThemeChunkSegment[] Segments { get; set; }
    public override bool ParseDataRow(string dataRowString, object userData)
    {
        if (string.IsNullOrWhiteSpace(dataRowString))
        {
            Log.Warning("主题段配置解析失败：空数据行。");
            return false;
        }

        string[] columns = dataRowString.Split(new[] { '\t' }, StringSplitOptions.None);
        if (columns.Length != ColumnCount)
        {
            Log.Warning("主题段配置解析失败：列数错误，Expected={0}，Actual={1}，Raw={2}。", ColumnCount, columns.Length, dataRowString);
            return false;
        }

        int id;
        if (!int.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
        {
            Log.Warning("主题段配置解析失败：Id 非法，Raw={0}。", dataRowString);
            return false;
        }

        if (id <= 0)
        {
            Log.Warning("主题段配置解析失败：Id 必须大于 0，Value={0}。", id);
            return false;
        }

        ThemeChunkSegment[] segments;
        string errorMessage;
        if (!TryParseSegments(columns[1], out segments, out errorMessage))
        {
            Log.Warning("主题段配置解析失败：Id={0}，Error={1}，Value={2}。", id, errorMessage, columns[1]);
            return false;
        }

        m_Id = id;
        Segments = segments;
        return true;
    }

    public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
    {
        return ParseDataRow(Utility.Converter.GetString(dataRowBytes, startIndex, length), userData);
    }

    private static bool TryParseSegments(string value, out ThemeChunkSegment[] segments, out string errorMessage)
    {
        segments = null;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "ThemeSegments 为空";
            return false;
        }

        string normalized = value.Replace('，', ',');
        string[] rawSegments = normalized.Split(new[] { ',' }, StringSplitOptions.None);
        if (rawSegments == null || rawSegments.Length == 0)
        {
            errorMessage = "ThemeSegments 为空";
            return false;
        }

        List<ThemeChunkSegment> parsedSegments = new List<ThemeChunkSegment>(rawSegments.Length);
        for (int i = 0; i < rawSegments.Length; i++)
        {
            string rawSegment = rawSegments[i].Trim();
            if (string.IsNullOrWhiteSpace(rawSegment))
            {
                errorMessage = string.Format(CultureInfo.InvariantCulture, "第 {0} 个片段为空", i + 1);
                return false;
            }

            string[] pair = rawSegment.Split(new[] { '|' }, StringSplitOptions.None);
            if (pair == null || pair.Length != 2)
            {
                errorMessage = string.Format(CultureInfo.InvariantCulture, "第 {0} 个片段格式非法，必须为 ThemeTag|ChunkCount", i + 1);
                return false;
            }

            string themeTag = pair[0].Trim();
            if (string.IsNullOrWhiteSpace(themeTag))
            {
                errorMessage = string.Format(CultureInfo.InvariantCulture, "第 {0} 个片段 ThemeTag 为空", i + 1);
                return false;
            }

            if (themeTag.IndexOf('|') >= 0 || themeTag.IndexOf(',') >= 0 || themeTag.IndexOf('，') >= 0)
            {
                errorMessage = string.Format(CultureInfo.InvariantCulture, "第 {0} 个片段 ThemeTag 含非法分隔符", i + 1);
                return false;
            }

            int chunkCount;
            if (!int.TryParse(pair[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out chunkCount))
            {
                errorMessage = string.Format(CultureInfo.InvariantCulture, "第 {0} 个片段 ChunkCount 非法", i + 1);
                return false;
            }

            if (chunkCount <= 0)
            {
                errorMessage = string.Format(CultureInfo.InvariantCulture, "第 {0} 个片段 ChunkCount 必须大于 0", i + 1);
                return false;
            }

            parsedSegments.Add(new ThemeChunkSegment(themeTag, chunkCount));
        }

        if (parsedSegments.Count <= 0)
        {
            errorMessage = "ThemeSegments 为空";
            return false;
        }

        segments = parsedSegments.ToArray();
        return true;
    }
}
