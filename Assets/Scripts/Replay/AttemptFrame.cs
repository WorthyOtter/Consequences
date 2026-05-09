using UnityEngine;

[System.Serializable]
public struct AttemptFrame
{
    public float time;
    public Vector2 position;
    public bool flipX;

    public AttemptFrame(float time, Vector2 position, bool flipX)
    {
        this.time = time;
        this.position = position;
        this.flipX = flipX;
    }
}