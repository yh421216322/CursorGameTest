// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：TestCommand.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件包含用于测试QFramework命令系统及其他相关功能的命令。
//     包括一个基础测试命令 (TestCommand) 和一个高级测试命令 (AdvancedTestCommand)，
//     以及一个相关的测试事件 (TestExecutedEvent)。
// ==============================================================================

using QFramework;
using MyGameNamespace; // 包含 TestExecutedEvent 事件定义
using UnityEngine;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 测试命令
    /// 功能：用于测试QFramework命令系统的基础功能，如参数传递、模型/系统获取、事件发送等。
    /// 用途：开发和调试时验证命令执行流程是否通畅。
    /// </summary>
    public class TestCommand : AbstractCommand
    {
        /// <summary>
        /// 测试参数：字符串类型，用于传递文本消息。
        /// </summary>
        public string TestMessage { get; set; }

        /// <summary>
        /// 测试参数：整数类型，用于传递数值。
        /// </summary>
        public int TestValue { get; set; }

        /// <summary>
        /// TestCommand 构造函数
        /// </summary>
        /// <param name="message">要传递的测试消息，默认为 "默认测试消息"。</param>
        /// <param name="value">要传递的测试数值，默认为 0。</param>
        public TestCommand(string message = "默认测试消息", int value = 0)
        {
            TestMessage = message;
            TestValue = value;
        }

        /// <summary>
        /// 执行测试命令的核心逻辑
        /// </summary>
        protected override void OnExecute()
        {
            // 1. 输出接收到的参数，验证参数传递
            Debug.Log($"[TestCommand] 命令已执行 - 消息: '{TestMessage}', 数值: {TestValue}");

            // 2. 测试获取模型 (Model)
            // 注意：IGameModel 需要在项目中实际定义和注册
            var gameModel = this.GetModel<IGameModel>();
            if (gameModel != null)
            {
                // 下面这行被注释掉了，如果IGameModel及其属性Food存在，可以取消注释进行测试
                // Debug.Log($"[TestCommand] 成功获取游戏模型 - 例如，当前食物: {gameModel.Food.Value}");
                Debug.Log("[TestCommand] 成功获取IGameModel实例。");
            }
            else
            {
                Debug.LogWarning("[TestCommand] 未能获取IGameModel实例，请确保已注册。");
            }

            // 3. 测试获取系统 (System)
            // 注意：IResourceSystem 和 ITimeSystem 需要在项目中实际定义和注册
            // var resourceSystem = this.GetSystem<IResourceSystem>();
            // if (resourceSystem != null)
            // {
            //     Debug.Log("[TestCommand] 成功获取IResourceSystem实例。");
            // }
            // else
            // {
            //     Debug.LogWarning("[TestCommand] 未能获取IResourceSystem实例。");
            // }

            // 4. 发送测试事件，通知其他模块此命令已执行
            this.SendEvent(new TestExecutedEvent
            {
                Message = TestMessage,    // 将命令的消息传递给事件
                Value = TestValue,        // 将命令的数值传递给事件
                ExecuteTime = Time.time   // 记录事件发生的时间
            });
            Debug.Log("[TestCommand] 已发送TestExecutedEvent事件。");

            // 5. 测试延时任务 (依赖ITimeSystem)
            try
            {
                var timeSystem = this.GetSystem<ITimeSystem>(); // 尝试获取时间系统
                if (timeSystem != null)
                {
                    timeSystem.AddDelayTask(1f, () => // 添加一个1秒后执行的任务
                    {
                        Debug.Log("[TestCommand] 延时1秒的任务已执行。");
                    });
                    Debug.Log("[TestCommand] 已添加延时任务到ITimeSystem。");
                }
                else
                {
                    Debug.LogWarning("[TestCommand] 未能获取ITimeSystem实例，跳过延时任务测试。");
                }
            }
            catch (System.Exception ex) // 捕获可能的异常，例如系统未注册
            {
                Debug.LogWarning($"[TestCommand] 尝试使用ITimeSystem时发生错误: {ex.Message}，跳过延时任务测试。");
            }

            Debug.Log("[TestCommand] 测试命令所有步骤执行完毕。");
        }
    }

    /// <summary>
    /// 高级测试命令
    /// 功能：提供一个框架来执行多种类型的特定测试，如模型、系统、事件或错误处理的专项测试。
    /// </summary>
    public class AdvancedTestCommand : AbstractCommand
    {
        /// <summary>
        /// 定义了高级测试命令可以执行的测试类型。
        /// </summary>
        public enum TestType
        {
            Basic,      // 基础测试：通常指命令本身能否成功执行。
            Model,      // 模型测试：测试与数据模型的交互。
            System,     // 系统测试：测试与特定游戏系统的交互。
            Event,      // 事件测试：测试事件的发送与接收。
            Error       // 错误测试：测试命令执行过程中的错误捕获和处理。
        }

        // 当前命令要执行的测试类型
        public TestType CommandTestType { get; set; }
        // 用于传递测试可能需要的任意数据
        public object TestData { get; set; }

        /// <summary>
        /// AdvancedTestCommand 构造函数
        /// </summary>
        /// <param name="testType">要执行的测试类型，默认为 TestType.Basic。</param>
        /// <param name="data">测试可能需要的附加数据，默认为 null。</param>
        public AdvancedTestCommand(TestType testType = TestType.Basic, object data = null)
        {
            CommandTestType = testType;
            TestData = data;
        }

        /// <summary>
        /// 执行高级测试命令的核心逻辑，根据 CommandTestType 分发到具体的测试方法。
        /// </summary>
        protected override void OnExecute()
        {
            Debug.Log($"[AdvancedTestCommand] 开始执行 '{CommandTestType}' 类型测试，附加数据: {TestData?.ToString() ?? "无"}");

            switch (CommandTestType)
            {
                case TestType.Basic:
                    ExecuteBasicTest();
                    break;
                case TestType.Model:
                    ExecuteModelTest();
                    break;
                case TestType.System:
                    ExecuteSystemTest();
                    break;
                case TestType.Event:
                    ExecuteEventTest();
                    break;
                case TestType.Error:
                    ExecuteErrorTest();
                    break;
                default:
                    Debug.LogWarning($"[AdvancedTestCommand] 未知的测试类型: {CommandTestType}");
                    break;
            }

            Debug.Log($"[AdvancedTestCommand] '{CommandTestType}' 类型测试完成。");
        }

        /// <summary>
        /// 执行基础测试的私有方法。
        /// </summary>
        private void ExecuteBasicTest()
        {
            Debug.Log("[AdvancedTestCommand] 正在执行基础功能测试...");
            // 基础测试通常只验证命令能够被调用并发送一个完成事件
            this.SendEvent(new TestExecutedEvent
            {
                Message = "高级测试 - 基础测试完成",
                Value = (int)TestType.Basic, // 使用枚举值作为示例
                ExecuteTime = Time.time
            });
        }

        /// <summary>
        /// 执行模型访问测试的私有方法。
        /// </summary>
        private void ExecuteModelTest()
        {
            Debug.Log("[AdvancedTestCommand] 正在执行模型访问测试...");
            var gameModel = this.GetModel<IGameModel>();
            if (gameModel != null)
            {
                Debug.Log("[AdvancedTestCommand] 模型测试 - 成功获取IGameModel实例。");
                // 示例：访问模型数据，需要IGameModel有实际属性
                // Debug.Log($"[AdvancedTestCommand] 模型测试 - 食物: {gameModel.Food.Value}, 弹药: {gameModel.Ammo.Value}");
            }
            else
            {
                Debug.LogWarning("[AdvancedTestCommand] 模型测试 - 未能获取IGameModel实例。");
            }
        }

        /// <summary>
        /// 执行系统访问测试的私有方法。
        /// </summary>
        private void ExecuteSystemTest()
        {
            Debug.Log("[AdvancedTestCommand] 正在执行系统访问测试...");
            // 可以在这里添加获取和使用各种游戏系统的测试代码
            // 例如: var someSystem = this.GetSystem<ISomeSystem>();
            // if (someSystem != null) { someSystem.PerformAction(); }
            Debug.Log("[AdvancedTestCommand] 系统访问测试占位符，请在此处添加具体系统测试逻辑。");
        }

        /// <summary>
        /// 执行事件发送测试的私有方法。
        /// </summary>
        private void ExecuteEventTest()
        {
            Debug.Log("[AdvancedTestCommand] 正在执行事件发送测试...");
            this.SendEvent(new TestExecutedEvent
            {
                Message = "高级测试 - 事件测试",
                Value = (int)TestType.Event,
                ExecuteTime = Time.time
            });
            Debug.Log("[AdvancedTestCommand] 事件发送测试 - 已发送TestExecutedEvent。");
        }

        /// <summary>
        /// 执行错误处理测试的私有方法。
        /// </summary>
        private void ExecuteErrorTest()
        {
            Debug.Log("[AdvancedTestCommand] 正在执行错误处理测试...");
            try
            {
                // 故意触发一个预期的错误来进行测试
                Debug.Log("[AdvancedTestCommand] 错误测试 - 准备触发一个模拟错误...");
                var nullModel = this.GetModel<IGameModel>(); // 假设获取模型成功
                // 下一行代码如果取消注释，并且nullModel.Food存在，则会尝试除零操作，从而引发DivideByZeroException
                // var testValue = nullModel.Food.Value / 0;
                // 为了更安全地模拟错误，可以抛出一个自定义异常或使用Assert等
                if (TestData is string && (string)TestData == "trigger_error")
                {
                    throw new System.InvalidOperationException("模拟的错误条件已触发。");
                }
                Debug.Log("[AdvancedTestCommand] 错误测试 - 未触发模拟错误（TestData可能不匹配）。");
            }
            catch (System.Exception ex) // 捕获所有类型的异常
            {
                Debug.LogWarning($"[AdvancedTestCommand] 错误测试 - 成功捕获到预期或意外的错误: {ex.GetType().Name} - {ex.Message}");
            }
        }
    }
}

// 事件定义 (通常放在单独的事件文件中或共享命名空间)
namespace MyGameNamespace
{
    /// <summary>
    /// 测试命令执行完成事件结构体
    /// 当TestCommand或AdvancedTestCommand中的特定测试执行完毕时，可能会发送此事件。
    /// </summary>
    public struct TestExecutedEvent
    {
        /// <summary>
        /// 与测试相关的消息文本。
        /// </summary>
        public string Message;

        /// <summary>
        /// 与测试相关的数值。
        /// </summary>
        public int Value;

        /// <summary>
        /// 命令或事件发生的Unity游戏时间 (Time.time)。
        /// </summary>
        public float ExecuteTime;
    }
}