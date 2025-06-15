using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 幸存者属性类型
    /// </summary>
    public enum SurvivorAttributeType
    {
        Production,     // 生产力
        Combat,         // 战斗力
        Technology,     // 技术力
        Medical,        // 医疗力
        Research,       // 科研力
        Exploration,    // 探索力
        Leadership      // 领导力
    }

    /// <summary>
    /// 幸存者属性数据
    /// </summary>
    [Serializable]
    public class SurvivorAttributeData
    {
        public SurvivorAttributeType Type;
        public int Value;               // 当前值（0-100）
        public int Experience;          // 经验值
        public int Level;               // 等级
        public float GrowthRate;        // 成长速率
        
        public SurvivorAttributeData(SurvivorAttributeType type, int initialValue = 20)
        {
            Type = type;
            Value = initialValue;
            Experience = 0;
            Level = 1;
            GrowthRate = 1.0f;
        }
        
        /// <summary>
        /// 添加经验值
        /// </summary>
        public bool AddExperience(int exp)
        {
            Experience += exp;
            int requiredExp = GetRequiredExperience();
            
            if (Experience >= requiredExp && Value < 100)
            {
                Experience -= requiredExp;
                Value = Mathf.Min(100, Value + 1);
                if (Value % 10 == 0) Level++;
                return true; // 升级了
            }
            return false;
        }
        
        /// <summary>
        /// 获取升级所需经验
        /// </summary>
        private int GetRequiredExperience()
        {
            return 100 + (Value * 10); // 属性越高，升级越难
        }
    }

    /// <summary>
    /// 幸存者属性等级描述
    /// </summary>
    public static class AttributeLevelHelper
    {
        public static string GetLevelName(int value)
        {
            if (value <= 20) return "新手级";
            if (value <= 40) return "熟练级";
            if (value <= 60) return "专业级";
            if (value <= 80) return "专家级";
            return "大师级";
        }
        
        public static Color GetLevelColor(int value)
        {
            if (value <= 20) return Color.gray;
            if (value <= 40) return Color.white;
            if (value <= 60) return Color.green;
            if (value <= 80) return Color.blue;
            return Color.yellow;
        }
        
        public static float GetEfficiencyMultiplier(int value)
        {
            return 1.0f + (value * 0.01f); // 每点属性增加1%效率
        }
    }
} 