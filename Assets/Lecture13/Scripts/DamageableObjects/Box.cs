using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour, IDamageable
{
    [SerializeField] private float hitPoints = 100f;

    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        string objectName = gameObject.name;
        hitPoints -= damage;
        Debug.Log($"{objectName} took {damage} damage!");
    }
}
