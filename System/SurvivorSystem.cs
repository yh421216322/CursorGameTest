using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
using SurvivalGame.Model;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 幸存者系统接口
    /// 功能：管理幸存者的生成、分配、技能升级等
    /// </summary>
    public interface ISurvivorSystem : QFISystem
    {
        // 幸存者管理
        bool AddSurvivor(SurvivorProfession profession, Vector3 position);
        bool RemoveSurvivor(string survivorId);
        SurvivorData GetSurvivor(string survivorId);
        List<SurvivorData> GetAllSurvivors();
        List<SurvivorData> GetSurvivorsByJob(SurvivorJob job);
        List<SurvivorData> GetAvailableWorkers();
        
        /// <summary>
        /// 分配幸存者到建筑
        /// </summary>
        /// <param name="survivorId">幸存者id</param>
        /// <param name="buildingId">建筑id</param>
        /// <returns></returns>
        bool AssignSurvivorToBuilding(string survivorId, string buildingConfigId,string buildingId);
        bool UnassignSurvivorFromBuilding(string survivorId);
        List<SurvivorData> GetBuildingWorkers(string buildingId);
        bool CanAssignToBuilding(string survivorId, string buildingId);
        
        // 属性系统
        bool UpgradeSurvivorAttribute(string survivorId, SurvivorAttributeType attributeType);
        int GetSurvivorAttributeValue(string survivorId, SurvivorAttributeType attributeType);
        
        // 状态管理
        void UpdateSurvivorStates(float deltaTime);
        void HealSurvivor(string survivorId, int amount);
        void AddExperienceToSurvivor(string survivorId, int experience);
        
        // 统计信息
        SurvivorSystemData GetSystemData();
        int GetTotalSurvivorCount();
        int GetAliveSurvivorCount();
        float GetAverageTeamMorale();
        
        // 事件
        Action<string> OnSurvivorAdded { get; set; }
        Action<string> OnSurvivorRemoved { get; set; }
        Action<string, string> OnSurvivorAssigned { get; set; }
        Action<string> OnSurvivorUnassigned { get; set; }
        Action<string> OnSurvivorLevelUp { get; set; }
    }

    /// <summary>
    /// 幸存者系统实现
    /// 负责所有幸存者相关的逻辑处理
    /// </summary>
    public class SurvivorSystem : AbstractSystem, ISurvivorSystem
    {
        private ISurvivalGameModel mSurvivalGameModel;
        private IGameModel mSystemData;
        //private SurvivorSystemData mSystemData;
        
        // 事件
        public Action<string> OnSurvivorAdded { get; set; }
        public Action<string> OnSurvivorRemoved { get; set; }
        public Action<string, string> OnSurvivorAssigned { get; set; }
        public Action<string> OnSurvivorUnassigned { get; set; }
        public Action<string> OnSurvivorLevelUp { get; set; }
        
        protected override void OnInit()
        {
            mSurvivalGameModel = this.GetModel<ISurvivalGameModel>();
            
            mSystemData = this.GetModel<IGameModel>();
            //mSystemData = new SurvivorSystemData();
            
            // 创建初始幸存者
            CreateInitialSurvivors();
            
            Debug.Log("幸存者系统初始化完成");
        }
        
        private void CreateInitialSurvivors()
        {
            // 创建5个初始幸存者，职业随机
            var professions = Enum.GetValues(typeof(SurvivorProfession)).Cast<SurvivorProfession>().ToArray();
            Vector3 spawnPosition = Vector3.zero;
            
            for (int i = 0; i < 5; i++)
            {
                Debug.Log("创建初始化第" + i+"个幸存者");
                var randomProfession = professions[UnityEngine.Random.Range(0, professions.Length)];
                spawnPosition.x = i * 2f; // 简单的位置分布
                AddSurvivor(randomProfession, spawnPosition);
            }
        }
        
        /// <summary>
        /// 添加一个新的幸存者到游戏中
        /// </summary>
        /// <param name="profession">幸存者的职业（影响属性）</param>
        /// <param name="position">生成位置</param>
        /// <returns>是否成功添加幸存者</returns>
        public bool AddSurvivor(SurvivorProfession profession, Vector3 position)
        {
            // 检查当前幸存者数量是否已达到系统设定的最大限制
            if (mSystemData.SurvivorData.survivors.Count >= mSystemData.SurvivorData.maxSurvivors)
            {
                Debug.LogWarning("已达到最大幸存者数量限制");
                return false;
            }

            // 创建具有指定职业的新幸存者实例
            var newSurvivor = CreateSurvivorWithProfession(profession, position);

            // 将新幸存者加入系统数据库（通过ID作为键）
            mSystemData.SurvivorData.survivors[newSurvivor.Id] = newSurvivor;

            // 更新该职业的计数器（统计每个职业的数量）
            mSystemData.SurvivorData.professionCounts[profession]++;

            // 更新总招募人数统计
            mSystemData.SurvivorData.totalSurvivorsRecruited++;

            // 同步更新游戏模型中的人口数量显示（UI等模块使用）
            mSurvivalGameModel.Population.Value = GetAliveSurvivorCount();

            // 触发幸存者添加事件（供其他系统订阅处理）
            OnSurvivorAdded?.Invoke(newSurvivor.Id);

            // 输出调试日志，提示添加成功
            Debug.Log($"添加幸存者成功：{newSurvivor.Name} ({profession})");

            // 返回成功标志
            return true;
        }

        
        private SurvivorData CreateSurvivorWithProfession(SurvivorProfession profession, Vector3 position)
        {
            var survivor = new SurvivorData
            {
                Position = position,
                TargetPosition = position,
                Name = GenerateRandomName()
            };
            
            // 初始化7个核心属性
            InitializeSurvivorAttributes(survivor, profession);
            
            return survivor;
        }
        
        private void InitializeSurvivorAttributes(SurvivorData survivor, SurvivorProfession profession)
        {
            // 为每个属性类型创建属性数据
            foreach (SurvivorAttributeType attrType in Enum.GetValues(typeof(SurvivorAttributeType)))
            {
                int baseValue = 20; // 基础值
                
                // 根据职业调整属性值
                baseValue += GetProfessionAttributeBonus(profession, attrType);
                
                var attributeData = new SurvivorAttributeData(attrType, baseValue);
                survivor.Attributes.Add(attributeData);
            }
        }
        
        private int GetProfessionAttributeBonus(SurvivorProfession profession, SurvivorAttributeType attributeType)
        {
            switch (profession)
            {
                case SurvivorProfession.Soldier:
                    return attributeType == SurvivorAttributeType.Combat ? 15 :
                           attributeType == SurvivorAttributeType.Leadership ? 10 : 0;
                           
                case SurvivorProfession.Engineer:
                    return attributeType == SurvivorAttributeType.Technology ? 15 :
                           attributeType == SurvivorAttributeType.Production ? 10 : 0;
                           
                case SurvivorProfession.Doctor:
                    return attributeType == SurvivorAttributeType.Medical ? 15 :
                           attributeType == SurvivorAttributeType.Research ? 5 : 0;
                           
                case SurvivorProfession.Scientist:
                    return attributeType == SurvivorAttributeType.Research ? 15 :
                           attributeType == SurvivorAttributeType.Technology ? 10 : 0;
                           
                case SurvivorProfession.Scout:
                    return attributeType == SurvivorAttributeType.Exploration ? 15 :
                           attributeType == SurvivorAttributeType.Combat ? 5 : 0;
                           
                case SurvivorProfession.Worker:
                    return attributeType == SurvivorAttributeType.Production ? 15 : 0;
                    
                case SurvivorProfession.Guard:
                    return attributeType == SurvivorAttributeType.Combat ? 10 :
                           attributeType == SurvivorAttributeType.Leadership ? 5 : 0;
                           
                default:
                    return 0;
            }
        }
        
        private string GenerateRandomName()
        {
            string[] firstNames = { "张", "李", "王", "刘", "陈", "杨", "赵", "黄", "周", "吴" };
            string[] lastNames = { "伟", "芳", "娜", "敏", "静", "丽", "强", "磊", "军", "洋" };
            
            return firstNames[UnityEngine.Random.Range(0, firstNames.Length)] + 
                   lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
        }
        
        public bool RemoveSurvivor(string survivorId)
        {
            if (!mSystemData.SurvivorData.survivors.ContainsKey(survivorId))
                return false;
            
            var survivor = mSystemData.SurvivorData.survivors[survivorId];
            
            // 如果幸存者有工作分配，先取消分配
            if (!string.IsNullOrEmpty(survivor.AssignedBuildingId))
            {
                UnassignSurvivorFromBuilding(survivorId);
            }
            
            // 移除幸存者
            mSystemData.SurvivorData.survivors.Remove(survivorId);
            
            // 更新死亡统计
            if (!survivor.IsAlive)
            {
                mSystemData.SurvivorData.totalDeaths++;
            }
            
            // 更新游戏模型
            mSurvivalGameModel.Population.Value = GetAliveSurvivorCount();
            
            // 触发事件
            OnSurvivorRemoved?.Invoke(survivorId);
            
            Debug.Log($"移除幸存者：{survivor.Name}");
            return true;
        }
        
        public SurvivorData GetSurvivor(string survivorId)
        {
            return mSystemData.SurvivorData.survivors.ContainsKey(survivorId) ? mSystemData.SurvivorData.survivors[survivorId] : null;
        }
        
        public List<SurvivorData> GetAllSurvivors()
        {
            return mSystemData.SurvivorData.survivors.Values.ToList();
        }
        
        public List<SurvivorData> GetSurvivorsByJob(SurvivorJob job)
        {
            return mSystemData.SurvivorData.survivors.Values.Where(s => s.CurrentJob == job && s.IsAlive).ToList();
        }
        
        public List<SurvivorData> GetAvailableWorkers()
        {
            return mSystemData.SurvivorData.survivors.Values.Where(s => s.CanWork && s.CurrentJob == SurvivorJob.Idle).ToList();
        }
        
        /// <summary>
        /// 获取幸存者在特定建筑的工作效率加成
        /// </summary>
        private float GetJobEfficiencyBonus(SurvivorData survivor, string buildingId)
        {
            // 根据建筑类型和幸存者属性计算效率加成
            switch (buildingId)
            {
                case "Farm":
                    return survivor.GetAttributeValue(SurvivorAttributeType.Production) * 0.01f;

                case "Workshop":
                    return survivor.GetAttributeValue(SurvivorAttributeType.Technology) * 0.01f;

                case "MedicalStation":
                    return survivor.GetAttributeValue(SurvivorAttributeType.Medical) * 0.01f;

                case "WatchTower":
                    return survivor.GetAttributeValue(SurvivorAttributeType.Combat) * 0.01f;

                case "Library":
                    return survivor.GetAttributeValue(SurvivorAttributeType.Research) * 0.01f;

                case "Quarry":
                    return survivor.GetAttributeValue(SurvivorAttributeType.Production) * 0.01f;

                default:
                    return 0f;
            }
        }

        public bool AssignSurvivorToBuilding(string survivorId,string buildingConfigId, string buildingId)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor == null || !survivor.CanWork)
                return false;
            
            // 检查是否已有工作分配
            if (!string.IsNullOrEmpty(survivor.AssignedBuildingId))
            {
                UnassignSurvivorFromBuilding(survivorId);
            }
            
            
            // 分配新工作
            survivor.AssignedBuildingId = buildingId;
            survivor.CurrentJob = GetJobTypeForBuilding(buildingConfigId);
            survivor.WorkStartTime = Time.time;
            
            // 计算并应用效率加成
            float efficiencyBonus = GetJobEfficiencyBonus(survivor, buildingId);
            survivor.WorkEfficiency = 1.0f + efficiencyBonus;
            
            // 更新建筑工人列表
            if (!mSystemData.SurvivorData.buildingWorkers.ContainsKey(buildingId))
            {
                mSystemData.SurvivorData.buildingWorkers[buildingId] = new List<string>();
            }
            mSystemData.SurvivorData.buildingWorkers[buildingId].Add(survivorId);
             
            
            // 触发事件
            OnSurvivorAssigned?.Invoke(survivorId, buildingId);
            
            Debug.Log($"分配幸存者 {survivor.Name} 到建筑 {buildingId}，效率加成：{efficiencyBonus:P0}");
            return true;
        }
        
        /// <summary>
        /// 根据建筑ID获取对应的工作类型
        /// </summary>
        /// <param name="buildingConfigId">建筑类型的唯一标识符</param>
        /// <returns>对应的幸存者工作类型</returns>
        private SurvivorJob GetJobTypeForBuilding(string buildingConfigId)
        {
            Debug.Log($"获取建筑 {buildingConfigId} 的工作类型");
            switch (buildingConfigId)
            {
                // 生产型建筑
                case "Farm":           // 农场
                case "Quarry":         // 采石场
                case "Workshop":       // 工作坊
                case "BasicFarm":      // 简易农田
                case "ScavengePost":   // 拾荒站
                case "BasicWorkbench": // 简易工作台
                case "LargeFarm":      // 大型农场
                case "ArmoryFactory":  // 军工厂
                case "RefineryPlant":  // 精炼厂
                    return SurvivorJob.Production; // 生产类工作
                
                // 防御型建筑
                case "WatchTower":     // 瞭望塔
                case "Wall":           // 围墙
                case "WoodenFence":    // 木制围栏
                case "BasicOutpost":   // 简易哨所
                case "TrapPit":        // 陷阱坑
                case "TrapZone":       // 陷阱区
                case "StoneWall":      // 石制围墙
                case "DefenseTower":   // 防御塔
                case "FortressWall":   // 城墙
                case "MachineGunBunker": // 机枪堡垒
                    return SurvivorJob.Defense; // 防御类工作
                
                // 研究型建筑
                case "Library":              // 图书馆
                case "Laboratory":           // 实验室
                case "BasicResearchCorner":  // 简易研究角
                case "ResearchTable":        // 研究台
                case "SmallLaboratory":      // 小型实验室
                case "ResearchLab":          // 研究所
                case "TechCenter":           // 科技中心
                    return SurvivorJob.Research; // 研究类工作
                
                // 医疗型建筑
                case "MedicalStation":   // 医疗站
                case "FirstAidStation":  // 急救站
                case "MedicalTent":      // 医疗帐篷
                case "AdvancedHospital": // 高级医院
                case "MegaMedicalCenter": // 超级医疗中心
                    return SurvivorJob.Medical; // 医疗类工作
                
                // 功能型建筑（通常需要操作和维护）
                case "WaterPurifier":      // 净水器
                case "WaterWell":          // 水井
                case "GeneratorRoom":      // 发电机房
                case "SolarPowerStation":  // 太阳能发电站
                    return SurvivorJob.Production; // 归类为生产工作
                
                // 储存型建筑（需要管理和维护）
                case "StorageDepot":              // 储存库
                case "BasicStorageBox":           // 简易储物箱
                case "FoodStorage":               // 食物储藏室
                case "Warehouse":                 // 仓库
                case "AmmoDepot":                 // 弹药库
                case "ColdStorage":               // 冷藏库
                case "LargeWarehouse":            // 大型仓库
                case "SpecializedStorageCenter":  // 专业储存中心
                case "AutomatedWarehouse":        // 自动化仓库
                case "MegaStorageCenter":         // 超级储存中心
                    return SurvivorJob.Production; // 归类为生产工作（库管）
                
                // 居住型建筑（一般不需要专门工作）
                case "Shelter":           // 避难所
                case "TempTent":          // 临时帐篷
                case "SimpleHut":         // 简易小屋
                case "ResidentialArea":   // 住宅区
                case "ApartmentBuilding": // 公寓楼
                case "LuxuryCommunity":   // 豪华社区
                    return SurvivorJob.Idle; // 居住建筑不需要专门工作
                
                // 贸易和领导型建筑
                case "CommunicationPost": // 通讯站
                case "TradeCenter":       // 贸易中心
                    return SurvivorJob.Leadership; // 领导和协调工作
                
                // 配置系统中的建筑ID
                case "farm_1":
                case "farm_2":
                    return SurvivorJob.Production;
                    
                case "workshop_1":
                    return SurvivorJob.Production;
                    
                case "wall_1":
                case "watchtower_1":
                    return SurvivorJob.Defense;
                    
                case "shelter_1":
                    return SurvivorJob.Idle;
                    
                case "storage_1":
                    return SurvivorJob.Production;
                    
                case "well_1":
                    return SurvivorJob.Production;
                
                default: // 默认情况（未知建筑或未分配）
                    Debug.LogWarning($"未知的建筑类型: {buildingConfigId}，返回空闲状态");
                    return SurvivorJob.Idle; // 空闲状态
            }
        }
        
        public bool UnassignSurvivorFromBuilding(string survivorId)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor == null)
                return false;
            
            var buildingId = survivor.AssignedBuildingId;
            if (string.IsNullOrEmpty(buildingId))
                return false;
            
            // 清除工作分配
            survivor.AssignedBuildingId = "";
            survivor.CurrentJob = SurvivorJob.Idle;
            survivor.WorkEfficiency = 1.0f; // 重置工作效率
            
            // 从建筑工人列表中移除
            if (mSystemData.SurvivorData.buildingWorkers.ContainsKey(buildingId))
            {
                mSystemData.SurvivorData.buildingWorkers[buildingId].Remove(survivorId);
                if (mSystemData.SurvivorData.buildingWorkers[buildingId].Count == 0)
                {
                    mSystemData.SurvivorData.buildingWorkers.Remove(buildingId);
                }
            }
            
            // 触发取消分配事件
            OnSurvivorUnassigned?.Invoke(survivorId);
            
            Debug.Log($"取消分配幸存者 {survivor.Name} 的工作");
            return true;
        }
        
        public List<SurvivorData> GetBuildingWorkers(string buildingId)
        {
            if (!mSystemData.SurvivorData.buildingWorkers.ContainsKey(buildingId))
                return new List<SurvivorData>();
            
            var workerIds = mSystemData.SurvivorData.buildingWorkers[buildingId];
            return workerIds.Select(id => GetSurvivor(id)).Where(s => s != null).ToList();
        }
        
        public bool CanAssignToBuilding(string survivorId, string buildingId)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor == null || !survivor.CanWork)
                return false;
            
            // 这里可以添加更多的检查逻辑，比如属性要求等
            return true;
        }
        
        public bool UpgradeSurvivorAttribute(string survivorId, SurvivorAttributeType attributeType)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor == null)
                return false;
            
            return survivor.AddAttributeExperience(attributeType, 100); // 添加经验来升级属性
        }
        
        public int GetSurvivorAttributeValue(string survivorId, SurvivorAttributeType attributeType)
        {
            var survivor = GetSurvivor(survivorId);
            return survivor?.GetAttributeValue(attributeType) ?? 0;
        }
        
        public void UpdateSurvivorStates(float deltaTime)
        {
            foreach (var survivor in mSystemData.SurvivorData.survivors.Values)
            {
                if (!survivor.IsAlive)
                    continue;
                
                // 更新工作状态
                UpdateWorkState(survivor, deltaTime);
                
                // 更新士气
                UpdateMorale(survivor, deltaTime);
                
                // 更新疲劳度
                UpdateFatigue(survivor, deltaTime);
                
                // 更新饥饿度
                UpdateHunger(survivor, deltaTime);
            }
            
            // 更新团队士气
            mSystemData.SurvivorData.UpdateTeamMorale();
            mSurvivalGameModel.Morale.Value = Mathf.RoundToInt(mSystemData.SurvivorData.teamMorale);
        }
        
        private void UpdateWorkState(SurvivorData survivor, float deltaTime)
        {
            if (survivor.CurrentJob != SurvivorJob.Idle && !string.IsNullOrEmpty(survivor.AssignedBuildingId))
            {
                // 工作时获得经验
                float workTime = Time.time - survivor.WorkStartTime;
                if (workTime > 60f) // 每分钟获得经验
                {
                    AddExperienceToSurvivor(survivor.Id, 10);
                    
                    // 根据工作类型增加相应属性经验
                    SurvivorAttributeType workAttribute = GetWorkAttribute(survivor.CurrentJob);
                    survivor.AddAttributeExperience(workAttribute, 5);
                    
                    survivor.WorkStartTime = Time.time;
                }
                
                // 增加疲劳
                survivor.Fatigue = Mathf.Min(100, survivor.Fatigue + Mathf.RoundToInt(deltaTime * 2f));
                
                // 疲劳过度时自动休息
                if (survivor.Fatigue > 80)
                {
                    survivor.CurrentJob = SurvivorJob.Idle;
                    Debug.Log($"{survivor.Name} 疲劳过度，停止工作");
                }
            }
            else
            {
                // 休息时恢复疲劳
                survivor.Fatigue = Mathf.Max(0, survivor.Fatigue - Mathf.RoundToInt(deltaTime * 5f));
                
                // 疲劳恢复后可以重新工作
                if (survivor.Fatigue < 20 && !string.IsNullOrEmpty(survivor.AssignedBuildingId))
                {
                    survivor.CurrentJob = GetJobTypeForBuilding(survivor.AssignedBuildingId);
                    Debug.Log($"{survivor.Name} 休息完成，恢复工作");
                }
            }
        }
        
        private SurvivorAttributeType GetWorkAttribute(SurvivorJob job)
        {
            switch (job)
            {
                case SurvivorJob.Production:
                    return SurvivorAttributeType.Production;
                case SurvivorJob.Defense:
                    return SurvivorAttributeType.Combat;
                case SurvivorJob.Research:
                    return SurvivorAttributeType.Research;
                case SurvivorJob.Medical:
                    return SurvivorAttributeType.Medical;
                case SurvivorJob.Construction:
                    return SurvivorAttributeType.Technology;
                case SurvivorJob.Exploration:
                    return SurvivorAttributeType.Exploration;
                case SurvivorJob.Leadership:
                    return SurvivorAttributeType.Leadership;
                default:
                    return SurvivorAttributeType.Production;
            }
        }
        
        private void UpdateMorale(SurvivorData survivor, float deltaTime)
        {
            int moraleChange = 0;
            
            // 基础士气变化
            if (survivor.CurrentJob != SurvivorJob.Idle)
            {
                // 工作时士气缓慢增加（有目标感）
                moraleChange += Mathf.RoundToInt(deltaTime * 0.5f);
            }
            else
            {
                // 闲置时士气缓慢下降
                moraleChange -= Mathf.RoundToInt(deltaTime * 0.2f);
            }
            
            // 健康状况影响士气
            if (survivor.Health < 50)
            {
                moraleChange -= Mathf.RoundToInt(deltaTime * 1f);
            }
            
            // 饥饿影响士气
            if (survivor.Hunger > 70)
            {
                moraleChange -= Mathf.RoundToInt(deltaTime * 1f);
            }
            
            // 应用士气变化
            survivor.Morale = Mathf.Clamp(survivor.Morale + moraleChange, 0, 100);
        }
        
        private void UpdateFatigue(SurvivorData survivor, float deltaTime)
        {
            // 疲劳自然恢复（如果不在工作）
            if (survivor.CurrentJob == SurvivorJob.Idle)
            {
                survivor.Fatigue = Mathf.Max(0, survivor.Fatigue - Mathf.RoundToInt(deltaTime * 3f));
            }
        }
        
        private void UpdateHunger(SurvivorData survivor, float deltaTime)
        {
            // 饥饿度自然增加
            survivor.Hunger = Mathf.Min(100, survivor.Hunger + Mathf.RoundToInt(deltaTime * 0.5f));
            
            // 饥饿影响健康
            if (survivor.Hunger > 80)
            {
                survivor.Health = Mathf.Max(0, survivor.Health - Mathf.RoundToInt(deltaTime * 0.5f));
            }
        }
        
        public void HealSurvivor(string survivorId, int amount)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor != null)
            {
                survivor.Health = Mathf.Min(100, survivor.Health + amount);
                Debug.Log($"治疗幸存者 {survivor.Name}，恢复 {amount} 生命值");
            }
        }
        
        public void AddExperienceToSurvivor(string survivorId, int experience)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor != null)
            {
                int oldLevel = survivor.Level;
                survivor.Experience += experience;
                
                // 检查是否升级
                int requiredExp = survivor.Level * 100;
                if (survivor.Experience >= requiredExp)
                {
                    survivor.Experience -= requiredExp;
                    survivor.Level++;
                    OnSurvivorLevelUp?.Invoke(survivorId);
                }
            }
        }
        
        public SurvivorSystemData GetSystemData()
        {
            return mSystemData.SurvivorData;
        }
        
        public int GetTotalSurvivorCount()
        {
            return mSystemData.SurvivorData.survivors.Count;
        }
        
        public int GetAliveSurvivorCount()
        {
            return mSystemData.SurvivorData.survivors.Values.Count(s => s.IsAlive);
        }
        
        public float GetAverageTeamMorale()
        {
            return mSystemData.SurvivorData.teamMorale;
        }
        
        // 兼容性方法，保持接口一致
        public List<SurvivorData> GetSurvivorsByProfession(SurvivorProfession profession)
        {
            // 由于新系统不再使用职业概念，返回空列表
            return new List<SurvivorData>();
        }
        
        public bool UpgradeSurvivorSkill(string survivorId, SurvivorSkill skill)
        {
            // 转换为新的属性系统
            var attributeType = ConvertSkillToAttribute(skill);
            return UpgradeSurvivorAttribute(survivorId, attributeType);
        }
        
        public int GetAvailableSkillPoints(string survivorId)
        {
            // 新系统不使用技能点，返回0
            return 0;
        }
        
        private SurvivorAttributeType ConvertSkillToAttribute(SurvivorSkill skill)
        {
            switch (skill)
            {
                case SurvivorSkill.Combat:
                    return SurvivorAttributeType.Combat;
                case SurvivorSkill.Construction:
                    return SurvivorAttributeType.Technology;
                case SurvivorSkill.Medicine:
                    return SurvivorAttributeType.Medical;
                case SurvivorSkill.Research:
                    return SurvivorAttributeType.Research;
                case SurvivorSkill.Leadership:
                    return SurvivorAttributeType.Leadership;
                case SurvivorSkill.Gathering:
                    return SurvivorAttributeType.Production;
                default:
                    return SurvivorAttributeType.Production;
            }
        }
    }
} 