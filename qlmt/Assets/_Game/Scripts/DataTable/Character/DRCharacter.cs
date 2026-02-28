//------------------------------------------------------------
// 角色配置数据表
//------------------------------------------------------------

using Game.Character;
using System.Collections.Generic;
using UnityGameFramework.Runtime;

namespace Game.DataTable
{
    /// <summary>
    /// 角色配置数据行
    /// </summary>
    public class DRCharacter : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取角色编号
        /// </summary>
        public override int Id => m_Id;

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 基础属性配置ID（关联 DRCharacterBaseStats）
        /// </summary>
        public int BaseStatsId { get; private set; }

        /// <summary>
        /// 初始身份字符串（Player/Ally/Enemy/Neutral）
        /// </summary>
        public string IdentityStr { get; private set; }

        /// <summary>
        /// 异能ID列表（用分号分隔，例如：1;2;3）
        /// </summary>
        public List<int> AbilityIds { get; private set; }

        /// <summary>
        /// 性格ID列表（用分号分隔，例如：1;2）
        /// </summary>
        public List<int> PersonalityIds { get; private set; }

        /// <summary>
        /// 预制体资源名
        /// </summary>
        public string AssetName { get; private set; }

        /// <summary>
        /// 获取身份枚举
        /// </summary>
        public CharacterIdentity GetIdentity()
        {
            if (System.Enum.TryParse(IdentityStr, out CharacterIdentity identity))
            {
                return identity;
            }
            return CharacterIdentity.Neutral;
        }

        /// <summary>
        /// 解析数据行
        /// </summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split('\t');

            if (columnStrings.Length < 7)
            {
                return false;
            }

            int index = 0;
            m_Id = int.Parse(columnStrings[index++]);
            Name = columnStrings[index++];
            BaseStatsId = int.Parse(columnStrings[index++]);
            IdentityStr = columnStrings[index++];
            AbilityIds = ParseIntList(columnStrings[index++]);
            PersonalityIds = ParseIntList(columnStrings[index++]);
            AssetName = columnStrings[index++];

            return true;
        }

        /// <summary>
        /// 解析整数列表（用分号分隔）
        /// 例如："1;2;3" → [1, 2, 3]
        /// "0" 或空字符串 → []
        /// </summary>
        private List<int> ParseIntList(string str)
        {
            List<int> result = new List<int>();

            if (string.IsNullOrEmpty(str) || str == "0")
            {
                return result;
            }

            string[] parts = str.Split(';');
            foreach (string part in parts)
            {
                if (int.TryParse(part.Trim(), out int id) && id > 0)
                {
                    result.Add(id);
                }
            }

            return result;
        }
    }
}
