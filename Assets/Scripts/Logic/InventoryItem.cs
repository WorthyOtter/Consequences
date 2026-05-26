using UnityEngine;

public class InventoryItem : MonoBehaviour
{
    public string InventoryItemName;
    public bool GlobalInventory = false;
    void Awake()
    {
        if (InventoryItemName == null || InventoryItemName == "")
        {
            Debug.LogError("Unassigned item name: " + gameObject);
        }
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        Inventory inv = collision.GetComponent<Inventory>();

        if (inv == null) return;
        if (inv.CheckInventory(InventoryItemName)) return;

        inv.AddItem(InventoryItemName, GlobalInventory);
        Destroy(gameObject);
    }
}
