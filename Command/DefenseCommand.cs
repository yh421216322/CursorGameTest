// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：DefenseCommand.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件包含所有与防御系统相关的命令，例如建造、升级、出售防御塔，
//     以及控制战斗状态和获取统计信息等。
// ==============================================================================

using System.Collections.Generic;
using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem; // 包含 IDefenseSystem 等接口
using SurvivalGame;
using SurvivalGame.Model; // 包含 TowerType, DefenseStats, TowerData, ZombieData, ProjectileData 等模型

namespace SurvivalGame.Command
{
    /// <summary>
    /// 建造防御塔命令
    /// </summary>
    public class BuildTowerCommand : AbstractCommand
    {
        // 防御塔建造位置
        public Vector3 Position { get; set; }
        // 要建造的防御塔类型
        public TowerType TowerType { get; set; }
        // 命令执行结果：成功时为防御塔ID，失败时为-1或其他错误码
        public int Result { get; private set; } = -1;
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            // 获取防御系统实例
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            // 调用防御系统的建造方法
            Result = defenseSystem.BuildTower(Position, TowerType);
            
            // 根据结果记录日志
            if (Result > 0)
            {
                Debug.Log($"建造防御塔成功: 类型 {TowerType} 于位置 {Position}，ID: {Result}");
            }
            else
            {
                Debug.LogWarning($"建造防御塔失败: 类型 {TowerType} 于位置 {Position}");
            }
        }
    }

    /// <summary>
    /// 升级防御塔命令
    /// </summary>
    public class UpgradeTowerCommand : AbstractCommand
    {
        // 要升级的防御塔ID
        public int TowerId { get; set; }
        // 命令执行结果：true表示成功，false表示失败
        public bool Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.UpgradeTower(TowerId);
            
            if (Result)
            {
                Debug.Log($"升级防御塔成功: ID {TowerId}");
            }
            else
            {
                Debug.LogWarning($"升级防御塔失败: ID {TowerId}");
            }
        }
    }

    /// <summary>
    /// 出售防御塔命令
    /// </summary>
    public class SellTowerCommand : AbstractCommand
    {
        // 要出售的防御塔ID
        public int TowerId { get; set; }
        // 命令执行结果：true表示成功，false表示失败
        public bool Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.SellTower(TowerId);
            
            if (Result)
            {
                Debug.Log($"出售防御塔成功: ID {TowerId}");
            }
            else
            {
                Debug.LogWarning($"出售防御塔失败: ID {TowerId}");
            }
        }
    }

    /// <summary>
    /// 修理防御塔命令
    /// </summary>
    public class RepairTowerCommand : AbstractCommand
    {
        // 要修理的防御塔ID
        public int TowerId { get; set; }
        // 命令执行结果：true表示成功，false表示失败
        public bool Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.RepairTower(TowerId);
            
            if (Result)
            {
                Debug.Log($"修理防御塔成功: ID {TowerId}");
            }
            else
            {
                Debug.LogWarning($"修理防御塔失败: ID {TowerId}");
            }
        }
    }

    /// <summary>
    /// 开始防御战斗命令
    /// </summary>
    public class StartDefenseCombatCommand : AbstractCommand
    {
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            // 通知防御系统开始战斗
            defenseSystem.StartCombat();
            
            Debug.Log("开始防御战斗");
        }
    }

    /// <summary>
    /// 结束防御战斗命令
    /// </summary>
    public class EndDefenseCombatCommand : AbstractCommand
    {
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            // 通知防御系统结束战斗
            defenseSystem.EndCombat();
            
            Debug.Log("结束防御战斗");
        }
    }

    /// <summary>
    /// 获取防御统计命令
    /// </summary>
    public class GetDefenseStatsCommand : AbstractCommand
    {
        // 命令执行结果：包含防御统计信息
        public DefenseStats Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.GetDefenseStats();
            
            // 记录获取到的部分统计信息
            Debug.Log($"获取防御统计 - 防御塔总数: {Result.totalTowers}, 击杀僵尸总数: {Result.totalZombiesKilled}");
        }
    }

    /// <summary>
    /// 获取所有防御塔信息命令
    /// </summary>
    public class GetAllTowersCommand : AbstractCommand
    {
        // 命令执行结果：包含所有防御塔数据的列表
        public List<TowerData> Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.GetAllTowers();
            
            Debug.Log($"获取所有防御塔 - 共 {Result.Count} 个");
        }
    }

    /// <summary>
    /// 获取指定范围内防御塔命令
    /// </summary>
    public class GetTowersInRangeCommand : AbstractCommand
    {
        // 搜索范围的中心点
        public Vector3 Center { get; set; }
        // 搜索范围的半径
        public float Range { get; set; }
        // 命令执行结果：范围内防御塔数据的列表
        public List<TowerData> Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.GetTowersInRange(Center, Range);
            
            Debug.Log($"获取范围内防御塔 - 中心: {Center}, 范围: {Range}, 找到: {Result.Count} 个");
        }
    }

    /// <summary>
    /// 获取单个防御塔详细信息命令
    /// </summary>
    public class GetTowerInfoCommand : AbstractCommand
    {
        // 要查询的防御塔ID
        public int TowerId { get; set; }
        // 命令执行结果：单个防御塔的数据，如果未找到则为null
        public TowerData Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.GetTower(TowerId);
            
            if (Result != null)
            {
                Debug.Log($"获取防御塔信息: ID {TowerId}, 类型: {Result.Type}, 等级: {Result.Level}");
            }
            else
            {
                Debug.LogWarning($"未找到防御塔: ID {TowerId}");
            }
        }
    }

    /// <summary>
    /// 批量建造防御塔命令（主要用于调试或特殊游戏逻辑）
    /// </summary>
    public class BatchBuildTowersCommand : AbstractCommand
    {
        // 要建造防御塔的位置列表
        public List<Vector3> Positions { get; set; }
        // 要建造的防御塔类型
        public TowerType TowerType { get; set; }
        // 命令执行结果：每个建造尝试的结果ID列表 (成功为塔ID，失败为-1)
        public List<int> Results { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Results = new List<int>(); // 初始化结果列表
            
            if (Positions != null)
            {
                // 遍历所有指定位置进行建造
                foreach (var position in Positions)
                {
                    int towerId = defenseSystem.BuildTower(position, TowerType);
                    Results.Add(towerId);
                }
                
                // 统计成功建造的数量
                int successCount = Results.FindAll(id => id > 0).Count;
                Debug.Log($"批量建造防御塔 - 尝试建造: {Positions.Count}, 成功: {successCount}");
            }
        }
    }

    /// <summary>
    /// 修理所有防御塔命令
    /// </summary>
    public class RepairAllTowersCommand : AbstractCommand
    {
        // 命令执行结果：成功修理的防御塔数量
        public int Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            // 获取所有防御塔
            var towers = defenseSystem.GetAllTowers();
            
            Result = 0; // 初始化计数器
            foreach (var tower in towers)
            {
                // 检查防御塔是否需要修理 (生命值低于最大生命值)
                if (tower.Health < tower.MaxHealth)
                {
                    // 尝试修理
                    if (defenseSystem.RepairTower(tower.Id))
                    {
                        Result++; // 修理成功则计数
                    }
                }
            }
            
            Debug.Log($"修理所有防御塔 - 成功修理: {Result} 个");
        }
    }

    /// <summary>
    /// 升级所有可升级的防御塔命令
    /// </summary>
    public class UpgradeAllTowersCommand : AbstractCommand
    {
        // 命令执行结果：成功升级的防御塔数量
        public int Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var towers = defenseSystem.GetAllTowers();
            
            Result = 0;
            foreach (var tower in towers)
            {
                // 假设最大等级为5，检查是否可升级
                if (tower.Level < 5)
                {
                    if (defenseSystem.UpgradeTower(tower.Id))
                    {
                        Result++;
                    }
                }
            }
            
            Debug.Log($"升级所有防御塔 - 成功升级: {Result} 个");
        }
    }

    /// <summary>
    /// 出售所有防御塔命令（主要用于调试或游戏结束清理）
    /// </summary>
    public class SellAllTowersCommand : AbstractCommand
    {
        // 命令执行结果：成功出售的防御塔数量
        public int Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var towers = defenseSystem.GetAllTowers();
            
            Result = 0;
            foreach (var tower in towers)
            {
                // 尝试出售
                if (defenseSystem.SellTower(tower.Id))
                {
                    Result++;
                }
            }
            
            Debug.Log($"出售所有防御塔 - 成功出售: {Result} 个");
        }
    }

    /// <summary>
    /// 设置防御塔当前攻击目标命令
    /// </summary>
    public class SetTowerTargetCommand : AbstractCommand
    {
        // 防御塔ID
        public int TowerId { get; set; }
        // 要设置的目标僵尸数据 (可以为null以清除目标)
        public ZombieData Target { get; set; }
        // 命令执行结果：true表示成功，false表示失败 (例如塔不存在)
        public bool Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var tower = defenseSystem.GetTower(TowerId); // 获取防御塔实例
            
            if (tower != null)
            {
                // 设置塔的目标 (具体如何使用此目标取决于TowerAI的实现)
                tower.CurrentTarget = Target;
                Result = true;
                
                if (Target != null)
                {
                    Debug.Log($"设置防御塔目标: 塔 {TowerId} -> 僵尸 {Target.id}");
                }
                else
                {
                    Debug.Log($"清除防御塔目标: 塔 {TowerId}");
                }
            }
            else
            {
                Result = false;
                Debug.LogWarning($"设置防御塔目标失败: 未找到ID为 {TowerId} 的防御塔");
            }
        }
    }

    /// <summary>
    /// 为防御塔寻找最佳攻击目标命令
    /// </summary>
    public class FindBestTargetCommand : AbstractCommand
    {
        // 防御塔当前位置
        public Vector3 TowerPosition { get; set; }
        // 防御塔的攻击范围
        public float Range { get; set; }
        // 防御塔类型 (可能影响目标选择策略)
        public TowerType TowerType { get; set; }
        // 命令执行结果：找到的最佳目标僵尸数据，如果未找到则为null
        public ZombieData Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            // 调用防御系统的目标搜索逻辑
            Result = defenseSystem.FindBestTarget(TowerPosition, Range, TowerType);
            
            if (Result != null)
            {
                Debug.Log($"找到最佳目标: 僵尸 {Result.id} 位置 {Result.position}");
            }
            else
            {
                Debug.Log($"在位置 {TowerPosition} 范围 {Range} 内未找到合适目标");
            }
        }
    }

    /// <summary>
    /// 查找指定范围内所有目标命令
    /// </summary>
    public class FindTargetsInRangeCommand : AbstractCommand
    {
        // 搜索范围的中心点
        public Vector3 Center { get; set; }
        // 搜索范围的半径
        public float Range { get; set; }
        // 命令执行结果：范围内所有目标僵尸数据的列表
        public List<ZombieData> Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.FindTargetsInRange(Center, Range);
            
            Debug.Log($"范围内目标搜索 - 中心: {Center}, 范围: {Range}, 找到: {Result.Count} 个目标");
        }
    }

    /// <summary>
    /// 获取所有当前活动投射物信息命令
    /// </summary>
    public class GetAllProjectilesCommand : AbstractCommand
    {
        // 命令执行结果：所有投射物数据的列表
        public List<ProjectileData> Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.GetAllProjectiles();
            
            Debug.Log($"获取所有投射物 - 共 {Result.Count} 个");
        }
    }

    /// <summary>
    /// 快速建造并升级防御塔命令（主要用于调试）
    /// </summary>
    public class QuickBuildTowerCommand : AbstractCommand
    {
        // 防御塔建造位置
        public Vector3 Position { get; set; }
        // 要建造的防御塔类型
        public TowerType TowerType { get; set; }
        // 目标等级 (默认为1级)
        public int Level { get; set; } = 1;
        // 命令执行结果：成功时为防御塔ID，失败时为-1
        public int Result { get; private set; } = -1;
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            
            // 第一步：先建造1级防御塔
            Result = defenseSystem.BuildTower(Position, TowerType);
            
            // 如果建造成功且目标等级大于1，则进行升级
            if (Result > 0 && Level > 1)
            {
                // 循环升级直到目标等级
                for (int i = 1; i < Level; i++)
                {
                    // 如果某次升级失败，则停止后续升级
                    if (!defenseSystem.UpgradeTower(Result))
                    {
                        Debug.LogWarning($"快速建造过程中升级防御塔 {Result} 到等级 {i+1} 失败。");
                        break;
                    }
                }
            }
            
            if (Result > 0)
            {
                // 获取最终塔的信息，确认等级
                var finalTower = defenseSystem.GetTower(Result);
                int finalLevel = finalTower?.Level ?? 0; // 安全获取等级
                Debug.Log($"快速建造防御塔成功: 类型 {TowerType} 等级 {finalLevel} 于位置 {Position}，ID: {Result}");
            }
            else
            {
                Debug.LogWarning($"快速建造防御塔失败: 类型 {TowerType} 于位置 {Position}");
            }
        }
    }

    /// <summary>
    /// 重置整个防御系统状态命令（主要用于调试或重新开始）
    /// </summary>
    public class ResetDefenseSystemCommand : AbstractCommand
    {
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            
            // 第一步：移除所有现有防御塔 (通过出售逻辑)
            var towers = defenseSystem.GetAllTowers();
            foreach (var tower in towers)
            {
                defenseSystem.SellTower(tower.Id); // 假设SellTower会处理所有清理逻辑
            }
            
            // 第二步：确保战斗状态已结束
            defenseSystem.EndCombat();
            
            // 可选：重置统计数据等其他状态 (取决于IDefenseSystem的实现)
            // defenseSystem.ResetStats();

            Debug.Log("防御系统已重置");
        }
    }

    /// <summary>
    /// 获取当前防御效率命令
    /// </summary>
    public class GetDefenseEfficiencyCommand : AbstractCommand
    {
        // 命令执行结果：计算出的防御效率值
        public float Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var stats = defenseSystem.GetDefenseStats(); // 获取最新的统计数据
            
            // 计算防御效率的简单示例：总造成伤害 / (防御塔数量 * 平均成本)
            // 注意：此计算方式非常简化，实际效率指标可能更复杂
            if (stats.totalTowers > 0 && stats.totalResourcesSpentOnTowers > 0) // 避免除以零
            {
                // 假设 totalDamageDealt 是总伤害，totalResourcesSpentOnTowers 是建塔总花费
                Result = stats.totalDamageDealt / stats.totalResourcesSpentOnTowers;
            }
            else if (stats.totalDamageDealt > 0 && stats.totalTowers > 0) // 如果没有花费数据，用塔数量估算
            {
                 Result = stats.totalDamageDealt / (stats.totalTowers * 50f); // 假设每塔平均成本50，作为备用计算
            }
            else
            {
                Result = 0f; // 没有塔或没有伤害则效率为0
            }
            
            Debug.Log($"计算防御效率: {Result:F2}");
        }
    }

    /// <summary>
    /// 紧急维修所有生命值低于阈值的防御塔命令
    /// </summary>
    public class EmergencyRepairCommand : AbstractCommand
    {
        // 生命值百分比阈值，低于此值的塔将被修理 (例如0.3代表30%)
        public float HealthThreshold { get; set; } = 0.3f;
        // 命令执行结果：成功修理的防御塔数量
        public int Result { get; private set; }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var towers = defenseSystem.GetAllTowers();
            
            Result = 0; // 初始化计数器
            foreach (var tower in towers)
            {
                // 检查塔的当前生命值比例是否低于或等于阈值
                if (tower.GetHealthRatio() <= HealthThreshold)
                {
                    // 尝试修理该塔
                    if (defenseSystem.RepairTower(tower.Id))
                    {
                        Result++; // 修理成功则计数
                    }
                }
            }
            
            Debug.Log($"紧急维修完成 - 修理了 {Result} 个血量低于 {HealthThreshold:P0} 的防御塔");
        }
    }
}