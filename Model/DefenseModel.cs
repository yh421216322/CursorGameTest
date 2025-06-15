// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：DefenseModel.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了与游戏防御系统相关的各种数据模型和枚举类型。
//     包括防御塔、投射物、防御工事、陷阱、防御区域、防御策略、
//     防御任务以及防御事件等的数据结构。这些是构建游戏防御玩法的基础。
// ==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine; // 用于Vector3等Unity特定类型
using QFramework;  // QFramework框架 (当前在此文件中未直接使用其特性，但可能用于项目中其他部分)

namespace SurvivalGame.Model
{
    /// <summary>
    /// 防御塔的动态数据模型。
    /// 存储一个防御塔实例在游戏运行时的所有状态和属性。
    /// </summary>
    [System.Serializable] // 标记为可序列化，以便能在Unity Inspector中显示或保存
    public class TowerData
    {
        public int id;                  // 防御塔的唯一标识符 (统一使用小写以符合新的命名约定)
        public TowerType type;          // 防御塔的类型 (参考TowerType枚举)
        public Vector3 position;        // 防御塔在世界空间中的位置
        public int level;               // 防御塔的当前等级
        public float currentHealth;     // 防御塔当前的生命值
        public float maxHealth;         // 防御塔的最大生命值
        public bool isActive;           // 防御塔当前是否激活并可运作
        public float lastFireTime;      // 上一次开火的时间戳 (例如 Time.time)
        public float timeSinceLastFire; // 自上次开火以来经过的时间（秒），可用于计算射速冷却
        public ZombieData currentTarget; // 当前攻击的僵尸目标 (ZombieData类型)
        public int totalKills;          // 此塔累计的击杀数
        public float totalDamageDealt;  // 此塔累计造成的总伤害
        public float buildTime;         // 建造或升级此塔所需的时间
        public bool isUnderConstruction; // 标记此塔当前是否正在建造中
        public List<int> assignedWorkers; // 分配到此塔工作的工人ID列表 (如果塔需要工人操作)
        public float upgradeProgress;   // 当前升级进度 (0到1)
        public bool isUpgrading;        // 标记此塔当前是否正在升级中
        
        // 为防御系统专门添加的战斗相关属性
        public float damage;            // 防御塔的单次攻击伤害
        public float range;             // 防御塔的攻击射程
        public float fireRate;          // 防御塔的射速 (例如：次/秒)
        public float projectileSpeed;   // 防御塔发射的投射物的速度
        public int totalShotsFired;     // 此塔累计发射的投射物数量
        
        // --- 为了兼容旧代码或提供更符合C#命名规范的属性访问器 ---
        // 这些属性是对小写公共字段的简单包装。
        public int Id { get => id; set => id = value; }
        public TowerType Type { get => type; set => type = value; }
        public Vector3 Position { get => position; set => position = value; }
        public int Level { get => level; set => level = value; }
        public float Health { get => currentHealth; set => currentHealth = value; } // Health属性映射到currentHealth
        public float MaxHealth { get => maxHealth; set => maxHealth = value; }
        public bool IsOperational { get => isActive; set => isActive = value; } // IsOperational属性映射到isActive
        public float LastFireTime { get => lastFireTime; set => lastFireTime = value; }
        public ZombieData CurrentTarget { get => currentTarget; set => currentTarget = value; }
        public int TotalKills { get => totalKills; set => totalKills = value; }
        public float TotalDamageDealt { get => totalDamageDealt; set => totalDamageDealt = value; }
        public float BuildTime { get => buildTime; set => buildTime = value; }
        public bool IsUnderConstruction { get => isUnderConstruction; set => isUnderConstruction = value; }
        public List<int> AssignedWorkers { get => assignedWorkers; set => assignedWorkers = value; }
        public float UpgradeProgress { get => upgradeProgress; set => upgradeProgress = value; }
        public bool IsUpgrading { get => isUpgrading; set => isUpgrading = value; }
        
        /// <summary>
        /// TowerData的构造函数。
        /// 初始化列表和一些默认值。
        /// </summary>
        public TowerData()
        {
            assignedWorkers = new List<int>(); // 初始化工人列表
            isActive = true;                   // 默认激活
            level = 1;                         // 默认等级为1
            totalShotsFired = 0;               // 初始射击次数为0
            timeSinceLastFire = 0f;            // 初始上次开火后经过时间为0
        }
        
        /// <summary>
        /// 获取当前生命值相对于最大生命值的比例 (0到1)。
        /// </summary>
        /// <returns>生命值比例；如果最大生命值为0或负，则返回0。</returns>
        public float GetHealthRatio()
        {
            return maxHealth > 0 ? currentHealth / maxHealth : 0f;
        }
        
        /// <summary>
        /// 判断防御塔是否需要修理 (例如，生命值低于80%)。
        /// </summary>
        /// <returns>如果需要修理则为true，否则为false。</returns>
        public bool NeedsRepair()
        {
            return currentHealth < maxHealth * 0.8f; // 阈值0.8可调整
        }
        
        /// <summary>
        /// 判断防御塔是否已被摧毁 (生命值小于或等于0)。
        /// </summary>
        /// <returns>如果已摧毁则为true，否则为false。</returns>
        public bool IsDestroyed()
        {
            return currentHealth <= 0f;
        }
    }

    /// <summary>
    /// 投射物（如子弹、炮弹等）的数据模型。
    /// </summary>
    [System.Serializable]
    public class ProjectileData
    {
        public int id;                    // 投射物的唯一ID
        public ProjectileType type;       // 投射物类型 (参考ProjectileType枚举)
        public Vector3 position;          // 当前位置
        public Vector3 targetPosition;    // 目标位置 (对于追踪型投射物，此项可能动态更新)
        public float speed;               // 飞行速度
        public float damage;              // 造成的伤害
        public float lifeTime;            // 剩余生命周期（秒），到期后消失
        public int towerId;               // 发射此投射物的防御塔ID
        public string targetZombieId;     // 目标僵尸的ID (使用字符串以兼容可能的GUID等格式)
        public bool hasExploded;          // 是否已爆炸 (针对爆炸型投射物)
        public float explosionRadius;     // 爆炸半径
        public List<string> statusEffects; // 携带的状态效果ID列表 (例如：减速、中毒)
        public float creationTime;        // 投射物的创建时间戳 (Time.time)
        
        // --- 兼容性属性映射 (同TowerData中的原因) ---
        public int Id { get => id; set => id = value; }
        public ProjectileType Type { get => type; set => type = value; }
        public Vector3 Position { get => position; set => position = value; }
        public Vector3 TargetPosition { get => targetPosition; set => targetPosition = value; }
        public float Speed { get => speed; set => speed = value; }
        public float Damage { get => damage; set => damage = value; }
        public float LifeTime { get => lifeTime; set => lifeTime = value; }
        public int SourceTowerId { get => towerId; set => towerId = value; } // 属性名更清晰
        // TargetZombieId 在新旧代码中类型不同 (int vs string)，这里做简单转换处理。
        // ?? "0" 确保如果targetZombieId为null时，int.Parse不会出错。
        public int TargetZombieId { get => int.Parse(targetZombieId ?? "0"); set => targetZombieId = value.ToString(); }
        public bool HasExploded { get => hasExploded; set => hasExploded = value; }
        public float ExplosionRadius { get => explosionRadius; set => explosionRadius = value; }
        public List<string> StatusEffects { get => statusEffects; set => statusEffects = value; }
        
        /// <summary>
        /// ProjectileData的构造函数。
        /// 初始化列表和默认值。
        /// </summary>
        public ProjectileData()
        {
            statusEffects = new List<string>();
            speed = 10f;        // 默认速度
            lifeTime = 5f;      // 默认5秒后消失
            creationTime = UnityEngine.Time.time; // 记录创建时间
            targetZombieId = "0"; // 默认目标ID为"0"或无效值
        }
        
        /// <summary>
        /// 判断投射物是否已过期 (生命周期结束)。
        /// </summary>
        public bool IsExpired()
        {
            return lifeTime <= 0f;
        }
        
        /// <summary>
        /// 获取当前位置到目标位置的距离。
        /// </summary>
        public float GetDistanceToTarget()
        {
            return Vector3.Distance(position, targetPosition);
        }
    }

    /// <summary>
    /// 防御工事（如墙壁、路障）的数据模型。
    /// </summary>
    [System.Serializable]
    public class FortificationData
    {
        public int Id;                        // 防御工事的唯一ID
        public FortificationType Type;        // 防御工事类型 (参考FortificationType枚举)
        public Vector3 Position;              // 位置
        public float Health;                  // 当前耐久度
        public float MaxHealth;               // 最大耐久度
        public bool IsDestroyed;              // 是否已被摧毁
        public int RepairCost;                // 修复所需资源成本 (可能是一个概括值或特定资源ID)
        public float ArmorValue;              // 护甲值，用于减免伤害
        public List<Vector3> ConnectedPositions; // 与其他防御工事连接的点位列表 (用于构建连续防线)
        
        public FortificationData()
        {
            ConnectedPositions = new List<Vector3>(); // 初始化连接点列表
        }
        
        /// <summary>
        /// 计算基于护甲值的伤害减免百分比。
        /// 公式示例：减免 = 护甲 / (护甲 + 常数)
        /// </summary>
        /// <returns>伤害减免比例 (0到1)。</returns>
        public float GetArmorReduction()
        {
            // 示例减伤公式，100是一个常数，可调整以平衡游戏
            return ArmorValue / (ArmorValue + 100f);
        }
    }

    /// <summary>
    /// 防御陷阱的数据模型。
    /// </summary>
    [System.Serializable]
    public class TrapData
    {
        public int Id;                    // 陷阱的唯一ID
        public TrapType Type;             // 陷阱类型 (参考TrapType枚举)
        public Vector3 Position;          // 位置
        public bool IsActive;             // 陷阱当前是否激活可用
        public bool IsTriggered;          // 陷阱是否已被触发（可能需要重置）
        public float Damage;              // 触发时造成的伤害
        public float EffectRadius;        // 影响范围半径
        public int MaxTriggers;           // 最大可触发次数 (-1可能表示无限次，或根据类型定)
        public int TriggersLeft;          // 剩余可触发次数
        public float RearmTime;           // 触发后自动重置所需时间（秒），0表示不自动重置
        public float LastTriggerTime;     // 上次触发的时间戳
        public List<string> EffectTypes;  // 陷阱附带的特殊效果类型列表 (例如：减速、流血)
        
        public TrapData()
        {
            EffectTypes = new List<string>(); // 初始化效果列表
            IsActive = true;                  // 默认激活
            MaxTriggers = 1;                  // 默认可触发1次
            TriggersLeft = 1;                 // 默认剩余1次
        }
        
        /// <summary>
        /// 判断陷阱当前是否可以被触发。
        /// </summary>
        public bool CanTrigger()
        {
            return IsActive && !IsTriggered && TriggersLeft > 0;
        }
        
        /// <summary>
        /// 判断陷阱是否在触发后达到了可以重置的时间。
        /// </summary>
        public bool NeedsRearm()
        {
            // 如果陷阱已被触发，并且已过重置时间 (RearmTime > 0)
            return IsTriggered && RearmTime > 0 && (Time.time - LastTriggerTime >= RearmTime);
        }
    }

    /// <summary>
    /// 防御区域的数据模型。用于定义地图上的特定防守区域。
    /// </summary>
    [System.Serializable]
    public class DefenseZoneData
    {
        public int Id;                          // 防御区域的唯一ID
        public string Name;                     // 防御区域的名称 (例如："东门防线", "基地核心区")
        public Vector3 Center;                  // 区域的中心点坐标
        public float Radius;                    // 区域的半径 (假设为圆形区域)
        public DefenseZoneType Type;            // 防御区域类型 (参考DefenseZoneType枚举)
        public List<int> TowerIds;              // 此区域内部署的防御塔ID列表
        public List<int> TrapIds;               // 此区域内部署的陷阱ID列表
        public List<int> FortificationIds;      // 此区域内部署的防御工事ID列表
        public bool IsActive;                   // 此防御区域当前是否启用
        public float ThreatLevel;               // 此区域当前评估的威胁等级
        public int Priority;                    // 防御优先级 (例如，数值越高越优先分配资源或火力)
        
        public DefenseZoneData()
        {
            TowerIds = new List<int>();
            TrapIds = new List<int>();
            FortificationIds = new List<int>();
            IsActive = true;    // 默认激活
            Priority = 1;       // 默认优先级
        }
        
        /// <summary>
        /// 判断给定位置是否在此防御区域内。
        /// </summary>
        /// <param name="position">要检查的世界坐标位置。</param>
        /// <returns>如果在区域内则为true，否则为false。</returns>
        public bool ContainsPosition(Vector3 position)
        {
            return Vector3.Distance(Center, position) <= Radius;
        }
    }

    /// <summary>
    /// 防御策略的数据模型。用于定义和配置不同的防御AI行为或玩家可选策略。
    /// </summary>
    [System.Serializable]
    public class DefenseStrategyData
    {
        public string Id;                 // 策略的唯一ID
        public string Name;               // 策略名称
        public string Description;        // 策略描述
        public DefenseStrategyType Type;  // 策略类型 (参考DefenseStrategyType枚举)
        public Dictionary<string, float> Parameters; // 策略相关的参数配置 (例如：攻击偏好、资源分配比例等)
        public bool IsActive;             // 此策略当前是否被激活使用
        public float Effectiveness;      // 策略的当前有效性评估 (可能动态变化)
        public List<string> RequiredTechs; // 启用此策略所需的前置科技ID列表
        
        public DefenseStrategyData()
        {
            Parameters = new Dictionary<string, float>();
            RequiredTechs = new List<string>();
            Effectiveness = 1f; // 默认有效性为100%
        }
    }

    /// <summary>
    /// 防御相关的任务数据模型（例如：建造防御塔、修理墙壁）。
    /// </summary>
    [System.Serializable]
    public class DefenseTaskData
    {
        public int Id;                    // 任务的唯一ID
        public DefenseTaskType Type;      // 任务类型 (参考DefenseTaskType枚举)
        public Vector3 Location;          // 任务执行的地点
        public int Priority;              // 任务优先级
        public bool IsCompleted;          // 任务是否已完成
        public bool IsAssigned;           // 任务是否已分配给某个单位或幸存者
        public int AssignedWorkerId;      // 被分配执行此任务的工人/单位ID (如果适用)
        public float Progress;            // 任务完成进度 (通常0到100)
        public float EstimatedTime;       // 预计完成任务所需时间（秒）
        public Dictionary<string, int> RequiredResources; // 完成任务所需的资源及其数量
        
        public DefenseTaskData()
        {
            RequiredResources = new Dictionary<string, int>();
            Priority = 1; // 默认优先级
        }
        
        /// <summary>
        /// 获取任务完成的比例 (0到1)。
        /// </summary>
        public float GetCompletionRatio()
        {
            return Progress / 100f; // 假设Progress是0-100范围
        }
    }

    /// <summary>
    /// 防御相关的事件记录数据模型。
    /// </summary>
    [System.Serializable]
    public class DefenseEventData
    {
        public int Id;                       // 事件的唯一ID
        public DefenseEventType Type;        // 事件类型 (参考DefenseEventType枚举)
        public Vector3 Location;             // 事件发生的地点
        public float Timestamp;              // 事件发生的时间戳 (Time.time)
        public string Description;           // 事件的文字描述
        public int Severity;                 // 事件的严重程度 (例如 1-5)
        public bool IsResolved;              // 事件是否已被处理或解决
        public Dictionary<string, object> EventData; // 与事件相关的额外数据 (灵活存储)
        
        public DefenseEventData()
        {
            EventData = new Dictionary<string, object>();
            Timestamp = Time.time; // 记录创建时间
        }
        
        /// <summary>
        /// 获取事件发生至今所经过的时间（秒）。
        /// </summary>
        public float GetTimeElapsed()
        {
            return Time.time - Timestamp;
        }
    }

    // --- 以下为防御系统相关的枚举定义 ---

    /// <summary>
    /// 防御塔的类型枚举。
    /// </summary>
    public enum TowerType
    {
        Basic,          // 基础型防御塔 (例如：箭塔、小型机枪塔)
        Heavy,          // 重型防御塔 (例如：加农炮塔、重机枪)
        Sniper,         // 狙击型防御塔 (远程、高单伤、低射速)
        Splash,         // 范围溅射型防御塔 (例如：迫击炮、榴弹发射器)
        MachineGun,     // 速射机枪塔
        Cannon,         // 加农炮塔
        Flamethrower,   // 火焰喷射塔 (持续范围伤害)
        Mortar,         // 迫击炮塔 (曲线攻击、范围伤害)
        Laser,          // 激光塔 (持续直线伤害或高精度单点)
        Tesla,          // 特斯拉电磁塔 (链式或范围闪电伤害)
        Freeze,         // 冰冻塔 (减速或冰冻效果)
        Missile,        // 导弹发射塔 (高伤害、可能追踪)
        Plasma          // 等离子塔 (高科技、特殊效果伤害)
    }

    /// <summary>
    /// 投射物的类型枚举。
    /// </summary>
    public enum ProjectileType
    {
        Bullet,         // 普通子弹 (例如：用于机枪塔、基础塔)
        Shell,          // 炮弹 (例如：用于加农炮塔)
        Flame,          // 火焰粒子或团块 (用于火焰塔)
        SniperBullet,   // 狙击子弹 (高精度、高伤害)
        Grenade,        // 榴弹 (用于迫击炮或榴弹发射器，通常有范围效果)
        Laser,          // 激光束
        Lightning,      // 闪电链或球状闪电
        Ice,            // 冰霜射弹或冰冻效果区域
        Missile,        // 导弹
        Plasma          // 等离子团或光束
    }

    /// <summary>
    /// 防御工事的类型枚举。
    /// </summary>
    public enum FortificationType
    {
        Wall,           // 墙壁 (基础阻挡单位)
        Barrier,        // 路障 (临时性或可被摧毁的障碍)
        Sandbag,        // 沙袋掩体 (提供掩护)
        Trench,         // 战壕 (提供掩护和减速)
        Bunker,         // 地堡 (坚固的防御点)
        Gate,           // 大门 (可开关的墙体部分)
        Fence           // 栅栏 (较低的阻挡或减速效果)
    }

    /// <summary>
    /// 陷阱的类型枚举。
    /// </summary>
    public enum TrapType
    {
        Spike,          // 尖刺陷阱 (地面固定伤害)
        Explosive,      // 爆炸陷阱 (例如：地雷)
        Electric,       // 电击陷阱 (造成伤害和/或麻痹)
        Poison,         // 毒气陷阱 (持续伤害或Debuff)
        Slow,           // 减速陷阱 (例如：粘性物质、力场)
        Alarm,          // 警报陷阱 (触发时发出警报，不一定有伤害)
        Pit             // 陷坑 (使单位坠落或受困)
    }

    /// <summary>
    /// 防御区域的类型枚举。
    /// </summary>
    public enum DefenseZoneType
    {
        Perimeter,      // 外围防线 (最外层防御)
        Checkpoint,     // 检查点/隘口 (关键通道的防御点)
        Stronghold,     // 据点/要塞 (核心防御区域)
        Fallback,       // 后备防线 (当外层被突破时的第二道防线)
        Critical        // 核心保护区 (例如：基地中心、重要设施区)
    }

    /// <summary>
    /// 防御策略的类型枚举。
    /// </summary>
    public enum DefenseStrategyType
    {
        Aggressive,     // 激进型 (例如：主动出击、火力覆盖优先)
        Defensive,      // 防守型 (例如：固守待援、强调生存和修复)
        Balanced,       // 平衡型 (攻守兼备)
        Economic,       // 经济型 (例如：优先保护资源点、低成本防御)
        Specialized     // 专门化 (例如：针对特定类型敌人或特定阶段的策略)
    }

    /// <summary>
    /// 防御相关任务的类型枚举。
    /// </summary>
    public enum DefenseTaskType
    {
        BuildTower,     // 建造新的防御塔
        RepairTower,    // 修理已损坏的防御塔
        UpgradeTower,   // 升级现有防御塔
        BuildWall,      // 建造墙壁或其他防御工事
        RepairWall,     // 修理防御工事
        SetTrap,        // 布置陷阱
        Patrol,         // 在指定区域巡逻
        Reinforce       // 增援特定区域 (例如：派遣单位或激活特殊能力)
    }

    /// <summary>
    /// 防御相关的事件类型枚举。
    /// </summary>
    public enum DefenseEventType
    {
        TowerDestroyed,     // 防御塔被摧毁
        WallBreached,       // 防御工事（如墙壁）被攻破
        TrapTriggered,      // 陷阱被触发
        EnemySpotted,       // 在特定区域侦测到敌人
        AmmoLow,            // 防御塔弹药不足警告
        PowerFailure,       // 防御设施电力中断 (如果依赖电力)
        RepairNeeded        // 有防御设施耐久度过低，需要维修
    }
}