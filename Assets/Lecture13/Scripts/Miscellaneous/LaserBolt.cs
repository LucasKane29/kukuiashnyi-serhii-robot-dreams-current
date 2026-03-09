using UnityEngine;

public class LaserBolt : MonoBehaviour
{
    [SerializeField] private float speed = 80f;
    [SerializeField] private float radius = 0.5f;

    private float range;
    private float distanceTraveled;

    private LayerMask targetLayer;
    private float hitDamage;
    private float hitForce;

    public void Initialize(float range, LayerMask targetLayer, float hitDamage, float hitForce)
    {
        this.range = range;
        this.targetLayer = targetLayer;
        this.hitDamage = hitDamage;
        this.hitForce = hitForce;
    }

    void FixedUpdate()
    {
        float step = speed * Time.fixedDeltaTime;

        transform.Translate(Vector3.forward * step);
        distanceTraveled += step;

        if (Physics.SphereCast(this.transform.position, radius, this.transform.forward, out RaycastHit hit, step, targetLayer))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                Vector3 forceDirection = (hit.rigidbody.position - hit.point).normalized;
                hit.rigidbody.AddForceAtPosition(forceDirection * hitForce, hit.point, ForceMode.Impulse);

                damageable.TakeDamage(hitDamage);
            }
            Destroy(gameObject);
        }

        if (distanceTraveled >= range)
            Destroy(gameObject);
    }

    public float GetSpeed() => speed;
}
