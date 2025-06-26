using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StartingWeaponEntry
{
    public Item item; // Can be WeaponItem or AbilityItem
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private readonly Dictionary<Slot, IEquippableSlotItem> equippedItems = new();

    [Header("Pickup Prefab")]
    [SerializeField] private GameObject weaponPickupPrefab;

    [Header("Starting Weapons or Abilities")]
    [SerializeField] private List<StartingWeaponEntry> startingWeapons;

    // Track the last selected slot
    private Slot lastSelectedSlot = Slot.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple PlayerInventory instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EquipStartingItems();
    }

    private void EquipStartingItems()
    {
        foreach (var entry in startingWeapons)
        {
            if (entry.item is IEquippableSlotItem equippable)
            {
                EquipItemToSlot(equippable, equippable.Slot);
                // Set the first equipped slot as the last selected by default
                if (lastSelectedSlot == Slot.None)
                    lastSelectedSlot = equippable.Slot;
            }
            else
            {
                Debug.LogWarning($"Starting item {entry.item?.itemName} is not an equippable slot item.");
            }
        }
    }

    public void EquipItemToSlot(IEquippableSlotItem newItem, Slot slot)
    {
        if (newItem == null) return;

        if (equippedItems.TryGetValue(slot, out var currentItem) && currentItem == newItem)
        {
            Debug.Log($"Item {newItem.ItemName} is already equipped in slot {slot}.");
            return;
        }

        if (currentItem != null)
        {
            DropItem(currentItem);
        }

        var itemInstance = Object.Instantiate(newItem as ScriptableObject) as IEquippableSlotItem;
        equippedItems[slot] = itemInstance;

        Debug.Log($"Equipped {itemInstance.ItemName} to {slot} slot.");
    }

    private void DropItem(IEquippableSlotItem item)
    {
        if (item == null || item.WorldModelPrefab == null)
        {
            Debug.LogWarning($"Item {item?.ItemName} is missing drop model.");
            return;
        }

        Vector3 dropPos = transform.position + transform.forward + Vector3.up * 0.5f;
        GameObject dropped = Instantiate(item.WorldModelPrefab, dropPos, Quaternion.identity);

        if (dropped.TryGetComponent<ItemPickup>(out var pickup))
        {
            pickup.SetItem(item as Item);
        }
        else
        {
            Debug.LogWarning("Dropped item missing ItemPickup component.");
        }
    }

    public IEquippableSlotItem GetEquippedItem(Slot slot)
    {
        equippedItems.TryGetValue(slot, out var item);
        return item;
    }

    public void UnequipItem(Slot slot)
    {
        if (equippedItems.Remove(slot))
        {
            Debug.Log($"Unequipped item from {slot} slot.");
        }
        else
        {
            Debug.LogWarning($"No item found in {slot} slot to unequip.");
        }
    }

    // NEW: Get the last selected slot
    public Slot GetLastSelectedSlot() => lastSelectedSlot;

    // NEW: Set the last selected slot
    public void SetLastSelectedSlot(Slot slot)
    {
        if (slot == lastSelectedSlot) return;
        lastSelectedSlot = slot;
        Debug.Log($"Last selected slot set to {slot}");
    }
}
