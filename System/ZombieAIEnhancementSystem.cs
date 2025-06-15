using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using DG.Tweening;
using MyGameNamespace;
using SurvivalGame.Model;
using SurvivalGame.GameSystem;
using SurvivalGame.Visualization;

namespace SurvivalGame.AI
{
    /// <summary>
    /// 僵尸AI增强系统接口
    /// </summary>
    public interface IZombieAIEnhancementSystem : QFISystem
    {
        void UpdateZombieAI();
        void ProcessZombieGroupBehavior();
        void HandleZombieSpecialAbilities();
        void TriggerZombieAnimations();
    }

    /// <summary>
    /// 僵尸AI增强系统
    /// 处理更智能的僵尸行为、群体行为和特殊能力
    /// </summary>
    public class ZombieAIEnhancementSystem : AbstractSystem, IZombieAIEnhancementSystem
    {
        private IZombieSystem zombieSystem;
        private IZombieVisualizationSystem visualizationSystem;
        private IEnhancedBuildingSystem buildingSystem;
        
        // AI参数
        private const float AI_UPDATE_INTERVAL = 0.3f;
        private const float GROUP_BEHAVIOR_INTERVAL = 1f;
        private const float SPECIAL_ABILITY_INTERVAL = 2f;
        
        // 群体行为参数
        private const float GROUP_FORMATION_RADIUS = 3f;
        private const float LEADER_INFLUENCE_RADIUS = 5f;
        private const int MIN_GROUP_SIZE = 3;
        
        // 特殊能力冷却时间
        private Dictionary<string, float> spitterCooldowns;
        private Dictionary<string, float> screamerCooldowns;
        private Dictionary<string, float> tankChargeCooldowns;
        
        // 时间戳
        private float lastAIUpdate;
        private float lastGroupBehaviorUpdate;
        private float lastSpecialAbilityUpdate;
        
        protected override void OnInit()
        {
            // 获取系统引用
            zombieSystem = this.GetSystem<IZombieSystem>();
            visualizationSystem = this.GetSystem<IZombieVisualizationSystem>();
            buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            
            // 初始化数据结构
            spitterCooldowns = new Dictionary<string, float>();
            screamerCooldowns = new Dictionary<string, float>();
            tankChargeCooldowns = new Dictionary<string, float>();
            
            // 注册事件
            this.RegisterEvent<ZombieSpawnedEvent>(OnZombieSpawned);
            this.RegisterEvent<ZombieDeathEvent>(OnZombieDied);
            this.RegisterEvent<TimeUpdateEvent>(OnTimeUpdate);
            
            // 初始化时间戳
            lastAIUpdate = Time.time;
            lastGroupBehaviorUpdate = Time.time;
            lastSpecialAbilityUpdate = Time.time;
            
//            UnityEngine.Debug.Log("僵尸AI增强系统初始化完成");
        }
        
        private void OnZombieSpawned(ZombieSpawnedEvent e)
        {
            // 为特殊僵尸初始化冷却时间
            var zombieData = zombieSystem.GetZombieData().zombies[e.ZombieId];
            
            switch (zombieData.type)
            {
                case ZombieType.Spitter:
                    spitterCooldowns[e.ZombieId] = 0f;
                    break;
                case ZombieType.Screamer:
                    screamerCooldowns[e.ZombieId] = 0f;
                    break;
                case ZombieType.Tank:
                    tankChargeCooldowns[e.ZombieId] = 0f;
                    break;
            }
            
            // 触发生成动画
            TriggerSpawnAnimation(e.ZombieId);
        }
        
        private void OnZombieDied(ZombieDeathEvent e)
        {
            // 清理冷却时间记录
            if (spitterCooldowns.ContainsKey(e.ZombieId))
                spitterCooldowns.Remove(e.ZombieId);
            if (screamerCooldowns.ContainsKey(e.ZombieId))
                screamerCooldowns.Remove(e.ZombieId);
            if (tankChargeCooldowns.ContainsKey(e.ZombieId))
                tankChargeCooldowns.Remove(e.ZombieId);
        }
        
        private void OnTimeUpdate(TimeUpdateEvent e)
        {
            float currentTime = Time.time;
            
            // 更新AI行为
            if (currentTime - lastAIUpdate >= AI_UPDATE_INTERVAL)
            {
                UpdateZombieAI();
                lastAIUpdate = currentTime;
            }
            
            // 更新群体行为
            if (currentTime - lastGroupBehaviorUpdate >= GROUP_BEHAVIOR_INTERVAL)
            {
                ProcessZombieGroupBehavior();
                lastGroupBehaviorUpdate = currentTime;
            }
            
            // 更新特殊能力
            if (currentTime - lastSpecialAbilityUpdate >= SPECIAL_ABILITY_INTERVAL)
            {
                HandleZombieSpecialAbilities();
                lastSpecialAbilityUpdate = currentTime;
            }
        }
        
        public void UpdateZombieAI()
        {
            var zombieData = zombieSystem.GetZombieData();
            
            foreach (var zombie in zombieData.zombies.Values)
            {
                if (!zombie.IsAlive) continue;
                
                // 更新个体AI行为
                UpdateIndividualZombieAI(zombie);
                
                // 触发相应的动画
                TriggerZombieStateAnimation(zombie);
            }
        }
        
        /// <summary>
        /// 更新单个僵尸的AI行为
        /// </summary>
        private void UpdateIndividualZombieAI(ZombieData zombie)
        {
            switch (zombie.type)
            {
                case ZombieType.Walker:
                    UpdateWalkerAI(zombie);
                    break;
                case ZombieType.Runner:
                    UpdateRunnerAI(zombie);
                    break;
                case ZombieType.Tank:
                    UpdateTankAI(zombie);
                    break;
                case ZombieType.Spitter:
                    UpdateSpitterAI(zombie);
                    break;
                case ZombieType.Screamer:
                    UpdateScreamerAI(zombie);
                    break;
            }
        }
        
        /// <summary>
        /// 更新Walker僵尸AI - 基础僵尸，寻找最近的建筑
        /// </summary>
        private void UpdateWalkerAI(ZombieData zombie)
        {
            if (zombie.state == ZombieState.Wandering)
            {
                // 扩大检测范围
                var nearestBuilding = FindNearestBuilding(zombie.position, zombie.detectionRange * 1.5f);
                if (nearestBuilding != null)
                {
                    zombie.targetBuildingId = nearestBuilding.Id.ToString();
                    zombie.targetPosition = nearestBuilding.Position;
                    zombie.state = ZombieState.Approaching;
                    
                    // 触发发现建筑的动画
                    TriggerDiscoveryAnimation(zombie.id);
                }
            }
        }
        
        /// <summary>
        /// 更新Runner僵尸AI - 快速移动，优先攻击防御建筑
        /// </summary>
        private void UpdateRunnerAI(ZombieData zombie)
        {
            if (zombie.state == ZombieState.Wandering)
            {
                // 优先寻找防御建筑
                var defenseBuilding = FindNearestDefenseBuilding(zombie.position, zombie.detectionRange * 2f);
                if (defenseBuilding != null)
                {
                    zombie.targetBuildingId = defenseBuilding.Id.ToString();
                    zombie.targetPosition = defenseBuilding.Position;
                    zombie.state = ZombieState.Approaching;
                    
                    // 加速效果
                    zombie.moveSpeed *= 1.3f;
                    TriggerSpeedBoostAnimation(zombie.id);
                }
                else
                {
                    // 寻找普通建筑
                    UpdateWalkerAI(zombie);
                }
            }
        }
        
        /// <summary>
        /// 更新Tank僵尸AI - 冲锋攻击，造成范围伤害
        /// </summary>
        private void UpdateTankAI(ZombieData zombie)
        {
            if (zombie.state == ZombieState.Approaching)
            {
                float distanceToTarget = Vector2.Distance(zombie.position, zombie.targetPosition);
                
                // 在冲锋范围内开始冲锋
                if (distanceToTarget <= 5f && CanTankCharge(zombie.id))
                {
                    StartTankCharge(zombie);
                }
            }
        }
        
        /// <summary>
        /// 更新Spitter僵尸AI - 远程攻击，保持距离
        /// </summary>
        private void UpdateSpitterAI(ZombieData zombie)
        {
            if (zombie.state == ZombieState.Approaching)
            {
                float distanceToTarget = Vector2.Distance(zombie.position, zombie.targetPosition);
                
                // 在远程攻击范围内停止接近，开始远程攻击
                if (distanceToTarget <= zombie.attackRange && CanSpitterAttack(zombie.id))
                {
                    zombie.state = ZombieState.Attacking;
                    PerformSpitterAttack(zombie);
                }
            }
        }
        
        /// <summary>
        /// 更新Screamer僵尸AI - 召唤其他僵尸
        /// </summary>
        private void UpdateScreamerAI(ZombieData zombie)
        {
            if (zombie.state != ZombieState.Dead && CanScreamerScream(zombie.id))
            {
                // 检查周围是否有建筑
                var nearbyBuildings = FindBuildingsInRadius(zombie.position, 8f);
                if (nearbyBuildings.Count > 0)
                {
                    PerformScreamerAbility(zombie);
                }
            }
        }
        
        public void ProcessZombieGroupBehavior()
        {
            var zombieData = zombieSystem.GetZombieData();
            var zombieList = new List<ZombieData>(zombieData.zombies.Values);
            
            // 形成僵尸群
            FormZombieGroups(zombieList);
            
            // 处理群体移动
            ProcessGroupMovement(zombieList);
        }
        
        /// <summary>
        /// 形成僵尸群
        /// </summary>
        private void FormZombieGroups(List<ZombieData> zombies)
        {
            foreach (var zombie in zombies)
            {
                if (!zombie.IsAlive || zombie.state == ZombieState.Attacking) continue;
                
                // 寻找附近的僵尸
                var nearbyZombies = FindNearbyZombies(zombie.position, GROUP_FORMATION_RADIUS, zombies);
                
                if (nearbyZombies.Count >= MIN_GROUP_SIZE)
                {
                    // 指定群体领袖（血量最高的）
                    var leader = GetGroupLeader(nearbyZombies);
                    if (leader != null)
                    {
                        // 其他僵尸跟随领袖
                        foreach (var follower in nearbyZombies)
                        {
                            if (follower.id != leader.id && follower.state == ZombieState.Wandering)
                            {
                                follower.targetPosition = leader.targetPosition;
                                follower.state = leader.state;
                            }
                        }
                        
                        // 触发群体形成动画
                        TriggerGroupFormationAnimation(nearbyZombies);
                    }
                }
            }
        }
        
        /// <summary>
        /// 处理群体移动
        /// </summary>
        private void ProcessGroupMovement(List<ZombieData> zombies)
        {
            // 群体移动逻辑可以在这里实现
            // 例如：保持阵型、避免碰撞等
        }
        
        public void HandleZombieSpecialAbilities()
        {
            var zombieData = zombieSystem.GetZombieData();
            
            foreach (var zombie in zombieData.zombies.Values)
            {
                if (!zombie.IsAlive) continue;
                
                switch (zombie.type)
                {
                    case ZombieType.Tank:
                        HandleTankSpecialAbility(zombie);
                        break;
                    case ZombieType.Spitter:
                        HandleSpitterSpecialAbility(zombie);
                        break;
                    case ZombieType.Screamer:
                        HandleScreamerSpecialAbility(zombie);
                        break;
                }
            }
        }
        
        public void TriggerZombieAnimations()
        {
            // 这个方法由其他方法调用来触发特定动画
        }
        
        /// <summary>
        /// 触发生成动画
        /// </summary>
        private void TriggerSpawnAnimation(string zombieId)
        {
            // 通过可视化系统触发特殊生成效果
            UnityEngine.Debug.Log($"触发僵尸 {zombieId} 生成动画");
        }
        
        /// <summary>
        /// 触发发现建筑动画
        /// </summary>
        private void TriggerDiscoveryAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId);
            if (zombieVisual != null)
            {
                // 感叹号效果
                CreateExclamationEffect(zombieVisual.transform.position);
                
                // 僵尸闪光效果
                var renderer = zombieVisual.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    var originalColor = renderer.color;
                    renderer.DOColor(Color.yellow, 0.2f)
                        .OnComplete(() => renderer.DOColor(originalColor, 0.2f));
                }
            }
        }
        
        /// <summary>
        /// 触发速度提升动画
        /// </summary>
        private void TriggerSpeedBoostAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId);
            if (zombieVisual != null)
            {
                // 创建速度线条效果
                CreateSpeedLinesEffect(zombieVisual.transform.position);
                
                // 僵尸发红光
                var renderer = zombieVisual.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.DOColor(Color.red, 0.1f)
                        .SetLoops(6, LoopType.Yoyo);
                }
            }
        }
        
        /// <summary>
        /// 触发群体形成动画
        /// </summary>
        private void TriggerGroupFormationAnimation(List<ZombieData> zombies)
        {
            foreach (var zombie in zombies)
            {
                var zombieVisual = visualizationSystem.GetZombieVisual(zombie.id);
                if (zombieVisual != null)
                {
                    // 所有僵尸同时发光
                    var renderer = zombieVisual.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.DOColor(Color.cyan, 0.3f)
                            .SetLoops(2, LoopType.Yoyo);
                    }
                }
            }
        }
        
        /// <summary>
        /// 触发僵尸状态动画
        /// </summary>
        private void TriggerZombieStateAnimation(ZombieData zombie)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombie.id);
            if (zombieVisual == null) return;
            
            switch (zombie.state)
            {
                case ZombieState.Approaching:
                    TriggerApproachingAnimation(zombieVisual);
                    break;
                case ZombieState.Attacking:
                    // 攻击动画由可视化系统处理
                    break;
            }
        }
        
        /// <summary>
        /// 触发接近动画
        /// </summary>
        private void TriggerApproachingAnimation(GameObject zombieVisual)
        {
            // 轻微的前倾效果
            zombieVisual.transform.DORotate(new Vector3(0, 0, -5f), 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        // Tank 相关方法
        private bool CanTankCharge(string zombieId)
        {
            if (!tankChargeCooldowns.ContainsKey(zombieId)) return true;
            return Time.time - tankChargeCooldowns[zombieId] >= 10f; // 10秒冷却
        }
        
        private void StartTankCharge(ZombieData zombie)
        {
            tankChargeCooldowns[zombie.id] = Time.time;
            
            // 增加移动速度
            zombie.moveSpeed *= 2f;
            
            // 触发冲锋动画
            TriggerChargeAnimation(zombie.id);
            
            UnityEngine.Debug.Log($"Tank僵尸 {zombie.id} 开始冲锋！");
        }
        
        private void HandleTankSpecialAbility(ZombieData zombie)
        {
            // Tank的特殊能力处理
        }
        
        // Spitter 相关方法
        private bool CanSpitterAttack(string zombieId)
        {
            if (!spitterCooldowns.ContainsKey(zombieId)) return true;
            return Time.time - spitterCooldowns[zombieId] >= 3f; // 3秒冷却
        }
        
        private void PerformSpitterAttack(ZombieData zombie)
        {
            spitterCooldowns[zombie.id] = Time.time;
            
            // 触发吐酸动画
            TriggerSpitAnimation(zombie.id);
            
            UnityEngine.Debug.Log($"Spitter僵尸 {zombie.id} 吐酸攻击！");
        }
        
        private void HandleSpitterSpecialAbility(ZombieData zombie)
        {
            // Spitter的特殊能力处理
        }
        
        // Screamer 相关方法
        private bool CanScreamerScream(string zombieId)
        {
            if (!screamerCooldowns.ContainsKey(zombieId)) return true;
            return Time.time - screamerCooldowns[zombieId] >= 15f; // 15秒冷却
        }
        
        private void PerformScreamerAbility(ZombieData zombie)
        {
            screamerCooldowns[zombie.id] = Time.time;
            
            // 召唤附近的僵尸
            var nearbyZombies = zombieSystem.GetZombiesNearPosition(zombie.position, 10f);
            foreach (var nearbyZombie in nearbyZombies)
            {
                if (nearbyZombie.id != zombie.id && nearbyZombie.state == ZombieState.Wandering)
                {
                    nearbyZombie.state = ZombieState.Approaching;
                    nearbyZombie.targetPosition = zombie.targetPosition;
                }
            }
            
            // 触发尖叫动画
            TriggerScreamAnimation(zombie.id);
            
            UnityEngine.Debug.Log($"Screamer僵尸 {zombie.id} 发出尖叫，召唤其他僵尸！");
        }
        
        private void HandleScreamerSpecialAbility(ZombieData zombie)
        {
            // Screamer的特殊能力处理
        }
        
        // 动画触发方法
        private void TriggerChargeAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId);
            if (zombieVisual != null)
            {
                // 冲锋特效
                zombieVisual.transform.DOShakePosition(1f, 0.2f, 10, 90, false, true);
                
                var renderer = zombieVisual.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.DOColor(Color.red, 0.1f)
                        .SetLoops(10, LoopType.Yoyo);
                }
            }
        }
        
        private void TriggerSpitAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId);
            if (zombieVisual != null)
            {
                // 向前突进动画
                var originalPos = zombieVisual.transform.position;
                zombieVisual.transform.DOMove(originalPos + Vector3.right * 0.3f, 0.2f)
                    .OnComplete(() => zombieVisual.transform.DOMove(originalPos, 0.2f));
                    
                // 创建酸液粒子效果
                CreateAcidEffect(zombieVisual.transform.position);
            }
        }
        
        private void TriggerScreamAnimation(string zombieId)
        {
            var zombieVisual = visualizationSystem.GetZombieVisual(zombieId);
            if (zombieVisual != null)
            {
                // 放大和震动效果
                var originalScale = zombieVisual.transform.localScale;
                zombieVisual.transform.DOScale(originalScale * 1.5f, 0.3f)
                    .OnComplete(() => zombieVisual.transform.DOScale(originalScale, 0.3f));
                    
                zombieVisual.transform.DOShakePosition(1f, 0.1f, 20, 90, false, true);
                
                // 创建音波效果
                CreateSoundWaveEffect(zombieVisual.transform.position);
            }
        }
        
        // 特效创建方法
        private void CreateExclamationEffect(Vector3 position)
        {
            GameObject effect = new GameObject("ExclamationEffect");
            effect.transform.position = position + Vector3.up * 1f;
            
            var textMesh = effect.AddComponent<TextMesh>();
            textMesh.text = "!";
            textMesh.fontSize = 30;
            textMesh.color = Color.yellow;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.fontStyle = FontStyle.Bold;
            
            // 动画效果
            effect.transform.localScale = Vector3.zero;
            effect.transform.DOScale(Vector3.one, 0.2f)
                .SetEase(Ease.OutBack);
                
            DOVirtual.DelayedCall(1f, () => {
                if (effect != null)
                {
                    effect.transform.DOScale(Vector3.zero, 0.2f)
                        .OnComplete(() => GameObject.Destroy(effect));
                }
            });
        }
        
        private void CreateSpeedLinesEffect(Vector3 position)
        {
            // 简化的速度线条效果
            for (int i = 0; i < 3; i++)
            {
                GameObject line = new GameObject("SpeedLine");
                line.transform.position = position + Vector3.back * (i * 0.3f);
                
                var renderer = line.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateLineSprite();
                renderer.color = new Color(1, 1, 0, 0.7f);
                renderer.sortingOrder = 3;
                
                // 向后移动并淡出
                line.transform.DOMove(position + Vector3.left * 2f, 0.5f);
                renderer.DOFade(0f, 0.5f)
                    .OnComplete(() => GameObject.Destroy(line));
            }
        }
        
        private void CreateAcidEffect(Vector3 position)
        {
            GameObject acid = new GameObject("AcidEffect");
            acid.transform.position = position + Vector3.right * 0.5f;
            
            var renderer = acid.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateAcidSprite();
            renderer.color = Color.green;
            renderer.sortingOrder = 4;
            
            // 飞行和淡出动画
            acid.transform.DOMove(position + Vector3.right * 3f, 1f);
            renderer.DOFade(0f, 1f)
                .OnComplete(() => GameObject.Destroy(acid));
        }
        
        private void CreateSoundWaveEffect(Vector3 position)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject wave = new GameObject("SoundWave");
                wave.transform.position = position;
                
                var renderer = wave.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateCircleSprite();
                renderer.color = new Color(1, 0, 1, 0.5f);
                renderer.sortingOrder = 3;
                
                // 扩散动画
                wave.transform.localScale = Vector3.zero;
                float delay = i * 0.1f;
                
                DOVirtual.DelayedCall(delay, () => {
                    if (wave != null)
                    {
                        wave.transform.DOScale(Vector3.one * 3f, 1f);
                        renderer.DOFade(0f, 1f)
                            .OnComplete(() => GameObject.Destroy(wave));
                    }
                });
            }
        }
        
        // 辅助方法
        private BuildingData FindNearestBuilding(Vector2 position, float radius)
        {
            // 简化实现，实际应该从建筑系统获取
            return null;
        }
        
        private BuildingData FindNearestDefenseBuilding(Vector2 position, float radius)
        {
            // 寻找防御类建筑
            return null;
        }
        
        private List<BuildingData> FindBuildingsInRadius(Vector2 position, float radius)
        {
            // 寻找范围内的所有建筑
            return new List<BuildingData>();
        }
        
        private List<ZombieData> FindNearbyZombies(Vector2 position, float radius, List<ZombieData> allZombies)
        {
            var nearbyZombies = new List<ZombieData>();
            
            foreach (var zombie in allZombies)
            {
                if (!zombie.IsAlive) continue;
                
                float distance = Vector2.Distance(position, zombie.position);
                if (distance <= radius)
                {
                    nearbyZombies.Add(zombie);
                }
            }
            
            return nearbyZombies;
        }
        
        private ZombieData GetGroupLeader(List<ZombieData> zombies)
        {
            ZombieData leader = null;
            float maxHealth = 0f;
            
            foreach (var zombie in zombies)
            {
                if (zombie.currentHealth > maxHealth)
                {
                    maxHealth = zombie.currentHealth;
                    leader = zombie;
                }
            }
            
            return leader;
        }
        
        // 图片创建方法
        private Sprite CreateLineSprite()
        {
            int width = 20, height = 2;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
        }
        
        private Sprite CreateAcidSprite()
        {
            int size = 8;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.4f;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, Color.green);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
        
        private Sprite CreateCircleSprite()
        {
            int size = 16;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.4f;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    // 创建圆环效果
                    if (distance <= radius && distance >= radius * 0.7f)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
    }
} 