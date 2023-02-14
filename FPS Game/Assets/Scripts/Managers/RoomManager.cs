using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
	public static RoomManager Instance;
	GameObject localPlayerManager;
	public float sensitivity;
	int selectedGameMode { get; set; }

	void Awake()
	{
		if(Instance)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		Instance = this;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if(scene.buildIndex != 0) // We're in the game scene
		{
			localPlayerManager = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
			StartCoroutine(InitializeGameRoutine());
		}
	}

	IEnumerator InitializeGameRoutine() {
		selectedGameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["gamemode"];
		yield return new WaitUntil(() => localPlayerManager.GetComponent<PlayerManager>().HasController());
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
		yield return new WaitForSeconds(3f);
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(false);
		yield return null;
	}
}
