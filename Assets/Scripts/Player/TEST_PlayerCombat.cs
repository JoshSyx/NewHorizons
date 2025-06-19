using UnityEngine;

public class TEST_PlayerCombat : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Combat.ManageHit(other.gameObject, 100f);
        }
    }
}
