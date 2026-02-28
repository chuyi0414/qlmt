//角色属性类型定义
namespace Game.Character
{
    /// <summary>
    /// 种族类型
    /// </summary>
    public enum RaceType
    {
        /// <summary>
        /// 人类
        /// </summary>
        Human = 1,

        /// <summary>
        /// 变异者
        /// </summary>
        Mutant = 2,

        /// <summary>
        /// 机械体
        /// </summary>
        Cyborg = 3
    }
    /// <summary>
    /// 性别类型
    /// </summary>
    public enum GenderType
    {
        /// <summary>
        /// 男性
        /// </summary>
        Male = 1,

        /// <summary>
        /// 女性
        /// </summary>
        Female = 2
    }
    /// <summary>
    /// 修饰器类型
    /// </summary>
    public enum ModifierType
    {
        /// <summary>
        /// 固定加成（例如：攻击力 +5）
        /// </summary>
        FlatAdd = 0,

        /// <summary>
        /// 百分比加成（例如：攻击力 +20%）
        /// </summary>
        PercentAdd = 1,

        /// <summary>
        /// 百分比乘算（例如：攻击力 ×1.5）
        /// </summary>
        PercentMultiply = 2
    }
    /// <summary>
    /// 角色身份类型
    /// </summary>
    public enum CharacterIdentity
    {
        /// <summary>
        /// 主角（玩家控制）
        /// </summary>
        Player = 0,

        /// <summary>
        /// 伙伴
        /// </summary>
        Ally = 1,

        /// <summary>
        /// 中立
        /// </summary>
        Neutral = 2,

        /// <summary>
        /// 敌人
        /// </summary>
        Enemy = 3,
    }
}