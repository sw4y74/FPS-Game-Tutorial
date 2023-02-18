using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameModeManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static GameModeManager Instance { get; private set; }
    GameObject localPlayerManager;
    [Header("Required References")]
    [SerializeField] GameModeUI gameModeUI;
    [SerializeField] int FFAKillLimit = 15;
    [SerializeField] bool FFAKillLimitEnabled = false;
    [Range(0, 20)]
    [SerializeField] double FFATimerMinutes = 4;
    [SerializeField] bool DEV_FFATimerEnabled = true;
    double FFATimer() { return DEV_FFATimerEnabled ? FFATimerMinutes * 60 : 200000; } // Multiply the minutes by 60 to get seconds
    public static readonly byte TeamElim_GameOverEventCode = 2;
    public static readonly byte FFA_GameOverEventCode = 1;
    [SerializeField] GameModeBase FFA;
    [SerializeField] GameModeBase TeamElim;

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }

    #region FFA Methods --------------------------------------------------

    public void FFA_Initialize(GameObject _localPlayerManager) {
        localPlayerManager = _localPlayerManager;
        FFA.Init(_localPlayerManager);
        StartCoroutine(FFARoutine());
    }

    IEnumerator FFARoutine() {
		Debug.Log("Starting FFA Gamemode");
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
		yield return new WaitForSeconds(3f);
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(false);
		gameModeUI.FFA_StartTimer(FFATimer());
		gameModeUI.OnTimerCompleted += HandleTimerCompleted;

		yield return null;
	}

    void FFA_GameOverRaiseEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        (int, string) winner = GetWinner();
        int winnerScore = winner.Item1;
        string winnerName = winner.Item2;
       
        object[] data = new object[] { winnerScore, winnerName };
        PhotonNetwork.RaiseEvent(FFA_GameOverEventCode, data, raiseEventOptions, sendOptions);
    }

    private (int, string) GetWinner()
    {
        int bestScore = -1;
        string bestPlayer = "";
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetScore() > bestScore)
            {
                bestScore = player.GetScore();
                bestPlayer = player.NickName;
            } else if (player.GetScore() == bestScore) {
                bestPlayer += " and " + player.NickName;
            }
        }
        return (bestScore, bestPlayer);
    }

    void FFA_GameOverEventHandler(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        int bestScore = (int)data[0];
        string bestPlayer = (string)data[1];
        Debug.Log("FFA_GameOverEventHandler");
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
		gameModeUI?.StartCoroutine(gameModeUI.SetFFAGameOverTextRoutine("Game Over! " + bestPlayer + " won with " + bestScore + " kills!"));
		StartCoroutine(LeaveRoomRoutine());
    }

    void HandleTimerCompleted() {
        RoomManager roomManager = RoomManager.Instance;
        if (roomManager.gameModes[roomManager.selectedGameMode].type != GameModeType.FFA && !PhotonNetwork.IsMasterClient)
            return;
        FFA_GameOverRaiseEvent();
	}

    public void HandlePlayerKill(int killScore, int photonID) {
		RoomManager roomManager = RoomManager.Instance;
        if (roomManager.gameModes[roomManager.selectedGameMode].type != GameModeType.FFA)
            return;
			CheckHighestScore(killScore, photonID);
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

		if (FFAKillLimitEnabled && killLimitReached) FFA_GameOverRaiseEvent();
    }

    #endregion

    #region Team Elimination Methods ------------------------------------

    public void TeamElim_Initialize(GameObject _localPlayerManager) {
        localPlayerManager = _localPlayerManager;
        StartCoroutine(TeamElimRoutine());
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

    #endregion

    IEnumerator LeaveRoomRoutine() {
		yield return new WaitForSeconds(5f);
        Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		FindObjectOfType<PauseMenu>().LoadMenu();
	}

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == FFA_GameOverEventCode) FFA_GameOverEventHandler(photonEvent);
		if (photonEvent.Code == TeamElim_GameOverEventCode) TeamElim_GameOverEventHandler(photonEvent);
    }
}
