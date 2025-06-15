// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：CraftingSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件实现了制造系统 (CraftingSystem)，负责管理游戏中的物品制造流程，
//     包括处理制造配方、管理激活的制造任务、计算制造效率、更新制造进度、
//     以及与建筑和幸存者系统的交互（如工人分配对效率的影响）。
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
    /// 制造系统 - 管理物品制造流程、配方以及相关的任务。
    /// </summary>
    public class CraftingSystem : AbstractSystem
    {
        #region 数据存储 (Data Storage)
        
        /// <summary>
        /// 存储当前所有激活的制造任务的字典，键为任务ID。
        /// </summary>
        private Dictionary<string, CraftingTask> _activeCraftingTasks;
        /// <summary>
        /// 存储每个建筑可用的配方ID列表的字典，键为建筑ID。
        /// （此字段用于缓存，以提高查询效率）
        /// </summary>
        private Dictionary<string, List<string>> _buildingRecipes; // 建筑ID -> 可用配方ID列表
        
        #endregion
        
        #region 系统初始化 (System Initialization)
        
        /// <summary>
        /// 系统初始化方法。
        /// 初始化用于存储制造任务和建筑配方缓存的字典，并注册相关事件监听。
        /// </summary>
        protected override void OnInit()
        {
            _activeCraftingTasks = new Dictionary<string, CraftingTask>();
            _buildingRecipes = new Dictionary<string, List<string>>();
            
            // 注册游戏更新事件，用于每帧更新制造进度
            this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);
            // 注册建筑完成事件，用于在建筑完成后更新其可用配方列表
            this.RegisterEvent<BuildingConstructionCompletedEvent>(OnBuildingCompleted);
            Debug.Log("[制造系统] 初始化完成。");
        }
        
        #endregion
        
        #region 制造任务管理 (Crafting Task Management)
        
        /// <summary>
        /// 开始一个新的制造任务。
        /// </summary>
        /// <param name="recipeId">要制造的配方ID。</param>
        /// <param name="buildingId">执行制造任务的建筑ID。</param>
        /// <param name="quantity">要制造的数量，默认为1。</param>
        /// <param name="crafterIds">（可选）参与制造的工人ID列表。</param>
        /// <returns>如果成功开始制造任务则返回true，否则返回false。</returns>
        public bool StartCrafting(string recipeId, string buildingId, int quantity = 1, List<string> crafterIds = null)
        {
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var recipe = configSystem.GetCraftingRecipe(recipeId); // 获取配方配置
            
            if (recipe == null) // 检查配方是否存在
            {
                Debug.LogError($"[制造系统] 找不到制造配方ID: {recipeId}");
                return false;
            }
            
            // 检查执行制造任务的建筑是否支持此配方
            if (!CanCraftInBuilding(recipeId, buildingId))
            {
                Debug.LogWarning($"[制造系统] 建筑 (ID: {buildingId}) 不支持制造配方: {recipe.Name} (ID: {recipeId})。");
                return false;
            }
            
            // 计算制造指定数量物品所需的总资源成本
            var resourceSystem = this.GetSystem<ResourceSystem>(); // 获取资源系统
            var totalCosts = CalculateTotalCosts(recipe.InputCosts, quantity); // 计算总成本
            
            // 检查资源是否充足
            if (!resourceSystem.CanAfford(totalCosts))
            {
                Debug.LogWarning($"[制造系统] 资源不足，无法制造 {recipe.Name} x{quantity}。");
                // TODO: 可以向UI发送资源不足的通知
                return false;
            }
            
            // 检查科技等级要求 (假设的TechSystem，具体实现可能不同)
            if (recipe.RequiredTechLevel > 0)
            {
                var techSystem = this.GetSystem<AdvancedTechSystem>(); // 或 ITechSystem
                // 示例：假设有一个全局科技等级或特定科技树等级需要满足
                // if (techSystem.GetCurrentTechLevel(TechTree.Engineering) < recipe.RequiredTechLevel) // 假设方法
                // {
                //     Debug.LogWarning($"[制造系统] 制造 {recipe.Name} 所需的科技等级不足。");
                //     return false;
                // }
            }
            
            // 消耗制造所需的资源
            resourceSystem.ConsumeResources(totalCosts); // 传入总成本列表
            
            // 创建新的制造任务实例
            var taskId = Guid.NewGuid().ToString(); // 生成唯一任务ID
            var craftingTask = new CraftingTask
            {
                Id = taskId, // 任务ID
                RecipeId = recipeId, // 配方ID
                BuildingId = buildingId, // 建筑ID
                Quantity = quantity, // 总制造数量
                RemainingQuantity = quantity, // 剩余待制造数量
                CrafterIds = crafterIds ?? new List<string>(), // 分配的工人ID列表 (如果为null则初始化为空列表)
                StartTime = Time.time, // 任务开始时间戳
                Progress = 0f, // 当前单位的制造进度 (0-1)
                IsActive = true // 任务默认为激活状态
            };
            
            // 计算并设置任务的制造效率和预计完成时间
            craftingTask.EfficiencyMultiplier = CalculateCraftingEfficiency(craftingTask, recipe);
            float totalCraftTimeSeconds = recipe.CraftingTime * 3600f * quantity; // 总基础制造时间（秒）
            craftingTask.EstimatedEndTime = Time.time + (totalCraftTimeSeconds / craftingTask.EfficiencyMultiplier); // 预计结束时间
            
            _activeCraftingTasks[taskId] = craftingTask; // 将新任务添加到活动任务列表
            
            Debug.Log($"[制造系统] 开始制造任务 (ID: {taskId}): {recipe.Name} x{quantity}，预计耗时: {(craftingTask.EstimatedEndTime - Time.time):F1} 秒。");
            // TODO: 发送制造任务开始事件
            return true;
        }
        
        /// <summary>
        /// 计算指定制造任务的综合制造效率。
        /// 效率受多种因素影响，如工人技能、建筑等级、科技加成等。
        /// </summary>
        /// <param name="task">要计算效率的制造任务。</param>
        /// <param name="recipe">该任务对应的制造配方。</param>
        /// <returns>综合效率乘数（例如1.0代表标准效率，1.5代表效率提升50%）。</returns>
        private float CalculateCraftingEfficiency(CraftingTask task, CraftingRecipe recipe)
        {
            float totalEfficiencyFactor = 1.0f; // 基础效率为100%
            
            // 1. 制造者技能加成
            var survivorSystem = this.GetSystem<SurvivorSystem>(); // 获取幸存者系统
            if (task.CrafterIds != null && task.CrafterIds.Any())
            {
                float totalWorkerEfficiencyBonus = 0f;
                foreach (var crafterId in task.CrafterIds)
                {
                    var crafter = survivorSystem.GetSurvivor(crafterId); // 获取工人数据
                    if (crafter != null)
                    {
                        // 假设 Technology 属性影响制造效率
                        var techAttribute = crafter.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Technology);
                        if (techAttribute != null)
                        {
                            // AttributeLevelHelper.GetEfficiencyMultiplier 将属性值转换为效率乘数 (例如 1.0 表示无加成, 1.2 表示 +20%)
                            // 0.5f 是一个示例系数，表示科技属性对制造效率的贡献程度
                            totalWorkerEfficiencyBonus += (AttributeLevelHelper.GetEfficiencyMultiplier(techAttribute.Value) - 1.0f) * 0.5f;
                        }
                    }
                }
                // 如果有多个工人，可以考虑平均他们的加成，或者取最高值，或者累加（取决于游戏设计）
                // 此处示例为累加，但要注意平衡性，可能需要调整系数或采用其他计算方式
                totalEfficiencyFactor += totalWorkerEfficiencyBonus;
            }
            
            // 2. 建筑等级/类型加成
            var buildingSystem = this.GetSystem<BuildingSystem>(); // 注意：之前代码为EnhancedBuildingSystem，此处统一为BuildingSystem
            var configSystem = this.GetSystem<ConfigSystem>();
            var buildingData = buildingSystem.GetBuilding(task.BuildingId); // 获取建筑数据
            if (buildingData != null)
            {
                var buildingConfig = configSystem.GetBuildingConfig(buildingData.ConfigId); // 获取建筑配置
                if (buildingConfig != null)
                {
                    // 示例：建筑每升一级，制造效率提升15% (相对于基础效率)
                    totalEfficiencyFactor += ((int)buildingConfig.Level -1) * 0.15f; // Level1为0加成, Level2为15%, ...
                    // 也可以根据建筑类型提供特定加成，例如特定工坊对某类配方有额外加成
                }
            }
            
            // 3. 科技加成
            var techSystem = this.GetSystem<AdvancedTechSystem>(); // 假设使用AdvancedTechSystem
            // 假设有一个名为 "crafting_efficiency_bonus" 的科技效果，其值为百分比加成 (例如 0.1 代表 +10%)
            totalEfficiencyFactor *= (1 + techSystem.GetTechEffectValue("crafting_efficiency_bonus", 0f));
            
            // 确保效率不低于某个下限 (例如10%)，防止出现负数或过低的效率
            return Mathf.Max(0.1f, totalEfficiencyFactor);
        }
        
        /// <summary>
        /// 每帧更新所有激活且未暂停的制造任务的进度。
        /// </summary>
        /// <param name="deltaTime">自上一帧以来经过的时间（秒）。</param>
        private void UpdateCrafting(float deltaTime)
        {
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var resourceSystem = this.GetSystem<ResourceSystem>(); // 获取资源系统
            var completedTaskIds = new List<string>(); // 用于存储已完成任务的ID，以便后续移除
            
            foreach (var task in _activeCraftingTasks.Values) // 遍历所有活动任务
            {
                if (!task.IsActive || task.IsPaused) continue; // 跳过未激活或已暂停的任务
                
                var recipe = configSystem.GetCraftingRecipe(task.RecipeId); // 获取任务对应的配方
                if (recipe == null)
                {
                    Debug.LogError($"[制造系统] 更新任务 (ID: {task.Id}) 失败：找不到配方ID {task.RecipeId}。");
                    task.IsActive = false; // 标记任务为非活动状态以避免重复错误
                    continue;
                }
                
                // 计算此时间间隔内增加的制造进度
                // 进度增加 = (deltaTime秒 / (单个物品的基础制造时间小时 * 3600秒/小时)) * 综合效率乘数
                float progressIncrement = (deltaTime / (recipe.CraftingTime * 3600f)) * task.EfficiencyMultiplier;
                task.Progress += progressIncrement; //累加当前物品的制造进度
                
                // 检查是否完成了一个或多个单位的制造
                while (task.Progress >= 1.0f && task.RemainingQuantity > 0)
                {
                    task.Progress -= 1.0f; // 消耗掉100%的进度，用于完成一个单位
                    task.RemainingQuantity--; // 剩余待制造数量减1
                    
                    // 将制造完成的物品添加到资源系统
                    resourceSystem.AddResource(recipe.OutputType, recipe.OutputAmount);
                    
                    // 为参与制造的工人增加经验值
                    var survivorSystem = this.GetSystem<SurvivorSystem>(); // 获取幸存者系统
                    if (task.CrafterIds != null && task.CrafterIds.Any())
                    {
                        foreach (var crafterId in task.CrafterIds)
                        {
                            // survivorSystem.AddExperienceToSurvivor(crafterId, 3); // 示例：增加3点通用经验
                            var crafter = survivorSystem.GetSurvivor(crafterId);
                            if (crafter != null)
                            {
                                // 假设制造主要增加科技属性经验
                                crafter.AddAttributeExperience(SurvivorAttributeType.Technology, 3); // 示例：增加3点科技经验
                            }
                        }
                    }
                    
                    Debug.Log($"[制造系统] 完成制造一个单位的 {recipe.Name} (配方ID: {recipe.Id})。剩余数量: {task.RemainingQuantity}");
                    
                    // 发送单个物品制造完成事件（如果需要更细粒度的事件）
                    this.SendEvent(new CraftingCompletedEvent // 使用已有的事件结构体
                    {
                        RecipeId = recipe.Id,
                        OutputType = recipe.OutputType,
                        OutputAmount = recipe.OutputAmount // 注意：这里是配方定义的单次产出量，不是任务总产出
                    });
                }
                
                // 检查整个制造任务（所有数量）是否已完成
                if (task.RemainingQuantity <= 0)
                {
                    completedTaskIds.Add(task.Id); // 将完成的任务ID添加到待移除列表
                    Debug.Log($"[制造系统] 制造任务 (ID: {task.Id}) [{recipe.Name} x{task.Quantity}] 已全部完成。");
                    // 可在此处发送整个任务完成的特定事件（如果需要）
                }
            }
            
            // 从活动任务字典中移除所有已完成的任务
            foreach (var taskId in completedTaskIds)
            {
                _activeCraftingTasks.Remove(taskId);
            }
        }
        
        #endregion
        
        #region 配方管理 (Recipe Management)
        
        /// <summary>
        /// 检查指定的建筑是否可以制造指定的配方。
        /// </summary>
        /// <param name="recipeId">要检查的配方ID。</param>
        /// <param name="buildingId">执行制造的建筑ID。</param>
        /// <returns>如果建筑支持该配方则返回true，否则返回false。</returns>
        public bool CanCraftInBuilding(string recipeId, string buildingId)
        {
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var recipe = configSystem.GetCraftingRecipe(recipeId); // 获取配方配置
            var buildingSystem = this.GetSystem<BuildingSystem>(); // 获取建筑系统 (修正为BuildingSystem)
            var building = buildingSystem.GetBuilding(buildingId); // 获取建筑数据
            
            if (recipe == null || building == null) // 配方或建筑不存在
            {
                Debug.LogWarning($"[制造系统] 检查配方可用性失败：配方 {recipeId} 或建筑 {buildingId} 不存在。");
                return false;
            }
            
            var buildingConfig = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
            if (buildingConfig == null)
            {
                Debug.LogWarning($"[制造系统] 检查配方可用性失败：找不到建筑 {building.ConfigId} 的配置。");
                return false;
            }
            
            // 检查配方要求的建筑ID列表是否包含当前建筑的ConfigId，
            // 或者是否包含当前建筑的类别 (作为一种更通用的匹配方式)
            return recipe.RequiredBuildings.Contains(buildingConfig.ConfigId) || 
                   recipe.RequiredBuildings.Contains(buildingConfig.Category.ToString()); // 假设类别也可能作为标识符
        }
        
        /// <summary>
        /// 获取指定建筑当前可用的所有制造配方列表。
        /// </summary>
        /// <param name="buildingId">建筑ID。</param>
        /// <returns>可用制造配方的列表。</returns>
        public List<CraftingRecipe> GetAvailableRecipes(string buildingId)
        {
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var allRecipes = configSystem.GetAllCraftingRecipes(); // 获取所有已定义的配方
            var availableRecipes = new List<CraftingRecipe>(); // 初始化可用配方列表
            
            foreach (var recipe in allRecipes.Values) // 遍历所有配方
            {
                if (CanCraftInBuilding(recipe.Id, buildingId)) // 检查此建筑是否支持当前配方
                {
                    // 进一步检查科技要求 (示例逻辑)
                    if (recipe.RequiredTechLevel > 0)
                    {
                        // var techSystem = this.GetSystem<AdvancedTechSystem>();
                        // if (techSystem.GetCurrentTechLevel(TechTree.Engineering) < recipe.RequiredTechLevel) // 假设检查工程科技树等级
                        //     continue; // 未满足科技要求，跳过此配方
                    }
                    availableRecipes.Add(recipe); // 如果满足条件，则添加到可用列表
                }
            }
            
            return availableRecipes;
        }
        
        /// <summary>
        /// 根据基础配方成本和制造数量计算总资源成本。
        /// </summary>
        /// <param name="baseCosts">单个单位的基础资源成本列表。</param>
        /// <param name="quantity">要制造的数量。</param>
        /// <returns>包含总资源成本的新列表。</returns>
        private List<ResourceCost> CalculateTotalCosts(List<ResourceCost> baseCosts, int quantity)
        {
            if (quantity <= 0) return new List<ResourceCost>(); // 如果数量无效，返回空成本列表

            var totalCosts = new List<ResourceCost>(); // 初始化总成本列表
            
            foreach (var cost in baseCosts) // 遍历基础成本中的每种资源
            {
                totalCosts.Add(new ResourceCost // 创建新的资源成本项
                {
                    Type = cost.Type, // 资源类型不变
                    Amount = cost.Amount * quantity // 资源数量 = 单个成本 * 总数量
                });
            }
            
            return totalCosts;
        }
        
        #endregion
        
        #region 任务控制 (Task Control)
        
        /// <summary>
        /// 切换指定制造任务的暂停/恢复状态。
        /// </summary>
        /// <param name="taskId">要操作的任务ID。</param>
        /// <returns>如果操作成功（任务存在）则返回true，否则返回false。</returns>
        public bool ToggleCraftingPause(string taskId)
        {
            if (_activeCraftingTasks.TryGetValue(taskId, out var task)) // 尝试获取任务
            {
                task.IsPaused = !task.IsPaused; // 切换暂停状态
                Debug.Log($"[制造系统] 制造任务 (ID: {taskId}) {(task.IsPaused ? "已暂停" : "已恢复")}。");
                return true;
            }
            Debug.LogWarning($"[制造系统] 尝试切换暂停状态失败：找不到任务ID {taskId}。");
            return false;
        }
        
        /// <summary>
        /// 取消一个正在进行的制造任务，并尝试返还部分已消耗的资源。
        /// </summary>
        /// <param name="taskId">要取消的任务ID。</param>
        /// <returns>如果成功取消任务则返回true，否则返回false（例如任务不存在）。</returns>
        public bool CancelCrafting(string taskId)
        {
            if (!_activeCraftingTasks.TryGetValue(taskId, out var task)) // 检查任务是否存在
            {
                Debug.LogWarning($"[制造系统] 尝试取消制造失败：找不到任务ID {taskId}。");
                return false;
            }
            
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var recipe = configSystem.GetCraftingRecipe(task.RecipeId); // 获取配方信息
            
            if (recipe != null)
            {
                // 计算并返还部分资源 (基于剩余未制造的数量，并打一定折扣，例如80%)
                var resourceSystem = this.GetSystem<ResourceSystem>(); // 获取资源系统
                // refundRatio 代表应返还的原始投入资源的比例
                float refundRatio = (task.RemainingQuantity > 0 && task.Quantity > 0) ? ((float)task.RemainingQuantity / task.Quantity) : 0f;
                
                if (refundRatio > 0 && recipe.InputCosts != null)
                {
                    foreach (var cost in recipe.InputCosts) // 遍历配方的原始输入成本
                    {
                        // 计算应返还的该种资源的数量：原始总投入 * 应返还比例 * 折扣率
                        int refundAmount = Mathf.FloorToInt(cost.Amount * task.Quantity * refundRatio * 0.8f); // 示例：返还剩余部分对应资源的80%
                        if (refundAmount > 0)
                        {
                            resourceSystem.AddResource(cost.Type, refundAmount); // 添加返还的资源
                        }
                    }
                    Debug.Log($"[制造系统] 取消制造任务 (ID: {taskId}) [{recipe.Name}]，已返还部分资源。");
                }
                else
                {
                    Debug.Log($"[制造系统] 取消制造任务 (ID: {taskId}) [{recipe.Name}]，无资源可返还或已全部制造。");
                }
            }
            
            _activeCraftingTasks.Remove(taskId); // 从活动任务列表中移除该任务
            // TODO: 发送制造任务取消事件
            return true;
        }
        
        /// <summary>
        /// 设置指定制造任务的优先级。
        /// （注意：实际的优先级调度逻辑需要在UpdateCrafting中实现，当前仅设置值）
        /// </summary>
        /// <param name="taskId">任务ID。</param>
        /// <param name="priority">新的优先级数值（例如，越高越优先）。</param>
        /// <returns>如果任务存在并成功设置优先级则返回true，否则返回false。</returns>
        public bool SetCraftingPriority(string taskId, int priority)
        {
            if (_activeCraftingTasks.TryGetValue(taskId, out var task)) // 尝试获取任务
            {
                task.Priority = priority; // 设置任务优先级
                Debug.Log($"[制造系统] 制造任务 (ID: {taskId}) 的优先级已设置为 {priority}。");
                // TODO: 可能需要重新排序任务队列或在Update中根据优先级处理
                return true;
            }
            Debug.LogWarning($"[制造系统] 设置优先级失败：找不到任务ID {taskId}。");
            return false;
        }
        
        #endregion
        
        #region 自动制造 (Auto-Crafting) - 占位实现
        
        /// <summary>
        /// （占位）设置指定建筑中特定配方的自动制造规则。
        /// </summary>
        /// <param name="buildingId">建筑ID。</param>
        /// <param name="recipeId">配方ID。</param>
        /// <param name="targetQuantity">期望维持的目标库存数量。</param>
        /// <param name="minQuantity">当库存低于此数量时，触发自动制造。</param>
        /// <returns>操作是否成功（当前恒为true）。</returns>
        public bool SetAutoCrafting(string buildingId, string recipeId, int targetQuantity, int minQuantity)
        {
            // TODO: 实现详细的自动制造规则存储和管理逻辑
            // 例如，可以将这些规则存储在BuildingData或一个专门的自动制造任务列表中
            Debug.Log($"[制造系统] (占位) 已为建筑 {buildingId} 设置配方 {recipeId} 的自动制造规则：目标{targetQuantity}, 最低{minQuantity}。");
            return true;
        }
        
        /// <summary>
        /// （占位）每帧检查是否需要触发自动制造任务。
        /// </summary>
        private void CheckAutoCrafting()
        {
            // TODO: 实现遍历所有自动制造规则，检查库存，并按需调用StartCrafting的逻辑
        }
        
        #endregion
        
        #region 事件处理 (Event Handling)
        
        /// <summary>
        /// 处理游戏更新事件，用于驱动制造系统的内部逻辑更新（如任务进度）。
        /// </summary>
        /// <param name="e">游戏更新事件参数，包含deltaTime。</param>
        private void OnGameUpdate(GameUpdateEvent e)
        {
            UpdateCrafting(e.DeltaTime); // 更新所有激活制造任务的进度
            CheckAutoCrafting();         // 检查并触发自动制造任务（如果已实现）
        }
        
        /// <summary>
        /// 处理建筑建造完成事件。
        /// 当新建筑完成时，需要更新该建筑可用的配方列表缓存。
        /// </summary>
        /// <param name="e">建筑建造完成事件参数。</param>
        private void OnBuildingCompleted(BuildingConstructionCompletedEvent e)
        {
            // 当一个建筑建造完成时，重新计算并缓存其可用的配方列表
            UpdateBuildingRecipes(e.BuildingId);
            Debug.Log($"[制造系统] 建筑 (ID: {e.BuildingId}, 类型: {e.ConfigId}) 已完成，其可用配方已更新。");
        }
        
        /// <summary>
        /// 更新并缓存指定建筑ID可用的配方ID列表。
        /// </summary>
        /// <param name="buildingId">要更新配方列表的建筑ID。</param>
        private void UpdateBuildingRecipes(string buildingId)
        {
            var availableRecipeObjects = GetAvailableRecipes(buildingId); // 获取该建筑可用的完整配方对象列表
            // 将配方对象列表转换为ID列表，并更新到缓存字典中
            _buildingRecipes[buildingId] = availableRecipeObjects.Select(r => r.Id).ToList();
        }
        
        #endregion
        
        #region 公共接口 (Public API)
        
        /// <summary>
        /// 获取所有当前激活的制造任务的字典副本。
        /// </summary>
        /// <returns>一个新的字典，包含所有激活的制造任务，键为任务ID。</returns>
        public Dictionary<string, CraftingTask> GetActiveCraftingTasks()
        {
            return new Dictionary<string, CraftingTask>(_activeCraftingTasks); // 返回副本以防外部修改
        }
        
        /// <summary>
        /// 获取特定建筑内正在进行的所有制造任务列表。
        /// </summary>
        /// <param name="buildingId">要查询的建筑ID。</param>
        /// <returns>在该建筑中进行的制造任务列表。</returns>
        public List<CraftingTask> GetBuildingCraftingTasks(string buildingId)
        {
            return _activeCraftingTasks.Values
                .Where(task => task.BuildingId == buildingId) // 筛选出属于该建筑的任务
                .OrderBy(task => task.Priority) // （可选）按优先级排序
                .ToList();
        }
        
        /// <summary>
        /// 根据任务ID获取制造任务的详细信息。
        /// </summary>
        /// <param name="taskId">任务ID。</param>
        /// <returns>对应的CraftingTask实例；如果找不到，则返回null。</returns>
        public CraftingTask GetCraftingTask(string taskId)
        {
            return _activeCraftingTasks.TryGetValue(taskId, out var task) ? task : null;
        }
        
        /// <summary>
        /// 检查当前资源是否足够制造指定数量的某种物品。
        /// </summary>
        /// <param name="itemType">要检查的物品资源类型。</param>
        /// <param name="quantity">要制造的数量，默认为1。</param>
        /// <returns>如果资源足够则返回true，否则返回false。</returns>
        public bool CanCraftItem(ResourceType itemType, int quantity = 1)
        {
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var allRecipes = configSystem.GetAllCraftingRecipes(); // 获取所有配方
            
            // 查找能产出指定物品类型的配方 (假设一个物品类型只对应一个主要配方，或取第一个匹配的)
            var recipe = allRecipes.Values.FirstOrDefault(r => r.OutputType == itemType);
            if (recipe == null) // 如果找不到该物品的配方
            {
                Debug.LogWarning($"[制造系统] 无法检查是否可制造 {itemType}：未找到对应的制造配方。");
                return false;
            }
            
            var resourceSystem = this.GetSystem<ResourceSystem>(); // 获取资源系统
            var totalCosts = CalculateTotalCosts(recipe.InputCosts, quantity); // 计算总成本
            
            return resourceSystem.CanAfford(totalCosts); // 检查资源是否充足
        }
        
        #endregion
    }
    
    #region 数据结构 (Data Structures)
    
    /// <summary>
    /// 代表一个正在进行或已计划的制造任务。
    /// </summary>
    [Serializable]
    public class CraftingTask
    {
        /// <summary>
        /// 任务的唯一标识符。
        /// </summary>
        public string Id;
        /// <summary>
        /// 此任务所使用的制造配方的ID。
        /// </summary>
        public string RecipeId;
        /// <summary>
        /// 执行此制造任务的建筑的ID。
        /// </summary>
        public string BuildingId;
        /// <summary>
        /// 计划制造的总数量。
        /// </summary>
        public int Quantity;
        /// <summary>
        /// 剩余尚未制造完成的数量。
        /// </summary>
        public int RemainingQuantity;
        /// <summary>
        /// （可选）参与此制造任务的工人（幸存者）ID列表。
        /// </summary>
        public List<string> CrafterIds;
        /// <summary>
        /// 任务开始时的游戏时间戳。
        /// </summary>
        public float StartTime;
        /// <summary>
        /// 根据当前效率估算的预计完成时间戳。
        /// </summary>
        public float EstimatedEndTime;
        /// <summary>
        /// 当前正在制造的单个物品的进度（范围 0 到 1）。
        /// </summary>
        public float Progress;              // 当前单位的进度（0-1）
        /// <summary>
        /// 综合效率乘数（受工人技能、建筑等级、科技等影响）。
        /// </summary>
        public float EfficiencyMultiplier;
        /// <summary>
        /// 任务是否处于活动状态（例如，是否因缺少资源而暂停）。
        /// </summary>
        public bool IsActive;
        /// <summary>
        /// 任务是否被玩家手动暂停。
        /// </summary>
        public bool IsPaused;
        /// <summary>
        /// 任务的优先级（用于决定在资源或工人有限时的处理顺序，数值越大优先级越高）。
        /// </summary>
        public int Priority;                // 优先级（数字越大优先级越高）
        
        /// <summary>
        /// CraftingTask的构造函数。
        /// 初始化列表和默认状态。
        /// </summary>
        public CraftingTask()
        {
            CrafterIds = new List<string>(); // 初始化工人ID列表
            IsActive = true;                 // 默认激活
            IsPaused = false;                // 默认未暂停
            Priority = 0;                    // 默认优先级为0
        }
    }
    
    #endregion
    
    #region 事件定义 (Event Definitions)
    
    /// <summary>
    /// 制造完成事件结构体。
    /// 当一个或一批次物品制造完成时发送。
    /// </summary>
    public struct CraftingCompletedEvent
    {
        /// <summary>
        /// 完成制造的配方ID。
        /// </summary>
        public string RecipeId;
        /// <summary>
        /// 产出物品的资源类型。
        /// </summary>
        public ResourceType OutputType;
        /// <summary>
        /// 本次完成的产出物品数量 (通常是配方定义的单次产出量)。
        /// </summary>
        public int OutputAmount;
    }
    
    #endregion
} 