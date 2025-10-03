public interface ITaskGiver1
{
    void OnTaskCompleted(TransportTask1 task); // Вызывается, когда логист выполнил задание
    Vector3 GetPosition(); // Получить позицию для логиста
}
