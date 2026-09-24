using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("0 = ne bouge presque pas, 1 = suit parfaitement la caméra")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0.5f;

    [Tooltip("Coche si tu veux aussi du parallax vertical (recommandé pour un level vertical)")]
    public bool affectY = true;

    private Transform cam;
    private Vector3 startPos;
    private float startZ;

    void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position;
        startZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        float x = startPos.x + (cam.position.x - startPos.x) * parallaxFactor;
        float y = affectY ? startPos.y + (cam.position.y - startPos.y) * parallaxFactor : startPos.y;

        transform.position = new Vector3(x, y, startZ);
    }
}
