using System.Collections.Generic;
using UnityGameFramework.Runtime;

/// <summary>
/// 角色名册管理器：维护角色实体缓存与主角引用。
/// </summary>
public sealed class CharacterRosterManager : GameFrameworkComponent
{
    /// <summary>
    /// 无效角色 Id。
    /// </summary>
    private const int InvalidCharacterId = -1;

    /// <summary>
    /// 当前主角 Id。
    /// </summary>
    public int ProtagonistId { get; private set; } = InvalidCharacterId;

    /// <summary>
    /// 全部角色缓存。
    /// Key=角色 Id，Value=角色实体。
    /// </summary>
    private readonly Dictionary<int, CharacterBaseEntity> _characterByIds = new Dictionary<int, CharacterBaseEntity>();

    /// <summary>
    /// 设置主角并写入名册（已存在则覆盖）。
    /// </summary>
    /// <param name="id">角色 Id。</param>
    /// <param name="characterEntity">角色实体。</param>
    /// <returns>设置是否成功。</returns>
    public bool SetProtagonist(int id, CharacterBaseEntity characterEntity)
    {
        if (!IsValidCharacter(id, characterEntity))
        {
            Log.Warning("设置主角失败：参数非法，Id={0}。", id);
            return false;
        }

        ProtagonistId = id;
        _characterByIds[id] = characterEntity;
        return true;
    }

    /// <summary>
    /// 移除主角（同时从名册中移除该角色）。
    /// </summary>
    /// <returns>是否成功移除。</returns>
    public bool RemoveProtagonist()
    {
        if (ProtagonistId <= 0)
        {
            return false;
        }

        bool removed = _characterByIds.Remove(ProtagonistId);
        ProtagonistId = InvalidCharacterId;
        return removed;
    }

    /// <summary>
    /// 新增角色到名册（已存在则覆盖）。
    /// </summary>
    /// <param name="id">角色 Id。</param>
    /// <param name="characterEntity">角色实体。</param>
    /// <returns>新增或更新是否成功。</returns>
    public bool AddCharacter(int id, CharacterBaseEntity characterEntity)
    {
        if (!IsValidCharacter(id, characterEntity))
        {
            Log.Warning("新增角色失败：参数非法，Id={0}。", id);
            return false;
        }

        _characterByIds[id] = characterEntity;
        return true;
    }

    /// <summary>
    /// 尝试按 Id 获取角色实体。
    /// </summary>
    /// <param name="id">角色 Id。</param>
    /// <param name="characterEntity">输出角色实体。</param>
    /// <returns>是否找到。</returns>
    public bool TryGetCharacter(int id, out CharacterBaseEntity characterEntity)
    {
        return _characterByIds.TryGetValue(id, out characterEntity);
    }

    /// <summary>
    /// 从名册中移除指定角色。
    /// </summary>
    /// <param name="id">角色 Id。</param>
    /// <returns>是否移除成功。</returns>
    public bool RemoveCharacter(int id)
    {
        if (id <= 0)
        {
            return false;
        }

        bool removed = _characterByIds.Remove(id);
        if (ProtagonistId == id)
        {
            ProtagonistId = InvalidCharacterId;
        }

        return removed;
    }

    /// <summary>
    /// 清空名册并重置主角。
    /// </summary>
    public void ClearAll()
    {
        _characterByIds.Clear();
        ProtagonistId = InvalidCharacterId;
    }

    /// <summary>
    /// 当前名册中的角色数量。
    /// </summary>
    public int Count
    {
        get { return _characterByIds.Count; }
    }

    /// <summary>
    /// 校验角色参数合法性。
    /// </summary>
    /// <param name="id">角色 Id。</param>
    /// <param name="characterEntity">角色实体。</param>
    /// <returns>合法返回 true。</returns>
    private static bool IsValidCharacter(int id, CharacterBaseEntity characterEntity)
    {
        return id > 0 && characterEntity != null;
    }
}
