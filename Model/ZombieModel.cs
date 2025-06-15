// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieModel.cs
// 作者：未知开发者
// 创建日期：2024年07月15日 // 根据实际情况修改
// 修改日期：2024年07月15日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了与游戏中僵尸（敌人）相关的核心数据模型、枚举和事件。
//     包括僵尸的威胁等级、类型、状态、单个僵尸数据、僵尸群数据、
//     攻击事件、生成配置以及整个僵尸系统的状态管理。
// ==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 僵尸威胁等级枚举。
    /// 用于评估当前游戏环境的危险程度。
    /// </summary>
    public enum ZombieThreatLevel
    {
        /// <summary>
        /// 安全：当前区域无明显僵尸威胁。
        /// </summary>
        Safe = 0,
        /// <summary>
        /// 低威胁：少量游荡僵尸，威胁较低。
        /// </summary>
        Low = 1,
        /// <summary>
        /// 中等威胁：出现小规模僵尸群体，需要警惕。
        /// </summary>
        Medium = 2,
        /// <summary>
        /// 高威胁：大规模僵尸群体活动，对生存构成显著威胁。
        /// </summary>
        High = 3,
        /// <summary>
        /// 极端威胁：面临僵尸潮等大规模攻击，生存极度困难。
        /// </summary>
        Extreme = 4
    }

    /// <summary>
    /// 僵尸类型枚举。
    /// 定义了不同种类的僵尸及其特性。
    /// </summary>
    public enum ZombieType
    {
        /// <summary>
        /// 行尸：最基础的僵尸类型，移动缓慢，攻击力一般。
        /// </summary>
        Walker,
        /// <summary>
        /// 奔跑者：移动速度较快的僵尸，能迅速接近目标。
        /// </summary>
        Runner,
        /// <summary>
        /// 吐酸者：能够进行远程吐酸攻击的僵尸。
        /// </summary>
        Spitter,
        /// <summary>
        /// 坦克型僵尸：具有极高生命值和强大攻击力，难以消灭。
        /// </summary>
        Tank,
        /// <summary>
        /// 尖叫者：能够发出尖叫吸引附近其他僵尸的特殊类型。
        /// </summary>
        Screamer
    }

    /// <summary>
    /// 僵尸状态枚举。
    /// 描述僵尸当前的行动或生理状态。
    /// </summary>
    public enum ZombieState
    {
        /// <summary>
        /// 游荡状态：僵尸在没有特定目标时漫无目的地移动。
        /// </summary>
        Wandering,
        /// <summary>
        /// 接近目标状态：僵尸发现目标并正在向其移动。
        /// </summary>
        Approaching,
        /// <summary>
        /// 攻击中状态：僵尸正在对目标（玩家、建筑等）进行攻击。
        /// </summary>
        Attacking,
        /// <summary>
        /// 死亡状态：僵尸已失去生命体征。
        /// </summary>
        Dead
    }

    /// <summary>
    /// 单个僵尸的数据模型类。
    /// 存储了僵尸的各种属性和状态信息。
    /// </summary>
    [Serializable]
    public class ZombieData
    {
        /// <summary>
        /// 僵尸的唯一标识符。
        /// </summary>
        public string id;
        /// <summary>
        /// 僵尸的类型 (参考 ZombieType 枚举)。
        /// </summary>
        public ZombieType type;
        /// <summary>
        /// 僵尸当前的状态 (参考 ZombieState 枚举)。
        /// </summary>
        public ZombieState state;
        /// <summary>
        /// 僵尸在游戏世界中的当前二维坐标位置。
        /// </summary>
        public Vector2 position;
        /// <summary>
        /// 僵尸正在前往的目标位置坐标。
        /// </summary>
        public Vector2 targetPosition;
        /// <summary>
        /// 僵尸当前的生命值。
        /// </summary>
        public float currentHealth;
        /// <summary>
        /// 僵尸的最大生命值。
        /// </summary>
        public float maxHealth;
        /// <summary>
        /// 僵尸的移动速度。
        /// </summary>
        public float moveSpeed;
        /// <summary>
        /// 僵尸的攻击力。
        /// </summary>
        public float attackPower;
        /// <summary>
        /// 僵尸能够侦测到目标的范围。
        /// </summary>
        public float detectionRange;
        /// <summary>
        /// 僵尸的攻击范围。
        /// </summary>
        public float attackRange;
        /// <summary>
        /// 僵尸上一次攻击的时间戳。
        /// </summary>
        public float lastAttackTime;
        /// <summary>
        /// 标记僵尸当前是否正在攻击建筑物。
        /// </summary>
        public bool isAttackingBuilding;
        /// <summary>
        /// 如果正在攻击建筑物，则为目标建筑物的ID。
        /// </summary>
        public string targetBuildingId;
        
        // 特殊属性 (Special Attributes)
        /// <summary>
        /// 吐酸者的吐酸技能冷却时间（秒）。
        /// </summary>
        public float spitCooldown; // 吐酸者冷却时间
        /// <summary>
        /// 尖叫者的尖叫技能冷却时间（秒）。
        /// </summary>
        public float screamCooldown; // 尖叫者冷却时间
        /// <summary>
        /// （可能指）被此僵尸（如尖叫者）吸引过来的其他僵尸数量。
        /// </summary>
        public int attractedZombiesCount; // 被吸引的僵尸数量
        
        /// <summary>
        /// ZombieData的构造函数。
        /// 初始化僵尸ID、默认状态和生命值。
        /// </summary>
        public ZombieData()
        {
            id = Guid.NewGuid().ToString(); // 生成唯一ID
            state = ZombieState.Wandering;  // 初始状态为游荡
            currentHealth = maxHealth;      // 初始生命值为最大生命值
            isAttackingBuilding = false;    // 初始不攻击建筑
        }
        
        /// <summary>
        /// 创建并初始化一个指定类型的僵尸实例。
        /// </summary>
        /// <param name="type">要创建的僵尸类型。</param>
        /// <param name="spawnPosition">僵尸的出生位置。</param>
        /// <returns>配置完成的ZombieData实例。</returns>
        public static ZombieData CreateZombie(ZombieType type, Vector2 spawnPosition)
        {
            var zombie = new ZombieData // 创建新的僵尸数据实例
            {
                type = type,
                position = spawnPosition,
                targetPosition = spawnPosition
            };
            
            // 根据类型设置属性
            switch (type)
            {
                case ZombieType.Walker:
                    zombie.maxHealth = 20f;
                    zombie.moveSpeed = 1.0f;
                    zombie.attackPower = 5f;
                    zombie.detectionRange = 3f;
                    zombie.attackRange = 1f;
                    break;
                    
                case ZombieType.Runner:
                    zombie.maxHealth = 15f;
                    zombie.moveSpeed = 2.5f;
                    zombie.attackPower = 4f;
                    zombie.detectionRange = 4f;
                    zombie.attackRange = 1f;
                    break;
                    
                case ZombieType.Spitter:
                    zombie.maxHealth = 12f;
                    zombie.moveSpeed = 1.2f;
                    zombie.attackPower = 6f;
                    zombie.detectionRange = 5f;
                    zombie.attackRange = 4f;
                    zombie.spitCooldown = 3f;
                    break;
                    
                case ZombieType.Tank:
                    zombie.maxHealth = 50f;
                    zombie.moveSpeed = 0.8f;
                    zombie.attackPower = 12f;
                    zombie.detectionRange = 3f;
                    zombie.attackRange = 1.5f;
                    break;
                    
                case ZombieType.Screamer:
                    zombie.maxHealth = 18f;
                    zombie.moveSpeed = 1.5f;
                    zombie.attackPower = 3f;
                    zombie.detectionRange = 6f;
                    zombie.attackRange = 1f;
                    zombie.screamCooldown = 10f;
                    break;
            }
            

            zombie.currentHealth = zombie.maxHealth; // 设置当前生命值为最大生命值
            return zombie; // 返回创建的僵尸实例
        }
        
        /// <summary>
        /// 获取一个布尔值，表示此僵尸是否仍然存活。
        /// 存活条件：状态不是死亡且当前生命值大于0。
        /// </summary>
        public bool IsAlive => state != ZombieState.Dead && currentHealth > 0;
        
        /// <summary>
        /// 对僵尸造成指定数值的伤害。
        /// 如果生命值降至0或以下，僵尸状态将变为死亡。
        /// </summary>
        /// <param name="damage">要施加的伤害量。</param>
        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage); // 扣除伤害，但不低于0
            if (currentHealth <= 0) // 如果生命值耗尽
            {
                state = ZombieState.Dead; // 将状态设置为死亡
            }
        }
    }

    /// <summary>
    /// 僵尸群（Horde）的数据模型类。
    /// 用于组织和管理一群僵尸。
    /// </summary>
    [Serializable]
    public class ZombieHorde
    {
        /// <summary>
        /// 僵尸群的唯一标识符。
        /// </summary>
        public string id;
        /// <summary>
        /// 僵尸群的名称（可选，用于区分或描述）。
        /// </summary>
        public string name;
        /// <summary>
        /// 僵尸群的中心位置坐标。
        /// </summary>
        public Vector2 centerPosition;
        /// <summary>
        /// 僵尸群的活动半径或影响范围。
        /// </summary>
        public float radius;
        /// <summary>
        /// 属于此僵尸群的僵尸ID列表。
        /// </summary>
        public List<string> zombieIds;
        /// <summary>
        /// 此僵尸群的威胁等级 (参考 ZombieThreatLevel 枚举)。
        /// </summary>
        public ZombieThreatLevel threatLevel;
        /// <summary>
        /// 僵尸群当前的移动方向。
        /// </summary>
        public Vector2 moveDirection;
        /// <summary>
        /// 僵尸群的整体移动速度。
        /// </summary>
        public float moveSpeed;
        /// <summary>
        /// 标记此僵尸群当前是否处于活动状态。
        /// </summary>
        public bool isActive;
        /// <summary>
        /// 僵尸群形成时的时间戳。
        /// </summary>
        public float formationTime;
        
        /// <summary>
        /// ZombieHorde的构造函数。
        /// 初始化僵尸群ID、僵尸ID列表、活动状态和形成时间。
        /// </summary>
        public ZombieHorde()
        {
            id = Guid.NewGuid().ToString();      // 生成唯一ID
            zombieIds = new List<string>();    // 初始化僵尸ID列表
            isActive = true;                   // 默认激活状态
            formationTime = Time.time;         // 记录形成时间
        }
        
        /// <summary>
        /// 获取此僵尸群中包含的僵尸数量。
        /// </summary>
        public int ZombieCount => zombieIds?.Count ?? 0; // 如果列表为空则返回0
    }

    /// <summary>
    /// 僵尸攻击事件的数据模型类。
    /// 记录了一次僵尸攻击的详细信息。
    /// </summary>
    [Serializable]
    public class ZombieAttackData
    {
        /// <summary>
        /// 发动攻击的僵尸的ID。
        /// </summary>
        public string zombieId;
        /// <summary>
        /// 被攻击的目标建筑物的ID（如果适用）。
        /// </summary>
        public string targetBuildingId;
        /// <summary>
        /// 本次攻击造成的伤害量。
        /// </summary>
        public float damage;
        /// <summary>
        /// 攻击发生时的位置坐标。
        /// </summary>
        public Vector2 attackPosition;
        /// <summary>
        /// 发动攻击的僵尸的类型 (参考 ZombieType 枚举)。
        /// </summary>
        public ZombieType attackerType;
        /// <summary>
        /// 攻击事件发生的时间戳。
        /// </summary>
        public float timestamp;
        
        /// <summary>
        /// ZombieAttackData的构造函数。
        /// </summary>
        /// <param name="zombieId">攻击者僵尸ID。</param>
        /// <param name="buildingId">目标建筑ID。</param>
        /// <param name="damage">伤害值。</param>
        /// <param name="pos">攻击位置。</param>
        /// <param name="type">攻击者僵尸类型。</param>
        public ZombieAttackData(string zombieId, string buildingId, float damage, Vector2 pos, ZombieType type)
        {
            this.zombieId = zombieId;
            this.targetBuildingId = buildingId;
            this.damage = damage;
            this.attackPosition = pos;
            this.attackerType = type;
            this.timestamp = Time.time; // 记录事件发生时间
        }
    }

    /// <summary>
    /// 僵尸生成配置类。
    /// 定义了特定类型僵尸在生成时的参数。
    /// </summary>
    [Serializable]
    public class ZombieSpawnConfig
    {
        /// <summary>
        /// 要生成的僵尸类型 (参考 ZombieType 枚举)。
        /// </summary>
        public ZombieType type;
        /// <summary>
        /// 一次生成此类型僵尸的最小数量。
        /// </summary>
        public int minCount;
        /// <summary>
        /// 一次生成此类型僵尸的最大数量。
        /// </summary>
        public int maxCount;
        /// <summary>
        /// 此类型僵尸的生成权重（影响其在混合生成时的相对概率）。
        /// </summary>
        public float spawnWeight;
        /// <summary>
        /// 生成此类型僵尸所需的最低游戏天数。
        /// </summary>
        public int dayRequirement; // 需要的最低天数
        
        /// <summary>
        /// ZombieSpawnConfig的构造函数。
        /// </summary>
        /// <param name="type">僵尸类型。</param>
        /// <param name="min">最小生成数量。</param>
        /// <param name="max">最大生成数量。</param>
        /// <param name="weight">生成权重。</param>
        /// <param name="dayReq">最低天数要求，默认为1。</param>
        public ZombieSpawnConfig(ZombieType type, int min, int max, float weight, int dayReq = 1)
        {
            this.type = type;
            this.minCount = min;
            this.maxCount = max;
            this.spawnWeight = weight;
            this.dayRequirement = dayReq;
        }
    }

    /// <summary>
    /// 威胁等级配置类。
    /// 定义了每个僵尸威胁等级下的僵尸生成规则和参数。
    /// </summary>
    [Serializable]
    public class ThreatLevelConfig
    {
        /// <summary>
        /// 此配置对应的威胁等级 (参考 ZombieThreatLevel 枚举)。
        /// </summary>
        public ZombieThreatLevel level;
        /// <summary>
        /// 在此威胁等级下，一次生成的僵尸总数的最小值。
        /// </summary>
        public int minZombies;
        /// <summary>
        /// 在此威胁等级下，一次生成的僵尸总数的最大值。
        /// </summary>
        public int maxZombies;
        /// <summary>
        /// 在此威胁等级下，发生僵尸生成的概率。
        /// </summary>
        public float spawnChance;
        /// <summary>
        /// 详细的僵尸生成配置列表，定义了在此威胁等级下可能生成的各种僵尸及其参数。
        /// </summary>
        public List<ZombieSpawnConfig> spawnConfigs;
        
        /// <summary>
        /// ThreatLevelConfig的构造函数。
        /// </summary>
        /// <param name="level">威胁等级。</param>
        /// <param name="min">最小僵尸总数。</param>
        /// <param name="max">最大僵尸总数。</param>
        /// <param name="chance">生成概率。</param>
        public ThreatLevelConfig(ZombieThreatLevel level, int min, int max, float chance)
        {
            this.level = level;
            this.minZombies = min;
            this.maxZombies = max;
            this.spawnChance = chance;
            this.spawnConfigs = new List<ZombieSpawnConfig>(); // 初始化生成配置列表
        }
    }

    /// <summary>
    /// 僵尸系统状态数据类。
    /// 存储和管理整个僵尸系统的当前状态、配置和运行时数据。
    /// </summary>
    [Serializable]
    public class ZombieSystemData
    {
        // 僵尸管理 (Zombie Management)
        /// <summary>
        /// 存储所有当前活动僵尸数据的字典，键为僵尸ID。
        /// </summary>
        public Dictionary<string, ZombieData> zombies;
        /// <summary>
        /// 存储所有当前活动僵尸群数据的字典，键为僵尸群ID。
        /// </summary>
        public Dictionary<string, ZombieHorde> hordes;
        /// <summary>
        /// 最近发生的僵尸攻击事件列表。
        /// </summary>
        public List<ZombieAttackData> recentAttacks;
        
        // 威胁系统 (Threat System)
        /// <summary>
        /// 当前游戏世界的僵尸威胁等级。
        /// </summary>
        public ZombieThreatLevel currentThreatLevel;
        /// <summary>
        /// 当前积累的威胁值，用于决定威胁等级的提升。
        /// </summary>
        public float threatAccumulation; // 威胁积累值
        /// <summary>
        /// 上一次威胁等级提升的时间戳或相关记录。
        /// </summary>
        public float lastThreatIncrease;
        /// <summary>
        /// 上一次发生大规模僵尸攻击的游戏天数。
        /// </summary>
        public int dayOfLastMajorAttack;
        
        // 生成系统 (Spawn System)
        /// <summary>
        /// 上一次成功生成僵尸的时间戳。
        /// </summary>
        public float lastSpawnTime;
        /// <summary>
        /// 预计下一次尝试生成僵尸的时间戳。
        /// </summary>
        public float nextSpawnTime;
        /// <summary>
        /// 游戏中已消灭的僵尸总数。
        /// </summary>
        public int totalZombiesKilled;
        /// <summary>
        /// 游戏中已生成的僵尸总数。
        /// </summary>
        public int totalZombiesSpawned;
        
        // 攻击系统 (Attack System)
        /// <summary>
        /// 记录当前正受到僵尸攻击的建筑及其受攻击开始时间。键为建筑ID，值为时间戳。
        /// </summary>
        public Dictionary<string, float> buildingUnderAttack; // 建筑ID -> 攻击时间
        /// <summary>
        /// 上一次发出僵尸攻击警告的时间戳。
        /// </summary>
        public float lastAttackWarningTime;
        
        // 配置数据 (Configuration Data)
        /// <summary>
        /// 存储所有威胁等级配置的字典，键为威胁等级。
        /// </summary>
        public Dictionary<ZombieThreatLevel, ThreatLevelConfig> threatConfigs;
        
        /// <summary>
        /// ZombieSystemData的构造函数。
        /// 初始化各种数据集合和默认状态值。
        /// </summary>
        public ZombieSystemData()
        {
            zombies = new Dictionary<string, ZombieData>();
            hordes = new Dictionary<string, ZombieHorde>();
            recentAttacks = new List<ZombieAttackData>();
            buildingUnderAttack = new Dictionary<string, float>();
            
            currentThreatLevel = ZombieThreatLevel.Safe; // 初始威胁等级为安全
            threatAccumulation = 0f;                     // 初始威胁积累值为0
            lastThreatIncrease = 0f;
            dayOfLastMajorAttack = 0;
            
            totalZombiesKilled = 0;
            totalZombiesSpawned = 0;
            
            InitializeThreatConfigs(); // 初始化威胁等级配置
        }
        
        /// <summary>
        /// 初始化默认的威胁等级配置。
        /// 定义了不同威胁等级下的僵尸生成规则。
        /// </summary>
        private void InitializeThreatConfigs()
        {
            threatConfigs = new Dictionary<ZombieThreatLevel, ThreatLevelConfig>();
            
            // 安全等级 - 通常不生成僵尸或极少量
            var safeConfig = new ThreatLevelConfig(ZombieThreatLevel.Safe, 0, 0, 0f);
            var safeConfig = new ThreatLevelConfig(ZombieThreatLevel.Safe, 0, 0, 0f); // 无僵尸，0概率
            threatConfigs[ZombieThreatLevel.Safe] = safeConfig;
            
            // 低威胁等级配置
            var lowConfig = new ThreatLevelConfig(ZombieThreatLevel.Low, 1, 3, 0.3f); // 1-3只僵尸，30%概率生成
            lowConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Walker, 1, 2, 0.8f, 1)); // 80%权重行尸
            lowConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Runner, 0, 1, 0.2f, 2)); // 20%权重奔跑者 (第2天后)
            threatConfigs[ZombieThreatLevel.Low] = lowConfig;
            
            // 中等威胁等级配置
            var mediumConfig = new ThreatLevelConfig(ZombieThreatLevel.Medium, 3, 8, 0.5f); // 3-8只僵尸，50%概率
            mediumConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Walker, 2, 4, 0.6f, 1)); // 60%行尸
            mediumConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Runner, 1, 3, 0.3f, 2)); // 30%奔跑者
            mediumConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Spitter, 0, 1, 0.1f, 5)); // 10%吐酸者 (第5天后)
            threatConfigs[ZombieThreatLevel.Medium] = mediumConfig;
            
            // 高威胁等级配置
            var highConfig = new ThreatLevelConfig(ZombieThreatLevel.High, 8, 15, 0.4f); // 8-15只僵尸，40%概率 (示例中概率反而降低，可能需要调整)
            highConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Walker, 3, 6, 0.5f, 1)); // 50%行尸
            highConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Runner, 2, 5, 0.3f, 2)); // 30%奔跑者
            highConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Spitter, 1, 2, 0.15f, 5)); // 15%吐酸者
            highConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Tank, 0, 1, 0.05f, 10));    // 5%坦克 (第10天后)
            threatConfigs[ZombieThreatLevel.High] = highConfig;
            
            // 极端威胁等级（例如僵尸潮）配置
            var extremeConfig = new ThreatLevelConfig(ZombieThreatLevel.Extreme, 15, 30, 0.25f); // 15-30只僵尸，25%概率 (同样，概率可能需审视)
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Walker, 5, 10, 0.4f, 1));  // 40%行尸
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Runner, 3, 8, 0.3f, 2));   // 30%奔跑者
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Spitter, 2, 4, 0.2f, 5));  // 20%吐酸者
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Tank, 1, 2, 0.08f, 10));     // 8%坦克
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Screamer, 0, 1, 0.02f, 15)); // 2%尖叫者 (第15天后)
            threatConfigs[ZombieThreatLevel.Extreme] = extremeConfig;
        }
        
        /// <summary>
        /// 获取当前所有存活的僵尸数量。
        /// </summary>
        /// <returns>存活僵尸的总数。</returns>
        public int GetAliveZombieCount()
        {
            int count = 0; // 初始化计数器
            foreach (var zombie in zombies.Values) // 遍历所有僵尸
            {
                if (zombie.IsAlive) // 如果僵尸存活
                    count++;        // 增加计数
            }
            return count; // 返回总数
        }
        
        /// <summary>
        /// 清理并移除所有已死亡的僵尸。
        /// 同时更新被击杀僵尸总数，并从僵尸群中移除这些死亡单位。
        /// 清理空的僵尸群。
        /// </summary>
        public void CleanupDeadZombies()
        {
            var deadZombieIds = new List<string>(); // 用于存储待移除的死亡僵尸ID
            foreach (var pair in zombies) // 遍历僵尸字典
            {
                if (!pair.Value.IsAlive) // 如果僵尸不再存活
                {
                    deadZombieIds.Add(pair.Key); // 将其ID添加到待移除列表
                }
            }
            
            foreach (var deadId in deadZombieIds) // 遍历待移除列表
            {
                zombies.Remove(deadId); // 从主僵尸字典中移除
                totalZombiesKilled++;   // 增加击杀僵尸计数
                
                // 从所有僵尸群中移除此僵尸ID
                foreach (var horde in hordes.Values)
                {
                    horde.zombieIds.Remove(deadId);
                }
            }
            
            // 清理空的僵尸群
            var emptyHordeIds = new List<string>(); // 用于存储待移除的空僵尸群ID
            foreach (var pair in hordes) // 遍历僵尸群字典
            {
                if (pair.Value.ZombieCount == 0) // 如果僵尸群中没有僵尸了
                {
                    emptyHordeIds.Add(pair.Key); // 将其ID添加到待移除列表
                }
            }
            
            foreach (var emptyId in emptyHordeIds) // 遍历待移除列表
            {
                hordes.Remove(emptyId); // 从主僵尸群字典中移除
            }
        }
    }
}