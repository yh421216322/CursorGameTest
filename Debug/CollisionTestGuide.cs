using UnityEngine;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸碰撞检测功能测试指南
    /// 请按照此指南测试僵尸避障和碰撞检测功能
    /// </summary>
    public class CollisionTestGuide : MonoBehaviour
    {
        void Start()
        {
            ShowTestGuide();
        }
        
        void ShowTestGuide()
        {
            UnityEngine.Debug.Log("=== 僵尸碰撞检测功能测试指南 ===");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("🎯 新增功能：");
            UnityEngine.Debug.Log("1. 僵尸生成时避开建筑物");
            UnityEngine.Debug.Log("2. 僵尸移动时绕过建筑和围墙");
            UnityEngine.Debug.Log("3. 僵尸之间不会重叠");
            UnityEngine.Debug.Log("4. 僵尸游荡目标避开障碍物");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("🧪 测试步骤：");
            UnityEngine.Debug.Log("步骤1: 建造围墙和建筑");
            UnityEngine.Debug.Log("  - 按数字键1-9建造不同建筑");
            UnityEngine.Debug.Log("  - 按4键建造围墙来阻挡僵尸");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("步骤2: 生成僵尸测试避障");
            UnityEngine.Debug.Log("  - 按J键生成单个僵尸");
            UnityEngine.Debug.Log("  - 按X键生成僵尸群");
            UnityEngine.Debug.Log("  - 按C键触发僵尸潮");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("步骤3: 观察僵尸行为");
            UnityEngine.Debug.Log("  - 僵尸应该绕过建筑移动");
            UnityEngine.Debug.Log("  - 僵尸不应该重叠在一起");
            UnityEngine.Debug.Log("  - 僵尸被围墙阻挡时会寻找其他路径");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("步骤4: 使用诊断工具");
            UnityEngine.Debug.Log("  - 按F8键诊断可视化问题");
            UnityEngine.Debug.Log("  - 按F9键强制创建可视化对象");
            UnityEngine.Debug.Log("  - 按F10键同步可视化系统");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("⚠️ 注意事项：");
            UnityEngine.Debug.Log("- 如果看不到僵尸，可能是生成在地图边缘");
            UnityEngine.Debug.Log("- 使用WASD移动相机查看僵尸");
            UnityEngine.Debug.Log("- 滚轮缩放相机查看更广范围");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("✅ 测试成功标准：");
            UnityEngine.Debug.Log("1. 僵尸不会穿过建筑");
            UnityEngine.Debug.Log("2. 僵尸不会重叠");
            UnityEngine.Debug.Log("3. 僵尸被围墙阻挡会绕行");
            UnityEngine.Debug.Log("4. 僵尸生成位置合理");
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 250, 400, 240));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("碰撞检测测试指南", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            GUILayout.Label("建筑快捷键:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label("1-避难所 2-农田 3-工坊 4-围墙 5-瞭望塔");
            
            GUILayout.Space(5);
            GUILayout.Label("僵尸测试键:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label("J-单个僵尸 X-僵尸群 C-僵尸潮");
            
            GUILayout.Space(5);
            GUILayout.Label("诊断工具:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label("F8-诊断 F9-强制创建 F10-同步");
            
            GUILayout.Space(5);
            GUILayout.Label("相机控制:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label("WASD-移动 滚轮-缩放");
            
            if (GUILayout.Button("重新显示指南"))
            {
                ShowTestGuide();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
} 