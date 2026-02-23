using UnityGameFramework.Runtime;

/// <summary>
/// 背景道路与侧边物资点生成配置行。
/// </summary>
public class DRBgRoadGenProfile : DataRowBase
{
    /// <summary>
    /// 配置 Id。
    /// </summary>
    private int _id;
    /// <summary>
    /// 固定随机种子。
    /// </summary>
    private int _seed;
    /// <summary>
    /// Perlin 频率。
    /// </summary>
    private float _perlinFrequency;
    /// <summary>
    /// 道路半宽。
    /// </summary>
    private float _roadHalfWidth;
    /// <summary>
    /// 生成区域边缘留白。
    /// </summary>
    private float _spawnEdgePaddingX;
    /// <summary>
    /// 不生成权重。
    /// </summary>
    private float _noneWeight;
    /// <summary>
    /// 小物资点权重。
    /// </summary>
    private float _smallWeight;
    /// <summary>
    /// 大物资点权重。
    /// </summary>
    private float _bigWeight;
    /// <summary>
    /// 最小纵向间距。
    /// </summary>
    private float _minSpawnGapY;
    /// <summary>
    /// 最大纵向间距。
    /// </summary>
    private float _maxSpawnGapY;
    /// <summary>
    /// 单个背景块最大点位数。
    /// </summary>
    private int _maxPointsPerBg;
    /// <summary>
    /// 同块最小点间距。
    /// </summary>
    private float _minInBlockPointDistance;
    /// <summary>
    /// 道路中心 X 偏移。
    /// </summary>
    private float _roadCenterOffsetX;
    /// <summary>
    /// 小物资点占位路径（相对 EntityRoot）。
    /// </summary>
    private string _smallMarkerPath;
    /// <summary>
    /// 大物资点占位路径（相对 EntityRoot）。
    /// </summary>
    private string _bigMarkerPath;

    /// <summary>
    /// 行 Id。
    /// </summary>
    public override int Id => _id;
    /// <summary>
    /// 固定随机种子。
    /// </summary>
    public int Seed => _seed;
    /// <summary>
    /// Perlin 频率。
    /// </summary>
    public float PerlinFrequency => _perlinFrequency;
    /// <summary>
    /// 道路半宽。
    /// </summary>
    public float RoadHalfWidth => _roadHalfWidth;
    /// <summary>
    /// 生成区域边缘留白。
    /// </summary>
    public float SpawnEdgePaddingX => _spawnEdgePaddingX;
    /// <summary>
    /// 不生成权重。
    /// </summary>
    public float NoneWeight => _noneWeight;
    /// <summary>
    /// 小物资点权重。
    /// </summary>
    public float SmallWeight => _smallWeight;
    /// <summary>
    /// 大物资点权重。
    /// </summary>
    public float BigWeight => _bigWeight;
    /// <summary>
    /// 最小纵向间距。
    /// </summary>
    public float MinSpawnGapY => _minSpawnGapY;
    /// <summary>
    /// 最大纵向间距。
    /// </summary>
    public float MaxSpawnGapY => _maxSpawnGapY;
    /// <summary>
    /// 单个背景块最大点位数。
    /// </summary>
    public int MaxPointsPerBg => _maxPointsPerBg;
    /// <summary>
    /// 同块最小点间距。
    /// </summary>
    public float MinInBlockPointDistance => _minInBlockPointDistance;
    /// <summary>
    /// 道路中心 X 偏移。
    /// </summary>
    public float RoadCenterOffsetX => _roadCenterOffsetX;
    /// <summary>
    /// 小物资点占位路径。
    /// </summary>
    public string SmallMarkerPath => _smallMarkerPath;
    /// <summary>
    /// 大物资点占位路径。
    /// </summary>
    public string BigMarkerPath => _bigMarkerPath;

    /// <summary>
    /// 解析文本行。
    /// </summary>
    public override bool ParseDataRow(string dataRowString, object userData)
    {
        if (string.IsNullOrEmpty(dataRowString))
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，数据行为空。");
            return false;
        }

        string[] columns = dataRowString.Split('\t');
        if (columns.Length < 15)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，列数量不足：{0}", dataRowString);
            return false;
        }

        if (!int.TryParse(columns[0].Trim(), out _id))
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，Id 非法：{0}", dataRowString);
            return false;
        }

        if (!int.TryParse(columns[1].Trim(), out _seed))
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，Seed 非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[2].Trim(), out _perlinFrequency) || _perlinFrequency <= 0f)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，PerlinFrequency 非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[3].Trim(), out _roadHalfWidth) || _roadHalfWidth <= 0f)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，RoadHalfWidth 非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[4].Trim(), out _spawnEdgePaddingX) || _spawnEdgePaddingX < 0f)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，SpawnEdgePaddingX 非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[5].Trim(), out _noneWeight) || _noneWeight < 0f)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，NoneWeight 非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[6].Trim(), out _smallWeight) || _smallWeight < 0f)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，SmallWeight 非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[7].Trim(), out _bigWeight) || _bigWeight < 0f)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，BigWeight 非法：{0}", dataRowString);
            return false;
        }

        if ((_noneWeight + _smallWeight + _bigWeight) <= 0f)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，权重总和必须大于 0：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[8].Trim(), out _minSpawnGapY) || _minSpawnGapY <= 0f)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，MinSpawnGapY 非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[9].Trim(), out _maxSpawnGapY) || _maxSpawnGapY < _minSpawnGapY)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，MaxSpawnGapY 非法：{0}", dataRowString);
            return false;
        }

        if (!int.TryParse(columns[10].Trim(), out _maxPointsPerBg) || _maxPointsPerBg < 0)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，MaxPointsPerBg 非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[11].Trim(), out _minInBlockPointDistance) || _minInBlockPointDistance < 0f)
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，MinInBlockPointDistance 非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[12].Trim(), out _roadCenterOffsetX))
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，RoadCenterOffsetX 非法：{0}", dataRowString);
            return false;
        }

        _smallMarkerPath = columns[13].Trim();
        _bigMarkerPath = columns[14].Trim();
        if (string.IsNullOrEmpty(_smallMarkerPath) || string.IsNullOrEmpty(_bigMarkerPath))
        {
            Log.Warning("DRBgRoadGenProfile 解析失败，占位路径为空：{0}", dataRowString);
            return false;
        }

        return true;
    }
}
