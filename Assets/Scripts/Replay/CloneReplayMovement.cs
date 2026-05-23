using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ReplayInputDriver))]
public class CloneReplayMovement : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;
    private Collider2D cloneCollider;
    private ReplayInputDriver replayInput;

    public Animator spriteAnimator;
    public SpriteRenderer spriteRenderer;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 60f;
    public float deceleration = 90f;
    public float airAcceleration = 25f;
    public float airDeceleration = 15f;

    [Header("Gravity")]
    public float upGravityScale = 1f;
    public float downGravityScale = 1.5f;

    [Header("Jump")]
    public float jumpForce = 10f;

    [Header("Ground Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Slope Checks")]
    public float slopeCheckDistance = 0.4f;
    public float maxSlopeAngle = 50f;

    [Header("Physics Materials")]
    public PhysicsMaterial2D noFriction;
    public PhysicsMaterial2D fullFriction;
    public PhysicsMaterial2D deadFriction;

    [Header("Spawn Collision Safety")]
    [Tooltip("Layers the clone should temporarily ignore while spawning. Usually Player + Clone.")]
    [SerializeField] private LayerMask spawnOverlapIgnoreLayers;

    [Tooltip("How many colliders can be checked when the clone spawns.")]
    [SerializeField] private int spawnOverlapBufferSize = 16;

    private readonly List<Collider2D> temporarilyIgnoredColliders = new List<Collider2D>();
    private Collider2D[] overlapBuffer;

    private bool isGrounded;
    private bool isOnSlope;
    private bool canWalkOnSlope;
    private bool isJumping;
    private bool canJump;

    private float slopeDownAngle;
    private float lastSlopeAngle;

    private Vector2 slopeNormalPerp;

    private float timeSinceGrounded = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cloneCollider = GetComponent<Collider2D>();
        replayInput = GetComponent<ReplayInputDriver>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (noFriction != null)
            cloneCollider.sharedMaterial = noFriction;

        overlapBuffer = new Collider2D[spawnOverlapBufferSize];
    }

    private void Start()
    {
        IgnoreSpawnOverlaps();
    }

    private void FixedUpdate()
    {
        replayInput.AdvanceTick();

        if (canMove) CheckGround();

        if (replayInput.JumpPressed && canJump && canMove)
        {
            Jump();
        }

        if (canMove) SlopeCheck();
        if (canMove) HandleMovement();
        HandleGravity();
        if (canMove) ClampSlopePop();
        KillFun();

        UpdateSpawnCollisionSafety();
    }

    private void IgnoreSpawnOverlaps()
    {
        temporarilyIgnoredColliders.Clear();

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(spawnOverlapIgnoreLayers);
        filter.useLayerMask = true;
        filter.useTriggers = false;

        int hitCount = cloneCollider.Overlap(filter, overlapBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D other = overlapBuffer[i];

            if (other == null)
                continue;

            if (other == cloneCollider)
                continue;

            Physics2D.IgnoreCollision(cloneCollider, other, true);
            temporarilyIgnoredColliders.Add(other);
        }
    }

    private void UpdateSpawnCollisionSafety()
    {
        for (int i = temporarilyIgnoredColliders.Count - 1; i >= 0; i--)
        {
            Collider2D other = temporarilyIgnoredColliders[i];

            if (other == null)
            {
                temporarilyIgnoredColliders.RemoveAt(i);
                continue;
            }

            ColliderDistance2D distance = cloneCollider.Distance(other);

            if (!distance.isOverlapped)
            {
                Physics2D.IgnoreCollision(cloneCollider, other, false);
                temporarilyIgnoredColliders.RemoveAt(i);
            }
        }
    }

    private void HandleMovement()
    {
        float inputX = replayInput.MoveX;
        float targetSpeed = inputX * moveSpeed;

        float accelRate;

        if (isGrounded)
        {
            accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        }
        else
        {
            accelRate = Mathf.Abs(targetSpeed) > 0.01f ? airAcceleration : airDeceleration;
        }

        if (isGrounded && isOnSlope && canWalkOnSlope && !isJumping)
        {
            if (Mathf.Abs(inputX) < 0.01f)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
            else
            {
                rb.linearVelocity = new Vector2(inputX * moveSpeed, rb.linearVelocity.y);
                rb.gravityScale = 1f;
            }
        }
        else
        {
            float newX = Mathf.MoveTowards(
                rb.linearVelocity.x,
                targetSpeed,
                accelRate * Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            rb.gravityScale = 1f;
        }

        if (spriteRenderer != null)
        {
            if (inputX > 0f)
                spriteRenderer.flipX = false;
            else if (inputX < 0f)
                spriteRenderer.flipX = true;
        }
    }

    private void HandleGravity()
    {
        if (rb.linearVelocity.y > 0.01f)
        {
            rb.gravityScale = upGravityScale;
        }
        else if (rb.linearVelocity.y < -0.01f)
        {
            rb.gravityScale = downGravityScale;
        }
    }

    private void ClampSlopePop()
    {
        if (isGrounded && isOnSlope && canWalkOnSlope && !isJumping && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    private void Jump()
    {
        canJump = false;
        isJumping = true;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void CheckGround()
    {
        if (groundCheck == null)
            return;

        isGrounded = false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            // Do not count this clone's own collider as ground.
            if (hit == cloneCollider)
                continue;

            // Also ignore any child colliders belonging to this same clone object.
            if (hit.transform.IsChildOf(transform))
                continue;

            isGrounded = true;
            break;
        }

        if (rb.linearVelocity.y <= 0f)
            isJumping = false;

        if (isGrounded && !isJumping && slopeDownAngle <= maxSlopeAngle)
        {
            timeSinceGrounded = 0f;
            canJump = true;
        }
    }

    private void KillFun()
    {
        if (!isGrounded)
        {
            timeSinceGrounded += Time.fixedDeltaTime;
        }

        if (!isGrounded && timeSinceGrounded >= 0.15f)
        {
            canJump = false;
        }
    }

    private void SlopeCheck()
    {
        Vector2 checkPos = new Vector2(
            transform.position.x,
            transform.position.y - cloneCollider.bounds.extents.y
        );

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(
            checkPos,
            Vector2.right,
            slopeCheckDistance,
            groundLayer
        );

        RaycastHit2D slopeHitBack = Physics2D.Raycast(
            checkPos,
            Vector2.left,
            slopeCheckDistance,
            groundLayer
        );

        if (slopeHitFront)
        {
            isOnSlope = true;
        }
        else if (slopeHitBack)
        {
            isOnSlope = true;
        }
        else
        {
            isOnSlope = false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            checkPos,
            Vector2.down,
            slopeCheckDistance,
            groundLayer
        );

        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }

            lastSlopeAngle = slopeDownAngle;

            canWalkOnSlope = slopeDownAngle <= maxSlopeAngle;

            if (isOnSlope && canWalkOnSlope && Mathf.Abs(replayInput.MoveX) < 0.01f)
            {
                if (fullFriction != null)
                    cloneCollider.sharedMaterial = fullFriction;
            }
            else
            {
                if (noFriction != null)
                    cloneCollider.sharedMaterial = noFriction;
            }
        }
        else
        {
            canWalkOnSlope = false;

            if (noFriction != null)
                cloneCollider.sharedMaterial = noFriction;
        }
    }
    private bool canMove = true;
    public void CanMove(bool move)
    {
        canMove = move;
        if (!canMove)
        {
            cloneCollider.sharedMaterial = deadFriction;
        }
    }
}