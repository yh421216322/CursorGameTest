// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieDebugHelper.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 ZombieDebugHelper 的 MonoBehaviour 类。
//     该脚本是一个调试辅助工具，主要用于在开发过程中诊断与僵尸相关的显示问题。
//     它提供了通过快捷键或IMGUI按钮来生成测试僵尸、清理场景中的僵尸对象，
//     并显示僵尸系统状态、统计数据以及场景中僵尸对象的详细信息。
// ==============================================================================

using UnityEngine;
using QFramework; // QFramework框架，用于获取系统实例
using SurvivalGame.GameSystem; // 包含IZombieSystem等游戏系统接口
using SurvivalGame.Visualization; // 包含IZombieVisualizationSystem等可视化系统接口
using SurvivalGame.Model;         // 包含ZombieType等模型定义
using MyGameNamespace;    // 自定义命名空间 (当前在此文件中可能未使用特定内容)

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸调试助手脚本。
    /// 提供一系列调试功能，用于在运行时诊断僵尸的生成、显示及系统状态问题。
    /// 包括通过快捷键或IMGUI界面生成测试僵尸、清理场景中的僵尸对象，
    /// 以及显示僵尸系统、可视化系统、僵尸统计和场景中具体僵尸对象的信息。
    /// </summary>
    public class ZombieDebugHelper : MonoBehaviour
    {
        [Header("调试选项")] // Inspector中分组显示
        public bool enableDebugGUI = true;      // 是否在屏幕上显示IMGUI调试面板
        public bool autoSpawnTestZombie = false; // 是否在启动时自动生成一个测试僵尸 (原默认为false)
        public KeyCode spawnKey = KeyCode.J;     // 生成测试僵尸的快捷键 (原默认为J)
        public KeyCode clearKey = KeyCode.K;     // 清理所有僵尸的快捷键 (原默认为K)
        
        // 系统接口引用
        private IZombieSystem zombieSystem;                 // 僵尸系统实例
        private IZombieVisualizationSystem visualizationSystem; // 僵尸可视化系统实例
        private bool hasSpawnedTestZombie = false; // 标记是否已自动生成过测试僵尸，防止重复生成
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 初始化时，会尝试获取所需的游戏系统实例。
        /// </summary>
        void Start()
        {
            // 获取系统引用
            try
            {
                zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
                UnityEngine.Debug.Log("[僵尸调试助手] 僵尸系统 (IZombieSystem) 获取成功。");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[僵尸调试助手] 获取僵尸系统 (IZombieSystem) 失败: {e.Message}");
            }
            
            try
            {
                visualizationSystem = RegisterManager.Interface.GetSystem<IZombieVisualizationSystem>();
                UnityEngine.Debug.Log("[僵尸调试助手] 僵尸可视化系统 (IZombieVisualizationSystem) 获取成功。");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[僵尸调试助手] 获取僵尸可视化系统 (IZombieVisualizationSystem) 失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 处理用户输入（快捷键）以及可能的自动生成测试僵尸逻辑。
        /// </summary>
        void Update()
        {
            // 处理快捷键输入
            if (Input.GetKeyDown(spawnKey)) // 如果按下了生成僵尸的快捷键
            {
                SpawnTestZombie(); // 调用生成测试僵尸的方法
            }
            
            if (Input.GetKeyDown(clearKey)) // 如果按下了清理僵尸的快捷键
            {
                ClearAllZombies(); // 调用清理所有僵尸的方法
            }
            
            // 如果设置了自动生成测试僵尸，并且尚未生成过，且僵尸系统已成功初始化
            if (autoSpawnTestZombie && !hasSpawnedTestZombie && zombieSystem != null)
            {
                SpawnTestZombie(); // 生成一个测试僵尸
                hasSpawnedTestZombie = true; // 标记为已生成，避免重复
            }
        }
        
        /// <summary>
        /// 生成一个测试用的Walker类型僵尸。
        /// 僵尸会生成在主相机前方5个单位的位置（如果在2D平面，Z轴设为0）。
        /// 如果主相机未找到，则在世界原点(0,0)生成。
        /// </summary>
        void SpawnTestZombie()
        {
            if (zombieSystem == null) // 检查僵尸系统是否已初始化
            {
                UnityEngine.Debug.LogError("[僵尸调试助手] 生成测试僵尸失败：僵尸系统 (zombieSystem) 未初始化。");
                return;
            }
            
            // 计算生成位置
            Camera mainCamera = Camera.main; // 获取主相机
            Vector3 spawnPos = Vector3.zero; // 默认生成在原点
            
            if (mainCamera != null) // 如果找到了主相机
            {
                // 将生成位置设在相机前方5个单位处
                spawnPos = mainCamera.transform.position + mainCamera.transform.forward * 5f;
                spawnPos.z = 0; // 确保僵尸生成在Z=0的2D平面上
            }
            
            UnityEngine.Debug.Log($"[僵尸调试助手] 尝试在位置 {spawnPos} 生成一个Walker类型的测试僵尸。");
            zombieSystem.SpawnZombie(ZombieType.Walker, new Vector2(spawnPos.x, spawnPos.y)); // 调用僵尸系统生成僵尸
        }
        
        /// <summary>
        /// 清理场景中所有名称包含 "Zombie" 或 "TempZombie" (不区分大小写) 的GameObject。
        /// 使用 DestroyImmediate 方法立即销毁对象。
        /// </summary>
        void ClearAllZombies()
        {
            UnityEngine.Debug.Log("[僵尸调试助手] 开始清理场景中的所有僵尸对象...");
            // 查找场景中所有类型的GameObject
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            int clearedCount = 0; // 记录清理掉的对象数量
            
            foreach (var obj in allObjects)
            {
                // 如果对象名称包含 "Zombie" 或 "TempZombie" (忽略大小写)，则销毁它
                if (obj.name.ToLower().Contains("zombie") || obj.name.ToLower().Contains("tempzombie"))
                {
                    UnityEngine.Debug.Log($"  [僵尸调试助手] 清理对象: {obj.name}");
                    DestroyImmediate(obj); // 立即销毁对象。注意：在运行时应谨慎使用，优先用Destroy()。
                    clearedCount++;
                }
            }
            
            UnityEngine.Debug.Log($"[僵尸调试助手] 场景清理完成，共清理了 {clearedCount} 个僵尸相关对象。");
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕左上角显示一个包含各种调试信息的面板和控制按钮。
        /// </summary>
        void OnGUI()
        {
            if (!enableDebugGUI) return; // 如果禁用了调试GUI，则不显示
            
            // 定义GUI区域在屏幕左上角
            GUILayout.BeginArea(new Rect(10, 10, 350, 500), "", GUI.skin.box); // 调整了宽度和高度以容纳更多信息
            GUILayout.BeginVertical();
            
            // 面板标题
            GUILayout.Label("=== 僵尸调试工具面板 ===", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            
            // 显示系统初始化状态
            GUILayout.Space(10);
            GUILayout.Label("核心系统状态:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label($"  僵尸系统 (IZombieSystem): {(zombieSystem != null ? "<color=green>✓ 已加载</color>" : "<color=red>✗ 未加载</color>")}", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Label($"  可视化系统 (IZombieVisualizationSystem): {(visualizationSystem != null ? "<color=green>✓ 已加载</color>" : "<color=red>✗ 未加载</color>")}", new GUIStyle(GUI.skin.label) { richText = true });
            
            // 显示僵尸统计信息
            if (zombieSystem != null)
            {
                var zombieData = zombieSystem.GetZombieData();
                GUILayout.Space(10);
                GUILayout.Label("僵尸统计数据:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
                GUILayout.Label($"  总记录僵尸数: {zombieData.zombies.Count}");
                GUILayout.Label($"  当前活跃僵尸数: {zombieSystem.GetActiveZombieCount()}");
                GUILayout.Label($"  当前威胁等级: {zombieSystem.GetCurrentThreatLevel()}");
            }
            
            // 显示场景中实际的僵尸对象信息
            GUILayout.Space(10);
            GUILayout.Label("场景中僵尸对象扫描:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            GameObject[] allSceneObjects = FindObjectsOfType<GameObject>(); // 每次OnGUI都查找，可能影响性能，但用于调试可接受
            int sceneZombieObjectCount = 0;
            int sceneSpriteRendererCount = 0;
            
            // 简单列出前几个找到的僵尸对象信息，避免GUI过长
            int displayLimit = 3;
            foreach (var obj in allSceneObjects)
            {
                if (obj.name.ToLower().Contains("zombie"))
                {
                    sceneZombieObjectCount++;
                    if(displayLimit > 0) {
                        GUILayout.Label($"  - '{obj.name}' (位置: {obj.transform.position.ToString("F1")})");
                        displayLimit--;
                    }
                    
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        sceneSpriteRendererCount++;
                        // GUILayout.Label($"    Sprite: {(renderer.sprite != null ? renderer.sprite.name : "无")}, 可见: {renderer.enabled}"); // 详细信息可选
                    }
                }
            }
            if(sceneZombieObjectCount > 3) GUILayout.Label("    ...更多僵尸对象未在此列出");
            
            GUILayout.Label($"  场景中含'Zombie'名称的对象总数: {sceneZombieObjectCount}");
            GUILayout.Label($"  其中带SpriteRenderer的僵尸对象数: {sceneSpriteRendererCount}");
            
            // 显示主相机信息
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                GUILayout.Space(10);
                GUILayout.Label("主相机信息:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
                GUILayout.Label($"  位置: {mainCamera.transform.position.ToString("F1")}");
                GUILayout.Label($"  投影模式: {(mainCamera.orthographic ? "正交" : "透视")}");
                if (mainCamera.orthographic)
                {
                    GUILayout.Label($"  正交相机大小: {mainCamera.orthographicSize:F1}");
                }
            }
            
            // 控制按钮区域
            GUILayout.Space(10);
            GUILayout.Label("调试控制:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            if (GUILayout.Button($"生成测试僵尸 (快捷键: {spawnKey})"))
            {
                SpawnTestZombie();
            }
            
            if (GUILayout.Button($"清理所有僵尸及测试对象 (快捷键: {clearKey})"))
            {
                ClearAllZombies();
            }
            
            GUILayout.EndVertical(); // 结束垂直布局
            GUILayout.EndArea();   // 结束IMGUI区域
        }
    }
}