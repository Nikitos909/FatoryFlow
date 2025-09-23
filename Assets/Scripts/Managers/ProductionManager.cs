using System.Collections.Generic;
using UnityEngine;

public class ProductionManager : MonoBehaviour
{
    public static ProductionManager Instance;
    public List<Machine> allMachines = new List<Machine>();

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

    // Просто регистрируем станки - вся логика в самих станках
    public void RegisterMachine(Machine machine)
    {
        if (!allMachines.Contains(machine))
        {
            allMachines.Add(machine);
            Debug.Log($"Зарегистрирован станок: {machine.machineType.displayName}");
        }
    }

    public void UnregisterMachine(Machine machine)
    {
        allMachines.Remove(machine);
    }

    // Для MVP можно даже не использовать Update - станки сами работают
}