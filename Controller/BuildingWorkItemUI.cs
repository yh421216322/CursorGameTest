// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：BuildingWorkItemUI.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了建筑工作项的UI控制器 (BuildingWorkItemUI)。
//     该组件用于在工作分配面板（或其他类似UI）中展示单个可分配工作的建筑信息，
//     并处理用户与该建筑项的交互，如点击分配按钮。
// ==============================================================================

using UnityEngine;
using UnityEngine.UI;
using QFramework;
using SurvivalGame.Model; // 包含 BuildingData, SurvivorData, BuildingConfig, BuildingCategory 等模型
using System; // 用于 System.Action

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 建筑工作项UI组件控制器。
    /// 负责在UI上显示一个建筑的相关信息（名称、类型、效率、工人数、描述），
    /// 并提供一个“分配”按钮，允许用户将当前选中的幸存者分配到此建筑工作。
    /// </summary>
    public class BuildingWorkItemUI : BaseUIController // 继承自包含UI查找功能的基类
    {
        [Header("UI组件引用")] // 在Unity Inspector中方便组织和识别
        public Text buildingNameText;      // 显示建筑名称的Text组件
        public Text buildingTypeText;      // 显示建筑类型的Text组件
        public Text efficiencyText;       // 显示建筑当前工作效率的Text组件
        public Text workerCountText;      // 显示建筑当前工人数/最大工人数的Text组件
        public Button assignButton;       // “分配”按钮组件
        public Text buildingDescText;     // 显示建筑描述信息的Text组件

        // 内部数据存储
        private BuildingData mBuildingData; // 当前UI项代表的建筑数据
        private SurvivorData mSurvivorData; // 当前待分配或相关的幸存者数据

        /// <summary>
        /// 当用户点击“分配”按钮时触发的事件。
        /// 参数1 (string): 幸存者ID。
        /// 参数2 (string): 建筑ID。
        /// </summary>
        public System.Action<string, string> OnAssignClicked;

        /// <summary>
        /// Unity生命周期方法：在对象加载时调用，早于Start。
        /// 用于初始化操作，此处用于查找UI组件。
        /// </summary>
        private void Awake()
        {
            FindUIComponents(); // 初始化时查找必要的UI子组件
        }

        /// <summary>
        /// Unity生命周期方法：在对象首次启用且Awake执行完毕后调用。
        /// 用于设置事件监听等。
        /// </summary>
        private void Start()
        {
            // 为分配按钮注册点击事件监听器
            if (assignButton != null)
                assignButton.onClick.AddListener(OnAssignButtonClicked);
        }

        /// <summary>
        /// 查找并赋值UI子组件引用。
        /// 如果在Inspector中未手动拖拽赋值，则尝试通过名称查找。
        /// 注意：此方法使用 transform.Find，适用于查找直接子对象。
        /// 如果UI结构更复杂，可以考虑使用基类提供的 FindUIComponent<T>(path) 方法。
        /// </summary>
        private void FindUIComponents()
        {
            // 逐个检查并查找UI组件
            if (buildingNameText == null) buildingNameText = transform.Find("BuildingNameText")?.GetComponent<Text>();
            if (buildingTypeText == null) buildingTypeText = transform.Find("BuildingTypeText")?.GetComponent<Text>();
            if (efficiencyText == null) efficiencyText = transform.Find("EfficiencyText")?.GetComponent<Text>();
            if (workerCountText == null) workerCountText = transform.Find("WorkerCountText")?.GetComponent<Text>();
            if (assignButton == null) assignButton = transform.Find("AssignButton")?.GetComponent<Button>();
            if (buildingDescText == null) buildingDescText = transform.Find("BuildingDescText")?.GetComponent<Text>();
            
            // 建议：添加对查找失败的警告日志，以便调试
            // if (buildingNameText == null) Debug.LogWarning("[BuildingWorkItemUI] BuildingNameText 未找到！");
            // ... 对其他组件也进行类似检查
        }

        /// <summary>
        /// 设置并更新UI项显示的建筑数据及相关信息。
        /// </summary>
        /// <param name="building">要显示的建筑数据。</param>
        /// <param name="survivor">当前操作相关的幸存者数据。</param>
        /// <param name="config">建筑的配置信息（如名称、描述、类型等）。</param>
        /// <param name="efficiency">建筑的当前工作效率。</param>
        /// <param name="currentWorkers">建筑当前的工人数。</param>
        /// <param name="maxWorkers">建筑允许的最大工人数。</param>
        public void SetBuildingData(BuildingData building, SurvivorData survivor, BuildingConfig config, float efficiency, int currentWorkers, int maxWorkers)
        {
            mBuildingData = building; // 存储建筑数据
            mSurvivorData = survivor; // 存储幸存者数据

            UpdateDisplay(config, efficiency, currentWorkers, maxWorkers); // 调用方法更新UI显示
        }

        /// <summary>
        /// 根据传入的数据更新UI元素的显示内容。
        /// </summary>
        private void UpdateDisplay(BuildingConfig config, float efficiency, int currentWorkers, int maxWorkers)
        {
            if (buildingNameText != null)
                buildingNameText.text = config.Name; // 设置建筑名称

            if (buildingTypeText != null)
                buildingTypeText.text = GetBuildingCategoryDisplayName(config.Category); // 设置建筑类型（本地化显示）

            if (efficiencyText != null)
                efficiencyText.text = $"效率: {efficiency:P0}"; // 设置效率，格式化为百分比

            if (workerCountText != null)
                workerCountText.text = $"工人: {currentWorkers}/{maxWorkers}"; // 设置工人数

            if (buildingDescText != null)
                buildingDescText.text = config.Description; // 设置建筑描述

            // 更新“分配”按钮的状态和文本
            if (assignButton != null)
            {
                bool canAssign = currentWorkers < maxWorkers; // 判断是否还能分配工人
                assignButton.interactable = canAssign;        // 如果已满员，则按钮不可交互
                
                var buttonText = assignButton.GetComponentInChildren<Text>(); // 获取按钮上的文本组件
                if (buttonText != null)
                {
                    buttonText.text = canAssign ? "分配" : "已满"; // 根据是否可分配更新按钮文本
                }
            }
        }

        /// <summary>
        /// 当“分配”按钮被点击时调用的处理方法。
        /// </summary>
        private void OnAssignButtonClicked()
        {
            // 确保建筑数据和幸存者数据都存在，然后触发OnAssignClicked事件
            if (mBuildingData != null && mSurvivorData != null)
            {
                // 触发事件，传递幸存者ID和建筑ID（将建筑ID转为字符串）
                OnAssignClicked?.Invoke(mSurvivorData.Id, mBuildingData.Id.ToString());
                // Debug.Log($"[BuildingWorkItemUI] 分配按钮点击：幸存者 {mSurvivorData.Id} -> 建筑 {mBuildingData.Id}");
            }
            else
            {
                Debug.LogWarning("[BuildingWorkItemUI] 分配按钮点击，但建筑或幸存者数据为空！");
            }
        }

        /// <summary>
        /// 根据建筑类别枚举获取本地化的显示名称。
        /// </summary>
        /// <param name="category">建筑类别枚举值。</param>
        /// <returns>对应类别的中文字符串。</returns>
        private string GetBuildingCategoryDisplayName(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Production: return "生产类";
                case BuildingCategory.Defense:    return "防御类";
                case BuildingCategory.Habitat:    return "居住类";
                case BuildingCategory.Storage:    return "存储类";
                case BuildingCategory.Functional: return "功能类";
                default: return "其他"; // 未知或其他类型的默认显示
            }
        }

        /// <summary>
        /// (静态方法示例) 获取指定建筑ID对应的职业要求描述文本。
        /// 注意：这种基于硬编码ID的方式耦合度较高，实际项目中可能通过配置数据驱动。
        /// </summary>
        /// <param name="buildingId">建筑的唯一ID。</param>
        /// <returns>描述该建筑适合哪些职业的字符串。</returns>
        public static string GetBuildingProfessionRequirement(string buildingId)
        {
            // 示例：根据建筑ID返回不同的职业适应性描述
            switch (buildingId)
            {
                case "Farm":           return "适合：平民、工人";
                case "Workshop":       return "适合：工程师、工人";
                case "MedicalStation": return "适合：医生";
                case "WatchTower":     return "适合：士兵、侦察员、守卫";
                case "Library":        return "适合：科学家";
                case "Quarry":         return "适合：工人、工程师";
                default:               return "适合：平民"; // 默认适合平民
            }
        }
    }
}