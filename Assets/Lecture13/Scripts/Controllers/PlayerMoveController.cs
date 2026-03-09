using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lesson13
{
    public class PlayerMoveController : MonoBehaviour
    {
        [Header("Move Settings")]

        [SerializeField]
        private CharacterController characterController;

        [SerializeField]
        private float speed = 1f, rotationSpeed = 5f;

        [SerializeField]
        private float topClamp = 10f, bottomClamp = -45f;

        [SerializeField]
        private float lookRange = 100f;

        [SerializeField]
        private Transform followTarget, weaponTarget, playerCamera;

        [SerializeField]
        private string moveActionName = "Move", lookActionName = "Look";

        private InputAction moveAction, lookAction;
        private float yaw = 0f;
        private float pitch = 0f;
        private Vector2 movementVector;


        void Start()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            moveAction = InputSystem.actions.FindAction(moveActionName);
            lookAction = InputSystem.actions.FindAction(lookActionName);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            followTarget.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            weaponTarget.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void Update()
        {
            movementVector = moveAction.ReadValue<Vector2>();
            Vector2 lookVector = lookAction.ReadValue<Vector2>();

            yaw += lookVector.x * rotationSpeed * Time.deltaTime;

            pitch += lookVector.y * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, bottomClamp, topClamp);

        }

        private void FixedUpdate()
        {
            this.Move(movementVector);
            this.Look();
        }

        private void Move(Vector2 movementVector)
        {
            Vector3 move = (transform.forward * movementVector.y + transform.right * movementVector.x) * speed;
            characterController.SimpleMove(move);
        }

        private void Look()
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            followTarget.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            Vector3 aimPoint = playerCamera.position + playerCamera.forward * lookRange;

            if(Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, lookRange))
            {
                aimPoint = hit.point;
            }
            weaponTarget.transform.rotation = Quaternion.LookRotation((aimPoint - weaponTarget.position).normalized);
        }

        private void OnApplicationFocus(bool focus)
        {
            Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !focus;
        }
    }
}
