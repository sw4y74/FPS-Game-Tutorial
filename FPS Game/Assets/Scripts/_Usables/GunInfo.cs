using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { sniperRifle, pistol, smg, assaultRifle }

[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo
{
	[Range(0.0f, 200.0f)]
	public float damage;

	public WeaponType weaponType;

	[Range(0.0f, 50.0f)]
	public float weight;

	[Header("Weapon properties")]
	public bool automatic;
	public float fireRate;
	public int maxAmmo;
	public bool firstShootAccurate;
	public bool primaryWeapon = true;

	[Header("Recoil")]
	[SerializeField] public float recoilX;
	[SerializeField] public float recoilY;
	[SerializeField] public float recoilZ;

	[Header("Spread")]
	[Range(5f, 100f)]
	public float spreadComeback;
	public float spreadY;
	public float spreadX;

	[Header("Kickback")]
	[SerializeField] public float kickbackZ;

	[Header("Weapon movement accuracy")]
	[Range(0.0f, 20.0f)]
	[SerializeField] public float movementAccuracy = 2;

	[Range(0.0f, 20.0f)]
	[SerializeField] public float jumpAccuracy = 2;

	[Range(0.0f, 20.0f)]
	[SerializeField] public float noScopeAccuracy = 0.6f;

	[Range(0.0f, 20.0f)]
	[SerializeField] public float crouchAccuracy = 2;

	[Header("Sound")]
	[SerializeField] public AudioClip gunSound;

	[Header("Reload properties")]
	public float reloadSpeed = 2f;

}