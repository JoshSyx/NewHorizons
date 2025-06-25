using UnityEngine;

public class Combat : MonoBehaviour
{
    public void ManageHit(GameObject target, DamageData data)
    {
        if (!target.TryGetComponent(out Health targetHealth)) return;
        targetHealth.InflictDamage(data);
    }
}
