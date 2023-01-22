using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperScope : MonoBehaviour
{
    [SerializeField] GameObject sniperScope;
    [SerializeField] GameObject playerCam;
    [SerializeField] GameObject crosshair;

    PlayerController root;

    float originalSensitivity;

    public bool scopeOn = false;

    void Start()
    {
        root = transform.root.gameObject.GetComponent<PlayerController>();
        originalSensitivity = root.mouseSensitivity * 10;
    }

    void Update()
    {
        if (!root.pauseMenu.GameIsPaused)
        {
            if (GetComponent<Gun>().currentlyEquipped)
            {
                crosshair.SetActive(false);
            }
            else crosshair.SetActive(true);

            if (GetComponent<Gun>().currentlyEquipped && GetComponent<Gun>().allowFire && !GetComponent<Gun>().reloading && Input.GetMouseButtonDown(1))
            {
                ToggleScope(!scopeOn);
            }
        }
    }

    public void ToggleScope(bool toggle)
    {
        float sensitivity = toggle ? root.mouseSensitivity * 10 * 0.25f : originalSensitivity;

        root.aimingDownSights = toggle;

        root.ChangeSensitivity(sensitivity);

        root.ToggleWeaponRender(!toggle);
        playerCam.GetComponent<Camera>().fieldOfView = !toggle ? 75 : 20;
        sniperScope.SetActive(toggle);
        scopeOn = !scopeOn;
    }
}
