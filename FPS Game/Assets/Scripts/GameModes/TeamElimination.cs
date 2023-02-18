using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamElimination : GameModeBase
{
    [SerializeField] int TeamElimRoundLimit = 7;
    public readonly byte GameOverEventCode = 2;

    public override void Init(GameObject localPlayer)
    {
        base.Init(localPlayer);
    }

    protected override IEnumerator GameModeRoutine()
    {
        base.GameModeRoutine();
        yield return null;
    } 
}
