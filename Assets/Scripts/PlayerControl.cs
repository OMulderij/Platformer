using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;


[RequireComponent(typeof(CharacterController))]
public class PlayerControl : MonoBehaviour
{
    // Timer
    private float timeWhenStarted;
    private bool inMenu = false;
    // Inputs
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction lookAction;
    private InputAction selectAction;
    private InputAction placeAction;
    private InputAction resetAction;

    // Camera
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float lookSpeed = 4f;
    [SerializeField] private float lookXLimit = 75f;
    private float pitch = 0;
    private float yaw = 0;
    private bool aiming = false;
    private float timeWhenChangedAimState = 0.0f;
    private float aimTransitionLength = 0.1f;
    private bool inAimingTransition = false;
    
    // Mana
    public float maxMana = 100.0f;
    public float currentMana;
    public Action OnManaChange;
    
    // Movement
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 momentumVar;
    [SerializeField] private LayerMask groundMask;

    // CharacterController
    private CharacterController characterController;
    private bool canMove = true;
    private bool hasJumped = false;
    private float timeWhenLastGrounded = 0.0f;
    private float timeWhenLastJumpAction = 0.0f;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private float gravity = 15f;
    [SerializeField] private float bufferTime = 0.1f;
    
    // In scene objects
    private float trampolinePower = 15f;
    private float timeWhenLastOnTrampoline = 0.0f;

    // Abilities
    [SerializeField] private float jumpDelay = 0.5f;
    [SerializeField] private float placementCooldown = 1f;
    private const int manaConsumeMultiplier = 5;
    [SerializeField] private float runSpeed = 12f;
    private bool doubleJumpAvailable = false;
    [SerializeField] private float doubleJumpManaCost = 20f;
    [SerializeField] private float platformManaCost = 40f;
    [SerializeField] private GameObject abilityPlatform;
    private bool placingPlatform = false;
    private float timeWhenLastPlacedPlatform;
    private GameObject platformToPlace;
    [SerializeField] private float maxPlatformSpawnLength = 25f;

    // Particles & Animations
    [SerializeField] private ParticleSystem doubleJumpParticles;
    [SerializeField] private ParticleSystem sprintParticles;
    [SerializeField] private ParticleSystem windJumpParticles;
    [SerializeField] private GameObject wizardModel;
    [SerializeField] private Transform wizardHeadBone;
    private Animator wizardAnimator;


    public void Start()
    {
        currentMana = maxMana;

        characterController = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        lookAction = InputSystem.actions.FindAction("Look");
        selectAction = InputSystem.actions.FindAction("Select");
        placeAction = InputSystem.actions.FindAction("Place");
        resetAction = InputSystem.actions.FindAction("Reset");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        wizardAnimator = wizardModel.GetComponent<Animator>();
    }
    public void Awake()
    {
        timeWhenStarted = Time.time;
    }

    public void Update()
    {
        CheckForMovement();
        CheckForMouseMovement();
        CheckForMouseButton();

        if (resetAction.IsPressed() && !inMenu)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    private void CheckForMovement()
    {
        bool isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, characterController.height / 2, 0), 0.25f, groundMask);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector2 moveValue = new Vector2(0, 0);
        bool isSprinting = false;
        if (canMove && moveAction.IsPressed())
        {
            moveValue = moveAction.ReadValue<Vector2>();
            if (sprintAction.IsPressed() && currentMana > 0)
            {
                isSprinting = true;
                ParticleSystem.EmissionModule em = sprintParticles.emission;
                em.enabled = true;

                ChangeManaAmount(-Time.deltaTime * manaConsumeMultiplier);
                moveValue *= runSpeed;
            }
            else
            {
                moveValue *= walkSpeed;
            }
        }

        if (!isSprinting)
        {
            ParticleSystem.EmissionModule em = sprintParticles.emission;
            em.enabled = false;
        }

        moveDirection = (forward * moveValue.y) + (right * moveValue.x);

        CheckForMovingPlatforms();

        bool jump = false;
        if (jumpAction.IsPressed())
        {
            if (doubleJumpAvailable && currentMana >= doubleJumpManaCost && Time.time - timeWhenLastGrounded > jumpDelay)
            {
                jump = true;
                doubleJumpAvailable = false;
                doubleJumpParticles.Play();
                ChangeManaAmount(-doubleJumpManaCost);
            }
            timeWhenLastJumpAction = Time.time;
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
                windJumpParticles.Play();
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
            if (Physics.Raycast(transform.position, -playerCamera.transform.forward, out RaycastHit hit, 5f, groundMask))
            {
                offset.z = -Vector3.Distance(hit.point, transform.position);
            }

            float nextToPlayer = 0f;
            Vector3 aimingOffset = new Vector3(1f, 0.5f, offset.z / 2);
            Vector3 pointToLookAt = new();


            if (inAimingTransition)
            {
                float pointInTransition = (Time.time - timeWhenChangedAimState) / aimTransitionLength;
                if (aiming)
                {
                    offset = Vector3.Lerp(offset, aimingOffset, pointInTransition);
                    nextToPlayer = Mathf.Lerp(0f, 1f, pointInTransition);
                    playerCamera.fieldOfView = Mathf.Lerp(60f, 50f, pointInTransition);
                }
                else
                {
                    offset = Vector3.Lerp(offset, aimingOffset, 1 - pointInTransition);
                    nextToPlayer = Mathf.Lerp(0f, 1f, 1 - pointInTransition);
                    playerCamera.fieldOfView = Mathf.Lerp(50f, 60f, pointInTransition);
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
            wizardHeadBone.localRotation = Quaternion.Euler(0, 0, -pitch);
        }
    }

    private void CheckForMouseButton()
    {
        wizardAnimator.SetBool("IsCasting", false);
        if (Time.time - timeWhenLastPlacedPlatform < placementCooldown)
        {
            aiming = false;
            return;
        }

        if (aiming && currentMana < platformManaCost)
        {
            inAimingTransition = true;
            aiming = false;
            placingPlatform = false;
            timeWhenChangedAimState = Time.time;

            Destroy(platformToPlace);
        }

        if (currentMana < platformManaCost)
        {
            aiming = false;
            return;
        }

        if (Time.time - timeWhenChangedAimState < aimTransitionLength)
        {
            inAimingTransition = true;
        }
        else
        {
            inAimingTransition = false;
        }

        if (selectAction.IsPressed())
        {
            wizardAnimator.SetBool("IsCasting", true);
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
            ChangeManaAmount(-platformManaCost);
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


    public void ChangeManaAmount(float changeAmount)
    {
        currentMana += changeAmount;

        if (currentMana < 0)
        {
            currentMana = 0;
        }

        OnManaChange?.Invoke();
    }

    public void FullyFillMana()
    {
        currentMana = maxMana;
        OnManaChange?.Invoke();
    }

    public void WinGame(string playerName)
    {
        float score = Time.time - timeWhenStarted;
        PlayerPrefs.SetFloat(playerName, score);
    }

    public string EndScreenTrigger()
    {
        canMove = false;
        inMenu = true;
        return Convert.ToString(Mathf.Round((Time.time - timeWhenStarted) * 10f) * 0.1f);
    }

    public void LeaderboardScreenTrigger()
    {
        inMenu = false;
    }
}