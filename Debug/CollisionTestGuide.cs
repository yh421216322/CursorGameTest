// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：CollisionTestGuide.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 CollisionTestGuide 的 MonoBehaviour 类。
//     该类的主要功能是在游戏运行时，通过控制台输出和屏幕GUI的方式，
//     为测试人员提供一份关于如何测试僵尸碰撞检测及避障功能的详细指南。
//     这有助于确保相关游戏机制按预期工作。
// ==============================================================================

using UnityEngine;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸碰撞检测功能测试指南。
    /// 此脚本在游戏开始时会在控制台打印详细的测试步骤和说明，
    /// 并在屏幕上通过IMGUI显示一个简要的快捷键提示面板，以辅助测试僵尸的避障和碰撞行为。
    /// </summary>
    public class CollisionTestGuide : MonoBehaviour
    {
        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// </summary>
        void Start()
        {
            // 游戏开始时，在控制台显示详细的测试指南信息。
            ShowTestGuide();
        }
        
        /// <summary>
        /// 在Unity控制台打印详细的僵尸碰撞检测功能测试指南。
        /// 指南内容包括新增功能点、具体测试步骤、观察要点、诊断工具使用方法、
        /// 注意事项以及测试成功的标准。
        /// </summary>
        void ShowTestGuide()
        {
            // 使用UnityEngine.Debug.Log输出多行文本到控制台，形成测试指南。
            UnityEngine.Debug.Log("========================================");
            UnityEngine.Debug.Log("=== 僵尸碰撞检测功能测试指南 ===");
            UnityEngine.Debug.Log("========================================");
            UnityEngine.Debug.Log(""); // 空行用于分隔
            UnityEngine.Debug.Log("🎯 本次测试的核心功能点：");
            UnityEngine.Debug.Log("  1. 僵尸在生成时不应与现有建筑物重叠。");
            UnityEngine.Debug.Log("  2. 僵尸在移动过程中应能有效绕过建筑和围墙等障碍物。");
            UnityEngine.Debug.Log("  3. 多个僵尸同时存在时，它们之间不应发生物理重叠或穿透。");
            UnityEngine.Debug.Log("  4. 僵尸在选择游荡目标点时，应能避开已知障碍物区域。");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("🧪 建议测试步骤：");
            UnityEngine.Debug.Log("  步骤1: 场景准备 - 建造多种障碍物");
            UnityEngine.Debug.Log("    - 使用数字键 1-9 建造不同类型的建筑。");
            UnityEngine.Debug.Log("    - 特别注意使用数字键 4 建造围墙，以形成明确的阻挡区域。");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("  步骤2: 生成僵尸 - 测试不同情况下的避障");
            UnityEngine.Debug.Log("    - 按 J 键：在鼠标位置生成单个僵尸，观察其初始位置是否合理及后续移动。");
            UnityEngine.Debug.Log("    - 按 X 键：在鼠标位置生成一小群僵尸，观察群体行为和相互避让。");
            UnityEngine.Debug.Log("    - 按 C 键：触发一次大规模僵尸潮，全面测试在高密度情况下的碰撞与寻路。");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("  步骤3: 行为观察 - 仔细检查僵尸的移动和交互");
            UnityEngine.Debug.Log("    - 僵尸是否能够平滑地绕过建筑物的边缘进行移动？");
            UnityEngine.Debug.Log("    - 僵尸之间是否保持了一定的距离，没有出现重叠或“卡”在一起的现象？");
            UnityEngine.Debug.Log("    - 当僵尸的路径被围墙完全阻挡时，它们是否会尝试寻找其他路径，还是会卡在原地？");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("  步骤4: (可选) 使用内置诊断工具辅助观察");
            UnityEngine.Debug.Log("    - 按 F8 键：启动或切换可视化诊断模式（具体功能需查看相关诊断脚本）。");
            UnityEngine.Debug.Log("    - 按 F9 键：强制创建或刷新可视化调试对象（例如寻路路径、碰撞体范围等）。");
            UnityEngine.Debug.Log("    - 按 F10 键：同步或更新可视化系统的数据。");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("⚠️ 测试时注意事项：");
            UnityEngine.Debug.Log("  - 如果生成僵尸后在视野内看不到，它们可能被生成在了地图的较远边缘区域。");
            UnityEngine.Debug.Log("  - 使用 WASD 键移动主相机来寻找和跟踪僵尸。");
            UnityEngine.Debug.Log("  - 使用鼠标滚轮来缩放相机视野，以便观察更大范围或特定细节。");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("✅ 测试成功标准：");
            UnityEngine.Debug.Log("  1. 僵尸在任何情况下都不会直接穿过建筑物或围墙。");
            UnityEngine.Debug.Log("  2. 僵尸个体之间能维持清晰的边界，不会出现模型重叠。");
            UnityEngine.Debug.Log("  3. 当路径被围墙等障碍物阻挡时，僵尸能够表现出绕行或重新寻路的尝试。");
            UnityEngine.Debug.Log("  4. 僵尸的生成位置始终在可通行的区域，不会直接生成在障碍物内部。");
            UnityEngine.Debug.Log("========================================");
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕左下角显示一个简要的快捷键指南。
        /// </summary>
        void OnGUI()
        {
            // 定义GUI区域在屏幕左下角
            GUILayout.BeginArea(new Rect(10, Screen.height - 250, 400, 240)); // (x, y, width, height)
            GUILayout.BeginVertical("box"); // 使用"box"样式创建一个带边框的垂直布局组
            
            // 标题，使用加粗字体
            GUILayout.Label("僵尸碰撞测试快捷指南", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 });
            GUILayout.Space(5); // 添加一些垂直间距
            
            // 分类列出快捷键
            GUILayout.Label("建筑快捷键:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label("  1-避难所, 2-农田, 3-工坊, 4-围墙, 5-瞭望塔 ..."); // 提示部分建筑快捷键
            
            GUILayout.Space(5);
            GUILayout.Label("僵尸生成快捷键:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label("  J - 生成单个僵尸 (鼠标位置)");
            GUILayout.Label("  X - 生成僵尸群 (鼠标位置)");
            GUILayout.Label("  C - 触发大规模僵尸潮");
            
            GUILayout.Space(5);
            GUILayout.Label("诊断工具快捷键 (可选):", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label("  F8 - 诊断可视化, F9 - 强制创建, F10 - 同步");
            
            GUILayout.Space(5);
            GUILayout.Label("相机控制:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label("  WASD - 移动相机, 鼠标滚轮 - 缩放视野");
            
            GUILayout.Space(10);
            // 添加一个按钮，允许用户在需要时重新在控制台打印详细指南
            if (GUILayout.Button("在控制台重新显示详细指南"))
            {
                ShowTestGuide(); // 点击按钮时调用方法
            }
            
            GUILayout.EndVertical(); // 结束垂直布局组
            GUILayout.EndArea();   // 结束GUI区域
        }
    }
}