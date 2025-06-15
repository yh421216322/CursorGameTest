// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieAIEnhancementSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了僵尸AI增强系统 (ZombieAIEnhancementSystem)，旨在为游戏中的僵尸
//     提供更高级的人工智能行为。这包括处理个体僵尸的特殊AI逻辑（如不同类型僵尸的
//     独特行为）、僵尸的群体行为、特殊能力的实现与冷却管理，以及相关的动画触发
//     和视觉效果表现。该系统依赖于基础僵尸系统、可视化系统和建筑系统。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq; // 用于 LINQ 查询
using UnityEngine;   // Unity 核心功能
using QFramework;    // QFramework 框架
using DG.Tweening;   // DOTween 动画库
using MyGameNamespace; // 包含ZombieSpawnedEvent, ZombieDeathEvent等自定义事件的命名空间
using SurvivalGame.Model; // 游戏核心数据模型
using SurvivalGame.GameSystem; // 游戏系统命名空间
using SurvivalGame.Visualization; // 僵尸可视化系统

namespace SurvivalGame.AI // 将此系统归类到AI相关的命名空间
{
    /// <summary>
    /// 僵尸AI增强系统接口 (IZombieAIEnhancementSystem)。
    /// 定义了僵尸AI增强模块应提供的核心功能。
    /// </summary>
    public interface IZombieAIEnhancementSystem : QFISystem
    {
        /// <summary>
        /// 更新所有活动僵尸的AI逻辑。
        /// </summary>
        void UpdateZombieAI();

        /// <summary>
        /// 处理僵尸的群体行为逻辑，如集群、跟随领袖等。
        /// </summary>
        void ProcessZombieGroupBehavior();

        /// <summary>
        /// 处理并触发僵尸的特殊能力。
        /// </summary>
        void HandleZombieSpecialAbilities();

        /// <summary>
        /// （可能是一个辅助或占位方法）触发僵尸的通用或特定动画。
        /// 实际动画触发可能分散在各个具体行为方法中。
        /// </summary>
        void TriggerZombieAnimations();
    }

    /// <summary>
    /// 僵尸AI增强系统 (ZombieAIEnhancementSystem) 的具体实现。
    /// 负责实现更智能的僵尸行为，包括个体AI、群体协作和特殊能力的运用。
    /// </summary>
    public class ZombieAIEnhancementSystem : AbstractSystem, IZombieAIEnhancementSystem
    {
        // --- 系统引用 (System References) ---
        private IZombieSystem zombieSystem; // 基础僵尸系统，用于获取僵尸数据和状态
        private IZombieVisualizationSystem visualizationSystem; // 僵尸可视化系统，用于触发动画和特效
        private IEnhancedBuildingSystem buildingSystem; // 增强建筑系统，用于僵尸索敌（寻找建筑目标）

        // --- AI参数常量 (AI Parameters) ---
        /// <summary>僵尸个体AI逻辑的更新频率（秒）。</summary>
        private const float AI_UPDATE_INTERVAL = 0.3f; // 每0.3秒更新一次AI决策
        /// <summary>僵尸群体行为逻辑的更新频率（秒）。</summary>
        private const float GROUP_BEHAVIOR_INTERVAL = 1f; // 每1秒更新一次群体行为
        /// <summary>僵尸特殊能力使用判断的更新频率（秒）。</summary>
        private const float SPECIAL_ABILITY_INTERVAL = 2f; // 每2秒检查一次特殊能力使用

        // --- 群体行为参数 (Group Behavior Parameters) ---
        /// <summary>僵尸形成群体的最大搜索半径。</summary>
        private const float GROUP_FORMATION_RADIUS = 3f; // 在3米半径内寻找同伴形成群体
        /// <summary>僵尸领袖能影响其追随者的最大半径。</summary>
        private const float LEADER_INFLUENCE_RADIUS = 5f; // 领袖在5米半径内影响其他僵尸
        /// <summary>形成一个有效僵尸群体的最小数量。</summary>
        private const int MIN_GROUP_SIZE = 3; // 至少3个僵尸才能组成一个群体

        // --- 特殊能力冷却时间管理 (Special Ability Cooldowns) ---
        /// <summary>存储吐酸者 (Spitter) 特殊攻击的冷却计时，键为僵尸ID。</summary>
        private Dictionary<string, float> spitterCooldowns;
        /// <summary>存储尖叫者 (Screamer) 特殊能力的冷却计时，键为僵尸ID。</summary>
        private Dictionary<string, float> screamerCooldowns;
        /// <summary>存储坦克 (Tank) 冲锋技能的冷却计时，键为僵尸ID。</summary>
        private Dictionary<string, float> tankChargeCooldowns;
        
        // --- 更新时间戳 (Update Timestamps) ---
        /// <summary>上一次AI逻辑更新的游戏时间。</summary>
        private float lastAIUpdate;
        /// <summary>上一次群体行为逻辑更新的游戏时间。</summary>
        private float lastGroupBehaviorUpdate;
        /// <summary>上一次特殊能力逻辑更新的游戏时间。</summary>
        private float lastSpecialAbilityUpdate;
        
        /// <summary>
        /// 系统初始化方法。
        /// 获取其他系统引用，初始化内部数据结构（如冷却时间字典），并注册相关事件监听。
        /// </summary>
        protected override void OnInit()
        {
            // 获取所需系统模块的引用
            zombieSystem = this.GetSystem<IZombieSystem>();
            visualizationSystem = this.GetSystem<IZombieVisualizationSystem>();
            buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            
            // 初始化用于存储特殊能力冷却时间的数据结构
            spitterCooldowns = new Dictionary<string, float>();
            screamerCooldowns = new Dictionary<string, float>();
            tankChargeCooldowns = new Dictionary<string, float>();
            
            // 注册监听游戏内的僵尸生成、死亡及时间更新事件
            this.RegisterEvent<ZombieSpawnedEvent>(OnZombieSpawned); // 监听僵尸生成事件
            this.RegisterEvent<ZombieDeathEvent>(OnZombieDied);     // 监听僵尸死亡事件
            this.RegisterEvent<TimeUpdateEvent>(OnTimeUpdate);       // 监听游戏时间更新事件 (自定义或QFramework提供)
            
            // 初始化各类更新逻辑的上次执行时间戳
            lastAIUpdate = Time.time;
            lastGroupBehaviorUpdate = Time.time;
            lastSpecialAbilityUpdate = Time.time;
            
            UnityEngine.Debug.Log("[僵尸AI增强系统] 初始化完成。");
        }
        
        /// <summary>
        /// 处理僵尸生成事件。
        /// 当特定类型的僵尸生成时，为其初始化特殊能力的冷却计时器，并触发其生成动画。
        /// </summary>
        /// <param name="e">僵尸生成事件的参数，包含生成的僵尸ID。</param>
        private void OnZombieSpawned(ZombieSpawnedEvent e)
        {
            // 为具有特殊能力的僵尸初始化其技能冷却计时器
            var zombieData = zombieSystem.GetZombieData().zombies[e.ZombieId]; // 从基础僵尸系统获取数据
            
            switch (zombieData.type) // 根据僵尸类型进行处理
            {
                case ZombieType.Spitter: // 吐酸者
                    spitterCooldowns[e.ZombieId] = 0f; // 初始化吐酸冷却
                    break;
                case ZombieType.Screamer: // 尖叫者
                    screamerCooldowns[e.ZombieId] = 0f; // 初始化尖叫冷却
                    break;
                case ZombieType.Tank: // 坦克
                    tankChargeCooldowns[e.ZombieId] = 0f; // 初始化冲锋冷却
                    break;
            }
            
            TriggerSpawnAnimation(e.ZombieId); // 触发该僵尸的出生/生成动画
        }
        
        /// <summary>
        /// 处理僵尸死亡事件。
        /// 当僵尸死亡时，从相关的冷却计时器字典中移除其条目，以清理数据。
        /// </summary>
        /// <param name="e">僵尸死亡事件的参数，包含死亡的僵尸ID。</param>
        private void OnZombieDied(ZombieDeathEvent e)
        {
            // 清理已死亡僵尸的特殊能力冷却时间记录，避免内存泄漏
            spitterCooldowns.Remove(e.ZombieId);    // 尝试移除，如果键不存在也不会报错
            screamerCooldowns.Remove(e.ZombieId);
            tankChargeCooldowns.Remove(e.ZombieId);
        }
        
        /// <summary>
        /// 处理游戏时间更新事件（通常由主游戏循环定期发送）。
        /// 根据预设的时间间隔，分别调用AI行为、群体行为和特殊能力的更新逻辑。
        /// </summary>
        /// <param name="e">时间更新事件的参数 (当前未使用其内部数据)。</param>
        private void OnTimeUpdate(TimeUpdateEvent e)
        {
            float currentTime = Time.time; // 获取当前游戏时间
            
            // 按固定间隔更新僵尸的个体AI行为
            if (currentTime - lastAIUpdate >= AI_UPDATE_INTERVAL)
            {
                UpdateZombieAI(); // 调用AI更新方法
                lastAIUpdate = currentTime; // 更新上次AI更新的时间戳
            }
            
            // 按固定间隔更新僵尸的群体行为逻辑
            if (currentTime - lastGroupBehaviorUpdate >= GROUP_BEHAVIOR_INTERVAL)
            {
                ProcessZombieGroupBehavior(); // 调用群体行为处理方法
                lastGroupBehaviorUpdate = currentTime; // 更新上次群体行为更新的时间戳
            }
            
            // 按固定间隔更新僵尸的特殊能力使用判断和执行
            if (currentTime - lastSpecialAbilityUpdate >= SPECIAL_ABILITY_INTERVAL)
            {
                HandleZombieSpecialAbilities(); // 调用特殊能力处理方法
                lastSpecialAbilityUpdate = currentTime; // 更新上次特殊能力更新的时间戳
            }
        }
        
        /// <summary>
        /// 更新所有存活僵尸的AI逻辑。
        /// 遍历所有僵尸，为每个存活的僵尸更新其个体AI，并触发相应的状态动画。
        /// </summary>
        public void UpdateZombieAI()
        {
            var zombieSystemData = zombieSystem.GetZombieData(); // 获取所有僵尸的数据
            if (zombieSystemData == null || zombieSystemData.zombies == null) return;

            foreach (var zombie in zombieSystemData.zombies.Values) // 遍历所有僵尸
            {
                if (!zombie.IsAlive) continue; // 跳过已死亡的僵尸
                
                UpdateIndividualZombieAI(zombie); // 更新该僵尸的个体AI行为
                TriggerZombieStateAnimation(zombie); // 根据其当前状态触发相应动画
            }
        }
        
        /// <summary>
        /// （辅助方法）根据僵尸的类型，调用相应类型的特定AI更新逻辑。
        /// </summary>
        /// <param name="zombie">要更新AI的僵尸数据对象。</param>
        private void UpdateIndividualZombieAI(ZombieData zombie)
        {
            switch (zombie.type) // 根据僵尸类型分派到不同的AI更新函数
            {
                case ZombieType.Walker:   UpdateWalkerAI(zombie);   break; // 普通行走僵尸
                case ZombieType.Runner:   UpdateRunnerAI(zombie);   break; // 快速奔跑僵尸
                case ZombieType.Tank:     UpdateTankAI(zombie);     break; // 坦克僵尸
                case ZombieType.Spitter:  UpdateSpitterAI(zombie);  break; // 远程吐酸僵尸
                case ZombieType.Screamer: UpdateScreamerAI(zombie); break; // 尖叫者僵尸
            }
        }
        
        /// <summary>
        /// 更新普通行走僵尸 (Walker) 的AI逻辑。
        /// 如果处于游荡状态，会尝试寻找并接近最近的建筑。
        /// </summary>
        /// <param name="zombie">行走僵尸的数据对象。</param>
        private void UpdateWalkerAI(ZombieData zombie)
        {
            if (zombie.state == ZombieState.Wandering) // 如果僵尸当前正在游荡
            {
                // 尝试寻找比其基础探测范围稍大一些范围内的最近建筑
                var nearestBuilding = FindNearestBuilding(zombie.position, zombie.detectionRange * 1.5f);
                if (nearestBuilding != null) // 如果找到了建筑
                {
                    zombie.targetBuildingId = nearestBuilding.Id.ToString(); // 设置目标建筑ID
                    zombie.targetPosition = nearestBuilding.Position;       // 设置目标位置
                    zombie.state = ZombieState.Approaching;                 // 切换到接近目标状态
                    
                    TriggerDiscoveryAnimation(zombie.id); // 触发发现目标的动画/特效
                }
            }
            // 当状态变为Approaching后，其移动逻辑由基础ZombieSystem或可视化系统处理
        }
        
        /// <summary>
        /// 更新快速奔跑僵尸 (Runner) 的AI逻辑。
        /// 优先寻找并攻击防御性建筑；如果找不到，则行为类似普通行走僵尸。
        /// </summary>
        /// <param name="zombie">奔跑僵尸的数据对象。</param>
        private void UpdateRunnerAI(ZombieData zombie)
        {
            if (zombie.state == ZombieState.Wandering) // 如果僵尸当前正在游荡
            {
                // 优先在其探测范围的更大倍数内寻找防御型建筑
                var defenseBuilding = FindNearestDefenseBuilding(zombie.position, zombie.detectionRange * 2f);
                if (defenseBuilding != null) // 如果找到了防御建筑
                {
                    zombie.targetBuildingId = defenseBuilding.Id.ToString();
                    zombie.targetPosition = defenseBuilding.Position;
                    zombie.state = ZombieState.Approaching;
                    
                    // 作为奔跑者的特性，暂时性提高其移动速度
                    zombie.moveSpeed *= 1.3f; // 速度提升30% (此修改应有时效性或在状态改变时重置)
                    TriggerSpeedBoostAnimation(zombie.id); // 触发速度提升的动画/特效
                }
                else // 如果没有找到防御建筑
                {
                    UpdateWalkerAI(zombie); // 则行为退化为普通行走僵尸的AI逻辑
                }
            }
        }
        
        /// <summary>
        /// 更新坦克僵尸 (Tank) 的AI逻辑。
        /// 在接近目标时，如果满足条件（距离、冷却），则可能发动冲锋。
        /// </summary>
        /// <param name="zombie">坦克僵尸的数据对象。</param>
        private void UpdateTankAI(ZombieData zombie)
        {
            if (zombie.state == ZombieState.Approaching) // 如果正在接近目标
            {
                float distanceToTarget = Vector2.Distance(zombie.position, zombie.targetPosition); // 计算与目标的距离
                
                // 如果与目标的距离小于等于5米，并且冲锋技能不在冷却中
                if (distanceToTarget <= 5f && CanTankCharge(zombie.id))
                {
                    StartTankCharge(zombie); // 发动冲锋
                }
            }
            // 冲锋后的伤害判定和效果可能在其他地方处理（如碰撞检测或特殊技能更新循环）
        }
        
        /// <summary>
        /// 更新吐酸者僵尸 (Spitter) 的AI逻辑。
        /// 在接近目标一定距离后，如果特殊攻击可用，则停止移动并发动远程吐酸攻击。
        /// </summary>
        /// <param name="zombie">吐酸者僵尸的数据对象。</param>
        private void UpdateSpitterAI(ZombieData zombie)
        {
            if (zombie.state == ZombieState.Approaching) // 如果正在接近目标
            {
                float distanceToTarget = Vector2.Distance(zombie.position, zombie.targetPosition); // 计算与目标的距离
                
                // 如果与目标的距离在其攻击范围内，并且吐酸技能不在冷却中
                if (distanceToTarget <= zombie.attackRange && CanSpitterAttack(zombie.id))
                {
                    zombie.state = ZombieState.Attacking; // 切换到攻击状态 (远程攻击)
                    PerformSpitterAttack(zombie);        // 执行吐酸攻击动作
                }
                // 注意：如果距离太近，吐酸者可能需要后退的逻辑，此处未实现
            }
        }
        
        /// <summary>
        /// 更新尖叫者僵尸 (Screamer) 的AI逻辑。
        /// 如果特殊能力可用且周围有目标（如建筑），则可能发动尖叫以吸引其他僵尸。
        /// </summary>
        /// <param name="zombie">尖叫者僵尸的数据对象。</param>
        private void UpdateScreamerAI(ZombieData zombie)
        {
            // 只要尖叫者存活且尖叫技能不在冷却中
            if (zombie.state != ZombieState.Dead && CanScreamerScream(zombie.id))
            {
                // 检查其周围一定范围（例如8米）内是否存在建筑 (作为触发尖叫的条件之一)
                var nearbyBuildings = FindBuildingsInRadius(zombie.position, 8f);
                if (nearbyBuildings.Count > 0) // 如果附近有建筑
                {
                    PerformScreamerAbility(zombie); // 发动尖叫能力
                }
            }
        }
        
        /// <summary>
        /// 处理僵尸的群体行为逻辑。
        /// 包括尝试形成僵尸群组和处理已形成群组的集体移动。
        /// </summary>
        public void ProcessZombieGroupBehavior()
        {
            var zombieSystemData = zombieSystem.GetZombieData(); // 获取所有僵尸数据
            if (zombieSystemData == null || zombieSystemData.zombies == null) return;
            var allZombiesList = new List<ZombieData>(zombieSystemData.zombies.Values); // 转换为列表方便操作
            
            FormZombieGroups(allZombiesList);     // 尝试让符合条件的僵尸形成群组
            ProcessGroupMovement(allZombiesList); // （占位）处理已形成群组的移动逻辑
        }
        
        /// <summary>
        /// （辅助方法）尝试让符合条件的僵尸形成或加入群组。
        /// </summary>
        /// <param name="zombies">当前场景中所有僵尸的列表。</param>
        private void FormZombieGroups(List<ZombieData> zombies)
        {
            foreach (var zombie in zombies) // 遍历每个僵尸
            {
                // 跳过已死亡或正在攻击的僵尸（它们通常不参与新的群体形成）
                if (!zombie.IsAlive || zombie.state == ZombieState.Attacking) continue;
                
                // 为当前僵尸查找其预设群体半径内的其他僵尸
                var nearbyZombies = FindNearbyZombies(zombie.position, GROUP_FORMATION_RADIUS, zombies);
                
                // 如果附近同伴数量达到最小成群规模
                if (nearbyZombies.Count >= MIN_GROUP_SIZE)
                {
                    var leader = GetGroupLeader(nearbyZombies); // 从附近僵尸中选出一个领袖 (例如血量最高的)
                    if (leader != null && leader.id != zombie.id) // 如果成功选出领袖且不是自己
                    {
                        // 让群体中的其他游荡状态的僵尸跟随领袖的目标和状态
                        foreach (var follower in nearbyZombies)
                        {
                            if (follower.id != leader.id && follower.state == ZombieState.Wandering)
                            {
                                follower.targetPosition = leader.targetPosition; // 跟随领袖的目标位置
                                follower.state = leader.state;                   // 同步为领袖的状态 (例如都变为Approaching)
                            }
                        }
                        TriggerGroupFormationAnimation(nearbyZombies); // 触发群体形成的视觉效果
                    }
                }
            }
        }
        
        /// <summary>
        /// （占位方法）处理僵尸群体的集体移动逻辑。
        /// 例如，保持阵型、避免互相碰撞、协同包围等。
        /// </summary>
        /// <param name="zombies">所有僵尸的列表（可能包含已成群的）。</param>
        private void ProcessGroupMovement(List<ZombieData> zombies)
        {
            // TODO: 此处实现更复杂的群体移动AI。
            // 例如，已成群的僵尸可能围绕领袖移动，或保持特定队形。
            // 可能需要为ZombieData添加groupID或leaderID字段来管理群组成员。
        }
        
        /// <summary>
        /// 处理并尝试触发所有存活僵尸的特殊能力。
        /// </summary>
        public void HandleZombieSpecialAbilities()
        {
            var zombieSystemData = zombieSystem.GetZombieData(); // 获取所有僵尸数据
            if (zombieSystemData == null || zombieSystemData.zombies == null) return;

            foreach (var zombie in zombieSystemData.zombies.Values) // 遍历所有僵尸
            {
                if (!zombie.IsAlive) continue; // 跳过已死亡的僵尸
                
                // 根据僵尸类型，调用相应的特殊能力处理方法
                switch (zombie.type)
                {
                    case ZombieType.Tank:     HandleTankSpecialAbility(zombie);     break;
                    case ZombieType.Spitter:  HandleSpitterSpecialAbility(zombie);  break;
                    case ZombieType.Screamer: HandleScreamerSpecialAbility(zombie); break;
                    // 其他类型的僵尸如果有特殊能力，在此处添加case
                }
            }
        }
        
        /// <summary>
        /// （占位方法）用于触发僵尸的通用或特定动画。
        /// 实际的动画触发逻辑更可能分散在各个行为方法中（如攻击、移动、特殊能力等）。
        /// </summary>
        public void TriggerZombieAnimations()
        {
            // 此方法可能是一个集中的动画触发点，或者是一个供外部调用的接口。
            // 但更常见的做法是在具体行为逻辑中直接调用可视化系统来触发动画。
            // 例如, 在FireAtTarget中触发攻击动画, 在UpdateZombieAI中根据状态触发移动或待机动画。
            UnityEngine.Debug.LogWarning("[僵尸AI增强系统] TriggerZombieAnimations 方法被调用，但其具体实现可能分散在各处。");
        }
        
        /// <summary>
        /// （辅助方法）触发指定僵尸的出生/生成动画或特效。
        /// </summary>
        /// <param name="zombieId">要触发动画的僵尸ID。</param>
        private void TriggerSpawnAnimation(string zombieId)
        {
            // TODO: 通过可视化系统(visualizationSystem)触发一个更复杂的生成特效或动画序列
            // 例如: visualizationSystem.PlaySpawnEffect(zombieId, SpawnEffectType.EmergingFromGround);
            UnityEngine.Debug.Log($"[僵尸AI增强系统] 正在为僵尸 {zombieId} 触发生成动画/特效。");
        }
        
        /// <summary>
        /// （辅助方法）当僵尸发现目标（如建筑）时，触发一个提示性的动画或特效。
        /// </summary>
        /// <param name="zombieId">发现目标的僵尸ID。</param>
        private void TriggerDiscoveryAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId); // 获取僵尸的GameObject
            if (zombieVisual != null)
            {
                CreateExclamationEffect(zombieVisual.transform.position); // 在其头顶创建感叹号特效
                
                // 使僵尸模型短暂闪烁黄色以示强调
                var renderer = zombieVisual.GetComponent<SpriteRenderer>(); // 假设是2D Sprite
                if (renderer != null)
                {
                    var originalColor = renderer.color;
                    renderer.DOColor(Color.yellow, 0.2f) // 0.2秒内变为黄色
                        .OnComplete(() => renderer.DOColor(originalColor, 0.2f)); // 完成后0.2秒内恢复原色
                }
            }
        }
        
        /// <summary>
        /// （辅助方法）当奔跑者僵尸速度提升时，触发相应的视觉效果。
        /// </summary>
        /// <param name="zombieId">速度提升的僵尸ID。</param>
        private void TriggerSpeedBoostAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId);
            if (zombieVisual != null)
            {
                CreateSpeedLinesEffect(zombieVisual.transform.position); // 在其身后创建速度线特效
                
                // 使僵尸模型短暂闪烁红色或附加红色光环
                var renderer = zombieVisual.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.DOColor(Color.red, 0.1f) // 0.1秒内变为红色
                        .SetLoops(6, LoopType.Yoyo); // 快速闪烁3次 (变红再变回原色为一次循环)
                }
            }
        }
        
        /// <summary>
        /// （辅助方法）当僵尸形成一个群组时，触发群体相关的视觉提示。
        /// </summary>
        /// <param name="zombiesInGroup">组成群体的僵尸列表。</param>
        private void TriggerGroupFormationAnimation(List<ZombieData> zombiesInGroup)
        {
            // 示例：让群组中的所有僵尸同时短暂闪烁青色光芒
            foreach (var zombie in zombiesInGroup)
            {
                var zombieVisual = visualizationSystem.GetZombieVisual(zombie.id);
                if (zombieVisual != null)
                {
                    var renderer = zombieVisual.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.DOColor(Color.cyan, 0.3f) // 0.3秒变为青色
                            .SetLoops(2, LoopType.Yoyo); // 闪烁1次 (变青再变回)
                    }
                }
            }
            // Debug.Log($"[僵尸AI增强系统] {zombiesInGroup.Count} 个僵尸形成了群组，触发了群体形成动画。");
        }
        
        /// <summary>
        /// （辅助方法）根据僵尸的当前状态触发相应的状态动画（如移动、攻击等）。
        /// </summary>
        /// <param name="zombie">要触发状态动画的僵尸数据。</param>
        private void TriggerZombieStateAnimation(ZombieData zombie)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombie.id); // 获取僵尸GameObject
            if (zombieVisual == null) return; // 如果找不到视觉对象，则无法播放动画
            
            switch (zombie.state) // 根据僵尸当前状态
            {
                case ZombieState.Approaching: // 如果是正在接近目标状态
                    TriggerApproachingAnimation(zombieVisual); // 触发“正在接近”的特定动画/姿态
                    break;
                case ZombieState.Attacking: // 如果是攻击状态
                    // 攻击动画通常更复杂，可能由ZombieVisualizationSystem内部根据攻击类型和目标处理，
                    // 或者在此处根据情况调用 visualizationSystem.PlayAttackAnimation(zombie.id, zombie.targetBuildingId);
                    break;
                // case ZombieState.Wandering: // 游荡状态动画
                // case ZombieState.Dead: // 死亡动画 (通常在OnZombieDied中处理一次性触发)
                // 等其他状态...
            }
        }
        
        /// <summary>
        /// （辅助方法）触发僵尸“正在接近目标”时的特定动画或姿态调整。
        /// </summary>
        /// <param name="zombieVisual">僵尸的GameObject。</param>
        private void TriggerApproachingAnimation(GameObject zombieVisual)
        {
            // 示例：使僵尸模型轻微前倾，并循环播放此姿态动画，模拟急切接近的动态
            // 使用DOTween的DORotate实现。-5度表示轻微前倾。
            // SetEase设定缓动类型，SetLoops(-1, LoopType.Yoyo)使其无限次来回播放。
            // 注意：如果已有通过Animator控制的移动动画，此效果可能需要与其协调或整合。
            if (!DOTween.IsTweening(zombieVisual.transform)) // 防止重复添加动画
            {
                zombieVisual.transform.DORotate(new Vector3(0, 0, -5f), 0.5f) // 0.5秒内旋转到-5度
                    .SetEase(Ease.InOutSine) // 使用平滑的缓动效果
                    .SetLoops(-1, LoopType.Yoyo); // 无限循环播放，Yoyo效果使动画来回进行
            }
        }
        
        // --- Tank (坦克) 相关AI方法 ---
        /// <summary>检查坦克僵尸是否可以发动冲锋技能（基于冷却时间）。</summary>
        private bool CanTankCharge(string zombieId)
        {
            if (!tankChargeCooldowns.ContainsKey(zombieId)) return true; // 如果没有记录，则认为可用
            return Time.time - tankChargeCooldowns[zombieId] >= 10f; // 假设冲锋冷却时间为10秒
        }
        
        /// <summary>坦克僵尸开始冲锋。</summary>
        private void StartTankCharge(ZombieData zombie)
        {
            tankChargeCooldowns[zombie.id] = Time.time; // 重置冲锋冷却计时
            
            zombie.moveSpeed *= 2f; // 冲锋时移动速度加倍 (此修改应有时效性)
            // TODO: 冲锋状态可能需要一个单独的ZombieState或标记，以便后续处理其特殊碰撞/伤害逻辑
            
            TriggerChargeAnimation(zombie.id); // 触发冲锋的视觉/动画效果
            UnityEngine.Debug.Log($"[僵尸AI增强系统] Tank僵尸 {zombie.id} 开始冲锋！当前速度: {zombie.moveSpeed}");
        }
        
        /// <summary>（占位）处理坦克僵尸的其他特殊能力逻辑。</summary>
        private void HandleTankSpecialAbility(ZombieData zombie)
        {
            // 例如，如果坦克有范围践踏或投掷能力，在此处处理其触发条件和效果
        }
        
        // --- Spitter (吐酸者) 相关AI方法 ---
        /// <summary>检查吐酸者是否可以发动吐酸攻击（基于冷却时间）。</summary>
        private bool CanSpitterAttack(string zombieId)
        {
            if (!spitterCooldowns.ContainsKey(zombieId)) return true;
            return Time.time - spitterCooldowns[zombieId] >= 3f; // 假设吐酸冷却时间为3秒
        }
        
        /// <summary>吐酸者执行吐酸攻击。</summary>
        private void PerformSpitterAttack(ZombieData zombie)
        {
            spitterCooldowns[zombie.id] = Time.time; // 重置吐酸冷却
            
            TriggerSpitAnimation(zombie.id); // 触发吐酸的视觉/动画效果
            // TODO: 此处应创建远程投射物(酸液)，并由投射物系统处理其飞行和命中逻辑
            // 例如: projectileSystem.CreateSpitProjectile(zombie.position, zombie.targetPosition, zombie.damage);
            
            UnityEngine.Debug.Log($"[僵尸AI增强系统] Spitter僵尸 {zombie.id} 向目标 {zombie.targetBuildingId ?? "未知目标"} 发动了吐酸攻击！");
        }
        
        /// <summary>（占位）处理吐酸者的其他特殊能力逻辑。</summary>
        private void HandleSpitterSpecialAbility(ZombieData zombie)
        {
            // 例如，如果吐酸者能留下持续伤害的酸液池，在此处理
        }
        
        // --- Screamer (尖叫者) 相关AI方法 ---
        /// <summary>检查尖叫者是否可以发动尖叫能力（基于冷却时间）。</summary>
        private bool CanScreamerScream(string zombieId)
        {
            if (!screamerCooldowns.ContainsKey(zombieId)) return true;
            return Time.time - screamerCooldowns[zombieId] >= 15f; // 假设尖叫冷却时间为15秒
        }
        
        /// <summary>尖叫者执行尖叫能力，吸引附近其他僵尸。</summary>
        private void PerformScreamerAbility(ZombieData zombie)
        {
            screamerCooldowns[zombie.id] = Time.time; // 重置尖叫冷却
            
            // 查找尖叫者周围10米范围内的其他僵尸
            var nearbyZombies = zombieSystem.GetZombiesNearPosition(zombie.position, 10f);
            foreach (var nearbyZombie in nearbyZombies)
            {
                // 如果这些僵尸是自己以外的、且当前处于游荡状态
                if (nearbyZombie.id != zombie.id && nearbyZombie.state == ZombieState.Wandering)
                {
                    nearbyZombie.state = ZombieState.Approaching; // 将其状态改为接近
                    nearbyZombie.targetPosition = zombie.targetPosition; // 使其目标与尖叫者的目标一致 (通常是玩家基地或建筑)
                    // Debug.Log($"[僵尸AI增强系统] 僵尸 {nearbyZombie.id} 被尖叫者 {zombie.id} 吸引。");
                }
            }
            
            TriggerScreamAnimation(zombie.id); // 触发尖叫的视觉/音效
            UnityEngine.Debug.Log($"[僵尸AI增强系统] Screamer僵尸 {zombie.id} 发出尖叫，吸引了 {nearbyZombies.Count-1} 个其他僵尸！");
        }
        
        /// <summary>（占位）处理尖叫者的其他特殊能力逻辑。</summary>
        private void HandleScreamerSpecialAbility(ZombieData zombie)
        {
            // 例如，如果尖叫能给附近僵尸提供Buff，在此处理
        }
        
        // --- 动画与特效触发方法 (Animation and Effect Triggers) ---
        // 这些方法主要通过调用ZombieVisualizationSystem或直接使用DOTween创建简单效果

        /// <summary>触发坦克僵尸冲锋的动画/视觉效果。</summary>
        private void TriggerChargeAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId);
            if (zombieVisual != null)
            {
                zombieVisual.transform.DOShakePosition(1f, strength: 0.2f, vibrato: 10, randomness: 90, fadeOut: false); // 冲锋时抖动
                var renderer = zombieVisual.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.DOColor(Color.magenta, 0.1f).SetLoops(10, LoopType.Yoyo); // 快速闪烁洋红色
                }
            }
        }
        
        /// <summary>触发吐酸者吐酸的动画/视觉效果。</summary>
        private void TriggerSpitAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId);
            if (zombieVisual != null)
            {
                var originalPos = zombieVisual.transform.position;
                // 模拟一个快速向前再收回的吐酸动作
                zombieVisual.transform.DOMove(originalPos + zombieVisual.transform.right * 0.3f, 0.2f) // 假设僵尸朝向右方
                    .OnComplete(() => zombieVisual.transform.DOMove(originalPos, 0.2f));
                CreateAcidEffect(originalPos + zombieVisual.transform.right * 0.5f); // 在前方创建酸液特效
            }
        }
        
        /// <summary>触发尖叫者尖叫的动画/视觉效果。</summary>
        private void TriggerScreamAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId);
            if (zombieVisual != null)
            {
                var originalScale = zombieVisual.transform.localScale;
                // 模拟一个身体膨胀再收缩的尖叫动作
                zombieVisual.transform.DOScale(originalScale * 1.5f, 0.3f)
                    .OnComplete(() => zombieVisual.transform.DOScale(originalScale, 0.3f));
                zombieVisual.transform.DOShakeRotation(0.5f, strength: new Vector3(0,0,30), vibrato: 10); // 旋转抖动
                CreateSoundWaveEffect(zombieVisual.transform.position); // 创建音波特效
            }
        }
        
        // --- 特效创建辅助方法 (Effect Creation Helpers) ---
        // 这些方法创建临时的GameObject来显示简单的视觉特效，并使用DOTween控制其动画和生命周期。

        /// <summary>在指定位置创建一个感叹号漂浮特效。</summary>
        private void CreateExclamationEffect(Vector3 position)
        {
            GameObject effectGO = new GameObject("ExclamationEffect"); // 创建特效GameObject
            effectGO.transform.position = position + Vector3.up * 1f; // 位置在目标头顶上方1米

            var textMesh = effectGO.AddComponent<TextMesh>(); // 添加TextMesh组件显示文字
            textMesh.text = "!"; // 设置文本为感叹号
            textMesh.fontSize = 30;    // 字体大小
            textMesh.color = Color.yellow; // 颜色为黄色
            textMesh.anchor = TextAnchor.MiddleCenter; // 文本锚点居中
            textMesh.fontStyle = FontStyle.Bold;    // 加粗
            // TODO: 考虑使用TextMeshPro以获得更好的文本渲染效果

            // 动画：从小到大出现，然后缩小消失
            effectGO.transform.localScale = Vector3.zero; // 初始大小为0
            effectGO.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack); // 0.2秒放大到原大小，使用Ease.OutBack缓动
            DOVirtual.DelayedCall(1f, () => { // 延迟1秒后执行
                if (effectGO != null) // 再次检查对象是否存在（可能已被销毁）
                {
                    effectGO.transform.DOScale(Vector3.zero, 0.2f) // 0.2秒缩小到0
                        .OnComplete(() => GameObject.Destroy(effectGO)); // 动画完成后销毁GameObject
                }
            });
        }
        
        /// <summary>在指定位置创建速度线条拖尾特效。</summary>
        private void CreateSpeedLinesEffect(Vector3 position)
        {
            // 示例：创建3条简单的速度线
            for (int i = 0; i < 3; i++)
            {
                GameObject lineGO = new GameObject("SpeedLineEffect");
                // 将速度线放置在僵尸当前位置略微靠后的地方，并有微小偏移以形成多条线
                lineGO.transform.position = position + Vector3.left * 0.1f * i; // 假设僵尸面朝右，速度线在左侧
                
                var renderer = lineGO.AddComponent<SpriteRenderer>(); // 使用SpriteRenderer显示线条
                renderer.sprite = CreateLineSprite(); // 创建一个简单的白色线条精灵
                renderer.color = new Color(1, 1, 0, 0.7f); // 设置为半透明黄色
                renderer.sortingOrder = 3; // 调整渲染顺序
                
                // 动画：线条向僵尸移动的反方向快速移动并淡出
                lineGO.transform.DOMove(position + Vector3.left * (2f + 0.2f*i), 0.5f); // 向左移动更远
                renderer.DOFade(0f, 0.5f) // 0.5秒内淡出至完全透明
                    .OnComplete(() => GameObject.Destroy(lineGO)); // 动画完成后销毁
            }
        }
        
        /// <summary>在指定位置创建酸液特效（例如一个绿色的圆点）。</summary>
        private void CreateAcidEffect(Vector3 position)
        {
            GameObject acidGO = new GameObject("AcidEffect");
            acidGO.transform.position = position; // 酸液初始位置
            
            var renderer = acidGO.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateAcidSprite(); // 创建一个简单的绿色圆形精灵代表酸液
            renderer.color = Color.green; // 设置颜色
            renderer.sortingOrder = 4; // 渲染顺序
            
            // 动画：酸液向前飞行一小段距离然后淡出消失
            // TODO: 实际应根据目标位置计算飞行方向和距离
            acidGO.transform.DOMove(position + Vector3.right * 3f, 1f).SetEase(Ease.OutQuad); // 假设向右飞行3米，1秒到达，先快后慢
            renderer.DOFade(0f, 1f) // 1秒内淡出
                .OnComplete(() => GameObject.Destroy(acidGO)); // 完成后销毁
        }
        
        /// <summary>在指定位置创建音波扩散特效。</summary>
        private void CreateSoundWaveEffect(Vector3 position)
        {
            // 示例：创建3个同心圆环模拟音波扩散
            for (int i = 0; i < 3; i++)
            {
                GameObject waveGO = new GameObject("SoundWaveEffect");
                waveGO.transform.position = position; // 音波中心位置
                
                var renderer = waveGO.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateCircleSprite(); // 创建一个简单的圆环精灵
                renderer.color = new Color(0.8f, 0.3f, 1f, 0.5f); // 设置为半透明的紫色或品红色
                renderer.sortingOrder = 3;
                
                // 动画：圆环从中心点小规模逐渐扩大并淡出
                waveGO.transform.localScale = Vector3.zero; // 初始大小为0
                float delay = i * 0.15f; // 每个圆环的扩散略有延迟
                
                // 使用DOTween的延迟调用来错开每个圆环的动画开始时间
                DOVirtual.DelayedCall(delay, () => {
                    if (waveGO != null) // 检查GameObject是否依然存在
                    {
                        waveGO.transform.DOScale(Vector3.one * (2f + i*0.5f) , 0.8f - delay*0.5f).SetEase(Ease.OutQuad); // 0.8秒内放大到最终尺寸
                        renderer.DOFade(0f, 0.8f - delay*0.5f).SetEase(Ease.InQuad) // 同时淡出
                            .OnComplete(() => GameObject.Destroy(waveGO)); // 动画完成后销毁
                    }
                });
            }
        }
        
        // --- 辅助方法 (Helper Methods for AI Logic) ---
        /// <summary>（占位）查找指定位置和半径内最近的建筑。实际应调用建筑系统。</summary>
        private BuildingData FindNearestBuilding(Vector2 position, float radius)
        {
            // TODO: 实现与建筑系统的交互，查找范围内的建筑并返回最近的一个
            // return buildingSystem.GetBuildingsInRadius(position, radius)
            //                     .OrderBy(b => Vector2.Distance(position, b.Position))
            //                     .FirstOrDefault();
            return null; // 临时返回null
        }
        
        /// <summary>（占位）查找指定位置和半径内最近的防御型建筑。实际应调用建筑系统并筛选类型。</summary>
        private BuildingData FindNearestDefenseBuilding(Vector2 position, float radius)
        {
            // TODO: 实现查找防御建筑的逻辑
            // return buildingSystem.GetBuildingsByCategory(BuildingCategory.Defense)
            //                     .Where(b => Vector2.Distance(position, b.Position) <= radius)
            //                     .OrderBy(b => Vector2.Distance(position, b.Position))
            //                     .FirstOrDefault();
            return null; // 临时返回null
        }
        
        /// <summary>（占位）查找指定位置和半径内的所有建筑。实际应调用建筑系统。</summary>
        private List<BuildingData> FindBuildingsInRadius(Vector2 position, float radius)
        {
            // TODO: 实现返回范围内所有建筑的逻辑
            // return buildingSystem.GetBuildingsInRadius(position, radius);
            return new List<BuildingData>(); // 临时返回空列表
        }
        
        /// <summary>（辅助）从僵尸列表中查找附近的其他僵尸。</summary>
        private List<ZombieData> FindNearbyZombies(Vector2 position, float radius, List<ZombieData> allZombies)
        {
            var nearbyZombies = new List<ZombieData>();
            if (allZombies == null) return nearbyZombies;

            foreach (var zombie in allZombies) // 遍历传入的僵尸列表
            {
                if (!zombie.IsAlive) continue; // 只考虑存活的僵尸
                
                if (Vector2.Distance(position, zombie.position) <= radius) // 如果距离在指定半径内
                {
                    nearbyZombies.Add(zombie); // 添加到附近僵尸列表
                }
            }
            return nearbyZombies;
        }
        
        /// <summary>（辅助）从一组僵尸中选出血量最高的一个作为领袖。</summary>
        private ZombieData GetGroupLeader(List<ZombieData> zombies)
        {
            if (zombies == null || !zombies.Any()) return null;

            ZombieData leader = null;
            float maxHealth = -1f; // 使用-1确保第一个僵尸的血量能被选中
            
            foreach (var zombie in zombies)
            {
                if (zombie.IsAlive && zombie.currentHealth > maxHealth) // 必须是存活的且血量更高
                {
                    maxHealth = zombie.currentHealth;
                    leader = zombie;
                }
            }
            return leader; // 返回血量最高的僵尸作为领袖
        }
        
        // --- 动态创建精灵的辅助方法 (Helper Methods for Dynamic Sprite Creation) ---
        // 这些方法用于为简单的视觉特效动态创建Texture2D和Sprite。
        // 在实际项目中，更推荐使用预制的Sprite或粒子系统。

        /// <summary>创建一个简单的白色线条精灵。</summary>
        private Sprite CreateLineSprite()
        {
            int width = 20, height = 2; // 线条的宽度和高度
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true); // 创建纹理，禁用mipmaps，线性颜色空间
            Color[] pixels = new Color[width * height];
            for(int i=0; i < pixels.Length; ++i) pixels[i] = Color.white; // 所有像素设为白色
            texture.SetPixels(pixels);
            texture.Apply(); // 应用更改
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f); // 100 pixels per unit
        }
        
        /// <summary>创建一个简单的绿色圆形（代表酸液点）精灵。</summary>
        private Sprite CreateAcidSprite()
        {
            int size = 8; // 酸液点精灵的边长
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            Color[] pixels = new Color[size*size];
            Vector2 center = new Vector2(size * 0.5f - 0.5f, size * 0.5f - 0.5f); // 纹理中心点 (调整0.5以使圆心在像素中心)
            float radius = size * 0.4f; // 圆形半径

            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    if (Vector2.Distance(new Vector2(x, y), center) <= radius) { // 如果像素在圆内
                        pixels[y * size + x] = Color.green; // 设为绿色
                    } else {
                        pixels[y * size + x] = Color.clear; // 否则设为透明
                    }
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
        
        /// <summary>创建一个简单的空心圆环精灵（代表音波）。</summary>
        private Sprite CreateCircleSprite()
        {
            int size = 16; // 圆环精灵的边长
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            Color[] pixels = new Color[size*size];
            Vector2 center = new Vector2(size * 0.5f - 0.5f, size * 0.5f - 0.5f);
            float outerRadius = size * 0.45f; // 外圆半径
            float innerRadius = size * 0.30f; // 内圆半径 (形成圆环)

             for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    float distance = Vector2.Distance(new Vector2(x,y), center);
                    if (distance <= outerRadius && distance >= innerRadius) { // 如果像素在圆环内
                        pixels[y * size + x] = Color.white; // 设为白色
                    } else {
                        pixels[y * size + x] = Color.clear; // 否则设为透明
                    }
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
} 