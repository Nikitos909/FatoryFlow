using UnityEngine;

public class ResourceWarehouse : MonoBehaviour, ITaskGiver
{
    [SerializeField] private ProductType1 storedProductType = ProductType.RawPipe;
    [SerializeField] private int pipeCost = 100; // Стоимость одной трубы
    [SerializeField] private Transform spawnPoint; // Точка, где появляется сырье

    public void Initialize() { /* ... */ }

    // Вызывается по кнопке UI
    public void BuyRawMaterial()
    {
        if (EconomyManager1.Instance.SpendMoney(pipeCost))
        {
            // Создаем задание на перемещение "сырья" со склада... но куда?
            // На самом деле, склад просто сообщает, что у него есть продукт.
            // Задание на перемещение должен создать первый свободный станок, которому нужно сырье.
            // Или мы можем создать задание "на складирование", которое будет ждать, пока станок его не заберет.
            // Давайте упростим: склад создает продукт и сразу дает задание логисту отнести его на первый станок.
            // Но это негибко. Лучше использовать систему "поставщик-потребитель".

            // Вместо этого, давайте создадим визуальный объект на складе и сообщим LogisticsManager, что продукт доступен.
            CreateProductVisual();

            // Создаем задание на перемещение от склада к... пока никому.
            // Это задание будет "предложением", которое заберет первый станок, которому нужно сырье.
            // Переработаем LogisticsManager для поддержки двух типов заданий: 
            // 1. Конкретное (от А к Б)
            // 2. Общее (от А -> "кто хочет?")

            // Для простоты на старте, давайте предположим, что мы знаем первый станок.
            // В полноценной системе здесь была бы более сложная логика.
            Machine firstMachine = FindFirstMachineThatNeeds(storedProductType);
            if (firstMachine != null)
            {
                Vector3 fromPos = GetPosition();
                Vector3 toPos = firstMachine.GetPosition();
                TransportTask task = new TransportTask(storedProductType, fromPos, toPos, this);
                LogisticsManager.Instance.AddTask(task);
            }
        }
    }

    private Machine FindFirstMachineThatNeeds(ProductType product)
    {
        // Это упрощенная реализация. В реальной системе должен быть更好的 способ (например, регистрация станков в менеджере)
        Machine[] allMachines = FindObjectsOfType<Machine>();
        foreach (Machine machine in allMachines)
        {
            if (machine.CanAcceptProduct(product) && machine.IsInputSlotFree())
            {
                return machine;
            }
        }
        return null;
    }

    private void CreateProductVisual()
    {
        // Создаем визуальное представление продукта на складе (например, 3D-модель трубы)
        // Этот объект будет уничтожен, когда логист заберет продукт.
    }

    // ITaskGiver implementation
    public void OnTaskCompleted(TransportTask task)
    {
        // Когда логист доставил сырье со склада на станок, мы можем очистить визуал на складе (если он был) или просто ничего не делать.
        Debug.Log($"Сырье доставлено со склада на станок.");
    }

    public Vector3 GetPosition() => spawnPoint.position;
}
