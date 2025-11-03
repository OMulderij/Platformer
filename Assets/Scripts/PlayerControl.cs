using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;


[RequireComponent(typeof(CharacterController))]
public class PlayerControl : MonoBehaviour
{
    // Inputs
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction lookAction;
    private InputAction selectAction;
    private InputAction placeAction;

    // Camera
    public Camera playerCamera;
    public float lookSpeed = 4f;
    public float lookXLimit = 75f;
    private float pitch = 0;
    private float yaw = 0;
    private bool aiming = false;
    private float timeWhenChangedAimState = 0.0f;
    private float aimTransitionLength = 0.1f;
    private bool inAimingTransition = false;
    
    // Health
    public float maxHealth = 100.0f;
    public float currentHealth;
    public Action OnHealthChange;
    
    // Movement
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 momentumVar;
    public LayerMask groundMask;

    // CharacterController
    private CharacterController characterController;
    public float defaultHeight = 2f;
    private bool canMove = true;
    private bool hasJumped = false;
    private bool isRunning = false;
    private float timeWhenLastGrounded = 0.0f;
    private float timeWhenLastJumpAction = 0.0f;
    public float walkSpeed = 6f;
    public float jumpPower = 10f;
    public float gravity = 15f;
    public float bufferTime = 0.1f;
    
    // In scene objects
    private float trampolinePower = 15f;
    private float timeWhenLastOnTrampoline = 0.0f;

    // Abilities
    public float jumpDelay = 0.5f;
    public float placementCooldown = 1f;
    private const int manaConsumeMultiplier = 5;
    public float runSpeed = 12f;
    private bool doubleJumpAvailable = false;
    public float doubleJumpManaCost = 20f;
    public float platformManaCost = 40f;
    public GameObject abilityPlatform;
    private bool placingPlatform = false;
    private float timeWhenLastPlacedPlatform;
    private GameObject platformToPlace;
    public float maxPlatformSpawnLength = 25f;


    public void Start()
    {
        currentHealth = maxHealth;

        characterController = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        lookAction = InputSystem.actions.FindAction("Look");
        selectAction = InputSystem.actions.FindAction("Select");
        placeAction = InputSystem.actions.FindAction("Place");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Update()
    {
        CheckForMovement();
        CheckForMouseMovement();
        CheckForMouseButton();
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

        bool jump = false;
        if (jumpAction.IsPressed())
        {
            timeWhenLastJumpAction = Time.time;

            if (doubleJumpAvailable && currentHealth >= doubleJumpManaCost && Time.time - timeWhenLastGrounded > jumpDelay)
            {
                jump = true;
                doubleJumpAvailable = false;
                ChangeHealth(-doubleJumpManaCost);
            }
        }

        if (!hasJumped && Math.Abs(timeWhenLastGrounded - timeWhenLastJumpAction) < bufferTime)
        {
            hasJumped = true;
            jump = true;
        }

        if (canMove && jump)
        {
            if (isGrounded)
            {

                moveDirection.y = jumpPower;
            }
            else
            {
                momentumVar.y = jumpPower;
            }

            if (Time.time - timeWhenLastOnTrampoline < bufferTime)
            {
                moveDirection.y += trampolinePower;
                momentumVar.y += trampolinePower;
            }
        }

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
            doubleJumpAvailable = true;
            hasJumped = false;
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

            Vector3 offset = new(0f, 0f, -5.0f);

            float nextToPlayer = 0f;
            Vector3 aimingOffset = new Vector3(1f, 0.5f, offset.z / 2);
            Vector3 pointToLookAt = new();
            if (inAimingTransition)
            {
                if (aiming)
                {
                    offset = Vector3.Lerp(offset, aimingOffset, (Time.time - timeWhenChangedAimState) / aimTransitionLength);
                    nextToPlayer = Mathf.Lerp(0f, 1f, (Time.time - timeWhenChangedAimState) / aimTransitionLength);
                }
                else
                {
                    offset = Vector3.Lerp(offset, aimingOffset, 1 - (Time.time - timeWhenChangedAimState) / aimTransitionLength);
                    nextToPlayer = Mathf.Lerp(0f, 1f, 1 - (Time.time - timeWhenChangedAimState) / aimTransitionLength);
                }
            }
            else if (aiming)
            {
                offset = aimingOffset;
                nextToPlayer = 1;
            }
            pointToLookAt.x = nextToPlayer;
            pointToLookAt = playerCamera.transform.rotation * pointToLookAt;
            pointToLookAt.y = 0.5f;

            offset = rotation * offset;

            playerCamera.transform.position = transform.position + offset;

            Vector3 camForward = playerCamera.transform.forward;
            camForward.y = 0f;

            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.deltaTime * 10f);

            playerCamera.transform.LookAt(this.transform.position + pointToLookAt);
        }
    }

    private void CheckForMouseButton()
    {
        if (Time.time - timeWhenChangedAimState < aimTransitionLength)
        {
            inAimingTransition = true;
        }
        else
        {
            inAimingTransition = false;
        }

        if (Time.time - timeWhenLastPlacedPlatform < placementCooldown)
        {
            aiming = false;
            return;
        }

        if (currentHealth < platformManaCost)
        {
            aiming = false;
            return;
        }

        if (selectAction.IsPressed())
        {
            if (!aiming)
            {
                aiming = true;
                timeWhenChangedAimState = Time.time;
            }
            
            if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxPlatformSpawnLength, groundMask))
            {
                Destroy(platformToPlace);
                placingPlatform = false;
                return;
            }

            if (!placingPlatform)
            {
                platformToPlace = Instantiate(abilityPlatform);
                platformToPlace.GetComponentInChildren<BoxCollider>().enabled = false;
                placingPlatform = true;
            }

            platformToPlace.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
        }
        else
        {
            if (aiming)
            {
                aiming = false;
                timeWhenChangedAimState = Time.time;
            }

            Destroy(platformToPlace);
            platformToPlace = null;
            placingPlatform = false;
        }

        if (placeAction.IsPressed() && placingPlatform)
        {
            ChangeHealth(-platformManaCost);
            inAimingTransition = true;
            timeWhenChangedAimState = Time.time;
            placingPlatform = false;

            platformToPlace.GetComponentInChildren<BoxCollider>().enabled = true;
            platformToPlace.GetComponent<AbilityPlatform>().StartTimer(this);

            foreach (Transform child in platformToPlace.transform)
            {
                child.gameObject.layer = 0;
            }

            timeWhenLastPlacedPlatform = Time.time;
            platformToPlace = null;
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

    public void TrampolineHit()
    {
        timeWhenLastOnTrampoline = Time.time;
    }

    public void ApplyForce(Vector3 direction)
    {
        characterController.Move(direction * Time.deltaTime);
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