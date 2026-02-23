using UnityGameFramework.Runtime;

/// <summary>
/// 关卡配置行。
/// </summary>
public class DRLevel : DataRowBase
{
    /// <summary>
    /// 关卡 Id。
    /// </summary>
    private int _id;
    /// <summary>
    /// 关卡主题 Id。
    /// </summary>
    private int _themeId;
    /// <summary>
    /// 是否循环主题。
    /// </summary>
    private bool _isLoopTheme;
    /// <summary>
    /// 是否启用噪点侧边物资点。
    /// </summary>
    private bool _useNoiseSidePoints;
    /// <summary>
    /// 背景道路生成配置 Id。
    /// </summary>
    private int _bgRoadGenProfileId;

    /// <summary>
    /// 行 Id。
    /// </summary>
    public override int Id => _id;
    /// <summary>
    /// 主题 Id。
    /// </summary>
    public int ThemeId => _themeId;
    /// <summary>
    /// 是否循环主题。
    /// </summary>
    public bool IsLoopTheme => _isLoopTheme;
    /// <summary>
    /// 是否启用噪点侧边物资点。
    /// </summary>
    public bool UseNoiseSidePoints => _useNoiseSidePoints;
    /// <summary>
    /// 背景道路生成配置 Id。
    /// </summary>
    public int BgRoadGenProfileId => _bgRoadGenProfileId;

    /// <summary>
    /// 解析文本行。
    /// </summary>
    public override bool ParseDataRow(string dataRowString, object userData)
    {
        if (string.IsNullOrEmpty(dataRowString))
        {
            Log.Warning("DRLevel 解析失败，数据行为空。");
            return false;
        }

        string[] columns = dataRowString.Split('\t');
        if (columns.Length < 3)
        {
            Log.Warning("DRLevel 解析失败，列数量不足：{0}", dataRowString);
            return false;
        }

        if (!int.TryParse(columns[0].Trim(), out _id))
        {
            Log.Warning("DRLevel 解析失败，Id 非法：{0}", dataRowString);
            return false;
        }

        if (!int.TryParse(columns[1].Trim(), out _themeId))
        {
            Log.Warning("DRLevel 解析失败，主题 Id 非法：{0}", dataRowString);
            return false;
        }

        if (!int.TryParse(columns[2].Trim(), out int isLoopThemeValue))
        {
            Log.Warning("DRLevel 解析失败，是否循环字段非法：{0}", dataRowString);
            return false;
        }

        _isLoopTheme = isLoopThemeValue != 0;

        // 兼容旧表：未提供扩展列时，默认关闭噪点侧边物资点。
        _useNoiseSidePoints = false;
        _bgRoadGenProfileId = 0;

        if (columns.Length >= 4)
        {
            if (!int.TryParse(columns[3].Trim(), out int useNoiseSidePointsValue))
            {
                Log.Warning("DRLevel 解析失败，噪点侧边物资开关非法：{0}", dataRowString);
                return false;
            }

            _useNoiseSidePoints = useNoiseSidePointsValue != 0;
        }

        if (columns.Length >= 5)
        {
            if (!int.TryParse(columns[4].Trim(), out _bgRoadGenProfileId))
            {
                Log.Warning("DRLevel 解析失败，背景道路生成配置 Id 非法：{0}", dataRowString);
                return false;
            }
        }

        if (_useNoiseSidePoints && _bgRoadGenProfileId <= 0)
        {
            Log.Warning("DRLevel 解析失败，启用噪点侧边物资时配置 Id 必须大于 0：{0}", dataRowString);
            return false;
        }

        return true;
    }
}
