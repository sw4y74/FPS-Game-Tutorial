using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TesterScript : MonoBehaviourPunCallbacks
{
    public bool testing = false;

    private void Awake() {
        if (testing) PhotonNetwork.OfflineMode = true;
    }
}
