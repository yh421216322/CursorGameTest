using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Visualization;
using SurvivalGame.Model;
using MyGameNamespace;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 一键测试脚本
    /// 自动执行所有测试流程
    /// </summary>
    public class QuickTestScript : MonoBehaviour
    {
        [Header("自动测试设置")]
        public bool autoRunTest = true;
        public float testDelay = 2f;
        
        private float testTimer = 0f;
        private int testStep = 0;
        
        void Start()
        {
            if (autoRunTest)
            {
                UnityEngine.Debug.Log("=== 开始自动测试 ===");
                testTimer = Time.time + testDelay;
            }
        }
        
        void Update()
        {
            if (autoRunTest && Time.time >= testTimer)
            {
                ExecuteTestStep();
                testTimer = Time.time + testDelay;
                testStep++;
            }
            
            // 手动测试快捷键
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ExecuteFullTest();
            }
        }
        
        void ExecuteTestStep()
        {
            switch (testStep)
            {
                case 0:
                    TestSystemStatus();
                    break;
                case 1:
                    TestSpriteLoading();
                    break;
                case 2:
                    TestZombieSpawning();
                    break;
                case 3:
                    TestCameraPosition();
                    break;
                case 4:
                    TestComplete();
                    autoRunTest = false; // 停止自动测试
                    break;
            }
        }
        
        void TestSystemStatus()
        {
            UnityEngine.Debug.Log("=== 步骤1: 测试系统状态 ===");
            
            try
            {
                var zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
                UnityEngine.Debug.Log("✅ 僵尸系统：正常");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("❌ 僵尸系统：失败 - " + e.Message);
            }
            
            try
            {
                var visualSystem = RegisterManager.Interface.GetSystem<IZombieVisualizationSystem>();
                UnityEngine.Debug.Log("✅ 可视化系统：正常");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("❌ 可视化系统：失败 - " + e.Message);
            }
        }
        
        void TestSpriteLoading()
        {
            UnityEngine.Debug.Log("=== 步骤2: 测试图片加载 ===");
            
            string[] spriteNames = { "zb1", "zb2", "zb3", "zb4" };
            int successCount = 0;
            
            foreach (string spriteName in spriteNames)
            {
                Sprite sprite = Resources.Load<Sprite>($"Image/{spriteName}");
                if (sprite != null)
                {
                    UnityEngine.Debug.Log($"✅ {spriteName}: 加载成功");
                    successCount++;
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"⚠️ {spriteName}: 加载失败");
                }
            }
            
            UnityEngine.Debug.Log($"图片加载结果: {successCount}/{spriteNames.Length} 成功");
        }
        
        void TestZombieSpawning()
        {
            UnityEngine.Debug.Log("=== 步骤3: 测试僵尸生成 ===");
            
            try
            {
                var zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
                
                // 生成测试僵尸
                Vector2 spawnPos = new Vector2(0, 0);
                zombieSystem.SpawnZombie(ZombieType.Walker, spawnPos);
                
                UnityEngine.Debug.Log("✅ 僵尸生成命令已发送");
                
                // 检查僵尸数量
                var zombieData = zombieSystem.GetZombieData();
                UnityEngine.Debug.Log($"当前僵尸数量: {zombieData.zombies.Count}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("❌ 僵尸生成失败: " + e.Message);
            }
        }
        
        void TestCameraPosition()
        {
            UnityEngine.Debug.Log("=== 步骤4: 检查摄像机设置 ===");
            
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                UnityEngine.Debug.Log($"📷 摄像机位置: {mainCamera.transform.position}");
                UnityEngine.Debug.Log($"📷 投影模式: {(mainCamera.orthographic ? "正交" : "透视")}");
                
                if (mainCamera.orthographic)
                {
                    UnityEngine.Debug.Log($"📷 正交大小: {mainCamera.orthographicSize}");
                }
                
                // 自动调整摄像机到合适位置
                if (Vector3.Distance(mainCamera.transform.position, Vector3.zero) > 20f)
                {
                    mainCamera.transform.position = new Vector3(0, 0, -10);
                    UnityEngine.Debug.Log("📷 已自动调整摄像机位置到原点");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("❌ 未找到主摄像机");
            }
        }
        
        void TestComplete()
        {
            UnityEngine.Debug.Log("=== 测试完成 ===");
            
            // 统计场景中的僵尸对象
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int zombieCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Zombie"))
                {
                    zombieCount++;
                    UnityEngine.Debug.Log($"🧟 找到僵尸对象: {obj.name} 位置: {obj.transform.position}");
                }
            }
            
            if (zombieCount > 0)
            {
                UnityEngine.Debug.Log($"✅ 测试成功！找到 {zombieCount} 个僵尸对象");
                UnityEngine.Debug.Log("💡 如果看不到僵尸，请检查Game窗口或调整摄像机位置");
            }
            else
            {
                UnityEngine.Debug.LogWarning("⚠️ 未找到僵尸对象，请检查系统配置");
            }
            
            UnityEngine.Debug.Log("🎮 按 F1 键可以重新运行完整测试");
        }
        
        void ExecuteFullTest()
        {
            UnityEngine.Debug.Log("=== F1键：执行完整测试 ===");
            testStep = 0;
            autoRunTest = true;
            testTimer = Time.time + 0.5f;
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 100));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("一键测试工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            if (autoRunTest)
            {
                GUILayout.Label($"自动测试进行中...\n步骤: {testStep + 1}/5");
            }
            else
            {
                if (GUILayout.Button("开始自动测试"))
                {
                    ExecuteFullTest();
                }
            }
            
            GUILayout.Label("快捷键: F1 - 完整测试");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
} 