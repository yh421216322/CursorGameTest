using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Visualization;
using SurvivalGame.Model;
using MyGameNamespace;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸可视化诊断工具
    /// 检查僵尸数据和可视化对象之间的同步问题
    /// </summary>
    public class ZombieVisualizationDiagnostic : MonoBehaviour, IController
    {
        [Header("诊断设置")]
        public KeyCode diagnoseKey = KeyCode.F8;
        public KeyCode forceCreateKey = KeyCode.F9;
        public KeyCode syncKey = KeyCode.F10;
        
        private IZombieSystem zombieSystem;
        private IZombieVisualizationSystem visualizationSystem;
        
        void Start()
        {
          //  UnityEngine.Debug.Log("=== 僵尸可视化诊断工具启动 ===");
            InitializeSystems();
        }
        
        void InitializeSystems()
        {
            try
            {
                zombieSystem = this.GetSystem<IZombieSystem>();
              //  UnityEngine.Debug.Log("✅ 僵尸系统获取成功");
            }
            catch (System.Exception e)
            {
              //  UnityEngine.Debug.LogError($"❌ 僵尸系统获取失败: {e.Message}");
            }
            
            try
            {
                visualizationSystem = this.GetSystem<IZombieVisualizationSystem>();
               // UnityEngine.Debug.Log("✅ 可视化系统获取成功");
            }
            catch (System.Exception e)
            {
               // UnityEngine.Debug.LogError($"❌ 可视化系统获取失败: {e.Message}");
            }
        }
        
        void Update()
        {
            if (Input.GetKeyDown(diagnoseKey))
            {
                DiagnoseZombieVisualization();
            }
            
            if (Input.GetKeyDown(forceCreateKey))
            {
                ForceCreateVisuals();
            }
            
            if (Input.GetKeyDown(syncKey))
            {
                SynchronizeVisualization();
            }
        }
        
        void DiagnoseZombieVisualization()
        {
            UnityEngine.Debug.Log("=== 开始僵尸可视化诊断 ===");
            
            if (zombieSystem == null || visualizationSystem == null)
            {
                UnityEngine.Debug.LogError("❌ 系统未初始化，无法进行诊断");
                return;
            }
            
            // 1. 检查僵尸数据
            var zombieData = zombieSystem.GetZombieData();
            UnityEngine.Debug.Log($"📊 数据层：僵尸总数 = {zombieData.zombies.Count}");
            UnityEngine.Debug.Log($"📊 数据层：僵尸群数 = {zombieData.hordes.Count}");
            
            int aliveZombies = 0;
            foreach (var zombie in zombieData.zombies.Values)
            {
                if (zombie.IsAlive)
                {
                    aliveZombies++;
                    UnityEngine.Debug.Log($"🧟 数据僵尸: {zombie.type} ID:{zombie.id} 位置:{zombie.position} 状态:{zombie.state}");
                }
            }
            UnityEngine.Debug.Log($"📊 活着的僵尸: {aliveZombies}");
            
            // 2. 检查可视化对象
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int visualZombies = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie"))
                {
                    visualZombies++;
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    UnityEngine.Debug.Log($"🎮 可视化对象: {obj.name} 位置:{obj.transform.position} " +
                                          $"渲染器:{renderer != null} 精灵:{renderer?.sprite?.name}");
                }
            }
            UnityEngine.Debug.Log($"📊 可视化对象数: {visualZombies}");
            
            // 3. 检查可视化系统状态
            for (int i = 0; i < aliveZombies && i < 5; i++) // 检查前5个僵尸
            {
                var zombie = zombieData.zombies.Values.ToArray()[i];
                if (zombie.IsAlive)
                {
                    var visualObj = visualizationSystem.GetZombieVisual(zombie.id);
                    UnityEngine.Debug.Log($"🔍 僵尸 {zombie.id} 的可视化对象: {(visualObj != null ? visualObj.name : "null")}");
                }
            }
            
            // 4. 诊断结论
            if (aliveZombies > 0 && visualZombies == 0)
            {
                UnityEngine.Debug.LogError("❌ 诊断结果：僵尸数据存在但可视化对象缺失！");
                UnityEngine.Debug.Log("💡 可能原因：");
                UnityEngine.Debug.Log("   1. ZombieSpawnedEvent 事件没有触发");
                UnityEngine.Debug.Log("   2. 可视化系统没有监听到事件");
                UnityEngine.Debug.Log("   3. CreateZombieVisual 方法执行失败");
                UnityEngine.Debug.Log("🔧 建议：按F9键强制创建可视化对象");
            }
            else if (aliveZombies == visualZombies)
            {
                UnityEngine.Debug.Log("✅ 诊断结果：数据和可视化对象数量匹配");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"⚠️ 诊断结果：数据({aliveZombies})和可视化({visualZombies})数量不匹配");
            }
        }
        
        void ForceCreateVisuals()
        {
            UnityEngine.Debug.Log("=== 强制创建可视化对象 ===");
            
            if (zombieSystem == null || visualizationSystem == null)
            {
                UnityEngine.Debug.LogError("❌ 系统未初始化");
                return;
            }
            
            var zombieData = zombieSystem.GetZombieData();
            int createdCount = 0;
            
            foreach (var zombie in zombieData.zombies.Values)
            {
                if (zombie.IsAlive)
                {
                    // 检查是否已有可视化对象
                    var existingVisual = visualizationSystem.GetZombieVisual(zombie.id);
                    if (existingVisual == null)
                    {
                        try
                        {
                            visualizationSystem.CreateZombieVisual(zombie);
                            createdCount++;
                            UnityEngine.Debug.Log($"✅ 强制创建: {zombie.type} ID:{zombie.id}");
                        }
                        catch (System.Exception e)
                        {
                            UnityEngine.Debug.LogError($"❌ 创建失败: {zombie.id} - {e.Message}");
                        }
                    }
                }
            }
            
            UnityEngine.Debug.Log($"🎯 强制创建完成，共创建 {createdCount} 个可视化对象");
        }
        
        void SynchronizeVisualization()
        {
            UnityEngine.Debug.Log("=== 同步可视化系统 ===");
            
            if (visualizationSystem == null)
            {
                UnityEngine.Debug.LogError("❌ 可视化系统未初始化");
                return;
            }
            
            try
            {
                visualizationSystem.UpdateAllZombieVisuals();
                UnityEngine.Debug.Log("✅ 可视化系统同步完成");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ 同步失败: {e.Message}");
            }
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 300, Screen.height - 200, 290, 190));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("可视化诊断工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            if (GUILayout.Button($"诊断可视化问题 ({diagnoseKey})"))
            {
                DiagnoseZombieVisualization();
            }
            
            if (GUILayout.Button($"强制创建可视化 ({forceCreateKey})"))
            {
                ForceCreateVisuals();
            }
            
            if (GUILayout.Button($"同步可视化系统 ({syncKey})"))
            {
                SynchronizeVisualization();
            }
            
            GUILayout.Space(10);
            
            // 实时状态显示
            if (zombieSystem != null)
            {
                var data = zombieSystem.GetZombieData();
                GUILayout.Label($"数据僵尸: {data.zombies.Count}");
                
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                int visualCount = 0;
                foreach (var obj in allObjects)
                {
                    if (obj.name.ToLower().Contains("zombie"))
                        visualCount++;
                }
                GUILayout.Label($"可视化僵尸: {visualCount}");
                
                if (data.zombies.Count > 0 && visualCount == 0)
                {
                    GUILayout.Label("状态: 需要修复", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
                }
                else if (data.zombies.Count == visualCount)
                {
                    GUILayout.Label("状态: 正常", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } });
                }
                else
                {
                    GUILayout.Label("状态: 不同步", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } });
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
    }
} 