using System.Collections.Generic;
using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Model;

namespace SurvivalGame.Command
{
    // /// <summary>
    // /// 开始研究科技命令
    // /// </summary>
    // public class StartResearchCommand : AbstractCommand
    // {
    //     public string TechId { get; set; }
    //     
    //     protected override void OnExecute()
    //     {
    //         var techSystem = this.GetSystem<IAdvancedTechSystem>();
    //         bool success = techSystem.StartResearch(TechId);
    //         
    //         if (success)
    //         {
    //             Debug.Log($"开始研究科技: {TechId}");
    //         }
    //         else
    //         {
    //             Debug.LogWarning($"无法开始研究科技: {TechId}");
    //         }
    //     }
    // }

    /// <summary>
    /// 暂停研究命令
    /// </summary>
    public class PauseResearchCommand : AbstractCommand
    {
        public string TechId { get; set; }
        
        protected override void OnExecute()
        {
            Debug.Log($"暂停研究科技: {TechId}");
        }
    }

    /// <summary>
    /// 取消研究命令
    /// </summary>
    public class CancelResearchCommand : AbstractCommand
    {
        public string TechId { get; set; }
        
        protected override void OnExecute()
        {
            Debug.Log($"取消研究科技: {TechId}");
        }
    }

    /// <summary>
    /// 快速完成研究命令（调试用）
    /// </summary>
    public class FastCompleteResearchCommand : AbstractCommand
    {
        public string TechId { get; set; }
        
        protected override void OnExecute()
        {
            var techSystem = this.GetSystem<IAdvancedTechSystem>();
            techSystem.CompleteResearch(TechId);
            
            Debug.Log($"快速完成研究科技: {TechId}");
        }
    }

    /// <summary>
    /// 完成研究命令（调试用）
    /// </summary>
    public class CompleteResearchCommand : AbstractCommand
    {
        public string TechId { get; set; }
        
        protected override void OnExecute()
        {
            var techSystem = this.GetSystem<IAdvancedTechSystem>();
            techSystem.CompleteResearch(TechId);
            
            Debug.Log($"完成研究科技: {TechId}");
        }
    }

    /// <summary>
    /// 添加研究点数命令（调试用）
    /// </summary>
    public class AddResearchPointsCommand : AbstractCommand
    {
        public float Points { get; set; }
        
        protected override void OnExecute()
        {
            var techSystem = this.GetSystem<IAdvancedTechSystem>();
            techSystem.AddResearchPoints(Points);
            
            Debug.Log($"添加研究点数: {Points}");
        }
    }

    /// <summary>
    /// 设置研究效率命令
    /// </summary>
    public class SetResearchEfficiencyCommand : AbstractCommand
    {
        public float Efficiency { get; set; }
        
        protected override void OnExecute()
        {
            Debug.Log($"设置研究效率: {Efficiency:P0}");
        }
    }

    /// <summary>
    /// 解锁所有科技命令（调试用）
    /// </summary>
    public class UnlockAllTechsCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            Debug.Log("解锁所有科技");
        }
    }

    /// <summary>
    /// 重置科技树命令（调试用）
    /// </summary>
    public class ResetTechTreeCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            Debug.Log("重置科技树");
        }
    }

    /// <summary>
    /// 批量研究命令（调试用）
    /// </summary>
    public class BatchResearchCommand : AbstractCommand
    {
        public List<string> TechIds { get; set; }
        
        protected override void OnExecute()
        {
            var techSystem = this.GetSystem<IAdvancedTechSystem>();
            
            if (TechIds != null)
            {
                foreach (string techId in TechIds)
                {
                    techSystem.StartResearch(techId);
                }
                
                Debug.Log($"批量开始研究 {TechIds.Count} 个科技");
            }
        }
    }

    /// <summary>
    /// 获取科技信息命令
    /// </summary>
    public class GetTechInfoCommand : AbstractCommand
    {
        public string TechId { get; set; }
        public object Result { get; private set; }
        
        protected override void OnExecute()
        {
            Debug.Log($"获取科技信息: {TechId}");
            Result = null;
        }
    }

    /// <summary>
    /// 获取研究状态命令
    /// </summary>
    public class GetResearchStatusCommand : AbstractCommand
    {
        public object Result { get; private set; }
        
        protected override void OnExecute()
        {
            Result = null;
            Debug.Log("获取研究状态");
        }
    }

    /// <summary>
    /// 设置研究优先级命令
    /// </summary>
    public class SetResearchPriorityCommand : AbstractCommand
    {
        public string TechId { get; set; }
        public int Priority { get; set; }
        
        protected override void OnExecute()
        {
            Debug.Log($"设置科技研究优先级: {TechId} -> {Priority}");
        }
    }

    /// <summary>
    /// 自动研究命令
    /// </summary>
    public class AutoResearchCommand : AbstractCommand
    {
        public bool Enable { get; set; }
        public string Category { get; set; } = "";
        
        protected override void OnExecute()
        {
            string categoryText = string.IsNullOrEmpty(Category) ? "全部" : Category;
            Debug.Log($"{(Enable ? "启用" : "禁用")}自动研究 - 类别: {categoryText}");
        }
    }

    /// <summary>
    /// 获取可研究科技列表命令
    /// </summary>
    public class GetAvailableTechsCommand : AbstractCommand
    {
        public List<string> Result { get; private set; }
        
        protected override void OnExecute()
        {
            Result = new List<string>();
            Debug.Log("获取可研究科技列表");
        }
    }

    /// <summary>
    /// 获取已完成科技列表命令
    /// </summary>
    public class GetCompletedTechsCommand : AbstractCommand
    {
        public List<string> Result { get; private set; }
        
        protected override void OnExecute()
        {
            Result = new List<string>();
            Debug.Log("获取已完成科技列表");
        }
    }

    /// <summary>
    /// 导出科技数据命令（调试用）
    /// </summary>
    public class ExportTechDataCommand : AbstractCommand
    {
        public string FilePath { get; set; }
        
        protected override void OnExecute()
        {
            Debug.Log($"导出科技数据到: {FilePath}");
        }
    }

    /// <summary>
    /// 导入科技数据命令（调试用）
    /// </summary>
    public class ImportTechDataCommand : AbstractCommand
    {
        public string FilePath { get; set; }
        
        protected override void OnExecute()
        {
            Debug.Log($"从文件导入科技数据: {FilePath}");
        }
    }
} 