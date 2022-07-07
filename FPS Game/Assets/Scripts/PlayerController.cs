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

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
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
	SingleShotGun[] items;
	SingleShotGun[] itemsMP;
	[System.NonSerialized] public bool aimingDownSights = false;

	[System.NonSerialized] public int itemIndex;
	int previousItemIndex = -1;

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

	Rigidbody rb;
	public CharacterController controller;

	Vector3 velocity;
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
			Destroy(headCollider);

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
			EquipItem(0);
			gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
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

			for (int i = 0; i < items.Length; i++)
			{
				if (Input.GetKeyDown((i + 1).ToString()))
				{
					EquipItem(i);
					break;
				}
			}

			if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
			{
				if (itemIndex >= items.Length - 1)
				{
					EquipItem(0);
				}
				else
				{
					EquipItem(itemIndex + 1);
				}
			}
			else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
			{
				if (itemIndex <= 0)
				{
					EquipItem(items.Length - 1);
				}
				else
				{
					EquipItem(itemIndex - 1);
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
}