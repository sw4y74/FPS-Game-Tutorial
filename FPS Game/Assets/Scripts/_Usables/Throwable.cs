using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public enum ThrowableType { frag, flashbang } // allows to choose grenade type from list

public class Throwable : MonoBehaviour
{
    public Transform throwPoint;
    [SerializeField] GameObject throwableGO;
    [SerializeField] int amount = 1;

    [SerializeField] float range = 18f;

    void Update()
    {
        if (GetComponent<PhotonView>().IsMine && Input.GetKeyDown(KeyCode.G) && amount > 0)
        {
            Throw();
        }
    }


    void Throw()
    {
        Vector3 throwDirection = GetComponent<PlayerController>().firstPersonCamera.transform.forward;
        Vector3 throwDirectionAdjusted = new Vector3(throwDirection.x, throwDirection.y + 0.2f, throwDirection.z);
        Vector3 playerVelocity = GetComponent<PlayerController>().playerCharacterVelocity;

        Debug.Log(range + playerVelocity.magnitude / 2);
        amount--;
        GetComponent<PhotonView>().RPC("RPC_Throw", RpcTarget.All, throwDirectionAdjusted, playerVelocity);
    }

    [PunRPC]
    void RPC_Throw(Vector3 throwDirection, Vector3 playerVelocity)
    {
        GameObject throwableInstance = Instantiate(throwableGO, throwPoint.position, throwPoint.rotation);
        throwableInstance.GetComponent<Rigidbody>().AddForce(throwDirection * (range + playerVelocity.magnitude / 2), ForceMode.Impulse);
    }
}
