using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 性别枚举
    /// </summary>
    public enum Gender
    {
        Male,       // 男性
        Female      // 女性
    }

    /// <summary>
    /// 幸存者工作类型
    /// </summary>
    public enum SurvivorJob
    {
        Idle,           // 空闲
        Production,     // 生产工作
        Defense,        // 防御工作
        Research,       // 研究工作
        Construction,   // 建造工作
        Medical,        // 医疗工作
        Exploration,    // 探索工作
        Leadership      // 领导工作
    }

    /// <summary>
    /// 幸存者职业
    /// </summary>
    public enum SurvivorProfession
    {
        Civilian,       // 平民 - 通用劳动者
        Soldier,        // 士兵 - 擅长战斗
        Engineer,       // 工程师 - 擅长建造和修理
        Doctor,         // 医生 - 擅长治疗
        Scientist,      // 科学家 - 擅长研究
        Scout,          // 侦察员 - 擅长探索
        Worker,         // 工人 - 擅长生产
        Guard           // 守卫 - 专门防御
    }

    /// <summary>
    /// 幸存者状态
    /// </summary>
    public enum SurvivorState
    {
        Idle,           // 空闲
        Working,        // 工作中
        Fighting,       // 战斗中
        Patrolling,     // 巡逻中
        Resting,        // 休息中
        Injured,        // 受伤
        Dead            // 死亡
    }

    /// <summary>
    /// 幸存者技能
    /// </summary>
    public enum SurvivorSkill
    {
        Combat,         // 战斗技能
        Construction,   // 建造技能
        Medicine,       // 医疗技能
        Research,       // 研究技能
        Leadership,     // 领导技能
        Stealth,        // 潜行技能
        Repair,         // 修理技能
        Gathering       // 采集技能
    }

    /// <summary>
    /// 新的幸存者数据结构 - 基于属性系统
    /// </summary>
    [Serializable]
    public class SurvivorData
    {
        // 基本信息
        public string Id;                           // 唯一ID
        public string Name;                         // 姓名
        public int Age;                             // 年龄
        public Gender Gender;                       // 性别
        
        // 状态属性
        public int Health;                          // 健康值 (0-100)
        public int Morale;                          // 士气 (0-100)
        public int Fatigue;                         // 疲劳度 (0-100)
        public int Hunger;                          // 饥饿度 (0-100)
        
        // 工作相关
        public SurvivorJob CurrentJob;              // 当前工作
        public string AssignedBuildingId;           // 分配的建筑ID
        public float WorkEfficiency;                // 工作效率
        public SurvivorState State;
        
        // 属性系统
        public List<SurvivorAttributeData> Attributes; // 7个核心属性
        
        // 位置信息
        public Vector3 Position;
        public Vector3 TargetPosition;
        
        // 经验和成长
        public int Experience;
        public int Level;
        
        // 装备
        public string Weapon;
        public string Armor;
        
        // 状态计时
        public float LastActionTime;
        public float WorkStartTime;
        
        public SurvivorData()
        {
            Id = System.Guid.NewGuid().ToString();
            Name = "未命名幸存者";
            Age = 25;
            Gender = Gender.Male;
            Health = 100;
            Morale = 75;
            Fatigue = 0;
            Hunger = 0;
            CurrentJob = SurvivorJob.Idle;
            State = SurvivorState.Idle;
            AssignedBuildingId = "";
            WorkEfficiency = 1.0f;
            Attributes = new List<SurvivorAttributeData>();
            Position = Vector3.zero;
            TargetPosition = Vector3.zero;
            Experience = 0;
            Level = 1;
            Weapon = "";
            Armor = "";
            WorkStartTime = 0f;
        }
        
        /// <summary>
        /// 获取指定属性的值
        /// </summary>
        public int GetAttributeValue(SurvivorAttributeType type)
        {
            var attr = Attributes.Find(a => a.Type == type);
            return attr?.Value ?? 20; // 默认值20
        }
        
        /// <summary>
        /// 设置属性值
        /// </summary>
        public void SetAttributeValue(SurvivorAttributeType type, int value)
        {
            var attr = Attributes.Find(a => a.Type == type);
            if (attr != null)
            {
                attr.Value = Mathf.Clamp(value, 0, 100);
            }
        }
        
        /// <summary>
        /// 添加属性经验
        /// </summary>
        public bool AddAttributeExperience(SurvivorAttributeType type, int exp)
        {
            var attr = Attributes.Find(a => a.Type == type);
            return attr?.AddExperience(exp) ?? false;
        }
        
        /// <summary>
        /// 幸存者是否存活
        /// </summary>
        public bool IsAlive => Health > 0;
        
        /// <summary>
        /// 幸存者是否可以工作
        /// </summary>
        public bool CanWork => IsAlive && Health > 30 && Morale > 20 && Fatigue < 80;
        
        /// <summary>
        /// 幸存者是否可以战斗
        /// </summary>
        public bool CanFight => IsAlive && Health > 50 && Morale > 30 && Fatigue < 70;
        
        /// <summary>
        /// 获取综合工作效率
        /// </summary>
        public float GetTotalWorkEfficiency()
        {
            float baseEfficiency = WorkEfficiency;
            
            // 健康影响
            float healthModifier = Health / 100f;
            
            // 士气影响
            float moraleModifier = Morale / 100f;
            
            // 疲劳影响
            float fatigueModifier = (100f - Fatigue) / 100f;
            
            return baseEfficiency * healthModifier * moraleModifier * fatigueModifier;
        }
        
        /// <summary>
        /// 设置幸存者状态
        /// </summary>
        public void SetState(SurvivorState newState)
        {
            var previousState = State;
            State = newState;
            
            // 状态切换时的逻辑处理
            OnStateChanged(previousState, newState);
        }
        
        /// <summary>
        /// 状态改变时的处理
        /// </summary>
        private void OnStateChanged(SurvivorState previousState, SurvivorState newState)
        {
            // 记录状态切换时间
            LastActionTime = Time.time;
            
            // 根据新状态调整工作分配
            switch (newState)
            {
                case SurvivorState.Working:
                    if (CurrentJob == SurvivorJob.Idle)
                    {
                        // 如果没有指定工作，设置为生产工作
                        CurrentJob = SurvivorJob.Production;
                    }
                    WorkStartTime = Time.time;
                    break;
                    
                case SurvivorState.Idle:
                    CurrentJob = SurvivorJob.Idle;
                    AssignedBuildingId = "";
                    break;
                    
                case SurvivorState.Injured:
                    // 受伤时停止所有工作
                    CurrentJob = SurvivorJob.Idle;
                    AssignedBuildingId = "";
                    break;
                    
                case SurvivorState.Dead:
                    // 死亡时清除所有分配
                    CurrentJob = SurvivorJob.Idle;
                    AssignedBuildingId = "";
                    Health = 0;
                    break;
                    
                case SurvivorState.Resting:
                    // 休息时停止工作但保留分配
                    CurrentJob = SurvivorJob.Idle;
                    break;
            }
        }
        
        /// <summary>
        /// 分配工作
        /// </summary>
        public bool AssignJob(SurvivorJob job, string buildingId = "")
        {
            // 检查是否可以工作
            if (!CanWork)
            {
                return false;
            }
            
            CurrentJob = job;
            AssignedBuildingId = buildingId;
            
            // 根据工作类型设置状态
            if (job != SurvivorJob.Idle)
            {
                SetState(SurvivorState.Working);
            }
            else
            {
                SetState(SurvivorState.Idle);
            }
            
            return true;
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage)
        {
            Health = Mathf.Max(0, Health - damage);
            Morale = Mathf.Max(0, Morale - damage / 2); // 受伤影响士气
            
            if (Health <= 0)
            {
                SetState(SurvivorState.Dead);
            }
            else if (Health <= 30)
            {
                SetState(SurvivorState.Injured);
            }
        }
        
        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(int amount)
        {
            Health = Mathf.Min(100, Health + amount);
            
            // 如果从受伤状态恢复
            if (State == SurvivorState.Injured && Health > 30)
            {
                SetState(SurvivorState.Idle);
            }
        }
        
        /// <summary>
        /// 休息
        /// </summary>
        public void Rest()
        {
            if (State != SurvivorState.Dead && State != SurvivorState.Injured)
            {
                SetState(SurvivorState.Resting);
            }
        }
        
        /// <summary>
        /// 开始战斗
        /// </summary>
        public bool StartFighting()
        {
            if (!CanFight)
            {
                return false;
            }
            
            SetState(SurvivorState.Fighting);
            return true;
        }
        
        /// <summary>
        /// 开始巡逻
        /// </summary>
        public bool StartPatrolling()
        {
            if (!CanWork)
            {
                return false;
            }
            
            CurrentJob = SurvivorJob.Defense;
            SetState(SurvivorState.Patrolling);
            return true;
        }
        
        /// <summary>
        /// 更新幸存者状态（每帧调用）
        /// </summary>
        public void UpdateState()
        {
            // 更新疲劳度
            if (State == SurvivorState.Working || State == SurvivorState.Fighting || State == SurvivorState.Patrolling)
            {
                float workTime = Time.time - WorkStartTime;
                Fatigue = Mathf.Min(100, Fatigue + Mathf.RoundToInt(workTime * 0.1f)); // 工作增加疲劳
            }
            else if (State == SurvivorState.Resting)
            {
                Fatigue = Mathf.Max(0, Fatigue - Mathf.RoundToInt(Time.deltaTime * 10f)); // 休息减少疲劳
            }
            
            // 更新饥饿度
            Hunger = Mathf.Min(100, Hunger + Mathf.RoundToInt(Time.deltaTime * 0.5f)); // 持续增加饥饿
            
            // 检查是否需要自动切换状态
            CheckAutoStateTransition();
        }
        
        /// <summary>
        /// 检查自动状态转换
        /// </summary>
        private void CheckAutoStateTransition()
        {
            // 如果疲劳过高，自动休息
            if (Fatigue >= 90 && State == SurvivorState.Working)
            {
                Rest();
                return;
            }
            
            // 如果健康过低，自动设为受伤
            if (Health <= 20 && State != SurvivorState.Injured && State != SurvivorState.Dead)
            {
                SetState(SurvivorState.Injured);
                return;
            }
            
            // 如果休息充分，可以重新工作
            if (Fatigue <= 20 && State == SurvivorState.Resting && CurrentJob != SurvivorJob.Idle)
            {
                SetState(SurvivorState.Working);
                return;
            }
            
            // 如果没有工作且不在特殊状态，设为空闲
            if (CurrentJob == SurvivorJob.Idle && State == SurvivorState.Working)
            {
                SetState(SurvivorState.Idle);
            }
        }
    }

    // 保留原有的SurvivorData类作为兼容
    [Serializable]
    public class LegacySurvivorData
    {
        public string id;
        public string name;
        public SurvivorProfession profession;
        public SurvivorState state;
        public int level;
        
        public int Skill;                 // 技能(0-100)
        public JobType Job;               // 当前工作
        public float WorkEfficiency;      // 工作效率
        
        // 基本属性
        public float maxHealth;
        public float currentHealth;
        public float attack;
        public float defense;
        public float speed;
        public float morale;
        
        // 位置信息
        public Vector3 position;
        public Vector3 targetPosition;
        
        // 工作分配
        public string assignedBuildingId; // 分配的建筑ID
        public string currentTask;        // 当前任务
        
        // 技能等级
        public Dictionary<SurvivorSkill, int> skills;
        
        // 装备
        public string weapon;
        public string armor;
        
        // 状态计时
        public float lastActionTime;
        public float workStartTime;
        public float restNeeded;
        
        // 经验和成长
        public int experience;
        public int skillPoints;
        
        public LegacySurvivorData()
        {
            id = System.Guid.NewGuid().ToString();
            state = SurvivorState.Idle;
            level = 1;
            skills = new Dictionary<SurvivorSkill, int>();
            
            // 初始化基本属性
            InitializeBasicStats();
            
            // 初始化技能
            InitializeSkills();
            
            currentHealth = maxHealth;
            morale = 100f;
        }
        
        private void InitializeBasicStats()
        {
            maxHealth = 100f;
            attack = 10f;
            defense = 5f;
            speed = 2f;
            experience = 0;
            skillPoints = 0;
        }
        
        private void InitializeSkills()
        {
            // 所有技能初始等级为1
            foreach (SurvivorSkill skill in Enum.GetValues(typeof(SurvivorSkill)))
            {
                skills[skill] = 1;
            }
            
            // 根据职业提升相关技能
            ApplyProfessionBonuses();
        }
        
        public void Initialize()
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "Survivor_" + ((int)Time.time).ToString();
            }
            lastActionTime = Time.time;
        }
        
        private void ApplyProfessionBonuses()
        {
            switch (profession)
            {
                case SurvivorProfession.Soldier:
                    skills[SurvivorSkill.Combat] = 3;
                    skills[SurvivorSkill.Leadership] = 2;
                    attack += 5f;
                    defense += 3f;
                    break;
                    
                case SurvivorProfession.Engineer:
                    skills[SurvivorSkill.Construction] = 3;
                    skills[SurvivorSkill.Repair] = 3;
                    break;
                    
                case SurvivorProfession.Doctor:
                    skills[SurvivorSkill.Medicine] = 3;
                    maxHealth += 20f;
                    break;
                    
                case SurvivorProfession.Scientist:
                    skills[SurvivorSkill.Research] = 3;
                    break;
                    
                case SurvivorProfession.Scout:
                    skills[SurvivorSkill.Stealth] = 3;
                    skills[SurvivorSkill.Gathering] = 2;
                    speed += 1f;
                    break;
                    
                case SurvivorProfession.Worker:
                    skills[SurvivorSkill.Gathering] = 2;
                    skills[SurvivorSkill.Construction] = 2;
                    break;
                    
                case SurvivorProfession.Guard:
                    skills[SurvivorSkill.Combat] = 2;
                    defense += 3f;
                    break;
            }
        }
        
        public void GenerateRandomName()
        {
            // 随机名称生成逻辑
            name = "RandomName" + UnityEngine.Random.Range(0, 1000).ToString();
        }
        
        /// <summary>
        /// 创建指定职业的幸存者
        /// </summary>
        public static LegacySurvivorData CreateSurvivor(SurvivorProfession profession, Vector3 position)
        {
            var survivor = new LegacySurvivorData
            {
                profession = profession,
                position = position,
                targetPosition = position
            };
            
            // 重新应用职业加成（因为构造函数中profession可能还是默认值）
            survivor.ApplyProfessionBonuses();
            
            return survivor;
        }
        
        /// <summary>
        /// 幸存者是否存活
        /// </summary>
        public bool IsAlive => state != SurvivorState.Dead && currentHealth > 0;
        
        /// <summary>
        /// 幸存者是否可以工作
        /// </summary>
        public bool CanWork => IsAlive && state != SurvivorState.Injured && morale > 20f;
        
        /// <summary>
        /// 幸存者是否可以战斗
        /// </summary>
        public bool CanFight => IsAlive && state != SurvivorState.Injured && currentHealth > maxHealth * 0.3f;
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            morale = Mathf.Max(0, morale - damage * 0.1f);
            
            if (currentHealth <= 0)
            {
                state = SurvivorState.Dead;
            }
            else if (currentHealth < maxHealth * 0.5f)
            {
                state = SurvivorState.Injured;
            }
        }
        
        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            
            if (currentHealth > maxHealth * 0.5f && state == SurvivorState.Injured)
            {
                state = SurvivorState.Idle;
            }
        }
        
        /// <summary>
        /// 增加经验
        /// </summary>
        public void AddExperience(int exp)
        {
            experience += exp;
            
            // 检查是否可以升级
            int requiredExp = level * 100;
            if (experience >= requiredExp)
            {
                LevelUp();
            }
        }
        
        /// <summary>
        /// 升级
        /// </summary>
        private void LevelUp()
        {
            level++;
            experience = 0;
            skillPoints += 2;
            
            // 提升基本属性
            maxHealth += 10f;
            attack += 2f;
            defense += 1f;
            
            // 恢复生命值
            currentHealth = maxHealth;
            morale = Mathf.Min(100f, morale + 20f);
            
            Debug.Log($"幸存者 {name} 升级到 {level} 级!");
        }
        
        /// <summary>
        /// 提升技能
        /// </summary>
        public bool UpgradeSkill(SurvivorSkill skill)
        {
            if (skillPoints <= 0) return false;
            
            skills[skill]++;
            skillPoints--;
            
            Debug.Log($"幸存者 {name} 的 {skill} 技能提升到 {skills[skill]} 级");
            return true;
        }
        
        /// <summary>
        /// 获取技能等级
        /// </summary>
        public int GetSkillLevel(SurvivorSkill skill)
        {
            return skills.ContainsKey(skill) ? skills[skill] : 1;
        }
        
        /// <summary>
        /// 获取工作效率
        /// </summary>
        public float GetWorkEfficiency()
        {
            float baseEfficiency = 1f;
            
            // 士气影响
            float moraleBonus = morale / 100f;
            
            // 健康状态影响
            float healthBonus = currentHealth / maxHealth;
            
            // 等级影响
            float levelBonus = 1f + (level - 1) * 0.1f;
            
            return baseEfficiency * moraleBonus * healthBonus * levelBonus;
        }
        
        /// <summary>
        /// 获取战斗力
        /// </summary>
        public float GetCombatPower()
        {
            float basePower = attack * GetSkillLevel(SurvivorSkill.Combat);
            float healthModifier = currentHealth / maxHealth;
            float moraleModifier = morale / 100f;
            
            return basePower * healthModifier * moraleModifier;
        }
    }

    
    
    /// <summary>
    /// 幸存者系统数据
    /// </summary>
    [Serializable]
    public class SurvivorSystemData
    {
        // 幸存者管理
        public Dictionary<string, SurvivorData> survivors;
        public int maxSurvivors;
        public int totalSurvivorsRecruited;
        
        // 工作分配
        public Dictionary<string, List<string>> buildingWorkers; // 建筑ID -> 幸存者ID列表
        public Dictionary<SurvivorProfession, int> professionCounts;
        
        // 系统状态
        public float lastRecruitTime;
        public float nextRecruitTime;
        public float recruitCooldown;
        
        // 团队士气
        public float teamMorale;
        public int totalDeaths;
        
        public SurvivorSystemData()
        {
            survivors = new Dictionary<string, SurvivorData>();
            buildingWorkers = new Dictionary<string, List<string>>();
            professionCounts = new Dictionary<SurvivorProfession, int>();
            
            maxSurvivors = 20;
            totalSurvivorsRecruited = 0;
            totalDeaths = 0;
            teamMorale = 100f;
            recruitCooldown = 300f; // 5分钟招募冷却
            
            // 初始化职业计数
            foreach (SurvivorProfession profession in Enum.GetValues(typeof(SurvivorProfession)))
            {
                professionCounts[profession] = 0;
            }
        }
        
        /// <summary>
        /// 获取存活幸存者数量
        /// </summary>
        public int GetAliveSurvivorCount()
        {
            int count = 0;
            foreach (var survivor in survivors.Values)
            {
                if (survivor.IsAlive)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// 获取可工作幸存者数量
        /// </summary>
        public int GetAvailableWorkerCount()
        {
            int count = 0;
            foreach (var survivor in survivors.Values)
            {
                if (survivor.CanWork && survivor.CurrentJob == SurvivorJob.Idle)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// 获取可战斗幸存者数量
        /// </summary>
        public int GetCombatReadySurvivorCount()
        {
            int count = 0;
            foreach (var survivor in survivors.Values)
            {
                if (survivor.CanFight)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// 更新团队士气
        /// </summary>
        public void UpdateTeamMorale()
        {
            if (survivors.Count == 0)
            {
                teamMorale = 100f;
                return;
            }
            
            float totalMorale = 0f;
            int aliveCount = 0;
            
            foreach (var survivor in survivors.Values)
            {
                if (survivor.IsAlive)
                {
                    totalMorale += survivor.Morale;
                    aliveCount++;
                }
            }
            
            teamMorale = aliveCount > 0 ? totalMorale / aliveCount : 0f;
        }
    }


} 