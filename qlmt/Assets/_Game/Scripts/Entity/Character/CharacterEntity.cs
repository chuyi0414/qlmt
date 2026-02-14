using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 角色实体逻辑。
/// </summary>
public class CharacterEntity : EntityLogic
{
    /// <summary>
    /// 角色显示参数。
    /// </summary>
    public struct CharacterShowData
    {
        /// <summary>
        /// 出生世界坐标。
        /// </summary>
        public Vector3 SpawnPosition;
        /// <summary>
        /// 阵营类型。
        /// </summary>
        public CharacterCampType CampType;
        /// <summary>
        /// 最大生命值。
        /// </summary>
        public float MaxHp;
        /// <summary>
        /// 当前生命值。
        /// </summary>
        public float CurrentHp;
        /// <summary>
        /// 攻击力。
        /// </summary>
        public float Attack;
        /// <summary>
        /// 移动速度。
        /// </summary>
        public float MoveSpeed;
    }

    /// <summary>
    /// 阵营类型。
    /// </summary>
    private CharacterCampType _campType;
    /// <summary>
    /// 最大生命值。
    /// </summary>
    private float _maxHp;
    /// <summary>
    /// 当前生命值。
    /// </summary>
    private float _currentHp;
    /// <summary>
    /// 攻击力。
    /// </summary>
    private float _attack;
    /// <summary>
    /// 移动速度。
    /// </summary>
    private float _moveSpeed;

    /// <summary>
    /// 阵营类型。
    /// </summary>
    public CharacterCampType CampType => _campType;
    /// <summary>
    /// 最大生命值。
    /// </summary>
    public float MaxHp => _maxHp;
    /// <summary>
    /// 当前生命值。
    /// </summary>
    public float CurrentHp => _currentHp;
    /// <summary>
    /// 攻击力。
    /// </summary>
    public float Attack => _attack;
    /// <summary>
    /// 移动速度。
    /// </summary>
    public float MoveSpeed => _moveSpeed;

    /// <summary>
    /// 显示回调。
    /// </summary>
    protected override void OnShow(object userData)
    {
        if (userData is CharacterShowData showData)
        {
            // 在实体可见前完成定位，避免先闪到 (0,0,0)。
            CachedTransform.position = showData.SpawnPosition;
            _campType = showData.CampType;
            _maxHp = showData.MaxHp;
            _currentHp = showData.CurrentHp;
            _attack = showData.Attack;
            _moveSpeed = showData.MoveSpeed;
        }
        else
        {
            Log.Warning("CharacterEntity 显示参数非法：{0}", Name);
            ResetStats();
        }

        base.OnShow(userData);
    }

    /// <summary>
    /// 隐藏回调。
    /// </summary>
    protected override void OnHide(bool isShutdown, object userData)
    {
        base.OnHide(isShutdown, userData);
        ResetStats();
    }

    /// <summary>
    /// 重置基础属性。
    /// </summary>
    private void ResetStats()
    {
        _campType = CharacterCampType.Neutral;
        _maxHp = 0f;
        _currentHp = 0f;
        _attack = 0f;
        _moveSpeed = 0f;
    }
}
