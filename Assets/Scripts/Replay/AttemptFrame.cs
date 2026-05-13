using UnityEngine;

[System.Serializable]
public struct AttemptFrame
{
    public float time;
    public Vector2 position;
    public bool flipX;
    public int animStateHash;
    public float animNormalizedTime;

    public AttemptFrame(float time, Vector2 position, bool flipX, int animStateHash, float animNormalizedTime)
    {
        this.time = time;
        this.position = position;
        this.flipX = flipX;
        this.animStateHash = animStateHash;
        this.animNormalizedTime = animNormalizedTime;
    }
}