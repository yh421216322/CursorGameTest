using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Visualization;
using SurvivalGame.Model;
using MyGameNamespace;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸图片应用测试脚本
    /// 专门测试僵尸系统是否正确使用图片资源
    /// </summary>
    public class ZombieImageTest : MonoBehaviour
    {
        [Header("测试设置")]
        public KeyCode testKey = KeyCode.I; // 使用I键避免冲突
        
        private IZombieSystem zombieSystem;
        private IZombieVisualizationSystem visualSystem;
        
        void Start()
        {
            UnityEngine.Debug.Log("=== 僵尸图片测试初始化 ===");
            InitializeSystems();
        }
        
        void InitializeSystems()
        {
            try
            {
                zombieSystem = RegisterManager.Interface.GetSystem<IZombieSystem>();
                visualSystem = RegisterManager.Interface.GetSystem<IZombieVisualizationSystem>();
                UnityEngine.Debug.Log("✅ 僵尸图片测试：系统获取成功");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ 僵尸图片测试：系统获取失败 - {e.Message}");
            }
        }
        
        void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                TestZombieImageApplication();
            }
        }
        
        void TestZombieImageApplication()
        {
            UnityEngine.Debug.Log("=== 开始僵尸图片应用测试 ===");
            
            if (zombieSystem == null || visualSystem == null)
            {
                UnityEngine.Debug.LogError("系统未初始化");
                return;
            }
            
            // 1. 首先测试图片资源本身
            TestImageResources();
            
            // 2. 生成僵尸并监控创建过程
            TestZombieCreation();
            
            // 3. 检查现有僵尸对象
            CheckExistingZombies();
        }
        
        void TestImageResources()
        {
            UnityEngine.Debug.Log("--- 测试图片资源加载 ---");
            
            string[] spriteNames = { "zb1", "zb2", "zb3", "zb4" };
            foreach (string spriteName in spriteNames)
            {
                Sprite sprite = Resources.Load<Sprite>($"Image/{spriteName}");
                if (sprite != null)
                {
                    UnityEngine.Debug.Log($"✅ 图片 {spriteName} 加载成功: {sprite.name}, 大小: {sprite.bounds.size}");
                }
                else
                {
                    UnityEngine.Debug.LogError($"❌ 图片 {spriteName} 加载失败");
                }
            }
        }
        
        void TestZombieCreation()
        {
            UnityEngine.Debug.Log("--- 测试僵尸创建过程 ---");
            
            // 清理现有的测试僵尸
            ClearTestZombies();
            
            // 在摄像机附近的固定位置生成测试僵尸
            Vector2 testPosition = new Vector2(0, 0);
            
            UnityEngine.Debug.Log($"🎯 在位置 {testPosition} 创建测试僵尸");
            
            try
            {
                // 生成Walker类型僵尸
                zombieSystem.SpawnZombie(ZombieType.Walker, testPosition);
                UnityEngine.Debug.Log("✅ 僵尸生成命令发送成功，请观察Console中的创建日志");
                
                // 延迟检查结果
                Invoke("CheckCreationResult", 1f);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ 僵尸生成失败: {e.Message}");
            }
        }
        
        void CheckCreationResult()
        {
            UnityEngine.Debug.Log("--- 检查僵尸创建结果 ---");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie"))
                {
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        string spriteName = renderer.sprite?.name ?? "null";
                        UnityEngine.Debug.Log($"🧟 找到僵尸对象: {obj.name}");
                        UnityEngine.Debug.Log($"   位置: {obj.transform.position}");
                        UnityEngine.Debug.Log($"   精灵: {spriteName}");
                        UnityEngine.Debug.Log($"   颜色: {renderer.color}");
                        UnityEngine.Debug.Log($"   排序: {renderer.sortingOrder}");
                        UnityEngine.Debug.Log($"   可见: {renderer.enabled}");
                        
                        // 如果精灵是默认生成的，尝试手动设置真实图片
                        if (spriteName.Contains("ZombieSprite_"))
                        {
                            UnityEngine.Debug.LogWarning("⚠️ 检测到使用默认生成图片，尝试手动设置真实图片");
                            TryFixZombieSprite(obj, renderer);
                        }
                    }
                }
            }
        }
        
        void TryFixZombieSprite(GameObject zombieObj, SpriteRenderer renderer)
        {
            // 尝试手动加载并设置真实的僵尸图片
            Sprite realSprite = Resources.Load<Sprite>("Image/zb1");
            if (realSprite != null)
            {
                renderer.sprite = realSprite;
                UnityEngine.Debug.Log($"✅ 手动修复僵尸图片成功: {zombieObj.name} -> {realSprite.name}");
            }
            else
            {
                UnityEngine.Debug.LogError("❌ 无法加载真实僵尸图片进行修复");
            }
        }
        
        void CheckExistingZombies()
        {
            UnityEngine.Debug.Log("--- 检查现有僵尸状态 ---");
            
            if (zombieSystem != null)
            {
                try
                {
                    var zombieData = zombieSystem.GetZombieData();
                    UnityEngine.Debug.Log($"数据中的僵尸数量: {zombieData.zombies.Count}");
                    
                    foreach (var zombie in zombieData.zombies)
                    {
                        UnityEngine.Debug.Log($"数据僵尸: ID={zombie.Value.id}, 类型={zombie.Value.type}, 位置={zombie.Value.position}");
                        
                        // 检查对应的视觉对象
                        var visualObj = visualSystem.GetZombieVisual(zombie.Value.id);
                        if (visualObj != null)
                        {
                            var renderer = visualObj.GetComponent<SpriteRenderer>();
                            UnityEngine.Debug.Log($"   视觉对象: {visualObj.name}, 精灵: {renderer?.sprite?.name}");
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning($"   ⚠️ 僵尸 {zombie.Value.id} 缺少视觉对象");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"检查僵尸数据失败: {e.Message}");
                }
            }
        }
        
        void ClearTestZombies()
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int clearedCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie") || 
                    obj.name.Contains("TestSprite") || 
                    obj.name.Contains("TestSquare"))
                {
                    UnityEngine.Debug.Log($"清理对象: {obj.name}");
                    DestroyImmediate(obj);
                    clearedCount++;
                }
            }
            
            UnityEngine.Debug.Log($"清理完成，共清理 {clearedCount} 个对象");
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 300, 100));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("僵尸图片测试工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            if (GUILayout.Button($"测试图片应用 ({testKey})"))
            {
                TestZombieImageApplication();
            }
            
            if (GUILayout.Button("清理所有测试对象"))
            {
                ClearTestZombies();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
} 