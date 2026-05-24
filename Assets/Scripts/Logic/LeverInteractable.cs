using UnityEngine;
using UnityEngine.Events;

public class LeverInteractable : MonoBehaviour, IInteractable
{
    [Header("State")]
    [SerializeField] private bool startsOn = false;
    public Sprite onSprite;
    public Sprite offSprite;

    [Header("Events")]
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
            toggleOn?.Invoke();
            GetComponent<SpriteRenderer>().sprite = onSprite;
        }
        else
        {
            toggleOff?.Invoke();
            GetComponent<SpriteRenderer>().sprite = offSprite;
        }
    }

    public void SetOn(bool value)
    {
        if (IsOn == value)
            return;

        IsOn = value;

        if (IsOn)
            toggleOn?.Invoke();
        else
            toggleOff?.Invoke();
    }
}