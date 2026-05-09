using UnityEngine;

public class ReplayClone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Replay")]
    [SerializeField] private bool interpolateMovement = true;
    [SerializeField] private bool destroyWhenFinished = false;

    private AttemptData attemptData;
    private float replayTimer;
    private int frameIndex;
    private bool isReplaying;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

        ApplyFrame(attemptData.frames[0]);

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

            transform.position = new Vector3(
                lerpedPosition.x,
                lerpedPosition.y,
                transform.position.z
            );
        }
        else
        {
            ApplyFrame(currentFrame);
        }

        ApplyVisuals(currentFrame);
    }

    private void ApplyFrame(AttemptFrame frame)
    {
        transform.position = new Vector3(
            frame.position.x,
            frame.position.y,
            transform.position.z
        );

        ApplyVisuals(frame);
    }

    private void ApplyVisuals(AttemptFrame frame)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = frame.flipX;
    }

    private void FinishReplay()
    {
        isReplaying = false;

        if (destroyWhenFinished)
            Destroy(gameObject);
    }
}