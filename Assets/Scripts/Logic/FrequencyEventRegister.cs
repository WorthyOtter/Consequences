using UnityEngine;
using UnityEngine.Events;

public class FrequencyEventRegister : MonoBehaviour
{
    [SerializeField] private int frequency;
    [SerializeField] private UnityEvent eventToRegister;

    private void OnEnable()
    {
        if (FrequencyManager.Instance == null)
        {
            Debug.LogWarning("No FrequencyManager found in scene.");
            return;
        }

        FrequencyManager.Instance.AddListenerToFrequency(frequency, InvokeRegisteredEvent);
    }

    private void OnDisable()
    {
        if (FrequencyManager.Instance == null)
            return;

        FrequencyManager.Instance.RemoveListenerFromFrequency(frequency, InvokeRegisteredEvent);
    }

    private void InvokeRegisteredEvent()
    {
        eventToRegister?.Invoke();
    }
}