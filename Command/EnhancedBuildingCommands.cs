// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：EnhancedBuildingCommands.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件包含与增强型建筑系统相关的各种命令，
//     用于处理建筑的建造、升级、拆除、修复、操作控制、工人分配、产出收集等。
//     还包括一些调试用的批量操作命令。
// ==============================================================================

using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem; // 包含 IEnhancedBuildingSystem 接口
using SurvivalGame.Model;     // 包含 ISurvivalGameModel 及相关建筑模型数据
using System.Linq;            // 用于 Linq 查询，例如 OrderByDescending

namespace SurvivalGame.Command
{
    /// <summary>
    /// 开始建造建筑命令
    /// </summary>
    public class ConstructBuildingCommand : AbstractCommand
    {
        // 建筑的预定位置
        private Vector3 position;
        // 要建造的建筑类型标识符
        private string buildingType;
        
        /// <summary>
        /// ConstructBuildingCommand 构造函数
        /// </summary>
        /// <param name="position">建筑在世界中的目标位置</param>
        /// <param name="buildingType">要建造的建筑的类型或配置ID</param>
        public ConstructBuildingCommand(Vector3 position, string buildingType)
        {
            this.position = position;
            this.buildingType = buildingType;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            // 获取增强型建筑系统实例
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            // 尝试开始建造过程
            bool success = buildingSystem.StartConstruction(position, buildingType);
            
            // 根据结果记录日志
            if (success)
            {
                Debug.Log($"开始建造 {buildingType} 在位置 {position}");
            }
            else
            {
                Debug.LogWarning($"无法开始建造 {buildingType} 在位置 {position} (例如：资源不足、位置无效等)");
            }
        }
    }
    
    /// <summary>
    /// 升级指定建筑命令
    /// </summary>
    public class UpgradeBuildingCommand : AbstractCommand
    {
        // 要升级的建筑的唯一ID
        private string buildingId;
        
        /// <summary>
        /// UpgradeBuildingCommand 构造函数
        /// </summary>
        /// <param name="buildingId">要升级的建筑的ID</param>
        public UpgradeBuildingCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            // 尝试升级建筑
            bool success = buildingSystem.UpgradeBuilding(buildingId);
            
            if (success)
            {
                // 获取升级后的建筑信息用于日志记录
                var building = buildingSystem.GetBuilding(buildingId);
                Debug.Log($"建筑 {building?.ConfigId} (ID: {buildingId}) 升级成功");
            }
            else
            {
                Debug.LogWarning($"无法升级建筑 ID: {buildingId} (例如：已达最高等级、资源不足等)");
            }
        }
    }
    
    /// <summary>
    /// 拆除指定建筑命令
    /// </summary>
    public class DemolishBuildingCommand : AbstractCommand
    {
        // 要拆除的建筑的唯一ID
        private string buildingId;
        
        /// <summary>
        /// DemolishBuildingCommand 构造函数
        /// </summary>
        /// <param name="buildingId">要拆除的建筑的ID</param>
        public DemolishBuildingCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            // 尝试拆除建筑
            bool success = buildingSystem.DemolishBuilding(buildingId);
            
            if (success)
            {
                Debug.Log($"成功拆除建筑 ID: {buildingId}");
            }
            else
            {
                Debug.LogWarning($"无法拆除建筑 ID: {buildingId}");
            }
        }
    }
    
    /// <summary>
    /// 修复指定建筑命令
    /// </summary>
    public class RepairBuildingCommand : AbstractCommand
    {
        // 要修复的建筑的唯一ID
        private string buildingId;
        
        /// <summary>
        /// RepairBuildingCommand 构造函数
        /// </summary>
        /// <param name="buildingId">要修复的建筑的ID</param>
        public RepairBuildingCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            // 尝试修复建筑
            bool success = buildingSystem.RepairBuilding(buildingId);
            
            if (success)
            {
                Debug.Log($"成功修复建筑 ID: {buildingId}");
            }
            else
            {
                Debug.LogWarning($"无法修复建筑 ID: {buildingId} (例如：资源不足、建筑已满血等)");
            }
        }
    }
    
    /// <summary>
    /// 切换建筑操作状态命令 (例如：启用/停用生产)
    /// </summary>
    public class ToggleBuildingOperationCommand : AbstractCommand
    {
        // 要切换操作状态的建筑的唯一ID
        private string buildingId;
        
        /// <summary>
        /// ToggleBuildingOperationCommand 构造函数
        /// </summary>
        /// <param name="buildingId">要切换操作状态的建筑ID</param>
        public ToggleBuildingOperationCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            // 尝试切换建筑的操作状态
            bool success = buildingSystem.ToggleBuildingOperation(buildingId);
            
            if (success)
            {
                // 获取建筑信息和当前操作状态用于日志
                var building = buildingSystem.GetBuilding(buildingId);
                bool isOperational = buildingSystem.IsBuildingOperational(buildingId);
                string status = isOperational ? "启用" : "停用"; // 根据状态确定描述文本
                Debug.Log($"建筑 {building?.ConfigId} (ID: {buildingId}) 已成功切换为 {status} 状态");
            }
            else
            {
                Debug.LogWarning($"无法切换建筑 ID: {buildingId} 的操作状态");
            }
        }
    }
    
    /// <summary>
    /// 分配工人到指定建筑命令
    /// </summary>
    public class AssignWorkersToBuildingCommand : AbstractCommand
    {
        // 目标建筑的唯一ID
        private string buildingId;
        // 要分配的工人的唯一ID (或工人数量的标识，具体取决于系统实现)
        private string workerId; // TODO: 确认workerId是单个工人还是工人类型/数量
        
        /// <summary>
        /// AssignWorkersToBuildingCommand 构造函数
        /// </summary>
        /// <param name="buildingId">工人将要分配到的建筑ID</param>
        /// <param name="workerId">要分配的工人ID或标识</param>
        public AssignWorkersToBuildingCommand(string buildingId, string workerId)
        {
            this.buildingId = buildingId;
            this.workerId = workerId;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            // 执行工人分配逻辑 (具体实现由buildingSystem处理)
            buildingSystem.AssignWorkersToBuilding(buildingId, workerId);
            
            var building = buildingSystem.GetBuilding(buildingId);
            // 日志记录分配结果
            Debug.Log($"为建筑 {building?.ConfigId} (ID: {buildingId}) 分配了ID为 {workerId} 的工人/工作单元");
        }
    }
    
    /// <summary>
    /// 收集指定建筑产出物命令
    /// </summary>
    public class CollectBuildingOutputCommand : AbstractCommand
    {
        // 要收集产出的建筑的唯一ID
        private string buildingId;
        
        /// <summary>
        /// CollectBuildingOutputCommand 构造函数
        /// </summary>
        /// <param name="buildingId">要收集产出的建筑ID</param>
        public CollectBuildingOutputCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            // 执行收集产出逻辑
            buildingSystem.CollectBuildingOutput(buildingId);
            
            var building = buildingSystem.GetBuilding(buildingId);
            Debug.Log($"已尝试收集建筑 {building?.ConfigId} (ID: {buildingId}) 的产出");
        }
    }
    
    /// <summary>
    /// 取消正在进行的建筑建造过程命令
    /// </summary>
    public class CancelConstructionCommand : AbstractCommand
    {
        // 正在建造且要取消的建筑的唯一ID
        private string buildingId;
        
        /// <summary>
        /// CancelConstructionCommand 构造函数
        /// </summary>
        /// <param name="buildingId">要取消建造的建筑ID</param>
        public CancelConstructionCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            // 尝试取消建造
            bool success = buildingSystem.CancelConstruction(buildingId);
            
            if (success)
            {
                Debug.Log($"成功取消建造建筑 ID: {buildingId}");
            }
            else
            {
                Debug.LogWarning($"无法取消建造建筑 ID: {buildingId} (例如：建筑已完成或不存在)");
            }
        }
    }
    
    /// <summary>
    /// 快速建造命令（主要用于调试，会先添加资源并立即完成建造）
    /// </summary>
    public class QuickBuildCommand : AbstractCommand
    {
        // 建筑的目标位置
        private Vector3 position;
        // 要快速建造的建筑类型
        private string buildingType;
        
        /// <summary>
        /// QuickBuildCommand 构造函数
        /// </summary>
        /// <param name="position">建筑位置</param>
        /// <param name="buildingType">建筑类型</param>
        public QuickBuildCommand(Vector3 position, string buildingType)
        {
            this.position = position;
            this.buildingType = buildingType;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            var gameModel = this.GetModel<ISurvivalGameModel>(); // 用于修改游戏数据模型，如资源
            
            // 步骤1: (调试用) 添加足够的虚拟资源以确保建造可以开始
            gameModel.Materials.Value += 1000; // 假设Materials是资源属性
            
            // 步骤2: 尝试开始建造
            if (buildingSystem.StartConstruction(position, buildingType))
            {
                // 步骤3: 如果开始建造成功，则立即完成它
                // 需要找到刚开始建造的建筑实例。这里假设通过获取所有建筑并按ID排序来找到最新的一个。
                // 注意：这种方式在多线程或高并发场景下可能不完全可靠，但对于单机调试通常可行。
                var buildings = buildingSystem.GetAllBuildings();
                // 根据ID或其他创建时间戳属性降序排列，获取第一个，即最新的建筑
                var latestBuilding = buildings.OrderByDescending(b => b.Id).FirstOrDefault(b => b.CurrentState == BuildingState.Constructing && b.ConfigId == buildingType);
                if (latestBuilding != null)
                {
                    buildingSystem.CompleteConstruction(latestBuilding.Id); // 调用系统方法立即完成建造
                    Debug.Log($"快速建造完成: {buildingType} 在位置 {position}, ID: {latestBuilding.Id}");
                }
                else
                {
                    Debug.LogWarning($"快速建造：开始建造 {buildingType} 后未能找到对应的建造中实例。");
                }
            }
            else
            {
                Debug.LogWarning($"快速建造失败: 无法开始建造 {buildingType}");
            }
        }
    }
    
    /// <summary>
    /// 批量建造命令（主要用于调试，会添加资源并快速建造多个建筑）
    /// </summary>
    public class BatchBuildCommand : AbstractCommand
    {
        // 要批量建造的建筑类型
        private string buildingType;
        // 要建造的数量
        private int count;
        
        /// <summary>
        /// BatchBuildCommand 构造函数
        /// </summary>
        /// <param name="buildingType">建筑类型</param>
        /// <param name="count">建造数量</param>
        public BatchBuildCommand(string buildingType, int count)
        {
            this.buildingType = buildingType;
            this.count = count;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            var gameModel = this.GetModel<ISurvivalGameModel>();
            
            // 步骤1: (调试用) 添加足够的总资源
            gameModel.Materials.Value += count * 100; // 假设每个建筑平均需要100材料
            
            int builtCount = 0; // 成功建造的数量
            for (int i = 0; i < count; i++)
            {
                // 步骤2: 获取下一个可用的建造位置 (具体实现由buildingSystem提供)
                Vector3 position = buildingSystem.GetNextBuildPosition(buildingType);
                
                // 步骤3: 尝试开始建造
                if (buildingSystem.StartConstruction(position, buildingType))
                {
                    // 步骤4: 立即完成建造 (同QuickBuildCommand逻辑)
                    var buildings = buildingSystem.GetAllBuildings();
                    var latestBuilding = buildings.OrderByDescending(b => b.Id).FirstOrDefault(b => b.CurrentState == BuildingState.Constructing && b.ConfigId == buildingType && b.Position == position);
                    if (latestBuilding != null)
                    {
                        buildingSystem.CompleteConstruction(latestBuilding.Id);
                        builtCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"批量建造：开始建造第 {i+1} 个 {buildingType} 后未能找到对应实例。");
                        // 可能需要中断，或者记录失败并继续
                    }
                }
                else
                {
                    Debug.LogWarning($"批量建造：在尝试建造第 {i+1} 个 {buildingType} 时失败，建造中止。");
                    break; // 如果一次建造失败（如位置不足），则停止批量建造
                }
            }
            
            Debug.Log($"批量建造完成: 成功建造 {builtCount}/{count} 个 {buildingType}");
        }
    }
    
    /// <summary>
    /// 修复所有受损建筑命令（主要用于调试）
    /// </summary>
    public class RepairAllBuildingsCommand : AbstractCommand
    {
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            var gameModel = this.GetModel<ISurvivalGameModel>();
            
            // (调试用) 添加一些通用修复材料
            gameModel.Materials.Value += 500;
            
            var buildings = buildingSystem.GetAllBuildings(); // 获取所有建筑实例
            int repairedCount = 0; // 成功修复的建筑数量
            
            foreach (var building in buildings)
            {
                // 检查建筑是否需要修复 (例如，生命值低于其最大生命值)
                // 假设BuildingData有Health和MaxHealth属性，并且100f是最大生命值的通用参考或具体值
                // TODO: 确认building.Health 和其最大值的比较方式
                if (building.Health < building.MaxHealth) // 假设MaxHealth存在
                {
                    if (buildingSystem.RepairBuilding(building.Id))
                    {
                        repairedCount++;
                    }
                }
            }
            
            Debug.Log($"已尝试修复所有建筑，成功修复了 {repairedCount} 座建筑");
        }
    }
    
    /// <summary>
    /// 升级所有可升级建筑命令（主要用于调试）
    /// </summary>
    public class UpgradeAllBuildingsCommand : AbstractCommand
    {
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            var gameModel = this.GetModel<ISurvivalGameModel>();
            
            // (调试用) 添加大量升级材料
            gameModel.Materials.Value += 2000;
            
            var buildings = buildingSystem.GetAllBuildings();
            int upgradedCount = 0; // 成功升级的建筑数量
            
            foreach (var building in buildings)
            {
                // 尝试升级每个建筑 (buildingSystem.UpgradeBuilding内部应有检查是否可升级的逻辑)
                if (buildingSystem.UpgradeBuilding(building.Id))
                {
                    upgradedCount++;
                }
            }
            
            Debug.Log($"已尝试升级所有建筑，成功升级了 {upgradedCount} 座建筑");
        }
    }
}