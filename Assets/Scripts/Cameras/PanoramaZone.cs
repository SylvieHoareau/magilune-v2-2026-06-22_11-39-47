using UnityEngine;
using Unity.Cinemachine;

public class PanoramaZone : MonoBehaviour
{
    [SerializeField] private CinemachineCamera panoramicCam;
    [SerializeField] private CinemachineCamera playerCam;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            panoramicCam.Priority.Value = 20; // Priorité haute = active
            playerCam.Priority.Value = 10;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            panoramicCam.Priority.Value = 10;
            playerCam.Priority.Value = 20;
        }
    }
}
