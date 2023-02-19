using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(GameModeUI))]
public abstract class GameModeBase : MonoBehaviourPunCallbacks
{
    protected GameObject localPlayerManager;
    public GameModeType gameModeType;
    [SerializeField] protected GameModeUI gameModeUI;

    private void Awake() {
        gameModeUI = GetComponent<GameModeUI>();
    }

    public virtual void Init(GameObject localPlayer) {
        localPlayerManager = localPlayer;
    }

    public abstract void HandlePlayerKill(int photonID);
}
