using UnityEngine;

public class SimpleLogist : MonoBehaviour
{
    public float speed = 2f;
    public Product carriedProduct;
    
    private Vector3 targetPosition;
    private Machine sourceMachine;
    private Machine targetMachine;
    private bool isMoving = false;
    private bool isDelivering = false;

    void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
    }

   public void AssignTask(Machine source, Machine destination)
    {
        if (source == null || destination == null)
        {
            Debug.LogError("Ошибка: source или destination Machine равно null");
            return;
        }
        
        sourceMachine = source;
        targetMachine = destination;
        targetPosition = sourceMachine.GetOutputPosition();
        isMoving = true;
        isDelivering = false;
        
        Debug.Log($"Логист получил задание: {sourceMachine.machineType.displayName} -> {targetMachine.machineType.displayName}");
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (!isDelivering)
            {
                PickUpProduct();
            }
            else
            {
                DeliverProduct();
            }
        }
    }

   private void DeliverProduct()
    {
        if (carriedProduct != null && targetMachine != null && targetMachine.CanAcceptInput(carriedProduct.type))
        {
            targetMachine.PutInputProduct(carriedProduct);
            carriedProduct = null;
            Debug.Log("Логист доставил продукт");
        }
        else
        {
            Debug.Log("Ошибка доставки продукта");
        }
        
        isMoving = false;
        if (SimpleLogisticsManager.Instance != null)
        {
            SimpleLogisticsManager.Instance.OnTaskCompleted(this);
        }
    }

    // Упрощенные методы для совместимости
    public void MakeSick(float duration)
    {
        Debug.Log($"Логист заболел на {duration} секунд");
        // Временное отключение
        speed *= 0.5f;
        Invoke(nameof(RecoverFromSickness), duration);
    }

     private void RecoverFromSickness()
    {
        speed *= 2f;
        Debug.Log("Логист выздоровел");
    }
}
