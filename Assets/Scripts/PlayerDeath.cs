using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private TimeLoopManager timeLoopManager;

    [Header("References")]
    [SerializeField] private PlayerInputHandler input;

    private bool hasDied;
    private float retryCooldownTimer;

    private void Awake()
    {
        if (input == null)
            input = GetComponent<PlayerInputHandler>();
    }

    private void Start()
    {
        timeLoopManager = FindAnyObjectByType<TimeLoopManager>();

        // Prevent the new player from instantly consuming the same Retry press
        // that caused the previous death.
        retryCooldownTimer = 0.15f;
    }

    private void Update()
    {
        if (hasDied)
            return;

        if (retryCooldownTimer > 0f)
        {
            retryCooldownTimer -= Time.deltaTime;
            return;
        }

        if (input != null && input.RetryPressed)
        {
            Die();
        }
    }

    public void Die()
    {
        if (hasDied)
            return;

        hasDied = true;

        Debug.Log("Death triggered by: " + gameObject.name);

        if (timeLoopManager != null)
            timeLoopManager.PlayerDied();
    }
}