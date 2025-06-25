using UnityEngine;

public enum ItemType { Weapon, Consumable, Powerup }

public abstract class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    [Tooltip("Prefab to instantiate when dropped or shown in the world")]
    public GameObject worldModelPrefab;

    // Called when picked up by player
    public virtual void OnPickup(GameObject player) { }
}
