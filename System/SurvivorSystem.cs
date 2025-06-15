// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：SurvivorSystem.cs
// 作者：未知开发者
// 创建日期：2024年07月16日 // 根据实际情况修改
// 修改日期：2024年07月16日 // 根据实际情况修改
// 文件版本：1.0.0
// 描述：
//     此文件定义了幸存者管理系统 (SurvivorSystem) 及其相关接口。
//     该系统负责管理游戏中的所有幸存者，包括其生成、属性、职业（或技能）、
//     状态（健康、士气、饥饿、疲劳）、工作分配以及相关的事件处理。
//     它还提供了查询和操作幸存者数据的各种方法。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq; // 用于LINQ查询，例如在GetSurvivorsByJob等方法中
using UnityEngine; // 用于UnityEngine功能，如Random.Range, Vector3, Debug.Log等
using QFramework;    // QFramework框架的相关引用
using SurvivalGame.Model; // 游戏核心数据模型的引用

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 幸存者系统接口 (ISurvivorSystem)。
    /// 定义了幸存者管理的核心功能，包括幸存者的增删查改、工作分配、属性/技能升级、状态更新及相关事件。
    /// </summary>
    public interface ISurvivorSystem : QFISystem
    {
        // --- 幸存者管理 (Survivor Management) ---
        /// <summary>
        /// 在指定位置添加一名具有特定职业的新幸存者。
        /// </summary>
        /// <param name="profession">新幸存者的职业。</param>
        /// <param name="position">幸存者的出生或初始位置。</param>
        /// <returns>如果成功添加幸存者则返回true，否则（例如达到数量上限）返回false。</returns>
        bool AddSurvivor(SurvivorProfession profession, Vector3 position);

        /// <summary>
        /// 从游戏中移除指定ID的幸存者。
        /// </summary>
        /// <param name="survivorId">要移除的幸存者的唯一ID。</param>
        /// <returns>如果成功移除则返回true，否则（例如幸存者不存在）返回false。</returns>
        bool RemoveSurvivor(string survivorId);

        /// <summary>
        /// 根据ID获取指定的幸存者数据。
        /// </summary>
        /// <param name="survivorId">要查询的幸存者的唯一ID。</param>
        /// <returns>对应的SurvivorData实例；如果找不到，则返回null。</returns>
        SurvivorData GetSurvivor(string survivorId);

        /// <summary>
        /// 获取当前游戏中所有幸存者的数据列表。
        /// </summary>
        /// <returns>包含所有SurvivorData对象的列表。</returns>
        List<SurvivorData> GetAllSurvivors();

        /// <summary>
        /// 根据指定的工作类型获取正在从事该工作的幸存者列表。
        /// </summary>
        /// <param name="job">要查询的工作类型。</param>
        /// <returns>从事指定工作的幸存者数据列表。</returns>
        List<SurvivorData> GetSurvivorsByJob(SurvivorJob job);

        /// <summary>
        /// 获取当前所有空闲且能够工作的幸存者列表。
        /// </summary>
        /// <returns>可分配工作的幸存者数据列表。</returns>
        List<SurvivorData> GetAvailableWorkers();
        
        /// <summary>
        /// 将指定ID的幸存者分配到指定ID和配置ID的建筑中工作。
        /// </summary>
        /// <param name="survivorId">要分配的幸存者的唯一ID。</param>
        /// <param name="buildingConfigId">目标建筑的配置ID (用于确定工作类型等)。</param>
        /// <param name="buildingId">目标建筑的实例唯一ID。</param>
        /// <returns>如果成功分配则返回true，否则（如幸存者不可用、建筑已满等）返回false。</returns>
        bool AssignSurvivorToBuilding(string survivorId, string buildingConfigId, string buildingId);

        /// <summary>
        /// 取消指定ID幸存者的当前建筑工作分配，使其变为空闲状态。
        /// </summary>
        /// <param name="survivorId">要取消分配的幸存者的唯一ID。</param>
        /// <returns>如果成功取消分配则返回true，否则false。</returns>
        bool UnassignSurvivorFromBuilding(string survivorId);

        /// <summary>
        /// 获取分配给指定建筑ID的所有工人（幸存者）的数据列表。
        /// </summary>
        /// <param name="buildingId">要查询的建筑的唯一ID。</param>
        /// <returns>在该建筑工作的幸存者数据列表。</returns>
        List<SurvivorData> GetBuildingWorkers(string buildingId);

        /// <summary>
        /// 检查指定ID的幸存者是否可以被分配到指定ID的建筑工作。
        /// （可能包含对幸存者状态、建筑容量、特定技能要求的检查）
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="buildingId">建筑ID。</param>
        /// <returns>如果可以分配则返回true，否则返回false。</returns>
        bool CanAssignToBuilding(string survivorId, string buildingId);
        
        // --- 属性系统 (Attribute System) ---
        /// <summary>
        /// 尝试升级指定幸存者的某项属性。
        /// （通常通过消耗经验值或技能点实现）
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="attributeType">要升级的属性类型。</param>
        /// <returns>如果成功升级则返回true，否则false。</returns>
        bool UpgradeSurvivorAttribute(string survivorId, SurvivorAttributeType attributeType);

        /// <summary>
        /// 获取指定幸存者某项属性的当前值。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="attributeType">要查询的属性类型。</param>
        /// <returns>属性的当前值；如果幸存者或属性不存在，则可能返回0或默认值。</returns>
        int GetSurvivorAttributeValue(string survivorId, SurvivorAttributeType attributeType);
        
        // --- 状态管理 (State Management) ---
        /// <summary>
        /// 更新所有幸存者的状态，如饥饿、疲劳、士气等。
        /// 此方法通常由游戏主循环定期调用。
        /// </summary>
        /// <param name="deltaTime">自上次更新以来经过的时间（秒）。</param>
        void UpdateSurvivorStates(float deltaTime);

        /// <summary>
        /// 治疗指定ID的幸存者，恢复其一定量的生命值。
        /// </summary>
        /// <param name="survivorId">要治疗的幸存者ID。</param>
        /// <param name="amount">恢复的生命值量。</param>
        void HealSurvivor(string survivorId, int amount);

        /// <summary>
        /// 为指定ID的幸存者增加经验值，可能触发等级提升。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="experience">增加的经验值量。</param>
        void AddExperienceToSurvivor(string survivorId, int experience);
        
        // --- 统计信息 (Statistics) ---
        /// <summary>
        /// 获取幸存者系统的核心数据对象。
        /// </summary>
        /// <returns>SurvivorSystemData实例，包含所有幸存者数据和统计。</returns>
        SurvivorSystemData GetSystemData();

        /// <summary>
        /// 获取当前游戏中幸存者的总数量（包括所有状态，如存活、死亡等，取决于具体实现）。
        /// </summary>
        /// <returns>幸存者总数。</returns>
        int GetTotalSurvivorCount();

        /// <summary>
        /// 获取当前游戏中所有存活的幸存者数量。
        /// </summary>
        /// <returns>存活的幸存者数量。</returns>
        int GetAliveSurvivorCount();

        /// <summary>
        /// 获取当前幸存者团队的平均士气值。
        /// </summary>
        /// <returns>平均士气值。</returns>
        float GetAverageTeamMorale();
        
        // --- 事件回调 (Events) ---
        /// <summary>当有新幸存者被添加到游戏中时触发的事件。参数为新幸存者的ID。</summary>
        Action<string> OnSurvivorAdded { get; set; }
        /// <summary>当有幸存者从游戏中被移除（如死亡）时触发的事件。参数为被移除幸存者的ID。</summary>
        Action<string> OnSurvivorRemoved { get; set; }
        /// <summary>当幸存者被成功分配到建筑工作时触发的事件。参数1为幸存者ID，参数2为建筑ID。</summary>
        Action<string, string> OnSurvivorAssigned { get; set; }
        /// <summary>当幸存者的工作分配被取消时触发的事件。参数为幸存者ID。</summary>
        Action<string> OnSurvivorUnassigned { get; set; }
        /// <summary>当幸存者等级提升时触发的事件。参数为升级幸存者的ID。</summary>
        Action<string> OnSurvivorLevelUp { get; set; }
    }

    /// <summary>
    /// 幸存者系统 (SurvivorSystem) 的具体实现类。
    /// 负责游戏中所有与幸存者相关的逻辑处理，包括其生命周期、属性、状态和行为。
    /// </summary>
    public class SurvivorSystem : AbstractSystem, ISurvivorSystem
    {
        /// <summary>
        /// 生存游戏核心数据模型的引用，用于访问和修改全局游戏状态。
        /// </summary>
        private ISurvivalGameModel mSurvivalGameModel;
        /// <summary>
        /// 通用游戏数据模型的引用，特别用于访问其中存储的幸存者系统数据 (SurvivorSystemData)。
        /// </summary>
        private IGameModel mSystemData; // 注意：此处的IGameModel持有SurvivorSystemData
        //private SurvivorSystemData mSystemData; // 原直接持有SurvivorSystemData的方式，现通过IGameModel访问
        
        // --- 事件定义 (Event Definitions) ---
        /// <summary>当有新幸存者被添加到游戏中时触发的事件。参数为新幸存者的ID。</summary>
        public Action<string> OnSurvivorAdded { get; set; }
        /// <summary>当有幸存者从游戏中被移除（如死亡）时触发的事件。参数为被移除幸存者的ID。</summary>
        public Action<string> OnSurvivorRemoved { get; set; }
        /// <summary>当幸存者被成功分配到建筑工作时触发的事件。参数1为幸存者ID，参数2为建筑ID。</summary>
        public Action<string, string> OnSurvivorAssigned { get; set; }
        /// <summary>当幸存者的工作分配被取消时触发的事件。参数为幸存者ID。</summary>
        public Action<string> OnSurvivorUnassigned { get; set; }
        /// <summary>当幸存者等级提升时触发的事件。参数为升级幸存者的ID。</summary>
        public Action<string> OnSurvivorLevelUp { get; set; }
        
        /// <summary>
        /// 系统初始化方法。
        /// 获取对数据模型的引用，并创建初始的幸存者群体。
        /// </summary>
        protected override void OnInit()
        {
            mSurvivalGameModel = this.GetModel<ISurvivalGameModel>(); // 获取生存游戏特定的数据模型
            mSystemData = this.GetModel<IGameModel>(); // 获取通用的游戏数据模型，其中应包含SurvivorSystemData
            
            // mSystemData = new SurvivorSystemData(); // 如果SurvivorSystemData不由IGameModel管理，则在此处初始化
            
            CreateInitialSurvivors(); // 创建游戏开始时的初始幸存者
            
            Debug.Log("[幸存者系统] 初始化完成。");
        }
        
        /// <summary>
        /// 创建初始的幸存者群体。
        /// （示例：创建5名职业随机的初始幸存者）
        /// </summary>
        private void CreateInitialSurvivors()
        {
            // 获取所有可能的幸存者职业
            var professions = Enum.GetValues(typeof(SurvivorProfession)).Cast<SurvivorProfession>().ToArray();
            Vector3 spawnPosition = Vector3.zero; // 初始生成位置基点
            
            for (int i = 0; i < 5; i++) // 创建5名初始幸存者
            {
                Debug.Log($"[幸存者系统] 正在创建第 {i+1} 个初始幸存者...");
                // 随机选择一个职业
                var randomProfession = professions[UnityEngine.Random.Range(0, professions.Length)];
                // 为每个幸存者设置一个略微不同的生成位置（简单示例）
                spawnPosition.x = i * 2f;
                AddSurvivor(randomProfession, spawnPosition); // 调用添加幸存者的方法
            }
        }
        
        /// <summary>
        /// 添加一名新的幸存者到游戏中。
        /// </summary>
        /// <param name="profession">新幸存者的职业，这将影响其初始属性。</param>
        /// <param name="position">幸存者在游戏世界中的生成位置。</param>
        /// <returns>如果成功添加幸存者则返回true，否则（例如达到最大幸存者上限）返回false。</returns>
        public bool AddSurvivor(SurvivorProfession profession, Vector3 position)
        {
            // 检查当前幸存者数量是否已达到系统设定的最大限制
            if (mSystemData.SurvivorData.survivors.Count >= mSystemData.SurvivorData.maxSurvivors)
            {
                Debug.LogWarning("[幸存者系统] 添加幸存者失败：已达到最大幸存者数量限制。");
                return false;
            }

            // 创建具有指定职业的新幸存者数据实例
            var newSurvivor = CreateSurvivorWithProfession(profession, position);

            // 将新幸存者数据加入到由ID索引的全局幸存者字典中
            mSystemData.SurvivorData.survivors[newSurvivor.Id] = newSurvivor;

            // 更新对应职业的幸存者数量统计
            mSystemData.SurvivorData.professionCounts[profession]++;

            // 更新已招募的幸存者总数统计
            mSystemData.SurvivorData.totalSurvivorsRecruited++;

            // 同步更新游戏主模型中的总人口数量（通常用于UI显示或全局状态判断）
            mSurvivalGameModel.Population.Value = GetAliveSurvivorCount();

            // 触发“幸存者已添加”事件，通知其他可能对此感兴趣的系统
            OnSurvivorAdded?.Invoke(newSurvivor.Id);

            Debug.Log($"[幸存者系统] 成功添加幸存者：{newSurvivor.Name} (职业: {profession}, ID: {newSurvivor.Id})。");
            return true; // 返回成功标志
        }
        
        /// <summary>
        /// （辅助方法）创建一个具有指定职业和位置的幸存者数据对象，并初始化其属性。
        /// </summary>
        /// <param name="profession">幸存者的职业。</param>
        /// <param name="position">幸存者的初始位置。</param>
        /// <returns>初始化完成的SurvivorData对象。</returns>
        private SurvivorData CreateSurvivorWithProfession(SurvivorProfession profession, Vector3 position)
        {
            var survivor = new SurvivorData // 创建SurvivorData实例
            {
                // Id 会在SurvivorData的构造函数中自动生成
                Position = position,         // 设置初始位置
                TargetPosition = position,   // 初始目标位置与当前位置相同
                Name = GenerateRandomName() // 生成一个随机的中文名
            };
            
            InitializeSurvivorAttributes(survivor, profession); // 初始化该幸存者的所有核心属性
            survivor.Profession = profession; // 设置其职业
            
            return survivor;
        }
        
        /// <summary>
        /// （辅助方法）根据指定的职业初始化幸存者的各项核心属性。
        /// </summary>
        /// <param name="survivor">要初始化属性的幸存者数据对象。</param>
        /// <param name="profession">幸存者的职业，用于决定属性的基础值和加成。</param>
        private void InitializeSurvivorAttributes(SurvivorData survivor, SurvivorProfession profession)
        {
            // 遍历所有定义的幸存者属性类型 (SurvivorAttributeType 枚举)
            foreach (SurvivorAttributeType attrType in Enum.GetValues(typeof(SurvivorAttributeType)))
            {
                int baseValue = 20; // 为所有属性设置一个通用基础值 (例如20)
                
                // 根据幸存者的职业，获取该职业在此特定属性上的额外加成值
                baseValue += GetProfessionAttributeBonus(profession, attrType);
                
                // 创建属性数据对象 (SurvivorAttributeData) 并设置其类型和最终计算出的值
                var attributeData = new SurvivorAttributeData(attrType, baseValue);
                survivor.Attributes.Add(attributeData); // 将此属性数据添加到幸存者的属性列表中
            }
        }
        
        /// <summary>
        /// （辅助方法）根据幸存者的职业和目标属性类型，获取职业带来的属性点加成。
        /// </summary>
        /// <param name="profession">幸存者的职业。</param>
        /// <param name="attributeType">要查询加成的属性类型。</param>
        /// <returns>该职业在此属性上的加成值；如果没有特定加成，则返回0。</returns>
        private int GetProfessionAttributeBonus(SurvivorProfession profession, SurvivorAttributeType attributeType)
        {
            // 不同职业对不同属性有不同的加成
            switch (profession)
            {
                case SurvivorProfession.Soldier: // 士兵
                    return attributeType == SurvivorAttributeType.Combat ? 15 : // 战斗属性 +15
                           attributeType == SurvivorAttributeType.Leadership ? 10 : 0; // 领导属性 +10
                           
                case SurvivorProfession.Engineer: // 工程师
                    return attributeType == SurvivorAttributeType.Technology ? 15 : // 科技属性 +15
                           attributeType == SurvivorAttributeType.Production ? 10 : 0; // 生产属性 +10
                           
                case SurvivorProfession.Doctor: // 医生
                    return attributeType == SurvivorAttributeType.Medical ? 15 :    // 医疗属性 +15
                           attributeType == SurvivorAttributeType.Research ? 5 : 0; // 研究属性 +5
                           
                case SurvivorProfession.Scientist: // 科学家
                    return attributeType == SurvivorAttributeType.Research ? 15 :   // 研究属性 +15
                           attributeType == SurvivorAttributeType.Technology ? 10 : 0; // 科技属性 +10
                           
                case SurvivorProfession.Scout: // 侦察兵
                    return attributeType == SurvivorAttributeType.Exploration ? 15 :// 探索属性 +15
                           attributeType == SurvivorAttributeType.Combat ? 5 : 0;    // 战斗属性 +5
                           
                case SurvivorProfession.Worker: // 工人
                    return attributeType == SurvivorAttributeType.Production ? 15 : 0; // 生产属性 +15
                    
                case SurvivorProfession.Guard: // 守卫
                    return attributeType == SurvivorAttributeType.Combat ? 10 :     // 战斗属性 +10
                           attributeType == SurvivorAttributeType.Leadership ? 5 : 0; // 领导属性 +5
                           
                default: // 其他或未定义职业无特定加成
                    return 0;
            }
        }
        
        /// <summary>
        /// （辅助方法）生成一个随机的中文名。
        /// </summary>
        /// <returns>随机生成的中文名，格式为“姓+名”。</returns>
        private string GenerateRandomName()
        {
            // 预设的姓氏和名字列表
            string[] firstNames = { "张", "李", "王", "刘", "陈", "杨", "赵", "黄", "周", "吴", "徐", "孙", "胡", "朱", "高", "林", "何", "郭", "马", "罗" };
            string[] lastNames = { "伟", "芳", "娜", "敏", "静", "丽", "强", "磊", "军", "洋", "勇", "杰", "秀", "艳", "平", "刚", "建华", "桂英", "秀英", "明" };
            
            // 从列表中随机选择一个姓和一个名进行组合
            return firstNames[UnityEngine.Random.Range(0, firstNames.Length)] + 
                   lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
        }
        
        /// <summary>
        /// 从游戏中移除指定ID的幸存者。
        /// </summary>
        /// <param name="survivorId">要移除的幸存者ID。</param>
        /// <returns>如果成功移除则返回true，否则（如幸存者不存在）返回false。</returns>
        public bool RemoveSurvivor(string survivorId)
        {
            if (!mSystemData.SurvivorData.survivors.ContainsKey(survivorId)) // 检查幸存者是否存在
            {
                Debug.LogWarning($"[幸存者系统] 尝试移除失败：找不到ID为 {survivorId} 的幸存者。");
                return false;
            }
            
            var survivor = mSystemData.SurvivorData.survivors[survivorId]; // 获取幸存者数据
            
            // 如果幸存者当前有工作分配，则先取消其工作分配
            if (!string.IsNullOrEmpty(survivor.AssignedBuildingId))
            {
                UnassignSurvivorFromBuilding(survivorId);
            }
            
            mSystemData.SurvivorData.survivors.Remove(survivorId); // 从主列表中移除幸存者
            
            // 如果幸存者在移除时已死亡，更新死亡统计 (假设IsAlive属性反映此状态)
            if (!survivor.IsAlive)
            {
                mSystemData.SurvivorData.totalDeaths++;
            }
            // 注意：如果职业计数也需要精确反映当前存活的各职业数量，则此处也应更新 professionCounts
            // mSystemData.SurvivorData.professionCounts[survivor.Profession]--;
            
            mSurvivalGameModel.Population.Value = GetAliveSurvivorCount(); // 更新全局人口数量
            OnSurvivorRemoved?.Invoke(survivorId); // 触发幸存者移除事件
            
            Debug.Log($"[幸存者系统] 已移除幸存者：{survivor.Name} (ID: {survivorId})。");
            return true;
        }
        
        /// <summary>
        /// 根据ID获取幸存者数据。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <returns>SurvivorData对象；如果不存在则返回null。</returns>
        public SurvivorData GetSurvivor(string survivorId)
        {
            // 使用TryGetValue以安全地获取，如果键不存在则返回null
            return mSystemData.SurvivorData.survivors.TryGetValue(survivorId, out var survivor) ? survivor : null;
        }
        
        /// <summary>
        /// 获取所有幸存者的数据列表。
        /// </summary>
        /// <returns>包含所有SurvivorData的列表。</returns>
        public List<SurvivorData> GetAllSurvivors()
        {
            return mSystemData.SurvivorData.survivors.Values.ToList(); // 返回字典中所有值的列表副本
        }
        
        /// <summary>
        /// 根据工作类型筛选当前存活的幸存者。
        /// </summary>
        /// <param name="job">要筛选的工作类型。</param>
        /// <returns>从事该工作的存活幸存者列表。</returns>
        public List<SurvivorData> GetSurvivorsByJob(SurvivorJob job)
        {
            return mSystemData.SurvivorData.survivors.Values
                .Where(s => s.CurrentJob == job && s.IsAlive) // 筛选条件：工作类型匹配且存活
                .ToList();
        }
        
        /// <summary>
        /// 获取所有当前空闲且能够工作的幸存者。
        /// </summary>
        /// <returns>可分配工作的幸存者列表。</returns>
        public List<SurvivorData> GetAvailableWorkers()
        {
            return mSystemData.SurvivorData.survivors.Values
                .Where(s => s.CanWork && s.CurrentJob == SurvivorJob.Idle && s.IsAlive) // 筛选条件：能工作、当前空闲且存活
                .ToList();
        }
        
        /// <summary>
        /// （辅助方法，可能已废弃或用于特定场景）获取幸存者在特定建筑（由建筑ID标识）中的工作效率加成。
        /// 注意：当前的AssignSurvivorToBuilding中效率计算不直接使用此方法。
        /// </summary>
        /// <param name="survivor">幸存者数据。</param>
        /// <param name="buildingId">建筑的实例ID (非配置ID)。</param>
        /// <returns>基于幸存者属性和建筑类型的效率加成值 (例如0.01代表1%)。</returns>
        private float GetJobEfficiencyBonus(SurvivorData survivor, string buildingId)
        {
            // TODO: 此方法逻辑可能需要与AssignSurvivorToBuilding中的效率计算统一或澄清其用途。
            // 当前实现依赖于传入buildingId直接对应特定建筑类型，这可能不够灵活。
            // 更通用的做法是传入BuildingConfig或BuildingCategory。
            switch (buildingId) // 此处的buildingId似乎是指建筑类型或配置ID，而非实例ID
            {
                case "Farm": // 农场
                    return survivor.GetAttributeValue(SurvivorAttributeType.Production) * 0.01f; // 生产属性每点提供1%加成

                case "Workshop": // 工坊
                    return survivor.GetAttributeValue(SurvivorAttributeType.Technology) * 0.01f; // 科技属性每点提供1%加成

                case "MedicalStation": // 医疗站
                    return survivor.GetAttributeValue(SurvivorAttributeType.Medical) * 0.01f;    // 医疗属性每点提供1%加成

                case "WatchTower": // 瞭望塔
                    return survivor.GetAttributeValue(SurvivorAttributeType.Combat) * 0.01f;     // 战斗属性每点提供1%加成

                case "Library": // 图书馆 (假设用于研究)
                    return survivor.GetAttributeValue(SurvivorAttributeType.Research) * 0.01f;   // 研究属性每点提供1%加成

                case "Quarry": // 采石场
                    return survivor.GetAttributeValue(SurvivorAttributeType.Production) * 0.01f; // 生产属性每点提供1%加成

                default: // 其他未知建筑类型无特定加成
                    return 0f;
            }
        }

        /// <summary>
        /// 将幸存者分配到指定的建筑进行工作。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="buildingConfigId">目标建筑的配置ID（用于确定工作类型）。</param>
        /// <param name="buildingId">目标建筑的实例ID。</param>
        /// <returns>成功分配返回true，否则false。</returns>
        public bool AssignSurvivorToBuilding(string survivorId,string buildingConfigId, string buildingId)
        {
            var survivor = GetSurvivor(survivorId); // 获取幸存者数据
            // 检查幸存者是否存在且能够工作
            if (survivor == null || !survivor.CanWork)
            {
                Debug.LogWarning($"[幸存者系统] 分配失败：幸存者 {survivorId} 不存在或无法工作。");
                return false;
            }
            
            // 如果幸存者当前已有工作分配，则先取消之前的分配
            if (!string.IsNullOrEmpty(survivor.AssignedBuildingId))
            {
                UnassignSurvivorFromBuilding(survivorId);
            }
            
            // 分配新的工作
            survivor.AssignedBuildingId = buildingId; // 记录分配到的建筑实例ID
            survivor.CurrentJob = GetJobTypeForBuilding(buildingConfigId); // 根据建筑配置ID确定工作类型
            survivor.WorkStartTime = Time.time; // 记录工作开始时间
            
            // 计算并应用工作效率加成 (此处的GetJobEfficiencyBonus可能需要用buildingConfigId)
            float efficiencyBonus = GetJobEfficiencyBonus(survivor, buildingConfigId); // 注意：原用buildingId，可能应为buildingConfigId
            survivor.WorkEfficiency = 1.0f + efficiencyBonus; // 基础效率100% + 额外加成
            
            // 更新建筑的工人列表缓存
            if (!mSystemData.SurvivorData.buildingWorkers.ContainsKey(buildingId))
            {
                mSystemData.SurvivorData.buildingWorkers[buildingId] = new List<string>();
            }
            mSystemData.SurvivorData.buildingWorkers[buildingId].Add(survivorId);
             
            OnSurvivorAssigned?.Invoke(survivorId, buildingId); // 触发幸存者分配事件
            
            Debug.Log($"[幸存者系统] 已分配幸存者 {survivor.Name} (ID: {survivorId}) 到建筑 {buildingConfigId} (实例ID: {buildingId})。" +
                      $"工作类型: {survivor.CurrentJob}, 效率加成: {efficiencyBonus:P0}。");
            return true;
        }
        
        /// <summary>
        /// （辅助方法）根据建筑的配置ID确定其提供的工作类型。
        /// </summary>
        /// <param name="buildingConfigId">建筑的配置ID。</param>
        /// <returns>对应的SurvivorJob枚举值。</returns>
        private SurvivorJob GetJobTypeForBuilding(string buildingConfigId)
        {
            // Debug.Log($"[幸存者系统] 正在为建筑配置ID '{buildingConfigId}' 获取工作类型..."); // 日志输出当前查询的配置ID
            // 注意：此方法依赖于硬编码的建筑配置ID到工作类型的映射。在更复杂的系统中，这部分信息可能来自建筑配置本身。
            switch (buildingConfigId)
            {
                // --- 生产型建筑 ---
                case "Farm":            // 农场 (来自旧数据或测试)
                case "Quarry":          // 采石场 (来自旧数据或测试)
                case "Workshop":        // 工作坊 (来自旧数据或测试)
                case "BasicFarm":       // 简易农田 (来自BuildingSystem的建筑类型)
                case "ScavengePost":    // 拾荒站
                case "BasicWorkbench":  // 简易工作台
                case "LargeFarm":       // 大型农场
                case "ArmoryFactory":   // 军工厂
                case "RefineryPlant":   // 精炼厂
                case "farm_1":          // 农田等级1 (来自ConfigSystem示例)
                case "farm_2":          // 农田等级2 (来自ConfigSystem示例)
                case "workshop_1":      // 工坊等级1 (来自ConfigSystem示例)
                case "well_1":          // 水井 (来自ConfigSystem示例，视为生产水)
                    return SurvivorJob.Production; // 生产类工作
                
                // --- 防御型建筑 ---
                case "WatchTower":      // 瞭望塔 (来自旧数据或测试)
                case "Wall":            // 围墙
                case "WoodenFence":     // 木制围栏
                case "BasicOutpost":    // 简易哨所
                case "TrapPit":         // 陷阱坑
                case "TrapZone":        // 陷阱区
                case "StoneWall":       // 石制围墙
                case "DefenseTower":    // 防御塔
                case "FortressWall":    // 城墙
                case "MachineGunBunker":// 机枪堡垒
                case "wall_1":          // 木制围墙 (来自ConfigSystem示例)
                case "watchtower_1":    // 简易哨塔 (来自ConfigSystem示例)
                    return SurvivorJob.Defense; // 防御类工作
                
                // --- 研究型建筑 ---
                case "Library":               // 图书馆 (来自旧数据或测试)
                case "Laboratory":            // 实验室
                case "BasicResearchCorner":   // 简易研究角
                case "ResearchTable":         // 研究台
                case "SmallLaboratory":       // 小型实验室
                case "ResearchLab":           // 研究所
                case "TechCenter":            // 科技中心
                    return SurvivorJob.Research; // 研究类工作
                
                // --- 医疗型建筑 ---
                case "MedicalStation":    // 医疗站 (来自旧数据或测试)
                case "FirstAidStation":   // 急救站
                case "MedicalTent":       // 医疗帐篷
                case "AdvancedHospital":  // 高级医院
                case "MegaMedicalCenter": // 超级医疗中心
                    return SurvivorJob.Medical; // 医疗类工作
                
                // --- 功能型建筑 (部分归为生产或特定类型) ---
                case "WaterPurifier":       // 净水器
                case "WaterWell":           // 水井 (与ConfigSystem的well_1重复，统一为Production)
                case "GeneratorRoom":       // 发电机房
                case "SolarPowerStation":   // 太阳能发电站
                    return SurvivorJob.Production; // 此类建筑操作员也视为广义的“生产”工人
                
                // --- 储存型建筑 (通常不直接分配固定工人，但可能需要管理) ---
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
                case "storage_1":                 // 简易储物箱 (来自ConfigSystem示例)
                    return SurvivorJob.Production; // 库管员等可视为生产支持类
                
                // --- 居住型建筑 (通常不分配固定工人) ---
                case "Shelter":            // 避难所 (来自旧数据或测试)
                case "TempTent":           // 临时帐篷
                case "SimpleHut":          // 简易小屋
                case "ResidentialArea":    // 住宅区
                case "ApartmentBuilding":  // 公寓楼
                case "LuxuryCommunity":    // 豪华社区
                case "shelter_1":          // 简易住所 (来自ConfigSystem示例)
                    return SurvivorJob.Idle; // 居住建筑通常不产生工作岗位
                
                // --- 贸易和领导型建筑 ---
                case "CommunicationPost":  // 通讯站
                case "TradeCenter":        // 贸易中心
                    return SurvivorJob.Leadership; // 涉及协调、沟通、管理等
                
                default: // 对于未知或未明确分类的建筑ID
                    Debug.LogWarning($"[幸存者系统] 未知的建筑配置ID '{buildingConfigId}' 无法确定工作类型，默认为空闲。");
                    return SurvivorJob.Idle; // 默认为空闲状态
            }
        }
        
        /// <summary>
        /// 取消幸存者的当前工作分配。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <returns>成功取消返回true，否则false。</returns>
        public bool UnassignSurvivorFromBuilding(string survivorId)
        {
            var survivor = GetSurvivor(survivorId); // 获取幸存者数据
            if (survivor == null) // 检查幸存者是否存在
            {
                Debug.LogWarning($"[幸存者系统] 取消分配失败：找不到ID为 {survivorId} 的幸存者。");
                return false;
            }
            
            var buildingId = survivor.AssignedBuildingId; // 获取其当前分配的建筑ID
            if (string.IsNullOrEmpty(buildingId)) // 如果未分配到任何建筑
            {
                // Debug.Log($"[幸存者系统] 幸存者 {survivor.Name} (ID: {survivorId}) 当前未分配工作，无需取消。");
                return false; // 无需操作，或视为已成功（取决于设计）
            }
            
            // 清除幸存者数据中的工作分配信息
            survivor.AssignedBuildingId = "";            // 清空分配的建筑ID
            survivor.CurrentJob = SurvivorJob.Idle;      // 设置当前工作为空闲
            survivor.WorkEfficiency = 1.0f;              // 重置工作效率为标准值
            
            // 从对应建筑的工人列表中移除此幸存者
            if (mSystemData.SurvivorData.buildingWorkers.TryGetValue(buildingId, out var workerList))
            {
                workerList.Remove(survivorId);
                // 如果移除后工人列表为空，可以选择从字典中移除该建筑的条目以保持清洁
                if (workerList.Count == 0)
                {
                    mSystemData.SurvivorData.buildingWorkers.Remove(buildingId);
                }
            }
            
            OnSurvivorUnassigned?.Invoke(survivorId); // 触发幸存者取消分配事件
            
            Debug.Log($"[幸存者系统] 已取消幸存者 {survivor.Name} (ID: {survivorId}) 的工作分配（原建筑ID: {buildingId}）。");
            return true;
        }
        
        /// <summary>
        /// 获取在指定建筑中工作的所有幸存者的数据列表。
        /// </summary>
        /// <param name="buildingId">建筑ID。</param>
        /// <returns>在该建筑工作的幸存者列表；如果建筑不存在或无工人，则返回空列表。</returns>
        public List<SurvivorData> GetBuildingWorkers(string buildingId)
        {
            // 如果建筑ID无效或未在工人分配记录中，则返回空列表
            if (string.IsNullOrEmpty(buildingId) || !mSystemData.SurvivorData.buildingWorkers.ContainsKey(buildingId))
                return new List<SurvivorData>(); // 返回空列表
            
            var workerIds = mSystemData.SurvivorData.buildingWorkers[buildingId]; // 获取该建筑的工人ID列表
            // 根据工人ID列表查询并返回完整的幸存者数据列表，同时过滤掉可能已不存在的幸存者
            return workerIds.Select(id => GetSurvivor(id)).Where(s => s != null).ToList();
        }
        
        /// <summary>
        /// 检查幸存者是否能被分配到指定建筑。
        /// （基础检查，可扩展更复杂逻辑）
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="buildingId">建筑ID。</param>
        /// <returns>如果可以分配则为true，否则为false。</returns>
        public bool CanAssignToBuilding(string survivorId, string buildingId)
        {
            var survivor = GetSurvivor(survivorId);
            // 基础检查：幸存者存在且能够工作
            if (survivor == null || !survivor.CanWork)
                return false;
            
            // TODO: 此处可以根据游戏设计添加更多检查逻辑，例如：
            // 1. 建筑是否有空余工位 (mSystemData.SurvivorData.buildingWorkers[buildingId].Count < config.MaxWorkers)
            // 2. 幸存者是否满足建筑的特定属性或技能要求 (config.RequiredAttributes)
            // 3. 幸存者当前状态是否允许工作（如未受伤、未极度疲劳等）
            return true;
        }
        
        /// <summary>
        /// 升级幸存者的指定属性（通过增加经验值的方式）。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="attributeType">要升级的属性类型。</param>
        /// <returns>如果成功增加经验（可能导致升级）则为true，否则false。</returns>
        public bool UpgradeSurvivorAttribute(string survivorId, SurvivorAttributeType attributeType)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor == null) // 检查幸存者是否存在
            {
                Debug.LogWarning($"[幸存者系统] 升级属性失败：找不到ID为 {survivorId} 的幸存者。");
                return false;
            }
            
            // 示例：直接增加100点经验到指定属性，AddAttributeExperience内部应处理升级逻辑
            bool success = survivor.AddAttributeExperience(attributeType, 100);
            if (success)
            {
                Debug.Log($"[幸存者系统] 为幸存者 {survivor.Name} (ID: {survivorId}) 的属性 {attributeType} 增加了经验值。");
            }
            return success;
        }
        
        /// <summary>
        /// 获取幸存者指定属性的当前值。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="attributeType">属性类型。</param>
        /// <returns>属性值；如果幸存者或属性不存在，则返回0。</returns>
        public int GetSurvivorAttributeValue(string survivorId, SurvivorAttributeType attributeType)
        {
            var survivor = GetSurvivor(survivorId);
            // 使用空值条件运算符 ?. 和空值合并运算符 ?? 安全地获取属性值
            return survivor?.GetAttributeValue(attributeType) ?? 0;
        }
        
        /// <summary>
        /// 更新所有存活幸存者的状态（工作、士气、疲劳、饥饿等）。
        /// </summary>
        /// <param name="deltaTime">帧间隔时间。</param>
        public void UpdateSurvivorStates(float deltaTime)
        {
            foreach (var survivor in mSystemData.SurvivorData.survivors.Values) // 遍历所有幸存者
            {
                if (!survivor.IsAlive) // 只更新存活的幸存者
                    continue;
                
                UpdateWorkState(survivor, deltaTime);  // 更新工作状态和相关影响（经验、疲劳）
                UpdateMorale(survivor, deltaTime);     // 更新士气状态
                UpdateFatigue(survivor, deltaTime);    // 更新疲劳状态
                UpdateHunger(survivor, deltaTime);     // 更新饥饿状态
                // TODO: 可能还有健康状态的自然恢复或恶化逻辑
            }
            
            // 在所有个体状态更新后，重新计算并更新团队平均士气
            mSystemData.SurvivorData.UpdateTeamMorale();
            // 将计算得到的团队平均士气同步到游戏主模型中（可能用于UI显示）
            mSurvivalGameModel.Morale.Value = Mathf.RoundToInt(mSystemData.SurvivorData.teamMorale);
        }
        
        /// <summary>
        /// （辅助方法）更新单个幸存者的工作状态、经验获取和疲劳累积。
        /// </summary>
        private void UpdateWorkState(SurvivorData survivor, float deltaTime)
        {
            // 如果幸存者当前有工作且被分配到建筑
            if (survivor.CurrentJob != SurvivorJob.Idle && !string.IsNullOrEmpty(survivor.AssignedBuildingId))
            {
                // 工作时获得经验的逻辑 (示例：每分钟获得一次)
                float workTimeSinceLastExp = Time.time - survivor.WorkStartTime; // 假设WorkStartTime记录的是上次获得经验或开始工作的时间
                if (workTimeSinceLastExp > 60f) // 如果工作超过1分钟
                {
                    AddExperienceToSurvivor(survivor.Id, 10); // 增加10点通用经验
                    
                    // 根据当前从事的工作类型，增加对应属性的经验
                    SurvivorAttributeType workRelatedAttribute = GetWorkAttribute(survivor.CurrentJob);
                    survivor.AddAttributeExperience(workRelatedAttribute, 5); // 增加5点相关属性经验
                    
                    survivor.WorkStartTime = Time.time; // 重置工作开始计时（用于下次经验计算）
                }
                
                // 工作会增加疲劳值 (示例：每秒增加2点疲劳)
                survivor.Fatigue = Mathf.Min(100, survivor.Fatigue + Mathf.RoundToInt(deltaTime * 2f)); // 疲劳上限100
                
                // 如果疲劳值过高，则强制幸存者停止工作，变为空闲状态以恢复疲劳
                if (survivor.Fatigue > 80) // 疲劳阈值示例为80
                {
                    Debug.LogWarning($"[幸存者系统] 幸存者 {survivor.Name} (ID: {survivor.Id}) 因疲劳过度 ({survivor.Fatigue}) 而停止工作。");
                    // 注意：此处直接修改CurrentJob。如果UnassignSurvivorFromBuilding有其他重要逻辑（如从建筑列表移除），应调用它。
                    // 为简化，这里直接设置为空闲。在更完整系统中，可能需要一个“休息中”状态或确保正确调用Unassign。
                    survivor.CurrentJob = SurvivorJob.Idle;
                }
            }
            else // 如果幸存者当前空闲 (Idle)
            {
                // 休息时会恢复疲劳值 (示例：每秒恢复5点疲劳)
                survivor.Fatigue = Mathf.Max(0, survivor.Fatigue - Mathf.RoundToInt(deltaTime * 5f)); // 疲劳下限0
                
                // 如果疲劳已充分恢复且之前有分配的建筑，则可能自动返回工作岗位
                if (survivor.Fatigue < 20 && !string.IsNullOrEmpty(survivor.AssignedBuildingId)) // 疲劳恢复阈值为20
                {
                    // 重新获取该建筑提供的工作类型并设置回去
                    // survivor.CurrentJob = GetJobTypeForBuilding(survivor.AssignedBuildingId); // 这行会导致不停在休息和工作间切换
                    // TODO: 需要更智能的逻辑来决定何时自动复工，或依赖玩家手动分配。
                    // Debug.Log($"[幸存者系统] 幸存者 {survivor.Name} (ID: {survivor.Id}) 疲劳已恢复，可以返回工作。");
                }
            }
        }
        
        /// <summary>
        /// （辅助方法）根据工作类型获取其关联的主要属性类型（用于经验增长）。
        /// </summary>
        private SurvivorAttributeType GetWorkAttribute(SurvivorJob job)
        {
            switch (job)
            {
                case SurvivorJob.Production:   return SurvivorAttributeType.Production;
                case SurvivorJob.Defense:      return SurvivorAttributeType.Combat;
                case SurvivorJob.Research:     return SurvivorAttributeType.Research;
                case SurvivorJob.Medical:      return SurvivorAttributeType.Medical;
                case SurvivorJob.Construction: return SurvivorAttributeType.Technology; // 建造工作关联科技/工程属性
                case SurvivorJob.Exploration:  return SurvivorAttributeType.Exploration;
                case SurvivorJob.Leadership:   return SurvivorAttributeType.Leadership;
                default:                       return SurvivorAttributeType.Production; // 默认关联生产属性
            }
        }
        
        /// <summary>
        /// （辅助方法）更新单个幸存者的士气状态。
        /// </summary>
        private void UpdateMorale(SurvivorData survivor, float deltaTime)
        {
            int moraleChangeThisFrame = 0; // 本帧士气变化量
            
            // 基础士气自然变化
            if (survivor.CurrentJob != SurvivorJob.Idle)
            {
                moraleChangeThisFrame += Mathf.RoundToInt(deltaTime * 0.5f); // 工作时士气缓慢增加 (有目标和成就感)
            }
            else
            {
                moraleChangeThisFrame -= Mathf.RoundToInt(deltaTime * 0.2f); // 长期闲置士气缓慢下降 (无聊、无价值感)
            }
            
            // 健康状况对士气的影响
            if (survivor.Health < 50) // 如果健康低于50%
            {
                moraleChangeThisFrame -= Mathf.RoundToInt(deltaTime * 1f); // 士气因健康不佳而下降
            }
            
            // 饥饿状态对士气的影响
            if (survivor.Hunger > 70) // 如果饥饿度高于70%
            {
                moraleChangeThisFrame -= Mathf.RoundToInt(deltaTime * 1f); // 士气因饥饿而下降
            }
            
            // TODO: 此处还可以加入更多影响士气的因素，如：
            // - 团队平均士气的影响 (从众效应)
            // - 特定事件的临时buff/debuff
            // - 居住条件、娱乐设施等环境因素

            // 应用计算出的士气变化，并确保士气值在0到100的范围内
            survivor.Morale = Mathf.Clamp(survivor.Morale + moraleChangeThisFrame, 0, 100);
        }
        
        /// <summary>
        /// （辅助方法）更新单个幸存者的疲劳状态（主要处理休息时的自然恢复）。
        /// 工作时的疲劳累积在 UpdateWorkState 中处理。
        /// </summary>
        private void UpdateFatigue(SurvivorData survivor, float deltaTime)
        {
            // 如果幸存者当前处于空闲状态，则其疲劳度会逐渐恢复
            if (survivor.CurrentJob == SurvivorJob.Idle)
            {
                survivor.Fatigue = Mathf.Max(0, survivor.Fatigue - Mathf.RoundToInt(deltaTime * 3f)); // 示例：每秒恢复3点疲劳
            }
        }
        
        /// <summary>
        /// （辅助方法）更新单个幸存者的饥饿状态。
        /// </summary>
        private void UpdateHunger(SurvivorData survivor, float deltaTime)
        {
            // 饥饿度随时间自然增加 (示例：每秒增加0.5点饥饿)
            survivor.Hunger = Mathf.Min(100, survivor.Hunger + Mathf.RoundToInt(deltaTime * 0.5f)); // 饥饿上限100
            
            // 如果饥饿度过高，可能会对健康造成负面影响
            if (survivor.Hunger > 80) // 饥饿阈值示例为80
            {
                // 示例：每秒因饥饿损失0.5点健康 (应考虑deltaTime)
                survivor.Health = Mathf.Max(0, survivor.Health - Mathf.RoundToInt(deltaTime * 0.5f * ( (survivor.Hunger - 80)/20f ) ) ); // 饥饿越严重，健康下降越快
            }
        }
        
        /// <summary>
        /// 治疗指定的幸存者，增加其生命值。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="amount">要恢复的生命值数量。</param>
        public void HealSurvivor(string survivorId, int amount)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor != null && amount > 0) // 确保幸存者存在且治疗量为正
            {
                survivor.Health = Mathf.Min(survivor.MaxHealth, survivor.Health + amount); // 恢复生命值，但不超过最大值
                Debug.Log($"[幸存者系统] 已治疗幸存者 {survivor.Name} (ID: {survivorId})，恢复 {amount} 点生命值。当前生命值: {survivor.Health}。");
                // TODO: 发送幸存者被治疗事件
            }
        }
        
        /// <summary>
        /// 为指定的幸存者增加经验值，并处理可能的等级提升。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="experience">增加的经验值数量。</param>
        public void AddExperienceToSurvivor(string survivorId, int experience)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor != null && experience > 0) // 确保幸存者存在且经验为正
            {
                int oldLevel = survivor.Level; // 记录旧等级以判断是否升级
                survivor.Experience += experience; // 增加经验值
                
                // 检查是否满足升级条件 (示例：每升一级需要 (当前等级 * 100) 点经验)
                int requiredExpForNextLevel = SurvivorData.GetExperienceForNextLevel(survivor.Level);
                while (survivor.Experience >= requiredExpForNextLevel && survivor.Level < SurvivorData.MAX_LEVEL) // 循环处理可能的多级连升
                {
                    survivor.Experience -= requiredExpForNextLevel; // 扣除升级所需经验
                    survivor.Level++; // 等级提升
                    // survivor.SkillPoints++; // 假设升级会获得技能点 (如果游戏设计有技能点系统)
                    Debug.Log($"[幸存者系统] 幸存者 {survivor.Name} (ID: {survivorId}) 等级提升至 {survivor.Level}！");
                    OnSurvivorLevelUp?.Invoke(survivorId); // 触发等级提升事件
                    requiredExpForNextLevel = SurvivorData.GetExperienceForNextLevel(survivor.Level); // 更新下一级所需经验
                }
                // 如果经验超过当前等级上限但未达到下一级，可以将多余经验清零或保留一部分，取决于设计
                // if (survivor.Experience >= requiredExpForNextLevel && survivor.Level == SurvivorData.MAX_LEVEL) survivor.Experience = requiredExpForNextLevel -1; // 防止满级后经验溢出显示
            }
        }
        
        /// <summary>
        /// 获取幸存者系统的核心数据。
        /// </summary>
        /// <returns>SurvivorSystemData 实例。</returns>
        public SurvivorSystemData GetSystemData()
        {
            return mSystemData.SurvivorData; // 从IGameModel中获取SurvivorSystemData
        }
        
        /// <summary>
        /// 获取当前游戏中幸存者的总数（包括所有状态）。
        /// </summary>
        /// <returns>幸存者总数。</returns>
        public int GetTotalSurvivorCount()
        {
            return mSystemData.SurvivorData.survivors.Count;
        }
        
        /// <summary>
        /// 获取当前游戏中所有存活的幸存者数量。
        /// </summary>
        /// <returns>存活幸存者数量。</returns>
        public int GetAliveSurvivorCount()
        {
            return mSystemData.SurvivorData.survivors.Values.Count(s => s.IsAlive);
        }
        
        /// <summary>
        /// 获取团队的平均士气。
        /// </summary>
        /// <returns>平均士气值。</returns>
        public float GetAverageTeamMorale()
        {
            // SurvivorSystemData内部应有方法计算或直接存储teamMorale
            return mSystemData.SurvivorData.teamMorale;
        }
        
        // --- 与旧版技能系统兼容的方法 (如果需要) ---
        // 这些方法是为了保持与可能存在的旧代码或接口的兼容性，
        // 它们将旧的“技能”概念映射到新的“属性”系统。

        /// <summary>
        /// （兼容性方法）根据旧的职业枚举获取幸存者列表。
        /// 当前实现由于新系统不直接使用此职业概念进行主要分组，故返回空列表。
        /// 如果需要基于新职业的筛选，应实现新的方法或调整此方法逻辑。
        /// </summary>
        /// <param name="profession">旧的幸存者职业枚举。</param>
        /// <returns>空的幸存者列表。</returns>
        public List<SurvivorData> GetSurvivorsByProfession(SurvivorProfession profession)
        {
            // 新系统已将职业概念融入属性初始化，不再作为主要的动态筛选依据。
            // 如果确实需要按初始职业筛选，可以迭代所有幸存者并检查其 Profession 字段。
            // 为保持接口兼容性，此处返回空列表或根据需要调整。
            Debug.LogWarning("[幸存者系统] GetSurvivorsByProfession 方法是为旧版兼容保留，可能不完全符合当前属性系统设计。");
            return mSystemData.SurvivorData.survivors.Values.Where(s => s.Profession == profession).ToList();
        }
        
        /// <summary>
        /// （兼容性方法）尝试升级幸存者的“技能”，实际映射为升级对应的属性。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <param name="skill">旧的技能枚举。</param>
        /// <returns>如果对应属性的经验增加（可能导致升级）成功，则返回true。</returns>
        public bool UpgradeSurvivorSkill(string survivorId, SurvivorSkill skill)
        {
            // 将旧的技能枚举转换为新的属性类型
            var attributeType = ConvertSkillToAttribute(skill);
            // 调用属性升级方法（即增加对应属性的经验）
            return UpgradeSurvivorAttribute(survivorId, attributeType);
        }
        
        /// <summary>
        /// （兼容性方法）获取幸存者可用的“技能点”。
        /// 当前属性系统不直接使用技能点概念，故返回0。
        /// </summary>
        /// <param name="survivorId">幸存者ID。</param>
        /// <returns>0，因为新系统不使用技能点。</returns>
        public int GetAvailableSkillPoints(string survivorId)
        {
            // 新的属性系统通过经验值和等级提升属性，不直接使用“技能点”概念。
            // 为保持接口兼容性，返回0。
            return 0;
        }
        
        /// <summary>
        /// （辅助方法）将旧的技能枚举 (SurvivorSkill) 转换为新的属性类型枚举 (SurvivorAttributeType)。
        /// 用于兼容旧系统调用。
        /// </summary>
        /// <param name="skill">旧的技能枚举值。</param>
        /// <returns>对应的新属性类型枚举值。</returns>
        private SurvivorAttributeType ConvertSkillToAttribute(SurvivorSkill skill)
        {
            switch (skill) // 根据旧技能映射到新属性
            {
                case SurvivorSkill.Combat:       return SurvivorAttributeType.Combat;
                case SurvivorSkill.Construction: return SurvivorAttributeType.Technology; // 建造技能映射到科技/工程属性
                case SurvivorSkill.Medicine:     return SurvivorAttributeType.Medical;
                case SurvivorSkill.Research:     return SurvivorAttributeType.Research;
                case SurvivorSkill.Leadership:   return SurvivorAttributeType.Leadership;
                case SurvivorSkill.Gathering:    return SurvivorAttributeType.Production; // 采集/搜集技能映射到生产属性
                default:                         return SurvivorAttributeType.Production; // 默认或未知技能映射到生产
            }
        }
    }
} 