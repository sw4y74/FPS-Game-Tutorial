using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class Launcher : MonoBehaviourPunCallbacks
{
	public static Launcher Instance;

	[SerializeField] TMP_InputField roomNameInputField;
	[SerializeField] TMP_Text errorText;
	[SerializeField] TMP_Text roomNameText;
	[SerializeField] TMP_Text mapLabel;
	[SerializeField] TMP_Text gameModeLabel;
	[SerializeField] TMP_Text roundTimeLabel;
	[SerializeField] TMP_Text numberOfPlayersText;
	[SerializeField] Transform roomListContent;
	[SerializeField] GameObject roomListItemPrefab;
	[SerializeField] Transform playerListContent;
	[SerializeField] GameObject PlayerListItemPrefab;
	[SerializeField] GameObject startGameButton;
	[SerializeField] GameObject[] mapSelectionButtons;
	[SerializeField] GameObject[] gameModeSelectionButtons;
	[SerializeField] GameObject[] roundTimeSelectionButtons;
	[SerializeField] Button createRoomButton;
	[SerializeField] Button findRoomButton;
	[SerializeField] Button nextMap;
	[SerializeField] Button previousMap;
	[SerializeField] int numberOfPlayers = 4;
	[SerializeField] int maxNoOfPlayers = 12;
	
	private int selectedMap;
	[System.NonSerialized] public int selectedGameMode;
	int selectedRoundTime;
    private GameObject[] gameModeControls;

    void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		Debug.Log("Connecting to Master");
		PhotonNetwork.ConnectUsingSettings();
		selectedMap = 0;
		selectedGameMode = 0;
		selectedRoundTime = 0;
		UpdateNoOfPlayersText();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master");
		PhotonNetwork.JoinLobby();
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnJoinedLobby()
	{
		MenuManager.Instance.OpenMenu("title");
		Debug.Log("Joined Lobby");
	}

	public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
		ToggleNavElements(false);
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
	{
		Debug.Log("Joined Room");
		ToggleNavElements(false);
		SetMap(selectedMap);
		SetGameMode(selectedGameMode);
		SetRoundTime(selectedRoundTime);
		MenuManager.Instance.OpenMenu("room");
		roomNameText.text = PhotonNetwork.CurrentRoom.Name;

		Player[] players = PhotonNetwork.PlayerList;

		foreach(Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}

		for(int i = 0; i < players.Count(); i++)
		{
			Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
		}

		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
		foreach (GameObject item in mapSelectionButtons)
		{
			item.SetActive(PhotonNetwork.IsMasterClient);
		}
		foreach (GameObject item in gameModeSelectionButtons)
		{
			item.SetActive(PhotonNetwork.IsMasterClient);
		}
		foreach (GameObject item in roundTimeSelectionButtons)
		{
			item.SetActive(PhotonNetwork.IsMasterClient);
		}
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
		foreach (GameObject item in mapSelectionButtons)
		{
			item.SetActive(PhotonNetwork.IsMasterClient);
		}
		foreach (GameObject item in gameModeSelectionButtons)
		{
			item.SetActive(PhotonNetwork.IsMasterClient);
		}
		foreach (GameObject item in roundTimeSelectionButtons)
		{
			item.SetActive(PhotonNetwork.IsMasterClient);
		}
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		errorText.text = "Room Creation Failed: " + message;
		Debug.LogError("Room Creation Failed: " + message);
		MenuManager.Instance.OpenMenu("error");
	}

	public void StartGame()
	{
		PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.CurrentRoom.IsVisible = false;
		PhotonNetwork.LoadLevel(RoomManager.Instance.maps[selectedMap].id);
	}

	public void LeaveRoom()
	{
		ToggleNavElements(true);
		PhotonNetwork.LeaveRoom();
		MenuManager.Instance.OpenMenu("loading");
	}

	public void JoinRoom(RoomInfo info)
	{
		PhotonNetwork.JoinRoom(info.Name);
		MenuManager.Instance.OpenMenu("loading");
	}

	public override void OnLeftRoom()
	{
		MenuManager.Instance.OpenMenu("title");
	}

	public IEnumerator DisconnectAndLoad()
    {
		PhotonNetwork.Disconnect();

		while (PhotonNetwork.IsConnected)
			yield return null;
		SceneManager.LoadScene("_Menu");
    }

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach(Transform trans in roomListContent)
		{
			Destroy(trans.gameObject);
		}

		for(int i = 0; i < roomList.Count; i++)
		{
			if(roomList[i].RemovedFromList)
				continue;
			Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		PlayerListItem newPlayerItem = Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>();
		newPlayerItem.SetUp(newPlayer);
		newPlayerItem.ChangeTeam(0);
		SetMap(selectedMap);
		SetGameMode(selectedGameMode);
		SetRoundTime(selectedRoundTime);
	}

	public void MapLeft() 
    {
        selectedMap--;
        if(selectedMap < 0) {
            selectedMap = 0;
        }
		SetMap(selectedMap);
    }

    public void MapRight() 
    {
        selectedMap++;
        if (selectedMap > RoomManager.Instance.maps.Count - 1) {
            selectedMap = RoomManager.Instance.maps.Count - 1;
        }
		SetMap(selectedMap);
    }

	public void GameModeLeft() 
    {
        selectedGameMode--;
        if(selectedGameMode < 0) {
            selectedGameMode = 0;
        }
		SetGameMode(selectedGameMode);
    }

    public void GameModeRight() 
    {
        selectedGameMode++;
        if (selectedGameMode > RoomManager.Instance.gameModes.Count - 1) {
            selectedGameMode = RoomManager.Instance.gameModes.Count - 1;
        }
		SetGameMode(selectedGameMode);
    }

	public void RoundTimeLeft() 
    {
        selectedRoundTime--;
        if(selectedRoundTime < 0) {
            selectedRoundTime = 0;
        }
		SetRoundTime(selectedRoundTime);
    }

    public void RoundTimeRight() 
    {
        selectedRoundTime++;
        if (selectedRoundTime > RoomManager.Instance.roundTimes.Count - 1) {
            selectedRoundTime = RoomManager.Instance.roundTimes.Count - 1;
        }
		SetRoundTime(selectedRoundTime);
    }

    public void UpdateMapLabel()
    {
        mapLabel.text = RoomManager.Instance.maps[selectedMap].name;
    }

    private void UpdateGameModeUI()
    {
		foreach(Transform child in playerListContent)
		{
			child.GetComponent<PlayerListItem>().SetupGamemode(RoomManager.Instance.gameModes[selectedGameMode]);
		}
		gameModeLabel.text = RoomManager.Instance.gameModes[selectedGameMode].name;
    }

	private void UpdateGameModeControls()
    {
		foreach (GameMode item in RoomManager.Instance.gameModes)
		{
			if (item.gameModeControlUI != null)
				item.gameModeControlUI.SetActive(false);
		}
		if (RoomManager.Instance.gameModes[selectedGameMode].gameModeControlUI != null)
			RoomManager.Instance.gameModes[selectedGameMode].gameModeControlUI.SetActive(true);
    }

	public void UpdateRoundTimeLabel()
    {
        roundTimeLabel.text = RoomManager.Instance.roundTimes[selectedRoundTime].timeMinutes.ToString() + "m";
    }

	private void ToggleNavElements(bool toggle)
    {
        findRoomButton.interactable = toggle;
        createRoomButton.interactable = toggle;
    }

	public void ChangeNumberOfPlayers(int change)
	{
		numberOfPlayers += change;
		if (numberOfPlayers < 2)
		{
			numberOfPlayers = 2;
		}
		if (numberOfPlayers > maxNoOfPlayers)
		{
			numberOfPlayers = maxNoOfPlayers;
		}
		UpdateNoOfPlayersText();
	}

	private void UpdateNoOfPlayersText()
	{
		numberOfPlayersText.text = numberOfPlayers.ToString();
	}

	private void OnApplicationQuit() {
		PlayerPrefs.Save();
	}

	public void SetPlayerTeam(int team, Player player)
	{
		player.SetTeam(team);
	}

	public void SetGameMode(int mode)
	{
		if (PhotonNetwork.IsMasterClient) {
			PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "gamemode", mode } });
		}
	}

	public void SetRoundTime(int roundTime)
	{
		if (PhotonNetwork.IsMasterClient) {
			PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "roundtime", roundTime } });
			RoomManager.Instance.selectedRoundTime = roundTime;
		}
	}
	
	public void SetMap(int map)
	{
		if (PhotonNetwork.IsMasterClient) {
			PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "map", map } });
		}
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
		// Debug.Log("OnPlayerPropertiesUpdate: " + targetPlayer.NickName + " changedProps: " + changedProps.ToStringFull());
		if (changedProps.ContainsKey("map")) {
			int map = (int)changedProps["map"];
			selectedMap = map;
			RoomManager.Instance.selectedMap = map;
			UpdateMapLabel();
		}
		if (changedProps.ContainsKey("gamemode")) {
			int gameMode = (int)changedProps["gamemode"];
			selectedGameMode = gameMode;
			RoomManager.Instance.selectedGameMode = gameMode;
			UpdateGameModeUI();
			UpdateGameModeControls();
		}
		if (changedProps.ContainsKey("roundtime")) {
			int roundTime = (int)changedProps["roundtime"];
			selectedRoundTime = roundTime;
			RoomManager.Instance.selectedRoundTime = roundTime;
			UpdateRoundTimeLabel();
		}
    }
}

[System.Serializable]
public class Maps
{
	public int id;
	public string name;
	public Sprite sprite;
}

[System.Serializable]
public class GameMode {
	public int id;
	public string name;
	public GameModeType type;
	public GameObject gameModeControlUI;
	public GameMode(int id, string name, GameModeType type, GameObject gameModeControlUI) {
		this.id = id;
		this.name = name;
		this.type = type;
		this.gameModeControlUI = gameModeControlUI;
	}
}

[System.Serializable]
public class RoundTime {
	[Range(0.1f, 60)]
	public double timeMinutes;
	public RoundTime(double timeMinutes) {
		this.timeMinutes = timeMinutes;
	}
}
