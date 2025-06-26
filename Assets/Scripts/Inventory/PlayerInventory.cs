using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StartingWeaponEntry
{
    public WeaponSlot slot;
    public WeaponItem weapon;
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private readonly Dictionary<WeaponSlot, WeaponItem> equippedWeapons = new();

    [Header("Pickup Prefab")]
    [SerializeField] private GameObject weaponPickupPrefab;  // assign your pickup prefab here in inspector

    [Header("Starting Weapons")]
    [SerializeField] private List<StartingWeaponEntry> startingWeapons;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple PlayerInventory instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EquipStartingWeapons();
    }

    private void EquipStartingWeapons()
    {
        foreach (var entry in startingWeapons)
        {
            if (entry.weapon != null)
            {
                EquipWeaponToSlot(entry.weapon, entry.slot);
            }
            else
            {
                Debug.LogWarning($"Starting weapon for slot {entry.slot} is null.");
            }
        }
    }

    public void EquipWeaponToSlot(WeaponItem newWeapon, WeaponSlot slot)
    {
        if (newWeapon == null) return;

        if (equippedWeapons.TryGetValue(slot, out WeaponItem currentWeapon) && currentWeapon == newWeapon)
        {
            Debug.Log($"Weapon {newWeapon.itemName} is already equipped in slot {slot}.");
            return;
        }

        if (currentWeapon != null)
        {
            DropWeapon(currentWeapon);
        }

        // Instantiate a unique copy
        WeaponItem weaponInstance = Instantiate(newWeapon);

        equippedWeapons[slot] = weaponInstance;
        Debug.Log($"Equipped {weaponInstance.itemName} to {slot} slot.");
    }

    private void DropWeapon(WeaponItem weaponToDrop)
    {
        if (weaponToDrop == null)
        {
            Debug.LogWarning("Trying to drop a null weapon.");
            return;
        }

        if (weaponToDrop.worldModelPrefab == null)
        {
            Debug.LogWarning($"Weapon {weaponToDrop.itemName} does not have a worldModelPrefab assigned!");
            return;
        }

        Vector3 dropPosition = transform.position + transform.forward + Vector3.up * 0.5f;

        GameObject droppedPickup = Instantiate(weaponToDrop.worldModelPrefab, dropPosition, Quaternion.identity);

        // Setup ItemPickup component on the instantiated prefab
        ItemPickup itemPickup = droppedPickup.GetComponent<ItemPickup>();
        if (itemPickup != null)
        {
            itemPickup.SetItem(weaponToDrop);
        }
        else
        {
            Debug.LogWarning("Dropped weapon prefab missing ItemPickup component.");
        }
    }

    public WeaponItem GetEquippedWeapon(WeaponSlot slot)
    {
        equippedWeapons.TryGetValue(slot, out var weapon);
        return weapon;
    }

    public void UnequipWeapon(WeaponSlot slot)
    {
        if (equippedWeapons.Remove(slot))
        {
            Debug.Log($"Unequipped weapon from {slot} slot.");
        }
        else
        {
            Debug.LogWarning($"No weapon found in {slot} slot to unequip.");
        }
    }
}
