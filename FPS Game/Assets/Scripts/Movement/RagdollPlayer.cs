using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class RagdollPlayer : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}
