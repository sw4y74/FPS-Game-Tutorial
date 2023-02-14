using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    PhotonView PV;
    Rigidbody rb;
    PauseMenu pauseMenu;
    PlayerController PC;
    WallRun wallRun;
    [Header("Synced rotation objects and camera holder")]
    [SerializeField] Transform[] syncRotationObjects;
    [SerializeField] GameObject cameraHolder;
    [Header("Mouse input and rotation")]
    private float mouseInputX;
	private float mouseInputY;
	private float yaw;
    float verticalLookRotation;
    [Header("Weapon Bobbing")]
	public float BobFrequency = 7f;
	public float BobSharpness = 5f;
	public float DefaultBobAmount = 0.035f;
	public float AimingBobAmount = 0.02f;
	float m_WeaponBobFactor;
	Vector3 m_WeaponBobLocalPosition;
    private PlayerMovement playerMovement;
    public bool IsAiming { get; private set; }

    void Awake() {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        pauseMenu = FindObjectOfType<PauseMenu>();
        PC = GetComponent<PlayerController>();
        wallRun = GetComponent<WallRun>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!PV.IsMine)
            return;

        if (pauseMenu.GameIsPaused)
            return;

        LookInput();
		Look();	

		if (PC.isActiveAndEnabled)
        	PC.itemHolder.transform.localPosition = m_WeaponBobLocalPosition;
    }

    void LateUpdate()
	{
		if (!PV.IsMine || pauseMenu.GameIsPaused)
			return;
		UpdateWeaponBob();
	}

    void LookInput() {
		mouseInputX = Input.GetAxisRaw("Mouse X");
		mouseInputY = Input.GetAxisRaw("Mouse Y");
	}

	void Look()
	{
		if (cameraHolder == null)
			return;
		// transform.Rotate(Vector3.up * mouseInputX * mouseSensitivity);
		yaw = (yaw + mouseInputX * PC.mouseSensitivity) % 360f;
		rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
		verticalLookRotation += mouseInputY * PC.mouseSensitivity;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

		cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
		if (wallRun.enabled) {
			cameraHolder.transform.localEulerAngles = new Vector3(cameraHolder.transform.localEulerAngles.x, cameraHolder.transform.localEulerAngles.y, wallRun.tilt);
		}
		foreach(Transform obj in syncRotationObjects)
        {
			obj.localEulerAngles = Vector3.left * verticalLookRotation;
		}
	}

    void UpdateWeaponBob()
	{
		if (Time.deltaTime > 0f)
		{
			PC.playerCharacterVelocity =
				(transform.position - PC.LastCharacterPosition) / Time.deltaTime;

			// calculate a smoothed weapon bob amount based on how close to our max grounded movement velocity we are
			float characterMovementFactor = 0f;
			if (playerMovement.isGrounded)
			{
				characterMovementFactor =
					Mathf.Clamp01(PC.playerCharacterVelocity.magnitude /
								  (playerMovement.walkSpeed * 2));
			}

			m_WeaponBobFactor =
				Mathf.Lerp(m_WeaponBobFactor, characterMovementFactor, BobSharpness * Time.deltaTime);

			// Calculate vertical and horizontal weapon bob values based on a sine function
			float bobAmount = IsAiming ? AimingBobAmount : DefaultBobAmount;
			float frequency = BobFrequency;
			float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * m_WeaponBobFactor;
			float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.5f) * bobAmount *
							  m_WeaponBobFactor;

			// Apply weapon bob
			m_WeaponBobLocalPosition.x = hBobValue;
			m_WeaponBobLocalPosition.y = Mathf.Abs(vBobValue)+0.08f;
			m_WeaponBobLocalPosition.z = -0.176f;

			PC.LastCharacterPosition = transform.position;
		}
	}
}
