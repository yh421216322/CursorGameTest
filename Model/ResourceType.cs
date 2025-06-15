// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ResourceType.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了与游戏中资源、物品制造相关的核心数据结构和枚举类型。
//     包括各种资源的类型 (ResourceType)、资源的分类 (ResourceCategory)、
//     单个资源的配置信息 (ResourceConfig)、制造配方 (CraftingRecipe)、
//     资源成本表示 (ResourceCost) 以及游戏中实际资源实例的数据 (ResourceData)。
//     这些是构建游戏经济和制造系统的基础。
// ==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine; // 用于Sprite等Unity特定类型

namespace SurvivalGame.Model
{
    /// <summary>
    /// 游戏内所有可用资源的类型枚举。
    /// 资源被大致分为基础物资、成品物资和高级资源。
    /// </summary>
    public enum ResourceType
    {
        // --- 基础物资 (原材料) ---
        /// <summary>
        /// 食物：用于维持幸存者生存。
        /// </summary>
        Food,           // 食物
        /// <summary>
        /// 建筑材料：用于建造和升级建筑。
        /// </summary>
        Materials,      // 材料
        /// <summary>
        /// 水：生命必需品，也可能用于某些生产过程。
        /// </summary>
        Water,          // 水
        /// <summary>
        /// 废料：可回收或用于制造低级物品的常见材料。
        /// </summary>
        Scrap,          // 废料
        /// <summary>
        /// 有机物：例如木材、植物纤维等，可用于燃料、建筑或特殊制造。
        /// </summary>
        OrganicMatter,  // 有机物
        
        // --- 成品物资 (通过制造获得) ---
        /// <summary>
        /// 弹药：用于防御塔或幸存者武器。
        /// </summary>
        Ammunition,     // 弹药
        /// <summary>
        /// 医疗用品：用于治疗伤病，如绷带、药品。
        /// </summary>
        MedicalSupplies,// 医疗用品
        /// <summary>
        /// 工具：可提高特定工作的效率或解锁某些操作。
        /// </summary>
        Tools,          // 工具
        /// <summary>
        /// 燃料：用于发电机、车辆或其他消耗能源的设备。
        /// </summary>
        Fuel,           // 燃料
        /// <summary>
        /// 电子元件：用于制造和升级高级设备或建筑。
        /// </summary>
        Electronics,    // 电子元件
        
        // --- 高级或特殊资源 ---
        /// <summary>
        /// 能源/电力：一种特殊资源，可能由发电机产生并由设施消耗。
        /// </summary>
        Energy,         // 能源
        /// <summary>
        /// 科研点数：用于解锁科技。
        /// </summary>
        ResearchPoints, // 科研点数
        /// <summary>
        /// 稀有金属：用于制造高级物品或特定科技研发。
        /// </summary>
        RareMetals      // 稀有金属
    }

    /// <summary>
    /// 资源的分类枚举。
    /// 用于对资源进行大致归类，方便管理和筛选。
    /// </summary>
    public enum ResourceCategory
    {
        BasicMaterial,  // 基础物资：通常指原材料，可直接采集或低级生产。
        Manufactured,   // 制成品：通过制造系统由原材料加工而成的物品。
        Advanced        // 高级资源：获取难度较高，通常用于后期或特殊用途的资源。
    }

    /// <summary>
    /// 单个资源的静态配置数据类。
    /// 存储特定类型资源的所有固定属性。
    /// </summary>
    [Serializable] // 标记为可序列化，以便能在Unity Inspector中编辑或用于数据存储
    public class ResourceConfig
    {
        /// <summary>
        /// 资源的类型 (参考ResourceType枚举)。
        /// </summary>
        public ResourceType Type;
        /// <summary>
        /// 资源的显示名称 (例如："木材", "铁矿石")。
        /// </summary>
        public string Name;
        /// <summary>
        /// 资源的描述文本。
        /// </summary>
        public string Description;
        /// <summary>
        /// 资源所属的类别 (参考ResourceCategory枚举)。
        /// </summary>
        public ResourceCategory Category;
        /// <summary>
        /// 标记此资源是否会随着时间腐坏或变质。
        /// </summary>
        public bool CanDecay;              // 是否会腐坏
        /// <summary>
        /// 如果资源会腐坏 (CanDecay=true)，此为其腐坏速率 (例如，每日损失的百分比或数量)。
        /// </summary>
        public float DecayRate;            // 腐坏速率（例如：每日损失百分比）
        /// <summary>
        /// 此资源在单个库存堆叠中的最大数量。
        /// </summary>
        public int MaxStackSize;           // 最大堆叠数量
        /// <summary>
        /// 标记此资源是否需要特殊的储存设施 (例如，易腐食物需要冰箱)。
        /// </summary>
        public bool RequiresSpecialStorage; // 是否需要特殊储存条件
        /// <summary>
        /// 如果需要特殊储存 (RequiresSpecialStorage=true)，此为对储存条件的文字描述。
        /// </summary>
        public string StorageRequirement;   // 特殊储存要求的描述文本
        /// <summary>
        /// 此资源在UI中显示的图标。
        /// </summary>
        public Sprite Icon;                // 图标
    }

    /// <summary>
    /// 物品或资源的制造配方数据类。
    /// 定义了制造一个物品所需的输入、时间、条件等。
    /// </summary>
    [Serializable]
    public class CraftingRecipe
    {
        /// <summary>
        /// 配方的唯一ID。
        /// </summary>
        public string Id;
        /// <summary>
        /// 配方的显示名称 (例如："制造铁斧")。
        /// </summary>
        public string Name;
        /// <summary>
        /// 配方的描述文本。
        /// </summary>
        public string Description;
        /// <summary>
        /// 此配方产出的物品资源类型。
        /// </summary>
        public ResourceType OutputType;
        /// <summary>
        /// 一次制造成功产出的物品数量。
        /// </summary>
        public int OutputAmount;
        /// <summary>
        /// 制造此物品所需的输入资源及其数量列表。
        /// </summary>
        public List<ResourceCost> InputCosts;
        /// <summary>
        /// 完成一次制造所需的时间 (例如，游戏内小时或秒)。
        /// </summary>
        public float CraftingTime;         // 制造时间（例如：小时）
        /// <summary>
        /// 制造此物品所需的最低科技等级或特定科技ID。
        /// </summary>
        public int RequiredTechLevel;      // 需要的科技等级 (或者可以是一个科技ID列表)
        /// <summary>
        /// 可以执行此制造配方的建筑类型ID列表 (例如："workshop_level1", "forge")。
        /// </summary>
        public List<string> RequiredBuildings; // 可制造此配方的建筑类型ID列表
    }

    /// <summary>
    /// 表示特定类型和数量的资源成本。
    /// 用于定义建造、制造、维护等操作的资源消耗。
    /// </summary>
    [Serializable]
    public class ResourceCost
    {
        /// <summary>
        /// 消耗的资源类型。
        /// </summary>
        public ResourceType Type;
        /// <summary>
        /// 消耗的数量。
        /// </summary>
        public int Amount;
    }

    /// <summary>
    /// 游戏中实际持有的资源实例数据。
    /// 表示特定类型资源在某个库存或全局中的当前数量及状态。
    /// </summary>
    [Serializable]
    public class ResourceData
    {
        /// <summary>
        /// 资源的类型。
        /// </summary>
        public ResourceType Type;
        /// <summary>
        /// 当前持有的该资源数量。
        /// </summary>
        public int Amount;
        /// <summary>
        /// 上次更新此资源数据的时间戳 (例如 Time.time)，可用于计算腐坏等。
        /// </summary>
        public float LastUpdateTime;       // 上次更新时间
        /// <summary>
        /// 腐坏累积值。用于更精确地计算随时间发生的腐坏，而不是简单地按整数天扣除。
        /// </summary>
        public float DecayAccumulator;     // 腐坏累积器 (例如，累积小数部分的腐坏直到达到1)
        
        /// <summary>
        /// ResourceData的构造函数。
        /// </summary>
        /// <param name="type">资源类型。</param>
        /// <param name="amount">初始数量。</param>
        public ResourceData(ResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
            LastUpdateTime = Time.time; // 初始化时记录当前时间
            DecayAccumulator = 0f;      // 初始化腐坏累积器
        }
    }
}