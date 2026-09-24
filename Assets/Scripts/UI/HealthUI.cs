using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image[] hearts;          // 3 images
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private PlayerHealth playerHealth;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged += UpdateHearts;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= UpdateHearts;
    }

    private void Start()
    {
        if (playerHealth != null)
            UpdateHearts(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            bool isFull = i < current;
            hearts[i].sprite = isFull ? fullHeart : emptyHeart;

            // Petite animation de scale quand on perd/gagne une vie
            if (isFull)
                hearts[i].transform.localScale = Vector3.one;
            else
                hearts[i].transform.localScale = Vector3.one * 0.85f;
        }
    }
}
