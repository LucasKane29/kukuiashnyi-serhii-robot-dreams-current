using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFireController : MonoBehaviour
{
    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private float explosionRadius = 1f, explosionForce = 100f;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private string attackActionName = "Attack";

    private InputAction attackAction;

    void Start()
    {
        attackAction = InputSystem.actions.FindAction(attackActionName);
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

        HashSet<Rigidbody> affectedBodies = new HashSet<Rigidbody>();

        if (Physics.Raycast(firePoint.position, firePoint.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Collider[] colliders = Physics.OverlapSphere(hit.point, explosionRadius, layerMask);

            foreach (Collider collider in colliders)
            {
                Rigidbody rb = collider.attachedRigidbody;
                if (rb != null && affectedBodies.Add(rb))
                {
                    Vector3 forceDirection = (rb.position - hit.point).normalized;
                    rb.AddForce(forceDirection * explosionForce);
                }
            }
            affectedBodies.Clear();
        }
        else
        {
            Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * 1000, Color.white);
        }
        
        return hit;
    }
}
