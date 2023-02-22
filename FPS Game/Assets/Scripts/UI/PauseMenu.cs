using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

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

    Gun[] guns;

    [System.NonSerialized] public int primaryWeapon = 5;
    [System.NonSerialized] public int secondaryWeapon = 0;

    [SerializeField] GameObject mapChoice;
    [SerializeField] TMP_Text mapLabel;
	[SerializeField] Button nextMap;
	[SerializeField] Button previousMap;
	[SerializeField] List<Maps> maps = new List<Maps>();
	private int selectedMap;

    private void Start() {
        mapChoice.SetActive(PhotonNetwork.IsMasterClient);
        selectedMap = 0;
		UpdateMapLabel();
    }

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

        foreach(Gun gun in guns)
        {
            GameObject item;
            if (gun.gun.weaponSlot == WeaponSlotType.primary)
                item = Instantiate(panelItem, primaryWeaponsParent);
            else if (gun.gun.weaponSlot == WeaponSlotType.secondary)
                item = Instantiate(panelItem, secondaryWeaponsParent);
            else item = null;
            if (item == null) continue;
            item.GetComponent<LoadoutItem>().gun = gun;
            item.GetComponentInChildren<TextMeshProUGUI>().text = gun.gun.name;
            panelItems.Add(item);
        }
    }

    public void ChangeWeapon(int weaponIndex)
    {
        if (guns[weaponIndex].gun.weaponSlot == WeaponSlotType.primary) primaryWeapon = weaponIndex;
        else secondaryWeapon = weaponIndex;
    }

    public void MapLeft() 
    {
        selectedMap--;
        if(selectedMap < 0) {
            selectedMap = 0;
        }
        UpdateMapLabel();
    }

    public void MapRight() 
    {
        selectedMap++;
        if (selectedMap > maps.Count - 1) {
            selectedMap = maps.Count - 1;
        }
        UpdateMapLabel();
    }

    public void UpdateMapLabel()
    {
        mapLabel.text = maps[selectedMap].name;
    }

    public void SwitchMap() {
		PhotonNetwork.LoadLevel(maps[selectedMap].id);
    }   
}
