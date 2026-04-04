using UnityEngine;

public class LaserBolt : MonoBehaviour
{
    [SerializeField] private float speed = 80f;
    [SerializeField] private float radius = 0.01f;

    private float _range;
    private float distanceTraveled;

    private LayerMask _targetLayer;
    private float _hitDamage;
    private float _hitForce;
    private Vector3 _damagerPosition;
    private Vector3? _targetPosition = null;
    private Vector3 _forwardDirection;

    public void Initialize(float range, LayerMask targetLayer, float hitDamage, float hitForce, Vector3 damagerPosition, Vector3? targetPosition = null)
    {
        _range = range;
        _targetLayer = targetLayer;
        _hitDamage = hitDamage;
        _hitForce = hitForce;
        _damagerPosition = damagerPosition;
        if (targetPosition != null)
        {
            _targetPosition = (targetPosition.Value - damagerPosition).normalized;
        }

    }

    void FixedUpdate()
    {
        float step = speed * Time.fixedDeltaTime;
        if (_targetPosition != null)
        {
            transform.Translate(_targetPosition.Value * step, Space.World);
            _forwardDirection = _targetPosition.Value;
        }
        else
        {
            transform.Translate(Vector3.forward * step);
            _forwardDirection = this.transform.forward;
        }

        distanceTraveled += step;

        if (Physics.SphereCast(this.transform.position, radius, _forwardDirection, out RaycastHit hit, step, _targetLayer))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                Vector3 aimedPosition = hit.transform.position;
                Rigidbody hitRigidbody = hit.rigidbody;

                if (hitRigidbody != null)
                {
                    Vector3 forceDirection = (hitRigidbody.position - hit.point).normalized;
                    hitRigidbody.AddForceAtPosition(forceDirection * _hitForce, hit.point, ForceMode.Impulse);
                }

                damageable.TakeDamage(_hitDamage, hit.point, _damagerPosition);
            }
            Destroy(gameObject);
        }

        if (distanceTraveled >= _range)
            Destroy(gameObject);
    }

    public float GetSpeed() => speed;
}
