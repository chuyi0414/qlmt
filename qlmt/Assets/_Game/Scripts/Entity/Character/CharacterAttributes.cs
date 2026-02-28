//------------------------------------------------------------
// 角色属性管理器
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    /// <summary>
    /// 角色属性管理器
    /// 负责管理基准值、修饰器、计算最终属性
    /// </summary>
    public class CharacterAttributes : MonoBehaviour
    {
        /// <summary>
        /// 属性类（表示单个属性）
        /// </summary>
        private class Attribute
        {
            /// <summary>
            /// 属性名称
            /// </summary>
            public string Name;

            /// <summary>
            /// 基准值（从数据表读取）
            /// </summary>
            public float BaseValue;

            /// <summary>
            /// 修饰器列表（装备、性格、异能添加的修改）
            /// </summary>
            public List<AttributeModifier> Modifiers;

            /// <summary>
            /// 最终值（缓存）
            /// </summary>
            public float FinalValue;

            /// <summary>
            /// 是否需要重新计算
            /// </summary>
            public bool IsDirty;

            public Attribute(string name, float baseValue)
            {
                Name = name;
                BaseValue = baseValue;
                Modifiers = new List<AttributeModifier>();
                FinalValue = baseValue;
                IsDirty = true;
            }
        }

        // 属性字典（通过名称访问）
        private Dictionary<string, Attribute> m_Attributes = new Dictionary<string, Attribute>();

        // 常用属性名称常量
        public const string HEALTH = "Health";
        public const string ATTACK = "Attack";
        public const string MOVE_SPEED = "MoveSpeed";

        /// <summary>
        /// 初始化属性（从数据表读取基准值后调用）
        /// </summary>
        /// <param name="baseHealth">基础生命值</param>
        /// <param name="baseAttack">基础攻击力</param>
        /// <param name="baseMoveSpeed">基础移动速度</param>
        public void Initialize(float baseHealth, float baseAttack, float baseMoveSpeed)
        {
            m_Attributes[HEALTH] = new Attribute(HEALTH, baseHealth);
            m_Attributes[ATTACK] = new Attribute(ATTACK, baseAttack);
            m_Attributes[MOVE_SPEED] = new Attribute(MOVE_SPEED, baseMoveSpeed);
        }

        /// <summary>
        /// 添加修饰器到指定属性
        /// </summary>
        /// <param name="attributeName">属性名称（使用常量）</param>
        /// <param name="modifier">修饰器</param>
        public void AddModifier(string attributeName, AttributeModifier modifier)
        {
            if (m_Attributes.TryGetValue(attributeName, out Attribute attr))
            {
                attr.Modifiers.Add(modifier);
                attr.IsDirty = true;  // 标记需要重新计算
            }
            else
            {
                Debug.LogWarning($"[CharacterAttributes] 属性不存在: {attributeName}");
            }
        }

        /// <summary>
        /// 移除指定修饰器
        /// </summary>
        /// <param name="attributeName">属性名称</param>
        /// <param name="modifier">要移除的修饰器</param>
        public void RemoveModifier(string attributeName, AttributeModifier modifier)
        {
            if (m_Attributes.TryGetValue(attributeName, out Attribute attr))
            {
                attr.Modifiers.Remove(modifier);
                attr.IsDirty = true;
            }
        }

        /// <summary>
        /// 从所有属性移除来自特定源的修饰器
        /// 用于：装备脱下、异能失效、Buff消失等
        /// </summary>
        /// <param name="source">来源对象</param>
        public void RemoveModifiersFromSource(object source)
        {
            foreach (var attr in m_Attributes.Values)
            {
                int removedCount = attr.Modifiers.RemoveAll(m => m.Source == source);
                if (removedCount > 0)
                {
                    attr.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// 获取属性的最终值（自动重算）
        /// </summary>
        /// <param name="attributeName">属性名称</param>
        /// <returns>最终属性值</returns>
        public float GetAttributeValue(string attributeName)
        {
            if (m_Attributes.TryGetValue(attributeName, out Attribute attr))
            {
                if (attr.IsDirty)
                {
                    attr.FinalValue = CalculateAttributeValue(attr);
                    attr.IsDirty = false;
                }
                return attr.FinalValue;
            }

            Debug.LogWarning($"[CharacterAttributes] 属性不存在: {attributeName}");
            return 0f;
        }

        /// <summary>
        /// 计算单个属性的最终值
        /// 公式：(基准值 + 固定加成) × (1 + 百分比加成) × 百分比乘算
        /// </summary>
        private float CalculateAttributeValue(Attribute attr)
        {
            float flatAdd = 0f;          // 固定加成总和
            float percentAdd = 0f;       // 百分比加成总和
            float percentMultiply = 1f;  // 百分比乘算总和

            // 收集所有修饰器
            foreach (var modifier in attr.Modifiers)
            {
                switch (modifier.ModifierType)
                {
                    case ModifierType.FlatAdd:
                        flatAdd += modifier.Value;
                        break;

                    case ModifierType.PercentAdd:
                        percentAdd += modifier.Value;
                        break;

                    case ModifierType.PercentMultiply:
                        percentMultiply *= modifier.Value;
                        break;
                }
            }

            // 计算最终值
            float finalValue = (attr.BaseValue + flatAdd) * (1f + percentAdd) * percentMultiply;
            return finalValue;
        }

        // ========== 便捷访问属性 ==========

        /// <summary>
        /// 当前生命值
        /// </summary>
        public float Health => GetAttributeValue(HEALTH);

        /// <summary>
        /// 当前攻击力
        /// </summary>
        public float Attack => GetAttributeValue(ATTACK);

        /// <summary>
        /// 当前移动速度
        /// </summary>
        public float MoveSpeed => GetAttributeValue(MOVE_SPEED);
    }
}
