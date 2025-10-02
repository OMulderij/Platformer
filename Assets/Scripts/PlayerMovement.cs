using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction lookAction;

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
    public float coyoteTime = 0.5f;
    public LayerMask groundMask;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 momentumVar;
    private float rotationX = 0;
    private CharacterController characterController;
    private Transform playerTransform;

    private bool canMove = true;
    private bool canJump = true;
    private float timeWhenLastGrounded = 0.0f;


    public void Start()
    {
        characterController = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        lookAction = InputSystem.actions.FindAction("Look");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Update()
    {
        CheckForMoveAndJump();
        CheckForMouseMovement();
    }

    private void CheckForMoveAndJump()
    {
        bool isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, characterController.height / 2, 0), 0.45f, groundMask);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);


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

        moveDirection = (forward * moveValue.y) + (right * moveValue.x);
        if (!isGrounded && moveAction.IsPressed())
        {
            momentumVar.x = Mathf.Lerp(momentumVar.x, moveDirection.x, 0.005f);
            momentumVar.z = Mathf.Lerp(momentumVar.z, moveDirection.z, 0.005f);
        }

        if (jumpAction.IsPressed() && canMove && canJump && Time.time - timeWhenLastGrounded < coyoteTime)
        {
            if (isGrounded)
            {
                canJump = false;
                moveDirection.y = jumpPower;
            }
            else
            {
                canJump = false;
                momentumVar.y = jumpPower;
            }
        }

        if (!isGrounded)
        {
            momentumVar.y -= gravity * Time.deltaTime;
            characterController.Move(momentumVar * Time.deltaTime);
        }
        else
        {
            canJump = true;
            timeWhenLastGrounded = Time.time;
            momentumVar = moveDirection;
            characterController.Move(moveDirection * Time.deltaTime);
        }
    }

    private void CheckForMouseMovement()
    {
        if (canMove)
        {
            Vector2 mouseMovement = lookAction.ReadValue<Vector2>() / 10;
            rotationX += -mouseMovement.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, mouseMovement.x * lookSpeed, 0);
        }
    }
    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ((characterController.collisionFlags & CollisionFlags.Sides) == 0)
        {
            return;
        }
        if (Vector3.Dot(hit.normal, momentumVar) >= 0)
        {
            return;
        }

        momentumVar -= hit.normal * Vector3.Dot(hit.normal, momentumVar);
    }
}