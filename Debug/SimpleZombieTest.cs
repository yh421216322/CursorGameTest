using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Visualization;
using SurvivalGame.Model;
using MyGameNamespace;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 简化的僵尸测试脚本
    /// 专门用于快速验证系统状态，不使用任何标签查找
    /// </summary>
    public class SimpleZombieTest : MonoBehaviour
    {
        [Header("测试设置")]
        public bool showDebugInfo = true;
        public KeyCode testKey = KeyCode.F5;
        
        private IZombieSystem zombieSystem;
        private IZombieVisualizationSystem visualSystem;
        private float lastTestTime = 0f;
        
        void Start()
        {
            UnityEngine.Debug.Log("=== 简化僵尸测试启动 ===");
            InitializeSystems();
        }
        
        void InitializeSystems()
        {
            try
            {
                zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
                UnityEngine.Debug.Log("✅ 僵尸系统初始化成功");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ 僵尸系统初始化失败: {e.Message}");
            }
            
            try
            {
                visualSystem = RegisterManager.Interface.GetSystem<IZombieVisualizationSystem>();
                UnityEngine.Debug.Log("✅ 可视化系统初始化成功");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ 可视化系统初始化失败: {e.Message}");
            }
        }
        
        void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                RunQuickTest();
            }
        }
        
        void RunQuickTest()
        {
            UnityEngine.Debug.Log("=== 开始快速测试 ===");
            
            // 1. 系统状态检查
            bool systemsOK = (zombieSystem != null && visualSystem != null);
            UnityEngine.Debug.Log($"系统状态: {(systemsOK ? "✅ 正常" : "❌ 异常")}");
            
            // 2. 僵尸数据检查
            if (zombieSystem != null)
            {
                try
                {
                    var zombieData = zombieSystem.GetZombieData();
                    int activeCount = zombieSystem.GetActiveZombieCount();
                    var threatLevel = zombieSystem.GetCurrentThreatLevel();
                    
                    UnityEngine.Debug.Log($"僵尸数据: 总数={zombieData.zombies.Count}, 活跃={activeCount}, 威胁={threatLevel}");
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"获取僵尸数据失败: {e.Message}");
                }
            }
            
            // 3. 场景对象检查（不使用标签）
            CheckSceneObjects();
            
            // 4. 摄像机检查
            CheckCamera();
            
            // 5. 生成测试僵尸
            if (systemsOK && Time.time - lastTestTime > 2f)
            {
                SpawnTestZombie();
                lastTestTime = Time.time;
            }
            
            UnityEngine.Debug.Log("=== 快速测试完成 ===");
        }
        
        void CheckSceneObjects()
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int zombieCount = 0;
            int rendererCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie"))
                {
                    zombieCount++;
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        rendererCount++;
                        UnityEngine.Debug.Log($"🧟 僵尸对象: {obj.name}, 位置: {obj.transform.position}, " +
                                              $"Sprite: {renderer.sprite?.name}, 可见: {renderer.enabled}, " +
                                              $"Layer: {renderer.sortingOrder}");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"🧟 僵尸对象: {obj.name}, 位置: {obj.transform.position} [无SpriteRenderer]");
                    }
                }
            }
            
            UnityEngine.Debug.Log($"场景统计: 僵尸对象={zombieCount}, 渲染器={rendererCount}");
        }
        
        void CheckCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                UnityEngine.Debug.Log($"📷 摄像机: 位置={mainCamera.transform.position}, " +
                                      $"正交={mainCamera.orthographic}, " +
                                      $"大小={mainCamera.orthographicSize}");
            }
            else
            {
                UnityEngine.Debug.LogWarning("⚠️ 未找到主摄像机");
            }
        }
        
        void SpawnTestZombie()
        {
            if (zombieSystem == null) return;
            
            // 在摄像机附近生成
            Vector2 spawnPos = Vector2.zero;
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 camPos = mainCamera.transform.position;
                spawnPos = new Vector2(camPos.x + Random.Range(-3f, 3f), camPos.y + Random.Range(-3f, 3f));
            }
            
            UnityEngine.Debug.Log($"🎯 在位置 {spawnPos} 生成测试僵尸");
            
            try
            {
                zombieSystem.SpawnZombie(ZombieType.Walker, spawnPos);
                UnityEngine.Debug.Log("✅ 僵尸生成命令发送成功");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ 僵尸生成失败: {e.Message}");
            }
        }
        
        void OnGUI()
        {
            if (!showDebugInfo) return;
            
            // 右下角显示简单信息
            float width = 250f;
            float height = 120f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 10, Screen.height - height - 10, width, height));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("简化测试工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            GUILayout.Label($"系统: {(zombieSystem != null && visualSystem != null ? "✅" : "❌")}");
            
            if (zombieSystem != null)
            {
                try
                {
                    var data = zombieSystem.GetZombieData();
                    GUILayout.Label($"僵尸: {data.zombies.Count} / {zombieSystem.GetActiveZombieCount()}");
                }
                catch
                {
                    GUILayout.Label("僵尸: 数据错误");
                }
            }
            
            // 统计场景中的僵尸对象
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int sceneZombies = 0;
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie"))
                    sceneZombies++;
            }
            GUILayout.Label($"场景中: {sceneZombies} 个僵尸对象");
            
            if (GUILayout.Button($"快速测试 ({testKey})"))
            {
                RunQuickTest();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
} 