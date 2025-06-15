using UnityEngine;

namespace SurvivalGame.DebugTools
{
    /// <summary>
    /// 简单的Sprite测试脚本
    /// 用于验证图片资源是否能正常显示
    /// </summary>
    public class SpriteTestScript : MonoBehaviour
    {
        [Header("测试设置")]
        public bool createTestSprites = false; // 改为false，避免自动创建静态图片
        public float testSpriteScale = 1f;
        
        void Start()
        {
            if (createTestSprites)
            {
                CreateTestSprites();
            }
        }
        
        void CreateTestSprites()
        {
            string[] spriteNames = { "zb1", "zb2", "zb3", "zb4" };
            
            for (int i = 0; i < spriteNames.Length; i++)
            {
                // 尝试加载图片
                Sprite sprite = Resources.Load<Sprite>($"Image/{spriteNames[i]}");
                
                if (sprite != null)
                {
                    UnityEngine.Debug.Log($"成功加载 {spriteNames[i]}: {sprite.name}");
                    
                    // 创建GameObject
                    GameObject go = new GameObject($"TestSprite_{spriteNames[i]}");
                    go.transform.position = new Vector3(i * 2f, 0, 0);
                    go.transform.localScale = Vector3.one * testSpriteScale;
                    
                    // 添加SpriteRenderer
                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sortingOrder = 100; // 确保在最顶层
                    
                    UnityEngine.Debug.Log($"创建测试Sprite: {go.name} 位置: {go.transform.position}");
                }
                else
                {
                    UnityEngine.Debug.LogError($"无法加载图片: Image/{spriteNames[i]}");
                    
                    // 创建一个简单的彩色方块作为替代
                    CreateColoredSquare(spriteNames[i], i);
                }
            }
        }
        
        void CreateColoredSquare(string name, int index)
        {
            // 创建一个简单的彩色方块
            GameObject go = new GameObject($"TestSquare_{name}");
            go.transform.position = new Vector3(index * 2f, -2f, 0);
            go.transform.localScale = Vector3.one * testSpriteScale;
            
            // 创建一个简单的纹理
            Texture2D texture = new Texture2D(64, 64);
            Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };
            Color color = colors[index % colors.Length];
            
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 100;
            
            UnityEngine.Debug.Log($"创建测试方块: {go.name} 位置: {go.transform.position}");
        }
        
        void Update()
        {
            // 按T键重新创建测试Sprite
            if (Input.GetKeyDown(KeyCode.T))
            {
                // 清理旧的测试对象
                GameObject[] testObjects = GameObject.FindGameObjectsWithTag("Untagged");
                foreach (var obj in testObjects)
                {
                    if (obj.name.Contains("TestSprite") || obj.name.Contains("TestSquare"))
                    {
                        DestroyImmediate(obj);
                    }
                }
                
                // 重新创建
                CreateTestSprites();
                UnityEngine.Debug.Log("重新创建测试Sprite");
            }
        }
        
        void OnGUI()
        {
            GUI.Label(new Rect(10, Screen.height - 80, 300, 60), 
                "Sprite测试脚本\n按T键重新创建测试图片\n检查场景中是否有TestSprite对象");
        }
    }
} 