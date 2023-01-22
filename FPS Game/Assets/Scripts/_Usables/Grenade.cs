using Photon.Pun;
using System.Collections;
using UnityEngine;


public class Grenade : Item
{
	[SerializeField] Camera cam;

	PhotonView PV;
	PlayerController root;

	[Header("Weapon properties")]
	[System.NonSerialized] public int index;
	[System.NonSerialized] public GunInfo gun;
	public bool allowFire = true;
	[System.NonSerialized] public bool currentlyEquipped = false;

	public Transform throwPoint;
	[SerializeField] GameObject throwableGO;
	[SerializeField] public int amount = 1;

	[SerializeField] float range = 18f;
	bool isThrowing = false;

	void Awake()
	{
		PV = GetComponent<PhotonView>();
		root = transform.root.gameObject.GetComponent<PlayerController>();
		gun = (GunInfo)itemInfo; // EXAMPLE OF GETTING HIGHER INTERFACE WITH THE LOWER INTERFACE
	}

	public override void Use()
	{
		if (amount > 0 && !isThrowing)
		{
			StartCoroutine(Throw());
		}
	}

	IEnumerator Throw()
	{
		allowFire = false;
		isThrowing = true;
		yield return new WaitForSeconds(1f);
		Vector3 throwDirection = GetComponent<PlayerController>().firstPersonCamera.transform.forward;
		Vector3 throwDirectionAdjusted = new Vector3(throwDirection.x, throwDirection.y + 0.2f, throwDirection.z);
		Vector3 playerVelocity = GetComponent<PlayerController>().playerCharacterVelocity;

		Debug.Log(range + playerVelocity.magnitude / 2);
		amount--;
		GetComponent<PhotonView>().RPC("RPC_Throw", RpcTarget.All, throwDirectionAdjusted, playerVelocity);

		isThrowing = false;
		allowFire = true;
	}

	[PunRPC]
	void RPC_Throw(Vector3 throwDirection, Vector3 playerVelocity)
	{
		GameObject throwableInstance = Instantiate(throwableGO, throwPoint.position, throwPoint.rotation);
		throwableInstance.GetComponent<Rigidbody>().AddForce(throwDirection * (range + playerVelocity.magnitude / 2), ForceMode.Impulse);
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
