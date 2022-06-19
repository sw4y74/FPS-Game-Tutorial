using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
	PhotonView PV;

	GameObject controller;

	DeathCam deathCam;

	void Awake()
	{
		PV = GetComponent<PhotonView>();
		deathCam = FindObjectOfType<DeathCam>();
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

	IEnumerator DieRoutine(string killer)
    {
		float timeout = 3f;
		//PV.RPC("RPC_DestroyViewModel", RpcTarget.Others);
		Transform cameraTransform = controller.GetComponent<PlayerController>().firstPersonCamera.transform;
		PhotonNetwork.Destroy(controller);

		GameObject.FindGameObjectWithTag("DeathCam").GetComponent<AudioListener>().enabled = true;
		GameObject.FindGameObjectWithTag("DeathCam").GetComponent<AudioLowPassFilter>().enabled = true;
		GameObject.FindGameObjectWithTag("DeathCam").transform.position = cameraTransform.position;
		GameObject.FindGameObjectWithTag("DeathCam").transform.rotation = cameraTransform.rotation;

		deathCam.DisplayDeathInfo(timeout, killer);

		yield return new WaitForSeconds(timeout);

		GameObject.FindGameObjectWithTag("DeathCam").GetComponent<AudioListener>().enabled = false;
		GameObject.FindGameObjectWithTag("DeathCam").GetComponent<AudioLowPassFilter>().enabled = false;
		CreateController();
	}

	public void Die(string killer)
	{
		StartCoroutine(DieRoutine(killer));
		//PhotonNetwork.Destroy(controller);
		//CreateController();
	}
}