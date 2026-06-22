using UnityEngine;
using System.Collections;

public class Gunner : Enemy
{
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private GameObject bulletPrefab;

    protected override void OnPlayerDetected()
    {
        // Oriente vers le joueur et tire
        StartCoroutine(ShootRoutine());
    }

    // Coroutine pour tirer à intervalles réguliers
    private IEnumerator ShootRoutine()
    {    while (playerDetected)
        {
            // Instancie une balle vers le joueur
            yield return new WaitForSeconds(1f / fireRate);
        }
    }
}


