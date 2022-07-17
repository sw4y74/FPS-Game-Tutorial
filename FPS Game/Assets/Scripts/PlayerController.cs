using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utilities;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.IO;

public enum SlotType { Primary, Secondary, Grenade }

[System.Serializable]
public struct WeaponSlot
{
	//Variable declaration
	//Note: I'm explicitly declaring them as public, but they are public by default. You can use private if you choose.
	[SerializeField] public string name;
	[SerializeField] public int weaponIndex;
	[SerializeField] public SlotType slotType;
	[SerializeField] public SingleShotGun gunInstance;

	public WeaponSlot(SingleShotGun wpn)
	{
		gunInstance = wpn;
		weaponIndex = wpn.index;

		if (wpn.gun.primaryWeapon)
		{
			slotType = SlotType.Primary;
			name = "PrimaryWeapon";
		}
		else
		{
			slotType = SlotType.Secondary;
			name = "SecondaryWeapon";
		}
	}
}

public class PlayerController : MonoBehaviourPunCallbacks
{
	[SerializeField] Image healthbarImage;
	[SerializeField] GameObject ui;

	[SerializeField] GameObject cameraHolder;
	[SerializeField] Transform[] syncRotationObjects;

	public GameObject viewModel;
	[SerializeField] GameObject localViewModel;
	public float mouseSensitivity;
	public float sprintSpeed, walkSpeed, jumpForce, smoothTime;
	public bool isSprinting = false;

	[SerializeField] GameObject itemHolder;
	[SerializeField] GameObject itemHolderMP;
	[SerializeField] GameObject arms;
	[SerializeField] GameObject reloadPos;
	[SerializeField] GameObject armsPos;
	[SerializeField] Animator GunsAnimator;
	public AudioSource gunAudioSource;
	public SingleShotGun[] items;
	SingleShotGun[] itemsMP;
	[System.NonSerialized] public bool aimingDownSights = false;
	public WeaponSlot weaponSlot1;
	public WeaponSlot weaponSlot2;
	public List<WeaponSlot> weaponSlots;

	[System.NonSerialized] public int itemIndex;
	int previousItemIndex = -1;
	int slotIndex = -1;

	[SerializeField] TMP_Text ammoText;

	float verticalLookRotation;
	public Transform groundCheck;
	public float groundDistance;
	public LayerMask groundMask;
	public bool grounded = true;
	public bool isMoving;
	Vector3 smoothMoveVelocity;
	Vector3 moveAmount;
	public PauseMenu pauseMenu;

	float m_WeaponBobFactor;
	Vector3 m_WeaponBobLocalPosition;
	Vector3 LastCharacterPosition;
	public float BobFrequency = 10f;
	public float BobSharpness = 10f;
	public float DefaultBobAmount = 0.05f;
	public float AimingBobAmount = 0.02f;

	Rigidbody rb;
	public CharacterController controller;

	public Vector3 velocity;
	public Vector3 playerCharacterVelocity;
	public float gravity = -16.81f;

	PhotonView PV;

	const float maxHealth = 100f;
	float currentHealth = maxHealth;
	public SphereCollider headCollider;

	PlayerManager playerManager;
	public Camera firstPersonCamera;
	KillFeed killFeed;
	[SerializeField] GameObject ragdollPlayer;

	PlayerAnimController animationController;

    public bool IsAiming { get; private set; }

    void Awake()
	{
		//rb = GetComponent<Rigidbody>();
		PV = GetComponent<PhotonView>();

		pauseMenu = FindObjectOfType<PauseMenu>();

		playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

		items = itemHolder.GetComponentsInChildren<SingleShotGun>();
		itemsMP = itemHolderMP.GetComponentsInChildren<SingleShotGun>();
		killFeed = FindObjectOfType<KillFeed>();
		animationController = GetComponent<PlayerAnimController>();
	}

	void Start()
	{
		if(PV.IsMine)
        {
            headCollider.enabled = false;

            if (PlayerPrefs.HasKey("sensitivity"))
            {
                ChangeSensitivity(PlayerPrefs.GetFloat("sensitivity"));
            }

            //turn off MP viewModel renderers and gunHolder
            foreach (Transform child in viewModel.transform)
            {
                if (child.GetComponent<SkinnedMeshRenderer>())
                {
                    child.GetComponent<SkinnedMeshRenderer>().enabled = false;
                }
            }

            itemHolderMP.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            EquipItem(weaponSlots[0].weaponIndex);
            //gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
            SetLayer(10);
            gameObject.tag = "LocalPlayer";

            for (int i = 0; i < items.Length; i++)
            {
                items[i].index = i;
            }

            Debug.Log(PhotonNetwork.LocalPlayer.GetScore());
        }
        else
		{
			Destroy(GetComponentInChildren<Camera>().gameObject);
			Destroy(controller);
			Destroy(ui);
			localViewModel.SetActive(false);
		}
	}

    void Update()
	{
		if(!PV.IsMine)
			return;

		if (grounded && velocity.y < 0)
		{
			velocity.y = -2f;
		}

		if (!pauseMenu.GameIsPaused)
		{
			Look();
			Move();
			Jump();
			GetComponent<Crouch>().CrouchToggler();

			for (int i = 0; i < weaponSlots.Count; i++)
            {
				if (Input.GetKeyDown("1") && weaponSlots[i].slotType.Equals(SlotType.Primary))
				{
					EquipItem(weaponSlots[i].weaponIndex);
				}
				if (Input.GetKeyDown("2") && weaponSlots[i].slotType.Equals(SlotType.Secondary))
                {
					EquipItem(weaponSlots[i].weaponIndex);
				}
			}

			if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
			{
				if (slotIndex >= weaponSlots.Count - 1)
				{
					EquipItem(weaponSlots[0].weaponIndex);
				}
				else
				{
					EquipItem(weaponSlots[slotIndex + 1].weaponIndex);
				}
			}
			else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
			{
				if (slotIndex <= 0)
				{
					EquipItem(weaponSlots[weaponSlots.Count - 1].weaponIndex);
				}
				else
				{
					EquipItem(weaponSlots[slotIndex - 1].weaponIndex);
				}
			}

			if (items[itemIndex].gun.automatic)
			{
				if (Input.GetMouseButton(0) && items[itemIndex].allowFire)
				{
					items[itemIndex].Use();
				}
			}
			else
			{
				if (Input.GetMouseButtonDown(0) && items[itemIndex].allowFire)
				{
					items[itemIndex].Use();
				}
			}

			if (Input.GetKeyDown(KeyCode.R))
            {
				items[itemIndex].Reload();
			}
		}

		if (transform.position.y < -10f) // Die if you fall out of the world
		{
			Die();
		}

	}

	void LateUpdate()
	{
		if (!PV.IsMine)
			return;

		if (!pauseMenu.GameIsPaused)
			UpdateWeaponBob();

		// Set final weapon socket position based on all the combined animation influences
		itemHolder.transform.localPosition = m_WeaponBobLocalPosition;
	}

	void Look()
	{
		transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

		verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

		cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
		foreach(Transform obj in syncRotationObjects)
        {
			obj.localEulerAngles = Vector3.left * verticalLookRotation;
		}
	}

	void Move()
    {
		grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

		if (!grounded)
        {
			smoothTime = 0.03f * 7;
			controller.stepOffset = 0f;
		} else
        {
			smoothTime = 0.03f;
			controller.stepOffset = 0.7f;
		}

		float movementX = Input.GetAxis("Horizontal");
		float movementY = Input.GetAxis("Vertical");
		float strafeThreshold = 0.6f;
		float playerActualSpeed = walkSpeed;

		Vector3 move = transform.right * movementX + transform.forward * movementY;
		Vector3 inputs = Vector3.ClampMagnitude(move, 1f);
		bool movingHorizontally = movementX > strafeThreshold || movementX < -strafeThreshold;
		bool movingVertically = movementY > strafeThreshold || movementY < -strafeThreshold;

		// Define player speed in cases
		if (GetComponent<Crouch>().isCrouching || aimingDownSights)
        {
			isSprinting = false;
			if (GetComponent<Crouch>().isCrouching && aimingDownSights)
				playerActualSpeed = walkSpeed * 0.3f;
			else playerActualSpeed = walkSpeed * 0.5f;

		}
		else if (Input.GetKey(KeyCode.LeftShift) && (movingHorizontally || movingVertically) && grounded)
		{
			playerActualSpeed = sprintSpeed * (1 - CurrentlyEquippedItem().gun.weight / 100);
			isSprinting = true;
		}
		else
        {			
			playerActualSpeed = walkSpeed * (1 - CurrentlyEquippedItem().gun.weight / 100);
			isSprinting = false;
		}

		moveAmount = Vector3.SmoothDamp(moveAmount, inputs * playerActualSpeed, ref smoothMoveVelocity, smoothTime);

		controller.Move(moveAmount * Time.deltaTime);

		velocity.y += gravity * Time.deltaTime;

		controller.Move(velocity * Time.deltaTime);

		animationController.MovementAnimation(movementX, movementY);


		if (movingHorizontally || movingVertically)
		{
			isMoving = true;
		}
		else
		{
			isMoving = false;
		}

	}

	void Jump()
	{
		if(Input.GetKeyDown(KeyCode.Space) && grounded)
		{
			velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
		}
	}

	void EquipItem(int _index)
	{
		if(_index == previousItemIndex)
			return;

		itemIndex = _index;

		items[itemIndex].itemGameObject.SetActive(true);
		itemsMP[itemIndex].itemGameObject.SetActive(true);
		if (items[itemIndex].gun.primaryWeapon) slotIndex = 0;
		else if (!items[itemIndex].gun.primaryWeapon) slotIndex = 1;

		if (previousItemIndex != -1)
		{
			items[previousItemIndex].itemGameObject.SetActive(false);
			itemsMP[previousItemIndex].itemGameObject.SetActive(false);
		}

		if(PV.IsMine)
		{
			if (previousItemIndex != -1)
			{
				items[previousItemIndex].OnUnequip();
			}
			items[itemIndex].OnEquip();
			Hashtable hash = new Hashtable();
			hash.Add("itemIndex", itemIndex);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
			UpdateAmmoUI();
		}

		previousItemIndex = itemIndex;
	}

	void ChangeLoadout(SingleShotGun primaryWeapon, SingleShotGun secondaryWeapon)
	{
		if (primaryWeapon.gun.primaryWeapon)
		{
			int index = weaponSlots.FindIndex(x => x.slotType == SlotType.Primary);
			weaponSlots[index] = new WeaponSlot(primaryWeapon);
		}
		else Debug.LogError("Secondary weapon in primary slot!");

		if (!secondaryWeapon.gun.primaryWeapon)
		{
			int index = weaponSlots.FindIndex(x => x.slotType == SlotType.Secondary);
			weaponSlots[index] = new WeaponSlot(secondaryWeapon);
		}
		else Debug.LogError("Primary weapon in secondary slot!");
	}

	void ChangeLoadout___(SingleShotGun primaryWeapon, SingleShotGun secondaryWeapon)
	{
		if (primaryWeapon.gun.primaryWeapon)
		{
			weaponSlot1 = new WeaponSlot(primaryWeapon);
		}
		else Debug.LogError("Secondary weapon in primary slot!");

		if (!secondaryWeapon.gun.primaryWeapon)
		{
			weaponSlot2 = new WeaponSlot(secondaryWeapon);
		}
		else Debug.LogError("Primary weapon in secondary slot!");
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if(changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
		{
			EquipItem((int)changedProps["itemIndex"]);
		}
	}

	public void SetGroundedState(bool _grounded)
	{
		grounded = _grounded;
	}

	void FixedUpdate()
	{
		if(!PV.IsMine)
			return;

		//rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
	}

	public void TakeDamage(float damage, int photonID)
	{
		PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, photonID);
	}

	[PunRPC]
	void RPC_TakeDamage(float damage, int photonID)
	{
		if(!PV.IsMine)
			return;

		currentHealth -= damage;

		healthbarImage.fillAmount = currentHealth / maxHealth;

		if(currentHealth <= 0)
		{
			Die(photonID);
		}
	}

	[PunRPC]
	void RPC_AddKillFeedItem(string damageDealer, string targetPlayer)
	{
		killFeed.AddKillFeedItem(damageDealer, targetPlayer);
	}

	public void UpdateAmmoUI()
    {
		SingleShotGun gun = items[itemIndex];
		ammoText.text = gun.currentAmmo + "/" + gun.gun.maxAmmo;
    }

	void Die(int photonID = -1)
	{
		string killer;

		if (photonID != -1)
		{
			PhotonNetwork.GetPhotonView(photonID).Owner.AddScore(1);
			killer = PhotonNetwork.GetPhotonView(photonID).Owner.NickName;

		} else killer = "gravity";

		Vector3 ragdollPosition = new Vector3(transform.position.x, transform.position.y-1, transform.position.z);

		PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "RagdollPlayer"), ragdollPosition, transform.rotation);
		//Instantiate(ragdollPlayer, transform.position, transform.rotation);

		PV.RPC("RPC_AddKillFeedItem", RpcTarget.All, killer, PV.Owner.NickName);

		PhotonNetwork.LocalPlayer.AddDeaths(1);
		
		playerManager.Die(killer);
	}

	public void ChangeSensitivity(float value)
    {
		mouseSensitivity = value / 10;
	}

	public void ToggleWeaponRender(bool toggle)
    {
		items[itemIndex].transform.Find("root").gameObject.SetActive(toggle);
    }

	public IEnumerator LerpArmsReloadPosition(bool toggle)
    {
		float timeElapsed = 0;
		float lerpDuration = 0.3f;

		Vector3 a = toggle ? armsPos.transform.localPosition : reloadPos.transform.localPosition;
		Vector3 b = toggle ? reloadPos.transform.localPosition : armsPos.transform.localPosition;

		while (timeElapsed < lerpDuration)
		{
			arms.transform.localPosition = Vector3.Lerp(a, b, (timeElapsed / lerpDuration));
			timeElapsed += Time.deltaTime;

			// Yield here
			yield return null;
		}
		// Make sure we got there
		arms.transform.localPosition = b;
		yield return null;
	}

	public SingleShotGun CurrentlyEquippedItem()
    {
		return items[itemIndex].GetComponent<SingleShotGun>();
    }

	// Updates the weapon bob animation based on character speed
	void UpdateWeaponBob()
	{
		if (Time.deltaTime > 0f)
		{
			playerCharacterVelocity =
				(controller.transform.position - LastCharacterPosition) / Time.deltaTime;

			// calculate a smoothed weapon bob amount based on how close to our max grounded movement velocity we are
			float characterMovementFactor = 0f;
			if (grounded)
			{
				characterMovementFactor =
					Mathf.Clamp01(playerCharacterVelocity.magnitude /
								  (walkSpeed * 2));
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

			LastCharacterPosition = controller.transform.position;
		}
	}

	public void SetLayer(int layer, bool includeChildren = true)
	{
		if (!gameObject) return;
		if (!includeChildren)
		{
			gameObject.layer = layer;
			return;
		}

		foreach (var child in gameObject.GetComponentsInChildren(typeof(Collider), true))
		{
			child.gameObject.layer = layer;
		}
	}

	public void TestChangeLoadout(int primaryWeapon, int secondaryWeapon)
    {
		ChangeLoadout(items[primaryWeapon], items[secondaryWeapon]);
		EquipItem(weaponSlots[0].weaponIndex);
    }
}