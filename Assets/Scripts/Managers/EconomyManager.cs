using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    public int currentMoney = 1000;
    public TextMeshProUGUI moneyText;
    public int workerSalary = 30;
    public float salaryInterval = 20f;
    private float salaryTimer;

    void Awake()
    {
        Instance = this;
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
        Debug.Log($"💰 +{amount}₽");
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            Debug.Log($"💸 -{amount}₽");
            return true;
        }
        return false;
    }

    private void PaySalaries()
    {
        int logistCount = FindObjectsOfType<Logist>().Length;
        int totalSalary = workerSalary * logistCount;

        if (SpendMoney(totalSalary))
        {
            Debug.Log($"Выплачена зарплата {logistCount} логистам: {totalSalary}₽");
        }
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"Деньги: {currentMoney}₽";
    }

    public void SellProduct(Product product)
    {
        int value = product.isDefective ? product.baseValue / 3 : product.baseValue;
        AddMoney(value);
        Debug.Log($"Продажа: {product.type} за {value}₽");
    }
}