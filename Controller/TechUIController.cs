using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using MyGameNamespace;
using SurvivalGame.Model;
using SurvivalGame.GameSystem;
using SurvivalGame.Command;
using DG.Tweening;

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 科技树UI控制器
    /// 挂载对象：Canvas/TechUI
    /// 依赖系统：ITechSystem、ISurvivalGameModel
    /// </summary>
    public class TechUIController : MonoBehaviour, IController, ICanSendEvent
    {
        [Header("UI引用")]
        [SerializeField] private GameObject techPanel;
        [SerializeField] private Button techToggleBtn;
        [SerializeField] private Button closeTechBtn;
        [SerializeField] private Transform techNodeContainer;
        [SerializeField] private GameObject techNodePrefab;
        
        [Header("研究信息面板")]
        [SerializeField] private GameObject researchInfoPanel;
        [SerializeField] private Text currentResearchText;
        [SerializeField] private Slider researchProgressSlider;
        [SerializeField] private Text researchPointsText;
        [SerializeField] private Text scientistCountText;
        [SerializeField] private Button addScientistBtn;
        [SerializeField] private Button removeScientistBtn;
        
        [Header("科技详情面板")]
        [SerializeField] private GameObject techDetailPanel;
        [SerializeField] private Text techNameText;
        [SerializeField] private Text techDescriptionText;
        [SerializeField] private Text techCostText;
        [SerializeField] private Text techPrereqText;
        [SerializeField] private Button startResearchBtn;
        [SerializeField] private Button closeTechDetailBtn;
        
        // 框架引用
        private IAdvancedTechSystem mTechSystem;
        private ISurvivalGameModel mGameModel;
        
        // UI状态
        private bool mIsTechPanelOpen = false;
        private Dictionary<string, GameObject> mTechNodeObjects;
        private string mSelectedTechId;
        private string mCurrentResearchTech;
        
        private void Awake()
        {
            // 获取框架组件
            mTechSystem = this.GetSystem<IAdvancedTechSystem>();
            mGameModel = this.GetModel<ISurvivalGameModel>();
            
            // 初始化容器
            mTechNodeObjects = new Dictionary<string, GameObject>();
            
            // 初始化UI状态
            techPanel.SetActive(false);
            techDetailPanel.SetActive(false);
        }
        
        private void Start()
        {
            SetupButtons();
            CreateTechNodes();
            UpdateResearchInfo();
            
            // 注册事件监听
            this.RegisterEvent<TechUnlockedEvent>(OnTechUnlocked);
            this.RegisterEvent<TechResearchStartedEvent>(OnResearchStarted);
            this.RegisterEvent<TechResearchCompletedEvent>(OnResearchCompleted);
            this.RegisterEvent<StartResearchSuccessEvent>(OnStartResearchSuccess);
            this.RegisterEvent<ScientistAssignedEvent>(OnScientistAssigned);
        }
        
        private void Update()
        {
            // 快捷键
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleTechPanel();
            }
            
            // 更新研究信息
            if (mIsTechPanelOpen)
            {
                UpdateResearchInfo();
            }
        }
        
        private void SetupButtons()
        {
            // 主按钮
            techToggleBtn.onClick.AddListener(ToggleTechPanel);
            closeTechBtn.onClick.AddListener(CloseTechPanel);
            
            // 科学家分配按钮
            addScientistBtn.onClick.AddListener(() => {
                var currentAssignment = mGameModel.WorkerAssignment.ContainsKey("Scientist") ? 
                    mGameModel.WorkerAssignment["Scientist"] : new List<string>();
                int currentCount = currentAssignment.Count;
                int newCount = currentCount + 1;
                
                // 发送科学家分配事件
                this.SendEvent(new ScientistAssignedEvent 
                { 
                    RequestedCount = newCount, 
                    AssignedCount = newCount 
                });
                
                Debug.Log($"增加科学家到: {newCount}");
            });
            
            removeScientistBtn.onClick.AddListener(() => {
                var currentAssignment = mGameModel.WorkerAssignment.ContainsKey("Scientist") ? 
                    mGameModel.WorkerAssignment["Scientist"] : new List<string>();
                int currentCount = currentAssignment.Count;
                if (currentCount > 0)
                {
                    int newCount = currentCount - 1;
                    
                    // 发送科学家分配事件
                    this.SendEvent(new ScientistAssignedEvent 
                    { 
                        RequestedCount = newCount, 
                        AssignedCount = newCount 
                    });
                    
                    Debug.Log($"减少科学家到: {newCount}");
                }
            });
            
            // 科技详情按钮
            startResearchBtn.onClick.AddListener(StartSelectedResearch);
            closeTechDetailBtn.onClick.AddListener(() => techDetailPanel.SetActive(false));
        }
        
        private void CreateTechNodes()
        {
            if (techNodePrefab == null)
            {
                Debug.LogError("TechNodePrefab 未设置！");
                return;
            }
            
            // 获取所有科技数据 - 使用TechSystem获取TechNode
            var allTechs = mTechSystem.GetAvailableTechs();
            allTechs.AddRange(mTechSystem.GetResearchedTechs());
            
            // 也获取锁定的科技用于显示
            var techSystemData = mTechSystem.GetTechSystemData();
            
            foreach (var tech in allTechs)
            {
                CreateTechNode(tech);
            }
            
            Debug.Log($"创建了 {mTechNodeObjects.Count} 个科技节点");
        }
        
        private void CreateTechNode(TechNode tech)
        {
            if (mTechNodeObjects.ContainsKey(tech.Id)) return;
            
            GameObject nodeGO = Instantiate(techNodePrefab, techNodeContainer);
            nodeGO.name = $"TechNode_{tech.Id}";
            
            // 设置科技节点组件
            var nodeController = nodeGO.GetComponent<TechNodeUI>();
            if (nodeController == null)
            {
                nodeController = nodeGO.AddComponent<TechNodeUI>();
            }
            
            nodeController.Initialize(tech, this);
            mTechNodeObjects[tech.Id] = nodeGO;
            
            // 设置节点位置（简单的网格布局）
            SetTechNodePosition(nodeGO, tech);
        }
        
        private void SetTechNodePosition(GameObject nodeGO, TechNode tech)
        {
            // 根据科技类别和层级设置位置
            Vector2 basePosition = Vector2.zero;
            
            // 按类别分列
            switch (tech.Category)
            {
                case TechCategory.Production:
                    basePosition.x = -300f;
                    break;
                case TechCategory.Defense:
                    basePosition.x = -100f;
                    break;
                case TechCategory.Medical:
                    basePosition.x = 100f;
                    break;
                case TechCategory.Energy:
                    basePosition.x = 300f;
                    break;
                case TechCategory.Special:
                    basePosition.x = 0f;
                    break;
            }
            
            // 按前置条件数量分层
            int layer = tech.Prerequisites.Count;
            basePosition.y = 100f - (layer * 150f);
            
            nodeGO.GetComponent<RectTransform>().anchoredPosition = basePosition;
        }
        
        private void UpdateResearchInfo()
        {
            if (researchInfoPanel == null) return;
            
            // 更新当前研究
            bool hasCurrentResearch = !string.IsNullOrEmpty(mCurrentResearchTech);
            currentResearchText.text = hasCurrentResearch ? 
                $"正在研究：{mTechSystem.GetTech(mCurrentResearchTech)?.Name ?? "无"}" : 
                "当前研究：无";
            
            // 更新研究进度
            if (hasCurrentResearch)
            {
                float progress = mTechSystem.GetResearchProgress(mCurrentResearchTech);
                researchProgressSlider.value = progress;
                researchProgressSlider.gameObject.SetActive(true);
            }
            else
            {
                researchProgressSlider.gameObject.SetActive(false);
            }
            
            // 更新研究点数
            float researchPoints = mTechSystem.GetResearchPointsPerSecond();
            researchPointsText.text = $"研究速度：{researchPoints}/秒";
            
            // 更新科学家数量
            var scientistAssignment = mGameModel.WorkerAssignment.ContainsKey("Scientist") ? 
                mGameModel.WorkerAssignment["Scientist"] : new List<string>();
            int scientists = scientistAssignment.Count;
            scientistCountText.text = $"科学家：{scientists}";
        }
        
        public void SelectTech(string techId)
        {
            mSelectedTechId = techId;
            ShowTechDetail(techId);
        }
        
        private void ShowTechDetail(string techId)
        {
            var tech = mTechSystem.GetTech(techId);
            if (tech == null) return;
            
            // 更新详情面板信息
            techNameText.text = tech.Name;
            techDescriptionText.text = tech.Description;
            techCostText.text = $"研究成本：{tech.ResearchCost} 点";
            
            // 显示前置条件
            if (tech.Prerequisites.Count > 0)
            {
                string prereqText = "前置科技：\n";
                foreach (var prereq in tech.Prerequisites)
                {
                    var prereqTech = mTechSystem.GetTech(prereq);
                    string status = prereqTech.Status == TechStatus.Researched ? "✓" : "✗";
                    prereqText += $"{status} {prereqTech.Name}\n";
                }
                techPrereqText.text = prereqText;
            }
            else
            {
                techPrereqText.text = "前置科技：无";
            }
            
            // 设置开始研究按钮状态
            bool canResearch = tech.Status == TechStatus.Available;
            startResearchBtn.interactable = canResearch;
            startResearchBtn.GetComponentInChildren<Text>().text = 
                tech.Status == TechStatus.Researched ? "已研究" : 
                canResearch ? "开始研究" : "无法研究";
            
            // 显示详情面板
            techDetailPanel.SetActive(true);
            
            // DOTween动画
            techDetailPanel.transform.localScale = Vector3.zero;
            techDetailPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        
        private void StartSelectedResearch()
        {
            if (string.IsNullOrEmpty(mSelectedTechId)) return;
            
            // 使用默认研究建筑ID，实际项目中应该从UI选择或自动分配
            string defaultResearchBuildingId = "research_lab_1";
            this.SendCommand(new StartResearchCommand(mSelectedTechId, defaultResearchBuildingId));
            techDetailPanel.SetActive(false);
        }
        
        private void ToggleTechPanel()
        {
            if (mIsTechPanelOpen)
            {
                CloseTechPanel();
            }
            else
            {
                OpenTechPanel();
            }
        }
        
        private void OpenTechPanel()
        {
            techPanel.SetActive(true);
            mIsTechPanelOpen = true;
            
            // 更新科技节点状态
            RefreshTechNodes();
            
            // DOTween动画
            techPanel.transform.localScale = Vector3.zero;
            techPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
        
        private void CloseTechPanel()
        {
            // DOTween动画
            techPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                .OnComplete(() => {
                    techPanel.SetActive(false);
                    techDetailPanel.SetActive(false);
                    mIsTechPanelOpen = false;
                });
        }
        
        private void RefreshTechNodes()
        {
            // 刷新所有科技节点的状态
            foreach (var kvp in mTechNodeObjects)
            {
                var tech = mTechSystem.GetTech(kvp.Key);
                var nodeController = kvp.Value.GetComponent<TechNodeUI>();
                nodeController?.UpdateTechState(tech);
            }
        }
        
        #region 事件处理
        
        private void OnTechUnlocked(TechUnlockedEvent e)
        {
            // 科技解锁时的视觉效果
            if (mTechNodeObjects.ContainsKey(e.TechId))
            {
                var nodeGO = mTechNodeObjects[e.TechId];
                var nodeController = nodeGO.GetComponent<TechNodeUI>();
                nodeController?.PlayUnlockAnimation();
            }
            
            Debug.Log($"科技解锁动画：{e.TechName}");
        }
        
        private void OnResearchStarted(TechResearchStartedEvent e)
        {
            mCurrentResearchTech = e.TechId;
            
            // 研究开始的视觉反馈
            if (mTechNodeObjects.ContainsKey(e.TechId))
            {
                var nodeController = mTechNodeObjects[e.TechId].GetComponent<TechNodeUI>();
                nodeController?.PlayResearchingAnimation();
            }
        }
        
        private void OnResearchCompleted(TechResearchCompletedEvent e)
        {
            mCurrentResearchTech = null;
            
            // 研究完成的视觉效果
            if (mTechNodeObjects.ContainsKey(e.TechId))
            {
                var nodeController = mTechNodeObjects[e.TechId].GetComponent<TechNodeUI>();
                nodeController?.PlayCompletedAnimation();
            }
            
            // 刷新所有节点（可能解锁新科技）
            RefreshTechNodes();
        }
        
        private void OnStartResearchSuccess(StartResearchSuccessEvent e)
        {
            // 研究开始成功的UI反馈
            Debug.Log("研究开始成功");
        }
        
        private void OnScientistAssigned(ScientistAssignedEvent e)
        {
            // 科学家分配成功的反馈
            Debug.Log($"科学家分配：{e.AssignedCount}/{e.RequestedCount}");
        }
        
        #endregion
        
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
        
        private void OnDestroy()
        {
            // 取消事件订阅
            this.UnRegisterEvent<TechUnlockedEvent>(OnTechUnlocked);
            this.UnRegisterEvent<TechResearchStartedEvent>(OnResearchStarted);
            this.UnRegisterEvent<TechResearchCompletedEvent>(OnResearchCompleted);
            this.UnRegisterEvent<StartResearchSuccessEvent>(OnStartResearchSuccess);
            this.UnRegisterEvent<ScientistAssignedEvent>(OnScientistAssigned);
        }
    }
    
    /// <summary>
    /// 单个科技节点的UI组件
    /// </summary>
    public class TechNodeUI : MonoBehaviour
    {
        [Header("节点组件")]
        [SerializeField] private Button nodeButton;
        [SerializeField] private Image nodeImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Image progressFill;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject completedIcon;
        
        private TechNode mTech;
        private TechUIController mController;
        
        public void Initialize(TechNode tech, TechUIController controller)
        {
            mTech = tech;
            mController = controller;
            
            // 设置基础信息
            if (nameText != null) nameText.text = tech.Name;
            
            // 设置按钮点击事件
            if (nodeButton != null)
            {
                nodeButton.onClick.AddListener(() => mController.SelectTech(tech.Id));
            }
            
            // 初始化状态
            UpdateTechState(tech);
        }
        
        public void UpdateTechState(TechNode tech)
        {
            if (tech == null) return;
            
            mTech = tech;
            
            // 更新视觉状态
            if (tech.Status == TechStatus.Researched)
            {
                // 已研究状态
                SetNodeColor(Color.green);
                SetLockState(false);
                SetCompletedState(true);
                SetInteractable(false);
            }
            else if (tech.Status == TechStatus.Available)
            {
                // 可研究状态
                SetNodeColor(Color.yellow);
                SetLockState(false);
                SetCompletedState(false);
                SetInteractable(true);
            }
            else
            {
                // 锁定状态
                SetNodeColor(Color.gray);
                SetLockState(true);
                SetCompletedState(false);
                SetInteractable(false);
            }
        }
        
        public void PlayUnlockAnimation()
        {
            // 解锁动画：缩放+闪烁
            transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
            
            if (nodeImage != null)
            {
                nodeImage.DOColor(Color.white, 0.1f).SetLoops(6, LoopType.Yoyo);
            }
        }
        
        public void PlayResearchingAnimation()
        {
            // 研究中动画：进度条填充
            if (progressFill != null)
            {
                progressFill.gameObject.SetActive(true);
                progressFill.DOFillAmount(1f, 2f).SetLoops(-1, LoopType.Restart);
            }
        }
        
        public void PlayCompletedAnimation()
        {
            // 完成动画：绿色闪烁+缩放
            if (progressFill != null)
            {
                progressFill.DOKill();
                progressFill.gameObject.SetActive(false);
            }
            
            transform.DOScale(1.3f, 0.3f).SetLoops(2, LoopType.Yoyo);
            
            if (nodeImage != null)
            {
                nodeImage.DOColor(Color.green, 0.2f).SetLoops(4, LoopType.Yoyo);
            }
        }
        
        private void SetNodeColor(Color color)
        {
            if (nodeImage != null)
            {
                nodeImage.color = color;
            }
        }
        
        private void SetLockState(bool isLocked)
        {
            if (lockIcon != null)
            {
                lockIcon.SetActive(isLocked);
            }
        }
        
        private void SetCompletedState(bool isCompleted)
        {
            if (completedIcon != null)
            {
                completedIcon.SetActive(isCompleted);
            }
        }
        
        private void SetInteractable(bool interactable)
        {
            if (nodeButton != null)
            {
                nodeButton.interactable = interactable;
            }
        }
    }
} 