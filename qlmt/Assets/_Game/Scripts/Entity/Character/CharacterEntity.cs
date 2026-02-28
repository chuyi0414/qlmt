//------------------------------------------------------------
// 角色实体
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;
using Game.Character;
using Game.DataTable;

namespace Game.Entity
{
    /// <summary>
    /// 角色实体
    /// 所有角色（主角/伙伴/敌人/中立）都使用这个类
    /// </summary>
    public class CharacterEntity : EntityLogic
    {
        //角色属性组件
        private CharacterAttributes m_Attributes;

        // 角色数据
        private CharacterData m_CharacterData;
        private DRCharacter m_CharacterConfig;
        private DRCharacterBaseStats m_BaseStatsConfig;

        // 角色身份
        private CharacterIdentity m_Identity;

        // 当前拥有的异能和性格ID
        private System.Collections.Generic.List<int> m_CurrentAbilityIds = new System.Collections.Generic.List<int>();
        private System.Collections.Generic.List<int> m_CurrentPersonalityIds = new System.Collections.Generic.List<int>();

        /// <summary>
        /// 获取角色身份
        /// </summary>
        public CharacterIdentity Identity => m_Identity;

        /// <summary>
        /// 获取角色名称
        /// </summary>
        public string CharacterName => m_CharacterData?.Name ?? "Unknown";

        /// <summary>
        /// 获取属性管理器
        /// </summary>
        public CharacterAttributes Attributes => m_Attributes;

        /// <summary>
        /// 实体初始化
        /// </summary>
        protected  override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 获取或添加属性组件
            if (m_Attributes == null)
            {
                m_Attributes = GetComponent<CharacterAttributes>();
                if (m_Attributes == null)
                {
                    m_Attributes = gameObject.AddComponent<CharacterAttributes>();
                }
            }
        }

        /// <summary>
        /// 实体显示
        /// </summary>
        protected  override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_CharacterData = userData as CharacterData;
            if (m_CharacterData == null)
            {
                Log.Error("CharacterData is invalid.");
                return;
            }

            // 设置名称
            Name = m_CharacterData.Name;

            // 设置身份
            m_Identity = m_CharacterData.Identity;

            // 设置位置和旋转
            CachedTransform.position = m_CharacterData.Position;
            CachedTransform.rotation = m_CharacterData.Rotation;

            // 加载配置
            LoadConfigs();

            // 初始化属性
            InitializeAttributes();

            // 应用性格
            ApplyPersonalities();

            // 添加异能
            AddAbilities();

            // 初始化行为
            InitializeBehavior();

            Log.Info($"角色显示: {CharacterName} ({m_Identity})");
        }

        /// <summary>
        /// 实体隐藏
        /// </summary>
        protected  override void OnHide(bool isShutdown, object userData)
        {
            base.OnHide(isShutdown, userData);

            // 清理数据
            m_CharacterData = null;
            m_CharacterConfig = null;
            m_BaseStatsConfig = null;
            m_CurrentAbilityIds.Clear();
            m_CurrentPersonalityIds.Clear();
        }

        /// <summary>
        /// 实体轮询
        /// </summary>
        protected  override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            // TODO: 更新逻辑（移动、战斗等）
        }

        /// <summary>
        /// 加载配置数据
        /// </summary>
        private void LoadConfigs()
        {
            // 加载角色配置（可选，如果需要从配置表读取）
            if (m_CharacterData.CharacterId > 0)
            {
                m_CharacterConfig = GameEntry.DataTable.GetDataTable<DRCharacter>()?.GetDataRow(m_CharacterData.CharacterId);
            }

            // 加载基础属性配置
            if (m_CharacterData.BaseStatsId > 0)
            {
                m_BaseStatsConfig = GameEntry.DataTable.GetDataTable<DRCharacterBaseStats>()?.GetDataRow(m_CharacterData.BaseStatsId);
                if (m_BaseStatsConfig == null)
                {
                    Log.Error($"找不到基础属性配置: {m_CharacterData.BaseStatsId}");
                }
            }
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitializeAttributes()
        {
            if (m_BaseStatsConfig == null)
            {
                Log.Warning("基础属性配置为空，使用默认值");
                m_Attributes.Initialize(100f, 10f, 5f);
                return;
            }

            // 从配置读取基准值
            m_Attributes.Initialize(
                m_BaseStatsConfig.BaseHealth,
                m_BaseStatsConfig.BaseAttack,
                m_BaseStatsConfig.BaseMoveSpeed
            );

            Log.Info($"属性初始化: 生命={m_Attributes.Health}, 攻击={m_Attributes.Attack}, 速度={m_Attributes.MoveSpeed}");
        }

        /// <summary>
        /// 应用所有性格
        /// </summary>
        private void ApplyPersonalities()
        {
            if (m_CharacterData.PersonalityIds == null || m_CharacterData.PersonalityIds.Count == 0)
            {
                return;
            }

            foreach (int personalityId in m_CharacterData.PersonalityIds)
            {
                ApplyPersonality(personalityId);
            }
        }

        /// <summary>
        /// 应用单个性格
        /// </summary>
        private void ApplyPersonality(int personalityId)
        {
            if (personalityId <= 0)
            {
                return;
            }

            // TODO: 从数据表读取性格配置并应用
            // DRPersonality personalityConfig = GameEntry.DataTable.GetDataTable<DRPersonality>()?.GetDataRow(personalityId);
            // 应用属性修饰器

            m_CurrentPersonalityIds.Add(personalityId);
            Log.Info($"应用性格: {personalityId}");
        }

        /// <summary>
        /// 添加所有异能
        /// </summary>
        private void AddAbilities()
        {
            if (m_CharacterData.AbilityIds == null || m_CharacterData.AbilityIds.Count == 0)
            {
                return;
            }

            foreach (int abilityId in m_CharacterData.AbilityIds)
            {
                AddAbility(abilityId);
            }
        }

        /// <summary>
        /// 添加单个异能
        /// </summary>
        private void AddAbility(int abilityId)
        {
            if (abilityId <= 0)
            {
                return;
            }

            // TODO: 从数据表读取异能配置并添加
            // DRAbility abilityConfig = GameEntry.DataTable.GetDataTable<DRAbility>()?.GetDataRow(abilityId);
            // 创建异能组件

            m_CurrentAbilityIds.Add(abilityId);
            Log.Info($"添加异能: {abilityId}");
        }

        /// <summary>
        /// 初始化行为（根据身份）
        /// </summary>
        private void InitializeBehavior()
        {
            switch (m_Identity)
            {
                case CharacterIdentity.Player:
                    // 主角：接受玩家输入
                    EnablePlayerControl();
                    break;

                case CharacterIdentity.Ally:
                    // 伙伴：跟随主角的 AI
                    EnableAllyAI();
                    break;

                case CharacterIdentity.Enemy:
                    // 敌人：攻击主角的 AI
                    EnableEnemyAI();
                    break;

                case CharacterIdentity.Neutral:
                    // 中立：不主动攻击
                    EnableNeutralAI();
                    break;
            }
        }

        /// <summary>
        /// 启用玩家控制
        /// </summary>
        private void EnablePlayerControl()
        {
            // TODO: 添加玩家输入组件
            Log.Info($"{CharacterName} 启用玩家控制");
        }

        /// <summary>
        /// 启用伙伴AI
        /// </summary>
        private void EnableAllyAI()
        {
            // TODO: 添加伙伴AI组件
            Log.Info($"{CharacterName} 启用伙伴AI");
        }

        /// <summary>
        /// 启用敌人AI
        /// </summary>
        private void EnableEnemyAI()
        {
            // TODO: 添加敌人AI组件
            Log.Info($"{CharacterName} 启用敌人AI");
        }

        /// <summary>
        /// 启用中立AI
        /// </summary>
        private void EnableNeutralAI()
        {
            // TODO: 添加中立AI组件
            Log.Info($"{CharacterName} 启用中立AI");
        }

        /// <summary>
        /// 改变身份（敌人变伙伴、中立变敌人等）
        /// </summary>
        public void ChangeIdentity(CharacterIdentity newIdentity)
        {
            if (m_Identity == newIdentity)
            {
                return;
            }

            CharacterIdentity oldIdentity = m_Identity;
            m_Identity = newIdentity;

            // 重新初始化行为
            InitializeBehavior();

            // 切换阵营层级（用于碰撞检测）
            gameObject.layer = GetLayerByIdentity(newIdentity);

            Log.Info($"{CharacterName} 身份改变: {oldIdentity} → {newIdentity}");

            // TODO: 触发身份改变事件
            // GameEntry.Event.Fire(this, IdentityChangedEventArgs.Create(this, oldIdentity, newIdentity));
        }

        /// <summary>
        /// 根据身份获取层级
        /// </summary>
        private int GetLayerByIdentity(CharacterIdentity identity)
        {
            switch (identity)
            {
                case CharacterIdentity.Player:
                case CharacterIdentity.Ally:
                    return LayerMask.NameToLayer("Ally");

                case CharacterIdentity.Enemy:
                    return LayerMask.NameToLayer("Enemy");

                case CharacterIdentity.Neutral:
                    return LayerMask.NameToLayer("Neutral");

                default:
                    return 0;
            }
        }

        /// <summary>
        /// 判断是否是主角
        /// </summary>
        public bool IsPlayer()
        {
            return m_Identity == CharacterIdentity.Player;
        }

        /// <summary>
        /// 判断是否是敌人
        /// </summary>
        public bool IsEnemy()
        {
            return m_Identity == CharacterIdentity.Enemy;
        }

        /// <summary>
        /// 判断是否是伙伴
        /// </summary>
        public bool IsAlly()
        {
            return m_Identity == CharacterIdentity.Ally;
        }
    }
}
