using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private DamageData damageData;
    private GameObject owner;
    private Rigidbody rb;
    private Collider projectileCollider;
    private TrailRenderer trail;

    private float stickDuration = 5f;
    private float knockbackForce = 3f;
    private bool hasHit = false;

    private float originalTrailTime;
    private Vector3 velocityAtHit;

    // Audio
    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip hitSound;
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponentInChildren<Collider>();
        trail = GetComponent<TrailRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (rb == null)
            Debug.LogWarning("Rigidbody missing on projectile.");

        if (projectileCollider == null)
            Debug.LogWarning("Collider missing on projectile.");

        if (audioSource == null)
            Debug.LogWarning("AudioSource missing on projectile.");

        if (trail != null)
            originalTrailTime = trail.time;
    }

    public void Initialize(DamageData data, GameObject source, Vector3 velocity, IRangedItem rangedItem)
    {
        damageData = data;
        owner = source;
        velocityAtHit = velocity;

        if (rb != null)
        {
            rb.useGravity = rangedItem.useGravity;
            rb.linearVelocity = velocity;
        }

        if (projectileCollider != null && owner != null)
        {
            Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();
            foreach (var col in ownerColliders)
            {
                Physics.IgnoreCollision(projectileCollider, col);
            }
        }

        stickDuration = rangedItem.stickDuration;
        knockbackForce = rangedItem.knockbackForce;

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        Destroy(gameObject, rangedItem.maxLifetime);
    }

    private void Update()
    {
        if (!hasHit && rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
        else if (hasHit && trail != null)
        {
            trail.emitting = false;
            trail.time = Mathf.MoveTowards(trail.time, 0f, Time.deltaTime * (originalTrailTime / stickDuration));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (owner != null && (other.transform == owner.transform || other.transform.IsChildOf(owner.transform)))
            return;

        hasHit = true;

        if (projectileCollider != null)
            projectileCollider.enabled = false;

        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        GameObject target = other.transform.root.gameObject;
        if (target == null) return;

        // Check for health component before playing hit sound
        var health = target.GetComponent<Health>();
        if (health != null && audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        Combat ownerCombat = owner.GetComponent<Combat>();
        if (ownerCombat != null)
        {
            Vector3 knockbackDir = (target.transform.position - transform.position).normalized;
            ownerCombat.ApplyDamage(target, damageData, knockbackDir, knockbackForce);
        }
        else
        {
            Debug.LogWarning("Projectile's owner has no Combat component.");
        }

        StartCoroutine(StickAndDestroyDelayed(other));
    }


    private IEnumerator StickAndDestroyDelayed(Collider other)
    {
        yield return null;
        StickToTarget(other);
        Destroy(gameObject, stickDuration);
    }

    private void StickToTarget(Collider target)
    {
        hasHit = true;

        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.isKinematic = true;
        }

        transform.parent = target.transform;

        if (trail != null)
        {
            trail.emitting = false;
        }
    }
}
