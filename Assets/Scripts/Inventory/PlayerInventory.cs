using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StartingWeaponEntry
{
    public Item item;
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private readonly Dictionary<Slot, IEquippableSlotItem> equippedItems = new();

    [Header("Pickup Prefab")]
    [SerializeField] private GameObject weaponPickupPrefab;

    [Header("Starting Weapons or Abilities")]
    [SerializeField] private List<StartingWeaponEntry> startingWeapons;

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
                if (lastSelectedSlot == Slot.None)
                    SetLastSelectedSlot(equippable.Slot);
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
            return;

        if (currentItem != null)
            DropItem(currentItem);

        var itemInstance = Object.Instantiate(newItem as ScriptableObject) as IEquippableSlotItem;
        equippedItems[slot] = itemInstance;
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

    public Slot GetLastSelectedSlot() => lastSelectedSlot;

    public void SetLastSelectedSlot(Slot slot)
    {
        if (slot == lastSelectedSlot) return;

        lastSelectedSlot = slot;
        OverlayManager.Instance?.ShowActiveSlot(slot);
    }
}
