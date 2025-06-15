// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：GameUIController.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了游戏主界面的控制器 (GameUIController)。
//     它负责管理和更新游戏中的各种UI元素，如资源显示、生产效率、
//     僵尸威胁等级、防御/科技/建筑系统状态、建造菜单以及游戏控制按钮等。
//     该控制器与多个游戏系统和数据模型交互，并响应游戏事件来刷新UI。
// ==============================================================================

using UnityEngine;
using UnityEngine.UI; // Unity UI命名空间
using QFramework;      // QFramework框架
using MyGameNamespace; // 自定义命名空间，包含事件定义
using SurvivalGame.Model; // 游戏数据模型
using SurvivalGame;       // 可能包含一些通用定义
using SurvivalGame.Command; // 游戏指令
using DG.Tweening;    // DoTween动画库
using System.Linq;    // LINQ库，用于数据查询
using SurvivalGame.GameSystem; // 游戏系统接口

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 游戏主界面UI控制器。
    /// 管理游戏界面上各种信息的显示和用户交互，例如资源、建造菜单、系统状态等。
    /// 通常挂载在场景中的主Canvas下的一个UI根对象上（例如 "Canvas/GameUI"）。
    /// 依赖于 ISurvivalGameModel, IResourceSystem, IEnhancedBuildingSystem 等核心游戏组件。
    /// </summary>
    public class GameUIController : MonoBehaviour, IController, ICanSendEvent // 实现QFramework接口
    {
        [Header("顶部资源条显示")] // Inspector中分组显示
        [SerializeField] private Text foodText;             // 食物数量文本
        [SerializeField] private Text waterText;            // 水资源数量文本
        [SerializeField] private Text materialsText;        // 建筑材料数量文本
        [SerializeField] private Text ammunitionText;       // 弹药数量文本
        [SerializeField] private Text populationText;       // 人口数量文本
        [SerializeField] private Text moraleText;           // 士气值文本
        [SerializeField] private Text dayText;              // 当前游戏天数文本
        [SerializeField] private Text energyText;           // 能源数量文本
        [SerializeField] private Text researchPointsText;   // 科研点数文本
        
        [Header("资源生产效率显示")]
        [SerializeField] private Text foodProductionText;      // 食物每分钟生产效率文本
        [SerializeField] private Text ammoProductionText;      // 弹药每分钟生产效率文本
        [SerializeField] private Text materialsProductionText; // 建材每分钟生产效率文本
        [SerializeField] private Text totalProductionText;     // 总生产效率概览文本

        [Header("僵尸威胁等级显示")]
        [SerializeField] private Text threatLevelText;      // 当前僵尸威胁等级文本
        [SerializeField] private Text zombieCountText;      // 当前活跃僵尸数量文本
        [SerializeField] private GameObject threatWarningPanel; // 高威胁警告提示面板

        [Header("防御系统状态显示")]
        [SerializeField] private Text towerCountText;       // 防御塔数量/状态文本
        [SerializeField] private Text towerStatsText;       // 防御塔综合统计文本（如击杀、伤害）

        [Header("科技系统状态显示")]
        [SerializeField] private Text currentResearchText;  // 当前正在研究的科技名称文本
        [SerializeField] private Text researchProgressText; // 当前研究进度及预计时间文本

        [Header("建筑系统状态显示")]
        [SerializeField] private Text buildingCountText;    // 建筑总数/运营中数量文本
        [SerializeField] private Text buildingStatsText;    // 各类型建筑数量统计文本
        [SerializeField] private Text buildingProductionText; // 建筑总产出概览文本

        [Header("建造菜单UI")]
        [SerializeField] private GameObject buildMenuPanel;     // 建造菜单的主面板GameObject
        [SerializeField] private Button buildMenuToggleBtn;   // 打开/关闭建造菜单的按钮
        [SerializeField] private Transform buildButtonContainer; // 容纳动态生成的建造按钮的容器Transform

        [Header("游戏控制按钮")]
        [SerializeField] private Button pauseBtn;           // 暂停/继续游戏按钮
        [SerializeField] private Text pauseBtnText;         // 暂停按钮上显示的文本 ("暂停" 或 "继续")
        [SerializeField] private Button techBtn;            // 打开科技树界面的按钮
        [SerializeField] private Button survivorBtn;        // 打开幸存者管理界面的按钮

        // QFramework及游戏核心系统引用
        private ISurvivalGameModel mSurvivalGameModel;  // 游戏数据模型
        private IResourceSystem mResourceSystem;        // 资源系统
        private IEnhancedBuildingSystem mBuildingSystem; // 增强型建筑系统
        private IZombieSystem mZombieSystem;            // 僵尸系统
        private IDefenseSystem mDefenseSystem;          // 防御系统
        private IAdvancedTechSystem mTechSystem;        // 高级科技系统
        private ConfigSystem mConfigSystem;             // 配置数据系统

        // 建造模式相关状态变量
        private bool mInBuildMode = false;      // 当前是否处于建筑放置模式
        private string mSelectedBuildingId = "";// 当前选中的待放置建筑的ID

        // UI内部状态
        private bool mBuildMenuOpen = false;    // 建造菜单是否已打开

        // 生产效率显示更新计时器
        private float mLastProductionDisplayUpdate = 0f; // 上次更新生产效率显示的时间戳
        private const float PRODUCTION_DISPLAY_UPDATE_INTERVAL = 2f; // 生产效率显示的更新间隔（秒）
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被创建时调用。
        /// 用于获取QFramework框架中的模型和系统实例，并自动关联UI组件。
        /// </summary>
        private void Awake()
        {
            // 获取QFramework框架组件
            mSurvivalGameModel = this.GetModel<ISurvivalGameModel>();
            mResourceSystem = this.GetSystem<IResourceSystem>();
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            mZombieSystem = this.GetSystem<IZombieSystem>();
            mDefenseSystem = this.GetSystem<IDefenseSystem>();
            mTechSystem = this.GetSystem<IAdvancedTechSystem>();
            mConfigSystem = this.GetSystem<ConfigSystem>();
            
            // 自动查找并关联UI组件引用
            FindUIComponents();
        }
        
        /// <summary>
        /// 自动查找场景中尚未在Inspector中手动赋值的UI组件。
        /// 采用 transform.Find 方法，适用于UI元素层级结构相对固定的情况。
        /// 如果组件已通过Inspector赋值，则不会再次查找。
        /// </summary>
        private void FindUIComponents()
        {
            // 资源显示UI元素 - 仅当Inspector中未赋值时才查找
            if (foodText == null) foodText = transform.Find("ResourcePanel/FoodText")?.GetComponent<Text>();
            if (waterText == null) waterText = transform.Find("ResourcePanel/WaterText")?.GetComponent<Text>();
            if (materialsText == null) materialsText = transform.Find("ResourcePanel/MaterialsText")?.GetComponent<Text>();
            if (ammunitionText == null) ammunitionText = transform.Find("ResourcePanel/AmmoText")?.GetComponent<Text>();
            if (populationText == null) populationText = transform.Find("ResourcePanel/PopulationText")?.GetComponent<Text>();
            if (moraleText == null) moraleText = transform.Find("ResourcePanel/MoraleText")?.GetComponent<Text>();
            if (dayText == null) dayText = transform.Find("ResourcePanel/DayText")?.GetComponent<Text>();
            if (energyText == null) energyText = transform.Find("ResourcePanel/EnergyText")?.GetComponent<Text>();
            if (researchPointsText == null) researchPointsText = transform.Find("ResourcePanel/ResearchPointsText")?.GetComponent<Text>();
            
            // 资源生产效率显示UI元素
            if (foodProductionText == null) foodProductionText = transform.Find("ResourcePanel/FoodProductionText")?.GetComponent<Text>();
            if (ammoProductionText == null) ammoProductionText = transform.Find("ResourcePanel/AmmoProductionText")?.GetComponent<Text>();
            if (materialsProductionText == null) materialsProductionText = transform.Find("ResourcePanel/MaterialsProductionText")?.GetComponent<Text>();
            if (totalProductionText == null) totalProductionText = transform.Find("ResourcePanel/TotalProductionText")?.GetComponent<Text>();
            
            // 僵尸威胁显示UI元素
            if (threatLevelText == null) threatLevelText = transform.Find("ThreatPanel/ThreatLevelText")?.GetComponent<Text>();
            if (zombieCountText == null) zombieCountText = transform.Find("ThreatPanel/ZombieCountText")?.GetComponent<Text>();
            if (threatWarningPanel == null) threatWarningPanel = transform.Find("ThreatPanel/WarningPanel")?.gameObject;
            
            // 防御系统显示UI元素
            if (towerCountText == null) towerCountText = transform.Find("DefensePanel/TowerCountText")?.GetComponent<Text>();
            if (towerStatsText == null) towerStatsText = transform.Find("DefensePanel/TowerStatsText")?.GetComponent<Text>();
            
            // 科技系统显示UI元素
            if (currentResearchText == null) currentResearchText = transform.Find("TechPanel/CurrentResearchText")?.GetComponent<Text>();
            if (researchProgressText == null) researchProgressText = transform.Find("TechPanel/ResearchProgressText")?.GetComponent<Text>();
            
            // 建筑系统显示UI元素
            if (buildingCountText == null) buildingCountText = transform.Find("BuildingPanel/BuildingCountText")?.GetComponent<Text>();
            if (buildingStatsText == null) buildingStatsText = transform.Find("BuildingPanel/BuildingStatsText")?.GetComponent<Text>();
            if (buildingProductionText == null) buildingProductionText = transform.Find("BuildingPanel/BuildingProductionText")?.GetComponent<Text>();
            
            // 建造菜单UI元素
            if (buildMenuPanel == null) buildMenuPanel = transform.Find("BuildMenuPanel")?.gameObject;
            if (buildMenuToggleBtn == null) buildMenuToggleBtn = transform.Find("ControlPanel/BuildMenuBtn")?.GetComponent<Button>();
            if (buildButtonContainer == null) buildButtonContainer = transform.Find("BuildMenuPanel/ButtonContainer")?.GetComponent<Transform>();
            
            // 游戏控制按钮UI元素
            if (pauseBtn == null) pauseBtn = transform.Find("ControlPanel/PauseBtn")?.GetComponent<Button>();
            if (pauseBtnText == null) pauseBtnText = transform.Find("ControlPanel/PauseBtn/Text")?.GetComponent<Text>(); // 通常按钮文本是按钮的子对象
            if (techBtn == null) techBtn = transform.Find("ControlPanel/TechBtn")?.GetComponent<Button>();
            if (survivorBtn == null) survivorBtn = transform.Find("ControlPanel/SurvivorBtn")?.GetComponent<Button>();
        }
        
        /// <summary>
        /// Unity生命周期方法：在Awake之后、首次Update之前调用一次。
        /// 用于UI初始化、事件订阅和创建动态UI元素（如建造按钮）。
        /// </summary>
        private void Start()
        {
            InitializeUI();        // 初始化UI元素状态和按钮监听
            SubscribeToEvents();   // 订阅游戏模型和自定义事件
            CreateBuildButtons();  // 根据配置动态创建建造菜单中的按钮
            UpdateResourceDisplay(0); // 初始更新一次资源显示（参数0无实际意义，仅为匹配委托签名）

            // 示例：使用QFramework的BindableProperty特性，当mSurvivalGameModel.Food变化时自动调用UpdateResourceDisplay
            // 注意：原代码中此行可能会导致重复更新，因为SubscribeToEvents中已对Food等资源进行了订阅。
            // 如果OnResourceChanged已正确处理所有资源更新，则此特定行可能多余或应整合。
            // 为保持与原代码逻辑一致，此处保留，但建议审查事件订阅以避免冗余。
            mSurvivalGameModel.Food.RegisterWithInitValue(UpdateResourceDisplay)
                .UnRegisterWhenGameObjectDestroyed(this.gameObject); // 确保在对象销毁时自动取消注册
        }
        
        /// <summary>
        /// 初始化UI元素的状态，例如隐藏建造菜单、绑定按钮的点击事件等。
        /// </summary>
        private void InitializeUI()
        {
            // 初始化时隐藏建造菜单面板
            if (buildMenuPanel != null)
                buildMenuPanel.SetActive(false);
            
            // 为各个控制按钮绑定点击事件处理方法
            if (buildMenuToggleBtn != null) buildMenuToggleBtn.onClick.AddListener(ToggleBuildMenu);
            if (pauseBtn != null) pauseBtn.onClick.AddListener(TogglePause);
            if (techBtn != null) techBtn.onClick.AddListener(OpenTechTree);
            if (survivorBtn != null) survivorBtn.onClick.AddListener(OpenSurvivorPanel);
            
            // 根据当前游戏暂停状态，更新暂停按钮的初始文本
            UpdatePauseButton();
        }
        
        /// <summary>
        /// 订阅游戏数据模型中的可绑定属性变化事件以及自定义的游戏事件。
        /// 当这些数据或事件发生时，会调用相应的处理方法来更新UI。
        /// </summary>
        private void SubscribeToEvents()
        {
            // 监听主要资源量的变化，并在变化时调用 OnResourceChanged 方法，同时在初始时也调用一次
            mSurvivalGameModel.Food.RegisterWithInitValue(OnResourceChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            mSurvivalGameModel.Water.RegisterWithInitValue(OnResourceChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            mSurvivalGameModel.Materials.RegisterWithInitValue(OnResourceChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            mSurvivalGameModel.Ammunition.RegisterWithInitValue(OnResourceChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            mSurvivalGameModel.Population.RegisterWithInitValue(OnResourceChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            mSurvivalGameModel.Morale.RegisterWithInitValue(OnResourceChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            mSurvivalGameModel.Energy.RegisterWithInitValue(OnResourceChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            mSurvivalGameModel.ResearchPoints.RegisterWithInitValue(OnResourceChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            mSurvivalGameModel.GameDay.RegisterWithInitValue(OnDayChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            mSurvivalGameModel.IsPaused.RegisterWithInitValue(OnPauseStateChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        }
        
        private void CreateBuildButtons()
        {
            if (buildButtonContainer == null || mConfigSystem == null) return;
            
            // 清除现有按钮
            foreach (Transform child in buildButtonContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 为每种建筑类型创建按钮
            var allBuildingConfigs = mConfigSystem.GetAllBuildingConfigs();
            foreach (var buildingConfig in allBuildingConfigs.Values)
            {
                CreateBuildButton(buildingConfig);
            }
        }
        
        private void CreateBuildButton(BuildingConfig config)
        {
            // 创建简单按钮
                GameObject buttonGO = new GameObject($"Build_{config.ConfigId}");
                buttonGO.transform.SetParent(buildButtonContainer);
                
                var button = buttonGO.AddComponent<Button>();
                var image = buttonGO.AddComponent<Image>();
                image.color = GetBuildingTypeColor(config.Category);
                
                // 添加文本
                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform);
                var text = textGO.AddComponent<Text>();
                
                // 计算建造成本显示
                string costText = "";
                if (config.BuildCosts != null && config.BuildCosts.Count > 0)
                {
                    var firstCost = config.BuildCosts[0];
                    costText = $"\n{firstCost.Type}:{firstCost.Amount}";
                }
                
                text.text = $"{config.Name}{costText}";
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.color = Color.black;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 12;
                
                // 设置RectTransform
                var rectTransform = buttonGO.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(100, 60);
                
                var textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                button.onClick.AddListener(() => SelectBuilding(config.ConfigId));
        }
        
        private Color GetBuildingTypeColor(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Defense:
                    return Color.red;
                case BuildingCategory.Production:
                    return Color.yellow;
                case BuildingCategory.Habitat:
                    return Color.blue;
                case BuildingCategory.Storage:
                    return Color.green;
                case BuildingCategory.Functional:
                    return Color.cyan;
                default:
                    return Color.gray;
            }
        }
        
        private void SelectBuilding(string buildingId)
        {
            mSelectedBuildingId = buildingId;
            mInBuildMode = true;
            
            // 发送建造模式变化事件，通知网格UI显示
            this.SendEvent(new BuildModeChangedEvent
            {
                InBuildMode = true,
                BuildingType = buildingId
            });
            
            // 关闭建造菜单
            ToggleBuildMenu();
            
            UnityEngine.Debug.Log($"选择建造：{buildingId}");
        }
        
        private void Update()
        {
            if (mInBuildMode)
        {
            HandleBuildMode();
            }
            
            // 定期更新各种显示
            UpdateZombieDisplay();
            UpdateDefenseDisplay();
            UpdateTechDisplay();
            UpdateBuildingDisplay();
            
            // 定期更新生产效率显示
            if (Time.time - mLastProductionDisplayUpdate >= PRODUCTION_DISPLAY_UPDATE_INTERVAL)
            {
                UpdateProductionEfficiencyDisplay();
                mLastProductionDisplayUpdate = Time.time;
            }
        }
        
        /// <summary>
        /// 处理建造模式下的用户输入
        /// 支持连续建造模式（左键连续点击放置建筑）
        /// 支持右键或ESC退出建造模式
        /// </summary>
        private void HandleBuildMode()
        {
            if (!mInBuildMode) return;
            
            // 获取鼠标在世界坐标中的位置（2D平面）
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0; // 确保Z轴为0，保持在2D平面上
            
            // 鼠标左键点击建造（连续建造模式，不退出建造模式）
            if (Input.GetMouseButtonDown(0))
            {
                // 发送建造命令到命令系统
                this.SendCommand(new BuildCommand(mouseWorldPos, mSelectedBuildingId));
                // 注意：不调用ExitBuildMode()，保持建造模式以支持连续点击
            }
            
            // 右键取消建造模式
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                ExitBuildMode(); // 退出建造模式
            }
        }
        
        private void ExitBuildMode()
        {
            mInBuildMode = false;
            mSelectedBuildingId = "";
            
            // 发送建造模式变化事件，通知网格UI隐藏
            this.SendEvent(new BuildModeChangedEvent
            {
                InBuildMode = false,
                BuildingType = ""
            });
            
            UnityEngine.Debug.Log("退出建造模式");
        }
        
        private void ToggleBuildMenu()
        {
            mBuildMenuOpen = !mBuildMenuOpen;
            
            if (mBuildMenuOpen)
            {
                buildMenuPanel.SetActive(true);
                buildMenuPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
            else
            {
                buildMenuPanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() => buildMenuPanel.SetActive(false));
            }
        }
        
        private void TogglePause()
        {
            bool currentPaused = mSurvivalGameModel.IsPaused.Value;
            mSurvivalGameModel.IsPaused.Value = !currentPaused;
        }
        
        private void OpenTechTree()
        {
            // 查找科技树UI控制器并打开
            var techUIController = FindObjectOfType<TechUIController>();
            if (techUIController != null)
            {
                // 通过反射或公共方法打开科技树面板
                techUIController.SendMessage("ToggleTechPanel", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("未找到科技树UI控制器！");
            }
        }
        
        private void OpenSurvivorPanel()
        {
            // 临时实现：通过SendMessage调用幸存者UI面板
            GameObject survivorUIObject = GameObject.Find("SurvivorUIManager");
            if (survivorUIObject != null)
            {
                survivorUIObject.SendMessage("ToggleSurvivorPanel", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.Log("幸存者管理界面：S键打开幸存者面板（临时按键）");
            }
        }
        
        private void UpdatePauseButton()
        {
            if (pauseBtnText != null)
            {
                pauseBtnText.text = mSurvivalGameModel.IsPaused.Value ? "继续" : "暂停";
            }
        }
        
        private void UpdateResourceDisplay(int value)
        {
            // 更新资源文本显示
            if (foodText != null)
                foodText.text = $"食物: {mSurvivalGameModel.Food.Value}";
            
            if (waterText != null)
                waterText.text = $"水: {mSurvivalGameModel.Water.Value}";
            
            if (materialsText != null)
                materialsText.text = $"建材: {mSurvivalGameModel.Materials.Value}";
            
            if (ammunitionText != null)
                ammunitionText.text = $"弹药: {mSurvivalGameModel.Ammunition.Value}";
            
            if (populationText != null)
                populationText.text = $"人口: {mSurvivalGameModel.Population.Value}";
            
            if (moraleText != null)
                moraleText.text = $"士气: {mSurvivalGameModel.Morale.Value}%";
            
            if (dayText != null)
                dayText.text = $"第 {mSurvivalGameModel.GameDay.Value} 天";
            
            if (energyText != null)
                energyText.text = $"能量: {mSurvivalGameModel.Energy.Value}";
            
            if (researchPointsText != null)
                researchPointsText.text = $"研究点数: {mSurvivalGameModel.ResearchPoints.Value}";
        }
        
        private void OnResourceChanged(int index)
        {
            // 资源变化时的动画效果
            AnimateResourceChange();
        }
        
        private void OnDayChanged(int day)
        {
            // 新的一天的动画效果
            if (dayText != null)
            {
                dayText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
                dayText.color = Color.yellow;
                dayText.DOColor(Color.white, 1f);
            }
        }
        
        private void OnPauseStateChanged(bool isPaused)
        {
            UpdatePauseButton();
        }
        
        private void AnimateResourceChange()
        {
            // 资源数字跳动动画
            if (foodText != null)
                foodText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            if (waterText != null)
                waterText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            if (materialsText != null)
                materialsText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            if (ammunitionText != null)
                ammunitionText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            if (populationText != null)
                populationText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            if (moraleText != null)
                moraleText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            if (energyText != null)
                energyText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            if (researchPointsText != null)
                researchPointsText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
        }
        
        // 移除有冲突的事件处理方法
        
        private void OnBuildSuccess(BuildCommandSuccessEvent e)
        {
            // 建造成功后保持建造模式，支持连续建造
            // 不调用ExitBuildMode()，让玩家可以继续建造
            
            // 显示建造成功提示
            UnityEngine.Debug.Log($"建造成功：{e.BuildingId}，继续建造模式");
        }
        
        private void OnBuildFailed(BuildCommandFailedEvent e)
        {
            // 显示建造失败提示
            UnityEngine.Debug.Log($"建造失败：{e.Reason}");
        }
        
        /// <summary>
        /// 生产效率更新事件处理
        /// </summary>
        private void OnProductionEfficiencyUpdated(ProductionEfficiencyUpdatedEvent e)
        {
            // 立即更新生产效率显示
            UpdateProductionEfficiencyDisplay();
        }
        
        // 实现IController接口
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
        
        private void UpdateZombieDisplay()
        {
            if (mZombieSystem != null)
            {
                // 更新威胁等级显示
                var threatLevel = mZombieSystem.GetCurrentThreatLevel();
                if (threatLevelText != null)
                {
                    threatLevelText.text = $"威胁等级: {GetThreatLevelText(threatLevel)}";
                    threatLevelText.color = GetThreatLevelColor(threatLevel);
                }
                
                // 更新僵尸数量显示
                var zombieCount = mZombieSystem.GetActiveZombieCount();
                if (zombieCountText != null)
                {
                    zombieCountText.text = $"僵尸数量: {zombieCount}";
                }
                
                // 显示高威胁警告
                if (threatWarningPanel != null)
                {
                    bool showWarning = threatLevel >= ZombieThreatLevel.High;
                    threatWarningPanel.SetActive(showWarning);
                }
            }
        }
        
        private string GetThreatLevelText(ZombieThreatLevel level)
        {
            switch (level)
            {
                case ZombieThreatLevel.Safe: return "安全";
                case ZombieThreatLevel.Low: return "低威胁";
                case ZombieThreatLevel.Medium: return "中等威胁";
                case ZombieThreatLevel.High: return "高威胁";
                case ZombieThreatLevel.Extreme: return "极端威胁";
                default: return "未知";
            }
        }
        
        private Color GetThreatLevelColor(ZombieThreatLevel level)
        {
            switch (level)
            {
                case ZombieThreatLevel.Safe: return Color.green;
                case ZombieThreatLevel.Low: return Color.yellow;
                case ZombieThreatLevel.Medium: return new Color(1f, 0.5f, 0f); // 橙色
                case ZombieThreatLevel.High: return Color.red;
                case ZombieThreatLevel.Extreme: return Color.magenta;
                default: return Color.gray;
            }
        }
        
        private void OnThreatLevelChanged(ThreatLevelChangedEvent e)
        {
            // 威胁等级变化时的动画和提示
            if (threatLevelText != null)
            {
                threatLevelText.transform.DOScale(1.2f, 0.3f).SetLoops(2, LoopType.Yoyo);
            }
            
            // 高威胁等级时的警告
            if (e.NewLevel >= ZombieThreatLevel.High)
            {
                if (threatWarningPanel != null)
                {
                    threatWarningPanel.SetActive(true);
                    threatWarningPanel.transform.DOShakePosition(1f, 10f);
                }
            }
            
            Debug.Log($"威胁等级变化：{e.OldLevel} -> {e.NewLevel}");
        }
        
        private void OnZombieSpawned(ZombieSpawnedEvent e)
        {
            // 僵尸生成时的提示
            if (zombieCountText != null)
            {
                zombieCountText.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);
            }
        }
        
        private void UpdateDefenseDisplay()
        {
            if (mDefenseSystem == null) return;
            
            // 更新防御塔数量
            var towers = mDefenseSystem.GetAllTowers();
            var operationalTowers = towers.Where(t => t.IsOperational).Count();
            var totalTowers = towers.Count;
            
            if (towerCountText != null)
            {
                towerCountText.text = $"防御塔: {operationalTowers}/{totalTowers}";
                
                // 如果有损坏的防御塔，显示红色
                if (operationalTowers < totalTowers)
                {
                    towerCountText.color = Color.red;
                }
                else
                {
                    towerCountText.color = Color.white;
                }
            }
            
            // 更新防御统计
            if (towerStatsText != null)
            {
                var stats = mDefenseSystem.GetDefenseStats();
                var totalKills = stats.totalZombiesKilled;
                var totalDamage = (int)stats.totalDamageDealt;
                var activeProjectiles = mDefenseSystem.GetAllProjectiles().Count;
                
                towerStatsText.text = $"击杀: {totalKills}\n伤害: {totalDamage}\n投射物: {activeProjectiles}";
                
                // 根据击杀数量设置颜色
                if (totalKills > 50)
                    towerStatsText.color = Color.green;
                else if (totalKills > 20)
                    towerStatsText.color = Color.yellow;
                else
                    towerStatsText.color = Color.white;
            }
        }
        
        private void UpdateTechDisplay()
        {
            if (mTechSystem == null) return;
            
            // 更新研究点数
            if (researchPointsText != null)
            {
                float currentPoints = mTechSystem.GetCurrentResearchPoints();
                float pointsPerSecond = mTechSystem.GetResearchPointsPerSecond();
                
                researchPointsText.text = $"研究点数: {currentPoints:F1}\n(+{pointsPerSecond:F2}/秒)";
                
                // 根据点数多少设置颜色
                if (currentPoints > 200)
                    researchPointsText.color = Color.green;
                else if (currentPoints > 50)
                    researchPointsText.color = Color.yellow;
                else
                    researchPointsText.color = Color.white;
            }
            
            // 更新当前研究
            if (currentResearchText != null)
            {
                var techData = mTechSystem.GetTechSystemData();
                
                if (!string.IsNullOrEmpty(techData.CurrentResearch))
                {
                    var currentTech = mTechSystem.GetTech(techData.CurrentResearch);
                    if (currentTech != null)
                    {
                        currentResearchText.text = $"正在研究:\n{currentTech.Name}";
                        currentResearchText.color = Color.cyan;
                    }
                }
                else
                {
                    currentResearchText.text = "当前研究:\n无";
                    currentResearchText.color = Color.gray;
                }
            }
            
            // 更新研究进度
            if (researchProgressText != null)
            {
                var techData = mTechSystem.GetTechSystemData();
                
                if (!string.IsNullOrEmpty(techData.CurrentResearch))
                {
                    var progress = mTechSystem.GetResearchProgress(techData.CurrentResearch);
                    var estimatedTime = mTechSystem.GetEstimatedCompletionTime(techData.CurrentResearch);
                    
                    researchProgressText.text = $"进度: {progress:P1}\n预计: {estimatedTime:F1}小时";
                    
                    // 根据进度设置颜色
                    if (progress > 0.8f)
                        researchProgressText.color = Color.green;
                    else if (progress > 0.5f)
                        researchProgressText.color = Color.yellow;
                    else
                        researchProgressText.color = Color.white;
                }
                else
                {
                    var researchedCount = mTechSystem.GetResearchedTechs().Count;
                    var availableCount = mTechSystem.GetAvailableTechs().Count;
                    
                    researchProgressText.text = $"已研发: {researchedCount}\n可研发: {availableCount}";
                    researchProgressText.color = Color.white;
                }
            }
        }
        
        private void UpdateBuildingDisplay()
        {
            if (mBuildingSystem == null) return;
            
            // 更新建筑数量统计
            if (buildingCountText != null)
            {
                var allBuildings = mBuildingSystem.GetAllBuildings();
                var operationalBuildings = allBuildings.Where(b => mBuildingSystem.IsBuildingOperational(b.Id)).Count();
                var totalBuildings = allBuildings.Count;
                
                buildingCountText.text = $"建筑总数: {totalBuildings}\n运营中: {operationalBuildings}";
                
                // 根据运营比例设置颜色
                if (totalBuildings == 0)
                {
                    buildingCountText.color = Color.gray;
                }
                else
                {
                    float operationalRatio = (float)operationalBuildings / totalBuildings;
                    if (operationalRatio > 0.8f)
                        buildingCountText.color = Color.green;
                    else if (operationalRatio > 0.5f)
                        buildingCountText.color = Color.yellow;
                    else
                        buildingCountText.color = Color.red;
                }
            }
            
            // 更新建筑类型统计
            if (buildingStatsText != null)
            {
                var habitatCount = mBuildingSystem.GetBuildingsByCategory(BuildingCategory.Habitat).Count;
                var productionCount = mBuildingSystem.GetBuildingsByCategory(BuildingCategory.Production).Count;
                var defenseCount = mBuildingSystem.GetBuildingsByCategory(BuildingCategory.Defense).Count;
                var storageCount = mBuildingSystem.GetBuildingsByCategory(BuildingCategory.Storage).Count;
                var functionalCount = mBuildingSystem.GetBuildingsByCategory(BuildingCategory.Functional).Count;
                
                buildingStatsText.text = $"住宅: {habitatCount}\n生产: {productionCount}\n防御: {defenseCount}\n存储: {storageCount}\n功能: {functionalCount}";
                buildingStatsText.color = Color.white;
            }
            
            // 更新生产统计
            if (buildingProductionText != null)
            {
                var foodProduction = mBuildingSystem.GetTotalProduction(ResourceType.Food);
                var materialProduction = mBuildingSystem.GetTotalProduction(ResourceType.Materials);
                var ammoProduction = mBuildingSystem.GetTotalProduction(ResourceType.Ammunition);
                
                buildingProductionText.text = $"食物: +{foodProduction:F1}/min\n材料: +{materialProduction:F1}/min\n弹药: +{ammoProduction:F1}/min";
                
                // 根据总生产力设置颜色
                float totalProduction = foodProduction + materialProduction + ammoProduction;
                if (totalProduction > 30)
                    buildingProductionText.color = Color.green;
                else if (totalProduction > 10)
                    buildingProductionText.color = Color.yellow;
                else
                    buildingProductionText.color = Color.white;
            }
        }
        
        private void OnDestroy()
        {
            // 取消订阅
            this.UnRegisterEvent<BuildCommandSuccessEvent>(OnBuildSuccess);
            this.UnRegisterEvent<BuildCommandFailedEvent>(OnBuildFailed);
            this.UnRegisterEvent<ThreatLevelChangedEvent>(OnThreatLevelChanged);
            this.UnRegisterEvent<ZombieSpawnedEvent>(OnZombieSpawned);
        }
        
        /// <summary>
        /// 更新生产效率显示
        /// </summary>
        private void UpdateProductionEfficiencyDisplay()
        {
            // 更新资源生产效率显示
            if (foodProductionText != null)
                foodProductionText.text = $"食物生产: +{mResourceSystem.GetResourceProductionRate(ResourceType.Food):F1}/min";
            
            if (ammoProductionText != null)
                ammoProductionText.text = $"弹药生产: +{mResourceSystem.GetResourceProductionRate(ResourceType.Ammunition):F1}/min";
            
            if (materialsProductionText != null)
                materialsProductionText.text = $"建材生产: +{mResourceSystem.GetResourceProductionRate(ResourceType.Materials):F1}/min";
            
            if (totalProductionText != null)
            {
                float totalProduction = mResourceSystem.GetResourceProductionRate(ResourceType.Food) +
                                      mResourceSystem.GetResourceProductionRate(ResourceType.Ammunition) +
                                      mResourceSystem.GetResourceProductionRate(ResourceType.Materials);
                totalProductionText.text = $"总生产: +{totalProduction:F1}/min";
                
                // 根据总生产效率设置颜色
                if (totalProduction > 30)
                    totalProductionText.color = Color.green;
                else if (totalProduction > 10)
                    totalProductionText.color = Color.yellow;
                else if (totalProduction > 0)
                    totalProductionText.color = Color.cyan;
                else
                    totalProductionText.color = Color.red;
            }
        }
    }
}

// 建造模式事件定义
namespace MyGameNamespace
{
    /// <summary>
    /// 建造模式变化事件
    /// </summary>
    public struct BuildModeChangedEvent
    {
        public bool InBuildMode;
        public string BuildingType;
    }
    
    /// <summary>
    /// 网格透明度变化事件
    /// </summary>
    public struct GridAlphaChangedEvent
    {
        public float Alpha;
    }
    
    /// <summary>
    /// 网格显示范围变化事件
    /// </summary>
    public struct GridRangeChangedEvent
    {
        public int Range;
    }
} 