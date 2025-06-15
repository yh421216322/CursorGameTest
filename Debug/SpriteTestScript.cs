// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：SpriteTestScript.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了一个名为 SpriteTestScript 的 MonoBehaviour 类。
//     该脚本用于在Unity编辑器或运行时测试Sprite资源的加载和显示。
//     它可以自动或手动（通过快捷键）在场景中创建带有指定Sprite的GameObject。
//     如果Sprite加载失败，它会创建一个彩色的方块作为占位符。
//     同时，它还提供一个简单的IMGUI界面提示用户操作。
// ==============================================================================

using UnityEngine;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 简单的Sprite（精灵图片）测试脚本。
    /// 用于在开发和调试过程中快速验证项目中的图片资源（Sprite）是否能够被正确加载和显示。
    /// 如果图片加载失败，会生成一个彩色的方块作为替代，以便于识别问题。
    /// </summary>
    public class SpriteTestScript : MonoBehaviour
    {
        [Header("测试设置")] // Inspector中分组显示
        public bool createTestSprites = false; // 是否在游戏开始时自动创建测试Sprite对象。默认为false，以避免在非测试场景自动创建。
        public float testSpriteScale = 1f;     // 创建的测试Sprite对象的统一缩放比例。

        /// <summary>
        /// Unity生命周期方法：当脚本实例被激活和游戏开始时调用一次。
        /// 如果 createTestSprites 设置为true，则调用 CreateTestSprites 方法。
        /// </summary>
        void Start()
        {
            if (createTestSprites)
            {
                CreateTestSprites(); // 创建测试用的Sprite对象
            }
        }
        
        /// <summary>
        /// 创建一系列测试用的Sprite对象。
        /// 它会尝试从 "Resources/Image/" 目录下加载一组预定义的Sprite。
        /// 如果加载成功，则在场景中创建对应的GameObject并显示Sprite；
        /// 如果加载失败，则创建一个彩色的方块作为占位符。
        /// </summary>
        void CreateTestSprites()
        {
            // 定义一组要测试加载的Sprite资源的名称
            string[] spriteNames = { "zb1", "zb2", "zb3", "zb4" }; // 假设这些是僵尸或其他测试用的Sprite名称
            
            // 遍历每个Sprite名称进行加载和创建
            for (int i = 0; i < spriteNames.Length; i++)
            {
                // 尝试从 "Resources/Image/" 路径下加载Sprite资源
                // 注意：Resources.Load的路径是相对于任何名为 "Resources" 的文件夹的内部路径。
                Sprite sprite = Resources.Load<Sprite>($"Image/{spriteNames[i]}");
                
                if (sprite != null) // 如果Sprite资源成功加载
                {
                    UnityEngine.Debug.Log($"[SpriteTest] 成功加载Sprite: {spriteNames[i]} (资源名: {sprite.name})");
                    
                    // 创建一个新的GameObject来显示这个Sprite
                    GameObject go = new GameObject($"TestSprite_{spriteNames[i]}"); // 命名以便于在场景中识别
                    go.transform.position = new Vector3(i * 2f, 0, 0); // 将每个Sprite错开排列，避免重叠
                    go.transform.localScale = Vector3.one * testSpriteScale; // 设置统一的缩放
                    
                    // 添加SpriteRenderer组件并分配加载到的Sprite
                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sortingOrder = 100; // 设置一个较高的渲染排序顺序，确保它显示在其他元素之上
                    
                    UnityEngine.Debug.Log($"[SpriteTest] 已创建测试Sprite对象: {go.name}，位置: {go.transform.position}");
                }
                else // 如果Sprite资源加载失败
                {
                    UnityEngine.Debug.LogError($"[SpriteTest] 无法加载Sprite资源: Resources/Image/{spriteNames[i]}。请检查路径和资源是否存在。");
                    
                    // 创建一个彩色的方块作为加载失败的占位符或提示
                    CreateColoredSquare(spriteNames[i] + "_Fallback", i);
                }
            }
        }
        
        /// <summary>
        /// 当指定的Sprite加载失败时，创建一个彩色的方形Sprite作为占位符。
        /// </summary>
        /// <param name="baseName">用于生成占位符名称的基础字符串。</param>
        /// <param name="index">一个索引，用于从预定义的颜色数组中选择颜色，并确定占位符的位置。</param>
        void CreateColoredSquare(string baseName, int index)
        {
            // 创建GameObject
            GameObject go = new GameObject($"TestSquare_Fallback_{baseName}");
            // 将占位符方块放置在原始Sprite预期位置的下方 (-2f on Y axis) 以示区别
            go.transform.position = new Vector3(index * 2f, -2f, 0);
            go.transform.localScale = Vector3.one * testSpriteScale;
            
            // 程序化创建一个简单的64x64纯色纹理
            Texture2D texture = new Texture2D(64, 64);
            // 定义一组颜色，用于循环给不同的占位符上色
            Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
            Color color = colors[index % colors.Length]; // 通过取模运算循环使用颜色
            
            // 填充纹理的每个像素为选定的颜色
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply(); // 应用像素更改到纹理
            
            // 从生成的纹理创建一个Sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f)); // Pivot设为中心
            
            // 添加SpriteRenderer并显示创建的彩色方块Sprite
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 100; // 同样设置较高渲染顺序
            
            UnityEngine.Debug.Log($"[SpriteTest] 由于Sprite加载失败，已创建彩色方块占位符: {go.name}，位置: {go.transform.position}，颜色: {color}");
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用。
        /// 检测是否按下了 'T' 键，如果按下，则清理旧的测试对象并重新创建测试Sprite。
        /// </summary>
        void Update()
        {
            // 如果玩家按下了 'T' 键
            if (Input.GetKeyDown(KeyCode.T))
            {
                UnityEngine.Debug.Log("[SpriteTest] 检测到 'T' 键按下，开始清理并重新创建测试Sprite...");
                // 清理场景中之前由这个脚本创建的测试对象
                // 注意：FindGameObjectsWithTag依赖于对象是否有正确的Tag，这里基于名称查找更符合当前脚本逻辑
                GameObject[] testObjects = FindObjectsOfType<GameObject>(); // 获取场景中所有对象
                foreach (var obj in testObjects)
                {
                    // 如果对象名称包含 "TestSprite" 或 "TestSquare"，则认为是此脚本创建的测试对象
                    if (obj.name.Contains("TestSprite_") || obj.name.Contains("TestSquare_Fallback_"))
                    {
                        // DestroyImmediate用于编辑器模式或需要立即生效的场景，运行时通常用Destroy()
                        // 此脚本可能在编辑器和运行时都被使用，DestroyImmediate在此处是可接受的
                        DestroyImmediate(obj);
                    }
                }
                
                // 重新调用创建测试Sprite的方法
                CreateTestSprites();
                UnityEngine.Debug.Log("[SpriteTest] 测试Sprite已重新创建。");
            }
        }
        
        /// <summary>
        /// Unity生命周期方法：用于渲染和处理IMGUI事件。
        /// 在屏幕左下角显示一个简单的操作提示。
        /// </summary>
        void OnGUI()
        {
            // 定义一个矩形区域用于显示文本标签
            // 位置在屏幕左下角 (10, Screen.height - 80)，尺寸为300x60
            GUI.Label(new Rect(10, Screen.height - 80, 300, 60), 
                "Sprite资源测试脚本\n按 'T' 键可重新生成测试图片和占位符。\n请检查场景中是否有 'TestSprite_' 或 'TestSquare_' 开头的对象，并确认显示是否正确。"
            );
        }
    }
}