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
using SurvivalGame.Utils;
namespace SurvivalGame.Controller
{
    /// <summary>
    /// 幸存者管理界面控制器
    /// 功能：显示幸存者列表、工作分配、技能升级等管理界面
    /// 挂载对象：Canvas/SurvivorUI
    /// 依赖系统：ISurvivorSystem（幸存者逻辑）、ISurvivalGameModel（游戏模型）
    /// </summary>
    public class SurvivorUIController : BaseUIController
    {
        [Header("主界面组件")]
        [SerializeField] private GameObject survivorManagePanel;      // 主面板（包含所有子元素）
        [SerializeField] private Button survivorToggleBtn;      // 打开/关闭面板按钮
        [SerializeField] private Button closeSurvivorBtn;       // 关闭面板按钮

        [Header("幸存者列表")]
        [SerializeField] private Transform survivorListContainer; // 列表容器，用于存放每个幸存者的UI项
        [SerializeField] private Text totalSurvivorCountText;   // 显示总人数文本

        [Header("过滤和排序")]
        [SerializeField] private Dropdown professionFilterDropdown; // 职业筛选下拉菜单
        [SerializeField] private Dropdown stateFilterDropdown;      // 状态筛选下拉菜单
        [SerializeField] private Dropdown sortByDropdown;         // 排序列下拉菜单
        [SerializeField] private Toggle sortDescendingToggle;     // 是否降序排列

        [Header("统计信息面板")]
        [SerializeField] private Text aliveSurvivorCountText;     // 显示存活人数
        [SerializeField] private Text workingSurvivorCountText;   // 显示工作中人数
        [SerializeField] private Text idleSurvivorCountText;      // 显示空闲人数
        [SerializeField] private Text averageMoraleText;          // 显示平均士气值
        [SerializeField] private Text averageLevelText;           // 显示平均等级

        [Header("详情面板")]
        [SerializeField] private GameObject survivorDetailPanel;  // 幸存者详细信息面板
        [SerializeField] private Text detailNameText;             // 姓名
        [SerializeField] private Text detailProfessionText;       // 职业
        [SerializeField] private Text detailLevelText;           // 等级
        [SerializeField] private Text detailHealthText;          // 当前健康值
        [SerializeField] private Text detailMoraleText;          // 士气值
        [SerializeField] private Text detailStateText;            // 当前状态
        [SerializeField] private Text detailWorkAssignmentText;  // 工作分配
        [SerializeField] private Button detailCloseBtn;
        [SerializeField] private Button assignWorkBtn;            // 分配工作按钮
        [SerializeField] private Button unassignWorkBtn;          // 解除工作按钮// 关闭详情面板按钮

        [Header("工作分配面板")]
        [SerializeField] private GameObject workAssignmentPanel;  // 工作分配面板
        [SerializeField] private Transform buildingListContainer; // 建筑列表容器
        [SerializeField] private Button workAssignCloseBtn;       // 关闭工作分配面板按钮
        [SerializeField] private Text workAssignTitleText;        // 工作分配标题
        [SerializeField] private Text noBuildingsText;           // 没有可分配建筑的提示

        [Header("技能面板")]
        [SerializeField] private Transform skillContainer;        // 技能容器
        [SerializeField] private GameObject skillItemPrefab;      // 技能项预制体
        [SerializeField] private Text availableSkillPointsText;   // 可用技能点数
        [SerializeField] private Button healBtn;                 // 治疗按钮


        [Header("招募面板")]
        [SerializeField] private GameObject recruitPanel;         // 招募面板
        [SerializeField] private Dropdown recruitProfessionDropdown; // 招募职业选择
        [SerializeField] private Button recruitBtn;               // 招募新幸存者按钮
        [SerializeField] private Text recruitCostText;            // 招募费用文本
        [SerializeField] private Text maxSurvivorText;           // 最大幸存者数量文本

        // 框架引用
        private ISurvivorSystem mSurvivorSystem;                // 幸存者系统接口
        private ISurvivalGameModel mGameModel;                  // 游戏模型接口
        private IObjectPoolSystem  mObjectPoolSystem;
        private IEnhancedBuildingSystem mBuildingSystem;        // 建筑系统接口
        private ConfigSystem mConfigSystem;                     // 配置系统

        // UI状态
        private bool mIsSurvivorPanelOpen = false;              // 面板是否打开
        private string mSelectedSurvivorId = "";                 // 当前选中幸存者ID
        private List<SurvivorData> mFilteredSurvivors = new List<SurvivorData>(); // 过滤后的幸存者列表
        private Dictionary<string, GameObject> mSurvivorItemObjects = new Dictionary<string, GameObject>(); // 缓存每个幸存者的UI对象

        // 过滤器状态
        private SurvivorJob mFilterJob = (SurvivorJob)(-1); // -1表示所有工作类型
        private SurvivorJob mFilterState = (SurvivorJob)(-1); // -1表示所有状态
        private SurvivorProfession mFilterProfession = (SurvivorProfession)(-1); // 添加缺少的变量

        private void Awake()
        {
            // 获取框架组件
            mSurvivorSystem = this.GetSystem<ISurvivorSystem>();
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mObjectPoolSystem = this.GetSystem<IObjectPoolSystem>();
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            mConfigSystem = this.GetSystem<ConfigSystem>();
            // 自动关联UI组件
            FindUIComponents();
            survivorManagePanel.SetActive(true);
        }

        private void Start()
        {
            // 初始化UI
            InitializeUI();

            // 订阅事件
            SubscribeEvents();

            // 初始化数据
            RefreshSurvivorList();
            UpdateStatistics();
            
            survivorManagePanel.SetActive(true);
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            UnsubscribeEvents();
        }

        private void FindUIComponents()
        {
            if (survivorManagePanel == null) survivorManagePanel = this.gameObject;
            if (survivorToggleBtn == null)
                survivorToggleBtn = transform.Find("HeaderPanel/SurvivorToggleBtn").GetComponent<Button>();
            if (closeSurvivorBtn == null)
                closeSurvivorBtn = transform.Find("HeaderPanel/CloseButton").GetComponent<Button>();

            if (survivorListContainer == null)
                survivorListContainer = transform.Find("SurvivorListPanel/Scroll View/Viewport/Content")
                    ?.GetComponent<Transform>();
            if (totalSurvivorCountText == null)
                totalSurvivorCountText = transform.Find("StatsPanel/TotalSurvivorsText")?.GetComponent<Text>();

            // 过滤控件
            if (professionFilterDropdown == null)
                professionFilterDropdown = transform.Find("FilterPanel/ProfessionDropdown")?.GetComponent<Dropdown>();
            if (stateFilterDropdown == null)
                stateFilterDropdown = transform.Find("FilterPanel/StateDropdown")?.GetComponent<Dropdown>();
            if (sortByDropdown == null)
                sortByDropdown = transform.Find("FilterPanel/SortByDropdown")?.GetComponent<Dropdown>();
            if (sortDescendingToggle == null)
                sortDescendingToggle = transform.Find("FilterPanel/SortDescendingToggle")?.GetComponent<Toggle>();

            // 统计信息
            if (aliveSurvivorCountText == null)
                aliveSurvivorCountText = transform.Find("StatsPanel/AliveSurvivorCountText")?.GetComponent<Text>();
            if (workingSurvivorCountText == null)
                workingSurvivorCountText = transform.Find("StatsPanel/WorkingCountText")?.GetComponent<Text>();
            if (idleSurvivorCountText == null)
                idleSurvivorCountText = transform.Find("StatsPanel/IdleCountText")?.GetComponent<Text>();
            if (averageMoraleText == null)
                averageMoraleText = transform.Find("StatsPanel/AverageMoraleText")?.GetComponent<Text>();
            if (averageLevelText == null)
                averageLevelText = transform.Find("StatsPanel/AverageLevelText")?.GetComponent<Text>();

            // 详情面板
            if (survivorDetailPanel == null)
                survivorDetailPanel = transform.Find("SurvivorDetailPanel")?.gameObject;

            if (detailNameText == null)
                detailNameText = transform.Find("SurvivorDetailPanel/NameText")?.GetComponent<Text>();
            if (detailProfessionText == null)
                detailProfessionText = transform.Find("SurvivorDetailPanel/ProfessionText")?.GetComponent<Text>();
            if (detailLevelText == null)
                detailLevelText = transform.Find("SurvivorDetailPanel/LevelText")?.GetComponent<Text>();
            if (detailHealthText == null)
                detailHealthText = transform.Find("SurvivorDetailPanel/HealthText")?.GetComponent<Text>();
            if (detailMoraleText == null)
                detailMoraleText = transform.Find("SurvivorDetailPanel/MoraleText")?.GetComponent<Text>();
            if (detailStateText == null)
                detailStateText = transform.Find("SurvivorDetailPanel/StateText")?.GetComponent<Text>();
            if (detailWorkAssignmentText == null)
                detailWorkAssignmentText =
                    transform.Find("SurvivorDetailPanel/WorkAssignmentText")?.GetComponent<Text>();
            if (detailCloseBtn == null)
                detailCloseBtn = transform.Find("SurvivorDetailPanel/CloseButton")?.GetComponent<Button>();
            if (assignWorkBtn == null)
                assignWorkBtn = transform.Find("SurvivorDetailPanel/AssignWorkBtn")?.GetComponent<Button>();
            if (unassignWorkBtn == null)
                unassignWorkBtn = transform.Find("SurvivorDetailPanel/UnassignWorkBtn")?.GetComponent<Button>();


            
            // 技能面板
            if (skillContainer == null)
                skillContainer = transform.Find("SurvivorSkillPanel/Scroll View/Viewport/skillContainer")
                    ?.GetComponent<Transform>();
            if (availableSkillPointsText == null)
                availableSkillPointsText =
                    transform.Find("SurvivorSkillPanel/AvailableSkillPointsText")?.GetComponent<Text>();
            if (healBtn == null)
                healBtn = transform.Find("SurvivorSkillPanel/HealBtn")?.GetComponent<Button>();
         

            // 招募面板
            if (recruitPanel == null)
                recruitPanel = transform.Find("RecruitPanel")?.gameObject;

            if (recruitProfessionDropdown == null)
                recruitProfessionDropdown =
                    transform.Find("RecruitPanel/RecruitProfessionDropdown")?.GetComponent<Dropdown>();
            if (recruitBtn == null)
                recruitBtn = transform.Find("RecruitPanel/RecruitBtn")?.GetComponent<Button>();
            if (recruitCostText == null)
                recruitCostText = transform.Find("RecruitPanel/RecruitCostText")?.GetComponent<Text>();
            if (maxSurvivorText == null)
                maxSurvivorText = transform.Find("RecruitPanel/MaxSurvivorText")?.GetComponent<Text>();

            // 工作分配面板
            if (workAssignmentPanel == null)
                workAssignmentPanel = transform.Find("WorkAssignmentPanel")?.gameObject;
            if (buildingListContainer == null)
                buildingListContainer = transform.Find("WorkAssignmentPanel/Scroll View/Viewport/BuildingListContainer")?.GetComponent<Transform>();
            if (workAssignCloseBtn == null)
                workAssignCloseBtn = transform.Find("WorkAssignmentPanel/CloseButton")?.GetComponent<Button>();
            if (workAssignTitleText == null)
                workAssignTitleText = transform.Find("WorkAssignmentPanel/TitleText")?.GetComponent<Text>();
            if (noBuildingsText == null)
                noBuildingsText = transform.Find("WorkAssignmentPanel/NoBuildingsText")?.GetComponent<Text>();
        }

        private void InitializeUI()
        {
            // 按钮事件绑定
            if (survivorToggleBtn != null)
                survivorToggleBtn.onClick.AddListener(ToggleSurvivorPanel);
            if (closeSurvivorBtn != null)
                closeSurvivorBtn.onClick.AddListener(CloseSurvivorPanel);
            if (detailCloseBtn != null)
                detailCloseBtn.onClick.AddListener(CloseDetailPanel);
            if (recruitBtn != null)
                recruitBtn.onClick.AddListener(RecruitSurvivor);

            // 分配工作按钮事件绑定
            if (assignWorkBtn != null)
                assignWorkBtn.onClick.AddListener(OnAssignWorkBtnClick);
            if (unassignWorkBtn != null)
                unassignWorkBtn.onClick.AddListener(OnUnassignWorkBtnClick);
            if (workAssignCloseBtn != null)
                workAssignCloseBtn.onClick.AddListener(CloseWorkAssignmentPanel);

            // 过滤器事件绑定
            if (professionFilterDropdown != null)
                professionFilterDropdown.onValueChanged.AddListener(OnProfessionFilterChanged);
            if (stateFilterDropdown != null)
                stateFilterDropdown.onValueChanged.AddListener(OnStateFilterChanged);
            if (sortByDropdown != null)
                sortByDropdown.onValueChanged.AddListener(OnSortChanged);
            if (sortDescendingToggle != null)
                sortDescendingToggle.onValueChanged.AddListener(OnSortOrderChanged);

            // 初始化下拉菜单选项
            InitializeDropdowns();

            // 初始状态设置
            if (survivorManagePanel != null)
                
                survivorManagePanel.SetActive(false);
            if (survivorDetailPanel != null)
                
                survivorDetailPanel.SetActive(false);

            if (workAssignmentPanel != null)
                workAssignmentPanel.SetActive(false);

           // Debug.Log("幸存者UI初始化完成");
        }

        private void InitializeDropdowns()
        {
            // 职业过滤下拉菜单
            if (professionFilterDropdown != null)
            {
                professionFilterDropdown.options.Clear();
                professionFilterDropdown.options.Add(new Dropdown.OptionData("所有职业"));

                foreach (SurvivorProfession profession in System.Enum.GetValues(typeof(SurvivorProfession)))
                {
                    string professionName = GetProfessionDisplayName(profession);
                    professionFilterDropdown.options.Add(new Dropdown.OptionData(professionName));
                }
                professionFilterDropdown.value = 0;
            }

            // 状态过滤下拉菜单
            if (stateFilterDropdown != null)
            {
                stateFilterDropdown.options.Clear();
                stateFilterDropdown.options.Add(new Dropdown.OptionData("所有状态"));

                foreach (SurvivorState state in System.Enum.GetValues(typeof(SurvivorState)))
                {
                    string stateName = GetStateDisplayName(state);
                    stateFilterDropdown.options.Add(new Dropdown.OptionData(stateName));
                }
                stateFilterDropdown.value = 0;
            }

            // 排序下拉菜单
            if (sortByDropdown != null)
            {
                sortByDropdown.options.Clear();
                sortByDropdown.options.Add(new Dropdown.OptionData("姓名"));
                sortByDropdown.options.Add(new Dropdown.OptionData("等级"));
                sortByDropdown.options.Add(new Dropdown.OptionData("健康"));
                sortByDropdown.options.Add(new Dropdown.OptionData("士气"));
                sortByDropdown.options.Add(new Dropdown.OptionData("职业"));
                sortByDropdown.value = 0;
            }

            // 招募职业下拉菜单
            if (recruitProfessionDropdown != null)
            {
                recruitProfessionDropdown.options.Clear();

                foreach (SurvivorProfession profession in System.Enum.GetValues(typeof(SurvivorProfession)))
                {
                    string professionName = GetProfessionDisplayName(profession);
                    recruitProfessionDropdown.options.Add(new Dropdown.OptionData(professionName));
                }
                recruitProfessionDropdown.value = 0;
            }
        }

        private void SubscribeEvents()
        {
            if (mSurvivorSystem != null)
            {
                mSurvivorSystem.OnSurvivorAdded += OnSurvivorAdded;
                mSurvivorSystem.OnSurvivorRemoved += OnSurvivorRemoved;
                mSurvivorSystem.OnSurvivorAssigned += OnSurvivorAssigned;
                mSurvivorSystem.OnSurvivorUnassigned += OnSurvivorUnassigned;
                mSurvivorSystem.OnSurvivorLevelUp += OnSurvivorLevelUp;
            }
        }

        private void UnsubscribeEvents()
        {
            if (mSurvivorSystem != null)
            {
                mSurvivorSystem.OnSurvivorAdded -= OnSurvivorAdded;
                mSurvivorSystem.OnSurvivorRemoved -= OnSurvivorRemoved;
                mSurvivorSystem.OnSurvivorAssigned -= OnSurvivorAssigned;
                mSurvivorSystem.OnSurvivorUnassigned -= OnSurvivorUnassigned;
                mSurvivorSystem.OnSurvivorLevelUp -= OnSurvivorLevelUp;
            }
        }

        private void Update()
        {
            // 定期更新UI显示
            if (mIsSurvivorPanelOpen && Time.frameCount % 30 == 0) // 每秒刷新2次
            {
                UpdateStatistics();
                RefreshSurvivorItems();
            }
        }

        public void ToggleSurvivorPanel()
        {
            if (mIsSurvivorPanelOpen)
            {
                CloseSurvivorPanel();
            }
            else
            {
                OpenSurvivorPanel();
            }
        }

        public void OpenSurvivorPanel()
        {
            if (survivorManagePanel == null) return;

            survivorManagePanel.SetActive(true);
            mIsSurvivorPanelOpen = true;

            // 刷新数据
            RefreshSurvivorList();
            UpdateStatistics();

            // 使用 DOTween 添加打开动画
            survivorManagePanel.transform.localScale = Vector3.zero;
            survivorManagePanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            Debug.Log("打开幸存者管理面板");
        }

        public void CloseSurvivorPanel()
        {
            if (survivorManagePanel == null) return;

            // 使用 DOTween 添加关闭动画
            survivorManagePanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                .OnComplete(() => {
                    survivorManagePanel.SetActive(false);
                    CloseDetailPanel();
                    mIsSurvivorPanelOpen = false;
                });

            Debug.Log("关闭幸存者管理面板");
        }

        private void CloseDetailPanel()
        {
            if (survivorDetailPanel != null)
            {
                survivorDetailPanel.SetActive(false);
                mSelectedSurvivorId = "";
            }
        }

        private void RefreshSurvivorList()
        {
            Debug.Log("刷新幸存者列表");
            if (mSurvivorSystem == null) return;

            // 获取所有幸存者
            var allSurvivors = mSurvivorSystem.GetAllSurvivors();

            // 应用过滤器
            mFilteredSurvivors = FilterSurvivors(allSurvivors);

            // 应用排序
            SortSurvivors(mFilteredSurvivors);

            // 更新UI显示
            UpdateSurvivorListUI();

            // 更新计数显示
            if (totalSurvivorCountText != null)
                totalSurvivorCountText.text = $"幸存者总数：{allSurvivors.Count}";
        }

        private List<SurvivorData> FilterSurvivors(List<SurvivorData> survivors)
        {
            var filtered = survivors.AsEnumerable();

            // 工作类型过滤
            if (mFilterJob != (SurvivorJob)(-1))
            {
                filtered = filtered.Where(s => s.CurrentJob == mFilterJob);
            }

            // 状态过滤（使用工作状态）
            if (mFilterState != (SurvivorJob)(-1))
            {
                filtered = filtered.Where(s => s.CurrentJob == mFilterState);
            }

            return filtered.ToList();
        }

        private void SortSurvivors(List<SurvivorData> survivors)
        {
            if (sortByDropdown == null) return;

            bool descending = sortDescendingToggle != null && sortDescendingToggle.isOn;

            switch (sortByDropdown.value)
            {
                case 0: // 姓名
                    survivors.Sort((a, b) => descending ? 
                        string.Compare(b.Name, a.Name) : 
                        string.Compare(a.Name, b.Name));
                    break;
                case 1: // 等级
                    survivors.Sort((a, b) => descending ? 
                        b.Level.CompareTo(a.Level) : 
                        a.Level.CompareTo(b.Level));
                    break;
                case 2: // 健康
                    survivors.Sort((a, b) => descending ? 
                        b.Health.CompareTo(a.Health) : 
                        a.Health.CompareTo(b.Health));
                    break;
                case 3: // 士气
                    survivors.Sort((a, b) => descending ? 
                        b.Morale.CompareTo(a.Morale) : 
                        a.Morale.CompareTo(b.Morale));
                    break;
                case 4: // 工作类型
                    survivors.Sort((a, b) => descending ? 
                        b.CurrentJob.CompareTo(a.CurrentJob) : 
                        a.CurrentJob.CompareTo(b.CurrentJob));
                    break;
            }
        }

        private void UpdateSurvivorListUI()
        {
            Debug.Log("更新幸存者信息");
            if (survivorListContainer == null) return;

            // 清除现有的UI项
            ClearSurvivorItems();

            // 为每位幸存者创建一个列表项
            foreach (var survivor in mFilteredSurvivors)
            {
                Debug.Log("幸存者信息"+survivor);
                CreateSurvivorItem(survivor);
            }
        }

        private void ClearSurvivorItems()
        {
            foreach (var kvp in mSurvivorItemObjects)
            {
                if (kvp.Value != null)
                    DestroyImmediate(kvp.Value);
            }
            mSurvivorItemObjects.Clear(); // 清空缓存
        }

        private void CreateSurvivorItem(SurvivorData survivor)
        {
            if (survivorListContainer == null) return;

            // 实例化幸存者项
            //GameObject itemObj = Instantiate(survivorItemPrefab, survivorListContainer);

            GameObject itemObj = mObjectPoolSystem.Spanw("UI/Item/SurvivorItem_PF").gameObject;
            itemObj.transform.SetParent(survivorListContainer);
            mSurvivorItemObjects[survivor.Id] = itemObj;

            // 获取组件并设置数据
            var itemController = itemObj.GetComponent<SurvivorItemUI>();
            if (itemController != null)
            {
                itemController.SetSurvivorData(survivor);
                itemController.OnItemClicked = OnSurvivorItemClicked;
            }
            else
            {
                // 如果没有专门的组件，直接设置基本信息
                SetSurvivorItemData(itemObj, survivor);
            }
        }

        private void SetSurvivorItemData(GameObject itemObj, SurvivorData survivor)
        {
            // 基本信息
            var nameText = itemObj.transform.Find("NameText")?.GetComponent<Text>();
            if (nameText != null) nameText.text = survivor.Name;

            var professionText = itemObj.transform.Find("ProfessionText")?.GetComponent<Text>();
            if (professionText != null) professionText.text = GetJobDisplayName(survivor.CurrentJob);

            var levelText = itemObj.transform.Find("LevelText")?.GetComponent<Text>();
            if (levelText != null) levelText.text = $"Lv.{survivor.Level}";

            var stateText = itemObj.transform.Find("StateText")?.GetComponent<Text>();
            if (stateText != null) stateText.text = GetJobDisplayName(survivor.CurrentJob);

            // 血量条
            var healthSlider = itemObj.transform.Find("HealthSlider")?.GetComponent<Slider>();
            if (healthSlider != null)
            {
                healthSlider.value = survivor.Health / 100f; // 健康值范围0-100
            }

            // 士气条
            var moraleSlider = itemObj.transform.Find("MoraleSlider")?.GetComponent<Slider>();
            if (moraleSlider != null)
            {
                moraleSlider.value = survivor.Morale / 100f;
            }

            // 点击事件
            var button = itemObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnSurvivorItemClicked(survivor.Id));
            }
        }

        private void RefreshSurvivorItems()
        {
            if (mSurvivorSystem == null) return;

            foreach (var kvp in mSurvivorItemObjects)
            {
                var survivor = mSurvivorSystem.GetSurvivor(kvp.Key);
                if (survivor != null && kvp.Value != null)
                {
                    SetSurvivorItemData(kvp.Value, survivor);
                }
            }
        }

        private void OnSurvivorItemClicked(string survivorId)
        {
            mSelectedSurvivorId = survivorId;
            ShowSurvivorDetail(survivorId);
        }

        private void ShowSurvivorDetail(string survivorId)
        {
            var survivor = mSurvivorSystem.GetSurvivor(survivorId);
            if (survivor == null || survivorDetailPanel == null) return;

            // 更新详情面板信息
            if (detailNameText != null) detailNameText.text = survivor.Name;
            if (detailProfessionText != null) detailProfessionText.text = GetJobDisplayName(survivor.CurrentJob);
            if (detailLevelText != null) detailLevelText.text = $"等级：{survivor.Level}";
            if (detailHealthText != null) detailHealthText.text = $"健康：{survivor.Health:F0}/100";
            if (detailMoraleText != null) detailMoraleText.text = $"士气：{survivor.Morale:F0}/100";
            if (detailStateText != null) detailStateText.text = $"状态：{GetJobDisplayName(survivor.CurrentJob)}";
            if (detailWorkAssignmentText != null)
            {
                detailWorkAssignmentText.text = string.IsNullOrEmpty(survivor.AssignedBuildingId) ? 
                    "工作分配：无" : $"工作分配：{survivor.AssignedBuildingId}";
            }

            // 更新技能信息
            UpdateSkillDisplay(survivor);

            // 显示详情面板
            survivorDetailPanel.SetActive(true);

            // 使用 DOTween 添加打开动画
            survivorDetailPanel.transform.localScale = Vector3.zero;
            survivorDetailPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            Debug.Log($"显示幸存者详情：{survivor.Name}");
        }

        private void UpdateSkillDisplay(SurvivorData survivor)
        {
            if (skillContainer == null || skillItemPrefab == null) return;

            // 清除现有技能项
            for (int i = skillContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(skillContainer.GetChild(i).gameObject);
            }

            // 创建技能项
            foreach (SurvivorAttributeType attributeType in System.Enum.GetValues(typeof(SurvivorAttributeType)))
            {
                int skillLevel = survivor.GetAttributeValue(attributeType);
                CreateSkillItem(attributeType, skillLevel, survivor.Experience > 0);
            }

            // 更新技能点显示
            if (availableSkillPointsText != null)
                availableSkillPointsText.text = $"可用经验：{survivor.Experience}";
        }

        private void CreateSkillItem(SurvivorAttributeType attributeType, int level, bool canUpgrade)
        {
            GameObject skillItemObj = Instantiate(skillItemPrefab, skillContainer);

            // 设置技能信息
            var skillNameText = skillItemObj.transform.Find("SkillNameText")?.GetComponent<Text>();
            if (skillNameText != null) skillNameText.text = GetAttributeDisplayName(attributeType);

            var skillLevelText = skillItemObj.transform.Find("SkillLevelText")?.GetComponent<Text>();
            if (skillLevelText != null) skillLevelText.text = $"等级：{level}";

            // 升级按钮
            var upgradeBtn = skillItemObj.transform.Find("UpgradeBtn")?.GetComponent<Button>();
            if (upgradeBtn != null)
            {
                upgradeBtn.interactable = canUpgrade;
                upgradeBtn.onClick.RemoveAllListeners();
                upgradeBtn.onClick.AddListener(() => UpgradeSkill(attributeType));
            }
        }

        private void UpgradeSkill(SurvivorAttributeType attributeType)
        {
            if (string.IsNullOrEmpty(mSelectedSurvivorId)) return;

            bool success = mSurvivorSystem.UpgradeSurvivorAttribute(mSelectedSurvivorId, attributeType);
            if (success)
            {
                // 刷新技能显示
                var survivor = mSurvivorSystem.GetSurvivor(mSelectedSurvivorId);
                if (survivor != null)
                    UpdateSkillDisplay(survivor);

                Debug.Log($"技能升级成功：{GetAttributeDisplayName(attributeType)}");
            }
            else
            {
                Debug.LogWarning("技能升级失败：经验不足");
            }
        }

        private void RecruitSurvivor()
        {
            if (recruitProfessionDropdown == null) return;

            // 获取选择的职业
            var professions = System.Enum.GetValues(typeof(SurvivorProfession));
            if (recruitProfessionDropdown.value < professions.Length)
            {
                SurvivorProfession selectedProfession = (SurvivorProfession)professions.GetValue(recruitProfessionDropdown.value);

                // 招募新幸存者
                Vector3 spawnPosition = Vector3.zero; // 可以设置为营地位置
                bool success = mSurvivorSystem.AddSurvivor(selectedProfession, spawnPosition);

                if (success)
                {
                    RefreshSurvivorList();
                    UpdateStatistics();
                    Debug.Log($"招募成功：{GetProfessionDisplayName(selectedProfession)}");
                }
                else
                {
                    Debug.LogWarning("招募失败：可能已达到最大人数");
                }
            }
        }

        private void UpdateStatistics()
        {
            if (mSurvivorSystem == null) return;

            var systemData = mSurvivorSystem.GetSystemData();

            // 更新统计显示
            if (aliveSurvivorCountText != null)
                aliveSurvivorCountText.text = $"存活：{mSurvivorSystem.GetAliveSurvivorCount()}";

            if (workingSurvivorCountText != null)
            {
                int workingCount = mSurvivorSystem.GetAllSurvivors().Count(s => s.CurrentJob != SurvivorJob.Idle);
                workingSurvivorCountText.text = $"工作中：{workingCount}";
            }

            if (idleSurvivorCountText != null)
            {
                int idleCount = mSurvivorSystem.GetAvailableWorkers().Count;
                idleSurvivorCountText.text = $"空闲：{idleCount}";
            }

            if (averageMoraleText != null)
                averageMoraleText.text = $"平均士气：{mSurvivorSystem.GetAverageTeamMorale():F1}";

            if (averageLevelText != null)
            {
                float avgLevel = (float)mSurvivorSystem.GetAllSurvivors().Where(s => s.IsAlive).Average(s => s.Level);
                averageLevelText.text = $"平均等级：{avgLevel:F1}";
            }

            // 更新招募信息
            if (maxSurvivorText != null)
                maxSurvivorText.text = $"最大人数：{systemData.maxSurvivors}";
        }

        #region 事件处理

        private void OnSurvivorAdded(string survivorId)
        {
            RefreshSurvivorList();
            UpdateStatistics();
        }

        private void OnSurvivorRemoved(string survivorId)
        {
            // 如果当前选中的幸存者被移除，关闭详情面板
            if (mSelectedSurvivorId == survivorId)
            {
                CloseDetailPanel();
            }

            RefreshSurvivorList();
            UpdateStatistics();
        }

        private void OnSurvivorAssigned(string survivorId, string buildingId)
        {
            // 刷新幸存者列表项显示
            RefreshSurvivorItems();
            
            // 更新统计信息（工作中人数、空闲人数等）
            UpdateStatistics();
            
            // 如果当前选中的幸存者被分配了工作，刷新详情面板
            if (mSelectedSurvivorId == survivorId)
            {
                ShowSurvivorDetail(survivorId);
            }
            
            Debug.Log($"UI已更新：幸存者 {survivorId} 被分配到建筑 {buildingId}");
        }

        private void OnSurvivorUnassigned(string survivorId)
        {
            // 刷新幸存者列表项显示
            RefreshSurvivorItems();
            
            // 更新统计信息（工作中人数、空闲人数等）
            UpdateStatistics();
            
            // 如果当前选中的幸存者取消了工作分配，刷新详情面板
            if (mSelectedSurvivorId == survivorId)
            {
                ShowSurvivorDetail(survivorId);
            }
            
            Debug.Log($"UI已更新：幸存者 {survivorId} 取消了工作分配");
        }

        private void OnSurvivorLevelUp(string survivorId)
        {
            // 显示升级特效
            Debug.Log($"幸存者升级：{survivorId}");

            // 如果是当前选中的幸存者，刷新详情面板
            if (mSelectedSurvivorId == survivorId)
            {
                ShowSurvivorDetail(survivorId);
            }
        }

        private void OnProfessionFilterChanged(int value)
        {
            mFilterProfession = value == 0 ? (SurvivorProfession)(-1) : (SurvivorProfession)(value - 1);
            RefreshSurvivorList();
        }

        private void OnStateFilterChanged(int value)
        {
            mFilterState = value == 0 ? (SurvivorJob)(-1) : (SurvivorJob)(value - 1);
            RefreshSurvivorList();
        }

        private void OnSortChanged(int value)
        {
            RefreshSurvivorList();
        }

        private void OnSortOrderChanged(bool value)
        {
            RefreshSurvivorList();
        }

        #endregion

        #region 辅助方法

        private string GetProfessionDisplayName(SurvivorProfession profession)
        {
            switch (profession)
            {
                case SurvivorProfession.Civilian: return "平民";
                case SurvivorProfession.Soldier: return "士兵";
                case SurvivorProfession.Engineer: return "工程师";
                case SurvivorProfession.Doctor: return "医生";
                case SurvivorProfession.Scientist: return "科学家";
                case SurvivorProfession.Scout: return "侦察员";
                case SurvivorProfession.Worker: return "工人";
                case SurvivorProfession.Guard: return "守卫";
                default: return profession.ToString();
            }
        }

        private string GetStateDisplayName(SurvivorState state)
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

        private string GetAttributeDisplayName(SurvivorAttributeType attributeType)
        {
            switch (attributeType)
            {
                case SurvivorAttributeType.Production: return "生产力";
                case SurvivorAttributeType.Combat: return "战斗力";
                case SurvivorAttributeType.Technology: return "技术力";
                case SurvivorAttributeType.Medical: return "医疗力";
                case SurvivorAttributeType.Research: return "科研力";
                case SurvivorAttributeType.Exploration: return "探索力";
                case SurvivorAttributeType.Leadership: return "领导力";
                default: return attributeType.ToString();
            }
        }

        #endregion

        #region 工作分配方法

        /// <summary>
        /// 点击分配工作按钮
        /// </summary>
        private void OnAssignWorkBtnClick()
        {
            if (string.IsNullOrEmpty(mSelectedSurvivorId))
            {
                Debug.LogWarning("没有选中幸存者");
                return;
            }

            var survivor = mSurvivorSystem.GetSurvivor(mSelectedSurvivorId);
            if (survivor == null)
            {
                Debug.LogWarning("幸存者不存在");
                return;
            }

            // 检查幸存者是否可以工作
            if (!survivor.CanWork)
            {
                Debug.LogWarning($"幸存者{survivor.Name}当前无法工作（可能受伤或状态不佳）");
                return;
            }

            // 打开工作分配面板
            ShowWorkAssignmentPanel(survivor);
        }

        /// <summary>
        /// 点击取消分配工作按钮
        /// </summary>
        private void OnUnassignWorkBtnClick()
        {
            if (string.IsNullOrEmpty(mSelectedSurvivorId))
            {
                Debug.LogWarning("没有选中幸存者");
                return;
            }

            bool success = mSurvivorSystem.UnassignSurvivorFromBuilding(mSelectedSurvivorId);
            if (success)
            {
                Debug.Log("成功取消工作分配");
                // 注意：这里不需要手动刷新UI，因为OnSurvivorUnassigned事件会自动处理
            }
            else
            {
                Debug.LogWarning("取消工作分配失败");
            }
        }

        /// <summary>
        /// 显示工作分配面板
        /// </summary>
        private void ShowWorkAssignmentPanel(SurvivorData survivor)
        {
            if (workAssignmentPanel == null)
            {
                Debug.LogWarning("工作分配面板未设置");
                return;
            }

            // 设置标题
            if (workAssignTitleText != null)
                workAssignTitleText.text = $"为 {survivor.Name} 分配工作";

            // 获取可分配的建筑
            var availableBuildings = GetAvailableBuildingsForSurvivor(survivor);

            // 清空现有列表
            ClearBuildingList();

            // 显示建筑列表
            if (availableBuildings.Count > 0)
            {
                if (noBuildingsText != null)
                    noBuildingsText.gameObject.SetActive(false);

                foreach (var building in availableBuildings)
                {
                    CreateBuildingItem(building, survivor);
                }
            }
            else
            {
                if (noBuildingsText != null)
                {
                    noBuildingsText.gameObject.SetActive(true);
                    noBuildingsText.text = "没有适合的建筑可供分配";
                }
            }

            // 显示面板
            workAssignmentPanel.SetActive(true);

            // 添加打开动画
            workAssignmentPanel.transform.localScale = Vector3.zero;
            workAssignmentPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// 关闭工作分配面板
        /// </summary>
        private void CloseWorkAssignmentPanel()
        {
            if (workAssignmentPanel != null)
            {
                workAssignmentPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                    .OnComplete(() => workAssignmentPanel.SetActive(false));
            }
        }

        /// <summary>
        /// 获取幸存者可分配的建筑列表
        /// </summary>
        private List<BuildingData> GetAvailableBuildingsForSurvivor(SurvivorData survivor)
        {
            var availableBuildings = new List<BuildingData>();

            if (mBuildingSystem == null)
            {
                Debug.LogWarning("建筑系统未初始化");
                return availableBuildings;
            }

            // 获取所有可运行的建筑
            var allBuildings = mBuildingSystem.GetAllBuildings();
            var operationalBuildings = allBuildings.Where(b => mBuildingSystem.IsBuildingOperational(b.Id));

            foreach (var building in operationalBuildings)
            {
                // 检查建筑是否适合该幸存者
                if (IsBuildingSuitableForSurvivor(building, survivor))
                {
                    availableBuildings.Add(building);
                }
            }

            return availableBuildings;
        }

        /// <summary>
        /// 检查建筑是否适合该幸存者
        /// </summary>
        private bool IsBuildingSuitableForSurvivor(BuildingData building, SurvivorData survivor)
        {
            var config = mConfigSystem?.GetBuildingConfig(building.ConfigId);
            if (config == null) return false;

            // 移除职业限制，所有幸存者都可以分配到任何建筑
            return true;
        }

        /// <summary>
        /// 清空建筑列表
        /// </summary>
        private void ClearBuildingList()
        {
            if (buildingListContainer == null) return;

            for (int i = buildingListContainer.childCount - 1; i >= 0; i--)
            {
                var child = buildingListContainer.GetChild(i);
                if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 创建建筑项列表
        /// </summary>
        private void CreateBuildingItem(BuildingData building, SurvivorData survivor)
        {
            if (buildingListContainer == null) return;

            // 创建建筑项UI
            GameObject buildingItem = mObjectPoolSystem.Spanw("UI/Item/BuildingWorkItem_PF").gameObject;
            buildingItem.transform.SetParent(buildingListContainer);
            buildingItem.transform.localScale = Vector3.one;

            // 设置建筑信息
            SetBuildingItemData(buildingItem, building, survivor);
        }

        /// <summary>
        /// 设置建筑项数据
        /// </summary>
        private void SetBuildingItemData(GameObject buildingItem, BuildingData building, SurvivorData survivor)
        {
            var config = mConfigSystem?.GetBuildingConfig(building.ConfigId);
            if (config == null) return;
            
            // 设置建筑名称
            var nameText = buildingItem.transform.Find("BuildingNameText")?.GetComponent<Text>();
            if (nameText != null) nameText.text = config.Name;

            // 设置建筑类型
            var typeText = buildingItem.transform.Find("BuildingTypeText")?.GetComponent<Text>();
            if (typeText != null) typeText.text = GetBuildingCategoryDisplayName(config.Category);

            // 设置建筑效率
            var efficiencyText = buildingItem.transform.Find("EfficiencyText")?.GetComponent<Text>();
            if (efficiencyText != null)
            {
                float efficiency = mBuildingSystem.GetBuildingEfficiency(building.Id);
                efficiencyText.text = $"效率: {efficiency:P0}";
            }

            // 设置当前工人数
            var workerText = buildingItem.transform.Find("WorkerCountText")?.GetComponent<Text>();
            if (workerText != null)
            {
                var workers = mSurvivorSystem.GetBuildingWorkers(building.Id.ToString());
                int maxWorkers = GetMaxWorkersForBuilding(building.ConfigId);
                workerText.text = $"工人: {workers.Count}/{maxWorkers}";
            }

            // 设置建筑描述
            var descText = buildingItem.transform.Find("BuildingDescText")?.GetComponent<Text>();
            if (descText != null)
            {
                descText.text = config.Description;
            }

            // 设置职业要求
            var reqText = buildingItem.transform.Find("RequirementText")?.GetComponent<Text>();
            if (reqText != null)
            {
                reqText.text = GetBuildingProfessionRequirement(building.ConfigId);
            }

            // 设置分配按钮
            var assignButton = buildingItem.transform.Find("AssignButton")?.GetComponent<Button>();
            if (assignButton != null)
            {
                var workers = mSurvivorSystem.GetBuildingWorkers(building.Id.ToString());
                int maxWorkers = GetMaxWorkersForBuilding(building.ConfigId);
                bool canAssign = workers.Count < maxWorkers;

                assignButton.interactable = canAssign;
                assignButton.onClick.RemoveAllListeners();
                assignButton.onClick.AddListener(() => AssignSurvivorToBuilding(survivor.Id, building.ConfigId.ToString(),building.Id));

                var buttonText = assignButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = canAssign ? "分配" : "已满";
                }
            }
        }

        /// <summary>
        /// 获取建筑的最大工人数
        /// </summary>
        private int GetMaxWorkersForBuilding(string buildingId)
        {
            switch (buildingId)
            {
                case "Farm": return 2;
                case "Workshop": return 3;
                case "MedicalStation": return 1;
                case "WatchTower": return 2;
                case "Library": return 1;
                case "Quarry": return 4;
                case "Shelter": return 1;
                default: return 2;
            }
        }

        /// <summary>
        /// 获取建筑职业要求描述
        /// </summary>
        private string GetBuildingProfessionRequirement(string buildingId)
        {
            switch (buildingId)
            {
                case "Farm": return "推荐：平民、工人（效率+20%）";
                case "Workshop": return "推荐：工程师、工人（效率+20%）";
                case "MedicalStation": return "推荐：医生（效率+30%）";
                case "WatchTower": return "推荐：士兵、侦察员、守卫（效率+20%）";
                case "Library": return "推荐：科学家（效率+30%）";
                case "Quarry": return "推荐：工人、工程师（效率+20%）";
                default: return "所有职业均可工作";
            }
        }

        /// <summary>
        /// 分配幸存者到建筑
        /// </summary>
        private void AssignSurvivorToBuilding(string survivorId,string buildingConfigId, string buildingId)
        {
            bool success = mSurvivorSystem.AssignSurvivorToBuilding(survivorId,buildingConfigId, buildingId);
            if (success)
            {
                // 关闭工作分配面板
                CloseWorkAssignmentPanel();

                Debug.Log($"成功分配幸存者到建筑 {buildingId}");
                // 注意：这里不需要手动刷新UI，因为OnSurvivorAssigned事件会自动处理
            }
            else
            {
                Debug.LogWarning("分配工作失败");
            }
        }

        /// <summary>
        /// 获取建筑类型显示名称
        /// </summary>
        private string GetBuildingCategoryDisplayName(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Production: return "生产类";
                case BuildingCategory.Defense: return "防御类";
                case BuildingCategory.Habitat: return "居住类";
                case BuildingCategory.Storage: return "储存类";
                case BuildingCategory.Functional: return "功能类";
                default: return category.ToString();
            }
        }

        #endregion

        public IArchitecture GetArchitecture()
        {
            return RegisterManager.Interface;
        }
    }


   
}
