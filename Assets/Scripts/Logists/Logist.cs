using UnityEngine;

public class Logist : MonoBehaviour
{
    [Header("Настройки")]
    public int salary = 50; // Зарплата в минуту
    public float baseSpeed = 2f; // Базовая скорость перемещения
    public int carryCapacity = 1; // Вместимость (сколько единиц может нести)

    [Header("Текущее состояние")]
    public bool isSick = false;
    public float fatigue = 0f; // Уровень усталости (0-1)
    public TransportTask currentTask = null;
    public Product[] carriedProducts; // Массив продуктов, которые несет

    private Vector3 targetPosition;
    private bool isMoving = false;
    private float sicknessTimer = 0f;

    public void Initialize(float speed)
    {
        baseSpeed = speed;
        carriedProducts = new Product[carryCapacity];
    }

    public void UpdateLogic(float deltaTime)
    {
        if (isSick)
        {
            UpdateSickness(deltaTime);
            return;
        }

        UpdateMovement(deltaTime);
        UpdateFatigue(deltaTime);
    }

    private void UpdateMovement(float deltaTime)
    {
        if (!isMoving) return;

        // Движение к цели
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, baseSpeed * deltaTime);

        // Проверка достижения цели
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            OnReachedDestination();
        }
    }

    private void UpdateFatigue(float deltaTime)
    {
        // Увеличиваем усталость при движении
        if (isMoving)
        {
            fatigue += deltaTime * 0.1f;
            fatigue = Mathf.Clamp01(fatigue);
        }
        else
        {
            // Отдых уменьшает усталость
            fatigue -= deltaTime * 0.05f;
            fatigue = Mathf.Max(0f, fatigue);
        }

        // Влияние усталости на скорость
        float currentSpeed = baseSpeed * (1f - fatigue * 0.3f);
    }

    private void UpdateSickness(float deltaTime)
    {
        sicknessTimer -= deltaTime;
        if (sicknessTimer <= 0f)
        {
            RecoverFromSickness();
        }
    }

    public void AssignTask(TransportTask task)
    {
        if (currentTask != null) return;

        currentTask = task;
        SetDestination(task.fromMachine.GetOutputSlotPosition());
    }

    private void SetDestination(Vector3 destination)
    {
        targetPosition = destination;
        isMoving = true;
    }

    private void OnReachedDestination()
    {
        isMoving = false;

        if (currentTask == null) return;

        // Если пришли за продуктом - забираем его
        if (currentTask.status == TaskStatus.PickingUp)
        {
            PickUpProducts();
            SetDestination(currentTask.toMachine.GetInputSlotPosition());
            currentTask.status = TaskStatus.Delivering;
        }
        // Если пришли доставлять - сдаем продукты
        else if (currentTask.status == TaskStatus.Delivering)
        {
            DeliverProducts();
            currentTask.OnCompleted();
            currentTask = null;
            
            // Сообщаем менеджеру, что свободен
            LogisticsManager.Instance.OnLogistFree(this);
        }
    }

    private void PickUpProducts()
    {
        // Логика подбора продуктов со станка
        for (int i = 0; i < carryCapacity; i++)
        {
            if (carriedProducts[i] == null)
            {
                carriedProducts[i] = currentTask.fromMachine.TakeOutputProduct();
                if (carriedProducts[i] != null)
                {
                    carriedProducts[i].transform.SetParent(transform);
                    carriedProducts[i].transform.localPosition = GetCarryPosition(i);
                }
            }
        }
    }

    private void DeliverProducts()
    {
        // Логика доставки продуктов на станок
        for (int i = 0; i < carryCapacity; i++)
        {
            if (carriedProducts[i] != null)
            {
                currentTask.toMachine.PutInputProduct(carriedProducts[i]);
                carriedProducts[i] = null;
            }
        }
    }

    private Vector3 GetCarryPosition(int index)
    {
        // Позиция для переноски продукта (например, на плечах)
        return new Vector3(0, 0.5f + index * 0.2f, 0);
    }

    public void MakeSick(float duration)
    {
        isSick = true;
        sicknessTimer = duration;
        // Визуальный эффект болезни
    }

    private void RecoverFromSickness()
    {
        isSick = false;
        fatigue = 0f;
        // Убрать визуальный эффект
    }

    public LogistData GetData()
    {
        return new LogistData
        {
            position = transform.position,
            isSick = isSick,
            fatigue = fatigue,
            currentTask = currentTask?.GetData()
        };
    }
}
