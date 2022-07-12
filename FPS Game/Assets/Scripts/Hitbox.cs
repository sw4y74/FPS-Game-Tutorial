using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour, IDamageable
{
    [SerializeField] PlayerController PC;
    public bool isHead = false;

    public void TakeDamage(float dmg, int viewID)
    {
        PC.TakeDamage(dmg, viewID);
    }
}
