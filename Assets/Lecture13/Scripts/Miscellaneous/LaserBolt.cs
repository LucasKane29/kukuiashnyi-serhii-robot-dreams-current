using UnityEngine;

public class LaserBolt : MonoBehaviour
{
    [SerializeField] private float speed = 80f;

    private float travelDistance;
    private float distanceTraveled;
    private int targetLayer;
    private float hitDamage;
    private float hitForce;

    public void Initialize(float distance, int targetLayer, float hitDamage, float hitForce)
    {
        travelDistance = distance;
        this.targetLayer = targetLayer;
        this.hitDamage = hitDamage;
        this.hitForce = hitForce;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        transform.Translate(Vector3.forward * step);
        distanceTraveled += step;

        if (Physics.SphereCast(this.transform.position, 0.5f, this.transform.forward, out RaycastHit hit, 1.0f, targetLayer))
        {

            Vector3 forceDirection = (hit.rigidbody.position - hit.point).normalized;
            hit.rigidbody.AddForceAtPosition(forceDirection * hitForce, hit.point, ForceMode.Impulse);

            if (hit.collider.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(hitDamage);
        }

        if (distanceTraveled >= travelDistance)
            Destroy(gameObject);
    }

    public float GetSpeed() => speed;
}
