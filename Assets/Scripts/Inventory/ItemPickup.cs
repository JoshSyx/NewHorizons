using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public static bool PlayerIsInPickupRange => pickupsInRange.Count > 0;

    [SerializeField] private Item item;
    private static List<ItemPickup> pickupsInRange = new();
    private static GameObject playerRef;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!pickupsInRange.Contains(this))
                pickupsInRange.Add(this);
            playerRef = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickupsInRange.Remove(this);
            if (pickupsInRange.Count == 0)
                playerRef = null;
        }
    }

    public static ItemPickup GetClosestPickup()
    {
        if (playerRef == null) return null;

        pickupsInRange = pickupsInRange.Where(p => p != null).ToList();

        if (pickupsInRange.Count == 0) return null;

        return pickupsInRange.OrderBy(p => Vector3.Distance(p.transform.position, playerRef.transform.position)).FirstOrDefault();
    }

    public void SetItem(Item newItem)
    {
        item = newItem;
        // Optional: update visuals
    }

    public static void PickupWeaponAtRange()
    {
        var pickup = GetClosestPickup();
        pickup?.Pickup();
    }

    private void Pickup()
    {
        if (playerRef == null || item == null) return;

        var inventory = PlayerInventory.Instance;
        if (inventory == null) return;

        pickupsInRange.Remove(this);

        if (item is IEquippableSlotItem slotItem)
        {
            inventory.EquipItemToSlot(slotItem, slotItem.Slot);
            Debug.Log($"Picked up {item.itemName} and equipped to slot {slotItem.Slot}");
        }
        else
        {
            Debug.Log($"Picked up non-slot item {item.itemName}");
            item.OnPickup(playerRef);
        }

        Destroy(gameObject);
    }
}
