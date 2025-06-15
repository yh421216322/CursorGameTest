// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：GridUIController.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了网格UI控制器 (GridUIController)。
//     该控制器负责在建筑模式下管理和显示建造辅助网格，包括网格单元的创建、
//     颜色更新（以指示可建造性）、建筑预览的显示和更新等。
//     它响应游戏事件（如进入/退出建造模式、建筑放置/拆除）来动态调整网格状态。
// ==============================================================================

using UnityEngine;
using UnityEngine.UI; // 尽管主要是SpriteRenderer，但可能间接涉及UI元素或未来扩展
using System.Collections.Generic;
using QFramework;
using MyGameNamespace;      // 包含自定义事件定义
using SurvivalGame.Model;   // 包含BuildingCategory等模型定义
using SurvivalGame.GameSystem; // 包含IEnhancedBuildingSystem, ConfigSystem等系统接口
using SurvivalGame.Utility; // 包含IGridUtility工具接口

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 网格显示和建造预览控制器。
    /// 在建筑模式下，此控制器负责在游戏世界中动态创建和管理一个可见的网格，
    /// 网格单元会根据是否可建造显示不同颜色（如绿色/红色）。
    /// 同时，它还管理一个建筑预览图像，该图像会跟随鼠标并吸附到网格上。
    /// 挂载对象通常是场景中的一个UI或游戏逻辑相关的GameObject (例如 Canvas/GridUI)。
    /// 依赖于建筑系统 (IBuildingSystem/IEnhancedBuildingSystem) 和网格工具 (IGridUtility)。
    /// </summary>
    public class GridUIController : MonoBehaviour, IController, ICanGetUtility // 实现QFramework接口
    {
        [Header("网格显示核心设置")] // Inspector中分组显示
        [SerializeField] private GameObject gridContainer;      // 用于容纳所有网格单元的父GameObject
        [SerializeField] private GameObject gridCellPrefab;     // 网格单元的预制件 (可选，如果为null则程序化创建)
        [SerializeField] private Material gridMaterial;         // 网格单元的材质 (如果程序化创建Sprite，可能未使用)
        [SerializeField] private int gridRange = 25;            // 从相机中心点算起的网格显示范围（单位：格子数）

        [Header("网格单元颜色配置")]
        [SerializeField] private Color availableGridColor = new Color(0,1,0,0.3f); // 可建造区域的网格颜色 (默认半透明绿)
        [SerializeField] private Color occupiedGridColor  = new Color(1,0,0,0.3f); // 已占用或不可建造区域的颜色 (默认半透明红)
        [SerializeField] private Color previewGridColor   = new Color(1,1,0,0.4f); // 鼠标悬停的建造预览位置颜色 (默认半透明黄)
        [SerializeField] private float gridAlpha = 0.3f;               // 网格单元的整体透明度

        [Header("建筑预览对象设置")]
        [SerializeField] private GameObject buildingPreview;    // 用于显示建筑预览图像的GameObject
        [SerializeField] private Material previewMaterial;      // 建筑预览图像的材质 (如果使用SpriteRenderer，材质通常由Sprite决定)

        // QFramework及游戏核心系统/工具引用
        private IEnhancedBuildingSystem mBuildingSystem; // 增强型建筑系统接口
        private IGridUtility mGridUtility;              // 网格工具接口
        private ISurvivalGameModel mGameModel;          // 游戏数据模型接口
        private ConfigSystem mConfigSystem;             // 配置数据系统接口

        // 网格内部管理变量
        private Dictionary<Vector2, GameObject> mGridCells; // 存储当前所有网格单元的字典，键为网格坐标
        private bool mIsGridVisible = false;            // 网格当前是否可见
        private string mCurrentBuildingType = "";       // 当前正在预览或尝试建造的建筑类型ID
        private Vector3 mLastMousePosition;             // 上一帧记录的鼠标世界位置，用于检测鼠标是否移动

        // 常量定义
        private const float GRID_SIZE = 2f; // 每个网格单元在世界空间中的大小 (假设为2x2单位)
        private const float GRID_UPDATE_INTERVAL = 0.1f; // 网格颜色更新的最小时间间隔（秒），用于性能优化
        private float mLastGridUpdate = 0f;             // 上次网格更新的时间戳
        
        /// <summary>
        /// Unity生命周期方法：当脚本实例被创建时调用。
        /// 用于获取QFramework框架组件和初始化网格系统。
        /// </summary>
        private void Awake()
        {
            // 获取QFramework框架组件实例
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            mGridUtility = this.GetUtility<IGridUtility>(); // 获取网格工具
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mConfigSystem = this.GetSystem<ConfigSystem>();
            
            InitializeGridSystem(); // 初始化网格系统相关设置
        }
        
        /// <summary>
        /// Unity生命周期方法：在Awake之后、首次Update之前调用一次。
        /// 用于注册监听游戏事件。
        /// </summary>
        private void Start()
        {
            // 注册对游戏事件的监听，以便在事件发生时更新网格UI
            // 注意：原代码在InitializeGridSystem中也注册了事件，这里是第二次注册。
            // QFramework的事件系统通常能处理重复注册（只注册一次），但最佳实践是只注册一次。
            // 为保持与原逻辑接近，此处保留，但建议审查并统一注册位置。
            this.RegisterEvent<BuildModeChangedEvent>(OnBuildModeChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject); // 进入/退出建造模式时
            this.RegisterEvent<BuildingStartedEvent>(OnBuildingPlaced)
                .UnRegisterWhenGameObjectDestroyed(gameObject); // 建筑成功放置后
            this.RegisterEvent<BuildingDemolishedEvent>(OnBuildingRemoved)
                .UnRegisterWhenGameObjectDestroyed(gameObject); // 建筑被拆除后
            this.RegisterEvent<GridAlphaChangedEvent>(OnGridAlphaChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject); // 网格透明度被外部更改时
            this.RegisterEvent<GridRangeChangedEvent>(OnGridRangeChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject); // 网格显示范围被外部更改时
        }
        
        /// <summary>
        /// 初始化网格系统，包括创建数据结构、网格容器和建筑预览对象。
        /// </summary>
        private void InitializeGridSystem()
        {
            mGridCells = new Dictionary<Vector2, GameObject>(); // 初始化存储网格单元的字典
            mLastMousePosition = Vector3.zero; // 初始化鼠标位置记录
            
            // 如果网格容器未在Inspector中指定，则动态创建一个
            if (gridContainer == null)
            {
                gridContainer = new GameObject("GridCellsContainer"); // 命名以便于场景中识别
                gridContainer.transform.SetParent(transform, false); // 设置为当前GameObject的子对象
            }
            
            // 创建建筑预览用的GameObject（如果尚未创建）
            CreateBuildingPreview();
            
            // 初始化时，网格和建筑预览都是隐藏的
            gridContainer.SetActive(false);
            if (buildingPreview != null) buildingPreview.SetActive(false);
            
            // UnityEngine.Debug.Log("网格UI系统初始化完成"); // 初始化完成日志（当前注释）
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 如果网格可见（处于建造模式），则更新网格显示和建筑预览。
        /// </summary>
        private void Update()
        {
            if (mIsGridVisible) // 仅当网格应显示时才执行更新
            {
                UpdateGridDisplay();     // 更新网格单元的颜色和状态
                UpdateBuildingPreview(); // 更新建筑预览的位置和外观
            }
        }
        
        /// <summary>
        /// 显示建造网格和建筑预览。
        /// </summary>
        /// <param name="buildingType">当前要建造的建筑类型ID。</param>
        public void ShowBuildGrid(string buildingType)
        {
            mCurrentBuildingType = buildingType; // 记录当前建筑类型
            mIsGridVisible = true;               // 设置网格为可见状态

            if (gridContainer != null) gridContainer.SetActive(true); // 激活网格容器
            if (buildingPreview != null) buildingPreview.SetActive(true); // 激活建筑预览对象
            
            CreateGridCells();  // 根据当前相机位置和范围重新创建或更新网格单元
            UpdateGridColors(); // 立即更新一次网格颜色
            
            Debug.Log($"[GridUI] 显示建造网格，建筑类型: {buildingType}");
        }
        
        /// <summary>
        /// 隐藏建造网格和建筑预览。
        /// </summary>
        public void HideBuildGrid()
        {
            mIsGridVisible = false;             // 设置网格为不可见状态
            mCurrentBuildingType = "";          // 清空当前建筑类型
            
            if (gridContainer != null) gridContainer.SetActive(false); // 隐藏网格容器
            if (buildingPreview != null) buildingPreview.SetActive(false); // 隐藏建筑预览对象

            Debug.Log("[GridUI] 已隐藏建造网格。");
        }
        
        /// <summary>
        /// 根据当前相机位置和设定的网格范围，动态创建或更新网格单元。
        /// </summary>
        private void CreateGridCells()
        {
            ClearGridCells(); // 首先清除所有已存在的旧网格单元
            
            Camera cam = Camera.main; // 获取主相机
            if (cam == null)
            {
                Debug.LogError("[GridUI] 无法找到主相机 (Camera.main)！网格无法创建。");
                return;
            }
            
            Vector3 cameraPosition = cam.transform.position; // 获取相机当前世界位置
            
            // 根据相机位置、网格范围(gridRange)和网格大小(GRID_SIZE)计算需要创建网格单元的X和Y坐标范围
            int minX = Mathf.FloorToInt((cameraPosition.x - gridRange * GRID_SIZE) / GRID_SIZE);
            int maxX = Mathf.CeilToInt((cameraPosition.x + gridRange * GRID_SIZE) / GRID_SIZE);
            int minY = Mathf.FloorToInt((cameraPosition.y - gridRange * GRID_SIZE) / GRID_SIZE);
            int maxY = Mathf.CeilToInt((cameraPosition.y + gridRange * GRID_SIZE) / GRID_SIZE);
            
            // 在计算出的范围内遍历，为每个网格坐标创建对应的单元GameObject
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2 gridPos = new Vector2(x, y); // 当前网格单元的逻辑坐标
                    Vector3 worldPos = GridToWorld(gridPos); // 将逻辑坐标转换为世界坐标
                    
                    GameObject gridCell = CreateGridCell(worldPos, gridPos); // 创建单个网格单元
                    mGridCells[gridPos] = gridCell; // 将创建的单元存入字典中管理
                }
            }
        }
        
        /// <summary>
        /// 创建单个网格单元的GameObject。
        /// 如果有预制件(gridCellPrefab)则实例化它，否则程序化创建一个带SpriteRenderer的简单方形。
        /// </summary>
        /// <param name="worldPosition">网格单元在世界空间中的位置。</param>
        /// <param name="gridPosition">网格单元的逻辑网格坐标。</param>
        /// <returns>创建的网格单元GameObject。</returns>
        private GameObject CreateGridCell(Vector3 worldPosition, Vector2 gridPosition)
        {
            GameObject cell;
            
            if (gridCellPrefab != null) // 如果有预制件，则使用预制件
            {
                cell = Instantiate(gridCellPrefab, gridContainer.transform);
            }
            else // 否则，程序化创建一个简单的网格单元
            {
                cell = new GameObject($"GridCell_{gridPosition.x}_{gridPosition.y}"); // 命名
                cell.transform.SetParent(gridContainer.transform, false); // 设置父对象
                
                var spriteRenderer = cell.AddComponent<SpriteRenderer>(); // 添加SpriteRenderer
                spriteRenderer.sprite = CreateGridSprite(); // 创建并设置一个简单的方形Sprite
                spriteRenderer.material = gridMaterial; // 应用指定的网格材质（如果存在）
                spriteRenderer.sortingOrder = -1; // 设置渲染层级，使其在背景之后，其他物体之前
                spriteRenderer.sortingLayerName = "Grid"; // 使用"Grid"渲染层 (需在Unity中定义)
            }
            
            cell.transform.position = worldPosition; // 设置网格单元的世界位置
            // cell.transform.localScale = Vector3.one * GRID_SIZE; // 如果Sprite本身不是单位大小，可能需要调整Scale
            return cell;
        }
        
        /// <summary>
        /// 程序化创建一个简单的方形Sprite，用作网格单元的视觉表现。
        /// Sprite的纹理是一个中心透明、边缘为白色的方框。
        /// </summary>
        /// <returns>创建的Sprite对象。</returns>
        private Sprite CreateGridSprite()
        {
            int textureSize = 64; // 纹理的像素尺寸
            Texture2D texture = new Texture2D(textureSize, textureSize);
            
            // 绘制一个白色边框，内部透明的纹理
            Color transparent = Color.clear; // 完全透明
            Color borderColor = Color.white * 0.5f; // 半透明白色边框 (可调整)
            int borderThickness = 2; // 边框厚度（像素）

            for (int x = 0; x < textureSize; x++)
            {
                for (int y = 0; y < textureSize; y++)
                {
                    // 判断当前像素是否在边框上
                    if (x < borderThickness || x >= textureSize - borderThickness ||
                        y < borderThickness || y >= textureSize - borderThickness)
                    {
                        texture.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, transparent);
                    }
                }
            }
            
            texture.Apply(); // 应用像素更改
            texture.filterMode = FilterMode.Point; // 使用点滤波保持像素感
            
            // 创建Sprite，pixelsPerUnit基于GRID_SIZE，使得Sprite在世界中恰好是GRID_SIZE大小
            return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize / GRID_SIZE);
        }
        
        /// <summary>
        /// 更新网格的显示状态，主要是颜色。此方法会节流，不会每帧都执行完整的颜色更新。
        /// </summary>
        private void UpdateGridDisplay()
        {
            // 限制此更新操作的频率，以优化性能
            if (Time.time - mLastGridUpdate < GRID_UPDATE_INTERVAL) return;
            mLastGridUpdate = Time.time; // 更新上次执行时间
            
            // 获取当前鼠标在世界中的位置
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // 确保在2D平面
            
            // 如果鼠标位置相比上一帧有显著变化，则更新网格颜色
            if (Vector3.Distance(mousePos, mLastMousePosition) > 0.1f) // 0.1f是阈值，可调整
            {
                mLastMousePosition = mousePos; // 更新记录的鼠标位置
                UpdateGridColors(); // 调用实际的颜色更新逻辑
            }
        }
        
        /// <summary>
        /// 更新所有可见网格单元的颜色，以反映当前建造预览和可建造状态。
        /// </summary>
        private void UpdateGridColors()
        {
            // 如果当前没有选定的建筑类型，则不进行颜色更新
            if (string.IsNullOrEmpty(mCurrentBuildingType)) return;
            
            // 获取当前选定建筑的配置信息
            var buildingConfig = mConfigSystem?.GetBuildingConfig(mCurrentBuildingType);
            if (buildingConfig == null)
            {
                Debug.LogWarning($"[GridUI] 未能找到建筑类型 '{mCurrentBuildingType}' 的配置信息。");
                return;
            }
            
            // 获取鼠标当前指向的网格单元的逻辑坐标
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3 snappedMouseWorldPos = mGridUtility?.SnapToGrid(mouseWorldPos) ?? SnapToGrid(mouseWorldPos); // 使用工具类或备用方法吸附到网格
            Vector2 mouseGridLogicPos = WorldToGrid(snappedMouseWorldPos); // 转换为逻辑网格坐标
            
            // 遍历当前所有显示的网格单元
            foreach (var kvp in mGridCells)
            {
                Vector2 cellLogicPos = kvp.Key;    // 当前单元的逻辑坐标
                GameObject gridCellGO = kvp.Value; // 当前单元的GameObject
                
                if (gridCellGO == null) continue; // 跳过已销毁的单元
                
                var spriteRenderer = gridCellGO.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null) continue; // 跳过没有SpriteRenderer的单元
                
                // 判断当前单元是否是鼠标悬停的预览位置
                // 由于浮点数精度问题，使用小范围比较代替直接相等判断
                bool isPreviewCell = Vector2.Distance(cellLogicPos, mouseGridLogicPos) < 0.1f;
                
                // 检查当前单元在世界中的位置是否可用于建造选定的建筑类型
                Vector3 cellWorldPos = GridToWorld(cellLogicPos);
                bool canBuildHere = CanBuildAtPosition(cellWorldPos, mCurrentBuildingType);
                
                // 根据情况设置目标颜色
                Color targetColor;
                if (isPreviewCell) // 如果是鼠标悬停的预览单元
                {
                    // 根据是否可建造，使用预览色或占用色
                    targetColor = canBuildHere ? previewGridColor : occupiedGridColor;
                }
                else // 如果不是预览单元
                {
                    // 根据是否可建造，使用可用色或占用色
                    targetColor = canBuildHere ? availableGridColor : occupiedGridColor;
                }
                
                targetColor.a = gridAlpha; // 应用设定的全局透明度
                spriteRenderer.color = targetColor; // 设置最终颜色
            }
        }
        
        /// <summary>
        /// 更新建筑预览对象的位置和外观（主要是颜色和Sprite）。
        /// </summary>
        private void UpdateBuildingPreview()
        {
            if (buildingPreview == null || string.IsNullOrEmpty(mCurrentBuildingType)) return;
            
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0; // 确保在2D平面
            
            // 将预览对象的位置吸附到网格
            Vector3 snapPos = mGridUtility?.SnapToGrid(mouseWorldPos) ?? SnapToGrid(mouseWorldPos);
            buildingPreview.transform.position = snapPos;
            
            // 更新预览对象的SpriteRenderer颜色以指示可建造性
            var spriteRenderer = buildingPreview.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                bool canBuild = CanBuildAtPosition(snapPos, mCurrentBuildingType);
                spriteRenderer.color = canBuild ? new Color(0,1,0,0.5f) : new Color(1,0,0,0.5f); // 半透明绿/红
                
                // 如果预览Sprite尚未设置或与当前建筑类型不符，则重新创建/设置
                // (简单示例，实际可能需要更复杂的Sprite管理)
                if (spriteRenderer.sprite == null || spriteRenderer.name != $"Preview_{mCurrentBuildingType}")
                {
                    spriteRenderer.sprite = CreateBuildingPreviewSprite(mCurrentBuildingType);
                    spriteRenderer.name = $"Preview_{mCurrentBuildingType}";
                }
            }
        }
        
        /// <summary>
        /// 程序化创建一个用于建筑预览的Sprite。
        /// </summary>
        /// <param name="buildingType">建筑类型ID，用于决定预览Sprite的颜色或样式。</param>
        /// <returns>创建的预览Sprite。</returns>
        private Sprite CreateBuildingPreviewSprite(string buildingType)
        {
            int textureSize = 64; // 预览Sprite的纹理大小
            Texture2D texture = new Texture2D(textureSize, textureSize);
            
            Color buildingColor = GetBuildingTypeColor(buildingType); // 获取该建筑类型对应的基础颜色
            buildingColor.a = 0.7f; // 设置预览Sprite的透明度
            
            // 创建一个简单的建筑轮廓或填充色块作为预览
            // 此处示例为带黑色边框的彩色方块
            Color borderColor = Color.black * 0.8f;
            int border = 4;

            for (int x = 0; x < textureSize; x++)
            {
                for (int y = 0; y < textureSize; y++)
                {
                    if (x < border || x >= textureSize - border || y < border || y >= textureSize - border)
                        texture.SetPixel(x, y, borderColor); // 边框
                    else
                        texture.SetPixel(x, y, buildingColor); // 内部填充色
                }
            }
            
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            // pixelsPerUnit 应与网格单元的Sprite设置保持一致或协调
            return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize / GRID_SIZE);
        }
        
        /// <summary>
        /// 根据建筑类型（实际是其类别）获取一个基础颜色，用于预览Sprite。
        /// </summary>
        private Color GetBuildingTypeColor(string buildingType)
        {
            var config = mConfigSystem?.GetBuildingConfig(buildingType); // 从配置系统获取建筑信息
            if (config == null) return Color.gray; // 未找到配置则返回灰色
            
            // 根据建筑的类别返回不同颜色
            switch (config.Category)
            {
                case BuildingCategory.Defense:    return Color.red;
                case BuildingCategory.Production: return Color.yellow;
                case BuildingCategory.Habitat:    return Color.blue;
                case BuildingCategory.Storage:    return Color.green;
                case BuildingCategory.Functional: return Color.cyan;
                default: return Color.magenta; // 其他类型用洋红色
            }
        }
        
        /// <summary>
        /// 检查指定世界位置是否可以建造特定类型的建筑。
        /// 优先使用建筑系统的检查方法，如果系统不可用，则使用备用的简单占用检查。
        /// </summary>
        private bool CanBuildAtPosition(Vector3 position, string buildingType)
        {
            if (mBuildingSystem != null)
            {
                // 使用建筑系统提供的方法来判断是否可建造（通常会考虑地形、资源、碰撞等多种因素）
                return mBuildingSystem.CanBuildAt(position, buildingType);
            }
            
            // 备用逻辑：简单检查该位置是否已被其他建筑占据
            // Debug.LogWarning("[GridUI] mBuildingSystem 为空，使用简化的位置占用检查。");
            return !IsPositionOccupied(position);
        }
        
        /// <summary>
        /// （备用逻辑）简单检查指定世界位置是否已被现有建筑占据。
        /// 将位置吸附到网格，然后遍历游戏模型中的所有建筑进行距离比较。
        /// </summary>
        private bool IsPositionOccupied(Vector3 position)
        {
            Vector3 gridSnappedPos = SnapToGrid(position); // 将输入位置吸附到最近的网格点
            
            if (mGameModel?.Buildings != null) // 确保游戏模型和建筑列表存在
            {
                foreach (var building in mGameModel.Buildings.Values) // 遍历所有已建成的建筑
                {
                    // 比较吸附后的位置与现有建筑的位置，如果距离小于网格尺寸的一定比例，则认为已占用
                    // 0.8f * GRID_SIZE 是一个经验值，确保不会因为浮点精度问题导致误判
                    if (Vector3.Distance(building.Position, gridSnappedPos) < GRID_SIZE * 0.8f)
                    {
                        return true; // 已被占据
                    }
                }
            }
            return false; // 未被占据
        }
        
        /// <summary>
        /// 将给定的世界坐标位置吸附到最近的网格点。
        /// </summary>
        private Vector3 SnapToGrid(Vector3 position)
        {
            // 这是GridUtility应该提供的核心功能。如果IGridUtility未实现或获取失败，则使用此本地备用实现。
            if(mGridUtility != null) return mGridUtility.SnapToGrid(position);

            // 备用实现：
            float snappedX = Mathf.Round(position.x / GRID_SIZE) * GRID_SIZE;
            float snappedY = Mathf.Round(position.y / GRID_SIZE) * GRID_SIZE;
            return new Vector3(snappedX, snappedY, 0); // Z轴通常为0（2D平面）
        }
        
        /// <summary>
        /// 将世界坐标转换为逻辑网格坐标 (整数对)。
        /// </summary>
        private Vector2 WorldToGrid(Vector3 worldPosition)
        {
            if(mGridUtility != null) return mGridUtility.WorldToGrid(worldPosition);

            // 备用实现：
            int gridX = Mathf.RoundToInt(worldPosition.x / GRID_SIZE);
            int gridY = Mathf.RoundToInt(worldPosition.y / GRID_SIZE);
            return new Vector2(gridX, gridY);
        }
        
        /// <summary>
        /// 将逻辑网格坐标转换回世界坐标 (通常是网格单元的中心点)。
        /// </summary>
        private Vector3 GridToWorld(Vector2 gridPosition)
        {
            if(mGridUtility != null) return mGridUtility.GridToWorld(gridPosition);

            // 备用实现：
            float worldX = gridPosition.x * GRID_SIZE;
            float worldY = gridPosition.y * GRID_SIZE;
            return new Vector3(worldX, worldY, 0);
        }
        
        /// <summary>
        /// 清除当前显示的所有网格单元GameObject。
        /// </summary>
        private void ClearGridCells()
        {
            foreach (var cell in mGridCells.Values) // 遍历字典中存储的网格单元
            {
                if (cell != null) // 确保对象未被销毁
                {
                    // 使用DestroyImmediate更适合编辑器或非运行时清理，但在运行时Update循环中应优先用Destroy
                    // 此处上下文是运行时，但可能是为了快速响应或编辑器内测试，原作者用了DestroyImmediate
                    // 为安全起见，运行时建议用Destroy()
                    #if UNITY_EDITOR
                    if(!Application.isPlaying) DestroyImmediate(cell);
                    else Destroy(cell);
                    #else
                    Destroy(cell);
                    #endif
                }
            }
            mGridCells.Clear(); // 清空字典
        }
        
        // === 事件处理器 ===

        /// <summary>
        /// 处理建造模式状态改变事件。
        /// </summary>
        private void OnBuildModeChanged(BuildModeChangedEvent e)
        {
            if (e.InBuildMode) // 如果事件指示进入建造模式
            {
                ShowBuildGrid(e.BuildingType); // 显示对应建筑类型的网格
            }
            else // 如果事件指示退出建造模式
            {
                HideBuildGrid(); // 隐藏网格
            }
        }
        
        /// <summary>
        /// 处理建筑已放置（开始建造）事件。
        /// 主要用于在建筑放置后刷新网格颜色，以反映新的占用情况。
        /// </summary>
        private void OnBuildingPlaced(BuildingStartedEvent e)
        {
            if (mIsGridVisible) // 仅当网格当前可见时才更新
            {
                UpdateGridColors();
            }
        }
        
        /// <summary>
        /// 处理建筑已被拆除事件。
        /// 刷新网格颜色，以反映空出的区域。
        /// </summary>
        private void OnBuildingRemoved(BuildingDemolishedEvent e)
        {
            if (mIsGridVisible)
            {
                UpdateGridColors();
            }
        }
        
        /// <summary>
        /// 处理网格透明度改变事件。
        /// </summary>
        private void OnGridAlphaChanged(GridAlphaChangedEvent e)
        {
            gridAlpha = Mathf.Clamp01(e.Alpha); // 更新透明度并限制在0-1范围
            if (mIsGridVisible)
            {
                UpdateGridColors(); // 更新网格颜色以应用新的透明度
            }
        }
        
        /// <summary>
        /// 处理网格显示范围改变事件。
        /// </summary>
        private void OnGridRangeChanged(GridRangeChangedEvent e)
        {
            gridRange = Mathf.Max(5, e.Range); // 更新范围，并确保不小于一个最小值（例如5）
            if (mIsGridVisible)
            {
                CreateGridCells(); // 重新创建网格以适应新的范围
            }
        }
        
        // === 公共属性和方法 ===

        /// <summary>
        /// 获取当前网格是否可见。
        /// </summary>
        public bool IsGridVisible => mIsGridVisible;
        
        /// <summary>
        /// 设置网格的显示范围。
        /// </summary>
        /// <param name="range">新的显示范围（格子数）。会被限制在10到50之间。</param>
        public void SetGridRange(int range)
        {
            gridRange = Mathf.Clamp(range, 10, 50); // 限制范围值
            if (mIsGridVisible) // 如果网格当前可见，则立即重新创建网格
            {
                CreateGridCells();
            }
        }
        
        /// <summary>
        /// 设置网格的透明度。
        /// </summary>
        /// <param name="alpha">新的透明度值 (0到1之间)。</param>
        public void SetGridAlpha(float alpha)
        {
            gridAlpha = Mathf.Clamp01(alpha); // 限制透明度值在0-1
            if (mIsGridVisible) // 如果网格当前可见，则立即更新网格颜色
            {
                UpdateGridColors();
            }
        }
        
        /// <summary>
        /// 实现QFramework的IController接口，返回全局唯一的架构实例。
        /// </summary>
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
        
        /// <summary>
        /// Unity生命周期方法：当对象被销毁时调用。
        /// 用于清理资源和取消事件订阅，防止内存泄漏。
        /// </summary>
        private void OnDestroy()
        {
            ClearGridCells(); // 清理所有动态创建的网格单元
            
            // 取消对此脚本中注册的所有事件的监听
            this.UnRegisterEvent<BuildModeChangedEvent>(OnBuildModeChanged);
            this.UnRegisterEvent<BuildingStartedEvent>(OnBuildingPlaced);
            this.UnRegisterEvent<BuildingDemolishedEvent>(OnBuildingRemoved);
            this.UnRegisterEvent<GridAlphaChangedEvent>(OnGridAlphaChanged);
            this.UnRegisterEvent<GridRangeChangedEvent>(OnGridRangeChanged);
        }
        
        /// <summary>
        /// 创建并初始化建筑预览用的GameObject（如果尚未创建）。
        /// </summary>
        private void CreateBuildingPreview()
        {
            if (buildingPreview == null) // 如果Inspector中未指定或已被销毁
            {
                buildingPreview = new GameObject("BuildingPlacementPreview"); // 创建新GameObject
                buildingPreview.transform.SetParent(transform, false); // 设置为当前控制器的子对象
                
                var spriteRenderer = buildingPreview.AddComponent<SpriteRenderer>(); // 添加SpriteRenderer
                spriteRenderer.sortingOrder = 10; // 设置较高的渲染层序，使其显示在网格之上
                spriteRenderer.sortingLayerName = "UI_Overlay"; // 使用一个合适的渲染层 (需在Unity中定义)
                // spriteRenderer.material = previewMaterial; // 可以应用特定预览材质
                buildingPreview.SetActive(false); // 初始时隐藏
            }
        }
    }
}

// === 网格UI相关的事件定义 ===
// 注意：部分事件如BuildModeChangedEvent等可能已在MyGameNamespace的其他地方定义（例如GameUIController.cs）。
// 在一个项目中，事件定义通常应放在一个集中的位置以避免重复和冲突。
// 此处假设这些是此控制器特有或在此处首次定义的事件。
namespace MyGameNamespace
{
    /// <summary>
    /// 建筑开始建造事件。
    /// 当一个建筑的建造过程开始时（例如，玩家确认放置后）触发。
    /// </summary>
    public struct BuildingStartedEvent // 此事件可能与BuildCommandSuccessEvent有重叠，需根据实际设计区分或合并
    {
        public string BuildingType; // 开始建造的建筑类型ID
        public Vector3 Position;    // 建筑放置的世界坐标
        public string BuildingId;   // 新建建筑实例的唯一ID
    }
    
    /// <summary>
    /// 建筑被拆除事件。
    /// 当一个建筑被成功拆除后触发。
    /// </summary>
    public struct BuildingDemolishedEvent
    {
        public string BuildingType; // 被拆除的建筑类型ID
        public Vector3 Position;    // 建筑被拆除前的位置
        public string BuildingId;   // 被拆除建筑的唯一ID
    }
    
    // 以下事件定义在GameUIController.cs中也存在，此处注释掉以避免重复定义。
    // 如果这些事件是共享的，应确保只在一个地方定义。
    // /// <summary>
    // /// 建造模式变化事件
    // /// </summary>
    // public struct BuildModeChangedEvent
    // {
    //     public bool InBuildMode;
    //     public string BuildingType;
    // }
    //
    // /// <summary>
    // /// 网格透明度变化事件
    // /// </summary>
    // public struct GridAlphaChangedEvent
    // {
    //     public float Alpha;
    // }
    //
    // /// <summary>
    // /// 网格显示范围变化事件
    // /// </summary>
    // public struct GridRangeChangedEvent
    // {
    //     public int Range;
    // }
}