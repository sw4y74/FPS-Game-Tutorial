using Photon.Pun;
using UnityEngine;
using Utilities;

//just to showcase score changes
public class ScoreTest : MonoBehaviour
{
	public void IncreaseScore()
	{
		//PlayerExt.AddScore(PhotonNetwork.LocalPlayer, 10);
		//PhotonNetwork.LocalPlayer.AddScore(10);
	}

	public void DecreaseScore()
	{
		//PlayerExt.AddScore(PhotonNetwork.LocalPlayer, -10);
		//PhotonNetwork.LocalPlayer.AddScore(-10);
	}
}