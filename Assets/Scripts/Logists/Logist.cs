using UnityEngine;

public class Logist : MonoBehaviour
{
    public float speed = 2f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isMoving = false;
    private bool isDelivering = false;

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                if (!isDelivering) 
                    PickUpProduct();
                else 
                    DeliverProduct();
            }
        }
    }

       public void AssignTask(TransportTask task)
    {
        currentTask = task;
        targetPosition = task.sourceMachine.GetOutputSlotPosition();
        isMoving = true;
        isDelivering = false;
        
        Debug.Log($"🚚 Логист получил задачу: {task.productType}");
    }

   private void PickUpProduct()
    {
        carriedProduct = currentTask.sourceMachine.TakeOutputProduct();
        if (carriedProduct != null)
        {
            carriedProduct.transform.SetParent(transform);
            carriedProduct.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            // Определяем куда двигаться дальше
            if (currentTask.destinationMachine != null)
            {
                // Обычная задача: на другой станок
                targetPosition = currentTask.destinationMachine.transform.position;
            }
            else
            {
                // Задача на продажу: на точку продажи
                targetPosition = LogisticsManager.Instance.sellPoint.transform.position;
            }
            
            isDelivering = true;
            Debug.Log($"📥 Логист взял продукт: {carriedProduct.type}");
        }
        else
        {
            // Продукт уже забрали - отменяем задачу
            Debug.LogWarning("❌ Продукт исчез, отмена задачи");
            isMoving = false;
            LogisticsManager.Instance.OnTaskCompleted(this);
        }
    }

    private void DeliverProduct()
    {
        if (carriedProduct != null)
        {
            if (currentTask.destinationMachine != null)
            {
                // Доставляем на станок
                if (currentTask.destinationMachine.CanAcceptInput(carriedProduct.type))
                {
                    currentTask.destinationMachine.PutInputProduct(carriedProduct);
                    carriedProduct = null;
                    Debug.Log($"📤 Логист доставил продукт на станок");
                }
                else
                {
                    Debug.LogWarning("❌ Станок-приемник не может принять продукт");
                    // Можно добавить логику повторной попытки или создания новой задачи
                }
            }
            else
            {
                // Доставляем на склад (продажа)
                // Product будет уничтожен в ProductSellPoint.OnTriggerEnter2D
                carriedProduct.transform.SetParent(null);
                // Позиционируем продукт рядом с точкой продажи
                carriedProduct.transform.position = targetPosition;
                carriedProduct = null;
                Debug.Log($"💰 Логист доставил продукт на склад");
            }
        }

        isMoving = false;
        currentTask = null;
        LogisticsManager.Instance.OnTaskCompleted(this);
    }

    // Визуализация для отладки
    private void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = isDelivering ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
