using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
	[SerializeField] Image healthbarImage;
	[SerializeField] GameObject ui;

	[SerializeField] GameObject cameraHolder;
	[SerializeField] Transform[] syncRotationObjects;

	public GameObject viewModel;
	[SerializeField] GameObject localViewModel;
	public float mouseSensitivity;
	[SerializeField] float sprintSpeed, walkSpeed, jumpForce, smoothTime;

	[SerializeField] GameObject itemHolder;
	[SerializeField] GameObject itemHolderMP;
	[SerializeField] Animator GunsAnimator;
	public AudioSource gunAudioSource;
	SingleShotGun[] items;
	SingleShotGun[] itemsMP;

	public int itemIndex;
	int previousItemIndex = -1;

	[SerializeField] TMP_Text ammoText;

	float verticalLookRotation;
	public Transform groundCheck;
	public float groundDistance;
	public LayerMask groundMask;
	public bool grounded = true;
	public bool isMoving;
	public bool isSneaking;
	Vector3 smoothMoveVelocity;
	Vector3 moveAmount;
	PauseMenu pauseMenu;

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

	[SerializeField] Animator playerAnimator;

	void Awake()
	{
		//rb = GetComponent<Rigidbody>();
		PV = GetComponent<PhotonView>();

		pauseMenu = FindObjectOfType<PauseMenu>();

		playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

		items = itemHolder.GetComponentsInChildren<SingleShotGun>();
		itemsMP = itemHolderMP.GetComponentsInChildren<SingleShotGun>();
		killFeed = FindObjectOfType<KillFeed>();
	}

	void Start()
	{
		if(PV.IsMine)
		{
			Destroy(headCollider);

			ChangeSensitivity(RoomManager.Instance.sensitivity);

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

		if (!pauseMenu.GameIsPaused)
		{

			Look();
			Move();
			Jump();

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

			if (items[itemIndex].automatic)
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

		if(transform.position.y < -10f) // Die if you fall out of the world
		{
			Die("gravity");
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

		if (grounded && velocity.y < 0)
        {
			velocity.y = -2f;
        }

		float movementX = Input.GetAxis("Horizontal");
		float movementY = Input.GetAxis("Vertical");
		float strafeThreshold = 0.6f;

		Vector3 move = transform.right * movementX + transform.forward * movementY;
		moveAmount = Vector3.SmoothDamp(moveAmount, move * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);

		if (Input.GetKey(KeyCode.LeftShift)) 
			isSneaking = true;
		else isSneaking = false;

		controller.Move(moveAmount * Time.deltaTime);

		velocity.y += gravity * Time.deltaTime;

		controller.Move(velocity * Time.deltaTime);

		bool movingHorizontally = movementX > strafeThreshold || movementX < -strafeThreshold;
		bool movingVertically = movementY > strafeThreshold || movementY < -strafeThreshold;

		if (movingHorizontally || movingVertically)
		{
			isMoving = true;
			playerAnimator.SetBool("run", true);
		}
		else
		{
			isMoving = false;
			playerAnimator.SetBool("run", false);
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

		if(previousItemIndex != -1)
		{
			items[previousItemIndex].itemGameObject.SetActive(false);
			itemsMP[previousItemIndex].itemGameObject.SetActive(false);
		}

		previousItemIndex = itemIndex;

		if(PV.IsMine)
		{
			Hashtable hash = new Hashtable();
			hash.Add("itemIndex", itemIndex);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
			UpdateAmmoUI();
		}
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if(!PV.IsMine && targetPlayer == PV.Owner)
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

	public void TakeDamage(float damage, string damageDealer)
	{
		PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, damageDealer);
	}

	[PunRPC]
	void RPC_TakeDamage(float damage, string damageDealer)
	{
		if(!PV.IsMine)
			return;

		currentHealth -= damage;

		healthbarImage.fillAmount = currentHealth / maxHealth;

		if(currentHealth <= 0)
		{
			PV.RPC("RPC_AddKillFeedItem", RpcTarget.All, damageDealer, PV.Owner.NickName);
			Die(damageDealer);
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
		ammoText.text = gun.currentAmmo + "/" + gun.maxAmmo;
    }

	void Die(string killer)
	{
		playerManager.Die(killer);
	}

	public void ChangeSensitivity(float value)
    {
		mouseSensitivity = value / 10;
	}
}