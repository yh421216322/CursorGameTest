using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Model;
using SurvivalGame.Command;
using MyGameNamespace;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸命令测试脚本
    /// 专门测试X键和C键的僵尸群/僵尸潮生成命令
    /// </summary>
    public class ZombieCommandTest : MonoBehaviour, IController
    {
        [Header("测试设置")]
        public bool enableDebugGUI = true;
        public KeyCode testHordeKey = KeyCode.F6; // 测试僵尸群
        public KeyCode testWaveKey = KeyCode.F7;  // 测试僵尸潮
        
        private IZombieSystem zombieSystem;
        
        void Start()
        {
           // UnityEngine.Debug.Log("=== 僵尸命令测试初始化 ===");
            InitializeSystem();
        }
        
        void InitializeSystem()
        {
            try
            {
                zombieSystem = this.GetSystem<IZombieSystem>();
              ///  UnityEngine.Debug.Log("✅ 僵尸命令测试：系统获取成功");
            }
            catch (System.Exception e)
            {
              //  UnityEngine.Debug.LogError($"❌ 僵尸命令测试：系统获取失败 - {e.Message}");
            }
        }
        
        void Update()
        {
            // 测试按键
            if (Input.GetKeyDown(testHordeKey))
            {
                TestSpawnZombieHorde();
            }
            
            if (Input.GetKeyDown(testWaveKey))
            {
                TestTriggerZombieWave();
            }
            
            // 也监听GameManager的X和C键，看看能否捕获
            if (Input.GetKeyDown(KeyCode.X))
            {
             //   UnityEngine.Debug.Log("🔍 检测到X键按下，测试SpawnZombieHordeCommand");
                TestGameManagerHordeCommand();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
              //  UnityEngine.Debug.Log("🔍 检测到C键按下，测试TriggerZombieWaveCommand");
                TestGameManagerWaveCommand();
            }
        }
        
        void TestSpawnZombieHorde()
        {
           // UnityEngine.Debug.Log("=== 测试僵尸群生成命令 ===");
            
            if (zombieSystem == null)
            {
              //  UnityEngine.Debug.LogError("❌ 僵尸系统未初始化");
                return;
            }
            
            Vector2 mousePos = Vector2.zero;
            if (Camera.main != null)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos = new Vector2(worldPos.x, worldPos.y);
            }
            
           // UnityEngine.Debug.Log($"🎯 测试位置: {mousePos}");
            
            try
            {
                // 直接调用系统方法
            //    UnityEngine.Debug.Log("方法1: 直接调用 SpawnZombieHorde");
                zombieSystem.SpawnZombieHorde(ZombieThreatLevel.Medium, mousePos);
                
                // 使用命令
                UnityEngine.Debug.Log("方法2: 使用 SpawnZombieHordeCommand");
                this.SendCommand(new SpawnZombieHordeCommand(ZombieThreatLevel.Medium, mousePos));
                
                // 检查结果
                Invoke("CheckHordeResult", 1f);
            }
            catch (System.Exception e)
            {
             //   UnityEngine.Debug.LogError($"❌ 僵尸群生成测试失败: {e.Message}");
               // UnityEngine.Debug.LogError($"堆栈跟踪: {e.StackTrace}");
            }
        }
        
        void TestTriggerZombieWave()
        {
           // UnityEngine.Debug.Log("=== 测试僵尸潮命令 ===");
            
            if (zombieSystem == null)
            {
              //  UnityEngine.Debug.LogError("❌ 僵尸系统未初始化");
                return;
            }
            
            try
            {
                // 使用命令
              //  UnityEngine.Debug.Log("使用 TriggerZombieWaveCommand");
                this.SendCommand(new TriggerZombieWaveCommand(ZombieThreatLevel.High));
                
                // 检查结果
                Invoke("CheckWaveResult", 2f);
            }
            catch (System.Exception e)
            {
              //  UnityEngine.Debug.LogError($"❌ 僵尸潮测试失败: {e.Message}");
                //UnityEngine.Debug.LogError($"堆栈跟踪: {e.StackTrace}");
            }
        }
        
        void TestGameManagerHordeCommand()
        {
           // UnityEngine.Debug.Log("--- 模拟GameManager的X键命令 ---");
            
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            UnityEngine.Debug.Log($"鼠标世界坐标: {mousePos}");
            
            try
            {
                var command = new SpawnZombieHordeCommand(ZombieThreatLevel.Medium, mousePos);
                this.SendCommand(command);
                UnityEngine.Debug.Log("✅ SpawnZombieHordeCommand 发送成功");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ SpawnZombieHordeCommand 发送失败: {e.Message}");
            }
        }
        
        void TestGameManagerWaveCommand()
        {
            UnityEngine.Debug.Log("--- 模拟GameManager的C键命令 ---");
            
            try
            {
                var command = new TriggerZombieWaveCommand(ZombieThreatLevel.High);
                this.SendCommand(command);
                UnityEngine.Debug.Log("✅ TriggerZombieWaveCommand 发送成功");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ TriggerZombieWaveCommand 发送失败: {e.Message}");
            }
        }
        
        void CheckHordeResult()
        {
            UnityEngine.Debug.Log("--- 检查僵尸群生成结果 ---");
            
            if (zombieSystem != null)
            {
                var zombieData = zombieSystem.GetZombieData();
                UnityEngine.Debug.Log($"当前僵尸总数: {zombieData.zombies.Count}");
                UnityEngine.Debug.Log($"当前僵尸群数: {zombieData.hordes.Count}");
                
                foreach (var horde in zombieData.hordes.Values)
                {
                    UnityEngine.Debug.Log($"僵尸群: {horde.name}, 位置: {horde.centerPosition}, 僵尸数: {horde.ZombieCount}");
                }
            }
        }
        
        void CheckWaveResult()
        {
            UnityEngine.Debug.Log("--- 检查僵尸潮结果 ---");
            CheckHordeResult();
        }
        
        void OnGUI()
        {
            if (!enableDebugGUI) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("僵尸命令测试", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            if (GUILayout.Button($"测试僵尸群 ({testHordeKey})"))
            {
                TestSpawnZombieHorde();
            }
            
            if (GUILayout.Button($"测试僵尸潮 ({testWaveKey})"))
            {
                TestTriggerZombieWave();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("原始命令测试:");
            
            if (GUILayout.Button("模拟X键命令"))
            {
                TestGameManagerHordeCommand();
            }
            
            if (GUILayout.Button("模拟C键命令"))
            {
                TestGameManagerWaveCommand();
            }
            
            GUILayout.Space(10);
            
            // 显示系统状态
            if (zombieSystem != null)
            {
                var data = zombieSystem.GetZombieData();
                GUILayout.Label($"僵尸数: {data.zombies.Count}");
                GUILayout.Label($"僵尸群数: {data.hordes.Count}");
                GUILayout.Label($"威胁级别: {data.currentThreatLevel}");
            }
            else
            {
                GUILayout.Label("系统未初始化", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        // 实现IController接口
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
    }
} 