using UnityEngine;
using System.Collections;

public class AbilityFeedback : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject jumpIconActive;
    [SerializeField] private GameObject jumpIconDisabled;
    [SerializeField] private GameObject jetpackIcon;
    [SerializeField] private GameObject tutorialPanel; // "Maintiens pour voler"

    [Header("Feedback")]
    [SerializeField] private AudioClip powerUpClip;
    [SerializeField] private ParticleSystem powerUpParticles;
    [SerializeField] private float freezeDuration = 0.5f;

    public void OnJumpLost()
    {
        if (jumpIconActive != null) jumpIconActive.SetActive(false);
        if (jumpIconDisabled != null) jumpIconDisabled.SetActive(true);

        PlayFeedback();
    }

    public void OnJetpackGained()
    {
        if (jetpackIcon != null)
        {
            jetpackIcon.SetActive(true);
            // petite anim scale optionnelle
        }

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        PlayFeedback();
        StartCoroutine(HideTutorialAfterDelay(4f));
    }

    private void PlayFeedback()
    {
        if (powerUpClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(powerUpClip);

        if (powerUpParticles != null)
            powerUpParticles.Play();

        StartCoroutine(ShortFreeze());
    }

    private IEnumerator ShortFreeze()
    {
        Time.timeScale = 0.15f; // quasi pause
        yield return new WaitForSecondsRealtime(freezeDuration);
        Time.timeScale = 1f;
    }

    private IEnumerator HideTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    // À appeler quand le joueur utilise le jetpack la première fois
    public void HideTutorialNow()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
}
