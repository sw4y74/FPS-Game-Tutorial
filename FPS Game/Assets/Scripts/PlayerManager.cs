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
		//destroy mp viewmodel through network
		PV.RPC("RPC_DestroyViewModel", RpcTarget.All);

		yield return new WaitForSeconds(3f);

		PhotonNetwork.Destroy(controller);
		CreateController();
	}

	public void Die()
	{
		StartCoroutine(DieRoutine());
		//PhotonNetwork.Destroy(controller);
		//CreateController();
	}

	[PunRPC]
	void RPC_DestroyViewModel()
    {
		//something weird is happening here
		Debug.Log("RPC_DestroyVM");
		Debug.Log(controller.GetComponent<PlayerController>().itemIndex);
		controller.GetComponent<PlayerController>().viewModel.gameObject.SetActive(false);
		//controller.GetComponent<PlayerController>().viewModel.gameObject.SetActive(false);
		Destroy(controller.GetComponent<PlayerController>());
	}
}