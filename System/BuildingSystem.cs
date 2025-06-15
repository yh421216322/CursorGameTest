// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：BuildingSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件实现了建筑系统 (BuildingSystem)，负责管理游戏中的所有建筑相关的操作，
//     包括建筑的建造、升级、拆除、工人分配、状态更新、资源生产和维护等。
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
    /// 建筑系统接口。
    /// 定义了建筑系统的核心功能，如开始建造、分配工人、拆除建筑、查询建筑信息等。
    /// </summary>
    public interface IBuildingSystem : QFISystem
    {
        /// <summary>
        /// 开始建造一个新的建筑。
        /// </summary>
        /// <param name="configId">要建造的建筑的配置ID。</param>
        /// <param name="position">建筑的建造位置。</param>
        /// <returns>如果成功开始建造则返回true，否则返回false。</returns>
        public bool StartConstruction(string configId, Vector3 position);
        /// <summary>
        /// 为指定建筑分配一名工人。
        /// </summary>
        /// <param name="buildingId">建筑的唯一ID。</param>
        /// <param name="workerId">工人的唯一ID。</param>
        /// <returns>如果成功分配则返回true，否则返回false。</returns>
        public bool AssignWorker(string buildingId, string workerId);
        /// <summary>
        /// 拆除指定的建筑。
        /// </summary>
        /// <param name="buildingId">要拆除的建筑的唯一ID。</param>
        /// <returns>如果成功拆除则返回true，否则返回false。</returns>
        public bool DemolishBuilding(string buildingId);
        /// <summary>
        /// 获取所有当前存在的建筑数据。
        /// </summary>
        /// <returns>一个包含所有建筑数据的字典，键为建筑ID。</returns>
        public Dictionary<string, BuildingData> GetAllBuildings();
        /// <summary>
        /// 获取指定类型或所有类型的建筑数量。
        /// </summary>
        /// <param name="configId">可选参数，指定建筑配置ID以统计特定类型建筑的数量。如果为null，则返回所有建筑的总数。</param>
        /// <returns>建筑的数量。</returns>
        public int GetBuildingCount(string configId = null);
        /// <summary>
        /// 根据建筑类别获取该类别下的所有建筑。
        /// </summary>
        /// <param name="category">建筑类别。</param>
        /// <returns>属于指定类别的建筑数据列表。</returns>
        public List<BuildingData> GetBuildingsByCategory(BuildingCategory category);
        /// <summary>
        /// 获取分配给指定建筑的工人ID列表。
        /// </summary>
        /// <param name="buildingId">建筑的唯一ID。</param>
        /// <returns>工人ID列表。</returns>
        public List<string> GetBuildingWorkers(string buildingId);
        /// <summary>
        /// 尝试升级指定的建筑。
        /// </summary>
        /// <param name="buildingId">要升级的建筑的唯一ID。</param>
        /// <returns>如果成功开始升级则返回true，否则返回false。</returns>
        public bool UpgradeBuilding(string buildingId);
    }

    /// <summary>
    /// 建筑系统 - 管理所有建筑的建造、运行和维护。
    /// 实现了IBuildingSystem接口。
    /// </summary>
    public class BuildingSystem : AbstractSystem,IBuildingSystem
    {
        #region 数据存储 (Data Storage)

        /// <summary>
        /// 存储所有建筑实例的字典，键为建筑ID。
        /// </summary>
        private Dictionary<string, BuildingData> _buildings;
        /// <summary>
        /// 建筑队列，存储正在建造或升级中的建筑ID。
        /// </summary>
        private List<string> _buildingQueue;
        /// <summary>
        /// 工人分配记录字典，键为建筑ID，值为分配给该建筑的工人ID列表。
        /// </summary>
        private Dictionary<string, List<string>> _workerAssignments; // 建筑ID -> 工人ID列表

        #endregion

        #region 系统初始化 (System Initialization)

        /// <summary>
        /// 系统初始化方法。
        /// 在此方法中初始化建筑数据存储结构，并注册游戏更新事件。
        /// </summary>
        protected override void OnInit()
        {
            _buildings = new Dictionary<string, BuildingData>();
            _buildingQueue = new List<string>();
            _workerAssignments = new Dictionary<string, List<string>>();
            Debug.Log("[建筑系统] 初始化完成"); // 日志：初始化建筑系统

            // 注册游戏更新事件，以便每帧更新建筑状态
            this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);
        }

        #endregion

        #region 建筑建造 (Building Construction)

        /// <summary>
        /// 开始建造一个新的建筑。
        /// </summary>
        /// <param name="configId">要建造的建筑的配置ID。</param>
        /// <param name="position">建筑的建造位置。</param>
        /// <returns>如果成功开始建造则返回true，否则返回false。</returns>
        public bool StartConstruction(string configId, Vector3 position)
        {
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var resourceSystem = this.GetSystem<ResourceSystem>(); // 获取资源系统

            var config = configSystem.GetBuildingConfig(configId); // 获取建筑配置
            if (config == null)
            {
                Debug.LogError($"[建筑系统] 找不到建筑配置ID: {configId}");
                return false;
            }

            // 检查科技前置条件 (示例代码，需要TechSystem和ConfigSystem中的具体实现)
            // var techSystem = this.GetSystem<AdvancedTechSystem>();
            // var researchedTechIds = techSystem.GetResearchedTechs().Select(t => t.Id).ToList();
            // if (config.RequiredTechs != null && config.RequiredTechs.Any(reqTechId => !researchedTechIds.Contains(reqTechId)))
            // {
            //     Debug.LogWarning($"[建筑系统] 建筑 {config.Name} (ID: {configId}) 的科技前置条件未满足。");
            //     return false;
            // }

            // 检查资源是否足够
            if (!resourceSystem.CanAfford(config.BuildCosts))
            {
                Debug.LogWarning($"[建筑系统] 资源不足，无法建造 {config.Name} (ID: {configId})。");
                // TODO: 可以向UI发送资源不足的通知
                return false;
            }

            // 扣除建造成本资源
            resourceSystem.ConsumeResources(config.BuildCosts);

            // 创建建筑数据实例
            var buildingData = new BuildingData
            {
                ConfigId = configId, // 建筑配置ID
                Position = position, // 建筑位置
                State = BuildingState.UnderConstruction, // 初始状态为建造中
                MaxHealth = config.MaxHealth, // 最大生命值 (从配置读取)
                Health = config.MaxHealth * 0.1f, // 初始生命值 (例如10%，或在建造中逐渐增加)
                BuildProgress = 0f // 初始建造进度为0
            };

            _buildings[buildingData.Id] = buildingData; // 添加到建筑字典
            _buildingQueue.Add(buildingData.Id);       // 添加到建造队列
            _workerAssignments[buildingData.Id] = new List<string>(); // 初始化工人列表

            Debug.Log($"[建筑系统] 开始建造 {config.Name} (ID: {buildingData.Id}) 于位置 {position}。");
            // TODO: 发送建筑开始建造事件，通知UI等其他系统
            // this.SendEvent(new BuildingConstructionStartedEvent { BuildingId = buildingData.Id, ConfigId = configId });
            return true;
        }

        /// <summary>
        /// 每帧更新建筑的建造或升级进度。
        /// </summary>
        /// <param name="deltaTime">自上一帧以来经过的时间（秒）。</param>
        private void UpdateConstruction(float deltaTime)
        {
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var completedConstructionItems = new List<string>(); // 用于存储已完成建造/升级的建筑ID

            // 使用for循环以便在迭代过程中安全地修改_buildingQueue（尽管此处是通过completedConstructionItems延迟移除）
            for (int i = _buildingQueue.Count - 1; i >= 0; i--)
            {
                string buildingId = _buildingQueue[i];
                if (!_buildings.TryGetValue(buildingId, out var building))
                {
                    completedConstructionItems.Add(buildingId); // 如果建筑数据不存在，则从队列中移除
                    continue;
                }

                BuildingConfig currentConfig = configSystem.GetBuildingConfig(building.ConfigId);
                if (currentConfig == null)
                {
                    Debug.LogError($"[建筑系统] 找不到建筑 {building.ConfigId} 的配置信息，无法更新建造进度。");
                    completedConstructionItems.Add(buildingId); // 配置错误，移除
                    continue;
                }

                float buildTimeHours = currentConfig.BuildTime; // 获取配置中的建造时间（小时）
                if (building.State == BuildingState.Upgrading)
                {
                    // TODO: 如果升级有单独的升级时间配置，则应从升级配置中获取
                    // BuildingUpgradeInfo upgradeInfo = configSystem.GetBuildingUpgrade(originalConfigId, building.ConfigId);
                    // buildTimeHours = upgradeInfo?.UpgradeTime ?? currentConfig.BuildTime; // 假设升级时间与新建时间一致
                }
                if (buildTimeHours <= 0) buildTimeHours = 1f; // 防止除以零，默认至少1小时

                // 计算建造速度，可能受工人数量等因素影响
                float buildSpeedFactor = 1.0f; // 基础建造速度因子
                if (_workerAssignments.TryGetValue(buildingId, out var workers) && workers.Count > 0)
                {
                    // 示例：每个工人提供额外0.2的建造速度加成，上限暂不设
                    buildSpeedFactor += workers.Count * 0.2f;
                }

                // 更新建造进度： 进度增加 = (deltaTime秒 / (总时间小时 * 3600秒/小时)) * 速度因子
                building.BuildProgress += (deltaTime / (buildTimeHours * 3600f)) * buildSpeedFactor;
                building.Health = building.MaxHealth * building.BuildProgress; // 建造过程中血量随进度增加

                if (building.BuildProgress >= 1.0f)
                {
                    building.BuildProgress = 1.0f; // 确保进度不超过100%
                    building.Health = building.MaxHealth; // 满血

                    if (building.State == BuildingState.UnderConstruction)
                    {
                        building.State = BuildingState.Operational; // 状态变为可运作
                        Debug.Log($"[建筑系统] 建筑 {currentConfig.Name} (ID: {buildingId}) 建造完成。");
                        this.SendEvent(new BuildingConstructionCompletedEvent { BuildingId = buildingId, ConfigId = building.ConfigId });
                    }
                    else if (building.State == BuildingState.Upgrading)
                    {
                        // TODO: 完成升级的逻辑，例如切换到新的ConfigId（如果之前未切换）
                        // string upgradedConfigId = building.TargetUpgradeConfigId; // 假设有这样一个字段
                        // building.ConfigId = upgradedConfigId;
                        // building.State = BuildingState.Operational;
                        // Debug.Log($"[建筑系统] 建筑 {currentConfig.Name} (ID: {buildingId}) 升级至 {upgradedConfigId} 完成。");
                        // this.SendEvent(new BuildingUpgradeCompletedEvent { BuildingId = buildingId, NewConfigId = upgradedConfigId });
                         building.State = BuildingState.Operational; // 简单处理，直接变为可运作
                         Debug.Log($"[建筑系统] 建筑 {currentConfig.Name} (ID: {buildingId}) 升级完成（状态恢复运作）。");
                    }

                    completedConstructionItems.Add(buildingId); // 添加到待移除列表
                }
            }

            // 从建造队列中移除已完成的项目
            foreach (var buildingId in completedConstructionItems)
            {
                _buildingQueue.Remove(buildingId);
            }
        }

        #endregion

        #region 建筑运行 (Building Operations)

        /// <summary>
        /// 每帧更新所有运作中建筑的操作，如资源生产、维护消耗、工人经验等。
        /// </summary>
        /// <param name="deltaTime">自上一帧以来经过的时间（秒）。</param>
        private void UpdateBuildingOperations(float deltaTime)
        {
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统

            foreach (var building in _buildings.Values) // 遍历所有建筑实例
            {
                if (building.State != BuildingState.Operational) continue; // 只处理运作中的建筑

                var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
                if (config == null)
                {
                    Debug.LogError($"[建筑系统] 找不到建筑 {building.ConfigId} 的配置，无法更新其运作。");
                    continue;
                }

                UpdateResourceProduction(building, config, deltaTime); // 更新资源生产
                UpdateMaintenance(building, config, deltaTime);       // 更新维护消耗
                UpdateWorkerExperience(building, config, deltaTime);  // 更新工人经验
            }
        }

        /// <summary>
        /// 更新指定建筑的资源生产。
        /// </summary>
        /// <param name="building">要更新的建筑数据。</param>
        /// <param name="config">该建筑的配置信息。</param>
        /// <param name="deltaTime">自上一帧以来经过的时间（秒）。</param>
        private void UpdateResourceProduction(BuildingData building, BuildingConfig config, float deltaTime)
        {
            if (config.Productions == null || !config.Productions.Any()) return; // 如果没有生产配置，则跳过

            var resourceSystem = this.GetSystem<ResourceSystem>(); // 获取资源系统
            var survivorSystem = this.GetSystem<SurvivorSystem>(); // 获取幸存者系统

            foreach (var production in config.Productions) // 遍历建筑的所有生产条目
            {
                // 如果生产需要工人，但当前建筑没有分配工人，则跳过此生产条目
                if (production.RequiresWorker && (!_workerAssignments.TryGetValue(building.Id, out var assignedWorkers) || assignedWorkers.Count == 0))
                    continue;

                // 检查生产所需的输入资源是否足够
                if (production.InputCosts != null && production.InputCosts.Count > 0)
                {
                    if (!resourceSystem.CanAfford(production.InputCosts)) // 如果资源不足
                        continue; // 跳过此生产条目
                }

                // 计算基础生产效率
                float currentEfficiency = production.BaseRate; // 从配置中获取基础速率

                // 计算工人提供的额外效率加成
                if (_workerAssignments.TryGetValue(building.Id, out var workersInBuilding))
                {
                    foreach (var workerId in workersInBuilding)
                    {
                        var worker = survivorSystem.GetSurvivor(workerId); // 获取工人数据
                        if (worker != null)
                        {
                            // 累加每个工人的效率加成 (工人基础加成 * 工人特定效率修正)
                            currentEfficiency += production.WorkerBonus * GetWorkerEfficiency(worker, config);
                        }
                    }
                }

                // 计算此时间间隔内的实际产出量 (产出 = 每小时效率 * (deltaTime秒 / 3600秒/小时))
                float actualProductionAmount = currentEfficiency * (deltaTime / 3600f);

                if (actualProductionAmount > 0) // 如果有产出
                {
                    // 消耗输入资源 (如果生产有输入成本)
                    if (production.InputCosts != null && production.InputCosts.Any())
                    {
                        resourceSystem.ConsumeResources(production.InputCosts);
                    }

                    // 添加产出资源到总资源 (取整数部分)
                    resourceSystem.AddResource(production.Type, Mathf.FloorToInt(actualProductionAmount));
                    // Debug.Log($"[建筑系统] 建筑 {config.Name} 生产了 {Mathf.FloorToInt(actualProductionAmount)} 单位的 {production.Type}");
                }
            }
        }

        /// <summary>
        /// 根据工人的属性和建筑配置计算工人在该建筑中的工作效率修正值。
        /// </summary>
        /// <param name="worker">工人数据。</param>
        /// <param name="config">建筑配置。</param>
        /// <returns>工人的效率修正百分比（例如1.0代表100%效率）。</returns>
        private float GetWorkerEfficiency(SurvivorData worker, BuildingConfig config)
        {
            float efficiencyFactor = 1.0f; // 基础效率因子为100%

            // 根据建筑类别和工人相关属性调整效率因子
            // 示例：如果建筑是生产类，则生产属性高的工人效率更高
            switch (config.Category)
            {
                case BuildingCategory.Production:
                    // 获取工人的生产属性值，如果不存在则默认为20 (示例值)
                    // AttributeLevelHelper.GetEfficiencyMultiplier 将属性值转换为效率乘数
                    efficiencyFactor *= AttributeLevelHelper.GetEfficiencyMultiplier(
                        worker.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Production)?.Value ?? 20);
                    break;
                case BuildingCategory.Defense:
                    efficiencyFactor *= AttributeLevelHelper.GetEfficiencyMultiplier(
                        worker.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Combat)?.Value ?? 20);
                    break;
                case BuildingCategory.Functional: // 例如研究、医疗等
                    efficiencyFactor *= AttributeLevelHelper.GetEfficiencyMultiplier(
                        worker.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Technology)?.Value ?? 20);
                    break;
            }

            // 此处还可以加入其他影响因素，如工人心情、健康状况等
            // if (worker.Morale < 50) efficiencyFactor *= 0.8f; // 心情不好效率降低

            return Mathf.Max(0.1f, efficiencyFactor); // 确保效率至少为10%
        }

        /// <summary>
        /// 更新建筑的维护资源消耗。
        /// </summary>
        /// <param name="building">要更新的建筑数据。</param>
        /// <param name="config">该建筑的配置信息。</param>
        /// <param name="deltaTime">自上一帧以来经过的时间（秒）。</param>
        private void UpdateMaintenance(BuildingData building, BuildingConfig config, float deltaTime)
        {
            // 如果建筑没有维护成本配置，则直接返回
            if (config.MaintenanceCosts == null || !config.MaintenanceCosts.Any()) return;

            // 累积自上次维护检查以来的时间
            building.TimeSinceLastMaintenanceCheck += deltaTime;

            // 通常维护检查按固定周期进行（例如每游戏小时）
            if (building.TimeSinceLastMaintenanceCheck >= 3600f) // 3600秒 = 1游戏小时
            {
                var resourceSystem = this.GetSystem<ResourceSystem>(); // 获取资源系统

                // 检查是否有足够资源支付维护费用
                if (resourceSystem.CanAfford(config.MaintenanceCosts))
                {
                    resourceSystem.ConsumeResources(config.MaintenanceCosts); // 消耗维护资源
                    building.LastMaintenanceTime = Time.time; // 更新上次成功维护的时间戳
                    // Debug.Log($"[建筑系统] 建筑 {config.Name} 已成功维护。");
                }
                else
                {
                    // 维护资源不足，建筑可能遭受耐久度损失或效率降低
                    building.Health = Mathf.Max(0, building.Health - 5f); // 示例：每次维护失败损失5点耐久
                    Debug.LogWarning($"[建筑系统] 建筑 {config.Name} (ID: {building.Id}) 因缺乏维护资源而损坏，当前耐久: {building.Health}。");
                    if (building.Health <= 0)
                    {
                        building.State = BuildingState.Damaged; // 建筑损坏
                        Debug.LogError($"[建筑系统] 建筑 {config.Name} (ID: {building.Id}) 已完全损坏！");
                        // TODO: 发送建筑损坏事件
                    }
                }
                building.TimeSinceLastMaintenanceCheck = 0f; // 重置维护检查计时器
            }
        }

        /// <summary>
        /// 更新在建筑中工作的工人的经验值。
        /// </summary>
        /// <param name="building">工人所在的建筑数据。</param>
        /// <param name="config">该建筑的配置信息。</param>
        /// <param name="deltaTime">自上一帧以来经过的时间（秒）。</param>
        private void UpdateWorkerExperience(BuildingData building, BuildingConfig config, float deltaTime)
        {
            // 如果没有工人分配给这个建筑，或者建筑本身不提供经验，则跳过
            if (!_workerAssignments.TryGetValue(building.Id, out var workersInBuilding) || !workersInBuilding.Any()) return;

            var survivorSystem = this.GetSystem<SurvivorSystem>(); // 获取幸存者系统

            foreach (var workerId in workersInBuilding) // 遍历在该建筑工作的每个工人
            {
                var worker = survivorSystem.GetSurvivor(workerId); // 获取工人数据
                if (worker == null) continue; // 如果找不到工人数据，则跳过

                // 根据建筑类别确定工人获得的经验类型
                SurvivorAttributeType associatedAttribute = GetExperienceType(config.Category);
                // 示例：每秒为对应属性提供0.1经验值 (可调整)
                int experienceGained = Mathf.FloorToInt(deltaTime * 0.1f);

                if (experienceGained > 0)
                {
                    // survivorSystem.AddExperienceToSurvivor(workerId, experienceGained); // 增加幸存者通用经验 (如果设计有此项)
                    worker.AddAttributeExperience(associatedAttribute, experienceGained); // 为特定属性增加经验
                    // Debug.Log($"[建筑系统] 工人 {worker.Name} (ID: {workerId}) 在 {config.Name} 工作获得了 {experienceGained} 点 {associatedAttribute} 经验。");
                }
            }
        }

        /// <summary>
        /// 根据建筑类别获取工人应获得的经验类型。
        /// </summary>
        /// <param name="category">建筑的类别。</param>
        /// <returns>对应的幸存者属性类型。</returns>
        private SurvivorAttributeType GetExperienceType(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Production: return SurvivorAttributeType.Production; // 生产建筑提供生产经验
                case BuildingCategory.Defense:    return SurvivorAttributeType.Combat;     // 防御建筑提供战斗经验
                case BuildingCategory.Functional: return SurvivorAttributeType.Technology; // 功能性建筑（如研究）提供科技经验
                default: return SurvivorAttributeType.Production; // 默认或未知类型提供生产经验 (可调整)
            }
        }

        #endregion

        #region 工人分配 (Worker Assignment)

        /// <summary>
        /// 将一名工人分配到一个建筑。
        /// </summary>
        /// <param name="buildingId">目标建筑的ID。</param>
        /// <param name="workerId">要分配的工人的ID。</param>
        /// <returns>如果分配成功则返回true，否则返回false。</returns>
        public bool AssignWorker(string buildingId, string workerId)
        {
            // 检查建筑是否存在
            if (!_buildings.TryGetValue(buildingId, out var building))
            {
                Debug.LogError($"[建筑系统] 尝试向不存在的建筑 (ID: {buildingId}) 分配工人。");
                return false;
            }

            // 检查建筑状态是否允许分配工人 (例如，不能给已损坏或暂停的建筑分配)
            // 允许向建造中/升级中的建筑分配工人，他们可以加速建造/升级过程
            if (building.State != BuildingState.Operational &&
                building.State != BuildingState.UnderConstruction &&
                building.State != BuildingState.Upgrading)
            {
                Debug.LogWarning($"[建筑系统] 建筑 {building.ConfigId} (ID: {buildingId}) 当前状态 ({building.State}) 不允许分配工人。");
                return false;
            }

            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
            if (config == null)
            {
                 Debug.LogError($"[建筑系统] 找不到建筑 {building.ConfigId} 的配置，无法分配工人。");
                return false;
            }

            // 初始化该建筑的工人列表（如果尚不存在）
            if (!_workerAssignments.ContainsKey(buildingId))
            {
                _workerAssignments[buildingId] = new List<string>();
            }

            // 检查建筑是否已达到最大工人数量
            if (_workerAssignments[buildingId].Count >= config.MaxWorkers)
            {
                Debug.LogWarning($"[建筑系统] 建筑 {config.Name} (ID: {buildingId}) 已达到最大工人数量 ({config.MaxWorkers})。");
                return false;
            }

            // 检查工人是否已分配到此建筑
            if (_workerAssignments[buildingId].Contains(workerId))
            {
                Debug.LogWarning($"[建筑系统] 工人 (ID: {workerId}) 已经分配到建筑 {config.Name} (ID: {buildingId})。");
                return false;
            }

            // 通知幸存者系统处理工人分配状态
            var survivorSystem = this.GetSystem<SurvivorSystem>();
            if (!survivorSystem.AssignSurvivorToBuilding(workerId, buildingId))
            {
                // AssignSurvivorToBuilding 内部应处理工人是否可用的逻辑并打印相应日志
                return false;
            }

            _workerAssignments[buildingId].Add(workerId); // 将工人添加到建筑的分配列表
            Debug.Log($"[建筑系统] 工人 (ID: {workerId}) 已成功分配到建筑 {config.Name} (ID: {buildingId})。");
            // TODO: 发送工人分配事件
            return true;
        }

        /// <summary>
        /// 从一个建筑中移除一名工人。
        /// </summary>
        /// <param name="buildingId">工人所在的建筑ID。</param>
        /// <param name="workerId">要移除的工人的ID。</param>
        /// <returns>如果移除成功则返回true，否则返回false。</returns>
        public bool RemoveWorker(string buildingId, string workerId)
        {
            // 检查建筑是否有工人分配记录，以及该工人是否确实在该建筑
            if (!_workerAssignments.TryGetValue(buildingId, out var workersInBuilding) || !workersInBuilding.Contains(workerId))
            {
                Debug.LogWarning($"[建筑系统] 尝试从建筑 (ID: {buildingId}) 移除不存在或未分配的工人 (ID: {workerId})。");
                return false;
            }

            if (!workersInBuilding.Remove(workerId)) // 从建筑的工人列表中移除
            {
                 Debug.LogError($"[建筑系统] 从建筑 (ID: {buildingId}) 的工人列表中移除工人 (ID: {workerId}) 失败。");
                return false; // 理论上不应发生，因为上面已检查过Contains
            }

            // 通知幸存者系统工人已被解除分配
            var survivorSystem = this.GetSystem<SurvivorSystem>();
            survivorSystem.UnassignSurvivorFromBuilding(workerId);

            Debug.Log($"[建筑系统] 工人 (ID: {workerId}) 已从建筑 (ID: {buildingId}) 移除。");
            // TODO: 发送工人移除事件
            return true;
        }

        /// <summary>
        /// 获取分配给指定建筑的工人ID列表。
        /// </summary>
        /// <param name="buildingId">建筑的ID。</param>
        /// <returns>一个包含工人ID的新列表；如果建筑不存在或没有工人，则返回空列表。</returns>
        public List<string> GetBuildingWorkers(string buildingId)
        {
            // 返回一个新的列表副本，以防止外部修改内部数据结构
            return _workerAssignments.TryGetValue(buildingId, out var workers)
                ? new List<string>(workers)
                : new List<string>();
        }

        #endregion

        #region 建筑升级 (Building Upgrades)

        /// <summary>
        /// 尝试升级指定的建筑到下一等级。
        /// </summary>
        /// <param name="buildingId">要升级的建筑ID。</param>
        /// <returns>如果成功开始升级则返回true，否则返回false。</returns>
        public bool UpgradeBuilding(string buildingId)
        {
            if (!_buildings.TryGetValue(buildingId, out var building)) // 检查建筑是否存在
            {
                Debug.LogError($"[建筑系统] 尝试升级不存在的建筑 (ID: {buildingId})。");
                return false;
            }

            if (building.State == BuildingState.Upgrading || building.State == BuildingState.UnderConstruction)
            {
                Debug.LogWarning($"[建筑系统] 建筑 {building.ConfigId} (ID: {buildingId}) 已经在建造或升级中，无法再次升级。");
                return false;
            }

            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            var currentConfig = configSystem.GetBuildingConfig(building.ConfigId); // 获取当前建筑的配置
            if (currentConfig == null)
            {
                Debug.LogError($"[建筑系统] 找不到当前建筑 {building.ConfigId} 的配置，无法升级。");
                return false;
            }

            // 查找目标升级配置
            // 假设升级逻辑：同名建筑（例如 "Farm_L1" -> "Farm_L2"），等级+1
            string baseName = currentConfig.Name.Contains('_') ? currentConfig.Name.Split('_')[0] : currentConfig.Name;
            int nextLevel = currentConfig.Level + 1;

            BuildingConfig targetConfig = configSystem.GetAllBuildingConfigs().Values
                .FirstOrDefault(cfg => cfg.Name.StartsWith(baseName) && cfg.Level == nextLevel && cfg.Category == currentConfig.Category);

            if (targetConfig == null)
            {
                Debug.LogWarning($"[建筑系统] 建筑 {currentConfig.Name} (ID: {buildingId}) 没有可用的下一等级升级配置。");
                return false;
            }

            // 获取升级成本信息 (可能在BuildingUpgrade中定义，或直接使用目标配置的建造成本)
            BuildingUpgrade upgradeInfo = configSystem.GetBuildingUpgrade(currentConfig.ConfigId, targetConfig.ConfigId);
            List<ResourceCost> upgradeCosts = upgradeInfo?.UpgradeCosts ?? targetConfig.BuildCosts; // 优先使用特定升级成本

            if (upgradeCosts == null || !upgradeCosts.Any())
            {
                Debug.LogWarning($"[建筑系统] 建筑 {currentConfig.Name} 到 {targetConfig.Name} 的升级成本未定义。");
                return false;
            }

            // 检查科技前置条件 (假设目标配置有RequiredTechs列表)
            // var techSystem = this.GetSystem<AdvancedTechSystem>();
            // if (targetConfig.RequiredTechs != null &&
            //     targetConfig.RequiredTechs.Any(reqTechId => !techSystem.IsTechResearched(reqTechId)))
            // {
            //     Debug.LogWarning($"[建筑系统] 升级到 {targetConfig.Name} 所需的科技尚未研究。");
            //     return false;
            // }

            // 检查资源是否足够支付升级成本
            var resourceSystem = this.GetSystem<ResourceSystem>();
            if (!resourceSystem.CanAfford(upgradeCosts))
            {
                Debug.LogWarning($"[建筑系统] 资源不足，无法将建筑 {currentConfig.Name} 升级到 {targetConfig.Name}。");
                return false;
            }

            // 消耗升级资源
            resourceSystem.ConsumeResources(upgradeCosts);

            // 设置建筑为升级中状态
            building.State = BuildingState.Upgrading;
            building.BuildProgress = 0f; // 重置进度条以显示升级进度
            // building.TargetUpgradeConfigId = targetConfig.ConfigId; // 存储目标配置ID，在UpdateConstruction中完成升级时使用
            // 或者，如果升级时间与新建筑时间一致，可以直接在UpdateConstruction中获取targetConfig

            _buildingQueue.Add(buildingId); // 将建筑加入建造/升级队列

            Debug.Log($"[建筑系统] 开始将建筑 {currentConfig.Name} (ID: {buildingId}) 升级到 {targetConfig.Name}。");
            // TODO: 发送建筑开始升级事件
            return true;
        }

        #endregion

        #region 事件处理 (Event Handling)

        /// <summary>
        /// 处理游戏更新事件，用于驱动建筑系统的内部逻辑更新。
        /// </summary>
        /// <param name="e">游戏更新事件参数，包含deltaTime。</param>
        private void OnGameUpdate(GameUpdateEvent e)
        {
            float deltaTime = e.DeltaTime; // 获取帧间隔时间

            UpdateConstruction(deltaTime);       // 更新所有在建/升级中建筑的进度
            UpdateBuildingOperations(deltaTime); // 更新所有运作中建筑的逻辑
        }

        #endregion

        #region 公共接口 (Public API)

        /// <summary>
        /// 根据建筑ID获取建筑数据。
        /// </summary>
        /// <param name="buildingId">建筑的唯一ID。</param>
        /// <returns>对应的BuildingData实例，如果找不到则返回null。</returns>
        public BuildingData GetBuilding(string buildingId)
        {
            return _buildings.TryGetValue(buildingId, out var building) ? building : null;
        }

        /// <summary>
        /// 获取当前游戏中所有建筑的数据。
        /// </summary>
        /// <returns>一个新的字典，包含所有建筑数据，键为建筑ID。</returns>
        public Dictionary<string, BuildingData> GetAllBuildings()
        {
            return new Dictionary<string, BuildingData>(_buildings); // 返回副本以防外部修改
        }

        /// <summary>
        /// 根据指定的建筑类别获取该类别下的所有建筑。
        /// </summary>
        /// <param name="category">要查询的建筑类别。</param>
        /// <returns>一个包含指定类别建筑数据的列表。</returns>
        public List<BuildingData> GetBuildingsByCategory(BuildingCategory category)
        {
            var configSystem = this.GetSystem<ConfigSystem>(); // 获取配置系统
            return _buildings.Values
                .Where(building =>
                {
                    var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取每个建筑的配置
                    return config != null && config.Category == category; // 检查配置是否存在且类别匹配
                })
                .ToList();
        }

        /// <summary>
        /// 获取游戏中建筑的总数量，可以按特定配置ID筛选。
        /// </summary>
        /// <param name="configId">（可选）如果提供，则只统计该配置ID的建筑数量。否则统计所有建筑数量。</param>
        /// <returns>符合条件的建筑数量。</returns>
        public int GetBuildingCount(string configId = null)
        {
            if (string.IsNullOrEmpty(configId)) // 如果未提供configId，返回所有建筑总数
                return _buildings.Count;

            // 如果提供了configId，则统计匹配该ID的建筑数量
            return _buildings.Values.Count(b => b.ConfigId == configId);
        }

        /// <summary>
        /// 拆除指定的建筑。
        /// </summary>
        /// <param name="buildingId">要拆除的建筑ID。</param>
        /// <returns>如果成功拆除则返回true，否则返回false。</returns>
        public bool DemolishBuilding(string buildingId)
        {
            if (!_buildings.TryGetValue(buildingId, out var building)) // 检查建筑是否存在
            {
                Debug.LogWarning($"[建筑系统] 尝试拆除不存在的建筑 (ID: {buildingId})。");
                return false;
            }

            // 移除所有分配给此建筑的工人
            if(_workerAssignments.TryGetValue(buildingId, out var workersToUnassign))
            {
                // 创建工人列表副本进行迭代，因为RemoveWorker会修改_workerAssignments[buildingId]
                var workersSnapshot = new List<string>(workersToUnassign);
                foreach (var workerId in workersSnapshot)
                {
                    RemoveWorker(buildingId, workerId); // 调用移除工人逻辑
                }
            }

            // 返还部分建造成本资源 (例如50%)
            var configSystem = this.GetSystem<ConfigSystem>();
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            var resourceSystem = this.GetSystem<ResourceSystem>();

            if(config != null && config.BuildCosts != null)
            {
                foreach (var cost in config.BuildCosts)
                {
                    int returnAmount = Mathf.FloorToInt(cost.Amount * 0.5f); // 计算返还50%的资源量
                    if(returnAmount > 0) resourceSystem.AddResource(cost.Type, returnAmount); // 添加返还的资源
                }
                Debug.Log($"[建筑系统] 拆除建筑 {config.Name} (ID: {buildingId}) 并返还了部分资源。");
            }

            // 从系统中移除建筑数据
            _buildings.Remove(buildingId);
            _workerAssignments.Remove(buildingId); // 同时移除工人分配记录
            _buildingQueue.Remove(buildingId);     // 如果在建造队列中也移除

            Debug.Log($"[建筑系统] 建筑 {(config != null ? config.Name : buildingId)} (ID: {buildingId}) 已被成功拆除。");
            // TODO: 发送建筑拆除事件，通知UI等其他系统
            // this.SendEvent(new BuildingDemolishedEvent { BuildingId = buildingId, ConfigId = building.ConfigId });
            return true;
        }

        #endregion
    }

    #region 事件定义 (Event Definitions)

    /// <summary>
    /// 建筑建造完成事件结构体。
    /// 当一个建筑完成其建造过程时发送。
    /// </summary>
    public struct BuildingConstructionCompletedEvent
    {
        /// <summary>
        /// 完成建造的建筑的唯一ID。
        /// </summary>
        public string BuildingId;
        /// <summary>
        /// 完成建造的建筑的配置ID。
        /// </summary>
        public string ConfigId;
    }

    // TODO: 可以根据需要添加更多建筑相关事件，例如：
    // public struct BuildingUpgradeCompletedEvent { public string BuildingId; public string NewConfigId; }
    // public struct BuildingDemolishedEvent { public string BuildingId; public string ConfigId; }
    // public struct BuildingDamagedEvent { public string BuildingId; public float CurrentHealth; }
    // public struct BuildingRepairedEvent { public string BuildingId; }

    #endregion
}