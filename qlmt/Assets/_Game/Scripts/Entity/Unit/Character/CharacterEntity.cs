using GameFramework.DataTable;
using UnityGameFramework.Runtime;

/// <summary>
/// 角色实体逻辑。
/// 通过 ShowEntity 传入的 userData（严格 int）确定 UnitId，并据此加载角色基础属性。
/// </summary>
public class CharacterEntity : UnitBaseEntity
{
    /// <summary>
    /// Unit 配置表名称。
    /// </summary>
    private const string UnitDataTableName = "Entity/UnitConfig";

    /// <summary>
    /// 最大血量。
    /// </summary>
    public float MaxHealth { get; private set; }

    /// <summary>
    /// 当前血量。
    /// </summary>
    public float CurrentHealth { get; private set; }

    /// <summary>
    /// 攻击力。
    /// </summary>
    public float Attack { get; private set; }

    /// <summary>
    /// 移动速度。
    /// </summary>
    public float Speed { get; private set; }

    /// <summary>
    /// 实体显示时读取角色配置。
    /// </summary>
    /// <param name="userData">单位配置 Id，仅接受 int 且必须大于 0。</param>
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        ResetStats();

        if (!(userData is int unitId) || unitId <= 0)
        {
            Log.Error("角色实体初始化失败：userData 必须为大于 0 的 int，Actual={0}。", userData ?? "null");
            return;
        }

        if (GameEntry.DataTable == null)
        {
            Log.Error("角色实体初始化失败：DataTable 组件不可用。");
            return;
        }

        IDataTable<DRUnitConfig> dataTable = GameEntry.DataTable.GetDataTable<DRUnitConfig>(UnitDataTableName);
        if (dataTable == null)
        {
            Log.Error("角色实体初始化失败：未找到数据表 {0}。", UnitDataTableName);
            return;
        }

        DRUnitConfig row = dataTable.GetDataRow(unitId);
        if (row == null)
        {
            Log.Error("角色实体初始化失败：未找到 Unit 配置行，UnitId={0}。", unitId);
            return;
        }

        ApplyStats(row);
    }

    /// <summary>
    /// 重置属性到安全默认值。
    /// </summary>
    private void ResetStats()
    {
        MaxHealth = 0f;
        CurrentHealth = 0f;
        Attack = 0f;
        Speed = 0f;
    }

    /// <summary>
    /// 应用配置行到实体属性。
    /// </summary>
    /// <param name="row">单位配置数据行。</param>
    private void ApplyStats(DRUnitConfig row)
    {
        MaxHealth = row.Health;
        CurrentHealth = row.Health;
        Attack = row.Attack;
        Speed = row.Speed;
    }
}
