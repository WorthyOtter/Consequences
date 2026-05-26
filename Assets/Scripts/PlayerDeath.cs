using System.Collections;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private TimeLoopManager timeLoopManager;

    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private Animator spriteAnimator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D rb;

    [Header("Death Animation")]
    [SerializeField] private float deathAnimationDuration = 0.6f;

    private bool hasDied;
    private float retryCooldownTimer;

    private void Awake()
    {
        if (input == null)
            input = GetComponent<PlayerInputHandler>();
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (spriteAnimator == null && playerMovement != null)
            spriteAnimator = playerMovement.spriteAnimator;
        if (spriteAnimator == null)
            spriteAnimator = GetComponentInChildren<Animator>();
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

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (spriteAnimator != null && spriteAnimator.runtimeAnimatorController != null)
        {
            spriteAnimator.SetBool("IsCrouching", false);
            spriteAnimator.SetBool("IsMoving", false);
            spriteAnimator.SetBool("IsGrounded", true);
            spriteAnimator.Play("Death", 0, 0f);
        }
        else
        {
            Debug.LogWarning("[PlayerDeath] spriteAnimator missing or has no controller — death animation skipped.");
        }

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSecondsRealtime(deathAnimationDuration);

        if (timeLoopManager != null)
        {
            timeLoopManager.PlayerDied();
        }
        else
        {
            Debug.LogWarning("[PlayerDeath] timeLoopManager is null — respawn skipped.");
        }
    }
}
