using System.Collections.Generic;
using UnityEngine;

public class ProductionManager : MonoBehaviour
{
    public static ProductionManager Instance;

    public List<Machine> AllMachines = new List<Machine>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize() { /* Загрузка данных о машинах из SaveManager */ }

    public void RegisterMachine(Machine machine)
    {
        AllMachines.Add(machine);
    }

    public void UnregisterMachine(Machine machine)
    {
        AllMachines.Remove(machine);
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var machine in AllMachines)
        {
            if (machine.IsBroken) continue;

            // Обновление таймеров производства
            if (machine.IsProducing)
            {
                machine.ProductionTimer -= deltaTime;
                if (machine.ProductionTimer <= 0)
                    OnProductionFinished(machine);
            }

            // Проверка возможности начать производство
            if (!machine.IsProducing && HasInputResources(machine))
                StartProduction(machine);
        }
    }

    private void CompleteProduction(Machine machine)
    {
        // Проверяем брак
        bool isDefective = GameManager.Instance.DifficultyService
            .CheckForDefect(machine.DefectChance);
        
        if (!isDefective)
        {
            // Создаем продукт
            Product newProduct = Instantiate(machine.OutputProductPrefab, 
                machine.OutputSlot.position, Quaternion.identity);
            
            // Сообщаем логистам о новом продукте
            GameManager.Instance.LogisticsManager
                .AddTransportTask(newProduct, machine);
        }
        else
        {
            Debug.Log($"Брак на станке {machine.name}!");
            // Можно создать визуал брака или просто уничтожить ресурс
        }
        
        machine.ResetProduction();
    }

    public bool TryStartProduction(Machine machine)
    {
        if (machine.CanStartProduction())
        {
            machine.StartProduction(GameManager.Instance.Config.BaseProductionTime);
            return true;
        }
        return false;
    }
}
