using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State { get; private set; } = GameState.Playing; // Playing, Paused, GameOver
    
    public GameConfig Config;
    public GameData Data;

    public ProductionManager ProductionManager;
    public LogisticsManager LogisticsManager;
    public EconomyManager EconomyManager;
    public UIManager UIManager;
    public DifficultyService DifficultyService;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeManagers();       
    }

    private void InitializeManagers()
    {
        // Загружаем конфиги
        Config = Resources.Load<GameConfig>("Configs/GameConfig");
        
        // Создаем данные игры
        Data = new GameData();
        
        // Инициализация других менеджеров
        SaveManager.Instance.LoadGame();
        EconomyManager.Instance.Initialize(Config);
        ProductionManager.Instance.Initialize();
        LogisticsManager.Instance.Initialize(Config.BaseLogistSpeed);
        DifficultyService.Instance.Initialize(Config);
    }

    private void StartGame()
    {
        EconomyManager.Initialize(Config.StartMoney);
        UIManager.UpdateMoneyDisplay(EconomyManager.CurrentMoney);
        Debug.Log("Game started!");
    }

    private void Update()
    {
        ProductionManager.UpdateProduction(Time.deltaTime);
        LogisticsManager.UpdateLogistics(Time.deltaTime);
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(Data);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }

    public void LoadGame()
    {
        if (System.IO.File.Exists(Application.persistentDataPath + "/save.json"))
        {
            string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/save.json");
            Data = JsonUtility.FromJson<GameData>(json);
        }
    }
}
