using UnityEngine;
using System.Collections;

public class LogisticsDebugVisualizer : MonoBehaviour
{
    void OnDrawGizmos()
    {
        if (LogisticsManager.Instance == null) return;
        
        // Рисуем связи между станками
        Machine[] machines = FindObjectsOfType<Machine>();
        
        foreach (Machine machine in machines)
        {
            if (machine.defaultNextMachine != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(machine.transform.position, machine.defaultNextMachine.transform.position);
                
                // Стрелка
                Vector3 dir = (machine.defaultNextMachine.transform.position - machine.transform.position).normalized;
                Vector3 arrowEnd = machine.transform.position + dir * 2f;
                Gizmos.DrawLine(arrowEnd, arrowEnd - Quaternion.Euler(0, 30, 0) * dir * 0.5f);
                Gizmos.DrawLine(arrowEnd, arrowEnd - Quaternion.Euler(0, -30, 0) * dir * 0.5f);
            }
        }
        
        // Рисуем задачи в очереди
        var field = typeof(LogisticsManager).GetField("taskQueue", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            var queue = (Queue<TransportTask>)field.GetValue(LogisticsManager.Instance);
            int index = 0;
            
            foreach (TransportTask task in queue)
            {
                if (task.sourceMachine != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(task.sourceMachine.transform.position + Vector3.up * 3f, 0.3f + index * 0.1f);
                    
                    if (task.destMachine != null)
                    {
                        Gizmos.DrawLine(
                            task.sourceMachine.transform.position + Vector3.up * 3f,
                            task.destMachine.transform.position + Vector3.up * 3f
                        );
                    }
                }
                index++;
            }
        }
    }
}
