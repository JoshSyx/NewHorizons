using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private static ItemPickup currentPickup = null;

    public static bool PlayerIsInPickupRange => currentPickup != null;

    [SerializeField] private Item item;

    private GameObject _player;

    public void SetItem(Item newItem)
    {
        item = newItem;
        // Optional: update visuals to match new item here
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentPickup = this;
            _player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentPickup == this)
                currentPickup = null;
            _player = null;
        }
    }

    public static void PickupWeaponAtRange(WeaponSlot slot)
    {
        if (currentPickup == null) return;
        currentPickup.Pickup(slot);
    }

    private void Pickup(WeaponSlot slot)
    {
        if (_player == null || item == null) return;

        var inventory = PlayerInventory.Instance;
        if (inventory == null) return;

        if (item is WeaponItem weapon)
        {
            inventory.EquipWeaponToSlot(weapon, slot);
            Debug.Log($"Picked up weapon {weapon.itemName} and equipped to {slot} slot");
        }
        else
        {
            Debug.Log($"Picked up non-weapon item {item.itemName}");
            item.OnPickup(_player);
        }

        Destroy(gameObject);
        currentPickup = null;
    }
}
