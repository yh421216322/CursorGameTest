using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 僵尸威胁等级
    /// </summary>
    public enum ZombieThreatLevel
    {
        Safe = 0,      // 安全 - 无威胁
        Low = 1,       // 低威胁 - 偶尔有僵尸出现
        Medium = 2,    // 中等威胁 - 小规模僵尸群
        High = 3,      // 高威胁 - 大规模僵尸群
        Extreme = 4    // 极端威胁 - 僵尸潮
    }

    /// <summary>
    /// 僵尸类型
    /// </summary>
    public enum ZombieType
    {
        Walker,     // 行尸 - 基础僵尸，移动缓慢
        Runner,     // 奔跑者 - 移动快速
        Spitter,    // 吐酸者 - 远程攻击
        Tank,       // 坦克型 - 高血量高攻击
        Screamer    // 尖叫者 - 会吸引其他僵尸
    }

    /// <summary>
    /// 僵尸状态
    /// </summary>
    public enum ZombieState
    {
        Wandering,  // 游荡
        Approaching,// 接近目标
        Attacking,  // 攻击中
        Dead        // 死亡
    }

    /// <summary>
    /// 单个僵尸数据
    /// </summary>
    [Serializable]
    public class ZombieData
    {
        public string id;
        public ZombieType type;
        public ZombieState state;
        public Vector2 position;
        public Vector2 targetPosition;
        public float currentHealth;
        public float maxHealth;
        public float moveSpeed;
        public float attackPower;
        public float detectionRange;
        public float attackRange;
        public float lastAttackTime;
        public bool isAttackingBuilding;
        public string targetBuildingId;
        
        // 特殊属性
        public float spitCooldown; // 吐酸者冷却时间
        public float screamCooldown; // 尖叫者冷却时间
        public int attractedZombiesCount; // 被吸引的僵尸数量
        
        public ZombieData()
        {
            id = Guid.NewGuid().ToString();
            state = ZombieState.Wandering;
            currentHealth = maxHealth;
            isAttackingBuilding = false;
        }
        
        /// <summary>
        /// 创建指定类型的僵尸
        /// </summary>
        public static ZombieData CreateZombie(ZombieType type, Vector2 spawnPosition)
        {
            var zombie = new ZombieData
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
            
            zombie.currentHealth = zombie.maxHealth;
            return zombie;
        }
        
        /// <summary>
        /// 僵尸是否还活着
        /// </summary>
        public bool IsAlive => state != ZombieState.Dead && currentHealth > 0;
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            if (currentHealth <= 0)
            {
                state = ZombieState.Dead;
            }
        }
    }

    /// <summary>
    /// 僵尸群数据
    /// </summary>
    [Serializable]
    public class ZombieHorde
    {
        public string id;
        public string name;
        public Vector2 centerPosition;
        public float radius;
        public List<string> zombieIds;
        public ZombieThreatLevel threatLevel;
        public Vector2 moveDirection;
        public float moveSpeed;
        public bool isActive;
        public float formationTime;
        
        public ZombieHorde()
        {
            id = Guid.NewGuid().ToString();
            zombieIds = new List<string>();
            isActive = true;
            formationTime = Time.time;
        }
        
        public int ZombieCount => zombieIds?.Count ?? 0;
    }

    /// <summary>
    /// 僵尸攻击事件数据
    /// </summary>
    [Serializable]
    public class ZombieAttackData
    {
        public string zombieId;
        public string targetBuildingId;
        public float damage;
        public Vector2 attackPosition;
        public ZombieType attackerType;
        public float timestamp;
        
        public ZombieAttackData(string zombieId, string buildingId, float damage, Vector2 pos, ZombieType type)
        {
            this.zombieId = zombieId;
            this.targetBuildingId = buildingId;
            this.damage = damage;
            this.attackPosition = pos;
            this.attackerType = type;
            this.timestamp = Time.time;
        }
    }

    /// <summary>
    /// 僵尸生成配置
    /// </summary>
    [Serializable]
    public class ZombieSpawnConfig
    {
        public ZombieType type;
        public int minCount;
        public int maxCount;
        public float spawnWeight;
        public int dayRequirement; // 需要的最低天数
        
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
    /// 威胁等级配置
    /// </summary>
    [Serializable]
    public class ThreatLevelConfig
    {
        public ZombieThreatLevel level;
        public int minZombies;
        public int maxZombies;
        public float spawnChance;
        public List<ZombieSpawnConfig> spawnConfigs;
        
        public ThreatLevelConfig(ZombieThreatLevel level, int min, int max, float chance)
        {
            this.level = level;
            this.minZombies = min;
            this.maxZombies = max;
            this.spawnChance = chance;
            this.spawnConfigs = new List<ZombieSpawnConfig>();
        }
    }

    /// <summary>
    /// 僵尸系统状态数据
    /// </summary>
    [Serializable]
    public class ZombieSystemData
    {
        // 僵尸管理
        public Dictionary<string, ZombieData> zombies;
        public Dictionary<string, ZombieHorde> hordes;
        public List<ZombieAttackData> recentAttacks;
        
        // 威胁系统
        public ZombieThreatLevel currentThreatLevel;
        public float threatAccumulation; // 威胁积累值
        public float lastThreatIncrease;
        public int dayOfLastMajorAttack;
        
        // 生成系统
        public float lastSpawnTime;
        public float nextSpawnTime;
        public int totalZombiesKilled;
        public int totalZombiesSpawned;
        
        // 攻击系统
        public Dictionary<string, float> buildingUnderAttack; // 建筑ID -> 攻击时间
        public float lastAttackWarningTime;
        
        // 配置数据
        public Dictionary<ZombieThreatLevel, ThreatLevelConfig> threatConfigs;
        
        public ZombieSystemData()
        {
            zombies = new Dictionary<string, ZombieData>();
            hordes = new Dictionary<string, ZombieHorde>();
            recentAttacks = new List<ZombieAttackData>();
            buildingUnderAttack = new Dictionary<string, float>();
            
            currentThreatLevel = ZombieThreatLevel.Safe;
            threatAccumulation = 0f;
            lastThreatIncrease = 0f;
            dayOfLastMajorAttack = 0;
            
            totalZombiesKilled = 0;
            totalZombiesSpawned = 0;
            
            InitializeThreatConfigs();
        }
        
        private void InitializeThreatConfigs()
        {
            threatConfigs = new Dictionary<ZombieThreatLevel, ThreatLevelConfig>();
            
            // 安全等级 - 无僵尸生成
            var safeConfig = new ThreatLevelConfig(ZombieThreatLevel.Safe, 0, 0, 0f);
            threatConfigs[ZombieThreatLevel.Safe] = safeConfig;
            
            // 低威胁等级
            var lowConfig = new ThreatLevelConfig(ZombieThreatLevel.Low, 1, 3, 0.3f);
            lowConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Walker, 1, 2, 0.8f, 1));
            lowConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Runner, 0, 1, 0.2f, 2));
            threatConfigs[ZombieThreatLevel.Low] = lowConfig;
            
            // 中等威胁等级
            var mediumConfig = new ThreatLevelConfig(ZombieThreatLevel.Medium, 3, 8, 0.5f);
            mediumConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Walker, 2, 4, 0.6f, 1));
            mediumConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Runner, 1, 3, 0.3f, 2));
            mediumConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Spitter, 0, 1, 0.1f, 5));
            threatConfigs[ZombieThreatLevel.Medium] = mediumConfig;
            
            // 高威胁等级
            var highConfig = new ThreatLevelConfig(ZombieThreatLevel.High, 8, 15, 0.4f);
            highConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Walker, 3, 6, 0.5f, 1));
            highConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Runner, 2, 5, 0.3f, 2));
            highConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Spitter, 1, 2, 0.15f, 5));
            highConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Tank, 0, 1, 0.05f, 10));
            threatConfigs[ZombieThreatLevel.High] = highConfig;
            
            // 极端威胁等级（僵尸潮）
            var extremeConfig = new ThreatLevelConfig(ZombieThreatLevel.Extreme, 15, 30, 0.25f);
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Walker, 5, 10, 0.4f, 1));
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Runner, 3, 8, 0.3f, 2));
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Spitter, 2, 4, 0.2f, 5));
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Tank, 1, 2, 0.08f, 10));
            extremeConfig.spawnConfigs.Add(new ZombieSpawnConfig(ZombieType.Screamer, 0, 1, 0.02f, 15));
            threatConfigs[ZombieThreatLevel.Extreme] = extremeConfig;
        }
        
        /// <summary>
        /// 获取存活僵尸数量
        /// </summary>
        public int GetAliveZombieCount()
        {
            int count = 0;
            foreach (var zombie in zombies.Values)
            {
                if (zombie.IsAlive)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// 清理死亡僵尸
        /// </summary>
        public void CleanupDeadZombies()
        {
            var deadZombieIds = new List<string>();
            foreach (var pair in zombies)
            {
                if (!pair.Value.IsAlive)
                {
                    deadZombieIds.Add(pair.Key);
                }
            }
            
            foreach (var deadId in deadZombieIds)
            {
                zombies.Remove(deadId);
                totalZombiesKilled++;
                
                // 从僵尸群中移除
                foreach (var horde in hordes.Values)
                {
                    horde.zombieIds.Remove(deadId);
                }
            }
            
            // 清理空的僵尸群
            var emptyHordeIds = new List<string>();
            foreach (var pair in hordes)
            {
                if (pair.Value.ZombieCount == 0)
                {
                    emptyHordeIds.Add(pair.Key);
                }
            }
            
            foreach (var emptyId in emptyHordeIds)
            {
                hordes.Remove(emptyId);
            }
        }
    }
}