using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeForAll : GameModeBase
{
    [SerializeField] int FFAKillLimit = 15;
    [SerializeField] bool FFAKillLimitEnabled = false;
    public readonly byte GameOverEventCode = 1;

    public override void Init(GameObject localPlayer)
    {
        base.Init(localPlayer);
        Debug.Log("Starting FFA Gamemode");
    }

    protected override IEnumerator GameModeRoutine()
    {
        base.GameModeRoutine();
        yield return null;
    } 
}