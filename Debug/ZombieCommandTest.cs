// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieCommandTest.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 ZombieCommandTest 的 MonoBehaviour 类。
//     该脚本是一个调试工具，用于专门测试与僵尸生成相关的命令，
//     特别是生成僵尸群 (Horde) 和触发僵尸潮 (Wave) 的命令。
//     它提供了通过快捷键或IMGUI按钮来执行这些测试，并观察结果。
// ==============================================================================

using UnityEngine;
using QFramework; // QFramework框架，用于获取系统实例和发送命令
using SurvivalGame.GameSystem; // 包含IZombieSystem等游戏系统接口
using SurvivalGame.Model;     // 包含ZombieType, ZombieThreatLevel等模型定义
using SurvivalGame.Command;   // 包含僵尸相关的命令定义，如SpawnZombieHordeCommand
using MyGameNamespace;    // 自定义命名空间 (当前在此文件中可能未使用特定内容)

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸相关命令的测试脚本。
    /// 主要用于在开发和调试阶段，通过快捷键或简易UI按钮，
    /// 快速触发和验证生成僵尸群 (Horde) 和僵尸潮 (Wave) 的命令是否按预期工作。
    /// </summary>
    public class ZombieCommandTest : MonoBehaviour, IController // 实现QFramework的IController接口
    {
        [Header("测试设置")] // Inspector中分组显示
        public bool enableDebugGUI = true;      // 是否在屏幕上显示IMGUI调试面板
        public KeyCode testHordeKey = KeyCode.F6; // 测试生成僵尸群的快捷键，默认为F6
        public KeyCode testWaveKey = KeyCode.F7;  // 测试触发僵尸潮的快捷键，默认为F7
        
        private IZombieSystem zombieSystem; // 僵尸系统实例的引用
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 初始化时，会尝试获取僵尸系统实例。
        /// </summary>
        void Start()
        {
            UnityEngine.Debug.Log("=== [僵尸命令测试] 脚本初始化 ===");
            InitializeSystem(); // 调用方法初始化系统引用
        }
        
        /// <summary>
        /// 初始化所需的系统引用（当前只有僵尸系统）。
        /// 获取结果会输出到控制台。
        /// </summary>
        void InitializeSystem()
        {
            try
            {
                // 通过QFramework的this.GetSystem扩展方法获取IZombieSystem的实例
                zombieSystem = this.GetSystem<IZombieSystem>();
                if (zombieSystem != null) UnityEngine.Debug.Log("  [僵尸命令测试] ✅ 僵尸系统 (IZombieSystem) 获取成功。");
                else UnityEngine.Debug.LogError("  [僵尸命令测试] ❌ 僵尸系统 (IZombieSystem) 获取失败：返回null。");
            }
            catch (System.Exception e) // 捕获获取过程中可能发生的异常
            {
                UnityEngine.Debug.LogError($"  [僵尸命令测试] ❌ 获取僵尸系统时发生异常: {e.Message}");
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 监听用户按下的快捷键，以触发相应的僵尸命令测试。
        /// </summary>
        void Update()
        {
            // 如果按下了测试僵尸群的快捷键
            if (Input.GetKeyDown(testHordeKey))
            {
                TestSpawnZombieHorde(); // 执行测试生成僵尸群的方法
            }
            
            // 如果按下了测试僵尸潮的快捷键
            if (Input.GetKeyDown(testWaveKey))
            {
                TestTriggerZombieWave(); // 执行测试触发僵尸潮的方法
            }
            
            // 也监听GameManager中可能使用的X和C键，用于模拟GameManager发送的命令，以进行更全面的测试
            if (Input.GetKeyDown(KeyCode.X))
            {
                UnityEngine.Debug.Log("  [僵尸命令测试] 🔍 检测到X键按下，尝试模拟GameManager发送SpawnZombieHordeCommand...");
                TestGameManagerHordeCommand(); // 模拟GameManager的生成僵尸群命令
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                UnityEngine.Debug.Log("  [僵尸命令测试] 🔍 检测到C键按下，尝试模拟GameManager发送TriggerZombieWaveCommand...");
                TestGameManagerWaveCommand(); // 模拟GameManager的触发僵尸潮命令
            }
        }
        
        /// <summary>
        /// 测试生成僵尸群的功能。
        /// 它会尝试在鼠标当前位置生成一个中等威胁等级的僵尸群。
        /// 此方法同时演示了直接调用系统方法和通过发送命令两种方式。
        /// </summary>
        void TestSpawnZombieHorde()
        {
            UnityEngine.Debug.Log("=== [测试僵尸群生成命令] 开始执行 ===");
            
            if (zombieSystem == null) // 检查僵尸系统是否已初始化
            {
                UnityEngine.Debug.LogError("  ❌ [测试僵尸群] 失败：僵尸系统未初始化。");
                return;
            }
            
            // 获取鼠标当前在世界中的2D位置作为生成点
            Vector2 mousePos = Vector2.zero;
            if (Camera.main != null) // 确保主相机存在
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos = new Vector2(worldPos.x, worldPos.y);
            }
            UnityEngine.Debug.Log($"  🎯 预定生成位置 (鼠标位置): {mousePos}");
            
            try
            {
                // 方式1: 直接调用僵尸系统的SpawnZombieHorde方法 (如果系统接口允许直接调用)
                UnityEngine.Debug.Log("    方法1: 直接调用 IZombieSystem.SpawnZombieHorde (实际已注释，推荐使用命令)");
                // zombieSystem.SpawnZombieHorde(ZombieThreatLevel.Medium, mousePos); // 此行原被注释，保持注释
                
                // 方式2: 通过QFramework发送SpawnZombieHordeCommand命令 (推荐方式，解耦)
                UnityEngine.Debug.Log("    方法2: 发送 SpawnZombieHordeCommand 命令");
                this.SendCommand(new SpawnZombieHordeCommand(ZombieThreatLevel.Medium, mousePos));
                
                // 延迟1秒后调用CheckHordeResult方法，以检查生成结果 (因为生成可能是异步的)
                Invoke("CheckHordeResult", 1f);
            }
            catch (System.Exception e) // 捕获测试过程中可能发生的异常
            {
                UnityEngine.Debug.LogError($"  ❌ [测试僵尸群] 过程中发生错误: {e.Message}");
                UnityEngine.Debug.LogError($"  堆栈跟踪: {e.StackTrace}"); // 打印堆栈信息以便调试
            }
            UnityEngine.Debug.Log("=== [测试僵尸群生成命令] 执行完毕 ===");
        }
        
        /// <summary>
        /// 测试触发僵尸潮的功能。
        /// 它会发送一个TriggerZombieWaveCommand命令来请求触发一次高威胁等级的僵尸潮。
        /// </summary>
        void TestTriggerZombieWave()
        {
            UnityEngine.Debug.Log("=== [测试僵尸潮命令] 开始执行 ===");
            
            if (zombieSystem == null) // 检查僵尸系统
            {
                UnityEngine.Debug.LogError("  ❌ [测试僵尸潮] 失败：僵尸系统未初始化。");
                return;
            }
            
            try
            {
                UnityEngine.Debug.Log("    发送 TriggerZombieWaveCommand 命令 (高威胁等级)");
                this.SendCommand(new TriggerZombieWaveCommand(ZombieThreatLevel.High)); // 发送命令
                
                // 延迟2秒后检查结果
                Invoke("CheckWaveResult", 2f);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"  ❌ [测试僵尸潮] 过程中发生错误: {e.Message}");
                UnityEngine.Debug.LogError($"  堆栈跟踪: {e.StackTrace}");
            }
            UnityEngine.Debug.Log("=== [测试僵尸潮命令] 执行完毕 ===");
        }
        
        /// <summary>
        /// 模拟GameManager通过按X键发送SpawnZombieHordeCommand命令。
        /// </summary>
        void TestGameManagerHordeCommand()
        {
            UnityEngine.Debug.Log("  --- [模拟GameManager] 接收到X键，准备发送SpawnZombieHordeCommand ---");
            
            // 获取鼠标世界坐标作为生成中心点
            Vector3 mousePos = Vector3.zero;
            if(Camera.main != null) mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // 确保在2D平面
            UnityEngine.Debug.Log($"    模拟命令的鼠标世界坐标: {mousePos}");
            
            try
            {
                var command = new SpawnZombieHordeCommand(ZombieThreatLevel.Medium, mousePos); // 创建命令实例
                this.SendCommand(command); // 发送命令
                UnityEngine.Debug.Log("    ✅ [模拟GameManager] SpawnZombieHordeCommand 已成功发送。");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"    ❌ [模拟GameManager] 发送SpawnZombieHordeCommand时发生错误: {e.Message}");
            }
        }
        
        /// <summary>
        /// 模拟GameManager通过按C键发送TriggerZombieWaveCommand命令。
        /// </summary>
        void TestGameManagerWaveCommand()
        {
            UnityEngine.Debug.Log("  --- [模拟GameManager] 接收到C键，准备发送TriggerZombieWaveCommand ---");
            
            try
            {
                var command = new TriggerZombieWaveCommand(ZombieThreatLevel.High); // 创建命令实例
                this.SendCommand(command); // 发送命令
                UnityEngine.Debug.Log("    ✅ [模拟GameManager] TriggerZombieWaveCommand 已成功发送。");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"    ❌ [模拟GameManager] 发送TriggerZombieWaveCommand时发生错误: {e.Message}");
            }
        }
        
        /// <summary>
        /// 检查并记录僵尸群生成后的结果，如总僵尸数、僵尸群数量及各群信息。
        /// </summary>
        void CheckHordeResult()
        {
            UnityEngine.Debug.Log("  --- [结果检查] 正在检查僵尸群生成情况 ---");
            
            if (zombieSystem != null)
            {
                var zombieData = zombieSystem.GetZombieData(); // 获取僵尸数据容器
                UnityEngine.Debug.Log($"    当前僵尸总数: {zombieData.zombies.Count}");
                UnityEngine.Debug.Log($"    当前僵尸群(Horde)数量: {zombieData.hordes.Count}");
                
                // 遍历并打印每个僵尸群的详细信息
                foreach (var horde in zombieData.hordes.Values)
                {
                    UnityEngine.Debug.Log($"    僵尸群信息: ID='{horde.name}', 位置={horde.centerPosition}, 内部僵尸数={horde.ZombieCount}");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("  [结果检查] 僵尸系统未初始化，无法检查结果。");
            }
            UnityEngine.Debug.Log("  --- 结果检查完毕 ---");
        }
        
        /// <summary>
        /// 检查并记录僵尸潮触发后的结果。实际上是调用CheckHordeResult来查看生成的僵尸群情况。
        /// </summary>
        void CheckWaveResult()
        {
            UnityEngine.Debug.Log("  --- [结果检查] 正在检查僵尸潮触发情况 (通过查看僵尸群) ---");
            CheckHordeResult(); // 僵尸潮通常由多个僵尸群组成
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕右上角显示一个小的测试工具面板，包含操作按钮和状态信息。
        /// </summary>
        void OnGUI()
        {
            if (!enableDebugGUI) return; // 如果禁用了调试GUI，则不显示
            
            // 定义GUI区域在屏幕右上角
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 220), "", GUI.skin.box); // 调整了高度以容纳更多内容
            GUILayout.BeginVertical();
            
            // 面板标题
            GUILayout.Label("僵尸命令测试工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            
            // 功能按钮，并显示其对应的快捷键
            if (GUILayout.Button($"测试僵尸群生成 ({testHordeKey})")) { TestSpawnZombieHorde(); }
            if (GUILayout.Button($"测试僵尸潮触发 ({testWaveKey})")) { TestTriggerZombieWave(); }
            
            GUILayout.Space(10);
            GUILayout.Label("模拟GameManager原始命令:");
            if (GUILayout.Button("模拟X键 (生成僵尸群)")) { TestGameManagerHordeCommand(); }
            if (GUILayout.Button("模拟C键 (触发僵尸潮)")) { TestGameManagerWaveCommand(); }
            
            GUILayout.Space(10);
            
            // 显示当前僵尸系统的简要状态
            if (zombieSystem != null)
            {
                var data = zombieSystem.GetZombieData();
                GUILayout.Label($"当前僵尸总数: {data.zombies.Count}");
                GUILayout.Label($"当前僵尸群数: {data.hordes.Count}");
                GUILayout.Label($"当前威胁级别: {data.currentThreatLevel}");
            }
            else
            {
                // 如果系统未初始化，用红色文字提示
                GUILayout.Label("僵尸系统: 未初始化", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
            }
            
            GUILayout.EndVertical(); // 结束垂直布局
            GUILayout.EndArea();   // 结束IMGUI区域
        }
        
        /// <summary>
        /// 实现IController接口，返回QFramework的全局架构实例。
        /// </summary>
        public IArchitecture GetArchitecture() => RegisterManager.Interface; // RegisterManager是QFramework中用于获取架构实例的类
    }
}