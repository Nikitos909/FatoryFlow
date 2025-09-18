using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameConfig Config;
    public GameData Data;

    public ProductionManager ProductionManager;
    public LogisticsManager LogisticsManager;
    public EconomyManager EconomyManager;
    public UIManager UIManager;
    public DifficultyService DifficultyService;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManagers()
    {
        // Загружаем конфиги
        Config = Resources.Load<GameConfig>("Configs/GameConfig");
        
        // Создаем данные игры
        Data = new GameData();
        
        // Инициализируем сервисы
        DifficultyService = new DifficultyService();
        
        // Получаем ссылки на менеджеры
        ProductionManager = GetComponent<ProductionManager>();
        LogisticsManager = GetComponent<LogisticsManager>();
        EconomyManager = GetComponent<EconomyManager>();
        UIManager = GetComponent<UIManager>();
        
        // Запускаем игру
        StartGame();
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
