using UnityEngine;
using System.Collections;

public class Logist1 : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float pickUpDelay = 0.5f;
    [SerializeField] private SpriteRenderer carriedProductSprite; // Спрайт для переносимого продукта

    private TransportTask1 currentTask;
    private bool isWorking = false;
    private ProductType1 carriedProductType;

    public void AssignTask(TransportTask1 task)
    {
        if (isWorking)
        {
            Debug.LogError("Логист уже занят!");
            return;
        }

        currentTask = task;
        isWorking = true;
        StartCoroutine(ExecuteTask());
    }

    private IEnumerator ExecuteTask()
    {
        Debug.Log($"Логист начал задание: {currentTask.ProductType}");

        // Движение к точке забора
        yield return StartCoroutine(MoveToPosition(currentTask.FromPosition));

        // Забор продукта
        yield return new WaitForSeconds(pickUpDelay);
        PickUpProduct();

        // Движение к точке доставки
        yield return StartCoroutine(MoveToPosition(currentTask.ToPosition));

        // Доставка продукта
        yield return new WaitForSeconds(pickUpDelay);
        DeliverProduct();

        // Завершение задания
        CompleteTask();
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        // Для 2D используем только X координату для движения
        Vector2 targetPos2D = new Vector2(targetPosition.x, transform.position.y);
        
        while (Vector2.Distance(transform.position, targetPos2D) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, 
                targetPos2D, 
                movementSpeed * Time.deltaTime
            );
            
            // Поворот в сторону движения
            if (targetPos2D.x > transform.position.x)
                transform.localScale = new Vector3(1, 1, 1); // Смотрит вправо
            else
                transform.localScale = new Vector3(-1, 1, 1); // Смотрит влево
                
            yield return null;
        }
    }

    private void PickUpProduct()
    {
        carriedProductType = currentTask.ProductType;
        
        // Активируем спрайт переносимого продукта
        if (carriedProductSprite != null)
        {
            carriedProductSprite.enabled = true;
            carriedProductSprite.color = GetProductColor(carriedProductType);
        }
        
        Debug.Log($"Логист взял {carriedProductType}");
    }

    private void DeliverProduct()
    {
        // Передаем продукт целевому объекту
        if (currentTask.TaskGiver is Machine1 machine)
        {
            machine.ReceiveProduct(carriedProductType);
        }
        else if (currentTask.TaskGiver is ProductSellPoint1 sellPoint)
        {
            sellPoint.ReceiveProduct(carriedProductType);
        }

        // Скрываем переносимый продукт
        if (carriedProductSprite != null)
        {
            carriedProductSprite.enabled = false;
        }

        // Уведомляем о завершении задания
        currentTask.TaskGiver.OnTaskCompleted(currentTask);
        
        carriedProductType = ProductType.None;
    }

    private Color GetProductColor(ProductType1 product)
    {
        return product switch
        {
            ProductType1.RawPipe => Color.gray,
            ProductType1.CutPipe => Color.blue,
            ProductType1.BentPipe => Color.red,
            _ => Color.white
        };
    }

    private void CompleteTask()
    {
        isWorking = false;
        LogisticsManager1.Instance.OnTaskCompleted(currentTask, this);
        currentTask = null;
        Debug.Log("Логист завершил задание и свободен");
    }
}
