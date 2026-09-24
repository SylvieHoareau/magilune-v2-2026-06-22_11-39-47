using UnityEngine;

public class BiomeSwitcher : MonoBehaviour
{
    [Header("Parallax Roots")]
    public CanvasGroup jungleGroup;   // Mets un CanvasGroup sur le parent Jungle
    public CanvasGroup volcanoGroup;  // Pareil pour Volcano

    [Header("Transition")]
    public Transform player;
    public float transitionStartY = 40f;   // début de la transition
    public float transitionEndY   = 60f;   // fin de la transition

    void Update()
    {
        if (player == null) return;

        float t = Mathf.InverseLerp(transitionStartY, transitionEndY, player.position.y);
        t = Mathf.Clamp01(t);

        // Jungle disparaît progressivement
        jungleGroup.alpha = 1f - t;
        // Volcano apparaît progressivement
        volcanoGroup.alpha = t;

        // Optionnel : désactiver complètement quand invisible (perf)
        jungleGroup.gameObject.SetActive(t < 0.99f);
        volcanoGroup.gameObject.SetActive(t > 0.01f);
    }
}
