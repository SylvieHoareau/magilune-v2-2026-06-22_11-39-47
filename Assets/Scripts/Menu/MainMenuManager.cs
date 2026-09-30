using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Level1";

    [Header("Audio")]
    [SerializeField] private AudioClip menuMusic;

    void Start()
    {
        if (AudioManager.Instance != null && menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(menuMusic);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
