using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenController : MonoBehaviour
{
    public const string VolumePrefKey = "MasterVolume";
    public const string MixerParam = "MasterVolume";
    public const float DefaultVolume = 0.75f;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button startButton;
    [SerializeField] private string targetScene = "RewindDemo";

    private void Start()
    {
        float saved = PlayerPrefs.GetFloat(VolumePrefKey, DefaultVolume);
        volumeSlider.SetValueWithoutNotify(saved);
        ApplyVolume(saved);

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(VolumePrefKey, value);
        ApplyVolume(value);
    }

    private void OnStartClicked()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene(targetScene);
    }

    private void ApplyVolume(float linear)
    {
        if (mixer == null) return;
        float dB = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        mixer.SetFloat(MixerParam, dB);
    }
}
