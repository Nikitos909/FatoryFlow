using System;
using System.Collections.Generic;

// Этот атрибут делает объект сериализуемым в JSON
[Serializable]
public class GameData
{
    // 1. Экономика
    public int Money;
    public int TotalRevenue;
    public int TotalExpenses;

    // 2. Прогресс и ресурсы
    public int CurrentLevel;
    public Dictionary<string, int> ResourceInventory; // Например, "Pipe": 10, "BentSector": 5

    // 3. Состояние фабрики
    public List<MachineSaveData> Machines;
    public List<LogistSaveData> Logists;

    // 4. Статистика и аналитика
    public int ProductsProduced;
    public int DefectsProduced;
    public float TotalPlayTime;

    // 5. Настройки игры (если нужно)
    public float MusicVolume;
    public float SfxVolume;

    // Конструктор для установки значений по умолчанию
    // (используется при начале новой игры)
    public GameData()
    {
        Money = 1000; // Стартовый капитал
        CurrentLevel = 1;
        ResourceInventory = new Dictionary<string, int>();
        Machines = new List<MachineSaveData>();
        Logists = new List<LogistSaveData>();

        // Добавляем стартовые ресурсы, если нужно
        ResourceInventory.Add("Pipe", 5);

        // Настройки по умолчанию
        MusicVolume = 0.7f;
        SfxVolume = 1.0f;
    }
}
