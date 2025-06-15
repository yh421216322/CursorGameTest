// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using QFramework;
// using SurvivalGame.Model;
//
// namespace SurvivalGame.GameSystem
// {
//     
//     public interface IBuildingSystem : QFISystem
//     {
//         public bool StartConstruction(string configId, Vector3 position);
//         public bool AssignWorker(string buildingId, string workerId);
//         public bool DemolishBuilding(string buildingId);
//         public Dictionary<string, BuildingData> GetAllBuildings();
//         public int GetBuildingCount(string configId = null);
//         public List<BuildingData> GetBuildingsByCategory(BuildingCategory category);
//         public List<string> GetBuildingWorkers(string buildingId);
//         public bool UpgradeBuilding(string buildingId);
//     }
//     /// <summary>
//     /// 建筑系统 - 管理所有建筑的建造、运行和维护
//     /// </summary>
//     public class BuildingSystem : AbstractSystem,IBuildingSystem
//     {
//         #region 数据存储
//         
//         private Dictionary<string, BuildingData> _buildings;
//         private List<string> _buildingQueue;
//         private Dictionary<string, List<string>> _workerAssignments; // 建筑ID -> 工人ID列表
//         
//         #endregion
//         
//         #region 系统初始化
//         
//         protected override void OnInit()
//         {
//             _buildings = new Dictionary<string, BuildingData>();
//             _buildingQueue = new List<string>();
//             _workerAssignments = new Dictionary<string, List<string>>();
//             Debug.Log("初始化建筑系统");
//             
//             // 注册事件
//             this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);
//         }
//         
//         #endregion
//         
//         #region 建筑建造
//         
//         // /// <summary>
//         // /// 开始建造建筑
//         // /// </summary>
//         // public bool StartConstruction(string configId, Vector3 position)
//         // {
//         //     var configSystem = this.GetSystem<ConfigSystem>();
//         //     var resourceSystem = this.GetSystem<ResourceSystem>();
//         //     
//         //     var config = configSystem.GetBuildingConfig(configId);
//         //     if (config == null)
//         //     {
//         //         Debug.LogError($"找不到建筑配置: {configId}");
//         //         return false;
//         //     }
//         //     
//         //     // 检查科技前置条件
//         //     var techSystem = this.GetSystem<TechSystem>();
//         //     var completedTechs = techSystem.GetCompletedTechs();
//         //     if (!configSystem.IsBuildingUnlockedByTech(configId, completedTechs))
//         //     {
//         //         Debug.LogWarning($"建筑 {config.Name} 需要先研究相关科技");
//         //         return false;
//         //     }
//         //     
//         //     // 检查资源是否足够
//         //     if (!resourceSystem.CanAfford(config.BuildCosts))
//         //     {
//         //         Debug.LogWarning($"资源不足，无法建造 {config.Name}");
//         //         return false;
//         //     }
//         //     
//         //     // 扣除资源
//         //     resourceSystem.ConsumeResources();
//         //     
//         //     // 创建建筑实例
//         //     var buildingData = new BuildingData
//         //     {
//         //         ConfigId = configId,
//         //         Position = position,
//         //         State = BuildingState.UnderConstruction,
//         //         MaxHealth = 100f,
//         //         Health = 100f
//         //     };
//         //     
//         //     _buildings[buildingData.Id] = buildingData;
//         //     _buildingQueue.Add(buildingData.Id);
//         //     _workerAssignments[buildingData.Id] = new List<string>();
//         //     
//         //     Debug.Log($"开始建造 {config.Name} 于位置 {position}");
//         //     return true;
//         // }
//         
//         /// <summary>
//         /// 更新建造进度
//         /// </summary>
//         private void UpdateConstruction(float deltaTime)
//         {
//             var configSystem = this.GetSystem<ConfigSystem>();
//             var completedBuildings = new List<string>();
//             
//             foreach (var buildingId in _buildingQueue)
//             {
//                 if (!_buildings.TryGetValue(buildingId, out var building)) continue;
//                 
//                 var config = configSystem.GetBuildingConfig(building.ConfigId);
//                 if (config == null) continue;
//                 
//                 // 计算建造速度（可以根据分配的工人数量调整）
//                 float buildSpeed = 1.0f;
//                 if (_workerAssignments[buildingId].Count > 0)
//                 {
//                     buildSpeed += _workerAssignments[buildingId].Count * 0.2f; // 每个工人增加20%速度
//                 }
//                 
//                 building.BuildProgress += (deltaTime / config.BuildTime) * buildSpeed;
//                 
//                 if (building.BuildProgress >= 1.0f)
//                 {
//                     building.BuildProgress = 1.0f;
//                     building.State = BuildingState.Operational;
//                     completedBuildings.Add(buildingId);
//                     
//                     Debug.Log($"建筑 {config.Name} 建造完成");
//                     
//                     // 发送建造完成事件
//                     this.SendEvent(new BuildingConstructionCompletedEvent
//                     {
//                         BuildingId = buildingId,
//                         ConfigId = building.ConfigId
//                     });
//                 }
//             }
//             
//             // 移除已完成的建筑
//             foreach (var buildingId in completedBuildings)
//             {
//                 _buildingQueue.Remove(buildingId);
//             }
//         }
//         
//         #endregion
//         
//         #region 建筑运行
//         
//         /// <summary>
//         /// 更新建筑运行
//         /// </summary>
//         private void UpdateBuildingOperations(float deltaTime)
//         {
//             var configSystem = this.GetSystem<ConfigSystem>();
//             var resourceSystem = this.GetSystem<ResourceSystem>();
//             var survivorSystem = this.GetSystem<SurvivorSystem>();
//             
//             foreach (var building in _buildings.Values)
//             {
//                 if (building.State != BuildingState.Operational) continue;
//                 
//                 var config = configSystem.GetBuildingConfig(building.ConfigId);
//                 if (config == null) continue;
//                 
//                 // 更新资源生产
//                 UpdateResourceProduction(building, config, deltaTime);
//                 
//                 // 更新维护消耗
//                 UpdateMaintenance(building, config, deltaTime);
//                 
//                 // 更新工人经验
//                 UpdateWorkerExperience(building, config, deltaTime);
//             }
//         }
//         
//         /// <summary>
//         /// 更新资源生产
//         /// </summary>
//         private void UpdateResourceProduction(BuildingData building, BuildingConfig config, float deltaTime)
//         {
//             var resourceSystem = this.GetSystem<ResourceSystem>();
//             var survivorSystem = this.GetSystem<SurvivorSystem>();
//             
//             foreach (var production in config.Productions)
//             {
//                 if (production.RequiresWorker && _workerAssignments[building.Id].Count == 0)
//                     continue;
//                 
//                 // 检查输入资源
//                 if (production.InputCosts != null && production.InputCosts.Count > 0)
//                 {
//                     if (!resourceSystem.CanAfford(production.InputCosts))
//                         continue;
//                 }
//                 
//                 // 计算生产效率
//                 float efficiency = production.BaseRate;
//                 
//                 // 工人加成
//                 foreach (var workerId in _workerAssignments[building.Id])
//                 {
//                     var worker = survivorSystem.GetSurvivor(workerId);
//                     if (worker != null)
//                     {
//                         efficiency += production.WorkerBonus * GetWorkerEfficiency(worker, config);
//                     }
//                 }
//                 
//                 // 计算实际产出
//                 float actualProduction = efficiency * (deltaTime / 3600f); // 转换为小时
//                 
//                 if (actualProduction > 0)
//                 {
//                     // 消耗输入资源
//                     if (production.InputCosts != null)
//                     {
//                         resourceSystem.ConsumeResources();
//                     }
//                     
//                     // 添加产出资源
//                     resourceSystem.AddResource(production.Type, Mathf.FloorToInt(actualProduction));
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 计算工人效率
//         /// </summary>
//         private float GetWorkerEfficiency(SurvivorData worker, BuildingConfig config)
//         {
//             float efficiency = 1.0f;
//             
//             // 根据建筑类型和工人属性计算效率
//             switch (config.Category)
//             {
//                 case BuildingCategory.Production:
//                     efficiency *= AttributeLevelHelper.GetEfficiencyMultiplier(
//                         worker.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Production)?.Value ?? 20);
//                     break;
//                 case BuildingCategory.Defense:
//                     efficiency *= AttributeLevelHelper.GetEfficiencyMultiplier(
//                         worker.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Combat)?.Value ?? 20);
//                     break;
//                 case BuildingCategory.Functional:
//                     efficiency *= AttributeLevelHelper.GetEfficiencyMultiplier(
//                         worker.Attributes.FirstOrDefault(a => a.Type == SurvivorAttributeType.Technology)?.Value ?? 20);
//                     break;
//             }
//             
//             return efficiency;
//         }
//         
//         /// <summary>
//         /// 更新维护消耗
//         /// </summary>
//         private void UpdateMaintenance(BuildingData building, BuildingConfig config, float deltaTime)
//         {
//             if (config.MaintenanceCosts == null || config.MaintenanceCosts.Count == 0) return;
//             
//             float timeSinceLastMaintenance = Time.time - building.LastMaintenanceTime;
//             if (timeSinceLastMaintenance >= 3600f) // 每小时维护一次
//             {
//                 var resourceSystem = this.GetSystem<ResourceSystem>();
//                 
//                 if (resourceSystem.CanAfford(config.MaintenanceCosts))
//                 {
//                     resourceSystem.ConsumeResources();
//                     building.LastMaintenanceTime = Time.time;
//                 }
//                 else
//                 {
//                     // 维护不足，建筑效率下降或损坏
//                     building.Health = Mathf.Max(0, building.Health - 5f);
//                     if (building.Health <= 0)
//                     {
//                         building.State = BuildingState.Damaged;
//                         Debug.LogWarning($"建筑 {config.Name} 因缺乏维护而损坏");
//                     }
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// 更新工人经验
//         /// </summary>
//         private void UpdateWorkerExperience(BuildingData building, BuildingConfig config, float deltaTime)
//         {
//             var survivorSystem = this.GetSystem<SurvivorSystem>();
//             
//             foreach (var workerId in _workerAssignments[building.Id])
//             {
//                 var worker = survivorSystem.GetSurvivor(workerId);
//                 if (worker == null) continue;
//                 
//                 // 根据建筑类型给予相应属性经验
//                 SurvivorAttributeType expType = GetExperienceType(config.Category);
//                 int expAmount = Mathf.FloorToInt(deltaTime * 0.1f); // 每秒0.1经验
//                 
//                 // 添加通用经验
//                 survivorSystem.AddExperienceToSurvivor(workerId, expAmount);
//                 // 添加属性经验
//                 worker.AddAttributeExperience(expType, expAmount);
//             }
//         }
//         
//         private SurvivorAttributeType GetExperienceType(BuildingCategory category)
//         {
//             switch (category)
//             {
//                 case BuildingCategory.Production: return SurvivorAttributeType.Production;
//                 case BuildingCategory.Defense: return SurvivorAttributeType.Combat;
//                 case BuildingCategory.Functional: return SurvivorAttributeType.Technology;
//                 default: return SurvivorAttributeType.Production;
//             }
//         }
//         
//         #endregion
//         
//         #region 工人分配
//         
//         /// <summary>
//         /// 分配工人到建筑
//         /// </summary>
//         public bool AssignWorker(string buildingId, string workerId)
//         {
//             if (!_buildings.ContainsKey(buildingId))
//             {
//                 Debug.LogError($"建筑不存在: {buildingId}");
//                 return false;
//             }
//             
//             var building = _buildings[buildingId];
//             if (building.State != BuildingState.Operational)
//             {
//                 Debug.LogWarning("只能向运行中的建筑分配工人");
//                 return false;
//             }
//             
//             var configSystem = this.GetSystem<ConfigSystem>();
//             var config = configSystem.GetBuildingConfig(building.ConfigId);
//             
//             if (_workerAssignments[buildingId].Count >= config.MaxWorkers)
//             {
//                 Debug.LogWarning($"建筑 {config.Name} 已达到最大工人数量");
//                 return false;
//             }
//             
//             if (_workerAssignments[buildingId].Contains(workerId))
//             {
//                 Debug.LogWarning("工人已经分配到此建筑");
//                 return false;
//             }
//             
//             var survivorSystem = this.GetSystem<SurvivorSystem>();
//             if (!survivorSystem.AssignSurvivorToBuilding(workerId, buildingId))
//             {
//                 return false;
//             }
//             
//             _workerAssignments[buildingId].Add(workerId);
//             Debug.Log($"工人 {workerId} 已分配到建筑 {config.Name}");
//             return true;
//         }
//         
//         /// <summary>
//         /// 从建筑移除工人
//         /// </summary>
//         public bool RemoveWorker(string buildingId, string workerId)
//         {
//             if (!_workerAssignments.ContainsKey(buildingId))
//                 return false;
//             
//             if (!_workerAssignments[buildingId].Remove(workerId))
//                 return false;
//             
//             var survivorSystem = this.GetSystem<SurvivorSystem>();
//             survivorSystem.UnassignSurvivorFromBuilding(workerId);
//             
//             Debug.Log($"工人 {workerId} 已从建筑移除");
//             return true;
//         }
//         
//         /// <summary>
//         /// 获取建筑的工人列表
//         /// </summary>
//         public List<string> GetBuildingWorkers(string buildingId)
//         {
//             return _workerAssignments.TryGetValue(buildingId, out var workers) 
//                 ? new List<string>(workers) 
//                 : new List<string>();
//         }
//         
//         #endregion
//         
//         #region 建筑升级
//         
//         /// <summary>
//         /// 升级建筑
//         /// </summary>
//         public bool UpgradeBuilding(string buildingId)
//         {
//             if (!_buildings.TryGetValue(buildingId, out var building))
//                 return false;
//             
//             var configSystem = this.GetSystem<ConfigSystem>();
//             var currentConfig = configSystem.GetBuildingConfig(building.ConfigId);
//             
//             // 查找升级配置
//             var upgrades = configSystem.GetAllBuildingConfigs().Values
//                 .Where(config => config.Category == currentConfig.Category && 
//                                 config.Level == currentConfig.Level + 1)
//                 .ToList();
//             
//             if (upgrades.Count == 0)
//             {
//                 Debug.LogWarning($"建筑 {currentConfig.Name} 无法升级");
//                 return false;
//             }
//             
//             var targetConfig = upgrades.First();
//             var upgrade = configSystem.GetBuildingUpgrade(building.ConfigId, targetConfig.ConfigId);
//             
//             if (upgrade == null)
//             {
//                 Debug.LogWarning($"找不到从 {currentConfig.Name} 到 {targetConfig.Name} 的升级配置");
//                 return false;
//             }
//             
//             // 检查科技和资源
//             var techSystem = this.GetSystem<TechSystem>();
//             var resourceSystem = this.GetSystem<ResourceSystem>();
//             
//             if (!configSystem.AreTechPrerequisitesMet(targetConfig.ConfigId, techSystem.GetCompletedTechs()))
//             {
//                 Debug.LogWarning("升级需要先研究相关科技");
//                 return false;
//             }
//             
//             if (!resourceSystem.CanAfford(upgrade.UpgradeCosts))
//             {
//                 Debug.LogWarning("资源不足，无法升级");
//                 return false;
//             }
//             
//             // 开始升级
//             resourceSystem.ConsumeResources();
//             building.State = BuildingState.Upgrading;
//             building.BuildProgress = 0f;
//             
//             Debug.Log($"开始升级建筑 {currentConfig.Name} 到 {targetConfig.Name}");
//             return true;
//         }
//         
//         #endregion
//         
//         #region 事件处理
//         
//         private void OnGameUpdate(GameUpdateEvent e)
//         {
//            // Debug.Log($"游戏更新 {e.DeltaTime}");
//             float deltaTime = e.DeltaTime;
//             
//             UpdateConstruction(deltaTime);
//             UpdateBuildingOperations(deltaTime);
//         }
//         
//         #endregion
//         
//         #region 公共接口
//         
//         /// <summary>
//         /// 获取建筑数据
//         /// </summary>
//         public BuildingData GetBuilding(string buildingId)
//         {
//             return _buildings.TryGetValue(buildingId, out var building) ? building : null;
//         }
//         
//         /// <summary>
//         /// 获取所有建筑
//         /// </summary>
//         public Dictionary<string, BuildingData> GetAllBuildings()
//         {
//             return new Dictionary<string, BuildingData>(_buildings);
//         }
//         
//         /// <summary>
//         /// 获取指定类别的建筑
//         /// </summary>
//         public List<BuildingData> GetBuildingsByCategory(BuildingCategory category)
//         {
//             var configSystem = this.GetSystem<ConfigSystem>();
//             return _buildings.Values
//                 .Where(building => 
//                 {
//                     var config = configSystem.GetBuildingConfig(building.ConfigId);
//                     return config != null && config.Category == category;
//                 })
//                 .ToList();
//         }
//         
//         /// <summary>
//         /// 获取建筑总数量
//         /// </summary>
//         public int GetBuildingCount(string configId = null)
//         {
//             if (string.IsNullOrEmpty(configId))
//                 return _buildings.Count;
//             
//             return _buildings.Values.Count(b => b.ConfigId == configId);
//         }
//         
//         /// <summary>
//         /// 拆除建筑
//         /// </summary>
//         public bool DemolishBuilding(string buildingId)
//         {
//             if (!_buildings.TryGetValue(buildingId, out var building))
//                 return false;
//             
//             // 移除所有工人
//             var workers = GetBuildingWorkers(buildingId);
//             foreach (var workerId in workers)
//             {
//                 RemoveWorker(buildingId, workerId);
//             }
//             
//             // 返还部分资源
//             var configSystem = this.GetSystem<ConfigSystem>();
//             var config = configSystem.GetBuildingConfig(building.ConfigId);
//             var resourceSystem = this.GetSystem<ResourceSystem>();
//             
//             foreach (var cost in config.BuildCosts)
//             {
//                 int returnAmount = Mathf.FloorToInt(cost.Amount * 0.5f); // 返还50%资源
//                 resourceSystem.AddResource(cost.Type, returnAmount);
//             }
//             
//             // 移除建筑
//             _buildings.Remove(buildingId);
//             _workerAssignments.Remove(buildingId);
//             _buildingQueue.Remove(buildingId);
//             
//             Debug.Log($"建筑 {config.Name} 已拆除");
//             return true;
//         }
//         
//         #endregion
//     }
//     
//     #region 事件定义
//     
//     public struct BuildingConstructionCompletedEvent
//     {
//         public string BuildingId;
//         public string ConfigId;
//     }
//     
//     #endregion
// } 