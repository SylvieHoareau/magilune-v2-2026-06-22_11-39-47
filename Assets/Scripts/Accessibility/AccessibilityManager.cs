using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Point central d'accessibilité pour Magilune.
/// Place ce script sur un GameObject "AccessibilityManager" dans la première scène
/// (ou crée-le automatiquement). Les autres scripts (PlayerController, PlayerJetpack,
/// PlayerHealth, etc.) lisent les valeurs via AccessibilityManager.Instance.
///
/// Toutes les options sont optionnelles et peuvent être changées en runtime
/// (menu pause / menu démarrage).
/// </summary>
public class AccessibilityManager : MonoBehaviour
{
    public static AccessibilityManager Instance { get; private set; }

    // ─────────────────────────────────────────────
    //  PARAMÈTRES EXPOSÉS (modifiables dans l'Inspector + menu)
    // ─────────────────────────────────────────────

    [Header("=== MOUVEMENT & SAUT ===")]
    [Tooltip("Multiplicateur de vitesse de déplacement (1 = normal)")]
    [Range(0.5f, 2f)]
    public float moveSpeedMultiplier = 1f;

    [Tooltip("Multiplicateur de force de saut (1 = normal)")]
    [Range(0.7f, 1.8f)]
    public float jumpForceMultiplier = 1f;

    [Tooltip("Coyote time en secondes (temps de grâce après avoir quitté le sol)")]
    [Range(0.08f, 0.45f)]
    public float coyoteTime = 0.12f;

    [Tooltip("Jump buffer en secondes (fenêtre pour appuyer avant d'atterrir)")]
    [Range(0.08f, 0.45f)]
    public float jumpBufferTime = 0.12f;

    [Tooltip("Saut automatique quand on approche du bord d'une plateforme")]
    public bool autoJump = false;

    [Tooltip("Maintenir le bouton = saut continu (au lieu d'un appui)")]
    public bool holdToJump = false;

    [Header("=== TIR ===")]
    [Tooltip("Aide à la visée : le tir se dirige vers l'ennemi le plus proche")]
    public bool aimAssist = false;

    [Tooltip("Rayon de détection de l'aim assist")]
    public float aimAssistRadius = 6f;

    [Header("=== JETPACK ===")]
    [Tooltip("Multiplicateur de force du jetpack")]
    [Range(0.8f, 1.6f)]
    public float jetpackThrustMultiplier = 1f;

    [Tooltip("Multiplicateur de carburant max")]
    [Range(1f, 2.5f)]
    public float jetpackFuelMultiplier = 1f;

    [Tooltip("Régénération de carburant plus rapide")]
    [Range(1f, 2.5f)]
    public float jetpackRegenMultiplier = 1f;

    [Header("=== MODE EXPLORATION / DIFFICULTÉ ===")]
    [Tooltip("Mode Exploration : dégâts réduits + ennemis plus tolérants")]
    public bool explorationMode = false;

    [Tooltip("Multiplicateur de dégâts reçus (0.5 = moitié des dégâts)")]
    [Range(0.25f, 1f)]
    public float damageTakenMultiplier = 1f;

    [Tooltip("Durée d'invulnérabilité après un coup (secondes)")]
    [Range(0.8f, 3f)]
    public float iFrameDuration = 1.1f;

    [Header("=== TEMPS & RYTHME ===")]
    [Tooltip("Ralentit globalement le jeu (0.75 = 75 % de la vitesse)")]
    [Range(0.5f, 1f)]
    public float timeScale = 1f;

    [Header("=== FEEDBACKS ===")]
    [Tooltip("Vibrations manette plus fortes")]
    public bool strongHaptics = true;

    [Tooltip("Feedbacks visuels renforcés (flashs, particules, outlines)")]
    public bool enhancedVisualFeedback = true;

    [Tooltip("Sons d'aide (indices sonores directionnels, etc.)")]
    public bool helpfulAudioCues = true;

    [Header("=== VISUEL ===")]
    [Tooltip("Mode contraste élevé + contours")]
    public bool highContrast = false;

    [Tooltip("Éléments UI et sprites un peu plus grands")]
    public bool largeElements = false;

    // ─────────────────────────────────────────────
    //  ÉVÉNEMENTS (les autres scripts peuvent s'abonner)
    // ─────────────────────────────────────────────

    public event System.Action OnSettingsChanged;

    // ─────────────────────────────────────────────
    //  CYCLE DE VIE
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyTimeScale();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ─────────────────────────────────────────────
    //  API PUBLIQUE – appelée par les autres scripts
    // ─────────────────────────────────────────────

    /// <summary>Vitesse de déplacement effective (déjà multipliée).</summary>
    public float GetMoveSpeed(float baseSpeed) => baseSpeed * moveSpeedMultiplier;

    /// <summary>Force de saut effective.</summary>
    public float GetJumpForce(float baseForce) => baseForce * jumpForceMultiplier;

    /// <summary>Coyote time effectif.</summary>
    public float GetCoyoteTime() => coyoteTime;

    /// <summary>Jump buffer effectif.</summary>
    public float GetJumpBufferTime() => jumpBufferTime;

    /// <summary>Force de poussée jetpack effective.</summary>
    public float GetJetpackThrust(float baseThrust) => baseThrust * jetpackThrustMultiplier;

    /// <summary>Carburant max jetpack effectif.</summary>
    public float GetJetpackMaxFuel(float baseFuel) => baseFuel * jetpackFuelMultiplier;

    /// <summary>Régénération jetpack effective.</summary>
    public float GetJetpackRegen(float baseRegen) => baseRegen * jetpackRegenMultiplier;

    /// <summary>Dégâts reçus après application du multiplicateur.</summary>
    public int GetDamageTaken(int rawDamage)
    {
        if (explorationMode)
            return Mathf.Max(1, Mathf.RoundToInt(rawDamage * damageTakenMultiplier * 0.5f));

        return Mathf.Max(1, Mathf.RoundToInt(rawDamage * damageTakenMultiplier));
    }

    /// <summary>Durée d'i-frames effective.</summary>
    public float GetIFrameDuration() => iFrameDuration;

    /// <summary>Déclenche une vibration manette (si strongHaptics est activé).</summary>
    public void PlayHaptic(float lowFrequency = 0.4f, float highFrequency = 0.6f, float duration = 0.15f)
    {
        if (!strongHaptics) return;

        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        // Simple one-shot (pour une vraie courbe, utiliser une coroutine)
        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        CancelInvoke(nameof(StopHaptics));
        Invoke(nameof(StopHaptics), duration);
    }

    private void StopHaptics()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
            gamepad.SetMotorSpeeds(0f, 0f);
    }

    // ─────────────────────────────────────────────
    //  CHANGEMENT DES OPTIONS (appelé par le menu UI)
    // ─────────────────────────────────────────────

    public void SetMoveSpeedMultiplier(float value)
    {
        moveSpeedMultiplier = Mathf.Clamp(value, 0.5f, 2f);
        NotifyChanged();
    }

    public void SetJumpForceMultiplier(float value)
    {
        jumpForceMultiplier = Mathf.Clamp(value, 0.7f, 1.8f);
        NotifyChanged();
    }

    public void SetCoyoteTime(float value)
    {
        coyoteTime = Mathf.Clamp(value, 0.08f, 0.45f);
        NotifyChanged();
    }

    public void SetJumpBufferTime(float value)
    {
        jumpBufferTime = Mathf.Clamp(value, 0.08f, 0.45f);
        NotifyChanged();
    }

    public void SetAutoJump(bool value)
    {
        autoJump = value;
        NotifyChanged();
    }

    public void SetHoldToJump(bool value)
    {
        holdToJump = value;
        NotifyChanged();
    }

    public void SetAimAssist(bool value)
    {
        aimAssist = value;
        NotifyChanged();
    }

    public void SetExplorationMode(bool value)
    {
        explorationMode = value;
        // En mode exploration on force aussi un multiplicateur de dégâts plus bas
        if (value && damageTakenMultiplier > 0.6f)
            damageTakenMultiplier = 0.5f;
        NotifyChanged();
    }

    public void SetDamageTakenMultiplier(float value)
    {
        damageTakenMultiplier = Mathf.Clamp(value, 0.25f, 1f);
        NotifyChanged();
    }

    public void SetTimeScale(float value)
    {
        timeScale = Mathf.Clamp(value, 0.5f, 1f);
        ApplyTimeScale();
        NotifyChanged();
    }

    public void SetHighContrast(bool value)
    {
        highContrast = value;
        NotifyChanged();
    }

    public void SetStrongHaptics(bool value)
    {
        strongHaptics = value;
        NotifyChanged();
    }

    public void SetEnhancedVisualFeedback(bool value)
    {
        enhancedVisualFeedback = value;
        NotifyChanged();
    }

    /// <summary>
    /// Applique un profil prédéfini (utile pour le menu).
    /// </summary>
    public void ApplyPreset(AccessibilityPreset preset)
    {
        switch (preset)
        {
            case AccessibilityPreset.Default:
                ResetToDefaults();
                break;

            case AccessibilityPreset.MotorAssistance:
                moveSpeedMultiplier = 0.85f;
                jumpForceMultiplier = 1.15f;
                coyoteTime = 0.28f;
                jumpBufferTime = 0.28f;
                autoJump = true;
                holdToJump = true;
                aimAssist = true;
                jetpackThrustMultiplier = 1.2f;
                jetpackFuelMultiplier = 1.6f;
                jetpackRegenMultiplier = 1.5f;
                explorationMode = true;
                damageTakenMultiplier = 0.5f;
                iFrameDuration = 1.8f;
                timeScale = 0.85f;
                strongHaptics = true;
                enhancedVisualFeedback = true;
                break;

            case AccessibilityPreset.Exploration:
                explorationMode = true;
                damageTakenMultiplier = 0.4f;
                iFrameDuration = 2.2f;
                timeScale = 0.9f;
                enhancedVisualFeedback = true;
                helpfulAudioCues = true;
                break;

            case AccessibilityPreset.VisualAssistance:
                highContrast = true;
                largeElements = true;
                enhancedVisualFeedback = true;
                helpfulAudioCues = true;
                break;
        }

        ApplyTimeScale();
        NotifyChanged();
        SaveSettings();
    }

    public void ResetToDefaults()
    {
        moveSpeedMultiplier = 1f;
        jumpForceMultiplier = 1f;
        coyoteTime = 0.12f;
        jumpBufferTime = 0.12f;
        autoJump = false;
        holdToJump = false;
        aimAssist = false;
        aimAssistRadius = 6f;
        jetpackThrustMultiplier = 1f;
        jetpackFuelMultiplier = 1f;
        jetpackRegenMultiplier = 1f;
        explorationMode = false;
        damageTakenMultiplier = 1f;
        iFrameDuration = 1.1f;
        timeScale = 1f;
        strongHaptics = true;
        enhancedVisualFeedback = true;
        helpfulAudioCues = true;
        highContrast = false;
        largeElements = false;

        ApplyTimeScale();
        NotifyChanged();
        SaveSettings();
    }

    // ─────────────────────────────────────────────
    //  SAUVEGARDE / CHARGEMENT (PlayerPrefs simple)
    // ─────────────────────────────────────────────

    private const string PREFS_PREFIX = "Magilune_Access_";

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(PREFS_PREFIX + "MoveSpeed", moveSpeedMultiplier);
        PlayerPrefs.SetFloat(PREFS_PREFIX + "JumpForce", jumpForceMultiplier);
        PlayerPrefs.SetFloat(PREFS_PREFIX + "Coyote", coyoteTime);
        PlayerPrefs.SetFloat(PREFS_PREFIX + "JumpBuffer", jumpBufferTime);
        PlayerPrefs.SetInt(PREFS_PREFIX + "AutoJump", autoJump ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_PREFIX + "HoldToJump", holdToJump ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_PREFIX + "AimAssist", aimAssist ? 1 : 0);
        PlayerPrefs.SetFloat(PREFS_PREFIX + "JetpackThrust", jetpackThrustMultiplier);
        PlayerPrefs.SetFloat(PREFS_PREFIX + "JetpackFuel", jetpackFuelMultiplier);
        PlayerPrefs.SetFloat(PREFS_PREFIX + "JetpackRegen", jetpackRegenMultiplier);
        PlayerPrefs.SetInt(PREFS_PREFIX + "Exploration", explorationMode ? 1 : 0);
        PlayerPrefs.SetFloat(PREFS_PREFIX + "DamageMult", damageTakenMultiplier);
        PlayerPrefs.SetFloat(PREFS_PREFIX + "IFrames", iFrameDuration);
        PlayerPrefs.SetFloat(PREFS_PREFIX + "TimeScale", timeScale);
        PlayerPrefs.SetInt(PREFS_PREFIX + "StrongHaptics", strongHaptics ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_PREFIX + "EnhancedVisual", enhancedVisualFeedback ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_PREFIX + "HighContrast", highContrast ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_PREFIX + "LargeElements", largeElements ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        if (!PlayerPrefs.HasKey(PREFS_PREFIX + "MoveSpeed")) return; // première fois → valeurs par défaut

        moveSpeedMultiplier = PlayerPrefs.GetFloat(PREFS_PREFIX + "MoveSpeed", 1f);
        jumpForceMultiplier = PlayerPrefs.GetFloat(PREFS_PREFIX + "JumpForce", 1f);
        coyoteTime = PlayerPrefs.GetFloat(PREFS_PREFIX + "Coyote", 0.12f);
        jumpBufferTime = PlayerPrefs.GetFloat(PREFS_PREFIX + "JumpBuffer", 0.12f);
        autoJump = PlayerPrefs.GetInt(PREFS_PREFIX + "AutoJump", 0) == 1;
        holdToJump = PlayerPrefs.GetInt(PREFS_PREFIX + "HoldToJump", 0) == 1;
        aimAssist = PlayerPrefs.GetInt(PREFS_PREFIX + "AimAssist", 0) == 1;
        jetpackThrustMultiplier = PlayerPrefs.GetFloat(PREFS_PREFIX + "JetpackThrust", 1f);
        jetpackFuelMultiplier = PlayerPrefs.GetFloat(PREFS_PREFIX + "JetpackFuel", 1f);
        jetpackRegenMultiplier = PlayerPrefs.GetFloat(PREFS_PREFIX + "JetpackRegen", 1f);
        explorationMode = PlayerPrefs.GetInt(PREFS_PREFIX + "Exploration", 0) == 1;
        damageTakenMultiplier = PlayerPrefs.GetFloat(PREFS_PREFIX + "DamageMult", 1f);
        iFrameDuration = PlayerPrefs.GetFloat(PREFS_PREFIX + "IFrames", 1.1f);
        timeScale = PlayerPrefs.GetFloat(PREFS_PREFIX + "TimeScale", 1f);
        strongHaptics = PlayerPrefs.GetInt(PREFS_PREFIX + "StrongHaptics", 1) == 1;
        enhancedVisualFeedback = PlayerPrefs.GetInt(PREFS_PREFIX + "EnhancedVisual", 1) == 1;
        highContrast = PlayerPrefs.GetInt(PREFS_PREFIX + "HighContrast", 0) == 1;
        largeElements = PlayerPrefs.GetInt(PREFS_PREFIX + "LargeElements", 0) == 1;
    }

    // ─────────────────────────────────────────────
    //  UTILITAIRES INTERNES
    // ─────────────────────────────────────────────

    private void ApplyTimeScale()
    {
        Time.timeScale = timeScale;
        // On garde fixedDeltaTime cohérent pour la physique
        Time.fixedDeltaTime = 0.02f * timeScale;
    }

    private void NotifyChanged()
    {
        OnSettingsChanged?.Invoke();
        SaveSettings();
    }

#if UNITY_EDITOR
    // Permet de tester rapidement les changements dans l'Inspector
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyTimeScale();
            OnSettingsChanged?.Invoke();
        }
    }
#endif
}

/// <summary>
/// Profils d'accessibilité prédéfinis (utilisés par le menu).
/// </summary>
public enum AccessibilityPreset
{
    Default,
    MotorAssistance,   // Aide motrice forte
    Exploration,       // Mode découverte / facile
    VisualAssistance   // Contraste + feedbacks visuels
}
