using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 战斗组件
/// </summary>
public partial class CombatManager : GameFrameworkComponent
{
    /// <summary>
    /// 启动关卡（对外统一入口）。
    /// </summary>
    /// <param name="levelId">关卡 Id。</param>
    public void StartLevel(int levelId)
    {
        PrepareLevelBackground(levelId);
    }
}
