using UnityEngine;

/// <summary>
/// Enemy behaviour implemented as a finite state machine:
/// Idle  -> Patrol  -> Chase  -> Patrol
/// </summary>
[RequireComponent(typeof(Chaser))]
[RequireComponent(typeof(Patroller))]
[RequireComponent(typeof(GuardIdle))]
public class GuardEnemyStateMachine : StateMachine
{
    [Tooltip("Radius in which the enemy sees the player and starts chasing.")]
    [SerializeField] private float watchRadius = 5f;

    [Tooltip("Radius after which the enemy considers the player lost and goes back to patrol.")]
    [SerializeField] private float lostRadius = 7f;

    private Chaser chaser;
    private Patroller patroller;
    private GuardIdle idle;

    private void Awake()
    {
        chaser = GetComponent<Chaser>();
        patroller = GetComponent<Patroller>();
        idle = GetComponent<GuardIdle>();

        // Register states. The first state that is added becomes the initial state (Idle).
        AddState(idle)
            .AddState(patroller)
            .AddState(chaser);

        // Define transitions between states.
        // Idle -> Chase (player enters watch radius)
        AddTransition(idle, () => PlayerInSight(), chaser);

        // Idle -> Patrol (idle time finished and player not in sight)
        AddTransition(idle, () => idle.IsFinished && !PlayerInSight(), patroller);

        // Patrol -> Chase (player enters watch radius)
        AddTransition(patroller, () => PlayerInSight(), chaser);

        // Chase -> Patrol (player is no longer in sight and far enough)
        AddTransition(chaser, () => !PlayerInSight() && PlayerTooFar(), patroller);
    }

    private float DistanceToPlayer()
    {
        if (chaser == null)
            return Mathf.Infinity;

        return Vector3.Distance(transform.position, chaser.TargetObjectPosition());
    }

    private bool PlayerInSight()
    {
        return DistanceToPlayer() <= watchRadius;
    }

    private bool PlayerTooFar()
    {
        return DistanceToPlayer() >= lostRadius;
    }

    private void OnDrawGizmosSelected()
    {
        // Yellow: watch radius (start chasing)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, watchRadius);

        // Red: lost radius (stop chasing, back to patrol)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lostRadius);
    }
}
