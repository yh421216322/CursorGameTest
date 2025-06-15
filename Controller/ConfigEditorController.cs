// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ConfigEditorController.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了 ConfigEditorController 类，它是一个在Unity编辑器环境下运行的
//     游戏配置管理工具。该工具允许开发者通过图形界面（IMGUI或可选的uGUI）
//     来查看、创建、编辑、验证和导出游戏中的各种配置数据，
//     例如建筑、科技、资源和制造配方等。这些配置通常以ScriptableObject的形式存储。
//     该控制器也负责在运行时加载这些配置到ConfigSystem中。
// ==============================================================================

using System.Collections.Generic;
using MyGameNamespace; // 假设包含一些自定义的命名空间定义
using UnityEngine;
using UnityEngine.UI; // 用于uGUI相关组件
using QFramework;
using SurvivalGame.GameSystem; // 包含ConfigSystem等系统接口
using SurvivalGame.Model;      // 包含BuildingConfig, TechConfig等模型定义

// 仅在Unity编辑器环境下编译Editor相关的代码
#if UNITY_EDITOR
using UnityEditor; // 用于访问编辑器API，如资源创建、保存面板等
#endif

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 配置编辑器控制器 - 在Unity编辑器中或运行时（用于调试）管理游戏配置。
    /// 提供一个GUI界面来查看、创建、编辑、验证和导出各种游戏配置（建筑、科技、资源、配方）。
    /// </summary>
    public class ConfigEditorController : MonoBehaviour, IController // 实现QFramework的IController接口
    {
        [Header("配置管理选项")] // Inspector中显示的头部标签
        [SerializeField] private bool _autoLoadConfigs = true; // 是否在启动时自动加载配置
        [SerializeField] private bool _showDebugInfo = true;   // 是否显示调试信息
        [SerializeField] private bool _showEditorUI = true;    // 是否在运行时显示编辑器UI (IMGUI)
        [SerializeField] private bool _useUGUI = false;        // 新增选项：是否使用uGUI界面替代OnGUI

        [Header("建筑配置列表")] // Inspector中显示的头部标签
        [SerializeField] private List<BuildingConfigAsset> _buildingConfigs = new List<BuildingConfigAsset>(); // 存储建筑配置ScriptableObject的列表
        
        [Header("科技配置列表")]
        [SerializeField] private List<TechConfigAsset> _techConfigs = new List<TechConfigAsset>(); // 存储科技配置ScriptableObject的列表
        
        [Header("资源配置列表")]
        [SerializeField] private List<ResourceConfigAsset> _resourceConfigs = new List<ResourceConfigAsset>(); // 存储资源配置ScriptableObject的列表
        
        [Header("制造配方列表")]
        [SerializeField] private List<CraftingRecipeAsset> _craftingRecipes = new List<CraftingRecipeAsset>(); // 存储制造配方ScriptableObject的列表
        
        [Header("uGUI组件引用 (可选)")] // Inspector中显示的头部标签
        [SerializeField] private Canvas _uiCanvas;       // uGUI界面的Canvas引用
        [SerializeField] private GameObject _uiPanel;    // uGUI界面的主面板引用
        
        private ConfigSystem _configSystem; // 游戏配置系统实例
        
        // IMGUI界面相关变量
        private bool _showBuildingPanel = true;  // 控制是否显示建筑配置面板
        private bool _showTechPanel = false;     // 控制是否显示科技配置面板
        private bool _showResourcePanel = false; // 控制是否显示资源配置面板
        private bool _showRecipePanel = false;   // 控制是否显示制造配方面板
        private Vector2 _scrollPosition = Vector2.zero; // IMGUI滚动视图的位置
        private Rect _windowRect = new Rect(20, 20, 1000, 700); // IMGUI窗口的位置和大小
        
        // IMGUI样式缓存，避免在OnGUI中重复创建
        private GUIStyle _boldStyle;    // 加粗文本样式
        private GUIStyle _centerStyle;  // 居中文本样式
        private GUIStyle _buttonStyle;  // 按钮样式
        
        #region Unity生命周期方法
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被创建并且游戏开始时调用一次。
        /// </summary>
        private void Start()
        {
            // 如果设置了自动加载配置，则在启动时调用加载方法
            if (_autoLoadConfigs)
            {
                LoadAllConfigs();
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 此方法每帧被多次调用（布局和重绘事件）。
        /// </summary>
        private void OnGUI()
        {
            // 如果不显示编辑器UI或游戏不在运行状态，则不执行后续IMGUI渲染
            if (!_showEditorUI || !Application.isPlaying) return;
            
            // 初始化IMGUI的中文字体支持（如果尚未初始化）
            InitializeChineseFontSupport();
            
            // 在屏幕左上角显示当前GUI皮肤使用的字体状态，便于调试字体问题
            GUI.Label(new Rect(10, 10, 300, 20), $"IMGUI字体状态: {(GUI.skin.font != null ? GUI.skin.font.name : "无字体")}");
            
            // 提供一个按钮用于切换到uGUI界面模式
            if (GUI.Button(new Rect(10, 35, 150, 30), "切换到uGUI界面"))
            {
                _useUGUI = true; // 设置使用uGUI标志
                CreateUGUIInterface(); // 创建uGUI界面（如果尚未创建）
            }
            
            // 提供一个按钮用于测试中文字体渲染情况
            if (GUI.Button(new Rect(170, 35, 100, 30), "字体渲染测试"))
            {
                TestChineseFont(); // 调用字体测试方法
            }
            
            // 如果不使用uGUI模式，则绘制IMGUI窗口
            if (!_useUGUI)
            {
                // GUI.Window会创建一个可拖拽的窗口，并调用ConfigEditorWindow方法来填充其内容
                _windowRect = GUI.Window(0, _windowRect, ConfigEditorWindow, "游戏配置编辑器 (IMGUI)");
            }
        }
        
        /// <summary>
        /// 动态创建uGUI界面作为IMGUI的备用或替代方案。
        /// 此方法会在需要时（例如点击切换按钮且Canvas不存在时）创建Canvas、Panel、Title和CloseButton。
        /// </summary>
        private void CreateUGUIInterface()
        {
            // 仅在Canvas尚未创建时执行
            if (_uiCanvas == null)
            {
                // 1. 创建Canvas对象
                GameObject canvasObj = new GameObject("ConfigEditorCanvas_uGUI");
                _uiCanvas = canvasObj.AddComponent<Canvas>();
                _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // 设置为屏幕空间覆盖模式
                canvasObj.AddComponent<CanvasScaler>(); // 添加CanvasScaler以适应不同分辨率
                canvasObj.AddComponent<GraphicRaycaster>(); // 添加GraphicRaycaster以响应UI事件
                
                // 2. 创建主面板 (Panel)
                GameObject panelObj = new GameObject("ConfigPanel_uGUI");
                panelObj.transform.SetParent(_uiCanvas.transform, false); // 设置父对象
                
                var panelImage = panelObj.AddComponent<Image>(); // 添加Image组件作为背景
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // 设置半透明深色背景
                
                // 设置面板的RectTransform，使其占据Canvas的80%区域
                var rectTransform = panelObj.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
                rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
                rectTransform.offsetMin = Vector2.zero; // 清除偏移，使其完全填充锚点区域
                rectTransform.offsetMax = Vector2.zero;
                
                // 3. 添加标题文本
                GameObject titleObj = new GameObject("TitleText_uGUI");
                titleObj.transform.SetParent(panelObj.transform, false);
                var titleText = titleObj.AddComponent<Text>();
                titleText.text = "游戏配置编辑器 (uGUI 版本)";
                titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // 使用内置Arial字体
                titleText.fontSize = 20;
                titleText.color = Color.white;
                titleText.alignment = TextAnchor.MiddleCenter; // 文本居中对齐
                
                // 设置标题的RectTransform，使其位于面板顶部
                var titleRect = titleObj.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 0.9f);
                titleRect.anchorMax = new Vector2(1, 1f); // Y锚点从0.9到1，占据顶部10%高度
                titleRect.offsetMin = new Vector2(10, 0); // 左右边距
                titleRect.offsetMax = new Vector2(-10, -5); // 上下边距（相对于锚点）
                
                // 4. 添加关闭按钮
                GameObject closeButtonObj = new GameObject("CloseButton_uGUI");
                closeButtonObj.transform.SetParent(panelObj.transform, false);
                var closeButton = closeButtonObj.AddComponent<Button>();
                var closeButtonImage = closeButtonObj.AddComponent<Image>(); // 按钮背景图
                closeButtonImage.color = Color.red * 0.8f; // 设置按钮背景为半透明红色
                
                // 为关闭按钮添加文本
                var buttonTextObj = new GameObject("ButtonText_uGUI");
                buttonTextObj.transform.SetParent(closeButtonObj.transform, false);
                var buttonTextComponent = buttonTextObj.AddComponent<Text>();
                buttonTextComponent.text = "关闭";
                buttonTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                buttonTextComponent.fontSize = 14;
                buttonTextComponent.color = Color.white;
                buttonTextComponent.alignment = TextAnchor.MiddleCenter;
                
                // 设置按钮文本的RectTransform，使其填充按钮区域
                var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;
                
                // 设置关闭按钮的RectTransform，使其位于面板右上角
                var closeButtonRect = closeButtonObj.GetComponent<RectTransform>();
                closeButtonRect.anchorMin = new Vector2(1, 1); // 右上角锚点
                closeButtonRect.anchorMax = new Vector2(1, 1);
                closeButtonRect.pivot = new Vector2(1, 1); // 轴心点也为右上角
                closeButtonRect.sizeDelta = new Vector2(80, 30); // 设置按钮大小
                closeButtonRect.anchoredPosition = new Vector2(-10, -10); // 相对于锚点的偏移
                
                // 添加关闭按钮的点击事件监听
                closeButton.onClick.AddListener(() => {
                    _useUGUI = false; // 切换回IMGUI模式
                    if (canvasObj != null) DestroyImmediate(canvasObj); // 立即销毁Canvas对象
                    _uiCanvas = null; // 清空Canvas引用
                    _uiPanel = null;  // 清空Panel引用
                });
                
                _uiPanel = panelObj; // 保存对主面板的引用
                
                Debug.Log("uGUI 配置编辑器界面已创建。");
            }
            else if (_uiPanel != null)
            {
                _uiPanel.SetActive(true); // 如果已创建，则仅激活
            }
        }
        
        #endregion
        
        #region 中文字体支持 (IMGUI)
        
        private bool _chineseFontInitialized = false; // 标记中文字体是否已初始化
        
        /// <summary>
        /// 初始化IMGUI的中文字体支持。
        /// 尝试加载系统中的常见中文字体（如微软雅黑、黑体、宋体等）。
        /// 如果成功加载，则将其设置为GUI.skin的默认字体，并预请求一些常用中文字符以确保渲染。
        /// </summary>
        private void InitializeChineseFontSupport()
        {
            // 如果已初始化，则直接返回，避免重复操作
            if (_chineseFontInitialized) return;
            
            try
            {
                // 尝试按顺序加载几种常见的中文字体
                Font chineseFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16); // 微软雅黑
                if (chineseFont == null)
                    chineseFont = Font.CreateDynamicFontFromOSFont("SimHei", 16); // 黑体
                if (chineseFont == null)
                    chineseFont = Font.CreateDynamicFontFromOSFont("SimSun", 16); // 宋体 (可能在某些系统上效果不佳)
                if (chineseFont == null)
                    chineseFont = Font.CreateDynamicFontFromOSFont("Arial Unicode MS", 16); // 一个包含很多Unicode字符的字体
                
                // 如果成功找到并创建了中文字体，并且GUI.skin存在
                if (chineseFont != null && GUI.skin != null)
                {
                    // 将加载到的中文字体应用到GUI.skin的各个元素上
                    GUI.skin.font = chineseFont;
                    GUI.skin.label.font = chineseFont;
                    GUI.skin.button.font = chineseFont;
                    GUI.skin.box.font = chineseFont;
                    GUI.skin.toggle.font = chineseFont;
                    GUI.skin.textField.font = chineseFont;
                    GUI.skin.textArea.font = chineseFont;
                    
                    // 预加载一些常用中文字符到字体纹理中，以避免首次显示时出现卡顿或方块字
                    // 这里的字符列表可以根据实际UI中使用的文字进行扩充
                    string chineseChars = "建筑配置管理科技资源制造配方生产防御居住储存功能型建筑农场工坊医疗站瞭望塔图书馆采石场避难所围墙储存库实验室净水器加载验证导出所有配置等级类型名称编辑删除暂无可用快速创建切换字体状态测试区域结束原生方法显示正常红色样式临时帐篷研究角急救站研究台简易小屋医疗帐篷小型实验室住宅区研究所公寓楼高级医院科技中心豪华社区超级医疗中心储物箱食物储藏室仓库弹药库冷藏库大型仓库专业储存中心自动化仓库超级储存中心水井发电机房通讯站太阳能发电站贸易中心";
                    chineseFont.RequestCharactersInTexture(chineseChars, 12); // 请求不同字号的字符
                    chineseFont.RequestCharactersInTexture(chineseChars, 14);
                    chineseFont.RequestCharactersInTexture(chineseChars, 16);
                    chineseFont.RequestCharactersInTexture(chineseChars, 18);
                    chineseFont.RequestCharactersInTexture(chineseChars, 20);
                    
                    Debug.Log($"IMGUI中文字体设置成功: {chineseFont.name}");
                    _chineseFontInitialized = true; // 标记为已成功初始化
                }
                else
                {
                    Debug.LogWarning("无法从操作系统找到合适的中文字体，IMGUI将使用默认字体。部分中文可能无法正确显示。");
                    _chineseFontInitialized = true; // 标记为已尝试初始化，避免重复尝试
                }
            }
            catch (System.Exception e) // 捕获字体加载过程中可能发生的任何异常
            {
                Debug.LogError($"IMGUI中文字体初始化失败: {e.Message}");
                _chineseFontInitialized = true; // 标记为已尝试初始化，避免重复尝试
            }
        }
        
        /// <summary>
        /// 测试当前GUI.skin中的中文字体显示情况。
        /// 主要通过在控制台打印一些中文字符串和字体信息来进行。
        /// </summary>
        private void TestChineseFont()
        {
            Debug.Log("开始测试IMGUI中文字体显示...");
            Debug.Log("常用中文字符串示例: 建筑配置管理, 科技, 资源, 制造配方");
            Debug.Log("建筑类型示例: 生产型、防御型、居住型、储存型、功能型");
            Debug.Log("操作按钮示例: 加载、验证、导出、创建、编辑、删除");
            
            // 在Console中显示当前GUI.skin使用的字体信息
            if (GUI.skin != null && GUI.skin.font != null)
            {
                Debug.Log($"当前IMGUI字体: {GUI.skin.font.name}");
                Debug.Log($"该字体是否为动态字体 (支持Unicode): {GUI.skin.font.dynamic}");
            }
            else
            {
                Debug.LogWarning("GUI.skin 或 GUI.skin.font 为空，无法获取字体信息。");
            }
            Debug.Log("字体测试结束。请检查控制台输出及IMGUI界面中的中文显示是否正常。");
        }
        
        #endregion
        
        #region 配置加载逻辑
        
        /// <summary>
        /// 加载所有类型的配置数据。
        /// 此方法会获取ConfigSystem的实例，并调用各个具体类型的配置加载方法。
        /// 标记为[ContextMenu]可以在Inspector中右键点击此脚本组件来执行此方法。
        /// </summary>
        [ContextMenu("加载所有配置到系统中")]
        public void LoadAllConfigs()
        {
            // 获取QFramework架构中的ConfigSystem实例
            _configSystem = this.GetSystem<ConfigSystem>();
            
            if (_configSystem == null)
            {
                Debug.LogError("ConfigSystem 未能成功初始化或获取。请检查QFramework架构设置。");
                return;
            }
            
            // 调用各个具体配置类型的加载方法
            LoadBuildingConfigs();
            LoadTechConfigs();
            LoadResourceConfigs();
            LoadCraftingRecipes();
            
            // 如果启用了调试信息，则打印加载总结
            if (_showDebugInfo)
            {
                Debug.Log($"所有配置加载尝试完毕 - 建筑配置数量: {_buildingConfigs.Count}, 科技配置数量: {_techConfigs.Count}, 资源配置数量: {_resourceConfigs.Count}, 制造配方数量: {_craftingRecipes.Count}");
            }
        }
        
        /// <summary>
        /// 加载建筑配置。
        /// 遍历_buildingConfigs列表中的ScriptableObject资源，
        /// 并将其中的配置数据注入到ConfigSystem中（当前实现为注释占位）。
        /// </summary>
        private void LoadBuildingConfigs()
        {
            foreach (var configAsset in _buildingConfigs)
            {
                if (configAsset != null && configAsset.Config != null)
                {
                    // TODO: 实现将configAsset.Config注入到_configSystem的逻辑
                    // 例如: _configSystem.RegisterBuildingConfig(configAsset.Config);
                    // 当前ConfigSystem可能已有内置配置，此处的逻辑可能是用于覆盖或动态添加。
                }
            }
        }
        
        /// <summary>
        /// 加载科技配置。 (逻辑与LoadBuildingConfigs类似，具体注入方法需在ConfigSystem中实现)
        /// </summary>
        private void LoadTechConfigs()
        {
            foreach (var configAsset in _techConfigs)
            {
                if (configAsset != null && configAsset.Config != null)
                {
                    // TODO: 注入科技配置到_configSystem
                    // 例如: _configSystem.RegisterTechConfig(configAsset.Config);
                }
            }
        }
        
        /// <summary>
        /// 加载资源配置。 (逻辑与LoadBuildingConfigs类似)
        /// </summary>
        private void LoadResourceConfigs()
        {
            foreach (var configAsset in _resourceConfigs)
            {
                if (configAsset != null && configAsset.Config != null)
                {
                    // TODO: 注入资源配置到_configSystem
                    // 例如: _configSystem.RegisterResourceConfig(configAsset.Config);
                }
            }
        }
        
        /// <summary>
        /// 加载制造配方配置。 (逻辑与LoadBuildingConfigs类似)
        /// </summary>
        private void LoadCraftingRecipes()
        {
            foreach (var recipeAsset in _craftingRecipes)
            {
                if (recipeAsset != null && recipeAsset.Recipe != null)
                {
                    // TODO: 注入制造配方到_configSystem
                    // 例如: _configSystem.RegisterCraftingRecipe(recipeAsset.Recipe);
                }
            }
        }
        
        #endregion
        
        #region 配置创建工具 (仅限Unity编辑器环境)
        
        /// <summary>
        /// 在Unity编辑器中创建一个新的建筑配置ScriptableObject资源。
        /// 用户会被提示选择保存路径和文件名。
        /// </summary>
        [ContextMenu("创建新建筑配置资源")] // 允许在Inspector中右键执行
        public void CreateNewBuildingConfig()
        {
// 仅在Unity编辑器中执行以下代码
#if UNITY_EDITOR
            // 创建BuildingConfigAsset的实例（ScriptableObject）
            var newConfigAsset = ScriptableObject.CreateInstance<BuildingConfigAsset>();
            // 初始化其内部的Config数据结构
            newConfigAsset.Config = new BuildingConfig
            {
                ConfigId = "new_building_" + System.DateTime.Now.Ticks, // 使用时间戳确保ID初始唯一性
                Name = "新建筑",
                Description = "请填写建筑描述",
                Category = BuildingCategory.Production, // 默认分类
                Level = BuildingLevel.Level1,          // 默认等级
                BuildCosts = new List<ResourceCost>(),   // 初始化列表
                BuildTime = 60f,                       // 默认建造时间
                RequiredTechs = new List<string>(),
                MaxWorkers = 1,
                RequiredAttributes = new List<SurvivorAttribute>(),
                MaintenanceCosts = new List<ResourceCost>(),
                Productions = new List<ResourceProduction>(),
                AvailableRecipes = new List<CraftingRecipe>(),
                StorageCapacity = 0,
                StorageTypes = new List<ResourceType>(),
                HousingCapacity = 0,
                DefensePower = 0f,
                DefenseRange = 0f,
                Size = new Vector2Int(1, 1) // 默认大小
            };
            
            // 弹出保存文件对话框，让用户选择保存位置和文件名
            string path = EditorUtility.SaveFilePanelInProject(
                "保存新建筑配置",         // 对话框标题
                "NewBuildingConfig",    // 默认文件名
                "asset",                // 文件扩展名
                "请选择保存新建筑配置文件的位置"); // 对话框提示信息
                
            // 如果用户选择了有效路径
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path); // 在指定路径创建资源文件
                AssetDatabase.SaveAssets();                     // 保存所有未保存的资源更改
                _buildingConfigs.Add(newConfigAsset);            // 将新创建的配置添加到当前控制器的列表中
                Debug.Log($"成功创建新的建筑配置资源于: {path}");
            }
#endif
        }
        
        /// <summary>
        /// 在Unity编辑器中创建一个新的科技配置ScriptableObject资源。
        /// </summary>
        [ContextMenu("创建新科技配置资源")]
        public void CreateNewTechConfig()
        {
#if UNITY_EDITOR
            var newConfigAsset = ScriptableObject.CreateInstance<TechConfigAsset>();
            newConfigAsset.Config = new TechConfig
            {
                Id = "new_tech_" + System.DateTime.Now.Ticks,
                Name = "新科技",
                Description = "请填写科技描述",
                Tree = TechTree.Survival, // 默认科技树分支
                Tier = TechTier.Tier1,    // 默认科技等级
                ResearchPointsCost = 50,  // 默认研究点数成本
                ResearchTime = 2f,        // 默认研究时间 (单位可能需根据游戏设计确定，如小时)
                Prerequisites = new List<string>(),
                AdditionalCosts = new List<ResourceCost>(),
                MinResearchLevel = 1,
                RequiredResearchers = new List<SurvivorAttribute>(),
                FailureRate = 0.1f, // 默认失败率10%
                UnlockedBuildings = new List<string>(),
                UnlockedRecipes = new List<string>(),
                Effects = new List<TechEffect>(),
                UIPosition = Vector2.zero, // UI中显示位置，可能用于科技树可视化
                UIColor = Color.white      // UI中显示颜色
            };
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存新科技配置", "NewTechConfig", "asset", "请选择保存新科技配置文件的位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path);
                AssetDatabase.SaveAssets();
                _techConfigs.Add(newConfigAsset);
                Debug.Log($"成功创建新的科技配置资源于: {path}");
            }
#endif
        }
        
        /// <summary>
        /// 在Unity编辑器中创建一个新的制造配方ScriptableObject资源。
        /// </summary>
        [ContextMenu("创建新制造配方资源")]
        public void CreateNewCraftingRecipe()
        {
#if UNITY_EDITOR
            var newRecipeAsset = ScriptableObject.CreateInstance<CraftingRecipeAsset>();
            newRecipeAsset.Recipe = new CraftingRecipe
            {
                Id = "new_recipe_" + System.DateTime.Now.Ticks,
                Name = "新配方",
                Description = "请填写配方描述",
                OutputType = ResourceType.Tools, // 默认产出类型
                OutputAmount = 1,                // 默认产出数量
                InputCosts = new List<ResourceCost> // 默认输入成本
                {
                    new ResourceCost { Type = ResourceType.Materials, Amount = 10 } // 示例成本
                },
                CraftingTime = 1f, // 默认制造时间 (单位可能为小时或秒)
                RequiredTechLevel = 0, // 默认所需科技等级
                RequiredBuildings = new List<string> { "workshop_1" } // 默认需要在 "workshop_1" 建筑中制造
            };
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存新制造配方", "NewCraftingRecipe", "asset", "请选择保存新制造配方文件的位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newRecipeAsset, path);
                AssetDatabase.SaveAssets();
                _craftingRecipes.Add(newRecipeAsset);
                Debug.Log($"成功创建新的制造配方资源于: {path}");
            }
#endif
        }
        
        #endregion
        
        #region 配置验证逻辑
        
        /// <summary>
        /// 验证所有当前加载的配置数据是否符合基本规则。
        /// 例如检查ID唯一性、必要字段是否为空等。
        /// </summary>
        [ContextMenu("验证所有配置数据")]
        public void ValidateAllConfigs()
        {
            int errorCount = 0; // 记录错误总数
            
            // 分别验证各种类型的配置
            errorCount += ValidateBuildingConfigs();
            errorCount += ValidateTechConfigs();
            errorCount += ValidateCraftingRecipes();
            // TODO: 添加对资源配置的验证 ValidateResourceConfigs()
            
            if (errorCount == 0)
            {
                Debug.Log("所有配置项均通过验证！");
            }
            else
            {
                Debug.LogWarning($"配置验证完成，共发现 {errorCount} 个错误。请检查控制台输出以获取详细信息。");
            }
        }
        
        /// <summary>
        /// 验证建筑配置列表中的数据。
        /// </summary>
        /// <returns>发现的错误数量。</returns>
        private int ValidateBuildingConfigs()
        {
            int errors = 0;
            var usedIds = new HashSet<string>(); // 用于检查ID的唯一性
            
            foreach (var configAsset in _buildingConfigs)
            {
                if (configAsset == null || configAsset.Config == null)
                {
                    Debug.LogError("建筑配置列表中发现空的配置资源(Asset)或配置数据(Config)。");
                    errors++;
                    continue; // 跳过此空配置
                }
                
                var config = configAsset.Config;
                
                // 1. 检查ID唯一性
                if (string.IsNullOrEmpty(config.ConfigId) || !usedIds.Add(config.ConfigId))
                {
                    Debug.LogError($"建筑配置错误：ID '{config.ConfigId ?? "空"}' 重复或为空。资产名: {configAsset.name}");
                    errors++;
                }
                
                // 2. 检查必填字段（例如名称）
                if (string.IsNullOrEmpty(config.Name))
                {
                    Debug.LogError($"建筑配置错误：ID '{config.ConfigId}' 的名称(Name)为空。");
                    errors++;
                }
                
                // 3. 检查建造成本是否合理
                if (config.BuildCosts == null || config.BuildCosts.Count == 0)
                {
                    // 这可能是一个警告而非错误，取决于设计（例如某些建筑可能无成本）
                    Debug.LogWarning($"建筑配置提示：ID '{config.ConfigId}' ({config.Name}) 没有定义建造成本(BuildCosts)。");
                }
                
                // 4. 检查建造时间是否有效
                if (config.BuildTime <= 0)
                {
                    Debug.LogError($"建筑配置错误：ID '{config.ConfigId}' ({config.Name}) 的建造时间(BuildTime) 无效（应大于0）。当前值: {config.BuildTime}");
                    errors++;
                }
                // TODO: 根据BuildingConfig的具体字段添加更多验证规则
            }
            
            return errors;
        }
        
        /// <summary>
        /// 验证科技配置列表中的数据。
        /// </summary>
        /// <returns>发现的错误数量。</returns>
        private int ValidateTechConfigs()
        {
            int errors = 0;
            var usedIds = new HashSet<string>();
            
            foreach (var configAsset in _techConfigs)
            {
                if (configAsset == null || configAsset.Config == null)
                {
                    Debug.LogError("科技配置列表中发现空的配置资源(Asset)或配置数据(Config)。");
                    errors++;
                    continue;
                }
                
                var config = configAsset.Config;
                
                if (string.IsNullOrEmpty(config.Id) || !usedIds.Add(config.Id))
                {
                    Debug.LogError($"科技配置错误：ID '{config.Id ?? "空"}' 重复或为空。资产名: {configAsset.name}");
                    errors++;
                }
                
                if (string.IsNullOrEmpty(config.Name))
                {
                    Debug.LogError($"科技配置错误：ID '{config.Id}' 的名称(Name)为空。");
                    errors++;
                }
                
                if (config.ResearchPointsCost <= 0)
                {
                    Debug.LogError($"科技配置错误：ID '{config.Id}' ({config.Name}) 的研究点数成本(ResearchPointsCost)无效。当前值: {config.ResearchPointsCost}");
                    errors++;
                }
                
                if (config.ResearchTime <= 0)
                {
                    Debug.LogError($"科技配置错误：ID '{config.Id}' ({config.Name}) 的研究时间(ResearchTime)无效。当前值: {config.ResearchTime}");
                    errors++;
                }
                // TODO: 验证前置科技ID是否存在于_techConfigs中 (需要所有科技ID列表)
            }
            
            return errors;
        }
        
        /// <summary>
        /// 验证制造配方列表中的数据。
        /// </summary>
        /// <returns>发现的错误数量。</returns>
        private int ValidateCraftingRecipes()
        {
            int errors = 0;
            var usedIds = new HashSet<string>();
            
            foreach (var recipeAsset in _craftingRecipes)
            {
                if (recipeAsset == null || recipeAsset.Recipe == null)
                {
                    Debug.LogError("制造配方列表中发现空的配置资源(Asset)或配方数据(Recipe)。");
                    errors++;
                    continue;
                }
                
                var recipe = recipeAsset.Recipe;
                
                if (string.IsNullOrEmpty(recipe.Id) || !usedIds.Add(recipe.Id))
                {
                    Debug.LogError($"制造配方错误：ID '{recipe.Id ?? "空"}' 重复或为空。资产名: {recipeAsset.name}");
                    errors++;
                }
                
                if (string.IsNullOrEmpty(recipe.Name))
                {
                    Debug.LogError($"制造配方错误：ID '{recipe.Id}' 的名称(Name)为空。");
                    errors++;
                }
                
                if (recipe.InputCosts == null || recipe.InputCosts.Count == 0)
                {
                    Debug.LogWarning($"制造配方提示：ID '{recipe.Id}' ({recipe.Name}) 没有定义输入成本(InputCosts)。");
                }
                
                if (recipe.OutputAmount <= 0)
                {
                    Debug.LogError($"制造配方错误：ID '{recipe.Id}' ({recipe.Name}) 的产出数量(OutputAmount)无效。当前值: {recipe.OutputAmount}");
                    errors++;
                }
                
                if (recipe.CraftingTime <= 0)
                {
                    Debug.LogError($"制造配方错误：ID '{recipe.Id}' ({recipe.Name}) 的制造时间(CraftingTime)无效。当前值: {recipe.CraftingTime}");
                    errors++;
                }
                // TODO: 验证所需建筑ID是否存在于_buildingConfigs中
            }
            
            return errors;
        }
        
        #endregion
        
        #region 配置导出为JSON
        
        /// <summary>
        /// 将当前所有配置数据导出到一个JSON文件中。
        /// 用户会被提示选择保存JSON文件的位置。
        /// </summary>
        [ContextMenu("导出所有配置到JSON文件")]
        public void ExportConfigsToJson()
        {
// 仅在Unity编辑器中执行
#if UNITY_EDITOR
            // 创建用于存储所有配置数据的聚合对象
            var exportData = new ConfigExportData
            {
                Buildings = new List<BuildingConfig>(),
                Techs = new List<TechConfig>(),
                Resources = new List<ResourceConfig>(),
                Recipes = new List<CraftingRecipe>()
            };
            
            // 从各自的Asset列表中提取实际的配置数据并添加到exportData中
            foreach (var asset in _buildingConfigs)
            {
                if (asset?.Config != null) // 安全检查，确保Asset和其内部Config不为空
                    exportData.Buildings.Add(asset.Config);
            }
            
            foreach (var asset in _techConfigs)
            {
                if (asset?.Config != null)
                    exportData.Techs.Add(asset.Config);
            }
            
            foreach (var asset in _resourceConfigs)
            {
                if (asset?.Config != null)
                    exportData.Resources.Add(asset.Config);
            }
            
            foreach (var asset in _craftingRecipes)
            {
                if (asset?.Recipe != null) // 注意这里是 .Recipe
                    exportData.Recipes.Add(asset.Recipe);
            }
            
            // 使用JsonUtility将exportData对象序列化为JSON字符串（美化格式以方便阅读）
            string json = JsonUtility.ToJson(exportData, true);
            // 弹出保存文件对话框
            string path = EditorUtility.SaveFilePanel("导出游戏配置为JSON", "", "GameConfigs_Exported", "json");
            
            // 如果用户选择了有效路径
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, json); // 将JSON字符串写入文件
                Debug.Log($"所有配置已成功导出到JSON文件: {path}");
            }
#endif
        }
        
        #endregion
        
        #region IMGUI样式初始化
        
        /// <summary>
        /// 初始化IMGUI界面所需的自定义GUIStyle。
        /// 这些样式（如粗体、居中对齐）只在首次使用前创建一次，以提高性能。
        /// </summary>
        private void InitializeStyles()
        {
            // 初始化粗体文本样式
            if (_boldStyle == null)
            {
                _boldStyle = new GUIStyle(GUI.skin.label); // 基于默认label样式创建
                _boldStyle.font = GUI.skin.font;           // 确保使用当前皮肤的字体 (重要：在字体初始化后调用)
                _boldStyle.fontStyle = FontStyle.Bold;     // 设置为粗体
                _boldStyle.fontSize = 14;                  // 设置字号
                // 设置各种状态下的文字颜色，确保一致性
                _boldStyle.normal.textColor = Color.black;
                _boldStyle.focused.textColor = Color.black;
                _boldStyle.hover.textColor = Color.black;
                _boldStyle.active.textColor = Color.black;
            }
            
            // 初始化居中文本样式
            if (_centerStyle == null)
            {
                _centerStyle = new GUIStyle(GUI.skin.label);
                _centerStyle.font = GUI.skin.font;
                _centerStyle.alignment = TextAnchor.MiddleCenter; // 设置文本居中对齐
                _centerStyle.normal.textColor = Color.gray;
                _centerStyle.focused.textColor = Color.gray;
                _centerStyle.hover.textColor = Color.gray;
                _centerStyle.active.textColor = Color.gray;
            }
            
            // 初始化自定义按钮样式
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button); // 基于默认button样式创建
                _buttonStyle.font = GUI.skin.font;
                _buttonStyle.fontSize = 12;
                _buttonStyle.normal.textColor = Color.black;
                _buttonStyle.focused.textColor = Color.black;
                _buttonStyle.hover.textColor = Color.black;
                _buttonStyle.active.textColor = Color.black;
            }
        }
        
        #endregion
        
        #region 编辑器UI界面 (IMGUI)
        
        /// <summary>
        /// IMGUI窗口的绘制回调函数。由GUI.Window调用。
        /// </summary>
        /// <param name="windowID">窗口的唯一ID。</param>
        private void ConfigEditorWindow(int windowID)
        {
            // 尝试强制修复IMGUI字体可能为空的问题，尤其是在编辑器中某些情况下
            try
            {
                if (GUI.skin.font == null)
                {
                    // 尝试获取几种常见的Unity内置字体或项目中存在的字体
                    var defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (defaultFont == null) defaultFont = Resources.Load<Font>("Arial"); // 尝试从Resources文件夹加载
                    if (defaultFont == null && Resources.FindObjectsOfTypeAll<Font>().Length > 0)
                        defaultFont = Resources.FindObjectsOfTypeAll<Font>()[0]; // 使用项目中找到的第一个字体
                    
                    // 如果成功获取到字体，则应用到GUI.skin
                    if (defaultFont != null)
                    {
                        GUI.skin.font = defaultFont;
                        GUI.skin.label.font = defaultFont;
                        GUI.skin.button.font = defaultFont;
                        GUI.skin.box.font = defaultFont;
                        // Debug.Log($"[ConfigEditorWindow] IMGUI 默认字体已设置为: {defaultFont.name}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ConfigEditorWindow] IMGUI 字体加载或设置时发生异常: {e.Message}");
            }
            
            // 初始化或刷新IMGUI样式 (确保字体已设置)
            InitializeStyles();
            
            GUILayout.BeginVertical(); // 开始垂直布局组
            
            // 简易字体渲染测试区域 (用于直观检查字体是否正常工作)
            GUILayout.Label("=== IMGUI 字体测试区域 ===", _boldStyle); // 使用加粗样式
            GUILayout.Box("这是一个Box控件中的测试文本。");
            
            var testStyle = new GUIStyle(GUI.skin.label); // 创建一个临时测试样式
            testStyle.normal.textColor = Color.red;
            testStyle.fontSize = 20;
            if (GUI.skin.font != null) testStyle.font = GUI.skin.font; // 确保使用当前皮肤字体
            GUILayout.Label("这是一段红色的测试文本，字号20。", testStyle);
            
            // 原生GUI.Label方法测试 (不使用GUILayout)
            GUI.Label(new Rect(10, _windowRect.height - 30, 200, 20), "原生GUI.Label测试文本"); // 放在窗口底部
            
            GUILayout.Label("=== 测试区域结束 ===", _boldStyle);
            GUILayout.Space(10); // 添加一些垂直间距
            
            // 实现标签页 (Tab) 切换按钮
            GUILayout.BeginHorizontal(); // 开始水平布局组用于放置标签按钮
            // 每个GUILayout.Toggle创建一个类似标签页的按钮。当按钮被选中时，对应布尔值设为true，其他设为false。
            if (GUILayout.Toggle(_showBuildingPanel, "建筑配置", "Button")) { SelectPanel(ref _showBuildingPanel); }
            if (GUILayout.Toggle(_showTechPanel, "科技配置", "Button")) { SelectPanel(ref _showTechPanel); }
            if (GUILayout.Toggle(_showResourcePanel, "资源配置", "Button")) { SelectPanel(ref _showResourcePanel); }
            if (GUILayout.Toggle(_showRecipePanel, "制造配方", "Button")) { SelectPanel(ref _showRecipePanel); }
            GUILayout.EndHorizontal(); // 结束水平布局组
            
            GUILayout.Space(10); // 标签按钮和内容区之间的间距
            
            // 开始滚动视图，用于容纳可能超出窗口高度的内容
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
            
            // 根据当前选中的标签页，绘制对应的配置面板内容
            if (_showBuildingPanel) { DrawBuildingPanel(); }
            else if (_showTechPanel) { DrawTechPanel(); }
            else if (_showResourcePanel) { DrawResourcePanel(); }
            else if (_showRecipePanel) { DrawRecipePanel(); }
            
            GUILayout.EndScrollView(); // 结束滚动视图
            
            // 底部常用操作按钮区域
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("加载所有配置到系统", _buttonStyle)) { LoadAllConfigs(); } // 使用自定义按钮样式
            if (GUILayout.Button("验证所有配置数据", _buttonStyle)) { ValidateAllConfigs(); }
            if (GUILayout.Button("导出所有配置到JSON", _buttonStyle)) { ExportConfigsToJson(); }
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical(); // 结束主垂直布局组
            
            // 允许通过拖拽窗口标题栏来移动窗口
            GUI.DragWindow();
        }

        /// <summary>
        /// 辅助方法，用于处理标签页的选择逻辑。
        /// </summary>
        private void SelectPanel(ref bool panelToShow)
        {
            _showBuildingPanel = false;
            _showTechPanel = false;
            _showResourcePanel = false;
            _showRecipePanel = false;
            panelToShow = true;
        }
        
        /// <summary>
        /// 绘制建筑配置相关的IMGUI界面。
        /// </summary>
        private void DrawBuildingPanel()
        {
            GUILayout.Box("建筑配置管理", _boldStyle); // 面板标题，使用粗体样式
            
            // “快速创建建筑”按钮区域
            GUILayout.BeginVertical("box"); // 使用"box"样式创建一个带边框的垂直组
            GUILayout.Label("快速创建新建筑:", _boldStyle);
            GUILayout.BeginHorizontal();
            // 为每种建筑类型创建一个快速创建按钮
            if (GUILayout.Button("生产型", _buttonStyle)) { CreateQuickBuildingConfig(BuildingCategory.Production); }
            if (GUILayout.Button("防御型", _buttonStyle)) { CreateQuickBuildingConfig(BuildingCategory.Defense); }
            if (GUILayout.Button("居住型", _buttonStyle)) { CreateQuickBuildingConfig(BuildingCategory.Habitat); }
            if (GUILayout.Button("储存型", _buttonStyle)) { CreateQuickBuildingConfig(BuildingCategory.Storage); }
            if (GUILayout.Button("功能型", _buttonStyle)) { CreateQuickBuildingConfig(BuildingCategory.Functional); }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // 显示现有建筑配置列表
            GUILayout.Box($"现有建筑配置 ({_buildingConfigs.Count})", _boldStyle);
            if (_buildingConfigs.Count == 0)
            {
                GUILayout.Label("暂无已加载的建筑配置。", _centerStyle); // 居中显示提示信息
            }
            else
            {
                // 遍历并显示每个建筑配置项
                for (int i = 0; i < _buildingConfigs.Count; i++)
                {
                    var configAsset = _buildingConfigs[i];
                    if (configAsset?.Config != null) // 确保配置资源及其内部Config存在
                    {
                        GUILayout.BeginHorizontal("box"); // 每个配置项用一个带边框的水平组显示
                        GUILayout.Label($"ID: {configAsset.Config.ConfigId}", GUILayout.Width(200));
                        GUILayout.Label($"名称: {configAsset.Config.Name}", GUILayout.Width(150));
                        GUILayout.Label($"类型: {GetCategoryName(configAsset.Config.Category)}", GUILayout.Width(100));
                        GUILayout.Label($"等级: {configAsset.Config.Level}", GUILayout.Width(80));
                        if (GUILayout.Button("编辑", _buttonStyle, GUILayout.Width(60))) { EditBuildingConfig(configAsset); }
                        if (GUILayout.Button("移除", _buttonStyle, GUILayout.Width(60))) { RemoveBuildingConfig(i); i--; } // i-- 是因为移除了元素后列表会变动
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
        
        /// <summary>
        /// 绘制科技配置相关的IMGUI界面。
        /// </summary>
        private void DrawTechPanel()
        {
            GUILayout.Box("科技配置管理", _boldStyle);
            
            GUILayout.BeginVertical("box");
            GUILayout.Label("快速创建新科技:", _boldStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生存科技", _buttonStyle)) { CreateQuickTechConfig(TechTree.Survival); }
            if (GUILayout.Button("防御科技", _buttonStyle)) { CreateQuickTechConfig(TechTree.Defense); }
            if (GUILayout.Button("居住科技", _buttonStyle)) { CreateQuickTechConfig(TechTree.Habitat); }
            if (GUILayout.Button("工程科技", _buttonStyle)) { CreateQuickTechConfig(TechTree.Engineering); }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            GUILayout.Box($"现有科技配置 ({_techConfigs.Count})", _boldStyle);
            if (_techConfigs.Count == 0)
            {
                GUILayout.Label("暂无已加载的科技配置。", _centerStyle);
            }
            else
            {
                for (int i = 0; i < _techConfigs.Count; i++)
                {
                    var configAsset = _techConfigs[i];
                    if (configAsset?.Config != null)
                    {
                        GUILayout.BeginHorizontal("box");
                        GUILayout.Label($"ID: {configAsset.Config.Id}", GUILayout.Width(200));
                        GUILayout.Label($"名称: {configAsset.Config.Name}", GUILayout.Width(150));
                        GUILayout.Label($"科技树: {GetTechTreeName(configAsset.Config.Tree)}", GUILayout.Width(100));
                        GUILayout.Label($"等级段: {GetTechTierName(configAsset.Config.Tier)}", GUILayout.Width(80));
                        GUILayout.Label($"点数: {configAsset.Config.ResearchPointsCost}", GUILayout.Width(70));
                        if (GUILayout.Button("编辑", _buttonStyle, GUILayout.Width(60))) { EditTechConfig(configAsset); }
                        if (GUILayout.Button("移除", _buttonStyle, GUILayout.Width(60))) { RemoveTechConfig(i); i--; }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
        
        /// <summary>
        /// 绘制资源配置相关的IMGUI界面。
        /// </summary>
        private void DrawResourcePanel()
        {
            GUILayout.Box("资源配置管理", _boldStyle);
            
            GUILayout.BeginVertical("box");
            GUILayout.Label("快速创建新资源:", _boldStyle);
            GUILayout.BeginHorizontal();
            // 为常见的资源类型提供快速创建按钮
            if (GUILayout.Button("基础材料", _buttonStyle)) { CreateQuickResourceConfig(ResourceType.Materials); }
            if (GUILayout.Button("食物", _buttonStyle)) { CreateQuickResourceConfig(ResourceType.Food); }
            if (GUILayout.Button("工具", _buttonStyle)) { CreateQuickResourceConfig(ResourceType.Tools); }
            if (GUILayout.Button("医疗用品", _buttonStyle)) { CreateQuickResourceConfig(ResourceType.MedicalSupplies); }
            if (GUILayout.Button("燃料", _buttonStyle)) { CreateQuickResourceConfig(ResourceType.Fuel); }
            GUILayout.EndHorizontal();
            // 也可以提供一个通用创建按钮，让用户选择类型
            if (GUILayout.Button("创建自定义类型资源", _buttonStyle)) { CreateNewResourceConfig(); }
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            GUILayout.Box($"现有资源配置 ({_resourceConfigs.Count})", _boldStyle);
            if (_resourceConfigs.Count == 0)
            {
                GUILayout.Label("暂无已加载的资源配置。", _centerStyle);
            }
            else
            {
                for (int i = 0; i < _resourceConfigs.Count; i++)
                {
                    var configAsset = _resourceConfigs[i];
                    if (configAsset?.Config != null)
                    {
                        GUILayout.BeginHorizontal("box");
                        GUILayout.Label($"类型: {GetResourceTypeName(configAsset.Config.Type)}", GUILayout.Width(120));
                        GUILayout.Label($"名称: {configAsset.Config.Name}", GUILayout.Width(150));
                        GUILayout.Label($"分类: {GetResourceCategoryName(configAsset.Config.Category)}", GUILayout.Width(100));
                        GUILayout.Label($"堆叠上限: {configAsset.Config.MaxStackSize}", GUILayout.Width(100));
                        if (GUILayout.Button("编辑", _buttonStyle, GUILayout.Width(60))) { EditResourceConfig(configAsset); }
                        if (GUILayout.Button("移除", _buttonStyle, GUILayout.Width(60))) { RemoveResourceConfig(i); i--; }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
        
        /// <summary>
        /// 绘制制造配方相关的IMGUI界面。
        /// </summary>
        private void DrawRecipePanel()
        {
            GUILayout.Box("制造配方管理", _boldStyle);
            
            GUILayout.BeginVertical("box");
            GUILayout.Label("快速创建新配方 (按产出类型):", _boldStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("工具配方", _buttonStyle)) { CreateQuickRecipeConfig(ResourceType.Tools); }
            if (GUILayout.Button("弹药配方", _buttonStyle)) { CreateQuickRecipeConfig(ResourceType.Ammunition); }
            if (GUILayout.Button("医疗配方", _buttonStyle)) { CreateQuickRecipeConfig(ResourceType.MedicalSupplies); }
            // ...可以为更多常见产出类型添加按钮
            GUILayout.EndHorizontal();
            if (GUILayout.Button("创建自定义产出配方", _buttonStyle)) { CreateNewCraftingRecipe(); }
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            GUILayout.Box($"现有制造配方 ({_craftingRecipes.Count})", _boldStyle);
            if (_craftingRecipes.Count == 0)
            {
                GUILayout.Label("暂无已加载的制造配方。", _centerStyle);
            }
            else
            {
                for (int i = 0; i < _craftingRecipes.Count; i++)
                {
                    var recipeAsset = _craftingRecipes[i];
                    if (recipeAsset?.Recipe != null)
                    {
                        GUILayout.BeginHorizontal("box");
                        GUILayout.Label($"ID: {recipeAsset.Recipe.Id}", GUILayout.Width(200));
                        GUILayout.Label($"名称: {recipeAsset.Recipe.Name}", GUILayout.Width(150));
                        GUILayout.Label($"产出: {GetResourceTypeName(recipeAsset.Recipe.OutputType)} x{recipeAsset.Recipe.OutputAmount}", GUILayout.Width(150));
                        GUILayout.Label($"时间: {recipeAsset.Recipe.CraftingTime}s", GUILayout.Width(80)); // 假设时间单位为秒
                        if (GUILayout.Button("编辑", _buttonStyle, GUILayout.Width(60))) { EditRecipeConfig(recipeAsset); }
                        if (GUILayout.Button("移除", _buttonStyle, GUILayout.Width(60))) { RemoveRecipeConfig(i); i--; }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
        
        #endregion
        
        #region 快速创建建筑配置 (Editor-Only)
        
        /// <summary>
        /// 为指定建筑类别创建一个预设的建筑配置，并打开保存对话框。
        /// (仅在Unity编辑器环境下可用)
        /// </summary>
        /// <param name="category">要创建的建筑的类别。</param>
        private void CreateQuickBuildingConfig(BuildingCategory category)
        {
            // 创建一个基于类别的配置模板
            var newConfig = CreateBuildingConfigTemplate(category);
            
#if UNITY_EDITOR
            // 创建ScriptableObject资源实例
            var newConfigAsset = ScriptableObject.CreateInstance<BuildingConfigAsset>();
            newConfigAsset.Config = newConfig; // 将模板数据赋给资源
            
            string categoryName = GetCategoryName(category); // 获取类别对应的中文名
            // 准备保存对话框的参数
            string defaultFileName = $"NewBuilding_{categoryName}_{System.DateTime.Now:yyyyMMddHHmmss}"; //更详细的默认文件名
            // 弹出保存对话框
            string path = EditorUtility.SaveFilePanelInProject(
                $"保存新的 {categoryName} 建筑配置",
                defaultFileName,
                "asset", 
                "请选择新建筑配置文件的保存位置");
                
            if (!string.IsNullOrEmpty(path)) // 如果用户选择了路径
            {
                AssetDatabase.CreateAsset(newConfigAsset, path); // 创建资源
                AssetDatabase.SaveAssets();                      // 保存资源数据库
                _buildingConfigs.Add(newConfigAsset);            // 添加到当前列表以在UI中显示
                Debug.Log($"成功创建新的 {categoryName} 建筑配置于: {path}");
                Selection.activeObject = newConfigAsset; // 在Project窗口中选中新创建的资源
            }
#endif
        }
        
        /// <summary>
        /// 根据建筑类别创建一个包含预设默认值的 BuildingConfig 对象。
        /// </summary>
        /// <param name="category">建筑类别。</param>
        /// <returns>填充了默认值的BuildingConfig实例。</returns>
        private BuildingConfig CreateBuildingConfigTemplate(BuildingCategory category)
        {
            var config = new BuildingConfig
            {
                // 通用默认值
                Category = category,
                Level = BuildingLevel.Level1,
                BuildCosts = new List<ResourceCost>(),
                RequiredTechs = new List<string>(),
                RequiredAttributes = new List<SurvivorAttribute>(),
                MaintenanceCosts = new List<ResourceCost>(),
                Productions = new List<ResourceProduction>(),
                AvailableRecipes = new List<CraftingRecipe>(), // 注意：直接引用CraftingRecipe可能导致循环依赖或数据冗余，通常应为配方ID列表
                StorageTypes = new List<ResourceType>(),
                Size = new Vector2Int(1, 1) // 默认占地1x1格子
            };
            
            // 根据不同建筑类型设置特定的默认值
            switch (category)
            {
                case BuildingCategory.Production:
                    config.ConfigId = $"production_bld_{System.DateTime.Now.Ticks}"; // 使用更明确的前缀
                    config.Name = "新的生产建筑";
                    config.Description = "这是一个用于生产各类资源的建筑。";
                    config.MaxWorkers = 2;
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 50 });
                    config.BuildTime = 60f; // 例如60秒
                    config.Productions.Add(new ResourceProduction 
                    { 
                        Type = ResourceType.Materials, // 示例：产出材料
                        BaseRate = 1f,                 // 每单位时间基础产率
                        WorkerBonus = 0.5f,            // 每个工人的额外加成
                        RequiresWorker = true          // 是否需要工人才能生产
                    });
                    break;
                    
                case BuildingCategory.Defense:
                    config.ConfigId = $"defense_bld_{System.DateTime.Now.Ticks}";
                    config.Name = "新的防御建筑";
                    config.Description = "用于保护基地免受威胁的防御设施。";
                    config.MaxWorkers = 1; // 防御塔可能需要操作员
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 40 });
                    config.BuildTime = 45f;
                    config.DefensePower = 50f; // 攻击力
                    config.DefenseRange = 3f;  // 攻击范围
                    break;
                    
                case BuildingCategory.Habitat:
                    config.ConfigId = $"habitat_bld_{System.DateTime.Now.Ticks}";
                    config.Name = "新的居住建筑";
                    config.Description = "为幸存者提供居住空间。";
                    config.MaxWorkers = 0; // 通常居住建筑不直接分配工人进行“工作”
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 30 });
                    config.BuildTime = 40f;
                    config.HousingCapacity = 4; // 提供4个床位
                    break;
                    
                case BuildingCategory.Storage:
                    config.ConfigId = $"storage_bld_{System.DateTime.Now.Ticks}";
                    config.Name = "新的储存建筑";
                    config.Description = "用于安全储存各类物资。";
                    config.MaxWorkers = 0;
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 25 });
                    config.BuildTime = 30f;
                    config.StorageCapacity = 200; // 总存储容量
                    // config.StorageTypes.Add(ResourceType.Food); // 示例：可存储食物
                    break;
                    
                case BuildingCategory.Functional:
                    config.ConfigId = $"functional_bld_{System.DateTime.Now.Ticks}";
                    config.Name = "新的功能建筑"; // 例如：研究室、通讯站等
                    config.Description = "提供特定支持或功能的建筑。";
                    config.MaxWorkers = 1;
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 60 });
                    config.BuildTime = 90f;
                    break;
            }
            
            return config;
        }
        
        /// <summary>
        /// 在Unity编辑器中选中指定的建筑配置资源，方便用户编辑。
        /// (仅在Unity编辑器环境下可用)
        /// </summary>
        private void EditBuildingConfig(BuildingConfigAsset configAsset)
        {
#if UNITY_EDITOR
            Selection.activeObject = configAsset; // 在Project窗口或Inspector中选中该资源
            EditorGUIUtility.PingObject(configAsset); // 如果在Project窗口，会高亮显示
#endif
        }
        
        /// <summary>
        /// 从当前列表中移除指定索引的建筑配置。
        /// 注意：此操作仅从控制器维护的列表中移除，并不会删除对应的ScriptableObject资源文件。
        /// </summary>
        private void RemoveBuildingConfig(int index)
        {
            if (index >= 0 && index < _buildingConfigs.Count)
            {
                var configAsset = _buildingConfigs[index];
                _buildingConfigs.RemoveAt(index); // 从列表中移除
                
                if (configAsset?.Config != null)
                {
                    Debug.Log($"已从编辑器列表中移除建筑配置: {configAsset.Config.Name} (ID: {configAsset.Config.ConfigId})。资源文件本身未被删除。");
                }
            }
        }
        
        /// <summary>
        /// 创建一个新的资源配置ScriptableObject。（此方法是通用的，不像QuickCreate那样针对特定类型）
        /// (仅在Unity编辑器环境下可用)
        /// </summary>
        private void CreateNewResourceConfig()
        {
#if UNITY_EDITOR
            var newConfigAsset = ScriptableObject.CreateInstance<ResourceConfigAsset>();
            newConfigAsset.Config = new ResourceConfig // 初始化一个默认的资源配置
             {
                 Type = ResourceType.Materials, // 默认类型
                 Name = "新资源名称",
                 Description = "请填写资源描述",
                 Category = ResourceCategory.BasicMaterial, // 默认分类
                 CanDecay = false,
                 DecayRate = 0f,
                 MaxStackSize = 100 // 默认最大堆叠数量
             };
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存新资源配置", "NewResourceConfig", "asset", "请选择保存新资源配置文件的位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path);
                AssetDatabase.SaveAssets();
                _resourceConfigs.Add(newConfigAsset);
                Debug.Log($"成功创建新的资源配置于: {path}");
                Selection.activeObject = newConfigAsset;
            }
#endif
        }
        
        /// <summary>
        /// 获取建筑类别枚举对应的中文字符串名称。
        /// </summary>
        private string GetCategoryName(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Production: return "生产型";
                case BuildingCategory.Defense:    return "防御型";
                case BuildingCategory.Habitat:    return "居住型";
                case BuildingCategory.Storage:    return "储存型";
                case BuildingCategory.Functional: return "功能型";
                default: return "未知类型";
            }
        }
        
        #endregion
        
        #region 科技配置管理 (Editor-Only)
        
        /// <summary>
        /// 为指定科技树分支创建一个预设的科技配置，并打开保存对话框。
        /// (仅在Unity编辑器环境下可用)
        /// </summary>
        private void CreateQuickTechConfig(TechTree techTree)
        {
            var newConfig = CreateTechConfigTemplate(techTree);
            
#if UNITY_EDITOR
            var newConfigAsset = ScriptableObject.CreateInstance<TechConfigAsset>();
            newConfigAsset.Config = newConfig;
            
            string treeName = GetTechTreeName(techTree);
            string defaultFileName = $"NewTech_{treeName}_{System.DateTime.Now:yyyyMMddHHmmss}";
            string path = EditorUtility.SaveFilePanelInProject(
                $"保存新的 {treeName} 科技配置", defaultFileName, "asset", "请选择新科技配置文件的保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path);
                AssetDatabase.SaveAssets();
                _techConfigs.Add(newConfigAsset);
                Debug.Log($"成功创建新的 {treeName} 科技配置于: {path}");
                Selection.activeObject = newConfigAsset;
            }
#endif
        }
        
        /// <summary>
        /// 根据科技树分支创建一个包含预设默认值的 TechConfig 对象。
        /// </summary>
        private TechConfig CreateTechConfigTemplate(TechTree techTree)
        {
            var config = new TechConfig
            {
                // 通用默认值
                Tree = techTree,
                Tier = TechTier.Tier1,
                Prerequisites = new List<string>(),
                AdditionalCosts = new List<ResourceCost>(),
                RequiredResearchers = new List<SurvivorAttribute>(),
                UnlockedBuildings = new List<string>(),
                UnlockedRecipes = new List<string>(),
                Effects = new List<TechEffect>(),
                UIPosition = Vector2.zero,
                UIColor = Color.gray // 默认灰色，可根据科技树调整
            };
            
            // 根据科技树类型设置特定的默认值
            switch (techTree)
            {
                case TechTree.Survival:
                    config.Id = $"tech_survival_{System.DateTime.Now.Ticks}";
                    config.Name = "新的生存科技";
                    config.Description = "提升幸存者基本生存能力的科技。";
                    config.ResearchPointsCost = 50;
                    config.ResearchTime = 2f; // 例如2小时
                    config.MinResearchLevel = 1; // 研究所等级要求
                    config.FailureRate = 0.1f; // 10%失败率
                    config.UIColor = new Color(0.5f, 0.8f, 0.5f); // 淡绿色
                    break;
                    
                case TechTree.Defense:
                    config.Id = $"tech_defense_{System.DateTime.Now.Ticks}";
                    config.Name = "新的防御科技";
                    config.Description = "增强基地防御工事和武器效能的科技。";
                    config.ResearchPointsCost = 75;
                    config.ResearchTime = 3f;
                    config.MinResearchLevel = 1;
                    config.FailureRate = 0.15f;
                    config.UIColor = new Color(0.8f, 0.5f, 0.5f); // 淡红色
                    break;
                    
                case TechTree.Habitat:
                    config.Id = $"tech_habitat_{System.DateTime.Now.Ticks}";
                    config.Name = "新的居住科技";
                    config.Description = "改善幸存者居住环境和生活质量的科技。";
                    config.ResearchPointsCost = 60;
                    config.ResearchTime = 2.5f;
                    config.MinResearchLevel = 1;
                    config.FailureRate = 0.1f;
                    config.UIColor = new Color(0.5f, 0.5f, 0.8f); // 淡蓝色
                    break;
                    
                case TechTree.Engineering:
                    config.Id = $"tech_engineering_{System.DateTime.Now.Ticks}";
                    config.Name = "新的工程科技";
                    config.Description = "解锁高级建筑和提升建造效率的科技。";
                    config.ResearchPointsCost = 100;
                    config.ResearchTime = 4f;
                    config.MinResearchLevel = 2; // 可能需要更高级的研究设施
                    config.FailureRate = 0.2f;
                    config.UIColor = new Color(0.8f, 0.8f, 0.5f); // 淡黄色
                    break;
            }
            
            return config;
        }
        
        /// <summary>
        /// 在Unity编辑器中选中指定的科技配置资源。
        /// </summary>
        private void EditTechConfig(TechConfigAsset configAsset)
        {
#if UNITY_EDITOR
            Selection.activeObject = configAsset;
            EditorGUIUtility.PingObject(configAsset);
#endif
        }
        
        /// <summary>
        /// 从列表中移除指定的科技配置。
        /// </summary>
        private void RemoveTechConfig(int index)
        {
            if (index >= 0 && index < _techConfigs.Count)
            {
                var configAsset = _techConfigs[index];
                _techConfigs.RemoveAt(index);
                if (configAsset?.Config != null)
                {
                    Debug.Log($"已从编辑器列表中移除科技配置: {configAsset.Config.Name} (ID: {configAsset.Config.Id})。资源文件本身未被删除。");
                }
            }
        }
        
        /// <summary>
        /// 获取科技树枚举对应的中文字符串名称。
        /// </summary>
        private string GetTechTreeName(TechTree tree)
        {
            switch (tree)
            {
                case TechTree.Survival:    return "生存";
                case TechTree.Defense:     return "防御";
                case TechTree.Habitat:     return "居住";
                case TechTree.Engineering: return "工程";
                default: return "未知分支";
            }
        }
        
        /// <summary>
        /// 获取科技等级段枚举对应的中文字符串名称。
        /// </summary>
        private string GetTechTierName(TechTier tier)
        {
            switch (tier)
            {
                case TechTier.Tier1: return "等级一";
                case TechTier.Tier2: return "等级二";
                case TechTier.Tier3: return "等级三";
                case TechTier.Tier4: return "等级四";
                default: return "未知等级";
            }
        }
        
        #endregion
        
        #region 资源配置管理 (Editor-Only)
        
        /// <summary>
        /// 为指定资源类型创建一个预设的资源配置，并打开保存对话框。
        /// (仅在Unity编辑器环境下可用)
        /// </summary>
        private void CreateQuickResourceConfig(ResourceType resourceType)
        {
            var newConfig = CreateResourceConfigTemplate(resourceType);
            
#if UNITY_EDITOR
            var newConfigAsset = ScriptableObject.CreateInstance<ResourceConfigAsset>();
            newConfigAsset.Config = newConfig;
            
            string typeName = GetResourceTypeName(resourceType);
            string defaultFileName = $"NewResource_{typeName}_{System.DateTime.Now:yyyyMMddHHmmss}";
            string path = EditorUtility.SaveFilePanelInProject(
                $"保存新的 {typeName} 资源配置", defaultFileName, "asset", "请选择新资源配置文件的保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path);
                AssetDatabase.SaveAssets();
                _resourceConfigs.Add(newConfigAsset);
                Debug.Log($"成功创建新的 {typeName} 资源配置于: {path}");
                Selection.activeObject = newConfigAsset;
            }
#endif
        }
        
        /// <summary>
        /// 根据资源类型创建一个包含预设默认值的 ResourceConfig 对象。
        /// </summary>
        private ResourceConfig CreateResourceConfigTemplate(ResourceType resourceType)
        {
            var config = new ResourceConfig
            {
                // 通用默认值
                Type = resourceType,
                CanDecay = false, // 大部分资源默认不腐烂
                DecayRate = 0f,
                RequiresSpecialStorage = false, // 大部分资源不需要特殊存储
                StorageRequirement = ""
            };
            
            // 根据资源类型设置特定的默认值
            switch (resourceType)
            {
                case ResourceType.Food:
                    config.Name = "新的食物资源";
                    config.Description = "为幸存者提供能量和营养的基本食物。";
                    config.Category = ResourceCategory.BasicMaterial; // 食物通常是基础消耗品
                    config.CanDecay = true;    // 食物会腐烂
                    config.DecayRate = 0.05f;  // 每日腐烂5% (示例)
                    config.MaxStackSize = 50;  // 每堆最大数量
                    break;
                    
                case ResourceType.Materials:
                    config.Name = "新的建筑材料";
                    config.Description = "用于建造和修复建筑的基础材料。";
                    config.Category = ResourceCategory.BasicMaterial;
                    config.MaxStackSize = 100;
                    break;
                    
                case ResourceType.Tools:
                    config.Name = "新的工具";
                    config.Description = "用于提高特定工作效率的工具。";
                    config.Category = ResourceCategory.Manufactured; // 工具是制造品
                    config.MaxStackSize = 20; // 工具通常堆叠较少
                    break;
                    
                case ResourceType.MedicalSupplies:
                    config.Name = "新的医疗用品";
                    config.Description = "用于治疗伤病和恢复健康的医疗物资。";
                    config.Category = ResourceCategory.Manufactured;
                    config.MaxStackSize = 30;
                    config.RequiresSpecialStorage = true; // 例如需要冷藏或无菌环境
                    config.StorageRequirement = "需要医疗柜或冷藏设施";
                    break;
                    
                case ResourceType.Fuel:
                    config.Name = "新的燃料";
                    config.Description = "为发电机、车辆等提供能源。";
                    config.Category = ResourceCategory.BasicMaterial; // 燃料可能是采集或简单加工
                    config.MaxStackSize = 75;
                    config.RequiresSpecialStorage = true; // 例如易燃，需防火存储
                    config.StorageRequirement = "需要安全的燃料储存罐";
                    break;
                    
                default: // 其他未特别指定的资源类型
                    config.Name = $"新的{GetResourceTypeName(resourceType)}"; // 使用类型名作为默认名
                    config.Description = "一种游戏内资源。";
                    config.Category = ResourceCategory.BasicMaterial; // 默认为基础物资
                    config.MaxStackSize = 50; // 通用堆叠上限
                    break;
            }
            
            return config;
        }
        
        /// <summary>
        /// 在Unity编辑器中选中指定的资源配置。
        /// </summary>
        private void EditResourceConfig(ResourceConfigAsset configAsset)
        {
#if UNITY_EDITOR
            Selection.activeObject = configAsset;
            EditorGUIUtility.PingObject(configAsset);
#endif
        }
        
        /// <summary>
        /// 从列表中移除指定的资源配置。
        /// </summary>
        private void RemoveResourceConfig(int index)
        {
            if (index >= 0 && index < _resourceConfigs.Count)
            {
                var configAsset = _resourceConfigs[index];
                _resourceConfigs.RemoveAt(index);
                if (configAsset?.Config != null)
                {
                    Debug.Log($"已从编辑器列表中移除资源配置: {configAsset.Config.Name} (类型: {configAsset.Config.Type})。资源文件本身未被删除。");
                }
            }
        }
        
        /// <summary>
        /// 获取资源类型枚举对应的中文字符串名称。
        /// </summary>
        private string GetResourceTypeName(ResourceType type)
        {
            // TODO: 此方法可以考虑使用DisplayAttribute或本地化系统来获取名称，而不是硬编码。
            switch (type)
            {
                case ResourceType.Food:            return "食物";
                case ResourceType.Materials:       return "材料";
                case ResourceType.Water:           return "水";
                case ResourceType.Scrap:           return "废料";
                case ResourceType.OrganicMatter:   return "有机物";
                case ResourceType.Ammunition:      return "弹药";
                case ResourceType.MedicalSupplies: return "医疗用品";
                case ResourceType.Tools:           return "工具";
                case ResourceType.Fuel:            return "燃料";
                case ResourceType.Electronics:     return "电子元件";
                case ResourceType.Energy:          return "能源";
                case ResourceType.ResearchPoints:  return "科研点数";
                case ResourceType.RareMetals:      return "稀有金属";
                default: return type.ToString(); // 对于未明确指定的，返回枚举的字符串表示
            }
        }
        
        /// <summary>
        /// 获取资源分类枚举对应的中文字符串名称。
        /// </summary>
        private string GetResourceCategoryName(ResourceCategory category)
        {
            switch (category)
            {
                case ResourceCategory.BasicMaterial: return "基础物资";
                case ResourceCategory.Manufactured:  return "制成品";
                case ResourceCategory.Advanced:      return "高级资源";
                default: return category.ToString();
            }
        }
        
        #endregion
        
        #region 制造配方配置管理 (Editor-Only)
        
        /// <summary>
        /// 为指定产出类型创建一个预设的制造配方，并打开保存对话框。
        /// (仅在Unity编辑器环境下可用)
        /// </summary>
        private void CreateQuickRecipeConfig(ResourceType outputType)
        {
            var newConfig = CreateRecipeConfigTemplate(outputType);
            
#if UNITY_EDITOR
            var newConfigAsset = ScriptableObject.CreateInstance<CraftingRecipeAsset>();
            newConfigAsset.Recipe = newConfig;
            
            string typeName = GetResourceTypeName(outputType);
            string defaultFileName = $"NewRecipe_{typeName}_{System.DateTime.Now:yyyyMMddHHmmss}";
            string path = EditorUtility.SaveFilePanelInProject(
                $"保存新的 {typeName} 产出配方", defaultFileName, "asset", "请选择新制造配方文件的保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path);
                AssetDatabase.SaveAssets();
                _craftingRecipes.Add(newConfigAsset);
                Debug.Log($"成功创建新的 {typeName} 产出配方于: {path}");
                Selection.activeObject = newConfigAsset;
            }
#endif
        }
        
        /// <summary>
        /// 根据产出资源类型创建一个包含预设默认值的 CraftingRecipe 对象。
        /// </summary>
        private CraftingRecipe CreateRecipeConfigTemplate(ResourceType outputType)
        {
            var config = new CraftingRecipe
            {
                // 通用默认值
                OutputType = outputType,
                OutputAmount = 1, // 默认产出1个
                InputCosts = new List<ResourceCost>(),
                RequiredTechLevel = 0, // 默认无科技等级要求
                RequiredBuildings = new List<string>() // 默认无特定建筑要求 (或可设为通用工作台)
            };
            
            // 根据产出类型设置特定的默认值
            switch (outputType)
            {
                case ResourceType.Tools:
                    config.Id = $"recipe_tools_{System.DateTime.Now.Ticks}";
                    config.Name = "新的工具制造配方";
                    config.Description = "用于制造基础工具的配方。";
                    config.OutputAmount = 1;
                    config.CraftingTime = 2f; // 例如2小时
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 10 });
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Scrap, Amount = 5 });
                    config.RequiredBuildings.Add("workshop_level1"); // 假设建筑ID "workshop_level1"
                    break;
                    
                case ResourceType.Ammunition:
                    config.Id = $"recipe_ammo_{System.DateTime.Now.Ticks}";
                    config.Name = "新的弹药制造配方";
                    config.Description = "用于制造基础弹药的配方。";
                    config.OutputAmount = 10; // 例如一次制造10发
                    config.CraftingTime = 1.5f;
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 5 });
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.RareMetals, Amount = 2 }); // 假设需要稀有金属
                    config.RequiredBuildings.Add("ammunition_factory_level1");
                    config.RequiredTechLevel = 1; // 假设需要1级相关科技
                    break;
                    
                case ResourceType.MedicalSupplies:
                    config.Id = $"recipe_meds_{System.DateTime.Now.Ticks}";
                    config.Name = "新的医疗用品配方";
                    config.Description = "用于制造基础医疗用品的配方。";
                    config.OutputAmount = 5;
                    config.CraftingTime = 3f;
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.OrganicMatter, Amount = 8 });
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Water, Amount = 3 });
                    config.RequiredBuildings.Add("medical_lab_level1");
                    config.RequiredTechLevel = 1;
                    break;
                    
                // 为其他类型的配方添加更多case...
                    
                default: // 通用配方模板
                    config.Id = $"recipe_generic_{System.DateTime.Now.Ticks}";
                    config.Name = $"新的{GetResourceTypeName(outputType)}配方";
                    config.Description = $"制造 {GetResourceTypeName(outputType)} 的配方。";
                    config.CraftingTime = 1f;
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 5 }); // 通用成本
                    // config.RequiredBuildings.Add("general_workshop"); // 通用工作台
                    break;
            }
            
            return config;
        }
        
        /// <summary>
        /// 在Unity编辑器中选中指定的制造配方资源。
        /// </summary>
        private void EditRecipeConfig(CraftingRecipeAsset configAsset)
        {
#if UNITY_EDITOR
            Selection.activeObject = configAsset;
            EditorGUIUtility.PingObject(configAsset);
#endif
        }
        
        /// <summary>
        /// 从列表中移除指定的制造配方。
        /// </summary>
        private void RemoveRecipeConfig(int index)
        {
            if (index >= 0 && index < _craftingRecipes.Count)
            {
                var recipeAsset = _craftingRecipes[index];
                _craftingRecipes.RemoveAt(index);
                if (recipeAsset?.Recipe != null)
                {
                    Debug.Log($"已从编辑器列表中移除制造配方: {recipeAsset.Recipe.Name} (ID: {recipeAsset.Recipe.Id})。资源文件本身未被删除。");
                }
            }
        }
        
        #endregion
        
        /// <summary>
        /// 实现IController接口，返回QFramework架构实例。
        /// </summary>
        public IArchitecture GetArchitecture()
        {
            return RegisterManager.Interface; // RegisterManager是QFramework中用于获取架构实例的类
        }
    }
    
    // ==============================================================================
    // 配置相关的ScriptableObject资源包装类定义
    // 这些类使得配置数据可以作为Unity中的.asset文件存在，方便在Inspector中编辑和管理。
    // ==============================================================================
    
    /// <summary>
    /// 建筑配置的ScriptableObject包装类。
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuildingConfig", menuName = "生存游戏配置/建筑配置 (Building)")] // 在Assets/Create菜单中显示的路径和名称
    public class BuildingConfigAsset : ScriptableObject
    {
        public BuildingConfig Config; // 实际的建筑配置数据
    }
    
    /// <summary>
    /// 科技配置的ScriptableObject包装类。
    /// </summary>
    [CreateAssetMenu(fileName = "NewTechConfig", menuName = "生存游戏配置/科技配置 (Tech)")]
    public class TechConfigAsset : ScriptableObject
    {
        public TechConfig Config; // 实际的科技配置数据
    }
    
    /// <summary>
    /// 资源配置的ScriptableObject包装类。
    /// </summary>
    [CreateAssetMenu(fileName = "NewResourceConfig", menuName = "生存游戏配置/资源配置 (Resource)")]
    public class ResourceConfigAsset : ScriptableObject
    {
        public ResourceConfig Config; // 实际的资源配置数据
    }
    
    /// <summary>
    /// 制造配方配置的ScriptableObject包装类。
    /// </summary>
    [CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "生存游戏配置/制造配方 (Crafting Recipe)")]
    public class CraftingRecipeAsset : ScriptableObject
    {
        public CraftingRecipe Recipe; // 实际的制造配方数据
    }
    
    #endregion
    
    #region JSON导出用数据结构
    
    /// <summary>
    /// 用于将所有配置数据聚合以便导出为单个JSON文件的辅助类。
    /// </summary>
    [System.Serializable] // 标记为可序列化，以便JsonUtility能够处理
    public class ConfigExportData
    {
        public List<BuildingConfig> Buildings;   // 建筑配置列表
        public List<TechConfig> Techs;       // 科技配置列表
        public List<ResourceConfig> Resources;   // 资源配置列表
        public List<CraftingRecipe> Recipes;     // 制造配方列表
    }
    
    #endregion
}