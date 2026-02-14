using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 角色管理器组件。
/// </summary>
public sealed partial class CharacterManagerComponent : GameFrameworkComponent
{
    /// <summary>
    /// 角色运行时数据。
    /// </summary>
    private struct CharacterRuntime
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
        /// 角色实体逻辑。
        /// </summary>
        public CharacterEntity Entity;
    }

    /// <summary>
    /// 角色异步创建请求数据。
    /// </summary>
    private struct CharacterSpawnRequest
    {
        /// <summary>
        /// 角色配置 Id。
        /// </summary>
        public int CharacterConfigId;
    }

    /// <summary>
    /// 角色实体组名称。
    /// </summary>
    [SerializeField] private string _characterEntityGroupName = "Character";

    /// <summary>
    /// 角色运行时映射（EntityId -> Runtime）。
    /// </summary>
    private readonly Dictionary<int, CharacterRuntime> _characterRuntimes = new Dictionary<int, CharacterRuntime>();
    /// <summary>
    /// 待完成创建请求（EntityId -> Request）。
    /// </summary>
    private readonly Dictionary<int, CharacterSpawnRequest> _pendingRequests = new Dictionary<int, CharacterSpawnRequest>();
    /// <summary>
    /// 已取消但可能仍会回调的请求 Id。
    /// </summary>
    private readonly HashSet<int> _abortedRequestIds = new HashSet<int>();

    /// <summary>
    /// 是否已订阅实体显示回调。
    /// </summary>
    private bool _isSubscribedEntityEvents;
}
