// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：GameSystemExample.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件包含一个名为 GameSystemExample 的 MonoBehaviour 类，
//     用于演示如何在游戏中使用各种核心游戏系统，如资源系统、建筑系统、
//     科技系统、制造系统和配置系统。它提供了通过 Inspector 触发或自动运行的
//     示例方法，展示了如何获取系统实例、调用其API以及发送命令等。
// ==============================================================================

using System.Collections.Generic;
using UnityEngine;
using QFramework;           // QFramework 核心命名空间
using SurvivalGame.Model;   // 游戏数据模型，如 ResourceType, BuildingCategory 等
using SurvivalGame.GameSystem; // 游戏核心系统接口和实现，如 ResourceSystem, ConfigSystem
using SurvivalGame.Command; // 游戏指令，如 AddBuildingCommand

namespace SurvivalGame.Example
{
    /// <summary>
    /// 游戏核心系统使用示例脚本。
    /// 展示了如何在 MonoBehaviour 中与游戏的各个主要系统进行交互，
    /// 包括资源管理、建筑放置、科技研发、物品制造以及配置数据的读取。
    /// </summary>
    public class GameSystemExample : MonoBehaviour, IController // 实现QFramework的IController接口以便获取系统和发送命令
    {
        [Header("测试配置选项")] // Inspector中分组显示
        [SerializeField] private bool _runExampleOnStart = false; // 是否在游戏开始时自动运行所有示例方法
        [SerializeField] private Vector3 _testBuildingPosition = Vector3.zero; // 测试建筑放置的默认位置

        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 如果 _runExampleOnStart 为true，则延迟1秒后执行 RunExample 方法。
        /// </summary>
        private void Start()
        {
            if (_runExampleOnStart)
            {
                // 使用Invoke延迟执行，是为了确保所有游戏系统都已在QFramework架构中完成初始化和注册。
                Invoke(nameof(RunExample), 1f); // 1秒延迟
            }
        }

        /// <summary>
        /// 运行所有游戏系统示例方法的入口。
        /// 此方法可以通过Unity编辑器的上下文菜单（右键点击此脚本组件）手动触发。
        /// </summary>
        [ContextMenu("运行所有系统示例 (Run All Examples)")] // 使此方法在Inspector中可被右键调用
        public void RunExample()
        {
            Debug.Log("=== [游戏系统示例] 开始执行所有示例方法 ===");

            // 依次调用各个系统的示例方法
            ResourceSystemExample();    // 1. 资源系统示例
            BuildingSystemExample();    // 2. 建筑系统示例
            TechSystemExample();        // 3. 科技系统示例
            CraftingSystemExample();    // 4. 制造系统示例
            ConfigSystemExample();      // 5. 配置系统示例
            TestAddNewConfigs();        // 6. 测试动态添加配置示例

            Debug.Log("=== [游戏系统示例] 所有示例方法执行完毕 ===");
        }

        /// <summary>
        /// 演示资源系统 (ResourceSystem) 的基本用法。
        /// 包括添加资源、获取资源数量、检查是否能支付成本以及消耗资源。
        /// </summary>
        private void ResourceSystemExample()
        {
            Debug.Log("--- [资源系统示例] ---");

            // 通过QFramework的this.GetSystem扩展方法获取ResourceSystem的实例
            var resourceSystem = this.GetSystem<ResourceSystem>();
            if (resourceSystem == null) { Debug.LogError("[资源系统示例] 获取ResourceSystem失败！"); return; }

            // 添加初始资源作为示例
            resourceSystem.AddResource(ResourceType.Materials, 1000); // 添加1000单位材料
            resourceSystem.AddResource(ResourceType.Food, 500);     // 添加500单位食物
            resourceSystem.AddResource(ResourceType.Water, 300);    // 添加300单位水

            // 打印当前资源数量
            Debug.Log($"  当前材料数量: {resourceSystem.GetResourceAmount(ResourceType.Materials)}");
            Debug.Log($"  当前食物数量: {resourceSystem.GetResourceAmount(ResourceType.Food)}");
            Debug.Log($"  当前水数量: {resourceSystem.GetResourceAmount(ResourceType.Water)}");

            // 定义一个资源成本列表，用于测试消耗
            var costs = new List<ResourceCost>
            {
                new ResourceCost { Type = ResourceType.Materials, Amount = 50 }, // 需要50材料
                new ResourceCost { Type = ResourceType.Water, Amount = 10 }     // 需要10水
            };

            // 检查是否拥有足够的资源来支付此成本
            if (resourceSystem.CanAfford(costs))
            {
                resourceSystem.ConsumeResources(costs); // 如果足够，则消耗这些资源
                Debug.Log("  成功消耗指定资源 (50材料, 10水)。");
                Debug.Log($"  消耗后材料: {resourceSystem.GetResourceAmount(ResourceType.Materials)}, 水: {resourceSystem.GetResourceAmount(ResourceType.Water)}");
            }
            else
            {
                Debug.LogWarning("  未能消耗指定资源：资源不足。");
            }
            Debug.Log("--- [资源系统示例] 结束 ---");
        }

        /// <summary>
        /// 演示建筑系统 (BuildingSystem) 的基本用法。
        /// 包括通过发送命令来建造建筑，以及获取建筑信息。
        /// </summary>
        private void BuildingSystemExample()
        {
            Debug.Log("--- [建筑系统示例] ---");

            // 使用命令 (AddBuildingCommand) 来请求建造建筑
            // 假设 "farm_1" 和 "workshop_1" 是在ConfigSystem中定义的建筑类型ID
            this.SendCommand(new AddBuildingCommand("farm_1", _testBuildingPosition)); // 在测试位置建造农场
            this.SendCommand(new AddBuildingCommand("workshop_1", _testBuildingPosition + Vector3.right * 2)); // 在农场右侧2个单位处建工坊

            // 获取建筑系统实例以查询建筑信息
            var buildingSystem = this.GetSystem<BuildingSystem>();
            if (buildingSystem == null) { Debug.LogError("[建筑系统示例] 获取BuildingSystem失败！"); return; }
            var configSystem = this.GetSystem<ConfigSystem>(); // 同时需要配置系统来获取建筑名称等信息
            if (configSystem == null) { Debug.LogError("[建筑系统示例] 获取ConfigSystem失败！"); return; }

            var allBuildings = buildingSystem.GetAllBuildings(); // 获取当前所有已建成或建造中的建筑

            Debug.Log($"  当前场景中建筑总数: {allBuildings.Count}");

            // 遍历并打印每个建筑的详细信息
            foreach (var building in allBuildings.Values) // GetAllBuildings返回的是字典
            {
                var config = configSystem.GetBuildingConfig(building.ConfigId); // 从配置系统获取建筑的静态配置数据
                Debug.Log($"  建筑: {config?.Name ?? building.ConfigId} (ID: {building.Id}), " +
                          $"状态: {building.State}, 建造进度: {building.BuildProgress:P0}"); // P0表示百分比格式，0位小数
            }
            Debug.Log("--- [建筑系统示例] 结束 ---");
        }

        /// <summary>
        /// 演示科技系统 (TechSystem) 的基本用法。
        /// 包括添加研究点数、获取可研究科技、以及尝试开始研究。
        /// </summary>
        private void TechSystemExample()
        {
            Debug.Log("--- [科技系统示例] ---");

            var techSystem = this.GetSystem<TechSystem>();
            if (techSystem == null) { Debug.LogError("[科技系统示例] 获取TechSystem失败！"); return; }
            var configSystem = this.GetSystem<ConfigSystem>();
            if (configSystem == null) { Debug.LogError("[科技系统示例] 获取ConfigSystem失败！"); return; }
            var buildingSystem = this.GetSystem<BuildingSystem>(); // 需要建筑系统来查找研究建筑
            if (buildingSystem == null) { Debug.LogError("[科技系统示例] 获取BuildingSystem失败！"); return; }

            // 添加200点研究点数作为示例
            techSystem.AddResearchPoints(200);
            Debug.Log($"  当前可用研究点数: {techSystem.GetAvailableResearchPoints()}");

            // 获取所有已定义的科技配置
            var allTechConfigs = configSystem.GetAllTechConfigs();
            Debug.Log($"  已定义的科技总数: {allTechConfigs.Count}");

            // 遍历所有科技，检查其状态，并尝试研究第一个可用的科技
            foreach (var techConfig in allTechConfigs.Values)
            {
                var state = techSystem.GetTechState(techConfig.Id); // 获取该科技的当前状态 (Available, Researching, Researched, Locked)
                Debug.Log($"  科技: {techConfig.Name} (ID: {techConfig.Id}), 当前状态: {state}");

                // 如果找到一个可研究的科技 (Available)
                if (state == TechState.Available)
                {
                    // 尝试开始研究此科技。通常研究需要特定的研究建筑。
                    // 此处简单查找第一个功能型建筑作为研究地点。
                    var functionalBuildings = buildingSystem.GetBuildingsByCategory(BuildingCategory.Functional);

                    if (functionalBuildings.Count > 0)
                    {
                        string researchBuildingId = functionalBuildings[0].Id; // 使用第一个找到的功能建筑ID
                        Debug.Log($"    尝试在建筑 '{researchBuildingId}' 开始研究科技 '{techConfig.Name}'...");
                        // 发送StartResearchCommand命令来请求开始研究
                        // 注意：StartResearchCommand可能需要更多参数，如研究员ID列表等，此处为简化版
                        this.SendCommand(new StartResearchCommand(techConfig.Id, researchBuildingId, null));
                        break; // 示例中只尝试研究第一个可用的，然后跳出循环
                    }
                    else
                    {
                        Debug.LogWarning($"    未能开始研究科技 '{techConfig.Name}'，因为找不到可用的研究建筑 (功能型建筑)。");
                    }
                }
            }
            Debug.Log("--- [科技系统示例] 结束 ---");
        }

        /// <summary>
        /// 演示制造系统 (CraftingSystem) 的基本用法。
        /// 包括查找可用的制造建筑、获取其可用配方以及尝试开始制造。
        /// </summary>
        private void CraftingSystemExample()
        {
            Debug.Log("--- [制造系统示例] ---");

            var craftingSystem = this.GetSystem<CraftingSystem>();
            if (craftingSystem == null) { Debug.LogError("[制造系统示例] 获取CraftingSystem失败！"); return; }
            var buildingSystem = this.GetSystem<BuildingSystem>();
            if (buildingSystem == null) { Debug.LogError("[制造系统示例] 获取BuildingSystem失败！"); return; }

            // 假设我们需要在“工坊”(Workshop)类型的建筑中进行制造
            // 首先找到一个已建成且可运作的工坊
            var workshops = buildingSystem.GetBuildingsByCategory(BuildingCategory.Production) // 假设工坊属于生产类
                                       .Where(b => b.ConfigId.Contains("workshop") && b.State == BuildingState.Operational) // 筛选ID包含"workshop"且状态为可运作
                                       .ToList();

            if (workshops.Any()) // 如果找到了工坊
            {
                var workshop = workshops.First(); // 取第一个找到的工坊
                Debug.Log($"  在工坊 (ID: {workshop.Id}, 类型: {workshop.ConfigId}) 中查找可用配方...");

                // 获取该工坊当前可用的所有制造配方
                var availableRecipes = craftingSystem.GetAvailableRecipes(workshop.Id);
                Debug.Log($"  工坊 '{workshop.ConfigId}' (ID: {workshop.Id}) 当前可用配方数量: {availableRecipes.Count}");

                // 如果有可用配方，尝试制造列表中的第一个配方，数量为3
                if (availableRecipes.Count > 0)
                {
                    var recipeToCraft = availableRecipes[0];
                    int quantityToCraft = 3;
                    Debug.Log($"    尝试在工坊 '{workshop.Id}' 开始制造物品: '{recipeToCraft.Name}' x{quantityToCraft}...");
                    // 发送StartCraftingCommand命令请求开始制造
                    this.SendCommand(new StartCraftingCommand(recipeToCraft.Id, workshop.Id, quantityToCraft, null)); // 最后一个参数是可选的制造者ID列表
                }
                else
                {
                    Debug.LogWarning($"    工坊 '{workshop.Id}' 当前没有可用的制造配方。");
                }
            }
            else
            {
                Debug.LogWarning("  未能找到可运作的工坊来进行制造系统示例。请先建造一个工坊。");
            }

            // 显示当前所有活跃的制造任务
            var activeTasks = craftingSystem.GetActiveCraftingTasks();
            Debug.Log($"  当前活跃的制造任务总数: {activeTasks.Count}");
            foreach(var task in activeTasks)
            {
                Debug.Log($"    任务: 物品ID '{task.RecipeId}', 数量 {task.Quantity}, 进度 {task.Progress:P0}, 剩余时间 {task.RemainingTime:F1}s");
            }
            Debug.Log("--- [制造系统示例] 结束 ---");
        }

        /// <summary>
        /// 演示配置系统 (ConfigSystem) 的用法。
        /// 主要展示如何从配置系统中获取各种类型的配置数据。
        /// </summary>
        [ContextMenu("运行配置系统示例 (Config System Example)")]
        public void ConfigSystemExample()
        {
            Debug.Log("--- [配置系统示例] ---");

            var configSystem = this.GetSystem<ConfigSystem>();
            if (configSystem == null) { Debug.LogError("[配置系统示例] 获取ConfigSystem失败！"); return; }

            // 1. 获取所有建筑配置
            var buildingConfigs = configSystem.GetAllBuildingConfigs();
            Debug.Log($"  已加载的建筑配置总数: {buildingConfigs.Count}");
            // 按建筑类别显示部分建筑信息
            foreach (BuildingCategory category in System.Enum.GetValues(typeof(BuildingCategory)))
            {
                var buildingsInCategory = configSystem.GetBuildingsByCategory(category);
                Debug.Log($"    {GetBuildingCategoryDisplayName(category)} 类建筑 ({buildingsInCategory.Count} 个):");
                foreach (var buildingConf in buildingsInCategory.Take(2)) // 只显示前2个作为示例
                {
                    Debug.Log($"      - {buildingConf.Name} (ID: {buildingConf.ConfigId}, 等级: {buildingConf.Level})");
                }
            }

            // 2. 获取所有科技配置
            var techConfigs = configSystem.GetAllTechConfigs();
            Debug.Log($"  已加载的科技配置总数: {techConfigs.Count}");
            // 按科技树分支显示部分科技信息
            foreach (TechTree tree in System.Enum.GetValues(typeof(TechTree)))
            {
                var techsInTree = configSystem.GetTechsByTree(tree);
                Debug.Log($"    {GetTechTreeDisplayName(tree)} 科技树 ({techsInTree.Count} 个科技):");
                foreach (var techConf in techsInTree.Take(2)) // 只显示前2个作为示例
                {
                    Debug.Log($"      - {techConf.Name} (ID: {techConf.Id}, 层级: {techConf.Tier})");
                }
            }

            // 3. 获取所有资源配置
            var resourceConfigs = configSystem.GetAllResourceConfigs();
            Debug.Log($"  已加载的资源配置总数: {resourceConfigs.Count}");
            foreach (var resourceConf in resourceConfigs.Values.Take(3)) // 显示前3个资源作为示例
            {
                Debug.Log($"    资源: {resourceConf.Name} (类型: {resourceConf.Type}) - 描述: {resourceConf.Description}");
            }

            // 4. 获取所有制造配方
            var recipes = configSystem.GetAllCraftingRecipes();
            Debug.Log($"  已加载的制造配方总数: {recipes.Count}");
            foreach (var recipeConf in recipes.Values.Take(3)) // 显示前3个配方作为示例
            {
                Debug.Log($"    配方: {recipeConf.Name} (ID: {recipeConf.Id}) -> 产出: {recipeConf.OutputType} x{recipeConf.OutputAmount}");
            }
            Debug.Log("--- [配置系统示例] 结束 ---");
        }

        /// <summary>
        /// 测试动态添加新配置到ConfigSystem的功能。
        /// （注意：这部分依赖于ConfigSystem是否支持运行时的动态配置修改）
        /// </summary>
        [ContextMenu("测试动态添加新配置 (Test Add New Configs)")]
        public void TestAddNewConfigs()
        {
            Debug.Log("--- [测试动态添加新配置] ---");

            // 此示例主要用于演示概念，实际的ConfigSystem可能设计为在初始化时加载所有配置且运行时只读。
            // 如果需要在运行时动态添加或修改配置，ConfigSystem需要提供相应的公共API方法。

            var configSystem = this.GetSystem<ConfigSystem>();
            if (configSystem == null) { Debug.LogError("[测试动态添加新配置] 获取ConfigSystem失败！"); return; }

            // 提示：当前的ConfigSystem设计（根据其他文件推断）可能是只读的。
            // 如果需要动态添加，ConfigSystem内部需要有类似 RegisterBuildingConfig, RegisterTechConfig 等方法。
            Debug.Log("  当前ConfigSystem主要用于加载和读取配置。");
            Debug.Log("  如果需要在运行时动态添加配置，ConfigSystem需要扩展以下类型的方法:");
            Debug.Log("    - public void RegisterBuildingConfig(BuildingConfig config)");
            Debug.Log("    - public void RegisterTechConfig(TechConfig config)");
            Debug.Log("    - public void RegisterCraftingRecipe(CraftingRecipe recipe)");
            Debug.Log("    - public void RegisterResourceConfig(ResourceConfig config)");
            Debug.Log("    - public bool RemoveConfig(ConfigType type, string id) 等");
            Debug.Log("  （此示例不执行实际的动态添加操作，仅作说明）");
            Debug.Log("--- [测试动态添加新配置] 结束 ---");
        }

        /// <summary>
        /// 实现IController接口，返回QFramework的全局架构实例。
        /// </summary>
        public IArchitecture GetArchitecture()
        {
            // SurvivalGameApp.Interface 是QFramework中定义的获取应用架构单例的推荐方式
            return SurvivalGameApp.Interface;
        }

        // 以下为辅助方法，用于获取枚举的中文显示名，避免在日志中直接打印英文枚举名
        private string GetBuildingCategoryDisplayName(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Production: return "生产";
                case BuildingCategory.Defense:    return "防御";
                case BuildingCategory.Habitat:    return "居住";
                case BuildingCategory.Storage:    return "仓储";
                case BuildingCategory.Functional: return "功能";
                default: return category.ToString();
            }
        }

        private string GetTechTreeDisplayName(TechTree tree)
        {
             switch (tree)
            {
                case TechTree.Survival:    return "生存";
                case TechTree.Defense:     return "防御";
                case TechTree.Habitat:     return "居住";
                case TechTree.Engineering: return "工程";
                default: return tree.ToString();
            }
        }
    }
}