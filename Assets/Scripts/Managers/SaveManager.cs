using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private GameData _currentGameData;
    private string _saveFilePath;

    private const string SAVE_FILE_NAME = "factory_save.json";

    private void Awake() => _saveFilePath = Application.persistentDataPath + "/save.json";

    private void Initialize()
    {
        _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        Debug.Log($"Save file path: {_saveFilePath}");
    }

    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            Money = EconomyManager.Instance.CurrentMoney,
            Machines = ProductionManager.Instance.AllMachines.Select(m => m.GetData()).ToList(),
            Logists = LogisticsManager.Instance.AllLogists.Select(l => l.GetData()).ToList()
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(_saveFilePath, json);
    }

    public void LoadGame()
    {
        if (File.Exists(_saveFilePath))
        {
            string json = File.ReadAllText(_saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            // Восстанавливаем состояние игры
            EconomyManager.Instance.SetMoney(data.Money);
            // ... восстановление станков и логистов
        }
    }


    public void CreateNewGame()
    {
        _currentGameData = new GameData();
        Debug.Log("New game data created!");
    }

    public void DeleteSave()
    {
        if (File.Exists(_saveFilePath))
        {
            File.Delete(_saveFilePath);
            _currentGameData = null;
            Debug.Log("Save file deleted!");
        }
    }

    public bool SaveFileExists()
    {
        return File.Exists(_saveFilePath);
    }


    public GameData GetCurrentGameData()
    {
        if (_currentGameData == null)
        {
            CreateNewGame();
        }
        return _currentGameData;
    }

    public void UpdateGameData(GameData newData)
    {
        _currentGameData = newData;
    }

    public static void QuickSave()
    {
        if (Instance != null)
        {
            Instance.SaveGame();
        }
    }

    //public static bool QuickLoad()
    //{
        //return Instance != null && Instance.LoadGame();
    //}

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
}
