using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public InputAction MoveAction;
    public InputAction JumpAction;
    public InputAction InteractAction;
    public InputAction RetryAction;
    public bool JumpHeld => JumpAction.IsPressed();


    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool RetryPressed { get; private set; }

    private void OnEnable()
    {
        MoveAction.Enable();
        JumpAction.Enable();
        InteractAction.Enable();
        RetryAction.Enable();
    }

    private void OnDisable()
    {
        MoveAction.Disable();
        JumpAction.Disable();
        InteractAction.Disable();
        RetryAction.Disable();
    }

    private void Update()
    {
        MoveInput = MoveAction.ReadValue<Vector2>();
        JumpPressed = JumpAction.triggered;
        InteractPressed = InteractAction.triggered;
        RetryPressed = RetryAction.triggered;
    }

}