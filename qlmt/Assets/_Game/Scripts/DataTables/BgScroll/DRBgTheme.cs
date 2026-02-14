using System.Collections.Generic;
using UnityGameFramework.Runtime;

/// <summary>
/// 关卡主题配置行。
/// </summary>
public class DRBgTheme : DataRowBase
{
    /// <summary>
    /// 主题内背景块及数量。
    /// </summary>
    public struct BgPair
    {
        /// <summary>
        /// 背景块 Id。
        /// </summary>
        public int BgBlockId;
        /// <summary>
        /// 数量。
        /// </summary>
        public int Count;
    }

    /// <summary>
    /// 主题 Id。
    /// </summary>
    private int _id;
    /// <summary>
    /// 原始背景对字符串。
    /// </summary>
    private string _bgPairsRaw;
    /// <summary>
    /// 解析后的背景对缓存。
    /// </summary>
    private readonly List<BgPair> _bgPairs = new List<BgPair>();

    /// <summary>
    /// 行 Id。
    /// </summary>
    public override int Id => _id;
    /// <summary>
    /// 原始背景对字符串。
    /// </summary>
    public string BgPairsRaw => _bgPairsRaw;
    /// <summary>
    /// 解析后的背景对。
    /// </summary>
    public List<BgPair> BgPairs => _bgPairs;

    /// <summary>
    /// 解析文本行。
    /// </summary>
    public override bool ParseDataRow(string dataRowString, object userData)
    {
        if (string.IsNullOrEmpty(dataRowString))
        {
            Log.Warning("DRBgTheme 解析失败，数据行为空。");
            return false;
        }

        int firstTabIndex = dataRowString.IndexOf('\t');
        if (firstTabIndex <= 0 || firstTabIndex >= dataRowString.Length - 1)
        {
            Log.Warning("DRBgTheme 解析失败，格式非法：{0}", dataRowString);
            return false;
        }

        if (!int.TryParse(dataRowString.Substring(0, firstTabIndex).Trim(), out _id))
        {
            Log.Warning("DRBgTheme 解析失败，Id 非法：{0}", dataRowString);
            return false;
        }

        _bgPairsRaw = dataRowString.Substring(firstTabIndex + 1).Trim();
        _bgPairs.Clear();
        if (string.IsNullOrEmpty(_bgPairsRaw))
        {
            Log.Warning("DRBgTheme 解析失败，背景对为空：{0}", dataRowString);
            return false;
        }

        string[] pairStrings = _bgPairsRaw.Split(',');
        for (int i = 0; i < pairStrings.Length; i++)
        {
            string pairString = pairStrings[i].Trim();
            if (string.IsNullOrEmpty(pairString))
            {
                continue;
            }

            string[] pairColumns = pairString.Split('|');
            if (pairColumns.Length != 2)
            {
                Log.Warning("DRBgTheme 解析失败，背景对格式非法：{0}", pairString);
                return false;
            }

            if (!int.TryParse(pairColumns[0].Trim(), out int bgBlockId) ||
                !int.TryParse(pairColumns[1].Trim(), out int count))
            {
                Log.Warning("DRBgTheme 解析失败，背景对数值非法：{0}", pairString);
                return false;
            }

            if (count <= 0)
            {
                Log.Warning("DRBgTheme 解析失败，数量必须大于 0：{0}", pairString);
                return false;
            }

            BgPair pair = new BgPair
            {
                BgBlockId = bgBlockId,
                Count = count
            };
            _bgPairs.Add(pair);
        }

        if (_bgPairs.Count == 0)
        {
            Log.Warning("DRBgTheme 解析失败，解析后背景对为空：{0}", dataRowString);
            return false;
        }

        return true;
    }
}
