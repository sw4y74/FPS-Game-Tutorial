using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
	PhotonView PV;

	GameObject PlayerGameObject;
	public bool freezeTime = false;

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
		PlayerGameObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
		if (freezeTime) FreezeTime(true);
		PV.RPC("RPC_CreateController", RpcTarget.OthersBuffered, PlayerGameObject.GetPhotonView().ViewID);
	}

	[PunRPC]
	void RPC_CreateController(int viewID)
	{
		PlayerGameObject = PhotonView.Find(viewID).gameObject;
		Debug.Log(PlayerGameObject.name);
	}

	IEnumerator DieRoutine(string killer)
    {
		float timeout = 3f;
		//PV.RPC("RPC_DestroyViewModel", RpcTarget.Others);
		Transform cameraTransform = PlayerGameObject.GetComponent<PlayerController>().firstPersonCamera.transform;
		PV.RPC("RPC_DestroyViewModel", RpcTarget.All);
		PlayerGameObject.GetComponent<PlayerController>().enabled = false;
		PlayerGameObject.GetComponent<PlayerMovement>().enabled = false;
		PlayerGameObject.GetComponent<WallRun>().enabled = false;
		PlayerGameObject.GetComponent<Throwable>().enabled = false;
		PlayerGameObject.GetComponent<Footsteps>().enabled = false;

		GameObject.FindGameObjectWithTag("DeathCam").GetComponent<AudioListener>().enabled = true;
		GameObject.FindGameObjectWithTag("DeathCam").GetComponent<AudioLowPassFilter>().enabled = true;
		GameObject.FindGameObjectWithTag("DeathCam").transform.position = cameraTransform.position;
		GameObject.FindGameObjectWithTag("DeathCam").transform.rotation = cameraTransform.rotation;

		deathCam.DisplayDeathInfo(timeout, killer);

		yield return new WaitForSeconds(timeout);
		PhotonNetwork.Destroy(PlayerGameObject);

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

	[PunRPC]
	void RPC_DestroyViewModel()
	{
		Destroy(PlayerGameObject.transform.GetChild(0).gameObject);
	}

	public void FreezeTime(bool toggle) {
		freezeTime = toggle;
		PlayerGameObject.GetComponent<PlayerController>().freezeTime = toggle;
		PlayerGameObject.GetComponent<PlayerMovement>().enabled = !toggle;
	}

	public bool HasController() {
		return PlayerGameObject != null;
	}
}