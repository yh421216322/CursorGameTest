// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：EnhancedBuildingSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了增强型建筑系统 (EnhancedBuildingSystem)，它扩展了基础建筑功能，
//     提供了更细致的建筑管理、建造流程控制、生产逻辑、工人分配、状态查询
//     以及用户界面反馈（如漂浮文字）等。该系统旨在模拟更复杂的建筑与运营策略。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI; // Unity UI 相关的命名空间
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{

    /// <summary>
    /// 增强建筑系统接口 (IEnhancedBuildingSystem)。
    /// 定义了游戏中所有与建筑相关的操作和行为的高级接口，
    /// 包括建造管理、建筑操作、生产管理、信息查询、建筑状态判断和系统级更新等。
    /// </summary>
    public interface IEnhancedBuildingSystem : QFISystem
    {
        // ===== 建造管理 (Construction Management) =====

        /// <summary>
        /// 检查是否可以在指定的世界坐标位置开始建造某种类型的建筑。
        /// 此检查通常会考虑资源是否充足、科技前置是否满足、目标位置是否被占用或地形是否允许等。
        /// </summary>
        /// <param name="position">要尝试建造的目标世界坐标。</param>
        /// <param name="buildingType">想要建造的建筑类型（通常对应一个配置ID）。</param>
        /// <returns>如果满足所有建造条件则返回true，否则返回false。</returns>
        bool CanBuildAt(Vector3 position, string buildingType);

        /// <summary>
        /// 在指定位置开始建造一个新的建筑。
        /// 此操作会扣除所需资源，并在游戏数据中创建一个新的建筑实例，初始状态为“建造中”。
        /// </summary>
        /// <param name="position">建筑的建造位置。</param>
        /// <param name="buildingType">要建造的建筑类型（配置ID）。</param>
        /// <returns>如果成功开始建造流程则返回true，否则（如资源不足、条件不满足）返回false。</returns>
        bool StartConstruction(Vector3 position, string buildingType);

        /// <summary>
        /// 标记指定ID的建筑已完成建造流程。
        /// 建筑状态将从“建造中”更新为“可运作”或其他合适状态。
        /// </summary>
        /// <param name="buildingId">已完成建造的建筑的唯一标识符。</param>
        void CompleteConstruction(string buildingId);

        /// <summary>
        /// 取消一个正在建造中的建筑项目。
        /// 通常会返还一部分已消耗的建造成本资源。
        /// </summary>
        /// <param name="buildingId">要取消建造的建筑的唯一标识符。</param>
        /// <returns>如果成功取消建造则返回true，否则（如建筑不存在或已建成）返回false。</returns>
        bool CancelConstruction(string buildingId);


        // ===== 建筑操作 (Building Operations) =====

        /// <summary>
        /// 升级指定ID的建筑到下一等级。
        /// 需要满足升级所需的资源、科技等条件。
        /// </summary>
        /// <param name="buildingId">要升级的建筑的唯一标识符。</param>
        /// <returns>如果成功开始或完成升级则返回true，否则返回false。</returns>
        bool UpgradeBuilding(string buildingId);

        /// <summary>
        /// 拆除指定ID的建筑。
        /// 建筑将被从游戏中移除，并可能返还一部分建造时消耗的材料。
        /// </summary>
        /// <param name="buildingId">要拆除的建筑的唯一标识符。</param>
        /// <returns>如果成功拆除则返回true，否则返回false。</returns>
        bool DemolishBuilding(string buildingId);

        /// <summary>
        /// 修复指定ID的受损建筑。
        /// 通常需要消耗一定的资源来恢复建筑的生命值。
        /// </summary>
        /// <param name="buildingId">要修复的建筑的唯一标识符。</param>
        /// <returns>如果成功开始或完成修复则返回true，否则返回false。</returns>
        bool RepairBuilding(string buildingId);

        /// <summary>
        /// 切换指定建筑的操作状态（例如，从“运作中”切换到“已暂停”，反之亦然）。
        /// 仅对已完全建成且未被摧毁的建筑有效。
        /// </summary>
        /// <param name="buildingId">要切换状态的建筑的唯一标识符。</param>
        /// <returns>如果成功切换状态则返回true，否则返回false。</returns>
        bool ToggleBuildingOperation(string buildingId);


        // ===== 生产管理 (Production Management) =====

        /// <summary>
        /// 更新所有建筑的资源生产逻辑。
        /// 此方法通常由游戏主循环定期调用，根据时间间隔（deltaTime）计算各建筑的产出。
        /// </summary>
        /// <param name="deltaTime">自上次更新以来经过的时间（秒）。</param>
        void UpdateBuildingProduction(float deltaTime);

        /// <summary>
        /// 立即触发一次指定建筑的资源收集动作。
        /// （此方法可能用于玩家手动点击收集，或特定事件触发的立即产出）
        /// </summary>
        /// <param name="buildingId">要收集资源的建筑的唯一标识符。</param>
        void CollectBuildingOutput(string buildingId);

        /// <summary>
        /// 为指定的建筑分配一名工人。
        /// 工人数量和属性通常会影响建筑的生产效率或其他运作参数。
        /// </summary>
        /// <param name="buildingId">要分配工人的建筑的唯一标识符。</param>
        /// <param name="workerId">要分配的工人的唯一标识符。</param>
        void AssignWorkersToBuilding(string buildingId, string workerId); // 参数修改为单个工人ID


        // ===== 信息查询 (Information Queries) =====

        /// <summary>
        /// 根据唯一标识符获取指定的建筑数据对象。
        /// </summary>
        /// <param name="buildingId">建筑的唯一标识符。</param>
        /// <returns>包含建筑详细信息的BuildingData对象；如果找不到，则返回null。</returns>
        BuildingData GetBuilding(string buildingId);

        /// <summary>
        /// 获取当前游戏中所有已存在建筑的数据列表。
        /// </summary>
        /// <returns>包含所有建筑BuildingData对象的列表。</returns>
        List<BuildingData> GetAllBuildings();

        /// <summary>
        /// 根据指定的建筑分类（如生产类、防御类、功能类等）获取该类别下的所有建筑列表。
        /// </summary>
        /// <param name="category">建筑分类枚举。</param>
        /// <returns>符合指定分类的建筑数据列表。</returns>
        List<BuildingData> GetBuildingsByCategory(BuildingCategory category);

        /// <summary>
        /// 获取以指定中心点和半径构成的圆形区域内存在的所有建筑。
        /// </summary>
        /// <param name="center">搜索区域的中心世界坐标。</param>
        /// <param name="radius">搜索区域的半径。</param>
        /// <returns>位于指定范围内的建筑数据列表。</returns>
        List<BuildingData> GetBuildingsInRadius(Vector3 center, float radius);

        /// <summary>
        /// 获取当前已建造的特定类型（配置ID）的建筑数量。
        /// </summary>
        /// <param name="buildingType">要查询的建筑类型（配置ID）。</param>
        /// <returns>该类型建筑的当前数量。</returns>
        int GetBuildingCount(string buildingType);

        /// <summary>
        /// 计算所有当前运作中的建筑对指定资源类型的总产出速率（例如，每小时产出量）。
        /// </summary>
        /// <param name="resourceType">要查询的资源类型。</param>
        /// <returns>该资源类型的总产出速率。</returns>
        float GetTotalProduction(ResourceType resourceType);


        // ===== 建筑状态 (Building Status) =====

        /// <summary>
        /// 检查指定ID的建筑当前是否处于可运作状态。
        /// （例如，已完成建造、未被摧毁、未暂停等）。
        /// </summary>
        /// <param name="buildingId">建筑的唯一标识符。</param>
        /// <returns>如果建筑可运作则返回true，否则返回false。</returns>
        bool IsBuildingOperational(string buildingId);

        /// <summary>
        /// 获取指定ID建筑的当前综合生产效率。
        /// 效率可能受多种因素影响，如建筑当前生命值、分配的工人数量及技能、已研究的科技加成等。
        /// </summary>
        /// <param name="buildingId">建筑的唯一标识符。</param>
        /// <returns>一个表示效率的浮点值（例如，1.0代表100%标准效率，更高或更低均有可能）。</returns>
        float GetBuildingEfficiency(string buildingId);

        /// <summary>
        /// （可选功能）为指定类型的建筑推荐一个合适的下一个建造位置。
        /// 可能基于网格系统、周围环境、资源点或其他策略进行计算。
        /// </summary>
        /// <param name="buildingType">要寻找位置的建筑类型（配置ID）。</param>
        /// <returns>推荐的建造位置世界坐标。</returns>
        Vector3 GetNextBuildPosition(string buildingType);


        // ===== 系统管理 (System Management) =====

        /// <summary>
        /// 系统的主更新方法，通常由游戏主循环每帧调用。
        /// 用于处理所有建筑相关的周期性逻辑，如建造进度更新、生产周期计时、维护检查等。
        /// </summary>
        void Update();
    }


    /// <summary>
    /// 增强建筑系统的具体实现类。
    /// 实现了 IEnhancedBuildingSystem 接口，管理游戏中所有建筑的生命周期和交互。
    /// </summary>
    public class EnhancedBuildingSystem : AbstractSystem, IEnhancedBuildingSystem
    {
        // --- 系统引用 (System References) ---
        /// <summary>
        /// 生存游戏核心数据模型的引用，用于访问和修改全局游戏状态如资源、建筑列表等。
        /// </summary>
        private ISurvivalGameModel gameModel;
        /// <summary>
        /// 资源系统的引用，用于处理资源的消耗和产出。
        /// </summary>
        private IResourceSystem resourceSystem;
        /// <summary>
        /// 高级科技系统的引用，用于检查科技前置和获取科技效果加成。
        /// </summary>
        private IAdvancedTechSystem techSystem;
        /// <summary>
        /// 配置系统的引用，用于获取建筑、资源等的配置信息。
        /// </summary>
        private ConfigSystem configSystem;
        /// <summary>
        /// 幸存者系统的引用，用于工人分配和属性查询。
        /// </summary>
        private ISurvivorSystem  survivorSystem;
        /// <summary>
        /// 通用游戏数据模型的引用 (可能与ISurvivalGameModel有重叠或特定用途)。
        /// </summary>
        private IGameModel mGameModel; // 此处mGameModel可能与gameModel用途不同或更通用，需根据实际情况区分
        
        // --- 建筑管理数据 (Building Management Data) ---
        /// <summary>
        /// 存储建筑ID与其对应的Unity场景中GameObject的映射。用于管理建筑的可视化实例。
        /// </summary>
        private Dictionary<string, GameObject> buildingObjects;
        /// <summary>
        /// 存储正在建造中的建筑ID与其当前建造进度的映射 (0.0 到 1.0)。
        /// </summary>
        private Dictionary<string, float> constructionProgress;
        /// <summary>
        /// 存储生产型建筑ID与其当前生产周期的计时器的映射。
        /// </summary>
        private Dictionary<string, float> productionTimers;
        // private Dictionary<string, int> assignedWorkers; // 此行被注释掉，工人分配信息现在可能直接存储在BuildingData或通过SurvivorSystem管理
        
        // --- 系统参数 (System Parameters) ---
        /// <summary>
        /// 游戏中建筑放置的网格大小。
        /// </summary>
        private const float GRID_SIZE = 2f; // 网格单位大小，例如2米
        /// <summary>
        /// 建筑建造进度更新的时间间隔（秒）。
        /// </summary>
        private const float CONSTRUCTION_UPDATE_INTERVAL = 0.5f; // 每0.5秒更新一次建造进度
        /// <summary>
        /// 建筑生产逻辑更新的时间间隔（秒）。
        /// </summary>
        private const float PRODUCTION_UPDATE_INTERVAL = 1f;     // 每1秒更新一次生产逻辑
        
        // --- 更新计时器 (Update Timers) ---
        /// <summary>
        /// 上一次建造进度更新的游戏时间戳。
        /// </summary>
        private float lastConstructionUpdate;
        /// <summary>
        /// 上一次生产逻辑更新的游戏时间戳。
        /// </summary>
        private float lastProductionUpdate;
        
        /// <summary>
        /// 系统初始化方法。
        /// 获取其他系统引用，初始化内部数据结构和计时器。
        /// </summary>
        protected override void OnInit()
        {
            // 获取其他所需系统的引用
            gameModel = this.GetModel<ISurvivalGameModel>();
            resourceSystem = this.GetSystem<IResourceSystem>();
            techSystem = this.GetSystem<IAdvancedTechSystem>();
            configSystem = this.GetSystem<ConfigSystem>();
            survivorSystem  = this.GetSystem<ISurvivorSystem>();
            mGameModel = this.GetModel<IGameModel>(); // 注意: 此处获取了IGameModel，需要确认其与ISurvivalGameModel的关系和用途
            
            // 初始化用于管理建筑状态的容器
            buildingObjects = new Dictionary<string, GameObject>();     // 建筑ID -> Unity场景对象
            constructionProgress = new Dictionary<string, float>();      // 建筑ID -> 建造进度 (0-1)
            productionTimers = new Dictionary<string, float>();          // 建筑ID -> 生产计时器
            
            // assignedWorkers = new Dictionary<string, int>(); // 此行被注释，工人信息可能由mGameModel.SurvivorData.buildingWorkers管理

            // 初始化更新计时器的时间戳
            lastConstructionUpdate = Time.time;
            lastProductionUpdate = Time.time;
            
            Debug.Log("[增强建筑系统] 初始化完成。");
            
            // 注册游戏更新事件 (当前被注释掉，Update方法由外部驱动)
            // this.RegisterEvent<GameUpdateEvent>(OnGameUpdate);
        }

        #region 建造管理 (Construction Management)
        
        /// <summary>
        /// 检查是否可以在指定位置建造指定类型的建筑。
        /// 会验证配置是否存在、科技是否满足、位置是否可用、资源是否足够以及是否达到数量上限。
        /// </summary>
        /// <param name="position">要建造的候选位置。</param>
        /// <param name="buildingType">要建造的建筑类型ID。</param>
        /// <returns>如果可以建造则返回true，否则返回false并打印警告信息。</returns>
        public bool CanBuildAt(Vector3 position, string buildingType)
        {
            // 1. 检查建筑配置是否存在
            var config = configSystem.GetBuildingConfig(buildingType);
            if (config == null)
            {
                Debug.LogWarning($"[增强建筑系统] 检查建造条件失败：未找到建筑配置 {buildingType}。");
                return false;
            }
            
            // 2. 检查科技需求是否满足
            if (config.RequiredTechs != null && config.RequiredTechs.Count > 0 && !AreTechRequirementsMet(config.RequiredTechs))
            {
                Debug.LogWarning($"[增强建筑系统] 建造 {buildingType} 的科技需求未满足。");
                return false;
            }
            
            // 3. 检查目标位置及其周围区域是否已被占用 (基于建筑大小)
            Vector2Int buildingSize = config.Size; // 从配置获取建筑占地大小
            if (IsAreaOccupied(position, buildingSize))
            {
                Debug.LogWarning($"[增强建筑系统] 目标位置 {position} (尺寸: {buildingSize}) 已被占用或无效，无法建造 {buildingType}。");
                return false;
            }
            
            // 4. 检查所需资源是否足够
            if (config.BuildCosts != null)
            {
                foreach (var cost in config.BuildCosts)
                {
                    if (gameModel.GetResourceAmount(cost.Type) < cost.Amount) // gameModel应提供GetResourceAmount方法
                    {
                        Debug.LogWarning($"[增强建筑系统] 建造 {buildingType} 资源不足：需要 {cost.Amount} 单位的 {cost.Type}。");
                    return false;
                    }
                }
            }
            
            // 5. 检查此类建筑是否已达到最大建造数量限制
            if (GetBuildingCount(buildingType) >= GetMaxBuildingCount(buildingType))
            {
                Debug.LogWarning($"[增强建筑系统] 建筑类型 {buildingType} 已达到最大建造数量上限。");
                return false;
            }
            
            return true; // 所有检查通过，可以建造
        }
        
        /// <summary>
        /// 开始在指定位置建造一个指定类型的建筑。
        /// </summary>
        /// <param name="position">建筑的建造位置（会自动对齐到网格）。</param>
        /// <param name="buildingType">要建造的建筑的配置ID。</param>
        /// <returns>如果成功启动建造流程则返回true，否则返回false。</returns>
        public bool StartConstruction(Vector3 position, string buildingType)
        {
            // 首先检查是否满足所有建造条件
            if (!CanBuildAt(position, buildingType))
            {
                Debug.LogWarning($"[增强建筑系统] 无法在 {position} 开始建造 {buildingType}：不满足建造条件。");
                return false;
            }

            var config = configSystem.GetBuildingConfig(buildingType); // 再次获取配置（或从CanBuildAt传递）

            // 扣除建造所需资源
            if (config.BuildCosts != null)
            {
                foreach (var cost in config.BuildCosts)
                {
                    gameModel.ConsumeResource(cost.Type, cost.Amount); // gameModel应提供ConsumeResource方法
                }
            }

            // 创建建筑数据实例
            var buildingData = new BuildingData
            {
                Id = System.Guid.NewGuid().ToString(),     // 生成全局唯一ID
                ConfigId = buildingType,                   // 建筑类型ID
                Position = SnapToGrid(position),           // 位置对齐到网格
                State = BuildingState.UnderConstruction,   // 初始状态为建造中
                Health = config.MaxHealth * 0.1f,          // 初始少量生命值 (例如10%或配置值)
                MaxHealth = config.MaxHealth,              // 最大生命值
                BuildProgress = 0f                         // 建造进度从0开始
            };

            gameModel.AddBuilding(buildingData); // 将建筑数据添加到全局游戏模型中
            CreateBuildingGameObject(buildingData); // 创建建筑在场景中的可视化对象

            // 初始化系统内部对此建筑的状态追踪
            constructionProgress[buildingData.Id] = 0f;  // 记录建造进度
            productionTimers[buildingData.Id] = 0f;      // 初始化生产计时器
            // assignedWorkers[buildingData.Id] = 0;      // 初始化分配工人数量 (如果不由BuildingData管理)
            if (!mGameModel.SurvivorData.buildingWorkers.ContainsKey(buildingData.Id)) // 确保工人分配列表存在
            {
                mGameModel.SurvivorData.buildingWorkers[buildingData.Id] = new List<string>();
            }


            Debug.Log($"[增强建筑系统] 开始在 {buildingData.Position} 建造 {config.Name} (ID: {buildingData.Id})。");
            // TODO: 发送建筑开始建造的事件 (EventSystem.Send)
            return true;
        }

        
        /// <summary>
        /// 标记指定ID的建筑已完成建造。
        /// 将建筑状态更新为可运作，生命值恢复满，并清理相关的建造进度数据。
        /// </summary>
        /// <param name="buildingId">已完成建造的建筑的ID。</param>
        public void CompleteConstruction(string buildingId)
        {
            if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 使用TryGetValue确保建筑存在
            {
                Debug.LogWarning($"[增强建筑系统] 尝试完成不存在的建筑 (ID: {buildingId}) 的建造。");
                return;
            }
            
            var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
            if (config == null)
            {
                Debug.LogError($"[增强建筑系统] 找不到建筑 {building.ConfigId} 的配置，无法完成建造。");
                return;
            }
            
            // 更新建筑数据状态
            building.State = BuildingState.Operational; // 设置为可运作状态
            building.Health = building.MaxHealth;       // 完全恢复生命值
            building.BuildProgress = 1f;                // 建造进度设置为100%
            
            // 如果之前没有为该建筑创建工人列表，则现在创建 (虽然StartConstruction已处理，作为安全校验)
            if (!mGameModel.SurvivorData.buildingWorkers.ContainsKey(buildingId))
            {
                mGameModel.SurvivorData.buildingWorkers.Add(buildingId, new List<string>());
            }
            
            constructionProgress.Remove(buildingId); // 从建造进度字典中移除该建筑
            UpdateBuildingVisual(buildingId);        // 更新建筑的视觉表现（例如，移除脚手架，显示完整模型）
            
            Debug.Log($"[增强建筑系统] 建筑 {config.Name} (ID: {buildingId}) 建造完成。");
            // TODO: 发送建筑建造完成事件 (EventSystem.Send)
        }
        
        /// <summary>
        /// 取消指定ID的正在建造中的建筑项目。
        /// 会返还部分已消耗的资源，并移除建筑数据和场景对象。
        /// </summary>
        /// <param name="buildingId">要取消建造的建筑ID。</param>
        /// <returns>如果成功取消则返回true，否则false。</returns>
        public bool CancelConstruction(string buildingId)
        {
            // 检查该建筑是否确实在建造中 (通过constructionProgress字典判断)
            if (!constructionProgress.ContainsKey(buildingId))
            {
                Debug.LogWarning($"[增强建筑系统] 尝试取消的建筑 (ID: {buildingId}) 不在建造队列中或已完成。");
                return false;
            }
            
            if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 获取建筑数据
            {
                 Debug.LogError($"[增强建筑系统] 取消建造失败：在GameModel中找不到建筑ID {buildingId}。");
                constructionProgress.Remove(buildingId); // 即使数据不一致，也尝试清理进度
                return false;
            }

            var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
            if (config == null)
            {
                Debug.LogError($"[增强建筑系统] 取消建造失败：找不到建筑 {building.ConfigId} 的配置。");
                // 依然尝试清理其他数据
            }
            
            // 返还部分建造成本材料 (例如，70%)
            if (config != null && config.BuildCosts != null)
            {
                foreach (var cost in config.BuildCosts)
                {
                    int refundAmount = Mathf.RoundToInt(cost.Amount * 0.7f); // 计算返还数量
                    if (refundAmount > 0) gameModel.AddResource(cost.Type, refundAmount); // 添加返还的资源
                }
            }
            
            // 清理系统内部追踪数据
            constructionProgress.Remove(buildingId);
            productionTimers.Remove(buildingId); // 如果已初始化也一并移除
            mGameModel.SurvivorData.buildingWorkers.Remove(buildingId); // 移除工人分配槽
            
            // 销毁场景中的建筑GameObject
            if (buildingObjects.TryGetValue(buildingId, out var buildingGO))
            {
                UnityEngine.Object.Destroy(buildingGO); // 销毁GameObject
                buildingObjects.Remove(buildingId);    // 从字典中移除
            }
            
            gameModel.RemoveBuilding(buildingId); // 从全局游戏模型中移除建筑数据
            
            Debug.Log($"[增强建筑系统] 已取消建造 {(config != null ? config.Name : "未知建筑")} (ID: {buildingId})，并返还部分资源。");
            // TODO: 发送建筑取消建造事件
            return true;
        }
        
        #endregion

        #region 建筑操作 (Building Operations)
        
        /// <summary>
        /// 尝试升级指定ID的建筑。
        /// </summary>
        /// <param name="buildingId">要升级的建筑ID。</param>
        /// <returns>如果成功开始或完成升级则返回true，否则false。</returns>
        public bool UpgradeBuilding(string buildingId)
        {
            if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 检查建筑是否存在
            {
                Debug.LogWarning($"[增强建筑系统] 尝试升级失败：找不到建筑ID {buildingId}。");
                return false;
            }
            
            var currentConfig = configSystem.GetBuildingConfig(building.ConfigId); // 获取当前配置
            if (currentConfig == null)
            {
                Debug.LogError($"[增强建筑系统] 找不到当前建筑 {building.ConfigId} 的配置，无法升级。");
                return false;
            }

            // 查找此建筑的升级配置路径
            var upgradePath = FindUpgradeConfig(building.ConfigId); // (需要实现此辅助方法)
            if (upgradePath == null)
            {
                Debug.LogWarning($"[增强建筑系统] 建筑 {currentConfig.Name} (ID: {buildingId}) 没有可用的升级路径或已是最高级。");
                return false;
            }
            
            var targetConfig = configSystem.GetBuildingConfig(upgradePath.ToConfigId); // 获取目标升级配置
            if (targetConfig == null)
            {
                 Debug.LogError($"[增强建筑系统] 找不到目标升级配置ID: {upgradePath.ToConfigId}。");
                return false;
            }

            // 检查升级所需的科技前置条件
            if (upgradePath.RequiredTechs != null && !AreTechRequirementsMet(upgradePath.RequiredTechs))
            {
                 Debug.LogWarning($"[增强建筑系统] 升级到 {targetConfig.Name} 的科技需求未满足。");
                return false;
            }

            // 检查升级所需的资源成本
            if (upgradePath.UpgradeCosts != null)
            {
                foreach (var cost in upgradePath.UpgradeCosts)
                {
                    if (gameModel.GetResourceAmount(cost.Type) < cost.Amount)
                    {
                        Debug.LogWarning($"[增强建筑系统] 升级到 {targetConfig.Name} 资源不足：需要 {cost.Amount} 单位的 {cost.Type}。");
                    return false;
                    }
                }
                // 消耗升级资源
                foreach (var cost in upgradePath.UpgradeCosts)
                {
                    gameModel.ConsumeResource(cost.Type, cost.Amount);
                }
            }
            
            // 更新建筑数据以反映升级 (直接完成或进入升级中状态)
            // 方案1: 直接完成升级 (如果升级是即时的)
            building.ConfigId = upgradePath.ToConfigId;    // 更新配置ID为升级后的ID
            building.MaxHealth = targetConfig.MaxHealth;   // 更新最大生命值
            building.Health = targetConfig.MaxHealth;      // 升级后血量补满 (或按比例)
            building.State = BuildingState.Operational;  // 保持或设置为可运作
            // TODO: 可能需要更新其他属性，如防御力、存储容量等，基于targetConfig

            // 方案2: 进入“升级中”状态 (如果升级需要时间)
            // building.State = BuildingState.Upgrading;
            // building.TargetUpgradeConfigId = upgradePath.ToConfigId; // 存储目标配置
            // constructionProgress[buildingId] = 0f; // 开始升级进度
            // _buildingQueue.Add(buildingId); // 加入更新队列

            UpdateBuildingVisual(buildingId); // 更新建筑的视觉表现
            
            Debug.Log($"[增强建筑系统] 建筑 {currentConfig.Name} (ID: {buildingId}) 已成功升级到 {targetConfig.Name}。");
            // TODO: 发送建筑升级完成/开始事件
            return true;
        }
        
        /// <summary>
        /// 拆除指定ID的建筑，返还部分资源。
        /// </summary>
        /// <param name="buildingId">要拆除的建筑ID。</param>
        /// <returns>成功拆除返回true，否则false。</returns>
        public bool DemolishBuilding(string buildingId)
        {
            if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 检查建筑是否存在
            {
                Debug.LogWarning($"[增强建筑系统] 尝试拆除失败：找不到建筑ID {buildingId}。");
                return false;
            }
            
            var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
            if (config == null)
            {
                 Debug.LogError($"[增强建筑系统] 拆除失败：找不到建筑 {building.ConfigId} 的配置。");
                // 即使配置丢失，也尝试清理数据
            }
            
            // 返还部分建造成本材料 (例如，配置中定义或固定比例，如30%)
            if (config != null && config.BuildCosts != null)
            {
                foreach (var cost in config.BuildCosts)
                {
                    int refundAmount = Mathf.FloorToInt(cost.Amount * 0.3f); // 示例：返还30%
                    if (refundAmount > 0) gameModel.AddResource(cost.Type, refundAmount);
                }
            }
            
            // 清理所有与此建筑相关的系统内部数据
            constructionProgress.Remove(buildingId); // 如果在建造中，移除进度
            productionTimers.Remove(buildingId);     // 移除生产计时器
            if (mGameModel.SurvivorData.buildingWorkers.ContainsKey(buildingId)) // 遣散工人
            {
                List<string> workers = new List<string>(mGameModel.SurvivorData.buildingWorkers[buildingId]);
                foreach(string workerId in workers)
                {
                    survivorSystem.UnassignSurvivorFromBuilding(workerId);
                }
                mGameModel.SurvivorData.buildingWorkers.Remove(buildingId);
            }
            
            // 销毁场景中的建筑GameObject
            if (buildingObjects.TryGetValue(buildingId, out var buildingGO))
            {
                UnityEngine.Object.Destroy(buildingGO);
                buildingObjects.Remove(buildingId);
            }
            
            gameModel.RemoveBuilding(buildingId); // 从全局游戏模型中移除建筑数据
            
            Debug.Log($"[增强建筑系统] 已拆除建筑 {(config != null ? config.Name : "未知类型")} (ID: {buildingId})。");
            // TODO: 发送建筑拆除事件
            return true;
        }
        
        /// <summary>
        /// 修复指定ID的受损建筑。
        /// </summary>
        /// <param name="buildingId">要修复的建筑ID。</param>
        /// <returns>成功修复或开始修复返回true，否则false。</returns>
        public bool RepairBuilding(string buildingId)
        {
            if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 检查建筑是否存在
            {
                Debug.LogWarning($"[增强建筑系统] 尝试修复失败：找不到建筑ID {buildingId}。");
                return false;
            }
            
            if (building.Health >= building.MaxHealth) // 如果建筑已经是满血状态
            {
                Debug.Log($"[增强建筑系统] 建筑 {building.ConfigId} (ID: {buildingId}) 无需修复，已是满血。");
                return false; // 不需要修复
            }
            
            var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
            if (config == null)
            {
                Debug.LogError($"[增强建筑系统] 修复失败：找不到建筑 {building.ConfigId} 的配置。");
                return false;
            }
            
            // 计算修复所需的资源成本 (例如，按损坏百分比和原建造成本的一定比例计算)
            float damagePercentage = (building.MaxHealth - building.Health) / building.MaxHealth; // 损坏百分比
            var repairCosts = new List<ResourceCost>();
            
            if (config.BuildCosts != null)
            {
                foreach (var buildCost in config.BuildCosts)
                {
                    // 示例：修复成本为损坏部分对应原成本的50%
                    int repairAmount = Mathf.CeilToInt(buildCost.Amount * damagePercentage * 0.5f);
                    if (repairAmount > 0)
                    {
                        repairCosts.Add(new ResourceCost { Type = buildCost.Type, Amount = repairAmount });
                    }
                }
            }

            // 检查是否有足够资源进行修复
            if (repairCosts.Any()) // 如果有修复成本
            {
                foreach (var cost in repairCosts)
                {
                    if (gameModel.GetResourceAmount(cost.Type) < cost.Amount)
                    {
                        Debug.LogWarning($"[增强建筑系统] 修复建筑 {config.Name} 资源不足：需要 {cost.Amount} 单位的 {cost.Type}。");
                    return false; // 资源不足，无法修复
                    }
                }
                // 消耗修复资源
                foreach (var cost in repairCosts)
                {
                    gameModel.ConsumeResource(cost.Type, cost.Amount);
                }
            }
            
            // 执行修复：恢复建筑生命值并更新状态
            building.Health = building.MaxHealth; // 直接恢复满血 (或可设计为逐渐修复)
            if (building.State == BuildingState.Damaged || building.State == BuildingState.Destroyed) // 如果之前是损坏或被摧毁状态
            {
                 building.State = BuildingState.Operational; // 修复后变为可运作
            }
            
            UpdateBuildingVisual(buildingId); // 更新建筑视觉（例如移除损坏效果）
            
            Debug.Log($"[增强建筑系统] 建筑 {config.Name} (ID: {buildingId}) 已修复。");
            // TODO: 发送建筑修复事件
            return true;
        }
        
        /// <summary>
        /// 切换指定建筑的操作状态（启用/停用）。
        /// </summary>
        /// <param name="buildingId">建筑ID。</param>
        /// <returns>成功切换返回true，否则false。</returns>
        public bool ToggleBuildingOperation(string buildingId)
        {
            if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 检查建筑是否存在
            {
                Debug.LogWarning($"[增强建筑系统] 切换操作状态失败：找不到建筑ID {buildingId}。");
                return false;
            }
            
            // 只有在特定状态下（例如，已建成且未被摧毁）才能切换操作状态
            if (building.State == BuildingState.UnderConstruction ||
                building.State == BuildingState.Upgrading ||
                building.State == BuildingState.Destroyed)
            {
                Debug.LogWarning($"[增强建筑系统] 建筑 {building.ConfigId} (ID: {buildingId}) 当前状态 ({building.State}) 不允许切换操作。");
                return false;
            }

            // 切换状态：如果是运作中，则变为暂停（或损坏，取决于设计）；如果是暂停/损坏，则变为运作中
            // 此处简化为 Operational 和 Damaged (作为非运作的代表) 之间的切换
            building.State = (building.State == BuildingState.Operational) ? BuildingState.Damaged : BuildingState.Operational;
            // 注意：Damaged状态通常因维护不足或攻击造成，此处用作“非运作”的示例。更合适的状态可能是 "Paused" 或 "Inactive"。
            
            UpdateBuildingVisual(buildingId); // 更新视觉表现
            
            Debug.Log($"[增强建筑系统] 建筑 {building.ConfigId} (ID: {buildingId}) 操作状态已切换为: {building.State}。");
            // TODO: 发送建筑状态变更事件
            return true;
        }
        
        #endregion

        #region 生产管理 (Production Management)
        
        /// <summary>
        /// 更新所有建筑的生产逻辑。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间。</param>
        public void UpdateBuildingProduction(float deltaTime)
        {
            foreach (var building in gameModel.Buildings.Values) // 遍历所有建筑
            {
                // 只处理处于可运作状态且不在建造/升级中的建筑
                if (building.State != BuildingState.Operational || constructionProgress.ContainsKey(building.Id))
                    continue;
                
                UpdateSingleBuildingProduction(building, deltaTime); // 调用单个建筑的生产更新逻辑
            }
        }
        
        /// <summary>
        /// 更新单个建筑的生产计时和产出。
        /// </summary>
        /// <param name="building">要更新的建筑数据。</param>
        /// <param name="deltaTime">帧间隔时间。</param>
        private void UpdateSingleBuildingProduction(BuildingData building, float deltaTime)
        {
            var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
            // 如果建筑没有生产配置或当前不可运作，则不进行生产
            if (config == null || config.Productions == null || config.Productions.Count == 0)
                return;
            
            // 初始化或获取该建筑的生产计时器
            if (!productionTimers.ContainsKey(building.Id))
                productionTimers[building.Id] = 0f;
            
            productionTimers[building.Id] += deltaTime; //累加生产时间
            
            // 检查是否到达一个生产周期 (示例：每3个游戏单位时间，假设1单位时间=1秒，即3秒产出一次)
            // 实际游戏的生产周期应从配置读取或更复杂计算
            float productionCycleDuration = 3f; // 假设的生产周期（秒）
            if (productionTimers[building.Id] >= productionCycleDuration)
            {
                ProduceBuildingOutput(building); // 执行产出逻辑
                productionTimers[building.Id] -= productionCycleDuration; // 重置或减去一个周期的时间
            }
        }
        
        /// <summary>
        /// 根据建筑配置和当前状态（如效率、工人）实际产出资源。
        /// </summary>
        /// <param name="building">要执行产出的建筑数据。</param>
        private void ProduceBuildingOutput(BuildingData building)
        {
            var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
            if (config == null || config.Productions == null) return;

            float currentEfficiency = GetBuildingEfficiency(building.Id); // 获取建筑当前综合效率

            foreach (var productionInfo in config.Productions) // 遍历建筑的所有生产条目
            {
                // 检查是否需要工人以及是否有工人分配
                List<string> workersInThisBuilding = mGameModel.SurvivorData.buildingWorkers.TryGetValue(building.Id, out var lst) ? lst : new List<string>();
                if (productionInfo.RequiresWorker && workersInThisBuilding.Count == 0)
                    continue; // 需要工人但没有工人，则不产出此项

                // 检查生产所需的输入资源是否足够
                bool canAffordInputs = true;
                if (productionInfo.InputCosts != null)
                {
                    foreach (var inputCost in productionInfo.InputCosts)
                    {
                        if (gameModel.GetResourceAmount(inputCost.Type) < inputCost.Amount)
                        {
                            canProduce = false; // 原变量名canProduce在此作用域未定义，应为canAffordInputs
                            break;
                        }
                    }
                }

                if (!canAffordInputs) continue; // 输入资源不足，不产出此项

                // 消耗输入资源
                if (productionInfo.InputCosts != null)
                {
                    foreach (var inputCost in productionInfo.InputCosts)
                    {
                        gameModel.ConsumeResource(inputCost.Type, inputCost.Amount);
                    }
                }
                
                // 计算受工人数量影响的产出速率
                float rateWithWorkerBonus = productionInfo.BaseRate + (productionInfo.WorkerBonus * workersInThisBuilding.Count);
                
                // 计算最终实际产出量 (受综合效率影响)
                // 假设BaseRate是每生产周期（例如上面定义的productionCycleDuration）的产出量
                int actualOutputAmount = Mathf.RoundToInt(rateWithWorkerBonus * currentEfficiency);

                if (actualOutputAmount > 0)
                {
                    gameModel.AddResource(productionInfo.Type, actualOutputAmount); // 将产出资源添加到全局库存
                    ShowFloatingText(building.Position, $"+{actualOutputAmount}", GetResourceColor(productionInfo.Type)); // 显示漂浮文字反馈
                    Debug.Log($"[增强建筑系统] 建筑 {config.Name} (ID: {building.Id}) 产出了 {actualOutputAmount} 单位的 {productionInfo.Type}。");
                }
            }
        }
        
        /// <summary>
        /// 立即触发指定建筑的资源收集（产出）。
        /// </summary>
        /// <param name="buildingId">建筑ID。</param>
        public void CollectBuildingOutput(string buildingId)
        {
            if (gameModel.Buildings.TryGetValue(buildingId, out var building)) // 使用TryGetValue更安全
            {
                ProduceBuildingOutput(building); // 调用核心产出逻辑
                productionTimers[buildingId] = 0f; // 手动收集后重置生产计时器 (可选设计)
            }
            else
            {
                Debug.LogWarning($"[增强建筑系统] 尝试收集不存在的建筑 (ID: {buildingId}) 的产出。");
            }
        }
        
        /// <summary>
        /// 为指定建筑分配一名工人。
        /// </summary>
        /// <param name="buildingId">建筑ID。</param>
        /// <param name="workerId">要分配的工人ID。</param>
        public void AssignWorkersToBuilding(string buildingId, string workerId) // 修改参数以反映单个工人分配
        {
            if (!gameModel.Buildings.ContainsKey(buildingId)) // 检查建筑是否存在
            {
                Debug.LogWarning($"[增强建筑系统] 分配工人失败：找不到建筑ID {buildingId}。");
                return;
            }
            
            var building = gameModel.Buildings[buildingId];
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            if (config == null) {
                Debug.LogError($"[增强建筑系统] 找不到建筑 {building.ConfigId} 的配置，无法分配工人。");
                return;
            }

            // 确保建筑的工人列表已初始化
            if (!mGameModel.SurvivorData.buildingWorkers.ContainsKey(buildingId))
            {
                mGameModel.SurvivorData.buildingWorkers[buildingId] = new List<string>();
            }

            // 检查是否超过最大工人数限制
            if (mGameModel.SurvivorData.buildingWorkers[buildingId].Count >= config.MaxWorkers)
            {
                Debug.LogWarning($"[增强建筑系统] 建筑 {config.Name} (ID: {buildingId}) 已达到最大工人数量 ({config.MaxWorkers})。");
                return;
            }

            // 检查工人是否已被分配到其他地方或是否有效 (这部分逻辑应在SurvivorSystem中处理更佳)
            // if (!survivorSystem.IsSurvivorAvailable(workerId)) { return; }


            mGameModel.SurvivorData.buildingWorkers[buildingId].Add(workerId); // 添加工人
            // survivorSystem.SetSurvivorAssignment(workerId, buildingId); // 通知幸存者系统更新工人状态
            
            Debug.Log($"[增强建筑系统] 已为建筑 {config.Name} (ID: {buildingId}) 分配工人 (ID: {workerId})。当前工人数量: {mGameModel.SurvivorData.buildingWorkers[buildingId].Count}");
            // TODO: 发送工人分配事件
        }
        
        #endregion

        #region 信息查询 (Information Queries)
        
        /// <summary>
        /// 根据ID获取建筑数据。
        /// </summary>
        /// <param name="buildingId">建筑的唯一ID。</param>
        /// <returns>BuildingData对象，如果不存在则返回null。</returns>
        public BuildingData GetBuilding(string buildingId)
        {
            return gameModel.Buildings.TryGetValue(buildingId, out var building) ? building : null;
        }
        
        /// <summary>
        /// 获取所有已建造的建筑列表。
        /// </summary>
        /// <returns>包含所有BuildingData的列表。</returns>
        public List<BuildingData> GetAllBuildings()
        {
            return gameModel.Buildings.Values.ToList(); // 返回字典中所有值的列表副本
        }
        
        /// <summary>
        /// 根据建筑类别筛选建筑。
        /// </summary>
        /// <param name="category">要筛选的建筑类别。</param>
        /// <returns>属于该类别的建筑列表。</returns>
        public List<BuildingData> GetBuildingsByCategory(BuildingCategory category)
        {
            return gameModel.Buildings.Values
                .Where(b => {
                    var config = configSystem.GetBuildingConfig(b.ConfigId); // 获取配置
                    return config != null && config.Category == category; // 检查配置和类别
                })
                .ToList();
        }
        
        /// <summary>
        /// 获取指定中心点和半径范围内的所有建筑。
        /// </summary>
        /// <param name="center">搜索中心点。</param>
        /// <param name="radius">搜索半径。</param>
        /// <returns>在范围内的建筑列表。</returns>
        public List<BuildingData> GetBuildingsInRadius(Vector3 center, float radius)
        {
            float sqrRadius = radius * radius; // 使用平方距离以避免开方运算
            return gameModel.Buildings.Values
                .Where(b => (b.Position - center).sqrMagnitude <= sqrRadius) // 比较平方距离
                .ToList();
        }
        
        /// <summary>
        /// 获取特定类型建筑的当前已建造数量。
        /// </summary>
        /// <param name="buildingType">建筑类型ID (ConfigId)。</param>
        /// <returns>该类型建筑的数量。</returns>
        public int GetBuildingCount(string buildingType)
        {
            return gameModel.Buildings.Values.Count(b => b.ConfigId == buildingType);
        }
        
        /// <summary>
        /// 计算指定资源类型的总生产速率（每小时）。
        /// </summary>
        /// <param name="resourceType">要查询的资源类型。</param>
        /// <returns>该资源的总生产速率。</returns>
        public float GetTotalProduction(ResourceType resourceType)
        {
            float totalProductionRate = 0f;
            
            foreach (var building in gameModel.Buildings.Values) // 遍历所有建筑
            {
                if (building.State != BuildingState.Operational) continue; // 只考虑运作中的建筑
                
                var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取配置
                if (config == null || config.Productions == null) continue; // 无配置或无生产条目则跳过
                
                float efficiency = GetBuildingEfficiency(building.Id); // 获取建筑效率
                List<string> workersInThisBuilding = mGameModel.SurvivorData.buildingWorkers.TryGetValue(building.Id, out var lst) ? lst : new List<string>();
                int workerCount = workersInThisBuilding.Count; // 获取分配的工人数
                
                foreach (var productionInfo in config.Productions) // 遍历生产条目
                {
                    if (productionInfo.Type == resourceType) // 如果是目标资源类型
                    {
                        // 计算此建筑对此资源的生产速率 (基础速率 + 工人加成) * 效率
                        float buildingRate = (productionInfo.BaseRate + (productionInfo.WorkerBonus * workerCount)) * efficiency;
                        totalProductionRate += buildingRate; // 累加到总速率
                    }
                }
            }
            
            return totalProductionRate; // 返回总生产速率
        }
        
        #endregion

        #region 建筑状态 (Building Status)
        
        /// <summary>
        /// 检查指定建筑是否处于可运作状态。
        /// </summary>
        /// <param name="buildingId">建筑ID。</param>
        /// <returns>如果建筑可运作则返回true，否则false。</returns>
        public bool IsBuildingOperational(string buildingId)
        {
            if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 检查建筑是否存在
                return false;
            
            // 可运作条件：状态为Operational，并且不在建造进度中 (constructionProgress中没有记录代表已完成)
            return building.State == BuildingState.Operational && !constructionProgress.ContainsKey(buildingId);
        }
        
        /// <summary>
        /// 获取指定建筑的当前综合效率。
        /// </summary>
        /// <param name="buildingId">建筑ID。</param>
        /// <returns>效率值 (0.0f - 1.0f+)，如果建筑不存在则返回0。</returns>
        public float GetBuildingEfficiency(string buildingId)
        {
            if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 检查建筑是否存在
            {
                Debug.LogWarning($"[增强建筑系统] 查询效率失败：找不到建筑ID {buildingId}。");
                return 0f;
            }
            
            // 1. 基础效率 (受建筑当前生命值影响)
            float healthEfficiency = (building.MaxHealth > 0) ? (building.Health / building.MaxHealth) : 0f;
            
            // 2. 工人效率 (受分配工人数量和工人技能影响 - 此处简化为仅数量)
            var config = configSystem.GetBuildingConfig(building.ConfigId);
            if (config == null) return healthEfficiency; // 配置不存在则只考虑健康效率

            List<string> workersInThisBuilding = mGameModel.SurvivorData.buildingWorkers.TryGetValue(building.Id, out var lst) ? lst : new List<string>();
            int assignedWorkerCount = workersInThisBuilding.Count;
            int maxWorkerSlots = config.MaxWorkers > 0 ? config.MaxWorkers : 1; // 防止除以零，至少1个槽位
            float workerFactor = (float)assignedWorkerCount / maxWorkerSlots; // 工人填充比例带来的效率因子
            // TODO: 可以进一步结合工人的特定技能或属性来调整workerFactor

            // 3. 科技加成 (从科技系统获取对此类建筑或特定生产的效率加成)
            float techBonusFactor = 1f + GetTechProductionBonus(building.ConfigId); // 1f基础 + 百分比加成
            
            // 综合效率 = 健康效率 * 工人因子 * 科技因子
            return Mathf.Clamp01(healthEfficiency) * Mathf.Clamp01(workerFactor) * techBonusFactor;
        }
        
        /// <summary>
        /// 获取指定类型建筑的下一个建议建造位置（简单示例逻辑）。
        /// </summary>
        /// <param name="buildingType">建筑类型ID。</param>
        /// <returns>建议的建造位置。如果找不到则返回Vector3.zero。</returns>
        public Vector3 GetNextBuildPosition(string buildingType)
        {
            Vector3 basePosition = Vector3.zero; // 示例：从原点开始搜索
            
            // 简单地向外扩展搜索可用位置 (非常基础的示例)
            for (int radius = 1; radius <= 10; radius++) // 搜索半径
            {
                for (int xOffset = -radius; xOffset <= radius; xOffset++)
                {
                    for (int yOffset = -radius; yOffset <= radius; yOffset++)
                    {
                        // 只检查当前半径的外环，避免重复检查内部点
                        if (Mathf.Abs(xOffset) != radius && Mathf.Abs(yOffset) != radius) continue;

                        Vector3 testPosition = basePosition + new Vector3(xOffset * GRID_SIZE, yOffset * GRID_SIZE, 0);
                        if (CanBuildAt(testPosition, buildingType)) // 如果此位置可用
                        {
                            return testPosition; // 返回找到的第一个可用位置
                        }
                    }
                }
            }
            
            Debug.LogWarning($"[增强建筑系统] 未能为 {buildingType} 找到合适的建造位置。");
            return basePosition; // 如果找不到，则返回基础位置或一个标记无效的值
        }

        /// <summary>
        /// 系统的主更新循环。
        /// </summary>
        public void Update()
        {
            float currentTime = Time.time; // 获取当前游戏时间
            
            // 按固定间隔更新建筑的建造进度
            if (currentTime - lastConstructionUpdate >= CONSTRUCTION_UPDATE_INTERVAL)
            {
                UpdateConstruction(CONSTRUCTION_UPDATE_INTERVAL); // 传入固定的时间间隔进行更新
                lastConstructionUpdate = currentTime; // 更新上次建造更新的时间戳
            }
            
            // 按固定间隔更新建筑的生产逻辑
            if (currentTime - lastProductionUpdate >= PRODUCTION_UPDATE_INTERVAL)
            {
                UpdateBuildingProduction(PRODUCTION_UPDATE_INTERVAL); // 传入固定的时间间隔进行更新
                lastProductionUpdate = currentTime; // 更新上次生产更新的时间戳
            }
            
            // Debug.Log("[增强建筑系统] Update 方法被调用。"); // 确认Update被执行
        }

        #endregion

        #region 系统管理 (System Management) - 原OnGameUpdate内容移至Update
        
        // /// <summary>
        // /// 处理游戏更新事件，用于驱动建筑系统的内部逻辑更新。
        // /// </summary>
        // /// <param name="e">游戏更新事件参数，包含deltaTime。</param>
        // private void OnGameUpdate(GameUpdateEvent e)
        // {
        //     // 当前此方法体为空，因为更新逻辑已移至独立的 Update() 方法，
        //     // 并由外部（可能是游戏主循环或QFramework的轮询机制）调用。
        //     // 如果希望通过事件驱动，可以将Update()中的逻辑移回此处，并确保事件被正确发送。
        // }
        
        /// <summary>
        /// 更新所有在建建筑的建造进度。
        /// </summary>
        /// <param name="deltaTime">自上次更新以来经过的时间（秒）。</param>
        private void UpdateConstruction(float deltaTime) // 此方法现在由内部Update调用
        {
            var completedBuildings = new List<string>(); // 存储本轮完成建造的建筑ID
            
            // 遍历constructionProgress字典的副本，以允许在循环中修改原字典（通过CompleteConstruction）
            foreach (var kvp in constructionProgress.ToList())
            {
                string buildingId = kvp.Key;
                float currentProgress = kvp.Value;
                
                if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 检查建筑数据是否存在
                {
                    Debug.LogError($"[增强建筑系统] 更新建造进度时找不到建筑数据 (ID: {buildingId})。可能已被移除。");
                    completedBuildings.Add(buildingId); // 标记为“完成”以从进度字典中移除
                    continue;
                }
                var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
                if (config == null || config.BuildTime <= 0)
                {
                     Debug.LogError($"[增强建筑系统] 建筑 {building.ConfigId} 配置无效或建造时间为0，无法更新进度。");
                    completedBuildings.Add(buildingId);
                    continue;
                }
                
                // 计算此时间间隔内应增加的建造速率/进度
                // 建造速率 = 1 / 总建造时间 (单位：进度/秒)
                // 假设BuildTime单位是小时，需转换为秒
                float constructionRatePerSecond = 1f / (config.BuildTime * 3600f);
                // TODO: 此处可以加入工人数量、技能等对建造速率的影响因子
                
                currentProgress += deltaTime * constructionRatePerSecond; // 更新进度
                constructionProgress[buildingId] = currentProgress;       // 写回字典
                building.BuildProgress = currentProgress;                 // 更新BuildingData中的进度
                building.Health = Mathf.Lerp(config.MaxHealth * 0.1f, config.MaxHealth, currentProgress); // 血量随进度线性增加
                
                if (currentProgress >= 1f) // 如果进度达到或超过100%
                {
                    completedBuildings.Add(buildingId); // 添加到完成列表
                }
            }
            
            // 处理所有在本轮完成建造的建筑
            foreach (string buildingId in completedBuildings)
            {
                CompleteConstruction(buildingId); // 调用完成建造的逻辑
            }
        }
        
        #endregion

        #region 辅助方法 (Helper Methods)
        
        /// <summary>
        /// 在指定位置显示一个漂浮的文本（例如，资源产出提示）。
        /// </summary>
        /// <param name="position">文本产生的世界坐标位置。</param>
        /// <param name="text">要显示的文本内容。</param>
        /// <param name="color">文本的颜色。</param>
        private void ShowFloatingText(Vector3 position, string text, Color color)
        {
            // 创建一个新的GameObject用于承载漂浮文字
            GameObject floatingTextGO = new GameObject("FloatingText_" + text); // 命名以便调试
            floatingTextGO.transform.position = position + Vector3.up * 0.5f; // 在建筑位置上方0.5单位处显示
            
            // 为漂浮文字添加Canvas组件，使其能在世界空间中渲染UI元素
            Canvas canvas = floatingTextGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace; // 世界空间渲染模式
            canvas.sortingOrder = 10; // 设置较高的排序顺序以确保在其他物体之上显示
            
            // 添加CanvasScaler组件，用于控制UI元素的缩放，使文字大小相对固定
            CanvasScaler canvasScaler = floatingTextGO.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 20f; // 调整每单位像素数，影响文字清晰度和大小
            
            // 创建文本子对象
            GameObject textGO = new GameObject("TextElement");
            textGO.transform.SetParent(floatingTextGO.transform); // 设置父对象为Canvas的GameObject
            
            // 添加UnityEngine.UI.Text组件用于显示文本
            UnityEngine.UI.Text textComponent = textGO.AddComponent<UnityEngine.UI.Text>();
            textComponent.text = text; // 设置文本内容
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // 使用内置字体 (项目中应替换为自定义字体)
            textComponent.fontSize = 3; // 设置字体大小 (根据dynamicPixelsPerUnit调整)
            textComponent.color = color; // 设置文本颜色
            textComponent.alignment = TextAnchor.MiddleCenter; // 文本居中对齐
            textComponent.fontStyle = FontStyle.Bold; // 加粗

            // 设置文本框的RectTransform属性
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(20, 7.5f); // 设置文本框大小 (根据字体大小和内容调整)
            textRect.anchoredPosition = Vector2.zero;   // 相对于父对象（Canvas）居中
            
            // 启动漂浮和淡出动画效果
            StartFloatingAnimation(floatingTextGO, 1.5f); // 动画持续1.5秒
        }
        
        /// <summary>
        /// 为指定的GameObject启动一个简单的向上漂浮并淡出的动画。
        /// </summary>
        /// <param name="floatingTextGO">要应用动画的GameObject（通常是承载漂浮文字的Canvas对象）。</param>
        /// <param name="duration">动画的持续时间（秒）。</param>
        private void StartFloatingAnimation(GameObject floatingTextGO, float duration)
        {
            // 给GameObject添加自定义的SimpleFloatingText脚本组件来处理动画逻辑
            SimpleFloatingText floatingComponent = floatingTextGO.AddComponent<SimpleFloatingText>();
            floatingComponent.Initialize(duration); // 初始化动画参数
        }
        
        /// <summary>
        /// 根据资源类型获取一个预设的颜色，用于UI显示等。
        /// </summary>
        /// <param name="resourceType">资源类型枚举。</param>
        /// <returns>对应的颜色；如果未定义则返回白色。</returns>
        private Color GetResourceColor(ResourceType resourceType)
        {
            switch (resourceType) // 根据不同的资源类型返回特定颜色
            {
                case ResourceType.Food:           return new Color(0.8f, 0.6f, 0.2f); // 食物 - 橙黄色
                case ResourceType.Water:          return new Color(0.2f, 0.6f, 1f);   // 水 - 蓝色
                case ResourceType.Materials:      return new Color(0.6f, 0.4f, 0.2f); // 材料 - 棕色
                case ResourceType.Ammunition:     return new Color(1f, 0.2f, 0.2f);   // 弹药 - 红色
                case ResourceType.Energy:         return new Color(1f, 1f, 0.2f);   // 能源 - 黄色
                case ResourceType.ResearchPoints: return new Color(0.6f, 0.2f, 1f);   // 科研点 - 紫色
                default:                          return Color.white; // 其他未知类型 - 白色
            }
        }
        
        /// <summary>
        /// 将给定的世界坐标位置对齐到预定义的网格。
        /// </summary>
        /// <param name="position">原始世界坐标。</param>
        /// <returns>对齐到网格后的坐标。</returns>
        private Vector3 SnapToGrid(Vector3 position)
        {
            // 将X和Y坐标分别对齐到最近的网格点
            float snappedX = Mathf.Round(position.x / GRID_SIZE) * GRID_SIZE;
            float snappedY = Mathf.Round(position.y / GRID_SIZE) * GRID_SIZE;
            return new Vector3(snappedX, snappedY, position.z); // Z坐标通常保持不变或设为0（如果是2D俯视角）
        }
        
        /// <summary>
        /// 检查以指定中心点和建筑尺寸构成的区域是否已被其他建筑占用。
        /// </summary>
        /// <param name="centerPosition">候选区域的中心世界坐标。</param>
        /// <param name="buildingSize">建筑的占地尺寸（网格单位）。</param>
        /// <returns>如果区域被占用则返回true，否则返回false。</returns>
        private bool IsAreaOccupied(Vector3 centerPosition, Vector2Int buildingSize)
        {
            Vector3 gridAlignedCenter = SnapToGrid(centerPosition); // 首先将中心点对齐到网格
            
            // 计算建筑实际占用的网格范围的边界
            // 假设buildingSize是奇数时中心点在格子中央，偶数时在格子角点或边上，这里简化处理
            int halfWidth = Mathf.FloorToInt(buildingSize.x / 2f);
            int halfHeight = Mathf.FloorToInt(buildingSize.y / 2f);
            // 注意：对于偶数尺寸，上述计算可能需要调整以确保覆盖正确。
            // 更精确的方式是迭代从 (centerX - sizeX/2*GRID_SIZE) 到 (centerX + sizeX/2*GRID_SIZE)
            
            // 遍历建筑将占用的每一个网格单元
            for (int xOffset = -halfWidth; xOffset < halfWidth + (buildingSize.x % 2 == 0 ? 0 : 1) ; xOffset++) // 修正偶数宽度遍历
            {
                for (int yOffset = -halfHeight; yOffset < halfHeight + (buildingSize.y % 2 == 0 ? 0 : 1); yOffset++) // 修正偶数高度遍历
                {
                    Vector3 testGridCellCenter = gridAlignedCenter + new Vector3(xOffset * GRID_SIZE, yOffset * GRID_SIZE, 0);
                    
                    // 检查此网格单元是否已被现有建筑的任何部分占用
                    foreach (var existingBuilding in gameModel.Buildings.Values)
                    {
                        // 获取现有建筑的尺寸和对齐后的位置
                        var existingConfig = configSystem.GetBuildingConfig(existingBuilding.ConfigId);
                        if (existingConfig == null) continue;
                        Vector2Int existingSize = existingConfig.Size;
                        Vector3 existingGridPos = existingBuilding.Position; // 假设已对齐

                        // 简单的碰撞检测：如果测试单元的中心点落入现有建筑的包围盒内
                        // (更精确的检测需要考虑两个建筑的完整包围盒是否重叠)
                        Rect existingBuildingRect = new Rect(
                            existingGridPos.x - (existingSize.x / 2f * GRID_SIZE),
                            existingGridPos.y - (existingSize.y / 2f * GRID_SIZE),
                            existingSize.x * GRID_SIZE,
                            existingSize.y * GRID_SIZE
                        );
                        if (existingBuildingRect.Contains(testGridCellCenter)) // 使用矩形包含点测试
                        {
                            return true; // 区域被占用
                        }
                    }
                }
            }
            
            return false; // 该区域未被占用
        }
        
        /// <summary>
        /// 检查是否已满足所有指定的科技前置条件。
        /// </summary>
        /// <param name="requiredTechs">必需的科技ID列表。</param>
        /// <returns>如果所有科技都已研究完成则返回true，否则返回false。</returns>
        private bool AreTechRequirementsMet(List<string> requiredTechs)
        {
            if (techSystem == null) // 如果没有科技系统，则认为所有科技条件都满足 (或应报错)
            {
                Debug.LogWarning("[增强建筑系统] 科技系统未初始化，无法检查科技需求。默认允许。");
                return true;
            }
            
            if (requiredTechs == null || !requiredTechs.Any()) return true; // 没有科技要求，则视为满足

            foreach (var techId in requiredTechs) // 遍历所有必需的科技
            {
                if (!techSystem.IsTechResearched(techId)) // 检查该科技是否已研究
                    return false; // 只要有一个未研究，则条件不满足
            }
            return true; // 所有科技都已研究
        }
        
        /// <summary>
        /// 获取指定建筑类型的最大允许建造数量。
        /// （示例逻辑，实际游戏中可能更复杂或从配置读取）
        /// </summary>
        /// <param name="buildingType">建筑类型ID。</param>
        /// <returns>该类型建筑的最大数量限制。</returns>
        private int GetMaxBuildingCount(string buildingType)
        {
            var config = configSystem.GetBuildingConfig(buildingType); // 获取建筑配置
            if (config == null) return 1; // 如果配置不存在，默认限制为1（或0）
            
            // 示例：根据建筑类别设定不同的数量上限
            switch (config.Category)
            {
                case BuildingCategory.Defense:    return 100; // 防御塔可以建很多
                case BuildingCategory.Production: return 20;  // 生产建筑有一定上限
                case BuildingCategory.Habitat:    return 15;  // 居住建筑上限
                case BuildingCategory.Storage:    return 10;  // 存储建筑上限
                case BuildingCategory.Functional: return 5;   // 特殊功能建筑可能数量更少
                default:                          return 10;  // 其他类型默认上限
            }
        }
        
        /// <summary>
        /// 查找指定建筑ID的下一个升级配置。
        /// </summary>
        /// <param name="currentBuildingConfigId">当前建筑的配置ID。</param>
        /// <returns>对应的BuildingUpgrade配置；如果找不到升级路径，则返回null。</returns>
        private BuildingUpgrade FindUpgradeConfig(string currentBuildingConfigId)
        {
            // 遍历所有已定义的建筑升级配置
            // (ConfigSystem应提供一个GetAllBuildingUpgrades()方法或直接访问其内部存储)
            // 此处假设ConfigSystem有一个方法可以获取所有升级配置，或按源ID查找
            // 这是一个简化的查找逻辑，实际可能需要更复杂的匹配
            var allConfigs = configSystem.GetAllBuildingConfigs(); // 获取所有建筑配置
            foreach (var potentialTargetConfig in allConfigs.Values) // 遍历所有可能的升级目标配置
            {
                // 尝试从ConfigSystem获取从当前建筑到潜在目标建筑的升级路径信息
                var upgradePath = configSystem.GetBuildingUpgrade(currentBuildingConfigId, potentialTargetConfig.ConfigId);
                if (upgradePath != null) // 如果找到了一个有效的升级路径
                    return upgradePath; // 返回这个升级配置
            }
            return null; // 没有找到可用的升级路径
        }
        
        /// <summary>
        /// 获取指定建筑类型的科技生产加成。
        /// </summary>
        /// <param name="buildingType">建筑类型ID。</param>
        /// <returns>科技提供的生产效率加成百分比（例如0.1代表+10%）。</returns>
        private float GetTechProductionBonus(string buildingType)
        {
            if (techSystem == null) return 0f; // 无科技系统则无加成
            
            float bonus = 0f;
            var config = configSystem.GetBuildingConfig(buildingType); // 获取建筑配置
            if (config == null) return bonus; // 无配置则无加成
            
            // 示例：根据建筑类别从科技系统查询对应的效率加成效果
            switch (config.Category)
            {
                case BuildingCategory.Production:
                    // 假设科技效果名为 "production_efficiency"
                    bonus += techSystem.GetTechEffectValue("production_efficiency", 0f);
                    break;
                case BuildingCategory.Defense:
                    // 防御建筑的“生产”可能是指弹药制造速度或防御效果，此处假设为 "defense_effectiveness"
                    bonus += techSystem.GetTechEffectValue("defense_effectiveness", 0f);
                    break;
                // 可以为其他类别添加特定的科技效果查询
            }
            
            return bonus; // 返回总的科技加成
        }
        
        /// <summary>
        /// 创建建筑在Unity场景中的可视化GameObject。
        /// </summary>
        /// <param name="building">要创建视觉对象的建筑数据。</param>
        private void CreateBuildingGameObject(BuildingData building)
        {
            var config = configSystem.GetBuildingConfig(building.ConfigId); // 获取建筑配置
            if (config == null)
            {
                Debug.LogError($"[增强建筑系统] 创建建筑GameObject失败：找不到配置ID {building.ConfigId}。");
                return;
            }
            
            // 创建一个新的空GameObject作为建筑的根对象
            GameObject buildingGO = new GameObject($"Building_{building.ConfigId}_{building.Id}"); //规范命名
            buildingGO.transform.position = building.Position; // 设置其在场景中的位置
            
            // 添加SpriteRenderer组件用于2D显示 (或MeshRenderer用于3D)
            var spriteRenderer = buildingGO.AddComponent<SpriteRenderer>();
            // 创建或加载建筑的纹理/精灵图
            // 此处使用一个辅助方法CreateBuildingTexture动态创建简单纹理作为示例
            var texture = CreateBuildingTexture(building.ConfigId);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                                     new Vector2(0.5f, 0.5f), 32f); // 32 pixels per unit示例
            
            spriteRenderer.sprite = sprite; // 设置精灵
            spriteRenderer.sortingOrder = 1; // 设置渲染排序顺序 (确保在背景之上)
            spriteRenderer.sortingLayerName = "Buildings"; // （可选）使用排序层
            
            // 根据建造状态设置初始视觉效果（例如，半透明表示正在建造）
            if (constructionProgress.ContainsKey(building.Id))
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.6f); // 半透明白色
            }
            
            buildingObjects[building.Id] = buildingGO; // 将创建的GameObject存入字典，与建筑数据关联
        }
        
        /// <summary>
        /// （辅助示例）动态创建一个简单的纹理作为建筑的视觉表现。
        /// 实际项目中通常会加载预制的精灵或模型。
        /// </summary>
        /// <param name="buildingType">建筑类型ID，用于决定纹理颜色等。</param>
        /// <returns>生成的Texture2D对象。</returns>
        private Texture2D CreateBuildingTexture(string buildingType)
        {
            Texture2D texture = new Texture2D(32, 32); // 创建一个32x32像素的纹理
            Color buildingColor = GetBuildingColor(buildingType); // 根据建筑类型获取基础颜色
            
            // 简单填充颜色，边缘颜色稍暗以示区别
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    if (x < 2 || x >= texture.width - 2 || y < 2 || y >= texture.height - 2) // 边缘2像素
                    {
                        texture.SetPixel(x, y, buildingColor * 0.8f); // 边缘颜色稍暗
                    }
                    else
                    {
                        texture.SetPixel(x, y, buildingColor); // 中心颜色
                    }
                }
            }
            
            texture.Apply(); // 应用像素更改
            return texture;
        }
        
        /// <summary>
        /// （辅助示例）根据建筑类型获取一个预设的基础颜色。
        /// </summary>
        /// <param name="buildingType">建筑类型ID。</param>
        /// <returns>对应的颜色；如果配置不存在或未定义颜色，则返回白色。</returns>
        private Color GetBuildingColor(string buildingType)
        {
            var config = configSystem.GetBuildingConfig(buildingType); // 获取建筑配置
            if (config == null) return Color.white; // 配置不存在则返回白色
            
            // 根据建筑类别返回不同颜色 (示例)
            switch (config.Category)
            {
                case BuildingCategory.Habitat:    return Color.blue;   // 居住类 - 蓝色
                case BuildingCategory.Production: return Color.green;  // 生产类 - 绿色
                case BuildingCategory.Defense:    return Color.red;    // 防御类 - 红色
                case BuildingCategory.Storage:    return new Color(1f, 0.5f, 0f); // 存储类 - 橙色
                case BuildingCategory.Functional: return Color.magenta;// 功能类 -品红色
                default:                          return Color.white;  // 其他 - 白色
            }
        }
        
        /// <summary>
        /// 更新指定ID建筑的视觉表现，通常基于其当前状态（如生命值、运作状态）。
        /// </summary>
        /// <param name="buildingId">要更新视觉的建筑ID。</param>
        private void UpdateBuildingVisual(string buildingId)
        {
            if (!buildingObjects.TryGetValue(buildingId, out var buildingGO)) // 检查GameObject是否存在
                return;
            
            if (!gameModel.Buildings.TryGetValue(buildingId, out var building)) // 检查建筑数据是否存在
                return;
            
            var spriteRenderer = buildingGO.GetComponent<SpriteRenderer>(); // 获取SpriteRenderer组件
            if (spriteRenderer == null) return; // 如果没有SpriteRenderer，则无法更新视觉
            
            // 根据建筑的当前状态更新颜色或透明度等视觉属性
            switch (building.State)
            {
                case BuildingState.UnderConstruction: // 建造中
                    spriteRenderer.color = new Color(1f, 1f, 1f, 0.6f); // 半透明
                    // TODO: 可能还会显示建造动画或脚手架模型
                    break;
                case BuildingState.Damaged: // 已损坏
                    spriteRenderer.color = Color.gray; // 灰色表示损坏
                    // TODO: 可能还会显示损坏贴图或烟雾效果
                    break;
                case BuildingState.Destroyed: // 已被摧毁
                    spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // 深灰色半透明表示废墟
                    // TODO: 可能替换为废墟模型
                    break;
                case BuildingState.Operational: // 正常运作
                    if (building.Health < building.MaxHealth * 0.5f) // 如果血量低于50%
                        spriteRenderer.color = new Color(1f, 0.7f, 0.7f); // 轻微红色调表示低血量
                    else
                        spriteRenderer.color = Color.white; // 正常颜色
                    break;
                // 可以为Upgrading, Paused等其他状态添加特定视觉效果
                default:
                    spriteRenderer.color = Color.white; // 默认颜色
                    break;
            }
        }
        
        #endregion
    }

    /// <summary>
    /// 简单的漂浮文字组件，用于在游戏世界中显示临时的文本信息（如资源产出、伤害数字等）。
    /// 文本会自动向上漂浮并逐渐淡出。
    /// </summary>
    public class SimpleFloatingText : MonoBehaviour
    {
        /// <summary>
        /// 动画效果的总持续时间（秒）。
        /// </summary>
        private float duration;
        /// <summary>
        /// 漂浮动画的起始世界坐标。
        /// </summary>
        private Vector3 startPosition;
        /// <summary>
        /// 漂浮动画的结束世界坐标（通常在起始位置上方）。
        /// </summary>
        private Vector3 endPosition;
        /// <summary>
        /// UnityEngine.UI.Text组件的引用，用于显示文本。
        /// </summary>
        private Text textComponent;
        /// <summary>
        /// 文本动画起始颜色。
        /// </summary>
        private Color startColor;
        /// <summary>
        /// 文本动画结束颜色（通常是完全透明）。
        /// </summary>
        private Color endColor;
        /// <summary>
        /// 动画已进行的时间（秒）。
        /// </summary>
        private float elapsed;
        
        /// <summary>
        /// 初始化漂浮文字动画的参数。
        /// </summary>
        /// <param name="duration">动画效果的总持续时间（秒）。</param>
        public void Initialize(float duration)
        {
            this.duration = duration;
            this.elapsed = 0f; // 重置已用时间
            
            // 获取子对象中的Text组件
            textComponent = GetComponentInChildren<Text>();
            if (textComponent == null) // 安全检查
            {
                Debug.LogError("[SimpleFloatingText] 初始化失败：未能找到子对象中的Text组件。");
                Destroy(gameObject); // 没有Text组件则销毁自身
                return;
            }
            
            // 设置动画的起始和结束位置
            startPosition = transform.position; // 当前GameObject的位置即为起始位置
            endPosition = startPosition + Vector3.up * 1f; // 目标位置在起始位置上方1个单位处 (向上漂浮)
            
            // 设置动画的起始和结束颜色 (用于淡出效果)
            startColor = textComponent.color; // 起始颜色为Text组件当前颜色
            endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 结束颜色为完全透明
        }
        
        /// <summary>
        /// MonoBehaviour的Update方法，每帧调用以更新动画状态。
        /// </summary>
        private void Update()
        {
            if (textComponent == null) return; // 如果Text组件无效则不执行
            
            elapsed += Time.deltaTime; //累加已用时间
            float progress = elapsed / duration; // 计算当前动画进度 (0到1)
            
            if (progress >= 1f) // 如果动画已达到或超过总时长
            {
                Destroy(gameObject); // 销毁此漂浮文字对象
                return;
            }
            
            // 根据进度线性插值更新GameObject的位置 (实现向上漂浮)
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            
            // 根据进度线性插值更新文本颜色 (实现淡出效果)
            textComponent.color = Color.Lerp(startColor, endColor, progress);
        }
    }
} 