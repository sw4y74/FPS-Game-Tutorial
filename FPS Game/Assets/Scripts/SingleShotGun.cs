using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
{
	[SerializeField] Camera cam;

	PhotonView PV;

	public bool automatic;
	public float fireRate;
	public bool allowFire = true;

	[SerializeField] private Recoil Recoil;
	//hipfire
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

	[SerializeField] private AudioClip gunSound;

	void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	public override void Use()
	{
		StartCoroutine(Shoot());
	}

	IEnumerator Shoot()
	{
		allowFire = false;
		float accuracyX = 0.5f;
		float accuracyY = 0.5f;
		float randX = Random.Range(-2, 2)/20f;
		float randY = Random.Range(-2, 2)/20f;

		bool isMoving = transform.root.gameObject.GetComponent<PlayerController>().isMoving;
		bool grounded = transform.root.gameObject.GetComponent<PlayerController>().grounded;

		if (isMoving || !grounded) {
			accuracyX += randX;
			accuracyY += randY;
		}

		Ray ray = cam.ViewportPointToRay(new Vector3(accuracyX, accuracyY));
		ray.origin = cam.transform.position;
		int layerMask = 1 << 10;
		layerMask = ~layerMask;

		if (Physics.Raycast(ray, out RaycastHit hit, 2000f, layerMask))
		{
			Debug.Log(hit.collider.tag);

			if (hit.collider.CompareTag("Player"))
			{
				if (hit.collider is SphereCollider)
				{
					transform.root.gameObject.GetComponent<Hitmarker>().ShowHitHS();
				}
				else transform.root.gameObject.GetComponent<Hitmarker>().ShowHit();

			}

			float damage = ((GunInfo)itemInfo).damage;

			if (hit.collider is SphereCollider)
			{
				damage *= 3;
			}

			if (hit.collider is BoxCollider)
			{
				damage = 0;
			}

			hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage);

		}

		PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);

		Recoil.RecoilFire(recoilX, recoilY, recoilZ);
		yield return new WaitForSeconds(fireRate/100);
		allowFire = true;
	}

	[PunRPC]
	void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
	{
        AudioSource gunAudioSource = transform.root.gameObject.GetComponent<PlayerController>().gunAudioSource;

		gunAudioSource.PlayOneShot(gunSound);

		Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
		if(colliders.Length != 0)
		{
			Quaternion rotation = hitNormal == Vector3.zero
                                  ? Quaternion.identity
                                  : Quaternion.LookRotation(hitNormal);
			GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
			Destroy(bulletImpactObj, 10f);
			bulletImpactObj.transform.SetParent(colliders[0].transform);
		}
	}
}
