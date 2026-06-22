using UnityEngine;

public class Swordsman : Enemy
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float moveSpeed = 2f;

    protected override void OnPlayerDetected()
    {
        // Poursuit le joueur
        MoveTowardPlayer();
        if (Vector2.Distance(transform.position, player.position) < attackRange)
            Attack();
    }

    private void MoveTowardPlayer()
    {
        // Code pour se déplacer vers le joueur
        transform.position = Vector2.MoveTowards(transform.position, player.position, Time.deltaTime * moveSpeed);
    }

    private void Attack()
    {
        // Code pour attaquer le joueur
        Debug.Log("Swordsman attacks!");
    }
}
