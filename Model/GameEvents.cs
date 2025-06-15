using UnityEngine;

namespace SurvivalGame.Model
{
    /// <summary>
    /// 游戏更新事件
    /// </summary>
    public struct GameUpdateEvent
    {
        public float DeltaTime;
    }

    /// <summary>
    /// 建筑建造完成事件
    /// </summary>
    public struct BuildingConstructionCompletedEvent
    {
        public string BuildingId;
        public string ConfigId;
    }

    /// <summary>
    /// 科技完成事件
    /// </summary>
    public struct TechCompletedEvent
    {
        public string TechId;
        public string TechName;
    }

    /// <summary>
    /// 制造完成事件
    /// </summary>
    public struct CraftingCompletedEvent
    {
        public string RecipeId;
        public ResourceType OutputType;
        public int OutputAmount;
    }

    /// <summary>
    /// 资源变化事件
    /// </summary>
    public struct ResourceChangedEvent
    {
        public ResourceType Type;
        public int OldAmount;
        public int NewAmount;
        public int Change;
    }

    /// <summary>
    /// 幸存者状态变化事件
    /// </summary>
    public struct SurvivorStateChangedEvent
    {
        public string SurvivorId;
        public SurvivorJob OldJob;
        public SurvivorJob NewJob;
    }

    /// <summary>
    /// 建筑状态变化事件
    /// </summary>
    public struct BuildingStateChangedEvent
    {
        public string BuildingId;
        public BuildingState OldState;
        public BuildingState NewState;
    }
} 