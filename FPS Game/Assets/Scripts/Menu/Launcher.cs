using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class Maps
{
	public int id;
	public string name;
}

public class Launcher : MonoBehaviourPunCallbacks
{
	public static Launcher Instance;

	[SerializeField] TMP_InputField roomNameInputField;
	[SerializeField] TMP_Text errorText;
	[SerializeField] TMP_Text roomNameText;
	[SerializeField] TMP_Text mapLabel;
	[SerializeField] TMP_Text numberOfPlayersText;
	[SerializeField] Transform roomListContent;
	[SerializeField] GameObject roomListItemPrefab;
	[SerializeField] Transform playerListContent;
	[SerializeField] GameObject PlayerListItemPrefab;
	[SerializeField] GameObject startGameButton;
	[SerializeField] GameObject mapChoice;
	[SerializeField] Button createRoomButton;
	[SerializeField] Button findRoomButton;
	[SerializeField] Button nextMap;
	[SerializeField] Button previousMap;
	[SerializeField] List<Maps> maps = new List<Maps>();
	[SerializeField] int numberOfPlayers = 4;
	[SerializeField] int maxNoOfPlayers = 12;
	
	private int selectedMap;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		Debug.Log("Connecting to Master");
		PhotonNetwork.ConnectUsingSettings();
		selectedMap = 0;
		UpdateMapLabel();
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
        ToggleNavElements(false);
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
	{
		ToggleNavElements(false);
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
		mapChoice.SetActive(PhotonNetwork.IsMasterClient);
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
		Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
	}

	public void MapLeft() 
    {
        selectedMap--;
        if(selectedMap < 0) {
            selectedMap = 0;
        }
        UpdateMapLabel();
    }

    public void MapRight() 
    {
        selectedMap++;
        if (selectedMap > maps.Count - 1) {
            selectedMap = maps.Count - 1;
        }
        UpdateMapLabel();
    }

    public void UpdateMapLabel()
    {
        mapLabel.text = maps[selectedMap].name;
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
}