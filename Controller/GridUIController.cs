using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using QFramework;
using MyGameNamespace;
using SurvivalGame.Model;
using SurvivalGame.GameSystem;
using SurvivalGame.Utility;

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 功能：网格显示和建造预览控制器，在建造模式下显示绿色/红色网格
    /// 挂载对象：Canvas/GridUI
    /// 依赖系统：IBuildingSystem、IGridUtility
    /// </summary>
    public class GridUIController : MonoBehaviour, IController,ICanGetUtility
    {
        [Header("网格显示设置")]
        [SerializeField] private GameObject gridContainer;           // 网格容器
        [SerializeField] private GameObject gridCellPrefab;         // 网格单元预制体
        [SerializeField] private Material gridMaterial;             // 网格材质
        [SerializeField] private int gridRange = 25;                // 网格显示范围
        
        [Header("网格颜色设置")]
        [SerializeField] private Color availableGridColor = Color.green;   // 可建造区域颜色
        [SerializeField] private Color occupiedGridColor = Color.red;      // 已占用区域颜色
        [SerializeField] private Color previewGridColor = Color.yellow;    // 建造预览颜色
        [SerializeField] private float gridAlpha = 0.3f;                   // 网格透明度
        
        [Header("建造预览设置")]
        [SerializeField] private GameObject buildingPreview;        // 建筑预览对象
        [SerializeField] private Material previewMaterial;          // 预览材质
        
        // 框架引用
        private IEnhancedBuildingSystem mBuildingSystem;
        private IGridUtility mGridUtility;
        private ISurvivalGameModel mGameModel;
        private ConfigSystem mConfigSystem;
        
        // 网格管理
        private Dictionary<Vector2, GameObject> mGridCells;
        private bool mIsGridVisible = false;
        private string mCurrentBuildingType = "";
        private Vector3 mLastMousePosition;
        
        // 常量
        private const float GRID_SIZE = 2f;
        private const float GRID_UPDATE_INTERVAL = 0.1f;
        private float mLastGridUpdate = 0f;
        
        private void Awake()
        {
            // 获取框架组件
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            mGridUtility = this.GetUtility<IGridUtility>();
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mConfigSystem = this.GetSystem<ConfigSystem>();
            
            InitializeGridSystem();
        }
        
        private void Start()
        {
            // 注册建造模式事件
            this.RegisterEvent<BuildModeChangedEvent>(OnBuildModeChanged);
            this.RegisterEvent<BuildingStartedEvent>(OnBuildingPlaced);
            this.RegisterEvent<BuildingDemolishedEvent>(OnBuildingRemoved);
            this.RegisterEvent<GridAlphaChangedEvent>(OnGridAlphaChanged);
            this.RegisterEvent<GridRangeChangedEvent>(OnGridRangeChanged);
        }
        
        private void InitializeGridSystem()
        {
            mGridCells = new Dictionary<Vector2, GameObject>();
            mLastMousePosition = Vector3.zero;
            
            // 创建网格容器
            if (gridContainer == null)
            {
                gridContainer = new GameObject("GridContainer");
                gridContainer.transform.SetParent(transform);
            }
            
            // 注册事件监听器
            this.RegisterEvent<BuildModeChangedEvent>(OnBuildModeChanged);
            this.RegisterEvent<BuildingStartedEvent>(OnBuildingPlaced);
            this.RegisterEvent<BuildingDemolishedEvent>(OnBuildingRemoved);
            this.RegisterEvent<GridAlphaChangedEvent>(OnGridAlphaChanged);
            this.RegisterEvent<GridRangeChangedEvent>(OnGridRangeChanged);
            
            // 创建建造预览对象
            CreateBuildingPreview();
            
            // 初始化时隐藏网格
            gridContainer.SetActive(false);
            
           // UnityEngine.Debug.Log("网格UI系统初始化完成");
        }
        
        private void Update()
        {
            if (mIsGridVisible)
            {
                UpdateGridDisplay();
                UpdateBuildingPreview();
            }
        }
        
        /// <summary>
        /// 显示建造网格
        /// </summary>
        public void ShowBuildGrid(string buildingType)
        {
            mCurrentBuildingType = buildingType;
            mIsGridVisible = true;
            gridContainer.SetActive(true);
            buildingPreview.SetActive(true);
            
            CreateGridCells();
            UpdateGridColors();
            
            Debug.Log($"显示建造网格：{buildingType}");
        }
        
        /// <summary>
        /// 隐藏建造网格
        /// </summary>
        public void HideBuildGrid()
        {
            mIsGridVisible = false;
            mCurrentBuildingType = "";
            gridContainer.SetActive(false);
            buildingPreview.SetActive(false);
            
            Debug.Log("隐藏建造网格");
        }
        
        private void CreateGridCells()
        {
            // 清除现有网格
            ClearGridCells();
            
            // 获取摄像机视野范围
            Camera cam = Camera.main;
            if (cam == null) return;
            
            Vector3 cameraPosition = cam.transform.position;
            
            // 计算网格范围
            int minX = Mathf.FloorToInt((cameraPosition.x - gridRange) / GRID_SIZE);
            int maxX = Mathf.CeilToInt((cameraPosition.x + gridRange) / GRID_SIZE);
            int minY = Mathf.FloorToInt((cameraPosition.y - gridRange) / GRID_SIZE);
            int maxY = Mathf.CeilToInt((cameraPosition.y + gridRange) / GRID_SIZE);
            
            // 创建网格单元
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2 gridPos = new Vector2(x, y);
                    Vector3 worldPos = new Vector3(x * GRID_SIZE, y * GRID_SIZE, 0);
                    
                    GameObject gridCell = CreateGridCell(worldPos, gridPos);
                    mGridCells[gridPos] = gridCell;
                }
            }
        }
        
        private GameObject CreateGridCell(Vector3 worldPosition, Vector2 gridPosition)
        {
            GameObject cell;
            
            if (gridCellPrefab != null)
            {
                cell = Instantiate(gridCellPrefab, gridContainer.transform);
            }
            else
            {
                // 创建简单的网格单元
                cell = new GameObject($"GridCell_{gridPosition.x}_{gridPosition.y}");
                cell.transform.SetParent(gridContainer.transform);
                
                // 添加SpriteRenderer
                var spriteRenderer = cell.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateGridSprite();
                spriteRenderer.sortingOrder = -1;
                spriteRenderer.sortingLayerName = "Background";
            }
            
            cell.transform.position = worldPosition;
            return cell;
        }
        
        private Sprite CreateGridSprite()
        {
            // 创建简单的网格纹理
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            
            // 创建网格线条
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / GRID_SIZE);
        }
        
        private void UpdateGridDisplay()
        {
            // 限制更新频率
            if (Time.time - mLastGridUpdate < GRID_UPDATE_INTERVAL) return;
            mLastGridUpdate = Time.time;
            
            // 检查鼠标位置是否改变
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            if (Vector3.Distance(mousePos, mLastMousePosition) > 0.1f)
            {
                mLastMousePosition = mousePos;
                UpdateGridColors();
            }
        }
        
        private void UpdateGridColors()
        {
            if (string.IsNullOrEmpty(mCurrentBuildingType)) return;
            
            // 获取建筑配置
            var buildingConfig = mConfigSystem?.GetBuildingConfig(mCurrentBuildingType);
            if (buildingConfig == null) return;
            
            // 获取鼠标位置的网格坐标
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3 snapPos = mGridUtility?.SnapToGrid(mouseWorldPos) ?? SnapToGrid(mouseWorldPos);
            Vector2 mouseGridPos = WorldToGrid(snapPos);
            
            // 更新所有网格单元颜色
            foreach (var kvp in mGridCells)
            {
                Vector2 gridPos = kvp.Key;
                GameObject gridCell = kvp.Value;
                
                if (gridCell == null) continue;
                
                var spriteRenderer = gridCell.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null) continue;
                
                // 检查是否是预览位置
                bool isPreviewPosition = Vector2.Distance(gridPos, mouseGridPos) < 0.1f;
                
                // 检查位置是否可建造
                Vector3 worldPos = GridToWorld(gridPos);
                bool canBuild = CanBuildAtPosition(worldPos, mCurrentBuildingType);
                
                // 设置颜色
                Color targetColor;
                if (isPreviewPosition)
                {
                    targetColor = canBuild ? previewGridColor : occupiedGridColor;
                }
                else if (canBuild)
                {
                    targetColor = availableGridColor;
                }
                else
                {
                    targetColor = occupiedGridColor;
                }
                
                targetColor.a = gridAlpha;
                spriteRenderer.color = targetColor;
            }
        }
        
        private void UpdateBuildingPreview()
        {
            if (buildingPreview == null || string.IsNullOrEmpty(mCurrentBuildingType)) return;
            
            // 获取鼠标世界位置
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            // 吸附到网格
            Vector3 snapPos = mGridUtility?.SnapToGrid(mouseWorldPos) ?? SnapToGrid(mouseWorldPos);
            buildingPreview.transform.position = snapPos;
            
            // 更新预览颜色
            var spriteRenderer = buildingPreview.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                bool canBuild = CanBuildAtPosition(snapPos, mCurrentBuildingType);
                spriteRenderer.color = canBuild ? Color.green : Color.red;
                
                // 确保预览图显示
                if (spriteRenderer.sprite == null)
                {
                    spriteRenderer.sprite = CreateBuildingPreviewSprite(mCurrentBuildingType);
                }
            }
        }
        
        private Sprite CreateBuildingPreviewSprite(string buildingType)
        {
            // 创建建筑预览图标
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            
            // 根据建筑类型设置颜色
            Color buildingColor = GetBuildingTypeColor(buildingType);
            
            // 创建简单的建筑轮廓
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (x < 8 || x >= size - 8 || y < 8 || y >= size - 8)
                    {
                        texture.SetPixel(x, y, Color.black);
                    }
                    else
                    {
                        texture.SetPixel(x, y, buildingColor);
                    }
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
        
        private Color GetBuildingTypeColor(string buildingType)
        {
            var config = mConfigSystem?.GetBuildingConfig(buildingType);
            if (config == null) return Color.gray;
            
            switch (config.Category)
            {
                case BuildingCategory.Defense:
                    return Color.red;
                case BuildingCategory.Production:
                    return Color.yellow;
                case BuildingCategory.Habitat:
                    return Color.blue;
                default:
                    return Color.gray;
            }
        }
        
        private bool CanBuildAtPosition(Vector3 position, string buildingType)
        {
            // 使用建筑系统的检查方法
            if (mBuildingSystem != null)
            {
                return mBuildingSystem.CanBuildAt(position, buildingType);
            }
            
            // 备用检查方法
            return !IsPositionOccupied(position);
        }
        
        private bool IsPositionOccupied(Vector3 position)
        {
            Vector3 gridPos = SnapToGrid(position);
            
            foreach (var building in mGameModel.Buildings.Values)
            {
                if (Vector3.Distance(building.Position, gridPos) < GRID_SIZE * 0.8f)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private Vector3 SnapToGrid(Vector3 position)
        {
            float x = Mathf.Round(position.x / GRID_SIZE) * GRID_SIZE;
            float y = Mathf.Round(position.y / GRID_SIZE) * GRID_SIZE;
            return new Vector3(x, y, 0);
        }
        
        private Vector2 WorldToGrid(Vector3 worldPosition)
        {
            int gridX = Mathf.RoundToInt(worldPosition.x / GRID_SIZE);
            int gridY = Mathf.RoundToInt(worldPosition.y / GRID_SIZE);
            return new Vector2(gridX, gridY);
        }
        
        private Vector3 GridToWorld(Vector2 gridPosition)
        {
            float worldX = gridPosition.x * GRID_SIZE;
            float worldY = gridPosition.y * GRID_SIZE;
            return new Vector3(worldX, worldY, 0);
        }
        
        private void ClearGridCells()
        {
            foreach (var cell in mGridCells.Values)
            {
                if (cell != null)
                {
                    DestroyImmediate(cell);
                }
            }
            mGridCells.Clear();
        }
        
        // 事件处理
        private void OnBuildModeChanged(BuildModeChangedEvent e)
        {
            if (e.InBuildMode)
            {
                ShowBuildGrid(e.BuildingType);
            }
            else
            {
                HideBuildGrid();
            }
        }
        
        private void OnBuildingPlaced(BuildingStartedEvent e)
        {
            // 建筑放置后刷新网格颜色
            if (mIsGridVisible)
            {
                UpdateGridColors();
            }
        }
        
        private void OnBuildingRemoved(BuildingDemolishedEvent e)
        {
            // 建筑移除后刷新网格颜色
            if (mIsGridVisible)
            {
                UpdateGridColors();
            }
        }
        
        private void OnGridAlphaChanged(GridAlphaChangedEvent e)
        {
            gridAlpha = e.Alpha;
            if (mIsGridVisible)
            {
                UpdateGridColors();
            }
        }
        
        private void OnGridRangeChanged(GridRangeChangedEvent e)
        {
            gridRange = e.Range;
            if (mIsGridVisible)
            {
                CreateGridCells();
            }
        }
        
        // 公共接口
        public bool IsGridVisible => mIsGridVisible;
        
        public void SetGridRange(int range)
        {
            gridRange = Mathf.Clamp(range, 10, 50);
            if (mIsGridVisible)
            {
                CreateGridCells();
            }
        }
        
        public void SetGridAlpha(float alpha)
        {
            gridAlpha = Mathf.Clamp01(alpha);
            if (mIsGridVisible)
            {
                UpdateGridColors();
            }
        }
        
        // 实现IController接口
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
        
        private void OnDestroy()
        {
            // 清理资源
            ClearGridCells();
            
            // 取消事件订阅
            this.UnRegisterEvent<BuildModeChangedEvent>(OnBuildModeChanged);
            this.UnRegisterEvent<BuildingStartedEvent>(OnBuildingPlaced);
            this.UnRegisterEvent<BuildingDemolishedEvent>(OnBuildingRemoved);
            this.UnRegisterEvent<GridAlphaChangedEvent>(OnGridAlphaChanged);
            this.UnRegisterEvent<GridRangeChangedEvent>(OnGridRangeChanged);
        }
        
        private void CreateBuildingPreview()
        {
            if (buildingPreview == null)
            {
                buildingPreview = new GameObject("BuildingPreview");
                buildingPreview.transform.SetParent(transform);
                
                var spriteRenderer = buildingPreview.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = 10;
                spriteRenderer.sortingLayerName = "UI";
                buildingPreview.SetActive(false);
            }
        }
    }
}

// 网格UI相关事件
namespace MyGameNamespace
{
    public struct BuildingStartedEvent
    {
        public string BuildingType;
        public Vector3 Position;
        public string BuildingId;
    }
    
    public struct BuildingDemolishedEvent
    {
        public string BuildingType;
        public Vector3 Position;
        public string BuildingId;
    }
    
    // public struct BuildModeChangedEvent
    // {
    //     public bool InBuildMode;
    //     public string BuildingType;
    // }
    //
    // public struct GridAlphaChangedEvent
    // {
    //     public float Alpha;
    // }
    //
    // public struct GridRangeChangedEvent
    // {
    //     public int Range;
    // }
} 