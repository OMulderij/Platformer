using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction lookAction;

    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 momentumVar;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        lookAction = InputSystem.actions.FindAction("Look");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float movementDirectionY = moveDirection.y;

        Vector2 moveValue = new Vector2(0, 0);

        if (canMove && moveAction.IsPressed())
        {
            moveValue = moveAction.ReadValue<Vector2>();
            if (sprintAction.IsPressed())
            {
                moveValue *= runSpeed;
            }
            else
            {
                moveValue *= walkSpeed;
            }
        }

        if (characterController.isGrounded)
        {
            moveDirection = (forward * moveValue.y) + (right * moveValue.x);
        }
        else
        {
            if (Math.Abs(momentumVar.x) < Math.Abs(moveDirection.x))
            {
                momentumVar.x += moveDirection.x / 3;
            }

            if (Math.Abs(momentumVar.z) < Math.Abs(moveDirection.z))
            {
                momentumVar.z += moveDirection.z / 3;
            }
            moveDirection.x = momentumVar.x;
            moveDirection.z = momentumVar.z;
            // moveDirection += ((forward * moveValue.y) + (right * moveValue.x)) * 3/1;
        }



        if (jumpAction.IsPressed() && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            momentumVar = moveDirection;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            Vector2 mouseMovement = lookAction.ReadValue<Vector2>() / 10;
            rotationX += -mouseMovement.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, mouseMovement.x * lookSpeed, 0);
        }
    }
    public void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if ((characterController.collisionFlags & CollisionFlags.Sides) == 0)
        {
            return;
        }
        if (Vector3.Dot(hit.normal, moveDirection) >= 0)
        {
            return;
        }

        momentumVar -= hit.normal * Vector3.Dot(hit.normal, moveDirection);
    }
}