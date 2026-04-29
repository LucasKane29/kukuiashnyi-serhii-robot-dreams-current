using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private string _explosionSoundName;

    private LayerMask targetLayer;
    private float damage;
    private Vector3 damagerPosition;
    private AudioManager _audioManager;

    public void Init(float damage, LayerMask targetLayer, Vector3 damagerPosition, AudioManager audioManager)
    {
        this.damage = damage;
        this.targetLayer = targetLayer;
        this.damagerPosition = damagerPosition;
        _audioManager = audioManager;
        StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(fuseTime);

        Vector3 explosionPoint = this.transform.position;

        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, explosionPoint, explosionEffect.transform.rotation);
            _audioManager.PlaySoundAtPosition(_explosionSoundName, explosionPoint);
            Destroy(effect, 1f);
        }

        HashSet<Rigidbody> affectedBodies = new HashSet<Rigidbody>();
        Collider[] colliders = Physics.OverlapSphere(explosionPoint, explosionRadius, targetLayer);
        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.attachedRigidbody;
            if (rb != null && affectedBodies.Add(rb))
            {
                collider.attachedRigidbody.AddExplosionForce(explosionForce, explosionPoint, explosionRadius);

                if (collider.TryGetComponent(out IDamageable damageable))
                    damageable.TakeDamage(damage, collider.ClosestPoint(explosionPoint), damagerPosition);
            }
        }

        Destroy(this.gameObject);
    }
}
