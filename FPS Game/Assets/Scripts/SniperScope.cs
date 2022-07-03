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
    }

    public void ToggleScope(bool toggle)
    {
        float sensitivity = toggle ? root.mouseSensitivity * 10 * 0.25f : originalSensitivity;

        if (!root.GetComponent<Crouch>().isCrouching)
            root.ChangePlayerSpeed(toggle ? root.walkSpeed * 0.5f : root.originalWalkSpeed * 0.9f);

        root.ChangeSensitivity(sensitivity);

        root.ToggleWeaponRender(!toggle);
        playerCam.GetComponent<Camera>().fieldOfView = !toggle ? 75 : 20;
        sniperScope.SetActive(toggle);
        scopeOn = !scopeOn;
    }
}
