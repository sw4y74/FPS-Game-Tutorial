using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class RoomManager : MonoBehaviourPunCallbacks
{
	public static RoomManager Instance;
	public List<GameMode> gameModes = new List<GameMode>();
	GameModeUI gameModeUI;
	public List<Maps> maps = new List<Maps>();
	public List<RoundTime> roundTimes = new List<RoundTime>();
	GameObject localPlayerManager;
	public float sensitivity;
	public int selectedGameMode { get; set; }
	public int selectedMap { get; set; }
	public int selectedRoundTime { get; set; }

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
	
	IEnumerator InitializeGameRoutine() {
		SpawnManager.Instance.SetSpawnpoints(gameModes[selectedGameMode].type); //set spawnpoints based on gamemode
		localPlayerManager = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity); //spawn player manager -> then PM spawns player
		yield return new WaitUntil(() => localPlayerManager.GetComponent<PlayerManager>().HasController());
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
		yield return new WaitUntil(() => !WaitingForPlayers());
		// HERE - might change to a RaiseEvent from master to initialize instead of letting every client check if players are loaded
		GameModeManager.Instance.InitializeGameMode(gameModes[selectedGameMode].type, localPlayerManager);
		yield return null;
	}

	bool WaitingForPlayers() {
		bool waitingForPlayers = PhotonNetwork.PlayerList.ToList().Any(player => !player.IsLoaded());
		return waitingForPlayers;
	}
}
