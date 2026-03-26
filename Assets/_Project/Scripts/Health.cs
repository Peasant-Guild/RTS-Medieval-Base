using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    private const float MinMaxHealth = 1f;
    private const float AliveThreshold = 0.001f;

    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private float _currentHealth;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsAlive => _currentHealth > AliveThreshold;

    public event Action<Health> Died;

    private void Awake()
    {
        ClampHealthValues();
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive || damage <= 0f)
        {
            return;
        }

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);

        if (_currentHealth <= AliveThreshold)
        {
            _currentHealth = 0f;
        }

        if (!IsAlive)
        {
            Die();
        }
    }

    public void ConfigureMaxHealth(float maxHealth, bool resetCurrentHealth)
    {
        _maxHealth = Mathf.Max(MinMaxHealth, maxHealth);

        if (resetCurrentHealth)
        {
            _currentHealth = _maxHealth;
            return;
        }

        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);
    }

    private void ClampHealthValues()
    {
        _maxHealth = Mathf.Max(MinMaxHealth, _maxHealth);

        if (_currentHealth <= AliveThreshold || _currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    private void Die()
    {
        Died?.Invoke(this);
        Destroy(gameObject);
    }
}
