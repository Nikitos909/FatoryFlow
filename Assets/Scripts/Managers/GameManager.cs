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

    void Update()
    {
        if (State != GameState.Playing) return;
        // Основной игровой цикл
        ProductionManager.Instance.OnUpdate(Time.deltaTime);
        LogisticsManager.Instance.OnUpdate(Time.deltaTime);
        EconomyManager.Instance.OnUpdate(Time.deltaTime);
    }

    public void PauseGame() => State = GameState.Paused;
    public void ResumeGame() => State = GameState.Playing;
    public void CompleteLevel() { /* Сохранить прогресс, показать экран победы */ }
}
