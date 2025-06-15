using System.Collections.Generic;
using MyGameNamespace;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame.Model;
using SurvivalGame.GameSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 配置编辑器控制器 - 在编辑器中管理游戏配置
    /// </summary>
    public class ConfigEditorController : MonoBehaviour, IController
    {
        [Header("配置管理")]
        [SerializeField] private bool _autoLoadConfigs = true;
        [SerializeField] private bool _showDebugInfo = true;
        [SerializeField] private bool _showEditorUI = true;
        [SerializeField] private bool _useUGUI = false; // 新增：使用uGUI而不是OnGUI
        
        [Header("建筑配置")]
        [SerializeField] private List<BuildingConfigAsset> _buildingConfigs = new List<BuildingConfigAsset>();
        
        [Header("科技配置")]
        [SerializeField] private List<TechConfigAsset> _techConfigs = new List<TechConfigAsset>();
        
        [Header("资源配置")]
        [SerializeField] private List<ResourceConfigAsset> _resourceConfigs = new List<ResourceConfigAsset>();
        
        [Header("制造配方")]
        [SerializeField] private List<CraftingRecipeAsset> _craftingRecipes = new List<CraftingRecipeAsset>();
        
        [Header("uGUI组件")]
        [SerializeField] private Canvas _uiCanvas;
        [SerializeField] private GameObject _uiPanel;
        
        private ConfigSystem _configSystem;
        
        // UI相关变量
        private bool _showBuildingPanel = true;
        private bool _showTechPanel = false;
        private bool _showResourcePanel = false;
        private bool _showRecipePanel = false;
        private Vector2 _scrollPosition = Vector2.zero;
        private Rect _windowRect = new Rect(20, 20, 1000, 700);
        
        // UI样式缓存
        private GUIStyle _boldStyle;
        private GUIStyle _centerStyle;
        private GUIStyle _buttonStyle;
        
        #region Unity生命周期
        
        private void Start()
        {
            if (_autoLoadConfigs)
            {
                LoadAllConfigs();
            }
        }
        
        private void OnGUI()
        {
            if (!_showEditorUI || !Application.isPlaying) return;
            
            // 初始化中文字体支持
            InitializeChineseFontSupport();
            
            // 显示字体状态信息
            GUI.Label(new Rect(10, 10, 300, 20), $"字体状态: {(GUI.skin.font != null ? GUI.skin.font.name : "无字体")}");
            
            // 提供切换选项
            if (GUI.Button(new Rect(10, 35, 150, 30), "切换到uGUI"))
            {
                _useUGUI = true;
                CreateUGUIInterface();
            }
            
            // 字体测试区域
            if (GUI.Button(new Rect(170, 35, 100, 30), "字体测试"))
            {
                TestChineseFont();
            }
            
            if (!_useUGUI)
            {
                _windowRect = GUI.Window(0, _windowRect, ConfigEditorWindow, "游戏配置编辑器");
            }
        }
        
        private void CreateUGUIInterface()
        {
            // 创建基本的uGUI界面作为备用方案
            if (_uiCanvas == null)
            {
                GameObject canvasObj = new GameObject("ConfigEditorCanvas");
                _uiCanvas = canvasObj.AddComponent<Canvas>();
                _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // 创建主面板
                GameObject panelObj = new GameObject("ConfigPanel");
                panelObj.transform.SetParent(_uiCanvas.transform);
                
                var panelImage = panelObj.AddComponent<Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                
                var rectTransform = panelObj.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
                rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                // 添加标题
                GameObject titleObj = new GameObject("Title");
                titleObj.transform.SetParent(panelObj.transform);
                var titleText = titleObj.AddComponent<Text>();
                titleText.text = "Game Config Editor (uGUI Version)";
                titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                titleText.fontSize = 20;
                titleText.color = Color.white;
                titleText.alignment = TextAnchor.MiddleCenter;
                
                var titleRect = titleObj.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 0.9f);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.offsetMin = Vector2.zero;
                titleRect.offsetMax = Vector2.zero;
                
                // 添加关闭按钮
                GameObject closeButtonObj = new GameObject("CloseButton");
                closeButtonObj.transform.SetParent(panelObj.transform);
                var closeButton = closeButtonObj.AddComponent<Button>();
                var closeButtonImage = closeButtonObj.AddComponent<Image>();
                closeButtonImage.color = Color.red;
                
                var buttonText = new GameObject("ButtonText");
                buttonText.transform.SetParent(closeButtonObj.transform);
                var buttonTextComponent = buttonText.AddComponent<Text>();
                buttonTextComponent.text = "Close";
                buttonTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                buttonTextComponent.fontSize = 14;
                buttonTextComponent.color = Color.white;
                buttonTextComponent.alignment = TextAnchor.MiddleCenter;
                
                var buttonTextRect = buttonText.GetComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;
                
                var closeButtonRect = closeButtonObj.GetComponent<RectTransform>();
                closeButtonRect.anchorMin = new Vector2(0.9f, 0.9f);
                closeButtonRect.anchorMax = new Vector2(1, 1);
                closeButtonRect.offsetMin = Vector2.zero;
                closeButtonRect.offsetMax = Vector2.zero;
                
                closeButton.onClick.AddListener(() => {
                    _useUGUI = false;
                    DestroyImmediate(canvasObj);
                    _uiCanvas = null;
                });
                
                _uiPanel = panelObj;
                
                Debug.Log("uGUI interface created successfully, this should display text properly");
            }
        }
        
        #endregion
        
        #region 中文字体支持
        
        private bool _chineseFontInitialized = false;
        
        /// <summary>
        /// 初始化中文字体支持
        /// </summary>
        private void InitializeChineseFontSupport()
        {
            if (_chineseFontInitialized) return;
            
            try
            {
                // 直接尝试手动设置系统字体
                Font chineseFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16);
                if (chineseFont == null)
                    chineseFont = Font.CreateDynamicFontFromOSFont("SimHei", 16);
                if (chineseFont == null)
                    chineseFont = Font.CreateDynamicFontFromOSFont("SimSun", 16);
                if (chineseFont == null)
                    chineseFont = Font.CreateDynamicFontFromOSFont("Arial Unicode MS", 16);
                
                if (chineseFont != null && GUI.skin != null)
                {
                    GUI.skin.font = chineseFont;
                    GUI.skin.label.font = chineseFont;
                    GUI.skin.button.font = chineseFont;
                    GUI.skin.box.font = chineseFont;
                    GUI.skin.toggle.font = chineseFont;
                    GUI.skin.textField.font = chineseFont;
                    GUI.skin.textArea.font = chineseFont;
                    
                    // 请求常用中文字符
                    string chineseChars = "建筑配置管理科技资源制造配方生产防御居住储存功能型建筑农场工坊医疗站瞭望塔图书馆采石场避难所围墙储存库实验室净水器加载验证导出所有配置等级类型名称编辑删除暂无可用快速创建切换字体状态测试区域结束原生方法显示正常红色样式临时帐篷研究角急救站研究台简易小屋医疗帐篷小型实验室住宅区研究所公寓楼高级医院科技中心豪华社区超级医疗中心储物箱食物储藏室仓库弹药库冷藏库大型仓库专业储存中心自动化仓库超级储存中心水井发电机房通讯站太阳能发电站贸易中心";
                    chineseFont.RequestCharactersInTexture(chineseChars, 12);
                    chineseFont.RequestCharactersInTexture(chineseChars, 14);
                    chineseFont.RequestCharactersInTexture(chineseChars, 16);
                    chineseFont.RequestCharactersInTexture(chineseChars, 18);
                    chineseFont.RequestCharactersInTexture(chineseChars, 20);
                    
                    Debug.Log($"中文字体设置成功: {chineseFont.name}");
                    _chineseFontInitialized = true;
                }
                else
                {
                    Debug.LogWarning("无法找到合适的中文字体，使用默认字体");
                    _chineseFontInitialized = true; // 仍然标记为已初始化，避免重复尝试
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"中文字体初始化失败: {e.Message}");
                _chineseFontInitialized = true; // 标记为已初始化，避免重复尝试
            }
        }
        
        /// <summary>
        /// 测试中文字体显示
        /// </summary>
        private void TestChineseFont()
        {
            Debug.Log("测试中文字体显示");
            Debug.Log("常用中文字符: 建筑配置管理科技资源制造配方");
            Debug.Log("建筑类型: 生产型、防御型、居住型、储存型、功能型");
            Debug.Log("操作: 加载、验证、导出、创建、编辑、删除");
            
            // 在Console中显示字体信息
            if (GUI.skin != null && GUI.skin.font != null)
            {
                Debug.Log($"当前字体: {GUI.skin.font.name}");
                Debug.Log($"字体支持Unicode: {GUI.skin.font.dynamic}");
            }
            else
            {
                Debug.LogWarning("GUI字体为空");
            }
        }
        
        #endregion
        
        #region 配置加载
        
        /// <summary>
        /// 加载所有配置
        /// </summary>
        [ContextMenu("加载所有配置")]
        public void LoadAllConfigs()
        {
            _configSystem = this.GetSystem<ConfigSystem>();
            
            if (_configSystem == null)
            {
                Debug.LogError("ConfigSystem 未初始化");
                return;
            }
            
            LoadBuildingConfigs();
            LoadTechConfigs();
            LoadResourceConfigs();
            LoadCraftingRecipes();
            
            if (_showDebugInfo)
            {
                Debug.Log($"配置加载完成 - 建筑:{_buildingConfigs.Count}, 科技:{_techConfigs.Count}, 资源:{_resourceConfigs.Count}, 配方:{_craftingRecipes.Count}");
            }
        }
        
        private void LoadBuildingConfigs()
        {
            foreach (var configAsset in _buildingConfigs)
            {
                if (configAsset != null && configAsset.Config != null)
                {
                    // 这里可以将配置数据注入到ConfigSystem中
                    // 由于ConfigSystem已经有内置配置，这里主要用于覆盖或扩展
                }
            }
        }
        
        private void LoadTechConfigs()
        {
            foreach (var configAsset in _techConfigs)
            {
                if (configAsset != null && configAsset.Config != null)
                {
                    // 注入科技配置
                }
            }
        }
        
        private void LoadResourceConfigs()
        {
            foreach (var configAsset in _resourceConfigs)
            {
                if (configAsset != null && configAsset.Config != null)
                {
                    // 注入资源配置
                }
            }
        }
        
        private void LoadCraftingRecipes()
        {
            foreach (var recipeAsset in _craftingRecipes)
            {
                if (recipeAsset != null && recipeAsset.Recipe != null)
                {
                    // 注入制造配方
                }
            }
        }
        
        #endregion
        
        #region 配置创建工具
        
        /// <summary>
        /// 创建新建筑配置
        /// </summary>
        [ContextMenu("创建新建筑配置")]
        public void CreateNewBuildingConfig()
        {
#if UNITY_EDITOR
            var newConfig = ScriptableObject.CreateInstance<BuildingConfigAsset>();
            newConfig.Config = new BuildingConfig
            {
                ConfigId = "new_building_" + System.DateTime.Now.Ticks,
                Name = "新建筑",
                Description = "建筑描述",
                Category = BuildingCategory.Production,
                Level = BuildingLevel.Level1,
                BuildCosts = new List<ResourceCost>(),
                BuildTime = 60f,
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
                Size = new Vector2Int(1, 1)
            };
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存建筑配置", 
                "NewBuildingConfig", 
                "asset", 
                "选择保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                _buildingConfigs.Add(newConfig);
                Debug.Log($"创建建筑配置: {path}");
            }
#endif
        }
        
        /// <summary>
        /// 创建新科技配置
        /// </summary>
        [ContextMenu("创建新科技配置")]
        public void CreateNewTechConfig()
        {
#if UNITY_EDITOR
            var newConfig = ScriptableObject.CreateInstance<TechConfigAsset>();
            newConfig.Config = new TechConfig
            {
                Id = "new_tech_" + System.DateTime.Now.Ticks,
                Name = "新科技",
                Description = "科技描述",
                Tree = TechTree.Survival,
                Tier = TechTier.Tier1,
                ResearchPointsCost = 50,
                ResearchTime = 2f,
                Prerequisites = new List<string>(),
                AdditionalCosts = new List<ResourceCost>(),
                MinResearchLevel = 1,
                RequiredResearchers = new List<SurvivorAttribute>(),
                FailureRate = 0.1f,
                UnlockedBuildings = new List<string>(),
                UnlockedRecipes = new List<string>(),
                Effects = new List<TechEffect>(),
                UIPosition = Vector2.zero,
                UIColor = Color.white
            };
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存科技配置", 
                "NewTechConfig", 
                "asset", 
                "选择保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                _techConfigs.Add(newConfig);
                Debug.Log($"创建科技配置: {path}");
            }
#endif
        }
        
        /// <summary>
        /// 创建新制造配方
        /// </summary>
        [ContextMenu("创建新制造配方")]
        public void CreateNewCraftingRecipe()
        {
#if UNITY_EDITOR
            var newRecipe = ScriptableObject.CreateInstance<CraftingRecipeAsset>();
            newRecipe.Recipe = new CraftingRecipe
            {
                Id = "new_recipe_" + System.DateTime.Now.Ticks,
                Name = "新配方",
                Description = "配方描述",
                OutputType = ResourceType.Tools,
                OutputAmount = 1,
                InputCosts = new List<ResourceCost>
                {
                    new ResourceCost { Type = ResourceType.Materials, Amount = 10 }
                },
                CraftingTime = 1f,
                RequiredTechLevel = 0,
                RequiredBuildings = new List<string> { "workshop_1" }
            };
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存制造配方", 
                "NewCraftingRecipe", 
                "asset", 
                "选择保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newRecipe, path);
                AssetDatabase.SaveAssets();
                _craftingRecipes.Add(newRecipe);
                Debug.Log($"创建制造配方: {path}");
            }
#endif
        }
        
        #endregion
        
        #region 配置验证
        
        /// <summary>
        /// 验证所有配置
        /// </summary>
        [ContextMenu("验证所有配置")]
        public void ValidateAllConfigs()
        {
            int errorCount = 0;
            
            errorCount += ValidateBuildingConfigs();
            errorCount += ValidateTechConfigs();
            errorCount += ValidateCraftingRecipes();
            
            if (errorCount == 0)
            {
                Debug.Log("所有配置验证通过！");
            }
            else
            {
                Debug.LogWarning($"发现 {errorCount} 个配置错误，请检查控制台输出");
            }
        }
        
        private int ValidateBuildingConfigs()
        {
            int errors = 0;
            var usedIds = new HashSet<string>();
            
            foreach (var configAsset in _buildingConfigs)
            {
                if (configAsset == null || configAsset.Config == null)
                {
                    Debug.LogError("发现空的建筑配置");
                    errors++;
                    continue;
                }
                
                var config = configAsset.Config;
                
                // 检查ID唯一性
                if (usedIds.Contains(config.ConfigId))
                {
                    Debug.LogError($"建筑配置ID重复: {config.ConfigId}");
                    errors++;
                }
                else
                {
                    usedIds.Add(config.ConfigId);
                }
                
                // 检查必填字段
                if (string.IsNullOrEmpty(config.Name))
                {
                    Debug.LogError($"建筑配置 {config.ConfigId} 缺少名称");
                    errors++;
                }
                
                if (config.BuildCosts == null || config.BuildCosts.Count == 0)
                {
                    Debug.LogWarning($"建筑配置 {config.ConfigId} 没有建造成本");
                }
                
                if (config.BuildTime <= 0)
                {
                    Debug.LogError($"建筑配置 {config.ConfigId} 建造时间无效");
                    errors++;
                }
            }
            
            return errors;
        }
        
        private int ValidateTechConfigs()
        {
            int errors = 0;
            var usedIds = new HashSet<string>();
            
            foreach (var configAsset in _techConfigs)
            {
                if (configAsset == null || configAsset.Config == null)
                {
                    Debug.LogError("发现空的科技配置");
                    errors++;
                    continue;
                }
                
                var config = configAsset.Config;
                
                // 检查ID唯一性
                if (usedIds.Contains(config.Id))
                {
                    Debug.LogError($"科技配置ID重复: {config.Id}");
                    errors++;
                }
                else
                {
                    usedIds.Add(config.Id);
                }
                
                // 检查必填字段
                if (string.IsNullOrEmpty(config.Name))
                {
                    Debug.LogError($"科技配置 {config.Id} 缺少名称");
                    errors++;
                }
                
                if (config.ResearchPointsCost <= 0)
                {
                    Debug.LogError($"科技配置 {config.Id} 研究点数成本无效");
                    errors++;
                }
                
                if (config.ResearchTime <= 0)
                {
                    Debug.LogError($"科技配置 {config.Id} 研究时间无效");
                    errors++;
                }
            }
            
            return errors;
        }
        
        private int ValidateCraftingRecipes()
        {
            int errors = 0;
            var usedIds = new HashSet<string>();
            
            foreach (var recipeAsset in _craftingRecipes)
            {
                if (recipeAsset == null || recipeAsset.Recipe == null)
                {
                    Debug.LogError("发现空的制造配方");
                    errors++;
                    continue;
                }
                
                var recipe = recipeAsset.Recipe;
                
                // 检查ID唯一性
                if (usedIds.Contains(recipe.Id))
                {
                    Debug.LogError($"制造配方ID重复: {recipe.Id}");
                    errors++;
                }
                else
                {
                    usedIds.Add(recipe.Id);
                }
                
                // 检查必填字段
                if (string.IsNullOrEmpty(recipe.Name))
                {
                    Debug.LogError($"制造配方 {recipe.Id} 缺少名称");
                    errors++;
                }
                
                if (recipe.InputCosts == null || recipe.InputCosts.Count == 0)
                {
                    Debug.LogWarning($"制造配方 {recipe.Id} 没有输入成本");
                }
                
                if (recipe.OutputAmount <= 0)
                {
                    Debug.LogError($"制造配方 {recipe.Id} 输出数量无效");
                    errors++;
                }
                
                if (recipe.CraftingTime <= 0)
                {
                    Debug.LogError($"制造配方 {recipe.Id} 制造时间无效");
                    errors++;
                }
            }
            
            return errors;
        }
        
        #endregion
        
        #region 配置导出
        
        /// <summary>
        /// 导出配置到JSON
        /// </summary>
        [ContextMenu("导出配置到JSON")]
        public void ExportConfigsToJson()
        {
#if UNITY_EDITOR
            var exportData = new ConfigExportData
            {
                Buildings = new List<BuildingConfig>(),
                Techs = new List<TechConfig>(),
                Resources = new List<ResourceConfig>(),
                Recipes = new List<CraftingRecipe>()
            };
            
            foreach (var asset in _buildingConfigs)
            {
                if (asset?.Config != null)
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
                if (asset?.Recipe != null)
                    exportData.Recipes.Add(asset.Recipe);
            }
            
            string json = JsonUtility.ToJson(exportData, true);
            string path = EditorUtility.SaveFilePanel("导出配置", "", "GameConfigs", "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"配置已导出到: {path}");
            }
#endif
        }
        
        #endregion
        
        #region UI样式初始化
        
        private void InitializeStyles()
        {
            if (_boldStyle == null)
            {
                _boldStyle = new GUIStyle(GUI.skin.label);
                _boldStyle.font = GUI.skin.font; // 确保字体设置
                _boldStyle.fontStyle = FontStyle.Bold;
                _boldStyle.fontSize = 14;
                _boldStyle.normal.textColor = Color.black;
                _boldStyle.focused.textColor = Color.black;
                _boldStyle.hover.textColor = Color.black;
                _boldStyle.active.textColor = Color.black;
            }
            
            if (_centerStyle == null)
            {
                _centerStyle = new GUIStyle(GUI.skin.label);
                _centerStyle.font = GUI.skin.font; // 确保字体设置
                _centerStyle.alignment = TextAnchor.MiddleCenter;
                _centerStyle.normal.textColor = Color.gray;
                _centerStyle.focused.textColor = Color.gray;
                _centerStyle.hover.textColor = Color.gray;
                _centerStyle.active.textColor = Color.gray;
            }
            
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.font = GUI.skin.font; // 确保字体设置
                _buttonStyle.fontSize = 12;
                _buttonStyle.normal.textColor = Color.black;
                _buttonStyle.focused.textColor = Color.black;
                _buttonStyle.hover.textColor = Color.black;
                _buttonStyle.active.textColor = Color.black;
            }
        }
        
        #endregion
        
        #region 编辑器UI界面
        
        private void ConfigEditorWindow(int windowID)
        {
            // 强制修复字体缺失问题
            try
            {
                // 尝试多种字体加载方式
                if (GUI.skin.font == null)
                {
                    var defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    if (defaultFont == null)
                        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (defaultFont == null)
                        defaultFont = Resources.Load<Font>("Arial");
                    if (defaultFont == null && Resources.FindObjectsOfTypeAll<Font>().Length > 0)
                        defaultFont = Resources.FindObjectsOfTypeAll<Font>()[0];
                    
                    if (defaultFont != null)
                    {
                        GUI.skin.font = defaultFont;
                        GUI.skin.label.font = defaultFont;
                        GUI.skin.button.font = defaultFont;
                        GUI.skin.box.font = defaultFont;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"字体加载失败: {e.Message}");
            }
            
            // 初始化样式
            InitializeStyles();
            
            GUILayout.BeginVertical();
            
            // 字体测试区域
            GUILayout.Label("=== 字体测试区域 ===");
            GUILayout.Box("盒子样式测试文本");
            
            var testStyle = new GUIStyle(GUI.skin.label);
            testStyle.normal.textColor = Color.red;
            testStyle.fontSize = 20;
            if (GUI.skin.font != null)
                testStyle.font = GUI.skin.font;
            GUILayout.Label("红色测试文本", testStyle);
            
            // 原生GUI方法测试
            GUI.Label(new Rect(10, 100, 200, 30), "原生GUI.Label测试");
            
            GUILayout.Label("=== 测试区域结束 ===");
            
            // 标签页按钮 - 中文版本
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_showBuildingPanel, "建筑", "Button"))
            {
                _showBuildingPanel = true;
                _showTechPanel = false;
                _showResourcePanel = false;
                _showRecipePanel = false;
            }
            if (GUILayout.Toggle(_showTechPanel, "科技", "Button"))
            {
                _showBuildingPanel = false;
                _showTechPanel = true;
                _showResourcePanel = false;
                _showRecipePanel = false;
            }
            if (GUILayout.Toggle(_showResourcePanel, "资源", "Button"))
            {
                _showBuildingPanel = false;
                _showTechPanel = false;
                _showResourcePanel = true;
                _showRecipePanel = false;
            }
            if (GUILayout.Toggle(_showRecipePanel, "配方", "Button"))
            {
                _showBuildingPanel = false;
                _showTechPanel = false;
                _showResourcePanel = false;
                _showRecipePanel = true;
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // 显示对应的面板
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            if (_showBuildingPanel)
            {
                DrawBuildingPanel();
            }
            else if (_showTechPanel)
            {
                DrawTechPanel();
            }
            else if (_showResourcePanel)
            {
                DrawResourcePanel();
            }
            else if (_showRecipePanel)
            {
                DrawRecipePanel();
            }
            
            GUILayout.EndScrollView();
            
            // 常用操作按钮
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("加载所有配置"))
            {
                LoadAllConfigs();
            }
            
            if (GUILayout.Button("验证配置"))
            {
                ValidateAllConfigs();
            }
            
            if (GUILayout.Button("导出配置"))
            {
                ExportConfigsToJson();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            // 使窗口可拖拽
            GUI.DragWindow();
        }
        
        private void DrawBuildingPanel()
        {
            // 建筑配置管理
            GUILayout.Box("建筑配置管理");
            
            // 新建筑按钮
            GUILayout.BeginVertical("box");
            GUILayout.Box("快速创建建筑:");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生产型"))
            {
                CreateQuickBuildingConfig(BuildingCategory.Production);
            }
            if (GUILayout.Button("防御型"))
            {
                CreateQuickBuildingConfig(BuildingCategory.Defense);
            }
            if (GUILayout.Button("居住型"))
            {
                CreateQuickBuildingConfig(BuildingCategory.Habitat);
            }
            if (GUILayout.Button("储存型"))
            {
                CreateQuickBuildingConfig(BuildingCategory.Storage);
            }
            if (GUILayout.Button("功能型"))
            {
                CreateQuickBuildingConfig(BuildingCategory.Functional);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // 现有建筑列表
            GUILayout.Box($"现有建筑配置 ({_buildingConfigs.Count})");
            
            for (int i = 0; i < _buildingConfigs.Count; i++)
            {
                var configAsset = _buildingConfigs[i];
                if (configAsset?.Config != null)
                {
                    GUILayout.BeginHorizontal("box");
                    
                    GUILayout.Box($"ID: {configAsset.Config.ConfigId}", GUILayout.Width(150));
                    GUILayout.Box($"名称: {configAsset.Config.Name}", GUILayout.Width(120));
                    GUILayout.Box($"类型: {GetCategoryName(configAsset.Config.Category)}", GUILayout.Width(80));
                    GUILayout.Box($"等级: {configAsset.Config.Level}", GUILayout.Width(60));
                    
                    if (GUILayout.Button("编辑", GUILayout.Width(50)))
                    {
                        EditBuildingConfig(configAsset);
                    }
                    
                    if (GUILayout.Button("删除", GUILayout.Width(50)))
                    {
                        RemoveBuildingConfig(i);
                    }
                    
                    GUILayout.EndHorizontal();
                }
            }
            
            if (_buildingConfigs.Count == 0)
            {
                GUILayout.Box("暂无可用的建筑配置");
            }
        }
        
        private void DrawTechPanel()
        {
            // 科技配置管理
            GUILayout.Box("科技配置管理");
            
            // 快速创建科技按钮
            GUILayout.BeginVertical("box");
            GUILayout.Box("快速创建科技:");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生存科技"))
            {
                CreateQuickTechConfig(TechTree.Survival);
            }
            if (GUILayout.Button("防御科技"))
            {
                CreateQuickTechConfig(TechTree.Defense);
            }
            if (GUILayout.Button("居住科技"))
            {
                CreateQuickTechConfig(TechTree.Habitat);
            }
            if (GUILayout.Button("工程科技"))
            {
                CreateQuickTechConfig(TechTree.Engineering);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // 现有科技列表
            GUILayout.Box($"现有科技配置 ({_techConfigs.Count})");
            
            for (int i = 0; i < _techConfigs.Count; i++)
            {
                var configAsset = _techConfigs[i];
                if (configAsset?.Config != null)
                {
                    GUILayout.BeginHorizontal("box");
                    
                    GUILayout.Box($"ID: {configAsset.Config.Id}", GUILayout.Width(150));
                    GUILayout.Box($"名称: {configAsset.Config.Name}", GUILayout.Width(120));
                    GUILayout.Box($"科技树: {GetTechTreeName(configAsset.Config.Tree)}", GUILayout.Width(80));
                    GUILayout.Box($"等级: {GetTechTierName(configAsset.Config.Tier)}", GUILayout.Width(60));
                    GUILayout.Box($"研究点: {configAsset.Config.ResearchPointsCost}", GUILayout.Width(60));
                    
                    if (GUILayout.Button("编辑", GUILayout.Width(50)))
                    {
                        EditTechConfig(configAsset);
                    }
                    
                    if (GUILayout.Button("删除", GUILayout.Width(50)))
                    {
                        RemoveTechConfig(i);
                    }
                    
                    GUILayout.EndHorizontal();
                }
            }
            
            if (_techConfigs.Count == 0)
            {
                GUILayout.Box("暂无可用的科技配置");
            }
        }
        
        private void DrawResourcePanel()
        {
            // 资源配置管理
            GUILayout.Box("资源配置管理");
            
            // 快速创建资源按钮
            GUILayout.BeginVertical("box");
            GUILayout.Box("快速创建资源:");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("基础材料"))
            {
                CreateQuickResourceConfig(ResourceType.Materials);
            }
            if (GUILayout.Button("食物"))
            {
                CreateQuickResourceConfig(ResourceType.Food);
            }
            if (GUILayout.Button("工具"))
            {
                CreateQuickResourceConfig(ResourceType.Tools);
            }
            if (GUILayout.Button("医疗用品"))
            {
                CreateQuickResourceConfig(ResourceType.MedicalSupplies);
            }
            if (GUILayout.Button("燃料"))
            {
                CreateQuickResourceConfig(ResourceType.Fuel);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // 现有资源列表
            GUILayout.Box($"现有资源配置 ({_resourceConfigs.Count})");
            
            for (int i = 0; i < _resourceConfigs.Count; i++)
            {
                var configAsset = _resourceConfigs[i];
                if (configAsset?.Config != null)
                {
                    GUILayout.BeginHorizontal("box");
                    
                    GUILayout.Box($"类型: {GetResourceTypeName(configAsset.Config.Type)}", GUILayout.Width(120));
                    GUILayout.Box($"名称: {configAsset.Config.Name}", GUILayout.Width(100));
                    GUILayout.Box($"分类: {GetResourceCategoryName(configAsset.Config.Category)}", GUILayout.Width(80));
                    GUILayout.Box($"堆叠: {configAsset.Config.MaxStackSize}", GUILayout.Width(60));
                    
                    if (GUILayout.Button("编辑", GUILayout.Width(50)))
                    {
                        EditResourceConfig(configAsset);
                    }
                    
                    if (GUILayout.Button("删除", GUILayout.Width(50)))
                    {
                        RemoveResourceConfig(i);
                    }
                    
                    GUILayout.EndHorizontal();
                }
            }
            
            if (_resourceConfigs.Count == 0)
            {
                GUILayout.Box("暂无可用的资源配置");
            }
        }
        
        private void DrawRecipePanel()
        {
            // 制造配方管理
            GUILayout.Box("制造配方管理");
            
            // 快速创建配方按钮
            GUILayout.BeginVertical("box");
            GUILayout.Box("快速创建配方:");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("工具配方"))
            {
                CreateQuickRecipeConfig(ResourceType.Tools);
            }
            if (GUILayout.Button("弹药配方"))
            {
                CreateQuickRecipeConfig(ResourceType.Ammunition);
            }
            if (GUILayout.Button("医疗配方"))
            {
                CreateQuickRecipeConfig(ResourceType.MedicalSupplies);
            }
            if (GUILayout.Button("燃料配方"))
            {
                CreateQuickRecipeConfig(ResourceType.Fuel);
            }
            if (GUILayout.Button("电子配方"))
            {
                CreateQuickRecipeConfig(ResourceType.Electronics);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // 现有配方列表
            GUILayout.Box($"现有制造配方 ({_craftingRecipes.Count})");
            
            for (int i = 0; i < _craftingRecipes.Count; i++)
            {
                var recipeAsset = _craftingRecipes[i];
                if (recipeAsset?.Recipe != null)
                {
                    GUILayout.BeginHorizontal("box");
                    
                    GUILayout.Box($"ID: {recipeAsset.Recipe.Id}", GUILayout.Width(150));
                    GUILayout.Box($"名称: {recipeAsset.Recipe.Name}", GUILayout.Width(120));
                    GUILayout.Box($"产出: {GetResourceTypeName(recipeAsset.Recipe.OutputType)}", GUILayout.Width(80));
                    GUILayout.Box($"数量: {recipeAsset.Recipe.OutputAmount}", GUILayout.Width(60));
                    GUILayout.Box($"时间: {recipeAsset.Recipe.CraftingTime}h", GUILayout.Width(60));
                    
                    if (GUILayout.Button("编辑", GUILayout.Width(50)))
                    {
                        EditRecipeConfig(recipeAsset);
                    }
                    
                    if (GUILayout.Button("删除", GUILayout.Width(50)))
                    {
                        RemoveRecipeConfig(i);
                    }
                    
                    GUILayout.EndHorizontal();
                }
            }
            
            if (_craftingRecipes.Count == 0)
            {
                GUILayout.Box("暂无可用的制造配方");
            }
        }
        
        #endregion
        
        #region 快速创建建筑
        
        private void CreateQuickBuildingConfig(BuildingCategory category)
        {
            var newConfig = CreateBuildingConfigTemplate(category);
            
#if UNITY_EDITOR
            var newConfigAsset = ScriptableObject.CreateInstance<BuildingConfigAsset>();
            newConfigAsset.Config = newConfig;
            
            string categoryName = GetCategoryName(category);
            string path = EditorUtility.SaveFilePanelInProject(
                $"保存{categoryName}建筑配置", 
                $"New{categoryName}Building", 
                "asset", 
                "选择保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path);
                AssetDatabase.SaveAssets();
                _buildingConfigs.Add(newConfigAsset);
                Debug.Log($"创建{categoryName}建筑配置: {path}");
            }
#endif
        }
        
        private BuildingConfig CreateBuildingConfigTemplate(BuildingCategory category)
        {
            var config = new BuildingConfig
            {
                Category = category,
                Level = BuildingLevel.Level1,
                BuildCosts = new List<ResourceCost>(),
                RequiredTechs = new List<string>(),
                RequiredAttributes = new List<SurvivorAttribute>(),
                MaintenanceCosts = new List<ResourceCost>(),
                Productions = new List<ResourceProduction>(),
                AvailableRecipes = new List<CraftingRecipe>(),
                StorageTypes = new List<ResourceType>(),
                Size = new Vector2Int(1, 1)
            };
            
            // 根据类型设置默认值
            switch (category)
            {
                case BuildingCategory.Production:
                    config.ConfigId = $"production_building_{System.DateTime.Now.Ticks}";
                    config.Name = "新生产建筑";
                    config.Description = "生产各种资源的建筑";
                    config.MaxWorkers = 2;
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 50 });
                    config.BuildTime = 60f;
                    config.Productions.Add(new ResourceProduction 
                    { 
                        Type = ResourceType.Materials, 
                        BaseRate = 1f, 
                        WorkerBonus = 0.5f, 
                        RequiresWorker = true 
                    });
                    break;
                    
                case BuildingCategory.Defense:
                    config.ConfigId = $"defense_building_{System.DateTime.Now.Ticks}";
                    config.Name = "新防御建筑";
                    config.Description = "保护基地安全的防御设施";
                    config.MaxWorkers = 1;
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 40 });
                    config.BuildTime = 45f;
                    config.DefensePower = 50f;
                    config.DefenseRange = 3f;
                    break;
                    
                case BuildingCategory.Habitat:
                    config.ConfigId = $"habitat_building_{System.DateTime.Now.Ticks}";
                    config.Name = "新居住建筑";
                    config.Description = "为幸存者提供住所";
                    config.MaxWorkers = 0;
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 30 });
                    config.BuildTime = 40f;
                    config.HousingCapacity = 4;
                    break;
                    
                case BuildingCategory.Storage:
                    config.ConfigId = $"storage_building_{System.DateTime.Now.Ticks}";
                    config.Name = "新储存建筑";
                    config.Description = "储存各种物资";
                    config.MaxWorkers = 0;
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 25 });
                    config.BuildTime = 30f;
                    config.StorageCapacity = 200;
                    break;
                    
                case BuildingCategory.Functional:
                    config.ConfigId = $"functional_building_{System.DateTime.Now.Ticks}";
                    config.Name = "新功能建筑";
                    config.Description = "提供特殊功能的建筑";
                    config.MaxWorkers = 1;
                    config.BuildCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 60 });
                    config.BuildTime = 90f;
                    break;
            }
            
            return config;
        }
        
        private void EditBuildingConfig(BuildingConfigAsset configAsset)
        {
#if UNITY_EDITOR
            Selection.activeObject = configAsset;
            EditorGUIUtility.PingObject(configAsset);
#endif
        }
        
        private void RemoveBuildingConfig(int index)
        {
            if (index >= 0 && index < _buildingConfigs.Count)
            {
                var configAsset = _buildingConfigs[index];
                _buildingConfigs.RemoveAt(index);
                
                if (configAsset?.Config != null)
                {
                    Debug.Log($"移除建筑配置: {configAsset.Config.Name}");
                }
            }
        }
        
        private void CreateNewResourceConfig()
        {
#if UNITY_EDITOR
            var newConfig = ScriptableObject.CreateInstance<ResourceConfigAsset>();
                         newConfig.Config = new ResourceConfig
             {
                 Type = ResourceType.Materials,
                 Name = "新资源",
                 Description = "资源描述",
                 Category = ResourceCategory.BasicMaterial,
                 CanDecay = false,
                 DecayRate = 0f,
                 MaxStackSize = 100
             };
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存资源配置", 
                "NewResourceConfig", 
                "asset", 
                "选择保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                _resourceConfigs.Add(newConfig);
                Debug.Log($"创建资源配置: {path}");
            }
#endif
        }
        
        private string GetCategoryName(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Production: return "生产型";
                case BuildingCategory.Defense: return "防御型";
                case BuildingCategory.Habitat: return "居住型";
                case BuildingCategory.Storage: return "储存型";
                case BuildingCategory.Functional: return "功能型";
                default: return "未知类型";
            }
        }
        
        #endregion
        
        #region 科技配置管理
        
        private void CreateQuickTechConfig(TechTree techTree)
        {
            var newConfig = CreateTechConfigTemplate(techTree);
            
#if UNITY_EDITOR
            var newConfigAsset = ScriptableObject.CreateInstance<TechConfigAsset>();
            newConfigAsset.Config = newConfig;
            
            string treeName = GetTechTreeName(techTree);
            string path = EditorUtility.SaveFilePanelInProject(
                $"保存{treeName}科技配置", 
                $"New{treeName}Tech", 
                "asset", 
                "选择保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path);
                AssetDatabase.SaveAssets();
                _techConfigs.Add(newConfigAsset);
                Debug.Log($"创建{treeName}科技配置: {path}");
            }
#endif
        }
        
        private TechConfig CreateTechConfigTemplate(TechTree techTree)
        {
            var config = new TechConfig
            {
                Tree = techTree,
                Tier = TechTier.Tier1,
                Prerequisites = new List<string>(),
                AdditionalCosts = new List<ResourceCost>(),
                RequiredResearchers = new List<SurvivorAttribute>(),
                UnlockedBuildings = new List<string>(),
                UnlockedRecipes = new List<string>(),
                Effects = new List<TechEffect>(),
                UIPosition = Vector2.zero,
                UIColor = Color.white
            };
            
            // 根据科技树类型设置默认值
            switch (techTree)
            {
                case TechTree.Survival:
                    config.Id = $"survival_tech_{System.DateTime.Now.Ticks}";
                    config.Name = "新生存科技";
                    config.Description = "提升生存能力的科技";
                    config.ResearchPointsCost = 50;
                    config.ResearchTime = 2f;
                    config.MinResearchLevel = 1;
                    config.FailureRate = 0.1f;
                    break;
                    
                case TechTree.Defense:
                    config.Id = $"defense_tech_{System.DateTime.Now.Ticks}";
                    config.Name = "新防御科技";
                    config.Description = "增强防御能力的科技";
                    config.ResearchPointsCost = 75;
                    config.ResearchTime = 3f;
                    config.MinResearchLevel = 1;
                    config.FailureRate = 0.15f;
                    break;
                    
                case TechTree.Habitat:
                    config.Id = $"habitat_tech_{System.DateTime.Now.Ticks}";
                    config.Name = "新居住科技";
                    config.Description = "改善居住条件的科技";
                    config.ResearchPointsCost = 60;
                    config.ResearchTime = 2.5f;
                    config.MinResearchLevel = 1;
                    config.FailureRate = 0.1f;
                    break;
                    
                case TechTree.Engineering:
                    config.Id = $"engineering_tech_{System.DateTime.Now.Ticks}";
                    config.Name = "新工程科技";
                    config.Description = "提升工程建造能力的科技";
                    config.ResearchPointsCost = 100;
                    config.ResearchTime = 4f;
                    config.MinResearchLevel = 2;
                    config.FailureRate = 0.2f;
                    break;
            }
            
            return config;
        }
        
        private void EditTechConfig(TechConfigAsset configAsset)
        {
#if UNITY_EDITOR
            Selection.activeObject = configAsset;
            EditorGUIUtility.PingObject(configAsset);
#endif
        }
        
        private void RemoveTechConfig(int index)
        {
            if (index >= 0 && index < _techConfigs.Count)
            {
                var configAsset = _techConfigs[index];
                _techConfigs.RemoveAt(index);
                
                if (configAsset?.Config != null)
                {
                    Debug.Log($"移除科技配置: {configAsset.Config.Name}");
                }
            }
        }
        
        private string GetTechTreeName(TechTree tree)
        {
            switch (tree)
            {
                case TechTree.Survival: return "生存";
                case TechTree.Defense: return "防御";
                case TechTree.Habitat: return "居住";
                case TechTree.Engineering: return "工程";
                default: return "未知";
            }
        }
        
        private string GetTechTierName(TechTier tier)
        {
            switch (tier)
            {
                case TechTier.Tier1: return "一级";
                case TechTier.Tier2: return "二级";
                case TechTier.Tier3: return "三级";
                case TechTier.Tier4: return "四级";
                default: return "未知";
            }
        }
        
        #endregion
        
        #region 资源配置管理
        
        private void CreateQuickResourceConfig(ResourceType resourceType)
        {
            var newConfig = CreateResourceConfigTemplate(resourceType);
            
#if UNITY_EDITOR
            var newConfigAsset = ScriptableObject.CreateInstance<ResourceConfigAsset>();
            newConfigAsset.Config = newConfig;
            
            string typeName = GetResourceTypeName(resourceType);
            string path = EditorUtility.SaveFilePanelInProject(
                $"保存{typeName}资源配置", 
                $"New{typeName}Resource", 
                "asset", 
                "选择保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path);
                AssetDatabase.SaveAssets();
                _resourceConfigs.Add(newConfigAsset);
                Debug.Log($"创建{typeName}资源配置: {path}");
            }
#endif
        }
        
        private ResourceConfig CreateResourceConfigTemplate(ResourceType resourceType)
        {
            var config = new ResourceConfig
            {
                Type = resourceType,
                CanDecay = false,
                DecayRate = 0f,
                RequiresSpecialStorage = false,
                StorageRequirement = ""
            };
            
            // 根据资源类型设置默认值
            switch (resourceType)
            {
                case ResourceType.Food:
                    config.Name = "新食物";
                    config.Description = "提供营养的食物资源";
                    config.Category = ResourceCategory.BasicMaterial;
                    config.CanDecay = true;
                    config.DecayRate = 0.1f;
                    config.MaxStackSize = 50;
                    break;
                    
                case ResourceType.Materials:
                    config.Name = "新材料";
                    config.Description = "用于建造的基础材料";
                    config.Category = ResourceCategory.BasicMaterial;
                    config.MaxStackSize = 100;
                    break;
                    
                case ResourceType.Tools:
                    config.Name = "新工具";
                    config.Description = "提高工作效率的工具";
                    config.Category = ResourceCategory.Manufactured;
                    config.MaxStackSize = 20;
                    break;
                    
                case ResourceType.MedicalSupplies:
                    config.Name = "新医疗用品";
                    config.Description = "治疗伤病的医疗物资";
                    config.Category = ResourceCategory.Manufactured;
                    config.MaxStackSize = 30;
                    config.RequiresSpecialStorage = true;
                    config.StorageRequirement = "需要医疗储存设施";
                    break;
                    
                case ResourceType.Fuel:
                    config.Name = "新燃料";
                    config.Description = "提供能源的燃料";
                    config.Category = ResourceCategory.BasicMaterial;
                    config.MaxStackSize = 75;
                    config.RequiresSpecialStorage = true;
                    config.StorageRequirement = "需要防火储存设施";
                    break;
                    
                default:
                    config.Name = "新资源";
                    config.Description = "资源描述";
                    config.Category = ResourceCategory.BasicMaterial;
                    config.MaxStackSize = 50;
                    break;
            }
            
            return config;
        }
        
        private void EditResourceConfig(ResourceConfigAsset configAsset)
        {
#if UNITY_EDITOR
            Selection.activeObject = configAsset;
            EditorGUIUtility.PingObject(configAsset);
#endif
        }
        
        private void RemoveResourceConfig(int index)
        {
            if (index >= 0 && index < _resourceConfigs.Count)
            {
                var configAsset = _resourceConfigs[index];
                _resourceConfigs.RemoveAt(index);
                
                if (configAsset?.Config != null)
                {
                    Debug.Log($"移除资源配置: {configAsset.Config.Name}");
                }
            }
        }
        
        private string GetResourceTypeName(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Food: return "食物";
                case ResourceType.Materials: return "材料";
                case ResourceType.Water: return "水";
                case ResourceType.Scrap: return "废料";
                case ResourceType.OrganicMatter: return "有机物";
                case ResourceType.Ammunition: return "弹药";
                case ResourceType.MedicalSupplies: return "医疗用品";
                case ResourceType.Tools: return "工具";
                case ResourceType.Fuel: return "燃料";
                case ResourceType.Electronics: return "电子元件";
                case ResourceType.Energy: return "能源";
                case ResourceType.ResearchPoints: return "科研点数";
                case ResourceType.RareMetals: return "稀有金属";
                default: return "未知资源";
            }
        }
        
        private string GetResourceCategoryName(ResourceCategory category)
        {
            switch (category)
            {
                case ResourceCategory.BasicMaterial: return "基础物资";
                case ResourceCategory.Manufactured: return "成品物资";
                case ResourceCategory.Advanced: return "高级资源";
                default: return "未知分类";
            }
        }
        
        #endregion
        
        #region 配方配置管理
        
        private void CreateQuickRecipeConfig(ResourceType outputType)
        {
            var newConfig = CreateRecipeConfigTemplate(outputType);
            
#if UNITY_EDITOR
            var newConfigAsset = ScriptableObject.CreateInstance<CraftingRecipeAsset>();
            newConfigAsset.Recipe = newConfig;
            
            string typeName = GetResourceTypeName(outputType);
            string path = EditorUtility.SaveFilePanelInProject(
                $"保存{typeName}配方配置", 
                $"New{typeName}Recipe", 
                "asset", 
                "选择保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newConfigAsset, path);
                AssetDatabase.SaveAssets();
                _craftingRecipes.Add(newConfigAsset);
                Debug.Log($"创建{typeName}配方配置: {path}");
            }
#endif
        }
        
        private CraftingRecipe CreateRecipeConfigTemplate(ResourceType outputType)
        {
            var config = new CraftingRecipe
            {
                OutputType = outputType,
                OutputAmount = 1,
                InputCosts = new List<ResourceCost>(),
                RequiredTechLevel = 0,
                RequiredBuildings = new List<string>()
            };
            
            // 根据产出类型设置默认值
            switch (outputType)
            {
                case ResourceType.Tools:
                    config.Id = $"tools_recipe_{System.DateTime.Now.Ticks}";
                    config.Name = "新工具配方";
                    config.Description = "制造工具的配方";
                    config.OutputAmount = 1;
                    config.CraftingTime = 2f;
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 10 });
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Scrap, Amount = 5 });
                    config.RequiredBuildings.Add("workshop_1");
                    break;
                    
                case ResourceType.Ammunition:
                    config.Id = $"ammunition_recipe_{System.DateTime.Now.Ticks}";
                    config.Name = "新弹药配方";
                    config.Description = "制造弹药的配方";
                    config.OutputAmount = 10;
                    config.CraftingTime = 1.5f;
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 5 });
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.RareMetals, Amount = 2 });
                    config.RequiredBuildings.Add("workshop_1");
                    config.RequiredTechLevel = 1;
                    break;
                    
                case ResourceType.MedicalSupplies:
                    config.Id = $"medical_recipe_{System.DateTime.Now.Ticks}";
                    config.Name = "新医疗用品配方";
                    config.Description = "制造医疗用品的配方";
                    config.OutputAmount = 5;
                    config.CraftingTime = 3f;
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.OrganicMatter, Amount = 8 });
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Water, Amount = 3 });
                    config.RequiredBuildings.Add("medical_station_1");
                    config.RequiredTechLevel = 1;
                    break;
                    
                case ResourceType.Fuel:
                    config.Id = $"fuel_recipe_{System.DateTime.Now.Ticks}";
                    config.Name = "新燃料配方";
                    config.Description = "制造燃料的配方";
                    config.OutputAmount = 20;
                    config.CraftingTime = 4f;
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.OrganicMatter, Amount = 15 });
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Scrap, Amount = 5 });
                    config.RequiredBuildings.Add("refinery_1");
                    config.RequiredTechLevel = 2;
                    break;
                    
                case ResourceType.Electronics:
                    config.Id = $"electronics_recipe_{System.DateTime.Now.Ticks}";
                    config.Name = "新电子元件配方";
                    config.Description = "制造电子元件的配方";
                    config.OutputAmount = 3;
                    config.CraftingTime = 5f;
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.RareMetals, Amount = 5 });
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Scrap, Amount = 10 });
                    config.RequiredBuildings.Add("electronics_factory_1");
                    config.RequiredTechLevel = 3;
                    break;
                    
                default:
                    config.Id = $"generic_recipe_{System.DateTime.Now.Ticks}";
                    config.Name = "新配方";
                    config.Description = "制造配方描述";
                    config.OutputAmount = 1;
                    config.CraftingTime = 1f;
                    config.InputCosts.Add(new ResourceCost { Type = ResourceType.Materials, Amount = 5 });
                    config.RequiredBuildings.Add("workshop_1");
                    break;
            }
            
            return config;
        }
        
        private void EditRecipeConfig(CraftingRecipeAsset configAsset)
        {
#if UNITY_EDITOR
            Selection.activeObject = configAsset;
            EditorGUIUtility.PingObject(configAsset);
#endif
        }
        
        private void RemoveRecipeConfig(int index)
        {
            if (index >= 0 && index < _craftingRecipes.Count)
            {
                var configAsset = _craftingRecipes[index];
                _craftingRecipes.RemoveAt(index);
                
                if (configAsset?.Recipe != null)
                {
                    Debug.Log($"移除配方配置: {configAsset.Recipe.Name}");
                }
            }
        }
        
        #endregion
        
        public IArchitecture GetArchitecture()
        {
            return RegisterManager.Interface;
        }
    }
    
    #region 配置资源类
    
    /// <summary>
    /// 建筑配置资源
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "生存游戏/建筑配置")]
    public class BuildingConfigAsset : ScriptableObject
    {
        public BuildingConfig Config;
    }
    
    /// <summary>
    /// 科技配置资源
    /// </summary>
    [CreateAssetMenu(fileName = "TechConfig", menuName = "生存游戏/科技配置")]
    public class TechConfigAsset : ScriptableObject
    {
        public TechConfig Config;
    }
    
    /// <summary>
    /// 资源配置资源
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceConfig", menuName = "生存游戏/资源配置")]
    public class ResourceConfigAsset : ScriptableObject
    {
        public ResourceConfig Config;
    }
    
    /// <summary>
    /// 制造配方资源
    /// </summary>
    [CreateAssetMenu(fileName = "CraftingRecipe", menuName = "生存游戏/制造配方")]
    public class CraftingRecipeAsset : ScriptableObject
    {
        public CraftingRecipe Recipe;
    }
    
    #endregion
    
    #region 导出数据结构
    
    [System.Serializable]
    public class ConfigExportData
    {
        public List<BuildingConfig> Buildings;
        public List<TechConfig> Techs;
        public List<ResourceConfig> Resources;
        public List<CraftingRecipe> Recipes;
    }
    
    #endregion
} 