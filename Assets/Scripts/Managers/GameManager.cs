using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState gameState;
    public GameConfig config;

    public RawMaterialWarehouse warehouse;

    private List<Machine> allMachines = new List<Machine>();

    // Ссылки на менеджеры (перетащи в инспекторе)
    public EconomyManager economyManager;
    public LogisticsManager logisticsManager;
    public Machine cutterMachine;
    public Machine benderMachine;
    public Logist logist;
    public Transform resourceSpawnPoint;

    private float spawnTimer;
    private bool isGameRunning = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeGame();
    }

    private void InitializeGame()
    {
        // Создаем состояние игры
        gameState = new GameState();
        gameState.currentMoney = config.startMoney;

        // Загружаем конфиг если не назначен
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");

        // Инициализируем EconomyManager ПЕРЕД использованием
        if (economyManager != null) { economyManager.Initialize(config); }
        else { Debug.LogError("EconomyManager не назначен в инспекторе!"); }

        // Регистрируем логиста
        if (logisticsManager != null) {  }
        else { Debug.LogError("LogisticsManager не назначен в инспекторе!"); }

        // Автоматически находим все станки на сцене
        allMachines = new List<Machine>(FindObjectsOfType<Machine>());
        
        spawnTimer = config.rawPipeSpawnInterval;
        Debug.Log("🎮 Игра инициализирована!");
    }

    private void Update()
    {
        if (!isGameRunning) return;

        // Проверяем инициализацию менеджеров
        if (economyManager == null || logisticsManager == null) return;

        // Обновляем экономику
        economyManager.UpdateEconomy(Time.deltaTime);
    }

    private Sprite CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(16, 16);
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                texture.SetPixel(x, y, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f);
    }

    public void OnProductSold(Product product)
    {
        economyManager.SellProduct(product);
        gameState.totalRevenue += product.baseValue;
        gameState.ordersCompleted++;
        gameState.UpdateEfficiencyMetrics();
    }

    public void PauseGame() => isGameRunning = false;
    public void ResumeGame() => isGameRunning = true;

    // Можно добавить для статистики
    public int GetWorkingMachinesCount()
    {
        int count = 0;
        foreach (var machine in allMachines)
        {
            if (machine.isWorking) count++;
        }
        return count;
    }

    public void OnBuyRawMaterialButton()
    {
        if (warehouse != null)
        {
            warehouse.BuyRawMaterial();
        }
    }
}
