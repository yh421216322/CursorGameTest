using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Model;
using MyGameNamespace;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸碰撞检测测试工具
    /// 测试僵尸避障、碰撞检测功能
    /// </summary>
    public class ZombieCollisionTest : MonoBehaviour, IController
    {
        [Header("测试设置")]
        public KeyCode testCollisionKey = KeyCode.F11;
        public KeyCode spawnTestZombiesKey = KeyCode.F12;
        public KeyCode clearTestKey = KeyCode.Delete;
        
        private IZombieSystem zombieSystem;
        private IEnhancedBuildingSystem buildingSystem;
        
        void Start()
        {
            UnityEngine.Debug.Log("=== 僵尸碰撞检测测试工具启动 ===");
            InitializeSystems();
        }
        
        void InitializeSystems()
        {
            try
            {
                zombieSystem = this.GetSystem<IZombieSystem>();
                UnityEngine.Debug.Log("✅ 僵尸系统获取成功");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ 僵尸系统获取失败: {e.Message}");
            }
            
            try
            {
                buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
                UnityEngine.Debug.Log("✅ 建筑系统获取成功");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ 建筑系统获取失败: {e.Message}");
            }
        }
        
        void Update()
        {
            if (Input.GetKeyDown(testCollisionKey))
            {
                TestZombieCollision();
            }
            
            if (Input.GetKeyDown(spawnTestZombiesKey))
            {
                SpawnTestZombies();
            }
            
            if (Input.GetKeyDown(clearTestKey))
            {
                ClearTestObjects();
            }
        }
        
        /// <summary>
        /// 测试僵尸碰撞检测
        /// </summary>
        void TestZombieCollision()
        {
            UnityEngine.Debug.Log("=== 开始僵尸碰撞检测测试 ===");
            
            if (zombieSystem == null)
            {
                UnityEngine.Debug.LogError("❌ 僵尸系统未初始化");
                return;
            }
            
            // 创建测试建筑
            CreateTestBuildings();
            
            // 测试僵尸生成（应该避开建筑）
            UnityEngine.Debug.Log("🧪 测试1: 僵尸生成避障");
            for (int i = 0; i < 5; i++)
            {
                zombieSystem.SpawnZombie(ZombieType.Walker, Vector2.zero); // 在中心附近生成
            }
            
            // 测试僵尸聚集（检查重叠）
            UnityEngine.Debug.Log("🧪 测试2: 僵尸聚集避免重叠");
            Vector2 clusterCenter = new Vector2(5f, 5f);
            for (int i = 0; i < 8; i++)
            {
                zombieSystem.SpawnZombie(ZombieType.Runner, clusterCenter);
            }
            
            // 验证僵尸位置
            ValidateZombiePositions();
        }
        
        /// <summary>
        /// 创建测试建筑
        /// </summary>
        void CreateTestBuildings()
        {
            UnityEngine.Debug.Log("🏗️ 创建测试建筑");
            
            if (buildingSystem == null)
            {
                UnityEngine.Debug.LogWarning("⚠️ 建筑系统未可用，跳过建筑创建");
                return;
            }
            
            try
            {
                // 在不同位置创建建筑来测试避障
                Vector3[] testPositions = {
                    new Vector3(3f, 0f, 0f),   // 右侧
                    new Vector3(-3f, 0f, 0f),  // 左侧
                    new Vector3(0f, 3f, 0f),   // 上方
                    new Vector3(0f, -3f, 0f),  // 下方
                    new Vector3(8f, 8f, 0f),   // 聚集测试区域附近
                };
                
                string[] buildingTypes = { "Wall", "Farm", "Workshop", "Shelter", "Wall" };
                
                for (int i = 0; i < testPositions.Length && i < buildingTypes.Length; i++)
                {
                    if (buildingSystem.CanBuildAt(testPositions[i], buildingTypes[i]))
                    {
                        buildingSystem.CanBuildAt(testPositions[i], buildingTypes[i]);
                        UnityEngine.Debug.Log($"✅ 创建测试建筑: {buildingTypes[i]} 在 {testPositions[i]}");
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"⚠️ 无法在 {testPositions[i]} 建造 {buildingTypes[i]}");
                    }
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ 创建测试建筑失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 生成测试僵尸
        /// </summary>
        void SpawnTestZombies()
        {
            UnityEngine.Debug.Log("🧟 生成测试僵尸群");
            
            if (zombieSystem == null)
            {
                UnityEngine.Debug.LogError("❌ 僵尸系统未初始化");
                return;
            }
            
            // 测试僵尸群生成
            zombieSystem.SpawnZombieHorde(ZombieThreatLevel.Medium, new Vector2(10f, 0f));
            zombieSystem.SpawnZombieHorde(ZombieThreatLevel.Low, new Vector2(-10f, 0f));
            
            UnityEngine.Debug.Log("✅ 测试僵尸群生成完成");
        }
        
        /// <summary>
        /// 验证僵尸位置（检查碰撞）
        /// </summary>
        void ValidateZombiePositions()
        {
            UnityEngine.Debug.Log("🔍 验证僵尸位置...");
            
            if (zombieSystem == null) return;
            
            var zombieData = zombieSystem.GetZombieData();
            var zombies = zombieData.zombies.Values;
            
            int collisionCount = 0;
            int totalZombies = 0;
            
            foreach (var zombie1 in zombies)
            {
                if (!zombie1.IsAlive) continue;
                totalZombies++;
                
                // 检查与其他僵尸的碰撞
                foreach (var zombie2 in zombies)
                {
                    if (zombie1.id == zombie2.id || !zombie2.IsAlive) continue;
                    
                    float distance = Vector2.Distance(zombie1.position, zombie2.position);
                    if (distance < 0.8f) // 僵尸半径 * 2
                    {
                        collisionCount++;
                        UnityEngine.Debug.LogWarning($"⚠️ 发现僵尸碰撞: {zombie1.id} 与 {zombie2.id} 距离: {distance:F2}");
                    }
                }
                
                // 检查与建筑的碰撞
                if (buildingSystem != null)
                {
                    var buildings = buildingSystem.GetAllBuildings();
                    foreach (var building in buildings)
                    {
                        if (building.Health <= 0) continue;
                        
                        float distance = Vector2.Distance(zombie1.position, building.Position);
                        if (distance < 1.5f) // 僵尸半径 + 建筑半径 + 缓冲区
                        {
                            UnityEngine.Debug.LogWarning($"⚠️ 僵尸与建筑过近: {zombie1.id} 距离建筑 {building.ConfigId} 仅 {distance:F2}");
                        }
                    }
                }
            }
            
            UnityEngine.Debug.Log($"📊 碰撞检测结果: 总僵尸数 {totalZombies}, 发现碰撞 {collisionCount} 次");
            
            if (collisionCount == 0)
            {
                UnityEngine.Debug.Log("✅ 碰撞检测测试通过！");
            }
            else
            {
                UnityEngine.Debug.LogError("❌ 碰撞检测测试失败，存在重叠问题");
            }
        }
        
        /// <summary>
        /// 清理测试对象
        /// </summary>
        void ClearTestObjects()
        {
            UnityEngine.Debug.Log("🧹 清理测试对象");
            
            // 清理所有僵尸可视化对象
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int cleanedCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("zombie") || obj.name.ToLower().Contains("test"))
                {
                    Destroy(obj);
                    cleanedCount++;
                }
            }
            
            UnityEngine.Debug.Log($"✅ 清理完成，删除了 {cleanedCount} 个测试对象");
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 340, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("僵尸碰撞检测测试", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            if (GUILayout.Button($"运行碰撞测试 ({testCollisionKey})"))
            {
                TestZombieCollision();
            }
            
            if (GUILayout.Button($"生成测试僵尸 ({spawnTestZombiesKey})"))
            {
                SpawnTestZombies();
            }
            
            if (GUILayout.Button($"验证位置"))
            {
                ValidateZombiePositions();
            }
            
            if (GUILayout.Button($"清理测试对象 ({clearTestKey})"))
            {
                ClearTestObjects();
            }
            
            GUILayout.Space(10);
            
            // 显示测试状态
            if (zombieSystem != null)
            {
                var data = zombieSystem.GetZombieData();
                GUILayout.Label($"活跃僵尸: {data.GetAliveZombieCount()}");
                
                if (buildingSystem != null)
                {
                    var buildings = buildingSystem.GetAllBuildings();
                    GUILayout.Label($"建筑数量: {buildings.Count}");
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
    }
} 