using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance; // Добавляем статический Instance

    public TextMeshProUGUI moneyText;

    private float salaryTimer;
    private GameConfig config;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("Initialize Economy Manager");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(GameConfig gameConfig)
    {
        config = gameConfig;
        salaryTimer = config.salaryInterval;
        UpdateMoneyUI();
    }

    public void UpdateEconomy(float deltaTime)
    {
        if (config == null || GameManager.Instance == null) return;
        
        salaryTimer -= deltaTime;
        if (salaryTimer <= 0f)
        {
            PaySalaries();
            salaryTimer = config.salaryInterval;
        }
    }

    public void AddMoney(int amount)
    {
        GameManager.Instance.gameState.currentMoney += amount;
        UpdateMoneyUI();
        Debug.Log($"💰 +{amount}₽");
    }

    public bool SpendMoney(int amount)
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) { return false; }
    
        if (GameManager.Instance.gameState.currentMoney >= amount)
        {
            GameManager.Instance.gameState.currentMoney -= amount;
            GameManager.Instance.gameState.totalExpenses += amount;
            UpdateMoneyUI();
            return true;
        }
        return false;
    }

    private void PaySalaries()
    {
        if (config == null || GameManager.Instance == null) return;
        
        int logistCount = FindObjectsOfType<Logist>().Length;
        int totalSalary = config.workerSalary * logistCount;
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"Деньги: {GameManager.Instance.gameState.currentMoney}₽";
    }

    public void SellProduct(Product product)
    {
        int value = product.baseValue;
        AddMoney(value);
        Debug.Log($"Продажа: {product.type} за {value}₽");
    }
}
