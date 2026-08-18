using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PanoramaTrigger : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private bool returnToPlayerOnExit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si c'est bien le joueur qui entre dans la zone
        if (other.CompareTag("Player"))
        {
            cameraManager.ActivatePanoramaCamera();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Si souhaité, réactive la caméra du joueur en sortant de la zone
        if (returnToPlayerOnExit && other.CompareTag("Player"))
        {
            cameraManager.ActivatePlayerCamera();
        }
    }
}
