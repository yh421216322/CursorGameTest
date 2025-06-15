// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieImageTest.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 ZombieImageTest 的 MonoBehaviour 类。
//     该脚本是一个调试工具，用于专门测试僵尸系统是否能够正确加载和应用
//     预期的Sprite图片资源到僵尸的视觉对象上。它包含检查资源加载、
//     生成测试僵尸、检查生成的僵尸视觉效果，并在必要时尝试手动修复Sprite的功能。
// ==============================================================================

using UnityEngine;
using QFramework; // QFramework框架，用于获取系统实例
using SurvivalGame.GameSystem; // 包含IZombieSystem等游戏系统接口
using SurvivalGame.Visualization; // 包含IZombieVisualizationSystem可视化系统接口
using SurvivalGame.Model;         // 包含ZombieType等模型定义
using MyGameNamespace;    // 自定义命名空间 (当前在此文件中可能未使用特定内容)

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸图片应用测试脚本。
    /// 专门用于测试和验证僵尸系统在创建僵尸时是否能正确地为其视觉对象加载和应用预期的Sprite图片资源。
    /// 提供快捷键和IMGUI按钮来触发测试流程。
    /// </summary>
    public class ZombieImageTest : MonoBehaviour
    {
        [Header("测试设置")] // Inspector中分组显示
        public KeyCode testKey = KeyCode.I; // 手动触发图片应用测试的快捷键，默认为 I 键 (避免与其他测试脚本冲突)
        
        // 系统接口引用
        private IZombieSystem zombieSystem;                 // 僵尸系统实例
        private IZombieVisualizationSystem visualSystem; // 僵尸可视化系统实例
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 初始化时，记录日志并尝试获取所需的游戏系统实例。
        /// </summary>
        void Start()
        {
            UnityEngine.Debug.Log("=== [僵尸图片测试] 脚本初始化 ===");
            InitializeSystems(); // 调用方法初始化系统引用
        }
        
        /// <summary>
        /// 初始化所需的系统引用（僵尸系统和可视化系统）。
        /// 获取结果会输出到控制台。
        /// </summary>
        void InitializeSystems()
        {
            try
            {
                // 通过QFramework的RegisterManager获取IZombieSystem的实例
                zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
                // 获取IZombieVisualizationSystem的实例
                visualSystem = RegisterManager.Interface.GetSystem<IZombieVisualizationSystem>();

                if (zombieSystem != null && visualSystem != null)
                {
                    UnityEngine.Debug.Log("  [僵尸图片测试] ✅ 僵尸系统和可视化系统均获取成功。");
                }
                else
                {
                    if(zombieSystem == null) UnityEngine.Debug.LogError("  [僵尸图片测试] ❌ 僵尸系统获取失败：返回null。");
                    if(visualSystem == null) UnityEngine.Debug.LogError("  [僵尸图片测试] ❌ 可视化系统获取失败：返回null。");
                }
            }
            catch (System.Exception e) // 捕获获取过程中可能发生的异常
            {
                UnityEngine.Debug.LogError($"  [僵尸图片测试] ❌ 系统获取时发生异常: {e.Message}");
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 监听用户按下的快捷键，以触发僵尸图片应用测试。
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(testKey)) // 如果按下了测试快捷键
            {
                TestZombieImageApplication(); // 执行测试流程
            }
        }
        
        /// <summary>
        /// 执行僵尸图片应用的完整测试流程。
        /// 包括：测试图片资源加载、生成测试僵尸、检查已创建僵尸的视觉效果。
        /// </summary>
        [ContextMenu("执行僵尸图片应用测试")] // 使此方法可在Inspector中通过右键菜单调用
        void TestZombieImageApplication()
        {
            UnityEngine.Debug.Log("=== [僵尸图片测试] 开始执行 ===");
            
            // 检查核心系统是否已成功初始化
            if (zombieSystem == null || visualSystem == null)
            {
                UnityEngine.Debug.LogError("  ❌ [僵尸图片测试] 无法执行：一个或多个核心系统未初始化。请检查Start方法中的日志。");
                return;
            }
            
            // 步骤1: 测试预期的Sprite图片资源本身是否能加载成功
            TestImageResources();
            
            // 步骤2: 生成测试僵尸，并监控其创建过程（尤其是视觉对象的创建）
            TestZombieCreation();
            
            // 步骤3: 检查当前场景中已存在的僵尸对象，确认其视觉表现是否正确
            // （此步骤可能与步骤2的结果检查有重叠，但可用于检查非本脚本生成的僵尸）
            CheckExistingZombies();
            UnityEngine.Debug.Log("=== [僵尸图片测试] 执行完毕 ===");
        }
        
        /// <summary>
        /// 测试预定义的Sprite图片资源是否能从 "Resources/Image/" 目录成功加载。
        /// </summary>
        void TestImageResources()
        {
            UnityEngine.Debug.Log("  --- [图片资源加载测试] ---");
            
            string[] spriteNames = { "zb1", "zb2", "zb3", "zb4" }; // 假设这些是预期的僵尸Sprite名称
            int loadedCount = 0;
            foreach (string spriteName in spriteNames)
            {
                Sprite sprite = Resources.Load<Sprite>($"Image/{spriteName}"); // 尝试加载
                if (sprite != null)
                {
                    UnityEngine.Debug.Log($"    ✅ 图片资源 '{spriteName}' 加载成功。名称: {sprite.name}, 尺寸: {sprite.rect.width}x{sprite.rect.height}");
                    loadedCount++;
                }
                else
                {
                    UnityEngine.Debug.LogError($"    ❌ 图片资源 'Image/{spriteName}' 加载失败！请确保资源存在于正确路径且已导入为Sprite。");
                }
            }
            UnityEngine.Debug.Log($"  --- 图片资源加载测试完毕: {loadedCount}/{spriteNames.Length} 个资源成功加载 ---");
        }
        
        /// <summary>
        /// 测试僵尸的创建过程，特别关注其视觉对象的生成和Sprite的初步应用。
        /// </summary>
        void TestZombieCreation()
        {
            UnityEngine.Debug.Log("  --- [僵尸创建过程监控] ---");
            
            ClearTestZombies(); // 清理之前可能存在的测试僵尸，确保测试环境干净
            
            Vector2 testPosition = Vector2.zero; // 在原点生成测试僵尸
            UnityEngine.Debug.Log($"    🎯 准备在位置 {testPosition} 创建一个Walker类型的测试僵尸。");
            
            try
            {
                zombieSystem.SpawnZombie(ZombieType.Walker, testPosition); // 发送生成僵尸命令
                UnityEngine.Debug.Log("    ✅ 僵尸生成命令已发送。可视化系统应已处理其视觉创建。请观察控制台后续的创建结果日志。");
                
                // 延迟1秒后调用CheckCreationResult，以便系统有时间处理僵尸的实际创建和可视化
                Invoke("CheckCreationResult", 1f);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"    ❌ 发送僵尸生成命令时发生错误: {e.Message}");
            }
        }
        
        /// <summary>
        /// 检查并记录最近一次僵尸创建操作的结果，特别是关注其视觉对象的Sprite信息。
        /// </summary>
        void CheckCreationResult()
        {
            UnityEngine.Debug.Log("  --- [僵尸创建结果检查] ---");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>(); // 获取场景中所有对象
            bool foundTestZombie = false;
            foreach (var obj in allObjects)
            {
                // 通过名称（或其他特定组件/标签）识别由测试生成的僵尸
                // 此处简单用名称包含"zombie"判断，实际项目中可能需要更精确的识别方式
                if (obj.name.ToLower().Contains("zombie"))
                {
                    foundTestZombie = true;
                    var renderer = obj.GetComponent<SpriteRenderer>(); // 获取其SpriteRenderer
                    if (renderer != null)
                    {
                        string spriteName = renderer.sprite?.name ?? "无Sprite"; // 安全获取Sprite名称
                        UnityEngine.Debug.Log($"    🧟 发现场景中的僵尸对象: '{obj.name}'");
                        UnityEngine.Debug.Log($"       位置: {obj.transform.position.ToString("F1")}");
                        UnityEngine.Debug.Log($"       Sprite名称: '{spriteName}'");
                        UnityEngine.Debug.Log($"       Sprite颜色: {renderer.color}");
                        UnityEngine.Debug.Log($"       渲染层排序: {renderer.sortingOrder}");
                        UnityEngine.Debug.Log($"       是否可见 (Renderer.enabled): {renderer.enabled}");
                        
                        // 检查是否使用了默认生成的Sprite（通常表示真实Sprite未被正确应用）
                        if (spriteName.Contains("ZombieSprite_")) // "ZombieSprite_" 是可视化系统中默认Sprite的命名模式
                        {
                            UnityEngine.Debug.LogWarning($"    ⚠️ 注意: 僵尸 '{obj.name}' 可能正在使用默认生成的Sprite ('{spriteName}')。尝试手动修复...");
                            TryFixZombieSprite(obj, renderer); // 尝试用预期的Sprite进行修复
                        }
                        else if (spriteName == "无Sprite")
                        {
                             UnityEngine.Debug.LogError($"    ❌ 错误: 僵尸 '{obj.name}' 的SpriteRenderer上没有分配Sprite！");
                        }
                        else
                        {
                             UnityEngine.Debug.Log($"    ✅ 僵尸 '{obj.name}' 看上去使用了预期的Sprite ('{spriteName}')。");
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"    ⚠️ 僵尸对象 '{obj.name}' 缺少SpriteRenderer组件。");
                    }
                }
            }
            if(!foundTestZombie) UnityEngine.Debug.Log("    ℹ️ 未在场景中找到符合条件的僵尸对象进行创建结果检查。");
            UnityEngine.Debug.Log("  --- 创建结果检查完毕 ---");
        }
        
        /// <summary>
        /// 尝试手动为一个被检测到使用默认Sprite的僵尸对象加载并应用一个预期的真实Sprite。
        /// （此方法主要用于调试，展示如果自动应用失败，手动加载是否可行）
        /// </summary>
        /// <param name="zombieObj">目标僵尸的GameObject。</param>
        /// <param name="renderer">目标僵尸的SpriteRenderer组件。</param>
        void TryFixZombieSprite(GameObject zombieObj, SpriteRenderer renderer)
        {
            // 尝试加载一个预期的真实僵尸Sprite (例如 "zb1")
            Sprite realSprite = Resources.Load<Sprite>("Image/zb1");
            if (realSprite != null)
            {
                renderer.sprite = realSprite; // 应用加载到的Sprite
                UnityEngine.Debug.Log($"      ✅ [手动修复尝试] 成功将僵尸 '{zombieObj.name}' 的Sprite替换为 '{realSprite.name}'。");
            }
            else
            {
                UnityEngine.Debug.LogError($"      ❌ [手动修复尝试] 无法加载预期的修复用Sprite (例: 'Image/zb1')。");
            }
        }
        
        /// <summary>
        /// 检查当前已存在于僵尸系统中的所有僵尸数据，并验证其对应的视觉对象和Sprite。
        /// </summary>
        void CheckExistingZombies()
        {
            UnityEngine.Debug.Log("  --- [检查现有僵尸状态与视觉对象] ---");
            
            if (zombieSystem != null && visualSystem != null)
            {
                try
                {
                    var zombieDataContainer = zombieSystem.GetZombieData();
                    UnityEngine.Debug.Log($"    数据模型中僵尸总数: {zombieDataContainer.zombies.Count}");
                    
                    // 遍历数据模型中的每个僵尸
                    foreach (var zombieEntry in zombieDataContainer.zombies) // zombies是Dictionary<string, ZombieData>
                    {
                        ZombieData zombie = zombieEntry.Value;
                        UnityEngine.Debug.Log($"    数据僵尸: ID='{zombie.id}', 类型='{zombie.type}', 位置={zombie.position}");
                        
                        // 尝试从可视化系统获取该僵尸ID对应的视觉GameObject
                        var visualObj = visualSystem.GetZombieVisual(zombie.id);
                        if (visualObj != null) // 如果找到了视觉对象
                        {
                            var renderer = visualObj.GetComponent<SpriteRenderer>();
                            UnityEngine.Debug.Log($"      视觉对象: '{visualObj.name}', Sprite: '{(renderer?.sprite?.name ?? "无Sprite")}'");
                        }
                        else // 如果未找到视觉对象
                        {
                            UnityEngine.Debug.LogWarning($"      ⚠️ 警告: 僵尸ID '{zombie.id}' 在可视化系统中没有对应的视觉对象！");
                        }
                    }
                }
                catch (System.Exception e) // 捕获检查过程中可能发生的异常
                {
                    UnityEngine.Debug.LogError($"    ❌ 检查现有僵尸数据时发生错误: {e.Message}");
                }
            }
            else
            {
                 UnityEngine.Debug.LogWarning("    ⚠️ 僵尸系统或可视化系统未初始化，无法检查现有僵尸。");
            }
            UnityEngine.Debug.Log("  --- 现有僵尸检查完毕 ---");
        }
        
        /// <summary>
        /// 清理场景中所有名称包含 "zombie", "TestSprite", 或 "TestSquare" (不区分大小写) 的GameObject。
        /// </summary>
        [ContextMenu("清理所有测试相关对象")]
        void ClearTestZombies()
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>(); // 获取场景中所有对象
            int clearedCount = 0;
            
            UnityEngine.Debug.Log("  🧹 开始清理场景中的测试对象 (僵尸、TestSprite、TestSquare)...");
            foreach (var obj in allObjects)
            {
                string objNameLower = obj.name.ToLower();
                if (objNameLower.Contains("zombie") ||
                    objNameLower.Contains("testsprite") ||
                    objNameLower.Contains("testsquare"))
                {
                    UnityEngine.Debug.Log($"    清理对象: {obj.name}");
                    DestroyImmediate(obj); // 立即销毁，用于调试和编辑器环境
                    clearedCount++;
                }
            }
            UnityEngine.Debug.Log($"  🧹 清理完成，共移除了 {clearedCount} 个测试相关对象。");
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕左下角显示一个简单的测试控制面板。
        /// </summary>
        void OnGUI()
        {
            // 定义GUI区域在屏幕左下角
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 300, 100), "", GUI.skin.box);
            GUILayout.BeginVertical();
            
            // 面板标题
            GUILayout.Label("僵尸图片应用测试工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            
            // 功能按钮，并显示其对应的快捷键
            if (GUILayout.Button($"执行图片应用测试 ({testKey})"))
            {
                TestZombieImageApplication();
            }
            
            if (GUILayout.Button("清理所有测试对象 (参考用)")) // 此按钮不直接绑定快捷键，提示用户使用Delete等键
            {
                ClearTestZombies();
            }
            
            GUILayout.EndVertical(); // 结束垂直布局
            GUILayout.EndArea();   // 结束IMGUI区域
        }
    }
}