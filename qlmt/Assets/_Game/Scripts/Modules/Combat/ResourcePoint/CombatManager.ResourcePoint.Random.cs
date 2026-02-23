using UnityEngine;

/// <summary>
/// 战斗组件（背景侧边物资点随机采样工具）。
/// </summary>
public partial class CombatManager
{
    /// <summary>
    /// 创建分段确定性随机数生成器。
    /// </summary>
    private System.Random CreateSegmentRandom(int segmentIndex)
    {
        int segmentSeed = HashSeed(_procSeed, segmentIndex, 991);
        if (segmentSeed == 0)
        {
            segmentSeed = 1;
        }

        return new System.Random(segmentSeed);
    }

    /// <summary>
    /// 获取 0~1 随机值。
    /// </summary>
    private static float NextFloat01(System.Random random)
    {
        return random != null ? (float)random.NextDouble() : 0f;
    }

    /// <summary>
    /// 组合哈希种子。
    /// </summary>
    private static int HashSeed(int a, int b, int c)
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + a;
            h = h * 31 + b;
            h = h * 31 + c;
            return h;
        }
    }

    /// <summary>
    /// 按种子与世界坐标采样 0~1 Perlin 噪点。
    /// </summary>
    private float SamplePerlin01ByWorld(float worldY, int salt)
    {
        float seedBase = Mathf.Abs(_procSeed) + 0.123f;
        float x = seedBase * 0.017f + salt * 0.131f;
        float y = worldY * _procPerlinFrequency + salt * 0.071f;
        return Mathf.Clamp01(Mathf.PerlinNoise(x, y));
    }
}
