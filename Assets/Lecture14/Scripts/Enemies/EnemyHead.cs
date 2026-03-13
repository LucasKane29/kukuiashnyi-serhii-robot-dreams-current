using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHead : MonoBehaviour, IDamageable
{
    [SerializeField] private Enemy parentEnemy;
    [SerializeField] private float damageMultiplier;
    [SerializeField] private float additionalScore = 0f;

    [SerializeField]
    private EventBus eventBus;

    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        parentEnemy.TakeDamage(damage * damageMultiplier, hitPoint);
        eventBus.Publish(new HeadshotMadeEvent(parentEnemy, additionalScore));
    }
}
