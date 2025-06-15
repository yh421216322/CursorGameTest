using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Model;
using System.Linq;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 建造建筑命令
    /// </summary>
    public class ConstructBuildingCommand : AbstractCommand
    {
        private Vector3 position;
        private string buildingType;
        
        public ConstructBuildingCommand(Vector3 position, string buildingType)
        {
            this.position = position;
            this.buildingType = buildingType;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            bool success = buildingSystem.StartConstruction(position, buildingType);
            
            if (success)
            {
                Debug.Log($"开始建造 {buildingType} 在位置 {position}");
            }
            else
            {
                Debug.LogWarning($"无法建造 {buildingType} 在位置 {position}");
            }
        }
    }
    
    /// <summary>
    /// 升级建筑命令
    /// </summary>
    public class UpgradeBuildingCommand : AbstractCommand
    {
        private string buildingId;
        
        public UpgradeBuildingCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            bool success = buildingSystem.UpgradeBuilding(buildingId);
            
            if (success)
            {
                var building = buildingSystem.GetBuilding(buildingId);
                Debug.Log($"升级建筑 {building?.ConfigId} 成功");
            }
            else
            {
                Debug.LogWarning($"无法升级建筑 ID: {buildingId}");
            }
        }
    }
    
    /// <summary>
    /// 拆除建筑命令
    /// </summary>
    public class DemolishBuildingCommand : AbstractCommand
    {
        private string buildingId;
        
        public DemolishBuildingCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            bool success = buildingSystem.DemolishBuilding(buildingId);
            
            if (success)
            {
                Debug.Log($"拆除建筑 ID: {buildingId}");
            }
            else
            {
                Debug.LogWarning($"无法拆除建筑 ID: {buildingId}");
            }
        }
    }
    
    /// <summary>
    /// 修复建筑命令
    /// </summary>
    public class RepairBuildingCommand : AbstractCommand
    {
        private string buildingId;
        
        public RepairBuildingCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            bool success = buildingSystem.RepairBuilding(buildingId);
            
            if (success)
            {
                Debug.Log($"修复建筑 ID: {buildingId}");
            }
            else
            {
                Debug.LogWarning($"无法修复建筑 ID: {buildingId}");
            }
        }
    }
    
    /// <summary>
    /// 切换建筑操作状态命令
    /// </summary>
    public class ToggleBuildingOperationCommand : AbstractCommand
    {
        private string buildingId;
        
        public ToggleBuildingOperationCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            bool success = buildingSystem.ToggleBuildingOperation(buildingId);
            
            if (success)
            {
                var building = buildingSystem.GetBuilding(buildingId);
                bool isOperational = buildingSystem.IsBuildingOperational(buildingId);
                string status = isOperational ? "启用" : "停用";
                Debug.Log($"建筑 {building?.ConfigId} 已{status}");
            }
            else
            {
                Debug.LogWarning($"无法切换建筑 ID: {buildingId} 的操作状态");
            }
        }
    }
    
    /// <summary>
    /// 分配工人到建筑命令
    /// </summary>
    public class AssignWorkersToBuildingCommand : AbstractCommand
    {
        private string buildingId;
        private string workerId;
        
        public AssignWorkersToBuildingCommand(string buildingId, string workerId)
        {
            this.buildingId = buildingId;
            this.workerId = workerId;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            buildingSystem.AssignWorkersToBuilding(buildingId, workerId);
            
            var building = buildingSystem.GetBuilding(buildingId);
            Debug.Log($"为建筑 {building?.ConfigId} 分配了ID为  {workerId} 的工人");
        }
    }
    
    /// <summary>
    /// 收集建筑产出命令
    /// </summary>
    public class CollectBuildingOutputCommand : AbstractCommand
    {
        private string buildingId;
        
        public CollectBuildingOutputCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            buildingSystem.CollectBuildingOutput(buildingId);
            
            var building = buildingSystem.GetBuilding(buildingId);
            Debug.Log($"收集建筑 {building?.ConfigId} 的产出");
        }
    }
    
    /// <summary>
    /// 取消建造命令
    /// </summary>
    public class CancelConstructionCommand : AbstractCommand
    {
        private string buildingId;
        
        public CancelConstructionCommand(string buildingId)
        {
            this.buildingId = buildingId;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            bool success = buildingSystem.CancelConstruction(buildingId);
            
            if (success)
            {
                Debug.Log($"取消建造建筑 ID: {buildingId}");
            }
            else
            {
                Debug.LogWarning($"无法取消建造建筑 ID: {buildingId}");
            }
        }
    }
    
    /// <summary>
    /// 快速建造命令（调试用）
    /// </summary>
    public class QuickBuildCommand : AbstractCommand
    {
        private Vector3 position;
        private string buildingType;
        
        public QuickBuildCommand(Vector3 position, string buildingType)
        {
            this.position = position;
            this.buildingType = buildingType;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            var gameModel = this.GetModel<ISurvivalGameModel>();
            
            // 添加足够的资源
            gameModel.Materials.Value += 1000;
            
            // 开始建造
            if (buildingSystem.StartConstruction(position, buildingType))
            {
                // 立即完成建造
                var buildings = buildingSystem.GetAllBuildings();
                var latestBuilding = buildings.OrderByDescending(b => b.Id).FirstOrDefault();
                if (latestBuilding != null)
                {
                    buildingSystem.CompleteConstruction(latestBuilding.Id);
                    Debug.Log($"快速建造完成: {buildingType} 在位置 {position}");
                }
            }
            else
            {
                Debug.LogWarning($"快速建造失败: {buildingType}");
            }
        }
    }
    
    /// <summary>
    /// 批量建造命令（调试用）
    /// </summary>
    public class BatchBuildCommand : AbstractCommand
    {
        private string buildingType;
        private int count;
        
        public BatchBuildCommand(string buildingType, int count)
        {
            this.buildingType = buildingType;
            this.count = count;
        }
        
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            var gameModel = this.GetModel<ISurvivalGameModel>();
            
            // 添加足够的资源
            gameModel.Materials.Value += count * 100;
            
            int built = 0;
            for (int i = 0; i < count; i++)
            {
                Vector3 position = buildingSystem.GetNextBuildPosition(buildingType);
                
                if (buildingSystem.StartConstruction(position, buildingType))
                {
                    // 立即完成建造
                    var buildings = buildingSystem.GetAllBuildings();
                    var latestBuilding = buildings.OrderByDescending(b => b.Id).FirstOrDefault();
                    if (latestBuilding != null)
                    {
                        buildingSystem.CompleteConstruction(latestBuilding.Id);
                        built++;
                    }
                }
                else
                {
                    break; // 无法继续建造
                }
            }
            
            Debug.Log($"批量建造完成: {built}/{count} 个 {buildingType}");
        }
    }
    
    /// <summary>
    /// 修复所有建筑命令
    /// </summary>
    public class RepairAllBuildingsCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            var gameModel = this.GetModel<ISurvivalGameModel>();
            
            // 添加足够的修复材料
            gameModel.Materials.Value += 500;
            
            var buildings = buildingSystem.GetAllBuildings();
            int repaired = 0;
            
            foreach (var building in buildings)
            {
                if (building.Health < 100f)
                {
                    if (buildingSystem.RepairBuilding(building.Id))
                    {
                        repaired++;
                    }
                }
            }
            
            Debug.Log($"修复了 {repaired} 座建筑");
        }
    }
    
    /// <summary>
    /// 升级所有建筑命令
    /// </summary>
    public class UpgradeAllBuildingsCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            var gameModel = this.GetModel<ISurvivalGameModel>();
            
            // 添加足够的升级材料
            gameModel.Materials.Value += 2000;
            
            var buildings = buildingSystem.GetAllBuildings();
            int upgraded = 0;
            
            foreach (var building in buildings)
            {
                if (buildingSystem.UpgradeBuilding(building.Id))
                {
                    upgraded++;
                }
            }
            
            Debug.Log($"升级了 {upgraded} 座建筑");
        }
    }
} 