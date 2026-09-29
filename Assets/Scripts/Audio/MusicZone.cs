using UnityEngine;

/// <summary>
/// Zone trigger qui change la musique (Jungle / Transition / Volcan).
/// Même principe que AbilityZone.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MusicZone : MonoBehaviour
{
    [Header("Musique de cette zone")]
    [SerializeField] private AudioClip zoneMusic;

    [Header("Options")]
    [Tooltip("Si true, ne change la musique qu'une seule fois.")]
    [SerializeField] private bool triggerOnce = false;

    [Tooltip("Joue aussi un petit SFX en entrant (optionnel).")]
    [SerializeField] private AudioClip enterSfx;

    private bool used;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        // Change la musique
        if (zoneMusic != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(zoneMusic);

        // Petit feedback sonore optionnel
        if (enterSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(enterSfx);

        if (triggerOnce)
            used = true;
    }
}
