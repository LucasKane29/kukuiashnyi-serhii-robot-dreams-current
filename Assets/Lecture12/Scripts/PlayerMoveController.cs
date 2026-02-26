using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveController : MonoBehaviour
{
    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private float speed = 5f, rotationSpeed = 5f;

    private InputAction moveAction, lookAction;

    private float rotationY;

    void Start()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
    }

    void LateUpdate()
    {
        Vector2 movementVector = moveAction.ReadValue<Vector2>();
        this.Move(movementVector);

        Vector2 lookVector = lookAction.ReadValue<Vector2>();
        this.Look(lookVector);
    }

    private void Move(Vector2 movementVector)
    {
        Vector3 move = transform.forward * movementVector.y + transform.right * movementVector.x;
        move = move * speed * Time.deltaTime;
        characterController.Move(move);
    }

    private void Look(Vector2 lookVector)
    {
        rotationY += lookVector.x * rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !focus;
    }
}
