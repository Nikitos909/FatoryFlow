using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Text moneyText;
    public Text profitText;
    public GameObject selectionPanel;
    public GameObject gameOverPanel;

    public void UpdateMoneyUI(int money) => moneyText.text = $"Деньги: {money}₽";

    public void ShowMachineInfo(Machine machine)
    {
        selectionPanel.SetActive(true);
        // Заполняем информацию о станке
    }

    public void UpdateProductionProgress(float progress)
    {
        // Обновляем progress bar
    }

    public void ShowGameOver() => gameOverPanel.SetActive(true);
}
