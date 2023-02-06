using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.Events;
 using UnityEngine.EventSystems;

public class EnterConfirm : MonoBehaviour
{
    [SerializeField] UnityEvent myUnityEvent;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            myUnityEvent.Invoke();
        }
    }
}
