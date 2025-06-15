// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：SceneCleanup.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 SceneCleanup 的 MonoBehaviour 类。
//     该脚本提供了一个用于在Unity编辑器或运行时清理场景中特定测试对象的工具。
//     它可以自动在启动时清理，或通过快捷键、上下文菜单以及IMGUI按钮手动触发清理。
//     主要目的是移除临时的、损坏的或仅用于测试的GameObject，以保持场景整洁，
//     确保测试环境的一致性，或为正式运行做准备。
// ==============================================================================

using UnityEngine;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 场景清理工具。
    /// 提供功能来自动或手动清理场景中所有被识别为测试或临时对象的GameObject。
    /// 这有助于确保场景中只保留由游戏系统管理的、有效的僵尸（或其他）对象。
    /// </summary>
    public class SceneCleanup : MonoBehaviour
    {
        [Header("清理设置")] // Inspector中分组显示
        public KeyCode cleanupKey = KeyCode.Delete; // 用于手动触发场景清理的快捷键，默认为 Delete 键
        public bool autoCleanOnStart = true;      // 是否在游戏开始时自动执行一次场景清理

        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 如果 autoCleanOnStart 为true，则执行场景清理。
        /// </summary>
        void Start()
        {
            if (autoCleanOnStart)
            {
                UnityEngine.Debug.Log("=== [场景清理] 自动清理已启动 ===");
                CleanupScene(); // 调用主要的场景清理方法
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 检测是否按下了指定的清理快捷键 (cleanupKey)。
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(cleanupKey)) // 如果按下了清理快捷键
            {
                CleanupScene(); // 执行场景清理
            }
        }
        
        /// <summary>
        /// 执行场景清理操作。
        /// 查找场景中所有GameObject，并根据一系列规则（名称包含特定子串、组件缺失等）
        /// 来识别并销毁测试对象或损坏的对象。
        /// 此方法也可以通过Unity编辑器的上下文菜单（右键点击脚本组件）调用。
        /// </summary>
        [ContextMenu("执行场景清理 (Cleanup Scene)")] // 使此方法在Inspector中可被右键调用
        public void CleanupScene()
        {
            UnityEngine.Debug.Log("[场景清理] 开始扫描并清理场景中的测试及损坏对象...");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>(); // 获取场景中所有的GameObject
            int clearedCount = 0; // 记录本次清理掉的对象数量
            
            // 遍历场景中的每一个GameObject
            foreach (var obj in allObjects)
            {
                bool shouldDelete = false; // 标记当前对象是否应该被删除
                string reason = "";        // 删除原因，用于日志记录
                
                // --- 定义删除规则 ---
                // 规则1: 名称包含特定测试标记的简单对象
                if (obj.name.Contains("TestSprite")) { shouldDelete = true; reason = "静态测试图片对象"; }
                else if (obj.name.Contains("TestSquare")) { shouldDelete = true; reason = "测试方块对象"; }
                else if (obj.name.Contains("TempZombie")) { shouldDelete = true; reason = "临时僵尸对象 (TempZombie)"; }
                // 规则2: 针对名称包含 "zombie" (不区分大小写) 的对象进行更细致的检查，以识别损坏的僵尸对象
                else if (obj.name.ToLower().Contains("zombie"))
                {
                    var renderer = obj.GetComponent<SpriteRenderer>(); // 获取其SpriteRenderer组件
                    if (renderer == null) // 如果没有SpriteRenderer，则认为是损坏的
                    {
                        shouldDelete = true;
                        reason = "疑似损坏的僵尸对象 (缺少SpriteRenderer组件)";
                    }
                    else if (renderer.sprite == null) // 如果有SpriteRenderer但没有Sprite，也认为是损坏的
                    {
                        shouldDelete = true;
                        reason = "疑似损坏的僵尸对象 (SpriteRenderer组件上缺少Sprite)";
                    }
                    // 示例：如果Sprite名称包含特定前缀且颜色为纯黑，可能代表未正确初始化的僵尸
                    else if (renderer.sprite.name.Contains("ZombieSprite_") && renderer.color == Color.black)
                    {
                        shouldDelete = true;
                        reason = "疑似未初始化或默认状态的黑色僵尸对象";
                    }
                }
                
                // 如果根据以上任一规则，对象被标记为应删除
                if (shouldDelete)
                {
                    UnityEngine.Debug.Log($"[场景清理] 正在清理对象: '{obj.name}' - 原因: {reason}");
                    // 使用DestroyImmediate是因为此脚本可能在编辑器模式下通过ContextMenu运行，
                    // 或者在运行时需要立即移除对象以避免影响后续逻辑。
                    // 注意：在运行时频繁使用DestroyImmediate可能影响性能，常规销毁应使用Destroy()。
                    DestroyImmediate(obj);
                    clearedCount++; // 增加已清理对象的计数
                }
            }
            
            UnityEngine.Debug.Log($"[场景清理] 清理操作完成，共移除了 {clearedCount} 个对象。");
            
            // 清理完成后，调用方法在控制台显示当前场景中剩余的有效僵尸信息
            ShowRemainingZombies();
        }
        
        /// <summary>
        /// 在控制台显示当前场景中所有被识别为“有效”的僵尸对象的信息。
        /// “有效”通常指具有SpriteRenderer和Sprite的僵尸。
        /// </summary>
        void ShowRemainingZombies()
        {
            UnityEngine.Debug.Log("--- [场景清理] 检查剩余的有效僵尸对象 ---");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>(); // 再次获取场景中所有对象
            int validZombieCount = 0; // 记录有效僵尸的数量
            
            foreach (var obj in allObjects)
            {
                // 检查名称是否包含 "zombie" (不区分大小写)
                if (obj.name.ToLower().Contains("zombie"))
                {
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    // 如果对象有SpriteRenderer且其上有Sprite，则认为是有效的僵尸视觉对象
                    if (renderer != null && renderer.sprite != null)
                    {
                        validZombieCount++;
                        UnityEngine.Debug.Log($"  ✅ 有效僵尸: '{obj.name}', 使用Sprite: '{renderer.sprite.name}', " +
                                              $"位置: {obj.transform.position}, 颜色: {renderer.color}");
                    }
                }
            }
            
            if (validZombieCount == 0)
            {
                UnityEngine.Debug.Log("  ℹ️ 当前场景中没有检测到有效的僵尸对象。");
            }
            else
            {
                UnityEngine.Debug.Log($"  ℹ️ 当前场景中共有 {validZombieCount} 个有效的僵尸对象。");
            }
            UnityEngine.Debug.Log("--- 检查完毕 ---");
        }
        
        /// <summary>
        /// 强制清理所有名称中包含 "zombie", "TestSprite", 或 "TestSquare" 的GameObject。
        /// 这是一个更广泛的清理操作，可能移除一些非预期对象，请谨慎使用。
        /// </summary>
        [ContextMenu("强制清理所有僵尸及测试对象")]
        public void ForceCleanAllZombies()
        {
            UnityEngine.Debug.LogWarning("[场景清理] 正在执行强制清理操作，将移除所有名称匹配特定模式的对象...");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int clearedCount = 0;
            
            foreach (var obj in allObjects)
            {
                string objNameLower = obj.name.ToLower(); // 转换为小写以便不区分大小写比较
                if (objNameLower.Contains("zombie") ||
                    objNameLower.Contains("testsprite") || // TestSprite也转为小写比较
                    objNameLower.Contains("testsquare"))   // TestSquare也转为小写比较
                {
                    UnityEngine.Debug.Log($"[场景清理] 强制清理对象: '{obj.name}'");
                    DestroyImmediate(obj);
                    clearedCount++;
                }
            }
            
            UnityEngine.Debug.Log($"[场景清理] 强制清理完成，共移除了 {clearedCount} 个对象。");
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕右下角显示一个小的场景清理工具面板，包含操作按钮和对象统计。
        /// </summary>
        void OnGUI()
        {
            // 定义GUI区域在屏幕右下角
            GUILayout.BeginArea(new Rect(Screen.width - 200, Screen.height - 170, 190, 160), "", GUI.skin.box);
            GUILayout.BeginVertical();
            
            // 面板标题
            GUILayout.Label("场景清理工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            
            // 清理测试对象按钮，显示其快捷键
            if (GUILayout.Button($"清理测试对象 ({cleanupKey})"))
            {
                CleanupScene();
            }
            
            // 强制清理所有僵尸按钮
            if (GUILayout.Button("强制清理所有僵尸"))
            {
                ForceCleanAllZombies();
            }
            
            // 显示剩余僵尸信息按钮
            if (GUILayout.Button("日志显示剩余僵尸"))
            {
                ShowRemainingZombies();
            }
            
            GUILayout.Space(5);
            // 统计并显示当前场景中的僵尸和测试对象数量
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int zombieCount = 0;
            int testObjectCount = 0; // 更准确的命名
            
            foreach (var obj in allObjects)
            {
                string objNameLower = obj.name.ToLower();
                if (objNameLower.Contains("zombie")) zombieCount++;
                // "Test"作为测试对象的一个通用标识，可能需要更精确的规则
                else if (objNameLower.Contains("test")) testObjectCount++;
            }
            
            GUILayout.Label($"场景中僵尸对象: {zombieCount}");
            GUILayout.Label($"场景中测试对象: {testObjectCount}");
            
            GUILayout.EndVertical(); // 结束垂直布局
            GUILayout.EndArea();   // 结束IMGUI区域
        }
    }
}