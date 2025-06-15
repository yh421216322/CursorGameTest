using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 建筑类别
    /// </summary>
    public enum BuildingCategory
    {
        Production,     // 生产型
        Defense,        // 防御型
        Habitat,        // 居住型
        Storage,        // 储存型
        Functional      // 功能型
    }

    /// <summary>
    /// 建筑等级
    /// </summary>
    public enum BuildingLevel
    {
        Level1 = 1,     // 基础设施
        Level2 = 2,     // 改进设施
        Level3 = 3,     // 高级设施
        Level4 = 4,     // 专业设施
        Level5 = 5      // 顶级设施
    }

    /// <summary>
    /// 建筑状态
    /// </summary>
    public enum BuildingState
    {
        Planning,       // 规划中
        UnderConstruction, // 建造中
        Operational,    // 运行中
        Damaged,        // 损坏
        Destroyed,      // 摧毁
        Upgrading       // 升级中
    }

    /// <summary>
    /// 建筑配置数据
    /// </summary>
    [Serializable]
    public class BuildingConfig
    {
        [Header("基础信息")]
        public string ConfigId;
        public string Name;
        public string Description;
        public BuildingCategory Category;
        public BuildingLevel Level;
        
        [Header("建造需求")]
        public List<ResourceCost> BuildCosts;
        public float BuildTime;             // 建造时间（秒）
        public List<string> RequiredTechs; // 需要的科技ID
        
        [Header("运行参数")]
        public int MaxWorkers;              // 最大工人数
        public List<SurvivorAttribute> RequiredAttributes; // 需要的幸存者属性
        public List<ResourceCost> MaintenanceCosts; // 维护消耗（每小时）
        
        [Header("产出配置")]
        public List<ResourceProduction> Productions; // 资源产出
        public List<CraftingRecipe> AvailableRecipes; // 可用配方
        
        [Header("特殊功能")]
        public int StorageCapacity;         // 储存容量（储存型建筑）
        public List<ResourceType> StorageTypes; // 可储存的资源类型
        public int HousingCapacity;         // 住房容量（居住型建筑）
        public float DefensePower;          // 防御力（防御型建筑）
        public float DefenseRange;          // 防御范围
        
        [Header("视觉效果")]
        public GameObject Prefab;           // 预制体
        public Sprite Icon;                 // 图标
        public Vector2Int Size;             // 占用格子大小
    }

    /// <summary>
    /// 幸存者属性需求
    /// </summary>
    [Serializable]
    public class SurvivorAttribute
    {
        public SurvivorAttributeType Type;
        public int MinValue;                // 最低要求
        public float EfficiencyBonus;      // 效率加成（每点属性）
    }

    /// <summary>
    /// 资源产出配置
    /// </summary>
    [Serializable]
    public class ResourceProduction
    {
        public ResourceType Type;
        public float BaseRate;              // 基础产出速率（每小时）
        public float WorkerBonus;           // 每个工人的加成
        public List<ResourceCost> InputCosts; // 输入消耗
        public bool RequiresWorker;         // 是否需要工人
    }

    /// <summary>
    /// 建筑实例数据
    /// </summary>
    [Serializable]
    public class BuildingData
    {
        public string Id;                   // 唯一ID
        public string ConfigId;             // 配置ID
        public Vector3 Position;            // 位置
        public BuildingState State;         // 状态
        public float Health;                // 当前耐久度
        public float MaxHealth;             // 最大耐久度
        public float BuildProgress;         // 建造进度（0-1）
        public List<string> AssignedWorkers; // 分配的工人ID
        public Dictionary<ResourceType, int> StoredResources; // 储存的资源
        public float LastUpdateTime;        // 上次更新时间
        public float LastMaintenanceTime;   // 上次维护时间
        
        public BuildingData()
        {
            Id = Guid.NewGuid().ToString();
            State = BuildingState.Planning;
            Health = 100f;
            MaxHealth = 100f;
            BuildProgress = 0f;
            AssignedWorkers = new List<string>();
            StoredResources = new Dictionary<ResourceType, int>();
            LastUpdateTime = Time.time;
            LastMaintenanceTime = Time.time;
        }
    }

    /// <summary>
    /// 建筑升级配置
    /// </summary>
    [Serializable]
    public class BuildingUpgrade
    {
        public string FromConfigId;         // 源建筑配置ID
        public string ToConfigId;           // 目标建筑配置ID
        public List<ResourceCost> UpgradeCosts; // 升级消耗
        public float UpgradeTime;           // 升级时间
        public List<string> RequiredTechs; // 需要的科技
    }
} 