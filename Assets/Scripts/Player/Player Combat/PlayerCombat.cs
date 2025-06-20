using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Combat combat;
    [SerializeField] private float attack1Damage = 10f;
    [SerializeField] private float attack2Damage = 20f;
    [SerializeField] private float attack3Damage = 30f;
    [SerializeField] private float heightOffset = 1.8f;

    private void Awake()
    {
        if (combat == null)
            combat = GetComponent<Combat>();
    }

    public void DoAttack1()
    {
        GameObject enemy = DetectEnemy();
        if (enemy != null)
        {
            combat.ManageHit(enemy, attack1Damage);
            Debug.Log("Attack 1 executed");
        }
    }

    public void DoAttack2()
    {
        GameObject enemy = DetectEnemy();
        if (enemy != null)
        {
            combat.ManageHit(enemy, attack2Damage);
            Debug.Log("Attack 2 executed");
        }
    }

    public void DoAttack3()
    {
        GameObject enemy = DetectEnemy();
        if (enemy != null)
        {
            combat.ManageHit(enemy, attack3Damage);
            Debug.Log("Attack 3 executed");
        }
    }

    private GameObject DetectEnemy()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position + new Vector3(0, heightOffset, 0), transform.forward*10, Color.red, Mathf.Infinity);
        if (Physics.Raycast(transform.position + new Vector3(0, heightOffset, 0), transform.forward, out hit, 10f))
        {
            if (hit.collider.gameObject.CompareTag("Enemy"))
                return hit.collider.gameObject;
        }
        return null;
    }
}
