// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：SimpleZombieTest.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 SimpleZombieTest 的 MonoBehaviour 类。
//     该脚本是一个简化的测试工具，用于快速验证游戏中与僵尸相关的核心系统
//     （如僵尸系统本身、可视化系统）的状态和基本功能（如僵尸生成）。
//     它避免使用Unity的标签查找对象，而是直接通过名称或其他属性进行识别。
//     测试结果和状态信息会输出到控制台，并在屏幕上通过IMGUI显示一个小型调试面板。
// ==============================================================================

using UnityEngine;
using QFramework; // QFramework框架，用于获取系统实例
using SurvivalGame.GameSystem; // 包含IZombieSystem等游戏系统接口
using SurvivalGame.Visualization; // 包含IZombieVisualizationSystem等可视化系统接口
using SurvivalGame.Model;         // 包含ZombieType等模型定义
using MyGameNamespace;    // 自定义命名空间，可能包含事件或其他定义

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 简化的僵尸相关功能快速测试脚本。
    /// 本脚本旨在提供一种不依赖Unity标签（Tag）查找对象的方式，来快速验证
    /// 僵尸系统、可视化系统等核心模块的基本状态和功能是否正常。
    /// 它可以通过快捷键或IMGUI按钮触发一系列测试。
    /// </summary>
    public class SimpleZombieTest : MonoBehaviour
    {
        [Header("测试设置")] // Inspector中分组显示
        public bool showDebugInfo = true;    // 是否在屏幕上显示IMGUI调试信息面板
        public KeyCode testKey = KeyCode.F5; // 手动触发快速测试的快捷键，默认为F5

        // 系统接口引用
        private IZombieSystem zombieSystem; // 僵尸系统实例
        private IZombieVisualizationSystem visualSystem; // 僵尸可视化系统实例
        
        private float lastTestTime = 0f; // 上次执行生成僵尸测试的时间戳，用于防止过于频繁地生成
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 初始化时，会尝试获取所需的游戏系统实例。
        /// </summary>
        void Start()
        {
            UnityEngine.Debug.Log("=== [简化僵尸测试] 脚本已启动并开始初始化系统... ===");
            InitializeSystems(); // 调用方法初始化系统引用
        }
        
        /// <summary>
        /// 初始化所需的系统引用（僵尸系统和可视化系统）。
        /// 结果会输出到控制台。
        /// </summary>
        void InitializeSystems()
        {
            try
            {
                // 通过QFramework的RegisterManager获取IZombieSystem的实例
                zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
                if (zombieSystem != null) UnityEngine.Debug.Log("  [简化僵尸测试] ✅ 僵尸系统 (IZombieSystem) 初始化成功。");
                else UnityEngine.Debug.LogError("  [简化僵尸测试] ❌ 僵尸系统 (IZombieSystem) 初始化失败：未能获取实例。");
            }
            catch (System.Exception e) // 捕获获取过程中可能发生的异常
            {
                UnityEngine.Debug.LogError($"  [简化僵尸测试] ❌ 僵尸系统 (IZombieSystem) 初始化时发生异常: {e.Message}");
            }
            
            try
            {
                // 获取IZombieVisualizationSystem的实例
                visualSystem = RegisterManager.Interface.GetSystem<IZombieVisualizationSystem>();
                if (visualSystem != null) UnityEngine.Debug.Log("  [简化僵尸测试] ✅ 僵尸可视化系统 (IZombieVisualizationSystem) 初始化成功。");
                else UnityEngine.Debug.LogError("  [简化僵尸测试] ❌ 僵尸可视化系统 (IZombieVisualizationSystem) 初始化失败：未能获取实例。");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"  [简化僵尸测试] ❌ 僵尸可视化系统 (IZombieVisualizationSystem) 初始化时发生异常: {e.Message}");
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 监听指定的测试快捷键 (testKey)，按下时执行快速测试。
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(testKey)) // 如果按下了测试快捷键
            {
                RunQuickTest(); // 执行快速测试流程
            }
        }
        
        /// <summary>
        /// 执行一系列快速测试步骤，并将结果输出到控制台。
        /// 包括：系统状态检查、僵尸数据检查、场景对象检查、相机检查以及生成测试僵尸。
        /// </summary>
        void RunQuickTest()
        {
            UnityEngine.Debug.Log("=== [快速测试] 开始执行 ===");
            
            // 1. 检查核心系统是否已成功初始化
            bool systemsOK = (zombieSystem != null && visualSystem != null);
            UnityEngine.Debug.Log($"  [快速测试] 系统状态: {(systemsOK ? "✅ 所有核心系统正常" : "❌ 部分核心系统异常或未初始化")}");
            
            // 2. 检查僵尸系统数据
            if (zombieSystem != null)
            {
                try
                {
                    var zombieDataContainer = zombieSystem.GetZombieData(); // 获取僵尸数据容器
                    int activeCount = zombieSystem.GetActiveZombieCount();    // 获取活跃僵尸数量
                    var threatLevel = zombieSystem.GetCurrentThreatLevel(); // 获取当前威胁等级
                    
                    UnityEngine.Debug.Log($"  [快速测试] 僵尸数据: 总数={zombieDataContainer.zombies.Count}, 活跃数={activeCount}, 当前威胁等级={threatLevel}");
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"  [快速测试] 获取僵尸数据时发生错误: {e.Message}");
                }
            }
            
            // 3. 检查场景中的相关对象 (不依赖标签)
            CheckSceneObjects();
            
            // 4. 检查主相机状态
            CheckCamera();
            
            // 5. 如果系统状态正常，并且距离上次生成测试僵尸已超过2秒，则生成一个新的测试僵尸
            if (systemsOK && (Time.time - lastTestTime > 2f)) // 2秒冷却时间，防止短时间内大量生成
            {
                SpawnTestZombie(); // 调用生成测试僵尸的方法
                lastTestTime = Time.time; // 更新上次测试时间
            }
            else if (!systemsOK)
            {
                UnityEngine.Debug.LogWarning("  [快速测试] 由于核心系统未正常初始化，跳过生成测试僵尸步骤。");
            }
            
            UnityEngine.Debug.Log("=== [快速测试] 执行完毕 ===");
        }
        
        /// <summary>
        /// 检查场景中所有名称包含 "zombie" (不区分大小写) 的GameObject，并记录其信息。
        /// </summary>
        void CheckSceneObjects()
        {
            UnityEngine.Debug.Log("  --- [场景对象检查] ---");
            GameObject[] allObjects = FindObjectsOfType<GameObject>(); // 获取场景中所有GameObject
            int zombieObjectCount = 0; // 场景中找到的僵尸相关对象计数
            int zombieWithRendererCount = 0; // 其中有SpriteRenderer组件的计数
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie")) // 如果对象名称包含 "zombie"
                {
                    zombieObjectCount++;
                    var renderer = obj.GetComponent<SpriteRenderer>(); // 尝试获取其SpriteRenderer组件
                    if (renderer != null) // 如果有SpriteRenderer
                    {
                        zombieWithRendererCount++;
                        UnityEngine.Debug.Log($"    🧟 发现僵尸对象: '{obj.name}', 位置: {obj.transform.position}, " +
                                              $"Sprite: {(renderer.sprite != null ? renderer.sprite.name : "无Sprite")}, 可见性: {renderer.enabled}, " +
                                              $"渲染层排序: {renderer.sortingOrder}");
                    }
                    else // 如果没有SpriteRenderer
                    {
                        UnityEngine.Debug.Log($"    🧟 发现僵尸对象 (无SpriteRenderer): '{obj.name}', 位置: {obj.transform.position}");
                    }
                }
            }
            
            UnityEngine.Debug.Log($"  [场景对象检查] 统计: 共找到 {zombieObjectCount} 个名称含'zombie'的对象, 其中 {zombieWithRendererCount} 个带有SpriteRenderer。");
            UnityEngine.Debug.Log("  --- [场景对象检查] 完毕 ---");
        }
        
        /// <summary>
        /// 检查主相机的状态并记录其属性。
        /// </summary>
        void CheckCamera()
        {
            UnityEngine.Debug.Log("  --- [主相机检查] ---");
            Camera mainCamera = Camera.main; // 获取主相机
            if (mainCamera != null)
            {
                UnityEngine.Debug.Log($"    📷 主相机: '{mainCamera.name}', 位置: {mainCamera.transform.position}, " +
                                      $"投影模式: {(mainCamera.orthographic ? "正交" : "透视")}, " +
                                      (mainCamera.orthographic ? $"正交大小: {mainCamera.orthographicSize}" : $"视野角度: {mainCamera.fieldOfView}"));
            }
            else
            {
                UnityEngine.Debug.LogWarning("    ⚠️ 未能找到主相机 (Camera.main)。请确保场景中有一个相机被标记为MainCamera。");
            }
            UnityEngine.Debug.Log("  --- [主相机检查] 完毕 ---");
        }
        
        /// <summary>
        /// 生成一个测试僵尸（Walker类型）在相机视野附近的一个随机位置。
        /// </summary>
        void SpawnTestZombie()
        {
            if (zombieSystem == null) // 如果僵尸系统未初始化，则不执行
            {
                UnityEngine.Debug.LogError("  [生成测试僵尸] 失败：僵尸系统 (zombieSystem) 未初始化。");
                return;
            }
            
            // 计算生成位置：默认为原点，如果找到主相机，则在相机当前位置附近随机偏移
            Vector2 spawnPos = Vector2.zero;
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 camPos = mainCamera.transform.position;
                // 在相机XY平面位置 +/- 3个单位的范围内随机选择生成点
                spawnPos = new Vector2(camPos.x + Random.Range(-3f, 3f), camPos.y + Random.Range(-3f, 3f));
            }
            
            UnityEngine.Debug.Log($"  [生成测试僵尸] 准备在位置 {spawnPos} 生成一个 Walker 类型的僵尸。");
            
            try
            {
                zombieSystem.SpawnZombie(ZombieType.Walker, spawnPos); // 调用僵尸系统生成僵尸
                UnityEngine.Debug.Log("    ✅ 僵尸生成命令已成功发送。");
            }
            catch (System.Exception e) // 捕获生成过程中可能发生的异常
            {
                UnityEngine.Debug.LogError($"    ❌ 生成测试僵尸时发生错误: {e.Message}");
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕右下角显示一个简易的调试信息面板和测试按钮。
        /// </summary>
        void OnGUI()
        {
            if (!showDebugInfo) return; // 如果不显示调试信息，则直接返回
            
            // 定义GUI区域在屏幕右下角
            float panelWidth = 250f;
            float panelHeight = 130f; // 调整高度以容纳更多信息
            GUILayout.BeginArea(new Rect(Screen.width - panelWidth - 10, Screen.height - panelHeight - 10, panelWidth, panelHeight), "", GUI.skin.box);
            GUILayout.BeginVertical();
            
            // 面板标题
            GUILayout.Label("简化快速测试工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            
            // 显示核心系统状态 (僵尸系统和可视化系统是否都已获取)
            GUILayout.Label($"核心系统状态: {(zombieSystem != null && visualSystem != null ? "✅ 正常" : "❌ 异常")}");
            
            // 显示从僵尸系统获取的僵尸数量信息
            if (zombieSystem != null)
            {
                try
                {
                    var data = zombieSystem.GetZombieData();
                    GUILayout.Label($"系统僵尸数: {data.zombies.Count} (总) / {zombieSystem.GetActiveZombieCount()} (活跃)");
                }
                catch // 如果获取数据出错
                {
                    GUILayout.Label("僵尸系统数据: 获取错误");
                }
            }
            else
            {
                GUILayout.Label("僵尸系统数据: 未初始化");
            }
            
            // 统计并显示场景中实际的僵尸对象数量 (基于名称)
            GameObject[] allSceneObjects = FindObjectsOfType<GameObject>();
            int sceneZombieCount = 0;
            foreach (var obj in allSceneObjects)
            {
                if (obj.name.ToLower().Contains("zombie")) sceneZombieCount++;
            }
            GUILayout.Label($"场景中对象名含'zombie': {sceneZombieCount} 个");
            
            GUILayout.Space(5);
            // 添加一个按钮来手动触发快速测试，并显示其快捷键
            if (GUILayout.Button($"执行快速测试 ({testKey})"))
            {
                RunQuickTest();
            }
            
            GUILayout.EndVertical(); // 结束垂直布局
            GUILayout.EndArea();   // 结束IMGUI区域
        }
    }
}