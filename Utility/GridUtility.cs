// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：GridUtility.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了网格工具类 (GridUtility) 及其接口 (IGridUtility)。
//     该工具类提供了一系列与游戏世界中的二维网格系统相关的功能，
//     例如坐标转换（世界坐标与网格坐标互转）、位置吸附到网格、
//     检查特定网格或区域是否被占用、标记/释放网格占用状态等。
//     这对于游戏内建筑的放置和管理至关重要。
// ==============================================================================

using UnityEngine; // 引入Unity引擎核心命名空间，用于Vector3, Mathf等
using QFramework;    // QFramework框架的相关引用
using SurvivalGame.Model; // 游戏核心数据模型的引用 (虽然此类无直接模型依赖，但可能间接相关)
using System.Collections.Generic; // 用于使用List和Dictionary等集合类型

namespace SurvivalGame.Utility // 将此工具类归类到Utility命名空间
{
    /// <summary>
    /// 网格工具接口 (IGridUtility)。
    /// 定义了网格系统应提供的标准功能，如坐标转换、占用检查及管理。
    /// 继承自 QFIUtility，表明它是一个QFramework框架下的工具类。
    /// </summary>
    public interface IGridUtility : QFIUtility
    {
        /// <summary>
        /// 将给定的世界坐标吸附到最近的网格点。
        /// </summary>
        /// <param name="worldPosition">原始世界坐标。</param>
        /// <returns>吸附到网格后的世界坐标。</returns>
        Vector3 SnapToGrid(Vector3 worldPosition);

        /// <summary>
        /// 将世界坐标转换为对应的网格坐标。
        /// </summary>
        /// <param name="worldPosition">世界坐标。</param>
        /// <returns>对应的二维网格坐标 (通常是整数表示的格子索引)。</returns>
        Vector2 WorldToGrid(Vector3 worldPosition);

        /// <summary>
        /// 将网格坐标转换为对应的世界坐标（通常是格子的中心点）。
        /// </summary>
        /// <param name="gridPosition">二维网格坐标。</param>
        /// <returns>对应的世界坐标。</returns>
        Vector3 GridToWorld(Vector2 gridPosition);

        /// <summary>
        /// 检查指定的单个网格坐标位置是否已被占用。
        /// </summary>
        /// <param name="gridPosition">要检查的网格坐标。</param>
        /// <returns>如果该网格已被占用，则返回true；否则返回false。</returns>
        bool IsGridPositionOccupied(Vector2 gridPosition);

        /// <summary>
        /// 检查以指定网格坐标为中心、给定大小的区域是否与任何已占用的网格重叠。
        /// </summary>
        /// <param name="gridCenter">区域的中心网格坐标。</param>
        /// <param name="size">区域的大小（通常是占用的网格单元数量，例如2x2个格子）。</param>
        /// <returns>如果区域内有任何网格被占用，则返回true；否则返回false。</returns>
        bool IsAreaOccupied(Vector2 gridCenter, Vector2 size);

        /// <summary>
        /// 标记以指定网格坐标为中心、给定大小的区域为已被占用。
        /// </summary>
        /// <param name="gridCenter">区域的中心网格坐标。</param>
        /// <param name="size">区域的大小。</param>
        /// <param name="buildingId">占用该区域的建筑或其他实体的ID。</param>
        void OccupyGridArea(Vector2 gridCenter, Vector2 size, int buildingId);

        /// <summary>
        /// 释放以指定网格坐标为中心、给定大小的区域的占用状态。
        /// </summary>
        /// <param name="gridCenter">区域的中心网格坐标。</param>
        /// <param name="size">区域的大小。</param>
        void FreeGridArea(Vector2 gridCenter, Vector2 size);

        /// <summary>
        /// 获取以指定网格坐标为中心、给定大小的区域所覆盖的所有网格坐标列表。
        /// </summary>
        /// <param name="gridCenter">区域的中心网格坐标。</param>
        /// <param name="size">区域的大小。</param>
        /// <returns>构成该区域的所有网格坐标的列表。</returns>
        List<Vector2> GetOccupiedGrids(Vector2 gridCenter, Vector2 size);

        /// <summary>
        /// 检查是否可以在指定的世界坐标位置放置一个给定大小的建筑。
        /// 此方法通常会先将世界坐标转换为网格坐标，然后检查对应区域是否被占用。
        /// </summary>
        /// <param name="worldPosition">尝试放置建筑的世界坐标。</param>
        /// <param name="buildingSize">建筑的占地大小（网格单位）。</param>
        /// <returns>如果可以放置则返回true，否则返回false。</returns>
        bool CanPlaceBuildingAt(Vector3 worldPosition, Vector2 buildingSize);

        /// <summary>
        /// 根据给定的世界坐标和建筑大小，找到一个最佳的、吸附到网格的可放置位置。
        /// 如果原始吸附位置不可用，则可能在其附近搜索。
        /// </summary>
        /// <param name="worldPosition">玩家期望放置的初始世界坐标。</param>
        /// <param name="buildingSize">建筑的占地大小。</param>
        /// <returns>一个调整过的、吸附到网格且可用的最佳世界坐标；如果找不到，则可能返回原始吸附位置或其他指示。</returns>
        Vector3 GetBestSnapPosition(Vector3 worldPosition, Vector2 buildingSize);
    }

    /// <summary>
    /// 网格工具 (GridUtility) 的具体实现类。
    /// 功能：提供建筑放置时的网格吸附、重叠检测以及网格占用状态的管理功能。
    /// 依赖模型：无直接的游戏核心数据模型依赖，主要管理自身的网格占用数据。
    /// 注册方式：通常在游戏的启动流程中，通过QFramework的依赖注入或服务注册机制（如 RegisterUtility）进行注册。
    /// </summary>
    public class GridUtility : IGridUtility // 未继承AbstractUtility，但实现了IGridUtility，符合QFramework工具类要求
    {
        // --- 网格设置 (Grid Settings) ---
        /// <summary>
        /// 单个网格单元的边长（世界单位）。例如，2f表示每个格子是2x2米。
        /// </summary>
        private const float GRID_SIZE = 2f;
        /// <summary>
        /// 网格坐标的最大绝对值限制。例如，100表示网格范围从 (-100, -100) 到 (100, 100)。
        /// </summary>
        private const int MAX_GRID_SIZE = 100;
        
        // --- 网格占用状态 (Grid Occupancy Status) ---
        /// <summary>
        /// 存储网格占用信息的字典。
        /// 键 (Vector2) 是网格的整数坐标 (例如 (0,0), (1,0), (0,1))。
        /// 值 (int) 是占用该网格的建筑或其他实体的ID。如果值为0或键不存在，则表示该网格空闲。
        /// </summary>
        private Dictionary<Vector2, int> gridOccupancy;
        
        /// <summary>
        /// GridUtility的构造函数。
        /// 初始化网格占用状态字典。
        /// </summary>
        public GridUtility()
        {
            gridOccupancy = new Dictionary<Vector2, int>();
            Debug.Log("[网格工具] 初始化完成。");
        }
        
        /// <summary>
        /// 将给定的世界坐标吸附到最近的网格点。
        /// </summary>
        /// <param name="worldPosition">原始世界坐标。</param>
        /// <returns>吸附到网格后的世界坐标（Z轴通常设为0，适用于2D或2.5D俯视角游戏）。</returns>
        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            // 将世界坐标的X和Y分量分别除以网格大小，四舍五入到最近的整数，然后再乘以网格大小
            float snappedX = Mathf.Round(worldPosition.x / GRID_SIZE) * GRID_SIZE;
            float snappedY = Mathf.Round(worldPosition.y / GRID_SIZE) * GRID_SIZE;
            return new Vector3(snappedX, snappedY, 0); // 假设为2D游戏，Z轴固定为0
        }
        
        /// <summary>
        /// 将世界坐标转换为对应的整数网格坐标。
        /// </summary>
        /// <param name="worldPosition">世界坐标。</param>
        /// <returns>二维整数向量表示的网格坐标。</returns>
        public Vector2 WorldToGrid(Vector3 worldPosition)
        {
            // 计算方式与SnapToGrid类似，但不乘回GRID_SIZE，直接取整得到网格索引
            int gridX = Mathf.RoundToInt(worldPosition.x / GRID_SIZE);
            int gridY = Mathf.RoundToInt(worldPosition.y / GRID_SIZE);
            return new Vector2(gridX, gridY);
        }
        
        /// <summary>
        /// 将整数网格坐标转换为对应的世界坐标（通常是格子的中心点）。
        /// </summary>
        /// <param name="gridPosition">二维整数网格坐标。</param>
        /// <returns>对应的世界坐标（Z轴设为0）。</returns>
        public Vector3 GridToWorld(Vector2 gridPosition)
        {
            float worldX = gridPosition.x * GRID_SIZE; // 网格X索引乘以格子大小
            float worldY = gridPosition.y * GRID_SIZE; // 网格Y索引乘以格子大小
            return new Vector3(worldX, worldY, 0); // Z轴为0
        }
        
        /// <summary>
        /// 检查指定的单个网格坐标位置是否已被占用。
        /// </summary>
        /// <param name="gridPosition">要检查的网格坐标。</param>
        /// <returns>如果该网格在占用记录中且占用者ID不为0，则返回true；否则返回false。</returns>
        public bool IsGridPositionOccupied(Vector2 gridPosition)
        {
            // 检查字典中是否存在该网格坐标的记录，并且其对应的占用ID不为0（0通常表示空闲）
            return gridOccupancy.ContainsKey(gridPosition) && gridOccupancy[gridPosition] != 0;
        }
        
        /// <summary>
        /// 检查以指定网格坐标为中心、给定大小的矩形区域内是否有任何网格被占用。
        /// </summary>
        /// <param name="gridCenter">区域的中心网格坐标。</param>
        /// <param name="size">区域的大小（占用的网格单元数，例如 (2,2) 表示2x2个格子）。</param>
        /// <returns>如果区域内有任何网格被占用，则返回true；否则返回false。</returns>
        public bool IsAreaOccupied(Vector2 gridCenter, Vector2 size)
        {
            // 获取该区域覆盖的所有网格坐标
            var gridsInArea = GetOccupiedGrids(gridCenter, size); // 注意：此方法名可能易误导，它实际返回区域内的所有格子，无论是否占用
            
            foreach (var gridPos in gridsInArea) // 遍历区域内的每个格子
            {
                if (IsGridPositionOccupied(gridPos)) // 检查单个格子是否被占用
                {
                    return true; // 如果有任何一个格子被占用，则整个区域被视为占用
                }
            }
            
            return false; // 区域内所有格子都未被占用
        }
        
        /// <summary>
        /// 标记以指定网格坐标为中心、给定大小的区域为已被特定建筑ID占用。
        /// </summary>
        /// <param name="gridCenter">区域的中心网格坐标。</param>
        /// <param name="size">区域的大小。</param>
        /// <param name="buildingId">占用该区域的建筑的ID。</param>
        public void OccupyGridArea(Vector2 gridCenter, Vector2 size, int buildingId)
        {
            var gridsToOccupy = GetOccupiedGrids(gridCenter, size); // 获取该区域覆盖的所有网格坐标
            
            foreach (var gridPos in gridsToOccupy) // 遍历这些网格
            {
                gridOccupancy[gridPos] = buildingId; // 在占用字典中记录占用者ID
            }
            
            Debug.Log($"[网格工具] 建筑 {buildingId} 已占用网格区域: 中心({gridCenter.x}, {gridCenter.y}), 大小({size.x}x{size.y})。");
        }
        
        /// <summary>
        /// 释放以指定网格坐标为中心、给定大小的区域的占用状态（标记为空闲）。
        /// </summary>
        /// <param name="gridCenter">区域的中心网格坐标。</param>
        /// <param name="size">区域的大小。</param>
        public void FreeGridArea(Vector2 gridCenter, Vector2 size)
        {
            var gridsToFree = GetOccupiedGrids(gridCenter, size); // 获取该区域覆盖的所有网格坐标
            
            foreach (var gridPos in gridsToFree) // 遍历这些网格
            {
                if (gridOccupancy.ContainsKey(gridPos)) // 如果该网格之前有占用记录
                {
                    gridOccupancy[gridPos] = 0; // 将其占用者ID设为0，表示空闲
                }
                // 如果字典中原本就没有这个键，则它本来就是空闲的，无需操作
            }
            
            Debug.Log($"[网格工具] 已释放网格区域: 中心({gridCenter.x}, {gridCenter.y}), 大小({size.x}x{size.y})。");
        }
        
        /// <summary>
        /// 获取以指定网格坐标为中心、给定大小的矩形区域所覆盖的所有网格坐标的列表。
        /// </summary>
        /// <param name="gridCenter">区域的中心网格坐标。</param>
        /// <param name="size">区域的大小（X和Y方向占用的网格单元数）。</param>
        /// <returns>构成该区域的所有网格坐标的列表。</returns>
        public List<Vector2> GetOccupiedGrids(Vector2 gridCenter, Vector2 size) // 方法名可能应为GetGridsInArea更准确
        {
            List<Vector2> grids = new List<Vector2>();
            
            // 计算区域在X和Y方向上距离中心点的半宽度/半高度（向上取整，以覆盖完整格子）
            // 例如，size.x=2, halfWidth=1, 循环从-1到1，覆盖3个格子。如果size.x=1, halfWidth=1, 也是-1到1。
            // 这看起来像是在处理以gridCenter为“锚点”然后扩展size/2的情况，但CeilToInt可能使它覆盖更多。
            // 标准处理方式通常是：startX = centerX - floor(sizeX/2), endX = centerX + ceil(sizeX/2)-1 (或类似)
            // 当前实现：如果size是(1,1), half=1, 循环-1,0,1 (3x3格子)。如果size是(2,2), half=1, 也是3x3格子。这可能不是预期。
            // 假设size代表实际的格子数，例如size(2,2)代表2x2的区域。
            // 如果gridCenter是区域的左下角，则循环应为：x from 0 to size.x-1, y from 0 to size.y-1。
            // 如果gridCenter是区域的中心，则：
            int startX = Mathf.RoundToInt(gridCenter.x - (size.x / 2f) + 0.5f * (size.x % 2 == 0 ? 0 : 1)); // 尝试更精确的起始点
            int endX = Mathf.RoundToInt(gridCenter.x + (size.x / 2f) - 0.5f * (size.x % 2 == 0 ? 1 : 0));   // 尝试更精确的结束点
            int startY = Mathf.RoundToInt(gridCenter.y - (size.y / 2f) + 0.5f * (size.y % 2 == 0 ? 0 : 1));
            int endY = Mathf.RoundToInt(gridCenter.y + (size.y / 2f) - 0.5f * (size.y % 2 == 0 ? 1 : 0));
            
            // 当前的CeilToInt方法对于size=1会产生-1 to 1的循环，覆盖3个格子，可能需要根据具体设计调整。
            // 为保持与原逻辑一致，暂时保留CeilToInt，但标记为潜在问题。
            int halfWidth = Mathf.CeilToInt(size.x / 2f);  // 覆盖范围：中心 +/- (halfWidth - (size.x % 2 == 0 ? 1: 0) )
            int halfHeight = Mathf.CeilToInt(size.y / 2f); // 如果size.x=1, halfWidth=1, 循环x from gridCenter.x-0 to gridCenter.x+0 (如果x=0)
                                                        // 如果size.x=2, halfWidth=1, 循环x from gridCenter.x-0 to gridCenter.x+0 (如果x=0)
                                                        // 这部分逻辑可能需要重新审视以确保精确覆盖 buildingSize 定义的区域。
                                                        // 例如，一个2x2的建筑，如果中心是(0,0)，它应该覆盖(-0.5,-0.5)到(0.5,0.5)的网格，即(0,0),(1,0),(0,1),(1,1)或(-1,-1),(0,-1),(-1,0),(0,0)等，取决于锚点。
                                                        // 当前的CeilToInt(size.x/2f)对于size=2会是1，循环-1,0,1。对于size=1也是1，循环-1,0,1.
            // 修正迭代范围以正确反映建筑尺寸：
            // 对于一个以gridCenter为中心，尺寸为size.x * size.y的建筑
            // 它覆盖的格子范围是：
            // x: from floor(gridCenter.x - size.x/2 + epsilon) to floor(gridCenter.x + size.x/2 - epsilon)
            // y: from floor(gridCenter.y - size.y/2 + epsilon) to floor(gridCenter.y + size.y/2 - epsilon)
            // 或者，更简单地，如果gridCenter是对齐的，且size是奇数，则 +/- (size-1)/2
            // 如果size是偶数，则需要定义锚点（左下角或中心）。假设中心，则 +/- size/2-1 和 +size/2

            // 假设 gridCenter 是区域的中心，size.x 和 size.y 是占用的格子数。
            // 例如 size (2,2) 以 (0.5, 0.5) 为中心，应覆盖 (0,0), (1,0), (0,1), (1,1)
            // 如果以整数 gridCenter 为中心：
            float startOffsetX = -(size.x - 1) / 2.0f;
            float endOffsetX = (size.x - 1) / 2.0f;
            float startOffsetY = -(size.y - 1) / 2.0f;
            float endOffsetY = (size.y - 1) / 2.0f;

            for (float xOff = startOffsetX; xOff <= endOffsetX; xOff += 1.0f)
            {
                for (float yOff = startOffsetY; yOff <= endOffsetY; yOff += 1.0f)
                {
                    Vector2 gridPos = new Vector2(Mathf.FloorToInt(gridCenter.x + xOff), Mathf.FloorToInt(gridCenter.y + yOff));
                    
                    // 检查坐标是否在定义的网格最大范围内
                    if (Mathf.Abs(gridPos.x) <= MAX_GRID_SIZE && Mathf.Abs(gridPos.y) <= MAX_GRID_SIZE)
                    {
                        grids.Add(gridPos);
                    }
                }
            }
            
            return grids; // 返回构成该区域的所有网格坐标
        }
        
        /// <summary>
        /// 检查在给定的世界坐标位置是否可以放置一个特定大小的建筑。
        /// </summary>
        /// <param name="worldPosition">期望放置建筑的世界坐标。</param>
        /// <param name="buildingSize">建筑的占地尺寸（网格单位）。</param>
        /// <returns>如果可以放置则返回true，否则返回false。</returns>
        public bool CanPlaceBuildingAt(Vector3 worldPosition, Vector2 buildingSize)
        {
            Vector3 snappedPosition = SnapToGrid(worldPosition); // 首先将世界坐标吸附到网格
            Vector2 gridCenterPosition = WorldToGrid(snappedPosition); // 然后转换为网格中心坐标
            
            // 检查以此网格中心和给定尺寸构成的区域是否已被占用
            return !IsAreaOccupied(gridCenterPosition, buildingSize);
        }
        
        /// <summary>
        /// 为给定大小的建筑在指定世界坐标附近寻找一个最佳的可放置位置（已吸附到网格）。
        /// 如果初始吸附位置不可用，则向外螺旋搜索一定范围内的可用位置。
        /// </summary>
        /// <param name="worldPosition">玩家期望放置的初始世界坐标。</param>
        /// <param name="buildingSize">建筑的占地大小（网格单位）。</param>
        /// <returns>最佳可放置的世界坐标；如果找不到，则返回原始的吸附位置（可能仍不可用）。</returns>
        public Vector3 GetBestSnapPosition(Vector3 worldPosition, Vector2 buildingSize)
        {
            Vector3 baseSnapPosition = SnapToGrid(worldPosition); // 获取初始的网格吸附位置
            
            // 首先检查初始吸附位置是否可用
            if (CanPlaceBuildingAt(baseSnapPosition, buildingSize))
            {
                return baseSnapPosition; // 如果可用，直接返回此位置
            }
            
            // 如果初始位置不可用，则在其周围进行螺旋式或扩展矩形式搜索
            // 示例：从半径为1的邻近格子开始，逐步扩大搜索半径，最大到5个格子远
            for (int radius = 1; radius <= 5; radius++) // 搜索半径
            {
                for (int xOffset = -radius; xOffset <= radius; xOffset++) // X方向偏移
                {
                    for (int yOffset = -radius; yOffset <= radius; yOffset++) // Y方向偏移
                    {
                        // 跳过内部点，只检查当前半径“环”上的点，以实现扩展搜索
                        if (Mathf.Abs(xOffset) < radius && Mathf.Abs(yOffset) < radius) continue;
                        
                        Vector3 testPosition = baseSnapPosition + new Vector3(xOffset * GRID_SIZE, yOffset * GRID_SIZE, 0); // 计算测试位置

                        if (CanPlaceBuildingAt(testPosition, buildingSize)) // 如果此测试位置可用
                        {
                            return testPosition; // 返回找到的第一个可用位置
                        }
                    }
                }
            }
            
            // 如果在搜索范围内没有找到可用位置，则返回原始的吸附位置（尽管它可能仍不可用）
            // 更好的做法可能是返回一个特定的无效标记或抛出异常，取决于调用者的期望
            return baseSnapPosition;
        }
        
        /// <summary>
        /// 获取指定建筑ID当前占用的所有网格坐标列表。
        /// </summary>
        /// <param name="buildingId">要查询的建筑ID。</param>
        /// <returns>该建筑占用的网格坐标列表；如果建筑未占用任何网格，则返回空列表。</returns>
        public List<Vector2> GetBuildingOccupiedGrids(int buildingId)
        {
            List<Vector2> occupiedGrids = new List<Vector2>();
            if (buildingId == 0) return occupiedGrids; // ID为0通常表示空闲或无效

            foreach (var kvp in gridOccupancy) // 遍历所有已记录的网格占用信息
            {
                if (kvp.Value == buildingId) // 如果网格的占用者ID与查询的建筑ID匹配
                {
                    occupiedGrids.Add(kvp.Key); // 则将此网格坐标添加到结果列表
                }
            }
            
            return occupiedGrids;
        }
        
        /// <summary>
        /// 释放指定建筑ID当前占用的所有网格单元，将它们标记为空闲。
        /// </summary>
        /// <param name="buildingId">要释放其所占网格的建筑ID。</param>
        public void FreeBuildingGrids(int buildingId)
        {
            if (buildingId == 0) return; // 无效ID，不执行操作

            var gridsToActuallyFree = new List<Vector2>(); // 用于存储确实需要被释放的格子
            
            // 遍历查找所有被该buildingId占用的格子
            foreach (var kvp in gridOccupancy)
            {
                if (kvp.Value == buildingId)
                {
                    gridsToActuallyFree.Add(kvp.Key);
                }
            }
            
            // 将这些格子标记为空闲 (值为0)
            foreach (var gridPos in gridsToActuallyFree)
            {
                gridOccupancy[gridPos] = 0;
                // 或者 gridOccupancy.Remove(gridPos); 如果空闲格子不存储在字典中
            }
            
            if (gridsToActuallyFree.Any())
            {
                Debug.Log($"[网格工具] 已释放建筑 {buildingId} 所占用的 {gridsToActuallyFree.Count} 个网格。");
            }
        }
        
        /// <summary>
        /// 获取网格系统中定义的单个网格单元的尺寸（边长）。
        /// </summary>
        /// <returns>网格单元的尺寸。</returns>
        public float GetGridSize()
        {
            return GRID_SIZE;
        }
        
        /// <summary>
        /// 清理所有网格的占用状态，将整个网格系统重置为空闲。
        /// 通常用于游戏重置或加载新关卡等场景。
        /// </summary>
        public void ClearAllGrids()
        {
            gridOccupancy.Clear(); // 清空占用字典
            Debug.Log("[网格工具] 已清理所有网格占用状态。");
        }
        
        /// <summary>
        /// 获取当前网格占用状态的调试信息字符串。
        /// </summary>
        /// <returns>包含总占用格子数和各建筑占用格子数的调试信息字符串。</returns>
        public string GetGridDebugInfo()
        {
            int totalOccupiedCells = 0; // 总占用格子计数
            // 用于统计每个建筑ID占用了多少格子的字典
            Dictionary<int, int> buildingCellCounts = new Dictionary<int, int>();
            
            foreach (var kvp in gridOccupancy) // 遍历所有记录的网格占用
            {
                if (kvp.Value != 0) // 如果格子被占用 (占用ID不为0)
                {
                    totalOccupiedCells++; // 总占用格子数增加
                    
                    // 统计每个建筑ID占用的格子数
                    if (!buildingCellCounts.ContainsKey(kvp.Value))
                        buildingCellCounts[kvp.Value] = 0;
                    buildingCellCounts[kvp.Value]++;
                }
            }
            
            // 构建调试信息字符串
            System.Text.StringBuilder debugInfoBuilder = new System.Text.StringBuilder();
            debugInfoBuilder.Append($"[网格调试信息] - 总占用格子数: {totalOccupiedCells}。");
            if (buildingCellCounts.Any())
            {
                debugInfoBuilder.Append(" 各建筑占用分布: ");
                foreach (var kvp in buildingCellCounts)
                {
                    debugInfoBuilder.Append($"建筑ID {kvp.Key}: {kvp.Value}格; ");
                }
            }
            else
            {
                debugInfoBuilder.Append(" 当前无建筑占用任何网格。");
            }
            
            return debugInfoBuilder.ToString();
        }
    }
} 