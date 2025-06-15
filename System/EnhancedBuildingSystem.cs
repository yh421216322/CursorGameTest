using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{

    /// <summary>
    /// 增强建筑系统接口，定义了游戏中所有与建筑相关的操作和行为
    /// </summary>
    public interface IEnhancedBuildingSystem : QFISystem
    {
        // ===== 建造管理 =====

        /// <summary>
        /// 检查是否可以在指定位置建造某种类型的建筑（检查资源、科技、空间等）
        /// </summary>
        /// <param name="position">要尝试建造的位置</param>
        /// <param name="buildingType">建筑类型（对应配置ID）</param>
        /// <returns>是否可以建造</returns>
        bool CanBuildAt(Vector3 position, string buildingType);

        /// <summary>
        /// 开始建造一个新建筑（扣除资源并创建基础数据）
        /// </summary>
        /// <param name="position">建造位置</param>
        /// <param name="buildingType">建筑类型</param>
        /// <returns>是否成功开始建造</returns>
        bool StartConstruction(Vector3 position, string buildingType);

        /// <summary>
        /// 完成某个建筑的建造流程（设置为可操作状态）
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        void CompleteConstruction(string buildingId);

        /// <summary>
        /// 取消正在建造的建筑，并返还部分资源
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        /// <returns>是否成功取消</returns>
        bool CancelConstruction(string buildingId);


        // ===== 建筑操作 =====

        /// <summary>
        /// 升级指定建筑（需要满足升级资源和条件）
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        /// <returns>是否成功升级</returns>
        bool UpgradeBuilding(string buildingId);

        /// <summary>
        /// 拆除指定建筑（移除游戏对象和数据，并返还部分材料）
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        /// <returns>是否成功拆除</returns>
        bool DemolishBuilding(string buildingId);

        /// <summary>
        /// 修复指定建筑（消耗资源恢复血量）
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        /// <returns>是否成功修复</returns>
        bool RepairBuilding(string buildingId);

        /// <summary>
        /// 切换建筑的操作状态（启用/停用），仅限完全建成且健康的建筑
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        /// <returns>是否成功切换</returns>
        bool ToggleBuildingOperation(string buildingId);


        // ===== 生产管理 =====

        /// <summary>
        /// 更新所有建筑的生产逻辑（如每隔一段时间产出资源）
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        void UpdateBuildingProduction(float deltaTime);

        /// <summary>
        /// 立即触发一次指定建筑的资源产出
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        void CollectBuildingOutput(string buildingId);

        /// <summary>
        /// 为指定建筑分配工人，影响生产效率
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        /// <param name="workerCount">要分配的工人数</param>
        void AssignWorkersToBuilding(string buildingId, string workerId);


        // ===== 信息查询 =====

        /// <summary>
        /// 获取指定ID的建筑数据
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        /// <returns>建筑数据对象</returns>
        BuildingData GetBuilding(string buildingId);

        /// <summary>
        /// 获取所有建筑的数据列表
        /// </summary>
        /// <returns>所有建筑的列表</returns>
        List<BuildingData> GetAllBuildings();

        /// <summary>
        /// 根据建筑分类获取建筑列表（如生产类、防御类等）
        /// </summary>
        /// <param name="category">建筑分类</param>
        /// <returns>符合分类的建筑列表</returns>
        List<BuildingData> GetBuildingsByCategory(BuildingCategory category);

        /// <summary>
        /// 获取指定半径内存在的所有建筑
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">搜索半径</param>
        /// <returns>在范围内的建筑列表</returns>
        List<BuildingData> GetBuildingsInRadius(Vector3 center, float radius);

        /// <summary>
        /// 获取当前已建造的某类建筑的数量
        /// </summary>
        /// <param name="buildingType">建筑类型</param>
        /// <returns>该类型建筑的数量</returns>
        int GetBuildingCount(string buildingType);

        /// <summary>
        /// 计算所有运作中建筑对该资源类型的总产量
        /// </summary>
        /// <param name="resourceType">资源类型</param>
        /// <returns>该资源的总产出量</returns>
        float GetTotalProduction(ResourceType resourceType);


        // ===== 建筑状态 =====

        /// <summary>
        /// 检查指定建筑是否处于可运行状态（已建成且不在建造中）
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        /// <returns>是否可运行</returns>
        bool IsBuildingOperational(string buildingId);

        /// <summary>
        /// 获取指定建筑的生产效率（基于血量、工人数量和科技加成）
        /// </summary>
        /// <param name="buildingId">建筑唯一标识符</param>
        /// <returns>效率值（0.0f ~ 1.0f 或更高）</returns>
        float GetBuildingEfficiency(string buildingId);

        /// <summary>
        /// 获取下一个适合建造该类型建筑的位置（自动搜索周围可用网格）
        /// </summary>
        /// <param name="buildingType">建筑类型</param>
        /// <returns>推荐的建造位置</returns>
        Vector3 GetNextBuildPosition(string buildingType);


        // ===== 系统管理 =====

        /// <summary>
        /// 每帧更新逻辑（处理建造进度、生产周期等）
        /// </summary>
        void Update();
    }


    /// <summary>
    /// 增强建筑系统实现
    /// </summary>
    public class EnhancedBuildingSystem : AbstractSystem, IEnhancedBuildingSystem
    {
        private ISurvivalGameModel gameModel;
        private IResourceSystem resourceSystem;
        private IAdvancedTechSystem techSystem;
        private ConfigSystem configSystem;
        private ISurvivorSystem  survivorSystem;
        private IGameModel mGameModel;
        
        // 建筑管理
        private Dictionary<string, GameObject> buildingObjects;
        private Dictionary<string, float> constructionProgress;
        private Dictionary<string, float> productionTimers;
        //private Dictionary<string, int> assignedWorkers;
        
        // 系统参数
        private const float GRID_SIZE = 2f;
        private const float CONSTRUCTION_UPDATE_INTERVAL = 0.5f;
        private const float PRODUCTION_UPDATE_INTERVAL = 1f;
        
        // 更新计时
        private float lastConstructionUpdate;
        private float lastProductionUpdate;
        
        protected override void OnInit()
        {
            // 获取系统引用
            gameModel = this.GetModel<ISurvivalGameModel>();
            resourceSystem = this.GetSystem<IResourceSystem>();
            techSystem = this.GetSystem<IAdvancedTechSystem>();
            configSystem = this.GetSystem<ConfigSystem>();
            survivorSystem  = this.GetSystem<ISurvivorSystem>();
            mGameModel = this.GetModel<IGameModel>();
            // 初始化容器
            buildingObjects = new Dictionary<string, GameObject>();     // 存储建筑ID与对应GameObject的映射：用于管理建筑在Unity场景中的可视化对象
            constructionProgress = new Dictionary<string, float>();      // 存储建筑ID与建造进度的映射：记录正在建造中的建筑完成度（0.0f ~ 1.0f）
            productionTimers = new Dictionary<string, float>();          // 存储建筑ID与生产计时器的映射：用于控制资源产出的时间间隔
            
            //assignedWorkers = new Dictionary<string, int>();             // 存储建筑ID与分配工人数量的映射：表示每个建筑当前分配了多少工人

            
            // 初始化时间
            lastConstructionUpdate = Time.time;
            lastProductionUpdate = Time.time;
            
            Debug.Log("增强建筑系统初始化完成");
            
            // 注册事件
            // this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);
        }

        #region 建造管理
        
        public bool CanBuildAt(Vector3 position, string buildingType)
        {
            // 检查建筑配置
            var config = configSystem.GetBuildingConfig(buildingType);
            if (config == null)
            {
                Debug.LogWarning($"未找到建筑配置: {buildingType}");
                return false;
            }
            
            // 检查科技需求
            if (config.RequiredTechs.Count > 0 && !AreTechRequirementsMet(config.RequiredTechs))
            {
                Debug.LogWarning($"科技需求未满足: {buildingType}");
                return false;
            }
            
            // 检查位置是否可用
            Vector2Int buildingSize = config.Size;
            if (IsAreaOccupied(position, buildingSize))
            {
                Debug.LogWarning($"建筑区域被占用: {position}, 尺寸: {buildingSize}");
                return false;
            }
            
            // 检查资源
            foreach (var cost in config.BuildCosts)
            {
                if (gameModel.GetResourceAmount(cost.Type) < cost.Amount)
                {
                    Debug.LogWarning($"资源不足，需要: {cost.Amount} {cost.Type}");
                return false;
                }
            }
            
            // 检查建筑数量限制
            if (GetBuildingCount(buildingType) >= GetMaxBuildingCount(buildingType))
            {
                Debug.LogWarning($"已达到最大建筑数量: {buildingType}");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 开始建造指定类型的建筑
        /// </summary>
        /// <param name="position">尝试建造的位置</param>
        /// <param name="buildingType">要建造的建筑类型（对应配置ID）</param>
        /// <returns>是否成功开始建造</returns>
        public bool StartConstruction(Vector3 position, string buildingType)
        {
            // 检查是否可以在该位置建造此类型的建筑
            if (!CanBuildAt(position, buildingType))
                return false;

            // 获取建筑配置数据
            var config = configSystem.GetBuildingConfig(buildingType);

            // 扣除建造所需资源
            foreach (var cost in config.BuildCosts)
            {
                gameModel.ConsumeResource(cost.Type, cost.Amount);
            }

            // 创建建筑数据对象，记录建筑的基本信息
            var buildingData = new BuildingData
            {
                Id = System.Guid.NewGuid().ToString(),     // 生成唯一ID
                ConfigId = buildingType,                  // 建筑配置ID
                Position = SnapToGrid(position),           // 将位置对齐到网格系统
                State = BuildingState.UnderConstruction,  // 初始状态为"正在建造"
                Health = 50f,                             // 建造期间血量较低
                MaxHealth = 100f,                         // 最大血量
                BuildProgress = 0f                        // 初始建造进度为0
            };

            // 将建筑加入游戏模型中（用于保存和管理）
            gameModel.AddBuilding(buildingData);

            // 创建对应的Unity GameObject，用于可视化显示
            CreateBuildingGameObject(buildingData);

            // 初始化该建筑在系统的各项状态数据
            constructionProgress[buildingData.Id] = 0f;  // 建造进度初始化为0
            productionTimers[buildingData.Id] = 0f;      // 生产计时器初始化为0
            //assignedWorkers[buildingData.Id] = 0;         // 默认没有分配工人

            // 输出调试日志，提示开始建造
            Debug.Log($"开始建造 {config.Name} 在位置 {buildingData.Position}");

            // 返回成功建造标志
            return true;
        }

        
        public void CompleteConstruction(string buildingId)
        {
            if (!gameModel.Buildings.ContainsKey(buildingId))
                return;
            
            var building = gameModel.Buildings[buildingId];
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            
            // 完成建造
            building.State = BuildingState.Operational;
            building.Health = building.MaxHealth;
            building.BuildProgress = 1f;
            mGameModel.SurvivorData.buildingWorkers.Add(buildingId, new List<string>());
            
            // 清理建造数据
            constructionProgress.Remove(buildingId);
            
            // 更新建筑外观
            UpdateBuildingVisual(buildingId);
            
            Debug.Log($"建筑 {config.Name} 建造完成");
        }
        
        public bool CancelConstruction(string buildingId)
        {
            if (!constructionProgress.ContainsKey(buildingId))
                return false;
            
            var building = gameModel.Buildings[buildingId];
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            
            // 返还部分材料
            foreach (var cost in config.BuildCosts)
            {
                int refund = Mathf.RoundToInt(cost.Amount * 0.7f);
                gameModel.AddResource(cost.Type, refund);
            }
            
            // 清理数据
            constructionProgress.Remove(buildingId);
            productionTimers.Remove(buildingId);
            mGameModel.SurvivorData.buildingWorkers.Remove(buildingId);
            //mGameModel.SurvivorData.assignedWorkers.Remove(buildingId);
            
            // 销毁游戏对象
            if (buildingObjects.ContainsKey(buildingId))
            {
                UnityEngine.Object.Destroy(buildingObjects[buildingId]);
                buildingObjects.Remove(buildingId);
            }
            
            // 从模型移除
            gameModel.RemoveBuilding(buildingId);
            
            Debug.Log($"取消建造 {config.Name}");
            return true;
        }
        
        #endregion

        #region 建筑操作
        
        public bool UpgradeBuilding(string buildingId)
        {
            if (!gameModel.Buildings.ContainsKey(buildingId))
                return false;
            
            var building = gameModel.Buildings[buildingId];
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            
            // 查找升级配置
            var upgradeConfig = FindUpgradeConfig(building.ConfigId);
            if (upgradeConfig == null)
            {
                Debug.LogWarning($"建筑无法升级: {config.Name}");
                return false;
            }
            
            // 检查资源
            foreach (var cost in upgradeConfig.UpgradeCosts)
            {
                if (gameModel.GetResourceAmount(cost.Type) < cost.Amount)
                {
                    Debug.LogWarning($"升级资源不足，需要: {cost.Amount} {cost.Type}");
                return false;
                }
            }
            
            // 消耗资源并升级建筑
            foreach (var cost in upgradeConfig.UpgradeCosts)
            {
                gameModel.ConsumeResource(cost.Type, cost.Amount);
            }
            
            building.ConfigId = upgradeConfig.ToConfigId;
            building.Health = building.MaxHealth;
            building.State = BuildingState.Operational;
            
            // 更新外观
            UpdateBuildingVisual(buildingId);
            
            Debug.Log($"建筑 {config.Name} 升级完成");
            return true;
        }
        
        public bool DemolishBuilding(string buildingId)
        {
            if (!gameModel.Buildings.ContainsKey(buildingId))
                return false;
            
            var building = gameModel.Buildings[buildingId];
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            
            // 返还材料
            foreach (var cost in config.BuildCosts)
            {
                int refund = cost.Amount / 3;
                gameModel.AddResource(cost.Type, refund);
            }
            
            // 清理所有相关数据
            constructionProgress.Remove(buildingId);
            productionTimers.Remove(buildingId);
            mGameModel.SurvivorData.buildingWorkers.Remove(buildingId);
            //assignedWorkers.Remove(buildingId);
            
            // 销毁游戏对象
            if (buildingObjects.ContainsKey(buildingId))
            {
                UnityEngine.Object.Destroy(buildingObjects[buildingId]);
                buildingObjects.Remove(buildingId);
            }
            
            // 从模型移除
            gameModel.RemoveBuilding(buildingId);
            
            Debug.Log($"拆除建筑 {config.Name}");
            return true;
        }
        
        public bool RepairBuilding(string buildingId)
        {
            if (!gameModel.Buildings.ContainsKey(buildingId))
                return false;
            
            var building = gameModel.Buildings[buildingId];
            
            if (building.Health >= building.MaxHealth)
                return false;
            
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            
            // 计算修复成本
            float damagePercent = (building.MaxHealth - building.Health) / building.MaxHealth;
            var repairCosts = new List<ResourceCost>();
            
            foreach (var cost in config.BuildCosts)
            {
                int repairAmount = Mathf.RoundToInt(cost.Amount * damagePercent * 0.5f);
                if (repairAmount > 0)
                {
                    repairCosts.Add(new ResourceCost { Type = cost.Type, Amount = repairAmount });
                }
            }
            
            // 检查资源
            foreach (var cost in repairCosts)
            {
                if (gameModel.GetResourceAmount(cost.Type) < cost.Amount)
                return false;
            }
            
            // 修复建筑
            foreach (var cost in repairCosts)
            {
                gameModel.ConsumeResource(cost.Type, cost.Amount);
            }
            
            building.Health = building.MaxHealth;
            building.State = BuildingState.Operational;
            
            UpdateBuildingVisual(buildingId);
            
            Debug.Log($"修复建筑 {config.Name}");
            return true;
        }
        
        public bool ToggleBuildingOperation(string buildingId)
        {
            if (!gameModel.Buildings.ContainsKey(buildingId))
                return false;
            
            var building = gameModel.Buildings[buildingId];
            
            // 只有完全建成且健康的建筑才能切换操作状态
            if (building.State == BuildingState.UnderConstruction || building.Health < 20f)
                return false;
            
            building.State = building.State == BuildingState.Operational ? BuildingState.Damaged : BuildingState.Operational;
            UpdateBuildingVisual(buildingId);
            
            Debug.Log($"建筑 {building.ConfigId} 操作状态: {building.State}");
            return true;
        }
        
        #endregion

        #region 生产管理
        
        public void UpdateBuildingProduction(float deltaTime)
        {
            foreach (var building in gameModel.Buildings.Values)
            {
                if (building.State != BuildingState.Operational || constructionProgress.ContainsKey(building.Id))
                    continue;
                
                UpdateSingleBuildingProduction(building, deltaTime);
            }
        }
        
        private void UpdateSingleBuildingProduction(BuildingData building, float deltaTime)
        {
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            if (config == null || config.Productions.Count == 0)
                return;
            
            // 更新生产计时器
            if (!productionTimers.ContainsKey(building.Id))
                productionTimers[building.Id] = 0f;
            
            productionTimers[building.Id] += deltaTime;
            
            // 检查是否到达生产周期
            float productionCycle = 3f; // 1小时 = 3600秒
            if (productionTimers[building.Id] >= productionCycle)
            {
                ProduceBuildingOutput(building);
                productionTimers[building.Id] = 0f;
            }
        }
        
        /// <summary>
        /// 根据建筑配置和当前状态生产资源输出
        /// </summary>
        /// <param name="building">需要生产资源的建筑数据</param>
        private void ProduceBuildingOutput(BuildingData building)
        {
            // 获取该建筑的实际配置信息
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            
            // 获取当前建筑的生产效率（基于血量、工人数量和科技加成）
            float efficiency = GetBuildingEfficiency(building.Id);

            // 遍历所有可能的产出项
            foreach (var production in config.Productions)
            {
                // 如果该产出需要工人但当前没有分配工人，则跳过
                if (production.RequiresWorker && mGameModel.SurvivorData.buildingWorkers[building.Id].Count == 0)
                    continue;

                // 检查是否满足输入资源需求
                bool canProduce = true;
                foreach (var inputCost in production.InputCosts)
                {
                    if (gameModel.GetResourceAmount(inputCost.Type) < inputCost.Amount)
                    {
                        canProduce = false;
                        break;
                    }
                }

                if (!canProduce) continue;

                // 扣除所需输入资源
                foreach (var inputCost in production.InputCosts)
                {
                    gameModel.ConsumeResource(inputCost.Type, inputCost.Amount);
                }

                // 计算当前工人数量
                int workerCount = mGameModel.SurvivorData.buildingWorkers[building.Id].Count;
                
                // 根据基础产出率和工人加成计算总产出率
                float totalRate = production.BaseRate + (production.WorkerBonus * workerCount);
                
                // 应用效率影响后计算实际产出数量
                int actualOutput = Mathf.RoundToInt(totalRate * efficiency);

                // 将最终产出资源加入游戏模型
                gameModel.AddResource(production.Type, actualOutput);
                
                // 显示漂浮数字效果
                ShowFloatingText(building.Position, $"+{actualOutput}", GetResourceColor(production.Type));
                
                Debug.Log("产出资源"+production.Type+"数量"+actualOutput);
            }
        }

        
        public void CollectBuildingOutput(string buildingId)
        {
            if (gameModel.Buildings.ContainsKey(buildingId))
            {
                var building = gameModel.Buildings[buildingId];
                ProduceBuildingOutput(building);
            }
        }
        
        public void AssignWorkersToBuilding(string buildingId, string workerID)
        {
            if (!gameModel.Buildings.ContainsKey(buildingId))
                return;
            
            var building = gameModel.Buildings[buildingId];
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            
            mGameModel.SurvivorData.buildingWorkers[buildingId].Add(workerID);
            
            
            Debug.Log($"为建筑 {building.ConfigId} 分配 id为  {workerID} 的工人");
        }
        
        #endregion

        #region 信息查询
        
        public BuildingData GetBuilding(string buildingId)
        {
            return gameModel.Buildings.ContainsKey(buildingId) ? gameModel.Buildings[buildingId] : null;
        }
        
        public List<BuildingData> GetAllBuildings()
        {
            return gameModel.Buildings.Values.ToList();
        }
        
        public List<BuildingData> GetBuildingsByCategory(BuildingCategory category)
        {
            return gameModel.Buildings.Values
                .Where(b => {
                    var config = configSystem.GetBuildingConfig(b.ConfigId);
                    return config != null && config.Category == category;
                })
                .ToList();
        }
        
        public List<BuildingData> GetBuildingsInRadius(Vector3 center, float radius)
        {
            return gameModel.Buildings.Values
                .Where(b => Vector3.Distance(b.Position, center) <= radius)
                .ToList();
        }
        
        public int GetBuildingCount(string buildingType)
        {
            return gameModel.Buildings.Values.Count(b => b.ConfigId == buildingType);
        }
        
        public float GetTotalProduction(ResourceType resourceType)
        {
            float total = 0f;
            
            foreach (var building in gameModel.Buildings.Values)
            {
                if (building.State != BuildingState.Operational) continue;
                
                var config = configSystem.GetBuildingConfig(building.ConfigId);
                if (config == null) continue;
                
                float efficiency = GetBuildingEfficiency(building.Id);
                int workerCount = mGameModel.SurvivorData.buildingWorkers[building.Id].Count;
                
                foreach (var production in config.Productions)
                        {
                    if (production.Type == resourceType)
                    {
                        float totalRate = production.BaseRate + (production.WorkerBonus * workerCount);
                        total += totalRate * efficiency;
                        }
                }
            }
            
            return total;
        }
        
        #endregion

        #region 建筑状态
        
        public bool IsBuildingOperational(string buildingId)
        {
            if (!gameModel.Buildings.ContainsKey(buildingId))
                return false;
            
            var building = gameModel.Buildings[buildingId];
            return building.State == BuildingState.Operational && !constructionProgress.ContainsKey(buildingId);
        }
        
        public float GetBuildingEfficiency(string buildingId)
        {
            if (!gameModel.Buildings.ContainsKey(buildingId))
                return 0f;
            
            var building = gameModel.Buildings[buildingId];
            
            // 基础效率（基于血量）
            float healthEfficiency = building.Health / building.MaxHealth;
            
            // 工人效率
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            int workers = mGameModel.SurvivorData.buildingWorkers[buildingId].Count;
            int maxWorkers = config?.MaxWorkers ?? 1;
            float workerEfficiency = maxWorkers > 0 ? (float)workers / maxWorkers : 1f;
            
            // 科技加成
            float techBonus = GetTechProductionBonus(building.ConfigId);
            
            return healthEfficiency * workerEfficiency * (1f + techBonus);
        }
        
        public Vector3 GetNextBuildPosition(string buildingType)
        {
            Vector3 basePosition = Vector3.zero;
            
            for (int radius = 1; radius <= 10; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        Vector3 testPos = basePosition + new Vector3(x * GRID_SIZE, y * GRID_SIZE, 0);
                        if (CanBuildAt(testPos, buildingType))
                        {
                            return testPos;
                        }
                    }
                }
            }
            
            return basePosition;
        }

        public void Update()
        {
            
            float currentTime = Time.time;
            
            // 更新建造进度
            if (currentTime - lastConstructionUpdate >= CONSTRUCTION_UPDATE_INTERVAL)
            {
                UpdateConstruction(CONSTRUCTION_UPDATE_INTERVAL);
                lastConstructionUpdate = currentTime;
            }
            
            // 更新生产
            if (currentTime - lastProductionUpdate >= PRODUCTION_UPDATE_INTERVAL)
            {
                UpdateBuildingProduction(PRODUCTION_UPDATE_INTERVAL);
                lastProductionUpdate = currentTime;
            }
            
            Debug.Log("Update");
        }

        #endregion

        #region 系统管理
        
        // private void OnGameUpdate(GameUpdateEvent e)
        // {
        //     float currentTime = e.DeltaTime;
        //     
        //     // 更新建造进度
        //     if (currentTime - lastConstructionUpdate >= CONSTRUCTION_UPDATE_INTERVAL)
        //     {
        //         UpdateConstruction(CONSTRUCTION_UPDATE_INTERVAL);
        //         lastConstructionUpdate = currentTime;
        //     }
        //     
        //     // 更新生产
        //     if (currentTime - lastProductionUpdate >= PRODUCTION_UPDATE_INTERVAL)
        //     {
        //         UpdateBuildingProduction(PRODUCTION_UPDATE_INTERVAL);
        //         lastProductionUpdate = currentTime;
        //     }
        // }
        
        private void UpdateConstruction(float deltaTime)
        {
            var completedBuildings = new List<string>();
            
            foreach (var kvp in constructionProgress.ToList())
            {
                string buildingId = kvp.Key;
                float progress = kvp.Value;
                
                var building = gameModel.Buildings[buildingId];
                var config = configSystem.GetBuildingConfig(building.ConfigId);
                
                // 计算建造效率
                float constructionRate = 1f / config.BuildTime;
                
                // 更新进度
                progress += deltaTime * constructionRate;
                constructionProgress[buildingId] = progress;
                building.BuildProgress = progress;
                
                // 检查是否完成
                if (progress >= 1f)
                {
                    completedBuildings.Add(buildingId);
                }
            }
            
            // 完成建造
            foreach (string buildingId in completedBuildings)
            {
                CompleteConstruction(buildingId);
            }
        }
        
        #endregion

        #region 辅助方法
        
        /// <summary>
        /// 显示漂浮文字效果
        /// </summary>
        /// <param name="position">显示位置</param>
        /// <param name="text">显示文本</param>
        /// <param name="color">文字颜色</param>
        private void ShowFloatingText(Vector3 position, string text, Color color)
        {
            // 创建漂浮文字GameObject
            GameObject floatingTextGO = new GameObject("FloatingText");
            floatingTextGO.transform.position = position + Vector3.up * 0.5f; // 稍微向上偏移
            
            // 添加Canvas组件用于UI渲染
            Canvas canvas = floatingTextGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10; // 确保在建筑上方显示
            
            // 添加CanvasScaler
            CanvasScaler canvasScaler = floatingTextGO.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 20f; // 增加像素密度，让文字更小
            
            // 创建文字对象
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(floatingTextGO.transform);
            
            // 添加Text组件
            UnityEngine.UI.Text textComponent = textGO.AddComponent<UnityEngine.UI.Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 3; // 减小字体大小从24到12
            textComponent.color = color;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.fontStyle = FontStyle.Bold;
            
            // 设置RectTransform
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(20, 7.5f); // 减小尺寸从200x50到80x30
            textRect.anchoredPosition = Vector2.zero;
            
            // 添加简单的销毁逻辑和动画效果
            StartFloatingAnimation(floatingTextGO, 1.5f);
        }
        
        /// <summary>
        /// 启动漂浮动画效果
        /// </summary>
        /// <param name="floatingTextGO">漂浮文字对象</param>
        /// <param name="duration">持续时间</param>
        private void StartFloatingAnimation(GameObject floatingTextGO, float duration)
        {
            // 添加简单的漂浮动画组件
            SimpleFloatingText floatingComponent = floatingTextGO.AddComponent<SimpleFloatingText>();
            floatingComponent.Initialize(duration);
        }
        
        /// <summary>
        /// 根据资源类型获取对应的颜色
        /// </summary>
        /// <param name="resourceType">资源类型</param>
        /// <returns>对应的颜色</returns>
        private Color GetResourceColor(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.Food:
                    return new Color(0.8f, 0.6f, 0.2f); // 橙色
                case ResourceType.Water:
                    return new Color(0.2f, 0.6f, 1f); // 蓝色
                case ResourceType.Materials:
                    return new Color(0.6f, 0.4f, 0.2f); // 棕色
                case ResourceType.Ammunition:
                    return new Color(1f, 0.2f, 0.2f); // 红色
                case ResourceType.Energy:
                    return new Color(1f, 1f, 0.2f); // 黄色
                case ResourceType.ResearchPoints:
                    return new Color(0.6f, 0.2f, 1f); // 紫色
                default:
                    return Color.white;
            }
        }
        
        private Vector3 SnapToGrid(Vector3 position)
        {
            float snappedX = Mathf.Round(position.x / GRID_SIZE) * GRID_SIZE;
            float snappedY = Mathf.Round(position.y / GRID_SIZE) * GRID_SIZE;
            return new Vector3(snappedX, snappedY, 0);
        }
        
        private bool IsAreaOccupied(Vector3 centerPosition, Vector2Int buildingSize)
        {
            Vector3 gridCenter = SnapToGrid(centerPosition);
            
            // 计算建筑占用的网格范围
            int halfWidth = buildingSize.x / 2;
            int halfHeight = buildingSize.y / 2;
            
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    Vector3 testPos = gridCenter + new Vector3(x * GRID_SIZE, y * GRID_SIZE, 0);
                    
                    foreach (var building in gameModel.Buildings.Values)
                    {
                        if (Vector3.Distance(building.Position, testPos) < GRID_SIZE * 0.5f)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        private bool AreTechRequirementsMet(List<string> requiredTechs)
        {
            if (techSystem == null) return true;
            
            foreach (var techId in requiredTechs)
            {
                if (!techSystem.IsTechResearched(techId))
                    return false;
            }
            return true;
        }
        
        private int GetMaxBuildingCount(string buildingType)
        {
            // 根据建筑类型返回最大数量限制
            var config = configSystem.GetBuildingConfig(buildingType);
            if (config == null) return 1;
            
            switch (config.Category)
            {
                case BuildingCategory.Defense: return 100;
                case BuildingCategory.Production: return 20;
                case BuildingCategory.Habitat: return 15;
                case BuildingCategory.Storage: return 10;
                case BuildingCategory.Functional: return 5;
                default: return 10;
            }
        }
        
        private BuildingUpgrade FindUpgradeConfig(string buildingId)
            {
            // 查找可能的升级配置
            var allUpgrades = configSystem.GetAllBuildingConfigs();
            foreach (var config in allUpgrades.Values)
            {
                var upgrade = configSystem.GetBuildingUpgrade(buildingId, config.ConfigId);
                if (upgrade != null)
                    return upgrade;
            }
            return null;
        }
        
        private float GetTechProductionBonus(string buildingType)
        {
            if (techSystem == null) return 0f;
            
            // 根据建筑类型获取科技加成
            float bonus = 0f;
            var config = configSystem.GetBuildingConfig(buildingType);
            if (config == null) return bonus;
            
            switch (config.Category)
            {
                case BuildingCategory.Production:
                    bonus += techSystem.GetTechEffectValue("production_efficiency", 0f);
                    break;
                case BuildingCategory.Defense:
                    bonus += techSystem.GetTechEffectValue("defense_efficiency", 0f);
                    break;
            }
            
            return bonus;
        }
        
        private void CreateBuildingGameObject(BuildingData building)
        {
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            
            // 创建游戏对象
            GameObject buildingGO = new GameObject($"Building_{building.ConfigId}_{building.Id}");
            buildingGO.transform.position = building.Position;
            
            // 添加2D渲染
            var spriteRenderer = buildingGO.AddComponent<SpriteRenderer>();
            var texture = CreateBuildingTexture(building.ConfigId);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                                     new Vector2(0.5f, 0.5f), 32f);
            
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = 1;
            spriteRenderer.sortingLayerName = "Buildings";
            
            // 设置颜色表示建造状态
            if (constructionProgress.ContainsKey(building.Id))
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.6f);
            }
            
            buildingObjects[building.Id] = buildingGO;
        }
        
        private Texture2D CreateBuildingTexture(string buildingType)
        {
            Texture2D texture = new Texture2D(32, 32);
            Color buildingColor = GetBuildingColor(buildingType);
            
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    if (x < 2 || x >= texture.width - 2 || y < 2 || y >= texture.height - 2)
                    {
                        texture.SetPixel(x, y, buildingColor * 0.8f);
                    }
                    else
                    {
                        texture.SetPixel(x, y, buildingColor);
                    }
                }
            }
            
            texture.Apply();
            return texture;
        }
        
        private Color GetBuildingColor(string buildingType)
        {
            var config = configSystem.GetBuildingConfig(buildingType);
            if (config == null) return Color.white;
            
            switch (config.Category)
            {
                case BuildingCategory.Habitat: return Color.blue;
                case BuildingCategory.Production: return Color.green;
                case BuildingCategory.Defense: return Color.red;
                case BuildingCategory.Storage: return new Color(1f, 0.5f, 0f); // orange
                case BuildingCategory.Functional: return Color.magenta;
                default: return Color.white;
            }
        }
        
        private void UpdateBuildingVisual(string buildingId)
        {
            if (!buildingObjects.ContainsKey(buildingId))
                return;
            
            var building = gameModel.Buildings[buildingId];
            var spriteRenderer = buildingObjects[buildingId].GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null) return;
            
            // 根据建筑状态更新颜色
            switch (building.State)
            {
                case BuildingState.UnderConstruction:
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.6f);
                    break;
                case BuildingState.Damaged:
                spriteRenderer.color = Color.gray;
                    break;
                case BuildingState.Destroyed:
                spriteRenderer.color = Color.red;
                    break;
                case BuildingState.Operational:
                    if (building.Health < 50f)
                        spriteRenderer.color = Color.yellow;
            else
                spriteRenderer.color = Color.white;
                    break;
                default:
                    spriteRenderer.color = Color.white;
                    break;
            }
        }
        
        #endregion
    }

    /// <summary>
    /// 简单的漂浮文字组件，用于显示生产资源数量
    /// </summary>
    public class SimpleFloatingText : MonoBehaviour
    {
        private float duration;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private Text textComponent;
        private Color startColor;
        private Color endColor;
        private float elapsed;
        
        /// <summary>
        /// 初始化漂浮文字效果
        /// </summary>
        /// <param name="duration">持续时间</param>
        public void Initialize(float duration)
        {
            this.duration = duration;
            this.elapsed = 0f;
            
            // 获取文字组件
            textComponent = GetComponentInChildren<Text>();
            if (textComponent == null)
            {
                Debug.LogError("SimpleFloatingText: 未找到Text组件");
                Destroy(gameObject);
                return;
            }
            
            // 设置起始和结束位置
            startPosition = transform.position;
            endPosition = startPosition + Vector3.up * 1f; // 向上漂浮1个单位，减少移动距离
            
            // 设置起始和结束颜色
            startColor = textComponent.color;
            endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 透明
        }
        
        private void Update()
        {
            if (textComponent == null) return;
            
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            if (progress >= 1f)
            {
                // 动画完成，销毁对象
                Destroy(gameObject);
                return;
            }
            
            // 位置插值（向上漂浮）
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            
            // 颜色插值（淡出效果）
            textComponent.color = Color.Lerp(startColor, endColor, progress);
        }
    }
} 