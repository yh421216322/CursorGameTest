// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ResourceSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了资源管理系统 (ResourceSystem)，负责处理游戏中所有资源的
//     生产、消耗、存储、腐坏以及相关的查询操作。该系统与其他系统（如建筑、
//     科技、幸存者）交互，以动态调整资源状态，并响应游戏事件。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq; // 引入 LINQ 用于更方便的数据操作
using QFramework;
using SurvivalGame.Model;
using UnityEngine; // 引入 UnityEngine 用于 Time.time 和 Mathf 等功能

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 资源管理系统接口 (IResourceSystem)。
    /// 定义了资源管理的核心功能和职责，包括：
    /// 1. 定期处理资源消耗（人口需求、建筑维护）。
    /// 2. 提供资源查询（数量、产出/消耗速率）和操作（增加、消耗、检查余量）的接口。
    /// 3. 处理资源的腐坏逻辑。
    /// 4. （注意：原有的 ProduceResources() 方法已被注释，暗示资源产出可能由其他系统如 EnhancedBuildingSystem 主导）
    /// </summary>
    public interface IResourceSystem : QFISystem
    {
        /// <summary>
        /// （已注释）定期资源产出 - 原计划根据建筑和工人产出各种资源。
        /// 当前资源产出逻辑可能已移至 EnhancedBuildingSystem。
        /// </summary>
        //  void ProduceResources();
        
        /// <summary>
        /// 定期处理各类资源的消耗，例如人口对食物和水的需求，以及建筑的维护成本。
        /// </summary>
        void ConsumeResources();
        
        /// <summary>
        /// 检查当前是否拥有足够的资源来承担指定的资源成本列表。
        /// </summary>
        /// <param name="costs">一个包含多种资源及其所需数量的成本列表。</param>
        /// <returns>如果所有指定资源都足够，则返回true；否则返回false。</returns>
        bool CanAfford(List<ResourceCost> costs);
        
        /// <summary>
        /// 尝试消耗指定的资源列表。只有在所有资源都充足的情况下才会实际扣除。
        /// </summary>
        /// <param name="costs">要消耗的资源及其数量的列表。</param>
        /// <returns>如果成功消耗所有资源则返回true；如果任一资源不足导致无法消耗，则返回false。</returns>
        bool ConsumeResourcesList(List<ResourceCost> costs);
        
        /// <summary>
        /// 尝试消耗指定类型和数量的单一资源。
        /// </summary>
        /// <param name="type">要消耗的资源类型。</param>
        /// <param name="amount">要消耗的数量。</param>
        /// <returns>如果资源足够并成功消耗则返回true，否则返回false。</returns>
        bool TryConsumeResource(ResourceType type, int amount);
        
        /// <summary>
        /// 向库存中添加指定类型和数量的资源。
        /// </summary>
        /// <param name="type">要添加的资源类型。</param>
        /// <param name="amount">要添加的数量 (应为正数)。</param>
        void AddResource(ResourceType type, int amount);
        
        /// <summary>
        /// 获取指定资源类型当前的库存数量。
        /// </summary>
        /// <param name="type">要查询的资源类型。</param>
        /// <returns>该资源的当前数量。</returns>
        int GetResourceAmount(ResourceType type);
        
        /// <summary>
        /// 获取指定资源类型的预计产出速率（例如，单位资源/分钟）。
        /// 此计算可能基于当前所有生产建筑、工人分配和科技加成等。
        /// </summary>
        /// <param name="type">要查询的资源类型。</param>
        /// <returns>该资源的预计产出速率。</returns>
        float GetResourceProductionRate(ResourceType type);
        
        /// <summary>
        /// 获取指定资源类型的预计消耗速率（例如，单位资源/分钟）。
        /// 此计算可能基于人口需求、建筑维护成本等。
        /// </summary>
        /// <param name="type">要查询的资源类型。</param>
        /// <returns>该资源的预计消耗速率。</returns>
        float GetResourceConsumptionRate(ResourceType type);
        
        /// <summary>
        /// （可能用于调试或特殊事件）立即触发并更新一次资源产出逻辑。
        /// 注意：实际的周期性产出由EnhancedBuildingSystem处理。
        /// </summary>
        void UpdateResourceProduction();
        
        /// <summary>
        /// 处理易腐资源的腐坏逻辑，例如食物、医疗用品等。
        /// </summary>
        void ProcessResourceDecay();
    }
    
    /// <summary>
    /// 资源管理系统 (ResourceSystem) 的具体实现类。
    /// 负责游戏中所有资源的跟踪、生产（间接）、消耗和腐坏等核心逻辑。
    /// </summary>
    public class ResourceSystem : AbstractSystem, IResourceSystem
    {
        // --- 依赖的系统和模型 (Dependent Systems and Models) ---
        /// <summary>
        /// 游戏核心数据模型，用于存储和访问全局游戏状态，如资源库存。
        /// </summary>
        private ISurvivalGameModel mGameModel;
        /// <summary>
        /// 科技系统接口，用于查询已研究科技带来的资源相关加成。
        /// </summary>
        private IAdvancedTechSystem mTechSystem;
        /// <summary>
        /// （增强型）建筑系统接口，用于获取建筑信息（例如，哪些建筑在生产特定资源）。
        /// </summary>
        private IEnhancedBuildingSystem mBuildingSystem;
        /// <summary>
        /// 幸存者（工人）系统接口，用于获取工人对资源生产的效率影响。
        /// </summary>
        private ISurvivorSystem mSurvivorSystem;
        
        // --- 时间记录变量 (Time Tracking Variables) ---
        /// <summary>
        /// 上一次执行资源产出逻辑的时间戳。 (注意：主要产出逻辑已移至EnhancedBuildingSystem)
        /// </summary>
        private float mLastProductionTime;
        /// <summary>
        /// 上一次执行资源消耗逻辑的时间戳。
        /// </summary>
        private float mLastConsumptionTime;
        /// <summary>
        /// 上一次执行资源腐坏检查逻辑的时间戳。
        /// </summary>
        private float mLastDecayTime;
        
        // --- 时间间隔常量 (Time Interval Constants) ---
        /// <summary>
        /// 资源产出检查的时间间隔（秒）。(注意：主要产出逻辑已移至EnhancedBuildingSystem)
        /// </summary>
        private const float PRODUCTION_INTERVAL = 5f;   // 原计划每5秒产出一次，现主要由建筑系统驱动
        /// <summary>
        /// 资源消耗检查的时间间隔（秒）。
        /// </summary>
        private const float CONSUMPTION_INTERVAL = 10f; // 资源消耗（如人口需求）每10秒计算一次
        /// <summary>
        /// 资源腐坏检查的时间间隔（秒）。
        /// </summary>
        private const float DECAY_INTERVAL = 60f;       // 易腐资源每60秒检查一次腐坏状态
        
        /// <summary>
        /// 系统初始化方法。
        /// 在此方法中获取对其他系统和数据模型的引用，并注册相关的游戏事件监听器，初始化时间记录。
        /// </summary>
        protected override void OnInit()
        {
            // 获取依赖的系统和模型实例
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mTechSystem = this.GetSystem<IAdvancedTechSystem>();
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            mSurvivorSystem = this.GetSystem<ISurvivorSystem>();
            
            // 初始化上次执行各项逻辑的时间戳为当前游戏时间
            mLastProductionTime = Time.time;
            mLastConsumptionTime = Time.time;
            mLastDecayTime = Time.time;
            
            // 注册监听游戏更新、科技完成和建筑完成事件
            this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);               // 监听游戏每帧更新事件
            this.RegisterEvent<TechCompletedEvent>(OnTechCompleted);         // 监听科技研发完成事件
            this.RegisterEvent<BuildingConstructionCompletedEvent>(OnBuildingCompleted); // 监听建筑建造完成事件
            
            Debug.Log("[资源系统] 初始化完成。");
        }
        
        /// <summary>
        /// 处理游戏更新事件。
        /// 根据预设的时间间隔，定期执行资源的消耗和腐坏检查逻辑。
        /// （注意：原有的资源产出逻辑已注释掉，暗示产出由EnhancedBuildingSystem更主动地管理）
        /// </summary>
        /// <param name="e">游戏更新事件参数，包含deltaTime。</param>
        private void OnGameUpdate(GameUpdateEvent e)
        {
            if (mGameModel.IsPaused.Value) return; // 如果游戏已暂停，则不执行任何逻辑
            
            float currentTime = Time.time; // 获取当前游戏时间
            
            // // 检查是否到达资源产出时间 (此部分逻辑已注释，产出主要由EnhancedBuildingSystem驱动)
            // if (currentTime - mLastProductionTime >= PRODUCTION_INTERVAL)
            // {
            //     ProduceResources(); // 原计划调用产出方法
            //     mLastProductionTime = currentTime; // 更新时间戳
            // }
            
            // 检查是否到达资源消耗计算时间
            if (currentTime - mLastConsumptionTime >= CONSUMPTION_INTERVAL)
            {
                ConsumeResources(); // 执行资源消耗逻辑
                mLastConsumptionTime = currentTime; // 更新时间戳
            }
            
            // 检查是否到达资源腐坏检查时间
            if (currentTime - mLastDecayTime >= DECAY_INTERVAL)
            {
                ProcessResourceDecay(); // 执行资源腐坏逻辑
                mLastDecayTime = currentTime; // 更新时间戳
            }
        }
        
        /// <summary>
        /// （已注释）执行资源产出逻辑。
        /// 原计划遍历所有可运营的建筑，并根据建筑类型和工人情况调用ProduceFromBuilding产出相应资源。
        /// 当前此方法及其调用已被注释，资源产出由 EnhancedBuildingSystem.UpdateBuildingProduction 更细致地管理。
        /// </summary>
        // public void ProduceResources()
        // {
        //     var buildings = mBuildingSystem.GetAllBuildings();
        //     foreach (var building in buildings)
        //     {
        //         if (building.State != BuildingState.Operational) continue;
        //         ProduceFromBuilding(building);
        //     }
        //     this.SendEvent(new ResourceProductionEvent { ProductionTime = Time.time });
        //     Debug.Log($"资源产出完成 - 食物:{mGameModel.Food.Value} 弹药:{mGameModel.Ammunition.Value} 材料:{mGameModel.Materials.Value}");
        // }
        
        /// <summary>
        /// （辅助方法，当前未被直接周期性调用）根据单个建筑的类型和分配的工人情况来产出资源。
        /// 此方法包含针对不同建筑（农田、工坊等）的特定产出逻辑。
        /// 注意：此方法的调用逻辑已大部分移至 EnhancedBuildingSystem。
        /// </summary>
        /// <param name="building">要进行资源产出的建筑数据对象。</param>
        private void ProduceFromBuilding(BuildingData building)
        {
            // 获取在该建筑工作的幸存者（工人）列表
            var workers = mSurvivorSystem.GetBuildingWorkers(building.Id);
            // 如果建筑类型配置为需要工人但当前没有工人，则不产出 (具体逻辑在各ProduceXXXResources中判断)
            // if (config.RequiresWorkers && workers.Count == 0) return;
            
            // 根据建筑的配置ID，调用相应的具体资源产出方法
            switch (building.ConfigId)
            {
                case "Farm": // 农场
                    ProduceFarmResources(building, workers);        // 产出食物和有机物
                    break;
                case "Workshop": // 工坊
                    ProduceWorkshopResources(building, workers);    // 产出弹药和工具
                    break;
                case "Laboratory": // 实验室
                    ProduceLaboratoryResources(building, workers);  // 产出科研点数
                    break;
                case "MedicalStation": // 医疗站
                    ProduceMedicalResources(building, workers);     // 产出医疗用品
                    break;
                case "Quarry": // 采石场
                    ProduceQuarryResources(building, workers);      // 产出材料和废料
                    break;
                case "WaterPurifier": // 净水器
                    ProduceWaterResources(building, workers);       // 产出水
                    break;
                // 可根据需要添加更多建筑类型的产出逻辑
            }
        }
        
        /// <summary>
        /// （辅助方法）处理农场的资源产出（食物、有机物）。
        /// </summary>
        private void ProduceFarmResources(BuildingData building, List<SurvivorData> workers)
        {
            // 农场产出食物和有机物
            float baseProductionFood = 2f * (PRODUCTION_INTERVAL / 3600f); // 食物基础产率 (单位/产出周期)
            float workerBonusFood = CalculateWorkerBonus(workers, SurvivorAttributeType.Production); // 工人对食物的加成
            float techBonusFood = GetTechBonus("agriculture_efficiency"); // 农业科技对食物的加成
            
            int foodAmountToProduce = Mathf.RoundToInt(baseProductionFood + workerBonusFood + techBonusFood); // 总食物产出
            int organicAmountToProduce = Mathf.RoundToInt((baseProductionFood + workerBonusFood) * 0.3f); // 有机物副产品 (示例：食物产量的30%)
            
            if (foodAmountToProduce > 0) AddResource(ResourceType.Food, foodAmountToProduce);
            if (organicAmountToProduce > 0) AddResource(ResourceType.OrganicMatter, organicAmountToProduce);
            
            // 为参与工作的工人增加经验值
            foreach (var worker in workers)
            {
                mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 3); // 增加3点通用经验
                worker.AddAttributeExperience(SurvivorAttributeType.Production, 2); // 增加2点生产属性经验
            }
        }
        
        /// <summary>
        /// （辅助方法）处理工坊的资源产出（弹药、工具），消耗材料。
        /// </summary>
        private void ProduceWorkshopResources(BuildingData building, List<SurvivorData> workers)
        {
            // 工坊产出弹药和工具，需要消耗2单位材料作为前提
            if (GetResourceAmount(ResourceType.Materials) < 2) return; // 材料不足则不生产
            
            float baseProductionAmmo = 1f * (PRODUCTION_INTERVAL / 3600f); // 弹药基础产率
            float workerBonusTech = CalculateWorkerBonus(workers, SurvivorAttributeType.Technology); // 工人科技属性加成
            float techBonusWorkshop = GetTechBonus("workshop_efficiency"); // 工坊效率科技加成
            
            int ammoAmountToProduce = Mathf.RoundToInt(baseProductionAmmo + workerBonusTech + techBonusWorkshop); // 总弹药产出
            int toolsAmountToProduce = Mathf.RoundToInt((baseProductionAmmo + workerBonusTech) * 0.5f); // 工具副产品 (示例：弹药产量的50%)
            
            if (TryConsumeResource(ResourceType.Materials, 2)) // 尝试消耗2单位材料
            {
                if (ammoAmountToProduce > 0) AddResource(ResourceType.Ammunition, ammoAmountToProduce);
                if (toolsAmountToProduce > 0) AddResource(ResourceType.Tools, toolsAmountToProduce);
                
                foreach (var worker in workers)
                {
                    mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 4); // 增加4点通用经验
                    worker.AddAttributeExperience(SurvivorAttributeType.Technology, 3); // 增加3点科技属性经验
                }
            }
        }
        
        /// <summary>
        /// （辅助方法）处理实验室的资源产出（科研点数）。
        /// </summary>
        private void ProduceLaboratoryResources(BuildingData building, List<SurvivorData> workers)
        {
            // 实验室产出科研点数
            float baseProductionResearch = 3f * (PRODUCTION_INTERVAL / 3600f); // 科研点基础产率
            float workerBonusResearch = CalculateWorkerBonus(workers, SurvivorAttributeType.Research); // 工人研究属性加成
            float techBonusResearch = GetTechBonus("research_efficiency"); // 科研效率科技加成
            
            int researchAmountToProduce = Mathf.RoundToInt(baseProductionResearch + workerBonusResearch + techBonusResearch); // 总科研点产出
            
            if (researchAmountToProduce > 0) AddResource(ResourceType.ResearchPoints, researchAmountToProduce);
            
            foreach (var worker in workers)
            {
                mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 5); // 增加5点通用经验
                worker.AddAttributeExperience(SurvivorAttributeType.Research, 4); // 增加4点研究属性经验
            }
        }
        
        /// <summary>
        /// （辅助方法）处理医疗站的资源产出（医疗用品），消耗有机物。
        /// </summary>
        private void ProduceMedicalResources(BuildingData building, List<SurvivorData> workers)
        {
            // 医疗站产出医疗用品，需要消耗1单位有机物
            if (GetResourceAmount(ResourceType.OrganicMatter) < 1) return; // 有机物不足则不生产
            
            float baseProductionMedical = 1f * (PRODUCTION_INTERVAL / 3600f); // 医疗用品基础产率
            float workerBonusMedical = CalculateWorkerBonus(workers, SurvivorAttributeType.Medical); // 工人医疗属性加成
            float techBonusMedical = GetTechBonus("medical_efficiency"); // 医疗效率科技加成
            
            int medicalAmountToProduce = Mathf.RoundToInt(baseProductionMedical + workerBonusMedical + techBonusMedical); // 总医疗用品产出
            
            if (TryConsumeResource(ResourceType.OrganicMatter, 1)) // 尝试消耗1单位有机物
            {
                if (medicalAmountToProduce > 0) AddResource(ResourceType.MedicalSupplies, medicalAmountToProduce);
                
                foreach (var worker in workers)
                {
                    mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 4); // 增加4点通用经验
                    worker.AddAttributeExperience(SurvivorAttributeType.Medical, 3); // 增加3点医疗属性经验
                }
            }
        }
        
        /// <summary>
        /// （辅助方法）处理采石场的资源产出（材料、废料）。
        /// </summary>
        private void ProduceQuarryResources(BuildingData building, List<SurvivorData> workers)
        {
            // 采石场产出材料和废料
            float baseProductionMaterials = 2f * (PRODUCTION_INTERVAL / 3600f); // 材料基础产率
            float workerBonusProduction = CalculateWorkerBonus(workers, SurvivorAttributeType.Production); // 工人生产属性加成
            // (此处未加入特定科技加成，可按需添加如 "quarry_efficiency")
            
            int materialsAmountToProduce = Mathf.RoundToInt(baseProductionMaterials + workerBonusProduction); // 总材料产出
            int scrapAmountToProduce = Mathf.RoundToInt((baseProductionMaterials + workerBonusProduction) * 0.8f); // 废料副产品 (示例：材料产量的80%)
            
            if (materialsAmountToProduce > 0) AddResource(ResourceType.Materials, materialsAmountToProduce);
            if (scrapAmountToProduce > 0) AddResource(ResourceType.Scrap, scrapAmountToProduce);
            
            foreach (var worker in workers)
            {
                mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 3);
                worker.AddAttributeExperience(SurvivorAttributeType.Production, 2);
            }
        }
        
        /// <summary>
        /// （辅助方法）处理净水器的资源产出（水），消耗能源。
        /// </summary>
        private void ProduceWaterResources(BuildingData building, List<SurvivorData> workers)
        {
            // 净水器产出水，需要消耗1单位能源
            if (GetResourceAmount(ResourceType.Energy) < 1) return; // 能源不足则不生产
            
            float baseProductionWater = 3f * (PRODUCTION_INTERVAL / 3600f); // 水基础产率
            float workerBonusTechnology = CalculateWorkerBonus(workers, SurvivorAttributeType.Technology); // 工人科技属性加成
            // (此处未加入特定科技加成，可按需添加如 "water_purification_efficiency")

            int waterAmountToProduce = Mathf.RoundToInt(baseProductionWater + workerBonusTechnology); // 总水产出
            
            if (TryConsumeResource(ResourceType.Energy, 1)) // 尝试消耗1单位能源
            {
                if (waterAmountToProduce > 0) AddResource(ResourceType.Water, waterAmountToProduce);
                
                foreach (var worker in workers)
                {
                    mSurvivorSystem.AddExperienceToSurvivor(worker.Id, 3);
                    worker.AddAttributeExperience(SurvivorAttributeType.Technology, 2);
                }
            }
        }
        
        /// <summary>
        /// （辅助方法）计算由工人技能和效率带来的生产加成。
        /// </summary>
        /// <param name="workers">参与工作的工人列表。</param>
        /// <param name="attributeType">影响生产效率的相关幸存者属性类型。</param>
        /// <returns>总的工人生产加成值。</returns>
        private float CalculateWorkerBonus(List<SurvivorData> workers, SurvivorAttributeType attributeType)
        {
            float totalBonus = 0f;
            if (workers == null) return totalBonus;

            foreach (var worker in workers)
            {
                float efficiency = worker.GetTotalWorkEfficiency(); // 获取工人的综合工作效率 (0-1范围)
                float attributeValue = worker.GetAttributeValue(attributeType); // 获取工人相关属性值
                // 示例加成公式：(属性值 / 100) * 个人效率 * 基础加成系数 (0.5)
                // 这个公式需要根据游戏平衡仔细设计
                totalBonus += (attributeValue / 100f) * efficiency * 0.5f;
            }
            return totalBonus;
        }
        
        /// <summary>
        /// （辅助方法）获取指定科技效果ID带来的生产加成值。
        /// </summary>
        /// <param name="techEffectId">科技效果的唯一ID。</param>
        /// <returns>科技提供的加成值（通常是百分比的小数形式，如0.1代表10%）。</returns>
        private float GetTechBonus(string techEffectId)
        {
            if (mTechSystem == null) return 0f; // 如果科技系统不存在，则无加成
            // 从科技系统获取效果值，如果效果不存在则默认为0
            return mTechSystem.GetTechEffectValue(techEffectId, 0f);
        }
        
        /// <summary>
        /// 处理科技研发完成事件。
        /// （当前仅打印日志，可扩展用于更新与新科技相关的资源产耗逻辑）
        /// </summary>
        /// <param name="e">科技完成事件参数。</param>
        private void OnTechCompleted(TechCompletedEvent e)
        {
            Debug.Log($"[资源系统] 科技 {e.TechId} 完成，可能需要更新资源产出逻辑。");
            // UpdateResourceProduction(); // (已注释)原计划在科技完成后立即更新一次产出，视具体逻辑决定是否需要
        }
        
        /// <summary>
        /// 处理建筑建造完成事件。
        /// （当前仅打印日志，可扩展用于更新与新建筑相关的资源产耗逻辑）
        /// </summary>
        /// <param name="e">建筑完成事件参数。</param>
        private void OnBuildingCompleted(BuildingConstructionCompletedEvent e)
        {
            Debug.Log($"[资源系统] 建筑 {e.ConfigId} (ID: {e.BuildingId}) 完成，可能需要更新资源产出逻辑。");
           // UpdateResourceProduction(); // (已注释)原计划在建筑完成后立即更新一次产出
        }
        
        /// <summary>
        /// 执行周期性的资源消耗逻辑。
        /// 主要处理人口对食物和水的消耗，以及建筑物的日常维护消耗。
        /// 如果资源不足，会触发相应的负面效果（如降低士气、健康，建筑损坏）。
        /// </summary>
        public void ConsumeResources()
        {
            // 1. 计算并处理人口的食物和水消耗
            int population = mGameModel.Population.Value;
            int foodNeeded = population * 1;  // 简化：每人每消耗周期需要1单位食物
            int waterNeeded = population * 1; // 简化：每人每消耗周期需要1单位水
            
            // 尝试消耗食物
            if (!TryConsumeResource(ResourceType.Food, foodNeeded))
            {
                // 食物不足，对幸存者施加负面状态
                ReduceSurvivorStats(5, 3, "食物不足，士气和健康受到影响！"); // 示例：士气-5，健康-3
                // 发送资源短缺事件，UI可以监听此事件以通知玩家
                this.SendEvent(new ResourceShortageEvent 
                { 
                    ResourceType = ResourceType.Food,
                    RequiredAmount = foodNeeded,
                    CurrentAmount = GetResourceAmount(ResourceType.Food)
                });
            }
            
            // 尝试消耗水
            if (!TryConsumeResource(ResourceType.Water, waterNeeded))
            {
                // 水资源不足，施加更严重的负面状态
                ReduceSurvivorStats(3, 8, "饮用水匮乏，健康状况恶化！"); // 示例：士气-3，健康-8
                this.SendEvent(new ResourceShortageEvent 
                { 
                    ResourceType = ResourceType.Water,
                    RequiredAmount = waterNeeded,
                    CurrentAmount = GetResourceAmount(ResourceType.Water)
                });
            }
            
            // 2. 处理所有建筑的维护消耗
            ProcessBuildingMaintenance();
            
            // 发送资源消耗周期完成事件
            this.SendEvent(new ResourceConsumptionEvent { ConsumptionTime = Time.time });
            
            Debug.Log($"[资源系统] 周期性资源消耗处理完毕。食物消耗: {foodNeeded} (需求), 水消耗: {waterNeeded} (需求)。");
        }
        
        /// <summary>
        /// （辅助方法）当关键资源（如食物、水）不足时，降低所有幸存者的士气和健康。
        /// </summary>
        /// <param name="moraleReduction">士气降低值。</param>
        /// <param name="healthReduction">健康降低值。</param>
        /// <param name="reasonLogMessage">记录到日志的原因说明。</param>
        private void ReduceSurvivorStats(int moraleReduction, int healthReduction, string reasonLogMessage)
        {
            var survivors = mSurvivorSystem.GetAllSurvivors(); // 获取所有幸存者
            foreach (var survivor in survivors)
            {
                survivor.Morale = Mathf.Max(0, survivor.Morale - moraleReduction); // 降低士气，不低于0
                survivor.Health = Mathf.Max(0, survivor.Health - healthReduction); // 降低健康，不低于0
                // TODO: 可能还需要检查幸存者是否因此死亡
            }
            
            Debug.LogWarning($"[资源系统] 由于 {reasonLogMessage}，所有幸存者状态下降！士气 -{moraleReduction}, 健康 -{healthReduction}。");
        }
        
        /// <summary>
        /// （辅助方法）处理所有运作中建筑的维护资源消耗。
        /// 如果维护资源不足，则对建筑造成损害。
        /// </summary>
        private void ProcessBuildingMaintenance()
        {
            var buildings = mBuildingSystem.GetAllBuildings(); // 获取所有建筑
            
            foreach (var building in buildings)
            {
                if (building.State != BuildingState.Operational) continue; // 只处理运作中的建筑
                
                // TODO: 维护成本应从BuildingConfig中读取
                // 简化示例：每个运作中的建筑每周期消耗1单位材料进行维护
                if (!TryConsumeResource(ResourceType.Materials, 1))
                {
                    // 维护材料不足，对建筑造成损害
                    DamageBuilding(building);
                }
            }
        }
        
        /// <summary>
        /// （辅助方法）对指定的建筑造成因缺乏维护等原因的损害。
        /// </summary>
        /// <param name="building">要损坏的建筑数据对象。</param>
        private void DamageBuilding(BuildingData building)
        {
            building.Health = Mathf.Max(0, building.Health - 10f); // 示例：每次维护失败损失10点耐久
            
            if (building.Health <= 0) // 如果耐久度降至0或以下
            {
                building.State = BuildingState.Damaged; // 将建筑状态设为损坏 (或Destroyed，取决于设计)
                Debug.LogWarning($"[资源系统] 建筑 {building.ConfigId} (ID: {building.Id}) 因长期缺乏维护而严重损坏/无法运作！");
                // TODO: 发送建筑损坏事件
            }
            else
            {
                Debug.Log($"[资源系统] 建筑 {building.ConfigId} (ID: {building.Id}) 因缺乏维护而受损，当前耐久: {building.Health}。");
            }
        }
        
        /// <summary>
        /// 检查当前是否拥有足够的资源来支付指定的资源成本列表。
        /// </summary>
        /// <param name="costs">一个包含多种资源及其所需数量的成本列表。</param>
        /// <returns>如果所有指定资源都足够，则返回true；否则返回false。</returns>
        public bool CanAfford(List<ResourceCost> costs)
        {
            if (costs == null || !costs.Any()) return true; // 如果成本列表为空或null，视为可承担
            
            // 遍历检查每一项资源成本
            foreach (var cost in costs)
            {
                if (GetResourceAmount(cost.Type) < cost.Amount) // 如果当前资源量少于需求量
                    return false; // 任一资源不足，则无法承担
            }
            return true; // 所有资源均充足
        }
        
        /// <summary>
        /// 尝试消耗指定的资源列表。只有在所有资源都充足的情况下才会实际扣除。
        /// </summary>
        /// <param name="costs">要消耗的资源及其数量的列表。</param>
        /// <returns>如果成功消耗所有资源则返回true；如果任一资源不足导致无法消耗，则返回false。</returns>
        public bool ConsumeResourcesList(List<ResourceCost> costs)
        {
            if (!CanAfford(costs)) return false; // 首先检查是否能承担所有成本
            
            // 如果能承担，则逐项执行实际的资源消耗
            if (costs != null)
            {
                foreach (var cost in costs)
                {
                    TryConsumeResource(cost.Type, cost.Amount); // 调用单项资源消耗方法
                }
            }
            return true;
        }
        
        /// <summary>
        /// 处理周期性的资源腐坏逻辑。
        /// 例如，食物和医疗用品可能会随时间变质。
        /// </summary>
        public void ProcessResourceDecay()
        {
            // 处理食物腐坏
            int currentFood = GetResourceAmount(ResourceType.Food);
            if (currentFood > 0)
            {
                // 示例：每周期固定腐坏当前食物总量的1% (可配置或更复杂模型)
                int foodDecayAmount = Mathf.RoundToInt(currentFood * 0.01f);
                if (foodDecayAmount > 0)
                {
                    TryConsumeResource(ResourceType.Food, foodDecayAmount); // 消耗腐坏的食物
                    Debug.Log($"[资源系统] {foodDecayAmount} 单位食物因腐坏而损失。");
                }
            }
            
            // 处理医疗用品腐坏/过期
            int currentMedicalSupplies = GetResourceAmount(ResourceType.MedicalSupplies);
            if (currentMedicalSupplies > 0)
            {
                // 示例：每周期固定腐坏当前医疗用品总量的0.5%
                int medicalDecayAmount = Mathf.RoundToInt(currentMedicalSupplies * 0.005f);
                if (medicalDecayAmount > 0)
                {
                    TryConsumeResource(ResourceType.MedicalSupplies, medicalDecayAmount); // 消耗腐坏的医疗用品
                    Debug.Log($"[资源系统] {medicalDecayAmount} 单位医疗用品因过期而损失。");
                }
            }
            // TODO: 可以为其他易腐资源添加类似逻辑
        }
        
        /// <summary>
        /// 尝试消耗指定类型和数量的单一资源。
        /// </summary>
        /// <param name="type">要消耗的资源类型。</param>
        /// <param name="amount">要消耗的数量 (应为正数)。</param>
        /// <returns>如果资源足够并成功消耗则返回true，否则返回false。</returns>
        public bool TryConsumeResource(ResourceType type, int amount)
        {
            if (amount <= 0) return true; // 消耗非正数视为成功，不改变资源
            return mGameModel.ConsumeResource(type, amount); // 调用数据模型层的方法执行消耗
        }
        
        /// <summary>
        /// 向库存中添加指定类型和数量的资源。
        /// 此方法会更新数据模型，并发送资源变化事件以通知UI等其他系统。
        /// </summary>
        /// <param name="type">要添加的资源类型。</param>
        /// <param name="amount">要添加的数量 (应为正数)。</param>
        public void AddResource(ResourceType type, int amount)
        {
            if (amount <= 0) return; // 不添加非正数数量的资源

            int oldAmount = GetResourceAmount(type); // 获取添加前的数量
            mGameModel.AddResource(type, amount);    // 调用数据模型层的方法执行添加
            int newAmount = GetResourceAmount(type); // 获取添加后的数量
            
            // 发送资源数量已变化的事件，包含变化的详情
            this.SendEvent(new SurvivalGame.Model.ResourceChangedEvent 
            { 
                Type = type,          // 发生变化的资源类型
                OldAmount = oldAmount,  // 原有数量
                NewAmount = newAmount,  // 新的数量
                Change = amount       // 本次变化的量 (正数为增加)
            });
        }
        
        /// <summary>
        /// 获取指定资源类型当前的库存数量。
        /// </summary>
        /// <param name="type">要查询的资源类型。</param>
        /// <returns>该资源的当前数量。</returns>
        public int GetResourceAmount(ResourceType type)
        {
            return mGameModel.GetResourceAmount(type); // 从数据模型获取资源数量
        }
        
        /// <summary>
        /// 获取指定资源类型的预计总产出速率（单位资源/分钟）。
        /// </summary>
        /// <param name="type">要查询的资源类型。</param>
        /// <returns>该资源的预计产出速率。</returns>
        public float GetResourceProductionRate(ResourceType type)
        {
            // 注意：此方法计算的是“每分钟”的速率，而内部PRODUCTION_INTERVAL是“每周期”
            // 因此需要进行单位转换。
            float totalProductionPerCycle = 0f;
            
            var buildings = mBuildingSystem.GetAllBuildings(); // 获取所有建筑
            foreach (var building in buildings)
            {
                if (building.State != BuildingState.Operational) continue; // 只统计运作中的建筑
                
                var workers = mSurvivorSystem.GetBuildingWorkers(building.Id); // 获取该建筑的工人
                // 注意：原GetBuildingProductionForResource可能需要调整为返回每“周期”的量，或此方法直接使用建筑的配置信息
                // 此处假设GetBuildingProductionForResource返回的是每PRODUCTION_INTERVAL的产出
                totalProductionPerCycle += GetBuildingProductionForResource(building, type, workers);
            }
            
            // 将每PRODUCTION_INTERVAL的产出转换为每分钟的产出
            // (总产出 / 产出间隔秒数) * 60秒/分钟
            return (totalProductionPerCycle / PRODUCTION_INTERVAL) * 60f;
        }
        
        /// <summary>
        /// （辅助方法）获取单个建筑对特定资源的“每生产周期”产出量。
        /// </summary>
        private float GetBuildingProductionForResource(BuildingData building, ResourceType type, List<SurvivorData> workers)
        {
            // 此方法是GetResourceProductionRate的辅助，需要精确定义其返回单位
            // 假设这里返回的是基于PRODUCTION_INTERVAL的产出量
            var config = this.GetSystem<ConfigSystem>().GetBuildingConfig(building.ConfigId);
            if (config == null || config.Productions == null) return 0f;

            float buildingProductionForType = 0f;
            foreach (var prodInfo in config.Productions)
            {
                if (prodInfo.Type == type)
                {
                    if (prodInfo.RequiresWorker && (workers == null || workers.Count == 0)) continue;

                    float baseRateForCycle = prodInfo.BaseRate * (PRODUCTION_INTERVAL / 3600f); // 将小时速率转为周期速率
                    float workerBonusForCycle = CalculateWorkerBonus(workers, SurvivorAttributeType.Production); // (此方法也需确保返回周期加成)
                    // TODO: 科技加成也应考虑周期
                    buildingProductionForType += baseRateForCycle + workerBonusForCycle;
                }
            }
            return buildingProductionForType;
        }
        
        /// <summary>
        /// 获取指定资源类型的预计总消耗速率（单位资源/分钟）。
        /// </summary>
        /// <param name="type">要查询的资源类型。</param>
        /// <returns>该资源的预计消耗速率。</returns>
        public float GetResourceConsumptionRate(ResourceType type)
        {
            // 注意：与产出速率类似，需要将“每消耗周期”的量转换为“每分钟”
            float totalConsumptionPerCycle = 0f;
            
            switch (type)
            {
                case ResourceType.Food: // 食物消耗
                case ResourceType.Water: // 水消耗
                    // 人口消耗是基于CONSUMPTION_INTERVAL计算的
                    totalConsumptionPerCycle = mGameModel.Population.Value * 1; // 假设每人每周期消耗1单位
                    break;
                case ResourceType.Materials: // 材料消耗 (主要用于建筑维护)
                    var buildings = mBuildingSystem.GetAllBuildings();
                    totalConsumptionPerCycle = buildings.Count(b => b.State == BuildingState.Operational); // 假设每建筑每周期消耗1材料
                    break;
                // 其他资源的消耗逻辑可在此添加
            }
            
            // 将每CONSUMPTION_INTERVAL的消耗转换为每分钟的消耗
            return (totalConsumptionPerCycle / CONSUMPTION_INTERVAL) * 60f;
        }
        
        /// <summary>
        /// 立即触发一次资源产出更新。
        /// （注意：当前核心产出逻辑在EnhancedBuildingSystem，此方法可能主要用于调试或特殊事件）
        /// </summary>
        public void UpdateResourceProduction()
        {
            Debug.Log("[资源系统] 尝试手动更新资源产出...(注意：主要产出逻辑可能在EnhancedBuildingSystem)");
           // ProduceResources(); // 原计划调用此方法，但它已被注释
           // 实际可能需要调用 EnhancedBuildingSystem.UpdateBuildingProduction(0) 或类似方法，
           // 或者此方法本身在当前架构下意义不大，除非重新设计其职责。
        }
    }
    
    // --- 资源系统相关的事件定义 ---
    
    /// <summary>
    /// 资源产出事件。
    /// 当游戏中的资源通过建筑生产或其他方式增加时，可能会发送此事件。
    /// （注意：当前系统中，ResourceChangedEvent 更常用于通知资源量的具体变化）
    /// </summary>
    public struct ResourceProductionEvent
    {
        /// <summary>
        /// 资源产出事件发生的游戏内时间戳。
        /// </summary>
        public float ProductionTime;    // 产出时间戳
    }
    
    /// <summary>
    /// 资源消耗事件。
    /// 当游戏中的资源因人口需求、建筑维护等原因被消耗时，可能会发送此事件。
    /// （注意：当前系统中，ResourceChangedEvent 更常用于通知资源量的具体变化）
    /// </summary>
    public struct ResourceConsumptionEvent
    {
        /// <summary>
        /// 资源消耗事件发生的游戏内时间戳。
        /// </summary>
        public float ConsumptionTime;   // 消耗时间戳
    }
    
    /// <summary>
    /// 资源短缺事件。
    /// 当某种关键资源不足以满足当前的需求（如人口消耗或建筑维护）时发送。
    /// </summary>
    public struct ResourceShortageEvent
    {
        /// <summary>
        /// 发生短缺的资源类型。
        /// </summary>
        public ResourceType ResourceType;
        /// <summary>
        /// 当前需求该资源的总量。
        /// </summary>
        public int RequiredAmount;
        /// <summary>
        /// 当前库存中该资源的实际数量。
        /// </summary>
        public int CurrentAmount;
    }
} 