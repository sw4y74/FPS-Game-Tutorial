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
    public void RPC_TakeDamage(float dmg, int viewID)
    {
        TakeDamageActions(dmg);
    }

    public void TakeDamageActions(float dmg) {
        Debug.Log("Target took " + dmg);
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
        yield return new WaitForSeconds(3f);
        ReInitialize();
    }

    void ReInitialize() {
        isDead = false;
        health = 100f;
        targetMaterial.color = Color.white;
    }
}