// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：BaseUIController.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了UI控制器的基类 (BaseUIController)。
//     它提供了UI控制器常用的一些基础功能，特别是用于查找子UI元素的方法，
//     并集成了QFramework的IController接口。
// ==============================================================================

using MyGameNamespace; // 根据项目实际情况调整或移除
using QFramework;
using UnityEngine;
using UnityEngine.UI; // 通常UI控制器会用到UnityEngine.UI

namespace SurvivalGame.Controller
{
    /// <summary>
    /// UI控制器的基类。
    /// 继承自 MonoBehaviour，并实现 QFramework 的 IController 接口。
    /// 提供了查找子UI元素（组件或GameObject）的便捷方法。
    /// </summary>
    public class BaseUIController : MonoBehaviour, IController
    {
        
        /// <summary>
        /// 获取当前场景中注册的QFramework架构实例。
        /// </summary>
        /// <returns>实现IArchitecture接口的架构实例。</returns>
        public IArchitecture GetArchitecture() => RegisterManager.Interface;

        /// <summary>
        /// 查找并返回指定路径下的指定类型的UI组件。
        /// 路径格式支持通过斜杠 "/" 分隔的深层嵌套路径，例如 "PanelA/ButtonB/TextC"。
        /// </summary>
        /// <typeparam name="T">要查找的组件类型，必须是 Component 的子类。</typeparam>
        /// <param name="path">UI元素的层级路径，相对于当前GameObject或Canvas根节点。</param>
        /// <returns>找到的组件实例；如果未找到对应路径的Transform或Transform上没有指定类型的组件，则返回null。</returns>
        protected T FindUIComponent<T>(string path) where T : Component
        {
            // 使用 DeepFind 方法查找目标 Transform
            Transform foundTransform = DeepFind(path);

            if (foundTransform == null)
            {
                // 如果路径对应的Transform未找到，记录警告并返回null
                Debug.LogWarning($"[UI] 在 {this.gameObject.name} 控制器下未找到UI组件的路径: {path}");
                return null;
            }

            // 尝试从找到的Transform上获取指定类型的组件
            T component = foundTransform.GetComponent<T>();
            if (component == null)
            {
                // 如果Transform上没有该类型的组件，记录警告并返回null
                Debug.LogWarning($"[UI] 在路径 {path} (实际对象名: {foundTransform.name}) 上未找到组件类型: {typeof(T).Name}");
                return null;
            }

            return component;
        }

        /// <summary>
        /// 查找并返回指定路径下的 GameObject。
        /// 路径格式支持通过斜杠 "/" 分隔的深层嵌套路径。
        /// </summary>
        /// <param name="path">UI元素的层级路径，相对于当前GameObject或Canvas根节点。</param>
        /// <returns>找到的GameObject；如果未找到，则返回null。</returns>
        protected GameObject FindUIGameObject(string path)
        {
            Transform foundTransform = DeepFind(path);

            if (foundTransform == null)
            {
                Debug.LogWarning($"[UI] 在 {this.gameObject.name} 控制器下未找到UI对象的路径: {path}");
                return null;
            }

            return foundTransform.gameObject;
        }

        /// <summary>
        /// 深度优先查找指定路径对应的 Transform 对象。
        /// 路径格式为 "父对象名/子对象名/更深层子对象名"。
        /// 首先尝试从当前UI控制器挂载的GameObject开始查找，如果第一级未找到，
        /// 则会尝试从父级Canvas的根节点开始查找（适用于UI元素不在当前控制器直接子级的情况）。
        /// </summary>
        /// <param name="path">要查找的层级路径。</param>
        /// <returns>找到的Transform对象；如果任一级路径未找到，则返回null。</returns>
        private Transform DeepFind(string path)
        {
            // 按斜杠分割路径字符串，得到各层级的名称
            string[] subPath = path.Split('/');
            Transform current = transform; // 初始查找起点为当前GameObject的Transform

            // 遍历路径中的每个层级名称
            for (int i = 0; i < subPath.Length; i++)
            {
                string name = subPath[i];
                bool foundThisLevel = false;
                // 在当前Transform的子对象中查找匹配的名称
                foreach (Transform child in current)
                {
                    if (child.name == name)
                    {
                        current = child; // 更新当前Transform为找到的子对象
                        foundThisLevel = true;
                        break; // 已找到当前层级的对象，跳出内部循环
                    }
                }

                // 如果在当前Transform的子对象中未找到匹配项
                if (!foundThisLevel)
                {
                    // 特殊处理：如果这是路径的第一部分 (i == 0)，并且在当前transform的子物体中没找到，
                    // 则尝试从父级Canvas的根节点开始重新查找整个路径。
                    // 这有助于处理那些UI元素并非严格挂载在Controller对应GameObject下的情况。
                    if (i == 0)
                    {
                        Canvas canvas = GetComponentInParent<Canvas>(); // 获取父级Canvas
                        if (canvas != null)
                        {
                            // Debug.Log($"[UI] DeepFind: 在 {this.name} 的子节点中未找到 '{name}'，尝试从Canvas '{canvas.name}' 根节点开始查找完整路径 '{path}'。");
                            current = canvas.transform; // 将查找起点改为Canvas的Transform
                            // 从Canvas开始重新匹配当前层级名称 name
                            foundThisLevel = false; // 重置查找状态
                            foreach (Transform childInCanvas in current)
                            {
                                if (childInCanvas.name == name)
                                {
                                    current = childInCanvas;
                                    foundThisLevel = true;
                                    break;
                                }
                            }
                            // 如果在Canvas下仍然找不到第一级，则说明路径无效
                            if (!foundThisLevel)
                            {
                                // Debug.LogWarning($"[UI] DeepFind: 从Canvas '{canvas.name}' 根节点也未找到路径的第一部分 '{name}' (完整路径: '{path}')。");
                                return null;
                            }
                            // 如果在Canvas下找到了第一级，则继续下一个层级的查找 (continue to next iteration of outer loop)
                        }
                        else
                        {
                            // 如果没有父级Canvas，且第一级就没找到，则路径无效
                            // Debug.LogWarning($"[UI] DeepFind: 在 {this.name} 的子节点中未找到 '{name}'，且无父级Canvas可供回退查找 (完整路径: '{path}')。");
                            return null;
                        }
                    }
                    else
                    {
                        // 如果不是路径的第一部分未找到，则说明中间路径断开，查找失败
                        // Debug.LogWarning($"[UI] DeepFind: 在路径 '{path}' 中，未能找到 '{current.name}' 的子对象 '{name}'。");
                        return null;
                    }
                }
            }
            // 所有层级都成功找到，返回最终的Transform
            return current;
        }
    }
}