using System;
using System.Collections.Generic;

// Ýòîò àòðèáóò äåëàåò îáúåêò ñåðèàëèçóåìûì â JSON
[Serializable]
public class GameData
{
    public int CurrentMoney;
    public int CurrentLevel;
    public int CompletedOrders;
    public float TotalPlayTime;
    
    public GameData()
    {
        CurrentMoney = 1000;
        CurrentLevel = 1;
        CompletedOrders = 0;
        TotalPlayTime = 0;
    }

//================================
/*    
    // 1. Ýêîíîìèêà
    public int Money;
    public int TotalRevenue;
    public int TotalExpenses;

    // 2. Ïðîãðåññ è ðåñóðñû
    public int CurrentLevel;
    public Dictionary<string, int> ResourceInventory; // Íàïðèìåð, "Pipe": 10, "BentSector": 5

    // 3. Ñîñòîÿíèå ôàáðèêè
    public List<MachineSaveData> Machines;
    public List<LogistSaveData> Logists;

    // 4. Ñòàòèñòèêà è àíàëèòèêà
    public int ProductsProduced;
    public int DefectsProduced;
    public float TotalPlayTime;

    // 5. Íàñòðîéêè èãðû (åñëè íóæíî)
    public float MusicVolume;
    public float SfxVolume;

    // Êîíñòðóêòîð äëÿ óñòàíîâêè çíà÷åíèé ïî óìîë÷àíèþ
    // (èñïîëüçóåòñÿ ïðè íà÷àëå íîâîé èãðû)
    public GameData()
    {
        Money = 1000; // Ñòàðòîâûé êàïèòàë
        CurrentLevel = 1;
        ResourceInventory = new Dictionary<string, int>();
        Machines = new List<MachineSaveData>();
        Logists = new List<LogistSaveData>();

        // Äîáàâëÿåì ñòàðòîâûå ðåñóðñû, åñëè íóæíî
        ResourceInventory.Add("Pipe", 5);

        // Íàñòðîéêè ïî óìîë÷àíèþ
        MusicVolume = 0.7f;
        SfxVolume = 1.0f;
    }*/
}
