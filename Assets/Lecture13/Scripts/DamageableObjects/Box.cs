using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour, IDamageable
{
    [SerializeField] private float hitPoints = 100f;

    private Rigidbody rb;
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public void TakeDamage(float damage)
    {
        string objectName = gameObject.name;
        hitPoints -= damage;
        Debug.Log($"{objectName} took {damage} damage!");
    }
}
