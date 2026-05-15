using UnityEngine;

[System.Serializable]
public struct RecordedInputFrame
{
    public int tick;
    public float moveX;
    public bool jumpPressed;
    public bool jumpHeld;
    public bool interactPressed;

    public RecordedInputFrame(
        int tick,
        float moveX,
        bool jumpPressed,
        bool jumpHeld,
        bool interactPressed)
    {
        this.tick = tick;
        this.moveX = moveX;
        this.jumpPressed = jumpPressed;
        this.jumpHeld = jumpHeld;
        this.interactPressed = interactPressed;
    }
}