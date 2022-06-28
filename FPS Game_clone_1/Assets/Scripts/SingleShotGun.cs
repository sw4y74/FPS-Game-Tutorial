using Photon.Pun;
using System.Collections;
using UnityEngine;


public class SingleShotGun : Gun
{
	[SerializeField] Camera cam;

	PhotonView PV;
	PlayerController root;

	[Header("Weapon properties")]
	[System.NonSerialized] public int index;
	public bool automatic;
	public float fireRate;
	public bool allowFire = true;
	public int maxAmmo;
	public int currentAmmo;
	public bool currentlyEquipped = false;

	[SerializeField] private Recoil Recoil;
	
	[Header("Recoil values")]
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

	[SerializeField] private Kickback Kickback;
	
	[Header("Kickback values")]
	[SerializeField] private float kickbackZ;

	[Header("Weapon accuracy in movement")]
	[SerializeField] private float movementAccuracy = 2;
	[SerializeField] private float jumpAccuracy = 2;

	[Header("Sound")]
	[SerializeField] private AudioClip gunSound;

	[Header("Reload properties")]
	public float reloadSpeed = 2f;
    public bool reloading;
	Coroutine reloadRoutine = null;
	Coroutine shootRoutine = null;

	void Awake()
	{
		PV = GetComponent<PhotonView>();
		currentAmmo = maxAmmo;
		root = transform.root.gameObject.GetComponent<PlayerController>();
	}

	public override void Use()
	{
		if (currentAmmo > 0 && !reloading)
		{
			shootRoutine = StartCoroutine(Shoot());
		}
	}

	public void Reload()
	{
		if (currentAmmo < maxAmmo && !reloading)
		{
			StopCoroutine(shootRoutine);
			allowFire = true;
			reloadRoutine = StartCoroutine(ReloadCoroutine());
		}
	}

	IEnumerator Shoot()
	{
		allowFire = false;

		bool scopeEnabled = false;

		if (GetComponent<SniperScope>() && GetComponent<SniperScope>().scopeOn)
		{
			GetComponent<SniperScope>().ToggleScope(false);
			scopeEnabled = true;
		}

		GunInfo gun = ((GunInfo)itemInfo);

		float accuracyX = 0.5f;
		float accuracyY = 0.5f;
		float randX = Random.Range(-movementAccuracy, movementAccuracy)/10;
		float randY = Random.Range(-movementAccuracy, movementAccuracy)/10;
		float jRandX = Random.Range(-jumpAccuracy, jumpAccuracy) / 10;
		float jRandY = Random.Range(-jumpAccuracy, jumpAccuracy) / 10;

		bool isMoving = root.isMoving;
		bool grounded = root.grounded;
		bool isSneaking = root.isSneaking;

		if (!grounded)
		{
			accuracyX += jRandX;
			accuracyY += jRandY;
		} else if (isMoving)
		{
			accuracyX += randX;
			accuracyY += randY;
		}
		
		if (grounded && isMoving && isSneaking)
        {
			accuracyX = 0.5f + randX / 2;
			accuracyY = 0.5f + randY / 2;
        }

		Ray ray = cam.ViewportPointToRay(new Vector3(accuracyX, accuracyY));
		ray.origin = cam.transform.position;
		int layerMask = 1 << 10;
		layerMask = ~layerMask;

		//ammo
		currentAmmo--;
		root.UpdateAmmoUI();
		if (currentAmmo == 0)
        {
			Reload();
        }

		if (Physics.Raycast(ray, out RaycastHit hit, 2000f, layerMask))
		{

			if (hit.collider.CompareTag("Player"))
			{
				if (hit.collider is SphereCollider)
				{
					transform.root.gameObject.GetComponent<Hitmarker>().ShowHitHS();
				}
				else transform.root.gameObject.GetComponent<Hitmarker>().ShowHit();

			}

			float damage = gun.damage;

			if (hit.collider is SphereCollider)
			{
				damage *= 3;
			}

			if (hit.collider is BoxCollider)
			{
				damage = 0;
			}

			hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage, PV.ViewID);

		}

		PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);

		Recoil.RecoilFire(recoilX, recoilY, recoilZ);
		Kickback.KickbackFire(kickbackZ);
		yield return new WaitForSeconds(fireRate/100);

		if (GetComponent<SniperScope>() && !GetComponent<SniperScope>().scopeOn && scopeEnabled)
		{
			GetComponent<SniperScope>().ToggleScope(true);

		}

		scopeEnabled = false;
		allowFire = true;
	}

	[PunRPC]
	void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
	{
		AudioSource gunAudioSource = root.gunAudioSource;

		gunAudioSource.PlayOneShot(gunSound);

		Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
		if(colliders.Length != 0)
		{
			Quaternion rotation = hitNormal == Vector3.zero
                                  ? Quaternion.identity
                                  : Quaternion.LookRotation(hitNormal);
			GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, rotation * bulletImpactPrefab.transform.rotation);
			Destroy(bulletImpactObj, 10f);
			bulletImpactObj.transform.SetParent(colliders[0].transform);
		}
	}

    void Update()
    {
		//stop reloading when weapon switched
        if (reloading)
        {
			if (root.itemIndex != index)
            {
				StopCoroutine(reloadRoutine);
				reloading = false;
            }
        }

		if (!allowFire)
        {
			if (root.itemIndex != index)
			{
				StopCoroutine(shootRoutine);
				allowFire = true;
			}
		}
    }

    IEnumerator ReloadCoroutine()
    {

		reloading = true;

		yield return new WaitForSeconds(reloadSpeed);

		currentAmmo = maxAmmo;
		reloading = false;
		root.UpdateAmmoUI();
	}

	public void OnEquip()
    {
		currentlyEquipped = true;
		if (GetComponent<SniperScope>())
		{
			root.ChangePlayerSpeed(root.walkSpeed * 0.8f);
		}

		Debug.Log("Equipped "+itemInfo.itemName);
    }

	public void OnUnequip()
    {
		currentlyEquipped = false;

		if (GetComponent<SniperScope>()) 

		if (GetComponent<SniperScope>()) {

			root.ChangePlayerSpeed(root.originalWalkSpeed);

			if (GetComponent<SniperScope>().scopeOn) GetComponent<SniperScope>().ToggleScope(false);

		}

		Debug.Log("Unequipped " + itemInfo.itemName);
	}
}
