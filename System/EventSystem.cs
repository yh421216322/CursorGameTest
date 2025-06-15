using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using MyGameNamespace;
using SurvivalGame.Model;
using GameEventType = MyGameNamespace.EventType;

namespace SurvivalGame.GameSystem
{
    /// <summary>
    /// 随机事件系统接口
    /// </summary>
    public interface IEventSystem : QFISystem
    {
        void TriggerRandomEvent();
        void TriggerSpecificEvent(string eventId);
        List<RandomEvent> GetAvailableEvents();
        void ProcessEvent(RandomEvent randomEvent);
    }
    
    /// <summary>
    /// 随机事件系统
    /// 负责触发各种随机事件，增加游戏的不确定性和挑战性
    /// </summary>
    public class EventSystem : AbstractSystem, IEventSystem
    {
        private ISurvivalGameModel mGameModel;
        private IResourceSystem mResourceSystem;
        //private IBuildingSystem mBuildingSystem;
        private IAdvancedTechSystem mTechSystem;
        
        // 事件管理
        private Dictionary<string, RandomEvent> mEvents;
        private List<string> mTriggeredEvents; // 已触发的一次性事件
        
        // 系统参数
        private const float EVENT_CHECK_INTERVAL = 30f; // 30秒检查一次事件
        private const float BASE_EVENT_CHANCE = 0.15f; // 基础事件概率 15%
        
        private float mLastEventCheckTime = 0f;
        
        protected override void OnInit()
        {
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mResourceSystem = this.GetSystem<IResourceSystem>();
            //mBuildingSystem = this.GetSystem<IBuildingSystem>();
            mTechSystem = this.GetSystem<IAdvancedTechSystem>();
            
            // 初始化容器
            mEvents = new Dictionary<string, RandomEvent>();
            mTriggeredEvents = new List<string>();
            
            // 初始化事件库
            InitializeEvents();
            
            // 注册事件
            this.RegisterEvent<TimeUpdateEvent>(OnTimeUpdate);
            this.RegisterEvent<NewDayEvent>(OnNewDay);
            
          //  Debug.Log("随机事件系统初始化完成");
        }
        
        private void InitializeEvents()
        {
            // === 资源类事件 ===
            CreateEvent("trader_arrival", "商人到访", "一位商人来到了你的避难所，愿意交易资源。",
                GameEventType.Positive, EventCategory.Resource, false, 1, 50,
                new Dictionary<string, object>
                {
                    {"choice_1", "用10食物换15建材"},
                    {"choice_1_cost", new Dictionary<ResourceType, int> { { ResourceType.Food, 10 } }},
                    {"choice_1_reward", new Dictionary<ResourceType, int> { { ResourceType.Materials, 15 } }},
                    {"choice_2", "用8建材换12弹药"},
                    {"choice_2_cost", new Dictionary<ResourceType, int> { { ResourceType.Materials, 8 } }},
                    {"choice_2_reward", new Dictionary<ResourceType, int> { { ResourceType.Ammunition, 12 } }},
                    {"choice_3", "拒绝交易"}
                });
                
            CreateEvent("resource_discovery", "资源发现", "探索队在废墟中发现了一批物资！",
                GameEventType.Positive, EventCategory.Resource, false, 1, 30,
                new Dictionary<string, object>
                {
                    {"reward_food", 8},
                    {"reward_materials", 5},
                    {"reward_ammo", 3}
                });
                
            CreateEvent("supply_drop", "空投补给", "一个军用补给箱从天而降！",
                GameEventType.Positive, EventCategory.Resource, false, 1, 25,
                new Dictionary<string, object>
                {
                    {"reward_food", 15},
                    {"reward_ammo", 10},
                    {"morale_bonus", 10}
                });
            
            // === 负面事件 ===
            CreateEvent("food_spoilage", "食物腐坏", "由于保存不当，部分食物腐坏了。",
                GameEventType.Negative, EventCategory.Resource, false, 2, 35,
                new Dictionary<string, object>
                {
                    {"food_loss_percentage", 0.2f}, // 损失20%食物
                    {"morale_penalty", -5}
                });
                
            CreateEvent("equipment_breakdown", "设备故障", "一些生产设备出现故障，需要修理。",
                GameEventType.Negative, EventCategory.Building, false, 2, 40,
                new Dictionary<string, object>
                {
                    {"repair_cost", 8},
                    {"production_penalty_days", 2}
                });
                
            CreateEvent("disease_outbreak", "疾病爆发", "避难所内出现疾病，需要隔离治疗。",
                GameEventType.Negative, EventCategory.Population, false, 3, 30,
                new Dictionary<string, object>
                {
                    {"population_loss", 2},
                    {"morale_penalty", -15},
                    {"choice_1", "使用5弹药强化医疗"},
                    {"choice_1_cost_ammo", 5},
                    {"choice_1_reduce_loss", 1},
                    {"choice_2", "听天由命"}
                });
            
            // === 机遇类事件 ===
            CreateEvent("scientist_joins", "科学家加入", "一位流浪的科学家请求加入你的团队。",
                GameEventType.Positive, EventCategory.Population, true, 1, 20,
                new Dictionary<string, object>
                {
                    {"population_bonus", 1},
                    {"scientist_bonus", 1},
                    {"research_speed_bonus_days", 5},
                    {"morale_bonus", 8}
                });
                
            CreateEvent("engineer_arrival", "工程师到来", "一名经验丰富的工程师愿意分享建造技巧。",
                GameEventType.Positive, EventCategory.Building, true, 1, 15,
                new Dictionary<string, object>
                {
                    {"building_speed_bonus_days", 3},
                    {"building_cost_reduction", 0.25f},
                    {"morale_bonus", 5}
                });
            
            // === 选择类事件 ===
            CreateEvent("refugee_group", "难民团体", "一群难民请求加入避难所，但他们消耗资源较多。",
                GameEventType.Choice, EventCategory.Population, false, 2, 45,
                new Dictionary<string, object>
                {
                    {"choice_1", "接纳所有人"},
                    {"choice_1_population", 4},
                    {"choice_1_food_consumption", 1.5f},
                    {"choice_1_morale", 10},
                    {"choice_2", "只接纳有技能的人"},
                    {"choice_2_population", 2},
                    {"choice_2_morale", 5},
                    {"choice_3", "拒绝接纳"},
                    {"choice_3_morale", -5}
                });
                
            CreateEvent("old_bunker", "废弃掩体", "发现了一个废弃的军事掩体，是否派人探索？",
                GameEventType.Choice, EventCategory.Exploration, false, 2, 35,
                new Dictionary<string, object>
                {
                    {"choice_1", "立即探索"},
                    {"choice_1_success_rate", 0.7f},
                    {"choice_1_reward_materials", 20},
                    {"choice_1_reward_ammo", 15},
                    {"choice_1_risk_population", 1},
                    {"choice_2", "准备充分后探索"},
                    {"choice_2_cost_ammo", 5},
                    {"choice_2_success_rate", 0.9f},
                    {"choice_2_reward_materials", 25},
                    {"choice_2_reward_ammo", 20},
                    {"choice_3", "放弃探索"}
                });
            
            // === 天气事件 ===
            CreateEvent("harsh_winter", "严冬来临", "异常寒冷的天气增加了食物消耗。",
                GameEventType.Negative, EventCategory.Weather, false, 3, 25,
                new Dictionary<string, object>
                {
                    {"duration_days", 3},
                    {"food_consumption_multiplier", 1.5f},
                    {"morale_penalty", -10}
                });
                
            CreateEvent("abundant_rain", "充沛雨水", "连续的降雨有利于农作物生长。",
                GameEventType.Positive, EventCategory.Weather, false, 1, 30,
                new Dictionary<string, object>
                {
                    {"duration_days", 2},
                    {"farm_production_multiplier", 1.4f},
                    {"morale_bonus", 5}
                });
                
           // Debug.Log($"随机事件库初始化完成，共载入 {mEvents.Count} 个事件");
        }
        
        private void CreateEvent(string id, string name, string description, GameEventType type, 
            EventCategory category, bool isOneTime, int minDay, int baseChance, 
            Dictionary<string, object> parameters)
        {
            var eventData = new RandomEvent
            {
                Id = id,
                Name = name,
                Description = description,
                Type = type,
                Category = category,
                IsOneTime = isOneTime,
                MinDay = minDay,
                BaseChance = baseChance,
                Parameters = parameters
            };
            
            mEvents[id] = eventData;
        }
        
        private void OnTimeUpdate(TimeUpdateEvent e)
        {
            if (Time.time - mLastEventCheckTime < EVENT_CHECK_INTERVAL)
                return;
                
            mLastEventCheckTime = Time.time;
            
            // 检查是否触发随机事件
            CheckForRandomEvents();
        }
        
        private void OnNewDay(NewDayEvent e)
        {
            // 新的一天，增加事件触发机会
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f) // 30%概率在新一天触发事件
            {
                TriggerRandomEvent();
            }
        }
        
        private void CheckForRandomEvents()
        {
            if (UnityEngine.Random.Range(0f, 1f) < BASE_EVENT_CHANCE)
            {
                TriggerRandomEvent();
            }
        }
        
        public void TriggerRandomEvent()
        {
            var availableEvents = GetAvailableEvents();
            
            if (availableEvents.Count == 0)
            {
                Debug.Log("没有可用的随机事件");
                return;
            }
            
            // 根据权重选择事件
            var selectedEvent = SelectEventByWeight(availableEvents);
            ProcessEvent(selectedEvent);
        }
        
        public void TriggerSpecificEvent(string eventId)
        {
            if (mEvents.ContainsKey(eventId))
            {
                ProcessEvent(mEvents[eventId]);
            }
        }
        
        public List<RandomEvent> GetAvailableEvents()
        {
            var availableEvents = new List<RandomEvent>();
            int currentDay = mGameModel.GameDay.Value;
            
            foreach (var eventData in mEvents.Values)
            {
                // 检查最小天数要求
                if (currentDay < eventData.MinDay) continue;
                
                // 检查一次性事件是否已触发
                if (eventData.IsOneTime && mTriggeredEvents.Contains(eventData.Id)) continue;
                
                // 检查其他条件
                if (CheckEventConditions(eventData))
                {
                    availableEvents.Add(eventData);
                }
            }
            
            return availableEvents;
        }
        
        private bool CheckEventConditions(RandomEvent eventData)
        {
            // 根据事件类型检查特定条件
            switch (eventData.Category)
            {
                case EventCategory.Resource:
                    // 资源事件需要有一定的资源基础
                    return mGameModel.Food.Value > 5 || mGameModel.Materials.Value > 5;
                    
                case EventCategory.Building:
                    // 建筑事件需要有建筑
                    return mGameModel.Buildings.Count > 0;
                    
                case EventCategory.Population:
                    // 人口事件需要有一定人口
                    return mGameModel.Population.Value > 1;
                    
                case EventCategory.Exploration:
                    // 探索事件需要有一定实力
                    return mGameModel.Population.Value >= 3 && mGameModel.Ammunition.Value >= 5;
                    
                default:
                    return true;
            }
        }
        
        private RandomEvent SelectEventByWeight(List<RandomEvent> events)
        {
            // 根据基础概率权重选择
            float totalWeight = 0f;
            foreach (var evt in events)
            {
                totalWeight += evt.BaseChance;
            }
            
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var evt in events)
            {
                currentWeight += evt.BaseChance;
                if (randomValue <= currentWeight)
                {
                    return evt;
                }
            }
            
            return events[events.Count - 1]; // 备选方案
        }
        
        public void ProcessEvent(RandomEvent eventData)
        {
            Debug.Log($"触发随机事件：{eventData.Name} - {eventData.Description}");
            
            // 标记一次性事件为已触发
            if (eventData.IsOneTime)
            {
                mTriggeredEvents.Add(eventData.Id);
            }
            
            // 发送事件给UI系统显示
            this.SendEvent(new RandomEventTriggeredEvent
            {
                EventData = eventData
            });
            
            // 根据事件类型自动处理或等待玩家选择
            if (eventData.Type != GameEventType.Choice)
            {
                ApplyEventEffects(eventData, null);
            }
        }
        
        public void ApplyEventEffects(RandomEvent eventData, string choiceId = null)
        {
            var parameters = eventData.Parameters;
            
            // 处理选择相关的参数
            if (!string.IsNullOrEmpty(choiceId))
            {
                ProcessChoice(eventData, choiceId);
                return;
            }
            
            // 直接效果事件
            ProcessDirectEffects(parameters);
            
            // 发送事件完成
            this.SendEvent(new RandomEventCompletedEvent
            {
                EventId = eventData.Id,
                EventName = eventData.Name
            });
        }
        
        private void ProcessChoice(RandomEvent eventData, string choiceId)
        {
            var parameters = eventData.Parameters;
            
            // 处理选择的代价和奖励
            string costKey = $"{choiceId}_cost";
            string rewardKey = $"{choiceId}_reward";
            
            // 处理资源代价
            if (parameters.ContainsKey(costKey))
            {
                var costs = (Dictionary<ResourceType, int>)parameters[costKey];
                foreach (var cost in costs)
                {
                    mGameModel.ConsumeResource(cost.Key, cost.Value);
                }
            }
            
            // 处理奖励
            if (parameters.ContainsKey(rewardKey))
            {
                var rewards = (Dictionary<ResourceType, int>)parameters[rewardKey];
                foreach (var reward in rewards)
                {
                    mGameModel.AddResource(reward.Key, reward.Value);
                }
            }
            
            // 处理其他选择特定的效果
            ProcessChoiceSpecificEffects(eventData, choiceId);
        }
        
        private void ProcessChoiceSpecificEffects(RandomEvent eventData, string choiceId)
        {
            var parameters = eventData.Parameters;
            
            // 人口变化
            string popKey = $"{choiceId}_population";
            if (parameters.ContainsKey(popKey))
            {
                int popChange = (int)parameters[popKey];
                mGameModel.Population.Value += popChange;
            }
            
            // 士气变化
            string moraleKey = $"{choiceId}_morale";
            if (parameters.ContainsKey(moraleKey))
            {
                int moraleChange = (int)parameters[moraleKey];
                mGameModel.Morale.Value += moraleChange;
            }
            
            // 风险处理（如探索风险）
            string riskKey = $"{choiceId}_risk_population";
            if (parameters.ContainsKey(riskKey))
            {
                float successRate = parameters.ContainsKey($"{choiceId}_success_rate") ? 
                    (float)parameters[$"{choiceId}_success_rate"] : 0.5f;
                    
                if (UnityEngine.Random.Range(0f, 1f) > successRate)
                {
                    // 失败，承受风险
                    int popLoss = (int)parameters[riskKey];
                    mGameModel.Population.Value -= popLoss;
                    Debug.Log($"探索失败！损失 {popLoss} 人口");
                }
                else
                {
                    // 成功，获得奖励
                    ProcessChoiceRewards(eventData, choiceId);
                }
            }
        }
        
        private void ProcessChoiceRewards(RandomEvent eventData, string choiceId)
        {
            var parameters = eventData.Parameters;
            
            // 材料奖励
            string materialKey = $"{choiceId}_reward_materials";
            if (parameters.ContainsKey(materialKey))
            {
                int materials = (int)parameters[materialKey];
                mGameModel.AddResource(ResourceType.Materials, materials);
            }
            
            // 弹药奖励
            string ammoKey = $"{choiceId}_reward_ammo";
            if (parameters.ContainsKey(ammoKey))
            {
                int ammo = (int)parameters[ammoKey];
                mGameModel.AddResource(ResourceType.Ammunition, ammo);
            }
        }
        
        private void ProcessDirectEffects(Dictionary<string, object> parameters)
        {
            // 资源奖励
            ProcessResourceRewards(parameters);
            
            // 资源损失
            ProcessResourcePenalties(parameters);
            
            // 士气变化
            if (parameters.ContainsKey("morale_bonus"))
            {
                int bonus = (int)parameters["morale_bonus"];
                mGameModel.Morale.Value += bonus;
            }
            
            if (parameters.ContainsKey("morale_penalty"))
            {
                int penalty = (int)parameters["morale_penalty"];
                mGameModel.Morale.Value += penalty; // penalty应该是负数
            }
            
            // 人口变化
            if (parameters.ContainsKey("population_bonus"))
            {
                int bonus = (int)parameters["population_bonus"];
                mGameModel.Population.Value += bonus;
            }
            
            if (parameters.ContainsKey("population_loss"))
            {
                int loss = (int)parameters["population_loss"];
                mGameModel.Population.Value -= loss;
            }
        }
        
        private void ProcessResourceRewards(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("reward_food"))
            {
                mGameModel.AddResource(ResourceType.Food, (int)parameters["reward_food"]);
            }
            
            if (parameters.ContainsKey("reward_materials"))
            {
                mGameModel.AddResource(ResourceType.Materials, (int)parameters["reward_materials"]);
            }
            
            if (parameters.ContainsKey("reward_ammo"))
            {
                mGameModel.AddResource(ResourceType.Ammunition, (int)parameters["reward_ammo"]);
            }
        }
        
        private void ProcessResourcePenalties(Dictionary<string, object> parameters)
        {
            // 食物腐坏
            if (parameters.ContainsKey("food_loss_percentage"))
            {
                float lossRate = (float)parameters["food_loss_percentage"];
                int foodLoss = Mathf.RoundToInt(mGameModel.Food.Value * lossRate);
                mGameModel.ConsumeResource(ResourceType.Food, foodLoss);
                Debug.Log($"食物腐坏损失：{foodLoss}");
            }
        }
        

    }
}

// 随机事件相关数据结构和事件
namespace MyGameNamespace
{
    public enum EventType
    {
        Positive,  // 正面事件
        Negative,  // 负面事件
        Neutral,   // 中性事件
        Choice     // 选择事件
    }
    
    public enum EventCategory
    {
        Resource,     // 资源相关
        Building,     // 建筑相关
        Population,   // 人口相关
        Weather,      // 天气相关
        Exploration,  // 探索相关
        Combat,       // 战斗相关
        Technology    // 科技相关
    }
    
    public struct RandomEventTriggeredEvent
    {
        public RandomEvent EventData;
    }
    
    public struct RandomEventCompletedEvent
    {
        public string EventId;
        public string EventName;
    }
    
    /// <summary>
    /// 随机事件数据类
    /// </summary>
    [System.Serializable]
    public class RandomEvent
    {
        public string Id;
        public string Name;
        public string Description;
        public EventType Type;
        public EventCategory Category;
        public bool IsOneTime;
        public int MinDay;
        public int BaseChance;
        public Dictionary<string, object> Parameters;
        public List<string> Choices;
        
        public RandomEvent()
        {
            Parameters = new Dictionary<string, object>();
            Choices = new List<string>();
        }
        
        public RandomEvent(string id, string name, string description, EventType type, 
            EventCategory category, bool isOneTime, int minDay, int baseChance, 
            Dictionary<string, object> parameters)
        {
            Id = id;
            Name = name;
            Description = description;
            Type = type;
            Category = category;
            IsOneTime = isOneTime;
            MinDay = minDay;
            BaseChance = baseChance;
            Parameters = parameters ?? new Dictionary<string, object>();
            Choices = new List<string>();
        }
    }
} 