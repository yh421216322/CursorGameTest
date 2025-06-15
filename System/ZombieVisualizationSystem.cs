// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieVisualizationSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了僵尸可视化系统 (ZombieVisualizationSystem)，负责处理游戏中僵尸单位的
//     视觉表现，包括创建、更新、移除僵尸的GameObject，加载和应用僵尸的图片资源，
//     以及播放各种动画（如生成、移动、攻击、受伤、死亡）和视觉特效。
//     该系统使用DOTween库来实现平滑的动画效果，并能动态生成简单的精灵图作为占位符。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq; // 用于 LINQ 查询 (当前代码中未明显使用，但可能未来扩展会用到)
using UnityEngine;   // Unity 核心功能
using QFramework;    // QFramework 框架
using DG.Tweening;   // DOTween 动画库，用于创建平滑的动画效果
using MyGameNamespace; // 包含ZombieSpawnedEvent, ZombieDeathEvent等自定义事件的命名空间
using SurvivalGame.Model; // 游戏核心数据模型，如ZombieData, ZombieType等
using SurvivalGame.GameSystem; // 游戏系统命名空间，用于获取其他系统如IZombieSystem

namespace SurvivalGame.Visualization // 将此系统归类到可视化相关的命名空间
{
    /// <summary>
    /// 僵尸可视化系统接口 (IZombieVisualizationSystem)。
    /// 定义了僵尸在游戏世界中视觉表现所需的核心功能。
    /// </summary>
    public interface IZombieVisualizationSystem : QFISystem
    {
        /// <summary>
        /// 根据提供的僵尸数据创建一个新的僵尸视觉对象（GameObject）。
        /// </summary>
        /// <param name="zombieData">新生成僵尸的数据。</param>
        void CreateZombieVisual(ZombieData zombieData);

        /// <summary>
        /// 根据最新的僵尸数据更新指定ID僵尸的视觉表现（如位置、状态动画、血条等）。
        /// </summary>
        /// <param name="zombieId">要更新视觉的僵尸的唯一ID。</param>
        /// <param name="zombieData">该僵尸的最新数据。</param>
        void UpdateZombieVisual(string zombieId, ZombieData zombieData);

        /// <summary>
        /// 从场景中移除指定ID僵尸的视觉对象。
        /// </summary>
        /// <param name="zombieId">要移除视觉对象的僵尸ID。</param>
        void RemoveZombieVisual(string zombieId);

        /// <summary>
        /// 更新所有当前活动僵尸的视觉表现。
        /// 通常由系统按固定时间间隔调用。
        /// </summary>
        void UpdateAllZombieVisuals();

        /// <summary>
        /// 根据僵尸ID获取其在场景中对应的GameObject。
        /// </summary>
        /// <param name="zombieId">僵尸的唯一ID。</param>
        /// <returns>对应的GameObject；如果不存在，则返回null。</returns>
        GameObject GetZombieVisual(string zombieId);

        /// <summary>
        /// 在指定僵尸位置显示受伤的视觉特效（如伤害数字、闪烁等）。
        /// </summary>
        /// <param name="zombieId">受伤害僵尸的ID。</param>
        /// <param name="damage">造成的伤害数值（可能用于显示伤害数字）。</param>
        void ShowDamageEffect(string zombieId, float damage);

        /// <summary>
        /// 在指定僵尸位置显示死亡的视觉特效和动画。
        /// </summary>
        /// <param name="zombieId">已死亡僵尸的ID。</param>
        void ShowDeathEffect(string zombieId);
    }

    /// <summary>
    /// 增强的僵尸可视化系统 (ZombieVisualizationSystem) 的具体实现。
    /// 使用Unity的GameObject、SpriteRenderer以及DOTween动画库来创建和管理僵尸的视觉表现。
    /// </summary>
    public class ZombieVisualizationSystem : AbstractSystem, IZombieVisualizationSystem
    {
        // --- 系统引用 (System References) ---
        /// <summary>基础僵尸系统，用于获取僵尸的逻辑数据。</summary>
        private IZombieSystem zombieSystem;
        
        // --- 视觉对象管理 (Visual Object Management) ---
        /// <summary>存储僵尸ID与其对应的Unity场景中GameObject的映射。</summary>
        private Dictionary<string, GameObject> zombieVisuals;
        /// <summary>存储僵尸ID与其SpriteRenderer组件的映射，方便快速访问以修改视觉属性。</summary>
        private Dictionary<string, SpriteRenderer> zombieRenderers;
        /// <summary>存储僵尸ID与其当前活动的DOTween移动动画的映射，用于控制和取消动画。</summary>
        private Dictionary<string, Tween> movementTweens;
        /// <summary>存储僵尸ID与其当前活动的复杂动画序列（如待机动画）的映射。</summary>
        private Dictionary<string, Sequence> animationSequences;
        
        // --- 僵尸图片与配置资源 (Zombie Sprite and Configuration Resources) ---
        /// <summary>存储每种僵尸类型对应的Sprite图片资源。</summary>
        private Dictionary<ZombieType, Sprite> zombieSprites;
        /// <summary>存储每种僵尸类型的视觉配置参数（如基础颜色、大小、动画参数等）。</summary>
        private Dictionary<ZombieType, ZombieVisualConfig> visualConfigs;
        
        // --- 动画预设参数 (Animation Presets) ---
        /// <summary>僵尸行走时上下摆动的幅度。</summary>
        private float walkBobAmplitude = 0.1f;
        /// <summary>僵尸行走时上下摆动的速度。</summary>
        private float walkBobSpeed = 2f;
        /// <summary>僵尸待机时轻微上下浮动的幅度。</summary>
        private float idleFloatAmplitude = 0.05f;
        /// <summary>僵尸待机时轻微上下浮动的速度。</summary>
        private float idleFloatSpeed = 1f;
        
        // --- 更新参数 (Update Parameters) ---
        /// <summary>上一次执行批量视觉更新的时间戳。</summary>
        private float lastVisualUpdate;
        /// <summary>批量视觉更新的时间间隔（秒），以控制更新频率，优化性能。</summary>
        private const float VISUAL_UPDATE_INTERVAL = 0.1f; // 每0.1秒更新一次所有僵尸视觉
        
        /// <summary>
        /// 系统初始化方法。
        /// 获取其他系统引用，初始化内部数据容器，加载所需资源（如僵尸图片），
        /// 并注册相关的游戏事件监听器。
        /// </summary>
        protected override void OnInit()
        {
            // 获取所需系统模块的引用
            zombieSystem = this.GetSystem<IZombieSystem>();
            
            // 初始化用于存储视觉对象、渲染器、动画等信息的字典
            zombieVisuals = new Dictionary<string, GameObject>();
            zombieRenderers = new Dictionary<string, SpriteRenderer>();
            movementTweens = new Dictionary<string, Tween>();
            animationSequences = new Dictionary<string, Sequence>();
            zombieSprites = new Dictionary<ZombieType, Sprite>();
            visualConfigs = new Dictionary<ZombieType, ZombieVisualConfig>();
            
            LoadZombieSprites();       // 加载僵尸的图片资源
            InitializeVisualConfigs(); // 初始化各种僵尸类型的视觉配置参数
            
            // 注册监听游戏内的僵尸相关事件，以便创建、移除或更新僵尸的视觉表现
            this.RegisterEvent<ZombieSpawnedEvent>(OnZombieSpawned);       // 监听僵尸生成事件
            this.RegisterEvent<ZombieDeathEvent>(OnZombieDied);           // 监听僵尸死亡事件
            this.RegisterEvent<ZombieAttackBuildingEvent>(OnZombieAttack); // 监听僵尸攻击事件 (用于触发攻击动画)
            
            lastVisualUpdate = Time.time; // 初始化上次视觉更新的时间戳
            
            UnityEngine.Debug.Log("[僵尸可视化系统] 初始化完成。");
        }
        
        /// <summary>
        /// 加载所有僵尸类型的图片资源。
        /// 尝试从Unity的Resources文件夹中加载图片；如果失败，则创建程序化的默认图片作为备用。
        /// </summary>
        private void LoadZombieSprites()
        {
            try
            {
                // UnityEngine.Debug.Log("[僵尸可视化系统] 开始加载僵尸图片资源...");
                
                // 示例：加载名为 "zb1", "zb2" 等的图片资源 (应位于 "Resources/Image/" 目录下)
                var zb1Sprite = Resources.Load<Sprite>("Image/zb1");
                var zb2Sprite = Resources.Load<Sprite>("Image/zb2");
                var zb3Sprite = Resources.Load<Sprite>("Image/zb3");
                var zb4Sprite = Resources.Load<Sprite>("Image/zb4");
                
                // UnityEngine.Debug.Log($"[僵尸可视化系统] 图片加载结果: zb1='{zb1Sprite?.name}', zb2='{zb2Sprite?.name}', zb3='{zb3Sprite?.name}', zb4='{zb4Sprite?.name}'");
                
                // 将加载到的（或默认创建的）Sprite分配给各种僵尸类型
                zombieSprites[ZombieType.Walker] = zb1Sprite ?? CreateDefaultZombieSprite(ZombieType.Walker);     // 普通僵尸
                zombieSprites[ZombieType.Runner] = zb2Sprite ?? CreateDefaultZombieSprite(ZombieType.Runner);     // 快速僵尸
                zombieSprites[ZombieType.Tank] = zb3Sprite ?? CreateDefaultZombieSprite(ZombieType.Tank);         // 坦克僵尸
                zombieSprites[ZombieType.Spitter] = zb4Sprite ?? CreateDefaultZombieSprite(ZombieType.Spitter);   // 吐酸者
                zombieSprites[ZombieType.Screamer] = zb1Sprite ?? CreateDefaultZombieSprite(ZombieType.Screamer); // 尖叫者 (示例：复用zb1或创建特定默认图)
                
                // UnityEngine.Debug.Log($"[僵尸可视化系统] 僵尸图片资源加载完成，共为 {zombieSprites.Count} 个主要类型分配了图片。");
                // foreach (var kvp in zombieSprites) // 调试：显示每个类型的最终Sprite信息
                // {
                //     UnityEngine.Debug.Log($"  {kvp.Key} -> {(kvp.Value != null ? kvp.Value.name : "默认程序化图片")} (尺寸: {(kvp.Value != null ? kvp.Value.bounds.size.ToString() : "N/A")})");
                // }
            }
            catch (System.Exception e) // 捕获加载过程中可能发生的任何异常
            {
                UnityEngine.Debug.LogError($"[僵尸可视化系统] 加载僵尸图片资源时发生严重错误: {e.Message}\nStackTrace: {e.StackTrace}");
                UnityEngine.Debug.LogWarning("[僵尸可视化系统] 由于资源加载失败，将为所有僵尸类型创建默认的程序化图片。");
                // 如果加载失败，为所有僵尸类型创建默认的程序化图片作为后备
                foreach (ZombieType type in System.Enum.GetValues(typeof(ZombieType))) // 遍历所有ZombieType枚举值
                {
                    if (!zombieSprites.ContainsKey(type) || zombieSprites[type] == null) // 仅当尚未成功加载时创建
                    {
                        zombieSprites[type] = CreateDefaultZombieSprite(type);
                        // UnityEngine.Debug.Log($"[僵尸可视化系统] 已为僵尸类型 {type} 创建了默认的程序化图片。");
                    }
                }
            }
        }
        
        /// <summary>
        /// （辅助方法）为指定僵尸类型创建一个程序化的默认Sprite图片。
        /// 此方法作为图片资源加载失败时的备用方案，或用于快速原型开发。
        /// </summary>
        /// <param name="type">要为其创建默认图片的僵尸类型。</param>
        /// <returns>一个新创建的Sprite对象。</returns>
        private Sprite CreateDefaultZombieSprite(ZombieType type)
        {
            int textureSize = 64; // 默认纹理尺寸为64x64像素
            Texture2D dynamicTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false); // 创建一个空的RGBA32纹理
            
            // 获取该僵尸类型的基础颜色（如果已配置，否则使用默认颜色）
            Color baseColor = visualConfigs.TryGetValue(type, out var config) ? config.baseColor : GetZombieTypeColor(type);
            
            // 简单的程序化图像：以僵尸类型基础色为底，添加一些噪点和边缘阴影，并根据类型添加微小特征
            Vector2 textureCenter = new Vector2(textureSize * 0.5f, textureSize * 0.5f); // 纹理中心点
            float spriteRadius = textureSize * 0.4f; // 主体圆形区域的半径
            
            for (int x = 0; x < textureSize; x++)
            {
                for (int y = 0; y < textureSize; y++)
                {
                    Vector2 currentPixelPos = new Vector2(x, y);
                    float distanceToCenter = Vector2.Distance(currentPixelPos, textureCenter); // 当前像素到中心的距离
                    
                    if (distanceToCenter <= spriteRadius) // 如果像素在主体圆形区域内
                    {
                        Color pixelColor = baseColor; // 从基础颜色开始
                        
                        // 添加Perlin噪点以增加纹理细节
                        float noiseValue = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.2f; // 噪点强度0.2
                        pixelColor += new Color(noiseValue, noiseValue, noiseValue, 0); // 噪点只影响RGB，不影响Alpha
                        
                        // 为圆形边缘添加简单的阴影效果
                        if (distanceToCenter > spriteRadius * 0.8f) // 在半径的80%以外区域
                        {
                            pixelColor *= 0.6f; // 颜色变暗60%
                        }
                        
                        AddZombieFeatures(ref pixelColor, type, x, y, textureSize, textureCenter); // 根据僵尸类型添加额外视觉特征
                        
                        pixelColor.a = 1f; // 确保主体区域完全不透明
                        dynamicTexture.SetPixel(x, y, pixelColor); // 设置像素颜色
                    }
                    else
                    {
                        dynamicTexture.SetPixel(x, y, Color.clear); // 圆形区域以外的像素设为透明
                    }
                }
            }
            
            dynamicTexture.filterMode = FilterMode.Point; // 使用点滤波，使像素风格更明显
            dynamicTexture.Apply(); // 应用所有像素更改到纹理
            
            // 从生成的Texture2D创建Sprite对象
            return Sprite.Create(dynamicTexture, new Rect(0, 0, textureSize, textureSize),
                                 new Vector2(0.5f, 0.5f), 32f); // 每Unity单位对应32像素
        }
        
        /// <summary>
        /// （辅助方法）根据僵尸类型获取其主题颜色。
        /// </summary>
        /// <param name="type">僵尸类型。</param>
        /// <returns>对应的颜色。</returns>
        private Color GetZombieTypeColor(ZombieType type)
        {
            switch (type) // 为不同僵尸类型定义不同的基础色调
            {
                case ZombieType.Walker:   return new Color(0.5f, 0.6f, 0.4f); // 行走者 - 暗绿色系
                case ZombieType.Runner:   return new Color(0.7f, 0.4f, 0.4f); // 奔跑者 - 暗红色系
                case ZombieType.Tank:     return new Color(0.4f, 0.5f, 0.3f); // 坦克 - 更深的暗绿色
                case ZombieType.Spitter:  return new Color(0.6f, 0.6f, 0.3f); // 吐酸者 - 黄绿色
                case ZombieType.Screamer: return new Color(0.6f, 0.3f, 0.6f); // 尖叫者 - 紫色系
                default:                  return new Color(0.5f, 0.5f, 0.5f); // 其他或未知类型 - 灰色
            }
        }
        
        /// <summary>
        /// （辅助方法）在程序化生成僵尸图片时，为不同类型的僵尸添加一些独特的视觉特征。
        /// </summary>
        /// <param name="pixelColor">当前像素的颜色（引用传递，将被修改）。</param>
        /// <param name="type">僵尸类型。</param>
        /// <param name="x">当前像素的x坐标。</param>
        /// <param name="y">当前像素的y坐标。</param>
        /// <param name="size">纹理的总尺寸。</param>
        /// <param name="center">纹理的中心点坐标。</param>
        private void AddZombieFeatures(ref Color pixelColor, ZombieType type, int x, int y, int size, Vector2 center)
        {
            float distanceFromCenter = Vector2.Distance(new Vector2(x, y), center); // 当前像素到中心的距离
            float radius = size * 0.4f; // 主体圆形区域半径
            
            switch (type) // 根据僵尸类型添加不同特征
            {
                case ZombieType.Runner: // 奔跑者：添加一些血红色条纹
                    if ((x + y) % 8 < 2) // 通过取模运算制造条纹感
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.red, 0.4f); // 与红色混合，比例0.4
                    }
                    break;
                    
                case ZombieType.Tank: // 坦克：颜色更深，添加灰色模拟装甲感
                    pixelColor *= 0.8f; // 整体颜色加深
                    if (x % 4 == 0 || y % 4 == 0) // 每隔4像素制造格子状纹理
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.gray, 0.3f); // 与灰色混合
                    }
                    break;
                    
                case ZombieType.Spitter: // 吐酸者：在特定区域（如下半部分）添加黄色模拟酸液效果
                    if (y > size * 0.6f && distanceFromCenter < radius * 0.3f) // 仅在“嘴部”附近区域
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.yellow, 0.6f); // 与黄色混合
                    }
                    break;
                    
                case ZombieType.Screamer: // 尖叫者：添加品红色能量环效果
                    if (Mathf.Abs(distanceFromCenter - radius * 0.7f) < 2f) // 在半径70%附近的一个窄环带
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.magenta, 0.5f); // 与品红色混合
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 初始化各种僵尸类型的视觉配置参数。
        /// 这些配置包括基础颜色、大小、移动动画参数等。
        /// </summary>
        private void InitializeVisualConfigs()
        {
            // 为每种僵尸类型创建一个ZombieVisualConfig实例并设置其属性
            visualConfigs[ZombieType.Walker] = new ZombieVisualConfig {
                baseColor = GetZombieTypeColor(ZombieType.Walker), // 基础颜色
                size = Vector3.one * 0.8f,                         // 相对大小 (0.8倍标准大小)
                moveSpeed = 1f,                                    // 移动动画速度因子
                bobIntensity = 0.8f,                               // 行走时上下摆动强度
                rotationSpeed = 30f                                // （未使用）旋转速度
            };
            
            visualConfigs[ZombieType.Runner] = new ZombieVisualConfig {
                baseColor = GetZombieTypeColor(ZombieType.Runner),
                size = Vector3.one * 0.7f, // 奔跑者体型稍小
                moveSpeed = 2f,            // 移动动画更快
                bobIntensity = 1.2f,       // 摆动更剧烈
                rotationSpeed = 60f
            };
            
            visualConfigs[ZombieType.Tank] = new ZombieVisualConfig {
                baseColor = GetZombieTypeColor(ZombieType.Tank),
                size = Vector3.one * 1.2f, // 坦克体型更大
                moveSpeed = 0.5f,          // 移动动画更慢重
                bobIntensity = 0.4f,       // 摆动较小，显得沉稳
                rotationSpeed = 15f
            };
            
            visualConfigs[ZombieType.Spitter] = new ZombieVisualConfig {
                baseColor = GetZombieTypeColor(ZombieType.Spitter),
                size = Vector3.one * 0.9f,
                moveSpeed = 1.2f,
                bobIntensity = 0.6f,
                rotationSpeed = 40f
            };
            
            visualConfigs[ZombieType.Screamer] = new ZombieVisualConfig {
                baseColor = GetZombieTypeColor(ZombieType.Screamer),
                size = Vector3.one * 1.1f, // 尖叫者体型可能略大或有特殊轮廓
                moveSpeed = 1.5f,
                bobIntensity = 1.0f,
                rotationSpeed = 50f
            };
        }
        
        /// <summary>
        /// 处理僵尸生成事件。当接收到ZombieSpawnedEvent时，创建对应僵尸的视觉对象。
        /// </summary>
        /// <param name="e">僵尸生成事件参数。</param>
        private void OnZombieSpawned(ZombieSpawnedEvent e)
        {
            UnityEngine.Debug.Log($"[可视化系统] 🔥 收到僵尸生成事件，准备创建视觉对象: {e.ZombieId}");
            try
            {
                // 从基础僵尸系统获取生成僵尸的完整数据
                var zombieData = zombieSystem.GetZombieData()?.zombies?[e.ZombieId];
                if (zombieData == null) {
                    UnityEngine.Debug.LogError($"[可视化系统] ❌ 获取僵尸数据失败，ID: {e.ZombieId}。无法创建视觉对象。");
                    return;
                }
                // UnityEngine.Debug.Log($"[可视化系统] 📊 成功获取僵尸数据: 类型={zombieData.type}, 位置={zombieData.position}");
                CreateZombieVisual(zombieData); // 创建该僵尸的视觉表现
                // UnityEngine.Debug.Log($"[可视化系统] ✅ 僵尸视觉对象创建流程调用完成: {e.ZombieId}");
            }
            catch (System.Exception ex) // 捕获创建过程中可能发生的任何异常
            {
                UnityEngine.Debug.LogError($"[可视化系统] ❌ 创建僵尸视觉对象时发生严重错误 (ID: {e.ZombieId}): {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 处理僵尸死亡事件。当接收到ZombieDeathEvent时，播放死亡特效并延迟移除视觉对象。
        /// </summary>
        /// <param name="e">僵尸死亡事件参数。</param>
        private void OnZombieDied(ZombieDeathEvent e)
        {
            ShowDeathEffect(e.ZombieId); // 显示死亡特效（如爆炸、消失动画）
            // 延迟2秒后移除僵尸的视觉对象，以便死亡动画有足够时间播放完毕
            DOVirtual.DelayedCall(2f, () => RemoveZombieVisual(e.ZombieId));
        }
        
        /// <summary>
        /// 处理僵尸攻击建筑事件。当接收到ZombieAttackBuildingEvent时，播放攻击动画。
        /// </summary>
        /// <param name="e">僵尸攻击建筑事件参数。</param>
        private void OnZombieAttack(ZombieAttackBuildingEvent e)
        {
            PlayAttackAnimation(e.ZombieId); // 为攻击的僵尸播放攻击动画
        }
        
        /// <summary>
        /// 为指定的僵尸数据创建其在游戏世界中的视觉表现 (GameObject)。
        /// </summary>
        /// <param name="zombieData">要创建视觉对象的僵尸的数据。</param>
        public void CreateZombieVisual(ZombieData zombieData)
        {
            // 如果该僵尸的视觉对象已存在，则记录警告并返回，避免重复创建
            if (zombieVisuals.ContainsKey(zombieData.id))
            {
                UnityEngine.Debug.LogWarning($"[可视化系统] 尝试为已存在视觉对象的僵尸 (ID: {zombieData.id}) 重复创建，已忽略。");
                return;
            }
            
            // UnityEngine.Debug.Log($"[可视化系统] 开始创建僵尸视觉对象: 类型={zombieData.type}, ID={zombieData.id}, 位置={zombieData.position}");
            
            GameObject zombieGO = CreateZombieGameObject(zombieData); // 调用辅助方法创建GameObject和基础组件
            zombieVisuals[zombieData.id] = zombieGO; // 存入字典，ID与GameObject的映射
            
            var renderer = zombieGO.GetComponent<SpriteRenderer>(); // 获取SpriteRenderer组件
            if (renderer != null) zombieRenderers[zombieData.id] = renderer; // 存入渲染器字典
            
            // 设置初始位置 (Z轴通常为0，除非游戏有高度概念)
            zombieGO.transform.position = new Vector3(zombieData.position.x, zombieData.position.y, 0);
            
            // UnityEngine.Debug.Log($"[可视化系统] 僵尸GameObject '{zombieGO.name}' 创建完成。位置: {zombieGO.transform.position}, Sprite: {renderer?.sprite?.name ?? "无Sprite"}");
            
            PlaySpawnAnimation(zombieData.id); // 播放出生/生成动画
            StartIdleAnimation(zombieData.id); // 开始播放默认的待机动画
            
            // UnityEngine.Debug.Log($"[可视化系统] 增强僵尸视觉对象创建流程结束: {zombieData.type} ID: {zombieData.id}");
        }
        
        /// <summary>
        /// （辅助方法）实际创建僵尸的GameObject，并设置其Sprite、颜色、大小、血条和阴影等。
        /// </summary>
        /// <param name="zombieData">僵尸数据。</param>
        /// <returns>创建好的GameObject。</returns>
        private GameObject CreateZombieGameObject(ZombieData zombieData)
        {
            GameObject zombieGO = new GameObject($"Zombie_{zombieData.type}_{zombieData.id}"); // 创建并命名GameObject
            
            var spriteRenderer = zombieGO.AddComponent<SpriteRenderer>(); // 添加SpriteRenderer组件
            
            // UnityEngine.Debug.Log($"[可视化系统] 尝试为僵尸 {zombieData.type} (ID: {zombieData.id}) 设置图片。图片字典中是否存在该类型: {zombieSprites.ContainsKey(zombieData.type)}");
            
            // 尝试从已加载的资源中获取Sprite；如果失败，则使用程序化生成的默认Sprite
            if (zombieSprites.TryGetValue(zombieData.type, out Sprite sprite) && sprite != null)
            {
                spriteRenderer.sprite = sprite;
                // UnityEngine.Debug.Log($"[可视化系统] ✅ 成功为僵尸 {zombieData.type} (ID: {zombieData.id}) 设置了预加载的图片: {sprite.name}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[可视化系统] ⚠️ 僵尸类型 {zombieData.type} (ID: {zombieData.id}) 的图片资源未找到或为null，将创建默认程序化图片。");
                spriteRenderer.sprite = CreateDefaultZombieSprite(zombieData.type); // 创建并使用默认图片
            }
            
            spriteRenderer.sortingOrder = 15; // 设置渲染层级，确保僵尸在较高层显示
            // spriteRenderer.sortingLayerName = "Characters"; // 可选：分配到特定的Unity排序层
            
            // 应用该僵尸类型的视觉配置（大小、基础颜色等）
            if (visualConfigs.TryGetValue(zombieData.type, out var config))
            {
                zombieGO.transform.localScale = config.size;
                spriteRenderer.color = config.baseColor;
            }
            else // 如果没有特定配置，使用通用默认值
            {
                 zombieGO.transform.localScale = Vector3.one * 0.8f; // 默认大小
                 spriteRenderer.color = GetZombieTypeColor(zombieData.type); // 默认颜色
            }
            
            CreateHealthBar(zombieGO, zombieData); // 为僵尸创建并附加血条UI元素
            CreateZombieShadow(zombieGO);          // 为僵尸创建并附加简单的阴影效果
            
            return zombieGO;
        }
        
        /// <summary>
        /// （辅助方法）为指定的僵尸GameObject创建一个简单的阴影效果子对象。
        /// </summary>
        /// <param name="zombieGO">父对象，即僵尸的GameObject。</param>
        private void CreateZombieShadow(GameObject zombieGO)
        {
            GameObject shadowGO = new GameObject("Shadow"); // 创建名为"Shadow"的子对象
            shadowGO.transform.SetParent(zombieGO.transform); // 设置父对象
            // 将阴影放置在父对象下方略微偏移的位置，模拟投影效果
            shadowGO.transform.localPosition = new Vector3(0.1f, -0.1f, 0.1f); // Z值为0.1使其在父对象之后渲染（如果使用透视相机和Z排序）
            shadowGO.transform.localScale = new Vector3(0.8f, 0.3f, 1f); // 阴影通常比主体扁平一些
            
            var shadowRenderer = shadowGO.AddComponent<SpriteRenderer>();
            // 阴影使用与父对象相同的Sprite形状，但颜色为半透明黑色
            shadowRenderer.sprite = zombieGO.GetComponent<SpriteRenderer>()?.sprite; // 安全获取父对象的Sprite
            shadowRenderer.color = new Color(0, 0, 0, 0.3f); // 半透明黑色
            shadowRenderer.sortingOrder = zombieGO.GetComponent<SpriteRenderer>().sortingOrder - 1; // 确保阴影在主体下方渲染
        }
        
        /// <summary>
        /// （辅助方法）为指定的僵尸GameObject创建一个血条显示子对象。
        /// </summary>
        /// <param name="parentZombieGO">父对象，即僵尸的GameObject。</param>
        /// <param name="zombieData">僵尸数据，用于初始化血条（例如基于最大生命值）。</param>
        private void CreateHealthBar(GameObject parentZombieGO, ZombieData zombieData)
        {
            // 创建血条背景 (通常为红色或暗色)
            GameObject healthBarBackgroundGO = new GameObject("HealthBarBG");
            healthBarBackgroundGO.transform.SetParent(parentZombieGO.transform); // 设置父对象
            // 将血条背景放置在僵尸头顶上方固定位置
            healthBarBackgroundGO.transform.localPosition = new Vector3(0, visualConfigs[zombieData.type].size.y * 0.5f + 0.1f, 0); // 示例：在僵尸高度一半再往上0.1单位
            healthBarBackgroundGO.transform.localScale = Vector3.one; // 通常血条背景和前景使用相同局部缩放，通过Sprite尺寸控制

            var backgroundRenderer = healthBarBackgroundGO.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = CreateHealthBarSprite(new Color(0.2f,0,0,0.8f), 32, 4); // 创建一个深红色背景条，宽32高4像素
            backgroundRenderer.sortingOrder = parentZombieGO.GetComponent<SpriteRenderer>().sortingOrder + 1; // 血条背景在僵尸之上

            // 创建血条前景 (通常为绿色，长度随生命值变化)
            GameObject healthBarForegroundGO = new GameObject("HealthBar");
            healthBarForegroundGO.transform.SetParent(healthBarBackgroundGO.transform); // 父对象为血条背景，方便对齐
            healthBarForegroundGO.transform.localPosition = Vector3.zero; // 相对于背景居中
            healthBarForegroundGO.transform.localScale = Vector3.one; // 初始满血，长度与背景一致

            var foregroundRenderer = healthBarForegroundGO.AddComponent<SpriteRenderer>();
            foregroundRenderer.sprite = CreateHealthBarSprite(Color.green, 32, 4); // 创建一个绿色前景条
            foregroundRenderer.sortingOrder = backgroundRenderer.sortingOrder + 1; // 血条前景在背景之上
        }
        
        /// <summary>
        /// （辅助方法）程序化创建一个用于血条的纯色矩形Sprite。
        /// </summary>
        /// <param name="barColor">血条的颜色。</param>
        /// <param name="width">纹理宽度（像素）。</param>
        /// <param name="height">纹理高度（像素）。</param>
        /// <returns>创建的Sprite对象。</returns>
        private Sprite CreateHealthBarSprite(Color barColor, int width, int height)
        {
            Texture2D barTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; ++i) pixels[i] = barColor; // 所有像素填充为指定颜色
            barTexture.SetPixels(pixels);
            barTexture.Apply();
            return Sprite.Create(barTexture, new Rect(0, 0, width, height), new Vector2(0.0f, 0.5f), 32f); // Pivot在左侧中心，方便缩放宽度来显示血量
        }

        /// <summary>
        /// 根据最新的僵尸数据更新其视觉表现。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        /// <param name="zombieData">最新的僵尸数据。</param>
        public void UpdateZombieVisual(string zombieId, ZombieData zombieData)
        {
            if (!zombieVisuals.ContainsKey(zombieId)) return; // 如果视觉对象不存在，则不更新
                
            UpdateZombiePosition(zombieId, zombieData.position); // 更新位置（使用DOTween平滑移动）
            UpdateHealthBar(zombieId, zombieData.currentHealth / zombieData.maxHealth); // 更新血条显示
            UpdateZombieStateVisual(zombieId, zombieData.state); // 根据僵尸状态更新视觉表现（如颜色、动画）
        }
        
        /// <summary>
        /// （辅助方法）使用DOTween平滑更新僵尸GameObject的位置。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        /// <param name="targetPosition">僵尸的目标逻辑位置。</param>
        private void UpdateZombiePosition(string zombieId, Vector2 targetPosition)
        {
            if (!zombieVisuals.TryGetValue(zombieId, out var zombieGO)) return; // 获取GameObject，不存在则返回
                
            Vector3 currentVisualPos = zombieGO.transform.position;
            Vector3 targetVisualPos = new Vector3(targetPosition.x, targetPosition.y, currentVisualPos.z); //保持Z轴不变
            
            float distanceToTarget = Vector3.Distance(currentVisualPos, targetVisualPos);
            if (distanceToTarget < 0.01f) return; // 如果距离非常小，则认为已到达，无需移动以避免抖动
            
            // 如果已存在对此僵尸的移动动画，先终止它，以开始新的移动
            if (movementTweens.TryGetValue(zombieId, out Tween existingTween))
            {
                existingTween?.Kill(); // 终止旧的移动动画
            }
            
            // 获取该类型僵尸的视觉配置以确定移动速度等参数
            ZombieType type = GetZombieType(zombieId); // 需要一个方法从zombieId获取ZombieType
            ZombieVisualConfig visualConfig = visualConfigs.TryGetValue(type, out var cfg) ? cfg : visualConfigs[ZombieType.Walker]; // 默认用Walker配置

            // 计算移动动画的持续时间，基于距离和配置的移动速度
            // 注意：visualConfig.moveSpeed 此处可能指动画层面的速度因子，实际逻辑移动速度在ZombieData中
            float moveDuration = distanceToTarget / (visualConfig.moveSpeed * 3f); // 乘以一个系数调整动画速度，使其看起来自然
            moveDuration = Mathf.Clamp(moveDuration, 0.05f, 0.5f); // 限制动画时长，避免过快或过慢
            
            StopIdleAnimation(zombieId); // 在开始移动前停止待机动画
            
            var newMoveSequence = DOTween.Sequence(); // 创建一个新的动画序列
            
            // 创建平滑移动到目标位置的动画
            var moveTween = zombieGO.transform.DOMove(targetVisualPos, moveDuration).SetEase(Ease.Linear);
            newMoveSequence.Append(moveTween);
            
            // （可选）如果僵尸在移动，可以加入上下摆动的动画模拟行走
            var bobbingTween = zombieGO.transform.DOMoveY(targetVisualPos.y + walkBobAmplitude, 1f/walkBobSpeed) // walkBobSpeed是频率
                .SetLoops(-1, LoopType.Yoyo) // 无限循环播放Yoyo效果（上下）
                .SetEase(Ease.InOutSine);
            newMoveSequence.Join(bobbingTween); // 将摆动动画加入序列并行播放
            
            // 动画序列完成后的回调
            newMoveSequence.OnComplete(() => {
                movementTweens.Remove(zombieId); // 从字典中移除已完成的动画
                if (zombieGO.activeInHierarchy && GetZombieDataFromSystem(zombieId)?.state != ZombieState.Attacking) // 如果不是在攻击状态
                {
                    StartIdleAnimation(zombieId); // 重新开始待机动画
                }
                bobbingTween.Kill(); // 确保摆动动画被停止
            });
            
            movementTweens[zombieId] = newMoveSequence; // 存储新的移动动画序列
        }
        
        /// <summary>
        /// （辅助方法）开始指定僵尸的待机动画。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        private void StartIdleAnimation(string zombieId)
        {
            if (!zombieVisuals.TryGetValue(zombieId, out var zombieGO) || animationSequences.ContainsKey(zombieId))
                return; // GameObject不存在或已有其他动画序列在播放
                
            var sequence = DOTween.Sequence(); // 创建新的动画序列
            
            // 示例：轻微的上下浮动效果
            sequence.Append(zombieGO.transform.DOMoveY(zombieGO.transform.position.y + idleFloatAmplitude, 1f/idleFloatSpeed)
                .SetLoops(-1, LoopType.Yoyo) // 无限循环Yoyo
                .SetEase(Ease.InOutSine));
            
            // 示例：轻微的左右摇摆/旋转效果 (围绕Z轴)
            // sequence.Join(zombieGO.transform.DORotate(new Vector3(0, 0, 5f), 1.5f/idleFloatSpeed)
            //     .SetLoops(-1, LoopType.Yoyo)
            //     .SetEase(Ease.InOutSine));
            
            sequence.SetLoops(-1); // 整个序列无限循环
            animationSequences[zombieId] = sequence; // 存储此待机动画序列
        }
        
        /// <summary>
        /// （辅助方法）停止指定僵尸的待机动画。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        private void StopIdleAnimation(string zombieId)
        {
            if (animationSequences.TryGetValue(zombieId, out Sequence existingSequence))
            {
                existingSequence?.Kill(); // 终止动画序列
                animationSequences.Remove(zombieId); // 从字典中移除
            }
        }
        
        /// <summary>
        /// （辅助方法）更新僵尸血条的视觉显示（长度和颜色）。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        /// <param name="healthPercent">当前生命值百分比 (0到1)。</param>
        private void UpdateHealthBar(string zombieId, float healthPercent)
        {
            if (!zombieVisuals.TryGetValue(zombieId, out var zombieGO)) return;
                
            Transform healthBarTransform = zombieGO.transform.Find("HealthBarBG/HealthBar"); // 查找血条前景的Transform
            if (healthBarTransform != null)
            {
                // 使用DOTween平滑地调整血条前景的X轴缩放以反映生命值百分比
                healthBarTransform.DOScaleX(Mathf.Clamp01(healthPercent), 0.2f).SetEase(Ease.OutQuad);
                
                // 根据生命值百分比平滑地改变血条颜色 (从红到绿)
                var renderer = healthBarTransform.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Color targetColor = Color.Lerp(Color.red, Color.green, healthPercent); // 计算目标颜色
                    renderer.DOColor(targetColor, 0.2f); // 平滑过渡到目标颜色
                }
            }
        }
        
        /// <summary>
        /// （辅助方法）根据僵尸的当前逻辑状态更新其视觉表现（如颜色变化、特殊效果等）。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        /// <param name="state">僵尸当前的ZombieState。</param>
        private void UpdateZombieStateVisual(string zombieId, ZombieState state)
        {
            if (!zombieRenderers.TryGetValue(zombieId, out var renderer)) return; // 获取渲染器
            if (!zombieVisuals.TryGetValue(zombieId, out var zombieGO)) return; // 获取GameObject
            
            // 获取该僵尸类型的原始基础颜色
            ZombieType type = GetZombieType(zombieId);
            Color baseColor = visualConfigs.TryGetValue(type, out var cfg) ? cfg.baseColor : Color.white;

            switch (state) // 根据不同状态应用不同视觉效果
            {
                case ZombieState.Wandering: // 游荡状态
                    renderer.DOColor(baseColor, 0.3f); // 恢复基础颜色
                    // 确保停止任何攻击或特殊状态的动画
                    break;
                case ZombieState.Approaching: // 接近目标状态
                    renderer.DOColor(Color.Lerp(baseColor, Color.yellow, 0.5f), 0.3f); // 混合一点黄色，表示警觉/激活
                    // 可以添加轻微的加速或前冲姿态动画（如果未在UpdateZombiePosition中处理）
                    break;
                case ZombieState.Attacking: // 攻击状态
                    renderer.DOColor(Color.Lerp(baseColor, Color.red, 0.7f), 0.1f); // 混合较多红色，表示攻击性
                    // 攻击动画通常在此状态下由特定攻击事件触发 (OnZombieAttack)
                    break;
                case ZombieState.Dead: // 死亡状态
                    renderer.DOColor(new Color(0.3f,0.3f,0.3f,0.7f), 0.5f); // 变为深灰色半透明，模拟死亡/消散
                    // 死亡动画/特效已在OnZombieDied -> ShowDeathEffect中处理
                    break;
            }
        }
        
        /// <summary>
        /// 从场景中移除指定ID僵尸的视觉对象，并清理相关动画和引用。
        /// </summary>
        /// <param name="zombieId">要移除视觉的僵尸ID。</param>
        public void RemoveZombieVisual(string zombieId)
        {
            if (zombieVisuals.TryGetValue(zombieId, out var zombieGO)) // 检查视觉对象是否存在
            {
                // 停止并清理所有与此僵尸相关的DOTween动画
                movementTweens.TryGetValue(zombieId, out Tween moveTween);
                moveTween?.Kill(); // 终止移动动画
                movementTweens.Remove(zombieId); // 从字典中移除

                animationSequences.TryGetValue(zombieId, out Sequence animSeq);
                animSeq?.Kill(); // 终止其他动画序列
                animationSequences.Remove(zombieId);
                
                DOTween.Kill(zombieGO.transform); // 终止所有附加到此GameObject transform上的DOTween动画
                // 对于SpriteRenderer的颜色动画等，如果不是序列的一部分，也需要单独Kill或Kill(renderer)
                if(zombieRenderers.TryGetValue(zombieId, out var renderer))
                {
                    DOTween.Kill(renderer);
                    zombieRenderers.Remove(zombieId);
                }

                GameObject.Destroy(zombieGO); // 销毁Unity场景中的GameObject
                zombieVisuals.Remove(zombieId); // 从主视觉对象字典中移除
                
                UnityEngine.Debug.Log($"[可视化系统] 已移除僵尸视觉对象: {zombieId}");
            }
        }
        
        /// <summary>
        /// 定期更新所有当前活动僵尸的视觉表现。
        /// </summary>
        public void UpdateAllZombieVisuals()
        {
            // 通过时间间隔控制此方法的执行频率，以优化性能
            if (Time.time - lastVisualUpdate < VISUAL_UPDATE_INTERVAL)
                return;
                
            var zombieSystemData = zombieSystem.GetZombieData(); // 获取所有僵尸数据
            if (zombieSystemData == null || zombieSystemData.zombies == null) return;
            
            foreach (var kvp in zombieSystemData.zombies) // 遍历所有僵尸
            {
                string zombieId = kvp.Key;
                var zombie = kvp.Value;
                
                if (zombie.IsAlive) // 只更新存活的僵尸
                {
                    UpdateZombieVisual(zombieId, zombie); // 调用单个僵尸的视觉更新方法
                }
            }
            lastVisualUpdate = Time.time; // 更新上次执行时间戳
        }
        
        /// <summary>
        /// 根据僵尸ID获取其在场景中的GameObject。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        /// <returns>对应的GameObject，如果不存在则为null。</returns>
        public GameObject GetZombieVisual(string zombieId)
        {
            return zombieVisuals.TryGetValue(zombieId, out var visual) ? visual : null;
        }
        
        /// <summary>
        /// （辅助方法）播放僵尸生成时的动画效果。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        private void PlaySpawnAnimation(string zombieId)
        {
            if (!zombieVisuals.TryGetValue(zombieId, out var zombieGO)) return;
            if (!zombieRenderers.TryGetValue(zombieId, out var renderer)) return;
            if (!visualConfigs.TryGetValue(GetZombieType(zombieId), out var config)) return;

            // UnityEngine.Debug.Log($"[可视化系统] 开始为僵尸 {zombieId} 播放生成动画。");

            zombieGO.transform.localScale = Vector3.zero; // 初始大小为0
            // 确保Sprite在动画开始前是不透明的，如果后续有淡入效果则调整
            renderer.color = new Color(config.baseColor.r, config.baseColor.g, config.baseColor.b, 1f);

            var spawnSequence = DOTween.Sequence();
            // 动画1: 从小到大出现 (弹性效果)
            spawnSequence.Append(zombieGO.transform.DOScale(config.size, 0.5f).SetEase(Ease.OutBack));
            // 动画2: （可选）出现后轻微抖动一下
            spawnSequence.AppendCallback(() => {
                if (zombieGO != null) zombieGO.transform.DOShakeScale(0.3f, 0.1f, 5, 90, true);
                // UnityEngine.Debug.Log($"[可视化系统] 僵尸 {zombieId} 生成动画播放完成。");
            });
        }
        
        /// <summary>
        /// （辅助方法）播放僵尸攻击时的动画效果。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        private void PlayAttackAnimation(string zombieId)
        {
            if (!zombieVisuals.TryGetValue(zombieId, out var zombieGO)) return;
            if (!zombieRenderers.TryGetValue(zombieId, out var renderer)) return;
            
            var originalScale = zombieGO.transform.localScale; // 保存原始大小
            var originalColor = renderer.color; // 保存原始颜色 (或应为visualConfig中的baseColor)
            
            var attackSequence = DOTween.Sequence();
            
            // 步骤1: 短暂向前“冲刺”或“伸出”的动作
            // 假设僵尸面朝右方，x轴正方向为前。实际应根据僵尸朝向调整。
            attackSequence.Append(zombieGO.transform.DOMoveX(zombieGO.transform.position.x + 0.2f, 0.1f)
                .SetEase(Ease.OutQuad));
            // 步骤2: 攻击时身体略微放大并变为警示色（如红色）
            attackSequence.Join(zombieGO.transform.DOScale(originalScale * 1.1f, 0.1f)); // 放大10%
            attackSequence.Join(renderer.DOColor(Color.Lerp(originalColor, Color.red, 0.7f), 0.1f)); // 混合红色
            // 步骤3: 攻击后快速收回动作，恢复原状
            attackSequence.Append(zombieGO.transform.DOMoveX(zombieGO.transform.position.x, 0.15f) // 恢复X位置 (DOMoveX的参数应为绝对值)
                .SetEase(Ease.OutBounce)); // 使用弹性效果收回
            attackSequence.Join(zombieGO.transform.DOScale(originalScale, 0.15f)); // 恢复原始大小
            attackSequence.Join(renderer.DOColor(originalColor, 0.15f)); // 恢复原始颜色
            // 步骤4: （可选）攻击完成后轻微抖动一下身体
            attackSequence.AppendCallback(() => {
                if (zombieGO != null) zombieGO.transform.DOShakeRotation(0.2f, new Vector3(0,0,15f), 5, 90, true); // 轻微Z轴抖动
            });
        }
        
        /// <summary>
        /// （辅助方法）播放僵尸受伤时的动画效果。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        private void PlayDamageAnimation(string zombieId)
        {
            if (!zombieRenderers.TryGetValue(zombieId, out var renderer)) return;
            if (!zombieVisuals.TryGetValue(zombieId, out var zombieGO)) return;
                
            Color baseColor = visualConfigs.TryGetValue(GetZombieType(zombieId), out var cfg) ? cfg.baseColor : Color.white;

            var damageSequence = DOTween.Sequence();
            // 效果1: 短暂闪烁红色
            damageSequence.Append(renderer.DOColor(Color.red, 0.08f));
            damageSequence.Append(renderer.DOColor(baseColor, 0.08f)); // 恢复基础色
            // 效果2: 轻微向后震退一点 (基于僵尸当前朝向，此处简化为X轴)
            // float knockbackDirection = zombieGO.transform.localScale.x > 0 ? -1f : 1f; // 假设localScale.x控制朝向
            // damageSequence.Join(zombieGO.transform.DOMoveX(zombieGO.transform.position.x + knockbackDirection * 0.1f, 0.1f)
            //     .SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad)); // 来回震一下
            // 效果3: （可选）身体轻微抖动
            damageSequence.AppendCallback(() => {
                if (zombieGO != null) zombieGO.transform.DOShakePosition(0.15f, strength: 0.05f, vibrato: 8, fadeOut:false);
            });
        }
        
        /// <summary>
        /// （辅助方法）播放僵尸死亡时的动画和特效。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        private void PlayDeathAnimation(string zombieId)
        {
            if (!zombieVisuals.TryGetValue(zombieId, out var zombieGO)) return;
            if (!zombieRenderers.TryGetValue(zombieId, out var renderer)) return;
                
            StopIdleAnimation(zombieId); // 停止所有正在进行的待机动画
            movementTweens.TryGetValue(zombieId, out Tween moveTween); // 停止移动动画
            moveTween?.Kill();
            movementTweens.Remove(zombieId);

            var deathSequence = DOTween.Sequence();
            // 动画1: （可选）向后倒地或旋转效果
            deathSequence.Append(zombieGO.transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(70f, 110f) * (UnityEngine.Random.value > 0.5f ? 1: -1) ), 0.4f)
                .SetEase(Ease.OutBounce));
            // 动画2: 逐渐缩小并淡出直至消失
            deathSequence.Join(zombieGO.transform.DOScale(Vector3.one * 0.3f, 0.8f).SetDelay(0.2f) // 延迟0.2秒后开始缩小
                .SetEase(Ease.InSine));
            deathSequence.Join(renderer.DOFade(0f, 0.8f).SetDelay(0.2f).SetEase(Ease.InSine)); // 同时淡出
            
            // 血条和阴影也应随之淡出或立即隐藏
            var healthBarBG = zombieGO.transform.Find("HealthBarBG");
            if (healthBarBG != null)
            {
                var bgRenderer = healthBarBG.GetComponent<SpriteRenderer>();
                if(bgRenderer != null) deathSequence.Join(bgRenderer.DOFade(0f, 0.5f));
                var healthBar = healthBarBG.Find("HealthBar");
                if (healthBar != null)
                {
                    var fgRenderer = healthBar.GetComponent<SpriteRenderer>();
                    if(fgRenderer != null) deathSequence.Join(fgRenderer.DOFade(0f, 0.5f));
                }
            }
            var shadow = zombieGO.transform.Find("Shadow");
            if (shadow != null)
            {
                var shadowRenderer = shadow.GetComponent<SpriteRenderer>();
                if(shadowRenderer != null) deathSequence.Join(shadowRenderer.DOFade(0f, 0.8f));
            }
            // 动画序列完成后（不是立即），ZombieSystem会通过事件调用RemoveZombieVisual来销毁对象
        }
        
        /// <summary>
        /// 显示僵尸受伤的视觉效果（如播放受伤动画、显示伤害数字）。
        /// </summary>
        /// <param name="zombieId">受伤僵尸的ID。</param>
        /// <param name="damage">受到的伤害值。</param>
        public void ShowDamageEffect(string zombieId, float damage)
        {
            PlayDamageAnimation(zombieId); // 播放受伤动画（例如闪烁、震退）
            
            if (zombieVisuals.TryGetValue(zombieId, out var zombieGO)) // 如果视觉对象存在
            {
                CreateDamageText(zombieGO.transform.position, damage); // 在其位置创建伤害数字特效
            }
        }
        
        /// <summary>
        /// 显示僵尸死亡的视觉效果（如播放死亡动画、粒子效果等）。
        /// </summary>
        /// <param name="zombieId">死亡僵尸的ID。</param>
        public void ShowDeathEffect(string zombieId)
        {
            PlayDeathAnimation(zombieId); // 播放死亡动画（例如倒下、消失）
            
            if (zombieVisuals.TryGetValue(zombieId, out var zombieGO)) // 如果视觉对象存在
            {
                CreateDeathEffect(zombieGO.transform.position); // 在其位置创建死亡粒子特效
            }
        }
        
        /// <summary>
        /// （辅助方法）在指定位置创建一个显示伤害数值的漂浮文字特效。
        /// </summary>
        /// <param name="position">特效的生成位置。</param>
        /// <param name="damage">要显示的伤害数值。</param>
        private void CreateDamageText(Vector3 position, float damage)
        {
            GameObject textGO = new GameObject("DamageTextEffect"); // 创建GameObject
            textGO.transform.position = position + Vector3.up * 0.8f; // 在目标上方0.8米处显示

            var textMesh = textGO.AddComponent<TextMesh>(); // 添加TextMesh组件
            textMesh.text = "-" + damage.ToString("F0"); // 显示伤害值，格式为 "-XX"
            textMesh.fontSize = 24; // 字体大小
            textMesh.color = Color.red; // 伤害数字通常为红色
            textMesh.anchor = TextAnchor.MiddleCenter; // 文本居中
            textMesh.fontStyle = FontStyle.Bold; // 加粗
            // 注意: TextMesh依赖字体资源，若项目中无默认字体或配置不当可能不显示。建议使用TextMeshPro。

            var textSequence = DOTween.Sequence(); // 创建DOTween动画序列
            // 动画1: 向上漂浮
            textSequence.Append(textGO.transform.DOMoveY(position.y + 1.8f, 1f) // 1秒内向上移动1米 (总共在原目标上方1.8米处)
                .SetEase(Ease.OutQuad)); // 使用先快后慢的缓动
            // 动画2: 初始放大再略微缩小 (弹出效果)
            textGO.transform.localScale = Vector3.zero; // 初始大小为0
            textSequence.Join(textGO.transform.DOScale(Vector3.one * 0.05f, 0.2f) // 0.2秒放大到0.05倍大小（根据场景调整）
                .SetEase(Ease.OutBack));
            // 动画3: 逐渐淡出并消失
            textSequence.Join(DOTween.ToAlpha(() => textMesh.color, c => textMesh.color = c, 0f, 0.8f).SetDelay(0.2f)); // 延迟0.2秒后，0.8秒内淡出
            
            textSequence.OnComplete(() => GameObject.Destroy(textGO)); // 动画序列完成后销毁该文本对象
        }
        
        /// <summary>
        /// （辅助方法）在指定位置创建一个简单的死亡粒子效果。
        /// </summary>
        /// <param name="position">特效的生成中心位置。</param>
        private void CreateDeathEffect(Vector3 position)
        {
            // 示例：创建5个简单的红色方块作为粒子，向随机方向飞散并淡出
            for (int i = 0; i < 5; i++)
            {
                GameObject particleGO = new GameObject("DeathParticleEffect");
                particleGO.transform.position = position; // 粒子初始位置
                
                var renderer = particleGO.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateParticleSprite(); // 创建一个小的红色方形Sprite
                renderer.color = new Color(0.5f + UnityEngine.Random.Range(-0.1f,0.1f), 0, 0, 1f); // 深红色略带随机
                renderer.sortingOrder = 5; // 渲染层级
                
                // 随机一个2D方向作为飞散方向
                Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
                Vector3 flyDirection = new Vector3(randomDir.x, randomDir.y, 0);
                
                var particleSequence = DOTween.Sequence();
                // 动画1: 向随机方向飞散
                particleSequence.Append(particleGO.transform.DOMove(position + flyDirection * UnityEngine.Random.Range(0.5f, 1.5f), 0.8f)
                    .SetEase(Ease.OutQuad));
                // 动画2: （可选）随机旋转
                particleSequence.Join(particleGO.transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-180f,180f)), 0.8f, RotateMode.FastBeyond360));
                // 动画3: 逐渐淡出
                particleSequence.Join(renderer.DOFade(0f, 0.8f).SetEase(Ease.InQuad));
                
                particleSequence.OnComplete(() => GameObject.Destroy(particleGO)); // 完成后销毁粒子
            }
        }
        
        /// <summary>
        /// （辅助方法）创建一个用于死亡粒子效果的简单红色方形Sprite。
        /// </summary>
        private Sprite CreateParticleSprite()
        {
            int particleSize = 4; // 粒子Sprite的边长（像素）
            Texture2D particleTexture = new Texture2D(particleSize, particleSize, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[particleSize * particleSize];
            for (int i = 0; i < pixels.Length; ++i) pixels[i] = new Color(0.7f,0.1f,0.1f); // 深红色
            particleTexture.SetPixels(pixels);
            particleTexture.Apply();
            return Sprite.Create(particleTexture, new Rect(0, 0, particleSize, particleSize), new Vector2(0.5f, 0.5f), 100f);
        }
        
        /// <summary>
        /// （辅助方法）根据僵尸ID从基础僵尸系统中获取其类型。
        /// </summary>
        /// <param name="zombieId">僵尸ID。</param>
        /// <returns>该僵尸的ZombieType；如果找不到，则返回默认类型 (Walker)。</returns>
        private ZombieType GetZombieType(string zombieId)
        {
            var zombieDataDict = zombieSystem.GetZombieData()?.zombies; // 安全获取僵尸数据字典
            if (zombieDataDict != null && zombieDataDict.TryGetValue(zombieId, out ZombieData data))
            {
                return data.type; // 返回找到的僵尸类型
            }
            UnityEngine.Debug.LogWarning($"[可视化系统] 获取僵尸类型失败：在ZombieSystem中找不到ID为 {zombieId} 的僵尸。将使用默认类型 Walker。");
            return ZombieType.Walker; // 未找到或数据无效时，返回默认类型
        }
    }
    
    /// <summary>
    /// 僵尸视觉配置类。
    /// 用于存储不同类型僵尸的特定视觉参数，如基础颜色、大小、动画速度等。
    /// </summary>
    [System.Serializable] // 标记为可序列化，如果需要在Unity Inspector中编辑或保存
    public class ZombieVisualConfig
    {
        /// <summary>僵尸的基础色调。</summary>
        public Color baseColor;
        /// <summary>僵尸视觉对象的相对大小（缩放因子）。</summary>
        public Vector3 size;
        /// <summary>用于动画计算的移动速度参考值（可能影响动画播放速率或效果强度）。</summary>
        public float moveSpeed;
        /// <summary>僵尸在移动或待机时上下摆动（bobbing）的强度/幅度。</summary>
        public float bobIntensity;
        /// <summary>（未使用）僵尸可能的旋转速度。</summary>
        public float rotationSpeed;
        
        /// <summary>
        /// ZombieVisualConfig的默认构造函数。
        /// 初始化所有成员为通用默认值。
        /// </summary>
        public ZombieVisualConfig()
        {
            baseColor = Color.gray; // 默认基础颜色为灰色
            size = Vector3.one;     // 默认大小为 (1,1,1)
            moveSpeed = 1f;         // 默认移动速度因子为1
            bobIntensity = 1f;      // 默认摆动强度为1
            rotationSpeed = 30f;    // 默认旋转速度为30度/秒
        }
    }
} 