// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：TechSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了科技系统 (TechSystem)，负责管理游戏中的科技研发流程、
//     科技树的维护、科技效果的应用以及科研点数的管理。该系统与其他游戏系统
//     （如配置系统、资源系统、幸存者系统、建筑系统）交互，以实现完整的科技研发功能。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq; // 用于LINQ查询，例如在GetCompletedTechs等方法中
using UnityEngine; // 用于UnityEngine功能，如Time.time, Mathf, Debug.Log等
using QFramework;    // QFramework框架的相关引用
using SurvivalGame.Model; // 游戏核心数据模型的引用

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 科技系统 (TechSystem) - 管理科技研究、科技树进度及科技效果。
    /// 该系统不直接实现QFISystem接口，而是继承自AbstractSystem，后者通常已实现QFISystem。
    /// </summary>
    public class TechSystem : AbstractSystem // 注意：此类未直接声明实现ITechSystem接口，但其公共方法构成了事实上的API
    {
        #region 数据存储 (Data Storage)
        
        /// <summary>
        /// 存储整个科技树的数据，包括所有科技的状态和玩家的科研点数信息。
        /// </summary>
        private TechTreeData _techTreeData;
        /// <summary>
        /// 当前正在进行的所有研究任务列表。
        /// </summary>
        private List<ResearchTask> _activeResearchTasks;
        /// <summary>
        /// 缓存已激活的科技效果及其累积数值。
        /// 键为科技效果类型 (TechEffectType)，值为一个字典，该字典的键为效果目标标识符 (通常是科技ID或"global")，值为效果数值。
        /// </summary>
        private Dictionary<TechEffectType, Dictionary<string, float>> _techEffects; // 科技效果缓存
        /// <summary>
        /// 生存游戏核心数据模型的引用，用于访问全局游戏状态。
        /// </summary>
        private ISurvivalGameModel _gameModel;
        /// <summary>
        /// 配置系统的引用，用于获取科技、建筑等配置信息。
        /// </summary>
        private ConfigSystem _configSystem;
        
        #endregion
        
        #region 系统初始化 (System Initialization)
        
        /// <summary>
        /// 系统初始化方法。
        /// 此方法在系统启动时调用，用于获取其他系统引用，初始化科技树数据结构，
        /// 加载科技配置，并注册相关的游戏事件监听器。
        /// </summary>
        protected override void OnInit()
        {
            // 获取对其他核心系统和数据模型的引用
            _gameModel = this.GetModel<ISurvivalGameModel>();
            _configSystem = this.GetSystem<ConfigSystem>();
            
            // 初始化用于存储科技系统内部状态的数据结构
            _techTreeData = new TechTreeData(); // 科技树整体数据
            _activeResearchTasks = new List<ResearchTask>(); // 当前研究任务列表
            _techEffects = new Dictionary<TechEffectType, Dictionary<string, float>>(); // 已激活科技效果缓存
            
            InitializeTechTree(); // 加载并初始化科技树配置
            
            // 注册监听游戏更新事件，以驱动研究进度等逻辑
            this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);
            Debug.Log("[科技系统] 初始化完成。");
        }
        
        /// <summary>
        /// 初始化科技树结构。
        /// 从配置系统中加载所有科技配置，并为每个配置创建相应的科技数据实例，存入科技树。
        /// 同时根据科技树分支对科技进行分类。
        /// </summary>
        private void InitializeTechTree()
        {
            var allTechConfigs = _configSystem.GetAllTechConfigs(); // 从配置系统获取所有科技的配置信息
            
            foreach (var techConfig in allTechConfigs.Values) // 遍历每个科技配置
            {
                // 为每个配置创建一个TechData实例，用于追踪该科技的动态状态（如是否已研究、进度等）
                var techData = new TechData
                {
                    Id = techConfig.Id,                     // 科技ID
                    Name = techConfig.Name,                   // 科技名称
                    Description = techConfig.Description,     // 科技描述
                    IsResearched = false,                   // 初始状态：未研究
                    IsResearching = false,                  // 初始状态：未在研究中
                    ResearchProgress = 0f,                  // 初始研究进度为0
                    Category = techConfig.Category,         // 科技所属类别 (从配置读取)
                    Tier = (TechTier)techConfig.Tier        // 科技所属层级 (从配置读取并转换)
                };
                
                _techTreeData.Technologies[techConfig.Id] = techData; // 将科技数据存入科技树字典
                
                // 将科技ID按其所属的科技树分支进行分类存储
                if (!_techTreeData.TreeTechs.ContainsKey(techConfig.Tree)) // 如果该科技树分支的列表还未创建
                {
                    _techTreeData.TreeTechs[techConfig.Tree] = new List<string>(); // 则创建一个新列表
                }
                if (!_techTreeData.TreeTechs[techConfig.Tree].Contains(techConfig.Id)) // 避免重复添加
                {
                    _techTreeData.TreeTechs[techConfig.Tree].Add(techConfig.Id);
                }
            }
            
            UpdateTechAvailability(); // 初始化后，更新一次所有科技的可用状态（例如，解锁所有无前置的基础科技）
            
            Debug.Log($"[科技系统] 科技树初始化完成，共载入 {_techTreeData.Technologies.Count} 项科技。");
        }
        
        #endregion
        
        #region 科技研究 (Technology Research)
        
        /// <summary>
        /// 开始一项新的科技研究。
        /// </summary>
        /// <param name="techId">要研究的科技ID。</param>
        /// <param name="researchBuildingId">执行研究的建筑ID。</param>
        /// <param name="researcherIds">（可选）参与研究的幸存者ID列表。</param>
        /// <returns>如果成功开始研究则返回true，否则返回false（例如，条件不满足、资源不足等）。</returns>
        public bool StartResearch(string techId, string researchBuildingId, List<string> researcherIds = null)
        {
            var techConfig = _configSystem.GetTechConfig(techId); // 获取科技配置
            
            // 1. 检查科技配置是否存在
            if (techConfig == null)
            {
                Debug.LogError($"[科技系统] 开始研究失败：找不到科技配置ID {techId}。");
                return false;
            }
            
            // 2. 检查科技数据是否存在于科技树中
            if (!_techTreeData.Technologies.TryGetValue(techId, out var techData))
            {
                Debug.LogError($"[科技系统] 开始研究失败：科技数据中不存在ID {techId}。");
                return false;
            }
            
            // 3. 检查科技是否已研究完成或已在研究队列中
            if (techData.IsResearched || techData.IsResearching)
            {
                Debug.LogWarning($"[科技系统] 科技 {techConfig.Name} (ID: {techId}) 已完成或正在研究中，无法重复开始。");
                return false;
            }
            // 检查是否已在活动研究任务列表中 (更精确的“正在研究中”判断)
            if (_activeResearchTasks.Any(task => task.TechId == techId))
            {
                Debug.LogWarning($"[科技系统] 科技 {techConfig.Name} (ID: {techId}) 已存在于当前研究任务列表中。");
                return false;
            }
            
            // 4. 检查科研点数是否足够
            if (GetAvailableResearchPoints() < techConfig.ResearchPointsCost) // 使用GetAvailableResearchPoints更准确
            {
                Debug.LogWarning($"[科技系统] 科研点数不足以开始研究 {techConfig.Name}。需要 {techConfig.ResearchPointsCost}，当前可用 {GetAvailableResearchPoints()}。");
                return false;
            }
            
            // 5. 检查额外的资源消耗是否满足
            if (techConfig.AdditionalCosts != null && !CanAffordResources(techConfig.AdditionalCosts))
            {
                Debug.LogWarning($"[科技系统] 启动研究 {techConfig.Name} 所需的额外资源不足。");
                return false;
            }
            
            // 6. 检查执行研究的建筑是否存在且可运作
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>(); // 使用接口类型
            var researchBuilding = buildingSystem.GetBuilding(researchBuildingId);
            if (researchBuilding == null || researchBuilding.State != BuildingState.Operational)
            {
                Debug.LogWarning($"[科技系统] 开始研究 {techConfig.Name} 失败：需要一个运行中的研究设施 (指定建筑ID: {researchBuildingId} 无效或未运作)。");
                return false;
            }
            // TODO: 此处还应检查研究建筑是否满足研究此科技的等级要求 (techConfig.MinResearchLevel)
            
            // 7. 消耗科研点数和额外资源
            _techTreeData.SpentResearchPoints += techConfig.ResearchPointsCost; // 记录已花费的科研点
            if (techConfig.AdditionalCosts != null)
            {
                ConsumeResources(techConfig.AdditionalCosts); // 消耗其他资源
            }
            
            // 8. 创建新的研究任务
            var researchTask = new ResearchTask(techId, researchBuildingId) // 创建任务实例
            {
                ResearcherIds = researcherIds ?? new List<string>() // 设置研究员列表
            };
            
            // 9. 计算并设置研究效率和预计完成时间
            researchTask.EfficiencyMultiplier = CalculateResearchEfficiency(researchTask, techConfig);
            float totalBaseTimeSeconds = techConfig.ResearchTime * 3600f; // 将小时转换为秒
            researchTask.EstimatedEndTime = Time.time + (totalBaseTimeSeconds / Mathf.Max(0.001f, researchTask.EfficiencyMultiplier)); // 防止除以零
            
            _activeResearchTasks.Add(researchTask); // 将任务添加到活动列表
            techData.IsResearching = true;          // 更新科技数据状态为“研究中”
            techData.ResearchStartTime = Time.time; // 记录研究开始时间
            
            Debug.Log($"[科技系统] 开始研究科技: {techConfig.Name} (ID: {techId})。预计耗时: {(researchTask.EstimatedEndTime - Time.time):F1} 秒 (效率: {researchTask.EfficiencyMultiplier:P0})。");
            // TODO: 发送科技开始研究事件
            return true;
        }
        
        /// <summary>
        /// （辅助方法）检查当前是否拥有足够的资源来支付指定的成本列表。
        /// </summary>
        /// <param name="costs">资源成本列表。</param>
        /// <returns>如果所有资源都足够则返回true，否则false。</returns>
        private bool CanAffordResources(List<ResourceCost> costs)
        {
            foreach (var cost in costs) // 遍历每项成本
            {
                if (_gameModel.GetResourceAmount(cost.Type) < cost.Amount) // 检查资源存量
                {
                    return false; // 任一资源不足则判定为无法承担
                }
            }
            return true; // 所有资源均充足
        }
        
        /// <summary>
        /// （辅助方法）消耗指定的资源列表。
        /// </summary>
        /// <param name="costs">要消耗的资源列表。</param>
        private void ConsumeResources(List<ResourceCost> costs)
        {
            foreach (var cost in costs) // 遍历并消耗每项资源
            {
                _gameModel.ConsumeResource(cost.Type, cost.Amount);
            }
        }
        
        /// <summary>
        /// （辅助方法）计算指定研究任务的综合研究效率。
        /// 效率受全局加成、参与研究员的属性、研究建筑等级等多种因素影响。
        /// </summary>
        /// <param name="task">要计算效率的研究任务。</param>
        /// <param name="techConfig">该任务对应的科技配置。</param>
        /// <returns>综合效率乘数（例如1.0为标准效率）。</returns>
        private float CalculateResearchEfficiency(ResearchTask task, TechConfig techConfig)
        {
            float totalEfficiency = _techTreeData.GlobalResearchBonus; // 从全局科研加成开始（默认为1.0f）
            
            // 1. 研究员属性加成
            var survivorSystem = this.GetSystem<ISurvivorSystem>(); // 获取幸存者系统
            if (task.ResearcherIds != null)
            {
                foreach (var researcherId in task.ResearcherIds) // 遍历所有参与的研究员
                {
                    var researcher = survivorSystem.GetSurvivor(researcherId); // 获取研究员数据
                    if (researcher != null)
                    {
                        // 获取研究员的研究属性值
                        var researchAttribute = researcher.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Research);
                        if (researchAttribute != null)
                        {
                            // AttributeLevelHelper.GetEfficiencyMultiplier 将属性值转换为效率乘数 (例如1.0基础，每级+0.1)
                            // 此处假设加成是累加的，例如多个研究员的加成效果可以叠加一部分
                            totalEfficiency += (AttributeLevelHelper.GetEfficiencyMultiplier(researchAttribute.Value) - 1.0f); // 减去1.0f是因为基础效率已包含
                        }
                    }
                }
            }
            
            // 2. 研究建筑等级加成
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>(); // 获取建筑系统
            var researchBuilding = buildingSystem.GetBuilding(task.BuildingId); // 获取研究建筑数据
            if (researchBuilding != null)
            {
                var researchBuildingConfig = _configSystem.GetBuildingConfig(researchBuilding.ConfigId); // 获取建筑配置
                if (researchBuildingConfig != null)
                {
                    // 示例：建筑每高一级，研究效率额外增加10%
                    totalEfficiency += ((int)researchBuildingConfig.Level - 1) * 0.1f;
                }
            }
            
            // TODO: 此处还可以加入其他影响研究效率的因素，如特定科技的加成、临时Buff/Debuff等

            return Mathf.Max(0.1f, totalEfficiency); // 确保效率至少为10%，防止出现0或负效率导致研究无法进行
        }
        
        /// <summary>
        /// 每帧更新所有活动研究任务的进度。
        /// </summary>
        /// <param name="deltaTime">自上一帧以来经过的时间（秒）。</param>
        private void UpdateResearch(float deltaTime)
        {
            var completedTasks = new List<ResearchTask>(); // 用于存储本轮完成的研究任务
            
            foreach (var task in _activeResearchTasks) // 遍历所有当前激活的研究任务
            {
                if (task.IsPaused) continue; // 如果任务已暂停，则跳过
                
                var techConfig = _configSystem.GetTechConfig(task.TechId); // 获取科技配置
                if (techConfig == null) // 安全检查，如果找不到配置则跳过此任务
                {
                    Debug.LogError($"[科技系统] 更新研究失败：找不到科技配置ID {task.TechId}。");
                    completedTasks.Add(task); // 将其视为异常完成，以便从队列移除
                    continue;
                }
                
                if (!_techTreeData.Technologies.TryGetValue(task.TechId, out var techData)) // 获取科技动态数据
                {
                     Debug.LogError($"[科技系统] 更新研究失败：科技动态数据中不存在ID {task.TechId}。");
                    completedTasks.Add(task);
                    continue;
                }
                
                // 计算此时间间隔内应增加的研究进度
                // 进度增加 = (实际经过时间 * 研究效率) / (科技所需总基础时间秒数)
                float progressIncrement = (deltaTime / (techConfig.ResearchTime * 3600f)) * task.EfficiencyMultiplier;
                techData.ResearchProgress += progressIncrement; // 累加研究进度
                
                // 检查科技是否已研究完成
                if (techData.ResearchProgress >= 1.0f)
                {
                    // 检查是否有研究失败的可能性
                    if (UnityEngine.Random.Range(0f, 1f) < techConfig.FailureRate) // FailureRate应为0到1之间的小数
                    {
                        HandleResearchFailure(task, techConfig, techData); // 处理研究失败逻辑
                    }
                    else
                    {
                        CompleteResearch(task, techConfig, techData); // 处理研究成功逻辑
                    }
                    completedTasks.Add(task); // 无论成功或失败，任务都已结束，添加到完成列表
                }
            }
            
            // 从活动任务列表中移除所有已在本轮完成或失败的任务
            foreach (var task in completedTasks)
            {
                _activeResearchTasks.Remove(task);
            }
        }
        
        /// <summary>
        /// （辅助方法）处理科技研究失败的情况。
        /// </summary>
        /// <param name="task">失败的研究任务。</param>
        /// <param name="techConfig">对应的科技配置。</param>
        /// <param name="techData">对应的科技动态数据。</param>
        private void HandleResearchFailure(ResearchTask task, TechConfig techConfig, TechData techData)
        {
            techData.IsResearching = false; // 标记为未在研究中
            techData.ResearchProgress = 0f;   // 重置研究进度 (或根据设计保留部分进度/返还部分资源)
            
            Debug.LogWarning($"[科技系统] 科技 {techConfig.Name} (ID: {techConfig.Id}) 研究失败！");
            
            // （可选）给予参与研究的幸存者少量经验（即使失败也有所学习）
            var survivorSystem = this.GetSystem<ISurvivorSystem>();
            if (task.ResearcherIds != null)
            {
                foreach (var researcherId in task.ResearcherIds)
                {
                    survivorSystem.AddExperienceToSurvivor(researcherId, 5); // 示例：给予5点经验
                }
            }
            
            // TODO: 根据游戏设计，研究失败可能会有其他后果，如士气下降、资源部分损失等
            // 此处简化为可以直接重新尝试研究 (状态已重置)
            Debug.Log($"[科技系统] 科技 {techConfig.Name} 研究失败，可以重新开始研究。");
            // 发送研究失败事件，UI可以处理此事件
            // this.SendEvent(new TechResearchFailedEvent { TechId = techConfig.Id, TechName = techConfig.Name });
        }
        
        /// <summary>
        /// （辅助方法）处理科技研究成功完成的逻辑。
        /// </summary>
        /// <param name="task">完成的研究任务。</param>
        /// <param name="techConfig">对应的科技配置。</param>
        /// <param name="techData">对应的科技动态数据。</param>
        private void CompleteResearch(ResearchTask task, TechConfig techConfig, TechData techData)
        {
            techData.IsResearched = true;     // 标记为已研究
            techData.IsResearching = false;   // 标记为未在研究中
            techData.ResearchProgress = 1.0f; // 研究进度设置为100%
            
            Debug.Log($"[科技系统] 科技 {techConfig.Name} (ID: {techConfig.Id}) 研究完成！");
            
            ApplyTechEffects(techConfig); // 应用此科技带来的所有效果
            
            // 给予参与研究的幸存者经验奖励
            var survivorSystem = this.GetSystem<ISurvivorSystem>();
            if (task.ResearcherIds != null)
            {
                foreach (var researcherId in task.ResearcherIds)
                {
                    survivorSystem.AddExperienceToSurvivor(researcherId, 20); // 示例：给予20点经验
                }
            }
            
            UpdateTechAvailability(); // 研究完成后，更新其他科技的可用状态（可能解锁了新的科技）
            
            // 发送科技研发完成事件，通知其他系统（如UI、建筑系统等）
            this.SendEvent(new TechCompletedEvent
            {
                TechId = techConfig.Id,
                TechName = techConfig.Name
            });
        }
        
        /// <summary>
        /// （辅助方法）应用指定科技配置中定义的所有效果。
        /// </summary>
        /// <param name="techConfig">已完成研究的科技的配置信息。</param>
        private void ApplyTechEffects(TechConfig techConfig)
        {
            if (techConfig.Effects == null) return; // 如果没有定义效果，则直接返回

            foreach (var effectData in techConfig.Effects) // 遍历配置中的每个效果
            {
                // TechEffectType effectType = effectData.Type; // 假设 TechEffect 有 Type 字段
                // string targetId = effectData.Target ?? "global"; // 效果目标，默认为全局
                // float value = effectData.Value; // 效果数值
                
                // --- 以下为基于旧版ParseEffectType的简化逻辑，实际应使用TechEffect结构中的明确数据 ---
                // 这是一个临时的简化版效果应用逻辑，它尝试从科技描述中解析效果类型。
                // 在一个更完善的系统中，TechEffect结构体应包含明确的EffectType, Target, Value等字段。
                var parsedEffectType = ParseEffectType(techConfig.Description); // 从描述解析类型 (非常粗略)
                var effectTarget = techConfig.Id; // 简化：效果目标暂用科技ID自身作为标识
                var effectValue = 0.2f; // 简化：效果值固定为0.2 (20%加成)
                                     // 实际应从 effectData.Value 读取
                // --- 简化逻辑结束 ---

                // 更新科技效果缓存
                if (!_techEffects.ContainsKey(parsedEffectType)) // 如果缓存中还没有此效果类型
                {
                    _techEffects[parsedEffectType] = new Dictionary<string, float>(); // 则创建新的字典项
                }
                
                if (!_techEffects[parsedEffectType].ContainsKey(effectTarget)) // 如果此效果类型下还没有此目标的记录
                {
                    _techEffects[parsedEffectType][effectTarget] = 0f; // 则初始化为0
                }
                
                _techEffects[parsedEffectType][effectTarget] += effectValue; // 累加效果值
                
                Debug.Log($"[科技系统] 已应用科技效果: {parsedEffectType} -> 目标 {effectTarget} (增加 {effectValue})。当前总值: {_techEffects[parsedEffectType][effectTarget]}");
                // TODO: 发送科技效果应用的事件，以便其他系统（如属性、生产）可以响应这些变化
                // this.SendEvent(new TechEffectAppliedEvent { EffectType = parsedEffectType, Target = effectTarget, Value = effectValue });
            }
        }
        
        /// <summary>
        /// （辅助方法，极简陋）根据科技描述文本尝试解析出科技效果类型。
        /// **注意：这是一个非常不可靠的实现，实际项目中应使用结构化的效果数据。**
        /// </summary>
        /// <param name="description">科技的描述文本。</param>
        /// <returns>解析出的TechEffectType，如果无法判断则返回默认值。</returns>
        private TechEffectType ParseEffectType(string description)
        {
            if (string.IsNullOrEmpty(description)) return TechEffectType.ProductionBonus; // 描述为空则返回默认

            if (description.Contains("生产") || description.Contains("产量")) // 如果描述中包含“生产”或“产量”
                return TechEffectType.ProductionBonus; // 认为是生产加成
            if (description.Contains("效率")) // 如果包含“效率”
                return TechEffectType.EfficiencyBonus; // 认为是效率加成
            if (description.Contains("防御")) // 如果包含“防御”
                return TechEffectType.DefenseBonus; // 认为是防御加成
            if (description.Contains("研究")) // 如果包含“研究”
                return TechEffectType.ResearchSpeedBonus; // 认为是研究速度加成

            return TechEffectType.ProductionBonus; // 默认返回生产加成
        }
        
        #endregion
        
        #region 科技状态管理 (Technology State Management)
        
        /// <summary>
        /// 更新科技树中所有科技的可用性状态。
        /// 当一项科技研究完成后，或游戏条件变化时（如天数），调用此方法。
        /// </summary>
        private void UpdateTechAvailability()
        {
            var completedTechIds = GetCompletedTechs(); // 获取所有已研究完成的科技ID列表
            
            foreach (var techData in _techTreeData.Technologies.Values) // 遍历科技树中的每一项科技数据
            {
                // 跳过已经研究完成或正在研究中的科技 (它们的状态是确定的)
                if (techData.IsResearched || techData.IsResearching)
                    continue;
                
                var techConfig = _configSystem.GetTechConfig(techData.Id); // 获取该科技的配置信息
                if (techConfig == null) continue; // 如果配置不存在，则跳过
                
                // 检查该科技的所有前置科技是否都已完成
                bool prerequisitesMet = _configSystem.AreTechPrerequisitesMet(techData.Id, completedTechIds);
                
                // 根据前置条件满足情况更新科技状态 (此处简化，实际可能还有其他条件，如资源、建筑等)
                // TechData本身没有直接的 "IsAvailable" 字段，其可用性由GetTechState动态判断。
                // 此处可以打印日志，或如果TechData有IsAvailable字段则更新它。
                if (prerequisitesMet)
                {
                     // techData.Status = TechStatus.Available; // 如果TechData有Status字段
                    Debug.Log($"[科技系统] 科技 {techConfig.Name} (ID: {techData.Id}) 的前置条件已满足，变为可研究。");
                }
                else
                {
                    // techData.Status = TechStatus.Locked; // 如果TechData有Status字段
                    // Debug.Log($"[科技系统] 科技 {techConfig.Name} (ID: {techData.Id}) 的前置条件未满足，保持锁定。");
                }
            }
        }
        
        /// <summary>
        /// 获取所有已研究完成的科技的ID列表。
        /// </summary>
        /// <returns>已完成科技ID的列表。</returns>
        public List<string> GetCompletedTechs()
        {
            return _techTreeData.Technologies.Values // 获取所有科技数据
                .Where(tech => tech.IsResearched)    // 筛选出已研究的
                .Select(tech => tech.Id)            // 选择其ID
                .ToList();                          //转换为列表
        }
        
        /// <summary>
        /// 检查指定ID的科技是否已经研究完成。
        /// </summary>
        /// <param name="techId">要检查的科技ID。</param>
        /// <returns>如果已研究则返回true，否则false。</returns>
        public bool IsTechCompleted(string techId)
        {
            // 尝试从科技树数据中获取该科技的状态，如果存在且IsResearched为true，则返回true
            return _techTreeData.Technologies.TryGetValue(techId, out var techData) && 
                   techData.IsResearched;
        }
        
        /// <summary>
        /// 获取指定ID科技的当前状态（锁定、可研究、研究中、已完成）。
        /// </summary>
        /// <param name="techId">科技ID。</param>
        /// <returns>科技的当前TechState。</returns>
        public TechState GetTechState(string techId)
        {
            if (!_techTreeData.Technologies.TryGetValue(techId, out var techData)) // 科技数据不存在
                return TechState.Locked; // 默认为锁定状态
            
            if (techData.IsResearched) return TechState.Completed;   // 已研究完成
            if (techData.IsResearching) return TechState.Researching; // 正在研究中
            
            // 如果未研究也未在研究中，则检查其前置条件是否满足以判断是否可研究
            var completedTechs = GetCompletedTechs(); // 获取所有已完成的科技
            bool prerequisitesMet = _configSystem.AreTechPrerequisitesMet(techId, completedTechs); // 检查前置
            
            return prerequisitesMet ? TechState.Available : TechState.Locked; // 前置满足则可研究，否则锁定
        }
        
        #endregion
        
        #region 科研点数管理 (Research Points Management)
        
        /// <summary>
        /// 向系统中添加指定数量的科研点数。
        /// </summary>
        /// <param name="points">要添加的科研点数量 (应为正数)。</param>
        public void AddResearchPoints(int points)
        {
            if (points <= 0) return; // 不添加非正数点数
            _techTreeData.TotalResearchPoints += points;
            Debug.Log($"[科技系统] 获得科研点数 +{points}。当前总科研点数: {_techTreeData.TotalResearchPoints}。");
            // TODO: 发送科研点数变化事件
            // this.SendEvent(new ResearchPointsChangedEvent { NewTotal = _techTreeData.TotalResearchPoints });
        }
        
        /// <summary>
        /// 获取当前可用于研究的科研点数（总点数 - 已花费点数）。
        /// </summary>
        /// <returns>可用的科研点数。</returns>
        public int GetAvailableResearchPoints()
        {
            return _techTreeData.TotalResearchPoints - _techTreeData.SpentResearchPoints;
        }
        
        /// <summary>
        /// 获取玩家已获得过的科研点数总量。
        /// </summary>
        /// <returns>总科研点数。</returns>
        public int GetTotalResearchPoints()
        {
            return _techTreeData.TotalResearchPoints;
        }
        
        #endregion
        
        #region 科技效果查询 (Technology Effect Queries)
        
        /// <summary>
        /// 获取特定科技效果类型在特定目标上的累积效果值。
        /// </summary>
        /// <param name="effectType">要查询的科技效果类型。</param>
        /// <param name="target">效果作用的目标标识符（例如 "global", 建筑ID, 单位类型ID等）。</param>
        /// <returns>该效果的累积数值；如果不存在则返回0。</returns>
        public float GetTechEffectValue(TechEffectType effectType, string target)
        {
            // 尝试从缓存的科技效果字典中获取值
            if (_techEffects.TryGetValue(effectType, out var effectsOnTargetType))
            {
                return effectsOnTargetType.TryGetValue(target, out var value) ? value : 0f; // 如果目标存在则返回值，否则0
            }
            return 0f; // 如果效果类型不存在，则返回0
        }
        
        /// <summary>
        /// （兼容旧接口或特定需求）根据效果名称字符串获取科技效果值。
        /// 此方法内部将效果名称映射到具体的TechEffectType和目标。
        /// </summary>
        /// <param name="effectName">效果的名称字符串（例如 "production_efficiency"）。</param>
        /// <param name="defaultValue">如果找不到对应的效果，则返回此默认值。</param>
        /// <returns>效果的数值或默认值。</returns>
        public float GetTechEffectValue(string effectName, float defaultValue)
        {
            // 简化的实现：根据传入的效果名称字符串，硬编码映射到具体的TechEffectType和目标
            // 实际项目中，这种映射可能更复杂或由配置驱动
            switch (effectName)
            {
                case "production_efficiency": // 生产效率加成
                    return GetTechEffectValue(TechEffectType.ProductionBonus, "global"); // 假设全局生产加成
                case "defense_efficiency":    // 防御效率加成
                    return GetTechEffectValue(TechEffectType.DefenseBonus, "global");   // 假设全局防御加成
                case "research_speed_bonus":  // 研究速度加成
                    return GetTechEffectValue(TechEffectType.ResearchSpeedBonus, "global"); // 假设全局研究速度加成
                // 可以添加更多效果名称的映射
                default:
                    Debug.LogWarning($"[科技系统] 未知的科技效果名称请求: '{effectName}'。将返回默认值 {defaultValue}。");
                    return defaultValue; // 如果名称未匹配，则返回传入的默认值
            }
        }
        
        /// <summary>
        /// 获取指定建筑ID的生产相关的科技加成。
        /// </summary>
        /// <param name="buildingId">建筑ID (或更通用的目标ID)。</param>
        /// <returns>生产加成值。</returns>
        public float GetProductionBonus(string buildingId)
        {
            // 此处target可以是建筑ID，也可以是建筑类型ID，或者"global"
            // 取决于科技效果如何设计和应用
            return GetTechEffectValue(TechEffectType.ProductionBonus, buildingId);
        }
        
        /// <summary>
        /// 获取指定目标的效率相关的科技加成。
        /// </summary>
        /// <param name="target">效果目标ID (例如，特定工种ID，或"global"表示全局效率)。</param>
        /// <returns>效率加成值。</returns>
        public float GetEfficiencyBonus(string target)
        {
            return GetTechEffectValue(TechEffectType.EfficiencyBonus, target);
        }
        
        #endregion
        
        #region 事件处理 (Event Handling)
        
        /// <summary>
        /// 处理游戏更新事件，主要用于驱动研究进度的更新。
        /// </summary>
        /// <param name="e">游戏更新事件参数，包含deltaTime。</param>
        private void OnGameUpdate(GameUpdateEvent e)
        {
            UpdateResearch(e.DeltaTime); // 根据帧间隔时间更新所有活动研究任务的进度
        }
        
        #endregion
        
        #region 公共接口 (Public API - 其他查询方法)
        
        /// <summary>
        /// 获取完整的科技树数据对象。
        /// </summary>
        /// <returns>TechTreeData实例。</returns>
        public TechTreeData GetTechTreeData()
        {
            return _techTreeData;
        }
        
        /// <summary>
        /// 根据指定的科技树分支，获取该分支下的所有科技ID列表。
        /// </summary>
        /// <param name="tree">要查询的科技树分支枚举。</param>
        /// <returns>属于该分支的科技ID列表；如果分支不存在或无科技，则返回空列表。</returns>
        public List<string> GetTechsByTree(TechTree tree)
        {
            // 安全地尝试获取，如果键不存在则返回新的空列表
            return _techTreeData.TreeTechs.TryGetValue(tree, out var techs) 
                ? new List<string>(techs) // 返回列表副本以防外部修改
                : new List<string>();
        }
        
        /// <summary>
        /// 获取所有当前正在进行的研究任务列表。
        /// </summary>
        /// <returns>活动研究任务的列表副本。</returns>
        public List<ResearchTask> GetActiveResearchTasks()
        {
            return new List<ResearchTask>(_activeResearchTasks); // 返回副本
        }
        
        /// <summary>
        /// 切换指定ID科技研究任务的暂停/恢复状态。
        /// </summary>
        /// <param name="techId">要操作的科技ID。</param>
        /// <returns>如果成功切换状态则返回true，否则（如任务不存在）返回false。</returns>
        public bool ToggleResearchPause(string techId)
        {
            var task = _activeResearchTasks.FirstOrDefault(t => t.TechId == techId); // 查找任务
            if (task != null)
            {
                task.IsPaused = !task.IsPaused; // 切换暂停状态
                Debug.Log($"[科技系统] 研究任务 {techId} 已 {(task.IsPaused ? "暂停" : "恢复")}。");
                return true;
            }
            Debug.LogWarning($"[科技系统] 尝试切换暂停状态失败：找不到科技ID为 {techId} 的研究任务。");
            return false;
        }
        
        /// <summary>
        /// 取消指定ID的科技研究任务，并返还部分已消耗的科研点数。
        /// </summary>
        /// <param name="techId">要取消的科技ID。</param>
        /// <returns>如果成功取消则返回true，否则（如任务不存在）返回false。</returns>
        public bool CancelResearch(string techId)
        {
            var task = _activeResearchTasks.FirstOrDefault(t => t.TechId == techId); // 查找任务
            if (task == null)
            {
                Debug.LogWarning($"[科技系统] 尝试取消研究失败：找不到科技ID为 {techId} 的研究任务。");
                return false;
            }
            
            _activeResearchTasks.Remove(task); // 从活动任务列表中移除
            
            // 重置科技数据的研究状态
            if (_techTreeData.Technologies.TryGetValue(techId, out var techData))
            {
                techData.IsResearching = false;
                techData.ResearchProgress = 0f;
                techData.ResearchStartTime = 0f;
            }
            
            // 返还部分已消耗的科研点数 (例如，50%)
            var techConfig = _configSystem.GetTechConfig(techId);
            if (techConfig != null)
            {
                int refundAmount = Mathf.FloorToInt(techConfig.ResearchPointsCost * 0.5f); // 计算返还数量
                _techTreeData.SpentResearchPoints -= refundAmount; // 从已花费中减去（相当于增加可用点数）
                // 确保SpentResearchPoints不为负
                if (_techTreeData.SpentResearchPoints < 0) _techTreeData.SpentResearchPoints = 0;
                Debug.Log($"[科技系统] 已取消研究科技 {techConfig.Name} (ID: {techId})，并返还了 {refundAmount} 科研点数。");
            }
            // TODO: 考虑是否返还其他额外资源成本
            
            return true;
        }
        
        /// <summary>
        /// （兼容旧接口）检查指定ID的科技是否已经研究完成。
        /// 等同于 IsTechCompleted 方法。
        /// </summary>
        /// <param name="techId">要检查的科技ID。</param>
        /// <returns>如果已研究则返回true，否则false。</returns>
        public bool IsTechResearched(string techId)
        {
            return IsTechCompleted(techId);
        }
        
        #endregion
    }
    
    #region 事件定义 (Event Definitions)
    
    /// <summary>
    /// 当一项科技成功研发完成时发送的事件结构体。
    /// </summary>
    public struct TechCompletedEvent
    {
        /// <summary>
        /// 完成研发的科技的唯一ID。
        /// </summary>
        public string TechId;
        /// <summary>
        /// 完成研发的科技的名称。
        /// </summary>
        public string TechName;
    }
    
    // GameUpdateEvent 通常由QFramework或游戏主循环提供，此处不重复定义
    // public struct GameUpdateEvent
    // {
    //     public float DeltaTime; // 帧间隔时间
    // }
    
    #endregion
    
    #region 辅助类 (Helper Classes)
    
    /// <summary>
    /// 属性等级助手类。
    /// 提供将属性值转换为效率乘数等辅助功能。
    /// </summary>
    public static class AttributeLevelHelper
    {
        /// <summary>
        /// 根据幸存者的属性值计算其在该属性上的工作效率乘数。
        /// </summary>
        /// <param name="attributeValue">幸存者的属性值。</param>
        /// <returns>效率乘数（例如，1.0代表100%效率，1.2代表120%效率）。</returns>
        public static float GetEfficiencyMultiplier(int attributeValue)
        {
            // 示例：一个简单的分级效率模型
            if (attributeValue >= 80) return 2.0f;      // 大师级: 200% 效率
            if (attributeValue >= 60) return 1.6f;      // 专家级: 160% 效率
            if (attributeValue >= 40) return 1.3f;      // 专业级: 130% 效率
            if (attributeValue >= 20) return 1.1f;      // 熟练级: 110% 效率
            return 1.0f;                                 // 新手级 (或基础值): 100% 效率
        }
    }
    
    #endregion
} 