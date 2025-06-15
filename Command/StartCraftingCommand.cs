using System.Collections.Generic;
using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 开始制造命令
    /// </summary>
    public class StartCraftingCommand : AbstractCommand
    {
        private string _recipeId;
        private string _buildingId;
        private int _quantity;
        private List<string> _crafterIds;
        
        public StartCraftingCommand(string recipeId, string buildingId, int quantity = 1, List<string> crafterIds = null)
        {
            _recipeId = recipeId;
            _buildingId = buildingId;
            _quantity = quantity;
            _crafterIds = crafterIds;
        }
        
        protected override void OnExecute()
        {
            var craftingSystem = this.GetSystem<CraftingSystem>();
            bool success = craftingSystem.StartCrafting(_recipeId, _buildingId, _quantity, _crafterIds);
            
            if (success)
            {
                Debug.Log($"开始制造: {_recipeId} x{_quantity}");
            }
            else
            {
                Debug.LogWarning($"无法开始制造: {_recipeId}");
            }
        }
    }
} 