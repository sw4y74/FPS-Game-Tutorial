using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameModeUI))]
public abstract class GameModeBase : MonoBehaviour
{
    protected GameObject localPlayerManager;
    public GameModeType gameModeType;
    [SerializeField] protected GameModeUI gameModeUI;

    private void Awake() {
        gameModeUI = GetComponent<GameModeUI>();
    }

    public virtual void Init(GameObject localPlayer) {
        localPlayerManager = localPlayer;
        StartCoroutine(GameModeRoutine());
    }

    protected virtual IEnumerator GameModeRoutine()
    {
        Debug.Log("Starting Gamemode");
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(true);
		yield return new WaitForSeconds(3f);
		localPlayerManager.GetComponent<PlayerManager>().FreezeTime(false);
    }
}
