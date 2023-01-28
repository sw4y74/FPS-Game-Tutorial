using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameManager : MonoBehaviour
{
	[SerializeField] TMP_InputField usernameInput;
	[SerializeField] TMP_Text usernameText;

	void Start()
	{
		if(PlayerPrefs.HasKey("username"))
		{
			usernameText.text = PlayerPrefs.GetString("username");
			usernameInput.text = PlayerPrefs.GetString("username");
			PhotonNetwork.NickName = PlayerPrefs.GetString("username");
			Debug.Log("Username loaded: " + PlayerPrefs.GetString("username"));
		}
		else
		{
			MenuManager.Instance.OpenMenu("Username");
			Debug.Log("No username found, opening username menu");
			// usernameInput.text = "Player " + Random.Range(0, 10000).ToString("0000");
			// OnUsernameInputValueChanged();
		}
	}

	public void OnUsernameInputValueChanged()
	{
		Debug.Log("Username changed to: " + usernameInput.text);
		usernameText.text = usernameInput.text;
		PhotonNetwork.NickName = usernameInput.text;
		PlayerPrefs.SetString("username", usernameInput.text);
	}
}
