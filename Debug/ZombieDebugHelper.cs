using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Visualization;
using SurvivalGame.Model;
using MyGameNamespace;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸调试助手
    /// 用于诊断僵尸显示问题
    /// </summary>
    public class ZombieDebugHelper : MonoBehaviour
    {
        [Header("调试选项")]
        public bool enableDebugGUI = true;
        public bool autoSpawnTestZombie = false; // 改为false，避免自动生成
        public KeyCode spawnKey = KeyCode.J; // 改为J键，避免与GameManager的Z键冲突
        public KeyCode clearKey = KeyCode.K; // 改为K键，避免与GameManager的C键冲突
        
        private IZombieSystem zombieSystem;
        private IZombieVisualizationSystem visualizationSystem;
        private bool hasSpawnedTestZombie = false;
        
        void Start()
        {
            // 获取系统引用
            try
            {
                zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
//                UnityEngine.Debug.Log("僵尸系统获取成功");
            }
            catch (System.Exception e)
            {
             //   UnityEngine.Debug.LogError($"获取僵尸系统失败: {e.Message}");
            }
            
            try
            {
                visualizationSystem = RegisterManager.Interface.GetSystem<IZombieVisualizationSystem>();
             //   UnityEngine.Debug.Log("僵尸可视化系统获取成功");
            }
            catch (System.Exception e)
            {
              //  UnityEngine.Debug.LogError($"获取僵尸可视化系统失败: {e.Message}");
            }
        }
        
        void Update()
        {
            // 处理输入
            if (Input.GetKeyDown(spawnKey))
            {
                SpawnTestZombie();
            }
            
            if (Input.GetKeyDown(clearKey))
            {
                ClearAllZombies();
            }
            
            // 自动生成测试僵尸
            if (autoSpawnTestZombie && !hasSpawnedTestZombie && zombieSystem != null)
            {
                SpawnTestZombie();
                hasSpawnedTestZombie = true;
            }
        }
        
        void SpawnTestZombie()
        {
            if (zombieSystem == null)
            {
                UnityEngine.Debug.LogError("僵尸系统未初始化");
                return;
            }
            
            // 在摄像机前方生成僵尸
            Camera mainCamera = Camera.main;
            Vector3 spawnPos = Vector3.zero;
            
            if (mainCamera != null)
            {
                spawnPos = mainCamera.transform.position + mainCamera.transform.forward * 5f;
                spawnPos.z = 0; // 确保在2D平面上
            }
            
            UnityEngine.Debug.Log($"在位置 {spawnPos} 生成测试僵尸");
            zombieSystem.SpawnZombie(ZombieType.Walker, new Vector2(spawnPos.x, spawnPos.y));
        }
        
        void ClearAllZombies()
        {
            // 清理所有僵尸视觉对象 - 不使用标签，直接通过名称查找
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            int clearedCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Zombie") || obj.name.Contains("TempZombie"))
                {
                    UnityEngine.Debug.Log($"清理僵尸对象: {obj.name}");
                    DestroyImmediate(obj);
                    clearedCount++;
                }
            }
            
            UnityEngine.Debug.Log($"清理所有僵尸对象完成，共清理 {clearedCount} 个对象");
        }
        
        void OnGUI()
        {
            if (!enableDebugGUI) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== 僵尸调试工具 ===", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            // 系统状态
            GUILayout.Space(10);
            GUILayout.Label("系统状态:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label($"僵尸系统: {(zombieSystem != null ? "✓" : "✗")}");
            GUILayout.Label($"可视化系统: {(visualizationSystem != null ? "✓" : "✗")}");
            
            // 僵尸统计
            if (zombieSystem != null)
            {
                var zombieData = zombieSystem.GetZombieData();
                GUILayout.Space(10);
                GUILayout.Label("僵尸统计:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
                GUILayout.Label($"总数: {zombieData.zombies.Count}");
                GUILayout.Label($"活着: {zombieSystem.GetActiveZombieCount()}");
                GUILayout.Label($"威胁等级: {zombieSystem.GetCurrentThreatLevel()}");
            }
            
            // 场景中的对象
            GUILayout.Space(10);
            GUILayout.Label("场景对象:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int zombieObjectCount = 0;
            int spriteRendererCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Zombie"))
                {
                    zombieObjectCount++;
                    GUILayout.Label($"- {obj.name} (位置: {obj.transform.position})");
                    
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        spriteRendererCount++;
                        GUILayout.Label($"  SpriteRenderer: {renderer.sprite?.name}, 可见: {renderer.enabled}");
                    }
                }
            }
            
            GUILayout.Label($"僵尸对象总数: {zombieObjectCount}");
            GUILayout.Label($"SpriteRenderer数: {spriteRendererCount}");
            
            // 摄像机信息
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                GUILayout.Space(10);
                GUILayout.Label("摄像机信息:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
                GUILayout.Label($"位置: {mainCamera.transform.position}");
                GUILayout.Label($"投影: {mainCamera.orthographic}");
                if (mainCamera.orthographic)
                {
                    GUILayout.Label($"正交大小: {mainCamera.orthographicSize}");
                }
            }
            
            // 控制按钮
            GUILayout.Space(10);
            GUILayout.Label("控制:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            if (GUILayout.Button($"生成测试僵尸 (按 {spawnKey})"))
            {
                SpawnTestZombie();
            }
            
            if (GUILayout.Button($"清理所有僵尸 (按 {clearKey})"))
            {
                ClearAllZombies();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
} 