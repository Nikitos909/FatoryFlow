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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            EconomyManager.AddMoney(10);
        }
    }
}
