using UnityEngine;

/// <summary>
/// Zone trigger du niveau pour le rythme des consignes :
/// partie 1 = saut, milieu = perte du saut, partie 2 = jetpack.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AbilityZone : MonoBehaviour
{
    [Header("Au contact du joueur")]
    [SerializeField] private bool disableJump;
    [SerializeField] private bool enableJump;
    [SerializeField] private bool enableJetpack;
    [SerializeField] private bool disableJetpack;

    [Header("Usage")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private AudioClip feedbackClip;

    private bool used;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;

        PlayerController controller = other.GetComponent<PlayerController>();
        PlayerJetpack jetpack = other.GetComponent<PlayerJetpack>();
        if (controller == null) return;

        if (disableJump) controller.SetJumpAbility(false);
        if (enableJump) controller.SetJumpAbility(true);
        if (enableJetpack) controller.SetJetpackAbility(true);
        if (disableJetpack) controller.SetJetpackAbility(false);

        if (jetpack != null)
        {
            if (enableJetpack) jetpack.SetUnlocked(true);
            if (disableJetpack) jetpack.SetUnlocked(false);
        }

        if (feedbackClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(feedbackClip);

        if (triggerOnce)
        {
            used = true;
            gameObject.SetActive(false);
        }
    }
}
