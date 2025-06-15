// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieCollisionTest.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 ZombieCollisionTest 的 MonoBehaviour 类。
//     该脚本是一个调试工具，专门用于测试游戏中僵尸的碰撞检测和避障行为。
//     它提供了通过快捷键或IMGUI按钮触发的功能，如创建测试建筑、生成测试僵尸、
//     验证僵尸位置（检查碰撞/重叠）以及清理场景中的测试对象。
//     目的是辅助开发者快速验证和调试僵尸与环境及其他僵尸的交互逻辑。
// ==============================================================================

using UnityEngine;
using QFramework; // QFramework框架，用于获取系统实例
using SurvivalGame.GameSystem; // 包含IZombieSystem, IEnhancedBuildingSystem等游戏系统接口
using SurvivalGame.Model;     // 包含ZombieType, ZombieThreatLevel等模型定义
using MyGameNamespace;    // 自定义命名空间，可能包含事件或其他定义 (当前在此文件中未使用)

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 僵尸碰撞检测与避障功能测试工具。
    /// 此脚本提供了一系列方法来创建测试场景（建筑）、生成僵尸，并验证它们的碰撞行为，
    /// 例如僵尸之间是否重叠，僵尸是否能避开建筑等。
    /// 可以通过快捷键或IMGUI界面来触发这些测试功能。
    /// </summary>
    public class ZombieCollisionTest : MonoBehaviour, IController // 实现QFramework的IController接口
    {
        [Header("测试快捷键设置")] // Inspector中分组显示
        public KeyCode testCollisionKey = KeyCode.F11;     // 触发完整碰撞测试的快捷键
        public KeyCode spawnTestZombiesKey = KeyCode.F12;  // 单独生成测试僵尸的快捷键
        public KeyCode clearTestKey = KeyCode.Delete;      // 清理所有测试对象的快捷键
        
        // 系统接口引用
        private IZombieSystem zombieSystem;             // 僵尸系统实例
        private IEnhancedBuildingSystem buildingSystem; // 增强型建筑系统实例
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 初始化时，会记录日志并尝试获取所需的游戏系统实例。
        /// </summary>
        void Start()
        {
            UnityEngine.Debug.Log("=== [僵尸碰撞测试工具] 已启动 ===");
            InitializeSystems(); // 调用方法初始化系统引用
        }
        
        /// <summary>
        /// 初始化所需的系统引用（僵尸系统和建筑系统）。
        /// 获取结果会输出到控制台。
        /// </summary>
        void InitializeSystems()
        {
            try
            {
                // 通过QFramework的this.GetSystem扩展方法获取IZombieSystem的实例
                zombieSystem = this.GetSystem<IZombieSystem>();
                if (zombieSystem != null) UnityEngine.Debug.Log("  [僵尸碰撞测试工具] ✅ 僵尸系统 (IZombieSystem) 获取成功。");
                else UnityEngine.Debug.LogError("  [僵尸碰撞测试工具] ❌ 僵尸系统 (IZombieSystem) 获取失败：返回null。");
            }
            catch (System.Exception e) // 捕获获取过程中可能发生的异常
            {
                UnityEngine.Debug.LogError($"  [僵尸碰撞测试工具] ❌ 获取僵尸系统时发生异常: {e.Message}");
            }
            
            try
            {
                // 获取IEnhancedBuildingSystem的实例
                buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
                if (buildingSystem != null) UnityEngine.Debug.Log("  [僵尸碰撞测试工具] ✅ 建筑系统 (IEnhancedBuildingSystem) 获取成功。");
                else UnityEngine.Debug.LogError("  [僵尸碰撞测试工具] ❌ 建筑系统 (IEnhancedBuildingSystem) 获取失败：返回null。");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"  [僵尸碰撞测试工具] ❌ 获取建筑系统时发生异常: {e.Message}");
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 监听用户按下的快捷键，以触发相应的测试功能。
        /// </summary>
        void Update()
        {
            // 如果按下了预设的碰撞测试快捷键
            if (Input.GetKeyDown(testCollisionKey))
            {
                TestZombieCollision(); // 执行完整的僵尸碰撞测试流程
            }
            
            // 如果按下了预设的生成测试僵尸快捷键
            if (Input.GetKeyDown(spawnTestZombiesKey))
            {
                SpawnTestZombies(); // 单独执行生成测试僵尸群的逻辑
            }
            
            // 如果按下了预设的清理测试对象快捷键
            if (Input.GetKeyDown(clearTestKey))
            {
                ClearTestObjects(); // 清理场景中的所有测试相关对象
            }
        }
        
        /// <summary>
        /// 执行完整的僵尸碰撞检测测试流程。
        /// 包括创建测试建筑、生成僵尸以测试避障、生成僵尸群以测试重叠，并验证僵尸位置。
        /// </summary>
        [ContextMenu("执行僵尸碰撞测试")] // 使此方法可在Inspector中通过右键菜单调用
        void TestZombieCollision()
        {
            UnityEngine.Debug.Log("=== [僵尸碰撞测试] 开始执行 ===");
            
            if (zombieSystem == null) // 检查僵尸系统是否已成功初始化
            {
                UnityEngine.Debug.LogError("  ❌ [僵尸碰撞测试] 无法执行：僵尸系统 (zombieSystem) 未初始化或获取失败。");
                return;
            }
            
            CreateTestBuildings(); // 首先在场景中创建一些测试用的建筑作为障碍物
            
            UnityEngine.Debug.Log("  🧪 测试1: 僵尸生成时的避障行为");
            // 在场景中心附近生成5个Walker类型的僵尸，测试它们是否会避开已创建的建筑
            for (int i = 0; i < 5; i++)
            {
                zombieSystem.SpawnZombie(ZombieType.Walker, Vector2.zero);
            }
            
            UnityEngine.Debug.Log("  🧪 测试2: 僵尸聚集时的相互避免重叠行为");
            // 在一个指定点 (5,5) 附近生成8个Runner类型的僵尸，观察它们是否会重叠
            Vector2 clusterCenter = new Vector2(5f, 5f);
            for (int i = 0; i < 8; i++)
            {
                zombieSystem.SpawnZombie(ZombieType.Runner, clusterCenter);
            }
            
            ValidateZombiePositions(); // 最后，验证所有僵尸的位置，检查是否存在碰撞或不合理情况
            UnityEngine.Debug.Log("=== [僵尸碰撞测试] 执行完毕 ===");
        }
        
        /// <summary>
        /// 在场景中创建一组预定义的测试建筑，用于测试僵尸的避障功能。
        /// </summary>
        void CreateTestBuildings()
        {
            UnityEngine.Debug.Log("  🏗️ 正在创建测试用建筑...");
            
            if (buildingSystem == null) // 检查建筑系统是否可用
            {
                UnityEngine.Debug.LogWarning("  ⚠️ [创建测试建筑] 建筑系统 (buildingSystem) 未初始化，跳过建筑创建步骤。");
                return;
            }
            
            try
            {
                // 定义一组测试建筑的位置和类型
                Vector3[] testPositions = {
                    new Vector3(3f, 0f, 0f),   // 场景右侧
                    new Vector3(-3f, 0f, 0f),  // 场景左侧
                    new Vector3(0f, 3f, 0f),   // 场景上方
                    new Vector3(0f, -3f, 0f),  // 场景下方
                    new Vector3(8f, 8f, 0f),   // 用于僵尸聚集测试的区域附近
                };
                string[] buildingTypes = { "Wall", "Farm", "Workshop", "Shelter", "Wall" }; // 对应的建筑类型ID
                
                // 遍历并尝试创建这些测试建筑
                for (int i = 0; i < testPositions.Length && i < buildingTypes.Length; i++)
                {
                    // 检查指定位置是否可以建造该类型的建筑
                    if (buildingSystem.CanBuildAt(testPositions[i], buildingTypes[i]))
                    {
                        // buildingSystem.CanBuildAt(testPositions[i], buildingTypes[i]); // 原代码中此行重复，应为建造调用
                        // 假设实际建造调用类似于: buildingSystem.StartConstruction(testPositions[i], buildingTypes[i]);
                        // 由于此脚本是测试工具，可能仅依赖CanBuildAt的副作用或日志，或实际建造逻辑在别处
                        // 为确保明确，如果需要实际建造，应调用如 StartConstruction 或 PlaceBuilding 等方法
                        // 此处保持原逻辑，但添加注释提醒
                        UnityEngine.Debug.Log($"    ✅ 预定创建测试建筑: {buildingTypes[i]} 于 {testPositions[i]} (依赖CanBuildAt的检查或实际建造逻辑)");
                        // 如果需要实际放置，应是: this.SendCommand(new BuildCommand(testPositions[i], buildingTypes[i])); 或类似
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"    ⚠️ 无法在位置 {testPositions[i]} 建造类型为 {buildingTypes[i]} 的测试建筑。");
                    }
                }
            }
            catch (System.Exception e) // 捕获创建过程中可能发生的异常
            {
                UnityEngine.Debug.LogError($"  ❌ [创建测试建筑] 过程中发生错误: {e.Message}");
            }
            UnityEngine.Debug.Log("  --- 测试建筑创建尝试完毕 ---");
        }
        
        /// <summary>
        /// 生成一些测试用的僵尸群。
        /// </summary>
        [ContextMenu("生成测试僵尸群")]
        void SpawnTestZombies()
        {
            UnityEngine.Debug.Log("  🧟 正在生成测试僵尸群...");
            
            if (zombieSystem == null)
            {
                UnityEngine.Debug.LogError("  ❌ [生成测试僵尸群] 失败：僵尸系统未初始化。");
                return;
            }
            
            // 在两个不同位置分别生成中等和低威胁等级的僵尸群
            zombieSystem.SpawnZombieHorde(ZombieThreatLevel.Medium, new Vector2(10f, 0f));
            zombieSystem.SpawnZombieHorde(ZombieThreatLevel.Low, new Vector2(-10f, 0f));
            
            UnityEngine.Debug.Log("  ✅ 测试僵尸群生成命令已发送。");
            UnityEngine.Debug.Log("  --- 测试僵尸群生成完毕 ---");
        }
        
        /// <summary>
        /// 验证当前场景中所有存活僵尸的位置，检查是否存在相互碰撞或与建筑过于接近的情况。
        /// </summary>
        [ContextMenu("验证僵尸位置（检查碰撞）")]
        void ValidateZombiePositions()
        {
            UnityEngine.Debug.Log("  🔍 开始验证僵尸位置以检测碰撞...");
            
            if (zombieSystem == null) {
                UnityEngine.Debug.LogError("  ❌ [验证僵尸位置] 失败：僵尸系统未初始化。");
                return;
            }
            
            var zombieDataContainer = zombieSystem.GetZombieData();
            if (zombieDataContainer == null || zombieDataContainer.zombies == null) {
                UnityEngine.Debug.LogError("  ❌ [验证僵尸位置] 失败：无法获取僵尸数据。");
                return;
            }
            var zombies = zombieDataContainer.zombies.Values; // 获取所有僵尸的数据集合
            
            int interZombieCollisionCount = 0; // 僵尸间碰撞计数
            int zombieBuildingProximityCount = 0; // 僵尸与建筑过近计数
            int aliveZombiesCount = 0;         // 存活僵尸总数
            
            foreach (var zombie1 in zombies)
            {
                if (!zombie1.IsAlive) continue; // 只检查存活的僵尸
                aliveZombiesCount++;
                
                // 检查与其他存活僵尸的碰撞 (基于简单距离判断)
                foreach (var zombie2 in zombies)
                {
                    if (zombie1.id == zombie2.id || !zombie2.IsAlive) continue; // 跳过自身和已死亡的僵尸
                    
                    float distance = Vector2.Distance(zombie1.position, zombie2.position);
                    // 假设僵尸的碰撞半径约为0.4，两个僵尸间距小于0.8则认为可能发生碰撞
                    if (distance < 0.8f)
                    {
                        interZombieCollisionCount++;
                        UnityEngine.Debug.LogWarning($"    ⚠️ 潜在僵尸间碰撞: {zombie1.id} 与 {zombie2.id} 距离过近: {distance:F2}");
                    }
                }
                
                // 检查与建筑的碰撞或过近 (基于简单距离判断)
                if (buildingSystem != null)
                {
                    var buildings = buildingSystem.GetAllBuildings(); // 获取所有建筑数据
                    foreach (var building in buildings)
                    {
                        if (building.Health <= 0) continue; // 跳过已摧毁的建筑
                        
                        // 假设建筑中心点为Position，建筑占用半径约为0.5-1.0 (取决于建筑大小)
                        // 僵尸半径0.4 + 建筑半径(估算0.5) + 缓冲区0.6 = 1.5
                        float proximityThreshold = 1.5f;
                        float distanceToBuilding = Vector2.Distance(zombie1.position, building.Position);
                        if (distanceToBuilding < proximityThreshold)
                        {
                            zombieBuildingProximityCount++;
                            UnityEngine.Debug.LogWarning($"    ⚠️ 僵尸与建筑过近: {zombie1.id} 距离建筑 '{building.ConfigId}' (ID: {building.Id}) 仅 {distanceToBuilding:F2}");
                        }
                    }
                }
            }
            
            // 由于每个碰撞对会被记录两次（A-B和B-A），实际碰撞次数应为 interZombieCollisionCount / 2
            interZombieCollisionCount /= 2;

            UnityEngine.Debug.Log($"  📊 [碰撞检测结果] 存活僵尸数: {aliveZombiesCount}。");
            UnityEngine.Debug.Log($"    - 僵尸间潜在碰撞对数量: {interZombieCollisionCount}。");
            UnityEngine.Debug.Log($"    - 僵尸与建筑过近事件数量: {zombieBuildingProximityCount}。");
            
            if (interZombieCollisionCount == 0 && zombieBuildingProximityCount == 0 && aliveZombiesCount > 0)
            {
                UnityEngine.Debug.Log("  ✅ 碰撞检测测试通过！未发现明显的僵尸重叠或与建筑过于接近的情况。");
            }
            else if (aliveZombiesCount == 0)
            {
                 UnityEngine.Debug.LogWarning("  ⚠️ 碰撞检测：场景中没有存活的僵尸可供验证。");
            }
            else
            {
                UnityEngine.Debug.LogError("  ❌ 碰撞检测测试发现问题，存在潜在的重叠或避障问题。请检查上述警告信息。");
            }
            UnityEngine.Debug.Log("  --- 僵尸位置验证完毕 ---");
        }
        
        /// <summary>
        /// 清理场景中所有名称包含 "zombie" 或 "test" (不区分大小写) 的GameObject。
        /// </summary>
        [ContextMenu("清理所有测试对象")]
        void ClearTestObjects()
        {
            UnityEngine.Debug.Log("  🧹 开始清理场景中的所有测试相关对象...");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>(); // 获取场景中所有对象
            int cleanedCount = 0;
            
            foreach (var obj in allObjects)
            {
                string objNameLower = obj.name.ToLower(); // 转换为小写以便不区分大小写比较
                // 如果对象名称包含 "zombie" 或 "test"，则认为其为测试对象并销毁
                if (objNameLower.Contains("zombie") || objNameLower.Contains("test"))
                {
                    Destroy(obj); // 在运行时使用Destroy()，而非DestroyImmediate()
                    cleanedCount++;
                }
            }
            
            UnityEngine.Debug.Log($"  ✅ 清理完成，共移除了 {cleanedCount} 个测试相关对象。");
            UnityEngine.Debug.Log("  --- 测试对象清理完毕 ---");
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕右上角显示一个小的测试工具面板，包含操作按钮和状态信息。
        /// </summary>
        void OnGUI()
        {
            // 定义GUI区域在屏幕右上角
            GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 340, 200), "", GUI.skin.box);
            GUILayout.BeginVertical();
            
            // 面板标题
            GUILayout.Label("僵尸碰撞与避障测试工具", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            
            // 功能按钮，并显示其对应的快捷键
            if (GUILayout.Button($"运行完整碰撞测试 ({testCollisionKey})")) { TestZombieCollision(); }
            if (GUILayout.Button($"生成测试僵尸群 ({spawnTestZombiesKey})")) { SpawnTestZombies(); }
            if (GUILayout.Button("验证当前僵尸位置")) { ValidateZombiePositions(); }
            if (GUILayout.Button($"清理所有测试对象 ({clearTestKey})")) { ClearTestObjects(); }
            
            GUILayout.Space(10); // 添加一些垂直间距
            
            // 显示当前游戏状态相关的简要信息
            if (zombieSystem != null)
            {
                var data = zombieSystem.GetZombieData();
                GUILayout.Label($"系统内活跃僵尸数: {zombieSystem.GetActiveZombieCount()} / 总记录: {data?.zombies?.Count ?? 0}");
                
                if (buildingSystem != null)
                {
                    GUILayout.Label($"场景中建筑数量: {buildingSystem.GetAllBuildings()?.Count ?? 0}");
                } else { GUILayout.Label("建筑系统: 未初始化"); }
            } else { GUILayout.Label("僵尸系统: 未初始化"); }
            
            GUILayout.EndVertical(); // 结束垂直布局
            GUILayout.EndArea();   // 结束IMGUI区域
        }
        
        /// <summary>
        /// 实现IController接口，返回QFramework的全局架构实例。
        /// </summary>
        public IArchitecture GetArchitecture() => RegisterManager.Interface; // RegisterManager是QFramework中用于获取架构实例的类
    }
}