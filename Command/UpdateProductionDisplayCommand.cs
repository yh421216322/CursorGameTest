// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：UpdateProductionDisplayCommand.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了“更新生产效率显示”命令。
//     该命令用于在特定条件下（如幸存者工作分配变更后）触发UI界面上生产效率相关信息的刷新。
// ==============================================================================

using QFramework;
using MyGameNamespace; // 包含 ProductionEfficiencyUpdatedEvent 事件定义
using UnityEngine;     // 用于访问 UnityEngine.Time.time

namespace SurvivalGame.Command
{
    /// <summary>
    /// 更新生产效率显示命令。
    /// 当例如幸存者被分配到新的工作岗位，或任何可能影响生产效率的因素发生变化时，
    /// 此命令被发送，以通知UI系统（或其他相关系统）刷新生产效率的显示。
    /// </summary>
    public class UpdateProductionDisplayCommand : AbstractCommand
    {
        /// <summary>
        /// 执行命令的核心逻辑。
        /// </summary>
        protected override void OnExecute()
        {
            // 主要操作是发送一个“生产效率已更新”事件。
            // 其他关心此事件的系统（如UI系统）可以订阅此事件并在接收到时执行相应的更新操作。
            this.SendEvent(new ProductionEfficiencyUpdatedEvent
            {
                // 记录事件发生的具体时间，可用于调试或某些时间相关的逻辑。
                UpdateTime = UnityEngine.Time.time
            });

            // 可以在此处添加日志记录，以方便调试。
            // Debug.Log("[UpdateProductionDisplayCommand] 已发送生产效率更新事件。");
        }
    }
}

// 事件定义 (通常建议将事件定义在专门的事件文件或更通用的命名空间中)
namespace MyGameNamespace
{
    /// <summary>
    /// 生产效率更新事件结构体。
    /// 当生产效率相关数据需要刷新显示时，此事件被触发。
    /// </summary>
    public struct ProductionEfficiencyUpdatedEvent
    {
        /// <summary>
        /// 事件触发时的时间戳。
        /// 可以用于追踪更新频率或进行其他时间相关的计算。
        /// </summary>
        public float UpdateTime;
        // 注意：当前事件仅包含时间戳。
        // 根据实际需求，未来可能需要在此事件中添加更具体的生产效率数据，
        // 例如：总效率、特定建筑的效率、影响因素等，以避免接收方再次查询。
    }
}