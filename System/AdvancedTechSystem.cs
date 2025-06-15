using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 高级科技系统接口
    /// </summary>
    public interface IAdvancedTechSystem : QFISystem
    {
        // 研究管理
        bool StartResearch(string techId);
        bool PauseResearch(string techId);
        bool CancelResearch(string techId);
        void CompleteResearch(string techId);
        
        // 研究点数管理
        void AddResearchPoints(float points);
        bool ConsumeResearchPoints(float points);
        float GetCurrentResearchPoints();
        float GetResearchPointsPerSecond();
        
        // 科技查询
        TechNode GetTech(string techId);
        List<TechNode> GetAvailableTechs();
        List<TechNode> GetResearchedTechs();
        List<TechNode> GetTechsByCategory(TechCategory category);
        bool IsTechResearched(string techId);
        bool IsTechAvailable(string techId);
        
        // 研究进度
        float GetResearchProgress(string techId);
        float GetEstimatedCompletionTime(string techId);
        TechSystemData GetTechSystemData();
        
        // 科技效果
        float GetTechEffectValue(string effectName, float defaultValue);
        void ApplyTechEffect(string techId);
        void RemoveTechEffect(string techId);
        
        // 系统更新
        void Update();
    }

    /// <summary>
    /// 高级科技系统实现
    /// </summary>
    public class AdvancedTechSystem : AbstractSystem, IAdvancedTechSystem
    {
        private ISurvivalGameModel gameModel;
        private TechSystemData techSystemData;
        private Dictionary<string, TechNode> techTree;
        private Dictionary<string, float> techEffects;
        
        // 更新间隔控制
        private float lastResearchUpdate;
        private float lastPointsUpdate;
        private float lastEfficiencyUpdate;
        
        private const float RESEARCH_UPDATE_INTERVAL = 1f;
        private const float POINTS_UPDATE_INTERVAL = 0.1f;
        private const float EFFICIENCY_UPDATE_INTERVAL = 5f;

        protected override void OnInit()
        {
            gameModel = this.GetModel<ISurvivalGameModel>();
            
            techSystemData = new TechSystemData();
            techTree = new Dictionary<string, TechNode>();
            techEffects = new Dictionary<string, float>();
            
            InitializeTechTree();
            
            techSystemData.ResearchPoints = 10f;
            
            lastResearchUpdate = Time.time;
            lastPointsUpdate = Time.time;
            lastEfficiencyUpdate = Time.time;
            
          //  Debug.Log("高级科技系统初始化完成");
        }

        private void InitializeTechTree()
        {
            // === 第一层：基础科技 ===
            CreateTech("basic_farming", "基础农业", TechCategory.Production, TechType.Basic,
                "提高农田产量20%", 35, 1.5f, 8, 0, 0, 0,
                new Dictionary<string, float> { {"farm_production_bonus", 0.2f} });
            
            CreateTech("basic_crafting", "基础工艺", TechCategory.Production, TechType.Basic,
                "提高工坊效率15%", 40, 2f, 12, 0, 0, 0,
                new Dictionary<string, float> { {"workshop_efficiency", 0.15f} });
            
            CreateTech("basic_medicine", "基础医疗", TechCategory.Medical, TechType.Basic,
                "提高治疗效率25%", 45, 3f, 15, 0, 0, 0,
                new Dictionary<string, float> { {"healing_efficiency", 0.25f} });
            
            CreateTech("basic_defense", "基础防御", TechCategory.Defense, TechType.Basic,
                "提高建筑耐久度10%", 50, 2.5f, 10, 0, 0, 0,
                new Dictionary<string, float> { {"building_durability", 0.1f} });
            
            CreateTech("basic_energy", "基础能源", TechCategory.Energy, TechType.Basic,
                "解锁发电机建造", 55, 3f, 18, 0, 0, 0,
                new Dictionary<string, float> { {"generator_unlock", 1f} });
            
            // === 第二层：进阶科技 ===
            CreateTech("advanced_farming", "高级农业", TechCategory.Production, TechType.Applied,
                "农田产量提升至45%累计加成", 80, 5f, 25, 0, 0, 0,
                new Dictionary<string, float> { {"farm_production_bonus", 0.25f} },
                new List<string> { "basic_farming" });
            
            CreateTech("mechanized_production", "机械化生产", TechCategory.Production, TechType.Applied,
                "工坊和采石场效率提升30%", 90, 7f, 35, 0, 0, 0,
                new Dictionary<string, float> { {"production_efficiency", 0.3f} },
                new List<string> { "basic_crafting" });
            
            CreateTech("advanced_medicine", "高级医疗", TechCategory.Medical, TechType.Applied,
                "治疗效率提升至50%，解锁实验室", 100, 8f, 40, 0, 0, 0,
                new Dictionary<string, float> { {"healing_efficiency", 0.25f}, {"laboratory_unlock", 1f} },
                new List<string> { "basic_medicine" });
            
            CreateTech("reinforced_defense", "强化防御", TechCategory.Defense, TechType.Applied,
                "建筑血量提升30%", 85, 6f, 30, 0, 0, 0,
                new Dictionary<string, float> { {"building_health", 0.3f} },
                new List<string> { "basic_defense" });
            
            CreateTech("renewable_energy", "可再生能源", TechCategory.Energy, TechType.Applied,
                "解锁太阳能板，提升能源效率", 95, 10f, 45, 0, 0, 0,
                new Dictionary<string, float> { {"solar_unlock", 1f}, {"energy_efficiency", 0.4f} },
                new List<string> { "basic_energy" });
            
            // === 第三层：高端科技 ===
            CreateTech("hydroponics", "水培技术", TechCategory.Production, TechType.Advanced,
                "解锁温室建筑，农业产量+100%", 200, 15f, 80, 0, 0, 0,
                new Dictionary<string, float> { {"greenhouse_unlock", 1f}, {"hydroponic_bonus", 1.0f} },
                new List<string> { "advanced_farming", "renewable_energy" });
            
            CreateTech("nanotechnology", "纳米技术", TechCategory.Military, TechType.Advanced,
                "所有生产效率提升50%", 250, 20f, 100, 0, 0, 0,
                new Dictionary<string, float> { {"nano_efficiency", 0.5f} },
                new List<string> { "mechanized_production", "advanced_medicine" });
            
            CreateTech("quantum_computing", "量子计算", TechCategory.Research, TechType.Advanced,
                "研究速度提升100%", 300, 30f, 150, 0, 0, 0,
                new Dictionary<string, float> { {"research_speed_bonus", 1.0f} },
                new List<string> { "advanced_medicine", "renewable_energy" });
            
            // === 第四层：终极科技 ===
            CreateTech("ultimate_survival", "终极生存", TechCategory.Special, TechType.Experimental,
                "全面生存加成200%，解锁逃生协议", 500, 50f, 300, 0, 25, 0,
                new Dictionary<string, float> { {"ultimate_survival_bonus", 2.0f}, {"exodus_protocol", 1f} },
                new List<string> { "hydroponics", "nanotechnology", "quantum_computing" });
            
            SetTechAvailability();
        }
        
        private void CreateTech(string id, string name, TechCategory category, TechType type, 
                              string description, int cost, float timeHours, int materials, 
                              int ammo, int power, int population,
                              Dictionary<string, float> effects, List<string> prerequisites = null)
        {
            var tech = new TechNode
            {
                Id = id,
                Name = name,
                Category = category,
                Type = type,
                Description = description,
                ResearchCost = cost,
                ResearchTime = timeHours,
                MaterialRequirement = materials,
                AmmoRequirement = ammo,
                PowerRequirement = power,
                PopulationRequirement = population,
                Prerequisites = prerequisites ?? new List<string>(),
                Effects = effects ?? new Dictionary<string, float>(),
                Status = TechStatus.Locked,
                ResearchProgress = 0f
            };
            
            techTree[id] = tech;
        }
        
        private void SetTechAvailability()
        {
            foreach (var tech in techTree.Values.Where(t => t.Type == TechType.Basic))
            {
                tech.Status = TechStatus.Available;
            }
        }

        public bool StartResearch(string techId)
        {
            if (!IsTechAvailable(techId))
            {
                Debug.LogWarning($"科技 {techId} 不可研究");
                return false;
            }
            
            var tech = techTree[techId];
            
            if (techSystemData.ResearchPoints < tech.ResearchCost)
            {
                Debug.LogWarning($"研究点数不足：需要 {tech.ResearchCost}，当前 {techSystemData.ResearchPoints}");
                return false;
            }
            
            if (!CheckResourceRequirements(tech))
            {
                Debug.LogWarning($"资源需求不满足：{tech.Name}");
                return false;
            }
            
            techSystemData.ResearchPoints -= tech.ResearchCost;
            
            if (!string.IsNullOrEmpty(techSystemData.CurrentResearch))
            {
                PauseResearch(techSystemData.CurrentResearch);
            }
            
            techSystemData.CurrentResearch = techId;
            tech.Status = TechStatus.Researching;
            tech.ResearchStartTime = Time.time;
            
            Debug.Log($"开始研究：{tech.Name}");
            return true;
        }
        
        public bool PauseResearch(string techId)
        {
            if (!techTree.ContainsKey(techId) || techTree[techId].Status != TechStatus.Researching)
                return false;
            
            var tech = techTree[techId];
            tech.Status = TechStatus.Available;
            
            if (techSystemData.CurrentResearch == techId)
            {
                techSystemData.CurrentResearch = "";
            }
            
            Debug.Log($"暂停研究：{tech.Name}");
            return true;
        }
        
        public bool CancelResearch(string techId)
        {
            if (!techTree.ContainsKey(techId))
                return false;
            
            var tech = techTree[techId];
            
            if (tech.Status == TechStatus.Researching)
            {
                float refund = tech.ResearchCost * 0.5f;
                techSystemData.ResearchPoints += refund;
                
                tech.Status = TechStatus.Available;
                tech.ResearchProgress = 0f;
                
                if (techSystemData.CurrentResearch == techId)
                {
                    techSystemData.CurrentResearch = "";
                }
                
                Debug.Log($"取消研究：{tech.Name}，返还 {refund} 研究点数");
                return true;
            }
            
            return false;
        }
        
        public void CompleteResearch(string techId)
        {
            if (!techTree.ContainsKey(techId))
                return;
            
            var tech = techTree[techId];
            tech.Status = TechStatus.Researched;
            tech.ResearchProgress = 1f;
            
            if (techSystemData.CurrentResearch == techId)
            {
                techSystemData.CurrentResearch = "";
            }
            
            ApplyTechEffect(techId);
            UnlockDependentTechs(techId);
            
            Debug.Log($"研究完成：{tech.Name}");
        }

        public void AddResearchPoints(float points)
        {
            techSystemData.ResearchPoints += points;
        }
        
        public bool ConsumeResearchPoints(float points)
        {
            if (techSystemData.ResearchPoints >= points)
            {
                techSystemData.ResearchPoints -= points;
                return true;
            }
            return false;
        }
        
        public float GetCurrentResearchPoints()
        {
            return techSystemData.ResearchPoints;
        }
        
        public float GetResearchPointsPerSecond()
        {
            float baseRate = 0.5f;
            float populationBonus = gameModel.Population.Value * 0.02f;
            float techBonus = GetTechEffectValue("research_speed_bonus", 0f);
            float buildingBonus = 0f;
            
            return baseRate + populationBonus + (baseRate * techBonus) + buildingBonus;
        }

        public TechNode GetTech(string techId)
        {
            return techTree.ContainsKey(techId) ? techTree[techId] : null;
        }
        
        public List<TechNode> GetAvailableTechs()
        {
            return techTree.Values.Where(t => t.Status == TechStatus.Available).ToList();
        }
        
        public List<TechNode> GetResearchedTechs()
        {
            return techTree.Values.Where(t => t.Status == TechStatus.Researched).ToList();
        }
        
        public List<TechNode> GetTechsByCategory(TechCategory category)
        {
            return techTree.Values.Where(t => t.Category == category).ToList();
        }
        
        public bool IsTechResearched(string techId)
        {
            return techTree.ContainsKey(techId) && techTree[techId].Status == TechStatus.Researched;
        }
        
        public bool IsTechAvailable(string techId)
        {
            if (!techTree.ContainsKey(techId))
                return false;
            
            var tech = techTree[techId];
            
            if (tech.Status != TechStatus.Available && tech.Status != TechStatus.Researching)
                return false;
            
            foreach (string prereq in tech.Prerequisites)
            {
                if (!IsTechResearched(prereq))
                    return false;
            }
            
            return true;
        }

        public float GetResearchProgress(string techId)
        {
            if (!techTree.ContainsKey(techId))
                return 0f;
            
            return techTree[techId].ResearchProgress;
        }
        
        public float GetEstimatedCompletionTime(string techId)
        {
            if (!techTree.ContainsKey(techId) || techTree[techId].Status != TechStatus.Researching)
                return 0f;
            
            var tech = techTree[techId];
            float timeElapsed = Time.time - tech.ResearchStartTime;
            float totalTime = tech.ResearchTime * 3600f;
            float remainingTime = totalTime - timeElapsed;
            
            return Math.Max(0f, remainingTime / 3600f);
        }
        
        public TechSystemData GetTechSystemData()
        {
            return techSystemData;
        }

        public float GetTechEffectValue(string effectName, float defaultValue)
        {
            return techEffects.ContainsKey(effectName) ? techEffects[effectName] : defaultValue;
        }
        
        public void ApplyTechEffect(string techId)
        {
            if (!techTree.ContainsKey(techId))
                return;
            
            var tech = techTree[techId];
            
            foreach (var effect in tech.Effects)
            {
                if (techEffects.ContainsKey(effect.Key))
                {
                    techEffects[effect.Key] += effect.Value;
                }
                else
                {
                    techEffects[effect.Key] = effect.Value;
                }
            }
            
            Debug.Log($"应用科技效果：{tech.Name}");
        }
        
        public void RemoveTechEffect(string techId)
        {
            if (!techTree.ContainsKey(techId))
                return;
            
            var tech = techTree[techId];
            
            foreach (var effect in tech.Effects)
            {
                if (techEffects.ContainsKey(effect.Key))
                {
                    techEffects[effect.Key] -= effect.Value;
                    if (techEffects[effect.Key] <= 0)
                    {
                        techEffects.Remove(effect.Key);
                    }
                }
            }
        }

        public void Update()
        {
            float currentTime = Time.time;
            
            if (currentTime - lastResearchUpdate >= RESEARCH_UPDATE_INTERVAL)
            {
                UpdateResearchProgress(RESEARCH_UPDATE_INTERVAL);
                lastResearchUpdate = currentTime;
            }
            
            if (currentTime - lastPointsUpdate >= POINTS_UPDATE_INTERVAL)
            {
                UpdateResearchPointsGeneration(POINTS_UPDATE_INTERVAL);
                lastPointsUpdate = currentTime;
            }
            
            if (currentTime - lastEfficiencyUpdate >= EFFICIENCY_UPDATE_INTERVAL)
            {
                UpdateResearchEfficiency();
                lastEfficiencyUpdate = currentTime;
            }
        }
        
        private void UpdateResearchProgress(float deltaTime)
        {
            if (string.IsNullOrEmpty(techSystemData.CurrentResearch))
                return;
            
            var tech = techTree[techSystemData.CurrentResearch];
            
            if (tech.Status == TechStatus.Researching)
            {
                float researchSpeed = GetResearchSpeed();
                float timeRequired = tech.ResearchTime * 3600f;
                
                tech.ResearchProgress += (deltaTime * researchSpeed) / timeRequired;
                
                if (tech.ResearchProgress >= 1f)
                {
                    CompleteResearch(techSystemData.CurrentResearch);
                }
            }
        }
        
        private void UpdateResearchPointsGeneration(float deltaTime)
        {
            float pointsPerSecond = GetResearchPointsPerSecond();
            AddResearchPoints(pointsPerSecond * deltaTime);
        }
        
        private void UpdateResearchEfficiency()
        {
            int currentDay = gameModel.GameDay.Value;
            
            foreach (var tech in techTree.Values)
            {
                if (tech.Status == TechStatus.Locked)
                {
                    if (ShouldUnlockTech(tech, currentDay))
                    {
                        tech.Status = TechStatus.Available;
                        Debug.Log($"科技解锁：{tech.Name}");
                    }
                }
            }
        }

        private bool CheckResourceRequirements(TechNode tech)
        {
            if (tech.MaterialRequirement > 0 && gameModel.Materials.Value < tech.MaterialRequirement)
                return false;
            
            if (tech.AmmoRequirement > 0 && gameModel.Ammunition.Value < tech.AmmoRequirement)
                return false;
            
            if (tech.PopulationRequirement > 0 && gameModel.Population.Value < tech.PopulationRequirement)
                return false;
            
            return true;
        }
        
        private void UnlockDependentTechs(string completedTechId)
        {
            foreach (var tech in techTree.Values)
            {
                if (tech.Status == TechStatus.Locked && tech.Prerequisites.Contains(completedTechId))
                {
                    bool allPrereqsMet = tech.Prerequisites.All(prereq => IsTechResearched(prereq));
                    
                    if (allPrereqsMet)
                    {
                        tech.Status = TechStatus.Available;
                        Debug.Log($"解锁科技：{tech.Name}");
                    }
                }
            }
        }
        
        private bool ShouldUnlockTech(TechNode tech, int currentDay)
        {
            switch (tech.Type)
            {
                case TechType.Basic:
                    return currentDay >= 1;
                case TechType.Applied:
                    return currentDay >= 3;
                case TechType.Advanced:
                    return currentDay >= 7;
                case TechType.Experimental:
                    return currentDay >= 15;
                default:
                    return false;
            }
        }
        
        private float GetResearchSpeed()
        {
            float baseSpeed = 1f;
            float techBonus = GetTechEffectValue("research_speed_bonus", 0f);
            float buildingBonus = GetTechEffectValue("research_building_bonus", 0f);
            
            return baseSpeed * (1f + techBonus + buildingBonus);
        }
    }
}