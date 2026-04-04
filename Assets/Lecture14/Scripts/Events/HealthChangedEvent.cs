using UnityEngine;

public struct HealthChangedEvent
{
    public GameObject TargetObject { get; private set; }
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }

    public HealthChangedEvent(GameObject targetObject, float currentHealth, float maxHealth)
    {
        TargetObject = targetObject;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
    }
}
