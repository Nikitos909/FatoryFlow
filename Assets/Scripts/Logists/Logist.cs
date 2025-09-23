using UnityEngine;

public class Logist : MonoBehaviour
{
    public float speed = 2f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private Machine fromMachine;
    private Machine toMachine;
    private bool isMoving = false;
    private bool isDelivering = false;

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                if (!isDelivering) PickUpProduct();
                else DeliverProduct();
            }
        }
    }

    public void AssignTask(Machine from, Machine to)
    {
        fromMachine = from;
        toMachine = to;
        targetPosition = fromMachine.GetOutputSlotPosition();
        isMoving = true;
        isDelivering = false;
    }

    private void PickUpProduct()
    {
        carriedProduct = fromMachine.TakeOutputProduct();
        if (carriedProduct != null)
        {
            carriedProduct.transform.SetParent(transform);
            carriedProduct.transform.localPosition = new Vector3(0, 0.5f, 0);
            targetPosition = toMachine.transform.position;
            isDelivering = true;
            Debug.Log("Логист взял продукт");
        }
        else
        {
            isMoving = false;
            LogisticsManager.Instance.OnTaskCompleted(this);
        }
    }

    private void DeliverProduct()
    {
        if (carriedProduct != null && toMachine.CanAcceptInput(carriedProduct.type))
        {
            toMachine.PutInputProduct(carriedProduct);
            carriedProduct = null;
            Debug.Log("Логист доставил продукт");
        }

        isMoving = false;
        LogisticsManager.Instance.OnTaskCompleted(this);
    }
}