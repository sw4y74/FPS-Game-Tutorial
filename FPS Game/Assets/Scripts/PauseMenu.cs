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

    [SerializeField] List<GameObject> panelItems;
    [ColorUsageAttribute(true, true)]
    public Color weaponPickedColor;

    SingleShotGun[] guns;

    public int primaryWeapon = 5;
    public int secondaryWeapon = 3;

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

        //Change item tile color for which weapon is picked
        foreach (GameObject item in panelItems)
        {
            int itemIndex = item.GetComponent<LoadoutItem>().gun.index;
            if (itemIndex == primaryWeapon || itemIndex == secondaryWeapon)
            {
                item.GetComponent<Image>().color = Color.Lerp(item.GetComponent<Image>().color, weaponPickedColor, 4f * Time.deltaTime);
            } else item.GetComponent<Image>().color = Color.Lerp(item.GetComponent<Image>().color, Color.white, 4f * Time.deltaTime);
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
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().ChangeLoadoutByIndex(primaryWeapon, secondaryWeapon);
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
            panelItems.Add(item);
        }
    }

    public void ChangeWeapon(int weaponIndex)
    {
        if (guns[weaponIndex].gun.primaryWeapon) primaryWeapon = weaponIndex;
        else secondaryWeapon = weaponIndex;
    }
}
