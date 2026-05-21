using UnityEngine;

/*
    Simple chase-and-attack enemy.
    Idle until it has line of sight to the player, then walks toward them,
    and once in range plays the attack animation which knocks the player back.
*/

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Idle, Chase, Attack }

    [Header("Detection")]
    [SerializeField] private float sightRange = 8f;
    [SerializeField] private float attackRange = 1.5f;
    [Tooltip("Layers that block line of sight (e.g. Ground).")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;
    [Tooltip("Delay into the attack before the push lands.")]
    [SerializeField] private float attackWindup = 0.2f;
    [Tooltip("How long the attack state lasts before re-evaluating.")]
    [SerializeField] private float attackDuration = 0.35f;

    [Header("Knockback Applied To Player")]
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float knockbackUp = 4f;
    [Tooltip("How long the player loses movement control after the hit.")]
    [SerializeField] private float knockbackLockout = 0.2f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private PlayerMovement player;
    private State state = State.Idle;

    // Attack sequencing.
    private float attackTimer;
    private float cooldownTimer;
    private bool knockbackApplied;

    private static readonly int IdleHash = Animator.StringToHash("EnemyIdle");
    private static readonly int WalkHash = Animator.StringToHash("EnemyWalk");
    private static readonly int AttackHash = Animator.StringToHash("EnemyAttack");
    private int currentStateHash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.fixedDeltaTime;

        if (!AcquirePlayer())
        {
            EnterIdle();
            return;
        }

        // Once an attack has started it runs to completion.
        if (state == State.Attack)
        {
            TickAttack();
            return;
        }

        bool hasLineOfSight = HasLineOfSight(out float distance);

        if (hasLineOfSight && distance <= attackRange && cooldownTimer <= 0f)
            BeginAttack();
        else if (hasLineOfSight && distance > attackRange)
            Chase();
        else
            EnterIdle();
    }

    private bool AcquirePlayer()
    {
        // Re-acquire after time-loop respawns destroy the old player.
        if (player == null)
            player = FindAnyObjectByType<PlayerMovement>();
        return player != null;
    }

    private bool HasLineOfSight(out float distance)
    {
        Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
        distance = toPlayer.magnitude;

        if (distance > sightRange)
            return false;

        // Blocked if an obstacle sits between us and the player.
        return Physics2D.Linecast(transform.position, player.transform.position, obstacleMask).collider == null;
    }

    private void Chase()
    {
        state = State.Chase;
        Play(WalkHash);

        float dirX = Mathf.Sign(player.transform.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);
        FaceDirection(dirX);
    }

    private void EnterIdle()
    {
        state = State.Idle;
        Play(IdleHash);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void BeginAttack()
    {
        state = State.Attack;
        attackTimer = 0f;
        knockbackApplied = false;
        Play(AttackHash);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        FaceDirection(Mathf.Sign(player.transform.position.x - transform.position.x));
    }

    private void TickAttack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        attackTimer += Time.fixedDeltaTime;

        if (!knockbackApplied && attackTimer >= attackWindup)
        {
            knockbackApplied = true;
            ShovePlayer();
        }

        if (attackTimer >= attackDuration)
        {
            cooldownTimer = attackCooldown;
            state = State.Idle; // Re-evaluated next FixedUpdate.
        }
    }

    private void ShovePlayer()
    {
        if (player == null)
            return;

        float dirX = Mathf.Sign(player.transform.position.x - transform.position.x);
        player.ApplyKnockback(new Vector2(dirX * knockbackForce, knockbackUp), knockbackLockout);
    }

    private void FaceDirection(float dirX)
    {
        // Sprites face east (right) by default; flip when heading left.
        if (dirX != 0f)
            spriteRenderer.flipX = dirX < 0f;
    }

    private void Play(int stateHash)
    {
        if (currentStateHash == stateHash)
            return;
        currentStateHash = stateHash;
        animator.Play(stateHash);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
