using UnityEngine;

public class Turret : Enemy
{
    protected override void Patrol() { } // La tourelle ne bouge pas

    protected override void OnPlayerDetected()
    {
        AimAndShoot();
    }

    private void AimAndShoot()
    {
        // Code pour orienter vers le joueur et tirer
        Debug.Log("Turret shoots at player!");
    }
}
