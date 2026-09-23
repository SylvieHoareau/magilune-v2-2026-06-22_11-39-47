using UnityEngine;

/// <summary>
/// Base de toute entité avec des points de vie (joueur, ennemis).
/// </summary>
public abstract class Entity : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 3;
    protected int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;

    protected virtual void Awake() => currentHealth = maxHealth;

    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth <= 0) Die();
    }

    public virtual void Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    protected virtual void Die() => Destroy(gameObject);
}
