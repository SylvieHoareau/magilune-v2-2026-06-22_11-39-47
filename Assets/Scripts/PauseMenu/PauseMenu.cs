using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu pause : Pause (Escape / Start) pour mettre en pause ou reprendre,
/// et bouton pour retourner au menu principal.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;   // Le panneau qui contient les boutons

    [Header("Scènes")]
    [Tooltip("Nom exact de la scène du menu principal")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Au démarrage on s'assure que le temps tourne
        Time.timeScale = 1f;
        isPaused = false;
    }

    /// <summary>
    /// À brancher sur l'action Pause (Invoke Unity Events → started).
    /// </summary>
    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;               // Stoppe le jeu (physique, Update, etc.)

        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Optionnel : afficher le curseur si tu joues à la souris
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        PlayerController.IsGamePaused = true;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Cursor.visible = false;            // À adapter selon ton jeu
        Cursor.lockState = CursorLockMode.Locked;

        PlayerController.IsGamePaused = false;
    }

    /// <summary>
    /// Appelé par le bouton "Reprendre" du panneau.
    /// </summary>
    public void OnResumeButton()
    {
        Resume();
    }

    /// <summary>
    /// Appelé par le bouton "Menu principal".
    /// </summary>
    public void OnMainMenuButton()
    {
        // Important : remettre le timeScale avant de changer de scène
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Optionnel : bouton Quitter (uniquement en build, pas dans l’éditeur).
    /// </summary>
    public void OnQuitButton()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
