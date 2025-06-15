// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：StartResearchCommand.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了开始科技研究的命令。
//     该命令包含了执行研究任务所需的参数，如科技ID、研究建筑物ID以及可选的研究员ID列表。
// ==============================================================================

using System.Collections.Generic;
using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem; // 假设TechSystem在SurvivalGame.GameSystem命名空间下

namespace SurvivalGame.Command
{
    /// <summary>
    /// 开始研究科技命令
    /// 用于向科技系统请求开始一个新的科技研究项目。
    /// </summary>
    public class StartResearchCommand : AbstractCommand
    {
        // 要研究的科技的唯一标识符
        private string _techId;
        // 执行研究的建筑物的ID (例如：研究室、科技中心)
        private string _researchBuildingId;
        // 参与研究的研究员ID列表 (可选)
        private List<string> _researcherIds;
        
        /// <summary>
        /// StartResearchCommand 构造函数
        /// </summary>
        /// <param name="techId">要研究的科技的ID</param>
        /// <param name="researchBuildingId">进行研究的建筑物的ID</param>
        /// <param name="researcherIds">参与研究的研究员ID列表，默认为null (可能表示无人或系统自动分配)</param>
        public StartResearchCommand(string techId, string researchBuildingId, List<string> researcherIds = null)
        {
            _techId = techId;
            _researchBuildingId = researchBuildingId;
            _researcherIds = researcherIds; // 如果为null，表示没有特定的研究员或由系统处理
        }
        
        /// <summary>
        /// 执行开始研究命令的逻辑
        /// </summary>
        protected override void OnExecute()
        {
            // 获取科技系统实例
            var techSystem = this.GetSystem<TechSystem>();

            // 调用科技系统的核心方法来尝试开始研究过程
            // 参数包括：科技ID，研究建筑物ID，以及可选的研究员列表
            bool success = techSystem.StartResearch(_techId, _researchBuildingId, _researcherIds);
            
            // 根据研究是否成功启动来记录不同的日志信息
            if (success)
            {
                // 成功日志，包含科技ID
                Debug.Log($"成功请求开始研究科技: '{_techId}' 在建筑 '{_researchBuildingId}'.");
            }
            else
            {
                // 失败日志，指明哪个科技无法开始研究
                // 失败原因可能包括：前置科技未解锁、资源不足、研究建筑无效、研究队列已满等，具体由TechSystem内部逻辑判断
                Debug.LogWarning($"无法开始研究科技: '{_techId}'. 请检查前置条件、资源及研究建筑状态。");
            }
        }
    }
}