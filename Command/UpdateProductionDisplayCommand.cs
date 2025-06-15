using QFramework;
using MyGameNamespace;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 更新生产效率显示命令
    /// 当幸存者分配工作后，通知UI系统更新生产效率显示
    /// </summary>
    public class UpdateProductionDisplayCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            // 发送生产效率更新事件
            this.SendEvent(new ProductionEfficiencyUpdatedEvent
            {
                UpdateTime = UnityEngine.Time.time
            });
        }
    }
}

// 事件定义
namespace MyGameNamespace
{
    /// <summary>
    /// 生产效率更新事件
    /// </summary>
    public struct ProductionEfficiencyUpdatedEvent
    {
        public float UpdateTime;
    }
} 