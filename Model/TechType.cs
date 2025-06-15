// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：TechType.cs
// 作者：未知开发者
// 创建日期：2024年07月15日 // 根据实际情况修改
// 修改日期：2024年07月15日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了与游戏中科技系统相关的枚举类型和数据结构，
//     包括科技树分支、科技层级、科技状态、科技配置、效果、
//     以及科技树的整体数据和研究任务结构。
// ==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 科技树分支枚举。
    /// 用于区分不同领域的科技研究方向。
    /// </summary>
    public enum TechTree
    {
        /// <summary>
        /// 生存科技树：专注于提升角色生存能力、资源获取等方面的科技。
        /// </summary>
        Survival,
        /// <summary>
        /// 防御科技树：专注于提升基地防御、武器装备等方面的科技。
        /// </summary>
        Defense,
        /// <summary>
        /// 居住科技树：专注于改善生活条件、提升舒适度、人口容量等方面的科技。
        /// </summary>
        Habitat,
        /// <summary>
        /// 工程科技树：专注于建筑建造、工业生产、高级材料等方面的科技。
        /// </summary>
        Engineering
    }

    /// <summary>
    /// 科技层级枚举。
    /// 表示科技的先进程度和解锁顺序。
    /// </summary>
    public enum TechTier
    {
        /// <summary>
        /// 第一层科技：通常是游戏初期的基础科技。
        /// </summary>
        Tier1 = 1,
        /// <summary>
        /// 第二层科技：在第一层基础上发展的改进型科技。
        /// </summary>
        Tier2 = 2,
        /// <summary>
        /// 第三层科技：代表更高级和复杂的技术。
        /// </summary>
        Tier3 = 3,
        /// <summary>
        /// 第四层科技：顶尖科技，通常具有强大的效果或解锁关键功能。
        /// </summary>
        Tier4 = 4
    }

    /// <summary>
    /// 科技状态枚举。
    /// 表示单个科技项目当前的研发进展情况。
    /// </summary>
    public enum TechState
    {
        /// <summary>
        /// 锁定状态：科技尚未满足解锁条件或前置科技未完成。
        /// </summary>
        Locked,
        /// <summary>
        /// 可研究状态：科技已解锁，可以投入资源开始研究。
        /// </summary>
        Available,
        /// <summary>
        /// 研究中状态：科技正在进行研究。
        /// </summary>
        Researching,
        /// <summary>
        /// 已完成状态：科技已成功研究完毕。
        /// </summary>
        Completed,
        /// <summary>
        /// 研究失败状态：科技在研究过程中可能由于某些因素（如失败率）导致研究失败。
        /// </summary>
        Failed
    }

    /// <summary>
    /// 科技配置数据类。
    /// 存储单个科技项目的详细配置信息。
    /// </summary>
    [Serializable]
    public class TechConfig
    {
        /// <summary>
        /// 科技的唯一标识符。
        /// </summary>
        [Header("基础信息")]
        public string Id;
        /// <summary>
        /// 科技的显示名称。
        /// </summary>
        public string Name;
        /// <summary>
        /// 科技的详细描述文本。
        /// </summary>
        public string Description;
        /// <summary>
        /// 科技所属的科技树分支 (参考 TechTree 枚举)。
        /// </summary>
        public TechTree Tree;
        /// <summary>
        /// 科技所属的层级 (参考 TechTier 枚举)。
        /// </summary>
        public TechTier Tier;
        
        /// <summary>
        /// 研究此科技所需的科研点数。
        /// </summary>
        [Header("研究需求")]
        public int ResearchPointsCost;      // 科研点数消耗
        /// <summary>
        /// 完成此科技研究所需的基础时间（单位：游戏内小时）。
        /// </summary>
        public float ResearchTime;          // 研究时间（小时）
        /// <summary>
        /// 研究此科技所需满足的前置科技ID列表。
        /// </summary>
        public List<string> Prerequisites; // 前置科技ID
        /// <summary>
        /// 研究此科技除科研点数外，还需消耗的其他资源列表。
        /// </summary>
        public List<ResourceCost> AdditionalCosts; // 额外资源消耗 (ResourceCost结构需在别处定义)
        
        /// <summary>
        /// 开始研究此科技所需的研究设施（如实验室）的最低等级。
        /// </summary>
        [Header("研究条件")]
        public int MinResearchLevel;        // 最低研究设施等级
        /// <summary>
        /// 研究此科技可能需要的特定研究员属性或数量。
        /// </summary>
        public List<SurvivorAttribute> RequiredResearchers; // 研究员要求 (SurvivorAttribute结构需在别处定义)
        /// <summary>
        /// 研究此科技的基础失败概率 (范围 0 到 1，0表示永不失败，1表示永远失败)。
        /// </summary>
        public float FailureRate;           // 失败率（0-1）
        
        /// <summary>
        /// 此科技研究完成后解锁的建筑ID列表。
        /// </summary>
        [Header("解锁内容")]
        public List<string> UnlockedBuildings; // 解锁的建筑ID
        /// <summary>
        /// 此科技研究完成后解锁的制作配方ID列表。
        /// </summary>
        public List<string> UnlockedRecipes;   // 解锁的配方ID
        /// <summary>
        /// 此科技研究完成后产生的具体效果列表。
        /// </summary>
        public List<TechEffect> Effects;       // 科技效果 (TechEffect类定义在此文件下方或别处)
        
        /// <summary>
        /// 科技在UI中显示的图标。
        /// </summary>
        [Header("视觉效果")]
        public Sprite Icon;
        /// <summary>
        /// 科技节点在科技树用户界面中的二维坐标位置。
        /// </summary>
        public Vector2 UIPosition;          // 在科技树UI中的位置
        /// <summary>
        /// 科技节点在UI中显示的特定颜色，默认为白色。
        /// </summary>
        public Color UIColor = Color.white; // UI颜色
    }

    // /// <summary>
    // /// 科技效果 (此类已注释掉，如需启用请取消注释并补充完整注释)
    // /// </summary>
    // [Serializable]
    // public class TechEffect
    // {
    //     public TechEffectType Type;
    //     public string Target;               // 目标（建筑ID、资源类型等）
    //     public float Value;                 // 效果数值
    //     public string Description;          // 效果描述 (此字段已注释掉)
    // }

    /// <summary>
    /// 科技效果类型枚举。
    /// 定义了科技研究完成后可能产生的各种增益或效果。
    /// </summary>
    public enum TechEffectType
    {
        /// <summary>
        /// 生产加成：提升特定资源的产出速率或数量。
        /// </summary>
        ProductionBonus,
        /// <summary>
        /// 效率加成：提高工作效率，如缩短制作时间、减少资源消耗等。
        /// </summary>
        EfficiencyBonus,
        /// <summary>
        /// 容量加成：增加存储容量、人口上限等。
        /// </summary>
        CapacityBonus,
        /// <summary>
        /// 防御加成：增强防御建筑的耐久度或攻击力，或提升单位的防御属性。
        /// </summary>
        DefenseBonus,
        /// <summary>
        /// 研究速度加成：加快科技研究的速度。
        /// </summary>
        ResearchSpeedBonus,
        /// <summary>
        /// 维护消耗减少：降低建筑或单位的日常维护资源需求。
        /// </summary>
        MaintenanceReduction,
        /// <summary>
        /// 失败率减少：降低某些操作（如研究、制造）的失败概率。
        /// </summary>
        FailureRateReduction,
        /// <summary>
        /// 存储加成：特指增加物品或资源的存储上限。
        /// </summary>
        StorageBonus,
        /// <summary>
        /// 健康加成：提升单位的生命值上限或恢复速度，改善医疗效果。
        /// </summary>
        HealthBonus,
        /// <summary>
        /// 士气加成：提升单位的士气值，可能影响其工作效率或战斗表现。
        /// </summary>
        MoraleBonus
    }

    // /// <summary>
    // /// 科技实例数据 (此类已注释掉，如需启用请取消注释并补充完整注释)
    // /// 代表游戏中单个科技的动态数据，如当前状态、研究进度等。
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
    //     /// <summary>
    //     /// TechData的构造函数。(此构造函数已注释掉)
    //     /// </summary>
    //     /// <param name="id">科技ID。</param>
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
    /// 科技树数据类。
    /// 存储整个科技系统的状态，包括所有科技的实例数据、科研点数等。
    /// </summary>
    [Serializable]
    public class TechTreeData
    {
        /// <summary>
        /// 存储所有科技实例数据的字典，键为科技ID。
        /// (注意：TechData类当前被注释掉了，这里可能引用的是其他地方定义的TechData或应调整)
        /// </summary>
        public Dictionary<string, TechData> Technologies;
        /// <summary>
        /// 按科技树分支对科技ID进行分类存储的字典。
        /// </summary>
        public Dictionary<TechTree, List<string>> TreeTechs; // 按科技树分类
        /// <summary>
        /// 玩家当前拥有的总科研点数。
        /// </summary>
        public int TotalResearchPoints;     // 总科研点数
        /// <summary>
        /// 玩家已经消耗在研究上的科研点数。
        /// </summary>
        public int SpentResearchPoints;     // 已消耗科研点数
        /// <summary>
        /// 全局研究加成百分比（例如1.0代表100%，1.1代表110%）。
        /// </summary>
        public float GlobalResearchBonus;   // 全局研究加成
        
        /// <summary>
        /// TechTreeData的构造函数。
        /// 初始化科技数据存储结构和默认值。
        /// </summary>
        public TechTreeData()
        {
            Technologies = new Dictionary<string, TechData>();
            TreeTechs = new Dictionary<TechTree, List<string>>();
            TotalResearchPoints = 0;
            SpentResearchPoints = 0;
            GlobalResearchBonus = 1.0f; // 默认全局研究加成为100%
            
            // 初始化科技树分支列表
            foreach (TechTree tree in Enum.GetValues(typeof(TechTree)))
            {
                TreeTechs[tree] = new List<string>();
            }
        }
    }

    /// <summary>
    /// 研究任务类。
    /// 表示一个具体的科技研究项目，包含其相关信息和状态。
    /// </summary>
    [Serializable]
    public class ResearchTask
    {
        /// <summary>
        /// 正在研究的科技的唯一ID。
        /// </summary>
        public string TechId;               // 科技ID
        /// <summary>
        /// 执行此研究任务的建筑（如实验室）的唯一ID。
        /// </summary>
        public string BuildingId;           // 研究建筑ID
        /// <summary>
        /// 分配给此研究任务的研究员ID列表。
        /// </summary>
        public List<string> ResearcherIds;  // 研究员ID列表
        /// <summary>
        /// 此研究任务开始时的游戏时间戳。
        /// </summary>
        public float StartTime;             // 开始时间
        /// <summary>
        /// 预计完成此研究任务的游戏时间戳。
        /// </summary>
        public float EstimatedEndTime;      // 预计结束时间
        /// <summary>
        /// 此研究任务的效率乘数（可能受研究员技能、建筑等级等影响）。
        /// </summary>
        public float EfficiencyMultiplier;  // 效率倍数
        /// <summary>
        /// 标记此研究任务当前是否已暂停。
        /// </summary>
        public bool IsPaused;               // 是否暂停
        
        /// <summary>
        /// ResearchTask的构造函数。
        /// </summary>
        /// <param name="techId">要研究的科技ID。</param>
        /// <param name="buildingId">执行研究的建筑ID。</param>
        public ResearchTask(string techId, string buildingId)
        {
            TechId = techId;
            BuildingId = buildingId;
            ResearcherIds = new List<string>();
            StartTime = Time.time; // 通常在实际开始时设置，这里可能只是初始化
            EstimatedEndTime = 0f;
            EfficiencyMultiplier = 1.0f; // 默认效率为100%
            IsPaused = false;
        }
    }
}