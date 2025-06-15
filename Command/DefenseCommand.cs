using System.Collections.Generic;
using UnityEngine;
using QFramework;
using SurvivalGame.GameSystem;
using SurvivalGame;
using SurvivalGame.Model;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 建造防御塔命令
    /// </summary>
    public class BuildTowerCommand : AbstractCommand
    {
        public Vector3 Position { get; set; }
        public TowerType TowerType { get; set; }
        public int Result { get; private set; } = -1;
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.BuildTower(Position, TowerType);
            
            if (Result > 0)
            {
                Debug.Log($"建造防御塔成功: {TowerType} at {Position}，ID: {Result}");
            }
            else
            {
                Debug.LogWarning($"建造防御塔失败: {TowerType} at {Position}");
            }
        }
    }

    /// <summary>
    /// 升级防御塔命令
    /// </summary>
    public class UpgradeTowerCommand : AbstractCommand
    {
        public int TowerId { get; set; }
        public bool Result { get; private set; }
        
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
        public int TowerId { get; set; }
        public bool Result { get; private set; }
        
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
        public int TowerId { get; set; }
        public bool Result { get; private set; }
        
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
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            defenseSystem.StartCombat();
            
            Debug.Log("开始防御战斗");
        }
    }

    /// <summary>
    /// 结束防御战斗命令
    /// </summary>
    public class EndDefenseCombatCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            defenseSystem.EndCombat();
            
            Debug.Log("结束防御战斗");
        }
    }

    /// <summary>
    /// 获取防御统计命令
    /// </summary>
    public class GetDefenseStatsCommand : AbstractCommand
    {
        public DefenseStats Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.GetDefenseStats();
            
            Debug.Log($"获取防御统计 - 防御塔: {Result.totalTowers}, 击杀数: {Result.totalZombiesKilled}");
        }
    }

    /// <summary>
    /// 获取所有防御塔命令
    /// </summary>
    public class GetAllTowersCommand : AbstractCommand
    {
        public List<TowerData> Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.GetAllTowers();
            
            Debug.Log($"获取所有防御塔 - 共 {Result.Count} 个");
        }
    }

    /// <summary>
    /// 获取范围内防御塔命令
    /// </summary>
    public class GetTowersInRangeCommand : AbstractCommand
    {
        public Vector3 Center { get; set; }
        public float Range { get; set; }
        public List<TowerData> Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.GetTowersInRange(Center, Range);
            
            Debug.Log($"获取范围内防御塔 - 中心: {Center}, 范围: {Range}, 找到: {Result.Count} 个");
        }
    }

    /// <summary>
    /// 获取单个防御塔信息命令
    /// </summary>
    public class GetTowerInfoCommand : AbstractCommand
    {
        public int TowerId { get; set; }
        public TowerData Result { get; private set; }
        
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
    /// 批量建造防御塔命令（调试用）
    /// </summary>
    public class BatchBuildTowersCommand : AbstractCommand
    {
        public List<Vector3> Positions { get; set; }
        public TowerType TowerType { get; set; }
        public List<int> Results { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Results = new List<int>();
            
            if (Positions != null)
            {
                foreach (var position in Positions)
                {
                    int towerId = defenseSystem.BuildTower(position, TowerType);
                    Results.Add(towerId);
                }
                
                int successCount = Results.FindAll(id => id > 0).Count;
                Debug.Log($"批量建造防御塔 - 尝试: {Positions.Count}, 成功: {successCount}");
            }
        }
    }

    /// <summary>
    /// 修理所有防御塔命令
    /// </summary>
    public class RepairAllTowersCommand : AbstractCommand
    {
        public int Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var towers = defenseSystem.GetAllTowers();
            
            Result = 0;
            foreach (var tower in towers)
            {
                if (tower.Health < tower.MaxHealth)
                {
                    if (defenseSystem.RepairTower(tower.Id))
                    {
                        Result++;
                    }
                }
            }
            
            Debug.Log($"修理所有防御塔 - 成功修理: {Result} 个");
        }
    }

    /// <summary>
    /// 升级所有防御塔命令
    /// </summary>
    public class UpgradeAllTowersCommand : AbstractCommand
    {
        public int Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var towers = defenseSystem.GetAllTowers();
            
            Result = 0;
            foreach (var tower in towers)
            {
                if (tower.Level < 5) // 假设最大等级为5
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
    /// 出售所有防御塔命令（调试用）
    /// </summary>
    public class SellAllTowersCommand : AbstractCommand
    {
        public int Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var towers = defenseSystem.GetAllTowers();
            
            Result = 0;
            foreach (var tower in towers)
            {
                if (defenseSystem.SellTower(tower.Id))
                {
                    Result++;
                }
            }
            
            Debug.Log($"出售所有防御塔 - 成功出售: {Result} 个");
        }
    }

    /// <summary>
    /// 设置防御塔目标命令
    /// </summary>
    public class SetTowerTargetCommand : AbstractCommand
    {
        public int TowerId { get; set; }
        public ZombieData Target { get; set; }
        public bool Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var tower = defenseSystem.GetTower(TowerId);
            
            if (tower != null)
            {
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
                Debug.LogWarning($"未找到防御塔: ID {TowerId}");
            }
        }
    }

    /// <summary>
    /// 获取最佳目标命令
    /// </summary>
    public class FindBestTargetCommand : AbstractCommand
    {
        public Vector3 TowerPosition { get; set; }
        public float Range { get; set; }
        public TowerType TowerType { get; set; }
        public ZombieData Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.FindBestTarget(TowerPosition, Range, TowerType);
            
            if (Result != null)
            {
                Debug.Log($"找到最佳目标: 僵尸 {Result.id} at {Result.position}");
            }
            else
            {
                Debug.Log($"未找到目标 - 位置: {TowerPosition}, 范围: {Range}");
            }
        }
    }

    /// <summary>
    /// 获取范围内目标命令
    /// </summary>
    public class FindTargetsInRangeCommand : AbstractCommand
    {
        public Vector3 Center { get; set; }
        public float Range { get; set; }
        public List<ZombieData> Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.FindTargetsInRange(Center, Range);
            
            Debug.Log($"范围内目标搜索 - 中心: {Center}, 范围: {Range}, 找到: {Result.Count} 个");
        }
    }

    /// <summary>
    /// 获取所有投射物命令
    /// </summary>
    public class GetAllProjectilesCommand : AbstractCommand
    {
        public List<ProjectileData> Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            Result = defenseSystem.GetAllProjectiles();
            
            Debug.Log($"获取所有投射物 - 共 {Result.Count} 个");
        }
    }

    /// <summary>
    /// 快速建造防御塔命令（调试用）
    /// </summary>
    public class QuickBuildTowerCommand : AbstractCommand
    {
        public Vector3 Position { get; set; }
        public TowerType TowerType { get; set; }
        public int Level { get; set; } = 1;
        public int Result { get; private set; } = -1;
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            
            // 先建造防御塔
            Result = defenseSystem.BuildTower(Position, TowerType);
            
            if (Result > 0 && Level > 1)
            {
                // 然后升级到指定等级
                for (int i = 1; i < Level; i++)
                {
                    defenseSystem.UpgradeTower(Result);
                }
            }
            
            if (Result > 0)
            {
                Debug.Log($"快速建造防御塔成功: {TowerType} 等级 {Level} at {Position}");
            }
            else
            {
                Debug.LogWarning($"快速建造防御塔失败: {TowerType} at {Position}");
            }
        }
    }

    /// <summary>
    /// 重置防御系统命令（调试用）
    /// </summary>
    public class ResetDefenseSystemCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            
            // 移除所有防御塔
            var towers = defenseSystem.GetAllTowers();
            foreach (var tower in towers)
            {
                defenseSystem.SellTower(tower.Id);
            }
            
            // 结束战斗状态
            defenseSystem.EndCombat();
            
            Debug.Log("重置防御系统完成");
        }
    }

    /// <summary>
    /// 获取防御效率命令
    /// </summary>
    public class GetDefenseEfficiencyCommand : AbstractCommand
    {
        public float Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var stats = defenseSystem.GetDefenseStats();
            
            // 计算防御效率 = 总伤害 / 总花费（简化计算）
            if (stats.totalTowers > 0)
            {
                Result = stats.totalDamageDealt / (stats.totalTowers * 50f); // 假设每塔平均成本50
            }
            else
            {
                Result = 0f;
            }
            
            Debug.Log($"计算防御效率: {Result:F2}");
        }
    }

    /// <summary>
    /// 紧急维修命令
    /// </summary>
    public class EmergencyRepairCommand : AbstractCommand
    {
        public float HealthThreshold { get; set; } = 0.3f;
        public int Result { get; private set; }
        
        protected override void OnExecute()
        {
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            var towers = defenseSystem.GetAllTowers();
            
            Result = 0;
            foreach (var tower in towers)
            {
                if (tower.GetHealthRatio() <= HealthThreshold)
                {
                    if (defenseSystem.RepairTower(tower.Id))
                    {
                        Result++;
                    }
                }
            }
            
            Debug.Log($"紧急维修完成 - 修理了 {Result} 个血量低于 {HealthThreshold:P0} 的防御塔");
        }
    }
} 