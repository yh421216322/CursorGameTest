using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using MyGameNamespace;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 僵尸系统接口
    /// 定义僵尸系统对外暴露的核心行为，供其他系统调用或监听
    /// </summary>
    public interface IZombieSystem : QFISystem
    {
        void Update(); // 系统主更新循环
        void SpawnZombie(ZombieType type, Vector2 position); // 生成单个僵尸
        void SpawnZombieHorde(ZombieThreatLevel threatLevel, Vector2 centerPosition); // 生成僵尸群
        void UpdateZombieMovement(); // 更新所有僵尸移动逻辑
        void UpdateZombieAttacks(); // 更新所有僵尸攻击逻辑
        void ProcessZombieDeath(string zombieId); // 处理僵尸死亡
        void UpdateThreatLevel(); // 更新当前威胁等级
        ZombieSystemData GetZombieData(); // 获取当前僵尸系统数据
        List<ZombieData> GetZombiesNearPosition(Vector2 position, float radius); // 获取指定范围内的僵尸列表
        void TakeDamageToZombie(string zombieId, float damage); // 给指定僵尸造成伤害
        ZombieThreatLevel GetCurrentThreatLevel(); // 获取当前僵尸威胁等级
        int GetTotalZombieCount(); // 获取总生成的僵尸数量
        int GetActiveZombieCount(); // 获取当前存活的僵尸数量
    }
    
    /// <summary>
    /// 僵尸系统实现类
    /// 负责管理僵尸的生成、移动、攻击、死亡以及威胁等级的动态变化。
    /// 
    /// 核心机制：
    /// - 按照时间间隔自动刷新僵尸状态
    /// - 根据威胁等级动态调整生成频率与类型
    /// - 支持多种类型的僵尸（Walker, Runner, Tank等）
    /// - 支持群体生成（horde）和攻击目标建筑
    /// - 提供可视化系统所需的数据支持
    /// 
    /// 关键依赖：
    /// - ISurvivalGameModel: 游戏模型，用于获取游戏时间和资源信息
    /// - IResourceSystem: 资源系统，用于影响游戏经济系统
    /// - IEnhancedBuildingSystem: 建筑系统，用于僵尸寻路和攻击
    /// - ZombieSystemData: 数据容器，保存僵尸状态和威胁信息
    /// - 各种事件驱动更新（TimeUpdateEvent, NewDayEvent等）
    /// 
    /// 状态流转：
    /// Wandering → Approaching → Attacking → Dead
    /// 
    /// 事件响应：
    /// - TimeUpdateEvent: 每帧更新时触发
    /// - NewDayEvent: 新的一天开始时增加威胁积累
    /// - BuildingDestroyedEvent: 建筑被摧毁时处理僵尸目标变更
    /// - 发送事件:
    ///   - ZombieSpawnedEvent: 僵尸生成时通知其他系统
    ///   - ZombieAttackBuildingEvent: 僵尸攻击建筑时通知UI或其他系统
    ///   - ThreatLevelChangedEvent: 威胁等级变化时通知UI或防御系统
    /// </summary>
    public class ZombieSystem : AbstractSystem, IZombieSystem
    {
        private ISurvivalGameModel mGameModel; // 游戏核心模型引用
        private IResourceSystem mResourceSystem; // 资源系统引用
        private IEnhancedBuildingSystem mBuildingSystem; // 建筑系统引用
        
        private ZombieSystemData mZombieData; // 当前僵尸系统数据存储
        
        // 系统运行参数配置
        private const float ZOMBIE_UPDATE_INTERVAL = 0.5f; // 僵尸更新周期
        private const float THREAT_UPDATE_INTERVAL = 60f; // 威胁等级更新周期
        private const float SPAWN_CHECK_INTERVAL = 15f; // 僵尸生成检查周期
        private const float MAP_SIZE = 50f; // 地图边界尺寸
        private const float SPAWN_DISTANCE_MIN = 15f; // 最小生成距离
        private const float SPAWN_DISTANCE_MAX = 25f; // 最大生成距离
        private const float ATTACK_COOLDOWN = 2f; // 攻击冷却时间
        private const float CLEANUP_INTERVAL = 30f; // 死亡僵尸清理周期
        
        private float mLastZombieUpdate = 0f; // 上次僵尸更新时间
        private float mLastThreatUpdate = 0f; // 上次威胁等级更新时间
        private float mLastSpawnCheck = 0f; // 上次生成检查时间
        private float mLastCleanup = 0f; // 上次死亡僵尸清理时间
        
        protected override void OnInit()
        {
            // 获取框架依赖组件
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mResourceSystem = this.GetSystem<IResourceSystem>();
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            
            // 初始化僵尸系统数据容器
            mZombieData = new ZombieSystemData();
            
            // 注册监听事件
            this.RegisterEvent<TimeUpdateEvent>(OnTimeUpdate);
            this.RegisterEvent<NewDayEvent>(OnNewDay);
            this.RegisterEvent<BuildingDestroyedEvent>(OnBuildingDestroyed);
            
            //Debug.Log("僵尸系统初始化完成");
        }

        /// <summary>
        /// 时间更新事件回调函数
        /// 每帧根据时间间隔执行不同任务
        /// </summary>
        private void OnTimeUpdate(TimeUpdateEvent e)
        {
            float currentTime = Time.time;
            
            // 更新僵尸移动和行为
            if (currentTime - mLastZombieUpdate >= ZOMBIE_UPDATE_INTERVAL)
            {
                UpdateZombieMovement();
                UpdateZombieAttacks();
                mLastZombieUpdate = currentTime;
            }
            
            // 更新威胁等级
            if (currentTime - mLastThreatUpdate >= THREAT_UPDATE_INTERVAL)
            {
                UpdateThreatLevel();
                mLastThreatUpdate = currentTime;
            }
            
            // 检查是否需要生成新僵尸
            if (currentTime - mLastSpawnCheck >= SPAWN_CHECK_INTERVAL)
            {
                CheckZombieSpawn();
                mLastSpawnCheck = currentTime;
            }
            
            // 定期清理死亡僵尸
            if (currentTime - mLastCleanup >= CLEANUP_INTERVAL)
            {
                mZombieData.CleanupDeadZombies();
                mLastCleanup = currentTime;
            }
        }

        /// <summary>
        /// 新的一天开始时回调
        /// 增加僵尸威胁等级积累，并触发威胁等级更新
        /// </summary>
        private void OnNewDay(NewDayEvent e)
        {
            // 每日增加威胁积累
            float threatIncrease = CalculateDailyThreatIncrease();
            mZombieData.threatAccumulation += threatIncrease;
            mZombieData.lastThreatIncrease = Time.time;
            
            Debug.Log($"第{e.Day}天 - 威胁积累增加 {threatIncrease:F1}，当前总积累: {mZombieData.threatAccumulation:F1}");
            
            // 触发威胁等级更新
            UpdateThreatLevel();
        }

        /// <summary>
        /// 建筑被摧毁时回调
        /// 停止僵尸对该建筑的攻击，并重新分配目标
        /// </summary>
        private void OnBuildingDestroyed(BuildingDestroyedEvent e)
        {
            // 如果该建筑正在被攻击，则移除攻击目标
            if (mZombieData.buildingUnderAttack.ContainsKey(e.BuildingId.ToString()))
            {
                mZombieData.buildingUnderAttack.Remove(e.BuildingId.ToString());
            }
            
            // 让所有攻击该建筑的僵尸恢复游荡状态
            foreach (var zombie in mZombieData.zombies.Values)
            {
                if (zombie.targetBuildingId == e.BuildingId.ToString())
                {
                    zombie.isAttackingBuilding = false;
                    zombie.targetBuildingId = null;
                    zombie.state = ZombieState.Wandering;
                }
            }
        }

        /// <summary>
        /// 根据当前游戏天数计算每日威胁增长量
        /// 威胁越大，僵尸生成越频繁
        /// </summary>
        private float CalculateDailyThreatIncrease()
        {
            int currentDay = mGameModel.GameDay.Value;
            float baseIncrease = 5f; // 基础增长
            
            // 随着天数增加，威胁增长加速
            float dayMultiplier = 1f + (currentDay - 1) * 0.1f;
            
            // 根据当前威胁等级调整
            float threatMultiplier = 1f;
            switch (mZombieData.currentThreatLevel)
            {
                case ZombieThreatLevel.Safe:
                    threatMultiplier = 1.2f; // 安全期后增长更快
                    break;
                case ZombieThreatLevel.Low:
                    threatMultiplier = 1.0f;
                    break;
                case ZombieThreatLevel.Medium:
                    threatMultiplier = 0.8f;
                    break;
                case ZombieThreatLevel.High:
                    threatMultiplier = 0.6f;
                    break;
                case ZombieThreatLevel.Extreme:
                    threatMultiplier = 0.4f; // 极端威胁时增长放缓
                    break;
            }
            
            return baseIncrease * dayMultiplier * threatMultiplier;
        }

        /// <summary>
        /// 检查是否应该生成新的僵尸
        /// 根据当前僵尸数量和威胁等级决定是否触发生成
        /// </summary>
        private void CheckZombieSpawn()
        {
            int currentZombieCount = mZombieData.GetAliveZombieCount();
            var threatConfig = mZombieData.threatConfigs[mZombieData.currentThreatLevel];
            
            bool shouldSpawn = currentZombieCount < threatConfig.minZombies || 
                             (currentZombieCount < threatConfig.maxZombies && UnityEngine.Random.value < threatConfig.spawnChance);
            
            if (shouldSpawn)
            {
                Vector2 spawnPosition = GetRandomSpawnPosition();
                SpawnZombieHorde(mZombieData.currentThreatLevel, spawnPosition);
            }
        }

        /// <summary>
        /// 获取随机生成位置
        /// 在避难所周围一定范围内随机生成僵尸
        /// </summary>
        private Vector2 GetRandomSpawnPosition()
        {
            const int MAX_ATTEMPTS = 20; // 最大尝试次数
            Vector2 centerPosition = Vector2.zero; // 避难所中心
            
            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                // 在指定距离范围内随机生成位置
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = UnityEngine.Random.Range(SPAWN_DISTANCE_MIN, SPAWN_DISTANCE_MAX);
                
                Vector2 spawnPosition = centerPosition + new Vector2(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance
                );
                
                // 确保生成位置在地图范围内
                spawnPosition.x = Mathf.Clamp(spawnPosition.x, -MAP_SIZE, MAP_SIZE);
                spawnPosition.y = Mathf.Clamp(spawnPosition.y, -MAP_SIZE, MAP_SIZE);
                
                // 检查生成位置是否有效（不与建筑重叠）
                if (IsPositionValid(spawnPosition, ""))
                {
                    return spawnPosition;
                }
            }
            
            // 如果尝试多次都失败，返回一个安全的默认位置
            UnityEngine.Debug.LogWarning("无法找到有效的僵尸生成位置，使用默认位置");
            Vector2 fallbackDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            return fallbackDirection * SPAWN_DISTANCE_MIN;
        }

        /// <summary>
        /// 生成一个僵尸个体
        /// 用于单独生成特定类型僵尸（如玩家手动召唤）
        /// </summary>
        public void SpawnZombie(ZombieType type, Vector2 position)
        {
            var zombie = ZombieData.CreateZombie(type, position);
            mZombieData.zombies[zombie.id] = zombie;
            mZombieData.totalZombiesSpawned++;
            
            Debug.Log($"生成僵尸: {type} 于位置 {position}");
            
            // 发送僵尸生成事件
            this.SendEvent(new ZombieSpawnedEvent { ZombieId = zombie.id, Type = type, Position = position });
        }

        /// <summary>
        /// 生成一群僵尸（根据威胁等级）
        /// 用于游戏自动刷新僵尸群
        /// </summary>
        public void SpawnZombieHorde(ZombieThreatLevel threatLevel, Vector2 centerPosition)
        {
            var threatConfig = mZombieData.threatConfigs[threatLevel];
            if (threatConfig.spawnConfigs.Count == 0) return;
            
            int totalZombies = UnityEngine.Random.Range(threatConfig.minZombies, threatConfig.maxZombies + 1);
            
            // 创建僵尸群组
            var horde = new ZombieHorde
            {
                name = $"{threatLevel}威胁僵尸群",
                centerPosition = centerPosition,
                radius = 5f,
                threatLevel = threatLevel,
                moveDirection = (Vector2.zero - centerPosition).normalized,
                moveSpeed = 0.5f
            };
            
            mZombieData.hordes[horde.id] = horde;
            
            // 生成僵尸
            for (int i = 0; i < totalZombies; i++)
            {
                ZombieType spawnType = SelectZombieTypeByWeight(threatConfig.spawnConfigs);
                Vector2 spawnPos = GetHordeSpawnPosition(centerPosition, horde.radius);
                
                var zombie = ZombieData.CreateZombie(spawnType, spawnPos);
                mZombieData.zombies[zombie.id] = zombie;
                horde.zombieIds.Add(zombie.id);
                mZombieData.totalZombiesSpawned++;
                
                this.SendEvent(new ZombieSpawnedEvent { ZombieId = zombie.id, Type = spawnType, Position = spawnPos });

            }
            
            Debug.Log($"生成{threatLevel}威胁僵尸群，共{totalZombies}只僵尸");
            
            // 发送僵尸群生成事件
            this.SendEvent(new ZombieHordeSpawnedEvent 
            { 
                HordeId = horde.id, 
                ThreatLevel = threatLevel, 
                CenterPosition = centerPosition,
                ZombieCount = totalZombies
            });
        }

        /// <summary>
        /// 根据权重选择僵尸类型
        /// 用于根据威胁等级配置，生成不同种类僵尸
        /// </summary>
        private ZombieType SelectZombieTypeByWeight(List<ZombieSpawnConfig> configs)
        {
            var validConfigs = configs.Where(c => c.dayRequirement <= mGameModel.GameDay.Value).ToList();
            if (validConfigs.Count == 0) return ZombieType.Walker;
            
            float totalWeight = validConfigs.Sum(c => c.spawnWeight);
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            
            float currentWeight = 0f;
            foreach (var config in validConfigs)
            {
                currentWeight += config.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return config.type;
                }
            }
            
            return validConfigs.First().type;
        }

        /// <summary>
        /// 获取僵尸群中的随机生成位置
        /// 用于生成僵尸群时分散生成僵尸位置，避开建筑
        /// </summary>
        private Vector2 GetHordeSpawnPosition(Vector2 center, float radius)
        {
            const int MAX_ATTEMPTS = 15; // 最大尝试次数
            
            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = UnityEngine.Random.Range(0f, radius);
                
                Vector2 spawnPosition = center + new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                
                // 检查生成位置是否有效
                if (IsPositionValid(spawnPosition, ""))
                {
                    return spawnPosition;
                }
            }
            
            // 如果无法在群内找到有效位置，尝试在群外找
            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = radius + UnityEngine.Random.Range(1f, 3f); // 在群外更远处
                
                Vector2 spawnPosition = center + new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                
                if (IsPositionValid(spawnPosition, ""))
                {
                    return spawnPosition;
                }
            }
            
            // 最后的备选方案
            return center + new Vector2(UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius));
        }

        /// <summary>
        /// 更新所有僵尸的移动逻辑
        /// 包括游荡、接近建筑、攻击等状态迁移
        /// </summary>
        public void UpdateZombieMovement()
        {
            foreach (var zombie in mZombieData.zombies.Values)
            {
                if (!zombie.IsAlive) continue;
                
                UpdateSingleZombieMovement(zombie);
            }
        }

        /// <summary>
        /// 更新单个僵尸的移动逻辑
        /// 根据僵尸当前状态执行不同的移动策略
        /// </summary>
        private void UpdateSingleZombieMovement(ZombieData zombie)
        {
            switch (zombie.state)
            {
                case ZombieState.Wandering:
                    UpdateZombieWandering(zombie);
                    break;
                    
                case ZombieState.Approaching:
                    UpdateZombieApproaching(zombie);
                    break;
                    
                case ZombieState.Attacking:
                    // 攻击状态下不移动
                    break;
            }
        }

        /// <summary>
        /// 僵尸游荡行为更新
        /// 随机游荡，发现附近建筑则转向攻击模式
        /// </summary>
        private void UpdateZombieWandering(ZombieData zombie)
        {
            // 检测是否有可攻击的建筑
            var nearestBuilding = FindNearestBuilding(zombie.position, zombie.detectionRange);
            if (nearestBuilding != null)
            {
                zombie.targetBuildingId = nearestBuilding.Id.ToString();
                zombie.targetPosition = nearestBuilding.Position;
                zombie.state = ZombieState.Approaching;
                zombie.isAttackingBuilding = true;
                return;
            }
            
            // 随机游荡
            if (Vector2.Distance(zombie.position, zombie.targetPosition) < 0.5f)
            {
                // 设置新的随机目标点（避开建筑）
                zombie.targetPosition = FindValidWanderTarget(zombie.position);
            }
            
            MoveZombieTowardsTarget(zombie);
        }

        /// <summary>
        /// 僵尸接近目标建筑的行为更新
        /// 接近后进入攻击状态
        /// </summary>
        private void UpdateZombieApproaching(ZombieData zombie)
        {
            float distanceToTarget = Vector2.Distance(zombie.position, zombie.targetPosition);
            
            // 检查是否到达攻击范围
            if (distanceToTarget <= zombie.attackRange)
            {
                zombie.state = ZombieState.Attacking;
                return;
            }
            
            // 检查目标建筑是否还存在
            if (!string.IsNullOrEmpty(zombie.targetBuildingId))
            {
                if (int.TryParse(zombie.targetBuildingId, out int buildingId))
                {
                    var building = mBuildingSystem.GetBuilding(buildingId.ToString());
                    if (building == null)
                    {
                        zombie.state = ZombieState.Wandering;
                        zombie.isAttackingBuilding = false;
                        zombie.targetBuildingId = null;
                        return;
                    }
                    
                    zombie.targetPosition = building.Position;
                }
            }
            
            MoveZombieTowardsTarget(zombie);
        }

        /// <summary>
        /// 移动僵尸向目标位置
        /// 包含碰撞检测和避障功能
        /// </summary>
        private void MoveZombieTowardsTarget(ZombieData zombie)
        {
            Vector2 direction = (zombie.targetPosition - zombie.position).normalized;
            float moveDistance = zombie.moveSpeed * ZOMBIE_UPDATE_INTERVAL;
            Vector2 targetPosition = zombie.position + direction * moveDistance;
            
            // 检查目标位置是否可移动
            Vector2 validPosition = FindValidMovePosition(zombie, targetPosition, moveDistance);
            zombie.position = validPosition;
        }
        
        /// <summary>
        /// 寻找有效的移动位置（避障算法）
        /// </summary>
        private Vector2 FindValidMovePosition(ZombieData zombie, Vector2 targetPosition, float moveDistance)
        {
            // 1. 检查目标位置是否有障碍物
            if (IsPositionValid(targetPosition, zombie.id))
            {
                return targetPosition; // 直接移动
            }
            
            // 2. 如果有障碍物，尝试绕行
            Vector2 currentPosition = zombie.position;
            Vector2 originalDirection = (targetPosition - currentPosition).normalized;
            
            // 尝试不同角度的绕行路径
            float[] tryAngles = { -45f, 45f, -90f, 90f, -135f, 135f };
            
            foreach (float angle in tryAngles)
            {
                Vector2 rotatedDirection = RotateVector(originalDirection, angle);
                Vector2 tryPosition = currentPosition + rotatedDirection * moveDistance;
                
                if (IsPositionValid(tryPosition, zombie.id))
                {
                    return tryPosition;
                }
            }
            
            // 3. 如果所有方向都被阻挡，尝试向后退一点点
            Vector2 retreatPosition = currentPosition - originalDirection * (moveDistance * 0.5f);
            if (IsPositionValid(retreatPosition, zombie.id))
            {
                return retreatPosition;
            }
            
            // 4. 最后的选择：保持原地不动
            return currentPosition;
        }
        
        /// <summary>
        /// 检查位置是否有效（无碰撞）
        /// </summary>
        private bool IsPositionValid(Vector2 position, string zombieId)
        {
            const float ZOMBIE_RADIUS = 0.4f; // 僵尸半径
            const float BUILDING_BUFFER = 0.2f; // 建筑周围缓冲区
            
            // 1. 检查地图边界
            if (position.x < -MAP_SIZE/2 || position.x > MAP_SIZE/2 || 
                position.y < -MAP_SIZE/2 || position.y > MAP_SIZE/2)
            {
                return false;
            }
            
            // 2. 检查与建筑的碰撞
            var buildings = mBuildingSystem.GetAllBuildings();
            foreach (var building in buildings)
            {
                if (building.Health <= 0) continue; // 跳过已摧毁的建筑
                
                float distance = Vector2.Distance(position, building.Position);
                float requiredDistance = ZOMBIE_RADIUS + GetBuildingRadius(building.ConfigId) + BUILDING_BUFFER;
                
                if (distance < requiredDistance)
                {
                    return false; // 与建筑碰撞
                }
            }
            
            // 3. 检查与其他僵尸的碰撞
            foreach (var otherZombie in mZombieData.zombies.Values)
            {
                if (otherZombie.id == zombieId || !otherZombie.IsAlive) continue;
                
                float distance = Vector2.Distance(position, otherZombie.position);
                if (distance < ZOMBIE_RADIUS * 2) // 两个僵尸的半径之和
                {
                    return false; // 与其他僵尸碰撞
                }
            }
            
            return true; // 位置有效
        }
        
        /// <summary>
        /// 获取建筑的半径（用于碰撞检测）
        /// </summary>
        private float GetBuildingRadius(string buildingType)
        {
            switch (buildingType)
            {
                case "Shelter": return 1.5f;
                case "Farm": return 1.0f;
                case "Workshop": return 1.2f;
                case "Wall": return 0.8f;
                case "WatchTower": return 1.0f;
                case "MedicalStation": return 1.1f;
                case "Quarry": return 1.5f;
                case "Library": return 1.2f;
                case "StorageDepot": return 1.3f;
                default: return 1.0f;
            }
        }
        
        /// <summary>
        /// 旋转向量
        /// </summary>
        private Vector2 RotateVector(Vector2 vector, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRadians);
            float sin = Mathf.Sin(angleRadians);
            
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }
        
        /// <summary>
        /// 寻找有效的游荡目标位置
        /// </summary>
        private Vector2 FindValidWanderTarget(Vector2 currentPosition)
        {
            const int MAX_ATTEMPTS = 10;
            
            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = UnityEngine.Random.Range(2f, 5f);
                Vector2 targetPosition = currentPosition + new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                
                // 检查目标位置是否有效
                if (IsPositionValid(targetPosition, ""))
                {
                    return targetPosition;
                }
            }
            
            // 如果找不到有效位置，返回当前位置附近的安全位置
            return currentPosition + new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
        }

        /// <summary>
        /// 查找最近的建筑
        /// 用于僵尸寻找攻击目标
        /// </summary>
        private BuildingData FindNearestBuilding(Vector2 position, float detectionRange)
        {
            var buildings = mBuildingSystem.GetAllBuildings();
            BuildingData nearestBuilding = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var building in buildings)
            {
                if (building.Health <= 0) continue; // 跳过已摧毁的建筑
                
                float distance = Vector2.Distance(position, building.Position);
                if (distance <= detectionRange && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestBuilding = building;
                }
            }
            
            return nearestBuilding;
        }

        /// <summary>
        /// 更新僵尸攻击逻辑
        /// 判断是否可以攻击建筑并触发攻击
        /// </summary>
        public void UpdateZombieAttacks()
        {
            foreach (var zombie in mZombieData.zombies.Values)
            {
                if (!zombie.IsAlive || zombie.state != ZombieState.Attacking) continue;
                
                ProcessZombieAttack(zombie);
            }
        }

        /// <summary>
        /// 处理僵尸攻击
        /// 检查攻击冷却、目标是否存在，并执行攻击
        /// </summary>
        private void ProcessZombieAttack(ZombieData zombie)
        {
            // 检查攻击冷却
            if (Time.time - zombie.lastAttackTime < ATTACK_COOLDOWN) return;
            
            // 检查目标建筑
            if (string.IsNullOrEmpty(zombie.targetBuildingId))
            {
                zombie.state = ZombieState.Wandering;
                return;
            }

            // 临时注释掉有问题的代码，等待后续修复
            /*
            var building = mBuildingSystem.GetBuilding(zombie.targetBuildingId);
            if (building == null || building.Health <= 0)
            {
                zombie.state = ZombieState.Wandering;
                zombie.isAttackingBuilding = false;
                zombie.targetBuildingId = null;
                return;
            }

            // 检查攻击距离
            float distance = Vector2.Distance(zombie.position, building.Position);
            if (distance > zombie.attackRange)
            {
                zombie.state = ZombieState.Approaching;
                return;
            }

            // 执行攻击
            PerformZombieAttack(zombie, building);
            */
        }

        /// <summary>
        /// 执行僵尸攻击
        /// 对建筑造成伤害，并发送攻击事件
        /// </summary>
        private void PerformZombieAttack(ZombieData zombie, BuildingData building)
        {
            zombie.lastAttackTime = Time.time;
            
            // 记录攻击数据
            var attackData = new ZombieAttackData(zombie.id, building.Id.ToString(), zombie.attackPower, zombie.position, zombie.type);
            mZombieData.recentAttacks.Add(attackData);
            
            // 更新建筑受攻击记录
            mZombieData.buildingUnderAttack[building.Id.ToString()] = Time.time;
            
            // 临时注释掉实际攻击方法
            // mBuildingSystem.DamageBuilding(building.Id, zombie.attackPower);
            
            Debug.Log($"{zombie.type}僵尸攻击了建筑，造成{zombie.attackPower}点伤害");
            
            // 发送攻击事件
            this.SendEvent(new ZombieAttackBuildingEvent 
            { 
                ZombieId = zombie.id,
                BuildingId = building.Id.ToString(),
                Damage = zombie.attackPower,
                AttackerType = zombie.type
            });
            
            // 处理特殊攻击类型
            ProcessSpecialAttack(zombie, building);
        }

        /// <summary>
        /// 处理特殊僵尸的额外攻击效果
        /// 如喷毒、尖叫吸引其他僵尸等
        /// </summary>
        private void ProcessSpecialAttack(ZombieData zombie, BuildingData building)
        {
            switch (zombie.type)
            {
                case ZombieType.Spitter:
                    // 吐酸攻击，造成持续伤害
                    if (Time.time - zombie.lastAttackTime >= zombie.spitCooldown)
                    {
                        // 临时注释掉
                        // mBuildingSystem.DamageBuilding(building.Id, zombie.attackPower * 0.5f);
                        zombie.spitCooldown = Time.time + 3f;
                    }
                    break;
                    
                case ZombieType.Screamer:
                    // 尖叫吸引附近僵尸
                    if (Time.time - zombie.lastAttackTime >= zombie.screamCooldown)
                    {
                        AttractNearbyZombies(zombie.position, 10f, zombie.targetBuildingId);
                        zombie.screamCooldown = Time.time + 10f;
                    }
                    break;
                    
                case ZombieType.Tank:
                    // 坦克型造成额外结构伤害
                    // 临时注释掉，因为BuildingType.Wall不存在
                    /*
                    if (building.Type == BuildingType.Wall)
                    {
                        mBuildingSystem.DamageBuilding(building.Id, zombie.attackPower * 0.5f);
                    }
                    */
                    break;
            }
        }

        /// <summary>
        /// 吸引附近的僵尸加入攻击
        /// </summary>
        private void AttractNearbyZombies(Vector2 position, float radius, string targetBuildingId)
        {
            int attractedCount = 0;
            foreach (var zombie in mZombieData.zombies.Values)
            {
                if (!zombie.IsAlive || zombie.isAttackingBuilding) continue;
                
                float distance = Vector2.Distance(zombie.position, position);
                if (distance <= radius)
                {
                    zombie.targetBuildingId = targetBuildingId;
                    zombie.state = ZombieState.Approaching;
                    zombie.isAttackingBuilding = true;
                    attractedCount++;
                }
            }
            
            Debug.Log($"尖叫者吸引了{attractedCount}只僵尸攻击建筑");
        }

        /// <summary>
        /// 更新当前威胁等级
        /// 根据僵尸总数、攻击强度等综合判断威胁等级
        /// </summary>
        public void UpdateThreatLevel()
        {
            ZombieThreatLevel newThreatLevel = CalculateNewThreatLevel();
            
            if (newThreatLevel != mZombieData.currentThreatLevel)
            {
                var oldThreatLevel = mZombieData.currentThreatLevel;
                mZombieData.currentThreatLevel = newThreatLevel;
                
                Debug.Log($"威胁等级变化: {oldThreatLevel} -> {newThreatLevel}");
                
                // 发送威胁等级变化事件
                this.SendEvent(new ThreatLevelChangedEvent 
                { 
                    OldLevel = oldThreatLevel, 
                    NewLevel = newThreatLevel,
                    ThreatAccumulation = mZombieData.threatAccumulation
                });
            }
        }

        public ZombieSystemData GetZombieData()
        {
            return mZombieData;
            
            
        }

        /// <summary>
        /// 根据威胁积累量计算新的威胁等级
        /// </summary>
        private ZombieThreatLevel CalculateNewThreatLevel()
        {
            int currentDay = mGameModel.GameDay.Value;
            float accumulation = mZombieData.threatAccumulation;
            
            // 基于威胁积累的等级判定
            if (accumulation < 20f) return ZombieThreatLevel.Safe;
            if (accumulation < 50f) return ZombieThreatLevel.Low;
            if (accumulation < 100f) return ZombieThreatLevel.Medium;
            if (accumulation < 200f) return ZombieThreatLevel.High;
            return ZombieThreatLevel.Extreme;
        }

        /// <summary>
        /// 处理僵尸死亡
        /// 清除状态，播放特效，减少威胁积累
        /// </summary>
        public void ProcessZombieDeath(string zombieId)
        {
            if (mZombieData.zombies.ContainsKey(zombieId))
            {
                var zombie = mZombieData.zombies[zombieId];
                zombie.state = ZombieState.Dead;
                zombie.currentHealth = 0;
                
                Debug.Log($"{zombie.type}僵尸死亡");
                
                // 发送死亡事件
                this.SendEvent(new ZombieDeathEvent { ZombieId = zombieId, Type = zombie.type, Position = zombie.position });
            }
        }

        /// <summary>
        /// 给指定僵尸造成伤害
        /// 如果伤害致死，则调用死亡处理
        /// </summary>
        public void TakeDamageToZombie(string zombieId, float damage)
        {
            if (mZombieData.zombies.ContainsKey(zombieId))
            {
                var zombie = mZombieData.zombies[zombieId];
                zombie.TakeDamage(damage);
                
                if (!zombie.IsAlive)
                {
                    ProcessZombieDeath(zombieId);
                }
            }
        }

        /// <summary>
        /// 获取当前威胁等级
        /// </summary>
        public ZombieThreatLevel GetCurrentThreatLevel() => mZombieData.currentThreatLevel;

        /// <summary>
        /// 获取总生成僵尸数量
        /// </summary>
        public int GetTotalZombieCount() => mZombieData.totalZombiesSpawned;

        /// <summary>
        /// 获取当前存活僵尸数量
        /// </summary>
        public int GetActiveZombieCount() => mZombieData.GetAliveZombieCount();

        /// <summary>
        /// 主更新方法
        /// 实际由OnTimeUpdate驱动，提供统一入口
        /// </summary>
        public void Update()
        {
            // 这个Update方法供外部系统调用，内部逻辑在OnTimeUpdate中处理
        }

        /// <summary>
        /// 获取指定范围内的所有僵尸
        /// </summary>
        public List<ZombieData> GetZombiesNearPosition(Vector2 position, float radius)
        {
            var nearbyZombies = new List<ZombieData>();
            foreach (var zombie in mZombieData.zombies.Values)
            {
                if (zombie.IsAlive && Vector2.Distance(zombie.position, position) <= radius)
                {
                    nearbyZombies.Add(zombie);
                }
            }
            return nearbyZombies;
        }
    }
}

// === 僵尸系统事件定义 ===
namespace MyGameNamespace
{
    /// <summary>
    /// 建筑被摧毁事件
    /// </summary>
    public struct BuildingDestroyedEvent
    {
        public int BuildingId;
        public string BuildingType;
        public Vector3 Position;
    }
    
    /// <summary>
    /// 僵尸生成事件
    /// </summary>
    public struct ZombieSpawnedEvent
    {
        public string ZombieId;
        public ZombieType Type;
        public Vector2 Position;
    }
    
    /// <summary>
    /// 僵尸群生成事件
    /// </summary>
    public struct ZombieHordeSpawnedEvent
    {
        public string HordeId;
        public ZombieThreatLevel ThreatLevel;
        public Vector2 CenterPosition;
        public int ZombieCount;
    }
    
    /// <summary>
    /// 僵尸攻击建筑事件
    /// </summary>
    public struct ZombieAttackBuildingEvent
    {
        public string ZombieId;
        public string BuildingId;
        public float Damage;
        public ZombieType AttackerType;
    }
    
    /// <summary>
    /// 僵尸死亡事件
    /// </summary>
    public struct ZombieDeathEvent
    {
        public string ZombieId;
        public ZombieType Type;
        public Vector2 Position;
    }
    
    /// <summary>
    /// 威胁等级变化事件
    /// </summary>
    public struct ThreatLevelChangedEvent
    {
        public ZombieThreatLevel OldLevel;
        public ZombieThreatLevel NewLevel;
        public float ThreatAccumulation;
    }
}
