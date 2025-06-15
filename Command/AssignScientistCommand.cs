using UnityEngine;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 分配科学家命令
    /// </summary>
    public class AssignScientistCommand : AbstractCommand
    {
        public int Count { get; set; }
        
        protected override void OnExecute()
        {
            Debug.Log($"分配科学家数量: {Count}");
            
            // 发送科学家分配事件
            this.SendEvent(new ScientistAssignedEvent
            {
                RequestedCount = Count,
                AssignedCount = Count
            });
        }
    }
} 