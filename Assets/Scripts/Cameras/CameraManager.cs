using UnityEngine;
using Unity.Cinemachine; // Sous Unity 6 / Cinemachine 3.x

public class CameraManager : MonoBehaviour
{
    [Header("Caméras Virtuals Cinemachine")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera panoramaCamera;

    [Header("Priorités")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 0;

    /// <summary>
    /// Active la caméra panoramique pour offrir une vue contemplative.
    /// </summary>
    public void ActivatePanoramaCamera()
    {
        if (panoramaCamera != null && playerCamera != null)
        {
            panoramaCamera.Priority = activePriority;
            playerCamera.Priority = inactivePriority;
        }
    }

    /// <summary>
    /// Réactive la caméra qui suit le joueur.
    /// </summary>
    public void ActivatePlayerCamera()
    {
        if (panoramaCamera != null && playerCamera != null)
        {
            playerCamera.Priority = activePriority;
            panoramaCamera.Priority = inactivePriority;
        }
    }
}
