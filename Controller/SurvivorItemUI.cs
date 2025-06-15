using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using SurvivalGame.Model;
using SurvivalGame.GameSystem;
using DG.Tweening;
using MyGameNamespace;
using Unity.VisualScripting;

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 幸存者列表项UI组件
    /// 可选择性实现，用于更复杂的列表项交互
    /// </summary>
    public class SurvivorItemUI : BaseUIController
    {
        [Header("UI组件")] 
        public Text nameText;
        public Text professionText;
        public Text levelText;
        public Text stateText;
        public Slider healthSlider;
        public Slider moraleSlider;
        public Button itemButton;

        private SurvivorData mSurvivorData;
        public System.Action<string> OnItemClicked;


        private void Awake()
        {
            // 利用基类方法自动查找并关联UI组件
            FindUIComponents();
            
            // 移除旧的初始化代码，新的SurvivorData不需要这些方法
        }

        // 新增方法：自动查找并关联UI组件
        private void FindUIComponents()
        {
            if (nameText == null) nameText = FindUIComponent<Text>("nameText");
            if (professionText == null) professionText = FindUIComponent<Text>("professionText");
            if (levelText == null) levelText = FindUIComponent<Text>("levelText");
            if (stateText == null) stateText = FindUIComponent<Text>("stateText");
            if (healthSlider == null) healthSlider = FindUIComponent<Slider>("healthSlider");
            if (moraleSlider == null) moraleSlider = FindUIComponent<Slider>("moraleSlider");
            if (itemButton == null) itemButton = FindUIComponent<Button>("itemButton");

           // Debug.Log("幸存者列表项UI组件自动关联完成");
        }

        private void Start()
        {
            if (itemButton != null)
                itemButton.onClick.AddListener(OnButtonClicked);
        }

        public void SetSurvivorData(SurvivorData survivor)
        {
            mSurvivorData = survivor;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (mSurvivorData == null) return;

            if (nameText != null) nameText.text = mSurvivorData.Name;
            if (professionText != null) professionText.text = GetJobDisplayName(mSurvivorData.CurrentJob);
            if (levelText != null) levelText.text = $"Lv.{mSurvivorData.Level}";
            if (stateText != null) stateText.text = GetSurvivorStateName(mSurvivorData.State);
            if (healthSlider != null)
                healthSlider.value = mSurvivorData.Health / 100f; // 健康值范围0-100
            if (moraleSlider != null)
                moraleSlider.value = mSurvivorData.Morale / 100f;
        }

        private void OnButtonClicked()
        {
            OnItemClicked?.Invoke(mSurvivorData?.Id);
        }

        private string GetJobDisplayName(SurvivorJob job)
        {
            switch (job)
            {
                case SurvivorJob.Idle: return "空闲";
                case SurvivorJob.Production: return "生产工作";
                case SurvivorJob.Defense: return "防御工作";
                case SurvivorJob.Research: return "研究工作";
                case SurvivorJob.Construction: return "建造工作";
                case SurvivorJob.Medical: return "医疗工作";
                case SurvivorJob.Exploration: return "探索工作";
                case SurvivorJob.Leadership: return "领导工作";
                default: return job.ToString();
            }
        }

        public static string GetSurvivorStateName(SurvivorState state)
        {
            switch (state)
            {
                case SurvivorState.Idle: return "空闲";
                case SurvivorState.Working: return "工作中";
                case SurvivorState.Fighting: return "战斗中";
                case SurvivorState.Patrolling: return "巡逻中";
                case SurvivorState.Resting: return "休息中";
                case SurvivorState.Injured: return "受伤";
                case SurvivorState.Dead: return "死亡";
                default: return state.ToString();
            }
        }
    }
}