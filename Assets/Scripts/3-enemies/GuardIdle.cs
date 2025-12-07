using UnityEngine;

/// <summary>
/// State in which the enemy stands still for a short time.
/// Used as the first state of the guard.
/// </summary>
public class GuardIdle : MonoBehaviour
{
    [Tooltip("How long to stay in idle state (seconds).")]
    [SerializeField] private float idleDuration = 2f;

    private float timer = 0f;

    /// <summary>
    /// True when the idle time has finished.
    /// </summary>
    public bool IsFinished => timer >= idleDuration;

    private void OnEnable()
    {
        // Reset timer whenever we enter this state.
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        // No movement here. Other movement states (Patroller / Chaser)
        // are disabled while we are in Idle.
    }
}
