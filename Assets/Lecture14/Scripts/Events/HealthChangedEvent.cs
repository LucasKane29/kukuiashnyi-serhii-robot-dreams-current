public struct HealthChangedEvent
{
    public Enemy Enemy { get; private set; }
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }

    public HealthChangedEvent(Enemy enemy, float currentHealth, float maxHealth)
    {
        Enemy = enemy;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
    }
}
