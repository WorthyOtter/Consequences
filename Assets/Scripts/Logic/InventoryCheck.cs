using UnityEngine;
using UnityEngine.Events;

public class InventoryCheck : MonoBehaviour
{
    [Header("Inventory Requirement")]
    public string RequiredItem;
    public bool GlobalInventory = false;

    [Header("Events")]
    public UnityEvent TriggerEvent;
    public UnityEvent FailEvent;

    [Header("Frequencies")]
    [Tooltip("Called when the inventory check succeeds. Use -1 to disable.")]
    [SerializeField] private int successFrequency = -1;

    [Tooltip("Called when the inventory check fails. Use -1 to disable.")]
    [SerializeField] private int failFrequency = -1;

    [Header("Settings")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered && triggerOnlyOnce)
            return;

        Inventory inv = collision.GetComponentInParent<Inventory>();

        if (inv == null)
            return;

        if (inv.CheckInventory(RequiredItem, GlobalInventory))
        {
            TriggerSuccess();
        }
        else
        {
            TriggerFail();
        }
    }

    private void TriggerSuccess()
    {
        TriggerEvent?.Invoke();
        CallFrequency(successFrequency);

        if (triggerOnlyOnce)
            triggered = true;
    }

    private void TriggerFail()
    {
        FailEvent?.Invoke();
        CallFrequency(failFrequency);
    }

    private void CallFrequency(int frequency)
    {
        if (frequency < 0)
            return;

        if (FrequencyManager.Instance == null)
        {
            Debug.LogWarning("No FrequencyManager found in scene.");
            return;
        }

        FrequencyManager.Instance.CallFrequency(frequency);
    }
}