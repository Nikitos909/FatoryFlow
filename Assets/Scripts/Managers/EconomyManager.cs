using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    public int currentMoney = 1000;
    public TextMeshProUGUI moneyText;

    [Header("Расходы")]
    public int workerSalary = 50;
    public float salaryInterval = 30f;
    private float salaryTimer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        UpdateMoneyUI();
    }

    void Update()
    {
        salaryTimer -= Time.deltaTime;
        if (salaryTimer <= 0f)
        {
            PaySalaries();
            salaryTimer = salaryInterval;
        }
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
        Debug.Log($"Получено денег: +{amount}₽");
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            Debug.Log($"Потрачено денег: -{amount}₽");
            return true;
        }
        return false;
    }

    private void PaySalaries()
    {
        Logist[] logists = FindObjectsOfType<Logist>();
        int totalSalary = workerSalary * logists.Length;

        if (SpendMoney(totalSalary))
        {
            Debug.Log($"Выплачена зарплата {logists.Length} логистам: -{totalSalary}₽");
        }
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"Деньги: {currentMoney}₽";
    }

    public void OnProductSold(Product product)
    {
        int value = product.isDefective ? product.baseValue / 2 : product.baseValue;
        AddMoney(value);
        Debug.Log($"Продукт продан за {value}₽");
    }
}