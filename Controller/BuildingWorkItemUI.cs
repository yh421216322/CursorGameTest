using UnityEngine;
using UnityEngine.UI;
using QFramework;
using SurvivalGame.Model;
using System;

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 建筑工作项UI组件
    /// 用于在工作分配面板中显示可分配的建筑信息
    /// </summary>
    public class BuildingWorkItemUI : BaseUIController
    {
        [Header("UI组件")]
        public Text buildingNameText;      // 建筑名称
        public Text buildingTypeText;      // 建筑类型
        public Text efficiencyText;       // 建筑效率
        public Text workerCountText;      // 当前工人数
        public Button assignButton;       // 分配按钮
        public Text buildingDescText;     // 建筑描述

        private BuildingData mBuildingData;
        private SurvivorData mSurvivorData;
        public System.Action<string, string> OnAssignClicked; // survivorId, buildingId

        private void Awake()
        {
            FindUIComponents();
        }

        private void Start()
        {
            if (assignButton != null)
                assignButton.onClick.AddListener(OnAssignButtonClicked);
        }

        private void FindUIComponents()
        {
            if (buildingNameText == null) buildingNameText = transform.Find("BuildingNameText").GetComponent<Text>();
            
            if (buildingTypeText == null) buildingTypeText = transform.Find("BuildingTypeText").GetComponent<Text>();
            if (efficiencyText == null) efficiencyText = transform.Find("EfficiencyText").GetComponent<Text>();
            if (workerCountText == null) workerCountText = transform.Find("WorkerCountText").GetComponent<Text>();
            if (assignButton == null) assignButton = transform.Find("AssignButton").GetComponent<Button>();
            if (buildingDescText == null) buildingDescText = transform.Find("BuildingDescText").GetComponent<Text>();
            
        }

        public void SetBuildingData(BuildingData building, SurvivorData survivor, BuildingConfig config, float efficiency, int currentWorkers, int maxWorkers)
        {
            mBuildingData = building;
            mSurvivorData = survivor;

            UpdateDisplay(config, efficiency, currentWorkers, maxWorkers);
        }

        private void UpdateDisplay(BuildingConfig config, float efficiency, int currentWorkers, int maxWorkers)
        {
            if (buildingNameText != null)
                buildingNameText.text = config.Name;

            if (buildingTypeText != null)
                buildingTypeText.text = GetBuildingCategoryDisplayName(config.Category);

            if (efficiencyText != null)
                efficiencyText.text = $"效率: {efficiency:P0}";

            if (workerCountText != null)
                workerCountText.text = $"工人: {currentWorkers}/{maxWorkers}";

            if (buildingDescText != null)
                buildingDescText.text = config.Description;

            // 设置分配按钮状态
            if (assignButton != null)
            {
                bool canAssign = currentWorkers < maxWorkers;
                assignButton.interactable = canAssign;
                
                var buttonText = assignButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = canAssign ? "分配" : "已满";
                }
            }
        }

        private void OnAssignButtonClicked()
        {
            if (mBuildingData != null && mSurvivorData != null)
            {
                OnAssignClicked?.Invoke(mSurvivorData.Id, mBuildingData.Id.ToString());
            }
        }

        private string GetBuildingCategoryDisplayName(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Production: return "生产类";
                case BuildingCategory.Defense: return "防御类";
                case BuildingCategory.Habitat: return "居住类";
                case BuildingCategory.Storage: return "存储类";
                case BuildingCategory.Functional: return "功能类";
                default: return "其他";
            }
        }

        /// <summary>
        /// 获取建筑职业要求描述
        /// </summary>
        public static string GetBuildingProfessionRequirement(string buildingId)
        {
            switch (buildingId)
            {
                case "Farm": return "适合：平民、工人";
                case "Workshop": return "适合：工程师、工人";
                case "MedicalStation": return "适合：医生";
                case "WatchTower": return "适合：士兵、侦察员、守卫";
                case "Library": return "适合：科学家";
                case "Quarry": return "适合：工人、工程师";
                default: return "适合：平民";
            }
        }
    }
} 