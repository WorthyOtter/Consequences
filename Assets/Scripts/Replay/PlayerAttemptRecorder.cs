using UnityEngine;

public class PlayerAttemptRecorder : MonoBehaviour
{
    [Header("Recording")]
    [SerializeField] private float sampleRate = 30f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private AttemptData currentAttempt;
    private float timer;
    private float nextSampleTime;
    private bool isRecording;

    public AttemptData CurrentAttempt => currentAttempt;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void StartRecording()
    {
        currentAttempt = new AttemptData();
        timer = 0f;
        nextSampleTime = 0f;
        isRecording = true;
    }

    public AttemptData StopRecording()
    {
        isRecording = false;

        Debug.Log("Stopped recording. Frames recorded: " +
            (currentAttempt != null ? currentAttempt.frames.Count : 0));

        return currentAttempt;
    }

    private void Update()
    {
        if (!isRecording)
            return;

        timer += Time.deltaTime;

        if (timer >= nextSampleTime)
        {
            RecordFrame();
            nextSampleTime += 1f / sampleRate;
        }
    }

    private void RecordFrame()
    {
        bool flipX = spriteRenderer != null && spriteRenderer.flipX;

        int animHash = 0;
        float animTime = 0f;
        if (animator != null)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            animHash = info.fullPathHash;
            animTime = info.normalizedTime;
        }

        currentAttempt.frames.Add(new AttemptFrame(
            timer,
            transform.position,
            flipX,
            animHash,
            animTime
        ));
    }
}