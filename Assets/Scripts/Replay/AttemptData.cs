using System.Collections.Generic;

[System.Serializable]
public class AttemptData
{
    public List<AttemptFrame> frames = new List<AttemptFrame>();

    public float Duration
    {
        get
        {
            if (frames == null || frames.Count == 0)
                return 0f;

            return frames[frames.Count - 1].time;
        }
    }
}