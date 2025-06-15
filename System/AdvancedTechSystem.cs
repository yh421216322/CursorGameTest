// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：AdvancedTechSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月15日 // 根据实际情况修改
// 修改日期：2024年07月15日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件实现了高级科技系统 (AdvancedTechSystem)，负责管理游戏中的科技研发流程，
//     包括科技树的初始化、研究的开始、暂停、取消与完成，研究点数的管理，
//     科技效果的应用与移除，以及科技状态的查询和更新。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 高级科技系统接口。
    /// 定义了科技系统的核心功能，包括研究管理、点数管理、科技查询、进度跟踪和效果应用。
    /// </summary>
    public interface IAdvancedTechSystem : QFISystem
    {
        // --- 研究管理 ---
        /// <summary>
        /// 开始一项新的科技研究。
        /// </summary>
        /// <param name="techId">要研究的科技ID。</param>
        /// <returns>如果成功开始研究则返回true，否则返回false。</returns>
        bool StartResearch(string techId);
        /// <summary>
        /// 暂停当前正在研究的科技。
        /// </summary>
        /// <param name="techId">要暂停的科技ID。</param>
        /// <returns>如果成功暂停则返回true，否则返回false。</returns>
        bool PauseResearch(string techId);
        /// <summary>
        /// 取消一项正在研究或已暂停的科技。
        /// </summary>
        /// <param name="techId">要取消的科技ID。</param>
        /// <returns>如果成功取消则返回true，否则返回false。</returns>
        bool CancelResearch(string techId);
        /// <summary>
        /// 标记一项科技为研究完成。
        /// </summary>
        /// <param name="techId">已完成研究的科技ID。</param>
        void CompleteResearch(string techId);
        
        // --- 研究点数管理 ---
        /// <summary>
        /// 增加指定数量的研究点数。
        /// </summary>
        /// <param name="points">要增加的点数量。</param>
        void AddResearchPoints(float points);
        /// <summary>
        /// 消耗指定数量的研究点数。
        /// </summary>
        /// <param name="points">要消耗的点数量。</param>
        /// <returns>如果点数充足并成功消耗则返回true，否则返回false。</returns>
        bool ConsumeResearchPoints(float points);
        /// <summary>
        /// 获取当前拥有的研究点数总量。
        /// </summary>
        /// <returns>当前研究点数。</returns>
        float GetCurrentResearchPoints();
        /// <summary>
        /// 获取研究点数的每秒产出速率。
        /// </summary>
        /// <returns>每秒产出的研究点数。</returns>
        float GetResearchPointsPerSecond();
        
        // --- 科技查询 ---
        /// <summary>
        /// 根据ID获取指定的科技节点信息。
        /// </summary>
        /// <param name="techId">科技ID。</param>
        /// <returns>对应的TechNode实例，如果不存在则返回null。</returns>
        TechNode GetTech(string techId);
        /// <summary>
        /// 获取所有当前可研究的科技列表。
        /// </summary>
        /// <returns>可研究科技的列表。</returns>
        List<TechNode> GetAvailableTechs();
        /// <summary>
        /// 获取所有已研究完成的科技列表。
        /// </summary>
        /// <returns>已研究科技的列表。</returns>
        List<TechNode> GetResearchedTechs();
        /// <summary>
        /// 根据科技类别获取该类别下的所有科技列表。
        /// </summary>
        /// <param name="category">科技类别。</param>
        /// <returns>指定类别下的科技列表。</returns>
        List<TechNode> GetTechsByCategory(TechCategory category);
        /// <summary>
        /// 检查指定的科技是否已经研究完成。
        /// </summary>
        /// <param name="techId">科技ID。</param>
        /// <returns>如果已研究则返回true，否则返回false。</returns>
        bool IsTechResearched(string techId);
        /// <summary>
        /// 检查指定的科技当前是否可供研究（满足前置条件等）。
        /// </summary>
        /// <param name="techId">科技ID。</param>
        /// <returns>如果可研究则返回true，否则返回false。</returns>
        bool IsTechAvailable(string techId);
        
        // --- 研究进度 ---
        /// <summary>
        /// 获取指定科技的当前研究进度。
        /// </summary>
        /// <param name="techId">科技ID。</param>
        /// <returns>研究进度 (0到1之间)，如果科技不存在则返回0。</returns>
        float GetResearchProgress(string techId);
        /// <summary>
        /// 获取指定科技预计的剩余完成时间（单位：游戏内小时）。
        /// </summary>
        /// <param name="techId">科技ID。</param>
        /// <returns>预计完成时间，如果科技未在研究中则返回0。</returns>
        float GetEstimatedCompletionTime(string techId);
        /// <summary>
        /// 获取整个科技系统的状态数据。
        /// </summary>
        /// <returns>TechSystemData实例。</returns>
        TechSystemData GetTechSystemData();
        
        // --- 科技效果 ---
        /// <summary>
        /// 获取指定科技效果的当前数值。
        /// </summary>
        /// <param name="effectName">效果的唯一名称或标识符。</param>
        /// <param name="defaultValue">如果效果不存在或未激活，则返回此默认值。</param>
        /// <returns>效果的数值。</returns>
        float GetTechEffectValue(string effectName, float defaultValue);
        /// <summary>
        /// 应用指定科技研究完成后所带来的效果。
        /// </summary>
        /// <param name="techId">已完成研究的科技ID。</param>
        void ApplyTechEffect(string techId);
        /// <summary>
        /// （可选功能）移除指定科技带来的效果（例如科技被禁用或回滚）。
        /// </summary>
        /// <param name="techId">要移除效果的科技ID。</param>
        void RemoveTechEffect(string techId);
        
        // --- 系统更新 ---
        /// <summary>
        /// 每帧调用的更新方法，用于处理研究进度、点数生成等。
        /// </summary>
        void Update();
    }

    /// <summary>
    /// 高级科技系统的具体实现类。
    /// 管理科技树、研究状态、研究点数及科技效果。
    /// </summary>
    public class AdvancedTechSystem : AbstractSystem, IAdvancedTechSystem
    {
        /// <summary>
        /// 游戏核心数据模型的引用。
        /// </summary>
        private ISurvivalGameModel gameModel;
        /// <summary>
        /// 科技系统的核心数据，如当前研究点数、正在研究的科技等。
        /// </summary>
        private TechSystemData techSystemData;
        /// <summary>
        /// 存储所有科技节点信息的字典，键为科技ID。
        /// </summary>
        private Dictionary<string, TechNode> techTree;
        /// <summary>
        /// 存储已激活的科技效果及其累积数值的字典，键为效果标识符。
        /// </summary>
        private Dictionary<string, float> techEffects;
        
        // 更新间隔控制 (Update Interval Control)
        /// <summary>
        /// 上一次研究进度更新的时间戳。
        /// </summary>
        private float lastResearchUpdate;
        /// <summary>
        /// 上一次研究点数生成的时间戳。
        /// </summary>
        private float lastPointsUpdate;
        /// <summary>
        /// 上一次研究效率或科技可用性检查的时间戳。
        /// </summary>
        private float lastEfficiencyUpdate;
        
        /// <summary>
        /// 研究进度更新的频率（秒）。
        /// </summary>
        private const float RESEARCH_UPDATE_INTERVAL = 1f; // 每秒更新一次研究进度
        /// <summary>
        /// 研究点数生成的频率（秒）。
        /// </summary>
        private const float POINTS_UPDATE_INTERVAL = 0.1f; // 每0.1秒更新一次研究点数（更平滑）
        /// <summary>
        /// 科技效率和可用性检查的频率（秒）。
        /// </summary>
        private const float EFFICIENCY_UPDATE_INTERVAL = 5f; // 每5秒更新一次效率相关计算

        /// <summary>
        /// 系统初始化方法。
        /// 获取模型引用，初始化科技数据结构和科技树。
        /// </summary>
        protected override void OnInit()
        {
            gameModel = this.GetModel<ISurvivalGameModel>(); // 获取游戏模型
            
            techSystemData = new TechSystemData(); // 初始化科技系统数据
            techTree = new Dictionary<string, TechNode>(); // 初始化科技树字典
            techEffects = new Dictionary<string, float>(); // 初始化科技效果字典
            
            InitializeTechTree(); // 构建科技树结构
            
            techSystemData.ResearchPoints = 10f; // 设置初始研究点数
            
            // 初始化更新时间戳
            lastResearchUpdate = Time.time;
            lastPointsUpdate = Time.time;
            lastEfficiencyUpdate = Time.time;
            
          //  Debug.Log("高级科技系统初始化完成"); // 调试日志：系统初始化完成
        }

        /// <summary>
        /// 初始化科技树，定义所有可研究的科技及其属性。
        /// </summary>
        private void InitializeTechTree()
        {
            // === 第一层：基础科技 (Tier 1: Basic Technologies) ===
            CreateTech("basic_farming", "基础农业", TechCategory.Production, TechType.Basic,
                "提高农田产量20%", 35, 1.5f, 8, 0, 0, 0, // 描述, 科研点, 小时, 材料, 弹药, 电力, 人口
                new Dictionary<string, float> { {"farm_production_bonus", 0.2f} }); // 效果：农田产量加成20%
            
            CreateTech("basic_crafting", "基础工艺", TechCategory.Production, TechType.Basic,
                "提高工坊效率15%", 40, 2f, 12, 0, 0, 0,
                new Dictionary<string, float> { {"workshop_efficiency", 0.15f} }); // 效果：工坊效率+15%
            
            CreateTech("basic_medicine", "基础医疗", TechCategory.Medical, TechType.Basic,
                "提高治疗效率25%", 45, 3f, 15, 0, 0, 0,
                new Dictionary<string, float> { {"healing_efficiency", 0.25f} }); // 效果：治疗效率+25%
            
            CreateTech("basic_defense", "基础防御", TechCategory.Defense, TechType.Basic,
                "提高建筑耐久度10%", 50, 2.5f, 10, 0, 0, 0,
                new Dictionary<string, float> { {"building_durability", 0.1f} }); // 效果：建筑耐久+10%
            
            CreateTech("basic_energy", "基础能源", TechCategory.Energy, TechType.Basic,
                "解锁发电机建造", 55, 3f, 18, 0, 0, 0,
                new Dictionary<string, float> { {"generator_unlock", 1f} }); // 效果：解锁发电机 (1f代表解锁)
            
            // === 第二层：进阶科技 (Tier 2: Applied Technologies) ===
            CreateTech("advanced_farming", "高级农业", TechCategory.Production, TechType.Applied,
                "农田产量提升至45%累计加成", 80, 5f, 25, 0, 0, 0,
                new Dictionary<string, float> { {"farm_production_bonus", 0.25f} }, // 效果：农田产量再加成25% (总共0.2+0.25=0.45)
                new List<string> { "basic_farming" }); // 前置：基础农业
            
            CreateTech("mechanized_production", "机械化生产", TechCategory.Production, TechType.Applied,
                "工坊和采石场效率提升30%", 90, 7f, 35, 0, 0, 0,
                new Dictionary<string, float> { {"production_efficiency", 0.3f} }, // 效果：通用生产效率+30%
                new List<string> { "basic_crafting" }); // 前置：基础工艺
            
            CreateTech("advanced_medicine", "高级医疗", TechCategory.Medical, TechType.Applied,
                "治疗效率提升至50%，解锁实验室", 100, 8f, 40, 0, 0, 0,
                new Dictionary<string, float> { {"healing_efficiency", 0.25f}, {"laboratory_unlock", 1f} }, // 效果：治疗效率再+25%, 解锁实验室
                new List<string> { "basic_medicine" }); // 前置：基础医疗
            
            CreateTech("reinforced_defense", "强化防御", TechCategory.Defense, TechType.Applied,
                "建筑血量提升30%", 85, 6f, 30, 0, 0, 0,
                new Dictionary<string, float> { {"building_health", 0.3f} }, // 效果：建筑生命值+30%
                new List<string> { "basic_defense" }); // 前置：基础防御
            
            CreateTech("renewable_energy", "可再生能源", TechCategory.Energy, TechType.Applied,
                "解锁太阳能板，提升能源效率", 95, 10f, 45, 0, 0, 0,
                new Dictionary<string, float> { {"solar_unlock", 1f}, {"energy_efficiency", 0.4f} }, // 效果：解锁太阳能, 能源效率+40%
                new List<string> { "basic_energy" }); // 前置：基础能源
            
            // === 第三层：高端科技 (Tier 3: Advanced Technologies) ===
            CreateTech("hydroponics", "水培技术", TechCategory.Production, TechType.Advanced,
                "解锁温室建筑，农业产量+100%", 200, 15f, 80, 0, 0, 0,
                new Dictionary<string, float> { {"greenhouse_unlock", 1f}, {"hydroponic_bonus", 1.0f} }, // 效果：解锁温室, 水培加成+100%
                new List<string> { "advanced_farming", "renewable_energy" }); // 前置：高级农业, 可再生能源
            
            CreateTech("nanotechnology", "纳米技术", TechCategory.Military, TechType.Advanced, // 注意：此处分类为军事，但效果是生产效率
                "所有生产效率提升50%", 250, 20f, 100, 0, 0, 0,
                new Dictionary<string, float> { {"nano_efficiency", 0.5f} }, // 效果：纳米效率+50%
                new List<string> { "mechanized_production", "advanced_medicine" }); // 前置：机械化生产, 高级医疗
            
            CreateTech("quantum_computing", "量子计算", TechCategory.Research, TechType.Advanced,
                "研究速度提升100%", 300, 30f, 150, 0, 0, 0,
                new Dictionary<string, float> { {"research_speed_bonus", 1.0f} }, // 效果：研究速度+100%
                new List<string> { "advanced_medicine", "renewable_energy" }); // 前置：高级医疗, 可再生能源
            
            // === 第四层：终极科技 (Tier 4: Experimental/Ultimate Technologies) ===
            CreateTech("ultimate_survival", "终极生存", TechCategory.Special, TechType.Experimental,
                "全面生存加成200%，解锁逃生协议", 500, 50f, 300, 0, 25, 0, // 人口需求25示例
                new Dictionary<string, float> { {"ultimate_survival_bonus", 2.0f}, {"exodus_protocol", 1f} }, // 效果：终极生存加成, 解锁逃生协议
                new List<string> { "hydroponics", "nanotechnology", "quantum_computing" }); // 前置：三大三级科技
            
            SetTechAvailability(); // 设置初始可研究的科技（通常是基础科技）
        }
        
        /// <summary>
        /// 创建一个新的科技节点并将其添加到科技树中。
        /// </summary>
        /// <param name="id">科技的唯一ID。</param>
        /// <param name="name">科技的显示名称。</param>
        /// <param name="category">科技所属类别。</param>
        /// <param name="type">科技的类型或层级。</param>
        /// <param name="description">科技的描述文本。</param>
        /// <param name="cost">研究所需科研点数。</param>
        /// <param name="timeHours">研究所需基础时间（小时）。</param>
        /// <param name="materials">研究所需材料数量。</param>
        /// <param name="ammo">研究所需弹药数量。</param>
        /// <param name="power">研究所需电力（可能指设施要求或消耗）。</param>
        /// <param name="population">研究所需人口前置。</param>
        /// <param name="effects">科技完成后产生的效果字典。</param>
        /// <param name="prerequisites">前置科技ID列表，默认为null。</param>
        private void CreateTech(string id, string name, TechCategory category, TechType type, 
                              string description, int cost, float timeHours, int materials, 
                              int ammo, int power, int population,
                              Dictionary<string, float> effects, List<string> prerequisites = null)
        {
            var tech = new TechNode // 创建科技节点实例
            {
                Id = id,
                Name = name, // 科技名称
                Category = category, // 科技类别
                Type = type, // 科技类型/层级
                Description = description, // 科技描述
                ResearchCost = cost, // 研究点数成本
                ResearchTime = timeHours, // 研究时间（小时）
                MaterialRequirement = materials, // 所需材料
                AmmoRequirement = ammo, // 所需弹药
                PowerRequirement = power, // 所需电力
                PopulationRequirement = population, // 所需人口
                Prerequisites = prerequisites ?? new List<string>(), // 前置科技ID列表
                Effects = effects ?? new Dictionary<string, float>(), // 科技效果
                Status = TechStatus.Locked, // 初始状态：锁定
                ResearchProgress = 0f // 初始研究进度：0
            };
            
            techTree[id] = tech; // 添加到科技树字典
        }
        
        /// <summary>
        /// 设置初始科技的可用性。
        /// 通常，所有基础科技（Basic TechType）在游戏开始时即可研究。
        /// </summary>
        private void SetTechAvailability()
        {
            foreach (var tech in techTree.Values.Where(t => t.Type == TechType.Basic)) // 筛选出所有基础科技
            {
                tech.Status = TechStatus.Available; // 将其状态设置为可研究
            }
        }

        /// <summary>
        /// 尝试开始一项科技研究。
        /// 会检查科技是否可研究、研究点数是否足够、资源是否满足。
        /// </summary>
        /// <param name="techId">要研究的科技ID。</param>
        /// <returns>成功开始返回true，否则返回false。</returns>
        public bool StartResearch(string techId)
        {
            if (!IsTechAvailable(techId)) // 检查科技是否满足基本可研究条件（如前置完成）
            {
                Debug.LogWarning($"[科技系统] 科技 {techId} 前置条件未满足或不存在，无法开始研究。");
                return false;
            }
            
            var tech = techTree[techId]; // 获取科技节点
            
            if (techSystemData.ResearchPoints < tech.ResearchCost) // 检查研究点数是否足够
            {
                Debug.LogWarning($"[科技系统] 研究点数不足以开始研究 {tech.Name}。需要 {tech.ResearchCost}，当前 {techSystemData.ResearchPoints}。");
                return false;
            }
            
            if (!CheckResourceRequirements(tech)) // 检查其他资源需求（材料、弹药等）
            {
                // CheckResourceRequirements内部会打印具体不足的资源
                Debug.LogWarning($"[科技系统] 启动研究 {tech.Name} 的资源需求不满足。");
                return false;
            }
            
            // 消耗研究点数 (注意：此处消耗的是科研点，其他材料等资源通常在Update中随进度消耗或一次性消耗)
            techSystemData.ResearchPoints -= tech.ResearchCost;
            
            // 如果当前有其他科技正在研究，则暂停它
            if (!string.IsNullOrEmpty(techSystemData.CurrentResearch) && techSystemData.CurrentResearch != techId)
            {
                PauseResearch(techSystemData.CurrentResearch);
            }
            
            // 设置当前研究项目
            techSystemData.CurrentResearch = techId;
            tech.Status = TechStatus.Researching; // 更新科技状态为研究中
            tech.ResearchStartTime = Time.time;   // 记录研究开始时间
            
            Debug.Log($"[科技系统] 开始研究：{tech.Name} (ID: {techId})");
            this.SendEvent(new TechResearchStartedEvent { TechId = techId, TechName = tech.Name }); // 发送事件
            return true;
        }
        
        /// <summary>
        /// 暂停指定ID的科技研究。
        /// </summary>
        /// <param name="techId">要暂停的科技ID。</param>
        /// <returns>成功暂停返回true，否则返回false。</returns>
        public bool PauseResearch(string techId)
        {
            if (!techTree.ContainsKey(techId) || techTree[techId].Status != TechStatus.Researching)
            {
                Debug.LogWarning($"[科技系统] 尝试暂停的科技 {techId} 不存在或未在研究中。");
                return false;
            }
            
            var tech = techTree[techId];
            tech.Status = TechStatus.Available; // 状态改回可研究，保留当前进度
            
            if (techSystemData.CurrentResearch == techId) // 如果暂停的是当前主要研究项目
            {
                techSystemData.CurrentResearch = ""; // 清空当前研究项目标识
            }
            
            Debug.Log($"[科技系统] 暂停研究：{tech.Name}");
            // 可选：发送研究暂停事件
            return true;
        }
        
        /// <summary>
        /// 取消指定ID的科技研究，并返还部分研究点数。
        /// </summary>
        /// <param name="techId">要取消的科技ID。</param>
        /// <returns>成功取消返回true，否则返回false。</returns>
        public bool CancelResearch(string techId)
        {
            if (!techTree.ContainsKey(techId))
            {
                Debug.LogWarning($"[科技系统] 尝试取消的科技 {techId} 不存在。");
                return false;
            }
            
            var tech = techTree[techId];
            
            // 只能取消正在研究中或已暂停（状态为Available但有进度）的科技
            if (tech.Status == TechStatus.Researching || (tech.Status == TechStatus.Available && tech.ResearchProgress > 0))
            {
                float refundPoints = tech.ResearchCost * 0.5f; // 假设返还50%的科研点
                AddResearchPoints(refundPoints); // 增加返还的研究点数
                
                tech.Status = TechStatus.Available; // 状态设置回可研究
                tech.ResearchProgress = 0f;         // 研究进度清零
                tech.ResearchStartTime = 0f;        // 开始时间清零
                
                if (techSystemData.CurrentResearch == techId) // 如果取消的是当前主要研究项目
                {
                    techSystemData.CurrentResearch = ""; // 清空当前研究项目标识
                }
                
                Debug.Log($"[科技系统] 取消研究：{tech.Name}。已返还 {refundPoints} 研究点数。");
                // 可选：发送研究取消事件
                return true;
            }
            
            Debug.LogWarning($"[科技系统] 科技 {tech.Name} 未处于可取消状态 (当前状态: {tech.Status})。");
            return false;
        }
        
        /// <summary>
        /// 完成指定ID的科技研究。
        /// 更新科技状态，应用效果，并解锁相关后续科技。
        /// </summary>
        /// <param name="techId">已完成的科技ID。</param>
        public void CompleteResearch(string techId)
        {
            if (!techTree.ContainsKey(techId))
            {
                Debug.LogError($"[科技系统] 尝试完成不存在的科技ID: {techId}");
                return;
            }
            
            var tech = techTree[techId];
            tech.Status = TechStatus.Researched; // 标记为已研究完成
            tech.ResearchProgress = 1f;          // 确保进度为100%
            
            if (techSystemData.CurrentResearch == techId) // 如果完成的是当前主要研究项目
            {
                techSystemData.CurrentResearch = ""; // 清空当前研究项目标识
            }
            
            ApplyTechEffect(techId);      // 应用此科技带来的效果
            UnlockDependentTechs(techId); // 检查并解锁依赖此科技的其他科技
            
            Debug.Log($"[科技系统] 研究完成：{tech.Name} (ID: {techId})");
            this.SendEvent(new TechResearchCompletedEvent { TechId = techId, TechName = tech.Name }); // 发送事件
        }

        /// <summary>
        /// 向系统中添加研究点数。
        /// </summary>
        /// <param name="points">要添加的点数数量。</param>
        public void AddResearchPoints(float points)
        {
            if (points < 0) return; // 防止负数点数
            techSystemData.ResearchPoints += points;
        }
        
        /// <summary>
        /// 尝试消耗指定数量的研究点数。
        /// </summary>
        /// <param name="points">要消耗的点数数量。</param>
        /// <returns>如果点数足够且消耗成功则返回true，否则返回false。</returns>
        public bool ConsumeResearchPoints(float points)
        {
            if (points < 0) return false; // 不能消耗负数点数
            if (techSystemData.ResearchPoints >= points)
            {
                techSystemData.ResearchPoints -= points;
                return true;
            }
            Debug.LogWarning("[科技系统] 尝试消耗研究点数失败，点数不足。");
            return false;
        }
        
        /// <summary>
        /// 获取当前拥有的总研究点数。
        /// </summary>
        /// <returns>当前研究点数。</returns>
        public float GetCurrentResearchPoints()
        {
            return techSystemData.ResearchPoints;
        }
        
        /// <summary>
        /// 计算并获取当前每秒研究点数的产出速率。
        /// 此速率可能受多种因素影响（基础速率、人口、其他科技加成、建筑加成等）。
        /// </summary>
        /// <returns>每秒产出的研究点数。</returns>
        public float GetResearchPointsPerSecond()
        {
            float baseRate = 0.5f; // 基础每秒研究点数产出速率
            float populationBonus = gameModel.Population.Value * 0.02f; // 每单位人口提供的加成
            float techRateBonus = GetTechEffectValue("research_points_rate_bonus", 0f); // 从科技效果获取的产点速率百分比加成
            float buildingBonus = 0f; // 来自研究建筑的固定点数或百分比加成 (此处暂未实现，可扩展)
            
            // 总速率 = (基础 + 人口加成 + 建筑固定加成) * (1 + 科技速率百分比加成 + 建筑百分比加成)
            return (baseRate + populationBonus + buildingBonus) * (1f + techRateBonus);
        }

        /// <summary>
        /// 根据ID获取科技节点。
        /// </summary>
        /// <param name="techId">科技ID。</param>
        /// <returns>对应的TechNode，如果不存在则返回null。</returns>
        public TechNode GetTech(string techId)
        {
            return techTree.TryGetValue(techId, out var tech) ? tech : null;
        }
        
        /// <summary>
        /// 获取所有当前状态为“可研究”的科技列表。
        /// </summary>
        /// <returns>可研究科技的列表。</returns>
        public List<TechNode> GetAvailableTechs()
        {
            return techTree.Values.Where(t => t.Status == TechStatus.Available).ToList();
        }
        
        /// <summary>
        /// 获取所有当前状态为“已研究”的科技列表。
        /// </summary>
        /// <returns>已研究科技的列表。</returns>
        public List<TechNode> GetResearchedTechs()
        {
            return techTree.Values.Where(t => t.Status == TechStatus.Researched).ToList();
        }
        
        /// <summary>
        /// 根据指定的科技类别获取该类别下的所有科技列表。
        /// </summary>
        /// <param name="category">要查询的科技类别。</param>
        /// <returns>属于该类别的科技列表。</returns>
        public List<TechNode> GetTechsByCategory(TechCategory category)
        {
            return techTree.Values.Where(t => t.Category == category).ToList();
        }
        
        /// <summary>
        /// 检查指定ID的科技是否已经研究完成。
        /// </summary>
        /// <param name="techId">要检查的科技ID。</param>
        /// <returns>如果已研究则返回true，否则返回false。</returns>
        public bool IsTechResearched(string techId)
        {
            return techTree.TryGetValue(techId, out var tech) && tech.Status == TechStatus.Researched;
        }
        
        /// <summary>
        /// 检查指定ID的科技当前是否可供研究。
        /// 主要检查其状态是否为“可研究”或“研究中”（允许对正在研究的再次检查），并验证所有前置科技是否已完成。
        /// </summary>
        /// <param name="techId">要检查的科技ID。</param>
        /// <returns>如果可研究则返回true，否则返回false。</returns>
        public bool IsTechAvailable(string techId)
        {
            if (!techTree.TryGetValue(techId, out var tech)) // 科技不存在
                return false;
            
            // 如果科技状态不是 Available 或 Researching，则不可用
            // (Researching状态也认为是"available"的一种形式，因为它满足了前置条件且已被选择)
            if (tech.Status != TechStatus.Available && tech.Status != TechStatus.Researching)
                return false;
            
            // 检查所有前置科技是否都已研究完成
            foreach (string prereqId in tech.Prerequisites)
            {
                if (!IsTechResearched(prereqId)) // 只要有一个前置科技未完成
                    return false;
            }
            
            return true; // 所有条件满足，科技可研究
        }

        /// <summary>
        /// 获取指定科技的当前研究进度 (0到1)。
        /// </summary>
        /// <param name="techId">科技ID。</param>
        /// <returns>研究进度，如果科技不存在则返回0。</returns>
        public float GetResearchProgress(string techId)
        {
            return techTree.TryGetValue(techId, out var tech) ? tech.ResearchProgress : 0f;
        }
        
        /// <summary>
        /// 估算指定科技的剩余完成时间（单位：游戏内小时）。
        /// </summary>
        /// <param name="techId">科技ID。</param>
        /// <returns>预计剩余小时数。如果科技不存在、未在研究中或研究速度为0，则可能返回0或无穷大。</returns>
        public float GetEstimatedCompletionTime(string techId)
        {
            if (!techTree.TryGetValue(techId, out var tech) || tech.Status != TechStatus.Researching)
                return 0f; // 科技不存在或未在研究中
            
            float totalResearchTimeSeconds = tech.ResearchTime * 3600f; // 此科技总共需要的基础研究时间（秒）
            float remainingProgress = 1f - tech.ResearchProgress;       // 剩余进度比例

            if (remainingProgress <= 0.0001f) return 0f; // 如果进度几乎完成，则剩余时间为0
            
            float currentResearchSpeedFactor = GetResearchSpeed(); // 获取当前的研究速度修正因子 (例如1.0为标准速度)
            if (currentResearchSpeedFactor <= 0.0001f) return float.PositiveInfinity; // 研究速度为0或极小，则时间无穷大

            // 预计剩余时间（秒）= (总基础时间 * 剩余进度比例) / 当前研究速度因子
            float estimatedRemainingSeconds = (totalResearchTimeSeconds * remainingProgress) / currentResearchSpeedFactor;

            return Math.Max(0f, estimatedRemainingSeconds / 3600f); // 转换为小时并确保不为负
        }
        
        /// <summary>
        /// 获取科技系统的核心数据对象。
        /// </summary>
        /// <returns>TechSystemData实例。</returns>
        public TechSystemData GetTechSystemData()
        {
            return techSystemData;
        }

        /// <summary>
        /// 获取指定名称的科技效果的当前累积值。
        /// </summary>
        /// <param name="effectName">效果的唯一标识符。</param>
        /// <param name="defaultValue">如果效果不存在，则返回此默认值。</param>
        /// <returns>效果的当前数值。</returns>
        public float GetTechEffectValue(string effectName, float defaultValue)
        {
            return techEffects.TryGetValue(effectName, out var value) ? value : defaultValue;
        }
        
        /// <summary>
        /// 应用指定ID科技的所有效果。
        /// 会将科技定义的效果值累加到全局效果字典中。
        /// </summary>
        /// <param name="techId">已完成研究的科技ID。</param>
        public void ApplyTechEffect(string techId)
        {
            if (!techTree.TryGetValue(techId, out var tech)) // 科技不存在
            {
                Debug.LogError($"[科技系统] 尝试应用不存在的科技ID的效果: {techId}");
                return;
            }
            
            foreach (var effectEntry in tech.Effects) // 遍历该科技的所有效果
            {
                string effectKey = effectEntry.Key;
                float effectValue = effectEntry.Value;

                if (techEffects.ContainsKey(effectKey))
                {
                    techEffects[effectKey] += effectValue; // 如果效果已存在，则累加其值
                }
                else
                {
                    techEffects[effectKey] = effectValue; // 如果是新效果，则添加到字典
                }
                // Debug.Log($"[科技系统] 应用效果: {effectKey} = {techEffects[effectKey]} (来自 {tech.Name})");
                // 可在此处发送事件，通知其他系统特定效果值已更新
                // this.SendEvent(new TechEffectChangedEvent { EffectName = effectKey, NewValue = techEffects[effectKey] });
            }
            
            Debug.Log($"[科技系统] 已应用科技 {tech.Name} 的所有效果。");
        }
        
        /// <summary>
        /// （可选功能）移除指定ID科技的所有效果。
        /// 注意：此功能在标准游戏中较少使用，因为科技效果通常是永久的。
        /// </summary>
        /// <param name="techId">要移除效果的科技ID。</param>
        public void RemoveTechEffect(string techId)
        {
            if (!techTree.TryGetValue(techId, out var tech)) // 科技不存在
            {
                Debug.LogWarning($"[科技系统] 尝试移除不存在的科技ID的效果: {techId}");
                return;
            }
            
            foreach (var effectEntry in tech.Effects) // 遍历该科技的所有效果
            {
                string effectKey = effectEntry.Key;
                float effectValue = effectEntry.Value;

                if (techEffects.ContainsKey(effectKey))
                {
                    techEffects[effectKey] -= effectValue; // 从全局效果中减去此科技贡献的值
                    if (techEffects[effectKey] <= 0.001f && techEffects[effectKey] >= -0.001f) // 如果效果值接近于零
                    {
                        // 可选：如果效果值非常小，可以考虑移除该键，以保持字典清洁
                        // techEffects.Remove(effectKey);
                        // Debug.Log($"[科技系统] 效果 {effectKey} 值已接近零，考虑移除。");
                    }
                    // Debug.Log($"[科技系统] 移除效果贡献: {effectKey}，当前值 = {techEffects.GetValueOrDefault(effectKey, 0f)} (来自 {tech.Name})");
                }
            }
            Debug.LogWarning($"[科技系统] 已尝试移除科技 {tech.Name} 的效果（注意：此操作可能不符合标准游戏逻辑）。");
        }

        /// <summary>
        /// 系统的主更新循环，按固定间隔处理研究进度、点数生成和效率更新。
        /// </summary>
        public void Update()
        {
            float currentTime = Time.time; // 获取当前游戏时间
            
            // 定期更新研究进度
            if (currentTime - lastResearchUpdate >= RESEARCH_UPDATE_INTERVAL)
            {
                UpdateResearchProgress(currentTime - lastResearchUpdate); // 传入实际经过的时间差
                lastResearchUpdate = currentTime; // 更新上次研究进度更新的时间戳
            }
            
            // 定期更新研究点数生成
            if (currentTime - lastPointsUpdate >= POINTS_UPDATE_INTERVAL)
            {
                UpdateResearchPointsGeneration(currentTime - lastPointsUpdate); // 传入实际经过的时间差
                lastPointsUpdate = currentTime; // 更新上次点数生成的时间戳
            }
            
            // 定期更新科技相关的效率计算或基于天数的解锁检查
            if (currentTime - lastEfficiencyUpdate >= EFFICIENCY_UPDATE_INTERVAL)
            {
                UpdateResearchEfficiency();
                lastEfficiencyUpdate = currentTime; // 更新上次效率更新的时间戳
            }
        }
        
        /// <summary>
        /// 更新当前正在研究的科技的进度。
        /// </summary>
        /// <param name="deltaTime">自上次更新以来经过的时间（秒）。</param>
        private void UpdateResearchProgress(float deltaTime)
        {
            if (string.IsNullOrEmpty(techSystemData.CurrentResearch)) // 如果没有正在研究的科技
                return;
            
            TechNode currentTech = techTree[techSystemData.CurrentResearch]; // 获取当前研究的科技节点
            
            if (currentTech.Status == TechStatus.Researching) // 确保科技仍在研究中
            {
                float researchSpeedFactor = GetResearchSpeed();       // 获取当前总的研究速度修正因子
                float totalTimeForTechSeconds = currentTech.ResearchTime * 3600f; // 此科技总共需要的基础时间（秒）
                
                if (totalTimeForTechSeconds <= 0) // 防止除以零或负数时间
                {
                    Debug.LogWarning($"[科技系统] 科技 {currentTech.Name} 的研究时间配置错误 ({currentTech.ResearchTime} 小时)。");
                    currentTech.ResearchProgress = 1f; // 直接标记为完成或处理错误
                }
                else
                {
                    // 计算此时间间隔内完成的进度量
                    // 进度增加 = (实际经过时间 * 研究速度因子) / 此科技总共需要的基础时间（秒）
                    currentTech.ResearchProgress += (deltaTime * researchSpeedFactor) / totalTimeForTechSeconds;
                }
                
                if (currentTech.ResearchProgress >= 1f) // 如果进度达到或超过100%
                {
                    currentTech.ResearchProgress = 1f; // 确保进度不超过100%
                    CompleteResearch(techSystemData.CurrentResearch); // 调用完成研究的逻辑
                }
            }
        }
        
        /// <summary>
        /// 更新研究点数的自动生成。
        /// </summary>
        /// <param name="deltaTime">自上次更新以来经过的时间（秒）。</param>
        private void UpdateResearchPointsGeneration(float deltaTime)
        {
            float pointsGenerated = GetResearchPointsPerSecond() * deltaTime; // 计算此时间间隔内产生的点数
            AddResearchPoints(pointsGenerated); // 添加到总研究点数
        }
        
        /// <summary>
        /// 更新研究效率相关的计算或基于特定条件（如游戏天数）的科技解锁。
        /// </summary>
        private void UpdateResearchEfficiency()
        {
            int currentDay = gameModel.GameDay.Value; // 获取当前游戏天数
            
            foreach (var tech in techTree.Values) // 遍历所有科技
            {
                if (tech.Status == TechStatus.Locked) // 只检查当前仍处于锁定状态的科技
                {
                    // 检查是否满足基于天数的解锁条件，并且其所有前置科技都已研究完成
                    if (ShouldUnlockTech(tech, currentDay) && tech.Prerequisites.All(prereqId => IsTechResearched(prereqId)))
                    {
                        tech.Status = TechStatus.Available; // 将科技状态更新为可研究
                        Debug.Log($"[科技系统] 科技因满足天数及前置条件而自动解锁：{tech.Name}");
                        // 可选：发送科技变为可用的事件
                        // this.SendEvent(new TechBecameAvailableEvent { TechId = tech.Id });
                    }
                }
            }
            // 此处还可以包含其他效率相关的更新逻辑，例如根据当前资源状况调整研究速度等。
        }

        /// <summary>
        /// 检查当前是否满足指定科技研究所需的各类资源（材料、弹药、人口等）。
        /// </summary>
        /// <param name="tech">要检查的科技节点。</param>
        /// <returns>如果所有资源都满足则返回true，否则返回false并在控制台打印警告。</returns>
        private bool CheckResourceRequirements(TechNode tech)
        {
            // 检查材料需求
            if (tech.MaterialRequirement > 0 && gameModel.Materials.Value < tech.MaterialRequirement)
            {
                Debug.LogWarning($"[科技系统] 研究 {tech.Name} 材料不足: 需要 {tech.MaterialRequirement}, 当前拥有 {gameModel.Materials.Value}");
                return false;
            }
            // 检查弹药需求
            if (tech.AmmoRequirement > 0 && gameModel.Ammunition.Value < tech.AmmoRequirement)
            {
                Debug.LogWarning($"[科技系统] 研究 {tech.Name} 弹药不足: 需要 {tech.AmmoRequirement}, 当前拥有 {gameModel.Ammunition.Value}");
                return false;
            }
            // 检查人口前置需求 (注意：这通常是门槛，不是消耗)
            if (tech.PopulationRequirement > 0 && gameModel.Population.Value < tech.PopulationRequirement)
            {
                 Debug.LogWarning($"[科技系统] 研究 {tech.Name} 人口不足: 需要 {tech.PopulationRequirement} 人, 当前 {gameModel.Population.Value} 人");
                return false;
            }
            // 注意：电力需求 (PowerRequirement) 在此未作为一次性消耗检查。
            // 电力通常是研究设施持续运作的条件，或影响研究速度，而不是启动研究的一次性成本。
            // 如果设计为一次性消耗，则应类似添加检查。
            
            return true; // 所有显式资源需求均满足
        }
        
        /// <summary>
        /// 当一项科技研究完成后，检查并解锁所有以此科技为直接前置条件的后续科技。
        /// </summary>
        /// <param name="completedTechId">刚刚完成研究的科技ID。</param>
        private void UnlockDependentTechs(string completedTechId)
        {
            foreach (var techNode in techTree.Values) // 遍历科技树中的所有科技节点
            {
                // 如果某科技处于锁定状态，并且其前置条件列表中包含刚完成的这项科技
                if (techNode.Status == TechStatus.Locked && techNode.Prerequisites.Contains(completedTechId))
                {
                    // 进一步检查此科技的所有前置条件是否都已满足（即都已研究完成）
                    bool allPrerequisitesMet = techNode.Prerequisites.All(prereqId => IsTechResearched(prereqId));
                    
                    if (allPrerequisitesMet) // 如果所有前置都满足
                    {
                        techNode.Status = TechStatus.Available; // 将此科技的状态更新为可研究
                        Debug.Log($"[科技系统] 因 {completedTechId} 研究完成，科技 {techNode.Name} 已解锁变为可研究状态。");
                        // 可选：发送科技变为可用的事件
                        // this.SendEvent(new TechBecameAvailableEvent { TechId = techNode.Id });
                    }
                }
            }
        }
        
        /// <summary>
        /// 根据科技类型和当前游戏天数，判断一个科技是否应该基于时间条件解锁。
        /// 此方法主要用于设定某些科技在游戏进行到特定阶段时自动变为“候选可研究”状态，
        /// 实际是否可研究还需满足其所有前置科技条件。
        /// </summary>
        /// <param name="tech">要判断的科技节点。</param>
        /// <param name="currentDay">当前游戏进行到的天数。</param>
        /// <returns>如果满足基于天数的解锁条件则返回true，否则返回false。</returns>
        private bool ShouldUnlockTech(TechNode tech, int currentDay)
        {
            // 这是一个示例性的基于天数的解锁逻辑
            switch (tech.Type) // 根据科技的类型/层级来判断
            {
                case TechType.Basic:
                    return currentDay >= 1;  // 基础科技在游戏第1天或之后就满足时间条件
                case TechType.Applied:
                    return currentDay >= 3;  // 应用科技在游戏第3天或之后满足时间条件
                case TechType.Advanced:
                    return currentDay >= 7;  // 高级科技在游戏第7天或之后满足时间条件
                case TechType.Experimental:
                    return currentDay >= 15; // 实验性或顶级科技在游戏第15天或之后满足时间条件
                default:
                    return false; // 其他未知类型默认不基于时间解锁
            }
        }
        
        /// <summary>
        /// 获取当前总的研究速度修正因子。
        /// 此因子会乘以基础研究时间来确定实际研究速率。
        /// 例如，返回1.0代表标准速度，1.5代表速度加快50%，0.5代表速度减慢50%。
        /// </summary>
        /// <returns>研究速度修正因子。</returns>
        private float GetResearchSpeed()
        {
            float baseSpeedFactor = 1f; // 基础研究速度因子，1.0代表100%标准速度

            // 从已激活的科技效果中获取研究速度相关的百分比加成
            // "research_speed_bonus" 通常是一个表示额外百分比的浮点数 (例如 0.1 代表 +10%)
            float techSpeedBonusPercentage = GetTechEffectValue("research_speed_bonus", 0f);

            // 从建筑等其他来源获取研究速度相关的百分比加成 (示例，当前为0)
            float buildingSpeedBonusPercentage = GetTechEffectValue("research_building_bonus", 0f);
            
            // 总速度因子 = 基础因子 * (1 + 所有百分比加成之和)
            // 例如，如果科技提供10%加成 (0.1)，建筑提供5%加成 (0.05)
            // 则总速度因子 = 1.0f * (1f + 0.1f + 0.05f) = 1.15f (即115%的速度，或加快15%)
            return baseSpeedFactor * (1f + techSpeedBonusPercentage + buildingSpeedBonusPercentage);
        }
    }
}