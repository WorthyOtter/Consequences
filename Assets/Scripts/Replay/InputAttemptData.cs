using System.Collections.Generic;

[System.Serializable]
public class InputAttemptData
{
    public List<RecordedInputFrame> frames = new List<RecordedInputFrame>();

    public int DurationTicks
    {
        get
        {
            if (frames == null || frames.Count == 0)
                return 0;

            return frames[frames.Count - 1].tick;
        }
    }
}