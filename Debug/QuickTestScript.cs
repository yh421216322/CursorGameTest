// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：QuickTestScript.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 QuickTestScript 的 MonoBehaviour 类。
//     该脚本提供了一个“一键测试”功能，用于自动或手动触发一系列预定义的测试流程，
//     以快速验证游戏核心系统（如僵尸系统、可视化、资源加载、相机等）的基本状态和功能。
//     测试结果主要通过控制台日志输出，同时提供一个简易的IMGUI界面显示测试状态和快捷键。
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
    /// 一键快速测试脚本。
    /// 挂载此脚本到场景中的一个GameObject上，可以自动按顺序执行一系列预设的测试步骤，
    /// 或通过快捷键手动触发完整测试。主要用于开发和调试阶段快速验证核心功能。
    /// </summary>
    public class QuickTestScript : MonoBehaviour
    {
        [Header("自动测试设置")] // Inspector中分组显示
        public bool autoRunTest = true;     // 是否在脚本启动时自动开始执行测试序列
        public float testDelay = 2f;        // 自动测试时，每个测试步骤之间的延迟时间（秒）
        
        private float testTimer = 0f;       // 计时器，用于控制自动测试的步骤间隔
        private int testStep = 0;           // 当前执行到的自动测试步骤的索引
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 如果设置了autoRunTest，则初始化自动测试计时器。
        /// </summary>
        void Start()
        {
            if (autoRunTest)
            {
                UnityEngine.Debug.Log("=== 自动快速测试已启动 ===");
                // 设置第一个测试步骤的执行时间点
                testTimer = Time.time + testDelay;
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 如果启用了自动测试且当前时间达到了下一个测试步骤的执行时间点，则执行测试步骤。
        /// 同时监听F1快捷键以手动触发完整测试。
        /// </summary>
        void Update()
        {
            // 自动测试逻辑
            if (autoRunTest && Time.time >= testTimer)
            {
                ExecuteTestStep(); // 执行当前测试步骤
                testTimer = Time.time + testDelay; // 设置下一个测试步骤的执行时间点
                testStep++; // 移动到下一个测试步骤
            }
            
            // 手动触发完整测试的快捷键
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ExecuteFullTest(); // 执行完整测试序列
            }
        }
        
        /// <summary>
        /// 根据当前的testStep索引，执行相应的测试步骤。
        /// </summary>
        void ExecuteTestStep()
        {
            switch (testStep)
            {
                case 0: TestSystemStatus(); break;    // 测试核心系统状态
                case 1: TestSpriteLoading(); break;   // 测试Sprite资源加载
                case 2: TestZombieSpawning(); break;  // 测试僵尸生成功能
                case 3: TestCameraPosition(); break;  // 检查并可能调整相机位置
                case 4:
                    TestComplete();             // 执行测试完成后的总结和检查
                    autoRunTest = false;        // 所有自动测试步骤完成后，停止自动测试
                    break;
            }
        }
        
        /// <summary>
        /// 测试步骤1：检查核心游戏系统（如僵尸系统、可视化系统）是否能正常获取。
        /// </summary>
        void TestSystemStatus()
        {
            UnityEngine.Debug.Log("=== 测试步骤 1: 核心系统状态检查 ===");
            bool allSystemsOK = true;
            
            try
            {
                // 尝试通过QFramework的RegisterManager获取IZombieSystem实例
                var zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
                if (zombieSystem != null) UnityEngine.Debug.Log("  ✅ 僵尸系统 (IZombieSystem): 成功获取实例。");
                else { UnityEngine.Debug.LogError("  ❌ 僵尸系统 (IZombieSystem): 获取实例失败 (返回null)。"); allSystemsOK = false; }
            }
            catch (System.Exception e) // 捕获获取过程中可能发生的异常
            {
                UnityEngine.Debug.LogError("  ❌ 僵尸系统 (IZombieSystem): 获取时发生异常 - " + e.Message);
                allSystemsOK = false;
            }
            
            try
            {
                // 尝试获取IZombieVisualizationSystem实例
                var visualSystem = RegisterManager.Interface.GetSystem<IZombieVisualizationSystem>();
                if (visualSystem != null) UnityEngine.Debug.Log("  ✅ 僵尸可视化系统 (IZombieVisualizationSystem): 成功获取实例。");
                else { UnityEngine.Debug.LogError("  ❌ 僵尸可视化系统 (IZombieVisualizationSystem): 获取实例失败 (返回null)。"); allSystemsOK = false; }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("  ❌ 僵尸可视化系统 (IZombieVisualizationSystem): 获取时发生异常 - " + e.Message);
                allSystemsOK = false;
            }

            if(allSystemsOK) UnityEngine.Debug.Log("--- 系统状态检查完毕: 所有请求的系统均可访问 ---");
            else UnityEngine.Debug.LogError("--- 系统状态检查完毕: 部分核心系统未能成功访问! ---");
        }
        
        /// <summary>
        /// 测试步骤2：检查预定义的Sprite资源是否能从Resources文件夹中成功加载。
        /// </summary>
        void TestSpriteLoading()
        {
            UnityEngine.Debug.Log("=== 测试步骤 2: Sprite资源加载测试 ===");
            
            // 定义要测试加载的Sprite名称列表 (路径相对于 "Resources/")
            string[] spriteNames = { "zb1", "zb2", "zb3", "zb4" }; // 假设这些Sprite位于 "Resources/Image/" 目录下
            int successCount = 0;
            
            foreach (string spriteName in spriteNames)
            {
                // 尝试加载Sprite资源
                Sprite sprite = Resources.Load<Sprite>($"Image/{spriteName}"); // 拼接完整路径
                if (sprite != null) // 如果加载成功
                {
                    UnityEngine.Debug.Log($"  ✅ Sprite '{spriteName}': 加载成功。 (尺寸: {sprite.rect.width}x{sprite.rect.height})");
                    successCount++;
                }
                else // 如果加载失败
                {
                    UnityEngine.Debug.LogWarning($"  ⚠️ Sprite '{spriteName}': 加载失败。请检查路径和资源是否存在于 'Resources/Image/' 目录下。");
                }
            }
            
            UnityEngine.Debug.Log($"--- Sprite资源加载测试完毕: {successCount}/{spriteNames.Length} 个Sprite加载成功 ---");
        }
        
        /// <summary>
        /// 测试步骤3：测试僵尸生成功能。
        /// </summary>
        void TestZombieSpawning()
        {
            UnityEngine.Debug.Log("=== 测试步骤 3: 僵尸生成功能测试 ===");
            
            try
            {
                var zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
                
                // 在原点(0,0)生成一个Walker类型的测试僵尸
                Vector2 spawnPos = Vector2.zero;
                zombieSystem.SpawnZombie(ZombieType.Walker, spawnPos); // 调用僵尸系统的生成方法
                
                UnityEngine.Debug.Log("  ✅ 僵尸生成命令已成功发送。");
                
                // 检查生成后的僵尸数量 (这可能不会立即反映，取决于系统实现是同步还是异步)
                var zombieData = zombieSystem.GetZombieData(); // 获取僵尸数据
                UnityEngine.Debug.Log($"  ℹ️ 当前总僵尸数量 (可能包含刚生成的): {zombieData.zombies.Count}");
            }
            catch (System.Exception e) // 捕获生成过程中可能发生的异常
            {
                UnityEngine.Debug.LogError("  ❌ 僵尸生成测试失败: " + e.Message);
            }
            UnityEngine.Debug.Log("--- 僵尸生成测试完毕 ---");
        }
        
        /// <summary>
        /// 测试步骤4：检查主相机的设置和位置。
        /// 如果相机位置过远，会自动尝试将其移回原点附近。
        /// </summary>
        void TestCameraPosition()
        {
            UnityEngine.Debug.Log("=== 测试步骤 4: 主相机状态检查与调整 ===");
            
            Camera mainCamera = Camera.main; // 获取场景中的主相机
            if (mainCamera != null)
            {
                UnityEngine.Debug.Log($"  📷 主相机名称: {mainCamera.name}");
                UnityEngine.Debug.Log($"  📷 当前位置: {mainCamera.transform.position}");
                UnityEngine.Debug.Log($"  📷 投影模式: {(mainCamera.orthographic ? "正交 (Orthographic)" : "透视 (Perspective)")}");
                
                if (mainCamera.orthographic) // 如果是正交相机，额外打印其大小
                {
                    UnityEngine.Debug.Log($"  📷 正交相机大小 (Orthographic Size): {mainCamera.orthographicSize}");
                }
                
                // 如果相机距离世界原点过远（例如超过20个单位），则自动将其位置重置到预设的(-10 Z轴)位置
                if (Vector3.Distance(mainCamera.transform.position, Vector3.zero) > 20f)
                {
                    mainCamera.transform.position = new Vector3(0, 0, -10); // 默认相机Z轴为-10以面向XY平面
                    UnityEngine.Debug.Log("  📷 相机位置距离原点过远，已自动调整至 (0, 0, -10)。");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("  ❌ 主相机 (Camera.main) 未找到！请确保场景中存在一个标记为'MainCamera'的相机。");
            }
            UnityEngine.Debug.Log("--- 主相机检查完毕 ---");
        }
        
        /// <summary>
        /// 自动测试的最后一个步骤：总结测试结果，例如统计场景中生成的僵尸对象。
        /// </summary>
        void TestComplete()
        {
            UnityEngine.Debug.Log("========================================");
            UnityEngine.Debug.Log("=== 所有自动测试步骤已完成 ===");
            UnityEngine.Debug.Log("========================================");
            
            // 统计场景中所有名称包含"Zombie"的GameObject数量
            GameObject[] allObjects = FindObjectsOfType<GameObject>(); // 获取场景中所有GameObject
            int zombieCountInScene = 0;
            
            foreach (var obj in allObjects)
            {
                // 通过名称简单判断是否为僵尸对象（可能不够精确，取决于命名规范）
                if (obj.name.ToLower().Contains("zombie")) // 不区分大小写查找"zombie"
                {
                    zombieCountInScene++;
                    UnityEngine.Debug.Log($"  🧟 场景中发现僵尸对象: '{obj.name}' (位置: {obj.transform.position})");
                }
            }
            
            if (zombieCountInScene > 0)
            {
                UnityEngine.Debug.Log($"  ✅ 测试总结：在场景中总共找到 {zombieCountInScene} 个可能由测试生成的僵尸对象。");
                UnityEngine.Debug.Log("  💡 提示：如果视觉上未看到僵尸，请检查Game窗口的显示或调整相机位置/视野。");
            }
            else
            {
                UnityEngine.Debug.LogWarning("  ⚠️ 测试总结：未在场景中找到符合条件的僵尸对象。请检查之前的测试步骤日志，确认僵尸生成是否成功，以及命名是否匹配查找条件。");
            }
            
            UnityEngine.Debug.Log("🎮 按 F1 键可以手动重新运行完整测试序列。");
        }
        
        /// <summary>
        /// 执行完整的测试序列。通常由手动快捷键（如F1）调用。
        /// </summary>
        void ExecuteFullTest()
        {
            UnityEngine.Debug.Log("=== F1键被按下：开始执行完整测试序列 ===");
            testStep = 0;         // 重置测试步骤到第一步
            autoRunTest = true;   // 激活自动测试标志
            testTimer = Time.time + 0.5f; // 设置一个较短的延迟后开始第一个测试步骤
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕右上角显示一个小的“一键测试工具”状态面板。
        /// </summary>
        void OnGUI()
        {
            // 定义IMGUI区域在屏幕右上角
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 120), "", GUI.skin.box); // 使用box样式作为背景
            GUILayout.BeginVertical();
            
            // 标题，使用加粗字体
            GUILayout.Label("一键快速测试", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            
            if (autoRunTest && testStep < 5) // 假设总共5个步骤 (0-4)
            {
                // 如果自动测试正在进行中，显示当前步骤
                GUILayout.Label($"自动测试进行中...\n当前步骤: {testStep + 1} / 5");
            }
            else
            {
                // 如果自动测试未运行或已完成，显示一个按钮来手动开始
                if (GUILayout.Button("开始自动测试 (F1)"))
                {
                    ExecuteFullTest(); // 点击按钮等同于按F1
                }
            }
            
            // 始终显示F1快捷键提示
            GUILayout.Label("快捷键: F1 - 重新运行测试");
            
            GUILayout.EndVertical(); // 结束垂直布局
            GUILayout.EndArea();   // 结束IMGUI区域
        }
    }
}