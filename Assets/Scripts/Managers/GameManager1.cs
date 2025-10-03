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
            DontDestroyOnLoad(gameObject); // Если это нужно для вашей структуры сцен
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeGame();
    }

    private void InitializeGame()
    {
        // Инициализация менеджеров в правильном порядке, если это необходимо
        economyManager.Initialize();
        logisticsManager.Initialize();
        resourceWarehouse.Initialize();
    }
}
