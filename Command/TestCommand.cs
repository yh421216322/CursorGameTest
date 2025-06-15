using QFramework;
using MyGameNamespace;
using UnityEngine;

namespace SurvivalGame.Command
{
    /// <summary>
    /// 测试命令
    /// 功能：用于测试QFramework命令系统的基础功能
    /// 用途：开发和调试时验证命令执行流程
    /// </summary>
    public class TestCommand : AbstractCommand
    {
        /// <summary>
        /// 测试参数：字符串类型
        /// </summary>
        public string TestMessage { get; set; }

        /// <summary>
        /// 测试参数：整数类型
        /// </summary>
        public int TestValue { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">测试消息</param>
        /// <param name="value">测试数值</param>
        public TestCommand(string message = "默认测试消息", int value = 0)
        {
            TestMessage = message;
            TestValue = value;
        }

        /// <summary>
        /// 执行测试命令
        /// </summary>
        protected override void OnExecute()
        {
            // 输出测试日志
            Debug.Log($"[TestCommand] 执行测试命令 - 消息: {TestMessage}, 数值: {TestValue}");

            // 测试获取模型数据
            var gameModel = this.GetModel<IGameModel>();
            if (gameModel != null)
            {
              //  Debug.Log($"[TestCommand] 成功获取游戏模型 - 当前食物: {gameModel.Food.Value}");
            }

            // 测试获取系统
          //  var resourceSystem = this.GetSystem<IResourceSystem>();
            // if (resourceSystem != null)
            // {
            //     Debug.Log("[TestCommand] 成功获取资源系统");
            // }

            // 发送测试事件
            this.SendEvent(new TestExecutedEvent
            {
                Message = TestMessage,
                Value = TestValue,
                ExecuteTime = Time.time
            });

            // 测试延时任务（如果有时间系统）
            try
            {
                var timeSystem = this.GetSystem<ITimeSystem>();
                if (timeSystem != null)
                {
                    timeSystem.AddDelayTask(1f, () =>
                    {
                        Debug.Log("[TestCommand] 延时任务执行完成");
                    });
                }
            }
            catch (System.Exception)
            {
                Debug.Log("[TestCommand] 时间系统未找到，跳过延时任务测试");
            }

            Debug.Log("[TestCommand] 测试命令执行完成");
        }
    }

    /// <summary>
    /// 高级测试命令
    /// 功能：测试更复杂的命令功能，包括参数验证和错误处理
    /// </summary>
    public class AdvancedTestCommand : AbstractCommand
    {
        public enum TestType
        {
            Basic,      // 基础测试
            Model,      // 模型测试
            System,     // 系统测试
            Event,      // 事件测试
            Error       // 错误测试
        }

        public TestType CommandTestType { get; set; }
        public object TestData { get; set; }

        public AdvancedTestCommand(TestType testType = TestType.Basic, object data = null)
        {
            CommandTestType = testType;
            TestData = data;
        }

        protected override void OnExecute()
        {
            Debug.Log($"[AdvancedTestCommand] 开始执行 {CommandTestType} 类型测试");

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
            }

            Debug.Log($"[AdvancedTestCommand] {CommandTestType} 类型测试完成");
        }

        private void ExecuteBasicTest()
        {
            Debug.Log("[AdvancedTestCommand] 执行基础功能测试");
            this.SendEvent(new TestExecutedEvent
            {
                Message = "基础测试完成",
                Value = 1,
                ExecuteTime = Time.time
            });
        }

        private void ExecuteModelTest()
        {
            Debug.Log("[AdvancedTestCommand] 执行模型访问测试");
            var gameModel = this.GetModel<IGameModel>();
            if (gameModel != null)
            {
                //Debug.Log($"[AdvancedTestCommand] 模型测试 - 食物: {gameModel.Food.Value}, 弹药: {gameModel.Ammo.Value}");
            }
        }

        private void ExecuteSystemTest()
        {
            Debug.Log("[AdvancedTestCommand] 执行系统访问测试");
            // 可以在这里测试各种系统的访问
        }

        private void ExecuteEventTest()
        {
            Debug.Log("[AdvancedTestCommand] 执行事件发送测试");
            this.SendEvent(new TestExecutedEvent
            {
                Message = "事件测试",
                Value = 999,
                ExecuteTime = Time.time
            });
        }

        private void ExecuteErrorTest()
        {
            Debug.Log("[AdvancedTestCommand] 执行错误处理测试");
            try
            {
                // 故意触发一个错误进行测试
                var nullModel = this.GetModel<IGameModel>();
               // var testValue = nullModel.Food.Value / 0; // 除零错误
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AdvancedTestCommand] 捕获到预期错误: {ex.Message}");
            }
        }
    }
}

// 事件定义
namespace MyGameNamespace
{
    /// <summary>
    /// 测试命令执行完成事件
    /// </summary>
    public struct TestExecutedEvent
    {
        /// <summary>
        /// 测试消息
        /// </summary>
        public string Message;

        /// <summary>
        /// 测试数值
        /// </summary>
        public int Value;

        /// <summary>
        /// 执行时间
        /// </summary>
        public float ExecuteTime;
    }
} 