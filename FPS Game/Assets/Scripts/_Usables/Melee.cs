using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : Item
{
	[SerializeField] Camera cam;

	PhotonView PV;
	PlayerController root;
	public GameObject impactPrefab;
	[Header("Weapon properties")]
	public int index;
	[System.NonSerialized] public GunInfo gun;
	public bool allowFire = true;
	[System.NonSerialized] public int currentAmmo;
	[System.NonSerialized] public bool currentlyEquipped = false;

	PlayerMovement playerMovement;
    void Awake()
	{
		PV = GetComponent<PhotonView>();
		playerMovement = transform.root.gameObject.GetComponent<PlayerMovement>();
		root = transform.root.gameObject.GetComponent<PlayerController>();
		gun = (GunInfo)itemInfo; // EXAMPLE OF GETTING HIGHER INTERFACE WITH THE LOWER INTERFACE
		currentAmmo = gun.maxAmmo;
	}

	public override void Use()
	{
		return;
	}

	public void OnEquip()
    {
		StartCoroutine(root.LerpArmsReloadPosition(false));
		currentlyEquipped = true;

    }

	public void OnUnequip()
    {
		currentlyEquipped = false;

		if (GetComponent<SniperScope>()) {
			if (GetComponent<SniperScope>().scopeOn) GetComponent<SniperScope>().ToggleScope(false);
		}

	}
}
