using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Menu d'accessibilité pour Magilune.
///
/// Tous les labels utilisent TextMeshProUGUI (composant UI Canvas).
/// Ne pas utiliser TextMeshPro (3D) ni UnityEngine.UI.Text pour les labels.
/// </summary>
public class AccessibilityUI : MonoBehaviour
{
    [Header("Références principales")]
    [SerializeField] private GameObject panel;
    [Tooltip("TextMeshPro - Text (UI) uniquement")]
    [SerializeField] private TextMeshProUGUI statusLabel;

    [Header("Bouton d'ouverture (créé auto si vide)")]
    [SerializeField] private bool createFloatingButton = true;
    [SerializeField] private Button openMenuButton;

    [Header("Boutons Presets")]
    [SerializeField] private Button presetDefaultButton;
    [SerializeField] private Button presetMotorButton;
    [SerializeField] private Button presetExplorationButton;
    [SerializeField] private Button presetVisualButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;

    [Header("Toggles")]
    [SerializeField] private Toggle autoJumpToggle;
    [SerializeField] private Toggle holdToJumpToggle;
    [SerializeField] private Toggle aimAssistToggle;
    [SerializeField] private Toggle explorationToggle;
    [SerializeField] private Toggle strongHapticsToggle;
    [SerializeField] private Toggle enhancedVisualToggle;
    [SerializeField] private Toggle highContrastToggle;

    [Header("Sliders")]
    [SerializeField] private Slider moveSpeedSlider;
    [SerializeField] private Slider jumpForceSlider;
    [SerializeField] private Slider timeScaleSlider;
    [SerializeField] private Slider damageSlider;

    [Header("Labels des sliders (TextMeshPro - Text UI)")]
    [SerializeField] private TextMeshProUGUI moveSpeedLabel;
    [SerializeField] private TextMeshProUGUI jumpForceLabel;
    [SerializeField] private TextMeshProUGUI timeScaleLabel;
    [SerializeField] private TextMeshProUGUI damageLabel;

    [Header("Section avancée (optionnel)")]
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

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        WireButtons();
        WireToggles();
        WireSliders();

        if (advancedToggleButton != null)
            advancedToggleButton.onClick.AddListener(ToggleAdvancedSection);

        advancedVisible = !startWithAdvancedHidden;
        if (advancedSection != null)
            advancedSection.SetActive(advancedVisible);
    }

    private void Start()
    {
        EnsureManagerExists();
        EnsureCanvas();

        if (createFloatingButton && openMenuButton == null)
            CreateFloatingOpenButton();
        else if (openMenuButton != null)
            openMenuButton.onClick.AddListener(ToggleMenu);

        if (AccessibilityManager.Instance != null)
            AccessibilityManager.Instance.OnSettingsChanged += RefreshUI;

        RefreshUI();
        UpdateOpenButtonVisibility();

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
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("[AccessibilityUI] Aucun Canvas trouvé.");
            return;
        }

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
        ColorBlock colors = openMenuButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.85f, 0.95f, 1f);
        colors.pressedColor = new Color(0.85f, 0.25f, 0.55f, 1f);
        openMenuButton.colors = colors;
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

    public void ToggleMenu()
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        if (panel == null)
        {
            Debug.LogWarning("[AccessibilityUI] Assigne le Panel dans l'Inspector.");
            return;
        }

        isOpen = true;
        panel.SetActive(true);
        RefreshUI();
        UpdateOpenButtonVisibility();

        if (pauseGameWhenOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            PlayerController.IsGamePaused = true;
        }
    }

    public void CloseMenu()
    {
        if (panel == null) return;

        isOpen = false;
        panel.SetActive(false);
        UpdateOpenButtonVisibility();

        if (pauseGameWhenOpen)
        {
            PlayerController.IsGamePaused = false;
            if (AccessibilityManager.Instance != null)
                Time.timeScale = AccessibilityManager.Instance.timeScale;
            else
                Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
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
        if (presetDefaultButton != null)
            presetDefaultButton.onClick.AddListener(() => ApplyPreset(AccessibilityPreset.Default));
        if (presetMotorButton != null)
            presetMotorButton.onClick.AddListener(() => ApplyPreset(AccessibilityPreset.MotorAssistance));
        if (presetExplorationButton != null)
            presetExplorationButton.onClick.AddListener(() => ApplyPreset(AccessibilityPreset.Exploration));
        if (presetVisualButton != null)
            presetVisualButton.onClick.AddListener(() => ApplyPreset(AccessibilityPreset.VisualAssistance));
        if (resetButton != null)
            resetButton.onClick.AddListener(() => ApplyPreset(AccessibilityPreset.Default));
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMenu);
    }

    private void WireToggles()
    {
        BindToggle(autoJumpToggle, v => AccessibilityManager.Instance.SetAutoJump(v));
        BindToggle(holdToJumpToggle, v => AccessibilityManager.Instance.SetHoldToJump(v));
        BindToggle(aimAssistToggle, v => AccessibilityManager.Instance.SetAimAssist(v));
        BindToggle(explorationToggle, v => AccessibilityManager.Instance.SetExplorationMode(v));
        BindToggle(strongHapticsToggle, v => AccessibilityManager.Instance.SetStrongHaptics(v));
        BindToggle(enhancedVisualToggle, v => AccessibilityManager.Instance.SetEnhancedVisualFeedback(v));
        BindToggle(highContrastToggle, v => AccessibilityManager.Instance.SetHighContrast(v));
    }

    private void BindToggle(Toggle t, System.Action<bool> action)
    {
        if (t == null) return;
        t.onValueChanged.AddListener(v =>
        {
            if (suppressCallbacks || AccessibilityManager.Instance == null) return;
            action(v);
        });
    }

    private void WireSliders()
    {
        if (moveSpeedSlider != null)
        {
            moveSpeedSlider.minValue = 0.5f;
            moveSpeedSlider.maxValue = 2f;
            moveSpeedSlider.onValueChanged.AddListener(v =>
            {
                if (suppressCallbacks || AccessibilityManager.Instance == null) return;
                AccessibilityManager.Instance.SetMoveSpeedMultiplier(v);
                UpdateSliderLabel(moveSpeedLabel, "Vitesse", v, "x");
            });
        }

        if (jumpForceSlider != null)
        {
            jumpForceSlider.minValue = 0.7f;
            jumpForceSlider.maxValue = 1.8f;
            jumpForceSlider.onValueChanged.AddListener(v =>
            {
                if (suppressCallbacks || AccessibilityManager.Instance == null) return;
                AccessibilityManager.Instance.SetJumpForceMultiplier(v);
                UpdateSliderLabel(jumpForceLabel, "Saut", v, "x");
            });
        }

        if (timeScaleSlider != null)
        {
            timeScaleSlider.minValue = 0.5f;
            timeScaleSlider.maxValue = 1f;
            timeScaleSlider.onValueChanged.AddListener(v =>
            {
                if (suppressCallbacks || AccessibilityManager.Instance == null) return;
                AccessibilityManager.Instance.timeScale = v;
                AccessibilityManager.Instance.SaveSettings();
                UpdateSliderLabel(timeScaleLabel, "Rythme", v, "x");
            });
        }

        if (damageSlider != null)
        {
            damageSlider.minValue = 0.25f;
            damageSlider.maxValue = 1f;
            damageSlider.onValueChanged.AddListener(v =>
            {
                if (suppressCallbacks || AccessibilityManager.Instance == null) return;
                AccessibilityManager.Instance.SetDamageTakenMultiplier(v);
                UpdateSliderLabel(damageLabel, "Dégâts", v, "x");
            });
        }
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

    private void ApplyPreset(AccessibilityPreset preset)
    {
        if (AccessibilityManager.Instance == null) return;
        AccessibilityManager.Instance.ApplyPreset(preset);
        RefreshUI();
    }

    private static void SetToggle(Toggle t, bool value)
    {
        if (t != null) t.isOn = value;
    }

    private static void SetSlider(Slider s, float value)
    {
        if (s != null) s.SetValueWithoutNotify(value);
    }

    private static void UpdateSliderLabel(TextMeshProUGUI label, string name, float value, string suffix)
    {
        if (label != null)
            label.text = name + " : " + value.ToString("0.00") + suffix;
    }

    private static string BuildStatusText(AccessibilityManager m)
    {
        if (m.explorationMode) return "Mode : Je découvre";
        if (m.autoJump || m.holdToJump || m.aimAssist) return "Mode : J'ai besoin d'aide";
        if (m.highContrast) return "Mode : Je vois mieux";
        return "Mode : Normal";
    }

    private void EnsureManagerExists()
    {
        if (AccessibilityManager.Instance != null) return;
        new GameObject("AccessibilityManager").AddComponent<AccessibilityManager>();
    }
}
