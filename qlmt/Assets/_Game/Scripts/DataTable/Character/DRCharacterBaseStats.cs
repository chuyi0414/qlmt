//------------------------------------------------------------
// 角色基础属性数据表
//------------------------------------------------------------

using Game.Character;
using UnityGameFramework.Runtime;

namespace Game.DataTable
{
    /// <summary>
    /// 角色基础属性数据行（种族/性别基准值）
    /// </summary>
    public class DRCharacterBaseStats : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取配置编号
        /// </summary>
        public override int Id => m_Id;

        /// <summary>
        /// 种族ID（1=人类，2=变异者，3=机械体）
        /// </summary>
        public int RaceId { get; private set; }

        /// <summary>
        /// 种族名称（显示用）
        /// </summary>
        public string RaceName { get; private set; }

        /// <summary>
        /// 性别ID（1=男，2=女）
        /// </summary>
        public int GenderId { get; private set; }

        /// <summary>
        /// 性别名称（显示用）
        /// </summary>
        public string GenderName { get; private set; }

        /// <summary>
        /// 基础生命值
        /// </summary>
        public float BaseHealth { get; private set; }

        /// <summary>
        /// 基础攻击力
        /// </summary>
        public float BaseAttack { get; private set; }

        /// <summary>
        /// 基础移动速度
        /// </summary>
        public float BaseMoveSpeed { get; private set; }

        /// <summary>
        /// 解析数据行（从文本文件读取）
        /// </summary>
        /// <param name="dataRowString">数据行字符串（Tab分隔）</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>是否解析成功</returns>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            // 按 Tab 分隔字符串
            string[] columnStrings = dataRowString.Split('\t');
            
            // 检查列数是否正确（至少8列）
            if (columnStrings.Length < 8)
            {
                return false;
            }

            int index = 0;
            
            // 按顺序解析每一列
            m_Id = int.Parse(columnStrings[index++]);           // 第1列：配置ID
            RaceId = int.Parse(columnStrings[index++]);         // 第2列：种族ID
            RaceName = columnStrings[index++];                  // 第3列：种族名称
            GenderId = int.Parse(columnStrings[index++]);       // 第4列：性别ID
            GenderName = columnStrings[index++];                // 第5列：性别名称
            BaseHealth = float.Parse(columnStrings[index++]);   // 第6列：基础生命
            BaseAttack = float.Parse(columnStrings[index++]);   // 第7列：基础攻击
            BaseMoveSpeed = float.Parse(columnStrings[index++]);// 第8列：基础速度

            return true;
        }

        /// <summary>
        /// 获取种族类型枚举
        /// </summary>
        public RaceType GetRaceType()
        {
            return (RaceType)RaceId;
        }

        /// <summary>
        /// 获取性别类型枚举
        /// </summary>
        public GenderType GetGenderType()
        {
            return (GenderType)GenderId;
        }
    }
}
