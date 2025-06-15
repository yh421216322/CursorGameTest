using MyGameNamespace;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalGame.Controller
{
    public class BaseUIController : MonoBehaviour, IController
    {
        
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
        /// <summary>
        /// 查找并返回指定类型的组件，支持深层路径 a/b/c 格式
        /// </summary>
        protected T FindUIComponent<T>(string path) where T : Component
        {
            Transform foundTransform = DeepFind(path);

            if (foundTransform == null)
            {
                Debug.LogWarning($"[UI] 未找到UI组件路径: {path}");
                return null;
            }

            T component = foundTransform.GetComponent<T>();
            if (component == null)
            {
                Debug.LogWarning($"[UI] 路径 {path} 上未找到组件类型: {typeof(T).Name}");
                return null;
            }

            return component;
        }

        /// <summary>
        /// 查找并返回指定路径的 GameObject
        /// </summary>
        protected GameObject FindUIGameObject(string path)
        {
            Transform foundTransform = DeepFind(path);

            if (foundTransform == null)
            {
                Debug.LogWarning($"[UI] 未找到UI对象路径: {path}");
                return null;
            }

            return foundTransform.gameObject;
        }

        /// <summary>
        /// 深度优先查找路径对应的 Transform（支持 a/b/c 格式）
        /// </summary>
        private Transform DeepFind(string path)
        {
            string[] subPath = path.Split('/');
            Transform current = transform;

            foreach (string name in subPath)
            {
                bool found = false;
                foreach (Transform child in current)
                {
                    if (child.name == name)
                    {
                        current = child;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // 尝试从 Canvas 开始查找
                    Canvas canvas = GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        current = canvas.transform;
                        foreach (Transform child in current)
                        {
                            if (child.name == name)
                            {
                                current = child;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        return null;
                    }
                }
            }

            return current;
        }
    }
}
