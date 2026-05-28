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
    private bool queuedCrouchedPressed;

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
        queuedCrouchedPressed = false;
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
        // Read the raw InputActions here instead of the latched properties,
        // because PlayerMovement may consume the latched values in FixedUpdate.
        if (input.JumpAction.triggered)
            queuedJumpPressed = true;

        if (input.InteractAction.triggered)
            queuedInteractPressed = true;

        if (input.CrouchAction.triggered)
            queuedCrouchedPressed = true;
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
            queuedInteractPressed,
            queuedCrouchedPressed
        ));

        queuedJumpPressed = false;
        queuedInteractPressed = false;
        queuedCrouchedPressed = false;

        tick++;
    }
}