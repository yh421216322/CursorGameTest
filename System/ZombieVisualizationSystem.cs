using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using DG.Tweening;
using MyGameNamespace;
using SurvivalGame.Model;
using SurvivalGame.GameSystem;

namespace SurvivalGame.Visualization
{
    /// <summary>
    /// 僵尸可视化系统接口
    /// </summary>
    public interface IZombieVisualizationSystem : QFISystem
    {
        void CreateZombieVisual(ZombieData zombieData);
        void UpdateZombieVisual(string zombieId, ZombieData zombieData);
        void RemoveZombieVisual(string zombieId);
        void UpdateAllZombieVisuals();
        GameObject GetZombieVisual(string zombieId);
        void ShowDamageEffect(string zombieId, float damage);
        void ShowDeathEffect(string zombieId);
    }

    /// <summary>
    /// 增强的僵尸可视化系统
    /// 使用真实图片资源和DOTween动画
    /// </summary>
    public class ZombieVisualizationSystem : AbstractSystem, IZombieVisualizationSystem
    {
        private IZombieSystem zombieSystem;
        
        // 视觉对象管理
        private Dictionary<string, GameObject> zombieVisuals;
        private Dictionary<string, SpriteRenderer> zombieRenderers;
        private Dictionary<string, Tween> movementTweens;
        private Dictionary<string, Sequence> animationSequences;
        
        // 僵尸图片资源
        private Dictionary<ZombieType, Sprite> zombieSprites;
        private Dictionary<ZombieType, ZombieVisualConfig> visualConfigs;
        
        // 动画预设
        private float walkBobAmplitude = 0.1f;
        private float walkBobSpeed = 2f;
        private float idleFloatAmplitude = 0.05f;
        private float idleFloatSpeed = 1f;
        
        // 更新参数
        private float lastVisualUpdate;
        private const float VISUAL_UPDATE_INTERVAL = 0.1f;
        
        protected override void OnInit()
        {
            // 获取系统引用
            zombieSystem = this.GetSystem<IZombieSystem>();
            
            // 初始化容器
            zombieVisuals = new Dictionary<string, GameObject>();
            zombieRenderers = new Dictionary<string, SpriteRenderer>();
            movementTweens = new Dictionary<string, Tween>();
            animationSequences = new Dictionary<string, Sequence>();
            zombieSprites = new Dictionary<ZombieType, Sprite>();
            visualConfigs = new Dictionary<ZombieType, ZombieVisualConfig>();
            
            // 加载僵尸图片资源
            LoadZombieSprites();
            
            // 初始化视觉配置
            InitializeVisualConfigs();
            
            // 注册事件
            this.RegisterEvent<ZombieSpawnedEvent>(OnZombieSpawned);
            this.RegisterEvent<ZombieDeathEvent>(OnZombieDied);
            this.RegisterEvent<ZombieAttackBuildingEvent>(OnZombieAttack);
            
            lastVisualUpdate = Time.time;
            
          //  UnityEngine.Debug.Log("增强僵尸可视化系统初始化完成");
        }
        
        /// <summary>
        /// 加载僵尸图片资源
        /// </summary>
        private void LoadZombieSprites()
        {
            try
            {
               // UnityEngine.Debug.Log("开始加载僵尸图片资源...");
                
                // 加载不同僵尸类型的图片
                var zb1 = Resources.Load<Sprite>("Image/zb1");
                var zb2 = Resources.Load<Sprite>("Image/zb2");
                var zb3 = Resources.Load<Sprite>("Image/zb3");
                var zb4 = Resources.Load<Sprite>("Image/zb4");
                
               // UnityEngine.Debug.Log($"图片加载结果: zb1={zb1?.name}, zb2={zb2?.name}, zb3={zb3?.name}, zb4={zb4?.name}");
                
                // 分配给不同僵尸类型
                zombieSprites[ZombieType.Walker] = zb1 ?? CreateDefaultZombieSprite(ZombieType.Walker);
                zombieSprites[ZombieType.Runner] = zb2 ?? CreateDefaultZombieSprite(ZombieType.Runner);
                zombieSprites[ZombieType.Tank] = zb3 ?? CreateDefaultZombieSprite(ZombieType.Tank);
                zombieSprites[ZombieType.Spitter] = zb4 ?? CreateDefaultZombieSprite(ZombieType.Spitter);
                zombieSprites[ZombieType.Screamer] = zb1 ?? CreateDefaultZombieSprite(ZombieType.Screamer); // 使用zb1作为备用
                
              //  UnityEngine.Debug.Log($"僵尸图片资源加载完成: 共{zombieSprites.Count}个类型");
                
                // 显示最终分配结果
                foreach (var kvp in zombieSprites)
                {
                 //   UnityEngine.Debug.Log($"{kvp.Key} -> {kvp.Value?.name} (size: {kvp.Value?.bounds.size})");
                }
            }
            catch (System.Exception e)
            {
              //  UnityEngine.Debug.LogError($"加载僵尸图片资源失败: {e.Message}");
               // UnityEngine.Debug.Log("使用默认程序化图片");
                // 创建默认图片
                foreach (ZombieType type in System.Enum.GetValues(typeof(ZombieType)))
                {
                    zombieSprites[type] = CreateDefaultZombieSprite(type);
                  //  UnityEngine.Debug.Log($"为 {type} 创建了默认图片");
                }
            }
        }
        
        /// <summary>
        /// 创建默认僵尸图片（备用方案）
        /// </summary>
        private Sprite CreateDefaultZombieSprite(ZombieType type)
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            var config = visualConfigs.ContainsKey(type) ? visualConfigs[type] : new ZombieVisualConfig();
            Color baseColor = GetZombieTypeColor(type);
            
            // 创建僵尸头像风格的图片
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
                        Color pixelColor = baseColor;
                        
                        // 添加噪点效果
                        float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.2f;
                        pixelColor += new Color(noise, noise, noise, 0);
                        
                        // 边缘阴影
                        if (distance > radius * 0.8f)
                        {
                            pixelColor *= 0.6f;
                        }
                        
                        // 添加特征
                        AddZombieFeatures(ref pixelColor, type, x, y, size, center);
                        
                        pixelColor.a = 1f;
                        texture.SetPixel(x, y, pixelColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
        
        /// <summary>
        /// 获取僵尸类型对应的颜色
        /// </summary>
        private Color GetZombieTypeColor(ZombieType type)
        {
            switch (type)
            {
                case ZombieType.Walker: return new Color(0.5f, 0.6f, 0.4f); // 绿色系
                case ZombieType.Runner: return new Color(0.7f, 0.4f, 0.4f); // 红色系
                case ZombieType.Tank: return new Color(0.4f, 0.5f, 0.3f); // 深绿色
                case ZombieType.Spitter: return new Color(0.6f, 0.6f, 0.3f); // 黄绿色
                case ZombieType.Screamer: return new Color(0.6f, 0.3f, 0.6f); // 紫色系
                default: return new Color(0.5f, 0.5f, 0.5f);
            }
        }
        
        /// <summary>
        /// 添加僵尸类型特征
        /// </summary>
        private void AddZombieFeatures(ref Color pixelColor, ZombieType type, int x, int y, int size, Vector2 center)
        {
            float distanceFromCenter = Vector2.Distance(new Vector2(x, y), center);
            float radius = size * 0.4f;
            
            switch (type)
            {
                case ZombieType.Runner:
                    // 血红色条纹
                    if ((x + y) % 8 < 2)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.red, 0.4f);
                    }
                    break;
                    
                case ZombieType.Tank:
                    // 更深的颜色和装甲感
                    pixelColor *= 0.8f;
                    if (x % 4 == 0 || y % 4 == 0)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.gray, 0.3f);
                    }
                    break;
                    
                case ZombieType.Spitter:
                    // 黄色酸液效果
                    if (y > size * 0.6f && distanceFromCenter < radius * 0.3f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.yellow, 0.6f);
                    }
                    break;
                    
                case ZombieType.Screamer:
                    // 紫色能量环
                    if (Mathf.Abs(distanceFromCenter - radius * 0.7f) < 2f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.magenta, 0.5f);
                    }
                    break;
            }
        }
        
        private void InitializeVisualConfigs()
        {
            // Walker 僵尸配置
            visualConfigs[ZombieType.Walker] = new ZombieVisualConfig
            {
                baseColor = GetZombieTypeColor(ZombieType.Walker),
                size = Vector3.one * 0.8f,
                moveSpeed = 1f,
                bobIntensity = 0.8f,
                rotationSpeed = 30f
            };
            
            // Runner 僵尸配置
            visualConfigs[ZombieType.Runner] = new ZombieVisualConfig
            {
                baseColor = GetZombieTypeColor(ZombieType.Runner),
                size = Vector3.one * 0.7f,
                moveSpeed = 2f,
                bobIntensity = 1.2f,
                rotationSpeed = 60f
            };
            
            // Tank 僵尸配置
            visualConfigs[ZombieType.Tank] = new ZombieVisualConfig
            {
                baseColor = GetZombieTypeColor(ZombieType.Tank),
                size = Vector3.one * 1.2f,
                moveSpeed = 0.5f,
                bobIntensity = 0.4f,
                rotationSpeed = 15f
            };
            
            // Spitter 僵尸配置
            visualConfigs[ZombieType.Spitter] = new ZombieVisualConfig
            {
                baseColor = GetZombieTypeColor(ZombieType.Spitter),
                size = Vector3.one * 0.9f,
                moveSpeed = 1.2f,
                bobIntensity = 0.6f,
                rotationSpeed = 40f
            };
            
            // Screamer 僵尸配置
            visualConfigs[ZombieType.Screamer] = new ZombieVisualConfig
            {
                baseColor = GetZombieTypeColor(ZombieType.Screamer),
                size = Vector3.one * 1.1f,
                moveSpeed = 1.5f,
                bobIntensity = 1.0f,
                rotationSpeed = 50f
            };
        }
        
        private void OnZombieSpawned(ZombieSpawnedEvent e)
        {
            UnityEngine.Debug.Log($"🔥 收到僵尸生成事件: {e.ZombieId}");
            
            try
            {
                var zombieData = zombieSystem.GetZombieData().zombies[e.ZombieId];
                UnityEngine.Debug.Log($"📊 获取僵尸数据成功: {zombieData.type} 位置:{zombieData.position}");
                CreateZombieVisual(zombieData);
                UnityEngine.Debug.Log($"✅ 僵尸可视化对象创建完成: {e.ZombieId}");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ 创建僵尸可视化失败: {ex.Message}");
                UnityEngine.Debug.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
        
        private void OnZombieDied(ZombieDeathEvent e)
        {
            ShowDeathEffect(e.ZombieId);
            // 延迟移除视觉对象，给死亡动画时间
            DOVirtual.DelayedCall(2f, () => RemoveZombieVisual(e.ZombieId));
        }
        
        private void OnZombieAttack(ZombieAttackBuildingEvent e)
        {
            PlayAttackAnimation(e.ZombieId);
        }
        
        public void CreateZombieVisual(ZombieData zombieData)
        {
            if (zombieVisuals.ContainsKey(zombieData.id))
            {
                UnityEngine.Debug.Log($"僵尸视觉对象已存在: {zombieData.id}");
                return; // 已存在视觉对象
            }
            
            UnityEngine.Debug.Log($"开始创建僵尸视觉对象: {zombieData.type} ID: {zombieData.id} 位置: {zombieData.position}");
            
            // 创建僵尸视觉对象
            GameObject zombieGO = CreateZombieGameObject(zombieData);
            zombieVisuals[zombieData.id] = zombieGO;
            
            // 获取渲染器引用
            var renderer = zombieGO.GetComponent<SpriteRenderer>();
            zombieRenderers[zombieData.id] = renderer;
            
            // 设置初始位置
            Vector3 position = new Vector3(zombieData.position.x, zombieData.position.y, 0);
            zombieGO.transform.position = position;
            
            UnityEngine.Debug.Log($"僵尸GameObject创建完成: {zombieGO.name}, 位置: {position}, sprite: {renderer.sprite?.name}");
            
            // 播放生成动画
            PlaySpawnAnimation(zombieData.id);
            
            // 开始待机动画
            StartIdleAnimation(zombieData.id);
            
            UnityEngine.Debug.Log($"创建增强僵尸视觉对象完成: {zombieData.type} ID: {zombieData.id}");
        }
        
        private GameObject CreateZombieGameObject(ZombieData zombieData)
        {
            GameObject zombieGO = new GameObject($"Zombie_{zombieData.type}_{zombieData.id}");
            
            // 添加SpriteRenderer
            var spriteRenderer = zombieGO.AddComponent<SpriteRenderer>();
            
            // 使用加载的图片资源
            UnityEngine.Debug.Log($"尝试为僵尸 {zombieData.type} 设置图片，字典中有图片: {zombieSprites.ContainsKey(zombieData.type)}");
            
            if (zombieSprites.ContainsKey(zombieData.type) && zombieSprites[zombieData.type] != null)
            {
                spriteRenderer.sprite = zombieSprites[zombieData.type];
                UnityEngine.Debug.Log($"✅ 成功为僵尸 {zombieData.type} 设置图片: {spriteRenderer.sprite.name}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"⚠️ 僵尸 {zombieData.type} 的图片不存在，使用默认图片");
                spriteRenderer.sprite = CreateDefaultZombieSprite(zombieData.type);
            }
            
            spriteRenderer.sortingOrder = 15; // 进一步提高显示优先级
            // spriteRenderer.sortingLayerName = "Characters"; // 暂时注释掉，使用默认层
            
            // 应用视觉配置
            var config = visualConfigs[zombieData.type];
            zombieGO.transform.localScale = config.size;
            spriteRenderer.color = config.baseColor;
            
            // 添加血条
            CreateHealthBar(zombieGO, zombieData);
            
            // 添加阴影效果
            CreateZombieShadow(zombieGO);
            
            return zombieGO;
        }
        
        /// <summary>
        /// 创建僵尸阴影
        /// </summary>
        private void CreateZombieShadow(GameObject zombieGO)
        {
            GameObject shadowGO = new GameObject("Shadow");
            shadowGO.transform.SetParent(zombieGO.transform);
            shadowGO.transform.localPosition = new Vector3(0.1f, -0.1f, 0);
            shadowGO.transform.localScale = new Vector3(0.8f, 0.3f, 1f);
            
            var shadowRenderer = shadowGO.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = zombieGO.GetComponent<SpriteRenderer>().sprite;
            shadowRenderer.color = new Color(0, 0, 0, 0.3f);
            shadowRenderer.sortingOrder = 9;
        }
        
        private void CreateHealthBar(GameObject parent, ZombieData zombieData)
        {
            // 创建血条背景
            GameObject healthBarBG = new GameObject("HealthBarBG");
            healthBarBG.transform.SetParent(parent.transform);
            healthBarBG.transform.localPosition = new Vector3(0, 0.6f, 0);
            healthBarBG.transform.localScale = Vector3.one;
            
            var bgRenderer = healthBarBG.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = CreateHealthBarSprite(Color.red, 32, 4);
            bgRenderer.sortingOrder = 11;
            
            // 创建血条前景
            GameObject healthBar = new GameObject("HealthBar");
            healthBar.transform.SetParent(healthBarBG.transform);
            healthBar.transform.localPosition = Vector3.zero;
            healthBar.transform.localScale = Vector3.one;
            
            var fgRenderer = healthBar.AddComponent<SpriteRenderer>();
            fgRenderer.sprite = CreateHealthBarSprite(Color.green, 32, 4);
            fgRenderer.sortingOrder = 12;
        }
        
        private Sprite CreateHealthBarSprite(Color color, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
        }

        public void UpdateZombieVisual(string zombieId, ZombieData zombieData)
        {
            if (!zombieVisuals.ContainsKey(zombieId))
                return;
                
            // 更新位置（使用DOTween平滑移动）
            UpdateZombiePosition(zombieId, zombieData.position);
            
            // 更新血量显示
            UpdateHealthBar(zombieId, zombieData.currentHealth / zombieData.maxHealth);
            
            // 更新状态表现
            UpdateZombieStateVisual(zombieId, zombieData.state);
        }
        
        private void UpdateZombiePosition(string zombieId, Vector2 targetPosition)
        {
            if (!zombieVisuals.ContainsKey(zombieId))
                return;
                
            var zombieGO = zombieVisuals[zombieId];
            Vector3 currentPos = zombieGO.transform.position;
            Vector3 targetPos3D = new Vector3(targetPosition.x, targetPosition.y, 0);
            
            float distance = Vector3.Distance(currentPos, targetPos3D);
            if (distance < 0.1f) return; // 距离太小不需要移动
            
            // 取消之前的移动动画
            if (movementTweens.ContainsKey(zombieId))
            {
                movementTweens[zombieId]?.Kill();
            }
            
            // 创建新的移动动画
            var zombieType = GetZombieType(zombieId);
            var config = visualConfigs[zombieType];
            float duration = distance / (config.moveSpeed * 3f);
            duration = Mathf.Clamp(duration, 0.1f, 1f);
            
            // 停止待机动画，开始移动动画
            StopIdleAnimation(zombieId);
            
            var sequence = DOTween.Sequence();
            
            // 移动动画
            var moveTween = zombieGO.transform.DOMove(targetPos3D, duration)
                .SetEase(Ease.Linear);
            
            // 移动时的上下摆动
            var bobTween = zombieGO.transform.DOMoveY(targetPos3D.y + walkBobAmplitude, walkBobSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
            sequence.Append(moveTween);
            sequence.Join(bobTween);
            sequence.OnComplete(() =>
            {
                if (movementTweens.ContainsKey(zombieId))
                    movementTweens.Remove(zombieId);
                
                // 移动完成后恢复待机动画
                StartIdleAnimation(zombieId);
                bobTween.Kill();
            });
            
            movementTweens[zombieId] = sequence;
        }
        
        /// <summary>
        /// 开始待机动画
        /// </summary>
        private void StartIdleAnimation(string zombieId)
        {
            if (!zombieVisuals.ContainsKey(zombieId) || animationSequences.ContainsKey(zombieId))
                return;
                
            var zombieGO = zombieVisuals[zombieId];
            var zombieType = GetZombieType(zombieId);
            var config = visualConfigs[zombieType];
            
            var sequence = DOTween.Sequence();
            
            // 轻微的上下浮动
            var floatTween = zombieGO.transform.DOMoveY(zombieGO.transform.position.y + idleFloatAmplitude, idleFloatSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
            // 轻微的左右摇摆
            var swayTween = zombieGO.transform.DORotate(new Vector3(0, 0, 5f), idleFloatSpeed * 1.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
            sequence.Join(floatTween);
            sequence.Join(swayTween);
            sequence.SetLoops(-1);
            
            animationSequences[zombieId] = sequence;
        }
        
        /// <summary>
        /// 停止待机动画
        /// </summary>
        private void StopIdleAnimation(string zombieId)
        {
            if (animationSequences.ContainsKey(zombieId))
            {
                animationSequences[zombieId]?.Kill();
                animationSequences.Remove(zombieId);
            }
        }
        
        private void UpdateHealthBar(string zombieId, float healthPercent)
        {
            if (!zombieVisuals.ContainsKey(zombieId))
                return;
                
            var zombieGO = zombieVisuals[zombieId];
            var healthBar = zombieGO.transform.Find("HealthBarBG/HealthBar");
            
            if (healthBar != null)
            {
                // 平滑更新健康条长度
                healthBar.DOScaleX(healthPercent, 0.2f);
                
                // 更新颜色
                var renderer = healthBar.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Color healthColor = Color.Lerp(Color.red, Color.green, healthPercent);
                    renderer.DOColor(healthColor, 0.2f);
                }
            }
        }
        
        private void UpdateZombieStateVisual(string zombieId, ZombieState state)
        {
            if (!zombieRenderers.ContainsKey(zombieId))
                return;
                
            var renderer = zombieRenderers[zombieId];
            var zombieGO = zombieVisuals[zombieId];
            
            switch (state)
            {
                case ZombieState.Wandering:
                    renderer.DOColor(Color.white, 0.3f);
                    break;
                    
                case ZombieState.Approaching:
                    renderer.DOColor(Color.yellow, 0.3f);
                    // 增加移动速度感
                    if (!movementTweens.ContainsKey(zombieId))
                    {
                        zombieGO.transform.DOShakePosition(0.1f, 0.05f, 10, 0, false, true);
                    }
                    break;
                    
                case ZombieState.Attacking:
                    renderer.DOColor(Color.red, 0.3f);
                    break;
                    
                case ZombieState.Dead:
                    renderer.DOColor(Color.gray, 0.5f);
                    break;
            }
        }
        
        public void RemoveZombieVisual(string zombieId)
        {
            if (zombieVisuals.ContainsKey(zombieId))
            {
                // 停止所有动画
                if (movementTweens.ContainsKey(zombieId))
                {
                    movementTweens[zombieId]?.Kill();
                    movementTweens.Remove(zombieId);
                }
                
                if (animationSequences.ContainsKey(zombieId))
                {
                    animationSequences[zombieId]?.Kill();
                    animationSequences.Remove(zombieId);
                }
                
                // 销毁游戏对象
                GameObject.Destroy(zombieVisuals[zombieId]);
                zombieVisuals.Remove(zombieId);
                
                if (zombieRenderers.ContainsKey(zombieId))
                {
                    zombieRenderers.Remove(zombieId);
                }
                
                UnityEngine.Debug.Log($"移除僵尸视觉对象: {zombieId}");
            }
        }
        
        public void UpdateAllZombieVisuals()
        {
            if (Time.time - lastVisualUpdate < VISUAL_UPDATE_INTERVAL)
                return;
                
            var zombieData = zombieSystem.GetZombieData();
            
            foreach (var kvp in zombieData.zombies)
            {
                string zombieId = kvp.Key;
                var zombie = kvp.Value;
                
                if (zombie.IsAlive)
                {
                    UpdateZombieVisual(zombieId, zombie);
                }
            }
            
            lastVisualUpdate = Time.time;
        }
        
        public GameObject GetZombieVisual(string zombieId)
        {
            return zombieVisuals.ContainsKey(zombieId) ? zombieVisuals[zombieId] : null;
        }
        
        /// <summary>
        /// 播放生成动画
        /// </summary>
        private void PlaySpawnAnimation(string zombieId)
        {
            if (!zombieVisuals.ContainsKey(zombieId))
                return;
                
            var zombieGO = zombieVisuals[zombieId];
            var renderer = zombieRenderers[zombieId];
            var config = visualConfigs[GetZombieType(zombieId)];
            
            UnityEngine.Debug.Log($"开始播放僵尸 {zombieId} 的生成动画");
            
            // 保存原始位置
            Vector3 originalPosition = zombieGO.transform.position;
            
            // 简化的生成动画 - 从小到大
            zombieGO.transform.localScale = Vector3.zero;
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1f); // 保持完全不透明
            
            var sequence = DOTween.Sequence();
            
            // 缩放动画 - 从0到正常大小
            sequence.Append(zombieGO.transform.DOScale(config.size, 0.5f)
                .SetEase(Ease.OutBack));
            
            // 轻微震动效果
            sequence.AppendCallback(() => {
                zombieGO.transform.DOShakeScale(0.2f, 0.1f, 3, 90, true);
                UnityEngine.Debug.Log($"僵尸 {zombieId} 生成动画播放完成");
            });
        }
        
        /// <summary>
        /// 播放攻击动画
        /// </summary>
        private void PlayAttackAnimation(string zombieId)
        {
            if (!zombieVisuals.ContainsKey(zombieId))
                return;
                
            var zombieGO = zombieVisuals[zombieId];
            var renderer = zombieRenderers[zombieId];
            var originalScale = zombieGO.transform.localScale;
            var originalColor = renderer.color;
            
            var sequence = DOTween.Sequence();
            
            // 向前冲刺
            sequence.Append(zombieGO.transform.DOMoveX(zombieGO.transform.position.x + 0.2f, 0.1f)
                .SetEase(Ease.OutQuad));
            
            // 放大并变红
            sequence.Join(zombieGO.transform.DOScale(originalScale * 1.2f, 0.1f));
            sequence.Join(renderer.DOColor(Color.red, 0.1f));
            
            // 后退并恢复
            sequence.Append(zombieGO.transform.DOMoveX(zombieGO.transform.position.x - 0.2f, 0.15f)
                .SetEase(Ease.OutBounce));
            sequence.Join(zombieGO.transform.DOScale(originalScale, 0.15f));
            sequence.Join(renderer.DOColor(originalColor, 0.15f));
            
            // 震动效果
            sequence.AppendCallback(() => {
                zombieGO.transform.DOShakeRotation(0.2f, 10f, 5, 90, true);
            });
        }
        
        /// <summary>
        /// 播放受伤动画
        /// </summary>
        private void PlayDamageAnimation(string zombieId)
        {
            if (!zombieRenderers.ContainsKey(zombieId))
                return;
                
            var zombieGO = zombieVisuals[zombieId];
            var renderer = zombieRenderers[zombieId];
            var originalColor = renderer.color;
            
            var sequence = DOTween.Sequence();
            
            // 闪红
            sequence.Append(renderer.DOColor(Color.red, 0.1f));
            sequence.Append(renderer.DOColor(originalColor, 0.1f));
            
            // 后退
            sequence.Join(zombieGO.transform.DOMoveX(zombieGO.transform.position.x - 0.1f, 0.1f)
                .SetEase(Ease.OutQuad));
            sequence.Append(zombieGO.transform.DOMoveX(zombieGO.transform.position.x + 0.1f, 0.1f)
                .SetEase(Ease.OutBounce));
            
            // 震动
            sequence.AppendCallback(() => {
                zombieGO.transform.DOShakePosition(0.15f, 0.05f, 8, 90, false, true);
            });
        }
        
        /// <summary>
        /// 播放死亡动画
        /// </summary>
        private void PlayDeathAnimation(string zombieId)
        {
            if (!zombieVisuals.ContainsKey(zombieId))
                return;
                
            var zombieGO = zombieVisuals[zombieId];
            var renderer = zombieRenderers[zombieId];
            
            // 停止所有其他动画
            StopIdleAnimation(zombieId);
            if (movementTweens.ContainsKey(zombieId))
            {
                movementTweens[zombieId]?.Kill();
                movementTweens.Remove(zombieId);
            }
            
            var sequence = DOTween.Sequence();
            
            // 倒地旋转
            sequence.Append(zombieGO.transform.DORotate(new Vector3(0, 0, 90), 0.5f)
                .SetEase(Ease.OutBounce));
            
            // 缩放消失
            sequence.Append(zombieGO.transform.DOScale(Vector3.zero, 1f)
                .SetEase(Ease.InBack));
            
            // 透明度消失
            sequence.Join(renderer.DOFade(0f, 1f));
            
            // 血条也要消失
            var healthBarBG = zombieGO.transform.Find("HealthBarBG");
            if (healthBarBG != null)
            {
                sequence.Join(healthBarBG.GetComponent<SpriteRenderer>().DOFade(0f, 0.5f));
                var healthBar = healthBarBG.Find("HealthBar");
                if (healthBar != null)
                {
                    sequence.Join(healthBar.GetComponent<SpriteRenderer>().DOFade(0f, 0.5f));
                }
            }
            
            // 阴影消失
            var shadow = zombieGO.transform.Find("Shadow");
            if (shadow != null)
            {
                sequence.Join(shadow.GetComponent<SpriteRenderer>().DOFade(0f, 0.8f));
            }
        }
        
        public void ShowDamageEffect(string zombieId, float damage)
        {
            PlayDamageAnimation(zombieId);
            
            // 创建伤害数字显示
            if (zombieVisuals.ContainsKey(zombieId))
            {
                var zombieGO = zombieVisuals[zombieId];
                CreateDamageText(zombieGO.transform.position, damage);
            }
        }
        
        public void ShowDeathEffect(string zombieId)
        {
            PlayDeathAnimation(zombieId);
            
            // 创建死亡特效
            if (zombieVisuals.ContainsKey(zombieId))
            {
                var zombieGO = zombieVisuals[zombieId];
                CreateDeathEffect(zombieGO.transform.position);
            }
        }
        
        /// <summary>
        /// 创建伤害数字显示
        /// </summary>
        private void CreateDamageText(Vector3 position, float damage)
        {
            GameObject textGO = new GameObject("DamageText");
            textGO.transform.position = position + Vector3.up * 0.8f;
            
            var textMesh = textGO.AddComponent<TextMesh>();
            textMesh.text = "-" + damage.ToString("F0");
            textMesh.fontSize = 24;
            textMesh.color = Color.red;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.fontStyle = FontStyle.Bold;
            
            // 伤害数字动画
            var sequence = DOTween.Sequence();
            
            // 向上浮动
            sequence.Append(textGO.transform.DOMoveY(position.y + 1.5f, 1f)
                .SetEase(Ease.OutQuad));
            
            // 缩放效果
            textGO.transform.localScale = Vector3.zero;
            sequence.Join(textGO.transform.DOScale(Vector3.one, 0.2f)
                .SetEase(Ease.OutBack));
            
                         // 淡出
             sequence.Append(DOTween.ToAlpha(() => textMesh.color, x => textMesh.color = x, 0f, 0.5f));
            
            // 销毁
            sequence.OnComplete(() => GameObject.Destroy(textGO));
        }
        
        /// <summary>
        /// 创建死亡特效
        /// </summary>
        private void CreateDeathEffect(Vector3 position)
        {
            // 创建粒子效果（简化版）
            for (int i = 0; i < 5; i++)
            {
                GameObject particle = new GameObject("DeathParticle");
                particle.transform.position = position;
                
                var renderer = particle.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateParticleSprite();
                renderer.color = new Color(0.5f, 0, 0, 1f);
                renderer.sortingOrder = 5;
                
                // 随机方向飞散
                Vector3 direction = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f),
                    0
                ).normalized;
                
                var sequence = DOTween.Sequence();
                
                // 飞散动画
                sequence.Append(particle.transform.DOMove(position + direction * 2f, 1f)
                    .SetEase(Ease.OutQuad));
                
                // 旋转
                sequence.Join(particle.transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360));
                
                // 淡出
                sequence.Join(renderer.DOFade(0f, 1f));
                
                // 销毁
                sequence.OnComplete(() => GameObject.Destroy(particle));
            }
        }
        
        /// <summary>
        /// 创建粒子图片
        /// </summary>
        private Sprite CreateParticleSprite()
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
                        texture.SetPixel(x, y, Color.red);
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
        
        private ZombieType GetZombieType(string zombieId)
        {
            var zombieData = zombieSystem.GetZombieData();
            if (zombieData.zombies.ContainsKey(zombieId))
            {
                return zombieData.zombies[zombieId].type;
            }
            return ZombieType.Walker; // 默认类型
        }
    }
    
    /// <summary>
    /// 增强的僵尸视觉配置
    /// </summary>
    [System.Serializable]
    public class ZombieVisualConfig
    {
        public Color baseColor;
        public Vector3 size;
        public float moveSpeed;
        public float bobIntensity;    // 移动时摆动强度
        public float rotationSpeed;   // 旋转速度
        
        public ZombieVisualConfig()
        {
            baseColor = Color.gray;
            size = Vector3.one;
            moveSpeed = 1f;
            bobIntensity = 1f;
            rotationSpeed = 30f;
        }
    }
} 