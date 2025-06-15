// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：BuildingType.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了与游戏中“建筑”相关的各种数据结构和枚举类型。
//     包括建筑的类别、等级、状态，以及具体的建筑配置信息（如建造成本、产出、
//     特殊功能等）、建筑实例在游戏中的动态数据，以及建筑升级的相关配置。
//     这些定义是游戏建筑系统的核心数据模型。
// ==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine; // 用于Vector3, GameObject, Sprite, Vector2Int等Unity特定类型

namespace SurvivalGame.Model
{
    /// <summary>
    /// 建筑类别枚举。
    /// 用于区分不同功能的建筑。
    /// </summary>
    public enum BuildingCategory
    {
        Production,     // 生产型：例如农田、工坊，主要用于产出资源或物品。
        Defense,        // 防御型：例如炮塔、围墙，用于抵御敌人。
        Habitat,        // 居住型：例如住所、宿舍，提供人口上限或影响士气。
        Storage,        // 储存型：例如仓库，用于存放资源。
        Functional      // 功能型：例如研究室、医院，提供特殊功能或服务。
    }

    /// <summary>
    /// 建筑等级枚举。
    /// 代表建筑可以升级到的不同阶段，通常等级越高功能越强或效率越高。
    /// </summary>
    public enum BuildingLevel
    {
        Level1 = 1,     // 1级：通常为基础或初始等级的设施。
        Level2 = 2,     // 2级：改进型设施。
        Level3 = 3,     // 3级：高级设施。
        Level4 = 4,     // 4级：专业化或顶尖设施。
        Level5 = 5      // 5级：最高等级或终极设施。
    }

    /// <summary>
    /// 建筑当前状态枚举。
    /// 描述建筑在游戏中的不同生命周期阶段。
    /// </summary>
    public enum BuildingState
    {
        Planning,          // 规划中：建筑已被规划但尚未开始建造（例如，仅为蓝图状态）。
        UnderConstruction, // 建造中：建筑正在被建造，尚未完成。
        Operational,       // 运行中：建筑已建成并正常运作。
        Damaged,           // 损坏状态：建筑受到攻击或损耗，功能可能受限，需要修理。
        Destroyed,         // 已摧毁：建筑已被完全破坏，无法运作。
        Upgrading          // 升级中：建筑正在进行升级到更高级别的过程。
    }

    /// <summary>
    /// 建筑的静态配置数据类。
    /// 存储一种特定类型建筑的所有固定属性和参数，这些数据通常在游戏设计时确定，并在运行时加载。
    /// </summary>
    [Serializable] // 标记为可序列化，以便能在Unity Inspector中编辑或保存到文件
    public class BuildingConfig
    {
        [Header("基础信息")] // Inspector中显示的分类标签
        public string ConfigId;           // 建筑配置的唯一ID，例如 "farm_level1"
        public string Name;               // 建筑的显示名称，例如 "农场"
        public string Description;        // 建筑的描述文本
        public BuildingCategory Category; // 建筑所属的类别 (参考BuildingCategory枚举)
        public BuildingLevel Level;       // 建筑的等级 (参考BuildingLevel枚举)
        
        [Header("建造需求")]
        public List<ResourceCost> BuildCosts; // 建造此建筑所需的资源列表 (参考ResourceCost类)
        public float BuildTime;               // 建造所需时间（例如，秒）
        public List<string> RequiredTechs;   // 解锁或建造此建筑所需的前置科技ID列表
        
        [Header("运行参数")]
        public int MaxWorkers;                // 此建筑最大可容纳的工人数
        public List<SurvivorAttribute> RequiredAttributes; // 对在此建筑工作的幸存者可能有的属性或技能要求
        public List<ResourceCost> MaintenanceCosts; // 建筑维持运作所需的周期性资源消耗 (例如，每小时消耗)
        
        [Header("产出与功能配置")]
        public List<ResourceProduction> Productions; // 如果是生产型建筑，其资源产出配置列表
        public List<CraftingRecipe> AvailableRecipes; // 如果是制造型建筑（如工坊），其可用的制造配方列表 (或配方ID列表)
        
        [Header("特殊功能参数")]
        public int StorageCapacity;           // 如果是储存型建筑，其提供的总储存容量
        public List<ResourceType> StorageTypes; // 可储存的特定资源类型列表 (如果为空，可能表示可储存所有类型)
        public int HousingCapacity;           // 如果是居住型建筑，其提供的人口或住房容量
        public float DefensePower;            // 如果是防御型建筑，其攻击力或防御指数
        public float DefenseRange;            // 防御型建筑的攻击范围或有效范围
        
        [Header("视觉与布局")]
        public GameObject Prefab;             // 此建筑在场景中对应的预制件 (GameObject)
        public Sprite Icon;                   // 此建筑在UI中显示的图标 (Sprite)
        public Vector2Int Size;               // 建筑在游戏地图上占用的格子大小 (例如 2x2, 3x3)
    }

    /// <summary>
    /// 定义在建筑工作的幸存者所需的属性（或技能）及其效果。
    /// </summary>
    [Serializable]
    public class SurvivorAttribute
    {
        public SurvivorAttributeType Type; // 所需的幸存者属性类型 (参考SurvivorAttributeType枚举)
        public int MinValue;               // 幸存者拥有此属性的最低值要求
        public float EfficiencyBonus;      // 每点属性值对应的工作效率加成 (例如，0.05代表每点属性提高5%效率)
    }

    /// <summary>
    /// 定义建筑的资源产出配置。
    /// </summary>
    [Serializable]
    public class ResourceProduction
    {
        public ResourceType Type;           // 产出的资源类型 (参考ResourceType枚举)
        public float BaseRate;              // 基础产出速率（例如，单位资源/每小时），无工人或工人属性加成时的速率
        public float WorkerBonus;           // 每个分配到此产出任务的工人提供的额外产出加成，或工人属性对产率的乘数因子
        public List<ResourceCost> InputCosts; // 产出此资源是否需要消耗其他输入资源 (例如，面包需要面粉)
        public bool RequiresWorker;         // 此项产出是否必须有工人才能进行
    }

    /// <summary>
    /// 建筑实例在游戏中的动态数据。
    /// 代表一个已放置在场景中或正在建造的建筑。
    /// </summary>
    [Serializable]
    public class BuildingData
    {
        public string Id;                   // 建筑实例的唯一ID (通常由GUID生成)
        public string ConfigId;             // 对应的建筑配置ID (BuildingConfig.ConfigId)
        public Vector3 Position;            // 建筑在世界空间中的位置
        public BuildingState State;         // 建筑当前的运行状态 (参考BuildingState枚举)
        public float Health;                // 建筑当前的耐久度或生命值
        public float MaxHealth;             // 建筑的最大耐久度或生命值
        public float BuildProgress;         // 建造或升级的进度 (范围通常为 0.0 到 1.0)
        public List<string> AssignedWorkers; // 当前分配到此建筑工作的幸存者ID列表
        public Dictionary<ResourceType, int> StoredResources; // 如果是储存型建筑，其实际储存的各项资源数量
        public float LastUpdateTime;        // 上次更新此建筑数据的时间戳 (例如 Time.time)
        public float LastMaintenanceTime;   // 上次执行维护操作的时间戳
        
        /// <summary>
        /// BuildingData的构造函数。
        /// 初始化建筑实例的默认值。
        /// </summary>
        public BuildingData()
        {
            Id = Guid.NewGuid().ToString(); // 生成一个全局唯一的ID
            State = BuildingState.Planning; // 初始状态为规划中
            Health = 100f;                  // 初始健康值
            MaxHealth = 100f;                 // 初始最大健康值
            BuildProgress = 0f;             // 初始建造进度为0
            AssignedWorkers = new List<string>(); // 初始化空的工人列表
            StoredResources = new Dictionary<ResourceType, int>(); // 初始化空的存储资源字典
            LastUpdateTime = Time.time;       // 记录创建时的时间
            LastMaintenanceTime = Time.time;  // 同上
        }
    }

    /// <summary>
    /// 建筑升级的配置数据。
    /// 定义了从一个建筑配置升级到另一个建筑配置所需的条件和时间。
    /// </summary>
    [Serializable]
    public class BuildingUpgrade
    {
        public string FromConfigId;         // 源建筑的配置ID (要升级的建筑类型)
        public string ToConfigId;           // 目标建筑的配置ID (升级完成后的建筑类型)
        public List<ResourceCost> UpgradeCosts; // 升级所需的资源列表
        public float UpgradeTime;           // 升级所需的时间（例如，秒）
        public List<string> RequiredTechs; // 升级可能需要的前置科技ID列表
    }
}