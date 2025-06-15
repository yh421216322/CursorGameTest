using UnityEngine;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 场景清理工具
    /// 用于清理所有测试对象，确保只有游戏系统的僵尸存在
    /// </summary>
    public class SceneCleanup : MonoBehaviour
    {
        [Header("清理设置")]
        public KeyCode cleanupKey = KeyCode.Delete;
        public bool autoCleanOnStart = true;
        
        void Start()
        {
            if (autoCleanOnStart)
            {
                UnityEngine.Debug.Log("=== 自动清理场景开始 ===");
                CleanupScene();
            }
        }
        
        void Update()
        {
            if (Input.GetKeyDown(cleanupKey))
            {
                CleanupScene();
            }
        }
        
        [ContextMenu("清理场景")]
        public void CleanupScene()
        {
            UnityEngine.Debug.Log("开始清理场景中的测试对象...");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int clearedCount = 0;
            
            foreach (var obj in allObjects)
            {
                bool shouldDelete = false;
                string reason = "";
                
                // 检查各种测试对象
                if (obj.name.Contains("TestSprite"))
                {
                    shouldDelete = true;
                    reason = "静态测试图片";
                }
                else if (obj.name.Contains("TestSquare"))
                {
                    shouldDelete = true;
                    reason = "测试方块";
                }
                else if (obj.name.Contains("TempZombie"))
                {
                    shouldDelete = true;
                    reason = "临时僵尸对象";
                }
                // 检查是否是没有SpriteRenderer的僵尸对象（可能是损坏的）
                else if (obj.name.ToLower().Contains("zombie"))
                {
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    if (renderer == null)
                    {
                        shouldDelete = true;
                        reason = "损坏的僵尸对象（无渲染器）";
                    }
                    else if (renderer.sprite == null)
                    {
                        shouldDelete = true;
                        reason = "损坏的僵尸对象（无精灵）";
                    }
                    else if (renderer.sprite.name.Contains("ZombieSprite_") && 
                             renderer.color == Color.black)
                    {
                        shouldDelete = true;
                        reason = "黑色默认僵尸对象";
                    }
                }
                
                if (shouldDelete)
                {
                    UnityEngine.Debug.Log($"清理对象: {obj.name} - 原因: {reason}");
                    DestroyImmediate(obj);
                    clearedCount++;
                }
            }
            
            UnityEngine.Debug.Log($"场景清理完成，共清理 {clearedCount} 个对象");
            
            // 清理完成后，显示剩余的有效僵尸
            ShowRemainingZombies();
        }
        
        void ShowRemainingZombies()
        {
            UnityEngine.Debug.Log("--- 剩余有效僵尸对象 ---");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int validZombieCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie"))
                {
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    if (renderer != null && renderer.sprite != null)
                    {
                        validZombieCount++;
                        UnityEngine.Debug.Log($"✅ 有效僵尸: {obj.name}, 精灵: {renderer.sprite.name}, " +
                                              $"位置: {obj.transform.position}, 颜色: {renderer.color}");
                    }
                }
            }
            
            if (validZombieCount == 0)
            {
                UnityEngine.Debug.Log("场景中没有有效的僵尸对象");
            }
            else
            {
                UnityEngine.Debug.Log($"场景中共有 {validZombieCount} 个有效僵尸对象");
            }
        }
        
        [ContextMenu("强制清理所有僵尸")]
        public void ForceCleanAllZombies()
        {
            UnityEngine.Debug.Log("强制清理所有僵尸对象...");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int clearedCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie") || 
                    obj.name.Contains("TestSprite") || 
                    obj.name.Contains("TestSquare"))
                {
                    UnityEngine.Debug.Log($"强制清理: {obj.name}");
                    DestroyImmediate(obj);
                    clearedCount++;
                }
            }
            
            UnityEngine.Debug.Log($"强制清理完成，共清理 {clearedCount} 个对象");
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, Screen.height - 150, 190, 140));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("场景清理工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            if (GUILayout.Button($"清理测试对象 ({cleanupKey})"))
            {
                CleanupScene();
            }
            
            if (GUILayout.Button("强制清理所有僵尸"))
            {
                ForceCleanAllZombies();
            }
            
            if (GUILayout.Button("显示剩余僵尸"))
            {
                ShowRemainingZombies();
            }
            
            // 统计当前对象
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int zombieCount = 0;
            int testCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie"))
                    zombieCount++;
                else if (obj.name.Contains("Test"))
                    testCount++;
            }
            
            GUILayout.Label($"僵尸对象: {zombieCount}");
            GUILayout.Label($"测试对象: {testCount}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
} 