// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：AssignScientistCommand.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件包含分配科学家的命令。
// ==============================================================================

using UnityEngine;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 分配科学家命令
    /// 用于处理分配科学家的逻辑
    /// </summary>
    public class AssignScientistCommand : AbstractCommand
    {
        // 要分配的科学家数量
        public int Count { get; set; }
        
        // 执行命令
        protected override void OnExecute()
        {
            // 打印分配的科学家数量到控制台，用于调试
            Debug.Log($"分配科学家数量: {Count}");
            
            // 发送科学家分配事件
            // 此事件将通知其他系统科学家已被分配
            this.SendEvent(new ScientistAssignedEvent
            {
                RequestedCount = Count, // 请求分配的数量
                AssignedCount = Count   // 实际分配的数量 (在此示例中与请求数量相同)
            });
        }
    }
}