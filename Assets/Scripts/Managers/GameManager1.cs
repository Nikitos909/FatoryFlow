using UnityEngine;

public class GameManager1 : MonoBehaviour
{
    public static GameManager1 Instance;

    [Header("Managers")]
    public EconomyManager1 economyManager;
    public LogisticsManager1 logisticsManager;
    public ResourceWarehouse1 resourceWarehouse;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("Инициализация игры...");
        
        // Явно находим менеджеры на сцене если не присвоены в инспекторе
        if (economyManager == null) economyManager = FindObjectOfType<EconomyManager1>();
        if (logisticsManager == null) logisticsManager = FindObjectOfType<LogisticsManager1>();
        if (resourceWarehouse == null) resourceWarehouse = FindObjectOfType<ResourceWarehouse1>();

        economyManager?.Initialize();
        logisticsManager?.Initialize();
        resourceWarehouse?.Initialize();
        
        Debug.Log("Игра инициализирована");
    }
}
