using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private GameData _currentGameData;
    private string _saveFilePath;

    private const string SAVE_FILE_NAME = "factory_save.json";

    private void Awake()
    {
        // Реализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Чтобы не уничтожался при загрузке новых сцен
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // Формируем путь к файлу сохранения
        _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        Debug.Log($"Save file path: {_saveFilePath}");
    }

    // Сохраняем текущие данные игры в файл
    public void SaveGame()
    {
        if (_currentGameData == null)
        {
            Debug.LogWarning("No game data to save!");
            return;
        }

        try
        {
            // Конвертируем объект в JSON строку
            string jsonData = JsonUtility.ToJson(_currentGameData, prettyPrint: true);
            // Записываем в файл
            File.WriteAllText(_saveFilePath, jsonData);

            Debug.Log("Game saved successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }

    // Загружаем данные из файла
    public bool LoadGame()
    {
        if (!File.Exists(_saveFilePath))
        {
            Debug.Log("No save file found. Starting new game.");
            return false;
        }

        try
        {
            string jsonData = File.ReadAllText(_saveFilePath);
            _currentGameData = JsonUtility.FromJson<GameData>(jsonData);

            Debug.Log("Game loaded successfully!");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            // Создаем новые данные если загрузка не удалась
            CreateNewGame();
            return false;
        }
    }

    // Создаем новые данные для начала игры
    public void CreateNewGame()
    {
        _currentGameData = new GameData();
        Debug.Log("New game data created!");
    }

    // Удаляем файл сохранения
    public void DeleteSave()
    {
        if (File.Exists(_saveFilePath))
        {
            File.Delete(_saveFilePath);
            _currentGameData = null;
            Debug.Log("Save file deleted!");
        }
    }

    // Проверяем существование файла сохранения
    public bool SaveFileExists()
    {
        return File.Exists(_saveFilePath);
    }

    // === Public методы для доступа к данным ===

    public GameData GetCurrentGameData()
    {
        // Если данных нет - создаем новые
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

    // Быстрое сохранение (можно вызывать откуда угодно)
    public static void QuickSave()
    {
        if (Instance != null)
        {
            Instance.SaveGame();
        }
    }

    // Быстрая загрузка
    public static bool QuickLoad()
    {
        return Instance != null && Instance.LoadGame();
    }

    // Автосохранение при выходе из игры
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    // Также хорошо бы сохраняться при паузе (для мобильных устройств)
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
}
