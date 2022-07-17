using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public Menu optionsMenu;
    public GameObject optionsScreen;
    [SerializeField] GameObject panelItem;
    [SerializeField] Transform primaryWeaponsParent;
    [SerializeField] Transform secondaryWeaponsParent;
    bool loadoutPanelSet = false;

    SingleShotGun[] guns;

    int primaryWeapon = 1;
    int secondaryWeapon = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                if (!optionsScreen.activeSelf)
                {
                    Resume();
                }
                else
                {
                    MenuManager.Instance.OpenMenu("pause");
                    MenuManager.Instance.CloseMenu(optionsMenu);
                }
            }
            else Pause();
            
        }
    }

    public void Resume ()
    {
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
    }
    void Pause ()
    {
        if (!loadoutPanelSet)
            SetupLoadoutPanel();

        pauseMenuUI.SetActive(true);
        GameIsPaused = true;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
    }

    public void LoadMenu()
    {
        Destroy(RoomManager.Instance.gameObject);
        StartCoroutine(Launcher.Instance.DisconnectAndLoad());
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void ApplyLoadout()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().TestChangeLoadout(primaryWeapon, secondaryWeapon);
    }

    void SetupLoadoutPanel()
    {
        loadoutPanelSet = true;
        guns = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().items;

        foreach(SingleShotGun gun in guns)
        {
            GameObject item;
            if (gun.gun.primaryWeapon)
                item = Instantiate(panelItem, primaryWeaponsParent);
            else item = Instantiate(panelItem, secondaryWeaponsParent);

            item.GetComponent<LoadoutItem>().gun = gun;
            item.GetComponentInChildren<TextMeshProUGUI>().text = gun.gun.name;
        }
    }

    public void ChangeWeapon(int weaponIndex)
    {
        if (guns[weaponIndex].gun.primaryWeapon) primaryWeapon = weaponIndex;
        else secondaryWeapon = weaponIndex;
    }
}
