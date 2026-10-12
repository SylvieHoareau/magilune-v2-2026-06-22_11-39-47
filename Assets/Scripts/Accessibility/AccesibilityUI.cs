using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Menu d'accessibilité Magilune.
///
/// HIERARCHY OBLIGATOIRE :
/// Canvas
/// ├── AccessibilityButton          ← bouton ouverture (HORS du panel)
/// ├── AccessibilityUI (ce script)  ← de préférence ici, PAS dans le panel
/// └── AccessibilityPanel           ← seulement le contenu du menu
///       ├── Title
///       ├── PresetsRow
///       └── Fermer Button
///
/// Si AccessibilityUI est ENFANT du Panel, le désactiver casse le bouton d'ouverture.
/// </summary>
public class AccessibilityUI : MonoBehaviour
{
    [Header("Références principales")]
    [SerializeField] private GameObject panel;
    [Tooltip("Laisser VIDE. Ne pas y mettre le Title.")]
    [SerializeField] private TextMeshProUGUI statusLabel;

    [Header("Bouton d'ouverture")]
    [Tooltip("Doit être HORS du Panel")]
    [SerializeField] private bool createFloatingButton = false;
    [SerializeField] private Button openMenuButton;

    [Header("Boutons Presets")]
    [SerializeField] private Button presetDefaultButton;
    [SerializeField] private Button presetMotorButton;
    [SerializeField] private Button presetExplorationButton;
    [SerializeField] private Button presetVisualButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;

    [Header("Toggles (optionnel)")]
    [SerializeField] private Toggle autoJumpToggle;
    [SerializeField] private Toggle holdToJumpToggle;
    [SerializeField] private Toggle aimAssistToggle;
    [SerializeField] private Toggle explorationToggle;
    [SerializeField] private Toggle strongHapticsToggle;
    [SerializeField] private Toggle enhancedVisualToggle;
    [SerializeField] private Toggle highContrastToggle;

    [Header("Sliders (optionnel)")]
    [SerializeField] private Slider moveSpeedSlider;
    [SerializeField] private Slider jumpForceSlider;
    [SerializeField] private Slider timeScaleSlider;
    [SerializeField] private Slider damageSlider;

    [Header("Labels sliders (TMP UI)")]
    [SerializeField] private TextMeshProUGUI moveSpeedLabel;
    [SerializeField] private TextMeshProUGUI jumpForceLabel;
    [SerializeField] private TextMeshProUGUI timeScaleLabel;
    [SerializeField] private TextMeshProUGUI damageLabel;

    [Header("Section avancée")]
    [SerializeField] private GameObject advancedSection;
    [SerializeField] private Button advancedToggleButton;
    [SerializeField] private bool startWithAdvancedHidden = true;

    [Header("Comportement")]
    [SerializeField] private bool pauseGameWhenOpen = true;
    [SerializeField] private bool openOnFirstLaunch = false;

    private bool isOpen;
    private bool advancedVisible;
    private float previousTimeScale = 1f;
    private bool suppressCallbacks;
    private bool buttonsWired;

    private void Awake()
    {
        EnsureManagerExists();

        WireButtons();
        WireToggles();
        WireSliders();

        if (advancedToggleButton != null)
            advancedToggleButton.onClick.AddListener(ToggleAdvancedSection);

        if (openMenuButton != null)
        {
            openMenuButton.onClick.RemoveListener(ToggleMenu);
            openMenuButton.onClick.AddListener(ToggleMenu);
        }

        advancedVisible = !startWithAdvancedHidden;
        if (advancedSection != null)
            advancedSection.SetActive(advancedVisible);

        HidePanelSafely();
    }

    private void Start()
    {
        EnsureManagerExists();
        EnsureCanvas();

        if (createFloatingButton && openMenuButton == null)
            CreateFloatingOpenButton();

        if (openMenuButton != null)
        {
            openMenuButton.onClick.RemoveListener(ToggleMenu);
            openMenuButton.onClick.AddListener(ToggleMenu);
        }

        if (AccessibilityManager.Instance != null)
            AccessibilityManager.Instance.OnSettingsChanged += RefreshUI;

        RefreshUI();
        UpdateOpenButtonVisibility();
        LogWiringStatus();

        if (openOnFirstLaunch && !PlayerPrefs.HasKey("Magilune_Access_SeenMenu"))
        {
            PlayerPrefs.SetInt("Magilune_Access_SeenMenu", 1);
            OpenMenu();
        }
    }

    private void OnDestroy()
    {
        if (AccessibilityManager.Instance != null)
            AccessibilityManager.Instance.OnSettingsChanged -= RefreshUI;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ToggleMenu();
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            ToggleMenu();
    }

    private void HidePanelSafely()
    {
        if (panel == null) return;

        if (panel == gameObject)
        {
            Debug.LogWarning("[AccessibilityUI] Le Panel est le même objet que le script. Mets le script sur le Canvas.");
            return;
        }

        if (transform.IsChildOf(panel.transform))
        {
            Debug.LogWarning("[AccessibilityUI] Ce script est ENFANT du Panel. Déplace AccessibilityUI hors du Panel (sous le Canvas). Panel non désactivé pour ne pas casser le menu.");
            return;
        }

        panel.SetActive(false);
    }

    public void ApplyNormalPreset()
    {
        Debug.Log("[AccessibilityUI] Clic → Normal");
        ApplyPreset(AccessibilityPreset.Default);
    }

    public void ApplyMotorPreset()
    {
        Debug.Log("[AccessibilityUI] Clic → J'ai besoin d'aide");
        ApplyPreset(AccessibilityPreset.MotorAssistance);
    }

    public void ApplyExplorationPreset()
    {
        Debug.Log("[AccessibilityUI] Clic → Je découvre");
        ApplyPreset(AccessibilityPreset.Exploration);
    }

    public void ApplyVisualPreset()
    {
        Debug.Log("[AccessibilityUI] Clic → Je vois mieux");
        ApplyPreset(AccessibilityPreset.VisualAssistance);
    }

    public void ToggleMenu()
    {
        Debug.Log("[AccessibilityUI] ToggleMenu isOpen=" + isOpen);
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        Debug.Log("[AccessibilityUI] OpenMenu");
        if (panel == null)
        {
            Debug.LogWarning("[AccessibilityUI] Panel non assigné.");
            return;
        }

        EnsureManagerExists();
        isOpen = true;
        panel.SetActive(true);
        RefreshUI();
        UpdateOpenButtonVisibility();

        if (pauseGameWhenOpen)
        {
            previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            PlayerController.IsGamePaused = true;
        }
    }

    public void CloseMenu()
    {
        Debug.Log("[AccessibilityUI] CloseMenu");
        if (panel == null) return;

        isOpen = false;
        if (panel != gameObject && !transform.IsChildOf(panel.transform))
            panel.SetActive(false);

        UpdateOpenButtonVisibility();

        if (pauseGameWhenOpen)
        {
            PlayerController.IsGamePaused = false;
            float restore = 1f;
            if (AccessibilityManager.Instance != null)
                restore = Mathf.Max(0.01f, AccessibilityManager.Instance.timeScale);
            else if (previousTimeScale > 0f)
                restore = previousTimeScale;
            Time.timeScale = restore;
        }

        if (AccessibilityManager.Instance != null)
            AccessibilityManager.Instance.SaveSettings();
    }

    public void ToggleAdvancedSection()
    {
        advancedVisible = !advancedVisible;
        if (advancedSection != null)
            advancedSection.SetActive(advancedVisible);
    }

    private void WireButtons()
    {
        if (buttonsWired) return;
        buttonsWired = true;
        BindButton(presetDefaultButton, ApplyNormalPreset);
        BindButton(presetMotorButton, ApplyMotorPreset);
        BindButton(presetExplorationButton, ApplyExplorationPreset);
        BindButton(presetVisualButton, ApplyVisualPreset);
        BindButton(resetButton, ApplyNormalPreset);
        BindButton(closeButton, CloseMenu);
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void WireToggles()
    {
        BindToggle(autoJumpToggle, v => AccessibilityManager.Instance?.SetAutoJump(v));
        BindToggle(holdToJumpToggle, v => AccessibilityManager.Instance?.SetHoldToJump(v));
        BindToggle(aimAssistToggle, v => AccessibilityManager.Instance?.SetAimAssist(v));
        BindToggle(explorationToggle, v => AccessibilityManager.Instance?.SetExplorationMode(v));
        BindToggle(strongHapticsToggle, v => AccessibilityManager.Instance?.SetStrongHaptics(v));
        BindToggle(enhancedVisualToggle, v => AccessibilityManager.Instance?.SetEnhancedVisualFeedback(v));
        BindToggle(highContrastToggle, v => AccessibilityManager.Instance?.SetHighContrast(v));
    }

    private void BindToggle(Toggle t, System.Action<bool> action)
    {
        if (t == null) return;
        t.onValueChanged.AddListener(v =>
        {
            if (suppressCallbacks) return;
            action(v);
        });
    }

    private void WireSliders()
    {
        SetupSlider(moveSpeedSlider, 0.5f, 2f, v =>
        {
            AccessibilityManager.Instance?.SetMoveSpeedMultiplier(v);
            UpdateSliderLabel(moveSpeedLabel, "Vitesse", v, "x");
        });
        SetupSlider(jumpForceSlider, 0.7f, 1.8f, v =>
        {
            AccessibilityManager.Instance?.SetJumpForceMultiplier(v);
            UpdateSliderLabel(jumpForceLabel, "Saut", v, "x");
        });
        SetupSlider(timeScaleSlider, 0.5f, 1f, v =>
        {
            if (AccessibilityManager.Instance == null) return;
            AccessibilityManager.Instance.timeScale = v;
            AccessibilityManager.Instance.SaveSettings();
            UpdateSliderLabel(timeScaleLabel, "Rythme", v, "x");
        });
        SetupSlider(damageSlider, 0.25f, 1f, v =>
        {
            AccessibilityManager.Instance?.SetDamageTakenMultiplier(v);
            UpdateSliderLabel(damageLabel, "Dégâts", v, "x");
        });
    }

    private void SetupSlider(Slider s, float min, float max, System.Action<float> action)
    {
        if (s == null) return;
        s.minValue = min;
        s.maxValue = max;
        s.onValueChanged.AddListener(v =>
        {
            if (suppressCallbacks) return;
            action(v);
        });
    }

    private void ApplyPreset(AccessibilityPreset preset)
    {
        EnsureManagerExists();
        if (AccessibilityManager.Instance == null)
        {
            Debug.LogError("[AccessibilityUI] AccessibilityManager introuvable.");
            return;
        }
        AccessibilityManager.Instance.ApplyPreset(preset);
        RefreshUI();
        Debug.Log("[AccessibilityUI] Preset appliqué : " + preset);
    }

    public void RefreshUI()
    {
        if (AccessibilityManager.Instance == null) return;
        suppressCallbacks = true;
        var m = AccessibilityManager.Instance;
        SetToggle(autoJumpToggle, m.autoJump);
        SetToggle(holdToJumpToggle, m.holdToJump);
        SetToggle(aimAssistToggle, m.aimAssist);
        SetToggle(explorationToggle, m.explorationMode);
        SetToggle(strongHapticsToggle, m.strongHaptics);
        SetToggle(enhancedVisualToggle, m.enhancedVisualFeedback);
        SetToggle(highContrastToggle, m.highContrast);
        SetSlider(moveSpeedSlider, m.moveSpeedMultiplier);
        SetSlider(jumpForceSlider, m.jumpForceMultiplier);
        SetSlider(timeScaleSlider, m.timeScale);
        SetSlider(damageSlider, m.damageTakenMultiplier);
        UpdateSliderLabel(moveSpeedLabel, "Vitesse", m.moveSpeedMultiplier, "x");
        UpdateSliderLabel(jumpForceLabel, "Saut", m.jumpForceMultiplier, "x");
        UpdateSliderLabel(timeScaleLabel, "Rythme", m.timeScale, "x");
        UpdateSliderLabel(damageLabel, "Dégâts", m.damageTakenMultiplier, "x");
        if (statusLabel != null)
            statusLabel.text = BuildStatusText(m);
        suppressCallbacks = false;
    }

    private void LogWiringStatus()
    {
        if (openMenuButton == null)
            Debug.LogWarning("[AccessibilityUI] Open Menu Button non assigné.");
        if (panel == null)
            Debug.LogWarning("[AccessibilityUI] Panel non assigné.");
        if (panel != null && panel != gameObject && transform.IsChildOf(panel.transform))
            Debug.LogWarning("[AccessibilityUI] Déplace AccessibilityUI HORS du Panel.");
        if (AccessibilityManager.Instance != null)
            Debug.Log("[AccessibilityUI] Prêt.");
    }

    private void EnsureCanvas()
    {
        if (GetComponentInParent<Canvas>() != null) return;
        if (FindFirstObjectByType<Canvas>() != null) return;
        GameObject canvasGo = new GameObject("AccessibilityCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();
    }

    private void CreateFloatingOpenButton()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject root = new GameObject("BtnOpenAccessibility");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(1f, 1f);
        rootRt.anchorMax = new Vector2(1f, 1f);
        rootRt.pivot = new Vector2(1f, 1f);
        rootRt.anchoredPosition = new Vector2(-16f, -16f);
        rootRt.sizeDelta = new Vector2(160f, 64f);
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.95f, 0.35f, 0.65f, 0.95f);
        openMenuButton = root.AddComponent<Button>();
        openMenuButton.targetGraphic = bg;
        openMenuButton.onClick.AddListener(ToggleMenu);
        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(root.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        Text label = textGo.AddComponent<Text>();
        label.text = "Aide";
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.fontSize = 28;
        label.fontStyle = FontStyle.Bold;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (label.font == null)
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private void UpdateOpenButtonVisibility()
    {
        if (openMenuButton != null)
            openMenuButton.gameObject.SetActive(!isOpen);
    }

    private static void SetToggle(Toggle t, bool value) { if (t != null) t.isOn = value; }
    private static void SetSlider(Slider s, float value) { if (s != null) s.SetValueWithoutNotify(value); }

    private static void UpdateSliderLabel(TextMeshProUGUI label, string name, float value, string suffix)
    {
        if (label != null)
            label.text = name + " : " + value.ToString("0.00") + suffix;
    }

    private static string BuildStatusText(AccessibilityManager m)
    {
        if (m.autoJump || m.holdToJump || m.aimAssist) return "Mode : J'ai besoin d'aide";
        if (m.highContrast || m.largeElements) return "Mode : Je vois mieux";
        if (m.explorationMode) return "Mode : Je découvre";
        return "Mode : Normal";
    }

    private void EnsureManagerExists()
    {
        if (AccessibilityManager.Instance != null) return;
        if (FindFirstObjectByType<AccessibilityManager>() != null) return;
        new GameObject("AccessibilityManager").AddComponent<AccessibilityManager>();
    }
}
