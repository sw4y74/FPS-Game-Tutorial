using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class FreeForAll : GameModeBase, IOnEventCallback
{
    [SerializeField] int FFAKillLimit = 15;
    [SerializeField] bool FFAKillLimitEnabled = false;
    double FFATimerSeconds = 4;
    [SerializeField] bool DEV_FFATimerEnabled = true;
    [SerializeField] int freezeTime = 3;
    [SerializeField] float leaveRoomDelay = 10f;

    double FFATimer() { return DEV_FFATimerEnabled ? FFATimerSeconds : 200000; }
    public readonly byte GameOverEventCode = 1;

    public override void Init(GameObject localPlayer)
    {
        base.Init(localPlayer);
        FFATimerSeconds = RoomManager.Instance.roundTimes[RoomManager.Instance.selectedRoundTime].timeSeconds;
        FFAKillLimit = RoomManager.Instance.killTargets[RoomManager.Instance.selectedKillTarget].killTarget;
        StartCoroutine(GameModeRoutine());
    }

    IEnumerator GameModeRoutine()
    {
        yield return new WaitUntil(() => localPlayerManager.GetComponent<PlayerManager>().HasController());
        localPlayerManager.GetComponent<PlayerManager>().OnPlayerKill += HandlePlayerKill;
        localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
        gameModeUI.FFA_StartTimer(freezeTime);
		yield return new WaitForSeconds(freezeTime);
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(false);
        Debug.Log("Starting FFA Gamemode");
        gameModeUI.RestartTimer();
        gameModeUI.FFA_StartTimer(FFATimer());
		gameModeUI.OnTimerCompleted += HandleTimerCompleted;

        yield return null;
    } 

    void GameOverRaiseEvent(bool byKillLimit, int photonID)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        (int, string) winner = byKillLimit ? GetWinnerKillLimit(photonID) : GetWinnerTimeLimit();
        int winnerScore = winner.Item1;
        string winnerName = winner.Item2;
       
        object[] data = new object[] { winnerScore, winnerName };
        PhotonNetwork.RaiseEvent(GameOverEventCode, data, raiseEventOptions, sendOptions);
    }

    private (int, string) GetWinnerTimeLimit()
    {
        int bestScore = 0;
        string bestPlayer = "";
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetScore() == 0) {
                continue;
            } 
            else if (player.GetScore() > bestScore)
            {
                bestScore = player.GetScore();
                bestPlayer = player.NickName + " wins!";
            }
            else if (player.GetScore() == bestScore) {
                bestPlayer += "'" + bestPlayer + "' and '" + player.NickName;
            }
        }
        return (bestScore, bestPlayer);
    }

    private (int, string) GetWinnerKillLimit(int photonID)
    {
        int bestScore = 0;
        string bestPlayer = "";
        
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetScore() > bestScore && player.GetScore() > 0)
            {
                bestScore = player.GetScore();
                bestPlayer = player.NickName + " wins!";
            } else if (player.GetScore() == bestScore) {
                bestPlayer += "'" + bestPlayer + "' and '" + player.NickName;
            }
        }

        PhotonView killerView = PhotonNetwork.GetPhotonView(photonID);
        int killerScore = killerView.Owner.GetScore() + 1;
        if (killerScore > bestScore) { //if killer has the highest score
            bestScore = killerScore;
            bestPlayer = killerView.Owner.NickName;
        }

        return (bestScore, bestPlayer);
    }

    void GameOverEventHandler(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        int bestScore = (int)data[0];
        string bestPlayer = (string)data[1];
        Debug.Log("FFA_GameOverEventHandler");
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
		gameModeUI?.StartCoroutine(gameModeUI.SetFFAGameOverTextRoutine(bestPlayer, leaveRoomDelay));
		StartCoroutine(LeaveRoomRoutine());
    }

    void HandleTimerCompleted() {
        RoomManager roomManager = RoomManager.Instance;
        if (GameModeManager.Instance.currentGameModeType != GameModeType.FFA && !PhotonNetwork.IsMasterClient)
            return;
        GameOverRaiseEvent(false, -1);
	}

    public override void HandlePlayerKill(int photonID) {
		RoomManager roomManager = RoomManager.Instance;
        if (GameModeManager.Instance.currentGameModeType != GameModeType.FFA)
            return;
		CheckHighestScore(photonID);
	}

    public void CheckHighestScore(int photonID)
    {
		string bestPlayer = "";
		PhotonView killerView = PhotonNetwork.GetPhotonView(photonID);
		int killerScore = killerView.Owner.GetScore() + 1;
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

		if (FFAKillLimitEnabled && killLimitReached) GameOverRaiseEvent(true, photonID);
    }

    IEnumerator LeaveRoomRoutine() {
        localPlayerManager.GetComponent<PlayerManager>().OnPlayerKill -= HandlePlayerKill;
		yield return new WaitForSeconds(leaveRoomDelay);
        Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		FindObjectOfType<PauseMenu>().LoadMenu();
	}

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == GameOverEventCode) GameOverEventHandler(photonEvent);
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {

        PhotonNetwork.RemoveCallbackTarget(this);
    }
}