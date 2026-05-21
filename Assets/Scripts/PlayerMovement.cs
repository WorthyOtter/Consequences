using System.Collections;
using UnityEngine;

/*
    NOTE: I'm reusing this script from another project to save time. Slopes are "implemented," 
    but were never tested so no idea if that functions.
*/



[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour, IKnockbackTarget
{
    [Header("References")]
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    public PlayerInputHandler input;
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

    [Header("Crouch")]
    public float crouchSpeedMultiplier = 0.4f;

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

    private bool isGrounded;
    private bool isOnSlope;
    private bool canWalkOnSlope;
    private bool isJumping;
    private bool canJump;
    private bool facingRight = true;
    private bool isCrouching;

    private float slopeDownAngle;
    private float slopeSideAngle;
    private float lastSlopeAngle;

    private Vector2 slopeNormalPerp;

    [Header("Footsteps")]
    [SerializeField] private float footstepInterval = 0.4f;
    [SerializeField] private float minMoveSpeed = 0.2f;

    private float footstepTimer;
    private float timeSinceGrounded = 0;

    private float knockbackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        if (noFriction != null)
            playerCollider.sharedMaterial = noFriction;

    }

    private void Update()
    {
        CheckGround();

        if (!isGrounded)
            isCrouching = false;
        else if (input.CrouchPressed)
            isCrouching = !isCrouching;

        if (input.JumpPressed && canJump && !isCrouching)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        SlopeCheck();
        HandleMovement();
        HandleGravity();
        ClampSlopePop();
        KillFun();
    }

    private void KillFun() // kills funny coyote jumps
    {
        if (!isGrounded)
        {
            timeSinceGrounded += Time.deltaTime;
        }
        if (!isGrounded && timeSinceGrounded >= 0.15f)
        {
            canJump = false;
        }
    }

    private void HandleMovement()
    {
        // While knocked back, ignore input so the hit actually moves the player.
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        float inputX = input.MoveInput.x;
        float speed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        float targetSpeed = inputX * speed;

        float accelRate;
        if (isGrounded)
        {
            accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        }
        else
            accelRate = Mathf.Abs(targetSpeed) > 0.01f ? airAcceleration : airDeceleration;

        // On slopes, do not use acceleration buildup.
        // Just use direct horizontal movement and let the collider shape handle the climb.
        if (isGrounded && isOnSlope && canWalkOnSlope && !isJumping)
        {
            if (Mathf.Abs(inputX) < 0.01f)
            {
                // Let friction hold the player in place on the slope.
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0;
            }
            else
            {
                rb.linearVelocity = new Vector2(inputX * speed, rb.linearVelocity.y);
                rb.gravityScale = 1;
            }
        }
        else
        {
            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            rb.gravityScale = 1;

        }

        if (inputX > 0)
            facingRight = true;
        else if (inputX < 0)
            facingRight = false;

        spriteRenderer.flipX = !facingRight;
        spriteAnimator.SetBool("IsMoving", Mathf.Abs(inputX) > 0.1f);
        spriteAnimator.SetBool("IsGrounded", isGrounded);
        spriteAnimator.SetBool("IsCrouching", isCrouching);

        HandleFootsteps();
    }

    private void HandleGravity()
    {
        if (rb.linearVelocity.y > 0.01f)
            rb.gravityScale = upGravityScale;
        else if (rb.linearVelocity.y < -0.01f)
            rb.gravityScale = downGravityScale;
    }

    private void ClampSlopePop()
    {
        // Prevent the little upward launch / hop caused by slope contact resolution.
        // Only applies while grounded on a walkable slope and not in an intentional jump rise.
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

    // Called by enemies to shove the player. Movement input is suspended for
    // lockoutDuration so the knockback velocity isn't immediately overwritten.
    public void ApplyKnockback(Vector2 velocity, float lockoutDuration)
    {
        rb.linearVelocity = velocity;
        knockbackTimer = lockoutDuration;
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (rb.linearVelocity.y <= 0f)
            isJumping = false;

        if (isGrounded && !isJumping && slopeDownAngle <= maxSlopeAngle)
        {
            timeSinceGrounded = 0f;
            canJump = true;
        }

    }

    private void SlopeCheck()
    {
        Vector2 checkPos = new Vector2(
            transform.position.x,
            transform.position.y - playerCollider.bounds.extents.y
        );

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, Vector2.right, slopeCheckDistance, groundLayer);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, Vector2.left, slopeCheckDistance, groundLayer);

        if (slopeHitFront)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else if (slopeHitBack)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0f;
            isOnSlope = false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, groundLayer);

        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != lastSlopeAngle)
                isOnSlope = true;

            lastSlopeAngle = slopeDownAngle;

            Debug.DrawRay(hit.point, slopeNormalPerp, Color.blue);
            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }
        else
        {
            slopeDownAngle = 0f;
        }

        canWalkOnSlope = slopeDownAngle <= maxSlopeAngle && slopeSideAngle <= maxSlopeAngle;

        if (playerCollider != null)
        {
            if (isGrounded &&
                isOnSlope &&
                canWalkOnSlope &&
                Mathf.Abs(input.MoveInput.x) < 0.01f &&
                Mathf.Abs(rb.linearVelocity.y) < 0.05f)
            {
                playerCollider.sharedMaterial = fullFriction;
            }
            else
            {
                playerCollider.sharedMaterial = noFriction;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Vector3 checkPos = new Vector3(
                transform.position.x,
                transform.position.y - col.bounds.extents.y,
                transform.position.z
            );

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(checkPos, checkPos + Vector3.right * slopeCheckDistance);
            Gizmos.DrawLine(checkPos, checkPos + Vector3.left * slopeCheckDistance);
            Gizmos.DrawLine(checkPos, checkPos + Vector3.down * slopeCheckDistance);
        }
    }

    private void HandleFootsteps()
    {
        if (!isGrounded || Mathf.Abs(rb.linearVelocity.x) < minMoveSpeed)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            //Audio Clip goes here
            footstepTimer = footstepInterval;
        }
    }
}