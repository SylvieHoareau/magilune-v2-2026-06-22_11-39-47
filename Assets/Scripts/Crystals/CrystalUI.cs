using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Affiche le nombre de cristaux ramassés dans le HUD.
/// </summary>
public class CrystalUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI crystalText;
    [SerializeField] private Image crystalIcon;

    [Header("Animation (optionnel)")]
    [SerializeField] private float punchScale = 1.25f;
    [SerializeField] private float punchDuration = 0.15f;

    private Vector3 iconBaseScale = Vector3.one;
    private Coroutine punchRoutine;
    private bool isSubscribed;

    private void Awake()
    {
        if (crystalIcon != null)
            iconBaseScale = crystalIcon.transform.localScale;
    }

    private void Start()
    {
        TrySubscribe();
        Refresh();
    }

    private void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (CrystalManager.Instance == null) return;

        CrystalManager.Instance.OnCrystalChanged += UpdateDisplay;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed) return;
        if (CrystalManager.Instance != null)
            CrystalManager.Instance.OnCrystalChanged -= UpdateDisplay;

        isSubscribed = false;
    }

    private void Refresh()
    {
        int current = CrystalManager.Instance != null
            ? CrystalManager.Instance.CurrentCrystals
            : 0;

        UpdateDisplay(current);
    }

    private void UpdateDisplay(int count)
    {
        if (crystalText != null)
            crystalText.text = count.ToString();
        else
            Debug.LogWarning("CrystalUI : crystalText n'est pas assigné dans l'Inspector", this);

        if (crystalIcon != null)
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(PunchIcon());
        }
    }

    private System.Collections.IEnumerator PunchIcon()
    {
        Transform t = crystalIcon.transform;
        float half = punchDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(iconBaseScale, iconBaseScale * punchScale, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(iconBaseScale * punchScale, iconBaseScale, elapsed / half);
            yield return null;
        }

        t.localScale = iconBaseScale;
        punchRoutine = null;
    }
}
