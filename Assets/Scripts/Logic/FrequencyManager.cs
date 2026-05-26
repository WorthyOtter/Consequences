using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FrequencyManager : MonoBehaviour
{
    public static FrequencyManager Instance { get; private set; }

    private readonly Dictionary<int, UnityEvent> frequencyEvents = new Dictionary<int, UnityEvent>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CallFrequency(int frequency)
    {
        if (!frequencyEvents.TryGetValue(frequency, out UnityEvent unityEvent))
            return;

        unityEvent.Invoke();
    }

    public void AddListenerToFrequency(int frequency, UnityAction action)
    {
        if (action == null)
            return;

        if (!frequencyEvents.TryGetValue(frequency, out UnityEvent unityEvent))
        {
            unityEvent = new UnityEvent();
            frequencyEvents.Add(frequency, unityEvent);
        }

        unityEvent.AddListener(action);
    }

    public void RemoveListenerFromFrequency(int frequency, UnityAction action)
    {
        if (action == null)
            return;

        if (!frequencyEvents.TryGetValue(frequency, out UnityEvent unityEvent))
            return;

        unityEvent.RemoveListener(action);
    }
}