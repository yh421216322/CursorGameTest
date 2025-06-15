using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 科技树分支
    /// </summary>
    public enum TechTree
    {
        Survival,       // 生存科技树
        Defense,        // 防御科技树
        Habitat,        // 居住科技树
        Engineering     // 工程科技树
    }

    /// <summary>
    /// 科技层级
    /// </summary>
    public enum TechTier
    {
        Tier1 = 1,      // 第一层（基础科技）
        Tier2 = 2,      // 第二层（改进科技）
        Tier3 = 3,      // 第三层（高级科技）
        Tier4 = 4       // 第四层（顶级科技）
    }

    /// <summary>
    /// 科技状态
    /// </summary>
    public enum TechState
    {
        Locked,         // 锁定
        Available,      // 可研究
        Researching,    // 研究中
        Completed,      // 已完成
        Failed          // 研究失败
    }

    /// <summary>
    /// 科技配置数据
    /// </summary>
    [Serializable]
    public class TechConfig
    {
        [Header("基础信息")]
        public string Id;
        public string Name;
        public string Description;
        public TechTree Tree;
        public TechTier Tier;
        
        [Header("研究需求")]
        public int ResearchPointsCost;      // 科研点数消耗
        public float ResearchTime;          // 研究时间（小时）
        public List<string> Prerequisites; // 前置科技ID
        public List<ResourceCost> AdditionalCosts; // 额外资源消耗
        
        [Header("研究条件")]
        public int MinResearchLevel;        // 最低研究设施等级
        public List<SurvivorAttribute> RequiredResearchers; // 研究员要求
        public float FailureRate;           // 失败率（0-1）
        
        [Header("解锁内容")]
        public List<string> UnlockedBuildings; // 解锁的建筑ID
        public List<string> UnlockedRecipes;   // 解锁的配方ID
        public List<TechEffect> Effects;       // 科技效果
        
        [Header("视觉效果")]
        public Sprite Icon;
        public Vector2 UIPosition;          // 在科技树UI中的位置
        public Color UIColor = Color.white; // UI颜色
    }

    // /// <summary>
    // /// 科技效果
    // /// </summary>
    // [Serializable]
    // public class TechEffect
    // {
    //     public TechEffectType Type;
    //     public string Target;               // 目标（建筑ID、资源类型等）
    //     public float Value;                 // 效果数值
    //     public string Description;          // 效果描述
    // }

    /// <summary>
    /// 科技效果类型
    /// </summary>
    public enum TechEffectType
    {
        ProductionBonus,        // 生产加成
        EfficiencyBonus,        // 效率加成
        CapacityBonus,          // 容量加成
        DefenseBonus,           // 防御加成
        ResearchSpeedBonus,     // 研究速度加成
        MaintenanceReduction,   // 维护消耗减少
        FailureRateReduction,   // 失败率减少
        StorageBonus,           // 储存加成
        HealthBonus,            // 健康加成
        MoraleBonus            // 士气加成
    }

    // /// <summary>
    // /// 科技实例数据
    // /// </summary>
    // [Serializable]
    // public class TechData
    // {
    //     public string Id;                   // 科技ID
    //     public TechState State;             // 当前状态
    //     public float ResearchProgress;      // 研究进度（0-1）
    //     public float ResearchStartTime;     // 研究开始时间
    //     public List<string> AssignedResearchers; // 分配的研究员ID
    //     public int FailureCount;            // 失败次数
    //     public float CompletionTime;        // 完成时间
    //     
    //     public TechData(string id)
    //     {
    //         Id = id;
    //         State = TechState.Locked;
    //         ResearchProgress = 0f;
    //         ResearchStartTime = 0f;
    //         AssignedResearchers = new List<string>();
    //         FailureCount = 0;
    //         CompletionTime = 0f;
    //     }
    // }

    /// <summary>
    /// 科技树数据
    /// </summary>
    [Serializable]
    public class TechTreeData
    {
        public Dictionary<string, TechData> Technologies;
        public Dictionary<TechTree, List<string>> TreeTechs; // 按科技树分类
        public int TotalResearchPoints;     // 总科研点数
        public int SpentResearchPoints;     // 已消耗科研点数
        public float GlobalResearchBonus;   // 全局研究加成
        
        public TechTreeData()
        {
            Technologies = new Dictionary<string, TechData>();
            TreeTechs = new Dictionary<TechTree, List<string>>();
            TotalResearchPoints = 0;
            SpentResearchPoints = 0;
            GlobalResearchBonus = 1.0f;
            
            // 初始化科技树分类
            foreach (TechTree tree in Enum.GetValues(typeof(TechTree)))
            {
                TreeTechs[tree] = new List<string>();
            }
        }
    }

    /// <summary>
    /// 研究任务
    /// </summary>
    [Serializable]
    public class ResearchTask
    {
        public string TechId;               // 科技ID
        public string BuildingId;           // 研究建筑ID
        public List<string> ResearcherIds;  // 研究员ID列表
        public float StartTime;             // 开始时间
        public float EstimatedEndTime;      // 预计结束时间
        public float EfficiencyMultiplier;  // 效率倍数
        public bool IsPaused;               // 是否暂停
        
        public ResearchTask(string techId, string buildingId)
        {
            TechId = techId;
            BuildingId = buildingId;
            ResearcherIds = new List<string>();
            StartTime = Time.time;
            EstimatedEndTime = 0f;
            EfficiencyMultiplier = 1.0f;
            IsPaused = false;
        }
    }
} 