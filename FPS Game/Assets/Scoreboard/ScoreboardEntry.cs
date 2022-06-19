using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardEntry : MonoBehaviour
{
	[SerializeField] private Text m_label = null;
	[SerializeField] private Image m_background = null;
	public Player Player => m_player;
	public int Score => m_player.GetScore();
	public int Deaths => m_player.GetDeaths();

	private Player m_player;

	//store player for this entry
	//set init value and color
	public void Set(Player player)
	{
		m_player = player;
		UpdateScore();
		//m_label.color = PhotonNetwork.LocalPlayer == m_player ? Color.black : Color.grey;
		m_background.color = PhotonNetwork.LocalPlayer == m_player ? Color.white : Color.HSVToRGB(0, 0, 80);
	}

	//update label bases on score and name
	public void UpdateScore()
	{
		m_label.text = $"{m_player.NickName} | {m_player.GetScore()} | {m_player.GetDeaths()}";
	}
}