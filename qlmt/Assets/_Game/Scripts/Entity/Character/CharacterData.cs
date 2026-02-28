//------------------------------------------------------------
// 角色显示数据
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    /// <summary>
    /// 角色显示数据
    /// 用于传递给 CharacterEntity.OnShow() 方法
    /// </summary>
    public class CharacterData
    {
        /// <summary>
        /// 角色配置ID（对应 DRCharacter 表的 Id）
        /// </summary>
        public int CharacterId;

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 基础属性配置ID（对应 DRCharacterBaseStats 表的 Id）
        /// </summary>
        public int BaseStatsId;

        /// <summary>
        /// 角色身份（主角/伙伴/敌人/中立）
        /// </summary>
        public CharacterIdentity Identity;

        /// <summary>
        /// 异能ID列表（支持多个异能）
        /// </summary>
        public List<int> AbilityIds;

        /// <summary>
        /// 性格ID列表（支持多个性格）
        /// </summary>
        public List<int> PersonalityIds;

        /// <summary>
        /// 初始位置
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// 初始旋转
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CharacterData()
        {
            CharacterId = 0;
            Name = string.Empty;
            BaseStatsId = 0;
            Identity = CharacterIdentity.Neutral;
            AbilityIds = new List<int>();
            PersonalityIds = new List<int>();
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
        }
    }
}
