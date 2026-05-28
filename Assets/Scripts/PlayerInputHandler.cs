using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public InputAction MoveAction;
    public InputAction JumpAction;
    public InputAction InteractAction;
    public InputAction RetryAction;
    public InputAction CrouchAction;

    public bool JumpHeld => JumpAction.IsPressed();

    public Vector2 MoveInput { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool RetryPressed { get; private set; }
    public bool CrouchPressed { get; private set; }

    private void OnEnable()
    {
        MoveAction.Enable();
        JumpAction.Enable();
        InteractAction.Enable();
        RetryAction.Enable();
        CrouchAction.Enable();
    }

    private void OnDisable()
    {
        MoveAction.Disable();
        JumpAction.Disable();
        InteractAction.Disable();
        RetryAction.Disable();
        CrouchAction.Disable();
    }

    private void Update()
    {
        MoveInput = MoveAction.ReadValue<Vector2>();

        // These latch until PlayerMovement consumes them.
        // This prevents quick taps from being lost between Update and FixedUpdate.
        if (JumpAction.triggered)
            JumpPressed = true;

        if (InteractAction.triggered)
            InteractPressed = true;

        if (CrouchAction.triggered)
            CrouchPressed = true;

        // Leave retry as a normal one-frame input.
        RetryPressed = RetryAction.triggered;
    }

    public void ConsumeJumpPressed()
    {
        JumpPressed = false;
    }

    public void ConsumeInteractPressed()
    {
        InteractPressed = false;
    }

    public void ConsumeCrouchPressed()
    {
        CrouchPressed = false;
    }
}