using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Item
{
	[SerializeField] Camera cam;

	PhotonView PV;
	PlayerController root;
	public GameObject bulletImpactPrefab;
	[Header("Weapon properties")]
	public int index;
	[System.NonSerialized] public GunInfo gun;
	public bool allowFire = true;
	[System.NonSerialized] public int currentAmmo;
	[System.NonSerialized] public bool currentlyEquipped = false;

	[SerializeField] private Recoil Recoil;

	[SerializeField] private Kickback Kickback;

	[SerializeField] private float firstShootAccurate = 0f;
	[SerializeField] private float spreadCooldown = 0f;

	[Header("Reload properties")]
    public bool reloading;
	Coroutine reloadRoutine = null;
	Coroutine shootRoutine = null;

	[SerializeField] ParticleSystem muzzleFlash;
	[SerializeField] ParticleSystem muzzleFlashStraight;
	[SerializeField] ParticleSystem bulletTrail;

    void Awake()
	{
		PV = GetComponent<PhotonView>();
		root = transform.root.gameObject.GetComponent<PlayerController>();
		gun = (GunInfo)itemInfo; // EXAMPLE OF GETTING HIGHER INTERFACE WITH THE LOWER INTERFACE
		currentAmmo = gun.maxAmmo;
	}

	public override void Use()
	{
		if (currentAmmo > 0 && !reloading && !root.isSprinting)
		{
			shootRoutine = StartCoroutine(Shoot());
		}
	}

	public void Reload()
	{
		if (currentAmmo < gun.maxAmmo && !reloading)
		{
			if (GetComponent<SniperScope>() && GetComponent<SniperScope>().scopeOn)
			{
				GetComponent<SniperScope>().ToggleScope(false);
			}

			StopCoroutine(shootRoutine);
			allowFire = true;
			
			reloadRoutine = StartCoroutine(ReloadCoroutine());
		}
	}

	IEnumerator Shoot()
	{
		allowFire = false;

		bool scopeEnabled = false;

		float accuracyX = 0.5f;
		float accuracyY = 0.5f;
		float randX = Random.Range(-gun.movementAccuracy, gun.movementAccuracy)/10;
		float randY = Random.Range(-gun.movementAccuracy, gun.movementAccuracy)/10;
		float jRandX = Random.Range(-gun.jumpAccuracy, gun.jumpAccuracy) / 10;
		float jRandY = Random.Range(-gun.jumpAccuracy, gun.jumpAccuracy) / 10;
		float cRandX = Random.Range(-gun.crouchAccuracy, gun.crouchAccuracy) / 10;
		float cRandY = Random.Range(-gun.crouchAccuracy, gun.crouchAccuracy) / 10;

		bool isMoving = root.isMoving;
		bool grounded = root.grounded;
		bool isCrouching = root.GetComponent<Crouch>().isCrouching;
		bool adsOn = root.aimingDownSights;

		#region Spread and Recoil

		// CS LIKE RECOIL
		accuracyY = 0.5f + spreadCooldown / 10;
		accuracyX = 0.5f + Random.Range(-(spreadCooldown + gun.spreadX/20) / 100, (spreadCooldown + gun.spreadX/20) / 100);
		if (spreadCooldown < gun.spreadY/3)
			spreadCooldown += gun.spreadY/2 / 10;
		else spreadCooldown = gun.spreadY/3 + Random.Range(-gun.spreadY/50, gun.spreadY/50);

		if (!grounded)
		{
			accuracyX += jRandX;
			accuracyY += jRandY;
		} else if (isMoving)
		{
			accuracyX += randX;
			accuracyY += randY;
		}
		
		if (grounded && isMoving && isCrouching)
        {
			accuracyX = 0.5f + cRandX;
			accuracyY = 0.5f + cRandY;
        }

		if (gun.firstShootAccurate)
        {
			if (firstShootAccurate == 0f && grounded)
            {
				accuracyX = 0.5f;
				accuracyY = 0.5f;
			}

			firstShootAccurate = gun.fireRate / 1.5f;
		}

		if (gun.weaponType.Equals(WeaponType.sniperRifle))
        {
			// noscope
			if (grounded && !isMoving)
            {
				accuracyX = 0.5f + Random.Range(-gun.noScopeAccuracy, gun.noScopeAccuracy) / 10;
				accuracyY = 0.5f + Random.Range(-gun.noScopeAccuracy, gun.noScopeAccuracy) / 10;
			} else
            {
				accuracyX = 0.5f + Random.Range(-gun.noScopeAccuracy * 3, gun.noScopeAccuracy * 3) / 10;
				accuracyY = 0.5f + Random.Range(-gun.noScopeAccuracy * 3, gun.noScopeAccuracy * 3) / 10;
			}

			if (grounded && isCrouching)
            {
				accuracyX = 0.5f + Random.Range(-gun.noScopeAccuracy * 0.6f, gun.noScopeAccuracy * 0.6f) / 10;
				accuracyY = 0.5f + Random.Range(-gun.noScopeAccuracy * 0.6f, gun.noScopeAccuracy * 0.6f) / 10;
			}

			if (adsOn)
            {
				GetComponent<SniperScope>().ToggleScope(false);
				scopeEnabled = true;

				if (root.grounded)
                {
					accuracyX = 0.5f;
					accuracyY = 0.5f;
				} else
                {
					accuracyX = 0.5f + Random.Range(-1.5f, 1.5f) / 10;
					accuracyY = 0.5f + Random.Range(-1.5f, 1.5f) / 10;
				}

			}
		}

		#endregion
	
		if (gun.weaponType == WeaponType.shotgun) {
			Vector3[] bulletPositions = new Vector3[8];
			Vector3[] bulletNormals = new Vector3[8];
			for (int i = 0; i < 8; i++) {
				float xDeviation = Random.Range(-0.02f, 0.02f);
				float yDeviation = Random.Range(-0.05f, 0.05f);
				Ray shotgunRay = cam.ViewportPointToRay(new Vector3(accuracyX+xDeviation, accuracyY+yDeviation));
				shotgunRay.origin = cam.transform.position;
				int shotgunLayerMask = 1 << 10;
				shotgunLayerMask = ~shotgunLayerMask;

				if (Physics.Raycast(shotgunRay, out RaycastHit shotgunHit, 2000f, shotgunLayerMask))
				{
					Hitbox hitbox = shotgunHit.collider.gameObject.GetComponent<Hitbox>();

					if (hitbox)
					{
						if (hitbox.isHead)
						{
							transform.root.gameObject.GetComponent<Hitmarker>().ShowHitHS();
						}
						else transform.root.gameObject.GetComponent<Hitmarker>().ShowHit();

					}

					float damage = gun.damage;

					if (hitbox && hitbox.isHead)
					{
						damage *= 3;
					} else if (hitbox && hitbox.isLimb) {
						damage *= 0.8f;
					}

					shotgunHit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage, PV.ViewID);
					bulletPositions[i] = shotgunHit.point;
					bulletNormals[i] = shotgunHit.normal;
				}
			}
			currentAmmo--;
			root.UpdateAmmoUI();
			if (currentAmmo == 0)
			{
				Reload();
			}
			PV.RPC("RPC_Shoot", RpcTarget.All, bulletPositions, bulletNormals);
		} else {
			Vector3[] bulletPositions = new Vector3[1];
			Vector3[] bulletNormals = new Vector3[1];
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
				Hitbox hitbox = hit.collider.gameObject.GetComponent<Hitbox>();

				if (hitbox)
				{
					if (hitbox.isHead)
					{
						transform.root.gameObject.GetComponent<Hitmarker>().ShowHitHS();
					}
					else transform.root.gameObject.GetComponent<Hitmarker>().ShowHit();

				}

				float damage = gun.damage;

				if (hitbox && hitbox.isHead)
				{
					damage *= 3;
				} else if (hitbox && hitbox.isLimb) {
					damage *= 0.8f;
				}

				hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage, PV.ViewID);
				bulletPositions[0] = hit.point;
				bulletNormals[0] = hit.normal;
				PV.RPC("RPC_Shoot", RpcTarget.All, bulletPositions, bulletNormals);
			}
		}
		

		Recoil.RecoilFire(gun.recoilX/2, gun.recoilY/2, gun.recoilZ/2);
		Kickback.KickbackFire(gun.kickbackZ);

		yield return new WaitForSeconds(gun.fireRate/100);

		if (gun.weaponType.Equals(WeaponType.sniperRifle) && !GetComponent<SniperScope>().scopeOn && !reloading && scopeEnabled)
		{
			GetComponent<SniperScope>().ToggleScope(true);

		}

		scopeEnabled = false;
		allowFire = true;
	}

	[PunRPC]
	void RPC_Shoot(Vector3[] hitPositions, Vector3[] hitNormals)
	{
		AudioSource gunAudioSource = root.gunAudioSource;

		muzzleFlash.Play();
		muzzleFlashStraight.Play();
/*		bulletTrail.transform.rotation = Quaternion.LookRotation(hitNormal);
		bulletTrail.Play();*/

		gunAudioSource.PlayOneShot(gun.gunSound);

		for (int i = 0; i < hitPositions.Length; i++)
		{
			Collider[] colliders = Physics.OverlapSphere(hitPositions[i], 0.3f);
			if(colliders.Length != 0)
			{
				Quaternion rotation = hitNormals[i] == Vector3.zero
									? Quaternion.identity
									: Quaternion.LookRotation(hitNormals[i]);
				GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPositions[i] + hitNormals[i] * 0.001f, rotation * bulletImpactPrefab.transform.rotation);
				Destroy(bulletImpactObj, 10f);
				bulletImpactObj.transform.SetParent(colliders[0].transform);
			}
		}
	}

    void FixedUpdate()
    {
		//stop reloading when weapon switched
        if (reloading)
        {
			if (root.itemIndex != index)
            {
				StopCoroutine(reloadRoutine);
				StartCoroutine(root.LerpArmsReloadPosition(false));
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
		} else
        {
			if (firstShootAccurate > 0f)
			{
				firstShootAccurate -= 0.5f;
			}
			else if (firstShootAccurate < 0f) firstShootAccurate = 0f;

			if (spreadCooldown > 0f) {
				spreadCooldown -= 1 / gun.spreadComeback;
			} else if (spreadCooldown < 0f) spreadCooldown = 0f;
		}
		
    }

    IEnumerator ReloadCoroutine()
    {
		reloading = true;

		StartCoroutine(root.LerpArmsReloadPosition(true));

		yield return new WaitForSeconds(gun.reloadSpeed * 0.8f);

		StartCoroutine(root.LerpArmsReloadPosition(false));

		yield return new WaitForSeconds(gun.reloadSpeed - gun.reloadSpeed * 0.8f);

		currentAmmo = gun.maxAmmo;
		reloading = false;
		root.UpdateAmmoUI();
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
