using UnityEngine;
using System;

[Serializable]
public struct ProductData
{
    public ProductType type;
    public Vector3 position;
    public Quaternion rotation;
    public bool isDefective;
}
