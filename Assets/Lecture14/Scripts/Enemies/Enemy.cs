using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Zenject;

public abstract class Enemy : MonoBehaviour, IDamageable, IInitializable, IDisposable
{
    [SerializeField] private float hitPoints = 10f;
    [SerializeField] private EnemyHead head;
    [SerializeField] private float scoreForDeath = 1.0f;
    [SerializeField] private HealhbarManager healthbar;
    [SerializeField] private GameObject shotEffect;
    [SerializeField] private float shotEffectDuration = 0.5f;

    private SignalBus signalBus;
    private float currentScoreForDeath;
    private HashSet<VisualEffect> activeEffects = new HashSet<VisualEffect>();

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }

    private float maxHealth;

    void Start()
    {
        maxHealth = hitPoints;
        currentScoreForDeath = scoreForDeath;
    }

    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        string objectName = gameObject.name;
        hitPoints -= damage;
        if (hitPoints < 0)
            hitPoints = 0;
        Debug.Log($"{objectName} took {damage} damage!");
        signalBus.Fire(new HealthChangedSignal(this, hitPoints, maxHealth));
        StartCoroutine(ShowShotEffect(hitPoint));
    }

    void Die()
    {
        foreach(var effect in activeEffects)
            Destroy(effect);

        activeEffects.Clear();

        Debug.Log($"{gameObject.name} died!");
        signalBus.Fire(new EnemyKilledSignal(currentScoreForDeath));
        Destroy(gameObject);
    }

    public abstract void runDieAnimation();
    public abstract void runTakeDamageAnimation();

    public void Initialize()
    {
        signalBus.Subscribe<HeadshotMadeSignal>(OnHeadshotGot);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<HeadshotMadeSignal>(OnHeadshotGot);
    }

    void OnHeadshotGot(HeadshotMadeSignal signal)
    {
        if (this.GetInstanceID() != signal.enemy.GetInstanceID())
            return;

        currentScoreForDeath += signal.additionalScore;
        Debug.Log($"Score for killing {gameObject.name} is now {currentScoreForDeath}");
    }

    private IEnumerator ShowShotEffect(Vector3 hitPoint)
    {
        if (shotEffect != null)
        {
            GameObject effect = Instantiate(shotEffect, hitPoint, shotEffect.transform.rotation);
            VisualEffect vfx = effect.GetComponent<VisualEffect>();
            if (activeEffects.Add(vfx))
            {
                vfx.Play();

                yield return new WaitForSeconds(shotEffectDuration);
                activeEffects.Remove(vfx);
                Destroy(effect);
            }
            if (hitPoints <= 0)
                Die();
        }
    }
}
