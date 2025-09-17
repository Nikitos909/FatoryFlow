using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    private int count;

    private void OnEnable()
    {
        // Подписываемся на событие из EconomyManager
        EconomyManager.OnMoneyChanged += UpdateMoneyText;
    }

    private void OnDisable()
    {
        // Обязательно отписываемся
        EconomyManager.OnMoneyChanged -= UpdateMoneyText;
    }

    private void Start()
    {
        // Обновляем текст при старте
        UpdateMoneyText(EconomyManager.Instance.CurrentMoney);
    }

    private void UpdateMoneyText(int newMoneyValue)
    {
        moneyText.text = $"$: {newMoneyValue}";
    }
}
