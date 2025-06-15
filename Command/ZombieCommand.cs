using QFramework;
using UnityEngine;
using SurvivalGame.Model;
using SurvivalGame.GameSystem;
using MyGameNamespace;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 手动生成僵尸命令
    /// </summary>
    public class SpawnZombieCommand : AbstractCommand
    {
        private ZombieType mZombieType;
        private Vector2 mPosition;
        
        public SpawnZombieCommand(ZombieType type, Vector2 position)
        {
            mZombieType = type;
            mPosition = position;
        }
        
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            zombieSystem.SpawnZombie(mZombieType, mPosition);
        }
    }
    
    /// <summary>
    /// 生成僵尸群命令
    /// </summary>
    public class SpawnZombieHordeCommand : AbstractCommand
    {
        private ZombieThreatLevel mThreatLevel;
        private Vector2 mCenterPosition;
        
        public SpawnZombieHordeCommand(ZombieThreatLevel threatLevel, Vector2 centerPosition)
        {
            mThreatLevel = threatLevel;
            mCenterPosition = centerPosition;
        }
        
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            zombieSystem.SpawnZombieHorde(mThreatLevel, mCenterPosition);
        }
    }
    
    /// <summary>
    /// 攻击僵尸命令（用于防御塔等攻击僵尸）
    /// </summary>
    public class AttackZombieCommand : AbstractCommand
    {
        private string mZombieId;
        private float mDamage;
        private Vector2 mAttackPosition;
        
        public AttackZombieCommand(string zombieId, float damage, Vector2 attackPosition)
        {
            mZombieId = zombieId;
            mDamage = damage;
            mAttackPosition = attackPosition;
        }
        
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            zombieSystem.TakeDamageToZombie(mZombieId, mDamage);
            
            // 发送攻击事件用于特效显示
            this.SendEvent(new ZombieAttackedEvent
            {
                ZombieId = mZombieId,
                Damage = mDamage,
                AttackPosition = mAttackPosition
            });
        }
    }
    
    /// <summary>
    /// 批量攻击僵尸命令（用于范围攻击）
    /// </summary>
    public class AttackZombiesInAreaCommand : AbstractCommand
    {
        private Vector2 mCenterPosition;
        private float mRadius;
        private float mDamage;
        
        public AttackZombiesInAreaCommand(Vector2 centerPosition, float radius, float damage)
        {
            mCenterPosition = centerPosition;
            mRadius = radius;
            mDamage = damage;
        }
        
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            var zombiesInArea = zombieSystem.GetZombiesNearPosition(mCenterPosition, mRadius);
            
            int hitCount = 0;
            foreach (var zombie in zombiesInArea)
            {
                if (zombie.IsAlive)
                {
                    zombieSystem.TakeDamageToZombie(zombie.id, mDamage);
                    hitCount++;
                }
            }
            
            // 发送范围攻击事件
            this.SendEvent(new AreaAttackEvent
            {
                CenterPosition = mCenterPosition,
                Radius = mRadius,
                Damage = mDamage,
                HitCount = hitCount
            });
            
            Debug.Log($"范围攻击命中 {hitCount} 只僵尸");
        }
    }
    
    /// <summary>
    /// 清理死亡僵尸命令
    /// </summary>
    public class CleanupDeadZombiesCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            var zombieData = zombieSystem.GetZombieData();
            zombieData.CleanupDeadZombies();
            
            Debug.Log("清理死亡僵尸完成");
        }
    }
    
    /// <summary>
    /// 强制更新威胁等级命令
    /// </summary>
    public class UpdateThreatLevelCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            zombieSystem.UpdateThreatLevel();
            
            Debug.Log("威胁等级更新完成");
        }
    }
    
    /// <summary>
    /// 测试用：立即触发僵尸潮命令
    /// </summary>
    public class TriggerZombieWaveCommand : AbstractCommand
    {
        private ZombieThreatLevel mForceLevel;
        
        public TriggerZombieWaveCommand(ZombieThreatLevel forceLevel = ZombieThreatLevel.High)
        {
            mForceLevel = forceLevel;
        }
        
        protected override void OnExecute()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            
            // 在地图边缘随机生成3-5个僵尸群
            int hordeCount = Random.Range(3, 6);
            
            for (int i = 0; i < hordeCount; i++)
            {
                Vector2 spawnPosition = GetRandomEdgePosition();
                zombieSystem.SpawnZombieHorde(mForceLevel, spawnPosition);
            }
            
            Debug.Log($"触发僵尸潮！生成 {hordeCount} 个{mForceLevel}威胁等级僵尸群");
        }
        
        private Vector2 GetRandomEdgePosition()
        {
            // 在地图边缘随机选择位置
            float mapSize = 50f;
            int edge = Random.Range(0, 4); // 0:上 1:右 2:下 3:左
            
            switch (edge)
            {
                case 0: // 上边缘
                    return new Vector2(Random.Range(-mapSize, mapSize), mapSize);
                case 1: // 右边缘
                    return new Vector2(mapSize, Random.Range(-mapSize, mapSize));
                case 2: // 下边缘
                    return new Vector2(Random.Range(-mapSize, mapSize), -mapSize);
                case 3: // 左边缘
                    return new Vector2(-mapSize, Random.Range(-mapSize, mapSize));
                default:
                    return new Vector2(mapSize, 0);
            }
        }
    }
}

// === 僵尸攻击相关事件 ===
namespace MyGameNamespace
{
    public struct ZombieAttackedEvent
    {
        public string ZombieId;
        public float Damage;
        public Vector2 AttackPosition;
    }
    
    public struct AreaAttackEvent
    {
        public Vector2 CenterPosition;
        public float Radius;
        public float Damage;
        public int HitCount;
    }
}