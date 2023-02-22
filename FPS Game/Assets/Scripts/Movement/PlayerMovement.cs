using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float playerHeight = 2f;
    [SerializeField] public CapsuleCollider playerCollider;
    PauseMenu pauseMenu;
    PhotonView PV;
    PlayerAnimController animationController;
    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    float movementMultiplier = 10f;
    [Range(0.5f, 3f)]
    [SerializeField] float movementThreshold = 0.5f; // Mainly for side to side recoil movement
    
    [Header("Sprinting")]
    [SerializeField] public float walkSpeed = 4f;
    [SerializeField] public float sprintSpeed = 6f;
    [SerializeField] public float crouchSpeed = 2f;
    [SerializeField] float acceleration = 10f;
    [Range(0.5f, 2.5f)]
    [SerializeField] float slopeSpeedMultiplier = 1f;

    [Header("Jumping")]
    public float jumpForce = 5f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    float horizontalMovement;
    float verticalMovement;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;
    public bool isGrounded { get; private set; }

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

    Rigidbody rb;

    RaycastHit slopeHit;
    [SerializeField] float slideMultiplier;
    [SerializeField] float slideAccelerationMultiplier;
    [SerializeField] float slopeDistance;
    [Range(1f, 85f)]
    [SerializeField] float maxSlopeAngle = 40f;

    private bool OnSlope()
    {
        slopeDistance = playerHeight / 2 + 0.5f;
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, slopeDistance, groundMask))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private float GetSlopeAngle() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, slopeDistance, groundMask))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle;
        }
        return 0;
    }

    private void Awake() {
        PV = GetComponent<PhotonView>();
        animationController = GetComponent<PlayerAnimController>();
        pauseMenu = FindObjectOfType<PauseMenu>();
    }

    private void Start()
    {
        if(!PV.IsMine)
			return;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if(!PV.IsMine)
			return;
        if (!pauseMenu.GameIsPaused)
        {
            MyInput();
            AnimatePlayer();
            Jump();
            GetComponent<Crouch>().CrouchToggler();
        }
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        ControlDrag();
        ControlSpeed();
        ControlPlayerHeight();

        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection * slopeSpeedMultiplier, slopeHit.normal);
    }

    private void ControlPlayerHeight()
    {
        if (GetComponent<Crouch>().isCrouching)
        {
            playerHeight = 1.55f;
            playerCollider.height = playerHeight;
            playerCollider.center = new Vector3(0, 0.735f, 0);
        }
        else
        {
            playerHeight = 2f;
            playerCollider.height = playerHeight;
            playerCollider.center = new Vector3(0, 0.955f, 0);
        }
    }

    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void Jump()
    {
        float gunWeight = 1 - GetComponent<PlayerController>().CurrentlyEquippedItem().gun.weight / 300;
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce * gunWeight, ForceMode.Impulse);
        }
    }

    void ControlSpeed()
    {
        if (GetComponent<Crouch>().isCrouching && isGrounded)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, crouchSpeed, acceleration * (moveSpeed > crouchSpeed * 1.2f ? slideAccelerationMultiplier : 1f) * Time.deltaTime);
        }
        else if (Input.GetKey(sprintKey) && isGrounded && !GetComponent<Crouch>().isCrouching)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
        moveSpeed *= 1 - GetComponent<PlayerController>().CurrentlyEquippedItem().gun.weight / 1000;
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
            return;
        MovePlayer();
    }

    void MovePlayer()
    {
        if (isGrounded && !OnSlope())
        {
            if (GetSlopeAngle() > maxSlopeAngle && GetSlopeAngle() != 0) {
                rb.AddForce(Vector3.down * 50f, ForceMode.Force);
            } else {
                rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
            }
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
        // rb.useGravity = !OnSlope();
    }

    void AnimatePlayer() {
        animationController.MovementAnimation(horizontalMovement, verticalMovement);
    }

    public bool IsMoving()
    {
        if (rb == null) return false;
        if (rb.velocity.magnitude > 2f+movementThreshold) return true;
        else return false;
        // if (horizontalMovement != 0 || verticalMovement != 0)
        // {
        //     // Player is moving
        //     return true;
        // }
        // else
        // {
        //     // Player is not moving
        //     return false;
        // }
    }

    public bool IsSprinting() {
        if (Input.GetKey(sprintKey) && IsMoving() && isGrounded)
        {
            // Player is sprinting
            return true;
        }
        else
        {
            // Player is not sprinting
            return false;
        }
    }
}
