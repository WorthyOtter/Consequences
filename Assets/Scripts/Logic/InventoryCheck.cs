using UnityEngine;
using UnityEngine.Events;

public class InventoryCheck : MonoBehaviour
{
    public UnityEvent TriggerEvent;
    public string RequiredItem;

    public bool GlobalInventory = false;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        Inventory inv = collision.GetComponent<Inventory>();

        if (inv == null || triggered) return;

        if (inv.CheckInventory(RequiredItem, GlobalInventory))
        {
            TriggerEvent?.Invoke();
            triggered = true;
        }

    }
}
