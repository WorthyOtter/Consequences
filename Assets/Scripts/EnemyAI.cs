using System.Collections.Generic;
using UnityEngine;

/*
    Simple chase-and-attack enemy.
    Idle until it has line of sight to a target (the player or a clone), then
    walks toward the closest visible one, and once in range plays the attack
    animation which knocks that target back.
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
    [Tooltip("Layers that count as targets (Player + Clone).")]
    [SerializeField] private LayerMask targetMask;
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

    [Header("Knockback Applied To Target")]
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float knockbackUp = 4f;
    [Tooltip("How long the target loses movement control after the hit.")]
    [SerializeField] private float knockbackLockout = 0.2f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private State state = State.Idle;

    // The target currently being chased/attacked. A UnityEngine.Object reference
    // so destroyed clones compare == null correctly.
    private MonoBehaviour target;

    // Attack sequencing.
    private float attackTimer;
    private float cooldownTimer;
    private bool knockbackApplied;

    // Reusable buffers for target detection (no per-frame allocations).
    private ContactFilter2D targetFilter;
    private readonly List<Collider2D> overlapResults = new List<Collider2D>();

    private static readonly int IdleHash = Animator.StringToHash("EnemyIdle");
    private static readonly int WalkHash = Animator.StringToHash("EnemyWalk");
    private static readonly int AttackHash = Animator.StringToHash("EnemyAttack");
    private int currentStateHash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        targetFilter = new ContactFilter2D();
        targetFilter.SetLayerMask(targetMask);
        targetFilter.useTriggers = false;
    }

    private void FixedUpdate()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.fixedDeltaTime;

        // Once an attack has started it runs to completion on the same target.
        if (state == State.Attack)
        {
            TickAttack();
            return;
        }

        target = FindClosestVisibleTarget();

        if (target == null)
        {
            EnterIdle();
            return;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance <= attackRange && cooldownTimer <= 0f)
            BeginAttack();
        else if (distance > attackRange)
            Chase();
        else
            EnterIdle();
    }

    // Picks the nearest target within sight range that isn't blocked by an obstacle.
    private MonoBehaviour FindClosestVisibleTarget()
    {
        Physics2D.OverlapCircle(transform.position, sightRange, targetFilter, overlapResults);

        MonoBehaviour closest = null;
        float closestSqr = float.MaxValue;

        foreach (Collider2D col in overlapResults)
        {
            IKnockbackTarget candidate = col.GetComponent<IKnockbackTarget>();
            if (candidate == null)
                continue;

            Vector2 candidatePos = col.transform.position;
            if (!HasLineOfSight(candidatePos))
                continue;

            float sqr = ((Vector2)transform.position - candidatePos).sqrMagnitude;
            if (sqr < closestSqr)
            {
                closestSqr = sqr;
                closest = candidate as MonoBehaviour;
            }
        }

        return closest;
    }

    private bool HasLineOfSight(Vector2 targetPos)
    {
        // Blocked if an obstacle sits between us and the target.
        return Physics2D.Linecast(transform.position, targetPos, obstacleMask).collider == null;
    }

    private void Chase()
    {
        state = State.Chase;
        Play(WalkHash);

        float dirX = Mathf.Sign(target.transform.position.x - transform.position.x);
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
        FaceDirection(Mathf.Sign(target.transform.position.x - transform.position.x));
    }

    private void TickAttack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        attackTimer += Time.fixedDeltaTime;

        if (!knockbackApplied && attackTimer >= attackWindup)
        {
            knockbackApplied = true;
            ShoveTarget();
        }

        if (attackTimer >= attackDuration)
        {
            cooldownTimer = attackCooldown;
            state = State.Idle; // Re-evaluated next FixedUpdate.
        }
    }

    private void ShoveTarget()
    {
        if (target == null)
            return;

        float dirX = Mathf.Sign(target.transform.position.x - transform.position.x);
        ((IKnockbackTarget)target).ApplyKnockback(
            new Vector2(dirX * knockbackForce, knockbackUp), knockbackLockout);
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
