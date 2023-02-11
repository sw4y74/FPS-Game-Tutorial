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
        StartCoroutine(ToggleScopeRoutine(toggle ? 20 : 75));
        // playerCam.GetComponent<Camera>().fieldOfView = !toggle ? 75 : 20;
        sniperScope.SetActive(toggle);
        scopeOn = !scopeOn;
    }

    IEnumerator ToggleScopeRoutine (float target)
    {
        float timeToStart = Time.time;
        while(playerCam.GetComponent<Camera>().fieldOfView > target)
        {
            // transform.position = Vector3.Lerp(transform.position, target.position, (Time.time - timeToStart )* Speed ); //Here speed is the 1 or any number w$$anonymous$$ch decides the how fast it reach to one to other end.
            playerCam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(playerCam.GetComponent<Camera>().fieldOfView, 20, (Time.time - timeToStart ) * 5);
            yield return null;
        }
        yield return null;
    }
}
