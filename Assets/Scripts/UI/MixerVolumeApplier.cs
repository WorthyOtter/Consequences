using UnityEngine;
using UnityEngine.Audio;

public class MixerVolumeApplier : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string mixerParam = StartScreenController.MixerParam;
    [SerializeField] private string prefKey = StartScreenController.VolumePrefKey;

    private void Awake()
    {
        if (mixer == null) return;
        float linear = PlayerPrefs.GetFloat(prefKey, StartScreenController.DefaultVolume);
        float dB = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        mixer.SetFloat(mixerParam, dB);
    }
}
