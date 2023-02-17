using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using UnityEngine.Events;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class RoomManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	public static RoomManager Instance;
	public List<GameMode> gameModes = new List<GameMode>();
	public List<Maps> maps = new List<Maps>();
	GameObject localPlayerManager;
	public float sensitivity;
	int selectedGameMode { get; set; }
	int selectedMap { get; set; }
    public int FFAKillLimit = 15;

    public static readonly byte TeamElim_GameOverEventCode = 2;

    public static readonly byte FFA_GameOverEventCode = 1;

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
		switch (gameModes[selectedGameMode].type)
		{
			case GameModeType.team:
				StartCoroutine(TeamElimRoutine());
				break;
			case GameModeType.freeForAll:
				StartCoroutine(FFARoutine());
				break;
			default:
				break;
		}
	}

	public void HandlePlayerKill(int killScore, int photonID) {
		if (gameModes[selectedGameMode].type == GameModeType.freeForAll)
			CheckHighestScore(killScore, photonID);
	}

	IEnumerator FFARoutine() {
		Debug.Log("Starting FFA Gamemode");
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
		yield return new WaitForSeconds(3f);
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(false);
		yield return null;
	}

    public void CheckHighestScore(int killScore, int photonID)
    {
		string bestPlayer = "";
		PhotonView killerView = PhotonNetwork.GetPhotonView(photonID);
		int killerScore = killerView.Owner.GetScore() + killScore;
        int bestScore = 0;
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetScore() > bestScore) {
				bestScore = player.GetScore();
				bestPlayer = player.NickName;
			}
        }
		if (killerScore > bestScore) { //if killer has the highest score
			bestScore = killerScore;
			bestPlayer = killerView.Owner.NickName;
		}
		bool killLimitReached = bestScore == FFAKillLimit;

		if (killLimitReached) GameOver(bestPlayer, bestScore);
		// GameOver(bestPlayer, bestScore);
    }

	void GameOver(string bestPlayer, int bestScore) {
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
		SendOptions sendOptions = new SendOptions { Reliability = true };
		object[] data = new object[] { bestPlayer, bestScore };
		PhotonNetwork.RaiseEvent(FFA_GameOverEventCode, data, raiseEventOptions, sendOptions);
	}

	void FFA_GameOverEventHandler(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        string bestPlayer = (string)data[0];
        int bestScore = (int)data[1];
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
		GameOverUI gameOverUI = FindObjectOfType<GameOverUI>();
		gameOverUI.StartCoroutine(gameOverUI.SetFFAGameOverTextRoutine("Game Over! " + bestPlayer + " won with " + bestScore + " kills!"));
		ReloadScene();
    }

    IEnumerator TeamElimRoutine() {
		Debug.Log("Starting Team Elimination Gamemode");
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
		yield return new WaitForSeconds(3f);
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(false);
		yield return null;	
	}

	void TeamElim_GameOverEventHandler(EventData photonEvent)
    {
       	Debug.LogError("not implemented yet");
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == FFA_GameOverEventCode) FFA_GameOverEventHandler(photonEvent);
		if (photonEvent.Code == TeamElim_GameOverEventCode) TeamElim_GameOverEventHandler(photonEvent);
    }
}
