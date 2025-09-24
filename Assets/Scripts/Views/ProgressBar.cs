using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage;

    public void SetProgress(float progress)
    {
        fillImage.fillAmount = Mathf.Clamp01(progress);
    }

    /*
        🎯 КУДА ВЕШАТЬ СКРИПТЫ:
        На сцене создай такую структуру:
        text
        Main Camera
        Canvas
        └── MoneyText (TextMeshPro)
        
        EconomyManager (пустой объект)
        LogisticsManager (пустой объект) 
        GameStarter (пустой объект)
        
        CutterMachine (спрайт)
        ├── Machine (скрипт)
        ├── InputSlot (пустой объект)
        └── OutputSlot (пустой объект)
        
        BenderMachine (спрайт)  
        ├── Machine (скрипт)
        ├── InputSlot (пустой объект)
        └── OutputSlot (пустой объект)
        
        Logist (спрайт)
        └── Logist (скрипт)
        
        ResourceSpawnPoint (пустой объект)
        SellPoint (спрайт с Collider2D)
        └── ProductSellPoint (скрипт)
        В инспекторе GameStarter перетащи:
        •	CutterMachine → Cutter Machine
        •	BenderMachine → Bender Machine
        •	Logist → Logist
        •	ResourceSpawnPoint → Resource Spawn Point
        Теперь все должно работать без ошибок! 🚀
    */
}
