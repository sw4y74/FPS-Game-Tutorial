using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	public static SpawnManager Instance;

	[SerializeField] GameObject ffaSpawnpointsGO;
	[SerializeField] GameObject teamElimSpawnpointsGO;
	Spawnpoint[] spawnpoints;
	Spawnpoint[] ffaSpawnpoints;
	Spawnpoint[] teamElimSpawnpoints;

	void Awake()
	{
		Instance = this;
		ffaSpawnpoints = ffaSpawnpointsGO.GetComponentsInChildren<Spawnpoint>();
		teamElimSpawnpoints = teamElimSpawnpointsGO.GetComponentsInChildren<Spawnpoint>();
	}

	public void SetSpawnpoints(GameModeType gameMode)
	{
		switch(gameMode)
		{
			case GameModeType.FFA:
				spawnpoints = ffaSpawnpoints;
				break;
			case GameModeType.TeamElimination:
				spawnpoints = teamElimSpawnpoints;
				break;
		}
	}

	public Transform GetSpawnpoint()
	{
		return spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
	}
}
