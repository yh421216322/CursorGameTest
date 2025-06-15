using UnityEngine;
using QFramework;
using SurvivalGame.Model;
using System.Collections.Generic;

namespace SurvivalGame.Utility
{
    /// <summary>
    /// 网格工具接口
    /// </summary>
    public interface IGridUtility : QFIUtility
    {
        Vector3 SnapToGrid(Vector3 worldPosition);
        Vector2 WorldToGrid(Vector3 worldPosition);
        Vector3 GridToWorld(Vector2 gridPosition);
        bool IsGridPositionOccupied(Vector2 gridPosition);
        bool IsAreaOccupied(Vector2 gridCenter, Vector2 size);
        void OccupyGridArea(Vector2 gridCenter, Vector2 size, int buildingId);
        void FreeGridArea(Vector2 gridCenter, Vector2 size);
        List<Vector2> GetOccupiedGrids(Vector2 gridCenter, Vector2 size);
        bool CanPlaceBuildingAt(Vector3 worldPosition, Vector2 buildingSize);
        Vector3 GetBestSnapPosition(Vector3 worldPosition, Vector2 buildingSize);
    }

    /// <summary>
    /// 网格工具实现
    /// 功能：提供建筑网格吸附、重叠检测、网格管理功能
    /// 依赖模型：无直接依赖
    /// 注册方式：在RegisterManager中调用RegisterUtility<IGridUtility>(new GridUtility())
    /// </summary>
    public class GridUtility :  IGridUtility
    {
        // 网格设置
        private const float GRID_SIZE = 2f;
        private const int MAX_GRID_SIZE = 100; // 网格大小限制 (-50 到 +50)
        
        // 网格占用状态
        private Dictionary<Vector2, int> gridOccupancy; // Vector2是网格坐标，int是建筑ID（0表示空闲）
        
        public GridUtility()
        {
            gridOccupancy = new Dictionary<Vector2, int>();
            Debug.Log("网格工具初始化完成");
        }
        
        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            float snappedX = Mathf.Round(worldPosition.x / GRID_SIZE) * GRID_SIZE;
            float snappedY = Mathf.Round(worldPosition.y / GRID_SIZE) * GRID_SIZE;
            return new Vector3(snappedX, snappedY, 0); // 2D游戏Z轴固定为0
        }
        
        public Vector2 WorldToGrid(Vector3 worldPosition)
        {
            int gridX = Mathf.RoundToInt(worldPosition.x / GRID_SIZE);
            int gridY = Mathf.RoundToInt(worldPosition.y / GRID_SIZE);
            return new Vector2(gridX, gridY);
        }
        
        public Vector3 GridToWorld(Vector2 gridPosition)
        {
            float worldX = gridPosition.x * GRID_SIZE;
            float worldY = gridPosition.y * GRID_SIZE;
            return new Vector3(worldX, worldY, 0);
        }
        
        public bool IsGridPositionOccupied(Vector2 gridPosition)
        {
            return gridOccupancy.ContainsKey(gridPosition) && gridOccupancy[gridPosition] != 0;
        }
        
        public bool IsAreaOccupied(Vector2 gridCenter, Vector2 size)
        {
            var gridsToCheck = GetOccupiedGrids(gridCenter, size);
            
            foreach (var gridPos in gridsToCheck)
            {
                if (IsGridPositionOccupied(gridPos))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        public void OccupyGridArea(Vector2 gridCenter, Vector2 size, int buildingId)
        {
            var gridsToOccupy = GetOccupiedGrids(gridCenter, size);
            
            foreach (var gridPos in gridsToOccupy)
            {
                gridOccupancy[gridPos] = buildingId;
            }
            
            Debug.Log($"建筑 {buildingId} 占用网格区域: 中心({gridCenter.x}, {gridCenter.y}) 大小({size.x}, {size.y})");
        }
        
        public void FreeGridArea(Vector2 gridCenter, Vector2 size)
        {
            var gridsToFree = GetOccupiedGrids(gridCenter, size);
            
            foreach (var gridPos in gridsToFree)
            {
                if (gridOccupancy.ContainsKey(gridPos))
                {
                    gridOccupancy[gridPos] = 0; // 设为空闲
                }
            }
            
            Debug.Log($"释放网格区域: 中心({gridCenter.x}, {gridCenter.y}) 大小({size.x}, {size.y})");
        }
        
        public List<Vector2> GetOccupiedGrids(Vector2 gridCenter, Vector2 size)
        {
            List<Vector2> grids = new List<Vector2>();
            
            // 计算占用的网格范围
            int halfWidth = Mathf.CeilToInt(size.x / 2f);
            int halfHeight = Mathf.CeilToInt(size.y / 2f);
            
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    Vector2 gridPos = new Vector2(gridCenter.x + x, gridCenter.y + y);
                    
                    // 检查是否在有效范围内
                    if (Mathf.Abs(gridPos.x) <= MAX_GRID_SIZE && Mathf.Abs(gridPos.y) <= MAX_GRID_SIZE)
                    {
                        grids.Add(gridPos);
                    }
                }
            }
            
            return grids;
        }
        
        public bool CanPlaceBuildingAt(Vector3 worldPosition, Vector2 buildingSize)
        {
            Vector3 snappedPosition = SnapToGrid(worldPosition);
            Vector2 gridPosition = WorldToGrid(snappedPosition);
            
            return !IsAreaOccupied(gridPosition, buildingSize);
        }
        
        public Vector3 GetBestSnapPosition(Vector3 worldPosition, Vector2 buildingSize)
        {
            Vector3 baseSnapPosition = SnapToGrid(worldPosition);
            
            // 如果当前位置可用，直接返回
            if (CanPlaceBuildingAt(baseSnapPosition, buildingSize))
            {
                return baseSnapPosition;
            }
            
            // 搜索附近的可用位置
            for (int radius = 1; radius <= 5; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        Vector3 testPosition = baseSnapPosition + new Vector3(x * GRID_SIZE, y * GRID_SIZE, 0);
                        
                        if (CanPlaceBuildingAt(testPosition, buildingSize))
                        {
                            return testPosition;
                        }
                    }
                }
            }
            
            // 如果找不到可用位置，返回原始吸附位置
            return baseSnapPosition;
        }
        
        /// <summary>
        /// 获取指定建筑占用的网格列表
        /// </summary>
        public List<Vector2> GetBuildingOccupiedGrids(int buildingId)
        {
            List<Vector2> occupiedGrids = new List<Vector2>();
            
            foreach (var kvp in gridOccupancy)
            {
                if (kvp.Value == buildingId)
                {
                    occupiedGrids.Add(kvp.Key);
                }
            }
            
            return occupiedGrids;
        }
        
        /// <summary>
        /// 移除指定建筑的所有网格占用
        /// </summary>
        public void FreeBuildingGrids(int buildingId)
        {
            var gridsToFree = new List<Vector2>();
            
            foreach (var kvp in gridOccupancy)
            {
                if (kvp.Value == buildingId)
                {
                    gridsToFree.Add(kvp.Key);
                }
            }
            
            foreach (var gridPos in gridsToFree)
            {
                gridOccupancy[gridPos] = 0;
            }
            
            Debug.Log($"释放建筑 {buildingId} 的所有网格占用，共 {gridsToFree.Count} 个网格");
        }
        
        /// <summary>
        /// 获取网格尺寸
        /// </summary>
        public float GetGridSize()
        {
            return GRID_SIZE;
        }
        
        /// <summary>
        /// 清理所有网格占用（用于重置游戏状态）
        /// </summary>
        public void ClearAllGrids()
        {
            gridOccupancy.Clear();
            Debug.Log("清理所有网格占用状态");
        }
        
        /// <summary>
        /// 获取网格占用状态的调试信息
        /// </summary>
        public string GetGridDebugInfo()
        {
            int totalOccupied = 0;
            Dictionary<int, int> buildingGridCounts = new Dictionary<int, int>();
            
            foreach (var kvp in gridOccupancy)
            {
                if (kvp.Value != 0)
                {
                    totalOccupied++;
                    
                    if (!buildingGridCounts.ContainsKey(kvp.Value))
                        buildingGridCounts[kvp.Value] = 0;
                    buildingGridCounts[kvp.Value]++;
                }
            }
            
            string debugInfo = $"网格占用状态 - 总占用: {totalOccupied}, 建筑分布: ";
            foreach (var kvp in buildingGridCounts)
            {
                debugInfo += $"建筑{kvp.Key}:{kvp.Value}格 ";
            }
            
            return debugInfo;
        }
    }
} 