// using System.Collections.Generic;
// using UnityEngine;
// using QFramework;
// using SurvivalGame.Model;
// using SurvivalGame.System;
// using SurvivalGame.Command;
//
// namespace SurvivalGame.Example
// {
//     /// <summary>
//     /// 游戏系统使用示例
//     /// </summary>
//     public class GameSystemExample : MonoBehaviour, IController
//     {
//         [Header("测试配置")]
//         [SerializeField] private bool _runExampleOnStart = false;
//         [SerializeField] private Vector3 _testBuildingPosition = Vector3.zero;
//         
//         private void Start()
//         {
//             if (_runExampleOnStart)
//             {
//                 // 延迟执行，确保系统初始化完成
//                 Invoke(nameof(RunExample), 1f);
//             }
//         }
//         
//         /// <summary>
//         /// 运行示例代码
//         /// </summary>
//         [ContextMenu("运行示例")]
//         public void RunExample()
//         {
//             Debug.Log("=== 游戏系统示例开始 ===");
//             
//             // 1. 资源系统示例
//             ResourceSystemExample();
//             
//             // 2. 建筑系统示例
//             BuildingSystemExample();
//             
//             // 3. 科技系统示例
//             TechSystemExample();
//             
//             // 4. 制造系统示例
//             CraftingSystemExample();
//             
//             Debug.Log("=== 游戏系统示例结束 ===");
//         }
//         
//         /// <summary>
//         /// 资源系统示例
//         /// </summary>
//         private void ResourceSystemExample()
//         {
//             Debug.Log("--- 资源系统示例 ---");
//             
//             var resourceSystem = this.GetSystem<ResourceSystem>();
//             
//             // 添加初始资源
//             resourceSystem.AddResource(ResourceType.Materials, 1000);
//             resourceSystem.AddResource(ResourceType.Food, 500);
//             resourceSystem.AddResource(ResourceType.Water, 300);
//             
//             Debug.Log($"材料: {resourceSystem.GetResourceAmount(ResourceType.Materials)}");
//             Debug.Log($"食物: {resourceSystem.GetResourceAmount(ResourceType.Food)}");
//             Debug.Log($"水: {resourceSystem.GetResourceAmount(ResourceType.Water)}");
//             
//             // 测试资源消耗
//             var costs = new List<ResourceCost>
//             {
//                 new ResourceCost { Type = ResourceType.Materials, Amount = 50 },
//                 new ResourceCost { Type = ResourceType.Water, Amount = 10 }
//             };
//             
//             if (resourceSystem.CanAfford(costs))
//             {
//                 resourceSystem.ConsumeResources(costs);
//                 Debug.Log("消耗资源成功");
//             }
//         }
//         
//         /// <summary>
//         /// 建筑系统示例
//         /// </summary>
//         private void BuildingSystemExample()
//         {
//             Debug.Log("--- 建筑系统示例 ---");
//             
//             // 使用命令建造建筑
//             this.SendCommand(new AddBuildingCommand("farm_1", _testBuildingPosition));
//             this.SendCommand(new AddBuildingCommand("workshop_1", _testBuildingPosition + Vector3.right * 2));
//             
//             // 获取建筑信息
//             var buildingSystem = this.GetSystem<BuildingSystem>();
//             var allBuildings = buildingSystem.GetAllBuildings();
//             
//             Debug.Log($"当前建筑数量: {allBuildings.Count}");
//             
//             foreach (var building in allBuildings.Values)
//             {
//                 var configSystem = this.GetSystem<ConfigSystem>();
//                 var config = configSystem.GetBuildingConfig(building.ConfigId);
//                 Debug.Log($"建筑: {config?.Name}, 状态: {building.State}, 进度: {building.BuildProgress:P}");
//             }
//         }
//         
//         /// <summary>
//         /// 科技系统示例
//         /// </summary>
//         private void TechSystemExample()
//         {
//             Debug.Log("--- 科技系统示例 ---");
//             
//             var techSystem = this.GetSystem<TechSystem>();
//             
//             // 添加科研点数
//             techSystem.AddResearchPoints(200);
//             Debug.Log($"科研点数: {techSystem.GetAvailableResearchPoints()}");
//             
//             // 获取可研究的科技
//             var configSystem = this.GetSystem<ConfigSystem>();
//             var allTechs = configSystem.GetAllTechConfigs();
//             
//             foreach (var tech in allTechs.Values)
//             {
//                 var state = techSystem.GetTechState(tech.Id);
//                 Debug.Log($"科技: {tech.Name}, 状态: {state}");
//                 
//                 // 尝试研究第一个可用科技
//                 if (state == TechState.Available)
//                 {
//                     // 需要先有研究建筑
//                     var buildingSystem = this.GetSystem<BuildingSystem>();
//                     var buildings = buildingSystem.GetBuildingsByCategory(BuildingCategory.Functional);
//                     
//                     if (buildings.Count > 0)
//                     {
//                         this.SendCommand(new StartResearchCommand(tech.Id, buildings[0].Id));
//                         break;
//                     }
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 制造系统示例
//         /// </summary>
//         private void CraftingSystemExample()
//         {
//             Debug.Log("--- 制造系统示例 ---");
//             
//             var craftingSystem = this.GetSystem<CraftingSystem>();
//             var buildingSystem = this.GetSystem<BuildingSystem>();
//             
//             // 找到工坊建筑
//             var workshops = buildingSystem.GetBuildingsByCategory(BuildingCategory.Production);
//             
//             foreach (var workshop in workshops)
//             {
//                 if (workshop.State == BuildingState.Operational)
//                 {
//                     // 获取可用配方
//                     var availableRecipes = craftingSystem.GetAvailableRecipes(workshop.Id);
//                     
//                     Debug.Log($"工坊 {workshop.Id} 可用配方数量: {availableRecipes.Count}");
//                     
//                     // 尝试制造第一个配方
//                     if (availableRecipes.Count > 0)
//                     {
//                         var recipe = availableRecipes[0];
//                         this.SendCommand(new StartCraftingCommand(recipe.Id, workshop.Id, 3));
//                         Debug.Log($"开始制造: {recipe.Name} x3");
//                     }
//                     break;
//                 }
//             }
//             
//             // 显示活跃的制造任务
//             var activeTasks = craftingSystem.GetActiveCraftingTasks();
//             Debug.Log($"活跃制造任务数量: {activeTasks.Count}");
//         }
//         
//         /// <summary>
//         /// 配置系统示例
//         /// </summary>
//         [ContextMenu("配置系统示例")]
//         public void ConfigSystemExample()
//         {
//             Debug.Log("--- 配置系统示例 ---");
//             
//             var configSystem = this.GetSystem<ConfigSystem>();
//             
//             // 获取所有建筑配置
//             var buildingConfigs = configSystem.GetAllBuildingConfigs();
//             Debug.Log($"建筑配置数量: {buildingConfigs.Count}");
//             
//             // 按类别显示建筑
//             foreach (BuildingCategory category in System.Enum.GetValues(typeof(BuildingCategory)))
//             {
//                 var buildings = configSystem.GetBuildingsByCategory(category);
//                 Debug.Log($"{category} 类建筑: {buildings.Count} 个");
//                 
//                 foreach (var building in buildings)
//                 {
//                     Debug.Log($"  - {building.Name} (等级{(int)building.Level})");
//                 }
//             }
//             
//             // 获取所有科技配置
//             var techConfigs = configSystem.GetAllTechConfigs();
//             Debug.Log($"科技配置数量: {techConfigs.Count}");
//             
//             // 按科技树显示科技
//             foreach (TechTree tree in System.Enum.GetValues(typeof(TechTree)))
//             {
//                 var techs = configSystem.GetTechsByTree(tree);
//                 Debug.Log($"{tree} 科技树: {techs.Count} 个科技");
//                 
//                 foreach (var tech in techs)
//                 {
//                     Debug.Log($"  - {tech.Name} (层级{(int)tech.Tier})");
//                 }
//             }
//             
//             // 获取所有资源配置
//             var resourceConfigs = configSystem.GetAllResourceConfigs();
//             Debug.Log($"资源配置数量: {resourceConfigs.Count}");
//             
//             foreach (var resource in resourceConfigs.Values)
//             {
//                 Debug.Log($"资源: {resource.Name} - {resource.Description}");
//             }
//             
//             // 获取所有制造配方
//             var recipes = configSystem.GetAllCraftingRecipes();
//             Debug.Log($"制造配方数量: {recipes.Count}");
//             
//             foreach (var recipe in recipes.Values)
//             {
//                 Debug.Log($"配方: {recipe.Name} -> {recipe.OutputType} x{recipe.OutputAmount}");
//             }
//         }
//         
//         /// <summary>
//         /// 测试新增配置
//         /// </summary>
//         [ContextMenu("测试新增配置")]
//         public void TestAddNewConfigs()
//         {
//             Debug.Log("--- 测试新增配置 ---");
//             
//             // 这里演示如何在运行时动态添加配置
//             // 实际项目中，配置通常在初始化时加载
//             
//             var configSystem = this.GetSystem<ConfigSystem>();
//             
//             // 注意：当前的ConfigSystem是只读的，如果需要动态添加配置
//             // 需要扩展ConfigSystem添加相应的方法
//             
//             Debug.Log("当前系统为只读配置，如需动态添加请扩展ConfigSystem");
//             Debug.Log("建议的扩展方法:");
//             Debug.Log("- AddBuildingConfig(BuildingConfig config)");
//             Debug.Log("- AddTechConfig(TechConfig config)");
//             Debug.Log("- AddCraftingRecipe(CraftingRecipe recipe)");
//             Debug.Log("- RemoveConfig(string id)");
//         }
//         
//         public IArchitecture GetArchitecture()
//         {
//             return SurvivalGameApp.Interface;
//         }
//     }
// } 