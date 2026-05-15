using UnityEngine;

public class ReplayInputDriver : MonoBehaviour
{
    private InputAttemptData attemptData;
    private int tick;

    public float MoveX { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool IsFinished { get; private set; }

    public void BeginReplay(InputAttemptData data)
    {
        attemptData = data;
        tick = 0;

        MoveX = 0f;
        JumpPressed = false;
        JumpHeld = false;
        InteractPressed = false;

        IsFinished = attemptData == null ||
                     attemptData.frames == null ||
                     attemptData.frames.Count == 0;

        Debug.Log("Input replay started. Frames: " +
                  (attemptData != null && attemptData.frames != null
                      ? attemptData.frames.Count
                      : 0));
    }

    public void AdvanceTick()
    {
        JumpPressed = false;
        InteractPressed = false;

        if (IsFinished)
            return;

        if (tick >= attemptData.frames.Count)
        {
            MoveX = 0f;
            JumpHeld = false;
            IsFinished = true;
            return;
        }

        RecordedInputFrame frame = attemptData.frames[tick];

        MoveX = frame.moveX;
        JumpPressed = frame.jumpPressed;
        JumpHeld = frame.jumpHeld;
        InteractPressed = frame.interactPressed;

        tick++;
    }
}