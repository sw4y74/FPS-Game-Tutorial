using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.IO;
using System;
using UnityEngine.Events;

public enum SlotType { Primary, Secondary, Grenade, Knife }

[System.Serializable]
public struct WeaponSlot
{
	//Variable declaration
	//Note: I'm explicitly declaring them as public, but they are public by default. You can use private if you choose.
	[SerializeField] public string name;
	[SerializeField] public int weaponIndex;
	[SerializeField] public SlotType slotType;
	[SerializeField] public Gun gunInstance;

	public WeaponSlot(Gun wpn)
	{
		gunInstance = wpn;
		weaponIndex = wpn.index;

		switch (wpn.gun.weaponSlot)
		{
			case WeaponSlotType.primary:
				slotType = SlotType.Primary;
				name = "PrimaryWeapon";
				break;
			case WeaponSlotType.secondary:
				slotType = SlotType.Secondary;
				name = "SecondaryWeapon";
				break;
			case WeaponSlotType.melee:
				slotType = SlotType.Knife;
				name = "Knife";
				break;
			default:
				slotType = SlotType.Primary;
				name = "PrimaryWeapon";
				break;
		}
	}
}

public class PlayerController : MonoBehaviourPunCallbacks
{
	[Header("UI and Camera")]
	[SerializeField] Image healthbarImage;
	[SerializeField] GameObject ui;
	public PauseMenu pauseMenu; 
	WallRun wallRun;

	[Header("Viewmodels")]
	public GameObject viewModel;
	[SerializeField] GameObject localViewModel;
	public float mouseSensitivity;
	

	[Header("Weapon Gameobjects")]
	public GameObject itemHolder;
	public GameObject itemHolderMP;
	[SerializeField] GameObject arms;
	[SerializeField] GameObject reloadPos;
	[SerializeField] GameObject armsPos;
	[SerializeField] Animator GunsAnimator;
	public AudioSource gunAudioSource;
	public Gun[] items;
	public Gun[] itemsMP;
	[System.NonSerialized] public bool aimingDownSights = false;
	public bool freezeTime = false;

	[Header("Loadout")]
	[SerializeField] TMP_Text ammoText;
	public List<WeaponSlot> weaponSlots;
	[System.NonSerialized] public int itemIndex;
	int previousItemIndex = -1;
	int slotIndex = -1;
	public Vector3 LastCharacterPosition;

	[Header("Velocity")]
	public Vector3 playerCharacterVelocity;

	// [Header("fall damage")] -------- TO BE MOVED TO PLAYERMOVEMENT SCRIPT
    // private float fallVelocity = 0f;
	// [Range(5f, 80f)]
    // [SerializeField] float damagableFallVelocity = 15f;
	// [SerializeField] AudioClip fallDamageSound;
	// [Range(0f, 3f)]
    // [SerializeField] float fallModifier = 1.75f;

	PhotonView PV;

	const float maxHealth = 100f;
	float currentHealth = maxHealth;
	bool isDead = false;
	public SphereCollider headCollider;

	PlayerManager playerManager;
	public Camera firstPersonCamera;
	KillFeed killFeed;
	[SerializeField] GameObject ragdollPlayer;

	PlayerAnimController animationController;
	PlayerMovement playerMovement;
	private UnityAction<int, int> OnPlayerKill;

	private void OnDestroy() {
		Debug.Log("Destroyed!!!");
	}

    void Awake()
	{
	
		PV = GetComponent<PhotonView>();
		wallRun = GetComponent<WallRun>();
		pauseMenu = FindObjectOfType<PauseMenu>();

		if (PV.isRuntimeInstantiated) playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

		items = itemHolder.GetComponentsInChildren<Gun>();
		itemsMP = itemHolderMP.GetComponentsInChildren<Gun>();
		killFeed = FindObjectOfType<KillFeed>();
		animationController = GetComponent<PlayerAnimController>();
	}

	private void OnEnable() {
		base.OnEnable();
		OnPlayerKill += GameModeManager.Instance.HandlePlayerKill;
	}

	void Start()
	{
		if(PV.IsMine)
        {
			foreach (var item in PhotonNetwork.PlayerList)
			{
				if (item.GetTeam() != -1)
				{
					Debug.Log(item.GetTeam());
				}
			}
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

			playerMovement = GetComponent<PlayerMovement>();
            itemHolderMP.SetActive(false);

			// if game's not paused lock cursor
			if (!pauseMenu.GameIsPaused) {
				Cursor.lockState = CursorLockMode.Locked;
            	Cursor.visible = false;
			}

			//SET CURRENT LOADOUT AFTER RESPAWN
			ChangeLoadoutByIndex(pauseMenu.primaryWeapon, pauseMenu.secondaryWeapon);
			EquipItem(weaponSlots[0].weaponIndex);
			SetLayer(10);

            gameObject.tag = "LocalPlayer";
        }
        else
		{
			wallRun.enabled = false;
			Destroy(GetComponentInChildren<Camera>().gameObject);
			Destroy(ui);
			
			localViewModel.SetActive(false);
		}
	}

    void Update()
    {
        if (!PV.IsMine)
            return;

        if (transform.position.y < -10f) // Die if you fall out of the world
        {
            Die();
        }

        if (pauseMenu.GameIsPaused)
            return;

        WeaponInput();
    }

    private void WeaponInput()
    {
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
			if (Input.GetKeyDown("3") && weaponSlots[i].slotType.Equals(SlotType.Knife))
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

		if (!freezeTime) {
			if (items[itemIndex].gun.automatic)
			{
				if (Input.GetMouseButton(0) && items[itemIndex].allowFire)
				{
					if (Cursor.visible)
					{
						Cursor.lockState = CursorLockMode.Locked;
						Cursor.visible = false;
					}
					items[itemIndex].Use();
				}
			}
			else
			{
				if (Input.GetMouseButtonDown(0) && items[itemIndex].allowFire)
				{
					if (Cursor.visible)
					{
						Cursor.lockState = CursorLockMode.Locked;
						Cursor.visible = false;
					}
					items[itemIndex].Use();
				}
			}
		}
        if (Input.GetKeyDown(KeyCode.R))
        {
            items[itemIndex].Reload();
        }
    }

    // private void HandleFallDamage()
    // {
    //     if (!grounded)
    // 	{
    // 		fallVelocity = velocity.y*2;
    // 	}
    // 	else if (grounded)
    // 	{
    // 		if (fallVelocity < -damagableFallVelocity)
    // 		{
    // 			Debug.Log("Falling damage: " + fallVelocity);
    // 			TakeDamage(-fallVelocity*fallModifier, -1);
    // 			gunAudioSource.PlayOneShot(fallDamageSound);
    // 		}
    // 		fallVelocity = 0f;
    // 	}
    // }

    void FixedUpdate() {
		if (!PV.IsMine || pauseMenu.GameIsPaused)
			return;
	}

	void EquipItem(int _index)
	{
		if (_index == previousItemIndex)
			return;

		itemIndex = _index;

		items[itemIndex].itemGameObject.SetActive(true);
		itemsMP[itemIndex].itemGameObject.SetActive(true);

		switch (items[itemIndex].gun.weaponSlot)
		{
			case WeaponSlotType.primary:
				slotIndex = 0;
				break;
			case WeaponSlotType.secondary:
				slotIndex = 1;
				break;
			case WeaponSlotType.melee:
				slotIndex = 2;
				break;
		}
		// if (items[itemIndex].gun.weaponSlot == WeaponSlotType.primary)
		// 	slotIndex = 0;
		// else if (items[itemIndex].gun.weaponSlot == WeaponSlotType.secondary)
		// 	slotIndex = 1;
		// else if (items[itemIndex].gun.weaponSlot == WeaponSlotType.melee)
		// 	slotIndex = 2;
		

		if (previousItemIndex != -1)
		{
			items[previousItemIndex].itemGameObject.SetActive(false);
			itemsMP[previousItemIndex].itemGameObject.SetActive(false);
		}

		if (PV.IsMine)
		{
			if (previousItemIndex != -1)
			{
				items[previousItemIndex].OnUnequip();
			}
			items[itemIndex].OnEquip();
			PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "itemIndex", itemIndex } });
			UpdateAmmoUI();
		}

		previousItemIndex = itemIndex;
	}

	void ChangeLoadout(Gun primaryWeapon, Gun secondaryWeapon)
	{
		if (primaryWeapon.gun.weaponSlot == WeaponSlotType.primary)
		{
			int index = weaponSlots.FindIndex(x => x.slotType == SlotType.Primary);
			weaponSlots[index] = new WeaponSlot(primaryWeapon);
		}
		else Debug.LogError("Secondary weapon in primary slot!");

		if (secondaryWeapon.gun.weaponSlot == WeaponSlotType.secondary)
		{
			int index = weaponSlots.FindIndex(x => x.slotType == SlotType.Secondary);
			weaponSlots[index] = new WeaponSlot(secondaryWeapon);
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

		if(currentHealth <= 0 && !isDead)
		{
			isDead = true;
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
		Gun gun = items[itemIndex];
		ammoText.text = gun.currentAmmo + "/" + gun.gun.maxAmmo;
    }

	void Die(int photonID = -1)
	{
		string killer;

		if (photonID != -1)
		{
			int killScore = 1;
			OnPlayerKill?.Invoke(killScore, photonID);
			PhotonNetwork.GetPhotonView(photonID).Owner.AddScore(killScore);		
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

	public Gun CurrentlyEquippedItem()
    {
		return items[itemIndex].GetComponent<Gun>();
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

	public void ChangeLoadoutByIndex(int primaryWeapon, int secondaryWeapon)
    {
		ChangeLoadout(items[primaryWeapon], items[secondaryWeapon]);
		EquipItem(weaponSlots[0].weaponIndex);
    }
}