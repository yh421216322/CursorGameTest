// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：DefenseSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件实现了防御系统 (DefenseSystem)，负责管理游戏中的防御塔的建造、
//     升级、索敌、攻击以及投射物的逻辑处理。它还包括战斗状态管理和
//     防御相关的统计数据。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.Model;
// using SurvivalGame.GameSystem; // GameSystem命名空间已在此文件中，通常不需要再using

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 防御系统接口。
    /// 定义了防御系统的核心功能，包括防御塔管理、战斗状态管理、信息查询、目标选择和伤害处理。
    /// </summary>
    public interface IDefenseSystem : QFISystem
    {
        // --- 防御塔管理 (Tower Management) ---
        /// <summary>
        /// 在指定位置建造一座指定类型的防御塔。
        /// </summary>
        /// <param name="position">防御塔的建造位置。</param>
        /// <param name="type">要建造的防御塔类型。</param>
        /// <returns>成功建造则返回防御塔的唯一ID，否则返回-1（或其他错误码）。</returns>
        int BuildTower(Vector3 position, TowerType type);
        /// <summary>
        /// 升级指定ID的防御塔。
        /// </summary>
        /// <param name="towerId">要升级的防御塔ID。</param>
        /// <returns>如果成功升级则返回true，否则返回false。</returns>
        bool UpgradeTower(int towerId);
        /// <summary>
        /// 出售指定ID的防御塔。
        /// </summary>
        /// <param name="towerId">要出售的防御塔ID。</param>
        /// <returns>如果成功出售则返回true，否则返回false。</returns>
        bool SellTower(int towerId);
        /// <summary>
        /// 修复指定ID的防御塔（通常是恢复其生命值）。
        /// </summary>
        /// <param name="towerId">要修复的防御塔ID。</param>
        /// <returns>如果成功修复则返回true，否则返回false。</returns>
        bool RepairTower(int towerId);
        
        // --- 战斗管理 (Combat Management) ---
        /// <summary>
        /// 开始战斗状态。
        /// </summary>
        void StartCombat();
        /// <summary>
        /// 结束战斗状态。
        /// </summary>
        void EndCombat();
        /// <summary>
        /// 检查当前是否处于战斗状态。
        /// </summary>
        /// <returns>如果处于战斗状态则返回true，否则返回false。</returns>
        bool IsInCombat();
        
        // --- 查询接口 (Query Interface) ---
        /// <summary>
        /// 获取所有当前存在的防御塔数据。
        /// </summary>
        /// <returns>防御塔数据列表。</returns>
        List<TowerData> GetAllTowers();
        /// <summary>
        /// 获取指定中心点和范围内的所有防御塔。
        /// </summary>
        /// <param name="center">中心位置。</param>
        /// <param name="range">查询范围半径。</param>
        /// <returns>范围内的防御塔数据列表。</returns>
        List<TowerData> GetTowersInRange(Vector3 center, float range);
        /// <summary>
        /// 获取所有当前活动（飞行中）的投射物数据。
        /// </summary>
        /// <returns>投射物数据列表。</returns>
        List<ProjectileData> GetAllProjectiles();
        /// <summary>
        /// 根据ID获取指定的防御塔数据。
        /// </summary>
        /// <param name="towerId">防御塔ID。</param>
        /// <returns>对应的TowerData实例，如果不存在则返回null。</returns>
        TowerData GetTower(int towerId);
        /// <summary>
        /// 获取当前的防御统计数据。
        /// </summary>
        /// <returns>DefenseStats实例。</returns>
        DefenseStats GetDefenseStats();
        
        // --- 目标系统 (Targeting System) ---
        /// <summary>
        /// 为指定位置和类型的防御塔寻找最佳攻击目标。
        /// </summary>
        /// <param name="towerPosition">防御塔的位置。</param>
        /// <param name="range">防御塔的攻击范围。</param>
        /// <param name="towerType">防御塔的类型（可能影响目标选择策略）。</param>
        /// <returns>最佳目标ZombieData实例，如果没有合适目标则返回null。</returns>
        ZombieData FindBestTarget(Vector3 towerPosition, float range, TowerType towerType);
        /// <summary>
        /// 查找指定中心点和范围内的所有僵尸目标。
        /// </summary>
        /// <param name="center">中心位置。</param>
        /// <param name="range">查询范围半径。</param>
        /// <returns>范围内的僵尸数据列表。</returns>
        List<ZombieData> FindTargetsInRange(Vector3 center, float range);
        
        // --- 系统更新 (System Update) ---
        /// <summary>
        /// 每帧调用的更新方法，用于处理防御塔攻击、投射物飞行等逻辑。
        /// </summary>
        void Update();
        
        // --- 伤害处理 (Damage Handling) ---
        /// <summary>
        /// 对指定的僵尸造成伤害。
        /// </summary>
        /// <param name="zombieId">目标僵尸的ID。</param>
        /// <param name="damage">造成的伤害量。</param>
        /// <param name="position">伤害发生的位置（用于特效等）。</param>
        void DealDamageToZombie(string zombieId, float damage, Vector3 position);
    }

    /// <summary>
    /// 防御系统实现类。
    /// 管理游戏中的防御塔、投射物、战斗状态及相关逻辑。
    /// </summary>
    public class DefenseSystem : AbstractSystem, IDefenseSystem
    {
        // --- 系统引用 (System References) ---
        /// <summary>
        /// 游戏核心数据模型的引用。
        /// </summary>
        private ISurvivalGameModel gameModel;
        /// <summary>
        /// 僵尸系统的引用，用于获取僵尸信息和对僵尸造成伤害。
        /// </summary>
        private IZombieSystem zombieSystem;
        /// <summary>
        /// 资源系统的引用，用于检查和消耗建造/升级防御塔所需的资源。
        /// </summary>
        private IResourceSystem resourceSystem;
        /// <summary>
        /// 高级科技系统的引用，可能用于防御塔的解锁或加成。
        /// </summary>
        private IAdvancedTechSystem techSystem;
        
        // --- 防御数据 (Defense Data) ---
        /// <summary>
        /// 存储所有防御塔数据的字典，键为防御塔ID。
        /// </summary>
        private Dictionary<int, TowerData> towers;
        /// <summary>
        /// 存储防御塔对应的游戏对象（GameObject）的字典，键为防御塔ID。用于视觉表现。
        /// </summary>
        private Dictionary<int, GameObject> towerObjects;
        /// <summary>
        /// 当前所有活动（飞行中）的投射物数据列表。
        /// </summary>
        private List<ProjectileData> projectiles;
        /// <summary>
        /// 存储投射物对应的游戏对象列表。用于视觉表现。
        /// </summary>
        private List<GameObject> projectileObjects;
        
        // --- 防御统计 (Defense Statistics) ---
        /// <summary>
        /// 存储与防御相关的统计数据，如击杀数、造成伤害等。
        /// </summary>
        private DefenseStats defenseStats;
        
        // --- 系统状态 (System State) ---
        /// <summary>
        /// 标记当前是否处于战斗状态。
        /// </summary>
        private bool inCombat;
        /// <summary>
        /// 用于生成下一个防御塔的唯一ID。
        /// </summary>
        private int nextTowerId = 1;
        /// <summary>
        /// 用于生成下一个投射物的唯一ID。
        /// </summary>
        private int nextProjectileId = 1;
        
        // --- 更新间隔控制 (Update Intervals) ---
        /// <summary>
        /// 上一次防御塔逻辑更新的时间戳。
        /// </summary>
        private float lastTowerUpdate;
        /// <summary>
        /// 上一次投射物逻辑更新的时间戳。
        /// </summary>
        private float lastProjectileUpdate;
        /// <summary>
        /// 上一次目标选择逻辑更新的时间戳。
        /// </summary>
        private float lastTargetingUpdate;
        
        /// <summary>
        /// 防御塔逻辑（如索敌、开火判定）的更新频率（秒）。
        /// </summary>
        private const float TOWER_UPDATE_INTERVAL = 0.1f;       // 防御塔每0.1秒更新一次
        /// <summary>
        /// 投射物逻辑（如移动、命中判定）的更新频率（秒）。
        /// </summary>
        private const float PROJECTILE_UPDATE_INTERVAL = 0.02f; // 投射物每0.02秒更新一次 (更平滑)
        /// <summary>
        /// 目标选择逻辑的更新频率（秒）。
        /// </summary>
        private const float TARGETING_UPDATE_INTERVAL = 0.2f;   // 目标选择每0.2秒更新一次

        /// <summary>
        /// 系统初始化方法。
        /// 获取其他系统引用，初始化数据结构、统计数据和状态。
        /// </summary>
        protected override void OnInit()
        {
            // 获取其他所需系统的引用
            gameModel = this.GetModel<ISurvivalGameModel>();
            zombieSystem = this.GetSystem<IZombieSystem>();
            resourceSystem = this.GetSystem<IResourceSystem>();
            techSystem = this.GetSystem<IAdvancedTechSystem>();
            
            // 初始化数据存储结构
            towers = new Dictionary<int, TowerData>();
            towerObjects = new Dictionary<int, GameObject>();
            projectiles = new List<ProjectileData>();
            projectileObjects = new List<GameObject>();
            
            // 初始化防御统计对象
            defenseStats = new DefenseStats();
            
            // 注册感兴趣的事件 (事件类型前缀 MyGameNamespace 仅为示例，应替换为实际的命名空间)
            this.RegisterEvent<MyGameNamespace.ZombieSpawnedEvent>(OnZombieSpawned); // 监听僵尸生成事件
            this.RegisterEvent<MyGameNamespace.ZombieDeathEvent>(OnZombieDeath);     // 监听僵尸死亡事件
            
            // 初始化系统状态
            inCombat = false; // 游戏开始时非战斗状态
            
            // 初始化更新时间戳，确保第一次更新会立即执行或按预期执行
            lastTowerUpdate = Time.time;
            lastProjectileUpdate = Time.time;
            lastTargetingUpdate = Time.time;
            
           // Debug.Log("[防御系统] 初始化完成。"); // 调试日志：防御系统初始化完成
        }

        /// <summary>
        /// 处理僵尸生成事件。
        /// 当有僵尸生成时，如果当前不处于战斗状态且有存活僵尸，则启动战斗状态。
        /// </summary>
        /// <param name="e">僵尸生成事件参数。</param>
        private void OnZombieSpawned(MyGameNamespace.ZombieSpawnedEvent e)
        {
            // 如果当前非战斗状态，并且游戏中有活动的僵尸，则切换到战斗状态
            if (!inCombat && zombieSystem.GetActiveZombieCount() > 0) // GetActiveZombieCount() 是IZombieSystem中假设的方法
            {
                StartCombat(); // 开始战斗
            }
        }

        /// <summary>
        /// 处理僵尸死亡事件。
        /// 更新击杀统计，并检查是否所有僵尸已被消灭以结束战斗状态。
        /// </summary>
        /// <param name="e">僵尸死亡事件参数。</param>
        private void OnZombieDeath(MyGameNamespace.ZombieDeathEvent e)
        {
            // 更新防御统计中的总击杀数
            defenseStats.totalZombiesKilled++;
            
            // 如果当前处于战斗状态，并且所有活动的僵尸都已被消灭，则结束战斗状态
            if (inCombat && zombieSystem.GetActiveZombieCount() == 0)
            {
                EndCombat(); // 结束战斗
            }
        }

        /// <summary>
        /// 防御系统的主更新方法，由游戏主循环每帧调用。
        /// 按预设的时间间隔分别调用防御塔、投射物和目标选择的更新逻辑。
        /// </summary>
        public void Update()
        {
            float currentTime = Time.time; // 获取当前游戏时间
            
            // 按固定间隔更新防御塔逻辑
            if (currentTime - lastTowerUpdate >= TOWER_UPDATE_INTERVAL)
            {
                UpdateTowers(); // 调用防御塔更新方法
                lastTowerUpdate = currentTime; // 更新上次防御塔更新的时间戳
            }
            
            // 按固定间隔更新投射物逻辑
            if (currentTime - lastProjectileUpdate >= PROJECTILE_UPDATE_INTERVAL)
            {
                UpdateProjectiles(); // 调用投射物更新方法
                lastProjectileUpdate = currentTime; // 更新上次投射物更新的时间戳
            }
            
            // 按固定间隔更新目标选择逻辑
            if (currentTime - lastTargetingUpdate >= TARGETING_UPDATE_INTERVAL)
            {
                UpdateTargeting(); // 调用目标选择更新方法
                lastTargetingUpdate = currentTime; // 更新上次目标选择更新的时间戳
            }
        }

        /// <summary>
        /// 更新所有活动防御塔的状态和行为，如冷却、索敌和开火。
        /// </summary>
        private void UpdateTowers()
        {
            foreach (var tower in towers.Values) // 遍历所有已建造的防御塔
            {
                // 跳过不活动或已损坏的防御塔
                if (!tower.isActive || tower.currentHealth <= 0)
                    continue;

                // 更新防御塔的开火冷却时间
                if (tower.lastFireTime > 0) // 如果不是第一次开火
                {
                    tower.timeSinceLastFire = Time.time - tower.lastFireTime; // 计算自上次开火以来经过的时间
                }
                else // 处理第一次开火的情况，确保可以立即尝试开火
                {
                    tower.timeSinceLastFire = tower.fireRate; // 将冷却时间设置为满足开火条件
                }

                // 如果冷却时间已到，则尝试寻找目标并攻击
                if (tower.timeSinceLastFire >= tower.fireRate)
                {
                    var target = FindBestTarget(tower.position, tower.range, tower.type); // 寻找最佳目标
                    if (target != null) // 如果找到目标
                    {
                        FireAtTarget(tower, target); // 向目标开火
                    }
                }
            }
        }

        /// <summary>
        /// 控制防御塔向指定目标开火，创建并初始化投射物。
        /// </summary>
        /// <param name="tower">开火的防御塔数据。</param>
        /// <param name="target">攻击的目标僵尸数据。</param>
        private void FireAtTarget(TowerData tower, ZombieData target)
        {
            // 创建投射物数据实例
            var projectile = new ProjectileData
            {
                id = nextProjectileId++, // 分配唯一ID
                towerId = tower.id,      // 发射此投射物的塔ID
                damage = tower.damage,   // 投射物伤害（继承自塔）
                speed = tower.projectileSpeed, // 投射物飞行速度
                position = tower.position,     // 初始位置（塔的位置）
                targetPosition = target.position, // 目标位置（僵尸当前位置）
                targetZombieId = target.id,       // 目标僵尸ID
                creationTime = Time.time,         // 创建时间戳
                type = GetProjectileType(tower.type) // 根据塔类型确定投射物类型
            };

            projectiles.Add(projectile); // 将新投射物添加到活动列表
            CreateProjectileVisual(projectile); // 创建投射物的视觉表现

            // 更新防御塔的开火状态
            tower.lastFireTime = Time.time; // 重置上次开火时间
            tower.timeSinceLastFire = 0f;   // 重置冷却计时器
            tower.totalShotsFired++;      // 增加总射击次数统计

            // TODO: 调用音效系统播放射击音效
            // AudioManager.PlaySoundEffect("tower_fire", tower.position);

            Debug.Log($"[防御系统] 防御塔 {tower.id} ({tower.type}) 向僵尸 {target.id} 开火，伤害: {tower.damage}");
        }

        /// <summary>
        /// 根据防御塔类型获取对应的投射物类型。
        /// </summary>
        /// <param name="towerType">防御塔的类型。</param>
        /// <returns>相应的投射物类型。</returns>
        private ProjectileType GetProjectileType(TowerType towerType)
        {
            switch (towerType) // 根据不同的防御塔类型返回不同的投射物类型
            {
                case TowerType.Basic:  return ProjectileType.Bullet;       // 基础塔发射子弹
                case TowerType.Heavy:  return ProjectileType.Shell;        // 重型塔发射炮弹
                case TowerType.Sniper: return ProjectileType.SniperBullet; // 狙击塔发射狙击子弹
                case TowerType.Splash: return ProjectileType.Grenade;      // 范围塔发射手榴弹/榴弹
                default:               return ProjectileType.Bullet;       // 默认返回子弹类型
            }
        }

        /// <summary>
        /// 创建投射物的视觉表现（例如，一个简单的球体）。
        /// </summary>
        /// <param name="projectile">投射物数据。</param>
        private void CreateProjectileVisual(ProjectileData projectile)
        {
            // 创建一个基础球体作为投射物的视觉效果 (仅为示例，实际项目中会使用预制件)
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = $"Projectile_{projectile.id}"; // 命名以便于调试
            projectileObject.transform.position = projectile.position; // 设置初始位置
            projectileObject.transform.localScale = Vector3.one * 0.1f; // 设置大小
            
            // 根据投射物类型设置不同的颜色以区分
            var renderer = projectileObject.GetComponent<Renderer>();
            if (renderer != null) // 确保渲染器存在
            {
                switch (projectile.type)
                {
                    case ProjectileType.Bullet:       renderer.material.color = Color.yellow; break;
                    case ProjectileType.Shell:        renderer.material.color = Color.red;    break;
                    case ProjectileType.SniperBullet: renderer.material.color = Color.green;  break;
                    case ProjectileType.Grenade:      renderer.material.color = new Color(1f, 0.5f, 0f); break; // 橙色
                    default:                          renderer.material.color = Color.white;  break;
                }
            }

            // TODO: 实际项目中，这里会实例化一个预制件，并可能附加一个脚本来处理投射物的复杂行为
            projectileObjects.Add(projectileObject); // 添加到视觉对象列表
        }

        /// <summary>
        /// 更新所有活动投射物的状态，如移动和命中检测。
        /// </summary>
        private void UpdateProjectiles()
        {
            // 从后向前遍历列表，以便在飞行过程中安全地移除投射物
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                
                // 计算投射物当前应在的位置 (简单线性插值，实际可能需要更复杂的弹道)
                float flightDuration = Time.time - projectile.creationTime; // 已飞行时间
                // 预期总飞行时间 = 距离 / 速度 (如果目标固定)。此处简化为直接朝向初始目标位置移动。
                // 如果目标会移动，需要动态更新 targetPosition 或采用预测拦截逻辑。
                
                Vector3 direction = (projectile.targetPosition - projectile.position).normalized; // 飞行方向
                Vector3 expectedPosition = projectile.position + direction * projectile.speed * flightDuration; // 当前帧的预期位置
                
                // 更新视觉对象的位置
                // 检查索引是否有效，防止在投射物数据和视觉对象列表不同步时出错
                if (i < projectileObjects.Count && projectileObjects[i] != null)
                {
                    projectileObjects[i].transform.position = expectedPosition;
                }
                
                // 检查是否到达或超过目标位置 (简单距离判断)
                // 注意：如果速度很快或帧率低，可能会穿过目标。更鲁棒的检测是检查是否穿过了目标平面。
                float distanceToOriginalTarget = Vector3.Distance(expectedPosition, projectile.targetPosition);
                if (distanceToOriginalTarget <= 0.2f || Vector3.Dot(direction, projectile.targetPosition - expectedPosition) < 0) // 0.2f 作为命中阈值，或者检查是否飞过目标点
                {
                    HitTarget(projectile); // 处理命中逻辑
                    
                    // 清理此投射物（数据和视觉对象）
                    if (i < projectileObjects.Count && projectileObjects[i] != null)
                    {
                        GameObject.Destroy(projectileObjects[i]); // 销毁视觉对象
                        projectileObjects.RemoveAt(i);            // 从列表中移除引用
                    }
                    projectiles.RemoveAt(i); // 从数据列表中移除
                }
                else if (flightDuration > 5f) // 超时清理 (例如投射物飞行超过5秒)
                {
                    Debug.Log($"[防御系统] 投射物 {projectile.id} 超时，自动清理。");
                    if (i < projectileObjects.Count && projectileObjects[i] != null)
                    {
                        GameObject.Destroy(projectileObjects[i]);
                        projectileObjects.RemoveAt(i);
                    }
                    projectiles.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 处理投射物命中目标的逻辑。
        /// </summary>
        /// <param name="projectile">命中的投射物数据。</param>
        private void HitTarget(ProjectileData projectile)
        {
            // 对目标僵尸造成伤害
            DealDamageToZombie(projectile.targetZombieId, projectile.damage, projectile.targetPosition);
            
            // 更新防御统计数据
            defenseStats.totalDamageDealt += projectile.damage; // 累加总伤害
            
            // 如果是范围伤害类型（如手榴弹），则处理溅射伤害
            if (projectile.type == ProjectileType.Grenade)
            {
                // 假设溅射伤害是主伤害的50%，范围2米
                HandleSplashDamage(projectile.targetPosition, projectile.damage * 0.5f, 2f);
            }
            // TODO: 创建命中特效，如爆炸、火花等
            // EffectManager.CreateHitEffect(projectile.targetPosition, projectile.type);
        }

        /// <summary>
        /// 处理范围（溅射）伤害。
        /// </summary>
        /// <param name="center">伤害中心点。</param>
        /// <param name="damage">对范围内每个目标造成的伤害量。</param>
        /// <param name="radius">伤害半径。</param>
        private void HandleSplashDamage(Vector3 center, float damage, float radius)
        {
            // 查找伤害中心点附近的所有僵尸
            var nearbyZombies = FindTargetsInRange(center, radius);
            foreach (var zombie in nearbyZombies)
            {
                // 对范围内的每个僵尸造成溅射伤害
                DealDamageToZombie(zombie.id, damage, zombie.position);
                // Debug.Log($"[防御系统] 僵尸 {zombie.id} 受到溅射伤害 {damage}");
            }
        }

        /// <summary>
        /// 对指定的僵尸造成伤害，并可能触发伤害特效。
        /// </summary>
        /// <param name="zombieId">目标僵尸的ID。</param>
        /// <param name="damage">造成的伤害值。</param>
        /// <param name="position">伤害发生的位置（用于特效）。</param>
        public void DealDamageToZombie(string zombieId, float damage, Vector3 position)
        {
            zombieSystem.TakeDamageToZombie(zombieId, damage); // 调用僵尸系统处理伤害
            
            // 创建伤害数字或视觉特效
            CreateDamageEffect(position, damage);
        }

        /// <summary>
        /// （占位）创建伤害视觉特效，如伤害数字跳动。
        /// </summary>
        /// <param name="position">特效产生位置。</param>
        /// <param name="damage">伤害数值（用于显示）。</param>
        private void CreateDamageEffect(Vector3 position, float damage)
        {
            // TODO: 实现具体的伤害数字显示或粒子特效等
            // 例如: FloatingTextManager.ShowDamageNumber(damage, position, Color.red);
            Debug.Log($"[防御系统] 在位置 {position} 对僵尸造成伤害: {damage}");
        }

        /// <summary>
        /// 更新所有防御塔的目标选择逻辑。
        /// </summary>
        private void UpdateTargeting()
        {
            foreach (var tower in towers.Values) // 遍历所有防御塔
            {
                // 跳过不活动或已损坏的塔
                if (!tower.isActive || tower.currentHealth <= 0)
                {
                    tower.currentTarget = null; // 清除其当前目标
                    continue;
                }

                // 为该塔寻找并更新最佳目标
                tower.currentTarget = FindBestTarget(tower.position, tower.range, tower.type);
            }
        }

        /// <summary>
        /// 根据防御塔的位置、范围和类型，从可用目标中选择最佳攻击目标。
        /// </summary>
        /// <param name="towerPosition">防御塔当前位置。</param>
        /// <param name="range">防御塔的攻击范围。</param>
        /// <param name="towerType">防御塔的类型，可能影响索敌策略。</param>
        /// <returns>最佳目标ZombieData；如果没有有效目标，则返回null。</returns>
        public ZombieData FindBestTarget(Vector3 towerPosition, float range, TowerType towerType)
        {
            // 获取在塔攻击范围内的所有存活僵尸
            var targetsInRange = FindTargetsInRange(towerPosition, range);
            if (!targetsInRange.Any()) // 如果范围内没有目标
                return null;

            // 根据防御塔类型应用不同的索敌策略
            switch (towerType)
            {
                case TowerType.Basic:  // 基础塔：优先攻击最近的僵尸
                    return targetsInRange.OrderBy(z => Vector3.Distance(towerPosition, z.position)).FirstOrDefault();
                
                case TowerType.Heavy:  // 重型塔：优先攻击生命值最高的僵尸 (假设其能有效打击强力单位)
                    return targetsInRange.OrderByDescending(z => z.currentHealth).FirstOrDefault();
                
                case TowerType.Sniper: // 狙击塔：优先攻击最远的僵尸 (发挥其射程优势)
                    return targetsInRange.OrderByDescending(z => Vector3.Distance(towerPosition, z.position)).FirstOrDefault();
                
                case TowerType.Splash: // 范围伤害塔：优先攻击能波及最多僵尸的目标点 (或僵尸群中心)
                    return FindBestSplashTarget(targetsInRange, towerPosition); // 调用特定方法寻找溅射最佳目标
                
                default: // 其他或未知类型：默认选择列表中的第一个目标
                    return targetsInRange.FirstOrDefault();
            }
        }

        /// <summary>
        /// 为范围伤害塔查找最佳攻击目标（通常是僵尸最密集区域的某个僵尸）。
        /// </summary>
        /// <param name="targets">已在塔攻击范围内的目标列表。</param>
        /// <param name="towerPosition">防御塔位置（当前未使用，但可能用于更复杂逻辑）。</param>
        /// <returns>被选为溅射中心的ZombieData；如果列表为空则返回null。</returns>
        private ZombieData FindBestSplashTarget(List<ZombieData> targets, Vector3 towerPosition)
        {
            if (!targets.Any()) return null;

            ZombieData bestTargetCandidate = null;
            int maxNearbyZombiesCount = 0;

            // 遍历范围内每个僵尸，计算其小范围内的僵尸数量，选择数量最多的那个作为溅射中心
            foreach (var potentialTarget in targets)
            {
                // 假设溅射半径为2米，计算以此僵尸为中心2米内的僵尸数量
                int zombiesInSplashRadius = zombieSystem.GetZombiesNearPosition(potentialTarget.position, 2f).Count;
                if (zombiesInSplashRadius > maxNearbyZombiesCount)
                {
                    maxNearbyZombiesCount = zombiesInSplashRadius;
                    bestTargetCandidate = potentialTarget;
                }
            }
            // 如果没有找到特别密集的点，则退回选择列表中的第一个目标
            return bestTargetCandidate ?? targets.FirstOrDefault();
        }

        /// <summary>
        /// 获取指定中心点和范围内的所有存活僵尸。
        /// </summary>
        /// <param name="center">搜索中心点。</param>
        /// <param name="range">搜索半径。</param>
        /// <returns>范围内所有存活僵尸的数据列表。</returns>
        public List<ZombieData> FindTargetsInRange(Vector3 center, float range)
        {
            // 调用僵尸系统的方法获取指定位置和范围内的僵尸
            return zombieSystem.GetZombiesNearPosition(center, range)
                .Where(z => z.IsAlive) // 筛选出仍然存活的僵尸
                .ToList();
        }

        /// <summary>
        /// 建造一座新的防御塔。
        /// </summary>
        /// <param name="position">建造位置。</param>
        /// <param name="type">防御塔类型。</param>
        /// <returns>成功则返回防御塔ID，失败则返回-1。</returns>
        public int BuildTower(Vector3 position, TowerType type)
        {
            // 获取该类型防御塔的配置信息
            var config = GetTowerConfig(type);
            // 检查是否有足够资源建造
            if (!HasSufficientResources(config.buildCost))
            {
                Debug.LogWarning($"[防御系统] 资源不足，无法建造 {type} 防御塔。");
                return -1; // 表示建造失败
            }

            // 创建防御塔数据实例
            var tower = new TowerData
            {
                id = nextTowerId++, // 分配并递增唯一ID
                type = type,
                position = position,
                level = 1, // 初始等级为1
                currentHealth = config.maxHealth, // 初始血量为最大血量
                maxHealth = config.maxHealth,
                damage = config.damage,
                range = config.range,
                fireRate = config.fireRate,
                projectileSpeed = config.projectileSpeed,
                isActive = true, // 初始为激活状态
                buildTime = Time.time, // 记录建造完成的时间戳 (或者理解为开始建造的时间)
                totalShotsFired = 0,
                totalKills = 0,
                timeSinceLastFire = config.fireRate // 确保首次可以立即尝试开火
            };

            towers[tower.id] = tower; // 添加到防御塔字典
            CreateTowerVisual(tower); // 创建防御塔的视觉表现

            ConsumeResources(config.buildCost); // 消耗建造资源

            // 更新防御统计数据
            defenseStats.totalTowers++;       // 总塔数增加
            defenseStats.operationalTowers++; // 可运作塔数增加

            Debug.Log($"[防御系统] 成功建造防御塔 {tower.id} (类型: {type}) 于位置: {position}");
            return tower.id; // 返回新建塔的ID
        }

        /// <summary>
        /// 根据防御塔类型获取其配置信息（伤害、射程、建造成本等）。
        /// 注意：此处为硬编码示例，实际项目中应从配置文件或数据管理系统中读取。
        /// </summary>
        /// <param name="type">防御塔类型。</param>
        /// <returns>对应的TowerConfig实例。</returns>
        private TowerConfig GetTowerConfig(TowerType type)
        {
            // 实际项目中，这些配置应从外部文件（如JSON, XML, ScriptableObject）加载
            switch (type)
            {
                case TowerType.Basic:
                    return new TowerConfig {
                        maxHealth = 100f, damage = 20f, range = 5f, fireRate = 1f, projectileSpeed = 10f,
                        buildCost = new Dictionary<ResourceType, int> { { ResourceType.Materials, 50 } }
                    };
                case TowerType.Heavy:
                    return new TowerConfig {
                        maxHealth = 150f, damage = 50f, range = 4f, fireRate = 2f, projectileSpeed = 8f,
                        buildCost = new Dictionary<ResourceType, int> { { ResourceType.Materials, 100 }, { ResourceType.Ammunition, 25 } }
                    };
                case TowerType.Sniper:
                    return new TowerConfig {
                        maxHealth = 80f, damage = 80f, range = 8f, fireRate = 3f, projectileSpeed = 15f,
                        buildCost = new Dictionary<ResourceType, int> { { ResourceType.Materials, 75 }, { ResourceType.Ammunition, 50 } }
                    };
                case TowerType.Splash:
                    return new TowerConfig {
                        maxHealth = 120f, damage = 30f, range = 4f, fireRate = 2.5f, projectileSpeed = 6f, // 溅射塔本身伤害可能不高，但有范围效果
                        buildCost = new Dictionary<ResourceType, int> { { ResourceType.Materials, 80 }, { ResourceType.Ammunition, 40 } }
                    };
                default:
                    Debug.LogError($"[防御系统] 未知的防御塔类型: {type}，无法获取配置。");
                    return new TowerConfig(); // 返回一个默认空配置或抛出异常
            }
        }

        /// <summary>
        /// 检查当前是否拥有足够的资源来支付指定的成本。
        /// </summary>
        /// <param name="cost">资源成本字典（资源类型 -> 数量）。</param>
        /// <returns>如果资源充足则返回true，否则返回false。</returns>
        private bool HasSufficientResources(Dictionary<ResourceType, int> cost)
        {
            if (cost == null) return true; // 如果没有成本，则视为资源充足
            foreach (var requirement in cost) // 遍历每一种资源成本
            {
                // 调用资源系统查询当前资源量是否小于所需量
                if (resourceSystem.GetResourceAmount(requirement.Key) < requirement.Value)
                    return false; // 只要有一种资源不足，即判定为资源不足
            }
            return true; // 所有资源都充足
        }

        /// <summary>
        /// 消耗指定的资源。
        /// </summary>
        /// <param name="cost">要消耗的资源列表（资源类型 -> 数量）。</param>
        private void ConsumeResources(Dictionary<ResourceType, int> cost)
        {
            if (cost == null) return;
            foreach (var requirement in cost) // 遍历每一种要消耗的资源
            {
                // 调用资源系统尝试消耗资源 (TryConsumeResource通常会处理不足的情况，但此处假设HasSufficientResources已检查过)
                resourceSystem.TryConsumeResource(requirement.Key, requirement.Value);
            }
        }

        /// <summary>
        /// 创建防御塔的视觉表现（例如，一个简单的圆柱体）。
        /// </summary>
        /// <param name="tower">防御塔数据。</param>
        private void CreateTowerVisual(TowerData tower)
        {
            // 创建一个基础圆柱体作为防御塔的视觉效果 (仅为示例)
            GameObject towerObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            towerObject.name = $"Tower_{tower.id}_{tower.type}"; // 命名
            towerObject.transform.position = tower.position;    // 设置位置
            towerObject.transform.localScale = new Vector3(0.8f, 1f, 0.8f); // 设置大小
            
            // 根据防御塔类型设置不同的颜色以区分
            var renderer = towerObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                switch (tower.type)
                {
                    case TowerType.Basic:  renderer.material.color = Color.gray; break;
                    case TowerType.Heavy:  renderer.material.color = Color.red; break;
                    case TowerType.Sniper: renderer.material.color = Color.green; break;
                    case TowerType.Splash: renderer.material.color = new Color(1f, 0.5f, 0f); break; // 橙色
                    default:               renderer.material.color = Color.blue; break; // 默认蓝色
                }
            }

            towerObjects[tower.id] = towerObject; // 将创建的GameObject存入字典，与塔数据关联
        }

        /// <summary>
        /// 启动战斗状态。
        /// </summary>
        public void StartCombat()
        {
            if (inCombat) return; // 如果已在战斗中，则不重复执行
            inCombat = true;
            Debug.Log("[防御系统] 进入战斗状态！僵尸来袭！");
            // TODO: 可能需要通知UI或其他系统进入战斗模式
            // this.SendEvent(new CombatStateChangedEvent { IsInCombat = true });
        }

        /// <summary>
        /// 结束战斗状态。
        /// </summary>
        public void EndCombat()
        {
            if (!inCombat) return; // 如果已不在战斗中，则不重复执行
            inCombat = false;
            Debug.Log("[防御系统] 战斗结束。区域暂时安全。");
            // TODO: 可能需要通知UI或其他系统解除战斗模式
            // this.SendEvent(new CombatStateChangedEvent { IsInCombat = false });
        }

        /// <summary>
        /// 查询当前是否处于战斗状态。
        /// </summary>
        /// <returns>如果正在战斗则为true，否则为false。</returns>
        public bool IsInCombat() => inCombat;

        // --- 其他接口方法的实现 (Implementations for other interface methods) ---
        /// <summary>
        /// 升级指定ID的防御塔。
        /// （示例实现：等级+1，伤害、射程、血量按比例提升）
        /// </summary>
        /// <param name="towerId">要升级的防御塔ID。</param>
        /// <returns>成功升级返回true，否则false。</returns>
        public bool UpgradeTower(int towerId) 
        { 
            if (towers.TryGetValue(towerId, out var tower)) // 使用TryGetValue更安全
            {
                // TODO: 实际升级应检查资源、科技，并从TowerConfig获取升级后的属性
                tower.level++; // 等级提升
                tower.damage *= 1.2f; // 伤害提升20%
                tower.range *= 1.1f;  // 射程提升10%
                tower.maxHealth *= 1.15f; // 最大血量提升15%
                tower.currentHealth = tower.maxHealth; // 升级后血量补满
                Debug.Log($"[防御系统] 防御塔 {towerId} 已升级到等级 {tower.level}。");
                // TODO: 可能需要更新防御塔的视觉表现
                return true;
            }
            Debug.LogWarning($"[防御系统] 尝试升级失败：找不到防御塔ID {towerId}。");
            return false; 
        }
        
        /// <summary>
        /// 出售指定ID的防御塔。
        /// （示例实现：移除塔，返还部分资源）
        /// </summary>
        /// <param name="towerId">要出售的防御塔ID。</param>
        /// <returns>成功出售返回true，否则false。</returns>
        public bool SellTower(int towerId) 
        { 
            if (towers.ContainsKey(towerId)) // 检查塔是否存在
            {
                // TODO: 返还部分建造成本资源 (例如50%)
                // var config = GetTowerConfig(towers[towerId].type);
                // foreach(var cost in config.buildCost) { resourceSystem.AddResource(cost.Key, Mathf.FloorToInt(cost.Value * 0.5f)); }

                towers.Remove(towerId); // 从数据中移除
                if (towerObjects.TryGetValue(towerId, out var towerVisual)) // 检查是否有视觉对象
                {
                    GameObject.Destroy(towerVisual); // 销毁视觉对象
                    towerObjects.Remove(towerId);    // 从视觉对象字典中移除
                }
                defenseStats.totalTowers--; // 更新统计
                defenseStats.operationalTowers--; // 假设被出售的塔是可运作的
                Debug.Log($"[防御系统] 防御塔 {towerId} 已被出售。");
                return true;
            }
            Debug.LogWarning($"[防御系统] 尝试出售失败：找不到防御塔ID {towerId}。");
            return false; 
        }
        
        /// <summary>
        /// 修复指定ID的防御塔，将其当前生命值恢复到最大值。
        /// </summary>
        /// <param name="towerId">要修复的防御塔ID。</param>
        /// <returns>成功修复返回true，否则false。</returns>
        public bool RepairTower(int towerId) 
        { 
            if (towers.TryGetValue(towerId, out var tower)) // 使用TryGetValue
            {
                // TODO: 修复可能需要消耗资源
                tower.currentHealth = tower.maxHealth; // 血量恢复到最大
                Debug.Log($"[防御系统] 防御塔 {towerId} 已被完全修复。");
                return true;
            }
            Debug.LogWarning($"[防御系统] 尝试修复失败：找不到防御塔ID {towerId}。");
            return false; 
        }
        
        /// <summary>获取所有防御塔的数据列表。</summary>
        public List<TowerData> GetAllTowers() => towers.Values.ToList(); // 返回塔数据的值集合的列表副本

        /// <summary>获取指定中心点和范围内的所有防御塔。</summary>
        public List<TowerData> GetTowersInRange(Vector3 center, float range) 
        { 
            return towers.Values.Where(t => Vector3.Distance(t.position, center) <= range).ToList(); 
        }

        /// <summary>获取所有活动投射物的数据列表。</summary>
        public List<ProjectileData> GetAllProjectiles() => new List<ProjectileData>(projectiles); // 返回投射物列表的副本

        /// <summary>根据ID获取防御塔数据。</summary>
        public TowerData GetTower(int towerId) => towers.TryGetValue(towerId, out var tower) ? tower : null;

        /// <summary>获取当前的防御统计数据。</summary>
        public DefenseStats GetDefenseStats() => defenseStats; // 直接返回统计对象引用（如果允许外部修改）或其副本
    }

    /// <summary>
    /// 防御统计数据类。
    /// 用于记录与防御相关的各项统计信息。
    /// </summary>
    [System.Serializable]
    public class DefenseStats
    {
        /// <summary>
        /// 当前已建造的防御塔总数。
        /// </summary>
        public int totalTowers;
        /// <summary>
        /// 当前可运作的防御塔数量（例如，未损坏且激活的）。
        /// </summary>
        public int operationalTowers;
        /// <summary>
        /// 防御系统（或所有防御塔）累计击杀的僵尸总数。
        /// </summary>
        public int totalZombiesKilled;
        /// <summary>
        /// 防御系统（或所有防御塔）累计造成的总伤害量。
        /// </summary>
        public float totalDamageDealt;
        /// <summary>
        /// 当前场景中活动的（飞行中）投射物数量。
        /// </summary>
        public int activeProjectiles;
    }

    /// <summary>
    /// 防御塔配置数据类。
    /// 定义了特定类型防御塔的基础属性和建造成本。
    /// </summary>
    [System.Serializable]
    public class TowerConfig
    {
        /// <summary>
        /// 防御塔的最大生命值。
        /// </summary>
        public float maxHealth;
        /// <summary>
        /// 防御塔单次攻击造成的伤害。
        /// </summary>
        public float damage;
        /// <summary>
        /// 防御塔的攻击范围（半径）。
        /// </summary>
        public float range;
        /// <summary>
        /// 防御塔的攻击速率（次/秒 或 秒/次，取决于具体实现，此处理解为攻击间隔时间秒）。
        /// </summary>
        public float fireRate;
        /// <summary>
        /// 防御塔发射的投射物的飞行速度。
        /// </summary>
        public float projectileSpeed;
        /// <summary>
        /// 建造此防御塔所需的资源成本列表。
        /// </summary>
        public Dictionary<ResourceType, int> buildCost;
    }

    // 此处不再重复定义已在Model命名空间中定义的TowerType和ProjectileType枚举。
    // 假设它们已通过 `using SurvivalGame.Model;` 引入。
}