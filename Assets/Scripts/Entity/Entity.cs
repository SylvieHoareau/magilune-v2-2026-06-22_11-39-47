using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 3;
    protected int currentHealth;

    protected virtual void Awake() => currentHealth = maxHealth;

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die() => Destroy(gameObject);
}