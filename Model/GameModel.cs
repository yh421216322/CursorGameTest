// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：GameModel.cs // 根据实际文件名，如果IDE或工具已将其重命名为 SurvivalGameModel.cs，应相应调整
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了游戏的核心数据模型接口 (IGameModel) 及其具体实现 (GameModel)。
//     该模型遵循QFramework框架的规范，用于存储和管理游戏全局的、可绑定的状态数据，
//     例如特定的游戏计数器（如SkyBlazeSphere）和更复杂的数据结构（如SurvivorSystemData）。
//     (注意：实际文件内容可能与此描述有出入，特别是关于 ISurvivalGameModel 的具体字段，
//      此注释基于对 GameModel.cs 文件的直接分析。)
// ==============================================================================

using QFramework;
using SurvivalGame.Model; // 假设 SurvivorSystemData 在这个命名空间

namespace SurvivalGame // 保持原始命名空间 SurvivalGame
{
    /// <summary>
    /// 游戏核心数据模型接口。
    /// 继承自QFramework的QFIModel，定义了游戏全局状态属性。
    /// 其他系统或UI可以通过此接口访问和监听模型数据的变化。
    /// </summary>
    public interface IGameModel : QFIModel // QFIModel是QFramework中模型接口的基类
    {
        /// <summary>
        /// 一个可绑定的整型属性示例，名为 SkyBlazeSphere。
        /// BindableProperty是QFramework提供的特性，允许UI或其他逻辑单元订阅其值的变化。
        /// </summary>
        BindableProperty<int> SkyBlazeSphere { get; set; }

        /// <summary>
        /// 幸存者系统的数据聚合对象。
        /// 包含了与幸存者管理相关的所有状态和数据。
        /// </summary>
        SurvivorSystemData SurvivorData { get; set; }
    }

    /// <summary>
    /// 游戏核心数据模型的具体实现类。
    /// 继承自QFramework的AbstractModel，并实现了IGameModel接口。
    /// </summary>
    public class GameModel : AbstractModel, IGameModel
    {
        // // public BindableProperty<int> SkyBlazeSphere { get; set; } // 此为早期或重复的声明，已被下方正确的属性实现所替代。

        /// <summary>
        /// 幸存者系统的数据聚合对象实例。
        /// 实现了IGameModel接口中定义的SurvivorData属性。
        /// </summary>
        public SurvivorSystemData SurvivorData { get; set; }
      
        /// <summary>
        /// QFramework模型初始化方法。
        /// 在模型首次被获取时调用，用于设置其属性的初始值。
        /// </summary>
        protected override void OnInit()
        {
            // 初始化SkyBlazeSphere为一个新的BindableProperty<int>实例，初始值为0。
            SkyBlazeSphere = new BindableProperty<int>(0);
            
            // 初始化SurvivorData为一个新的SurvivorSystemData实例。
            // SurvivorSystemData内部的默认值将在其自身的构造函数中设置。
            SurvivorData = new SurvivorSystemData();
        }

        /// <summary>
        /// SkyBlazeSphere属性的具体实现。
        /// 这是一个可绑定的整型属性，用于存储某种游戏特定的计数或状态值。
        /// </summary>
        public BindableProperty<int> SkyBlazeSphere { get; set; }
    }
}