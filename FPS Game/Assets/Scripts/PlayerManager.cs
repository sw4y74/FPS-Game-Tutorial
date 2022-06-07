using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
	PhotonView PV;

	GameObject controller;

	void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	void Start()
	{
		if(PV.IsMine)
		{
			CreateController();
		}
	}

	void CreateController()
	{
		Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
		controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
	}

	IEnumerator DieRoutine()
    {
		//PV.RPC("RPC_DestroyViewModel", RpcTarget.Others);
		Transform cameraTransform = controller.GetComponent<PlayerController>().firstPersonCamera.transform;
		PhotonNetwork.Destroy(controller);

		GameObject.FindGameObjectWithTag("DeathCam").transform.position = cameraTransform.position;
		GameObject.FindGameObjectWithTag("DeathCam").transform.rotation = cameraTransform.rotation;

		yield return new WaitForSeconds(3f);

		CreateController();
	}

	public void Die()
	{
		StartCoroutine(DieRoutine());
		//PhotonNetwork.Destroy(controller);
		//CreateController();
	}
}