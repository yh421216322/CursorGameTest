using System.Collections.Generic;
using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.GameSystem;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 开始研究科技命令
    /// </summary>
    public class StartResearchCommand : AbstractCommand
    {
        private string _techId;
        private string _researchBuildingId;
        private List<string> _researcherIds;
        
        public StartResearchCommand(string techId, string researchBuildingId, List<string> researcherIds = null)
        {
            _techId = techId;
            _researchBuildingId = researchBuildingId;
            _researcherIds = researcherIds;
        }
        
        protected override void OnExecute()
        {
            var techSystem = this.GetSystem<TechSystem>();
            bool success = techSystem.StartResearch(_techId, _researchBuildingId, _researcherIds);
            
            if (success)
            {
                Debug.Log($"开始研究科技: {_techId}");
            }
            else
            {
                Debug.LogWarning($"无法开始研究科技: {_techId}");
            }
        }
    }
} 