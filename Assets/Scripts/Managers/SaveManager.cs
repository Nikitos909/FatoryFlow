using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private GameData _currentGameData;
    private string _saveFilePath;

    private const string SAVE_FILE_NAME = "factory_save.json";

    private void Awake() => savePath = Application.persistentDataPath + "/save.json";

    private void Initialize()
    {
        // Ôîðìèðóåì ïóòü ê ôàéëó ñîõðàíåíèÿ
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
        File.WriteAllText(savePath, json);
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            // Восстанавливаем состояние игры
            EconomyManager.Instance.SetMoney(data.Money);
            // ... восстановление станков и логистов
        }
    }


    // Ñîçäàåì íîâûå äàííûå äëÿ íà÷àëà èãðû
    public void CreateNewGame()
    {
        _currentGameData = new GameData();
        Debug.Log("New game data created!");
    }

    // Óäàëÿåì ôàéë ñîõðàíåíèÿ
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

    // === Public ìåòîäû äëÿ äîñòóïà ê äàííûì ===

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

    public static bool QuickLoad()
    {
        return Instance != null && Instance.LoadGame();
    }

    // Àâòîñîõðàíåíèå ïðè âûõîäå èç èãðû
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    // Òàêæå õîðîøî áû ñîõðàíÿòüñÿ ïðè ïàóçå (äëÿ ìîáèëüíûõ óñòðîéñòâ)
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
}
