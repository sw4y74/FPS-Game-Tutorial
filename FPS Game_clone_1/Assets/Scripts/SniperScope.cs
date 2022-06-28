using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperScope : MonoBehaviour
{
    [SerializeField] GameObject sniperScope;
    [SerializeField] GameObject playerCam;
    [SerializeField] GameObject crosshair;
    float originalSensitivity;

    public bool scopeOn = false;

    void Start()
    {
        PlayerController root = transform.root.gameObject.GetComponent<PlayerController>();
        originalSensitivity = root.mouseSensitivity * 10;
    }

    void Update()
    {
        if (GetComponent<SingleShotGun>().currentlyEquipped && GetComponent<SingleShotGun>().allowFire && Input.GetMouseButtonDown(1))
        {
            ToggleScope(!scopeOn);
        }
    }

    public void ToggleScope(bool toggle)
    {
        PlayerController root = transform.root.gameObject.GetComponent<PlayerController>();

        Debug.Log(toggle);
        float sensitivity = toggle ? root.mouseSensitivity * 10 / 4 : originalSensitivity;
        Debug.Log(sensitivity);

        root.ChangeSensitivity(sensitivity);

        crosshair.SetActive(!toggle);
        transform.root.gameObject.GetComponent<PlayerController>().ToggleWeaponRender(!toggle);
        playerCam.GetComponent<Camera>().fieldOfView = !toggle ? 75 : 20;
        sniperScope.SetActive(toggle);
        scopeOn = !scopeOn;
    }
}
