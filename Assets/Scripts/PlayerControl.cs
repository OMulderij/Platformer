using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class PlayerControl : MonoBehaviour
{
    public float maxHealth = 100.0f;
    public float currentHealth;
    public Action OnHealthChange;


    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction lookAction;

    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 10f;
    public float gravity = 15f;
    public float lookSpeed = 4f;
    public float lookXLimit = 75f;
    public float defaultHeight = 2f;
    public float bufferTime = 0.1f;
    public LayerMask groundMask;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 momentumVar;
    private float pitch = 0;
    private float yaw = 0;
    private CharacterController characterController;

    private bool canMove = true;
    private bool canJump = true;
    private float timeWhenLastGrounded = 0.0f;
    private float timeWhenLastJumpAction = 0.0f;
    private int manaConsumeMultiplier = 5;
    private bool isRunning = false;
    // private float timeSpentUsingAbility = 0.0f;


    public void Start()
    {
        currentHealth = maxHealth;

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
        CheckForMovement();
        CheckForMouseMovement();
    }

    private void CheckForMovement()
    {
        bool isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, characterController.height / 2, 0), 0.2f, groundMask);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);



        Vector2 moveValue = new Vector2(0, 0);
        isRunning = false;
        if (canMove && moveAction.IsPressed())
        {
            moveValue = moveAction.ReadValue<Vector2>();
            if (sprintAction.IsPressed() && currentHealth > 0)
            {
                isRunning = true;
                moveValue *= runSpeed;
            }
            else
            {
                moveValue *= walkSpeed;
            }
        }

        if (isRunning)
        {
            ChangeHealth(-Time.deltaTime * manaConsumeMultiplier);
        }

        moveDirection = (forward * moveValue.y) + (right * moveValue.x);

        CheckForMovingPlatforms();

        if (!isGrounded)
        {
            if (moveAction.IsPressed())
            {
                momentumVar.x = Mathf.Lerp(momentumVar.x, moveDirection.x, 2f * Time.deltaTime);
                momentumVar.z = Mathf.Lerp(momentumVar.z, moveDirection.z, 2f * Time.deltaTime);
            }
            else
            {
                momentumVar.x = Mathf.Lerp(momentumVar.x, 0f, 2f * Time.deltaTime);
                momentumVar.z = Mathf.Lerp(momentumVar.z, 0f, 2f * Time.deltaTime);
            }
        }

        if (jumpAction.IsPressed())
        {
            timeWhenLastJumpAction = Time.time;
        }

        if (canMove && canJump && Math.Abs(timeWhenLastJumpAction - timeWhenLastGrounded) < bufferTime)
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
            bool touchingCeiling = Physics.CheckSphere(transform.position + new Vector3(0, characterController.height / 2, 0), 0.2f, groundMask);
            if (touchingCeiling && momentumVar.y > 0)
            {
                momentumVar.y = 0;
            }
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


    private void CheckForMovingPlatforms()
    {
        Vector3 raycastDirection = new Vector3(0, -1, 0);
        if (!Physics.Raycast(transform.position, raycastDirection, out RaycastHit hit, characterController.height / 2 + 0.5f, groundMask))
        {
            return;
        }
        
        if (!hit.transform.GetComponent<VelocityCalculator>())
        {
            return;
        }

        characterController.Move(hit.transform.GetComponent<VelocityCalculator>().GetVelocity(this.transform));
    }
    private void CheckForMouseMovement()
    {
        if (canMove)
        {
            Vector2 mouseMovement = lookAction.ReadValue<Vector2>() * 10;
            yaw += mouseMovement.x * lookSpeed * Time.deltaTime;
            pitch -= mouseMovement.y * lookSpeed * Time.deltaTime;

            pitch = Mathf.Clamp(pitch, -lookXLimit, lookXLimit);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

            Vector3 offset = rotation * new Vector3(0f, 0f, -5.0f);
            playerCamera.transform.position = transform.position + offset;

            Vector3 camForward = playerCamera.transform.forward;
            camForward.y = 0f;

            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.deltaTime * 10f);

            Vector3 pointToLookAt = new Vector3(0, 0, 0);
            playerCamera.transform.LookAt(this.transform.position + pointToLookAt);
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


    public void ChangeHealth(float changeAmount)
    {
        currentHealth += changeAmount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        OnHealthChange?.Invoke();
    }

    public void HealToFull()
    {
        currentHealth = maxHealth;
        OnHealthChange?.Invoke();
    }
}