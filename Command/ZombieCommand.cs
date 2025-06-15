// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：ZombieCommand.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件包含与僵尸（敌人）相关的各种命令，
//     例如生成僵尸、攻击僵尸、清理死亡僵尸以及管理僵尸潮等。
//     同时定义了与僵尸攻击相关的事件。
// ==============================================================================

using QFramework;
using UnityEngine;
using SurvivalGame.Model;     // 包含 ZombieType, ZombieThreatLevel 等模型定义
using SurvivalGame.GameSystem; // 包含 IZombieSystem 接口定义
using MyGameNamespace;        // 包含 ZombieAttackedEvent, AreaAttackEvent 事件定义

namespace SurvivalGame.Command
{
    /// <summary>
    /// 手动生成单个僵尸命令
    /// </summary>
    public class SpawnZombieCommand : AbstractCommand
    {
        // 要生成的僵尸类型
        private ZombieType mZombieType;
        // 僵尸生成的目标位置
        private Vector2 mPosition;
        
        /// <summary>
        /// SpawnZombieCommand 构造函数
        /// </summary>
        /// <param name="type">要生成的僵尸的类型</param>
        /// <param name="position">僵尸生成的位置</param>
        public SpawnZombieCommand(ZombieType type, Vector2 position)
        {
            mZombieType = type;
            mPosition = position;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            // 获取僵尸系统实例
            var zombieSystem = this.GetSystem<IZombieSystem>();
            // 调用系统方法生成僵尸
            zombieSystem.SpawnZombie(mZombieType, mPosition);
            Debug.Log($"[SpawnZombieCommand] 已请求在位置 {mPosition} 生成类型为 {mZombieType} 的僵尸。");
        }
    }
    
    /// <summary>
    /// 根据威胁等级在指定中心点生成僵尸群的命令
    /// </summary>
    public class SpawnZombieHordeCommand : AbstractCommand
    {
        // 僵尸群的威胁等级
        private ZombieThreatLevel mThreatLevel;
        // 僵尸群生成的中心位置
        private Vector2 mCenterPosition;
        
        /// <summary>
        /// SpawnZombieHordeCommand 构造函数
        /// </summary>
        /// <param name="threatLevel">僵尸群的威胁等级</param>
        /// <param name="centerPosition">僵尸群生成的中心位置</param>
        public SpawnZombieHordeCommand(ZombieThreatLevel threatLevel, Vector2 centerPosition)
        {
            mThreatLevel = threatLevel;
            mCenterPosition = centerPosition;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            // 调用系统方法生成僵尸群
            zombieSystem.SpawnZombieHorde(mThreatLevel, mCenterPosition);
            Debug.Log($"[SpawnZombieHordeCommand] 已请求在位置 {mCenterPosition} 生成威胁等级为 {mThreatLevel} 的僵尸群。");
        }
    }
    
    /// <summary>
    /// 对单个僵尸造成伤害的命令（例如：由防御塔或玩家单体技能触发）
    /// </summary>
    public class AttackZombieCommand : AbstractCommand
    {
        // 被攻击僵尸的唯一ID
        private string mZombieId;
        // 对僵尸造成的伤害值
        private float mDamage;
        // 攻击发生的位置（可用于特效或弹道起点）
        private Vector2 mAttackPosition; // 注意：此参数当前未直接传递给IZombieSystem的TakeDamageToZombie，主要用于事件
        
        /// <summary>
        /// AttackZombieCommand 构造函数
        /// </summary>
        /// <param name="zombieId">目标僵尸的ID</param>
        /// <param name="damage">造成的伤害量</param>
        /// <param name="attackPosition">攻击发生的位置或来源点</param>
        public AttackZombieCommand(string zombieId, float damage, Vector2 attackPosition)
        {
            mZombieId = zombieId;
            mDamage = damage;
            mAttackPosition = attackPosition;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            // 对指定ID的僵尸造成伤害
            zombieSystem.TakeDamageToZombie(mZombieId, mDamage);
            
            // 发送僵尸被攻击事件，可用于触发伤害数字显示、受击特效、声音等
            this.SendEvent(new ZombieAttackedEvent
            {
                ZombieId = mZombieId,
                Damage = mDamage,
                AttackPosition = mAttackPosition // 将攻击位置信息传递给事件监听者
            });
            Debug.Log($"[AttackZombieCommand] 僵尸 {mZombieId} 受到 {mDamage}点伤害。攻击事件已发送。");
        }
    }
    
    /// <summary>
    /// 对指定区域内所有僵尸造成伤害的命令（例如：范围技能、爆炸等）
    /// </summary>
    public class AttackZombiesInAreaCommand : AbstractCommand
    {
        // 范围攻击的中心位置
        private Vector2 mCenterPosition;
        // 攻击范围的半径
        private float mRadius;
        // 对范围内每个僵尸造成的伤害值
        private float mDamage;
        
        /// <summary>
        /// AttackZombiesInAreaCommand 构造函数
        /// </summary>
        /// <param name="centerPosition">范围攻击的中心点</param>
        /// <param name="radius">攻击半径</param>
        /// <param name="damage">对范围内每个目标造成的伤害</param>
        public AttackZombiesInAreaCommand(Vector2 centerPosition, float radius, float damage)
        {
            mCenterPosition = centerPosition;
            mRadius = radius;
            mDamage = damage;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            // 获取指定区域内的所有僵尸
            var zombiesInArea = zombieSystem.GetZombiesNearPosition(mCenterPosition, mRadius);
            
            int hitCount = 0; // 统计成功命中的僵尸数量
            foreach (var zombie in zombiesInArea)
            {
                // 确保只对存活的僵尸造成伤害
                if (zombie.IsAlive) // 假设ZombieData有IsAlive属性
                {
                    zombieSystem.TakeDamageToZombie(zombie.id, mDamage); // 假设ZombieData有id属性
                    hitCount++;
                }
            }
            
            // 发送范围攻击事件，可用于触发范围效果、统计等
            this.SendEvent(new AreaAttackEvent
            {
                CenterPosition = mCenterPosition,
                Radius = mRadius,
                Damage = mDamage,
                HitCount = hitCount // 传递命中数量
            });
            
            Debug.Log($"[AttackZombiesInAreaCommand] 范围攻击执行完毕，中心: {mCenterPosition}, 半径: {mRadius}, 伤害: {mDamage}。命中 {hitCount} 只僵尸。事件已发送。");
        }
    }
    
    /// <summary>
    /// 清理战场上所有已死亡僵尸实体的命令
    /// </summary>
    public class CleanupDeadZombiesCommand : AbstractCommand
    {
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            // 获取僵尸数据容器或管理器
            var zombieData = zombieSystem.GetZombieData(); // 假设此方法返回一个可操作僵尸集合的对象
            // 调用其清理方法
            zombieData.CleanupDeadZombies(); // 假设此方法移除已标记为死亡的僵尸
            
            Debug.Log("[CleanupDeadZombiesCommand] 清理死亡僵尸操作已请求完成。");
        }
    }
    
    /// <summary>
    /// 强制游戏系统更新当前威胁等级的命令
    /// （威胁等级可能基于游戏时间、玩家进度等动态变化）
    /// </summary>
    public class UpdateThreatLevelCommand : AbstractCommand
    {
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            // 调用系统方法更新威胁等级
            zombieSystem.UpdateThreatLevel();
            
            Debug.Log("[UpdateThreatLevelCommand] 威胁等级更新已请求完成。");
        }
    }
    
    /// <summary>
    /// 测试用命令：立即在地图边缘随机位置触发一波指定威胁等级的僵尸潮
    /// </summary>
    public class TriggerZombieWaveCommand : AbstractCommand
    {
        // 强制指定的僵尸潮威胁等级
        private ZombieThreatLevel mForceLevel;
        
        /// <summary>
        /// TriggerZombieWaveCommand 构造函数
        /// </summary>
        /// <param name="forceLevel">要触发的僵尸潮的威胁等级，默认为高威胁</param>
        public TriggerZombieWaveCommand(ZombieThreatLevel forceLevel = ZombieThreatLevel.High)
        {
            mForceLevel = forceLevel;
        }
        
        // 执行命令逻辑
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            
            // 随机决定生成3到5个僵尸群
            int hordeCount = Random.Range(3, 6); // Random.Range对于整数是[min, max-1]
            
            for (int i = 0; i < hordeCount; i++)
            {
                // 为每个僵尸群获取一个随机的地图边缘位置
                Vector2 spawnPosition = GetRandomEdgePosition();
                // 使用获取的僵尸系统生成指定威胁等级的僵尸群
                zombieSystem.SpawnZombieHorde(mForceLevel, spawnPosition);
            }
            
            Debug.Log($"[TriggerZombieWaveCommand] [调试] 触发僵尸潮！已请求生成 {hordeCount} 个威胁等级为 {mForceLevel} 的僵尸群。");
        }
        
        /// <summary>
        /// 获取地图边缘的一个随机位置。
        /// </summary>
        /// <returns>地图边缘的随机二维坐标</returns>
        private Vector2 GetRandomEdgePosition()
        {
            // 假设地图大小为 50x50 (中心为0,0, 范围是 -mapSize 到 +mapSize)
            // TODO: 实际地图尺寸应从配置或游戏模型中获取
            float mapSize = 50f;
            int edge = Random.Range(0, 4); // 随机选择一条边：0=上, 1=右, 2=下, 3=左

            float randomCoord = Random.Range(-mapSize, mapSize); // 边上的随机坐标
            
            switch (edge)
            {
                case 0: // 上边缘 (y固定为mapSize, x在[-mapSize, mapSize]间随机)
                    return new Vector2(randomCoord, mapSize);
                case 1: // 右边缘 (x固定为mapSize, y在[-mapSize, mapSize]间随机)
                    return new Vector2(mapSize, randomCoord);
                case 2: // 下边缘 (y固定为-mapSize, x在[-mapSize, mapSize]间随机)
                    return new Vector2(randomCoord, -mapSize);
                case 3: // 左边缘 (x固定为-mapSize, y在[-mapSize, mapSize]间随机)
                    return new Vector2(-mapSize, randomCoord);
                default: // 理论上不会执行到这里
                    return new Vector2(mapSize, 0); // 默认返回右边缘中点
            }
        }
    }
}

// === 僵尸攻击相关事件 ===
// (通常建议将事件定义在专门的事件文件或共享的事件命名空间中)
namespace MyGameNamespace
{
    /// <summary>
    /// 单个僵尸被攻击事件结构体。
    /// 当一个僵尸受到伤害时（例如被防御塔击中），此事件被触发。
    /// </summary>
    public struct ZombieAttackedEvent
    {
        /// <summary>
        /// 被攻击僵尸的唯一ID。
        /// </summary>
        public string ZombieId;
        /// <summary>
        /// 本次攻击对僵尸造成的伤害量。
        /// </summary>
        public float Damage;
        /// <summary>
        /// 攻击发生的位置或来源点。
        /// 可用于显示伤害数字、粒子效果等。
        /// </summary>
        public Vector2 AttackPosition;
    }
    
    /// <summary>
    /// 范围攻击事件结构体。
    /// 当执行一次范围攻击（如爆炸、AOE技能）时，此事件被触发。
    /// </summary>
    public struct AreaAttackEvent
    {
        /// <summary>
        /// 范围攻击的中心位置。
        /// </summary>
        public Vector2 CenterPosition;
        /// <summary>
        /// 范围攻击的半径。
        /// </summary>
        public float Radius;
        /// <summary>
        /// 本次范围攻击对每个有效目标造成的伤害量。
        /// </summary>
        public float Damage;
        /// <summary>
        /// 本次范围攻击实际命中的目标数量。
        /// </summary>
        public int HitCount;
    }
}