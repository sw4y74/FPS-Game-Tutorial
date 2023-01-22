using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragGrenade : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Boom!!");
        Destroy(gameObject);
    }

    void Update()
    {
        
    }
}
