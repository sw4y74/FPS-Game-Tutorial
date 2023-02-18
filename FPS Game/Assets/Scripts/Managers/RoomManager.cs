using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
	public static RoomManager Instance;
	public List<GameMode> gameModes = new List<GameMode>();
	GameModeUI gameModeUI;
	public List<Maps> maps = new List<Maps>();
	GameObject localPlayerManager;
	public float sensitivity;
	public int selectedGameMode { get; set; }
	public int selectedMap { get; set; }

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
		PhotonNetwork.AddCallbackTarget(this);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		SceneManager.sceneLoaded -= OnSceneLoaded;
		PhotonNetwork.RemoveCallbackTarget(this);
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if(scene.buildIndex != 0) // We're in the game scene
		{
			StartCoroutine(InitializeGameRoutine());
		}
	}

	void ReloadScene() {
		PhotonNetwork.LoadLevel(maps[(int)PhotonNetwork.CurrentRoom.CustomProperties["map"]].id); // DOESNT WORK FOR SOME REASON
	}

	IEnumerator InitializeGameRoutine() {
		selectedGameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["gamemode"];
		SpawnManager.Instance.SetSpawnpoints(gameModes[selectedGameMode].type); //set spawnpoints based on gamemode
		localPlayerManager = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity); //spawn player manager -> then PM spawns player
		yield return new WaitUntil(() => localPlayerManager.GetComponent<PlayerManager>().HasController());
		yield return new WaitUntil(() => GameModeManager.Instance != null);
		Debug.Log("GameModeManager Initialized");
		switch (gameModes[selectedGameMode].type)
		{
			case GameModeType.TeamElimination:
				GameModeManager.Instance.TeamElim_Initialize(localPlayerManager);
				break;
			case GameModeType.FFA:
				GameModeManager.Instance.FFA_Initialize(localPlayerManager);
				break;
			default:
				break;
		}
	}
}
