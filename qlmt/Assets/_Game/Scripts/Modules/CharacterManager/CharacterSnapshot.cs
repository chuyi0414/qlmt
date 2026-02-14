/// <summary>
/// 角色快照数据。
/// </summary>
public struct CharacterSnapshot
{
    /// <summary>
    /// 实体 Id。
    /// </summary>
    public int EntityId;
    /// <summary>
    /// 角色配置 Id。
    /// </summary>
    public int CharacterConfigId;
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
