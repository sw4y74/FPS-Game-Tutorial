using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { sniperRifle, pistol, smg, assaultRifle }

[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo
{
	public float damage;
	public WeaponType weaponType;
}