using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<string> InventoryObjects;
    public static List<string> GlobalInventory;

    public void AddItem(string item, bool global = false)
    {
        if (!global)
        {
            if (CheckInventory(item)) return;

            InventoryObjects.Add(item);
        }
        else
        {
            if (CheckInventory(item, true)) return;

            GlobalInventory.Add(item);
        }
    }

    public void RemoveItem(string item, bool global = false)
    {
        if (!global)
        {
            if (!CheckInventory(item)) return;

            InventoryObjects.Remove(item);
        }
        else
        {
            if (!CheckInventory(item, true)) return;

            GlobalInventory.Remove(item);
        }
    }

    public bool CheckInventory(string item, bool global = false)
    {
        if (!global)
            return InventoryObjects.Contains(item);
        else
            return GlobalInventory.Contains(item);
    }
}
