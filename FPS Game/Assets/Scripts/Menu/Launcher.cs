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
	[SerializeField] TMP_Text numberOfPlayersText;
	[SerializeField] Transform roomListContent;
	[SerializeField] GameObject roomListItemPrefab;
	[SerializeField] Transform playerListContent;
	[SerializeField] GameObject PlayerListItemPrefab;
	[SerializeField] GameObject startGameButton;
	[SerializeField] GameObject[] mapSelectionButtons;
	[SerializeField] GameObject[] gameModeSelectionButtons;
	[SerializeField] Button createRoomButton;
	[SerializeField] Button findRoomButton;
	[SerializeField] Button nextMap;
	[SerializeField] Button previousMap;
	[SerializeField] List<Maps> maps = new List<Maps>();
	[SerializeField] int numberOfPlayers = 4;
	[SerializeField] int maxNoOfPlayers = 12;
	
	private int selectedMap;
	[SerializeField] public List<GameMode> gameModes = new List<GameMode>();
	[System.NonSerialized] public int selectedGameMode;

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
		Debug.Log(selectedMap);
		ToggleNavElements(false);
		SetMap(selectedMap);
		SetGameMode(selectedGameMode);
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
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		errorText.text = "Room Creation Failed: " + message;
		Debug.LogError("Room Creation Failed: " + message);
		MenuManager.Instance.OpenMenu("error");
	}

	public void StartGame()
	{
		PhotonNetwork.LoadLevel(maps[selectedMap].id);
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
        if (selectedMap > maps.Count - 1) {
            selectedMap = maps.Count - 1;
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
        if (selectedGameMode > gameModes.Count - 1) {
            selectedGameMode = gameModes.Count - 1;
        }
		SetGameMode(selectedGameMode);
    }

    public void UpdateMapLabel()
    {
        mapLabel.text = maps[selectedMap].name;
    }

    private void UpdateGameModeLabel()
    {
		gameModeLabel.text = gameModes[selectedGameMode].name;
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
	
	public void SetMap(int map)
	{
		if (PhotonNetwork.IsMasterClient) {
			PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "map", map } });
		}
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
		Debug.Log("OnPlayerPropertiesUpdate: " + targetPlayer.NickName + " changedProps: " + changedProps.ToStringFull());
		if (changedProps.ContainsKey("map")) {
			int map = (int)changedProps["map"];
			selectedMap = map;
			UpdateMapLabel();
		}
		if (changedProps.ContainsKey("gamemode")) {
			int gameMode = (int)changedProps["gamemode"];
			selectedGameMode = gameMode;
			UpdateGameModeLabel();
			UpdateGameModeUI();
		}
    }

    private void UpdateGameModeUI()
    {
        foreach(Transform child in playerListContent)
		{
			child.GetComponent<PlayerListItem>().SetupGamemode(gameModes[selectedGameMode]);
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
	public GameMode(int id, string name, GameModeType type) {
		this.id = id;
		this.name = name;
		this.type = type;
	}
}

public enum GameModeType
{
	team, freeForAll
}