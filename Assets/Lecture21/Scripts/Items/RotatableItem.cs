using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatableItem : MonoBehaviour
{
    [SerializeField]
    private Vector3 _rotationSpeed = new Vector3(0f, 30f, 0f);
    void Update()
    {
        transform.Rotate(_rotationSpeed * Time.deltaTime);
    }
}
