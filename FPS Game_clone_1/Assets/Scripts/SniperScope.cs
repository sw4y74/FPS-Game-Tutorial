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
        if (GetComponent<SingleShotGun>().currentlyEquipped)
        {
            crosshair.SetActive(false);
        }
        else crosshair.SetActive(true);

        if (GetComponent<SingleShotGun>().currentlyEquipped && GetComponent<SingleShotGun>().allowFire && !GetComponent<SingleShotGun>().reloading && Input.GetMouseButtonDown(1))
        {
            ToggleScope(!scopeOn);
        }
    }

    public void ToggleScope(bool toggle)
    {
        PlayerController root = transform.root.gameObject.GetComponent<PlayerController>();

        float sensitivity = toggle ? root.mouseSensitivity * 10 * 0.25f : originalSensitivity;

        root.ChangeSensitivity(sensitivity);

        transform.root.gameObject.GetComponent<PlayerController>().ToggleWeaponRender(!toggle);
        playerCam.GetComponent<Camera>().fieldOfView = !toggle ? 75 : 20;
        sniperScope.SetActive(toggle);
        scopeOn = !scopeOn;
    }
}
