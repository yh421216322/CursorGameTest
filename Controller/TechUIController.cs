// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：TechUIController.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了科技树界面的UI控制器 (TechUIController)。
//     该控制器负责管理科技树的显示、科技节点的创建与交互、研究信息展示、
//     科技详情面板的显示以及与科技研发相关的用户操作（如开始研究、分配科学家等）。
//     它通过与科技系统 (IAdvancedTechSystem) 和游戏数据模型 (ISurvivalGameModel)
//     交互来获取数据并响应游戏事件。
// ==============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Unity UI 命名空间
using QFramework;      // QFramework 框架
using MyGameNamespace; // 自定义命名空间，包含事件定义
using SurvivalGame.Model;   // 游戏数据模型，如 TechNode, TechStatus 等
using SurvivalGame.GameSystem; // 游戏系统接口，如 IAdvancedTechSystem
using SurvivalGame.Command; // 游戏指令，如 StartResearchCommand
using DG.Tweening;    // DOTween 动画库，用于UI动画效果

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 科技树界面UI控制器。
    /// 负责管理科技树UI的显示、交互逻辑，包括科技节点的动态创建、状态更新、
    /// 研究信息面板的维护以及科技详情的展示。
    /// 通常挂载于场景中的科技树UI主面板对象上 (例如 Canvas/TechUI)。
    /// 依赖于科技系统 (IAdvancedTechSystem) 和游戏数据模型 (ISurvivalGameModel)。
    /// </summary>
    public class TechUIController : MonoBehaviour, IController, ICanSendEvent // 实现QFramework接口
    {
        [Header("主UI面板引用")] // Inspector中UI元素的分组标签
        [SerializeField] private GameObject techPanel;          // 科技树主面板的GameObject
        [SerializeField] private Button techToggleBtn;        // 打开/关闭科技树面板的按钮 (通常在主游戏UI上)
        [SerializeField] private Button closeTechBtn;         // 关闭科技树主面板的按钮
        [SerializeField] private Transform techNodeContainer;    // 容纳所有科技节点UI的父Transform容器
        [SerializeField] private GameObject techNodePrefab;       // 单个科技节点UI的预制件

        [Header("研究信息面板UI元素")]
        [SerializeField] private GameObject researchInfoPanel;    // 显示当前研究概览信息的面板
        [SerializeField] private Text currentResearchText;    // 显示当前正在研究的科技名称
        [SerializeField] private Slider researchProgressSlider; // 显示当前研究进度的滑动条
        [SerializeField] private Text researchPointsText;     // 显示当前研究点数产出速率 (例如：+X点/秒)
        [SerializeField] private Text scientistCountText;     // 显示已分配的科学家数量
        [SerializeField] private Button addScientistBtn;        // 增加参与研究的科学家按钮
        [SerializeField] private Button removeScientistBtn;     // 减少参与研究的科学家按钮

        [Header("科技详情面板UI元素")]
        [SerializeField] private GameObject techDetailPanel;      // 显示选定科技详细信息的面板
        [SerializeField] private Text techNameText;           // 科技名称 (详情)
        [SerializeField] private Text techDescriptionText;    // 科技描述 (详情)
        [SerializeField] private Text techCostText;           // 研究该科技所需的成本 (研究点数、资源等)
        [SerializeField] private Text techPrereqText;         // 研究该科技所需的前置科技条件
        [SerializeField] private Button startResearchBtn;       // 开始/取消研究该科技的按钮
        [SerializeField] private Button closeTechDetailBtn;     // 关闭科技详情面板的按钮

        // QFramework及游戏核心系统引用
        private IAdvancedTechSystem mTechSystem;    // 科技系统接口实例
        private ISurvivalGameModel mGameModel;      // 游戏数据模型接口实例
        
        // UI内部状态变量
        private bool mIsTechPanelOpen = false; // 标记科技树主面板当前是否打开
        private Dictionary<string, GameObject> mTechNodeObjects; // 缓存已创建的科技节点UI对象，键为科技ID
        private string mSelectedTechId;        // 当前在详情面板中选中的科技ID
        private string mCurrentResearchTech;   // 当前正在研究的科技ID

        /// <summary>
        /// Unity生命周期方法：当脚本实例被创建时调用。
        /// 用于获取QFramework框架组件和初始化内部数据结构及UI初始状态。
        /// </summary>
        private void Awake()
        {
            // 获取QFramework框架中的科技系统和游戏数据模型实例
            mTechSystem = this.GetSystem<IAdvancedTechSystem>();
            mGameModel = this.GetModel<ISurvivalGameModel>();
            
            // 初始化用于存储科技节点UI对象的字典
            mTechNodeObjects = new Dictionary<string, GameObject>();
            
            // 设置UI面板的初始可见状态 (默认关闭)
            if(techPanel != null) techPanel.SetActive(false);
            if(techDetailPanel != null) techDetailPanel.SetActive(false);
        }
        
        /// <summary>
        /// Unity生命周期方法：在Awake之后、首次Update之前调用一次。
        /// 用于设置按钮监听、创建科技树节点、更新初始研究信息及注册游戏事件。
        /// </summary>
        private void Start()
        {
            SetupButtons();         // 初始化各个UI按钮的点击事件监听器
            CreateTechNodes();      // 根据配置动态创建科技树中的所有科技节点UI
            UpdateResearchInfo();   // 更新研究信息面板的初始显示内容
            
            // 注册对相关游戏事件的监听，以便在事件发生时更新UI
            this.RegisterEvent<TechUnlockedEvent>(OnTechUnlocked).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<TechResearchStartedEvent>(OnResearchStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<TechResearchCompletedEvent>(OnResearchCompleted).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<StartResearchSuccessEvent>(OnStartResearchSuccess).UnRegisterWhenGameObjectDestroyed(gameObject); // 可能是冗余或特定流程事件
            this.RegisterEvent<ScientistAssignedEvent>(OnScientistAssigned).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 用于处理快捷键（如T键开关科技面板）和在面板打开时定期更新研究信息。
        /// </summary>
        private void Update()
        {
            // 检测快捷键 T，用于快速打开/关闭科技树面板
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleTechPanel();
            }
            
            // 如果科技树面板是打开的，则每帧更新研究信息面板（例如进度条）
            // TODO: 研究进度条的更新可以优化为仅在进度实际变化时更新，而不是每帧，以提升性能。
            // 可以通过监听TechSystem中研究进度的变化事件来实现。
            if (mIsTechPanelOpen)
            {
                UpdateResearchInfo();
            }
        }
        
        /// <summary>
        /// 设置UI中各个按钮的点击事件监听器。
        /// </summary>
        /// <summary>
        /// 设置UI中各个按钮的点击事件监听器。
        /// </summary>
        private void SetupButtons()
        {
            // 为主控制按钮（如打开/关闭科技面板）添加监听
            if(techToggleBtn != null) techToggleBtn.onClick.AddListener(ToggleTechPanel);
            if(closeTechBtn != null) closeTechBtn.onClick.AddListener(CloseTechPanel);
            
            // 为研究信息面板中的科学家分配按钮添加监听
            if(addScientistBtn != null) addScientistBtn.onClick.AddListener(() => {
                // 获取当前科学家分配情况 (假设存储在游戏模型中)
                var currentAssignment = mGameModel.WorkerAssignment.ContainsKey("Scientist") ? 
                    mGameModel.WorkerAssignment["Scientist"] : new List<string>();
                int currentCount = currentAssignment.Count;
                int newCount = currentCount + 1; // 目标是增加一个科学家
                
                // 发送一个 ScientistAssignedEvent 事件来请求更改科学家数量
                // 注意：此事件可能需要一个更复杂的命令来处理实际的工人分配逻辑
                this.SendEvent(new ScientistAssignedEvent 
                { 
                    RequestedCount = newCount, // 请求分配的数量
                    AssignedCount = newCount   // 假设总是成功，实际应由系统确认
                });
                // TODO: 实际的科学家分配逻辑应由专门的命令或系统处理，这里仅发送事件作为通知
                Debug.Log($"[TechUI] 请求增加科学家数量至: {newCount}");
            });
            
            if(removeScientistBtn != null) removeScientistBtn.onClick.AddListener(() => {
                var currentAssignment = mGameModel.WorkerAssignment.ContainsKey("Scientist") ? 
                    mGameModel.WorkerAssignment["Scientist"] : new List<string>();
                int currentCount = currentAssignment.Count;
                if (currentCount > 0) // 确保当前有科学家才能移除
                {
                    int newCount = currentCount - 1; // 目标是减少一个科学家
                    this.SendEvent(new ScientistAssignedEvent 
                    { 
                        RequestedCount = newCount, 
                        AssignedCount = newCount 
                    });
                    Debug.Log($"[TechUI] 请求减少科学家数量至: {newCount}");
                }
            });
            
            // 为科技详情面板中的按钮添加监听
            if(startResearchBtn != null) startResearchBtn.onClick.AddListener(StartSelectedResearch); // 开始研究选中科技
            if(closeTechDetailBtn != null) closeTechDetailBtn.onClick.AddListener(() => {
                if(techDetailPanel != null) techDetailPanel.SetActive(false); // 关闭详情面板
                mSelectedTechId = null; // 清除选中的科技ID
            });
        }
        
        /// <summary>
        /// 根据配置动态创建科技树中的所有科技节点UI元素。
        /// </summary>
        private void CreateTechNodes()
        {
            if (techNodePrefab == null) // 确保科技节点的预制件已在Inspector中设置
            {
                Debug.LogError("[TechUI] 科技节点预制件 (TechNodePrefab) 未设置！无法创建科技树。");
                return;
            }
            
            // 从科技系统中获取所有科技的信息 (包括已研究、可研究和锁定的)
            // 此处假设GetAvailableTechs和GetResearchedTechs返回的是TechNode或类似包含完整信息的对象
            var allTechs = new List<TechNode>();
            allTechs.AddRange(mTechSystem.GetAvailableTechs()); // 获取可研究的
            allTechs.AddRange(mTechSystem.GetResearchedTechs()); // 获取已研究的
            
            // 为了显示完整的科技树，可能还需要获取所有未解锁的科技
            // 这里用一个HashSet来避免重复添加（如果GetTechSystemData().AllTechs包含所有科技的话）
            var displayedTechIds = new HashSet<string>(allTechs.Select(t => t.Id));
            var systemTechData = mTechSystem.GetTechSystemData(); // 假设这里能拿到所有科技的配置
            if(systemTechData != null && systemTechData.AllTechs != null) // AllTechs应为List<TechNode>或类似
            {
                foreach(var techNode in systemTechData.AllTechs) // 遍历所有定义的科技
                {
                    if(!displayedTechIds.Contains(techNode.Id)) // 如果尚未在列表中，则添加
                    {
                        allTechs.Add(techNode);
                        displayedTechIds.Add(techNode.Id);
                    }
                }
            }
            
            // 为获取到的每个科技数据创建对应的UI节点
            foreach (var tech in allTechs)
            {
                CreateTechNode(tech);
            }
            
            Debug.Log($"[TechUI] 已创建 {mTechNodeObjects.Count} 个科技节点UI。");
        }
        
        /// <summary>
        /// 为单个科技数据创建一个UI节点实例，并进行初始化。
        /// </summary>
        /// <param name="tech">要为其创建UI节点的科技数据。</param>
        private void CreateTechNode(TechNode tech)
        {
            // 如果已为该科技ID创建过节点，则不再重复创建 (防止意外的重复调用)
            if (mTechNodeObjects.ContainsKey(tech.Id)) return;
            
            // 实例化科技节点预制件，并将其父对象设置为techNodeContainer
            GameObject nodeGO = Instantiate(techNodePrefab, techNodeContainer);
            nodeGO.name = $"TechNode_{tech.Id}"; // 设置GameObject名称，便于调试识别
            
            // 获取或添加TechNodeUI组件 (预制件上应已挂载此脚本)
            var nodeController = nodeGO.GetComponent<TechNodeUI>();
            if (nodeController == null)
            {
                Debug.LogWarning($"[TechUI] TechNodePrefab上缺少TechNodeUI组件，为 {nodeGO.name} 动态添加。建议在预制件上预设。");
                nodeController = nodeGO.AddComponent<TechNodeUI>();
            }
            
            // 初始化TechNodeUI组件，传入科技数据和对当前TechUIController的引用
            nodeController.Initialize(tech, this);
            mTechNodeObjects[tech.Id] = nodeGO; // 将创建的节点UI对象存入字典，以便后续访问和更新
            
            // 根据科技数据设置节点在科技树UI中的位置
            SetTechNodePosition(nodeGO, tech);
        }
        
        /// <summary>
        /// 设置单个科技节点UI在科技树容器中的位置。
        /// 此为简易布局逻辑，实际项目中可能需要更复杂的布局算法（如依赖关系连线、自动排列等）。
        /// </summary>
        /// <param name="nodeGO">科技节点的GameObject。</param>
        /// <param name="tech">该节点的科技数据。</param>
        private void SetTechNodePosition(GameObject nodeGO, TechNode tech)
        {
            // 简化的基于科技类别和层级（由前置条件数量估算）的网格布局
            Vector2 basePosition = Vector2.zero; // 初始基准位置
            
            // 1. 按科技类别 (TechCategory) 分配大致的水平位置 (X轴)
            // 这是一种常见的科技树可视化方式，不同分支在不同列
            switch (tech.Category) // 假设TechNode有Category属性
            {
                case TechCategory.Production: basePosition.x = -300f; break;
                case TechCategory.Defense:    basePosition.x = -100f; break;
                case TechCategory.Medical:    basePosition.x = 100f;  break;
                case TechCategory.Energy:     basePosition.x = 300f;  break;
                case TechCategory.Special:    basePosition.x = 0f;    break; // 特殊科技放中间或其他位置
                default: basePosition.x = (mTechNodeObjects.Count % 5 - 2) * 200f; break; // 未分类的简单排列
            }
            
            // 2. 按科技的层级 (Tier) 或前置条件数量分配大致的垂直位置 (Y轴)
            // 层级越高的科技（或前置条件越多的）显示在越下方
            // int layer = tech.Prerequisites?.Count ?? 0; // 使用前置条件数量作为层级估算 (?.安全访问)
            int layer = (int)tech.Tier; // 或者如果TechNode有明确的Tier属性
            basePosition.y = 200f - (layer * 150f); // Y轴位置，层级越高越靠下，150f是行间距
            
            // 应用计算出的位置到节点的RectTransform (假设是UGUI)
            var rectTransform = nodeGO.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = basePosition;
            }
            else
            {
                nodeGO.transform.localPosition = basePosition; // 如果不是UGUI，则使用localPosition
            }
        }
        
        /// <summary>
        /// 更新研究信息面板的显示内容（当前研究、进度、研究速度、科学家数量）。
        /// </summary>
        private void UpdateResearchInfo()
        {
            if (researchInfoPanel == null || mTechSystem == null || mGameModel == null) return; // 确保所需组件和系统存在
            
            // 更新当前正在研究的科技名称
            bool hasCurrentResearch = !string.IsNullOrEmpty(mCurrentResearchTech); // mCurrentResearchTech在OnResearchStarted时设置
            if (currentResearchText != null)
            {
                currentResearchText.text = hasCurrentResearch ?
                    $"研究中: {mTechSystem.GetTech(mCurrentResearchTech)?.Name ?? "未知科技"}" :
                    "当前研究: 无";
            }

            // 更新研究进度条
            if (researchProgressSlider != null)
            {
                if (hasCurrentResearch)
                {
                    float progress = mTechSystem.GetResearchProgress(mCurrentResearchTech); // 获取进度 (0-1)
                    researchProgressSlider.value = progress;
                    researchProgressSlider.gameObject.SetActive(true); // 显示进度条
                }
                else
                {
                    researchProgressSlider.gameObject.SetActive(false); // 无研究则隐藏进度条
                }
            }
            
            // 更新研究点数产出速率
            if (researchPointsText != null)
            {
                float researchRate = mTechSystem.GetResearchPointsPerSecond(); // 获取每秒研究点数
                researchPointsText.text = $"研究速度: {researchRate:F2} 点/秒"; // F2表示保留两位小数
            }
            
            // 更新已分配的科学家数量
            if (scientistCountText != null)
            {
                // 假设科学家分配信息存储在游戏模型mGameModel.WorkerAssignment中
                // 键为"Scientist" (或其他代表科学家的标识符), 值为一个列表或计数
                var scientistAssignment = mGameModel.WorkerAssignment.ContainsKey("Scientist_Research") ? // 使用更具体的键
                    mGameModel.WorkerAssignment["Scientist_Research"] : new List<string>(); // 假设是ID列表
                int scientistsCount = scientistAssignment.Count;
                // TODO: 可能需要从科技系统或幸存者系统获取最大可分配科学家数量
                // int maxScientists = mTechSystem.GetMaxScientistCapacity();
                scientistCountText.text = $"科学家: {scientistsCount}"; // / {maxScientists}";
            }
        }
        
        /// <summary>
        /// 当玩家在科技树中点击选择一个科技节点时调用。
        /// </summary>
        /// <param name="techId">被选中的科技的ID。</param>
        public void SelectTech(string techId)
        {
            mSelectedTechId = techId;    // 记录当前选中的科技ID
            ShowTechDetail(techId);     // 显示该科技的详情面板
        }
        
        /// <summary>
        /// 显示指定科技的详细信息面板，并填充其内容。
        /// </summary>
        /// <param name="techId">要显示详情的科技ID。</param>
        private void ShowTechDetail(string techId)
        {
            var tech = mTechSystem.GetTech(techId); // 从科技系统获取该科技的详细数据
            if (tech == null) // 如果未找到该科技数据，则不显示详情
            {
                Debug.LogWarning($"[TechUI] 尝试显示详情失败：未找到ID为 '{techId}' 的科技。");
                if(techDetailPanel != null) techDetailPanel.SetActive(false);
                return;
            }
            
            // 填充详情面板的各个UI文本元素
            if(techNameText != null) techNameText.text = tech.Name;
            if(techDescriptionText != null) techDescriptionText.text = tech.Description;
            if(techCostText != null) techCostText.text = $"研究成本: {tech.ResearchCost} 点科研值"; // 示例成本显示
            
            // 显示前置科技条件
            if (techPrereqText != null)
            {
                if (tech.Prerequisites != null && tech.Prerequisites.Count > 0)
                {
                    string prereqDisplayText = "前置科技要求:\n";
                    foreach (var prereqId in tech.Prerequisites)
                    {
                        var prereqTech = mTechSystem.GetTech(prereqId); // 获取前置科技的信息
                        string statusSymbol = (prereqTech != null && prereqTech.Status == TechStatus.Researched) ? "<color=green>✓</color>" : "<color=red>✗</color>"; // 根据是否已研究显示对勾或叉
                        prereqDisplayText += $"  {statusSymbol} {prereqTech?.Name ?? prereqId}\n"; // 显示前置科技名称
                    }
                    techPrereqText.text = prereqDisplayText;
                }
                else
                {
                    techPrereqText.text = "前置科技: 无";
                }
            }

            // 更新“开始研究”按钮的状态和文本
            if (startResearchBtn != null)
            {
                bool canResearch = tech.Status == TechStatus.Available; // 判断该科技当前是否可研究
                startResearchBtn.interactable = canResearch; // 设置按钮是否可交互
                var buttonText = startResearchBtn.GetComponentInChildren<Text>(); // 获取按钮上的文本组件
                if (buttonText != null)
                {
                    if (tech.Status == TechStatus.Researched) buttonText.text = "已研究完成";
                    else if (tech.Status == TechStatus.Researching) buttonText.text = "研究中..."; // 如果有正在研究的状态
                    else if (canResearch) buttonText.text = "开始研究";
                    else buttonText.text = "条件未满足";
                }
            }
            
            // 激活并显示科技详情面板，并播放打开动画
            if(techDetailPanel != null)
            {
                techDetailPanel.SetActive(true);
                techDetailPanel.transform.localScale = Vector3.zero; // 先设置为0，为动画做准备
                techDetailPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack); // 播放放大动画
            }
        }
        
        /// <summary>
        /// 当玩家点击“开始研究”按钮时调用此方法。
        /// </summary>
        private void StartSelectedResearch()
        {
            if (string.IsNullOrEmpty(mSelectedTechId)) // 如果没有选中的科技，则不执行
            {
                Debug.LogWarning("[TechUI] 开始研究失败：没有选中的科技。");
                return;
            }

            // 实际项目中，研究建筑ID可能需要从玩家选择或特定逻辑中获取
            string defaultResearchBuildingId = "research_lab_1"; // 假设一个默认的研究建筑ID
            
            // 发送StartResearchCommand命令来请求开始研究选定的科技
            this.SendCommand(new StartResearchCommand(mSelectedTechId, defaultResearchBuildingId, null)); // 第三个参数是研究员ID列表，此处为null

            // 研究开始后（无论成功与否，具体由命令和系统处理），关闭科技详情面板
            if(techDetailPanel != null) techDetailPanel.SetActive(false);
            mSelectedTechId = null; // 清除选中的科技
        }
        
        /// <summary>
        /// 切换科技树主面板的显示/隐藏状态。
        /// </summary>
        private void ToggleTechPanel()
        {
            if (mIsTechPanelOpen) CloseTechPanel();
            else OpenTechPanel();
        }
        
        /// <summary>
        /// 打开科技树主面板，并播放打开动画。
        /// </summary>
        private void OpenTechPanel()
        {
            if (techPanel == null) return;

            techPanel.SetActive(true); // 先激活面板才能播放动画
            mIsTechPanelOpen = true;    // 更新面板打开状态标志
            
            RefreshTechNodes(); // 打开时刷新所有科技节点的状态和显示
            
            // 使用DOTween播放打开动画：从零大小缩放到原始大小
            techPanel.transform.localScale = Vector3.zero;
            techPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            Debug.Log("[TechUI] 科技树面板已打开。");
        }
        
        /// <summary>
        /// 关闭科技树主面板及详情面板，并播放关闭动画。
        /// </summary>
        private void CloseTechPanel()
        {
            if (techPanel == null) return;

            // 使用DOTween播放关闭动画：从当前大小缩放到零
            // 动画完成后，将面板设置为非激活状态，并关闭可能打开的详情面板，更新状态标志
            techPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                .OnComplete(() => { // 动画完成后的回调
                    techPanel.SetActive(false);
                    if(techDetailPanel != null) techDetailPanel.SetActive(false); // 同时关闭详情面板
                    mIsTechPanelOpen = false;
                    mSelectedTechId = null; // 清除选中项
                });
            Debug.Log("[TechUI] 科技树面板已关闭。");
        }
        
        /// <summary>
        /// 刷新所有已创建的科技节点UI的状态（例如：是否已研究、可研究、锁定）。
        /// </summary>
        private void RefreshTechNodes()
        {
            if (mTechSystem == null || mTechNodeObjects == null) return;

            // 遍历缓存中的所有科技节点UI对象
            foreach (var kvp in mTechNodeObjects)
            {
                var techNodeData = mTechSystem.GetTech(kvp.Key); // 从科技系统获取最新的科技数据
                var nodeController = kvp.Value.GetComponent<TechNodeUI>(); // 获取该UI对象上的TechNodeUI组件
                if (nodeController != null && techNodeData != null)
                {
                    nodeController.UpdateTechState(techNodeData); // 调用TechNodeUI的方法更新其显示状态
                }
            }
        }
        
        #region 事件处理
        
        /// <summary>
        /// 当某个科技被成功解锁（研究完成）时调用的事件处理器。
        /// </summary>
        /// <param name="e">科技解锁事件数据。</param>
        private void OnTechUnlocked(TechUnlockedEvent e)
        {
            // Debug.Log($"[TechUI] 接收到科技解锁事件: {e.TechName} (ID: {e.TechId})");
            // 找到对应的科技节点UI，并播放解锁动画
            if (mTechNodeObjects.ContainsKey(e.TechId))
            {
                var nodeGO = mTechNodeObjects[e.TechId];
                var nodeController = nodeGO.GetComponent<TechNodeUI>();
                nodeController?.PlayUnlockAnimation(); // 播放解锁动画
                nodeController?.UpdateTechState(mTechSystem.GetTech(e.TechId)); // 更新状态为已研究
            }
            RefreshTechNodes(); // 可能有其他科技因此变为可研究，刷新整个树的状态
            UpdateResearchInfo(); // 清理当前研究信息
        }
        
        /// <summary>
        /// 当某个科技开始研究时调用的事件处理器。
        /// </summary>
        /// <param name="e">科技开始研究事件数据。</param>
        private void OnResearchStarted(TechResearchStartedEvent e)
        {
            mCurrentResearchTech = e.TechId; // 记录当前正在研究的科技ID
            // Debug.Log($"[TechUI] 接收到科技开始研究事件: {e.TechName} (ID: {e.TechId})");

            // 找到对应的科技节点UI，并播放研究中动画/更新状态
            if (mTechNodeObjects.ContainsKey(e.TechId))
            {
                var nodeController = mTechNodeObjects[e.TechId].GetComponent<TechNodeUI>();
                nodeController?.PlayResearchingAnimation(); // 播放研究中动画
                nodeController?.UpdateTechState(mTechSystem.GetTech(e.TechId)); // 更新状态为研究中
            }
            UpdateResearchInfo(); // 更新研究信息面板的显示
        }
        
        /// <summary>
        /// 当某个科技研究完成时调用的事件处理器。
        /// (注意：TechUnlockedEvent 通常在此时也会触发，需确认逻辑是否重复或各有侧重)
        /// </summary>
        /// <param name="e">科技研究完成事件数据。</param>
        private void OnResearchCompleted(TechResearchCompletedEvent e)
        {
            if (mCurrentResearchTech == e.TechId) // 确保是当前研究的科技完成了
            {
                mCurrentResearchTech = null; // 清空当前研究的科技ID
            }
            // Debug.Log($"[TechUI] 接收到科技研究完成事件: {e.TechName} (ID: {e.TechId})");

            // 找到对应的科技节点UI，并播放完成动画/更新状态
            if (mTechNodeObjects.ContainsKey(e.TechId))
            {
                var nodeController = mTechNodeObjects[e.TechId].GetComponent<TechNodeUI>();
                nodeController?.PlayCompletedAnimation(); // 播放完成动画
                // OnTechUnlocked事件处理器通常也会更新节点状态为已研究，这里可能无需重复调用UpdateTechState
            }
            
            RefreshTechNodes();   // 刷新所有节点状态，因为一个科技的完成可能解锁其他科技
            UpdateResearchInfo(); // 更新研究信息面板（当前研究应为空）
        }
        
        /// <summary>
        /// 当通过命令成功启动一项研究时的事件处理器。
        /// (此事件可能是对StartResearchCommand成功执行的响应)
        /// </summary>
        private void OnStartResearchSuccess(StartResearchSuccessEvent e)
        {
            // 通常实际的研究开始逻辑由OnResearchStarted处理，此事件可能用于UI反馈如“研究已成功排入队列”
            Debug.Log($"[TechUI] 科技 {e.TechId} 已成功开始/加入研究队列。");
            // 可能需要更新详情面板按钮状态，例如变为“取消研究”或显示进度
            if (mSelectedTechId == e.TechId && techDetailPanel != null && techDetailPanel.activeSelf)
            {
                ShowTechDetail(mSelectedTechId); // 刷新详情面板
            }
        }
        
        /// <summary>
        /// 当科学家数量发生变化时的事件处理器。
        /// </summary>
        private void OnScientistAssigned(ScientistAssignedEvent e)
        {
            // Debug.Log($"[TechUI] 接收到科学家分配事件：请求 {e.RequestedCount}, 实际分配 {e.AssignedCount}");
            UpdateResearchInfo(); // 更新研究信息面板中显示的科学家数量和研究速度
        }
        
        #endregion
        
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
        
        private void OnDestroy()
        {
            // 取消事件订阅，防止内存泄漏
            this.UnRegisterEvent<TechUnlockedEvent>(OnTechUnlocked);
            this.UnRegisterEvent<TechResearchStartedEvent>(OnResearchStarted);
            this.UnRegisterEvent<TechResearchCompletedEvent>(OnResearchCompleted);
            this.UnRegisterEvent<StartResearchSuccessEvent>(OnStartResearchSuccess);
            this.UnRegisterEvent<ScientistAssignedEvent>(OnScientistAssigned);
        }
    }
    
    /// <summary>
    /// 控制单个科技树节点的UI表现和交互。
    /// 通常挂载在科技节点预制件上。
    /// </summary>
    public class TechNodeUI : MonoBehaviour
    {
        [Header("节点UI组件引用")] // Inspector中分组显示
        [SerializeField] private Button nodeButton;     // 节点的可点击按钮区域
        [SerializeField] private Image nodeImage;        // 节点背景图片或图标
        [SerializeField] private Text nameText;         // 显示科技名称的文本
        [SerializeField] private Image progressFill;     // (可选) 研究进度填充条图片
        [SerializeField] private GameObject lockIcon;     // (可选) 科技未解锁时显示的锁定图标
        [SerializeField] private GameObject completedIcon; // (可选) 科技已完成时显示的完成图标
        
        private TechNode mTech;             // 当前节点关联的科技数据
        private TechUIController mController; // 对主TechUIController的引用，用于回调选择等操作
        
        /// <summary>
        /// 初始化科技节点UI。
        /// </summary>
        /// <param name="tech">此节点代表的科技数据。</param>
        /// <param name="controller">主科技UI控制器实例。</param>
        public void Initialize(TechNode tech, TechUIController controller)
        {
            mTech = tech;
            mController = controller;
            
            // 设置基础显示信息，如科技名称
            if (nameText != null) nameText.text = tech.Name;
            
            // 为节点按钮添加点击监听，点击时通知主控制器选择了此科技
            if (nodeButton != null)
            {
                nodeButton.onClick.RemoveAllListeners(); // 清除旧监听，防止重复添加
                nodeButton.onClick.AddListener(() => mController.SelectTech(tech.Id));
            }
            
            // 根据初始科技状态更新节点UI显示
            UpdateTechState(tech);
        }
        
        /// <summary>
        /// 根据传入的科技数据更新节点的视觉状态（颜色、图标、可交互性等）。
        /// </summary>
        /// <param name="tech">最新的科技数据。</param>
        public void UpdateTechState(TechNode tech)
        {
            if (tech == null) return;
            mTech = tech; // 更新内部数据引用
            
            // 根据科技的当前状态 (TechStatus) 更新节点的视觉表现
            switch(tech.Status)
            {
                case TechStatus.Researched: // 已研究完成
                    SetNodeColor(Color.green * 0.8f); // 例如：深绿色
                    SetLockState(false);       // 隐藏锁定图标
                    SetCompletedState(true);   // 显示完成图标
                    SetInteractable(false);    // 通常已研究的科技不可再次操作
                    if(progressFill != null) progressFill.gameObject.SetActive(false); // 隐藏进度条
                    break;
                case TechStatus.Researching: // 正在研究中 (假设有此状态)
                    SetNodeColor(Color.cyan * 0.8f);  // 例如：青色
                    SetLockState(false);
                    SetCompletedState(false);
                    SetInteractable(true);     // 可能允许取消研究或查看进度
                    // PlayResearchingAnimation(); // 可能需要在这里启动或更新研究动画
                    break;
                case TechStatus.Available: // 可研究（前置条件满足，但尚未开始研究）
                    SetNodeColor(Color.yellow * 0.8f); // 例如：黄色
                    SetLockState(false);
                    SetCompletedState(false);
                    SetInteractable(true);     // 允许点击开始研究
                    if(progressFill != null) progressFill.gameObject.SetActive(false);
                    break;
                case TechStatus.Locked: // 锁定状态（前置条件未满足）
                default:
                    SetNodeColor(Color.gray * 0.8f);   // 例如：灰色
                    SetLockState(true);        // 显示锁定图标
                    SetCompletedState(false);
                    SetInteractable(false);    // 锁定状态不可交互
                    if(progressFill != null) progressFill.gameObject.SetActive(false);
                    break;
            }
        }
        
        /// <summary>
        /// 播放科技解锁时的动画效果。
        /// </summary>
        public void PlayUnlockAnimation()
        {
            // 示例动画：节点缩放（放大再缩小）以示强调
            transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo); // 放大到1.2倍再恢复，往返一次
            
            // 节点背景图片颜色闪烁（例如，快速变为白色再恢复）
            if (nodeImage != null)
            {
                nodeImage.DOColor(Color.white, 0.1f).SetLoops(6, LoopType.Yoyo); // 快速闪烁3次
            }
        }
        
        /// <summary>
        /// 播放科技正在研究中的动画效果（例如进度条动画）。
        /// </summary>
        public void PlayResearchingAnimation()
        {
            // 示例：激活并填充研究进度条
            if (progressFill != null)
            {
                progressFill.gameObject.SetActive(true); // 确保进度条可见
                // 假设研究时间是动态的，这里仅演示一个从0到1的填充动画，实际应由外部更新进度值
                // progressFill.DOFillAmount(1f, mTech.ResearchTime).SetEase(Ease.Linear); // 填充动画，时间为科技的研究时间
                // 注意：如果进度由TechSystem驱动，这里可能只是设置初始状态或一个循环的视觉提示
                // 例如一个无限循环的微小动画来表示“正在处理”
                progressFill.fillAmount = 0; // 重置进度条
                // 可以添加一个shader动画或简单的ping-pong效果来表示研究中
            }
        }
        
        /// <summary>
        /// 播放科技研究完成时的动画效果。
        /// </summary>
        public void PlayCompletedAnimation()
        {
            // 示例：停止研究中动画，节点变为完成状态的视觉效果
            if (progressFill != null)
            {
                progressFill.DOKill(); // 停止任何正在进行的填充动画
                progressFill.gameObject.SetActive(false); // 隐藏进度条
            }
            
            // 节点短暂放大并变为完成颜色（例如绿色）
            transform.DOScale(1.3f, 0.3f).SetLoops(2, LoopType.Yoyo);
            if (nodeImage != null)
            {
                // nodeImage.DOColor(Color.green, 0.2f).SetLoops(4, LoopType.Yoyo); // 闪烁绿色
                // UpdateTechState会设置最终颜色，这里可以只是一个瞬时效果
            }
            SetCompletedState(true); // 显示完成图标
        }
        
        /// <summary>
        /// 设置节点背景图片/图标的颜色。
        /// </summary>
        private void SetNodeColor(Color color)
        {
            if (nodeImage != null) nodeImage.color = color;
        }

        /// <summary>
        /// 设置锁定图标的显示状态。
        /// </summary>
        private void SetLockState(bool isLocked)
        {
            if (lockIcon != null) lockIcon.SetActive(isLocked);
        }

        /// <summary>
        /// 设置完成图标的显示状态。
        /// </summary>
        private void SetCompletedState(bool isCompleted)
        {
            if (completedIcon != null) completedIcon.SetActive(isCompleted);
        }

        /// <summary>
        /// 设置节点按钮是否可交互。
        /// </summary>
        private void SetInteractable(bool interactable)
        {
            if (nodeButton != null) nodeButton.interactable = interactable;
        }
    }
}
        {
            // 刷新所有科技节点的状态
            foreach (var kvp in mTechNodeObjects)
            {
                var tech = mTechSystem.GetTech(kvp.Key);
                var nodeController = kvp.Value.GetComponent<TechNodeUI>();
                nodeController?.UpdateTechState(tech);
            }
        }
        
        #region 事件处理方法 (Event Handlers)

        /// <summary>
        /// 当某个科技被成功解锁（研究完成）时调用的事件处理器。
        /// </summary>
        /// <param name="e">科技解锁事件数据。</param>
        private void OnTechUnlocked(TechUnlockedEvent e)
        {
            // Debug.Log($"[TechUI] 接收到科技解锁事件: {e.TechName} (ID: {e.TechId})");
            // 找到对应的科技节点UI，并播放解锁动画
            if (mTechNodeObjects.ContainsKey(e.TechId))
            {
                var nodeGO = mTechNodeObjects[e.TechId];
                var nodeController = nodeGO.GetComponent<TechNodeUI>();
                nodeController?.PlayUnlockAnimation(); // 播放解锁动画
                nodeController?.UpdateTechState(mTechSystem.GetTech(e.TechId)); // 更新状态为已研究
            }
            RefreshTechNodes(); // 可能有其他科技因此变为可研究，刷新整个树的状态
            UpdateResearchInfo(); // 清理当前研究信息
        }
        
        /// <summary>
        /// 当某个科技开始研究时调用的事件处理器。
        /// </summary>
        /// <param name="e">科技开始研究事件数据。</param>
        private void OnResearchStarted(TechResearchStartedEvent e)
        {
            mCurrentResearchTech = e.TechId; // 记录当前正在研究的科技ID
            // Debug.Log($"[TechUI] 接收到科技开始研究事件: {e.TechName} (ID: {e.TechId})");

            // 找到对应的科技节点UI，并播放研究中动画/更新状态
            if (mTechNodeObjects.ContainsKey(e.TechId))
            {
                var nodeController = mTechNodeObjects[e.TechId].GetComponent<TechNodeUI>();
                nodeController?.PlayResearchingAnimation(); // 播放研究中动画
                nodeController?.UpdateTechState(mTechSystem.GetTech(e.TechId)); // 更新状态为研究中
            }
            UpdateResearchInfo(); // 更新研究信息面板的显示
        }
        
        /// <summary>
        /// 当某个科技研究完成时调用的事件处理器。
        /// (注意：TechUnlockedEvent 通常在此时也会触发，需确认逻辑是否重复或各有侧重)
        /// </summary>
        /// <param name="e">科技研究完成事件数据。</param>
        private void OnResearchCompleted(TechResearchCompletedEvent e)
        {
            if (mCurrentResearchTech == e.TechId) // 确保是当前研究的科技完成了
            {
                mCurrentResearchTech = null; // 清空当前研究的科技ID
            }
            // Debug.Log($"[TechUI] 接收到科技研究完成事件: {e.TechName} (ID: {e.TechId})");

            // 找到对应的科技节点UI，并播放完成动画/更新状态
            if (mTechNodeObjects.ContainsKey(e.TechId))
            {
                var nodeController = mTechNodeObjects[e.TechId].GetComponent<TechNodeUI>();
                nodeController?.PlayCompletedAnimation(); // 播放完成动画
                // OnTechUnlocked事件处理器通常也会更新节点状态为已研究，这里可能无需重复调用UpdateTechState
            }

            RefreshTechNodes();   // 刷新所有节点状态，因为一个科技的完成可能解锁其他科技
            UpdateResearchInfo(); // 更新研究信息面板（当前研究应为空）
        }

        /// <summary>
        /// 当通过命令成功启动一项研究时的事件处理器。
        /// (此事件可能是对StartResearchCommand成功执行的响应)
        /// </summary>
        private void OnStartResearchSuccess(StartResearchSuccessEvent e)
        {
            // 通常实际的研究开始逻辑由OnResearchStarted处理，此事件可能用于UI反馈如“研究已成功排入队列”
            Debug.Log($"[TechUI] 科技 {e.TechId} 已成功开始/加入研究队列。");
            // 可能需要更新详情面板按钮状态，例如变为“取消研究”或显示进度
            if (mSelectedTechId == e.TechId && techDetailPanel != null && techDetailPanel.activeSelf)
            {
                ShowTechDetail(mSelectedTechId); // 刷新详情面板
            }
        }

        /// <summary>
        /// 当科学家数量发生变化时的事件处理器。
        /// </summary>
        private void OnScientistAssigned(ScientistAssignedEvent e)
        {
            // Debug.Log($"[TechUI] 接收到科学家分配事件：请求 {e.RequestedCount}, 实际分配 {e.AssignedCount}");
            UpdateResearchInfo(); // 更新研究信息面板中显示的科学家数量和研究速度
        }

        #endregion

        /// <summary>
        /// 实现IController接口，返回QFramework的全局架构实例。
        /// </summary>
        public IArchitecture GetArchitecture() => RegisterManager.Interface; // RegisterManager是QFramework中用于获取架构实例的类

        /// <summary>
        /// Unity生命周期方法：当对象被销毁时调用。
        /// 用于取消事件订阅，防止内存泄漏。
        /// </summary>
        private void OnDestroy()
        {
            // 取消对此脚本中注册的所有事件的监听
            this.UnRegisterEvent<TechUnlockedEvent>(OnTechUnlocked);
            this.UnRegisterEvent<TechResearchStartedEvent>(OnResearchStarted);
            this.UnRegisterEvent<TechResearchCompletedEvent>(OnResearchCompleted);
            this.UnRegisterEvent<StartResearchSuccessEvent>(OnStartResearchSuccess);
            this.UnRegisterEvent<ScientistAssignedEvent>(OnScientistAssigned);
        }
    }

    /// <summary>
    /// 控制单个科技树节点的UI表现和交互。
    /// 通常挂载在科技节点预制件上。
    /// </summary>
    public class TechNodeUI : MonoBehaviour
    {
        [Header("节点UI组件引用")] // Inspector中分组显示
        [SerializeField] private Button nodeButton;     // 节点的可点击按钮区域
        [SerializeField] private Image nodeImage;        // 节点背景图片或图标
        [SerializeField] private Text nameText;         // 显示科技名称的文本
        [SerializeField] private Image progressFill;     // (可选) 研究进度填充条图片
        [SerializeField] private GameObject lockIcon;     // (可选) 科技未解锁时显示的锁定图标
        [SerializeField] private GameObject completedIcon; // (可选) 科技已完成时显示的完成图标

        private TechNode mTech;             // 当前节点关联的科技数据
        private TechUIController mController; // 对主TechUIController的引用，用于回调选择等操作

        /// <summary>
        /// 初始化科技节点UI。
        /// </summary>
        /// <param name="tech">此节点代表的科技数据。</param>
        /// <param name="controller">主科技UI控制器实例。</param>
        public void Initialize(TechNode tech, TechUIController controller)
        {
            mTech = tech;
            mController = controller;

            // 设置基础显示信息，如科技名称
            if (nameText != null) nameText.text = tech.Name;

            // 为节点按钮添加点击监听，点击时通知主控制器选择了此科技
            if (nodeButton != null)
            {
                nodeButton.onClick.RemoveAllListeners(); // 清除旧监听，防止重复添加
                nodeButton.onClick.AddListener(() => mController.SelectTech(tech.Id));
            }

            // 根据初始科技状态更新节点UI显示
            UpdateTechState(tech);
        }

        /// <summary>
        /// 根据传入的科技数据更新节点的视觉状态（颜色、图标、可交互性等）。
        /// </summary>
        /// <param name="tech">最新的科技数据。</param>
        public void UpdateTechState(TechNode tech)
        {
            if (tech == null) return;
            mTech = tech; // 更新内部数据引用

            // 根据科技的当前状态 (TechStatus) 更新节点的视觉表现
            switch(tech.Status)
            {
                case TechStatus.Researched: // 已研究完成
                    SetNodeColor(Color.green * 0.8f); // 例如：深绿色
                    SetLockState(false);       // 隐藏锁定图标
                    SetCompletedState(true);   // 显示完成图标
                    SetInteractable(false);    // 通常已研究的科技不可再次操作
                    if(progressFill != null) progressFill.gameObject.SetActive(false); // 隐藏进度条
                    break;
                case TechStatus.Researching: // 正在研究中 (假设有此状态)
                    SetNodeColor(Color.cyan * 0.8f);  // 例如：青色
                    SetLockState(false);
                    SetCompletedState(false);
                    SetInteractable(true);     // 可能允许取消研究或查看进度
                    // PlayResearchingAnimation(); // 可能需要在这里启动或更新研究动画
                    break;
                case TechStatus.Available: // 可研究（前置条件满足，但尚未开始研究）
                    SetNodeColor(Color.yellow * 0.8f); // 例如：黄色
                    SetLockState(false);
                    SetCompletedState(false);
                    SetInteractable(true);     // 允许点击开始研究
                    if(progressFill != null) progressFill.gameObject.SetActive(false);
                    break;
                case TechStatus.Locked: // 锁定状态（前置条件未满足）
                default:
                    SetNodeColor(Color.gray * 0.8f);   // 例如：灰色
                    SetLockState(true);        // 显示锁定图标
                    SetCompletedState(false);
                    SetInteractable(false);    // 锁定状态不可交互
                    if(progressFill != null) progressFill.gameObject.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// 播放科技解锁时的动画效果。
        /// </summary>
        public void PlayUnlockAnimation()
        {
            // 示例动画：节点缩放（放大再缩小）以示强调
            transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo); // 放大到1.2倍再恢复，往返一次

            // 节点背景图片颜色闪烁（例如，快速变为白色再恢复）
            if (nodeImage != null)
            {
                nodeImage.DOColor(Color.white, 0.1f).SetLoops(6, LoopType.Yoyo); // 快速闪烁3次
            }
        }

        /// <summary>
        /// 播放科技正在研究中的动画效果（例如进度条动画）。
        /// </summary>
        public void PlayResearchingAnimation()
        {
            // 示例：激活并填充研究进度条
            if (progressFill != null)
            {
                progressFill.gameObject.SetActive(true); // 确保进度条可见
                // 假设研究时间是动态的，这里仅演示一个从0到1的填充动画，实际应由外部更新进度值
                // progressFill.DOFillAmount(1f, mTech.ResearchTime).SetEase(Ease.Linear); // 填充动画，时间为科技的研究时间
                // 注意：如果进度由TechSystem驱动，这里可能只是设置初始状态或一个循环的视觉提示
                // 例如一个无限循环的微小动画来表示“正在处理”
                progressFill.fillAmount = 0; // 重置进度条
                // 可以添加一个shader动画或简单的ping-pong效果来表示研究中
            }
        }

        /// <summary>
        /// 播放科技研究完成时的动画效果。
        /// </summary>
        public void PlayCompletedAnimation()
        {
            // 示例：停止研究中动画，节点变为完成状态的视觉效果
            if (progressFill != null)
            {
                progressFill.DOKill(); // 停止任何正在进行的填充动画
                progressFill.gameObject.SetActive(false); // 隐藏进度条
            }

            // 节点短暂放大并变为完成颜色（例如绿色）
            transform.DOScale(1.3f, 0.3f).SetLoops(2, LoopType.Yoyo);
            if (nodeImage != null)
            {
                // nodeImage.DOColor(Color.green, 0.2f).SetLoops(4, LoopType.Yoyo); // 闪烁绿色
                // UpdateTechState会设置最终颜色，这里可以只是一个瞬时效果
            }
            SetCompletedState(true); // 显示完成图标
        }

        /// <summary>
        /// 设置节点背景图片/图标的颜色。
        /// </summary>
        private void SetNodeColor(Color color)
        {
            if (nodeImage != null) nodeImage.color = color;
        }

        /// <summary>
        /// 设置锁定图标的显示状态。
        /// </summary>
        private void SetLockState(bool isLocked)
        {
            if (lockIcon != null) lockIcon.SetActive(isLocked);
        }

        /// <summary>
        /// 设置完成图标的显示状态。
        /// </summary>
        private void SetCompletedState(bool isCompleted)
        {
            if (completedIcon != null) completedIcon.SetActive(isCompleted);
        }

        /// <summary>
        /// 设置节点按钮是否可交互。
        /// </summary>
        private void SetInteractable(bool interactable)
        {
            if (nodeButton != null) nodeButton.interactable = interactable;
        }
    }
} 