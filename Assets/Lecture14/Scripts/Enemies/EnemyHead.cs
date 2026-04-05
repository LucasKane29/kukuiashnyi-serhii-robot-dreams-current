using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHead : MonoBehaviour, IDamageable
{
    [SerializeField] private Enemy parentEnemy;
    [SerializeField] private float damageMultiplier;
    [SerializeField] private float additionalScore = 0f;
    private ScoreManager _scoreManger;

    void Start()
    {
        _scoreManger = IServiceLocator.Instance.GetService<ScoreManager>();
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 damagerPosition)
    {
        parentEnemy.TakeDamage(damage * damageMultiplier, hitPoint, damagerPosition);
        parentEnemy.IncreaseScoreForDeath(additionalScore);
        _scoreManger.OnHeadshotMade();
    }
}
