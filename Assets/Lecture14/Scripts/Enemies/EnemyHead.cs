using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemyHead : MonoBehaviour, IDamageable
{
    [SerializeField] private Enemy parentEnemy;
    [SerializeField] private float damageMultiplier;
    [SerializeField] private float additionalScore = 0f;

    private SignalBus signalBus;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }

    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        parentEnemy.TakeDamage(damage * damageMultiplier, hitPoint);
        signalBus.Fire(new HeadshotMadeSignal(parentEnemy, additionalScore));
    }
}
