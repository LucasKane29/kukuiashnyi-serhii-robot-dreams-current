using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField]
    private Transform followTarget;

    [SerializeField]  
    private float followSpeed = 10f, rotationSpeed = 100f;

    [SerializeField]
    private float topClamp = 10f, bottomClamp = -45f;

    private InputAction lookAction;

    private float rotationX;

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Look");
    }

    private void LateUpdate()
    {
        if (followTarget != null)
        {
            this.Follow(followTarget);
        }
        Vector2 lookVector = lookAction.ReadValue<Vector2>();
        this.Look(lookVector);

    }
    private void Look(Vector2 lookVector)
    {
        rotationX -= lookVector.y * rotationSpeed * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, bottomClamp, topClamp);
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    private void Follow(Transform followTarget)
    {
        Vector3 targetPosition = followTarget.position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
