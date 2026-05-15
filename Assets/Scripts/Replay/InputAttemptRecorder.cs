using UnityEngine;

public class InputAttemptRecorder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;

    private InputAttemptData currentAttempt;
    private int tick;
    private bool isRecording;

    private bool queuedJumpPressed;
    private bool queuedInteractPressed;

    public InputAttemptData CurrentAttempt => currentAttempt;

    private void Awake()
    {
        if (input == null)
            input = GetComponent<PlayerInputHandler>();
    }

    public void StartRecording()
    {
        currentAttempt = new InputAttemptData();
        tick = 0;
        queuedJumpPressed = false;
        queuedInteractPressed = false;
        isRecording = true;
    }

    public InputAttemptData StopRecording()
    {
        isRecording = false;
        return currentAttempt;
    }

    private void Update()
    {
        if (!isRecording || input == null)
            return;

        // Cache button-down events in Update so quick taps are not missed.
        if (input.JumpPressed)
            queuedJumpPressed = true;

        if (input.InteractPressed)
            queuedInteractPressed = true;
    }

    private void FixedUpdate()
    {
        if (!isRecording || input == null)
            return;

        currentAttempt.frames.Add(new RecordedInputFrame(
            tick,
            input.MoveInput.x,
            queuedJumpPressed,
            input.JumpHeld,
            queuedInteractPressed
        ));

        queuedJumpPressed = false;
        queuedInteractPressed = false;

        tick++;
    }
}