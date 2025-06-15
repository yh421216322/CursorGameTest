// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：SurvivorModel.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了与游戏中“幸存者”相关的核心数据模型和枚举类型。
//     包括幸存者的性别、当前工作、职业专长、状态、技能类型等枚举，
//     以及存储单个幸存者详细动态数据的 SurvivorData 类，
//     一个可能用于兼容旧版的 LegacySurvivorData 类，
//     和用于管理整个幸存者系统的聚合数据 SurvivorSystemData 类。
// ==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine; // 用于 Vector3, Mathf 等 Unity API
using QFramework;  // QFramework 框架 (当前在此文件中未直接使用其特性，但可能用于项目中其他部分)

namespace SurvivalGame.Model
{
    /// <summary>
    /// 幸存者的性别枚举。
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// 男性幸存者。
        /// </summary>
        Male,       // 男性
        /// <summary>
        /// 女性幸存者。
        /// </summary>
        Female      // 女性
    }

    /// <summary>
    /// 幸存者当前从事的工作类型或任务类别枚举。
    /// 这描述了幸存者当前正在执行的宏观活动。
    /// </summary>
    public enum SurvivorJob
    {
        /// <summary>
        /// 空闲状态，当前未执行任何特定工作。
        /// </summary>
        Idle,           // 空闲
        /// <summary>
        /// 从事生产类工作，如农耕、伐木、采矿、工坊制造等。
        /// </summary>
        Production,     // 生产工作
        /// <summary>
        /// 从事防御类工作，如守卫岗位、操作防御设施等。
        /// </summary>
        Defense,        // 防御工作
        /// <summary>
        /// 从事科技研究工作。
        /// </summary>
        Research,       // 研究工作
        /// <summary>
        /// 从事建筑建造或维修工作。
        /// </summary>
        Construction,   // 建造工作
        /// <summary>
        /// 从事医疗救护工作，如治疗伤员。
        /// </summary>
        Medical,        // 医疗工作
        /// <summary>
        /// 从事外出探索或侦察任务。
        /// </summary>
        Exploration,    // 探索工作
        /// <summary>
        /// 从事领导或管理类工作，可能影响团队士气或效率。
        /// </summary>
        Leadership      // 领导工作
    }

    /// <summary>
    /// 幸存者的职业专长枚举。
    /// 这通常代表幸存者固有的、或经过训练后获得的专业身份，影响其在特定工作上的效率或能力。
    /// </summary>
    public enum SurvivorProfession
    {
        /// <summary>
        /// 平民：没有特定专长，但可以从事多种基础工作，是团队的基础。
        /// </summary>
        Civilian,       // 平民 - 通用劳动者
        /// <summary>
        /// 士兵：擅长战斗和使用武器，在防御和探索的战斗方面有优势。
        /// </summary>
        Soldier,        // 士兵 - 擅长战斗
        /// <summary>
        /// 工程师：擅长建筑的建造、升级和维修，也可能擅长操作复杂设备。
        /// </summary>
        Engineer,       // 工程师 - 擅长建造和修理
        /// <summary>
        /// 医生：擅长医疗救护，能更有效地治疗伤病。
        /// </summary>
        Doctor,         // 医生 - 擅长治疗
        /// <summary>
        /// 科学家：擅长科技研究，能加快研究速度或解锁高级科技。
        /// </summary>
        Scientist,      // 科学家 - 擅长研究
        /// <summary>
        /// 侦察员：擅长外出探索、规避危险和收集情报。
        /// </summary>
        Scout,          // 侦察员 - 擅长探索
        /// <summary>
        /// 工人：专注于生产类工作，如资源采集和基础制造，效率较高。
        /// </summary>
        Worker,         // 工人 - 擅长生产
        /// <summary>
        /// 守卫：专门负责防御任务，如站岗放哨，可能在特定防御岗位有加成。
        /// </summary>
        Guard           // 守卫 - 专门防御
    }

    /// <summary>
    /// 幸存者当前的生理或行为状态枚举。
    /// </summary>
    public enum SurvivorState
    {
        /// <summary>
        /// 空闲：有行动能力但当前没有任务。
        /// </summary>
        Idle,           // 空闲
        /// <summary>
        /// 工作中：正在执行分配的任务。
        /// </summary>
        Working,        // 工作中
        /// <summary>
        /// 战斗中：正在与敌人交战。
        /// </summary>
        Fighting,       // 战斗中
        /// <summary>
        /// 巡逻中：在指定区域执行巡逻任务。
        /// </summary>
        Patrolling,     // 巡逻中
        /// <summary>
        /// 休息中：正在恢复体力或满足其他生理需求。
        /// </summary>
        Resting,        // 休息中
        /// <summary>
        /// 受伤：生命值较低或有伤病，可能影响工作和战斗能力。
        /// </summary>
        Injured,        // 受伤
        /// <summary>
        /// 死亡：幸存者已失去生命。
        /// </summary>
        Dead            // 死亡
    }

    /// <summary>
    /// 幸存者可拥有的技能类型枚举。
    /// 技能通常可以通过经验提升等级，影响相关活动的效率或效果。
    /// </summary>
    public enum SurvivorSkill
    {
        /// <summary>
        /// 战斗技能：影响攻击力、防御力、命中率等战斗相关表现。
        /// </summary>
        Combat,         // 战斗技能
        /// <summary>
        /// 建造技能：影响建筑的建造和修理速度。
        /// </summary>
        Construction,   // 建造技能
        /// <summary>
        /// 医疗技能：影响治疗效果和速度。
        /// </summary>
        Medicine,       // 医疗技能
        /// <summary>
        /// 研究技能：影响科技研发的速度。
        /// </summary>
        Research,       // 研究技能
        /// <summary>
        /// 领导技能：可能影响团队士气、其他幸存者的工作效率等。
        /// </summary>
        Leadership,     // 领导技能
        /// <summary>
        /// 潜行技能：影响在探索或特定任务中被敌人发现的几率。
        /// </summary>
        Stealth,        // 潜行技能
        /// <summary>
        /// 修理技能：影响修理设备或建筑的效率。
        /// </summary>
        Repair,         // 修理技能
        /// <summary>
        /// 采集技能：影响采集资源的数量和速度。
        /// </summary>
        Gathering       // 采集技能
    }

    /// <summary>
    /// 新的幸存者数据结构 - 基于属性系统。
    /// 存储单个幸存者的所有动态信息和状态。
    /// </summary>
    [Serializable] // 标记为可序列化，以便能在Unity Inspector中显示或保存到文件
    public class SurvivorData
    {
        // --- 基本信息 ---
        /// <summary>
        /// 幸存者的唯一标识符 (GUID)。
        /// </summary>
        public string Id;                           // 唯一ID
        /// <summary>
        /// 幸存者的姓名。
        /// </summary>
        public string Name;                         // 姓名
        /// <summary>
        /// 幸存者的年龄。
        /// </summary>
        public int Age;                             // 年龄
        /// <summary>
        /// 幸存者的性别 (参考Gender枚举)。
        /// </summary>
        public Gender Gender;                       // 性别
        
        // --- 核心状态属性 ---
        /// <summary>
        /// 当前健康值 (通常范围0-100)。低于0表示死亡。
        /// </summary>
        public int Health;                          // 健康值 (0-100)
        /// <summary>
        /// 当前士气值 (通常范围0-100)。影响工作效率和特殊事件。
        /// </summary>
        public int Morale;                          // 士气 (0-100)
        /// <summary>
        /// 当前疲劳度 (通常范围0-100)。过高会影响效率或强制休息。
        /// </summary>
        public int Fatigue;                         // 疲劳度 (0-100)
        /// <summary>
        /// 当前饥饿度 (通常范围0-100)。过高会影响健康或士气。
        /// </summary>
        public int Hunger;                          // 饥饿度 (0-100)
        
        // --- 工作与状态 ---
        /// <summary>
        /// 当前从事的工作类型 (参考SurvivorJob枚举)。
        /// </summary>
        public SurvivorJob CurrentJob;              // 当前工作
        /// <summary>
        /// 如果当前正在工作，此为分配到的建筑的唯一ID。空字符串表示未分配。
        /// </summary>
        public string AssignedBuildingId;           // 分配的建筑ID
        /// <summary>
        /// 基础工作效率乘数 (例如1.0f代表100%效率)。
        /// </summary>
        public float WorkEfficiency;                // 工作效率
        /// <summary>
        /// 幸存者当前的具体行为状态 (参考SurvivorState枚举)。
        /// </summary>
        public SurvivorState State;                 // 当前状态 (Idle, Working, etc.)
        
        // --- 属性/技能系统 ---
        /// <summary>
        /// 幸存者拥有的各项属性/技能及其等级/经验列表。
        /// (参考SurvivorAttributeData类和SurvivorAttributeType枚举)
        /// </summary>
        public List<SurvivorAttributeData> Attributes; // 7个核心属性
        
        // --- 位置与移动信息 ---
        /// <summary>
        /// 幸存者当前在世界空间中的位置。
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// 幸存者当前移动的目标位置。
        /// </summary>
        public Vector3 TargetPosition;
        
        // --- 经验与等级 ---
        /// <summary>
        /// 幸存者的通用经验值，可用于提升等级或特定技能。
        /// </summary>
        public int Experience;                      // 通用经验值 (用于角色等级或可分配技能点)
        /// <summary>
        /// 幸存者的角色等级。
        /// </summary>
        public int Level;                           // 角色等级
        
        // --- 装备信息 ---
        /// <summary>
        /// 当前装备的武器ID或名称。空字符串表示未装备。
        /// </summary>
        public string Weapon;
        /// <summary>
        /// 当前装备的护甲ID或名称。空字符串表示未装备。
        /// </summary>
        public string Armor;
        
        // --- 状态计时器 ---
        /// <summary>
        /// 上次执行主要动作或状态更新的时间戳 (Time.time)。
        /// </summary>
        public float LastActionTime;
        /// <summary>
        /// 开始当前工作的时间戳 (Time.time)。用于计算工作时长和疲劳等。
        /// </summary>
        public float WorkStartTime;
        
        /// <summary>
        /// SurvivorData的构造函数。
        /// 初始化新幸存者实例的各项默认值。
        /// </summary>
        public SurvivorData()
        {
            Id = System.Guid.NewGuid().ToString(); // 生成全局唯一ID
            Name = "未命名幸存者";                 // 默认名称
            Age = 25;                            // 默认年龄
            Gender = Gender.Male;                // 默认性别
            Health = 100;                        // 默认满健康
            Morale = 75;                         // 默认士气
            Fatigue = 0;                         // 默认无疲劳
            Hunger = 0;                          // 默认不饥饿
            CurrentJob = SurvivorJob.Idle;       // 默认空闲
            State = SurvivorState.Idle;          // 默认空闲状态
            AssignedBuildingId = "";             // 默认未分配建筑
            WorkEfficiency = 1.0f;               // 默认100%工作效率
            Attributes = new List<SurvivorAttributeData>(); // 初始化属性列表
            Position = Vector3.zero;             // 默认位置在原点
            TargetPosition = Vector3.zero;       // 默认目标位置在原点
            Experience = 0;                      // 初始经验为0
            Level = 1;                           // 初始等级为1
            Weapon = "";                         // 初始未装备武器
            Armor = "";                          // 初始未装备护甲
            WorkStartTime = 0f;                  // 工作开始时间初始化
            LastActionTime = Time.time;          // 初始化上次行动时间
        }

        /// <summary>
        /// 获取指定类型属性的当前数值。
        /// </summary>
        /// <param name="type">要查询的属性类型。</param>
        /// <returns>属性的数值；如果该属性未定义，则返回一个默认值 (当前为20)。</returns>
        public int GetAttributeValue(SurvivorAttributeType type)
        {
            // 从属性列表中查找指定类型的属性
            var attr = Attributes.Find(a => a.Type == type);
            // 如果找到则返回其Value，否则返回默认值20 (这个默认值可能需要调整或配置化)
            return attr?.Value ?? 20;
        }
        
        /// <summary>
        /// 设置指定类型属性的数值。
        /// 数值会被限制在0到100之间。
        /// </summary>
        /// <param name="type">要设置的属性类型。</param>
        /// <param name="value">要设置的新数值。</param>
        public void SetAttributeValue(SurvivorAttributeType type, int value)
        {
            var attr = Attributes.Find(a => a.Type == type);
            if (attr != null) // 如果找到了该属性
            {
                attr.Value = Mathf.Clamp(value, 0, 100); // 更新其值，并确保在0-100范围内
            }
            // else: 如果属性不存在，可以选择创建它或记录一个警告
        }
        
        /// <summary>
        /// 为指定类型的属性添加经验值。
        /// </summary>
        /// <param name="type">要添加经验的属性类型。</param>
        /// <param name="exp">要添加的经验量。</param>
        /// <returns>如果属性因此次经验增加而提升了Value，则返回true；否则返回false。</returns>
        public bool AddAttributeExperience(SurvivorAttributeType type, int exp)
        {
            var attr = Attributes.Find(a => a.Type == type);
            // 如果找到属性，则调用其AddExperience方法；否则返回false
            return attr?.AddExperience(exp) ?? false;
        }
        
        /// <summary>
        /// (只读属性) 判断幸存者当前是否存活 (健康值大于0)。
        /// </summary>
        public bool IsAlive => Health > 0;
        
        /// <summary>
        /// (只读属性) 判断幸存者当前是否能够工作。
        /// 条件：存活、健康值高于30、士气高于20、疲劳度低于80。
        /// </summary>
        public bool CanWork => IsAlive && Health > 30 && Morale > 20 && Fatigue < 80;
        
        /// <summary>
        /// (只读属性) 判断幸存者当前是否能够战斗。
        /// 条件：存活、健康值高于50、士气高于30、疲劳度低于70。
        /// </summary>
        public bool CanFight => IsAlive && Health > 50 && Morale > 30 && Fatigue < 70;
        
        /// <summary>
        /// 计算并获取幸存者当前的综合工作效率。
        /// 综合效率受到基础工作效率、健康、士气和疲劳度的共同影响。
        /// </summary>
        /// <returns>综合工作效率的乘数因子 (例如，1.0f代表100%)。</returns>
        public float GetTotalWorkEfficiency()
        {
            float baseEfficiency = WorkEfficiency; // 基础效率
            
            // 健康状况对效率的影响 (0%-100%)
            float healthModifier = Health / 100f;
            // 士气对效率的影响 (0%-100%)
            float moraleModifier = Morale / 100f;
            // 疲劳度对效率的影响 (疲劳越低，效率越高，100%为无疲劳影响)
            float fatigueModifier = (100f - Fatigue) / 100f;
            
            // 综合效率 = 基础效率 * 各项修正因子
            return baseEfficiency * healthModifier * moraleModifier * fatigueModifier;
        }
        
        /// <summary>
        /// 设置幸存者的新状态，并触发状态变更后的逻辑处理。
        /// </summary>
        /// <param name="newState">要设置的新状态。</param>
        public void SetState(SurvivorState newState)
        {
            SurvivorState previousState = State; // 保存旧状态，用于比较
            if (previousState == newState) return; // 如果状态未改变，则不执行操作

            State = newState; // 更新状态
            
            // 调用状态改变后的处理逻辑
            OnStateChanged(previousState, newState);
        }
        
        /// <summary>
        /// 当幸存者状态发生改变时，执行的内部逻辑处理。
        /// 例如，根据新状态调整当前工作、记录时间等。
        /// </summary>
        /// <param name="previousState">改变前的旧状态。</param>
        /// <param name="newState">改变后的新状态。</param>
        private void OnStateChanged(SurvivorState previousState, SurvivorState newState)
        {
            LastActionTime = Time.time; // 记录状态变更（或任何重要行动）的时间戳
            
            // 根据新的状态执行特定操作
            switch (newState)
            {
                case SurvivorState.Working: // 如果进入工作状态
                    // 如果之前是空闲且没有明确分配工作，则默认分配一个生产类工作（示例逻辑）
                    if (CurrentJob == SurvivorJob.Idle)
                    {
                        CurrentJob = SurvivorJob.Production; // TODO: 此处逻辑可能需要调整，不应随意改变CurrentJob
                    }
                    WorkStartTime = Time.time; // 记录工作开始时间
                    break;
                    
                case SurvivorState.Idle: // 如果进入空闲状态
                    CurrentJob = SurvivorJob.Idle; // 将当前工作也设为空闲
                    AssignedBuildingId = "";       // 清除已分配的建筑ID
                    break;
                    
                case SurvivorState.Injured: // 如果进入受伤状态
                    // 受伤时通常会停止当前所有工作
                    CurrentJob = SurvivorJob.Idle;
                    AssignedBuildingId = "";
                    // TODO: 可能需要触发寻求医疗的AI行为
                    break;
                    
                case SurvivorState.Dead: // 如果进入死亡状态
                    // 死亡时清除所有工作分配和状态
                    CurrentJob = SurvivorJob.Idle;
                    AssignedBuildingId = "";
                    Health = 0; // 确保健康值为0
                    // TODO: 触发死亡相关的游戏逻辑（例如，从列表中移除、更新人口等）
                    break;
                    
                case SurvivorState.Resting: // 如果进入休息状态
                    // 休息时通常会停止当前工作，但可能保留工作分配信息（AssignedBuildingId）
                    CurrentJob = SurvivorJob.Idle; // 表现为空闲，但可能仍在某个建筑内休息
                    break;
            }
        }
        
        /// <summary>
        /// 为幸存者分配一项新工作。
        /// </summary>
        /// <param name="job">要分配的新工作类型。</param>
        /// <param name="buildingId">（可选）与此工作关联的建筑ID。</param>
        /// <returns>如果分配成功（例如幸存者能够工作），则返回true；否则返回false。</returns>
        public bool AssignJob(SurvivorJob job, string buildingId = "")
        {
            // 检查幸存者当前是否能够承担工作
            if (!CanWork)
            {
                Debug.LogWarning($"[SurvivorData] 幸存者 {Name} (ID: {Id}) 当前状态无法分配工作。");
                return false; // 不能工作则分配失败
            }
            
            CurrentJob = job; // 设置新的工作类型
            AssignedBuildingId = buildingId; // 设置分配的建筑ID
            
            // 根据新的工作类型，更新幸存者的具体状态 (State)
            if (job != SurvivorJob.Idle) // 如果分配的是非空闲工作
            {
                SetState(SurvivorState.Working); // 将状态设为工作中
            }
            else // 如果分配的是空闲 (例如解除分配)
            {
                SetState(SurvivorState.Idle); // 将状态设为空闲
            }
            
            return true; // 分配成功
        }
        
        /// <summary>
        /// 使幸存者受到指定量的伤害。
        /// 会同时影响健康值和士气值。如果健康值降至0或以下，状态会变为死亡。
        /// 如果健康值较低，状态会变为受伤。
        /// </summary>
        /// <param name="damage">受到的伤害量 (应为正数)。</param>
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return; // 不处理非正数伤害

            Health = Mathf.Max(0, Health - damage); // 减少健康值，但不低于0
            Morale = Mathf.Max(0, Morale - damage / 2); // 假设伤害会同时降低士气，其影响量为伤害的一半
            
            // 根据健康值更新状态
            if (Health <= 0) // 如果健康值耗尽
            {
                SetState(SurvivorState.Dead); // 设置为死亡状态
            }
            else if (Health <= 30 && State != SurvivorState.Dead) // 如果健康值低于30但还活着 (阈值可配置)
            {
                SetState(SurvivorState.Injured); // 设置为受伤状态
            }
        }
        
        /// <summary>
        /// 为幸存者恢复指定量的健康值。
        /// 如果幸存者之前是受伤状态且健康恢复到一定程度，状态可能变回空闲。
        /// </summary>
        /// <param name="amount">要恢复的健康量 (应为正数)。</param>
        public void Heal(int amount)
        {
            if (amount <= 0 || State == SurvivorState.Dead) return; // 不治疗非正数量或已死亡者

            Health = Mathf.Min(100, Health + amount); // 增加健康值，但不超过上限100
            
            // 如果之前是受伤状态，并且健康恢复到30以上 (阈值可配置)
            if (State == SurvivorState.Injured && Health > 30)
            {
                SetState(SurvivorState.Idle); // 状态恢复为空闲 (如果之前没有其他任务)
            }
        }
        
        /// <summary>
        /// 使幸存者进入休息状态（如果当前状态允许）。
        /// </summary>
        public void Rest()
        {
            // 只有非死亡、非重伤（或其他不允许休息的）状态下才能主动休息
            if (State != SurvivorState.Dead && State != SurvivorState.Injured)
            {
                SetState(SurvivorState.Resting);
            }
        }
        
        /// <summary>
        /// 使幸存者进入战斗状态（如果当前状态允许）。
        /// </summary>
        /// <returns>如果成功进入战斗状态，则返回true；否则返回false。</returns>
        public bool StartFighting()
        {
            if (!CanFight) // 检查是否满足战斗条件
            {
                Debug.LogWarning($"[SurvivorData] 幸存者 {Name} (ID: {Id}) 当前无法进入战斗状态。");
                return false;
            }
            
            SetState(SurvivorState.Fighting); // 设置为战斗状态
            return true;
        }
        
        /// <summary>
        /// 使幸存者开始巡逻（如果当前状态允许）。
        /// 巡逻通常被视为一种防御类工作。
        /// </summary>
        /// <returns>如果成功开始巡逻，则返回true；否则返回false。</returns>
        public bool StartPatrolling()
        {
            if (!CanWork) // 检查是否能工作（巡逻也是一种工作）
            {
                 Debug.LogWarning($"[SurvivorData] 幸存者 {Name} (ID: {Id}) 当前无法开始巡逻。");
                return false;
            }
            
            CurrentJob = SurvivorJob.Defense; // 将当前工作设为防御类
            SetState(SurvivorState.Patrolling); // 设置具体状态为巡逻中
            return true;
        }
        
        /// <summary>
        /// 更新幸存者的状态，例如随时间变化的疲劳和饥饿。
        /// 此方法应由游戏主循环或幸存者系统定期调用。
        /// </summary>
        public void UpdateState()
        {
            if (State == SurvivorState.Dead) return; // 死亡的幸存者不更新状态

            // 根据当前状态更新疲劳度
            if (State == SurvivorState.Working || State == SurvivorState.Fighting || State == SurvivorState.Patrolling)
            {
                // 假设工作时间越长，疲劳增长越快 (这里的0.1f是每秒疲劳增长系数，需要平衡)
                float workDuration = Time.time - WorkStartTime; // 简单计算工作时长
                // Fatigue += Mathf.RoundToInt(Time.deltaTime * 0.5f); // 更平滑的增长方式：每秒增长0.5点疲劳
                Fatigue = Mathf.Min(100, Fatigue + Mathf.RoundToInt(workDuration * 0.01f + Time.deltaTime * 0.2f)); // 混合增长，避免workstartTime未更新时疲劳不增长
            }
            else if (State == SurvivorState.Resting)
            {
                // 休息时快速减少疲劳 (这里的10f是每秒疲劳恢复系数)
                Fatigue = Mathf.Max(0, Fatigue - Mathf.RoundToInt(Time.deltaTime * 10f));
            }
            
            // 更新饥饿度 (假设饥饿度随时间持续增长)
            // 0.5f是每秒饥饿增长系数，需要平衡
            Hunger = Mathf.Min(100, Hunger + Mathf.RoundToInt(Time.deltaTime * 0.5f));
            
            // 检查是否因状态变化（如过劳、过饿、重伤）需要自动转换到其他状态
            CheckAutoStateTransition();
        }
        
        /// <summary>
        /// (私有辅助方法) 检查并执行幸存者状态的自动转换逻辑。
        /// 例如：疲劳过高自动休息，健康过低自动受伤等。
        /// </summary>
        private void CheckAutoStateTransition()
        {
            // 如果当前正在工作且疲劳度达到很高水平（例如90）
            if (Fatigue >= 90 && State == SurvivorState.Working)
            {
                Rest(); // 强制进入休息状态
                Debug.Log($"[SurvivorData] 幸存者 {Name} 因过度疲劳 ({Fatigue}) 已自动进入休息状态。");
                return; // 状态已改变，不再执行后续检查
            }
            
            // 如果健康值过低（例如20或以下），且当前不是已受伤或已死亡状态
            if (Health <= 20 && State != SurvivorState.Injured && State != SurvivorState.Dead)
            {
                SetState(SurvivorState.Injured); // 强制进入受伤状态
                Debug.Log($"[SurvivorData] 幸存者 {Name} 因健康值过低 ({Health}) 已自动进入受伤状态。");
                return;
            }
            
            // 如果当前正在休息，并且疲劳度已恢复到较低水平（例如20或以下），且之前有工作分配
            if (Fatigue <= 20 && State == SurvivorState.Resting && CurrentJob != SurvivorJob.Idle)
            {
                SetState(SurvivorState.Working); // 自动恢复到工作状态
                Debug.Log($"[SurvivorData] 幸存者 {Name} 休息完毕，已自动恢复工作 ({CurrentJob})。");
                return;
            }
            
            // 如果当前被分配的工作是空闲，但状态仍然是“工作中”（可能因之前操作遗留）
            if (CurrentJob == SurvivorJob.Idle && State == SurvivorState.Working)
            {
                SetState(SurvivorState.Idle); // 将状态同步为空闲
            }
            // TODO: 添加更多自动状态转换逻辑，如饥饿、士气过低等导致的状变。
        }
    }

    // 保留原有的SurvivorData类作为兼容性参考或旧版数据结构。
    // 在新系统中，应优先使用上面定义的 SurvivorData 类。
    /// <summary>
    /// 旧版的幸存者数据结构 (LegacySurvivorData)。
    /// 注释表明此类是为了与旧系统兼容而保留的。
    /// 包含了与新 SurvivorData 结构有所不同的字段和方法。
    /// </summary>
    [Serializable]
    public class LegacySurvivorData
    {
        /// <summary>
        /// 幸存者的唯一ID。
        /// </summary>
        public string id;
        /// <summary>
        /// 幸存者姓名。
        /// </summary>
        public string name;
        /// <summary>
        /// 幸存者的职业专长 (参考SurvivorProfession枚举)。
        /// </summary>
        public SurvivorProfession profession;
        /// <summary>
        /// 幸存者当前状态 (参考SurvivorState枚举)。
        /// </summary>
        public SurvivorState state;
        /// <summary>
        /// 幸存者等级。
        /// </summary>
        public int level;
        
        /// <summary>
        /// 某个综合技能值或特定技能值 (0-100)。具体指代不明，需参考旧系统设计。
        /// </summary>
        public int Skill;                 // 技能(0-100)
        /// <summary>
        /// 当前从事的工作类型 (参考旧版JobType枚举)。
        /// </summary>
        public JobType Job;               // 当前工作
        /// <summary>
        /// 工作效率的基础值或乘数。
        /// </summary>
        public float WorkEfficiency;      // 工作效率
        
        // --- 基本战斗和生理属性 ---
        /// <summary>
        /// 最大健康值。
        /// </summary>
        public float maxHealth;
        /// <summary>
        /// 当前健康值。
        /// </summary>
        public float currentHealth;
        /// <summary>
        /// 攻击力。
        /// </summary>
        public float attack;
        /// <summary>
        /// 防御力。
        /// </summary>
        public float defense;
        /// <summary>
        /// 移动速度。
        /// </summary>
        public float speed;
        /// <summary>
        /// 士气值。
        /// </summary>
        public float morale;
        
        // --- 位置信息 ---
        /// <summary>
        /// 当前世界坐标。
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// 移动目标的世界坐标。
        /// </summary>
        public Vector3 targetPosition;
        
        // --- 工作分配信息 ---
        /// <summary>
        /// 分配到的建筑的ID。
        /// </summary>
        public string assignedBuildingId;
        /// <summary>
        /// 当前执行的具体任务描述或ID。
        /// </summary>
        public string currentTask;
        
        // --- 技能等级 ---
        /// <summary>
        /// 存储各项技能等级的字典 (参考SurvivorSkill枚举)。
        /// </summary>
        public Dictionary<SurvivorSkill, int> skills;
        
        // --- 装备 ---
        /// <summary>
        /// 当前装备的武器ID或名称。
        /// </summary>
        public string weapon;
        /// <summary>
        /// 当前装备的护甲ID或名称。
        /// </summary>
        public string armor;
        
        // --- 状态相关计时器 ---
        /// <summary>
        /// 上次执行动作的时间戳。
        /// </summary>
        public float lastActionTime;
        /// <summary>
        /// 开始当前工作的时间戳。
        /// </summary>
        public float workStartTime;
        /// <summary>
        /// (可能指) 还需要多少休息时间，或下次需要休息的时间点。
        /// </summary>
        public float restNeeded;
        
        // --- 经验与成长 ---
        /// <summary>
        /// 通用经验值。
        /// </summary>
        public int experience;
        /// <summary>
        /// 可用于提升技能的技能点数。
        /// </summary>
        public int skillPoints;
        
        /// <summary>
        /// LegacySurvivorData的构造函数。
        /// 初始化ID、状态、等级、技能字典，并调用辅助方法初始化基础属性和技能。
        /// </summary>
        public LegacySurvivorData()
        {
            id = System.Guid.NewGuid().ToString(); // 生成唯一ID
            state = SurvivorState.Idle;           // 默认状态为空闲
            level = 1;                            // 默认等级为1
            skills = new Dictionary<SurvivorSkill, int>(); // 初始化技能字典
            
            InitializeBasicStats(); // 初始化基础属性
            InitializeSkills();     // 初始化技能等级并应用职业加成
            
            currentHealth = maxHealth; // 初始时满健康
            morale = 100f;             // 初始时满士气
        }
        
        /// <summary>
        /// (私有辅助方法) 初始化幸存者的基础属性值。
        /// </summary>
        private void InitializeBasicStats()
        {
            maxHealth = 100f;
            attack = 10f;
            defense = 5f;
            speed = 2f;
            experience = 0;
            skillPoints = 0;
        }
        
        /// <summary>
        /// (私有辅助方法) 初始化幸存者的所有技能等级为1，并应用其职业带来的初始技能加成。
        /// </summary>
        private void InitializeSkills()
        {
            // 将所有定义的技能类型 (SurvivorSkill) 的初始等级设为1
            foreach (SurvivorSkill skill in Enum.GetValues(typeof(SurvivorSkill)))
            {
                skills[skill] = 1;
            }
            
            // 根据幸存者的职业 (profession) 应用特定的技能点数或属性加成
            ApplyProfessionBonuses();
        }
        
        /// <summary>
        /// (公有方法，可能是用于后续初始化或重置)
        /// 如果幸存者姓名为空，则生成一个基于时间的默认名称。
        /// 更新上次行动时间为当前时间。
        /// </summary>
        public void Initialize()
        {
            if (string.IsNullOrEmpty(name))
            {
                // 生成一个基于当前时间（取整）的临时名称
                name = "Survivor_" + ((int)Time.time).ToString();
            }
            lastActionTime = Time.time; // 更新上次行动时间
        }
        
        /// <summary>
        /// (私有辅助方法) 根据幸存者的职业 (profession) 应用初始的技能等级和属性加成。
        /// </summary>
        private void ApplyProfessionBonuses()
        {
            switch (profession)
            {
                case SurvivorProfession.Soldier:   // 士兵职业加成
                    skills[SurvivorSkill.Combat] = 3;       // 提升战斗技能等级
                    skills[SurvivorSkill.Leadership] = 2;   // 提升领导技能等级
                    attack += 5f;                           // 增加攻击力
                    defense += 3f;                          // 增加防御力
                    break;
                    
                case SurvivorProfession.Engineer:  // 工程师职业加成
                    skills[SurvivorSkill.Construction] = 3; // 提升建造技能
                    skills[SurvivorSkill.Repair] = 3;       // 提升修理技能
                    break;
                    
                case SurvivorProfession.Doctor:    // 医生职业加成
                    skills[SurvivorSkill.Medicine] = 3;     // 提升医疗技能
                    maxHealth += 20f;                       // 增加最大健康值
                    break;
                    
                case SurvivorProfession.Scientist: // 科学家职业加成
                    skills[SurvivorSkill.Research] = 3;     // 提升研究技能
                    break;
                    
                case SurvivorProfession.Scout:     // 侦察员职业加成
                    skills[SurvivorSkill.Stealth] = 3;      // 提升潜行技能
                    skills[SurvivorSkill.Gathering] = 2;    // 提升采集技能
                    speed += 1f;                            // 增加移动速度
                    break;
                    
                case SurvivorProfession.Worker:    // 工人职业加成
                    skills[SurvivorSkill.Gathering] = 2;    // 提升采集技能
                    skills[SurvivorSkill.Construction] = 2; // 提升建造技能
                    break;
                    
                case SurvivorProfession.Guard:     // 守卫职业加成
                    skills[SurvivorSkill.Combat] = 2;       // 提升战斗技能
                    defense += 3f;                          // 增加防御力
                    break;
                // 平民 (Civilian) 没有特定的初始加成，保持默认技能等级
            }
        }
        
        /// <summary>
        /// (示例方法) 为幸存者生成一个随机的测试名称。
        /// </summary>
        public void GenerateRandomName()
        {
            // 简单地将 "RandomName" 与一个0到999的随机数拼接
            name = "RandomName" + UnityEngine.Random.Range(0, 1000).ToString();
        }
        
        /// <summary>
        /// (静态工厂方法) 创建一个具有指定职业和位置的 LegacySurvivorData 实例。
        /// </summary>
        /// <param name="profession">要创建的幸存者的职业。</param>
        /// <param name="position">幸存者的初始位置。</param>
        /// <returns>新创建的 LegacySurvivorData 实例。</returns>
        public static LegacySurvivorData CreateSurvivor(SurvivorProfession profession, Vector3 position)
        {
            var survivor = new LegacySurvivorData
            {
                profession = profession,     // 设置职业
                position = position,         // 设置初始位置
                targetPosition = position    // 初始目标位置与当前位置相同
            };
            
            // 注意：在构造函数中已经调用了ApplyProfessionBonuses。
            // 如果构造函数中的profession字段在调用ApplyProfessionBonuses时尚未被正确设置（例如，如果profession是后赋值的），
            // 则此处再次调用是必要的，以确保职业加成基于正确的职业。
            // 但如果构造函数能保证顺序，则此处的再次调用可能冗余。
            survivor.ApplyProfessionBonuses(); // 确保职业加成被应用
            
            return survivor;
        }
        
        /// <summary>
        /// (只读属性) 判断幸存者当前是否存活。
        /// 条件：状态不是死亡 (Dead) 且当前健康值大于0。
        /// </summary>
        public bool IsAlive => state != SurvivorState.Dead && currentHealth > 0;
        
        /// <summary>
        /// (只读属性) 判断幸存者当前是否可以工作。
        /// 条件：存活、状态不是受伤 (Injured) 且士气值大于20。
        /// </summary>
        public bool CanWork => IsAlive && state != SurvivorState.Injured && morale > 20f;
        
        /// <summary>
        /// (只读属性) 判断幸存者当前是否可以战斗。
        /// 条件：存活、状态不是受伤 (Injured) 且当前健康值大于最大健康值的30%。
        /// </summary>
        public bool CanFight => IsAlive && state != SurvivorState.Injured && currentHealth > maxHealth * 0.3f;
        
        /// <summary>
        /// 使幸存者受到指定量的伤害。
        /// 会同时降低健康值和士气值。如果健康值降为0或以下，状态设为死亡；
        /// 如果健康值低于最大值的50%，状态设为受伤。
        /// </summary>
        /// <param name="damage">受到的伤害量 (应为正数)。</param>
        public void TakeDamage(float damage)
        {
            if(damage <= 0) return; // 不处理非正数伤害

            currentHealth = Mathf.Max(0, currentHealth - damage); // 减少健康值，不低于0
            morale = Mathf.Max(0, morale - damage * 0.1f); // 伤害也轻微影响士气 (影响系数0.1可调整)
            
            // 根据受伤害后的健康状况更新状态
            if (currentHealth <= 0)
            {
                state = SurvivorState.Dead; // 健康耗尽则死亡
            }
            else if (currentHealth < maxHealth * 0.5f && state != SurvivorState.Dead) // 健康低于一半但还活着
            {
                state = SurvivorState.Injured; // 设为受伤状态
            }
        }
        
        /// <summary>
        /// 为幸存者恢复指定量的健康值。
        /// 如果之前是受伤状态且健康恢复到一定阈值以上，状态可能变回空闲。
        /// </summary>
        /// <param name="amount">要恢复的健康量 (应为正数)。</param>
        public void Heal(float amount)
        {
            if(amount <= 0 || state == SurvivorState.Dead) return; // 不治疗非正数或已死亡者

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount); // 增加健康，但不超过最大值
            
            // 如果之前是受伤状态，并且健康恢复到最大值的50%以上
            if (state == SurvivorState.Injured && currentHealth > maxHealth * 0.5f)
            {
                state = SurvivorState.Idle; // 状态恢复为空闲 (如果之前没有其他任务)
            }
        }
        
        /// <summary>
        /// 为幸存者增加指定量的经验值。
        /// 如果经验值达到升级所需，则调用 LevelUp 方法。
        /// </summary>
        /// <param name="exp">要增加的经验值数量 (应为正数)。</param>
        public void AddExperience(int exp)
        {
            if(exp <= 0) return;

            experience += exp;
            
            // 检查是否满足升级条件 (升级所需经验 = 当前等级 * 100)
            int requiredExp = level * 100; // 示例升级曲线
            if (experience >= requiredExp)
            {
                LevelUp(); // 调用升级方法
            }
        }
        
        /// <summary>
        /// (私有辅助方法) 执行幸存者升级逻辑。
        /// 提升等级、重置经验、增加技能点、提升基础属性、恢复健康和士气。
        /// </summary>
        private void LevelUp()
        {
            level++;                 // 等级提升
            experience = 0;          // 当前等级经验清零 (或 experience -= requiredExp)
            skillPoints += 2;        // 获得2个技能点 (示例)
            
            // 升级时提升一些基础属性
            maxHealth += 10f;
            attack += 2f;
            defense += 1f;
            
            // 升级时完全恢复健康并增加一些士气
            currentHealth = maxHealth;
            morale = Mathf.Min(100f, morale + 20f); // 士气增加20，但不超过100
            
            Debug.Log($"[LegacySurvivorData] 幸存者 {name} 已升级到 {level} 级!");
        }
        
        /// <summary>
        /// 消耗技能点来提升指定的技能等级。
        /// </summary>
        /// <param name="skill">要提升的技能类型。</param>
        /// <returns>如果技能提升成功（有足够技能点），则返回true；否则返回false。</returns>
        public bool UpgradeSkill(SurvivorSkill skill)
        {
            if (skillPoints <= 0) return false; // 没有可用技能点则失败
            
            // 假设技能字典中已包含所有技能类型
            if (skills.ContainsKey(skill))
            {
                skills[skill]++; // 技能等级加1
            }
            else
            {
                skills[skill] = 1; // 如果技能不存在则初始化为1 (通常应在InitializeSkills中完成)
            }
            skillPoints--; // 消耗一个技能点
            
            Debug.Log($"[LegacySurvivorData] 幸存者 {name} 的 {skill} 技能已提升到 {skills[skill]} 级。剩余技能点: {skillPoints}");
            return true;
        }
        
        /// <summary>
        /// 获取指定技能的当前等级。
        /// </summary>
        /// <param name="skill">要查询的技能类型。</param>
        /// <returns>技能的当前等级；如果技能未在字典中定义，则返回1（作为默认基础等级）。</returns>
        public int GetSkillLevel(SurvivorSkill skill)
        {
            return skills.ContainsKey(skill) ? skills[skill] : 1;
        }
        
        /// <summary>
        /// 计算并获取幸存者当前的工作效率。
        /// 效率受士气、健康状况和等级等多种因素影响。
        /// </summary>
        /// <returns>综合工作效率的乘数因子。</returns>
        public float GetWorkEfficiency()
        {
            float baseEfficiency = 1f; // 基础效率100%
            
            // 士气对效率的影响 (0%-100%)
            float moraleBonus = morale / 100f;
            // 健康状况对效率的影响 (0%-100%)
            float healthBonus = currentHealth / maxHealth;
            // 等级对效率的影响 (每级提升10%效率，示例)
            float levelBonus = 1f + (level - 1) * 0.1f;
            
            // 综合效率 = 基础效率 * 各项修正因子
            return baseEfficiency * moraleBonus * healthBonus * levelBonus;
        }
        
        /// <summary>
        /// 计算并获取幸存者当前的综合战斗力。
        /// 战斗力受基础攻击力、战斗技能等级、健康和士气等因素影响。
        /// </summary>
        /// <returns>综合战斗力数值。</returns>
        public float GetCombatPower()
        {
            // 基础战斗力 = 攻击力 * 战斗技能等级 (技能等级作为乘数)
            float basePower = attack * GetSkillLevel(SurvivorSkill.Combat);
            // 健康修正 (满血为1，空血为0)
            float healthModifier = currentHealth / maxHealth;
            // 士气修正 (满士气为1，零士气为0)
            float moraleModifier = morale / 100f;
            
            // 综合战斗力 = 基础战斗力 * 健康修正 * 士气修正
            return basePower * healthModifier * moraleModifier;
        }
    }

    
    
    /// <summary>
    /// 幸存者系统数据
    /// </summary>
    [Serializable]
    public class SurvivorSystemData
    {
        // 幸存者管理
        public Dictionary<string, SurvivorData> survivors;
        public int maxSurvivors;
        public int totalSurvivorsRecruited;
        
        // 工作分配
        public Dictionary<string, List<string>> buildingWorkers; // 建筑ID -> 幸存者ID列表
        public Dictionary<SurvivorProfession, int> professionCounts;
        
        // 系统状态
        public float lastRecruitTime;
        public float nextRecruitTime;
        public float recruitCooldown;
        
        // 团队士气
        public float teamMorale;
        public int totalDeaths;
        
        public SurvivorSystemData()
        {
            survivors = new Dictionary<string, SurvivorData>();
            buildingWorkers = new Dictionary<string, List<string>>();
            professionCounts = new Dictionary<SurvivorProfession, int>();
            
            maxSurvivors = 20;
            totalSurvivorsRecruited = 0;
            totalDeaths = 0;
            teamMorale = 100f;
            recruitCooldown = 300f; // 5分钟招募冷却
            
            // 初始化职业计数
            foreach (SurvivorProfession profession in Enum.GetValues(typeof(SurvivorProfession)))
            {
                professionCounts[profession] = 0;
            }
        }
        
        /// <summary>
        /// 获取存活幸存者数量
        /// </summary>
        public int GetAliveSurvivorCount()
        {
            int count = 0;
            foreach (var survivor in survivors.Values)
            {
                if (survivor.IsAlive)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// 获取可工作幸存者数量
        /// </summary>
        public int GetAvailableWorkerCount()
        {
            int count = 0;
            foreach (var survivor in survivors.Values)
            {
                if (survivor.CanWork && survivor.CurrentJob == SurvivorJob.Idle)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// 获取可战斗幸存者数量
        /// </summary>
        public int GetCombatReadySurvivorCount()
        {
            int count = 0;
            foreach (var survivor in survivors.Values)
            {
                if (survivor.CanFight)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// 更新团队士气
        /// </summary>
        public void UpdateTeamMorale()
        {
            if (survivors.Count == 0)
            {
                teamMorale = 100f;
                return;
            }
            
            float totalMorale = 0f;
            int aliveCount = 0;
            
            foreach (var survivor in survivors.Values)
            {
                if (survivor.IsAlive)
                {
                    totalMorale += survivor.Morale;
                    aliveCount++;
                }
            }
            
            teamMorale = aliveCount > 0 ? totalMorale / aliveCount : 0f;
        }
    }


} 