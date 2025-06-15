// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：SurvivorItemUI.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了幸存者列表项的UI控制器 (SurvivorItemUI)。
//     该组件用于在幸存者列表中显示单个幸存者的信息，如姓名、职业、等级、状态、
//     健康值和士气值，并处理用户与该列表项的交互（例如点击）。
// ==============================================================================

using System.Collections.Generic; // 尽管当前未使用，但通常列表项可能需要处理集合
using System.Linq;             // 同样，可能用于未来更复杂的逻辑
using UnityEngine;
using UnityEngine.UI;           // Unity UI组件命名空间
using QFramework;               // QFramework框架
using SurvivalGame.Model;       // 包含SurvivorData, SurvivorJob, SurvivorState等模型
using SurvivalGame.GameSystem;  // 可能包含游戏系统接口 (当前未使用)
using DG.Tweening;            // DoTween动画库 (当前未使用，但可能用于未来的UI动画)
using MyGameNamespace;        // 自定义命名空间 (当前未使用)
// using Unity.VisualScripting; // VisualScripting命名空间，当前注释表明未使用

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 幸存者列表项UI控制器。
    /// 负责显示单个幸存者的详细信息，并响应用户的点击操作。
    /// 此组件通常被预制化，并在幸存者列表中动态实例化和填充数据。
    /// </summary>
    public class SurvivorItemUI : BaseUIController // 继承自包含UI查找功能的基类
    {
        [Header("UI组件引用")] // 在Unity Inspector中方便组织和识别
        public Text nameText;         // 显示幸存者姓名的Text组件
        public Text professionText;   // 显示幸存者当前职业的Text组件
        public Text levelText;        // 显示幸存者等级的Text组件
        public Text stateText;        // 显示幸存者当前状态的Text组件
        public Slider healthSlider;   // 显示幸存者健康值的Slider组件
        public Slider moraleSlider;   // 显示幸存者士气值的Slider组件
        public Button itemButton;     // 整个列表项的可点击按钮，用于选中或触发操作

        // 内部数据存储
        private SurvivorData mSurvivorData; // 当前UI项代表的幸存者数据

        /// <summary>
        /// 当用户点击此列表项时触发的事件。
        /// 参数 (string): 被点击幸存者的ID。
        /// </summary>
        public System.Action<string> OnItemClicked; // 定义一个Action委托作为点击事件

        /// <summary>
        /// Unity生命周期方法：在对象加载时调用，早于Start。
        /// 主要用于初始化，此处调用FindUIComponents来自动关联UI子组件。
        /// </summary>
        private void Awake()
        {
            // 利用基类(BaseUIController)提供的FindUIComponent方法来查找并关联UI组件
            FindUIComponents();
            
            // 原注释提到“移除旧的初始化代码，新的SurvivorData不需要这些方法”
            // 这表明此部分可能经过重构，旧的初始化逻辑已被移除或整合。
        }

        /// <summary>
        /// 自动查找并关联此UI项所需的各个子UI组件。
        /// 如果在Inspector中未手动拖拽赋值，则会尝试通过名称在子对象中查找。
        /// </summary>
        private void FindUIComponents()
        {
            // 使用基类提供的FindUIComponent<T>(path)方法查找各组件
            // path参数是相对于当前GameObject的子对象路径
            if (nameText == null) nameText = FindUIComponent<Text>("nameText");
            if (professionText == null) professionText = FindUIComponent<Text>("professionText");
            if (levelText == null) levelText = FindUIComponent<Text>("levelText");
            if (stateText == null) stateText = FindUIComponent<Text>("stateText");
            if (healthSlider == null) healthSlider = FindUIComponent<Slider>("healthSlider");
            if (moraleSlider == null) moraleSlider = FindUIComponent<Slider>("moraleSlider");
            if (itemButton == null) itemButton = FindUIComponent<Button>("itemButton");

           // Debug.Log("[SurvivorItemUI] UI组件自动关联完成。"); // 调试日志，确认关联过程
        }

        /// <summary>
        /// Unity生命周期方法：在对象首次启用且Awake执行完毕后调用。
        /// 用于为itemButton（如果存在）添加点击事件监听器。
        /// </summary>
        private void Start()
        {
            if (itemButton != null) // 确保按钮已正确关联
                itemButton.onClick.AddListener(OnButtonClicked); // 注册点击事件处理方法
        }

        /// <summary>
        /// 设置并显示指定幸存者的数据。
        /// </summary>
        /// <param name="survivor">要在此UI项中显示的幸存者数据对象。</param>
        public void SetSurvivorData(SurvivorData survivor)
        {
            mSurvivorData = survivor; // 存储传入的幸存者数据
            UpdateDisplay();          // 调用方法更新UI显示
        }

        /// <summary>
        /// 根据当前存储的 mSurvivorData 更新所有UI元素的显示内容。
        /// </summary>
        private void UpdateDisplay()
        {
            if (mSurvivorData == null) return; // 如果没有幸存者数据，则不执行更新

            // 更新各个Text和Slider组件的显示
            if (nameText != null) nameText.text = mSurvivorData.Name; // 显示姓名
            if (professionText != null) professionText.text = GetJobDisplayName(mSurvivorData.CurrentJob); // 显示职业（本地化名称）
            if (levelText != null) levelText.text = $"等级 {mSurvivorData.Level}"; // 显示等级，格式化为 "Lv.X"
            if (stateText != null) stateText.text = GetSurvivorStateName(mSurvivorData.State); // 显示状态（本地化名称）

            // 更新健康值Slider，假设健康值范围是0-100，Slider范围是0-1
            if (healthSlider != null)
                healthSlider.value = mSurvivorData.Health / 100f;

            // 更新士气值Slider，假设士气值范围是0-100，Slider范围是0-1
            if (moraleSlider != null)
                moraleSlider.value = mSurvivorData.Morale / 100f;
        }

        /// <summary>
        /// 当列表项的按钮被点击时调用的处理方法。
        /// </summary>
        private void OnButtonClicked()
        {
            // 触发OnItemClicked事件，并传递当前幸存者的ID
            // 使用?.安全调用，确保mSurvivorData不为null且OnItemClicked有订阅者
            OnItemClicked?.Invoke(mSurvivorData?.Id);
            // Debug.Log($"[SurvivorItemUI] 幸存者项被点击: {mSurvivorData?.Name} (ID: {mSurvivorData?.Id})");
        }

        /// <summary>
        /// 根据幸存者职业枚举 (SurvivorJob) 获取本地化的显示名称。
        /// </summary>
        /// <param name="job">幸存者的职业枚举值。</param>
        /// <returns>对应职业的中文显示名称。</returns>
        private string GetJobDisplayName(SurvivorJob job)
        {
            switch (job)
            {
                case SurvivorJob.Idle:         return "空闲";
                case SurvivorJob.Production:   return "生产";
                case SurvivorJob.Defense:      return "防御";
                case SurvivorJob.Research:     return "研究";
                case SurvivorJob.Construction: return "建造";
                case SurvivorJob.Medical:      return "医疗";
                case SurvivorJob.Exploration:  return "探索";
                case SurvivorJob.Leadership:   return "领导";
                default: return job.ToString(); // 对于未明确指定的枚举值，返回其字符串表示
            }
        }

        /// <summary>
        /// (静态方法) 根据幸存者状态枚举 (SurvivorState) 获取本地化的显示名称。
        /// </summary>
        /// <param name="state">幸存者的状态枚举值。</param>
        /// <returns>对应状态的中文显示名称。</returns>
        public static string GetSurvivorStateName(SurvivorState state)
        {
            switch (state)
            {
                case SurvivorState.Idle:      return "空闲";
                case SurvivorState.Working:   return "工作中";
                case SurvivorState.Fighting:  return "战斗中";
                case SurvivorState.Patrolling:return "巡逻中";
                case SurvivorState.Resting:   return "休息中";
                case SurvivorState.Injured:   return "受伤";
                case SurvivorState.Dead:      return "阵亡"; // "死亡"可能更中性，"阵亡"带有荣誉感
                default: return state.ToString(); // 默认返回枚举的字符串表示
            }
        }
    }
}