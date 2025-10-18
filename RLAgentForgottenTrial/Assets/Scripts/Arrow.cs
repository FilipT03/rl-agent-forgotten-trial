using System;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float ttl = 10f;
    [SerializeField] private float damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;
        transform.rotation = Quaternion.LookRotation(velocity);
        ttl -= Time.fixedDeltaTime;
        if (ttl < 0)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || other.isTrigger)
            return;
        rb.isKinematic = true;
        transform.SetParent(other.transform, true);
        Destroy(rb);

        // Check target
        if (other.CompareTag("Target"))
        {
            other.GetComponentInParent<Target>().OnHit();
            GetComponentInParent<TrainingReferences>().player.HitTarget(other.GetInstanceID());
        }
        else if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                if (enemy.IsDead())
                    GetComponentInParent<TrainingReferences>().player.KilledEnemy();
            }
        }

        this.enabled = false;
    }
}
