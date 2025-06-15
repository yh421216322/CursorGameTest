using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 配置管理系统 - 负责加载和管理所有游戏配置数据
    /// </summary>
    public class ConfigSystem : AbstractSystem
    {
        #region 配置数据存储
        
        private Dictionary<string, BuildingConfig> _buildingConfigs;
        private Dictionary<string, TechConfig> _techConfigs;
        private Dictionary<ResourceType, ResourceConfig> _resourceConfigs;
        private Dictionary<string, CraftingRecipe> _craftingRecipes;
        private Dictionary<string, BuildingUpgrade> _buildingUpgrades;
        
        #endregion
        
        #region 系统初始化
        
        protected override void OnInit()
        {
            _buildingConfigs = new Dictionary<string, BuildingConfig>();
            _techConfigs = new Dictionary<string, TechConfig>();
            _resourceConfigs = new Dictionary<ResourceType, ResourceConfig>();
            _craftingRecipes = new Dictionary<string, CraftingRecipe>();
            _buildingUpgrades = new Dictionary<string, BuildingUpgrade>();
            
            LoadAllConfigs();
        }
        
        /// <summary>
        /// 加载所有配置数据
        /// </summary>
        private void LoadAllConfigs()
        {
            LoadResourceConfigs();
            LoadBuildingConfigs();
            LoadTechConfigs();
            LoadCraftingRecipes();
            LoadBuildingUpgrades();
            
            Debug.Log($"配置系统初始化完成 - 建筑:{_buildingConfigs.Count}, 科技:{_techConfigs.Count}, 资源:{_resourceConfigs.Count}, 配方:{_craftingRecipes.Count}");
        }
        
        #endregion
        
        #region 资源配置
        
        /// <summary>
        /// 加载资源配置
        /// </summary>
        private void LoadResourceConfigs()
        {
            // 基础物资配置
            AddResourceConfig(ResourceType.Food, "食物", "维持幸存者生存的基本需求", ResourceCategory.BasicMaterial, true, 0.1f, 1000);
            AddResourceConfig(ResourceType.Materials, "材料", "建造和制造的基础材料", ResourceCategory.BasicMaterial, false, 0f, 2000);
            AddResourceConfig(ResourceType.Water, "水", "生存必需的清洁水源", ResourceCategory.BasicMaterial, false, 0f, 500);
            AddResourceConfig(ResourceType.Scrap, "废料", "可回收利用的废弃材料", ResourceCategory.BasicMaterial, false, 0f, 5000);
            AddResourceConfig(ResourceType.OrganicMatter, "有机物", "可用于堆肥和生物燃料的有机材料", ResourceCategory.BasicMaterial, true, 0.05f, 1500);
            
            // 成品物资配置
            AddResourceConfig(ResourceType.Ammunition, "弹药", "防御和狩猎用的弹药", ResourceCategory.Manufactured, false, 0f, 500);
            AddResourceConfig(ResourceType.MedicalSupplies, "医疗用品", "治疗伤病的医疗物资", ResourceCategory.Manufactured, true, 0.02f, 200);
            AddResourceConfig(ResourceType.Tools, "工具", "提高工作效率的各种工具", ResourceCategory.Manufactured, false, 0f, 100);
            AddResourceConfig(ResourceType.Fuel, "燃料", "发电机和载具使用的燃料", ResourceCategory.Manufactured, false, 0f, 300);
            AddResourceConfig(ResourceType.Electronics, "电子元件", "高科技设备的核心组件", ResourceCategory.Manufactured, false, 0f, 50);
            
            // 高级资源配置
            AddResourceConfig(ResourceType.Energy, "能源", "驱动各种设施的电力", ResourceCategory.Advanced, false, 0f, 10000);
            AddResourceConfig(ResourceType.ResearchPoints, "科研点数", "推进科技发展的研究积累", ResourceCategory.Advanced, false, 0f, 99999);
            AddResourceConfig(ResourceType.RareMetals, "稀有金属", "制造高级设备的珍贵材料", ResourceCategory.Advanced, false, 0f, 20);
        }
        
        private void AddResourceConfig(ResourceType type, string name, string description, ResourceCategory category, 
            bool canDecay, float decayRate, int maxStack, bool requiresSpecialStorage = false, string storageRequirement = "")
        {
            var config = new ResourceConfig
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
            _resourceConfigs[type] = config;
        }
        
        #endregion
        
        #region 建筑配置
        
        /// <summary>
        /// 加载建筑配置
        /// </summary>
        private void LoadBuildingConfigs()
        {
            // 生产型建筑
            CreateProductionBuildings();
            // 防御型建筑
            CreateDefenseBuildings();
            // 居住型建筑
            CreateHabitatBuildings();
            // 储存型建筑
            CreateStorageBuildings();
            // 功能型建筑
            CreateFunctionalBuildings();
        }
        
        private void CreateProductionBuildings()
        {
            // 农田系列
            AddBuildingConfig("farm_1", "简易农田", "基础的食物生产设施", BuildingCategory.Production, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 50 } },
                6f, new List<string>(), 2,
                new List<ResourceProduction> { new ResourceProduction { Type = ResourceType.Food, BaseRate = 5f, WorkerBonus = 2f, InputCosts = new List<ResourceCost>(), RequiresWorker = true } });
                
            AddBuildingConfig("farm_2", "改良农田", "效率更高的农业生产", BuildingCategory.Production, BuildingLevel.Level2,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 100 }, new ResourceCost { Type = ResourceType.Tools, Amount = 5 } },
                120f, new List<string> { "agriculture_1" }, 3,
                new List<ResourceProduction> { new ResourceProduction { Type = ResourceType.Food, BaseRate = 8f, WorkerBonus = 3f, RequiresWorker = true } });
                
            // 工坊系列
            AddBuildingConfig("workshop_1", "简易工坊", "基础的制造设施", BuildingCategory.Production, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 80 } },
                90f, new List<string>(), 2,
                new List<ResourceProduction> { new ResourceProduction { Type = ResourceType.Tools, BaseRate = 1f, WorkerBonus = 0.5f, RequiresWorker = true } });
        }
        
        private void CreateDefenseBuildings()
        {
            // 围墙系列
            AddBuildingConfig("wall_1", "木制围墙", "基础防御结构", BuildingCategory.Defense, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 30 } },
                30f, new List<string>(), 0, null, 0, null, 0, 50f, 2f);
                
            // 哨塔系列
            AddBuildingConfig("watchtower_1", "简易哨塔", "提供远程防御", BuildingCategory.Defense, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 60 } },
                60f, new List<string>(), 1, null, 0, null, 0, 80f, 5f);
        }
        
        private void CreateHabitatBuildings()
        {
            // 住所系列
            AddBuildingConfig("shelter_1", "简易住所", "基础的居住设施", BuildingCategory.Habitat, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 40 } },
                45f, new List<string>(), 0, null, 0, null, 2);
        }
        
        private void CreateStorageBuildings()
        {
            // 储存系列
            AddBuildingConfig("storage_1", "简易储物箱", "基础储存设施", BuildingCategory.Storage, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 25 } },
                20f, new List<string>(), 0, null, 500);
        }
        
        private void CreateFunctionalBuildings()
        {
            // 功能建筑
            AddBuildingConfig("well_1", "水井", "提供清洁水源", BuildingCategory.Functional, BuildingLevel.Level1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 100 } },
                180f, new List<string>(), 1,
                new List<ResourceProduction> { new ResourceProduction { Type = ResourceType.Water, BaseRate = 10f, WorkerBonus = 5f, RequiresWorker = true } });
        }
        
        private void AddBuildingConfig(string id, string name, string description, BuildingCategory category, BuildingLevel level,
            List<ResourceCost> buildCosts, float buildTime, List<string> requiredTechs, int maxWorkers,
            List<ResourceProduction> productions = null, int storageCapacity = 0, List<ResourceType> storageTypes = null,
            int housingCapacity = 0, float defensePower = 0f, float defenseRange = 0f)
        {
            var config = new BuildingConfig
            {
                ConfigId = id,
                Name = name,
                Description = description,
                Category = category,
                Level = level,
                BuildCosts = buildCosts ?? new List<ResourceCost>(),
                BuildTime = buildTime,
                RequiredTechs = requiredTechs ?? new List<string>(),
                MaxWorkers = maxWorkers,
                RequiredAttributes = new List<SurvivorAttribute>(),
                MaintenanceCosts = new List<ResourceCost>(),
                Productions = productions ?? new List<ResourceProduction>(),
                AvailableRecipes = new List<CraftingRecipe>(),
                StorageCapacity = storageCapacity,
                StorageTypes = storageTypes ?? new List<ResourceType>(),
                HousingCapacity = housingCapacity,
                DefensePower = defensePower,
                DefenseRange = defenseRange,
                Size = new Vector2Int(1, 1)
            };
            _buildingConfigs[id] = config;
        }
        
        #endregion
        
        #region 科技配置
        
        /// <summary>
        /// 加载科技配置
        /// </summary>
        private void LoadTechConfigs()
        {
            // 生存科技树
            CreateSurvivalTechs();
            // 防御科技树
            CreateDefenseTechs();
            // 居住科技树
            CreateHabitatTechs();
            // 工程科技树
            CreateEngineeringTechs();
        }
        
        private void CreateSurvivalTechs()
        {
            AddTechConfig("agriculture_1", "基础农业", "学会基本的农作物种植技术", TechTree.Survival, TechTier.Tier1,
                50, 2f, new List<string>(), new List<string> { "farm_2" });
                
            AddTechConfig("hunting_1", "狩猎技巧", "提高食物获取效率", TechTree.Survival, TechTier.Tier1,
                40, 1.5f, new List<string>(), new List<string>());
        }
        
        private void CreateDefenseTechs()
        {
            AddTechConfig("fortification_1", "基础防御", "学会建造基本防御设施", TechTree.Defense, TechTier.Tier1,
                60, 2.5f, new List<string>(), new List<string> { "wall_2", "watchtower_2" });
        }
        
        private void CreateHabitatTechs()
        {
            AddTechConfig("construction_1", "基础建造", "改进建筑技术", TechTree.Habitat, TechTier.Tier1,
                45, 2f, new List<string>(), new List<string> { "shelter_2" });
        }
        
        private void CreateEngineeringTechs()
        {
            AddTechConfig("mechanics_1", "基础机械", "学会制造简单工具", TechTree.Engineering, TechTier.Tier1,
                70, 3f, new List<string>(), new List<string> { "workshop_2" });
        }
        
        private void AddTechConfig(string id, string name, string description, TechTree tree, TechTier tier,
            int researchCost, float researchTime, List<string> prerequisites, List<string> unlockedBuildings)
        {
            var config = new TechConfig
            {
                Id = id,
                Name = name,
                Description = description,
                Tree = tree,
                Tier = tier,
                ResearchPointsCost = researchCost,
                ResearchTime = researchTime,
                Prerequisites = prerequisites ?? new List<string>(),
                AdditionalCosts = new List<ResourceCost>(),
                MinResearchLevel = 1,
                RequiredResearchers = new List<SurvivorAttribute>(),
                FailureRate = 0.1f,
                UnlockedBuildings = unlockedBuildings ?? new List<string>(),
                UnlockedRecipes = new List<string>(),
                Effects = new List<TechEffect>(),
                UIPosition = Vector2.zero,
                UIColor = Color.white
            };
            _techConfigs[id] = config;
        }
        
        #endregion
        
        #region 制造配方
        
        private void LoadCraftingRecipes()
        {
            // 基础制造配方
            AddCraftingRecipe("craft_tools", "制造工具", "用材料制造基础工具",
                ResourceType.Tools, 1,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 10 }, new ResourceCost { Type = ResourceType.Scrap, Amount = 5 } },
                1f, 0, new List<string> { "workshop_1" });
                
            AddCraftingRecipe("craft_ammo", "制造弹药", "生产防御用弹药",
                ResourceType.Ammunition, 10,
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 5 }, new ResourceCost { Type = ResourceType.Scrap, Amount = 10 } },
                0.5f, 0, new List<string> { "workshop_1" });
        }
        
        private void AddCraftingRecipe(string id, string name, string description, ResourceType outputType, int outputAmount,
            List<ResourceCost> inputCosts, float craftingTime, int requiredTechLevel, List<string> requiredBuildings)
        {
            var recipe = new CraftingRecipe
            {
                Id = id,
                Name = name,
                Description = description,
                OutputType = outputType,
                OutputAmount = outputAmount,
                InputCosts = inputCosts,
                CraftingTime = craftingTime,
                RequiredTechLevel = requiredTechLevel,
                RequiredBuildings = requiredBuildings
            };
            _craftingRecipes[id] = recipe;
        }
        
        #endregion
        
        #region 建筑升级
        
        private void LoadBuildingUpgrades()
        {
            // 农田升级
            AddBuildingUpgrade("farm_1", "farm_2",
                new List<ResourceCost> { new ResourceCost { Type = ResourceType.Materials, Amount = 50 }, new ResourceCost { Type = ResourceType.Tools, Amount = 2 } },
                60f, new List<string> { "agriculture_1" });
        }
        
        private void AddBuildingUpgrade(string fromId, string toId, List<ResourceCost> costs, float time, List<string> requiredTechs)
        {
            var upgrade = new BuildingUpgrade
            {
                FromConfigId = fromId,
                ToConfigId = toId,
                UpgradeCosts = costs,
                UpgradeTime = time,
                RequiredTechs = requiredTechs
            };
            _buildingUpgrades[$"{fromId}_to_{toId}"] = upgrade;
        }
        
        #endregion
        
        #region 公共接口
        
        /// <summary>
        /// 获取建筑配置
        /// </summary>
        public BuildingConfig GetBuildingConfig(string id)
        {
            return _buildingConfigs.TryGetValue(id, out var config) ? config : null;
        }
        
        /// <summary>
        /// 获取所有建筑配置
        /// </summary>
        public Dictionary<string, BuildingConfig> GetAllBuildingConfigs()
        {
            return new Dictionary<string, BuildingConfig>(_buildingConfigs);
        }
        
        /// <summary>
        /// 根据类别获取建筑配置
        /// </summary>
        public List<BuildingConfig> GetBuildingsByCategory(BuildingCategory category)
        {
            return _buildingConfigs.Values.Where(config => config.Category == category).ToList();
        }
        
        /// <summary>
        /// 获取科技配置
        /// </summary>
        public TechConfig GetTechConfig(string id)
        {
            return _techConfigs.TryGetValue(id, out var config) ? config : null;
        }
        
        /// <summary>
        /// 获取所有科技配置
        /// </summary>
        public Dictionary<string, TechConfig> GetAllTechConfigs()
        {
            return new Dictionary<string, TechConfig>(_techConfigs);
        }
        
        /// <summary>
        /// 根据科技树获取科技配置
        /// </summary>
        public List<TechConfig> GetTechsByTree(TechTree tree)
        {
            return _techConfigs.Values.Where(config => config.Tree == tree).ToList();
        }
        
        /// <summary>
        /// 获取资源配置
        /// </summary>
        public ResourceConfig GetResourceConfig(ResourceType type)
        {
            return _resourceConfigs.TryGetValue(type, out var config) ? config : null;
        }
        
        /// <summary>
        /// 获取所有资源配置
        /// </summary>
        public Dictionary<ResourceType, ResourceConfig> GetAllResourceConfigs()
        {
            return new Dictionary<ResourceType, ResourceConfig>(_resourceConfigs);
        }
        
        /// <summary>
        /// 获取制造配方
        /// </summary>
        public CraftingRecipe GetCraftingRecipe(string id)
        {
            return _craftingRecipes.TryGetValue(id, out var recipe) ? recipe : null;
        }
        
        /// <summary>
        /// 获取所有制造配方
        /// </summary>
        public Dictionary<string, CraftingRecipe> GetAllCraftingRecipes()
        {
            return new Dictionary<string, CraftingRecipe>(_craftingRecipes);
        }
        
        /// <summary>
        /// 获取建筑升级配置
        /// </summary>
        public BuildingUpgrade GetBuildingUpgrade(string fromId, string toId)
        {
            string key = $"{fromId}_to_{toId}";
            return _buildingUpgrades.TryGetValue(key, out var upgrade) ? upgrade : null;
        }
        
        /// <summary>
        /// 检查科技是否解锁建筑
        /// </summary>
        public bool IsBuildingUnlockedByTech(string buildingId, List<string> completedTechs)
        {
            var buildingConfig = GetBuildingConfig(buildingId);
            if (buildingConfig == null) return false;
            
            return buildingConfig.RequiredTechs.All(techId => completedTechs.Contains(techId));
        }
        
        /// <summary>
        /// 检查科技前置条件
        /// </summary>
        public bool AreTechPrerequisitesMet(string techId, List<string> completedTechs)
        {
            var techConfig = GetTechConfig(techId);
            if (techConfig == null) return false;
            
            return techConfig.Prerequisites.All(prereqId => completedTechs.Contains(prereqId));
        }
        
        #endregion
    }
} 