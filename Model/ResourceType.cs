using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 资源类型枚举
    /// </summary>
    public enum ResourceType
    {
        // 基础物资（原材料）
        Food,           // 食物
        Materials,      // 材料
        Water,          // 水
        Scrap,          // 废料
        OrganicMatter,  // 有机物
        
        // 成品物资（制造品）
        Ammunition,     // 弹药
        MedicalSupplies,// 医疗用品
        Tools,          // 工具
        Fuel,           // 燃料
        Electronics,    // 电子元件
        
        // 高级资源
        Energy,         // 能源
        ResearchPoints, // 科研点数
        RareMetals      // 稀有金属
    }

    /// <summary>
    /// 资源类别
    /// </summary>
    public enum ResourceCategory
    {
        BasicMaterial,  // 基础物资
        Manufactured,   // 成品物资
        Advanced        // 高级资源
    }

    /// <summary>
    /// 资源配置数据
    /// </summary>
    [Serializable]
    public class ResourceConfig
    {
        public ResourceType Type;
        public string Name;
        public string Description;
        public ResourceCategory Category;
        public bool CanDecay;              // 是否会腐坏
        public float DecayRate;            // 腐坏速率（每天）
        public int MaxStackSize;           // 最大堆叠数量
        public bool RequiresSpecialStorage; // 是否需要特殊储存
        public string StorageRequirement;   // 储存要求描述
        public Sprite Icon;                // 图标
    }

    /// <summary>
    /// 制造配方
    /// </summary>
    [Serializable]
    public class CraftingRecipe
    {
        public string Id;
        public string Name;
        public string Description;
        public ResourceType OutputType;
        public int OutputAmount;
        public List<ResourceCost> InputCosts;
        public float CraftingTime;         // 制造时间（小时）
        public int RequiredTechLevel;      // 需要的科技等级
        public List<string> RequiredBuildings; // 可制造的建筑类型
    }

    /// <summary>
    /// 资源消耗
    /// </summary>
    [Serializable]
    public class ResourceCost
    {
        public ResourceType Type;
        public int Amount;//  数量
    }

    /// <summary>
    /// 资源数据
    /// </summary>
    [Serializable]
    public class ResourceData
    {
        public ResourceType Type;
        public int Amount;
        public float LastUpdateTime;       // 上次更新时间
        public float DecayAccumulator;     // 腐坏累积器
        
        public ResourceData(ResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
            LastUpdateTime = Time.time;
            DecayAccumulator = 0f;
        }
    }
} 