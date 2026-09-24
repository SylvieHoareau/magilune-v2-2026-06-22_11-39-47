using UnityEngine;

/// <summary>
/// À mettre sur les objets de lave et de poison.
/// Fonctionne avec PlayerHealth (tags Lava / Poison).
/// </summary>
public class Hazard : MonoBehaviour
{
    public enum HazardType
    {
        Lava,
        Poison
    }

    [Header("Type de danger")]
    public HazardType type = HazardType.Lava;

    [Header("Optionnel")]
    [Tooltip("Si coché, le collider sera mis en Trigger automatiquement")]
    public bool forceTrigger = true;

    private void Reset()
    {
        // Se lance quand tu ajoutes le composant
        Setup();
    }

    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        // Met le bon tag automatiquement
        if (type == HazardType.Lava)
            gameObject.tag = "Lava";
        else
            gameObject.tag = "Poison";

        // Force le collider en Trigger (recommandé)
        if (forceTrigger)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }
    }
}
