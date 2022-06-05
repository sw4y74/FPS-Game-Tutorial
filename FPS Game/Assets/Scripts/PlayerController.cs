using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
	[SerializeField] Image healthbarImage;
	[SerializeField] GameObject ui;

	[SerializeField] GameObject cameraHolder;

	[SerializeField] GameObject viewModel;
	[SerializeField] GameObject localViewModel;

	[SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

	[SerializeField] GameObject itemHolder;
	[SerializeField] GameObject itemHolderMP;
	public AudioSource gunAudioSource;
	SingleShotGun[] items;
	SingleShotGun[] itemsMP;

	int itemIndex;
	int previousItemIndex = -1;

	float verticalLookRotation;
	public Transform groundCheck;
	public float groundDistance;
	public LayerMask groundMask;
	public bool grounded = true;
	public bool isMoving;
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

	void Awake()
	{
		//rb = GetComponent<Rigidbody>();
		PV = GetComponent<PhotonView>();

		pauseMenu = FindObjectOfType<PauseMenu>();

		playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

		items = itemHolder.GetComponentsInChildren<SingleShotGun>();
		itemsMP = itemHolderMP.GetComponentsInChildren<SingleShotGun>();
	}

	void Start()
	{
		if(PV.IsMine)
		{
			Destroy(headCollider);
			mouseSensitivity = RoomManager.Instance.sensitivity / 10;
			viewModel.SetActive(false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			EquipItem(0);
			gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
		}
		else
		{
			Destroy(GetComponentInChildren<Camera>().gameObject);
			//Destroy(rb);
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
		}

		if(transform.position.y < -10f) // Die if you fall out of the world
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
	}

	void Move()
    {
		grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

		if (grounded && velocity.y < 0)
        {
			velocity.y = -2f;
        }

		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");
		float strafeThreshold = 0.6f;

		Vector3 move = transform.right * x + transform.forward * z;
		moveAmount = Vector3.SmoothDamp(moveAmount, move * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);

		controller.Move(moveAmount * Time.deltaTime);

		velocity.y += gravity * Time.deltaTime;

		controller.Move(velocity * Time.deltaTime);

		bool movingHorizontally = x > strafeThreshold || x < -strafeThreshold;
		bool movingVertically = z > strafeThreshold || z < -strafeThreshold;

		if (movingHorizontally || movingVertically)
		{
			isMoving = true;
		}
		else isMoving = false;

	}

	void Move2()
	{
		Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

		moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
		
		float strafeThreshold = 0.6f;

		bool horizontal = Input.GetAxis("Horizontal") > strafeThreshold || Input.GetAxis("Horizontal") < -strafeThreshold;
 		bool vertical = Input.GetAxis("Vertical") > strafeThreshold || Input.GetAxis("Vertical") < -strafeThreshold;

		if (horizontal || vertical) {
			isMoving = true;
		} else isMoving = false;
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

	public void TakeDamage(float damage)
	{
		PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
	}

	[PunRPC]
	void RPC_TakeDamage(float damage)
	{
		if(!PV.IsMine)
			return;

		currentHealth -= damage;

		healthbarImage.fillAmount = currentHealth / maxHealth;

		if(currentHealth <= 0)
		{
			Die();
		}
	}

	void Die()
	{
		playerManager.Die();
	}
}