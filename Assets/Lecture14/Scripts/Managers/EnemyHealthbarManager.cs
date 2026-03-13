using System;
using System.Collections;
using UnityEngine;

public class HealhbarManager : MonoBehaviour
{
    [SerializeField]
    private Enemy enemy;

    [SerializeField]
    private RectTransform healthbarLevel;

    [SerializeField]
    private float animationSpeed = 0.3f;

    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private EventBus eventBus;

    private float maxHealthBarWidth;

    private Coroutine currentAnimation;

    public void Awake()
    {
        maxHealthBarWidth = healthbarLevel.sizeDelta.x;
    }

    public void OnEnable()
    {
        eventBus.Subscribe<HealthChangedEvent>(OnHealthChanged);
    }
    public void OnDisable()
    {
        eventBus.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
    }

    void OnHealthChanged(HealthChangedEvent subscribedEvent)
    {
        if (subscribedEvent.Enemy.GetInstanceID() != this.enemy.GetInstanceID())
            return;

        UpdateBar(subscribedEvent.CurrentHealth, subscribedEvent.MaxHealth);
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
