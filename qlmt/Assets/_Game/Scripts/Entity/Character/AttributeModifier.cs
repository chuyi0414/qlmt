//------------------------------------------------------------
// 属性修饰器
//------------------------------------------------------------

namespace Game.Character
{
    /// <summary>
    /// 属性修饰器（表示对某个属性的修改）
    /// 来源可以是：装备、性格、异能、Buff等
    /// </summary>
    public class AttributeModifier
    {
        /// <summary>
        /// 修饰器类型（固定加成/百分比加成/乘算）
        /// </summary>
        public ModifierType ModifierType { get; private set; }

        /// <summary>
        /// 修饰值
        /// FlatAdd: 直接数值（例如 5 表示 +5）
        /// PercentAdd: 百分比（例如 0.2 表示 +20%）
        /// PercentMultiply: 乘数（例如 1.5 表示 ×1.5）
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        /// 来源对象（用于批量移除）
        /// 例如：装备对象、性格对象、异能对象
        /// </summary>
        public object Source { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="modType">修饰器类型</param>
        /// <param name="value">修饰值</param>
        /// <param name="source">来源对象</param>
        public AttributeModifier(ModifierType modType, float value, object source)
        {
            ModifierType = modType;
            Value = value;
            Source = source;
        }
    }
}
