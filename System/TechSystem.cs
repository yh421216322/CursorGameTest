using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 科技系统 - 管理科技研究和科技树进度
    /// </summary>
    public class TechSystem : AbstractSystem
    {
        #region 数据存储
        
        private TechTreeData _techTreeData;
        private List<ResearchTask> _activeResearchTasks;
        private Dictionary<TechEffectType, Dictionary<string, float>> _techEffects; // 科技效果缓存
        private ISurvivalGameModel _gameModel;
        private ConfigSystem _configSystem;
        
        #endregion
        
        #region 系统初始化
        
        protected override void OnInit()
        {
            _gameModel = this.GetModel<ISurvivalGameModel>();
            _configSystem = this.GetSystem<ConfigSystem>();
            
            _techTreeData = new TechTreeData();
            _activeResearchTasks = new List<ResearchTask>();
            _techEffects = new Dictionary<TechEffectType, Dictionary<string, float>>();
            
            InitializeTechTree();
            
            // 注册事件
            this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);
        }
        
        /// <summary>
        /// 初始化科技树
        /// </summary>
        private void InitializeTechTree()
        {
            var allTechConfigs = _configSystem.GetAllTechConfigs();
            
            foreach (var techConfig in allTechConfigs.Values)
            {
                var techData = new TechData
                {
                    Id = techConfig.Id,
                    Name = techConfig.Name,
                    Description = techConfig.Description,
                    IsResearched = false,
                    IsResearching = false,
                    ResearchProgress = 0f,
                    Category = TechCategory.Production, // 默认分类，可根据配置调整
                    Tier = TechTier.Tier1 // 默认层级，可根据配置调整
                };
                
                _techTreeData.Technologies[techConfig.Id] = techData;
                
                // 按科技树分类
                if (!_techTreeData.TreeTechs[techConfig.Tree].Contains(techConfig.Id))
                {
                    _techTreeData.TreeTechs[techConfig.Tree].Add(techConfig.Id);
                }
            }
            
            // 更新科技状态
            UpdateTechAvailability();
            
            Debug.Log($"科技树初始化完成，共 {_techTreeData.Technologies.Count} 项科技");
        }
        
        #endregion
        
        #region 科技研究
        
        /// <summary>
        /// 开始研究科技
        /// </summary>
        public bool StartResearch(string techId, string researchBuildingId, List<string> researcherIds = null)
        {
            var techConfig = _configSystem.GetTechConfig(techId);
            
            if (techConfig == null)
            {
                Debug.LogError($"找不到科技配置: {techId}");
                return false;
            }
            
            if (!_techTreeData.Technologies.TryGetValue(techId, out var techData))
            {
                Debug.LogError($"科技数据不存在: {techId}");
                return false;
            }
            
            if (techData.IsResearched || techData.IsResearching)
            {
                Debug.LogWarning($"科技 {techConfig.Name} 已完成或正在研究中");
                return false;
            }
            
            // 检查是否已有研究任务
            if (_activeResearchTasks.Any(task => task.TechId == techId))
            {
                Debug.LogWarning($"科技 {techConfig.Name} 已在研究中");
                return false;
            }
            
            // 检查研究点数
            if (_techTreeData.TotalResearchPoints < techConfig.ResearchPointsCost)
            {
                Debug.LogWarning($"科研点数不足，需要 {techConfig.ResearchPointsCost}，当前 {_techTreeData.TotalResearchPoints}");
                return false;
            }
            
            // 检查额外资源消耗
            if (techConfig.AdditionalCosts != null && !CanAffordResources(techConfig.AdditionalCosts))
            {
                Debug.LogWarning($"资源不足，无法研究 {techConfig.Name}");
                return false;
            }
            
            // 检查研究建筑
            var buildingSystem = this.GetSystem<EnhancedBuildingSystem>();
            var researchBuilding = buildingSystem.GetBuilding(researchBuildingId);
            if (researchBuilding == null || researchBuilding.State != BuildingState.Operational)
            {
                Debug.LogWarning("需要运行中的研究设施");
                return false;
            }
            
            // 消耗资源
            _techTreeData.SpentResearchPoints += techConfig.ResearchPointsCost;
            if (techConfig.AdditionalCosts != null)
            {
                ConsumeResources(techConfig.AdditionalCosts);
            }
            
            // 创建研究任务
            var researchTask = new ResearchTask(techId, researchBuildingId);
            if (researcherIds != null)
            {
                researchTask.ResearcherIds.AddRange(researcherIds);
            }
            
            // 计算研究效率
            researchTask.EfficiencyMultiplier = CalculateResearchEfficiency(researchTask, techConfig);
            researchTask.EstimatedEndTime = Time.time + (techConfig.ResearchTime * 3600f / researchTask.EfficiencyMultiplier);
            
            _activeResearchTasks.Add(researchTask);
            techData.IsResearching = true;
            techData.ResearchStartTime = Time.time;
            
            Debug.Log($"开始研究科技 {techConfig.Name}，预计完成时间: {researchTask.EstimatedEndTime - Time.time:F1} 秒");
            return true;
        }
        
        /// <summary>
        /// 检查是否能负担资源消耗
        /// </summary>
        private bool CanAffordResources(List<ResourceCost> costs)
        {
            foreach (var cost in costs)
            {
                if (_gameModel.GetResourceAmount(cost.Type) < cost.Amount)
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 消耗资源
        /// </summary>
        private void ConsumeResources(List<ResourceCost> costs)
        {
            foreach (var cost in costs)
            {
                _gameModel.ConsumeResource(cost.Type, cost.Amount);
            }
        }
        
        /// <summary>
        /// 计算研究效率
        /// </summary>
        private float CalculateResearchEfficiency(ResearchTask task, TechConfig techConfig)
        {
            float efficiency = _techTreeData.GlobalResearchBonus;
            
            // 研究员加成
            var survivorSystem = this.GetSystem<SurvivorSystem>();
            foreach (var researcherId in task.ResearcherIds)
            {
                var researcher = survivorSystem.GetSurvivor(researcherId);
                if (researcher != null)
                {
                    // 使用新的属性系统
                    var researchAttr = researcher.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Research);
                    if (researchAttr != null)
                    {
                        efficiency += AttributeLevelHelper.GetEfficiencyMultiplier(researchAttr.Value) - 1.0f;
                    }
                }
            }
            
            // 建筑等级加成
            var buildingSystem = this.GetSystem<EnhancedBuildingSystem>();
            var building = buildingSystem.GetBuilding(task.BuildingId);
            if (building != null)
            {
                var buildingConfig = _configSystem.GetBuildingConfig(building.ConfigId);
                if (buildingConfig != null)
                {
                    efficiency += (int)buildingConfig.Level * 0.1f; // 每级增加10%效率
                }
            }
            
            return Mathf.Max(0.1f, efficiency); // 最低10%效率
        }
        
        /// <summary>
        /// 更新研究进度
        /// </summary>
        private void UpdateResearch(float deltaTime)
        {
            var completedTasks = new List<ResearchTask>();
            
            foreach (var task in _activeResearchTasks)
            {
                if (task.IsPaused) continue;
                
                var techConfig = _configSystem.GetTechConfig(task.TechId);
                if (techConfig == null) continue;
                
                var techData = _techTreeData.Technologies[task.TechId];
                
                // 更新进度
                float progressDelta = (deltaTime / (techConfig.ResearchTime * 3600f)) * task.EfficiencyMultiplier;
                techData.ResearchProgress += progressDelta;
                
                // 检查是否完成
                if (techData.ResearchProgress >= 1.0f)
                {
                    // 检查失败率
                    if (UnityEngine.Random.Range(0f, 1f) < techConfig.FailureRate)
                    {
                        // 研究失败
                        HandleResearchFailure(task, techConfig, techData);
                    }
                    else
                    {
                        // 研究成功
                        CompleteResearch(task, techConfig, techData);
                    }
                    
                    completedTasks.Add(task);
                }
            }
            
            // 移除已完成的任务
            foreach (var task in completedTasks)
            {
                _activeResearchTasks.Remove(task);
            }
        }
        
        /// <summary>
        /// 处理研究失败
        /// </summary>
        private void HandleResearchFailure(ResearchTask task, TechConfig techConfig, TechData techData)
        {
            techData.IsResearching = false;
            techData.ResearchProgress = 0f;
            
            Debug.LogWarning($"科技 {techConfig.Name} 研究失败");
            
            // 给研究员经验（失败也有经验）
            var survivorSystem = this.GetSystem<SurvivorSystem>();
            foreach (var researcherId in task.ResearcherIds)
            {
                survivorSystem.AddExperienceToSurvivor(researcherId, 5);
            }
            
            // 一段时间后可以重新研究（简化实现）
            // 注意：这里移除了对ITimeSystem的依赖，因为该接口可能不存在
            Debug.Log($"科技 {techConfig.Name} 研究失败，可以重新研究");
        }
        
        /// <summary>
        /// 完成研究
        /// </summary>
        private void CompleteResearch(ResearchTask task, TechConfig techConfig, TechData techData)
        {
            techData.IsResearched = true;
            techData.IsResearching = false;
            techData.ResearchProgress = 1.0f;
            
            Debug.Log($"科技 {techConfig.Name} 研究完成！");
            
            // 应用科技效果
            ApplyTechEffects(techConfig);
            
            // 给研究员经验
            var survivorSystem = this.GetSystem<SurvivorSystem>();
            foreach (var researcherId in task.ResearcherIds)
            {
                survivorSystem.AddExperienceToSurvivor(researcherId, 20);
            }
            
            // 更新科技可用性
            UpdateTechAvailability();
            
            // 发送科技完成事件
            this.SendEvent(new TechCompletedEvent
            {
                TechId = techConfig.Id,
                TechName = techConfig.Name
            });
        }
        
        /// <summary>
        /// 应用科技效果
        /// </summary>
        private void ApplyTechEffects(TechConfig techConfig)
        {
            foreach (var effect in techConfig.Effects)
            {
                // 使用简化的效果系统，直接基于科技描述
                var effectType = ParseEffectType(techConfig.Description);
                var target = techConfig.Id; // 使用科技ID作为目标
                var value = 0.2f; // 默认20%加成
                
                if (!_techEffects.ContainsKey(effectType))
                {
                    _techEffects[effectType] = new Dictionary<string, float>();
                }
                
                if (!_techEffects[effectType].ContainsKey(target))
                {
                    _techEffects[effectType][target] = 0f;
                }
                
                _techEffects[effectType][target] += value;
                
                Debug.Log($"应用科技效果: {effectType} -> {target} +{value}");
            }
        }
        
        /// <summary>
        /// 解析效果类型
        /// </summary>
        private TechEffectType ParseEffectType(string description)
        {
            if (description.Contains("生产") || description.Contains("产量"))
                return TechEffectType.ProductionBonus;
            if (description.Contains("效率"))
                return TechEffectType.EfficiencyBonus;
            if (description.Contains("防御"))
                return TechEffectType.DefenseBonus;
            if (description.Contains("研究"))
                return TechEffectType.ResearchSpeedBonus;
            
            return TechEffectType.ProductionBonus; // 默认
        }
        
        #endregion
        
        #region 科技状态管理
        
        /// <summary>
        /// 更新科技可用性
        /// </summary>
        private void UpdateTechAvailability()
        {
            var completedTechs = GetCompletedTechs();
            
            foreach (var techData in _techTreeData.Technologies.Values)
            {
                if (techData.IsResearched || techData.IsResearching)
                    continue;
                
                var techConfig = _configSystem.GetTechConfig(techData.Id);
                if (techConfig == null) continue;
                
                // 检查前置条件
                bool prerequisitesMet = _configSystem.AreTechPrerequisitesMet(techData.Id, completedTechs);
                
                // 这里可以添加更多的可用性逻辑
                Debug.Log($"科技 {techConfig.Name} 前置条件满足: {prerequisitesMet}");
            }
        }
        
        /// <summary>
        /// 获取已完成的科技列表
        /// </summary>
        public List<string> GetCompletedTechs()
        {
            return _techTreeData.Technologies.Values
                .Where(tech => tech.IsResearched)
                .Select(tech => tech.Id)
                .ToList();
        }
        
        /// <summary>
        /// 检查科技是否已完成
        /// </summary>
        public bool IsTechCompleted(string techId)
        {
            return _techTreeData.Technologies.TryGetValue(techId, out var techData) && 
                   techData.IsResearched;
        }
        
        /// <summary>
        /// 获取科技状态
        /// </summary>
        public TechState GetTechState(string techId)
        {
            if (!_techTreeData.Technologies.TryGetValue(techId, out var techData))
                return TechState.Locked;
            
            if (techData.IsResearched)
                return TechState.Completed;
            if (techData.IsResearching)
                return TechState.Researching;
            
            // 检查是否可研究
            var completedTechs = GetCompletedTechs();
            bool prerequisitesMet = _configSystem.AreTechPrerequisitesMet(techId, completedTechs);
            
            return prerequisitesMet ? TechState.Available : TechState.Locked;
        }
        
        #endregion
        
        #region 科研点数管理
        
        /// <summary>
        /// 添加科研点数
        /// </summary>
        public void AddResearchPoints(int points)
        {
            _techTreeData.TotalResearchPoints += points;
            Debug.Log($"获得科研点数 +{points}，总计: {_techTreeData.TotalResearchPoints}");
        }
        
        /// <summary>
        /// 获取可用科研点数
        /// </summary>
        public int GetAvailableResearchPoints()
        {
            return _techTreeData.TotalResearchPoints - _techTreeData.SpentResearchPoints;
        }
        
        /// <summary>
        /// 获取总科研点数
        /// </summary>
        public int GetTotalResearchPoints()
        {
            return _techTreeData.TotalResearchPoints;
        }
        
        #endregion
        
        #region 科技效果查询
        
        /// <summary>
        /// 获取科技效果值
        /// </summary>
        public float GetTechEffectValue(TechEffectType effectType, string target)
        {
            if (_techEffects.TryGetValue(effectType, out var effects))
            {
                return effects.TryGetValue(target, out var value) ? value : 0f;
            }
            return 0f;
        }
        
        /// <summary>
        /// 获取科技效果值（字符串版本，兼容旧接口）
        /// </summary>
        public float GetTechEffectValue(string effectName, float defaultValue)
        {
            // 简化实现，根据效果名称返回对应的加成
            switch (effectName)
            {
                case "production_efficiency":
                    return GetTechEffectValue(TechEffectType.ProductionBonus, "global");
                case "defense_efficiency":
                    return GetTechEffectValue(TechEffectType.DefenseBonus, "global");
                case "research_speed_bonus":
                    return GetTechEffectValue(TechEffectType.ResearchSpeedBonus, "global");
                default:
                    return defaultValue;
            }
        }
        
        /// <summary>
        /// 获取生产加成
        /// </summary>
        public float GetProductionBonus(string buildingId)
        {
            return GetTechEffectValue(TechEffectType.ProductionBonus, buildingId);
        }
        
        /// <summary>
        /// 获取效率加成
        /// </summary>
        public float GetEfficiencyBonus(string target)
        {
            return GetTechEffectValue(TechEffectType.EfficiencyBonus, target);
        }
        
        #endregion
        
        #region 事件处理
        
        private void OnGameUpdate(GameUpdateEvent e)
        {
            UpdateResearch(e.DeltaTime);
        }
        
        #endregion
        
        #region 公共接口
        
        /// <summary>
        /// 获取科技树数据
        /// </summary>
        public TechTreeData GetTechTreeData()
        {
            return _techTreeData;
        }
        
        /// <summary>
        /// 获取指定科技树的科技列表
        /// </summary>
        public List<string> GetTechsByTree(TechTree tree)
        {
            return _techTreeData.TreeTechs.TryGetValue(tree, out var techs) 
                ? new List<string>(techs) 
                : new List<string>();
        }
        
        /// <summary>
        /// 获取当前研究任务
        /// </summary>
        public List<ResearchTask> GetActiveResearchTasks()
        {
            return new List<ResearchTask>(_activeResearchTasks);
        }
        
        /// <summary>
        /// 暂停/恢复研究
        /// </summary>
        public bool ToggleResearchPause(string techId)
        {
            var task = _activeResearchTasks.FirstOrDefault(t => t.TechId == techId);
            if (task != null)
            {
                task.IsPaused = !task.IsPaused;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 取消研究
        /// </summary>
        public bool CancelResearch(string techId)
        {
            var task = _activeResearchTasks.FirstOrDefault(t => t.TechId == techId);
            if (task == null) return false;
            
            _activeResearchTasks.Remove(task);
            
            if (_techTreeData.Technologies.TryGetValue(techId, out var techData))
            {
                techData.IsResearching = false;
                techData.ResearchProgress = 0f;
            }
            
            // 返还部分科研点数
            var techConfig = _configSystem.GetTechConfig(techId);
            if (techConfig != null)
            {
                int refund = Mathf.FloorToInt(techConfig.ResearchPointsCost * 0.5f);
                _techTreeData.SpentResearchPoints -= refund;
                Debug.Log($"取消研究 {techConfig.Name}，返还科研点数: {refund}");
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查科技是否已研究（兼容接口）
        /// </summary>
        public bool IsTechResearched(string techId)
        {
            return IsTechCompleted(techId);
        }
        
        #endregion
    }
    
    #region 事件定义
    
    public struct TechCompletedEvent
    {
        public string TechId;
        public string TechName;
    }
    
    // public struct GameUpdateEvent
    // {
    //     public float DeltaTime;
    // }
    
    #endregion
    
    #region 辅助类
    
    /// <summary>
    /// 属性等级助手类
    /// </summary>
    public static class AttributeLevelHelper
    {
        public static float GetEfficiencyMultiplier(int attributeValue)
        {
            // 根据属性值计算效率倍数
            if (attributeValue >= 80) return 2.0f;      // 大师级
            if (attributeValue >= 60) return 1.6f;      // 专家级
            if (attributeValue >= 40) return 1.3f;      // 专业级
            if (attributeValue >= 20) return 1.1f;      // 熟练级
            return 1.0f;                                 // 新手级
        }
    }
    
    #endregion
} 