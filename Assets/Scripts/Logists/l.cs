using UnityEngine;
using UnityEngine.AI;

public class Logist : MonoBehaviour
{
    [Header("Logist Settings")]
    public float movementSpeed = 3.5f;
    public float pickUpTime = 1f;
    
    public GameObject CurrentProduct { get; private set; }
    public LogisticsTask CurrentTask { get; private private set; }
    
    private LogisticsManager logisticsManager;
    private NavMeshAgent navMeshAgent;
    private bool isExecutingTask = false;
    
    public void Initialize(LogisticsManager manager)
    {
        logisticsManager = manager;
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = movementSpeed;
        }
    }
    
    public void AssignTask(LogisticsTask task)
    {
        CurrentTask = task;
        isExecutingTask = true;
        StartCoroutine(ExecuteTask());
    }
    
    private System.Collections.IEnumerator ExecuteTask()
    {
        // 1. Подойти к станку-источнику
        yield return MoveToPosition(CurrentTask.sourceMachine.transform.position);
        
        // 2. Забрать продукт
        yield return new WaitForSeconds(pickUpTime);
        PickUpProduct();
        
        // 3. Найти целевой станок
        Machine targetMachine = logisticsManager.FindMachineForProduct(CurrentTask.targetMachineType);
        
        if (targetMachine != null)
        {
            // 4. Подойти к целевому станку
            yield return MoveToPosition(targetMachine.transform.position);
            
            // 5. Доставить продукт (доставка происходит через OnTriggerEnter в MachineController)
            yield return new WaitUntil(() => CurrentProduct == null);
        }
        
        // Задача завершена
        CompleteTask();
    }
    
    private System.Collections.IEnumerator MoveToPosition(Vector3 position)
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(position);
            yield return new WaitUntil(() => 
                !navMeshAgent.pathPending && 
                navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance);
        }
        else
        {
            // Резервное управление без NavMesh
            float distance = Vector3.Distance(transform.position, position);
            float time = distance / movementSpeed;
            Vector3 startPosition = transform.position;
            float elapsedTime = 0;
            
            while (elapsedTime < time)
            {
                transform.position = Vector3.Lerp(startPosition, position, elapsedTime / time);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = position;
        }
    }
    
    private void PickUpProduct()
    {
        if (CurrentTask.product != null)
        {
            CurrentProduct = CurrentTask.product;
            CurrentProduct.transform.SetParent(transform);
            CurrentProduct.transform.localPosition = Vector3.up * 1.5f; // Над головой логиста
        }
    }
    
    public GameObject DeliverProduct()
    {
        GameObject product = CurrentProduct;
        CurrentProduct = null;
        return product;
    }
    
    private void CompleteTask()
    {
        CurrentTask = default;
        isExecutingTask = false;
        logisticsManager.OnTaskCompleted(this);
    }
}
