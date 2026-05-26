using UnityEngine;

public class FrequencyCaller : MonoBehaviour
{
    [SerializeField] private int frequency;

    public void Call()
    {
        if (FrequencyManager.Instance == null)
        {
            Debug.LogWarning("No FrequencyManager found in scene.");
            return;
        }

        FrequencyManager.Instance.CallFrequency(frequency);
    }

    public void CallFrequency(int frequencyToCall)
    {
        if (FrequencyManager.Instance == null)
        {
            Debug.LogWarning("No FrequencyManager found in scene.");
            return;
        }

        FrequencyManager.Instance.CallFrequency(frequencyToCall);
    }
}