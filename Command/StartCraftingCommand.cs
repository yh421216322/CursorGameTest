// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：StartCraftingCommand.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了开始物品制造过程的命令。
//     它包含了执行制造任务所需的所有参数，如配方ID、建筑物ID、数量和制造者ID。
// ==============================================================================

using System.Collections.Generic;
using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem; // 假设CraftingSystem在SurvivalGame.GameSystem命名空间下

namespace SurvivalGame.Command
{
    /// <summary>
    /// 开始制造命令
    /// 用于向制造系统请求开始一个新的制造任务。
    /// </summary>
    public class StartCraftingCommand : AbstractCommand
    {
        // 要制造的配方ID
        private string _recipeId;
        // 在哪个建筑物中进行制造 (例如：工作台、熔炉等)
        private string _buildingId;
        // 要制造的数量
        private int _quantity;
        // 参与制造的角色或单位的ID列表 (可选)
        private List<string> _crafterIds;
        
        /// <summary>
        /// StartCraftingCommand 构造函数
        /// </summary>
        /// <param name="recipeId">要制造的物品的配方ID</param>
        /// <param name="buildingId">执行制造的建筑物的ID</param>
        /// <param name="quantity">要制造的数量，默认为1</param>
        /// <param name="crafterIds">参与制造过程的角色ID列表，默认为null (可能表示无人或系统自动分配)</param>
        public StartCraftingCommand(string recipeId, string buildingId, int quantity = 1, List<string> crafterIds = null)
        {
            _recipeId = recipeId;
            _buildingId = buildingId;
            _quantity = quantity;
            _crafterIds = crafterIds; // 如果为null，表示没有特定的制造者或由系统处理
        }
        
        /// <summary>
        /// 执行开始制造命令的逻辑
        /// </summary>
        protected override void OnExecute()
        {
            // 获取制造系统实例
            var craftingSystem = this.GetSystem<CraftingSystem>();

            // 调用制造系统的核心方法来尝试开始制造过程
            // 参数包括：配方ID，建筑物ID，数量，以及可选的制造者列表
            bool success = craftingSystem.StartCrafting(_recipeId, _buildingId, _quantity, _crafterIds);
            
            // 根据制造是否成功启动来记录不同的日志信息
            if (success)
            {
                // 成功日志，包含配方ID和数量
                Debug.Log($"成功请求开始制造: 配方 '{_recipeId}', 数量 x{_quantity}, 建筑ID '{_buildingId}'.");
            }
            else
            {
                // 失败日志，指明哪个配方无法开始制造
                // 失败原因可能包括：资源不足、建筑物无效、配方不存在、制造队列已满等，具体由CraftingSystem内部逻辑判断
                Debug.LogWarning($"无法开始制造: 配方 '{_recipeId}'. 请检查资源、建筑状态及配方有效性。");
            }
        }
    }
}