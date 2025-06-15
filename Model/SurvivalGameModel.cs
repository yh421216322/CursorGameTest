// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：SurvivalGameModel.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了丧尸末日生存游戏的核心数据模型接口 (ISurvivalGameModel)
//     及其具体实现 (SurvivalGameModel)。该模型遵循QFramework框架的规范，
//     用于集中存储和管理游戏全局的、可被UI或其他系统绑定的状态数据。
//     这些数据包括各类资源、游戏状态（如天数、时间、暂停）、建筑、幸存者、
//     科技以及丧尸威胁等级等。
// ==============================================================================

using System.Collections.Generic;
using QFramework;
using MyGameNamespace; // 假设包含事件定义或此命名空间下的其他模型 (当前文件中未直接使用)
using UnityEngine;     // 用于访问UnityEngine的Time.time等
using System;          // 用于Guid等

namespace SurvivalGame.Model
{
    /// <summary>
    /// 丧尸末日生存游戏核心数据模型接口。
    /// 继承自QFramework的QFIModel，定义了游戏全局状态属性和基本操作方法。
    /// 其他系统或UI可以通过此接口访问和监听模型数据的变化。
    /// 注册方式通常是在项目的架构初始化部分（如RegisterManager或应用启动类）中
    /// 调用 this.RegisterModel<ISurvivalGameModel>(new SurvivalGameModel())。
    /// </summary>
    public interface ISurvivalGameModel : QFIModel // QFIModel是QFramework中模型接口的基类
    {
        // --- 基础资源数据 ---
        // BindableProperty是QFramework提供的特性，允许UI或其他逻辑单元订阅其值的变化。
        /// <summary>
        /// 食物资源数量。可绑定属性，其值变化时可通知监听者。
        /// </summary>
        BindableProperty<int> Food { get; set; }          // 食物
        /// <summary>
        /// 建筑材料资源数量。
        /// </summary>
        BindableProperty<int> Materials { get; set; }     // 材料
        /// <summary>
        /// 水资源数量。
        /// </summary>
        BindableProperty<int> Water { get; set; }         // 水
        /// <summary>
        /// 废料资源数量。
        /// </summary>
        BindableProperty<int> Scrap { get; set; }         // 废料
        /// <summary>
        /// 有机物资源数量。
        /// </summary>
        BindableProperty<int> OrganicMatter { get; set; } // 有机物
        
        // --- 成品物资数据 ---
        /// <summary>
        /// 弹药数量。
        /// </summary>
        BindableProperty<int> Ammunition { get; set; }    // 弹药
        /// <summary>
        /// 医疗用品数量。
        /// </summary>
        BindableProperty<int> MedicalSupplies { get; set; } // 医疗用品
        /// <summary>
        /// 工具数量。
        /// </summary>
        BindableProperty<int> Tools { get; set; }         // 工具
        /// <summary>
        /// 燃料数量。
        /// </summary>
        BindableProperty<int> Fuel { get; set; }          // 燃料
        /// <summary>
        /// 电子元件数量。
        /// </summary>
        BindableProperty<int> Electronics { get; set; }   // 电子元件
        
        // --- 高级资源数据 ---
        /// <summary>
        /// 能源/电力总量。
        /// </summary>
        BindableProperty<int> Energy { get; set; }        // 能源
        /// <summary>
        /// 当前拥有的科研点数。
        /// </summary>
        BindableProperty<int> ResearchPoints { get; set; } // 科研点数
        /// <summary>
        /// 稀有金属数量。
        /// </summary>
        BindableProperty<int> RareMetals { get; set; }    // 稀有金属
        
        // --- 游戏状态数据 ---
        /// <summary>
        /// 当前幸存者人口数量。
        /// </summary>
        BindableProperty<int> Population { get; set; }    // 人口
        /// <summary>
        /// 队伍整体士气值 (通常为百分比)。
        /// </summary>
        BindableProperty<int> Morale { get; set; }        // 士气
        /// <summary>
        /// 当前游戏进行到的天数。
        /// </summary>
        BindableProperty<int> GameDay { get; set; }       // 游戏天数
        /// <summary>
        /// 当天的时间进度 (通常为0到1之间，代表从0点到24点)。
        /// </summary>
        BindableProperty<float> GameTime { get; set; }    // 当日时间(0-1)
        /// <summary>
        /// 游戏当前是否处于暂停状态。
        /// </summary>
        BindableProperty<bool> IsPaused { get; set; }     // 游戏暂停状态
        
        // --- 建筑相关数据 ---
        /// <summary>
        /// 存储所有已建造或正在建造的建筑实例数据。
        /// 键为建筑实例的唯一ID (string)，值为BuildingData对象。
        /// </summary>
        Dictionary<string, BuildingData> Buildings { get; set; }        // 已建造建筑
        
        // --- 人口（幸存者）相关数据 ---
        /// <summary>
        /// 存储所有幸存者的详细信息。
        /// 键为幸存者的唯一ID (string)，值为SurvivorData对象。
        /// </summary>
        Dictionary<string, SurvivorData> Survivors { get; set; }        // 幸存者信息
        /// <summary>
        /// 记录工作分配情况。
        /// 键为建筑实例的唯一ID (string)，值为一个字符串列表，列表中存储分配到该建筑工作的幸存者ID。
        /// </summary>
        Dictionary<string, List<string>> WorkerAssignment { get; set; } // 工作分配 建筑ID -> 工人ID列表
        
        // --- 科技研发相关数据 ---
        /// <summary>
        /// 存储所有已定义科技的状态和数据。
        /// 键为科技的唯一ID (string)，值为TechData对象。
        /// </summary>
        Dictionary<string, TechData> Technologies { get; set; }         // 科技数据
        /// <summary>
        /// 当前正在研究的科技项目的ID。如果为空字符串或null，则表示当前没有研究项目。
        /// </summary>
        BindableProperty<string> CurrentResearch { get; set; }          // 当前研究项目
        /// <summary>
        /// 当前研究项目的进度 (通常为0到1之间)。
        /// </summary>
        BindableProperty<float> ResearchProgress { get; set; }          // 研究进度
        
        // --- 丧尸威胁相关数据 ---
        /// <summary>
        /// 当前的丧尸威胁等级。数值越高，威胁越大。 (原字段名ZombieThreadLevel有拼写错误)
        /// </summary>
        BindableProperty<int> ZombieThreatLevel { get; set; } // 更正: Threat 而非 Thread
        /// <summary>
        /// 下一次丧尸袭击预计发生的游戏时间点或倒计时。
        /// </summary>
        BindableProperty<float> NextAttackTime { get; set; }         // 下次袭击时间
        
        // --- 模型操作方法 ---
        /// <summary>
        /// 初始化游戏模型的核心数据，例如创建初始幸存者、设置初始科技状态等。
        /// </summary>
        void InitializeGame();
        /// <summary>
        /// 增加指定类型的资源数量。
        /// </summary>
        /// <param name="type">要增加的资源类型。</param>
        /// <param name="amount">要增加的数量 (应为正数)。</param>
        void AddResource(ResourceType type, int amount);
        /// <summary>
        /// 消耗指定类型的资源数量。
        /// </summary>
        /// <param name="type">要消耗的资源类型。</param>
        /// <param name="amount">要消耗的数量 (应为正数)。</param>
        /// <returns>如果资源足够并成功消耗则返回true，否则返回false。</returns>
        bool ConsumeResource(ResourceType type, int amount);
        /// <summary>
        /// 获取指定类型的当前资源数量。
        /// </summary>
        /// <param name="type">要查询的资源类型。</param>
        /// <returns>该资源的当前数量。</returns>
        int GetResourceAmount(ResourceType type);
        /// <summary>
        /// 向模型中添加一个新的建筑实例数据。
        /// </summary>
        /// <param name="building">要添加的建筑数据对象。</param>
        void AddBuilding(BuildingData building);
        /// <summary>
        /// 从模型中移除指定ID的建筑实例数据，并处理相关的工人解配。
        /// </summary>
        /// <param name="buildingId">要移除的建筑实例的唯一ID。</param>
        void RemoveBuilding(string buildingId);
    }
    
    /// <summary>
    /// 丧尸末日生存游戏核心数据模型的具体实现类。
    /// 继承自QFramework的AbstractModel，并实现了ISurvivalGameModel接口。
    /// </summary>
    public class SurvivalGameModel : AbstractModel, ISurvivalGameModel
    {
        // --- 基础资源 BindableProperty 实现 ---
        public BindableProperty<int> Food { get; set; }
        public BindableProperty<int> Materials { get; set; }
        public BindableProperty<int> Water { get; set; }
        public BindableProperty<int> Scrap { get; set; }
        public BindableProperty<int> OrganicMatter { get; set; }
        
        // --- 成品物资 BindableProperty 实现 ---
        public BindableProperty<int> Ammunition { get; set; }
        public BindableProperty<int> MedicalSupplies { get; set; }
        public BindableProperty<int> Tools { get; set; }
        public BindableProperty<int> Fuel { get; set; }
        public BindableProperty<int> Electronics { get; set; }
        
        // --- 高级资源 BindableProperty 实现 ---
        public BindableProperty<int> Energy { get; set; }
        public BindableProperty<int> ResearchPoints { get; set; }
        public BindableProperty<int> RareMetals { get; set; }
        
        // --- 游戏状态 BindableProperty 实现 ---
        public BindableProperty<int> Population { get; set; }
        public BindableProperty<int> Morale { get; set; }
        public BindableProperty<int> GameDay { get; set; }
        public BindableProperty<float> GameTime { get; set; }
        public BindableProperty<bool> IsPaused { get; set; }
        
        // --- 建筑数据 Dictionary 实现 ---
        public Dictionary<string, BuildingData> Buildings { get; set; }
        
        // --- 人口数据 Dictionary 实现 ---
        public Dictionary<string, SurvivorData> Survivors { get; set; }
        public Dictionary<string, List<string>> WorkerAssignment { get; set; }
        
        // --- 科技数据 Dictionary 和 BindableProperty 实现 ---
        public Dictionary<string, TechData> Technologies { get; set; }
        public BindableProperty<string> CurrentResearch { get; set; }
        public BindableProperty<float> ResearchProgress { get; set; }
        
        // --- 威胁数据 BindableProperty 实现 ---
        public BindableProperty<int> ZombieThreatLevel { get; set; } // 原为ZombieThreadLevel，已在接口中修正
        public BindableProperty<float> NextAttackTime { get; set; }
        
        /// <summary>
        /// QFramework模型初始化方法。在模型首次被获取时自动调用。
        /// 用于设置所有属性和集合的初始值。
        /// </summary>
        protected override void OnInit()
        {
            InitializeProperties();    // 初始化所有BindableProperty
            InitializeCollections();   // 初始化所有字典和列表等集合类型
            InitializeGame();          // 执行游戏逻辑相关的初始化（如创建初始幸存者）
        }
        
        /// <summary>
        /// 初始化所有BindableProperty属性，赋予它们初始值。
        /// </summary>
        private void InitializeProperties()
        {
            // 初始化基础资源，并设定初始值
            Food = new BindableProperty<int>(100); // 初始食物100
            Materials = new BindableProperty<int>(50); // 初始材料50
            Water = new BindableProperty<int>(80);   // 初始水80
            Scrap = new BindableProperty<int>(30);   // 初始废料30
            OrganicMatter = new BindableProperty<int>(20); // 初始有机物20
            
            // 初始化成品物资
            Ammunition = new BindableProperty<int>(20);
            MedicalSupplies = new BindableProperty<int>(10);
            Tools = new BindableProperty<int>(5);
            Fuel = new BindableProperty<int>(15);
            Electronics = new BindableProperty<int>(2);
            
            // 初始化高级资源
            Energy = new BindableProperty<int>(0);
            ResearchPoints = new BindableProperty<int>(0);
            RareMetals = new BindableProperty<int>(0);
            
            // 初始化游戏状态
            Population = new BindableProperty<int>(5); // 初始人口5
            Morale = new BindableProperty<int>(75);    // 初始士气75%
            GameDay = new BindableProperty<int>(1);    // 从第1天开始
            GameTime = new BindableProperty<float>(0.25f); // 假设0.25代表早上6点 (一天从0到1)
            IsPaused = new BindableProperty<bool>(false); // 游戏开始时不暂停
            
            // 初始化科技相关数据
            CurrentResearch = new BindableProperty<string>(""); // 初始没有研究项目
            ResearchProgress = new BindableProperty<float>(0f); // 初始研究进度为0
            
            // 初始化威胁相关数据
            ZombieThreatLevel = new BindableProperty<int>(1); // 初始威胁等级1
            // 假设NextAttackTime单位是游戏内总秒数，3天 = 3 * dayDuration (需从GameManager获取或配置)
            // 此处仅为示例，实际初始化应更精确或由GameManager控制
            NextAttackTime = new BindableProperty<float>(3f * 24f * 60f * 60f);
        }
        
        /// <summary>
        /// 初始化所有作为集合类型的属性（如字典、列表）。
        /// </summary>
        private void InitializeCollections()
        {
            Buildings = new Dictionary<string, BuildingData>();
            Survivors = new Dictionary<string, SurvivorData>();
            WorkerAssignment = new Dictionary<string, List<string>>();
            Technologies = new Dictionary<string, TechData>();
        }
        
        /// <summary>
        /// 执行游戏逻辑层面的初始化，例如创建初始幸存者和设置初始科技状态。
        /// </summary>
        public void InitializeGame()
        {
            InitializeSurvivors();    // 创建初始的幸存者角色
            InitializeWorkAssignment(); // 初始化工作分配记录 (当前为空实现)
            InitializeTechTree();     // 设置初始的科技树状态
        }
        
        /// <summary>
        /// 创建并初始化一组初始幸存者。
        /// </summary>
        private void InitializeSurvivors()
        {
            Survivors.Clear(); // 清空可能存在的旧数据
            // 根据模型中设定的人口值，创建相应数量的初始幸存者
            for (int i = 0; i < Population.Value; i++)
            {
                var survivorId = System.Guid.NewGuid().ToString(); // 生成唯一ID
                var survivor = new SurvivorData // 创建新的幸存者数据实例
                {
                    Id = survivorId,
                    Name = $"幸存者 {i + 1}", // 简单命名
                    Age = UnityEngine.Random.Range(18, 60), // 随机年龄
                    Gender = (Gender)(UnityEngine.Random.Range(0, 2)), // 随机性别 (假设Gender枚举已定义)
                    Health = UnityEngine.Random.Range(80, 101), // 随机健康值 (101上限确保能取到100)
                    Morale = UnityEngine.Random.Range(70, 91),  // 随机士气值
                    Fatigue = UnityEngine.Random.Range(0, 21),  // 随机疲劳度
                    Hunger = UnityEngine.Random.Range(0, 31),   // 随机饥饿度
                    CurrentJob = SurvivorJob.Idle, // 初始状态为空闲
                    AssignedBuildingId = "",       // 未分配到任何建筑
                    WorkEfficiency = 1.0f,        // 基础工作效率
                    Attributes = new List<SurvivorAttributeData>(), // 初始化属性列表
                    LastActionTime = Time.time    // 记录活动时间
                };
                
                // 为每个幸存者随机初始化一组基础属性值
                foreach (SurvivorAttributeType attrType in System.Enum.GetValues(typeof(SurvivorAttributeType)))
                {
                    var attr = new SurvivorAttributeData(attrType, UnityEngine.Random.Range(15, 35)); // 属性值在15-34之间
                    survivor.Attributes.Add(attr);
                }
                
                Survivors[survivorId] = survivor; // 将创建的幸存者添加到字典中
            }
        }
        
        /// <summary>
        /// 初始化工作分配相关的字典结构。
        /// 当前为空实现，实际的工作分配逻辑可能在游戏过程中动态处理。
        /// </summary>
        private void InitializeWorkAssignment()
        {
            // 目前 WorkerAssignment 字典在 InitializeCollections 中已创建。
            // 此方法可以用于预设一些初始分配，或确保与建筑相关的分配列表被创建。
            // 例如：为已存在的建筑（如果有的话）在WorkerAssignment中创建空的工人列表。
        }
        
        /// <summary>
        /// 初始化科技树中部分基础科技的状态。
        /// </summary>
        private void InitializeTechTree()
        {
            Technologies.Clear(); // 清空可能存在的旧数据
            // 定义一组基础科技的ID作为示例
            var basicTechIds = new string[]
            { 
                "agriculture_1", "hunting_1", "fortification_1", 
                "construction_1", "mechanics_1" 
            };
            
            // 为每个基础科技ID创建一个TechData实例并存入字典
            foreach (var techId in basicTechIds)
            {
                Technologies[techId] = new TechData
                {
                    Id = techId,
                    Name = $"基础科技_{techId}", // 示例名称，实际应从配置读取
                    Description = $"这是 {techId} 的详细描述信息。", // 示例描述
                    Level = 1,           // 科技等级
                    Cost = 100,          // 研究成本 (例如：科研点数)
                    Duration = 1.0f,     // 研究时间 (例如：游戏内小时或天)
                    IsResearched = false, // 初始状态为未研究
                    IsResearching = false,// 初始状态为未在研究中
                    ResearchProgress = 0f,// 初始研究进度为0
                    Prerequisites = new List<string>(), // 假设这些基础科技无前置条件
                    UnlockedBy = new List<string>(),    // 解锁此科技的其他科技 (通常用于反向查找)
                    Category = TechCategory.Production, // 示例类别，应根据实际科技设定
                    Tier = TechTier.Tier1               // 示例层级
                };
            }
        }
        
        /// <summary>
        /// 增加指定类型资源的数量。
        /// </summary>
        /// <param name="type">要增加的资源类型。</param>
        /// <param name="amount">要增加的数量。</param>
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