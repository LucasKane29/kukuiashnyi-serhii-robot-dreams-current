using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class HealhbarManager : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField]
    private Enemy enemy;

    [SerializeField]
    private RectTransform healthbarLevel;

    [SerializeField]
    private float animationSpeed = 0.3f;

    [SerializeField]
    private Camera playerCamera;

    private SignalBus signalBus;

    private float maxHealthBarWidth;

    private Coroutine currentAnimation;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }

    public void Initialize()
    {
        maxHealthBarWidth = healthbarLevel.sizeDelta.x;
        signalBus.Subscribe<HealthChangedSignal>(OnHealthChanged);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<HealthChangedSignal>(OnHealthChanged);
    }

    void OnHealthChanged(HealthChangedSignal signal)
    {
        if (signal.Enemy.GetInstanceID() != this.enemy.GetInstanceID())
            return;

        UpdateBar(signal.CurrentHealth, signal.MaxHealth);
    }

    private void UpdateBar(float current, float max)
    {
        Debug.Log($"Health changed: {current} / {max}");

        float healthPercentage = current / max;

        if(currentAnimation != null) {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(AnimateHealthBar(maxHealthBarWidth * healthPercentage));
    }

    private IEnumerator AnimateHealthBar(float targetWidth)
    {
        float initialWidth = healthbarLevel.sizeDelta.x;
        float elapsedTime = 0f;
        while (elapsedTime < animationSpeed)
        {
            elapsedTime += Time.deltaTime;
            float newWidth = Mathf.Lerp(initialWidth, targetWidth, elapsedTime / animationSpeed);
            healthbarLevel.sizeDelta = new Vector2(newWidth, healthbarLevel.sizeDelta.y);
            yield return null;
        }
        healthbarLevel.sizeDelta = new Vector2(targetWidth, healthbarLevel.sizeDelta.y);
    }
}
