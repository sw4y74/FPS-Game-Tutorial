using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TrainingTarget : MonoBehaviour, IDamageable
{
    [SerializeField] PhotonView PV;
    [SerializeField] float health = 100f;
    [SerializeField] bool isDead = false;
    Material targetMaterial;
    [SerializeField] AudioClip ding;
    AudioSource audioSource;

    private void Awake() {
        PV = GetComponentInParent<PhotonView>();
        targetMaterial = GetComponent<MeshRenderer>().material;
        audioSource = GetComponentInParent<AudioSource>();
    }

    public void TakeDamage(float dmg, int viewID)
    {
        TakeDamageActions(dmg);
        PV.RPC("RPC_TakeDamage", RpcTarget.Others, dmg, viewID);
    }

    [PunRPC]
    void RPC_TakeDamage(float dmg, int viewID)
    {
        TakeDamageActions(dmg);
    }

    public void TakeDamageActions(float dmg) {
        health -= dmg;
        // TODO: do a ding
        if (health <= 0 && !isDead) {
            isDead = true;
            StartCoroutine(TargetDieRoutine());
        }
    }

    IEnumerator TargetDieRoutine() {
        audioSource.PlayOneShot(ding);
        targetMaterial.color = Color.red;
        Debug.Log(targetMaterial.color);
        yield return new WaitForSeconds(3f);
        ReInitialize();
    }

    void ReInitialize() {
        isDead = false;
        health = 100f;
        targetMaterial.color = Color.white;
    }
}