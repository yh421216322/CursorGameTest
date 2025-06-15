using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 防御塔数据模型
    /// </summary>
    [System.Serializable]
    public class TowerData
    {
        public int id;  // 统一使用小写命名
        public TowerType type;
        public Vector3 position;
        public int level;
        public float currentHealth;  // 当前血量
        public float maxHealth;      // 最大血量
        public bool isActive;        // 是否激活
        public float lastFireTime;   // 上次开火时间
        public float timeSinceLastFire; // 自上次开火经过的时间
        public ZombieData currentTarget; // 当前目标
        public int totalKills;
        public float totalDamageDealt;
        public float buildTime;
        public bool isUnderConstruction;
        public List<int> assignedWorkers;
        public float upgradeProgress;
        public bool isUpgrading;
        
        // 防御系统需要的属性
        public float damage;         // 伤害
        public float range;          // 射程
        public float fireRate;       // 射速
        public float projectileSpeed; // 投射物速度
        public int totalShotsFired;  // 总射击次数
        
        // 原有属性的兼容性映射
        public int Id { get => id; set => id = value; }
        public TowerType Type { get => type; set => type = value; }
        public Vector3 Position { get => position; set => position = value; }
        public int Level { get => level; set => level = value; }
        public float Health { get => currentHealth; set => currentHealth = value; }
        public float MaxHealth { get => maxHealth; set => maxHealth = value; }
        public bool IsOperational { get => isActive; set => isActive = value; }
        public float LastFireTime { get => lastFireTime; set => lastFireTime = value; }
        public ZombieData CurrentTarget { get => currentTarget; set => currentTarget = value; }
        public int TotalKills { get => totalKills; set => totalKills = value; }
        public float TotalDamageDealt { get => totalDamageDealt; set => totalDamageDealt = value; }
        public float BuildTime { get => buildTime; set => buildTime = value; }
        public bool IsUnderConstruction { get => isUnderConstruction; set => isUnderConstruction = value; }
        public List<int> AssignedWorkers { get => assignedWorkers; set => assignedWorkers = value; }
        public float UpgradeProgress { get => upgradeProgress; set => upgradeProgress = value; }
        public bool IsUpgrading { get => isUpgrading; set => isUpgrading = value; }
        
        public TowerData()
        {
            assignedWorkers = new List<int>();
            isActive = true;
            level = 1;
            totalShotsFired = 0;
            timeSinceLastFire = 0f;
        }
        
        public float GetHealthRatio()
        {
            return MaxHealth > 0 ? Health / MaxHealth : 0f;
        }
        
        public bool NeedsRepair()
        {
            return Health < MaxHealth * 0.8f;
        }
        
        public bool IsDestroyed()
        {
            return Health <= 0f;
        }
    }

    /// <summary>
    /// 投射物数据模型
    /// </summary>
    [System.Serializable]
    public class ProjectileData
    {
        public int id;
        public ProjectileType type;
        public Vector3 position;
        public Vector3 targetPosition;
        public float speed;
        public float damage;
        public float lifeTime;
        public int towerId;  // 源防御塔ID
        public string targetZombieId;  // 目标僵尸ID (字符串类型)
        public bool hasExploded;
        public float explosionRadius;
        public List<string> statusEffects;
        public float creationTime;  // 创建时间
        
        // 兼容性映射
        public int Id { get => id; set => id = value; }
        public ProjectileType Type { get => type; set => type = value; }
        public Vector3 Position { get => position; set => position = value; }
        public Vector3 TargetPosition { get => targetPosition; set => targetPosition = value; }
        public float Speed { get => speed; set => speed = value; }
        public float Damage { get => damage; set => damage = value; }
        public float LifeTime { get => lifeTime; set => lifeTime = value; }
        public int SourceTowerId { get => towerId; set => towerId = value; }
        public int TargetZombieId { get => int.Parse(targetZombieId ?? "0"); set => targetZombieId = value.ToString(); }
        public bool HasExploded { get => hasExploded; set => hasExploded = value; }
        public float ExplosionRadius { get => explosionRadius; set => explosionRadius = value; }
        public List<string> StatusEffects { get => statusEffects; set => statusEffects = value; }
        
        public ProjectileData()
        {
            statusEffects = new List<string>();
            speed = 10f;
            lifeTime = 5f;
            creationTime = UnityEngine.Time.time;
            targetZombieId = "0";
        }
        
        public bool IsExpired()
        {
            return LifeTime <= 0f;
        }
        
        public float GetDistanceToTarget()
        {
            return Vector3.Distance(Position, TargetPosition);
        }
    }

    /// <summary>
    /// 防御工事数据
    /// </summary>
    [System.Serializable]
    public class FortificationData
    {
        public int Id;
        public FortificationType Type;
        public Vector3 Position;
        public float Health;
        public float MaxHealth;
        public bool IsDestroyed;
        public int RepairCost;
        public float ArmorValue;
        public List<Vector3> ConnectedPositions;
        
        public FortificationData()
        {
            ConnectedPositions = new List<Vector3>();
        }
        
        public float GetArmorReduction()
        {
            return ArmorValue / (ArmorValue + 100f);
        }
    }

    /// <summary>
    /// 防御陷阱数据
    /// </summary>
    [System.Serializable]
    public class TrapData
    {
        public int Id;
        public TrapType Type;
        public Vector3 Position;
        public bool IsActive;
        public bool IsTriggered;
        public float Damage;
        public float EffectRadius;
        public int MaxTriggers;
        public int TriggersLeft;
        public float RearmTime;
        public float LastTriggerTime;
        public List<string> EffectTypes;
        
        public TrapData()
        {
            EffectTypes = new List<string>();
            IsActive = true;
            MaxTriggers = 1;
            TriggersLeft = 1;
        }
        
        public bool CanTrigger()
        {
            return IsActive && !IsTriggered && TriggersLeft > 0;
        }
        
        public bool NeedsRearm()
        {
            return IsTriggered && Time.time - LastTriggerTime >= RearmTime;
        }
    }

    /// <summary>
    /// 防御区域数据
    /// </summary>
    [System.Serializable]
    public class DefenseZoneData
    {
        public int Id;
        public string Name;
        public Vector3 Center;
        public float Radius;
        public DefenseZoneType Type;
        public List<int> TowerIds;
        public List<int> TrapIds;
        public List<int> FortificationIds;
        public bool IsActive;
        public float ThreatLevel;
        public int Priority;
        
        public DefenseZoneData()
        {
            TowerIds = new List<int>();
            TrapIds = new List<int>();
            FortificationIds = new List<int>();
            IsActive = true;
            Priority = 1;
        }
        
        public bool ContainsPosition(Vector3 position)
        {
            return Vector3.Distance(Center, position) <= Radius;
        }
    }

    /// <summary>
    /// 防御策略数据
    /// </summary>
    [System.Serializable]
    public class DefenseStrategyData
    {
        public string Id;
        public string Name;
        public string Description;
        public DefenseStrategyType Type;
        public Dictionary<string, float> Parameters;
        public bool IsActive;
        public float Effectiveness;
        public List<string> RequiredTechs;
        
        public DefenseStrategyData()
        {
            Parameters = new Dictionary<string, float>();
            RequiredTechs = new List<string>();
            Effectiveness = 1f;
        }
    }

    /// <summary>
    /// 防御任务数据
    /// </summary>
    [System.Serializable]
    public class DefenseTaskData
    {
        public int Id;
        public DefenseTaskType Type;
        public Vector3 Location;
        public int Priority;
        public bool IsCompleted;
        public bool IsAssigned;
        public int AssignedWorkerId;
        public float Progress;
        public float EstimatedTime;
        public Dictionary<string, int> RequiredResources;
        
        public DefenseTaskData()
        {
            RequiredResources = new Dictionary<string, int>();
            Priority = 1;
        }
        
        public float GetCompletionRatio()
        {
            return Progress / 100f;
        }
    }

    /// <summary>
    /// 防御事件数据
    /// </summary>
    [System.Serializable]
    public class DefenseEventData
    {
        public int Id;
        public DefenseEventType Type;
        public Vector3 Location;
        public float Timestamp;
        public string Description;
        public int Severity;
        public bool IsResolved;
        public Dictionary<string, object> EventData;
        
        public DefenseEventData()
        {
            EventData = new Dictionary<string, object>();
            Timestamp = Time.time;
        }
        
        public float GetTimeElapsed()
        {
            return Time.time - Timestamp;
        }
    }

    /// <summary>
    /// 防御塔类型枚举
    /// </summary>
    public enum TowerType
    {
        Basic,          // 基础防御塔
        Heavy,          // 重型防御塔
        Sniper,         // 狙击塔
        Splash,         // 溅射塔
        MachineGun,     // 机枪塔
        Cannon,         // 火炮塔
        Flamethrower,   // 火焰塔
        Mortar,         // 榴弹塔
        Laser,          // 激光塔
        Tesla,          // 电磁塔
        Freeze,         // 冰冻塔
        Missile,        // 导弹塔
        Plasma          // 等离子塔
    }

    /// <summary>
    /// 投射物类型枚举
    /// </summary>
    public enum ProjectileType
    {
        Bullet,         // 子弹
        Shell,          // 炮弹
        Flame,          // 火焰
        SniperBullet,   // 狙击弹
        Grenade,        // 榴弹
        Laser,          // 激光
        Lightning,      // 闪电
        Ice,            // 冰弹
        Missile,        // 导弹
        Plasma          // 等离子
    }

    /// <summary>
    /// 防御工事类型枚举
    /// </summary>
    public enum FortificationType
    {
        Wall,           // 墙壁
        Barrier,        // 路障
        Sandbag,        // 沙袋
        Trench,         // 战壕
        Bunker,         // 地堡
        Gate,           // 大门
        Fence           // 围栏
    }

    /// <summary>
    /// 陷阱类型枚举
    /// </summary>
    public enum TrapType
    {
        Spike,          // 尖刺陷阱
        Explosive,      // 爆炸陷阱
        Electric,       // 电击陷阱
        Poison,         // 毒气陷阱
        Slow,           // 减速陷阱
        Alarm,          // 警报陷阱
        Pit             // 陷坑
    }

    /// <summary>
    /// 防御区域类型枚举
    /// </summary>
    public enum DefenseZoneType
    {
        Perimeter,      // 外围防线
        Checkpoint,     // 检查点
        Stronghold,     // 据点
        Fallback,       // 后备防线
        Critical        // 核心区域
    }

    /// <summary>
    /// 防御策略类型枚举
    /// </summary>
    public enum DefenseStrategyType
    {
        Aggressive,     // 激进型
        Defensive,      // 防守型
        Balanced,       // 平衡型
        Economic,       // 经济型
        Specialized     // 专门化
    }

    /// <summary>
    /// 防御任务类型枚举
    /// </summary>
    public enum DefenseTaskType
    {
        BuildTower,     // 建造防御塔
        RepairTower,    // 修理防御塔
        UpgradeTower,   // 升级防御塔
        BuildWall,      // 建造墙壁
        RepairWall,     // 修理墙壁
        SetTrap,        // 设置陷阱
        Patrol,         // 巡逻
        Reinforce       // 增援
    }

    /// <summary>
    /// 防御事件类型枚举
    /// </summary>
    public enum DefenseEventType
    {
        TowerDestroyed,     // 防御塔被摧毁
        WallBreached,       // 墙壁被突破
        TrapTriggered,      // 陷阱触发
        EnemySpotted,       // 发现敌人
        AmmoLow,            // 弹药不足
        PowerFailure,       // 电力故障
        RepairNeeded        // 需要维修
    }
} 