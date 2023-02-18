using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
	[SerializeField] TMP_Text text;
	Player player;
	[SerializeField] Button changeTeamLeft;
	[SerializeField] Button changeTeamRight;
	public int teamId;

	private void Awake() {
		changeTeamLeft.gameObject.SetActive(false);
		changeTeamRight.gameObject.SetActive(false);
	}

	public void SetUp(Player _player)
	{
		player = _player;
		text.text = _player.NickName;
		// if (PhotonNetwork.LocalPlayer == player) ChangeTeam(0); // when localPlayer joins, set team to 0 (blue)
		// else 
		if (PhotonNetwork.LocalPlayer != player) teamId = player.GetTeam(); // set teams for other players that are already in the room
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if(player == otherPlayer)
		{
			Destroy(gameObject);
		}
	}

	public override void OnLeftRoom()
	{
		Destroy(gameObject);
	}

	public void SetupGamemode(GameMode gamemode)
	{
		switch (gamemode.type)
		{
			case GameModeType.TeamElimination:
				ToggleChangeTeamButtons(teamId == 1 ? true : false, teamId == 0 ? true : false);
				text.color = teamId == 1 ? Color.yellow : Color.cyan;
				break;
			case GameModeType.FFA:
				ToggleChangeTeamButtons(false, false);
				text.color = Color.white;
				break;
			default:
				break;
		}
	}

	void ToggleChangeTeamButtons(bool toggleLeft, bool toggleRight)
	{
		if (PhotonNetwork.IsMasterClient) {
			changeTeamLeft.gameObject.SetActive(toggleLeft);
			changeTeamRight.gameObject.SetActive(toggleRight);
		}
	}

	public void ChangeTeam(int team)
	{
		player.SetTeam(team);
		// Launcher.Instance.SetPlayerTeam(team, player);
	}

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
		if (changedProps.ContainsKey("teamIdx")) {
			if (targetPlayer == player && RoomManager.Instance.gameModes[Launcher.Instance.selectedGameMode].type == GameModeType.TeamElimination) {
				Debug.Log("OnPlayerPropertiesUpdate: " + targetPlayer.NickName + " changedProps: " + changedProps.ToStringFull());
				int team = (int)changedProps["teamIdx"];
				teamId = team;
				text.color = team == 1 ? Color.yellow : Color.cyan;
				ToggleChangeTeamButtons(team == 1 ? true : false, team == 0 ? true : false);
			}
		}
    }
}