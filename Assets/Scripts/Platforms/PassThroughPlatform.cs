using UnityEngine;
using System.Collections;

public class PassThroughPlatform : MonoBehaviour
{
    private PlatformEffector2D effector;

    void Start() => effector = GetComponent<PlatformEffector2D>();

    // Appelé quand joueur appuie Bas + Interagir
    public void DropThrough()
    {
        StartCoroutine(TemporaryDisable());
    }

    private IEnumerator TemporaryDisable()
    {
        effector.rotationalOffset = 180f;
        yield return new WaitForSeconds(0.3f);
        effector.rotationalOffset = 0f;
    }
}
