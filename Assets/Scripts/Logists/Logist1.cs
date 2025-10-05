using UnityEngine;
using System.Collections;

public class Logist1 : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float pickUpDelay = 0.5f;

    private TransportTask1 currentTask;
    private bool isWorking = false;
    private GameObject carriedProduct;

    public void AssignTask(TransportTask1 task)
    {
        if (isWorking)
        {
            Debug.LogError("Логист уже выполняет задание!");
            return;
        }

        currentTask = task;
        isWorking = true;
        StartCoroutine(ExecuteTask());
    }

    private IEnumerator ExecuteTask()
    {
        // 1. Идем к точке забора
        yield return StartCoroutine(MoveToPosition(currentTask.FromPosition));

        // 2. Забираем продукт (здесь может быть анимация)
        yield return new WaitForSeconds(pickUpDelay);
        PickUpProduct();

        // 3. Идем к точке доставки
        yield return StartCoroutine(MoveToPosition(currentTask.ToPosition));

        // 4. Сдаем продукт
        yield return new WaitForSeconds(pickUpDelay);
        DeliverProduct();

        // 5. Задание завершено
        CompleteTask();
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
            transform.LookAt(targetPosition);
            yield return null;
        }
    }

    private void PickUpProduct()
    {
        // Здесь мы могли бы найти продукт в точке забора
        // Для простоты создаем визуальный объект
        carriedProduct = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Заглушка
        carriedProduct.transform.SetParent(transform);
        carriedProduct.transform.localPosition = new Vector3(0, 1, 0); // Над головой
        carriedProduct.name = $"Carried: {currentTask.ProductType}";

        Debug.Log($"Логист забрал {currentTask.ProductType}");
    }

    private void DeliverProduct()
    {
        // Передаем продукт целевому объекту
        if (currentTask.TaskGiver is Machine1 machine)
        {
            machine.ReceiveProduct(currentTask.ProductType, carriedProduct);
        }
        else if (currentTask.TaskGiver is ProductSellPoint1 sellPoint)
        {
            sellPoint.ReceiveProduct(currentTask.ProductType, carriedProduct);
        }
        else if (currentTask.TaskGiver is ResourceWarehouse1 warehouse)
        {
            // Для склада, вероятно, просто уничтожаем продукт
            Destroy(carriedProduct);
        }

        carriedProduct = null;

        // Уведомляем того, кто дал задание, что оно завершено
        currentTask.TaskGiver.OnTaskCompleted(currentTask);
    }

    private void CompleteTask()
    {
        isWorking = false;
        currentTask = null;
        LogisticsManager1.Instance.OnTaskCompleted(currentTask, this);
    }
}