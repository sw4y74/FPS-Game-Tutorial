using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class TeamElimination : GameModeBase
{
    [SerializeField] int TeamElimRoundLimit = 7;
    public readonly byte GameOverEventCode = 2;

    public override void Init(GameObject localPlayer)
    {
        base.Init(localPlayer);
        StartCoroutine(GameModeRoutine());
    }

    IEnumerator GameModeRoutine()
    {
        Debug.LogError("TeamElimination not implemented");
        yield return null;
    }

    public override void HandlePlayerKill(int photonID)
    {
        Debug.LogError("HandlePlayerKill not implemented");
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == GameOverEventCode) Debug.LogError("To Implement: Handle GameOverEventCode");
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
