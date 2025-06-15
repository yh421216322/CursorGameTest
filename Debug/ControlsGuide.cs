// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ControlsGuide.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 ControlsGuide 的 MonoBehaviour 类。
//     该脚本用于在游戏运行时通过IMGUI显示一个可交互的控制说明窗口。
//     玩家可以通过按键（默认为H）或点击屏幕上的“帮助”按钮来显示/隐藏此窗口。
//     窗口内会列出游戏中各种操作的快捷键，方便玩家（尤其是测试人员）查阅。
// ==============================================================================

using UnityEngine;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 游戏内控制说明界面。
    /// 通过IMGUI显示一个包含所有可用按键控制的窗口，方便玩家和测试人员查阅。
    /// 可以通过指定的快捷键或屏幕上的按钮来切换显示状态。
    /// </summary>
    public class ControlsGuide : MonoBehaviour
    {
        [Header("显示设置")] // Inspector中分组显示相关设置
        public bool showControls = true;    // 是否在游戏开始时默认显示右上角的“帮助”按钮
        public KeyCode toggleKey = KeyCode.H; // 用于切换控制说明窗口显示/隐藏状态的快捷键，默认为 H 键

        private bool isVisible = false;      // 当前控制说明窗口是否可见
        private Vector2 scrollPosition = Vector2.zero; // 用于窗口内部滚动视图的滚动位置
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 用于检测切换快捷键的按下事件，以改变窗口的可见状态。
        /// </summary>
        void Update()
        {
            // 如果玩家按下了指定的切换快捷键 (toggleKey)
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible; // 切换 isVisible 状态 (显示变隐藏，隐藏变显示)
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 此方法根据 isVisible 和 showControls 状态来决定是否绘制“帮助”按钮或完整的控制说明窗口。
        /// </summary>
        void OnGUI()
        {
            // 如果 showControls 为 false 且窗口当前也不可见，则不绘制任何内容
            if (!showControls && !isVisible) return;
            
            // 如果窗口当前不可见 (但 showControls 可能为 true)
            if (!isVisible)
            {
                // 在屏幕右上角绘制一个“帮助”按钮，点击可显示完整指南窗口
                if (GUI.Button(new Rect(Screen.width - 100, 10, 90, 30), $"帮助 ({toggleKey})"))
                {
                    isVisible = true; // 点击后将窗口设为可见
                }
                return; // 只绘制帮助按钮，不绘制窗口
            }
            
            // ---- 如果 isVisible 为 true，则绘制完整的控制说明窗口 ----

            // 定义窗口的尺寸和位置（居中显示）
            float windowWidth = 400f;
            float windowHeight = 500f;
            Rect windowRect = new Rect(
                (Screen.width - windowWidth) / 2,    // X 位置 (居中)
                (Screen.height - windowHeight) / 2,  // Y 位置 (居中)
                windowWidth,                         // 宽度
                windowHeight                         // 高度
            );
            
            // 创建一个IMGUI窗口。GUI.Window会调用 DrawControlsWindow 方法来填充窗口内容。
            // 0 是窗口的ID， "游戏控制说明" 是窗口标题。
            GUI.Window(0, windowRect, DrawControlsWindow, "游戏控制说明");
        }
        
        /// <summary>
        /// IMGUI窗口的绘制回调函数，由GUI.Window调用。
        /// 此方法负责绘制控制说明窗口内的所有内容。
        /// </summary>
        /// <param name="windowID">窗口的唯一ID (此例中为0)。</param>
        void DrawControlsWindow(int windowID)
        {
            GUILayout.BeginVertical(); // 开始垂直布局组，后续元素会垂直排列
            
            // 绘制窗口大标题
            GUILayout.Label("🎮 游戏控制指南", new GUIStyle(GUI.skin.label) 
            { 
                fontStyle = FontStyle.Bold, // 设置字体为粗体
                fontSize = 16,              // 设置字体大小
                alignment = TextAnchor.MiddleCenter // 文本居中对齐
            });
            
            GUILayout.Space(10); // 在标题和滚动区域之间添加一些垂直间距
            
            // 开始一个滚动视图区域，以容纳可能超出窗口高度的内容
            // GUILayout.Height(400) 指定了滚动视图的高度
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
            
            // 调用DrawSection方法为不同类别的控制分别绘制说明区域
            DrawSection("🧟 僵尸控制", new string[]
            {
                "Z - 在鼠标位置生成单个僵尸",
                "X - 在鼠标位置生成僵尸群",
                "C - 触发全局僵尸潮",
                "V - 强制更新威胁等级",
                "B - 清理死亡僵尸"
            });
            
            DrawSection("🏰 防御控制", new string[]
            {
                "T - 在鼠标位置建造基础防御塔",
                "Y - 在鼠标位置建造重型防御塔",
                "U - 在鼠标位置建造狙击塔",
                "I - 在鼠标位置建造溅射塔",
                "O - 显示防御统计",
                "P - 显示所有防御塔信息",
                "N - 批量修理所有防御塔"
            });
            
            DrawSection("🏗️ 建筑控制", new string[]
            {
                "1 - 建造庇护所",
                "2 - 建造农场",
                "3 - 建造工坊",
                "4 - 建造围墙",
                "5 - 建造瞭望塔",
                "6 - 建造医疗站",
                "7 - 建造采石场",
                "8 - 建造发电厂"
            });
            
            DrawSection("🔬 科技控制", new string[]
            {
                "R - 开始研究基础农业",
                "E - 开始研究基础工艺",
                "Q - 开始研究基础医学",
                "F - 快速完成当前研究",
                "G - 添加研究点数",
                "Ctrl+R - 快速研究基础科技",
                "Ctrl+E - 快速研究进阶科技"
            });
            
            DrawSection("🔧 调试控制", new string[]
            {
                "J - 在固定位置生成测试僵尸",
                "K - 清理所有僵尸",
                "F5 - 运行系统诊断",
                "F6 - 测试僵尸群生成命令",
                "F7 - 测试僵尸潮命令",
                "I - 测试僵尸图片应用", // 注意：此条目与防御控制中的'I'键有冲突，需在实际使用中区分或修改
                "Delete - 清理测试对象",
                "H - 显示/隐藏此帮助" // 对应toggleKey
            });
            
            DrawSection("📷 摄像机控制", new string[]
            {
                "WASD - 移动摄像机",
                "鼠标滚轮 - 缩放",
                "Space - 重置摄像机位置" // 注意：Space键通常用于暂停，此处可能有冲突
            });
            
            GUILayout.EndScrollView(); // 结束滚动视图区域
            
            // 在滚动区域下方添加一个关闭按钮
            GUILayout.Space(10);
            if (GUILayout.Button("关闭", GUILayout.Height(30))) // GUILayout.Height设置按钮高度
            {
                isVisible = false; // 点击关闭按钮后，将窗口设为不可见
            }
            
            GUILayout.EndVertical(); // 结束主垂直布局组
            
            // 允许通过拖拽窗口的任意空白区域（通常是标题栏，但此处是整个窗口）来移动窗口
            GUI.DragWindow();
        }
        
        /// <summary>
        /// 绘制单个控制说明区域的辅助方法。
        /// </summary>
        /// <param name="title">该区域的标题（例如“僵尸控制”）。</param>
        /// <param name="controls">一个包含该区域所有控制说明的字符串数组。</param>
        void DrawSection(string title, string[] controls)
        {
            GUILayout.Space(10); // 在每个区域前添加一些间距
            // 绘制区域标题，使用加粗和稍大字号
            GUILayout.Label(title, new GUIStyle(GUI.skin.label) 
            { 
                fontStyle = FontStyle.Bold,
                fontSize = 14
            });
            
            // 遍历并绘制该区域的每条控制说明
            foreach (string control in controls)
            {
                // 每条说明前加一个项目符号 "•" 并缩进显示
                GUILayout.Label($"  • {control}", GUI.skin.label);
            }
        }
    }
}