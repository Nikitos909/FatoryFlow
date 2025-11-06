using UnityEngine;
using System.Collections;

public class Logist1 : MonoBehaviour
{
    public float speed = 3f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isMoving = false;
    private bool isDelivering = false;
    private Vector3 spawnPosition;
    private bool isWaitingForProduct = false;
    private int pickupAttempts = 0;
    private const int MAX_PICKUP_ATTEMPTS = 3;

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
