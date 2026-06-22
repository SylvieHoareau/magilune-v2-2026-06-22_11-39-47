using UnityEngine;

public enum DamageType
{
    Poison,
    Lava
}

public class DamageZone : MonoBehaviour
{
    [SerializeField] private int damagePerSecond = 1;
    [SerializeField] private DamageType type; // Poison, Lava

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<Entity>(out var entity))
            entity.TakeDamage(damagePerSecond);
    }
}
