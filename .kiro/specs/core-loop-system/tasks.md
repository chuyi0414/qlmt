# 任务列表：核心循环系统

## 实现语言

本项目使用 **C#** 作为实现语言，基于 Unity 2021.3+ 和 GameFramework 框架。

## 第一阶段：核心状态机和基础子系统

### 1. 数据模型实现

- [ ] 1.1 实现核心数据结构
  - [ ] 1.1.1 实现 GameState 类（包含循环状态、队伍、距离、历史记录）
  - [ ] 1.1.2 实现 PlayerTeam 类（包含成员列表、资源、关系矩阵）
  - [ ] 1.1.3 实现 TeamMember 类（包含属性、状态、标记）
  - [ ] 1.1.4 实现 TeamResources 类（食物、药品、弹药、其他）
  - [ ] 1.1.5 实现 CoreLoopState 枚举（6个状态）

- [ ] 1.2 实现路线节点数据结构
  - [ ] 1.2.1 实现 RouteNode 类
  - [ ] 1.2.2 实现 NodeType 枚举（Safe/Resource/Shortcut/Danger）
  - [ ] 1.2.3 实现 NodeTypeWeights 结构

- [ ] 1.3 实现事件相关数据结构
  - [ ] 1.3.1 实现 EventData 类
  - [ ] 1.3.2 实现 EventType 枚举（Scavenge/Encounter/Conflict/SurvivorInteraction）
  - [ ] 1.3.3 实现 EventCondition 类
  - [ ] 1.3.4 实现 EventOption 类

- [ ] 1.4 实现结算相关数据结构
  - [ ] 1.4.1 实现 SettlementData 类
  - [ ] 1.4.2 实现 SettlementRule 类
  - [ ] 1.4.3 实现 SettlementResult 类
  - [ ] 1.4.4 实现 ResourceChanges 类
  - [ ] 1.4.5 实现 MemberStateChange 类
  - [ ] 1.4.6 实现 RelationshipChange 类
  - [ ] 1.4.7 实现 CasualtyEvent 类
  - [ ] 1.4.8 实现 SettlementRuleType 和 CasualtyType 枚举

- [ ] 1.5 实现距离计算数据结构
  - [ ] 1.5.1 实现 DistanceCalculationData 类
  - [ ] 1.5.2 实现 DistanceCalculationResult 类
  - [ ] 1.5.3 实现 DistanceChange 类
  - [ ] 1.5.4 实现 DistanceHistoryEntry 类
  - [ ] 1.5.5 实现 PlayerAction 和 DistanceStatus 枚举

### 2. 核心状态机实现

- [ ] 2.1 实现 CoreLoopProcedure
  - [ ] 2.1.1 实现 OnEnter 方法（初始化状态机、订阅事件）
  - [ ] 2.1.2 实现 OnUpdate 方法（更新状态机）
  - [ ] 2.1.3 实现 OnLeave 方法（清理资源）
  - [ ] 2.1.4 实现 OnCoreLoopEnd 事件处理（切换到胜利/失败流程）

- [ ] 2.2 实现 CoreLoopStateMachine
  - [ ] 2.2.1 实现 Initialize 方法（创建6个状态并启动FSM）
  - [ ] 2.2.2 实现 Update 方法
  - [ ] 2.2.3 实现 Shutdown 方法
  - [ ] 2.2.4 实现 GetGameState 方法

- [ ] 2.3 实现 RouteSelectionState（路线选择状态）
  - [ ] 2.3.1 实现 OnEnter 方法（生成节点、显示UI）
  - [ ] 2.3.2 实现 OnUpdate 方法（等待玩家选择）
  - [ ] 2.3.3 实现 OnLeave 方法（记录选择）
  - [ ] 2.3.4 实现状态转换逻辑（→ NodeEventState）

- [ ] 2.4 实现 NodeEventState（节点事件状态）
  - [ ] 2.4.1 实现 OnEnter 方法（抽取事件、显示事件UI）
  - [ ] 2.4.2 实现 OnUpdate 方法（等待玩家决策）
  - [ ] 2.4.3 实现 OnLeave 方法（记录决策结果）
  - [ ] 2.4.4 实现状态转换逻辑（→ SettlementState 或 ChaseBattleState）

- [ ] 2.5 实现 SettlementState（结算状态）
  - [ ] 2.5.1 实现 OnEnter 方法（执行结算、显示结算UI）
  - [ ] 2.5.2 实现 OnUpdate 方法（等待玩家确认）
  - [ ] 2.5.3 实现 OnLeave 方法（保存结算结果）
  - [ ] 2.5.4 实现状态转换逻辑（→ DistanceCalculationState 或游戏结束）

- [ ] 2.6 实现 DistanceCalculationState（距离计算状态）
  - [ ] 2.6.1 实现 OnEnter 方法（执行距离计算、更新UI）
  - [ ] 2.6.2 实现 OnUpdate 方法
  - [ ] 2.6.3 实现 OnLeave 方法（保存距离历史）
  - [ ] 2.6.4 实现状态转换逻辑（→ ChaseCheckState）

- [ ] 2.7 实现 ChaseCheckState（追猎检查状态）
  - [ ] 2.7.1 实现 OnEnter 方法（检查距离、检查终点）
  - [ ] 2.7.2 实现 OnUpdate 方法
  - [ ] 2.7.3 实现 OnLeave 方法
  - [ ] 2.7.4 实现状态转换逻辑（→ ChaseBattleState 或 RouteSelectionState 或游戏结束）

- [ ] 2.8 实现 ChaseBattleState（追猎战状态）
  - [ ] 2.8.1 实现 OnEnter 方法（显示警告、进入战斗）
  - [ ] 2.8.2 实现 OnUpdate 方法（等待战斗结果）
  - [ ] 2.8.3 实现 OnLeave 方法（记录战斗结果）
  - [ ] 2.8.4 实现状态转换逻辑（→ SettlementState 或游戏结束）

### 3. 路线节点系统实现

- [ ] 3.1 实现 RouteNodeSystem 类
  - [ ] 3.1.1 实现构造函数（加载配置表）
  - [ ] 3.1.2 实现 GenerateRouteNodes 方法（生成2-4个节点）
  - [ ] 3.1.3 实现 CalculateNodeTypeWeights 方法（根据距离和循环次数调整权重）
  - [ ] 3.1.4 实现 SelectNodeTypeByWeight 方法（加权随机选择）
  - [ ] 3.1.5 实现 CreateNodeByType 方法（从配置表创建节点）

- [ ] 3.2 创建路线节点配置表
  - [ ] 3.2.1 设计 DRRouteNode 配置表结构
  - [ ] 3.2.2 创建安全节点配置数据（至少3个）
  - [ ] 3.2.3 创建资源节点配置数据（至少3个）
  - [ ] 3.2.4 创建捷径节点配置数据（至少3个）
  - [ ] 3.2.5 创建高危节点配置数据（至少3个）

### 4. 事件池系统实现

- [ ] 4.1 实现 EventPoolSystem 类
  - [ ] 4.1.1 实现构造函数（加载配置表、初始化事件池）
  - [ ] 4.1.2 实现 LoadEventPools 方法（从配置表加载事件）
  - [ ] 4.1.3 实现 DrawEvent 方法（抽取事件）
  - [ ] 4.1.4 实现 CheckTriggerConditions 方法（检查触发条件）
  - [ ] 4.1.5 实现 IsInCooldown 方法（检查冷却）
  - [ ] 4.1.6 实现 AdjustWeightByNodeType 方法（根据节点类型调整权重）
  - [ ] 4.1.7 实现 WeightedRandomSelect 方法（加权随机选择）
  - [ ] 4.1.8 实现 UpdateCooldowns 方法（更新冷却）
  - [ ] 4.1.9 实现 ParseConditions 方法（解析触发条件JSON）
  - [ ] 4.1.10 实现 ParseOptions 方法（解析选项JSON）
  - [ ] 4.1.11 实现 EventCondition.Evaluate 方法（评估条件是否满足）
    - 支持资源数量检查
    - 支持队伍状态检查
    - 支持距离值检查
    - _Requirements: 2.1_

- [ ] 4.2 创建事件配置表
  - [ ] 4.2.1 设计 DREvent 配置表结构
  - [ ] 4.2.2 创建搜刮事件配置数据（至少3个）
  - [ ] 4.2.3 创建遭遇事件配置数据（至少3个）
  - [ ] 4.2.4 创建冲突事件配置数据（至少3个）
  - [ ] 4.2.5 创建幸存者互动事件配置数据（至少3个）

### 5. 距离计算系统实现

- [ ] 5.1 实现 DistanceCalculationSystem 类
  - [ ] 5.1.1 实现构造函数（初始化常量）
  - [ ] 5.1.2 实现 Calculate 方法（执行距离计算）
  - [ ] 5.1.3 实现 ApplyRushStressCost 方法（应用急行军压力代价）
  - [ ] 5.1.4 实现 GetDistanceStatus 方法（获取距离状态）

- [ ] 5.2 实现距离UI组件
  - [ ] 5.2.1 创建追逐进度条UI预制体
  - [ ] 5.2.2 实现距离显示逻辑
  - [ ] 5.2.3 实现距离变化动画
  - [ ] 5.2.4 实现距离变化原因提示UI

## 第二阶段：结算系统和追猎系统

### 6. 结算系统实现

- [ ] 6.1 实现 SettlementSystem 类
  - [ ] 6.1.1 实现构造函数
  - [ ] 6.1.2 实现 ExecuteSettlement 方法（执行完整结算流程）
  - [ ] 6.1.3 实现 CalculateResourceChanges 方法（计算资源变化）
  - [ ] 6.1.4 实现 ApplyResourceChanges 方法（应用资源变化）
  - [ ] 6.1.5 实现 CalculateMemberStateChanges 方法（计算成员状态变化）
  - [ ] 6.1.6 实现 ApplyMemberStateChanges 方法（应用成员状态变化）
  - [ ] 6.1.7 实现 CalculateRelationshipChanges 方法（计算关系变化）
  - [ ] 6.1.8 实现 ApplyRelationshipChanges 方法（应用关系变化）
  - [ ] 6.1.9 实现 CheckCasualties 方法（检查伤亡）

- [ ] 6.2 实现结算UI组件
  - [ ] 6.2.1 创建结算摘要UI预制体
  - [ ] 6.2.2 实现资源变化显示
  - [ ] 6.2.3 实现成员状态变化显示
  - [ ] 6.2.4 实现关系变化显示
  - [ ] 6.2.5 实现伤亡事件显示

### 7. 追猎系统实现

- [ ] 7.1 实现 ChaseSystem 类
  - [ ] 7.1.1 实现构造函数（初始化常量）
  - [ ] 7.1.2 实现 ShouldTriggerChaseBattle 方法（检查是否触发追猎战）
  - [ ] 7.1.3 实现 StartChaseBattle 方法（开始追猎战）
  - [ ] 7.1.4 实现 CalculateChaseBattleDifficulty 方法（计算难度）
  - [ ] 7.1.5 实现 HandleVictory 方法（处理胜利）
  - [ ] 7.1.6 实现 HandleDefeat 方法（处理失败）
  - [ ] 7.1.7 实现 HandleRetreat 方法（处理撤离）
  - [ ] 7.1.8 实现 ApplyDefeatPenalty 方法（应用失败惩罚）
  - [ ] 7.1.9 实现 CheckGameOver 方法（检查游戏结束）

- [ ] 7.2 实现追猎战数据结构
  - [ ] 7.2.1 实现 ChaseBattleContext 类
  - [ ] 7.2.2 实现 ChaseBattleResult 类
  - [ ] 7.2.3 实现 ChaseBattleDifficulty 和 ChaseBattleOutcome 枚举

- [ ] 7.3 实现追猎战UI组件
  - [ ] 7.3.1 创建追猎战警告UI预制体
  - [ ] 7.3.2 实现追猎战结果显示UI

## 第三阶段：持久化和统计系统

### 8. 状态持久化系统实现

- [ ] 8.1 实现 StatePersistenceSystem 类
  - [ ] 8.1.1 实现构造函数
  - [ ] 8.1.2 实现 SaveState 方法（保存游戏状态，<100ms）
  - [ ] 8.1.3 实现 LoadState 方法（加载游戏状态）
  - [ ] 8.1.4 实现 SerializeGameState 方法（序列化为JSON）
  - [ ] 8.1.5 实现 DeserializeGameState 方法（从JSON反序列化）
  - [ ] 8.1.6 实现 ValidateGameState 方法（验证状态完整性）
  - [ ] 8.1.7 实现 DeleteSavedState 方法（删除保存）
  - [ ] 8.1.8 实现 SerializePlayerTeam 和 DeserializePlayerTeam 辅助方法
  - [ ] 8.1.9 添加性能监控（确保<100ms）
    - _Requirements: 7.4_

- [ ] 8.2 实现保存数据结构
  - [ ] 8.2.1 实现 GameStateSaveData 类
  - [ ] 8.2.2 实现 PlayerTeamSaveData 类
  - [ ] 8.2.3 实现 SettlementResultSaveData 类

- [ ] 8.3 实现微信小游戏适配
  - [ ] 8.3.1 添加微信存储API调用（WX.SetStorageSync/GetStorageSync）
  - [ ] 8.3.2 实现降级方案（PlayerPrefs）
  - [ ] 8.3.3 添加性能监控（确保<100ms）

### 9. 数据统计系统实现

- [ ] 9.1 实现 DataStatisticsSystem 类
  - [ ] 9.1.1 实现构造函数
  - [ ] 9.1.2 实现 RecordLoopStatistics 方法（记录循环数据）
  - [ ] 9.1.3 实现 GenerateReport 方法（生成统计报告）
  - [ ] 9.1.4 实现 GenerateDistanceCurve 方法（生成距离曲线）
  - [ ] 9.1.5 实现 IdentifyKeyDecisions 方法（识别关键决策点）
  - [ ] 9.1.6 实现 GeneratePostMortem 方法（生成复盘分析）
  - [ ] 9.1.7 实现 AnalyzeDamageReasons 方法（分析受损原因）
  - [ ] 9.1.8 实现 AnalyzeDistanceChangeReasons 方法（分析距离变化原因）
  - [ ] 9.1.9 实现 IdentifyCriticalMistakes 方法（识别关键失误）

- [ ] 9.2 实现统计数据结构
  - [ ] 9.2.1 实现 LoopStatisticsData 类
  - [ ] 9.2.2 实现 LoopStatistics 类
  - [ ] 9.2.3 实现 StatisticsReport 类
  - [ ] 9.2.4 实现 DistancePoint 类
  - [ ] 9.2.5 实现 KeyDecision 类
  - [ ] 9.2.6 实现 PostMortemAnalysis 类

- [ ] 9.3 实现统计UI组件
  - [ ] 9.3.1 创建统计报告UI预制体
  - [ ] 9.3.2 实现距离曲线图表显示
  - [ ] 9.3.3 实现关键决策点显示
  - [ ] 9.3.4 实现复盘分析显示

## 第四阶段：测试和优化

### 10. 单元测试

- [ ] 10.1 RouteNodeSystem 单元测试
  - [ ] 10.1.1 测试节点数量约束（2-4个）
  - [ ] 10.1.2 测试节点数据完整性
  - [ ] 10.1.3 测试无效输入异常处理

- [ ] 10.2 EventPoolSystem 单元测试
  - [ ] 10.2.1 测试事件抽取逻辑
  - [ ] 10.2.2 测试权重调整正确性
  - [ ] 10.2.3 测试冷却机制
  - [ ] 10.2.4 测试事件池为空处理

- [ ] 10.3 DistanceCalculationSystem 单元测试
  - [ ] 10.3.1 测试基础追近计算
  - [ ] 10.3.2 测试急行军效果
  - [ ] 10.3.3 测试深度搜刮效果
  - [ ] 10.3.4 测试战斗超时惩罚
  - [ ] 10.3.5 测试捷径节点加成
  - [ ] 10.3.6 测试距离下限约束

- [ ] 10.4 SettlementSystem 单元测试
  - [ ] 10.4.1 测试资源变化计算
  - [ ] 10.4.2 测试成员状态变化计算
  - [ ] 10.4.3 测试关系变化计算
  - [ ] 10.4.4 测试伤亡事件触发

- [ ] 10.5 ChaseSystem 单元测试
  - [ ] 10.5.1 测试追猎战触发条件
  - [ ] 10.5.2 测试难度计算
  - [ ] 10.5.3 测试胜利奖励
  - [ ] 10.5.4 测试失败惩罚
  - [ ] 10.5.5 测试撤离惩罚

- [ ] 10.6 StatePersistenceSystem 单元测试
  - [ ] 10.6.1 测试正常保存和加载
  - [ ] 10.6.2 测试序列化往返一致性
  - [ ] 10.6.3 测试无保存数据处理
  - [ ] 10.6.4 测试损坏数据处理

### 11. 属性测试（使用FsCheck）

- [ ] 11.1 配置属性测试框架
  - [ ] 11.1.1 安装 FsCheck NuGet 包
  - [ ] 11.1.2 配置全局测试参数（100次迭代）

- [ ] 11.2 实现核心属性测试
  - [ ] 11.2.1 Property 2: 路线节点数量约束
  - [ ] 11.2.2 Property 11: 伤亡事件触发
  - [ ] 11.2.3 Property 13: 距离计算综合正确性
  - [ ] 11.2.4 Property 24: 游戏状态序列化往返一致性
  - [ ] 11.2.5 Property 28: 事件冷却机制

- [ ] 11.3 实现测试数据生成器
  - [ ] 11.3.1 实现 RouteNodeGen 生成器
  - [ ] 11.3.2 实现 TeamMemberGen 生成器
  - [ ] 11.3.3 实现 DistanceCalculationDataGen 生成器
  - [ ] 11.3.4 实现 GameStateGenerator 生成器

### 12. 集成测试

- [ ] 12.1 完整循环流程测试
  - [ ] 12.1.1 测试正常循环流程（RouteSelection → NodeEvent → Settlement → DistanceCalculation → ChaseCheck → RouteSelection）
  - [ ] 12.1.2 测试追猎战流程（ChaseCheck → ChaseBattle → Settlement）
  - [ ] 12.1.3 测试胜利结束流程
  - [ ] 12.1.4 测试失败结束流程

- [ ] 12.2 状态持久化集成测试
  - [ ] 12.2.1 测试保存和加载完整游戏进度
  - [ ] 12.2.2 测试中断继续功能

### 13. 性能优化

- [ ] 13.1 性能测试
  - [ ] 13.1.1 测试状态保存性能（<100ms）
  - [ ] 13.1.2 测试单次循环时长（1-2分钟）
  - [ ] 13.1.3 测试事件抽取性能
  - [ ] 13.1.4 测试距离计算性能

- [ ] 13.2 性能优化
  - [ ] 13.2.1 优化序列化性能（如果超过100ms）
  - [ ] 13.2.2 优化事件池查询性能
  - [ ] 13.2.3 优化UI更新频率

### 14. 微信小游戏适配

- [ ] 14.1 平台适配
  - [ ] 14.1.1 测试微信存储API
  - [ ] 14.1.2 测试断点续玩功能
  - [ ] 14.1.3 测试内存占用
  - [ ] 14.1.4 测试加载时间

- [ ] 14.2 UI适配
  - [ ] 14.2.1 适配微信小游戏屏幕尺寸
  - [ ] 14.2.2 优化触摸操作
  - [ ] 14.2.3 优化字体大小和可读性

## 第五阶段：文档和交付

### 15. 文档完善

- [ ] 15.1 代码文档
  - [ ] 15.1.1 为所有公共API添加XML注释
  - [ ] 15.1.2 为复杂算法添加详细注释
  - [ ] 15.1.3 为配置表添加字段说明

- [ ] 15.2 使用文档
  - [ ] 15.2.1 编写系统使用指南
  - [ ] 15.2.2 编写配置表编辑指南
  - [ ] 15.2.3 编写测试运行指南

### 16. 代码审查和重构

- [ ] 16.1 代码审查
  - [ ] 16.1.1 审查代码风格一致性
  - [ ] 16.1.2 审查错误处理完整性
  - [ ] 16.1.3 审查性能瓶颈

- [ ] 16.2 代码重构
  - [ ] 16.2.1 重构重复代码
  - [ ] 16.2.2 优化命名和结构
  - [ ] 16.2.3 移除未使用代码

### 17. 最终验收

- [ ] 17.1 功能验收
  - [ ] 17.1.1 验证所有需求已实现
  - [ ] 17.1.2 验证所有测试通过
  - [ ] 17.1.3 验证性能指标达标

- [ ] 17.2 交付准备
  - [ ] 17.2.1 准备发布说明
  - [ ] 17.2.2 准备演示视频
  - [ ] 17.2.3 准备培训材料
