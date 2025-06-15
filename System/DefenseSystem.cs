using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.Model;
using SurvivalGame.GameSystem;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 防御系统接口
    /// </summary>
    public interface IDefenseSystem : QFISystem
    {
        // 防御塔管理
        int BuildTower(Vector3 position, TowerType type);
        bool UpgradeTower(int towerId);
        bool SellTower(int towerId);
        bool RepairTower(int towerId);
        
        // 战斗管理
        void StartCombat();
        void EndCombat();
        bool IsInCombat();
        
        // 查询接口
        List<TowerData> GetAllTowers();
        List<TowerData> GetTowersInRange(Vector3 center, float range);
        List<ProjectileData> GetAllProjectiles();
        TowerData GetTower(int towerId);
        DefenseStats GetDefenseStats();
        
        // 目标系统
        ZombieData FindBestTarget(Vector3 towerPosition, float range, TowerType towerType);
        List<ZombieData> FindTargetsInRange(Vector3 center, float range);
        
        // 系统更新
        void Update();
        
        // 伤害处理
        void DealDamageToZombie(string zombieId, float damage, Vector3 position);
    }

    /// <summary>
    /// 防御系统实现
    /// </summary>
    public class DefenseSystem : AbstractSystem, IDefenseSystem
    {
        private ISurvivalGameModel gameModel;
        private IZombieSystem zombieSystem;
        private IResourceSystem resourceSystem;
        private IAdvancedTechSystem techSystem;
        
        // 防御数据
        private Dictionary<int, TowerData> towers;
        private Dictionary<int, GameObject> towerObjects;
        private List<ProjectileData> projectiles;
        private List<GameObject> projectileObjects;
        
        // 防御统计
        private DefenseStats defenseStats;
        
        // 系统状态
        private bool inCombat;
        private int nextTowerId = 1;
        private int nextProjectileId = 1;
        
        // 更新间隔
        private float lastTowerUpdate;
        private float lastProjectileUpdate;
        private float lastTargetingUpdate;
        
        private const float TOWER_UPDATE_INTERVAL = 0.1f;
        private const float PROJECTILE_UPDATE_INTERVAL = 0.02f;
        private const float TARGETING_UPDATE_INTERVAL = 0.2f;

        protected override void OnInit()
        {
            // 获取系统引用
            gameModel = this.GetModel<ISurvivalGameModel>();
            zombieSystem = this.GetSystem<IZombieSystem>();
            resourceSystem = this.GetSystem<IResourceSystem>();
            techSystem = this.GetSystem<IAdvancedTechSystem>();
            
            // 初始化数据结构
            towers = new Dictionary<int, TowerData>();
            towerObjects = new Dictionary<int, GameObject>();
            projectiles = new List<ProjectileData>();
            projectileObjects = new List<GameObject>();
            
            // 初始化统计数据
            defenseStats = new DefenseStats();
            
            // 监听事件
            this.RegisterEvent<MyGameNamespace.ZombieSpawnedEvent>(OnZombieSpawned);
            this.RegisterEvent<MyGameNamespace.ZombieDeathEvent>(OnZombieDeath);
            
            // 初始化状态
            inCombat = false;
            
            // 初始化时间戳
            lastTowerUpdate = Time.time;
            lastProjectileUpdate = Time.time;
            lastTargetingUpdate = Time.time;
            
           // Debug.Log("防御系统初始化完成");
        }

        private void OnZombieSpawned(MyGameNamespace.ZombieSpawnedEvent e)
        {
            // 检测是否需要进入战斗状态
            if (!inCombat && zombieSystem.GetActiveZombieCount() > 0)
            {
                StartCombat();
            }
        }

        private void OnZombieDeath(MyGameNamespace.ZombieDeathEvent e)
        {
            // 更新击杀统计
            defenseStats.totalZombiesKilled++;
            
            // 检查是否结束战斗
            if (inCombat && zombieSystem.GetActiveZombieCount() == 0)
            {
                EndCombat();
            }
        }

        public void Update()
        {
            // 防御系统主更新方法
            float currentTime = Time.time;
            
            // 更新防御塔
            if (currentTime - lastTowerUpdate >= TOWER_UPDATE_INTERVAL)
            {
                UpdateTowers();
                lastTowerUpdate = currentTime;
            }
            
            // 更新投射物
            if (currentTime - lastProjectileUpdate >= PROJECTILE_UPDATE_INTERVAL)
            {
                UpdateProjectiles();
                lastProjectileUpdate = currentTime;
            }
            
            // 更新目标系统
            if (currentTime - lastTargetingUpdate >= TARGETING_UPDATE_INTERVAL)
            {
                UpdateTargeting();
                lastTargetingUpdate = currentTime;
            }
        }

        private void UpdateTowers()
        {
            foreach (var tower in towers.Values)
            {
                if (!tower.isActive || tower.currentHealth <= 0)
                    continue;

                // 更新冷却时间
                if (tower.lastFireTime > 0)
                {
                    tower.timeSinceLastFire = Time.time - tower.lastFireTime;
                }

                // 查找目标并攻击
                if (tower.timeSinceLastFire >= tower.fireRate)
                {
                    var target = FindBestTarget(tower.position, tower.range, tower.type);
                    if (target != null)
                    {
                        FireAtTarget(tower, target);
                    }
                }
            }
        }

        private void FireAtTarget(TowerData tower, ZombieData target)
        {
            // 创建投射物
            var projectile = new ProjectileData
            {
                id = nextProjectileId++,
                towerId = tower.id,
                damage = tower.damage,
                speed = tower.projectileSpeed,
                position = tower.position,
                targetPosition = target.position,
                targetZombieId = target.id,
                creationTime = Time.time,
                type = GetProjectileType(tower.type)
            };

            projectiles.Add(projectile);
            CreateProjectileVisual(projectile);

            // 更新防御塔状态
            tower.lastFireTime = Time.time;
            tower.totalShotsFired++;

            // 播放射击音效
            // TODO: 添加音效系统调用

            Debug.Log($"防御塔 {tower.id} 向僵尸 {target.id} 开火，伤害: {tower.damage}");
        }

        private ProjectileType GetProjectileType(TowerType towerType)
        {
            switch (towerType)
            {
                case TowerType.Basic:
                    return ProjectileType.Bullet;
                case TowerType.Heavy:
                    return ProjectileType.Shell;
                case TowerType.Sniper:
                    return ProjectileType.SniperBullet;
                case TowerType.Splash:
                    return ProjectileType.Grenade;
                default:
                    return ProjectileType.Bullet;
            }
        }

        private void CreateProjectileVisual(ProjectileData projectile)
        {
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = $"Projectile_{projectile.id}";
            projectileObject.transform.position = projectile.position;
            projectileObject.transform.localScale = Vector3.one * 0.1f;
            
            // 设置颜色根据投射物类型
            var renderer = projectileObject.GetComponent<Renderer>();
            switch (projectile.type)
            {
                case ProjectileType.Bullet:
                    renderer.material.color = Color.yellow;
                    break;
                case ProjectileType.Shell:
                    renderer.material.color = Color.red;
                    break;
                case ProjectileType.SniperBullet:
                    renderer.material.color = Color.green;
                    break;
                case ProjectileType.Grenade:
                    renderer.material.color = new Color(1f, 0.5f, 0f); // 橙色
                    break;
            }

            projectileObjects.Add(projectileObject);
        }

        private void UpdateProjectiles()
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                
                // 移动投射物
                float travelTime = Time.time - projectile.creationTime;
                float distance = projectile.speed * travelTime;
                
                Vector3 direction = (projectile.targetPosition - projectile.position).normalized;
                Vector3 currentPosition = projectile.position + direction * distance;
                
                // 更新视觉对象位置
                if (i < projectileObjects.Count && projectileObjects[i] != null)
                {
                    projectileObjects[i].transform.position = currentPosition;
                }
                
                // 检查是否到达目标
                float distanceToTarget = Vector3.Distance(currentPosition, projectile.targetPosition);
                if (distanceToTarget <= 0.2f)
                {
                    // 命中目标
                    HitTarget(projectile);
                    
                    // 清理投射物
                    if (i < projectileObjects.Count && projectileObjects[i] != null)
                    {
                        GameObject.Destroy(projectileObjects[i]);
                        projectileObjects.RemoveAt(i);
                    }
                    projectiles.RemoveAt(i);
                }
                else if (travelTime > 5f) // 超时清理
                {
                    // 清理超时投射物
                    if (i < projectileObjects.Count && projectileObjects[i] != null)
                    {
                        GameObject.Destroy(projectileObjects[i]);
                        projectileObjects.RemoveAt(i);
                    }
                    projectiles.RemoveAt(i);
                }
            }
        }

        private void HitTarget(ProjectileData projectile)
        {
            // 对目标造成伤害
            DealDamageToZombie(projectile.targetZombieId, projectile.damage, projectile.targetPosition);
            
            // 更新统计
            defenseStats.totalDamageDealt += projectile.damage;
            
            // 处理溅射伤害
            if (projectile.type == ProjectileType.Grenade)
            {
                HandleSplashDamage(projectile.targetPosition, projectile.damage * 0.5f, 2f);
            }
        }

        private void HandleSplashDamage(Vector3 center, float damage, float radius)
        {
            var nearbyZombies = FindTargetsInRange(center, radius);
            foreach (var zombie in nearbyZombies)
            {
                DealDamageToZombie(zombie.id, damage, zombie.position);
            }
        }

        public void DealDamageToZombie(string zombieId, float damage, Vector3 position)
        {
            zombieSystem.TakeDamageToZombie(zombieId, damage);
            
            // 创建伤害特效
            CreateDamageEffect(position, damage);
        }

        private void CreateDamageEffect(Vector3 position, float damage)
        {
            // TODO: 添加伤害数字显示和特效
            Debug.Log($"造成伤害: {damage} 在位置: {position}");
        }

        private void UpdateTargeting()
        {
            // 更新所有防御塔的目标选择
            foreach (var tower in towers.Values)
            {
                if (!tower.isActive || tower.currentHealth <= 0)
                    continue;

                // 更新最优目标
                tower.currentTarget = FindBestTarget(tower.position, tower.range, tower.type);
            }
        }

        public ZombieData FindBestTarget(Vector3 towerPosition, float range, TowerType towerType)
        {
            var targetsInRange = FindTargetsInRange(towerPosition, range);
            if (targetsInRange.Count == 0)
                return null;

            // 根据防御塔类型选择最佳目标
            switch (towerType)
            {
                case TowerType.Basic:
                    // 选择最近的目标
                    return targetsInRange.OrderBy(z => Vector3.Distance(towerPosition, z.position)).FirstOrDefault();
                
                case TowerType.Heavy:
                    // 选择血量最高的目标
                    return targetsInRange.OrderByDescending(z => z.currentHealth).FirstOrDefault();
                
                case TowerType.Sniper:
                    // 选择最远的目标
                    return targetsInRange.OrderByDescending(z => Vector3.Distance(towerPosition, z.position)).FirstOrDefault();
                
                case TowerType.Splash:
                    // 选择周围僵尸最多的目标
                    return FindBestSplashTarget(targetsInRange, towerPosition);
                
                default:
                    return targetsInRange.FirstOrDefault();
            }
        }

        private ZombieData FindBestSplashTarget(List<ZombieData> targets, Vector3 towerPosition)
        {
            ZombieData bestTarget = null;
            int maxNearbyCount = 0;

            foreach (var target in targets)
            {
                int nearbyCount = zombieSystem.GetZombiesNearPosition(target.position, 2f).Count;
                if (nearbyCount > maxNearbyCount)
                {
                    maxNearbyCount = nearbyCount;
                    bestTarget = target;
                }
            }

            return bestTarget ?? targets.FirstOrDefault();
        }

        public List<ZombieData> FindTargetsInRange(Vector3 center, float range)
        {
            return zombieSystem.GetZombiesNearPosition(center, range)
                .Where(z => z.IsAlive)
                .ToList();
        }

        public int BuildTower(Vector3 position, TowerType type)
        {
            // 检查资源
            var config = GetTowerConfig(type);
            if (!HasSufficientResources(config.buildCost))
                return -1;

            // 创建防御塔数据
            var tower = new TowerData
            {
                id = nextTowerId++,
                type = type,
                position = position,
                level = 1,
                currentHealth = config.maxHealth,
                maxHealth = config.maxHealth,
                damage = config.damage,
                range = config.range,
                fireRate = config.fireRate,
                projectileSpeed = config.projectileSpeed,
                isActive = true,
                buildTime = Time.time,
                totalShotsFired = 0,
                totalKills = 0
            };

            towers[tower.id] = tower;
            CreateTowerVisual(tower);

            // 消耗资源
            ConsumeResources(config.buildCost);

            // 更新统计
            defenseStats.totalTowers++;
            defenseStats.operationalTowers++;

            Debug.Log($"建造防御塔 {tower.id} 类型: {type} 位置: {position}");
            return tower.id;
        }

        private TowerConfig GetTowerConfig(TowerType type)
        {
            // 返回防御塔配置
            switch (type)
            {
                case TowerType.Basic:
                    return new TowerConfig
                    {
                        maxHealth = 100f,
                        damage = 20f,
                        range = 5f,
                        fireRate = 1f,
                        projectileSpeed = 10f,
                        buildCost = new Dictionary<ResourceType, int> { { ResourceType.Materials, 50 } }
                    };
                case TowerType.Heavy:
                    return new TowerConfig
                    {
                        maxHealth = 150f,
                        damage = 50f,
                        range = 4f,
                        fireRate = 2f,
                        projectileSpeed = 8f,
                        buildCost = new Dictionary<ResourceType, int> { { ResourceType.Materials, 100 }, { ResourceType.Ammunition, 25 } }
                    };
                case TowerType.Sniper:
                    return new TowerConfig
                    {
                        maxHealth = 80f,
                        damage = 80f,
                        range = 8f,
                        fireRate = 3f,
                        projectileSpeed = 15f,
                        buildCost = new Dictionary<ResourceType, int> { { ResourceType.Materials, 75 }, { ResourceType.Ammunition, 50 } }
                    };
                case TowerType.Splash:
                    return new TowerConfig
                    {
                        maxHealth = 120f,
                        damage = 30f,
                        range = 4f,
                        fireRate = 2.5f,
                        projectileSpeed = 6f,
                        buildCost = new Dictionary<ResourceType, int> { { ResourceType.Materials, 80 }, { ResourceType.Ammunition, 40 } }
                    };
                default:
                    return new TowerConfig();
            }
        }

        private bool HasSufficientResources(Dictionary<ResourceType, int> cost)
        {
            foreach (var requirement in cost)
            {
                if (resourceSystem.GetResourceAmount(requirement.Key) < requirement.Value)
                    return false;
            }
            return true;
        }

        private void ConsumeResources(Dictionary<ResourceType, int> cost)
        {
            foreach (var requirement in cost)
            {
                resourceSystem.TryConsumeResource(requirement.Key, requirement.Value);
            }
        }

        private void CreateTowerVisual(TowerData tower)
        {
            GameObject towerObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            towerObject.name = $"Tower_{tower.id}_{tower.type}";
            towerObject.transform.position = tower.position;
            towerObject.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            
            // 根据类型设置颜色
            var renderer = towerObject.GetComponent<Renderer>();
            switch (tower.type)
            {
                case TowerType.Basic:
                    renderer.material.color = Color.gray;
                    break;
                case TowerType.Heavy:
                    renderer.material.color = Color.red;
                    break;
                case TowerType.Sniper:
                    renderer.material.color = Color.green;
                    break;
                case TowerType.Splash:
                    renderer.material.color = new Color(1f, 0.5f, 0f); // 橙色
                    break;
            }

            towerObjects[tower.id] = towerObject;
        }

        public void StartCombat()
        {
            inCombat = true;
            Debug.Log("进入战斗状态");
        }

        public void EndCombat()
        {
            inCombat = false;
            Debug.Log("战斗结束");
        }

        public bool IsInCombat() => inCombat;

        // 其他接口方法的实现
        public bool UpgradeTower(int towerId) 
        { 
            if (towers.ContainsKey(towerId))
            {
                var tower = towers[towerId];
                tower.level++;
                tower.damage *= 1.2f;
                tower.range *= 1.1f;
                tower.maxHealth *= 1.15f;
                tower.currentHealth = tower.maxHealth;
                Debug.Log($"升级防御塔 {towerId} 到等级 {tower.level}");
                return true;
            }
            return false; 
        }
        
        public bool SellTower(int towerId) 
        { 
            if (towers.ContainsKey(towerId))
            {
                towers.Remove(towerId);
                if (towerObjects.ContainsKey(towerId))
                {
                    GameObject.Destroy(towerObjects[towerId]);
                    towerObjects.Remove(towerId);
                }
                defenseStats.totalTowers--;
                defenseStats.operationalTowers--;
                Debug.Log($"出售防御塔 {towerId}");
                return true;
            }
            return false; 
        }
        
        public bool RepairTower(int towerId) 
        { 
            if (towers.ContainsKey(towerId))
            {
                var tower = towers[towerId];
                tower.currentHealth = tower.maxHealth;
                Debug.Log($"修复防御塔 {towerId}");
                return true;
            }
            return false; 
        }
        
        public List<TowerData> GetAllTowers() => towers.Values.ToList();
        public List<TowerData> GetTowersInRange(Vector3 center, float range) 
        { 
            return towers.Values.Where(t => Vector3.Distance(t.position, center) <= range).ToList(); 
        }
        public List<ProjectileData> GetAllProjectiles() => projectiles;
        public TowerData GetTower(int towerId) => towers.ContainsKey(towerId) ? towers[towerId] : null;
        public DefenseStats GetDefenseStats() => defenseStats;
    }

    // 防御统计数据
    [System.Serializable]
    public class DefenseStats
    {
        public int totalTowers;
        public int operationalTowers;
        public int totalZombiesKilled;
        public float totalDamageDealt;
        public int activeProjectiles;
    }

    // 防御塔配置
    [System.Serializable]
    public class TowerConfig
    {
        public float maxHealth;
        public float damage;
        public float range;
        public float fireRate;
        public float projectileSpeed;
        public Dictionary<ResourceType, int> buildCost;
    }

    // 枚举定义
}