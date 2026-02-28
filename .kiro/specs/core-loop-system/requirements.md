# 需求文档：核心循环系统

## 简介

核心循环系统是"前路迷途"游戏的主要玩法循环，负责驱动玩家在逃亡过程中的路线选择、事件触发、战斗结算、距离博弈和追猎机制。该系统确保单局游戏时长控制在5~10分钟，并支持微信小游戏的3~8分钟可玩段要求。

## 术语表

- **Core_Loop_System（核心循环系统）**：管理路线选择、事件触发、战斗结算、距离计算和追猎战的完整游戏循环系统
- **Route_Node（路线节点）**：玩家在逃亡路线上可选择的地点，包含类型、事件和距离属性
- **Node_Event（节点事件）**：在路线节点触发的游戏事件，包括搜刮、遭遇、冲突和幸存者互动
- **Terror_Distance（大恐怖距离）**：玩家队伍与大恐怖之间的距离值（单位：km），核心博弈变量
- **Chase_Battle（追猎战）**：当大恐怖距离降至临界值时触发的特殊战斗
- **Settlement_System（结算系统）**：处理战斗或事件后的资源、状态、关系和距离变化的系统
- **Player_Team（玩家队伍）**：玩家控制的逃亡小队，包含多个成员和共享资源
- **Event_Pool（事件池）**：存储可触发事件的数据集合，按类型和条件分类
- **Distance_Calculation（距离计算）**：根据玩家行为和时间消耗计算大恐怖距离变化的逻辑
- **Node_Type（节点类型）**：路线节点的分类，包括安全、资源、捷径和高危四种

## 需求

### 需求 1：路线节点选择系统

**用户故事：** 作为玩家，我希望在每个回合选择不同类型的路线节点，以便根据当前队伍状态制定逃亡策略。

#### 验收标准

1. THE Core_Loop_System SHALL 提供四种节点类型供玩家选择：安全节点、资源节点、捷径节点和高危节点
2. WHEN 玩家进入路线选择阶段，THE Core_Loop_System SHALL 生成2到4个可选路线节点
3. THE Core_Loop_System SHALL 为每个路线节点显示节点类型、预期风险等级和距离推进值
4. WHEN 玩家选择一个路线节点，THE Core_Loop_System SHALL 记录选择并进入节点事件触发阶段
5. THE Core_Loop_System SHALL 确保每个节点类型具有不同的风险收益特征（安全节点低风险低收益，高危节点高风险高收益）

### 需求 2：节点事件触发系统

**用户故事：** 作为玩家，我希望在到达节点后触发相应的事件，以便体验不同的游戏内容和挑战。

#### 验收标准

1. WHEN 玩家到达路线节点，THE Core_Loop_System SHALL 从事件池中选择一个与节点类型匹配的事件
2. THE Core_Loop_System SHALL 支持四种基础事件类型：搜刮事件、遭遇事件、冲突事件和幸存者互动事件
3. WHEN 事件被触发，THE Core_Loop_System SHALL 向玩家展示事件描述和可选行动选项
4. THE Core_Loop_System SHALL 根据节点类型调整事件池的抽取权重（资源节点优先搜刮事件，高危节点优先遭遇事件）
5. WHEN 事件需要玩家决策，THE Core_Loop_System SHALL 等待玩家输入后再继续流程

### 需求 3：战斗与事件结算系统

**用户故事：** 作为玩家，我希望在完成战斗或事件后获得明确的结算反馈，以便了解队伍状态的变化。

#### 验收标准

1. WHEN 战斗或事件结束，THE Settlement_System SHALL 计算资源变化（食物、药品、弹药、零件）
2. WHEN 战斗或事件结束，THE Settlement_System SHALL 计算队伍成员的状态变化（生命、压力、忠诚、负伤等级）
3. WHEN 战斗或事件结束，THE Settlement_System SHALL 计算队伍成员之间的关系变化
4. THE Settlement_System SHALL 在结算完成后向玩家展示变化摘要，包括资源增减、成员状态和关系变化
5. THE Settlement_System SHALL 将结算结果保存到游戏状态中，供后续循环使用
6. WHEN 结算导致成员死亡或离队，THE Settlement_System SHALL 触发相应的叙事事件

### 需求 4：大恐怖距离计算系统

**用户故事：** 作为玩家，我希望我的每个行为都会影响与大恐怖的距离，以便在压迫感中做出策略决策。

#### 验收标准

1. THE Distance_Calculation SHALL 维护一个表示玩家队伍与大恐怖之间距离的数值（单位：km）
2. WHEN 游戏回合开始，THE Distance_Calculation SHALL 自动减少距离值4km（大恐怖基础追近速度）
3. WHEN 玩家选择急行军行为，THE Distance_Calculation SHALL 增加距离值6km并增加全队压力值8点
4. WHEN 玩家选择深度搜刮行为，THE Distance_Calculation SHALL 减少距离值5km
5. WHEN 战斗超时，THE Distance_Calculation SHALL 每超时10秒减少距离值1km
6. WHEN 玩家前进1个节点，THE Distance_Calculation SHALL 根据节点类型调整距离值（捷径节点额外增加距离）
7. THE Distance_Calculation SHALL 在UI顶部显示追逐进度条，实时展示双方位置和当前距离
8. WHEN 距离值发生重大变化（单次变化超过5km），THE Distance_Calculation SHALL 显示变化原因提示

### 需求 5：追猎战触发系统

**用户故事：** 作为玩家，我希望当大恐怖距离过近时触发追猎战，以便感受到紧迫的压力和挑战。

#### 验收标准

1. WHEN 大恐怖距离降至0km或以下，THE Core_Loop_System SHALL 触发追猎战事件
2. WHEN 追猎战被触发，THE Core_Loop_System SHALL 暂停常规循环并进入追猎战战斗流程
3. THE Core_Loop_System SHALL 在追猎战前向玩家展示警告提示，说明当前危机状态
4. WHEN 追猎战胜利，THE Core_Loop_System SHALL 增加大恐怖距离值并给予爆发性补给奖励
5. WHEN 追猎战失败，THE Core_Loop_System SHALL 触发强制惨痛代价（减员、资源损失或游戏结束）
6. IF 玩家在追猎战中选择撤离，THEN THE Core_Loop_System SHALL 允许撤离但施加重度惩罚（距离进一步缩短或队伍状态恶化）

### 需求 6：循环流程控制系统

**用户故事：** 作为玩家，我希望游戏循环流畅地从一个阶段过渡到下一个阶段，以便获得连贯的游戏体验。

#### 验收标准

1. THE Core_Loop_System SHALL 按照以下顺序执行循环：路线选择 → 节点事件触发 → 战斗或事件结算 → 距离结算 → 追猎战检查
2. WHEN 一个循环阶段完成，THE Core_Loop_System SHALL 自动进入下一个阶段
3. WHEN 追猎战检查未触发追猎战，THE Core_Loop_System SHALL 返回路线选择阶段开始新循环
4. WHEN 玩家队伍到达终点，THE Core_Loop_System SHALL 结束循环并触发胜利结算
5. WHEN 玩家队伍崩溃（主角死亡或全员离队），THE Core_Loop_System SHALL 结束循环并触发失败结算
6. THE Core_Loop_System SHALL 确保单次完整循环的时间控制在1到2分钟内

### 需求 7：循环状态持久化系统

**用户故事：** 作为玩家，我希望游戏能够保存我的循环进度，以便在微信小游戏中支持中断继续。

#### 验收标准

1. WHEN 循环的任一阶段完成，THE Core_Loop_System SHALL 将当前游戏状态序列化并保存
2. THE Core_Loop_System SHALL 保存的状态包括：当前循环阶段、队伍状态、资源数据、距离值和事件历史
3. WHEN 玩家重新进入游戏，THE Core_Loop_System SHALL 加载保存的状态并恢复到上次中断的阶段
4. THE Core_Loop_System SHALL 确保状态保存操作不超过100毫秒，避免影响游戏流畅度
5. IF 状态加载失败，THEN THE Core_Loop_System SHALL 提示玩家并提供重新开始选项

### 需求 8：事件池管理系统

**用户故事：** 作为开发者，我希望事件池能够灵活配置和扩展，以便快速迭代游戏内容。

#### 验收标准

1. THE Event_Pool SHALL 支持从配置文件或数据表加载事件数据
2. THE Event_Pool SHALL 为每个事件存储以下属性：事件ID、事件类型、触发条件、描述文本、选项列表和结算规则
3. WHEN Core_Loop_System 请求事件，THE Event_Pool SHALL 根据节点类型和触发条件筛选可用事件
4. THE Event_Pool SHALL 支持事件权重配置，允许调整不同事件的出现概率
5. THE Event_Pool SHALL 支持事件冷却机制，防止同一事件在短时间内重复触发
6. THE Event_Pool SHALL 在运行时验证事件数据的完整性，并在发现错误时记录日志

### 需求 9：循环数据统计系统

**用户故事：** 作为玩家，我希望在循环结束后查看详细的统计数据，以便复盘和改进策略。

#### 验收标准

1. THE Core_Loop_System SHALL 记录每次循环的关键数据：节点选择、事件类型、战斗结果、距离变化和资源变化
2. WHEN 游戏结束（胜利或失败），THE Core_Loop_System SHALL 生成统计报告，包括总循环次数、距离变化曲线和关键决策点
3. THE Core_Loop_System SHALL 为每次距离重大变化提供原因说明，确保玩家可复盘
4. THE Core_Loop_System SHALL 记录至少3条复盘信息：受损原因、距离变化原因和关键失误
5. THE Core_Loop_System SHALL 将统计数据保存到本地，供玩家后续查看

### 需求 10：循环时长控制系统

**用户故事：** 作为玩家，我希望单局游戏时长可控，以便适应微信小游戏的碎片化场景。

#### 验收标准

1. THE Core_Loop_System SHALL 确保单局游戏的总循环次数控制在5到15次之间
2. THE Core_Loop_System SHALL 根据玩家行为动态调整循环节奏，确保单局时长在5到10分钟内
3. WHEN 单局时长超过10分钟，THE Core_Loop_System SHALL 提高终点接近速度或触发加速事件
4. WHEN 单局时长少于5分钟，THE Core_Loop_System SHALL 增加中间节点数量或降低距离推进速度
5. THE Core_Loop_System SHALL 支持微信小游戏的3到8分钟可玩段，允许玩家在任意循环阶段暂停并保存进度

## 附加说明

### 解析器与序列化器需求

本系统涉及游戏状态的序列化和反序列化，需要特别注意以下要求：

1. **状态序列化器**：THE Core_Loop_System SHALL 提供状态序列化器，将游戏状态转换为可存储的格式（JSON或二进制）
2. **状态解析器**：THE Core_Loop_System SHALL 提供状态解析器，从存储格式还原游戏状态
3. **往返属性测试**：FOR ALL 有效的游戏状态对象，序列化后再反序列化 SHALL 产生等价的状态对象（round-trip property）
4. **错误处理**：WHEN 解析器遇到无效数据，THE Core_Loop_System SHALL 返回描述性错误信息，而不是崩溃

### 数值平衡建议

以下数值为初始建议，可在实现后根据测试调整：

- 大恐怖基础追近速度：4 km/回合
- 急行军距离增加：6 km
- 深度搜刮距离减少：5 km
- 战斗超时惩罚：1 km/10秒
- 追猎战触发阈值：距离 ≤ 0 km
- 单次循环目标时长：1~2分钟
- 单局目标循环次数：5~15次

### 微信小游戏适配要点

- 状态保存频率：每个循环阶段结束时自动保存
- 保存操作性能：不超过100毫秒
- 断点续玩：支持从任意循环阶段恢复
- 操作简化：战斗轻操作，策略重决策
