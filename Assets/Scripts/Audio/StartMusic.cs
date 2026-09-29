using UnityEngine;

public class StartMusic : MonoBehaviour
{
    [SerializeField] private AudioClip jungleMusic;

    private void Start()
    {
        if (jungleMusic != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(jungleMusic);
    }
}
