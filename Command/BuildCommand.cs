using QFramework;
using MyGameNamespace;
using SurvivalGame;
using SurvivalGame.GameSystem;
using UnityEngine;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 功能：建造建筑的Command
    /// 调用方式：this.SendCommand(new BuildCommand(position, buildingId))
    /// </summary>
    public class BuildCommand : AbstractCommand
    {
        private Vector3 mPosition;
        private string mBuildingId;
        private IEnhancedBuildingSystem mBuildingSystem;
        
        public BuildCommand(Vector3 position, string buildingId)
        {
            mPosition = position;
            mBuildingId = buildingId;
        }
        
        protected override void OnExecute()
        {
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            
            // 先检查是否可以建造，然后开始建造
            if (mBuildingSystem.CanBuildAt(mPosition, mBuildingId) && 
                mBuildingSystem.StartConstruction(mPosition, mBuildingId))
            {
                // 发送建造成功事件
                this.SendEvent(new BuildCommandSuccessEvent
                {
                    Position = mPosition,
                    BuildingId = mBuildingId
                });
                
                UnityEngine.Debug.Log($"建造指令执行成功：{mBuildingId} 在 {mPosition}");
            }
            else
            {
                // 发送建造失败事件
                this.SendEvent(new BuildCommandFailedEvent
                {
                    Position = mPosition,
                    BuildingId = mBuildingId,
                    Reason = "资源不足或位置不可用"
                });
                
                UnityEngine.Debug.Log($"建造指令执行失败：{mBuildingId} 在 {mPosition}");
            }
        }
    }
}

// Command相关事件定义
namespace MyGameNamespace
{
    public struct BuildCommandSuccessEvent
    {
        public Vector3 Position;
        public string BuildingId;
    }
    
    public struct BuildCommandFailedEvent
    {
        public Vector3 Position;
        public string BuildingId;
        public string Reason;
    }
    
    public struct UpgradeBuildingSuccessEvent
    {
        public int BuildingId;
    }
    
    public struct UpgradeBuildingFailedEvent
    {
        public int BuildingId;
        public string Reason;
    }
    
    public struct DemolishBuildingSuccessEvent
    {
        public int BuildingId;
    }
    
    public struct DemolishBuildingFailedEvent
    {
        public int BuildingId;
        public string Reason;
    }
    
    public struct RepairBuildingEvent
    {
        public int BuildingId;
    }
} 