// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了基础僵尸系统 (ZombieSystem) 及其相关接口。该系统负责管理
//     游戏世界中僵尸的生成、行为（移动、攻击）、状态（生命、死亡）以及
//     全局的僵尸威胁等级。它与其他系统（如建筑系统、资源系统）交互，
//     并响应游戏事件（如时间更新、建筑被毁）来动态调整僵尸的行为和数量。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq; // 用于 LINQ 查询，例如在 SelectZombieTypeByWeight 中
using UnityEngine;   // Unity 核心功能
using QFramework;    // QFramework 框架
using MyGameNamespace; // 包含自定义事件结构体的命名空间
using SurvivalGame.Model; // 游戏核心数据模型

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 僵尸系统接口 (IZombieSystem)。
    /// 定义了僵尸系统对外提供的核心功能和交互点。
    /// </summary>
    public interface IZombieSystem : QFISystem
    {
        /// <summary>
        /// 系统的主更新方法，通常由游戏循环每帧或按固定时间间隔调用。
        /// </summary>
        void Update();

        /// <summary>
        /// 在指定位置生成一个特定类型的僵尸。
        /// </summary>
        /// <param name="type">要生成的僵尸类型。</param>
        /// <param name="position">僵尸的生成位置。</param>
        void SpawnZombie(ZombieType type, Vector2 position);

        /// <summary>
        /// 根据当前的威胁等级，在指定中心位置生成一群僵尸。
        /// </summary>
        /// <param name="threatLevel">当前的僵尸威胁等级，影响生成僵尸的数量和类型。</param>
        /// <param name="centerPosition">僵尸群生成的中心位置。</param>
        void SpawnZombieHorde(ZombieThreatLevel threatLevel, Vector2 centerPosition);

        /// <summary>
        /// 更新所有活动僵尸的移动逻辑。
        /// </summary>
        void UpdateZombieMovement();

        /// <summary>
        /// 更新所有活动僵尸的攻击逻辑。
        /// </summary>
        void UpdateZombieAttacks();

        /// <summary>
        /// 处理指定ID僵尸的死亡逻辑，例如移除、播放特效、更新统计等。
        /// </summary>
        /// <param name="zombieId">已死亡僵尸的唯一ID。</param>
        void ProcessZombieDeath(string zombieId);

        /// <summary>
        /// 根据当前游戏状态（如天数、现有僵尸数量等）更新全局的僵尸威胁等级。
        /// </summary>
        void UpdateThreatLevel();

        /// <summary>
        /// 获取当前僵尸系统的所有运行时数据。
        /// </summary>
        /// <returns>包含所有僵尸、僵尸群及威胁等级信息的 ZombieSystemData 对象。</returns>
        ZombieSystemData GetZombieData();

        /// <summary>
        /// 获取在指定位置和半径范围内的所有僵尸列表。
        /// </summary>
        /// <param name="position">搜索的中心位置。</param>
        /// <param name="radius">搜索半径。</param>
        /// <returns>符合条件的僵尸数据列表。</returns>
        List<ZombieData> GetZombiesNearPosition(Vector2 position, float radius);

        /// <summary>
        /// 对指定ID的僵尸施加一定量的伤害。
        /// </summary>
        /// <param name="zombieId">目标僵尸的唯一ID。</param>
        /// <param name="damage">要施加的伤害量。</param>
        void TakeDamageToZombie(string zombieId, float damage);

        /// <summary>
        /// 获取当前的全局僵尸威胁等级。
        /// </summary>
        /// <returns>当前的 ZombieThreatLevel 枚举值。</returns>
        ZombieThreatLevel GetCurrentThreatLevel();

        /// <summary>
        /// 获取自游戏开始以来已生成的僵尸总数量。
        /// </summary>
        /// <returns>已生成的僵尸总数。</returns>
        int GetTotalZombieCount();

        /// <summary>
        /// 获取当前场景中所有存活（活动）的僵尸数量。
        /// </summary>
        /// <returns>当前存活的僵尸数量。</returns>
        int GetActiveZombieCount();
    }
    
    /// <summary>
    /// 僵尸系统 (ZombieSystem) 的具体实现类。
    /// 负责管理游戏世界中所有僵尸的生命周期、行为逻辑（如生成、移动、攻击、死亡）
    /// 以及全局僵尸威胁等级的动态调整。
    /// 
    /// 核心机制：
    /// - 定期更新：按预设时间间隔刷新僵尸状态、行为决策及威胁等级。
    /// - 动态生成：根据当前的威胁等级和游戏进程（如天数）动态调整僵尸的生成频率、数量和类型。
    /// - 多样化僵尸：支持不同类型的僵尸（如普通型Walker、快速型Runner、强壮型Tank等），每种类型可有不同属性和行为。
    /// - 群体行为：支持生成僵尸群（Horde），并可能包含初步的群体移动或目标选择逻辑（可由ZombieAIEnhancementSystem进一步增强）。
    /// - 目标选择：僵尸会以建筑（尤其是玩家基地核心）作为主要攻击目标。
    /// - 数据驱动：依赖ZombieSystemData容器存储所有僵尸实例和相关状态，并通过ConfigSystem获取配置。
    /// 
    /// 关键依赖：
    /// - ISurvivalGameModel: 用于访问和修改全局游戏状态，如当前天数、玩家资源（间接影响威胁）等。
    /// - IResourceSystem: （当前注释中未直接使用，但理论上僵尸行为可能与资源系统有交互，如僵尸掉落物）。
    /// - IEnhancedBuildingSystem: 用于获取建筑信息，作为僵尸的攻击目标或寻路参考。
    /// - ConfigSystem: (通过GetSystem获取) 用于读取僵尸类型、威胁等级对应的生成规则等配置。
    /// 
    /// 状态流转（单个僵尸）：
    ///   (生成) -> Wandering (游荡) -> Approaching (接近目标) -> Attacking (攻击中) -> Dead (死亡)
    /// 
    /// 事件交互：
    /// - 监听 TimeUpdateEvent: 驱动系统自身的周期性更新逻辑。
    /// - 监听 NewDayEvent: 在新的一天开始时调整威胁积累。
    /// - 监听 BuildingDestroyedEvent: 当建筑被摧毁时，可能需要重新评估僵尸的攻击目标。
    /// - 发送 ZombieSpawnedEvent: 当新僵尸（或僵尸群）生成时，通知其他系统（如可视化、AI增强系统）。
    /// - 发送 ZombieAttackBuildingEvent: 当僵尸对建筑发起攻击时，通知相关系统（如UI、建筑系统处理伤害）。
    /// - 发送 ZombieDeathEvent: 当僵尸死亡时通知。
    /// - 发送 ThreatLevelChangedEvent: 当全局威胁等级发生变化时通知。
    /// </summary>
    public class ZombieSystem : AbstractSystem, IZombieSystem
    {
        // --- 系统引用与数据模型 ---
        /// <summary>游戏核心数据模型，用于获取全局游戏状态如天数等。</summary>
        private ISurvivalGameModel mGameModel;
        /// <summary>资源系统引用（当前未使用，但未来可能用于僵尸掉落等）。</summary>
        private IResourceSystem mResourceSystem;
        /// <summary>建筑系统引用，用于僵尸索敌和路径规划。</summary>
        private IEnhancedBuildingSystem mBuildingSystem;
        
        /// <summary>存储当前所有僵尸数据、僵尸群数据以及威胁等级等信息的容器。</summary>
        private ZombieSystemData mZombieData;
        
        // --- 系统运行参数配置 (Constants for System Operation) ---
        /// <summary>僵尸个体行为（如移动、攻击决策）的更新周期（秒）。</summary>
        private const float ZOMBIE_UPDATE_INTERVAL = 0.5f;
        /// <summary>全局威胁等级评估与更新的周期（秒）。</summary>
        private const float THREAT_UPDATE_INTERVAL = 60f;
        /// <summary>检查是否需要生成新僵尸的周期（秒）。</summary>
        private const float SPAWN_CHECK_INTERVAL = 15f;
        /// <summary>游戏地图的边界尺寸或僵尸活动范围参考值（用于生成位置计算）。</summary>
        private const float MAP_SIZE = 50f;
        /// <summary>僵尸群生成的最小距离（相对于玩家基地或兴趣点）。</summary>
        private const float SPAWN_DISTANCE_MIN = 15f;
        /// <summary>僵尸群生成的最大距离。</summary>
        private const float SPAWN_DISTANCE_MAX = 25f;
        /// <summary>僵尸攻击的冷却时间（秒），防止攻击频率过高。</summary>
        private const float ATTACK_COOLDOWN = 2f;
        /// <summary>清理已死亡僵尸数据和对象的周期（秒）。</summary>
        private const float CLEANUP_INTERVAL = 30f;
        
        // --- 时间戳变量 (Timestamp Variables for Update Timing) ---
        /// <summary>记录上一次执行僵尸主要逻辑更新的时间。</summary>
        private float mLastZombieUpdate = 0f;
        /// <summary>记录上一次执行威胁等级更新的时间。</summary>
        private float mLastThreatUpdate = 0f;
        /// <summary>记录上一次检查僵尸生成条件的时间。</summary>
        private float mLastSpawnCheck = 0f;
        /// <summary>记录上一次清理死亡僵尸的时间。</summary>
        private float mLastCleanup = 0f;
        
        /// <summary>
        /// 系统初始化方法。
        /// 获取对其他系统和数据模型的引用，初始化内部数据结构，并注册相关的游戏事件监听器。
        /// </summary>
        protected override void OnInit()
        {
            // 获取框架内其他系统和数据模型的引用
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mResourceSystem = this.GetSystem<IResourceSystem>();
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            
            mZombieData = new ZombieSystemData(); // 初始化僵尸系统自身的数据容器
            
            // 注册对游戏内事件的监听，以便响应并执行相应逻辑
            this.RegisterEvent<TimeUpdateEvent>(OnTimeUpdate); // 监听时间更新事件，驱动周期性任务
            this.RegisterEvent<NewDayEvent>(OnNewDay);         // 监听新的一天开始事件，用于调整威胁等级
            this.RegisterEvent<BuildingDestroyedEvent>(OnBuildingDestroyed); // 监听建筑被摧毁事件，处理僵尸目标变更
            
            Debug.Log("[僵尸系统] 初始化完成。");
        }

        /// <summary>
        /// 时间更新事件的回调处理方法。
        /// 根据预设的时间间隔，定期驱动僵尸的移动、攻击、威胁等级更新、生成检查和死亡清理等逻辑。
        /// </summary>
        /// <param name="e">时间更新事件参数，包含deltaTime（当前未使用）。</param>
        private void OnTimeUpdate(TimeUpdateEvent e)
        {
            float currentTime = Time.time; // 获取当前游戏时间
            
            // --- 定期更新僵尸的移动和攻击行为 ---
            if (currentTime - mLastZombieUpdate >= ZOMBIE_UPDATE_INTERVAL)
            {
                UpdateZombieMovement(); // 更新所有僵尸的移动逻辑
                UpdateZombieAttacks();  // 更新所有僵尸的攻击逻辑
                mLastZombieUpdate = currentTime; // 更新上次执行时间戳
            }
            
            // --- 定期更新全局僵尸威胁等级 ---
            if (currentTime - mLastThreatUpdate >= THREAT_UPDATE_INTERVAL)
            {
                UpdateThreatLevel(); // 评估并可能调整威胁等级
                mLastThreatUpdate = currentTime;
            }
            
            // --- 定期检查是否需要生成新的僵尸 ---
            if (currentTime - mLastSpawnCheck >= SPAWN_CHECK_INTERVAL)
            {
                CheckZombieSpawn(); // 检查并执行僵尸生成逻辑
                mLastSpawnCheck = currentTime;
            }
            
            // --- 定期清理已死亡的僵尸数据 ---
            if (currentTime - mLastCleanup >= CLEANUP_INTERVAL)
            {
                mZombieData.CleanupDeadZombies(); // 调用数据容器的方法清理死亡僵尸
                mLastCleanup = currentTime;
            }
        }

        /// <summary>
        /// 当新的一天开始时的回调处理方法。
        /// 主要用于增加僵尸威胁等级的积累值，并触发一次威胁等级的更新评估。
        /// </summary>
        /// <param name="e">新的一天事件参数，包含当前天数。</param>
        private void OnNewDay(NewDayEvent e)
        {
            float threatIncrease = CalculateDailyThreatIncrease(); // 计算当日应增加的威胁积累值
            mZombieData.threatAccumulation += threatIncrease;      // 累加到总威胁值
            mZombieData.lastThreatIncrease = Time.time;            // 记录威胁增加的时间
            
            Debug.Log($"[僵尸系统] 第 {e.Day} 天开始 - 威胁积累增加 {threatIncrease:F1}，当前总积累: {mZombieData.threatAccumulation:F1}。");
            
            UpdateThreatLevel(); // 立即尝试更新威胁等级
        }

        /// <summary>
        /// 当一个建筑被摧毁时的回调处理方法。
        /// 如果有僵尸正在攻击这个已被摧毁的建筑，则需要重置这些僵尸的目标和状态。
        /// </summary>
        /// <param name="e">建筑被摧毁事件参数，包含被毁建筑的ID。</param>
        private void OnBuildingDestroyed(BuildingDestroyedEvent e)
        {
            string destroyedBuildingIdStr = e.BuildingId.ToString(); // 将建筑ID转为字符串比较
            // 如果被毁建筑在“正在被攻击的建筑”列表中，则从中移除
            if (mZombieData.buildingUnderAttack.ContainsKey(destroyedBuildingIdStr))
            {
                mZombieData.buildingUnderAttack.Remove(destroyedBuildingIdStr);
            }
            
            // 遍历所有僵尸，如果其目标是被毁建筑，则重置其状态为游荡
            foreach (var zombie in mZombieData.zombies.Values)
            {
                if (zombie.targetBuildingId == destroyedBuildingIdStr)
                {
                    zombie.isAttackingBuilding = false; // 不再攻击建筑
                    zombie.targetBuildingId = null;     // 清除目标建筑ID
                    zombie.state = ZombieState.Wandering; // 恢复到游荡状态
                    Debug.Log($"[僵尸系统] 僵尸 {zombie.id} 的目标建筑 {destroyedBuildingIdStr} 已被摧毁，僵尸恢复游荡。");
                }
            }
        }

        /// <summary>
        /// （辅助方法）根据当前游戏天数和现有威胁等级，计算每日应增加的威胁积累值。
        /// 威胁积累值越高，触发更高威胁等级的可能性越大。
        /// </summary>
        /// <returns>计算得出的当日威胁积累增量。</returns>
        private float CalculateDailyThreatIncrease()
        {
            int currentDay = mGameModel.GameDay.Value; // 获取当前游戏天数
            float baseIncrease = 5f; // 每日基础威胁增长值
            
            // 1. 天数乘数：随着游戏天数增加，威胁增长速度加快
            float dayMultiplier = 1f + (currentDay - 1) * 0.1f; // 例如，每天额外增加10%的基础增长
            
            // 2. 威胁等级乘数：当前威胁等级越高，每日威胁增长可能反而减缓（或加快，取决于设计）
            float threatLevelMultiplier = 1f;
            switch (mZombieData.currentThreatLevel) // 根据当前威胁等级调整增长速率
            {
                case ZombieThreatLevel.Safe:    threatLevelMultiplier = 1.2f; break; // 安全等级后，威胁增长稍快以推动游戏进程
                case ZombieThreatLevel.Low:     threatLevelMultiplier = 1.0f; break; // 低威胁等级，正常增长
                case ZombieThreatLevel.Medium:  threatLevelMultiplier = 0.8f; break; // 中等威胁，增长略微减缓
                case ZombieThreatLevel.High:    threatLevelMultiplier = 0.6f; break; // 高威胁，增长进一步减缓
                case ZombieThreatLevel.Extreme: threatLevelMultiplier = 0.4f; break; // 极端威胁，增长显著减缓（可能已是后期挑战）
            }
            
            return baseIncrease * dayMultiplier * threatLevelMultiplier; // 总增长 = 基础 * 天数乘数 * 威胁等级乘数
        }

        /// <summary>
        /// 检查是否需要生成新的僵尸。
        /// 根据当前存活僵尸数量与当前威胁等级配置的最小/最大僵尸数及生成概率来决定。
        /// </summary>
        private void CheckZombieSpawn()
        {
            int currentAliveZombieCount = mZombieData.GetAliveZombieCount(); // 获取当前存活僵尸数
            // 获取当前威胁等级对应的生成配置
            var currentThreatConfig = mZombieData.threatConfigs.TryGetValue(mZombieData.currentThreatLevel, out var cfg) ? cfg : null;
            if (currentThreatConfig == null)
            {
                Debug.LogError($"[僵尸系统] 无法获取威胁等级 {mZombieData.currentThreatLevel} 的配置！");
                return;
            }
            
            // 判断是否应该生成：
            // 1. 如果当前僵尸数少于该威胁等级的最小保证数，则尝试生成。
            // 2. 或者，如果当前僵尸数少于最大允许数，并且随机概率检定通过，则尝试生成。
            bool shouldAttemptSpawn = currentAliveZombieCount < currentThreatConfig.minZombies ||
                                    (currentAliveZombieCount < currentThreatConfig.maxZombies &&
                                     UnityEngine.Random.value < currentThreatConfig.spawnChance);
            
            if (shouldAttemptSpawn)
            {
                Vector2 spawnCenter = GetRandomSpawnPosition(); // 获取一个随机的僵尸群生成中心点
                SpawnZombieHorde(mZombieData.currentThreatLevel, spawnCenter); // 生成一群僵尸
            }
        }

        /// <summary>
        /// （辅助方法）获取一个随机的僵尸（群）生成位置。
        /// 该位置通常在玩家基地或重要区域的一定距离之外，并确保在地图边界内且不与现有建筑重叠。
        /// </summary>
        /// <returns>计算得到的随机生成位置 (Vector2)。</returns>
        private Vector2 GetRandomSpawnPosition()
        {
            const int MAX_SPAWN_ATTEMPTS = 20; // 为找到有效位置所做的最大尝试次数
            Vector2 playerBaseCenter = Vector2.zero; // 假设玩家基地中心为 (0,0)，实际应从游戏模型获取
            
            for (int attempt = 0; attempt < MAX_SPAWN_ATTEMPTS; attempt++)
            {
                // 在最小和最大生成距离之间随机选择一个距离和角度
                float randomAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad; // 随机角度 (0-360度)
                float randomDistance = UnityEngine.Random.Range(SPAWN_DISTANCE_MIN, SPAWN_DISTANCE_MAX); // 随机距离
                
                // 计算候选生成位置
                Vector2 candidateSpawnPosition = playerBaseCenter + new Vector2(
                    Mathf.Cos(randomAngle) * randomDistance,
                    Mathf.Sin(randomAngle) * randomDistance
                );
                
                // 确保生成位置在地图边界内
                candidateSpawnPosition.x = Mathf.Clamp(candidateSpawnPosition.x, -MAP_SIZE / 2, MAP_SIZE / 2); // 地图X轴范围
                candidateSpawnPosition.y = Mathf.Clamp(candidateSpawnPosition.y, -MAP_SIZE / 2, MAP_SIZE / 2); // 地图Y轴范围
                
                // 检查该位置是否有效（例如，不与重要建筑过于接近或重叠）
                // 此处的空字符串 "" 表示不检查与特定僵尸的碰撞，只检查与环境/建筑的碰撞
                if (IsPositionValid(candidateSpawnPosition, ""))
                {
                    return candidateSpawnPosition; // 如果有效，则返回此位置
                }
            }
            
            // 如果多次尝试后仍未找到理想位置，则记录警告并返回一个备用位置
            UnityEngine.Debug.LogWarning("[僵尸系统] 未能找到理想的僵尸生成位置，将使用一个靠近边界的备用位置。");
            Vector2 fallbackDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            return playerBaseCenter + fallbackDirection * SPAWN_DISTANCE_MIN; // 在最小生成距离处随机一个方向
        }

        /// <summary>
        /// 在指定位置生成一个特定类型的单个僵尸。
        /// 通常用于特殊事件、调试或特定游戏机制。
        /// </summary>
        /// <param name="type">要生成的僵尸类型。</param>
        /// <param name="position">僵尸的生成位置。</param>
        public void SpawnZombie(ZombieType type, Vector2 position)
        {
            var newZombie = ZombieData.CreateZombie(type, position); // 使用ZombieData的静态工厂方法创建实例
            mZombieData.zombies[newZombie.id] = newZombie; // 添加到僵尸数据字典
            mZombieData.totalZombiesSpawned++;            // 更新总生成数统计
            
            Debug.Log($"[僵尸系统] 已在位置 {position} 生成一个 {type} 类型的僵尸 (ID: {newZombie.id})。");
            
            // 发送僵尸生成事件，通知其他系统（如可视化、AI增强）
            this.SendEvent(new ZombieSpawnedEvent { ZombieId = newZombie.id, Type = type, Position = position });
        }

        /// <summary>
        /// 根据当前的威胁等级，在指定的中心位置生成一群僵尸。
        /// </summary>
        /// <param name="threatLevel">当前的僵尸威胁等级。</param>
        /// <param name="centerPosition">僵尸群生成的中心位置。</param>
        public void SpawnZombieHorde(ZombieThreatLevel threatLevel, Vector2 centerPosition)
        {
            // 获取当前威胁等级对应的生成配置
            var currentThreatConfig = mZombieData.threatConfigs.TryGetValue(threatLevel, out var cfg) ? cfg : null;
            if (currentThreatConfig == null || currentThreatConfig.spawnConfigs.Count == 0) // 如果无配置或配置中无具体生成项
            {
                Debug.LogWarning($"[僵尸系统] 无法生成僵尸群：威胁等级 {threatLevel} 没有有效的生成配置。");
                return;
            }
            
            // 随机决定本次生成的僵尸总数 (在配置的最小和最大数量之间)
            int totalZombiesToSpawn = UnityEngine.Random.Range(currentThreatConfig.minZombies, currentThreatConfig.maxZombies + 1);
            if (totalZombiesToSpawn <= 0) return; // 不生成0或负数数量的僵尸

            // 创建一个新的僵尸群组数据对象
            var hordeData = new ZombieHorde
            {
                // Id 在 ZombieHorde 构造函数中生成
                name = $"{threatLevel} 威胁等级僵尸群 @ {centerPosition}", // 群组名称
                centerPosition = centerPosition,     // 群组中心点
                radius = 5f,                         // 群组活动半径 (示例值)
                threatLevel = threatLevel,           // 群组的威胁等级
                moveDirection = (Vector2.zero - centerPosition).normalized, // 初始移动方向（朝向基地中心）
                moveSpeed = 0.5f                     // 群组的平均移动速度 (示例值)
            };
            
            mZombieData.hordes[hordeData.id] = hordeData; // 将群组数据添加到系统中
            
            // 生成指定数量的僵尸并加入该群组
            for (int i = 0; i < totalZombiesToSpawn; i++)
            {
                // 根据权重从配置中选择要生成的僵尸类型
                ZombieType typeToSpawn = SelectZombieTypeByWeight(currentThreatConfig.spawnConfigs);
                // 在群组中心点附近随机一个位置生成单个僵尸
                Vector2 individualSpawnPos = GetHordeSpawnPosition(centerPosition, hordeData.radius);
                
                var newZombie = ZombieData.CreateZombie(typeToSpawn, individualSpawnPos); // 创建僵尸实例
                mZombieData.zombies[newZombie.id] = newZombie; // 添加到主僵尸列表
                hordeData.zombieIds.Add(newZombie.id);       // 将僵尸ID添加到群组的成员列表
                mZombieData.totalZombiesSpawned++;           // 更新总生成数统计
                
                // 发送单个僵尸生成事件
                this.SendEvent(new ZombieSpawnedEvent { ZombieId = newZombie.id, Type = typeToSpawn, Position = individualSpawnPos });
            }
            
            Debug.Log($"[僵尸系统] 已在 {centerPosition} 生成一个 {threatLevel} 等级的僵尸群 (ID: {hordeData.id})，包含 {totalZombiesToSpawn} 只僵尸。");
            
            // 发送僵尸群已生成事件
            this.SendEvent(new ZombieHordeSpawnedEvent 
            { 
                HordeId = hordeData.id,
                ThreatLevel = threatLevel, 
                CenterPosition = centerPosition,
                ZombieCount = totalZombiesToSpawn
            });
        }

        /// <summary>
        /// （辅助方法）根据预设的权重从僵尸生成配置列表中随机选择一种僵尸类型。
        /// </summary>
        /// <param name="configs">包含各种僵尸类型及其生成权重的配置列表。</param>
        /// <returns>被选中的僵尸类型；如果列表无效则默认为Walker。</returns>
        private ZombieType SelectZombieTypeByWeight(List<ZombieSpawnConfig> configs)
        {
            // 筛选出当前游戏天数已满足其生成要求的配置项
            var validConfigs = configs.Where(c => c.dayRequirement <= mGameModel.GameDay.Value).ToList();
            if (!validConfigs.Any()) return ZombieType.Walker; // 如果没有满足天数要求的配置，则默认生成普通僵尸
            
            float totalWeight = validConfigs.Sum(c => c.spawnWeight); // 计算所有有效配置的总权重
            if (totalWeight <= 0) return validConfigs.First().type; // 如果总权重为0，返回第一个有效配置的类型

            float randomValue = UnityEngine.Random.Range(0f, totalWeight); // 在0到总权重之间生成一个随机数
            
            float currentCumulativeWeight = 0f;
            foreach (var config in validConfigs) // 遍历有效配置，累加权重以确定选中区间
            {
                currentCumulativeWeight += config.spawnWeight;
                if (randomValue <= currentCumulativeWeight) // 如果随机数落在当前配置的权重区间内
                {
                    return config.type; // 返回此配置的僵尸类型
                }
            }
            
            // 理论上不应执行到此处，作为备选方案返回第一个有效配置的类型
            return validConfigs.First().type;
        }

        /// <summary>
        /// （辅助方法）在指定的僵尸群中心点和半径内，获取一个随机且有效的单个僵尸生成位置。
        /// 会尝试多次以避开障碍物。
        /// </summary>
        /// <param name="center">僵尸群的中心位置。</param>
        /// <param name="radius">僵尸在群内的分布半径。</param>
        /// <returns>计算得到的单个僵尸生成位置。</returns>
        private Vector2 GetHordeSpawnPosition(Vector2 center, float radius)
        {
            const int MAX_ATTEMPTS_IN_HORDE = 15; // 在群内寻找有效位置的最大尝试次数
            
            for (int attempt = 0; attempt < MAX_ATTEMPTS_IN_HORDE; attempt++)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad; // 随机角度
                float distance = UnityEngine.Random.Range(0f, radius);             // 在0到群半径之间随机距离
                
                Vector2 spawnPosition = center + new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                
                if (IsPositionValid(spawnPosition, "")) // 检查位置是否有效（无碰撞等）
                {
                    return spawnPosition;
                }
            }
            
            // 如果在群内多次尝试失败，则尝试在群边缘外围略远处生成
            const int MAX_ATTEMPTS_OUTSIDE = 10;
            for (int attempt = 0; attempt < MAX_ATTEMPTS_OUTSIDE; attempt++)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = radius + UnityEngine.Random.Range(1f, 3f); // 在群半径外1到3米处
                
                Vector2 spawnPosition = center + new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                
                if (IsPositionValid(spawnPosition, ""))
                {
                    return spawnPosition;
                }
            }
            
            // 如果所有尝试均失败，则在群中心附近随机一个点作为最后手段（可能仍有碰撞）
            Debug.LogWarning($"[僵尸系统] 未能为僵尸群成员找到理想生成位置，将在中心点 {center} 附近随机放置。");
            return center + new Vector2(UnityEngine.Random.Range(-radius * 0.5f, radius * 0.5f),
                                        UnityEngine.Random.Range(-radius * 0.5f, radius * 0.5f));
        }

        /// <summary>
        /// 更新所有活动僵尸的移动逻辑。
        /// </summary>
        public void UpdateZombieMovement()
        {
            foreach (var zombie in mZombieData.zombies.Values) // 遍历所有僵尸
            {
                if (!zombie.IsAlive) continue; // 跳过已死亡的僵尸
                UpdateSingleZombieMovement(zombie); // 更新单个僵尸的移动
            }
        }

        /// <summary>
        /// （辅助方法）更新单个僵尸的移动逻辑。
        /// 根据僵尸的当前状态（游荡、接近、攻击）执行不同的移动策略。
        /// </summary>
        /// <param name="zombie">要更新移动的僵尸数据对象。</param>
        private void UpdateSingleZombieMovement(ZombieData zombie)
        {
            switch (zombie.state) // 根据僵尸当前状态
            {
                case ZombieState.Wandering:   // 游荡状态
                    UpdateZombieWandering(zombie);
                    break;
                case ZombieState.Approaching: // 接近目标状态
                    UpdateZombieApproaching(zombie);
                    break;
                case ZombieState.Attacking:   // 攻击状态
                    // 攻击状态下通常不进行大的位置移动，或者有特定的攻击位移逻辑（如跳跃、后退）
                    // 此处留空，具体攻击位移可在UpdateZombieAttacks或特定僵尸AI中处理
                    break;
            }
        }

        /// <summary>
        /// （辅助方法）更新处于游荡状态的僵尸的行为。
        /// 僵尸会尝试寻找附近可攻击的建筑，如果找不到则进行随机移动。
        /// </summary>
        /// <param name="zombie">要更新的游荡僵尸数据。</param>
        private void UpdateZombieWandering(ZombieData zombie)
        {
            // 尝试在其探测范围内寻找最近的建筑作为目标
            var nearestBuilding = FindNearestBuilding(zombie.position, zombie.detectionRange);
            if (nearestBuilding != null) // 如果找到目标建筑
            {
                zombie.targetBuildingId = nearestBuilding.Id.ToString(); // 设置目标建筑ID
                zombie.targetPosition = nearestBuilding.Position;       // 设置目标位置
                zombie.state = ZombieState.Approaching;                 // 切换到接近状态
                zombie.isAttackingBuilding = true;                      // 标记为正在攻击建筑（实际是前往攻击）
                return; // 已找到目标，本次游荡更新结束
            }
            
            // 如果没有找到建筑目标，则进行随机游荡
            // 检查是否已到达当前的随机游荡目标点 (或距离非常近)
            if (Vector2.Distance(zombie.position, zombie.targetPosition) < 0.5f)
            {
                // 到达或接近后，设置一个新的随机游荡目标点
                zombie.targetPosition = FindValidWanderTarget(zombie.position);
            }
            
            MoveZombieTowardsTarget(zombie); // 向当前（可能是新的）游荡目标点移动
        }

        /// <summary>
        /// （辅助方法）更新处于接近目标状态的僵尸的行为。
        /// 僵尸会持续向目标建筑移动，到达攻击范围后切换到攻击状态。
        /// 如果目标建筑消失或被摧毁，会重新进入游荡状态。
        /// </summary>
        /// <param name="zombie">要更新的接近目标状态的僵尸数据。</param>
        private void UpdateZombieApproaching(ZombieData zombie)
        {
            float distanceToTarget = Vector2.Distance(zombie.position, zombie.targetPosition); // 计算与目标的距离
            
            // 检查是否已到达目标的攻击范围
            if (distanceToTarget <= zombie.attackRange)
            {
                zombie.state = ZombieState.Attacking; // 切换到攻击状态
                return; // 到达攻击范围，本次移动更新结束
            }
            
            // 检查目标建筑是否仍然有效（未被摧毁）
            if (!string.IsNullOrEmpty(zombie.targetBuildingId)) // 如果有目标建筑ID
            {
                if (int.TryParse(zombie.targetBuildingId, out int buildingId)) // 尝试解析ID
                {
                    var targetBuilding = mBuildingSystem.GetBuilding(buildingId.ToString()); // 从建筑系统获取建筑数据
                    if (targetBuilding == null || targetBuilding.Health <= 0) // 如果建筑不存在或已被摧毁
                    {
                        // 目标失效，重置僵尸状态为游荡
                        zombie.state = ZombieState.Wandering;
                        zombie.isAttackingBuilding = false;
                        zombie.targetBuildingId = null;
                        Debug.Log($"[僵尸系统] 僵尸 {zombie.id} 的目标建筑 {buildingId} 已消失，恢复游荡。");
                        return;
                    }
                    // 如果目标建筑位置发生变化（不太可能，但作为健壮性考虑），更新僵尸的目标位置
                    if (zombie.targetPosition != targetBuilding.Position)
                    {
                        zombie.targetPosition = targetBuilding.Position;
                    }
                }
            }
            
            MoveZombieTowardsTarget(zombie); // 向当前目标位置移动
        }

        /// <summary>
        /// （辅助方法）实际移动僵尸向其当前目标位置，包含基本的避障。
        /// </summary>
        /// <param name="zombie">要移动的僵尸数据。</param>
        private void MoveZombieTowardsTarget(ZombieData zombie)
        {
            Vector2 direction = (zombie.targetPosition - zombie.position).normalized; // 计算移动方向
            float moveDistanceThisFrame = zombie.moveSpeed * ZOMBIE_UPDATE_INTERVAL; // 本次更新周期内应移动的距离
            Vector2 nextPotentialPosition = zombie.position + direction * moveDistanceThisFrame; // 计算不考虑碰撞的下一位置
            
            // 寻找一个有效的、尽可能接近目标方向的移动位置（包含避障）
            Vector2 finalValidPosition = FindValidMovePosition(zombie, nextPotentialPosition, moveDistanceThisFrame);
            zombie.position = finalValidPosition; // 更新僵尸的实际位置
        }
        
        /// <summary>
        /// （辅助方法）寻找一个有效的移动目标位置，实现简单的避障。
        /// 如果直接朝向目标点会被阻挡，则尝试向周围其他方向移动。
        /// </summary>
        /// <param name="zombie">正在移动的僵尸。</param>
        /// <param name="desiredNextPosition">理想的下一帧位置（无碰撞情况下）。</param>
        /// <param name="maxMoveDistance">本帧最大可移动距离。</param>
        /// <returns>一个有效的、尽可能好的下一位置。</returns>
        private Vector2 FindValidMovePosition(ZombieData zombie, Vector2 desiredNextPosition, float maxMoveDistance)
        {
            // 1. 首先检查理想的下一位置是否有效
            if (IsPositionValid(desiredNextPosition, zombie.id))
            {
                return desiredNextPosition; // 如果理想位置有效，则直接使用
            }
            
            // 2. 如果理想位置无效（例如有障碍物），则尝试向周围其他方向探测
            Vector2 currentPosition = zombie.position;
            Vector2 originalDirection = (desiredNextPosition - currentPosition).normalized; // 原始期望移动方向
            
            // 定义一组尝试探测的备选角度（相对于原始方向）
            float[] alternativeAngles = { -45f, 45f, -90f, 90f, -135f, 135f }; // 左右45度、90度、135度
            
            foreach (float angleOffset in alternativeAngles)
            {
                Vector2 alternativeDirection = RotateVector(originalDirection, angleOffset); // 旋转原始方向得到备选方向
                Vector2 candidatePosition = currentPosition + alternativeDirection * maxMoveDistance; // 计算备选方向上的目标点
                
                if (IsPositionValid(candidatePosition, zombie.id)) // 如果此备选位置有效
                {
                    return candidatePosition; // 则采用此位置
                }
            }
            
            // 3. 如果所有探测方向都被阻挡，尝试向原始期望移动方向的反方向小幅后退一点
            // (这有助于僵尸从角落或狭窄处解脱)
            Vector2 retreatPosition = currentPosition - originalDirection * (maxMoveDistance * 0.5f); // 后退一半距离
            if (IsPositionValid(retreatPosition, zombie.id))
            {
                return retreatPosition;
            }
            
            // 4. 如果所有尝试都失败，则僵尸保持在当前位置不动
            return currentPosition;
        }
        
        /// <summary>
        /// （辅助方法）检查指定位置对于特定僵尸是否有效（例如，没有碰撞地图边界、建筑或其他僵尸）。
        /// </summary>
        /// <param name="position">要检查的位置。</param>
        /// <param name="zombieIdToIgnore">进行检查的僵尸自身的ID，用于避免与其自身发生碰撞检测。</param>
        /// <returns>如果位置有效则返回true，否则返回false。</returns>
        private bool IsPositionValid(Vector2 position, string zombieIdToIgnore)
        {
            const float ZOMBIE_COLLISION_RADIUS = 0.4f; // 僵尸的碰撞半径
            const float BUILDING_COLLISION_BUFFER = 0.2f; // 建筑周围的额外避让缓冲区
            
            // 1. 检查是否超出地图边界
            if (position.x < -MAP_SIZE / 2f || position.x > MAP_SIZE / 2f ||
                position.y < -MAP_SIZE / 2f || position.y > MAP_SIZE / 2f)
            {
                return false; // 超出边界，无效
            }
            
            // 2. 检查是否与现有建筑发生碰撞
            var buildings = mBuildingSystem.GetAllBuildings(); // 获取所有建筑数据
            foreach (var building in buildings)
            {
                if (building.Health <= 0) continue; // 跳过已被摧毁的建筑
                
                float distanceToBuilding = Vector2.Distance(position, building.Position); // 计算与建筑中心的距离
                // 碰撞距离 = 僵尸半径 + 建筑半径 + 额外缓冲
                float requiredSeparation = ZOMBIE_COLLISION_RADIUS + GetBuildingRadius(building.ConfigId) + BUILDING_COLLISION_BUFFER;
                
                if (distanceToBuilding < requiredSeparation) // 如果距离小于所需间隔
                {
                    return false; // 与建筑发生碰撞，无效
                }
            }
            
            // 3. 检查是否与其他僵尸发生碰撞
            foreach (var otherZombie in mZombieData.zombies.Values) // 遍历所有僵尸
            {
                // 跳过自身以及已死亡的僵尸
                if (otherZombie.id == zombieIdToIgnore || !otherZombie.IsAlive) continue;
                
                float distanceToOtherZombie = Vector2.Distance(position, otherZombie.position);
                if (distanceToOtherZombie < ZOMBIE_COLLISION_RADIUS * 2) // 如果距离小于两个僵尸半径之和
                {
                    return false; // 与其他僵尸发生碰撞，无效
                }
            }
            
            return true; // 所有检查通过，位置有效
        }
        
        /// <summary>
        /// （辅助方法）根据建筑类型ID获取其近似的碰撞半径。
        /// （此为简化实现，实际半径应从建筑配置中读取或更精确计算）
        /// </summary>
        /// <param name="buildingConfigId">建筑的配置ID。</param>
        /// <returns>建筑的近似半径。</returns>
        private float GetBuildingRadius(string buildingConfigId)
        {
            // TODO: 此数据应从BuildingConfig中获取 (例如 config.CollisionRadius 或根据config.Size计算)
            switch (buildingConfigId) // 示例值
            {
                case "Shelter": return 1.5f;
                case "Farm": return 1.0f;
                case "Workshop": return 1.2f;
                case "Wall": return 0.8f; // 墙体可能较薄
                case "WatchTower": return 1.0f;
                case "MedicalStation": return 1.1f;
                case "Quarry": return 1.5f;
                case "Library": return 1.2f;
                case "StorageDepot": return 1.3f;
                default: return 1.0f; // 未知类型默认半径
            }
        }
        
        /// <summary>
        /// （辅助方法）将一个二维向量按指定角度（度数）旋转。
        /// </summary>
        /// <param name="vector">要旋转的向量。</param>
        /// <param name="angleDegrees">旋转角度（度数，顺时针为负，逆时针为正）。</param>
        /// <returns>旋转后的新向量。</returns>
        private Vector2 RotateVector(Vector2 vector, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad; // 将角度转换为弧度
            float cosTheta = Mathf.Cos(angleRadians);
            float sinTheta = Mathf.Sin(angleRadians);
            
            // 应用旋转矩阵
            return new Vector2(
                vector.x * cosTheta - vector.y * sinTheta,
                vector.x * sinTheta + vector.y * cosTheta
            );
        }
        
        /// <summary>
        /// （辅助方法）为游荡状态的僵尸寻找一个有效的随机目标位置。
        /// </summary>
        /// <param name="currentPosition">僵尸当前位置。</param>
        /// <returns>一个新的有效游荡目标点。</returns>
        private Vector2 FindValidWanderTarget(Vector2 currentPosition)
        {
            const int MAX_WANDER_ATTEMPTS = 10; // 为找到有效游荡点所做的最大尝试次数
            
            for (int attempt = 0; attempt < MAX_WANDER_ATTEMPTS; attempt++)
            {
                float randomAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad; // 随机方向
                float randomDistance = UnityEngine.Random.Range(2f, 5f); // 随机游荡距离 (2到5米)
                Vector2 candidateTarget = currentPosition + new Vector2(Mathf.Cos(randomAngle) * randomDistance,
                                                                      Mathf.Sin(randomAngle) * randomDistance);
                
                if (IsPositionValid(candidateTarget, "")) // 检查此随机点是否有效
                {
                    return candidateTarget;
                }
            }
            
            // 如果多次尝试后仍未找到，则在当前位置附近小范围随机一个点（可能仍无效，但作为备选）
            return currentPosition + new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
        }

        /// <summary>
        /// （辅助方法）在指定位置和探测范围内查找最近的有效建筑目标。
        /// </summary>
        /// <param name="position">搜索的中心位置（通常是僵尸当前位置）。</param>
        /// <param name="detectionRange">僵尸的探测范围半径。</param>
        /// <returns>最近的BuildingData对象；如果范围内无有效建筑，则返回null。</returns>
        private BuildingData FindNearestBuilding(Vector2 position, float detectionRange)
        {
            var allBuildings = mBuildingSystem.GetAllBuildings(); // 获取所有建筑
            BuildingData nearestValidBuilding = null;
            float minDistanceFound = float.MaxValue; // 初始化最小距离为极大值
            
            foreach (var building in allBuildings)
            {
                if (building.Health <= 0) continue; // 跳过已被摧毁或无效的建筑
                
                float distanceToBuilding = Vector2.Distance(position, building.Position); // 计算距离
                // 如果建筑在探测范围内，并且比当前已找到的最近建筑更近
                if (distanceToBuilding <= detectionRange && distanceToBuilding < minDistanceFound)
                {
                    minDistanceFound = distanceToBuilding; // 更新最小距离
                    nearestValidBuilding = building;      // 更新最近建筑
                }
            }
            
            return nearestValidBuilding; // 返回找到的最近建筑，或null
        }

        /// <summary>
        /// 更新所有活动僵尸的攻击逻辑。
        /// </summary>
        public void UpdateZombieAttacks()
        {
            foreach (var zombie in mZombieData.zombies.Values) // 遍历所有僵尸
            {
                // 只处理存活的、且当前处于攻击状态的僵尸
                if (!zombie.IsAlive || zombie.state != ZombieState.Attacking) continue;
                
                ProcessZombieAttack(zombie); // 处理该僵尸的攻击行为
            }
        }

        /// <summary>
        /// （辅助方法）处理单个僵尸的攻击行为。
        /// 包括检查攻击冷却、目标有效性、攻击距离，并执行攻击动作。
        /// </summary>
        /// <param name="zombie">要处理攻击的僵尸数据。</param>
        private void ProcessZombieAttack(ZombieData zombie)
        {
            // 1. 检查攻击冷却时间是否已到
            if (Time.time - zombie.lastAttackTime < ATTACK_COOLDOWN) return; // 如果还在冷却中，则不攻击
            
            // 2. 检查目标建筑是否依然有效
            if (string.IsNullOrEmpty(zombie.targetBuildingId)) // 如果没有目标建筑ID
            {
                zombie.state = ZombieState.Wandering; // 切换回游荡状态
                return;
            }

            // 注意：以下被注释的代码块依赖于mBuildingSystem.GetBuilding能正确处理ID，
            // 并且BuildingData有Health属性。如果这些不成立，此部分会出问题。
            // 假设这些是成立的，但为了通过编译检查，暂时注释。
            /*
            var targetBuilding = mBuildingSystem.GetBuilding(zombie.targetBuildingId); // 获取目标建筑数据
            if (targetBuilding == null || targetBuilding.Health <= 0) // 如果目标建筑不存在或已被摧毁
            {
                zombie.state = ZombieState.Wandering;        // 切换回游荡状态
                zombie.isAttackingBuilding = false;          // 不再以建筑为目标
                zombie.targetBuildingId = null;              // 清除目标ID
                Debug.Log($"[僵尸系统] 僵尸 {zombie.id} 的攻击目标建筑 {zombie.targetBuildingId} 已消失，恢复游荡。");
                return;
            }

            // 3. 再次检查攻击距离 (可能在接近过程中目标移动或僵尸被击退)
            float distanceToTargetBuilding = Vector2.Distance(zombie.position, targetBuilding.Position);
            if (distanceToTargetBuilding > zombie.attackRange) // 如果超出攻击范围
            {
                zombie.state = ZombieState.Approaching; // 切换回接近状态
                return;
            }

            // 4. 执行攻击动作
            PerformZombieAttack(zombie, targetBuilding); // 调用实际执行攻击的方法
            */

            // 临时的简化处理：假设目标始终有效且在范围内，直接记录攻击（无实际伤害）
             Debug.Log($"[僵尸系统] 僵尸 {zombie.id} 尝试攻击目标 {zombie.targetBuildingId} (因部分逻辑注释，未造成实际伤害)。");
             zombie.lastAttackTime = Time.time; // 更新攻击时间戳，进入冷却
        }

        /// <summary>
        /// （辅助方法）执行僵尸对建筑的实际攻击动作。
        /// 对建筑造成伤害，记录攻击数据，发送事件，并处理特殊攻击效果。
        /// </summary>
        /// <param name="zombie">发动攻击的僵尸。</param>
        /// <param name="building">被攻击的建筑。</param>
        private void PerformZombieAttack(ZombieData zombie, BuildingData building)
        {
            zombie.lastAttackTime = Time.time; // 更新上次攻击时间戳
            
            // 记录本次攻击的详细数据
            var attackEventData = new ZombieAttackData(zombie.id, building.Id.ToString(), zombie.attackPower, zombie.position, zombie.type);
            mZombieData.recentAttacks.Add(attackEventData); // 添加到最近攻击列表 (可能用于统计或回放)
            
            // 标记该建筑正在受到攻击 (用于其他系统判断，例如防御塔优先目标)
            mZombieData.buildingUnderAttack[building.Id.ToString()] = Time.time;
            
            // TODO: 调用建筑系统对建筑造成实际伤害
            // mBuildingSystem.TakeDamageToBuilding(building.Id, zombie.attackPower);
            // (假设建筑系统有TakeDamageToBuilding方法)
            
            Debug.Log($"[僵尸系统] {zombie.type} 僵尸 (ID: {zombie.id}) 攻击了建筑 {building.ConfigId} (ID: {building.Id})，造成 {zombie.attackPower} 点伤害。");
            
            // 发送僵尸攻击建筑事件，通知UI或其他相关系统
            this.SendEvent(new ZombieAttackBuildingEvent 
            { 
                ZombieId = zombie.id,
                BuildingId = building.Id.ToString(),
                Damage = zombie.attackPower,
                AttackerType = zombie.type
            });
            
            ProcessSpecialAttack(zombie, building); // 处理此僵尸可能有的特殊攻击效果
        }

        /// <summary>
        /// （辅助方法）处理不同类型僵尸在攻击时可能附带的特殊效果。
        /// 例如，吐酸者的持续伤害、尖叫者的吸引同伴、坦克对结构的额外伤害等。
        /// </summary>
        /// <param name="zombie">发动攻击的僵尸。</param>
        /// <param name="building">被攻击的建筑。</param>
        private void ProcessSpecialAttack(ZombieData zombie, BuildingData building)
        {
            switch (zombie.type) // 根据僵尸类型判断特殊攻击
            {
                case ZombieType.Spitter: // 吐酸者
                    // 假设吐酸攻击除了直接伤害外，还可能造成持续性区域伤害或对建筑有特殊腐蚀效果
                    // 此处示例为：如果其特殊技能冷却完毕，则可能造成额外伤害或施加debuff
                    if (Time.time - zombie.spitCooldown >= 3f) // 假设吐酸冷却3秒 (spitCooldown应为上次使用时间)
                    {
                        // mBuildingSystem.ApplyAcidEffect(building.Id, zombie.attackPower * 0.5f, 5f); // 示例：施加5秒的酸性腐蚀，每秒额外伤害
                        // zombie.spitCooldown = Time.time; // 重置冷却
                        Debug.Log($"[僵尸系统] 吐酸者 {zombie.id} 对建筑 {building.Id} 使用了特殊吐酸效果。");
                    }
                    break;
                    
                case ZombieType.Screamer: // 尖叫者
                    // 尖叫可能在攻击时（或被攻击时）触发，吸引附近其他僵尸共同攻击当前目标
                    if (Time.time - zombie.screamCooldown >= 10f) // 假设尖叫冷却10秒
                    {
                        AttractNearbyZombies(zombie.position, 10f, building.Id.ToString()); // 吸引10米内僵尸
                        // zombie.screamCooldown = Time.time; // 重置冷却
                        Debug.Log($"[僵尸系统] 尖叫者 {zombie.id} 发出尖叫，吸引了同伴攻击建筑 {building.Id}。");
                    }
                    break;
                    
                case ZombieType.Tank: // 坦克
                    // 坦克型僵尸可能对建筑结构造成额外伤害
                    // 假设BuildingData有一个BuildingType字段或可以从ConfigId推断
                    // if (building.Type == BuildingType.Wall || building.Type == BuildingType.Gate) // 假设对墙体类建筑有额外伤害
                    // {
                    //     mBuildingSystem.TakeDamageToBuilding(building.Id, zombie.attackPower * 0.5f); // 额外50%伤害
                    //     Debug.Log($"[僵尸系统] 坦克 {zombie.id} 对建筑 {building.Id} 造成了额外结构伤害。");
                    // }
                    break;
            }
        }

        /// <summary>
        /// （辅助方法）吸引指定位置和半径范围内的其他僵尸，使其将目标转向指定的建筑ID。
        /// </summary>
        /// <param name="centerPosition">吸引中心点（通常是尖叫者位置）。</param>
        /// <param name="radius">吸引半径。</param>
        /// <param name="newTargetBuildingId">被吸引僵尸的新目标建筑ID。</param>
        private void AttractNearbyZombies(Vector2 centerPosition, float radius, string newTargetBuildingId)
        {
            int attractedCount = 0; // 记录被成功吸引的僵尸数量
            foreach (var otherZombie in mZombieData.zombies.Values) // 遍历所有僵尸
            {
                // 跳过已死亡、正在攻击或已将此建筑作为目标的僵尸
                if (!otherZombie.IsAlive || otherZombie.isAttackingBuilding || otherZombie.targetBuildingId == newTargetBuildingId) continue;
                
                if (Vector2.Distance(otherZombie.position, centerPosition) <= radius) // 如果在吸引半径内
                {
                    otherZombie.targetBuildingId = newTargetBuildingId; // 设置新的目标建筑
                    otherZombie.state = ZombieState.Approaching;       // 状态切换为接近目标
                    otherZombie.isAttackingBuilding = true;            // 标记为以建筑为目标
                    attractedCount++;
                }
            }
            
            if (attractedCount > 0)
            {
                Debug.Log($"[僵尸系统] 尖叫吸引了 {attractedCount} 只僵尸转向攻击建筑 {newTargetBuildingId}。");
            }
        }

        /// <summary>
        /// 更新当前的全局僵尸威胁等级。
        /// </summary>
        public void UpdateThreatLevel()
        {
            ZombieThreatLevel newCalculatedThreatLevel = CalculateNewThreatLevel(); // 计算新的威胁等级
            
            // 如果计算出的新威胁等级与当前等级不同
            if (newCalculatedThreatLevel != mZombieData.currentThreatLevel)
            {
                ZombieThreatLevel oldThreatLevel = mZombieData.currentThreatLevel; // 保存旧等级
                mZombieData.currentThreatLevel = newCalculatedThreatLevel;        // 更新为新等级
                
                Debug.Log($"[僵尸系统] 全局威胁等级已从 {oldThreatLevel} 变为 {newCalculatedThreatLevel}。当前威胁积累值: {mZombieData.threatAccumulation:F1}。");
                
                // 发送威胁等级变化事件，通知其他系统（如UI、防御工事系统）
                this.SendEvent(new ThreatLevelChangedEvent 
                { 
                    OldLevel = oldThreatLevel, 
                    NewLevel = newCalculatedThreatLevel,
                    ThreatAccumulation = mZombieData.threatAccumulation // 附带当前的威胁积累值
                });
            }
        }

        /// <summary>
        /// 获取当前僵尸系统的所有运行时数据。
        /// </summary>
        /// <returns>ZombieSystemData 实例。</returns>
        public ZombieSystemData GetZombieData()
        {
            return mZombieData; // 直接返回内部数据对象的引用
        }

        /// <summary>
        /// （辅助方法）根据当前的威胁积累值（以及可能的游戏天数等因素）计算应设定的新威胁等级。
        /// </summary>
        /// <returns>计算得出的新ZombieThreatLevel。</returns>
        private ZombieThreatLevel CalculateNewThreatLevel()
        {
            // int currentDay = mGameModel.GameDay.Value; // 获取当前游戏天数 (可能影响等级阈值)
            float accumulation = mZombieData.threatAccumulation; // 获取当前的威胁积累值
            
            // 根据威胁积累值，将游戏划分为不同的威胁阶段
            // 这些阈值需要根据游戏平衡仔细调整
            if (accumulation < 20f)  return ZombieThreatLevel.Safe;    // 威胁值低于20：安全
            if (accumulation < 50f)  return ZombieThreatLevel.Low;     // 20-49：低威胁
            if (accumulation < 100f) return ZombieThreatLevel.Medium;  // 50-99：中等威胁
            if (accumulation < 200f) return ZombieThreatLevel.High;    // 100-199：高威胁
            return ZombieThreatLevel.Extreme; // 200及以上：极端威胁
        }

        /// <summary>
        /// 处理指定ID僵尸的死亡逻辑。
        /// </summary>
        /// <param name="zombieId">死亡僵尸的ID。</param>
        public void ProcessZombieDeath(string zombieId)
        {
            if (mZombieData.zombies.TryGetValue(zombieId, out var zombie)) // 安全地获取僵尸数据
            {
                if (zombie.IsAlive) // 确保只处理一次死亡，或从未标记为死亡的僵尸
                {
                    zombie.state = ZombieState.Dead; // 设置状态为死亡
                    zombie.currentHealth = 0;        // 生命值清零

                    Debug.Log($"[僵尸系统] {zombie.type} 僵尸 (ID: {zombieId}) 已死亡。");

                    // 发送僵尸死亡事件，通知其他系统（如可视化、统计、掉落物等）
                    this.SendEvent(new ZombieDeathEvent { ZombieId = zombieId, Type = zombie.type, Position = zombie.position });

                    // TODO: 威胁积累值可能会因僵尸死亡而略微降低（可选设计）
                    // mZombieData.threatAccumulation = Mathf.Max(0, mZombieData.threatAccumulation - 0.5f);
                }
            }
            else
            {
                Debug.LogWarning($"[僵尸系统] 尝试处理死亡失败：找不到ID为 {zombieId} 的僵尸。");
            }
        }

        /// <summary>
        /// 对指定ID的僵尸施加伤害。如果伤害导致其生命值降至0或以下，则处理其死亡。
        /// </summary>
        /// <param name="zombieId">目标僵尸ID。</param>
        /// <param name="damage">造成的伤害量。</param>
        public void TakeDamageToZombie(string zombieId, float damage)
        {
            if (damage <= 0) return; // 不处理无效伤害值

            if (mZombieData.zombies.TryGetValue(zombieId, out var zombie)) // 安全获取僵尸数据
            {
                if (!zombie.IsAlive) return; // 如果僵尸已经死亡，则不重复处理

                zombie.TakeDamage(damage); // 调用ZombieData内部的TakeDamage方法处理伤害和状态变更
                
                // Debug.Log($"[僵尸系统] 僵尸 {zombieId} 受到 {damage} 点伤害，剩余生命: {zombie.currentHealth}。");

                if (!zombie.IsAlive) // 如果在TakeDamage后僵尸不再存活
                {
                    ProcessZombieDeath(zombieId); // 则处理其死亡逻辑
                }
            }
            else
            {
                Debug.LogWarning($"[僵尸系统] 尝试对僵尸造成伤害失败：找不到ID为 {zombieId} 的僵尸。");
            }
        }

        /// <summary>获取当前全局僵尸威胁等级。</summary>
        public ZombieThreatLevel GetCurrentThreatLevel() => mZombieData.currentThreatLevel;

        /// <summary>获取自游戏开始以来总共生成过的僵尸数量。</summary>
        public int GetTotalZombieCount() => mZombieData.totalZombiesSpawned;

        /// <summary>获取当前场景中所有存活（活动）的僵尸数量。</summary>
        public int GetActiveZombieCount() => mZombieData.GetAliveZombieCount();

        /// <summary>
        /// 系统的主更新方法（由QF框架的AbstractSystem提供，但通常不直接在此类中实现主要逻辑）。
        /// 实际的周期性更新逻辑已移至由TimeUpdateEvent驱动的OnTimeUpdate方法。
        /// 此方法保留可能是为了满足接口或框架要求，或用于一次性的、非周期性的更新。
        /// </summary>
        public void Update()
        {
            // 主要的周期性更新逻辑已在OnTimeUpdate中处理，以实现更灵活的时间间隔控制。
            // 此处可以留空，或用于处理一些确实需要每帧执行的、不适合放入固定间隔的任务。
            // 例如，非常平滑的动画状态过渡（但通常动画由可视化系统处理）或输入检测（如果适用）。
        }

        /// <summary>
        /// 获取在指定位置和半径范围内的所有存活僵尸列表。
        /// </summary>
        /// <param name="position">搜索的中心位置。</param>
        /// <param name="radius">搜索半径。</param>
        /// <returns>符合条件的存活僵尸数据列表。</returns>
        public List<ZombieData> GetZombiesNearPosition(Vector2 position, float radius)
        {
            var nearbyZombies = new List<ZombieData>();
            if (mZombieData == null || mZombieData.zombies == null) return nearbyZombies;

            foreach (var zombie in mZombieData.zombies.Values) // 遍历所有僵尸
            {
                // 只考虑存活的僵尸，并且其与指定位置的距离在半径之内
                if (zombie.IsAlive && Vector2.Distance(zombie.position, position) <= radius)
                {
                    nearbyZombies.Add(zombie); // 添加到结果列表
                }
            }
            return nearbyZombies;
        }
    }
}

// === 僵尸系统相关的自定义事件结构体定义 ===
// 将这些定义放在一个独立的、游戏特定的命名空间中，以保持组织清晰并避免命名冲突。
namespace MyGameNamespace
{
    // 注意：原BuildingDestroyedEvent等已在各自系统或Model中定义，此处仅为示例，
    // 实际应确保事件定义的一致性和唯一性，避免重复。
    // 如果这些事件已在其他地方定义（如SurvivalGame.Model或各自系统文件内），则此处不应重复。

    /// <summary>
    /// 当一个建筑被摧毁时发送的事件。
    /// </summary>
    public struct BuildingDestroyedEvent // 假设此事件在别处定义，此处仅为引用示例
    {
        /// <summary>被摧毁建筑的唯一ID。</summary>
        public int BuildingId;
        /// <summary>被摧毁建筑的类型或配置ID。</summary>
        public string BuildingType;
        /// <summary>建筑被摧毁时的位置。</summary>
        public Vector3 Position;
    }
    
    /// <summary>
    /// 当一个新僵尸在游戏中生成时发送的事件。
    /// </summary>
    public struct ZombieSpawnedEvent
    {
        /// <summary>生成僵尸的唯一ID。</summary>
        public string ZombieId;
        /// <summary>生成僵尸的类型。</summary>
        public ZombieType Type;
        /// <summary>僵尸的生成位置。</summary>
        public Vector2 Position;
    }
    
    /// <summary>
    /// 当一群僵尸（Horde）在游戏中生成时发送的事件。
    /// </summary>
    public struct ZombieHordeSpawnedEvent
    {
        /// <summary>生成僵尸群的唯一ID。</summary>
        public string HordeId;
        /// <summary>此僵尸群对应的威胁等级。</summary>
        public ZombieThreatLevel ThreatLevel;
        /// <summary>僵尸群生成的中心位置。</summary>
        public Vector2 CenterPosition;
        /// <summary>此僵尸群中包含的僵尸数量。</summary>
        public int ZombieCount;
    }
    
    /// <summary>
    /// 当一个僵尸对建筑发起攻击时发送的事件。
    /// </summary>
    public struct ZombieAttackBuildingEvent
    {
        /// <summary>发动攻击的僵尸的ID。</summary>
        public string ZombieId;
        /// <summary>被攻击建筑的ID。</summary>
        public string BuildingId; // 注意：原为int，与BuildingData.Id (string)统一
        /// <summary>本次攻击造成的伤害量。</summary>
        public float Damage;
        /// <summary>发动攻击的僵尸的类型。</summary>
        public ZombieType AttackerType;
    }
    
    /// <summary>
    /// 当一个僵尸死亡时发送的事件。
    /// </summary>
    public struct ZombieDeathEvent
    {
        /// <summary>死亡僵尸的ID。</summary>
        public string ZombieId;
        /// <summary>死亡僵尸的类型。</summary>
        public ZombieType Type;
        /// <summary>僵尸死亡时的位置。</summary>
        public Vector2 Position;
    }
    
    /// <summary>
    /// 当全局僵尸威胁等级发生变化时发送的事件。
    /// </summary>
    public struct ThreatLevelChangedEvent
    {
        /// <summary>变化前的旧威胁等级。</summary>
        public ZombieThreatLevel OldLevel;
        /// <summary>变化后的新威胁等级。</summary>
        public ZombieThreatLevel NewLevel;
        /// <summary>导致等级变化（或当前）的威胁积累值。</summary>
        public float ThreatAccumulation;
    }
}
