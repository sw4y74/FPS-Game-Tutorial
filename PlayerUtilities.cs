using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;

public static class RoomProperties
{
	public static string Teams = "Teams";
}

public static class PlayerProperties
{
	public static string Team = "teamIdx";
	public static string Ready = "rdy";
	public static string Score = "scr";
}

public class PlayerUtilities : MonoBehaviour
{
	/// <summary>
	/// Player assigned Team index.
	/// </summary>
	/// <param name="player"></param>
	/// <returns>Team index if assigned,otherwise -1. </returns>
	public int GetTeam(Player player)
	{
		return GetPropertyValue(player, PlayerProperties.Team, -1);
	}

	/// <summary>
	/// Save Team index to Player properties.
	/// </summary>
	/// <param name="player">Target playedr</param>
	/// <param name="teamIdx">Team index</param>
	public void SetTeam(Player player, int teamIdx)
	{
		SetPropertyValue(player, PlayerProperties.Team, teamIdx);
	}

	/// <summary>
	/// Compares Team index from LocalPlayer and given Player.
	/// </summary>
	/// <param name="player">Target player to check if is friendly or not.</param>
	/// <returns>True if same team index otherwise false.</returns>
	public bool IsFriendly(Player player)
	{
	//var ownTeam = PhotonNetwork.LocalPlayer.GetTeam();
	//var targetTeam = player.GetTeam();
	//return ownTeam == targetTeam;
	return true;	
}

	#region PlayerReadyState

	public void SetReady(Player player, bool value)
	{
		SetPropertyValue(player, PlayerProperties.Ready, value);
	}

	public bool IsReady(Player player)
	{
		return GetPropertyValue(player, PlayerProperties.Ready, false);
	}

	#endregion

	#region Score

	public void SetScore(Player player, int amount)
	{
		SetPropertyValue(player, PlayerProperties.Score, amount);
	}

	public int GetScore(Player player)
	{
		return GetPropertyValue(player, PlayerProperties.Score, 0);
	}

	public void AddScore(Player player, int amount)
	{
		Debug.Log("Add score method");
		AddValueToProperty(player, PlayerProperties.Score, amount);
	}

	#region Player

	/// <summary>
	/// Check Player Properties for a Property Value.
	/// </summary>
	/// <param name="player">Photon Player</param>
	/// <param name="property">Property as string</param>
	/// <param name="defaultValue">Fallback Value.</param>
	/// <typeparam name="T">Type</typeparam>
	/// <returns>Property Value</returns>
	public T GetPropertyValue<T>(Player player, string property, T defaultValue)
	{
		if (player.CustomProperties.TryGetValue(property, out var value))
		{
			return (T)value;
		}

		return defaultValue;
	}

	/// <summary>
	/// Check Player Properties for a Property Value and set it.
	/// </summary>
	/// <param name="player">Photon Player</param>
	/// <param name="property">Property as string</param>
	/// <param name="value">Value to set.</param>
	/// <typeparam name="T">Type</typeparam>
	public void SetPropertyValue<T>(Player player, string property, T value)
	{
		var customProp = new Hashtable()
			{
				{property, value}
			};
		player.SetCustomProperties(customProp);
	}

	/// <summary>
	/// Check Player Properties for a Property Value, add given value to it.
	/// </summary>
	/// <param name="player">Photon Player</param>
	/// <param name="property">Property as string</param>
	/// <param name="value">Value to set.</param>
	public void AddValueToProperty(Player player, string property, int value)
	{
		var defaultValue = GetPropertyValue(player, property, 0);
		defaultValue += value;

		var scoreProp = new Hashtable()
			{
				{property, defaultValue}
			};
		player.SetCustomProperties(scoreProp);
	}

	/// <summary>
	/// Deletes Property if DeleteNullProperties is set to true in Room Options.
	/// </summary>
	/// <param name="player">Photon Player</param>
	/// <param name="property">Property as string</param>
	public void DeleteProperty(Player player, string property)
	{
		var customProp = new Hashtable()
			{
				{property, null}
			};
		player.SetCustomProperties(customProp);
	}

	#endregion

	#region Room

	/// <summary>
	/// Check Room Properties for a Property Value.
	/// </summary>
	/// <param name="room">Photon Room</param>
	/// <param name="property">Property as string</param>
	/// <param name="defaultValue">Fallback Value.</param>
	/// <typeparam name="T">Type</typeparam>
	/// <returns>Property Value</returns>
	public T GetPropertyValue<T>(Room room, string property, T defaultValue)
	{
		if (room.CustomProperties.TryGetValue(property, out var value))
		{
			return (T)value;
		}

		return defaultValue;
	}

	/// <summary>
	/// Check Room Properties for a Property Value and set it.
	/// </summary>
	/// <param name="room">Photon Room</param>
	/// <param name="property">Property as string</param>
	/// <param name="value">Value to set.</param>
	/// <typeparam name="T">Type</typeparam>
	public void SetPropertyValue<T>(Room room, string property, T value)
	{
		var customProp = new Hashtable()
			{
				{property, value}
			};
		room.SetCustomProperties(customProp);
	}

	/// <summary>
	/// Check Room Properties for a Property Value, add given value to it.
	/// </summary>
	/// <param name="room">Photon Room</param>
	/// <param name="property">Property as string</param>
	/// <param name="value">Value to set.</param>
	public void AddValueToProperty(Room room, string property, int value)
	{
		var defaultValue = GetPropertyValue(room, property, 0);
		defaultValue += value;

		var scoreProp = new Hashtable()
			{
				{property, defaultValue}
			};
		room.SetCustomProperties(scoreProp);
	}

	#endregion

	#endregion
}