using UnityEngine;

/// <summary>
/// Sauvegarde le point de respawn du joueur (lave / mort).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private AudioClip activateClip;
    private bool activated;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        activated = true;
        health.SetCheckpoint(transform);

        if (activateClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(activateClip);
    }
}
