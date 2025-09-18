using System.Collections.Generic;
using UnityEngine;

public class LogisticManager : MonoBehaviour
{
    public static LogisticManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
