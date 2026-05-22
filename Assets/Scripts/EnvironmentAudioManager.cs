using UnityEngine;
using UnityEngine.Audio;

/*
    Drives environmental audio: one looped ambient bed plus two one-shot tracks
    that each fire at an independent random interval. Volume is balanced by ear
    via the per-clip sliders below (no offline normalization).
*/

[DisallowMultipleComponent]
public class EnvironmentAudioManager : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup environmentGroup;

    [Header("Ambient Loop")]
    [SerializeField] private AudioClip ambientLoop;
    [SerializeField] [Range(0f, 1f)] private float ambientLoopVolume = 1f;

    [Header("Random One-Shots")]
    [SerializeField] private AudioClip randomClipA;
    [SerializeField] [Range(0f, 1f)] private float randomClipAVolume = 1f;
    [SerializeField] private AudioClip randomClipB;
    [SerializeField] [Range(0f, 1f)] private float randomClipBVolume = 1f;

    [Header("Random Timing (seconds)")]
    [SerializeField] private float minInterval = 15f;
    [SerializeField] private float maxInterval = 25f;

    private AudioSource loopSource;
    private AudioSource oneShotSource;
    private float timerA;
    private float timerB;

    private void Awake()
    {
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.clip = ambientLoop;
        loopSource.loop = true;
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = 0f;          // 2D, not positional
        loopSource.volume = ambientLoopVolume;
        loopSource.outputAudioMixerGroup = environmentGroup;

        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.playOnAwake = false;
        oneShotSource.spatialBlend = 0f;
        oneShotSource.outputAudioMixerGroup = environmentGroup;
    }

    private void Start()
    {
        if (ambientLoop != null) loopSource.Play();
        timerA = Random.Range(minInterval, maxInterval);
        timerB = Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        // Kept live so the loop slider can be balanced by ear while in Play mode.
        loopSource.volume = ambientLoopVolume;

        timerA = Tick(timerA, randomClipA, randomClipAVolume);
        timerB = Tick(timerB, randomClipB, randomClipBVolume);
    }

    // Counts a timer down; on reaching zero plays the clip and rolls a new interval.
    private float Tick(float timer, AudioClip clip, float volume)
    {
        if (clip == null) return timer;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            oneShotSource.PlayOneShot(clip, volume);
            timer = Random.Range(minInterval, maxInterval);
        }

        return timer;
    }
}
