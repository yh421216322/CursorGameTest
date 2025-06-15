// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：NewCommand.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个通用的“新建”命令。
//     它是一个基础命令模板，可以根据具体需求进行扩展和实现。
// ==============================================================================

using QFramework;
using UnityEngine; // 通常命令会与Unity引擎的某些部分交互，故保留

namespace SurvivalGame.Command
{
    /// <summary>
    /// 新建命令类 (通用模板)
    /// 可以根据此模板创建具体的命令，例如 "CreateNewItemCommand", "StartNewGameCommand" 等。
    /// </summary>
    public class NewCommand : AbstractCommand
    {
        /// <summary>
        /// 命令执行方法
        /// 当此命令被发送 (SendCommand) 时，此方法会被调用。
        /// </summary>
        protected override void OnExecute()
        {
            // 在这里添加具体的命令执行逻辑。
            // 例如：
            // 1. 获取系统 (this.GetSystem<ISomeSystem>())
            // 2. 获取模型 (this.GetModel<ISomeModel>())
            // 3. 执行操作 (system.DoSomething(), model.UpdateValue = newValue)
            // 4. 发送事件 (this.SendEvent<SomeEvent>())
            // 5. 记录日志 (Debug.Log("NewCommand executed"))

            Debug.Log("NewCommand 已执行，但未实现具体逻辑。请在 OnExecute 方法中添加实现。");
        }
    }
}