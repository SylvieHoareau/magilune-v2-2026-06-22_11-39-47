using UnityEngine;

public abstract class Enemy : Entity
{
    [Header("Detection")]
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected LayerMask playerLayer;

    [Header("Patrol")]
    [SerializeField] protected Transform[] patrolPoints;
    protected int currentPatrolIndex;
    protected bool playerDetected;

    protected Transform player;

    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindWithTag("Player")?.transform;
    }

    protected virtual void Update()
    {
        playerDetected = DetectPlayer();
        if (playerDetected) OnPlayerDetected();
        else Patrol();
    }

    protected bool DetectPlayer()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) < detectionRange;
    }

    protected abstract void OnPlayerDetected();

    protected virtual void Patrol()
    {
        // Déplacement entre les points de patrouille
    }
}