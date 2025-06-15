// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：SurvivorAttributeType.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了与幸存者属性（或技能）相关的枚举和数据结构。
//     包括幸存者可以拥有的各种属性类型 (SurvivorAttributeType)，
//     单个属性的动态数据 (SurvivorAttributeData)，如当前值、经验、等级等，
//     以及一个静态辅助类 (AttributeLevelHelper) 用于根据属性值获取描述性名称、
//     UI颜色和效率乘数。
// ==============================================================================

using System;
using System.Collections.Generic; // 尽管当前未使用，但通常模型类可能需要
using UnityEngine; // 用于 Mathf.Min 和 Color 等Unity特定类型

namespace SurvivalGame.Model
{
    /// <summary>
    /// 幸存者拥有的各种属性或技能的类型枚举。
    /// 这些属性决定了幸存者在不同工作或任务中的表现。
    /// </summary>
    public enum SurvivorAttributeType
    {
        /// <summary>
        /// 生产力：影响资源生产、物品制造等工作的效率。
        /// </summary>
        Production,     // 生产力
        /// <summary>
        /// 战斗力：影响在战斗中的攻击、防御等表现。
        /// </summary>
        Combat,         // 战斗力
        /// <summary>
        /// 技术力：可能影响建造速度、设备维修、解锁高级配方等。
        /// </summary>
        Technology,     // 技术力 / 工程能力
        /// <summary>
        /// 医疗力：影响治疗伤员的速度和效果。
        /// </summary>
        Medical,        // 医疗能力
        /// <summary>
        /// 科研力：影响科技研发的速度和效率。
        /// </summary>
        Research,       // 科研能力
        /// <summary>
        /// 探索力：影响外出探索任务的成功率、获取稀有资源的几率等。
        /// </summary>
        Exploration,    // 探索能力
        /// <summary>
        /// 领导力：可能影响团队士气、管理效率或提供团队增益效果。
        /// </summary>
        Leadership      // 领导力
    }

    /// <summary>
    /// 存储幸存者单个属性（或技能）的动态数据。
    /// 包括属性类型、当前数值、经验值、等级以及成长速率。
    /// </summary>
    [Serializable] // 标记为可序列化，以便能在Unity Inspector中显示或保存到文件
    public class SurvivorAttributeData
    {
        /// <summary>
        /// 此属性数据的类型 (参考 SurvivorAttributeType 枚举)。
        /// </summary>
        public SurvivorAttributeType Type;
        /// <summary>
        /// 属性的当前数值 (通常范围在0到100之间)。
        /// </summary>
        public int Value;               // 当前值（例如：0-100）
        /// <summary>
        /// 当前属性等级下积累的经验值。
        /// </summary>
        public int Experience;          // 当前经验值
        /// <summary>
        /// 属性的当前等级。等级可能随属性值Value的提升而提升。
        /// </summary>
        public int Level;               // 当前属性等级
        /// <summary>
        /// 此属性的成长速率或学习效率的乘数 (例如1.0f代表正常速率)。
        /// </summary>
        public float GrowthRate;        // 成长速率 / 学习效率乘数
        
        /// <summary>
        /// SurvivorAttributeData的构造函数。
        /// </summary>
        /// <param name="type">要创建的属性的类型。</param>
        /// <param name="initialValue">属性的初始数值，默认为20。</param>
        public SurvivorAttributeData(SurvivorAttributeType type, int initialValue = 20)
        {
            Type = type;
            Value = Mathf.Clamp(initialValue, 0, 100); // 确保初始值在0-100范围内
            Experience = 0;           // 初始经验为0
            Level = 1;                // 初始等级为1 (或根据Value计算初始等级)
            GrowthRate = 1.0f;        // 默认正常成长速率
        }
        
        /// <summary>
        /// 为此属性添加指定的经验值。
        /// 如果经验值达到升级所需，则提升属性值 (Value) 和可能的等级 (Level)。
        /// </summary>
        /// <param name="exp">要添加的经验值数量 (应为正数)。</param>
        /// <returns>如果属性值 (Value) 因此次经验增加而提升，则返回true；否则返回false。</returns>
        public bool AddExperience(int exp)
        {
            if (exp <= 0) return false; // 不处理负或零经验值

            Experience += exp; // 累加经验值
            int requiredExp = GetRequiredExperience(); // 获取当前提升1点Value所需的经验
            
            // 检查是否满足升级条件：经验足够 且 属性值尚未达到上限 (100)
            if (Experience >= requiredExp && Value < 100)
            {
                Experience -= requiredExp; // 扣除升级所需的经验
                Value = Mathf.Min(100, Value + 1); // 属性值提升1点，但不超过100

                // 简单示例：每当Value达到10的倍数时，等级提升 (实际等级系统可能更复杂)
                if (Value % 10 == 0 && Level < (Value/10)) // 确保等级与Value匹配且不超过理论上限
                {
                    Level++;
                }
                return true; // 属性值提升，返回true
            }
            return false; // 未提升属性值，返回false
        }
        
        /// <summary>
        /// (私有辅助方法) 计算当前属性值 (Value) 下，提升1点Value所需的经验值。
        /// 升级难度随属性值的增高而增加。
        /// </summary>
        /// <returns>提升1点Value所需的经验值。</returns>
        private int GetRequiredExperience()
        {
            // 示例公式：基础100经验 + 当前属性值 * 10
            // 例如：Value为20时，需100+200=300经验；Value为80时，需100+800=900经验。
            return 100 + (Value * 10);
        }
    }

    /// <summary>
    /// 幸存者属性等级的静态辅助类。
    /// 提供基于属性数值获取描述性名称、UI颜色和效率乘数等功能。
    /// </summary>
    public static class AttributeLevelHelper
    {
        /// <summary>
        /// 根据属性的数值获取其对应的等级描述名称 (例如："新手级", "大师级")。
        /// </summary>
        /// <param name="value">属性的当前数值 (0-100)。</param>
        /// <returns>该数值对应的等级描述字符串。</returns>
        public static string GetLevelName(int value)
        {
            if (value <= 20) return "新手"; // 0-20
            if (value <= 40) return "熟练"; // 21-40
            if (value <= 60) return "专业"; // 41-60
            if (value <= 80) return "专家"; // 61-80
            return "大师"; // 81-100
        }
        
        /// <summary>
        /// 根据属性的数值获取用于UI显示的推荐颜色。
        /// 等级越高，颜色可能越醒目或不同。
        /// </summary>
        /// <param name="value">属性的当前数值 (0-100)。</param>
        /// <returns>对应数值范围的颜色。</returns>
        public static Color GetLevelColor(int value)
        {
            if (value <= 20) return Color.gray;   // 新手级 - 灰色
            if (value <= 40) return Color.white;  // 熟练级 - 白色 (或浅蓝色等)
            if (value <= 60) return Color.green;  // 专业级 - 绿色
            if (value <= 80) return Color.blue;   // 专家级 - 蓝色
            return Color.yellow; // 大师级 - 黄色 (或金色、紫色等)
        }
        
        /// <summary>
        /// 根据属性的数值计算其提供的效率乘数。
        /// 例如，用于计算工作效率、战斗力加成等。
        /// </summary>
        /// <param name="value">属性的当前数值。</param>
        /// <returns>基于属性值的效率乘数 (例如，1.0f代表100%基础效率)。</returns>
        public static float GetEfficiencyMultiplier(int value)
        {
            // 示例公式：基础效率100% + 每点属性值额外增加1%效率
            return 1.0f + (value * 0.01f);
        }
    }
}