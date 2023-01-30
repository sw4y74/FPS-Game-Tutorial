using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Footsteps : MonoBehaviour
{
    PlayerMovement playerMovement;
    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    [PunRPC]
    void RPC_Footsteps(float randA, float randB)
    {
        GetComponent<AudioSource>().volume = randA;
        GetComponent<AudioSource>().pitch = randB;
        GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerMovement.isGrounded && !playerMovement.GetComponent<Crouch>().isCrouching && playerMovement.IsMoving() && GetComponent<AudioSource>().isPlaying == false)
        {
            float randA = Random.Range(0.8f, 1);
            float randB = Random.Range(0.8f, 1.1f);
            PV.RPC("RPC_Footsteps", RpcTarget.All, randA, randB);
        }
    }
}
