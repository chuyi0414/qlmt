using System;
using UnityGameFramework.Runtime;

/// <summary>
/// 角色配置行。
/// </summary>
public class DRCharacter : DataRowBase
{
    /// <summary>
    /// 角色 Id。
    /// </summary>
    private int _id;
    /// <summary>
    /// 角色名称。
    /// </summary>
    private string _name;
    /// <summary>
    /// 阵营类型。
    /// </summary>
    private CharacterCampType _campType;
    /// <summary>
    /// 最大生命值。
    /// </summary>
    private float _hp;
    /// <summary>
    /// 攻击力。
    /// </summary>
    private float _attack;
    /// <summary>
    /// 移动速度。
    /// </summary>
    private float _moveSpeed;
    /// <summary>
    /// 实体路径（相对 EntityRoot）。
    /// </summary>
    private string _entityPath;

    /// <summary>
    /// 行 Id。
    /// </summary>
    public override int Id => _id;
    /// <summary>
    /// 角色名称。
    /// </summary>
    public string Name => _name;
    /// <summary>
    /// 阵营类型。
    /// </summary>
    public CharacterCampType CampType => _campType;
    /// <summary>
    /// 最大生命值。
    /// </summary>
    public float Hp => _hp;
    /// <summary>
    /// 攻击力。
    /// </summary>
    public float Attack => _attack;
    /// <summary>
    /// 移动速度。
    /// </summary>
    public float MoveSpeed => _moveSpeed;
    /// <summary>
    /// 实体路径。
    /// </summary>
    public string EntityPath => _entityPath;

    /// <summary>
    /// 解析文本行。
    /// </summary>
    public override bool ParseDataRow(string dataRowString, object userData)
    {
        if (string.IsNullOrEmpty(dataRowString))
        {
            Log.Warning("DRCharacter 解析失败，数据行为空。");
            return false;
        }

        string[] columns = dataRowString.Split('\t');
        if (columns.Length < 7)
        {
            Log.Warning("DRCharacter 解析失败，列数量不足：{0}", dataRowString);
            return false;
        }

        if (!int.TryParse(columns[0].Trim(), out _id))
        {
            Log.Warning("DRCharacter 解析失败，Id 非法：{0}", dataRowString);
            return false;
        }

        _name = columns[1].Trim();

        if (!int.TryParse(columns[2].Trim(), out int campTypeValue) ||
            !Enum.IsDefined(typeof(CharacterCampType), campTypeValue))
        {
            Log.Warning("DRCharacter 解析失败，阵营类型非法：{0}", dataRowString);
            return false;
        }
        _campType = (CharacterCampType)campTypeValue;

        if (!float.TryParse(columns[3].Trim(), out _hp) || _hp <= 0f)
        {
            Log.Warning("DRCharacter 解析失败，生命值非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[4].Trim(), out _attack) || _attack < 0f)
        {
            Log.Warning("DRCharacter 解析失败，攻击力非法：{0}", dataRowString);
            return false;
        }

        if (!float.TryParse(columns[5].Trim(), out _moveSpeed) || _moveSpeed <= 0f)
        {
            Log.Warning("DRCharacter 解析失败，移动速度非法：{0}", dataRowString);
            return false;
        }

        _entityPath = columns[6].Trim();
        if (string.IsNullOrEmpty(_entityPath))
        {
            Log.Warning("DRCharacter 解析失败，实体路径为空：{0}", dataRowString);
            return false;
        }

        return true;
    }
}
