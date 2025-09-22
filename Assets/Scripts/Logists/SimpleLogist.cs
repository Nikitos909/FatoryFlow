using UnityEngine;

public class SimpleLogist : MonoBehaviour
{
    public float speed = 2f;
    public Product carriedProduct;
    
    private Vector3 targetPosition;
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

    public void AssignTask(Machine sourceMachine, Machine destinationMachine)
    {
        targetMachine = destinationMachine;
        targetPosition = sourceMachine.GetOutputPosition();
        isMoving = true;
        isDelivering = false;
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

    private void PickUpProduct()
    {
        carriedProduct = targetMachine.TakeOutputProduct();
        if (carriedProduct != null)
        {
            carriedProduct.transform.SetParent(transform);
            carriedProduct.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            targetPosition = targetMachine.GetOutputPosition();
            isDelivering = true;
        }
        else
        {
            isMoving = false;
        }
    }

    private void DeliverProduct()
    {
        if (carriedProduct != null && targetMachine.CanAcceptInput(carriedProduct.type))
        {
            targetMachine.SetInputProduct(carriedProduct);
            carriedProduct = null;
        }
        isMoving = false;
        LogisticsManager.Instance.OnTaskCompleted(this);
    }
}
