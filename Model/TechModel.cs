// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：TechModel.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了与游戏内科技研发系统相关的核心数据模型、枚举和事件。
//     包括科技的基础数据 (TechData)、科技树节点信息 (TechNode)、科技效果 (TechEffect)、
//     研究设施、研究员、研究队列、全局研究状态以及相关的枚举类型（如科技类别、
//     状态、研究员专业等）和事件结构体。
// ==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine; // 用于 Vector2 等 Unity 特定类型
using QFramework;  // QFramework 框架 (当前在此文件中未直接使用其特性)

namespace SurvivalGame.Model
{
    /// <summary>
    /// 科技的基础配置与动态数据模型。
    /// 存储单个科技项目的各种属性，如ID、名称、描述、等级、研究成本、
    /// 研究时长、当前研究状态和进度、前置条件等。
    /// </summary>
    [System.Serializable] // 标记为可序列化，以便能在Unity Inspector中显示或保存到文件
    public class TechData
    {
        /// <summary>
        /// 科技的唯一标识符。
        /// </summary>
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
        /// 科技的等级（如果科技可以分多级研究）。
        /// </summary>
        public int Level;
        /// <summary>
        /// 研究此科技所需的成本（例如：科研点数）。
        /// </summary>
        public int Cost;
        /// <summary>
        /// 完成此科技研究所需的基础时长（例如：游戏内小时或秒）。
        /// </summary>
        public float Duration;
        /// <summary>
        /// 标记此科技是否已经被研究完成。
        /// </summary>
        public bool IsResearched;
        /// <summary>
        /// 标记此科技当前是否正在研究中。
        /// </summary>
        public bool IsResearching;
        /// <summary>
        /// 当前的研究进度（通常为0到1之间，1表示完成）。
        /// </summary>
        public float ResearchProgress;
        /// <summary>
        /// 研究此科技所需满足的前置科技ID列表。
        /// </summary>
        public List<string> Prerequisites;
        /// <summary>
        /// (可能指) 此科技解锁后，会作为哪些其他科技的前置条件（反向链接）。
        /// </summary>
        public List<string> UnlockedBy; // 或理解为：此科技解锁了哪些后续科技
        /// <summary>
        /// 科技所属的类别 (参考TechCategory枚举)。
        /// </summary>
        public TechCategory Category;
        /// <summary>
        /// 科技的层级或阶段 (参考TechTier枚举，注意TechType枚举也存在，可能存在映射关系或其一为旧版)。
        /// </summary>
        public TechTier Tier; // TechTier枚举定义在此文件中缺失，假设它在别处定义或应为TechType
        /// <summary>
        /// 开始研究此科技时的游戏时间戳 (Time.time)。
        /// </summary>
        public float ResearchStartTime;
        /// <summary>
        /// 当前研究此科技的速度或效率乘数。
        /// </summary>
        public float ResearchSpeed;
        /// <summary>
        /// 标记此科技的研究是否已暂停。
        /// </summary>
        public bool IsPaused;
        
        /// <summary>
        /// TechData的构造函数。
        /// 初始化列表和默认研究速度。
        /// </summary>
        public TechData()
        {
            Prerequisites = new List<string>();
            UnlockedBy = new List<string>();
            ResearchSpeed = 1f; // 默认研究速度为100%
        }
    }

    /// <summary>
    /// 科技节点数据结构，在科技系统中使用的核心数据表示。
    /// 比 TechData 更详细，包含了如UI位置、效果、具体资源需求等信息。
    /// </summary>
    [System.Serializable]
    public class TechNode
    {
        /// <summary>
        /// 科技的唯一ID。
        /// </summary>
        public string Id;                       // 科技ID
        /// <summary>
        /// 科技的显示名称。
        /// </summary>
        public string Name;                     // 科技名称
        /// <summary>
        /// 科技的详细描述。
        /// </summary>
        public string Description;              // 科技描述
        /// <summary>
        /// 科技所属的类别 (参考TechCategory枚举)。
        /// </summary>
        public TechCategory Category;           // 科技分类
        /// <summary>
        /// 科技的类型或层级 (参考TechType枚举，对应旧的TechTier概念)。
        /// </summary>
        public TechType Type;                   // 科技类型 (对应原Tier)
        /// <summary>
        /// 研究此科技所需的科研点数成本。
        /// </summary>
        public int ResearchCost;               // 研究点数成本
        /// <summary>
        /// 研究此科技所需的基础时间（例如：游戏内小时）。
        /// </summary>
        public float ResearchTime;             // 研究时间(小时)
        /// <summary>
        /// (新增需求) 研究此科技所需的建筑材料数量。
        /// </summary>
        public int MaterialRequirement;        // 材料需求
        /// <summary>
        /// (新增需求) 研究此科技所需的弹药数量。
        /// </summary>
        public int AmmoRequirement;            // 弹药需求
        /// <summary>
        /// (新增需求) 研究此科技所需的电力或能源。
        /// </summary>
        public int PowerRequirement;           // 电力需求
        /// <summary>
        /// (新增需求) 研究此科技所需的人口前置（或用于解锁人口上限等）。
        /// </summary>
        public int PopulationRequirement;      // 人口需求
        /// <summary>
        /// 研究此科技所需满足的前置科技ID列表。
        /// </summary>
        public List<string> Prerequisites;     // 前置科技ID列表
        /// <summary>
        /// 此科技研究完成后产生的效果。
        /// 键为效果类型或目标属性的标识符，值为效果的数值（例如加成百分比或固定值）。
        /// </summary>
        public Dictionary<string, float> Effects; // 科技效果 (效果ID -> 效果值)
        /// <summary>
        /// 科技当前的研发状态 (参考TechStatus枚举)。
        /// </summary>
        public TechStatus Status;              // 科技状态 (Locked, Available, Researching, Researched)
        /// <summary>
        /// 当前的研究进度 (范围0到1)。
        /// </summary>
        public float ResearchProgress;         // 研究进度(0-1)
        /// <summary>
        /// 开始研究此科技时的游戏时间戳。
        /// </summary>
        public float ResearchStartTime;        // 研究开始时间
        /// <summary>
        /// 此科技节点在科技树UI中的二维位置坐标。
        /// </summary>
        public Vector2 Position;               // 在科技树中的位置 (用于UI布局)
        /// <summary>
        /// 标记此科技节点在科技树UI中当前是否可见。
        /// </summary>
        public bool IsVisible;                 // 是否可见 (用于UI显示控制)
        
        /// <summary>
        /// TechNode的默认构造函数。
        /// 初始化列表、效果字典，并设置默认的可见性和锁定状态。
        /// </summary>
        public TechNode()
        {
            Prerequisites = new List<string>();
            Effects = new Dictionary<string, float>();
            IsVisible = true; // 默认可见
            Status = TechStatus.Locked; // 默认锁定状态
        }
        
        /// <summary>
        /// TechNode的参数化构造函数。
        /// </summary>
        public TechNode(string id, string name, string description, TechCategory category, TechType type, 
                       int cost, float duration, List<string> prerequisites = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Category = category;
            Type = type;
            ResearchCost = cost;
            ResearchTime = duration;
            Prerequisites = prerequisites ?? new List<string>(); // 如果传入null则初始化为空列表
            Effects = new Dictionary<string, float>();
            IsVisible = true;
            Status = TechStatus.Locked;
        }
        
        /// <summary>
        /// 检查此科技的所有前置条件是否都已满足（即已研究完成）。
        /// </summary>
        /// <param name="allTechs">包含所有科技节点信息的字典，用于查找前置科技的状态。</param>
        /// <returns>如果所有前置条件均满足则返回true，否则返回false。</returns>
        public bool ArePrerequisitesMet(Dictionary<string, TechNode> allTechs)
        {
            if (Prerequisites == null || Prerequisites.Count == 0) return true; // 没有前置条件，则视为满足

            foreach (var prereqId in Prerequisites)
            {
                // 如果字典中不存在该前置科技ID，或者该前置科技尚未研究完成，则条件不满足
                if (!allTechs.ContainsKey(prereqId) || allTechs[prereqId].Status != TechStatus.Researched)
                {
                    return false;
                }
            }
            return true; // 所有前置条件均已研究完成
        }
        
        /// <summary>
        /// 获取当前科技状态对应的本地化显示文本。
        /// </summary>
        /// <returns>表示科技状态的字符串。</returns>
        public string GetStatusText()
        {
            switch (Status)
            {
                case TechStatus.Researched:  return "已完成";
                case TechStatus.Researching: return $"研究中 ({ResearchProgress:P0})"; // P0表示百分比，0位小数
                case TechStatus.Available:   return "可研究";
                case TechStatus.Locked:
                default:                     return "已锁定";
            }
        }
        
        /// <summary>
        /// 获取此科技效果的简要描述字符串。
        /// </summary>
        /// <returns>格式化的效果描述字符串，或"无特殊效果"。</returns>
        public string GetEffectsSummary()
        {
            if (Effects == null || Effects.Count == 0)
                return "无特殊效果";
                
            var summaryParts = new List<string>();
            // 遍历效果字典，格式化为 "效果名: +效果值" 的形式
            foreach (var effect in Effects)
            {
                summaryParts.Add($"{effect.Key}: {(effect.Value >= 0 ? "+" : "")}{effect.Value}"); // 根据正负添加+/-号
            }
            
            return summaryParts.Count > 0 ? string.Join(", ", summaryParts) : "无特殊效果";
        }
    }

    /// <summary>
    /// 表示科技研究完成后产生的具体效果。
    /// </summary>
    [System.Serializable]
    public class TechEffect
    {
        /// <summary>
        /// 产生此效果的科技的ID。
        /// </summary>
        public string TechId;
        /// <summary>
        /// 效果的类型或名称 (例如："FarmingOutput", "TowerDamageBonus")。
        /// </summary>
        public string EffectType;
        /// <summary>
        /// 效果的数值 (例如：增加的百分比、固定值等)。
        /// </summary>
        public float Value;
        /// <summary>
        /// 效果作用的目标 (可选，例如：特定建筑类型ID、所有单位等)。
        /// </summary>
        public string Target; // 例如："Farm", "AllTowers", "SurvivorStats"
        /// <summary>
        /// 此效果当前是否已激活并生效。
        /// </summary>
        public bool IsActive;
        
        /// <summary>
        /// TechEffect的构造函数。
        /// </summary>
        public TechEffect(string techId, string effectType, float value, string target = "")
        {
            TechId = techId;
            EffectType = effectType;
            Value = value;
            Target = target;
            IsActive = false; // 效果默认不激活，需由系统在科技完成后设置为true
        }
    }

    /// <summary>
    /// 科技树中的一个节点，可能用于UI展示或逻辑连接。
    /// (此结构与TechNode有一定重叠，需确认其具体用途和是否为冗余设计)
    /// </summary>
    [System.Serializable]
    public class TechTreeNode
    {
        /// <summary>
        /// 此节点关联的详细科技数据 (TechData实例)。
        /// </summary>
        public TechData TechData; // 包含具体的科技信息
        /// <summary>
        /// 此节点在科技树UI布局中的二维坐标。
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// 此节点的子节点（后续科技）的ID列表。
        /// </summary>
        public List<string> ChildrenIds;
        /// <summary>
        /// 此节点的父节点（前置科技）的ID列表。
        /// </summary>
        public List<string> ParentIds;
        /// <summary>
        /// 此节点在UI中是否可见。
        /// </summary>
        public bool IsVisible;
        /// <summary>
        /// 此节点代表的科技是否已解锁。
        /// </summary>
        public bool IsUnlocked; // 可能与TechData.IsResearched或TechNode.Status重复，需整合
        
        /// <summary>
        /// TechTreeNode的构造函数。
        /// </summary>
        public TechTreeNode()
        {
            ChildrenIds = new List<string>();
            ParentIds = new List<string>();
            IsVisible = true; // 默认可见
        }
    }

    /// <summary>
    /// 研究设施（如研究室、实验室）的数据模型。
    /// </summary>
    [System.Serializable]
    public class ResearchFacilityData
    {
        /// <summary>
        /// 研究设施的唯一ID (通常是建筑实例ID)。
        /// </summary>
        public int Id;
        /// <summary>
        /// 研究设施的名称。
        /// </summary>
        public string Name;
        /// <summary>
        /// 研究设施的等级。
        /// </summary>
        public int Level;
        /// <summary>
        /// 研究设施的基础研究效率乘数。
        /// </summary>
        public float Efficiency; // 基础效率，例如 1.0, 1.2
        /// <summary>
        /// 研究设施当前是否可运作。
        /// </summary>
        public bool IsOperational;
        /// <summary>
        /// 研究设施在世界中的位置。
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// 此设施支持研究的科技类别列表。如果为空，则可能支持所有类别。
        /// </summary>
        public List<string> SupportedCategories; // 支持的科技类别ID或名称
        /// <summary>
        /// 维持此设施运作的周期性资源消耗。
        /// </summary>
        public float MaintenanceCost; // 例如：每小时消耗的能源或资源点
        /// <summary>
        /// 此设施运作时的电力消耗。
        /// </summary>
        public float PowerConsumption; // 例如：单位电力/小时
        
        /// <summary>
        /// ResearchFacilityData的构造函数。
        /// </summary>
        public ResearchFacilityData()
        {
            SupportedCategories = new List<string>();
            Efficiency = 1f;    // 默认效率100%
            IsOperational = true; // 默认可运作
        }
    }

    /// <summary>
    /// 研究员（科学家）的数据模型。
    /// </summary>
    [System.Serializable]
    public class ResearcherData // 此类与SurvivorData有较大重叠，可考虑是否能统一或继承
    {
        /// <summary>
        /// 研究员的唯一ID (通常是幸存者ID)。
        /// </summary>
        public int Id;
        /// <summary>
        /// 研究员的姓名。
        /// </summary>
        public string Name;
        /// <summary>
        /// 研究员的专业领域 (参考ResearcherSpecialty枚举)，可能影响特定类别科技的研究效率。
        /// </summary>
        public ResearcherSpecialty Specialty;
        /// <summary>
        /// 研究员的科研等级。
        /// </summary>
        public int Level;
        /// <summary>
        /// 研究员的个人研究效率乘数。
        /// </summary>
        public float Efficiency; // 个人效率，例如 0.8, 1.0, 1.5
        /// <summary>
        /// 当前分配到的研究项目ID或研究设施ID。
        /// </summary>
        public string CurrentAssignment; // 例如：科技ID或研究设施ID
        /// <summary>
        /// 研究员当前是否可用于分配研究任务。
        /// </summary>
        public bool IsAvailable;
        /// <summary>
        /// 研究员的科研经验值。
        /// </summary>
        public float Experience;
        /// <summary>
        /// 研究员掌握的额外技能列表 (技能ID或名称)。
        /// </summary>
        public List<string> Skills; // 可能影响特定研究的特殊技能
        
        /// <summary>
        /// ResearcherData的构造函数。
        /// </summary>
        public ResearcherData()
        {
            Skills = new List<string>();
            Efficiency = 1f;    // 默认效率100%
            IsAvailable = true; // 默认可用
        }
    }

    /// <summary>
    /// 科技研究队列的数据模型。
    /// 管理待研究的科技项目列表。
    /// </summary>
    [System.Serializable]
    public class ResearchQueue
    {
        /// <summary>
        /// 当前在队列中等待研究的科技ID列表。
        /// </summary>
        public List<string> QueuedTechIds;
        /// <summary>
        /// （冗余字段，通常由TechSystemData.CurrentResearch管理）当前正在研究的科技ID。
        /// </summary>
        public string CurrentTechId; // 可能与TechSystemData中的CurrentResearch重复
        /// <summary>
        /// 研究队列的最大容量。
        /// </summary>
        public int MaxQueueSize;
        /// <summary>
        /// 是否启用自动将可研究科技加入队列的功能。
        /// </summary>
        public bool AutoQueue; // 是否自动将可研究项加入队列
        
        /// <summary>
        /// ResearchQueue的构造函数。
        /// </summary>
        public ResearchQueue()
        {
            QueuedTechIds = new List<string>();
            MaxQueueSize = 5; // 默认队列上限为5
            AutoQueue = false; // 默认不自动排队
        }
    }

    /// <summary>
    /// 全局科技研究状态的概要数据。
    /// </summary>
    [System.Serializable]
    public class ResearchStatus // 此类与TechSystemData有部分重叠，需明确各自职责
    {
        /// <summary>
        /// 游戏中已定义的科技总数。
        /// </summary>
        public int TotalTechs;
        /// <summary>
        /// 已完成研究的科技数量。
        /// </summary>
        public int ResearchedTechs;
        /// <summary>
        /// 当前正在进行的研究项目数量。
        /// </summary>
        public int ActiveResearch; // 正在进行的研究数量
        /// <summary>
        /// （冗余字段，通常由TechSystemData.ResearchPoints管理）当前拥有的总科研点数。
        /// </summary>
        public float TotalResearchPoints; // 可能与TechSystemData中的ResearchPoints重复
        /// <summary>
        /// （冗余字段，通常由TechSystemData.ResearchPointsPerSecond管理）科研点数的每秒产出速率。
        /// </summary>
        public float ResearchPointsPerSecond; // 可能与TechSystemData中的ResearchPointsPerSecond重复
        /// <summary>
        /// 当前可用的研究设施数量。
        /// </summary>
        public int ResearchFacilities; // 可用研究设施数量
        /// <summary>
        /// 当前参与研究的研究员总数。
        /// </summary>
        public int ActiveResearchers;  // 活跃研究员数量
        /// <summary>
        /// （可能指）当前研究的重点方向或类别。
        /// </summary>
        public string CurrentFocus; // 当前研究焦点 (例如某个科技类别)
        
        /// <summary>
        /// 计算并获取整体研究进度的百分比（已研究科技数 / 总科技数）。
        /// </summary>
        /// <returns>整体研究进度的百分比 (0到1)。</returns>
        public float GetResearchProgress()
        {
            return TotalTechs > 0 ? (float)ResearchedTechs / TotalTechs : 0f;
        }
    }

    /// <summary>
    /// 科技的类别枚举。
    /// 用于对科技进行功能或领域上的分类。
    /// </summary>
    public enum TechCategory
    {
        Production,     // 生产类：提升资源产出、制造效率等。
        Defense,        // 防御类：解锁或强化防御建筑、单位防御能力等。
        Medical,        // 医疗类：提升治疗效果、解锁新药品或医疗设施等。
        Energy,         // 能源类：提升能源产出效率、解锁新能源技术等。
        Military,       // 军事类：提升单位战斗力、解锁新兵种或武器等 (与Defense可能部分重叠，需明确区分)。
        Research,       // 研究类：提升科研效率、解锁更高级研究设施等。
        Special         // 特殊类：不属于以上常见分类的特殊科技。
    }

    /// <summary>
    /// 科技的类型或层级枚举 (对应TechNode中的Type字段，可能也关联旧的TechData.Tier)。
    /// 用于表示科技的技术阶段或先进程度。
    /// </summary>
    public enum TechType // 对应TechNode.Type，可能映射到TechData.Tier
    {
        Basic,          // 基础科技：游戏早期即可接触的入门级科技。
        Applied,        // 应用科技：在基础科技之上，将理论应用于实践的科技。
        Advanced,       // 高级科技：技术含量较高，通常能带来显著提升。
        Experimental    // 实验性或尖端科技：风险与回报并存，可能解锁非常规能力。
    }

    /// <summary>
    /// 科技的研发状态枚举。
    /// </summary>
    public enum TechStatus
    {
        Locked,         // 锁定状态：前置条件未满足或尚未发现。
        Available,      // 可研究状态：所有前置条件已满足，可以开始研究。
        Researching,    // 研究中状态：正在消耗科研点数和时间进行研究。
        Researched      // 已完成状态：研究成功，科技效果已解锁或可应用。
    }

    /// <summary>
    /// 研究员的专业领域枚举。
    /// 不同专业的研究员在研究对应类别的科技时可能有加成。
    /// </summary>
    public enum ResearcherSpecialty
    {
        General,        // 通用型：无特定专业，研究各领域效率一般。
        Production,     // 生产专家：擅长研究生产类科技。
        Defense,        // 防御专家：擅长研究防御类科技。
        Medical,        // 医疗专家：擅长研究医疗类科技。
        Energy,         // 能源专家：擅长研究能源类科技。
        Military,       // 军事专家：擅长研究军事类科技。
        Advanced        // 高级或理论专家：可能擅长研究更高级别或特殊类别的科技。
    }

    /// <summary>
    /// 定义科技解锁所需满足的特定条件。
    /// </summary>
    [System.Serializable]
    public class TechUnlockCondition
    {
        /// <summary>
        /// 条件类型 (例如："ResourceAmount", "BuildingCount", "SpecificTechResearched")。
        /// </summary>
        public string ConditionType;  // 例如："ResourceAmount", "BuildingLevel", "GameDay"
        /// <summary>
        /// 条件作用的目标 (例如：特定资源类型ID、建筑类型ID、游戏天数等)。
        /// </summary>
        public string Target;         // 例如：ResourceType.Food.ToString(), "Farm_Level2", "GameDay"
        /// <summary>
        /// 需要达到的数值。
        /// </summary>
        public float RequiredValue;  // 例如：1000 (食物数量), 2 (农场等级), 30 (游戏天数)
        /// <summary>
        /// 对此条件的文字描述，用于UI显示或调试。
        /// </summary>
        public string Description;    // 例如："拥有至少1000单位食物", "农场达到2级", "游戏进行到第30天"
        
        /// <summary>
        /// TechUnlockCondition的构造函数。
        /// </summary>
        public TechUnlockCondition(string type, string target, float value, string desc = "")
        {
            ConditionType = type;
            Target = target;
            RequiredValue = value;
            Description = desc;
        }
    }

    /// <summary>
    /// 与科技相关的成就数据模型。
    /// </summary>
    [System.Serializable]
    public class TechAchievement
    {
        /// <summary>
        /// 成就的唯一ID。
        /// </summary>
        public string Id;
        /// <summary>
        /// 成就的显示名称。
        /// </summary>
        public string Name;
        /// <summary>
        /// 成就的描述文本。
        /// </summary>
        public string Description;
        /// <summary>
        /// 解锁此成就所需完成研究的科技ID列表。
        /// </summary>
        public List<string> RequiredTechs; // 需要完成的科技ID列表
        /// <summary>
        /// 此成就是否已被玩家解锁。
        /// </summary>
        public bool IsUnlocked;
        /// <summary>
        /// 成就解锁的时间戳。
        /// </summary>
        public DateTime UnlockTime; // 解锁时间
        /// <summary>
        /// 解锁此成就后给予的奖励描述或ID。
        /// </summary>
        public string Reward; // 奖励描述或ID
        
        /// <summary>
        /// TechAchievement的构造函数。
        /// </summary>
        public TechAchievement()
        {
            RequiredTechs = new List<string>();
        }
    }

    /// <summary>
    /// 科技系统的全局数据。
    /// 存储了当前总科研点数、产速、当前研究项目等核心信息。
    /// </summary>
    [System.Serializable]
    public class TechSystemData
    {
        /// <summary>
        /// 玩家当前拥有的总科研点数。
        /// </summary>
        public float ResearchPoints;
        /// <summary>
        /// 科研点数的每秒自动生成速率。
        /// </summary>
        public float ResearchPointsPerSecond;
        /// <summary>
        /// 当前正在研究的科技的ID。如果为空，则表示没有项目在研究。
        /// </summary>
        public string CurrentResearch;
        /// <summary>
        /// 当前全局的研究效率乘数 (例如，受某些全局Buff或Debuff影响)。
        /// </summary>
        public float ResearchEfficiency;
        /// <summary>
        /// （冗余字段，通常由具体配置列表计算）游戏中定义的科技总数。
        /// </summary>
        public int TotalTechs; // 总科技数量 (可能从配置中动态计算得到)
        /// <summary>
        /// （冗余字段，通常由具体配置列表计算）已完成研究的科技数量。
        /// </summary>
        public int ResearchedTechs; // 已研究科技数量 (可能从配置中动态计算得到)
        
        /// <summary>
        /// TechSystemData的构造函数。
        /// </summary>
        public TechSystemData()
        {
            ResearchPoints = 0f;          // 初始科研点数为0
            ResearchPointsPerSecond = 1f; // 默认每秒产生1点科研点
            CurrentResearch = "";         // 初始无研究项目
            ResearchEfficiency = 1f;      // 默认研究效率100%
            TotalTechs = 0;               // 初始总科技数为0 (应由配置加载后更新)
            ResearchedTechs = 0;          // 初始已研究科技数为0
        }
        
        /// <summary>
        /// (冗余方法) 计算并获取整体研究进度的百分比（已研究科技数 / 总科技数）。
        /// 实际进度应基于单个科技的ResearchProgress和Cost。
        /// </summary>
        /// <returns>整体研究进度的百分比 (0到1)。</returns>
        public float GetResearchProgress() // 此方法可能与TechNode.ResearchProgress的含义不同，需注意区分
        {
            return TotalTechs > 0 ? (float)ResearchedTechs / TotalTechs : 0f;
        }
    }

    // --- 以下为科技系统相关的事件结构体定义 ---

    /// <summary>
    /// 科技成功解锁（研究完成）事件。
    /// </summary>
    public struct TechUnlockedEvent
    {
        /// <summary>
        /// 解锁的科技的唯一ID。
        /// </summary>
        public string TechId;
        /// <summary>
        /// 解锁的科技的显示名称。
        /// </summary>
        public string TechName;
    }

    /// <summary>
    /// 科技开始研究事件。
    /// </summary>
    public struct TechResearchStartedEvent
    {
        /// <summary>
        /// 开始研究的科技的唯一ID。
        /// </summary>
        public string TechId;
        /// <summary>
        /// 开始研究的科技的显示名称。
        /// </summary>
        public string TechName;
    }

    /// <summary>
    /// 科技研究过程完成事件（可能是某个阶段或全部完成）。
    /// </summary>
    public struct TechResearchCompletedEvent
    {
        /// <summary>
        /// 完成研究的科技的唯一ID。
        /// </summary>
        public string TechId;
        /// <summary>
        /// 完成研究的科技的显示名称。
        /// </summary>
        public string TechName;
    }

    /// <summary>
    /// 成功开始一项科技研究的事件 (通常在命令执行成功后发送)。
    /// </summary>
    public struct StartResearchSuccessEvent
    {
        /// <summary>
        /// 已成功开始研究的科技的ID。
        /// </summary>
        public string TechId;
    }

    /// <summary>
    /// 科学家分配状态变更事件。
    /// </summary>
    public struct ScientistAssignedEvent
    {
        /// <summary>
        /// 请求分配的科学家数量。
        /// </summary>
        public int RequestedCount;
        /// <summary>
        /// 实际成功分配的科学家数量。
        /// </summary>
        public int AssignedCount;
    }

    /// <summary>
    /// 科技效果已成功应用的事件。
    /// </summary>
    public struct TechEffectAppliedEvent
    {
        /// <summary>
        /// 应用了效果的科技的ID。
        /// </summary>
        public string TechId;
        /// <summary>
        /// 被应用的效果的名称或标识符。
        /// </summary>
        public string EffectName;
        /// <summary>
        /// 效果的数值。
        /// </summary>
        public float EffectValue;
    }
}