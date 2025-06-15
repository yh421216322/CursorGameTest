// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieVisualizationDiagnostic.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 ZombieVisualizationDiagnostic 的 MonoBehaviour 类。
//     该脚本是一个调试工具，用于诊断游戏中僵尸数据层与视觉表现层之间的同步问题。
//     它提供了通过快捷键或IMGUI按钮触发的功能，如执行诊断、强制创建缺失的视觉对象、
//     以及同步可视化系统等，帮助开发者快速定位和解决僵尸显示不正确的问题。
// ==============================================================================

using System.Linq; // 用于LINQ查询，例如在DiagnoseZombieVisualization中获取部分僵尸数据
using UnityEngine;
using QFramework; // QFramework框架，用于获取系统实例
using SurvivalGame.GameSystem; // 包含IZombieSystem等游戏系统接口
using SurvivalGame.Visualization; // 包含IZombieVisualizationSystem可视化系统接口
using SurvivalGame.Model;         // 包含ZombieType等模型定义
using MyGameNamespace;    // 自定义命名空间 (当前在此文件中可能未使用特定内容)

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸可视化诊断工具。
    /// 用于检查游戏数据中的僵尸信息与场景中实际显示的可视化对象之间是否存在不同步或缺失的问题。
    /// 提供手动触发诊断、强制创建视觉对象和同步可视化等功能。
    /// </summary>
    public class ZombieVisualizationDiagnostic : MonoBehaviour, IController // 实现QFramework的IController接口
    {
        [Header("诊断快捷键设置")] // Inspector中分组显示
        public KeyCode diagnoseKey = KeyCode.F8;     // 触发诊断流程的快捷键
        public KeyCode forceCreateKey = KeyCode.F9;  // 强制创建缺失视觉对象的快捷键
        public KeyCode syncKey = KeyCode.F10;        // 同步可视化系统的快捷键
        
        // 系统接口引用
        private IZombieSystem zombieSystem;                 // 僵尸数据管理系统实例
        private IZombieVisualizationSystem visualizationSystem; // 僵尸视觉表现管理系统实例
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 初始化时，记录日志并尝试获取所需的游戏系统实例。
        /// </summary>
        void Start()
        {
            UnityEngine.Debug.Log("=== [僵尸可视化诊断工具] 已启动 ===");
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
                // 通过QFramework的this.GetSystem扩展方法获取系统实例
                zombieSystem = this.GetSystem<IZombieSystem>();
                UnityEngine.Debug.Log("  [僵尸可视化诊断] ✅ 僵尸系统 (IZombieSystem) 获取成功。");
            }
            catch (System.Exception e) // 捕获获取过程中可能发生的异常
            {
                UnityEngine.Debug.LogError($"  [僵尸可视化诊断] ❌ 获取僵尸系统时发生异常: {e.Message}");
            }
            
            try
            {
                visualizationSystem = this.GetSystem<IZombieVisualizationSystem>();
                UnityEngine.Debug.Log("  [僵尸可视化诊断] ✅ 僵尸可视化系统 (IZombieVisualizationSystem) 获取成功。");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"  [僵尸可视化诊断] ❌ 获取僵尸可视化系统时发生异常: {e.Message}");
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 监听用户按下的快捷键，以触发相应的诊断或修复功能。
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(diagnoseKey)) // 如果按下诊断快捷键
            {
                DiagnoseZombieVisualization(); // 执行可视化诊断流程
            }
            
            if (Input.GetKeyDown(forceCreateKey)) // 如果按下强制创建视觉对象快捷键
            {
                ForceCreateVisuals(); // 执行强制创建操作
            }
            
            if (Input.GetKeyDown(syncKey)) // 如果按下同步可视化系统快捷键
            {
                SynchronizeVisualization(); // 执行同步操作
            }
        }
        
        /// <summary>
        /// 执行僵尸可视化的诊断流程。
        /// 比较数据层僵尸信息与场景中实际可视化对象的数量和状态。
        /// </summary>
        [ContextMenu("执行僵尸可视化诊断")] // 使此方法可在Inspector中通过右键菜单调用
        void DiagnoseZombieVisualization()
        {
            UnityEngine.Debug.Log("=== [僵尸可视化诊断] 开始执行 ===");
            
            // 检查核心系统是否已成功初始化
            if (zombieSystem == null || visualizationSystem == null)
            {
                UnityEngine.Debug.LogError("  ❌ [诊断失败] 僵尸系统或可视化系统未初始化，无法进行诊断。");
                return;
            }
            
            // 1. 从僵尸系统获取数据层信息
            var zombieDataContainer = zombieSystem.GetZombieData();
            UnityEngine.Debug.Log($"  📊 [数据层] 僵尸总数 (Data): {zombieDataContainer.zombies.Count}");
            UnityEngine.Debug.Log($"  📊 [数据层] 僵尸群数 (Hordes): {zombieDataContainer.hordes.Count}");
            
            int aliveZombiesInData = 0; // 数据层存活僵尸计数
            foreach (var zombie in zombieDataContainer.zombies.Values)
            {
                if (zombie.IsAlive)
                {
                    aliveZombiesInData++;
                    UnityEngine.Debug.Log($"    🧟 数据僵尸 (Alive): 类型={zombie.type}, ID='{zombie.id}', 位置={zombie.position}, 状态={zombie.state}");
                }
            }
            UnityEngine.Debug.Log($"  📊 [数据层] 存活僵尸数 (Alive Data): {aliveZombiesInData}");
            
            // 2. 检查场景中实际的可视化对象 (基于名称简单识别)
            GameObject[] allSceneObjects = FindObjectsOfType<GameObject>();
            int visualZombieObjectsCount = 0; // 场景中找到的僵尸视觉对象计数
            
            UnityEngine.Debug.Log("  --- [场景可视化对象扫描] ---");
            foreach (var obj in allSceneObjects)
            {
                if (obj.name.ToLower().Contains("zombie")) // 简单通过名称包含"zombie"来识别
                {
                    visualZombieObjectsCount++;
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    UnityEngine.Debug.Log($"    🎮 发现场景可视化对象: '{obj.name}', 位置={obj.transform.position}, " +
                                          $"SpriteRenderer存在: {renderer != null}, Sprite名称: {(renderer?.sprite?.name ?? "无Sprite")}");
                }
            }
            UnityEngine.Debug.Log($"  📊 [场景层] 含'zombie'名称的对象总数: {visualZombieObjectsCount}");
            
            // 3. 通过可视化系统接口检查部分僵尸的视觉对象获取情况 (抽样检查前5个)
            UnityEngine.Debug.Log("  --- [可视化系统接口检查 - 抽样] ---");
            var aliveZombiesSample = zombieDataContainer.zombies.Values.Where(z => z.IsAlive).Take(5).ToList();
            if (aliveZombiesSample.Any()) {
                foreach (var zombie in aliveZombiesSample)
                {
                    var visualObj = visualizationSystem.GetZombieVisual(zombie.id);
                    UnityEngine.Debug.Log($"    🔍 检查数据僵尸 ID='{zombie.id}': 可视化系统返回对象='{(visualObj != null ? visualObj.name : "null (未找到)")}'");
                }
            } else {
                 UnityEngine.Debug.Log("    ℹ️ 数据层无存活僵尸，跳过可视化系统接口抽样检查。");
            }
            
            // 4. 根据收集到的信息给出诊断结论
            UnityEngine.Debug.Log("  --- [诊断结论] ---");
            if (aliveZombiesInData > 0 && visualZombieObjectsCount == 0)
            {
                UnityEngine.Debug.LogError("  ❌ 诊断结果：数据层存在存活僵尸，但场景中未找到任何名称含'zombie'的可视化对象！");
                UnityEngine.Debug.Log("     💡 可能原因：");
                UnityEngine.Debug.Log("        1. ZombieSpawnedEvent 事件未正确触发或可视化系统未监听到。");
                UnityEngine.Debug.Log("        2. 可视化系统的 CreateZombieVisual 方法未能成功创建GameObject。");
                UnityEngine.Debug.Log("        3. 创建的GameObject名称不包含'zombie'，导致本诊断脚本的扫描逻辑未能识别。");
                UnityEngine.Debug.Log("     🔧 建议操作：尝试按快捷键 'F9' (forceCreateKey) 强制为数据层僵尸创建可视化对象。");
            }
            else if (aliveZombiesInData == visualZombieObjectsCount && aliveZombiesInData > 0)
            {
                UnityEngine.Debug.Log("  ✅ 诊断结果：数据层存活僵尸数量与场景中名称含'zombie'的可视化对象数量匹配。");
            }
            else if (aliveZombiesInData == 0 && visualZombieObjectsCount == 0)
            {
                 UnityEngine.Debug.Log("  ℹ️ 诊断结果：数据层无存活僵尸，场景中也无相关可视化对象，状态一致。");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"  ⚠️ 诊断结果：数据层存活僵尸数量 ({aliveZombiesInData}) 与场景中可视化对象数量 ({visualZombieObjectsCount}) 不匹配。可能存在同步问题或部分视觉对象创建失败/未被正确清理。");
            }
            UnityEngine.Debug.Log("=== [僵尸可视化诊断] 执行完毕 ===");
        }
        
        /// <summary>
        /// 强制为数据层中所有存活但尚未拥有视觉对象的僵尸创建视觉对象。
        /// </summary>
        [ContextMenu("强制创建缺失的视觉对象")]
        void ForceCreateVisuals()
        {
            UnityEngine.Debug.Log("=== [强制创建视觉对象] 开始执行 ===");
            
            if (zombieSystem == null || visualizationSystem == null)
            {
                UnityEngine.Debug.LogError("  ❌ [强制创建失败] 核心系统未初始化。");
                return;
            }
            
            var zombieDataContainer = zombieSystem.GetZombieData();
            int createdCount = 0; // 记录本次操作成功创建的视觉对象数量
            
            foreach (var zombie in zombieDataContainer.zombies.Values) // 遍历数据层所有僵尸
            {
                if (zombie.IsAlive) // 只处理存活的僵尸
                {
                    var existingVisual = visualizationSystem.GetZombieVisual(zombie.id); // 检查是否已存在视觉对象
                    if (existingVisual == null) // 如果不存在
                    {
                        try
                        {
                            // 调用可视化系统的方法创建视觉对象
                            visualizationSystem.CreateZombieVisual(zombie);
                            createdCount++;
                            UnityEngine.Debug.Log($"    ✅ [强制创建] 为僵尸 ID='{zombie.id}' (类型: {zombie.type}) 成功创建了视觉对象。");
                        }
                        catch (System.Exception e) // 捕获创建过程中可能发生的异常
                        {
                            UnityEngine.Debug.LogError($"    ❌ [强制创建] 为僵尸 ID='{zombie.id}' 创建视觉对象时发生错误: {e.Message}");
                        }
                    }
                }
            }
            
            UnityEngine.Debug.Log($"  🎯 [强制创建] 操作完成，共为 {createdCount} 个僵尸创建了视觉对象。");
            UnityEngine.Debug.Log("=== [强制创建视觉对象] 执行完毕 ===");
        }
        
        /// <summary>
        /// 请求可视化系统更新所有僵尸的视觉表现。
        /// </summary>
        [ContextMenu("同步可视化系统状态")]
        void SynchronizeVisualization()
        {
            UnityEngine.Debug.Log("=== [同步可视化系统] 开始执行 ===");
            
            if (visualizationSystem == null)
            {
                UnityEngine.Debug.LogError("  ❌ [同步失败] 可视化系统未初始化。");
                return;
            }
            
            try
            {
                visualizationSystem.UpdateAllZombieVisuals(); // 调用可视化系统的全局更新方法
                UnityEngine.Debug.Log("  ✅ [同步可视化系统] UpdateAllZombieVisuals 调用成功。");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"  ❌ [同步可视化系统] 调用时发生错误: {e.Message}");
            }
            UnityEngine.Debug.Log("=== [同步可视化系统] 执行完毕 ===");
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕右下角显示一个包含诊断操作按钮和简要状态信息的面板。
        /// </summary>
        void OnGUI()
        {
            // 定义GUI区域在屏幕右下角
            GUILayout.BeginArea(new Rect(Screen.width - 300, Screen.height - 220, 290, 210), "", GUI.skin.box); // 调整了高度
            GUILayout.BeginVertical();
            
            // 面板标题
            GUILayout.Label("僵尸可视化诊断工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            
            // 功能按钮，并显示其对应的快捷键
            if (GUILayout.Button($"执行诊断 ({diagnoseKey})")) { DiagnoseZombieVisualization(); }
            if (GUILayout.Button($"强制创建视觉对象 ({forceCreateKey})")) { ForceCreateVisuals(); }
            if (GUILayout.Button($"同步可视化系统 ({syncKey})")) { SynchronizeVisualization(); }
            
            GUILayout.Space(10); // 添加一些垂直间距
            
            // 显示当前游戏状态相关的简要信息
            GUILayout.Label("--- 实时状态 ---", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter });
            if (zombieSystem != null)
            {
                var data = zombieSystem.GetZombieData();
                GUILayout.Label($"数据层僵尸数: {data.zombies.Count} (活跃: {zombieSystem.GetActiveZombieCount()})");
                
                // 统计场景中的实际视觉对象数量 (基于名称)
                GameObject[] allSceneObjects = FindObjectsOfType<GameObject>(); // 注意：频繁调用此方法可能影响性能
                int visualCountInScene = 0;
                foreach (var obj in allSceneObjects)
                {
                    if (obj.name.ToLower().Contains("zombie")) visualCountInScene++;
                }
                GUILayout.Label($"场景中视觉对象数: {visualCountInScene}");
                
                // 根据数量匹配情况显示一个简单的状态指示
                string statusText;
                Color statusColor;
                if (zombieSystem.GetActiveZombieCount() > 0 && visualCountInScene == 0)
                {
                    statusText = "状态: 异常 (数据有但无视觉)";
                    statusColor = Color.red;
                }
                else if (zombieSystem.GetActiveZombieCount() == visualCountInScene)
                {
                    statusText = "状态: 正常 (数量匹配)";
                    statusColor = Color.green;
                }
                else
                {
                    statusText = "状态: 可能不同步";
                    statusColor = Color.yellow;
                }
                GUILayout.Label(statusText, new GUIStyle(GUI.skin.label) { normal = { textColor = statusColor } });
            }
            else
            {
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