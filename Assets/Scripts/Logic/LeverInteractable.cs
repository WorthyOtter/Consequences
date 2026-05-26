using UnityEngine;
using UnityEngine.Events;

public class LeverInteractable : MonoBehaviour, IInteractable
{
    [Header("State")]
    [SerializeField] private bool startsOn = false;
    public Sprite onSprite;
    public Sprite offSprite;

    [Header("Frequencies")]
    [SerializeField] private int toggleOnFrequency = -1;
    [SerializeField] private int toggleOffFrequency = -1;

    [Header("Local Events")]
    [SerializeField] private UnityEvent toggleOn;
    [SerializeField] private UnityEvent toggleOff;

    public bool IsOn { get; private set; }

    private void Awake()
    {
        IsOn = startsOn;
    }

    public void Interact(GameObject interactor)
    {
        Toggle();
    }

    public void Toggle()
    {
        IsOn = !IsOn;

        if (IsOn)
        {

            TriggerOn();
            GetComponent<SpriteRenderer>().sprite = onSprite;

        }
        else
        {
            TriggerOff();
            GetComponent<SpriteRenderer>().sprite = offSprite;

        }
    }

    public void SetOn(bool value)
    {
        if (IsOn == value)
            return;

        IsOn = value;

        if (IsOn)
            TriggerOn();
        else
            TriggerOff();
    }

    private void TriggerOn()
    {
        toggleOn?.Invoke();

        if (toggleOnFrequency >= 0 && FrequencyManager.Instance != null)
            FrequencyManager.Instance.CallFrequency(toggleOnFrequency);
    }

    private void TriggerOff()
    {
        toggleOff?.Invoke();

        if (toggleOffFrequency >= 0 && FrequencyManager.Instance != null)
            FrequencyManager.Instance.CallFrequency(toggleOffFrequency);
    }
}