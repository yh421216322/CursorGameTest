using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 科技数据模型
    /// </summary>
    [System.Serializable]
    public class TechData
    {
        public string Id;
        public string Name;
        public string Description;
        public int Level;
        public int Cost;
        public float Duration;
        public bool IsResearched;
        public bool IsResearching;
        public float ResearchProgress;
        public List<string> Prerequisites;
        public List<string> UnlockedBy;
        public TechCategory Category;
        public TechTier Tier;
        public float ResearchStartTime;
        public float ResearchSpeed;
        public bool IsPaused;
        
        public TechData()
        {
            Prerequisites = new List<string>();
            UnlockedBy = new List<string>();
            ResearchSpeed = 1f;
        }
    }

    /// <summary>
    /// 科技节点 - 科技系统中使用的核心数据结构
    /// </summary>
    [System.Serializable]
    public class TechNode
    {
        public string Id;                       // 科技ID
        public string Name;                     // 科技名称
        public string Description;              // 科技描述
        public TechCategory Category;           // 科技分类
        public TechType Type;                   // 科技类型 (对应原Tier)
        public int ResearchCost;               // 研究点数成本
        public float ResearchTime;             // 研究时间(小时)
        public int MaterialRequirement;        // 材料需求
        public int AmmoRequirement;            // 弹药需求
        public int PowerRequirement;           // 电力需求
        public int PopulationRequirement;      // 人口需求
        public List<string> Prerequisites;     // 前置科技
        public Dictionary<string, float> Effects; // 科技效果
        public TechStatus Status;              // 科技状态
        public float ResearchProgress;         // 研究进度(0-1)
        public float ResearchStartTime;        // 研究开始时间
        public Vector2 Position;               // 在科技树中的位置
        public bool IsVisible;                 // 是否可见
        
        public TechNode()
        {
            Prerequisites = new List<string>();
            Effects = new Dictionary<string, float>();
            IsVisible = true;
            Status = TechStatus.Locked;
        }
        
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
            Prerequisites = prerequisites ?? new List<string>();
            Effects = new Dictionary<string, float>();
            IsVisible = true;
            Status = TechStatus.Locked;
        }
        
        /// <summary>
        /// 检查前置条件是否满足
        /// </summary>
        public bool ArePrerequisitesMet(Dictionary<string, TechNode> allTechs)
        {
            foreach (var prereqId in Prerequisites)
            {
                if (!allTechs.ContainsKey(prereqId) || allTechs[prereqId].Status != TechStatus.Researched)
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 获取科技状态显示文本
        /// </summary>
        public string GetStatusText()
        {
            switch (Status)
            {
                case TechStatus.Researched:
                    return "已完成";
                case TechStatus.Researching:
                    return $"研究中 ({ResearchProgress:P0})";
                case TechStatus.Available:
                    return "可研究";
                case TechStatus.Locked:
                default:
                    return "已锁定";
            }
        }
        
        /// <summary>
        /// 获取科技效果总结
        /// </summary>
        public string GetEffectsSummary()
        {
            if (Effects == null || Effects.Count == 0)
                return "无特殊效果";
                
            var summaryParts = new List<string>();
            foreach (var effect in Effects)
            {
                summaryParts.Add($"{effect.Key}: +{effect.Value}");
            }
            
            return summaryParts.Count > 0 ? string.Join(", ", summaryParts) : "无特殊效果";
        }
    }

    /// <summary>
    /// 科技效果数据
    /// </summary>
    [System.Serializable]
    public class TechEffect
    {
        public string TechId;
        public string EffectType;
        public float Value;
        public string Target;
        public bool IsActive;
        
        public TechEffect(string techId, string effectType, float value, string target = "")
        {
            TechId = techId;
            EffectType = effectType;
            Value = value;
            Target = target;
            IsActive = false;
        }
    }

    /// <summary>
    /// 科技树节点
    /// </summary>
    [System.Serializable]
    public class TechTreeNode
    {
        public TechData TechData;
        public Vector2 Position;
        public List<string> ChildrenIds;
        public List<string> ParentIds;
        public bool IsVisible;
        public bool IsUnlocked;
        
        public TechTreeNode()
        {
            ChildrenIds = new List<string>();
            ParentIds = new List<string>();
            IsVisible = true;
        }
    }

    /// <summary>
    /// 研究设施数据
    /// </summary>
    [System.Serializable]
    public class ResearchFacilityData
    {
        public int Id;
        public string Name;
        public int Level;
        public float Efficiency;
        public bool IsOperational;
        public Vector3 Position;
        public List<string> SupportedCategories;
        public float MaintenanceCost;
        public float PowerConsumption;
        
        public ResearchFacilityData()
        {
            SupportedCategories = new List<string>();
            Efficiency = 1f;
            IsOperational = true;
        }
    }

    /// <summary>
    /// 研究员数据
    /// </summary>
    [System.Serializable]
    public class ResearcherData
    {
        public int Id;
        public string Name;
        public ResearcherSpecialty Specialty;
        public int Level;
        public float Efficiency;
        public string CurrentAssignment;
        public bool IsAvailable;
        public float Experience;
        public List<string> Skills;
        
        public ResearcherData()
        {
            Skills = new List<string>();
            Efficiency = 1f;
            IsAvailable = true;
        }
    }

    /// <summary>
    /// 科技研究队列
    /// </summary>
    [System.Serializable]
    public class ResearchQueue
    {
        public List<string> QueuedTechIds;
        public string CurrentTechId;
        public int MaxQueueSize;
        public bool AutoQueue;
        
        public ResearchQueue()
        {
            QueuedTechIds = new List<string>();
            MaxQueueSize = 5;
            AutoQueue = false;
        }
    }

    /// <summary>
    /// 科技研究状态
    /// </summary>
    [System.Serializable]
    public class ResearchStatus
    {
        public int TotalTechs;
        public int ResearchedTechs;
        public int ActiveResearch;
        public float TotalResearchPoints;
        public float ResearchPointsPerSecond;
        public int ResearchFacilities;
        public int ActiveResearchers;
        public string CurrentFocus;
        
        public float GetResearchProgress()
        {
            return TotalTechs > 0 ? (float)ResearchedTechs / TotalTechs : 0f;
        }
    }

    /// <summary>
    /// 科技类别枚举
    /// </summary>
    public enum TechCategory
    {
        Production,     // 生产
        Defense,        // 防御
        Medical,        // 医疗
        Energy,         // 能源
        Military,       // 军事
        Research,       // 研究
        Special         // 特殊
    }



    /// <summary>
    /// 科技类型枚举 (对应TechTier)
    /// </summary>
    public enum TechType
    {
        Basic,          // 基础
        Applied,        // 应用
        Advanced,       // 高级
        Experimental    // 实验性
    }

    /// <summary>
    /// 科技状态枚举
    /// </summary>
    public enum TechStatus
    {
        Locked,         // 锁定
        Available,      // 可研究
        Researching,    // 研究中
        Researched      // 已完成
    }

    /// <summary>
    /// 研究员专业枚举
    /// </summary>
    public enum ResearcherSpecialty
    {
        General,        // 通用
        Production,     // 生产专家
        Defense,        // 防御专家
        Medical,        // 医疗专家
        Energy,         // 能源专家
        Military,       // 军事专家
        Advanced        // 高级专家
    }

    /// <summary>
    /// 科技解锁条件
    /// </summary>
    [System.Serializable]
    public class TechUnlockCondition
    {
        public string ConditionType;
        public string Target;
        public float RequiredValue;
        public string Description;
        
        public TechUnlockCondition(string type, string target, float value, string desc = "")
        {
            ConditionType = type;
            Target = target;
            RequiredValue = value;
            Description = desc;
        }
    }

    /// <summary>
    /// 科技成就数据
    /// </summary>
    [System.Serializable]
    public class TechAchievement
    {
        public string Id;
        public string Name;
        public string Description;
        public List<string> RequiredTechs;
        public bool IsUnlocked;
        public DateTime UnlockTime;
        public string Reward;
        
        public TechAchievement()
        {
            RequiredTechs = new List<string>();
        }
    }

    /// <summary>
    /// 科技系统数据
    /// </summary>
    [System.Serializable]
    public class TechSystemData
    {
        public float ResearchPoints;           // 当前研究点数
        public float ResearchPointsPerSecond; // 每秒研究点数生成
        public string CurrentResearch;         // 当前研究的科技ID
        public float ResearchEfficiency;      // 研究效率
        public int TotalTechs;                // 总科技数量
        public int ResearchedTechs;           // 已研究科技数量
        
        public TechSystemData()
        {
            ResearchPoints = 0f;
            ResearchPointsPerSecond = 1f;
            CurrentResearch = "";
            ResearchEfficiency = 1f;
            TotalTechs = 0;
            ResearchedTechs = 0;
        }
        
        /// <summary>
        /// 获取研究进度百分比
        /// </summary>
        public float GetResearchProgress()
        {
            return TotalTechs > 0 ? (float)ResearchedTechs / TotalTechs : 0f;
        }
    }

    /// <summary>
    /// 科技解锁事件
    /// </summary>
    public struct TechUnlockedEvent
    {
        public string TechId;
        public string TechName;
    }

    /// <summary>
    /// 科技研究开始事件
    /// </summary>
    public struct TechResearchStartedEvent
    {
        public string TechId;
        public string TechName;
    }

    /// <summary>
    /// 科技研究完成事件
    /// </summary>
    public struct TechResearchCompletedEvent
    {
        public string TechId;
        public string TechName;
    }

    /// <summary>
    /// 开始研究成功事件
    /// </summary>
    public struct StartResearchSuccessEvent
    {
        public string TechId;
    }

    /// <summary>
    /// 科学家分配事件
    /// </summary>
    public struct ScientistAssignedEvent
    {
        public int RequestedCount;
        public int AssignedCount;
    }

    /// <summary>
    /// 科技效果应用事件
    /// </summary>
    public struct TechEffectAppliedEvent
    {
        public string TechId;
        public string EffectName;
        public float EffectValue;
    }
} 