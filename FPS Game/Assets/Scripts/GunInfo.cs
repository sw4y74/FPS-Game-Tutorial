using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { sniperRifle, pistol, smg, assaultRifle }

[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo
{
	public float damage;
	public WeaponType weaponType;
	public float weight;

	[Header("Weapon properties")]
	public bool automatic;
	public float fireRate;
	public int maxAmmo;
	public bool firstShootAccurate;

	[Header("Recoil")]
	[SerializeField] public float recoilX;
	[SerializeField] public float recoilY;
	[SerializeField] public float recoilZ;

	[Header("Kickback")]
	[SerializeField] public float kickbackZ;

	[Header("Weapon movement accuracy")]
	[SerializeField] public float movementAccuracy = 2;
	[SerializeField] public float jumpAccuracy = 2;
	[SerializeField] public float noScopeAccuracy = 0.6f;
	[SerializeField] public float crouchAccuracyModifier = 3;

	[Header("Sound")]
	[SerializeField] public AudioClip gunSound;

	[Header("Reload properties")]
	public float reloadSpeed = 2f;

}