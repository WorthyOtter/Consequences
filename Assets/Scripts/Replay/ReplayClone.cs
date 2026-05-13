using UnityEngine;

public class ReplayClone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Replay")]
    [SerializeField] private bool interpolateMovement = true;
    [SerializeField] private bool destroyWhenFinished = false;

    private AttemptData attemptData;
    private float replayTimer;
    private int frameIndex;
    private bool isReplaying;

    private Rigidbody2D rb;
    private Vector2 targetPosition;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.fireEvents = false;

        SetupPhysics();
        targetPosition = transform.position;
    }

    private void SetupPhysics()
    {
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.useFullKinematicContacts = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CapsuleCollider2D col = gameObject.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.163f, 0.521f);
        col.offset = new Vector2(-0.008f, -0.004f);
        col.usedByEffector = true;

        PlatformEffector2D effector = gameObject.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.rotationalOffset = 0f;
        effector.surfaceArc = 160f;
        effector.useSideFriction = false;
        effector.useSideBounce = false;
    }

    public void BeginReplay(AttemptData data)
    {
        attemptData = data;
        replayTimer = 0f;
        frameIndex = 0;

        if (attemptData == null || attemptData.frames == null || attemptData.frames.Count == 0)
        {
            Debug.LogWarning("ReplayClone was given no valid attempt data.");
            isReplaying = false;
            return;
        }

        isReplaying = true;

        // Snap directly before first FixedUpdate runs.
        targetPosition = attemptData.frames[0].position;
        if (rb != null)
            rb.position = targetPosition;
        else
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);

        ApplyVisuals(attemptData.frames[0]);

        Debug.Log("Clone replay started. Frames: " + attemptData.frames.Count);
    }

    private void Update()
    {
        if (!isReplaying)
            return;

        if (attemptData == null || attemptData.frames == null || attemptData.frames.Count == 0)
        {
            isReplaying = false;
            return;
        }

        // If there is only one frame, just stay there.
        if (attemptData.frames.Count == 1)
        {
            ApplyFrame(attemptData.frames[0]);
            isReplaying = false;
            return;
        }

        replayTimer += Time.deltaTime;

        if (replayTimer >= attemptData.Duration)
        {
            ApplyFrame(attemptData.frames[attemptData.frames.Count - 1]);
            FinishReplay();
            return;
        }

        ReplayMovement();
    }

    private void ReplayMovement()
    {
        int lastFrameIndex = attemptData.frames.Count - 1;

        while (frameIndex < lastFrameIndex - 1 &&
               attemptData.frames[frameIndex + 1].time <= replayTimer)
        {
            frameIndex++;
        }

        int nextFrameIndex = Mathf.Min(frameIndex + 1, lastFrameIndex);

        AttemptFrame currentFrame = attemptData.frames[frameIndex];
        AttemptFrame nextFrame = attemptData.frames[nextFrameIndex];

        if (interpolateMovement && nextFrameIndex != frameIndex)
        {
            float frameDuration = nextFrame.time - currentFrame.time;

            float t = frameDuration > 0f
                ? (replayTimer - currentFrame.time) / frameDuration
                : 0f;

            t = Mathf.Clamp01(t);

            Vector2 lerpedPosition = Vector2.Lerp(
                currentFrame.position,
                nextFrame.position,
                t
            );

            targetPosition = lerpedPosition;
        }
        else
        {
            ApplyFrame(currentFrame);
        }

        ApplyVisuals(currentFrame);
    }

    private void FixedUpdate()
    {
        if (rb != null)
            rb.MovePosition(targetPosition);
    }

    private void ApplyFrame(AttemptFrame frame)
    {
        targetPosition = frame.position;
        ApplyVisuals(frame);
    }

    private void ApplyVisuals(AttemptFrame frame)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = frame.flipX;

        if (animator != null && frame.animStateHash != 0)
            animator.Play(frame.animStateHash, 0, frame.animNormalizedTime % 1f);
    }

    private void FinishReplay()
    {
        isReplaying = false;

        if (destroyWhenFinished)
            Destroy(gameObject);
    }
}