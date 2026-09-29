using UnityEngine;

public class CrystalPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private ParticleSystem pickupParticles;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        if (CrystalManager.Instance != null)
            CrystalManager.Instance.AddCrystal(value);
        else
            Debug.LogWarning("CrystalManager manquant dans la scène");

        if (pickupClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(pickupClip);

        if (pickupParticles != null)
            Instantiate(pickupParticles, transform.position, Quaternion.identity);

        // Désactive le collider tout de suite pour éviter double trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject);
    }
}
