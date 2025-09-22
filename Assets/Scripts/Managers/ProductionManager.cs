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
            if (machine.isBroken) continue;

            // Обновление таймеров производства
            if (machine.isProducing)
            {
                machine.workTimer -= deltaTime;
                if (machine.workTimer <= 0)
                    OnProductionFinished(machine);
            }

            // Проверка возможности начать производство
            if (!machine.IsProducing && HasInputResources(machine))
                StartProduction(machine);
        }
    }

    private bool HasInputResources(Machine machine)
    {
        // Проверяет, есть ли на входных слотах станка нужные ресурсы
        foreach (var inputSlot in machine.inputSlot)
            if (inputSlot.CurrentProduct == null) return false;
        return true;
    }

    private void StartProduction(Machine machine)
    {
        machine.IsProducing = true;
        machine.workTimer = machine.Data.ProductionTime;
        
        // Убираем ресурсы со входов
        foreach (var inputSlot in machine.inputSlot)
            Destroy(inputSlot.CurrentProduct.gameObject);
    }

    private void OnProductionFinished(Machine machine)
    {
        // Проверка на брак через DifficultyService
        bool isDefective = DifficultyService.Instance.CheckForDefect(machine);
        
        if (isDefective)
        {
            // Создать бракованный продукт
            CreateProduct(machine.outputSlot, machine.Data.DefectiveProductType);
        }
        else
        {
            // Создать нормальный продукт
            CreateProduct(machine.outputSlot, machine.Data.OutputProductType);
        }

        machine.IsProducing = false;
        LogisticsManager.Instance.OnProductsReady(machine.outputSlot);
    }
}
