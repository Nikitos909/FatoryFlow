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
            MoveToTarget();
        }
    }

    public void AssignTask(TransportTask task)
    {
        currentTask = task;
        targetPosition = task.fromMachine.GetOutputSlotPosition();
        isMoving = true;
        isDelivering = false;
        Debug.Log("Логист получил задание");
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, targetPosition, speed * Time.deltaTime);

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

    private void PickUpProduct()
    {
        carriedProduct = currentTask.fromMachine.TakeOutputProduct();
        if (carriedProduct != null)
        {
            carriedProduct.transform.SetParent(transform);
            carriedProduct.transform.localPosition = new Vector3(0, 0.5f, 0);

            targetPosition = currentTask.toMachine.GetInputSlotPosition();
            isDelivering = true;
            Debug.Log("Логист забрал продукт");
        }
        else
        {
            isMoving = false;
            LogisticsManager.Instance.OnTaskCompleted(this);
        }
    }

    private void DeliverProduct()
    {
        if (carriedProduct != null &&
            currentTask.toMachine.CanAcceptInput(carriedProduct.type))
        {
            currentTask.toMachine.PutInputProduct(carriedProduct);
            carriedProduct = null;
            Debug.Log("Логист доставил продукт");
        }

        isMoving = false;
        currentTask = null;

        if (LogisticsManager.Instance != null)
        {
            LogisticsManager.Instance.OnTaskCompleted(this);
        }
    }
}