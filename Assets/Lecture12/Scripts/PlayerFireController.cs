using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFireController : MonoBehaviour
{
    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private float explosionRadius = 1f, explosionForce = 1000f;

    private LayerMask layerMask;

    private InputAction attackAction;

    void Start()
    {
        layerMask = LayerMask.GetMask("CanBeDamaged");
        attackAction = InputSystem.actions.FindAction("Attack");
    }

    void FixedUpdate()
    {
        RaycastHit hit;

        if (attackAction.ReadValue<float>() > 0 && firePoint != null)
        {
            hit = this.Fire(firePoint);
        }
    }

    private RaycastHit Fire(Transform firePoint)
    {
        RaycastHit hit;

        if (Physics.Raycast(firePoint.position, firePoint.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Collider[] colliders = Physics.OverlapSphere(hit.point, explosionRadius, layerMask);
            foreach (Collider collider in colliders)
            {
                if(collider.attachedRigidbody == null)
                {
                    continue;
                }

                Vector3 forceDirection = (collider.transform.position - hit.point).normalized;
                collider.attachedRigidbody.AddForce(forceDirection * explosionForce);
            }
        }
        else
        {
            Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * 1000, Color.white);
        }

        return hit;
    }
}
