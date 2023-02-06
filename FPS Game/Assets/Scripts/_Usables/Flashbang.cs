using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashbang : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Flash!");
        Destroy(gameObject);
    }

    void Update()
    {
        
    }
}
