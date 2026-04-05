using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    private UIManager _uiManager;
    private GameManager _gameManager;
    private float currentHealth;


    void Start()
    {
        currentHealth = maxHealth;
        _uiManager = IServiceLocator.Instance.GetService<UIManager>();
        _gameManager = IServiceLocator.Instance.GetService<GameManager>();
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 damagerPosition)
    {
        string objectName = gameObject.name;
        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;
        Debug.Log($"{objectName} took {damage} damage!");
        _uiManager.UpdatePlayerHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log($"{objectName} has died!");
            _gameManager.OnPlayerDeath();
        }
    }
}
