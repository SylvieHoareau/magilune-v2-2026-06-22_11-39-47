using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // ou UnityEngine.UI.Text si tu n’utilises pas TextMeshPro

public class PauseHint : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject hintPanel;   // Panel avec texte + icône
    [SerializeField] private float showDuration = 5f;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Comportement")]
    [SerializeField] private bool showOnlyOnce = true;
    [SerializeField] private string playerPrefsKey = "Magilune_PauseHintShown";

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (hintPanel != null)
        {
            canvasGroup = hintPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = hintPanel.AddComponent<CanvasGroup>();

            hintPanel.SetActive(false);
        }
    }

    private void Start()
    {
        if (showOnlyOnce && PlayerPrefs.GetInt(playerPrefsKey, 0) == 1)
            return;

        StartCoroutine(ShowHintRoutine());
    }

    private IEnumerator ShowHintRoutine()
    {
        // Petit délai pour laisser le joueur commencer à bouger
        yield return new WaitForSeconds(1.5f);

        if (hintPanel == null) yield break;

        hintPanel.SetActive(true);
        canvasGroup.alpha = 0f;

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // fonctionne même si le jeu est déjà en pause
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(showDuration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        hintPanel.SetActive(false);

        if (showOnlyOnce)
            PlayerPrefs.SetInt(playerPrefsKey, 1);
    }

    /// <summary>
    /// Pour re-afficher le hint (ex. depuis un menu Options « Réafficher les aides »).
    /// </summary>
    public void ResetHint()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
    }
}
