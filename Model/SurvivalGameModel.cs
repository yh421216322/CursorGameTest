using System.Collections.Generic;
using QFramework;
using MyGameNamespace;
using UnityEngine;
using System;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 功能：丧尸末日生存游戏核心数据模型
    /// 注册方式：在RegisterManager中调用RegisterModel<ISurvivalGameModel>(new SurvivalGameModel())
    /// </summary>
    public interface ISurvivalGameModel : QFIModel
    {
        // 基础资源数据
        BindableProperty<int> Food { get; set; }          // 食物
        BindableProperty<int> Materials { get; set; }     // 材料
        BindableProperty<int> Water { get; set; }         // 水
        BindableProperty<int> Scrap { get; set; }         // 废料
        BindableProperty<int> OrganicMatter { get; set; } // 有机物
        
        // 成品物资数据
        BindableProperty<int> Ammunition { get; set; }    // 弹药
        BindableProperty<int> MedicalSupplies { get; set; } // 医疗用品
        BindableProperty<int> Tools { get; set; }         // 工具
        BindableProperty<int> Fuel { get; set; }          // 燃料
        BindableProperty<int> Electronics { get; set; }   // 电子元件
        
        // 高级资源数据
        BindableProperty<int> Energy { get; set; }        // 能源
        BindableProperty<int> ResearchPoints { get; set; } // 科研点数
        BindableProperty<int> RareMetals { get; set; }    // 稀有金属
        
        // 游戏状态
        BindableProperty<int> Population { get; set; }    // 人口
        BindableProperty<int> Morale { get; set; }        // 士气
        BindableProperty<int> GameDay { get; set; }       // 游戏天数
        BindableProperty<float> GameTime { get; set; }    // 当日时间(0-1)
        BindableProperty<bool> IsPaused { get; set; }     // 游戏暂停状态
        
        // 建筑数据
        Dictionary<string, BuildingData> Buildings { get; set; }        // 已建造建筑
        
        // 人口数据
        Dictionary<string, SurvivorData> Survivors { get; set; }        // 幸存者信息
        Dictionary<string, List<string>> WorkerAssignment { get; set; } // 工作分配 建筑ID -> 工人ID列表
        
        // 科技数据
        Dictionary<string, TechData> Technologies { get; set; }         // 科技数据
        BindableProperty<string> CurrentResearch { get; set; }          // 当前研究项目
        BindableProperty<float> ResearchProgress { get; set; }          // 研究进度
        
        // 威胁数据
        BindableProperty<int> ZombieThreadLevel { get; set; }        // 丧尸威胁等级
        BindableProperty<float> NextAttackTime { get; set; }         // 下次袭击时间
        
        // 方法
        void InitializeGame();
        void AddResource(ResourceType type, int amount);
        bool ConsumeResource(ResourceType type, int amount);
        int GetResourceAmount(ResourceType type);
        void AddBuilding(BuildingData building);
        void RemoveBuilding(string buildingId);
    }
    
    public class SurvivalGameModel : AbstractModel, ISurvivalGameModel
    {
        // 基础资源
        public BindableProperty<int> Food { get; set; }
        public BindableProperty<int> Materials { get; set; }
        public BindableProperty<int> Water { get; set; }
        public BindableProperty<int> Scrap { get; set; }
        public BindableProperty<int> OrganicMatter { get; set; }
        
        // 成品物资
        public BindableProperty<int> Ammunition { get; set; }
        public BindableProperty<int> MedicalSupplies { get; set; }
        public BindableProperty<int> Tools { get; set; }
        public BindableProperty<int> Fuel { get; set; }
        public BindableProperty<int> Electronics { get; set; }
        
        // 高级资源
        public BindableProperty<int> Energy { get; set; }
        public BindableProperty<int> ResearchPoints { get; set; }
        public BindableProperty<int> RareMetals { get; set; }
        
        // 游戏状态
        public BindableProperty<int> Population { get; set; }
        public BindableProperty<int> Morale { get; set; }
        public BindableProperty<int> GameDay { get; set; }
        public BindableProperty<float> GameTime { get; set; }
        public BindableProperty<bool> IsPaused { get; set; }
        
        // 建筑数据
        public Dictionary<string, BuildingData> Buildings { get; set; }
        
        // 人口数据
        public Dictionary<string, SurvivorData> Survivors { get; set; }
        public Dictionary<string, List<string>> WorkerAssignment { get; set; }
        
        // 科技数据
        public Dictionary<string, TechData> Technologies { get; set; }
        public BindableProperty<string> CurrentResearch { get; set; }
        public BindableProperty<float> ResearchProgress { get; set; }
        
        // 威胁数据
        public BindableProperty<int> ZombieThreadLevel { get; set; }
        public BindableProperty<float> NextAttackTime { get; set; }
        
        protected override void OnInit()
        {
            InitializeProperties();
            InitializeCollections();
            InitializeGame();
        }
        
        private void InitializeProperties()
        {
            // 初始化基础资源
            Food = new BindableProperty<int> { Value = 100 };
            Materials = new BindableProperty<int> { Value = 50 };
            Water = new BindableProperty<int> { Value = 80 };
            Scrap = new BindableProperty<int> { Value = 30 };
            OrganicMatter = new BindableProperty<int> { Value = 20 };
            
            // 初始化成品物资
            Ammunition = new BindableProperty<int> { Value = 20 };
            MedicalSupplies = new BindableProperty<int> { Value = 10 };
            Tools = new BindableProperty<int> { Value = 5 };
            Fuel = new BindableProperty<int> { Value = 15 };
            Electronics = new BindableProperty<int> { Value = 2 };
            
            // 初始化高级资源
            Energy = new BindableProperty<int> { Value = 0 };
            ResearchPoints = new BindableProperty<int> { Value = 0 };
            RareMetals = new BindableProperty<int> { Value = 0 };
            
            // 初始化游戏状态
            Population = new BindableProperty<int> { Value = 5 };
            Morale = new BindableProperty<int> { Value = 75 };
            GameDay = new BindableProperty<int> { Value = 1 };
            GameTime = new BindableProperty<float> { Value = 0.5f };
            IsPaused = new BindableProperty<bool> { Value = false };
            
            // 初始化科技数据
            CurrentResearch = new BindableProperty<string> { Value = "" };
            ResearchProgress = new BindableProperty<float> { Value = 0f };
            
            // 初始化威胁数据
            ZombieThreadLevel = new BindableProperty<int> { Value = 1 };
            NextAttackTime = new BindableProperty<float> { Value = 3f };
        }
        
        private void InitializeCollections()
        {
            Buildings = new Dictionary<string, BuildingData>();
            Survivors = new Dictionary<string, SurvivorData>();
            WorkerAssignment = new Dictionary<string, List<string>>();
            Technologies = new Dictionary<string, TechData>();
        }
        
        public void InitializeGame()
        {
            // 初始化幸存者
            InitializeSurvivors();
            
            // 初始化工作分配
            InitializeWorkAssignment();
            
            // 初始化科技树
            InitializeTechTree();
        }
        
        private void InitializeSurvivors()
        {
            // 创建初始幸存者
            for (int i = 0; i < Population.Value; i++)
            {
                var survivorId = System.Guid.NewGuid().ToString();
                var survivor = new SurvivorData
                {
                    Id = survivorId,
                    Name = $"幸存者{i + 1}",
                    Age = UnityEngine.Random.Range(18, 60),
                    Gender = (Gender)(UnityEngine.Random.Range(0, 2)),
                    Health = UnityEngine.Random.Range(80, 100),
                    Morale = UnityEngine.Random.Range(70, 90),
                    Fatigue = UnityEngine.Random.Range(0, 20),
                    Hunger = UnityEngine.Random.Range(0, 30),
                    CurrentJob = SurvivorJob.Idle,
                    AssignedBuildingId = "",
                    WorkEfficiency = 1.0f,
                    Attributes = new List<SurvivorAttributeData>(),
                    LastActionTime = Time.time
                };
                
                // 初始化属性
                foreach (SurvivorAttributeType attrType in System.Enum.GetValues(typeof(SurvivorAttributeType)))
                {
                    var attr = new SurvivorAttributeData(attrType, UnityEngine.Random.Range(15, 35));
                    survivor.Attributes.Add(attr);
                }
                
                Survivors[survivorId] = survivor;
            }
        }
        
        private void InitializeWorkAssignment()
        {
            // 初始化工作分配字典
            // 实际分配会在建筑建造完成后进行
        }
        
        private void InitializeTechTree()
        {
            // 初始化基础科技状态
            var basicTechs = new string[] 
            { 
                "agriculture_1", "hunting_1", "fortification_1", 
                "construction_1", "mechanics_1" 
            };
            
            foreach (var techId in basicTechs)
            {
                Technologies[techId] = new TechData
                {
                    Id = techId,
                    Name = $"基础科技_{techId}",
                    Description = $"{techId}的描述",
                    Level = 1,
                    Cost = 100,
                    Duration = 1.0f,
                    IsResearched = false,
                    IsResearching = false,
                    ResearchProgress = 0f,
                    Prerequisites = new List<string>(),
                    UnlockedBy = new List<string>(),
                    Category = TechCategory.Production,
                    Tier = TechTier.Tier1
                };
            }
        }
        
        public void AddResource(ResourceType type, int amount)
        {
            switch (type)
            {
                // 基础物资
                case ResourceType.Food:
                    Food.Value += amount;
                    break;
                case ResourceType.Materials:
                    Materials.Value += amount;
                    break;
                case ResourceType.Water:
                    Water.Value += amount;
                    break;
                case ResourceType.Scrap:
                    Scrap.Value += amount;
                    break;
                case ResourceType.OrganicMatter:
                    OrganicMatter.Value += amount;
                    break;
                    
                // 成品物资
                case ResourceType.Ammunition:
                    Ammunition.Value += amount;
                    break;
                case ResourceType.MedicalSupplies:
                    MedicalSupplies.Value += amount;
                    break;
                case ResourceType.Tools:
                    Tools.Value += amount;
                    break;
                case ResourceType.Fuel:
                    Fuel.Value += amount;
                    break;
                case ResourceType.Electronics:
                    Electronics.Value += amount;
                    break;
                    
                // 高级资源
                case ResourceType.Energy:
                    Energy.Value += amount;
                    break;
                case ResourceType.ResearchPoints:
                    ResearchPoints.Value += amount;
                    break;
                case ResourceType.RareMetals:
                    RareMetals.Value += amount;
                    break;
            }
        }
        
        public bool ConsumeResource(ResourceType type, int amount)
        {
            switch (type)
            {
                // 基础物资
                case ResourceType.Food:
                    if (Food.Value >= amount)
                    {
                        Food.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.Materials:
                    if (Materials.Value >= amount)
                    {
                        Materials.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.Water:
                    if (Water.Value >= amount)
                    {
                        Water.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.Scrap:
                    if (Scrap.Value >= amount)
                    {
                        Scrap.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.OrganicMatter:
                    if (OrganicMatter.Value >= amount)
                    {
                        OrganicMatter.Value -= amount;
                        return true;
                    }
                    break;
                    
                // 成品物资
                case ResourceType.Ammunition:
                    if (Ammunition.Value >= amount)
                    {
                        Ammunition.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.MedicalSupplies:
                    if (MedicalSupplies.Value >= amount)
                    {
                        MedicalSupplies.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.Tools:
                    if (Tools.Value >= amount)
                    {
                        Tools.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.Fuel:
                    if (Fuel.Value >= amount)
                    {
                        Fuel.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.Electronics:
                    if (Electronics.Value >= amount)
                    {
                        Electronics.Value -= amount;
                        return true;
                    }
                    break;
                    
                // 高级资源
                case ResourceType.Energy:
                    if (Energy.Value >= amount)
                    {
                        Energy.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.ResearchPoints:
                    if (ResearchPoints.Value >= amount)
                    {
                        ResearchPoints.Value -= amount;
                        return true;
                    }
                    break;
                case ResourceType.RareMetals:
                    if (RareMetals.Value >= amount)
                    {
                        RareMetals.Value -= amount;
                        return true;
                    }
                    break;
            }
            return false;
        }
        
        public int GetResourceAmount(ResourceType type)
        {
            switch (type)
            {
                // 基础物资
                case ResourceType.Food: return Food.Value;
                case ResourceType.Materials: return Materials.Value;
                case ResourceType.Water: return Water.Value;
                case ResourceType.Scrap: return Scrap.Value;
                case ResourceType.OrganicMatter: return OrganicMatter.Value;
                
                // 成品物资
                case ResourceType.Ammunition: return Ammunition.Value;
                case ResourceType.MedicalSupplies: return MedicalSupplies.Value;
                case ResourceType.Tools: return Tools.Value;
                case ResourceType.Fuel: return Fuel.Value;
                case ResourceType.Electronics: return Electronics.Value;
                
                // 高级资源
                case ResourceType.Energy: return Energy.Value;
                case ResourceType.ResearchPoints: return ResearchPoints.Value;
                case ResourceType.RareMetals: return RareMetals.Value;
                
                default: return 0;
            }
        }
        
        public void AddBuilding(BuildingData building)
        {
            Buildings[building.Id] = building;
            
            // 初始化工作分配
            if (!WorkerAssignment.ContainsKey(building.Id))
            {
                WorkerAssignment[building.Id] = new List<string>();
            }
        }
        
        public void RemoveBuilding(string buildingId)
        {
            if (Buildings.ContainsKey(buildingId))
            {
                // 移除工人分配
                if (WorkerAssignment.ContainsKey(buildingId))
                {
                    var workers = WorkerAssignment[buildingId];
                    foreach (var workerId in workers)
                    {
                        if (Survivors.ContainsKey(workerId))
                        {
                            Survivors[workerId].CurrentJob = SurvivorJob.Idle;
                            Survivors[workerId].AssignedBuildingId = "";
                        }
                    }
                    WorkerAssignment.Remove(buildingId);
                }
                
                Buildings.Remove(buildingId);
            }
        }
    }
    
    // 保留旧的枚举定义以兼容现有代码
    public enum BuildingType
    {
        Defense,     // 防御类
        Production,  // 生产类
        Residential  // 居住类
    }
    
    public enum JobType
    {
        Idle,        // 空闲
        Builder,     // 建造工
        Forager,     // 搜刮队
        Researcher,  // 研究员
        Guard,       // 守卫
        Medic        // 医护
    }
}