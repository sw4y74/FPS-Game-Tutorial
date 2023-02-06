using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.IO;

public class KillFeed : MonoBehaviour
{
    public GameObject killFeedItem;
    public Transform spawn;

    public void AddKillFeedItem(string killerName, string deadName)
    {
        GameObject k = Instantiate(killFeedItem, spawn.position, spawn.rotation);

        k.transform.SetParent(spawn);        

        TextMeshProUGUI killerNameText = k.GetComponent<KillFeedItem>().killerText;
        TextMeshProUGUI targetNameText = k.GetComponent<KillFeedItem>().targetText;

        killerNameText.text = killerName;
        targetNameText.text = deadName;

        Destroy(k, 5f);
    }
        
}
