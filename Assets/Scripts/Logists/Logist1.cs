using UnityEngine;
using System.Collections;

public class Logist1 : MonoBehaviour
{
    public float speed = 3f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isEmployed = false;
    private Vector3 spawnPosition;

    void Start()
    {
        spawnPosition = transform.position;

        // Регистрируем в LogisticsManager
        if (LogisticsManager.Instance != null)
        {
            LogisticsManager.Instance.OnLogistAvailable(this);
        }
    }

}
