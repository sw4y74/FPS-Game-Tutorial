using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperScope : MonoBehaviour
{
    [SerializeField] GameObject sniperScope;
    [SerializeField] GameObject playerCam;
    [SerializeField] GameObject crosshair;

    bool scopeOn = false;

    void Start()
    {
        playerCam = transform.root.gameObject.GetComponent<PlayerController>().firstPersonCamera.gameObject;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ToggleScope(scopeOn);
        }
    }

    void ToggleScope(bool toggle)
    {
        crosshair.SetActive(toggle);
        transform.root.gameObject.GetComponent<PlayerController>().ToggleWeaponRender(toggle);
        playerCam.GetComponent<Camera>().fieldOfView = toggle ? 75 : 20;
        sniperScope.SetActive(!toggle);
        scopeOn = !scopeOn;
    }
}
