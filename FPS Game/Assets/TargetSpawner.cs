using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

enum TargetType
{
    TrainingTarget
}

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] TargetType targetType;
    public int numTargets = 10;
    public float spawnRadius = 10f;
    public float spawnHeight = 5f;
    public float spawnDelay = 1f;

    private void Start() {
        if (PhotonNetwork.IsMasterClient) {
            SpawnTarget();
        }
    }

    void SpawnTarget() {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", targetType.ToString()), transform.position, Quaternion.identity);
    }
}
