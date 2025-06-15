// ==============================================================================
// **版权所有 (C) 2024 未知开发者保留所有权利。**
//
// 文件名：GameManager.cs
// 作者：未知开发者
// 创建日期：2024年07月15日
// 修改日期：2024年07月15日
// 文件版本：1.0.0
// 描述：
//     此文件定义了游戏的核心管理器 (GameManager)。
//     GameManager 负责整个游戏的初始化流程、主游戏循环中的时间推进、
//     协调各个游戏系统（如资源、建筑、时间、僵尸、防御、科技等）的更新，
//     处理玩家输入（相机控制、游戏控制、调试快捷键），以及管理游戏状态（暂停、速度）。
//     它还包括一些调试功能，如动态创建地面、简化僵尸可视化和通过IMGUI显示调试信息。
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics; // 用于 ConditionalAttribute
using UnityEngine;
using QFramework;
using MyGameNamespace;      // 包含自定义事件如 TimeUpdateEvent, NewDayEvent
using SurvivalGame.Model;   // 包含游戏数据模型如 ISurvivalGameModel, ZombieData, TowerType 等
using SurvivalGame.GameSystem; // 包含各个游戏系统接口如 IResourceSystem, IZombieSystem 等
using SurvivalGame;         // 可能包含一些通用的游戏定义
using SurvivalGame.Command; // 包含游戏指令如 SpawnZombieCommand, BuildTowerCommand 等
using Debug = UnityEngine.Debug; // 使用UnityEngine.Debug以避免与System.Diagnostics.Debug冲突

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 游戏总管理器。
    /// 负责游戏的整体初始化、时间流逝控制、各游戏子系统的协调更新、
    /// 玩家输入处理以及提供调试接口等核心功能。
    /// 通常挂载于场景中的一个持久化空对象（例如 "GameManager"）。
    /// 依赖于QFramework框架中的模型(ISurvivalGameModel)和系统(ITimeSystem等)。
    /// </summary>
    public class GameManager : MonoBehaviour, IController, ICanSendEvent // 实现QFramework的IController和事件发送接口
    {
        [Header("游戏核心设置")] // Inspector中显示的头部标签
        [SerializeField] private bool autoStart = true;       // 是否在场景加载完成后自动开始游戏
        [SerializeField] private float gameSpeed = 1f;        // 当前游戏时间流逝速度的倍率
        [SerializeField] private float dayDuration = 120f;    // 游戏中一天在现实世界中持续的时间（秒）

        [Header("相机初始设置")]
        [SerializeField] private Camera gameCamera; // 游戏主相机引用
        [SerializeField] private Vector3 cameraStartPosition = new Vector3(0, 0, -10); // 相机初始位置
        [SerializeField] private Vector3 cameraStartRotation = new Vector3(0, 0, 0);   // 相机初始旋转

        // QFramework框架核心组件引用
        private ISurvivalGameModel mGameModel;      // 游戏数据模型接口
        private IResourceSystem mResourceSystem;    // 资源系统接口
        private IEnhancedBuildingSystem mBuildingSystem; // 增强型建筑系统接口
        private ITimeSystem mTimeSystem;            // 时间系统接口

        // 内部时间管理变量
        private float mTimeAccumulator = 0f; // 未使用的累加器（可能用于旧版时间逻辑或特定计时）
        private float mDayStartTime;         // 当前游戏日开始时的真实世界时间戳 (Time.time)

        /// <summary>
        /// Unity生命周期方法：当脚本实例被创建时调用，在所有Start方法之前。
        /// 主要用于获取QFramework框架中的模型和系统实例。
        /// </summary>
        private void Awake()
        {
            // 通过QFramework的this.GetModel/GetSystem扩展方法获取核心组件实例
            // 这通常会自动触发QFramework架构的初始化过程（如果尚未初始化）
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mResourceSystem = this.GetSystem<IResourceSystem>();
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            mTimeSystem = this.GetSystem<ITimeSystem>();
        }
        
        /// <summary>
        /// Unity生命周期方法：在Awake之后、首次Update之前调用一次。
        /// 用于执行游戏的主要初始化流程和启动游戏（如果autoStart为true）。
        /// </summary>
        private void Start()
        {
            InitializeGame(); // 执行游戏初始化
            
            // 此处注释掉的是添加各种调试辅助组件的代码，可以根据需要取消注释来启用它们
            // gameObject.AddComponent<SurvivalGame.DebugTools.ZombieDebugHelper>();
            // gameObject.AddComponent<SurvivalGame.DebugTools.ZombieLocator>();
            // gameObject.AddComponent<SurvivalGame.DebugTools.ZombieVisualizationDiagnostic>();
            // gameObject.AddComponent<SurvivalGame.DebugTools.ZombieCollisionTest>();
            // gameObject.AddComponent<SurvivalGame.DebugTools.CollisionTestGuide>();
            
            // 如果设置了自动开始，则调用StartGame方法
            if (autoStart)
            {
                StartGame();
            }
        }
        
        /// <summary>
        /// 初始化游戏的核心设置和初始状态。
        /// 包括设置相机、创建地面、初始化游戏时间、建造初始建筑结构等。
        /// </summary>
        private void InitializeGame()
        {
            SetupCamera();      // 配置主相机
            CreateGround();     // 创建程序化生成的2D地面
            
            mDayStartTime = Time.time; // 记录游戏开始时的时间作为第一天的起始时间
            
            BuildInitialStructures(); // 建造游戏开始时的初始建筑
            
            // Debug.Log("游戏初始化完成"); // 初始化完成日志（当前注释）
        }
        
        /// <summary>
        /// 设置游戏主相机的初始位置、旋转和投影模式。
        /// </summary>
        private void SetupCamera()
        {
            // 如果gameCamera未在Inspector中指定，则尝试获取场景中的主相机
            if (gameCamera == null)
                gameCamera = Camera.main;
            
            if (gameCamera != null)
            {
                gameCamera.transform.position = cameraStartPosition; // 设置相机位置
                gameCamera.transform.eulerAngles = cameraStartRotation; // 设置相机旋转
                gameCamera.orthographic = true; // 将相机设置为正交投影模式（适用于2D或2.5D俯视角游戏）
                gameCamera.orthographicSize = 10f; // 设置正交相机的大小（视野范围）
            }
        }
        
        /// <summary>
        /// 程序化创建一个2D地面背景。
        /// 使用Perlin噪声生成简单的泥土纹理，并应用到SpriteRenderer上。
        /// </summary>
        private void CreateGround()
        {
            GameObject ground = new GameObject("Ground"); // 创建名为"Ground"的游戏对象
            ground.transform.position = Vector3.zero;   // 设置在世界原点
            
            var spriteRenderer = ground.AddComponent<SpriteRenderer>(); // 添加SpriteRenderer组件用于显示2D图像
            
            // 创建一个64x64像素的纹理
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false); // RGBA32格式，不生成mipmap
            
            // 填充纹理像素数据
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color baseColor = new Color(0.4f, 0.3f, 0.2f, 1f); // 定义基础的泥土颜色
                    // 使用Perlin噪声给颜色添加一些随机变化，使地面看起来更自然
                    float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.1f;
                    baseColor += new Color(noise, noise, noise, 0); // 只影响RGB，不影响Alpha
                    texture.SetPixel(x, y, baseColor); // 设置像素颜色
                }
            }
            
            // 应用纹理设置，针对像素艺术风格优化
            texture.filterMode = FilterMode.Point; // 点滤波，保持像素边缘清晰
            texture.wrapMode = TextureWrapMode.Clamp; // 纹理边缘拉伸模式
            texture.Apply(); // 应用所有SetPixel更改到纹理
            
            // 从程序化生成的纹理创建Sprite对象
            // 32f是pixelsPerUnit，表示Unity中一个单位对应多少像素，影响Sprite在世界中的大小
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f), 32f);
            
            spriteRenderer.sprite = sprite; // 将创建的Sprite赋给SpriteRenderer
            spriteRenderer.transform.localScale = new Vector3(2f, 2f, 1f); // 调整地面Sprite的缩放
            spriteRenderer.sortingOrder = -10; // 设置渲染排序顺序，使其位于其他对象之后（作为背景）
            spriteRenderer.sortingLayerName = "Background"; // 分配到"Background"渲染层（需在Unity中定义此层）
        }
        
        /// <summary>
        /// 建造游戏开始时的初始建筑结构。
        /// 例如，一个避难所和一个农田。
        /// </summary>
        private void BuildInitialStructures()
        {
            // 尝试在(0,0,0)位置建造一个"Shelter"（避难所）
            if (mBuildingSystem.CanBuildAt(new Vector3(0, 0, 0), "Shelter")) // 检查是否可建造
            {
                mBuildingSystem.StartConstruction(new Vector3(0, 0, 0), "Shelter"); // 开始建造
            }
            
            // 尝试在(4,0,0)位置建造一个"Farm"（农田）
            if (mBuildingSystem.CanBuildAt(new Vector3(4, 0, 0), "Farm"))
            {
                mBuildingSystem.StartConstruction(new Vector3(4, 0, 0), "Farm");
            }
        }
        
        /// <summary>
        /// 正式开始游戏逻辑，例如取消暂停状态。
        /// </summary>
        private void StartGame()
        {
            mGameModel.IsPaused.Value = false; // 设置游戏数据模型中的暂停状态为false
            // Debug.Log("游戏开始"); // 游戏开始日志（当前注释）
        }
        
        /// <summary>
        /// Unity生命周期方法：每帧调用一次。
        /// 游戏的核心更新循环在此处。
        /// </summary>
        private void Update()
        {
            // 如果游戏已暂停，则不执行任何更新逻辑
            if (mGameModel.IsPaused.Value) return;
            
            // 依次更新游戏时间、时间系统、其他游戏系统和处理输入
            UpdateGameTime();     // 更新游戏内的时间和日期
            UpdateTimeSystem();   // 更新独立的时间系统（可能用于定时任务等）
            UpdateGameSystems();  // 更新所有主要的游戏逻辑系统
            HandleInput();        // 处理玩家的输入
        }
        
        /// <summary>
        /// 更新所有主要的游戏逻辑子系统。
        /// </summary>
        private void UpdateGameSystems()
        {
            // Debug.Log($"更新游戏系统"); // 调试日志，可能会产生大量输出，通常应注释掉

            // 发送一个通用的游戏更新事件，所有关心帧更新的系统都可以监听此事件
            this.SendEvent<GameUpdateEvent>(new GameUpdateEvent
            {
                DeltaTime = Time.deltaTime * gameSpeed // 传递调整过游戏速度的帧间隔时间
            });
            
            // 逐个更新各个游戏系统
            var zombieSystem = this.GetSystem<IZombieSystem>();
            zombieSystem.Update(); // 更新僵尸系统逻辑
            
            UpdateZombieVisualization(); // 更新僵尸的视觉表现
            UpdateZombieAI();            // 更新僵尸的AI逻辑
            
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            defenseSystem.Update(); // 更新防御系统逻辑
            
            var techSystem = this.GetSystem<IAdvancedTechSystem>();
            techSystem.Update(); // 更新科技系统逻辑
            
            var enhancedBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            enhancedBuildingSystem.Update(); // 更新增强型建筑系统逻辑
        }
        
        /// <summary>
        /// 更新僵尸的视觉表现。
        /// 尝试使用正式的IZombieVisualizationSystem，如果未注册，则回退到简化的可视化方法。
        /// </summary>
        private void UpdateZombieVisualization()
        {
            try
            {
                // 尝试获取并使用正式的僵尸可视化系统
                var visualizationSystem = this.GetSystem<SurvivalGame.Visualization.IZombieVisualizationSystem>();
                visualizationSystem.UpdateAllZombieVisuals(); // 调用其更新所有僵尸视觉的方法
            }
            catch (System.Exception e) // 如果获取系统失败（例如未注册）
            {
                // 记录警告并使用简化的可视化方法作为后备
                UnityEngine.Debug.LogWarning($"正式的僵尸可视化系统(IZombieVisualizationSystem)未找到或发生错误，将使用简化版可视化。错误: {e.Message}");
                UpdateSimpleZombieVisualization();
            }
        }
        
        /// <summary>
        /// 简化的僵尸可视化更新逻辑。
        /// 遍历所有存活的僵尸，并为它们创建或更新一个简单的球体作为视觉代表。
        /// </summary>
        private void UpdateSimpleZombieVisualization()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            var zombieData = zombieSystem.GetZombieData(); // 获取僵尸数据集合
            
            // 遍历所有僵尸数据
            foreach (var zombie in zombieData.zombies.Values) // 假设zombies是字典或类似结构
            {
                if (zombie.IsAlive) // 只处理存活的僵尸
                {
                    CreateSimpleZombieVisual(zombie); // 为其创建或更新简单的视觉对象
                }
                // TODO: 可能需要处理已死亡僵尸的视觉对象移除
            }
        }
        
        /// <summary>
        /// 更新僵尸的AI（人工智能）逻辑。
        /// 尝试使用正式的IZombieAIEnhancementSystem，如果未注册，则跳过AI更新。
        /// </summary>
        private void UpdateZombieAI()
        {
            try
            {
                // 尝试获取并使用正式的僵尸AI增强系统
                var aiSystem = this.GetSystem<SurvivalGame.AI.IZombieAIEnhancementSystem>();
                aiSystem.UpdateZombieAI();                 // 更新单个僵尸AI
                aiSystem.ProcessZombieGroupBehavior();     // 处理僵尸群体行为
                aiSystem.HandleZombieSpecialAbilities(); // 处理僵尸特殊能力
            }
            catch (System.Exception) // 如果获取系统失败
            {
                // 记录日志并跳过（不使用警告，因为这可能是可选系统）
                UnityEngine.Debug.Log("僵尸AI增强系统(IZombieAIEnhancementSystem)未注册，跳过AI更新。");
            }
        }
        
        // 用于存储临时僵尸视觉对象的字典，键为僵尸ID，值为对应的GameObject
        private Dictionary<string, GameObject> tempZombieVisuals = new Dictionary<string, GameObject>();
        
        /// <summary>
        /// 创建或更新一个僵尸的简化视觉表示（使用基本球体）。
        /// </summary>
        /// <param name="zombie">要可视化的僵尸数据。</param>
        private void CreateSimpleZombieVisual(ZombieData zombie)
        {
            // 检查是否已为该僵尸创建过视觉对象
            if (tempZombieVisuals.ContainsKey(zombie.id))
            {
                var existingGO = tempZombieVisuals[zombie.id];
                if (existingGO != null) // 如果对象仍然存在
                {
                    // 更新其位置到僵尸当前位置
                    existingGO.transform.position = new Vector3(zombie.position.x, zombie.position.y, 0);
                    return; // 完成更新，无需重新创建
                }
                else
                {
                    tempZombieVisuals.Remove(zombie.id); // 如果对象已被销毁，从字典中移除
                }
            }
            
            // 如果尚未创建或对象已销毁，则创建新的视觉对象
            GameObject zombieGO = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 创建一个球体
            zombieGO.name = $"TempZombie_{zombie.type}_{zombie.id}"; // 命名以便于调试识别
            zombieGO.transform.position = new Vector3(zombie.position.x, zombie.position.y, 0); // 设置位置
            zombieGO.transform.localScale = Vector3.one * 0.5f; // 设置默认大小
            
            // 根据僵尸类型设置不同的颜色和大小
            var renderer = zombieGO.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null) // 安全检查
            {
                switch (zombie.type)
                {
                    case ZombieType.Walker:  renderer.material.color = Color.gray; break;
                    case ZombieType.Runner:  renderer.material.color = Color.red; break;
                    case ZombieType.Tank:    renderer.material.color = Color.green; zombieGO.transform.localScale = Vector3.one * 0.8f; break;
                    case ZombieType.Spitter: renderer.material.color = Color.yellow; break;
                    case ZombieType.Screamer:renderer.material.color = Color.magenta; break;
                    default: renderer.material.color = Color.black; break; // 未知类型为黑色
                }
            }

            tempZombieVisuals[zombie.id] = zombieGO; // 将新创建的对象存入字典
            // UnityEngine.Debug.Log($"创建临时僵尸视觉对象: {zombie.type} 于位置 {zombie.position}"); // 日志（可能产生大量输出）
        }
        
        /// <summary>
        /// 更新游戏内的时间，包括天数和当天的时间进度。
        /// </summary>
        private void UpdateGameTime()
        {
            float deltaTime = Time.deltaTime * gameSpeed; // 计算考虑游戏速度的帧时间差
            // mTimeAccumulator += deltaTime; // 此累加器当前未使用，注释掉以避免混淆
            
            // 计算自当天开始以来经过的游戏时间
            float timeInDay = (Time.time - mDayStartTime) * gameSpeed;
            // 计算当天的时间进度 (0到1之间，0为当天开始，1为当天结束)
            float dayProgress = (timeInDay % dayDuration) / dayDuration;
            mGameModel.GameTime.Value = dayProgress; // 更新数据模型中的当天时间进度
            
            // 检查是否已度过一整天
            if (timeInDay >= dayDuration)
            {
                StartNewDay(); // 如果是，则开始新的一天
            }
        }
        
        /// <summary>
        /// 更新独立的时间系统，并发送时间更新事件。
        /// </summary>
        private void UpdateTimeSystem()
        {
            // 发送TimeUpdateEvent事件，包含帧时间差和总游戏时间（此处用Time.time，可能需调整为游戏内总时间）
            this.SendEvent(new TimeUpdateEvent
            {
                DeltaTime = Time.deltaTime * gameSpeed, // 调整过游戏速度的帧间隔
                TotalTime = Time.time // Unity引擎的真实总时间，如果需要游戏内总时间，需另外累计
            });
        }
        
        /// <summary>
        /// 开始新的一天的逻辑处理。
        /// </summary>
        private void StartNewDay()
        {
            mGameModel.GameDay.Value++; // 游戏天数加一
            mDayStartTime = Time.time;  // 重置当天开始的真实世界时间戳
            
            // 发送新的一天事件，包含新的天数
            this.SendEvent(new NewDayEvent
            {
                Day = mGameModel.GameDay.Value
            });
            
            UnityEngine.Debug.Log($"游戏进入第 {mGameModel.GameDay.Value} 天。");
        }
        
        /// <summary>
        /// 处理玩家的各种输入。
        /// </summary>
        private void HandleInput()
        {
            HandleCameraControls(); // 处理相机控制相关的输入
            HandleGameControls();   // 处理游戏控制和调试快捷键相关的输入
        }
        
        /// <summary>
        /// 处理相机控制的输入，如移动和缩放。
        /// </summary>
        private void HandleCameraControls()
        {
            if (gameCamera == null) return; // 如果没有相机引用，则不执行
            
            // 使用WASD键移动相机 (2D平面移动)
            Vector3 moveDirection = Vector3.zero; // 初始化移动方向向量
            if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.up;    // W向上
            if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.down;  // S向下
            if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;  // A向左
            if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right; // D向右
            
            // 如果有移动输入，则移动相机
            if (moveDirection != Vector3.zero)
            {
                float moveSpeed = 10f * Time.deltaTime; // 相机移动速度 (单位/秒 * 帧时间)
                gameCamera.transform.Translate(moveDirection * moveSpeed, Space.World); // 按世界坐标系移动
            }
            
            // 使用鼠标滚轮进行缩放 (针对2D正交相机)
            float scrollInput = Input.GetAxis("Mouse ScrollWheel"); // 获取鼠标滚轮输入值
            if (scrollInput != 0) // 如果有滚轮输入
            {
                float zoomSpeed = 2f; // 缩放速度因子
                gameCamera.orthographicSize -= scrollInput * zoomSpeed; // 调整正交相机大小以实现缩放
                //限制相机大小在合理范围内
                gameCamera.orthographicSize = Mathf.Clamp(gameCamera.orthographicSize, 3f, 20f);
            }
        }
        
        /// <summary>
        /// 处理游戏控制相关的输入，如暂停、改变速度、打开UI面板和各种调试快捷键。
        /// </summary>
        private void HandleGameControls()
        {
            // 空格键：切换游戏的暂停/继续状态
            if (Input.GetKeyDown(KeyCode.Space))
            {
                mGameModel.IsPaused.Value = !mGameModel.IsPaused.Value;
            }
            
            // 数字键1, 2, 3：调整游戏速度
            if (Input.GetKeyDown(KeyCode.Alpha1)) gameSpeed = 1f; // 正常速度
            else if (Input.GetKeyDown(KeyCode.Alpha2)) gameSpeed = 2f; // 2倍速
            else if (Input.GetKeyDown(KeyCode.Alpha3)) gameSpeed = 3f; // 3倍速
            
            // T键：打开/关闭科技树UI面板 (通过发送消息给TechUIController)
            if (Input.GetKeyDown(KeyCode.T))
            {
                var techUIController = FindFirstObjectByType<TechUIController>(); // 查找场景中的TechUIController实例
                if (techUIController != null)
                {
                    // 调用其ToggleTechPanel方法（如果存在），不要求接收者必须存在以避免错误
                    techUIController.SendMessage("ToggleTechPanel", SendMessageOptions.DontRequireReceiver);
                }
            }
            
            // Shift + S组合键：打开/关闭幸存者管理面板
            if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
            {
                ToggleSurvivorPanel();
            }
            
            // 处理各个子系统的测试快捷键
            HandleZombieTestControls();  // 僵尸系统相关测试
            HandleDefenseTestControls(); // 防御系统相关测试
            HandleTechTestControls();    // 科技系统相关测试
            // HandleBuildingTestControls(); // 建筑系统测试快捷键（当前注释掉）
        }
        
        /// <summary>
        /// 处理与僵尸系统相关的测试快捷键。
        /// </summary>
        private void HandleZombieTestControls()
        {
            // Z键：在鼠标位置生成一个普通僵尸(Walker)
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Vector3 mousePos = gameCamera.ScreenToWorldPoint(Input.mousePosition); // 将鼠标屏幕坐标转为世界坐标
                mousePos.z = 0; // 确保z轴为0 (2D平面)
                this.SendCommand(new SpawnZombieCommand(ZombieType.Walker, mousePos)); // 发送生成僵尸命令
            }
            
            // X键：在鼠标位置生成一个中等威胁等级的僵尸群
            if (Input.GetKeyDown(KeyCode.X))
            {
                Vector3 mousePos = gameCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                this.SendCommand(new SpawnZombieHordeCommand(ZombieThreatLevel.Medium, mousePos));
            }
            
            // C键：触发一次高威胁等级的僵尸潮
            if (Input.GetKeyDown(KeyCode.C))
            {
                this.SendCommand(new TriggerZombieWaveCommand(ZombieThreatLevel.High));
            }
            
            // V键：强制游戏系统更新当前威胁等级
            if (Input.GetKeyDown(KeyCode.V))
            {
                this.SendCommand(new UpdateThreatLevelCommand());
            }
            
            // B键：清理所有已死亡的僵尸
            if (Input.GetKeyDown(KeyCode.B))
            {
                this.SendCommand(new CleanupDeadZombiesCommand());
            }
        }
        
        /// <summary>
        /// 处理与防御系统相关的测试快捷键。
        /// </summary>
        private void HandleDefenseTestControls()
        {
            Vector3 mousePos = gameCamera.ScreenToWorldPoint(Input.mousePosition); // 获取鼠标世界位置
            mousePos.z = 0; // 确保z轴为0

            // T键：在鼠标位置建造基础防御塔
            if (Input.GetKeyDown(KeyCode.T)) { this.SendCommand(new BuildTowerCommand { Position = mousePos, TowerType = TowerType.Basic }); }
            // Y键：建造重型防御塔
            if (Input.GetKeyDown(KeyCode.Y)) { this.SendCommand(new BuildTowerCommand { Position = mousePos, TowerType = TowerType.Heavy }); }
            // U键：建造狙击塔
            if (Input.GetKeyDown(KeyCode.U)) { this.SendCommand(new BuildTowerCommand { Position = mousePos, TowerType = TowerType.Sniper }); }
            // I键：建造溅射塔
            if (Input.GetKeyDown(KeyCode.I)) { this.SendCommand(new BuildTowerCommand { Position = mousePos, TowerType = TowerType.Splash }); }
            
            // O键：获取并打印防御统计数据
            if (Input.GetKeyDown(KeyCode.O)) { this.SendCommand(new GetDefenseStatsCommand()); }
            // P键：获取并打印所有防御塔的信息
            if (Input.GetKeyDown(KeyCode.P)) { this.SendCommand(new GetAllTowersCommand()); }
            // N键：修理所有防御塔
            if (Input.GetKeyDown(KeyCode.N)) { this.SendCommand(new RepairAllTowersCommand()); }
            
            // M键：切换全局目标优先级 (此功能当前被注释，待完善)
            /*
            if (Input.GetKeyDown(KeyCode.M))
            {
                var defenseSystem = this.GetSystem<IDefenseSystem>();
                var stats = defenseSystem.GetDefenseStats();
                // 临时注释，等待DefenseStats模型完善后启用
                Debug.Log("切换目标优先级功能待完善");
            }
            */
        }
        
        /// <summary>
        /// 处理与科技系统相关的测试快捷键。
        /// </summary>
        private void HandleTechTestControls()
        {
            // R键：开始研究"基础农业"科技 (假设研究建筑和研究员ID通过其他方式处理或此处为简化版)
            if (Input.GetKeyDown(KeyCode.R) && !Input.GetKey(KeyCode.LeftControl)) // 避免与Ctrl+R冲突
            {
                this.SendCommand(new StartResearchCommand("basic_farming", "ResearchLab_Default", null)); // 提供默认研究建筑ID，研究员列表为null
            }
            
            // E键：开始研究"基础工艺"科技
            if (Input.GetKeyDown(KeyCode.E) && !Input.GetKey(KeyCode.LeftControl)) // 避免与Ctrl+E冲突
            {
                this.SendCommand(new StartResearchCommand("basic_crafting", "ResearchLab_Default", null));
            }
            
            // Q键：开始研究"基础医学"科技
            if (Input.GetKeyDown(KeyCode.Q))
            {
                this.SendCommand(new StartResearchCommand("basic_medicine", "ResearchLab_Default", null));
            }
            
            // F键：快速完成"基础农业"科技的研究 (示例，实际应完成当前正在研究的科技)
            if (Input.GetKeyDown(KeyCode.F))
            {
                this.SendCommand(new CompleteResearchCommand { TechId = "basic_farming" });
            }
            
            // G键：添加100点研究点数
            if (Input.GetKeyDown(KeyCode.G))
            {
                this.SendCommand(new AddResearchPointsCommand { Points = 100f });
            }
            
            // Ctrl + R组合键：批量开始研究一组基础科技
            if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
            {
                var basicTechs = new List<string> { "basic_farming", "basic_crafting", "basic_medicine", "basic_defense", "basic_energy" };
                // 注意：BatchResearchCommand可能需要每个科技都指定研究建筑和研究员，或TechSystem内部有默认处理逻辑
                this.SendCommand(new BatchResearchCommand { TechIds = basicTechs });
            }
            
            // Ctrl + E组合键：批量开始研究一组进阶科技
            if (Input.GetKeyDown(KeyCode.E) && Input.GetKey(KeyCode.LeftControl))
            {
                var advancedTechs = new List<string> { "advanced_farming", "mechanized_production", "advanced_medicine", "reinforced_defense", "renewable_energy" };
                this.SendCommand(new BatchResearchCommand { TechIds = advancedTechs });
            }
        }
        
        // 建筑系统测试快捷键部分被注释掉了，如果需要可以取消注释并实现
        // private void HandleBuildingTestControls()
        // { ... }

        // === 公共API方法 ===

        /// <summary>
        /// 暂停游戏。
        /// </summary>
        public void PauseGame()
        {
            mGameModel.IsPaused.Value = true;
        }
        
        /// <summary>
        /// 继续游戏。
        /// </summary>
        public void ResumeGame()
        {
            mGameModel.IsPaused.Value = false;
        }
        
        /// <summary>
        /// 设置游戏速度。
        /// </summary>
        /// <param name="speed">新的游戏速度倍率，会被限制在0.1到5之间。</param>
        public void SetGameSpeed(float speed)
        {
            gameSpeed = Mathf.Clamp(speed, 0.1f, 5f); // 限制速度范围
        }
        
        /// <summary>
        /// 向游戏中添加指定数量的资源。
        /// </summary>
        /// <param name="type">要添加的资源类型。</param>
        /// <param name="amount">要添加的数量。</param>
        public void AddResources(ResourceType type, int amount)
        {
            mResourceSystem.AddResource(type, amount); // 调用资源系统接口添加资源
        }
        
        /// <summary>
        /// 切换幸存者管理UI面板的显示状态。
        /// </summary>
        public void ToggleSurvivorPanel()
        {
            // 尝试查找场景中名为"SurvivorUIManager"的游戏对象
            GameObject survivorUIObject = GameObject.Find("SurvivorUIManager");
            if (survivorUIObject != null)
            {
                // 如果找到，则向其发送"ToggleSurvivorPanel"消息，不要求必须有接收者以避免错误
                survivorUIObject.SendMessage("ToggleSurvivorPanel", SendMessageOptions.DontRequireReceiver);
                UnityEngine.Debug.Log("已请求切换幸存者管理面板的显示状态。");
            }
            else
            {
                UnityEngine.Debug.LogWarning("未能找到名为 'SurvivorUIManager' 的游戏对象，无法切换幸存者面板。请确保场景中存在此对象并已正确配置。");
            }
        }
        
        // === IMGUI调试信息显示 ===
        // 注意：此OnGUI方法与文件顶部的OnGUI方法存在重复。
        // 此处使用Conditional特性，使其仅在UNITY_EDITOR宏定义时编译，通常用于编辑器内调试。
        // 上方的OnGUI方法似乎用于字体初始化和uGUI切换，其用途和执行条件需要明确。
        // 假设此处的OnGUI是主要的调试信息显示。
       [System.Diagnostics.Conditional("UNITY_EDITOR")] // 使此方法仅在编辑器环境下编译
        private void OnGUI_Debug() // 重命名以避免与之前的OnGUI冲突，或需合并逻辑
        {
            // 如果游戏不在运行状态，则不显示调试GUI
            if (!Application.isPlaying || _useUGUI) return; // 如果使用uGUI，也不显示IMGUI调试

            // 定义调试信息显示区域
            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 400), "游戏调试信息", GUI.skin.window); // 右上角区域
            // GUILayout.BeginArea(new Rect(10, Screen.height - 210, 300, 200)); // 左下角区域 (备选)
            
            GUILayout.Label($"游戏暂停: {mGameModel.IsPaused.Value}");
            GUILayout.Label($"游戏速度: {gameSpeed:F1}x");
            GUILayout.Label($"第 {mGameModel.GameDay.Value} 天");
            GUILayout.Label($"当日时间进度: {mGameModel.GameTime.Value:P0}"); // P0格式化为百分比，0位小数
            GUILayout.Label($"当前建筑数量: {mGameModel.Buildings.Count}"); // 假设mGameModel.Buildings存在

            GUILayout.Space(10); // 添加一些间距
            
            // 添加一些调试按钮
            if (GUILayout.Button("添加调试资源 (食物+材料+弹药)"))
            {
                AddResources(ResourceType.Food, 100);
                AddResources(ResourceType.Materials, 100);
                AddResources(ResourceType.Ammunition, 50);
            }
            
            if (GUILayout.Button(mGameModel.IsPaused.Value ? "继续游戏 (空格)" : "暂停游戏 (空格)"))
            {
                mGameModel.IsPaused.Value = !mGameModel.IsPaused.Value;
            }
            
            GUILayout.EndArea();
            
            // 控制按键说明区域
            GUILayout.BeginArea(new Rect(10, Screen.height - 250, 400, 240), "测试控制说明 (部分)", GUI.skin.window);
            GUILayout.Label("相机: WASD移动, 滚轮缩放");
            GUILayout.Label("游戏: 空格-暂停/继续, 1/2/3-速度");
            GUILayout.Label("界面: T-科技树, Shift+S-幸存者面板");
            GUILayout.Label("僵尸测试: Z-生成单个, X-生成群, C-僵尸潮, V-更新威胁, B-清理");
            GUILayout.Label("防御测试: T/Y/U/I-建塔, O-统计, P-列表, N-修理");
            GUILayout.Label("科技测试: R/E/Q-研究, F-完成, G-加点, Ctrl+R/E-批量研究");
            // 建筑测试快捷键被注释，此处不列出
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// 实现IController接口，返回QFramework的全局架构实例。
        /// </summary>
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
        
        /// <summary>
        /// Unity生命周期方法：当对象将被销毁时调用。
        /// 可用于清理资源或注销事件等。
        /// </summary>
        private void OnDestroy()
        {
            // TODO: 在此清理可能持有的资源或注销事件监听器
            // 例如： if (tempZombieVisuals != null) { foreach(var pair in tempZombieVisuals) Destroy(pair.Value); tempZombieVisuals.Clear(); }
        }
    }
}

// === 游戏管理相关的事件定义 ===
// (通常建议将事件定义在专门的事件文件或共享的事件命名空间中)
namespace MyGameNamespace
{
    /// <summary>
    /// 时间更新事件。
    /// 每帧由GameManager发送，携带时间增量和总时间信息。
    /// </summary>
    public struct TimeUpdateEvent
    {
        /// <summary>
        /// 当前帧的时间增量（考虑了游戏速度gameSpeed）。
        /// </summary>
        public float DeltaTime;
        /// <summary>
        /// 当前游戏的总运行时间（基于Time.time，可能需要根据游戏设计调整为游戏内累计时间）。
        /// </summary>
        public float TotalTime;
    }
    
    /// <summary>
    /// 新的一天开始事件。
    /// 当游戏内时间进入新的一天时发送。
    /// </summary>
    public struct NewDayEvent
    {
        /// <summary>
        /// 新的当前天数。
        /// </summary>
        public int Day;
    }
}