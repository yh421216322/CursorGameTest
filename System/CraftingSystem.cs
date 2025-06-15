using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 制造系统 - 管理物品制造和配方
    /// </summary>
    public class CraftingSystem : AbstractSystem
    {
        #region 数据存储
        
        private Dictionary<string, CraftingTask> _activeCraftingTasks;
        private Dictionary<string, List<string>> _buildingRecipes; // 建筑ID -> 可用配方ID列表
        
        #endregion
        
        #region 系统初始化
        
        protected override void OnInit()
        {
            _activeCraftingTasks = new Dictionary<string, CraftingTask>();
            _buildingRecipes = new Dictionary<string, List<string>>();
            
            // 注册事件
            this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);
            this.RegisterEvent<BuildingConstructionCompletedEvent>(OnBuildingCompleted);
        }
        
        #endregion
        
        #region 制造任务管理
        
        /// <summary>
        /// 开始制造任务
        /// </summary>
        public bool StartCrafting(string recipeId, string buildingId, int quantity = 1, List<string> crafterIds = null)
        {
            var configSystem = this.GetSystem<ConfigSystem>();
            var recipe = configSystem.GetCraftingRecipe(recipeId);
            
            if (recipe == null)
            {
                Debug.LogError($"找不到制造配方: {recipeId}");
                return false;
            }
            
            // 检查建筑是否支持此配方
            if (!CanCraftInBuilding(recipeId, buildingId))
            {
                Debug.LogWarning($"建筑不支持制造配方: {recipe.Name}");
                return false;
            }
            
            // 检查资源是否足够
            var resourceSystem = this.GetSystem<ResourceSystem>();
            var totalCosts = CalculateTotalCosts(recipe.InputCosts, quantity);
            
            if (!resourceSystem.CanAfford(totalCosts))
            {
                Debug.LogWarning($"资源不足，无法制造 {recipe.Name} x{quantity}");
                return false;
            }
            
            // 检查科技要求
            if (recipe.RequiredTechLevel > 0)
            {
                var techSystem = this.GetSystem<TechSystem>();
                // 这里可以添加具体的科技检查逻辑
            }
            
            // 消耗资源
            resourceSystem.ConsumeResources();
            
            // 创建制造任务
            var taskId = Guid.NewGuid().ToString();
            var craftingTask = new CraftingTask
            {
                Id = taskId,
                RecipeId = recipeId,
                BuildingId = buildingId,
                Quantity = quantity,
                RemainingQuantity = quantity,
                CrafterIds = crafterIds ?? new List<string>(),
                StartTime = Time.time,
                Progress = 0f,
                IsActive = true
            };
            
            // 计算制造效率
            craftingTask.EfficiencyMultiplier = CalculateCraftingEfficiency(craftingTask, recipe);
            craftingTask.EstimatedEndTime = Time.time + (recipe.CraftingTime * 3600f * quantity / craftingTask.EfficiencyMultiplier);
            
            _activeCraftingTasks[taskId] = craftingTask;
            
            Debug.Log($"开始制造 {recipe.Name} x{quantity}，预计完成时间: {craftingTask.EstimatedEndTime - Time.time:F1} 秒");
            return true;
        }
        
        /// <summary>
        /// 计算制造效率
        /// </summary>
        private float CalculateCraftingEfficiency(CraftingTask task, CraftingRecipe recipe)
        {
            float efficiency = 1.0f;
            
            // 制造者技能加成
            var survivorSystem = this.GetSystem<SurvivorSystem>();
            foreach (var crafterId in task.CrafterIds)
            {
                var crafter = survivorSystem.GetSurvivor(crafterId);
                if (crafter != null)
                {
                    var techAttr = crafter.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Technology);
                    if (techAttr != null)
                    {
                        efficiency += (AttributeLevelHelper.GetEfficiencyMultiplier(techAttr.Value) - 1.0f) * 0.5f;
                    }
                }
            }
            
            // 建筑等级加成
            var buildingSystem = this.GetSystem<EnhancedBuildingSystem>();
            var configSystem = this.GetSystem<ConfigSystem>();
            var building = buildingSystem.GetBuilding(task.BuildingId);
            if (building != null)
            {
                var buildingConfig = configSystem.GetBuildingConfig(building.ConfigId);
                if (buildingConfig != null)
                {
                    efficiency += (int)buildingConfig.Level * 0.15f; // 每级增加15%效率
                }
            }
            
            // 科技加成
            var techSystem = this.GetSystem<TechSystem>();
            efficiency += techSystem.GetEfficiencyBonus("crafting");
            
            return Mathf.Max(0.1f, efficiency);
        }
        
        /// <summary>
        /// 更新制造进度
        /// </summary>
        private void UpdateCrafting(float deltaTime)
        {
            var configSystem = this.GetSystem<ConfigSystem>();
            var resourceSystem = this.GetSystem<ResourceSystem>();
            var completedTasks = new List<string>();
            
            foreach (var task in _activeCraftingTasks.Values)
            {
                if (!task.IsActive || task.IsPaused) continue;
                
                var recipe = configSystem.GetCraftingRecipe(task.RecipeId);
                if (recipe == null) continue;
                
                // 更新进度
                float progressDelta = (deltaTime / (recipe.CraftingTime * 3600f)) * task.EfficiencyMultiplier;
                task.Progress += progressDelta;
                
                // 检查是否完成一个单位
                while (task.Progress >= 1.0f && task.RemainingQuantity > 0)
                {
                    task.Progress -= 1.0f;
                    task.RemainingQuantity--;
                    
                    // 产出物品
                    resourceSystem.AddResource(recipe.OutputType, recipe.OutputAmount);
                    
                    // 给制造者经验
                    var survivorSystem = this.GetSystem<SurvivorSystem>();
                    foreach (var crafterId in task.CrafterIds)
                    {
                        // 添加通用经验
                        survivorSystem.AddExperienceToSurvivor(crafterId, 3);
                        // 添加技术属性经验
                        var crafter = survivorSystem.GetSurvivor(crafterId);
                        if (crafter != null)
                        {
                            crafter.AddAttributeExperience(SurvivorAttributeType.Technology, 3);
                        }
                    }
                    
                    Debug.Log($"完成制造 {recipe.Name} x{recipe.OutputAmount}");
                    
                    // 发送制造完成事件
                    this.SendEvent(new CraftingCompletedEvent
                    {
                        RecipeId = recipe.Id,
                        OutputType = recipe.OutputType,
                        OutputAmount = recipe.OutputAmount
                    });
                }
                
                // 检查整个任务是否完成
                if (task.RemainingQuantity <= 0)
                {
                    completedTasks.Add(task.Id);
                    Debug.Log($"制造任务完成: {recipe.Name}");
                }
            }
            
            // 移除已完成的任务
            foreach (var taskId in completedTasks)
            {
                _activeCraftingTasks.Remove(taskId);
            }
        }
        
        #endregion
        
        #region 配方管理
        
        /// <summary>
        /// 检查建筑是否可以制造指定配方
        /// </summary>
        public bool CanCraftInBuilding(string recipeId, string buildingId)
        {
            var configSystem = this.GetSystem<ConfigSystem>();
            var recipe = configSystem.GetCraftingRecipe(recipeId);
            var buildingSystem = this.GetSystem<EnhancedBuildingSystem>();
            var building = buildingSystem.GetBuilding(buildingId);
            
            if (recipe == null || building == null) return false;
            
            var buildingConfig = configSystem.GetBuildingConfig(building.ConfigId);
            if (buildingConfig == null) return false;
            
            // 检查建筑是否在配方的可用建筑列表中
            return recipe.RequiredBuildings.Contains(buildingConfig.ConfigId) || 
                   recipe.RequiredBuildings.Contains(buildingConfig.Category.ToString());
        }
        
        /// <summary>
        /// 获取建筑可用的配方列表
        /// </summary>
        public List<CraftingRecipe> GetAvailableRecipes(string buildingId)
        {
            var configSystem = this.GetSystem<ConfigSystem>();
            var allRecipes = configSystem.GetAllCraftingRecipes();
            var availableRecipes = new List<CraftingRecipe>();
            
            foreach (var recipe in allRecipes.Values)
            {
                if (CanCraftInBuilding(recipe.Id, buildingId))
                {
                    // 检查科技要求
                    if (recipe.RequiredTechLevel > 0)
                    {
                        // 这里可以添加科技检查
                    }
                    
                    availableRecipes.Add(recipe);
                }
            }
            
            return availableRecipes;
        }
        
        /// <summary>
        /// 计算总制造成本
        /// </summary>
        private List<ResourceCost> CalculateTotalCosts(List<ResourceCost> baseCosts, int quantity)
        {
            var totalCosts = new List<ResourceCost>();
            
            foreach (var cost in baseCosts)
            {
                totalCosts.Add(new ResourceCost
                {
                    Type = cost.Type,
                    Amount = cost.Amount * quantity
                });
            }
            
            return totalCosts;
        }
        
        #endregion
        
        #region 任务控制
        
        /// <summary>
        /// 暂停/恢复制造任务
        /// </summary>
        public bool ToggleCraftingPause(string taskId)
        {
            if (_activeCraftingTasks.TryGetValue(taskId, out var task))
            {
                task.IsPaused = !task.IsPaused;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 取消制造任务
        /// </summary>
        public bool CancelCrafting(string taskId)
        {
            if (!_activeCraftingTasks.TryGetValue(taskId, out var task))
                return false;
            
            var configSystem = this.GetSystem<ConfigSystem>();
            var recipe = configSystem.GetCraftingRecipe(task.RecipeId);
            
            if (recipe != null)
            {
                // 返还部分资源（基于剩余数量）
                var resourceSystem = this.GetSystem<ResourceSystem>();
                var refundRatio = (float)task.RemainingQuantity / task.Quantity;
                
                foreach (var cost in recipe.InputCosts)
                {
                    int refundAmount = Mathf.FloorToInt(cost.Amount * task.Quantity * refundRatio * 0.8f); // 返还80%
                    if (refundAmount > 0)
                    {
                        resourceSystem.AddResource(cost.Type, refundAmount);
                    }
                }
                
                Debug.Log($"取消制造任务: {recipe.Name}，返还部分资源");
            }
            
            _activeCraftingTasks.Remove(taskId);
            return true;
        }
        
        /// <summary>
        /// 设置制造优先级
        /// </summary>
        public bool SetCraftingPriority(string taskId, int priority)
        {
            if (_activeCraftingTasks.TryGetValue(taskId, out var task))
            {
                task.Priority = priority;
                return true;
            }
            return false;
        }
        
        #endregion
        
        #region 自动制造
        
        /// <summary>
        /// 设置自动制造
        /// </summary>
        public bool SetAutoCrafting(string buildingId, string recipeId, int targetQuantity, int minQuantity)
        {
            // 这里可以实现自动制造逻辑
            // 当资源低于minQuantity时自动开始制造到targetQuantity
            return true;
        }
        
        /// <summary>
        /// 检查自动制造条件
        /// </summary>
        private void CheckAutoCrafting()
        {
            // 实现自动制造检查逻辑
        }
        
        #endregion
        
        #region 事件处理
        
        private void OnGameUpdate(GameUpdateEvent e)
        {
            UpdateCrafting(e.DeltaTime);
            CheckAutoCrafting();
        }
        
        private void OnBuildingCompleted(BuildingConstructionCompletedEvent e)
        {
            // 建筑完成时更新可用配方
            UpdateBuildingRecipes(e.BuildingId);
        }
        
        private void UpdateBuildingRecipes(string buildingId)
        {
            var availableRecipes = GetAvailableRecipes(buildingId);
            _buildingRecipes[buildingId] = availableRecipes.Select(r => r.Id).ToList();
        }
        
        #endregion
        
        #region 公共接口
        
        /// <summary>
        /// 获取活跃的制造任务
        /// </summary>
        public Dictionary<string, CraftingTask> GetActiveCraftingTasks()
        {
            return new Dictionary<string, CraftingTask>(_activeCraftingTasks);
        }
        
        /// <summary>
        /// 获取建筑的制造任务
        /// </summary>
        public List<CraftingTask> GetBuildingCraftingTasks(string buildingId)
        {
            return _activeCraftingTasks.Values
                .Where(task => task.BuildingId == buildingId)
                .ToList();
        }
        
        /// <summary>
        /// 获取制造任务详情
        /// </summary>
        public CraftingTask GetCraftingTask(string taskId)
        {
            return _activeCraftingTasks.TryGetValue(taskId, out var task) ? task : null;
        }
        
        /// <summary>
        /// 检查是否可以制造指定物品
        /// </summary>
        public bool CanCraftItem(ResourceType itemType, int quantity = 1)
        {
            var configSystem = this.GetSystem<ConfigSystem>();
            var allRecipes = configSystem.GetAllCraftingRecipes();
            
            var recipe = allRecipes.Values.FirstOrDefault(r => r.OutputType == itemType);
            if (recipe == null) return false;
            
            var resourceSystem = this.GetSystem<ResourceSystem>();
            var totalCosts = CalculateTotalCosts(recipe.InputCosts, quantity);
            
            return resourceSystem.CanAfford(totalCosts);
        }
        
        #endregion
    }
    
    #region 数据结构
    
    /// <summary>
    /// 制造任务
    /// </summary>
    [Serializable]
    public class CraftingTask
    {
        public string Id;
        public string RecipeId;
        public string BuildingId;
        public int Quantity;
        public int RemainingQuantity;
        public List<string> CrafterIds;
        public float StartTime;
        public float EstimatedEndTime;
        public float Progress;              // 当前单位的进度（0-1）
        public float EfficiencyMultiplier;
        public bool IsActive;
        public bool IsPaused;
        public int Priority;                // 优先级（数字越大优先级越高）
        
        public CraftingTask()
        {
            CrafterIds = new List<string>();
            IsActive = true;
            IsPaused = false;
            Priority = 0;
        }
    }
    
    #endregion
    
    #region 事件定义
    
    public struct CraftingCompletedEvent
    {
        public string RecipeId;
        public ResourceType OutputType;
        public int OutputAmount;
    }
    
    #endregion
} 