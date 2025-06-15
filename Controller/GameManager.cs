using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using QFramework;
using MyGameNamespace;
using SurvivalGame.Model;
using SurvivalGame.GameSystem;
using SurvivalGame;
using SurvivalGame.Command;
using Debug = UnityEngine.Debug;

namespace SurvivalGame.Controller
{
    /// <summary>
    /// 功能：游戏总管理器，负责游戏初始化、时间推进、系统协调
    /// 挂载对象：场景中的GameManager空对象
    /// 依赖系统：ISurvivalGameModel、ITimeSystem
    /// </summary>
    public class GameManager : MonoBehaviour, IController,ICanSendEvent
    {
        [Header("游戏设置")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private float gameSpeed = 1f;
        [SerializeField] private float dayDuration = 120f; // 每天持续时间（秒）
        
        [Header("相机设置")]
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Vector3 cameraStartPosition = new Vector3(0, 0, -10);
        [SerializeField] private Vector3 cameraStartRotation = new Vector3(0, 0, 0);
        
        // 框架引用
        private ISurvivalGameModel mGameModel;
        private IResourceSystem mResourceSystem;
        private IEnhancedBuildingSystem mBuildingSystem;
        private ITimeSystem mTimeSystem;
        
        // 时间管理
        private float mTimeAccumulator = 0f;
        private float mDayStartTime;
        
        private void Awake()
        {
            // 获取框架组件（会自动触发架构初始化）
            mGameModel = this.GetModel<ISurvivalGameModel>();
            mResourceSystem = this.GetSystem<IResourceSystem>();
            mBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            mTimeSystem = this.GetSystem<ITimeSystem>();
        }
        
        private void Start()
        {
            InitializeGame();
            
            // 添加调试和诊断组件
           // gameObject.AddComponent<SurvivalGame.DebugTools.ZombieDebugHelper>();
           // gameObject.AddComponent<SurvivalGame.DebugTools.ZombieLocator>();
           // gameObject.AddComponent<SurvivalGame.DebugTools.ZombieVisualizationDiagnostic>(); // 添加可视化诊断工具
           // gameObject.AddComponent<SurvivalGame.DebugTools.ZombieCollisionTest>(); // 添加碰撞检测测试工具（暂时注释）
           // gameObject.AddComponent<SurvivalGame.DebugTools.CollisionTestGuide>(); // 添加碰撞测试指南（暂时注释）
            
            if (autoStart)
            {
                StartGame();
            }
        }
        
        private void InitializeGame()
        {
            // 设置相机
            SetupCamera();
            
            // 创建地面
            CreateGround();
            
            // 初始化游戏状态
            mDayStartTime = Time.time;
            
            // 建造初始建筑
            BuildInitialStructures();
            
//            UnityEngine.Debug.Log("游戏初始化完成");
        }
        
        private void SetupCamera()
        {
            if (gameCamera == null)
                gameCamera = Camera.main;
            
            if (gameCamera != null)
            {
                gameCamera.transform.position = cameraStartPosition;
                gameCamera.transform.eulerAngles = cameraStartRotation;
                gameCamera.orthographic = true; // 设置为正交投影（2D）
                gameCamera.orthographicSize = 10f; // 设置2D相机大小
            }
        }
        
        private void CreateGround()
        {
            // 创建2D地面背景 (Unity 2023.2优化版本)
            GameObject ground = new GameObject("Ground");
            ground.transform.position = Vector3.zero;
            
            // 添加SpriteRenderer组件
            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            
            // 创建一个简单的地面纹理
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            
            // 创建简单的地面图案
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    // 基础泥土色
                    Color baseColor = new Color(0.4f, 0.3f, 0.2f, 1f);
                    
                    // 添加一些随机变化使其更有趣
                    float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.1f;
                    baseColor += new Color(noise, noise, noise, 0);
                    
                    texture.SetPixel(x, y, baseColor);
                }
            }
            
            // Unity 2023.2优化设置
            texture.filterMode = FilterMode.Point; // 像素艺术风格
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            
            // 创建Sprite (Unity 2023.2推荐的像素比率)
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f), 32f); // 使用32像素每单位
            
            spriteRenderer.sprite = sprite;
            spriteRenderer.transform.localScale = new Vector3(2f, 2f, 1f); // 调整缩放
            spriteRenderer.sortingOrder = -10; // 设置在最底层
            spriteRenderer.sortingLayerName = "Background"; // 使用分层渲染
        }
        
        private void BuildInitialStructures()
        {
            // 建造初始避难所
            if (mBuildingSystem.CanBuildAt(new Vector3(0, 0, 0), "Shelter"))
            {
                mBuildingSystem.StartConstruction(new Vector3(0, 0, 0), "Shelter");
            }
            
            // 建造初始农田
            if (mBuildingSystem.CanBuildAt(new Vector3(4, 0, 0), "Farm"))
            {
                mBuildingSystem.StartConstruction(new Vector3(4, 0, 0), "Farm");
            }
        }
        
        private void StartGame()
        {
            mGameModel.IsPaused.Value = false;
//            UnityEngine.Debug.Log("游戏开始");
        }
        
        private void Update()
        {
            if (mGameModel.IsPaused.Value) return;
            
            UpdateGameTime();
            UpdateTimeSystem();
            UpdateGameSystems();
            HandleInput();
        }
        
        private void UpdateGameSystems()
        {
            Debug.Log($"更新游戏系统");
            // 发送游戏更新事件给所有系统
            this.SendEvent<GameUpdateEvent>(new GameUpdateEvent
            {
                DeltaTime = Time.deltaTime * gameSpeed
            });
            
            // 更新僵尸系统
            var zombieSystem = this.GetSystem<IZombieSystem>();
            zombieSystem.Update();
            
            // 更新僵尸可视化系统
            UpdateZombieVisualization();
            
            // 更新僵尸AI增强系统
            UpdateZombieAI();
            
            // 更新防御系统
            var defenseSystem = this.GetSystem<IDefenseSystem>();
            defenseSystem.Update();
            
            // 更新科技系统
            var techSystem = this.GetSystem<IAdvancedTechSystem>();
            techSystem.Update();
            
            // 更新增强建筑系统
            var enhancedBuildingSystem = this.GetSystem<IEnhancedBuildingSystem>();
            enhancedBuildingSystem.Update();
        }
        
        private void UpdateZombieVisualization()
        {
            try
            {
                // 使用正式的可视化系统
                var visualizationSystem = this.GetSystem<SurvivalGame.Visualization.IZombieVisualizationSystem>();
                visualizationSystem.UpdateAllZombieVisuals();
            }
            catch (System.Exception e)
            {
                // 如果可视化系统未注册，使用简化版本
                UnityEngine.Debug.Log($"可视化系统未找到，使用简化版本: {e.Message}");
                UpdateSimpleZombieVisualization();
            }
        }
        
        private void UpdateSimpleZombieVisualization()
        {
            var zombieSystem = this.GetSystem<IZombieSystem>();
            var zombieData = zombieSystem.GetZombieData();
            
            // 临时实现：为每个活着的僵尸创建简单的视觉表示
            foreach (var zombie in zombieData.zombies.Values)
            {
                if (zombie.IsAlive)
                {
                    CreateSimpleZombieVisual(zombie);
                }
            }
        }
        
        private void UpdateZombieAI()
        {
            try
            {
                // 使用AI增强系统
                var aiSystem = this.GetSystem<SurvivalGame.AI.IZombieAIEnhancementSystem>();
                aiSystem.UpdateZombieAI();
                aiSystem.ProcessZombieGroupBehavior();
                aiSystem.HandleZombieSpecialAbilities();
            }
            catch (System.Exception)
            {
                // 如果AI系统未注册，跳过
                UnityEngine.Debug.Log("僵尸AI增强系统未注册，跳过AI更新");
            }
        }
        
        // 简化的僵尸可视化创建方法
        private Dictionary<string, GameObject> tempZombieVisuals = new Dictionary<string, GameObject>();
        
        private void CreateSimpleZombieVisual(ZombieData zombie)
        {
            // 如果已存在，更新位置
            if (tempZombieVisuals.ContainsKey(zombie.id))
            {
                var existingGO = tempZombieVisuals[zombie.id];
                if (existingGO != null)
                {
                    existingGO.transform.position = new Vector3(zombie.position.x, zombie.position.y, 0);
                    return;
                }
            }
            
            // 创建新的僵尸视觉对象
            GameObject zombieGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            zombieGO.name = $"TempZombie_{zombie.type}_{zombie.id}";
            zombieGO.transform.position = new Vector3(zombie.position.x, zombie.position.y, 0);
            zombieGO.transform.localScale = Vector3.one * 0.5f;
            
            // 根据僵尸类型设置颜色
            var renderer = zombieGO.GetComponent<Renderer>();
            switch (zombie.type)
            {
                case ZombieType.Walker:
                    renderer.material.color = Color.gray;
                    break;
                case ZombieType.Runner:
                    renderer.material.color = Color.red;
                    break;
                case ZombieType.Tank:
                    renderer.material.color = Color.green;
                    zombieGO.transform.localScale = Vector3.one * 0.8f;
                    break;
                case ZombieType.Spitter:
                    renderer.material.color = Color.yellow;
                    break;
                case ZombieType.Screamer:
                    renderer.material.color = Color.magenta;
                    break;
            }
            
            tempZombieVisuals[zombie.id] = zombieGO;
            UnityEngine.Debug.Log($"创建临时僵尸视觉对象: {zombie.type} 于位置 {zombie.position}");
        }
        
        private void UpdateGameTime()
        {
            float deltaTime = Time.deltaTime * gameSpeed;
            mTimeAccumulator += deltaTime;
            
            // 更新当日时间进度
            float timeInDay = (Time.time - mDayStartTime) * gameSpeed;
            float dayProgress = (timeInDay % dayDuration) / dayDuration;
            mGameModel.GameTime.Value = dayProgress;
            
            // 检查是否进入新的一天
            if (timeInDay >= dayDuration)
            {
                StartNewDay();
            }
        }
        
        private void UpdateTimeSystem()
        {
            // 发送时间更新事件给系统
            this.SendEvent(new TimeUpdateEvent
            {
                DeltaTime = Time.deltaTime * gameSpeed,
                TotalTime = Time.time
            });
        }
        
        private void StartNewDay()
        {
            mGameModel.GameDay.Value++;
            mDayStartTime = Time.time;
            
            // 新的一天事件
            this.SendEvent(new NewDayEvent
            {
                Day = mGameModel.GameDay.Value
            });
            
            UnityEngine.Debug.Log($"进入第 {mGameModel.GameDay.Value} 天");
        }
        
        private void HandleInput()
        {
            // 处理游戏输入
            HandleCameraControls();
            HandleGameControls();
        }
        
        private void HandleCameraControls()
        {
            if (gameCamera == null) return;
            
            // WASD移动相机 (2D)
            Vector3 moveDirection = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.up;
            if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.down;
            if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;
            if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;
            
            if (moveDirection != Vector3.zero)
            {
                float moveSpeed = 10f * Time.deltaTime;
                gameCamera.transform.Translate(moveDirection * moveSpeed, Space.World);
            }
            
            // 鼠标滚轮缩放 (2D正交相机)
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                float zoomSpeed = 2f;
                gameCamera.orthographicSize -= scrollInput * zoomSpeed;
                gameCamera.orthographicSize = Mathf.Clamp(gameCamera.orthographicSize, 3f, 20f);
            }
        }
        
        private void HandleGameControls()
        {
            // 空格键暂停/继续
            if (Input.GetKeyDown(KeyCode.Space))
            {
                mGameModel.IsPaused.Value = !mGameModel.IsPaused.Value;
            }
            
            // 1,2,3键调整游戏速度
            if (Input.GetKeyDown(KeyCode.Alpha1))
                gameSpeed = 1f;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                gameSpeed = 2f;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                gameSpeed = 3f;
            
            // T键打开科技树
            if (Input.GetKeyDown(KeyCode.T))
            {
                var techUIController = FindFirstObjectByType<TechUIController>();
                if (techUIController != null)
                {
                    techUIController.SendMessage("ToggleTechPanel", SendMessageOptions.DontRequireReceiver);
                }
            }
            
            // Shift+S键：打开幸存者管理面板
            if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
            {
                ToggleSurvivorPanel();
            }
            
            // === 僵尸系统测试快捷键 ===
            HandleZombieTestControls();
            
            // === 防御系统测试快捷键 ===
            HandleDefenseTestControls();
            
            // === 科技系统测试快捷键 ===
            HandleTechTestControls();
            
            // === 建筑系统测试快捷键 ===
            //HandleBuildingTestControls();
        }
        
        private void HandleZombieTestControls()
        {
            // Z键：生成单个僵尸
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                this.SendCommand(new SpawnZombieCommand(ZombieType.Walker, mousePos));
            }
            
            // X键：生成僵尸群
            if (Input.GetKeyDown(KeyCode.X))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                this.SendCommand(new SpawnZombieHordeCommand(ZombieThreatLevel.Medium, mousePos));
            }
            
            // C键：触发僵尸潮
            if (Input.GetKeyDown(KeyCode.C))
            {
                this.SendCommand(new TriggerZombieWaveCommand(ZombieThreatLevel.High));
            }
            
            // V键：强制更新威胁等级
            if (Input.GetKeyDown(KeyCode.V))
            {
                this.SendCommand(new UpdateThreatLevelCommand());
            }
            
            // B键：清理死亡僵尸
            if (Input.GetKeyDown(KeyCode.B))
            {
                this.SendCommand(new CleanupDeadZombiesCommand());
            }
        }
        
        private void HandleDefenseTestControls()
        {
            // T键：建造基础防御塔
            if (Input.GetKeyDown(KeyCode.T))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                this.SendCommand(new BuildTowerCommand { Position = mousePos, TowerType = TowerType.Basic });
            }
            
            // Y键：建造重型防御塔
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                this.SendCommand(new BuildTowerCommand { Position = mousePos, TowerType = TowerType.Heavy });
            }
            
            // U键：建造狙击塔
            if (Input.GetKeyDown(KeyCode.U))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                this.SendCommand(new BuildTowerCommand { Position = mousePos, TowerType = TowerType.Sniper });
            }
            
            // I键：建造溅射塔
            if (Input.GetKeyDown(KeyCode.I))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                this.SendCommand(new BuildTowerCommand { Position = mousePos, TowerType = TowerType.Splash });
            }
            
            // O键：显示防御统计
            if (Input.GetKeyDown(KeyCode.O))
            {
                this.SendCommand(new GetDefenseStatsCommand());
            }
            
            // P键：显示所有防御塔信息
            if (Input.GetKeyDown(KeyCode.P))
            {
                this.SendCommand(new GetAllTowersCommand());
            }
            
            // N键：批量修理所有防御塔
            if (Input.GetKeyDown(KeyCode.N))
            {
                this.SendCommand(new RepairAllTowersCommand());
            }
            
            // M键：切换全局目标优先级 (临时注释)
            /*
            if (Input.GetKeyDown(KeyCode.M))
            {
                var defenseSystem = this.GetSystem<IDefenseSystem>();
                var stats = defenseSystem.GetDefenseStats();
                // 临时注释，等DefenseStats完善后启用
                Debug.Log("切换目标优先级功能待完善");
            }
            */
        }
        
        private void HandleTechTestControls()
        {
            // R键：开始研究基础农业
            if (Input.GetKeyDown(KeyCode.R))
            {
                this.SendCommand(new StartResearchCommand("basic_farming", "基础农业", new List<string>()));
            }
            
            // E键：开始研究基础工艺
            if (Input.GetKeyDown(KeyCode.E))
            {
                this.SendCommand(new StartResearchCommand("basic_crafting", "基础工艺", new List<string>()));
            }
            
            // Q键：开始研究基础医学
            if (Input.GetKeyDown(KeyCode.Q))
            {
                this.SendCommand(new StartResearchCommand("basic_medicine", "基础医学", new List<string>()));
            }
            
            // F键：快速完成当前研究
            if (Input.GetKeyDown(KeyCode.F))
            {
                // 简化实现，直接完成基础农业研究
                this.SendCommand(new CompleteResearchCommand { TechId = "basic_farming" });
            }
            
            // G键：添加研究点数
            if (Input.GetKeyDown(KeyCode.G))
            {
                this.SendCommand(new AddResearchPointsCommand { Points = 100f });
            }
            
            // Ctrl + R：快速研究基础科技
            if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
            {
                var basicTechs = new List<string> 
                { 
                    "basic_farming", "basic_crafting", "basic_medicine", 
                    "basic_defense", "basic_energy" 
                };
                this.SendCommand(new BatchResearchCommand { TechIds = basicTechs });
            }
            
            // Ctrl + E：快速研究进阶科技
            if (Input.GetKeyDown(KeyCode.E) && Input.GetKey(KeyCode.LeftControl))
            {
                var advancedTechs = new List<string> 
                { 
                    "advanced_farming", "mechanized_production", "advanced_medicine", 
                    "reinforced_defense", "renewable_energy" 
                };
                this.SendCommand(new BatchResearchCommand { TechIds = advancedTechs });
            }
        }
        
        // private void HandleBuildingTestControls()
        // {
        //     // 数字键1-9：建造不同类型的建筑
        //     Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     mousePos.z = 0;
        //     
        //     if (Input.GetKeyDown(KeyCode.Alpha1))
        //     {
        //         this.SendCommand(new ConstructBuildingCommand(mousePos, "Shelter"));
        //     }
        //     
        //     if (Input.GetKeyDown(KeyCode.Alpha2))
        //     {
        //         this.SendCommand(new ConstructBuildingCommand(mousePos, "Farm"));
        //     }
        //     
        //     if (Input.GetKeyDown(KeyCode.Alpha3))
        //     {
        //         this.SendCommand(new ConstructBuildingCommand(mousePos, "Workshop"));
        //     }
        //     
        //     if (Input.GetKeyDown(KeyCode.Alpha4))
        //     {
        //         this.SendCommand(new ConstructBuildingCommand(mousePos, "Wall"));
        //     }
        //     
        //     if (Input.GetKeyDown(KeyCode.Alpha5))
        //     {
        //         this.SendCommand(new ConstructBuildingCommand(mousePos, "WatchTower"));
        //     }
        //     
        //     if (Input.GetKeyDown(KeyCode.Alpha6))
        //     {
        //         this.SendCommand(new ConstructBuildingCommand(mousePos, "MedicalStation"));
        //     }
        //     
        //     if (Input.GetKeyDown(KeyCode.Alpha7))
        //     {
        //         this.SendCommand(new ConstructBuildingCommand(mousePos, "Quarry"));
        //     }
        //     
        //     if (Input.GetKeyDown(KeyCode.Alpha8))
        //     {
        //         this.SendCommand(new ConstructBuildingCommand(mousePos, "Library"));
        //     }
        //     
        //     if (Input.GetKeyDown(KeyCode.Alpha9))
        //     {
        //         this.SendCommand(new ConstructBuildingCommand(mousePos, "StorageDepot"));
        //     }
        //     
        //     // B键：快速建造避难所
        //     if (Input.GetKeyDown(KeyCode.B))
        //     {
        //         this.SendCommand(new QuickBuildCommand(mousePos, "Shelter"));
        //     }
        //     
        //     // V键：快速建造农田
        //     if (Input.GetKeyDown(KeyCode.V))
        //     {
        //         this.SendCommand(new QuickBuildCommand(mousePos, "Farm"));
        //     }
        //     
        //     // H键：快速建造工坊 (避免与防御T键冲突)
        //     if (Input.GetKeyDown(KeyCode.H))
        //     {
        //         this.SendCommand(new QuickBuildCommand(mousePos, "Workshop"));
        //     }
        //     
        //     // J键：修复所有建筑 (避免与防御U键冲突)
        //     if (Input.GetKeyDown(KeyCode.J))
        //     {
        //         this.SendCommand(new RepairAllBuildingsCommand());
        //     }
        //     
        //     // K键：升级所有建筑 (避免与防御I键冲突)
        //     if (Input.GetKeyDown(KeyCode.K))
        //     {
        //         this.SendCommand(new UpgradeAllBuildingsCommand());
        //     }
        //     
        //     // Ctrl + B：批量建造农田
        //     if (Input.GetKeyDown(KeyCode.B) && Input.GetKey(KeyCode.LeftControl))
        //     {
        //         this.SendCommand(new BatchBuildCommand("Farm", 5));
        //     }
        //     
        //     // Ctrl + V：批量建造工坊
        //     if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftControl))
        //     {
        //         this.SendCommand(new BatchBuildCommand("Workshop", 3));
        //     }
        //     
        //     // Ctrl + T：批量建造围墙
        //     if (Input.GetKeyDown(KeyCode.T) && Input.GetKey(KeyCode.LeftControl))
        //     {
        //         this.SendCommand(new BatchBuildCommand("Wall", 10));
        //     }
        // }
        
        // 提供给外部调用的方法
        public void PauseGame()
        {
            mGameModel.IsPaused.Value = true;
        }
        
        public void ResumeGame()
        {
            mGameModel.IsPaused.Value = false;
        }
        
        public void SetGameSpeed(float speed)
        {
            gameSpeed = Mathf.Clamp(speed, 0.1f, 5f);
        }
        
        public void AddResources(ResourceType type, int amount)
        {
            mResourceSystem.AddResource(type, amount);
        }
        
        public void ToggleSurvivorPanel()
        {
            // 查找幸存者UI控制器并切换面板状态
            GameObject survivorUIObject = GameObject.Find("SurvivorUIManager");
            if (survivorUIObject != null)
            {
                survivorUIObject.SendMessage("ToggleSurvivorPanel", SendMessageOptions.DontRequireReceiver);
                UnityEngine.Debug.Log("切换幸存者管理面板");
            }
            else
            {
                UnityEngine.Debug.Log("幸存者管理：Shift+S键打开幸存者面板（需要在场景中添加SurvivorUIManager）");
            }
        }
        
        // 调试方法
       [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            // 游戏状态信息
            GUILayout.BeginArea(new Rect(10, 100, 300, 200));
            GUILayout.Label("=== 游戏调试信息 ===");
            GUILayout.Label($"游戏暂停: {mGameModel.IsPaused.Value}");
            GUILayout.Label($"游戏速度: {gameSpeed:F1}x");
            GUILayout.Label($"第 {mGameModel.GameDay.Value} 天");
            GUILayout.Label($"当日进度: {mGameModel.GameTime.Value:P0}");
            GUILayout.Label($"建筑数量: {mGameModel.Buildings.Count}");
            
            GUILayout.Space(10);
            if (GUILayout.Button("添加资源"))
            {
                AddResources(ResourceType.Food, 100);
                AddResources(ResourceType.Materials, 100);
                AddResources(ResourceType.Ammunition, 50);
            }
            
            if (GUILayout.Button("暂停/继续"))
            {
                mGameModel.IsPaused.Value = !mGameModel.IsPaused.Value;
            }
            
            GUILayout.EndArea();
            
            // 控制说明
            GUILayout.BeginArea(new Rect(10, 220, 400, 300));
            GUILayout.Label("=== 控制说明 ===");
            GUILayout.Label("防御系统:");
            GUILayout.Label("  T - 建造基础防御塔");
            GUILayout.Label("  Y - 建造重型防御塔");
            GUILayout.Label("  U - 建造狙击塔");
            GUILayout.Label("  I - 建造溅射塔");
            GUILayout.Label("  O - 显示防御统计");
            GUILayout.Label("  P - 显示防御塔信息");
            GUILayout.Label("僵尸系统:");
            GUILayout.Label("  Z - 生成僵尸");
            GUILayout.Label("  X - 生成僵尸群");
            GUILayout.Label("  C - 显示威胁等级");
            GUILayout.Label("建筑系统:");
            GUILayout.Label("  1-9 - 建造建筑");
            GUILayout.Label("  H - 快速建造工坊");
            GUILayout.Label("  J - 修复所有建筑");
            GUILayout.Label("  K - 升级所有建筑");
            GUILayout.EndArea();
        }
        
        // 实现IController接口
        public IArchitecture GetArchitecture() => RegisterManager.Interface;
        
        private void OnDestroy()
        {
            // 清理资源
        }
    }
}

// 游戏管理相关事件
namespace MyGameNamespace
{
    public struct TimeUpdateEvent
    {
        public float DeltaTime;
        public float TotalTime;
    }
    
    public struct NewDayEvent
    {
        public int Day;
    }
} 