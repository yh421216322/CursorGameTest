// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ConfigSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了配置管理系统 (ConfigSystem)，该系统负责加载、存储和提供
//     游戏中所有类型的配置数据，例如建筑配置、科技配置、资源配置、
//     制造配方以及建筑升级信息。这些配置数据通常在游戏启动时加载，并在
//     游戏运行时供其他系统查询使用。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 配置管理系统 - 负责加载和管理所有游戏配置数据。
    /// 此系统作为各种配置信息的中央存储库，供游戏其他部分按需访问。
    /// </summary>
    public class ConfigSystem : AbstractSystem
    {
        #region 配置数据存储 (Configuration Data Storage)
        
        /// <summary>
        /// 存储所有建筑配置的字典，键为建筑配置ID。
        /// </summary>
        private Dictionary<string, BuildingConfig> _buildingConfigs;
        /// <summary>
        /// 存储所有科技配置的字典，键为科技配置ID。
        /// </summary>
        private Dictionary<string, TechConfig> _techConfigs;
        /// <summary>
        /// 存储所有资源配置的字典，键为资源类型枚举。
        /// </summary>
        private Dictionary<ResourceType, ResourceConfig> _resourceConfigs;
        /// <summary>
        /// 存储所有制造配方的字典，键为配方ID。
        /// </summary>
        private Dictionary<string, CraftingRecipe> _craftingRecipes;
        /// <summary>
        /// 存储所有建筑升级路径配置的字典，键通常由源建筑ID和目标建筑ID组合而成。
        /// </summary>
        private Dictionary<string, BuildingUpgrade> _buildingUpgrades;
        
        #endregion
        
        #region 系统初始化 (System Initialization)
        
        /// <summary>
        /// 系统初始化方法。
        /// 在此方法中初始化用于存储各类配置的字典，并调用加载所有配置的方法。
        /// </summary>
        protected override void OnInit()
        {
            _buildingConfigs = new Dictionary<string, BuildingConfig>();
            _techConfigs = new Dictionary<string, TechConfig>();
            _resourceConfigs = new Dictionary<ResourceType, ResourceConfig>();
            _craftingRecipes = new Dictionary<string, CraftingRecipe>();
            _buildingUpgrades = new Dictionary<string, BuildingUpgrade>();
            
            LoadAllConfigs(); // 加载所有配置数据
        }
        
        /// <summary>
        /// 加载所有类型的游戏配置数据。
        /// 此方法按顺序调用各个具体配置类型的加载方法。
        /// </summary>
        private void LoadAllConfigs()
        {
            LoadResourceConfigs();    // 加载资源配置
            LoadBuildingConfigs();    // 加载建筑配置
            LoadTechConfigs();        // 加载科技配置
            LoadCraftingRecipes();    // 加载制造配方
            LoadBuildingUpgrades();   // 加载建筑升级配置
            
            // 打印日志，显示加载的配置数量，便于调试
            Debug.Log($"[配置系统] 初始化完成 - 建筑配置: {_buildingConfigs.Count}, 科技配置: {_techConfigs.Count}, 资源配置: {_resourceConfigs.Count}, 制造配方: {_craftingRecipes.Count}, 建筑升级: {_buildingUpgrades.Count}");
        }
        
        #endregion
        
        #region 资源配置 (Resource Configurations)
        
        /// <summary>
        /// 加载所有资源相关的配置数据。
        /// 此处通过硬编码方式添加资源配置，实际项目中可能从文件（如JSON, XML, ScriptableObjects）加载。
        /// </summary>
        private void LoadResourceConfigs()
        {
            // 基础物资配置 (Basic Materials)
            AddResourceConfig(ResourceType.Food, "食物", "维持幸存者生存的基本需求，长时间不进食会导致健康下降。", ResourceCategory.BasicMaterial, true, 0.1f, 1000); // 会腐烂，腐烂速率0.1/单位时间，最大堆叠1000
            AddResourceConfig(ResourceType.Materials, "材料", "用于建造建筑和制造物品的基础材料，如木材、金属片等。", ResourceCategory.BasicMaterial, false, 0f, 2000);
            AddResourceConfig(ResourceType.Water, "水", "生存所必需的清洁饮用水，缺乏会导致脱水。", ResourceCategory.BasicMaterial, false, 0f, 500); // 假设水不会腐烂，但可能需要净化
            AddResourceConfig(ResourceType.Scrap, "废料", "从环境中搜集或拆解物品得到的废弃材料，可回收利用。", ResourceCategory.BasicMaterial, false, 0f, 5000);
            AddResourceConfig(ResourceType.OrganicMatter, "有机物", "如腐烂的食物、植物废料等，可用于堆肥或生物燃料。", ResourceCategory.BasicMaterial, true, 0.05f, 1500); // 会进一步分解
            
            // 成品物资配置 (Manufactured Goods)
            AddResourceConfig(ResourceType.Ammunition, "弹药", "用于武器的消耗品，对防御和狩猎至关重要。", ResourceCategory.Manufactured, false, 0f, 500);
            AddResourceConfig(ResourceType.MedicalSupplies, "医疗用品", "如绷带、药品等，用于治疗伤病，恢复健康。", ResourceCategory.Manufactured, true, 0.02f, 200); // 药品可能过期
            AddResourceConfig(ResourceType.Tools, "工具", "各种手持工具，能提高特定工作的效率，自身可能损耗。", ResourceCategory.Manufactured, false, 0f, 100); // 工具本身不堆叠，但此处指同类工具的库存
            AddResourceConfig(ResourceType.Fuel, "燃料", "用于驱动发电机、载具或其他耗能设备的能源物资。", ResourceCategory.Manufactured, false, 0f, 300);
            AddResourceConfig(ResourceType.Electronics, "电子元件", "制造和维修高级设备所需的精密组件。", ResourceCategory.Manufactured, false, 0f, 50);
            
            // 高级或特殊资源配置 (Advanced/Special Resources)
            AddResourceConfig(ResourceType.Energy, "能源", "通常指电力，驱动各种高科技设施运作。", ResourceCategory.Advanced, false, 0f, 10000); // 通常不直接存储，而是表示可用量或产出速率
            AddResourceConfig(ResourceType.ResearchPoints, "科研点数", "通过研究活动积累，用于解锁新的科技。", ResourceCategory.Advanced, false, 0f, 99999); // 通常无上限或极高上限
            AddResourceConfig(ResourceType.RareMetals, "稀有金属", "用于制造高级设备或特殊物品的珍贵金属材料。", ResourceCategory.Advanced, false, 0f, 20);
        }
        
        /// <summary>
        /// 向资源配置字典中添加一个新的资源配置项。
        /// </summary>
        /// <param name="type">资源类型枚举。</param>
        /// <param name="name">资源的显示名称。</param>
        /// <param name="description">资源的描述文本。</param>
        /// <param name="category">资源所属类别。</param>
        /// <param name="canDecay">资源是否会随时间腐烂或损耗。</param>
        /// <param name="decayRate">如果会腐烂，其腐烂速率（单位/时间）。</param>
        /// <param name="maxStack">单个库存堆叠的最大数量。</param>
        /// <param name="requiresSpecialStorage">是否需要特殊的存储设施。</param>
        /// <param name="storageRequirement">如果需要特殊存储，其具体要求（例如特定建筑ID或标签）。</param>
        private void AddResourceConfig(ResourceType type, string name, string description, ResourceCategory category, 
            bool canDecay, float decayRate, int maxStack, bool requiresSpecialStorage = false, string storageRequirement = "")
        {
            var config = new ResourceConfig // 创建新的资源配置实例
            {
                Type = type,
                Name = name,
                Description = description,
                Category = category,
                CanDecay = canDecay,
                DecayRate = decayRate,
                MaxStackSize = maxStack,
                RequiresSpecialStorage = requiresSpecialStorage,
                StorageRequirement = storageRequirement
            };
            _resourceConfigs[type] = config; // 将配置添加到字典中
        }
        
        #endregion
        
        #region 建筑配置 (Building Configurations)
        
        /// <summary>
        /// 加载所有建筑相关的配置数据。
        /// 通过调用辅助方法按建筑类别分别创建配置。
        /// </summary>
        private void LoadBuildingConfigs()
        {
            // 分类加载各种建筑的配置，使代码更模块化
            CreateProductionBuildings(); // 加载生产类建筑配置
            CreateDefenseBuildings();    // 加载防御类建筑配置
            CreateHabitatBuildings();    // 加载居住类建筑配置
            CreateStorageBuildings();    // 加载存储类建筑配置
            CreateFunctionalBuildings(); // 加载功能类建筑配置
        }
        
        /// <summary>
        /// 创建并添加所有生产类建筑的配置。
        /// </summary>
        private void CreateProductionBuildings()
        {
            // 农田系列 (Farm Series)
            AddBuildingConfig("farm_1", "简易农田", "提供基础的食物生产能力，需要分配幸存者进行耕作。",
                BuildingCategory.Production, BuildingLevel.Level1, // 类别与等级
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 50 } }, // 建造成本：50材料
                6f, new List<string>(), 2, // 建造时间6小时，无科技前置，最大工人2名
                new List<ResourceProduction> { new ResourceProduction { Type = ResourceType.Food, BaseRate = 5f, WorkerBonus = 2f, InputCosts = new List<ResourceCost>(), RequiresWorker = true } }); // 产出：食物，基础速率5/小时，每个工人额外+2
                
            AddBuildingConfig("farm_2", "改良农田", "经过改良的农田，食物生产效率更高。",
                BuildingCategory.Production, BuildingLevel.Level2,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 100 }, new ResourceCost { Type = ResourceType.Tools, Amount = 5 } }, // 成本：100材料, 5工具
                120f, new List<string> { "agriculture_1" }, 3, // 建造时间120小时 (应为BuildTime，不是小时), 前置科技"agriculture_1", 最大工人3名
                new List<ResourceProduction> { new ResourceProduction { Type = ResourceType.Food, BaseRate = 8f, WorkerBonus = 3f, RequiresWorker = true } }); // 产出：食物，基础速率8/小时，每个工人额外+3
                
            // 工坊系列 (Workshop Series)
            AddBuildingConfig("workshop_1", "简易工坊", "可以制造基础工具和修理物品的场所。",
                BuildingCategory.Production, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 80 } }, // 成本：80材料
                90f, new List<string>(), 2, // 建造时间90小时, 无前置, 最大工人2名
                new List<ResourceProduction> { new ResourceProduction { Type = ResourceType.Tools, BaseRate = 1f, WorkerBonus = 0.5f, RequiresWorker = true } }); // 产出：工具，基础速率1/小时，每个工人额外+0.5
        }
        
        /// <summary>
        /// 创建并添加所有防御类建筑的配置。
        /// </summary>
        private void CreateDefenseBuildings()
        {
            // 围墙系列 (Wall Series)
            AddBuildingConfig("wall_1", "木制围墙", "基础的防御性结构，能有效阻挡低级威胁。",
                BuildingCategory.Defense, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 30 } }, // 成本：30材料
                30f, new List<string>(), 0, null, 0, null, 0, 50f, 2f); // 建造时间30小时, 无工人, 无产出/存储/住房, 防御力50, 防御范围2
                
            // 哨塔系列 (Watchtower Series)
            AddBuildingConfig("watchtower_1", "简易哨塔", "提供视野并允许幸存者进行远程攻击。",
                BuildingCategory.Defense, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 60 } }, // 成本：60材料
                60f, new List<string>(), 1, null, 0, null, 0, 80f, 5f); // 建造时间60小时, 最大工人1 (哨兵), 防御力80, 防御范围5
        }
        
        /// <summary>
        /// 创建并添加所有居住类建筑的配置。
        /// </summary>
        private void CreateHabitatBuildings()
        {
            // 住所系列 (Shelter Series)
            AddBuildingConfig("shelter_1", "简易住所", "为幸存者提供基本的休息和庇护场所。",
                BuildingCategory.Habitat, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 40 } }, // 成本：40材料
                45f, new List<string>(), 0, null, 0, null, 2); // 建造时间45小时, 无工人, 住房容量2
        }
        
        /// <summary>
        /// 创建并添加所有存储类建筑的配置。
        /// </summary>
        private void CreateStorageBuildings()
        {
            // 储存系列 (Storage Series)
            AddBuildingConfig("storage_1", "简易储物箱", "提供基础的物品存储空间。",
                BuildingCategory.Storage, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 25 } }, // 成本：25材料
                20f, new List<string>(), 0, null, 500); // 建造时间20小时, 无工人, 存储容量500
        }
        
        /// <summary>
        /// 创建并添加所有功能类建筑的配置。
        /// </summary>
        private void CreateFunctionalBuildings()
        {
            // 水井 (Well)
            AddBuildingConfig("well_1", "水井", "从地下抽取清洁水源，是重要的生存设施。",
                BuildingCategory.Functional, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 100 } }, // 成本：100材料
                180f, new List<string>(), 1, // 建造时间180小时, 最大工人1名
                new List<ResourceProduction> { new ResourceProduction { Type = ResourceType.Water, BaseRate = 10f, WorkerBonus = 5f, RequiresWorker = true } }); // 产出：水，基础速率10/小时，工人额外+5
        }
        
        /// <summary>
        /// 向建筑配置字典中添加一个新的建筑配置项。
        /// </summary>
        /// <param name="id">建筑的唯一ID。</param>
        /// <param name="name">建筑的显示名称。</param>
        /// <param name="description">建筑的描述文本。</param>
        /// <param name="category">建筑所属类别。</param>
        /// <param name="level">建筑的等级。</param>
        /// <param name="buildCosts">建造成本列表。</param>
        /// <param name="buildTime">基础建造时间（小时）。</param>
        /// <param name="requiredTechs">解锁此建筑所需的前置科技ID列表。</param>
        /// <param name="maxWorkers">建筑可容纳的最大工人数。</param>
        /// <param name="productions">建筑的资源产出配置列表 (可选)。</param>
        /// <param name="storageCapacity">建筑提供的存储容量 (可选)。</param>
        /// <param name="storageTypes">建筑可存储的资源类型列表 (可选)。</param>
        /// <param name="housingCapacity">建筑提供的住房容量 (可选)。</param>
        /// <param name="defensePower">建筑的防御力 (可选)。</param>
        /// <param name="defenseRange">建筑的防御范围 (可选)。</param>
        private void AddBuildingConfig(string id, string name, string description, BuildingCategory category, BuildingLevel level,
            List<ResourceCost> buildCosts, float buildTime, List<string> requiredTechs, int maxWorkers,
            List<ResourceProduction> productions = null, int storageCapacity = 0, List<ResourceType> storageTypes = null,
            int housingCapacity = 0, float defensePower = 0f, float defenseRange = 0f)
        {
            var config = new BuildingConfig // 创建新的建筑配置实例
            {
                ConfigId = id, // 配置ID
                Name = name,   // 名称
                Description = description, // 描述
                Category = category, // 类别
                Level = level,       // 等级
                BuildCosts = buildCosts ?? new List<ResourceCost>(), // 建造成本 (确保列表不为null)
                BuildTime = buildTime, // 建造时间 (小时)
                RequiredTechs = requiredTechs ?? new List<string>(), // 前置科技 (确保列表不为null)
                MaxWorkers = maxWorkers, // 最大工人数
                RequiredAttributes = new List<SurvivorAttribute>(), // 需要的工人属性 (此处默认为空，可按需添加)
                MaintenanceCosts = new List<ResourceCost>(),      // 维护成本 (此处默认为空，可按需添加)
                Productions = productions ?? new List<ResourceProduction>(), // 资源产出 (确保列表不为null)
                AvailableRecipes = new List<CraftingRecipe>(),             // 可用配方 (此处默认为空)
                StorageCapacity = storageCapacity, // 存储容量
                StorageTypes = storageTypes ?? new List<ResourceType>(), // 可存储类型 (确保列表不为null)
                HousingCapacity = housingCapacity, // 住房容量
                DefensePower = defensePower,       // 防御力
                DefenseRange = defenseRange,       // 防御范围
                Size = new Vector2Int(1, 1)      // 占地大小 (默认为1x1，可按需修改)
            };
            _buildingConfigs[id] = config; // 将配置添加到字典
        }
        
        #endregion
        
        #region 科技配置 (Technology Configurations)
        
        /// <summary>
        /// 加载所有科技相关的配置数据。
        /// 通过调用辅助方法按科技树分支分别创建配置。
        /// </summary>
        private void LoadTechConfigs()
        {
            // 分类加载各科技树的配置
            CreateSurvivalTechs();    // 加载生存类科技配置
            CreateDefenseTechs();     // 加载防御类科技配置
            CreateHabitatTechs();     // 加载居住类科技配置
            CreateEngineeringTechs(); // 加载工程类科技配置
        }
        
        /// <summary>
        /// 创建并添加所有生存科技树的科技配置。
        /// </summary>
        private void CreateSurvivalTechs()
        {
            AddTechConfig("agriculture_1", "基础农业", "解锁改良农田，提高食物产量。",
                TechTree.Survival, TechTier.Tier1, // 科技树分支与层级
                50, 2f, new List<string>(), new List<string> { "farm_2" }); // 科研点50, 时间2小时, 无前置, 解锁建筑"farm_2"
                
            AddTechConfig("hunting_1", "狩猎技巧", "提升从狩猎中获取食物和材料的效率。",
                TechTree.Survival, TechTier.Tier1,
                40, 1.5f, new List<string>(), new List<string>()); // 科研点40, 时间1.5小时
        }
        
        /// <summary>
        /// 创建并添加所有防御科技树的科技配置。
        /// </summary>
        private void CreateDefenseTechs()
        {
            AddTechConfig("fortification_1", "基础防御工事", "解锁更坚固的墙体和基础防御塔。",
                TechTree.Defense, TechTier.Tier1,
                60, 2.5f, new List<string>(), new List<string> { "wall_2", "watchtower_2" }); // 解锁建筑"wall_2", "watchtower_2"
        }
        
        /// <summary>
        /// 创建并添加所有居住科技树的科技配置。
        /// </summary>
        private void CreateHabitatTechs()
        {
            AddTechConfig("construction_1", "改良建筑学", "提升建筑的建造速度和耐久度，解锁更高级的住所。",
                TechTree.Habitat, TechTier.Tier1,
                45, 2f, new List<string>(), new List<string> { "shelter_2" }); // 解锁建筑"shelter_2"
        }
        
        /// <summary>
        /// 创建并添加所有工程科技树的科技配置。
        /// </summary>
        private void CreateEngineeringTechs()
        {
            AddTechConfig("mechanics_1", "基础机械学", "解锁制造更复杂工具和零件的能力，改进工坊。",
                TechTree.Engineering, TechTier.Tier1,
                70, 3f, new List<string>(), new List<string> { "workshop_2" }); // 解锁建筑"workshop_2"
        }
        
        /// <summary>
        /// 向科技配置字典中添加一个新的科技配置项。
        /// </summary>
        /// <param name="id">科技的唯一ID。</param>
        /// <param name="name">科技的显示名称。</param>
        /// <param name="description">科技的描述文本。</param>
        /// <param name="tree">科技所属的科技树分支。</param>
        /// <param name="tier">科技的层级。</param>
        /// <param name="researchCost">研究所需的科研点数。</param>
        /// <param name="researchTime">基础研究时间（小时）。</param>
        /// <param name="prerequisites">研究此科技所需的前置科技ID列表。</param>
        /// <param name="unlockedBuildings">此科技研究完成后解锁的建筑ID列表。</param>
        private void AddTechConfig(string id, string name, string description, TechTree tree, TechTier tier,
            int researchCost, float researchTime, List<string> prerequisites, List<string> unlockedBuildings)
        {
            var config = new TechConfig // 创建新的科技配置实例
            {
                Id = id, // ID
                Name = name, // 名称
                Description = description, // 描述
                Tree = tree, // 所属科技树
                Tier = tier, // 层级
                ResearchPointsCost = researchCost, // 科研点成本
                ResearchTime = researchTime,       // 研究时间 (小时)
                Prerequisites = prerequisites ?? new List<string>(), // 前置科技 (确保列表不为null)
                AdditionalCosts = new List<ResourceCost>(), // 额外资源成本 (默认为空)
                MinResearchLevel = 1, // 最低研究设施等级 (默认为1)
                RequiredResearchers = new List<SurvivorAttribute>(), // 所需研究员特定属性 (默认为空)
                FailureRate = 0.1f, // 研究失败率 (默认为10%)
                UnlockedBuildings = unlockedBuildings ?? new List<string>(), // 解锁建筑 (确保列表不为null)
                UnlockedRecipes = new List<string>(), // 解锁配方 (默认为空)
                Effects = new List<TechEffect>(),     // 科技效果 (默认为空)
                UIPosition = Vector2.zero, // 在UI中的位置 (默认(0,0))
                UIColor = Color.white      // UI颜色 (默认白色)
            };
            _techConfigs[id] = config; // 将配置添加到字典
        }
        
        #endregion
        
        #region 制造配方 (Crafting Recipes)
        
        /// <summary>
        /// 加载所有制造配方的配置数据。
        /// </summary>
        private void LoadCraftingRecipes()
        {
            // 基础制造配方示例
            AddCraftingRecipe("craft_tools", "制造工具", "使用收集的材料和废料来制造基础的工具，提高工作效率。",
                ResourceType.Tools, 1, // 产出：1个工具
                new List<ResourceCost> {
                    new ResourceCost { Type = ResourceType.Materials, Amount = 10 }, // 需要：10材料
                    new ResourceCost { Type = ResourceType.Scrap, Amount = 5 }       // 需要：5废料
                },
                1f, 0, new List<string> { "workshop_1" }); // 制造时间1小时, 无科技等级要求, 需要建筑"workshop_1"
                
            AddCraftingRecipe("craft_ammo", "制造弹药", "将材料和废料转化为可用于防御的简易弹药。",
                ResourceType.Ammunition, 10, // 产出：10单位弹药
                new List<ResourceCost> {
                    new ResourceCost { Type = ResourceType.Materials, Amount = 5 }, // 需要：5材料
                    new ResourceCost { Type = ResourceType.Scrap, Amount = 10 }    // 需要：10废料
                },
                0.5f, 0, new List<string> { "workshop_1" }); // 制造时间0.5小时, 无科技等级要求, 需要建筑"workshop_1"
        }
        
        /// <summary>
        /// 向制造配方字典中添加一个新的配方配置项。
        /// </summary>
        /// <param name="id">配方的唯一ID。</param>
        /// <param name="name">配方的显示名称。</param>
        /// <param name="description">配方的描述文本。</param>
        /// <param name="outputType">产出资源的类型。</param>
        /// <param name="outputAmount">单次制造的产出数量。</param>
        /// <param name="inputCosts">制造成本列表（所需输入资源）。</param>
        /// <param name="craftingTime">基础制造时间（小时）。</param>
        /// <param name="requiredTechLevel">制造此配方所需的最低科技等级（通用或特定科技树等级）。</param>
        /// <param name="requiredBuildings">制造此配方所需的建筑ID列表（例如必须在某个工坊进行）。</param>
        private void AddCraftingRecipe(string id, string name, string description, ResourceType outputType, int outputAmount,
            List<ResourceCost> inputCosts, float craftingTime, int requiredTechLevel, List<string> requiredBuildings)
        {
            var recipe = new CraftingRecipe // 创建新的制造配方实例
            {
                Id = id, // ID
                Name = name, // 名称
                Description = description, // 描述
                OutputType = outputType,     // 产出资源类型
                OutputAmount = outputAmount,   // 产出数量
                InputCosts = inputCosts,       // 输入成本
                CraftingTime = craftingTime,     // 制造时间 (小时)
                RequiredTechLevel = requiredTechLevel, // 所需科技等级 (此参数可能需要更具体的科技ID关联)
                RequiredBuildings = requiredBuildings // 所需建筑ID列表
            };
            _craftingRecipes[id] = recipe; // 将配方添加到字典
        }
        
        #endregion
        
        #region 建筑升级 (Building Upgrades)
        
        /// <summary>
        /// 加载所有建筑升级路径的配置数据。
        /// </summary>
        private void LoadBuildingUpgrades()
        {
            // 农田升级示例：从简易农田(farm_1)升级到改良农田(farm_2)
            AddBuildingUpgrade("farm_1", "farm_2", // 源建筑ID, 目标建筑ID
                new List<ResourceCost> {
                    new ResourceCost { Type = ResourceType.Materials, Amount = 50 }, // 升级成本：50材料
                    new ResourceCost { Type = ResourceType.Tools, Amount = 2 }        // 升级成本：2工具
                },
                60f, new List<string> { "agriculture_1" }); // 升级时间60小时, 需要科技"agriculture_1"
        }
        
        /// <summary>
        /// 向建筑升级配置字典中添加一个新的升级路径配置项。
        /// </summary>
        /// <param name="fromId">源建筑的配置ID。</param>
        /// <param name="toId">目标建筑（升级后）的配置ID。</param>
        /// <param name="costs">升级所需的资源成本列表。</param>
        /// <param name="time">基础升级时间（小时）。</param>
        /// <param name="requiredTechs">完成此升级所需的前置科技ID列表。</param>
        private void AddBuildingUpgrade(string fromId, string toId, List<ResourceCost> costs, float time, List<string> requiredTechs)
        {
            var upgrade = new BuildingUpgrade // 创建新的建筑升级配置实例
            {
                FromConfigId = fromId, // 源建筑ID
                ToConfigId = toId,     // 目标建筑ID
                UpgradeCosts = costs,  // 升级成本
                UpgradeTime = time,    // 升级时间 (小时)
                RequiredTechs = requiredTechs // 所需前置科技
            };
            // 使用组合键确保唯一性
            _buildingUpgrades[$"{fromId}_to_{toId}"] = upgrade; // 将升级配置添加到字典
        }
        
        #endregion
        
        #region 公共接口 (Public API)
        
        /// <summary>
        /// 根据建筑配置ID获取对应的建筑配置信息。
        /// </summary>
        /// <param name="id">建筑配置ID。</param>
        /// <returns>对应的BuildingConfig实例；如果找不到，则返回null。</returns>
        public BuildingConfig GetBuildingConfig(string id)
        {
            return _buildingConfigs.TryGetValue(id, out var config) ? config : null;
        }
        
        /// <summary>
        /// 获取所有建筑配置信息的字典副本。
        /// </summary>
        /// <returns>一个新的字典，包含所有建筑配置，键为建筑配置ID。</returns>
        public Dictionary<string, BuildingConfig> GetAllBuildingConfigs()
        {
            return new Dictionary<string, BuildingConfig>(_buildingConfigs); // 返回副本以防外部修改
        }
        
        /// <summary>
        /// 根据指定的建筑类别获取该类别下的所有建筑配置列表。
        /// </summary>
        /// <param name="category">要查询的建筑类别。</param>
        /// <returns>属于该类别的建筑配置列表。</returns>
        public List<BuildingConfig> GetBuildingsByCategory(BuildingCategory category)
        {
            return _buildingConfigs.Values.Where(config => config.Category == category).ToList();
        }
        
        /// <summary>
        /// 根据科技配置ID获取对应的科技配置信息。
        /// </summary>
        /// <param name="id">科技配置ID。</param>
        /// <returns>对应的TechConfig实例；如果找不到，则返回null。</returns>
        public TechConfig GetTechConfig(string id)
        {
            return _techConfigs.TryGetValue(id, out var config) ? config : null;
        }
        
        /// <summary>
        /// 获取所有科技配置信息的字典副本。
        /// </summary>
        /// <returns>一个新的字典，包含所有科技配置，键为科技配置ID。</returns>
        public Dictionary<string, TechConfig> GetAllTechConfigs()
        {
            return new Dictionary<string, TechConfig>(_techConfigs); // 返回副本
        }
        
        /// <summary>
        /// 根据指定的科技树分支获取该分支下的所有科技配置列表。
        /// </summary>
        /// <param name="tree">要查询的科技树分支。</param>
        /// <returns>属于该科技树分支的科技配置列表。</returns>
        public List<TechConfig> GetTechsByTree(TechTree tree)
        {
            return _techConfigs.Values.Where(config => config.Tree == tree).ToList();
        }
        
        /// <summary>
        /// 根据资源类型枚举获取对应的资源配置信息。
        /// </summary>
        /// <param name="type">资源类型枚举。</param>
        /// <returns>对应的ResourceConfig实例；如果找不到，则返回null。</returns>
        public ResourceConfig GetResourceConfig(ResourceType type)
        {
            return _resourceConfigs.TryGetValue(type, out var config) ? config : null;
        }
        
        /// <summary>
        /// 获取所有资源配置信息的字典副本。
        /// </summary>
        /// <returns>一个新的字典，包含所有资源配置，键为资源类型枚举。</returns>
        public Dictionary<ResourceType, ResourceConfig> GetAllResourceConfigs()
        {
            return new Dictionary<ResourceType, ResourceConfig>(_resourceConfigs); // 返回副本
        }
        
        /// <summary>
        /// 根据制造配方ID获取对应的制造配方信息。
        /// </summary>
        /// <param name="id">制造配方ID。</param>
        /// <returns>对应的CraftingRecipe实例；如果找不到，则返回null。</returns>
        public CraftingRecipe GetCraftingRecipe(string id)
        {
            return _craftingRecipes.TryGetValue(id, out var recipe) ? recipe : null;
        }
        
        /// <summary>
        /// 获取所有制造配方信息的字典副本。
        /// </summary>
        /// <returns>一个新的字典，包含所有制造配方，键为配方ID。</returns>
        public Dictionary<string, CraftingRecipe> GetAllCraftingRecipes()
        {
            return new Dictionary<string, CraftingRecipe>(_craftingRecipes); // 返回副本
        }
        
        /// <summary>
        /// 根据源建筑ID和目标建筑ID获取对应的建筑升级配置信息。
        /// </summary>
        /// <param name="fromId">源建筑的配置ID。</param>
        /// <param name="toId">目标建筑（升级后）的配置ID。</param>
        /// <returns>对应的BuildingUpgrade实例；如果找不到，则返回null。</returns>
        public BuildingUpgrade GetBuildingUpgrade(string fromId, string toId)
        {
            string key = $"{fromId}_to_{toId}"; // 使用组合键查询
            return _buildingUpgrades.TryGetValue(key, out var upgrade) ? upgrade : null;
        }
        
        /// <summary>
        /// 检查指定的建筑是否已被已完成的科技解锁。
        /// </summary>
        /// <param name="buildingId">要检查的建筑的配置ID。</param>
        /// <param name="completedTechs">已完成的科技ID列表。</param>
        /// <returns>如果建筑的所有前置科技都已完成，则返回true；否则返回false。</returns>
        public bool IsBuildingUnlockedByTech(string buildingId, List<string> completedTechs)
        {
            var buildingConfig = GetBuildingConfig(buildingId); // 获取建筑配置
            if (buildingConfig == null)
            {
                Debug.LogWarning($"[配置系统] 检查建筑解锁状态失败：找不到建筑配置ID {buildingId}");
                return false; // 建筑配置不存在，视为未解锁
            }
            
            // 如果建筑没有科技前置要求，则视为已解锁
            if (buildingConfig.RequiredTechs == null || !buildingConfig.RequiredTechs.Any()) return true;

            // 检查所有必需的科技是否都在已完成列表中
            return buildingConfig.RequiredTechs.All(requiredTechId => completedTechs.Contains(requiredTechId));
        }
        
        /// <summary>
        /// 检查指定科技的所有前置科技是否都已完成。
        /// </summary>
        /// <param name="techId">要检查的科技的配置ID。</param>
        /// <param name="completedTechs">已完成的科技ID列表。</param>
        /// <returns>如果该科技的所有前置科技都已完成，则返回true；否则返回false。</returns>
        public bool AreTechPrerequisitesMet(string techId, List<string> completedTechs)
        {
            var techConfig = GetTechConfig(techId); // 获取科技配置
            if (techConfig == null)
            {
                Debug.LogWarning($"[配置系统] 检查科技前置条件失败：找不到科技配置ID {techId}");
                return false; // 科技配置不存在，视为前置未满足
            }

            // 如果科技没有前置要求，则视为已满足
            if (techConfig.Prerequisites == null || !techConfig.Prerequisites.Any()) return true;
            
            // 检查所有必需的前置科技是否都在已完成列表中
            return techConfig.Prerequisites.All(prereqId => completedTechs.Contains(prereqId));
        }
        
        #endregion
    }
} 