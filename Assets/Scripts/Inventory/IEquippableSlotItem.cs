using UnityEngine;

public interface IEquippableSlotItem
{
    Slot Slot { get; }
    string ItemName { get; }
    GameObject WorldModelPrefab { get; }
}