// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：BuildCommand.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件包含用于处理建筑建造逻辑的命令。
// ==============================================================================

using QFramework;
using MyGameNamespace; // 假设这是包含事件定义的命名空间
using SurvivalGame;
using SurvivalGame.GameSystem; // 假设这是建筑系统的命名空间
using UnityEngine;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 功能：建造建筑的Command (命令模式中的命令对象)
    /// 调用方式：this.SendCommand(new BuildCommand(position, buildingId))
    /// 描述：当需要建造一个新建筑时，创建并发送此命令。
    /// </summary>
    public class BuildCommand : AbstractCommand
    {
        // 建筑目标位置
        private Vector3 mPosition;
        // 要建造的建筑ID
        private string mBuildingId;
        // 建筑系统接口，用于与建筑逻辑交互
        private IEnhancedBuildingSystem mBuildingSystem;
        
        /// <summary>
        /// BuildCommand 构造函数
        /// </summary>
        /// <param name="position">建筑在世界中的位置</param>
        /// <param name="buildingId">要建造的建筑类型的唯一标识符</param>
        public BuildCommand(Vector3 position, string buildingId)
        {
            mPosition = position;
            mBuildingId = buildingId;
        }
        
        /// <summary>
        /// 执行建造命令的核心逻辑
        /// </summary>
        protected override void OnExecute()
        {
            // 获取建筑系统实例
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            
            // 核心逻辑：首先检查是否可以在指定位置建造指定建筑，然后尝试开始建造
            if (mBuildingSystem.CanBuildAt(mPosition, mBuildingId) && 
                mBuildingSystem.StartConstruction(mPosition, mBuildingId))
            {
                // 如果建造条件满足且建造成功启动
                // 发送建造成功事件，通知其他系统或UI
                this.SendEvent(new BuildCommandSuccessEvent
                {
                    Position = mPosition,    // 成功建造的建筑位置
                    BuildingId = mBuildingId // 成功建造的建筑ID
                });
                
                // 在Unity控制台记录成功日志，便于调试
                UnityEngine.Debug.Log($"建造指令执行成功：{mBuildingId} 在 {mPosition}");
            }
            else
            {
                // 如果建造条件不满足或建造未能启动
                // 发送建造失败事件，并提供原因
                this.SendEvent(new BuildCommandFailedEvent
                {
                    Position = mPosition,    // 尝试建造的建筑位置
                    BuildingId = mBuildingId, // 尝试建造的建筑ID
                    Reason = "资源不足或位置不可用" // 建造失败的原因 (这里是通用原因，可以更具体)
                });
                
                // 在Unity控制台记录失败日志，便于调试
                UnityEngine.Debug.Log($"建造指令执行失败：{mBuildingId} 在 {mPosition}");
            }
        }
    }
}

// Command相关事件定义 (这些事件用于命令执行后通知系统的其他部分)
namespace MyGameNamespace // 注意：通常事件会定义在更通用的命名空间或专门的事件命名空间中
{
    /// <summary>
    /// 建筑建造成功事件
    /// </summary>
    public struct BuildCommandSuccessEvent
    {
        public Vector3 Position;  // 成功建造的建筑位置
        public string BuildingId; // 成功建造的建筑ID
    }
    
    /// <summary>
    /// 建筑建造失败事件
    /// </summary>
    public struct BuildCommandFailedEvent
    {
        public Vector3 Position;  // 尝试建造的建筑位置
        public string BuildingId; // 尝试建造的建筑ID
        public string Reason;     // 建造失败的原因
    }
    
    /// <summary>
    /// 建筑升级成功事件
    /// </summary>
    public struct UpgradeBuildingSuccessEvent
    {
        public int BuildingId; // 成功升级的建筑的唯一标识符 (注意：这里用int，可能与BuildCommand中的string BuildingId不同，需确认一致性)
    }
    
    /// <summary>
    /// 建筑升级失败事件
    /// </summary>
    public struct UpgradeBuildingFailedEvent
    {
        public int BuildingId; // 尝试升级的建筑ID
        public string Reason;     // 升级失败的原因
    }
    
    /// <summary>
    /// 建筑拆除成功事件
    /// </summary>
    public struct DemolishBuildingSuccessEvent
    {
        public int BuildingId; // 成功拆除的建筑ID
    }
    
    /// <summary>
    /// 建筑拆除失败事件
    /// </summary>
    public struct DemolishBuildingFailedEvent
    {
        public int BuildingId; // 尝试拆除的建筑ID
        public string Reason;     // 拆除失败的原因
    }
    
    /// <summary>
    /// 建筑维修事件 (注意：这似乎是一个触发维修的事件，而不是维修成功/失败事件)
    /// </summary>
    public struct RepairBuildingEvent
    {
        public int BuildingId; // 需要维修的建筑ID
    }
}