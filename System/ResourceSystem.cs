using System;
using System.Collections.Generic;
using QFramework;
using SurvivalGame.Model;
using UnityEngine;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 功能：资源管理系统，管理游戏中所有资源的产出、消耗和流转
    /// 职责：
    /// 1. 定期处理资源产出（根据建筑和工人）
    /// 2. 定期处理资源消耗（人口需求、建筑维护）
    /// 3. 处理资源腐坏（食物、医疗用品）
    /// 4. 提供资源查询和操作接口
    /// 依赖模型：ISurvivalGameModel
    /// 依赖系统：IAdvancedTechSystem、IEnhancedBuildingSystem、ISurvivorSystem
    /// 注册方式：在RegisterManager中调用RegisterSystem<IResourceSystem>(new ResourceSystem())
    /// </summary>
    public interface IResourceSystem : QFISystem
    {
        /// <summary>定期资源产出 - 根据建筑和工人产出各种资源</summary>
      //  void ProduceResources();
        
        /// <summary>定期资源消耗 - 处理人口消耗和建筑维护</summary>
        void ConsumeResources();
        
        /// <summary>检查是否能承担指定的资源消耗</summary>
        /// <param name="costs">资源消耗列表</param>
        /// <returns>是否有足够资源</returns>
        bool CanAfford(List<ResourceCost> costs);
        
        /// <summary>消耗指定的资源列表</summary>
        /// <param name="costs">要消耗的资源列表</param>
        /// <returns>是否成功消耗</returns>
        bool ConsumeResourcesList(List<ResourceCost> costs);
        
        /// <summary>尝试消耗指定类型和数量的资源</summary>
        /// <param name="type">资源类型</param>
        /// <param name="amount">消耗数量</param>
        /// <returns>是否成功消耗</returns>
        bool TryConsumeResource(ResourceType type, int amount);
        
        /// <summary>添加指定类型和数量的资源</summary>
        /// <param name="type">资源类型</param>
        /// <param name="amount">添加数量</param>
        void AddResource(ResourceType type, int amount);
        
        /// <summary>获取指定类型资源的当前数量</summary>
        /// <param name="type">资源类型</param>
        /// <returns>资源数量</returns>
        int GetResourceAmount(ResourceType type);
        
        /// <summary>获取指定类型资源的产出速率（每分钟）</summary>
        /// <param name="type">资源类型</param>
        /// <returns>产出速率</returns>
        float GetResourceProductionRate(ResourceType type);
        
        /// <summary>获取指定类型资源的消耗速率（每分钟）</summary>
        /// <param name="type">资源类型</param>
        /// <returns>消耗速率</returns>
        float GetResourceConsumptionRate(ResourceType type);
        
        /// <summary>立即更新资源产出（强制执行一次产出）</summary>
        void UpdateResourceProduction();
        
        /// <summary>处理资源腐坏（食物、医疗用品等易腐资源）</summary>
        void ProcessResourceDecay();
    }
    
    /// <summary>
    /// 资源管理系统实现类
    /// 负责管理游戏中所有资源的产出、消耗、腐坏等逻辑
    /// </summary>
    public class ResourceSystem : AbstractSystem, IResourceSystem
    {
        // 依赖的系统和模型
        private ISurvivalGameModel mGameModel;          // 游戏数据模型
        private IAdvancedTechSystem mTechSystem;        // 科技系统（用于获取科技加成）
        private IEnhancedBuildingSystem mBuildingSystem;// 建筑系统（用于获取产出建筑）
        private ISurvivorSystem mSurvivorSystem;        // 幸存者系统（用于获取工人信息）
        
        // 时间记录变量
        private float mLastProductionTime;              // 上次资源产出时间
        private float mLastConsumptionTime;             // 上次资源消耗时间
        private float mLastDecayTime;                   // 上次腐坏检查时间
        
        // 时间间隔常量（秒）
        private const float PRODUCTION_INTERVAL = 5f;   // 资源产出间隔：每5秒产出一次
        private const float CONSUMPTION_INTERVAL = 10f; // 资源消耗间隔：每10秒消耗一次
        private const float DECAY_INTERVAL = 60f;       // 腐坏检查间隔：每60秒检查一次腐坏
        
        /// <summary>
        /// 系统初始化方法
        /// 获取依赖的系统和模型引用，注册事件监听，初始化时间记录
        /// </summary>
        protected override void OnInit()
        {
            // 获取依赖的系统和模型
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mTechSystem = this.GetSystem<IAdvancedTechSystem>();
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            mSurvivorSystem = this.GetSystem<ISurvivorSystem>();
            
            // 初始化时间记录
            mLastProductionTime = Time.time;
            mLastConsumptionTime = Time.time;
            mLastDecayTime = Time.time;
            
            // 注册事件监听
            this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);                          // 游戏更新事件
            this.RegisterEvent<TechCompletedEvent>(OnTechCompleted);                    // 科技完成事件
            this.RegisterEvent<BuildingConstructionCompletedEvent>(OnBuildingCompleted);// 建筑完成事件
            
            Debug.Log("资源管理系统初始化完成");
        }
        
        /// <summary>
        /// 游戏更新事件处理
        /// 根据时间间隔定期执行资源产出、消耗和腐坏检查
        /// </summary>
        /// <param name="e">游戏更新事件参数</param>
        private void OnGameUpdate(GameUpdateEvent e)
        {
            if (mGameModel.IsPaused.Value) return; // 游戏暂停时不处理
            
            float currentTime = Time.time;
            
            // // 检查是否到了资源产出时间
            // if (currentTime - mLastProductionTime >= PRODUCTION_INTERVAL)
            // {
            //     ProduceResources();
            //     mLastProductionTime = currentTime;
            // }
            
            // 检查是否到了资源消耗时间
            if (currentTime - mLastConsumptionTime >= CONSUMPTION_INTERVAL)
            {
                ConsumeResources();
                mLastConsumptionTime = currentTime;
            }
            
            // 检查是否到了腐坏检查时间
            if (currentTime - mLastDecayTime >= DECAY_INTERVAL)
            {
                ProcessResourceDecay();
                mLastDecayTime = currentTime;
            }
        }
        
        /// <summary>
        /// 执行资源产出逻辑
        /// 遍历所有可运营的建筑，根据建筑类型和工人情况产出相应资源
        /// </summary>
        // public void ProduceResources()
        // {
        //     // 获取所有建筑并遍历产出资源
        //     var buildings = mBuildingSystem.GetAllBuildings();
        //     foreach (var building in buildings)
        //     {
        //         if (building.State != BuildingState.Operational) continue; // 只有运营状态的建筑才能产出
        //         
        //         ProduceFromBuilding(building);
        //     }
        //     
        //     // 发送资源产出事件通知其他系统
        //     this.SendEvent(new ResourceProductionEvent { ProductionTime = Time.time });
        //     
        //     Debug.Log($"资源产出完成 - 食物:{mGameModel.Food.Value} 弹药:{mGameModel.Ammunition.Value} 材料:{mGameModel.Materials.Value}");
        // }
        
        /// <summary>
        /// 根据建筑类型和工人情况产出资源
        /// 每种建筑都有特定的产出逻辑和资源类型
        /// </summary>
        /// <param name="building">要产出资源的建筑数据</param>
        private void ProduceFromBuilding(BuildingData building)
        {
            // 获取在该建筑工作的幸存者
            var workers = mSurvivorSystem.GetBuildingWorkers(building.Id);
            if (workers.Count == 0) return; // 没有工人就不产出资源
            
            // 根据建筑配置ID调用对应的产出方法
            switch (building.ConfigId)
            {
                case "Farm":
                    ProduceFarmResources(building, workers);        // 农场：产出食物和有机物
                    break;
                case "Workshop":
                    ProduceWorkshopResources(building, workers);    // 工坊：产出弹药和工具
                    break;
                case "Laboratory":
                    ProduceLaboratoryResources(building, workers);  // 实验室：产出科研点数
                    break;
                case "MedicalStation":
                    ProduceMedicalResources(building, workers);     // 医疗站：产出医疗用品
                    break;
                case "Quarry":
                    ProduceQuarryResources(building, workers);      // 采石场：产出材料和废料
                    break;
                case "WaterPurifier":
                    ProduceWaterResources(building, workers);       // 净水器：产出水
                    break;
            }
        }
        
        private void ProduceFarmResources(BuildingData building, List<SurvivorData> workers)
        {
            // 农场产出食物和有机物
            float baseProduction = 2f * (PRODUCTION_INTERVAL / 3600f);
            float workerBonus = CalculateWorkerBonus(workers, SurvivorAttributeType.Production);
            float techBonus = GetTechBonus("agriculture_efficiency");
            
            int foodAmount = Mathf.RoundToInt(baseProduction + workerBonus + techBonus);
            int organicAmount = Mathf.RoundToInt((baseProduction + workerBonus) * 0.3f);
            
            AddResource(ResourceType.Food, foodAmount);
            AddResource(ResourceType.OrganicMatter, organicAmount);
            
            // 给工人增加经验
            foreach (var worker in workers)
            {
                mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 3);
                worker.AddAttributeExperience(SurvivorAttributeType.Production, 2);
            }
        }
        
        private void ProduceWorkshopResources(BuildingData building, List<SurvivorData> workers)
        {
            // 工坊产出弹药和工具，需要消耗材料
            if (GetResourceAmount(ResourceType.Materials) < 2) return;
            
            float baseProduction = 1f * (PRODUCTION_INTERVAL / 3600f);
            float workerBonus = CalculateWorkerBonus(workers, SurvivorAttributeType.Technology);
            float techBonus = GetTechBonus("workshop_efficiency");
            
            int ammoAmount = Mathf.RoundToInt(baseProduction + workerBonus + techBonus);
            int toolsAmount = Mathf.RoundToInt((baseProduction + workerBonus) * 0.5f);
            
            if (TryConsumeResource(ResourceType.Materials, 2))
            {
                AddResource(ResourceType.Ammunition, ammoAmount);
                AddResource(ResourceType.Tools, toolsAmount);
                
                // 给工人增加经验
                foreach (var worker in workers)
                {
                    mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 4);
                    worker.AddAttributeExperience(SurvivorAttributeType.Technology, 3);
                }
            }
        }
        
        private void ProduceLaboratoryResources(BuildingData building, List<SurvivorData> workers)
        {
            // 实验室产出科研点数
            float baseProduction = 3f * (PRODUCTION_INTERVAL / 3600f);
            float workerBonus = CalculateWorkerBonus(workers, SurvivorAttributeType.Research);
            float techBonus = GetTechBonus("research_efficiency");
            
            int researchAmount = Mathf.RoundToInt(baseProduction + workerBonus + techBonus);
            
            AddResource(ResourceType.ResearchPoints, researchAmount);
            
            // 给工人增加经验
            foreach (var worker in workers)
            {
                mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 5);
                worker.AddAttributeExperience(SurvivorAttributeType.Research, 4);
            }
        }
        
        private void ProduceMedicalResources(BuildingData building, List<SurvivorData> workers)
        {
            // 医疗站产出医疗用品，需要消耗有机物
            if (GetResourceAmount(ResourceType.OrganicMatter) < 1) return;
            
            float baseProduction = 1f * (PRODUCTION_INTERVAL / 3600f);
            float workerBonus = CalculateWorkerBonus(workers, SurvivorAttributeType.Medical);
            float techBonus = GetTechBonus("medical_efficiency");
            
            int medicalAmount = Mathf.RoundToInt(baseProduction + workerBonus + techBonus);
            
            if (TryConsumeResource(ResourceType.OrganicMatter, 1))
            {
                AddResource(ResourceType.MedicalSupplies, medicalAmount);
                
                // 给工人增加经验
                foreach (var worker in workers)
                {
                    mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 4);
                    worker.AddAttributeExperience(SurvivorAttributeType.Medical, 3);
                }
            }
        }
        
        private void ProduceQuarryResources(BuildingData building, List<SurvivorData> workers)
        {
            // 采石场产出材料和废料
            float baseProduction = 2f * (PRODUCTION_INTERVAL / 3600f);
            float workerBonus = CalculateWorkerBonus(workers, SurvivorAttributeType.Production);
            
            int materialsAmount = Mathf.RoundToInt(baseProduction + workerBonus);
            int scrapAmount = Mathf.RoundToInt((baseProduction + workerBonus) * 0.8f);
            
            AddResource(ResourceType.Materials, materialsAmount);
            AddResource(ResourceType.Scrap, scrapAmount);
            
            // 给工人增加经验
            foreach (var worker in workers)
            {
                mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 3);
                worker.AddAttributeExperience(SurvivorAttributeType.Production, 2);
            }
        }
        
        private void ProduceWaterResources(BuildingData building, List<SurvivorData> workers)
        {
            // 净水器产出水，需要消耗能源
            if (GetResourceAmount(ResourceType.Energy) < 1) return;
            
            float baseProduction = 3f * (PRODUCTION_INTERVAL / 3600f);
            float workerBonus = CalculateWorkerBonus(workers, SurvivorAttributeType.Technology);
            
            int waterAmount = Mathf.RoundToInt(baseProduction + workerBonus);
            
            if (TryConsumeResource(ResourceType.Energy, 1))
            {
                AddResource(ResourceType.Water, waterAmount);
                
                // 给工人增加经验
                foreach (var worker in workers)
                {
                    mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 3);
                    worker.AddAttributeExperience(SurvivorAttributeType.Technology, 2);
                }
            }
        }
        
        private float CalculateWorkerBonus(List<SurvivorData> workers, SurvivorAttributeType attributeType)
        {
            float totalBonus = 0f;
            foreach (var worker in workers)
            {
                float efficiency = worker.GetTotalWorkEfficiency();
                float attributeValue = worker.GetAttributeValue(attributeType);
                totalBonus += (attributeValue / 100f) * efficiency * 0.5f;
            }
            return totalBonus;
        }
        
        private float GetTechBonus(string techEffectId)
        {
            if (mTechSystem == null) return 0f;
            return mTechSystem.GetTechEffectValue(techEffectId, 0f);
        }
        
        private void OnTechCompleted(TechCompletedEvent e)
        {
            // 科技完成时的处理
            Debug.Log($"科技完成，更新资源产出：{e.TechId}");
            //UpdateResourceProduction();
        }
        
        private void OnBuildingCompleted(BuildingConstructionCompletedEvent e)
        {
            // 建筑完成时更新资源产出
            Debug.Log($"建筑完成，更新资源产出：{e.ConfigId}");
           // UpdateResourceProduction();
        }
        
        /// <summary>
        /// 执行资源消耗逻辑
        /// 处理人口对食物和水的消耗，以及建筑维护消耗
        /// 资源不足时会对幸存者造成负面影响
        /// </summary>
        public void ConsumeResources()
        {
            // 计算人口消耗需求
            int population = mGameModel.Population.Value;
            int foodConsumption = population * 1;  // 每人每次消耗1单位食物
            int waterConsumption = population * 1; // 每人每次消耗1单位水
            
            // 尝试消耗食物
            if (!TryConsumeResource(ResourceType.Food, foodConsumption))
            {
                // 食物不足时的处理：降低幸存者士气和健康
                ReduceSurvivorStats(5, 3, "食物不足");
                this.SendEvent(new ResourceShortageEvent 
                { 
                    ResourceType = ResourceType.Food,
                    RequiredAmount = foodConsumption,
                    CurrentAmount = GetResourceAmount(ResourceType.Food)
                });
            }
            
            // 尝试消耗水
            if (!TryConsumeResource(ResourceType.Water, waterConsumption))
            {
                // 水不足时的处理：严重影响健康（水比食物更重要）
                ReduceSurvivorStats(3, 8, "水源不足");
                this.SendEvent(new ResourceShortageEvent 
                { 
                    ResourceType = ResourceType.Water,
                    RequiredAmount = waterConsumption,
                    CurrentAmount = GetResourceAmount(ResourceType.Water)
                });
            }
            
            // 处理建筑维护消耗
            ProcessBuildingMaintenance();
            
            // 发送资源消耗事件通知其他系统
            this.SendEvent(new ResourceConsumptionEvent { ConsumptionTime = Time.time });
            
            Debug.Log($"资源消耗完成 - 食物消耗:{foodConsumption} 水消耗:{waterConsumption}");
        }
        
        private void ReduceSurvivorStats(int moraleReduction, int healthReduction, string reason)
        {
            var survivors = mSurvivorSystem.GetAllSurvivors();
            foreach (var survivor in survivors)
            {
                survivor.Morale = Mathf.Max(0, survivor.Morale - moraleReduction);
                survivor.Health = Mathf.Max(0, survivor.Health - healthReduction);
            }
            
            Debug.LogWarning($"幸存者状态下降：{reason}");
        }
        
        private void ProcessBuildingMaintenance()
        {
            var buildings = mBuildingSystem.GetAllBuildings();
            
            foreach (var building in buildings)
            {
                if (building.State != BuildingState.Operational) continue;
                
                // 简化的维护消耗：每个建筑消耗1材料
                if (!TryConsumeResource(ResourceType.Materials, 1))
                {
                    // 维护不足，建筑损坏
                    DamageBuilding(building);
                }
            }
        }
        
        private void DamageBuilding(BuildingData building)
        {
            building.Health = Mathf.Max(0, building.Health - 10f);
            
            if (building.Health <= 0)
            {
                building.State = BuildingState.Damaged;
                Debug.LogWarning($"建筑因维护不足而损坏：{building.ConfigId}");
            }
        }
        
        /// <summary>
        /// 检查是否能承担指定的资源消耗列表
        /// </summary>
        /// <param name="costs">资源消耗列表，包含资源类型和数量</param>
        /// <returns>如果所有资源都足够则返回true，否则返回false</returns>
        public bool CanAfford(List<ResourceCost> costs)
        {
            if (costs == null) return true; // 空列表视为无消耗，直接返回true
            
            // 检查每项资源是否足够
            foreach (var cost in costs)
            {
                if (GetResourceAmount(cost.Type) < cost.Amount)
                    return false; // 任意一项资源不足就返回false
            }
            return true; // 所有资源都足够
        }
        
        /// <summary>
        /// 消耗指定的资源列表
        /// 只有在所有资源都足够的情况下才会执行实际消耗
        /// </summary>
        /// <param name="costs">要消耗的资源列表</param>
        /// <returns>消耗成功返回true，资源不足返回false</returns>
        public bool ConsumeResourcesList(List<ResourceCost> costs)
        {
            if (!CanAfford(costs)) return false; // 先检查是否能承担
            
            // 执行实际的资源消耗
            foreach (var cost in costs)
            {
                TryConsumeResource(cost.Type, cost.Amount);
            }
            return true;
        }
        
        public void ProcessResourceDecay()
        {
            // 食物腐坏
            int foodAmount = GetResourceAmount(ResourceType.Food);
            if (foodAmount > 0)
            {
                int decayAmount = Mathf.RoundToInt(foodAmount * 0.01f); // 1%腐坏率
                if (decayAmount > 0)
                {
                    TryConsumeResource(ResourceType.Food, decayAmount);
                    Debug.Log($"食物腐坏了 {decayAmount} 单位");
                }
            }
            
            // 医疗用品腐坏
            int medicalAmount = GetResourceAmount(ResourceType.MedicalSupplies);
            if (medicalAmount > 0)
            {
                int decayAmount = Mathf.RoundToInt(medicalAmount * 0.005f); // 0.5%腐坏率
                if (decayAmount > 0)
                {
                    TryConsumeResource(ResourceType.MedicalSupplies, decayAmount);
                    Debug.Log($"医疗用品腐坏了 {decayAmount} 单位");
                }
            }
        }
        
        /// <summary>
        /// 尝试消耗指定类型和数量的资源
        /// </summary>
        /// <param name="type">要消耗的资源类型</param>
        /// <param name="amount">要消耗的数量</param>
        /// <returns>消耗成功返回true，资源不足返回false</returns>
        public bool TryConsumeResource(ResourceType type, int amount)
        {
            return mGameModel.ConsumeResource(type, amount);
        }
        
        /// <summary>
        /// 添加指定类型和数量的资源
        /// 会自动发送资源变化事件通知其他系统
        /// </summary>
        /// <param name="type">要添加的资源类型</param>
        /// <param name="amount">要添加的数量</param>
        public void AddResource(ResourceType type, int amount)
        {
            int oldAmount = GetResourceAmount(type);
            mGameModel.AddResource(type, amount);
            int newAmount = GetResourceAmount(type);
            
            // 发送资源变化事件通知其他系统（如UI更新）
            this.SendEvent(new SurvivalGame.Model.ResourceChangedEvent 
            { 
                Type = type, 
                OldAmount = oldAmount,
                NewAmount = newAmount,
                Change = amount
            });
        }
        
        /// <summary>
        /// 获取指定类型资源的当前数量
        /// </summary>
        /// <param name="type">资源类型</param>
        /// <returns>资源数量</returns>
        public int GetResourceAmount(ResourceType type)
        {
            return mGameModel.GetResourceAmount(type);
        }
        
        public float GetResourceProductionRate(ResourceType type)
        {
            // 计算每分钟产出量
            float totalProduction = 0f;
            
            var buildings = mBuildingSystem.GetAllBuildings();
            foreach (var building in buildings)
            {
                if (building.State != BuildingState.Operational) continue;
                
                var workers = mSurvivorSystem.GetBuildingWorkers(building.Id);
                if (workers.Count == 0) continue;
                
                // 根据建筑类型和资源类型计算产出
                float production = GetBuildingProductionForResource(building, type, workers);
                totalProduction += production;
            }
            
            return totalProduction * (60f / PRODUCTION_INTERVAL);
        }
        
        private float GetBuildingProductionForResource(BuildingData building, ResourceType type, List<SurvivorData> workers)
        {
            switch (building.ConfigId)
            {
                case "Farm":
                    if (type == ResourceType.Food)
                    {
                        float baseProduction = 2f * (PRODUCTION_INTERVAL / 3600f);
                        float workerBonus = CalculateWorkerBonus(workers, SurvivorAttributeType.Production);
                        return baseProduction + workerBonus;
                    }
                    break;
                case "Workshop":
                    if (type == ResourceType.Ammunition)
                    {
                        float baseProduction = 1f * (PRODUCTION_INTERVAL / 3600f);
                        float workerBonus = CalculateWorkerBonus(workers, SurvivorAttributeType.Technology);
                        return baseProduction + workerBonus;
                    }
                    break;
                case "Laboratory":
                    if (type == ResourceType.ResearchPoints)
                    {
                        float baseProduction = 3f * (PRODUCTION_INTERVAL / 3600f);
                        float workerBonus = CalculateWorkerBonus(workers, SurvivorAttributeType.Research);
                        return baseProduction + workerBonus;
                    }
                    break;
            }
            return 0f;
        }
        
        public float GetResourceConsumptionRate(ResourceType type)
        {
            // 计算每分钟消耗量
            float totalConsumption = 0f;
            
            switch (type)
            {
                case ResourceType.Food:
                case ResourceType.Water:
                    totalConsumption = mGameModel.Population.Value * (60f / CONSUMPTION_INTERVAL);
                    break;
                case ResourceType.Materials:
                    // 建筑维护消耗
                    var buildings = mBuildingSystem.GetAllBuildings();
                    totalConsumption = buildings.Count * (60f / CONSUMPTION_INTERVAL);
                    break;
            }
            
            return totalConsumption;
        }
        
        public void UpdateResourceProduction()
        {
            // 立即执行一次资源产出
           // ProduceResources();
        }
    }
    
    // 资源系统相关事件定义
    
    /// <summary>
    /// 资源产出事件
    /// 当资源产出完成时发送此事件
    /// </summary>
    public struct ResourceProductionEvent
    {
        public float ProductionTime;    // 产出时间戳
    }
    
    /// <summary>
    /// 资源消耗事件
    /// 当资源消耗完成时发送此事件
    /// </summary>
    public struct ResourceConsumptionEvent
    {
        public float ConsumptionTime;   // 消耗时间戳
    }
    
    /// <summary>
    /// 资源短缺事件
    /// 当某种资源不足以满足需求时发送此事件
    /// </summary>
    public struct ResourceShortageEvent
    {
        public ResourceType ResourceType;   // 短缺的资源类型
        public int RequiredAmount;          // 需要的数量
        public int CurrentAmount;           // 当前拥有的数量
    }
} 