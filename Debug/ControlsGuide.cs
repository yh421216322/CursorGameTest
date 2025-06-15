using UnityEngine;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 控制说明界面
    /// 显示所有可用的按键控制
    /// </summary>
    public class ControlsGuide : MonoBehaviour
    {
        [Header("显示设置")]
        public bool showControls = true;
        public KeyCode toggleKey = KeyCode.H;
        
        private bool isVisible = false;
        private Vector2 scrollPosition = Vector2.zero;
        
        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
            }
        }
        
        void OnGUI()
        {
            if (!showControls && !isVisible) return;
            
            // 右上角的帮助按钮
            if (!isVisible)
            {
                if (GUI.Button(new Rect(Screen.width - 100, 10, 90, 30), $"帮助 ({toggleKey})"))
                {
                    isVisible = true;
                }
                return;
            }
            
            // 控制说明窗口
            float windowWidth = 400f;
            float windowHeight = 500f;
            Rect windowRect = new Rect(
                (Screen.width - windowWidth) / 2, 
                (Screen.height - windowHeight) / 2, 
                windowWidth, 
                windowHeight
            );
            
            GUI.Window(0, windowRect, DrawControlsWindow, "游戏控制说明");
        }
        
        void DrawControlsWindow(int windowID)
        {
            GUILayout.BeginVertical();
            
            // 标题
            GUILayout.Label("🎮 游戏控制指南", new GUIStyle(GUI.skin.label) 
            { 
                fontStyle = FontStyle.Bold, 
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            });
            
            GUILayout.Space(10);
            
            // 滚动区域
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
            
            // 僵尸控制
            DrawSection("🧟 僵尸控制", new string[]
            {
                "Z - 在鼠标位置生成单个僵尸",
                "X - 在鼠标位置生成僵尸群",
                "C - 触发全局僵尸潮",
                "V - 强制更新威胁等级",
                "B - 清理死亡僵尸"
            });
            
            // 防御控制
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
            
            // 建筑控制
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
            
            // 科技控制
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
            
            // 调试控制
            DrawSection("🔧 调试控制", new string[]
            {
                "J - 在固定位置生成测试僵尸",
                "K - 清理所有僵尸",
                "F5 - 运行系统诊断",
                "F6 - 测试僵尸群生成命令",
                "F7 - 测试僵尸潮命令",
                "I - 测试僵尸图片应用",
                "Delete - 清理测试对象",
                "H - 显示/隐藏此帮助"
            });
            
            // 摄像机控制
            DrawSection("📷 摄像机控制", new string[]
            {
                "WASD - 移动摄像机",
                "鼠标滚轮 - 缩放",
                "Space - 重置摄像机位置"
            });
            
            GUILayout.EndScrollView();
            
            // 关闭按钮
            GUILayout.Space(10);
            if (GUILayout.Button("关闭", GUILayout.Height(30)))
            {
                isVisible = false;
            }
            
            GUILayout.EndVertical();
            
            GUI.DragWindow();
        }
        
        void DrawSection(string title, string[] controls)
        {
            GUILayout.Space(10);
            GUILayout.Label(title, new GUIStyle(GUI.skin.label) 
            { 
                fontStyle = FontStyle.Bold,
                fontSize = 14
            });
            
            foreach (string control in controls)
            {
                GUILayout.Label($"  • {control}", GUI.skin.label);
            }
        }
    }
} 