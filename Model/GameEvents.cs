// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：GameEvents.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了游戏中使用的各种事件结构体。这些事件用于在游戏的不同模块或系统之间
//     传递信息和通知状态变化，是实现事件驱动架构和解耦系统的重要组成部分。
//     例如，当游戏更新、建筑完成、科技研发完毕、资源数量改变等情况发生时，
//     会发送相应的事件。
// ==============================================================================

using UnityEngine; // 用于访问如Vector3等Unity特有类型，尽管此文件中当前未直接使用，但通常事件可能包含此类数据。

namespace SurvivalGame.Model
{
    /// <summary>
    /// 游戏更新事件。
    /// 通常在游戏主循环的每一帧或以固定时间间隔发送，用于驱动需要定期更新的游戏逻辑。
    /// </summary>
    public struct GameUpdateEvent
    {
        /// <summary>
        /// 当前帧或更新周期的时间增量（秒）。
        /// 对于依赖帧率的更新，这通常是 Time.deltaTime；对于固定更新，则是固定时间步长。
        /// </summary>
        public float DeltaTime;
    }

    /// <summary>
    /// 建筑建造完成事件。
    /// 当一个建筑的建造过程结束时发送。
    /// </summary>
    public struct BuildingConstructionCompletedEvent
    {
        /// <summary>
        /// 完成建造的建筑实例的唯一ID。
        /// </summary>
        public string BuildingId;
        /// <summary>
        /// 完成建造的建筑的配置ID (例如："farm_level1")。
        /// </summary>
        public string ConfigId;
    }

    /// <summary>
    /// 科技研究完成事件。
    /// 当一项科技的研究达到100%进度并成功解锁时发送。
    /// </summary>
    public struct TechCompletedEvent
    {
        /// <summary>
        /// 完成研究的科技的唯一ID。
        /// </summary>
        public string TechId;
        /// <summary>
        /// 完成研究的科技的显示名称。
        /// </summary>
        public string TechName;
    }

    /// <summary>
    /// 物品制造完成事件。
    /// 当一个制造队列中的物品成功制造出来时发送。
    /// </summary>
    public struct CraftingCompletedEvent
    {
        /// <summary>
        /// 完成制造的配方的ID。
        /// </summary>
        public string RecipeId;
        /// <summary>
        /// 产出物品的资源类型。
        /// </summary>
        public ResourceType OutputType;
        /// <summary>
        /// 本次制造成功产出的物品数量。
        /// </summary>
        public int OutputAmount;
    }

    /// <summary>
    /// 资源数量变化事件。
    /// 当任何一种受监控的资源数量发生增减时发送。
    /// </summary>
    public struct ResourceChangedEvent
    {
        /// <summary>
        /// 发生变化的资源类型。
        /// </summary>
        public ResourceType Type;
        /// <summary>
        /// 变化前的资源数量。
        /// </summary>
        public int OldAmount;
        /// <summary>
        /// 变化后的资源数量。
        /// </summary>
        public int NewAmount;
        /// <summary>
        /// 本次变化的量（NewAmount - OldAmount，正数为增加，负数为减少）。
        /// </summary>
        public int Change;
    }

    /// <summary>
    /// 幸存者状态或工作变化事件。
    /// 当一个幸存者的当前工作或主要状态发生改变时发送。
    /// 注意：字段名用的是Job，但可能也指代更广泛的状态，具体取决于SurvivorJob枚举的定义。
    /// </summary>
    public struct SurvivorStateChangedEvent
    {
        /// <summary>
        /// 状态发生变化的幸存者的唯一ID。
        /// </summary>
        public string SurvivorId;
        /// <summary>
        /// 幸存者变化前的工作或状态 (SurvivorJob类型)。
        /// </summary>
        public SurvivorJob OldJob; // 或 OldState
        /// <summary>
        /// 幸存者变化后的新工作或状态 (SurvivorJob类型)。
        /// </summary>
        public SurvivorJob NewJob; // 或 NewState
    }

    /// <summary>
    /// 建筑状态变化事件。
    /// 当一个建筑的运行状态（如从建造中变为运行中，或从运行中变为损坏）发生改变时发送。
    /// </summary>
    public struct BuildingStateChangedEvent
    {
        /// <summary>
        /// 状态发生变化的建筑实例的唯一ID。
        /// </summary>
        public string BuildingId;
        /// <summary>
        /// 建筑变化前的状态 (BuildingState类型)。
        /// </summary>
        public BuildingState OldState;
        /// <summary>
        /// 建筑变化后的新状态 (BuildingState类型)。
        /// </summary>
        public BuildingState NewState;
    }
}